using System;
using System.Collections.Generic;
using SBD.Models;
using SBD.Repositories;

namespace SBD.Services
{
    /// <summary>
    /// Serwis zarządzający karami klientów.
    /// </summary>
    public class PenaltyService
    {
        private readonly PenaltyRepository _penaltyRepo = new PenaltyRepository();
        private readonly ClientRepository _clientRepo = new ClientRepository();

        /// <summary>
        /// Nalicza karę klientowi.
        /// Trigger TRG_PENALTY_CHECK automatycznie sprawdzi próg kar.
        /// </summary>
        public void AddPenalty(int clientId, int? rentalId, string reason, decimal amount)
        {
            var client = _clientRepo.GetById(clientId);
            if (client == null)
                throw new Exception("Nie znaleziono klienta o podanym ID.");

            if (amount < 0)
                throw new Exception("Kwota kary nie może być ujemna.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new Exception("Powód kary jest wymagany.");

            var penalty = new Penalty
            {
                ClientId = clientId,
                RentalId = rentalId,
                Reason = reason,
                Amount = amount
            };

            _penaltyRepo.Insert(penalty);

            // Trigger TRG_PENALTY_CHECK automatycznie:
            // - przy >= 3 karach → zwiększa mnożnik ceny
            // - przy >= 5 karach → blokuje klienta

            int penaltyCount = _penaltyRepo.GetPenaltyCount(clientId);
            Console.WriteLine($"Kara {amount:F2} PLN naliczona klientowi {client.FullName}.");
            Console.WriteLine($"  Powód: {reason}");
            Console.WriteLine($"  Łączna liczba kar: {penaltyCount}");

            if (penaltyCount >= 5)
                Console.WriteLine("  ⚠ KLIENT ZOSTAŁ AUTOMATYCZNIE ZABLOKOWANY!");
            else if (penaltyCount >= 3)
                Console.WriteLine($"  ⚠ Mnożnik ceny został zwiększony.");
        }

        /// <summary>
        /// Wyświetla kary klienta.
        /// </summary>
        public List<Penalty> GetClientPenalties(int clientId)
        {
            return _penaltyRepo.GetByClientId(clientId);
        }

        /// <summary>
        /// Ręczne blokowanie klienta.
        /// </summary>
        public void BlockClient(int clientId)
        {
            var client = _clientRepo.GetById(clientId);
            if (client == null)
                throw new Exception("Nie znaleziono klienta o podanym ID.");

            if (client.IsBlocked)
            {
                Console.WriteLine($"Klient {client.FullName} jest już zablokowany.");
                return;
            }

            _clientRepo.BlockClient(clientId);
            Console.WriteLine($"Klient {client.FullName} został zablokowany.");
        }

        /// <summary>
        /// Odblokowanie klienta.
        /// </summary>
        public void UnblockClient(int clientId)
        {
            var client = _clientRepo.GetById(clientId);
            if (client == null)
                throw new Exception("Nie znaleziono klienta o podanym ID.");

            if (!client.IsBlocked)
            {
                Console.WriteLine($"Klient {client.FullName} nie jest zablokowany.");
                return;
            }

            _clientRepo.UnblockClient(clientId);
            Console.WriteLine($"Klient {client.FullName} został odblokowany. Mnożnik zresetowany do 1.00.");
        }
    }
}
