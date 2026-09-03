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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IDashboardDao
    {
        List<RMDashboardTermUsage> GetTop10TermUsageInfos(SourceFlag flag);

        List<RMDashboardTermUsage> GetTop10TermUsageInfos(SourceFlag flag, IEnumerable<string> termSetIds);

        List<RMDashboardDataUsage> GetTop10SiteUsageInfos(SourceFlag flag);

        List<RMDashboardDataUsage> GetTop10LocationUsageInfos(SourceFlag flag, IEnumerable<string> bottomLocationIds);

        List<RMDashboardDataUsage> GetTop10SiteUsageInfos(SourceFlag flag, IEnumerable<string> containerIds);

        List<RMDashboardDataUsage> GetTop10SiteUsageInfos(SourceFlag flag, IEnumerable<string> containerIds, List<string> fullPath);

        List<RMDashboardUserWaitingApprovalCount> GetTop10UserRecordsWaitingApproval(SourceFlag flag);

        List<RMDashboardDataUsageOfDate> GetDataUsageOfDates(SourceFlag sourceFlag, DateTime startTime);

        List<RMDashboardTermApplyRuleUsage> GetTermApplyRuleUsages();

        List<RMDashboardTermApplyRuleUsage> GetTermApplyRuleUsages(IEnumerable<string> termSetIds);
        List<RMDashboardTermApplyRuleUsage> GetLabelApplyRuleUsages();

        long GetExchangeSettingCount();

        long GetExchangeSettingCount(IEnumerable<Guid> containerIds);

        long GetFileSystemSettingCount();

        long GetOneDriveSettingCount();

        long GetOneDriveSettingCount(IEnumerable<Guid> containerIds);

        long GetPhysicalSettingCount();

        long GetPhysicalSettingCount(IEnumerable<Guid> locationIds);

        long GetSharePointOnPremiseSettingCount();

        long GetSharePointOnPremiseSettingCount(IEnumerable<Guid> containerIds);

        long GetSharePointSettingCount();

        long GetSharePointSettingCount(IEnumerable<Guid> containerIds);

        long GetAzureFileSettingCount();

        long GetBoxSettingCount();

        Dictionary<SourceFlag, long> GetActiveCountGroupBySource();

        long GetSourceActiveCount(SourceFlag sourceFlag);

        long GetSourceActiveCount(SourceFlag sourceFlag, IEnumerable<string> containerIds);

        long GetSourceStatusCountWithScopeId(SourceFlag sourceFlag, IEnumerable<string> scopeIds, Expression<Func<RMDashboardDataUsage,long>> func);

        long GetSourceActiveCount(SourceFlag sourceFlag, IEnumerable<string> containerIds, List<string> fullPaths);

        long GetSourceDestroyedCount(SourceFlag sourceFlag);

        long GetSourceDestroyedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds);

        long GetSourceDestroyedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds, List<string> fullPaths);

        long GetSourceArchivedCount(SourceFlag sourceFlag);

        long GetSourceArchivedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds);

        long GetSourceArchivedCount(SourceFlag sourceFlag, IEnumerable<string> containerIds, List<string> fullPaths);

        long GetPhysicalRequest(PhysicalRequestType physicalRequestType);

        long GetPhysicalRequest(PhysicalRequestType physicalRequestType, string userId);

        long GetPhysicalRequestByLocationIds(PhysicalRequestType physicalRequestType, List<Guid> locationIds);

        long GetPhysicalRequestByLocationIdsAndUserId(PhysicalRequestType physicalRequestType, List<Guid> locationIds, string userId);

        long GetPhysicalRequestByStatus(PhysicalRequestStatus status);

        long GetCountPhysicalRequestByStatusAndLocationIds(PhysicalRequestStatus status, List<Guid> locationIds);

        long GetPhysicalRequestByStatus(PhysicalRequestStatus status, string userId);

        long GetWaitingDisposalWaitingApproval(SourceFlag flag);

        long GetWaitingDisposalWaitingApproval(SourceFlag flag, IEnumerable<string> userAndGroupId, IEnumerable<string> userAndGroupIntId);

        long GetMyPhysicalRequest(PhysicalRequestType physicalRequestType, PhysicalRequestStatus physicalRequestStatus, string userId);

        long GetPhysicalTermTotal();

        long GetPhysicalTermTotal(IEnumerable<string> hasPermissionTermSetIds);

        long GetPhysicalLocationTotal();

        long GetCountLocationUnderTopLocations(List<Guid> topLocationIds);

        long GetLastCollectTime();

        long GetNextCollectTime();

        long GetGoogleSettingCount();

        long GetGoogleSettingCount(IEnumerable<Guid> containerIds);

        #region Teams
        long GetTeamsSettingCount();

        long GetTeamsSettingCount(IEnumerable<Guid> containerIds);
        #endregion
    }
}
