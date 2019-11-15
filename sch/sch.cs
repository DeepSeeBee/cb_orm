// This is the runtime for my 2nd generation ORM-Wrapper. (Schema subsystem)
using CbOrm.Eno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.sch
{
    public abstract class CSchema
    {
        private readonly List<CObject> PrototypesList = new List<CObject>();
        internal const string AddPrototype_Name = nameof(AddPrototype);
        protected void AddPrototype(CObject aPrototype) => this.PrototypesList.Add(aPrototype);

    }
}
