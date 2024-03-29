﻿using CbOrm.Util;
using CbOrm.Gen;
using CbOrm.Xdl;
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



    public abstract class CGenCtorArgsBuilderAttribute : Attribute
    {
        public abstract IEnumerable<CodeExpression> NewCtorArgs(CGenModelInterpreter aModelInterpreter, CCodeDomBuilder aDomBuilder, CRflProperty aProperty);
    }

    public sealed class CGenR1NPRefCtorArgsBuilderAttribute : CGenCtorArgsBuilderAttribute
    {
        public CGenR1NPRefCtorArgsBuilderAttribute()
        {
        }
        public override IEnumerable<CodeExpression> NewCtorArgs(CGenModelInterpreter aModelInterpreter, CCodeDomBuilder aDomBuilder, CRflProperty aProperty)
        {
            var aCTyp = aProperty.DeclaringTyp;
            var aCTypNme = aModelInterpreter.GetTypName(aCTyp,false);
            var aCdTypRef = new CodeTypeReference(aCTypNme);
            var aCdTypRefExp = new CodeTypeReferenceExpression(aCdTypRef);
            var aFldNme = aModelInterpreter.Tok.Trg_C_Mta_Pfx + aProperty.Name + aModelInterpreter.Tok.Trg_P_Fk_Sfx + aModelInterpreter.Tok.Dom_F_Mta_P_Sfx;
            var aFldRefExp = new CodeFieldReferenceExpression(aCdTypRefExp, aFldNme);
            yield return aFldRefExp;
        }
    }
    public sealed class CGenR11PRefCtorArgsBuilderAttribute : CGenCtorArgsBuilderAttribute
    {
        public CGenR11PRefCtorArgsBuilderAttribute()
        {
        }
        public override IEnumerable<CodeExpression> NewCtorArgs(CGenModelInterpreter aModelInterpreter, CCodeDomBuilder aDomBuilder, CRflProperty aProperty)
        {
            var aPTyp = aModelInterpreter.GetReturnTyp(aProperty);
            var aPTypNme = aModelInterpreter.GetTypName(aPTyp, false);
            var aCTyp = aProperty.DeclaringTyp;
            var aCTypNme = aCTyp.Name;
            var aCdTypRef = new CodeTypeReference(aCTypNme);
            var aCdTypRefExp = new CodeTypeReferenceExpression(aCdTypRef);
            var aFldNme = "_" + aProperty.Name + "_" + aModelInterpreter.Tok.Trg_P_Fk_Sfx  + aModelInterpreter.Tok.Dom_F_Mta_P_Sfx; //aProperty.Name.TrimStart(aModelInterpreter.Tok.Trg_P_Parent_Pfx).TrimStart(aPTypNme) + aModelInterpreter.Tok.Trg_P_Fk_Sfx + aModelInterpreter.Tok.Trg_C_Mta_P_Rel_Sfx;
            var aFldRefExp = new CodeFieldReferenceExpression(aCdTypRefExp, aFldNme);
            yield return aFldRefExp;
        }
    }

    public sealed class CGenR11CRefCtorArgsBuilderAttribute:  CGenCtorArgsBuilderAttribute
    {
        public override IEnumerable<CodeExpression> NewCtorArgs(CGenModelInterpreter aModelInterpreter, CCodeDomBuilder aDomBuilder, CRflProperty aProperty)
        {
            var aCTyp = aModelInterpreter.GetReturnTyp(aProperty);
            var aGenarateReverseNavigation = aCTyp.Interpret(() => bool.Parse(aCTyp.GetAttributeValue(aModelInterpreter.Tok.Mdl_T_A_GenerateReverseNavigation, () => true.ToString())));
            if (aGenarateReverseNavigation)
            {
                var aCTypNme = aModelInterpreter.GetTypName(aCTyp, true);
                var aCdTypRef = new CodeTypeReference(aCTypNme);
                var aCdTypRefExp = new CodeTypeReferenceExpression(aCdTypRef);
                var aPrpNme = aModelInterpreter.Tok.GetRelationyMetaInfoPropertyName(aModelInterpreter.GetR11CReverseNavigationRefName(aProperty));
                var aFldRefExp = new CodePropertyReferenceExpression(aCdTypRefExp, aPrpNme);
                yield return aFldRefExp;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class CSaveAsAttribute : Attribute
    {
        public CSaveAsAttribute(Type aSaveAsType)
        {
            this.SaveAsType = aSaveAsType;
        }
        internal readonly Type SaveAsType;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class CSaveConverterAttribute : Attribute
    {
        public CSaveConverterAttribute(Type aConverterType)
        {
            this.ConverterType = aConverterType;
        }
        internal readonly Type ConverterType;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CTargetTypeAttribute : Attribute
    {
        public CTargetTypeAttribute(Type aTargetType)
        {
            this.TargetType = aTargetType;
        }
        internal readonly Type TargetType;
    }

}
