using e_learning_vie.DTOs.AcademicYear;
using e_learning_vie.Services.Interfaces;
using e_learning_vie.Models;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Services.Implements
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly SchoolManagementContext _context;
        public AcademicYearService(SchoolManagementContext context)
        {
            _context = context;
        }

        public dynamic GetAcademicYearList()
        {
            try
            {
                return _context.AcademicYears.Select(s => new DTOs.AcademicYear.AcademicYearDTO
                {
                    AcademicYearId = s.AcademicYearId,
                    YearName = s.YearName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AspirationRegistrationStartDate = s.AspirationRegistrationStartDate,
                    AspirationRegistrationEndDate = s.AspirationRegistrationEndDate,
                    AspirationEditDeadline = s.AspirationEditDeadline
                }).ToList();
            }
            catch
            {
                throw;
            }
        }

        public dynamic GetAcademicYearById(int id)
        {
            try
            {
                var s = _context.AcademicYears.FirstOrDefault(x => x.AcademicYearId == id);
                if (s == null) return null;
                return new DTOs.AcademicYear.AcademicYearDTO
                {
                    AcademicYearId = s.AcademicYearId,
                    YearName = s.YearName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AspirationRegistrationStartDate = s.AspirationRegistrationStartDate,
                    AspirationRegistrationEndDate = s.AspirationRegistrationEndDate,
                    AspirationEditDeadline = s.AspirationEditDeadline
                };
            }
            catch
            {
                throw;
            }
        }

        public dynamic CreateAcademicYear(DTOs.AcademicYear.AcademicYearDTO dto)
        {
            try
            {
                var entity = new AcademicYear
                {
                    YearName = dto.YearName,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    AspirationRegistrationStartDate = dto.AspirationRegistrationStartDate,
                    AspirationRegistrationEndDate = dto.AspirationRegistrationEndDate,
                    AspirationEditDeadline = dto.AspirationEditDeadline
                };
                _context.AcademicYears.Add(entity);
                _context.SaveChanges();
                dto.AcademicYearId = entity.AcademicYearId;
                return dto;
            }
            catch
            {
                throw;
            }
        }

        public dynamic UpdateAcademicYear(int id, DTOs.AcademicYear.AcademicYearDTO dto)
        {
            if (id != dto.AcademicYearId)
                throw new ArgumentException("AcademicYear ID mismatch");
            try
            {
                var entity = _context.AcademicYears.FirstOrDefault(x => x.AcademicYearId == id);
                if (entity == null)
                    throw new KeyNotFoundException("AcademicYear not found");
                entity.YearName = dto.YearName;
                entity.StartDate = dto.StartDate;
                entity.EndDate = dto.EndDate;
                entity.AspirationRegistrationStartDate = dto.AspirationRegistrationStartDate;
                entity.AspirationRegistrationEndDate = dto.AspirationRegistrationEndDate;
                entity.AspirationEditDeadline = dto.AspirationEditDeadline;
                _context.SaveChanges();
                return dto;
            }
            catch
            {
                throw;
            }
        }
    }
}
