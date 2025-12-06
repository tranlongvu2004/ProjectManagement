using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;

namespace PorjectManagement.Repository
{
    public class TaskRepo : ITaskRepo
    {
        private readonly LabProjectManagementContext _context;

        public TaskRepo(LabProjectManagementContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task AddTaskAsync(Models.Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task<Models.Task?> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.TaskAssignments)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
        }
    }
}
