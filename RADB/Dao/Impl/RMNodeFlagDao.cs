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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System.Data.Entity;
using System.Data.SqlClient;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMNodeFlagDao : BaseDao<RMNodeFlag>, IRMNodeFlagDao
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMNodeFlagDao));
        public void AddSiteFlagInfo(RMNodeFlag scope)
        {

            using (var ctx = GetNewContext())
            {
                if (!ctx.NodeFlag.Any(s => s.NodeId == scope.NodeId && s.GroupId == scope.GroupId && s.NodeFlagType == scope.NodeFlagType))
                {
                    ctx.NodeFlag.Add(scope);
                    ctx.SaveChanges();
                }
                else
                {
                    var entities = ctx.NodeFlag.Where(s => s.NodeId == scope.NodeId && s.GroupId == scope.GroupId && s.NodeFlagType == scope.NodeFlagType).ToList();
                    foreach (var entity in entities)
                    {
                        entity.CollectionTime = scope.CollectionTime;
                        entity.FullPath = scope.FullPath;
                        entity.Title = scope.Title;
                        entity.IsRemoved = scope.IsRemoved;
                    }
                    BatchUpdate(entities);
                }
            }
        }

        public void AddListFlagInfo(RMNodeFlag scope)
        {
            using (var ctx = GetNewContext())
            {
                if (!ctx.NodeFlag.Any(s => s.NodeId == scope.NodeId && s.GroupId == scope.GroupId && s.ListId == scope.ListId && s.FolderId == scope.FolderId && s.NodeFlagType == scope.NodeFlagType))
                {
                    ctx.NodeFlag.Add(scope);
                    ctx.SaveChanges();
                }
                else
                {
                    var entities = ctx.NodeFlag.Where(s => s.NodeId == scope.NodeId && s.GroupId == scope.GroupId && s.ListId == scope.ListId && s.FolderId == scope.FolderId && s.NodeFlagType == scope.NodeFlagType).ToList();
                    foreach (var entity in entities)
                    {
                        entity.CollectionTime = scope.CollectionTime;
                        entity.FullPath = scope.FullPath;
                        entity.Title = scope.Title;
                        entity.IsRemoved = scope.IsRemoved;
                    }
                    BatchUpdate(entities);
                }
            }
        }

        public void ClearDataByType(int type)
        {
            using (var ctx = GetNewContext())
            {
                var entities = ctx.NodeFlag.Where(s => s.NodeFlagType == type).ToList();
                BatchDelete(entities);
            }
        }

        public long GetCollectionTime(int type, Guid groupId, Guid nodeId)
        {
            using (var ctx = GetNewContext())
            {
                var info = ctx.NodeFlag.Where(s => s.NodeFlagType == type && s.GroupId == groupId && s.NodeId == nodeId).FirstOrDefault();
                if (info != null)
                {
                    return info.CollectionTime;
                }
                else
                {
                    return DateTime.MinValue.Ticks;
                }
            }
        }


        public long GetSPValidChangeTime(int type, Guid groupId, Guid nodeId, long daysBefore)
        {
            var startTime = DateTime.UtcNow.AddDays(-daysBefore).Ticks;
            var dbCollectionTime = GetCollectionTime(type, groupId, nodeId);
            if (dbCollectionTime != DateTime.MinValue.Ticks)
            {
                if (dbCollectionTime < startTime)
                {
                    logger.Warn($"invalid change time:{dbCollectionTime}, reset changetime to:{startTime}");
                    dbCollectionTime = startTime;
                }
            }
            return dbCollectionTime;
        }

        public long GetAutoJobCollectionTime(int type, Guid folderId, Guid listId, Guid nodeId, Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                //nodeId = real site id
                var info = ctx.NodeFlag.Where(s => s.NodeFlagType == (int)type && s.FolderId == folderId && s.ListId == listId && s.NodeId == nodeId && s.GroupId == groupId).FirstOrDefault();
                if (info != null)
                {
                    return info.CollectionTime;
                }
                else
                {
                    return DateTime.MinValue.Ticks;
                }
            }
        }

        public List<RMNodeFlag> GetExistScopeInfo(NodeFlagType flagType)
        {
            using (var ctx = GetNewContext())
            {
                int type = (int)flagType;
                return ctx.NodeFlag.Where(s => s.IsRemoved == false && s.NodeFlagType == type).ToList();
            }
        }

        public RMNodeFlag GetNodeFlagInfoById(Guid id, NodeFlagType flagType)
        {
            using (var ctx = GetNewContext())
            {
                int type = (int)flagType;
                return ctx.NodeFlag.Where(s => s.NodeId == id && s.NodeFlagType == type).FirstOrDefault();
            }
        }

        public async Task<IEnumerable<RMNodeFlag>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.NodeFlag.AsNoTracking().OrderBy(s => s.RowId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertNodeFlagTableAsync(IEnumerable<RMNodeFlag> nodeFlags)
        {
            using var context = GetNewContext();
            string tableName = "RMNodeFlags";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = AvePoint.GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName}([RowId],[NodeId],[Title],[GroupId],[FullPath],[CollectionTime],[NodeFlagType],[IsRemoved],[ListId],[FolderId],[StreamPosition]) VALUES ");
                int i = 0;
                foreach (var row in nodeFlags) 
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", row.RowId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", row.NodeId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", row.Title));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", row.GroupId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", row.FullPath));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", row.CollectionTime));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", row.NodeFlagType));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", row.IsRemoved));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 8}", row.ListId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 9}", row.FolderId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 10}", row.StreamPosition));
                    paramIndex += 11;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                logger.Error($"Insert Term Group data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        public async Task<long> MultiGeoDeleteAllNodeFlagAsync()
        {
            return await TruncateAllDataInTableAsync("RMNodeFlags");
        }
        public bool IsNodeFlagExist(Guid groupId, Guid Id, int type)
        {
            using (var ctx = GetNewContext())
            {
                if (Id == Guid.Empty)
                {
                    return ctx.NodeFlag.Any(s => s.IsRemoved == false && s.GroupId == groupId && s.NodeFlagType == type);
                }
                else
                {
                    return ctx.NodeFlag.Any(s => s.IsRemoved == false && s.NodeId == Id && s.GroupId == Id && s.NodeFlagType == type);
                }

            }
        }
    }
}
