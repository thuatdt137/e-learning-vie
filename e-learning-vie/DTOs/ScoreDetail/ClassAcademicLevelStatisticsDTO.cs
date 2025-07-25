namespace e_learning_vie.DTOs.ScoreDetail
{
    public class ClassAcademicLevelStatisticsDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; }
        public string AcademicYear { get; set; }

        public int TotalStudents { get; set; }
        public int MaleStudents { get; set; }
        public int FemaleStudents { get; set; }

        public double AverageScore { get; set; }
        public List<SubjectAverageDTO> SubjectAverages { get; set; }

        public Dictionary<string, int> AcademicLevelStats { get; set; }
        public Dictionary<string, int> ConductStats { get; set; }
    }
}
