namespace NurseNow.DTOs.Admin
{
    public class ServiceRequestListDto
    {
        public int RequestId { get; set; }
        public string PatientName { get; set; } = "";
        public string AssignedNurse { get; set; } = "";
        public string ServiceType { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; } = "";
        public string Status { get; set; } = "";
    }
}