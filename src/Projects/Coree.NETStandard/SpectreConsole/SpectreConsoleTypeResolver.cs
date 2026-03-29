using System;
using System.Collections;
using System.Collections.Generic;

using Spectre.Console.Cli;

namespace Coree.NETStandard.SpectreConsole
{
    internal sealed class SpectreConsoleCompositeServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider primaryProvider;
        private readonly IServiceProvider secondaryProvider;

        public SpectreConsoleCompositeServiceProvider(IServiceProvider primaryProvider, IServiceProvider secondaryProvider)
        {
            this.primaryProvider = primaryProvider;
            this.secondaryProvider = secondaryProvider;
        }

        public object? GetService(Type serviceType)
        {
            return SpectreConsoleTypeResolver.ResolveService(serviceType, primaryProvider, secondaryProvider);
        }
    }

    /// <summary>
    /// Resolves types from the primary host provider first, and falls back to Spectre's late runtime registrations.
    /// </summary>
    public sealed class SpectreConsoleTypeResolver : ITypeResolver, IDisposable
    {
        private readonly IServiceProvider primaryProvider;
        private readonly IServiceProvider runtimeProvider;
        private readonly bool disposePrimaryProvider;

        /// <summary>
        /// Initializes with the primary host provider and a secondary runtime provider.
        /// </summary>
        /// <param name="primaryProvider">Primary service provider for type resolution.</param>
        /// <param name="runtimeProvider">Runtime registration provider for late Spectre registrations.</param>
        /// <param name="disposePrimaryProvider">True to dispose the primary service provider on disposal.</param>
        public SpectreConsoleTypeResolver(IServiceProvider primaryProvider, IServiceProvider runtimeProvider, bool disposePrimaryProvider)
        {
            this.primaryProvider = primaryProvider ?? throw new ArgumentNullException(nameof(primaryProvider));
            this.runtimeProvider = runtimeProvider ?? throw new ArgumentNullException(nameof(runtimeProvider));
            this.disposePrimaryProvider = disposePrimaryProvider;
        }

        /// <summary>
        /// Resolves a service of the specified type.
        /// </summary>
        /// <param name="type">Service type; returns null if type is null.</param>
        /// <returns>Service object or null if not found.</returns>
        public object? Resolve(Type? type)
        {
            return type == null ? null : ResolveService(type, primaryProvider, runtimeProvider);
        }

        internal static object? ResolveService(Type type, IServiceProvider primaryProvider, IServiceProvider secondaryProvider)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return MergeEnumerables(type, primaryProvider.GetService(type), secondaryProvider.GetService(type));
            }

            return primaryProvider.GetService(type) ?? secondaryProvider.GetService(type);
        }

        private static object MergeEnumerables(Type enumerableType, object? primaryEnumerable, object? secondaryEnumerable)
        {
            var elementType = enumerableType.GetGenericArguments()[0];
            var items = new List<object?>();

            AppendEnumerable(items, primaryEnumerable);
            AppendEnumerable(items, secondaryEnumerable);

            var array = Array.CreateInstance(elementType, items.Count);
            for (int index = 0; index < items.Count; index++)
            {
                array.SetValue(items[index], index);
            }

            return array;
        }

        private static void AppendEnumerable(List<object?> items, object? source)
        {
            if (source is IEnumerable enumerable && source is not string)
            {
                foreach (var item in enumerable)
                {
                    items.Add(item);
                }
            }
        }

        /// <summary>
        /// Disposes the runtime provider, and optionally the primary provider.
        /// </summary>
        public void Dispose()
        {
            if (runtimeProvider is IDisposable runtimeDisposable)
            {
                runtimeDisposable.Dispose();
            }

            if (disposePrimaryProvider && primaryProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
