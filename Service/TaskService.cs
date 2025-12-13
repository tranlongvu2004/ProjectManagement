using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepo _taskRepo;
        private readonly IUserProjectRepo _userProjectRepo;
        private readonly LabProjectManagementContext _context;

        public TaskService(
            ITaskRepo taskRepo,
            IUserProjectRepo userProjectRepo,
            LabProjectManagementContext context)
        {
            _taskRepo = taskRepo;
            _userProjectRepo = userProjectRepo;
            _context = context;
        }

        public async Task<int> CreateTaskAsync(Models.Task task)
        {
            await _taskRepo.AddTaskAsync(task);
            return task.TaskId;
        }

        public async System.Threading.Tasks.Task AssignUsersToTaskAsync(int taskId, List<int> userIds)
        {
            foreach (var u in userIds)
            {
                _context.TaskAssignments.Add(new TaskAssignment
                {
                    TaskId = taskId,
                    UserId = u
                });
            }
            await _context.SaveChangesAsync();
        }
        public async Task<TaskAssignViewModel> GetAssignTaskDataAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(x => x.Project)
                .ThenInclude(p => p.UserProjects)
                .ThenInclude(up => up.User)
                .FirstOrDefaultAsync(x => x.TaskId == taskId);

            if (task == null) return null;

            var users = task.Project.UserProjects
                .Select(up => new UserListItemVM
                {
                    UserId = up.User.UserId,
                    FullName = up.User.FullName,
                    Email = up.User.Email,
                    AvatarUrl = up.User.AvatarUrl,
                    RoleName = up.User.Role?.RoleName ?? "",
                }).ToList();

            return new TaskAssignViewModel
            {
                TaskId = taskId,
                ProjectId = task.ProjectId,
                Users = users
            };
        }

        public async Task<bool> AssignTaskAsync(int taskId, int userId)
        {
        
            bool exists = await _context.TaskAssignments
                .AnyAsync(x => x.TaskId == taskId && x.UserId == userId);

            if (exists)
                throw new Exception("This intern already assigned for another task");

            
            var newAssignment = new TaskAssignment
            {
                TaskId = taskId,
                UserId = userId,
                AssignedAt = DateTime.Now
            };

            _context.TaskAssignments.Add(newAssignment);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskEditViewModel?> GetTaskForEditAsync(int taskId, int currentUserId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.UserProjects)
                        .ThenInclude(up => up.User)
                            .ThenInclude(u => u.Role)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
                return null;

            // ✅ Check quyền: Chỉ người trong project hoặc creator có thể edit
            var isInProject = task.Project.UserProjects.Any(up => up.UserId == currentUserId);
            if (!isInProject && task.CreatedBy != currentUserId)
                return null;

            var currentAssignees = task.TaskAssignments.Select(ta => new TaskAssigneeItem
            {
                UserId = ta.User.UserId,
                FullName = ta.User.FullName,
                Email = ta.User.Email,
                RoleName = ta.User.Role?.RoleName
            }).ToList();

            var projectMembers = task.Project.UserProjects
                .Select(up => up.User)
                .ToList();

            return new TaskEditViewModel
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority ?? TaskPriority.Low,
                Status = task.Status ?? Models.TaskStatus.ToDo,
                Deadline = task.Deadline,
                CurrentAssigneeIds = task.TaskAssignments.Select(ta => ta.UserId).ToList(),
                SelectedUserIds = task.TaskAssignments.Select(ta => ta.UserId).ToList(),
                ProjectMembers = projectMembers,
                CurrentAssignees = currentAssignees
            };
        }

        public async Task<bool> UpdateTaskAsync(TaskEditViewModel model, int updatedByUserId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskAssignments)
                .FirstOrDefaultAsync(t => t.TaskId == model.TaskId);

            if (task == null)
                return false;

            // Update task info
            task.Title = model.Title;
            task.Description = model.Description;
            task.Priority = model.Priority;
            task.Status = model.Status;
            task.Deadline = model.Deadline;
            task.UpdatedAt = DateTime.Now;

            _context.Tasks.Update(task);

            // ✅ Update assignees
            // Xóa assignees cũ
            var existingAssignments = task.TaskAssignments.ToList();
            _context.TaskAssignments.RemoveRange(existingAssignments);

            // Thêm assignees mới
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                var newAssignments = model.SelectedUserIds.Select(userId => new TaskAssignment
                {
                    TaskId = model.TaskId,
                    UserId = userId,
                    AssignedAt = DateTime.Now
                }).ToList();

                await _context.TaskAssignments.AddRangeAsync(newAssignments);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
