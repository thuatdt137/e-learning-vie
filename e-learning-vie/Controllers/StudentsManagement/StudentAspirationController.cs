using e_learning_vie.Enums;
using e_learning_vie.ModelsDTO.Aspiration;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentAspirationController : ControllerBase
    {
        private readonly IStudentAspirationService _aspirationService;

        public StudentAspirationController(IStudentAspirationService aspirationService)
        {
            _aspirationService = aspirationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAspirations([FromQuery] int? academicYearId = null)
        {
            try
            {
                var aspirations = await _aspirationService.GetAspirationsAsync(User, academicYearId);
                return Ok(aspirations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAspiration([FromBody] CreateAspirationDto request)
        {
            try
            {
                var aspiration = await _aspirationService.CreateAspirationAsync(User, request);
                return CreatedAtAction(nameof(GetAspirations),
                    new { academicYearId = request.AcademicYearId },
                    aspiration);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("available-schools")]
        public async Task<IActionResult> GetAvailableSchools([FromQuery] SchoolType schoolType = SchoolType.C3)
        {
            try
            {
                var schools = await _aspirationService.GetAvailableSchoolsAsync(schoolType);
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
