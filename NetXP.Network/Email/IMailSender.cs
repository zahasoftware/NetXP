﻿namespace NetXP.Network.Email
{
    public interface IMailSender
    {
        void Server(MailServer mailServer);
        void Send(MailMessage message);
    }
}