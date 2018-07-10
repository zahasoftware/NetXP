using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.Email
{
    public class MailAddressCollection : ICollection<MailAddress>
    {
        List<MailAddress> mailAddress = new List<MailAddress>();
        public int Count => mailAddress.Count;

        public bool IsReadOnly => true;

        public void Add(MailAddress item)
        {
            mailAddress.Add(item);
        }

        public void Clear()
        {
            mailAddress.Clear();
        }

        public bool Contains(MailAddress item)
        {
            return mailAddress.Contains(item);
        }

        public void CopyTo(MailAddress[] array, int arrayIndex)
        {
            mailAddress.CopyTo(array, arrayIndex);
        }

        public IEnumerator<MailAddress> GetEnumerator()
        {
            return mailAddress.GetEnumerator();
        }

        public bool Remove(MailAddress item)
        {
            return this.mailAddress.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mailAddress.GetEnumerator();
        }
    }
}
