namespace PorjectManagement.Service.Interface
{
    public interface IUserServices
    {
        bool IsLoginValid(string email, string password);
    }
}
