namespace NetXP.NetStandard.Processes
{
    public class ProcessInput
    {
        /// <summary>
        /// The arguments will be passed to terminal
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Could be: window:[cmd, powershell]
        /// </summary>
        public string ShellName { get; set; }

        /// <summary>
        /// Standard input for process.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int MaxOfSecondToWaitCommand { get; set; }
    }
}