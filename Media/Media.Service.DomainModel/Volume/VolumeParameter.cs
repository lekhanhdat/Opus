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
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using global::Media.Service.DomainModel.MoveDataTier;

    #endregion using directives

    public class VolumeParameter
    {
        public String HostName { get; set; }

        public String FarmName { get; set; }

        public String PlanId { get; set; }

        public String CycleId { get; set; }

        public String JobId { get; set; }

        public String WebApplicationUrl { get; set; }

        public String SiteCollectionUrl { get; set; }

        public String IndexCrawlId { get; set; }
        public String EmailAddress { get; set; }//exo archiver
        public String TempFolder { get; set; }//exo archiver
        #region GDrive
        public String DriveId { get; set; } 
        public String DriveName { get; set; }
        public Boolean IsSharedDrive { get; set; }
        public String TenantId { get; set; }
        #endregion
        public VolumeParameter() { }
        public VolumeParameter(string siteUrl)
        {
            SiteCollectionUrl = siteUrl;
        }
        public VolumeParameter(PlatformRetentionInfo retentionInfo, string jobId)
        {
            FarmName = retentionInfo.FarmName;
            PlanId = retentionInfo.PlanId;
            CycleId = jobId;
            HostName = retentionInfo.FarmName;
            JobId = jobId;
        }

        public VolumeParameter(ArchiverBackupRequest request)
        {
            FarmName = request.ArchiverSiteInfoDto.FarmName;
            WebApplicationUrl = request.ArchiverSiteInfoDto.WebApplicationUrl;
            SiteCollectionUrl = request.ArchiverSiteInfoDto.SiteUrl;
        }

        public VolumeParameter(ArchiverRestoreRequest request)
        {
            FarmName = request.FarmName;
            WebApplicationUrl = request.WebAppUrl;
            SiteCollectionUrl = request.SiteUrl;
        }

        public VolumeParameter(ArchiverRestoreJob restoreJob)
        {
            FarmName = restoreJob.FarmName;
            WebApplicationUrl = restoreJob.WebAppUrl;
            SiteCollectionUrl = restoreJob.SiteUrl;
        }
        public VolumeParameter(MoveDataTierJob moveDataTierJob)
        {
            FarmName = moveDataTierJob.FarmName;
            WebApplicationUrl = moveDataTierJob.WebAppUrl;
            SiteCollectionUrl = moveDataTierJob.SiteUrl;
        }
        public VolumeParameter(ArchiverBrowseInfo browseInfo)
        {
            FarmName = browseInfo.FarmName;
            WebApplicationUrl = browseInfo.WebAppUrl;
            SiteCollectionUrl = browseInfo.SiteUrl;
        }
        public VolumeParameter(GDriveBrowseInfo browseInfo)
        {
            SiteCollectionUrl = browseInfo.SiteUrl;
        }
        public VolumeParameter(GDriveRestoreRequest request)
        {
            SiteCollectionUrl = request.SiteUrl;
        }
        public VolumeParameter(ArchiverDataInfo dataInfo)
        {
            FarmName = dataInfo.FarmName;
            SiteCollectionUrl = dataInfo.SiteUrl;
        }

        public VolumeParameter(ArchiverRetentionInfo retentionInfo)
        {
            FarmName = retentionInfo.FarmName;
            WebApplicationUrl = retentionInfo.WebApp;
            SiteCollectionUrl = retentionInfo.SiteUrl;
            TenantId = retentionInfo.WebApp;
            DriveId = retentionInfo.SiteUrl;
        }

        public VolumeParameter(ArchiverRebuildStubInfo rebuildStubInfo)
        {
            FarmName = rebuildStubInfo.FarmName;
            WebApplicationUrl = rebuildStubInfo.WebApp;
            SiteCollectionUrl = rebuildStubInfo.SiteUrl;
        }

        public VolumeParameter(MergeIndexSubJobInfo jobInfo)
        {
            FarmName = jobInfo.FarmName;
            WebApplicationUrl = jobInfo.WebAppName;
            SiteCollectionUrl = jobInfo.SiteUrl;
        }

        public VolumeParameter(PlatformCheckInfo checkInfo)
        {
            FarmName = checkInfo.FarmName;
            PlanId = checkInfo.BackupPlanId;
            CycleId = checkInfo.BackupCycleId;
        }

        public VolumeParameter(DRInfo info)
        {
            HostName = info.FarmName;
            FarmName = info.FarmName;
            PlanId = info.PlanId;
            CycleId = info.JobId;
            JobId = info.JobId;
        }

        public VolumeParameter(ArchiverBackupIndexRequest req)
        {
            FarmName = req.FarmName;
            JobId = req.BackupJobId;
            WebApplicationUrl = req.WebAppUrl;
            SiteCollectionUrl = req.SiteUrl;
        }

        public VolumeParameter(EndUserBrowseInfo browseInfo)
        {
            FarmName = browseInfo.FarmName;
            WebApplicationUrl = browseInfo.WebAppUrl;
            SiteCollectionUrl = browseInfo.SiteUrl;
        }

        public VolumeParameter(ErrorPageCheckInfo checkInfo)
        {
            FarmName = checkInfo.FarmName;
            WebApplicationUrl = checkInfo.WebAppUrl;
            SiteCollectionUrl = checkInfo.SiteUrl;
        }

        public VolumeParameter(EndUserDownloadInfo downloadInfo)
        {
            FarmName = downloadInfo.FarmName;
            WebApplicationUrl = downloadInfo.WebAppUrl;
            SiteCollectionUrl = downloadInfo.SiteUrl;
        }

        public VolumeParameter(GranularBrowseInfo browseInfo)
        {
            FarmName = browseInfo.FarmName;
            PlanId = browseInfo.BackupPlanId;
            CycleId = browseInfo.BackupCycleID;
        }

        public VolumeParameter(ExchangeRestoreRequest request)
        {
            PlanId = request.BackupPlanId;
            CycleId = request.BackupCycleId;
            JobId = request.BackupJobId;
            //ModulePath = request.GetModulePath();
        }

        public override string ToString()
        {
            return string.Format("VolumeParameter: FarmName : {0},PlanId : {1}, JobId: {2}, SiteCollectionUrl {3}, IndexCrawlId: {4}.",
                FarmName, PlanId, JobId, SiteCollectionUrl, IndexCrawlId);
        }
    }
}