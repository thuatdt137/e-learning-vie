using e_learning_vie.Models;
using e_learning_vie.Services.Interfaces;
using e_learning_vie.DTOs.Notification;
using System.Collections.Generic;
using System.Linq;

public class NotificationService : INotificationService
{
    private readonly SchoolManagementContext _context;
    public NotificationService(SchoolManagementContext context)
    {
        _context = context;
    }

    public void CreateNotification(NotificationDTO notification)
    {
        var existSchool = _context.Schools.FirstOrDefault(s => s.SchoolId == notification.SchoolId);
        if (existSchool == null)
        {
            throw new KeyNotFoundException("School not found.");
        }
        var existAcademicYear = _context.AcademicYears.FirstOrDefault(a => a.AcademicYearId == notification.AcademicYearId);
        if (existAcademicYear == null)
        {
            throw new KeyNotFoundException("Academic Year not found.");
        }
        var entity = new Notification
        {
            Content = notification.Content,
            DateSent = notification.DateSent,
            RecipientType = notification.RecipientType,
            SchoolId = notification.SchoolId,
            AcademicYearId = notification.AcademicYearId
        };
        _context.Notifications.Add(entity);
        _context.SaveChanges();
    }

    public dynamic GetNotifications()
    {
        return _context.Notifications.Select(n => new NotificationDTO
        {
            NotificationId = n.NotificationId,
            Content = n.Content,
            DateSent = n.DateSent,
            RecipientType = n.RecipientType,
            SchoolId = n.SchoolId,
            AcademicYearId = n.AcademicYearId
        }).OrderByDescending(n => n.DateSent).ToList();
    }
}
