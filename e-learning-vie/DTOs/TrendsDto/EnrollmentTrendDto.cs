namespace e_learning_vie.DTOs.TrendsDto
{
    /// <summary>
    /// Represents the enrollment trend over an academic year.
    /// </summary>
    public class EnrollmentTrendDto
    {
        public string AcademicYear { get; set; } = null!;
        public int TotalStudents { get; set; }
        public int Grade6Students { get; set; }
        public int Grade7Students { get; set; }
        public int Grade8Students { get; set; }
        public int Grade9Students { get; set; }
        public double ChangePercentage { get; set; }
        public int ChangeAmount { get; set; }
    }
}