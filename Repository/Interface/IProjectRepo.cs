using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Repository.Interface
{
    public interface IProjectRepo
    {
        // ===== Methods từ HEAD (dev/nghiafix1) =====
        Task<List<Project>> GetAllProjectsAsync();
        Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId);
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        Task<int> CreateProjectAsync(Project project);

        // ===== Methods từ origin/dev/Vu =====
        Task<List<Project>> GetProjectsOfUserAsync(int userId);
        Task<Project?> GetByIdAsync(int projectId);
    }
}
