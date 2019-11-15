// This is the runtime for my 2nd generation ORM-Wrapper. (entity relation sub system)

using CbOrm.Eno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Ref
{

    public class CR1NCRef<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
    }

    public class CR1NWRef<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
    }


    public class CR11CRef<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
    }

    public class CR11WRef<TParent, TChild>
    where TParent : CEntityObject
    where TChild : CObject
    {
    }
}
