//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d
{
    using CbOrm.Entity;
    using CbOrm.Storage;
    using CbOrm.Ref;
    using CbOrm.Meta;
    using System.Collections.Generic;
    using System;
    
    
    public class C : CEntityObject
    {
        
        public static CbOrm.Meta.CTyp _C_TypM = new CbOrm.Meta.CTyp(typeof(C), new System.Guid("00000000-0000-0000-0000-000000000000"), C._GetProperties);
        
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
        
        private static void _GetProperties(System.Action<CbOrm.Meta.CRefMetaInfo> aAddProperty)
        {
        }
    }
    
    public class TestSchema : CbOrm.Schema.CSchema
    {
        
        public static TestSchema SingletonM = new TestSchema();
        
        private TestSchema()
        {
            this.AddTyp(C._C_TypM);
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
