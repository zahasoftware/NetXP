using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetXP.Auditory
{
    public interface ILogger
    {
        void Info(string msg);
        void Error(string msg);
        void Error(string msg, Exception ex);
        void Warn(string msg);

        void Error(Exception ex);
        void Debug(string msg,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
    }
}
