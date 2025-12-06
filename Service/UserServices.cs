using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Service
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepo _userRepo;

        public UserServices(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public User? CreateAccount(User user)
        {
            return _userRepo.CreateAccount(user);
        }

        public User? GetUser(string email)
        {
            return _userRepo.GetUserByEmail(email);
        }

        public bool IsLoginValid(string email, string password)
        {
            return _userRepo.IsloginValid(email, password);
        }

        public void UpdateUser(User user)
        {
            _userRepo.UpdateUser(user);
        }
    }
}
