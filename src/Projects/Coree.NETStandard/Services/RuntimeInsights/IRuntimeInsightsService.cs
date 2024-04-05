using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Coree.NETStandard.Services.RuntimeInsights
{
    /// <summary>
    /// Represents a service for retrieving runtime insights.
    /// </summary>
    public interface IRuntimeInsightsService
    {
        /// <summary>
        /// Checks if the current build is a debug build.
        /// </summary>
        /// <returns>
        /// True if the current build is a debug build; otherwise, false.
        /// Returns null if the determination cannot be made.
        /// </returns>
        bool? IsDebugBuild();

        /// <summary>
        /// Checks if the current build is a debug build.
        /// </summary>
        /// /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains true if the current build is a debug build; otherwise, false.
        /// If the operation is canceled or an exception occurs, returns null.
        /// </returns>
        Task<bool?> IsDebugBuildAsync(CancellationToken cancellationToken = default);
    }
}
