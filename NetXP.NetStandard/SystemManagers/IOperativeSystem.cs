using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.SystemManagers
{
    public interface IOperativeSystem
    {
        void Restart();
        void Shutdown();
    }

}
