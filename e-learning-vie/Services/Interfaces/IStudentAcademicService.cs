namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentAcademicService
    {
        public dynamic GetStudentScores(int studentId, int semesterId);
    }
}
