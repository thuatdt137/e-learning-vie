using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.SubjectGroup;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.SubjectGroupManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectGroupsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public SubjectGroupsController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetSubjectGroups()
        {
            var subjectGroups = _context.SubjectGroups.Select(sg => new SubjectGroupDTO
            {
                SubjectGroupId = sg.SubjectGroupId,
                SubjectGroupName = sg.SubjectGroupName
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", subjectGroups));
        }

        [HttpGet("{id}")]
        public IActionResult GetSubjectGroupById(int id)
        {
            var subjectGroup = _context.SubjectGroups.Where(sg => sg.SubjectGroupId == id).Select(sg => new SubjectGroupDTO
            {
                SubjectGroupId = sg.SubjectGroupId,
                SubjectGroupName = sg.SubjectGroupName
            }).FirstOrDefault();
            if (subjectGroup == null)
            {
                return NotFound(ApiResponse<object>.Fail("Subject group not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", subjectGroup));
        }

        [HttpPost]
        public IActionResult CreateSubjectGroup([FromBody] SubjectGroupDTO subjectGroupDTO)
        {
            if (subjectGroupDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid subject group data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var subjectGroup = new SubjectGroup
            {
                SubjectGroupName = subjectGroupDTO.SubjectGroupName
            };
            var result = _context.SubjectGroups.Add(subjectGroup);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Subject group created successfully.", result.Entity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSubjectGroup(int id, [FromBody] SubjectGroupDTO subjectGroupDTO)
        {
            if (subjectGroupDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid subject group data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var subjectGroup = _context.SubjectGroups.Find(id);
            if (subjectGroup == null)
            {
                return NotFound(ApiResponse<object>.Fail("Subject group not found."));
            }
            subjectGroup.SubjectGroupName = subjectGroupDTO.SubjectGroupName;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Subject group updated successfully.", subjectGroup));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSubjectGroup(int id)
        {
            var subjectGroup = _context.SubjectGroups.Find(id);
            if (subjectGroup == null)
            {
                return NotFound(ApiResponse<object>.Fail("Subject group not found."));
            }
            _context.SubjectGroups.Remove(subjectGroup);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Subject group deleted successfully."));
        }
    }
}
