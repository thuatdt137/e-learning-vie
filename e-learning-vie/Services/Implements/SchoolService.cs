using e_learning_vie.DTOs.School;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;

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

        public SchoolDTO GetSchoolById(int id)
        {
            try
            {
                var school = _context.Schools.FirstOrDefault(s => s.SchoolId == id);
                if(school == null)
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
            if(id != schoolDto.SchoolId)
            {
                throw new ArgumentException("School ID mismatch");
            }
            try
            {
                var school = _context.Schools.FirstOrDefault(s => s.SchoolId == id);
                if(school == null)
                {
                    throw new KeyNotFoundException("School not found");
                }
                var principal = _context.Teachers.FirstOrDefault(p => p.TeacherId == schoolDto.PrincipalId);
                if(principal == null)
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
