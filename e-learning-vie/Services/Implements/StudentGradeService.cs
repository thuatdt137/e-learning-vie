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
            //var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            //if (studentId == null)
            //    throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            //if (academicYearId == null)
            //{
            //    // Lấy năm học hiện tại từ StudentClassHistories
            //    var currentStudent = await _context.Students
            //        .Include(s => s.StudentClassHistories)
            //            .ThenInclude(sch => sch.AcademicYear)
            //        .FirstOrDefaultAsync(s => s.StudentId == studentId);

            //    if (currentStudent == null)
            //        throw new InvalidOperationException("Không tìm thấy học sinh");

            //    var currentClassHistory = currentStudent.StudentClassHistories
            //        .Where(h => h.EndDate == null || h.EndDate > DateOnly.FromDateTime(DateTime.Now))
            //        .OrderByDescending(h => h.StartDate)
            //        .FirstOrDefault();

            //    if (currentClassHistory?.AcademicYear == null)
            //        throw new InvalidOperationException("Học sinh chưa được phân lớp hoặc không có năm học hiện tại");

            //    academicYearId = currentClassHistory.AcademicYear.AcademicYearId;
            //}

            //var academicYear = await _context.AcademicYears
            //    .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

            //if (academicYear == null)
            //    throw new ArgumentException("Không tìm thấy năm học");

            //// Sử dụng cấu trúc mới: Student → StudentScore → SubjectGrade → Subject/Grade
            //var studentScores = await _context.Students
            //    .Where(s => s.StudentId == studentId)
            //    .SelectMany(s => s.StudentScores)
            //    .Include(ss => ss.SubjectGrade)
            //        .ThenInclude(sg => sg.Subject)
            //    .Include(ss => ss.SubjectGrade)
            //        .ThenInclude(sg => sg.Grade)
            //    .ToListAsync();

            //var grades = studentScores
            //    .GroupBy(ss => ss.SubjectGrade.Subject)
            //    .Select(group => new SubjectGradeDto
            //    {
            //        SubjectId = group.Key.SubjectId,
            //        SubjectName = group.Key.SubjectName,
            //        Grades = group.Select(ss => new GradeItemDto
            //        {
            //            GradeId = ss.StudentScoreId,
            //            Score = ss.Score,
            //            GradeType = ss.SubjectGrade.Grade.GradeType ?? "Không xác định",
            //            Weight = ss.SubjectGrade.Weight,
            //            DateEntered = DateOnly.FromDateTime(ss.ScoreDate),
            //            Description = ss.Description
            //        }).OrderBy(g => g.DateEntered).ToList(),

            //        // Tính điểm trung bình có trọng số
            //        AverageScore = CalculateWeightedAverage(group.ToList()),
            //        TotalTests = group.Count()
            //    })
            //    .OrderBy(s => s.SubjectName)
            //    .ToList();

            //var overallAverage = grades.Any() ? grades.Average(g => g.AverageScore) : 0;

            //return new StudentGradeDto
            //{
            //    AcademicYear = academicYear.YearName,
            //    OverallAverage = Math.Round(overallAverage, 2),
            //    SubjectGrades = grades,
            //    TotalSubjects = grades.Count()
            //};
            return null;
        }

        /// <summary>
        /// Tính điểm trung bình có trọng số
        /// </summary>
        private double CalculateWeightedAverage(List<StudentScore> scores)
        {
            if (!scores.Any()) return 0;

            var weightedSum = scores.Sum(s => (s.Score ?? 0) * s.SubjectGrade.Weight);
            var totalWeight = scores.Sum(s => s.SubjectGrade.Weight);

            return totalWeight > 0 ? Math.Round(weightedSum / totalWeight, 2) : 0;
        }

        public async Task<List<AcademicYearDto>> GetAcademicYearsWithGradesAsync(ClaimsPrincipal user)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            // Lấy năm học từ StudentClassHistories thay vì Grades
            return await _context.ClassHistories
                .Include(ch => ch.AcademicYear)
                .Where(ch => ch.StudentId == studentId && ch.AcademicYear != null)
                .Select(ch => ch.AcademicYear!)
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
