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
    using System.Runtime.CompilerServices;
    using System.Text;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Network;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using AvePoint.Wrapper.Common;

    #endregion using directives

    [StorageInfoMetaDataBuilder(Key = "AvePoint.Media.Service.DomainModel.ArchiverStorageInfoMetaDataBuilder")]
    public class ArchiverBackupJob
        : BackupJobBase
    {
        public Int64 ArchiveTime { get; set; }

        public Int64 RetentionTimeSpanSeconds { get; set; }

        //public String ParentJobId { get; set; }
        public String SiteUrl { get; set; }

        public String SiteId { get; set; }

        public String WebAppId { get; set; }

        public String StoragePolicyId { get; set; }

        public IAveNetwork Network { get; set; }

        public LogicalDeviceDto DataLogicalDevice { get; set; }

        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        public string DataVersion { get; set; }

        public string PlatformName { get; set; }

        public Boolean UseSnapLock { get; set; }

        public Boolean UseArchiveTier { get; set; }

        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }

        public bool IsRAJob { set; get; }

        //是否使用FileLevel 来存储备份数据， true 为FileLevel, false 为DataBlockLevel
        public bool OutFileLevelBlock { set; get; }

        public string RuleId { set; get; }

        public int SourceFlag { set; get; }

        public int DataFlag { set; get; }

        public string O365TenantId { get; set; }

        public ArchiverBackupJob()
        { }

        public ArchiverBackupJob(ArchiverBackupRequest request)
        {
            var generator = new ArchiverVolumeGenerator();
            var volumeParam = new VolumeParameter(request);
            this.RuleId = request.RuleId;
            this.IndexVolume = generator.GenerateIndexVolume(volumeParam);
            this.DataVolume = generator.GenerateDataVolume(volumeParam);
            this.DataMode = Convert.ToByte(request.DataSecurity);
            if ((request.DataSecurity & AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia) != 0)
                this.CompressionType = (int)request.CompressionType;
            else this.CompressionType = -1;
            this.ArchiveTime = request.AchiverTime;
            this.FarmName = request.ArchiverSiteInfoDto.FarmName;
            this.FarmId = request.ArchiverSiteInfoDto.FarmId;
            this.JobId = request.JobId;
            this.UseSnapLock = request.UseSnapLock;
            //TODO
            //this.UseArchiveTier = request.UseArchiverTier;
            this.EncryptionInfo = request.EncryptionInfo;
            this.RetentionTimeSpanSeconds = request.RetentionTimeSpanSeconds;
            this.WebAppId = request.ArchiverSiteInfoDto.WebApplicationId;
            this.WebAppUrl = request.ArchiverSiteInfoDto.WebApplicationUrl;
            this.PlanId = request.PlanId;
            this.SiteUrl = request.ArchiverSiteInfoDto.SiteUrl;
            this.SiteId = request.ArchiverSiteInfoDto.SiteId;
            this.SpVersion = request.SpVersion;
            //this.EncryptionMethod = (int)request.EncryptionMethods;
            this.StoragePolicyId = request.StoragePolicyId;
            this.CacheSetting = request.CacheLocation;
            this.DataLogicalDevice = request.LogicalDevice;
            this.IndexLogicalDevice = request.IndexLogicalDevice;
            this.DataEncryptionInfoWrapper = request.DataEncryptionInfoWrapper;
            this.LogicalDevice= request.LogicalDevice;
            this.SourceFlag = request.SourceFlag;
            this.DataFlag = request.DataFlag;
            this.UseArchiveTier = WrapperConfiguration.MoveToAnotherTierType == (int)Storage.AccessTierType.Archive;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ArchiverBackupJob: FarmName:")
            .Append(this.FarmName)
            .Append(" SiteUrl: ")
            .Append(this.SiteUrl)
            .Append(" JobId: ")
            .Append(this.JobId)
            .Append(" IndexVolume: ")
            .Append(this.IndexVolume)
            .Append(" DataVolume: ")
            .Append(this.DataVolume);
            return sb.ToString();
        }
    }
}