using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public int? AcademicYearId { get; set; }

    public int? TeacherId { get; set; }

    public int? SchoolId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual School? School { get; set; }

    public virtual ICollection<StudentClassHistory> StudentClassHistories { get; set; } = new List<StudentClassHistory>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual Teacher? Teacher { get; set; }
}
