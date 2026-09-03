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
    using DocumentFormat.OpenXml.Wordprocessing;

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

        public List<EndUserRestoreItem> EndUserRequestItems { get; set; }

        public bool IsEndUserRestoreAccessTier { get; set; }

        public string RestoreToFSStorageString { get; set; }
        public string TenantGroupId { get; set; }

        public ArchiveIntegrationModules IntegrationModule { get; set; }

        public ArchiverRestoreJob()
        { }

        public ArchiverRestoreJob(ArchiverRestoreRequest request)
        {
            var generator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            var volumeParam = new VolumeParameter(request);
            this.IndexVolume = generator.GenerateIndexVolume(volumeParam);
            this.DataVolume = generator.GenerateDataVolume(volumeParam);
            this.JobId = request.RestoreJobId;
            this.SubJobId = request.JobId;
            this.PlanId = request.PlanId;
            this.SiteUrl = request.SiteUrl;
            this.WebAppUrl = request.WebAppUrl;
            this.TreeRoot = request.TreeRoot;
            this.FarmName = request.FarmName;
            this.BackupTime = request.ArchiveTime;
            this.BackupJobId = request.ArchiveJobId;
            this.IsSearchTree = request.IsSearchTree;
            this.CacheSetting = request.CacheLocation;
            this.TreeMode = (TreeMode)Enum.Parse(typeof(TreeMode), request.LoadTreeOption.ToString(), true);
            this.DataLogicalDeviceList = request.DataLogicalDeviceList;
            this.IndexLogicalDevice = request.IndexLogicalDevice;
            this.ArchiveRestoreOption = request.RestoreOption;
            this.RestoreFSOption = request.RestoreFSOption;
            this.DestinationFSDevice = request.DestinationFSDevice;
            this.RestoreSecurityInfos = request.RestoreSecurityInfos;
            this.ZipFilePassword = request.ZipFilePassword;
            this.IsEndUserRestore = request.IsEndUserRequest;
            this.EndUserRequestItems = request.EndUserRequestItems;
            this.IsEndUserRestoreAccessTier = request.IsEndUserRestoreAccessTier;
            this.RestoreToFSStorageString = request.EndUserRestoreToFSStorageString;
            this.IntegrationModule = request.IntegrationModule;
            this.TenantGroupId = request.TenantGroupId;
            this.KeepVersionsNumber = request.KeepVersionsNumber;
            this.RestoreVersionOption = request.RestoreVersionsOption;
            this.IsSearchAllRestore = request.IsSearchAllRestore;
            this.SearchContract = request.SearchContract;
            foreach (var ld in DataLogicalDeviceList)
            {
                //Azure System
                if (ld.Type == 403)
                {
                    this.CheckAccessTier = true;
                    break;
                }
            }
        }

        public ArchiverRestoreJob(ArchiverExportJob exportJob)
        {
            this.DataVolume = exportJob.DataVolume;
            this.LogicalDevice = exportJob.DataLogicalDevice;
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ArchiverRestoreJob: ")
            .Append("FarmName:" + FarmName)
            .Append(" ")
            .Append("SiteUrl:" + SiteUrl)
            .Append(" ")
            .Append("JobId:" + JobId)
            .Append(" ")
            .Append("BackupJobId:" + BackupJobId)
            .Append(" ")
            .Append("IndexVolume:" + IndexVolume)
            .Append(" ")
            .Append("DataVolume:" + DataVolume)
            .Append(" ")
            .Append("SearchContract:" + SearchContract);
            return sb.ToString();
        }
    }
}