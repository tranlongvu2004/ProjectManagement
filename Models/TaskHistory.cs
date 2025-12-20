using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class TaskHistory
{
    public int TaskHistoryId { get; set; }

    public int? TaskId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Task? Task { get; set; }

    public virtual User User { get; set; } = null!;
}
