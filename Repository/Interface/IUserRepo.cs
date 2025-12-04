using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface IUserRepo
    {
        IQueryable<User> GetUsers();
        User? GetUserByEmail(string email);
        bool IsloginValid(string email, string password);
    }
}
