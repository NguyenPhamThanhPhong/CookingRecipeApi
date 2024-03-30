using CookingRecipeApi.Configs;
using System.Net.Mail;

namespace CookingRecipeApi.Services.SMTPServices
{
    public class EmailService
    {
        private readonly SMTPConfigs _SMTPConfigs;
        public EmailService(SMTPConfigs SMTPConfigs)
        {
            _SMTPConfigs = SMTPConfigs;
        }
        public async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            string senderEmail = _SMTPConfigs.FromEmail;
            string senderPassword = _SMTPConfigs.Password;

            SmtpClient smtpClient = new SmtpClient(_SMTPConfigs.SMTPServer, _SMTPConfigs.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            MailMessage mailMessage = new MailMessage(from:senderEmail,to: toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("error is");
                Console.WriteLine(ex.Message);
                Console.WriteLine("end ------------");
                return false;
            }
        }

    }
}
