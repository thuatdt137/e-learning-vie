using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class User : IdentityUser<int>
{
    public int? StudentId { get; set; }

    public int? TeacherId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Request> RequestApprovedByNavigations { get; set; } = new List<Request>();

    public virtual ICollection<Request> RequestCreatedByNavigations { get; set; } = new List<Request>();

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
