//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Testd9774a1c_5e45_41d9_b259_9b33f0876267
{
    using System;
    using CbOrm.Entity;
    using CbOrm.Storage;
    using CbOrm.Ref;
    using CbOrm.Meta;
    using System.Collections.Generic;
    
    
    public class TestSchema : CbOrm.Schema.CSchema
    {
        
        public static TestSchema SingletonM = new TestSchema();
        
        private TestSchema()
        {
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