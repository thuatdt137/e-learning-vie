using e_learning_vie.ModelsDTO.Student;
using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentScheduleService
    {
        Task<StudentScheduleDto> GetCurrentWeekScheduleAsync(ClaimsPrincipal user);
    }
}
