using MailKit.Net.Smtp;
using MimeKit;
using NetXP.NetStandard.Network.Email;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.NetCore.Network.Email.Implementations
{
    public class MailKitWrapper : IMailSender
    {
        private MailServer mailServer;

        public void Send(MailMessage message)
        {
            message.IsValid();

            var mimeMessage = new MimeMessage();
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = message.Body;

            mimeMessage.Body = bodyBuilder.ToMessageBody();
            mimeMessage.From.Add(new MailboxAddress(message.From.Alias, message.From.Address));

            foreach (var cc in message.CC)
            {
                mimeMessage.Cc.Add(new MailboxAddress(cc.Alias, cc.Address));
            }
            foreach (var to in message.To)
            {
                mimeMessage.To.Add(new MailboxAddress(to.Alias, to.Address));
            }
            mimeMessage.Subject = message.Subject;

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(mailServer.Server, mailServer.Port, false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(mailServer.Credential.User, mailServer.Credential.Password);

                client.Send(mimeMessage);
                client.Disconnect(true);
            }
        }

        public void Server(MailServer mailServer)
        {
            this.mailServer = mailServer;
            if (mailServer.Credential == null)
                throw new InvalidOperationException("There is not credentials.");
        }
    }
}
