//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Testcfd975a9_b348_4085_9306_bbea67fc771e
{
    using System;
    using CbOrm.Entity;
    using CbOrm.Storage;
    using CbOrm.Ref;
    using CbOrm.Meta;
    using System.Collections.Generic;
    
    
    public class C : CEntityObject
    {
        
        public static CbOrm.Meta.CTyp _C_TypM = new CbOrm.Meta.CTyp(typeof(C), new System.Guid("00000000-0000-0000-0000-000000000000"), C._GetProperties);
        
        private CSkalarRef<C, Guid> Parent_P_CGuidM;
        
        private static CSkalarRefMetaInfo _Parent_P_CGuidMetaInfoM = new CSkalarRefMetaInfo(typeof(C), nameof(Parent_P_CGuid));
        
        private CR1NPRef<C, P> Parent_P_CM;
        
        private static CR1NPRefMetaInfo _Parent_P_CMetaInfoM = new CR1NPRefMetaInfo(typeof(C), nameof(Parent_P_C));
        
        public C(CStorage aStorage) : 
                base(aStorage)
        {
        }
        
        public static CbOrm.Meta.CTyp _C_Typ
        {
            get
            {
                return C._C_TypM;
            }
        }
        
        public override CbOrm.Meta.CTyp Typ
        {
            get
            {
                return C._C_Typ;
            }
        }
        
        [CbOrm.Attributes.CForeignKeyCounterpartTypeAttribute(typeof(P))]
        [CbOrm.Attributes.CForeignKeyCounterpartPropertyNameAttribute("C")]
        public CSkalarRef<C, Guid> Parent_P_CGuid
        {
            get
            {
                if (Object.ReferenceEquals(this.Parent_P_CGuidM, null))
                {
                    this.Parent_P_CGuidM = new CSkalarRef<C, Guid>(this, C._Parent_P_CGuidMetaInfo, new CbOrm.Ref.CAccessKey());
                }
                return this.Parent_P_CGuidM;
            }
        }
        
        public static CSkalarRefMetaInfo _Parent_P_CGuidMetaInfo
        {
            get
            {
                return C._Parent_P_CGuidMetaInfoM;
            }
        }
        
        public CR1NPRef<C, P> Parent_P_C
        {
            get
            {
                if (Object.ReferenceEquals(this.Parent_P_CM, null))
                {
                    this.Parent_P_CM = new CR1NPRef<C, P>(this, C._Parent_P_CMetaInfo, C._Parent_P_CGuidMetaInfo);
                }
                return this.Parent_P_CM;
            }
        }
        
        public static CR1NPRefMetaInfo _Parent_P_CMetaInfo
        {
            get
            {
                return C._Parent_P_CMetaInfoM;
            }
        }
        
        private static void _GetProperties(System.Action<CbOrm.Meta.CRefMetaInfo> aAddProperty)
        {
            aAddProperty.Invoke(C._Parent_P_CGuidMetaInfo);
            aAddProperty.Invoke(C._Parent_P_CMetaInfo);
        }
    }
    
    public class P : CEntityObject
    {
        
        public static CbOrm.Meta.CTyp _P_TypM = new CbOrm.Meta.CTyp(typeof(P), new System.Guid("00000000-0000-0000-0000-000000000000"), P._GetProperties);
        
        private CR1NCRef<P, C> CM;
        
        private static CR1NCRefMetaInfo _CMetaInfoM = new CR1NCRefMetaInfo(typeof(P), nameof(C));
        
        public P(CStorage aStorage) : 
                base(aStorage)
        {
        }
        
        public static CbOrm.Meta.CTyp _P_Typ
        {
            get
            {
                return P._P_TypM;
            }
        }
        
        public override CbOrm.Meta.CTyp Typ
        {
            get
            {
                return P._P_Typ;
            }
        }
        
        public CR1NCRef<P, C> C
        {
            get
            {
                if (Object.ReferenceEquals(this.CM, null))
                {
                    this.CM = new CR1NCRef<P, C>(this, P._CMetaInfo);
                }
                return this.CM;
            }
        }
        
        public static CR1NCRefMetaInfo _CMetaInfo
        {
            get
            {
                return P._CMetaInfoM;
            }
        }
        
        private static void _GetProperties(System.Action<CbOrm.Meta.CRefMetaInfo> aAddProperty)
        {
            aAddProperty.Invoke(P._CMetaInfo);
        }
    }
    
    public class TestSchema : CbOrm.Schema.CSchema
    {
        
        public static TestSchema SingletonM = new TestSchema();
        
        private TestSchema()
        {
            this.AddTyp(C._C_TypM);
            this.AddTyp(P._P_TypM);
            this.Init();
        }
        
        public static TestSchema Singleton
        {
            get
            {
                return TestSchema.SingletonM;
            }
        }
        
        public static CbOrm.Schema.CSchema GetSingleton()
        {
            return TestSchema.SingletonM;
        }
    }
}
