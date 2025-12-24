using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Student_Profile.Services
{
    public class EmailSender : IEmailSender
    {
        Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mernasaad131@gmail.com", "jqgf bduh dbib svub")
            };

            return client.SendMailAsync(
         new MailMessage(from: "mernasaad131@gmail.com",
                         to: email,
                         subject,
                         htmlMessage
                         )
         {
             IsBodyHtml = true
         });
        }
    }
}
