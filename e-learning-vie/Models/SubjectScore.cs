namespace e_learning_vie.Models
{
    public class SubjectScore
    {
        public int SubjectScoreId { get; set; }
        public int SubjectId { get; set; }
        public int ScoreTypeId { get; set; }
        
        public virtual Subject Subject { get; set; }
        public virtual ScoreType ScoreType { get; set; }

        public virtual ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();


    }
}
