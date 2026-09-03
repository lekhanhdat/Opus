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
    using AvePoint.GCommon.Contract.Storage.Entity;
    using Info = AvePoint.GCommon.Contract.Media.Object.ErrorPageCheckInfo;
    #endregion

    public class ErrorPageCheckInfo
        : ICheckInfo
    {
        public LogicalDeviceDto LogicalDevice { get; set; }
        public String Url { get; set; }
        public String FarmName { get; set; }
        public String WebAppUrl { get; set; }
        public String SiteUrl { get; set; }
        public IVolumeGeneratorFactory VolumeGeneratorFactory { get { return new VolumeGeneratorFactory(); } }
        public String IndexVolume { get; set; }
        public TreeMode TreeMode { get; set; }

        public ErrorPageCheckInfo()
        { }

        public ErrorPageCheckInfo(Info info)
        {
            this.LogicalDevice = info.LogicalDevice;
            this.Url = info.Url;
            this.FarmName = info.FarmName;
            this.WebAppUrl = info.WebAppUrl;
            this.SiteUrl = info.SiteUrl;
            var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            this.IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(this));
            this.TreeMode = TreeMode.SiteCollectionMode;
        }

        public override String ToString()
        {
            return String.Format("Url: {0}, FarmName: {1}, WebAppUrl: {2}, SiteUrl: {3}",
                this.Url,
                this.FarmName,
                this.WebAppUrl,
                this.SiteUrl);
        }
    }
}