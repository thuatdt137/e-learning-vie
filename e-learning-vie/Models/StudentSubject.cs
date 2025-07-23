namespace e_learning_vie.Models
{
    public class StudentSubject
    {
        public int StudentSubjectId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ScoreDate { get; set; }
        public bool? IsProgress { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int AcademicYearId { get; set; }
        public virtual AcademicYear AcademicYear { get; set; }
        public virtual Student Student { get; set; }
        public virtual Subject Subject { get; set; }
        public ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();
    }
}
