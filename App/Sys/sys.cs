using CbOrm.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.App.Sys
{
    public enum TriStateEnum
    {
        False,
        True,
        Maybe
    }

    [CAttributeTypeValueType]
    public sealed class CForeignKeyCounterpartTypeAttribute : CValueAttribute
    {
        public CForeignKeyCounterpartTypeAttribute(Type aValue)
        {
            this.Value = aValue;
        }
        internal readonly Type Value;
        public override object ValueObj => this.Value;
    }

    [CAttributeStringValueType]
    public sealed class CForeignKeyCounterpartPropertyNameAttribute : CValueAttribute
    {
        public CForeignKeyCounterpartPropertyNameAttribute(string aValue)
        {
            this.Value = aValue;
        }
        internal readonly string Value;
        public override object ValueObj => this.Value;
    }

    [CAttributBoolBalueType]
    public sealed class CAutoCreateAttribute : CValueAttribute
    {
        public CAutoCreateAttribute(bool aValue)
        {
            this.Value = aValue;
        }
        public readonly bool Value;
        public override object ValueObj => this.Value;
    }

}
