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
            var effectiveness = await _context.TeachingAssignments
                .Include(ta => ta.Teacher)
                .Include(ta => ta.Subject).ThenInclude(s => s.SubjectGroup)
                .Include(ta => ta.Session).ThenInclude(cs => cs.Class)
                .Include(ta => ta.Session).ThenInclude(cs => cs.Enrollments).ThenInclude(e => e.StudentScores)
                .Where(ta => ta.Session.SemesterId == semesterId)
                .GroupBy(ta => new {
                    ta.Teacher.TeacherId,
                    TeacherName = $"{ta.Teacher.FirstName} {ta.Teacher.LastName}",
                    ta.Subject.SubjectGroup.SubjectGroupName
                })
                .Select(g => new {
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
                .ToListAsync();

            return Ok(ApiResponse<object>.Success("Báo cáo hiệu quả giảng dạy", effectiveness));
        }
    }
}
