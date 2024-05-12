namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Represents a single criteria item used for building dynamic filter expressions.
    /// </summary>
    /// <typeparam name="TValue">The type of the value used in the criteria comparison.</typeparam>
    public class CriteriaItem<TValue>
    {
        /// <summary>
        /// Gets or sets the name of the property to be evaluated by the criteria.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the comparison method to be used in evaluating the criteria.
        /// </summary>
        public CriteriaComparisonMethod ComparisonMethod { get; set; }

        /// <summary>
        /// Gets or sets the value against which the property's value will be compared.
        /// This value can be null.
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the result of the criteria comparison should be negated.
        /// </summary>
        public bool Negate { get; set; }

        /// <summary>
        /// Gets or sets the logical operator to use in conjunction with previous or subsequent criteria.
        /// </summary>
        public CriteriaOperator Operator { get; set; } = CriteriaOperator.And; // Default to AND

        /// <summary>
        /// Initializes a new instance of the <see cref="CriteriaItem{TValue}"/> class with specified settings for filtering.
        /// </summary>
        /// <param name="propertyName">The property name of the object to evaluate.</param>
        /// <param name="comparisonMethod">The method of comparison to apply.</param>
        /// <param name="value">The value to compare against the property.</param>
        /// <param name="negate">True to negate the result of the comparison; otherwise, false.</param>
        /// <param name="logicalOperator">The logical operator to use in combining this criteria with others.</param>
        public CriteriaItem(string propertyName, CriteriaComparisonMethod comparisonMethod, TValue value, bool negate = false, CriteriaOperator logicalOperator = CriteriaOperator.And)
        {
            PropertyName = propertyName;
            ComparisonMethod = comparisonMethod;
            Value = value;
            Negate = negate;
            Operator = logicalOperator;
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="CriteriaItem{TValue}"/> to a <see cref="CriteriaItem{Object}"/>.
        /// </summary>
        /// <param name="expression">The <see cref="CriteriaItem{TValue}"/> to convert.</param>
        /// <returns>A new <see cref="CriteriaItem{Object}"/> that contains the converted instance.</returns>
        public static implicit operator CriteriaItem<object>(CriteriaItem<TValue> expression)
        {
            return new CriteriaItem<object>(expression.PropertyName, expression.ComparisonMethod, expression.Value!, expression.Negate, expression.Operator);
        }
    }
}