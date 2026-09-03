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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;

namespace AvePoint.RA.Contract.Box
{
    public interface IRMBoxSettingsService
    {
        bool EnqueueDataSyncJob(BoxTreeNode treeNode);
        void EnqueueDataSyncScheduleJob(bool isFromTimerPage);
        Task<string> RealRunDataSyncJobAsync(string jobRunByUser, string selectedNodeJson);
        Task<string> RealRunDataSyncScheduleJobAsync(string jobRunByUser);
        Task<(bool, BoxSettingDto)> TryGetSettingInfoAsync(string scopeId, string containerId, string connectionId = "", string userId = "");
        System.Threading.Tasks.Task ResetSyncSettingAsync(string scopeId, string containerId, string connectionId = "", string userId = "");
        System.Threading.Tasks.Task SaveNodeSettingAsync(BoxSettingDto dto);
        System.Threading.Tasks.Task SaveActiveSettingAsync(BoxSettingDto dto);
        System.Threading.Tasks.Task InheritParentSettingAsync(BoxTreeNode node);
        Task<BoxSettingDto> LoadNodeSettingAsync(BoxTreeNode node);
        RAReturnMessage EnqueueRunRecordsDisposalJob(BoxTreeNode treeNode);
        Task<string> RealRunBoxRecordsDisposalJobAsync(string jobRunByUser, string selectedNodeJson);
        Task<string> RealRunBoxRecordsDisposalJobForApprovalAsync(string jobRunByUser);
        RAReturnMessage RunBoxEnforceRuleActionScheduleJob(BoxSettingDto boxSetting, JobRunBy jobRunBy);
        ScheduleInfo GetScheduleInfo(List<Guid> ids);
        Task<bool> SyncADUsersAsync(List<ToUserInfo> users);
    }
}
