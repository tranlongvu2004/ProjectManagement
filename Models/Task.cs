using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;
public enum TaskStatus
{
    ToDo,
    Doing,
    Completed
}
public enum TaskPriority
{
    Low,
    Medium,
    High,
    Necessary
}
public partial class Task
{
    public int TaskId { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public TaskPriority? Priority { get; set; }

    public TaskStatus? Status { get; set; } = TaskStatus.ToDo;

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
