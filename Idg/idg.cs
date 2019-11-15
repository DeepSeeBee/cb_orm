using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Idg
{
    public class CIdGen
    {
        public virtual object GenerateModelId() => Guid.NewGuid();
    }
}
