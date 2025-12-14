using MailKit.Net.Smtp;
using MimeKit;

namespace PorjectManagement.Controllers
{
    public class EmailSender
    {
        private readonly IConfiguration _config;


        public EmailSender(IConfiguration config)
        {
            _config = config;
        }


        public async Task SendAsync(string to, string subject, string html)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Smtp:From"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;


            email.Body = new BodyBuilder
            {
                HtmlBody = html
            }.ToMessageBody();


            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
            _config["Smtp:Host"],
            int.Parse(_config["Smtp:Port"]),
            false);


            await smtp.AuthenticateAsync(
            _config["Smtp:Username"],
            _config["Smtp:Password"]);


            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
