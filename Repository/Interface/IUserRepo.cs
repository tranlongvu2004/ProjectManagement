using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface IUserRepo
    {
        IQueryable<User> GetUsers();
        User? getUserById(int userId);
        User? GetUserByEmail(string email);
        User? CreateAccount(User user);
        void UpdateUser(User user);
        void UpdateProfile(User user);
        void DeleteUser(string email);
        bool IsloginValid(string email, string password);
        Task<List<User>> GetAllUsersWithRolesAsync();
    }
}
