using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard
{
    public interface IMessage
    {
        void Warn(string s);
        void Error(string s);
        void Info(string s);
        void Fatal(string s);
        bool Confirm(string s);
    }
}
