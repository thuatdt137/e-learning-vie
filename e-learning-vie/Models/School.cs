using e_learning_vie.Enums;
using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class School
{
    public int SchoolId { get; set; }

    public string SchoolName { get; set; } = null!;

    public SchoolType SchoolType { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? PrincipalId { get; set; }

    public virtual ICollection<Aspiration> Aspirations { get; set; } = new List<Aspiration>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Teacher? Principal { get; set; }

    public virtual ICollection<Quota> Quota { get; set; } = new List<Quota>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
