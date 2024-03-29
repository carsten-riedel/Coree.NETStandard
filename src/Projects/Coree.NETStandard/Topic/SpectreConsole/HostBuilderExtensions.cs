using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Coree.NETStandard.Topic.SpectreConsole
{
    public static partial class HostBuilderExtensions
    {
        public static IHostBuilder UseSpectreConsole(this IHostBuilder builder, Action<IConfigurator> configureCommandApp)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices((_, services) =>
            {
                var cmdapp = new CommandApp();
                cmdapp.Configure(configureCommandApp);
                List<Type> types = cmdapp.GetCommandTypes();

                foreach (var commandType in types)
                {
                    services.AddSingleton(commandType);
                }

                services.AddSingleton<ICommandApp>((serviceProvider) =>
                {
                    var app = new CommandApp(new SpectreConsoleTypeRegistrar(serviceProvider, false));
                    app.Configure(configureCommandApp);
                    return app;
                });

                services.AddHostedService<SpectreConsoleHostedService>();
                //services.Configure<ConsoleLifetimeOptions>(e => { e.SuppressStatusMessages = true; });

            });

            return builder;
        }
    }
}
