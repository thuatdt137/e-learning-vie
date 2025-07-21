using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace e_learning_vie.Services.Implements
{
    public class UserContextService : IUserContextService
    {
        private readonly SchoolManagementContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(SchoolManagementContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> GetCurrentStudentIdAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null) return null;

            var userEntity = await _context.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return userEntity?.StudentId;
        }

        public async Task<int?> GetCurrentStudentIdAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var userEntity = await _context.Users
                    .Include(u => u.Student)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return userEntity?.StudentId;
            }
            return null;
        }

        public Task<int?> GetCurrentUserIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return Task.FromResult<int?>(null);

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return Task.FromResult<int?>(userId);
            }
            return Task.FromResult<int?>(null);
        }
    }
}
