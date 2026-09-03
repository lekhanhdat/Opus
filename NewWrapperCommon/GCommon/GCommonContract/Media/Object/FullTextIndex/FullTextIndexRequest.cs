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


namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextIndexRequest
    {
        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String BackupJobId { get; set; }

        /// <summary>
        /// backup and archiver都需要传递哪些备份的site collection
        /// </summary>
        [DataMember]
        public List<String> SiteUrls { get; set; }

        /// <summary>
        /// backup: 对应index和data的LogicalDevice
        /// archiver: 对应index的LogicalDevice
        /// </summary>
        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public IndexCrawlProfile IndexProfile { get; set; }
        /// <summary>
        /// backup and archiver: 对应index.db的MediaInfo
        /// </summary>
        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        [DataMember]
        public List<ServiceDto> DataMediaInfos { get; set; }

        [DataMember]
        public FullTextIndexJobType JobType { get; set; }

        [DataMember]
        public Int32 IndexJobType { get; set; }

        [DataMember]
        public FullTextIndexJobPolicy JobPolicy { get; set; }

        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public override String ToString()
        {
            //return String.Format("FarmName[{0}], JobId[{1}], BackupPlanId[{2}], BackupCycleId[{3}], ProcessType[{4}], JobType[{5}]", FarmName, BackupJobId, JobType);
            return String.Format("FarmName[{0}], JobId[{1}], JobType[{2}]", FarmName, BackupJobId, JobType);
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IndexCrawlProfile
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public LogicalDeviceDto SearchDevice { get; set; }

        [DataMember]
        public FullTextIndexSettingDto SettingDto { get; set; }

        [DataMember]
        public String IndexVolume { get; set; }

        public override String ToString()
        {
            return String.Format("ProfileId[{0}], ProfileName[{1}], Device[{2}], IndexVolume[{3}]", Id, Name, SearchDevice.Name, IndexVolume);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverBackupIndexRequest : FullTextIndexRequest
    {
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String WebAppUrl { get; set; }

        /// <summary>
        /// archiver: data的LogicalDevice
        /// </summary>
        [DataMember]
        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }

        [DataMember]
        public bool IsFSArchiver { get; set; }
        public override String ToString()
        {
            return String.Format("Archiver Backup Index Request: Site Url: {0}, Web Application Url: {1}",
                this.SiteUrl,
                this.WebAppUrl);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VaultBackupIndexRequest : FullTextIndexRequest
    {
        public override String ToString()
        {
            return base.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularBackupIndexRequest : FullTextIndexRequest
    {
        [DataMember]
        public String BackupPlanId { get; set; }

        [DataMember]
        public String BackupCycleId { get; set; }

        public override String ToString()
        {
            return String.Format("Granular Backup Index Request: Backup Plan Id: {0}, Backup Cycle Id: {1}",
                this.BackupPlanId,
                this.BackupCycleId);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FullTextIndexJobType
    {
        [EnumMember]
        BACKUP_JOB,
        [EnumMember]
        ARCHIVER_JOB,
        [EnumMember]
        VAULT_JOB,
        [EnumMember]
        EDISCOVERY_JOB,
        [EnumMember]
        ENDUSERARCHIVER_JOB
    }
}
