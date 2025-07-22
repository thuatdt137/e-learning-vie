namespace e_learning_vie.DTOs.Student
{
    public class StudentScheduleDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public WeekInfoDto CurrentWeek { get; set; } = new WeekInfoDto();
        public List<ScheduleItemDto> Schedules { get; set; } = new List<ScheduleItemDto>();
    }

    public class WeekInfoDto
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string WeekDescription { get; set; } = string.Empty; // "01/12 - 07/12"
        public bool IsCurrentWeek { get; set; }
    }

    public class ScheduleItemDto
    {
        public int ScheduleId { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string DayName { get; set; } = string.Empty; // "Thứ Hai", "Thứ Ba", v.v.
        public DateTime Date { get; set; } // Ngày cụ thể
        public string Room { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int Period { get; set; } // Tiết học
    }

    // DTOs for selection options
    public class YearOption
    {
        public int Year { get; set; }
        public string DisplayText { get; set; } = string.Empty; // "2025"
        public bool IsCurrentYear { get; set; }
        public bool IsAcademicYear { get; set; } // Có phải năm học hiện tại
    }

    public class WeekOption
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DisplayText { get; set; } = string.Empty; // "01/12 - 07/12"
        public bool IsCurrentWeek { get; set; }
        public bool HasSchedule { get; set; } // Có lịch học không
    }
}
