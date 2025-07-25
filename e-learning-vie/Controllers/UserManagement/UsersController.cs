using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.User;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.UserManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public UsersController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                StudentId = u.StudentId,
                TeacherId = u.TeacherId,
                ParentId = u.ParentId,
                IsActive = u.IsActive
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", users));
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.Where(u => u.Id == id).Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                StudentId = u.StudentId,
                TeacherId = u.TeacherId,
                ParentId = u.ParentId,
                IsActive = u.IsActive
            }).FirstOrDefault();

            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail("User not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", user));
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid user data."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value?.Errors?.Count > 0)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(ApiResponse<object>.Fail("Model validation failed.", errors));
            }

            try
            {
                var user = new User
                {
                    UserName = userDTO.UserName,
                    Email = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
                    StudentId = userDTO.StudentId,
                    TeacherId = userDTO.TeacherId,
                    ParentId = userDTO.ParentId,
                    IsActive = userDTO.IsActive
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("User created successfully.", user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid user data."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value?.Errors?.Count > 0)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(ApiResponse<object>.Fail("Model validation failed.", errors));
            }

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found."));
                }

                user.UserName = userDTO.UserName;
                user.Email = userDTO.Email;
                user.PhoneNumber = userDTO.PhoneNumber;
                user.StudentId = userDTO.StudentId;
                user.TeacherId = userDTO.TeacherId;
                user.ParentId = userDTO.ParentId;
                user.IsActive = userDTO.IsActive;

                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("User updated successfully.", user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found."));
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("User deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
