using CbOrm.Entity;
using CbOrm.Meta;
using CbOrm.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CbOrm.Blop
{

    public abstract class CBlopOutputStream : IDisposable
    {
        protected CBlopOutputStream(Stream aInnerStream) { this.Stream = aInnerStream; }
        public readonly Stream Stream;
        public void Dispose() => this.Stream.Dispose();
        public abstract void Commit();
    }

    internal sealed class CFileSystemBlopOutputStream : CBlopOutputStream
    {
        public CFileSystemBlopOutputStream(Stream aInnerStream) : base(aInnerStream) { }
        public override void Commit()
        {
            this.Stream.Flush();
            this.Stream.SetLength(this.Stream.Position);
            this.Stream.Close();
        }
    }

    public sealed class CBlop : CObject
    {
        public CBlop(CStorage aStorage) : base(aStorage)
        {
            this.GuidValue = default(Guid);
        }
        #region Typ
        private static void _GetProperties(Action<CRefMetaInfo> aAddProperty)
        {
        }
        private static CTyp _CBlop_TymM = new CTyp(typeof(CBlop), new Guid("2bc8fa06-ab32-4d6e-aa39-8001fd025f5a"), _GetProperties);
        public static CTyp _CBlop_Typ { get => _CBlop_TymM; }
        #endregion
        internal override bool IsStructureReflected => true;
        private Guid GuidM;
        public override Guid GuidValue { get => this.GuidM; internal set { this.CheckNotCached(); this.GuidM = value; } }
        public override CTyp Typ => _CBlop_Typ;
        public void SetByteArray(byte[] aByteArray) => this.SetStream(new MemoryStream(aByteArray));
        public byte[] NewByteArray()
        {
            using (var aInputStream = this.NewInputStream())
            {
                var aLength = this.Length;
                var aBytes = new byte[aLength];
                aInputStream.Read(aBytes, 0, (int)aLength);
                return aBytes;
            }
        }
        
        public Stream NewInputStream()
        {
            if (this.SaveStreamNullable == null)
            {
                var aStream = this.Storage.NewBlopInputStream(this);
                return aStream;
            }
            else
            {
                var aStream = this.SaveStreamNullable;
                aStream.Seek(0, SeekOrigin.Begin);
                var aMemoryStream = new MemoryStream((int)aStream.Length);
                aStream.CopyTo(aMemoryStream);
                return aMemoryStream;
            }
        }

        private Stream SaveStreamNullable;
        public void SetStream(Stream aStream)
        {
            this.SaveStreamNullable = aStream;
            this.Modify();
        }

        public long Length { get => this.Storage.GetBlopLength(this); }

        internal override void AcceptLoad()
        {
            if (this.SaveStreamNullable != null)
            {
                throw new Exception("Internal error.");
            }
            else
            {
                this.Storage.Load(this);
            }
        }

        internal override void AcceptSave()
        {
            this.Storage.VisitSave(this, this.SaveStreamNullable);
            this.SaveStreamNullable = null;
        }


        //internal void SetData(FileUpload aFileUpload)
        //{
        //    var aBytes = aFileUpload.FileBytes;
        //    var aMemoryStream = new MemoryStream(aBytes);
        //    this.SetStream(aMemoryStream);
        //}
    }
}
