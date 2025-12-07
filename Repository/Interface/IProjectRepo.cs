using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Repository.Interface
{
    public interface IProjectRepo
    {
        Task<List<Project>> GetAllProjectsAsync();
        Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId);
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        
        // Thêm method mới
        Task<int> CreateProjectAsync(Project project);
        Task<List<Project>> GetProjectsOfUserAsync(int userId);
    }
}
