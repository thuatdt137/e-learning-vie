using e_learning_vie.DTOs.Aspiration;
using e_learning_vie.Enums;
using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentAspirationService
    {
        Task<List<AspirationItemDto>> GetAspirationsAsync(ClaimsPrincipal user, int? academicYearId = null);
        Task<AspirationItemDto> CreateAspirationAsync(ClaimsPrincipal user, CreateAspirationDto request);
        Task<List<SchoolDto>> GetAvailableSchoolsAsync(SchoolType schoolType);

    }
}
