using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Coree.NETStandard.Topic.SpectreConsole
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpectreConsole(this IServiceCollection services, Action<IConfigurator> configureCommandApp)
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

            services.AddHostedService<SpectreHostedService>();
            return services;
        }
    }
}
