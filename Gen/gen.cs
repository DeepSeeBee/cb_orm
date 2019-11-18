// This is my 2nd generation ORM-Wrapper. (code generator)
// TODO:
// - R1NPRef.Guid WriteProtected.
// - xdl.Import, populate web.xdl 
// - xdl comment rows.
// - Optionale ReverseNavigation Extending
// - Optionale ID Extending
// - AutoCreate
// - WeakRef
// - Base=Mdl.Enum
// - ExpandIds
// - ExpandCrossreferences
// - MultiTableInheritance
// - ObjectVersion
// - Factor Output/Input
// - Mta obsolete?
// - Meta-New in static ctor / lazyload ? performance improvement?
// - Testing
// - Support structs/classes as skalar fields?


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

namespace CbOrm.Gen
{
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
        public string Mdl_G_A_Schema = "Schema";

        public string Mdl_P_A_Nme_Init = "init";
        public string Mdl_P_A_Nme_Crd = "Cardinality";
        public string Mdl_P_A_Nme_DomAGen = "a?:";
        public string Mdl_P_A_Nme_DomAExst = "a:";
        public string Dom_P_Singleton_Nme = "Singleton";
        public string MDl_O_A_Typ_Nme = "Typ";

        public string Mdl_T_A_Nme_Base = "Base";
        public string Mdl_T_A_Nme_Init = "Init";
        public string Mdl_T_A_Nme_ClrNs = "ClrNamespace";
        public string Mdl_T_A_Guid_Nme = "Guid";

        // CodeDOM Identifier tokens
        public string Dom_A_Cls_Sfx = "Attribute";

        public string Dom_C_Pfx = "C";        
        public string Dom_C_Ref_Sfx = "Ref";
        public string Dom_C_Sch_Sfx = "Schema";
        public string Dom_C_Var = "var";

        public string Dom_F_Sfx = "M";
        public string Dom_F_Mta_P_Sfx = "MetaInfo";

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
        public string Trg_C_Ref_Nme = typeof(Ref.CR1NCRef<Entity.CEntityObject, CEntityObject>).Namespace;
        public string Trg_P_AutoCreate_Nme = "AutoCreate";

        public string Trg_N_GetPrps_Nme = "_GetProperties";

        // Target Identifier Tokens (Target=.NET Framework, CbOrmMetaInfo)
        public string Trg_C_Mta_Pfx = "_";
        public string Trg_C_Mta_P_Fld_Nsp = typeof(Meta.CSkalarRefMetaInfo).Namespace;
        public string Trg_C_Mta_P_Fld_Nme = nameof(Meta.CSkalarRefMetaInfo);
        public string Trg_C_Mta_P_Rel_Sfx = "MetaInfo";
        public string Trg_C_Schema_M_GetSchmema = "GetSingleton";
        public string Trg_C_Fk_P_Sfx = "Guid";
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
                yield return this.Trg_C_Ref_Nme;
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

    }

 

    public class CGenModelInterpreter : CRflModelInterpreter
    {
        public readonly new CGenTokens Tok;
        public override CRflTokens Tokens => this.Tok;
        public CGenModelInterpreter(CGenTokens aTok) { this.Tok = aTok; }
        public CGenModelInterpreter() : this(new CGenTokens()) { }

        public virtual string GetSchema(CRflModel aMdl) => aMdl.GetPropertyAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Schema);
        public virtual string GetTypName(CRflTyp aRflClass) => aRflClass.Name;
        public virtual string GetBase(CRflTyp aRflClass) => aRflClass.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Base);
        public virtual string GetReturnTypName(CRflProperty aPrp) => aPrp.Name.Length == 0 ? string.Empty : aPrp.GetAttributeValue(this.Tok.MDl_O_A_Typ_Nme).DefaultIfEmpty(() => aPrp.Name);
        public virtual CRflTyp GetReturnTyp(CRflProperty aPrp) => aPrp.DeclaringTyp.Model.GetTypByName(GetReturnTypName(aPrp));
        public virtual bool GetIsEntityObject(CRflTyp aTyp) => GetBase(aTyp).Length > 0;
        public virtual string GetInit(CRflTyp aTyo) => aTyo.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Init);
        public virtual string GetInit(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Init).DefaultIfEmpty(() => this.GetInit(this.GetReturnTyp(aPrp)));
        public virtual IEnumerable<CRflAttribute> GetAttributes(CRflProperty aPrp) => aPrp.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomAGen);
        public virtual CCardinalityEnum GetCardinality(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Crd).DefaultIfEmpty(aCardinalityText => ("R" + aCardinalityText.Replace(":", string.Empty)).ParseEnum<CCardinalityEnum>(), () => this.GetIsEntityObject(this.GetReturnTyp(aPrp)) ? CCardinalityEnum.R1NC : CCardinalityEnum.Skalar);
        public virtual string GetClassName(CCardinalityEnum aCardinality) => this.Tok.Dom_C_Pfx + aCardinality.ToString() + this.Tok.Dom_C_Ref_Sfx;
        public virtual string GetEntityRefPropertyName(CRflProperty aPrp) => aPrp.Name + this.Tok.Dom_P_Ref_Sfx;
        public virtual IEnumerable<CRflTyp> GetEntityObjectClasses(CRflModel aMdl) => from aTest in aMdl.Typs where this.GetBase(aTest) == this.Tok.Trg_C_Eno_Nme select aTest;
        public virtual IEnumerable<string> GetClrNamespaces(CRflModel aMdl) => (from aTyp in aMdl.Typs
                                                                          where aTyp.Name.Length > 0
                                                                          select aTyp.GetPropertyAttributeValue(this.Tok.Mdl_T_A_Nme_ClrNs)).Where(aNs => aNs.Length > 0);
        public virtual Guid GetGuid(CRflTyp aTyp) => aTyp.GetAttributeValue(this.Tok.Mdl_T_A_Guid_Nme).DefaultIfEmpty<Guid>(s => new Guid(s), () => default(Guid));

        public virtual string GetR1NCForeignKeyPropertyName(CRflProperty aR1NPProperty) => aR1NPProperty.DeclaringTyp.Name + "_" + aR1NPProperty.Name + this.Tok.Trg_C_Fk_P_Sfx;
        internal IEnumerable<CRflRow> NewR1NCForeignKeyRows(CRflProperty aR1NPProperty)
        {
            var aR1NPTyp = aR1NPProperty.DeclaringTyp;
            var aR1NCTyp = this.GetReturnTyp(aR1NPProperty);
            var aPropertyName = this.GetR1NCForeignKeyPropertyName(aR1NPProperty);
            var aGeneratedComment = "Generated by " + nameof(CGenModelInterpreter) + "." + nameof(NewR1NCForeignKeyRows);
            var aFkPropertyRow = CRflRow.New(aR1NCTyp.Name, aPropertyName, this.Tok.MDl_O_A_Typ_Nme, nameof(System.Guid), string.Empty, aGeneratedComment);
            var aFkParentTypeAttributeType = typeof(CForeignKeyParentTypeAttribute).FullName;
            var aFkParentTypeAttributeValue = aR1NPTyp.Name;
            var aFkParentTypeAttributeRow = CRflRow.New(aR1NCTyp.Name, aPropertyName, this.Tok.Mdl_P_A_Nme_DomAExst + aFkParentTypeAttributeType, aFkParentTypeAttributeValue, string.Empty, aGeneratedComment);
            var aFkParentPropertyAttributeType = typeof(CForeignKeyParentPropertyNameAttribute).FullName;
            var aFkParentPropertyAttributeValue = aR1NPProperty.Name;
            var aFkParentPropertyAttributeRow = CRflRow.New(aR1NCTyp.Name, aPropertyName, this.Tok.Mdl_P_A_Nme_DomAExst + aFkParentPropertyAttributeType, aFkParentPropertyAttributeValue, string.Empty, aGeneratedComment);
            yield return aFkPropertyRow;
            yield return aFkParentTypeAttributeRow;
            yield return aFkParentPropertyAttributeRow;
        }
        public virtual string GetR11CForeignKeyPropertyName(CRflProperty aR11CPRoperty) => aR11CPRoperty.Name + this.Tok.Trg_C_Fk_P_Sfx;
        internal IEnumerable<CRflRow> NewR11CForeignKeyRows(CRflProperty aR11CProperty)                
        {
            var aR11PTyp = aR11CProperty.DeclaringTyp;
            var aR11CTyp = this.GetReturnTyp(aR11CProperty);
            var aPropertyName = this.GetR11CForeignKeyPropertyName(aR11CProperty);
            var aGeneratedComment = "Generated by " + nameof(CGenModelInterpreter) + "." + nameof(NewR11CForeignKeyRows);
            var aFkPropertyRow = CRflRow.New(aR11PTyp.Name, aPropertyName, this.Tok.MDl_O_A_Typ_Nme, nameof(System.Guid), string.Empty, aGeneratedComment);
            var aFkParentTypeAttributeType = typeof(CForeignKeyParentTypeAttribute).FullName;
            var aFkParentTypeAttributeValue = aR11PTyp.Name;
            var aFkParentTypeAttributeRow = CRflRow.New(aR11PTyp.Name, aPropertyName, this.Tok.Mdl_P_A_Nme_DomAExst + aFkParentTypeAttributeType, aFkParentTypeAttributeValue, string.Empty, aGeneratedComment);
            var aFkParentPropertyAttributeType = typeof(CForeignKeyParentPropertyNameAttribute).FullName;
            var aFkParentPropertyAttributeValue = aR11CProperty.Name;
            var aFkParentPropertyAttributeRow = CRflRow.New(aR11PTyp.Name, aPropertyName, this.Tok.Mdl_P_A_Nme_DomAExst + aFkParentPropertyAttributeType, aFkParentPropertyAttributeValue, string.Empty, aGeneratedComment);
            yield return aFkPropertyRow;
            yield return aFkParentTypeAttributeRow;
            yield return aFkParentPropertyAttributeRow;
        }

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
        public virtual CodeTypeReference NewCodeTypeRef(CRflTyp aTyp) => this.NewCodeTypeRefFromModel(this.Idl.GetTypName(aTyp));
        public virtual CodeExpression NewNameOfPrpertyExpression(string aPrpName) => new CodeSnippetExpression(this.Tok.Dom_O_NameOf_Nme + "(" + aPrpName + ")");
        public virtual CodeExpression NewNameOTypeExpression(string aTypName) => new CodeSnippetExpression(this.Tok.Dom_O_NameOf_Nme + "(" + aTypName + ")");

        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDeclsGen(CRflProperty aProperty)
            => from aAttribute in aProperty.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomAGen)
               select new CodeAttributeDeclaration(
                   new CodeTypeReference(this.Tok.GetClrAttributeClassNameFromModel(aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomAGen))),
                    new CodeAttributeArgument(new CodePrimitiveExpression(aAttribute.Value)));

        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDeclsExist(CRflProperty aProperty)
            => from aAttribute in aProperty.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomAExst)
               select new CodeAttributeDeclaration
               (
                   new CodeTypeReference(aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomAExst)),
                   System.Type.GetType(aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomAExst)).GetCustomAttribute<CAttributeValueTypeAttribute>().NewCodeAttributeArguments(aAttribute.Value).ToArray()
               );
        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDeclsExplicit(CRflProperty aProperty)
        {
            var aAttributeName = this.Tok.Trg_P_AutoCreate_Nme;
            var aAttributeValue = aProperty.GetAttributeValue(aAttributeName);
            if (aAttributeValue != string.Empty)
            {
                yield return new CodeAttributeDeclaration(new CodeTypeReference( typeof(CAutoCreateAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(aProperty.GetAttribute(aAttributeName).Interpret(()=> bool.Parse(aAttributeValue)))));
            }
        }

        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDecls(CRflProperty aProperty) =>
            this.NewCodeAttributeDeclsGen(aProperty).Concat(this.NewCodeAttributeDeclsExist(aProperty)).Concat(this.NewCodeAttributeDeclsExplicit(aProperty));

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
    internal sealed class CKeyExpander : CModelExpander
    {
        internal CKeyExpander(CGenModelInterpreter aGenModelInterpreter)
        {
            this.ModelInterpreter = aGenModelInterpreter;
        }
        private readonly CGenModelInterpreter ModelInterpreter;
        public override CRflModel Expand(CRflModel aModel)
        {
            var aModelInterpreter = this.ModelInterpreter;

            var aTyps = aModel.Typs;
            var aProperties = from aTyp in aTyps
                              from aProperty in aTyp.NamedProperties
                              select aProperty;
            var aR1NCProperties = from aProperty in aProperties
                                  where aModelInterpreter.GetCardinality(aProperty) == CCardinalityEnum.R1NC
                                  select aProperty;
            var aR1NCFkRows = (from aProperty in aR1NCProperties
                               from aRow in aModelInterpreter.NewR1NCForeignKeyRows(aProperty)
                               select aRow).ToArray()
                               ;
            var aR11CProperties = from aProperty in aProperties
                                  where aModelInterpreter.GetCardinality(aProperty) == CCardinalityEnum.R11C
                                  select aProperty;
            var aR11CFkRows = (from aProperty in aR11CProperties
                               from aRow in aModelInterpreter.NewR11CForeignKeyRows(aProperty)
                               select aRow);
            var aOrgRows = aModel.Rows;
            var aNewRows = aR1NCFkRows.Concat(aR11CFkRows).Concat(aOrgRows);
            var aOutModel = new CRflModel(aModelInterpreter, aNewRows);
            return aOutModel;
        }
    }



    public sealed class CCodeGenerator
    {
        public CChainedModelExpander Exp = new CChainedModelExpander();
        public CGenModelInterpreter ModelInterpreter;
        public CGenTokens Tok;
        public CCodeDomBuilder DomI;

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
            this.DomI = aDom;

            this.ModelInputFileInfo = aModelInputFileInfo;
            this.IdsInputFileInfoNullable = aIdsInputFileInfo;
            this.ModelOutputFileInfo = aModelOutputFileInfo;
            this.IdsOutputFileInfo = aIdsOutputFileInfo;

            this.Exp.ChainedExpanders.Add(new CKeyExpander(this.ModelInterpreter));
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
            var aPTypNme = this.ModelInterpreter.GetTypName(aPTyp);
            var aCdPTypRef = new CodeTypeReference(aPTypNme);
            var aCdPTypRefExp = new CodeTypeReferenceExpression(aCdPTypRef);
            var aCTyp = this.ModelInterpreter.GetReturnTyp(aProperty);
            var aPrpNme = aProperty.Name;
            string aMtaFldTypNme;
            var aMtaPrpNme = this.Tok.GetRelationyMetaInfoPropertyName(aPrpNme);
            var aMtaPrpRefExp = new CodePropertyReferenceExpression(aCdPTypRefExp, aMtaPrpNme);
            var aHasRelationObject = true; //this.ModelInterpreter.GetIsEntityObject(aCTyp)
            CodeMemberProperty aCdPrp;

            if (aHasRelationObject)
            {
                // Relations-Objects
                var aCrd = this.ModelInterpreter.GetCardinality(aProperty);
                var aRelTypName = this.ModelInterpreter.GetClassName(aCrd);
                var aCdRTypRef = new CodeTypeReference(aRelTypName, this.DomI.NewCodeTypeRef(aPTyp), this.DomI.NewCodeTypeRef(aCTyp));
                var aNewExp = new CodeObjectCreateExpression(aCdRTypRef, new CodeThisReferenceExpression(), aMtaPrpRefExp);
                var aIsClass = true;
                var aLazyPrp = this.DomI.NewLazyLoadPrperty(aCdRTypRef, aIsClass, this.ModelInterpreter.GetEntityRefPropertyName(aProperty), aNewExp, MemberAttributes.Public | MemberAttributes.Final);
                aCdTypDcl.Members.Add(aLazyPrp.Item1);
                aCdTypDcl.Members.Add(aLazyPrp.Item2);
                aCdPrp = aLazyPrp.Item1;
                aMtaFldTypNme = this.Tok.GeRelationMetaInfoTypName(aRelTypName);

            }
            else
            {
                // SkalarFields: "Simple" DataTypes (May be also struct on certain sql servers - support it?)
                var aPrpTypNme = this.ModelInterpreter.GetTypName(aCTyp);
                var aPrpTyp = new CodeTypeReference(aPrpTypNme);
                var aCdFldNme = this.Tok.GetFieldName(aPrpNme);
                var aInit = this.ModelInterpreter.GetInit(aProperty);
                var aCdFldRefExp = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), aCdFldNme);

                // MemberVar
                var aCdFld = new CodeMemberField(aPrpTyp, aCdFldNme);
                aCdFld.InitExpression = aInit.Length > 0 ? new CodeSnippetExpression(aInit) : default(CodeSnippetExpression);
                aCdTypDcl.Members.Add(aCdFld);

                // Property.Get
                aCdPrp = new CodeMemberProperty();
                aCdPrp.Name = aPrpNme;
                aCdPrp.HasGet = true;
                aCdPrp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                aCdPrp.GetStatements.Add(new CodeMethodReturnStatement(aCdFldRefExp));
                aCdPrp.Type = aPrpTyp;
                aCdTypDcl.Members.Add(aCdPrp);

                // Property.Set
                var aNewValExp = new CodeVariableReferenceExpression(this.ModelInterpreter.Tok.Dom_P_NewValNme);
                var aEqExp = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(object).Name),
                                                            nameof(object.Equals),
                                                            aCdFldRefExp,
                                                            aNewValExp
                                                            );
                var aAsgStm = new CodeAssignStatement(aCdFldRefExp, aNewValExp);
                var aIfExp = new CodeConditionStatement(aEqExp, aAsgStm);
                aCdPrp.SetStatements.Add(aIfExp);


                aMtaFldTypNme = this.Tok.Trg_C_Mta_P_Fld_Nme;

            }

            // Property.Attributes
            var aCdAs = this.DomI.NewCodeAttributeDecls(aProperty);
            aCdPrp.CustomAttributes.AddRange(aCdAs.ToArray());

            // Property.NetaInfo.Field
            var aMtaFldNme = this.Tok.GetRelationMetaInfoFieldName(aPrpNme);
            //aMtaFldTypNme = this.Tok.Trg_C_Mta_P_Fld_Nme;
            var aCdMtaFldTypRef = new CodeTypeReference(aMtaFldTypNme);
            var aCdMtaPrpFldDcl = new CodeMemberField(aMtaFldTypNme, aMtaFldNme);
            var aCdMtaPrpInitExp = new CodeObjectCreateExpression(aCdMtaFldTypRef,
                                                                  this.DomI.NewTypeOfExpression(aPTypNme),
                                                                  this.DomI.NewNameOfPrpertyExpression(aPrpNme)                                                                  
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

        private CodeTypeDeclaration NewCodeEntityObjectClass(CRflTyp aRflTyp)
        {
            var aModel = aRflTyp.Model;
            var aSchemaClsNme = this.Tok.GetSchemaClassName(this.ModelInterpreter.GetSchema(aModel));
            var aGetSchemaMthNme = this.Tok.GetGetSchemaFuncName();

            // Class
            var aClassName = this.ModelInterpreter.GetTypName(aRflTyp);
            var aCdTypDecl = new CodeTypeDeclaration(aClassName);
            var aCdTypRef = new CodeTypeReference(aClassName);
            var aCdTypRefExp = new CodeTypeReferenceExpression(aCdTypRef);
            aCdTypDecl.BaseTypes.Add(this.NewCodeTypeReference(this.Tok.Trg_C_Eno_Nme));

            var aGetPrpsMthdNme = this.Tok.Trg_N_GetPrps_Nme;
            var aGetPrpsMthActionArgNme = "aAddProperty";
            var aCdGetPrpsMthdRefExp = new CodeMethodReferenceExpression(aCdTypRefExp, aGetPrpsMthdNme);

            // Typ.Field
            var aTypTypRef = this.DomI.NewCodeTypeRef<CTyp>();
            var aTypFldNme = this.DomI.GetTypMetaInfoFieldName(aClassName);
            var aCdTypFldDcl = new CodeMemberField(aTypTypRef, aTypFldNme);
            var aCdTypeOfExp = this.DomI.NewTypeOfExpression(aClassName);
            var aCdNameOfExp = this.DomI.NewNameOTypeExpression(aClassName);
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
            var aTypPrpNme = this.DomI.GetTypMetaInfoPropName(aClassName);
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

        private void GenerateModelOutput()
        {
            var aMdl = this.Model;
            var aNamespace = new CodeNamespace();
            var aEnoClss = this.ModelInterpreter.GetEntityObjectClasses(aMdl);
            var aEnoTyps = (from aRflClass in aEnoClss select this.NewCodeEntityObjectClass(aRflClass)).ToArray();
            var aCompileUnit = new CodeCompileUnit();
            var aSchemaClass = this.NewSchemaClasses(aMdl);

            aNamespace.Name = this.ModelInterpreter.GetNamespace(aMdl);
            aCompileUnit.Namespaces.Add(aNamespace);
            aNamespace.Imports.AddRange((from aNs in this.Tok.Trg_Nsps.Concat(this.ModelInterpreter.GetClrNamespaces(aMdl)).NewWithoutDuplicates() select new CodeNamespaceImport(aNs)).ToArray());
            aNamespace.Types.AddRange(aEnoTyps);
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
            new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), CSchema.AddTyp_Name, this.DomI.NewTypMetaInfoFieldRefExp(this.ModelInterpreter.GetTypName(aTyp))));
        private CodeConstructor NewSchemaConstructor(CRflModel aMdl)
        {
            var aCtor = new CodeConstructor();
            aCtor.Statements.AddRange((from aEnoCls in this.ModelInterpreter.GetEntityObjectClasses(aMdl) select this.NewAddPrototypeStatement(aEnoCls)).ToArray());
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
            aCdGetSchemaMth.ReturnType = this.DomI.NewCodeTypeRef<CSchema>();
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
                aCdSchemaClassDecl.BaseTypes.Add(this.DomI.NewCodeTypeRef<CSchema>());
                aCdSchemaClassDecl.Members.AddRange(this.NewSchemaMembers(aModel, aSchemaClsNme).ToArray());
                yield return aCdSchemaClassDecl;
            }
        }

        private void LoadInput() => this.Model = this.Exp.Expand(new CRflModel(this.ModelInterpreter, CRflRow.NewFromTextFile(this.ModelInputFileInfo).Concat(this.IdsInputFileInfoNullable.IsNullRef()  ? new CRflRow[] { } : CRflRow.NewFromTextFile(this.IdsInputFileInfoNullable)).ToArray()));
    }
}
