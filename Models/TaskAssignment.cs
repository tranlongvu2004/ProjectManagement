using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class TaskAssignment
{
    public int TaskAssignmentId { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
