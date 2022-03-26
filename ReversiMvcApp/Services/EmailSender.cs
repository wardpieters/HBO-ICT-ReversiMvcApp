using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ReversiMvcApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string fromEmail = "no-reply@ward.nl";
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpClient client = new SmtpClient
            {
                Port = 1025,
                Host = "localhost",
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
            };
            
            MailMessage message = new MailMessage(fromEmail, email, subject, htmlMessage);
            message.IsBodyHtml = true;

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("E-mail niet verzonden!");
                Console.WriteLine(e);
            }
        }
    }
}