using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;

namespace PorjectManagement.Repository
{
    public class UserProjectRepo : IUserProjectRepo
    {
        private readonly LabProjectManagementContext _context;
        
        public UserProjectRepo(LabProjectManagementContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.UserProjects)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        }

        public async Task<bool> IsUserInProjectAsync(int userId, int projectId)
        {
            return await _context.UserProjects
                .AnyAsync(up => up.UserId == userId && up.ProjectId == projectId);
        }

        public async System.Threading.Tasks.Task AddUsersToProjectAsync(List<UserProject> userProjects)
        {
            await _context.UserProjects.AddRangeAsync(userProjects);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByProjectIdAsync(int projectId)
        {
            return await _context.UserProjects
                .Where(up => up.ProjectId == projectId)
                .Select(up => up.User)
                .ToListAsync();
        }

        // ✅ Method từ HEAD - GIỮ LẠI (dùng cho Create/Update Project)
        public async System.Threading.Tasks.Task AddMembersToProjectAsync(
            int projectId, 
            List<int> selectedUserIds, 
            int? leaderId)
        {
            var userProjects = selectedUserIds.Select(userId => new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsLeader = (leaderId.HasValue && userId == leaderId.Value),
                JoinedAt = DateTime.Now
            }).ToList();

            await _context.UserProjects.AddRangeAsync(userProjects);
            await _context.SaveChangesAsync();
        }

        // ✅ Method từ HEAD - GIỮ LẠI (dùng cho Update Project)
        public async System.Threading.Tasks.Task RemoveAllMembersFromProjectAsync(int projectId)
        {
            var userProjects = await _context.UserProjects
                .Where(up => up.ProjectId == projectId)
                .ToListAsync();
            
            _context.UserProjects.RemoveRange(userProjects);
            await _context.SaveChangesAsync();
        }

        // ✅ Method từ dev/Vu - GIỮ LẠI (dùng trong UserProjectService)
        public bool IsleaderOfProject(int userId, int projectId)
        {
            return _context.UserProjects.Any(up =>
                up.UserId == userId &&
                up.ProjectId == projectId &&
                up.IsLeader == true
            );
        }
    }
}
