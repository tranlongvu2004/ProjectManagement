// Repository/UserProjectRepo.cs
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
    }
}
