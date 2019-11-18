// This is the runtime for my 2nd generation ORM-Wrapper. (entity relation sub system)

using CbOrm.Entity;
using CbOrm.Meta;
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
        internal abstract void SetValueObj(object aModelValue, CAccessKey writeKeyNullable);
    }

    public class CRef<T> : CRef
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

    public abstract class CCollection<T> : IEnumerable<T>
    {
        internal abstract void Add(T aItem);
        internal abstract void Remove(T aItem);
        internal abstract IEnumerable<T> Items{ get; }
        public IEnumerator<T> GetEnumerator() => this.Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public sealed class CListCollection<T> : CCollection<T>
    {
        private readonly List<T> List = new List<T>();
        internal override void Add(T aItem)
        {
            this.List.Add(aItem);
        }
        internal override void Remove(T aItem)
        {
            this.List.Remove(aItem);
        }
        internal override IEnumerable<T> Items => this.List;
    }


    public abstract class CNRef<T> : CRef<IEnumerable<T>>
    {
        protected CNRef(CEntityObject aParentEntityObject, CRefMetaInfo aRefMetaInfo) : base(aParentEntityObject, aRefMetaInfo, new CAccessKey())
        {
            this.Collection = aRefMetaInfo.NewCollection<T>();
            this.SetValue(this.Collection, this.WriteKeyNullable, false);
        }
        protected readonly CCollection<T> Collection;
    }

    public class CSkalarRef<TParent, TChild>: CRef<TChild>
    where TParent : CEntityObject
    {
        public CSkalarRef(CEntityObject aParentEntityObject, CSkalarRefMetaInfo aSkalarRefMetaInfo, CAccessKey aWriteKeyNullable = null):base(aParentEntityObject, aSkalarRefMetaInfo, aWriteKeyNullable)
        {

        }
    }

    public class CR1NCRef<TParent, TChild> : CNRef<TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {

        public CR1NCRef(CEntityObject aParentEntityObject, CR1NCRefMetaInfo aSkalarRefMetaInfo) : base(aParentEntityObject, aSkalarRefMetaInfo)
        {

        }
    }

    public class CR1NWRef<TParent, TChild> : CNRef<TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {

        public CR1NWRef(CEntityObject aParentEntityObject, CR1NWRefMetaInfo aSkalarRefMetaInfo) : base(aParentEntityObject, aSkalarRefMetaInfo)
        {

        }
    }


    public class CR11CRef<TParent, TChild> : CRef<TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {

        public CR11CRef(CEntityObject aParentEntityObject, CR11CRefMetaInfo aSkalarRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aSkalarRefMetaInfo, aWriteKeyNullable)
        {

        }
    }

    public class CR11WRef<TParent, TChild> : CRef<TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
        public CR11WRef(CEntityObject aParentEntityObject, CR11WRefMetaInfo aRefMetaInfo, CAccessKey aWriteKeyNullable = null) : base(aParentEntityObject, aRefMetaInfo, aWriteKeyNullable)
        {

        }
    }
}
