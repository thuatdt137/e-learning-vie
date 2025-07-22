using e_learning_vie.DTOs.Student;
using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentScheduleService
    {
        // Main API - Lấy lịch theo năm và tuần
        Task<StudentScheduleDto> GetScheduleAsync(ClaimsPrincipal user, int? year = null, int? weekNumber = null);

        // Utility APIs - Lấy danh sách để chọn
        Task<List<YearOption>> GetAvailableYearsAsync(ClaimsPrincipal user);
        Task<List<WeekOption>> GetAvailableWeeksAsync(ClaimsPrincipal user, int year);
    }
}
