using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int? ClassId { get; set; }

    public int? SubjectId { get; set; }

    public int? TeacherId { get; set; }

    public string? DayOfWeek { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Room { get; set; }

    public int? AcademicYearId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Class? Class { get; set; }

    public virtual Subject? Subject { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
