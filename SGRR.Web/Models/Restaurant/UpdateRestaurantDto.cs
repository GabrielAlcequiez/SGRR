namespace SGRR.Web.Models.Restaurant
{
    public class UpdateRestaurantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
