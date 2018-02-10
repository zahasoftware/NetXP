namespace NetXP.NetStandard.SystemInformation
{
    ///Determine in which state of startup is the service
    public enum ServiceStartupState
    {
        ///Indicates that the service is to be started (or was started) by the operating system
        Active,
        ///Indicates that the service is disabled, so that it cannot be start on system startup
        Disabled,
    }
}