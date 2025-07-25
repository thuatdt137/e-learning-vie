using e_learning_vie.DTOs.AcademicLevel;
using e_learning_vie.DTOs.ScoreDetail;
using e_learning_vie.Enums;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using e_learning_vie.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace e_learning_vie.Services.Implements
{
    public class StudentAcademicService : IStudentAcademicService
    {
        private readonly SchoolManagementContext _context;
        private readonly AcademicLevelRulesConfig _rules;
        public StudentAcademicService(SchoolManagementContext context, IOptions<AcademicLevelRulesConfig> rules)
        {
            _context = context;
            _rules = rules.Value;
        }
        public StudentScoreDTO GetStudentScores(int studentId, int semesterId)
        {
            var semester = _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefault(s => s.SemesterId == semesterId);
            if(semester == null)
                throw new Exception("Semester not found.");

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if(student == null)
                throw new Exception("Student not found.");

            var enrollment = _context.Enrollments
                .Include(e => e.ClassSession).ThenInclude(cs => cs.Class)
                .Include(e => e.StudentScores)
                    .ThenInclude(ss => ss.SubjectScore)
                        .ThenInclude(sub => sub.Subject)
                .Include(e => e.StudentScores)
                    .ThenInclude(ss => ss.SubjectScore)
                        .ThenInclude(sub => sub.ScoreType)
                .FirstOrDefault(e => e.StudentId == studentId && e.ClassSession.SemesterId == semesterId);

            if(enrollment == null)
                throw new Exception("Enrollment not found.");

            var scores = enrollment.StudentScores
                .Where(s => s.SubjectScore != null && s.SubjectScore.Subject != null && s.SubjectScore.ScoreType != null)
                .GroupBy(s => s.SubjectScore.SubjectId)
                .Select(group => new SubjectScoreDTO
                {
                    SubjectId = group.Key,
                    SubjectName = group.First().SubjectScore.Subject.SubjectName,
                    IsMainSubject = group.First().SubjectScore.Subject.IsMainSubject,
                    ScoreDetails = group.Select(s => new ScoreDetailDTO
                    {
                        ScoreType = s.SubjectScore.ScoreType.TypeName,
                        Score = s.Score,
                        Weight = s.SubjectScore.ScoreType.Weight
                    }).ToList()
                }).ToList();

            return new StudentScoreDTO
            {
                StudentId = student.StudentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                StudentClass = enrollment.ClassSession.Class.ClassName,
                Gender = student.IsMale,
                Dob = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                Conduct = enrollment.Conduct,
                Semester = semester.SemesterName,
                AcademicYear = semester.AcademicYear.YearName,
                Scores = scores
            };
        }

        public ClassScoreDTO GetClassScores(int classId, int semesterId)
        {
            var semester = _context.Semesters.Include(s => s.AcademicYear)
                .FirstOrDefault(s => s.SemesterId == semesterId);
            if(semester == null) throw new Exception("Semester not found.");

            var currentClass = _context.Classes.Include(c => c.Grade)
                .FirstOrDefault(c => c.ClassId == classId);
            if(currentClass == null) throw new Exception("Class not found.");

            var classSession = _context.ClassSessions
                .FirstOrDefault(cs => cs.ClassSessionId == classId && cs.SemesterId == semesterId);
            if(classSession == null) throw new Exception("Class not found for the given semester.");

            var enrollments = _context.Enrollments
                .Where(e => e.ClassSessionId == classId && e.ClassSession.SemesterId == semesterId)
                .ToList();

            var students = enrollments.Select(e =>
            {
                var studentScores = _context.StudentScores
                    .Include(ss => ss.SubjectScore)
                        .ThenInclude(s => s.Subject)
                    .Include(ss => ss.SubjectScore)
                        .ThenInclude(s => s.ScoreType)
                    .Where(ss => ss.EnrollmentId == e.EnrollmentId)
                    .ToList();

                var scoreGroups = studentScores
                    .GroupBy(ss => ss.SubjectScore.SubjectId)
                    .Select(group =>
                    {
                        var subject = group.First().SubjectScore.Subject;
                        var scoreDetails = group.Select(ss => new
                        {
                            Score = ss.Score ?? 0,
                            Weight = ss.SubjectScore.ScoreType.Weight
                        }).ToList();

                        double totalWeight = scoreDetails.Sum(s => s.Weight);
                        double weightedSum = scoreDetails.Sum(s => s.Score * s.Weight);
                        double average = totalWeight == 0 ? 0 : Math.Round(weightedSum / totalWeight, 1, MidpointRounding.AwayFromZero);

                        return new SubjectAverageDTO
                        {
                            SubjectId = subject.SubjectId,
                            SubjectName = subject.SubjectName,
                            IsMainSubject = subject.IsMainSubject,
                            AverageScore = average
                        };
                    }).ToList();

                double avgAll = scoreGroups.Count == 0
                    ? 0
                    : Math.Round(scoreGroups.Average(s => s.AverageScore), 1, MidpointRounding.AwayFromZero);

                var subjectAverages = scoreGroups.Select(s => s.AverageScore).ToList();
                var mainSubjectAverages = scoreGroups
                    .Where(s => s.IsMainSubject)
                    .Select(s => s.AverageScore)
                    .ToList();

                var academicLevelUtil = new AcademicLevelUtil(_rules);
                string academicLevel = academicLevelUtil.EvaluateConductLevel(
                    avgAll,
                    mainSubjectAverages,
                    subjectAverages,
                    e.Conduct
                );

                var student = _context.Students.FirstOrDefault(s => s.StudentId == e.StudentId);

                return new StudentScoreInClassDTO
                {
                    StudentId = student.StudentId,
                    StudentName = $"{student.FirstName} {student.LastName}",
                    StudentClass = currentClass.ClassName,
                    Gender = student.IsMale,
                    Dob = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                    Conduct = e.Conduct,
                    AverageAllSubjects = avgAll,
                    AcademicLevel = academicLevel,
                    Scores = scoreGroups
                };
            }).ToList();

            return new ClassScoreDTO
            {
                ClassId = currentClass.ClassId,
                ClassName = currentClass.ClassName,
                GradeId = currentClass.GradeId.Value,
                Grade = currentClass.Grade.GradeName,
                SemesterId = semester.SemesterId,
                SemesterName = semester.SemesterName,
                AcademicYear = semester.AcademicYear.YearName,
                Students = students
            };
        }





        public ClassAcademicLevelStatisticsDTO GetClassAcademicLevel(int classId, int semesterId)
        {
            var classScores = GetClassScores(classId, semesterId); // dùng DTO thay vì dynamic

            var academicLevelStats = new Dictionary<string, int>
            {
                { AcademicLevel.Excellent.GetDisplayName(), 0 },
                { AcademicLevel.Good.GetDisplayName(), 0 },
                { AcademicLevel.Average.GetDisplayName(), 0 },
                { AcademicLevel.Weak.GetDisplayName(), 0 }
            };

            var conductStats = new Dictionary<string, int>
            {
                { ConductLevel.Excellent.GetDisplayName(), 0 },
                { ConductLevel.Good.GetDisplayName(), 0 },
                { ConductLevel.Average.GetDisplayName(), 0 },
                { ConductLevel.Weak.GetDisplayName(), 0 }
            };

            var subjectScoreSums = new Dictionary<int, (double total, int count)>();

            double totalAvg = 0;
            int studentCount = classScores.Students.Count;

            foreach(var student in classScores.Students)
            {
                var academic = student.AcademicLevel?.Trim();
                if(!string.IsNullOrEmpty(academic) && academicLevelStats.ContainsKey(academic))
                    academicLevelStats[academic]++;

                var conduct = student.Conduct?.Trim();
                if(!string.IsNullOrEmpty(conduct) && conductStats.ContainsKey(conduct))
                    conductStats[conduct]++;



                foreach(var subject in student.Scores)
                {
                    if(!subjectScoreSums.ContainsKey(subject.SubjectId))
                        subjectScoreSums[subject.SubjectId] = (0, 0);

                    subjectScoreSums[subject.SubjectId] = (
                        subjectScoreSums[subject.SubjectId].total + subject.AverageScore,
                        subjectScoreSums[subject.SubjectId].count + 1
                    );
                }
            }


            var subjectAverages = subjectScoreSums.Select(kv =>
            {
                var avgScore = Math.Round(kv.Value.total / kv.Value.count, 1, MidpointRounding.AwayFromZero);
                totalAvg += avgScore;
                var subject = _context.Subjects.FirstOrDefault(s => s.SubjectId == kv.Key);
                return new SubjectAverageDTO
                {
                    SubjectId = kv.Key,
                    SubjectName = classScores.Students
                    .SelectMany(s => s.Scores)
                    .FirstOrDefault(sc => sc.SubjectId == kv.Key)?.SubjectName ?? "Unknown",
                    IsMainSubject = subject?.IsMainSubject ?? false,
                    AverageScore = avgScore,
                };
            }).ToList();

            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.ClassSessionId == classId && e.ClassSession.SemesterId == semesterId)
                .ToList();

            return new ClassAcademicLevelStatisticsDTO
            {
                ClassId = classScores.ClassId,
                ClassName = classScores.ClassName,
                SemesterId = classScores.SemesterId,
                SemesterName = classScores.SemesterName,
                AcademicYear = classScores.AcademicYear,

                TotalStudents = enrollments.Count,
                MaleStudents = enrollments.Count(e => e.Student.IsMale),
                FemaleStudents = enrollments.Count(e => !e.Student.IsMale),

                AverageScore = studentCount > 0 ? Math.Round(totalAvg / studentCount, 1) : 0,
                SubjectAverages = subjectAverages,

                AcademicLevelStats = academicLevelStats,
                ConductStats = conductStats
            };
        }

        public dynamic GetGradeAcademicLevel(int gradeId, int semesterId)
        {
            var semester = _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefault(s => s.SemesterId == semesterId);

            if(semester == null)
                throw new Exception("Semester not found");

            var grade = _context.Grades.FirstOrDefault(g => g.GradeId == gradeId);
            if(grade == null)
                throw new Exception("Grade not found");

            var classSessions = _context.ClassSessions
                .Include(cs => cs.Class)
                .Where(cs => cs.Class.GradeId == gradeId && cs.SemesterId == semesterId)
                .ToList();

            var resultClasses = new List<object>();
            int totalStudents = 0, maleStudents = 0, femaleStudents = 0;

            var academicLevelStats = new Dictionary<string, int>
    {
        { AcademicLevel.Excellent.GetDisplayName(), 0 },
        { AcademicLevel.Good.GetDisplayName(), 0 },
        { AcademicLevel.Average.GetDisplayName(), 0 },
        { AcademicLevel.Weak.GetDisplayName(), 0 }
    };

            var conductStats = new Dictionary<string, int>
    {
        { ConductLevel.Excellent.GetDisplayName(), 0 },
        { ConductLevel.Good.GetDisplayName(), 0 },
        { ConductLevel.Average.GetDisplayName(), 0 },
        { ConductLevel.Weak.GetDisplayName(), 0 }
    };

            foreach(var cs in classSessions)
            {
                var classStats = GetClassAcademicLevel(cs.ClassId, semesterId);

                totalStudents += classStats.TotalStudents;
                maleStudents += classStats.MaleStudents;
                femaleStudents += classStats.FemaleStudents;

                // Gộp thống kê học lực
                foreach(var key in academicLevelStats.Keys.ToList())
                    academicLevelStats[key] += classStats.AcademicLevelStats.ContainsKey(key) ? classStats.AcademicLevelStats[key] : 0;

                // Gộp thống kê hạnh kiểm
                foreach(var key in conductStats.Keys.ToList())
                    conductStats[key] += classStats.ConductStats.ContainsKey(key) ? classStats.ConductStats[key] : 0;

                // Gộp danh sách lớp
                resultClasses.Add(new
                {
                    classId = classStats.ClassId,
                    className = classStats.ClassName,
                    totalStudents = classStats.TotalStudents,
                    academicLevelStats = classStats.AcademicLevelStats,
                    conductStats = classStats.ConductStats
                });
            }

            return new
            {
                gradeId = grade.GradeId,
                gradeName = grade.GradeName,
                semesterId = semester.SemesterId,
                semesterName = semester.SemesterName,
                academicYear = semester.AcademicYear.YearName,

                totalStudents,
                maleStudents,
                femaleStudents,

                academicLevelStats,
                conductStats,

                classes = resultClasses
            };
        }

    }
}
