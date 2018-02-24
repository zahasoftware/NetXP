using NetXP.NetStandard.SystemInformation;

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
    }
}