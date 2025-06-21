using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Request
{
    public int RequestId { get; set; }

    public string? RequestType { get; set; }

    public int? CreatedBy { get; set; }

    public string? Status { get; set; }

    public string? Details { get; set; }

    public int? AcademicYearId { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
