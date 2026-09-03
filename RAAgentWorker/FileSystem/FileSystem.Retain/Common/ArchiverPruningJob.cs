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
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverPruningJob
    {
        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String SiteUrl { get; set; }
        [DataMember]
        public String SiteId { get; set; }

        [DataMember]
        public String WebApp { get; set; }

        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String StoragePolicyId { get; set; }

        [DataMember]
        public Int64 ArchiverBackupTime { get; set; }

        [DataMember]
        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        [DataMember]
        public LogicalDeviceDto DataLogicalDevice { get; set; }

        [DataMember]
        public LogicalDeviceDto DestinationDevice { get; set; }

        public CacheSettingDto CacheSettings { get; set; }

        [DataMember]
        public MediaArchiverRetentionAction RetentionAction { get; set; }

        [DataMember]
        public String DestinationPhysicalDeviceId { get; set; }

        [DataMember]
        public ServiceDto MediaService { get; set; }

        /// <summary>
        /// 表示删除数据成功后是否删除job记录
        /// </summary>
        [DataMember]
        public Boolean IsDeleteJob { get; set; }

        [DataMember]
        public Boolean IsSimulateJob { get; set; }

        [DataMember]
        public long SimulateJobRunTime { get; set; }

        [DataMember]
        public string RetentionSourceName { get; set; }

        [DataMember]
        public int SourceFlag { get; set; }

        /// <summary>
        /// 表示retention成功还是失败,2是成功,3是失败
        /// </summary>
        [DataMember]
        public Int32 State { get; set; }

        /// <summary>
        /// media用于更新retention job进度等信息
        /// </summary>
        [DataMember]
        public SOJob RetentionJob { get; set; }

        /// <summary>
        /// 保存保留时间，以秒为单位，在Device间发生数据转移时使用
        /// Control根据当前转移的系统时间与被保留的时间换算成秒
        /// </summary>
        [DataMember]
        public Int64 RetentionTimeSpanSeconds { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }

        [DataMember]
        public string TenantGroupOwner { get; set; }

        [DataMember]
        public String MainIndexStorageInfo { get; set; }

        [DataMember]
        public String SubIndexStorageInfo { get; set; }

        [DataMember]
        public bool RemoveOrphanedStub { get; set; }

        [DataMember]
        public Boolean NeedStoreInArchiverTier { get; set; }
        [DataMember]
        public int AccessTierType { get; set; }
        #region for retention by modified time
        [DataMember]
        public int KeepValue { get; set; }
        [DataMember]
        public DateUnit ArchiveDateUnit { get; set; }
        [DataMember]
        public KeepDateType RetentionDataTimeType { get; set; }
        [DataMember]
        public String CurrentStoragePolicyId { get; set; }
        [DataMember]
        public long DateTimeNowTicks { get; set; }
        [DataMember]
        public bool IsFitSoftDelete { get; set; }
        [DataMember]
        public bool IsSoftDelete { get; set; }
        [DataMember]
        public int SoftDeleteKeepValue { get; set; }
        [DataMember]
        public DateUnit SoftDeleteDateUnit { get; set; }
        [DataMember]
        public Int64 SoftDeleteTime { get; set; }
        [DataMember]
        public string UNCPath { get; set; }
        #endregion

        [DataMember]
        public bool HasMoveActionInPreviousRules { get; set; }

        [DataMember]
        public bool IsEnableMoveToAnotherLocation { get; set; }

        [DataMember]
        public bool IsEnableCopyToAnotherLocation { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Media Archiver Retention Info: ");
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Site Url: {0}, ", this.SiteUrl);
            stringBuilder.AppendFormat("Job Id: {0}, ", this.JobId);
            stringBuilder.AppendFormat("Index Logical Device: {0}, ", this.IndexLogicalDevice);
            stringBuilder.AppendFormat("Data Logical Device: {0}, ", this.DataLogicalDevice);
            stringBuilder.AppendFormat("Destination Device: {0}, ", this.DestinationDevice);
            stringBuilder.AppendFormat("Is Delete Job: {0}, ", this.IsDeleteJob);
            stringBuilder.AppendFormat("State: {0}", this.State);
            return stringBuilder.ToString();
        }
    }
    public enum MediaArchiverRetentionAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        DeleteData = 1,
        [EnumMember]
        MoveData = 2,
        [EnumMember]
        MarkTier = 3,
    }
}
