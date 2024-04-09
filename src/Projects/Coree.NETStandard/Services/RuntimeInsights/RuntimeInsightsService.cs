using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.RuntimeInsights
{
    public partial class RuntimeInsightsService : DependencySingleton<RuntimeInsightsService>, IRuntimeInsightsService , IDependencySingleton
    {
        /// <summary>
        /// Checks if the current build is a debug build.
        /// </summary>
        /// <returns>
        /// True if the current build is a debug build; otherwise, false.
        /// If the operation is canceled or an exception occurs, returns null.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? IsDebugBuild()
        {
           return IsDebugBuildAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the current build is a debug build.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>
        /// True if the current build is a debug build; otherwise, false.
        /// If the operation is canceled or an exception occurs, returns null.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool?> IsDebugBuildAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var assembly = Assembly.GetEntryAssembly();

                    if (assembly.GetCustomAttributes(typeof(DebuggableAttribute), false) is DebuggableAttribute[] attributes && attributes.Length > 0)
                    {
                        var d = attributes[0];
                        if (d.IsJITTrackingEnabled)
                        {
                            logger.LogTrace("IsDebugBuild {value}", true);
                            return true;
                        }
                    }
                    logger.LogTrace("IsDebugBuild {value}", false);
                    return false;
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("IsDebugBuild operation was canceled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to determine if debug build asynchronously.");
            }

            return null;
        }

        /// <summary>
        /// Checks if the current build is a debug build.
        /// </summary>
        /// <returns>
        /// True if the current build is a debug build; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDevelopmentBuild()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly.GetCustomAttributes(typeof(DebuggableAttribute), false) is DebuggableAttribute[] attributes && attributes.Length > 0)
            {
                var d = attributes[0];
                if (d.IsJITTrackingEnabled)
                {
                    return true;
                }
            }
            return false;
        }

    }
}