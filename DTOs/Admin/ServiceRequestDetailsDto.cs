namespace NurseNow.DTOs.Admin
{
    public class ServiceRequestDetailsDto
    {
        public int RequestId { get; set; }
        public string PatientName { get; set; } = "";
        public string PatientEmail { get; set; } = "";
        public string PatientPhone { get; set; } = "";
        public string AssignedNurse { get; set; } = "";
        public string NurseId { get; set; } = "";
        public string ServiceType { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Status { get; set; } = "";
        public string ServiceAddress { get; set; } = "";
        public string AdditionalNotes { get; set; } = "";
    }
}