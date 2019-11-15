// Idea from the Reflectory Utility
// (Tool for generic (binary) remoting protocolls and orm code generation written by Karl-Michael Beck (c) Rehm GmbH.)
// Here we used it as MDL (Model definition language)
// As an input for my second generation ORM-Wrapper.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace CbOrm.Mdl
{
    public sealed class CRowId
    {
        private CRowId(FileInfo aFileInfo, int aLineNr1Based)
        {
            this.FileInfo = aFileInfo;
            this.LineNr1Based = aLineNr1Based;
        }
        private FileInfo FileInfo;
        private int LineNr1Based;
        public override string ToString()
        {
            return "In Line " + this.LineNr1Based.ToString() + " in file '" + this.FileInfo.FullName + "'";
        }
        internal static CRowId New(FileInfo aFileInfo, int aLineNr1Based) => new CRowId(aFileInfo, aLineNr1Based);
    }

    public sealed class CRflRow
    {

        public readonly object RowId;
        public readonly string DataSource;
        public readonly bool Generate;
        public readonly string Class;
        public readonly string Property;
        public readonly string Attribute;
        public readonly string Value;
        public readonly string ValueSource;
        public readonly string Comment;

        private bool GetGenerate(string aText)
        {
            return this.Interpret(() =>
            {
                var aTrimed = aText.Trim();
                if (aTrimed == "+")
                    return true;
                else if (aTrimed == "-")
                    return false;
                else
                    throw new Exception("The value '" + aText + "' is not a valid RflGenerate-Value. Expected +/-");
            });
        }
        private string GetString(string s) => s == null ? string.Empty : s.Trim();
        public CRflRow(object aRowId,
                    string aGenerate,
                    string aClass, 
                    string aProperty, 
                    string aAttribute = null, 
                    string aValue = null, 
                    string aValueSource = null, 
                    string aComment = null
                    )
        {
            this.RowId = aRowId;
            this.Generate = this.GetGenerate(this.GetString(aGenerate));
            this.Class = GetString(this.GetString(aClass));
            this.Property = GetString(aProperty);
            this.Attribute = GetString(aAttribute);
            this.Value = GetString(aValue);
            this.ValueSource = GetString(aValueSource);
            this.Comment = GetString(aComment);
        }
        private static string GetPart(string[] aParts, int aIdx) => aIdx < aParts.Length ? aParts[aIdx] : string.Empty;
        public CRflRow(CRowId aRowId, string[] aParts) :this(aRowId, GetPart(aParts, 0), GetPart(aParts, 1), GetPart(aParts, 2), GetPart(aParts, 3), GetPart(aParts, 4), GetPart(aParts, 5)) {}
        public CRflRow(CRowId aRowId, string aText) : this(aRowId, aText.Split('|'))
        {
        }

        public static CRflRow[] NewFromLines(string[] aLines, FileInfo aFileInfo) => (from aIdx in Enumerable.Range(0, aLines.Length)
                                                                  where aLines[aIdx].Trim().Length > 0
                                                                  select new CRflRow(CRowId.New(aFileInfo, aIdx + 1), aLines[aIdx])).ToArray();
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
    public sealed class CRflTyp
    {
        internal CRflTyp(CRflModel aRflModel, string aName, CRflRow[] aRflRows)
        {
            this.Model = aRflModel;
            this.Name = aName;        
            this.PropertiesDic = (from aGroup in aRflRows.GroupBy(aRflRow => aRflRow.Property) select new CRflProperty(this, aGroup.Key, aGroup.ToArray())).OrderBy(aProperty=>aProperty.Name).ToDictionary(aForKey => aForKey.Name);
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

        public string MetaTyp { get => this.GetAttributeValue("MetaTyp", ()=>string.Empty); }
    }
    public sealed class CRflProperty {

        internal CRflProperty(CRflTyp aRflClass, string aName, CRflRow[] aRows)
        {
            this.Typ = aRflClass;
            this.Name = aName;

            var aAttributesDic = new Dictionary<string, CRflAttribute>(aRows.Length);
            foreach (var aRow in aRows.OrderBy(aRow=>aRow.Attribute))
            {
                var aAttributeName = aRow.Attribute;
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
        public CRflAttribute GetAttribute(string aAttributeName, Func<CRflAttribute> aDefault) => this.AttributesDic.ContainsKey(aAttributeName) ? this.AttributesDic[aAttributeName] : default(CRflAttribute);
        public string GetAttributeValue(string aAttributeName, Func<string> aDefault) => this.AttributesDic.ContainsKey(aAttributeName) ? this.AttributesDic[aAttributeName].Value : aDefault();
        public string GetAttributeValue(string aAttributeName) => this.GetAttributeValue(aAttributeName, ()=>string.Empty);

        internal IEnumerable<CRflAttribute> GetAttributesWithPrefix(string aPrefix) => from aAttribute in this.AttributesDic.Values where aAttribute.Name.StartsWith(aPrefix) select aAttribute;

        public readonly CRflTyp Typ;
        public readonly string Name;
        public readonly Dictionary<string, CRflAttribute> AttributesDic;

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
        public string Name { get => this.RflRow.Attribute; }
        public string Value { get => this.RflRow.Value; }
        
    }

    public sealed class CRflModel
    {
        public CRflModel(IEnumerable<CRflRow> aRows)
        {
            this.Rows = aRows;
            var aGenRows = from aRow in aRows
                           where aRow.Generate
                           select aRow
                                ;
            this.ClassesDic = (from aGroup in aGenRows.GroupBy(aRow => aRow.Class)
                               select new CRflTyp(this, aGroup.Key, aGroup.ToArray())).OrderBy(aClass => aClass.Name).ToArray().ToDictionary(aForKey => aForKey.Name);
        }
        private IEnumerable<CRflRow> Rows;
        public IEnumerable<CRflTyp> Classes { get => this.ClassesDic.Values; }
        public readonly Dictionary<string, CRflTyp> ClassesDic;

        internal CRflTyp GetTypByName(string aTypName)
        {
            if (this.ClassesDic.ContainsKey(aTypName))
                return this.ClassesDic[aTypName];
            else
                throw new Exception("RflTyp '" + aTypName + "' not found.");
        }
    }
}
