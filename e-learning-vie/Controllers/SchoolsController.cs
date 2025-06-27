using e_learning_vie.Commons;
using e_learning_vie.DTOs.School;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace e_learning_vie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolsController : ControllerBase
    {

        private readonly SchoolManagementContext _context;
        private readonly ISchoolService _schoolService;

        public SchoolsController(SchoolManagementContext context, ISchoolService schoolService)
        {
            _context = context;
            _schoolService = schoolService;
        }

        [HttpGet]
        public IActionResult GetSchoolList(int? pageNumber, int? pageSize, string? schoolType, string? keyWord)
        {
            try
            {
                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);

                schoolType = schoolType?.Trim() ?? "";
                keyWord = keyWord?.Trim() ?? "";

                var schools = _schoolService.GetSchoolList();

                if(!string.IsNullOrEmpty(schoolType))
                {
                    schools = schools.Where(s => s.SchoolType != null && s.SchoolType.Contains(schoolType, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if(!string.IsNullOrEmpty(keyWord))
                {
                    schools = schools.Where(s => s.SchoolName.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.Address.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.Phone.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.Email.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                schools = schools
                    .Skip((effectivePageNumber - 1) * effectivePageSize)
                    .Take(effectivePageSize)
                    .ToList();
                if(schools == null || !schools.Any())
                {
                    return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Fail("No schools found!"));
                }
                return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get school list success", schools));
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Error($"An error occurred {ex.Message}"));
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetSchoolById(int? id)
        {
            try
            {
                var school = _schoolService.GetSchoolById(id.Value);
                if(school == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Fail($"School with ID {id} not found."));
                }
                return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get school success", school));
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail($"An error occurred: {ex.Message}", ex));
            }
        }

        [HttpPost]
        public IActionResult AddSchool([FromBody] SchoolDTO schoolDto)
        {
            if(schoolDto == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<object>.Fail("Invalid school data."));
            }
            if(!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors
                        .Select(e => e.ErrorMessage).ToArray()
                    );
                return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<object>.Fail("Invalid school data.", errors));
            }
            try
            {
                var newSchool = _schoolService.AddSchool(schoolDto);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Success("School added successfully", newSchool));
            }
            catch(KeyNotFoundException knfEx)
            {
                return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Error(knfEx.Message));
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Error($"An error occurred: {ex.Message}"));
            }
        }

    }
}
