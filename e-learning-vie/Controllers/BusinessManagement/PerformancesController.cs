using e_learning_vie.DTOs.AcademicLevel;
using e_learning_vie.Models;
using e_learning_vie.Utils;
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
        public PerformancesController(SchoolManagementContext context, IOptions<AcademicLevelRulesConfig> rules)
        {
            _context = context;
            _rules = rules.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAcademicLevelsByClass(int classId, int semesterId)
        {
            return Ok(_rules.Good.MinConduct.GetDisplayName());
        }
    }
}
