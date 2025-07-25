using e_learning_vie.Commons;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var targetGrades = new[] { "Khối 6", "Khối 7", "Khối 8", "Khối 9" };

            var studentCountsByGrade = await _context.Enrollments
                .Include(e => e.ClassSession.Class.Grade)
                .Where(e => targetGrades.Contains(e.ClassSession.Class.Grade.GradeName))
                .GroupBy(e => e.ClassSession.Class.Grade.GradeName)
                .Select(g => new {
                    GradeName = g.Key,
                    StudentCount = g.Select(e => e.StudentId).Distinct().Count()
                })
                .ToListAsync();

            var totalStudents = studentCountsByGrade.Sum(g => g.StudentCount);
            if (totalStudents == 0)
            {
                return Ok(new List<GradeDistributionDto>());
            }

            var result = studentCountsByGrade.Select(g => new GradeDistributionDto
            {
                GradeName = g.GradeName,
                StudentCount = g.StudentCount,
                Percentage = Math.Round((double)g.StudentCount / totalStudents * 100, 2)
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

        // Endpoint 4: Tỷ lệ giáo viên/học sinh theo tổ bộ môn
        // GET: api/Dashboard/teacher-student-ratio
        [HttpGet("teacher-student-ratio")]
        public async Task<ActionResult<IEnumerable<TeacherStudentRatioDto>>> GetTeacherStudentRatioBySubjectGroup()
        {
            // Viết lại toàn bộ logic vào một câu truy vấn duy nhất
            var ratioData = await _context.SubjectGroups
                .Select(group => new
                {
                    // Chọn ra các trường cần thiết
                    SubjectGroupName = group.SubjectGroupName,

                    // Đếm số giáo viên duy nhất trong tổ thông qua các môn học
                    TeacherCount = group.Subjects
                                        .SelectMany(s => s.TeacherSubjects) // Lấy tất cả các bản ghi TeacherSubject từ các môn học
                                        .Select(ts => ts.TeacherId) // Chọn ra TeacherId
                                        .Distinct()
                                        .Count(),

                    // Đếm số học sinh duy nhất học các môn trong tổ
                    StudentCount = _context.StudentScores
                                         .Where(ss => group.Subjects.Select(s => s.SubjectId).Contains(ss.SubjectId)) // Lọc điểm của các môn trong tổ
                                         .Select(ss => ss.Enrollment.StudentId) // Chọn ra StudentId
                                         .Distinct()
                                         .Count()
                })
                .ToListAsync(); // Thực thi truy vấn và lấy kết quả từ database

            // Sau khi đã có dữ liệu, thực hiện tính toán tỷ lệ trong bộ nhớ
            var result = ratioData.Select(data =>
            {
                string ratio = "N/A";
                if (data.TeacherCount > 0 && data.StudentCount > 0)
                {
                    double studentsPerTeacher = Math.Round((double)data.StudentCount / data.TeacherCount, 1);
                    ratio = $"1 : {studentsPerTeacher}";
                }

                return new TeacherStudentRatioDto
                {
                    SubjectGroupName = data.SubjectGroupName,
                    TeacherCount = data.TeacherCount,
                    StudentCount = data.StudentCount,
                    Ratio = ratio
                };
            }).ToList();

            return Ok(result);
        }
    }
}
