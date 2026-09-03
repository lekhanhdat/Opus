using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Enums;
using AvePoint.RA.Contract.RestoreCenter;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SourceFlag = AvePoint.RA.Contract.Explorer.SourceFlag;

namespace AvePoint.RA.DB.Dao.Discovery.Impl
{
    public class RMDiscoverySpecificSiteDao : BaseDao<RMDiscoverySpecificSite>, IRMDiscoverySpecificSiteDao
    {
        public async Task<(IEnumerable<RMDiscoverySpecificSite>, long)> LoadM365ExcludeListSitesByPaginationAsync(int pageIndex, int pageSize)
        {
            return await LoadSpecificSitesPaginationBySourceAndType(SourceFlag.SharePoint, SpecifySiteFlag.Exclude, pageIndex, pageSize);
        }
        private async Task<(IEnumerable<RMDiscoverySpecificSite>, long)> LoadSpecificSitesPaginationBySourceAndType(SourceFlag sourceFlag,
            SpecifySiteFlag specifySiteFlag, int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            long totalCount = await context.RMDiscoverySpecificSites.AsNoTracking()
                .Where(s => s.SourceFlag == sourceFlag && s.Type == specifySiteFlag)
                .CountAsync();
            return (await context.RMDiscoverySpecificSites.AsNoTracking()
                .Where(s => s.SourceFlag == sourceFlag && s.Type == specifySiteFlag)
                .OrderBy(s => s.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(), totalCount);

        }

        public Task<IEnumerable<RMDiscoverySpecificSite>> GetAllM365ExclusionListSitesAsync()
        {
            return GetAllSpecificSitesBySourceAndType(SourceFlag.SharePoint, SpecifySiteFlag.Exclude);
        }

        private async Task<IEnumerable<RMDiscoverySpecificSite>> GetAllSpecificSitesBySourceAndType(SourceFlag sourceFlag, SpecifySiteFlag specifySiteFlag)
        {
            using var context = GetNewContext();
            return await context.RMDiscoverySpecificSites.AsNoTracking()
                .Where(s => s.SourceFlag == sourceFlag && s.Type == specifySiteFlag)
                .ToListAsync();
        }

        public int BatchRemoveM365ExclusionListSitesByIds(IEnumerable<int> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                return 0;
            }

            return ExecuteWithRetry(context =>
            {
                var paramterizedStatement = DatabaseUtility.BuildInClause(ids, out List<SqlParameter> paras);
                paras.Add(new SqlParameter("@SourceFlag", SourceFlag.SharePoint));
                paras.Add(new SqlParameter("@SpecifySiteFlag", SpecifySiteFlag.Exclude));
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMDiscoverySpecificSites] WHERE Id IN {paramterizedStatement} AND SourceFlag=@SourceFlag AND Type=@SpecifySiteFlag";
                return context.Database.ExecuteSqlCommand(sql, paras.ToArray());
            });
        }

        public int AddSpecifySites(IEnumerable<RMDiscoverySpecificSite> sites)
        {
            using var context = GetNewContext();
            context.RMDiscoverySpecificSites.AddRange(sites);
            return context.SaveChanges();
        }

        public bool IsSiteIncludeInExclusionList(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl)) return false;
            using var context = GetNewContext();
            return context.RMDiscoverySpecificSites.AsNoTracking()
                .Any(s => s.SourceFlag == SourceFlag.SharePoint && s.Type == SpecifySiteFlag.Exclude && s.Url == siteUrl);
        }

        public bool ExistM365ExcludeListInSiteUrls(IEnumerable<string> siteUrls)
        {
            if (siteUrls == null || siteUrls.Count() == 0) return false;
            using var context = GetNewContext();
            return context.RMDiscoverySpecificSites.AsNoTracking()
                .Where(s => s.SourceFlag == SourceFlag.SharePoint && s.Type == SpecifySiteFlag.Exclude && siteUrls.Contains(s.Url)).Count() > 0;
        }

        public void DeleteM365ExcludeList()
        {
            DeleteSpecificSiteBySourceAndType(SourceFlag.SharePoint, SpecifySiteFlag.Exclude);
        }

        private void DeleteSpecificSiteBySourceAndType(SourceFlag source, SpecifySiteFlag type)
        {
            ExecuteWithRetry(context =>
            {
                var sql = $"DELETE FROM [{GetTenantSchemaName()}].[RMDiscoverySpecificSites] where SourceFlag=@SourceFlag AND Type=@SpecifySiteFlag";
                List<SqlParameter> paras = new List<SqlParameter>();
                paras.Add(new SqlParameter("@SourceFlag", source));
                paras.Add(new SqlParameter("@SpecifySiteFlag", type));
                context.Database.ExecuteSqlCommand(sql, paras.ToArray());
            });
        }

        public (IEnumerable<string> runnerSite, IEnumerable<string> skipExcludeSite) GetSiteNotInM365ExcludeSite(IEnumerable<string> siteUrls)
        {
            const int batchSize = 500;
            var existingItemsInDbs = new HashSet<string>(siteUrls.Count());
            using (var context = GetNewContext())
            {
                foreach (var batch in siteUrls.Batch(batchSize))
                {
                    var urls = context.RMDiscoverySpecificSites
                        .AsNoTracking()
                        .Where(x => batch.Contains(x.Url) && x.SourceFlag == SourceFlag.SharePoint && x.Type == SpecifySiteFlag.Exclude)
                        .Select(x => x.Url)
                        .ToList();
                    existingItemsInDbs.UnionWith(urls);
                }
            }
            return (siteUrls.Where(url => !existingItemsInDbs.Contains(url)).ToList(), existingItemsInDbs.ToList());
        }

        public bool IsExistM365ExcludeSite()
        {
            return HasSpecificSitesBySourceAndType(SourceFlag.SharePoint, SpecifySiteFlag.Exclude);
        }

        private bool HasSpecificSitesBySourceAndType(SourceFlag sharePoint, SpecifySiteFlag exclude)
        {
            using(var context = GetNewContext())
            {
                return context.RMDiscoverySpecificSites.AsNoTracking().Where(s => s.SourceFlag == sharePoint && s.Type == exclude).Count() > 0;
            }
        }
    }
}
