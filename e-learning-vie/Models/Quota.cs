using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Quota
{
    public int QuotaId { get; set; }

    public int? SchoolId { get; set; }

    public int? AcademicYearId { get; set; }

    public int? QuotaNumber { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual School? School { get; set; }
}
