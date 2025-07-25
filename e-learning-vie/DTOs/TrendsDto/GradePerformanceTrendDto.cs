namespace e_learning_vie.DTOs.TrendsDto
{
    /// <summary>
    /// Represents the performance trend of a grade over an academic year.
    /// </summary>
    public class GradePerformanceTrendDto
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; } = null!;
        public string AcademicYear { get; set; } = null!;
        public double AverageScore { get; set; }
        public int StudentCount { get; set; }
        public double ExcellentRate { get; set; }
        public double PassRate { get; set; } 
    }
}
