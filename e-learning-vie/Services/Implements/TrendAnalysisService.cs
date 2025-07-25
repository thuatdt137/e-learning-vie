using e_learning_vie.DTOs.AcademicLevel;
using e_learning_vie.DTOs.TrendsDto;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace e_learning_vie.Services.Implements
{
    public class TrendAnalysisService : ITrendAnalysisService
    {
        private readonly SchoolManagementContext _context;
        private readonly AcademicLevelRulesConfig _academicRules;

        public TrendAnalysisService(
            SchoolManagementContext context,
            IOptions<AcademicLevelRulesConfig> academicRules)
        {
            _context = context;
            _academicRules = academicRules.Value;
        }

        public async Task<(int fromYear, int toYear)> GetYearRangeAsync(int? startYear, int? endYear, int? yearsBack)
        {
            // Lấy tất cả các năm học có dữ liệu từ database
            var validYears = await _context.AcademicYears
                .Where(ay => ay.StartDate.HasValue)
                .OrderBy(ay => ay.StartDate)
                .Select(ay => ay.StartDate.Value.Year)
                .Distinct()
                .ToListAsync();

            if (!validYears.Any())
                throw new InvalidOperationException("Không có năm học nào trong hệ thống.");

            // Nếu chỉ có 1 năm học
            if (validYears.Count < 2)
            {
                return (validYears.First(), validYears.First());
            }

            int minYear = validYears.First();
            int maxYear = validYears.Last();
            int totalAvailableYears = validYears.Count;

            // Xử lý trường hợp có cả startYear và endYear
            if (startYear.HasValue && endYear.HasValue)
            {
                var validStartYear = validYears.Contains(startYear.Value) ? startYear.Value : minYear;
                var validEndYear = validYears.Contains(endYear.Value) ? endYear.Value : maxYear;

                // Đảm bảo startYear <= endYear
                var finalStartYear = Math.Min(validStartYear, validEndYear);
                var finalEndYear = Math.Max(validStartYear, validEndYear);

                return (finalStartYear, finalEndYear);
            }

            // Xử lý trường hợp có startYear và yearsBack
            if (startYear.HasValue && yearsBack.HasValue)
            {
                var validStartYear = validYears.Contains(startYear.Value) ? startYear.Value : minYear;

                // Đảm bảo yearsBack tối thiểu là 2 và không vượt quá số năm có sẵn
                var actualYearsBack = Math.Max(2, Math.Min(yearsBack.Value, totalAvailableYears));

                // Tính toán endYear dựa trên startYear + yearsBack
                var calculatedEndYear = validStartYear + actualYearsBack - 1;
                var finalEndYear = Math.Min(calculatedEndYear, maxYear);

                return (validStartYear, finalEndYear);
            }

            // Xử lý trường hợp có endYear và yearsBack
            if (endYear.HasValue && yearsBack.HasValue)
            {
                var validEndYear = validYears.Contains(endYear.Value) ? endYear.Value : maxYear;

                // Đảm bảo yearsBack tối thiểu là 2 và không vượt quá số năm có sẵn
                var actualYearsBack = Math.Max(2, Math.Min(yearsBack.Value, totalAvailableYears));

                // Tính toán startYear dựa trên endYear - yearsBack
                var calculatedStartYear = validEndYear - actualYearsBack + 1;
                var finalStartYear = Math.Max(calculatedStartYear, minYear);

                return (finalStartYear, validEndYear);
            }

            // Xử lý trường hợp chỉ có yearsBack
            if (yearsBack.HasValue)
            {
                // Đảm bảo yearsBack tối thiểu là 2 và không vượt quá số năm có sẵn
                var actualYearsBack = Math.Max(2, Math.Min(yearsBack.Value, totalAvailableYears));

                // Lấy từ năm học lớn nhất trở về
                var calculatedStartYear = maxYear - actualYearsBack + 1;
                var finalStartYear = Math.Max(calculatedStartYear, minYear);

                return (finalStartYear, maxYear);
            }

            // Xử lý trường hợp chỉ có startYear
            if (startYear.HasValue)
            {
                var validStartYear = validYears.Contains(startYear.Value) ? startYear.Value : minYear;
                return (validStartYear, maxYear);
            }

            // Xử lý trường hợp chỉ có endYear
            if (endYear.HasValue)
            {
                var validEndYear = validYears.Contains(endYear.Value) ? endYear.Value : maxYear;
                return (minYear, validEndYear);
            }

            // Trường hợp mặc định: trả về 2 năm gần nhất (năm lớn nhất và năm lớn nhì)
            if (totalAvailableYears >= 2)
            {
                var secondLatestYear = validYears[totalAvailableYears - 2]; // Năm lớn nhì
                return (secondLatestYear, maxYear); // Từ năm lớn nhì đến năm lớn nhất
            }

            // Nếu chỉ có 1 năm thì trả về năm đó
            return (maxYear, maxYear);
        }

        public async Task<List<EnrollmentTrendDto>> GetEnrollmentTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            try
            {
                var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

                var academicYears = await _context.AcademicYears
                    .Where(ay => ay.StartDate.HasValue &&
                                ay.StartDate.Value.Year >= fromYear &&
                                ay.StartDate.Value.Year <= toYear)
                    .OrderBy(ay => ay.StartDate)
                    .ToListAsync();

                if (!academicYears.Any())
                {
                    return new List<EnrollmentTrendDto>();
                }

                var trends = new List<EnrollmentTrendDto>();

                foreach (var academicYear in academicYears)
                {
                    var enrollments = await _context.Enrollments
                        .Include(e => e.ClassSession)
                            .ThenInclude(cs => cs.Semester)
                        .Include(e => e.ClassSession)
                            .ThenInclude(cs => cs.Class)
                                .ThenInclude(c => c.Grade)
                        .Where(e => e.ClassSession.Semester.AcademicYearId == academicYear.AcademicYearId)
                        .ToListAsync();

                    var uniqueStudentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();

                    var gradeGroups = enrollments
                        .GroupBy(e => e.ClassSession.Class.Grade.GradeName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.StudentId).Distinct().Count());

                    int totalStudents = uniqueStudentIds.Count;
                    int previousTotal = trends.LastOrDefault()?.TotalStudents ?? totalStudents;
                    int changeAmount = totalStudents - previousTotal;
                    double changePercentage = previousTotal > 0 ? (double)changeAmount / previousTotal * 100 : 0;

                    trends.Add(new EnrollmentTrendDto
                    {
                        AcademicYear = academicYear.YearName,
                        TotalStudents = totalStudents,
                        Grade6Students = GetGradeCount(gradeGroups, "6"),
                        Grade7Students = GetGradeCount(gradeGroups, "7"),
                        Grade8Students = GetGradeCount(gradeGroups, "8"),
                        Grade9Students = GetGradeCount(gradeGroups, "9"),
                        ChangeAmount = changeAmount,
                        ChangePercentage = Math.Round(changePercentage, 2)
                    });
                }

                return trends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEnrollmentTrendAsync: {ex.Message}");
                return new List<EnrollmentTrendDto>();
            }
        }

        public async Task<List<AcademicQualityTrendDto>> GetAcademicQualityTrendAsync(int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            try
            {
                var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

                Console.WriteLine($"GetAcademicQualityTrendAsync: fromYear={fromYear}, toYear={toYear}");

                var semesters = await _context.Semesters
                    .Include(s => s.AcademicYear)
                    .Where(s => s.AcademicYear.StartDate.HasValue &&
                               s.AcademicYear.StartDate.Value.Year >= fromYear &&
                               s.AcademicYear.StartDate.Value.Year <= toYear)
                    .OrderBy(s => s.AcademicYear.StartDate)
                    .ThenBy(s => s.SemesterName)
                    .ToListAsync();

                Console.WriteLine($"Tìm thấy {semesters.Count} semesters từ {fromYear} đến {toYear}");

                var trends = new List<AcademicQualityTrendDto>();

                foreach (var semester in semesters)
                {
                    Console.WriteLine($"Xử lý semester: {semester.SemesterName} - {semester.AcademicYear.YearName}");

                    // Tận dụng logic từ StudentAcademicService - lấy điểm theo học sinh
                    var studentAverages = await _context.StudentScores
                        .Include(ss => ss.Enrollment)
                            .ThenInclude(e => e.Student)
                        .Include(ss => ss.SubjectScore)
                            .ThenInclude(ssc => ssc.Subject)
                        .Where(ss => ss.Score.HasValue &&
                                    ss.Enrollment.ClassSession.SemesterId == semester.SemesterId)
                        .GroupBy(ss => ss.Enrollment.StudentId)
                        .Select(g => new
                        {
                            StudentId = g.Key,
                            StudentName = g.First().Enrollment.Student.FirstName + " " + g.First().Enrollment.Student.LastName,
                            Scores = g.Select(ss => new
                            {
                                Subject = ss.SubjectScore.Subject.SubjectName,
                                Score = ss.Score.Value
                            }).ToList(),
                            AverageScore = g.Average(ss => ss.Score.Value),
                            MinScore = g.Min(ss => ss.Score.Value),
                            MaxScore = g.Max(ss => ss.Score.Value),
                            SubjectCount = g.Count()
                        })
                        .ToListAsync();

                    Console.WriteLine($"Semester {semester.SemesterName}: Có {studentAverages.Count} học sinh với điểm");

                    // Debug: In ra thông tin chi tiết từng học sinh
                    foreach (var student in studentAverages)
                    {
                        Console.WriteLine($"- {student.StudentName}: TB={student.AverageScore:F2}, Min={student.MinScore}, Max={student.MaxScore}, Môn={student.SubjectCount}");
                        foreach (var score in student.Scores)
                        {
                            Console.WriteLine($"  + {score.Subject}: {score.Score}");
                        }
                    }

                    if (!studentAverages.Any())
                    {
                        Console.WriteLine($"Bỏ qua semester {semester.SemesterName} vì không có dữ liệu điểm");
                        continue;
                    }

                    // Phân loại theo chuẩn Bộ GD&ĐT (tận dụng config từ appsettings)
                    int excellentCount = studentAverages.Count(s =>
                        s.AverageScore >= _academicRules.Excellent.AverageThreshold &&
                        s.MinScore >= 6.5); // Giỏi: TB >= 8.0 và tất cả môn >= 6.5

                    int goodCount = studentAverages.Count(s =>
                        s.AverageScore >= _academicRules.Good.AverageThreshold &&
                        s.AverageScore < _academicRules.Excellent.AverageThreshold &&
                        s.MinScore >= 5.0); // Khá: 6.5 <= TB < 8.0 và tất cả môn >= 5.0

                    int averageCount = studentAverages.Count(s =>
                        s.AverageScore >= _academicRules.Average.AverageThreshold &&
                        s.AverageScore < _academicRules.Good.AverageThreshold); // Trung bình: 5.0 <= TB < 6.5

                    int belowAverageCount = studentAverages.Count(s =>
                        s.AverageScore < _academicRules.Average.AverageThreshold); // Yếu: TB < 5.0

                    int totalCount = studentAverages.Count;
                    double avgScore = studentAverages.Average(s => s.AverageScore);

                    Console.WriteLine($"Phân loại: Giỏi={excellentCount}, Khá={goodCount}, TB={averageCount}, Yếu={belowAverageCount}, Tổng={totalCount}");

                    // Tính chỉ số chất lượng theo chuẩn Bộ GD&ĐT
                    var qualityIndex = CalculateQualityIndexByMOET(excellentCount, goodCount, averageCount, belowAverageCount, totalCount);

                    trends.Add(new AcademicQualityTrendDto
                    {
                        AcademicYear = semester.AcademicYear.YearName,
                        Semester = semester.SemesterName,
                        OverallAverageScore = Math.Round(avgScore, 2),
                        ExcellentRate = Math.Round((double)excellentCount / totalCount * 100, 2),
                        GoodRate = Math.Round((double)goodCount / totalCount * 100, 2),
                        AverageRate = Math.Round((double)averageCount / totalCount * 100, 2),
                        BelowAverageRate = Math.Round((double)belowAverageCount / totalCount * 100, 2),
                        QualityIndex = qualityIndex,
                        ChangeFromPreviousSemester = 0
                    });

                    Console.WriteLine($"Thêm trend: TB={avgScore:F2}, QualityIndex={qualityIndex:F2}");
                }

                // Tính thay đổi từ kỳ trước
                for (int i = 1; i < trends.Count; i++)
                {
                    trends[i].ChangeFromPreviousSemester = Math.Round(
                        trends[i].QualityIndex - trends[i - 1].QualityIndex, 2);
                }

                Console.WriteLine($"Trả về {trends.Count} quality trend records");
                return trends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAcademicQualityTrendAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new List<AcademicQualityTrendDto>();
            }
        }

        // Hàm tính chỉ số chất lượng theo chuẩn Bộ GD&ĐT
        private double CalculateQualityIndexByMOET(int excellentCount, int goodCount, int averageCount, int belowAverageCount, int totalCount)
        {
            if (totalCount == 0) return 0;

            // Tỷ lệ các loại học lực
            var excellentRate = (double)excellentCount / totalCount;
            var goodRate = (double)goodCount / totalCount;
            var averageRate = (double)averageCount / totalCount;
            var belowAverageRate = (double)belowAverageCount / totalCount;

            // Công thức theo Thông tư 22/2021/TT-BGDĐT
            var qualityIndex =
                excellentRate * 4 +     // Xuất sắc: hệ số 4
                goodRate * 3 +          // Giỏi: hệ số 3  
                averageRate * 2 +       // Khá: hệ số 2
                belowAverageRate * 1;   // Đạt: hệ số 1

            // Nhân với 100 để có chỉ số từ 0-400
            return Math.Round(qualityIndex * 100, 2);
        }

        public async Task<List<GradePerformanceTrendDto>> GetGradePerformanceTrendAsync(int gradeId, int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            try
            {
                var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

                var grade = await _context.Grades.FindAsync(gradeId);
                if (grade == null)
                {
                    return new List<GradePerformanceTrendDto>();
                }

                var academicYears = await _context.AcademicYears
                    .Where(ay => ay.StartDate.HasValue &&
                                ay.StartDate.Value.Year >= fromYear &&
                                ay.StartDate.Value.Year <= toYear)
                    .OrderBy(ay => ay.StartDate)
                    .ToListAsync();

                var trends = new List<GradePerformanceTrendDto>();

                foreach (var academicYear in academicYears)
                {
                    var scores = await _context.StudentScores
                        .Include(ss => ss.Enrollment)
                            .ThenInclude(e => e.ClassSession)
                                .ThenInclude(cs => cs.Class)
                        .Include(ss => ss.Enrollment)
                            .ThenInclude(e => e.ClassSession)
                                .ThenInclude(cs => cs.Semester)
                        .Where(ss => ss.Score.HasValue &&
                                    ss.Enrollment.ClassSession.Class.GradeId == gradeId &&
                                    ss.Enrollment.ClassSession.Semester.AcademicYearId == academicYear.AcademicYearId)
                        .ToListAsync();

                    if (!scores.Any()) continue;

                    var scoreValues = scores.Select(ss => ss.Score.Value).ToList();
                    var studentCount = scores.Select(ss => ss.Enrollment.StudentId).Distinct().Count();

                    trends.Add(new GradePerformanceTrendDto
                    {
                        GradeId = gradeId,
                        GradeName = grade.GradeName,
                        AcademicYear = academicYear.YearName,
                        AverageScore = Math.Round(scoreValues.Average(), 2),
                        StudentCount = studentCount,
                        ExcellentRate = Math.Round((double)scoreValues.Count(s => s >= _academicRules.Excellent.AverageThreshold) / scoreValues.Count * 100, 2),
                        PassRate = Math.Round((double)scoreValues.Count(s => s >= _academicRules.Average.AverageThreshold) / scoreValues.Count * 100, 2)
                    });
                }

                return trends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGradePerformanceTrendAsync: {ex.Message}");
                return new List<GradePerformanceTrendDto>();
            }
        }

        public async Task<List<SubjectPerformanceTrendDto>> GetSubjectPerformanceTrendAsync(int subjectId, int? startYear = null, int? endYear = null, int? yearsBack = null)
        {
            try
            {
                var (fromYear, toYear) = await GetYearRangeAsync(startYear, endYear, yearsBack);

                var subject = await _context.Subjects.FindAsync(subjectId);
                if (subject == null)
                {
                    return new List<SubjectPerformanceTrendDto>();
                }

                var academicYears = await _context.AcademicYears
                    .Where(ay => ay.StartDate.HasValue &&
                                ay.StartDate.Value.Year >= fromYear &&
                                ay.StartDate.Value.Year <= toYear)
                    .OrderBy(ay => ay.StartDate)
                    .ToListAsync();

                var trends = new List<SubjectPerformanceTrendDto>();

                foreach (var academicYear in academicYears)
                {
                    var subjectScores = await _context.SubjectScores
                        .Where(ss => ss.SubjectId == subjectId)
                        .ToListAsync();

                    if (!subjectScores.Any()) continue;

                    var subjectScoreIds = subjectScores.Select(ss => ss.SubjectScoreId).ToList();

                    var studentScores = await _context.StudentScores
                        .Include(ss => ss.Enrollment)
                            .ThenInclude(e => e.ClassSession)
                                .ThenInclude(cs => cs.Semester)
                        .Where(ss => ss.Score.HasValue &&
                                    ss.SubjectScoreId != null &&
                                    subjectScoreIds.Contains(ss.SubjectScoreId) &&
                                    ss.Enrollment.ClassSession.Semester.AcademicYearId == academicYear.AcademicYearId)
                        .ToListAsync();

                    if (!studentScores.Any()) continue;

                    var scoreValues = studentScores.Select(ss => ss.Score.Value).ToList();
                    var studentCount = studentScores.Select(ss => ss.Enrollment.StudentId).Distinct().Count();

                    trends.Add(new SubjectPerformanceTrendDto
                    {
                        SubjectId = subjectId,
                        SubjectName = subject.SubjectName,
                        AcademicYear = academicYear.YearName,
                        AverageScore = Math.Round(scoreValues.Average(), 2),
                        PassRate = Math.Round((double)scoreValues.Count(s => s >= _academicRules.Average.AverageThreshold) / scoreValues.Count * 100, 2),
                        ExcellentRate = Math.Round((double)scoreValues.Count(s => s >= _academicRules.Excellent.AverageThreshold) / scoreValues.Count * 100, 2),
                        StudentCount = studentCount,
                        DifficultyIndex = Math.Round(100 - ((double)scoreValues.Count(s => s >= _academicRules.Average.AverageThreshold) / scoreValues.Count * 100), 2)
                    });
                }

                return trends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSubjectPerformanceTrendAsync: {ex.Message}");
                return new List<SubjectPerformanceTrendDto>();
            }
        }

        private int GetGradeCount(Dictionary<string, int> gradeCounts, string gradeNumber)
        {
            var patterns = new[] {
                $"Khối {gradeNumber}",
                $"Lớp {gradeNumber}",
                $"Grade {gradeNumber}",
                gradeNumber
            };

            foreach (var pattern in patterns)
            {
                var key = gradeCounts.Keys.FirstOrDefault(k =>
                    k.Equals(pattern, StringComparison.OrdinalIgnoreCase) ||
                    k.Contains(gradeNumber));
                if (key != null)
                    return gradeCounts[key];
            }

            return 0;
        }
    }
}