using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public int? ProgressPercent { get; set; }

    public DateTime? Deadline { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
}
