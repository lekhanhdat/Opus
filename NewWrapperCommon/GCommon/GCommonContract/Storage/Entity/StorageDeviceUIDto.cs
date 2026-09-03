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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract]
    public class StorageDeviceUIDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public long FreeSpace { get; set; }
        [DataMember]
        public long StorageDeviceSpace { get; set; } = -1;
        [DataMember]
        public StorageDeviceUIExtension Extension { get; set; }
        [DataMember]
        public float UseSpace { get; set; } = -1;
        [DataMember]
        public int SpaceType { get; set; }
        [DataMember]
        public UIXRI mCurrentXRI { get; set; }
        [DataMember]
        public bool SetupDataRetention { set; get; }
        [DataMember]
        public ScheduleDto Schedule { get; set; }
        [DataMember]
        public string NotificationId { get; set; }
        [DataMember]
        public List<RetentionRule> ArchiveRetentionRules { get; set; }
        [DataMember]
        public bool UseCompression { get; set; }
        [DataMember]
        public int CompressionSpeed { get; set; }
        [DataMember]
        public bool UseEncryption { get; set; }
        [DataMember]
        public string EncryptionProfileId { get; set; }
        [DataMember]
        public string LastModifiedTime { get; set; }
        [DataMember]
        public string LastArchivedTime { get; set; }
        [DataMember]
        public bool IsUsingDevice { get; set; }
        [DataMember]
        public string ConnectionString { get; set; } //for recenter
        [DataMember]
        public bool IsSystemStorage { get; set; }
        [DataMember]
        public bool? DAOMigrated { set; get; }
        [DataMember]
        public string DAOStoragePolicyId { get; set; }
        [DataMember]
        public string DAOLogicalDeviceId { get; set; }
        [DataMember]
        public string DAOPhysicalDeviceId { get; set; }
    }
    [DataContract]
    public class UIXRI
    {
        [DataMember]
        public Dictionary<string, string> Params { get; set; }
        [DataMember]
        public string VIM { get; set; }
    }
    [DataContract]
    public class StorageDeviceResult : CommonSettingResultForPage
    {
        [DataMember]
        public string IndexDeviceId { get; set; }
        [DataMember]
        public List<StorageDeviceUIDto> StorageDeviceUIDtosList { get; set; }
    }

    public class DevicesResult
    {
        public List<StorageIdAndName> StorageIdAndNameList { get; set; }
    }
    public class StorageIdAndName
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }
    }
    public class SecurityProfileResult
    {
        public string DefaultSecurityProfileId { get; set; }
        public List<SecurityProfileNameAndId> SecurityProfiles { get; set; }
    }
    public class SecurityProfileNameAndId
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    [DataContract]
    public class StorageDeviceUIExtension
    {
        [DataMember]
        public long UsedSpace { get; set; }
        [DataMember]
        public long TotalSpace { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RetentionRule : StroagePolicyContentDto
    {
        [DataMember]
        public int KeepValue { get; set; }
        [DataMember]
        public DateUnit ArchiveDateUnit { get; set; }
        [DataMember]
        public string KeepValueErrorMessage { get; set; }
        [DataMember]
        public bool TakeEffectToExistingData { get; set; }
        [DataMember]
        public bool DeleteTheData { get; set; }
        [DataMember]
        public bool IsArchivedTier { get; set; }
        [DataMember]
        public bool IsMove { get; set; }
        [DataMember]
        public bool RemoveTheJob { get; set; }
        [DataMember]
        public bool RemoveOrphanedStub { get; set; }
        [DataMember]
        public string MoveDeviceId { get; set; }
        //[DataMember]
        //public int RemoveOrphanedStub4CompatibilityUpgrade { get; set; }

        [DataMember]
        public bool KeepOrphanedStub4CompatibilityExistingRule { get; set; }
        [DataMember]
        public bool IsMarkDataTier { get; set; }
        [DataMember]
        public int TierType { get; set; }
        [DataMember]
        public KeepDateType RetentionDataTimeType { get; set; }

        [DataMember]
        public bool IsSoftDelete { get; set; }
        [DataMember]
        public int SoftDeleteKeepValue { get; set; }
        [DataMember]
        public DateUnit SoftDeleteDateUnit { get; set; }
        [DataMember]
        public bool IsFitSoftDelete { get; set; }

    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum RemoveStubStatus : int
    //{
    //    [EnumMember]
    //    True = 0,
    //    [EnumMember]
    //    False = 1,
    //}
}
