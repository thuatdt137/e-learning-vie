using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace e_learning_vie.Services.Implements
{
    public class StudentScoreService : IStudentScoreService
    {
        private readonly UserManager<User> _userManager;
        private readonly SchoolManagementContext _context;

        public StudentScoreService(UserManager<User> userManager, SchoolManagementContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<dynamic> GetStudentScoresInYear(string userId, int academicYearId)
        {

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var studentId = user.StudentId;

            var scores = _context.StudentScores

        .ToList();

            if (scores == null || !scores.Any())
            {
                throw new KeyNotFoundException($"Student with ID {studentId} does not have any subject scores in academic year {academicYearId}.");
            }

            return scores;


        }

        public dynamic GetYearlyAverageScores(string userId, int academicYearId)
        {
            throw new NotImplementedException();
        }

        public dynamic GetYearlyListAverageScores(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
