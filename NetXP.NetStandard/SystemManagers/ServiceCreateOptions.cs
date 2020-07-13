using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.SystemManagers
{
    public class ServiceCreateOptions
    {

        public string DisplayName { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Working directory just work for linux
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// If need restart after fail
        /// </summary>
        public bool Restart { get; set; }

        /// <summary>
        /// Timeout before restarting on failure
        /// </summary>
        public int RestartSeconds { get; set; } = 5;

    }
}