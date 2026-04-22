namespace NurseNow.DTOs.Nurse
{
    public class SubmitProblemDto
    {
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsUrgent { get; set; } = false;
    }
}