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
            this.DirectoryInfo = aDirectoryInfo;
        }

        public readonly DirectoryInfo DirectoryInfo;
        internal override Guid NewObjectId() => Guid.NewGuid();
        private Guid GetGuidFromFileName(FileInfo aFileInfo) => new Guid(aFileInfo.Name.TrimEnd(aFileInfo.Extension));

        internal override Stream NewBlopInputStream(CBlop aBlop)
        {
            var aFileInfo = this.GetObjectFileInfo(aBlop, false);
            if (aFileInfo.IsNullRef())
                return Stream.Null;
            aFileInfo.Refresh();
            var aStream =  aFileInfo.Exists
                        ? (Stream)File.OpenRead(aFileInfo.FullName)
                        : Stream.Null
                        ;
            return aStream;
        }

        protected override CBlopOutputStream NewBlopOutputStream(CBlop aBlop)
        {
            var aFileInfo = this.GetObjectFileInfo(aBlop);
            var aStream = File.OpenWrite(aFileInfo.FullName);
            var aBlopOutputStream = new CFileSystemBlopOutputStream(aStream);
            return aBlopOutputStream;
        }

        internal override long GetBlopLength(CBlop aBlop)
        {
            var aFileInfo = this.GetObjectFileInfo(aBlop, false);
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

        private FileInfo GetObjectFileInfo(CTyp aType, Guid aGuid)
        {
            var aIsBlop = aType.SystemType.Equals(typeof(CBlop));
            var aExtension = aIsBlop
                           ? ".bin"
                           : ".xml"
                           ;
            var aDirectory = this.GetObjectDirectory(aType);
            var aFileInfo = new FileInfo(Path.Combine(aDirectory.FullName, aGuid.ToString() + aExtension));
            return aFileInfo;
        }

        private object GetObjectDirectory(object tableName)
        {
            throw new NotImplementedException();
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

        internal override bool IsPersistent(CObject aObject)
        {
            var aObjectFileInfo = this.GetObjectFileInfo(aObject);
            var aIsPersistent = aObjectFileInfo.Exists;
            return aIsPersistent;
        }

        internal DirectoryInfo GetObjectDirectory(string aObjectTypeName)
        {
            return new DirectoryInfo(Path.Combine(this.DirectoryInfo.FullName, aObjectTypeName));
        }


        internal DirectoryInfo GetObjectDirectory(CTyp aType)
        {
            return this.GetObjectDirectory(aType.TableName);
        }

        private FileInfo GetObjectFileInfo(CObject aObject, Guid aGuid)
        {
            var aType = aObject.Typ;
            var aFileInfo = this.GetObjectFileInfo(aType, aGuid);
            return aFileInfo;
        }

        //TODO: PARamter CrateGuidOnDemand is obsolte.
        private FileInfo GetObjectFileInfo(CObject aObject, bool aCreateGuidOnDemand = false)
        {
            if (aCreateGuidOnDemand)
            {
                if (aObject.GuidIsNull)
                {
                    throw new Exception("Internal error.");
                    //aObject.CreateGuidOnDemand();
                }
            }
            return this.GetObjectFileInfo(aObject, aObject.GuidValue);
        }

        internal override void Load(CBlop aBlop)
        {
            // The blop will request the input stream or the length as soon as it is used.
            // Not sure if we need some special actions for databases.
        }

        internal override void Delete(CObject aObject)
        {
            if (aObject.IsPersistent)
            {
                var aFileInfo = this.GetObjectFileInfo(aObject, false);
                aFileInfo.Delete();
            }
        }
        internal override void Save(CEntityObject aEntityObject, CTyp aAspect)
        {
            var aFileInfo = this.GetObjectFileInfo(aAspect, aEntityObject.Guid.Value);
            if(aEntityObject.IsDeleted)
            {
                throw new NotImplementedException();
                //aFileInfo.Delete();
            }
            else
            {
                var aPersistentProperties = this.Schema.GetPersistentProperties(aAspect);
                var aXmlDocument = aEntityObject.NewXmlDocument(aPersistentProperties);
                aFileInfo.Directory.Create();
                aXmlDocument.Save(aFileInfo.FullName);
            }
        }

        //internal override void Load(CObject aObject)
        //{
        //    var aFileInfo = this.GetObjectFileInfo(aObject);
        //    if (!aFileInfo.Exists)
        //    {
        //        throw new CObjectDeletedException();
        //    }
        //    var aXmlDocument = new XmlDocument();
        //    aXmlDocument.Load(aFileInfo.FullName);
        //    aObject.Load(aXmlDocument.DocumentElement);
        //}


        public override CStorage CloneStorage(bool aConnect) => new CFileSystemStorage(this.Schema, this.DirectoryInfo);

    }


}
