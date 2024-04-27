using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

namespace Coree.NETStandard.SpectreConsole
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
        /// <param name="suppressLifeTimeStatusMessages">Supresses default output of the application host.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Initializes a new instance of <see cref="CommandApp"/> and configures it using the provided <paramref name="configureCommandApp"/> action.
        /// - Scans for command types registered within the <see cref="CommandApp"/> and registers them as singletons in the service collection.
        /// - Registers the <see cref="CommandApp"/> itself as a singleton service, configured to use a <see cref="SpectreConsoleTypeRegistrar"/> for dependency injection.
        /// - Adds a <see cref="SpectreConsoleHostedService"/>, to manage the application lifecycle of the <see cref="CommandApp"/>.
        /// <example>
        /// <code>
        /// static async Task Main(string[] args)
        /// {
        ///     var builder = Host.CreateDefaultBuilder(args);
        ///     builder.ConfigureServices(services =>
        ///     {
        ///         services.AddSpectreConsole(configCommandApp =>
        ///         {
        ///             configCommandApp.SetApplicationName("testapp");
        ///             configCommandApp.AddCommand&lt;MyAsyncCommand&gt;("my-command");
        ///         });
        ///         services.AddSingleton&lt;ISomeServiceDependency, SomeServiceDependency&gt;();
        ///         services.AddHostedService&lt;SomeOtherBackgroundService&gt;();
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
        public static IServiceCollection AddSpectreConsole(this IServiceCollection services, Action<IConfigurator> configureCommandApp, bool suppressLifeTimeStatusMessages = true)
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
            return services;
        }

        /// <summary>
        /// Registers a command execution environment with Spectre.Console integration into the application's service collection. This setup facilitates the execution of command-line commands synchronously within their own task contexts. Designed to support multiple commands, this configuration allows for abrupt termination of commands upon service cancellation, ensuring immediate response to stop requests.
        /// </summary>
        /// <param name="services">The IServiceCollection to configure, which is augmented with the necessary services and configurations for Spectre.Console command execution.</param>
        /// <param name="configureCommandApp">A delegate that configures the command-line application, facilitating the registration and setup of individual commands within the Spectre.Console framework.</param>
        /// <param name="suppressLifeTimeStatusMessages">Optional. Determines whether to suppress host lifetime status messages, with a default value of true. Setting this to false enables the visibility of these messages, providing insights into the lifecycle events of the host and command execution.</param>
        /// <returns>The IServiceCollection, now configured with the Spectre.Console command execution environment, ready for further configuration or immediate use within the application's startup process.</returns>
        /// <remarks>
        /// Integrating the Spectre.Console command executor into your services collection. Commands are executed synchronously but in a separate task. Note that attempts to abort commands gracefully will face challenges due to their execution in separate tasks, which are not managed by the service host's context.
        /// <example>
        /// <code>
        /// static async Task Main(string[] args)
        /// {
        ///     var builder = Host.CreateDefaultBuilder(args);
        ///     builder.ConfigureServices(services =>
        ///     {
        ///         services.AddSingleton&lt;ISomeServiceDependency, SomeServiceDependency&gt;();
        ///         services.AddHostedService&lt;SomeOtherBackgroundService&gt;();
        ///         services.AddSpectreConsoleSyncCommandExecutor(configureCommandApp =>
        ///         {
        ///             configureCommandApp.SetApplicationName("toolkit");
        ///             configureCommandApp.AddCommand&lt;MyAsyncCommand&gt;("my-command");
        ///         });
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
        public static IServiceCollection AddSpectreConsoleSyncCommandExecutor(this IServiceCollection services, Action<IConfigurator> configureCommandApp, bool suppressLifeTimeStatusMessages = true)
        {
            var app = new CommandApp(new SpectreConsoleTypeRegistrar(services, false));
            app.Configure(configureCommandApp);
            services.AddSingleton<ICommandApp>(app);
            services.AddHostedService<SpectreConsoleHostedService>();
            if (suppressLifeTimeStatusMessages)
            {
                services.Configure<ConsoleLifetimeOptions>(e => { e.SuppressStatusMessages = true; });
            }
            return services;
        }
    }
}