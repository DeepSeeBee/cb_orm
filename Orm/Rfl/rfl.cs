// This is the runtime for my 2nd generation ORM-Wrapper. (.NET reflection part)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Rfl
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CSaveConverterAttribute :Attribute
    {
        public CSaveConverterAttribute(string aConverterId)
        {
            this.ConverterId = aConverterId;
        }
        public string ConverterId;
    }
}
