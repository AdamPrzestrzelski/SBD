namespace SBD.Models
{
    public class ServiceStatus
    {
        public int StatusId { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"[{StatusId}] {Name}";
        }
    }
}
