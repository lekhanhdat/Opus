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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.FileSystem;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Net;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.RMSharePointTaxnomy
{
    public class RMSyncTermProcessor : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string currentJobId { get; set; }

        #region private property  
        private RMSharePointTaxnomy mRMSharePointTaxonomy;
        private List<GRMTermGroup> mTermGroups;
        private bool mHasError;
        private int mFinishCount;
        private string mErrorMessage;
        private TermSyncJobMessage mJobMessage;
        #endregion

        public RMSyncTermProcessor()
        {

        }

        public void Bind(string msg)
        {
            currentJobId = JobContext.Current.JobId;
            mJobMessage = SerializerHelper.DeserializeByDataContractSerializer<TermSyncJobMessage>(msg);
        }

        public void Run()
        {
            try
            {
                Initialize();
                ProcessTermGroups();
                Finish();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while running term sync job. Error: {e}");
                mHasError = true;
                Finish();
            }
        }

        #region private method
        private void Initialize()
        {
            mRMSharePointTaxonomy = new RMSharePointTaxnomy
            {
                TermGroupIdMappingStoreIds = mJobMessage.TermGroupMembership.GroupBy(o => o.TermGroupId).ToDictionary(k => k.Key, value => value.Select(v => v.TermStoreId).Distinct().ToList())
            };
            mTermGroups = mJobMessage.TermGroupNodes;
            if (mTermGroups == null || mTermGroups.Count == 0)
            {
                mErrorMessage = "RM_TS_SS_Summary";
                throw new Exception("There is no term groups in records.");
            }
            JobContext.Current.mProgressManager.Create().IncreaseBase(mTermGroups.Count);
        }

        private void ProcessTermGroups()
        {
            foreach (var group in mTermGroups)
            {
                ProcessTermGroup(group);
            }
        }

        private void ProcessTermGroup(GRMTermGroup termGroup)
        {
            try
            {
                mRMSharePointTaxonomy.InitTermGroupRelationInfo(termGroup);
                RealSyncTermGroup(termGroup);
            }
            catch (Exception ex)
            {
                mErrorMessage = "RM_SYNC_InitException";
                logger.Warn("Process sync termTree in termGroup {0} error , ID {1}, detail message {2}", termGroup.Name.LogBase64(), termGroup.UniqueId, ex.ToString());
                JobContext.Current.JobDetailManager.Create().Commit(new JMTermSyncJobDetails() { Term = "RM_JS_Common_Pending", SiteCollectionURL = termGroup.Name, Action = @"N/A", MMSApplication = "RM_JS_Common_Pending", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,  Status = JobDetailsStatus.Failed, Comment = "RM_SYNC_InitException" });
                mHasError = true;
            }
            finally
            {
                mHasError |= mRMSharePointTaxonomy.JobHasError;
                mFinishCount += mRMSharePointTaxonomy.FinsihCount;
            }
        }

        private void RealSyncTermGroup(GRMTermGroup termGroup)
        {
            var termGroupId = termGroup.UniqueId;
            var termGroupName = termGroup.Name;
            logger.Info($"Process term group name: {termGroupName.LogBase64()}, id: {termGroupId}, isSpecified: {termGroup.UsingMMSSpecified}");
            if (termGroup.UsingMMSSpecified)
            {
                if (!mJobMessage.FarmTermGroupIdsRelation.Contains(termGroupId))
                {
                    logger.Info($"The term group don't need sync to current farm, name:{termGroupName.LogBase64()}, id:{termGroupId}");
                    return;
                }
            }
            try
            {
                mRMSharePointTaxonomy.SyncTermToSharePoint();
                logger.Info($"Success to sync term group, name: {termGroupName.LogBase64()}, id: {termGroupId}");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to sync term to Farm, Error: {e}");
                JobContext.Current.JobDetailManager.Create().Commit(new JMTermSyncJobDetails() { Term = "RM_JS_Common_Pending", SiteCollectionURL = "", Action = @"N/A", MMSApplication = "RM_JS_Common_Pending", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = "RM_SYNC_InitException" });
                mHasError = true;
            }
        }
       
        private void Finish()
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
            }

            if (!mHasError)
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, currentJobId);
            }
            else if (mHasError && mFinishCount > 0)
            {
                HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.FinishWithException, "RM_TS_SS_Summary");
            }
            else
            {
                HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.Failed, string.IsNullOrWhiteSpace(mErrorMessage) ? "RM_TS_SS_Summary" : mErrorMessage);
            }
            logger.Info("sync term to sp onprem job finished.");
        }

        #endregion
    }
}
