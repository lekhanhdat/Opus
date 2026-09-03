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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSiteMasterIndexDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string CycleId { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public PRBackupPlanType PlanType { get; set; }
        [DataMember]
        public int BackupType { get; set; }
        [DataMember]
        public long BackupTime { get; set; }
        [DataMember]
        public string FarmName { get; set; }
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public string AgentHost { get; set; }
        [DataMember]
        public string LogicalDriveId { get; set; }
        [DataMember]
        public string PhysicalDriveId { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public int JobStatus { get; set; }
        [DataMember]
        public int IndexLevel { get; set; }
        [DataMember]
        public int IndexStatus { get; set; }
        [DataMember]
        public int PruneStatus { get; set; }
        [DataMember]
        public string SPVersion { get; set; }
        [DataMember]
        public string SecurityKey { get; set; }
        [DataMember]
        public string Extension { get; set; }
        //for support EMC
        [DataMember]
        public string StorageInfo { get; set; }
        /// <summary>media硬件配置信息</summary>
        [DataMember]
        public string StorageInfoExtension { get; set; }
        [DataMember]
        public string HostFullName { get; set; }
        [DataMember]
        public string StoragePolicyId { get; set; }
        [DataMember]
        public PRBackupJobDto PRBackupJob { get; set; }
        // 是否为plan中最后一个job
        [DataMember]
        public bool IsLastJob { get; set; }
        [DataMember]
        public List<PRSiteMasterIndexDto> PRSiteMasterIndexDtoList { get; set; }
        // 标示
        [DataMember]
        public int UpdateVersion { get; set; }
        [DataMember]
        public int CycleJobStatus { get; set; }
        [DataMember]
        public long CycleBackupTime { get; set; }

        [DataMember]
        public BackupManagementGroupType BackupGroupType { get; set; }

        // 存放数据升级版本信息
        [DataMember]
        public DataVersionContentDto VersionDetails { get; set; }

        /// <summary>标识是否为导入数据,非导入默认false</summary>
        [DataMember]
        public bool IsImportData { get; set; }

        [DataMember]
        public PRPlatformType PlatformType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MasterIndexReturnInfoDto
    {
        /// <summary>
        /// true
        /// </summary>
        [DataMember]
        public bool IsUpdate { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSiteMasterIndexSubDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string JobId { get; set; }

        /// <summary>
        ///  备份数据的web application name
        /// </summary>
        [DataMember]
        public string WebAppName { get; set; }

        /// <summary>
        /// 备份数据的site collection name
        /// </summary>
        [DataMember]
        public string SiteUrl { get; set; }

        /// <summary>
        /// 记录当前site collection是否真正备份了数据，主要用于Only show incremental data功能。注意与主表中ModifyData属性的区别。
        /// </summary>
        [DataMember]
        public int ModifyData { get; set; }
    }
}
