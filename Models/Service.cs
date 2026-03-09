namespace NurseNow.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string NurseId { get; set; }
        public string ServiceName { get; set; }
        public int DurationInMinutes { get; set; }
        public decimal Price { get; set; }

        public ApplicationUser Nurse { get; set; }
    }
}