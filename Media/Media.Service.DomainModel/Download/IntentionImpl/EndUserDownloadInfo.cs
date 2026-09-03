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
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;

    #endregion using directives

    public class EndUserDownloadInfo
        : DownloadInfoBase
        , IDownloadInfo
    {
        public LogicalDeviceDto DataDevice { get; set; }

        public LogicalDeviceDto IndexDevice { get; set; }

        public String FarmName { get; set; }

        public String WebAppUrl { get; set; }

        public String SiteUrl { get; set; }

        public List<String> PathMD5List { get; set; }

        public String DataVolume { get; set; }

        public TreeMode TreeMode { get; set; }

        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public EndUserDownloadInfo()
        { }

        public EndUserDownloadInfo(EndUserArchiverDownloadInfo info)
        {
            this.FarmName = info.FarmName;
            this.WebAppUrl = info.WebAppUrl;
            this.SiteUrl = info.SiteUrl;
            this.PathMD5List = info.PathMD5List;
            this.TreeMode = TreeMode.SiteCollectionMode;
            this.DataDevice = new LogicalDeviceDto();
            var deviceList = new List<LogicalDeviceDto>();
            deviceList.AddRange(info.DataDeviceList);
            deviceList.ForEach(device =>
            {
                this.DataDevice.PhysicalDrives.AddRange(device.PhysicalDrives);
            });
            this.IndexDevice = info.IndexDevice;
            var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            this.IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(this));
            this.DataVolume = volumeGenerator.GenerateDataVolume(new VolumeParameter(this));
            this.RestoreSecurityInfos = info.RestoreSecurityInfos;
        }

        public override string ToString()
        {
            return string.Format("EndUserDownloadInfo : FarmName : {0}, WebAppUrl : {1}, SiteUrl : {2}.", FarmName, WebAppUrl, SiteUrl);
        }
    }
}