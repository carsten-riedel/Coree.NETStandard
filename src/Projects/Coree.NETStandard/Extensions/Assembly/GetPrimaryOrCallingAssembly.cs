using System.Reflection;

namespace Coree.NETStandard.Extensions
{
    /// <summary>
    /// Provides extension methods for managing and retrieving assembly information.
    /// </summary>
    public static partial class AssemblyExtensions
    {
        /// <summary>
        /// Gets the entry assembly if available; otherwise, returns the calling assembly.
        /// This method is primarily useful in applications where you might need to
        /// fallback to the calling assembly if the entry assembly is not available
        /// (common in library scenarios or unit tests).
        /// </summary>
        /// <param name="assembly">The assembly to extend; not used in current context.</param>
        /// <returns>The primary (entry) or calling assembly.</returns>
        public static Assembly GetPrimaryOrCallingAssembly(this Assembly assembly)
        {
            var result = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            return result;
        }
    }
}