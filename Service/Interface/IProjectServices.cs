using PorjectManagement.Models;
using PorjectManagement.Models.ViewModels;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface IProjectServices
    {
        // ===== Methods từ HEAD (dev/nghiafix1) =====
        Task<ProjectDetailDto> GetProjectByIdAsync(int projectId);
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        Task<List<Project>> GetAllProjectsAsync();
        Task<int> CreateProjectWithTeamAsync(ProjectCreateViewModel model, int createdByUserId);
        Task<List<AvailableUserItem>> GetAvailableUsersAsync();

        // ===== Method từ origin/dev/Vu =====
        Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId);
    }
}
