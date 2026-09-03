/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */




namespace AvePoint.Media.ClassicStorage
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.StorageApi;

    #endregion using directives

    public interface IXParam
    {
    }

    public class StorageParam : IXParam
    {
    }

    [Serializable]
    public class XStorageInfo : FileSystemInfo
    {
        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }

    [Serializable]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StorageInfo : XStorageInfo
    {
        public Boolean NeedRenameIndexName { get; set; }
        public string FilePath { get; set; } //add for RevIM export
        string hl_name;
        string lo_name;
        string clipId;
        int bufferSize;
        long offset;
        long length;
        string storageInfo;
        bool isClosing;
        string objectId;
        DataBlockType dataType;
        AccessTierType fileTierType;

        private bool isRoot;

        public bool IsRoot
        {
            get
            {
                if ("\\".Equals(HighName))
                {
                    isRoot = true;
                }
                return isRoot;
            }
        }

        private StorageInfo parent;

        public string ListFilter { get; set; }

        public StorageInfo Parent
        {
            get
            {
                if (parent == null)
                {
                    if (!string.IsNullOrEmpty(HighName))
                    {
                        int lastIndex = HighName.LastIndexOf('\\');
                        if (lastIndex > 0)
                        {
                            parent = new StorageInfo(HighName.Substring(0, lastIndex), HighName.Substring(lastIndex + 1));
                        }
                        else if (lastIndex == 0)
                        {
                            parent = new StorageInfo(string.Empty, HighName.Substring(lastIndex + 1));
                        }
                    }
                }
                return parent;
            }
        }

        public int SkipNum { get; set; }

        public Data_Version DataVersion { get; set; }

        FileMode mode;
        Dictionary<string, string> metaInfos = new Dictionary<string, string>();

        public Dictionary<string, string> MetaInfos { get { return metaInfos; } internal set { this.metaInfos = value; } }

        public bool IsClosing { get { return isClosing; } set { this.isClosing = value; } }

        public string ExtraStorageInfo
        {
            get { return storageInfo; }
            set { this.storageInfo = value; }
        }

        public StorageInfo()
        {
            IsDeleteParentFolder = true;
            DataVersion = Data_Version.DocAve6;
        }

        public FileMode FileMode
        {
            get { return this.mode; }
            set { this.mode = value; }
        }
        public FileAccess FileAccess
        {
            get;
            set;
        }

        public StorageInfo(string highName, string lowName)
        {
            this.hl_name = highName;
            this.lo_name = lowName;
            IsDeleteParentFolder = true;
            DataVersion = Data_Version.DocAve6;
            SecurityUtils.SafeCombinePath(highName, lowName);
        }

        public StorageInfo Clone()
        {
            return this.MemberwiseClone() as StorageInfo;
        }

        public string HighPlusLowName
        {
            get
            {
                string HPLName = PathUtil.CombinePath(hl_name ?? "", LowName ?? "");
                //HPLName = HPLName.TrimStart('\\').TrimEnd('\\');
                return HPLName;
            }
        }

        [DataMember]
        public string HighName { get { return hl_name; } set { this.hl_name = value; } }

        [DataMember]
        public string LowName { get { return lo_name; } set { this.lo_name = value; } }

        [DataMember]
        public string ClipId
        {
            get
            {
                if (string.IsNullOrEmpty(this.clipId))
                {
                    if (!string.IsNullOrEmpty(this.storageInfo))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(this.storageInfo);
                        XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("StorageInfo");
                        this.clipId = node.GetAttribute("clipId");
                    }
                    if (string.IsNullOrEmpty(this.clipId))
                    {
                        this.clipId = HighName;
                    }
                }
                return this.clipId;
            }
            set
            {
                this.clipId = value;
            }
        }

        [DataMember]
        public int BufferSize { get { return bufferSize; } set { this.bufferSize = value; } }

        [DataMember]
        public long Offset { get { return offset; } set { this.offset = value; } }

        [DataMember]
        public long Length { get { return length; } set { this.length = value; } }

        [DataMember]
        public DataBlockType DataType { get { return dataType; } set { this.dataType = value; } }

        [DataMember]
        public AccessTierType FileTierType { get { return fileTierType; } set { fileTierType = value; } }

        [DataMember]
        public bool IsDeleteOldVersion { get; set; }

        //for fs
        public bool UseBuffer { get; set; }

        public bool IsDeleteParentFolder { get; set; }

        public bool IsLoadFirstLevel { get; set; }

        public List<string> ObjectIds
        {
            get
            {
                List<string> objs = new List<string>();
                if (!string.IsNullOrEmpty(this.storageInfo))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(this.storageInfo);
                    XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("StorageInfo");
                    objs.Add(node.GetAttribute("contentId"));
                    objs.Add(node.GetAttribute("metaId"));
                }
                if (!string.IsNullOrEmpty(ObjectId))
                {
                    if (!objs.Contains(ObjectId))
                    {
                        objs.Add(ObjectId);
                    }
                }
                return objs;
            }
        }

        public string ObjectId
        {
            get
            {
                if (string.IsNullOrEmpty(this.objectId))
                {
                    if (!string.IsNullOrEmpty(this.storageInfo))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(this.storageInfo);
                        XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("StorageInfo");
                        this.objectId = node.GetAttribute("contentId");
                        if (this.dataType == DataBlockType.MetaData || string.IsNullOrEmpty(this.objectId))
                        {
                            this.objectId = node.GetAttribute("metaId");
                        }
                    }
                    if (string.IsNullOrEmpty(objectId))
                    {
                        this.objectId = LowName;
                    }
                }
                if (string.IsNullOrEmpty(this.objectId))
                {
                    throw new Exception("object id can't be null or empty, storage info:" + this.storageInfo);
                }
                return this.objectId;
            }
            set
            {
                this.objectId = value;
            }
        }

        public int CurrentRetryCount { get; set; }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("StorageInfo: ");
            builder.Append(" Offset: " + this.Offset);
            builder.Append(" Length: " + this.Length);
            builder.Append(" HighName: " + this.HighName ?? string.Empty);
            builder.Append(" LowName: " + this.LowName ?? string.Empty);
            builder.Append(" ExtraStorageInfo: " + this.ExtraStorageInfo ?? string.Empty);
            return builder.ToString();
        }

    }

    public enum Data_Version
    {
        DocAve5 = 1,
        DocAve6 = 2,
    }

    public enum SyncMode
    {
        Synchronous = 0,
        ASynchronous = 1,
    }

    public enum DataBlockType
    {
        Other = 0,
        MetaData = 1,
        ContentData = 2,
    }

    public enum AccessTierType
    {
        Other = 0,
        Hot = 1,
        Cool = 2,
        Archive = 3,
    }
}