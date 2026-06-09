using System;

namespace SBD.Models
{
    public class Rental
    {
        public int RentalId { get; set; }
        public int? ReservationId { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public DateTime CreatedAt { get; set; }

        // Dane powiązane
        public string ClientName { get; set; }
        public string CarName { get; set; }
        public string PlateNumber { get; set; }

        public int PlannedDays => (EndDate - StartDate).Days;

        public override string ToString()
        {
            return $"[{RentalId}] {ClientName ?? $"Klient #{ClientId}"} | {CarName ?? $"Auto #{CarId}"} | " +
                   $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd} | {TotalPrice:F2} PLN | {Status}";
        }
    }
}
