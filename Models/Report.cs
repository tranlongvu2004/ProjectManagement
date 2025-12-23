using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class Report
{
    public int ReportId { get; set; }
    public int ProjectId { get; set; }
    public int LeaderId { get; set; }

    public string ReportType { get; set; } = "daily";
    public int TeamExecutePercent { get; set; }
    public string TeamNextPlan { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public virtual User Leader { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<ReportMember> Members { get; set; } = new List<ReportMember>();
}
