using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.App.Sys
{
    public enum TriStateEnum
    {
        False,
        True,
        Maybe
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CSaveConverterAttribute : Attribute
    {
        public CSaveConverterAttribute(string aConverterId)
        {
            this.ConverterId = aConverterId;
        }
        public string ConverterId;
    }

}
