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





        public dynamic GetClassAcademicLevel(int classId, int semesterId)
        {
            var classScores = GetClassScores(classId, semesterId);

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


            foreach(var student in classScores.Students)
            {
                var hocLuc = student.AcademicLevel?.Trim();
                if(!string.IsNullOrEmpty(hocLuc) && academicLevelStats.ContainsKey(hocLuc))
                {
                    academicLevelStats[hocLuc]++;
                }

                var hanhKiem = student.Conduct?.Trim();
                if(!string.IsNullOrEmpty(hanhKiem) && conductStats.ContainsKey(hanhKiem))
                {
                    conductStats[hanhKiem]++;
                }
            }

            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.ClassSessionId == classId && e.ClassSession.SemesterId == semesterId)
                .ToList();

            return new
            {
                classId = classScores.ClassId,
                className = classScores.ClassName,
                semesterId = classScores.SemesterId,
                semesterName = classScores.SemesterName,
                academicYear = classScores.AcademicYear,

                totalStudents = enrollments.Count,
                maleStudents = enrollments.Count(e => e.Student.IsMale),
                femaleStudents = enrollments.Count(e => !e.Student.IsMale),

                academicLevelStats = academicLevelStats,
                conductStats = conductStats
            };
        }

    }
}
