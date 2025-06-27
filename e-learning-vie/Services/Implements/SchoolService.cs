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
        public SchoolDTO AddSchool(SchoolDTO schoolDto)
        {
            throw new NotImplementedException();
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
            catch(Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the school with ID {id}.", ex);
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
            catch(Exception ex)
            {
                throw new Exception("An error occurred while retrieving the school list.", ex);
            }
        }

        public SchoolDTO UpdateSchool(int id, SchoolDTO schoolDto)
        {
            throw new NotImplementedException();
        }
    }
}
