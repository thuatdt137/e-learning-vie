using e_learning_vie.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.BusinessManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        public ReportController(SchoolManagementContext context)
        {
            _context = context;
        }
    }
}
