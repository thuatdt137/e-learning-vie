using e_learning_vie.DTOs.Student;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace e_learning_vie.Services.Implements
{
    public class StudentScheduleService : IStudentScheduleService
    {
        private readonly SchoolManagementContext _context;
        private readonly IUserContextService _userContextService;

        public StudentScheduleService(SchoolManagementContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<StudentScheduleDto> GetScheduleAsync(ClaimsPrincipal user, int? year = null, int? weekNumber = null)
        {
            var student = await GetStudentAsync(user);

            // Nếu không có tham số, lấy tuần hiện tại
            var currentDate = DateTime.Today;
            var targetYear = year ?? currentDate.Year;
            var targetWeek = weekNumber ?? GetISOWeekOfYear(currentDate);

            // Tính ngày bắt đầu và kết thúc của tuần
            var (startDate, endDate) = GetWeekDateRange(targetYear, targetWeek);

            // Validate tuần có trong phạm vi năm học không
            await ValidateWeekInAcademicYear(student, startDate, endDate);

            // Lấy lịch học
            var schedules = await GetSchedulesForWeek(student, startDate, endDate);

            return new StudentScheduleDto
            {
                StudentName = $"{student.FirstName} {student.LastName}",
                ClassName = student.Class?.ClassName ?? "",
                AcademicYear = student.Class?.AcademicYear?.YearName ?? "",
                CurrentWeek = new WeekInfoDto
                {
                    Year = targetYear,
                    WeekNumber = targetWeek,
                    StartDate = startDate,
                    EndDate = endDate,
                    WeekDescription = $"{startDate:dd/MM} - {endDate:dd/MM}",
                    IsCurrentWeek = IsCurrentWeek(startDate, endDate)
                },
                Schedules = schedules
            };
        }

        public async Task<List<YearOption>> GetAvailableYearsAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);
            var academicYear = student.Class?.AcademicYear;

            if (academicYear?.StartDate == null || academicYear?.EndDate == null)
                throw new InvalidOperationException("Năm học chưa được thiết lập");

            var startYear = academicYear.StartDate.Value.Year;
            var endYear = academicYear.EndDate.Value.Year;
            var currentYear = DateTime.Today.Year;

            var years = new List<YearOption>();

            for (int year = startYear; year <= endYear; year++)
            {
                years.Add(new YearOption
                {
                    Year = year,
                    DisplayText = year.ToString(),
                    IsCurrentYear = year == currentYear,
                    IsAcademicYear = year >= startYear && year <= endYear
                });
            }

            return years.OrderBy(y => y.Year).ToList();
        }

        public async Task<List<WeekOption>> GetAvailableWeeksAsync(ClaimsPrincipal user, int year)
        {
            var student = await GetStudentAsync(user);
            var academicYear = student.Class?.AcademicYear;

            if (academicYear?.StartDate == null || academicYear?.EndDate == null)
                throw new InvalidOperationException("Năm học chưa được thiết lập");

            var weeks = new List<WeekOption>();
            var currentDate = DateTime.Today;
            var currentWeek = GetISOWeekOfYear(currentDate);

            // Lấy tất cả tuần trong năm
            for (int week = 1; week <= GetWeeksInYear(year); week++)
            {
                var (startDate, endDate) = GetWeekDateRange(year, week);

                // Chỉ lấy tuần nằm trong năm học
                if (IsWeekInAcademicYear(startDate, endDate, academicYear))
                {
                    var hasSchedule = await HasScheduleInWeek(student, startDate, endDate);

                    weeks.Add(new WeekOption
                    {
                        WeekNumber = week,
                        StartDate = startDate,
                        EndDate = endDate,
                        DisplayText = $"{startDate:dd/MM} - {endDate:dd/MM}",
                        IsCurrentWeek = year == currentDate.Year && week == currentWeek,
                        HasSchedule = hasSchedule
                    });
                }
            }

            return weeks.OrderBy(w => w.WeekNumber).ToList();
        }

        #region Private Helper Methods

        private async Task<Student> GetStudentAsync(ClaimsPrincipal user)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            var student = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.AcademicYear)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.Class == null)
                throw new InvalidOperationException("Học sinh chưa được phân lớp");

            return student;
        }

        private async Task<List<ScheduleItemDto>> GetSchedulesForWeek(Student student, DateTime startDate, DateTime endDate)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Where(s => s.ClassId == student.ClassId &&
                           s.AcademicYearId == student.Class!.AcademicYearId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var scheduleItems = new List<ScheduleItemDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayOfWeekString = GetDayOfWeekString(date.DayOfWeek);
                var daySchedules = schedules.Where(s => s.DayOfWeek == dayOfWeekString);

                foreach (var schedule in daySchedules)
                {
                    scheduleItems.Add(new ScheduleItemDto
                    {
                        ScheduleId = schedule.ScheduleId,
                        DayOfWeek = schedule.DayOfWeek ?? "",
                        DayName = GetVietnameseDayName(date.DayOfWeek),
                        Date = date,
                        Room = schedule.Room ?? "",
                        Subject = schedule.Subject?.SubjectName ?? "",
                        Teacher = $"{schedule.Teacher?.FirstName ?? ""} {schedule.Teacher?.LastName ?? ""}".Trim(),
                        StartTime = schedule.StartTime?.ToTimeSpan(),
                        EndTime = schedule.EndTime?.ToTimeSpan(),
                        Period = CalculatePeriod(schedule.StartTime)
                    });
                }
            }

            return scheduleItems;
        }

        private async Task<bool> HasScheduleInWeek(Student student, DateTime startDate, DateTime endDate)
        {
            var hasSchedule = await _context.Schedules
                .AnyAsync(s => s.ClassId == student.ClassId &&
                              s.AcademicYearId == student.Class!.AcademicYearId);

            return hasSchedule;
        }

        private Task ValidateWeekInAcademicYear(Student student, DateTime startDate, DateTime endDate)
        {
            var academicYear = student.Class?.AcademicYear;
            if (academicYear?.StartDate == null || academicYear?.EndDate == null)
                throw new InvalidOperationException("Năm học chưa được thiết lập");

            if (!IsWeekInAcademicYear(startDate, endDate, academicYear))
                throw new ArgumentException("Tuần được chọn không nằm trong năm học");

            return Task.CompletedTask;
        }

        private bool IsWeekInAcademicYear(DateTime startDate, DateTime endDate, AcademicYear academicYear)
        {
            var yearStart = academicYear.StartDate!.Value.ToDateTime(TimeOnly.MinValue);
            var yearEnd = academicYear.EndDate!.Value.ToDateTime(TimeOnly.MinValue);

            return startDate <= yearEnd && endDate >= yearStart;
        }

        private (DateTime startDate, DateTime endDate) GetWeekDateRange(int year, int weekNumber)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);

            if (daysOffset > 0)
                firstMonday = firstMonday.AddDays(-7);

            var startDate = firstMonday.AddDays((weekNumber - 1) * 7);
            var endDate = startDate.AddDays(6);

            return (startDate, endDate);
        }

        private int GetISOWeekOfYear(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private int GetWeeksInYear(int year)
        {
            var lastDay = new DateTime(year, 12, 31);
            return GetISOWeekOfYear(lastDay);
        }

        private bool IsCurrentWeek(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            return today >= startDate && today <= endDate;
        }

        private string GetDayOfWeekString(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Monday",
                DayOfWeek.Tuesday => "Tuesday",
                DayOfWeek.Wednesday => "Wednesday",
                DayOfWeek.Thursday => "Thursday",
                DayOfWeek.Friday => "Friday",
                DayOfWeek.Saturday => "Saturday",
                DayOfWeek.Sunday => "Sunday",
                _ => "Monday"
            };
        }

        private string GetVietnameseDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ Hai",
                DayOfWeek.Tuesday => "Thứ Ba",
                DayOfWeek.Wednesday => "Thứ Tư",
                DayOfWeek.Thursday => "Thứ Năm",
                DayOfWeek.Friday => "Thứ Sáu",
                DayOfWeek.Saturday => "Thứ Bảy",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => "Thứ Hai"
            };
        }

        private int CalculatePeriod(TimeOnly? startTime)
        {
            if (!startTime.HasValue) return 1;

            var hour = startTime.Value.Hour;
            var minute = startTime.Value.Minute;

            return hour switch
            {
                7 => 1,
                8 => minute < 30 ? 2 : 3,
                9 => minute < 30 ? 3 : 4,
                10 => minute < 30 ? 4 : 5,
                13 => 6,
                14 => minute < 30 ? 7 : 8,
                15 => minute < 30 ? 8 : 9,
                16 => minute < 30 ? 9 : 10,
                _ => 1
            };
        }

        #endregion
    }
}
