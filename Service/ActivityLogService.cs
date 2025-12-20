using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Service
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly LabProjectManagementContext _context;

        public ActivityLogService(LabProjectManagementContext context)
        {
            _context = context;
        }

        public void Log(
            int userId,
            int projectId,
            int? taskId,
            string actionType,
            string message,
            DateTime createdAt,
            int? targetUserId = null)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                ProjectId = projectId,
                TaskId = taskId,
                ActionType = actionType,
                Message = message,
                TargetUserId = targetUserId,
                CreatedAt = createdAt
            });

            _context.SaveChanges();
        }
    }

}
