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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.SharePoint.Client;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.ExtendedProperties;
using AvePoint.RA.Contract.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMArchiveSiteInfoDao : BaseDao<RMArchiveSiteInfo>, IRMArchiveSiteInfoDao
    {
        private static string BuildLikePattern(string searchKey)
        {
            if (string.IsNullOrWhiteSpace(searchKey))
            {
                return null;
            }

            var normalizedKey = searchKey.Trim();
            var hasWildcard = normalizedKey.Contains("*") || normalizedKey.Contains("?");
            normalizedKey = normalizedKey
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]")
                .Replace("*", "%")
                .Replace("?", "_");

            return hasWildcard ? normalizedKey : $"%{normalizedKey}%";
        }

        public async Task<List<RMArchiveSiteInfo>> GetArchiverTop50SitesAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo.AsNoTracking().Where(site => site.ArchivedSize > 1E-06 || site.DeletedSize > 1E-06 || site.ArchiveBy365Size > 1E-06).OrderByDescending(data => data.ArchivedSize)
                .Take(50).ToListAsync();
            return result;
        }

        public async Task<List<RMArchiveSiteInfo>> GetArchiverSitesByPagerAsync(int pageIndex, int pageSize, string searchKey = null)
        {
            using var context = this.GetNewContext();
            var query = context.RMArchiveSiteInfo.Where(site => site.ArchivedSize > 1E-06 || site.DeletedSize > 1E-06 || site.ArchiveBy365Size > 1E-06);
            var likePattern = BuildLikePattern(searchKey);
            if (!string.IsNullOrWhiteSpace(likePattern))
            {
                query = query.Where(site => SqlFunctions.PatIndex(likePattern, site.SiteUrl) > 0);
            }

            var result = await query.OrderByDescending(site => site.ArchivedSize)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return result;
        }

        public async Task<int> GetAllArchivedSitesCountAsync()
        {
            using var context = this.GetNewContext();
            var count = await context.RMArchiveSiteInfo.CountAsync();
            return count;
        }

        public async Task<int> GetArchiverSitesTotalCount4DashboardAsync(string searchKey = null)
        {
            using var context = this.GetNewContext();
            var query = context.RMArchiveSiteInfo.Where(site => site.ArchivedSize > 1E-06 || site.DeletedSize > 1E-06 || site.ArchiveBy365Size > 1E-06);
            var likePattern = BuildLikePattern(searchKey);
            if (!string.IsNullOrWhiteSpace(likePattern))
            {
                query = query.Where(site => SqlFunctions.PatIndex(likePattern, site.SiteUrl) > 0);
            }

            var count = await query.CountAsync();
            return count;
        }

        public async Task<double> GetArchiverDataSizeAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo.AsNoTracking().Where(site => site.ArchivedSize > 1E-06).SumAsync(site => site.ArchivedSize);
            return result;
        }

        public async Task<double> GetSharePointArchiverDataSizeAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo
                .AsNoTracking()
                .Where(site => site.ArchivedSize > 1E-06
                    && (site.SiteUrl == null || !site.SiteUrl.Contains("-my.sharepoint.com")))
                .SumAsync(site => (double?)site.ArchivedSize);
            return result ?? 0;
        }

        public async Task<double> GetOneDriveArchiverDataSizeAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo
                .AsNoTracking()
                .Where(site => site.ArchivedSize > 1E-06
                    && site.SiteUrl != null
                    && site.SiteUrl.Contains("-my.sharepoint.com"))
                .SumAsync(site => (double?)site.ArchivedSize);
            return result ?? 0;
        }

        public async Task<double> GetDeleteFileCountAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo.Where(site => site.DeletedSize > 1E-06).SumAsync(site => site.DeleteFileNumbers);
            return result;
        }

        public async Task<double> GetDeleteSizeAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo.Where(site => site.DeletedSize > 1E-06).SumAsync(site => site.DeletedSize);
            return result;
        }

        public async Task<bool> ExistArchvierData()
        {
            using var context = this.GetNewContext();
            return await context.RMArchiveSiteInfo.AnyAsync();
        }

        public async Task<double> GetArchiverFileCountAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo.Where(site => site.ArchivedSize > 1E-06).SumAsync(site => site.FileNumber);
            return result;
        }

        public async Task<TenantArchiverDataInfo> GetArchiverDataSizeByTenantAsync(Guid O365tenantId)
        {
            try
            {
                using var context = this.GetNewContext();
                var totalSize = 0.00;
                var totalCount = 0.00;

                var currentTenantInfo = context.RMArchiveSiteInfo
                    .Where(site => !string.IsNullOrEmpty(site.O365TenantId) && site.O365TenantId == O365tenantId.ToString() && site.ArchivedSize > 1E-06); 

                var currentTeamsInfo = context.RMArchiveTeamsGroupInfoes
                    .Where(site => !string.IsNullOrEmpty(site.O365TenantId) && site.O365TenantId == O365tenantId.ToString() && site.ArchivedSizeWithoutRelatedSites > 1E-06);

                if (currentTenantInfo.Any())
                {
                    totalSize += await currentTenantInfo
                    .SumAsync(site => site.ArchivedSize);

                    totalCount += await currentTenantInfo
                    .SumAsync(site => site.FileNumber);
                }

                if (currentTeamsInfo.Any())
                {
                    totalSize += await currentTeamsInfo
                    .SumAsync(site => site.ArchivedSizeWithoutRelatedSites);
                }

                return new TenantArchiverDataInfo
                {
                    ArchivedDataSize = totalSize < 0.005 ? 0 : totalSize,
                    ArchivedFileNumber = totalCount < 0.005 ? 0 : totalCount,
                };
            }
            catch(Exception e)
            {
                return new TenantArchiverDataInfo
                {
                    ArchivedDataSize = 0,
                    ArchivedDataSizeUnit = ArchiverDataUnit.GB.ToString(),
                    ArchivedFileNumberUnit = ArchiverDataUnit.K.ToString(),
                    ArchivedFileNumber = 0,
                };
            }
        }

        public async Task<RMArchiveSiteInfo> GetArchiverSiteInfoBySiteAndTenant(string O365tenantId, string siteId)
        {
            using var context = GetNewContext();
            return await context.RMArchiveSiteInfo.Where(_ => !string.IsNullOrEmpty(_.O365TenantId) && _.O365TenantId.Equals(O365tenantId) &&
                        !string.IsNullOrEmpty(_.SiteId) && _.SiteId.Equals(siteId)).FirstOrDefaultAsync();
        }
        public async Task<List<RMArchiveSiteInfo>> GetAllArchiverSiteInfoByTenant(string O365tenantId, int pageIndex, int pageSize, List<string> siteIds)
        {
            using var context = GetNewContext();
            var tenant = (O365tenantId ?? string.Empty).Trim();
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 500 : pageSize;
            var normalizedSiteIds = siteIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct()
                .ToList();

            if (normalizedSiteIds != null && normalizedSiteIds.Count > 0)
            {
                return await context.RMArchiveSiteInfo
                    .Where(_ => _.O365TenantId == tenant && _.SiteId != null && normalizedSiteIds.Contains(_.SiteId))
                    .OrderByDescending(_ => _.ArchivedSize)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                return await context.RMArchiveSiteInfo
                    .Where(_ => _.O365TenantId.ToLower() == tenant)
                    .OrderByDescending(_ => _.ArchivedSize)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
        }
        public async Task<RMArchiveSiteInfo> GetArchiverSiteInfoByTenant(string O365tenantId)
        {
            using var context = GetNewContext();
            var tenant = (O365tenantId ?? string.Empty).Trim().ToLower();

            var query = context.RMArchiveSiteInfo
                .Where(_ => !string.IsNullOrEmpty(_.O365TenantId) && _.O365TenantId.ToLower() == tenant);

            var agg = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    ArchivedSize = g.Sum(x => (double?)x.ArchivedSize) ?? 0d,
                    ArchiverCount = g.Sum(x => (double?)x.FileNumber) ?? 0d,
                    DeletedSize = g.Sum(x => (double?)x.DeletedSize) ?? 0d,
                    DeleteCount = g.Sum(x => (double?)x.DeleteFileNumbers) ?? 0d
                })
                .FirstOrDefaultAsync();

            return new RMArchiveSiteInfo
            {
                O365TenantId = O365tenantId,
                ArchivedSize = agg?.ArchivedSize ?? 0d,
                FileNumber = agg?.ArchiverCount ?? 0d,
                DeletedSize = agg?.DeletedSize ?? 0d,
                DeleteFileNumbers = agg?.DeleteCount ?? 0d
            };
        }

        public void UpdateArchiverInfo(string siteUrl, long fileNumber, long versionNumber, string o365TenantId, double size = 0, string siteId = "")
        {
            using var context = this.GetNewContext();
            var temp = context.RMArchiveSiteInfo.Where(a => a.SiteUrl == siteUrl).FirstOrDefault();
            if (temp != null)
            {
                temp.FileNumber = (double)fileNumber / 1000;
                temp.VersionNumber = (double)versionNumber / 1000;
                temp.ArchivedSize = size;
                context.RMArchiveSiteInfo.AddOrUpdate(temp);
                context.SaveChanges();
            }
            else
            {
                RMArchiveSiteInfo info = new RMArchiveSiteInfo();
                info.Id = Guid.NewGuid().ToString();
                info.SiteUrl = siteUrl;
                info.SiteId = siteId;
                info.FileNumber = (double)fileNumber / 1000;
                info.VersionNumber = (double)versionNumber / 1000;
                info.ArchivedSize = size;
                info.O365TenantId = o365TenantId;
                context.RMArchiveSiteInfo.AddOrUpdate(info);
                context.SaveChanges();
            }
        }
        public void CreateOrUpdateDeletedInfo(string siteUrl, long size,string siteId, string o365TenantId, int deleteFileNumbers)
        {
            var tempSize = (double)size / ContractConstants.GBSizeInterval;
            using var context = this.GetNewContext();
            var temp = context.RMArchiveSiteInfo.FirstOrDefault(a => a.SiteUrl == siteUrl);
            if (temp != null)
            {
                temp.DeletedSize += tempSize;
                temp.DeleteFileNumbers = deleteFileNumbers;
                context.RMArchiveSiteInfo.AddOrUpdate(temp);
            }
            else
            {
                RMArchiveSiteInfo info = new RMArchiveSiteInfo();
                info.Id = Guid.NewGuid().ToString();
                info.SiteUrl = siteUrl;
                info.DeletedSize = tempSize;
                info.DeleteFileNumbers = deleteFileNumbers;
                info.FileNumber = 0;
                info.VersionNumber = 0;
                info.ArchivedSize = 0;
                info.SiteId = siteId;
                info.O365TenantId = o365TenantId;
                context.RMArchiveSiteInfo.Add(info);
            }
            context.SaveChanges();
        }
        public void CreateOrUpdateArchiveBy365Info(string siteUrl, long size, string siteId, string o365TenantId)
        {
            var tempSize = (double)size / ContractConstants.GBSizeInterval;
            using var context = this.GetNewContext();
            var temp = context.RMArchiveSiteInfo.FirstOrDefault(a => a.SiteUrl == siteUrl);
            if (temp != null)
            {
                temp.ArchiveBy365Size += tempSize;
                context.RMArchiveSiteInfo.AddOrUpdate(temp);
            }
            else
            {
                RMArchiveSiteInfo info = new RMArchiveSiteInfo();
                info.Id = Guid.NewGuid().ToString();
                info.SiteUrl = siteUrl;
                info.ArchiveBy365Size = tempSize;
                info.DeleteFileNumbers = 0;
                info.DeletedSize = 0;
                info.FileNumber = 0;
                info.VersionNumber = 0;
                info.ArchivedSize = 0;
                info.SiteId = siteId;
                info.O365TenantId = o365TenantId;
                context.RMArchiveSiteInfo.Add(info);
            }
            context.SaveChanges();
        }
        public void UpdateArchiverSize(string siteUrl, double size)
        {
            using var context = this.GetNewContext();
            var temp = context.RMArchiveSiteInfo.Where(a => a.SiteUrl == siteUrl).FirstOrDefault();
            if (temp != null)
            {
                temp.ArchivedSize = size;
                context.RMArchiveSiteInfo.AddOrUpdate(temp);
                context.SaveChanges();
            }
        }
        public async Task<double> GetArchiverVersionCountAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveSiteInfo.Where(site => site.ArchivedSize > 1E-06).SumAsync(site => site.VersionNumber);
            return result;
        }

        public async Task<int> DeleteAllAsync()
        {
            return await BatchDeleteAsync(item => true);
        }

        public async Task<int> SaveRetentionSiteInfo(RMRetentionSiteInfo info)
        {
            using var context = this.GetNewContext();
            var exist = await context.RMRetentionSiteInfoes.FirstOrDefaultAsync(i => i.SiteUrl == info.SiteUrl && i.RetentionJobID == info.RetentionJobID && i.ListUrl == info.ListUrl);
            if (exist == null)
            {
                context.RMRetentionSiteInfoes.Add(info);
                return await context.SaveChangesAsync();
            }
            else
            {
                exist.FileNumber += info.FileNumber;
                return await context.SaveChangesAsync();
            }
        }

        public async Task<long> GetDestructionFileNumberBySite(string siteURL, string listURL = "")
        {
            using var context = this.GetNewContext();
            if (string.IsNullOrEmpty(listURL))
            {
                return await context.RMRetentionSiteInfoes.Where(i => i.SiteUrl == siteURL).Select(x => x.FileNumber).DefaultIfEmpty(0).SumAsync();
            }
            else
            {
                return await context.RMRetentionSiteInfoes.Where(i => i.SiteUrl == siteURL && i.ListUrl == listURL).Select(x => x.FileNumber).DefaultIfEmpty(0).SumAsync();
            }
        }

        public int AddO365TenantIdInfo()
        {
            using var context = this.GetNewContext();
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var updateAllSql = $@"UPDATE B
SET B.O365TenantId = A.TenantId
FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMArchiveSiteInfoes AS B
INNER JOIN [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes AS A ON B.SiteUrl = A.Url
WHERE B.O365TenantId IS NULL OR B.O365TenantId = '';";

            return ExecuteWithRetry(context =>
            {
                return context.Database.ExecuteSqlCommand(updateAllSql);
            });
        }

        public int UpdateO365TenantIdInfo(List<RMArchiveSiteInfo> siteInfos)
        {
            using var context = this.GetNewContext();
            context.RMArchiveSiteInfo.AddOrUpdate([.. siteInfos]);
            return context.SaveChanges();
        }

        public List<RMArchiveSiteInfo> GetNoO365TenatIdSites()
        {
            using var context = this.GetNewContext();
            return context.RMArchiveSiteInfo.Where(item => string.IsNullOrEmpty(item.O365TenantId)).ToList();
        }

        public int GetNoO365TenatIdSitesCount()
        {
            using var context = this.GetNewContext();
            return context.RMArchiveSiteInfo.Where(item => string.IsNullOrEmpty(item.O365TenantId)).Count();
        }

        public List<RMArchiveSiteInfo> GetSiteInfoesBySiteUrls(List<string> siteUrls)
        {
            using var context = this.GetNewContext();
            return context.RMArchiveSiteInfo.Where(item => siteUrls.Contains(item.SiteUrl)).ToList();
        }
    }
}