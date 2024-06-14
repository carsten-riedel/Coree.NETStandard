using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.ProcessManagement
{
    /// <summary>
    /// Provides functionality for managing and executing external processes. This service supports a variety of operations including running processes with options for cancellation, timeouts, and potentially more features in the future.
    /// </summary>
    public partial class ProcessService : ServiceFactoryEx<ProcessService>, IProcessService
    {
        private readonly ILogger<ProcessService>? _logger;

        /// <summary>
        /// Initializes a new instance of the ProcessService class with the specified logger and configuration.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        public ProcessService(ILogger<ProcessService> logger)
        {
            _logger = logger;
        }
    }

    /// <summary>
    /// Defines a service for running external processes with support for cancellation and timeouts.
    /// </summary>
    public partial interface IProcessService
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
        Task<ProcessRunResult> RunProcessWithCancellationSupportAsync(string fileName, string arguments, string workingDirectory, bool killOnCancel = false, CancellationToken cancellationWaitRequest = default, TimeSpan? timeout = null);
    }

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
}