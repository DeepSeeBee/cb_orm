//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Test4aabaf80_96d2_40c4_9b46_99e5a445919c
{
    using System;
    using CbOrm.Entity;
    using CbOrm.Storage;
    using CbOrm.Ref;
    using CbOrm.Meta;
    using System.Collections.Generic;
    using CbOrm.Blop;
    using CbOrm.App.Sys;
    
    
    public class P : CEntityObject
    {
        
        public static CbOrm.Meta.CTyp _P_TypM = new CbOrm.Meta.CTyp(typeof(P), new System.Guid("00000000-0000-0000-0000-000000000000"), P._GetProperties);
        
        private CSkalarRef<Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P, System.Guid> BGuidM;
        
        private static CSkalarRefMetaInfo _BGuid_MetaInfoM = new CSkalarRefMetaInfo(typeof(Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P), nameof(BGuid));
        
        private CR11CRef<Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P, CbOrm.Blop.CBlop> BM;
        
        private static CR11CRefMetaInfo _B_MetaInfoM = new CR11CRefMetaInfo(typeof(Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P), nameof(B));
        
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
        
        [CbOrm.Attributes.CTargetTypeAttribute(typeof(System.Guid))]
        [CbOrm.App.Sys.CForeignKeyCounterpartTypeAttribute(typeof(P))]
        [CbOrm.App.Sys.CForeignKeyCounterpartPropertyNameAttribute("B")]
        public CSkalarRef<Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P, System.Guid> BGuid
        {
            get
            {
                if (Object.ReferenceEquals(this.BGuidM, null))
                {
                    this.BGuidM = new CSkalarRef<Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P, System.Guid>(this, Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P._BGuid_MetaInfo);
                }
                return this.BGuidM;
            }
        }
        
        public static CSkalarRefMetaInfo _BGuid_MetaInfo
        {
            get
            {
                return Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P._BGuid_MetaInfoM;
            }
        }
        
        [CbOrm.Attributes.CTargetTypeAttribute(typeof(CbOrm.Blop.CBlop))]
        public CR11CRef<Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P, CbOrm.Blop.CBlop> B
        {
            get
            {
                if (Object.ReferenceEquals(this.BM, null))
                {
                    this.BM = new CR11CRef<Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P, CbOrm.Blop.CBlop>(this, Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P._B_MetaInfo);
                }
                return this.BM;
            }
        }
        
        public static CR11CRefMetaInfo _B_MetaInfo
        {
            get
            {
                return Test4aabaf80_96d2_40c4_9b46_99e5a445919c.P._B_MetaInfoM;
            }
        }
        
        private static void _GetProperties(System.Action<CbOrm.Meta.CRefMetaInfo> aAddProperty)
        {
            aAddProperty.Invoke(P._BGuid_MetaInfo);
            aAddProperty.Invoke(P._B_MetaInfo);
        }
    }
    
    public class TestSchema : CbOrm.Schema.CSchema
    {
        
        public static TestSchema SingletonM = new TestSchema();
        
        private TestSchema()
        {
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
