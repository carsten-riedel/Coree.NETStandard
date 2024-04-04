using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace Coree.NETStandard.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder ConfigureServices(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, IServiceCollection> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder, hostBuilder.Services);
            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureServices(this IHostApplicationBuilder hostBuilder, Action<IServiceCollection> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder.Services);
            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureAppConfiguration(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, IConfigurationBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder, hostBuilder.Configuration);
            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureAppConfiguration(this IHostApplicationBuilder hostBuilder, Action<IConfigurationBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder.Configuration);
            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, ILoggingBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder, hostBuilder.Logging);
            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder hostBuilder, Action<ILoggingBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder.Logging);
            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureHostOptions(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, HostOptions> configureOptions)
        {
            Action<IHostApplicationBuilder, HostOptions> configureOptions2 = configureOptions;
            return hostBuilder.ConfigureServices(delegate (IHostApplicationBuilder context, IServiceCollection collection)
            {
                collection.Configure(delegate (HostOptions options)
                {
                    configureOptions2(context, options);
                });
            });
        }

        public static IHostApplicationBuilder ConfigureHostOptions(this IHostApplicationBuilder hostBuilder, Action<HostOptions> configureOptions)
        {
            Action<HostOptions> configureOptions2 = configureOptions;
            return hostBuilder.ConfigureServices(delegate (IServiceCollection collection)
            {
                collection.Configure(configureOptions2);
            });
        }

        public static IHostApplicationBuilder UseConsoleLifetime(this IHostApplicationBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(delegate (IServiceCollection collection)
            {
                collection.AddSingleton<IHostLifetime, ConsoleLifetime>();
            });
        }
    }
}
