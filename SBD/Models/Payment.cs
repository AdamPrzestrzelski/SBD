using System;

namespace SBD.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int RentalId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int StatusId { get; set; }
        public int InstallmentNo { get; set; } = 1;
        public int TotalInstallments { get; set; } = 1;
        public string PaymentMethod { get; set; }

        // Dane powiązane
        public string StatusName { get; set; }

        public bool IsInstallment => TotalInstallments > 1;

        public override string ToString()
        {
            string installmentInfo = IsInstallment ? $" (Rata {InstallmentNo}/{TotalInstallments})" : "";
            return $"[{PaymentId}] Wypożyczenie #{RentalId} | {Amount:F2} PLN | " +
                   $"{StatusName ?? $"Status #{StatusId}"} | {PaymentMethod}{installmentInfo}";
        }
    }
}
