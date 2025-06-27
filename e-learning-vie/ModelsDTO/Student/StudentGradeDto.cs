namespace e_learning_vie.ModelsDTO.Student
{
    public class StudentGradeDto
    {
        public string AcademicYear { get; set; }
        public double OverallAverage { get; set; }
        public List<SubjectGradeDto> SubjectGrades { get; set; }
        public int TotalSubjects { get; set; }
    }
    public class SubjectGradeDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public List<GradeItemDto> Grades { get; set; }
        public double AverageScore { get; set; }
        public int TotalTests { get; set; }
    }

    public class GradeItemDto
    {
        public int GradeId { get; set; }
        public double? Score { get; set; }
        public string GradeType { get; set; }
        public DateOnly? DateEntered { get; set; }
    }

    public class AcademicYearDto
    {
        public int AcademicYearId { get; set; }
        public string YearName { get; set; }
    }
}
