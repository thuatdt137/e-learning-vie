using e_learning_vie.DTOs.Student;
using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentScheduleService
    {
        Task<StudentScheduleDto> GetCurrentWeekScheduleAsync(ClaimsPrincipal user);
        Task<StudentScheduleDto> GetWeekScheduleAsync(ClaimsPrincipal user, int weekOffset = 0);
        Task<StudentScheduleDto> GetSpecificWeekScheduleAsync(ClaimsPrincipal user, DateTime weekStartDate);
    }
}
