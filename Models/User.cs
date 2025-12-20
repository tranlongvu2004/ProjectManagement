using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;
public enum UserStatus
{
    Active,
    Inactive,
    Dropped
}

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string? AvatarUrl { get; set; }

    public UserStatus? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogTargetUsers { get; set; } = new List<ActivityLog>();

    public virtual ICollection<ActivityLog> ActivityLogUsers { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskAttachment> TaskAttachments { get; set; } = new List<TaskAttachment>();

    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
