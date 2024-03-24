using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
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
        public ProcessRunErrorCode ProcessRunErrorCode { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; }
    }

    public enum ProcessRunErrorCode : int
    {
        Success = 0,
        ProcessStartFailed = -1,
        TaskCancelled = -2,
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
            ProcessRunResult returnValue = new ProcessRunResult();
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

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            logger.LogInformation("Output: {OutputData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            logger.LogError("Error : {ErrorData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                    };

                    var tcs = new TaskCompletionSource<bool>();

                    void ProcessExited(object sender, EventArgs e)
                    {
                        returnValue.ExitCode = process.ExitCode;
                        returnValue.Output = outputBuilder.ToString();
                        returnValue.ProcessRunErrorCode = ProcessRunErrorCode.Success;
                        logger.LogInformation("Process exited with code {ExitCode}.", process.ExitCode);
                        tcs.TrySetResult(true);
                    }

                    process.Exited += ProcessExited;

                    try
                    {
                        logger.LogDebug("Starting process {FileName} with arguments {Arguments}.", fileName, arguments);
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to start process {FileName}.", fileName);
                        returnValue.ProcessRunErrorCode = ProcessRunErrorCode.ProcessStartFailed;
                        returnValue.Output = "Failed to start process.";
                        return returnValue; // Early exit with failure indication
                    }

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.EnableRaisingEvents = true;

                    using (linkedCts.Token.Register(() =>
                    {
                        if (killOnCancel && !process.HasExited)
                        {
                            try
                            {
                                process.Kill();
                                logger.LogWarning("Process kill requested due to cancellation.");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Exception when trying to kill process.");
                            }
                        }
                        else if (!killOnCancel && !tcs.Task.IsCompleted)
                        {
                            logger.LogTrace("Cancellation requested. The process will continue to run in a detached state as 'killOnCancel' is false. Output and errors may be incomplete.");
                        }
                        returnValue.ProcessRunErrorCode = ProcessRunErrorCode.TaskCancelled;
                        tcs.TrySetCanceled();
                    }))
                    {
                        try
                        {
                            await tcs.Task.ConfigureAwait(false);
                        }
                        catch (TaskCanceledException ex)
                        {
                            logger.LogTrace(ex, "Task canceled. Output and errors may be incomplete.");
                            returnValue.Output = outputBuilder.ToString();
                        }
                    }

                    process.Exited -= ProcessExited;
                }
            }

            return returnValue;
        }

    }
}