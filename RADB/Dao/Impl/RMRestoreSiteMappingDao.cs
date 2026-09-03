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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RestoreCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMRestoreSiteMappingDao : BaseDao<RMRestoreSiteMapping>, IRMRestoreSiteMappingDao
    {
        public void DeleteAllMapping()
        {
            ExecuteWithRetry(context =>
            {
                context.RMRestoreSiteMappings.RemoveRange(context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping));
                context.SaveChanges();
            });
        }
        public void DeleteAllMappingByPage()
        {
            const int pageSize = 2000;
            bool hasMoreRecords = true;
            while (hasMoreRecords)
            {
                ExecuteWithRetry(context =>
                {
                    // 获取要删除的记录
                    var recordsToDelete = context.RMRestoreSiteMappings
                        .Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping)
                        .Take(pageSize)
                        .ToList();

                    // 检查是否还有更多记录
                    hasMoreRecords = recordsToDelete.Count == pageSize;

                    if (recordsToDelete.Any())
                    {
                        // 删除当前页的记录
                        context.RMRestoreSiteMappings.RemoveRange(recordsToDelete);
                        context.SaveChanges();
                    }
                    else
                    {
                        hasMoreRecords = false;
                    }
                });
            }
        }

        public List<RMRestoreSiteMapping> GetAllMappings()
        {
            try
            {
                using (var ctx = GetNewContext())
                {
                    return ctx.RMRestoreSiteMappings.AsNoTracking().Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping).OrderByDescending(a => a.intId).ToList();
                }
            }
            catch (Exception)
            {
                int pageIndex = 0;
                int pageSize = 2000;
                int total = 0;
                List<RMRestoreSiteMapping> res = new List<RMRestoreSiteMapping>();
                while (pageIndex * pageIndex < total || pageIndex == 0)
                {
                    res.AddRange(GetMappings(pageIndex++, pageSize, out total));
                }
                return res;
            }
        }

        public List<RMRestoreSiteMapping> GetMappingsById(IEnumerable<String> ids)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRestoreSiteMappings.AsNoTracking().Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping && ids.Contains(s.Id)).OrderByDescending(a => a.intId).ToList();
            }
        }

        public List<RMRestoreSiteMapping> GetRecordsByIds(IEnumerable<String> ids)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRestoreSiteMappings.AsNoTracking().Where(s => ids.Contains(s.Id)).OrderByDescending(a => a.intId).ToList();
            }
        }

        public List<RMRestoreSiteMapping> GetMappings(int pageIndex, int pageSize, out int totalRecord)
        {
            IQueryable<RMRestoreSiteMapping> query = null;
            using (var context = GetNewContext())
            {
                query = context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping).OrderByDescending(mappint => mappint.intId);
                return query.Paging(pageIndex, pageSize, out totalRecord).ToList();
            }
        }

        public List<RMRestoreSiteMapping> GetSiteMappingsByTargetSCUrl(string targetSCUrl)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRestoreSiteMappings
                    .Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping && s.TargetSiteUrl == targetSCUrl)
                    .Where(s => s.TargetSiteUrl == 
                    ctx.RMRestoreSiteMappings.FirstOrDefault(x => x.SourceSiteUrl == s.SourceSiteUrl && x.SettingFlag == RestoreSettingFlag.SiteMapping).TargetSiteUrl
                    )
                    .ToList();
            }
        }

        public List<String> GetSourceSCUrlsByTargetSCUrl(string targetSCUrl)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRestoreSiteMappings
                    .Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping && s.TargetSiteUrl == targetSCUrl)
                    .Select(s => s.SourceSiteUrl).ToList();
            }
        }

        public async Task<List<string>> GetSourceSCUrlsByTargetSCUrlAsync(string targetSCUrl)
        {
            using (var context = GetNewContext())
            {
                return await context.RMRestoreSiteMappings
                    .Where(m => m.TargetSiteUrl == targetSCUrl)
                    .Select(m => m.SourceSiteUrl)
                    .ToListAsync();
            }
        }

        public void DeleteMappingBySourceUrl(string sourceUrl)
        {
            ExecuteWithRetry(context =>
            {
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMRestoreSiteMappings] WHERE SourceSiteUrl = @SourceSiteUrl and SettingFlag = @RestoreSettingFlag";
                var parameters = new List<SqlParameter> 
                {
                    new("@SourceSiteUrl", sourceUrl),
                    new("@RestoreSettingFlag", RestoreSettingFlag.SiteMapping),
                };
                context.Database.ExecuteSqlCommand(sql, parameters);
            });
        }

        public void BatchDeleteMapping(params string[] ids)
        {
            if(ids.Length == 0)
            {
                return;
            }
            ExecuteWithRetry(context =>
            {
                var parameterizedStatement = DatabaseUtility.BuildInClause(ids, out List<SqlParameter> paras);
                paras.Add(new SqlParameter("@RestoreSettingFlag", RestoreSettingFlag.SiteMapping));
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMRestoreSiteMappings] WHERE id in {parameterizedStatement} and SettingFlag = @RestoreSettingFlag";
                context.Database.ExecuteSqlCommand(sql, paras.ToArray());
            });
        }

        public void DeleteMapping(string sourceUrl, string targetSiteUrl)
        {
            ExecuteWithRetry(context =>
            {
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMRestoreSiteMappings] WHERE SourceSiteUrl = @SourceSiteUrl and TargetSiteUrl = @TargetSiteUrl and SettingFlag = @RestoreSettingFlag";
                var sourceParameter = new SqlParameter("@SourceSiteUrl", sourceUrl);
                var targetParameter = new SqlParameter("@TargetSiteUrl", targetSiteUrl);
                var settingFlagParameter = new SqlParameter("@RestoreSettingFlag", RestoreSettingFlag.SiteMapping);
                context.Database.ExecuteSqlCommand(sql, sourceParameter, targetParameter, settingFlagParameter);
            });
        }

        public RMRestoreSiteMapping GetMappingBySourceSiteUrl(string sourceSiteUrl)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRestoreSiteMappings.FirstOrDefault(x => x.SourceSiteUrl == sourceSiteUrl && x.SettingFlag == RestoreSettingFlag.SiteMapping);
            });
        }

        public async Task<RMRestoreSiteMapping> GetMappingBySourceSiteUrlAsync(string sourceSiteUrl)
        {
            using var context = GetNewContext();
            return await context.RMRestoreSiteMappings.FirstOrDefaultAsync(x => x.SourceSiteUrl == sourceSiteUrl && x.SettingFlag == RestoreSettingFlag.SiteMapping);
        }

        public bool ExistMappingInSourcesSiteUrls(IEnumerable<string> sourceSiteUrl)
        {
            using (var context = GetNewContext())
            {
                return context.RMRestoreSiteMappings.Where(mapping => sourceSiteUrl.Contains(mapping.SourceSiteUrl) && mapping.SettingFlag == RestoreSettingFlag.SiteMapping).Count() > 0;
            }
        }

        public int GetLastMappingIntId()
        {
            return ExecuteWithRetry(context =>
            {
                RMRestoreSiteMapping map = context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SiteMapping).OrderByDescending(mapping => mapping.intId).Take(1).ToList().FirstOrDefault();
                if(map == null)
                {
                    return -1;
                }
                else
                {
                    return map.intId;
                }
            });

        }

        public void SaveMapping(List<RMRestoreSiteMapping> record)
        {
            using (var context = GetNewContext())
            {
                context.RMRestoreSiteMappings.AddRange(record);
                context.SaveChanges();
            }
        }

        public async Task CreateByBulkCopyAsync(IEnumerable<RMRestoreSiteMapping> items)
        {
            if (items.Count() == 0)
            {
                return;
            }
            using (new PerformanceScope("Batch index sub infoes"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(items))
                {
                    table.TableName = tableName;
                    await BatchAddAsync(table, tableName);
                }
            }
        }

        private string GetFullTableName()
        {
            return $"[{GetTenantSchemaName()}].[RMRestoreSiteMappings]";
        }

        private DataTable ConvertToDataTable(IEnumerable<RMRestoreSiteMapping> items)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("SourceSiteUrl", typeof(string));
            table.Columns.Add("TargetSiteUrl", typeof(string));
            table.Columns.Add("intId", typeof(int));
            table.Columns.Add("SettingFlag", typeof(int));

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["Id"] = item.Id;
                row["SourceSiteUrl"] = item.SourceSiteUrl;
                row["TargetSiteUrl"] = item.TargetSiteUrl;
                row["intId"] = item.intId;
                row["SettingFlag"] = item.SettingFlag;
                table.Rows.Add(row);
            }

            return table;
        }

        #region whitelist
        public List<RMRestoreSiteMapping> GetAllWhitelist()
        {
            try
            {
                using (var ctx = GetNewContext())
                {
                    return ctx.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SearchWhitelist).OrderByDescending(a => a.intId).ToList();
                }
            }
            catch (Exception)
            {
                int pageIndex = 0;
                int pageSize = 2000;
                int total = 0;
                List<RMRestoreSiteMapping> res = new List<RMRestoreSiteMapping>();
                while (pageIndex * pageSize < total || pageIndex == 0)
                {
                    res.AddRange(GetWhitelistByPage(pageIndex++, pageSize, out total));
                }
                return res;
            }
        }

        public List<RMRestoreSiteMapping> GetWhitelistByPage(int pageIndex, int pageSize, out int totalRecord)
        {
            IQueryable<RMRestoreSiteMapping> query = null;
            using (var context = GetNewContext())
            {
                query = context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SearchWhitelist).OrderByDescending(mappint => mappint.intId);
                return query.Paging(pageIndex, pageSize, out totalRecord).ToList();
            }
        }

        public void BatchDeleteWhitelist(params string[] ids)
        {
            BatchDeleteByFlag(ids, RestoreSettingFlag.SearchWhitelist);
        }

        public bool ExistWhitelistInSiteUrls(IEnumerable<string> siteUrl)
        {
            using (var context = GetNewContext())
            {
                return context.RMRestoreSiteMappings.Where(mapping => siteUrl.Contains(mapping.SourceSiteUrl) && mapping.SettingFlag == RestoreSettingFlag.SearchWhitelist).Count() > 0;
            }
        }

        public int GetWhiteListCount()
        {
            using (var context = GetNewContext())
            {
                return context.RMRestoreSiteMappings.Where(mapping =>  mapping.SettingFlag == RestoreSettingFlag.SearchWhitelist).Count();
            }
        }

        public int GetLastWhitelistIntId()
        {
            return ExecuteWithRetry(context =>
            {
                RMRestoreSiteMapping map = context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SearchWhitelist).OrderByDescending(mapping => mapping.intId).Take(1).ToList().FirstOrDefault();
                if (map == null)
                {
                    return -1;
                }
                else
                {
                    return map.intId;
                }
            });
        }

        public int GetWhitelistCount()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRestoreSiteMappings.Count(s => s.SettingFlag == RestoreSettingFlag.SearchWhitelist);
            });
        }

        public void DeleteWhitelist()
        {
            DeleteMappingsByFlag(RestoreSettingFlag.SearchWhitelist);
        }

        public void SaveWhitelist(RMRestoreSiteMapping siteInfo)
        {
            using (var context = GetNewContext())
            {
                context.RMRestoreSiteMappings.Add(siteInfo);
                context.SaveChanges();
            }
        }

        public void ConvertFullTextIndexListType(RestoreSettingFlag source, RestoreSettingFlag target)
        {
            ExecuteWithRetry(context =>
            {
                var sql = $"update [{GetTenantSchemaName()}].[RMRestoreSiteMappings] " +
                $" set SettingFlag = @Target" +
                $" where SettingFlag = @Source";
                var sourceFlagParameter = new SqlParameter("@Source", (int)source);
                var targetFlagParameter = new SqlParameter("@Target", (int)target);
                context.Database.ExecuteSqlCommand(sql, sourceFlagParameter, targetFlagParameter);
            });
        }

        public void DeleteMappingsByFlag(RestoreSettingFlag flag)
        {
            ExecuteWithRetry(context =>
            {
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMRestoreSiteMappings] where SettingFlag = @RestoreSettingFlag";
                var settingFlagParameter = new SqlParameter("@RestoreSettingFlag", flag);
                context.Database.ExecuteSqlCommand(sql, settingFlagParameter);
            });
        }
        #endregion

        #region blacklist
        public List<RMRestoreSiteMapping> GetAllBlacklist()
        {
            try
            {
                using (var ctx = GetNewContext())
                {
                    return ctx.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SearchBlacklist).OrderByDescending(a => a.intId).ToList();
                }
            }
            catch (Exception)
            {
                int pageIndex = 0;
                int pageSize = 2000;
                int total = 0;
                List<RMRestoreSiteMapping> res = new List<RMRestoreSiteMapping>();
                while (pageIndex * pageSize < total || pageIndex == 0)
                {
                    res.AddRange(GetBlacklistByPage(pageIndex++, pageSize, out total));
                }
                return res;
            }
        }

        public List<RMRestoreSiteMapping> GetBlacklistByPage(int pageIndex, int pageSize, out int totalRecord)
        {
            IQueryable<RMRestoreSiteMapping> query = null;
            using (var context = GetNewContext())
            {
                query = context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SearchBlacklist).OrderByDescending(mapping => mapping.intId);
                return query.Paging(pageIndex, pageSize, out totalRecord).ToList();
            }
        }

        public void BatchDeleteBlacklist(params string[] ids)
        {
            BatchDeleteByFlag(ids, RestoreSettingFlag.SearchBlacklist);
        }

        public bool ExistBlacklistInSiteUrls(IEnumerable<string> siteUrl)
        {
            using (var context = GetNewContext())
            {
                return context.RMRestoreSiteMappings.Where(mapping => siteUrl.Contains(mapping.SourceSiteUrl) && mapping.SettingFlag == RestoreSettingFlag.SearchBlacklist).Count() > 0;
            }
        }

        public int GetBlacklistCount()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRestoreSiteMappings.Count(s => s.SettingFlag == RestoreSettingFlag.SearchBlacklist);
            });
        }

        public int GetLastBlacklistIntId()
        {
            return ExecuteWithRetry(context =>
            {
                RMRestoreSiteMapping map = context.RMRestoreSiteMappings.Where(s => s.SettingFlag == RestoreSettingFlag.SearchBlacklist).OrderByDescending(mapping => mapping.intId).Take(1).ToList().FirstOrDefault();
                if (map == null)
                {
                    return -1;
                }
                else
                {
                    return map.intId;
                }
            });
        }

        public void DeleteBlacklist()
        {
            DeleteMappingsByFlag(RestoreSettingFlag.SearchBlacklist);
        }

        private void BatchDeleteByFlag(string[] ids, RestoreSettingFlag flag)
        {
            if (ids == null || ids.Length == 0)
            {
                return;
            }

#pragma warning disable 618
            ExecuteWithRetry(context =>
            {
                var parameterizedStatement = DatabaseUtility.BuildInClause(ids, out List<SqlParameter> paras);
                paras.Add(new SqlParameter("@RestoreSettingFlag", flag));
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMRestoreSiteMappings] WHERE id in {parameterizedStatement} and SettingFlag = @RestoreSettingFlag";
                context.Database.ExecuteSqlCommand(sql, paras.ToArray());
            });
#pragma warning restore 618
        }
        #endregion
    }
}
