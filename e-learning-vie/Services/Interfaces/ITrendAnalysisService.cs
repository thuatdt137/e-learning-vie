using e_learning_vie.DTOs.TrendsDto;

namespace e_learning_vie.Services.Interfaces
{
    public interface ITrendAnalysisService
    {
         Task<List<EnrollmentTrendDto>> GetEnrollmentTrendAsync(int yearsBack = 5);
        Task<List<AcademicQualityTrendDto>> GetAcademicQualityTrendAsync(int yearsBack = 3);
        Task<List<GradePerformanceTrendDto>> GetGradePerformanceTrendAsync(int gradeId, int yearsBack = 3);
        Task<List<SubjectPerformanceTrendDto>> GetSubjectPerformanceTrendAsync(int subjectId, int yearsBack = 3);
        Task<object> GetOverallTrendSummaryAsync();

    }
}
