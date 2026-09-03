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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.FullTextIndex;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverSiteMasterIndexContract
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public long ArchiverTime { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string WebURL { get; set; }

        [DataMember]
        public string SiteURL { get; set; }

        [DataMember]
        public int JobState { get; set; }

        //[DataMember]
        //public int FullTextIndexState { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }  //转到子表中

        [DataMember]
        public string MediaServiceId { get; set; } //子表

        [DataMember]
        public string IndexDeviceId { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string WebId { get; set; }

        [DataMember]
        public string SiteId { get; set; }

        [DataMember]
        public string O365TenantId { get; set; }

        [DataMember]
        public int SPVersion { get; set; }
        
        [DataMember]
        public string RuleId { set; get; }
        
        [DataMember]
        public int SourceFlag { set; get; }

        [DataMember]
        public int DataFlag { set; get; }

        [DataMember]
        public IndexModule Module { set; get; }
        //[DataMember]
        //public string StorageInfo { get; set; }

        //[DataMember]
        //public string Crc32 { get; set; }
        
        [DataMember]
        public ArchiverSiteMasterIndexExtension Extension { get; set; }

        [DataMember]
        public MergeIndexState MergeIndexState { get; set; }

        [DataMember]
        public SiteJobLockEnum SiteJobLockEnum { get; set; }

        [DataMember]
        public string LockedJobId { get; set; }

        [DataMember]
        public VersionDetails VersionDetails { get; set; }

        [DataMember]
        public List<ArchiverIndexSubInfoContract> SubInfo { get; set; }

        [DataMember]
        public string StorageInfo { get; set; }

        #region properties for compliace module
        [DataMember]
        public CrawlStatus CrawlStatus { set; get; }
        [DataMember]
        public CrawlIndexStatus CrawlTreatedStatus { set; get; }
        [DataMember]
        public string CrawlProfileId { set; get; }
        [DataMember]
        public string CrawlDeviceId { set; get; }
        #endregion

        [DataMember]
        public bool DAOMigrated { set; get; }

        [DataMember]
        public int BackupFileType { get; set; }

        [DataMember]
        public int DuplicateStatus { get; set; }

        [DataMember]
        public string TeamsId { get; set; }

        [DataMember]
        public bool IsSoftDeleted { get; set; }

        [DataMember]
        public string GroupMailboxAddress { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VersionDetails
    {
        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public long LastImportedTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverSiteMasterIndexExtension
    {
        [DataMember]
        public long UpdateTime { get; set; }
        [DataMember]
        public bool IsSiteCollectionArchivered { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverGroupSiteMasterIndexExtension : ArchiverSiteMasterIndexExtension
    {
        [DataMember]
        public long GroupCreated { get; set; }
        [DataMember]
        public string SPGroupSiteURL { get; set; }
        [DataMember]
        public List<string> ChannelSiteRelativeURLs { get; set; }
        [DataMember]
        public bool IsMicrosoftTeam { get; set; }
        [DataMember]
        public bool IsChannelSiteReadOnly { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MergeIndexState
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Merging = 1,
        [EnumMember]
        Succeed = 2,
        [EnumMember]
        Failed = 3,
        [EnumMember]
        Skip = 4,
        [EnumMember]
        Pruning = 5,
        [EnumMember]
        PruSucceed = 6,
        [EnumMember]
        PruFailed = 7,
        [EnumMember]
        DAOMigrated = 8,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteJobLockEnum
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Locked = 1,
        [EnumMember]
        Blocked = 2
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IndexModule
    {
        [EnumMember]
        Archiver = 0,
        [EnumMember]
        Vault = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleType
    {
        [EnumMember]
        Archiver = 0,
        [EnumMember]
        Vault = 1
    }
}
