using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Models.ViewModels;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service
{
    public class ProjectServices : IProjectServices
    {
        private readonly IProjectRepo _projectRepo;
        private readonly IUserProjectRepo _userProjectRepo;
        private readonly IUserRepo _userRepo;

        public ProjectServices(
            IProjectRepo projectRepo, 
            IUserProjectRepo userProjectRepo,
            IUserRepo userRepo)
        {
            _projectRepo = projectRepo;
            _userProjectRepo = userProjectRepo;
            _userRepo = userRepo;
        }

        public async Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId)
        {
            var projects = await _projectRepo.GetProjectsOfUserAsync(userId);
            
            return projects.Select(p => new ProjectListVM
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                Deadline = p.Deadline,
                Status = p.Status,
                LeaderName = p.UserProjects
                    .Where(up => up.IsLeader == true)
                    .Select(up => up.User.FullName)
                    .FirstOrDefault() ?? "Chưa có Leader",
                MemberCount = p.UserProjects.Count,
                // Lấy tên các members
                Members = string.Join(", ", p.UserProjects.Select(up => up.User.FullName))
            }).ToList();
        }

        // Existing methods...
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _projectRepo.GetAllProjectsAsync();
        }

        // FIX CS8603: Thêm null check và throw exception
        public async Task<ProjectDetailDto> GetProjectByIdAsync(int projectId)
        {
            var project = await _projectRepo.GetProjectByIdAsync(projectId);
            
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }
            
            return project;
        }

        public async Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId)
        {
            return await _projectRepo.GetProjectMembersAsync(projectId);
        }

        public async Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId)
        {
            return await _projectRepo.GetProjectTasksAsync(projectId);
        }

        public async Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId)
        {
            return await _projectRepo.GetWorkspaceAsync(projectId);
        }

        // New methods
        public async Task<int> CreateProjectWithTeamAsync(ProjectCreateViewModel model, int createdByUserId)
        {
            // 1. Tạo Project
            var newProject = new Project
            {
                ProjectName = model.ProjectName,
                Description = model.Description,
                Deadline = model.Deadline,
                Status = ProjectStatus.InProgress,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            int projectId = await _projectRepo.CreateProjectAsync(newProject);

            // 2. Assign members vào project
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                await _userProjectRepo.AddMembersToProjectAsync(
                    projectId, 
                    model.SelectedUserIds, 
                    model.LeaderId
                );
            }

            return projectId;
        }

        public async Task<List<AvailableUserItem>> GetAvailableUsersAsync()
        {
            var users = await _userRepo.GetAllUsersWithRolesAsync();
            
            return users.Select(u => new AvailableUserItem
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role.RoleName
            }).ToList();
        }
    }
}
