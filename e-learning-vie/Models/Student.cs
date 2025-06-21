using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? ClassId { get; set; }

    public int? SchoolId { get; set; }

    public virtual ICollection<Aspiration> Aspirations { get; set; } = new List<Aspiration>();

    public virtual Class? Class { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual School? School { get; set; }

    public virtual ICollection<StudentClassHistory> StudentClassHistories { get; set; } = new List<StudentClassHistory>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
