// Service/UserProjectService.cs
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Repository;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PorjectManagement.Service
{
    public class UserProjectService : IUserProjectService
    {
        private readonly IUserProjectRepo _repo;
        private readonly LabProjectManagementContext _context;

        public UserProjectService(IUserProjectRepo repo, LabProjectManagementContext context)
        {
            _repo = repo;
            _context = context;
        }
        public bool IsleaderOfProject(int userId, int projectId)
        {
            return _repo.IsleaderOfProject(userId, projectId);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
       .Include(u => u.Role)    
       .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return await _repo.GetProjectByIdAsync(projectId);
        }

        public async Task<bool> IsUserInProjectAsync(int userId, int projectId)
        {
            return await _repo.IsUserInProjectAsync(userId, projectId);
        }

        public async Task<Dictionary<int, string>> AddUsersToProjectAsync(int projectId, List<int> userIds)
        {
            var result = new Dictionary<int, string>();

            if (userIds == null || !userIds.Any())
                return result;

            var existingUserIds = await _context.UserProjects
                .Where(up => up.ProjectId == projectId && userIds.Contains(up.UserId))
                .Select(up => up.UserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();

            var toAdd = new List<UserProject>();

            foreach (var user in users)
            {
                if (existingUserIds.Contains(user.UserId))
                {
                    result[user.UserId] = "exists";
                    continue;
                }

                if (user.Status != UserStatus.Active)
                {
                    result[user.UserId] = "invalid_status";
                    continue;
                }

                toAdd.Add(new UserProject
                {
                    UserId = user.UserId,
                    ProjectId = projectId,
                    JoinedAt = DateTime.Now,
                    IsLeader = false
                });

                result[user.UserId] = "success";
            }

            if (toAdd.Any())
                await _repo.AddUsersToProjectAsync(toAdd);

            return result;
        }


        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<User>> GetUsersByProjectIdAsync(int projectId)
        {
            return await _repo.GetUsersByProjectIdAsync(projectId);
        }
        public async Task<List<User>> GetUsersByProjectIdNoMentorAsync(int projectId)
        {
            return await _repo.GetUsersByProjectIdNoMentorAsync(projectId);
        }

    }
}
