using e_learning_vie.DTOs.Student;
using e_learning_vie.Models;
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
                    .Include(s => s.StudentClassHistories)
                        .ThenInclude(sch => sch.AcademicYear)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (currentStudent == null)
                    throw new InvalidOperationException("Không tìm thấy học sinh");

                var currentClass = currentStudent.StudentClassHistories
                    .Where(h => h.EndDate == null || h.EndDate > DateOnly.FromDateTime(DateTime.Now))
                    .OrderByDescending(h => h.StartDate)
                    .FirstOrDefault();

                if (currentClass?.AcademicYear == null)
                    throw new InvalidOperationException("Học sinh chưa được phân lớp hoặc không có năm học hiện tại");

                academicYearId = currentClass.AcademicYear.AcademicYearId;
            }

            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

            if (academicYear == null)
                throw new ArgumentException("Không tìm thấy năm học");

            // Tạm thời sử dụng cấu trúc hiện có với Grade và SubjectGrade
            var subjectGrades = await _context.Grades
                .Include(g => g.SubjectGrades)
                    .ThenInclude(sg => sg.Subject)
                .Where(g => g.SubjectGrades.Any())
                .SelectMany(g => g.SubjectGrades)
                .GroupBy(sg => sg.Subject)
                .Select(group => new SubjectGradeDto
                {
                    SubjectId = group.Key.SubjectId,
                    SubjectName = group.Key.SubjectName,
                    Grades = group.Select(sg => new GradeItemDto
                    {
                        GradeId = sg.SubjectGradeId,
                        Score = 8.5, // Mock data - cần StudentScore để lưu điểm thực tế
                        GradeType = sg.Grade.GradeType ?? "Chưa xác định",
                        Weight = sg.Weight,
                        DateEntered = DateOnly.FromDateTime(DateTime.Now),
                        Description = sg.Grade.Description
                    }).OrderBy(g => g.DateEntered).ToList(),
                    
                    AverageScore = CalculateWeightedAverage(group.ToList()),
                    TotalTests = group.Count()
                })
                .OrderBy(s => s.SubjectName)
                .ToListAsync();

            var overallAverage = subjectGrades.Any() ? subjectGrades.Average(g => g.AverageScore) : 0;

            return new StudentGradeDto
            {
                AcademicYear = academicYear.YearName,
                OverallAverage = Math.Round(overallAverage, 2),
                SubjectGrades = subjectGrades,
                TotalSubjects = subjectGrades.Count()
            };
        }

        /// <summary>
        /// Tính điểm trung bình có trọng số cho 1 môn học
        /// </summary>
        /// <param name="subjectGrades">Danh sách SubjectGrade của môn học</param>
        /// <returns>Điểm trung bình có trọng số</returns>
        private double CalculateWeightedAverage(List<SubjectGrade> subjectGrades)
        {
            if (!subjectGrades.Any()) return 0;

            // Mock calculation - trong thực tế cần StudentScore để có điểm thực tế
            var mockScore = 8.5; // Điểm giả định
            var weightedSum = subjectGrades.Sum(sg => mockScore * sg.Weight);
            var totalWeight = subjectGrades.Sum(sg => sg.Weight);
            
            return totalWeight > 0 ? Math.Round(weightedSum / totalWeight, 2) : 0;
        }

        public async Task<List<AcademicYearDto>> GetAcademicYearsWithGradesAsync(ClaimsPrincipal user)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            // Lấy năm học từ StudentClassHistory thay vì Grades
            return await _context.StudentClassHistories
                .Include(sch => sch.AcademicYear)
                .Where(sch => sch.StudentId == studentId && sch.AcademicYear != null)
                .Select(sch => sch.AcademicYear!)
                .Distinct()
                .OrderByDescending(ay => ay.AcademicYearId)
                .Select(ay => new AcademicYearDto
                {
                    AcademicYearId = ay.AcademicYearId,
                    YearName = ay.YearName
                })
                .ToListAsync();
        }
    }
}
