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

namespace e_learning_vie.Controllers.StudentsManagement
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
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
		[Authorize(Roles = "TrainingDepartment")]
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
	}
}
