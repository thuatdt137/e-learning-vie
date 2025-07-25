using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;

namespace e_learning_vie.Services.Implements
{
    public class StudentAcademicService : IStudentAcademicService
    {
        private readonly SchoolManagementContext _context;
        public StudentAcademicService(SchoolManagementContext context)
        {
            _context = context;
        }
        public dynamic GetStudentScores(int studentId, int semesterId)
        {
            throw new NotImplementedException("This method is not implemented yet.");
        }
    }
}
