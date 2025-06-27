namespace e_learning_vie.ModelsDTO.Student
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
    }

    public class ScheduleItemDto
    {
        public int ScheduleId { get; set; }
        public string DayOfWeek { get; set; }
        public string Room { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
    }
}
