using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemInfo
{
    public interface IStorageInfo
    {
        ICollection<StorageInfo> GetStorageInfo();
    }
}
