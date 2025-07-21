using e_learning_vie.Commons;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Controllers.AdminManagement
{
    [Route("api/admin/aspiration-time")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AspirationTimeController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public AspirationTimeController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet("{academicYearId}")]
        public async Task<ActionResult<ApiResponse<AspirationTimeConfigDto>>> GetAspirationTimeConfig(int academicYearId)
        {
            try
            {
                var academicYear = await _context.AcademicYears
                    .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

                if (academicYear == null)
                    return NotFound(ApiResponse<AspirationTimeConfigDto>.Fail("Không tìm thấy năm học"));

                var config = new AspirationTimeConfigDto
                {
                    AcademicYearId = academicYear.AcademicYearId,
                    YearName = academicYear.YearName,
                    AspirationRegistrationStartDate = academicYear.AspirationRegistrationStartDate,
                    AspirationRegistrationEndDate = academicYear.AspirationRegistrationEndDate,
                    AspirationEditDeadline = academicYear.AspirationEditDeadline
                };

                return Ok(ApiResponse<AspirationTimeConfigDto>.Success("Lấy thông tin thành công", config));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AspirationTimeConfigDto>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpPut("{academicYearId}")]
        public async Task<ActionResult<ApiResponse<AspirationTimeConfigDto>>> UpdateAspirationTimeConfig(
            int academicYearId,
            UpdateAspirationTimeConfigDto request)
        {
            try
            {
                var academicYear = await _context.AcademicYears
                    .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

                if (academicYear == null)
                    return NotFound(ApiResponse<AspirationTimeConfigDto>.Fail("Không tìm thấy năm học"));

                // Validate logic: start date < end date < edit deadline
                if (request.AspirationRegistrationStartDate.HasValue &&
                    request.AspirationRegistrationEndDate.HasValue &&
                    request.AspirationRegistrationStartDate.Value >= request.AspirationRegistrationEndDate.Value)
                {
                    return BadRequest(ApiResponse<AspirationTimeConfigDto>.Fail(
                        "Ngày bắt đầu phải nhỏ hơn ngày kết thúc đăng ký"));
                }

                if (request.AspirationRegistrationEndDate.HasValue &&
                    request.AspirationEditDeadline.HasValue &&
                    request.AspirationRegistrationEndDate.Value > request.AspirationEditDeadline.Value)
                {
                    return BadRequest(ApiResponse<AspirationTimeConfigDto>.Fail(
                        "Ngày kết thúc đăng ký phải nhỏ hơn hoặc bằng hạn chót chỉnh sửa"));
                }

                academicYear.AspirationRegistrationStartDate = request.AspirationRegistrationStartDate;
                academicYear.AspirationRegistrationEndDate = request.AspirationRegistrationEndDate;
                academicYear.AspirationEditDeadline = request.AspirationEditDeadline;

                await _context.SaveChangesAsync();

                var result = new AspirationTimeConfigDto
                {
                    AcademicYearId = academicYear.AcademicYearId,
                    YearName = academicYear.YearName,
                    AspirationRegistrationStartDate = academicYear.AspirationRegistrationStartDate,
                    AspirationRegistrationEndDate = academicYear.AspirationRegistrationEndDate,
                    AspirationEditDeadline = academicYear.AspirationEditDeadline
                };

                return Ok(ApiResponse<AspirationTimeConfigDto>.Success("Cập nhật cấu hình thời gian thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AspirationTimeConfigDto>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AspirationTimeConfigDto>>>> GetAllAspirationTimeConfigs()
        {
            try
            {
                var configs = await _context.AcademicYears
                    .OrderByDescending(ay => ay.StartDate)
                    .Select(ay => new AspirationTimeConfigDto
                    {
                        AcademicYearId = ay.AcademicYearId,
                        YearName = ay.YearName,
                        AspirationRegistrationStartDate = ay.AspirationRegistrationStartDate,
                        AspirationRegistrationEndDate = ay.AspirationRegistrationEndDate,
                        AspirationEditDeadline = ay.AspirationEditDeadline
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<AspirationTimeConfigDto>>.Success("Lấy danh sách thành công", configs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AspirationTimeConfigDto>>.Error($"Lỗi server: {ex.Message}"));
            }
        }
    }

    public class AspirationTimeConfigDto
    {
        public int AcademicYearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateOnly? AspirationRegistrationStartDate { get; set; }
        public DateOnly? AspirationRegistrationEndDate { get; set; }
        public DateOnly? AspirationEditDeadline { get; set; }
    }

    public class UpdateAspirationTimeConfigDto
    {
        public DateOnly? AspirationRegistrationStartDate { get; set; }
        public DateOnly? AspirationRegistrationEndDate { get; set; }
        public DateOnly? AspirationEditDeadline { get; set; }
    }
}
