using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentScheduleController : ControllerBase
    {
        private readonly IStudentScheduleService _scheduleService;

        public StudentScheduleController(IStudentScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("ScheduleOfWeek")]
        public async Task<IActionResult> GetCurrentWeekSchedule()
        {
            try
            {
                var schedule = await _scheduleService.GetCurrentWeekScheduleAsync(User);
                return Ok(schedule);
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
    }
}
