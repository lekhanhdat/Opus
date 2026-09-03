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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Model;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMArchiveGDriveInfoDao : BaseDao<RMArchiveGDriveInfo>, IRMArchiveGDriveInfoDao
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

        public void CreateOrUpdateDeletedInfo(string driveName, long size, string driveId, string tenantId, long deletedNumber)
        {
            var tempSize = (double)size / ContractConstants.GBSizeInterval;
            using var context = this.GetNewContext();
            var temp = context.RMArchiveGDriveInfo.FirstOrDefault(a => a.TenantId == tenantId && a.DriveId == driveId);
            if (temp != null)
            {
                temp.DeletedSize += tempSize;
                temp.ControlPlusDeletedNumber += deletedNumber;
                context.RMArchiveGDriveInfo.AddOrUpdate(temp);
            }
            else
            {
                RMArchiveGDriveInfo info = new RMArchiveGDriveInfo();
                info.Id = Guid.NewGuid().ToString();
                info.DriveName = driveName;
                info.DeletedSize = tempSize;
                info.FileNumber = 0;
                info.VersionNumber = 0;
                info.ArchivedSize = 0;
                info.DriveId = driveId;
                info.TenantId = tenantId;
                info.ControlPlusDeletedNumber = deletedNumber;
                context.RMArchiveGDriveInfo.Add(info);
            }
            context.SaveChanges();
        }

        public async Task<List<RMArchiveGDriveInfo>> GetGoogleArchiverByPagerAsync(int pageIndex, int pageSize, string searchKey = null)
        {
            using var context = this.GetNewContext();
            var query = context.RMArchiveGDriveInfo.Where(site => site.ArchivedSize > 1E-06);
            var likePattern = BuildLikePattern(searchKey);
            if (!string.IsNullOrWhiteSpace(likePattern))
            {
                query = query.Where(site => SqlFunctions.PatIndex(likePattern, site.DriveName) > 0);
            }

            var result = await query.OrderByDescending(site => site.ArchivedSize)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return result;
        }

        public async Task<int> GetGoogleArchiverTotalCount4DashboardAsync(string searchKey = null)
        {
            using var context = this.GetNewContext();
            var query = context.RMArchiveGDriveInfo.Where(site => site.ArchivedSize > 1E-06);
            var likePattern = BuildLikePattern(searchKey);
            if (!string.IsNullOrWhiteSpace(likePattern))
            {
                query = query.Where(site => SqlFunctions.PatIndex(likePattern, site.DriveName) > 0);
            }

            var count = await query.CountAsync();
            return count;
        }

        public async Task<double> GetGoogleArchivedFileCount4DashboardAsync()
        {
            using var context = this.GetNewContext();
            var count = await context.RMArchiveGDriveInfo.Where(site => site.ArchivedSize > 1E-06).SumAsync(site => site.FileNumber);
            return count ?? 0;
        }
        public async Task<long> GetGoogleDeletedFileCount4DashboardAsync()
        {
            using var context = this.GetNewContext();
            var count = await context.RMArchiveGDriveInfo.Where(site => site.ControlPlusDeletedNumber > 0).SumAsync(site => site.ControlPlusDeletedNumber);

            return count ?? 0;
        }
        public void UpdateGoogleArchiverInfo(string driveName, long fileNumber, long versionNumber, string tenantId, string driveId, double size = 0)
        {
            using var context = this.GetNewContext();
            var temp = context.RMArchiveGDriveInfo.Where(a => a.DriveName == driveName && a.TenantId == tenantId && a.DriveId == driveId).FirstOrDefault();
            if (temp != null)
            {
                temp.FileNumber = (double)fileNumber / 1000;
                temp.VersionNumber = (double)versionNumber / 1000;
                temp.ArchivedSize = size;
                //temp.DeletedSize = (double)deleteSize / 1000; 
                context.RMArchiveGDriveInfo.AddOrUpdate(temp);
                context.SaveChanges();
            }
            else
            {
                RMArchiveGDriveInfo info = new RMArchiveGDriveInfo();
                info.Id = Guid.NewGuid().ToString();
                info.DriveName = driveName;
                info.DriveId = driveId;
                info.FileNumber = (double)fileNumber / 1000;
                info.VersionNumber = (double)versionNumber / 1000;
                //info.DeletedSize = (double)deleteSize / 1000;
                info.ArchivedSize = size;
                info.TenantId = tenantId;
                context.RMArchiveGDriveInfo.AddOrUpdate(info);
                context.SaveChanges();
            }
        }

        public async Task<int> SaveRetentionDriveInfo(RMRetentionGDriveInfo info)
        {
            using var context = this.GetNewContext();
            var exist = await context.RMRetentionGDriveInfoes.FirstOrDefaultAsync(i => i.DriveId == info.DriveId && i.RetentionJobID == info.RetentionJobID && i.ContainerId == info.ContainerId);
            if (exist == null)
            {
                context.RMRetentionGDriveInfoes.Add(info);
                return await context.SaveChangesAsync();
            }
            else
            {
                exist.FileNumber += info.FileNumber;
                return await context.SaveChangesAsync();
            }
        }

        public async Task<int> DeleteAllAsync()
        {
            return await BatchDeleteAsync(item => true);
        }

        public void UpdateGoogleArchiveInfo(string driveId, double size)
        {
            using var context = this.GetNewContext();
            var temp = context.RMArchiveGDriveInfo.Where(a => a.DriveId == driveId).FirstOrDefault();
            if (temp != null)
            {
                temp.ArchivedSize = size;
                context.RMArchiveGDriveInfo.AddOrUpdate(temp);
                context.SaveChanges();
            }
        }
    }
}
