using e_learning_vie.Models;
using e_learning_vie.ModelsDTO.Student;
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
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Monday
            var endOfWeek = startOfWeek.AddDays(6); // Sunday

            var schedules = await _context.Schedules
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Where(s => s.ClassId == student.ClassId &&
                            s.AcademicYearId == student.Class.AcademicYearId)
                .OrderBy(s => s.DayOfWeek)
                .Select(s => new ScheduleItemDto
                {
                    ScheduleId = s.ScheduleId,
                    DayOfWeek = s.DayOfWeek,
                    Room = s.Room,
                    Subject = s.Subject.SubjectName,
                    Teacher = s.Teacher.FirstName + " " + s.Teacher.LastName
                })
                .ToListAsync();

            return new StudentScheduleDto
            {
                StudentName = student.FirstName + " " + student.LastName,
                ClassName = student.Class.ClassName,
                AcademicYear = student.Class.AcademicYear.YearName,
                CurrentWeek = new WeekInfoDto
                {
                    StartDate = startOfWeek,
                    EndDate = endOfWeek,
                    WeekNumber = GetWeekOfYear(today)
                },
                Schedules = schedules
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
