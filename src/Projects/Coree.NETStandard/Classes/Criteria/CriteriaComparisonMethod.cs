namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Specifies the comparison methods available for evaluating string properties in criteria expressions.
    /// </summary>
    public enum CriteriaComparisonMethod
    {
        /// <summary>
        /// Specifies that the string property should contain a specified substring.
        /// </summary>
        Contains,

        /// <summary>
        /// Specifies that the string property should start with a specified substring.
        /// </summary>
        StartsWith,

        /// <summary>
        /// Specifies that the string property should end with a specified substring.
        /// </summary>
        EndsWith,

        /// <summary>
        /// Specifies that the string property should be either null, empty, or consist only of whitespace characters.
        /// </summary>
        IsNullOrWhiteSpace,

        /// <summary>
        /// Specifies that the string property should be equal to a specified string, typically considering case-sensitivity.
        /// </summary>
        Equals,

        /// <summary>
        /// Specifies that a numeric or date/time property should be greater than a specified value.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Specifies that a numeric or date/time property should be less than a specified value.
        /// </summary>
        LessThan,

        /// <summary>
        /// Specifies that a numeric or date/time property should be greater than or equal to a specified value.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Specifies that a numeric or date/time property should be less than or equal to a specified value.
        /// </summary>
        LessThanOrEqual
    }
}