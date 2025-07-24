namespace e_learning_vie.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? Conduct { get; set; }

        public int ClassSessionId { get; set; }
        public DateOnly JoinedDate { get; set; }

        public virtual Student Student { get; set; }

        public virtual ClassSession ClassSession { get; set; }

        public virtual ICollection<StudentScore> StudentScores { get; set; }
    }
}
