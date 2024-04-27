using System;

using Spectre.Console.Cli;

namespace Coree.NETStandard.SpectreConsole
{
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