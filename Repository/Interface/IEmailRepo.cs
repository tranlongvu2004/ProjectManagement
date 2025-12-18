namespace PorjectManagement.Repository.Interface
{
    public interface IEmailRepo
    {
        void Send(string to, string subject, string body);
    }
}
