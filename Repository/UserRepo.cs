using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;

namespace PorjectManagement.Repository
{
    public class UserRepo : IUserRepo
    {
        private readonly LabProjectManagementContext _context;

        public UserRepo(LabProjectManagementContext context)
        {
            _context = context;
        }

        public User? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email.Equals(email)) ;
        }

        public IQueryable<User> GetUsers()
        {
            return _context.Users.AsQueryable();
        }

        public bool IsloginValid(string email, string password)
        {
            return _context.Users.Any(u => u.Email == email && u.PasswordHash == password);
        }
    }
}
