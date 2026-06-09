namespace SBD.Models
{
    public class CarCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"[{CategoryId}] {Name} - {Description}";
        }
    }
}
