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

        public bool IsLoginValid(string email, string password)
        {
            return _userRepo.IsloginValid(email, password);
        }
    }
}
