namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentScoreService
    {
        dynamic GetYearlyAverageScores(string userId, int academicYearId);

        dynamic GetYearlyListAverageScores(string userId);

        Task<dynamic> GetStudentScoresInYear(string userId, int academicYearId);
    }
}
