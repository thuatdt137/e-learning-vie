using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;

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
                return _context.AcademicYears
                    .Select(ay => new
                    {
                        ay.AcademicYearId,
                        ay.YearName,
                        ay.StartDate,
                        ay.EndDate,

                    }).OrderByDescending(ay => ay.StartDate)
                    .ToList();
            }
            catch
            {
                throw;
            }
        }
    }
}
