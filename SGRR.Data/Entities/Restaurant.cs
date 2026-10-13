namespace SGRR.Data.Entities
{
    public class Restaurant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
