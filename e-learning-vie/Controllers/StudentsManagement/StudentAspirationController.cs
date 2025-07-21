using e_learning_vie.Commons;
using e_learning_vie.DTOs.Aspiration;
using e_learning_vie.Enums;
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
                return Ok(ApiResponse<List<AspirationItemDto>>.Success("Lấy danh sách nguyện vọng thành công", aspirations));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAspiration([FromBody] CreateAspirationDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                var aspiration = await _aspirationService.CreateAspirationAsync(User, request);
                return CreatedAtAction(nameof(GetAspirations),
                    new { academicYearId = request.AcademicYearId },
                    ApiResponse<AspirationDto>.Success("Tạo nguyện vọng thành công", aspiration));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpGet("available-schools")]
        public async Task<IActionResult> GetAvailableSchools([FromQuery] SchoolType schoolType = SchoolType.C3)
        {
            try
            {
                var schools = await _aspirationService.GetAvailableSchoolsAsync(schoolType);

                var result = schools.Select(s => new
                {
                    s.SchoolId,
                    s.SchoolName,
                    s.SchoolType,
                    s.Address,
                    s.Phone,
                    s.Email
                });

                return Ok(ApiResponse<object>.Success("Lấy danh sách trường thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpDelete("{aspirationId}")]
        public async Task<IActionResult> DeleteAspiration(int aspirationId)
        {
            try
            {
                await _aspirationService.DeleteAspirationAsync(User, aspirationId);
                return Ok(ApiResponse<object>.Success("Xóa nguyện vọng thành công", null));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpPut("{aspirationId}")]
        public async Task<IActionResult> UpdateAspiration(int aspirationId, [FromBody] CreateAspirationDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                var result = await _aspirationService.UpdateAspirationAsync(User, aspirationId, request);
                return Ok(ApiResponse<AspirationDto>.Success("Cập nhật nguyện vọng thành công", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
