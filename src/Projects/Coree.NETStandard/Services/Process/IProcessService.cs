using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Coree.NETStandard.Services.Process
{
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

}
