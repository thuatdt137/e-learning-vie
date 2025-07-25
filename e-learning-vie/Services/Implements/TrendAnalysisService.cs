using e_learning_vie.DTOs.TrendsDto;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Services.Implements
{
    public class TrendAnalysisService : ITrendAnalysisService
    {
        private readonly SchoolManagementContext _context;

        public TrendAnalysisService(SchoolManagementContext context)
        {
            _context = context;
        }
        public async Task<(int fromYear, int toYear)> GetYearRangeAsync(int? startYear, int? endYear, int? yearsBack)
        {
            var validYears = await _context.AcademicYears
                .Where(ay => ay.StartDate.HasValue)
                .OrderBy(ay => ay.StartDate)
                .Select(ay => ay.StartDate.Value.Year)
                .ToListAsync();

            if (validYears.Count < 2)
                throw new InvalidOperationException("Không đủ năm học để phân tích.");

            int minYear = validYears.First();
            int maxYear = validYears.Last();

            // Trường hợp 1: startYear + endYear
            if (startYear.HasValue && endYear.HasValue)
                return (startYear.Value, endYear.Value);

            // Trường hợp 2: startYear + yearsBack
            if (startYear.HasValue && yearsBack.HasValue)
                return (startYear.Value, Math.Min(startYear.Value + yearsBack.Value - 1, maxYear));

            // Trường hợp 3: endYear + yearsBack
            if (endYear.HasValue && yearsBack.HasValue)
                return (Math.Max(endYear.Value - yearsBack.Value + 1, minYear), endYear.Value);

            // Trường hợp 4: chỉ yearsBack
            if (yearsBack.HasValue)
                return (Math.Max(maxYear - yearsBack.Value + 1, minYear), maxYear);

            // Mặc định lấy 2 năm gần nhất
            int defaultBack = Math.Min(2, validYears.Count);
            return (maxYear - defaultBack + 1, maxYear);
        }

        public async Task<List<AcademicQualityTrendDto>> GetAcademicQualityTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

            var semesters = await _context.Semesters
                .Include(s => s.AcademicYear)
                .Where(s => s.AcademicYear.StartDate.HasValue &&
                            s.AcademicYear.StartDate.Value.Year >= fromYear &&
                            s.AcademicYear.StartDate.Value.Year <= toYear)
                .OrderBy(s => s.AcademicYear.StartDate)
                .ToListAsync();

            var trends = new List<AcademicQualityTrendDto>();

            foreach (var s in semesters)
            {
                var scores = s.ClassSessions
                    .SelectMany(cs => cs.Enrollments)
                    .SelectMany(e => e.StudentScores)
                    .Where(ss => ss.Score.HasValue)
                    .Select(ss => ss.Score.Value)
                    .ToList();

                if (!scores.Any()) continue;

                double avgScore = Math.Round(scores.Average(), 2);
                double excellentRate = Math.Round((double)scores.Count(x => x >= 8.0) / scores.Count * 100, 2);
                double goodRate = Math.Round((double)scores.Count(x => x >= 6.5 && x < 8.0) / scores.Count * 100, 2);
                double averageRate = Math.Round((double)scores.Count(x => x >= 5.0 && x < 6.5) / scores.Count * 100, 2);
                double belowAverageRate = Math.Round((double)scores.Count(x => x < 5.0) / scores.Count * 100, 2);

                trends.Add(new AcademicQualityTrendDto
                {
                    AcademicYear = s.AcademicYear.YearName,
                    Semester = s.SemesterName,
                    OverallAverageScore = avgScore,
                    ExcellentRate = excellentRate,
                    GoodRate = goodRate,
                    AverageRate = averageRate,
                    BelowAverageRate = belowAverageRate,
                    QualityIndex = Math.Round(avgScore * 10 + excellentRate * 0.2, 2),
                    ChangeFromPreviousSemester = 0 // Có thể tính thêm nếu cần
                });
            }

            return trends;
        }

        public async Task<List<EnrollmentTrendDto>> GetEnrollmentTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);
            var academicsYear = await _context.AcademicYears
                                .Where(ay => ay.StartDate.HasValue && ay.StartDate.Value.Year >= fromYear && ay.StartDate.Value.Year <= toYear)
                                .OrderBy(ay => ay.StartDate)
                                .ToListAsync();
            var trends = new List<EnrollmentTrendDto>();
            for (int i = 0; i < academicsYear.Count; i++)
            {
                var aY = academicsYear[i];
                var studentsId = aY.Semesters.SelectMany(s => s.ClassSessions)
                    .SelectMany(cs => cs.Enrollments)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToList();
                var gradeCounts = aY.Semesters.SelectMany(s => s.ClassSessions)
                    .SelectMany(cs => cs.Enrollments)
                    .GroupBy(e => e.ClassSession.Class.Grade.GradeName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.StudentId).Distinct().Count());

                int totalStudents = studentsId.Count;
                int previousTotal = i > 0 ? trends[i - 1].TotalStudents : totalStudents;
                int changeAmount = totalStudents - previousTotal;
                double changePercentage = previousTotal > 0 ? (double)changeAmount / previousTotal * 100 : 0;

                trends.Add(new EnrollmentTrendDto
                {
                    AcademicYear = aY.YearName,
                    TotalStudents = totalStudents,
                    Grade6Students = gradeCounts.ContainsKey("6") ? gradeCounts["6"] : 0,
                    Grade7Students = gradeCounts.ContainsKey("7") ? gradeCounts["7"] : 0,
                    Grade8Students = gradeCounts.ContainsKey("8") ? gradeCounts["8"] : 0,
                    Grade9Students = gradeCounts.ContainsKey("9") ? gradeCounts["9"] : 0,
                    ChangeAmount = changeAmount,
                    ChangePercentage = Math.Round(changePercentage, 2)
                });
            }
            return trends;
        }

        public async Task<List<GradePerformanceTrendDto>> GetGradePerformanceTrendAsync(int gradeId, int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

            var enrollments = await _context.Enrollments
                .Include(e => e.ClassSession)
                    .ThenInclude(cs => cs.Class)
                .Include(e => e.ClassSession)
                    .ThenInclude(cs => cs.Semester)
                        .ThenInclude(s => s.AcademicYear)
                .Include(e => e.StudentScores)
                .Where(e => e.ClassSession.Class.GradeId == gradeId &&
                            e.ClassSession.Semester.AcademicYear.StartDate.HasValue &&
                            e.ClassSession.Semester.AcademicYear.StartDate.Value.Year >= fromYear &&
                            e.ClassSession.Semester.AcademicYear.StartDate.Value.Year <= toYear)
                .ToListAsync();

            var trends = enrollments
                .GroupBy(e => e.ClassSession.Semester.AcademicYear.YearName)
                .Select(g =>
                {
                    var scores = g.SelectMany(e => e.StudentScores)
                                  .Where(ss => ss.Score.HasValue)
                                  .Select(ss => ss.Score.Value)
                                  .ToList();

                    return new GradePerformanceTrendDto
                    {
                        GradeId = gradeId,
                        GradeName = g.First().ClassSession.Class.Grade.GradeName,
                        AcademicYear = g.Key,
                        AverageScore = scores.Any() ? Math.Round(scores.Average(), 2) : 0,
                        StudentCount = g.Select(e => e.StudentId).Distinct().Count(),
                        ExcellentRate = scores.Any() ? Math.Round((double)scores.Count(s => s >= 8.0) / scores.Count * 100, 2) : 0,
                        PassRate = scores.Any() ? Math.Round((double)scores.Count(s => s >= 5.0) / scores.Count * 100, 2) : 0
                    };
                })
                .OrderBy(t => t.AcademicYear)
                .ToList();

            return trends;
        }

        public async Task<object> GetOverallTrendSummaryAsync(int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var enrollmentTrend = await GetEnrollmentTrendAsync(startYear, endYear, yearsBack);
            var qualityTrend = await GetAcademicQualityTrendAsync(startYear, endYear, yearsBack);

            return new
            {
                EnrollmentTrend = enrollmentTrend,
                QualityTrend = qualityTrend
            };
        }

        public async Task<List<SubjectPerformanceTrendDto>> GetSubjectPerformanceTrendAsync(int subjectId, int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

            var scores = await _context.StudentScores
                .Include(ss => ss.Subject)
                .Include(ss => ss.Enrollment)
                    .ThenInclude(e => e.ClassSession)
                        .ThenInclude(cs => cs.Semester)
                            .ThenInclude(s => s.AcademicYear)
                .Where(ss => ss.SubjectId == subjectId &&
                             ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.HasValue &&
                             ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year >= fromYear &&
                             ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year <= toYear &&
                             ss.Score.HasValue)
                .ToListAsync();

            var trends = scores
                .GroupBy(ss => ss.Enrollment.ClassSession.Semester.AcademicYear.YearName)
                .Select(g =>
                {
                    var scoreList = g.Select(x => x.Score.Value).ToList();
                    return new SubjectPerformanceTrendDto
                    {
                        SubjectId = subjectId,
                        SubjectName = g.First().Subject.SubjectName,
                        AcademicYear = g.Key,
                        AverageScore = scoreList.Any() ? Math.Round(scoreList.Average(), 2) : 0,
                        PassRate = scoreList.Any() ? Math.Round((double)scoreList.Count(s => s >= 5.0) / scoreList.Count * 100, 2) : 0,
                        ExcellentRate = scoreList.Any() ? Math.Round((double)scoreList.Count(s => s >= 8.0) / scoreList.Count * 100, 2) : 0,
                        StudentCount = g.Select(x => x.Enrollment.StudentId).Distinct().Count(),
                        DifficultyIndex = scoreList.Any() ? Math.Round(100 - ((double)scoreList.Count(s => s >= 5.0) / scoreList.Count * 100), 2) : 0
                    };
                })
                .OrderBy(t => t.AcademicYear)
                .ToList();

            return trends;
        }
    }
}