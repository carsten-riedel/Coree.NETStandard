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

        Task<ProcessRunResult> RunProcessWithCancellationSupportAsync2(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationRequest = default, TimeSpan? timeout = null);

        Task<ProcessRunResult> RunProcessWithCancellationSupportAsyncMe(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null);
    }

    public class ProcessRunResult
    {
        public ProcessRunExitCodeState ExitCodeState { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; }
    }

    public enum ProcessRunExitCodeState : int
    {
        IsValidSuccess = 0,
        IsUnknown = -1,
        IsFailedStart = -2,
        IsValidErrorCode = -3,
        IsCanceledSet = -4,
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
                            logger.LogDebug("StdOut: {OutputData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                        else
                        {
                            logger.LogDebug("IMPORTANT OUTPUT: NULL");
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
                            logger.LogDebug("IMPORTANT ERROR: NULL");
                        }
                    };

                    var tcs = new TaskCompletionSource<bool>();

                    void ProcessExited(object sender, EventArgs e)
                    {
                        returnValue.ExitCode = process.ExitCode;
                        returnValue.Output = outputBuilder.ToString();
                        if (returnValue.ExitCode == 0)
                        {
                            returnValue.ExitCodeState = ProcessRunExitCodeState.IsValidSuccess;
                        }
                        else
                        {
                            returnValue.ExitCodeState = ProcessRunExitCodeState.IsValidErrorCode;
                        }
                        logger.LogDebug("Process exited with code {ExitCode}.", process.ExitCode);
                        tcs.TrySetResult(true);
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
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsFailedStart;
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
                                logger.LogDebug("Process kill requested due to cancellation.");
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
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsCanceledSet;
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

        public async Task<ProcessRunResult> RunProcessWithCancellationSupportAsyncMe(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null)
        {
            ProcessRunResult returnValue = new ProcessRunResult() { ExitCodeState = ProcessRunExitCodeState.IsUnknown };
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
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsFailedStart;
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
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsCanceledSet;
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


                    if (returnValue.ExitCodeState == ProcessRunExitCodeState.IsUnknown)
                    {
                        returnValue.ExitCode = process.ExitCode;
                        returnValue.Output = outputBuilder.ToString();

                        if (returnValue.ExitCode == 0)
                        {
                            returnValue.ExitCodeState = ProcessRunExitCodeState.IsValidSuccess;
                        }
                        else
                        {
                            returnValue.ExitCodeState = ProcessRunExitCodeState.IsValidErrorCode;
                        }
                    }
                    else if (returnValue.ExitCodeState == ProcessRunExitCodeState.IsCanceledSet)
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

        public async Task<ProcessRunResult> RunProcessWithCancellationSupportAsync2(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null)
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

                    var outputCompletion = new TaskCompletionSource<bool>();
                    var errorCompletion = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            logger.LogDebug("Output: {OutputData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                        else
                        {
                            outputCompletion.TrySetResult(true);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            logger.LogDebug("Error : {ErrorData}", e.Data);
                            outputBuilder.AppendLine(e.Data);
                        }
                        else
                        {
                            errorCompletion.TrySetResult(true);
                        }
                    };

                    var processCompletion = new TaskCompletionSource<bool>();

                    void ProcessExited(object sender, EventArgs e)
                    {
                        processCompletion.TrySetResult(true);
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
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsFailedStart;
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
                                logger.LogDebug("Process kill requested due to cancellation.");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Exception when trying to kill process.");
                            }
                        }
                    }))
                    {
                        await Task.WhenAny(processCompletion.Task, outputCompletion.Task, errorCompletion.Task).ConfigureAwait(false);
                    }

                    returnValue.ExitCode = process.ExitCode;
                    returnValue.Output = outputBuilder.ToString();
                    if (returnValue.ExitCode == 0)
                    {
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsValidSuccess;
                    }
                    else
                    {
                        returnValue.ExitCodeState = ProcessRunExitCodeState.IsValidErrorCode;
                    }
                    logger.LogDebug("Process exited with code {ExitCode}.", process.ExitCode);

                    process.Exited -= ProcessExited;
                }
            }

            return returnValue;
        }

        //public async Task<ProcessRunResult> RunProcessWithCancellationSupportAsync2(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationRequest = default, TimeSpan? timeout = null)
        //{
        //    ProcessRunResult returnValue = new ProcessRunResult();
        //    StringBuilder outputBuilder = new StringBuilder();
        //    StringBuilder errorBuilder = new StringBuilder();

        //    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationRequest))
        //    {
        //        if (timeout.HasValue)
        //        {
        //            linkedCts.CancelAfter(timeout.Value);
        //        }

        //        using (var process = new Process())
        //        {
        //            process.StartInfo.UseShellExecute = false;
        //            process.StartInfo.CreateNoWindow = true;
        //            process.StartInfo.RedirectStandardOutput = true;
        //            process.StartInfo.RedirectStandardError = true;
        //            process.StartInfo.FileName = fileName;
        //            process.StartInfo.Arguments = arguments;
        //            process.StartInfo.WorkingDirectory = workingDirectory;

        //            var tcs = new TaskCompletionSource<bool>();

        //            process.EnableRaisingEvents = true;
        //            process.Exited += (sender, args) =>
        //            {
        //                tcs.TrySetResult(true);
        //            };

        //            try
        //            {
        //                logger.LogDebug("Starting process with arguments: {FileName} {Arguments}.", fileName, arguments);
        //                process.Start();
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.LogError(ex, "Failed to start process with arguments: {FileName} {Arguments}.", fileName, arguments);
        //                returnValue.ProcessRunErrorCode = ProcessRunErrorCode.ProcessStartFailed;
        //                returnValue.Output = "Failed to start process.";
        //                return returnValue;
        //            }

        //            var readOutputTask = ReadStreamAsync(process.StandardOutput, outputBuilder, linkedCts.Token);
        //            var readErrorTask = ReadStreamAsync(process.StandardError, errorBuilder, linkedCts.Token);
        //            var waitForExitTask = tcs.Task;

        //            using (linkedCts.Token.Register(() =>
        //            {
        //                if (killOnCancel && !process.HasExited)
        //                {
        //                    try
        //                    {
        //                        process.Kill();
        //                        logger.LogDebug("Process kill requested due to cancellation.");
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        logger.LogError(ex, "Exception when trying to kill process.");
        //                    }
        //                }
        //            }))
        //            {
        //                try
        //                {
        //                    await Task.WhenAll(readOutputTask, readErrorTask, waitForExitTask);
        //                }
        //                catch (OperationCanceledException ex)
        //                {
        //                    logger.LogTrace(ex, "Operation was canceled. Output and errors may be incomplete.");
        //                    // Ensure the process is killed if it's still running after cancellation
        //                    if (!process.HasExited)
        //                    {
        //                        process.Kill();
        //                    }
        //                }
        //            }

        //            returnValue.ExitCode = process.ExitCode;
        //            returnValue.Output = outputBuilder.ToString() + errorBuilder.ToString();
        //            returnValue.ProcessRunErrorCode = returnValue.ExitCode == 0 ? ProcessRunErrorCode.Success : ProcessRunErrorCode.ProcessErrorCode;
        //            logger.LogDebug("Process exited with code {ExitCode}.", process.ExitCode);
        //        }
        //    }

        //    return returnValue;
        //}

        //private async Task ReadStreamAsync(StreamReader stream, StringBuilder builder, CancellationToken cancellationToken)
        //{
        //    string line;
        //    while ((line = await stream.ReadLineAsync().ConfigureAwait(false)) != null)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            cancellationToken.ThrowIfCancellationRequested();
        //        }

        //        builder.AppendLine(line);
        //    }
        //}
    }
}