using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class UserProject
{
    public int UserProjectId { get; set; }

    public int UserId { get; set; }

    public int ProjectId { get; set; }

    public bool? IsLeader { get; set; }

    public DateTime? JoinedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
