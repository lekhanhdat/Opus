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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
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
    public class DashboardDao : IDashboardDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(DashboardDao));
        private RMDbContext Context => RMDBContextManager.GetNewDBContext();

        public List<RMDashboardTermUsage> GetTop10TermUsageInfos(SourceFlag flag)
        {
            using (var context = Context)
            {
                var query = from termUsage in context.DashboardTermUsage
                            where termUsage.SourceFlag == (int)flag
                            && termUsage.Active > 0
                            select termUsage;
                return query.OrderByDescending(item => item.Active).ThenBy(item => item.TermName).Take(10).ToList();
            }
        }

        public List<RMDashboardTermUsage> GetTop10TermUsageInfos(SourceFlag flag, IEnumerable<string> termSetIds)
        {
          
            using (var context = Context)
            {
                var query = context.DashboardTermUsage.Where(termUsage => termUsage.SourceFlag == (int)flag
                                                                            && termUsage.Active > 0);
                if (flag is not (SourceFlag.Google or SourceFlag.GGControl))
                {
                    query = query.Where(t => termSetIds.Contains(t.TermSetId));
                }

                return query.OrderByDescending(item => item.Active).ThenBy(item => item.TermName).Take(10).ToList();
            }
        }

        public List<RMDashboardDataUsage> GetTop10SiteUsageInfos(SourceFlag flag)
        {
            using (var context = Context)
            {
                var query = from dataUsage in context.DashboardDataUsage
                            where dataUsage.SourceFlag == (int)flag
                            && dataUsage.Active > 0
                            select dataUsage;
                return query.OrderByDescending(item => item.Active).ThenBy(item => item.Title).Take(10).ToList();
            }
        }

        public List<RMDashboardDataUsage> GetTop10LocationUsageInfos(SourceFlag flag, IEnumerable<string> bottomLocationIds)
        {
            using (var context = Context)
            {
                var query = from dataUsage in context.DashboardDataUsage
                            where dataUsage.SourceFlag == (int)flag
                            && bottomLocationIds.Contains(dataUsage.ScopeId)
                            && dataUsage.Active > 0
                            select dataUsage;
                return query.OrderByDescending(item => item.Active).ThenBy(item => item.Title).Take(10).ToList();
            }
        }

        public List<RMDashboardDataUsage> GetTop10SiteUsageInfos(SourceFlag flag, IEnumerable<string> containerIds)
        {
            using (var context = Context)
            {
                var query = from dataUsage in context.DashboardDataUsage
                            where dataUsage.SourceFlag == (int)flag
                            && containerIds.Contains(dataUsage.ContainerId)
                            && dataUsage.Active > 0
                            select dataUsage;
                return query.OrderByDescending(item => item.Active).ThenBy(item => item.Title).Take(10).ToList();
            }
        }

        public List<RMDashboardDataUsage> GetTop10SiteUsageInfos(SourceFlag flag, IEnumerable<string> containerIds, List<string> fullPath)
        {
            using (var context = Context)
            {
                var query = from dataUsage in context.DashboardDataUsage
                            where dataUsage.SourceFlag == (int)flag
                            && (containerIds.Contains(dataUsage.ContainerId) || (RMConstants.DefaultPrivateChannelSitesGroupId.Equals(dataUsage.ContainerId) && fullPath.Contains(dataUsage.Path)))
                            && fullPath.Contains(dataUsage.Path)
                            && dataUsage.Active > 0
                            select dataUsage;
                return query.OrderByDescending(item => item.Active).ThenBy(item => item.Title).Take(10).ToList();
            }
        }

        public List<RMDashboardUserWaitingApprovalCount> GetTop10UserRecordsWaitingApproval(SourceFlag flag)
        {
            using (var context = Context)
            {
                var query = from userWaitingApproval in context.DashboardUserWaitingApprovalCount
                            where userWaitingApproval.SourceFlag == (int)flag
                            && userWaitingApproval.Count > 0
                            select userWaitingApproval;
                return query.OrderByDescending(item => item.Count).ThenBy(item => item.DisplayName).Take(10).ToList();
            }
        }

        public List<RMDashboardDataUsageOfDate> GetDataUsageOfDates(SourceFlag sourceFlag, DateTime startTime)
        {
            using (var context = Context)
            {
                var query = from dataUsageOfDate in context.DashboardDataUsageOfDate
                            where dataUsageOfDate.SourceFlag == (int)sourceFlag
                            && dataUsageOfDate.Date >= startTime.Ticks
                            select dataUsageOfDate;
                return query.OrderBy(item => item.Date).ToList();
            }
        }

        public List<RMDashboardTermApplyRuleUsage> GetTermApplyRuleUsages()
        {
            using (var context = Context)
            {
                var query = from termApplyRuleUsage in context.DashboardTermApplyRuleUsage
                            select termApplyRuleUsage;
                return query.ToList();
            }
        }

        public List<RMDashboardTermApplyRuleUsage> GetTermApplyRuleUsages(IEnumerable<string> termSetIds)
        {
            using (var context = Context)
            {
                var query = from termApplyRuleUsage in context.DashboardTermApplyRuleUsage
                            where termSetIds.Contains(termApplyRuleUsage.TermSetId)
                            select termApplyRuleUsage;
                return query.ToList();
            }
        }

        public List<RMDashboardTermApplyRuleUsage> GetLabelApplyRuleUsages()
        {
            using (var context = Context)
            {
                var query = from labelApplyRuleUsage in context.DashboardTermApplyRuleUsage
                            where labelApplyRuleUsage.TermSetId.Equals("")
                                && labelApplyRuleUsage.TermGroupId.Equals("")
                            select labelApplyRuleUsage;
                return query.ToList();
            }
        }


        public long GetExchangeSettingCount()
        {
            using (var context = Context)
            {
                var exoSettings = context.RMExchangeOnlineSettings.Where(item => !item.IsRemoved);
                var remoteNodes = context.RMMailboxes.Where
                                (item => exoSettings.Any
                                    (setting => setting.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                                    && (setting.GroupId == new Guid(item.ParentId) || string.IsNullOrEmpty(item.ParentId)))).LongCount();
                return remoteNodes;
            }
        }

        public long GetExchangeSettingCount(IEnumerable<Guid> containerIds)
        {
            using (var context = Context)
            {
                var exoSettings = context.RMExchangeOnlineSettings.Where(item => !item.IsRemoved && containerIds.Contains(item.GroupId));
                var remoteNodes = context.RMMailboxes.Where
                               (item => exoSettings.Any
                                   (setting => setting.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                                   && (setting.GroupId == new Guid(item.ParentId) || string.IsNullOrEmpty(item.ParentId)))).LongCount();
                return remoteNodes;
            }
        }

        public long GetFileSystemSettingCount()
        {
            using (var context = Context)
            {
                return context.RMFileSystemSettings.Where(item => item.IsActive).LongCount();
            }
        }

        public long GetOneDriveSettingCount()
        {
            using var context = Context;
            return context.RMOneDriveSettings.Where(item => !item.IsRemoved).LongCount();
        }

        public long GetOneDriveSettingCount(IEnumerable<Guid> containerIds)
        {
            using var context = Context;
            return context.RMOneDriveSettings.Where(item => !item.IsRemoved && containerIds.Contains(item.SiteGroupId)).LongCount();
        }

        public long GetPhysicalSettingCount()
        {
            using (var context = Context)
            {
                return context.RMPhysicalRecordSetting.Where(item => !item.IsRemoved).LongCount();
            }
        }

        public long GetPhysicalSettingCount(IEnumerable<Guid> locationIds)
        {
            using (var context = Context)
            {
                return context.RMPhysicalRecordSetting.Where(item => !item.IsRemoved && locationIds.Contains(item.LocationUniqueId)).LongCount();
            }
        }

        public long GetSharePointOnPremiseSettingCount()
        {
            using (var context = Context)
            {
                return context.RMSharePointOnPremiseSettings.Where(item => !item.IsRemoved).LongCount();
            }
        }

        public long GetSharePointOnPremiseSettingCount(IEnumerable<Guid> containerIds)
        {
            using (var context = Context)
            {
                return context.RMSharePointOnPremiseSettings.Where(item => !item.IsRemoved && containerIds.Contains(item.SiteGroupId)).LongCount();
            }
        }

        public long GetSharePointSettingCount()
        {
            using var context = Context;
            return context.RMSharePointSettings.Where(item => !item.IsRemoved).LongCount();
        }

        public long GetSharePointSettingCount(IEnumerable<Guid> containerIds)
        {
            using var context = Context;
            return context.RMSharePointSettings.Where(item => !item.IsRemoved && containerIds.Contains(item.SiteGroupId)).LongCount();
        }

        public long GetAzureFileSettingCount() 
        {
            using (var context = Context)
            {
                return context.RMAzureFileShareSettings.LongCount();
            }
        }

        public long GetBoxSettingCount()
        {
            using (var context = Context)
            {
                return context.RMBoxSettings.LongCount();
            }
        }

        public Dictionary<SourceFlag, long> GetActiveCountGroupBySource()
        {
            using(var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking().GroupBy(item => item.SourceFlag).ToDictionary(item => (SourceFlag)item.Key,
                    item => item.Select(innerItem => innerItem.Active).Sum()
                    );
            }
        }

        public long GetSourceActiveCount(SourceFlag sourceFlag)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking().Where(item => item.SourceFlag == (int)sourceFlag).Sum(item => (long?)item.Active)??0;
            }
        }

        public long GetSourceActiveCount(SourceFlag sourceFlag, IEnumerable<string> containerIds)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && containerIds.Contains(item.ContainerId))
                    .Select(item => (long?)item.Active)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceStatusCountWithScopeId(SourceFlag sourceFlag, IEnumerable<string> scopeIds, Expression<Func<RMDashboardDataUsage, long>> func)
        {
            using var context = Context;
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && scopeIds.Contains(item.ScopeId))
                    .Select(func)
                    .Select(item => (long?)item)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceActiveCount(SourceFlag sourceFlag, IEnumerable<string> containerIds, List<string> fullPaths)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && (containerIds.Contains(item.ContainerId) || (RMConstants.DefaultPrivateChannelSitesGroupId.Equals(item.ContainerId) && fullPaths.Contains(item.Path))))
                    .Select(item => (long?)item.Active)
                    .Sum() ?? 0;
            }
        }
        public long GetSourceDestroyedCount(SourceFlag sourceFlag)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag)
                    .Select(item => (long?)item.Destroyed)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceArchivedCount(SourceFlag sourceFlag)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag)
                    .Select(item => (long?)item.Archived)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceDestroyedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && containerIds.Contains(item.ContainerId))
                    .Select(item => (long?)item.Destroyed)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceDestroyedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds, List<string> fullPaths)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && (containerIds.Contains(item.ContainerId) || (RMConstants.DefaultPrivateChannelSitesGroupId.Equals(item.ContainerId) && fullPaths.Contains(item.Path))))
                    .Select(item => (long?)item.Destroyed)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceArchivedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && containerIds.Contains(item.ContainerId))
                    .Select(item => (long?)item.Archived)
                    .Sum() ?? 0;
            }
        }

        public long GetSourceArchivedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds, List<string> fullPaths)
        {
            using (var context = Context)
            {
                return context.DashboardDataUsage.AsNoTracking()
                    .Where(item => item.SourceFlag == (int)sourceFlag && (containerIds.Contains(item.ContainerId) || (RMConstants.DefaultPrivateChannelSitesGroupId.Equals(item.ContainerId) && fullPaths.Contains(item.Path))))
                    .Select(item => (long?)item.Archived)
                    .Sum() ?? 0;
            }
        }

        public long GetPhysicalRequest(PhysicalRequestType physicalRequestType)
        {
            using (var context = Context)
            {
                return context.RMPhysicalRequest.Where(item => item.Type == (int)physicalRequestType && item.Status != (int)PhysicalRequestStatus.CancelRequest && item.GroupRequestId == Guid.Empty).LongCount()
                    + context.RMPhysicalRequest.Where(item => item.Type == (int)physicalRequestType && item.Status != (int)PhysicalRequestStatus.CancelRequest && item.GroupRequestId != Guid.Empty).Select(item => item.GroupRequestId).Distinct().LongCount();
            }
        }

        public long GetPhysicalRequest(PhysicalRequestType physicalRequestType, string userId)
        {
            using (var context = Context)
            {
                return context.RMPhysicalRequest.Where(item => item.CreatedUserId == userId && item.Type == (int)physicalRequestType && item.Status != (int)PhysicalRequestStatus.CancelRequest).LongCount();
            }
        }
        
        public long GetPhysicalRequestByLocationIds(PhysicalRequestType physicalRequestType, List<Guid> locationIds)
        {
            if (locationIds == null || locationIds.Count == 0)
            {
                return 0;
            }

            using (var context = Context)
            {
                var locationPatterns = locationIds
                    .Select(locationId => $"<LocationId>{locationId}</LocationId>")
                    .ToList();

                return context.RMPhysicalRequest
                    .Where(item => item.Type == (int)physicalRequestType
                        && item.Status != (int)PhysicalRequestStatus.CancelRequest
                        && item.GroupRequestId == Guid.Empty
                        && (locationPatterns.Any(pattern => item.MetaData.Contains(pattern)) || !item.MetaData.Contains("<LocationId>")))
                    .LongCount() + context.RMPhysicalRequest
                    .Where(item => item.Type == (int)physicalRequestType
                        && item.Status != (int)PhysicalRequestStatus.CancelRequest
                        && item.GroupRequestId != Guid.Empty
                        && (locationPatterns.Any(pattern => item.MetaData.Contains(pattern)) || !item.MetaData.Contains("<LocationId>")))
                    .Select(_ => _.GroupRequestId)
                    .Distinct()
                    .LongCount();
            }
        }


        public long GetPhysicalRequestByLocationIdsAndUserId(PhysicalRequestType physicalRequestType, List<Guid> locationIds, string userId)
        {
            if (locationIds == null || locationIds.Count == 0)
            {
                return 0;
            }

            using (var context = Context)
            {
                var locationPatterns = locationIds
                    .Select(locationId => $"<LocationId>{locationId}</LocationId>")
                    .ToList();

                return context.RMPhysicalRequest
                    .Where(item => item.Type == (int)physicalRequestType
                        && item.CreatedUserId == userId
                        && item.Status != (int)PhysicalRequestStatus.CancelRequest
                        && (locationPatterns.Any(pattern => item.MetaData.Contains(pattern))) || !item.MetaData.Contains("<LocationId>"))
                    .LongCount();
            }
        }        

        public long GetPhysicalRequestByStatus(PhysicalRequestStatus status)
        {
            using (var context = Context)
            {
                return context.RMPhysicalRequest.Where(item => item.Status == (int)status).LongCount();
            }
        }

        public long GetCountPhysicalRequestByStatusAndLocationIds(PhysicalRequestStatus status, List<Guid> locationIds)
        {
            using (var context = Context)
            {
                var locationPatterns = locationIds
                    .Select(locationId => $"<LocationId>{locationId}</LocationId>")
                    .ToList();

                return context.RMPhysicalRequest.Where(item => item.Status == (int)status && (locationPatterns.Any(pattern => item.MetaData.Contains(pattern)) || !item.MetaData.Contains("<LocationId>"))).LongCount();
            }
        }

        public long GetPhysicalRequestByStatus(PhysicalRequestStatus status, string userId)
        {
            using (var context = Context)
            {
                return context.RMPhysicalRequest.Where(item => item.CreatedUserId == userId && item.Status == (int)status).LongCount();
            }
        }

        public long GetWaitingDisposalWaitingApproval(SourceFlag flag)
        {
            var currentTicks = DateTime.UtcNow.Ticks;
            using (var context = Context)
            {
                return context.ManualApprove.Where(item => item.SourceFlag == (int)flag && item.Status == (int)SOApproveDBStatus.WaitingApprove && item.ExtendDispositionCustomTime < currentTicks).LongCount();
            }
        }

        public long GetWaitingDisposalWaitingApproval(SourceFlag flag, IEnumerable<string> userAndGroupId, IEnumerable<string> userAndGroupIntId)
        {

            var currentTicks = DateTime.UtcNow.Ticks;

            using (var context = Context)
            {
                HashSet<Guid> GetWorkflowInstanceIds()
                {
                    var workflowQuery = from instance in context.WorkflowInstance
                                        join stepConfig in context.WorkflowStepConfiguration
                                        on instance.CurStepId equals stepConfig.StepId.ToString()
                                        where instance.Status == RMWorkflowStatus.Running
                                        && userAndGroupId.Contains(stepConfig.OwnerId)
                                        && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(instance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == instance.Id && item.OwnerId == stepConfig.OwnerId)
                                        select instance.Id;
                    return workflowQuery.ToHashSet();
                }

                //var intUserId = context.Account.FirstOrDefault(item => item.UserId == userId).Id.ToString();
                var workflowInstanceIds = GetWorkflowInstanceIds();

                var manualApproveItems = context.ManualApprove.Where(item => item.SourceFlag == (int)flag && item.Status == (int)SOApproveDBStatus.WaitingApprove && item.ExtendDispositionCustomTime < currentTicks).ToList();
                var count = GetWaitingDisposalWaitingApprovalBySiteOwnerReview(manualApproveItems, userAndGroupId);

                return manualApproveItems.Where(
                    item => (
                    !string.IsNullOrEmpty(item.EscalateTo) 
                    && item.EscalateTo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Any(escalateToId => userAndGroupIntId.Contains(escalateToId)))
                    || (item.WorkflowInstanceId != Guid.Empty && workflowInstanceIds.Contains(item.WorkflowInstanceId))
                       ).LongCount() + count;
            }
        }

        private long GetWaitingDisposalWaitingApprovalBySiteOwnerReview(List<RMManualApprove> manualApproveItems, IEnumerable<string> userAndGroupId)
        {
            using(var context = Context)
            {
                var query = from manualApprove in manualApproveItems
                            join instance in context.WorkflowInstance
                            on manualApprove.WorkflowInstanceId equals instance.Id into leftInstances
                            from leftInstance in leftInstances
                            join step in context.WorkflowStep
                            on leftInstance.CurStepId equals step.Id.ToString() into leftSteps
                            from leftStep in leftSteps
                            join workflowSiteOwner in context.WorkflowSiteOwner
                            on leftInstance.DefinitionId.ToString() equals workflowSiteOwner.DefinitionId into leftWorkflowSiteOwners
                            from leftWorkflowSiteOwner in leftWorkflowSiteOwners
                            where userAndGroupId.Any(item => item == leftWorkflowSiteOwner.OwnerId)
                            && manualApprove.SiteId == leftWorkflowSiteOwner.SiteId
                            && leftStep.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners
                            //&& !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id)
                            && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(leftInstance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == leftInstance.Id && item.OwnerId == leftWorkflowSiteOwner.OwnerId)
                            select manualApprove;
                return query.Count();
            }
        }

        public long GetMyPhysicalRequest(PhysicalRequestType physicalRequestType, PhysicalRequestStatus physicalRequestStatus, string userId)
        {
            using (var context = Context)
            {
                return context.RMPhysicalRequest.Where(item => item.CreatedUserId == userId
                && item.Type == (int)physicalRequestType
                && item.Status == (int)physicalRequestStatus).LongCount();
            }
        }

        public long GetPhysicalTermTotal()
        {
            using (var context = Context)
            {
                return context.Terms.Where(item => !item.IsRemoved).LongCount();
            }
        }

        public long GetPhysicalTermTotal(IEnumerable<string> hasPermissionTermSetIds)
        {
            using (var context = Context)
            {
                var query = from terms in context.Terms
                            join termSets in context.TermSets
                            on terms.TermSetId equals termSets.Id
                            where hasPermissionTermSetIds.Any(item => item == termSets.UniqueId.ToString()) 
                            && !terms.IsRemoved
                            select terms.Id;
                return query.LongCount();
            }
        }

        public long GetPhysicalLocationTotal()
        {
            using (var context = Context)
            {
                return context.RMLocation.Where(item => !item.IsRemoved && item.NodeType != (int)RMNodeLevel.PhysicalRootLocation).LongCount();
            }
        }

        public long GetCountLocationUnderTopLocations(List<Guid> topLocationIds)
        {
            try
            {
                using var context = Context;

                var topLocationPaths = context.RMLocation
                     .Where(x => !x.IsRemoved && topLocationIds.Contains(x.UniqueId))
                     .Select(x => new
                     {
                         x.DirPath,
                         x.Id
                     })
                     .AsEnumerable()
                     .Select(x => $"{x.DirPath}{x.Id}")
                     .ToList();

                var result = context.RMLocation
                    .Where(x => !x.IsRemoved &&
                                (topLocationPaths.Any(p => x.DirPath.StartsWith(p)) || topLocationIds.Contains(x.UniqueId)))
                    .Select(x => x.UniqueId)
                    .Distinct()
                    .LongCount();
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Count location under top location have errors: {e}");
                return new();
            }
        }

        public long GetLastCollectTime()
        {
            using(var context = Context)
            {
                var data = context.DashboardDataUsageOfDate.FirstOrDefault(item => item.SourceFlag == (int)SourceFlag.None);
                if(data == null)
                {
                    return 0;
                }
                return data.Date;
            }
        }

        public long GetNextCollectTime()
        {
            using(var context = Context)
            {
                var data = context.Schedule.FirstOrDefault(item => item.JobCategory == (int)ScheduleType.Dashboard);
                if(data == null)
                {
                    return 0;
                }
                return data.NextTime;
            }
        }

        public long GetGoogleSettingCount()
        {
            using var ctx = Context;
            return ctx.RMGoogleSettings.Where(x => !x.IsRemoved).LongCount();
        }

        public long GetGoogleSettingCount(IEnumerable<Guid> containerIds)
        {
            using var ctx = Context;
            return ctx.RMGoogleSettings.Where(x => !x.IsRemoved && containerIds.Contains(x.ContainerId)).LongCount();
        }

        public long GetTeamsSettingCount()
        {
            using var ctx = Context;
            return ctx.RMTeamsSettings.Where(x => !x.IsRemoved).LongCount();
        }

        public long GetTeamsSettingCount(IEnumerable<Guid> containerIds)
        {
            using var ctx = Context;
            return ctx.RMTeamsSettings.Where(x => !x.IsRemoved && containerIds.Contains(x.TeamsGroupId)).LongCount();
        }
    }
}
