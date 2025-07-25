using e_learning_vie.DTOs.AcademicLevel;
using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace e_learning_vie.Controllers.BusinessManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerformancesController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        private readonly AcademicLevelRulesConfig _rules;
        private readonly IStudentAcademicService _service;
        public PerformancesController(SchoolManagementContext context, IOptions<AcademicLevelRulesConfig> rules, IStudentAcademicService service)
        {
            _context = context;
            _rules = rules.Value;
            _service = service;
        }

        [HttpGet("scores/student")]
        public IActionResult GetStudentScores(int studentId, int semesterId)
        {
            try
            {
                return Ok(_service.GetStudentScores(studentId, semesterId));
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("scores/class")]
        public IActionResult GetClassScores(int classId, int semesterId)
        {
            try
            {
                return Ok(_service.GetClassScores(classId, semesterId));
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("academic-level/class")]
        public IActionResult GetClassAcademicLevel(int classId, int semesterId)
        {
            try
            {
                return Ok(_service.GetClassAcademicLevel(classId, semesterId));
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("academic-level/grade")]
        public IActionResult GetGradeAcademicLevel(int gradeId, int semesterId)
        {
            try
            {
                return Ok(_service.GetGradeAcademicLevel(gradeId, semesterId));
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
