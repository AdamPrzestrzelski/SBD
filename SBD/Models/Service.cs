using System;

namespace SBD.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public int CarId { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public int StatusId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; }

        // Dane powiązane
        public string CarName { get; set; }
        public string StatusName { get; set; }

        public override string ToString()
        {
            return $"[{ServiceId}] {CarName ?? $"Auto #{CarId}"} | {Description} | {Cost:F2} PLN | {StatusName ?? ""}";
        }
    }
}
