using e_learning_vie.Models;
using e_learning_vie.ModelsDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public TeacherController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeachersDto>> GetTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

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

            return dto;
        }

        [HttpPost("CreateTeacher")]
        public async Task<ActionResult<TeachersDto>> CreateTeacher([FromForm] TeachersDto dto)
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

            dto.TeacherId = teacher.TeacherId; 

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.TeacherId }, dto);
        }

        [HttpPut("UpdateTeacher")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromForm] TeachersDto dto)
        {
            if (id != dto.TeacherId)
                return BadRequest();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

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
                if (!_context.Teachers.Any(e => e.TeacherId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("DeleteTeacher/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
