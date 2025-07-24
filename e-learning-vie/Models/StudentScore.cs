namespace e_learning_vie.Models
{
    public partial class StudentScore
    {
        public int StudentScoreId { get; set; }
        public double? Score { get; set; }
        public DateTime EnteredDate { get; set; }
        public string? Note { get; set; }
        public int EnrollmentId { get; set; }
        public int SubjectId { get; set; }
        public int ScoreTypeId { get; set; }
        public int? ExamId { get; set; }

        public virtual Exam? Exam { get; set; }
        public virtual Enrollment Enrollment { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual ScoreType ScoreType { get; set; }
    }
}
