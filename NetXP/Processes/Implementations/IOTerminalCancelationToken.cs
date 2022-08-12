using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NetXP.Processes.Implementations
{
    public class IOTerminalCancelationToken : IIOTerminal
    {
        private IOTerminalOptions ioTerminalOptions;

        public IOTerminalCancelationToken(IOptions<IOTerminalOptions> ioTerminalOptions)
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
                //pro.StartInfo.WorkingDirectory


                pro.Start();

                ProcessOutputTemp pot = new ProcessOutputTemp();
                pot.StandardOutput = new List<string>();
                pot.StandardError = new List<string>();

                using (var i = pro.StandardInput)
                {
                    i.WriteLine(processInput.Command);///Executing command
                }

                var cancellationTokenSource = ioTerminalOptions.WaitTimeOut != 0
                    ? new CancellationTokenSource(ioTerminalOptions.WaitTimeOut)
                    : new CancellationTokenSource();

                var task = WaitForExitAsync(pro, cancellationTokenSource.Token);

                var standardOutput = ReadAsync(
                    x =>
                    {
                        pro.OutputDataReceived += x;
                        pro.BeginOutputReadLine();
                    },
                    x => pro.OutputDataReceived -= x, pot, true,
                    cancellationTokenSource.Token);

                var standardError = ReadAsync(
                    x =>
                    {
                        pro.ErrorDataReceived += x;
                        pro.BeginErrorReadLine();
                    },
                    x => pro.ErrorDataReceived -= x, pot, false,
                    cancellationTokenSource.Token);

                Task.WhenAll(task, standardOutput, standardError).Wait();

                var output = new ProcessOutput();
                output.StandardOutput = pot.StandardOutput?.ToArray();
                output.StandardError = pot.StandardError?.ToArray();

                output.ExitCode = pro.ExitCode;

                return output;
            }
        }


        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return
        /// immediately as cancelled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public Task WaitForExitAsync(
            Process process,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            process.EnableRaisingEvents = true;

            var taskCompletionSource = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = (sender, args) =>
            {
                process.Exited -= handler;
                taskCompletionSource.TrySetResult(null);
            };
            process.Exited += handler;

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(
                    () =>
                    {
                        process.Exited -= handler;
                        taskCompletionSource.TrySetCanceled();
                    });
            }

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Reads the data from the specified data recieved event and writes it to the
        /// <paramref name="textWriter"/>.
        /// </summary>
        /// <param name="addHandler">Adds the event handler.</param>
        /// <param name="removeHandler">Removes the event handler.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task ReadAsync(
            Action<DataReceivedEventHandler> addHandler,
            Action<DataReceivedEventHandler> removeHandler,
            ProcessOutputTemp output,
            bool standardInput,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            DataReceivedEventHandler handler = null;
            handler = new DataReceivedEventHandler(
                (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        removeHandler(handler);
                        taskCompletionSource.TrySetResult(null);
                    }
                    else
                    {
                        if (standardInput)
                        {
                            output.StandardOutput.Add(e.Data);
                        }
                        else
                        {
                            output.StandardError.Add(e.Data);
                        }
                    }
                });

            addHandler(handler);

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(
                    () =>
                    {
                        removeHandler(handler);
                        taskCompletionSource.TrySetCanceled();
                    });
            }

            return taskCompletionSource.Task;
        }

        private class ProcessOutputTemp
        {
            public ProcessOutputTemp()
            {
            }

            public List<string> StandardOutput { get; internal set; }
            public List<string> StandardError { get; internal set; }
        }
    }
}
