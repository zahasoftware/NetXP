
namespace NetXP.NetStandard.SystemInformation
{

    ///<summary>
    ///State of service, It's used with ServiceInformer and ServiceInformation classes
    ///</summary>
    public enum ServiceState
    {
        Running,///Service is active and running.
        Stopped, ///Service is active but It's stoped.
        Disabled,///Service is disabled, and It cannot start in boot startup.
        Unknown,///Service is in an unrecognized state.
    }
}