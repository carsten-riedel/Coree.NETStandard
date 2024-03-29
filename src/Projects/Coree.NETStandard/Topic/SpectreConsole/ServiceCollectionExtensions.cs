﻿using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

namespace Coree.NETStandard.Topic.SpectreConsole
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to facilitate the integration of Spectre.Console's CommandApp.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures Spectre.Console's <see cref="CommandApp"/> and related services to the specified <see cref="IServiceCollection"/>.
        /// This setup allows for utilizing Spectre.Console's command line interface tools within a .NET Core application, 
        /// enabling the definition, parsing, and execution of commands.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureCommandApp">An <see cref="Action"/> delegate that is used to configure the <see cref="CommandApp"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Initializes a new instance of <see cref="CommandApp"/> and configures it using the provided <paramref name="configureCommandApp"/> action.
        /// - Scans for command types registered within the <see cref="CommandApp"/> and registers them as singletons in the service collection.
        /// - Registers the <see cref="CommandApp"/> itself as a singleton service, configured to use a <see cref="SpectreConsoleTypeRegistrar"/> for dependency injection.
        /// - Adds a <see cref="SpectreConsoleBackgroundService"/>, to manage the application lifecycle of the <see cref="CommandApp"/>.
        /// </remarks>
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

            services.AddHostedService<SpectreConsoleHostedService>();
            return services;
        }
    }
}
