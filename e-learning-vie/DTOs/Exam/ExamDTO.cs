namespace e_learning_vie.DTOs.Exam
{
    public class ExamDTO
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public string? Room { get; set; }
        public string? ExamType { get; set; }
        public int SemesterId { get; set; }
    }
}
