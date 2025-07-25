using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.Exam;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.ExamManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public ExamsController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetExams()
        {
            var exams = _context.Exams.Select(e => new ExamDTO
            {
                ExamId = e.ExamId,
                ExamDate = e.ExamDate,
                Room = e.Room,
                ExamType = e.ExamType,
                SemesterId = e.SemesterId
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", exams));
        }

        [HttpGet("{id}")]
        public IActionResult GetExamById(int id)
        {
            var exam = _context.Exams.Where(e => e.ExamId == id).Select(e => new ExamDTO
            {
                ExamId = e.ExamId,
                ExamDate = e.ExamDate,
                Room = e.Room,
                ExamType = e.ExamType,
                SemesterId = e.SemesterId
            }).FirstOrDefault();
            if (exam == null)
            {
                return NotFound(ApiResponse<object>.Fail("Exam not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", exam));
        }

        [HttpPost]
        public IActionResult CreateExam([FromBody] ExamDTO examDTO)
        {
            if (examDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid exam data."));
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
            var exam = new Exam
            {
                ExamDate = examDTO.ExamDate,
                Room = examDTO.Room,
                ExamType = examDTO.ExamType,
                SemesterId = examDTO.SemesterId
            };
            var result = _context.Exams.Add(exam);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Exam created successfully.", result.Entity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateExam(int id, [FromBody] ExamDTO examDTO)
        {
            if (examDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid exam data."));
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
            var exam = _context.Exams.Find(id);
            if (exam == null)
            {
                return NotFound(ApiResponse<object>.Fail("Exam not found."));
            }
            exam.ExamDate = examDTO.ExamDate;
            exam.Room = examDTO.Room;
            exam.ExamType = examDTO.ExamType;
            exam.SemesterId = examDTO.SemesterId;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Exam updated successfully.", exam));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null)
            {
                return NotFound(ApiResponse<object>.Fail("Exam not found."));
            }
            _context.Exams.Remove(exam);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Exam deleted successfully."));
        }
    }
}
