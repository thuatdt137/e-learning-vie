
namespace e_learning_vie.DTOs.ScoreDetail
{
    public class SubjectScoreDTO
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public bool IsMainSubject { get; set; }
        public List<ScoreDetailDTO> ScoreDetails { get; set; }
    }
}
