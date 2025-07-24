namespace e_learning_vie.Models
{
    public partial class Attendance
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }  // true = có mặt
        public string? Reason { get; set; }
        public int SemesterId { get; set; }
        public virtual Semester Semester { get; set; }

        public virtual Student Student { get; set; }
        public virtual Class Class { get; set; }
    }

}
