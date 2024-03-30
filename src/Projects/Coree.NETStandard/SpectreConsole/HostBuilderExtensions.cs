using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

namespace Coree.NETStandard.SpectreConsole
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
        /// <param name="suppressLifeTimeStatusMessages">Supresses default output of the application host.</param>
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
        public static IHostBuilder UseSpectreConsole(this IHostBuilder builder, Action<IConfigurator> configureCommandApp, bool suppressLifeTimeStatusMessages = true)
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
                if (suppressLifeTimeStatusMessages)
                {
                    services.Configure<ConsoleLifetimeOptions>(e => { e.SuppressStatusMessages = true; });
                }
            });

            return builder;
        }

        /// <summary>
        /// Sets up a command execution environment within the application's host configuration, where commands are executed synchronously.
        /// This method creates an entry point for a command-line application that supports multiple commands, each running in a synchronous manner within its own task context.
        /// In the event of a service cancellation, the currently executing command will be terminated abruptly.
        /// </summary>
        /// <param name="builder">The host builder to configure, which is augmented with services and configurations necessary for managing command execution.</param>
        /// <param name="configureCommandApp">A delegate that configures the command-line application, facilitating the registration and setup of individual commands.</param>
        /// <param name="suppressLifeTimeStatusMessages">Optional. Determines whether to suppress host lifetime status messages, with a default value of true. When set to false, these messages are made visible, offering insights into the lifecycle events of the host.</param>
        /// <returns>The host builder, now configured with the synchronous command execution environment, ready for further configuration or immediate use.</returns>
        /// <remarks>
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
        ///     builder.UseSpectreConsoleSyncCommandExecutor(configCommandApp =>
        ///     {
        ///         configCommandApp.SetApplicationName("testapp");
        ///         configCommandApp.AddCommand&lt;MyAsyncCommand&gt;("my-command");
        ///     });
        ///     await builder.Build().RunAsync();
        /// }
        ///
        /// public class MyAsyncCommand : AsyncCommand&lt;MyAsyncCommand.Settings&gt;
        /// {
        ///     private readonly ISomeServiceDependency dependencyService;
        ///     private readonly IHostApplicationLifetime appLifetime;
        ///
        ///     public MyAsyncCommand(ISomeServiceDependency dependencyService, IHostApplicationLifetime appLifetime)
        ///     {
        ///         this.dependencyService = dependencyService;
        ///         this.appLifetime = appLifetime;
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
        ///         // Attempts to abort the command gracefully will fail because the command runs on a separate task and not in the service host's managed context.
        ///         try {
        ///             await dependencyService.PerformConsoleAction(appLifetime.ApplicationStopping);
        ///         }
        ///         catch (Exception)
        ///         {
        ///             // This catch block is unreachable. The service host's abrupt termination of the command prevents this block from being executed.
        ///             return (int)SpectreConsoleHostedService.ExitCode.CommandTerminated;
        ///         }
        ///         // It's crucial to explicitly manage the service host's state after command execution completes, either continuing operation or initiating shutdown.
        ///         return (int)SpectreConsoleHostedService.ExitCode.SuccessAndContinue;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the provided host builder is null, ensuring method reliability and preventing null reference errors.</exception>
        public static IHostBuilder UseSpectreConsoleSyncCommandExecutor(this IHostBuilder builder, Action<IConfigurator> configureCommandApp, bool suppressLifeTimeStatusMessages = true)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices((_, services) =>
            {
                var command = new CommandApp(new SpectreConsoleTypeRegistrar(services, false));
                command.Configure(configureCommandApp);
                services.AddSingleton<ICommandApp>(command);
                services.AddHostedService<SpectreConsoleHostedService>();
                if (suppressLifeTimeStatusMessages)
                {
                    services.Configure<ConsoleLifetimeOptions>(e => { e.SuppressStatusMessages = true; });
                }
            });

            return builder;
        }
    }
}