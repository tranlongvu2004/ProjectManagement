using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface IUserRepo
    {
        IQueryable<User> GetUsers();
        User? GetUserByEmail(string email);
        User? CreateAccount(User user);
        void UpdateUser(User user);

        bool IsloginValid(string email, string password);
    }
}
