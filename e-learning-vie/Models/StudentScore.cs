namespace e_learning_vie.Models
{
    public partial class StudentScore
    {
        public int StudentScoreId { get; set; }
        public double? Score { get; set; }
        public DateTime ScoreDate { get; set; }
        public string? Description { get; set; }
        public int StudentSubjectId { get; set; }
        public int SubjectGradeId { get; set; }
        public virtual StudentSubject StudentSubject { get; set; }
        public virtual SubjectGrade SubjectGrade { get; set; }
    }
}
