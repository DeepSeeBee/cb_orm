//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Testcb9fb56f_38ef_439b_af0c_3df00ba1d611
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
        
        private CSkalarRef<Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C, string> PM;
        
        private static CSkalarRefMetaInfo _PMetaInfoM = new CSkalarRefMetaInfo(typeof(Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C), nameof(P));
        
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
        
        [CbOrm.Attributes.CTargetTypeAttribute(typeof(Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C))]
        public CSkalarRef<Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C, string> P
        {
            get
            {
                if (Object.ReferenceEquals(this.PM, null))
                {
                    this.PM = new CSkalarRef<Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C, string>(this, Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C._PMetaInfo);
                }
                return this.PM;
            }
        }
        
        public static CSkalarRefMetaInfo _PMetaInfo
        {
            get
            {
                return Testcb9fb56f_38ef_439b_af0c_3df00ba1d611.C._PMetaInfoM;
            }
        }
        
        private static void _GetProperties(System.Action<CbOrm.Meta.CRefMetaInfo> aAddProperty)
        {
            aAddProperty.Invoke(C._PMetaInfo);
        }
    }
    
    public class TestSchema : CbOrm.Schema.CSchema
    {
        
        public static TestSchema SingletonM = new TestSchema();
        
        private TestSchema()
        {
            this.AddTyp(C._C_TypM);
            this.RegisterDefaultCalculator(typeof(string), ()=>String.Empty);
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
