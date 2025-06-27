using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning_vie.Controllers.StudentsManagement
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentGradeController : ControllerBase
    {
        private readonly IStudentGradeService _gradeService;

        public StudentGradeController(IStudentGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpGet("by-academic-year")]
        public async Task<IActionResult> GetGradesByAcademicYear([FromQuery] int? academicYearId = null)
        {
            try
            {
                var grades = await _gradeService.GetGradesByAcademicYearAsync(User, academicYearId); // truyền `User` để service tự lấy studentId
                return Ok(grades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
