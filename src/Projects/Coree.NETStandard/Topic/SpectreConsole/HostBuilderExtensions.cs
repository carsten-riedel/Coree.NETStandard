using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Coree.NETStandard.Topic.SpectreConsole
{
    /// <summary>
    /// Provides extension methods for <see cref="IHostBuilder"/> to facilitate the integration of Spectre.Console's CommandApp into the application's host builder.
    /// </summary>
    public static partial class HostBuilderExtensions
    {
        /// <summary>
        /// Configures the application's host builder to use Spectre.Console's <see cref="CommandApp"/> and related services.
        /// This setup enables the application to utilize Spectre.Console's command line interface tools, 
        /// allowing for the definition, parsing, and execution of commands within a .NET Core application.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/> to configure.</param>
        /// <param name="configureCommandApp">An <see cref="Action{IConfigurator}"/> delegate that configures the <see cref="CommandApp"/>.</param>
        /// <returns>The <see cref="IHostBuilder"/> so that additional configuration calls can be chained.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Creates and configures a new instance of <see cref="CommandApp"/> using the provided <paramref name="configureCommandApp"/> action.
        /// - Scans for command types within the <see cref="CommandApp"/> and registers them as singletons in the service collection.
        /// - Registers the <see cref="CommandApp"/> as a singleton service, configured to use a <see cref="SpectreConsoleTypeRegistrar"/> for dependency injection.
        /// - Adds a <see cref="SpectreConsoleHostedService"/> to the services collection to manage the lifecycle of the <see cref="CommandApp"/> within the host application.
        /// <example>
        /// <code>
        /// static async Task Main(string[] args)
        /// {
        ///     var builder = Host.CreateDefaultBuilder(args);
        ///     builder.ConfigureServices(services =>
        ///     {
        ///         services.AddSingleton&lt;ISomeServiceDependency, SomeServiceDependency&gt;();
        ///         services.AddHostedService&lt;SomeOtherBackgroundService&gt;();
        ///     });
        ///     builder.UseSpectreConsole(configCommandApp =>
        ///     {
        ///         configCommandApp.SetApplicationName("testapp");
        ///         configCommandApp.AddCommand&lt;MyAsyncCommand&gt;("my-command");
        ///     });
        ///     await builder.Build().RunAsync();
        /// }
        /// 
        /// public class MyAsyncCommand : AsyncCommand&lt;MyAsyncCommand.Settings&gt;
        /// {
        ///     private readonly ISomeServiceDependency dependencyService; private readonly IHostApplicationLifetime appLifetime;
        ///     
        ///     public MyAsyncCommand(ISomeServiceDependency dependencyService, IHostApplicationLifetime appLifetime)
        ///     {
        ///         this.dependencyService = dependencyService; this.appLifetime = appLifetime;
        ///     }
        ///     
        ///     public class Settings : CommandSettings
        ///     {
        ///         [Description("The commandline setting")]
        ///         [CommandArgument(0, "&lt;Setting&gt;")]
        ///         public string? SomeSetting { get; init; }
        ///     }
        ///     
        ///     public override async Task&lt;int&gt; ExecuteAsync(CommandContext context, Settings settings)
        ///     {
        ///         try { await dependencyService.PerformConsoleAction(appLifetime.ApplicationStopping); }
        ///         catch (Exception) { return (int)SpectreConsoleHostedService.ExitCode.CommandTerminated; }
        ///         return (int)SpectreConsoleHostedService.ExitCode.SuccessAndContinue;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>
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
