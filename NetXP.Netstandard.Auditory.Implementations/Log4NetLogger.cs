using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace NetXP.NetStandard.Auditory.Implementations
{
    public class Log4NetLogger : ILogger
    {
        public Log4NetLogger()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));

            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }
        protected static ILog log = LogManager.GetLogger(typeof(ILogger));

        public virtual void Debug(string msg,
                                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            ReloadConfig();
            string @class = Path.GetFileNameWithoutExtension(sourceFilePath);
            log.Debug($"{msg} (<{sourceLineNumber}:{@class}.{memberName}>)");
            var basedir = Directory.GetCurrentDirectory();
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
                Error(tie.ToString());
                Error(tie.InnerException);
            }
        }
    }
}
