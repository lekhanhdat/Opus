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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.MyHub.Model.FIlter.Types;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FSConnectionDao : BaseDao<FSConnection>, IFSConnectionDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(FSConnectionDao));
        public List<FSConnection> GetAllConnections(bool onlyNoGroup = false)
        {
            using (var ctx = GetNewContext())
            {
                if (onlyNoGroup)
                {
                    return ctx.FSConnection.AsNoTracking().Where(c => c.GroupId == Guid.Empty).OrderByDescending(c => c.LastModifiedTime).ToList();
                }
                else
                {
                    var groupDic = ctx.FSConnectionGroup.ToDictionary(k => k.Id, v => v.Name);
                    var conns = ctx.FSConnection.AsNoTracking().OrderByDescending(c => c.LastModifiedTime).ToList();
                    foreach (var conn in conns)
                    {
                        conn.GroupName = groupDic.GetValue(conn.GroupId);
                    }
                    return conns;
                }
            }
        }

        public List<FSConnection> GetAllNoGroupConnections(GetConnectionListParam param, out int totalCount)
        {
            using (var ctx = GetNewContext())
            {
                var pageIndex = Math.Max(param.PageIndex, 1);
                var pageSize = Math.Max(param.PageSize, 1);

                var connectionIds = param?.ConnectionIds ?? new List<Guid>();
                var query = ctx.FSConnection.Where(c => c.GroupId == Guid.Empty && !connectionIds.Contains(c.Id));

                query = query.SortBy(nameof(FSConnection.LastModifiedTime), SortDirectionEnum.Descending);

                totalCount = query.Count();

                return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public List<FSConnection> QueryConnectionsPager(Expression<Func<FSConnection, bool>> whereLambda, GetConnectionListParam param, out int totalCount)
        {
            var defaultResult = new List<FSConnection>();
            using (var ctx = GetNewContext())
            {
                var sortDirection = param.Order.IsDesc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
                var isNeedIncludeGroup = param.Filters != null && param.Filters.Any(f => f.ColumnName == nameof(FSConnection.GroupName));

                var baseQuery = ctx.FSConnection.AsQueryable();

                if (whereLambda != null)
                    baseQuery = baseQuery.Where(whereLambda);

                if (isNeedIncludeGroup)
                {
                    var groupNames = param.Filters.Where(f => f.ColumnName == nameof(FSConnection.GroupName)).SelectMany(f => f.ColumnValues).ToList();
                    baseQuery = baseQuery.Where(conn => ctx.FSConnectionGroup.Any(gr => gr.Id == conn.GroupId && groupNames.Contains(gr.Name)));
                }

                totalCount = baseQuery.Count();

                if (totalCount == 0) return defaultResult;

                //var finalQuery = baseQuery.SortBy(param.Order.ColumnName, sortDirection);

                if (param.PageIndex < 0 || param.PageSize < 0)
                {
                    return defaultResult;
                }
                var isSortByGroupName = param.Order.ColumnName == nameof(FSConnection.GroupName);
                IQueryable<FSConnection> sortedQuery;

                if (isSortByGroupName)
                {
                    var joined = baseQuery
                        .GroupJoin(ctx.FSConnectionGroup,
                            conn => conn.GroupId,
                            grp => grp.Id,
                            (conn, grps) => new { conn, grps })
                        .SelectMany(
                            x => x.grps.DefaultIfEmpty(),
                            (x, grp) => new { Connection = x.conn, GroupName = grp != null ? grp.Name : null });

                    sortedQuery = sortDirection == SortDirectionEnum.Descending
                        ? joined.OrderByDescending(x => x.GroupName).Select(x => x.Connection)
                        : joined.OrderBy(x => x.GroupName).Select(x => x.Connection);
                }
                else
                {
                    sortedQuery = baseQuery.SortBy(param.Order.ColumnName, sortDirection);
                }
                var data = sortedQuery.Skip((param.PageIndex - 1) * param.PageSize).Take(param.PageSize).ToList();

                var connIds = data.Select(c => c.Id).ToList();

                var failureJobCounts = ctx.FSConnectionRelatedJobInfoes
                    .Where(r => r.EndTime > 0
                        && connIds.Contains(r.ConnectionId)
                        && (r.Status == (int)JobStatus.Failed || r.Status == (int)JobStatus.FinishWithException)
                        && ctx.JobMonitors.Any(j => j.Id == r.JobId))
                    .GroupBy(r => r.ConnectionId)
                    .Select(g => new { ConnectionId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.ConnectionId, x => x.Count);

                var groupIds = data.Where(c => c.GroupId != Guid.Empty).Select(c => c.GroupId).Distinct().ToList();
                var groupDic = ctx.FSConnectionGroup.Where(g => groupIds.Contains(g.Id)).ToDictionary(k => k.Id, v => v.Name);

                foreach (var conn in data)
                {
                    conn.GroupName = groupDic.TryGetValue(conn.GroupId, out var groupName) ? groupName : null;
                    conn.FailureJobCount = failureJobCounts.TryGetValue(conn.Id, out var count) ? count : 0;
                }

                return data;
            }
        }

        public List<FSConnection> QueryConnectionsPagerForOtherDCs(Expression<Func<FSConnection, bool>> whereLambda, GetConnectionListParam param, out int totalCount, string DCInternalName)
        {
            var defaultResult = new List<FSConnection>();
            using (var ctx = GetNewContext())
            {
                if (DCInternalName == null)
                {
                    totalCount = 0;
                    return new();
                }
                var fSConnectionGroupIdsBelongCurrentDC = ctx.FSConnectionGroup.AsNoTracking().Where(g => DCInternalName.Equals(g.DCInternalName)).Select(g => g.Id);
                var sortDirection = param.Order.IsDesc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
                var isNeedIncludeGroup = param.Filters != null && param.Filters.Any(f => f.ColumnName == nameof(FSConnection.GroupName));

                var baseQuery = ctx.FSConnection.AsQueryable().Where(conn => fSConnectionGroupIdsBelongCurrentDC.Contains(conn.GroupId));

                if (whereLambda != null)
                    baseQuery = baseQuery.Where(whereLambda);

                if (isNeedIncludeGroup)
                {
                    var groupNames = param.Filters.Where(f => f.ColumnName == nameof(FSConnection.GroupName)).SelectMany(f => f.ColumnValues).ToList();
                    baseQuery = baseQuery.Where(conn => ctx.FSConnectionGroup.Any(gr => gr.Id == conn.GroupId && groupNames.Contains(gr.Name)));
                }

                totalCount = baseQuery.Count();

                if (totalCount == 0) return defaultResult;

                var finalQuery = baseQuery.SortBy(param.Order.ColumnName, sortDirection);

                if (param.PageIndex < 0 || param.PageSize < 0)
                {
                    return defaultResult;
                }

                var data = finalQuery.Skip((param.PageIndex - 1) * param.PageSize).Take(param.PageSize).ToList();

                var connIds = data.Select(c => c.Id).ToList();

                var failureJobCounts = ctx.FSConnectionRelatedJobInfoes.AsNoTracking()
                    .Where(r => r.EndTime > 0
                        && connIds.Contains(r.ConnectionId)
                        && (r.Status == (int)JobStatus.Failed || r.Status == (int)JobStatus.FinishWithException)
                        && ctx.JobMonitors.Any(j => j.Id == r.JobId))
                    .GroupBy(r => r.ConnectionId)
                    .Select(g => new { ConnectionId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.ConnectionId, x => x.Count);

                var groupIds = data.Where(c => c.GroupId != Guid.Empty).Select(c => c.GroupId).Distinct().ToList();
                var groupDic = ctx.FSConnectionGroup.Where(g => groupIds.Contains(g.Id))
                    .Select(g => new { g.Name, g.Id }).ToDictionary(k => k.Id, v => v.Name);

                foreach (var conn in data)
                {
                    conn.GroupName = groupDic.TryGetValue(conn.GroupId, out var groupName) ? groupName : null;
                    conn.FailureJobCount = failureJobCounts.TryGetValue(conn.Id, out var count) ? count : 0;
                }

                return data;
            }
        }

        public List<FSConnection> GetAllConnectionsByGroupId(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnection.Where(c => c.GroupId == groupId).ToList();
            }
        }

        public async Task<List<FSConnection>> GetAllConnectionsByGroupIdAsync(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.FSConnection.Where(c => c.GroupId == groupId).ToListAsync();
            }

        }

        public async Task<List<FSConnection>> GetConnectionBySearchKey(string searchKey)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.FSConnection
                    .Where(g => g.Name.Contains(searchKey))
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<List<FSConnection>> GetConnectionBySearchKeyAndGroupId(string searchKey, IEnumerable<Guid> groupIds)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.FSConnection
                    .Where(g => g.Name.Contains(searchKey) && groupIds.Contains(g.GroupId))
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Guid>> GetAllConnectionIdsByGroupIdsAsync(IEnumerable<Guid> groupIds)
        {
            using (var ctx = GetNewContext())
            {
                if (groupIds == null) return Array.Empty<Guid>();
                return await ctx.FSConnection
                    .AsNoTracking()
                    .Where(conn => groupIds.Contains(conn.GroupId))
                    .Select(conn => conn.Id)
                    .ToListAsync();
            }
        }

        public FSConnection GetConnectionById(Guid connectionId)
        {
            using (var ctx = GetNewContext())
            {
                var conn = ctx.FSConnection.FirstOrDefault(g => g.Id == connectionId);

                if (conn == null)
                {
                    return null;
                }

                var failureJobCount = ctx.FSConnectionRelatedJobInfoes
                    .Count(r => r.ConnectionId == connectionId
                             && r.EndTime > 0
                             && (r.Status == (int)JobStatus.Failed || r.Status == (int)JobStatus.FinishWithException)
                             && ctx.JobMonitors.Any(j => j.Id == r.JobId));

                conn.FailureJobCount = failureJobCount;

                return conn;
            }
        }

        public List<FSConnection> GetConnectionByIds(List<Guid> connectionIds)
        {
            if (connectionIds == null || !connectionIds.Any())
            {
                return new List<FSConnection>();
            }

            using (var ctx = GetNewContext())
            {
                var connections = ctx.FSConnection.Where(g => connectionIds.Contains(g.Id)).ToList();

                if (!connections.Any())
                {
                    return connections;
                }

                var validConnIds = connections.Select(c => c.Id).ToList();

                var failureJobCounts = ctx.FSConnectionRelatedJobInfoes
                    .Where(r => r.EndTime > 0
                             && validConnIds.Contains(r.ConnectionId)
                             && (r.Status == (int)JobStatus.Failed || r.Status == (int)JobStatus.FinishWithException)
                             && ctx.JobMonitors.Any(j => j.Id == r.JobId))
                    .GroupBy(r => r.ConnectionId)
                    .Select(g => new { ConnectionId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.ConnectionId, x => x.Count);

                foreach (var conn in connections)
                {
                    conn.FailureJobCount = failureJobCounts.TryGetValue(conn.Id, out var count) ? count : 0;
                }

                return connections;
            }
        }
        public FSConnection GetConnectionByName(string name)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnection.FirstOrDefault(g => g.Name == name);
            }
        }

        public FSConnection GetConnectionByUNCPath(string uncPath)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnection.FirstOrDefault(g => g.UNCPath == uncPath);
            }
        }

        public async Task<bool> SaveConnectoinAsync(FSConnection connection)
        {
            using (var ctx = GetNewContext())
            {
                if (ctx.FSConnection.Any(g => g.Id != connection.Id && g.Name == connection.Name))
                {
                    throw new Exception(I18NEntity.GetString("RM_FS_Register_SameConnectionNameErrorMessage"));
                }
                var exist = ctx.FSConnection.Where(g => g.Id == connection.Id).FirstOrDefault();
                if (exist == null)
                {
                    ctx.FSConnection.Add(connection);
                    return ctx.SaveChanges() > 0;
                }
                else
                {
                    if (exist.JPMCConnectionId != connection.JPMCConnectionId)
                    {
                        connection.JPMCConnectionId = exist.JPMCConnectionId;
                    }
                    if (exist.LastSyncTime != 0)
                    {
                        connection.LastSyncTime = exist.LastSyncTime;
                    }
                    return await this.UpdateAsync(connection);
                }
            }
        }

        public bool CheckConnectoinUNCPathExist(Guid connectionId, string uncPath)
        {
            uncPath = uncPath.Trim().TrimEnd('\\') + "\\";
            var allConn = new List<FSConnection>();
            using (var ctx = GetNewContext())
            {
                allConn = ctx.FSConnection.Where(c => connectionId != c.Id).ToList();
            }
            return allConn.Any(c =>
            {
                var dbPath = c.UNCPath.TrimEnd('\\') + "\\";
                return uncPath == dbPath || uncPath.Contains(dbPath) || dbPath.Contains(uncPath);
            });
        }

        public bool CheckConnectionIdExist(string JPMCConnectionId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnection.Any(c => c.JPMCConnectionId == JPMCConnectionId);
            }
        }

        public bool CheckAllConnectionIdsExist(List<Guid> connectionIds)
        {
            if (connectionIds == null || connectionIds.Count == 0)
            {
                return true;
            }

            var uniqueIds = connectionIds.Distinct().ToList();
            using var ctx = GetNewContext();
            var matchedCount = ctx.FSConnection
                .AsNoTracking()
                .Count(c => uniqueIds.Contains(c.Id));

            return matchedCount == uniqueIds.Count;
        }

        public bool CheckUpdateConnectionIdExist(string JPMCConnectionId, Guid Id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnection.Any(c => c.JPMCConnectionId == JPMCConnectionId && c.Id != Id);
            }
        }

        public async Task<bool> UpdateConnectoinGroupIdAsync(Guid connectionId, Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var exist = ctx.FSConnection.Where(g => g.Id == connectionId).FirstOrDefault();
                if (exist == null)
                {
                    return false;
                }
                else
                {
                    exist.GroupId = groupId;
                    return await this.UpdateAsync(exist);
                }
            }
        }

        public void UpdateConnectionsGroupId(Guid groupId, List<Guid> connectionIds)
        {
            using (var ctx = GetNewContext())
            {
                var relateGroupConnections = ctx.FSConnection.Where(item => item.GroupId == groupId).ToList();
                relateGroupConnections.ForEach(item => item.GroupId = Guid.Empty);
                this.BatchUpdate(ctx, relateGroupConnections);

                var connections = ctx.FSConnection.Where(item => connectionIds.Contains(item.Id)).ToList();
                connections.ForEach(item => item.GroupId = groupId);
                this.BatchUpdate(ctx, connections);
            }
        }

        public void DeleteConnectoin(Guid connectionId)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    base.DeleteByKey(connectionId);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public FSConnection GetParentConnectionInfo(string uncPath)
        {
            using (var ctx = GetNewContext())
            {
                //If too many location ,replace to "select top1 orderby len()"
                return ctx.FSConnection.Where(c => uncPath.StartsWith(c.UNCPath)).OrderByDescending(c => c.UNCPath.Length).FirstOrDefault();
            }
        }

        public FSConnection GetParentConnectionInfoForImport(string uncPath)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnection.Where(c => (uncPath.StartsWith((c.UNCPath + "\\"))) || uncPath.Equals(c.UNCPath)).OrderByDescending(c => c.UNCPath.Length).FirstOrDefault();
            }
        }

        public async Task<bool> UpdateValidateResultAsync(List<Guid> connectionIds, Dictionary<Guid, string> uncPaths, Dictionary<Guid, int> pathTypes)
        {
            if (connectionIds == null || connectionIds.Count == 0)
            {
                return true;
            }

            using (var ctx = GetNewContext())
            {
                var connections = ctx.FSConnection.Where(c => connectionIds.Contains(c.Id)).ToList();
                foreach (var connection in connections)
                {
                    string finalPath = string.Empty;
                    int pathType;

                    var hasFinalPath = uncPaths != null
                        && uncPaths.TryGetValue(connection.Id, out finalPath)
                        && !string.IsNullOrWhiteSpace(finalPath);

                    if (hasFinalPath)
                    {
                        connection.UNCPath = finalPath;
                    }

                    connection.PathType = pathTypes != null && pathTypes.TryGetValue(connection.Id, out pathType)
                        ? pathType
                        : 0;
                }

                await ctx.SaveChangesAsync();
                return true;
            }
        }
        public async Task<IEnumerable<FSConnection>> LoadByPager(int pageIndex, int pageSize)
        {
            using var ctx = GetNewContext();
            return await ctx.FSConnection.AsNoTracking().OrderBy(c => c.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertFSConnectionTableAsync(IEnumerable<FSConnection> fSConnections)
        {
            using var context = GetNewContext();
            try
            {
                context.FSConnection.AddRange(fSConnections);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert FSConnections data has error: {ex}");
                return 0;
            }
        }
        public async Task<long> MultiGeoDeleteAllFSConnectionAsync()
        {
            return await TruncateAllDataInTableAsync("FSConnections");
        }

        public async Task<Guid> GetConnectionGroupIdByNameAsync(string groupName)
        {
            using (var ctx = GetNewContext())
            {
                var group = await ctx.FSConnectionGroup.FirstOrDefaultAsync(g => g.Name == groupName);
                return group != null ? group.Id : Guid.Empty;
            }
        }

        public async Task<Guid> GetConnectionGroupIdByConnectionIdAsync(Guid connectionId)
        {
            using (var ctx = GetNewContext())
            {
                var connection = await ctx.FSConnection.FirstOrDefaultAsync(c => c.Id == connectionId);
                return connection != null ? connection.GroupId : Guid.Empty;
            }
        }

        public bool AnyConnectionExistsOutsideGroup(List<Guid> connectionIds, Guid groupId, bool isCreate)
        {
            using var ctx = GetNewContext();
            if (isCreate)
            {
                return ctx.FSConnection.AsNoTracking().Count(c => connectionIds.Contains(c.Id) && c.GroupId != Guid.Empty) > 0;
            }
            else
            {
                return ctx.FSConnection.AsNoTracking().Count(c => connectionIds.Contains(c.Id) && c.GroupId != groupId && c.GroupId != Guid.Empty) > 0;
            }
        }

        #region JPMC
        public async Task<bool> UpdateLastSyncTimeAsync(Guid connectionId, long lastSyncTime)
        {
            using (var ctx = GetNewContext())
            {
                var exist = ctx.FSConnection.Where(g => g.Id == connectionId).FirstOrDefault();
                if (exist != null)
                {
                    exist.LastSyncTime = lastSyncTime;
                    return await UpdateAsync(exist);
                }
                return false;
            }
        }
        #endregion

        #region MyHub
        public async Task<(List<FSConnection> Items, bool HasMore, int Count)> QueryConnectionPaginationAsync(List<int> userIntIds, RMMyhubDriveQueryInfo queryInfo)
        {
            using (var ctx = GetNewContext())
            {

                var query = ctx.FSConnection
                    .Where(c => c.GroupId != Guid.Empty)
                    .Where(c => ctx.RMFSConnectionAndOwnerRelationship
                        .Any(r => userIntIds.Contains(r.UserIntId) && r.ConnectionId == c.Id))
                    .AsQueryable();

                if (queryInfo.FilterInfoes != null && queryInfo.FilterInfoes.Any())
                {
                    foreach (var filter in queryInfo.FilterInfoes)
                    {
                        if (filter.ColumnKey?.ToLower() == "lastsynctime" && !string.IsNullOrWhiteSpace(filter.ColumnValue))
                        {
                            var value = JsonConvert.DeserializeObject<RMMyhubDriveLastSyncTimeFilterValue>(filter.ColumnValue);

                            switch (value.Option)
                            {
                                case RMMyhubDriveLastSyncTimeFilterOption.AnyTime:
                                    break;

                                case RMMyhubDriveLastSyncTimeFilterOption.WithIn:
                                    var nowUtc = DateTime.UtcNow;
                                    var nowLocal = nowUtc + queryInfo.TimeOffSet; 
                                    var todayLocal = nowLocal.Date; 
                                    DateTime adjustedLocalTime;
                                    switch (value.WithinOption)
                                    {
                                        case RMMyhubDriveLastSyncTimeWithinFilter.Days:
                                            adjustedLocalTime = todayLocal.AddDays(-value.WithinNumber);
                                            break;
                                        case RMMyhubDriveLastSyncTimeWithinFilter.Weeks:
                                            adjustedLocalTime = todayLocal.AddDays(-value.WithinNumber * 7);
                                            break;
                                        case RMMyhubDriveLastSyncTimeWithinFilter.Months:
                                            adjustedLocalTime = todayLocal.AddMonths(-value.WithinNumber);
                                            break;
                                        case RMMyhubDriveLastSyncTimeWithinFilter.Years:
                                            adjustedLocalTime = todayLocal.AddYears(-value.WithinNumber);
                                            break;
                                        default:
                                            throw new NotSupportedException($"Unsupported within option: {value.WithinOption}");
                                    }

                                    var adjustedUtcTime = adjustedLocalTime - queryInfo.TimeOffSet;
                                    var adjustedTimeTicks = adjustedUtcTime.Ticks;

                                    query = query.Where(x => x.LastSyncTime >= adjustedTimeTicks);
                                    break;

                                case RMMyhubDriveLastSyncTimeFilterOption.Between:
                                    var offset = queryInfo.TimeOffSet;
                                    var start = (value.StartTime.Date - offset).Ticks;
                                    var end = (value.EndTime.Date.AddDays(1) - offset).Ticks;
                                    query = query.Where(x => x.LastSyncTime >= start && x.LastSyncTime < end);
                                    break;

                                default:
                                    throw new NotSupportedException($"Unsupported filter option: {value.Option}");
                            }
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(queryInfo.SearchValue))
                {
                    var normalizedSearchValue = queryInfo.SearchValue.Trim();
                    query = query.Where(item =>
                        item.Name.Contains(normalizedSearchValue) ||
                        item.JPMCConnectionId.Contains(normalizedSearchValue) ||
                        item.UNCPath.Contains(normalizedSearchValue));
                }

                query = query.OrderBy(x => x.Name);

                var count = await query.CountAsync();

                var skipCount = (queryInfo.PageIndex - 1) * queryInfo.PageSize;
                var items = await query.Skip(skipCount).Take(queryInfo.PageSize + 1).ToListAsync();
                var hasMore = items.Count > queryInfo.PageSize;
                var resultItems = items.Take(queryInfo.PageSize).ToList();

                return (resultItems, hasMore, count);
            }
        }

        public async Task<bool> UpdateConnectoinIsPauseAsync(List<Guid> connectionIds, int isPause)
        {
            using (var ctx = GetNewContext())
            {
                var exist = ctx.FSConnection.Where(g => connectionIds.Contains(g.Id)).ToList();
                if (exist == null || exist.Count < 1)
                {
                    return false;
                }
                else
                {
                    foreach (var item in exist)
                    {
                        item.IsPause = isPause;
                        item.LastModifiedTime = DateTime.Now.Ticks;
                    }
                    this.BatchUpdate(ctx, exist);
                    return exist.Count() > 0;
                }
            }
        }






        #endregion
    }
}
