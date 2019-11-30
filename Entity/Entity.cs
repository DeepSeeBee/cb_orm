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
        /// TODO - Remove this hack.
        internal static readonly Guid DeletedObjectGuid = new Guid("00000000-beef-dead-beef-000000000000");
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
            if(this.IsNull)
            {
                this.Seal();
            }
        }

        private bool Sealed { get; set; }
        private void Seal()
        {
            this.Sealed = true;
        }
        private void CheckNotSealed()
        {
            if(this.Sealed)
            {
                throw new InvalidOperationException();
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
                // Nicht cachen, erst wenn CreateAufrruf erfolgt.
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

        internal bool IsLocallyDeleted { get; private set; }
        private void DeleteCascade()
        {
            foreach (var aRef in this.Refs)
                aRef.DeleteCascade();
        }
        public void Delete()
        {
            this.DeleteCascade();
            this.IsLocallyDeleted = true;
            this.Modify();            
        }
        internal void CreateGuid()
        {
            this.CheckNotCached();
            this.GuidValue = this.Storage.NewObjectId();
        }
        public bool IsNull { get => this.GuidValue == new Guid(); }
        public bool IsRemoteDeleted { get => this.GuidValue == DeletedObjectGuid; }
        internal void CheckNotGuidIsNull()
        {
            if (this.IsNull)
            {
                throw new Exception(this.StaticTypName + " not found.");
            }
        }

        internal void CreateGuidOnDemand()
        {
            if (this.IsNull)
            {
                this.CreateGuid();
            }
        }
        private void CreateCascade()
        {
            foreach (var aComposition in this.Refs)
                aComposition.CreateCascade();
        }
        internal void Create()
        {
            this.CheckNotSealed();
            if (this.IsNull)
            {
                this.TypNameValue = this.StaticTypName;
                this.CreateGuid();
                this.AddToCache();
                this.CreateCascade();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        internal virtual void CreateRelationsOnDemand()
        {
        }

        internal virtual bool SaveIsOk { get => true; }

        internal virtual IEnumerable<CRef> Refs
        {
            get
            {
                var aObject = this;
                var aTyp = this.Typ;
                var aTyps = this.Schema.GetHirarchy(this.Typ);
                var aRefs = from aTest in aTyps from aProperty in aTest.Properties select aProperty.GetRef(aObject);
                return aRefs;
            }
        }
    }

    public static class CEntityObjectUtil
    {
        public static void StoreNewRelationsOnDemand(CEntityObject aEntityObject)
        {
            aEntityObject.CreateRelationsOnDemand();
            if(aEntityObject.IsModified)
            {
                aEntityObject.Storage.Save();
            }
        }

        public static CEntityObject GetParentNullable(CEntityObject aEntityObject)
            => (from aRef in aEntityObject.Refs
                         where aRef.IsCardinalityToParent
                         where !((CEntityObject)aRef.ValueObj).IsNull
                         select ((CEntityObject)aRef.ValueObj)).SingleOrDefault();
        public static IEnumerable<CEntityObject> GetPath(CEntityObject aEntityObject)
        {
            var aParent = GetParentNullable(aEntityObject);
            if(!aParent.IsNullRef())
                foreach (var aParentPathItem in GetPath(aParent))
                    yield return aParentPathItem;
            yield return aEntityObject;
            
        }
        public static T FindParentInPath<T>(this CEntityObject aEntityObject) where T : CEntityObject
        {
            var aPath = GetPath(aEntityObject).OfType<T>();
            var aParent = aPath.IsEmpty()
                        ? aEntityObject.Storage.CreateNullObject<T>()
                        : aPath.Last()
                        ;
            return aParent;
        }
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

        public virtual CSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        internal override void CreateRelationsOnDemand()
        {
            base.CreateRelationsOnDemand();
            foreach(var aRef in this.Refs)
            {
                aRef.CreateRelationsOnDemand();
            }
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
                this.Guid.ChangeValue(value, GuidWriteKey);
            }
        }
        #endregion
        #region TypeName
        private static readonly CSkalarRefMetaInfo _TypNameMetaInfoM = new CSkalarRefMetaInfo(typeof(CEntityObject), nameof(TypName));
        public static CSkalarRefMetaInfo _TypNameMetaInfo { get => _TypNameMetaInfoM; }
        private readonly CAccessKey TypNameWriteKey = new CAccessKey();
        private CSkalarRef<CEntityObject, string> TypNameM;
        //[CSpreadAcrossTables(true)]
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
                this.TypName.ChangeValue(value, TypNameWriteKey);
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
    }

     
}
