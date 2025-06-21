using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? SchoolId { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual School? School { get; set; }

    public virtual ICollection<School> Schools { get; set; } = new List<School>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
