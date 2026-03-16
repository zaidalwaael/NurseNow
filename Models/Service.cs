namespace NurseNow.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        public string NurseId { get; set; }

        public int ServiceCatalogId { get; set; }

        public decimal Price { get; set; }

        public ApplicationUser Nurse { get; set; }

        public ServiceCatalog ServiceCatalog { get; set; }
    }
}