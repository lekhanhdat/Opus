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


using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMManualApproveDao : BaseDao<RMManualApprove>, IRMManualApproveDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMManualApproveDao));
        public IRMWorkflowDefinitionDao WorkflowDefinitionDao { get; set; }
        public void SaveManualApprove(RMManualApprove manualApprove)
        {
            using (var ctx = GetNewContext())
            {
                var data = ctx.ManualApprove.Where(s => s.PartKey == manualApprove.PartKey && s.RowKey == manualApprove.RowKey).FirstOrDefault();
                if (data == null)
                {
                    data = ctx.ManualApprove.Where(s => s.PartKey == manualApprove.PartKey && s.NodeId == manualApprove.NodeId).FirstOrDefault();
                }
                if (null == data)
                {
                    ctx.ManualApprove.Add(manualApprove);
                    //this.ApplyCurrentValues(ctx, manualApprove);
                    ctx.SaveChanges();
                }
                else
                {
                    manualApprove.Id = data.Id;
                    this.ApplyCurrentValues(ctx, manualApprove);
                    //ctx.SaveChanges();
                }
            }
        }

        public void SaveArchivedManualApprove(RMManualApprove manualApprove)
        {
            using(var ctx = GetNewContext())
            {
                this.ApplyCurrentValues(ctx, manualApprove);
            }
        }

        public List<int> GetManualApproveOwnerIds(RMManualApprove manualApprove)
        {
            using(var context = GetNewContext())
            {
                return GetReviewUserIds(context, manualApprove).ConvertAll(item => Convert.ToInt32(item));
            }
        }

        public List<string> GetManualApproveOwnerNames(RMManualApprove manualApprove)
        {
            var ownerIds = GetManualApproveOwnerIds(manualApprove);
            using(var context = GetNewContext())
            {
                return context.Account.Where(item => item.IsRemoved == 0 && ownerIds.Contains(item.Id)).Select(item => item.DisplayName).ToList();
            }
        }

        public bool HasWorkflowData()
        {
            using (var context = GetNewContext())
            {
                return context.ManualApprove.Any(item => item.WorkflowInstanceId != Guid.Empty);
            }
        }

        public bool HasData()
        {
            using (var context = GetNewContext())
            {
                return context.ManualApprove.Any();
            }
        }

        public IEnumerable<List<RMManualApprove>> GetManualDatas(int limit = 1000)
        {
            var pageIndex = 0;
            var count = 0;

            using(var context = GetNewContext())
            {
                do
                {
                    var result = context.ManualApprove.OrderBy(item => item.Id).Skip(limit * pageIndex++).Take(limit).ToList();
                    count = result.Count;
                    yield return result;
                } while (count == limit);
            }
        }

        /// <summary>
        /// 从ArchiverTable到ManualApprove同步数据专用
        /// </summary>
        /// <param name="manualApprove"></param>
        public void SaveManualApproveItem(RMManualApprove manualApprove)
        {
            using (var ctx = GetNewContext())
            {
                var item = ctx.ManualApprove.Where(s => s.PartKey == manualApprove.PartKey && s.RowKey == manualApprove.RowKey).FirstOrDefault();
                if (item == null)
                {
                    item = ctx.ManualApprove.Where(s => s.PartKey == manualApprove.PartKey && s.NodeId == manualApprove.NodeId).FirstOrDefault();
                }
                
                if (item != null && item.Version != manualApprove.Version)
                {
                    manualApprove.Id = item.Id;
                    //this.Update(manualApprove);
                    this.ApplyCurrentValues(ctx, manualApprove);
                }
                else {
                    ctx.ManualApprove.Add(manualApprove);
                    //this.ApplyCurrentValues(ctx, manualApprove);
                    ctx.SaveChanges();
                }
            }
        }

        //public void AddManualApproveForPhysical(RMManualApprove manualApprove)
        //{
        //    using (var ctx = GetNewContext())
        //    {
        //        ctx.ManualApprove.Add(manualApprove);
        //        ctx.SaveChanges();
        //    }
        //}

        public void SaveManualApproveForPhysical(RMManualApprove manualApprove)
        {
            using (var ctx = GetNewContext())
            {
                var data = ctx.ManualApprove.Where(s => s.Id == manualApprove.Id).FirstOrDefault();
                if (null == data)
                {
                    ctx.ManualApprove.Add(manualApprove);
                    //this.ApplyCurrentValues(ctx, manualApprove);
                    ctx.SaveChanges();
                }
                else
                {
                    //manualApprove.Id = data.Id;
                    //this.Update(manualApprove);
                    this.ApplyCurrentValues(ctx, manualApprove);
                    //ctx.SaveChanges();
                }
            }
        }

        public void SaveManualApproveForFS(RMManualApprove manualApprove)
        {
            using (var ctx = GetNewContext())
            {
                var item = ctx.ManualApprove.Where(s => s.PartKey == manualApprove.PartKey && s.RowKey == manualApprove.RowKey && s.ActionStatus == (int)ActionStatus.None).FirstOrDefault();
                if (null == item)
                {
                    ctx.ManualApprove.Add(manualApprove);
                    //this.ApplyCurrentValues(ctx, manualApprove);
                    ctx.SaveChanges();
                }
                else 
                {
                    manualApprove.Id = item.Id;
                    //this.Update(manualApprove);
                    this.ApplyCurrentValues(ctx, manualApprove);
                }
            }
        }

        public async Task UpdateManualApproveActionStatusAsync(string partionKey, string rowKey, int status)
        {
            using (var ctx = GetNewContext())
            {
                var data = ctx.ManualApprove.Where(s => s.PartKey == partionKey && s.RowKey == rowKey).FirstOrDefault();
                if (null != data)
                {
                    data.Status = (int)status;
                    await this.UpdateAsync(data);
                    //ctx.SaveChanges();
                }
            }
        }
        public void UpdateManualApproveActionStatus(List<int> ids, int status, string approvedBy)
        {
            using (var ctx = GetNewContext())
            {
                var datas = ctx.ManualApprove.Where(s => ids.Contains(s.Id)).ToList();
                foreach (var data in datas)
                {
                    data.Status = (int)status;
                    data.ApprovedBy = approvedBy;
                }
                this.BatchUpdate(datas);
            }
        }

        public List<RMManualApprove> GetAllApproveOrRejectedData(SourceFlag sourceFlag)
        {
            var instanceIds = WorkflowDefinitionDao.GetCompleteInstanceIds();
            using var context = GetNewContext();
            if (instanceIds != null && instanceIds.Count > 0)
            {
                return context.ManualApprove.Where(s => s.SourceFlag == (int)sourceFlag
                                                                && (s.Status != (int)SOApproveDBStatus.WaitingApprove || (s.WorkflowInstanceId != Guid.Empty && instanceIds.Contains(s.WorkflowInstanceId)))
                                                                && s.ActionStatus == (int)ActionStatus.None).ToList();
            }
            else
            {
                return context.ManualApprove.Where(s => s.SourceFlag == (int)sourceFlag && s.Status != (int)SOApproveDBStatus.WaitingApprove && s.ActionStatus == (int)ActionStatus.None).ToList();
            }
        }

        public QueryResult GetAllDataInJob(ViewTab viewTab,out int totalRecord, int pageIndex,int pageSize, DateTime? startTime, DateTime? endTime, Expression<Func<RMManualApprove, bool>> whereLambda = null)
        {
            Expression<Func<RMManualApprove, bool>> statusLambda = null;
            var extendTime = DateTime.UtcNow.Ticks;
            if (viewTab == ViewTab.Independent)
            {
                statusLambda = s => s.ActionStatus == (int)ActionStatus.None && !s.IsRelatedRecords && extendTime > s.ExtendDispositionCustomTime;
            }
            else if (viewTab == ViewTab.Related)
            {
                statusLambda = s => s.ActionStatus == (int)ActionStatus.None && s.IsRelatedRecords && extendTime > s.ExtendDispositionCustomTime;
            }
            else
            {
                statusLambda = s => s.ActionStatus != (int)ActionStatus.None && extendTime > s.ExtendDispositionCustomTime;
            }
            try
            {
                using (var context = GetNewContext())
                {
                    Expression<Func<RMManualApprove, bool>> otherFiltersLambda = null;
                    long startTimeTicks = 0;
                    long endTimeTicks = 0;
                    if (startTime.HasValue && endTime.HasValue)
                    {
                        startTimeTicks = startTime.Value.Ticks;
                        endTimeTicks = endTime.Value.Ticks;
                        otherFiltersLambda = s => startTimeTicks <= s.CollectionTime && s.CollectionTime <= endTimeTicks;
                    }

                    List<int> query = null;
                    if (whereLambda != null)
                    {
                        if (otherFiltersLambda != null)
                        {
                            totalRecord = context.ManualApprove.AsQueryable().Where(whereLambda).Where(statusLambda).Where(otherFiltersLambda).Count();
                            query = context.ManualApprove.AsQueryable().Where(whereLambda).Where(statusLambda).Where(otherFiltersLambda).Select(S => S.Id).ToList();
                        }
                        else
                        {
                            totalRecord = context.ManualApprove.AsQueryable().Where(whereLambda).Where(statusLambda).Count();
                            query = context.ManualApprove.AsQueryable().Where(whereLambda).Where(statusLambda).Select(S => S.Id).ToList();
                        }

                    }
                    else
                    {
                        if (otherFiltersLambda != null)
                        {
                            totalRecord = context.ManualApprove.AsQueryable().Where(statusLambda).Where(otherFiltersLambda).Count();
                            query = context.ManualApprove.AsQueryable().Where(statusLambda).Where(otherFiltersLambda).Select(S =>S.Id).ToList();
                        }
                        else
                        {
                            totalRecord = context.ManualApprove.AsQueryable().Where(statusLambda).Count();
                            query = context.ManualApprove.AsQueryable().Where(statusLambda).Select(S => S.Id).ToList();
                        }
                    }
                    //context.Database.Log = new Action<string>(q => logger.Debug(q.Replace("\r\n","\\r\\n")));
                    var query2 = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    QueryResult queryResult = new QueryResult();
                    queryResult.TotalCount = totalRecord;
                    queryResult.ids = query2;
                    return queryResult;
                }
            }
            catch (Exception e)
            {
                logger.Error("Get manual approve review data error:{0}.", e.ToString());
                totalRecord = 0;
                return null;
            }
        }

        public List<RMManualApprove> GetAllData(ViewTab viewTab, int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, DateTime? startTime, DateTime? endTime, Expression<Func<RMManualApprove, bool>> whereLambda = null)
        {
            Expression<Func<RMManualApprove, bool>> statusLambda = null;
            var extendTime = DateTime.UtcNow.Ticks;
            if (viewTab == ViewTab.Independent)
            {
                statusLambda = s => s.ActionStatus == (int)ActionStatus.None && !s.IsRelatedRecords && extendTime > s.ExtendDispositionCustomTime;
            }
            else if (viewTab == ViewTab.Related)
            {
                statusLambda = s => s.ActionStatus == (int)ActionStatus.None && s.IsRelatedRecords && extendTime > s.ExtendDispositionCustomTime;
            }
            else
            {
                statusLambda = s => s.ActionStatus != (int)ActionStatus.None && extendTime > s.ExtendDispositionCustomTime;
            }

            string sortBy = "Status";
            SortDirectionEnum sortDirection = SortDirectionEnum.Ascending;
            string thenSortBy = "CollectionTime";
            SortDirectionEnum thenSortDirection = SortDirectionEnum.Descending;
            if (orderKey == null || orderKey == sortBy && isAsc)
            {
            }
            else
            {
                thenSortBy = sortBy;
                thenSortDirection = sortDirection;
                sortBy = orderKey;
                sortDirection = isAsc ? SortDirectionEnum.Ascending : SortDirectionEnum.Descending;
            }
            try
            {
                using (var context = GetNewContext())
                {

                    context.Database.CommandTimeout = 600;

                    Expression<Func<RMManualApprove, bool>> otherFiltersLambda = null;
                    long startTimeTicks = 0;
                    long endTimeTicks = 0;
                    if (startTime.HasValue && endTime.HasValue)
                    {
                        startTimeTicks = startTime.Value.Ticks;
                        endTimeTicks = endTime.Value.Ticks;
                        otherFiltersLambda = s => startTimeTicks <= s.CollectionTime && s.CollectionTime <= endTimeTicks;
                    }

                    var nameSortString = string.Empty;
                    if (new List<string>() { "ApprovedBy", "EscalateFrom", "EscalateTo" }.Contains(sortBy) || new List<string>() { "ApprovedBy", "EscalateFrom", "EscalateTo" }.Contains(thenSortBy))
                    {
                        var nameDic = context.Account.OrderBy(s => s.DisplayName).Select(k => k.Id).ToList();
                        nameSortString = string.Join("", nameDic.Select(k => k + "|"));
                        logger.Debug("nameSortString is {0}", nameSortString);
                    }

                    IQueryable<RMManualApprove> query = null;
                    if (whereLambda != null)
                    {
                        if (otherFiltersLambda != null)
                        {
                            query = context.ManualApprove.AsQueryable().Where(whereLambda).Where(statusLambda).Where(otherFiltersLambda).SortByName(sortBy, sortDirection, nameSortString).ThenSortByName(thenSortBy, thenSortDirection, nameSortString).ThenSortBy("Id", SortDirectionEnum.Ascending);
                        }
                        else
                        {
                            query = context.ManualApprove.AsQueryable().Where(whereLambda).Where(statusLambda).SortByName(sortBy, sortDirection, nameSortString).ThenSortByName(thenSortBy, thenSortDirection).ThenSortBy("Id", SortDirectionEnum.Ascending);
                        }

                    }
                    else
                    {
                        if (otherFiltersLambda != null)
                        {
                            query = context.ManualApprove.AsQueryable().Where(statusLambda).Where(otherFiltersLambda).SortByName(sortBy, sortDirection, nameSortString).ThenSortByName(thenSortBy, thenSortDirection, nameSortString).ThenSortBy("Id", SortDirectionEnum.Ascending);
                        }
                        else
                        {
                            query = context.ManualApprove.AsQueryable().Where(statusLambda).SortByName(sortBy, sortDirection, nameSortString).ThenSortByName(thenSortBy, thenSortDirection, nameSortString).ThenSortBy("Id", SortDirectionEnum.Ascending);
                        }
                    }
                    //context.Database.Log = new Action<string>(q => logger.Debug(q.Replace("\r\n", "\\r\\n")));
                    totalRecord = query.Count();
                    var results = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    return results.ToList();
                }
            }
            catch (Exception e)
            {
                logger.Error("Get manual approve review data error:{0}.", e.ToString());
                totalRecord = 0;
                return null;
            }
        }

        public List<T> GetFilterList<T>(Expression<Func<RMManualApprove, T>> selectLambda, Expression<Func<RMManualApprove, bool>> whereLambda)
        {

            if (selectLambda == null)
            {
                return new List<T>();
            }
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                if (whereLambda != null)
                {
                    return context.ManualApprove.AsQueryable().Where(whereLambda).Select(selectLambda).Distinct().ToList();
                }
                else
                {
                    return context.ManualApprove.AsQueryable().Select(selectLambda).Distinct().ToList();
                }
            }

        }
        public List<RMManualApprove> GetExportData(bool isAdmin, string accountId = "")
        {
            
            Expression<Func<RMManualApprove, bool>> statusLambda = s => s.ActionStatus != (int)ActionStatus.None;
            using (var context = GetNewContext())
            {
                if (!isAdmin)
                {
                    var user = context.Account.AsQueryable().Where(s => s.UserId == accountId).FirstOrDefault();
                    if (user == null)
                    {
                        return new List<RMManualApprove>();
                    }
                    var groupGuid = context.LnkUserGroup.AsQueryable().Where(l => l.UserId == accountId).Select(g => g.GroupId).ToList();
                    var groupIntIds = context.Account.AsQueryable().Where(g => groupGuid.Contains(g.UserId)).Select(g => g.Id);

                    var userAndGroupIds = new List<int>() { user.Id };
                    userAndGroupIds.AddRange(groupIntIds);

                    List<Expression> reviewerExpressionList = new List<Expression>();

                    ParameterExpression param = Expression.Parameter(typeof(RMManualApprove), "c");
                    var exps = userAndGroupIds.Select(i => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, "EscalateTo", i + "|"));
                    var userAndgroupExpression = exps.Aggregate(Expression.OrElse);
                    reviewerExpressionList.Add(userAndgroupExpression);

                    var wfInstanceIdStatusDic = new Dictionary<Guid, RMWorkflowStatus>();
                    List<RMWorkflowInstance> instances = null;

                    var groupIds = context.LnkUserGroup.AsQueryable().Where(l => l.UserId == accountId).Select(g => g.GroupId).ToList();
                    var userAndGroupStrIds = new List<string>() { accountId };
                    userAndGroupStrIds.AddRange(groupIds);
                    instances = WorkflowDefinitionDao.GetInstances(userAndGroupStrIds);
                    try
                    {
                        var instanceExpression = GetWorkflowInstanceExpression(instances, param);
                        if (instanceExpression != null)
                        {
                            reviewerExpressionList.Add(instanceExpression);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occured when generate workflow instance query conditions, message:{0}", e.ToString());
                    }

                    var userLambda = Expression.Lambda<Func<RMManualApprove, bool>>(reviewerExpressionList.Aggregate(Expression.OrElse), param);

                    logger.Info("Export Manual Approve Review Data user lambda:{0}", userLambda.ToString());
                    return context.ManualApprove.AsQueryable().Where(userLambda).Where(statusLambda).Distinct().ToList();
                }
                else
                {
                    return context.ManualApprove.AsQueryable().Where(statusLambda).Distinct().ToList();
                }

            }

        }

        private Expression GetWorkflowInstanceExpression(List<RMWorkflowInstance> instances, ParameterExpression param)
        {
            Expression instanceExpression = null;
            if (instances != null && instances.Count > 0)
            {
                var instanceIds = instances.Select(s => s.Id).ToList();
                var exps = instanceIds.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, "WorkflowInstanceId", c));
                instanceExpression = exps.Aggregate(Expression.OrElse);
            }
            return instanceExpression;
        }
        public List<RMManualApprove> GetWaitingApprovalData()
        {
            using (var context = GetNewContext())
            {
                return context.ManualApprove.AsQueryable().Where(m => m.ActionStatus == (int)ActionStatus.None && m.Status == 1).ToList();
            }
        }

        public List<RMManualApprove> GetAllDatas(SourceFlag flag = SourceFlag.None)
        {
            using (var context = GetNewContext())
            {
                if(flag == SourceFlag.None || flag == SourceFlag.All)
                { 
                    return context.ManualApprove.AsQueryable().ToList();
                }
                else
                {
                    int srcFlag = (int)flag;
                    return context.ManualApprove.Where(a=>a.SourceFlag == srcFlag).ToList();
                }
            }
        }

        public List<RMManualApprove> GetDatasByPager(int pageIndex, int pageSize, ref int totalCount, Expression<Func<RMManualApprove, bool>> whereLambda = null)
        {
            using (var context = GetNewContext())
            {
                if (whereLambda != null)
                {
                    if (totalCount == 0)
                    {
                        totalCount = context.ManualApprove.Count(whereLambda);
                    }
                    return context.ManualApprove.AsQueryable().Where(whereLambda).OrderBy(m => m.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    if (totalCount == 0)
                    {
                        totalCount = context.ManualApprove.AsQueryable().Count();
                    }
                    return context.ManualApprove.AsQueryable().OrderBy(m => m.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }

            }
        }
        #region Add for new manual approve calculate dashboard
        public List<long> GetAllCollectionTime(int pageIndex, int pageSize, ref int totalCount, SourceFlag flag)
        {
            int sourceFlag = (int)flag;
            using (var context = GetNewContext())
            {
                if (totalCount == 0)
                {
                    totalCount = context.ManualApprove.Count(a => a.SourceFlag == sourceFlag);
                }
                return context.ManualApprove.Where(a => a.SourceFlag == sourceFlag).OrderBy(m => m.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(s => s.CollectionTime).ToList();
            }
        }

        public int GetWaitingCount(SourceFlag flag)
        {
            int sourceFlag = (int)flag;
            using (var context = GetNewContext())
            {
                int totalCount = 0;
                if(flag == SourceFlag.None || flag == SourceFlag.All)
                {
                    totalCount = context.ManualApprove.Count(a => a.Status == 1 && a.ActionStatus == 0);
                }
                else
                {
                    totalCount = context.ManualApprove.Count(a => a.SourceFlag == sourceFlag && a.Status == 1 && a.ActionStatus == 0);
                }
                return totalCount;
            }
        }
        //public List<Guid> GetAllStepIDList()
        //{
        //    using (var context = GetNewContext())
        //    {
        //        return context.WorkflowStepConfiguration.Select(a => a.StepId).Distinct().ToList();
        //    }
        //}
        public Dictionary<string, int> GetUserAndWaitingReviewCountMapping()
        {
            string sql = @"select  s.OwnerId, count(m.Id) as TotalCount from {0}.RMWorkflowStepConfigurations as s,  {0}.RMManualApproves as m 
                        where m.ActionStatus = 0 and m.Status = 1 and m.WorkflowInstanceId != '00000000-0000-0000-0000-000000000000' 
                        and exists(select w.Id from  {0}.RMWorkflowInstances as w where w.Status = 0 and w.CurStepId = cast(s.StepId as nvarchar(36)) and w.Id = m.WorkflowInstanceId 
                        and not exists(select Id from {0}.RMWorkflowExcludeInstanceOwners as ex where ex.StepId = w.CurStepId and ex.InstanceId = w.Id)
                        )  group by s.OwnerId";
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                List<TempDic> result = context.Database.SqlQuery<TempDic>(string.Format(sql, GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName))).ToList();
                return result.ToDictionary(a=>a.OwnerId, b=>b.TotalCount);
            }
        }

        class TempDic
        {
            public string OwnerId { set; get; }
            public int TotalCount { set; get; }
        }

        public List<string> GetOwnerExceptWorkflow(int pageIndex, int pageSize, ref int totalCount)
        {
            using (var context = GetNewContext())
            {
                return context.ManualApprove.Where(a => a.Status == 1 && a.ActionStatus == 0 && a.WorkflowInstanceId == Guid.Empty).OrderBy(m => m.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(s => s.EscalateTo).ToList();
            }
        }
        #endregion
        public object GetTabInfo(Expression<Func<RMManualApprove, bool>> filterUserLambda)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.Database.CommandTimeout = 600;
                    //Expression<Func<RMManualApprove, bool>> filterUserLambda = null;
                    int wItems = 0;
                    int aItems = 0;
                    int rItems = 0;
                    var extendTime = DateTime.UtcNow.Ticks;
                    if (filterUserLambda != null)
                    {
                        wItems = context.ManualApprove.AsQueryable().Where(filterUserLambda).Count(s => s.Status == (int)SOApproveDBStatus.WaitingApprove && extendTime > s.ExtendDispositionCustomTime);
                        aItems = context.ManualApprove.AsQueryable().Where(filterUserLambda).Count(s => s.Status == (int)SOApproveDBStatus.Approved);
                        rItems = context.ManualApprove.AsQueryable().Where(filterUserLambda).Count(s => s.Status == (int)SOApproveDBStatus.Rejected);
                    }
                    else
                    {
                        wItems = context.ManualApprove.AsQueryable().Count(s => s.Status == (int)SOApproveDBStatus.WaitingApprove && extendTime > s.ExtendDispositionCustomTime);
                        aItems = context.ManualApprove.AsQueryable().Count(s => s.Status == (int)SOApproveDBStatus.Approved);
                        rItems = context.ManualApprove.AsQueryable().Count(s => s.Status == (int)SOApproveDBStatus.Rejected);
                    }
                    return new { WaitingApprove = wItems, Approved = aItems, Rejected = rItems };
                }
            }
            catch (Exception e)
            {
                logger.Error("Get Tab data error:{0}.", e.ToString());
                return null;
            }
        }

        public void UpdateManualApproveDisposalAction(List<int> ids, GCommon.Contract.StorageOptimization.Object.RelatedRecordOption relatedRecordAction)
        {
            using (var ctx = GetNewContext())
            {
                var datas = ctx.ManualApprove.Where(s => ids.Contains(s.Id)).ToList();
                foreach (var data in datas)
                {
                    data.RelatedRecordsAction = (int)relatedRecordAction;
                }
                this.BatchUpdate(datas);
            }
        }

        public ManualReviewInfo GetAuditInfos(Guid SiteId, Guid NodeId, bool isFSSource = false)
        {
            ManualReviewInfo maReviewInfo = new ManualReviewInfo();
            List<ReviewAudits> allAudits = new List<ReviewAudits>();
            using (var context = GetNewContext())
            {
                List<RMManualApprove> mainfos = new List<RMManualApprove>();
                if (isFSSource)
                {
                    //fs的数据源根据nodeid判断即可
                    mainfos = context.ManualApprove.AsQueryable().Where(m => m.NodeId.Equals(NodeId)).OrderByDescending(d => d.RowKey).ToList();
                }
                else
                {
                    mainfos = context.ManualApprove.AsQueryable().Where(m => m.SiteId.Equals(SiteId) && m.NodeId.Equals(NodeId)).OrderByDescending(d => d.RowKey).ToList();
                }
                RMManualApprove lastItem = null;
                foreach (var mainfo in mainfos)
                {
                    if (lastItem == null)
                    {
                        lastItem = mainfo;
                    }
                    string auditString = mainfo.Audits;
                    if (!string.IsNullOrEmpty(auditString))
                    {
                        List<ReviewAudits> auditInfos = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(auditString);
                        foreach (var item in auditInfos)
                        {
                            allAudits.Add(item);
                        }
                    }
                }

                if (allAudits != null && allAudits.Count > 0)
                {
                    maReviewInfo.ReviewAudits = allAudits;
                    ArgumentCheck.NotNull(lastItem, nameof(lastItem));
                    maReviewInfo.RecordOwner = GetLastReviewedUserIds(lastItem.SiteId, lastItem.NodeId);
                }
                else
                {
                    maReviewInfo.ReviewAudits = new List<ReviewAudits>();
                }
                return maReviewInfo;
            }
        }

        public string GetLastReviewedUserIds(Guid SiteId, Guid NodeId)
        {
            List<string> userIds = new List<string>();
            using (var context = GetNewContext())
            {
                var manualData = context.ManualApprove.AsQueryable().Where(m => m.SiteId.Equals(SiteId) && m.NodeId.Equals(NodeId)).OrderByDescending(d => d.RowKey).FirstOrDefault();
                //if (manualData != null) 
                //{
                //    userIds = GetReviewUserIds(context, manualData);
                //}
                //var result = string.Join("|", userIds);
                //result = AddBeforeAndAfterSeparator(result);
                string result = string.Empty;
                if (manualData != null)
                {
                    var ownerIds = GetManualApproveOwnerIds(manualData);
                    result = string.Join("|", ownerIds) + "|";
                }
                return result;
            }
        }

        private List<string> GetReviewUserIds(RMDbContext context,  RMManualApprove manualData)
        {
            var userIds = new HashSet<string>();
            if (manualData.WorkflowInstanceId != Guid.Empty)
            {
                var workflowOwners = GetWorkflowOwners(context, manualData);
                var workflowSiteOwners = GetWorkflowSiteOwners(context, manualData);
                var ownerIds = workflowOwners.Union(workflowSiteOwners);
                var uIds = context.Account.Where(u => ownerIds.Contains(u.UserId) && u.IsRemoved == 0).Select(u => u.Id.ToString());
                userIds.UnionWith(uIds);
            }
            if (!string.IsNullOrEmpty(manualData.EscalateTo))
            {
                var recordOwnerIds = manualData.EscalateTo?.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                userIds.UnionWith(recordOwnerIds);
            }
            logger.Info($"get review user id count:{userIds?.Count}");
            return userIds.ToList();
        }

        private List<string> GetWorkflowOwners(RMDbContext context, RMManualApprove manualData)
        {
            var query = from manualApprove in context.ManualApprove
                        join instance in context.WorkflowInstance
                        on manualApprove.WorkflowInstanceId equals instance.Id into leftInstances
                        from leftInstance in leftInstances.DefaultIfEmpty()
                        join stepConfig in context.WorkflowStepConfiguration
                        on leftInstance.CurStepId equals stepConfig.StepId.ToString() into leftStepConfigs
                        from leftStepConfig in leftStepConfigs
                        where manualApprove.Id == manualData.Id
                        && !string.IsNullOrEmpty(leftStepConfig.OwnerId)
                        && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id && item.OwnerId == leftStepConfig.OwnerId)
                        select leftStepConfig.OwnerId;
            return query.ToList();
        }

        private List<string> GetWorkflowSiteOwners(RMDbContext context, RMManualApprove manualData)
        {
            var query = from manualApprove in context.ManualApprove
                        join instance in context.WorkflowInstance
                        on manualApprove.WorkflowInstanceId equals instance.Id into leftInstances
                        from leftInstance in leftInstances
                        join step in context.WorkflowStep
                        on leftInstance.CurStepId equals step.Id.ToString() into leftSteps
                        from leftStep in leftSteps
                        join workflowSiteOwner in context.WorkflowSiteOwner
                        on leftInstance.DefinitionId.ToString() equals workflowSiteOwner.DefinitionId into leftWorkflowSiteOwners
                        from leftWorkflowSiteOwner in leftWorkflowSiteOwners
                        where manualApprove.Id == manualData.Id
                        && leftStep.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners
                        && manualApprove.SiteId == leftWorkflowSiteOwner.SiteId
                        && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id && item.OwnerId == leftWorkflowSiteOwner.OwnerId)
                        select leftWorkflowSiteOwner.OwnerId;
            return query.ToList();
        }

        /// <summary>
        /// 根据节点批量获取Manual信息, added by jlnan for data sync performance
        /// </summary>
        /// <returns></returns>
        public List<RMManualApprove> GetManualApproveByNodes(Guid siteId, List<Guid> nodeId)
        {
            using (var context = GetNewContext())
            {
                return context.ManualApprove.Where(a => a.SiteId == siteId && nodeId.Contains(a.NodeId)).ToList();
            }
        }
        /// <summary>
        /// 获取NodeId和Workflow OwnerId的关联关系, 用于批量查询RecordOwner, added by jlnan
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public Dictionary<Guid, List<string>> GetManualNodeAndApproverMapping(Guid siteId, List<Guid> nodeId)
        {
            Dictionary<Guid, List<string>> dic = new Dictionary<Guid, List<string>>();
            
            List<NodeIdAndUserID> result = null;
            using (var context = GetNewContext())
            {
                /* Fortify Issue Type: SQL Injection
                 */
                string nodeIdInClause = DatabaseUtility.BuildInClause(nodeId, out var paramList);
                var schemaName = AvePoint.GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                string sql = $@" select  m.NodeId, s.OwnerId from {schemaName}.RMManualApproves as m ,  {schemaName}.RMWorkflowStepConfigurations as s
              where m.ActionStatus = 0 and m.Status = 1 and m.SiteId = @siteId and m.NodeId in {nodeIdInClause} 
              and exists(select w.Id from  {schemaName}.RMWorkflowInstances as w where w.CurStepId = cast(s.StepId as nvarchar(36)) and w.Id = m.WorkflowInstanceId and 
              not exists(select Id from {schemaName}.RMWorkflowExcludeInstanceOwners as ex where ex.StepId = w.CurStepId and ex.InstanceId = w.Id and ex.OwnerId = s.OwnerId))";
                paramList.Add(new System.Data.SqlClient.SqlParameter("@siteId", siteId));

                result = context.Database.SqlQuery<NodeIdAndUserID>(sql, paramList.ToArray()).ToList();
            }

            if(result != null)
            {
                foreach(NodeIdAndUserID nu in result)
                {
                    if (!dic.ContainsKey(nu.NodeId))
                    {
                        dic.Add(nu.NodeId, new List<string>() { nu.OwnerId });
                    }
                    else
                    {
                        dic[nu.NodeId].Add(nu.OwnerId);
                    }
                }
            }

            var nodeMappingOwners = GetManualNodeAndApproverMappingBySiteOwnerReviewerType(siteId, nodeId);
            foreach(var node in nodeMappingOwners)
            {
                if(!dic.ContainsKey(node.Key))
                {
                    dic[node.Key] = new List<string>();
                }
                dic[node.Key].AddRange(node.Value);
            }

            return dic;
        }

        private Dictionary<Guid, List<string>> GetManualNodeAndApproverMappingBySiteOwnerReviewerType(Guid siteId, List<Guid> nodeId)
        {
            using(var context = GetNewContext())
            {
                var query = from manualApprove in context.ManualApprove
                            join instance in context.WorkflowInstance
                            on manualApprove.WorkflowInstanceId equals instance.Id into leftInstances
                            from leftInstance in leftInstances
                            join step in context.WorkflowStep
                            on leftInstance.CurStepId equals step.Id.ToString() into leftSteps
                            from leftStep in leftSteps
                            join workflowSiteOwner in context.WorkflowSiteOwner
                            on leftInstance.DefinitionId.ToString() equals workflowSiteOwner.DefinitionId into leftWorkflowSiteOwners
                            from leftWorkflowSiteOwner in leftWorkflowSiteOwners
                            where  leftStep.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners
                            && manualApprove.Status == (int)SOApproveDBStatus.WaitingApprove
                            && nodeId.Any(item => item == manualApprove.NodeId)
                            && manualApprove.SiteId == siteId
                            && manualApprove.WorkflowInstanceId != Guid.Empty
                            && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id && item.OwnerId == leftWorkflowSiteOwner.OwnerId)
                            group leftWorkflowSiteOwner.OwnerId by manualApprove.NodeId into groupResult
                            select groupResult;
                return query.ToDictionary(item => item.Key, item => item.ToList());
            }
        }

        class NodeIdAndUserID
        {
            public Guid NodeId { set; get; }
            public string OwnerId { set; get; }
        }
        public Dictionary<Guid, string> GetLastReviewedUserIdsByScope(Guid SiteId)
        {
            using (var context = GetNewContext())
            {
                //return context.ManualApprove.AsQueryable().Where(m => m.SiteId.Equals(SiteId) && m.NodeId.Equals(NodeId)).OrderByDescending(d => d.RowKey).Select(m => m.EscalateTo).FirstOrDefault();
                var result = context.ManualApprove.AsQueryable().Where(m => m.SiteId.Equals(SiteId)).OrderByDescending(d => d.RowKey).ToList().Distinct(new RMManualApproveComparer()).ToDictionary(
                    m => m.NodeId,
                    m =>
                    {
                        var ownerIds = GetManualApproveOwnerIds(m);
                        return string.Join("|", ownerIds) + "|";
                    });
                return result;
            }
        }

        public List<Guid> GetAllInstanceIds()
        {
            using (var context = GetNewContext())
            {
                return context.ManualApprove.Where(o => o.WorkflowInstanceId != Guid.Empty).Select(o => o.WorkflowInstanceId).ToList();
            }
        }
    }

    public class RMManualApproveComparer : IEqualityComparer<RMManualApprove>
    {
        public bool Equals(RMManualApprove x, RMManualApprove y)
        {
            var xItemId = x?.NodeId.ToString();
            var yItemId = y?.NodeId.ToString();
            return string.Equals(xItemId, yItemId, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(RMManualApprove obj)
        {
            return obj?.NodeId.ToString()?.GetHashCode() ?? 0;
        }
    }

    public static class SortByNameQueryExtensions
    {
        public static IOrderedQueryable<T> SortByName<T>(this IQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string nameSortString = null)
        {
            string OrderBy = "OrderBy";
            string OrderByDescending = "OrderByDescending";
            if (new List<string>() { "ApprovedBy", "EscalateFrom", "EscalateTo" }.Contains(sortPropertyName))
            {
                return BaseSortByName(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending, nameSortString);
            }
            return QueryExtensions.BaseSort(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending);
        }

        public static IOrderedQueryable<T> ThenSortByName<T>(this IOrderedQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string nameSortString = null)
        {
            string OrderBy = "ThenBy";
            string OrderByDescending = "ThenByDescending";
            if (new List<string>() { "ApprovedBy", "EscalateFrom", "EscalateTo" }.Contains(sortPropertyName))
            {
                return BaseSortByName(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending, nameSortString);
            }
            return QueryExtensions.BaseSort(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending);
        }
        private static IOrderedQueryable<T> BaseSortByName<T>(IQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string OrderBy, string OrderByDescending, string nameSortString = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (String.IsNullOrEmpty(sortPropertyName) || sortPropertyName.Trim().Length == 0)
            {
                return (IOrderedQueryable<T>)source;
            }
            Expression<Func<RMManualApprove, int?>> lambdaExp1 = s => SqlFunctions.CharIndex(string.IsNullOrEmpty(s.EscalateTo) ? "" : s.EscalateTo.Substring(0, SqlFunctions.CharIndex("|", s.EscalateTo).Value), nameSortString);
            Expression<Func<RMManualApprove, int?>> lambdaExp2 = s => SqlFunctions.CharIndex(string.IsNullOrEmpty(s.ApprovedBy) ? "" : s.ApprovedBy, nameSortString);
            Expression<Func<RMManualApprove, int?>> lambdaExp3 = s => SqlFunctions.CharIndex(string.IsNullOrEmpty(s.EscalateFrom) ? "" : s.EscalateFrom, nameSortString);
            Expression<Func<RMManualApprove, int?>> lambdaExp = null;
            if (sortPropertyName == "EscalateTo")
            {
                lambdaExp = lambdaExp1;
            }
            else if (sortPropertyName == "ApprovedBy")
            {
                lambdaExp = lambdaExp2;
            }
            else if (sortPropertyName == "EscalateFrom")
            {
                lambdaExp = lambdaExp3;
            }
            string methodName = (sortDirection == SortDirectionEnum.Ascending) ? OrderBy : OrderByDescending;

            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                                new Type[] { source.ElementType, typeof(int?) },
                                                source.Expression, Expression.Quote(lambdaExp));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(methodCallExpression);
        }




    }
}
