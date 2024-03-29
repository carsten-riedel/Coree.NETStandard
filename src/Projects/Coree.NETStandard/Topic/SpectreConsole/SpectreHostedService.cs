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
    public class SpectreHostedService : BackgroundService
    {
        private readonly ICommandApp _app;
        private readonly IHostApplicationLifetime appLifetime;
        private int CommandAppExitCode;

        public SpectreHostedService(ICommandApp app, IHostApplicationLifetime appLifetime)
        {
            _app = app;
            this.appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await WaitForApplicationStartAsync(appLifetime.ApplicationStarted);
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            //Exitcode 0 will be returned if just help is displayed, Exitcode -1 will be returned if command is not found.
            CommandAppExitCode = await _app.RunAsync(args);

            if (CommandAppExitCode == 0 || CommandAppExitCode == -1)
            {
                appLifetime.StopApplication();
                Environment.ExitCode = CommandAppExitCode;
            }

            if (CommandAppExitCode == 1)
            {
                Environment.ExitCode = 0;
            }
            else
            {
                Environment.ExitCode = CommandAppExitCode;
            }
        }
    }
}