namespace Coree.NETStandard.Extensions.Primitives.Bool
{
    /// <summary>
    /// Provides extension methods for working with nullable boolean values.
    /// </summary>
    public static partial class PrimitivesBoolExtensions
    {
        /// <summary>
        /// Checks if the nullable boolean value is null.
        /// </summary>
        /// <param name="nullableBool">The nullable boolean value.</param>
        /// <returns>True if the value is null; otherwise, false.</returns>
        public static bool IsNull(this bool? nullableBool)
        {
            return !nullableBool.HasValue;
        }
    }
}