using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;
using System.Linq;

namespace Coree.NETStandard.Topic.SpectreConsole
{
    /// <summary>
    /// Provides a hosted service for running Spectre.Console CLI applications
    /// within a .NET Core generic host. This class handles the application lifecycle,
    /// processes commands, and manages application exit codes based on command execution results.
    /// </summary>
    public class SpectreConsoleHostedService : IHostedService
    {
        private readonly ICommandApp commandApp;
        private readonly IHostApplicationLifetime appLifetime;

        private enum SpectreConsole : int
        {
            HelpVersionSuccess = 0,
            CommandNotFound = -1,
        }

        private enum GenericExitCode : int
        {
            Success = 0,
        }

        /// <summary>
        /// Defines exit codes for the SpectreConsoleHostedService application.
        /// These exit codes communicate the outcome of the application's execution
        /// to the calling process, allowing for scripted interactions and error handling.
        /// </summary>
        public enum ExitCode : int
        {
            /// <summary>
            /// Indicates that the application command failed to run. This can occur if initialization fails
            /// or if the application is aborted before it can run properly.
            /// </summary>
            CommandFailedToRun = 125,

            /// <summary>
            /// Indicates that the specified command could not be found. This exit code is used
            /// when a user inputs a command that does not match any known command patterns.
            /// </summary>
            CommandNotFound = 127,

            /// <summary>
            /// Indicates that help or version information was displayed. This neutral, Linux-like
            /// exit code is used when the application successfully displays help or version information
            /// upon request.
            /// </summary>
            HelpVersionDisplayed = 64,

            /// <summary>
            /// Indicates that the command was terminated, often due to an external interrupt signal
            /// (e.g., Ctrl+C). This code is reserved for scenarios where the application's execution
            /// is abruptly stopped.
            /// </summary>
            CommandTerminated = 130,

            /// <summary>
            /// Indicates that the command executed successfully and the application will not initiate a shutdown.
            /// This exit code signifies that the command was completed successfully,
            /// and the application remains active for further commands or operations. The application lifecycle
            /// is not affected, and `appLifetime.Stop` is not called.
            /// <para>Note: If background services or other tasks running within the application encounter a failure,
            /// they need to set a failed exit code manually (e.g., `Environment.ExitCode = non-zero value`) before
            /// calling `appLifetime.Stop` to ensure the exit code accurately reflects the application state.</para>
            /// </summary>
            SuccessAndContinue = 1,

            /// <summary>
            /// Indicates that the command executed successfully and the application will initiate a shutdown.
            /// This exit code is used when a command completes successfully and dictates that the application
            /// should perform a clean shutdown by calling `appLifetime.Stop`. It signifies the end of the application's
            /// current lifecycle, allowing for graceful termination.
            /// </summary>
            SuccessAndExit = 2,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectreConsoleHostedService"/> class
        /// with the specified Spectre.Console CLI application and application lifetime management services.
        /// </summary>
        /// <param name="commandApp">The Spectre.Console CLI application to be hosted.</param>
        /// <param name="appLifetime">The application lifetime management service.</param>
        public SpectreConsoleHostedService(ICommandApp commandApp, IHostApplicationLifetime appLifetime)
        {
            this.commandApp = commandApp;
            this.appLifetime = appLifetime;
        }

        /// <summary>
        /// Starts the hosted Spectre.Console CLI application asynchronously.
        /// Parses the command line arguments, executes the corresponding command,
        /// and sets the application exit code based on the outcome of the command execution.
        /// This method employs a specialized set of exit codes defined by the <see cref="ExitCode"/> enum,
        /// which may deviate from standard exit code conventions to suit the specific behavior and requirements
        /// of this application. For example, it includes codes for successful command execution that leads to application
        /// continuation or termination, and for handling cases where help information is displayed or a command is not found.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            //Exitcode 0 will be returned if just help is displayed, Exitcode -1 will be returned if command is not found.
            _ = Task.Run(async () =>
            {
                Environment.ExitCode = (int)ExitCode.CommandFailedToRun;
                var CommandAppExitCode = await commandApp.RunAsync(args);

                if (CommandAppExitCode == (int)SpectreConsole.HelpVersionSuccess)
                {
                    Environment.ExitCode = (int)ExitCode.HelpVersionDisplayed;
                    appLifetime.ApplicationStopped.Register(() =>
                    {
                        Environment.ExitCode = (int)ExitCode.HelpVersionDisplayed;
                    });
                    appLifetime.StopApplication();
                }

                if (CommandAppExitCode == (int)SpectreConsole.CommandNotFound)
                {
                    Environment.ExitCode = (int)ExitCode.CommandNotFound;
                    appLifetime.ApplicationStopped.Register(() =>
                    {
                        Environment.ExitCode = (int)ExitCode.CommandNotFound;
                    });
                    appLifetime.StopApplication();
                }

                if (CommandAppExitCode == (int)ExitCode.SuccessAndContinue)
                {
                    Environment.ExitCode = (int)GenericExitCode.Success;
                }
                else if (CommandAppExitCode == (int)ExitCode.SuccessAndExit)
                {
                    Environment.ExitCode = (int)GenericExitCode.Success;
                    appLifetime.ApplicationStopped.Register(() =>
                    {
                        Environment.ExitCode = (int)GenericExitCode.Success;
                    });
                    appLifetime.StopApplication();
                }
                else
                {
                    Environment.ExitCode = CommandAppExitCode;
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Currently, this method completes immediately as no cleanup is needed.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}