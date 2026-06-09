using System;

namespace SBD.Models
{
    public class Penalty
    {
        public int PenaltyId { get; set; }
        public int ClientId { get; set; }
        public int? RentalId { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Dane powiązane
        public string ClientName { get; set; }

        public override string ToString()
        {
            return $"[{PenaltyId}] {ClientName ?? $"Klient #{ClientId}"} | {Amount:F2} PLN | {Reason}";
        }
    }
}
