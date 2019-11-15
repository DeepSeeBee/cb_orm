// This is the runtime for my 2nd generation ORM-Wrapper. (meta layer)

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CbOrm.Mta
{
    public abstract class CProperty
    {
        public CProperty (Type aOwnerType, 
                          Type aPropertyType,
                          string aPropertyName
                          )
        {
            this.OwnerType = aOwnerType;
            this.PropertyType = aPropertyType;
            this.PropertyName = aPropertyName;
            this.PropertyInfo = aOwnerType.GetProperty(aPropertyName);
        }
        public readonly Type OwnerType;
        public readonly Type PropertyType;
        public readonly string PropertyName;
        public readonly PropertyInfo PropertyInfo;
    }
    public sealed class CFieldProperty : CProperty
    {
        public CFieldProperty(Type aOwnerType,
                              Type aPropertyType,
                              string aPropertyName) :base(aOwnerType, aPropertyType, aPropertyName)
        {
        }
    }
    public abstract class CRelProperty { }
    public sealed class CBlopProperty : CRelProperty { }
    public sealed class CR11cProperty { }
    public sealed class CR11pProperty { }
    public sealed class CR1NcProperty { }
    public sealed class CR1NpProperty { }
}
