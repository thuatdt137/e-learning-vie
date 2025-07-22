using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public int? SchoolId { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual School? School { get; set; }

    public virtual ICollection<ClassHistory> StudentClassHistories { get; set; } = new List<ClassHistory>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual Teacher? Teacher { get; set; }
}
