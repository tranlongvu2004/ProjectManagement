using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class ActivityLog
{
    public int ActivityLogId { get; set; }

    public int UserId { get; set; }

    public int? TargetUserId { get; set; }

    public int ProjectId { get; set; }

    public int? TaskId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User? TargetUser { get; set; }

    public virtual Task? Task { get; set; }

    public virtual User User { get; set; } = null!;
}

public static class ActivityAction
{
    public const string TASK_CREATED = "TASK_CREATED";
    public const string TASK_UPDATED = "TASK_UPDATED";
    public const string TASK_ASSIGNED = "TASK_ASSIGNED";
    public const string STATUS_CHANGED = "STATUS_CHANGED";
    public const string ATTACHMENT_UPLOADED = "ATTACHMENT_UPLOADED";
    public const string COMMENT_ADDED = "COMMENT_ADDED";
}

