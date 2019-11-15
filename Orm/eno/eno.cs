// This is the runtime for my 2nd generation ORM-Wrapper. (entity object sub system)

using CbOrm.Str;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CbOrm.Eno
{
 
    public abstract class CObject
    {
        public CObject(CStorage aStorage)
        {
            this.Storage = aStorage;
        }
        public readonly CStorage Storage;
    }

    public abstract class CEntityObject 
    : 
        CObject
    {
        public CEntityObject(CStorage aStorage) : base(aStorage)
        {
        }

        public dynamic Dynamic { get => throw new NotImplementedException(); }
    }

    public abstract class CLeaf 
    : 
        CObject
    {
        public CLeaf(CStorage aStorage) : base(aStorage)
        {
        }
    }

    public sealed class CBlop 
    : 
        CLeaf
    {
        public CBlop(CStorage aStorage) : base(aStorage)
        {
        }
    }






























    //internal sealed class CXmlConverters
    //{
    //    //internal static string SaveEncryptedString(object aClrValue, CStringEncryption aEncryption)
    //    //{
    //    //    var aString = aClrValue == null ? string.Empty : aClrValue.ToString();
    //    //    var aEncrypted = aEncryption.Encrypt(aString);
    //    //    return aEncrypted;
    //    //}
    //    //internal static object LoadEncryptedString(string aXmlValue, CStringEncryption aEncryption)
    //    //{
    //    //    return aEncryption.Decrypt(aXmlValue);
    //    //}

    //    internal static string SaveInt32(object aClrValue)
    //    {
    //        return aClrValue.ToString();
    //    }

    //    internal static object LoadInt32(string aXmlValue)
    //    {
    //        return aXmlValue == null || aXmlValue.Trim().Length == 0
    //             ? (Int32)0
    //             : (Int32)Convert.ToInt32(aXmlValue);
    //    }

    //    private const string UtcSuffix = "(Utc)";

    //    internal static object LoadDateTime(string aXmlValue)
    //    {
    //        if (aXmlValue == null
    //        || aXmlValue.Trim().Length == 0)
    //            return DateTime.FromFileTime(0);
    //        var aDateAndTime = aXmlValue.Split('T');
    //        var aDate = aDateAndTime[0];
    //        var aTime = aDateAndTime[1];
    //        var aDateParts = aDate.Split('-');
    //        var aTimeParts = aTime.Split(':');
    //        var aYear = aDateParts[0];
    //        var aMonth = aDateParts[1];
    //        var aDay = aDateParts[2];
    //        var aHour = aTimeParts[0];
    //        var aMinute = aTimeParts[1];
    //        var aSecondAndUtc = aTimeParts[2];
    //        if (aSecondAndUtc.EndsWith(UtcSuffix))
    //        {
    //            var aSecond = aSecondAndUtc.Substring(0, aSecondAndUtc.Length - UtcSuffix.Length);
    //            var aDateTime = new DateTime(Convert.ToInt32(aYear),
    //                                         Convert.ToInt32(aMonth),
    //                                         Convert.ToInt32(aDay),
    //                                         Convert.ToInt32(aHour),
    //                                         Convert.ToInt32(aMinute),
    //                                         Convert.ToInt32(aSecond),
    //                                         DateTimeKind.Utc);
    //            return aDateTime.ToLocalTime();
    //        }
    //        else
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    internal static string SaveDateTime(object aClrValue)
    //    {
    //        var aDateTime = (DateTime)aClrValue;
    //        var aUtcDateTime = aDateTime.ToUniversalTime();
    //        var aYear = aUtcDateTime.Year.ToString().PadLeft(4, '0');
    //        var aMonth = aUtcDateTime.Month.ToString().PadLeft(2, '0');
    //        var aDay = aUtcDateTime.Day.ToString().PadLeft(2, '0');
    //        var aHour = aUtcDateTime.Hour.ToString().PadLeft(2, '0');
    //        var aMinute = aUtcDateTime.Minute.ToString().PadLeft(2, '0');
    //        var aSecond = aUtcDateTime.Second.ToString().PadLeft(2, '0');
    //        var aString = aYear + "-" + aMonth + "-" + aDay + "T" + aHour + ":" + aMinute + ":" + aSecond + UtcSuffix;
    //        return aString;
    //    }

    //    internal static object LoadString(string aXmlValue)
    //    {
    //        return aXmlValue;
    //    }

    //    internal static string SaveString(object aClrValue)
    //    {
    //        return aClrValue == null ? string.Empty : aClrValue.ToString();
    //    }

    //    internal static object LoadBool(string aXmlValue)
    //    {
    //        return aXmlValue == null || aXmlValue.Length == 0 ? false : Boolean.Parse(aXmlValue);
    //    }

    //    internal static string SaveBool(object aClrValue)
    //    {
    //        return aClrValue.ToString();
    //    }

    //    internal static string SaveFileInfo(object aClrValue)
    //    {
    //        return aClrValue == null
    //            ? string.Empty
    //            : ((FileInfo)aClrValue).FullName;
    //    }

    //    internal static object LoadFileInfo(string aXmlValue)
    //    {
    //        return aXmlValue == null
    //            ? default(FileInfo)
    //            : aXmlValue.Trim().Length == 0
    //            ? default(FileInfo)
    //            : new FileInfo(aXmlValue)
    //            ;
    //    }

    //    internal static string SaveEnum(object aClrValue)
    //    {
    //        return aClrValue.ToString();
    //    }

    //    internal static object LoadEnum(Type aEnumType, string aXmlValue)
    //    {
    //        return aXmlValue == null || aXmlValue.Length == 0
    //            ? Enum.GetValues(aEnumType).Cast<Enum>().First()
    //            : Enum.Parse(aEnumType, aXmlValue);
    //    }


    //    internal static object LoadGuid(string aXmlValue)
    //    {
    //        return aXmlValue.Trim().Length == 0 ? default(Guid) : new Guid(aXmlValue);
    //    }

    //    internal static string SaveGuid(object aClrValue)
    //    {
    //        return aClrValue == null
    //             ? default(Guid).ToString()
    //             : aClrValue.ToString()
    //             ;
    //    }
    //}

    //internal delegate object CLoadValue(string aXmlValue);
    //internal delegate string CSaveValue(object aClrValue);

    //public abstract class CProperty
    //{
    //    internal CProperty(string aName, CSaveValue aSaveValue, CLoadValue aLoadValue)
    //    {
    //        this.Name = aName;
    //        this.SaveValue = aSaveValue;
    //        this.LoadValue = aLoadValue;
    //    }

    //    internal readonly string Name;
    //    internal readonly CSaveValue SaveValue;
    //    internal readonly CLoadValue LoadValue;
    //    internal const string ObjectElementName = "Object";

    //    internal virtual bool Embedd { get => true; }

    //    internal abstract void Save(CNode aObject, XmlDocument aXmlDocument, XmlElement aObjectElement);

    //    internal abstract void Load(XmlElement aObjectElement, CNode aObject);
    //}

    //internal sealed class CAttributeValueProperty : CProperty
    //{
    //    internal CAttributeValueProperty(string aName,
    //                                     CSaveValue aSaveValue,
    //                                     CLoadValue aLoadValue
    //                                     ) : base(aName, aSaveValue, aLoadValue) { }

    //    internal override void Save(CNode aObject, XmlDocument aXmlDocument, XmlElement aObjectElement)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aValueDic = aObject.ValueDic;
    //        var aClrValue = aValueDic.HasProperty(aPropertyName)
    //          ? aValueDic.GetValue(aPropertyName)
    //          : default(object)
    //          ;
    //        var aXmlValue = aProperty.SaveValue(aClrValue);
    //        aObjectElement.SetAttribute(aPropertyName, aXmlValue);
    //    }
    //    internal override void Load(XmlElement aObjectElement, CNode aObject)
    //    {
    //        var aProperty = this;
    //        var aValuesDic = aObject.ValueDic;
    //        var aPropertyName = aProperty.Name;
    //        var aXmlValue = aObjectElement.GetAttribute(aPropertyName);
    //        var aClrValue = aProperty.LoadValue(aXmlValue);
    //        aValuesDic.SetValue(aPropertyName, aClrValue);
    //        //CGlobalDebug.Object = aObject;
    //    }
    //}

    //internal sealed class CElementValueProperty : CProperty
    //{
    //    internal CElementValueProperty(string aName, CSaveValue aSaveValue, CLoadValue aLoadValue) : base(aName, aSaveValue, aLoadValue) { }

    //    internal override void Save(CNode aObject, XmlDocument aXmlDocument, XmlElement aObjectElement)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aValueDic = aObject.ValueDic;
    //        var aPropertyElement = aXmlDocument.CreateElement(aPropertyName);
    //        var aClrValue = aValueDic.HasProperty(aPropertyName)
    //                      ? aValueDic.GetValue(aPropertyName)
    //                      : null;
    //        var aXmlValue = aProperty.SaveValue(aClrValue);
    //        aPropertyElement.InnerText = aXmlValue;
    //        aObjectElement.AppendChild(aPropertyElement);
    //    }

    //    internal override void Load(XmlElement aObjectElement, CNode aObject)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aAttributeElement = aObjectElement.SelectNodes(aPropertyName).Cast<XmlElement>().SingleOrDefault();
    //        if(aAttributeElement != null)
    //        {
    //            var aXmlValue = aAttributeElement.InnerText;
    //            var aValue = aProperty.LoadValue(aXmlValue);
    //            var aValueDic = aObject.ValueDic;
    //            aValueDic.SetValue(aPropertyName, aValue);
    //        }            
    //    }
    //}

    //internal abstract class CStructuredObjects : IEnumerable
    //{
    //    internal CStructuredObjects(CObjectStorage aStorage)
    //    {
    //        this.Storage = aStorage;
    //    }

    //    internal CObjectStorage Storage;

    //    internal abstract void Clear();

    //    internal abstract void Add(CNode aItem);

    //    internal abstract void Remove(CNode aItem);

    //    IEnumerator IEnumerable.GetEnumerator() => this.AsEnumerable().GetEnumerator();

    //    internal abstract IEnumerable<CNode> AsEnumerable();

    //    internal abstract CNode GetItemBase(Guid aGuid);

    //}

    //internal sealed class CStructuredObjects<T> : CStructuredObjects, IEnumerable<T> where T : CNode
    //{
    //    internal CStructuredObjects(CObjectStorage aStorage) :base(aStorage) { }

    //    private readonly List<T> List = new List<T>();

    //    public IEnumerator<T> GetEnumerator() => this.List.GetEnumerator();
    //    internal override IEnumerable<CNode> AsEnumerable() => this;

    //    internal override void Clear()
    //    {
    //        this.List.Clear();
    //    }

    //    internal override void Add(CNode aItem) => this.Add((T)aItem);
    //    internal void Add(T aItem) => this.List.Add(aItem);
    //    internal void Remove(T aItem) => this.List.Remove(aItem); 
    //    internal override void Remove(CNode aItem) => this.Remove((T)aItem);

    //    internal T GetItem(Guid aGuid)
    //    {
    //        var aItem1 = (from aTest in this
    //                        where aTest.Guid == aGuid
    //                        select aTest).SingleOrDefault();
    //        var aItem2 = aItem1 == null
    //                   ? this.Storage.NewObject<T>()
    //                   : aItem1
    //                   ;
    //        var aItem = aItem2;
    //        return aItem;
    //    }
    //    internal override CNode GetItemBase(Guid aGuid) => (T)this.GetItem(aGuid);
    //}

    //internal sealed class CElementObjectProperty : CProperty
    //{
    //    internal CElementObjectProperty(string aName) : base(aName, null, null)
    //    {
    //    }


    //    internal override void Save(CNode aObject, XmlDocument aXmlDocument, XmlElement aObjectElement)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aValueDic = aObject.ValueDic;
    //        var aPropertyElement = aXmlDocument.CreateElement(aPropertyName);
    //        var aClrValue = aValueDic.HasProperty(aPropertyName)
    //                      ? aValueDic.GetValue(aPropertyName)
    //                      : null;
    //        var aChildObject = (CNode)aClrValue;
    //        var aXmlElement = aChildObject.NewXmlElement(aXmlDocument);
    //        aPropertyElement.AppendChild(aXmlElement);
    //        aObjectElement.AppendChild(aPropertyElement);
    //    }

    //    internal override void Load(XmlElement aObjectElement, CNode aObject)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aPropertyElement = aObjectElement.SelectSingleNode(aPropertyName);
    //        var aChildObjectElement = aPropertyElement == null
    //                                ? default(XmlElement)
    //                                : (XmlElement)aPropertyElement.SelectSingleNode(ObjectElementName)
    //                                ;
    //        var aChildObject = aChildObjectElement == null
    //                          ? default(CObject)
    //                          : CNode.New(aObject.ObjectStorage, aChildObjectElement);
    //        if (aChildObject != null)
    //        {
    //            var aValuesDic = aObject.ValueDic;
    //            aValuesDic.SetValue(aPropertyName, aChildObject);
    //        }
    //    }
    //}

    //internal sealed class CElementObjectsProperty : CProperty
    //{
    //    internal CElementObjectsProperty(string aName, Func<CNode, CStructuredObjectsRef> aGetRefFunc) : base(aName, null, null)
    //    {
    //        this.GetRefFunc = aGetRefFunc;
    //    }

    //    private readonly Func<CNode, CStructuredObjectsRef> GetRefFunc;

    //    internal override void Save(CNode aObject, XmlDocument aXmlDocument, XmlElement aObjectElement)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aPropertyElement = aXmlDocument.CreateElement(aPropertyName);
    //        var aStructuredObjectsRef = this.GetRefFunc(aObject);
    //        var aChildElements = from aChild in aStructuredObjectsRef.StructuredObjects
    //                             select aChild.NewXmlElement(aXmlDocument);
    //        foreach (var aChildElement in aChildElements.AsEnumerable())
    //        {
    //            aPropertyElement.AppendChild(aChildElement);
    //        }
    //        aObjectElement.AppendChild(aPropertyElement);
    //    }
    //    internal override void Load(XmlElement aObjectElement, CNode aObject)
    //    {
    //        var aProperty = this;
    //        var aPropertyName = aProperty.Name;
    //        var aPropertyElement = aObjectElement.SelectSingleNode(aPropertyName);
    //        if (aPropertyElement != null)
    //        {
    //            var aChildElements = aPropertyElement.SelectNodes(ObjectElementName).OfType<XmlElement>();
    //            var aChildObjects = from aChildElement in aChildElements select aObject.ObjectStorage.Load(aChildElement);
    //            var aStructuredObjectsRef = this.GetRefFunc(aObject);
    //            aStructuredObjectsRef.ClearWithoutDelete();
    //            foreach (var aChildObject in aChildObjects)
    //            {
    //                aStructuredObjectsRef.Add(aChildObject);
    //            }

    //        }
    //    }
    //}

    //public sealed class CType
    //{
    //    internal CType(CObject aPrototype)
    //    {
    //        this.Prototype = aPrototype;
    //        this.Properties = aPrototype.NewProperties().ToDictionary(aForKey => aForKey.Name);
    //    }
    //    private readonly CObject Prototype;
    //    internal readonly Dictionary<string, CProperty> Properties;
    //    internal CProperty GetPropertyByName(string aName) => this.Properties[aName];

    //    internal Type SystemType => this.Prototype.GetType();

    //    internal CObject NewObject(CObjectStorage aObjectStorage)
    //    {
    //        var aObject = this.Prototype.CloneObject(aObjectStorage);
    //        aObject.Unmodify();
    //        return aObject;
    //    }
    //    internal string Name => this.Prototype.StaticTypeName;
    //    internal bool IsStructureReflected { get => this.Prototype.IsStructureReflected; }
    //}

    //internal abstract class CSchema
    //{
    //    internal CSchema()
    //    {

    //    }
    //    internal readonly Dictionary<string, CType> TypeNameDic = new Dictionary<string, CType>();
    //    internal Dictionary<Type, CType> SystemTypeDicM;
    //    internal CType GetTypeBySystemType(Type aType) => this.SystemTypeDic[aType];


    //    internal Dictionary<Type, CType> SystemTypeDic => CLazyLoad.Get(ref SystemTypeDicM, () => this.TypeNameDic.Values.ToDictionary(aForKey => aForKey.SystemType));
    //    internal CType GetTypeByName(string aName) => this.TypeNameDic[aName];
    //    protected void RegisterPrototype(CObject aPrototype) => this.TypeNameDic.Add(aPrototype.StaticTypeName, new CType(aPrototype));


    //}

    //public abstract class CObjectStorage
    //{
    //    protected CObjectStorage()
    //    {
    //    }


    //    internal readonly Dictionary<Guid, CObject> Cache = new Dictionary<Guid, CObject>();

    //    public int Save()
    //    {
    //        var aCount = 0;
    //        var aObjects = this.Cache.Values;
    //        var aModifiedObjects = from aObject in aObjects
    //                               where aObject.IsModified
    //                               select aObject;
    //        foreach (var aObject in aModifiedObjects.ToArray())
    //        {
    //            aObject.SaveSingleObject();
    //            ++aCount;
    //        }
    //        return aCount;
    //    }

    //    internal abstract void Load(CNode aStructuredObject);
    //    internal abstract void Load(CBlop aBlop);

    //    internal CObject LoadOnDemand(Guid aGuid, Func<CObject> aLoad)
    //    {
    //        if (this.Cache.ContainsKey(aGuid))
    //        {
    //            return this.Cache[aGuid];
    //        }
    //        else
    //        {
    //            var aLoaded = aLoad();
    //            aLoaded.AddToCache();
    //            return aLoaded;
    //        }
    //    }

    //    internal T LoadObject<T>(Guid aGuid) where T : CObject
    //    {
    //        return (T)this.LoadOnDemand(aGuid,
    //            () =>
    //            {
    //                var aObject = this.NewObject<T>();
    //                aObject.Guid = aGuid;
    //                aObject.Load();
    //                return aObject;
    //            });
    //    }

    //    public IEnumerable<T> LoadObjects<T>() where T : CObject => this.LoadObjects(typeof(T)).Cast<T>();

    //    public virtual IEnumerable<TChild> LoadObjects<TParent, TChild>(CNode aParent, CR1NRefOptions<TParent, TChild> aR1NRefOptions) where TParent : CNode where TChild : CNode
    //    => from aTest in this.LoadObjects<TChild>() where aR1NRefOptions.GetForeignKey(aTest) == aParent.Guid select aTest;

    //    internal abstract CSchema Schema { get; }

    //    internal abstract void Delete(CObject aObject);
    //    internal abstract bool IsPersistent(CObject aObject);

    //    internal CObject Load(XmlElement aChildElement)
    //    {
    //        var aObjectStorage = this;
    //        var aTypName = aChildElement.GetAttribute(CNode.TypeAttributeName);
    //        var aTyp = aObjectStorage.Schema.GetTypeByName(aTypName);
    //        var aObject = aTyp.NewObject(aObjectStorage);
    //        aObject.Load(aChildElement);
    //        return aObject;
    //    }


    //    public abstract IEnumerable<CObject> LoadObjects(CType aType);
    //    public abstract CObject LoadObject(CType aType, Guid aGuid);

    //    public CObject LoadObject(string aTypeName, Guid aGuid) 
    //    {
    //        return this.LoadObject(this.Schema.GetTypeByName(aTypeName), aGuid);
    //    }

    //    public IEnumerable<CObject> LoadObjects(Type aSystemType) 
    //    {
    //        var aType = this.Schema.GetTypeBySystemType(aSystemType);
    //        var aObjects = this.LoadObjects(aType);
    //        return aObjects;
    //    }

    //    internal T NewObject<T>() where T : CObject
    //    {
    //        return (T)this.Schema.GetTypeBySystemType(typeof(T)).NewObject(this);
    //    }

    //    internal T Create<T>() where T : CObject
    //    {
    //        var aObj = this.NewObject<T>();
    //        aObj.Create();
    //        return aObj;
    //    }



    //    internal abstract long GetBlopLength(CBlop aBlop);

    //    internal abstract Stream NewBlopInputStream(CBlop aBlop);

    //    protected abstract CBlopOutputStream NewBlopOutputStream(CBlop aBlop);

    //    internal abstract void VisitSave(CNode aStructuredObject);
    //    internal void VisitSave(CBlop aBlop, Stream aStream)
    //    {
    //        using (var aBlopOutputStream = this.NewBlopOutputStream(aBlop))
    //        {
    //            aStream.CopyTo(aBlopOutputStream.Stream);
    //            aBlopOutputStream.Commit();
    //        }
    //    }

    //    public abstract CObjectStorage CloneStorage(bool aConnect);

    //}

    //public abstract class CBlopOutputStream : IDisposable
    //{
    //    protected CBlopOutputStream(Stream aInnerStream) { this.Stream = aInnerStream; }
    //    public readonly Stream Stream;
    //    public void Dispose() => this.Stream.Dispose();
    //    public abstract void Commit();
    //}

    //internal sealed class CFileSystemBlopOutputStream:CBlopOutputStream
    //{
    //    public CFileSystemBlopOutputStream(Stream aInnerStream):base(aInnerStream) { }
    //    public override void Commit()
    //    {
    //        this.Stream.Flush();
    //        this.Stream.SetLength(this.Stream.Position);
    //        this.Stream.Close();
    //    }
    //}

    //public abstract class CFileSystemObjectStorage : CObjectStorage
    //{

    //    internal CFileSystemObjectStorage():base()
    //    {
    //    }

    //    private Guid GetGuidFromFileName(FileInfo aFileInfo) => throw new NotImplementedException();// new Guid(aFileInfo.Name.TrimEnd(aFileInfo.Extension));

    //    internal override Stream NewBlopInputStream(CBlop aBlop)
    //    {
    //        var aFileInfo = this.GetObjectFileInfo(aBlop, false);
    //        var aStream = aFileInfo == null
    //                    ? (Stream)new MemoryStream()
    //                    : aFileInfo.CloneNullable().Exists
    //                    ? (Stream)File.OpenRead(aFileInfo.FullName)
    //                    : new MemoryStream()
    //                    ;
    //        return aStream;
    //    }

    //    protected override CBlopOutputStream NewBlopOutputStream(CBlop aBlop)
    //    {
    //        var aFileInfo = this.GetObjectFileInfo(aBlop);
    //        var aStream=  File.OpenWrite(aFileInfo.FullName);
    //        var aBlopOutputStream = new CFileSystemBlopOutputStream(aStream);
    //        return aBlopOutputStream;
    //    }

    //    internal override long GetBlopLength(CBlop aBlop)
    //    {
    //        var aFileInfo = this.GetObjectFileInfo(aBlop, false);
    //        var aLength = aFileInfo.Exists
    //                 ? aFileInfo.Length
    //                 : 0
    //                 ;
    //        return aLength;
    //    }

    //    public override CObject LoadObject(CType aType, Guid aGuid)
    //    {
    //        return this.LoadObject(aType, this.GetObjectFileInfo(aType, aGuid));
    //    }

    //    private FileInfo GetObjectFileInfo(CType aType, Guid aGuid)
    //    {
    //        var aExtension = aType.IsStructureReflected
    //                       ? ".xml"
    //                       : ".bin"                           
    //                       ;
    //        var aDirectory = this.GetObjectDirectory(aType.Name);
    //        var aFileInfo = new FileInfo(Path.Combine(aDirectory.FullName, aGuid.ToString() + aExtension));
    //        return aFileInfo;
    //    }

    //    internal CObject LoadObject(CType aType, FileInfo aFileInfo)
    //    {
    //        var aObjectId = this.GetGuidFromFileName(aFileInfo);
    //        return this.LoadOnDemand(aObjectId,                
    //                                ()=>
    //                                {
    //                                    var aObject = aType.NewObject(this);
    //                                    aObject.Guid = aObjectId;
    //                                    aObject.Load();
    //                                    return aObject;
    //                                }
    //                        );
    //    }

    //    public override IEnumerable<CObject> LoadObjects(CType aType)
    //    {
    //        var aObjectStorage = this;
    //        var aDirectory = this.GetObjectDirectory(aType);
    //        var aFiles = aDirectory.GetFiles();
    //        var aObjects = from aFile in aFiles select this.LoadObject(aType, aFile);
    //        return aObjects;
    //    }

    //    internal override bool IsPersistent(CObject aObject)
    //    {
    //        var aObjectFileInfo = this.GetObjectFileInfo(aObject);
    //        var aIsPersistent = aObjectFileInfo.Exists;
    //        return aIsPersistent;
    //    }

    //    internal abstract DirectoryInfo GetObjectDirectory(string aObjectTypeName);


    //    internal DirectoryInfo GetObjectDirectory(CType aType)
    //    {
    //        return this.GetObjectDirectory(aType.Name);
    //    }

    //    private FileInfo GetObjectFileInfo(CObject aObject, Guid aGuid)
    //    {
    //        var aType = aObject.Type;
    //        var aFileInfo = this.GetObjectFileInfo(aType, aGuid);
    //        return aFileInfo;
    //    }

    //    //TODO: PARamter CrateGuidOnDemand is obsolte.
    //    private FileInfo GetObjectFileInfo(CObject aObject, bool aCreateGuidOnDemand = false)
    //    {
    //        if (aCreateGuidOnDemand)
    //        {
    //            if(aObject.GuidIsNull)
    //            {
    //                throw new Exception("Internal error.");
    //                //aObject.CreateGuidOnDemand();
    //            }
    //        }
    //        return this.GetObjectFileInfo(aObject, aObject.Guid);
    //    }

    //    internal override void Load(CBlop aBlop)
    //    {
    //        // The blop will request the input stream or the length as soon as it is used.
    //        // Not sure if we need some special actions for databases.
    //    }

    //    internal override void Delete(CObject aObject)
    //    {
    //        if (!aObject.IsEmbedded
    //        && aObject.IsPersistent)
    //        {
    //            var aFileInfo = this.GetObjectFileInfo(aObject, false);
    //            aFileInfo.Delete();
    //        }
    //    }

    //    internal override void VisitSave(CNode aStructuredObject)
    //    {
    //        if (!aStructuredObject.IsDeleted)
    //        {
    //            if (aStructuredObject.GuidIsNull)
    //            {
    //                throw new Exception("Internal error.");
    //                //aObject.CreateGuidOnDemand();
    //            }
    //            var aFileInfo = this.GetObjectFileInfo(aStructuredObject);
    //            var aXmlDocument = aStructuredObject.NewXmlDocument();
    //            aXmlDocument.Save(aFileInfo.FullName);
    //        }
    //    }

    //    internal override void Load(CNode aStructuredObject)
    //    {
    //        var aFileInfo = this.GetObjectFileInfo(aStructuredObject);
    //        if(!aFileInfo.Exists)
    //        {
    //            throw new CObjectDeletedExcption();
    //        }
    //        var aXmlDocument = new XmlDocument();
    //        aXmlDocument.Load(aFileInfo.FullName);
    //        aStructuredObject.Load(aXmlDocument.DocumentElement);
    //    }


    //}

    //internal sealed class CObjectDeletedExcption : Exception
    //{

    //}

    //public abstract class CObject
    //{
    //    internal CObject(CObjectStorage aObjectStorage)
    //    {
    //        this.ObjectStorage = aObjectStorage;
    //    }

    //    public abstract CObject CloneObject(CObjectStorage aObjectStorage);
    //    internal abstract bool IsStructureReflected { get; }
    //    internal bool IsModified { get; private set; }
    //    internal void Modify() { this.IsModified = true; }

    //    internal void Unmodify() { this.IsModified = false; }

    //    internal readonly CObjectStorage ObjectStorage;

    //    internal virtual IEnumerable<CProperty> NewProperties()
    //    {
    //        return new CProperty[] { };
    //    }

    //    internal abstract string StaticTypeName
    //    {
    //        get;
    //    }
    //    internal static readonly Guid DeletedObjectGuid = new Guid("00000000-dead-dead-dead-000000000000");

    //    internal virtual string TypeName
    //    {
    //        get => this.StaticTypeName; set
    //        {
    //            if (this.StaticTypeName != value)
    //                throw new InvalidOperationException();
    //        }
    //    }
    //    public abstract Guid Guid { get; internal set; }

    //    internal abstract void AcceptSave();

    //    internal abstract void Load(XmlElement aObjectElement);
    //    internal abstract void AcceptLoad();
    //    internal virtual void LoadTemplate() => this.AcceptLoad();

    //    internal void Load()
    //    {
    //        var aObject = this;
    //        if (aObject.Guid != default(Guid)
    //        && aObject.Guid != CNode.DeletedObjectGuid
    //        && !aObject.IsEmbedded)
    //        {
    //            this.LoadTemplate();
    //            this.Unmodify();
    //        }
    //    }

    //    #region Cache
    //    internal bool IsCached { get; private set; }
    //    internal void CheckNotCached()
    //    {
    //        if (this.IsCached) { throw new InvalidOperationException(); }
    //    }
    //    internal void AddToCache()
    //    {
    //        this.CheckNotCached();
    //        if (this.Guid == default(Guid))
    //        {
    //            // Nicht cachen, erst wenn CreateAufrruf erfolgt.s
    //        }
    //        else if(this.IsDeleted)
    //        {
    //            this.IsCached = true;
    //        }
    //        else
    //        {
    //            this.ObjectStorage.Cache.Add(this.Guid, this);
    //            this.IsCached = true;
    //        }

    //    }
    //    #endregion

    //    protected virtual void SaveTemplate() => this.AcceptSave();

    //    internal void SaveSingleObject()
    //    {
    //        if (this.IsEmbedded)
    //        {

    //        }
    //        else if(!this.SaveIsOk)
    //        {

    //        }
    //        else
    //        {
    //            this.SaveTemplate();
    //            this.IsModified = false;
    //        }
    //    }

    //    public void SaveAllModifiedObjects()
    //    {
    //        this.ObjectStorage.Save();
    //    }


    //    internal virtual void Delete() => this.ObjectStorage.Delete(this);
    //    internal bool IsPersistent { get => this.ObjectStorage.IsPersistent(this); }

    //    internal void CreateGuid()
    //    {
    //        this.CheckNotCached();
    //        this.Guid = Guid.NewGuid();
    //    }
    //    public bool GuidIsNull { get => this.Guid == new Guid(); }
    //    public bool IsDeleted { get => this.Guid == DeletedObjectGuid; }

    //    internal bool IsEmbedded { get; set; }

    //    internal void CheckNotGuidIsNull()
    //    {
    //        if(this.GuidIsNull)
    //        {
    //            throw new Exception(this.StaticTypeName + " not found.");
    //        }
    //    }

    //    internal void CreateGuidOnDemand()
    //    {
    //        if (this.GuidIsNull)
    //        {
    //            this.CreateGuid();
    //        }
    //    }

    //    internal virtual void Create()
    //    {
    //        if(this.GuidIsNull)
    //        {
    //            this.CreateGuid();
    //            this.AddToCache();
    //        }
    //        else
    //        {
    //            throw new InvalidOperationException();
    //        }
    //    }

    //    internal virtual bool SaveIsOk { get => true; }
    //    public CType Type { get => this.ObjectStorage.Schema.GetTypeByName(this.StaticTypeName); }

    //    #region Name
    //    internal enum CNewNameErrorEnum
    //    {
    //        Ok,
    //        RequiredField,
    //        NameAllreadyExists
    //    }
    //    internal string MakeNameCompareable(string aName) => aName.Trim().ToLower();
    //    internal CNewNameErrorEnum GetNewNameErrorEnum<T, TName>(string aOldName, 
    //                                                             TName aNewName, 
    //                                                             IEnumerable<T> aObjects, 
    //                                                             Func<T, TName> aGetName,
    //                                                             Func<TName, bool> aGetNameIsEmpty) where T : CNode
    //    {
    //        if (aGetNameIsEmpty(aNewName))
    //            return CNewNameErrorEnum.RequiredField;
    //        else if ((from aItem in aObjects
    //                  where aItem.Guid != this.Guid
    //                  where object.Equals(aGetName(aItem), aNewName)
    //                  select aItem).FirstOrDefault() != null)
    //            return CNewNameErrorEnum.NameAllreadyExists;
    //        else
    //            return CNewNameErrorEnum.Ok;
    //    }

    //    internal abstract CRef GetRefByGuid(Guid aGuid);
    //    #endregion
    //}

    //public abstract class CLeaf : CObject
    //{
    //    internal CLeaf(CObjectStorage aStorage) :base(aStorage) { }
    //    internal override CRef GetRefByGuid(Guid aGuid) => throw new InvalidOperationException();
    //}

    //public sealed class CBlop : CLeaf
    //{
    //    internal CBlop(CObjectStorage aObjectStorage) : base(aObjectStorage)
    //    {
    //        this.Guid = default(Guid);
    //    }
    //    internal override string StaticTypeName => "Blop";
    //    internal override bool IsStructureReflected => true;
    //    public override CObject CloneObject(CObjectStorage aObjectStorage) => new CBlop(aObjectStorage);
    //    private Guid GuidM;
    //    public override Guid Guid { get => this.GuidM; internal set { this.CheckNotCached(); this.GuidM = value; } }
    //    internal override void Load(XmlElement aObjectElement)=> throw new NotImplementedException();
    //    internal Stream NewInputStream()
    //    {
    //        if (this.SaveStreamNullable == null)
    //        {
    //            var aStream = this.ObjectStorage.NewBlopInputStream(this);
    //            return aStream;
    //        }
    //        else
    //        {
    //            var aStream = this.SaveStreamNullable;
    //            aStream.Seek(0, SeekOrigin.Begin);
    //            var aMemoryStream = new MemoryStream((int)aStream.Length);
    //            aStream.CopyTo(aMemoryStream);
    //            return aMemoryStream;
    //        }
    //    }

    //    private Stream SaveStreamNullable;
    //    internal void SetStream(Stream aStream)
    //    {
    //        this.SaveStreamNullable = aStream;
    //        this.Modify();
    //    }

    //    public long Length { get => this.ObjectStorage.GetBlopLength(this); }

    //    internal override void AcceptLoad()
    //    {
    //        if (this.SaveStreamNullable != null)
    //        {
    //            throw new Exception("Internal error.");
    //        }
    //        else
    //        {
    //            this.ObjectStorage.Load(this);
    //        }
    //    }

    //    internal override void AcceptSave()
    //    {
    //        this.ObjectStorage.VisitSave(this, this.SaveStreamNullable);
    //        this.SaveStreamNullable = null;
    //    }

    //}

    //public abstract class CNode : CObject, IDynamicMetaObjectProvider
    //{
    //    internal CNode(CObjectStorage aObjectStorage) : base(aObjectStorage)
    //    {
    //        this.TypeName = this.StaticTypeName;
    //        this.Guid = default(Guid);
    //    }
    //    public const string ObjectHttpQueryParam = "Object";
    //    internal CSchema Schema { get => this.ObjectStorage.Schema; }
    //    internal override bool IsStructureReflected => true;
    //    #region Properties
    //    internal override IEnumerable<CProperty> NewProperties()
    //    {
    //        foreach (var aProperty in base.NewProperties())
    //            yield return aProperty;
    //        yield return new CAttributeValueProperty(nameof(Guid), CXmlConverters.SaveGuid, CXmlConverters.LoadGuid);
    //        yield return new CAttributeValueProperty(nameof(TypeName), CXmlConverters.SaveString, CXmlConverters.LoadString);
    //    }
    //    private IEnumerable<CProperty> PropertiesM;
    //    internal IEnumerable<CProperty> Properties { get => CLazyLoad.Get(ref this.PropertiesM, () => this.NewProperties()); }
    //    internal CProperty GetPropertyByName(string aName) => (from aProperty in this.Properties where aProperty.Name == aName select aProperty).Single();
    //    #endregion
    //    internal override void AcceptLoad() => this.ObjectStorage.Load(this);
    //    internal override void AcceptSave()
    //    {
    //        this.ObjectStorage.VisitSave(this);
    //    }
    //    internal override CRef GetRefByGuid(Guid aGuid)
    //    {
    //        return (from aTest in this.Refs
    //                where aTest.RefOptions.RefGuid == aGuid
    //                select aTest).Single();
    //    }

    //    private readonly ExpandoObject ExpandoObject = new ExpandoObject();
    //    private IDictionary<string, object> ExpandoObjectValueDic => this.ExpandoObject;
    //    internal CNode ValueDic { get => this; }

    //    //  Vorsicht: Rückgabetyp nicht entfernen, sonst gibt es laufzetfehler mit dynamischer binding.Diese Methode wird an ein property.set gebunden. rückgabe von "A=0" ist 0.
    //    internal object SetValue(string aName, object aValue)
    //    {
    //        if (!this.ExpandoObjectValueDic.ContainsKey(aName)
    //        || !object.Equals(this.ExpandoObjectValueDic[aName], aValue))
    //        {
    //            this.ExpandoObjectValueDic[aName] = aValue;
    //            this.Modify();
    //        }
    //        return aValue;
    //    }
    //    internal object GetValue(string aName) => this.ExpandoObjectValueDic[aName];
    //    internal bool HasProperty(string aName) => this.ExpandoObjectValueDic.ContainsKey(aName);


    //    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression aParameter) => new CDynamicMetaObject(this, aParameter);
    //    internal dynamic Dynamic
    //    {
    //        get
    //        {
    //            return this;
    //        }
    //    }
    //    private sealed class CDynamicMetaObject : DynamicMetaObject
    //    {
    //        internal CDynamicMetaObject(CNode cStructuredObject, Expression aParameter) : base(aParameter, BindingRestrictions.Empty, cStructuredObject) { }
    //        public override DynamicMetaObject BindSetMember(SetMemberBinder aBinder, DynamicMetaObject aValue)
    //        {
    //            var aRestrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
    //            var aThis = Expression.Convert(this.Expression, this.LimitType);
    //            var aMethodInfo = typeof(CNode).GetMethod(nameof(CNode.SetValue), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    //            var aPropertyNameExpression = Expression.Constant(aBinder.Name);
    //            var aValueExpression = Expression.Convert(aValue.Expression, typeof(object));
    //            var aCallExpression = Expression.Call(aThis, aMethodInfo, aPropertyNameExpression, aValueExpression);
    //            var aResult = new DynamicMetaObject(aCallExpression, aRestrictions);
    //            return aResult;
    //        }

    //        public override DynamicMetaObject BindGetMember(GetMemberBinder aBinder)
    //        {
    //            var aRestrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
    //            var aThisExpression = Expression.Convert(this.Expression, this.LimitType);
    //            var aMethodInfo = typeof(CNode).GetMethod(nameof(CNode.GetValue), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    //            var aPropertyNameArg = Expression.Constant(aBinder.Name);
    //            var aCallExpression = Expression.Call(aThisExpression, aMethodInfo, aPropertyNameArg);
    //            var aResult = new DynamicMetaObject(aCallExpression, aRestrictions);
    //            return aResult;
    //        }
    //    }
    //    protected override void SaveTemplate()
    //    {
    //        foreach (var aComposition in this.Refs)
    //            aComposition.SaveCascade();
    //        base.SaveTemplate();
    //    }

    //    internal override void Create()
    //    {
    //        foreach (var aComposition in this.Refs)
    //            aComposition.CreateCascade();
    //        base.Create();
    //    }

    //    internal virtual IEnumerable<CRef> Refs { get => new CRef[] { }; }
    //    internal override void Delete()
    //    {
    //        foreach (var aComposition in this.Refs)
    //            aComposition.DeleteCascade();

    //        base.Delete();
    //    }

    //    public override Guid Guid
    //    {
    //        get
    //        {
    //            return this.Dynamic.Guid;
    //        }
    //        internal set
    //        {
    //            this.CheckNotCached();
    //            this.Dynamic.Guid = value;
    //        }
    //    }

    //    internal static string TypeAttributeName = nameof(TypeName);
    //    internal override string TypeName
    //    {
    //        get
    //        {
    //            return (string)this.Dynamic.TypeName;
    //        }
    //        set
    //        {
    //            base.TypeName = value;
    //            this.Dynamic.TypeName = value;
    //        }
    //    }

    //    private CType TypeM;


    //    internal CType Type
    //    {
    //        get
    //        {
    //            if (this.TypeM == null)
    //            {
    //                this.TypeM = this.Schema.TypeNameDic[this.StaticTypeName];
    //            }
    //            return this.TypeM;
    //        }
    //    }

    //    // this.Created = DateTime.Now;
    //    //yield return new CAttributeValueProperty(nameof(Created), CXmlConverters.SaveDateTime, CXmlConverters.LoadDateTime);
    //    //internal DateTime Created { get => this.Dynamic.Created; set => this.Dynamic.Created = value.ToUniversalTime(); }


    //    private const string ObjectElementName = CProperty.ObjectElementName;
    //    internal XmlElement NewXmlElement(XmlDocument aXmlDocument)
    //    {
    //        var aObject = this;
    //        var aObjectElement = aXmlDocument.CreateElement(ObjectElementName);
    //        var aType = this.Type;
    //        var aProperties = aType.Properties;
    //        foreach (var aProperty in aProperties.Values)
    //        {
    //            aProperty.Save(aObject, aXmlDocument, aObjectElement);
    //        }
    //        return aObjectElement;
    //    }
    //    internal XmlDocument NewXmlDocument()
    //    {
    //        var aXmlDocument = new XmlDocument();
    //        var aElement = this.NewXmlElement(aXmlDocument);
    //        aXmlDocument.AppendChild(aElement);
    //        return aXmlDocument;
    //    }


    //    internal static CObject New(CObjectStorage aObjectStorage, XmlElement aXmlElement)
    //    {
    //        var aTypeName = aXmlElement.GetAttribute(TypeAttributeName);
    //        var aType = aObjectStorage.Schema.TypeNameDic[aTypeName];
    //        var aObject = aType.NewObject(aObjectStorage);
    //        aObject.Load(aXmlElement);
    //        return aObject;
    //    }

    //    internal override void Load(XmlElement aObjectElement)
    //    {
    //        var aType = this.Type;
    //        var aObject = this;
    //        var aProperties = aType.Properties;
    //        var aValuesDic = this.ValueDic;
    //        foreach (var aProperty in aProperties.Values)
    //        {
    //            aProperty.Load(aObjectElement, aObject);
    //        }
    //    }
    //}

    //internal delegate void CCascadeAction();

    //public abstract class CRef
    //{
    //    internal CRef(CNode aParent, CRefOptions aRefOptions)
    //    {
    //        if (aRefOptions == null)
    //            throw new ArgumentException();
    //        this.Parent = aParent;
    //        this.RefOptions = aRefOptions;
    //    }

    //    internal readonly CNode Parent;
    //    internal readonly CRefOptions RefOptions;

    //    #region Cascade
    //    private void Execute(IEnumerable<CCascadeAction> aActions)
    //    {
    //        foreach (var aAction in aActions)
    //        {
    //            aAction();
    //        }
    //    }

    //    internal abstract IEnumerable<CObject> UnbufferedTargets { get; }

    //    internal void DeleteCascade()
    //    {
    //        if (this.RefOptions.Composition)
    //        {
    //            foreach (var aTarget in this.UnbufferedTargets)
    //            {
    //                aTarget.Delete();
    //            }
    //        }
    //    }
    //    internal abstract CObject CreateTarget();
    //    #endregion
    //}

    //public abstract class CxNRefOptions:CRefOptions
    //{
    //    protected CxNRefOptions(Type aParentSystemType, string aRefPropertyName, CCompositionEnum aComposition =  CCompositionEnum.Delete, CRefDirEnum aRefDirEnum = CRefDirEnum.ToChild) :base(aParentSystemType, aRefPropertyName, CAllowNull.NotApplicable, aComposition, aRefDirEnum)
    //    {
    //    }
    //}

    //public abstract class CXNRef : CRef  
    //{
    //    internal CXNRef(CNode aParent, CxNRefOptions aRefOptions):base(aParent, aRefOptions)
    //    {
    //        this.RefOptions = aRefOptions;
    //    }
    //    internal readonly new CxNRefOptions RefOptions;

    //}


    //internal sealed class CNRefOptions<TParent, TChild> : CRefOptions where TParent : CNode where TChild : CObject
    //{
    //    internal delegate CStructuredObjects CGetChildsFunc(TParent aParent);

    //    internal CNRefOptions(Type aParentSystemType,
    //                          string aRelationPropertyName,
    //                          CRefDirEnum aRelationDirectionEnum,                               
    //                          CGetChildsFunc aGetChildsFunc, 
    //                          bool aComposition, 
    //                          bool aEmbedd,
    //                          bool aSaveCascade
    //                          ) : base(aParentSystemType, aRelationDirectionEnum, aComposition, aEmbedd, aSaveCascade)
    //    {
    //        this.PropertyName = aPropertyName;
    //        this.GetChildsFunc = aGetChildsFunc;
    //    }
    //    internal readonly string PropertyName;
    //    //internal readonly bool Embed;
    //    internal readonly CGetChildsFunc GetChildsFunc;


    //}

    //internal abstract class CStructuredObjectsRef : CRef
    //{
    //    internal CStructuredObjectsRef(CNode aParent, CRefOptions aRefOptions) : base(aParent, aRefOptions)
    //    {
    //    }


    //    internal abstract void Add(CObject aItem);

    //    internal abstract void ClearWithoutDelete();

    //    internal abstract IEnumerable<CNode> StructuredObjects { get; }

    //}

    //public enum CRefDirEnum
    //{
    //    ToChild,
    //    ToParent,
    //    //Suspect
    //}

    //public enum CBufferedEnum
    //{
    //    Yes,
    //    No
    //}

    //public sealed class CGuidAttribute : Attribute
    //{
    //    public CGuidAttribute(string aGuid)
    //    {
    //        this.Guid = new Guid(aGuid);
    //    }
    //    internal readonly Guid Guid;
    //}

    //public enum CCompositionEnum
    //{
    //    Delete,
    //    NoDelete
    //}

    //public enum CAllowNull
    //{
    //    NotApplicable,
    //    AllowNull,
    //    AutoCreate
    //}

    //public class CRefOptions
    //{

    //    internal CRefOptions(Type aParentSystemType,
    //                         string aRefPropertyName,
    //                         CRefDirEnum aRelationDirectionEnum)
    //        : this(aParentSystemType, aRefPropertyName, CAllowNull.NotApplicable, CCompositionEnum.NoDelete, CRefDirEnum.ToParent)
    //    {
    //        if(aRelationDirectionEnum != CRefDirEnum.ToParent)
    //        {
    //            throw new ArgumentException();
    //        }
    //    }

    //    internal CRefOptions(Type aParentSystemType,
    //                         string aRefPropertyName,
    //                         CAllowNull aAllowNull = CAllowNull.AutoCreate,                             
    //                         CCompositionEnum aComposition = CCompositionEnum.Delete,
    //                         CRefDirEnum aRelationDirectionEnum = CRefDirEnum.ToChild
    //        )                             
    //    {
    //        if(aRelationDirectionEnum == CRefDirEnum.ToParent
    //        && aComposition == CCompositionEnum.Delete)
    //        {
    //            throw new ArgumentException();
    //        }
    //        this.ParentSystemType = aParentSystemType;
    //        this.RefPropertyName = aRefPropertyName;
    //        this.RelationDirection = aRelationDirectionEnum;
    //        this.Composition = aComposition;
    //    }
    //    internal readonly Type ParentSystemType;
    //    internal readonly string RefPropertyName;
    //    internal readonly CRefDirEnum RelationDirection;
    //    internal readonly bool Composition;

    //    public Guid RefGuid { get => this.ParentSystemType.GetProperty(this.RefPropertyName).GetCustomAttribute<CGuidAttribute>().Guid; }
    //}

    //public sealed class CR11Ref<T> : CRx1Ref<T> where T : CNode
    //{
    //    internal CR11Ref(CNode aParentObject,
    //                  string aGuidPropertyName,
    //                  CRefOptions aRefOptions = null
    //                  ) 
    //        : base(aParentObject, aGuidPropertyName, aRefOptions)
    //    {
    //    }


    //    internal override CObject CreateTarget()
    //    {
    //        return this.CreateTypedTarget();
    //    }
    //    public T CreateTypedTarget()
    //    {
    //        if(this.Target.GuidIsNull)
    //        {
    //            var aTarget = this.Parent.ObjectStorage.Create<T>();
    //            this.Target = aTarget;
    //            return aTarget;
    //        }
    //        else
    //        {
    //            throw new Exception("The target has allready been created.");
    //        }
    //    }

    //    //internal override void DetectCorruptions(CCorruptionCheck aCorruptionCollector)
    //    //{
    //    //    base.DetectCorruptions(aCorruptionCollector);

    //    //    if (this.Target.IsDeleted)
    //    //    {
    //    //        aCorruptionCollector.AddCorruption(this.Parent, this.GuidProperty, CR11TargetDeletedRepairTool.GuidConst);
    //    //    }
    //    //    if (this.RefOptions.CreateCascade)
    //    //    {
    //    //        if (this.Target.GuidIsNull)
    //    //        {
    //    //            aCorruptionCollector.AddCorruption(this.Parent, this.GuidProperty, CR11TargetNullRepairTool.GuidConst);

    //    //            var aTarget = this.Parent.ObjectStorage.Create<T>();
    //    //             this.Target = aTarget;
    //    //            this.UpdateRefGuid();
    //    //        }
    //    //    }
    //    //}        
    //}

    //public abstract class CR1NRefOptions : CRefOptions
    //{
    //    internal CR1NRefOptions(Type aParentSystemType,
    //                            string aRefPropertyName,
    //                            string aForeignKeyPropertyName,
    //                            CRefDirEnum aRefDir
    //                            )
    //        :base(aParentSystemType, aRefPropertyName, aRefDir, true, true)
    //    {
    //        this.ForeignKeyPropertyName = aForeignKeyPropertyName;
    //    }
    //    internal readonly string ForeignKeyPropertyName;        
    //}

    //public sealed class CR1NRefOptions<TParent, TChild> 
    //:
    //    CR1NRefOptions
    //    where TParent : CNode
    //    where TChild : CNode
    //{
    //    internal CR1NRefOptions(Type aParentSystemType,
    //                            string aRefPropertyName,
    //                            string aForeignKeyPropertyName, 
    //                            Func<TParent, CR1NRef<TParent, TChild>> aGetRef,
    //                            Func<TChild, Guid> aGetForeignKey,
    //                            Action<TChild, Guid> aSetForeignKey
    //                            ) : base()
    //    {
    //        this.GetRef = aGetRef;
    //        this.GetForeignKey = aGetForeignKey;
    //        this.SetForeignKey = aSetForeignKey;
    //    }

    //    internal readonly Func<TParent, CR1NRef<TParent, TChild>> GetRef;
    //    internal readonly Func<TChild, Guid> GetForeignKey;
    //    internal readonly Action<TChild, Guid> SetForeignKey;
    //}

    //public sealed class CR1NRef<TParent, TChild>
    //: 
    //    CRef,
    //    IEnumerable<TChild>
    //    where TParent : CNode
    //    where TChild : CNode
    //{
    //    internal CR1NRef(CNode aParent, CR1NRefOptions<TParent, TChild> aRefOptions):base(aParent, aRefOptions)
    //    {
    //        this.RefOptions = aRefOptions;
    //    }

    //    internal readonly new CR1NRefOptions<TParent, TChild> RefOptions;

    //    internal override IEnumerable<CObject> UnbufferedTargets => this.TypedUnbufferedTargets;

    //    private List<TChild> BufferedTargetsM;
    //    private List<TChild> BufferedTargets { get => CLazyLoad.Get(ref this.BufferedTargetsM, () => new List<TChild>(this.TypedUnbufferedTargets)); }
    //    internal IEnumerable<TChild> TypedUnbufferedTargets => this.Parent.ObjectStorage.LoadObjects(this.Parent, this.RefOptions);
    //    internal override CObject CreateTarget() => throw new NotImplementedException();
    //    public IEnumerator<TChild> GetEnumerator() => this.BufferedTargets.GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    //    internal TChild CreateItem()
    //    {
    //        var aNewItem = this.Parent.ObjectStorage.Create<TChild>();
    //        this.RefOptions.SetForeignKey(aNewItem, this.Parent.Guid);
    //        return aNewItem;
    //    }
    //    internal TChild Add()
    //    {
    //        var aNewItem = this.CreateItem();
    //        this.BufferedTargets.Add(aNewItem);
    //        return aNewItem;
    //    }
    //}

    //public abstract class CRx1RefOptions : CRefOptions
    //{
    //    protected CRx1RefOptions(Guid aRefGuid, Guid aForeignKeyPropertyGuid):base()
    //        {}
    //}

    //public abstract class CRx1Ref<T> : CRef where T : CObject
    //{
    //    internal CRx1Ref(CNode aParentObject,
    //                  CRefOptions aRefOptions = null
    //                  )
    //        : base(aParentObject, aRefOptions)
    //    {
    //    }
    //    internal CProperty GuidProperty { get => throw new NotImplementedException(); }
    //    internal void UpdateRefGuid()
    //    {
    //        this.Parent.SetValue(this.GuidProperty.Name, this.Target.Guid);
    //    }
    //    internal Guid RefGuid
    //    {
    //        get => (Guid)this.Parent.GetValue(this.GuidProperty.Name);
    //        set
    //        {
    //            this.Parent.SetValue(this.GuidProperty.Name, value);
    //            this.Refresh();
    //        }
    //    }
    //    internal override IEnumerable<CObject> UnbufferedTargets { get { yield return this.Target; } }
    //    private T TargetM;
    //    internal T Target
    //    {
    //        get => CLazyLoad.Get(ref this.TargetM, () => this.Parent.ObjectStorage.LoadObject<T>(this.RefGuid));
    //        set
    //        {
    //            this.Setup(value);
    //            this.TargetM = value;
    //            this.UpdateRefGuid();
    //        }
    //    }
    //    public T CreateOnDemand()
    //    {
    //        if(this.RefGuid == default(Guid))
    //        {
    //            if(this.Target.GuidIsNull)
    //            {
    //                var aObject = this.Parent.ObjectStorage.Create<T>();
    //                this.Target = aObject;
    //                return aObject;
    //            }
    //            else
    //            {
    //                throw new Exception("Internal error.");
    //            }
    //        }
    //        else
    //        {
    //            return this.Target;
    //        }
    //    }

    //    //internal void CreateOnDemand()
    //    //{
    //    //    if (this.Target.GuidIsNull)
    //    //    {
    //    //        this.Target.CreateGuid();
    //    //        this.UpdateRefGuid();
    //    //    }
    //    //}

    //    internal override IEnumerable<CCascadeAction> GetCreateCascadeactions()
    //    {
    //        foreach (var aAction in base.GetCreateCascadeactions())
    //            yield return aAction;
    //        yield return new CCascadeAction(delegate ()
    //        {
    //            this.UpdateRefGuid();
    //        });
    //    }

    //    internal void Refresh()
    //    {
    //        this.TargetM = null;
    //    }
    //}

    //internal sealed class CBlopRefOptions  : CRefOptions
    //{

    //    internal CBlopRefOptions(Type aParentType, string aRefPropertyName, bool aComposition = true):base(aParentType, aRefPropertyName, CRefDirEnum.ToChild, aComposition) { }
    //}

    //internal sealed class CBlopRef : CRx1Ref<CBlop> 
    //{

    //    internal CBlopRef(CNode aParentObject, CBlopRefOptions aBlopRefOptions) : base(aParentObject, aBlopRefOptions)
    //    {
    //    }
    //    internal override CObject CreateTarget()=> throw new InvalidOperationException();
    //}

    //internal sealed class CFileExtensionToHttpContentType
    //{
    //    internal static CFileExtensionToHttpContentType Singleton = new CFileExtensionToHttpContentType();

    //    private Dictionary<string, string> Dic = new Dictionary<string, string>();
    //    internal CFileExtensionToHttpContentType()
    //    {
    //        this.Dic.Add(".jpg", "image/jpeg");
    //        this.Dic.Add(".jpeg", "image/jpeg");
    //        this.Dic.Add(".zip", "text/plain");
    //    }
    //    internal string GetContentType(string aExtension)
    //    {
    //        aExtension = aExtension.ToLower();
    //        if (this.Dic.ContainsKey(aExtension))
    //        {
    //            return this.Dic[aExtension];
    //        }
    //        return "text/plain";
    //        //throw new ArgumentException("No content type for file extension: '" + aExtension + "'.");
    //    }
    //}
}
