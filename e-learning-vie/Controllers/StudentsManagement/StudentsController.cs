using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Identity;
using e_learning_vie.DTOs.StudentDtos;
using e_learning_vie.Commons;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
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
        //[Authorize(Roles = "TrainingDepartment")]
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<StudentListDto>>> GetStudentsBySchool(
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            // Get validated pagination parameters
            var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);

            // Get total count
            var totalItems = await _context.Students.CountAsync();

            // Get paginated data
            var students = await _context.Students
                .AsNoTracking()
                .Select(s => new StudentListDto
                {
                    StudentId = s.StudentId,
                    IdentityCode = s.IdentityCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    ClassId = s.ClassId,
                    SchoolId = s.SchoolId
                })
                .OrderBy(s => s.StudentId) // Optional: Add sorting for consistent results
                .Skip((effectivePageNumber - 1) * effectivePageSize)
                .Take(effectivePageSize)
                .ToListAsync();

            // Create paginated response
            var response = new PaginatedResponse<StudentListDto>(
                items: students,
                totalItems: totalItems,
                pageNumber: effectivePageNumber,
                pageSize: effectivePageSize
            );

            return Ok(ApiResponse<PaginatedResponse<StudentListDto>>.Success(
                "Danh sách student",
                response
            ));
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDetailsDto>> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.Fail("Wrong Id or Student not found."));
            }

            StudentDetailsDto studentDetailsDto = new StudentDetailsDto(student);


            return Ok(ApiResponse<StudentDetailsDto>.Success("Get student successfully", studentDetailsDto));
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, [FromBody] StudentDetailsDto dto)
        {
            if (id != dto.StudentId)
            {
                return BadRequest(ApiResponse<object>.Fail("ID mismatch between route and payload."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} not found."));
            }
            try
            {
                student = StudentDetailsDto.map2Student(dto, student);

                await _context.SaveChangesAsync();
                return Ok(ApiResponse<object>.Success("Save student successfully"));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Database update error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Unexpected error: {ex.Message}"));
            }
        }

        // POST: api/Students
        [Authorize(Roles = "TrainingDepartment")]
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
        {
            // 1. Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail("Validation failed.", errors));
            }

            // 2. Create Student and User within a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create Student
                var student = dto.ToStudent();
                _context.Students.Add(student);
                await _context.SaveChangesAsync(); // Save to generate StudentId

                // Create User
                var user = new User
                {
                    UserName = dto.IdentityCode,
                    Student = student
                };

                var createUserResult = await _userManager.CreateAsync(user, "User@" + dto.IdentityCode);
                if (!createUserResult.Succeeded)
                {
                    var errors = createUserResult.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Description).ToArray()
                        );
                    return BadRequest(ApiResponse<object>.Fail("Không tạo được tài khoản người dùng.", errors));
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, "Student");

                // Commit transaction
                await transaction.CommitAsync();

                // 3. Return success response
                return StatusCode(201, ApiResponse<object>.Success(
                    "Tạo student thành công.",
                    new
                    {
                        student.StudentId,
                        student.FirstName,
                        student.LastName,
                        student.IdentityCode,
                        user.Id
                    }
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while creating the student.", null));
            }
        }


        [HttpGet("test-cause-error")]
        public IActionResult CauseError()
        {
            int a = 0;
            int result = 1 / a;

            return Ok(result);
        }

        [HttpGet("by-school/{schoolId}")]
        public async Task<IActionResult> GetStudentsBySchool(int schoolId)
        {
            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId)
                .Select(s => new StudentListDto
                {
                    StudentId = s.StudentId,
                    IdentityCode = s.IdentityCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    SchoolId = s.SchoolId,
                    ClassId = s.ClassId
                })
                .ToListAsync();

            return Ok(ApiResponse<List<StudentListDto>>.Success(
                $"Danh sách học sinh của trường {schoolId}",
                students
            ));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        //Import Excel
        [HttpPost("import-excel")]
        [Authorize(Roles = "TrainingDepartment")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportStudentsFromExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("File không hợp lệ hoặc rỗng.", null));
            }

            var studentsToAdd = new List<Student>();
            var rowErrors = new List<string>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using var package = new ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                    {
                        return BadRequest(ApiResponse<object>.Fail("Không tìm thấy worksheet trong file.", null));
                    }

                    int totalRows = worksheet.Dimension.Rows;

                    // Lấy danh sách IdentityCode đã tồn tại để tránh trùng
                    var existingIdentityCodes = _context.Students
                        .Select(s => s.IdentityCode)
                        .ToHashSet();

                    for (int row = 2; row <= totalRows; row++) 
                    {
                        try
                        {
                            // Bỏ qua dòng trắng
                            bool isRowEmpty = Enumerable.Range(1, 5)
                                .All(col => string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text));

                            if (isRowEmpty)
                            {
                                continue;
                            }

                            string identityCode = worksheet.Cells[row, 1].Text?.Trim();
                            string firstName = worksheet.Cells[row, 2].Text?.Trim();
                            string lastName = worksheet.Cells[row, 3].Text?.Trim();
                            int? classId = int.TryParse(worksheet.Cells[row, 4].Text, out int cid) ? cid : (int?)null;
                            int? schoolId = int.TryParse(worksheet.Cells[row, 5].Text, out int sid) ? sid : (int?)null;

                            // Kiểm tra dữ liệu bắt buộc
                            if (string.IsNullOrEmpty(identityCode))
                            {
                                rowErrors.Add($"Dòng {row}: Thiếu IdentityCode.");
                                continue;
                            }

                            if (existingIdentityCodes.Contains(identityCode))
                            {
                                rowErrors.Add($"Dòng {row}: IdentityCode '{identityCode}' đã tồn tại.");
                                continue;
                            }

                            var student = new Student
                            {
                                IdentityCode = identityCode,
                                FirstName = firstName,
                                LastName = lastName,
                                ClassId = classId,
                                SchoolId = schoolId
                            };

                            studentsToAdd.Add(student);
                        }
                        catch (Exception ex)
                        {
                            rowErrors.Add($"Dòng {row}: Lỗi không xác định ({ex.Message}).");
                            continue;
                        }
                    }
                }

                if (studentsToAdd.Count == 0)
                {
                    return BadRequest(ApiResponse<object>.Fail("Không có dữ liệu hợp lệ để import.", new { Errors = rowErrors }));
                }

                // Lưu vào DB
                await _context.Students.AddRangeAsync(studentsToAdd);
                await _context.SaveChangesAsync();

                var response = new
                {
                    SuccessCount = studentsToAdd.Count,
                    ErrorCount = rowErrors.Count,
                    Errors = rowErrors
                };

                return Ok(ApiResponse<object>.Success($"{studentsToAdd.Count} học sinh đã được import thành công.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi trong quá trình import: {ex.Message}", null));
            }
        }


    }
}
