using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.ProcessService
{
    /// <summary>
    /// Provides functionality for managing and executing external processes. This service supports a variety of operations including running processes with options for cancellation, timeouts, and potentially more features in the future.
    /// </summary>
    public partial class ProcessService : ServiceFactory<ProcessService>, IProcessService
    {
        /// <summary>
        /// Runs an external process asynchronously with options for cancellation and timeout.
        /// </summary>
        /// <param name="fileName">The name of the executable to run.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <param name="killOnCancel">Indicates whether the process should be killed if a cancellation is requested.</param>
        /// <param name="cancellationWaitRequest">The cancellation token that can be used to request cancellation of the operation.</param>
        /// <param name="timeout">Optional timeout after which the process will be killed if not completed.</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the process execution.</returns>
        public async Task<ProcessRunResult> RunProcessWithCancellationSupportAsync(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null)
        {
            ProcessRunResult returnValue = new ProcessRunResult() { ExitCodeState = ProcessRunExitCodeState.None, Commandline = $"{fileName} {arguments}", Filename = fileName, Arguments = arguments };
            StringBuilder outputBuilder = new StringBuilder();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationWaitRequest))
            {
                if (timeout.HasValue)
                {
                    linkedCts.CancelAfter(timeout.Value);
                }

                using (var process = new System.Diagnostics.Process())
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
                            _logger?.LogDebug("StdOut: {OutputData}", e.Data);
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
                            _logger?.LogDebug("StdErr: {ErrorData}", e.Data);
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
                        _logger?.LogDebug("Starting process with arguments: {FileName} {Arguments}.", fileName, arguments);
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to start process with arguments: {FileName} {Arguments}.", fileName, arguments);
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
                                _logger?.LogDebug("Process kill requested due to cancellation.");
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "Exception when trying to kill process.");
                            }
                        }
                        else if (!killOnCancel && !processComplition.Task.IsCompleted)
                        {
                            _logger?.LogTrace("Cancellation requested. The process will continue to run in a detached state as 'killOnCancel' is false. Output and errors may be incomplete.");
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
                            _logger?.LogTrace(ex, "Task canceled. Output and errors may be incomplete.");
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

                    _logger?.LogDebug("ExitCode state is {ExitCodeState}. Process exited with code {ExitCode}.", returnValue.ExitCodeState, returnValue.ExitCode);

                    process.Exited -= ProcessExited;
                }
            }

            return returnValue;
        }

    }
}
