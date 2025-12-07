using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Repository.Interface
{
    public interface IProjectRepo
    {
        Task<List<Project>> GetAllProjectsAsync(); // Giữ nguyên nếu không có vấn đề
        Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId); // Đổi return type
        Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId);
        Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId);
        Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId);
        //
        Task<int> CreateProjectAsync(Project project);
    }
}
