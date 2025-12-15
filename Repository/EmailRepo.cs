using PorjectManagement.Repository.Interface;
using System.Net;
using System.Net.Mail;

namespace PorjectManagement.Repository
{
    public class EmailRepo : IEmailRepo
    {
        public void Send(string to, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.From = new MailAddress("tasklab.noreply@gmail.com");

            var smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("tasklab.noreply@gmail.com", "zhwywlnccylhvlte");
            smtp.EnableSsl = true;

            smtp.Send(message);
        }
    }
}
