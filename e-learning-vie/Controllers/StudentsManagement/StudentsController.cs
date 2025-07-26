using e_learning_vie.Commons;
using e_learning_vie.DTOs.StudentDtos;
using e_learning_vie.Models;
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
        //[Authorize(Roles = "TrainingDepartment")]
        [HttpGet]
        public async Task<ActionResult> GetStudents(
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            int? classId, int? semesterId)
        {

            if(semesterId == null)
                semesterId = _context.Semesters.OrderByDescending(s => s.StartDate).Take(1).Select(s => s.SemesterId).FirstOrDefault();
            if(semesterId == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy học kỳ hiện tại."));

            var students = _context.Students.Include(s => s.Enrollments).ThenInclude(s => s.ClassSession).ThenInclude(s => s.Semester)
                .Include(s => s.Enrollments).ThenInclude(s => s.ClassSession).ThenInclude(s => s.Class)
                .Where(s => s.Enrollments.Any(e => e.ClassSession.SemesterId == semesterId));

            if(classId.HasValue)
            {
                students = students.Where(s => s.Enrollments.Any(e => e.ClassSession.ClassId == classId));
            }

            var result = await students
                .Select(s => new
                {
                    StudentId = s.StudentId,
                    IdentityCode = s.IdentityCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Success("Success", result));
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDetailsDto>> GetStudentById(int id)
        {



            return Ok(ApiResponse<StudentDetailsDto>.Success("Get student successfully"));
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, [FromBody] StudentDetailsDto dto)
        {


            try
            {
                return Ok(ApiResponse<object>.Success("Cập nhật student thành công."));
            }
            catch(DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Database update error: {ex.Message}"));
            }
            catch(Exception ex)
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
            if(!ModelState.IsValid)
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
                if(!createUserResult.Succeeded)
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
            catch(Exception ex)
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

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        //Import Excel
        [HttpPost("import-students")]
        public async Task<IActionResult> ImportStudentsFromExcel(IFormFile file)
        {
            if(file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var studentsToAdd = new List<Student>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension.Rows;

            for(int row = 2; row <= rowCount; row++)
            {
                var identityCode = worksheet.Cells[row, 1].Text.Trim();
                var firstName = worksheet.Cells[row, 2].Text.Trim();
                var lastName = worksheet.Cells[row, 3].Text.Trim();
                var dobText = worksheet.Cells[row, 4].Text.Trim();
                var address = worksheet.Cells[row, 5].Text.Trim();
                var phone = worksheet.Cells[row, 6].Text.Trim();
                var email = worksheet.Cells[row, 7].Text.Trim();

                // Check duplicate in DB
                bool isDuplicate = await _context.Students.AnyAsync(s =>
                    (identityCode != "" && s.IdentityCode == identityCode) ||
                    (phone != "" && s.Phone == phone) ||
                    (email != "" && s.Email == email));

                if(isDuplicate)
                    continue; // Skip this row

                // Parse Date
                DateTime? dob = null;
                if(DateTime.TryParse(dobText, out var parsedDate))
                    dob = parsedDate;

                studentsToAdd.Add(new Student
                {
                    IdentityCode = identityCode == "" ? null : identityCode,
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = dob.HasValue ? DateOnly.FromDateTime(dob.Value) : null,
                    Address = address == "" ? null : address,
                    Phone = phone == "" ? null : phone,
                    Email = email == "" ? null : email
                });
            }

            _context.Students.AddRange(studentsToAdd);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Success($"Đã thêm {studentsToAdd.Count} sinh viên từ Excel."));
        }


        //Export Excel
        [HttpGet("export-students")]
        public async Task<IActionResult> ExportStudentsToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var students = await _context.Students.AsNoTracking().ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");

            // Header
            worksheet.Cells[1, 1].Value = "IdentityCode";
            worksheet.Cells[1, 2].Value = "FirstName";
            worksheet.Cells[1, 3].Value = "LastName";
            worksheet.Cells[1, 4].Value = "DateOfBirth";
            worksheet.Cells[1, 5].Value = "Address";
            worksheet.Cells[1, 6].Value = "Phone";
            worksheet.Cells[1, 7].Value = "Email";

            int row = 2;
            foreach(var student in students)
            {
                worksheet.Cells[row, 1].Value = student.IdentityCode;
                worksheet.Cells[row, 2].Value = student.FirstName;
                worksheet.Cells[row, 3].Value = student.LastName;
                worksheet.Cells[row, 4].Value = student.DateOfBirth?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = student.Address;
                worksheet.Cells[row, 6].Value = student.Phone;
                worksheet.Cells[row, 7].Value = student.Email;
                row++;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students.xlsx");
        }

    }
}
