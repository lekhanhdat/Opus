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
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermGroupMembershipDao : BaseDao<RMTermGroupMembership>, ITermGroupMembershipDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(TermGroupMembershipDao));
        public void DeleteTermGroupInfo(Guid termGroupId, Guid termStoreId)
        {
            using var context = GetNewContext();
            RMTermGroupMembership relativedSite = context.TermGroupMembership.AsQueryable().Where(t => termGroupId.Equals(t.TermGroupId) && t.TermStoreId.Equals(termStoreId)).FirstOrDefault();
            if (relativedSite != null)
            {
                context.TermGroupMembership.Remove(relativedSite);
                context.SaveChanges();
            }

        }

        public List<RMTermGroupMembership> GetTermGroupInfoById(Guid termGroupId)
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.AsQueryable().Where(t => termGroupId.Equals(t.TermGroupId)).ToList();
        }

        public List<RMTermGroupMembership> GetTermGroupsByAgentGroupId(string id)
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.AsQueryable().Where(t => t.AgentGroupId.Equals(id, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<RMTermGroupMembership> GetOtherGroupsByAgentGroupIdAndTermGroupId(string id, Guid termGroupId)
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.AsQueryable().Where(t => !t.AgentGroupId.Equals(id) && termGroupId.Equals(t.TermGroupId)).ToList();
        }

        public List<RMTermGroupMembership> GetAllTermGroupMembership()
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.AsQueryable().ToList();
        }

        public bool ExistTermGroupInfo(Guid termGroupId, Guid termStoreId)
        {
            return this.Exist(t => t.TermGroupId == termGroupId && t.TermStoreId.Equals(termStoreId));
        }

        public bool ExistTermGroupInfo(Guid termGroupId, string googleTenant)
        {
            return this.Exist(t => t.TermGroupId == termGroupId && t.SiteUrl.Equals(googleTenant));
        }

        public RMTermGroupMembership GetTermGroupInfo(Guid termGroupId, Guid termStoreId)
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.AsQueryable().Where(t => termGroupId.Equals(t.TermGroupId) && t.TermStoreId.Equals(termStoreId)).FirstOrDefault();
        }


        public void AddTermGroupInfo(Guid termGroupId, string url, string displayName, string termStoreName, Guid termStoreId, string agentGroupId, SiteType siteType)
        {
            using (var context = GetNewContext())
            {
                if (context.TermGroupMembership.AsQueryable().Where(t => t.TermGroupId.Equals(termGroupId) && t.TermStoreId.Equals(termStoreId)).FirstOrDefault() == null)
                {
                    context.TermGroupMembership.Add(new RMTermGroupMembership() { TermGroupId = termGroupId, SiteUrl = url, DisplayName = displayName, TermStoreId = termStoreId, TermStoreName = termStoreName, AgentGroupId = agentGroupId, SiteType = siteType });
                    context.SaveChanges();
                }
            }
        }

        public async Task AddGoogleTenantTermGroup(Guid termGroupId, string url, string displayName, string termStoreName, Guid termStoreId,
            string agentGroupId, SiteType siteType)
        {
            using var context = GetNewContext();
            var existedGoogleTenants = await context.TermGroupMembership.Where(t =>
                !t.TermGroupId.Equals(termGroupId) && t.SiteUrl.Equals(url) && t.SiteType == SiteType.Google).ToListAsync();
            if (existedGoogleTenants.Count != 0)
            {
                context.TermGroupMembership.RemoveRange(existedGoogleTenants);
            }
            context.TermGroupMembership.Add(new RMTermGroupMembership() { TermGroupId = termGroupId, SiteUrl = url, DisplayName = displayName, TermStoreId = termStoreId, TermStoreName = termStoreName, AgentGroupId = agentGroupId, SiteType = siteType });
            await context.SaveChangesAsync();
        }

        public async Task AddGoogleTenantInTermGroupMembership(RMTermGroupMembership termGrMembership)
        {
            using var context = GetNewContext();

            var relatedRecords = await context.TermGroupMembership
                .Where(t => t.SiteUrl == termGrMembership.SiteUrl && t.SiteType == SiteType.Google)
                .ToListAsync();

            var recordsToDelete = relatedRecords
                .Where(t => t.TermGroupId != termGrMembership.TermGroupId)
                .ToList();

            if (recordsToDelete.Any())
            {
                context.TermGroupMembership.RemoveRange(recordsToDelete);
            }

            bool isExisted = relatedRecords
                .Any(t => t.TermGroupId == termGrMembership.TermGroupId && t.SiteUrl == termGrMembership.SiteUrl && t.SiteType == SiteType.Google);

            if (!isExisted)
            {
                context.TermGroupMembership.Add(termGrMembership);
                await context.SaveChangesAsync();
            }
        }


        public List<Guid> GetTermStoreIdsByTermGroupId(Guid termGroupId, SiteType siteType)
        {
            List<Guid> termStoreIds = new List<Guid>();
            using var context = GetNewContext();
            var terGroupMemberships = context.TermGroupMembership.AsNoTracking().Where(t => t.SiteType == siteType && termGroupId.ToString().Equals(t.TermGroupId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var t in terGroupMemberships)
            {
                if (termStoreIds == null || termStoreIds.Count == 0)
                {
                    termStoreIds.Add(t.TermStoreId);
                }
                else
                {
                    if (!termStoreIds.Contains(t.TermStoreId))
                    {
                        termStoreIds.Add(t.TermStoreId);
                    }
                }
            }
            return termStoreIds;
        }

        public List<RMTermGroupMembership> GetTermGroupMembershipByTermGroupId(Guid termGroupId, SiteType siteType)
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.AsNoTracking().Where(t => t.SiteType == siteType && termGroupId.ToString().Equals(t.TermGroupId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

       

        public async Task UpdateTermGroupInfoAsync(int id, Guid termGroupId, string url, string displayName, string termStoreName, Guid termStoreId, string agentGroupId, SiteType siteType)
        {
            using var context = GetNewContext();
            var result = context.TermGroupMembership.AsQueryable().Where(ts => ts.Id.Equals(id)).FirstOrDefault();
            result.TermGroupId = termGroupId;
            result.SiteUrl = url;
            result.DisplayName = displayName;
            result.TermStoreId = termStoreId;
            result.TermStoreName = termStoreName;
            result.AgentGroupId = agentGroupId;
            result.SiteType = siteType;
            await this.UpdateAsync(result);
        }

        public Dictionary<Guid, List<Guid>> GetTermStoreAndGroupIdMapping()
        {
            using (var context = GetNewContext())
            {
                return context.TermGroupMembership.GroupBy(o => o.TermGroupId).ToDictionary(k => k.Key, value => value.Select(v => v.TermStoreId).Distinct().ToList());
            }
        }
        public List<string> GetAllSpecifiedSites(SiteType siteType)
        {
            using var context = GetNewContext();
            return context.TermGroupMembership.Where(o => o.SiteType == siteType).Select(o => o.SiteUrl).ToList();
        }

        public async Task<Dictionary<string,string>> GetGoogleTenantsExisted(List<string> googleTenants, Guid termGroupId)
        {
            using var context = GetNewContext();
            var existedTenant = await context.TermGroupMembership
                .Where(termGr => googleTenants.Any(item => item == termGr.SiteUrl &&  termGroupId != termGr.TermGroupId))
                .Join(context.TermGruops.Where(termGroup => !termGroup.IsRemoved), termGrMembership => termGrMembership.TermGroupId, termGroup => termGroup.UniqueId, (termGrMembership, termGroup) => new
                {
                    DisplayName = termGrMembership.DisplayName,
                    TermGroupName = termGroup.Name
                })
                .ToDictionaryAsync(item => item.DisplayName, item => item.TermGroupName);
            return existedTenant;
        }

        public async Task DeleteGoogleTenantsByTermGroupId(Guid termGroupId)
        {
            using var context = GetNewContext();
            var result = await context.TermGroupMembership
                .Where(item => item.TermGroupId == termGroupId && item.SiteType == SiteType.Google).ToListAsync();
            context.TermGroupMembership.RemoveRange(result);
            await context.SaveChangesAsync();
        }

        public async Task DeleteGoogleTenantsByTermGroupIdAndSiteUrl(List<string> googleTenants, Guid termGroupId)
        {
            using var context = GetNewContext();
            var result = await context.TermGroupMembership
                .Where(item => item.TermGroupId == termGroupId && googleTenants.Contains(item.SiteUrl)).ToListAsync();
            context.TermGroupMembership.RemoveRange(result);
            await context.SaveChangesAsync();
        }

        public async Task<List<RMTermGroupMembership>> GetGoogleTermGroupMemberships()
        {
            using var context = GetNewContext();
            return await context.TermGroupMembership.Where(item => item.SiteType == SiteType.Google)
                .Join(context.TermGruops.Where(termGroup => !termGroup.IsRemoved), termGroupMembership => termGroupMembership.TermGroupId, termGroup => termGroup.UniqueId, (termGroupMembership, termGroup) => termGroupMembership).ToListAsync();
        }

        public async Task<List<string>> GetTermGroupsBySiteUrlGroupIds(List<string> siteUrls)
        {
            using var context = GetNewContext();
            return await context.TermGroupMembership
                .Where(x => siteUrls.Contains(x.SiteUrl))
                .Select(x => x.TermGroupId.ToString())
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<RMTermGroupMembership>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.TermGroupMembership.AsNoTracking().OrderBy(t => t.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertTermGroupMembershipTableAsync(IEnumerable<RMTermGroupMembership> termGroupMemberships)
        {
            using var context = GetNewContext();
            string tableName = "RMTermGroupMemberships";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, TermGroupId, SiteUrl, DisplayName, TermStoreId, TermStoreName, AgentGroupId, SiteType) VALUES ");
                int i = 0;
                foreach (var item in termGroupMemberships)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.TermGroupId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.SiteUrl));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.DisplayName));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", item.TermStoreId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", item.TermStoreName));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 6}", item.AgentGroupId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 7}", (int)item.SiteType));
                    paramIndex += 8;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMTermGroupMemberships data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        public async Task<long> MultiGeoDeleteAllTermGroupMembershipAsync()
        {
            return await TruncateAllDataInTableAsync("RMTermGroupMemberships");
        }
    }
}
