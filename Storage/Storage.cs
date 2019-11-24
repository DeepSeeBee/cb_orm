// This is the runtime for my 2nd generation ORM-Wrapper. (database adapter)

using CbOrm.Blop;
using CbOrm.Di;
using CbOrm.Entity;
using CbOrm.Meta;
using CbOrm.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CbOrm.Storage
{
    public abstract class CStorage
    {

        public readonly CDiContainer DiContainer = new CDiContainer();
        protected CStorage(CSchema aSchema)
        {
            this.Schema = aSchema;
        }

        public readonly CSchema Schema;

        internal readonly Dictionary<Guid, CObject> Cache = new Dictionary<Guid, CObject>();

        internal abstract bool R1NCContainsChildList { get; }

        public int Save()
        {
            var aCount = 0;
            var aObjects = this.Cache.Values;
            var aModifiedObjects = from aObject in aObjects
                                   where aObject.IsModified
                                   select aObject;
            foreach (var aObject in aModifiedObjects.ToArray())
            {
                aObject.SaveSingleObject();
                ++aCount;
            }
            return aCount;
        }

        protected abstract void Load(CEntityObject aEntityObject, CTyp aAspect);

        internal void VisitLoad(CEntityObject aEntityObject)
        {
            var aTyp = aEntityObject.Typ;
            var aSchema = this.Schema;
            var aHirarchy = aSchema.GetHirarchy(aTyp);
            foreach (var aAspect in aHirarchy)
            {
                this.Load(aEntityObject, aAspect);
            }
        }
        public abstract void Load(CBlop aBlop);

        protected CObject LoadOnDemand(Guid aGuid, Func<CObject> aLoad)
        {
            if (this.Cache.ContainsKey(aGuid))
            {
                return this.Cache[aGuid];
            }
            else
            {
                var aLoaded = aLoad();
                aLoaded.AddToCache();
                return aLoaded;
            }
        }
        public T LoadObject<T>(Guid aGuid) where T : CObject
        {
            return (T)this.LoadOnDemand(aGuid,
                () =>
                {
                    var aObject = this.NewObject<T>();
                    aObject.GuidValue = aGuid;
                    aObject.Load();
                    return aObject;
                });
        }
        public IEnumerable<T> LoadObjects<T>() where T : CObject => this.LoadObjects(typeof(T)).Cast<T>();
        public abstract IEnumerable<CObject> LoadObjects(CTyp aType);
        public abstract CObject LoadObject(CTyp aType, Guid aGuid);

        public CObject LoadObject(string aTypeName, Guid aGuid)
        {
            return this.LoadObject(this.Schema.Typs.GetByName(aTypeName), aGuid);
        }

        public IEnumerable<CObject> LoadObjects(Type aSystemType)
        {
            var aType = this.Schema.Typs.GetBySystemType(aSystemType);
            var aObjects = this.LoadObjects(aType);
            return aObjects;
        }

        internal T NewObject<T>() where T : CObject
        {
            return (T)this.Schema.Typs.GetBySystemType(typeof(T)).NewObject(this);
        }

        public T CreateObject<T>() where T : CObject
        {
            var aObj = this.NewObject<T>();
            aObj.Create();
            return aObj;
        }
        public T CreateNullObject<T>() where T : CObject => this.NewObject<T>();

        internal abstract long GetBlopLength(CBlop aBlop);

        internal abstract Stream NewBlopInputStream(CBlop aBlop);

        protected abstract CBlopOutputStream NewBlopOutputStream(CBlop aBlop);

        internal void VisitSave(CEntityObject aEntityObject)
        {
            var aTyp = aEntityObject.Typ;
            var aSchema = this.Schema;
            var aHirarchy = aSchema.GetHirarchy(aTyp);
            foreach(var aAspect in aHirarchy)
            {
                this.Save(aEntityObject, aAspect);
            }
        }
        protected abstract void Save(CEntityObject aEntityObject, CTyp aAspect);
        protected abstract void Delete(CBlop aBlop);
        internal void VisitSave(CBlop aBlop, Stream aStream)
        {
            if (aBlop.IsLocallyDeleted)
            {
                this.Delete(aBlop);
            }
            else
            {
                using (var aBlopOutputStream = this.NewBlopOutputStream(aBlop))
                {
                    aStream.CopyTo(aBlopOutputStream.Stream);
                    aBlopOutputStream.Commit();
                }
            }
        }

        internal abstract Guid NewObjectId();
        public abstract CStorage CloneStorage(bool aConnect);

    }

    /// TODO
    internal sealed class CObjectDeletedException : Exception
    {

    }
}
