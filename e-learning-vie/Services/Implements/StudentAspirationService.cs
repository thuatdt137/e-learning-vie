using e_learning_vie.Enums;
using e_learning_vie.Models;
using e_learning_vie.ModelsDTO.Aspiration;
using e_learning_vie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning_vie.Services.Implements
{
    public class StudentAspirationService : IStudentAspirationService
    {
        private readonly SchoolManagementContext _context;
        private readonly IUserContextService _userContextService;

        public StudentAspirationService(SchoolManagementContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<List<AspirationItemDto>> GetAspirationsAsync(ClaimsPrincipal user, int? academicYearId = null)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            var query = _context.Aspirations
                .Include(a => a.TargetSchool)
                .Include(a => a.AcademicYear)
                .Where(a => a.StudentId == studentId);

            if (academicYearId.HasValue)
                query = query.Where(a => a.AcademicYearId == academicYearId);

            return await query
                .OrderBy(a => a.Priority)
                .Select(a => new AspirationItemDto
                {
                    AspirationId = a.AspirationId,
                    Priority = a.Priority,
                    TargetSchool = new SchoolDto
                    {
                        SchoolId = a.TargetSchool.SchoolId,
                        SchoolName = a.TargetSchool.SchoolName,
                        SchoolType = a.TargetSchool.SchoolType,
                        Address = a.TargetSchool.Address,
                        Phone = a.TargetSchool.Phone,
                        Email = a.TargetSchool.Email
                    },
                    AcademicYear = a.AcademicYear.YearName,
                })
                .ToListAsync();
        }

        public async Task<AspirationItemDto> CreateAspirationAsync(ClaimsPrincipal user, CreateAspirationDto request)
        {
            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
            if (studentId == null)
                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.Class == null || !student.Class.ClassName.StartsWith("9"))
                throw new InvalidOperationException("Chỉ học sinh lớp 9 mới được đăng ký nguyện vọng vào lớp 10");

            var targetSchool = await _context.Schools
                .FirstOrDefaultAsync(s => s.SchoolId == request.TargetSchoolId && s.SchoolType == SchoolType.C3);

            if (targetSchool == null)
                throw new ArgumentException("Chỉ có thể chọn trường THPT (C3)");

            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.AcademicYearId == request.AcademicYearId);

            if (academicYear == null)
                throw new ArgumentException("Không tìm thấy năm học");

            var existingAspiration = await _context.Aspirations
                .FirstOrDefaultAsync(a => a.StudentId == studentId &&
                                          a.AcademicYearId == request.AcademicYearId &&
                                          a.Priority == request.Priority);

            if (existingAspiration != null)
                throw new InvalidOperationException($"Đã có nguyện vọng thứ {request.Priority} trong năm học này");

            var aspiration = new Aspiration
            {
                StudentId = studentId.Value,
                TargetSchoolId = request.TargetSchoolId,
                AcademicYearId = request.AcademicYearId,
                Priority = request.Priority,
            };

            _context.Aspirations.Add(aspiration);
            await _context.SaveChangesAsync();

            return new AspirationItemDto
            {
                AspirationId = aspiration.AspirationId,
                Priority = aspiration.Priority,
                TargetSchool = new SchoolDto
                {
                    SchoolId = targetSchool.SchoolId,
                    SchoolName = targetSchool.SchoolName,
                    SchoolType = targetSchool.SchoolType,
                    Address = targetSchool.Address,
                    Phone = targetSchool.Phone,
                    Email = targetSchool.Email
                },
                AcademicYear = academicYear.YearName,
            };
        }

        public async Task<List<SchoolDto>> GetAvailableSchoolsAsync(SchoolType schoolType)
        {
            return await _context.Schools
                .Where(s => s.SchoolType == schoolType)
                .Select(s => new SchoolDto
                {
                    SchoolId = s.SchoolId,
                    SchoolName = s.SchoolName,
                    SchoolType = s.SchoolType,
                    Address = s.Address,
                    Phone = s.Phone,
                    Email = s.Email
                })
                .OrderBy(s => s.SchoolName)
                .ToListAsync();
        }
    }
}
