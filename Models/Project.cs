using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;
public enum ProjectStatus
{
    InProgress,
    Completed,
    Dropped
}
public partial class Project
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Deadline { get; set; }

    public ProjectStatus? Status { get; set; } = ProjectStatus.InProgress;

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
