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
        Task<int> CreateProjectAsync(Project project);
        Task<List<ProjectReportItem>> GetProjectReportAsync(int projectId);
        Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId);
        Task<Project?> GetByIdAsync(int projectId);
      
        Task<Project?> GetProjectEntityByIdAsync(int projectId);
        System.Threading.Tasks.Task UpdateProjectAsync(Project project);
    }
}
