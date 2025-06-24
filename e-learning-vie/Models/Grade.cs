using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    public int? StudentId { get; set; }

    public string? GradeType { get; set; }

    public int? SubjectId { get; set; }

    public double? Score { get; set; }

    public DateOnly? DateEntered { get; set; }

    public int? AcademicYearId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Subject? Subject { get; set; }
}
