using PorjectManagement.Models;

namespace PorjectManagement.Service.Interface
{
    public interface IUserServices
    {
        void UpdateUser(User user);
        User? GetUser(string email);
        User? CreateAccount(User user);
        bool IsLoginValid(string email, string password);
    }
}
