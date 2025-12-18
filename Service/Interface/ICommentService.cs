using PorjectManagement.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface ICommentService
    {
        Task<List<CommentDisplayViewModel>> GetCommentsByTaskIdAsync(int taskId);
        Task<bool> AddCommentAsync(int taskId, int userId, string content);
        
    }
}
