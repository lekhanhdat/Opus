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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FSConnectionGroupWithAgentMemebershipDao : BaseDao<FSConnectionGroupWithAgentMembership>, IFSConnectionGroupWithAgentMemebershipDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(FSConnectionGroupWithAgentMemebershipDao));
        public void AddMemberships(Guid groupId, List<Guid> agentIds)
        {
            var needAddMemberships = agentIds.ConvertAll(item => new FSConnectionGroupWithAgentMembership
            {
                ConnectionGroupId = groupId,
                AgentId = item
            });
            BatchCreate(needAddMemberships);
        }

        public Task RemoveAllAsync(Guid groupId)
        {
            return BatchDeleteAsync(item => item.ConnectionGroupId == groupId);
        }

        public Task RemoveAllAsync(List<Guid> groupIds)
        {
            return BatchDeleteAsync(item => groupIds.Contains(item.ConnectionGroupId));
        }

        public bool CheckAgentIsUnderGroup(Guid agentId)
        {
            return Count(item => item.AgentId == agentId) > 0;
        }

        public Task RemoveAllByAgentIdsAsync(List<Guid> agentIds)
        {
            return BatchDeleteAsync(item => agentIds.Contains(item.AgentId));
        }

        public List<Guid> GetAgentIdByGroupId(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnectionGroupWithAgentMembership.Where(g => g.ConnectionGroupId == groupId).Select(f => f.AgentId).ToList();
            }
        }

        public async Task<IEnumerable<FSConnectionGroupWithAgentMembership>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.FSConnectionGroupWithAgentMembership.AsNoTracking().OrderBy(f => f.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertFSConnectionGroupWithAgentMembershipTableAsync(IEnumerable<FSConnectionGroupWithAgentMembership> fSConnectionGroupWithAgentMemberships)
        {
            using var context = GetNewContext();
            string tableName = "FSConnectionGroupWithAgentMemberships";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, ConnectionGroupId, AgentId) VALUES ");
                int i = 0;
                foreach (var item in fSConnectionGroupWithAgentMemberships)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.ConnectionGroupId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.AgentId));
                    paramIndex += 3;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert FSConnectionGroupWithAgentMemberships data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllFSConnectionGroupWithAgentMembershipAsync()
        {
            return await TruncateAllDataInTableAsync("FSConnectionGroupWithAgentMemberships");
        }

        public bool IsAgentInAnyGroupExcept(Guid agentId, Guid groupId)
        {
            if(groupId == Guid.Empty)
            {
                return CheckAgentIsUnderGroup(agentId);
            }
            return Count(item => item.AgentId == agentId && item.ConnectionGroupId != groupId) > 0;
        }
    }
}
