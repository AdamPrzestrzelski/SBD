using System;
using SBD.Models;
using SBD.Repositories;

namespace SBD.Services
{
    /// <summary>
    /// Serwis zarządzający wypożyczeniami - logika biznesowa.
    /// </summary>
    public class RentalService
    {
        private readonly RentalRepository _rentalRepo = new RentalRepository();
        private readonly CarRepository _carRepo = new CarRepository();
        private readonly ClientRepository _clientRepo = new ClientRepository();

        /// <summary>
        /// Tworzy nowe wypożyczenie z pełną walidacją.
        /// </summary>
        public int CreateRental(int clientId, int carId, DateTime startDate, DateTime endDate)
        {
            // Walidacja klienta
            var client = _clientRepo.GetById(clientId);
            if (client == null)
                throw new Exception("Nie znaleziono klienta o podanym ID.");
            if (client.IsBlocked)
                throw new Exception($"Klient {client.FullName} jest zablokowany i nie może wypożyczyć samochodu.");

            // Walidacja samochodu
            var car = _carRepo.GetById(carId);
            if (car == null)
                throw new Exception("Nie znaleziono samochodu o podanym ID.");
            if (car.Status != "AVAILABLE")
                throw new Exception($"Samochód {car.FullName} nie jest dostępny (status: {car.Status}).");

            // Walidacja dat
            if (startDate >= endDate)
                throw new Exception("Data rozpoczęcia musi być wcześniejsza niż data zakończenia.");
            if (startDate < DateTime.Today)
                throw new Exception("Data rozpoczęcia nie może być w przeszłości.");

            // Tworzenie przez pakiet PKG_RENTAL (walidacja dostępności w triggerze)
            int rentalId = _rentalRepo.CreateRental(clientId, carId, startDate, endDate);

            Console.WriteLine($"\nWypożyczenie #{rentalId} utworzone pomyślnie!");
            Console.WriteLine($"  Klient: {client.FullName}");
            Console.WriteLine($"  Samochód: {car.FullName} ({car.PlateNumber})");
            Console.WriteLine($"  Okres: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd} ({(endDate - startDate).Days} dni)");

            if (client.PenaltyMultiplier > 1.0m)
                Console.WriteLine($"  ⚠ Uwaga: mnożnik kar klienta: {client.PenaltyMultiplier:F2}x");

            return rentalId;
        }

        /// <summary>
        /// Anuluje wypożyczenie.
        /// </summary>
        public void CancelRental(int rentalId)
        {
            var rental = _rentalRepo.GetById(rentalId);
            if (rental == null)
                throw new Exception("Nie znaleziono wypożyczenia o podanym ID.");
            if (rental.Status != "ACTIVE")
                throw new Exception($"Wypożyczenie ma status '{rental.Status}' i nie może być anulowane.");

            _rentalRepo.CancelRental(rentalId);
            Console.WriteLine($"Wypożyczenie #{rentalId} zostało anulowane.");
        }

        /// <summary>
        /// Przedłuża wypożyczenie.
        /// </summary>
        public void ExtendRental(int rentalId, DateTime newEndDate)
        {
            var rental = _rentalRepo.GetById(rentalId);
            if (rental == null)
                throw new Exception("Nie znaleziono wypożyczenia o podanym ID.");
            if (rental.Status != "ACTIVE")
                throw new Exception($"Wypożyczenie ma status '{rental.Status}' i nie może być przedłużone.");
            if (newEndDate <= rental.EndDate)
                throw new Exception("Nowa data musi być późniejsza niż obecna data zakończenia.");

            _rentalRepo.ExtendRental(rentalId, newEndDate);
            Console.WriteLine($"Wypożyczenie #{rentalId} przedłużone do {newEndDate:yyyy-MM-dd}.");
        }

        /// <summary>
        /// Kończy wypożyczenie (zwrot samochodu).
        /// </summary>
        public void CompleteRental(int rentalId)
        {
            var rental = _rentalRepo.GetById(rentalId);
            if (rental == null)
                throw new Exception("Nie znaleziono wypożyczenia o podanym ID.");
            if (rental.Status != "ACTIVE")
                throw new Exception($"Wypożyczenie ma status '{rental.Status}' i nie może być zakończone.");

            _rentalRepo.CompleteRental(rentalId);
            Console.WriteLine($"Wypożyczenie #{rentalId} zakończone. Samochód zwrócony.");

            // Informacja o ewentualnym spóźnieniu
            if (DateTime.Today > rental.EndDate)
            {
                int daysLate = (DateTime.Today - rental.EndDate).Days;
                Console.WriteLine($"  ⚠ Zwrot opóźniony o {daysLate} dni. Rozważ naliczenie kary.");
            }
        }
    }
}
