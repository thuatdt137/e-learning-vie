using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class AcademicYear
{
    public int AcademicYearId { get; set; }

    public string YearName { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    // Thời gian mở đăng ký nguyện vọng
    public DateOnly? AspirationRegistrationStartDate { get; set; }
    
    // Thời gian đóng đăng ký nguyện vọng
    public DateOnly? AspirationRegistrationEndDate { get; set; }
    
    // Thời gian đóng chỉnh sửa nguyện vọng (sau này chỉ xem được)
    public DateOnly? AspirationEditDeadline { get; set; }

    public virtual ICollection<Aspiration> Aspirations { get; set; } = new List<Aspiration>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Quota> Quota { get; set; } = new List<Quota>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<ClassHistory> ClassHistories { get; set; } = new List<ClassHistory>();
}
