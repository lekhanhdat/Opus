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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    public class ArchiverRetentionResult
        : RetentionResultBase
        , IRetentionResult
    {
        public String FarmName { get; set; }
        public String SiteUrl { get; set; }
        public String WebApp { get; set; }
        public String JobId { get; set; }
        public String StoragePolicyId { get; set; }
        public Int64 ArchiverBackupTime { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public LogicalDeviceDto DataLogicalDevice { get; set; }
        public LogicalDeviceDto DestinationDevice { get; set; }
        public MediaArchiverRetentionAction RetentionAction { get; set; }
        public String DestinationPhysicalDeviceId { get; set; }
        public ServiceDto MediaService { get; set; }
        public long Size { get; set; }
        /// <summary>
        /// 表示删除数据成功后是否删除job记录
        /// </summary>
        public bool IsDeleteJob { get; set; }
        /// <summary>
        /// 表示retention成功还是失败,2是成功,3是失败
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// media用于更新retention job进度等信息
        /// </summary>
        public BaseJobDto RetentionJob { get; set; }

        /// <summary>
        /// 用于判断是否可以删除 ArchiverIndexSubInfoes 中的记录
        /// 当还有duplicated file 跟它有关联的话，就不能删除，否则会无法找到数据块对应的Storage Device
        /// </summary>
        public bool HasIndexRelatedToBackupJob { get; set; }
        public bool IsArchiveTierToColdTier { get; set; }

        //public ArchiverRetentionResult(ArchiverRetentionInfo info)
        //{
        //    this.FarmName = info.FarmName;
        //    this.JobId = info.JobId;
        //    this.SiteUrl = info.SiteUrl;
        //    this.ArchiverBackupTime = info.ArchiverBackupTime;
        //    this.StoragePolicyId = info.StoragePolicyId;
        //    this.MediaService = info.MediaService;
        //    this.RetentionAction = info.RetentionAction;
        //    this.RetentionJob = info.RetentionJob;
        //    this.DestinationPhysicalDeviceId = info.DestinationPhysicalDeviceId;
        //    this.DataLogicalDevice = info.DataLogicalDevice;
        //    this.IndexLogicalDevice = info.IndexLogicalDevice;
        //    this.IsDeleteJob = info.IsDeleteJob;
        //}

        public override String ToString()
        {
            return String.Format("FarmName: {0}, SiteUrl: {1}, JobId: {2} StoragePolicyId: {3}.",
                    this.FarmName,
                    this.SiteUrl,
                    this.JobId,
                    this.StoragePolicyId);
        }
    }

    public class ArchiverLifecycleRetentionResult : ArchiverRetentionResult
    {
        public List<ArchiverBasicIndex> manualItem { set; get; }

        public List<string> needClearManualRules { set; get; }

        public List<ArchiverBasicIndex> sucessItem { set; get; }

        public List<ArchiverBasicIndex> failedItem { set; get; }

        public List<ArchiverBasicIndex> manualSkippedItem { set; get; }

        public List<ArchiverBasicIndex> DoesNotSupportSharePointItem { get; set; }
    }
    public class GDriveArchiverLifecycleRetentionResult : ArchiverRetentionResult
    {
        public List<GoogleBasicIndex> manualItem { set; get; }

        public List<string> needClearManualRules { set; get; }

        public List<GoogleBasicIndex> sucessItem { set; get; }

        public List<GoogleBasicIndex> failedItem { set; get; }

        public List<GoogleBasicIndex> manualSkippedItem { set; get; }
    }
}