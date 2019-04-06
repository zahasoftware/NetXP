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
            //var psi = new ProcessStartInfo();
            using (var pro = new Process())
            {
                pro.StartInfo.FileName = processInput.ShellName;
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.Arguments = processInput.Arguments;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardError = true;

                var output = new ProcessOutput();

                List<string> soutput = new List<string>();
                List<string> serror = new List<string>();

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

                    try
                    {
                        pro.Start();

                        using (var i = pro.StandardInput)
                        {
                            i.WriteLine(processInput.Command);///Executing command
                        }

                        pro.BeginOutputReadLine();
                        pro.BeginErrorReadLine();

                        if (pro.WaitForExit(ioTerminalOptions.WaitTimeOut))
                        {
                            output.StandardOutput = soutput.ToArray();
                            output.StandardError = serror.ToArray();
                        }
                        else
                        {
                            pro.Kill();
                            throw new TimeoutException("Shell wait exit");
                        }

                        output.ExitCode = pro.ExitCode;
                    }
                    finally
                    {
                        outputWaitHandle.WaitOne(ioTerminalOptions.WaitTimeOut);
                        errorWaitHandle.WaitOne(ioTerminalOptions.WaitTimeOut);
                    }
                }
                return output;
            }
        }

    }
}
