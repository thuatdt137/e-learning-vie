using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning_vie.Models;
using e_learning_vie.DTOs.TeachingAssignment;
using e_learning_vie.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace e_learning_vie.Controllers.TeachingAssignmentManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,TrainingDepartment")]
    public class TeachingAssignmentsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public TeachingAssignmentsController(SchoolManagementContext context)
        {
            _context = context;
        }

        // GET: api/TeachingAssignments?semesterId=1&teacherId=101&classSessionId=201&subjectId=1
        [HttpGet]
        public async Task<IActionResult> GetTeachingAssignments(
            [FromQuery] int? semesterId = null,
            [FromQuery] int? teacherId = null,
            [FromQuery] int? classSessionId = null,
            [FromQuery] int? subjectId = null)
        {
            try
            {
                var query = _context.TeachingAssignments
                    .Include(ta => ta.Session)
                    .AsQueryable();

                // Lọc theo SemesterId
                if (semesterId.HasValue)
                {
                    query = query.Where(ta => ta.Session.SemesterId == semesterId.Value);
                }

                // Lọc theo TeacherId
                if (teacherId.HasValue)
                {
                    query = query.Where(ta => ta.TeacherId == teacherId.Value);
                }

                // Lọc theo ClassSessionId
                if (classSessionId.HasValue)
                {
                    query = query.Where(ta => ta.ClassSessionId == classSessionId.Value);
                }

                // Lọc theo SubjectId
                if (subjectId.HasValue)
                {
                    query = query.Where(ta => ta.SubjectId == subjectId.Value);
                }

                var teachingAssignments = await query
                    .Select(ta => new TeachingAssignmentDTO
                    {
                        TeachingAssignmentId = ta.TeachingAssignmentId,
                        TeacherId = ta.TeacherId,
                        SubjectId = ta.SubjectId,
                        ClassSessionId = ta.ClassSessionId
                    })
                    .ToListAsync();

                if (!teachingAssignments.Any())
                {
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy phân công giảng dạy nào phù hợp với bộ lọc."));
                }

                return Ok(ApiResponse<object>.Success("Lấy danh sách phân công giảng dạy thành công.", teachingAssignments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi lấy danh sách phân công giảng dạy: {ex.Message}"));
            }
        }

        // GET: api/TeachingAssignments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeachingAssignmentById(int id)
        {
            try
            {
                var teachingAssignment = await _context.TeachingAssignments
                    .Where(ta => ta.TeachingAssignmentId == id)
                    .Select(ta => new TeachingAssignmentDTO
                    {
                        TeachingAssignmentId = ta.TeachingAssignmentId,
                        TeacherId = ta.TeacherId,
                        SubjectId = ta.SubjectId,
                        ClassSessionId = ta.ClassSessionId
                    })
                    .FirstOrDefaultAsync();

                if (teachingAssignment == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy phân công giảng dạy với ID {id}."));
                }

                return Ok(ApiResponse<object>.Success("Lấy phân công giảng dạy thành công.", teachingAssignment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi lấy phân công giảng dạy: {ex.Message}"));
            }
        }

        // POST: api/TeachingAssignments
        [HttpPost]
        public async Task<IActionResult> CreateTeachingAssignment([FromBody] TeachingAssignmentDTO teachingAssignmentDTO)
        {
            if (teachingAssignmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu phân công giảng dạy không hợp lệ."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", errors));
            }

            try
            {
                // Kiểm tra sự tồn tại của TeacherId, SubjectId, ClassSessionId
                var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == teachingAssignmentDTO.TeacherId);
                if (!teacherExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy giáo viên với ID {teachingAssignmentDTO.TeacherId}."));
                }

                var subjectExists = await _context.Subjects.AnyAsync(s => s.SubjectId == teachingAssignmentDTO.SubjectId);
                if (!subjectExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy môn học với ID {teachingAssignmentDTO.SubjectId}."));
                }

                var classSessionExists = await _context.ClassSessions.AnyAsync(cs => cs.ClassSessionId == teachingAssignmentDTO.ClassSessionId);
                if (!classSessionExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy phiên lớp với ID {teachingAssignmentDTO.ClassSessionId}."));
                }

                // Kiểm tra TeacherSubjects
                var teacherSubjectExists = await _context.TeacherSubjects
                    .AnyAsync(ts => ts.TeacherId == teachingAssignmentDTO.TeacherId && ts.SubjectId == teachingAssignmentDTO.SubjectId);
                if (!teacherSubjectExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Giáo viên {teachingAssignmentDTO.TeacherId} không được phân công dạy môn {teachingAssignmentDTO.SubjectId}."));
                }

                // Kiểm tra tính duy nhất của tổ hợp TeacherId, SubjectId, ClassSessionId
                var duplicateExists = await _context.TeachingAssignments
                    .AnyAsync(ta => ta.TeacherId == teachingAssignmentDTO.TeacherId &&
                                    ta.SubjectId == teachingAssignmentDTO.SubjectId &&
                                    ta.ClassSessionId == teachingAssignmentDTO.ClassSessionId);
                if (duplicateExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Phân công giảng dạy với giáo viên {teachingAssignmentDTO.TeacherId}, môn {teachingAssignmentDTO.SubjectId}, và phiên lớp {teachingAssignmentDTO.ClassSessionId} đã tồn tại."));
                }

                // Tạo TeachingAssignment
                var teachingAssignment = new TeachingAssignment
                {
                    TeacherId = teachingAssignmentDTO.TeacherId,
                    SubjectId = teachingAssignmentDTO.SubjectId,
                    ClassSessionId = teachingAssignmentDTO.ClassSessionId
                };

                _context.TeachingAssignments.Add(teachingAssignment);
                await _context.SaveChangesAsync();

                var responseData = new TeachingAssignmentDTO
                {
                    TeachingAssignmentId = teachingAssignment.TeachingAssignmentId,
                    TeacherId = teachingAssignment.TeacherId,
                    SubjectId = teachingAssignment.SubjectId,
                    ClassSessionId = teachingAssignment.ClassSessionId
                };

                return Ok(ApiResponse<object>.Success("Tạo phân công giảng dạy thành công.", responseData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi tạo phân công giảng dạy: {ex.Message}"));
            }
        }

        // PUT: api/TeachingAssignments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeachingAssignment(int id, [FromBody] TeachingAssignmentDTO teachingAssignmentDTO)
        {
            if (teachingAssignmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu phân công giảng dạy không hợp lệ."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", errors));
            }

            try
            {
                var teachingAssignment = await _context.TeachingAssignments.FindAsync(id);
                if (teachingAssignment == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy phân công giảng dạy với ID {id}."));
                }

                // Kiểm tra sự tồn tại của TeacherId, SubjectId, ClassSessionId
                var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == teachingAssignmentDTO.TeacherId);
                if (!teacherExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy giáo viên với ID {teachingAssignmentDTO.TeacherId}."));
                }

                var subjectExists = await _context.Subjects.AnyAsync(s => s.SubjectId == teachingAssignmentDTO.SubjectId);
                if (!subjectExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy môn học với ID {teachingAssignmentDTO.SubjectId}."));
                }

                var classSessionExists = await _context.ClassSessions.AnyAsync(cs => cs.ClassSessionId == teachingAssignmentDTO.ClassSessionId);
                if (!classSessionExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy phiên lớp với ID {teachingAssignmentDTO.ClassSessionId}."));
                }

                // Kiểm tra TeacherSubjects
                var teacherSubjectExists = await _context.TeacherSubjects
                    .AnyAsync(ts => ts.TeacherId == teachingAssignmentDTO.TeacherId && ts.SubjectId == teachingAssignmentDTO.SubjectId);
                if (!teacherSubjectExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Giáo viên {teachingAssignmentDTO.TeacherId} không được phân công dạy môn {teachingAssignmentDTO.SubjectId}."));
                }

                // Kiểm tra tính duy nhất của tổ hợp TeacherId, SubjectId, ClassSessionId (trừ bản ghi hiện tại)
                var duplicateExists = await _context.TeachingAssignments
                    .AnyAsync(ta => ta.TeacherId == teachingAssignmentDTO.TeacherId &&
                                    ta.SubjectId == teachingAssignmentDTO.SubjectId &&
                                    ta.ClassSessionId == teachingAssignmentDTO.ClassSessionId &&
                                    ta.TeachingAssignmentId != id);
                if (duplicateExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Phân công giảng dạy với giáo viên {teachingAssignmentDTO.TeacherId}, môn {teachingAssignmentDTO.SubjectId}, và phiên lớp {teachingAssignmentDTO.ClassSessionId} đã tồn tại."));
                }

                // Cập nhật TeachingAssignment
                teachingAssignment.TeacherId = teachingAssignmentDTO.TeacherId;
                teachingAssignment.SubjectId = teachingAssignmentDTO.SubjectId;
                teachingAssignment.ClassSessionId = teachingAssignmentDTO.ClassSessionId;

                await _context.SaveChangesAsync();

                var responseData = new TeachingAssignmentDTO
                {
                    TeachingAssignmentId = teachingAssignment.TeachingAssignmentId,
                    TeacherId = teachingAssignment.TeacherId,
                    SubjectId = teachingAssignment.SubjectId,
                    ClassSessionId = teachingAssignment.ClassSessionId
                };

                return Ok(ApiResponse<object>.Success("Cập nhật phân công giảng dạy thành công.", responseData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi cập nhật phân công giảng dạy: {ex.Message}"));
            }
        }

        // DELETE: api/TeachingAssignments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeachingAssignment(int id)
        {
            try
            {
                var teachingAssignment = await _context.TeachingAssignments.FindAsync(id);
                if (teachingAssignment == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"Không tìm thấy phân công giảng dạy với ID {id}."));
                }

                _context.TeachingAssignments.Remove(teachingAssignment);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success("Xóa phân công giảng dạy thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi xóa phân công giảng dạy: {ex.Message}"));
            }
        }
    }
}