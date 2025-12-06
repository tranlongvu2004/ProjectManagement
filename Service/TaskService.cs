using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;

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
    }
}
