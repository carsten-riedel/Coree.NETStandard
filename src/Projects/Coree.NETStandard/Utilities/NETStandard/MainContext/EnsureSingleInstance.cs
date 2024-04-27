using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using Coree.NETStandard.Extensions;

namespace Coree.NETStandard.Utilities
{
    /// <summary>
    /// Provides utility methods for managing the application's execution context, typically used within the Program.Main entry point.
    /// </summary>
    public static partial class MainContext
    {
#pragma warning disable IDE0052 // Remove unread private members
        private static Mutex? mutex;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        /// Ensures that only a single instance of the application is running. If a duplicate instance is detected,
        /// it executes a specified action and optionally exits the application.
        /// </summary>
        /// <param name="onDuplicateInstance">An <see cref="Action"/> to be executed if a duplicate instance is found. This action is intended to provide a way to notify the user or log the event of a duplicate instance attempt.</param>
        /// <param name="exit">A boolean value indicating whether to exit the application if a duplicate instance is detected. Defaults to true, causing the application to exit.</param>
        /// <remarks>
        /// This method attempts to create a named <see cref="Mutex"/> based on the application's entry assembly name. If the mutex cannot be created because it already exists, this indicates that an instance of the application is already running.
        /// </remarks>
        public static void EnsureSingleInstance(Action onDuplicateInstance, bool exit = true)
        {
            string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UniqueAppName";
            mutex = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                onDuplicateInstance();
                if (exit)
                {
                    Environment.Exit(0);
                }
            }
        }
    }
}
