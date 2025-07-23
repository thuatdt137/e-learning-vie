using e_learning_vie.Commons;
using e_learning_vie.DTOs.Student;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [ApiController]
    [Route("api/students/schedule")]
    [Authorize(Roles = "Student")]
    public class StudentScheduleController : ControllerBase
    {
        private readonly IStudentScheduleService _scheduleService;

        public StudentScheduleController(IStudentScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Lấy lịch học (mặc định là tuần hiện tại)
        /// </summary>
        /// <param name="year">Năm (mặc định: năm hiện tại)</param>
        /// <param name="weekNumber">Số tuần trong năm (mặc định: tuần hiện tại)</param>
        [HttpGet]
        public async Task<IActionResult> GetSchedule([FromQuery] int? year = null, [FromQuery] int? weekNumber = null)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleAsync(User, year, weekNumber);
                return Ok(ApiResponse<StudentScheduleDto>.Success("Lấy lịch học thành công", schedule));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách năm có thể chọn
        /// </summary>
        [HttpGet("years")]
        public async Task<IActionResult> GetAvailableYears()
        {
            try
            {
                var years = await _scheduleService.GetAvailableYearsAsync(User);
                return Ok(ApiResponse<List<YearOption>>.Success("Lấy danh sách năm thành công", years));
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách tuần trong năm có thể chọn
        /// </summary>
        /// <param name="year">Năm cần lấy danh sách tuần</param>
        [HttpGet("weeks")]
        public async Task<IActionResult> GetAvailableWeeks([FromQuery] int year)
        {
            try
            {
                var weeks = await _scheduleService.GetAvailableWeeksAsync(User, year);
                return Ok(ApiResponse<List<WeekOption>>.Success($"Lấy danh sách tuần năm {year} thành công", weeks));
            }
            catch(ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
