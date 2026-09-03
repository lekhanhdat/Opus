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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class PhysicalRequestDao : BaseDao<RMPhysicalRequest>, IPhysicalRequestDao
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(PhysicalRequestDao));
        public IGeneralSettingService GeneralSettingService { get; set; }
        public List<RMPhysicalRequest> GetRequest(int id)
        {
            using (var context = GetNewContext())
            {
                var result = context.RMPhysicalRequest.FirstOrDefault(pr => pr.Id == id);
                if (result == null)
                {
                    return null;
                }
                if (result.GroupRequestId == Guid.Empty)
                {
                    return new List<RMPhysicalRequest>() { result };
                }

                return context.RMPhysicalRequest.Where(pr => pr.GroupRequestId == result.GroupRequestId).ToList();
            }
        }

        public List<RMPhysicalRequest> GetRequestByIds(List<int> ids)
        {
            using (var context = GetNewContext())
            {
                var results = new List<RMPhysicalRequest>();
                List<Guid> groupRequestIds = new List<Guid>();
                var requestDBs = context.RMPhysicalRequest.Where(pr => ids.Contains(pr.Id)).ToList();
                if (requestDBs == null || requestDBs.Count == 0)
                {
                    return null;
                }
                foreach (var request in requestDBs)
                {
                    if (request.GroupRequestId == Guid.Empty)
                        results.Add(request);
                    else
                        groupRequestIds.Add(request.GroupRequestId);
                }

                var groupRequest = context.RMPhysicalRequest.Where(pr => groupRequestIds.Contains(pr.GroupRequestId)).ToList();
                if (groupRequest.Count > 0)
                {
                    results.AddRange(groupRequest);
                }
                return results;
            }
        }

        public RMPhysicalRequest GetRequestByPhysicalRecordId(string id)
        {
            using (var context = GetNewContext())
            {
                return context.RMPhysicalRequest.FirstOrDefault(pr => pr.PhysicalFileId.Equals(id, StringComparison.OrdinalIgnoreCase) && pr.Status == (int)PhysicalRequestStatus.WaitingForApproval);
            }
        }

        /// <summary>
        /// Query Method, order by create time 
        /// </summary>
        /// <param name="pageIndex">从1开始的页码</param>
        /// <param name="pageSize">每页显示量</param>
        /// <param name="totalRecord">总数</param>  
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public async Task<(List<PhysicalQueryRequestDto>,int)> QueryAuthorizedAsync(PhysicalRequestParam param, int pageIndex, int pageSize, bool isPhysicalAdmin, Expression<Func<RMPhysicalRequest, bool>> whereLambda = null)
        {
            int totalRecord;
            var globalTimeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            if (param.StartTime.HasValue)
            {
                param.StartTime = TimeZoneInfo.ConvertTimeToUtc(param.StartTime.Value, localZone);
            }
            if (param.EndTime.HasValue)
            {
                param.EndTime = TimeZoneInfo.ConvertTimeToUtc(param.EndTime.Value, localZone);
            }
            Expression<Func<RMPhysicalRequest, bool>> otherFiltersLambda = null;
            long startTimeTicks = 0;
            long endTimeTicks = 0;
            if (param.StartTime.HasValue && param.EndTime.HasValue)
            {
                startTimeTicks = param.StartTime.Value.Ticks;
                endTimeTicks = param.EndTime.Value.Ticks;
                otherFiltersLambda = s => startTimeTicks <= s.CreatedTime && s.CreatedTime <= endTimeTicks;
            }
            string userId = Contract.Tenant.TenantLocalValue.LogonUserId;
            logger.Info("Authorized query, user {0}, is app admin {1}", Contract.Tenant.TenantLocalValue.LogonUserId, isPhysicalAdmin);
            using (var context = GetNewContext())
            {
                IOrderedQueryable<PhysicalQueryRequestDto> query = null;
                Expression<Func<RMPhysicalRequest, PhysicalQueryRequestDto>> selector = o => new PhysicalQueryRequestDto {
                    Id = o.Id,
                    Title = o.Title,
                    Status = o.Status,
                    Type = o.Type,
                    CreatedUserId = o.CreatedUserId,
                    PhysicalFileId = o.PhysicalFileId,
                    CreatedTime = o.CreatedTime,
                    ModifiedTime = o.ModifiedTime,
                    ManagerUserId = o.ManagerUserId,
                    HoldUserId = o.HoldUserId,
                    HoldCategory = o.HoldCategory,
                    HoldNumber = o.HoldNumber,
                    HoldUnit = o.HoldUnit,
                    EndTime = o.EndTime,
                    EndTimeStr = o.EndTimeStr,
                    IsDaylightSavingTime = o.IsDaylightSavingTime,
                    TimeZoneId = o.TimeZoneId,
                    MetaData = o.MetaData,
                    HoldByDisplayName = o.HoldByDisplayName,
                    GroupRequestId = o.GroupRequestId,
                    MoveInfo = o.MoveInfo
                };
                if (whereLambda != null)
                {
                    query = isPhysicalAdmin ? context.RMPhysicalRequest.AsQueryable().Where(whereLambda).Where(otherFiltersLambda).Select(selector).OrderByDescending(a => a.CreatedTime)
                        : context.RMPhysicalRequest.AsQueryable().Where(whereLambda).Where(otherFiltersLambda).Where(a => a.CreatedUserId == userId).Select(selector).OrderByDescending(a => a.CreatedTime);

                    var groupRequestIds = query.Where(r => r.GroupRequestId != Guid.Empty).Select(r => r.GroupRequestId).ToList();
                    query = query.Union(context.RMPhysicalRequest.AsQueryable().Where(r => groupRequestIds.Contains(r.GroupRequestId)).Select(selector)).OrderByDescending(a => a.CreatedTime);
                }
                else
                {
                    query = isPhysicalAdmin ? context.RMPhysicalRequest.AsQueryable().Where(otherFiltersLambda).Select(selector).OrderByDescending(a => a.CreatedTime)
                        : context.RMPhysicalRequest.AsQueryable().Where(otherFiltersLambda).Where(a => a.CreatedUserId == userId).Select(selector).OrderByDescending(a => a.CreatedTime);
                }
                var listRequest = new List<PhysicalQueryRequestDto>();

                totalRecord = await query.CountAsync();
                //var results = query.Skip((pageIndex - 1) * pageSize).Take(pageSize); 

                return (query.ToList(),totalRecord);
            }
        }

        private Expression<Func<RMPhysicalRequest, bool>> BuildBottomLocationFilter(List<Guid> bottomLocationIds)
        {
            var parameter = Expression.Parameter(typeof(RMPhysicalRequest), "x");

            var noLocationIdCondition = Expression.Not(
                Expression4DynamicQuery.GetContainsExpression(typeof(RMPhysicalRequest), parameter, "MetaData", "<LocationId>")
            );

            var locationExpressions = bottomLocationIds.Select(id =>
            {
                var xmlTag = $"<LocationId>{id}</LocationId>";
                return Expression4DynamicQuery.GetContainsExpression(typeof(RMPhysicalRequest), parameter, "MetaData", xmlTag);
            }).ToList();

            Expression combinedLocationExpression = null;
            if (locationExpressions.Any())
            {
                combinedLocationExpression = locationExpressions.Aggregate(Expression.OrElse);
            }

            Expression finalCondition;
            if (combinedLocationExpression != null)
            {
                finalCondition = Expression.OrElse(noLocationIdCondition, combinedLocationExpression);
            }
            else
            {
                finalCondition = noLocationIdCondition;
            }

            var lambda = Expression.Lambda<Func<RMPhysicalRequest, bool>>(finalCondition, parameter);
            return lambda;
        }

        public async Task<(List<PhysicalQueryRequestDto>, int)> QueryPhyRequestByBottomLocationIdsAsync(PhysicalRequestParam param, int pageIndex, int pageSize, bool isPhysicalAdmin, List<Guid> bottomLocationIds, Expression<Func<RMPhysicalRequest, bool>> whereLambda = null)
        {
            int totalRecord;
            var globalTimeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            if (param.StartTime.HasValue)
            {
                param.StartTime = TimeZoneInfo.ConvertTimeToUtc(param.StartTime.Value, localZone);
            }
            if (param.EndTime.HasValue)
            {
                param.EndTime = TimeZoneInfo.ConvertTimeToUtc(param.EndTime.Value, localZone);
            }
            Expression<Func<RMPhysicalRequest, bool>> otherFiltersLambda = null;
            long startTimeTicks = 0;
            long endTimeTicks = 0;
            if (param.StartTime.HasValue && param.EndTime.HasValue)
            {
                startTimeTicks = param.StartTime.Value.Ticks;
                endTimeTicks = param.EndTime.Value.Ticks;
                otherFiltersLambda = s => startTimeTicks <= s.CreatedTime && s.CreatedTime <= endTimeTicks;
            }

            var bottomLocationFilter = BuildBottomLocationFilter(bottomLocationIds);

            string userId = Contract.Tenant.TenantLocalValue.LogonUserId;
            logger.Info("Authorized query, user {0}, is app admin {1}", Contract.Tenant.TenantLocalValue.LogonUserId, isPhysicalAdmin);
            using (var context = GetNewContext())
            {
                IOrderedQueryable<PhysicalQueryRequestDto> query = null;
                Expression<Func<RMPhysicalRequest, PhysicalQueryRequestDto>> selector = o => new PhysicalQueryRequestDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Status = o.Status,
                    Type = o.Type,
                    CreatedUserId = o.CreatedUserId,
                    PhysicalFileId = o.PhysicalFileId,
                    CreatedTime = o.CreatedTime,
                    ModifiedTime = o.ModifiedTime,
                    ManagerUserId = o.ManagerUserId,
                    HoldUserId = o.HoldUserId,
                    HoldCategory = o.HoldCategory,
                    HoldNumber = o.HoldNumber,
                    HoldUnit = o.HoldUnit,
                    EndTime = o.EndTime,
                    EndTimeStr = o.EndTimeStr,
                    IsDaylightSavingTime = o.IsDaylightSavingTime,
                    TimeZoneId = o.TimeZoneId,
                    MetaData = o.MetaData,
                    HoldByDisplayName = o.HoldByDisplayName,
                    GroupRequestId = o.GroupRequestId
                };
                if (whereLambda != null)
                {
                    if (bottomLocationFilter != null)
                    {
                        query = isPhysicalAdmin ? context.RMPhysicalRequest.AsQueryable().Where(whereLambda).Where(otherFiltersLambda).Where(bottomLocationFilter).Select(selector).OrderByDescending(a => a.CreatedTime)
                            : context.RMPhysicalRequest.AsQueryable().Where(whereLambda).Where(otherFiltersLambda).Where(bottomLocationFilter).Where(a => a.CreatedUserId == userId).Select(selector).OrderByDescending(a => a.CreatedTime);
                    }
                    else
                    {
                        query = isPhysicalAdmin ? context.RMPhysicalRequest.AsQueryable().Where(whereLambda).Where(otherFiltersLambda).Select(selector).OrderByDescending(a => a.CreatedTime)
                            : context.RMPhysicalRequest.AsQueryable().Where(whereLambda).Where(otherFiltersLambda).Where(a => a.CreatedUserId == userId).Select(selector).OrderByDescending(a => a.CreatedTime);
                    }

                    var groupRequestIds = query.Where(r => r.GroupRequestId != Guid.Empty).Select(r => r.GroupRequestId).ToList();
                    query = query.Union(context.RMPhysicalRequest.AsQueryable().Where(r => groupRequestIds.Contains(r.GroupRequestId)).Select(selector)).OrderByDescending(a => a.CreatedTime);
                }
                else
                {
                    if (bottomLocationFilter != null)
                    {
                        query = isPhysicalAdmin ? context.RMPhysicalRequest.AsQueryable().Where(otherFiltersLambda).Where(bottomLocationFilter).Select(selector).OrderByDescending(a => a.CreatedTime)
                            : context.RMPhysicalRequest.AsQueryable().Where(otherFiltersLambda).Where(bottomLocationFilter).Where(a => a.CreatedUserId == userId).Select(selector).OrderByDescending(a => a.CreatedTime);
                    }
                    else
                    {
                        query = isPhysicalAdmin ? context.RMPhysicalRequest.AsQueryable().Where(otherFiltersLambda).Select(selector).OrderByDescending(a => a.CreatedTime)
                            : context.RMPhysicalRequest.AsQueryable().Where(otherFiltersLambda).Where(a => a.CreatedUserId == userId).Select(selector).OrderByDescending(a => a.CreatedTime);
                    }
                }
                var listRequest = new List<PhysicalQueryRequestDto>();

                try
                {
                    totalRecord = await query.CountAsync();
                }
                catch(Exception e)
                {
                    logger.Error($"{e}");
                    totalRecord = 0;
                }

                return (query.ToList(), totalRecord);
            }
        }
        public List<RequestBy> GetRequestBy()
        {
            using (var ctx = GetNewContext())
            {
                var sql = $"select distinct r.CreatedUserId as UserId,a.DisplayName,a.UserPrincipalName from {SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName)}.RMPhysicalRequests as r join {SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName)}.RMAccounts as a on r.CreatedUserId = a.UserId where a.IsRemoved != 1";
                return ctx.Database.SqlQuery<RequestBy>(sql).ToList();
            }
        }

        public List<RMPhysicalRequest> GetMoveRequestsWaitingByPhysicalFileIds(List<string> physicalFileIds)
        {
            using (var context = GetNewContext())
            {
                return context.RMPhysicalRequest.Where(pr => physicalFileIds.Contains(pr.PhysicalFileId) && pr.Status == (int)PhysicalRequestStatus.WaitingForApproval && pr.Type == (int)PhysicalRequestType.Move).ToList();
            }
        }

        public List<RMPhysicalRequest> GetRequestsByGroupRequestId(Guid groupRequestId, PhysicalRequestStatus status)
        {
            using (var context = GetNewContext())
            {
                return context.RMPhysicalRequest.Where(pr => pr.GroupRequestId == groupRequestId && pr.Status == (int)status).ToList();
            }
        }
    }
}
