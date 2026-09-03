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
    using AvePoint.Common;
    #region using directives

    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Common;
    using RAFileSystem.FileSystem.FileSystem.Backup;
    using Storage;
    using System;
    using System.Collections.Generic;

    #endregion using directives

    public class ArchiverIndexServiceOpenParameter : IndexServiceOpenParameter
    {
        public TreeMode TreeMode { set; get; }
        public String SiteUrl { get; set; }
        public bool CheckAccessTier { get; set; }

        public bool IsEndUserRequest { get; set; }

        public int WaitIndexLockerTimeOutInMs { get; set; } = 30 * 60 * 1000;

        public ArchiverIndexServiceOpenParameter() { }

        public ArchiverIndexServiceOpenParameter(FSArchiverBackupJob backupJob, IXSystem cacheSystem, IXSystem indexDevice, string dbPassword)
        {
            IndexVolume = backupJob.IndexVolume;
            IndexCacheDeviceSystem = cacheSystem;
            CacheSetting = backupJob.CacheSetting;
            BackupJobId = backupJob.JobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = TreeMode.JobMode;
            IsNeedCreateNewIndex = true;
            DBPassWord = dbPassword;
        }
        public ArchiverIndexServiceOpenParameter(MergeIndexSubJobInfo mergeIndexJobsState, IXSystem indexLogicalDevice, IXSystem indexCacheDevice, String indexVolume, string dbPassword)
        {
            IndexVolume = indexVolume;
            BackupJobId = mergeIndexJobsState.JobDto.Id;
            IndexDatabaseName = mergeIndexJobsState.JobDto.Id + "_" + ServiceConstants.IndexDBName;
            IndexLogicalDeviceSystem = indexLogicalDevice;
            IndexCacheDeviceSystem = indexCacheDevice;
            DBPassWord = dbPassword;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}