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
        public IActionResult GetSchoolList(int? pageNumber, int? pageSize)
        {
            try
            {
                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);
                var schools = _schoolService.GetSchoolList()
                    .Skip((effectivePageNumber - 1) * effectivePageSize)
                    .Take(effectivePageSize)
                    .ToList();
                if(schools == null || !schools.Any())
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No schools found.");
                }
                return StatusCode(StatusCodes.Status200OK, schools);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving the school list: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetSchoolById(int id)
        {
            try
            {
                var school = _schoolService.GetSchoolById(id);
                if(school == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"School with ID {id} not found.");
                }
                return StatusCode(StatusCodes.Status200OK, school);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving the school: {ex.Message}");
            }
        }

    }
}
