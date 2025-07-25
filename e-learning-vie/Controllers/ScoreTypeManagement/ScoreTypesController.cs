using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.ScoreType;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.ScoreTypeManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreTypesController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public ScoreTypesController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllScoreTypes()
        {
            var scoreTypes = _context.ScoreTypes.Select(s => new ScoreTypeDTO
            {
                ScoreId = s.ScoreId,
                TypeName = s.TypeName,
                Weight = s.Weight,
                Description = s.Description
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", scoreTypes));
        }

        [HttpGet("{id}")]
        public IActionResult GetScoreTypeById(int id)
        {
            var scoreType = _context.ScoreTypes.Where(s => s.ScoreId == id).Select(s => new ScoreTypeDTO
            {
                ScoreId = s.ScoreId,
                TypeName = s.TypeName,
                Weight = s.Weight,
                Description = s.Description
            }).FirstOrDefault();

            if (scoreType == null)
            {
                return NotFound(ApiResponse<object>.Fail("ScoreType not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", scoreType));
        }

        [HttpPost]
        public IActionResult CreateScoreType([FromBody] ScoreTypeDTO scoreTypeDTO)
        {
            if (scoreTypeDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid scoreType data."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value?.Errors?.Count > 0)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(ApiResponse<object>.Fail("Model validation failed.", errors));
            }

            try
            {
                var scoreType = new ScoreType
                {
                    TypeName = scoreTypeDTO.TypeName,
                    Weight = scoreTypeDTO.Weight,
                    Description = scoreTypeDTO.Description
                };

                _context.ScoreTypes.Add(scoreType);
                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("ScoreType created successfully.", scoreType));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateScoreType(int id, [FromBody] ScoreTypeDTO scoreTypeDTO)
        {
            if (scoreTypeDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid scoreType data."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value?.Errors?.Count > 0)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(ApiResponse<object>.Fail("Model validation failed.", errors));
            }

            try
            {
                var scoreType = _context.ScoreTypes.FirstOrDefault(s => s.ScoreId == id);
                if (scoreType == null)
                {
                    return NotFound(ApiResponse<object>.Fail("ScoreType not found."));
                }

                scoreType.TypeName = scoreTypeDTO.TypeName;
                scoreType.Weight = scoreTypeDTO.Weight;
                scoreType.Description = scoreTypeDTO.Description;

                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("ScoreType updated successfully.", scoreType));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteScoreType(int id)
        {
            try
            {
                var scoreType = _context.ScoreTypes.FirstOrDefault(s => s.ScoreId == id);
                if (scoreType == null)
                {
                    return NotFound(ApiResponse<object>.Fail("ScoreType not found."));
                }

                _context.ScoreTypes.Remove(scoreType);
                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("ScoreType deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
