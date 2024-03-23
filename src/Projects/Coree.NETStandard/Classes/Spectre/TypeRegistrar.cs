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
}
