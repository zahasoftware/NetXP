
namespace NetXP.NetStandard.SystemInformation
{
    public class ServiceInformation
    {
        public string ServiceName { get; internal set; }
        public string Description { get; internal set; }
        public ServiceState State { get; internal set; }
        public ServiceStartupState StartupState { get; internal set; }
    }
}