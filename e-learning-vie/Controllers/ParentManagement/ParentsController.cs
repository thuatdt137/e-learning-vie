using e_learning_vie.Commons;
using e_learning_vie.DTOs.Parent;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace e_learning_vie.Controllers.ParentManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        public ParentsController(SchoolManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetParents()
        {
            var parents = _context.Parents.Select(p => new ParentDTO
            {
                ParentId = p.ParentId,
                IdentityCode = p.IdentityCode,
                FirstName = p.FirstName,
                LastName = p.LastName,
                IsMale = p.IsMale,
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                Phone = p.Phone,
                Email = p.Email
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", parents));
        }
        [HttpGet("{id}")]
        public IActionResult GetParentById(int id)
        {
            var parent = _context.Parents.Where(p => p.ParentId == id).Select(p => new ParentDTO
            {
                ParentId = p.ParentId,
                IdentityCode = p.IdentityCode,
                FirstName = p.FirstName,
                LastName = p.LastName,
                IsMale = p.IsMale,
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                Phone = p.Phone,
                Email = p.Email
            }).FirstOrDefault();
            if (parent == null)
            {
                return NotFound(ApiResponse<object>.Fail("Parent not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", parent));
        }
        [HttpPost]
        public IActionResult CreateParent([FromBody] ParentDTO parentDTO)
        {
            if (parentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid parent data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var parent = new Parent
            {
                IdentityCode = parentDTO.IdentityCode,
                FirstName = parentDTO.FirstName,
                LastName = parentDTO.LastName,
                IsMale = parentDTO.IsMale,
                DateOfBirth = parentDTO.DateOfBirth,
                Address = parentDTO.Address,
                Phone = parentDTO.Phone,
                Email = parentDTO.Email
            };
            var result = _context.Parents.Add(parent);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Parent created successfully.", result.Entity));
        }
        [HttpPut("{id}")]
        public IActionResult UpdateParent(int id, [FromBody] ParentDTO parentDTO)
        {
            if (parentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid parent data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var parent = _context.Parents.Find(id);
            if (parent == null)
            {
                return NotFound(ApiResponse<object>.Fail("Parent not found."));
            }
            parent.IdentityCode = parentDTO.IdentityCode;
            parent.FirstName = parentDTO.FirstName;
            parent.LastName = parentDTO.LastName;
            parent.IsMale = parentDTO.IsMale;
            parent.DateOfBirth = parentDTO.DateOfBirth;
            parent.Address = parentDTO.Address;
            parent.Phone = parentDTO.Phone;
            parent.Email = parentDTO.Email;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Parent updated successfully.", parent));
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteParent(int id)
        {
            var parent = _context.Parents.Find(id);
            if (parent == null)
            {
                return NotFound(ApiResponse<object>.Fail("Parent not found."));
            }
            _context.Parents.Remove(parent);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Parent deleted successfully."));
        }
    }
}
