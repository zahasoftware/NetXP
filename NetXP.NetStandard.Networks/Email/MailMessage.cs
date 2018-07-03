using NetXP.NetStandard.Exceptions;
using NetXP.NetStandard.Network.Email;

namespace NetXP.NetStandard.Network.Email
{
    public class MailMessage
    {
        public MailMessage()
        {
            To = new MailAddressCollection();
            CC = new MailAddressCollection();
        }

        public string Body { get; set; }
        public MailAddressCollection CC { get; private set; }
        public MailAddress From { get; set; }
        public string Subject { get; set; }
        public MailAddressCollection To { get; private set; }
        public bool IsBodyHtml { get; set; }

        public void IsValid()
        {
            string message = "";
            if (From == null)
            {
                message += " From is not set,";
            }

            if (To.Count == 0)
            {
                message += " Should have at least one receptor,";
            }

            if (message != "")
            {
                message = message.Remove(message.Length - 1);
                throw new CustomApplicationException(message);
            }
        }

    }
}