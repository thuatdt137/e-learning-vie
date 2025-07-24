namespace e_learning_vie.Models
{
    public class Semester
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; }
        public int AcademicYearId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public virtual AcademicYear AcademicYear { get; set; }

        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public virtual ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();


    }
}
