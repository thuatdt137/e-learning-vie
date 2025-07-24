namespace e_learning_vie.Models
{
    public partial class Exam
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public string? Room { get; set; }
        public string? ExamType { get; set; }
        public int SemesterId { get; set; }
        public virtual Semester Semester { get; set; }
        
        public virtual ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();
    }
}
