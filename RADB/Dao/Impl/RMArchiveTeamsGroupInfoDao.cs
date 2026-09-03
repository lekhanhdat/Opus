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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMArchiveTeamsGroupInfoDao : BaseDao<RMArchiveTeamsGroupInfo>, IRMArchiveTeamsGroupInfoDao
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

        public async Task<int> DeleteAllAsync()
        {
            return await BatchDeleteAsync(item => true);
        }

        public async Task<List<ArchiverTeamsGroupSizeInfo>> GetAllArchiverTeamsSizeInfoAsync()
        {
            using var context = this.GetNewContext();
            List<ArchiverTeamsGroupSizeInfo> allData = new List<ArchiverTeamsGroupSizeInfo>();
            string doubleFormatStr = "0.###############################";// unable use scientific notation
            int pageIndex = 0;
            int pageSize = 5000;
            do
            {
                var result = await context.RMArchiveTeamsGroupInfoes.Where(teams => teams.ArchivedSize > 1E-06)
                    .OrderBy(teams => teams.Id)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                foreach (var item in result)
                {
                    allData.Add(new ArchiverTeamsGroupSizeInfo()
                    {
                        MailboxAddress = item.MailboxAddress,
                        TotalArchivedSize = item.ArchivedSize.ToString(doubleFormatStr) + "GB",
                        TotalArchivedSizeWithoutRelatedSites = item.ArchivedSizeWithoutRelatedSites.ToString(doubleFormatStr) + "GB"
                    });
                }

                if (result.Count < pageSize)
                {
                    break;
                }
                else
                {
                    pageIndex++;
                }

            } while (true);

            return allData;
        }

        public async Task<List<RMArchiveTeamsGroupInfo>> GetArchiverTeamsGroupsByPagerAsync(int pageIndex, int pageSize, string searchKey = null)
        {
            using var context = this.GetNewContext();
            var query = context.RMArchiveTeamsGroupInfoes.Where(teams => teams.ArchivedSize > 1E-06);
            var likePattern = BuildLikePattern(searchKey);
            if (!string.IsNullOrWhiteSpace(likePattern))
            {
                query = query.Where(teams => SqlFunctions.PatIndex(likePattern, teams.MailboxAddress) > 0);
            }

            var result = await query.OrderByDescending(teams => teams.ArchivedSize)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return result;
        }

        public async Task<int> GetArchiverTeamsGroupTotalCountAsync(string searchKey = null)
        {
            using var context = this.GetNewContext();
            var query = context.RMArchiveTeamsGroupInfoes.Where(teams => teams.ArchivedSize > 1E-06);
            var likePattern = BuildLikePattern(searchKey);
            if (!string.IsNullOrWhiteSpace(likePattern))
            {
                query = query.Where(teams => SqlFunctions.PatIndex(likePattern, teams.MailboxAddress) > 0);
            }

            var count = await query.CountAsync();
            return count;
        }

        public async Task<List<RMArchiveTeamsGroupInfo>> GetArchiverTop50TeamsGroupsAsync()
        {
            using var context = this.GetNewContext();
            var result = await context.RMArchiveTeamsGroupInfoes.Where(site => site.ArchivedSize > 1E-06).OrderByDescending(data => data.ArchivedSize)
                .Take(50).ToListAsync();
            return result;
        }

        public async Task<double> GetArchivedSizeWithoutRelatedSitesAsync()
        {
            double total = 0;
            using var context = this.GetNewContext();
            var availableInfoes = await context.RMArchiveTeamsGroupInfoes.Where(teams => teams.ArchivedSizeWithoutRelatedSites > 1E-06).ToListAsync();
            if (availableInfoes.Any())
            {
                total = availableInfoes.Sum(i => i.ArchivedSizeWithoutRelatedSites);
            }
            return total;
        }

        public async Task<double> GetArchivedSizeAsync()
        {
            double total = 0;
            using var context = this.GetNewContext();
            var availableInfoes = await context.RMArchiveTeamsGroupInfoes.Where(teams => teams.ArchivedSize > 1E-06).ToListAsync();
            if (availableInfoes.Any())
            {
                total = availableInfoes.Sum(t => t.ArchivedSize);
            }
            return total;
        }

        public async Task UpdateAchivedTeamsGroupInfo(RMArchiveTeamsGroupInfo info)
        {
            using var context = this.GetNewContext();
            var existing = await context.RMArchiveTeamsGroupInfoes.FirstOrDefaultAsync(a => a.MailboxAddress == info.MailboxAddress);
            if (existing is not null)
            {
                existing.ArchivedSize = info.ArchivedSize;
                existing.ArchivedSizeWithoutRelatedSites = info.ArchivedSizeWithoutRelatedSites;
                await UpdateAsync(existing);
            }
            else
            {
                info.Id ??= Guid.NewGuid().ToString();
                Create(info);
            }
        }

        public async Task<int> BatchUpsertAsync(List<RMArchiveTeamsGroupInfo> infoes)
        {
            if (infoes == null || !infoes.Any())
                return 0;

            int count = 0;

            using var context = this.GetNewContext();

            var mailboxAddresses = infoes.Select(i => i.MailboxAddress).ToList();

            var existingRecords = await context.RMArchiveTeamsGroupInfoes
                .Where(a => mailboxAddresses.Contains(a.MailboxAddress))
                .ToDictionaryAsync(a => a.MailboxAddress);

            foreach (var info in infoes)
            {
                if (existingRecords.TryGetValue(info.MailboxAddress, out var existing))
                {
                    existing.ArchivedSize = info.ArchivedSize;
                    existing.ArchivedSizeWithoutRelatedSites = info.ArchivedSizeWithoutRelatedSites;
                    await UpdateAsync(existing);
                }
                else
                {
                    info.Id = Guid.NewGuid().ToString();
                    Create(info);
                }
                count++;
            }
            return count;
        }

        public async Task<RMArchiveTeamsGroupInfo> GetArchiverInfoByGroupMailboxAsync(string groupMailbox)
        {
            using var context = this.GetNewContext();
            return await context.RMArchiveTeamsGroupInfoes.FirstOrDefaultAsync(a => a.MailboxAddress == groupMailbox);
        }

        public async Task UpdateArchivedSizeByGroupMailboxAsync(string groupMailbox, double archivedSite)
        {
            string sql = "update {0}.RMArchiveTeamsGroupInfoes set ArchivedSize = @ArchivedSize where MailboxAddress = @MailboxAddress";
            var parameters = new[]
            {
                new SqlParameter("@ArchivedSize", archivedSite),
                new SqlParameter("@MailboxAddress", groupMailbox)
            };
            using (RMDbContext context = GetNewContext())
            {
                await context.Database.ExecuteSqlCommandAsync(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), parameters);
            }
        }

    }
}
