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

            // Sử dụng truy vấn trực tiếp để tránh include dư thừa
            var qualityData = await _context.StudentScores
                .Where(ss => ss.Score.HasValue &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.HasValue &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year >= fromYear &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year <= toYear)
                .Select(ss => new
                {
                    AcademicYear = ss.Enrollment.ClassSession.Semester.AcademicYear.YearName,
                    Semester = ss.Enrollment.ClassSession.Semester.SemesterName,
                    Score = ss.Score.Value,
                    SemesterId = ss.Enrollment.ClassSession.Semester.SemesterId
                })
                .ToListAsync();

            var trends = qualityData
                .GroupBy(qd => new { qd.AcademicYear, qd.Semester, qd.SemesterId })
                .Select(g =>
                {
                    var scores = g.Select(x => x.Score).ToList();
                    var avgScore = scores.Average();
                    var excellentCount = scores.Count(s => s >= 8.0);
                    var goodCount = scores.Count(s => s >= 6.5 && s < 8.0);
                    var averageCount = scores.Count(s => s >= 5.0 && s < 6.5);
                    var belowAverageCount = scores.Count(s => s < 5.0);
                    var total = scores.Count;

                    return new AcademicQualityTrendDto
                    {
                        AcademicYear = g.Key.AcademicYear,
                        Semester = g.Key.Semester,
                        OverallAverageScore = Math.Round(avgScore, 2),
                        ExcellentRate = Math.Round((double)excellentCount / total * 100, 2),
                        GoodRate = Math.Round((double)goodCount / total * 100, 2),
                        AverageRate = Math.Round((double)averageCount / total * 100, 2),
                        BelowAverageRate = Math.Round((double)belowAverageCount / total * 100, 2),
                        QualityIndex = Math.Round(avgScore * 10 + (double)excellentCount / total * 20, 2),
                        ChangeFromPreviousSemester = 0 // Tính sau nếu cần
                    };
                })
                .OrderBy(t => t.AcademicYear)
                .ThenBy(t => t.Semester)
                .ToList();

            // Tính change từ semester trước
            for (int i = 1; i < trends.Count; i++)
            {
                var current = trends[i];
                var previous = trends[i - 1];
                current.ChangeFromPreviousSemester = Math.Round(
                    current.QualityIndex - previous.QualityIndex, 2);
            }

            return trends;
        }

        public async Task<List<EnrollmentTrendDto>> GetEnrollmentTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

            // Truy vấn trực tiếp để lấy dữ liệu enrollment
            var enrollmentData = await _context.Enrollments
                .Where(e => e.ClassSession.Semester.AcademicYear.StartDate.HasValue &&
                           e.ClassSession.Semester.AcademicYear.StartDate.Value.Year >= fromYear &&
                           e.ClassSession.Semester.AcademicYear.StartDate.Value.Year <= toYear)
                .Select(e => new
                {
                    AcademicYear = e.ClassSession.Semester.AcademicYear.YearName,
                    Year = e.ClassSession.Semester.AcademicYear.StartDate.Value.Year,
                    StudentId = e.StudentId,
                    GradeName = e.ClassSession.Class.Grade.GradeName
                })
                .ToListAsync();

            var yearlyData = enrollmentData
                .GroupBy(e => new { e.AcademicYear, e.Year })
                .OrderBy(g => g.Key.Year)
                .ToList();

            var trends = new List<EnrollmentTrendDto>();

            for (int i = 0; i < yearlyData.Count; i++)
            {
                var yearData = yearlyData[i];
                var uniqueStudents = yearData.Select(e => e.StudentId).Distinct().ToList();
                
                var gradeCounts = yearData
                    .GroupBy(e => e.GradeName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.StudentId).Distinct().Count());

                int totalStudents = uniqueStudents.Count;
                int previousTotal = i > 0 ? trends[i - 1].TotalStudents : totalStudents;
                int changeAmount = totalStudents - previousTotal;
                double changePercentage = previousTotal > 0 ? (double)changeAmount / previousTotal * 100 : 0;

                trends.Add(new EnrollmentTrendDto
                {
                    AcademicYear = yearData.Key.AcademicYear,
                    TotalStudents = totalStudents,
                    Grade6Students = GetGradeCount(gradeCounts, "6"),
                    Grade7Students = GetGradeCount(gradeCounts, "7"),
                    Grade8Students = GetGradeCount(gradeCounts, "8"),
                    Grade9Students = GetGradeCount(gradeCounts, "9"),
                    ChangeAmount = changeAmount,
                    ChangePercentage = Math.Round(changePercentage, 2)
                });
            }

            return trends;
        }

        public async Task<List<GradePerformanceTrendDto>> GetGradePerformanceTrendAsync(int gradeId, int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

            var gradeData = await _context.StudentScores
                .Where(ss => ss.Enrollment.ClassSession.Class.GradeId == gradeId &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.HasValue &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year >= fromYear &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year <= toYear &&
                            ss.Score.HasValue)
                .Select(ss => new
                {
                    AcademicYear = ss.Enrollment.ClassSession.Semester.AcademicYear.YearName,
                    GradeName = ss.Enrollment.ClassSession.Class.Grade.GradeName,
                    StudentId = ss.Enrollment.StudentId,
                    Score = ss.Score.Value
                })
                .ToListAsync();

            var trends = gradeData
                .GroupBy(g => g.AcademicYear)
                .Select(g =>
                {
                    var scores = g.Select(x => x.Score).ToList();
                    
                    return new GradePerformanceTrendDto
                    {
                        GradeId = gradeId,
                        GradeName = g.First().GradeName,
                        AcademicYear = g.Key,
                        AverageScore = scores.Any() ? Math.Round(scores.Average(), 2) : 0,
                        StudentCount = g.Select(x => x.StudentId).Distinct().Count(),
                        ExcellentRate = scores.Any() ? Math.Round((double)scores.Count(s => s >= 8.0) / scores.Count * 100, 2) : 0,
                        PassRate = scores.Any() ? Math.Round((double)scores.Count(s => s >= 5.0) / scores.Count * 100, 2) : 0
                    };
                })
                .OrderBy(t => t.AcademicYear)
                .ToList();

            return trends;
        }

        public async Task<List<SubjectPerformanceTrendDto>> GetSubjectPerformanceTrendAsync(int subjectId, int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

            // Sử dụng SubjectId thay vì SubjectScoreId
            var subjectData = await _context.StudentScores
                .Where(ss => ss.SubjectScore != null &&
                            ss.SubjectScore.Subject != null &&
                            ss.SubjectScore.Subject.SubjectId == subjectId &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.HasValue &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year >= fromYear &&
                            ss.Enrollment.ClassSession.Semester.AcademicYear.StartDate.Value.Year <= toYear &&
                            ss.Score.HasValue)
                .Select(ss => new
                {
                    AcademicYear = ss.Enrollment.ClassSession.Semester.AcademicYear.YearName,
                    SubjectName = ss.SubjectScore.Subject.SubjectName,
                    StudentId = ss.Enrollment.StudentId,
                    Score = ss.Score.Value
                })
                .ToListAsync();

            var trends = subjectData
                .GroupBy(s => s.AcademicYear)
                .Select(g =>
                {
                    var scoreList = g.Select(x => x.Score).ToList();
                    return new SubjectPerformanceTrendDto
                    {
                        SubjectId = subjectId,
                        SubjectName = g.First().SubjectName,
                        AcademicYear = g.Key,
                        AverageScore = scoreList.Any() ? Math.Round(scoreList.Average(), 2) : 0,
                        PassRate = scoreList.Any() ? Math.Round((double)scoreList.Count(s => s >= 5.0) / scoreList.Count * 100, 2) : 0,
                        ExcellentRate = scoreList.Any() ? Math.Round((double)scoreList.Count(s => s >= 8.0) / scoreList.Count * 100, 2) : 0,
                        StudentCount = g.Select(x => x.StudentId).Distinct().Count(),
                        DifficultyIndex = scoreList.Any() ? Math.Round(100 - ((double)scoreList.Count(s => s >= 5.0) / scoreList.Count * 100), 2) : 0
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
                EnrollmentTrend = new
                {
                    CurrentTotal = enrollmentTrend.LastOrDefault()?.TotalStudents ?? 0,
                    YearOverYearChange = enrollmentTrend.LastOrDefault()?.ChangePercentage ?? 0,
                    TrendData = enrollmentTrend
                },
                QualityTrend = new
                {
                    CurrentQualityIndex = qualityTrend.LastOrDefault()?.QualityIndex ?? 0,
                    RecentChange = qualityTrend.LastOrDefault()?.ChangeFromPreviousSemester ?? 0,
                    TrendData = qualityTrend
                }
            };
        }

        private int GetGradeCount(Dictionary<string, int> gradeCounts, string gradeNumber)
        {
            // Tìm key chứa số khối (ví dụ: "Khối 6", "6", "Grade 6", etc.)
            var key = gradeCounts.Keys.FirstOrDefault(k => k.Contains(gradeNumber));
            return key != null ? gradeCounts[key] : 0;
        }
    }
}