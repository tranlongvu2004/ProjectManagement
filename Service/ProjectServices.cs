using PorjectManagement.Models.ViewModels;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Service
{
    public class ProjectServices : IProjectServices
    {
        private readonly IProjectRepo _projectRepo;

        public ProjectServices(IProjectRepo projectRepo)
        {
            _projectRepo = projectRepo;
        }

        public async Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId)
        {
            var projects = await _projectRepo.GetProjectsOfUserAsync(userId);

            return projects.Select(p => new ProjectListVM
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                Deadline = p.Deadline,
                Status = p.Status,
                LeaderName = p.UserProjects
                    .Where(x => x.IsLeader == true)
                    .Select(x => x.User.FullName)
                    .FirstOrDefault() ?? "Không xác định",
                MemberCount = p.UserProjects.Count
            }).ToList();
        }
    }
}
