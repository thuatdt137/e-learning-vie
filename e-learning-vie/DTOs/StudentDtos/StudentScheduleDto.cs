namespace e_learning_vie.DTOs.Student
{
    public class StudentScheduleDto
    {
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string AcademicYear { get; set; }
        public WeekInfoDto CurrentWeek { get; set; }
        public List<ScheduleItemDto> Schedules { get; set; }
    }

    public class WeekInfoDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNumber { get; set; }
        public string WeekDescription { get; set; } // "Tuần hiện tại", "Tuần sau", v.v.
        public bool HasPreviousWeek { get; set; }
        public bool HasNextWeek { get; set; }
    }

    public class ScheduleItemDto
    {
        public int ScheduleId { get; set; }
        public string DayOfWeek { get; set; }
        public string DayName { get; set; } // "Thứ Hai", "Thứ Ba", v.v.
        public DateTime Date { get; set; } // Ngày cụ thể
        public string Room { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int Period { get; set; } // Tiết học
    }
}
