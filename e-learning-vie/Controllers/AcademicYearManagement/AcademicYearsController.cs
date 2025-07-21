using e_learning_vie.Commons;
using e_learning_vie.DTOs.AcademicYear;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.AcademicYearManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicYearsController : ControllerBase
    {

        private readonly SchoolManagementContext _context;

        public AcademicYearsController(SchoolManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(ApiResponse<object>.Success("Success", _context.AcademicYears.Select(s => new AcademicYearDTO
            {
                AcademicYearId = s.AcademicYearId,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                AspirationEditDeadline = s.AspirationEditDeadline,
                AspirationRegistrationEndDate = s.AspirationRegistrationEndDate,
                AspirationRegistrationStartDate = s.AspirationRegistrationStartDate,
                YearName = s.YearName
            })));
        }

    }
}
