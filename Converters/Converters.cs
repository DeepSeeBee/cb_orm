using CbOrm.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Converters
{
    public abstract class CConverter
    {
        public abstract object Convert(object aValue, Type aTargetType);
        public abstract object ConvertBack(object aValue, Type aTargetType);

        public T Convert<T>(object aValue, Type aTargetType) => (T)this.Convert(aValue, aTargetType);
        public T ConvertBack<T>(object aValue, Type aTargetType) => (T)this.ConvertBack(aValue, aTargetType);

    }

    public sealed class CNullConverter : CConverter
    {
        public static readonly CNullConverter Singleton = new CNullConverter();
        public override object Convert(object aValue, Type aTargetType) => aValue;
        public override object ConvertBack(object aValue, Type aTargetType) => aValue;
    }

    internal sealed class CConverters : CConverter
    {
        private Dictionary<Type, CConverter> Dic = new Dictionary<Type, CConverter>();
        internal void Register(Type aType, CConverter aValueConverter)
        {
            if(!this.Dic.ContainsKey(aType))
            {
                this.Dic.Add(aType, aValueConverter);
            }
        }

        internal CConverter GetConverter(Type aType)
        {
            var aTmpType = aType;
            do
            {
                if (this.Dic.ContainsKey(aTmpType))
                    return this.Dic[aTmpType];
                aTmpType = aTmpType.BaseType;
            }
            while (aTmpType != null);
            return CNullConverter.Singleton;
        }

        public override object Convert(object aValue, Type aTargetType)
        {
            var aConverter = this.GetConverter(aTargetType);
            var aConvertedValue = aConverter.Convert(aValue, aTargetType);
            return aConvertedValue;
        }
        public override object ConvertBack(object aValue, Type aTargetType)
        {
            var aConverter = this.GetConverter(aTargetType);
            var aConvertedValue = aConverter.ConvertBack(aValue, aTargetType);
            return aConvertedValue;
        }

    }
    internal sealed class CSaveXmlEnumConverter : CConverter
    {
        private readonly Type EnumType;
        internal CSaveXmlEnumConverter(Type aEnumType)
        {
            this.EnumType = aEnumType;
        }
        public override object Convert(object aValue, Type aTargetType) => aValue.ToString();
        public override object ConvertBack(object aValue, Type aTargetType) => aValue.IsNullRef() ? default(Enum) : ((string)aValue).Length == 0 ? default(Enum) : Enum.Parse(this.EnumType, (string)aValue);
    }

    public sealed class CXmlConverters : CConverter
    {
        private CXmlConverters(Func<object, string> aConvertFunc, Func<string, object> aConvertBackFunc)
        {
            this.ConvertFunc = aConvertFunc;
            this.ConvertBackFunc = aConvertBackFunc;
        }
        internal static void Register(Action<Type, CConverter> aRegister)
        {
            aRegister(typeof(string), StringConverter);
            aRegister(typeof(Byte), UInt8Converter);
            aRegister(typeof(SByte), Int8Converter);
            aRegister(typeof(UInt16), UInt16Converter);
            aRegister(typeof(Int16), Int16Converter);
            aRegister(typeof(UInt32), UInt32Converter);
            aRegister(typeof(Int32), Int32Converter);
            aRegister(typeof(UInt64), UInt64Converter);
            aRegister(typeof(Int64), Int64Converter);
            aRegister(typeof(DateTime), DateTimeConverter);
            aRegister(typeof(bool), BoolConverter);
            aRegister(typeof(FileInfo), FileInfoConverter);
            aRegister(typeof(Guid), GuidConverter);
        }


        private readonly Func<object, string> ConvertFunc;
        private readonly Func<string, object> ConvertBackFunc;

        internal static readonly CConverter StringConverter = new CXmlConverters(SaveString, LoadString);
        internal static readonly CConverter UInt8Converter = new CXmlConverters(SaveUInt8, LoadUInt8);
        internal static readonly CConverter Int8Converter = new CXmlConverters(SaveInt8, LoadInt8);
        internal static readonly CConverter UInt16Converter = new CXmlConverters(SaveUInt16, LoadUInt16);
        internal static readonly CConverter Int16Converter = new CXmlConverters(SaveInt16, LoadInt16);
        internal static readonly CConverter UInt32Converter = new CXmlConverters(SaveUInt32, LoadUInt32);
        internal static readonly CConverter Int32Converter = new CXmlConverters(SaveInt32, LoadInt32);
        internal static readonly CConverter UInt64Converter = new CXmlConverters(SaveUInt64, LoadUInt64);
        internal static readonly CConverter Int64Converter = new CXmlConverters(SaveInt64, LoadInt64);
        internal static readonly CConverter DateTimeConverter = new CXmlConverters(SaveDateTime, LoadDateTime);
        internal static readonly CConverter BoolConverter = new CXmlConverters(SaveBool, LoadBool);
        internal static readonly CConverter FileInfoConverter = new CXmlConverters(SaveFileInfo, LoadFileInfo);
        internal static readonly CConverter GuidConverter = new CXmlConverters(SaveGuid, LoadGuid);

        public override object Convert(object aValue, Type aTargetType)
        {
            return this.ConvertFunc(aValue);
        }
        public override object ConvertBack(object aValue, Type aTargetType)
        {
            return this.ConvertBackFunc((string)aValue);
        }
        private static string SaveInt8(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadInt8(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (SByte)0
                 : (SByte)System.Convert.ToSByte(aXmlValue);
        }
        private static string SaveUInt8(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadUInt8(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (Byte)0
                 : (Byte)System.Convert.ToByte(aXmlValue);
        }
        private static string SaveInt16(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadInt16(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (Int16)0
                 : (Int16)System.Convert.ToInt16(aXmlValue);
        }
        private static string SaveUInt16(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadUInt16(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (UInt16)0
                 : (UInt16)System.Convert.ToUInt16(aXmlValue);
        }
        private static string SaveInt32(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadInt32(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (Int32)0
                 : (Int32)System.Convert.ToInt32(aXmlValue);
        }
        private static string SaveUInt32(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadUInt32(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (UInt32)0
                 : (UInt32)System.Convert.ToUInt32(aXmlValue);
        }
        private static string SaveInt64(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadInt64(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (Int64)0
                 : (Int64)System.Convert.ToInt64(aXmlValue);
        }
        private static string SaveUInt64(object aClrValue)
        {
            return aClrValue.ToString();
        }
        internal static object LoadUInt64(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Trim().Length == 0
                 ? (UInt64)0
                 : (UInt64)System.Convert.ToUInt64(aXmlValue);
        }


        private const string UtcSuffix = "(Utc)";

        internal static object LoadDateTime(string aXmlValue)
        {
            if (aXmlValue == null
            || aXmlValue.Trim().Length == 0)
                return DateTime.FromFileTime(0);
            var aDateAndTime = aXmlValue.Split('T');
            var aDate = aDateAndTime[0];
            var aTime = aDateAndTime[1];
            var aDateParts = aDate.Split('-');
            var aTimeParts = aTime.Split(':');
            var aYear = aDateParts[0];
            var aMonth = aDateParts[1];
            var aDay = aDateParts[2];
            var aHour = aTimeParts[0];
            var aMinute = aTimeParts[1];
            var aSecondAndUtc = aTimeParts[2];
            if (aSecondAndUtc.EndsWith(UtcSuffix))
            {
                var aSecond = aSecondAndUtc.Substring(0, aSecondAndUtc.Length - UtcSuffix.Length);
                var aDateTime = new DateTime(System.Convert.ToInt32(aYear),
                                             System.Convert.ToInt32(aMonth),
                                             System.Convert.ToInt32(aDay),
                                             System.Convert.ToInt32(aHour),
                                             System.Convert.ToInt32(aMinute),
                                             System.Convert.ToInt32(aSecond),
                                             DateTimeKind.Utc);
                return aDateTime.ToLocalTime();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static string SaveDateTime(object aClrValue)
            => SaveDateTime((DateTime)aClrValue);
        
        public static string SaveDateTime(DateTime aClrValue)
        {
            var aDateTime = (DateTime)aClrValue;
            var aUtcDateTime = aDateTime.ToUniversalTime();
            var aYear = aUtcDateTime.Year.ToString().PadLeft(4, '0');
            var aMonth = aUtcDateTime.Month.ToString().PadLeft(2, '0');
            var aDay = aUtcDateTime.Day.ToString().PadLeft(2, '0');
            var aHour = aUtcDateTime.Hour.ToString().PadLeft(2, '0');
            var aMinute = aUtcDateTime.Minute.ToString().PadLeft(2, '0');
            var aSecond = aUtcDateTime.Second.ToString().PadLeft(2, '0');
            var aString = aYear + "-" + aMonth + "-" + aDay + "T" + aHour + ":" + aMinute + ":" + aSecond + UtcSuffix;
            return aString;
        }

        internal static object LoadString(string aXmlValue)
        {
            return aXmlValue;
        }

        internal static string SaveString(object aClrValue)
        {
            return aClrValue == null ? string.Empty : aClrValue.ToString();
        }

        internal static object LoadBool(string aXmlValue)
        {
            return aXmlValue == null || aXmlValue.Length == 0 ? false : Boolean.Parse(aXmlValue);
        }

        internal static string SaveBool(object aClrValue)
        {
            return aClrValue.ToString();
        }

        internal static string SaveFileInfo(object aClrValue)
        {
            return aClrValue == null
                ? string.Empty
                : ((FileInfo)aClrValue).FullName;
        }

        internal static object LoadFileInfo(string aXmlValue)
        {
            return aXmlValue == null
                ? default(FileInfo)
                : aXmlValue.Trim().Length == 0
                ? default(FileInfo)
                : new FileInfo(aXmlValue)
                ;
        }

        //internal static string SaveEnum(object aClrValue)
        //{
        //    return aClrValue.ToString();
        //}

        //internal static object LoadEnum(Type aEnumType, string aXmlValue)
        //{
        //    return aXmlValue == null || aXmlValue.Length == 0
        //        ? Enum.GetValues(aEnumType).Cast<Enum>().First()
        //        : Enum.Parse(aEnumType, aXmlValue);
        //}


        internal static object LoadGuid(string aXmlValue)
        {
            return aXmlValue.Trim().Length == 0 ? default(Guid) : new Guid(aXmlValue);
        }

        internal static string SaveGuid(object aClrValue)
        {
            return aClrValue == null
                 ? default(Guid).ToString()
                 : aClrValue.ToString()
                 ;
        }
    }

    public sealed class CStringWrapperConverter<T> : CConverter
    {
        public CStringWrapperConverter()
        {
        }
        public override object Convert(object aValue, Type aTargetType) => ((T)aValue).ToString();
        public override object ConvertBack(object aValue, Type aTargetType) => (T)Activator.CreateInstance(typeof(T), (string)aValue);
    }

    internal sealed class CModelConverterChain : CConverter
    {
        public readonly CConverters Ly0XmlConverters = new CConverters();
        public readonly CConverters Ly1WrapConverters = new CConverters();

        private IEnumerable<CConverter> Chain;
        internal CModelConverterChain()
        {
            this.Chain = new CConverters[] { this.Ly1WrapConverters, this.Ly0XmlConverters };
        }
        public override object Convert(object aValue, Type aTargetType)
        {
            var aTmpVal = aValue;
            foreach (var aConverter in this.Chain)
            {
                aTmpVal = aConverter.Convert(aTmpVal, aTargetType);
            }
            return aTmpVal;
        }

        public override object ConvertBack(object aValue, Type aTargetType)
        {
            var aTmpVal = aValue;
            foreach (var aConverter in this.Chain.Reverse())
            {
                aTmpVal = aConverter.ConvertBack(aTmpVal, aTargetType);
            }
            return aTmpVal;
        }
    }

}
