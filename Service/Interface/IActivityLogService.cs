namespace PorjectManagement.Service.Interface
{
    public interface IActivityLogService
    {
        void Log(
            int userId,
            int projectId,
            int? taskId,
            string actionType,
            string message,
            DateTime createdAt,
            int? targetUserId = null);
    }

}
