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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using GControlJobStatus = Cloud.Sdk.Data.Nexus.Foundation.JobStatus;
using GControlJobType = Cloud.Sdk.Data.Nexus.Foundation.JobType;
using GControlJobCategory = Cloud.Sdk.Data.Nexus.Foundation.JobCategory;
using GControlJobModule = Cloud.Sdk.Data.Nexus.Common.GCModuleType;

namespace AvePoint.RA.Common.Util;

public static class GControlJobEnumUtil
{
    public static GControlJobStatus ConvertToGControlJobStatus(this JobStatus opusJobStatus)
    {
        return opusJobStatus switch
        {
            JobStatus.Skipped => GControlJobStatus.Canceled,
            JobStatus.Failed => GControlJobStatus.Faulted,
            JobStatus.Stopped => GControlJobStatus.Stopped,
            JobStatus.Finished => GControlJobStatus.RanToCompletion,
            JobStatus.Pending or JobStatus.Wait => GControlJobStatus.WaitingToRun,
            JobStatus.FinishWithException => GControlJobStatus.FinishedWithException,
            _ => GControlJobStatus.Running
        };
    }
    
    public static GControlJobType ConvertToGControlJobType(this JobType opusJobType)
    {
        return opusJobType switch
        {
            JobType.GoogleApplySettings => GControlJobType.OpusApplySettings,
            JobType.GoogleDataSynchronization => GControlJobType.OpusSyncContentForSearch,
            JobType.GoogleRecordsDisposal => GControlJobType.OpusEnforceRuleAction,
            JobType.TermSynchronization => GControlJobType.OpusSyncClassifications,
            JobType.GoogleArchiverRestore => GControlJobType.OpusGoogleArchiveRestore,
            JobType.GoogleArchiverRetention => GControlJobType.OpusGoogleGoogleArchiveRetention,
            JobType.ExplorerOfflineSearch => GControlJobType.OpusExplorerOfflineSearch,
            JobType.GlobalSearchAction => GControlJobType.OpusGlobalSearchAction,
            JobType.ManualApprovalOrRejectJob => GControlJobType.ManualApprovalOrRejectJob,
            JobType.SyncNodesFromAOS => GControlJobType.OpusSyncNodesFromAOS,
            JobType.SyncSecurityContainer => GControlJobType.OpusSyncSecurityContainer,
            JobType.Dashboard => GControlJobType.OpusSyncDashboardData,
            JobType.ManualApprovalEmailSchedule => GControlJobType.OpusManualApprovalEmailSchedule,
            JobType.MachineLearningReviewReclassify => GControlJobType.OpusMachineLearningReviewReclassify,
            JobType.MachineLearningReviewApprove => GControlJobType.OpusMachineLearningReviewApprove,
            JobType.ImportTermStructure => GControlJobType.OpusImportTermStructure,
            JobType.DiscoveryGoogleJobV1 => GControlJobType.OpusDiscoveryGoogleJobV1,
            JobType.DiscoveryGoogleProfileJob => GControlJobType.OpusDiscoveryGoogleProfileJob,
            _ => throw new NotSupportedException($"Not support job type {opusJobType}")
        };
    }
}