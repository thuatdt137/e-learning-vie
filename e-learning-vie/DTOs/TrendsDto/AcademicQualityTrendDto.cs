namespace e_learning_vie.DTOs.TrendsDto
{
    /// <summary>
    /// Represents the academic quality trend over an academic year and semester.
    /// </summary>
    public class AcademicQualityTrendDto
    {
        public string AcademicYear { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public double OverallAverageScore { get; set; }
        public double ExcellentRate { get; set; } // >= 8.0
        public double GoodRate { get; set; } // 6.5 - 7.9
        public double AverageRate { get; set; } // 5.0 - 6.4
        public double BelowAverageRate { get; set; } // < 5.0
        public double QualityIndex { get; set; } // Chỉ số chất lượng tổng hợp
        public double ChangeFromPreviousSemester { get; set; }
    }
}