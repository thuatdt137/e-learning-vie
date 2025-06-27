using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning_vie.Services.Implements
{
    public class UserContextService : IUserContextService
    {
        private readonly SchoolManagementContext _context;

        public UserContextService(SchoolManagementContext context)
        {
            _context = context;
        }

        public async Task<int?> GetCurrentStudentIdAsync(ClaimsPrincipal user)
        {
            var userId = await GetCurrentUserIdAsync(user);
            if (userId == null) return null;

            var userEntity = await _context.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return userEntity?.StudentId;
        }

        public Task<int?> GetCurrentUserIdAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return Task.FromResult<int?>(userId);
            }
            return Task.FromResult<int?>(null);
        }
    }
}
