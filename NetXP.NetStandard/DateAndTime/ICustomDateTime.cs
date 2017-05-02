using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.DateAndTime
{
    /// <summary>
    /// Get the universal time + desface
    /// Ex: If you set desface to -60*4 and UTC is 5:00 you get 1:00 as hour
    /// </summary>
    public interface ICustomDateTime
    {
        DateTime Now { get; }

        void SetUtcOffset(int minutes);

        DateTime UtcNow { get; }
    }
}