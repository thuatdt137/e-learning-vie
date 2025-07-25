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
        public async Task<IActionResult> GetAllClasses([FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? keyWord)
        {
            try
            {
                var query = _context.Classes
                    .AsNoTracking()
                    .Include(c => c.Grade)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyWord))
                {
                    string keywordLower = keyWord.ToLower().Trim();
                    query = query.Where(c =>
                        (c.ClassName != null && c.ClassName.ToLower().Contains(keywordLower)) ||
                        (c.Grade != null && c.Grade.GradeName != null && c.Grade.GradeName.ToLower().Contains(keywordLower))
                    );
                }

                var totalItems = await query.CountAsync();

                // FIX CS0117: Sửa lại logic phân trang để chỉ dùng hàm GetPagingParameters
                // 1. Lấy các tham số phân trang đã được chuẩn hóa
                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);

                // 2. Sắp xếp và áp dụng Skip/Take trên CSDL
                var pagedQuery = query.OrderBy(c => c.Grade!.GradeId).ThenBy(c => c.ClassName)
                                      .Skip((effectivePageNumber - 1) * effectivePageSize)
                                      .Take(effectivePageSize);

                // 3. Thực thi truy vấn và map sang DTO
                var classDtos = await pagedQuery.Select(c => new ClassListDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    GradeName = c.Grade!.GradeName
                }).ToListAsync();


                if (totalItems == 0)
                {
                    return NotFound(ApiResponse<object>.Fail("No classes found!"));
                }

                // 4. Tạo đối tượng trả về với các tham số đã tính toán
                var paginatedResponse = new PaginatedResponse<ClassListDto>(
                    classDtos,
                    totalItems,
                    effectivePageNumber,
                    effectivePageSize
                );

                return Ok(ApiResponse<object>.Success("Get class list success", paginatedResponse));
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
                    .AsNoTracking()
                    .Include(c => c.Grade)
                    .Where(c => c.ClassId == id)
                    .Select(c => new ClassDetailDto
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName,
                        GradeId = c.GradeId,
                        GradeName = c.Grade!.GradeName,
                        StudentCount = c.ClassSessions
                                        .OrderByDescending(cs => cs.Semester.StartDate)
                                        .FirstOrDefault()!
                                        .Enrollments.Count()
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
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", ModelState));
            }

            try
            {
                if (await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName))
                {
                    return Conflict(ApiResponse<object>.Fail($"Lớp '{dto.ClassName}' đã tồn tại."));
                }

                if (!await _context.Grades.AnyAsync(g => g.GradeId == dto.GradeId))
                {
                    return BadRequest(ApiResponse<object>.Fail($"Khối có ID '{dto.GradeId}' không tồn tại."));
                }

                var newClass = new Class
                {
                    ClassName = dto.ClassName,
                    GradeId = dto.GradeId
                };

                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                var resultDto = new ClassListDto
                {
                    ClassId = newClass.ClassId,
                    ClassName = newClass.ClassName,
                    GradeName = (await _context.Grades.FindAsync(newClass.GradeId))?.GradeName
                };

                return CreatedAtAction(nameof(GetClassById), new { id = newClass.ClassId },
                    ApiResponse<object>.Success("Tạo lớp thành công.", resultDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi tạo lớp: {ex.Message}"));
            }
        }

        // PUT: api/Classes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassUpdateDto dto)
        {
            if (id != dto.ClassId || !ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ hoặc ID không khớp."));
            }

            try
            {
                var existingClass = await _context.Classes.FindAsync(id);
                if (existingClass == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy lớp có ID = {id}."));
                }

                if (await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName && c.ClassId != id))
                {
                    return Conflict(ApiResponse<object>.Fail($"Tên lớp '{dto.ClassName}' đã tồn tại."));
                }

                if (!await _context.Grades.AnyAsync(g => g.GradeId == dto.GradeId))
                {
                    return BadRequest(ApiResponse<object>.Fail($"Khối có ID '{dto.GradeId}' không tồn tại."));
                }

                existingClass.ClassName = dto.ClassName;
                existingClass.GradeId = dto.GradeId;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success("Cập nhật lớp thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}