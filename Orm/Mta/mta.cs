// This is the runtime for my 2nd generation ORM-Wrapper. (meta layer)

using System;
using System.Collections.Generic;
using System.Text;

namespace CbOrm.Mta
{
    public abstract class CProperty { }
    public sealed class CFieldProperty { }
    public abstract class CRelProperty { }
    public sealed class CBlopProperty : CRelProperty { }
    public sealed class CR11cProperty { }
    public sealed class CR11pProperty { }
    public sealed class CR1NcProperty { }
    public sealed class CR1NpProperty { }
}
