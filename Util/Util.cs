using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Util
{
    public static class CLazyLoad
    {
        public static T Get<T> (ref T? aVar, Func<T> aLoad) where T : struct
        {
            if (!aVar.HasValue)
                aVar = aLoad();
            return aVar.Value;
        }

        public static T Get<T>(ref T aVar, Func<T> aLoad) where T : class
        {
            if (aVar.IsNullRef())
                aVar = aLoad();
            return aVar;
        }

        public static T GetLocked<T>(object aLock, ref T aVal,  Func<T> aLoad) where T : class 
        {
            lock(aLock)
            {
                return CLazyLoad.Get<T>(ref aVal, aLoad);
            }
        }

        public static T Get<T>(ref T aVar, ref bool aLoaded, Func<T> aLoad) where T: class
        {
            if (!aLoaded)
            {
                aVar = aLoad();
                aLoaded = true;
            }
            return aVar;
        }
    }

    public static class CExtensions
    {
        public static bool IsNullRef(this object aValue) => object.Equals(aValue, null);
        public static string AvoidNullString(this string s) => s.IsNullRef() ? string.Empty : s;
        public static IEnumerable<Tuple<int, T>> GetIndexed<T>(this IEnumerable<T> aItems) => from aIdx in Enumerable.Range(0, aItems.Count()) select new Tuple<int, T>(aIdx, aItems.ElementAt(aIdx)); /// Todo_Opt ? 
        public static T DefaultIfNotHasValue<T>(this T? aIn, Func<T> aNoValue) where T : struct => aIn.HasValue? aIn.Value : aNoValue();
        public static T DefaultIfNull<T>(this T aValue, Func<T> aNull) => object.ReferenceEquals(aValue, null) ? aNull() : aValue;
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
        public static string JoinString(this IEnumerable<string> aStrings, string aSeperator)
        {
            var aResult = new System.Text.StringBuilder();
            var aOpen = false;
            foreach (var aString in aStrings)
            {
                if (aOpen)
                {
                    aResult.Append(aSeperator);
                }
                aResult.Append(aString);
                aOpen = true;
            }
            return aResult.ToString();
        }
        public static string TrimEnd(this string aText, string aTrim)
        {
            if (aText.EndsWith(aTrim))
                return aText.Substring(0, aText.Length - aTrim.Length);
            else
                throw new ArgumentException();
        }

        internal static IEnumerable<string> SplitString(this string aString, string aSeperator)
        {
            return from aTest in
                       (aString.IndexOf(aSeperator) > -1
                      ? aString.Split(new string[] { aSeperator }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries)
                      : new string[] { aString }
                      )
                   where aTest.Trim().Length > 0
                   select aTest
                 ;
        }
        public static bool IsTypeCompatible(this Type aChildType, Type aBaseType)
            => aChildType.Equals(aBaseType) || (!aChildType.BaseType.IsNullRef() && aChildType.BaseType.IsTypeCompatible(aBaseType));
        public static IEnumerable<Type> GetHirarchy(this Type aType)
        {
            if(!aType.BaseType.IsNullRef())
                foreach (var aBaseType in aType.BaseType.GetHirarchy())
                    yield return aBaseType;
            yield return aType;
        }
        public static dynamic Dyn<T>(this T aObj) => (dynamic)aObj;

        public static void DeleteRecursive(this DirectoryInfo aDirectory)
        {
            foreach(var aDir in aDirectory.GetDirectories())
            {
                aDir.DeleteRecursive();
            }
            foreach(var aFile in aDirectory.GetFiles())
            {
                aFile.Delete();
            }
            aDirectory.Delete();
        }

        public static IEnumerable<FileInfo> GetFilesRecursive(this DirectoryInfo aDirectory)
        {
            foreach(var aSubDir in aDirectory.GetDirectories())
                foreach (var aFile in aSubDir.GetFilesRecursive())
                    yield return aFile;
            foreach (var aFile in aDirectory.GetFiles())
                yield return aFile;
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this Type aType, bool aInherit = false) where TAttribute : Attribute
            => aType.GetCustomAttributes(typeof(TAttribute), aInherit).Cast<TAttribute>().Last();

        public static bool IsEmpty<T>(this IEnumerable<T> aItems)
        {
            var aEnumerator = aItems.GetEnumerator();
            return !aEnumerator.MoveNext();
        }

        public static T Interpret<T>(this FileInfo aFileInfo, Func<T> aFunc)
        {
            try
            {
                return aFunc();
            }
            catch(Exception aExc)
            {
                throw new Exception("Error reading file '" + aFileInfo.FullName + "'. " + aExc.Message);
            }
        }
    }
}
