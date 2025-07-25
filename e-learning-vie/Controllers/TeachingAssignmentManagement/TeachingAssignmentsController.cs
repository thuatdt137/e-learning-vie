using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.TeachingAssignment;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.TeachingAssignmentManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachingAssignmentsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public TeachingAssignmentsController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTeachingAssignments()
        {
            var teachingAssignments = _context.TeachingAssignments.Select(ta => new TeachingAssignmentDTO
            {
                TeachingAssignmentId = ta.TeachingAssignmentId,
                TeacherId = ta.TeacherId,
                SubjectId = ta.SubjectId,
                ClassSessionId = ta.ClassSessionId
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", teachingAssignments));
        }

        [HttpGet("{id}")]
        public IActionResult GetTeachingAssignmentById(int id)
        {
            var teachingAssignment = _context.TeachingAssignments.Where(ta => ta.TeachingAssignmentId == id).Select(ta => new TeachingAssignmentDTO
            {
                TeachingAssignmentId = ta.TeachingAssignmentId,
                TeacherId = ta.TeacherId,
                SubjectId = ta.SubjectId,
                ClassSessionId = ta.ClassSessionId
            }).FirstOrDefault();
            if (teachingAssignment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Teaching assignment not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", teachingAssignment));
        }

        [HttpPost]
        public IActionResult CreateTeachingAssignment([FromBody] TeachingAssignmentDTO teachingAssignmentDTO)
        {
            if (teachingAssignmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid teaching assignment data."));
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
            var teachingAssignment = new TeachingAssignment
            {
                TeacherId = teachingAssignmentDTO.TeacherId,
                SubjectId = teachingAssignmentDTO.SubjectId,
                ClassSessionId = teachingAssignmentDTO.ClassSessionId
            };
            var result = _context.TeachingAssignments.Add(teachingAssignment);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Teaching assignment created successfully.", result.Entity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTeachingAssignment(int id, [FromBody] TeachingAssignmentDTO teachingAssignmentDTO)
        {
            if (teachingAssignmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid teaching assignment data."));
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
            var teachingAssignment = _context.TeachingAssignments.Find(id);
            if (teachingAssignment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Teaching assignment not found."));
            }
            teachingAssignment.TeacherId = teachingAssignmentDTO.TeacherId;
            teachingAssignment.SubjectId = teachingAssignmentDTO.SubjectId;
            teachingAssignment.ClassSessionId = teachingAssignmentDTO.ClassSessionId;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Teaching assignment updated successfully.", teachingAssignment));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTeachingAssignment(int id)
        {
            var teachingAssignment = _context.TeachingAssignments.Find(id);
            if (teachingAssignment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Teaching assignment not found."));
            }
            _context.TeachingAssignments.Remove(teachingAssignment);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Teaching assignment deleted successfully."));
        }
    }
}
