using PorjectManagement.Models;
using PorjectManagement.Models.ViewModels;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface IProjectServices
    {
        Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId);
        Task<ProjectDetailDto> GetProjectByIdAsync(int projectId);
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        Task<List<Project>> GetAllProjectsAsync();

        // Thêm methods mới
        Task<int> CreateProjectWithTeamAsync(ProjectCreateViewModel model, int createdByUserId);
        Task<List<AvailableUserItem>> GetAvailableUsersAsync();

    }
}
