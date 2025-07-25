namespace e_learning_vie.DTOs.ScoreDetail
{
    public class StudentScoreInClassDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentClass { get; set; }

        public bool Gender { get; set; }
        public string Dob { get; set; }

        public string Conduct { get; set; }

        public double AverageAllSubjects { get; set; }

        public string AcademicLevel { get; set; }

        public List<SubjectAverageDTO> Scores { get; set; }
    }
}
