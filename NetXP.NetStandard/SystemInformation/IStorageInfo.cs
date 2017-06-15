using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemInformation
{
    public interface IStorageInfo
    {
        ICollection<StorageInfo> GetStorageInfo();
    }
}
