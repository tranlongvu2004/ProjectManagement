using PorjectManagement.Repository.Interface;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepo _emailRepo;

        public EmailService(IEmailRepo emailRepo)
        {
            _emailRepo = emailRepo;
        }

        public void Send(string to, string subject, string body)
        {
             _emailRepo.Send(to, subject, body);
        }
    }
}
