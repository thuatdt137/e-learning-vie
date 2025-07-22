
using e_learning_vie.DTOs.Notification;

namespace e_learning_vie.Services.Interfaces;

public interface INotificationService
{
    void CreateNotification(NotificationDTO notification);
    dynamic GetNotifications();
}