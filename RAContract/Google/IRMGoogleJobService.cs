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
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Google.Model;

namespace AvePoint.RA.Contract.Google
{
    public interface IRMGoogleJobService
    {
        Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod, string scopeId = null, string driveId = null, string fullPath = null);
        Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string selectedNode);
        RAReturnMessage ApplySettingsOnSelectedNode(RMGoogleTreeNode node);
        RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod);
        RAReturnMessage RunRecordsDisposalJob(RMGoogleTreeNode node);
        Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string parameters);
        Task<RAReturnMessage> RunEnforceRuleActionScheduleJobAsync(RMGoogleTreeNode node, JobRunBy jobRunBy);
        Task<RAReturnMessage> RunImportGoogleTermStructure(JobRunBy jobRunBy, RMGoogleTermGroupSetting setting);
        string RealRunImportGoogleTermJob(JobRunBy jobRunBy, string jobRunByUser, string termGroupId);
        RAReturnMessage RunDataSyncJob(JobRunBy jobRunBy, string jobRunByUser = "");
    }
}