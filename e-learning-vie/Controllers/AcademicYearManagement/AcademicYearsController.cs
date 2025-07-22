using e_learning_vie.Commons;
using e_learning_vie.DTOs.AcademicYear;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.AcademicYearManagement
{
    //[Authorize(Roles = "MinistryOfEducation")]
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
                var result = _service.GetAcademicYearList();
                return Ok(ApiResponse<object>.Success("Success", result));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetAcademicYearById(int id)
        {
            try
            {
                var result = _service.GetAcademicYearById(id);
                if(result == null)
                    return NotFound(ApiResponse<object>.Fail($"AcademicYear with ID {id} not found."));
                return Ok(ApiResponse<object>.Success("Success", result));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost]
        public IActionResult CreateAcademicYear([FromBody] AcademicYearDTO dto)
        {
            if(dto == null)
                return BadRequest(ApiResponse<object>.Fail("Invalid academic year data."));
            if(!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid academic year data.", errors));
            }
            try
            {
                var result = _service.CreateAcademicYear(dto);
                return StatusCode(201, ApiResponse<object>.Success("Academic year added successfully", result));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAcademicYear([FromBody] AcademicYearDTO dto, int id)
        {
            if(dto == null)
                return BadRequest(ApiResponse<object>.Fail("Invalid academic year data."));
            if(!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid academic year data.", errors));
            }
            try
            {
                var result = _service.UpdateAcademicYear(id, dto);
                return Ok(ApiResponse<object>.Success("Academic year updated successfully", result));
            }
            catch(KeyNotFoundException knfEx)
            {
                return NotFound(ApiResponse<object>.Error(knfEx.Message));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"An error occurred: {ex.Message}"));
            }
        }
    }
}
