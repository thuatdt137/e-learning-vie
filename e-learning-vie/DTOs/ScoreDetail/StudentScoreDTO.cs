
namespace e_learning_vie.DTOs.ScoreDetail
{
    public class StudentScoreDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentClass { get; set; }
        public bool Gender { get; set; }
        public string Dob { get; set; }
        public string Conduct { get; set; }
        public string Semester { get; set; }
        public string AcademicYear { get; set; }
        public List<SubjectScoreDTO> Scores { get; set; }
    }
}
