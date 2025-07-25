using e_learning_vie.DTOs.TeachersDto;
using e_learning_vie.Models;
using e_learning_vie.Commons;
using e_learning_vie.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.AspNetCore.Identity;

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
                        IdentityCode = t.IdentityCode,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        IsMale = t.IsMale,
                        DateOfBirth = t.DateOfBirth.HasValue ? DateTime.Parse(t.DateOfBirth.Value.ToString("yyyy-MM-dd")) : null,
                        Address = t.Address,
                        Phone = t.Phone,
                        Email = t.Email
                    })
                    .ToListAsync();

                if (!string.IsNullOrEmpty(keyWord))
                {
                    teachers = teachers.Where(s =>
                        (s.IdentityCode != null && s.IdentityCode.Contains(keyWord, StringComparison.OrdinalIgnoreCase)) ||
                        (s.FirstName != null && s.FirstName.Contains(keyWord, StringComparison.OrdinalIgnoreCase)) ||
                        (s.LastName != null && s.LastName.Contains(keyWord, StringComparison.OrdinalIgnoreCase)) ||
                        (s.Address != null && s.Address.Contains(keyWord, StringComparison.OrdinalIgnoreCase)) ||
                        (s.Phone != null && s.Phone.Contains(keyWord, StringComparison.OrdinalIgnoreCase)) ||
                        (s.Email != null && s.Email.Contains(keyWord, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    var totalItems = await _context.Teachers.CountAsync();
                    var (effectivePageNumber, effectivePageSize) = PagingUtil.GetPagingParameters(pageNumber, pageSize);
                    teachers = teachers
                        .Skip((effectivePageNumber - 1) * effectivePageSize)
                        .Take(effectivePageSize)
                        .ToList();

                    if (teachers != null)
                    {
                        return StatusCode(StatusCodes.Status200OK, ApiResponse<object>.Success(
                            "Get teachers list success",
                            new PaginatedResponse<TeachersDto>(teachers, totalItems, effectivePageNumber, effectivePageSize)));
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
                IdentityCode = teacher.IdentityCode,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                IsMale = teacher.IsMale,
                DateOfBirth = teacher.DateOfBirth.HasValue ? DateTime.Parse(teacher.DateOfBirth.Value.ToString("yyyy-MM-dd")) : null,
                Address = teacher.Address,
                Phone = teacher.Phone,
                Email = teacher.Email
            };

            return Ok(ApiResponse<TeachersDto>.Success("Thông tin giáo viên", dto));
        }

        // POST: api/Teacher
        [HttpPost]
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

            var identityExists = await _context.Teachers
                .AnyAsync(t => t.IdentityCode == dto.IdentityCode);
            if (identityExists)
            {
                return Conflict(ApiResponse<object>.Fail("Mã giáo viên (IdentityCode) đã tồn tại.", null));
            }

            var emailExists = await _context.Teachers
                .AnyAsync(t => t.Email == dto.Email);
            if (emailExists)
            {
                return Conflict(ApiResponse<object>.Fail("Email đã tồn tại.", null));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var teacher = new Teacher
                {
                    IdentityCode = dto.IdentityCode,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    IsMale = dto.IsMale,
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email
                };

                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var resultDto = new TeachersDto
                {
                    TeacherId = teacher.TeacherId,
                    IdentityCode = teacher.IdentityCode,
                    FirstName = teacher.FirstName,
                    LastName = teacher.LastName,
                    IsMale = teacher.IsMale,
                    DateOfBirth = teacher.DateOfBirth.HasValue ? DateTime.Parse(teacher.DateOfBirth.Value.ToString("yyyy-MM-dd")) : null,
                    Address = teacher.Address,
                    Phone = teacher.Phone,
                    Email = teacher.Email
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
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] TeachersDto dto)
        {
            if (id != dto.TeacherId)
                return BadRequest(ApiResponse<object>.Fail("Sai mã giáo viên.", null));

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên.", null));

            var identityExists = await _context.Teachers
                .AnyAsync(t => t.IdentityCode == dto.IdentityCode && t.TeacherId != id);
            if (identityExists)
            {
                return Conflict(ApiResponse<object>.Fail("Mã giáo viên (IdentityCode) đã tồn tại.", null));
            }

            var emailExists = await _context.Teachers
                .AnyAsync(t => t.Email == dto.Email && t.TeacherId != id);
            if (emailExists)
            {
                return Conflict(ApiResponse<object>.Fail("Email đã tồn tại.", null));
            }

            teacher.IdentityCode = dto.IdentityCode;
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.IsMale = dto.IsMale;
            teacher.DateOfBirth = dto.DateOfBirth.HasValue ? DateOnly.Parse(dto.DateOfBirth.Value.ToString("yyyy-MM-dd")) : null;
            teacher.Address = dto.Address;
            teacher.Phone = dto.Phone;
            teacher.Email = dto.Email;

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

        // GET: api/Teacher/{teacherId}/weekly-schedule
        [HttpGet("{teacherId}/weekly-schedule")]
        public async Task<IActionResult> GetWeeklySchedule(int teacherId)
        {
            try
            {
                var schedules = await _context.Schedules
                    .Where(s => s.TeachingAssignment.TeacherId == teacherId)
                    .Join(_context.Slots,
                        schedule => schedule.SlotId,
                        slot => slot.SlotId,
                        (schedule, slot) => new { schedule, slot })
                    .Join(_context.Rooms,
                        s => s.schedule.RoomId,
                        room => room.RoomId,
                        (s, room) => new { s.schedule, s.slot, room })
                    .Join(_context.TeachingAssignments,
                        s => s.schedule.TeachingAssignmentId,
                        ta => ta.TeachingAssignmentId,
                        (s, ta) => new { s.schedule, s.slot, s.room, ta })
                    .Join(_context.Subjects,
                        s => s.ta.SubjectId,
                        subject => subject.SubjectId,
                        (s, subject) => new { s.schedule, s.slot, s.room, s.ta, subject })
                    .Join(_context.ClassSessions,
                        s => s.ta.ClassSessionId,
                        cs => cs.ClassSessionId,
                        (s, cs) => new { s.schedule, s.slot, s.room, s.ta, s.subject, cs })
                    .Join(_context.Classes,
                        s => s.cs.ClassId,
                        c => c.ClassId,
                        (s, c) => new
                        {
                            ScheduleDate = s.schedule.Date, // Lưu DateOnly
                            s.slot.SlotName,
                            SlotStartTime = s.slot.StartTime,
                            SlotEndTime = s.slot.EndTime,
                            s.subject.SubjectName,
                            c.ClassName,
                            s.room.Name
                        })
                    .ToListAsync(); // Thực thi truy vấn SQL trước

                var result = schedules.Select(s => new TeacherScheduleDto
                {
                    Date = s.ScheduleDate.ToDateTime(TimeOnly.MinValue), // Chuyển đổi trong bộ nhớ
                    DayOfWeek = s.ScheduleDate.DayOfWeek.ToString(),
                    SlotName = s.SlotName,
                    StartTime = TimeOnly.FromTimeSpan(s.SlotStartTime),
                    EndTime = TimeOnly.FromTimeSpan(s.SlotEndTime),
                    SubjectName = s.SubjectName,
                    ClassName = s.ClassName,
                    RoomName = s.Name
                })
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToList();

                if (!result.Any())
                {
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy lịch học cho giáo viên này."));
                }

                return Ok(ApiResponse<List<TeacherScheduleDto>>.Success("Lịch học của giáo viên", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi khi lấy lịch học: {ex.Message}"));
            }
        }

        // EXPORT EXCEL
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportTeachersToExcel()
        {
            try
            {
                var teachers = await _context.Teachers
                    .AsNoTracking()
                    .ToListAsync();

                if (teachers == null || teachers.Count == 0)
                {
                    return NotFound(ApiResponse<string>.Fail("Không có dữ liệu giáo viên để xuất."));
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Teachers");

                var headers = new[]
                {
                "TeacherId", "IdentityCode", "FirstName", "LastName", "IsMale",
                "DateOfBirth", "Address", "Phone", "Email"
            };
                for (int col = 0; col < headers.Length; col++)
                {
                    worksheet.Cells[1, col + 1].Value = headers[col];
                    worksheet.Cells[1, col + 1].Style.Font.Bold = true;
                }

                for (int i = 0; i < teachers.Count; i++)
                {
                    var t = teachers[i];
                    worksheet.Cells[i + 2, 1].Value = t.TeacherId;
                    worksheet.Cells[i + 2, 2].Value = t.IdentityCode;
                    worksheet.Cells[i + 2, 3].Value = t.FirstName;
                    worksheet.Cells[i + 2, 4].Value = t.LastName;
                    worksheet.Cells[i + 2, 5].Value = t.IsMale ? "Nam" : "Nữ";
                    worksheet.Cells[i + 2, 6].Value = t.DateOfBirth?.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 7].Value = t.Address;
                    worksheet.Cells[i + 2, 8].Value = t.Phone;
                    worksheet.Cells[i + 2, 9].Value = t.Email;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Teachers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    excelName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Lỗi hệ thống khi xuất Excel: " + ex.Message));
            }
        }

        // IMPORT EXCEL
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportTeachersFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(ApiResponse<object>.Fail("File không hợp lệ.", null));

                var teachersToImport = new List<Teacher>();
                var errorLogs = new List<string>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using var package = new ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                        return BadRequest(ApiResponse<object>.Fail("Không tìm thấy worksheet.", null));

                    int rowCount = worksheet.Dimension.Rows;

                    var existingIdentityCodes = await _context.Teachers
                        .Select(t => t.IdentityCode)
                        .ToListAsync();

                    var existingEmails = await _context.Teachers
                        .Select(t => t.Email)
                        .ToListAsync();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            string identityCode = worksheet.Cells[row, 2].Text.Trim();
                            string email = worksheet.Cells[row, 9].Text.Trim();

                            if (string.IsNullOrWhiteSpace(identityCode) || string.IsNullOrWhiteSpace(email))
                            {
                                errorLogs.Add($"Dòng {row}: Thiếu IdentityCode hoặc Email.");
                                continue;
                            }

                            if (existingIdentityCodes.Contains(identityCode))
                            {
                                errorLogs.Add($"Dòng {row}: IdentityCode \"{identityCode}\" đã tồn tại.");
                                continue;
                            }

                            if (existingEmails.Contains(email))
                            {
                                errorLogs.Add($"Dòng {row}: Email \"{email}\" đã tồn tại.");
                                continue;
                            }

                            DateOnly? dateOfBirth = null;
                            if (DateTime.TryParse(worksheet.Cells[row, 6].Text, out var dob))
                            {
                                dateOfBirth = DateOnly.FromDateTime(dob);
                            }

                            bool isMale = worksheet.Cells[row, 5].Text.Trim().ToLower() == "nam";

                            var teacher = new Teacher
                            {
                                IdentityCode = identityCode,
                                FirstName = worksheet.Cells[row, 3].Text,
                                LastName = worksheet.Cells[row, 4].Text,
                                IsMale = isMale,
                                DateOfBirth = dateOfBirth,
                                Address = worksheet.Cells[row, 7].Text,
                                Phone = worksheet.Cells[row, 8].Text,
                                Email = email
                            };

                            teachersToImport.Add(teacher);
                            existingIdentityCodes.Add(identityCode);
                            existingEmails.Add(email);
                        }
                        catch (Exception exRow)
                        {
                            errorLogs.Add($"Dòng {row}: Lỗi khi đọc dữ liệu ({exRow.Message})");
                            continue;
                        }
                    }
                }

                if (teachersToImport.Count > 0)
                {
                    await _context.Teachers.AddRangeAsync(teachersToImport);
                    await _context.SaveChangesAsync();
                }

                return Ok(ApiResponse<object>.Success(
                    $"Import thành công {teachersToImport.Count} giáo viên. {(errorLogs.Count > 0 ? "Một số dòng bị bỏ qua." : "")}",
                    new
                    {
                        SuccessCount = teachersToImport.Count,
                        ErrorCount = errorLogs.Count,
                        Errors = errorLogs
                    }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống khi xử lý file.", null));
            }
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
    }
}
