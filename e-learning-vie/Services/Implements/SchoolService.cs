using e_learning_vie.DTOs.School;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using e_learning_vie.Utils;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Services.Implements
{
    public class SchoolService : ISchoolService
    {
        private SchoolManagementContext _context;
        public SchoolService(SchoolManagementContext context)
        {
            _context = context;
        }
        public dynamic AddSchool(SchoolDTO schoolDto)
        {
            try
            {
                if (schoolDto.PrincipalId != null)
                {
                    var principal = _context.Teachers.FirstOrDefault(p => p.TeacherId == schoolDto.PrincipalId);
                    if (principal == null)
                    {
                        throw new KeyNotFoundException("Principal not found");
                    }
                }
                var school = schoolDto.ToSchool();
                _context.Schools.Add(school);
                _context.SaveChanges();
                return new SchoolDTO(school);
            }
            catch
            {
                throw;
            }
        }

        public dynamic GetAdmissionScore(int quotaId)
        {
            var quota = _context.Quotas.Include(q => q.School).FirstOrDefault(q => q.QuotaId == quotaId);
            if (quota == null)
            {
                throw new KeyNotFoundException("Quota not found");
            }
            var school = quota.School;
            var students = _context.Aspirations
                .Where(a => a.TargetSchoolId == quota.SchoolId && a.AcademicYearId == quota.AcademicYearId)
                .OrderByDescending(a => a.Priority)
                .Select(a => a.Student).ToList();

            var examSubjects = _context.Subjects
                .Where(s => s.SubjectType == school.SchoolType.GetDisplayName())
                .Select(s => s.SubjectId)
                .ToList();
            var studentScores = new List<(int StudentId, double TotalScore)>();
            //foreach (var student in students)
            //{
            //    var scores = _context.StudentScores
            //        .Where(s => s.StudentId == student.StudentId && examSubjects.Contains(s.SubjectGrade.SubjectId))
            //        .Select(s => s.Score)
            //        .ToList();
            //    if (scores.Count == examSubjects.Count && scores.Count > 0)
            //    {
            //        double totalScore = (double)scores.Sum();
            //        studentScores.Add((student.StudentId, totalScore));
            //    }
            //}
            var ranked = studentScores.OrderByDescending(x => x.TotalScore).ToList();


            if (!ranked.Any())
                return new { AdmissionScore = 0, Message = "No valid student scores found." };

            int quotaNumber = quota.QuotaNumber.Value;
            int admissionIndex = quotaNumber - 1;
            if (admissionIndex >= ranked.Count)
                admissionIndex = ranked.Count - 1;

            double admissionScore = admissionIndex >= 0 ? ranked[admissionIndex].TotalScore : 0;

            int sameScoreCount = ranked.Count(x => x.TotalScore == admissionScore);

            double? nextHigherScore = ranked.Select(r => r.TotalScore)
                .Where(s => s > admissionScore)
                .OrderByDescending(s => s)
                .FirstOrDefault();

            return new
            {
                AdmissionScore = admissionScore,
                TotalCandidates = ranked.Count,
                Quota = quotaNumber
            };
        }

        public SchoolDTO GetSchoolById(int id)
        {
            try
            {
                var school = _context.Schools.FirstOrDefault(s => s.SchoolId == id);
                if (school == null)
                {
                    return null;
                }
                return new SchoolDTO
                {
                    SchoolId = school.SchoolId,
                    SchoolName = school.SchoolName,
                    SchoolType = school.SchoolType,
                    Address = school.Address,
                    Phone = school.Phone,
                    Email = school.Email,
                    PrincipalId = school.PrincipalId
                };
            }
            catch
            {
                throw;
            }
        }

        public List<SchoolDTO> GetSchoolList()
        {
            try
            {
                return _context.Schools.Select(s => new SchoolDTO
                {
                    SchoolId = s.SchoolId,
                    SchoolName = s.SchoolName,
                    SchoolType = s.SchoolType,
                    Address = s.Address,
                    Phone = s.Phone,
                    Email = s.Email,
                    PrincipalId = s.PrincipalId
                }).ToList();
            }
            catch
            {
                throw;
            }
        }

        public dynamic UpdateSchool(int id, SchoolDTO schoolDto)
        {
            if (id != schoolDto.SchoolId)
            {
                throw new ArgumentException("School ID mismatch");
            }
            try
            {
                var school = _context.Schools.FirstOrDefault(s => s.SchoolId == id);
                if (school == null)
                {
                    throw new KeyNotFoundException("School not found");
                }
                var principal = _context.Teachers.FirstOrDefault(p => p.TeacherId == schoolDto.PrincipalId);
                if (principal == null)
                {
                    throw new KeyNotFoundException("Principal not found");
                }
                school.SchoolName = schoolDto.SchoolName;
                school.SchoolType = schoolDto.SchoolType;
                school.Address = schoolDto.Address;
                school.Phone = schoolDto.Phone;
                school.Email = schoolDto.Email;
                school.PrincipalId = schoolDto.PrincipalId;
                _context.SaveChanges();
                return new SchoolDTO(school);
            }
            catch
            {
                throw;
            }
        }
    }
}
