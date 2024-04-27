using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.Process
{
    /// <summary>
    /// Represents the result of running an external process.
    /// </summary>
    public class ProcessRunResult
    {
        /// <summary>
        /// Gets or sets the state of the exit code indicating the outcome of the process run.
        /// </summary>
        public ProcessRunExitCodeState ExitCodeState { get; set; }

        /// <summary>
        /// Gets or sets the exit code returned by the process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the output (both stdout and stderr) of the process.
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// Gets or sets the command line used to start the process.
        /// </summary>
        public string? Commandline { get; set; }

        /// <summary>
        /// Gets or sets the file name of the executable that was run.
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// Gets or sets the arguments passed to the executable.
        /// </summary>
        public string? Arguments { get; set; }
    }

    /// <summary>
    /// Represents various outcome states for process execution operations within the service. This enum is designed to be flexible and applicable to a range of process-related methods, providing a standardized way to communicate success, failure, and other states across different types of process executions.
    /// </summary>
    [Flags]
    public enum ProcessRunExitCodeState : int
    {
        /// <summary>
        /// The default state, indicating no specific outcome has been determined. This state is used before an operation is performed or when an operation's outcome does not fit any other defined state.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates a successful execution where the process completed as expected without any errors, typically represented by an exit code of 0.
        /// </summary>
        IsValidSuccess = 1,

        /// <summary>
        /// Indicates the process failed to start, which could result from various issues such as executable not found, insufficient permissions, or invalid arguments.
        /// </summary>
        IsFailedStart = 2,

        /// <summary>
        /// Indicates the process ran but completed with an error, typically represented by a non-zero exit code. This state signifies an unsuccessful execution according to the process's own criteria.
        /// </summary>
        IsValidErrorCode = 4,

        /// <summary>
        /// Indicates the operation was canceled, either through user action or a timeout. This state reflects an intentional interruption of the process before completion.
        /// </summary>
        IsCanceledSet = 8,

        /// <summary>
        /// Indicates a valid exit code has been set, suggesting the process has concluded and an outcome is available for evaluation. This state is intended to signify the end of an operation, whether successful or not.
        /// </summary>
        IsValid = 16,
    }

    /// <summary>
    /// Provides functionality for managing and executing external processes. This service supports a variety of operations including running processes with options for cancellation, timeouts, and potentially more features in the future.
    /// </summary>
    public partial class ProcessService : IProcessService
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