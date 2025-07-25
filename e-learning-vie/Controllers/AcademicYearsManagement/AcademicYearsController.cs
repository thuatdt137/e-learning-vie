using e_learning_vie.Commons;
using e_learning_vie.DTOs.AcademicYear;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.AcademicYearsManagement
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
        public IActionResult GetAcademicYears()
        {
            var academicYears = _context.AcademicYears.Select(a => new AcademicYearDTO
            {
                AcademicYearId = a.AcademicYearId,
                YearName = a.YearName,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", academicYears));
        }

        [HttpPost]
        public IActionResult CreateAcademicYear([FromBody] AcademicYearDTO academicYearDTO)
        {
            if(academicYearDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid academic year data."));
            }
            if(!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var academicYear = new AcademicYear
            {
                YearName = academicYearDTO.YearName,
                StartDate = academicYearDTO.StartDate,
                EndDate = academicYearDTO.EndDate
            };
            var result = _context.AcademicYears.Add(academicYear);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Academic year created successfully.", result));
        }
        [HttpPut("{id}")]
        public IActionResult UpdateAcademicYear(int id, [FromBody] AcademicYearDTO academicYearDTO)
        {
            if(academicYearDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid academic year data."));
            }
            if(!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var academicYear = _context.AcademicYears.Find(id);
            if(academicYear == null)
            {
                return NotFound(ApiResponse<object>.Fail("Academic year not found."));
            }
            academicYear.YearName = academicYearDTO.YearName;
            academicYear.StartDate = academicYearDTO.StartDate;
            academicYear.EndDate = academicYearDTO.EndDate;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Academic year updated successfully.", academicYear));
        }
        [HttpGet("{id}")]
        public IActionResult GetAcademicYearById(int id)
        {
            var academicYear = _context.AcademicYears
                .Where(a => a.AcademicYearId == id)
                .Select(a => new AcademicYearDTO
                {
                    AcademicYearId = a.AcademicYearId,
                    YearName = a.YearName,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate
                })
                .FirstOrDefault();
            if(academicYear == null)
            {
                return NotFound(ApiResponse<object>.Fail("Academic year not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", academicYear));
        }
    }
}
