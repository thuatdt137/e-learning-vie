using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IUserContextService
    {
        public Task<int?> GetCurrentStudentIdAsync();
        public Task<int?> GetCurrentStudentIdAsync(ClaimsPrincipal user);
        public Task<int?> GetCurrentUserIdAsync();
    }
}
