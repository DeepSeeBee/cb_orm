﻿// Idea from the Reflectory Utility
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
using CbOrm.Gen;

namespace CbOrm.Xdl
{
    public sealed class CRflRowId
    {
        private CRflRowId(FileInfo aFileInfo, int aLineIdx)
        {
            this.FileInfoNullable = aFileInfo;
            this.LineIdx = aLineIdx;
        }
        public FileInfo FileInfoNullable { get; set; }
        public int LineIdx { get; set; }
        public int LineNr { get => this.LineIdx + 1; }
        public override string ToString()
        {
            return "In Line " + this.LineNr.ToString() + " in file '" + this.FileInfoNullable.FullName + "'";
        }
        public static CRflRowId New(FileInfo aFileInfo, int aLineIdx) => new CRflRowId(aFileInfo, aLineIdx);
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
            switch (aRecognizeEnum)
            {
                case CRecognizeEnum.RecognizeModelRow: return RecognizeModelRowText;
                case CRecognizeEnum.IgnoreModelRow: return IgnoreModelRowText;
                case CRecognizeEnum.IgnoreComment: return IgnoreCommentText;
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
        public override string ToString() => new string[] { this.RecognizeText, this.TypName, this.PropertyName, this.AttributeName, this.Value, this.ValueSource, this.Comment }.JoinString(ColSeperator.ToString());
        private static string GetPart(string[] aParts, int aIdx) => aIdx < aParts.Length ? aParts[aIdx] : default(string);
        public CRflRow(CRflRowId aRowId, string aLine, string[] aParts) : this(aRowId, CRflRow.GetRecognizeEnum(GetPart(aParts, 0)), GetPart(aParts, 1), GetPart(aParts, 2), GetPart(aParts, 3), GetPart(aParts, 4), GetPart(aParts, 5), GetPart(aParts, 6)) { }
        public const char ColSeperator = '|';
        public static CRflRow NewFromLine(CRflRowId aRowId, string aLine) => new CRflRow(aRowId, aLine, aLine.Split(ColSeperator));

        private static int GetDocVersion(string aLine)
        {
            var aPrefix = "<!DOCTYPE";
            var aRestOfLine = aLine.AvoidNullString().Trim();
            if (!aRestOfLine.StartsWith(aPrefix))
            {
                throw new Exception("DOCTYPE tag missing.");
            }
            aRestOfLine = aRestOfLine.TrimStart(aPrefix).Trim();
            var aDocTypePrefix = "CbXdl_V";
            if (!aRestOfLine.StartsWith(aDocTypePrefix))
            {
                throw new Exception("Not a valid doc type for this parser.");
            }
            aRestOfLine = aRestOfLine.TrimStart(aDocTypePrefix);
            var aClosePos = aRestOfLine.IndexOf(">");
            if (aClosePos == -1)
            {
                throw new Exception("Close bracket missing in DocType line.");
            }
            var aVersionText = aRestOfLine.Substring(0, aClosePos);
            UInt32 aVersionUInt32;
            if (!UInt32.TryParse(aVersionText, out aVersionUInt32))
            {
                throw new Exception("Not a valid version number");
            }
            aRestOfLine = aRestOfLine.Substring(aClosePos+ 1, aRestOfLine.Length - aClosePos-1);
            if (aRestOfLine.Trim().Length > 0)
            {
                throw new Exception("Invalid characters after Close-Bracket of DOCTYPE line.");
            }
            return unchecked((int)aVersionUInt32);
        }

        private static CRflRow[] NewFromLines(UInt32 aVersion, 
                                              string[] aLines, 
                                              FileInfo aFileInfo,
                                              int aLindeOffset)
        {
            switch (aVersion)
            {
                case 1:
                    return NewFromLinesV1(aLines, aFileInfo, aLindeOffset);

                default:
                    throw new Exception("DocType version '" + aVersion.ToString() + "' not supported.");
            }
        }

        private static CRflRow[] NewFromLinesV1(string[] aLines, FileInfo aFileInfo, int aLineOffset)
            => (from aIdx in Enumerable.Range(0, aLines.Length)
                where aLines[aIdx].Trim().Length > 0
                select CRflRow.NewFromLine(CRflRowId.New(aFileInfo, aIdx + aLineOffset), aLines[aIdx])).ToArray();

        public static CRflRow[] NewFromLines(string[] aLines, FileInfo aFileInfo)
        {
            return aFileInfo.Interpret(() =>
            {
                {
                    var aLineIdx = 0;
                    var aLinesTmp = aLines.AsEnumerable();
                    while (aLinesTmp.FirstOrDefault().AvoidNullString().Trim().Length == 0)
                    {
                        ++aLineIdx;
                        aLinesTmp = aLinesTmp.Skip(1);
                    }
                    if (aLines.IsEmpty())
                    {
                        return new CRflRow[] { };
                    }
                    else
                    {
                        var aVersion = GetDocVersion(aLines.First());
                        ++aLineIdx;
                        var aRows = NewFromLinesV1(aLines.Skip(1).ToArray(), aFileInfo, aLineIdx);
                        return aRows;
                    }
                }
            });
        }
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
            var aTexts1 = new string[] { "<DOCTYPE CbXdl_V1>" };
            var aTexts2 = from aRow in this select aRow.ToString();
            var aTexts3 = aTexts1.Concat(aTexts2);
            var aTexts = aTexts3;
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
        public virtual string GetName(CRflModel aModel) => aModel.GetAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Nme_Nme);
        public virtual string GetNamespace(CRflModel aModel) => aModel.GetAttributeValue(string.Empty, string.Empty, this.Tok.Mdl_G_A_Nsp_Nme);

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

        internal T Interpret<T>(Func<T> aFunc)
        {
            try
            {
                return aFunc();
            }
            catch (Exception aExc)
            {
                throw new Exception("Error evaluating type '" + this.FullName + "'. " + aExc.Message, aExc);
            }
        }

        public string FullName { get => this.Model.FullName + "." + this.Name; }

        internal CRflProperty GetProperty(string aPropertyName) => this.GetProperty(aPropertyName, () => new Exception("Property '" + aPropertyName + "' does not exist.").Throw<CRflProperty>());

        internal IEnumerable<CRflAttribute> GetPropertyAttributes(string aPropertyName, string aAttributeName)
            => this.PropertiesDic.ContainsKey(aPropertyName)
             ? this.PropertiesDic[aPropertyName].GetAttributes(aAttributeName)
             : new CRflAttribute[] { };
    }
    public sealed class CRflProperty
    {
        internal CRflProperty(CRflTyp aRflClass, string aName, CRflRow[] aRows)
        {
            this.DeclaringTyp = aRflClass;
            this.Name = aName;

            var aAttributesDic = new Dictionary<string, IEnumerable<CRflAttribute>>(aRows.Length);
            var aGroups = aRows.GroupBy(aRow => aRow.AttributeName);
            var aDic = aGroups.ToDictionary(aForKey => aForKey.Key, aForVal => (from aRow in aForVal select new CRflAttribute(this, aRow)).ToArray().AsEnumerable());
            this.AttributesDic = aDic;
        }
        public T Interpret<T>(Func<T> aFunc)
        {
            try
            {
                return aFunc();
            }
            catch(Exception aExc)
            {
                throw new Exception("Error evaluating property '" + this.FullName + "'. " + aExc.Message, aExc);
            }
        }
        public CRflModel Model { get => this.DeclaringTyp.Model; }
        private CRflAttribute GetSingleAttribute(string aAttributeName)
        {
            var aAttributes = this.AttributesDic.ContainsKey(aAttributeName)
                            ? this.AttributesDic[aAttributeName]
                            : new CRflAttribute[] { };
            if (aAttributes.Count() == 1)
                return aAttributes.Single();
            else if (aAttributes.Count() > 1)
                throw new Exception("Attribute is ambiguous: '" + aAttributeName + "'");
            else
                throw new Exception("Attribute does not exist: '" + aAttributeName + "'");
        }
        public CRflAttribute GetAttribute(string aAttributeName, Func<CRflAttribute> aDefault) => this.AttributesDic.ContainsKey(aAttributeName) ? this.GetSingleAttribute(aAttributeName) : default(CRflAttribute);
        public CRflAttribute GetAttribute(string aAttributeName) => this.GetAttribute(aAttributeName, () => throw new InvalidOperationException());
        public string GetAttributeValue(string aAttributeName, Func<string> aDefault) => this.AttributesDic.ContainsKey(aAttributeName) ? this.GetSingleAttribute(aAttributeName).Value : aDefault();
        public string GetAttributeValue(string aAttributeName) => this.GetAttributeValue(aAttributeName, ()=>string.Empty);

        internal IEnumerable<CRflAttribute> GetAttributesWithPrefix(string aPrefix) => from aAttributes in this.AttributesDic.Values
                                                                                       from aAttribute in aAttributes
                                                                                       where aAttribute.Name.StartsWith(aPrefix) select aAttribute;

        public readonly CRflTyp DeclaringTyp;
        public readonly string Name;
        public readonly Dictionary<string, IEnumerable<CRflAttribute>> AttributesDic;
        public string FullName { get => this.DeclaringTyp.FullName + "." + this.Name; }

        internal IEnumerable<CRflAttribute> GetAttributes(string aAttributeName) => this.AttributesDic.ContainsKey(aAttributeName) ? this.AttributesDic[aAttributeName] : new CRflAttribute[] { };
    }
    public sealed class CRflAttribute
    {
        public CRflAttribute(CRflProperty aRflProperty, CRflRow aRflRow)
        {
            this.RflProperty = aRflProperty;
            this.Row = aRflRow;
        }
        public readonly CRflProperty RflProperty;
        public readonly CRflRow Row;
        public string Name { get => this.Row.AttributeName; }
        public string Value { get => this.Row.Value; }

        public string FullName { get => this.RflProperty.FullName + "." + this.Name; }
        public override string ToString() => this.FullName + "=" + this.Value.AvoidNullString();

        internal T Interpret<T>(Func<T> aFunc)
        {
            try
            {
                return aFunc();
            }
            catch(Exception aExc)
            {
                throw new Exception("Error evaluating Attribute '" + this.FullName + "'. " + aExc.Message);
            }
        }

    }

    public sealed class CRflModel
    {
        public CRflModel(CRflModelInterpreter aModelInterpreter, FileInfo aFileInfo, IEnumerable<CRflRow> aRows)
        {
            this.ModelInterpreter = aModelInterpreter;
            this.FileInfo = aFileInfo;
            this.Rows = aRows;
            var aGenRows = from aRow in aRows
                           where aRow.RecognizeBool
                           select aRow
                                ;
            this.TypsDic = (from aGroup in aGenRows.GroupBy(aRow => aRow.TypName)
                               select new CRflTyp(this, aGroup.Key, aGroup.ToArray())).ToArray().ToDictionary(aForKey => aForKey.Name);
        }
        public readonly CRflModelInterpreter ModelInterpreter;
        public readonly FileInfo FileInfo;
        public readonly IEnumerable<CRflRow> Rows;
        public IEnumerable<CRflTyp> Typs { get => this.TypsDic.Values; }
        public readonly Dictionary<string, CRflTyp> TypsDic;
        public CRflTyp GetTypByName(string aTypName) => this.TypsDic.ContainsKey(aTypName).DefaultIfFalse(() => new Exception("RflTyp '" + aTypName + "' not found.").Throw<CRflTyp>(), () => this.TypsDic[aTypName]);
        public string GetAttributeValue(string aTypName, string aPropertyName, string aAttributeName) => this.GetTypNullable(aTypName).DefaultIfNull(() => string.Empty, aTyp => aTyp.GetPropertyAttributeValue(aPropertyName, aAttributeName));
        public IEnumerable<CRflAttribute> GetAttributes(string aTypName, string aPropertyName, string aAttributeName) => this.TypsDic.ContainsKey(aTypName) ? this.GetTypByName(aTypName).GetPropertyAttributes(aPropertyName, aAttributeName) : new CRflAttribute[] { };
        private CRflTyp GetTypNullable(string aTypName)=> this.TypsDic.ContainsKey(aTypName).DefaultIfFalse(() => default(CRflTyp), () => this.TypsDic[aTypName]);
        public static CRflModel NewFromTextFile(CRflModelInterpreter aInterpreter, FileInfo aFileInfo) => new CRflModel(aInterpreter, aFileInfo, CRflRow.NewFromTextFile(aFileInfo));
        public string Name { get => this.ModelInterpreter.GetName(this); }
        public string Namespace { get => this.ModelInterpreter.GetNamespace(this); }
        public string FullName { get => this.Namespace + "." + this.Name; }
    }
}
