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
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using global::Media.Common;
    #endregion

    public class ArchiverBrowseInfo
        : BrowseInfoBase
        , IBrowseInfo
    {
        public String WebAppUrl { get; set; }
        public String SiteUrl { get; set; }
        public String Path { get; set; }
        public TreeNodeLevel Level { get; set; }
        public Int64 StartTime { get; set; }
        public Int64 EndTime { get; set; }
        public Int32 OffSet { get; set; }
        public Int32 Length { get; set; }
        public String BackupJobId { get; set; }
        public String FarmName { get; set; }
        public String BackupPlanId { get; set; }
        public String BackupCycleID { get; set; }
        public String StorageInfo { get; set; }
        public TreeMode TreeMode { get; set; }
        public StorageDeviceDto IndexLogicalDevice { get; set; }

        public ArchiverBrowseInfo()
        { }

        public ArchiverBrowseInfo(ArchiverRestoreParamDto param)
        {
            WebAppUrl = param.WebAppUrl;
            SiteUrl = param.SiteUrl;
            Path = param.Path;
            Level = EnumConverter.ToEnum<TreeNodeLevel>(param.Level.ToString());
            StartTime = param.StartTime;
            EndTime = param.EndTime;
            OffSet = param.OffSet;
            Length = param.Length;
            BackupJobId = param.BackupJobId;
            FarmName = param.FarmName;
            BackupPlanId = param.BackupPlanId;
            BackupCycleID = param.BackupCycleID;
            StorageInfo = param.StorageInfo;
            TreeMode = EnumConverter.ToEnum<TreeMode>(param.LoadTreeOption.ToString());
            IndexLogicalDevice = param.IndexLogicalDevice;
            LogicalDevice = param.LogicalDevice;
            CacheSetting = param.CacheLocation;
            var volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(this));
        }
        public ArchiverBrowseInfo(ArchiverRestoreParamDto param, ProductModule mod)
        {
            WebAppUrl = param.WebAppUrl;
            SiteUrl = param.SiteUrl;
            Path = param.Path;
            Level = EnumConverter.ToEnum<TreeNodeLevel>(param.Level.ToString());
            StartTime = param.StartTime;
            EndTime = param.EndTime;
            OffSet = param.OffSet;
            Length = param.Length;
            BackupJobId = param.BackupJobId;
            FarmName = param.FarmName;
            BackupPlanId = param.BackupPlanId;
            BackupCycleID = param.BackupCycleID;
            StorageInfo = param.StorageInfo;
            TreeMode = EnumConverter.ToEnum<TreeMode>(param.LoadTreeOption.ToString());
            IndexLogicalDevice = param.IndexLogicalDevice;
            LogicalDevice = param.LogicalDevice;
            CacheSetting = param.CacheLocation;
            var volumeGenerator = VolumeGeneratorFactory.GetFSVolumeGenerator(mod);
            IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(this));
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverBrowseInfo: ");
            sb.Append(FarmName);
            sb.Append(" ");
            sb.Append(SiteUrl);
            sb.Append(" ");
            sb.Append(WebAppUrl);
            sb.Append(" ");
            sb.Append(Path);
            return sb.ToString();
        }
    }
}