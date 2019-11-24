// This is the runtime for my 2nd generation ORM-Wrapper. (entity relation sub system)

using CbOrm.App.Sys;
using CbOrm.Attributes;
using CbOrm.Entity;
using CbOrm.Gen;
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

        internal CTyp DeclaringTyp { get => this.ParentEntityObject.Typ; }
        internal virtual void CreateCascade()
        {
        }

        internal virtual void DeleteCascade()
        {    
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

        internal abstract object ValueObj { get; }
        internal T GetValue<T>() => (T)this.ValueObj;
        internal Type ValueType { get=>this.ValueObj.IsNullRef() ? this.DeclaredValueType : this.ValueObj.GetType(); }
        internal abstract Type DeclaredValueType { get; }
        internal abstract Type DeclaredTargetType { get; }

        internal abstract void SetValueObj(object aModelValue, CAccessKey writeKeyNullable);
        internal CStorage Storage { get => this.ParentEntityObject.Storage; }
        internal CSchema Schema { get => this.Storage.Schema; }
        internal abstract IEnumerable<Guid> TargetGuids { get; }
        internal abstract Guid TargetGuid { get;  }
        internal abstract void Load(IEnumerable<Guid> aGuids);
        internal abstract void Load(Guid aGuid);
        internal event EventHandler ValueChanged;
        internal virtual void OnValueChanged()
        {
            if(!this.ValueChanged.IsNullRef())
            {
                this.ValueChanged(this, default(EventArgs));
            }
        }
        internal static Type GetRefClass(CCardinalityEnum aCrd)
        { // TODO
            switch(aCrd)
            {
                case CCardinalityEnum.R11C:
                    return typeof(CR11CRef<CEntityObject, CObject>).GetGenericTypeDefinition();
                case CCardinalityEnum.R11P:
                    return typeof(CR11PRef<CEntityObject, CEntityObject>).GetGenericTypeDefinition();
                case CCardinalityEnum.R11W:
                    return typeof(CR11WRef<CEntityObject, CObject>).GetGenericTypeDefinition();
                case CCardinalityEnum.R1NC:
                    return typeof(CR1NCRef<CEntityObject, CEntityObject>).GetGenericTypeDefinition();
                case CCardinalityEnum.R1NP:
                    return typeof(CR1NPRef<CEntityObject, CEntityObject>).GetGenericTypeDefinition();
                case CCardinalityEnum.R1NW:
                    return typeof(CR1NWRef<CEntityObject, CEntityObject>).GetGenericTypeDefinition();
                case CCardinalityEnum.Skalar:
                    return typeof(CSkalarRef<CEntityObject, object>).GetGenericTypeDefinition();
                default:
                    throw new ArgumentException();
            }
        }

        internal virtual void RefreshValue()
        {
        }
    }

    public abstract class CRef<T> : CRef 
    {
        public CRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }

        internal override void RefreshValue()
        {
            base.RefreshValue();
            this.ValueM = default(T);
            this.ValueLoaded = false;
            this.OnValueChanged();
        }

        private bool ValueLoaded;
        protected virtual T LoadValue() => default(T);
        public T Value
        {
            get
            {
                if(!this.ValueLoaded)
                {
                    this.ValueM = this.LoadValue();
                    this.ValueLoaded = true;
                }
                return this.ValueM;
            }
            set
            {
                this.ChangeValue(value);
            }
        }
        protected virtual void DropValue(T aValue) { }
        internal override object ValueObj => this.Value;
        protected virtual void SetValueBuffer(T aValue)
        {
            if (!object.Equals(aValue, this.ValueM))
            {
                if (!this.ValueM.IsNullRef())
                { 
                    this.DropValue(this.ValueM);
                }
                this.ValueM = aValue;
                this.ValueLoaded = true;
                this.OnValueChanged();
            }
        }
        internal void ChangeValue(T aValue, CAccessKey aWriteKeyNullable = null, bool aModify = true)
        {
            this.CheckWriteable(aWriteKeyNullable);
            
            if (!object.Equals(aValue, this.ValueM))
            {
                this.SetValueBuffer(aValue);
                if (aModify)
                {
                    this.ParentEntityObject.Modify();
                }
            }
            else
            {
                this.ValueLoaded = true;
            }
        }
        internal override void SetValueObj(object aModelValue, CAccessKey aWriteKeyNullable)
        {
            this.ChangeValue((T)aModelValue, aWriteKeyNullable);
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
        internal abstract T GetItemOrNullObject(Guid aGuid);
        internal abstract bool Contains(Guid aGuid);
    }

    public sealed class CListCollection<T> : CCollection<T> where T :CObject
    {
        internal CListCollection(CStorage aStorage)
        {
            this.Storage = aStorage;
        }
        private readonly CStorage Storage;

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
        internal override T GetItemOrNullObject(Guid aGuid)
        {
            var aItems = from aTest in this.List
                         where aTest.Guid == aGuid
                         select aTest;
            var aItem = aItems.IsEmpty() ? this.Storage.CreateNullObject<T>() : aItems.Single().Object;
            return aItem;
        }
        internal override bool Contains(Guid aGuid)
            => this.Guids.Contains(aGuid);
    }


    public abstract class CNRef<TRef> 
    : 
        CRef<IEnumerable<TRef>>
    ,   IEnumerable<TRef> 
        where TRef :CEntityObject
    {
        protected CNRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo) : base(aParentEntityObject, aRefMetaInfo, new CAccessKey())
        {
            this.Collection = aRefMetaInfo.NewCollection<TRef>(aParentEntityObject.Storage);
            this.ChangeValue(this.Collection, this.WriteKeyNullable, false);
        }
        protected readonly CCollection<TRef> Collection;
        protected void SetForeignKey(TRef aObject)
        {
            var aParentId = this.ParentEntityObject.Guid.Value;
            this.SetForeignKey(aObject, aParentId);
        }
        internal override IEnumerable<Guid> TargetGuids => this.Collection.Guids;
        internal override Guid TargetGuid => throw new InvalidOperationException();
        internal override Type DeclaredTargetType => typeof(TRef);

        private CTyp TargetTypM;
        internal CTyp TargetTyp { get => CLazyLoad.Get(ref this.TargetTypM, () => this.Schema.Typs.GetBySystemType(this.DeclaredTargetType)); }

        internal CSkalarRefMetaInfo ForeignKeyMetaInfoM;
        private CSkalarRefMetaInfo ForeignKeyMetaInfo
            { get => CLazyLoad.Get(ref this.ForeignKeyMetaInfoM, () => this.TargetTyp.GetforeignKey(this.RefMetaInfo.OwnerType, this.RefMetaInfo.PropertyInfo.Name)); }

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
        internal override void Load(Guid aGuid) => throw new InvalidOperationException();

        public TRef GetItemOrNullObject(Guid aGuid)
            => this.Collection.GetItemOrNullObject(aGuid);

        public bool Contains(Guid aGuid) => this.Collection.Contains(aGuid);/// TODO_OPT
    }

    public class CSkalarRef<TParent, TChild>: CRef<TChild>
    where TParent : CEntityObject
    {
        public CSkalarRef(CEntityObject aParentEntityObject, CSkalarRefMetaInfo aSkalarRefMetaInfo, CAccessKey aWriteKeyNullable = null):base(aParentEntityObject, aSkalarRefMetaInfo, aWriteKeyNullable)
        {

        }
        internal override Type DeclaredTargetType => typeof(TChild);
        internal override IEnumerable<Guid> TargetGuids => throw new InvalidOperationException();
        internal override Guid TargetGuid => throw new InvalidOperationException();
        internal override void Load(IEnumerable<Guid> aGuids) => throw new InvalidOperationException();
        internal override void Load(Guid aGuid) => throw new InvalidOperationException();
        protected override TChild LoadValue()=> this.Schema.GetDefaultCalculator<TChild>()();
        public override string ToString() => this.Value.IsNullRef() ? string.Empty : this.Value.ToString();
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
            this.ObjectProxy = new CObjectProxy<TChild>(aParentEntityObject.Storage, default(Guid));
        }
        internal override Type DeclaredTargetType => typeof(TChild);
        private CObjectProxy<TChild> ObjectProxy;
        protected override TChild LoadValue()
        {
            return this.ObjectProxy.IsNullRef() ? default(TChild) : this.ObjectProxy.Object;
        }

        internal override void Load(Guid aGuid)
        {
            this.ObjectProxy = new CObjectProxy<TChild>(this.ParentEntityObject.Storage, aGuid);
            this.RefreshValue();
        }
        protected abstract void SetForeignKey(Guid aForeignKey);
        protected override void SetValueBuffer(TChild aValue)
        {
            base.SetValueBuffer(aValue);
            this.ObjectProxy = new CObjectProxy<TChild>(aValue);
            this.SetForeignKey(aValue.GuidValue);
        }
        internal override Guid TargetGuid => this.ObjectProxy.IsNullRef() ? default(Guid) : this.ObjectProxy.Guid;
        internal override IEnumerable<Guid> TargetGuids => throw new InvalidOperationException();
        internal override void Load(IEnumerable<Guid> aGuids) => throw new InvalidOperationException();
    }
    [CGenR11CRefCtorArgsBuilderAttribute]
    public sealed class CR11CRef<TParent, TChild> : CR11Ref<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
        public CR11CRef(CEntityObject aParentEntityObject, CR11CRefMetaInfo aSkalarRefMetaInfoNullable, CR11PRefMetaInfo aReverseNaviRefNullable = null) : base(aParentEntityObject, aSkalarRefMetaInfoNullable)
        {
            this.ReverseNaviRefMetaInfoNullable = aReverseNaviRefNullable;
        }
        private CR11PRefMetaInfo ReverseNaviRefMetaInfoNullable;
        private bool IsAutoCreateEnabled
        {
            get => this.RefMetaInfo.IsDefined<CAutoCreateAttribute>()
                 ? this.RefMetaInfo.GetAttribute<CAutoCreateAttribute>().Value
                 : true;
        }
        internal override void CreateCascade()
        {
            base.CreateCascade();
            var aAutoCreate = this.IsAutoCreateEnabled;
            if (aAutoCreate)
            {
                var aStorage = this.Storage;
                var aRefObject = aAutoCreate
                                ? aStorage.CreateObject<TChild>()
                                : aStorage.CreateNullObject<TChild>()
                                ;
                this.SetValueObj(aRefObject, this.WriteKeyNullable);
            }
        }
        internal override void DeleteCascade()
        {
            if (!this.Value.GuidIsNull)
            {
                this.ChangeValue(this.Storage.CreateNullObject<TChild>(), this.WriteKeyNullable);
            }
            base.DeleteCascade();
        }
        internal T Create<T>() where T : TChild
        {
            if (!this.Value.GuidIsNull)
            {
                throw new InvalidOperationException();
            }
            var aObject = this.Storage.CreateObject<T>();
            this.Value = aObject;
            return aObject;
        }
        internal TChild Create() => this.Create<TChild>();
        public TChild CreateOnDemand()
            => this.CreateOnDemand<TChild>();
        public TChild CreateOnDemand<T>() where T :TChild
        {
            if(this.TargetGuid == default)
            {
                this.Create<T>();
            }
            return this.Value;            
        }
        protected override void DropValue(TChild aValue)
        {
            aValue.Delete();
        }
        protected override void SetForeignKey(Guid aForeignKey)
        {
            base.SetForeignKey(aForeignKey);
            if (!this.ReverseNaviRefMetaInfoNullable.IsNullRef())
            {
                var aValue = this.Value;
                var aReverseNaviRef = this.ReverseNaviRefMetaInfoNullable.GetRef(aValue);
                aReverseNaviRef.SetValueObj(this.ParentEntityObject, aReverseNaviRef.WriteKeyNullable);
                //aReverseNaviRef.RefreshValue();
            }
        }
        internal override void OnValueChanged()
        {
            base.OnValueChanged();


        }
    }
    public abstract class CR11Ref<TParent, TChild> : CRx1Ref<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {

        public CR11Ref(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }

        private CSkalarRefMetaInfo ForeignKeyMetaInfoM;
        private CSkalarRefMetaInfo ForeignKeyMetaInfo
            { get => CLazyLoad.Get(ref this.ForeignKeyMetaInfoM, () => this.ParentEntityObject.Typ.GetforeignKey(this.RefMetaInfo.OwnerType, this.RefMetaInfo.PropertyInfo.Name)); }
        private CRef FkRef { get=> this.ForeignKeyMetaInfo.GetRef(this.ParentEntityObject); }
        protected override void SetForeignKey(Guid aForeignKey) => this.FkRef.SetValueObj(aForeignKey, this.FkRef.WriteKeyNullable);
        protected override TChild LoadValue() => this.Storage.LoadObject<TChild>(this.FkRef.GetValue<Guid>());
    }
    public sealed class CR11WRef<TParent, TChild> : CR11Ref<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
        public CR11WRef(CEntityObject aParentEntityObject, CR11WRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }
        protected override void SetForeignKey(Guid aForeignKey)
        {
            throw new NotImplementedException();
        }

    }

    public abstract class CRx1PRef<TChild, TParent>
    :
        CRx1Ref<TChild, TParent>
        where TChild : CEntityObject
        where TParent : CEntityObject
    {
        public CRx1PRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo, CSkalarRefMetaInfo aFkRefMetaInfo) : base(aParentEntityObject, aRefMetaInfo, new CAccessKey())
        {

        }

    }


    [CGenR1NPRefCtorArgsBuilder]
    public sealed class CR1NPRef<TChild, TParent>
    :
        CRx1PRef<TChild, TParent>
        where TChild : CEntityObject
        where TParent : CEntityObject
    {
        public CR1NPRef(CEntityObject aParentEntityObject, CR1NPRefMetaInfo aRefMetaInfo, CSkalarRefMetaInfo aFkRefMetaInfo) 
            : base(aParentEntityObject, aRefMetaInfo, aFkRefMetaInfo)
        {
            this.FkRefMetaInfo = aFkRefMetaInfo;
            this.FkRef = aFkRefMetaInfo.GetRef(aParentEntityObject);
            this.FkRef.ValueChanged += this.OnFkRefValueChanged;
        }
        private readonly CSkalarRefMetaInfo FkRefMetaInfo;
        private readonly CRef FkRef;
        private void OnFkRefValueChanged(object aSender, EventArgs aArgs)
        {
            this.RefreshValue();
        }

        protected override void SetForeignKey(Guid aForeignKey)
        {
            throw new InvalidOperationException();
        }
        protected override TParent LoadValue()
        {
            return this.Storage.LoadObject<TParent>(this.FkRef.GetValue<Guid>());
        }
    }

    [CGenR11PRefCtorArgsBuilder]
    public sealed class CR11PRef<TChild, TParent>
    :
        CRx1PRef<TChild, TParent>
        where TChild : CEntityObject
        where TParent : CEntityObject
    {
        public CR11PRef(CEntityObject aParentEntityObject, CR11PRefMetaInfo aRefMetaInfo, CSkalarRefMetaInfo aFkRefMetaInfo)
            : base(aParentEntityObject, aRefMetaInfo, aFkRefMetaInfo)
        {

        }
        protected override void SetForeignKey(Guid aForeignKey)
        {
        }
        private void OnFkRefValueChanged(object aSender, EventArgs aArgs)
        {
            this.RefreshValue();
        }
    }

}
