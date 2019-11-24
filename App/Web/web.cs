using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.App.Web
{

    public struct CEmailAdress
    {
        public CEmailAdress(string aText)
        {
            this.Text = aText;
        }
        private readonly string Text;
        public override string ToString()
        {
            return this.Text;
        }
    }
}
