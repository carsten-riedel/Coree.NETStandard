namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Defines logical operators that can be used to combine multiple criteria conditions.
    /// </summary>
    public enum CriteriaOperator
    {
        /// <summary>
        /// Represents a logical AND operator that requires all combined criteria to be true.
        /// </summary>
        And,

        /// <summary>
        /// Represents a logical OR operator that requires at least one of the combined criteria to be true.
        /// </summary>
        Or
    }
}