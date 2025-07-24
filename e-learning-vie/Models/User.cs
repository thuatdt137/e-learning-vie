using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class User : IdentityUser<int>
{
    public int? StudentId { get; set; }

    public int? TeacherId { get; set; }

    public int? ParentId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }

    public virtual Parent? Parent { get; set; }
}
