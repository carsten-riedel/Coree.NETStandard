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

        public enum ExitCode : int
        {
            FailedToRun = 125,
            CommandNotFound = 127,
            HelpVersionDisplayed = 64,
            CommandTerminated = 130,
            SuccessAndContinue = 1,
            SuccessAndExit = 2,
        }

        public SpectreConsoleHostedService(ICommandApp commandApp, IHostApplicationLifetime appLifetime)
        {
            this.commandApp = commandApp;
            this.appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            //Exitcode 0 will be returned if just help is displayed, Exitcode -1 will be returned if command is not found.
            _ = Task.Run(async () =>
            {
                Environment.ExitCode = (int)ExitCode.FailedToRun;
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
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}