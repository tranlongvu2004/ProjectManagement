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
            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(x => x.TaskId == taskId);

            if (assignment == null)
            {
                assignment = new TaskAssignment
                {
                    TaskId = taskId,
                    UserId = userId,
                    AssignedAt = DateTime.Now
                };
                _context.TaskAssignments.Add(assignment);
            }
            else
            {
                assignment.UserId = userId;
                assignment.AssignedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
