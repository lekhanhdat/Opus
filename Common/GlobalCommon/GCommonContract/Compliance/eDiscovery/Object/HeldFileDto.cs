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



using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class HeldFileDto
    {
        public HeldFileDto()
        {
            HoldItems = new List<HoldItemDto>();
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DataSourceType DataSourceType { get; set; }

        [DataMember]
        public FileType FileType { get; set; }

        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }

        [DataMember]
        public int Size { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public long LastModifiedTime { get; set; }

        [DataMember]
        public string WebId { get; set; }

        [DataMember]
        public int ItemID { get; set; }

        [DataMember]
        public string SiteCollectionId { get; set; }

        [DataMember]
        public string DataGuid { get; set; }

        [DataMember]
        public string ListId { get; set; }

        [DataMember]
        public int UIVersion { get; set; }
        [DataMember]
        public bool IsCurrent { get; set; }
        [DataMember]
        public bool IsMarked { get; set; }

        [DataMember]
        public string VersionString { get; set; }


        #region - Support Legal Hold And Release for Archived Data -

        [DataMember]
        public string ArchiveJobId { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public string FullPathMD5 { get; set; }

        [DataMember]
        public string VersionName { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string SiteURL { get; set; }

        [DataMember]
        public String Title { get; set; }

        [DataMember]
        public String CreatedBy { get; set; }

        [DataMember]
        public String ModifiedBy { get; set; }

        [DataMember]
        public String CreatedTime { get; set; }

        [DataMember]
        public String ModifiedTime { get; set; }

        [DataMember]
        public String FileContainer { get; set; }

        [DataMember]
        public String DataFilePath { get; set; }

        [DataMember]
        public String MetaDataFilePath { get; set; }

        [DataMember]
        public String MetaDataStorageInfo { get; set; }

        [DataMember]
        public String ContentDataStorageInfo { get; set; }

        #endregion - Support Legal Hold And Release for Archived Data -

        private string _subJobId;

        [DataMember]
        public string SubJobID 
        {
            get { return _subJobId; }
            set 
            { 
                ArchiveJobId = value;
                _subJobId = value;
            }
        }

        [DataMember]
        public string ArchiveBy { get; set; }

        [DataMember]
        public long ArchiveTime { get; set; }

        [DataMember]
        public string Summary { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DataSourceType
    {
        [EnumMember]
        SharePoint = 1,
        [EnumMember]
        Archiver = 2,
        [EnumMember]
        Vault = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FileType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Document = 1,
        [EnumMember]
        Item = 2,
        [EnumMember]
        Attachment = 3
    }

    //    [DataContract(Namespace = ContractConstants.Namespace)]
    //    public enum Relevant
    //    {
    //
    //    }
}