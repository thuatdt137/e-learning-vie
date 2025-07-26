using e_learning_vie.Commons;
using e_learning_vie.DTOs.classes;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Controllers.ClassesManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassSessionController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public ClassSessionController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateClassSession([FromBody] CreateClassSessionDto dto)
        {
            try
            {
                var classExists = await _context.Classes.AnyAsync(c => c.ClassId == dto.ClassId);
                if (!classExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy lớp học với ID {dto.ClassId}"));
                }

                var semesterExists = await _context.Semesters.AnyAsync(s => s.SemesterId == dto.SemesterId);
                if (!semesterExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy kỳ học với ID {dto.SemesterId}"));
                }

                if (dto.TeacherId.HasValue)
                {
                    var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == dto.TeacherId.Value);
                    if (!teacherExists)
                    {
                        return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy giáo viên với ID {dto.TeacherId.Value}"));
                    }
                }

                var existingClassSession = await _context.ClassSessions
                    .FirstOrDefaultAsync(cs => cs.ClassId == dto.ClassId && cs.SemesterId == dto.SemesterId);
                if (existingClassSession != null)
                {
                    return BadRequest(ApiResponse<object>.Fail($"ClassSession đã tồn tại cho lớp {dto.ClassId} và kỳ {dto.SemesterId}"));
                }

                var classSession = new ClassSession
                {
                    ClassId = dto.ClassId,
                    SemesterId = dto.SemesterId,
                    TeacherId = dto.TeacherId
                };

                _context.ClassSessions.Add(classSession);
                await _context.SaveChangesAsync();

                var responseData = new
                {
                    ClassSessionId = classSession.ClassSessionId,
                    ClassId = classSession.ClassId,
                    SemesterId = classSession.SemesterId,
                    TeacherId = classSession.TeacherId
                };

                return Ok(ApiResponse<object>.Success($"Tạo ClassSession thành công cho lớp {dto.ClassId} trong kỳ {dto.SemesterId}", responseData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi tạo ClassSession: {ex.Message}"));
            }
        }

        [HttpPost("create-list")]
        public async Task<ActionResult<ApiResponse<object>>> CreateClassSessionList([FromBody] CreateClassSessionListDto dto)
        {
            try
            {
                var classEntity = await _context.Classes
                    .Include(c => c.Grade)
                    .FirstOrDefaultAsync(c => c.ClassId == dto.ClassId);
                if (classEntity == null)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy lớp học với ID {dto.ClassId}"));
                }

                var semesterExists = await _context.Semesters.AnyAsync(s => s.SemesterId == dto.SemesterId);
                if (!semesterExists)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy kỳ học với ID {dto.SemesterId}"));
                }

                if (dto.HomeroomTeacherId.HasValue)
                {
                    var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == dto.HomeroomTeacherId.Value);
                    if (!teacherExists)
                    {
                        return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy giáo viên chủ nhiệm với ID {dto.HomeroomTeacherId.Value}"));
                    }
                }

                var existingClassSession = await _context.ClassSessions
                    .FirstOrDefaultAsync(cs => cs.ClassId == dto.ClassId && cs.SemesterId == dto.SemesterId);
                if (existingClassSession != null)
                {
                    return BadRequest(ApiResponse<object>.Fail($"ClassSession đã tồn tại cho lớp {dto.ClassId} và kỳ {dto.SemesterId}"));
                }

                var subjects = await _context.Subjects
                    .Where(s => s.GradeId == classEntity.GradeId)
                    .ToListAsync();

                if (!subjects.Any())
                {
                    return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy môn học nào thuộc khối lớp của lớp {dto.ClassId}"));
                }

                var classSession = new ClassSession
                {
                    ClassId = dto.ClassId,
                    SemesterId = dto.SemesterId,
                    TeacherId = dto.HomeroomTeacherId
                };

                _context.ClassSessions.Add(classSession);
                await _context.SaveChangesAsync();

                var teachingAssignments = new List<TeachingAssignment>();
                var errors = new List<string>();

                foreach (var subjectTeacher in dto.SubjectTeachers ?? new List<SubjectTeacherDto>())
                {
                    var subject = subjects.FirstOrDefault(s => s.SubjectId == subjectTeacher.SubjectId);
                    if (subject == null)
                    {
                        errors.Add($"Không tìm thấy môn học với ID {subjectTeacher.SubjectId}");
                        continue;
                    }

                    if (subjectTeacher.TeacherId.HasValue)
                    {
                        var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == subjectTeacher.TeacherId.Value);
                        if (!teacherExists)
                        {
                            errors.Add($"Không tìm thấy giáo viên với ID {subjectTeacher.TeacherId.Value} cho môn {subject.SubjectName}");
                            continue;
                        }

                        var teacherSubjectExists = await _context.TeacherSubjects
                            .AnyAsync(ts => ts.TeacherId == subjectTeacher.TeacherId.Value && ts.SubjectId == subjectTeacher.SubjectId);
                        if (!teacherSubjectExists)
                        {
                            errors.Add($"Giáo viên {subjectTeacher.TeacherId.Value} không được phân công dạy môn {subject.SubjectName}");
                            continue;
                        }

                        var teachingAssignment = new TeachingAssignment
                        {
                            ClassSessionId = classSession.ClassSessionId,
                            SubjectId = subjectTeacher.SubjectId,
                            TeacherId = subjectTeacher.TeacherId.Value
                        };

                        teachingAssignments.Add(teachingAssignment);
                    }
                }

                foreach (var subject in subjects.Where(s => !dto.SubjectTeachers?.Any(st => st.SubjectId == s.SubjectId) ?? true))
                {
                    var teachingAssignment = new TeachingAssignment
                    {
                        ClassSessionId = classSession.ClassSessionId,
                        SubjectId = subject.SubjectId,
                        TeacherId = null
                    };
                    teachingAssignments.Add(teachingAssignment);
                }

                if (errors.Any())
                {
                    return BadRequest(ApiResponse<object>.Fail("Có lỗi trong quá trình tạo TeachingAssignments", new { Errors = errors }));
                }

                _context.TeachingAssignments.AddRange(teachingAssignments);
                await _context.SaveChangesAsync();

                var responseData = new
                {
                    ClassSessionId = classSession.ClassSessionId,
                    ClassId = classSession.ClassId,
                    SemesterId = classSession.SemesterId,
                    HomeroomTeacherId = classSession.TeacherId,
                    TeachingAssignments = teachingAssignments.Select(ta => new
                    {
                        ta.TeachingAssignmentId,
                        ta.SubjectId,
                        ta.TeacherId
                    })
                };

                return Ok(ApiResponse<object>.Success($"Tạo ClassSession và TeachingAssignments thành công cho lớp {dto.ClassId} trong kỳ {dto.SemesterId}", responseData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi tạo ClassSession và TeachingAssignments: {ex.Message}"));
            }
        }

        [HttpPost("copy")]
        public async Task<ActionResult<ApiResponse<object>>> CopyClassSessions([FromBody] CopyClassSessionDto dto)
        {
            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var refSemesterExists = await _context.Semesters.AnyAsync(s => s.SemesterId == dto.RefSemesterId);
                    if (!refSemesterExists)
                    {
                        return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy kỳ tham chiếu với ID {dto.RefSemesterId}."));
                    }

                    var newSemesterExists = await _context.Semesters.AnyAsync(s => s.SemesterId == dto.NewSemesterId);
                    if (!newSemesterExists)
                    {
                        return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy kỳ mới với ID {dto.NewSemesterId}."));
                    }

                    var classSessions = await _context.ClassSessions
                        .Where(cs => cs.SemesterId == dto.RefSemesterId)
                        .Include(cs => cs.Class)
                            .ThenInclude(c => c.Grade)
                        .AsNoTracking()
                        .ToListAsync();

                    var classIds = dto.ClassIds != null && dto.ClassIds.Any()
                        ? classSessions.Where(cs => dto.ClassIds.Contains(cs.ClassId)).Select(cs => cs.ClassId).Distinct().ToList()
                        : classSessions.Select(cs => cs.ClassId).Distinct().ToList();

                    if (!classIds.Any())
                    {
                        return BadRequest(ApiResponse<object>.Fail("Không tìm thấy lớp nào trong kỳ tham chiếu hoặc danh sách ClassIds không hợp lệ."));
                    }

                    var existingClassSessions = await _context.ClassSessions
                        .Where(cs => cs.SemesterId == dto.NewSemesterId && classIds.Contains(cs.ClassId))
                        .Select(cs => cs.ClassId)
                        .ToListAsync();

                    if (existingClassSessions.Any())
                    {
                        return BadRequest(ApiResponse<object>.Fail($"Một số lớp đã có ClassSession trong kỳ mới: {string.Join(", ", existingClassSessions)}."));
                    }

                    var errors = new List<string>();
                    var createdClassSessions = new List<object>();
                    var createdTeachingAssignments = new List<object>();

                    foreach (var classId in classIds)
                    {
                        var refClassSession = classSessions.FirstOrDefault(cs => cs.ClassId == classId);
                        if (refClassSession == null)
                        {
                            errors.Add($"Không tìm thấy ClassSession cho lớp {classId} trong kỳ tham chiếu.");
                            continue;
                        }

                        var newClassSession = new ClassSession
                        {
                            ClassId = classId,
                            SemesterId = dto.NewSemesterId,
                            TeacherId = dto.CopyHomeroomTeacher ? refClassSession.TeacherId : null
                        };

                        if (newClassSession.TeacherId.HasValue)
                        {
                            var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == newClassSession.TeacherId.Value);
                            if (!teacherExists)
                            {
                                errors.Add($"Giáo viên chủ nhiệm {newClassSession.TeacherId.Value} không tồn tại cho lớp {classId}.");
                                newClassSession.TeacherId = null;
                            }
                        }

                        _context.ClassSessions.Add(newClassSession);
                        await _context.SaveChangesAsync();

                        var refTeachingAssignments = await _context.TeachingAssignments
                            .Where(ta => ta.ClassSessionId == refClassSession.ClassSessionId)
                            .AsNoTracking()
                            .ToListAsync();

                        var gradeId = refClassSession.Class.GradeId;
                        var subjects = await _context.Subjects
                            .Where(s => s.GradeId == gradeId)
                            .AsNoTracking()
                            .ToListAsync();

                        foreach (var subject in subjects)
                        {
                            var refAssignment = refTeachingAssignments.FirstOrDefault(ta => ta.SubjectId == subject.SubjectId);
                            int? teacherId = null;

                            var subjectTeacher = dto.SubjectTeachers?.FirstOrDefault(st => st.SubjectId == subject.SubjectId && st.ClassId == classId);
                            if (subjectTeacher != null)
                            {
                                if (subjectTeacher.TeacherId.HasValue)
                                {
                                    var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == subjectTeacher.TeacherId.Value);
                                    if (!teacherExists)
                                    {
                                        errors.Add($"Giáo viên {subjectTeacher.TeacherId.Value} không tồn tại cho môn {subject.SubjectId} của lớp {classId}.");
                                        continue;
                                    }

                                    var teacherSubjectExists = await _context.TeacherSubjects
                                        .AnyAsync(ts => ts.TeacherId == subjectTeacher.TeacherId.Value && ts.SubjectId == subject.SubjectId);
                                    if (!teacherSubjectExists)
                                    {
                                        errors.Add($"Giáo viên {subjectTeacher.TeacherId.Value} không được phân công dạy môn {subject.SubjectId} cho lớp {classId}.");
                                        continue;
                                    }

                                    teacherId = subjectTeacher.TeacherId.Value;
                                }
                            }
                            else if (refAssignment != null && refAssignment.TeacherId.HasValue)
                            {
                                var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == refAssignment.TeacherId.Value);
                                var teacherSubjectExists = await _context.TeacherSubjects
                                    .AnyAsync(ts => ts.TeacherId == refAssignment.TeacherId.Value && ts.SubjectId == subject.SubjectId);
                                if (teacherExists && teacherSubjectExists)
                                {
                                    teacherId = refAssignment.TeacherId.Value;
                                }
                                else
                                {
                                    errors.Add($"Giáo viên {refAssignment.TeacherId.Value} từ kỳ tham chiếu không hợp lệ cho môn {subject.SubjectId} của lớp {classId}.");
                                }
                            }

                            var duplicateExists = await _context.TeachingAssignments
                                .AnyAsync(ta => ta.TeacherId == teacherId &&
                                                ta.SubjectId == subject.SubjectId &&
                                                ta.ClassSessionId == newClassSession.ClassSessionId);
                            if (duplicateExists)
                            {
                                errors.Add($"Phân công giảng dạy cho giáo viên {teacherId}, môn {subject.SubjectId}, lớp {classId} trong kỳ mới đã tồn tại.");
                                continue;
                            }

                            var newTeachingAssignment = new TeachingAssignment
                            {
                                ClassSessionId = newClassSession.ClassSessionId,
                                SubjectId = subject.SubjectId,
                                TeacherId = teacherId
                            };

                            _context.TeachingAssignments.Add(newTeachingAssignment);
                            createdTeachingAssignments.Add(new
                            {
                                newTeachingAssignment.TeachingAssignmentId,
                                newTeachingAssignment.ClassSessionId,
                                newTeachingAssignment.SubjectId,
                                newTeachingAssignment.TeacherId
                            });
                        }

                        createdClassSessions.Add(new
                        {
                            newClassSession.ClassSessionId,
                            newClassSession.ClassId,
                            newClassSession.SemesterId,
                            newClassSession.TeacherId
                        });
                    }

                    if (errors.Any())
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(ApiResponse<object>.Fail("Có lỗi trong quá trình sao chép ClassSession và TeachingAssignments.", new { Errors = errors }));
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var responseData = new
                    {
                        ClassSessions = createdClassSessions,
                        TeachingAssignments = createdTeachingAssignments
                    };

                    return Ok(ApiResponse<object>.Success($"Sao chép ClassSession và TeachingAssignments thành công từ kỳ {dto.RefSemesterId} sang kỳ {dto.NewSemesterId}.", responseData));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Lỗi hệ thống khi sao chép ClassSession và TeachingAssignments: {ex.Message}"));
            }
        }
    }
}
