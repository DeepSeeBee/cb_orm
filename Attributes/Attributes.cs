using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Attributes
{
    public sealed class CSpreadAcrossTablesAttribute : Attribute
    {
        public CSpreadAcrossTablesAttribute(bool aValue)
        {
            this.Value = aValue;
        }
        public readonly bool Value;
    }
}
