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
//using AvePoint.Adonis.Records.Object.ActionOnly;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ActionOnly.Base;
//using AvePoint.RA.SharePoint.SPObjects.Collection;
using AvePoint.Records.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ActionOnly.SPActionOnly
{
    public class SPActionWorker
    {
        private IRMSubJobDao mSubJobDao { set; get; }
        protected IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }
        protected string JobId;
        protected static readonly IAveLogger logger = AveLogger.GetInstance(typeof(SPActionWorker));

        protected static IRMReportManager ReportManager
        {
            get
            {
                return ReportMangerFactory.Instance.ReportManager;
            }
        }

        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }

        #region 子job更新进度和状态的接口
        private IJobInfoUpdater _jobInfoUpdater;
        protected IJobInfoUpdater JobInfoUpdater
        {
            get
            {
                if (_jobInfoUpdater == null)
                {
                    _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
                }
                return _jobInfoUpdater;
            }
        }
        #endregion

        bool HasErrorNode = false;
        public SPActionWorker(string subJobId)
        {
            JobId = subJobId;

            JobInfoUpdater.UpdateJobState(JobId, (int)JobStatus.InProgress);
            JobInfoUpdater.UpdateJobProgress(JobId, 1);  //使用这个更新子job的进度, 才会级联到主job

            ReportManager.StartUpdateJobProgress();
        }
        public void Run()
        {
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(JobId, true);
            var recordsTreeNodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJobWithContext.JobContext.Settings);
            List<SPTreeNodeDto> runJobNodes = new List<SPTreeNodeDto>();
            recordsTreeNodes.ForEach(node => runJobNodes.Add(RMDtoConverter.ConvertRMTree2SPTree(node)));
            var excludeNodes = SerializerHelper.DeserializeByDataContractSerializer<List<RuleNodeContract>>(subJobWithContext.JobContext.Content);
            var recordsRules = RuleService.GetRulesFromRecords().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToList();
            foreach (var node in runJobNodes)
            {
                try
                {
                    logger.Info($"Run job node: {node?.FullPath}");
                    bool errorNode = false;
                    SPActionProcessorByQuery queryWorker = new SPActionProcessorByQuery(node, recordsRules);
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
                    //    using (new RA.Common.PerformanceScope(string.Format("Sync")))
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
                    HasErrorNode = true;
                    logger.Error($"Start action job failed {e.ToString()}");
                }
            }
            FinalUpdate(HasErrorNode);
        }
        public void FinalUpdate(bool hasErrorNode)
        {
            if (hasErrorNode)
            {
                ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_SS_CommonErrorMessage");
            }
            else
            {
                ReportManager.SetJobFinished(JobStatus.Finished);
            }
        }
    }
}
