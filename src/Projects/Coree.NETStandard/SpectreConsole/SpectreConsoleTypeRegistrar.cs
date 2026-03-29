using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace Coree.NETStandard.SpectreConsole
{
    /// <summary>
    /// Enables type registration for Spectre.Console applications, supporting both <see cref="IServiceProvider"/> and <see cref="IServiceCollection"/>.
    /// </summary>
    public class SpectreConsoleTypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceProvider? serviceProvider;
        private readonly IServiceCollection? services;
        private readonly bool disposeServiceProvider;
        private readonly List<Action<IServiceCollection, IServiceProvider>> runtimeRegistrations = new List<Action<IServiceCollection, IServiceProvider>>();

        /// <summary>
        /// Constructs with an <see cref="IServiceProvider"/>, typically for hosted applications using <c>builder.Build()</c>.
        /// <param name="disposeServiceProvider">True to dispose the service provider on CommandApp disposal.</param>
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
        /// <param name="disposeServiceProvider">True to dispose the service provider on CommandApp disposal.</param>
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
            IServiceProvider rootProvider;
            if (serviceProvider != null)
            {
                rootProvider = serviceProvider;
            }
            else if (services != null)
            {
                rootProvider = services.BuildServiceProvider();
            }
            else
            {
                throw new InvalidOperationException("Service provider or collection required.");
            }

            var runtimeServices = new ServiceCollection();
            foreach (var registration in runtimeRegistrations)
            {
                registration(runtimeServices, rootProvider);
            }

            return new SpectreConsoleTypeResolver(rootProvider, runtimeServices.BuildServiceProvider(), disposeServiceProvider);
        }

        /// <summary>
        /// Registers a service with its implementation for Spectre's runtime-created services.
        /// </summary>
        /// <param name="service">Service type.</param>
        /// <param name="implementation">Implementation type.</param>
        public void Register(Type service, Type implementation)
        {
            runtimeRegistrations.Add((runtimeServices, rootProvider) =>
            {
                runtimeServices.AddSingleton(service, runtimeProvider =>
                    ActivatorUtilities.CreateInstance(
                        new SpectreConsoleCompositeServiceProvider(rootProvider, runtimeProvider),
                        implementation));
            });
        }

        /// <summary>
        /// Registers a service instance for Spectre's runtime-created services.
        /// </summary>
        /// <param name="service">Service type.</param>
        /// <param name="implementation">Service instance.</param>
        public void RegisterInstance(Type service, object implementation)
        {
            runtimeRegistrations.Add((runtimeServices, _) => runtimeServices.AddSingleton(service, implementation));
        }

        /// <summary>
        /// Registers a service with a factory function for deferred instantiation.
        /// </summary>
        /// <param name="service">Service type.</param>
        /// <param name="func">Factory function.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
        public void RegisterLazy(Type service, Func<object> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func), "Factory function required.");
            runtimeRegistrations.Add((runtimeServices, _) => runtimeServices.AddSingleton(service, _ => func()));
        }
    }
}
