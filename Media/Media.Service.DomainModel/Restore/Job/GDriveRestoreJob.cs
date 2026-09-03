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
    #region directives

    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using AvePoint.RA.Common.Cache;
    using System.Collections.Generic;

    #endregion directives

    public class GDriveRestoreJob
        : RestoreJobBase
    {
        public string CycleId { get; set; }

        //public string ModulePath => RestoreRequest.GetModulePath();

        public GoogleDriveTreeNodeDto GDriveTreeRoot { get; set; }

        public Dictionary<string, string> IndexStorageInfoMap { get; set; }

        public TreeMode TreeMode { get; set; }

        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public long? FromSentDate { get; set; }

        public long? ToSentDate { get; set; }

        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public GDriveRestoreRequest RestoreRequest { get; set; }

        public LogicalDeviceDto IndexDBLogicalDevice { get; set; }

        public bool CheckAccessTier { get; set; }
        public string DriveId { get; set; }
        public string DriveName { get; set; }
        public bool IsSharedDrive { get; set; }
        public override string ToString()
        {
            return string.Format("GDriveRestoreJob : FarmName: {0}, PlanId: {1}, CycleId: {2}, JobId: {3}, DataVolume: {4}, IndexVolume: {5}, BackupJobId: {6}, OnlyOneJob: {7}, FromSentDate: {8}, ToSentDate: {9}.",
                FarmName,
                PlanId,
                CycleId,
                JobId,
                DataVolume,
                IndexVolume,
                BackupJobId,
                OnlyOneJob,
                FromSentDate,
                ToSentDate
                );
        }

        //public GDriveRestoreJob()
        //{ }

        public GDriveRestoreJob(GDriveRestoreRequest request)
        {
            RestoreRequest = request;


            var generator = new GDriveArchiverVolumeGenerator();
            DriveId = request.DriveId;
            DriveName = request.DriveName;
            IsSharedDrive = request.IsSharedDrive;
            var volumeParam = new VolumeParameter()
            {
                DriveId = request.DriveId,
                DriveName = request.DriveName,
                IsSharedDrive = request.IsSharedDrive,
                TenantId = request.TenantId,
            };

            this.IndexVolume = generator.GenerateIndexVolume(volumeParam);
            this.DataVolume = generator.GenerateDataVolume(volumeParam);
            this.JobId = request.RestoreJobId;
            this.SubJobId = request.JobId;
            this.PlanId = request.PlanId;
            this.GDriveTreeRoot = request.TreeRoot;
            
            this.BackupTime = request.ArchiveTime;
            this.BackupJobId = request.ArchiveJobId;
            this.IsSearchTree = request.IsSearchTree;
            this.CacheSetting = request.CacheLocation;
            this.TreeMode = (TreeMode)Enum.Parse(typeof(TreeMode), request.LoadTreeOption.ToString(), true);
            this.DataLogicalDeviceList = request.DataLogicalDeviceList;
            this.IndexLogicalDevice = request.IndexLogicalDevice;
            //this.ArchiveRestoreOption = request.RestoreOption;
            //this.RestoreFSOption = request.RestoreFSOption;
            this.DestinationFSDevice = request.DestinationFSDevice;
            this.RestoreSecurityInfos = request.RestoreSecurityInfos;
            //this.ZipFilePassword = request.ZipFilePassword;
            //this.IsEndUserRestore = request.IsEndUserRequest;
            //this.EndUserRequestItems = request.EndUserRequestItems;
            //this.IsEndUserRestoreAccessTier = request.IsEndUserRestoreAccessTier;
            //this.RestoreToFSStorageString = request.EndUserRestoreToFSStorageString;
            //this.IntegrationModule = request.IntegrationModule;
            //this.TenantGroupId = request.TenantGroupId;
            this.KeepVersionsNumber = request.KeepVersionsNumber;
            this.RestoreVersionOption = request.RestoreVersionsOption;
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

        public GDriveRestoreJob()
        {
        }
    }
}