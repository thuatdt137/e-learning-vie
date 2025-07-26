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
                    .Include(c => c.ClassSessions)
                        .ThenInclude(cs => cs.HomeroomTeacher)
                    .Include(c => c.ClassSessions)
                        .ThenInclude(cs => cs.Semester)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyWord))
                {
                    string keywordLower = keyWord.ToLower().Trim();
                    query = query.Where(c =>
                        (c.ClassName != null && c.ClassName.ToLower().Contains(keywordLower)) ||
                        (c.Grade != null && c.Grade.GradeName != null && c.Grade.GradeName.ToLower().Contains(keywordLower))
                    );
                }

                // Đếm tổng số mục trước khi phân trang
                var totalItems = await query.CountAsync();

                // FIX: Sử dụng PagingUtil của bạn để thực hiện phân trang thủ công
                // 1. Lấy các tham số phân trang đã được chuẩn hóa
                var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);

                // 2. Sắp xếp và áp dụng Skip/Take trên CSDL
                var pagedEntities = await query.OrderBy(c => c.Grade!.GradeId).ThenBy(c => c.ClassName)
                                               .Skip((effectivePageNumber - 1) * effectivePageSize)
                                               .Take(effectivePageSize)
                                               .ToListAsync();

                // 3. Map sang DTO sau khi đã lấy dữ liệu
                var classDtos = pagedEntities.Select(c => new ClassListDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    GradeName = c.Grade?.GradeName,
                    HomeroomTeacherName = c.ClassSessions
                                           .OrderByDescending(cs => cs.Semester.StartDate)
                                           .Select(cs => cs.HomeroomTeacher != null ? $"{cs.HomeroomTeacher.FirstName} {cs.HomeroomTeacher.LastName}" : null)
                                           .FirstOrDefault()
                }).ToList();

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
        // GET: api/Classes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            try
            {
                // BƯỚC 1: Lấy thông tin cơ bản của lớp học trước
                var classInfo = await _context.Classes
                    .AsNoTracking()
                    .Include(c => c.Grade)
                    .Where(c => c.ClassId == id)
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        c.GradeId,
                        GradeName = c.Grade!.GradeName
                    })
                    .FirstOrDefaultAsync();

                if (classInfo == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy lớp có ID = {id}."));
                }

                // BƯỚC 2: Tìm ClassSession mới nhất một cách riêng biệt
                var latestSession = await _context.ClassSessions
                    .AsNoTracking()
                    .Where(cs => cs.ClassId == id)
                    .OrderByDescending(cs => cs.Semester.StartDate)
                    .FirstOrDefaultAsync();

                int studentCount = 0;
                string? teacherName = "Chưa có";

                // BƯỚC 3: Nếu có session, thực hiện truy vấn đếm học sinh và lấy tên GVCN
                if (latestSession != null)
                {
                    // Đếm sĩ số bằng một truy vấn riêng, đơn giản
                    studentCount = await _context.Enrollments
                        .CountAsync(e => e.ClassSessionId == latestSession.ClassSessionId);

                    // Lấy tên GVCN bằng một truy vấn riêng
                    teacherName = await _context.ClassSessions
                        .Where(cs => cs.ClassSessionId == latestSession.ClassSessionId)
                        .Select(cs => cs.HomeroomTeacher != null ? $"{cs.HomeroomTeacher.FirstName} {cs.HomeroomTeacher.LastName}" : "Chưa có")
                        .FirstOrDefaultAsync();
                }

                // BƯỚC 4: Tổng hợp kết quả vào DTO
                var classDetailDto = new ClassDetailDto
                {
                    ClassId = classInfo.ClassId,
                    ClassName = classInfo.ClassName,
                    GradeId = classInfo.GradeId,
                    GradeName = classInfo.GradeName,
                    HomeroomTeacherName = teacherName,
                    StudentCount = studentCount
                };

                return Ok(ApiResponse<object>.Success("Lấy thông tin lớp thành công.", classDetailDto));
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

            // SỬA ĐỔI: Sử dụng transaction vì thao tác trên nhiều bảng
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // --- Các bước kiểm tra ---
                if (await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName))
                {
                    return Conflict(ApiResponse<object>.Fail($"Lớp '{dto.ClassName}' đã tồn tại."));
                }

                if (!await _context.Grades.AnyAsync(g => g.GradeId == dto.GradeId))
                {
                    return BadRequest(ApiResponse<object>.Fail($"Khối có ID '{dto.GradeId}' không tồn tại."));
                }

                // SỬA ĐỔI: Kiểm tra giáo viên có tồn tại không nếu được cung cấp
                if (dto.TeacherId.HasValue && !await _context.Teachers.AnyAsync(t => t.TeacherId == dto.TeacherId.Value))
                {
                    return BadRequest(ApiResponse<object>.Fail($"Giáo viên có ID '{dto.TeacherId}' không tồn tại."));
                }

                // --- Tạo lớp mới ---
                var newClass = new Class
                {
                    ClassName = dto.ClassName,
                    GradeId = dto.GradeId
                };
                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync(); // Lưu để lấy ClassId

                // SỬA ĐỔI: Nếu có TeacherId, tạo luôn ClassSession cho học kỳ hiện tại
                if (dto.TeacherId.HasValue)
                {
                    var currentDate = DateOnly.FromDateTime(DateTime.Now);
                    var currentSemester = await _context.Semesters
                        .FirstOrDefaultAsync(s => s.StartDate <= currentDate && s.EndDate >= currentDate);

                    if (currentSemester == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(ApiResponse<object>.Fail("Không tìm thấy học kỳ hiện tại để gán giáo viên chủ nhiệm."));
                    }

                    var newClassSession = new ClassSession
                    {
                        ClassId = newClass.ClassId,
                        TeacherId = dto.TeacherId.Value,
                        SemesterId = currentSemester.SemesterId
                    };
                    _context.ClassSessions.Add(newClassSession);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync(); // Hoàn tất transaction

                var result = await GetClassById(newClass.ClassId) as OkObjectResult;
                return CreatedAtAction(nameof(GetClassById), new { id = newClass.ClassId }, result?.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingClass = await _context.Classes.FindAsync(id);
                if (existingClass == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy lớp có ID = {id}."));
                }

                // --- Các bước kiểm tra ---
                if (await _context.Classes.AnyAsync(c => c.ClassName == dto.ClassName && c.ClassId != id))
                {
                    return Conflict(ApiResponse<object>.Fail($"Tên lớp '{dto.ClassName}' đã tồn tại."));
                }

                if (!await _context.Grades.AnyAsync(g => g.GradeId == dto.GradeId))
                {
                    return BadRequest(ApiResponse<object>.Fail($"Khối có ID '{dto.GradeId}' không tồn tại."));
                }

                if (dto.TeacherId.HasValue && !await _context.Teachers.AnyAsync(t => t.TeacherId == dto.TeacherId.Value))
                {
                    return BadRequest(ApiResponse<object>.Fail($"Giáo viên có ID '{dto.TeacherId}' không tồn tại."));
                }

                // --- Cập nhật thông tin lớp ---
                existingClass.ClassName = dto.ClassName;
                existingClass.GradeId = dto.GradeId;

                // SỬA ĐỔI: Cập nhật hoặc tạo mới ClassSession cho GVCN
                if (dto.TeacherId.HasValue)
                {
                    var currentDate = DateOnly.FromDateTime(DateTime.Now);
                    var currentSemester = await _context.Semesters
                        .FirstOrDefaultAsync(s => s.StartDate <= currentDate && s.EndDate >= currentDate);

                    if (currentSemester == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(ApiResponse<object>.Fail("Không tìm thấy học kỳ hiện tại để cập nhật giáo viên chủ nhiệm."));
                    }

                    var currentClassSession = await _context.ClassSessions
                        .FirstOrDefaultAsync(cs => cs.ClassId == id && cs.SemesterId == currentSemester.SemesterId);

                    if (currentClassSession != null)
                    {
                        // Nếu đã có session, chỉ cập nhật lại TeacherId
                        currentClassSession.TeacherId = dto.TeacherId.Value;
                    }
                    else
                    {
                        // Nếu chưa có, tạo mới
                        var newClassSession = new ClassSession
                        {
                            ClassId = id,
                            TeacherId = dto.TeacherId.Value,
                            SemesterId = currentSemester.SemesterId
                        };
                        _context.ClassSessions.Add(newClassSession);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.Success("Cập nhật lớp thành công."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}