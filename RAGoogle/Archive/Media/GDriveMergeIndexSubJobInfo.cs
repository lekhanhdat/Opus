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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;

namespace RAGoogle.Archive.Media;

internal class GDriveMergeIndexSubJobInfo
{
    public String IndexVolume { get; set; }
    public BaseJobDto JobDto { get; set; }
    public LogicalDeviceDto IndexLogicalDevice { get; set; }
    public CacheSettingDto CacheSetting { get; set; }
    public List<MergeIndexJobState> MergeIndexJobsState { get; set; }
    public GDriveBackupRequest request { get; set; }
    public String DriveId { get; set; }
    public String DriveName { get; set; }

    public GDriveMergeIndexSubJobInfo(GDriveMergeIndexJobInfo info, GDriveBackupRequest request)
    {
        this.CacheSetting = info.CacheLocation;
        this.IndexLogicalDevice = info.IndexLogicalDevice;
        this.JobDto = info.JobDto;
        this.MergeIndexJobsState = info.MergeIndexJobsState;
        this.DriveId = request.DriveId;
        this.DriveName = request.DriveName;

        var generator = new GDriveArchiverVolumeGenerator();

        var volumeParam = new VolumeParameter()
        {
            DriveId = request.DriveId,
            DriveName = request.DriveName,
            IsSharedDrive = request.IsSharedDrive,
            TenantId = request.TenantId,
        };
        this.IndexVolume = generator.GenerateIndexVolume(volumeParam);
    }

}
