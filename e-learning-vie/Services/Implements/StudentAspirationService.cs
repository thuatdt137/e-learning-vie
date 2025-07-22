//using e_learning_vie.DTOs.Aspiration;
//using e_learning_vie.DTOs.School;
//using e_learning_vie.Enums;
//using e_learning_vie.Models;
//using e_learning_vie.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace e_learning_vie.Services.Implements
//{
//    public class StudentAspirationService : IStudentAspirationService
//    {
//        private readonly SchoolManagementContext _context;
//        private readonly IUserContextService _userContextService;

//        public StudentAspirationService(SchoolManagementContext context, IUserContextService userContextService)
//        {
//            _context = context;
//            _userContextService = userContextService;
//        }
//        public async Task<List<AspirationItemDto>> GetAspirationsAsync(ClaimsPrincipal user, int? academicYearId = null)
//        {
//            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
//            if (studentId == null)
//                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

//            var query = _context.Aspirations
//                .Include(a => a.TargetSchool)
//                .Include(a => a.AcademicYear)
//                .Where(a => a.StudentId == studentId);

//            if (academicYearId.HasValue)
//                query = query.Where(a => a.AcademicYearId == academicYearId);

//            return await query
//                .OrderBy(a => a.Priority)
//                .Select(a => new AspirationItemDto
//                {
//                    AspirationId = a.AspirationId,
//                    Priority = a.Priority ?? 0,
//                    TargetSchool = new SchoolDTO(a.TargetSchool!),
//                    AcademicYear = a.AcademicYear!.YearName,
//                })
//                .ToListAsync();
//        }
//        private async Task<AcademicYear> ValidateAspirationTimeAsync(int academicYearId, string action = "đăng ký")
//        {
//            var academicYear = await _context.AcademicYears
//                .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

//            if (academicYear == null)
//                throw new ArgumentException("Năm học không tồn tại.");

//            var currentDate = DateTime.Now.Date;

//            // Kiểm tra thời gian đăng ký nguyện vọng
//            if (academicYear.AspirationRegistrationStartDate.HasValue &&
//                currentDate < academicYear.AspirationRegistrationStartDate.Value.ToDateTime(TimeOnly.MinValue))
//            {
//                throw new InvalidOperationException($"Thời gian {action} nguyện vọng chưa bắt đầu. Thời gian bắt đầu: {academicYear.AspirationRegistrationStartDate.Value:dd/MM/yyyy}");
//            }

//            if (academicYear.AspirationRegistrationEndDate.HasValue &&
//                currentDate > academicYear.AspirationRegistrationEndDate.Value.ToDateTime(TimeOnly.MinValue))
//            {
//                throw new InvalidOperationException($"Thời gian {action} nguyện vọng đã kết thúc. Thời gian kết thúc: {academicYear.AspirationRegistrationEndDate.Value:dd/MM/yyyy}");
//            }

//            return academicYear;
//        }

//        private async Task ValidateAspirationEditTimeAsync(int academicYearId)
//        {
//            var academicYear = await _context.AcademicYears
//                .FirstOrDefaultAsync(ay => ay.AcademicYearId == academicYearId);

//            if (academicYear == null)
//                throw new ArgumentException("Năm học không tồn tại.");

//            var currentDate = DateTime.Now.Date;

//            // Kiểm tra thời gian chỉnh sửa nguyện vọng
//            if (academicYear.AspirationEditDeadline.HasValue &&
//                currentDate > academicYear.AspirationEditDeadline.Value.ToDateTime(TimeOnly.MinValue))
//            {
//                throw new InvalidOperationException($"Thời gian chỉnh sửa nguyện vọng đã hết hạn. Hạn chót: {academicYear.AspirationEditDeadline.Value:dd/MM/yyyy}");
//            }

//            // Nếu không có thời gian chỉnh sửa riêng, sử dụng thời gian kết thúc đăng ký
//            if (!academicYear.AspirationEditDeadline.HasValue &&
//                academicYear.AspirationRegistrationEndDate.HasValue &&
//                currentDate > academicYear.AspirationRegistrationEndDate.Value.ToDateTime(TimeOnly.MinValue))
//            {
//                throw new InvalidOperationException($"Thời gian chỉnh sửa nguyện vọng đã kết thúc. Thời gian kết thúc: {academicYear.AspirationRegistrationEndDate.Value:dd/MM/yyyy}");
//            }
//        }

//        //public async Task<AspirationDto> CreateAspirationAsync(ClaimsPrincipal user, CreateAspirationDto request)
//        //{
//        //    var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
//        //    if (studentId == null)
//        //        throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

//        //    // Kiểm tra thời gian đăng ký nguyện vọng
//        //    var academicYear = await ValidateAspirationTimeAsync(request.AcademicYearId, "đăng ký");

//        //    var student = await _context.Students
//        //        .Include(s => s.StudentClassHistories)
//        //        .ThenInclude(sch => sch.Class!)
//        //        .ThenInclude(c => c!.AcademicYear)
//        //        .FirstOrDefaultAsync(s => s.StudentId == studentId);

//        //    if (student == null)
//        //        throw new ArgumentException("Học sinh không tồn tại.");

//        //    var currentDate = DateTime.Today;

//        //    // Tìm class hiện tại của học sinh theo ngày hôm nay nằm trong khoảng của AcademicYear
//        //    var currentClass = student.StudentClassHistories
//        //        .FirstOrDefault(sch =>
//        //            sch.Class != null &&
//        //            sch.Class.AcademicYear != null &&
//        //            sch.Class.AcademicYear.StartDate <= DateOnly.FromDateTime(currentDate) &&
//        //            sch.Class.AcademicYear.EndDate >= DateOnly.FromDateTime(currentDate));

//        //    if (currentClass == null)
//        //        throw new InvalidOperationException("Không tìm thấy lớp hiện tại.");

//        //    var className = currentClass.Class!.ClassName;

//        //    // Chỉ học sinh lớp 9 được phép đăng ký nguyện vọng vào trường cấp 3 (C3)
//        //    if (!className.StartsWith("9"))
//        //        throw new InvalidOperationException("Chỉ học sinh lớp 9 mới có thể đăng ký nguyện vọng vào trường cấp 3.");

//        //    // Kiểm tra xem đã có nguyện vọng với cùng thứ tự chưa
//        //    var existingAspiration = await _context.Aspirations
//        //        .Where(a => a.StudentId == studentId &&
//        //                   a.AcademicYearId == request.AcademicYearId &&
//        //                   a.Priority == request.Order)
//        //        .FirstOrDefaultAsync();

//        //    if (existingAspiration != null)
//        //        throw new InvalidOperationException($"Bạn đã có nguyện vọng với thứ tự {request.Order} trong năm học này.");

//        //    // Kiểm tra xem đã có nguyện vọng với cùng trường chưa
//        //    var existingSchoolAspiration = await _context.Aspirations
//        //        .Where(a => a.StudentId == studentId &&
//        //                   a.AcademicYearId == request.AcademicYearId &&
//        //                   a.TargetSchoolId == request.SchoolId)
//        //        .FirstOrDefaultAsync();

//        //    if (existingSchoolAspiration != null)
//        //        throw new InvalidOperationException("Bạn đã đăng ký nguyện vọng cho trường này rồi.");

//        //    // Kiểm tra trường có tồn tại không
//        //    var school = await _context.Schools
//        //        .FirstOrDefaultAsync(s => s.SchoolId == request.SchoolId);

//        //    if (school == null)
//        //        throw new ArgumentException("Trường không tồn tại.");

//        //    var aspiration = new Aspiration
//        //    {
//        //        StudentId = studentId,
//        //        TargetSchoolId = request.SchoolId,
//        //        Priority = request.Order,
//        //        AcademicYearId = request.AcademicYearId,
//        //        CreatedAt = DateTime.UtcNow
//        //    };

//        //    _context.Aspirations.Add(aspiration);
//        //    await _context.SaveChangesAsync();

//        //    return new AspirationDto
//        //    {
//        //        Id = aspiration.AspirationId,
//        //        AcademicYearId = request.AcademicYearId,
//        //        SchoolId = request.SchoolId,
//        //        SchoolName = school.SchoolName,
//        //        Order = request.Order,
//        //        CreatedAt = aspiration.CreatedAt ?? DateTime.UtcNow
//        //    };
//        //}

//        public async Task<List<School>> GetAvailableSchoolsAsync(SchoolType schoolType)
//        {
//            return await _context.Schools
//                .Where(s => s.SchoolType == schoolType)
//                .ToListAsync();
//        }

//        public async Task<AspirationDto> UpdateAspirationAsync(ClaimsPrincipal user, int aspirationId, CreateAspirationDto request)
//        {
//            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
//            if (studentId == null)
//                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

//            var aspiration = await _context.Aspirations
//                .FirstOrDefaultAsync(a => a.AspirationId == aspirationId && a.StudentId == studentId);

//            if (aspiration == null)
//                throw new ArgumentException("Không tìm thấy nguyện vọng");

//            // Kiểm tra thời gian chỉnh sửa nguyện vọng
//            await ValidateAspirationEditTimeAsync(request.AcademicYearId);

//            // Kiểm tra xem có nguyện vọng khác với cùng thứ tự không
//            var existingAspiration = await _context.Aspirations
//                .Where(a => a.StudentId == studentId &&
//                           a.AcademicYearId == request.AcademicYearId &&
//                           a.Priority == request.Order &&
//                           a.AspirationId != aspirationId)
//                .FirstOrDefaultAsync();

//            if (existingAspiration != null)
//                throw new InvalidOperationException($"Đã có nguyện vọng khác với thứ tự {request.Order}");

//            // Kiểm tra trường có tồn tại không
//            var school = await _context.Schools
//                .FirstOrDefaultAsync(s => s.SchoolId == request.SchoolId);

//            if (school == null)
//                throw new ArgumentException("Trường không tồn tại.");

//            aspiration.TargetSchoolId = request.SchoolId;
//            aspiration.Priority = request.Order;
//            aspiration.AcademicYearId = request.AcademicYearId;

//            await _context.SaveChangesAsync();

//            return new AspirationDto
//            {
//                Id = aspiration.AspirationId,
//                AcademicYearId = request.AcademicYearId,
//                SchoolId = request.SchoolId,
//                SchoolName = school.SchoolName,
//                Order = request.Order,
//                CreatedAt = aspiration.CreatedAt ?? DateTime.UtcNow
//            };
//        }

//        public async Task<bool> DeleteAspirationAsync(ClaimsPrincipal user, int aspirationId)
//        {
//            var studentId = await _userContextService.GetCurrentStudentIdAsync(user);
//            if (studentId == null)
//                throw new InvalidOperationException("Không tìm thấy học sinh hiện tại");

//            var aspiration = await _context.Aspirations
//                .Include(a => a.AcademicYear)
//                .FirstOrDefaultAsync(a => a.AspirationId == aspirationId && a.StudentId == studentId);

//            if (aspiration == null)
//                throw new ArgumentException("Không tìm thấy nguyện vọng");

//            // Kiểm tra thời gian chỉnh sửa nguyện vọng
//            if (aspiration.AcademicYearId.HasValue)
//                await ValidateAspirationEditTimeAsync(aspiration.AcademicYearId.Value);

//            _context.Aspirations.Remove(aspiration);
//            await _context.SaveChangesAsync();

//            return true;
//        }
//    }
//}
