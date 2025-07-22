//using e_learning_vie.Commons;
//using e_learning_vie.DTOs.classes;
//using e_learning_vie.Models;
//using e_learning_vie.Utils;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace e_learning_vie.Controllers.ClassesManagement
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ClassesController : ControllerBase
//    {
//        private readonly SchoolManagementContext _context;

//        public ClassesController(SchoolManagementContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult GetAllClasses(int? pageNumber, int? pageSize, string? keyWord, int? schoolId)
//        {
//            try
//            {
//                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);

//                keyWord = keyWord?.Trim() ?? "";

//                var classes = _context.Classes.Select(c => new ClassListDto(c)).ToList();

//                if(schoolId != null)
//                {
//                    classes = classes.Where(c => c.SchoolId == schoolId).ToList();
//                }

//                if(!string.IsNullOrEmpty(keyWord))
//                {
//                    classes = classes.Where(s => s.ClassName.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToList();
//                }

//                if(pageSize != null && pageNumber != null)
//                {
//                    classes = classes
//                    .Skip((effectivePageNumber - 1) * effectivePageSize)
//                    .Take(effectivePageSize)
//                    .ToList();
//                }

//                if(classes == null || !classes.Any())
//                {
//                    return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Fail("No classes found!"));
//                }
//                return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get class list success", classes));
//            }
//            catch(Exception ex)
//            {
//                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Error($"An error occurred {ex.Message}"));
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
//        {
//            if(!ModelState.IsValid)
//            {
//                var errors = ModelState
//                    .Where(e => e.Value?.Errors.Count > 0)
//                    .ToDictionary(
//                        kvp => kvp.Key,
//                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
//                    );
//                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
//            }

//            try
//            {
//                // Kiểm tra trùng tên lớp
//                bool isDuplicate = await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName);
//                if(isDuplicate)
//                {
//                    return Conflict(ApiResponse<object>.Fail($"Tên lớp '{dto.ClassName}' đã tồn tại."));
//                }

//                var newClass = new Class
//                {
//                    ClassName = dto.ClassName,
//                    AcademicYearId = dto.AcademicYearId,
//                    TeacherId = dto.TeacherId,
//                    SchoolId = dto.SchoolId
//                };

//                _context.Classes.Add(newClass);
//                await _context.SaveChangesAsync();

//                var resultDto = new ClassDetailsDto
//                {
//                    ClassId = newClass.ClassId,
//                    ClassName = newClass.ClassName,
//                    AcademicYearId = newClass.AcademicYearId,
//                    TeacherId = newClass.TeacherId,
//                    SchoolId = newClass.SchoolId
//                };

//                return StatusCode(201, ApiResponse<object>.Success("Tạo lớp thành công.", resultDto));
//            }
//            catch(Exception ex)
//            {
//                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while creating the class.", new { ex.Message }));
//            }
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetClassById(int id)
//        {
//            try
//            {
//                var cls = await _context.Classes.FindAsync(id);
//                if(cls == null)
//                {
//                    return NotFound(ApiResponse<object>.Fail($"Class with ID {id} not found."));

//                }
//                return Ok(new ClassListDto
//                {
//                    ClassId = cls.ClassId,
//                    ClassName = cls.ClassName,
//                    AcademicYearId = cls.AcademicYearId,
//                    TeacherId = cls.TeacherId,
//                    SchoolId = cls.SchoolId
//                });
//            }
//            catch(Exception ex)
//            {
//                return StatusCode(500, ApiResponse<object>.Error($"An error occurred while retrieving the class: {ex.Message}"));
//            }
//        }

//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassDetailsDto dto)
//        {
//            if(id != dto.ClassId)
//            {
//                return BadRequest(ApiResponse<object>.Fail("ID mismatch between route and payload."));
//            }

//            if(!ModelState.IsValid)
//            {
//                var errors = ModelState
//                    .Where(e => e.Value?.Errors.Count > 0)
//                    .ToDictionary(
//                        kvp => kvp.Key,
//                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
//                    );
//                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
//            }

//            var existingClass = await _context.Classes.FindAsync(id);
//            if(existingClass == null)
//            {
//                return NotFound(ApiResponse<object>.Fail($"Class with ID {id} not found."));
//            }

//            try
//            {
//                //Kiểm tra trùng tên lớp (loại trừ chính bản thân lớp đang cập nhật)
//                bool isDuplicate = await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName && c.ClassId != id);
//                if(isDuplicate)
//                {
//                    return Conflict(ApiResponse<object>.Fail($"Tên lớp '{dto.ClassName}' đã tồn tại."));
//                }

//                existingClass = ClassDetailsDto.map2Class(dto, existingClass);

//                await _context.SaveChangesAsync();

//                return Ok(ApiResponse<object>.Success("Cập nhật lớp thành công."));
//            }
//            catch(DbUpdateException ex)
//            {
//                return StatusCode(500, ApiResponse<object>.Fail($"Database update error: {ex.Message}"));
//            }
//            catch(Exception ex)
//            {
//                return StatusCode(500, ApiResponse<object>.Fail($"Unexpected error: {ex.Message}"));
//            }
//        }
//    }
//}
