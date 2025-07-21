using e_learning_vie.Commons;
using e_learning_vie.DTOs.Student;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Lấy điểm học sinh theo năm học (mặc định là năm học hiện tại)
        /// </summary>
        /// <param name="academicYearId">ID năm học (null = năm học hiện tại)</param>
        /// <returns>Điểm của học sinh theo từng môn học</returns>
        [HttpGet("by-academic-year")]
        public async Task<ActionResult<ApiResponse<StudentGradeDto>>> GetGradesByAcademicYear([FromQuery] int? academicYearId = null)
        {
            try
            {
                var grades = await _gradeService.GetGradesByAcademicYearAsync(User, academicYearId);
                return Ok(ApiResponse<StudentGradeDto>.Success("Lấy điểm thành công", grades));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<StudentGradeDto>.Fail(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<StudentGradeDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StudentGradeDto>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả các năm học mà học sinh có điểm
        /// </summary>
        /// <returns>Danh sách các năm học</returns>
        [HttpGet("academic-years")]
        public async Task<ActionResult<ApiResponse<List<AcademicYearDto>>>> GetAcademicYearsWithGrades()
        {
            try
            {
                var academicYears = await _gradeService.GetAcademicYearsWithGradesAsync(User);
                return Ok(ApiResponse<List<AcademicYearDto>>.Success("Lấy danh sách năm học thành công", academicYears));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<List<AcademicYearDto>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AcademicYearDto>>.Error($"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy điểm hiện tại của học sinh (năm học hiện tại)
        /// </summary>
        /// <returns>Điểm của năm học hiện tại</returns>
        [HttpGet("current")]
        public async Task<ActionResult<ApiResponse<StudentGradeDto>>> GetCurrentGrades()
        {
            try
            {
                var grades = await _gradeService.GetGradesByAcademicYearAsync(User, null);
                return Ok(ApiResponse<StudentGradeDto>.Success("Lấy điểm hiện tại thành công", grades));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<StudentGradeDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StudentGradeDto>.Error($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
