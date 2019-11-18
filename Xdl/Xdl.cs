// Idea from the Reflectory Utility
// (Tool for generic (binary) remoting protocolls and orm code generation written by Karl-Michael Beck (c) Rehm GmbH.)
// Here we used it as MDL (Model definition language)
// As an input for my second generation ORM-Wrapper.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using CbOrm.Util;
using System.Dynamic;

namespace CbOrm.Xdl
{
    public sealed class CRflRowId
    {
        private CRflRowId(FileInfo aFileInfo, int aLineNr1Based)
        {
            this.FileInfoNullable = aFileInfo;
            this.LineNr = aLineNr1Based;
        }
        public FileInfo FileInfoNullable { get; set; }
        public int LineNr { get; set; }
        public override string ToString()
        {
            return "In Line " + this.LineNr.ToString() + " in file '" + this.FileInfoNullable.FullName + "'";
        }
        public static CRflRowId New(FileInfo aFileInfo, int aLineNr1Based) => new CRflRowId(aFileInfo, aLineNr1Based);
        public static CRflRowId New(int aLineNr1Based) => new CRflRowId(null, aLineNr1Based);
    }

    public sealed class CRflRow
    {

        public CRflRowId RowId { get; set; }
        public string DataSource { get; set; }
        public CRecognizeEnum RecognizeEnum { get; set; }
        public string TypName { get; set; }
        public string PropertyName { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public string ValueSource { get; set; }
        public string Comment { get; set; }

        public enum CRecognizeEnum
        {
            RecognizeModelRow,
            IgnoreModelRow,
            IgnoreComment            
        }
        public const string RecognizeModelRowText = "+";
        public const string IgnoreModelRowText = "-";
        public const string IgnoreCommentText = "#";
        public static CRecognizeEnum GetRecognizeEnum(string aText)
        {
            switch (aText)
            {
                case RecognizeModelRowText: return CRecognizeEnum.RecognizeModelRow;
                case IgnoreModelRowText: return CRecognizeEnum.IgnoreModelRow;
                case IgnoreCommentText: return CRecognizeEnum.IgnoreModelRow;
                default: throw new ArgumentException();
            }
        }
        public static string GetRecognizeText(CRecognizeEnum aRecognizeEnum)
        {
            switch(aRecognizeEnum)
            {
                case CRecognizeEnum.RecognizeModelRow: return RecognizeModelRowText;
                case CRecognizeEnum.IgnoreModelRow: return IgnoreModelRowText;
                case CRecognizeEnum.IgnoreComment:return IgnoreCommentText;
                default: throw new ArgumentException();
            }
        }
        public string RecognizeText { get => GetRecognizeText(this.RecognizeEnum); }
        public bool RecognizeBool { get => this.RecognizeEnum == CRecognizeEnum.RecognizeModelRow; }
        private string GetString(string s) => s == null ? string.Empty : s.Trim();
        public CRflRow(CRflRowId aRowId,
                    CRecognizeEnum aRecognizeEnum,
                    string aClass, 
                    string aProperty, 
                    string aAttribute = null, 
                    string aValue = null, 
                    string aValueSource = null, 
                    string aComment = null
                    )
        {
            this.RowId = aRowId;
            this.RecognizeEnum = aRecognizeEnum; 
            this.TypName = GetString(this.GetString(aClass));
            this.PropertyName = GetString(aProperty);
            this.AttributeName = GetString(aAttribute);
            this.Value = GetString(aValue);
            this.ValueSource = GetString(aValueSource);
            this.Comment = GetString(aComment);
        }
        public static CRflRow New(string aTypName, string aPropertyName, string aAttributeName, string aAttributeValue, string aValueSource = null, string aComment = null)
            => new CRflRow(null, CRecognizeEnum.RecognizeModelRow, aTypName, aPropertyName, aAttributeName, aAttributeValue, aValueSource, aComment);
        public override string ToString()=> new string[] { this.RecognizeText, this.TypName, this.PropertyName, this.AttributeName, this.Value, this.ValueSource, this.Comment }.JoinString(ColSeperator.ToString());
        private static string GetPart(string[] aParts, int aIdx) => aIdx < aParts.Length ? aParts[aIdx] : default(string);
        public CRflRow(CRflRowId aRowId, string aLine, string[] aParts) :this(aRowId, CRflRow.GetRecognizeEnum(GetPart(aParts, 0)), GetPart(aParts, 1), GetPart(aParts, 2), GetPart(aParts, 3), GetPart(aParts, 4), GetPart(aParts, 5), GetPart(aParts, 6)) {}
        public const char ColSeperator = '|';
        public static CRflRow NewFromLine(CRflRowId aRowId, string aLine) => new CRflRow(aRowId, aLine, aLine.Split(ColSeperator));        
        public static CRflRow[] NewFromLines(string[] aLines, FileInfo aFileInfo) => (from aIdx in Enumerable.Range(0, aLines.Length)
                                                                  where aLines[aIdx].Trim().Length > 0
                                                                  select CRflRow.NewFromLine(CRflRowId.New(aFileInfo, aIdx + 1), aLines[aIdx])).ToArray();
        public static CRflRow[] NewFromTextFile(FileInfo aFileInfo) => NewFromLines(File.ReadAllLines(aFileInfo.FullName), aFileInfo);
        public T Interpret<T>(Func<T> aFunc)
        {
            try
            {
                return aFunc();
            }
            catch(Exception aExc)
            {
                throw new Exception("Error evaluating row '" + this.RowId.ToString() + "' (" + this.DataSource + ") " + aExc.Message, aExc);
            }
        }
    }

    public sealed class CRflRowList  :List<CRflRow>
    {
        public CRflRow Add(string aTypName, string aPropertyName, string aAttributeName, string aValue, string aValueSource, string aComment)
            => this.Add(CRflRow.CRecognizeEnum.RecognizeModelRow, aTypName, aPropertyName, aAttributeName, aValue, aValueSource, aComment);
        public CRflRow Add(CRflRow.CRecognizeEnum aRecognizeEnum, string aTypName, string aPropertyName, string aAttributeName, string aValue, string aValueSource, string aComment)
        {
            var aRow = new CRflRow(CRflRowId.New(this.Count + 1), aRecognizeEnum, aTypName, aPropertyName, aAttributeName, aValue, aValueSource, aComment);
            this.Add(aRow);
            return aRow;
        }
        public void SaveAsText(FileInfo aFileInfo)
        {
            var aTexts = from aRow in this select aRow.ToString();
            File.WriteAllLines(aFileInfo.FullName, aTexts.ToArray());
        }

    }

    public class CRflTokens
    {
        public string Mdl_P_A_Val_Nme = "Value";
        public string Mdl_G_A_Nme_Nme = "Name";
        public string Mdl_G_A_Nsp_Nme = "Namespace";
        public string Mdl_G_A_Beg_Nme = "BeginObject";
        public string Mdl_G_A_End_Nme = "EndObject";
    }

    public class CRflModelInterpreter
    {
        private CRflTokens TokM;
        public CRflTokens Tok { get => CLazyLoad.Get(ref this.TokM, () => new CRflTokens()); }
        public virtual CRflTokens Tokens { get => this.Tok; }

        public virtual dynamic Dyn(CRflTyp aTyp)
        {
            var aDic = (IDictionary<string, object>)new ExpandoObject();
            foreach (var aProp in aTyp.NamedProperties)
            {
                aDic.Add(aProp.Name, aProp.GetAttributeValue(this.Tok.Mdl_P_A_Val_Nme));
            }
            return aDic;
        }
        public virtual IEnumerable<CRflRow> GetRows(IDictionary<string, object> aObj, string aTypName)
        {
            yield return this.NewBeginObjectRow(aTypName);
            foreach (var aKvp in aObj)
                yield return new CRflRow(null, CRflRow.CRecognizeEnum.RecognizeModelRow, aTypName, aKvp.Key, this.Tok.Mdl_P_A_Val_Nme, aKvp.Value.IsNullRef() ? string.Empty : aKvp.Value.ToString());
            yield return this.NewEndObjectRow(aTypName);
        }
        public void Add(CRflRowList aRows, IDictionary<string, object> aDic, string aTypName)=>aRows.AddRange(this.GetRows(aDic, aTypName).ToArray());
        private CRflRow NewBeginObjectRow(string aTypName) => CRflRow.New(string.Empty, string.Empty, this.Tok.Mdl_G_A_Beg_Nme, aTypName);
        private CRflRow NewEndObjectRow(string aTypName) => CRflRow.New(string.Empty, string.Empty, this.Tok.Mdl_G_A_End_Nme, aTypName);
        public virtual string GetName(CRflModel aModel) => aModel.GetPropertyAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Nme_Nme);
        public virtual string GetNamespace(CRflModel aModel) => aModel.GetPropertyAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Nsp_Nme);

    }

    public sealed class CRflTyp
    {
        internal CRflTyp(CRflModel aRflModel, string aName, CRflRow[] aRflRows)
        {
            this.Model = aRflModel;
            this.Name = aName;        
            this.PropertiesDic = (from aGroup in aRflRows.GroupBy(aRflRow => aRflRow.PropertyName) select new CRflProperty(this, aGroup.Key, aGroup.ToArray())).ToDictionary(aForKey => aForKey.Name);
        }
        public readonly CRflModel Model;
        public readonly string Name;
        public readonly Dictionary<string, CRflProperty> PropertiesDic;
        public IEnumerable<CRflProperty> Properties { get => this.PropertiesDic.Values; }
        public CRflProperty GetProperty(string aPropertyName, Func<CRflProperty> aDefault) => this.PropertiesDic.ContainsKey(aPropertyName) ? this.PropertiesDic[aPropertyName] : aDefault();
        public CRflAttribute GetAttribute(string aPropertyName, string aAttributeName,Func<CRflAttribute> aDefault)
        {
            var aRflProperty = this.GetProperty(aPropertyName, () => default(CRflProperty));
            var aRflAttribute = aRflProperty != null ? aRflProperty.GetAttribute(aAttributeName, ()=>default(CRflAttribute)) : aDefault();
            return aRflAttribute;
        }
        public string GetttributeValue(string aPropertyName, string aAttributeName, Func<string> aDefault)
        {
            var aAttribute = this.GetAttribute(aPropertyName, aAttributeName, () => default(CRflAttribute));
            var aValue = aAttribute != null ? aAttribute.Value : aDefault();
            return aValue;
        }
        public string GetAttributeValue(string aAttributeName, Func<string> aDefault) => this.GetttributeValue(string.Empty, aAttributeName, aDefault);
        public string GetAttributeValue(string aAttributeName) => this.GetAttributeValue(aAttributeName, () => string.Empty);
        public IEnumerable<CRflProperty> NamedProperties { get => this.Properties.Where(aPrp => aPrp.Name.Length > 0); }
        public CRflProperty GetPropertyNullable(string aPropertyName) => this.GetProperty(aPropertyName, () => default(CRflProperty));
        public string GetPropertyAttributeValue(string aPropertyName, string aAttributeName)=>this.GetPropertyNullable(aPropertyName).DefaultIfNull(()=>string.Empty, aProperty=>aProperty.GetAttributeValue(aAttributeName));
        public string GetPropertyAttributeValue(string aAttributeName) => this.GetPropertyAttributeValue(string.Empty, aAttributeName);
        public string FullName { get => this.Model.FullName + "." + this.Name; }
    }
    public sealed class CRflProperty
    {
        internal CRflProperty(CRflTyp aRflClass, string aName, CRflRow[] aRows)
        {
            this.Typ = aRflClass;
            this.Name = aName;

            var aAttributesDic = new Dictionary<string, CRflAttribute>(aRows.Length);
            foreach (var aRow in aRows)
            {
                var aAttributeName = aRow.AttributeName;
                if (aAttributesDic.ContainsKey(aAttributeName))
                {
                    aRow.Interpret<object>(() => throw new Exception("Ambiguous attribute name '" + aAttributeName + "'."));
                }
                else
                {
                    aAttributesDic.Add(aAttributeName, new CRflAttribute(this, aRow));
                }
            }
            this.AttributesDic = aAttributesDic;           
        }

        public CRflModel Model { get => this.Typ.Model; }
        public CRflAttribute GetAttribute(string aAttributeName, Func<CRflAttribute> aDefault) => this.AttributesDic.ContainsKey(aAttributeName) ? this.AttributesDic[aAttributeName] : default(CRflAttribute);
        public string GetAttributeValue(string aAttributeName, Func<string> aDefault) => this.AttributesDic.ContainsKey(aAttributeName) ? this.AttributesDic[aAttributeName].Value : aDefault();
        public string GetAttributeValue(string aAttributeName) => this.GetAttributeValue(aAttributeName, ()=>string.Empty);

        internal IEnumerable<CRflAttribute> GetAttributesWithPrefix(string aPrefix) => from aAttribute in this.AttributesDic.Values where aAttribute.Name.StartsWith(aPrefix) select aAttribute;

        public readonly CRflTyp Typ;
        public readonly string Name;
        public readonly Dictionary<string, CRflAttribute> AttributesDic;
        public string FullName { get => this.Typ.FullName + "." + this.Name; }

    }
    public sealed class CRflAttribute
    {
        public CRflAttribute(CRflProperty aRflProperty, CRflRow aRflRow)
        {
            this.RflProperty = aRflProperty;
            this.RflRow = aRflRow;
        }
        public readonly CRflProperty RflProperty;
        public readonly CRflRow RflRow;
        public string Name { get => this.RflRow.AttributeName; }
        public string Value { get => this.RflRow.Value; }

        public string FullName { get => this.RflProperty.FullName + "." + this.Name; }
        public override string ToString() => this.FullName + "=" + this.Value.AvoidNullString();
    }

    public sealed class CRflModel
    {
        public CRflModel(CRflModelInterpreter aModelInterpreter, IEnumerable<CRflRow> aRows)
        {
            this.ModelInterpreter = aModelInterpreter;
            this.Rows = aRows;
            var aGenRows = from aRow in aRows
                           where aRow.RecognizeBool
                           select aRow
                                ;
            this.TypsDic = (from aGroup in aGenRows.GroupBy(aRow => aRow.TypName)
                               select new CRflTyp(this, aGroup.Key, aGroup.ToArray())).ToArray().ToDictionary(aForKey => aForKey.Name);
        }
        public readonly CRflModelInterpreter ModelInterpreter;
        public readonly IEnumerable<CRflRow> Rows;
        public IEnumerable<CRflTyp> Typs { get => this.TypsDic.Values; }
        public readonly Dictionary<string, CRflTyp> TypsDic;
        public CRflTyp GetTypByName(string aTypName) => this.TypsDic.ContainsKey(aTypName).DefaultIfFalse(() => new Exception("RflTyp '" + aTypName + "' not found.").Throw<CRflTyp>(), () => this.TypsDic[aTypName]);
        public string GetPropertyAttributeValue(string aTypName, string aPropertyName, string aAttributeName) => this.GetTypNullable(aTypName).DefaultIfNull(() => string.Empty, aTyp => aTyp.GetPropertyAttributeValue(aPropertyName, aAttributeName));
        private CRflTyp GetTypNullable(string aTypName)=> this.TypsDic.ContainsKey(aTypName).DefaultIfFalse(() => default(CRflTyp), () => this.TypsDic[aTypName]);
        public static CRflModel NewFromTextFile(CRflModelInterpreter aInterpreter, FileInfo aFileInfo) => new CRflModel(aInterpreter, CRflRow.NewFromTextFile(aFileInfo));
        public string Name { get => this.ModelInterpreter.GetName(this); }
        public string Namespace { get => this.ModelInterpreter.GetNamespace(this); }
        public string FullName { get => this.Namespace + "." + this.Name; }
    }
}
