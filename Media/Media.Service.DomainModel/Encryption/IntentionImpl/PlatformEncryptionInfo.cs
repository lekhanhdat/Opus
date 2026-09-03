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
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    public class PlatformEncryptionInfo
        : IEncryptionInfo
    {
        public String BackupCycleId { get; set; }
        public String BackupJobId { get; set; }
        public String BackupPlanId { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public String FarmName { get; set; }
        public LogicalDeviceDto LogicalDevice { get; set; }
        public String StorageInfo { get; set; }

        public override String ToString()
        {
            return String.Format("BackupCycleId: {0}, JobId: {1}, FarmName: {2}, PlanId: {3}, StorageInfo: {4}",
                this.BackupCycleId,
                this.BackupJobId,
                this.FarmName,
                this.BackupPlanId,
                this.StorageInfo);
        }

        public PlatformEncryptionInfo()
        { }

        public PlatformEncryptionInfo(PlatformRestoreRequest request)
        {
            FarmName = request.DRInfo.FarmName;
            BackupPlanId = request.DRInfo.PlanId;
            BackupCycleId = request.DRInfo.JobId;
            BackupJobId = request.DRInfo.JobId;
            CacheSetting = request.CacheLocation;
            LogicalDevice = request.LogicalDevice;
            StorageInfo = request.DRInfo.StorageInfo;
        }

        public PlatformEncryptionInfo(PlatformBackupRequest request)
        {
            FarmName = request.FarmName;
            BackupPlanId = request.PlanId;
            BackupCycleId = request.JobId;
            BackupJobId = request.JobId;
            CacheSetting = request.CacheLocation;
            LogicalDevice = request.LogicalDevice;
        }
    }
}