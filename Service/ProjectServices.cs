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
        public async Task<List<Project>> GetProjectsOfUserAsync(int userId)
        {
            return await _projectRepo.GetProjectsOfUserAsync(userId);
        }

        // Workspace detail
        public async Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId)
        {
            var workspace = await _projectRepo.GetWorkspaceAsync(projectId);
            
            if (workspace == null)
                return null;

            if (workspace.Project != null)
            {
                workspace.Project.Status = workspace.OverallProgress >= 100
                    ? "Completed"
                    : "InProgress";
            }

            return workspace;
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

        // Get available users - bỏ Mentor (RoleId = 1)
        public async Task<List<AvailableUserItem>> GetAvailableUsersAsync()
        {
            var users = await _userRepo.GetAllUsersWithRolesAsync();

            return users
                .Where(u => u.RoleId != 1) 
                .Select(u => new AvailableUserItem
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

            return new ProjectUpdateViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                Deadline = project.Deadline,
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

            // method Calculator status
            ProjectStatus newStatus = await CalculateProjectStatusAsync(model.ProjectId);

            // Tìm members bị remove
            var currentMemberIds = project.UserProjects.Select(up => up.UserId).ToList();
            var newMemberIds = model.SelectedUserIds ?? new List<int>();
            newMemberIds.Add(updatedByUserId); 

            var removedMemberIds = currentMemberIds.Except(newMemberIds).ToList();

            // Set assignee = NULL cho tasks của removed members
            if (removedMemberIds.Any())
            {
                var taskAssignmentsToRemove = await _context.TaskAssignments
                    .Where(ta => removedMemberIds.Contains(ta.UserId) &&
                                 ta.Task.ProjectId == model.ProjectId)
                    .ToListAsync();

                _context.TaskAssignments.RemoveRange(taskAssignmentsToRemove);
                await _context.SaveChangesAsync();
            }

            // Update inform project
            project.ProjectName = model.ProjectName;
            project.Description = model.Description;
            project.Deadline = model.Deadline;
            project.Status = newStatus;
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

        public int GetProjectId(int taskId)
        {
            return _context.Tasks
                .Where(t => t.TaskId == taskId)
                .Select(t => t.ProjectId)
                .First();
        }
        public async System.Threading.Tasks.Task UpdateProjectStatusAsync(int projectId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                return;

            // Tính status mới theo tasks
            var newStatus = await CalculateProjectStatusAsync(projectId);

            // update nếu status thay đổi
            if (project.Status != newStatus)
            {
                project.Status = newStatus;
                project.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // Tính status theo tasks 
        public async Task<ProjectStatus> CalculateProjectStatusAsync(int projectId)
        {
            var progressPercentage = await GetProgressPercentageAsync(projectId);

            // 100% completed → Completed
            if (progressPercentage >= 100)
            {
                return ProjectStatus.Completed;
            }

            return ProjectStatus.InProgress;
        }

        // % hoàn thành dựa trên tasks
        public async Task<int> GetProgressPercentageAsync(int projectId)
        {
            // List task IDs trong RecycleBin
            var deletedTaskIds = await _context.RecycleBins
                .Where(rb => rb.EntityType == "Task")
                .Select(rb => rb.EntityId)
                .ToListAsync();

            // List parent tasks (không bị xóa)
            var parentTasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId
                    && t.IsParent == true
                    && t.ParentId == null
                    && !deletedTaskIds.Contains(t.TaskId))
                .ToListAsync();

            if (!parentTasks.Any())
                return 0;

            // Đếm parent tasks completed
            int completedCount = parentTasks.Count(t => t.Status == PorjectManagement.Models.TaskStatus.Completed);
            int totalCount = parentTasks.Count;

            // Tính %
            int percentage = (int)Math.Round((double)completedCount / totalCount * 100);

            return percentage;
        }
    }
}
