using e_learning_vie.DTOs.TrendsDto;

namespace e_learning_vie.Services.Interfaces
{
    public interface ITrendAnalysisService
    {
        Task<(int fromYear, int toYear)> GetYearRangeAsync(int? startYear, int? endYear, int? yearsBack);
        Task<List<EnrollmentTrendDto>> GetEnrollmentTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null);
        Task<List<AcademicQualityTrendDto>> GetAcademicQualityTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null);
        Task<List<GradePerformanceTrendDto>> GetGradePerformanceTrendAsync(int gradeId, int? startYear = null, int? endYear = null, int? yearsBack = null);
        Task<List<SubjectPerformanceTrendDto>> GetSubjectPerformanceTrendAsync(int subjectId, int? startYear = null, int? endYear = null, int? yearsBack = null);
    }
}