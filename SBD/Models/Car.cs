using System;

namespace SBD.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string PlateNumber { get; set; }
        public string Vin { get; set; }
        public decimal DailyRate { get; set; }
        public int CategoryId { get; set; }
        public int BranchId { get; set; }
        public string Status { get; set; } = "AVAILABLE";
        public int Mileage { get; set; }
        public string Color { get; set; }
        public int Seats { get; set; } = 5;

        // Dane powiązane (z joinów)
        public string CategoryName { get; set; }
        public string BranchName { get; set; }

        public string FullName => $"{Brand} {Model} ({Year})";

        public override string ToString()
        {
            return $"[{CarId}] {FullName} | {PlateNumber} | {DailyRate:F2} PLN/dzień | {Status} | {CategoryName ?? ""}";
        }
    }
}
