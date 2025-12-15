using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Student_Profile.Services
{
    public class EmailSender : IEmailSender
    {
        //public Task SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    var client = new SmtpClient("smtp.gmail.com", 587)
        //    {
        //        Credentials = new NetworkCredential("yourEmail@gmail.com", "yourAppPassword"),
        //        EnableSsl = true
        //    };

        //    var mail = new MailMessage("yourEmail@gmail.com", email, subject, htmlMessage);
        //    mail.IsBodyHtml = true;

        //    return client.SendMailAsync(mail);
        //}
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
