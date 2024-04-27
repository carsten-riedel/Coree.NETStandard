namespace Coree.NETStandard.Extensions.Primitives.Bool
{
    /// <summary>
    /// Provides extension methods for working with nullable boolean values.
    /// </summary>
    public static partial class PrimitivesBoolExtensions
    {
        /// <summary>
        /// Checks if the nullable boolean value is false.
        /// </summary>
        /// <param name="nullableBool">The nullable boolean value.</param>
        /// <returns>True if the value is false; otherwise, false.</returns>
        public static bool IsFalse(this bool? nullableBool)
        {
            return nullableBool.HasValue && !nullableBool.Value;
        }
    }
}