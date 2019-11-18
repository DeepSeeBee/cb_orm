using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Attributes
{
    public abstract class CAttributeValueTypeAttribute : Attribute
    {
        public abstract IEnumerable<CodeAttributeArgument> NewCodeAttributeArguments(string aAttributeValue);
    }

    public sealed class CAttributeTypeValueTypeAttribute: CAttributeValueTypeAttribute
    {
        public override IEnumerable<CodeAttributeArgument> NewCodeAttributeArguments(string aAttributeValue)
        {
            yield return new CodeAttributeArgument(new CodeTypeOfExpression(aAttributeValue));
        }
    }

    public sealed class CAttributeStringValueTypeAttribute : CAttributeValueTypeAttribute
    {
        public override IEnumerable<CodeAttributeArgument> NewCodeAttributeArguments(string aAttributeValue)
        {
            yield return new CodeAttributeArgument(new CodePrimitiveExpression(aAttributeValue));
        }
    }


    public sealed class CAttributBoolBalueTypeAttribute : CAttributeValueTypeAttribute
    {
        public override IEnumerable<CodeAttributeArgument> NewCodeAttributeArguments(string aAttributeValue)
        {
            yield return new CodeAttributeArgument(new CodePrimitiveExpression(bool.Parse(aAttributeValue)));
        }
    }


    public sealed class CSpreadAcrossTablesAttribute : Attribute
    {
        public CSpreadAcrossTablesAttribute(bool aValue)
        {
            this.Value = aValue;
        }
        public readonly bool Value;
    }

    public abstract class CValueAttribute :Attribute
    {

        public abstract object ValueObj { get; }
    }



    [CAttributeTypeValueType]
    public sealed class CForeignKeyParentTypeAttribute  : CValueAttribute
    {
        public CForeignKeyParentTypeAttribute(Type aValue)
        {
            this.Value = aValue;
        }
        internal readonly Type Value;
        public override object ValueObj => this.Value;
    }

    [CAttributeStringValueType]
    public sealed class CForeignKeyParentPropertyNameAttribute : CValueAttribute
    {
        public CForeignKeyParentPropertyNameAttribute(string aValue)
        {
            this.Value = aValue;
        }
        internal readonly string Value;
        public override object ValueObj => this.Value;
    }

    [CAttributBoolBalueType]
    public sealed class CAutoCreateAttribute :CValueAttribute
    {
        public CAutoCreateAttribute(bool aValue)
        {
            this.Value = aValue;
        }
        public readonly bool Value;
        public override object ValueObj => this.Value;
    }

}
