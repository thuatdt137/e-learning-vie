namespace e_learning_vie.DTOs.ScoreDetail
{
    public class ClassScoreDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public int GradeId { get; set; }
        public string Grade { get; set; }

        public int SemesterId { get; set; }
        public string SemesterName { get; set; }
        public string AcademicYear { get; set; }

        public List<StudentScoreInClassDTO> Students { get; set; }
    }
}
