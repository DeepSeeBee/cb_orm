// This is my 2nd generation ORM-Wrapper. (code generator)
// - Clone
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.CodeDom;
using CbOrm.Orm;
using System.CodeDom.Compiler;
using CbOrm.Mdl;
using System.Collections;
using CbOrm.Mta;
using CbOrm.Eno;
using CbOrm.Ut;
using CbOrm.sch;

namespace CbOrm.Gen
{
    using CLazyLoadPrp = Tuple<CodeMemberProperty, CodeMemberField>;

    public enum CCardinalityEnum
    {
        R11C,
        R11P,
        R1NC,
        R1NP,
        R1NW,
        R11W,
    }

    public class CIdentifierTokens
    {
        // ModelDefinitionLange Identifier tokens
        public string Mdl_G_A_Namespace = "Namespace";
        public string Mdl_G_A_Schema = "Schema";

        public string Mdl_P_A_Nme_Init = "init";
        public string Mdl_P_A_Nme_Crd = "Cardinality";
        public string Mdl_P_A_Nme_DomA = "a.";

        public string Mdl_T_A_Nme_Base = "Base";
        public string Mdl_T_A_Nme_Init = "Init";
        public string Mdl_T_A_Nme_ClrNs = "ClrNamespace";

        // CodeDOM Identifier tokens
        public string Dom_A_Cls_Sfx = "Attribute";

        public string Dom_C_Pfx = "C";        
        public string Dom_C_Ref_Sfx = "Ref";
        public string Dom_C_Sch_Sfx = "Schema";
        public string Dom_C_Var = "var";

        public string Dom_F_Sfx = "M";
        public string Dom_F_MtaP_Sfx = "Prop";

        public string Dom_O_NameOf_Nme = "nameof";
        public string Dom_O_TypeOf_Nme = "typeof";

        public string Dom_P_NewValNme = "value";
        public string Dom_P_Ref_Sfx = "Ref";

        public string Dom_Tmpl_IEnumerable = nameof(IEnumerable);

        // Target identfiier Tokens (Target=.NET Framework)
        public string Trg_C_Obj_Nsp = typeof(Eno.CObject).Namespace;
        public string Trg_C_Obj_Nme = nameof(Eno.CObject);
        public string Trg_C_Eno_Nsp = typeof(Eno.CEntityObject).Namespace;
        public string Trg_C_Eno_Nme = nameof(Eno.CEntityObject);
        public string Trg_C_Str_Nsp = typeof(Str.CStorage).Namespace;
        public string Trg_C_Str_Nme = nameof(Str.CStorage);
        public string Trg_C_Ref_Nme = typeof(Ref.CR1NCRef<Eno.CEntityObject, Eno.CObject>).Namespace;

        public string Trg_N_GetPrps_Nme = "GetProperties";

        // Target Identoifier Tokens (Target=.NET Framework, CbOrmMetaInfo)
        public string Trg_C_Mta_Pfx = "_";
        public string Trg_C_Mta_P_Fld_Nsp = typeof(Mta.CFieldProperty).Namespace;
        public string Trg_C_Mta_P_Fld_Nme = nameof(Mta.CFieldProperty);

        public virtual IEnumerable<Type> NativeTypes
        {
            get
            {
                //yield return typeof(object);
                //yield return typeof(string);
                //yield return typeof(bool);
                //yield return typeof(Int32);
                yield return typeof(Rfl.CSaveConverterAttribute);
                yield return typeof(IEnumerable<object>).GetGenericTypeDefinition();
            }
        }
        public IEnumerable<string> Trg_Nsps
        {
            get
            {
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
        public virtual string GetMemberFieldName(string aPrpNme) => aPrpNme + this.Dom_F_Sfx;
        public virtual string GetMetaPropertyPropertyName(string aPrpNme) => this.Trg_C_Mta_Pfx + aPrpNme + this.Dom_F_MtaP_Sfx;
        internal string GetSchemaClassName(string aSchNme) => aSchNme + this.Dom_C_Sch_Sfx;
    }

 

    public class CMdlInterpreter
    {
        public CIdentifierTokens Tok;
        public CMdlInterpreter(CIdentifierTokens aTok) { this.Tok = aTok; }
        public CMdlInterpreter() : this(new CIdentifierTokens()) { }

        public virtual string GetNamespace(CRflModel aMdl) => aMdl.GetPropertyAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Namespace);
        public virtual string GetSchema(CRflModel aMdl) => aMdl.GetPropertyAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Schema);
        public virtual string GetTypName(CRflTyp aRflClass) => aRflClass.Name;
        public virtual string GetBase(CRflTyp aRflClass) => aRflClass.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Base);
        public virtual string GetReturnTypName(CRflProperty aPrp) => aPrp.Name.Length == 0 ? string.Empty : aPrp.GetAttributeValue("Typ").DefaultIfEmpty(() => aPrp.Name);
        public virtual CRflTyp GetReturnTyp(CRflProperty aPrp) => aPrp.Typ.Model.GetTypByName(GetReturnTypName(aPrp));
        public virtual bool GetIsEntityObject(CRflTyp aTyp) => GetBase(aTyp).Length > 0;
        public virtual string GetInit(CRflTyp aTyo) => aTyo.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Init);
        public virtual string GetInit(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Init).DefaultIfEmpty(() => this.GetInit(this.GetReturnTyp(aPrp)));
        public virtual IEnumerable<CRflAttribute> GetAttributes(CRflProperty aPrp) => aPrp.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomA);
        public virtual CCardinalityEnum GetCardinality(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Crd).DefaultIfEmpty(aCardinalityText => ("R" + aCardinalityText.Replace(":", string.Empty)).ParseEnum<CCardinalityEnum>(), () => CCardinalityEnum.R1NC);
        public virtual string GetClassName(CCardinalityEnum aCardinality) => this.Tok.Dom_C_Pfx + aCardinality.ToString() + this.Tok.Dom_C_Ref_Sfx;
        public virtual string GetEntityRefPropertyName(CRflProperty aPrp) => aPrp.Name + this.Tok.Dom_P_Ref_Sfx;
        public IEnumerable<CRflTyp> GetEntityObjectClasses(CRflModel aMdl) => from aTest in aMdl.Typs where this.GetBase(aTest) == this.Tok.Trg_C_Eno_Nme select aTest;
        internal IEnumerable<string> GetClrNamespaces(CRflModel aMdl) => (from aTyp in aMdl.Typs
                                                                          where aTyp.Name.Length > 0
                                                                          select aTyp.GetPropertyAttributeValue(this.Tok.Mdl_T_A_Nme_ClrNs)).Where(aNs => aNs.Length > 0);
    }

    public class CCodeDomBuilder
    {
        public CCodeDomBuilder(CMdlInterpreter aIdl, CIdentifierTokens aTok)
        {
            this.Idl = aIdl;
            this.Tok = aTok;
        }
        public CCodeDomBuilder() : this(new CMdlInterpreter(), new CIdentifierTokens()) { }

        public CMdlInterpreter Idl;
        public CIdentifierTokens Tok;

        public virtual CodeTypeReference NewCodeTypeRefFromModel(string aName) => new CodeTypeReference(aName);
        public virtual CodeTypeReference NewCodeTypeRef(CRflTyp aTyp) => this.NewCodeTypeRefFromModel(this.Idl.GetTypName(aTyp));
        public virtual CodeExpression NewNameOfPrpertyExpression(string aPrpName) => new CodeSnippetExpression(this.Tok.Dom_O_NameOf_Nme + "(" + aPrpName + ")");
        public virtual IEnumerable<CodeAttributeDeclaration> NewCodeAttributeDecls(CRflProperty aPrp)
            => from aAttribute in aPrp.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomA)
               select new CodeAttributeDeclaration(
                   new CodeTypeReference(this.Tok.GetClrAttributeClassNameFromModel(aAttribute.Name.TrimStart(this.Tok.Mdl_P_A_Nme_DomA))),
                    new CodeAttributeArgument(new CodePrimitiveExpression(aAttribute.Value)));

        public virtual Tuple<CodeMemberProperty, CodeMemberField> NewLazyLoadPrperty(CodeTypeReference aCdRetTyp,
                                                                                      bool aIsClass,
                                                                       string aName,
                                                                       CodeExpression aLdExp,
                                                                       MemberAttributes aAttributes
                                                                       )
        {
            // MemberField
            var aCdFldNme = this.Tok.GetMemberFieldName(aName);
            var aCdFld = new CodeMemberField(aCdRetTyp, aCdFldNme);
            var aCdFldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), aCdFldNme);
            var aEqMNme = aIsClass ? nameof(object.ReferenceEquals) : nameof(object.Equals);
            var aLdCond = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(object).Name),
                                                      aEqMNme,
                                                      aCdFldRef,
                                                      new CodePrimitiveExpression(null)
                                                      );
            var aLdStm = new CodeAssignStatement(aCdFldRef, aLdExp);
            var aLdIFStm = new CodeConditionStatement(aLdCond, new CodeStatement[] { }, new CodeStatement[] { aLdStm });
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

        internal CodeExpression NewTypeOfExpression(object aNme) => new CodeSnippetExpression(this.Tok.Dom_O_TypeOf_Nme + "(" + aNme + ")");
        internal string NewYieldReturnStatementSnippet(string aItem) => "yield return " + aItem + ";";
        internal IEnumerable<CodeStatement> NewYieldReturnBaseItemsStatements(string aMethodName)
        {
            yield return new CodeSnippetStatement("foreach(var aItem in base." + aMethodName + "()) ");
            yield return new CodeSnippetStatement(this.NewYieldReturnStatementSnippet("aItem"));
        }

        internal virtual CodeTypeReference NewCodeTypeRef<T>() => new CodeTypeReference(typeof(T));
    } 

    public class CModelExpander
    {
        public virtual CRflModel Expand(CRflModel aMdl) => aMdl;
    }

    public sealed class NewIdsModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel aMdl)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CNewCrossRefModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel aMdl)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CCodeGenerator
    {
        public CModelExpander Exp;
        public CMdlInterpreter MdlI;
        public CIdentifierTokens Tok;
        public CCodeDomBuilder DomI;

        public CCodeGenerator(FileInfo aModelInputFileInfo,
                                FileInfo aIdsInputFileInfo,
                                FileInfo aModelOutputFileInfo,
                                FileInfo aIdsOutputFileInfo)
            : this(new CModelExpander(),
                   new CMdlInterpreter(),
                   new CIdentifierTokens(),
                   new CCodeDomBuilder(),
                   aModelInputFileInfo,
                   aIdsInputFileInfo,
                   aModelOutputFileInfo,
                   aIdsOutputFileInfo)
        {
        }

        public CCodeGenerator(CModelExpander aExp,
                              CMdlInterpreter aMdlInterpreter,
                              CIdentifierTokens aTok,
                              CCodeDomBuilder aDom,
                              FileInfo aModelInputFileInfo,
                              FileInfo aIdsInputFileInfo,
                              FileInfo aModelOutputFileInfo,
                              FileInfo aIdsOutputFileInfo)
        {
            this.Exp = aExp;
            this.MdlI = aMdlInterpreter;
            this.Tok = aTok;
            this.DomI = aDom;

            this.ModelInputFileInfo = aModelInputFileInfo;
            this.IdsInputFileInfo = aIdsInputFileInfo;
            this.ModelOutputFileInfo = aModelOutputFileInfo;
            this.IdsOutputFileInfo = aIdsOutputFileInfo;
        }

        public readonly FileInfo ModelInputFileInfo;
        public readonly FileInfo IdsInputFileInfo;
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


        private void AddMembers(CRflTyp aPTyp, CodeTypeDeclaration aCdTypDcl, CRflProperty aPrp)
        {
            var aPTypNme = this.MdlI.GetTypName(aPTyp);
            var aCTyp = this.MdlI.GetReturnTyp(aPrp);
            var aPrpNme = aPrp.Name;
            if (this.MdlI.GetIsEntityObject(aCTyp))
            {
                var aCrd = this.MdlI.GetCardinality(aPrp);
                var aRTypName = this.MdlI.GetClassName(aCrd);
                var aCdRTypRef = new CodeTypeReference(aRTypName, this.DomI.NewCodeTypeRef(aPTyp), this.DomI.NewCodeTypeRef(aCTyp));
                var aNewExp = new CodeObjectCreateExpression(aCdRTypRef);
                var aIsClass = true;
                var aLazyPrp = this.DomI.NewLazyLoadPrperty(aCdRTypRef, aIsClass, this.MdlI.GetEntityRefPropertyName(aPrp), aNewExp, MemberAttributes.Public | MemberAttributes.Final);
                aCdTypDcl.Members.Add(aLazyPrp.Item1);
                aCdTypDcl.Members.Add(aLazyPrp.Item2);
            }
            else
            {
                var aPrpTypNme = this.MdlI.GetTypName(aCTyp);
                var aPrpTyp = new CodeTypeReference(aPrpTypNme);
                var aCdFldNme = this.Tok.GetMemberFieldName(aPrpNme);
                var aInit = this.MdlI.GetInit(aPrp);
                var aCdFldRefExp = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), aCdFldNme);

                // MemberVar
                var aCdFld = new CodeMemberField(aPrpTyp, aCdFldNme);
                aCdFld.InitExpression = aInit.Length > 0 ? new CodeSnippetExpression(aInit) : default(CodeSnippetExpression);
                aCdTypDcl.Members.Add(aCdFld);

                // Property.Get
                var aCdPrp = new CodeMemberProperty();
                aCdPrp.Name = aPrpNme;
                aCdPrp.HasGet = true;
                aCdPrp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                aCdPrp.GetStatements.Add(new CodeMethodReturnStatement(aCdFldRefExp));
                aCdPrp.Type = aPrpTyp;
                aCdTypDcl.Members.Add(aCdPrp);

                // Property.Set
                var aNewValExp = new CodeVariableReferenceExpression(this.MdlI.Tok.Dom_P_NewValNme);
                var aEqExp = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(object).Name),
                                                            nameof(object.Equals),
                                                            aCdFldRefExp,
                                                            aNewValExp
                                                            );
                var aAsgStm = new CodeAssignStatement(aCdFldRefExp, aNewValExp);
                var aIfExp = new CodeConditionStatement(aEqExp, aAsgStm);
                aCdPrp.SetStatements.Add(aIfExp);

                // Property.Attributes
                var aCdAs = this.DomI.NewCodeAttributeDecls(aPrp);
                aCdPrp.CustomAttributes.AddRange(aCdAs.ToArray());

                // CbOrm-NetaInfo
                var aMtaFldNme = this.Tok.GetMetaPropertyPropertyName(aPrpNme);
                var aMtaFldTypNme = this.Tok.Trg_C_Mta_P_Fld_Nme;
                var aCdMtaFldTypRef = new CodeTypeReference(aMtaFldTypNme);
                var aCdMtaPrpFldDcl = new CodeMemberField(aMtaFldTypNme, aMtaFldNme);
                var aCdMtaPrpInitExp = new CodeObjectCreateExpression(aCdMtaFldTypRef,
                                                                      this.DomI.NewTypeOfExpression(aPTypNme),
                                                                      this.DomI.NewTypeOfExpression(aPrpTypNme),
                                                                      this.DomI.NewNameOfPrpertyExpression(aPrpNme));
                aCdMtaPrpFldDcl.Type = aCdMtaFldTypRef;
                aCdMtaPrpFldDcl.Attributes = MemberAttributes.Static
                                         | MemberAttributes.Const
                                         | MemberAttributes.Public
                                         ;
                aCdMtaPrpFldDcl.InitExpression = aCdMtaPrpInitExp;
                aCdTypDcl.Members.Add(aCdMtaPrpFldDcl);
            }
        }

        private CodeTypeDeclaration NewEntityObjectClass(CRflTyp aRflTyp)
        {
            var aClassName = this.MdlI.GetTypName(aRflTyp);
            var aCdTypDecl = new CodeTypeDeclaration(aClassName);
            aCdTypDecl.BaseTypes.Add(this.NewCodeTypeReference(this.Tok.Trg_C_Eno_Nme));
            var aCodeConstructor = new CodeConstructor();
            aCodeConstructor.Attributes = MemberAttributes.Public;
            var aStorageArgName = "aStorage";
            aCodeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(this.NewCodeTypeReference(this.Tok.Trg_C_Str_Nme), aStorageArgName));
            aCodeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(aStorageArgName));
            aCdTypDecl.Members.Add(aCodeConstructor);
            var aNmePrps = aRflTyp.NamedProperties;
            foreach (var aPrp in aNmePrps)
            {
                this.AddMembers(aRflTyp, aCdTypDecl, aPrp);
            }

            // CObject.GetProperties
            var aCdGetPrpsMth = new CodeMemberMethod();
            aCdGetPrpsMth.Name = this.Tok.Trg_N_GetPrps_Nme;
            aCdGetPrpsMth.ReturnType = new CodeTypeReference(this.Tok.Dom_Tmpl_IEnumerable, this.DomI.NewCodeTypeRef<CProperty>());
            aCdGetPrpsMth.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            aCdGetPrpsMth.Statements.AddRange(this.DomI.NewYieldReturnBaseItemsStatements(nameof(CEntityObject.GetProperties)).ToArray());
            aCdGetPrpsMth.Statements.AddRange((from aPrp in aNmePrps where !this.MdlI.GetIsEntityObject(this.MdlI.GetReturnTyp(aPrp)) select new CodeSnippetStatement(this.DomI.NewYieldReturnStatementSnippet("this." + this.Tok.GetMetaPropertyPropertyName(aPrp.Name)))).ToArray());
            aCdTypDecl.Members.Add(aCdGetPrpsMth);
            return aCdTypDecl;
        }

        private void GenerateModelOutput()
        {
            var aMdl = this.Model;
            var aNamespace = new CodeNamespace();
            var aEnoClss = this.MdlI.GetEntityObjectClasses(aMdl);
            var aEnoTyps = (from aRflClass in aEnoClss select this.NewEntityObjectClass(aRflClass)).ToArray();
            var aCompileUnit = new CodeCompileUnit();
            var aSchemaClass = this.NewSchemaClass(aMdl);

            aNamespace.Name = this.MdlI.GetNamespace(aMdl);
            aCompileUnit.Namespaces.Add(aNamespace);
            aNamespace.Imports.AddRange((from aNs in this.Tok.Trg_Nsps.Concat(this.MdlI.GetClrNamespaces(aMdl)).NewWithoutDuplicates() select new CodeNamespaceImport(aNs)).ToArray());
            aNamespace.Types.AddRange(aEnoTyps);
            aNamespace.Types.Add(aSchemaClass);

            var aCodeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            var aMemoryStream = new MemoryStream();
            var aStreamWriter = new StreamWriter(aMemoryStream);
            var aCodeGeneratorOptions = new CodeGeneratorOptions();
            aCodeGeneratorOptions.BracingStyle = "C";
            aCodeDomProvider.GenerateCodeFromCompileUnit(aCompileUnit, aStreamWriter, aCodeGeneratorOptions);
            aStreamWriter.Flush();
            aMemoryStream.Position = 0;
            using (var aFileStream = this.ModelOutputFileInfo.OpenWrite())
            {
                aMemoryStream.CopyTo(aFileStream);
            }

            //throw new NotImplementedException();
        }
        private CodeStatement NewAddPrototypeStatement(CRflTyp aTyp) => new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), CSchema.AddPrototype_Name, new CodeObjectCreateExpression(this.DomI.NewCodeTypeRef(aTyp), new CodePrimitiveExpression(null))));
        private CodeConstructor NewSchemaConstructor(CRflModel aMdl)
        { //// TODO: Why does the syntax 15f7953c-1584-41de-90d4-827c062ca7a1 not work on CodeConstructor.Statements ? We wanna go functional...
            var aCtor = new CodeConstructor();
            aCtor.Statements.AddRange((from aEnoCls in this.MdlI.GetEntityObjectClasses(aMdl) select this.NewAddPrototypeStatement(aEnoCls)).ToArray());
            return aCtor;
        }
        private CodeTypeDeclaration NewSchemaClass(CRflModel aMdl)
            => new CodeTypeDeclaration
            {
                Name = this.Tok.GetSchemaClassName(this.MdlI.GetSchema(aMdl)),
                BaseTypes = { this.DomI.NewCodeTypeRef<CSchema>() },
                Members = { this.NewSchemaConstructor(aMdl) } // 15f7953c-1584-41de-90d4-827c062ca7a1
            };

        private void LoadInput() => this.Model = this.Exp.Expand(new CRflModel(CRflRow.NewFromTextFile(this.ModelInputFileInfo).Concat(CRflRow.NewFromTextFile(this.IdsInputFileInfo)).ToArray()));
    }
}
