// This is the runtime for my 2nd generation ORM-Wrapper. (entity object sub system)

using CbOrm.Attributes;
using CbOrm.Meta;
using CbOrm.Ref;
using CbOrm.Schema;
using CbOrm.Storage;
using CbOrm.Util;
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

namespace CbOrm.Entity
{
 
    public abstract class CObject
    {
        public CObject(CStorage aStorage)
        {
            this.Storage = aStorage;
        }
        public readonly CStorage Storage;
        internal abstract bool IsStructureReflected { get; }
        internal bool IsModified { get; private set; }
        internal void Modify() { this.IsModified = true; }
        internal void Unmodify() { this.IsModified = false; }
        internal virtual IEnumerable<CRefMetaInfo> NewProperties()
        {
            return new CRefMetaInfo[] { };
        }

        internal static readonly Guid DeletedObjectGuid = new Guid("00000000-dead-dead-dead-000000000000");
        private string StaticTypName { get => this.Typ.Name; }
        internal virtual string TypNameValue
        {
            get => this.StaticTypName;
            set
            {
                if (this.StaticTypName != value)
                    throw new InvalidOperationException();
            }
        }
        public abstract Guid GuidValue { get; internal set; }

        public const string ObjectHttpQueryParam = "Object";
        internal CSchema Schema { get => this.Storage.Schema; }
        internal abstract void AcceptSave();

        internal abstract void Load(XmlElement aObjectElement);
        internal abstract void AcceptLoad();
        internal virtual void LoadTemplate() => this.AcceptLoad();

        internal void Load()
        {
            var aObject = this;
            if (aObject.GuidValue != default(Guid)
            && aObject.GuidValue != CObject.DeletedObjectGuid)
            {
                this.LoadTemplate();
                this.Unmodify();
            }
        }

        #region Cache
        internal bool IsCached { get; private set; }
        internal void CheckNotCached()
        {
            if (this.IsCached) { throw new InvalidOperationException(); }
        }
        internal void AddToCache()
        {
            this.CheckNotCached();
            if (this.GuidValue == default(Guid))
            {
                // Nicht cachen, erst wenn CreateAufrruf erfolgt.s
            }
            else if (this.IsRemoteDeleted)
            {
                this.IsCached = true;
            }
            else
            {
                this.Storage.Cache.Add(this.GuidValue, this);
                this.IsCached = true;
            }

        }
        #endregion
        public abstract CTyp Typ
        {
            get;
        }
        protected virtual void SaveTemplate() => this.AcceptSave();

        internal void SaveSingleObject()
        {
            if (!this.SaveIsOk)
            {

            }
            else
            {
                this.SaveTemplate();
                this.Unmodify();
            }
        }

        public void SaveAllModifiedObjects()
        {
            this.Storage.Save();
        }

        internal bool IsLocallyDeleted { get; private set; }

        internal virtual void Delete()
        {
            this.IsLocallyDeleted = true;
            this.Modify();
        }
        internal bool IsPersistent { get => this.Storage.IsPersistent(this); }

        internal void CreateGuid()
        {
            this.CheckNotCached();
            this.GuidValue = this.Storage.NewObjectId();
        }
        public bool GuidIsNull { get => this.GuidValue == new Guid(); }
        public bool IsRemoteDeleted { get => this.GuidValue == DeletedObjectGuid; }
        internal void CheckNotGuidIsNull()
        {
            if (this.GuidIsNull)
            {
                throw new Exception(this.StaticTypName + " not found.");
            }
        }

        internal void CreateGuidOnDemand()
        {
            if (this.GuidIsNull)
            {
                this.CreateGuid();
            }
        }

        internal virtual void Create()
        {
            if (this.GuidIsNull)
            {
                this.TypNameValue = this.StaticTypName;
                this.CreateGuid();
                this.AddToCache();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        internal virtual bool SaveIsOk { get => true; }
    }

    public abstract class CEntityObject 
    : 
        CObject
    {
        public CEntityObject(CStorage aStorage) : base(aStorage)
        {
        }

        public virtual IEnumerable<CRefMetaInfo> GetProperties()
        {
            return new CRefMetaInfo[] { };
        }


        #region Typ
        private static CTyp _CEntityObject_TypM = new CTyp(typeof(CEntityObject), new Guid("59378a74-66be-4538-bd5a-635ce07eab10"), CEntityObject._GetProperties);
        public static CTyp _CEntityObject_Typ { get => _CEntityObject_TypM; }
        private static void _GetProperties(Action<CRefMetaInfo> aAddProperty)
        {
            aAddProperty(_GuidMetaInfo);
            aAddProperty(_TypNameMetaInfo);
        }
        #endregion


        internal override bool IsStructureReflected => true;
        #region Properties
        private IEnumerable<CRefMetaInfo> PropertiesM;
        internal IEnumerable<CRefMetaInfo> Properties { get => CLazyLoad.Get(ref this.PropertiesM, () => this.NewProperties()); }
        internal CRefMetaInfo GetPropertyByName(string aName) => throw new NotImplementedException(); // (from aProperty in this.Properties where aProperty.Name == aName select aProperty).Single();
        #endregion
        internal override void AcceptLoad() => this.Storage.VisitLoad(this);
        internal override void AcceptSave()=> this.Storage.VisitSave(this);
        //internal override CRef GetRefByGuid(Guid aGuid)
        //{
        //    return (from aTest in this.Refs
        //            where aTest.RefOptions.RefGuid == aGuid
        //            select aTest).Single();
        //}

        internal override void Create()
        {
            foreach (var aComposition in this.Refs)
                aComposition.CreateCascade();
            base.Create();
        }

        internal virtual IEnumerable<CRef> Refs { get => new CRef[] { }; }
        internal override void Delete()
        {
            foreach (var aComposition in this.Refs)
                aComposition.DeleteCascade();

            base.Delete();
        }

        public virtual CSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        #region Guid
        private static CSkalarRefMetaInfo _GuidMetaInfoM = new CSkalarRefMetaInfo(typeof(CEntityObject), nameof(Guid));
        public static CSkalarRefMetaInfo _GuidMetaInfo { get => _GuidMetaInfoM; }
        private CSkalarRef<CEntityObject, Guid> GuidM;
        private static CAccessKey GuidWriteKey = new CAccessKey();
        [CSpreadAcrossTables(true)]
        public CSkalarRef<CEntityObject, Guid> Guid { get => CLazyLoad.Get(ref GuidM, () => new CSkalarRef<CEntityObject, Guid>(this, _GuidMetaInfo, GuidWriteKey)); }
        public override Guid GuidValue
        {
            get => this.Guid.Value;
            internal set
            {
                this.Guid.SetValue(value, GuidWriteKey);
            }
        }
        #endregion
        #region TypeName
        //internal static string TypeAttributeName = nameof(TypName);

        private static readonly CSkalarRefMetaInfo _TypNameMetaInfoM = new CSkalarRefMetaInfo(typeof(CEntityObject), nameof(TypName));
        public static CSkalarRefMetaInfo _TypNameMetaInfo { get => _TypNameMetaInfoM; }
        private readonly CAccessKey TypNameWriteKey = new CAccessKey();
        private CSkalarRef<CEntityObject, string> TypNameM;
        [CSpreadAcrossTables(true)]
        public CSkalarRef<CEntityObject, string> TypName
        {
            get => CLazyLoad.Get(ref this.TypNameM, () => new CSkalarRef<CEntityObject, string>(this, _TypNameMetaInfo, TypNameWriteKey));
        }
        internal override string TypNameValue
        {
            get=> this.TypName.Value;
            set
            {
                base.TypNameValue = value;
                this.TypName.SetValue(value, TypNameWriteKey);
            }
        }
        #endregion
        private const string ObjectElementName = CRefMetaInfo.ObjectElementName;
        internal XmlElement NewXmlElement(XmlDocument aXmlDocument, IEnumerable<CRefMetaInfo> aProperties)
        {
            var aObject = this;
            var aXmlElement = aXmlDocument.CreateElement(ObjectElementName);
            var aType = this.Typ;
            foreach (var aProperty in aProperties)
            {
                aProperty.SaveXml(aObject, aXmlDocument, aXmlElement);
            }
            return aXmlElement;
        }
        internal XmlDocument NewXmlDocument(IEnumerable<CRefMetaInfo> aProperties)
        {
            var aXmlDocument = new XmlDocument();
            var aElement = this.NewXmlElement(aXmlDocument, aProperties);
            aXmlDocument.AppendChild(aElement);
            return aXmlDocument;
        }
        internal override void Load(XmlElement aObjectElement)
        {
            throw new NotImplementedException();
            //var aType = this.Type;
            //var aObject = this;
            //var aProperties = aType.Properties;
            //var aValuesDic = this.ValueDic;
            //foreach (var aProperty in aProperties.Values)
            //{
            //    aProperty.Load(aObjectElement, aObject);
            //}
        }


    }

     
}
