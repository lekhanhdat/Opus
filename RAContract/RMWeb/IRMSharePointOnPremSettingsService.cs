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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMSharePointOnPremSettingsService
    {

        Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod);
        RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod);
        string RunSharepointSettingsScheduleJob(JobRunBy jobRunBy);
        string RunOnPremiseEnforceRuleActionScheduleJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        Task<string> RealSharepointSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage);
        Task<string> RealRunOnpremiseEnforceRuleActionJobAsync(string jobRunByUser, JobRunBy jobRunBy, string param);
        Task<string> RealRunOnpremiseEnforceRuleActionJobForApprovalAsync(string jobRunByUser, JobRunBy jobRunBy);
        RAReturnMessage RunOnpremiseEnforceRuleActionJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        string GetApplySettingJobMessage(string jobId);
        Task<string> GetEnforceRuleActionJobMessageAsync(string jobId);
        string GetDataSyncJobMessage(string jobId);
        string GetUniqueIdSettingJobMessage(string jobId);
        string GetGlobalSearchActionJobMessage(string jobId);
        List<string> GetDesignLists();
        Task<RMSPTreeNode> LoadSampleNodeSettingsAsync(RMSPSampleTreeNode sNode);
        System.Threading.Tasks.Task LoadSPSettingIconAsync(List<RMSPSampleTreeNode> nodes);
        Task<RAReturnMessage> AddColumnSettingAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddUsingExistColumnSettingAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddGlobalColumnAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddCustomColumnAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddContainerTermAsync(RMSPTreeNode containerNode);
        Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddEnableColumnSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node);
        bool CheckParentNodeDisable(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true);
        RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node);
        Task<RAReturnMessage> AddIsSyncSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> AddSPOnPremGeneralSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> RunDataSyncJobAsync(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunSPDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null);
        RAReturnMessage RunSPDataSyncScheduleJob(JobRunBy jobRunBy);
        Task<bool> NeedRunUniqueIdJobAsync(List<RMSPTreeNode> needRunNodes = null);
        Task<(RecordsReturnMessage,string)> SPOnPremDeclaredItemRecordsAsync(List<Guid> ids, bool isDeclared);
        Task<(RecordsReturnMessage,string)> SPOnPremUnDeclaredItemRecordsAsync(List<Guid> ids, bool isDeclared);
        Task<(RecordsReturnMessage,string)> UpdateOnPremTermsAsync(AvePoint.RA.Contract.Object.RealTime.ChangeTermOption changeTermInfo);
        Task<bool> CheckHasAvailableAgentAsync();
    }
}
