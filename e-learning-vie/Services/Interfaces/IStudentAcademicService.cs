using e_learning_vie.DTOs.ScoreDetail;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentAcademicService
    {
        public StudentScoreDTO GetStudentScores(int studentId, int semesterId);
        public dynamic GetClassScores(int classId, int semesterId);

        public dynamic GetClassAcademicLevel(int classId, int semesterId);
    }
}
