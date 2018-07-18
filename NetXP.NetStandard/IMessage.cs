using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard
{
    public interface IMessage
    {
        void Warn(string message);
        void Error(string message);
        void Info(string message);
        void Fatal(string message);
        Task<bool> Confirm(string message);
    }
}
