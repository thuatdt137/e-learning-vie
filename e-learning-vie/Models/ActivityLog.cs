using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class ActivityLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public DateTime? ChangeDate { get; set; }

    public string? Details { get; set; }

    public virtual User? User { get; set; }
}
