namespace NetXP.NetStandard.SystemManagers
{
    public class RestartConstants
    {
        public const string Always = "always";
        public const string OnFailure = "on-failure";

        ///This will only restart the Perforce server in the event of a clean-shutdown. To avoid issues with the server restarting automatically, 
        ///you may want to disable the Perforce service prior to stopping the service, or use the RestartSec directive:
        ///RestartSec=300
        public const string OnSuccess = "on-success";

        ///This will restart the service on unexpected shut-down, but only if the exit code was "clean". This is typically the safest approach, 
        ///as the cause of the shutdown was likely external to Perforce.Do not be tempted to use "always" or "on-failure" to avoid causing more
        ///issues with your Perforce server.
        public const string OnAbnormal = "on-abnormal";

        ///Will only restart the server in the event of an unclean signal, not if the server was timed out or shutdown by a Watchdog daemon.
        public const string OnAbort = "on-abort";


        public const string OnWatchdog = "on-watchdog";
    }
}