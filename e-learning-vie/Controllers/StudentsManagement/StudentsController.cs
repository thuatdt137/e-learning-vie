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
        public async Task<ActionResult<IEnumerable<Student>>> GetStudentsBySchool()
        {

			var students = await _context.Students
			.AsNoTracking()
			.ToListAsync();

			return Ok(ApiResponse<List<Student>>.Success(
				"Danh sách student",
				students
			));
		}

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.StudentId)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

		// POST: api/Students
		[HttpPost]
		public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
		{
			// 1. Check trùng IdentityCode
			var exists = await _context.Students
				.AnyAsync(st => st.IdentityCode == dto.IdentityCode);

			if (exists)
			{
				return BadRequest(ApiResponse<object>.Fail(
					"IdentityCode đã tồn tại.",
					new { identityCode = new[] { "IdentityCode must be unique." } }
				));
			}

			// 2. Tạo Student
			var student = dto.ToStudent();
			_context.Students.Add(student);
			await _context.SaveChangesAsync(); // Lưu để có StudentId

			// 3. Tạo User
			var user = new User
			{
				UserName = dto.IdentityCode,
				Student = student
			};

			var createUserResult = await _userManager.CreateAsync(user, "Password123@");
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

			await _userManager.AddToRoleAsync(user, "Student");

			// 4. Trả về
			return StatusCode(201, ApiResponse<object>.Success(
				"Tạo student thành công.",
				new
				{
					student.StudentId,
					student.FirstName,
					student.LastName,
					student.IdentityCode
				}
			));
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
    }
}
