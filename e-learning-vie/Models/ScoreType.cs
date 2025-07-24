using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class ScoreType
{
    public int ScoreId { get; set; }

    public string? TypeName { get; set; }

    public double Weight { get; set; }

    public string? Description { get; set; }

    public ICollection<StudentScore> StudentScores { get; set;} = new List<StudentScore>();
}
