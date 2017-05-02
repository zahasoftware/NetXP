using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmartSecurity.NetStandard.net.mail.i
{
    public class MailSender : IMailSender, IDisposable
    {
        private readonly SmtpClient smtpClient;

        public MailSender()
        {
            smtpClient = new SmtpClient();
        }

        public void Server(MailServer mailServer)
        {
            smtpClient.Credentials =
                new System.Net.NetworkCredential(mailServer.Credential.User, mailServer.Credential.Password);
            smtpClient.Host = mailServer.Server;
            smtpClient.Port = mailServer.Port;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
        }

        public void Send(MailMessage message)
        {
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = message.Body;
            mailMessage.From = message.From;
            mailMessage.Subject = message.Subject;

            if (message.To != null)
            {
                foreach (var mailAddress in message.To)
                {
                    mailMessage.To.Add(mailAddress);
                }
            }

            if (message.CC != null)
            {
                foreach (var mailAddress in message.CC)
                {
                    mailMessage.CC.Add(mailAddress);
                }
            }
            smtpClient.Send(mailMessage);
        }

        public void Dispose()
        {
            smtpClient.Dispose();
        }
    }
}
