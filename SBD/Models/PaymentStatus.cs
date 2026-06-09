namespace SBD.Models
{
    public class PaymentStatus
    {
        public int StatusId { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"[{StatusId}] {Name}";
        }
    }
}
