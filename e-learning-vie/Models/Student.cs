using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string IdentityCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool IsMale { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();


    public virtual User? User { get; set; }
}
