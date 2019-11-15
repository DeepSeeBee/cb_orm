// This is my 2nd generation ORM-Wrapper. (code generator)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.CodeDom;
using CbOrm.Orm;
using System.CodeDom.Compiler;
using CbOrm.Mdl;

namespace CbOrm.Gen
{
    using CLazyLoadPrp = Tuple<CodeMemberProperty, CodeMemberField>;

    public enum CCardinalityEnum
    {
        R11C,
        R11P,
        R1NC,
        R1NP
    }

    public class CIdentifierTokens
    {        
        // ModelDefinitionLange Identifier tokens
        public string Mdl_T_A_Nme_Base = "Base";
        public string Mdl_T_A_Nme_Init = "Init";               
        public string Mdl_P_A_Nme_Init = "init";
        public string Mdl_P_A_Nme_Crd = "Cardinality";
        public string Mdl_P_A_Nme_DomA = "a.";

        // CodeDOM Identifier tokens
        public string Dom_A_Cls_Sfx = "Attribute";
        public string Dom_O_NameOf_Nme = "nameof";
        public string Dom_P_NewValNme = "value";
        public string Dom_C_Pfx = "C";
        public string Dom_F_Sfx = "M";
        public string Dom_P_Ref_Sfx = "Ref";
        public string Dom_C_Ref_Sfx = "Ref";

        // Target identfiier Tokens (Target=.NET Framework)
        public string Trg_C_Obj_Nsp = typeof(Eno.CObject).Namespace;
        public string Trg_C_Obj_Nme = typeof(Eno.CObject).Name;
        public string Trg_C_Eno_Nsp = typeof(Eno.CEntityObject).Namespace;
        public string Trg_C_Eno_Nme = typeof(Eno.CEntityObject).Name;
        public string Trg_C_Str_Nsp = typeof(Str.CStorage).Namespace;
        public string Trg_C_Str_Nme = typeof(Str.CStorage).Name;

        public virtual IEnumerable<Type> NativeTypes
        {
            get
            {
                yield return typeof(object);
                yield return typeof(string);
                yield return typeof(bool);
                yield return typeof(Int32);
                yield return typeof(Rfl.CSaveConverterAttribute);
            }
        }


        private IEnumerable<string> Trg_Nsps0
        {
            get
            {
                yield return this.Trg_C_Obj_Nsp;
                yield return this.Trg_C_Eno_Nsp;
                yield return this.Trg_C_Str_Nsp;
                foreach (var aNatTyp in this.NativeTypes)
                    yield return aNatTyp.Namespace;                
            }
        }
        public IEnumerable<string> Trg_Nsps { get => this.Trg_Nsps0.NewWithoutDuplicates(); }

        public virtual string GetClrAttributeClassNameFromModel(string aModelClassName) => this.Dom_C_Pfx
                                                                                                + aModelClassName
                                                                                                + this.Dom_A_Cls_Sfx;


    }

    public static class CExtensions
    {
        public static T DefaultIfEmpty<T>(this string aValue, Func<string, T> aGetValue, Func<T> aDefault) => aValue.Length > 0 ? aGetValue(aValue) : aDefault();
        public static string DefaultIfEmpty(this string aValue, Func<string> aDefault) => aValue.DefaultIfEmpty(aVal => aVal, aDefault);

        public static IEnumerable<T> NewWithoutDuplicates<T>(this IEnumerable<T> aItems)
        { //// TODO
            var aDic = new Dictionary<T, object>();
            foreach (var aItem in aItems)
                if (!aDic.ContainsKey(aItem))
                    aDic.Add(aItem, default(object));
            return aDic.Keys.ToArray();
        }
    }

    public class CIdlInterpreter     
    {
        public CIdentifierTokens Tok;
        public CIdlInterpreter(CIdentifierTokens aTok) { this.Tok = aTok; }
        public CIdlInterpreter():this(new CIdentifierTokens()) { }
        public virtual string GetTypName(CRflTyp aRflClass) => aRflClass.Name;
        public virtual string GetBase(CRflTyp aRflClass) => aRflClass.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Base);
        public virtual string GetReturnTypName(CRflProperty aPrp) => aPrp.Name.Length == 0 ? string.Empty : aPrp.GetAttributeValue("Typ").DefaultIfEmpty(() => aPrp.Name);
        public virtual CRflTyp GetReturnTyp(CRflProperty aPrp) => aPrp.Typ.Model.GetTypByName(GetReturnTypName(aPrp));
        public virtual bool GetIsEntityObject(CRflTyp aTyp) => GetBase(aTyp).Length > 0;
        public virtual string GetInit(CRflTyp aTyo) => aTyo.GetAttributeValue(this.Tok.Mdl_T_A_Nme_Init);
        public virtual string GetInit(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Init).DefaultIfEmpty(() => this.GetInit(this.GetReturnTyp(aPrp)));
        public virtual IEnumerable<CRflAttribute> GetAttributes(CRflProperty aPrp) => aPrp.GetAttributesWithPrefix(this.Tok.Mdl_P_A_Nme_DomA);        
        public virtual string GetClrClassNameFromModel(string aModelClassName) => this.Tok.Dom_C_Pfx + aModelClassName;
        public virtual CCardinalityEnum GetCardinality(CRflProperty aPrp) => aPrp.GetAttributeValue(this.Tok.Mdl_P_A_Nme_Crd).DefaultIfEmpty(aCardinalityText => ("R" + aCardinalityText.Replace(":", string.Empty)) .ParseEnum<CCardinalityEnum>(), ()=>CCardinalityEnum.R1NC);
        public virtual string GetClassName(CCardinalityEnum aCardinality) => this.Tok.Dom_C_Pfx + aCardinality.ToString() + this.Tok.Dom_C_Ref_Sfx;
        public virtual string GetMemberFieldName(string aPrpName) => aPrpName + this.Tok.Dom_F_Sfx;
        public virtual string GetEntityRefPropertyName(CRflProperty aPrp) => aPrp.Name + this.Tok.Dom_P_Ref_Sfx;
    }

    public class CCodeDomBuilder
    {
        public CCodeDomBuilder(CIdlInterpreter aIdl, CIdentifierTokens aTok)
        {
            this.Idl = aIdl;
            this.Tok = aTok;
        }
        public CCodeDomBuilder() : this(new CIdlInterpreter(), new CIdentifierTokens()) { }

        public CIdlInterpreter Idl;
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
                                                                       string aName,
                                                                       CodeExpression aLdExp,
                                                                       MemberAttributes aAttributes
                                                                       )
        {
            // MemberField
            var aCdFldNme = this.Idl.GetMemberFieldName(aName);
            var aCdFld = new CodeMemberField(aCdRetTyp, aCdFldNme);
            var aCdFldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), aCdFldNme);
            var aLdCond = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(object).Name),
                                                      nameof(object.Equals),
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

    }

    public static class CExceptionExtensions
    {
        public static T Throw<T>(this Exception aExc) => throw aExc;
    }

    public static class CStringExtensions
    {
        public static T ParseEnum<T>(this string aEnumText) => (T)Enum.Parse(typeof(T), aEnumText);
        public static string TrimStart(this string aText, string aTrim) => aText.StartsWith(aTrim) ? aText.Substring(aTrim.Length, aText.Length - aTrim.Length) : new ArgumentException().Throw<string>();
    }

    public class CModelExpander
    {
        public virtual CRflModel Expand(CRflModel aMdl) => aMdl;
    }

    public sealed class CCodeGenerator
    {
        public CModelExpander Exp;
        public CIdlInterpreter Idl;
        public CIdentifierTokens Tok;
        public CCodeDomBuilder Dom;        

        public CCodeGenerator(FileInfo aModelInputFileInfo,
                                FileInfo aIdsInputFileInfo,
                                FileInfo aModelOutputFileInfo,
                                FileInfo aIdsOutputFileInfo)
            : this(new CModelExpander(), 
                   new CIdlInterpreter(),
                   new CIdentifierTokens(),
                   new CCodeDomBuilder(),                   
                   aModelInputFileInfo,
                   aIdsInputFileInfo,
                   aModelOutputFileInfo,
                   aIdsOutputFileInfo)
        {
        }

        public CCodeGenerator(CModelExpander aExp, 
                              CIdlInterpreter aIdlInterpreter,
                              CIdentifierTokens aTok,
                              CCodeDomBuilder aDom,  
                              FileInfo aModelInputFileInfo,
                              FileInfo aIdsInputFileInfo,
                              FileInfo aModelOutputFileInfo,
                              FileInfo aIdsOutputFileInfo)
        {
            this.Exp = aExp;
            this.Idl = aIdlInterpreter;
            this.Tok = aTok;
            this.Dom = aDom;
            
            this.ModelInputFileInfo = aModelInputFileInfo;
            this.IdsInputFileInfo = aIdsInputFileInfo;
            this.ModelOutputFileInfo = aModelOutputFileInfo;
            this.IdsOutputFileInfo = aIdsOutputFileInfo;
        }

        public readonly FileInfo ModelInputFileInfo;
        public readonly FileInfo IdsInputFileInfo;
        public readonly FileInfo ModelOutputFileInfo;
        public readonly FileInfo IdsOutputFileInfo;

        private CRflModel RflModel;
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


        private void AddMembers(CRflTyp aRflTyp, CodeTypeDeclaration aCdTypDcl, CRflProperty aPrp)
        {
            var aCTyp = this.Idl.GetReturnTyp(aPrp);
            var aPrpName = aPrp.Name;
            if (this.Idl.GetIsEntityObject(aCTyp))
            {
                var aCrd = this.Idl.GetCardinality(aPrp);
                var aRTypName = this.Idl.GetClassName(aCrd);
                var aCdRTypRef = new CodeTypeReference(aRTypName, this.Dom.NewCodeTypeRef(aRflTyp), this.Dom.NewCodeTypeRef(aCTyp));
                var aNewExp = new CodeObjectCreateExpression(aCdRTypRef);
                var aLazyPrp = this.Dom.NewLazyLoadPrperty(aCdRTypRef, this.Idl.GetEntityRefPropertyName(aPrp), aNewExp, MemberAttributes.Public | MemberAttributes.Final);
                aCdTypDcl.Members.Add(aLazyPrp.Item1);
                aCdTypDcl.Members.Add(aLazyPrp.Item2);
            }
            else
            {
                var aPrpType = new CodeTypeReference(this.Idl.GetTypName(aCTyp));
                var aCdFldNme = this.Idl.GetMemberFieldName(aPrpName);
                var aInit = this.Idl.GetInit(aPrp);
                var aCdFldRefExp = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), aCdFldNme);

                // MemberVar
                var aCdFld = new CodeMemberField(aPrpType, aCdFldNme);
                aCdFld.InitExpression = aInit.Length > 0 ? new CodeSnippetExpression(aInit) : default(CodeSnippetExpression);
                aCdTypDcl.Members.Add(aCdFld);

                // Prperty.Get
                var aCdPrp = new CodeMemberProperty();
                aCdPrp.Name = aPrpName;
                aCdPrp.HasGet = true;
                aCdPrp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                aCdPrp.GetStatements.Add(new CodeMethodReturnStatement(aCdFldRefExp));
                aCdPrp.Type = aPrpType;
                aCdTypDcl.Members.Add(aCdPrp);

                // Prperty.Set
                var aNewValExp = new CodeVariableReferenceExpression(this.Idl.Tok.Dom_P_NewValNme);
                var aEqExp = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(object).Name),
                                                            nameof(object.Equals),
                                                            aCdFldRefExp,
                                                            aNewValExp
                                                            );
                var aAsgStm = new CodeAssignStatement(aCdFldRefExp, aNewValExp);
                var aIfExp = new CodeConditionStatement(aEqExp, aAsgStm);
                aCdPrp.SetStatements.Add(aIfExp);

                // Attributes
                var aCdAs = this.Dom.NewCodeAttributeDecls(aPrp);
                aCdPrp.CustomAttributes.AddRange(aCdAs.ToArray());
            }

        }
        private CodeTypeDeclaration NewEntityObjectClass(CRflTyp aRflTyp)
        {
            var aClassName = this.Idl.GetTypName(aRflTyp);
            var aCodeTypeDeclaration = new CodeTypeDeclaration(aClassName);
            aCodeTypeDeclaration.BaseTypes.Add(this.NewCodeTypeReference(this.Tok.Trg_C_Eno_Nme));
            var aCodeConstructor = new CodeConstructor();
            aCodeConstructor.Attributes = MemberAttributes.Public;
            var aStorageArgName = "aStorage";
            aCodeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(this.NewCodeTypeReference(this.Tok.Trg_C_Str_Nme), aStorageArgName));
            aCodeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(aStorageArgName));
            aCodeTypeDeclaration.Members.Add(aCodeConstructor);
            var aPrps = from aPrp in aRflTyp.Properties
                        where aPrp.Name.Length > 0
                        select aPrp;
            foreach (var aPrp in aPrps)
            {
                this.AddMembers(aRflTyp, aCodeTypeDeclaration, aPrp);
            }
            return aCodeTypeDeclaration;
        }

        private void GenerateModelOutput()
        {
            var aRflModel = this.RflModel;
            var aEntityObjectClasses = from aTest in aRflModel.Classes
                                       where this.Idl.GetBase(aTest) == this.Tok.Trg_C_Eno_Nme
                                       select aTest
                                       ;
            var aClasses = (from aRflClass in aEntityObjectClasses select this.NewEntityObjectClass(aRflClass)).ToArray();
            var aCompileUnit = new CodeCompileUnit();
            var aNamespace = new CodeNamespace(typeof(Ref.CR11CRef<Eno.CEntityObject, Eno.CObject>).Namespace);
            aCompileUnit.Namespaces.Add(aNamespace);
            aNamespace.Imports.AddRange((from aNs in this.Tok.Trg_Nsps select new CodeNamespaceImport(aNs)).ToArray());
            aNamespace.Types.AddRange(aClasses);
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


        private void LoadInput()
        {
            var aModelInputRows = CRflRow.NewFromTextFile(this.ModelInputFileInfo);
            var aIdsInputRows = CRflRow.NewFromTextFile(this.IdsInputFileInfo);
            var aInput = aModelInputRows.Concat(aIdsInputRows).ToArray();
            var aModel = new CRflModel(aInput);
            this.RflModel = this.Exp.Expand(aModel);
        }
    }
}
