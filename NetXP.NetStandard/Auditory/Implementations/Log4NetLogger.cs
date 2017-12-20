using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Diagnostics;
using NetXP.NetStandard.Auditory;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace NetXP.NetStandard.Auditory.Implementations
{
    public class Log4NetLogger : ILogger
    {
        protected static ILog log = LogManager.GetLogger(typeof(ILogger));

        public virtual void Debug(string msg,
                                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            ReloadConfig();
            string @class = Path.GetFileNameWithoutExtension(sourceFilePath);
            log.Debug($"<{sourceLineNumber}:{@class}.{memberName}>: {msg}");
        }

        private static void ReloadConfig()
        {
            log = LogManager.GetLogger(typeof(ILogger));
        }
        public virtual void Info(string msg)
        {
            ReloadConfig(); ;
            log.Info(msg);
        }

        public virtual void Error(string msg)
        {
            ReloadConfig();
            log.Error(msg);
        }

        public virtual void Error(string msg, Exception ex)
        {
            ReloadConfig();
            log.Error(msg, ex);
        }

        public virtual void Warn(string msg)
        {
            ReloadConfig();
            log.Warn(msg);
        }

        public virtual void Error(Exception tie)
        {
            if (tie != null)
            {
                this.Error(tie.ToString());
                this.Error(tie.InnerException);
            }
        }
    }
}
