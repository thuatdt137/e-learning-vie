using e_learning_vie.Commons;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.AcademicYearsController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicYearsController : ControllerBase
    {
        private readonly IAcademicYearService _service;

        public AcademicYearsController(IAcademicYearService service)
        {
            _service = service;
        }


        [HttpGet]
        public IActionResult GetAcademicYearList()
        {
            try
            {
                var academicYears = _service.GetAcademicYearList();
                return Ok(ApiResponse<Object>.Success("Success", academicYears));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(ex.Message));
            }
        }

    }
}
