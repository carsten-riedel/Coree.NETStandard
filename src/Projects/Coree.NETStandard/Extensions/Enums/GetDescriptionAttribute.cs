using System;

using System.ComponentModel;
using System.Reflection;

namespace Coree.NETStandard.Extensions.Enums
{
    /// <summary>
    /// Provides extension methods for enum types.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the description from the DescriptionAttribute applied to an enum value, if any.
        /// </summary>
        /// <param name="value">The enum value for which to retrieve the description.</param>
        /// <returns>The description string from the DescriptionAttribute of the specified enum value. Returns null if no DescriptionAttribute is found.</returns>
        public static string? GetDescriptionAttribute(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute? attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}
