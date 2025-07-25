namespace e_learning_vie.DTOs.ScoreDetail
{
    public class SubjectAverageDTO
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public bool IsMainSubject { get; set; }
        public double AverageScore { get; set; }
    }
}
