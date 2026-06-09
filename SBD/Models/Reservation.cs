using System;

namespace SBD.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }

        // Dane powiązane
        public string ClientName { get; set; }
        public string CarName { get; set; }
        public string StatusName { get; set; }

        public override string ToString()
        {
            return $"[{ReservationId}] {ClientName ?? $"Klient #{ClientId}"} | {CarName ?? $"Auto #{CarId}"} | " +
                   $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd} | {StatusName ?? $"Status #{StatusId}"}";
        }
    }
}
