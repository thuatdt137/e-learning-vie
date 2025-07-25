namespace e_learning_vie.DTOs.Attendance
{
    public class AttendanceDTO
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public string? Note { get; set; }
    }
}
