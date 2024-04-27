namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Represents a collection of criteria items to be used for building dynamic filter expressions.
    /// </summary>
    public class CriteriaItems
    {
        /// <summary>
        /// Gets or sets an array of <see cref="CriteriaItem{Object}"/> which define the filters to apply.
        /// </summary>
        /// <remarks>
        /// This property holds an array of criteria items, where each item specifies a condition to be used in filtering operations.
        /// </remarks>
        public CriteriaItem<object>[] Filters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CriteriaItems"/> class with the specified array of filter criteria.
        /// </summary>
        /// <param name="filters">An array of <see cref="CriteriaItem{Object}"/> that defines the set of criteria to use for filtering.</param>
        /// <remarks>
        /// This constructor allows for quick instantiation of <see cref="CriteriaItems"/> with an array of predefined criteria.
        /// Each <see cref="CriteriaItem{Object}"/> in the array represents a specific filter criterion that can be combined logically
        /// with others to perform complex queries.
        /// </remarks>
        public CriteriaItems(params CriteriaItem<object>[] filters)
        {
            Filters = filters;
        }
    }
}