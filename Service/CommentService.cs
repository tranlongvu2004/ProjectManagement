using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service
{
    public class CommentService : ICommentService
    {
        private readonly LabProjectManagementContext _context;

        public CommentService(LabProjectManagementContext context)
        {
            _context = context;
        }

        public async Task<List<CommentDisplayViewModel>> GetCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDisplayViewModel
                {
                    CommentId = c.CommentId,
                    TaskId = c.TaskId,
                    UserId = c.UserId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt ?? DateTime.Now,
                    UserFullName = c.User.FullName,
                    UserAvatarUrl = c.User.AvatarUrl,
                    RoleName = c.User.Role.RoleName
                })
                .ToListAsync();
        }

        public async Task<bool> AddCommentAsync(int taskId, int userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
