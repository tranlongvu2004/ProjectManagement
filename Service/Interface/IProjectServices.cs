using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface IProjectServices
    {
        // Danh sách project của user
        Task<List<Project>> GetProjectsOfUserAsync(int userId);
        
        // Workspace chi tiết
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        
        // Create project
        Task<int> CreateProjectWithTeamAsync(ProjectCreateViewModel model, int createdByUserId);
        Task<List<AvailableUserItem>> GetAvailableUsersAsync();
        
        Task<ProjectDetailDto> GetProjectByIdAsync(int projectId);
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);

        Task<Project?> GetProjectEntityByIdAsync(int projectId);
        int GetProjectId(int taskId);
        Task<ProjectUpdateViewModel?> GetProjectForUpdateAsync(int projectId, int mentorId);
        Task<bool> UpdateProjectWithTeamAsync(ProjectUpdateViewModel model, int updatedByUserId);

        System.Threading.Tasks.Task UpdateProjectStatusAsync(int projectId);
        Task<ProjectStatus> CalculateProjectStatusAsync(int projectId);
        Task<int> GetProgressPercentageAsync(int projectId);

    }
}
