using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string IdentityCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool IsMale { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();

    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();



    public virtual User? User { get; set; }
}
