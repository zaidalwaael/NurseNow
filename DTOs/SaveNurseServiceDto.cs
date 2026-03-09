namespace NurseNow.DTOs
{
    public class SaveNurseServiceDto
    {
        public string ServiceName { get; set; }
        public int DurationInMinutes { get; set; }
        public decimal Price { get; set; }
    }
}