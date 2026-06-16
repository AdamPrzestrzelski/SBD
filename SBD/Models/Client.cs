using System;

namespace SBD.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Pesel { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public bool IsBlocked { get; set; }
        public decimal PenaltyMultiplier { get; set; } = 1.00m;
        public string PasswordHash { get; set; }
        public DateTime RegistrationDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public override string ToString()
        {
            return $"[{ClientId}] {FullName} | {Email} | {(IsBlocked ? "ZABLOKOWANY" : "Aktywny")} | Mnożnik: {PenaltyMultiplier:F2}";
        }
    }
}
