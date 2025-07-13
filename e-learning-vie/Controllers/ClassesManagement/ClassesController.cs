using e_learning_vie.Commons;
using e_learning_vie.Models;
using e_learning_vie.Services.Implements;
using e_learning_vie.Services.Interfaces;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using e_learning_vie.DTOs.classes;

namespace e_learning_vie.Controllers.ClassesManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public ClassesController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllClasses(int? pageNumber, int? pageSize, string? keyWord, int? schoolId)
        {
            try
            {
                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);

                keyWord = keyWord?.Trim() ?? "";

                var classes = _context.Classes.Select(c => new ClassListDto(c)).ToList();

                if(schoolId != null)
                {
                    classes = classes.Where(c => c.SchoolId == schoolId).ToList();
                }

                if (!string.IsNullOrEmpty(keyWord))
                {
                    classes = classes.Where(s => s.ClassName.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                classes = classes
                    .Skip((effectivePageNumber - 1) * effectivePageSize)
                    .Take(effectivePageSize)
                    .ToList();
                if (classes == null || !classes.Any())
                {
                    return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Fail("No classes found!"));
                }
                return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get class list success", classes));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Error($"An error occurred {ex.Message}"));
            }
        }
    }
}
