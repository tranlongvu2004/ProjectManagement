using PorjectManagement.Models;

namespace PorjectManagement.Service.Interface
{
    public interface IUserServices
    {
        void UpdateUser(User user);
        void UpdateProfile(User user);
        void DeleteUser(string email);
        User? GetUserById(int userId);
        User? GetUser(string email);
        User? CreateAccount(User user);
        bool IsLoginValid(string email, string password);
    }
}
