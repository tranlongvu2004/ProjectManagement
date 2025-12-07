using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    
    public class ProjectDetailDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Status { get; set; }          
        public DateTime? Deadline { get; set; }      
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProjectMemberItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsLeader { get; set; }
    }

    public class ProjectTaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? ProgressPercent { get; set; }
        public DateTime? Deadline { get; set; }
        public string Assignees { get; set; } = string.Empty;
    }

    public class ProjectWorkspaceViewModel
    {
        public ProjectDetailDto Project { get; set; } = null!;  
        public List<ProjectMemberItem> Members { get; set; } = new();
        public List<ProjectTaskItem> Tasks { get; set; } = new();
        public int OverallProgress { get; set; }
    }
}