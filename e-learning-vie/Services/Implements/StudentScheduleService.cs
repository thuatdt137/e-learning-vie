using e_learning_vie.DTOs.Student;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public async Task<StudentScheduleDto> GetCurrentWeekScheduleAsync(ClaimsPrincipal user)
        {
            return await GetWeekScheduleAsync(user, 0);
        }

        public async Task<StudentScheduleDto> GetWeekScheduleAsync(ClaimsPrincipal user, int weekOffset = 0)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            var student = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c.AcademicYear)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.Class == null)
                throw new InvalidOperationException("Học sinh chưa được phân lớp");

            var today = DateTime.Today;
            var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + 1); // Monday
            var targetWeekStart = currentWeekStart.AddDays(weekOffset * 7);

            return await GetWeekScheduleByDateAsync(student, targetWeekStart, weekOffset);
        }

        public async Task<StudentScheduleDto> GetSpecificWeekScheduleAsync(ClaimsPrincipal user, DateTime weekStartDate)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            var student = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c.AcademicYear)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.Class == null)
                throw new InvalidOperationException("Học sinh chưa được phân lớp");

            // Đảm bảo ngày bắt đầu là thứ Hai
            var startOfWeek = weekStartDate.Date.AddDays(-(int)weekStartDate.DayOfWeek + 1);

            var today = DateTime.Today;
            var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + 1);
            var weekOffset = (int)(startOfWeek - currentWeekStart).TotalDays / 7;

            return await GetWeekScheduleByDateAsync(student, startOfWeek, weekOffset);
        }

        private async Task<StudentScheduleDto> GetWeekScheduleByDateAsync(Student student, DateTime weekStartDate, int weekOffset)
        {
            var weekEndDate = weekStartDate.AddDays(6); // Sunday

            var schedules = await _context.Schedules
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Where(s => s.ClassId == student.ClassId &&
                            s.AcademicYearId == student.Class!.AcademicYearId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var scheduleItems = new List<ScheduleItemDto>();
            for (int day = 0; day < 7; day++)
            {
                var currentDate = weekStartDate.AddDays(day);
                var dayOfWeekString = GetDayOfWeekString(currentDate.DayOfWeek);

                var daySchedules = schedules.Where(s => s.DayOfWeek == dayOfWeekString).ToList();

                foreach (var schedule in daySchedules)
                {
                    scheduleItems.Add(new ScheduleItemDto
                    {
                        ScheduleId = schedule.ScheduleId,
                        DayOfWeek = schedule.DayOfWeek ?? "",
                        DayName = GetVietnameseDayName(currentDate.DayOfWeek),
                        Date = currentDate,
                        Room = schedule.Room ?? "",
                        Subject = schedule.Subject?.SubjectName ?? "",
                        Teacher = (schedule.Teacher?.FirstName ?? "") + " " + (schedule.Teacher?.LastName ?? ""),
                        StartTime = schedule.StartTime?.ToTimeSpan(),
                        EndTime = schedule.EndTime?.ToTimeSpan(),
                        Period = CalculatePeriod(schedule.StartTime)
                    });
                }
            }

            // Kiểm tra xem có tuần trước/sau trong phạm vi năm học không
            var academicYear = student.Class!.AcademicYear!;
            var hasPreviousWeek = academicYear.StartDate.HasValue && weekStartDate > academicYear.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            var hasNextWeek = academicYear.EndDate.HasValue && weekEndDate < academicYear.EndDate.Value.ToDateTime(TimeOnly.MinValue);

            return new StudentScheduleDto
            {
                StudentName = student.FirstName + " " + student.LastName,
                ClassName = student.Class.ClassName ?? "",
                AcademicYear = student.Class.AcademicYear.YearName ?? "",
                CurrentWeek = new WeekInfoDto
                {
                    StartDate = weekStartDate,
                    EndDate = weekEndDate,
                    WeekNumber = GetWeekOfYear(weekStartDate),
                    WeekDescription = GetWeekDescription(weekOffset),
                    HasPreviousWeek = hasPreviousWeek,
                    HasNextWeek = hasNextWeek
                },
                Schedules = scheduleItems
            };
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

        private string GetWeekDescription(int weekOffset)
        {
            return weekOffset switch
            {
                0 => "Tuần hiện tại",
                1 => "Tuần sau",
                -1 => "Tuần trước",
                > 1 => $"Tuần sau {weekOffset} tuần",
                < -1 => $"Tuần trước {Math.Abs(weekOffset)} tuần"
            };
        }

        private int CalculatePeriod(TimeOnly? startTime)
        {
            if (!startTime.HasValue) return 1;

            // Tính tiết học dựa trên giờ bắt đầu (giả sử mỗi tiết 45 phút)
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

        private int GetWeekOfYear(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = (int)jan1.DayOfWeek - 1;
            var firstWeekday = jan1.AddDays(-daysOffset);
            var weeksSinceFirst = (date - firstWeekday).Days / 7;
            return weeksSinceFirst + 1;
        }
    }
}
