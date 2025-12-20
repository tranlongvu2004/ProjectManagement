using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Service
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly LabProjectManagementContext _context;

        public TaskHistoryService(LabProjectManagementContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task AddAsync(int taskId, int userId, string action, string description)
        {
            var history = new TaskHistory
            {
                TaskId = taskId,
                UserId = userId,
                Action = action,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _context.TaskHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskHistory>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TaskHistories
                .Include(h => h.User)
                .Where(h => h.TaskId == taskId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<TaskHistory>> GetAllAsync()
        {
            return await _context.TaskHistories
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

    }


}
