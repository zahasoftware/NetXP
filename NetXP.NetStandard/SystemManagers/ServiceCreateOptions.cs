﻿using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.SystemManagers
{
    public class ServiceCreateOptions
    {
        public ServiceCreateOptions()
        {
            ServiceStartupState = ServiceStartupState.Active;
        }
        public string DisplayName { get; set; }
        public SystemInformation.ServiceStartupState ServiceStartupState { get; set; }
        public string Description { get; set; }
        public string After { get; set; }
        public string WantedBy { get; set; }
        public string WorkingDirectory { get; set; }
        public string Restart { get; set; } = RestartConstants.Always;
        public int RestartSeconds { get; set; } = 5;

    }
}