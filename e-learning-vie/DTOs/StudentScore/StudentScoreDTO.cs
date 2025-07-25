namespace e_learning_vie.DTOs.StudentScore
{
    public class StudentScoreDTO
    {
        public double? Score { get; set; }
        public DateTime EnteredDate { get; set; }
        public string? Note { get; set; }
        public int EnrollmentId { get; set; }
        public int SubjectScoreId { get; set; }
        public int? ExamId { get; set; }
    }
}
