using e_learning_vie.Commons;
using e_learning_vie.DTOs.StudentDtos;
using e_learning_vie.Models;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        private readonly UserManager<User> _userManager;

        public StudentsController(SchoolManagementContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<StudentListDto>>> GetStudents(
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] string? keyword)
        {
            var query = _context.Students.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.ToLower().Trim();
                query = query.Where(s =>
                    (s.FirstName != null && s.FirstName.ToLower().Contains(kw)) ||
                    (s.LastName != null && s.LastName.ToLower().Contains(kw)) ||
                    (s.IdentityCode != null && s.IdentityCode.ToLower().Contains(kw))
                );
            }

            var totalItems = await query.CountAsync();
            var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);
            var pagedEntities = await query.OrderBy(s => s.FirstName)
                                           .Skip((effectivePageNumber - 1) * effectivePageSize)
                                           .Take(effectivePageSize)
                                           .ToListAsync();

            var studentDtos = pagedEntities.Select(s => new StudentListDto
            {
                StudentId = s.StudentId,
                IdentityCode = s.IdentityCode,
                FullName = $"{s.FirstName} {s.LastName}",
                IsMale = s.IsMale,
                DateOfBirth = s.DateOfBirth.HasValue ? s.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue) : null,
                CurrentClassName = _context.Enrollments
                                    .Where(e => e.StudentId == s.StudentId)
                                    .OrderByDescending(e => e.ClassSession.Semester.StartDate)
                                    .Select(e => e.ClassSession.Class.ClassName)
                                    .FirstOrDefault()
            }).ToList();

            var paginatedResponse = new PaginatedResponse<StudentListDto>(studentDtos, totalItems, effectivePageNumber, effectivePageSize);
            return Ok(ApiResponse<object>.Success("Lấy danh sách học sinh thành công", paginatedResponse));
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDetailsDto>> GetStudentById(int id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .Include(s => s.StudentParents).ThenInclude(sp => sp.Parent)
                .Include(s => s.Enrollments).ThenInclude(e => e.ClassSession).ThenInclude(cs => cs.Class)
                .Include(s => s.Enrollments).ThenInclude(e => e.ClassSession).ThenInclude(cs => cs.HomeroomTeacher)
                .Include(s => s.Enrollments).ThenInclude(e => e.ClassSession).ThenInclude(cs => cs.Semester)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy học sinh."));
            }

            var latestEnrollment = student.Enrollments.OrderByDescending(e => e.ClassSession.Semester.StartDate).FirstOrDefault();

            var studentDetails = new StudentDetailsDto
            {
                StudentId = student.StudentId,
                IdentityCode = student.IdentityCode,
                FirstName = student.FirstName,
                LastName = student.LastName,
                IsMale = student.IsMale,
                DateOfBirth = student.DateOfBirth.HasValue ? student.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue) : null,
                Address = student.Address,
                Phone = student.Phone,
                Email = student.Email,
                CurrentClassName = latestEnrollment?.ClassSession.Class.ClassName,
                HomeroomTeacherName = latestEnrollment?.ClassSession.HomeroomTeacher != null ? $"{latestEnrollment.ClassSession.HomeroomTeacher.FirstName} {latestEnrollment.ClassSession.HomeroomTeacher.LastName}" : null,
                Parents = student.StudentParents.Select(sp => new ParentInfoDto
                {
                    FullName = $"{sp.Parent.FirstName} {sp.Parent.LastName}",
                    Phone = sp.Parent.Phone,
                    Relationship = sp.RelationalName
                }).ToList()
            };

            return Ok(ApiResponse<StudentDetailsDto>.Success("Lấy thông tin học sinh thành công", studentDetails));
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, [FromBody] StudentUpdateDto dto)
        {
            if (id != dto.StudentId)
            {
                return BadRequest(ApiResponse<object>.Fail("ID không khớp."));
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy học sinh."));
            }

            if (!string.IsNullOrEmpty(dto.IdentityCode) && await _context.Students.AnyAsync(s => s.IdentityCode == dto.IdentityCode && s.StudentId != id))
            {
                return Conflict(ApiResponse<object>.Fail($"Mã định danh '{dto.IdentityCode}' đã tồn tại."));
            }

            student.IdentityCode = dto.IdentityCode;
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.IsMale = dto.IsMale;
            student.DateOfBirth = dto.DateOfBirth.HasValue ? DateOnly.FromDateTime(dto.DateOfBirth.Value) : null;
            student.Address = dto.Address;
            student.Phone = dto.Phone;
            student.Email = dto.Email;

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<object>.Success("Cập nhật thông tin học sinh thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi khi cập nhật: {ex.Message}"));
            }
        }

        // POST: api/Students
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", ModelState));

            if (await _context.Students.AnyAsync(s => s.IdentityCode == dto.IdentityCode))
                return Conflict(ApiResponse<object>.Fail($"Mã định danh '{dto.IdentityCode}' đã tồn tại."));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var student = new Student
                {
                    IdentityCode = dto.IdentityCode,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    IsMale = dto.IsMale,
                    // FIX CS0029: Chuyển đổi DateTime? (DTO) sang DateOnly? (model)
                    DateOfBirth = dto.DateOfBirth.HasValue ? DateOnly.FromDateTime(dto.DateOfBirth.Value) : null,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email,
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                var user = new User
                {
                    UserName = dto.IdentityCode,
                    Email = dto.Email,
                    StudentId = student.StudentId,
                    IsActive = true
                };
                var createUserResult = await _userManager.CreateAsync(user, "User@" + dto.IdentityCode);
                if (!createUserResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(ApiResponse<object>.Fail("Không tạo được tài khoản người dùng.", createUserResult.Errors));
                }
                await _userManager.AddToRoleAsync(user, "Student");

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetStudentById), new { id = student.StudentId }, ApiResponse<object>.Success("Tạo học sinh và tài khoản thành công."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        //Import Excel
        [HttpPost("import-students")]
        public async Task<IActionResult> ImportStudentsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file Excel."));

            var studentsToImport = new List<Student>();
            var errorLogs = new List<string>();

            // FIX CS1061: Dùng ToListAsync() rồi tạo HashSet từ danh sách đó
            var existingCodes = new HashSet<string>(await _context.Students.Select(s => s.IdentityCode!).ToListAsync());

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) return BadRequest(ApiResponse<object>.Fail("File Excel không có worksheet."));

            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                try
                {
                    var identityCode = worksheet.Cells[row, 1].Text.Trim();
                    if (string.IsNullOrEmpty(identityCode) || existingCodes.Contains(identityCode))
                    {
                        errorLogs.Add($"Dòng {row}: Mã định danh '{identityCode}' trống hoặc đã tồn tại.");
                        continue;
                    }

                    DateTime.TryParse(worksheet.Cells[row, 5].Text, out var dob);
                    bool.TryParse(worksheet.Cells[row, 4].Text, out var isMale);

                    studentsToImport.Add(new Student
                    {
                        IdentityCode = identityCode,
                        FirstName = worksheet.Cells[row, 2].Text.Trim(),
                        LastName = worksheet.Cells[row, 3].Text.Trim(),
                        IsMale = isMale,
                        DateOfBirth = dob == DateTime.MinValue ? null : DateOnly.FromDateTime(dob),
                        Address = worksheet.Cells[row, 6].Text.Trim(),
                        Phone = worksheet.Cells[row, 7].Text.Trim(),
                        Email = worksheet.Cells[row, 8].Text.Trim(),
                    });
                    existingCodes.Add(identityCode);
                }
                catch (Exception ex) { errorLogs.Add($"Dòng {row}: Lỗi - {ex.Message}"); }
            }

            if (studentsToImport.Any())
            {
                await _context.Students.AddRangeAsync(studentsToImport);
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Success(
                $"Import hoàn tất. Thêm thành công {studentsToImport.Count} học sinh.",
                new { SuccessCount = studentsToImport.Count, ErrorCount = errorLogs.Count, Errors = errorLogs }
            ));
        }


        //Export Excel
        [HttpGet("export-students")]
        public async Task<IActionResult> ExportStudentsToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var students = await _context.Students.AsNoTracking().ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");
            worksheet.Cells["A1"].LoadFromCollection(students, true);
            worksheet.Column(5).Style.Numberformat.Format = "yyyy-mm-dd";
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students.xlsx");
        }
    }
}