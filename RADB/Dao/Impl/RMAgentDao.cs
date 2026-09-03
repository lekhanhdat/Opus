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
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMAgentDao : BaseDao<RMAgent>, IRMAgentDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMAgentDao));
        public List<RMAgent> QueryAgents(AgentQueryParams queryDto, out int totalCount)
        {
            using (var ctx = GetNewContext())
            {
                var pageSize = queryDto.PageSize;
                var pageIndex = queryDto.PageIndex;
                var searchValue = queryDto.SearchValue;
                var sortBy = "Name";
                var sortDirection = SortDirectionEnum.Ascending;
                var isSearchInConnGroup = queryDto.AddAgentList != null && queryDto.AddAgentList.Count > 0;
                var agentIds = queryDto.AddAgentList;

                Expression<Func<RMAgent, bool>> notDeletedLambda = o => o.Status != ServiceStatus.Deleted;
                Expression<Func<RMAgent, bool>> searchLambda = null;

                if (!string.IsNullOrEmpty(queryDto.SortBy))
                {
                    sortBy = queryDto.SortBy;
                    sortDirection = queryDto.IsAscending ? SortDirectionEnum.Ascending : SortDirectionEnum.Descending;
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchLambda = o => o.Name.ToLower().Contains(searchValue.ToLower());
                }

                IQueryable<RMAgent> query;
                if (searchLambda != null)
                {
                    query = isSearchInConnGroup 
                            ? ctx.RMAgent.Where(notDeletedLambda).Where(searchLambda).Where(o => !agentIds.Contains(o.Id)).SortBy(sortBy, sortDirection) 
                            : ctx.RMAgent.Where(notDeletedLambda).Where(searchLambda).SortBy(sortBy, sortDirection);
                }
                else
                {
                    query = isSearchInConnGroup
                        ? ctx.RMAgent.Where(notDeletedLambda).Where(o => !agentIds.Contains(o.Id)).SortBy(sortBy, sortDirection)
                        : ctx.RMAgent.Where(notDeletedLambda).SortBy(sortBy, sortDirection);
                }
                var isMultiGeoOtherDC = !string.IsNullOrEmpty(queryDto.MainDCName) && !string.IsNullOrEmpty(queryDto.DataCenterName) && !queryDto.MainDCName.Equals(queryDto.DataCenterName);

                if (isMultiGeoOtherDC)
                {
                    query = query.Where(a => a.DCInternalName == queryDto.DataCenterName);
                }
                totalCount = query.Count();
                return query.Skip((queryDto.PageIndex - 1) * queryDto.PageSize).Take(queryDto.PageSize).ToList();
            }
        }

        public List<RMAgent> QueryAgentsByDC(AgentQueryParams queryDto, out int totalCount)
        {
            using (var ctx = GetNewContext())
            {
                var pageSize = queryDto.PageSize;
                var pageIndex = queryDto.PageIndex;
                var searchValue = queryDto.SearchValue;
                var sortBy = "Name";
                var sortDirection = SortDirectionEnum.Ascending;
                var isSearchInConnGroup = queryDto.AddAgentList != null && queryDto.AddAgentList.Count > 0;
                var agentIds = queryDto.AddAgentList;

                Expression<Func<RMAgent, bool>> notDeletedLambda = o => o.Status != ServiceStatus.Deleted;
                Expression<Func<RMAgent, bool>> searchLambda = null;

                if (!string.IsNullOrEmpty(queryDto.SortBy))
                {
                    sortBy = queryDto.SortBy;
                    sortDirection = queryDto.IsAscending ? SortDirectionEnum.Ascending : SortDirectionEnum.Descending;
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchLambda = o => o.Name.ToLower().Contains(searchValue.ToLower());
                }

                IQueryable<RMAgent> query = ctx.RMAgent.Where(notDeletedLambda);
                if (searchLambda != null)
                {
                    query = query.Where(searchLambda);
                }

                if (string.IsNullOrEmpty(queryDto.DataCenterName))
                {
                    query = string.IsNullOrEmpty(queryDto.MainDCName)
                        ? query.Where(a => string.IsNullOrEmpty(a.DCInternalName))
                        : query.Where(a => string.IsNullOrEmpty(a.DCInternalName) || a.DCInternalName == queryDto.MainDCName);
                }
                else
                {
                    query = query.Where(a => a.DCInternalName == queryDto.DataCenterName);
                }

                if (isSearchInConnGroup)
                {
                    query = query.Where(a => !agentIds.Contains(a.Id));
                }

                query = query.Distinct().SortBy(sortBy, sortDirection);

                totalCount = query.Count();
                return query.Skip((queryDto.PageIndex - 1) * queryDto.PageSize).Take(queryDto.PageSize).ToList();
            }
        }

        public async Task<IEnumerable<RMAgent>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMAgent.AsNoTracking().OrderBy(o => o.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> CreateReplicaAgentAsync(RMAgent agent)
        {
            using var context = GetNewContext();
            string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            string sql = $@"
INSERT INTO {schemaName}.RMAgents
    (Id, Name, SourceType, ClientId, InstallationCode, ServerName, AuthCode, Version, Errors, Status, CertificateId, Description, JobCounts, TimeStamp, CPUHZ, AvailableCPU, TotalMemory, AvailableMemeory, OSName, OSVersionNumber, FarmId, IsSupportUpgrade, CollectLog, DCInternalName)
VALUES
    (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21, @p22, @p23)";

            return await context.Database.ExecuteSqlCommandAsync(
                sql,
                new SqlParameter("@p0", agent.Id),
                new SqlParameter("@p1", agent.Name),
                new SqlParameter("@p2", (int)agent.SourceType),
                new SqlParameter("@p3", agent.ClientId),
                new SqlParameter("@p4", agent.InstallationCode),
                new SqlParameter("@p5", (object)agent.ServerName ?? DBNull.Value),
                new SqlParameter("@p6", agent.AuthCode),
                new SqlParameter("@p7", agent.Version),
                new SqlParameter("@p8", (int)agent.Errors),
                new SqlParameter("@p9", (int)agent.Status),
                new SqlParameter("@p10", agent.CertificateId),
                new SqlParameter("@p11", (object)agent.Description ?? DBNull.Value),
                new SqlParameter("@p12", agent.JobCounts),
                new SqlParameter("@p13", agent.TimeStamp),
                new SqlParameter("@p14", agent.CPUHZ),
                new SqlParameter("@p15", agent.AvailableCPU),
                new SqlParameter("@p16", agent.TotalMemory),
                new SqlParameter("@p17", agent.AvailableMemeory),
                new SqlParameter("@p18", (object)agent.OSName ?? DBNull.Value),
                new SqlParameter("@p19", agent.OSVersionNumber),
                new SqlParameter("@p20", (object)agent.FarmId ?? DBNull.Value),
                new SqlParameter("@p21", agent.IsSupportUpgrade),
                new SqlParameter("@p22", agent.CollectLog),
                new SqlParameter("@p23", (object)agent.DCInternalName ?? DBNull.Value));
        }

        public async Task<long> MultiGeoInsertAgentTableAsync(IEnumerable<RMAgent> agents)
        {
            using var context = GetNewContext();
            string tableName = "RMAgents";
            try
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, Name, SourceType, ClientId, InstallationCode, ServerName, AuthCode, Version, Errors, Status, CertificateId, Description, JobCounts, TimeStamp, CPUHZ, AvailableCPU, TotalMemory, AvailableMemeory, OSName, OSVersionNumber, FarmId, IsSupportUpgrade, CollectLog, DCInternalName) VALUES ");
                int i = 0;
                foreach (var item in agents)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10}, @p{paramIndex + 11}, @p{paramIndex + 12}, @p{paramIndex + 13}, @p{paramIndex + 14}, @p{paramIndex + 15}, @p{paramIndex + 16}, @p{paramIndex + 17}, @p{paramIndex + 18}, @p{paramIndex + 19}, @p{paramIndex + 20}, @p{paramIndex + 21}, @p{paramIndex + 22}, @p{paramIndex + 23})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.Name));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", (int)item.SourceType));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", item.ClientId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", item.InstallationCode));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", (object)item.ServerName ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", item.AuthCode));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", item.Version));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 8}", (int)item.Errors));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 9}", (int)item.Status));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 10}", item.CertificateId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 11}", (object)item.Description ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 12}", item.JobCounts));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 13}", item.TimeStamp));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 14}", item.CPUHZ));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 15}", item.AvailableCPU));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 16}", item.TotalMemory));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 17}", item.AvailableMemeory));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 18}", (object)item.OSName ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 19}", item.OSVersionNumber));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 20}", (object)item.FarmId ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 21}", item.IsSupportUpgrade));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 22}", item.CollectLog));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 23}", (object)item.DCInternalName ?? DBNull.Value));
                    paramIndex += 24;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMAgents data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllAgentAsync()
        {
            return await TruncateAllDataInTableAsync("RMAgents");
        }
    }
}
