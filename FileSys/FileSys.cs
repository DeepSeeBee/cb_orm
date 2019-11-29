// This is my 2nd generation ORM-Wrapper. (filesystem layer for database adapter)

using CbOrm.Blop;
using CbOrm.Entity;
using CbOrm.Meta;
using CbOrm.Schema;
using CbOrm.Storage;
using CbOrm.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CbOrm.FileSys
{
    public sealed class CFileSystemStorage 
    : 
        CStorage
    {
        public CFileSystemStorage(CSchema aSchema, DirectoryInfo aDirectoryInfo) : base(aSchema)
        {
            if (aSchema.IsNullRef())
                throw new ArgumentNullException(nameof(aSchema));
            if (aDirectoryInfo.IsNullRef())
                throw new ArgumentNullException(nameof(aDirectoryInfo));
            this.DirectoryInfo = aDirectoryInfo;
        }

        public readonly DirectoryInfo DirectoryInfo;
        internal override Guid NewObjectId() => Guid.NewGuid();
        private Guid GetGuidFromFileName(FileInfo aFileInfo) => new Guid(aFileInfo.Name.TrimEnd(aFileInfo.Extension));
        protected override CTyp GetObjectTyp(Guid aObjectId)
        {
            var aFileInfo = this.GetObjectFileInfo(this.Schema.Typs.GetBySystemType(typeof(CEntityObject)), aObjectId);
            var aXmlDocument = new XmlDocument();
            aXmlDocument.Load(aFileInfo.FullName);
            var aTypName = aXmlDocument.DocumentElement.GetAttribute(nameof(CEntityObject.TypName));
            var aTyp = this.Schema.Typs.GetByName(aTypName);
            return aTyp;
        }
        internal override Stream NewBlopInputStream(CBlop aBlop)
        {
            var aFileInfo = this.GetObjectFileInfo(aBlop);
            if (aFileInfo.IsNullRef())
                return Stream.Null;
            aFileInfo.Refresh();
            var aStream =  aFileInfo.Exists
                        ? (Stream)File.OpenRead(aFileInfo.FullName)
                        : Stream.Null
                        ;
            return aStream;
        }

        internal override bool R1NCContainsChildList => true;
        protected override CBlopOutputStream NewBlopOutputStream(CBlop aBlop)
        {
            var aFileInfo = this.GetObjectFileInfo(aBlop);
            var aStream = File.OpenWrite(aFileInfo.FullName);
            var aBlopOutputStream = new CFileSystemBlopOutputStream(aStream);
            return aBlopOutputStream;
        }

        internal override long GetBlopLength(CBlop aBlop)
        {
            var aFileInfo = this.GetObjectFileInfo(aBlop);
            var aLength = aFileInfo.Exists
                     ? aFileInfo.Length
                     : 0
                     ;
            return aLength;
        }

        public override CObject LoadObject(CTyp aType, Guid aGuid)
        {
            return this.LoadObject(aType, this.GetObjectFileInfo(aType, aGuid));
        }

        public override UInt64 GetObjectCount<T>()
        {
            var aTyp = this.Schema.Typs.GetBySystemType(typeof(T));
            var aDir = this.GetObjectDirectory(aTyp);
            var aExtension = this.GetExtension(aTyp);
            var aFiles = aDir.GetFiles("*" + aExtension);
            var aCount = aFiles.Count();
            return (UInt64)aCount;
        }

        private string GetExtension(CTyp aTyp)
        {
            var aIsBlop = aTyp.SystemType.Equals(typeof(CBlop));
            var aExtension = aIsBlop
                           ? ".bin"
                           : ".xml"
                           ;
            return aExtension;
        }

        private FileInfo GetObjectFileInfo(CTyp aType, Guid aGuid)
        {
            var aExtension = this.GetExtension(aType);
            var aDirectory = this.GetObjectDirectory(aType);
            var aFileInfo = new FileInfo(Path.Combine(aDirectory.FullName, aGuid.ToString() + aExtension));
            return aFileInfo;
        }

        internal CObject LoadObject(CTyp aType, FileInfo aFileInfo)
        {
            var aObjectId = this.GetGuidFromFileName(aFileInfo);
            return this.LoadOnDemand(aObjectId,
                                    () =>
                                    {
                                        var aObject = aType.NewObject(this);
                                        aObject.GuidValue = aObjectId;
                                        aObject.Load();
                                        return aObject;
                                    }
                            );
        }
        protected override void Load(CEntityObject aEntityObject, CTyp aAspect)
        {
            var aStorage = this;
            var aObjectId = aEntityObject.Guid.Value;
            var aFileInfo = this.GetObjectFileInfo(aAspect, aObjectId);
            var aXmlDocument = new XmlDocument();
            var aSchema = aStorage.Schema;
            var aProperties = aSchema.GetPersistentProperties(aAspect);
            aXmlDocument.Load(aFileInfo.FullName);
            var aXmlElement = aXmlDocument.SelectNodes(CRefMetaInfo.ObjectElementName).OfType<XmlElement>().Single();
            foreach(var aProperty in aProperties)
            {
                aProperty.LoadXml(aEntityObject, aXmlElement);
            }

        }

        public override IEnumerable<CObject> LoadObjects(CTyp aType)
        {
            var aObjectStorage = this;
            var aDirectory = this.GetObjectDirectory(aType);
            var aFiles = aDirectory.GetFiles();
            var aObjects = from aFile in aFiles select this.LoadObject(aType, aFile);
            return aObjects;
        }
        private DirectoryInfo GetObjectDirectory(string aObjectTypeName)
        {
            var aDir = new DirectoryInfo(Path.Combine(this.DirectoryInfo.FullName, aObjectTypeName));
            aDir.Create();
            return aDir;
        }
        internal DirectoryInfo GetObjectDirectory(CTyp aType)
        {
            return this.GetObjectDirectory(aType.TableName);
        }

        private FileInfo GetObjectFileInfo(CObject aObject)
        {
            return this.GetObjectFileInfo(aObject.Typ, aObject.GuidValue);
        }

        public override void Load(CBlop aBlop)
        {
            // The blop will request the input stream or the length as soon as it is used.
            // Not sure if we need some special actions for databases.
        }

        protected override void Save(CEntityObject aEntityObject, CTyp aAspect)
        {
            var aFileInfo = this.GetObjectFileInfo(aAspect, aEntityObject.Guid.Value);
            if(aEntityObject.IsLocallyDeleted)
            {                
                aFileInfo.Delete();
            }
            else
            {
                var aPersistentProperties = this.Schema.GetPersistentProperties(aAspect);
                var aXmlDocument = aEntityObject.NewXmlDocument(aPersistentProperties);
                aXmlDocument.Save(aFileInfo.FullName);
            }
        }

        public override CStorage CloneStorage(bool aConnect) => new CFileSystemStorage(this.Schema, this.DirectoryInfo);
        protected override void Delete(CBlop aBlop)
        {
            this.GetObjectFileInfo(aBlop).Delete();
        }
    }


}
