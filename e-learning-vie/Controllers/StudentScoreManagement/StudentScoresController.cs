using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.StudentScore;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.StudentScoreManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentScoresController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public StudentScoresController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetStudentScores()
        {
            var studentScores = _context.StudentScores.Select(ss => new
            {
                StudentScoreId = ss.StudentScoreId,
                Score = ss.Score,
                EnteredDate = ss.EnteredDate,
                Note = ss.Note,
                EnrollmentId = ss.EnrollmentId,
                SubjectScoreId = ss.SubjectScoreId,
                ExamId = ss.ExamId
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", studentScores));
        }

        [HttpGet("{id}")]
        public IActionResult GetStudentScoreById(int id)
        {
            var studentScore = _context.StudentScores.Where(ss => ss.StudentScoreId == id).Select(ss => new
            {
                StudentScoreId = ss.StudentScoreId,
                Score = ss.Score,
                EnteredDate = ss.EnteredDate,
                Note = ss.Note,
                EnrollmentId = ss.EnrollmentId,
                SubjectScoreId = ss.SubjectScoreId,
                ExamId = ss.ExamId
            }).FirstOrDefault();
            if (studentScore == null)
            {
                return NotFound(ApiResponse<object>.Fail("Student score not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", studentScore));
        }

        [HttpPost]
        public IActionResult CreateStudentScore([FromBody] StudentScoreDTO studentScoreDTO)
        {
            if (studentScoreDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid student score data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value != null && kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var studentScore = new StudentScore
            {
                Score = studentScoreDTO.Score,
                EnteredDate = studentScoreDTO.EnteredDate,
                Note = studentScoreDTO.Note,
                EnrollmentId = studentScoreDTO.EnrollmentId,
                SubjectScoreId = studentScoreDTO.SubjectScoreId,
                ExamId = studentScoreDTO.ExamId
            };
            var result = _context.StudentScores.Add(studentScore);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Student score created successfully.", result.Entity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudentScore(int id, [FromBody] StudentScoreDTO studentScoreDTO)
        {
            if (studentScoreDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid student score data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value != null && kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var studentScore = _context.StudentScores.Find(id);
            if (studentScore == null)
            {
                return NotFound(ApiResponse<object>.Fail("Student score not found."));
            }
            studentScore.Score = studentScoreDTO.Score;
            studentScore.EnteredDate = studentScoreDTO.EnteredDate;
            studentScore.Note = studentScoreDTO.Note;
            studentScore.EnrollmentId = studentScoreDTO.EnrollmentId;
            studentScore.SubjectScoreId = studentScoreDTO.SubjectScoreId;
            studentScore.ExamId = studentScoreDTO.ExamId;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Student score updated successfully.", studentScore));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudentScore(int id)
        {
            var studentScore = _context.StudentScores.Find(id);
            if (studentScore == null)
            {
                return NotFound(ApiResponse<object>.Fail("Student score not found."));
            }
            _context.StudentScores.Remove(studentScore);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Student score deleted successfully."));
        }
    }
}
