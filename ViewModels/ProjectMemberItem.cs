namespace PorjectManagement.ViewModels
{
    public class ProjectMemberItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsLeader { get; set; }
    }
}