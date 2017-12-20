using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Processes.Implementations
{
    public class IOTerminal : IIOTerminal
    {
        public ProcessOutput Execute(ProcessInput processInput)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = processInput.ShellName;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = processInput.Arguments;
            //TryToGetLastDirectory(dtoshell);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;

            var output = new ProcessOutput();

            using (var pro = Process.Start(psi))
            {
                using (var i = pro.StandardInput)
                {
                    i.WriteLine(processInput.Command);///Changing to current directory before execute any command.
                }

                using (var e = pro.StandardError)
                using (var r = pro.StandardOutput)
                {
                    Func<Task<string>> funcStandardOutput = async () =>
                    {
                        var task = r.ReadToEndAsync();
                        if (await Task.WhenAny(task, Task.Delay(5000)) != task) throw new TimeoutException("Standard Output Timeout");
                        return task.Result;
                    };
                    var standarOutput = funcStandardOutput();

                    Func<Task<string>> funcErrorOutput = async () =>
                    {
                        var task = e.ReadToEndAsync();
                        if (await Task.WhenAny(task, Task.Delay(5000)) != task) throw new TimeoutException("Standard Error Timeout");
                        return task.Result;
                    };
                    var errorOutput = funcErrorOutput();

                    if (!pro.WaitForExit(5000)) throw new TimeoutException("Shell wait exit");
                    else
                    {
                        output.StandardOutput = Regex.Split(standarOutput.Result, "\r\n");
                        output.StandardError = Regex.Split(errorOutput.Result, "\r\n");

                        for (int i = 0; i < (output.StandardOutput?.Length ?? 0); i++)
                        {
                            output.StandardOutput[i] = output.StandardOutput[i]?.Trim();
                        }

                        for (int i = 0; i < (output.StandardError?.Length ?? 0); i++)
                        {
                            output.StandardError[i] = output.StandardError[i]?.Trim();
                        }
                    }
                }
            }
            return output;
        }
    }
}
