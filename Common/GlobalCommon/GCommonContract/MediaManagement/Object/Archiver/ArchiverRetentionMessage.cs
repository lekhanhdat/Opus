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
namespace AvePoint.GCommon.Contract.MediaManagement.Object
{
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.RA.Contract.Explorer;
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverRetentionMessage : RetentionMessageBase
    {
        [DataMember]
        public String SiteUrl { get; set; }
        [DataMember]
        public String AgentId { get; set; }
        [DataMember]
        public String SiteId { get; set; }
        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String WebApp { get; set; }

        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String StoragePolicyId { get; set; }

        [DataMember]
        public Int64 ArchiverBackupTime { get; set; }
        [DataMember]
        public bool RemoveOrphanedStub { get; set; }

        [DataMember]
        public LogicalDeviceDto IndexLogicalDevice { get; set; }

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
        public String MainIndexStorageInfo { get; set; }

        [DataMember]
        public String SubIndexStorageInfo { get; set; }

        [DataMember]
        public bool IsArchivedTier { set; get; }
        [DataMember]
        public int AccessTierType { set; get; }
        [DataMember]
        public DataSourceForOrphanBlob dataSourceForOrphanBlob { set; get; }

        #region for retention by modified time
        [DataMember]
        public int KeepValue { get; set; }
        [DataMember]
        public DateUnit ArchiveDateUnit { get; set; }
        [DataMember]
        public KeepDateType RetentionDataTimeType { get; set; }
        [DataMember]
        public bool IsFitSoftDelete { get; set; }
        [DataMember]
        public bool IsSoftDelete { get; set; }
        [DataMember]
        public String CurrentStoragePolicyId { get; set; }
        [DataMember]
        public int SoftDeleteKeepValue { get; set; }
        [DataMember]
        public DateUnit SoftDeleteDateUnit { get; set; }
        [DataMember]
        public Int64 SoftDeleteTime { get; set; }
        [DataMember]
        public bool IsSystemStorage { get; set; }
        [DataMember]
        public int DeleteStatus { set; get; }
        #endregion

        [DataMember]
        public bool HasMoveActionInPreviousRules { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Media Archiver Retention Info: ");
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Site Url: {0}, ", this.SiteUrl);
            stringBuilder.AppendFormat("Job Id: {0}, ", this.JobId);
            stringBuilder.AppendFormat("Index Logical Device: {0}, ", this.IndexLogicalDevice);
            stringBuilder.AppendFormat("Data Logical Device: {0}, ", this.LogicalDevice);
            stringBuilder.AppendFormat("Destination Device: {0}, ", this.DestinationDevice);
            stringBuilder.AppendFormat("Is Delete Job: {0}, ", this.IsDeleteJob);
            stringBuilder.AppendFormat("State: {0}", this.State);
            stringBuilder.AppendFormat("Is Archived Tier: {0}", this.IsArchivedTier);
            return stringBuilder.ToString();
        }
    }
}
