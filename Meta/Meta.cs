// This is the runtime for my 2nd generation ORM-Wrapper. (meta layer)

using CbOrm.Entity;
using CbOrm.Ref;
using CbOrm.Schema;
using CbOrm.Storage;
using CbOrm.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CbOrm.Meta
{
    using System.Xml;
    using CGetPropertiesFunc = Action<Action<CRefMetaInfo>>;

    public struct CIdentifier
    {
        public CIdentifier(string aName, Guid aGuid)
        {
            this.Name = aName;
            this.Guid = aGuid;
        }
        public override int GetHashCode()
        {
            return this.Guid.GetHashCode();
        }
        public static bool operator ==(CIdentifier lhs, CIdentifier rhs) => lhs.Guid == rhs.Guid && lhs.Name == rhs.Name;
        public static bool operator !=(CIdentifier lhs, CIdentifier rhs) => !(lhs == rhs);
        public override bool Equals(object obj)
        {
            if (obj is CIdentifier)
                return this == (CIdentifier)obj;
            return false;
        }
        public readonly string Name;
        public readonly Guid Guid;
    }

    internal class CIdentifiers<T> : IEnumerable<T>
    {
        public delegate CIdentifier CGetIdFunc(T aItem);
        private CGetIdFunc GetIdFunc;

        internal CIdentifiers(IEnumerable<T> aItems, CGetIdFunc aGetIdFunc)
        {
            this.Items = aItems.ToArray();
            this.GetIdFunc = aGetIdFunc;
        }

        internal void Init()
        {
            foreach (var aItem in this.Items)
                this.Add(aItem);
        }

        internal virtual void Add(T aItem)
        {
            var aId = this.GetIdFunc(aItem);
            this.NameDic.Add(aId.Name, aItem);
            if (aId.Guid != default(Guid))
                this.GuidDic.Add(aId.Guid, aItem);
        }


        internal static CIdentifiers<T> New(Action<Action<T>> aAddItems, CGetIdFunc aGetId)
        {
            var aItems = new List<T>();
            aAddItems(aItems.Add);
            var aIds = new CIdentifiers<T>(aItems, aGetId);
            aIds.Init();
            return aIds;
        }

        internal readonly IEnumerable<T> Items;
        internal readonly Dictionary<string, T> NameDic = new Dictionary<string, T>();
        internal readonly Dictionary<Guid, T> GuidDic = new Dictionary<Guid, T>();

        public IEnumerator<T> GetEnumerator() => this.Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        internal T GetByName(string aName)
        {
            return this.NameDic[aName];
        }
    }

    internal sealed class CTyps : CIdentifiers<CTyp>
    {
        internal CTyps(IEnumerable<CTyp> aTyps) : base(aTyps, aTyp => aTyp.Id)
        {
        }
        private readonly Dictionary<Type, CTyp> SystemTypeDic = new Dictionary<Type, CTyp>();
        internal override void Add(CTyp aItem)
        {
            base.Add(aItem);
            this.SystemTypeDic.Add(aItem.SystemType, aItem);
        }

        public bool ContainsKey(Type aSystemType) => this.SystemTypeDic.ContainsKey(aSystemType);

        public CTyp GetBySystemType(Type aSystemType) => this.SystemTypeDic[aSystemType];
    }



    public sealed class CTyp
    {
        internal CTyp(Type aSystemType, Guid aGuid, CGetPropertiesFunc aGetPropertiesFunc)
        {
            this.SystemType = aSystemType;
            this.Id = new CIdentifier(this.Name, aGuid);
            this.GetPropertiesFunc = aGetPropertiesFunc;
        }

        private readonly CGetPropertiesFunc GetPropertiesFunc;

        private CIdentifiers<CRefMetaInfo> PropertiesM;
        internal CIdentifiers<CRefMetaInfo> Properties { get => CLazyLoad.GetLocked(this, ref this.PropertiesM, () => CIdentifiers<CRefMetaInfo>.New(this.GetPropertiesFunc, aProp => aProp.Id)); }
        internal string TableName { get => this.SystemType.Name; }
        public string Name { get => this.SystemType.Name; }
        internal readonly Type SystemType;
        internal readonly CIdentifier Id;

        internal CObject NewObject(CStorage aStorage)
        {
            var aObject = (CObject)Activator.CreateInstance(this.SystemType, new object[] { aStorage });
            aObject.Unmodify();
            return aObject;
        }
        //private Func<CSchema> GetSchema;
        //internal CSchema Schema { get=>this.GetSchema(); }
    }


    public abstract class CRefMetaInfo
    {
        public CRefMetaInfo(Type aOwnerType,
                             string aPropertyName
                             )
        {
            this.OwnerType = aOwnerType;
            this.PropertyInfo = aOwnerType.GetProperty(aPropertyName);
            this.Id = new CIdentifier(aPropertyName, default(Guid));
        }
        public readonly Type OwnerType;
        public readonly PropertyInfo PropertyInfo;
        internal const string ObjectElementName = "Object";
        internal readonly CIdentifier Id;
        internal CCollection<T> NewCollection<T>() => new CListCollection<T>();
        public bool IsDefined<TAttribute>() where TAttribute : Attribute => this.PropertyInfo.IsDefined(typeof(TAttribute));
        public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute => this.PropertyInfo.GetCustomAttributes<TAttribute>().Last();
        internal abstract void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement);
        internal CRef GetRef(CObject aObject) => (CRef)this.PropertyInfo.GetValue(aObject);
        internal abstract void LoadXml(CObject aEntityObject, XmlElement aXmlElement);
    }
    public sealed class CSkalarRefMetaInfo : CRefMetaInfo
    {
        public CSkalarRefMetaInfo(Type aOwnerType,
                              string aPropertyName
                            ) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            var aProperty = this;
            var aRef = aProperty.GetRef(aObject);
            var aValue = aRef.ValueObj;
            var aValueType = aRef.ValueType;
            var aSchema = aObject.Schema;
            var aSaveConverter = aSchema.GetSaveXmlConverter(aValueType);
            var aXmlValue = aSaveConverter.Convert<string>(aValue);
            var aPropertyName = aProperty.Id.Name;
            aElement.SetAttribute(aPropertyName, aXmlValue.ToString());
        }
        internal override void LoadXml(CObject aObject, XmlElement aXmlElement)
        {
            var aProperty = this;
            var aRef = aProperty.GetRef(aObject);
            var aPropertyName = aProperty.Id.Name;
            var aSchema = aObject.Schema;
            var aValueType = aRef.ValueType;
            var aSaveConverter = aSchema.GetSaveXmlConverter(aValueType);
            var aXmlValue = aXmlElement.GetAttribute(aPropertyName);
            var aModelValue = aSaveConverter.ConvertBack(aXmlValue);
            aRef.SetValueObj(aModelValue, aRef.WriteKeyNullable);

        }
    }
    public abstract class CObjectRefMetaInfo : CRefMetaInfo
    {
        public CObjectRefMetaInfo(Type aOwnerType,
                      string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }

    }
    public sealed class CBlopRefMetaInfo : CObjectRefMetaInfo
    {
        public CBlopRefMetaInfo(Type aOwnerType,
                  string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class CR11CRefMetaInfo : CRefMetaInfo
    {
        public CR11CRefMetaInfo(Type aOwnerType,
                  string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class CR11WRefMetaInfo : CRefMetaInfo
    {
        public CR11WRefMetaInfo(Type aOwnerType,
                  string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class CR11PRefMetaInfo : CRefMetaInfo
    {
        public CR11PRefMetaInfo(Type aOwnerType,
                  string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class CR1NCRefMetaInfo : CRefMetaInfo
    {
        public CR1NCRefMetaInfo(Type aOwnerType,
                            string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class CR1NWRefMetaInfo : CRefMetaInfo
    {
        public CR1NWRefMetaInfo(Type aOwnerType,
                            string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class CR1NPRefMetaInfo : CRefMetaInfo
    {
        public CR1NPRefMetaInfo(Type aOwnerType,
                  string aPropertyName) : base(aOwnerType, aPropertyName)
        {
        }
        internal override void SaveXml(CObject aObject, XmlDocument aXmlDocument, XmlElement aElement)
        {
            throw new NotImplementedException();
        }
        internal override void LoadXml(CObject aEntityObject, XmlElement aXmlElement)
        {
            throw new NotImplementedException();
        }
    }
}
