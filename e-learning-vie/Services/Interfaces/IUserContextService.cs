using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IUserContextService
    {
        Task<int?> GetCurrentStudentIdAsync(ClaimsPrincipal user);
        Task<int?> GetCurrentUserIdAsync(ClaimsPrincipal user);
    }
}
