using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Classes.Spectre;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Coree.NETStandard.Topic.SpectreConsole
{

    /// <summary>
    /// Enables type registration for Spectre.Console applications, supporting both <see cref="IServiceProvider"/> and <see cref="IServiceCollection"/>.
    /// </summary>
    public class SpectreConsoleTypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceProvider? serviceProvider;
        private readonly IServiceCollection? services;
        private readonly bool disposeServiceProvider;

        /// <summary>
        /// Constructs with an <see cref="IServiceProvider"/>, typically for hosted applications using <c>builder.Build()</c>.
        /// <param name="disposeServiceProvider">True to dispose the service provider on disposal.</param>
        /// </summary>
        /// <param name="serviceProvider">Service provider for type resolution.</param>
        public SpectreConsoleTypeRegistrar(IServiceProvider serviceProvider, bool disposeServiceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.disposeServiceProvider = disposeServiceProvider;
        }

        /// <summary>
        /// Constructs with an <see cref="IServiceCollection"/>, for manual service collection setups, e.g., simple console apps.
        /// </summary>
        /// <param name="serviceCollection">Service collection for registrations.</param>
        /// <param name="disposeServiceProvider">True to dispose the service provider on disposal.</param>
        public SpectreConsoleTypeRegistrar(IServiceCollection serviceCollection, bool disposeServiceProvider)
        {
            this.services = serviceCollection;
            this.disposeServiceProvider = disposeServiceProvider;
        }

        /// <summary>
        /// Builds a type resolver based on the provided service provider or service collection.
        /// </summary>
        /// <returns>Type resolver for configured services.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no service provider or collection is provided.</exception>
        public ITypeResolver Build()
        {
            if (serviceProvider != null)
            {
                return new SpectreConsoleTypeResolver(serviceProvider, disposeServiceProvider);
            }
            else if (services != null)
            {
                return new SpectreConsoleTypeResolver(services.BuildServiceProvider(), disposeServiceProvider);
            }
            else
            {
                throw new InvalidOperationException("Service provider or collection required.");
            }
        }

        /// <summary>
        /// Registers a service with its implementation in the service collection. Only for <see cref="IServiceCollection"/> usage.
        /// </summary>
        /// <param name="service">Service type.</param>
        /// <param name="implementation">Implementation type.</param>
        public void Register(Type service, Type implementation)
        {
            services?.AddSingleton(service, implementation);
        }

        /// <summary>
        /// Registers a service instance in the service collection. Only for <see cref="IServiceCollection"/> usage.
        /// </summary>
        /// <param name="service">Service type.</param>
        /// <param name="implementation">Service instance.</param>
        public void RegisterInstance(Type service, object implementation)
        {
            services?.AddSingleton(service, implementation);
        }

        /// <summary>
        /// Registers a service with a factory function for deferred instantiation. Only for <see cref="IServiceCollection"/> usage.
        /// </summary>
        /// <param name="service">Service type.</param>
        /// <param name="func">Factory function.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
        public void RegisterLazy(Type service, Func<object> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func), "Factory function required.");
            services?.AddSingleton(service, provider => func());
        }
    }

}
