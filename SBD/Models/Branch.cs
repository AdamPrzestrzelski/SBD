namespace SBD.Models
{
    public class Branch
    {
        public int BranchId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;

        public override string ToString()
        {
            return $"[{BranchId}] {Name} | {City} | {Address} | {Phone}";
        }
    }
}
