using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class ClassHistory
{
    public int HistoryId { get; set; }

    public int? StudentId { get; set; }

    public int? TeacherId { get; set; }

    public int? ClassId { get; set; }

    public int? SchoolId { get; set; }

    public string? ClassName { get; set; }

    public string? SchoolName { get; set; }

    public int? AcademicYearId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Class? Class { get; set; }

    public virtual School? School { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
