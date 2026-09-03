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
    using System.Text;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;

    #endregion

    public class EndUserBrowseInfo
        : BrowseInfoBase
        , IBrowseInfo
    {
        public String PathMD5 { get; set; }
        public String FarmName { get; set; }
        public String WebAppUrl { get; set; }
        public String SiteUrl { get; set; }
        public Int32 OffSet { get; set; }
        public Int32 Length { get; set; }
        public TreeMode TreeMode { get; set; }
        public Boolean NeedNodeMap { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public Boolean BrowseFoldersOnly { get; set; }

        public EndUserBrowseInfo()
        { }

        public EndUserBrowseInfo(EndUserArchiverViewInfo info)
        {
            this.PathMD5 = info.PathMD5;
            this.FarmName = info.FarmName;
            this.WebAppUrl = info.WebAppUrl;
            this.SiteUrl = info.SiteUrl;
            this.OffSet = info.OffSet;
            this.Length = info.Length;
            this.TreeMode = TreeMode.SiteCollectionMode;
            this.NeedNodeMap = info.NeedNodeMap;
            this.IndexLogicalDevice = info.IndexDevice;
            var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            this.IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(this));
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("EndUserBrowseInfo: ");
            sb.Append(FarmName);
            sb.Append(" ");
            sb.Append(SiteUrl);
            sb.Append(" ");
            sb.Append(WebAppUrl);
            sb.Append(" ");
            sb.Append(PathMD5);
            sb.Append(" ");
            sb.Append(NeedNodeMap.ToString());
            return sb.ToString();
        }
    }
}