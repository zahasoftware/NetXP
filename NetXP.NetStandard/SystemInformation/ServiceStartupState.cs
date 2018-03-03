namespace NetXP.NetStandard.SystemInformation
{
    ///Determine in which state of startup is the service
    public enum ServiceStartupState
    {

        ///Service will be started (or was started) by the operating system
        Active,
        ///Service is disabled, so it cannot be start on system startup
        Disabled,
        ///Service cannot start because exits an error.
        Failed
    }
}