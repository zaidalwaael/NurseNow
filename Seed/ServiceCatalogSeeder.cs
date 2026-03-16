using NurseNow.Data;
using NurseNow.Models;
using Microsoft.EntityFrameworkCore;

namespace NurseNow.Seed
{
    public static class ServiceCatalogSeeder
    {
        public static async Task SeedServiceCatalogAsync(ApplicationDbContext context)
        {
            if (await context.ServiceCatalogs.AnyAsync())
                return;

            var services = new List<ServiceCatalog>
{
    new ServiceCatalog { Name = "IV Therapy", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Wound Care and Dressing", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Injection or Medication Administration", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Post-Surgery Care", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Medication Management", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Vital Signs Monitoring", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Blood Draw or Lab Sample Collection", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Catheter Care", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Diabetes Monitoring and Insulin Injection", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Elderly Home Care Visit", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Oxygen Therapy Setup", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Nebulizer Therapy Session", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Stitches Removal", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Pressure Ulcer Care", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "General Nursing Home Visit", DefaultDurationInMinutes = 60 },
    new ServiceCatalog { Name = "Blood Pressure Check", DefaultDurationInMinutes = 60 },

    new ServiceCatalog { Name = "Short Care Shift (4 hours)", DefaultDurationInMinutes = 240 },
    new ServiceCatalog { Name = "Half-Day Home Care (6 hours)", DefaultDurationInMinutes = 360 },
    new ServiceCatalog { Name = "Full-Day Home Care (12 hours)", DefaultDurationInMinutes = 720 }
};

            await context.ServiceCatalogs.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }
    }
}
