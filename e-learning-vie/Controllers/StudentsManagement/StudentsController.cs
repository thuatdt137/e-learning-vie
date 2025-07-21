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
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

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
        [HttpPost("import-students")]
        public async Task<IActionResult> ImportStudentsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("File không hợp lệ.", null));

            var studentDtos = new List<StudentImportDto>();
            var errorLog = new StringBuilder();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return BadRequest(ApiResponse<object>.Fail("Không tìm thấy worksheet.", null));

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string dobText = worksheet.Cells[row, 5].Text;
                        DateOnly? dateOfBirth = null;
                        if (!string.IsNullOrWhiteSpace(dobText) && DateTime.TryParse(dobText, out var dob))
                        {
                            dateOfBirth = DateOnly.FromDateTime(dob);
                        }
                        else if (!string.IsNullOrWhiteSpace(dobText))
                        {
                            errorLog.AppendLine($"Dòng {row}: Ngày sinh không hợp lệ - '{dobText}'.");
                            continue;
                        }

                        var dto = new StudentImportDto
                        {
                            IdentityCode = worksheet.Cells[row, 2].Text,
                            FirstName = worksheet.Cells[row, 3].Text,
                            LastName = worksheet.Cells[row, 4].Text,
                            DateOfBirth = dateOfBirth, 
                            Address = worksheet.Cells[row, 6].Text,
                            Phone = worksheet.Cells[row, 7].Text,
                            Email = worksheet.Cells[row, 8].Text,
                            ClassId = int.TryParse(worksheet.Cells[row, 9].Text, out var classId) ? classId : null,
                            SchoolId = int.TryParse(worksheet.Cells[row, 10].Text, out var schoolId) ? schoolId : null
                        };

                        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                            string.IsNullOrWhiteSpace(dto.LastName) ||
                            dto.DateOfBirth == null)
                        {
                            errorLog.AppendLine($"Dòng {row}: Thiếu thông tin bắt buộc.");
                            continue;
                        }

                        studentDtos.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        errorLog.AppendLine($"Dòng {row}: Lỗi không xác định - {ex.Message}");
                        continue;
                    }
                }
            }

            if (studentDtos.Count == 0)
            {
                var errorMessage = "Không có dữ liệu hợp lệ để import.";
                return BadRequest(ApiResponse<object>.Fail(errorMessage, new { Errors = errorLog.ToString() }));
            }

            var students = studentDtos.Select(dto => new Student
            {
                IdentityCode = dto.IdentityCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                ClassId = dto.ClassId,
                SchoolId = dto.SchoolId
            }).ToList();

            try
            {
                await _context.Students.AddRangeAsync(students);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<object>.Fail("Lỗi khi lưu vào cơ sở dữ liệu.", errorMessage));
            }

            var message = $"Đã import {students.Count} học sinh thành công.";
            return Ok(ApiResponse<object>.Success(message, new { Errors = errorLog.ToString() }));
        }

    }
}
