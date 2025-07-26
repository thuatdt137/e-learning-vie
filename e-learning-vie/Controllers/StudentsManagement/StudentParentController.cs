using e_learning_vie.Commons;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentParentController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        public StudentParentController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet("children")]
        public async Task<ActionResult<object>> GetChildren()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.User.Id == userId);
            if (parent == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Bạn không có quyền truy cập thông tin phụ huynh"));
            }

            // Lấy danh sách con
            var children = await _context.StudentParents
                .Where(sp => sp.ParentId == parent.ParentId)
                .Include(sp => sp.Student)
                .Select(sp => new
                {
                    StudentId = sp.Student.StudentId,
                    FirstName = sp.Student.FirstName,
                    LastName = sp.Student.LastName,
                    Email = sp.Student.Email,
                    Phone = sp.Student.Phone,
                    Address = sp.Student.Address
                })
                .ToListAsync();

            if (!children.Any())
            {
                return NotFound(ApiResponse<object>.Fail( "Không tìm thấy học sinh nào liên quan đến phụ huynh này"));
            }
            return Ok(ApiResponse<object>.Success("Danh sách các con của phụ huynh", children));
        }

        [HttpGet("child/{studentId}/details")]
        public async Task<ActionResult<object>> GetChildDetails(int studentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.User.Id == userId);
            if (parent == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Bạn không có quyền truy cập thông tin phụ huynh"));
            }

            var studentParent = await _context.StudentParents
                .FirstOrDefaultAsync(sp => sp.ParentId == parent.ParentId && sp.StudentId == studentId);
            if (studentParent == null)
            {
                return Unauthorized(new { Message = "Bạn không có quyền xem thông tin của học sinh này" });
            }

            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentSemester = await _context.Semesters
                .Where(s => s.StartDate <= currentDate && s.EndDate >= currentDate)
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync() ?? await _context.Semesters
                    .Include(s => s.AcademicYear)
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

            if (currentSemester == null)
            {
                return NotFound(new { Message = "Không tìm thấy kỳ học nào trong cơ sở dữ liệu" });
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
            {
                return NotFound(new { Message = $"Không tìm thấy học sinh với ID {studentId}" });
            }

            var subjects = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Join(_context.ClassSessions,
                    e => e.ClassSessionId,
                    cs => cs.ClassSessionId,
                    (e, cs) => new { Enrollment = e, ClassSession = cs })
                .Join(_context.Semesters,
                    x => x.ClassSession.SemesterId,
                    sem => sem.SemesterId,
                    (x, sem) => new { x.Enrollment, x.ClassSession, Semester = sem })
                .Join(_context.TeachingAssignments,
                    x => x.ClassSession.ClassSessionId,
                    ta => ta.ClassSessionId,
                    (x, ta) => new { x.Enrollment, x.ClassSession, x.Semester, TeachingAssignment = ta })
                .Join(_context.Subjects,
                    x => x.TeachingAssignment.SubjectId,
                    s => s.SubjectId,
                    (x, s) => new
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.SubjectName,
                        SemesterName = x.Semester.SemesterName,
                        AcademicYear = x.Semester.AcademicYear.YearName,
                        ClassSessionId = x.ClassSession.ClassSessionId,
                        IsCurrentSemester = x.Semester.SemesterId == currentSemester.SemesterId
                    })
                .Distinct()
                .ToListAsync();

            var currentEnrollment = await _context.Enrollments
                .Include(e => e.ClassSession)
                    .ThenInclude(cs => cs.Semester)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.ClassSession.SemesterId == currentSemester.SemesterId);

            var averageScore = await _context.StudentScores
                .Where(ss => ss.Enrollment.StudentId == studentId && ss.Enrollment.ClassSession.SemesterId == currentSemester.SemesterId)
                .Include(ss => ss.SubjectScore)
                    .ThenInclude(ss => ss.ScoreType)
                .GroupBy(ss => ss.EnrollmentId)
                .Select(g => new
                {
                    WeightedAverage = g.Sum(ss => ss.Score * ss.SubjectScore.ScoreType.Weight) / g.Sum(ss => ss.SubjectScore.ScoreType.Weight)
                })
                .AverageAsync(s => s.WeightedAverage);

            var result = new
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Phone = student.Phone,
                Address = student.Address,
                CurrentSemester = new
                {
                    SemesterId = currentSemester.SemesterId,
                    SemesterName = currentSemester.SemesterName,
                    AcademicYear = currentSemester.AcademicYear.YearName
                },
                Conduct = currentEnrollment?.Conduct ?? "N/A",
                AverageScore = averageScore.HasValue ? Math.Round(averageScore.Value, 2) : (double?)null,
                Subjects = subjects.Select(s => new
                {
                    s.SubjectId,
                    s.SubjectName,
                    s.SemesterName,
                    s.AcademicYear,
                    Status = s.IsCurrentSemester ? "Đang học" : "Đã học"
                })
            };

            return Ok(ApiResponse<object>.Success
            (
                $"Chi tiết thông tin học sinh {student.FirstName} {student.LastName}",
                result
            ));
        }

        [HttpGet("child/{studentId}/subject/{subjectId}/scores")]
        public async Task<ActionResult<object>> GetChildSubjectScores(int studentId, int subjectId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.User.Id == userId);
            if (parent == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Bạn không có quyền truy cập thông tin phụ huynh"));
            }

            var studentParent = await _context.StudentParents
                .FirstOrDefaultAsync(sp => sp.ParentId == parent.ParentId && sp.StudentId == studentId);
            if (studentParent == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Bạn không có quyền xem thông tin của học sinh này"));
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);
            if (subject == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học với ID {subjectId}" ));
            }

            var scoreTypes = await _context.SubjectScores
                .Where(ss => ss.SubjectId == subjectId)
                .Include(ss => ss.ScoreType)
                .Select(ss => new
                {
                    ScoreTypeId = ss.ScoreTypeId,
                    ScoreTypeName = ss.ScoreType.TypeName,
                    Weight = ss.ScoreType.Weight
                })
                .ToListAsync();

            var studentScores = await _context.StudentScores
                .Where(ss => ss.Enrollment.StudentId == studentId && ss.SubjectScore.SubjectId == subjectId)
                .Include(ss => ss.Enrollment)
                    .ThenInclude(e => e.ClassSession)
                        .ThenInclude(cs => cs.Semester)
                            .ThenInclude(sem => sem.AcademicYear)
                .Include(ss => ss.SubjectScore)
                    .ThenInclude(ss => ss.ScoreType)
                .Include(ss => ss.Exam)
                .Select(ss => new
                {
                    ScoreTypeId = ss.SubjectScore.ScoreTypeId,
                    ScoreTypeName = ss.SubjectScore.ScoreType.TypeName,
                    Score = ss.Score,
                    Weight = ss.SubjectScore.ScoreType.Weight,
                    ExamName = ss.Exam != null ? ss.Exam.ExamType : "N/A",
                    Note = ss.Note ?? "N/A",
                    SemesterName = ss.Enrollment.ClassSession.Semester.SemesterName,
                    AcademicYear = ss.Enrollment.ClassSession.Semester.AcademicYear.YearName
                })
                .ToListAsync();

            var scores = scoreTypes
                .GroupJoin(studentScores,
                    st => st.ScoreTypeId,
                    ss => ss.ScoreTypeId,
                    (st, ss) => new
                    {
                        st.ScoreTypeName,
                        st.Weight,
                        Score = ss.FirstOrDefault()?.Score,
                        ExamName = ss.FirstOrDefault()?.ExamName ?? "N/A",
                        Note = ss.FirstOrDefault()?.Note ?? "N/A",
                        SemesterName = ss.FirstOrDefault()?.SemesterName ?? "N/A",
                        AcademicYear = ss.FirstOrDefault()?.AcademicYear ?? "N/A"
                    })
                .ToList();

            return Ok(ApiResponse<object>.Success
            (
                $"Điểm số của học sinh {studentId} trong môn {subject.SubjectName}",
                new
                {
                    SubjectName = subject.SubjectName,
                    Data = scores
                }
            ));
        }
    }
}
