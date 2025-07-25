using e_learning_vie.Commons;
using e_learning_vie.DTOs.Subject;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace e_learning_vie.Controllers.SubjectManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        public SubjectsController(SchoolManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetSubjects()
        {
            var subjects = _context.Subjects.Select(s => new SubjectDTO
            {
                SubjectId = s.SubjectId,
                SubjectName = s.SubjectName,
                SubjectGroupId = s.SubjectGroupId
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", subjects));
        }
        [HttpGet("{id}")]
        public IActionResult GetSubjectById(int id)
        {
            var subject = _context.Subjects.Where(s => s.SubjectId == id).Select(s => new SubjectDTO
            {
                SubjectId = s.SubjectId,
                SubjectName = s.SubjectName,
                SubjectGroupId = s.SubjectGroupId
            }).FirstOrDefault();
            if (subject == null)
            {
                return NotFound(ApiResponse<object>.Fail("Subject not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", subject));
        }
        [HttpPost]
        public IActionResult CreateSubject([FromBody] SubjectDTO subjectDTO)
        {
            if (subjectDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid subject data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var subject = new Subject
            {
                SubjectName = subjectDTO.SubjectName,
                SubjectGroupId = subjectDTO.SubjectGroupId ?? 0
            };
            var result = _context.Subjects.Add(subject);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Subject created successfully.", result.Entity));
        }
        [HttpPut("{id}")]
        public IActionResult UpdateSubject(int id, [FromBody] SubjectDTO subjectDTO)
        {
            if (subjectDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid subject data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var subject = _context.Subjects.Find(id);
            if (subject == null)
            {
                return NotFound(ApiResponse<object>.Fail("Subject not found."));
            }
            subject.SubjectName = subjectDTO.SubjectName;
            subject.SubjectGroupId = subjectDTO.SubjectGroupId ?? 0;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Subject updated successfully.", subject));
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteSubject(int id)
        {
            var subject = _context.Subjects.Find(id);
            if (subject == null)
            {
                return NotFound(ApiResponse<object>.Fail("Subject not found."));
            }
            _context.Subjects.Remove(subject);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Subject deleted successfully."));
        }
    }
}
