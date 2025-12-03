/*
using System.Net;
using System.Net.Mail;

namespace TeklifYonetimSistemi.Services
{
    public class MicrosoftEmailService : IEmailService
    {
        private readonly string _fromEmail = "TechSolutions@outlook.com.tr";
        private readonly string _password= 

        public async Task SendEmailAsync(string toEmail,string subject,string body)
        {
            var smtp = new SmtpClient("smtp.office365.com")//smtp sunucusu belirttik burada örnek olarak microsoft kullanıldı
            {
                Port = 587, // STARTTLS kullanan çoğu smtp sunucusunun kullandığı port
                Credentials = new NetworkCredential(_fromEmail, _password),
                //smtp suncusunda kimlik doğrulama için kullanıcı adı ve parola gerekir, gmailde app password kullan
                EnableSsl=true
                //sunucu ile TLS/SSL bağlantısı kurulmasını sağlar
            };
            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail), //gönderen adresi
                Subject = subject,
                Body=body,
                IsBodyHtml=true

            };
            mail.To.Add(toEmail);
            await smtp.SendMailAsync(mail); //smtp client ile maili asenkron gönderdik ve thread bloklanmadı
        }
        public async Task SendEmailWithAttachmentAsync(string toEmail,string subject,string body, byte[] attachmentBytes,string attachmentName)
        {
            //attachmentBytes -> pdf resim vb byte erray
            var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = true
            };
            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail), //gönderen adresi
                Subject = subject,
                Body = body,
                IsBodyHtml = true

            };
            mail.To.Add(toEmail); // PDF ekle
            mail.Attachments.Add(new Attachment(new MemoryStream(attachmentBytes), attachmentName));
            //memoryStream- byte array'i stream'e çevirir
            //Attachments maile eklenir
            //pdf böylece doprudan belleklten eklenir
            await smtp.SendMailAsync(mail);

        }
    }
}

using System.Net;
using System.Net.Mail;

namespace TeklifYonetimSistemi.Services
{
    public class MicrosoftEmailService : IEmailService
    {
        private readonly string _fromEmail = "TechSolutions@outlook.com.tr";
        private readonly string _password = "Techsolution-123!";

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.office365.com") // smtp sunucusu belirttik burada örnek olarak microsoft kullanıldı
            {
                Port = 587, // STARTTLS kullanan çoğu smtp sunucusunun kullandığı port
                Credentials = new NetworkCredential(_fromEmail, _password),
                // smtp sunucusunda kimlik doğrulama için kullanıcı adı ve parola gerekir, gmailde app password kullan
                EnableSsl = true
                // sunucu ile TLS/SSL bağlantısı kurulmasını sağlar
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail), // gönderen adresi
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail); // smtp client ile maili asenkron gönderdik ve thread bloklanmadı
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachmentBytes, string attachmentName)
        {
            // attachmentBytes -> pdf, resim vb byte array
            var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail), // gönderen adresi
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            mail.Attachments.Add(new Attachment(new MemoryStream(attachmentBytes), attachmentName));
            // memoryStream -> byte array'i stream'e çevirir
            // Attachments maile eklenir
            // pdf böylece doğrudan bellekten eklenir

            await smtp.SendMailAsync(mail);
        }
    }
}

*/
using System.Net;
using System.Net.Mail;

namespace TeklifYonetimSistemi.Services
{
    public class GmailEmailService : IEmailService
    {
        private readonly string _fromEmail = "cihaterensoy@gmail.com";
        private readonly string _password = "sagi urpr bnbo jjrq"; // 16 haneli uygulama şifresi

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail);
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachmentBytes, string attachmentName)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            mail.Attachments.Add(new Attachment(new MemoryStream(attachmentBytes), attachmentName));

            await smtp.SendMailAsync(mail);
        }
    }
}
