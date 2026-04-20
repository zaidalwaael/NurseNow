namespace NurseNow.DTOs.Admin
{
    public class NotificationStatsDto
    {
        public int AllUsers { get; set; }
        public int NursesOnly { get; set; }
        public int PatientsOnly { get; set; }
    }
}