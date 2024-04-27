using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Classes.Criteria
{
    public class CriteriaItem<TValue>
    {
        public string PropertyName { get; set; }
        public CriteriaComparisonMethod ComparisonMethod { get; set; }
        public TValue Value { get; set; }
        public bool Negate { get; set; }
        public CriteriaOperator Operator { get; set; } = CriteriaOperator.And; // Default to AND

        public CriteriaItem(string propertyName, CriteriaComparisonMethod comparisonMethod, TValue value, bool negate = false, CriteriaOperator logicalOperator = CriteriaOperator.And)
        {
            PropertyName = propertyName;
            ComparisonMethod = comparisonMethod;
            Value = value;
            Negate = negate;
            Operator = logicalOperator;
        }

        public static implicit operator CriteriaItem<object>(CriteriaItem<TValue> expression)
        {
            return new CriteriaItem<object>(expression.PropertyName, expression.ComparisonMethod, expression.Value, expression.Negate, expression.Operator);
        }
    }

}
