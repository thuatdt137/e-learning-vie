namespace e_learning_vie.Models
{
    public partial class Attendance
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }  // true = có mặt
        public string? Note { get; set; }

        public virtual Student Student { get; set; }
        public virtual Schedule Schedule { get; set; }
    }

}
