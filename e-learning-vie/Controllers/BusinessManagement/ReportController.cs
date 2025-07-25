using e_learning_vie.Commons;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using static e_learning_vie.DTOs.ReportDtos.SchoolOverviewDto;

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

        [HttpGet("department/{subjectGroupId}/latest-semester-scores")]
        public async Task<ActionResult<object>> GetLatestSemesterScores(int subjectGroupId)
        {
            // Xác thực trưởng bộ môn
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var subjectGroup = await _context.SubjectGroups
                .FirstOrDefaultAsync(sg => sg.SubjectGroupId == subjectGroupId && sg.LeadTeacherId == userId);
            if (subjectGroup == null)
            {
                return Unauthorized(new { Message = "Bạn không có quyền xem điểm số của bộ môn này" });
            }

            // Xác định kỳ học mới nhất
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var latestSemester = await _context.Semesters
                .Where(s => s.StartDate <= currentDate && s.EndDate >= currentDate)
                .Include(s => s.AcademicYear)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync() ?? await _context.Semesters
                    .Include(s => s.AcademicYear)
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

            if (latestSemester == null)
            {
                return NotFound(new { Message = "Không tìm thấy kỳ học nào trong cơ sở dữ liệu" });
            }

            // Lấy danh sách ClassSession trong kỳ học mới nhất
            var classSessions = await _context.ClassSessions
                .Where(cs => cs.SemesterId == latestSemester.SemesterId)
                .Select(cs => cs.ClassSessionId)
                .ToListAsync();

            if (!classSessions.Any())
            {
                return NotFound(new { Message = $"Không tìm thấy phiên lớp nào trong kỳ {latestSemester.SemesterName} {latestSemester.AcademicYear.YearName}" });
            }

            // Lấy danh sách môn học thuộc SubjectGroup
            var subjects = await _context.Subjects
                .Where(s => s.SubjectGroupId == subjectGroupId)
                .Select(s => s.SubjectId)
                .ToListAsync();

            if (!subjects.Any())
            {
                return NotFound(new { Message = $"Không tìm thấy môn học nào thuộc bộ môn {subjectGroupId}" });
            }

            // Lấy danh sách ClassSessionId từ TeachingAssignments
            var classSessionIds = await _context.TeachingAssignments
                .Where(ta => classSessions.Contains(ta.ClassSessionId) && subjects.Contains(ta.SubjectId.Value))
                .Select(ta => ta.ClassSessionId)
                .Distinct()
                .ToListAsync();

            if (!classSessionIds.Any())
            {
                return NotFound(new { Message = "Không tìm thấy phân công giảng dạy nào cho các môn học trong kỳ này" });
            }

            // Lấy danh sách EnrollmentId từ Enrollments
            var enrollmentIds = await _context.Enrollments
                .Where(e => classSessionIds.Contains(e.ClassSessionId))
                .Select(e => e.EnrollmentId)
                .ToListAsync();

            if (!enrollmentIds.Any())
            {
                return NotFound(new { Message = "Không tìm thấy học sinh nào được ghi danh trong các phiên lớp này" });
            }

            // Lấy danh sách StudentScores
            var studentScores = await _context.StudentScores
                .Where(ss => enrollmentIds.Contains(ss.EnrollmentId))
                .Include(ss => ss.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(ss => ss.SubjectScore)
                    .ThenInclude(ss => ss.Subject)
                .Include(ss => ss.SubjectScore)
                    .ThenInclude(ss => ss.ScoreType)
                .Include(ss => ss.Exam)
                .Select(ss => new
                {
                    StudentId = ss.Enrollment.Student.StudentId,
                    StudentName = $"{ss.Enrollment.Student.FirstName} {ss.Enrollment.Student.LastName}",
                    SubjectName = ss.SubjectScore.Subject.SubjectName,
                    ScoreTypeName = ss.SubjectScore.ScoreType.TypeName,
                    Score = ss.Score,
                    Weight = ss.SubjectScore.ScoreType.Weight,
                    ExamName = ss.Exam != null ? ss.Exam.ExamType : "N/A",
                    Note = ss.Note ?? "N/A",
                    ClassSessionId = ss.Enrollment.ClassSessionId
                })
                .ToListAsync();

            return Ok(new
            {
                Message = $"Danh sách điểm số của học sinh trong các môn học thuộc bộ môn {subjectGroupId} trong kỳ {latestSemester.SemesterName} {latestSemester.AcademicYear.YearName}",
                Semester = new
                {
                    SemesterId = latestSemester.SemesterId,
                    SemesterName = latestSemester.SemesterName,
                    AcademicYear = latestSemester.AcademicYear.YearName
                },
                Data = studentScores
            });
        }

        private double CalculatePerformanceScore(double averageScore, double passingRate, int classCount, int totalStudents)
        {
            double normalizedAverageScore = averageScore / 10.0;
            double normalizedPassingRate = passingRate / 100.0;
            double normalizedClassCount = Math.Min(classCount / 10.0, 1.0);
            double normalizedStudentCount = Math.Min(totalStudents / 100.0, 1.0);

            return 0.4 * normalizedAverageScore + 0.4 * normalizedPassingRate + 0.1 * normalizedClassCount + 0.1 * normalizedStudentCount;
        }

        // Endpoint 1: Lấy tổng số học sinh/giáo viên/lớp theo năm học
        // GET: api/Dashboard/stats-by-year/1
        [HttpGet("stats-by-year/{yearId}")]
        public async Task<ActionResult<AcademicYearStatsDto>> GetStatsByAcademicYear(int yearId)
        {
            var academicYear = await _context.AcademicYears.FindAsync(yearId);
            if (academicYear == null)
            {
                return NotFound("Không tìm thấy năm học.");
            }

            // Lấy các học kỳ thuộc năm học
            var semestersInYear = _context.Semesters.Where(s => s.AcademicYearId == yearId).Select(s => s.SemesterId);

            // Lấy các phiên lớp học (class session) thuộc các học kỳ đó
            var classSessionsInYear = _context.ClassSessions.Where(cs => semestersInYear.Contains(cs.SemesterId));

            // Tính toán
            var totalStudents = await classSessionsInYear
                .SelectMany(cs => cs.Enrollments)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            var totalTeachers = await classSessionsInYear
                .Select(cs => cs.TeacherId) // Giáo viên chủ nhiệm
                .Union(classSessionsInYear.SelectMany(cs => cs.TeachingAssignments).Select(ta => ta.TeacherId)) // Giáo viên bộ môn
                .Distinct()
                .CountAsync();

            var totalClasses = await classSessionsInYear
                .Select(cs => cs.ClassId)
                .Distinct()
                .CountAsync();

            var result = new AcademicYearStatsDto
            {
                AcademicYearId = academicYear.AcademicYearId,
                YearName = academicYear.YearName,
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses
            };

            return Ok(result);
        }


        // Endpoint 2: Biểu đồ tỷ lệ học sinh theo khối (6, 7, 8, 9)
        // GET: api/Dashboard/student-distribution
        [HttpGet("student-distribution")]
        public async Task<ActionResult<IEnumerable<GradeDistributionDto>>> GetStudentDistribution()
        {
            // Danh sách các khối lớp mục tiêu
            var targetGrades = new[] { "Khối 6", "Khối 7", "Khối 8", "Khối 9" };

            // Lấy AcademicYearId và YearName mới nhất
            var latestAcademicYear = await _context.AcademicYears
                .OrderByDescending(ay => ay.AcademicYearId)//newest
                .Select(ay => new { ay.AcademicYearId, ay.YearName })
                .FirstOrDefaultAsync();

            // Nếu không có năm học nào, trả về danh sách rỗng
            if (latestAcademicYear == null)
            {
                return Ok(new List<GradeDistributionDto>());
            }

            // Truy vấn Enrollments, lọc theo AcademicYearId mới nhất
            var studentCountsByGrade = await _context.Enrollments
                .Include(e => e.ClassSession)
                    .ThenInclude(cs => cs.Class)
                    .ThenInclude(c => c.Grade)
                .Include(e => e.ClassSession)
                    .ThenInclude(cs => cs.Semester)
                .Where(e => targetGrades.Contains(e.ClassSession.Class.Grade.GradeName)
                    && e.ClassSession.Semester.AcademicYearId == latestAcademicYear.AcademicYearId)
                .GroupBy(e => e.ClassSession.Class.Grade.GradeName)
                .Select(g => new
                {
                    GradeName = g.Key,
                    StudentCount = g.Select(e => e.StudentId).Distinct().Count()
                })
                .ToListAsync();

            // Tính tổng số học sinh
            var totalStudents = studentCountsByGrade.Sum(g => g.StudentCount);
            if (totalStudents == 0)
            {
                return Ok(new List<GradeDistributionDto>());
            }

            var result = studentCountsByGrade.Select(g => new GradeDistributionDto
            {
                GradeName = g.GradeName,
                StudentCount = g.StudentCount,
                Percentage = $"{Math.Round((double)g.StudentCount / totalStudents * 100, 2)}%", 
                YearName = latestAcademicYear.YearName
            }).ToList();

            return Ok(result);
        }

        // Endpoint 3: Sĩ số trung bình/lớp và so sánh với quy định
        // GET: api/Dashboard/average-class-size
        [HttpGet("average-class-size")]
        public async Task<ActionResult<ClassSizeStatsDto>> GetAverageClassSize()
        {
            var classSizes = await _context.Enrollments
                .GroupBy(e => e.ClassSessionId)
                .Select(g => g.Count())
                .ToListAsync();

            if (!classSizes.Any())
            {
                return Ok(new ClassSizeStatsDto { AverageClassSize = 0, Status = "Không có dữ liệu" });
            }

            double averageSize = classSizes.Average();
            const int standardSize = 45;

            var result = new ClassSizeStatsDto
            {
                AverageClassSize = Math.Round(averageSize, 2),
                StandardSize = standardSize,
                Status = averageSize <= standardSize ? "Đạt tiêu chuẩn" : "Vượt quá quy định"
            };

            return Ok(result);
        }
    }
}
