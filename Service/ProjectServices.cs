using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
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
        private readonly LabProjectManagementContext _context;

        public ProjectServices(
            IProjectRepo projectRepo,
            IUserProjectRepo userProjectRepo,
            IUserRepo userRepo,
            LabProjectManagementContext context)
        {
            _projectRepo = projectRepo;
            _userProjectRepo = userProjectRepo;
            _userRepo = userRepo;
            _context = context;
        }

        // Danh sách project của user
        public async Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId)
        {
            return await _projectRepo.GetProjectsOfUserAsync(userId);
        }

        // Workspace detail
        public async Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId)
        {
            return await _projectRepo.GetWorkspaceAsync(projectId);
        }

        // Get project by ID
        public async Task<ProjectDetailDto> GetProjectByIdAsync(int projectId)
        {
            return await _projectRepo.GetProjectByIdAsync(projectId);
        }

        // Get members
        public async Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId)
        {
            return await _projectRepo.GetProjectMembersAsync(projectId);
        }

        // Get tasks
        public async Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId)
        {
            return await _projectRepo.GetProjectTasksAsync(projectId);
        }

        public async Task<int> CreateProjectWithTeamAsync(ProjectCreateViewModel model, int createdByUserId)
        {
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

            var memberIds = new List<int> { createdByUserId };

            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                foreach (var userId in model.SelectedUserIds)
                {
                    if (!memberIds.Contains(userId))
                    {
                        memberIds.Add(userId);
                    }
                }
            }

            await _userProjectRepo.AddMembersToProjectAsync(
                projectId,
                memberIds,
                model.LeaderId
            );

            return projectId;
        }

        // Get available users - chỉ InternLead & Intern
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

        public async Task<ProjectUpdateViewModel?> GetProjectForUpdateAsync(int projectId, int mentorId)
        {
            var project = await _projectRepo.GetProjectByIdAsync(projectId);
            
            if (project == null)
                return null;

            var projectEntity = await _projectRepo.GetProjectEntityByIdAsync(projectId);
            if (projectEntity?.CreatedBy != mentorId)
                return null;

            var members = await _projectRepo.GetProjectMembersAsync(projectId);
            var availableUsers = await GetAvailableUsersAsync();
            var currentLeader = members.FirstOrDefault(m => m.IsLeader);

            // ✅ FIX: Parse Status từ string sang enum
            ProjectStatus currentStatus = ProjectStatus.InProgress; // Default
            if (!string.IsNullOrEmpty(project.Status))
            {
                Enum.TryParse<ProjectStatus>(project.Status, out currentStatus);
            }

            return new ProjectUpdateViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                Deadline = project.Deadline,
                Status = currentStatus, // ✅ Sử dụng biến local
                CurrentMemberIds = members.Select(m => m.UserId).ToList(),
                SelectedUserIds = members.Select(m => m.UserId).ToList(),
                CurrentLeaderId = currentLeader?.UserId,
                LeaderId = currentLeader?.UserId,
                AvailableUsers = availableUsers,
                CurrentMembers = members
            };
        }

        public async Task<bool> UpdateProjectWithTeamAsync(ProjectUpdateViewModel model, int updatedByUserId)
        {
            var project = await _projectRepo.GetProjectEntityByIdAsync(model.ProjectId);
            if (project == null)
                return false;

            // ✅ 1. Tìm members bị remove
            var currentMemberIds = project.UserProjects.Select(up => up.UserId).ToList();
            var newMemberIds = model.SelectedUserIds ?? new List<int>();
            newMemberIds.Add(updatedByUserId); // Mentor luôn trong team
            
            var removedMemberIds = currentMemberIds.Except(newMemberIds).ToList();

            // ✅ 2. Set assignee = NULL cho tasks của removed members
            if (removedMemberIds.Any())
            {
                var taskAssignmentsToRemove = await _context.TaskAssignments
                    .Where(ta => removedMemberIds.Contains(ta.UserId) && 
                                 ta.Task.ProjectId == model.ProjectId)
                    .ToListAsync();

                _context.TaskAssignments.RemoveRange(taskAssignmentsToRemove);
                await _context.SaveChangesAsync();
            }

            // Update thông tin project
            project.ProjectName = model.ProjectName;
            project.Description = model.Description;
            project.Deadline = model.Deadline;
            project.Status = model.Status;
            project.UpdatedAt = DateTime.Now;

            await _projectRepo.UpdateProjectAsync(project);

            // Update members
            await _userProjectRepo.RemoveAllMembersFromProjectAsync(model.ProjectId);

            var memberIds = new List<int> { updatedByUserId };
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                foreach (var userId in model.SelectedUserIds)
                {
                    if (!memberIds.Contains(userId))
                    {
                        memberIds.Add(userId);
                    }
                }
            }

            await _userProjectRepo.AddMembersToProjectAsync(
                model.ProjectId,
                memberIds,
                model.LeaderId
            );

            return true;
        }

        public async Task<Project?> GetProjectEntityByIdAsync(int projectId)
        {
            return await _projectRepo.GetProjectEntityByIdAsync(projectId);
        }
    }
}
