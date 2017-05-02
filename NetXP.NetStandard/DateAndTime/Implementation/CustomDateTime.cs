using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.DateAndTime.Implementation
{
    public class CustomDateTime : ICustomDateTime
    {
        public CustomDateTime() : this((int)(DateTime.Now - DateTime.UtcNow).TotalMinutes)
        {
        }

        public CustomDateTime(int minutes)
        {
            this.minutes = minutes;
        }

        private int minutes;

        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow.AddMinutes(minutes);
            }
        }

        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public void SetUtcOffset(int minutes)
        {
            this.minutes = minutes; 
        }
    }
}
