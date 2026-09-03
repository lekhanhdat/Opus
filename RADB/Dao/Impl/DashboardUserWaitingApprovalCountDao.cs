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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class DashboardUserWaitingApprovalCountDao : BaseDao<RMDashboardUserWaitingApprovalCount>, IDashboardUserWaitingApprovalCountDao
    {

        private static ManualApprovalRecordRepository Repository => new ManualApprovalRecordRepository();

        public Task RemoveAllAsync(SourceFlag sourceFlag)
        {
            return RemoveAllAsync((int)sourceFlag);
        }

        public Task RemoveAllAsync(int sourceFlag)
        {
            return BatchDeleteAsync(item => item.SourceFlag == sourceFlag);
        } 

        public IEnumerable<List<int[]>> GetWaitingApprovalOwners(SourceFlag sourceFlag, int limit = 5000)
        {
            var repository = Repository;

            var currentTicks = DateTime.UtcNow.Ticks;
            string continuation = null;
            do
            {
                var (Result, Continuation) = repository.QueryItemsWithPaginationAsync(
                    item => item.IsManualSynced && 
                    item.SourceFlag == (int)sourceFlag && 
                    item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove &&
                    item.ManualExtendTime < currentTicks,
                    item => item.ManualReviewer,
                    continuation,
                    limit
                ).GetAwaiter().GetResult();

                continuation = Continuation;
                yield return Result.ToList();
            } while (!string.IsNullOrEmpty(continuation));

            #region 
            //var pageIndex = 0;
            //var count = 0;
            //using (var context = GetNewContext())
            //{
            //    var query = from manualApprove in context.ManualApprove
            //                where manualApprove.SourceFlag == (int)sourceFlag
            //                && manualApprove.Status == (int)SOApproveDBStatus.WaitingApprove
            //                && manualApprove.ActionStatus == (int)ActionStatus.None
            //                && manualApprove.EscalateTo != null
            //                && manualApprove.ExtendDispositionCustomTime < currentTicks
            //                select manualApprove.EscalateTo;
            //    do
            //    {

            //        var datas = query.OrderBy(item => item).Skip(limit * pageIndex++).Take(limit).ToList();
            //        count = datas.Count;
            //        yield return datas;
            //    } while (count == limit);
            //}
            #endregion
        }

        public IEnumerable<Dictionary<string, long>> GetWaitingApprovalWorkflowOnwers(SourceFlag sourceFlag, int limit = 5000)
        {
            var pageIndex = 0;
            var count = 0;
            var currentTicks = DateTime.UtcNow.Ticks;

            using(var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                var query = from manualApprove in context.ManualApprove
                            join instance in context.WorkflowInstance
                            on manualApprove.WorkflowInstanceId equals instance.Id into leftInstances
                            from leftInstance in leftInstances.DefaultIfEmpty()
                            join stepConfig in context.WorkflowStepConfiguration
                            on leftInstance.CurStepId equals stepConfig.StepId.ToString() into leftStepConfigs
                            from leftStepConfig in leftStepConfigs.DefaultIfEmpty()
                            where manualApprove.SourceFlag == (int)sourceFlag
                            && manualApprove.Status == (int)SOApproveDBStatus.WaitingApprove
                            && manualApprove.ExtendDispositionCustomTime < currentTicks
                            && manualApprove.WorkflowInstanceId != Guid.Empty
                            && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id && item.OwnerId == leftStepConfig.OwnerId)
                            && !string.IsNullOrEmpty(leftStepConfig.OwnerId)
                            group manualApprove.Id by leftStepConfig.OwnerId into groupResult
                            select new { groupResult.Key, Count = groupResult.LongCount() };
                #region 
                //var query = from manualApprove in context.ManualApprove
                //            join instance in context.WorkflowInstance
                //            on manualApprove.WorkflowInstanceId equals instance.Id into leftInstances
                //            from leftInstance in leftInstances.DefaultIfEmpty()
                //            join stepConfig in context.WorkflowStepConfiguration
                //            on leftInstance.CurStepId equals stepConfig.StepId.ToString() into leftStepConfigs
                //            from leftStepConfig in leftStepConfigs.DefaultIfEmpty()
                //            join exclude in context.WorkflowExcludeInstanceOwner
                //            on leftInstance.CurStepId equals exclude.StepId into leftExcludes
                //            from leftExclude in leftExcludes.DefaultIfEmpty()
                //            where manualApprove.SourceFlag == (int)sourceFlag
                //            && manualApprove.Status == (int)SOApproveDBStatus.WaitingApprove
                //            && manualApprove.ExtendDispositionCustomTime < currentTicks
                //            && manualApprove.WorkflowInstanceId != Guid.Empty
                //            && (
                //                leftExclude.Id == null ||
                //                (
                //                    leftExclude.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase)
                //                    && (
                //                        leftExclude.InstanceId != leftInstance.Id 
                //                        || leftExclude.OwnerId != leftStepConfig.OwnerId
                //                    )
                //                )
                //            )
                //            group manualApprove.Id by leftStepConfig.OwnerId into groupResult
                //            select new { groupResult.Key, Count = groupResult.LongCount()};
                #endregion
                do
                {
                    var datas = query.OrderBy(item => item.Key).Skip(limit * pageIndex++).Take(limit).ToDictionary(item => item.Key, item => item.Count);
                    count = datas.Count;
                    yield return datas;
                } while (count == limit);
            }
        }

        public IEnumerable<Dictionary<string, long>> GetWaitingApprovalWorkflowOwnersBySiteOwnerReivewerType(SourceFlag sourceFlag, int limit = 5000)
        {
            var pageIndex = 0;
            var count = 0;
            var currentTicks = DateTime.UtcNow.Ticks;

            using(var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
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
                            where manualApprove.SourceFlag == (int)sourceFlag
                            && leftStep.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners
                            && manualApprove.Status == (int)SOApproveDBStatus.WaitingApprove
                            && manualApprove.ExtendDispositionCustomTime < currentTicks
                            && manualApprove.WorkflowInstanceId != Guid.Empty
                            && manualApprove.SiteId == leftWorkflowSiteOwner.SiteId
                            && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id && item.OwnerId == leftWorkflowSiteOwner.OwnerId)
                            group manualApprove.Id by leftWorkflowSiteOwner.OwnerId into groupResult
                            select new { groupResult.Key, Count = groupResult.LongCount() };
                do
                {
                    var datas = query.OrderBy(item => item.Key).Skip(limit * pageIndex++).Take(limit).ToDictionary(item => item.Key, item => item.Count);
                    count = datas.Count;
                    yield return datas;

                } while (count == limit);
            }
        }

        public Dictionary<string, int> ConvertOwnerUniqueIdsToIntIds(IEnumerable<string> ids)
        {
            using(var context = GetNewContext())
            {
                return context.Account.Where(item => item.IsRemoved == 0 && ids.Contains(item.UserId)).Select(item => new { item.UserId, item.Id }).ToDictionary(item => item.UserId, item => item.Id);
            }
        }

        public Dictionary<int, string> ConvertOwnerIdsToNames(IEnumerable<int> ids)
        {
            using(var context = GetNewContext())
            {
                return context.Account.Where(item => item.IsRemoved == 0 && ids.Contains(item.Id)).Select(item => new { item.Id, item.DisplayName }).ToDictionary(item => item.Id, item => item.DisplayName);
            }
        }

        public List<RMAccount> GetAccountInfosByOnwerIds(IEnumerable<int> ids)
        {
            using(var context = GetNewContext())
            {
                return context.Account.Where(item => item.IsRemoved == 0 && ids.Contains(item.Id)).ToList();
            }
        }

        public List<RMAccount> GetAccountInfosByUserIds(IEnumerable<string> userIds)
        {
            using (var context = GetNewContext())
            {
                return context.Account.Where(item => item.IsRemoved == 0 && userIds.Contains(item.AADId)).ToList();
            }
        }
    }
}
