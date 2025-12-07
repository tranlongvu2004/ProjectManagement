using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace PorjectManagement.Repository
{
    public class UserRepo : IUserRepo
    {
        private readonly LabProjectManagementContext _context;

        public UserRepo(LabProjectManagementContext context)
        {
            _context = context;
        }

        public User? CreateAccount(User user)
        {
            var exist = _context.Users.Any(u => u.Email == user.Email);
            if (exist)
            {
                return null; 
            }
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email) ;
        }

        public IQueryable<User> GetUsers()
        {
            return _context.Users.AsQueryable();
        }

        public bool IsloginValid(string email, string password)
        {
            return _context.Users.Any(u => u.Email == email && u.PasswordHash == password);
        }

        public void UpdateUser(User user)
        {
            var exist = _context.Users.FirstOrDefault(u => u.Email ==  user.Email);
            if (exist != null)
            {
                exist.PasswordHash = user.PasswordHash;
            }
            _context.SaveChanges();
        }

        public async Task<List<User>> GetAllUsersWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Status == UserStatus.Active) // Chỉ lấy active users
                .ToListAsync();
        }
    }
}
