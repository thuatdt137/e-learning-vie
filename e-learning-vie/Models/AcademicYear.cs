using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class AcademicYear
{
    public int AcademicYearId { get; set; }

    public string YearName { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();


}
