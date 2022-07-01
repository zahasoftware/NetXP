using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.SystemInformation
{
    public interface IStorageInfo
    {
        ICollection<StorageInfo> GetStorageInfo();
    }
}
