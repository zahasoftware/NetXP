
namespace NetXP.NetStandard.SystemInformation
{
    public class ServiceInformation
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public ServiceState State { get; set; }
        public ServiceStartupState StartupState { get; set; }
    }
}