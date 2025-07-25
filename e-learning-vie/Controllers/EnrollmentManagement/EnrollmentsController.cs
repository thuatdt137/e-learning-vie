using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.Enrollment;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.EnrollmentManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public EnrollmentsController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetEnrollments()
        {
            var enrollments = _context.Enrollments.Select(e => new EnrollmentDTO
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                Conduct = e.Conduct,
                ClassSessionId = e.ClassSessionId,
                JoinedDate = e.JoinedDate
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", enrollments));
        }

        [HttpGet("{id}")]
        public IActionResult GetEnrollmentById(int id)
        {
            var enrollment = _context.Enrollments.Where(e => e.EnrollmentId == id).Select(e => new EnrollmentDTO
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                Conduct = e.Conduct,
                ClassSessionId = e.ClassSessionId,
                JoinedDate = e.JoinedDate
            }).FirstOrDefault();
            if (enrollment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", enrollment));
        }

        [HttpPost]
        public IActionResult CreateEnrollment([FromBody] EnrollmentDTO enrollmentDTO)
        {
            if (enrollmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid enrollment data."));
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
            var enrollment = new Enrollment
            {
                StudentId = enrollmentDTO.StudentId,
                Conduct = enrollmentDTO.Conduct,
                ClassSessionId = enrollmentDTO.ClassSessionId,
                JoinedDate = enrollmentDTO.JoinedDate
            };
            var result = _context.Enrollments.Add(enrollment);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Enrollment created successfully.", result.Entity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEnrollment(int id, [FromBody] EnrollmentDTO enrollmentDTO)
        {
            if (enrollmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid enrollment data."));
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
            var enrollment = _context.Enrollments.Find(id);
            if (enrollment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
            }
            enrollment.StudentId = enrollmentDTO.StudentId;
            enrollment.Conduct = enrollmentDTO.Conduct;
            enrollment.ClassSessionId = enrollmentDTO.ClassSessionId;
            enrollment.JoinedDate = enrollmentDTO.JoinedDate;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Enrollment updated successfully.", enrollment));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEnrollment(int id)
        {
            var enrollment = _context.Enrollments.Find(id);
            if (enrollment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
            }
            _context.Enrollments.Remove(enrollment);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Enrollment deleted successfully."));
        }
    }
}
