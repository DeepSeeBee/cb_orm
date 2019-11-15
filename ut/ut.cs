using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Ut
{
    public static class CExtensions
    {
        public static TOut DefaultIfNull<TIn, TOut>(this TIn aValue, Func<TOut> aNull, Func<TIn, TOut> aNotNull) => object.ReferenceEquals(aValue, null) ? aNull() : aNotNull(aValue);
        public static TOut DefaultIfFalse<TOut>(this bool aValue, Func<TOut> aFalse, Func<TOut> aTrue) => aValue ? aTrue() : aFalse();
        public static T DefaultIfEmpty<T>(this string aValue, Func<string, T> aGetValue, Func<T> aDefault) => aValue.Length > 0 ? aGetValue(aValue) : aDefault();
        public static string DefaultIfEmpty(this string aValue, Func<string> aDefault) => aValue.DefaultIfEmpty(aVal => aVal, aDefault);
        public static IEnumerable<T> NewWithoutDuplicates<T>(this IEnumerable<T> aItems)
        { //// TODO
            var aDic = new Dictionary<T, object>();
            foreach (var aItem in aItems)
                if (!aDic.ContainsKey(aItem))
                    aDic.Add(aItem, default(object));
            return aDic.Keys.ToArray();
        }
        public static T Throw<T>(this Exception aExc) => throw aExc;
        public static T ParseEnum<T>(this string aEnumText) => (T)Enum.Parse(typeof(T), aEnumText);
        public static string TrimStart(this string aText, string aTrim) => aText.StartsWith(aTrim) ? aText.Substring(aTrim.Length, aText.Length - aTrim.Length) : new ArgumentException().Throw<string>();

    }
}
