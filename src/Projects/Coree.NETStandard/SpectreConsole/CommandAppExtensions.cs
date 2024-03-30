using System;
using System.Collections.Generic;
using System.Text;

using Spectre.Console.Cli;

namespace Coree.NETStandard.SpectreConsole
{
    /// <summary>
    /// Provides extension methods for <see cref="ICommandApp"/>.
    /// </summary>
    public static partial class CommandAppExtensions
    {
        /// <summary>
        /// Retrieves a list of command types registered with the <see cref="ICommandApp"/>.
        /// This method uses reflection to access the private configurator field within the <see cref="ICommandApp"/> instance,
        /// then extracts and returns the types of the registered commands.
        /// </summary>
        /// <param name="commandApp">The <see cref="ICommandApp"/> instance to extract command types from.</param>
        /// <returns>A list of <see cref="Type"/> objects representing the command types registered with the <see cref="ICommandApp"/>.</returns>
        /// <remarks>
        /// This method relies on the internal structure of the <see cref="ICommandApp"/> implementation.
        /// If the implementation changes in future versions, this method may no longer work as expected.
        /// </remarks>
        public static List<Type> GetCommandTypes(this ICommandApp commandApp)
        {
            var _configurator = commandApp.GetType().GetField("_configurator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(commandApp);
            var Commands = _configurator?.GetType().GetProperty("Commands", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?.GetValue(_configurator);
            var CommandsList = (System.Collections.IList?)Commands;
            List<Type> types = new List<Type>();

            if (CommandsList != null)
            {
                foreach (var item in CommandsList)
                {
                    var CommandTypeProperty = item.GetType().GetProperty("CommandType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (CommandTypeProperty != null)
                    {
                        var CommandType = CommandTypeProperty.GetValue(item) as Type;
                        if (CommandType != null)
                        {
                            types.Add(CommandType);
                        }
                    }
                }
            }

            return types;
        }
    }
}

