using System;

namespace SBD.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public int BranchId { get; set; }
        public DateTime HireDate { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;

        // Dane powiązane
        public string BranchName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public override string ToString()
        {
            return $"[{EmployeeId}] {FullName} | {Position} | {BranchName ?? $"Oddział #{BranchId}"} | {Email}";
        }
    }
}
