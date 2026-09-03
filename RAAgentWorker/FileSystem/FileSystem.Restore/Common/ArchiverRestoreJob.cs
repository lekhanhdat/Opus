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
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Storage;
    using RAFileSystem.FileSystem.FileSystem.Restore;

    #endregion using directives

    [AveCodeReview(
    "2012/8/2",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]

    public class ArchiverRestoreJob
        : RestoreJobBase
    {
        public string SiteUrl { get; set; }
        public IXSystem LogicalDeviceSystem;
        public string WebAppUrl { get; set; }

        public long ArchiveEndTime { get; set; }

        public long ArchiveStartTime { get; set; }

        public RestoreOption ArchiveRestoreOption { get; set; }

        public RestoreFSOption RestoreFSOption { get; set; }

        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }

        public TreeMode TreeMode { get; set; }

        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public String ParentJobId { get { return this.JobId.Remove(this.JobId.LastIndexOf("_", StringComparison.OrdinalIgnoreCase)); } }

        public String ZipFilePassword { get; set; }

        public bool CheckAccessTier { get; set; }

        public bool IsEndUserRestore { get; set; }


        public bool IsEndUserRestoreAccessTier { get; set; }

        public string RestoreToFSStorageString { get; set; }
        public string TenantGroupId { get; set; }


        public ArchiverRestoreJob()
        { }

        public ArchiverRestoreJob(FSRestoreWorker worker)
        {
            this.IndexVolume = worker.IndexVolume;
            this.DataVolume = worker.DataVolume;
            //this.JobId = request.RestoreJobId;
            //this.SubJobId = request.JobId;
            this.SiteUrl = worker.ConnectionId;
            //this.WebAppUrl = request.WebAppUrl;
            //this.BackupTime = request.ArchiveTime;
            //this.BackupJobId = worker.ArchiveJobId;
            this.CacheSetting = worker.CacheSetting;
            this.DataLogicalDeviceList = worker.DataLogicalDeviceList;
            this.IndexLogicalDevice = worker.IndexLogicalDevice;
            //this.ArchiveRestoreOption = request.RestoreOption;
            //this.RestoreFSOption = request.RestoreFSOption;
            //this.DestinationFSDevice = request.DestinationFSDevice;
            this.RestoreSecurityInfos = worker.restoreSecurityInfos;
            this.LogicalDeviceSystem = worker.dataLogicalDevice;
        }

    }
}