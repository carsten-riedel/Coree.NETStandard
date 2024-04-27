namespace Coree.NETStandard.Extensions.Primitives.Bool
{
    /// <summary>
    /// Provides extension methods for working with nullable boolean values.
    /// </summary>
    public static partial class BoolExtensions
    {
        /// <summary>
        /// Checks if the nullable boolean value is true.
        /// </summary>
        /// <param name="nullableBool">The nullable boolean value.</param>
        /// <returns>True if the value is true; otherwise, false.</returns>
        public static bool IsTrue(this bool? nullableBool)
        {
            return nullableBool.HasValue && nullableBool.Value;
        }
    }
}
