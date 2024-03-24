using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coree.NETStandard.Services
{
    public partial interface IProcessService
    {
        Task<ProcessRunResult> RunProcessWithCancellationSupportAsync(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null);
    }

    public class ProcessRunResult
    {
        public ProcessRunExitCodeState ExitCodeState { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; }
    }

    [Flags]
    public enum ProcessRunExitCodeState : int
    {
        None = 0,
        IsValidSuccess = 1,
        IsFailedStart = 2,
        IsValidErrorCode = 4,
        IsCanceledSet = 8,
        IsValid = 16,
    }

    public partial class ProcessService : IProcessService
    {
        private readonly ILogger<ProcessService> logger;

        public ProcessService(ILogger<ProcessService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProcessRunResult> RunProcessWithCancellationSupportAsync(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null)
        {
            ProcessRunResult returnValue = new ProcessRunResult() { ExitCodeState = ProcessRunExitCodeState.None };
            StringBuilder outputBuilder = new StringBuilder();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationWaitRequest))
            {
                if (timeout.HasValue)
                {
                    linkedCts.CancelAfter(timeout.Value);
                }

                using (var process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.EnableRaisingEvents = true;

                    var outputComplition = new TaskCompletionSource<bool>();
                    var errorComplition = new TaskCompletionSource<bool>();
                    var processComplition = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            logger.LogDebug("StdOut: {OutputData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                        else
                        {
                            outputComplition.TrySetResult(true);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            logger.LogDebug("StdErr: {ErrorData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                        else
                        {
                            errorComplition.TrySetResult(true);
                        }
                    };

                    void ProcessExited(object sender, EventArgs e)
                    {
                        processComplition.TrySetResult(true);
                    }

                    process.Exited += ProcessExited;

                    try
                    {
                        logger.LogDebug("Starting process with arguments: {FileName} {Arguments}.", fileName, arguments);
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to start process with arguments: {FileName} {Arguments}.", fileName, arguments);
                        returnValue.ExitCodeState |= ProcessRunExitCodeState.IsFailedStart;
                        returnValue.ExitCode = -1;
                        returnValue.Output = "Failed to start process.";
                        return returnValue; // Early exit with failure indication
                    }

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    using (linkedCts.Token.Register(() =>
                    {
                        if (killOnCancel && !process.HasExited)
                        {
                            try
                            {
                                process.Kill();
                                logger.LogDebug("Process kill requested due to cancellation.");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Exception when trying to kill process.");
                            }
                        }
                        else if (!killOnCancel && !processComplition.Task.IsCompleted)
                        {
                            logger.LogTrace("Cancellation requested. The process will continue to run in a detached state as 'killOnCancel' is false. Output and errors may be incomplete.");
                        }
                        returnValue.ExitCodeState |= ProcessRunExitCodeState.IsCanceledSet;
                        outputComplition.TrySetCanceled();
                        errorComplition.TrySetCanceled();
                        processComplition.TrySetCanceled();
                    }))
                    {
                        try
                        {
                            await Task.WhenAll(processComplition.Task, outputComplition.Task, errorComplition.Task).ConfigureAwait(false);
                        }
                        catch (TaskCanceledException ex)
                        {
                            logger.LogTrace(ex, "Task canceled. Output and errors may be incomplete.");
                            returnValue.Output = outputBuilder.ToString();
                        }
                    }

                    if (returnValue.ExitCodeState == ProcessRunExitCodeState.None)
                    {
                        returnValue.ExitCode = process.ExitCode;
                        returnValue.Output = outputBuilder.ToString();

                        returnValue.ExitCodeState |= ProcessRunExitCodeState.IsValid;

                        if (returnValue.ExitCode == 0)
                        {
                            returnValue.ExitCodeState |= ProcessRunExitCodeState.IsValidSuccess;
                        }
                        else
                        {
                            returnValue.ExitCodeState |= ProcessRunExitCodeState.IsValidErrorCode;
                        }
                    }
                    else if (returnValue.ExitCodeState.HasFlag(ProcessRunExitCodeState.IsCanceledSet))
                    {
                        returnValue.ExitCode = -1;
                        returnValue.Output = outputBuilder.ToString();
                    }

                    logger.LogDebug("ExitCode state is {ExitCodeState}. Process exited with code {ExitCode}.", returnValue.ExitCodeState, returnValue.ExitCode);

                    process.Exited -= ProcessExited;
                }
            }

            return returnValue;
        }
    }
}