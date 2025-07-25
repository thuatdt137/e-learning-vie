using e_learning_vie.DTOs.TrendsDto;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;

namespace e_learning_vie.Services.Implements
{
    public class TrendAnalysisService : ITrendAnalysisService
    {
        private readonly SchoolManagementContext _context;

        public TrendAnalysisService(SchoolManagementContext context)
        {
            _context = context;
        }

        public Task<List<AcademicQualityTrendDto>> GetAcademicQualityTrendAsync(int yearsBack = 3)
        {
            throw new NotImplementedException();
        }

        public Task<List<EnrollmentTrendDto>> GetEnrollmentTrendAsync(int yearsBack = 5)
        {
            var currentYear = DateTime.Now.Year;
            var startYear = currentYear - yearsBack;
        }

        public Task<List<GradePerformanceTrendDto>> GetGradePerformanceTrendAsync(int gradeId, int yearsBack = 3)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOverallTrendSummaryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<SubjectPerformanceTrendDto>> GetSubjectPerformanceTrendAsync(int subjectId, int yearsBack = 3)
        {
            throw new NotImplementedException();
        }
    }
}