using e_learning_vie.DTOs.AcademicLevel;
using e_learning_vie.DTOs.ScoreDetail;
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
            var semester = _context.Semesters.Include(s => s.AcademicYear)
                .FirstOrDefault(s => s.SemesterId == semesterId);
            if(semester == null) throw new Exception("Semester not found.");

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if(student == null) throw new Exception("Student not found.");

            var enrollment = _context.Enrollments
                .Include(e => e.ClassSession).ThenInclude(cs => cs.Class)
                .Include(e => e.StudentScores).ThenInclude(ss => ss.Subject)
                .Include(e => e.StudentScores).ThenInclude(ss => ss.ScoreType)
                .FirstOrDefault(e => e.StudentId == studentId && e.ClassSession.SemesterId == semesterId);

            if(enrollment == null) throw new Exception("Enrollment not found.");

            var scores = enrollment.StudentScores
                .GroupBy(s => s.SubjectId)
                .Select(group => new SubjectScoreDTO
                {
                    SubjectId = group.Key,
                    SubjectName = group.First().Subject.SubjectName,
                    IsMainSubject = group.First().Subject.IsMainSubject,
                    ScoreDetails = group.Select(s => new ScoreDetailDTO
                    {
                        ScoreType = s.ScoreType.TypeName,
                        Score = s.Score,
                        Weight = s.ScoreType.Weight
                    }).ToList()
                }).ToList();

            return new StudentScoreDTO
            {
                StudentId = student.StudentId,
                StudentName = student.FirstName + " " + student.LastName,
                StudentClass = enrollment.ClassSession.Class.ClassName,
                Gender = student.IsMale,
                Dob = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                Conduct = enrollment.Conduct,
                Semester = semester.SemesterName,
                AcademicYear = semester.AcademicYear.YearName,
                Scores = scores
            };
        }

        public dynamic GetClassScores(int classId, int semesterId)
        {
            var semester = _context.Semesters.Include(s => s.AcademicYear)
                .FirstOrDefault(s => s.SemesterId == semesterId);
            if(semester == null) throw new Exception("Semester not found.");

            var currentClass = _context.Classes.Include(c => c.Grade).FirstOrDefault(c => c.ClassId == classId);
            if(currentClass == null) throw new Exception("Class not found.");

            var classSession = _context.ClassSessions
                .FirstOrDefault(cs => cs.ClassSessionId == classId && cs.SemesterId == semesterId);
            if(classSession == null) throw new Exception("Class not found for the given semester.");

            var enrollments = _context.Enrollments
                .Where(e => e.ClassSessionId == classId && e.ClassSession.SemesterId == semesterId)
                .ToList();

            var studentScores = enrollments
                .Select(e =>
                {
                    var fullScores = GetStudentScores(e.StudentId, semesterId);

                    var avgScores = fullScores.Scores.Select(s =>
                    {
                        var totalWeight = s.ScoreDetails.Sum(sd => sd.Weight);
                        var weightedSum = s.ScoreDetails.Sum(sd => sd.Score.GetValueOrDefault() * sd.Weight);
                        var average = totalWeight == 0 ? 0 : Math.Round(weightedSum / totalWeight, 1, MidpointRounding.AwayFromZero);

                        return new
                        {
                            subjectId = s.SubjectId,
                            subjectName = s.SubjectName,
                            isMainSubject = s.IsMainSubject,
                            averageScore = average
                        };
                    }).ToList();

                    double avgAll = avgScores.Count == 0
                        ? 0
                        : Math.Round(avgScores.Average(s => s.averageScore), 1, MidpointRounding.AwayFromZero);

                    var subjectAverages = avgScores.Select(s => s.averageScore).ToList();
                    var mainSubjectAverages = avgScores
                        .Where(s => s.isMainSubject)
                        .Select(s => s.averageScore)
                        .ToList();

                    // Gọi util để xét học lực
                    var academicLevelUtil = new AcademicLevelUtil(_rules);
                    string academicLevel = academicLevelUtil.EvaluateConductLevel(
                        avgAll,
                        mainSubjectAverages,
                        subjectAverages,
                        fullScores.Conduct
                    );

                    return new
                    {
                        studentId = fullScores.StudentId,
                        studentName = fullScores.StudentName,
                        studentClass = fullScores.StudentClass,
                        gender = fullScores.Gender,
                        dob = fullScores.Dob,
                        conduct = fullScores.Conduct,
                        averageAllSubjects = avgAll,
                        academicLevel = academicLevel,
                        scores = avgScores
                    };
                }).ToList();

            return new
            {
                classId = currentClass.ClassId,
                className = currentClass.ClassName,
                gradId = currentClass.GradeId,
                grade = currentClass.Grade.GradeName,
                semesterId = semester.SemesterId,
                semesterName = semester.SemesterName,
                academicYear = semester.AcademicYear.YearName,
                students = studentScores
            };
        }



        public dynamic GetClassAcademicLevel(int classId, int semesterId)
        {
            throw new NotImplementedException("This method is not implemented yet.");
        }
    }
}
