using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Classes.Criteria
{
    public class CriteriaItems
    {
        public CriteriaItem<object>[] Filters { get; set; }

        public CriteriaItems(params CriteriaItem<object>[] filters)
        {
            Filters = filters;
        }
    }
}
