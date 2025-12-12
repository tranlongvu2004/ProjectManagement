namespace PorjectManagement.ViewModels
{
    public class ProjectWorkspaceViewModel
    {
        public ProjectDetailDto Project { get; set; } = null!;
        public List<ProjectMemberItem> Members { get; set; } = new();
        public List<ProjectTaskItem> Tasks { get; set; } = new();
        public int OverallProgress { get; set; } // Tính % hoàn thành tổng thể
    }
}