using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.Attendance;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.AttendanceManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendancesController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public AttendancesController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAttendances()
        {
            var attendances = _context.Attendances.Select(a => new AttendanceDTO
            {
                AttendanceId = a.AttendanceId,
                StudentId = a.StudentId,
                ScheduleId = a.ScheduleId,
                Date = a.Date,
                IsPresent = a.IsPresent,
                Note = a.Note
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", attendances));
        }

        [HttpGet("{id}")]
        public IActionResult GetAttendanceById(int id)
        {
            var attendance = _context.Attendances.Where(a => a.AttendanceId == id).Select(a => new AttendanceDTO
            {
                AttendanceId = a.AttendanceId,
                StudentId = a.StudentId,
                ScheduleId = a.ScheduleId,
                Date = a.Date,
                IsPresent = a.IsPresent,
                Note = a.Note
            }).FirstOrDefault();
            if (attendance == null)
            {
                return NotFound(ApiResponse<object>.Fail("Attendance not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", attendance));
        }

        [HttpPost]
        public IActionResult CreateAttendance([FromBody] AttendanceDTO attendanceDTO)
        {
            if (attendanceDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid attendance data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value != null && kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var attendance = new Attendance
            {
                StudentId = attendanceDTO.StudentId,
                ScheduleId = attendanceDTO.ScheduleId,
                Date = attendanceDTO.Date,
                IsPresent = attendanceDTO.IsPresent,
                Note = attendanceDTO.Note
            };
            var result = _context.Attendances.Add(attendance);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Attendance created successfully.", result.Entity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAttendance(int id, [FromBody] AttendanceDTO attendanceDTO)
        {
            if (attendanceDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid attendance data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value != null && kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var attendance = _context.Attendances.Find(id);
            if (attendance == null)
            {
                return NotFound(ApiResponse<object>.Fail("Attendance not found."));
            }
            attendance.StudentId = attendanceDTO.StudentId;
            attendance.ScheduleId = attendanceDTO.ScheduleId;
            attendance.Date = attendanceDTO.Date;
            attendance.IsPresent = attendanceDTO.IsPresent;
            attendance.Note = attendanceDTO.Note;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Attendance updated successfully.", attendance));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAttendance(int id)
        {
            var attendance = _context.Attendances.Find(id);
            if (attendance == null)
            {
                return NotFound(ApiResponse<object>.Fail("Attendance not found."));
            }
            _context.Attendances.Remove(attendance);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Attendance deleted successfully."));
        }
    }
}
