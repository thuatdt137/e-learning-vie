using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string? Content { get; set; }

    public DateOnly? DateSent { get; set; }

    public string? RecipientType { get; set; }

    public int? SchoolId { get; set; }

    public int? AcademicYearId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual School? School { get; set; }
}
