using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class StudentClassHistory
{
    public int HistoryId { get; set; }

    public int? StudentId { get; set; }

    public int? ClassId { get; set; }

    public int? AcademicYearId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Class? Class { get; set; }

    public virtual Student? Student { get; set; }
}
