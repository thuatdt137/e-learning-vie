namespace e_learning_vie.DTOs.ReportDtos
{
    public class SchoolOverviewDto
    {
        // DTO cho thống kê theo năm học
        public class AcademicYearStatsDto
        {
            public int AcademicYearId { get; set; }
            public string YearName { get; set; }
            public int TotalStudents { get; set; }
            public int TotalTeachers { get; set; }
            public int TotalClasses { get; set; }
        }

        // DTO cho biểu đồ tỷ lệ học sinh
        public class GradeDistributionDto
        {
            public string YearName { get; set; }
            public string GradeName { get; set; }
            public int StudentCount { get; set; }
            public String Percentage { get; set; }
        }

        // DTO cho sĩ số trung bình
        public class ClassSizeStatsDto
        {
            public double AverageClassSize { get; set; }
            public int StandardSize { get; set; } = 45;
            public string Status { get; set; }
        }

        // DTO cho tỷ lệ giáo viên/học sinh
        public class TeacherStudentRatioDto
        {
            public string SubjectGroupName { get; set; }
            public int TeacherCount { get; set; }
            public int StudentCount { get; set; }
            public string Ratio { get; set; }
        }
    }
}
