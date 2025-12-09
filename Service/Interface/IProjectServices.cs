using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface IProjectServices
    {
        // Danh sách project của user
        Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId);
        
        // Workspace chi tiết
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        
        // Create project
        Task<int> CreateProjectWithTeamAsync(ProjectCreateViewModel model, int createdByUserId);
        Task<List<AvailableUserItem>> GetAvailableUsersAsync();
        
        // Các methods khác nếu cần
        Task<ProjectDetailDto> GetProjectByIdAsync(int projectId);
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);
    }
}
