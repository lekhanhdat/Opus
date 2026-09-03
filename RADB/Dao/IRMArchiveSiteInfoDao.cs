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
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.DB.Model;
using Microsoft.ProjectServer.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMArchiveSiteInfoDao : IBaseDao<RMArchiveSiteInfo>
    {

        Task<List<RMArchiveSiteInfo>> GetArchiverTop50SitesAsync();
        Task<List<RMArchiveSiteInfo>> GetArchiverSitesByPagerAsync(int pageIndex, int pageSize, string searchKey = null);
        Task<int> GetAllArchivedSitesCountAsync();
        Task<int> GetArchiverSitesTotalCount4DashboardAsync(string searchKey = null);

        Task<double> GetArchiverDataSizeAsync();
        Task<double> GetSharePointArchiverDataSizeAsync();
        Task<double> GetOneDriveArchiverDataSizeAsync();
        Task<bool> ExistArchvierData();
        Task<double> GetArchiverFileCountAsync();
        Task<double> GetArchiverVersionCountAsync();
        Task<double> GetDeleteFileCountAsync();
        Task<double> GetDeleteSizeAsync();

        Task<TenantArchiverDataInfo> GetArchiverDataSizeByTenantAsync(Guid O365tenantId);

        void UpdateArchiverInfo(string siteUrl, long fileNumber, long versionNumber, string o365TenantId, double size = 0, string siteId = "");
        void UpdateArchiverSize(string siteUrl, double size);
        void CreateOrUpdateDeletedInfo(string siteUrl, long size, string siteId, string o365TenantId, int deleteFileNumbers);

        void CreateOrUpdateArchiveBy365Info(string siteUrl, long size, string siteId, string o365TenantId);
        Task<int> DeleteAllAsync();
        Task<int> SaveRetentionSiteInfo(RMRetentionSiteInfo info);
        Task<long> GetDestructionFileNumberBySite(string siteURL, string listURL = "");

        int AddO365TenantIdInfo();

        int UpdateO365TenantIdInfo(List<RMArchiveSiteInfo> siteInfos);

        List<RMArchiveSiteInfo> GetNoO365TenatIdSites();

        int GetNoO365TenatIdSitesCount();

        List<RMArchiveSiteInfo> GetSiteInfoesBySiteUrls(List<string> siteUrls);
        Task<RMArchiveSiteInfo> GetArchiverSiteInfoBySiteAndTenant(string O365tenantId, string siteId);
        Task<RMArchiveSiteInfo> GetArchiverSiteInfoByTenant(string O365tenantId);
        Task<List<RMArchiveSiteInfo>> GetAllArchiverSiteInfoByTenant(string O365tenantId,int pageIndex,int pageSize,List<string>siteIds);
    }
}
