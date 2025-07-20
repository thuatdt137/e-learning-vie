using e_learning_vie.DTOs.TeachersDto;
using e_learning_vie.Models;
using e_learning_vie.Commons;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using e_learning_vie.DTOs.School;

namespace e_learning_vie.Controllers.TeachersManagement
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public TeacherController(SchoolManagementContext context)
        {
            _context = context;
        }

        // GET: api/Teacher
        //[Authorize(Roles = "MinistryOfEducation")]
        [HttpGet]
        public async Task<ActionResult> GetTeachers(
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] string? keyWord)
        {
            try
            {
                keyWord = keyWord?.Trim() ?? "";

                var teachers = await _context.Teachers
                .Select(t => new TeachersDto
                {
                    TeacherId = t.TeacherId,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    DateOfBirth = t.DateOfBirth,
                    Address = t.Address,
                    Phone = t.Phone,
                    Email = t.Email,
                    SchoolId = t.SchoolId
                })
                .ToListAsync();

                if (!string.IsNullOrEmpty(keyWord))
                {
                    teachers = teachers.Where(s => s.FirstName.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.LastName.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.Address.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.Phone.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                                  s.Email.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (pageNumber.HasValue || pageSize.HasValue)
                {
                    var totalItems = await _context.Teachers.CountAsync();

                    var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);
                    teachers = teachers
                        .Skip((effectivePageNumber - 1) * effectivePageSize)
                        .Take(effectivePageSize)
                        .ToList();
                    if (teachers != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get teachers list success", new PaginatedResponse<TeachersDto>(teachers, totalItems, effectivePageNumber, effectivePageSize)));
                    }
                }

                if (teachers == null || !teachers.Any())
                {
                    return StatusCode(StatusCodes.Status404NotFound, ApiResponse<object>.Fail("No teacher found!"));
                }
                return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success("Get teacher list success", teachers));

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Error($"An error occurred {ex.Message}"));
            }
        }

        // GET: api/Teacher/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TeachersDto>> GetTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên.", null));

            var dto = new TeachersDto
            {
                TeacherId = teacher.TeacherId,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                DateOfBirth = teacher.DateOfBirth,
                Address = teacher.Address,
                Phone = teacher.Phone,
                Email = teacher.Email,
                SchoolId = teacher.SchoolId
            };

            return Ok(ApiResponse<TeachersDto>.Success("Thông tin giáo viên", dto));
        }

        // POST: api/Teacher
        [HttpPost]
        //[Authorize(Roles = "MinistryOfEducation")]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto dto)
        {
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var teacher = new Teacher
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    SchoolId = dto.SchoolId
                };

                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var resultDto = new TeachersDto
                {
                    TeacherId = teacher.TeacherId,
                    FirstName = teacher.FirstName,
                    LastName = teacher.LastName,
                    DateOfBirth = teacher.DateOfBirth,
                    Address = teacher.Address,
                    Phone = teacher.Phone,
                    Email = teacher.Email,
                    SchoolId = teacher.SchoolId
                };

                return StatusCode(201, ApiResponse<TeachersDto>.Success(
                    "Tạo giáo viên thành công.",
                    resultDto
                ));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi khi tạo giáo viên.", null));
            }
        }

        // PUT: api/Teacher/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "MinistryOfEducation")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] TeachersDto dto)
        {
            if (id != dto.TeacherId)
                return BadRequest(ApiResponse<object>.Fail("Sai mã giáo viên.", null));

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên.", null));

            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.DateOfBirth = dto.DateOfBirth;
            teacher.Address = dto.Address;
            teacher.Phone = dto.Phone;
            teacher.Email = dto.Email;
            teacher.SchoolId = dto.SchoolId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên.", null));
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Teacher/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "MinistryOfEducation")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên.", null));

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Teacher/by-school/3
        [HttpGet("by-school/{schoolId}")]
        public async Task<IActionResult> GetTeachersBySchool(int schoolId)
        {
            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId)
                .Select(t => new TeachersDto
                {
                    TeacherId = t.TeacherId,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    DateOfBirth = t.DateOfBirth,
                    Address = t.Address,
                    Phone = t.Phone,
                    Email = t.Email,
                    SchoolId = t.SchoolId
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TeachersDto>>.Success(
                $"Danh sách giáo viên của trường {schoolId}",
                teachers
            ));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }

        // EXPORT EXCEL
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportTeachersToExcel()
        {
            var teachers = await _context.Teachers.AsNoTracking().ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Teachers");

            // Header
            worksheet.Cells[1, 1].Value = "TeacherId";
            worksheet.Cells[1, 2].Value = "FirstName";
            worksheet.Cells[1, 3].Value = "LastName";
            worksheet.Cells[1, 4].Value = "DateOfBirth";
            worksheet.Cells[1, 5].Value = "Address";
            worksheet.Cells[1, 6].Value = "Phone";
            worksheet.Cells[1, 7].Value = "Email";
            worksheet.Cells[1, 8].Value = "SchoolId";

            // Data
            for (int i = 0; i < teachers.Count; i++)
            {
                var t = teachers[i];
                worksheet.Cells[i + 2, 1].Value = t.TeacherId;
                worksheet.Cells[i + 2, 2].Value = t.FirstName;
                worksheet.Cells[i + 2, 3].Value = t.LastName;
                worksheet.Cells[i + 2, 4].Value = t.DateOfBirth?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 5].Value = t.Address;
                worksheet.Cells[i + 2, 6].Value = t.Phone;
                worksheet.Cells[i + 2, 7].Value = t.Email;
                worksheet.Cells[i + 2, 8].Value = t.SchoolId;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"Teachers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        // IMPORT EXCEL
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportTeachersFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("File không hợp lệ.", null));

            var teachers = new List<Teacher>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // Đảm bảo stream ở đầu
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return BadRequest(ApiResponse<object>.Fail("Không tìm thấy worksheet.", null));

                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++) 
                {
                    try
                    {
                        DateOnly? dateOfBirth = null;
                        if (DateTime.TryParse(worksheet.Cells[row, 4].Text, out var dob))
                        {
                            dateOfBirth = DateOnly.FromDateTime(dob);
                        }

                        var teacher = new Teacher
                        {
                            FirstName = worksheet.Cells[row, 2].Text,
                            LastName = worksheet.Cells[row, 3].Text,
                            DateOfBirth = dateOfBirth,
                            Address = worksheet.Cells[row, 5].Text,
                            Phone = worksheet.Cells[row, 6].Text,
                            Email = worksheet.Cells[row, 7].Text,
                            SchoolId = int.TryParse(worksheet.Cells[row, 8].Text, out var schoolId) ? schoolId : 0
                        };
                        teachers.Add(teacher);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if (teachers.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Không có dữ liệu hợp lệ để import.", null));

            await _context.Teachers.AddRangeAsync(teachers);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Success($"Đã import {teachers.Count} giáo viên thành công.", null));
        }
    }
}
