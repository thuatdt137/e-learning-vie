using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Aspiration
{
    public int AspirationId { get; set; }

    public int? StudentId { get; set; }

    public int? TargetSchoolId { get; set; }

    public int? Priority { get; set; }

    public int? AcademicYearId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Student? Student { get; set; }

    public virtual School? TargetSchool { get; set; }
}
