using e_learning_vie.Models;
using e_learning_vie.ModelsDTO.Student;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning_vie.Services.Implements
{
    public class StudentGradeService : IStudentGradeService
    {
        private readonly SchoolManagementContext _context;
        private readonly IUserContextService _userContextService;
        public StudentGradeService(SchoolManagementContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<StudentGradeDto> GetGradesByAcademicYearAsync(ClaimsPrincipal user, int? academicYearId = null)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            if (academicYearId == null)
            {
                var currentStudent = await _context.Students
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (currentStudent?.Class == null)
                    throw new InvalidOperationException("Học sinh chưa được phân lớp");

                academicYearId = currentStudent.Class.AcademicYearId;
            }

            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

            if (academicYear == null)
                throw new ArgumentException("Không tìm thấy năm học");

            var grades = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentId == studentId && g.AcademicYearId == academicYearId)
                .GroupBy(g => g.Subject)
                .Select(group => new SubjectGradeDto
                {
                    SubjectId = group.Key.SubjectId,
                    SubjectName = group.Key.SubjectName,
                    Grades = group.Select(g => new GradeItemDto
                    {
                        GradeId = g.GradeId,
                        Score = g.Score,
                        GradeType = g.GradeType,
                        DateEntered = g.DateEntered
                    }).OrderBy(g => g.DateEntered).ToList(),
                    AverageScore = group.Average(g => g.Score ?? 0),
                    TotalTests = group.Count()
                })
                .OrderBy(s => s.SubjectName)
                .ToListAsync();

            var overallAverage = grades.Any() ? grades.Average(g => g.AverageScore) : 0;

            return new StudentGradeDto
            {
                AcademicYear = academicYear.YearName,
                OverallAverage = Math.Round(overallAverage, 2),
                SubjectGrades = grades,
                TotalSubjects = grades.Count()
            };
        }

        public async Task<List<AcademicYearDto>> GetAcademicYearsWithGradesAsync(ClaimsPrincipal user)
        {
            var studentId = await GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            return await _context.Grades
                .Include(g => g.AcademicYear)
                .Where(g => g.StudentId == studentId)
                .Select(g => g.AcademicYear)
                .Distinct()
                .OrderByDescending(ay => ay.AcademicYearId)
                .Select(ay => new AcademicYearDto
                {
                    AcademicYearId = ay.AcademicYearId,
                    YearName = ay.YearName
                })
                .ToListAsync();
        }

        private async Task<int?> GetCurrentStudentIdAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var userEntity = await _context.Users
                    .Include(u => u.Student)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return userEntity?.StudentId;
            }
            return null;
        }
    }
}
