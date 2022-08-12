
namespace NetXP.SystemInformation
{

    ///<summary>
    ///State of service, It's used with ServiceInformer and ServiceInformation classes
    ///</summary>
    public enum ServiceState
    {
        Running,///Service is active and running.
        Stopped,///Service is active but It's stoped.
        Paused,///Service is active but It's paused.
        Unknown,///Service is in an unrecognized state.
        Failed,///Service With Error.
    }
}