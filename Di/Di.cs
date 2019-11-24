using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Di
{
    public sealed class CDiContainer
    {
        private readonly Dictionary<Type, Func<object>> Dic = new Dictionary<Type, Func<object>>();

        public void Add<T>(Func<T> aGet) => this.Dic.Add(typeof(T), new Func<object>(() => aGet()));
        public T Get<T>() => (T)this.Dic[typeof(T)]();
        public T GetNullable<T>() => this.Dic.ContainsKey(typeof(T)) ? this.Get<T>() : default(T);


    }
}
