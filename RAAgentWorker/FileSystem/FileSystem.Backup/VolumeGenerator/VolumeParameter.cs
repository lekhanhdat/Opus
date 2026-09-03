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
    using RAFileSystem.FileSystem.FileSystem.Backup;

    #endregion using directives

    public class VolumeParameter
    {
        public String HostName { get; set; }

        public String FarmName { get; set; }

        public String PlanId { get; set; }

        public String CycleId { get; set; }

        public String JobId { get; set; }

        public String WebApplicationUrl { get; set; }
        public String ConnectionId { get; set; }
        public String ConnectionName { get; set; }
        public String SiteCollectionUrl { get; set; }

        public String IndexCrawlId { get; set; }

        public VolumeParameter() { }
        public VolumeParameter(string siteUrl)
        {
            SiteCollectionUrl = siteUrl;
        }


        public VolumeParameter(FSArchiverBackupRequest request)
        {
            //FarmName = request.ArchiverSiteInfoDto;
            //WebApplicationUrl = request.ArchiverSiteInfoDto.WebApplicationUrl;
            //SiteCollectionUrl = request.ArchiverSiteInfoDto.SiteUrl;
            ConnectionId = request.ArchiverSiteInfoDto.ConnectionId;
            ConnectionName = request.ArchiverSiteInfoDto.ConnectionName;
        }
        public VolumeParameter(ArchiverBrowseInfo browseInfo)
        {
            FarmName = browseInfo.FarmName;
            WebApplicationUrl = browseInfo.WebAppUrl;
            SiteCollectionUrl = browseInfo.SiteUrl;
        }


        //public VolumeParameter(EndUserBrowseInfo browseInfo)
        //{
        //    FarmName = browseInfo.FarmName;
        //    WebApplicationUrl = browseInfo.WebAppUrl;
        //    SiteCollectionUrl = browseInfo.SiteUrl;
        //}

        public VolumeParameter(GranularBrowseInfo browseInfo)
        {
            FarmName = browseInfo.FarmName;
            PlanId = browseInfo.BackupPlanId;
            CycleId = browseInfo.BackupCycleID;
        }

        public override string ToString()
        {
            return string.Format("VolumeParameter: FarmName : {0},PlanId : {1}, JobId: {2}, SiteCollectionUrl {3}, IndexCrawlId: {4}.",
                FarmName, PlanId, JobId, SiteCollectionUrl, IndexCrawlId);
        }
    }
}