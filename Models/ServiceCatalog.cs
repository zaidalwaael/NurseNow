namespace NurseNow.Models
{
    public class ServiceCatalog
    {
        public int ServiceCatalogId { get; set; }
        public string Name { get; set; }
        public int DefaultDurationInMinutes { get; set; }
    }
}