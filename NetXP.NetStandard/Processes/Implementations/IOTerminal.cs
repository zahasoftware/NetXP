using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NetXP.NetStandard.Processes.Implementations
{
    public class IOTerminal : IIOTerminal
    {
        private IOTerminalOptions ioTerminalOptions;

        public IOTerminal(IOptions<IOTerminalOptions> ioTerminalOptions)
        {
            this.ioTerminalOptions = ioTerminalOptions.Value;

            if (this.ioTerminalOptions.WaitTimeOut == 0)
            {
                this.ioTerminalOptions.WaitTimeOut = 7000;
            }
        }

        public ProcessOutput Execute(ProcessInput processInput)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = processInput.ShellName;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = processInput.Arguments;
            //TryToGetLastDirectory(ShellDTO);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;

            var output = new ProcessOutput();

            List<string> soutput = new List<string>();
            List<string> serror = new List<string>();

            using (var pro = Process.Start(psi))
            {
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    pro.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            soutput.Add(e.Data?.Trim());
                        }
                    };
                    pro.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            serror.Add(e.Data?.Trim());
                        }
                    };

                    using (var i = pro.StandardInput)
                    {
                        i.WriteLine(processInput.Command);///Executing command
                    }

                    pro.BeginOutputReadLine();
                    pro.BeginErrorReadLine();

                    if (pro.WaitForExit(this.ioTerminalOptions.WaitTimeOut))
                    {
                        output.StandardOutput = soutput.ToArray();
                        output.StandardError = serror.ToArray();
                    }
                    else
                    {
                        throw new TimeoutException("Shell wait exit");
                    }

                    output.ExitCode = pro.ExitCode;
                }
            }
            return output;
        }
    }
}
