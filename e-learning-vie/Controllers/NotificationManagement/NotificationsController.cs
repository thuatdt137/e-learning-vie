using Microsoft.AspNetCore.Mvc;
using e_learning_vie.Models;
using e_learning_vie.DTOs.Notification;
using e_learning_vie.Commons;

namespace e_learning_vie.Controllers.NotificationManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;

        public NotificationsController(SchoolManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllNotifications()
        {
            var notifications = _context.Notifications.Select(n => new NotificationDTO
            {
                NotificationId = n.NotificationId,
                Content = n.Content,
                DateSent = n.DateSent,
                RecipientType = n.RecipientType,
                AcademicYearId = n.AcademicYearId
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", notifications));
        }

        [HttpGet("{id}")]
        public IActionResult GetNotificationById(int id)
        {
            var notification = _context.Notifications.Where(n => n.NotificationId == id).Select(n => new NotificationDTO
            {
                NotificationId = n.NotificationId,
                Content = n.Content,
                DateSent = n.DateSent,
                RecipientType = n.RecipientType,
                AcademicYearId = n.AcademicYearId
            }).FirstOrDefault();

            if (notification == null)
            {
                return NotFound(ApiResponse<object>.Fail("Notification not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", notification));
        }

        [HttpPost]
        public IActionResult CreateNotification([FromBody] NotificationDTO notificationDTO)
        {
            if (notificationDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid notification data."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value?.Errors?.Count > 0)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(ApiResponse<object>.Fail("Model validation failed.", errors));
            }

            try
            {
                var notification = new Notification
                {
                    Content = notificationDTO.Content,
                    DateSent = notificationDTO.DateSent,
                    RecipientType = notificationDTO.RecipientType,
                    AcademicYearId = notificationDTO.AcademicYearId
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("Notification created successfully.", notification));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateNotification(int id, [FromBody] NotificationDTO notificationDTO)
        {
            if (notificationDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid notification data."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(kvp => kvp.Value?.Errors?.Count > 0)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(ApiResponse<object>.Fail("Model validation failed.", errors));
            }

            try
            {
                var notification = _context.Notifications.FirstOrDefault(n => n.NotificationId == id);
                if (notification == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Notification not found."));
                }

                notification.Content = notificationDTO.Content;
                notification.DateSent = notificationDTO.DateSent;
                notification.RecipientType = notificationDTO.RecipientType;
                notification.AcademicYearId = notificationDTO.AcademicYearId;

                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("Notification updated successfully.", notification));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNotification(int id)
        {
            try
            {
                var notification = _context.Notifications.FirstOrDefault(n => n.NotificationId == id);
                if (notification == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Notification not found."));
                }

                _context.Notifications.Remove(notification);
                _context.SaveChanges();

                return Ok(ApiResponse<object>.Success("Notification deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
