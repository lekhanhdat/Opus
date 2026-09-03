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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using RAFileSystem.SharePoint.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.SharePoint.EnforceRuleAction
{
    public class SPEnforceRuleActionWorker : IScheduleJobWorker
    {
        protected string JobId;
        protected static readonly IAveLogger logger = AveLogger.GetInstance(typeof(SPEnforceRuleActionWorker));
        private EnforceRuleActionJobMessage mMessage;
        bool HasErrorNode = false;

        public SPEnforceRuleActionWorker(string subJobId)
        {
            JobId = subJobId;
            //JobInfoUpdater.UpdateJobState(JobId, (int)JobStatus.InProgress);
            //JobInfoUpdater.UpdateJobProgress(JobId, 1);  //使用这个更新子job的进度, 才会级联到主job
            //ReportManager.StartUpdateJobProgress();
        }

        public SPEnforceRuleActionWorker()
        {
        }

        public void Run()
        {
            //RMSubJob subJobWithContext = SubJobDao.GetSubJob(JobId, true);
            //var recordsTreeNodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJobWithContext.JobContext.Settings);
            //logger.Info($"Run job node:{subJobWithContext.JobContext.Settings}");
            List<SPTreeNodeDto> runJobNodes = new List<SPTreeNodeDto>();
            //recordsTreeNodes.ForEach(node => runJobNodes.Add(RMDtoConverter.ConvertRMTree2SPTree(node)));
            var excludeNodes = new List<RuleNodeContract>(); //SerializerHelper.DeserializeByDataContractSerializer<List<RuleNodeContract>>(subJobWithContext.JobContext.Content);
            var recordsRules = new List<Rule>(); //RuleService.GetRulesFromDA().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToList();
            foreach (var node in runJobNodes)
            {
                try
                {
                    bool errorNode = false;
                    SPEnforceRuleActionProcessorByQuery queryWorker = new SPEnforceRuleActionProcessorByQuery(node, mMessage);
                    queryWorker.ExcludeNodes = excludeNodes;
                    errorNode = queryWorker.Run();
                    #region old logic
                    //logger.Info($"Start Action Job {node.FullPath} : {JobMessage.DiscoverType}");
                    //if (JobMessage.DiscoverType == 0)
                    //{
                    //    HasErrorNode = true;
                    //    throw new Exception("Current Discover type is invalid");
                    //}
                    //else if (JobMessage.DiscoverType == 1)
                    //{
                    //    SPActionProcessorByQuery queryWorker = new SPActionProcessorByQuery(JobMessage, node);
                    //    errorNode = queryWorker.Run();
                    //}
                    //else if (JobMessage.DiscoverType == 2)
                    //{
                    //    SPActionProcessorByExp expWorker = new SPActionProcessorByExp(JobMessage, node);
                    //    errorNode = expWorker.Run();
                    //    using (new RA.Common.AgentPerformanceScope(string.Format("Sync")))
                    //    {
                    //        try
                    //        {
                    //            logger.Info("Sync data to explorer when job finish");
                    //            SPObjectCollectionIncremental exIncSync = new SPObjectCollectionIncremental(JobMessage.AllRecordsRule);
                    //            exIncSync.ExcuteScan(node);
                    //        }
                    //        catch (Exception se)
                    //        {
                    //            logger.Info($"Sync data failed {node.Url} {se.ToString()}");
                    //        }
                    //    }
                    //}
                    //else if (JobMessage.DiscoverType == 3)
                    //{
                    //    SPActionProcessorNormal normal = new SPActionProcessorNormal(JobMessage, node);
                    //    errorNode = normal.Run();
                    //}
                    //else
                    //{
                    //    HasErrorNode = true;
                    //    throw new Exception("Invalid Discover type");
                    //}
                    #endregion
                    if (errorNode)
                    {
                        HasErrorNode = true;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Start action job failed {e.ToString()}");
                }
            }
            FinalUpdate(HasErrorNode);
        }

        public void RunEnforceRuleAction()
        {
            //logger.Info($"Run job node:{subJobWithContext.JobContext.Settings}");
            List<SPTreeNodeDto> runJobNodes = new List<SPTreeNodeDto>();
            mMessage.TreeNodes.ForEach(node => runJobNodes.Add(RMDtoConverter.ConvertRMTree2SPTree(node)));
            var excludeNodes = new List<RuleNodeContract>(); //SerializerHelper.DeserializeByDataContractSerializer<List<RuleNodeContract>>(subJobWithContext.JobContext.Content);
            foreach (var node in runJobNodes)
            {
                try
                {
                    bool errorNode = false;
                    SPEnforceRuleActionProcessorByQuery queryWorker = new SPEnforceRuleActionProcessorByQuery(node, mMessage);
                    queryWorker.ExcludeNodes = excludeNodes;
                    errorNode = queryWorker.Run();
                    if (errorNode)
                    {
                        HasErrorNode = true;
                    }
                }
                catch (Exception e)
                {
                    HasErrorNode = true;
                    logger.Error($"Start action job failed {e.ToString()}.");
                }
            }
            FinalUpdate(HasErrorNode);
        }

        public void FinalUpdate(bool hasErrorNode)
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while cleaning up. Error:{0}.", e.ToString());
            }
            if (hasErrorNode)
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.FinishWithException, JobContext.Current.JobId);
            }
            else
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, JobContext.Current.JobId);
            }
            logger.Info("EnforceRuleAction job finished.");
        }

        void IScheduleJobWorker.Bind(string msg)
        {
            mMessage = SerializerHelper.DeserializeByDataContractSerializer<EnforceRuleActionJobMessage>(msg);
        }

        void IScheduleJobWorker.Run()
        {
            RunEnforceRuleAction();
        }
    }
}
