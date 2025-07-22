namespace e_learning_vie.DTOs.Student
{
    public class StudentGradeDto
    {
        public string AcademicYear { get; set; } = null!;
        public double OverallAverage { get; set; }
        public List<SubjectGradeDto> SubjectGrades { get; set; } = new List<SubjectGradeDto>();
        public int TotalSubjects { get; set; }
    }
    public class SubjectGradeDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public List<GradeItemDto> Grades { get; set; } = new List<GradeItemDto>();
        public double AverageScore { get; set; }
        public int TotalTests { get; set; }
    }

    public class GradeItemDto
    {
        public int GradeId { get; set; }
        public double? Score { get; set; }
        public string GradeType { get; set; }
        public double Weight { get; set; }
        public DateOnly? DateEntered { get; set; }
        public string? Description { get; set; }
    }

    public class AcademicYearDto
    {
        public int AcademicYearId { get; set; }
        public string YearName { get; set; } = null!;
    }
}
