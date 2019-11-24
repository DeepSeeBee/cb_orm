// This is my 2nd generation ORM-Wrapper. (encryption layer)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Crypt
{
    using CbOrm.Util;
    using System.Security;

    internal sealed class CStringEncryption
    {
        internal CStringEncryption(string aPassword, int aBufferSize)
        {
            this.Password = aPassword;
            this.BufferSize = aBufferSize;
        }
        private readonly string Password;
        private readonly CEncryption Encryption = new CEncryption();
        private readonly int BufferSize;
        internal string Encrypt(string aValue) => this.Encryption.EncryptToString(aValue, this.Password);
        internal string Decrypt(string aEncryptedValue) => this.Encryption.DecryptToString(aEncryptedValue, this.Password, this.BufferSize);
    }

    public class CEncryption
    {
        private Rijndael CreateCryptoAlgorithm(string aPassword, byte[] aSalt)
        {
            var aPasswordDerviveBytes = new PasswordDeriveBytes(aPassword, aSalt);
            var aKey = aPasswordDerviveBytes.GetBytes(32);
            var aIV = aPasswordDerviveBytes.GetBytes(16);
            var aAlgorithm = Rijndael.Create();
            aAlgorithm.Key = aKey;
            aAlgorithm.IV = aIV;
            return aAlgorithm;
        }

        private static byte[] DefaultSaltM;
        public static byte[] DefaultSalt { get => CLazyLoad.Get(ref DefaultSaltM, () => LoadDefaultSalt()); set => DefaultSaltM = value; }
        private static byte[] LoadDefaultSalt()
        {
            System.Diagnostics.Debugger.Break();
            // using default salt
            var aDefaultSalt = new byte[] { 0xbc, 0x32, 0x82, 0x58, 0x18, 0xf1, 0x49, 0x7c, 0x91, 0x08, 0xde, 0x04, 0xcd, 0xfb, 0xf3, 0x94 };
            return aDefaultSalt;
        }


        #region EncryptString
        private byte[] ToByteArray(string aData)
        {
            var aStream = new MemoryStream();
            var aBinaryWriter = new BinaryWriter(aStream);
            aBinaryWriter.Write(aData);
            var aBytes = aStream.ToArray();
            return aBytes;
        }
        private string StringFromByteArray(byte[] aData)
        {
            var aStream = new MemoryStream(aData);
            var aBinaryReader = new BinaryReader(aStream);
            var aString = aBinaryReader.ReadString();
            return aString;
        }
        public byte[] EncryptString(string aData, string aPassword, byte[] aSalt)
        {
            return Encrypt(ToByteArray(aData), aPassword, aSalt);
        }

        public byte[] EncryptString(string aData, string aPassword)
        {
            return EncryptString(aData, aPassword, DefaultSalt);
        }
        public string DecryptString(byte[] aEncryptedString, string aPassword, byte[] aSalt, int aBufferSize)
        {
            return StringFromByteArray(this.Decrypt(aEncryptedString, aPassword, aSalt, aBufferSize));
        }
        public string DecryptString(byte[] aEncryptedString, string aPassword, int aBufferSize)
        {
            return this.DecryptString(aEncryptedString, aPassword, DefaultSalt, aBufferSize);
        }
        #endregion
        private string ToStringUsingBitConverter(byte[] aData) => BitConverter.ToString(aData).Replace("-", "");
        private byte[] ToByteArrayUsingBitConverter(string aText)
        {
            var aLen = aText.Length;
            if (aLen % 2 == 0)
            {
                var aParts = from aIdx in Enumerable.Range(0, aLen / 2) select aText.Substring(aIdx * 2, 2);
                var aBytes = from aPart in aParts select System.Convert.ToByte(aPart, 16);
                var aByteArray = aBytes.ToArray();
                return aByteArray;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public string EncryptToString(byte[] aData, string aPassword, byte[] aSalt)
        {
            var aEncryptedData = Encrypt(aData, aPassword, aSalt);
            var aString = this.ToStringUsingBitConverter(aEncryptedData);
            return aString;
        }
        public string EncryptToString(byte[] aData, string aPassword)
        {
            return EncryptToString(aData, aPassword, DefaultSalt);
        }

        public string EncryptToString(string aData, string aPassword)
        {
            return EncryptToString(aData, aPassword, DefaultSalt);
        }
        public string EncryptToString(string aData, string aPassword, byte[] aSalt)
        {
            return EncryptToString(ToByteArray(aData), aPassword, aSalt);
        }

        public void DecryptFile(FileInfo aEncryptedSourceFileInfo, FileInfo aDecryptedTargetFileInfo, string aPassword)
        {
            this.DecryptFile(aEncryptedSourceFileInfo, aDecryptedTargetFileInfo, aPassword, DefaultSalt);
        }

        public void DecryptFile(FileInfo aEncryptedSourceFileInfo, FileInfo aDecryptedTargetFileInfo, string aPassword, byte[] aSalt)
        {
            if (new FileInfo(aDecryptedTargetFileInfo.FullName).Exists)
            {
                aDecryptedTargetFileInfo.Delete();
            }
            using (var aDecryptedTargetStream = new FileStream(aDecryptedTargetFileInfo.FullName, FileMode.CreateNew, FileAccess.Write))
            {
                try
                {
                    using (var aEncryptedSourceStream = new FileStream(aEncryptedSourceFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        this.Decrypt(aEncryptedSourceStream, aDecryptedTargetStream, aPassword, aSalt);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        aDecryptedTargetFileInfo.Delete();
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
            }
        }

        public void EncryptFile(FileInfo aDecryptedSourceFileInfo, FileInfo aEncryptedTargetFileInfo, string aPassword)
        {
            EncryptFile(aDecryptedSourceFileInfo, aEncryptedTargetFileInfo, aPassword, DefaultSalt);
        }

        public void EncryptFile(FileInfo aDecryptedSourceFileInfo, FileInfo aEncryptedTargetFileInfo, string aPassword, byte[] aSalt)
        {
            if (new FileInfo(aEncryptedTargetFileInfo.FullName).Exists)
            {
                aEncryptedTargetFileInfo.Delete();
            }
            aEncryptedTargetFileInfo.Directory.Create();
            using (var aEncryptedFileStream = new FileStream(aEncryptedTargetFileInfo.FullName, FileMode.CreateNew, FileAccess.Write))
            {
                try
                {
                    using (var aDecryptedFileStream = new FileStream(aDecryptedSourceFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Encrypt(aDecryptedFileStream, aEncryptedFileStream, aPassword, aSalt);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        aEncryptedTargetFileInfo.Delete();
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
            }
        }

        public void Encrypt(Stream aDecryptedSource, Stream aEncryptedTarget, string aPassword, byte[] aSalt)
        {
            var aEncryption = this;
            var aAlgorithm = aEncryption.CreateCryptoAlgorithm(aPassword, aSalt);
            using (var aCryptoStream = new CryptoStream(aEncryptedTarget, aAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
            {
                aDecryptedSource.CopyTo(aCryptoStream);
                aCryptoStream.Close();
            }
        }

        public byte[] Encrypt(byte[] aData, string aPassword, byte[] aSalt)
        {
            var aEncryption = this;
            var aDecrypted = new MemoryStream(aData);
            var aEncrypted = new MemoryStream();
            aEncryption.Encrypt(aDecrypted, aEncrypted, aPassword, aSalt);
            var aEncryptedData = aEncrypted.ToArray();
            return aEncryptedData;

        }

        //public byte[] Encrypt(byte[] aData, string aPassword, byte[] aSalt)
        //{
        //    var aRegSettings = this;
        //    var aOutput = new MemoryStream();
        //    var aAlgorithm = aRegSettings.CreateCryptoAlgorithm(aPassword, aSalt);
        //    using (var aCryptoStream = new CryptoStream(aOutput, aAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
        //    {
        //        aCryptoStream.Write(aData, 0, aData.Length);
        //        aCryptoStream.Close();
        //    }
        //    var aEncryptedData = aOutput.ToArray();
        //    return aEncryptedData;
        //}

        public byte[] Decrypt(byte[] aEncryptedData, string aPassword, int aBufferSize)
        {
            return this.Decrypt(aEncryptedData, aPassword, DefaultSalt, aBufferSize);
        }

        public void Decrypt(Stream aEncryptedSource, Stream aDecryptedTarget, string aPassword, byte[] aSalt)
        {
            var aEncryption = this;
            var aAlgorithm = aEncryption.CreateCryptoAlgorithm(aPassword, aSalt);
            using (var aCryptoStream = new CryptoStream(aEncryptedSource, aAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
            {
                aCryptoStream.CopyTo(aDecryptedTarget);
            }
        }
        public string DecryptToString(string aEncrypted, string aPassword, int aBufferSize) => this.DecryptString(this.ToByteArrayUsingBitConverter(aEncrypted), aPassword, aBufferSize);
        public byte[] Decrypt(byte[] aEncryptedData, string aPassword, byte[] aSalt, int aBufferSize)
        {
            var aEncryption = this;
            var aEncryptedStream = new MemoryStream(aEncryptedData);
            var aAlgorithm = aEncryption.CreateCryptoAlgorithm(aPassword, aSalt);
            var aDecryptedStream = new MemoryStream();
            using (var aCryptoStream = new CryptoStream(aEncryptedStream, aAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
            {
                var aBuf = new Byte[aBufferSize];
                int aBytesRead;
                do
                {
                    aBytesRead = aCryptoStream.Read(aBuf, 0, aBuf.Length);
                    aDecryptedStream.Write(aBuf, 0, aBytesRead);
                }
                while (aBytesRead > 0);
            }
            var aDecryptedData = aDecryptedStream.ToArray();
            return aDecryptedData;
        }


        //public byte[] Decrypt(byte[] aEncryptedData, string aPassword, byte[] aSalt, int aBufferSize)
        //{
        //    var aEncryption = this;
        //    var aEncryptedStream = new MemoryStream(aEncryptedData);
        //    var aAlgorithm = aEncryption.CreateCryptoAlgorithm(aPassword, aSalt);
        //    var aDecryptedStream = new MemoryStream();
        //    using (var aCryptoStream = new CryptoStream(aEncryptedStream, aAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
        //    {
        //        var aBuf = new Byte[aBufferSize];
        //        int aBytesRead;
        //        do
        //        {
        //            aBytesRead = aCryptoStream.Read(aBuf, 0, aBuf.Length);
        //            aDecryptedStream.Write(aBuf, 0, aBytesRead);
        //        }
        //        while (aBytesRead > 0);
        //    }
        //    var aDecryptedData = aDecryptedStream.ToArray();
        //    return aDecryptedData;
        //}

        public byte[] EncryptInt32(Int32 aValue, string aPassword, byte[] aSalt)
        {
            byte[] aEncrypted;
            var aRegSettings = this;
            using (var aStream = new MemoryStream())
            {
                using (var aStreamWriter = new BinaryWriter(aStream))
                {
                    aStreamWriter.Write(aValue);
                    var aData = aStream.ToArray();
                    aEncrypted = aRegSettings.Encrypt(aData, aPassword, aSalt);
                }
            }
            return aEncrypted;
        }

        public Int32 DecryptInt32(byte[] aEncryptedData, string aPassword, byte[] aSalt, int aBufferSize)
        {
            int aDecryptedValue;
            var aRegSettings = this;
            var aDecryptedData = aRegSettings.Decrypt(aEncryptedData, aPassword, aSalt, aBufferSize);
            using (var aStream = new BinaryReader(new MemoryStream(aDecryptedData)))
            {
                aDecryptedValue = aStream.ReadInt32();
            }
            return aDecryptedValue;
        }

        public byte[] EncryptGuid(Guid aValue, string aPassword, byte[] aSalt)
        {
            byte[] aEncrypted;
            var aRegSettings = this;
            using (var aStream = new MemoryStream())
            {
                using (var aStreamWriter = new BinaryWriter(aStream))
                {
                    aStreamWriter.Write(aValue.ToByteArray());
                    var aData = aStream.ToArray();
                    aEncrypted = aRegSettings.Encrypt(aData, aPassword, aSalt);
                }
            }
            return aEncrypted;
        }

        public Guid DecryptGuid(byte[] aEncryptedData, string aPassword, byte[] aSalt, int aBufferSize)
        {
            Guid aDecryptedValue;
            var aEncryption = this;
            var aDecryptedData = aEncryption.Decrypt(aEncryptedData, aPassword, aSalt, aBufferSize);
            using (var aStream = new BinaryReader(new MemoryStream(aDecryptedData)))
            {
                var aBuf = new byte[16];
                aStream.Read(aBuf, 0, aBuf.Length);
                aDecryptedValue = new Guid(aBuf);
            }
            return aDecryptedValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CEncryptAttribute : Attribute
    {
        public CEncryptAttribute(bool aEncrypt)
        {
            this.Encrypt = aEncrypt;
        }
        internal readonly bool Encrypt;
    }

}
