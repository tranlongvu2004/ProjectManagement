using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class ProjectListVM
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime Deadline { get; set; }
        public ProjectStatus? Status { get; set; }
        public string LeaderName { get; set; }
        public int MemberCount { get; set; }
    }
}
