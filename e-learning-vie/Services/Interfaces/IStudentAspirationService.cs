using e_learning_vie.DTOs.Aspiration;
using e_learning_vie.Enums;
using e_learning_vie.Models;
using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentAspirationService
    {
        Task<List<AspirationItemDto>> GetAspirationsAsync(ClaimsPrincipal user, int? academicYearId);
        Task<AspirationDto> CreateAspirationAsync(ClaimsPrincipal user, CreateAspirationDto dto);
        Task<AspirationDto> UpdateAspirationAsync(ClaimsPrincipal user, int aspirationId, CreateAspirationDto dto);
        Task<bool> DeleteAspirationAsync(ClaimsPrincipal user, int aspirationId);
        Task<List<School>> GetAvailableSchoolsAsync(SchoolType type);
    }
}
