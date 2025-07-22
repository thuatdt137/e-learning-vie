using e_learning_vie.Commons;
using e_learning_vie.DTOs.Notification;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace e_learning_vie.Controllers.NotificationsController
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpGet]
        public IActionResult GetAllNotifications()
        {
            try
            {
                var notifications = _notificationService.GetNotifications();
                return Ok(ApiResponse<dynamic>.Success(notifications, "Notifications retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<Object>.Error($"Error retrieving notifications: {ex.Message}"));
            }
        }
        [HttpPost]
        public IActionResult CreateNotification([FromBody] NotificationDTO notification)
        {

            if (notification == null)
            {
                return BadRequest(ApiResponse<Object>.Fail("Notification data is required."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors
                        .Select(e => e.ErrorMessage).ToArray()
                    );
                return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<object>.Fail("Invalid notification data.", errors));
            }
            try
            {
                _notificationService.CreateNotification(notification);
                return Ok(ApiResponse<Object>.Success(null, "Notification created successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<Object>.Error($"Error creating notification: {ex.Message}"));
            }
        }
    }
}
