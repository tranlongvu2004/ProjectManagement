using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service
{
    public class ProjectServices : IProjectServices
    {
        private readonly IProjectRepo _projectRepo;

        public ProjectServices(IProjectRepo projectRepo)
        {
            _projectRepo = projectRepo;
        }
        public Task<List<Project>> GetAllProjectsAsync()
        {
            return _projectRepo.GetAllProjectsAsync();
        }

        // Chỉ call repository, nếu thêm logic thì thêm sau kiểu thế

        public Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId)
        {
            return _projectRepo.GetProjectByIdAsync(projectId);
        }

        public Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId)
        {
            return _projectRepo.GetProjectMembersAsync(projectId);
        }

        public Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId)
        {
            return _projectRepo.GetProjectTasksAsync(projectId);
        }

        public Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId)
        {
            return _projectRepo.GetWorkspaceAsync(projectId);
        }
    }
}
