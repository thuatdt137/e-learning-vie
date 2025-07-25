using e_learning_vie.Commons;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Controllers.BusinessManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        public ReportController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet("teaching-effective/{semesterId}")]
        //[Authorize(Roles = "HeaderDepartment")]
        public async Task<IActionResult> GetTeachingEffectiveness(int semesterId)
        {
            var teachingAssignments = _context.TeachingAssignments
                .Include(ta => ta.Teacher)
                .Include(ta => ta.Subject).ThenInclude(s => s.SubjectGroup)
                .Include(ta => ta.Session).ThenInclude(cs => cs.Class)
                .Include(ta => ta.Session).ThenInclude(cs => cs.Enrollments).ThenInclude(e => e.StudentScores)
                .Where(ta => ta.Session.SemesterId == semesterId)
                .ToList();

            if (!teachingAssignments.Any())
            {
                return Ok(ApiResponse<object>.Success("Không có dữ liệu cho semester này", new List<object>()));
            }

            var groupedData = teachingAssignments
                .GroupBy(ta => new
                {
                    ta.Teacher.TeacherId,
                    TeacherName = $"{ta.Teacher.FirstName} {ta.Teacher.LastName}",
                    ta.Subject.SubjectGroup.SubjectGroupName
                });

            var effectiveness = groupedData
                .Select(g => new
                {
                    TeacherId = g.Key.TeacherId,
                    TeacherName = g.Key.TeacherName,
                    SubjectGroup = g.Key.SubjectGroupName,
                    ClassCount = g.Count(),
                    TotalStudents = g.Sum(ta => ta.Session.Enrollments.Count),
                    AverageScore = g.Average(ta => ta.Session.Enrollments
                        .SelectMany(e => e.StudentScores)
                        .Where(ss => ss.SubjectId == ta.SubjectId)
                        .Average(ss => (double?)ss.Score) ?? 0)
                })
                .ToList();

            return Ok(ApiResponse<object>.Success("Báo cáo hiệu quả giảng dạy", effectiveness));
        }

        [HttpGet("teaching-performance/{semesterId}")]
        public async Task<IActionResult> GetTeachingPerformance(int semesterId, [FromQuery] int? subjectGroupId = null)
        {
            var query = _context.TeachingAssignments
                .Include(ta => ta.Teacher)
                .Include(ta => ta.Subject).ThenInclude(s => s.SubjectGroup)
                .Include(ta => ta.Session).ThenInclude(cs => cs.Class)
                .Include(ta => ta.Session).ThenInclude(cs => cs.Enrollments).ThenInclude(e => e.StudentScores)
                .Where(ta => ta.Session.SemesterId == semesterId);

            if (subjectGroupId.HasValue)
            {
                query = query.Where(ta => ta.Subject.SubjectGroupId == subjectGroupId.Value);
            }

            var teachingAssignments = await query.ToListAsync();

            if (!teachingAssignments.Any())
            {
                return Ok(ApiResponse<object>.Success("Không có dữ liệu cho semester hoặc tổ bộ môn này"));
            }

            var performanceBySubjectGroup = teachingAssignments
                .GroupBy(ta => new
                {
                    SubjectGroupId = ta.Subject.SubjectGroup != null ? ta.Subject.SubjectGroup.SubjectGroupId : 0,
                    SubjectGroupName = ta.Subject.SubjectGroup != null ? ta.Subject.SubjectGroup.SubjectGroupName : "Không có nhóm môn học"
                })
                .Select(g => new
                {
                    SubjectGroupId = g.Key.SubjectGroupId,
                    SubjectGroupName = g.Key.SubjectGroupName,

                    AverageScore = g.Average(ta => ta.Session.Enrollments
                        .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                        .Where(ss => ss.SubjectId == ta.SubjectId)
                        .Average(ss => (double?)ss.Score) ?? 0),


                    Teachers = g.GroupBy(ta => new
                    {
                        ta.Teacher.TeacherId,
                        TeacherName = $"{ta.Teacher.FirstName} {ta.Teacher.LastName}"
                    })
                    .Select(t => new
                    {
                        TeacherId = t.Key.TeacherId,
                        TeacherName = t.Key.TeacherName,

                        ClassCount = t.Count(),
                        TotalStudents = t.Sum(ta => ta.Session.Enrollments?.Count ?? 0),

                        AverageScore = t.Average(ta => ta.Session.Enrollments
                            .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                            .Where(ss => ss.SubjectId == ta.SubjectId)
                            .Average(ss => (double?)ss.Score) ?? 0),
                        
                        PassingRate = t.SelectMany(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId))
                            .Count(ss => ss.Score >= 5) * 100.0 /
                            (t.SelectMany(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId))
                            .Count() > 0 ? t.SelectMany(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId))
                            .Count() : 1),
                                           
                        PerformanceScore = CalculatePerformanceScore(
                            t.Average(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId)
                                .Average(ss => (double?)ss.Score) ?? 0),
                            t.SelectMany(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId))
                            .Count(ss => ss.Score >= 5) * 100.0 /
                            (t.SelectMany(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId))
                            .Count() > 0 ? t.SelectMany(ta => ta.Session.Enrollments
                                .SelectMany(e => e.StudentScores ?? Enumerable.Empty<StudentScore>())
                                .Where(ss => ss.SubjectId == ta.SubjectId))
                            .Count() : 1),
                            t.Count(),
                            t.Sum(ta => ta.Session.Enrollments?.Count ?? 0))
                    })
                    .ToList()
                })
                .OrderBy(g => g.SubjectGroupName)
                .ToList();

            return Ok(ApiResponse<object>.Success("Báo cáo hiệu quả giảng dạy theo tổ bộ môn", performanceBySubjectGroup));
        }

        private double CalculatePerformanceScore(double averageScore, double passingRate, int classCount, int totalStudents)
        {
            double normalizedAverageScore = averageScore / 10.0;
            double normalizedPassingRate = passingRate / 100.0;
            double normalizedClassCount = Math.Min(classCount / 10.0, 1.0);
            double normalizedStudentCount = Math.Min(totalStudents / 100.0, 1.0);

            return 0.4 * normalizedAverageScore + 0.4 * normalizedPassingRate + 0.1 * normalizedClassCount + 0.1 * normalizedStudentCount;
        }
    }
}
