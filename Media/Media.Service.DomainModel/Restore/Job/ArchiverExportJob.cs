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
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;

    #endregion using directives

    public class ArchiverExportJob
        : RestoreJobBase
    {
        public String SiteUrl { get; set; }

        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        public LogicalDeviceDto DataLogicalDevice { get; set; }

        public TreeMode TreeMode { get; set; }

        public List<ExportItemInfo> ExportItemList { get; set; }

        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public String ParentJobId { get { return this.JobId.Remove(this.JobId.LastIndexOf("_", StringComparison.OrdinalIgnoreCase)); } }

        public ArchiverExportJob()
        { }

        public ArchiverExportJob(ArchiverRestoreRequest request)
        {
            var generator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            var volumeParam = new VolumeParameter(request);
            this.IndexVolume = generator.GenerateIndexVolume(volumeParam);
            this.DataVolume = generator.GenerateDataVolume(volumeParam);
            this.JobId = request.RestoreJobId;
            this.PlanId = request.PlanId;
            this.SiteUrl = request.SiteUrl;
            this.TreeRoot = request.TreeRoot;
            this.FarmName = request.FarmName;
            this.BackupTime = request.ArchiveTime;
            this.BackupJobId = request.ArchiveJobId;
            this.TreeMode = TreeMode.SiteCollectionMode;
            this.DataLogicalDevice = new LogicalDeviceDto();
            var deviceList = new List<LogicalDeviceDto>();
            deviceList.AddRange(request.DataLogicalDeviceList);
            deviceList.ForEach(device =>
            {
                this.DataLogicalDevice.PhysicalDrives.AddRange(device.PhysicalDrives);
            });
            this.IndexLogicalDevice = request.IndexLogicalDevice;
            this.DestinationFSDevice = request.DestinationFSDevice;
            this.RestoreSecurityInfos = request.RestoreSecurityInfos;
            this.ExportItemList = new List<ExportItemInfo>();
            request.SearchResultList.ForEach(item => { this.ExportItemList.Add(new ExportItemInfo(item)); });
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ArchiverExportJob: ")
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
            .Append("DataVolume:" + DataVolume);
            return sb.ToString();
        }
    }
}