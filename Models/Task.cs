using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public enum TaskStatus
{
    ToDo,
    Doing,
    Completed,
    Stuck,
    Not_Started
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

    public int? ParentId { get; set; }

    public bool? IsParent { get; set; }

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

    // Navigation properties

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Task> InverseParent { get; set; } = new List<Task>();

    public virtual Task? Parent { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskAttachment> TaskAttachments { get; set; } = new List<TaskAttachment>();

    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
}
