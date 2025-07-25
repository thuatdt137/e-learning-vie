using e_learning_vie.Commons;
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

        private readonly ITrendAnalysisService _trendAnalysisService;
        public PerformancesController(SchoolManagementContext context, IOptions<AcademicLevelRulesConfig> rules, IStudentAcademicService service, ITrendAnalysisService trendAnalysisService)
        {
            _context = context;
            _rules = rules.Value;
            _service = service;
            _trendAnalysisService = trendAnalysisService;
        }

        [HttpGet("scores/student")]
        public IActionResult GetStudentScores(int studentId, int semesterId)
        {
            try
            {
                return Ok(_service.GetStudentScores(studentId, semesterId));
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("grade-performance-trend/{gradeId}")]
        public async Task<IActionResult> GetGradePerformanceTrend(
            int gradeId,
            [FromQuery] int? startYear = null,
            [FromQuery] int? endYear = null,
            [FromQuery] int? yearsBack = null)
        {
            var result = await _trendAnalysisService.GetGradePerformanceTrendAsync(gradeId, startYear, endYear, yearsBack);
            return Ok(ApiResponse<object>.Success($"Xu hướng kết quả học tập khối {gradeId}", result));
        }

        [HttpGet("subject-performance-trend/{subjectId}")]
        public async Task<IActionResult> GetSubjectPerformanceTrend(
            int subjectId,
            [FromQuery] int? startYear = null,
            [FromQuery] int? endYear = null,
            [FromQuery] int? yearsBack = null)
        {
            var result = await _trendAnalysisService.GetSubjectPerformanceTrendAsync(subjectId, startYear, endYear, yearsBack);
            return Ok(ApiResponse<object>.Success($"Xu hướng kết quả môn học {subjectId}", result));
        }
    }
}
