namespace e_learning_vie.DTOs.Notification;

public class NotificationDTO
{
    public int NotificationId { get; set; }

    public string? Content { get; set; }

    public DateOnly? DateSent { get; set; }

    public string? RecipientType { get; set; }

    public int? SchoolId { get; set; }

    public int? AcademicYearId { get; set; }
}