using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Runtime.InteropServices;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.RuntimeInsightsManagement
{
    /// <summary>
    /// Manages and provides access to runtime and system insights.
    /// </summary>
    public partial class RuntimeInsightsService : ServiceFactoryEx<RuntimeInsightsService>, IRuntimeInsightsService
    {

        private readonly ILogger<RuntimeInsightsService>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeInsightsService"/> class.
        /// </summary>
        /// <param name="logger">Optional logger instance for logging purposes.</param>
        /// <remarks>
        /// The logger provided here can be used with the field within the class.
        /// Be mindful that the logger may be null in scenarios where it's not explicitly provided.
        /// </remarks>
        public RuntimeInsightsService(ILogger<RuntimeInsightsService>? logger = null)
        {
            this._logger = logger;
            Initalize();
        }

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
                            _logger?.LogTrace("IsDebugBuild {value}", true);
                            return true;
                        }
                    }
                    _logger?.LogTrace("IsDebugBuild {value}", false);
                    return false;
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("IsDebugBuild operation was canceled.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to determine if debug build asynchronously.");
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

        /// <summary>
        /// Gets the name of the entry assembly.
        /// </summary>
        public string? EntryAssemblyName { get; private set; }

        /// <summary>
        /// Gets the name of the executing assembly.
        /// </summary>
        public string? ExecutingAssemblyName { get; private set; }

        /// <summary>
        /// Gets the name of the calling assembly.
        /// </summary>
        public string? CallingAssemblyName { get; private set; }

        /// <summary>
        /// Gets the main module name of the current process.
        /// </summary>
        public string? CurrentProcessMainModuleName { get; private set; }

        /// <summary>
        /// Gets the main module filename of the current process.
        /// </summary>
        public string? CurrentProcessMainModuleFilename { get; private set; }

        /// <summary>
        /// Gets the OS architecture.
        /// </summary>
        public string? OSArchitecture { get; private set; }

        /// <summary>
        /// Gets the process architecture.
        /// </summary>
        public string? ProcessArchitecture { get; private set; }

        /// <summary>
        /// Gets the command line used to start the environment.
        /// </summary>
        public string? EnvironmentCommandLine { get; private set; }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        public string? CurrentDirectory { get; private set; }

        /// <summary>
        /// Gets the username of the process owner.
        /// </summary>
        public string? Username { get; private set; }

        /// <summary>
        /// Initializes service properties with assembly and environment data.
        /// </summary>
        private void Initalize()
        {
            EntryAssemblyName = GetEntryAssemblyName();
            ExecutingAssemblyName = GetExecutingAssemblyName();
            CallingAssemblyName = GetCallingAssemblyName();
            CurrentProcessMainModuleFilename = GetCurrentProcessMainModuleFilename();
            CurrentProcessMainModuleName = GetCurrentProcessMainModuleName();
            OSArchitecture = GetOSArchitecture();
            ProcessArchitecture = GetProcessArchitecture();
            EnvironmentCommandLine = GetEnvironmentCommandLine();
            CurrentDirectory = GetCurrentDirectory();
            Username = GetUserName();
        }

        /// <summary>
        /// Retrieves the name of the entry assembly.
        /// </summary>
        private string GetEntryAssemblyName()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                _logger?.LogTrace("Assembly.GetEntryAssembly() returned null.");
                return "Unknown";
            }

            var assemblyName = entryAssembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                _logger?.LogTrace("Assembly.GetEntryAssembly()?.GetName().Name is null or empty.");
                return "Unknown";
            }

            // Using structured logging format
            _logger?.LogDebug("The entry assembly name is {AssemblyName}.", assemblyName);
            return assemblyName;
        }

        /// <summary>
        /// Retrieves the name of the executing assembly.
        /// </summary>
        private string GetExecutingAssemblyName()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            if (executingAssembly == null)
            {
                _logger?.LogTrace("Assembly.GetExecutingAssembly() returned null.");
                return "Unknown";
            }

            var assemblyName = executingAssembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                _logger?.LogTrace("Assembly.GetExecutingAssembly()?.GetName().Name is null or empty.");
                return "Unknown";
            }

            // Using structured logging format
            _logger?.LogDebug("The executing assembly name is {AssemblyName}.", assemblyName);
            return assemblyName;
        }

        /// <summary>
        /// Retrieves the name of the calling assembly.
        /// </summary>
        private string GetCallingAssemblyName()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            if (callingAssembly == null)
            {
                _logger?.LogTrace("Assembly.GetCallingAssembly() returned null.");
                return "Unknown";
            }

            var assemblyName = callingAssembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                _logger?.LogTrace("Assembly.GetCallingAssembly()?.GetName().Name is null or empty.");
                return "Unknown";
            }

            // Using structured logging format
            _logger?.LogDebug("The calling assembly name is {AssemblyName}.", assemblyName);
            return assemblyName;
        }

        /// <summary>
        /// Retrieves the main module name of the current process.
        /// </summary>
        private string GetCurrentProcessMainModuleName()
        {
            try
            {
                var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                if (mainModule == null)
                {
                    _logger?.LogTrace("Process.GetCurrentProcess().MainModule returned null.");
                    return "Unknown";
                }

                var moduleName = mainModule.ModuleName;
                if (string.IsNullOrEmpty(moduleName))
                {
                    _logger?.LogTrace("Process.GetCurrentProcess().MainModule.ModuleName is null or empty.");
                    return "Unknown";
                }

                // Using structured logging format
                _logger?.LogDebug("The current process main module name is {ModuleName}.", moduleName);
                return moduleName;
            }
            catch (Exception ex)
            {
                // Structured logging for exception
                _logger?.LogError(ex, "Failed to get current process main module name due to an exception.");
                return "Permission Denied";
            }
        }

        /// <summary>
        /// Retrieves the main module filename of the current process.
        /// </summary>
        private string GetCurrentProcessMainModuleFilename()
        {
            try
            {
                var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                if (mainModule == null)
                {
                    _logger?.LogTrace("Process.GetCurrentProcess().MainModule returned null.");
                    return "Unknown";
                }

                var moduleFullPath = mainModule.FileName;
                if (string.IsNullOrEmpty(moduleFullPath))
                {
                    _logger?.LogTrace("Process.GetCurrentProcess().MainModule.FileName is null or empty.");
                    return "Unknown";
                }

                // Using structured logging format
                _logger?.LogDebug("The current process main module full path is {ModuleFullPath}.", moduleFullPath);
                return moduleFullPath;
            }
            catch (Exception ex)
            {
                // Structured logging for exception
                _logger?.LogError(ex, "Failed to get current process main module full path due to an exception.");
                return "Permission Denied";
            }
        }

        /// <summary>
        /// Retrieves the operating system architecture.
        /// </summary>
        private string GetOSArchitecture()
        {
            return RuntimeInformation.OSArchitecture.ToString();
        }

        /// <summary>
        /// Retrieves the architecture of the current process.
        /// </summary>
        private string GetProcessArchitecture()
        {
            return RuntimeInformation.ProcessArchitecture.ToString();
        }

        /// <summary>
        /// Retrieves the command line used to start the current environment.
        /// </summary>
        private string GetEnvironmentCommandLine()
        {
            return Environment.CommandLine;
        }

        /// <summary>
        /// Retrieves the current working directory.
        /// </summary>
        private string GetCurrentDirectory()
        {
            try
            {
                return System.IO.Directory.GetCurrentDirectory();
            }
            catch (Exception ex)
            {
                // Structured logging for exception
                _logger?.LogError(ex, "Failed to get current directory due to an exception.");
                return "Exception";
            }
        }

        /// <summary>
        /// Retrieves the username of the process owner.
        /// </summary>
        private string GetUserName()
        {
            return Environment.UserName;
        }
    }

    /// <summary>
    /// Defines a contract for services that provide runtime and system insights.
    /// </summary>
    public interface IRuntimeInsightsService
    {
        /// <summary>
        /// Gets the name of the entry assembly.
        /// </summary>
        string? EntryAssemblyName { get; }

        /// <summary>
        /// Gets the name of the executing assembly.
        /// </summary>
        string? ExecutingAssemblyName { get; }

        /// <summary>
        /// Gets the name of the calling assembly.
        /// </summary>
        string? CallingAssemblyName { get; }

        /// <summary>
        /// Gets the main module name of the current process.
        /// </summary>
        string? CurrentProcessMainModuleName { get; }

        /// <summary>
        /// Gets the main module filename of the current process.
        /// </summary>
        string? CurrentProcessMainModuleFilename { get; }

        /// <summary>
        /// Gets the OS architecture.
        /// </summary>
        string? OSArchitecture { get; }

        /// <summary>
        /// Gets the process architecture.
        /// </summary>
        string? ProcessArchitecture { get; }

        /// <summary>
        /// Gets the command line used to start the environment.
        /// </summary>
        string? EnvironmentCommandLine { get; }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        string? CurrentDirectory { get; }

        /// <summary>
        /// Gets the username of the process owner.
        /// </summary>
        string? Username { get; }

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
