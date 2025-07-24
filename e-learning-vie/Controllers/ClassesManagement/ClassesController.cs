using e_learning_vie.Commons;
using e_learning_vie.DTOs.classes;
using e_learning_vie.Models;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/Classes
        [HttpGet]
        public IActionResult GetAllClasses(int? pageNumber, int? pageSize, string? keyWord)
        {
            try
            {
                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);
                keyWord = keyWord?.Trim() ?? "";

                var classes = _context.Classes.Select(c => new ClassListDto(c)).ToList();

                if (!string.IsNullOrEmpty(keyWord))
                {
                    classes = classes.Where(s => s.ClassName.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (pageSize != null && pageNumber != null)
                {
                    classes = classes
                    .Skip((effectivePageNumber - 1) * effectivePageSize)
                    .Take(effectivePageSize)
                    .ToList();
                }

                if (classes == null || !classes.Any())
                {
                    return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Fail("No classes found!"));
                }
                return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get class list success", classes.ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        // GET: api/Classes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            try
            {
                var cls = await _context.Classes
                    .Where(c => c.ClassId == id)
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName
                    })
                    .FirstOrDefaultAsync();

                if (cls == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy lớp có ID = {id}."));
                }

                return Ok(ApiResponse<object>.Success("Lấy thông tin lớp thành công.", cls));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        // POST: api/Classes
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", errors));
            }

            try
            {
                var isExist = await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName);
                if (isExist)
                {
                    return Conflict(ApiResponse<object>.Fail($"Lớp '{dto.ClassName}' đã tồn tại."));
                }

                var newClass = dto.ToClass();

                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                return StatusCode(201, ApiResponse<object>.Success("Tạo lớp thành công.", new
                {
                    newClass.ClassId,
                    newClass.ClassName
                }));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi tạo lớp: {inner}"));
            }
        }

        // PUT: api/Classes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassUpdateDto dto)
        {
            if (id != dto.ClassId)
            {
                return BadRequest(ApiResponse<object>.Fail("ID không khớp giữa URL và dữ liệu gửi lên."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", errors));
            }

            try
            {
                var existingClass = await _context.Classes.FindAsync(id);
                if (existingClass == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy lớp có ID = {id}."));
                }

                bool isDuplicate = await _context.Classes
                    .AnyAsync(c => c.ClassName == dto.ClassName && c.ClassId != id);

                if (isDuplicate)
                {
                    return Conflict(ApiResponse<object>.Fail($"Tên lớp '{dto.ClassName}' đã tồn tại."));
                }

                existingClass.ClassName = dto.ClassName;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success("Cập nhật lớp thành công."));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi khi cập nhật database: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}
