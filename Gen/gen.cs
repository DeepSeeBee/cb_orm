// This is my 2nd generation ORM-Wrapper. (code generator)
// TODO:
// - if(!ModelPopulated) PopulateModel auch für filesystemstorage
// - XDL DOcType und version.
// - CStorage.Load should return CObjectProxies collection.
// - Optionale ReverseNavigation Extending
// - Optionale ID Extending
// - WeakRef
// - Base=Mdl.Enum
// - ExpandIds
// - MultiTableInheritance
// - ObjectVersion
// - Meta-New in static ctor / lazyload ? performance improvement?
// - Support structs/classes as skalar fields?
// . throw own Exceotionclasses 

//
// - Property.WriteProtection: final check for all props.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using CbOrm.Xdl;
using System.Collections;
using CbOrm.Meta;
using CbOrm.Entity;
using CbOrm.Util;
using CbOrm.Schema;
using CbOrm.Loader;
using CbOrm.Attributes;
using CbOrm.Ref;
using CbOrm.Blop;
using CbOrm.App.Sys;

namespace CbOrm.Gen
{
    using CInclude = Tuple<CRflRow, FileInfo>;

    using CLazyLoadPrp = Tuple<CodeMemberProperty, CodeMemberField>;

    public enum CCardinalityEnum
    {
        Skalar,
        R11C,
        R11P,
        R1NC,
        R1NP,
        R1NW,
        R11W,        
    }

    public class CGenTokens : CRflTokens
    {

        // ModelDefinitionLange Identifier tokens
        public string Mdl_G_A_ModelName = "ModelName";
        public string Mdl_G_A_Include = "Include";

        public string Mdl_P_A_Nme_Init = "init";
        public string Mdl_P_A_Nme_Crd = "Cardinality";
        public string Mdl_P_A_Nme_DomA = "a:";
        public string Dom_P_Singleton_Nme = "Singleton";
        public string Mdl_P_A_Typ_Nme = "Typ";

        public string Mdl_T_A_Nme_Base = "Base";
        public string Mdl_T_A_Nme_Init = "Init";
        public string Mdl_T_A_Nme_ClrNs = "ClrNamespace";
        public string Mdl_T_A_Guid_Nme = "Guid";
        public string Mdl_T_A_GenerateReverseNavigation = "GenerateReverseNavigation";
        public string Mdl_T_A_Generate = "Generate";

        // CodeDOM Identifier tokens
        public string Dom_A_Cls_Sfx = "Attribute";

        public string Dom_C_Pfx = "C";        
        public string Dom_C_Ref_Sfx = "Ref";
        public string Dom_C_Sch_Sfx = "Schema";
        public string Dom_C_Var = "var";
        public string Dom_Nsp_Seperator = ".";
        public string Dom_F_Sfx = "M";
        public string Dom_F_Mta_P_Sfx = "_MetaInfo";

        public string Dom_O_NameOf_Nme = "nameof";
        public string Dom_O_TypeOf_Nme = "typeof";

        public string Dom_P_NewValNme = "value";
        public string Dom_P_Ref_Sfx = ""; // Ref

        public string Dom_Tmpl_IEnumerable = nameof(IEnumerable);

        // Target identfiier Tokens (Target=.NET Framework)
        public string Trg_C_SysObj_Nsp = typeof(object).Namespace;
        public string Trg_C_Obj_Nsp = typeof(Entity.CObject).Namespace;
        public string Trg_C_Obj_Nme = nameof(Entity.CObject);
        public string Trg_C_Eno_Nsp = typeof(Entity.CEntityObject).Namespace;
        public string Trg_C_Eno_Nme = nameof(Entity.CEntityObject);
        public string Trg_C_Str_Nsp = typeof(Storage.CStorage).Namespace;
        public string Trg_C_Str_Nme = nameof(Storage.CStorage);
        public string Trg_C_Ref_Nsp = typeof(Ref.CR1NCRef<Entity.CEntityObject, CEntityObject>).Namespace;
        public string Trg_P_AutoCreate_Nme = "AutoCreate";
        public string Trg_P_Parent_Pfx = "Parent_";
        public string Trg_P_A_WriteOnlyBySystem = "WriteOnlyBySystem";
        public string Trg_N_GetPrps_Nme = "_GetProperties";

        // Target Identifier Tokens (Target=.NET Framework, CbOrmMetaInfo)
        public string Trg_C_Mta_Pfx = "_";
        public string Trg_C_Mta_P_Fld_Nsp = typeof(Meta.CSkalarRefMetaInfo).Namespace;
        public string Trg_C_Mta_P_Fld_Nme = nameof(Meta.CSkalarRefMetaInfo);
        public string Trg_C_Mta_P_Rel_Sfx = "MetaInfo";
        public string Trg_C_Schema_M_GetSchmema = "GetSingleton";
        public string Trg_P_Fk_Sfx = "Guid";
        

        public virtual IEnumerable<Type> NativeTypes
        {
            get
            {
                //yield return typeof(Rfl.CSaveConverterAttribute);
                yield return typeof(IEnumerable<object>).GetGenericTypeDefinition();
            }
        }
        public IEnumerable<string> Trg_Nsps
        {
            get
            {
                yield return this.Trg_C_SysObj_Nsp;
                yield return this.Trg_C_Obj_Nsp;
                yield return this.Trg_C_Eno_Nsp;
                yield return this.Trg_C_Str_Nsp;
                yield return this.Trg_C_Ref_Nsp;
                yield return this.Trg_C_Mta_P_Fld_Nsp;
                foreach (var aNatTyp in this.NativeTypes)
                    yield return aNatTyp.Namespace;
            }
        }

        public virtual string GetClrAttributeClassNameFromModel(string aModelClassName) => this.Dom_C_Pfx
                                                                                                + aModelClassName
                                                                                                + this.Dom_A_Cls_Sfx;
        public virtual string GetClrClassNameFromModel(string aModelClassName) => this.Dom_C_Pfx + aModelClassName;
        public virtual string GetFieldName(string aPrpNme) => aPrpNme + this.Dom_F_Sfx;
        public virtual string GetRelationyMetaInfoPropertyName(string aPrpNme) => this.Trg_C_Mta_Pfx + aPrpNme + this.Dom_F_Mta_P_Sfx;
        public virtual string GetRelationMetaInfoFieldName(string aPropertyName) => this.GetFieldName(this.GetRelationyMetaInfoPropertyName(aPropertyName));
        public virtual string GetSchemaClassName(string aSchNme) => aSchNme.Length == 0 ? string.Empty : aSchNme + this.Dom_C_Sch_Sfx;
        public virtual string GeRelationMetaInfoTypName(string aRelTypName) => aRelTypName + this.Trg_C_Mta_P_Rel_Sfx;
        public virtual string GetGetSchemaFuncName() => this.Trg_C_Schema_M_GetSchmema;
        public virtual string GetModelAttributeTypName(Type aAttributeType) => aAttributeType.Name;
    }

    public class CGenModelInterpreter
    :
        CRflModelInterpreter
    , CIncludeModelExpander.IModelInterpreter
    {
        public readonly new CGenTokens Tok;
        public override CRflTokens Tokens => this.Tok;
        public CGenModelInterpreter(CGenTokens aTok) { this.Tok = aTok; }
        public CGenModelInterpreter() : this(new CGenTokens()) { }

        public virtual string GetSchema(CRflModel aMdl) => aMdl.GetAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_ModelName);
        public virtual string GetClrNamespace(CRflModel aModel) => aModel.GetAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Nsp_Nme);
        public virtual string GetClrNamespace(CRflTyp aTyp) => aTyp.GetAttributeValue(this.Tok.Mdl_T_A_Nme_ClrNs, () => this.GetClrNamespace(aTyp.Model));
        public virtual string GetTypName(CRflTyp aRflClass, bool aWithNamespace) => aWithNamespace
                                                                                 ? this.GetClrNamespace(aRflClass) + "." + aRflClass.Name
                                                                                 : aRflClass.Name;
        public virtual string GetBase(CRflTyp aRflClass) => aRflClass.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Base);
        public virtual bool GetIsEnum(CRflTyp aTyp) => this.GetBase(aTyp).Equals(nameof(Enum));
        public virtual string GetReturnTypName(CRflProperty aPrp) => this.GetIsEnum(aPrp.DeclaringTyp)
                                                                   ? aPrp.DeclaringTyp.Name
                                                                   : aPrp.Name.Length == 0 ? string.Empty : aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Typ_Nme).DefaultIfEmpty(() => aPrp.Name);
        public virtual CRflTyp GetReturnTyp(CRflProperty aPrp) => aPrp.DeclaringTyp.Model.GetTypByName(GetReturnTypName(aPrp));
        public virtual bool GetIsObject(CRflTyp aTyp) => this.GetIsEnum(aTyp)
                                                       ? false
                                                       : this.GetBase(aTyp).Length > 0
                                                       ? true
                                                       : this.GetIsBlop(aTyp)
                                                       ? true
                                                       : false
                                                       ;
        public virtual bool GetGenerate(CRflTyp aTyp) => aTyp.Interpret(() => bool.Parse(aTyp.GetAttributeValue(this.Tok.Mdl_T_A_Generate, () => true.ToString()))); 
        public virtual bool GetIsBlop(CRflTyp aTyp) => aTyp.Name == nameof(CBlop);
        public virtual string GetInit(CRflTyp aTyo) => aTyo.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Init);
        public virtual string GetInit(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Init).DefaultIfEmpty(() => this.GetInit(this.GetReturnTyp(aPrp)));
        public virtual IEnumerable<CRflAttribute> GetAttributes(CRflProperty aPrp) => aPrp.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomA);
        public virtual CCardinalityEnum GetCardinality(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Crd).DefaultIfEmpty(aCardinalityText => ("R" + aCardinalityText.Replace(":", string.Empty)).ParseEnum<CCardinalityEnum>(), () => this.GetIsObject(this.GetReturnTyp(aPrp)) ? CCardinalityEnum.R1NC : CCardinalityEnum.Skalar);
        public virtual string GetClassName(CCardinalityEnum aCardinality) => this.Tok.Dom_C_Pfx + aCardinality.ToString() + this.Tok.Dom_C_Ref_Sfx;
        public virtual string GetEntityRefPropertyName(CRflProperty aPrp) => aPrp.Name + this.Tok.Dom_P_Ref_Sfx;
        public virtual IEnumerable<CRflTyp> GetEntityObjectTyps(CRflModel aMdl) => from aTest in aMdl.Typs where this.GetBase(aTest) == this.Tok.Trg_C_Eno_Nme select aTest;

     
        public virtual IEnumerable<string> GetClrNamespaces(CRflModel aMdl) => (from aTyp in aMdl.Typs
                                                                          where aTyp.Name.Length > 0
                                                                          select aTyp.GetPropertyAttributeValue(this.Tok.Mdl_T_A_Nme_ClrNs)).Where(aNs => aNs.Length > 0);
        public virtual Guid GetGuid(CRflTyp aTyp) => aTyp.GetAttributeValue(this.Tok.Mdl_T_A_Guid_Nme).DefaultIfEmpty<Guid>(s => new Guid(s), () => default(Guid));

        public virtual string GetR1NCForeignKeyPropertyName(CRflProperty aR1NCProperty) => this.Tok.Trg_P_Parent_Pfx + aR1NCProperty.DeclaringTyp.Name + "_" + aR1NCProperty.Name + this.Tok.Trg_P_Fk_Sfx;
        internal IEnumerable<CRflRow> NewR1NCForeignKeyRows(CRflProperty aR1NCProperty)
        {
            var aR1NPTyp = aR1NCProperty.DeclaringTyp;
            var aR1NCTyp = this.GetReturnTyp(aR1NCProperty);
            var aFkPropertyName = this.GetR1NCForeignKeyPropertyName(aR1NCProperty);
            var aSelfTypName = aR1NPTyp.Name;
            var aFkTypName = aR1NCTyp.Name;
            var aGeneratedComment = "Generated by " + nameof(CGenModelInterpreter) + "." + nameof(NewR1NCForeignKeyRows);
            var aFkPropertyRow = CRflRow.New(aFkTypName, aFkPropertyName, this.Tok.Mdl_P_A_Typ_Nme, nameof(System.Guid), string.Empty, aGeneratedComment);
            var aFkPropertyWriteOnlyBySystemRow = CRflRow.New(aFkTypName, aFkPropertyName, this.Tok.Trg_P_A_WriteOnlyBySystem, true.ToString());
            var aFkCounterpartTypeAttributeType = this.Tok.GetModelAttributeTypName(typeof(CbOrm.App.Sys.CForeignKeyCounterpartTypeAttribute));
            var aFkCounterpartTypeAttributeValue = aSelfTypName;
            var aFkCounterpartTypeAttributeRow = CRflRow.New(aFkTypName, aFkPropertyName, this.Tok.Mdl_P_A_Nme_DomA + aFkCounterpartTypeAttributeType, aFkCounterpartTypeAttributeValue, string.Empty, aGeneratedComment);
            var aFkCounterpartPropertyAttributeType = this.Tok.GetModelAttributeTypName(typeof(CForeignKeyCounterpartPropertyNameAttribute));
            var aFkCounterpartPropertyAttributeValue = aR1NCProperty.Name;
            var aFkCounterpartAttributeRow = CRflRow.New(aFkTypName, aFkPropertyName, this.Tok.Mdl_P_A_Nme_DomA + aFkCounterpartPropertyAttributeType, aFkCounterpartPropertyAttributeValue, string.Empty, aGeneratedComment);
            yield return aFkPropertyRow;
            yield return aFkPropertyWriteOnlyBySystemRow;
            yield return aFkCounterpartTypeAttributeRow;
            yield return aFkCounterpartAttributeRow;
        }
        public virtual string GetR11CForeignKeyPropertyName(CRflProperty aR11CPRoperty) => aR11CPRoperty.Name + this.Tok.Trg_P_Fk_Sfx;
        internal IEnumerable<CRflRow> NewR11CForeignKeyRows(CRflProperty aR11CProperty)                
        {
            var aR11PTyp = aR11CProperty.DeclaringTyp;
            var aR11CTyp = this.GetReturnTyp(aR11CProperty);
            var aFkPropertyName = this.GetR11CForeignKeyPropertyName(aR11CProperty);
            var aGeneratedComment = "Generated by " + nameof(CGenModelInterpreter) + "." + nameof(NewR11CForeignKeyRows);
            var aSelfTypName = aR11PTyp.Name;
            var aSelfPropertyName = aR11CProperty.Name;
            var aFkPropertyRow = CRflRow.New(aSelfTypName, aFkPropertyName, this.Tok.Mdl_P_A_Typ_Nme, nameof(System.Guid), string.Empty, aGeneratedComment);
            var aFkCounterpartTypeAttributeType = this.Tok.GetModelAttributeTypName(typeof(CbOrm.App.Sys.CForeignKeyCounterpartTypeAttribute));
            var aFkCounterpartTypeAttributeValue = aSelfTypName;
            var aFkCounterpartTypeAttributeRow = CRflRow.New(aSelfTypName, aFkPropertyName, this.Tok.Mdl_P_A_Nme_DomA + aFkCounterpartTypeAttributeType, aFkCounterpartTypeAttributeValue, string.Empty, aGeneratedComment);
            var aFkCounterpartPropertyNameAttributeType = this.Tok.GetModelAttributeTypName(typeof(CForeignKeyCounterpartPropertyNameAttribute));
            var aFkParentPropertyAttributeValue = aSelfPropertyName;
            var aFkParentPropertyAttributeRow = CRflRow.New(aSelfTypName, aFkPropertyName, this.Tok.Mdl_P_A_Nme_DomA + aFkCounterpartPropertyNameAttributeType, aFkParentPropertyAttributeValue, string.Empty, aGeneratedComment);
            yield return aFkPropertyRow;
            yield return aFkCounterpartTypeAttributeRow;
            yield return aFkParentPropertyAttributeRow;
        }

        public virtual string GetR11CReverseNavigationRefName(CRflProperty aR11CProperty) => this.Tok.Trg_P_Parent_Pfx + aR11CProperty.DeclaringTyp.Name + "_" + aR11CProperty.Name;

        public virtual string GetR1NCReverseNavigationRefName(CRflProperty aR1NCProperty) => this.Tok.Trg_P_Parent_Pfx + aR1NCProperty.DeclaringTyp.Name + "_" + aR1NCProperty.Name;        

        public virtual IEnumerable<CRflRow> NewR1NCReverseNavigationRows(CRflProperty aR1NCProperty)
        {
            var aR1NPTyp = aR1NCProperty.DeclaringTyp;
            var aR1NCTyp = this.GetReturnTyp(aR1NCProperty);
            var aTargetTypName = aR1NCTyp.Name;
            var aReverseRefPropertyName = this.GetR1NCReverseNavigationRefName(aR1NCProperty);
            var aReferencedTypName = aR1NPTyp.Name;
            var aGeneratorComment = "Generated by " + nameof(CGenModelInterpreter) + "." + nameof(NewR1NCReverseNavigationRows);
            var aRefTypDeclRow = CRflRow.New(aTargetTypName, aReverseRefPropertyName, this.Tok.Mdl_P_A_Typ_Nme, aReferencedTypName, string.Empty, aGeneratorComment);
            var aRefCardinalityRow = CRflRow.New(aTargetTypName, aReverseRefPropertyName, this.Tok.Mdl_P_A_Nme_Crd, CCardinalityEnum.R1NP.ToString().TrimStart("R"), string.Empty, aGeneratorComment);
            yield return aRefTypDeclRow;
            yield return aRefCardinalityRow;
        }
        public virtual string GetR11CReverseNavigationFkPropertyName(CRflProperty aR11CProperty) => this.Tok.Trg_P_Parent_Pfx + aR11CProperty.DeclaringTyp.Name + "_" + aR11CProperty.Name  + "_" + this.Tok.Trg_P_Fk_Sfx;
        public virtual IEnumerable<CRflRow> NewR11CReverseNavigationRows(CRflProperty aR11CProperty)
        {
            var aR11PTyp = aR11CProperty.DeclaringTyp;
            var aR11CTyp = this.GetReturnTyp(aR11CProperty);
            var aTargetTypName = aR11CTyp.Name;
            var aReverseRefPropertyName = this.GetR11CReverseNavigationRefName(aR11CProperty);
            var aFkPropertyName = this.GetR11CReverseNavigationFkPropertyName(aR11CProperty);
            var aReferencedTypName = aR11PTyp.Name;
            var aGeneratorComment = "Generated by " + nameof(CGenModelInterpreter) + "." + nameof(NewR11CReverseNavigationRows);
            var aRefTypDeclRow = CRflRow.New(aTargetTypName, aReverseRefPropertyName, this.Tok.Mdl_P_A_Typ_Nme, aReferencedTypName, string.Empty, aGeneratorComment);
            var aRefCardinalityRow = CRflRow.New(aTargetTypName, aReverseRefPropertyName, this.Tok.Mdl_P_A_Nme_Crd, CCardinalityEnum.R11P.ToString().TrimStart("R"), string.Empty, aGeneratorComment);
            var aRefFkRow = CRflRow.New(aTargetTypName, aFkPropertyName, this.Tok.Mdl_P_A_Typ_Nme, nameof(Guid));
            var aFkCounterpartTypeAttributeType = this.Tok.GetModelAttributeTypName(typeof(CbOrm.App.Sys.CForeignKeyCounterpartTypeAttribute));
            var aSelfTypName = aR11CTyp.Name;
            var aSelfPropertyName = aReverseRefPropertyName;
            yield return aRefFkRow;
            yield return aRefTypDeclRow;
            yield return aRefCardinalityRow;
        }

        public IEnumerable<CInclude> GetIncludes(CRflModel aModel) => from aAttribute in aModel.GetAttributes(string.Empty, string.Empty, this.Tok.Mdl_G_A_Include) select new CInclude(aAttribute.Row, new FileInfo(Path.Combine(aModel.FileInfo.Directory.FullName, aAttribute.Value)));
        public CRflModel NewIncludedModel(FileInfo aFileInfo) => CRflModel.NewFromTextFile(this, aFileInfo);
        public virtual IEnumerable<CRflTyp> GetEnumTyps(CRflModel aModel) => from aTyp in aModel.Typs where this.GetIsEnum(aTyp) select aTyp;

      
    }

    public class CCodeDomBuilder
    {
        public CCodeDomBuilder(CGenModelInterpreter aIdl, CGenTokens aTok)
        {
            this.Idl = aIdl;
            this.Tok = aTok;
        }
        public CCodeDomBuilder() : this(new CGenModelInterpreter(), new CGenTokens()) { }

        public CGenModelInterpreter Idl;
        public CGenTokens Tok;

        public virtual CodeTypeReference NewCodeTypeRefFromModel(string aName) => new CodeTypeReference(aName);
        public virtual CodeTypeReference NewCodeTypeRef(CRflTyp aTyp) => this.NewCodeTypeRefFromModel(this.Idl.GetTypName(aTyp, true));
        public virtual CodeExpression NewNameOfPrpertyExpression(string aPrpName) => new CodeSnippetExpression(this.Tok.Dom_O_NameOf_Nme + "(" + aPrpName + ")");
        public virtual CodeExpression NewNameOTypeExpression(string aTypName) => new CodeSnippetExpression(this.Tok.Dom_O_NameOf_Nme + "(" + aTypName + ")");

        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDeclsGen(CRflProperty aProperty, CGenModelInterpreter aModelInterpreter)
            => from aAttribute in aProperty.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomA)
               where aModelInterpreter.GetGenerate(aProperty.Model.GetTypByName(aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomA)))
               select new CodeAttributeDeclaration(
                   new CodeTypeReference(this.Tok.GetClrAttributeClassNameFromModel(aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomA))),
                    new CodeAttributeArgument(new CodePrimitiveExpression(aAttribute.Value)));
        public virtual string GetAttributeTypeName(CRflProperty aProperty, string aModelAttributeTypeName, CGenModelInterpreter aModelInterpreter)
            => aModelAttributeTypeName.Contains(aModelInterpreter.Tok.Dom_Nsp_Seperator)
            ? aModelAttributeTypeName
            : aModelInterpreter.GetClrNamespace(aProperty.Model.GetTypByName(aModelAttributeTypeName)).Length == 0
            ? aModelAttributeTypeName
            : aModelInterpreter.GetClrNamespace(aProperty.Model.GetTypByName(aModelAttributeTypeName)) + aModelInterpreter.Tok.Dom_Nsp_Seperator + aModelAttributeTypeName
            ;


        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDeclsExist(CRflProperty aProperty, CGenModelInterpreter aModelInterpreter)
            => from aAttribute in aProperty.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomA)
               select new CodeAttributeDeclaration              (
                   new CodeTypeReference(this.GetAttributeTypeName(aProperty, aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomA), aModelInterpreter)),
                   System.Type.GetType(this.GetAttributeTypeName(aProperty, aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomA), aModelInterpreter)).GetCustomAttribute<CAttributeValueTypeAttribute>().NewCodeAttributeArguments(aAttribute.Value).ToArray()
               );
        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDeclsExplicit(CRflProperty aProperty)
        {
            var aAttributeName = this.Tok.Trg_P_AutoCreate_Nme;
            var aAttributeValue = aProperty.GetAttributeValue(aAttributeName);
            if (aAttributeValue != string.Empty)
            {
                yield return new CodeAttributeDeclaration(new CodeTypeReference(typeof(CbOrm.App.Sys.CAutoCreateAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(aProperty.GetAttribute(aAttributeName).Interpret(()=> bool.Parse(aAttributeValue)))));
            }
        }

        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDecls(CRflProperty aProperty, CGenModelInterpreter aModelInterpreter) =>
                        this.NewCodeAttributeDeclsExist(aProperty, aModelInterpreter)
                .Concat(this.NewCodeAttributeDeclsExplicit(aProperty));

        public virtual Tuple<CodeMemberProperty, CodeMemberField> NewLazyLoadPrperty(CodeTypeReference aCdRetTyp,
                                                                                      bool aIsClass,
                                                                       string aName,
                                                                       CodeExpression aLdExp,
                                                                       MemberAttributes aAttributes
                                                                       )
        {
            // MemberField
            var aCdFldNme = this.Tok.GetFieldName(aName);
            var aCdFld = new CodeMemberField(aCdRetTyp, aCdFldNme);
            var aCdFldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), aCdFldNme);
            var aEqMNme = aIsClass ? nameof(object.ReferenceEquals) : nameof(object.Equals);
            var aLdCond = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(object).Name),
                                                      aEqMNme,
                                                      aCdFldRef,
                                                      new CodePrimitiveExpression(null)
                                                      );
            var aLdStm = new CodeAssignStatement(aCdFldRef, aLdExp);
            var aLdIFStm = new CodeConditionStatement(aLdCond, new CodeStatement[] { aLdStm }, new CodeStatement[] {  });
            var aRetStm = new CodeMethodReturnStatement(aCdFldRef);
            var aCdPrp = new CodeMemberProperty();
            aCdPrp.Name = aName;
            aCdPrp.Attributes = aAttributes;
            aCdPrp.Type = aCdRetTyp;
            aCdPrp.HasGet = true;
            aCdPrp.GetStatements.Add(aLdIFStm);
            aCdPrp.GetStatements.Add(aRetStm);
            return new CLazyLoadPrp(aCdPrp, aCdFld);
        }

        internal CodeExpression NewTypeOfExpression(object aSystemTypeName) => new CodeSnippetExpression(this.NewTypeOfSnippet(aSystemTypeName));
        internal string NewTypeOfSnippet(object aSystemTypName) => this.Tok.Dom_O_TypeOf_Nme + "(" + aSystemTypName + ")";
        internal string NewYieldReturnStatementSnippet(string aItem) => "yield return " + aItem + ";";
        internal IEnumerable<CodeStatement> NewYieldReturnBaseItemsStatements(string aMethodName)
        {
            yield return new CodeSnippetStatement("foreach(var aItem in base." + aMethodName + "()) ");
            yield return new CodeSnippetStatement(this.NewYieldReturnStatementSnippet("aItem"));
        }
        internal virtual CodeTypeReference NewCodeTypeRef<T>() => new CodeTypeReference(typeof(T));
        internal CodeFieldReferenceExpression NewTypMetaInfoFieldRefExp(string aClassName) => new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(aClassName), this.GetTypMetaInfoFieldName(aClassName));
        internal string GetTypMetaInfoPropName(string aClassName)=> "_" + aClassName + "_" + "Typ";
        internal string GetTypMetaInfoFieldName(string aClassName) => this.Tok.GetFieldName(this.GetTypMetaInfoPropName(aClassName));
    }

    public sealed class CCodeGenerator
    {
        public CChainedModelExpander Exp = new CChainedModelExpander();
        public CGenModelInterpreter ModelInterpreter;
        public CGenTokens Tok;
        public CCodeDomBuilder CodeDomBuilder;

        public CCodeGenerator(FileInfo aModelInputFileInfo,
                                FileInfo aIdsInputFileInfo,
                                FileInfo aModelOutputFileInfo,
                                FileInfo aIdsOutputFileInfo)
            : this(new CGenModelInterpreter(),
                   new CGenTokens(),
                   new CCodeDomBuilder(),
                   aModelInputFileInfo,
                   aIdsInputFileInfo,
                   aModelOutputFileInfo,
                   aIdsOutputFileInfo)
        {
        }

        public CCodeGenerator(CGenModelInterpreter aMdlInterpreter,
                              CGenTokens aTok,
                              CCodeDomBuilder aDom,
                              FileInfo aModelInputFileInfo,
                              FileInfo aIdsInputFileInfo,
                              FileInfo aModelOutputFileInfo,
                              FileInfo aIdsOutputFileInfo)
        {
            this.ModelInterpreter = aMdlInterpreter;
            this.Tok = aTok;
            this.CodeDomBuilder = aDom;

            this.ModelInputFileInfo = aModelInputFileInfo;
            this.IdsInputFileInfoNullable = aIdsInputFileInfo;
            this.ModelOutputFileInfo = aModelOutputFileInfo;
            this.IdsOutputFileInfo = aIdsOutputFileInfo;

            this.Exp.ChainedExpanders.Add(new CIncludeModelExpander(this.ModelInterpreter));
            this.Exp.ChainedExpanders.Add(new CCrossReferenceExpander(this.ModelInterpreter));
        }

        public readonly FileInfo ModelInputFileInfo;
        public readonly FileInfo IdsInputFileInfoNullable;
        public readonly FileInfo ModelOutputFileInfo;
        public readonly FileInfo IdsOutputFileInfo;

        private CRflModel Model;
        public void GenerateCode()
        {
            this.LoadInput();
            this.GenerateModelOutput();
            this.ExtractOutputIds();
            this.SaveIdOutput();
        }

        private void SaveIdOutput()
        {
            //   throw new NotImplementedException();
        }

        private void ExtractOutputIds()
        {
            // throw new NotImplementedException();
        }

        private CodeTypeReference NewCodeTypeReference(string aClrTypName)
        {
            return new CodeTypeReference(aClrTypName);
        }


        private void AddMembers(CRflTyp aPTyp, CodeTypeDeclaration aCdTypDcl, CRflProperty aProperty)
        {
            var aModel = aProperty.Model;
            var aModelInterpreter = this.ModelInterpreter;
            var aCodeDomBuilder = this.CodeDomBuilder;
            var aPTypNme = this.ModelInterpreter.GetTypName(aPTyp, true);
            var aCdPTypRef = new CodeTypeReference(aPTypNme);
            var aCdPTypRefExp = new CodeTypeReferenceExpression(aCdPTypRef);
            var aCTyp = this.ModelInterpreter.GetReturnTyp(aProperty);
            var aPrpNme = aProperty.Name;
            string aMtaFldTypNme;
            var aMtaPrpNme = this.Tok.GetRelationyMetaInfoPropertyName(aPrpNme);
            var aMtaPrpRefExp = new CodePropertyReferenceExpression(aCdPTypRefExp, aMtaPrpNme);
            var aCTypRef = this.CodeDomBuilder.NewCodeTypeRef(aCTyp);            

            // Relations-Objects
            var aCrd = this.ModelInterpreter.GetCardinality(aProperty);
            var aRelTypName = this.ModelInterpreter.GetClassName(aCrd);
            var aRelSystemType = CRef.GetRefClass(aCrd);
            var aCdRTypRef = new CodeTypeReference(aRelTypName, this.CodeDomBuilder.NewCodeTypeRef(aPTyp), aCTypRef);
            var aCtorArgs1 = new CodeExpression[] { new CodeThisReferenceExpression(), aMtaPrpRefExp };
            var aCtorArgsBuildAttribIsDefined = aRelSystemType.IsNullRef() ? false : aRelSystemType.IsDefined(typeof(CGenCtorArgsBuilderAttribute), true);
            var aWriteOnlyBySystem = aProperty.Interpret(() => bool.Parse(aProperty.GetAttributeValue(this.Tok.Trg_P_A_WriteOnlyBySystem, false.ToString)));
            var aCtorArgs2 = aWriteOnlyBySystem ? new CodeExpression[] { new CodeObjectCreateExpression(typeof(CAccessKey)) } : new CodeExpression[] { };
            var aCtorArgBuilderAttrib = aCtorArgsBuildAttribIsDefined ? aRelSystemType.GetCustomAttribute<CGenCtorArgsBuilderAttribute>(true) : default(CGenCtorArgsBuilderAttribute);
            var aCtorArgs3 = aCtorArgBuilderAttrib.IsNullRef() ? new CodeExpression[] { } : aCtorArgBuilderAttrib.NewCtorArgs(aModelInterpreter, aCodeDomBuilder, aProperty);
            var aCtorArgs = aCtorArgs1.Concat(aCtorArgs2).Concat(aCtorArgs3);
            var aNewExp = new CodeObjectCreateExpression(aCdRTypRef, aCtorArgs.ToArray());
            var aIsClass = true;
            var aLazyPrp = this.CodeDomBuilder.NewLazyLoadPrperty(aCdRTypRef, aIsClass, this.ModelInterpreter.GetEntityRefPropertyName(aProperty), aNewExp, MemberAttributes.Public | MemberAttributes.Final);
            aCdTypDcl.Members.Add(aLazyPrp.Item1);
            aCdTypDcl.Members.Add(aLazyPrp.Item2);
            var aCdPrp = aLazyPrp.Item1;
            aMtaFldTypNme = this.Tok.GeRelationMetaInfoTypName(aRelTypName);

            // Property.Attributes from System
            aCdPrp.CustomAttributes.Add(new CodeAttributeDeclaration( new CodeTypeReference(typeof(CTargetTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(aCTypRef))));


            // Property.Attributes from Model
            var aCdAs = this.CodeDomBuilder.NewCodeAttributeDecls(aProperty, this.ModelInterpreter);
            aCdPrp.CustomAttributes.AddRange(aCdAs.ToArray());

            

            // Property.NetaInfo.Field
            var aMtaFldNme = this.Tok.GetRelationMetaInfoFieldName(aPrpNme);
            //aMtaFldTypNme = this.Tok.Trg_C_Mta_P_Fld_Nme;
            var aCdMtaFldTypRef = new CodeTypeReference(aMtaFldTypNme);
            var aCdMtaPrpFldDcl = new CodeMemberField(aMtaFldTypNme, aMtaFldNme);
            var aCdMtaPrpInitExp = new CodeObjectCreateExpression(aCdMtaFldTypRef,
                                                                  this.CodeDomBuilder.NewTypeOfExpression(aPTypNme),
                                                                  this.CodeDomBuilder.NewNameOfPrpertyExpression(aPrpNme)                                                                  
                                                                  );
            aCdMtaPrpFldDcl.Type = aCdMtaFldTypRef;
            aCdMtaPrpFldDcl.Attributes = MemberAttributes.Static
                                     | MemberAttributes.Private
                                     ;
            aCdMtaPrpFldDcl.InitExpression = aCdMtaPrpInitExp;
            aCdTypDcl.Members.Add(aCdMtaPrpFldDcl);

            var aCdMtaFldRefExp = new CodeFieldReferenceExpression(aCdPTypRefExp, aMtaFldNme);

            // Property.MetaInfo.Property
            var aCdMtaPrp = new CodeMemberProperty();
            aCdMtaPrp.Name = aMtaPrpNme;
            aCdMtaPrp.Type = aCdMtaFldTypRef;
            aCdMtaPrp.Attributes = MemberAttributes.Static
                                 | MemberAttributes.Public
                                 ;
            aCdMtaPrp.HasGet = true;
            aCdMtaPrp.GetStatements.Add(new CodeMethodReturnStatement(aCdMtaFldRefExp));
            aCdTypDcl.Members.Add(aCdMtaPrp);

        }

        private CodeTypeDeclaration NewCodeEntityObjectTypeDecl(CRflTyp aRflTyp)
        {
            var aModel = aRflTyp.Model;
            var aSchemaClsNme = this.Tok.GetSchemaClassName(this.ModelInterpreter.GetSchema(aModel));
            var aGetSchemaMthNme = this.Tok.GetGetSchemaFuncName();

            // Class
            var aClassName = this.ModelInterpreter.GetTypName(aRflTyp, false);
            var aCdTypDecl = new CodeTypeDeclaration(aClassName);
            var aCdTypRef = new CodeTypeReference(aClassName);
            var aCdTypRefExp = new CodeTypeReferenceExpression(aCdTypRef);
            aCdTypDecl.BaseTypes.Add(this.NewCodeTypeReference(this.Tok.Trg_C_Eno_Nme));

            var aGetPrpsMthdNme = this.Tok.Trg_N_GetPrps_Nme;
            var aGetPrpsMthActionArgNme = "aAddProperty";
            var aCdGetPrpsMthdRefExp = new CodeMethodReferenceExpression(aCdTypRefExp, aGetPrpsMthdNme);

            // Typ.Field
            var aTypTypRef = this.CodeDomBuilder.NewCodeTypeRef<CTyp>();
            var aTypFldNme = this.CodeDomBuilder.GetTypMetaInfoFieldName(aClassName);
            var aCdTypFldDcl = new CodeMemberField(aTypTypRef, aTypFldNme);
            var aCdTypeOfExp = this.CodeDomBuilder.NewTypeOfExpression(aClassName);
            var aCdNameOfExp = this.CodeDomBuilder.NewNameOTypeExpression(aClassName);
            var aGuid = this.ModelInterpreter.GetGuid(aRflTyp);
            var aCdGuidTypeRef = new CodeTypeReference(typeof(Guid));
            var aCdGuidExp = new CodeObjectCreateExpression(aCdGuidTypeRef, new CodePrimitiveExpression(aGuid.ToString()));
            var aCdPrototypeExp = new CodeObjectCreateExpression(aCdTypRef, new CodePrimitiveExpression(null));
            var aCdAddPrpsMthdRef = new CodeMethodReferenceExpression(aCdTypRefExp, aGetPrpsMthdNme);
            aCdTypFldDcl.Attributes = MemberAttributes.Public
                                  | MemberAttributes.Static
                                  ;
            aCdTypFldDcl.InitExpression = new CodeObjectCreateExpression(aTypTypRef, aCdTypeOfExp, aCdGuidExp, aCdGetPrpsMthdRefExp);
            aCdTypDecl.Members.Add(aCdTypFldDcl);

            var aTypFldRefExp = new CodeFieldReferenceExpression(aCdTypRefExp, aTypFldNme);

            // Typ.Property._Static
            var aTypPrpNme = this.CodeDomBuilder.GetTypMetaInfoPropName(aClassName);
            var aCdTypPrp = new CodeMemberProperty();
            aCdTypPrp.Name = aTypPrpNme;
            aCdTypPrp.Attributes = MemberAttributes.Static
                                 | MemberAttributes.Public
                                 ;
            aCdTypPrp.Type = aTypTypRef;
            aCdTypPrp.GetStatements.Add(new CodeMethodReturnStatement(aTypFldRefExp));
            aCdTypPrp.HasGet = true;
            aCdTypDecl.Members.Add(aCdTypPrp);

            var aCdTypPrpRefExp = new CodePropertyReferenceExpression(aCdTypRefExp, aTypPrpNme);

            // Typ.Property.Get.Override
            var aOverrideTypPrpName = nameof(CObject.Typ);
            var aCdOverrideTypPrp = new CodeMemberProperty();
            aCdOverrideTypPrp.Attributes = MemberAttributes.Public
                                         | MemberAttributes.Override
                                         ;
            aCdOverrideTypPrp.Type = aTypTypRef;
            aCdOverrideTypPrp.Name = aOverrideTypPrpName;
            aCdOverrideTypPrp.GetStatements.Add(new CodeMethodReturnStatement(aCdTypPrpRefExp));
            aCdOverrideTypPrp.HasGet = true;
            aCdTypDecl.Members.Add(aCdOverrideTypPrp);

            // Constructor
            var aCodeConstructor = new CodeConstructor();
            aCodeConstructor.Attributes = MemberAttributes.Public;
            var aStorageArgName = "aStorage";
            aCodeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(this.NewCodeTypeReference(this.Tok.Trg_C_Str_Nme), aStorageArgName));
            aCodeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(aStorageArgName));
            aCdTypDecl.Members.Add(aCodeConstructor);

            

            // Properties
            var aNmePrps = aRflTyp.NamedProperties;
            foreach (var aPrp in aNmePrps)
            {
                this.AddMembers(aRflTyp, aCdTypDecl, aPrp);
            }

            // CObject.GetProperties
            var aActionOfPropertyType = typeof(Action<CRefMetaInfo>);
            var aCdActionOfPropertyTypRef = new CodeTypeReference(aActionOfPropertyType);
            var aCdGetPrpsMth = new CodeMemberMethod();
            aCdGetPrpsMth.Name = aGetPrpsMthdNme;
            aCdGetPrpsMth.Attributes = MemberAttributes.Static 
                                     | MemberAttributes.Private
                                     ;
            aCdGetPrpsMth.Parameters.Add(new CodeParameterDeclarationExpression(aCdActionOfPropertyTypRef, aGetPrpsMthActionArgNme));
            foreach(var aNmePrp in aNmePrps)
            {
                var aPrpNme = aNmePrp.Name;
                var aMtaPrpNme = this.Tok.GetRelationyMetaInfoPropertyName(aPrpNme);
                var aMtaPrpRefExp = new CodePropertyReferenceExpression(aCdTypRefExp, aMtaPrpNme);
                var aArgRefExp = new CodeArgumentReferenceExpression(aGetPrpsMthActionArgNme);
                var aCdCallExpnew = new CodeMethodInvokeExpression(aArgRefExp, nameof(Action.Invoke), aMtaPrpRefExp);
                aCdGetPrpsMth.Statements.Add(aCdCallExpnew);
            }
            aCdTypDecl.Members.Add(aCdGetPrpsMth);

            return aCdTypDecl;
        }

        private CodeTypeDeclaration NewCodeEnumTypeDecl(CRflTyp aEnumTyp)
        {
            var aEnumTypName = aEnumTyp.Name;
            var aCdTypeDecl = new CodeTypeDeclaration(aEnumTypName);
            aCdTypeDecl.IsEnum = true;
            var aFields = from aProperty in aEnumTyp.NamedProperties select new CodeMemberField(aEnumTypName, aProperty.Name);
            aCdTypeDecl.Members.AddRange(aFields.ToArray());
            return aCdTypeDecl;
        }

        private void GenerateModelOutput()
        {
            var aModel = this.Model;
            var aNamespace = new CodeNamespace();
            var aEntityObjectTyps = this.ModelInterpreter.GetEntityObjectTyps(aModel);
            var aCdEntityObjectTypeDecls = (from aRflClass in aEntityObjectTyps select this.NewCodeEntityObjectTypeDecl(aRflClass));
            var aGeneratedEnumTyps = from aEnumTyp in this.ModelInterpreter.GetEnumTyps(aModel) where this.ModelInterpreter.GetGenerate(aEnumTyp) select aEnumTyp;
            var aCdEnumTypeDecls = from aEnumTyp in aGeneratedEnumTyps select this.NewCodeEnumTypeDecl(aEnumTyp);

            var aCompileUnit = new CodeCompileUnit();
            var aSchemaClass = this.NewSchemaClasses(aModel);

            aNamespace.Name = this.ModelInterpreter.GetNamespace(aModel);
            aCompileUnit.Namespaces.Add(aNamespace);
            aNamespace.Imports.AddRange((from aNs in this.Tok.Trg_Nsps.Concat(this.ModelInterpreter.GetClrNamespaces(aModel)).NewWithoutDuplicates() select new CodeNamespaceImport(aNs)).ToArray());
            aNamespace.Types.AddRange(aCdEnumTypeDecls.ToArray());
            aNamespace.Types.AddRange(aCdEntityObjectTypeDecls.ToArray());
            aNamespace.Types.AddRange(aSchemaClass.ToArray());

            var aCodeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            var aMemoryStream = new MemoryStream();
            using (var aStreamWriter = new StreamWriter(aMemoryStream))
            {
                var aCodeGeneratorOptions = new CodeGeneratorOptions();
                aCodeGeneratorOptions.BracingStyle = "C";
                aCodeDomProvider.GenerateCodeFromCompileUnit(aCompileUnit, aStreamWriter, aCodeGeneratorOptions);
                aStreamWriter.Flush();
                aMemoryStream.Position = 0;
                using (var aFileStream = this.ModelOutputFileInfo.OpenWrite())
                {
                    aMemoryStream.CopyTo(aFileStream);
                    aFileStream.SetLength(aFileStream.Position);
                    aFileStream.Flush();
                }
            }
            // TODO: throw new NotImplementedException();
        }

        private CodeStatement NewAddPrototypeStatement(CRflTyp aTyp) => 
            new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), CSchema.AddTyp_Name, this.CodeDomBuilder.NewTypMetaInfoFieldRefExp(this.ModelInterpreter.GetTypName(aTyp, false))));
        private CodeConstructor NewSchemaConstructor(CRflModel aModel)
        {
            var aCtor = new CodeConstructor();
            var aEnumTyps = this.ModelInterpreter.GetEnumTyps(aModel);
            var aRegisterEnumTypeCalls = from aEnumTyp in aEnumTyps select new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), CSchema.RegisterEnumType_Name, new CodeTypeOfExpression(this.ModelInterpreter.GetTypName(aEnumTyp, true))));
            var aRegisterDefaultCalculatorCalls = from aTyp in aModel.Typs
                                                  where aTyp.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Init).Length > 0
                                                  select new CodeExpressionStatement(
                                                      new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), CSchema.NameOf_RegisterDefaultCalculator),
                                                                                      new CodeTypeOfExpression(this.CodeDomBuilder.NewCodeTypeRef(aTyp)),
                                                                                        new CodeSnippetExpression("()=>" + aTyp.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Init))
                                                                                        )
                                                                                        );
            aCtor.Statements.AddRange((from aEnoCls in this.ModelInterpreter.GetEntityObjectTyps(aModel) select this.NewAddPrototypeStatement(aEnoCls)).ToArray());
            aCtor.Statements.AddRange(aRegisterEnumTypeCalls.ToArray());
            aCtor.Statements.AddRange(aRegisterDefaultCalculatorCalls.ToArray());
            aCtor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), CSchema.NameOf_Init));
            return aCtor;
        }

        private IEnumerable<CodeTypeMember> NewSchemaSingletonMembers(CRflModel aModel, string aSchemaClassName)
        {
            var aCdPrpNme = this.Tok.Dom_P_Singleton_Nme;
            var aFieldName = this.Tok.GetFieldName(aCdPrpNme);
            var aCdFieldType = new CodeTypeReference(aSchemaClassName);
            var aCdFieldDecl = new CodeMemberField(aCdFieldType, aFieldName);
            aCdFieldDecl.Attributes = MemberAttributes.Public
                                    | MemberAttributes.Static
                                    ;
            aCdFieldDecl.InitExpression = new CodeObjectCreateExpression(aCdFieldType);
            var aCdPrpDecl = new CodeMemberProperty();
            aCdPrpDecl.Name = aCdPrpNme;
            aCdPrpDecl.Type = aCdFieldType;
            aCdPrpDecl.Attributes = MemberAttributes.Public
                                            | MemberAttributes.Static
                                            ;
            var aCdTypeRefExp = new CodeTypeReferenceExpression(aSchemaClassName);
            var aCdFldRefExp = new CodeFieldReferenceExpression(aCdTypeRefExp, aFieldName);
            var aRetStm = new CodeMethodReturnStatement(aCdFldRefExp);
            aCdPrpDecl.GetStatements.Add(aRetStm);

            var aCdFieldTypeRef = new CodeTypeReferenceExpression(aSchemaClassName);

            var aGetSchemaMthNme = this.Tok.GetGetSchemaFuncName();
            var aCdGetSchemaMth = new CodeMemberMethod();
            aCdGetSchemaMth.Name = aGetSchemaMthNme;
            aCdGetSchemaMth.ReturnType = this.CodeDomBuilder.NewCodeTypeRef<CSchema>();
            aCdGetSchemaMth.Attributes = MemberAttributes.Static
                                       | MemberAttributes.Public
                                       ;
            aCdGetSchemaMth.Statements.Add(new CodeMethodReturnStatement(aCdFldRefExp));

            yield return aCdFieldDecl;
            yield return aCdPrpDecl;
            yield return aCdGetSchemaMth;
        }

        private IEnumerable<CodeTypeMember> NewSchemaMembers(CRflModel aModel, string aSchemaClassName)
        {
            yield return this.NewSchemaConstructor(aModel);
            foreach (var aMember in this.NewSchemaSingletonMembers(aModel, aSchemaClassName))
                yield return aMember;

        }

        private IEnumerable<CodeTypeDeclaration> NewSchemaClasses(CRflModel aModel)
        {
            var aSchemaClsNme = this.Tok.GetSchemaClassName(this.ModelInterpreter.GetSchema(aModel));
            if (aSchemaClsNme.Length > 0)
            {
                var aCdSchemaClassDecl = new CodeTypeDeclaration();
                aCdSchemaClassDecl.Name = aSchemaClsNme;
                aCdSchemaClassDecl.BaseTypes.Add(this.CodeDomBuilder.NewCodeTypeRef<CSchema>());
                aCdSchemaClassDecl.Members.AddRange(this.NewSchemaMembers(aModel, aSchemaClsNme).ToArray());
                yield return aCdSchemaClassDecl;
            }
        }
        private void LoadInput() => this.Model = this.Exp.Expand(CRflModel.NewFromTextFile(this.ModelInterpreter, this.ModelInputFileInfo));
    }
}
