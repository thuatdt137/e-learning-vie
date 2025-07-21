using e_learning_vie.Commons;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        /// Lấy lịch học của tuần hiện tại
        /// </summary>
        [HttpGet("current-week")]
        public async Task<IActionResult> GetCurrentWeekSchedule()
        {
            try
            {
                var schedule = await _scheduleService.GetCurrentWeekScheduleAsync(User);
                return Ok(ApiResponse<object>.Success("Lấy lịch học tuần hiện tại thành công", schedule));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch học theo tuần (với offset từ tuần hiện tại)
        /// </summary>
        /// <param name="weekOffset">Offset từ tuần hiện tại (0: tuần hiện tại, 1: tuần sau, -1: tuần trước)</param>
        [HttpGet("week")]
        public async Task<IActionResult> GetWeekSchedule([FromQuery] int weekOffset = 0)
        {
            try
            {
                var schedule = await _scheduleService.GetWeekScheduleAsync(User, weekOffset);
                return Ok(ApiResponse<object>.Success("Lấy lịch học thành công", schedule));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch học tuần trước
        /// </summary>
        [HttpGet("previous-week")]
        public async Task<IActionResult> GetPreviousWeekSchedule()
        {
            try
            {
                var schedule = await _scheduleService.GetWeekScheduleAsync(User, -1);
                return Ok(ApiResponse<object>.Success("Lấy lịch học tuần trước thành công", schedule));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch học tuần sau
        /// </summary>
        [HttpGet("next-week")]
        public async Task<IActionResult> GetNextWeekSchedule()
        {
            try
            {
                var schedule = await _scheduleService.GetWeekScheduleAsync(User, 1);
                return Ok(ApiResponse<object>.Success("Lấy lịch học tuần sau thành công", schedule));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy lịch học theo ngày cụ thể (tự động tính tuần chứa ngày đó)
        /// </summary>
        /// <param name="date">Ngày cần xem lịch (YYYY-MM-DD)</param>
        [HttpGet("by-date")]
        public async Task<IActionResult> GetScheduleByDate([FromQuery] DateTime date)
        {
            try
            {
                var schedule = await _scheduleService.GetSpecificWeekScheduleAsync(User, date);
                return Ok(ApiResponse<object>.Success("Lấy lịch học theo ngày thành công", schedule));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
