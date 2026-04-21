namespace NurseNow.Models
{
    public class NurseDocument
    {
        public int Id { get; set; }
        public string NurseId { get; set; } = "";
        public string DocumentName { get; set; } = "";
        public string FileUrl { get; set; } = "";
    }
}