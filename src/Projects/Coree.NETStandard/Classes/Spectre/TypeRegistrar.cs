using System;
using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace Coree.NETStandard.Classes.Spectre
{
    /// <summary>
    /// Registers types into an <see cref="IServiceCollection"/> for dependency injection.
    /// </summary>
    public sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection services;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRegistrar"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add registrations to.</param>
        public TypeRegistrar(IServiceCollection services)
        {
            this.services = services;
        }

        /// <summary>
        /// Builds a type resolver using the registered services.
        /// </summary>
        /// <returns>An <see cref="ITypeResolver"/> that resolves types from the registered services.</returns>
        public ITypeResolver Build()
        {
            return new TypeResolver(services.BuildServiceProvider());
        }

        /// <summary>
        /// Registers a service with a concrete implementation.
        /// </summary>
        /// <param name="service">The service type to register.</param>
        /// <param name="implementation">The implementation type of the service.</param>
        public void Register(Type service, Type implementation)
        {
            services.AddSingleton(service, implementation);
        }

        /// <summary>
        /// Registers an instance of a service.
        /// </summary>
        /// <param name="service">The service type to register.</param>
        /// <param name="implementation">The instance of the service implementation.</param>
        public void RegisterInstance(Type service, object implementation)
        {
            services.AddSingleton(service, implementation);
        }

        /// <summary>
        /// Registers a service with a factory function for lazy initialization.
        /// </summary>
        /// <param name="service">The service type to register.</param>
        /// <param name="func">The factory function that creates the service instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
        public void RegisterLazy(Type service, Func<object> func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            services.AddSingleton(service, (provider) => func());
        }
    }

    /// <summary>
    /// Resolves types by fetching them from an underlying <see cref="IServiceProvider"/>.
    /// This class also implements <see cref="IDisposable"/> to allow for proper disposal of the service provider if it supports it.
    /// </summary>
    public sealed class TypeResolver : ITypeResolver, IDisposable
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeResolver"/> class with the specified service provider.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use for type resolution.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
        public TypeResolver(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Resolves the service of the specified type from the service provider.
        /// </summary>
        /// <param name="type">The type of the service to resolve. If null, this method returns null.</param>
        /// <returns>The service object of type <paramref name="type"/> if available; otherwise, null.</returns>
        public object? Resolve(Type? type)
        {
            if (type == null)
            {
                return null;
            }

            return _provider.GetService(type);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// If the underlying service provider is disposable, it will be disposed.
        /// </summary>
        public void Dispose()
        {
            if (_provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }


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

    /// <summary>
    /// Resolves types from an <see cref="IServiceProvider"/>, and optionally disposes it.
    /// </summary>
    public sealed class SpectreConsoleTypeResolver : ITypeResolver, IDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly bool DisposeServiceProvider; // Indicates whether to dispose the service provider upon disposal.

        /// <summary>
        /// Initializes with a specified service provider.
        /// </summary>
        /// <param name="provider">Service provider for type resolution.</param>
        /// <param name="DisposeServiceProvider">True to dispose the service provider on disposal.</param>
        public SpectreConsoleTypeResolver(IServiceProvider provider, bool DisposeServiceProvider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.DisposeServiceProvider = DisposeServiceProvider;
        }

        /// <summary>
        /// Resolves a service of the specified type.
        /// </summary>
        /// <param name="type">Service type; returns null if type is null.</param>
        /// <returns>Service object or null if not found.</returns>
        public object? Resolve(Type? type)
        {
            return type == null ? null : _provider.GetService(type);
        }

        /// <summary>
        /// Disposes the service provider if disposable.
        /// </summary>
        public void Dispose()
        {
            if (DisposeServiceProvider && _provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

}
