namespace PorjectManagement.Service.Interface
{
    public interface IEmailService
    {
        void Send(string to, string subject, string body);
    }
}
