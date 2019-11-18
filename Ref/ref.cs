// This is the runtime for my 2nd generation ORM-Wrapper. (entity relation sub system)

using CbOrm.Attributes;
using CbOrm.Entity;
using CbOrm.Meta;
using CbOrm.Schema;
using CbOrm.Storage;
using CbOrm.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Ref
{
    public sealed class CAccessKey
    {
        public CAccessKey()
        { }
    }


    public abstract class CRef
    {
        protected CRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable)
        {
            this.ParentEntityObject = aParentEntityObject;
            this.RefMetaInfo = aRefMetaInfo;
            this.WriteKeyNullable = aWriteKeyNullable;
        }


        internal virtual void CreateCascade()
        {
            throw new NotImplementedException();
        }

        internal void DeleteCascade()
        {
            throw new NotImplementedException();
        }


        protected void CheckWriteable(CAccessKey aWriteKey)
        {
            if(!this.WriteKeyNullable.IsNullRef()
            && !object.ReferenceEquals(this.WriteKeyNullable, aWriteKey))
            {
                throw new InvalidOperationException("Property is write protected.");
            }
        }

        internal readonly CEntityObject ParentEntityObject;
        internal readonly CRefMetaInfo RefMetaInfo;
        internal readonly CAccessKey WriteKeyNullable;

        public abstract object ValueObj { get; }
        internal Type ValueType { get=>this.ValueObj.IsNullRef() ? this.DeclaredValueType : this.ValueObj.GetType(); }
        internal abstract Type DeclaredValueType { get; }
        public abstract Type DeclaredTargetType { get; }

        internal abstract void SetValueObj(object aModelValue, CAccessKey writeKeyNullable);
        public CStorage Storage { get => this.ParentEntityObject.Storage; }
        public CSchema Schema { get => this.Storage.Schema; }
        internal abstract IEnumerable<Guid> TargetGuids { get; }
        internal abstract void Load(IEnumerable<Guid> aGuids);
    }

    public abstract class CRef<T> : CRef
    {
        public CRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }

        public T Value
        {
            get => this.ValueM;
            set
            {
                this.SetValue(value);
            }
        }
        public override object ValueObj => this.Value;

        public void SetValue(T aValue, CAccessKey aWriteKeyNullable = null, bool aModify = true)
        {
            this.CheckWriteable(aWriteKeyNullable);
            if (!object.Equals(aValue, this.ValueM))
            {
                this.ValueM = aValue;
                if (aModify)
                {
                    this.ParentEntityObject.Modify();
                }
            }
        }
        internal override void SetValueObj(object aModelValue, CAccessKey aWriteKeyNullable)
        {
            this.SetValue((T)aModelValue, aWriteKeyNullable);
        }
        private T ValueM;
        internal override Type DeclaredValueType => typeof(T);

    }

    internal sealed class CObjectProxy<T> where T: CObject
    {
        internal CObjectProxy(CStorage aStorage, Guid aGuid)
        {
            this.Storage = aStorage;
            this.Guid = aGuid;
        }
        internal CObjectProxy(T aObject) : this(aObject.Storage, aObject.GuidValue)
        {
            this.Object = aObject;
        }

        private readonly CStorage Storage; // if using viewmodels may use a  weakref here.
        internal readonly Guid Guid; 
        private T ObjectM;
        internal T Object
        {
            get => CLazyLoad.Get(ref this.ObjectM, () => this.Storage.LoadObject<T>(this.Guid));
            private set => this.ObjectM = value;
        }
        public override int GetHashCode() => this.Guid.GetHashCode();
        public override bool Equals(object obj)=> obj is CObjectProxy<T> ? this == (CObjectProxy<T>)obj : false;
        public static bool operator ==(CObjectProxy<T> lhs, CObjectProxy<T> rhs) => lhs.Guid == rhs.Guid;
        public static bool operator !=(CObjectProxy<T> lhs, CObjectProxy<T> rhs) => !(lhs == rhs);

    }

    public abstract class CCollection<T> : IEnumerable<T>
    {
        internal abstract void Add(T aItem);
        internal abstract void Remove(T aItem);
        internal abstract IEnumerable<T> Items{ get; }
        public IEnumerator<T> GetEnumerator() => this.Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        internal abstract void Load(CStorage aStorage, IEnumerable<Guid> aGuids);
        internal abstract IEnumerable<Guid> Guids { get; }
    }

    public sealed class CListCollection<T> : CCollection<T> where T :CObject
    {
        private readonly List<CObjectProxy<T>> List = new List<CObjectProxy<T>>();
        internal override void Add(T aItem)
        {
            this.List.Add(new CObjectProxy<T>(aItem));
        }
        internal override void Remove(T aItem)
        {
            this.List.Remove(new CObjectProxy<T>(aItem));
        }
        internal override IEnumerable<T> Items => from aItem in this.List select aItem.Object;
        internal override void Load(CStorage aStorage, IEnumerable<Guid> aGuids)
        {
            foreach(var aGuid in aGuids)
            {
                this.List.Add(new CObjectProxy<T>(aStorage, aGuid));
            }
        }
        internal override IEnumerable<Guid> Guids => from aItem in this.List select aItem.Guid;
    }


    public abstract class CNRef<TRef> 
    : 
        CRef<IEnumerable<TRef>>
    ,   IEnumerable<TRef> 
        where TRef :CEntityObject
    {
        protected CNRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo) : base(aParentEntityObject, aRefMetaInfo, new CAccessKey())
        {
            this.Collection = aRefMetaInfo.NewCollection<TRef>();
            this.SetValue(this.Collection, this.WriteKeyNullable, false);
        }
        protected readonly CCollection<TRef> Collection;
        protected void SetForeignKey(TRef aObject)
        {
            var aParentId = this.ParentEntityObject.Guid.Value;
            this.SetForeignKey(aObject, aParentId);
        }
        internal override IEnumerable<Guid> TargetGuids => this.Collection.Guids;
        public override Type DeclaredTargetType => typeof(TRef);

        private CTyp TargetTypM;
        internal CTyp TargetTyp { get => CLazyLoad.Get(ref this.TargetTypM, () => this.Schema.Typs.GetBySystemType(this.DeclaredTargetType)); }

        internal CSkalarRefMetaInfo ForeignKeyMetaInfoM;
        private CSkalarRefMetaInfo ForeignKeyMetaInfo { get => CLazyLoad.Get(ref this.ForeignKeyMetaInfoM,
                                                             () => (from aProperty in this.TargetTyp.Properties
                                                                    where aProperty.IsDefined<CForeignKeyParentTypeAttribute>()
                                                                    where aProperty.IsDefined<CForeignKeyParentPropertyNameAttribute>()
                                                                    where aProperty.GetAttribute<CForeignKeyParentTypeAttribute>().Value == this.RefMetaInfo.OwnerType
                                                                    where aProperty.GetAttribute<CForeignKeyParentPropertyNameAttribute>().Value == this.RefMetaInfo.PropertyInfo.Name
                                                                    select aProperty).Cast<CSkalarRefMetaInfo>().Single()
                                                             );
                                                              } /// TODO_OPT


        protected void SetForeignKey(TRef aObject, Guid aForeignKey)
        {
            var aRef = this;
            var aForeignKeyMetaInfo = aRef.ForeignKeyMetaInfo;
            var aForeignKeyRef = aForeignKeyMetaInfo.GetRef(aObject);
            aForeignKeyRef.SetValueObj(aForeignKey, aForeignKeyRef.WriteKeyNullable);
        }

        public IEnumerator<TRef> GetEnumerator() => this.Collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public TRef GetByGuid(Guid aGuid) => (from aTest in this where aTest.Guid.Value == aGuid select aTest).Single(); /// TODO_OPT
        public virtual bool Contains(TRef aRef)=> this.Collection.Contains(aRef); /// TODO_OPT
        internal override void Load(IEnumerable<Guid> aGuids) => this.Collection.Load(this.ParentEntityObject.Storage, aGuids);
    }

    public class CSkalarRef<TParent, TChild>: CRef<TChild>
    where TParent : CEntityObject
    {
        public CSkalarRef(CEntityObject aParentEntityObject, CSkalarRefMetaInfo aSkalarRefMetaInfo, CAccessKey aWriteKeyNullable = null):base(aParentEntityObject, aSkalarRefMetaInfo, aWriteKeyNullable)
        {

        }
        public override Type DeclaredTargetType => typeof(TChild);
        internal override IEnumerable<Guid> TargetGuids => throw new InvalidOperationException();
        internal override void Load(IEnumerable<Guid> aGuids) => throw new InvalidOperationException();
    }

    public class CR1NCRef<TParent, TChild> : CNRef<TChild>
    where TParent : CEntityObject
    where TChild : CEntityObject
    {

        public CR1NCRef(CEntityObject aParentEntityObject, CR1NCRefMetaInfo aSkalarRefMetaInfo) : base(aParentEntityObject, aSkalarRefMetaInfo)
        {

        }
        public TChild Add() => this.Add<TChild>();
        public T Add<T>() where T : TChild
        {
            var aRef = this;
            var aStorage = aRef.Storage;
            var aObject = aStorage.CreateObject<T>();
            this.SetForeignKey(aObject);
            this.Collection.Add(aObject);
            return aObject;
        }
        public void Remove(TChild aChild)
        {
            if (this.Contains(aChild))
            {
                this.SetForeignKey(aChild, default(Guid));
                this.Collection.Remove(aChild);                
                aChild.Delete();
                if(this.Storage.R1NCContainsChildList)
                {
                    this.ParentEntityObject.Modify();
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }

    public class CR1NWRef<TParent, TChild> : CNRef<TChild>
    where TParent : CEntityObject
    where TChild : CEntityObject
    {

        public CR1NWRef(CEntityObject aParentEntityObject, CR1NWRefMetaInfo aSkalarRefMetaInfo) : base(aParentEntityObject, aSkalarRefMetaInfo)
        {

        }
    }


    public abstract class CRx1Ref<TParent, TChild> 
    : 
        CRef<TChild>
        where TParent : CEntityObject
        where TChild : CObject
    {
        public CRx1Ref(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }
        public override Type DeclaredTargetType => typeof(TChild);
        internal override IEnumerable<Guid> TargetGuids => throw new InvalidOperationException();
        internal override void Load(IEnumerable<Guid> aGuids) => throw new InvalidOperationException();
    }

    public sealed class CR11CRef<TParent, TChild> : CRx1Ref<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {

        public CR11CRef(CEntityObject aParentEntityObject, CR11CRefMetaInfo aSkalarRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aSkalarRefMetaInfo, aWriteKeyNullable)
        {

        }
    }

    public sealed class CR11WRef<TParent, TChild> : CRx1Ref<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
        public CR11WRef(CEntityObject aParentEntityObject, CR11WRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }
    }
}
