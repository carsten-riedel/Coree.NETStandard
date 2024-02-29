using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Coree.Hosting.Generic.Services
{
    public interface IRuntimeInsights
    {
        string? EntryAssemblyName { get; }
        string? ExecutingAssemblyName { get; }
        string? CallingAssemblyName { get; }
        string? CurrentProcessMainModuleName { get; }
        string? CurrentProcessMainModuleFilename { get; }
        string? OSArchitecture { get; }
        string? ProcessArchitecture { get; }
        string? EnvironmentCommandLine { get; }
        string? CurrentDirectory { get; }
    }

    public class RuntimeInsightsService : IRuntimeInsights
    {
        private readonly ILogger<IRuntimeInsights> logger;

        public string? EntryAssemblyName { get; private set; }
        public string? ExecutingAssemblyName { get; private set; }
        public string? CallingAssemblyName { get; private set; }
        public string? CurrentProcessMainModuleName { get; private set; }
        public string? CurrentProcessMainModuleFilename { get; private set; }
        public string? OSArchitecture { get; private set; }
        public string? ProcessArchitecture { get; private set; }
        public string? EnvironmentCommandLine { get; private set; }
        public string? CurrentDirectory { get; private set; }

        public RuntimeInsightsService(ILogger<IRuntimeInsights> logger)
        {
            this.logger = logger;
            Initalize();
        }

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
        }

        private string GetEntryAssemblyName()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                logger.LogTrace("Assembly.GetEntryAssembly() returned null.");
                return "Unknown";
            }

            var assemblyName = entryAssembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                logger.LogTrace("Assembly.GetEntryAssembly()?.GetName().Name is null or empty.");
                return "Unknown";
            }

            // Using structured logging format
            logger.LogDebug("The entry assembly name is {AssemblyName}.", assemblyName);
            return assemblyName;
        }

        private string GetExecutingAssemblyName()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            if (executingAssembly == null)
            {
                logger.LogTrace("Assembly.GetExecutingAssembly() returned null.");
                return "Unknown";
            }

            var assemblyName = executingAssembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                logger.LogTrace("Assembly.GetExecutingAssembly()?.GetName().Name is null or empty.");
                return "Unknown";
            }

            // Using structured logging format
            logger.LogDebug("The executing assembly name is {AssemblyName}.", assemblyName);
            return assemblyName;
        }

        private string GetCallingAssemblyName()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            if (callingAssembly == null)
            {
                logger.LogTrace("Assembly.GetCallingAssembly() returned null.");
                return "Unknown";
            }

            var assemblyName = callingAssembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                logger.LogTrace("Assembly.GetCallingAssembly()?.GetName().Name is null or empty.");
                return "Unknown";
            }

            // Using structured logging format
            logger.LogDebug("The calling assembly name is {AssemblyName}.", assemblyName);
            return assemblyName;
        }

        private string GetCurrentProcessMainModuleName()
        {
            try
            {
                var mainModule = Process.GetCurrentProcess().MainModule;
                if (mainModule == null)
                {
                    logger.LogTrace("Process.GetCurrentProcess().MainModule returned null.");
                    return "Unknown";
                }

                var moduleName = mainModule.ModuleName;
                if (string.IsNullOrEmpty(moduleName))
                {
                    logger.LogTrace("Process.GetCurrentProcess().MainModule.ModuleName is null or empty.");
                    return "Unknown";
                }

                // Using structured logging format
                logger.LogDebug("The current process main module name is {ModuleName}.", moduleName);
                return moduleName;
            }
            catch (Exception ex)
            {
                // Structured logging for exception
                logger.LogError(ex, "Failed to get current process main module name due to an exception.");
                return "Permission Denied";
            }
        }

        private string GetCurrentProcessMainModuleFilename()
        {
            try
            {
                var mainModule = Process.GetCurrentProcess().MainModule;
                if (mainModule == null)
                {
                    logger.LogTrace("Process.GetCurrentProcess().MainModule returned null.");
                    return "Unknown";
                }

                var moduleFullPath = mainModule.FileName;
                if (string.IsNullOrEmpty(moduleFullPath))
                {
                    logger.LogTrace("Process.GetCurrentProcess().MainModule.FileName is null or empty.");
                    return "Unknown";
                }

                // Using structured logging format
                logger.LogDebug("The current process main module full path is {ModuleFullPath}.", moduleFullPath);
                return moduleFullPath;
            }
            catch (Exception ex)
            {
                // Structured logging for exception
                logger.LogError(ex, "Failed to get current process main module full path due to an exception.");
                return "Permission Denied";
            }
        }

        private string GetOSArchitecture()
        {
            return RuntimeInformation.OSArchitecture.ToString();
        }

        private string GetProcessArchitecture()
        {
            return RuntimeInformation.ProcessArchitecture.ToString();
        }

        private string GetEnvironmentCommandLine()
        {
            return Environment.CommandLine;
        }

        private string GetCurrentDirectory()
        {
            try
            {
                return System.IO.Directory.GetCurrentDirectory();
            }
            catch (Exception ex)
            {
                // Structured logging for exception
                logger.LogError(ex, "Failed to get current directory due to an exception.");
                return "Exception";
            }
        }
    }
}