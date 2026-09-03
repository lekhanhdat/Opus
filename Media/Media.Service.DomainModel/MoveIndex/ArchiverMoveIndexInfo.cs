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
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    #endregion

    public class ArchiverMoveIndexInfo : IMoveIndexInfo
    {
        public String FarmName { get; set; }
        public List<String> SiteUrls { get; set; }
        public List<String> TeamsSiteUrls { get; set; }
        public List<String> ExchangeSiteUrls { get; set; }
        public List<ArchiverSiteMasterIndexContract> GDriveIndexInfos { get; set; }
        public List<FSMasterIndexContract> FSIndexInfos { get; set; }
        public String WebApp { get; set; }

        public String JobId { get; set; }
        public String SubJobId { set; get; }


        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public LogicalDeviceDto DestinationDevice { get; set; }
        public Int32 Type { set; get; }
        public Boolean hasStorageInfo { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public Int64 Stamp { set; get; }
        public ArchiverVolumeGenerator VolumeGenerator { get; set; }
        public TeamsArchiverVolumeGenerator TeamsVolumeGenerator { get; set; }
        public ExchangeVolumeGenerator ExchangeVolumeGenerator { get; set; }
        public GDriveArchiverVolumeGenerator GDriveVolumnGenerator { get; set; }



        public ArchiverMoveIndexInfo(ArchiverMoveIndexJobInfo jobInfo)
        {
            this.JobId = jobInfo.JobId;
            this.SubJobId = jobInfo.SubJobId;
            this.FarmName = jobInfo.FarmName;
            this.SiteUrls = jobInfo.SiteUrls;
            this.GDriveIndexInfos = jobInfo.GDriveIndexInfos;
            this.TeamsSiteUrls = jobInfo.TeamsSiteUrls;
            this.ExchangeSiteUrls = jobInfo.ExchangeSiteUrls;
            this.FSIndexInfos = jobInfo.FSIndexInfos;
            this.WebApp = jobInfo.WebApp;
            this.IndexLogicalDevice = jobInfo.IndexLogicalDevice;
            this.DestinationDevice = jobInfo.DestinationDevice;
            this.CacheSetting = jobInfo.CacheSetting;
            this.VolumeGenerator = new ArchiverVolumeGenerator();
            this.TeamsVolumeGenerator = new TeamsArchiverVolumeGenerator();
            this.ExchangeVolumeGenerator = new ExchangeVolumeGenerator();
            this.GDriveVolumnGenerator  = new GDriveArchiverVolumeGenerator();
        }
    }
}
