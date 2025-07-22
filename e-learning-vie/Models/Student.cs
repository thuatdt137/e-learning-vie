using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string IdentityCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Aspiration> Aspirations { get; set; } = new List<Aspiration>();

    public virtual ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();

    public virtual ICollection<ClassHistory> StudentClassHistories { get; set; } = new List<ClassHistory>();

    public virtual User? User { get; set; }
}
