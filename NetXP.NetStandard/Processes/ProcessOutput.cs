using System.Collections.Generic;

namespace NetXP.NetStandard.Processes
{
    public class ProcessOutput
    {
        public string[] StandardOutput { get; set; }

        public string[] StandardError { get; set; }
        public int ExitCode { get; internal set; }
    }
}