namespace e_learning_vie.DTOs.TrendsDto
{
    /// <summary>
    /// Represents the performance trend of a subject over an academic year.
    /// </summary>
    public class SubjectPerformanceTrendDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public string AcademicYear { get; set; } = null!;
        public double AverageScore { get; set; }
        public double PassRate { get; set; }
        public double ExcellentRate { get; set; }
        public int StudentCount { get; set; }
        public double DifficultyIndex { get; set; }
    }
}