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
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder;
    using AvePoint.Wrapper.Common;

    #endregion using directives

    [StorageInfoMetaDataBuilder(Key = "AvePoint.Media.Service.DomainModel.ArchiverStorageInfoMetaDataBuilder")]
    public class GDriveBackupJob
        : BackupJobBase
    {
        public string ParentJobId { get; set; }

        public int Order { get; set; }

        public int PlanType { get; set; }

        public long ArchiverTime { get; set; }

        public string StorageInfo { get; set; }

        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        public List<string> NeedUpdateIndexVolumns { get; set; }

        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }
        public bool UseSnapLock { get; private set; }
        public string TenantId { get; set; }
        public string DriveName { get; set; }
        public string DriveId { get; set; }
        public string RuleId { get; private set; }
        public bool UseArchiveTier { get; private set; }
        public bool OutFileLevelBlock { get; set; }
        public Int64 RetentionTimeSpanSeconds { get; set; }
        public GDriveBackupJob()
        { }

        public GDriveBackupJob(GDriveBackupRequest request)
        {
            ParentJobId = request.JobId.Contains("_")
                 ? request.JobId.Substring(0, request.JobId.IndexOf("_"))
                 : request.JobId;
            var volumeParam = new VolumeParameter()
            {
                PlanId = request.PlanId,
                DriveId = request.DriveId,
                DriveName = request.DriveName,
                IsSharedDrive = request.IsSharedDrive,
                JobId = ParentJobId,
                TenantId = request.TenantId,
            };
            TenantId = request.TenantId;
            IVolumeGenerator generator = new GDriveArchiverVolumeGenerator();
            DataVolume = generator.GenerateDataVolume(volumeParam);
            IndexVolume = generator.GenerateIndexVolume(volumeParam);
            this.DataMode = Convert.ToByte(request.DataSecurity);
            if ((request.DataSecurity & AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia) != 0)
                this.CompressionType = (int)request.CompressionType;
            else this.CompressionType = -1;
            UseSnapLock = request.UseSnapLock;
            EncryptionInfo = request.EncryptionInfo;
            RetentionTimeSpanSeconds = request.RetentionTimeSpanSeconds;
            //google drive
            DriveName = request.DriveName;
            DriveId = request.DriveId;
            RuleId = request.RuleId;
            PlanType = request.PlanType;
            PlanId = request.PlanId;
            JobId = request.JobId;
            ArchiverTime = request.AchiverTime;
            CacheSetting = request.CacheLocation;
            LogicalDevice = request.LogicalDevice;
            IndexLogicalDevice = request.IndexLogicalDevice;
            Order = request.Order;
            StoragePolicyName = request.StoragePolicyId;
            StorageInfo = request.StorageInfo;
            DataEncryptionInfoWrapper = request.DataEncryptionInfoWrapper;
            IndexEncryptionInfoWrapper = request.IndexEncryptionInfoWrapper;
            UseArchiveTier = WrapperConfiguration.MoveToAnotherTierType == (int)Storage.AccessTierType.Archive;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ArchiverBackupJob: DriveName:")
            .Append(this.DriveName)
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