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
//using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AI;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.VectorDataCenter.Models;
using AvePoint.RA.VectorDataCenter.Services;
using AvePoint.RA.VectorDataCenter.Similarity;
using AvePoint.RA.VectorDataCenter.Storage;
using Newtonsoft.Json;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAGoogle.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMSettingProcessor
    {

        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMSettingProcessor));

        public RMSettingProcessor(string jobId, JobQueueMessage jobQueueMsg)
        {
            currentJobId = jobId;
            InitCurrentJobInfo();
            #region fips logic for now not used
            //FipsModeUtil.InitControlCryptoMode();
            //if (CspCommunicationWrapper.CommunicationEncryptionKey == null)
            //{
            //    RMCPDocAveConnection docave = DocAveConnectionDaoService.Find(a => a.Id > 0);
            //}
            //ReportMangerFactory.Instance.Init(jobId, JobType.ApplySharePointSettings);
            ReportManager.StartUpdateJobProgress();
            #endregion
            SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
            mJobQueueMsg = jobQueueMsg;

            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                MainJobId = currentJobId.Split('_')[0],
                SubJobId = currentJobId,
                JobType = jobQueueMsg.JobType
            });
        }

        #region for SharePoint Settings Job
        private string currentJobId;

        protected JobQueueMessage mJobQueueMsg;
        private bool hasErrorNode = false;
        private bool mJobHasStopped = false;
        private bool hasSuccessNode = false;
        private bool isGetAPPSFailed = false;
        private bool isAddBCSFailed = false;
        private bool isAddContainerFailed = false;
        private bool isEnablePhysicalFailed = false;
        private bool isEnableAppFailed = false;
        private bool applyTermHasError = false;
        private bool autoApplyTermHasError = false;
        private bool smartApplyTermHasError = false;
        public List<JMGlobalSettingJobDetails> jobDetails = new List<JMGlobalSettingJobDetails>();
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        private void InitCurrentJobInfo()
        {
            baseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)JobType.SharePointGlobalSetting };
            //var info = RMSettingsJob.GetRMSettingJob(currentJobId);
            //string jobInfo = Encoding.UTF8.GetString(Convert.FromBase64String(info.JobInfos));
            //jobSettings = SerializerHelper.DeserializeByDataContractSerializer<JobSettings>(jobInfo);
            //totalCount = jobSettings.Nodes.Count;
            //RMJobService.UpdateJobProgress(currentJobId, 1);//
            JobInfoUpdater.UpdateJobState(currentJobId, (int)JobStatus.InProgress);
            JobInfoUpdater.UpdateJobProgress(currentJobId, 1);  //使用这个更新子job的进度, 才会级联到主job
        }
        #endregion

        #region Interface
        private ISPSettingTreeService mSPTreeService;
        //private IJobMonitorService mJobService;
        private ISharePointSettingDao mSharePointSettingDao;
        private BaseJobDto baseJobDto;
        //private IJobDetailService mJobDetailService;
        private IRMSettingJobDao mSettingJobDao;
        private IRMSubJobDao SubJobDao { set; get; }
        private IRMKeyValueDao mKeyValueDao;
        private IRMMLTrainingModelDao mTrainingModelDao;
        private IRMMLTermDao mMLTermDao;
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

        protected IRMSettingJobDao RMSettingsJob
        {
            get
            {
                if (mSettingJobDao == null)
                {
                    mSettingJobDao = (IRMSettingJobDao)PlatformWindsorManager.GetService(typeof(IRMSettingJobDao));
                }
                return mSettingJobDao;
            }
        }
        //protected IJobDetailService JobDetailService
        //{
        //    get
        //    {
        //        if (mJobDetailService == null)
        //        {
        //            mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
        //        }
        //        return mJobDetailService;
        //    }
        //}
        protected ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
            }
        }
        //protected IJobMonitorService RMJobService
        //{
        //    get
        //    {
        //        if (mJobService == null)
        //        {
        //            mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
        //        }
        //        return mJobService;
        //    }
        //}
        protected ISPSettingTreeService RMSPTreeService
        {
            get
            {
                if (mSPTreeService == null)
                {
                    mSPTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mSPTreeService;
            }
        }

        protected IRMKeyValueDao KeyValueDao
        {
            get
            {
                if(mKeyValueDao == null)
                {
                    mKeyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
                }
                return (IRMKeyValueDao)mKeyValueDao;
            }
        }

        protected IRMMLTrainingModelDao RMMLTrainingModelDao
        {
            get
            {
                if(mTrainingModelDao == null)
                {
                    mTrainingModelDao = (IRMMLTrainingModelDao)PlatformWindsorManager.GetService(typeof(IRMMLTrainingModelDao));
                }
                return mTrainingModelDao;
            }
        }

        protected IRMMLTermDao RMMLTermDao
        {
            get
            {
                if(mMLTermDao == null)
                {
                    mMLTermDao = (IRMMLTermDao)PlatformWindsorManager.GetService(typeof(IRMMLTermDao));
                }
                return mMLTermDao;
            }
        }

        #endregion

        public RMSharePointColumn RevIMConfig = new RMSharePointColumn();






        /// <summary>
        /// Merge 3.5 To Online ApplySPSetting
        /// </summary>
        public async Task ApplySPSettingAsync(bool isSchduleJob)
        {
            using (var scope = new PerformanceScope("RMSettingProcessor.ApplySPSetting"))
            {
                CompoundDisposalStatistics.Instance.StartStatistic();
                Dictionary<Guid, RMSharePointSetting> gruopSetingMap = [];
                try
                {
                    AvePoint.Wrapper.Common.WrapperConfiguration.CheckFileContentDismatch = false;
                    logger.Info($"EnableCheckFileContentDismatch is {AvePoint.Wrapper.Common.WrapperConfiguration.CheckFileContentDismatch}");
                    List<SPTreeNodeDto> treeList = new List<SPTreeNodeDto>();
                    Dictionary<Guid, bool> lifecycleValueByScopeId = new();
                    if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(currentJobId))
                    {
                        //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                        RMSubJob subJobWithContext = SubJobDao.GetSubJob(currentJobId, true);

                        //for debug xwwang start
                        logger.Info("subJobWithContext.JobContext is null:{0}", subJobWithContext.JobContext == null);
                        logger.Info("subJobWithContext.JobContext.Settings is null:{0}", string.IsNullOrEmpty(subJobWithContext.JobContext.Settings));
                        logger.Info("subJobWithContext.JobContext.Content is null:{0}", string.IsNullOrEmpty(subJobWithContext.JobContext.Content));
                        //for debug xwwang end

                        List<RMSPTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJobWithContext.JobContext.Settings);
                        if (!string.IsNullOrEmpty(subJobWithContext.JobContext.Content))
                        {
                            gruopSetingMap = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<Guid, RMSharePointSetting>>(subJobWithContext.JobContext.Content);
                        }
                        tempList.ForEach(node =>
                        {
                            treeList.Add(RMDtoConverter.ConvertRMTree2SPTree(node));
                            if (Guid.TryParse(node.SPObjectId, out var scopeId)
                                && node.EnableLifecycleManagementForSharePointLists.HasValue)
                            {
                                lifecycleValueByScopeId[scopeId] = node.EnableLifecycleManagementForSharePointLists.Value;
                            }
                        });
                    }
                    if (!treeList.IsNullOrEmpty())
                    {
                        logger.Info("treenode count is {0}", treeList.Count);
                        using var scopeTokenUsage = TokenUsageCache.Begin();
                        logger.Info($"start to count Token usage");

                        foreach (var treeNode in treeList)
                        {
                            if (string.IsNullOrEmpty(treeNode.SPObjectId))
                            {
                                treeNode.SPObjectId = Guid.Empty.ToString();
                            }
                            logger.Info("current treenode SPObjectId:[{0}]", treeNode.SPObjectId);
                            List<SPTreeNodeDto> needProcessSites = new List<SPTreeNodeDto>();
                            var siteId = Guid.Empty;
                            var scopeId = Guid.Parse(treeNode.SPObjectId);
                            var groupId = Guid.Parse(GetGroupNode(treeNode).SPObjectId);

                            if (treeNode.Level != NodeLevel.WebApplication)
                            {
                                siteId = new Guid(GetSiteCollectionNode(treeNode).SPObjectId);
                            }

                            RMSharePointSetting setting = SharePointSettingDao.GetSettingInfoByScope(groupId, siteId, new Guid(treeNode.SPObjectId));
                            logger.Info("get sp setting for node, id:{0}, setting is null:{1}", treeNode.SPObjectId, setting == null);

                            bool? isSupportLockedSite = null;

                            if (setting == null)
                            {
                                if (gruopSetingMap != null && gruopSetingMap.ContainsKey(groupId))
                                {
                                    setting = gruopSetingMap[groupId];
                                }
                                else
                                {
                                    // run group level setting ,get group setting
                                    setting = SharePointSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
                                    if (setting == null)
                                    {
                                        logger.Warn("Setting not available {0}", treeNode.FullPath);
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                if (treeNode.Level == NodeLevel.List)
                                {
                                    var parentSetting = GetParentWebSeting(treeNode, siteId, out var nodeLevel);
                                    if (nodeLevel == NodeLevel.WebApplication || nodeLevel == NodeLevel.SiteCollection)
                                    {
                                        isSupportLockedSite = CheckSupportLockedSite(parentSetting.NodeInfo);
                                    }
                                    setting.EnableRelatedRecords = parentSetting.EnableRelatedRecords;
                                }
                            }

                            if (!gruopSetingMap.TryGetValue(groupId, out var gSetting))
                            {
                                gSetting = SharePointSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
                                gruopSetingMap[groupId] = gSetting;
                            }

                            var enableLifecycleManagementForSharePointLists = GetLifecycleValue(
                                lifecycleValueByScopeId,
                                treeNode.SPObjectId,
                                SPCommonUtility.DeserializeTreeNodeInfo(setting?.NodeInfo)?.EnableLifecycleManagementForSharePointLists
                                    ?? SPCommonUtility.DeserializeTreeNodeInfo(gSetting?.NodeInfo)?.EnableLifecycleManagementForSharePointLists
                                    ?? true);

                            setting.IsKeepSharePointDefaultValue = gSetting.IsKeepSharePointDefaultValue;
                            setting.SetTermForEmptyDefaultValue = gSetting.SetTermForEmptyDefaultValue;

                            if (!isSupportLockedSite.HasValue)
                            {
                                var spSetting = SharePointSettingDao.GetSettingInfoByScope(groupId, siteId, siteId);
                                isSupportLockedSite = spSetting != null
                                    ? CheckSupportLockedSite(spSetting.NodeInfo)
                                    : CheckSupportLockedSite(gSetting.NodeInfo);
                            }

                            await HandleAddOrUpdateVectorTerm(setting);
                            var predictionModeType = (int)PredictionModeType.MLTraining; // Default fallback
                            try
                            {
                                var extension = JsonConvert.DeserializeObject<Dictionary<string, string>>(mJobQueueMsg.Extension);
                                predictionModeType = bool.Parse(extension?.GetValueOrDefault("IsZeroShotMode")) ? (int)PredictionModeType.ZeroShot : (int)PredictionModeType.MLTraining;
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Error parsing extension or prediction mode type: {ex}");
                                // Use default fallback value already assigned
                            }
                            if (treeNode.Level == NodeLevel.WebApplication)
                            {
                                List<SPTreeNodeDto> spNodes = RMSPTreeService.BrowseSPTreeNode(treeNode);
                                foreach (SPTreeNodeDto siteCollection in spNodes)
                                {
                                    siteCollection.PredictionModeType = predictionModeType;
                                    var siteLifecycleValue = GetLifecycleValue(lifecycleValueByScopeId, siteCollection.SPObjectId, enableLifecycleManagementForSharePointLists);
                                    var processor = new SPSettingFullProcessor(setting, siteCollection, setting.SettingTime, baseJobDto, new SPOLabelUtility(true), isSupportLockedSite.Value, siteLifecycleValue);//TO DO setting.SettingTime From Job Message Later
                                    await processor.RunAsync();
                                    if (processor.GetAppsFailed)
                                    {
                                        isGetAPPSFailed = true;
                                    }
                                    if (processor.AddBCSFailed)
                                    {
                                        isAddBCSFailed = true;
                                    }
                                    if (processor.AddContainerFailed)
                                    {
                                        isAddContainerFailed = true;
                                    }
                                    if (processor.EnablePhysicalFailed)
                                    {
                                        isEnablePhysicalFailed = true;
                                    }
                                    if (processor.EnableAppFailed)
                                    {
                                        isEnableAppFailed = true;
                                    }
                                    if (processor.ApplyTermHasError)
                                    {
                                        applyTermHasError = true;
                                    }
                                    if (processor.AutoApplyTermHasError)
                                    {
                                        autoApplyTermHasError = true;
                                    }
                                    if (processor.SmartApplyTermHasError)
                                    {
                                        smartApplyTermHasError = true;
                                    }
                                    if (processor.GetNodeError)
                                    {
                                        hasErrorNode = true;
                                    }
                                    if (processor.GetNodeSuccess)
                                    {
                                        hasSuccessNode = true;
                                    }
                                    //processor.RunUpdateJobDetails();
                                    //RunUpdateJobDetails(jobDetails);
                                }
                            }
                            else
                            {
                                var processor = new SPSettingFullProcessor(setting, treeNode, setting.SettingTime, baseJobDto, new SPOLabelUtility(true), isSupportLockedSite.Value, enableLifecycleManagementForSharePointLists);//TO DO setting.SettingTime From Job Message Later
                                await processor.RunAsync();
                                if (processor.GetAppsFailed)
                                {
                                    isGetAPPSFailed = true;
                                }
                                if (processor.AddBCSFailed)
                                {
                                    isAddBCSFailed = true;
                                }
                                if (processor.AddContainerFailed)
                                {
                                    isAddContainerFailed = true;
                                }
                                if (processor.EnablePhysicalFailed)
                                {
                                    isEnablePhysicalFailed = true;
                                }
                                if (processor.EnableAppFailed)
                                {
                                    isEnableAppFailed = true;
                                }
                                if (processor.ApplyTermHasError)
                                {
                                    applyTermHasError = true;
                                }
                                if (processor.AutoApplyTermHasError)
                                {
                                    autoApplyTermHasError = true;
                                }
                                if (processor.SmartApplyTermHasError)
                                {
                                    smartApplyTermHasError = true;
                                }
                                if (processor.GetNodeError)
                                {
                                    hasErrorNode = true;
                                }
                                if (processor.GetNodeSuccess)
                                {
                                    hasSuccessNode = true;
                                }
                                //processor.RunUpdateJobDetails();
                                //RunUpdateJobDetails(jobDetails);
                            }
                        }
                        //UpdateSettingJobStatus();
                        var total = scopeTokenUsage.End();
                        logger.Info($"Grand total token usage: {total}");
                        try
                        {
                            RMMLManualApprovalEmailSender.Commit(currentJobId);
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"An error while commit manual reviewers, message: {e}");
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    mJobHasStopped = true;
                }
                catch (Exception ex)
                {
                    hasErrorNode = true;
                    logger.Error("error occurred while apply SharePoint settings: {0}", ex.ToString());
                }
                finally
                {
                    CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                    CompoundDisposalStatistics.Instance.WaitEndStatistic();
                    if (mJobHasStopped)
                    {
                        ReportManager.SetJobFinished(JobStatus.Stopped);
                    }
                    else
                    {
                        if (hasSuccessNode && hasErrorNode)
                        {
                            ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_TS_SS_Summary");
                        }
                        else if (!hasErrorNode)
                        {
                            if (isGetAPPSFailed || isAddBCSFailed)
                            {
                                ReportManager.SetJobFinished(JobStatus.Failed, "RM_TS_SS_Summary");
                            }
                            else if (isAddContainerFailed || isEnablePhysicalFailed || isEnableAppFailed
                                || applyTermHasError || autoApplyTermHasError || smartApplyTermHasError)
                            {
                                ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_TS_SS_Summary");
                            }
                            else
                            {
                                if (SPSettingsUtility.HasFailedReport)
                                {
                                    ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_TS_SS_Summary");
                                }
                                else
                                {
                                    ReportManager.SetJobFinished(JobStatus.Finished);
                                }
                            }
                        }
                        else if (!hasSuccessNode)
                        {
                            ReportManager.SetJobFinished(JobStatus.Failed, "RM_TS_SS_Summary");
                        }
                        else
                        {
                            ReportManager.SetJobFinished(JobStatus.Skipped, "RM_SS_JobSkip");
                        }
                    }
                }
            }
        }

        private async Task HandleAddOrUpdateVectorTerm(RMSharePointSetting setting)
        {
            try
            {
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                               (!setting.IsUsingExistColumnName || (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn)))
                {
                    if (((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
                        || (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault))
                    {
                        if (KeyValueDao.EnableZeroShotFeature() && RMMLTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot)
                        {
                            logger.Info("Handle Add or Update vector for the term");
                            var mlTerms = RMMLTermDao.GetAllMLTerm();
                            if(KeyValueDao.EnableShowPredictReport()) ReportTermInfo(mlTerms);
                            foreach (var term in mlTerms)
                            {
                                try
                                {
                                    IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                                    var queryService = await QueryService.CreateWithRAIProvider(vectorStore, new CosineSimilarityCalculator());
                                    var metaData = await queryService.QueryMetaDataByTermId(term.Id);
                                    if (!string.IsNullOrEmpty(term.Description) && !metaData.EqualsIgnoreCase(term.Description))
                                    {
                                        logger.Info($"The term {term.Id} do not have vector or do have description change");
                                        var vectorizationService = await VectorizationService.CreateWithRAIProvider(vectorStore);
                                        await vectorizationService.StoreTermAsync(new TermDescription
                                        {
                                            Id = term.Id,
                                            Name = term.Name,
                                            Description = term.Description
                                        });
                                    }
                                    else
                                    {
                                        logger.Info($"Skip update or create vector for term {term?.Id}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"Add or update the vector for term {term?.Id} has errors: {ex}");
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error($"Handle add or update vector term has error {e}");
            }
        }

        private static bool GetLifecycleValue(
            Dictionary<Guid, bool> values,
            string scopeId,
            bool fallback)
        {
            return Guid.TryParse(scopeId, out var id)
                && values.TryGetValue(id, out var value)
                ? value
                : fallback;
        }

        private void ReportTermInfo(List<MLTermDto> mlTerms)
        {
            try
            {
                var mlTermInfos = mlTerms.Select(_ => new TermReportInfo
                {
                    TermID = _.Id.ToString(),
                    TermName = _.Name,
                    AITermDescription = _.Description
                }).ToList();
                RMMLPredictHelper.jobId = currentJobId;
                RMMLPredictHelper.RMMLPredictLogReport.WriteTermNewRow(mlTermInfos);
            }
            catch (Exception e) 
            {
                logger.Warn($"Report term info has errors: {e}");
            }
        }

        protected SPTreeNodeDto GetSiteCollectionNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        protected SPTreeNodeDto GetGroupNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }

        public RMSharePointSetting GetParentWebSeting(SPTreeNodeDto node, Guid siteId, out NodeLevel nodeLevel)
        {
            RMSharePointSetting spSetting = null;
            nodeLevel = node.Level;

            if (node.Level == NodeLevel.WebApplication)
            {
                return SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty, true);
            }

            if (node.Level == NodeLevel.SiteCollection || node.Level == NodeLevel.Site)
            {
                spSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }

            if (spSetting == null)
            {
                spSetting = GetParentWebSeting(node.Parent, siteId, out nodeLevel);
            }
            if (node.Level == NodeLevel.List)
            {
                logger.Info("this list node: [{0}] use the node: [{1}] setting, enable related app is: {2}", node.FullPath, spSetting.FullPath, spSetting.EnableRelatedRecords);
            }
            return spSetting;
        }

        private bool CheckSupportLockedSite(string nodeInfo)
        {
            try
            {
                RMSPTreeNode rMSPTree = SPCommonUtility.DeserializeTreeNodeInfo(nodeInfo);
                logger.Info($"SupportLockedSite value in tree node is {rMSPTree.SupportLockedSite}");
                return rMSPTree.SupportLockedSite;
            }
            catch (Exception ex)
            {
                logger.Warn($"Process locked site collection error: {ex}");
            }
            return false;
        }
    }
    #region not used for config quick sharepoint setting
    /// <summary>
    /// not used for config quick sharepoint setting
    /// </summary>

    #endregion
    #region old logic
    //public void SetCustomSetting()
    //{
    //    RMSharePointColumn columnSetting = null;
    //    foreach (var node in jobSettings.Nodes)
    //    {
    //        try
    //        {
    //            logger.Info("Begin set custom column settings {0}:{1}", node.FullPath, node.ColumnName);
    //            progress++;
    //            currentHasSuccessNode = false;
    //            var sitecolNode = GetSiteCollectionNode(node);
    //            if (jobSettings.IsWebAPI)
    //            {
    //                node.ColumnName = GetMetadataColumn(new Guid(jobSettings.SiteGroupId));
    //            }
    //            else
    //            {
    //                node.ColumnName = GetMetadataColumn(new Guid(sitecolNode.Parent.SPObjectId));
    //            }
    //            //处理之前的defaultTermName半角全角的转换
    //            ProcessNodeBefore(node);
    //            columnSetting = new RMSharePointColumn(sitecolNode);
    //            columnSetting.jobId = jobSettings.JobId;
    //            columnSetting.jobType = JobType.SharePointInheritSetting;
    //            columnSetting.InitSiteObject(node, jobSettings.NeedCheckDefaultVaule, true);
    //            if (node.Level == (int)NodeLevel.Site)
    //            {
    //                node.WebId = new Guid(node.SPObjectId);
    //            }
    //            else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library))
    //            {
    //                if (!jobSettings.IsWebAPI)
    //                {
    //                    node.WebId = new Guid(node.Parent.Parent.SPObjectId);
    //                    node.ListId = new Guid(node.SPObjectId);
    //                }
    //            }

    //            if (!IsOnlySetPhysicalLibrary(node))
    //            {
    //                columnSetting.AddCustomColumn(node);
    //            }
    //            columnSetting.currentjobProgress = CalculateProgress(progress, totalCount, false);
    //            columnSetting.nextJobProgress = CalculateProgress(progress + 1, totalCount, false);
    //            columnSetting.totalListCounts = columnSetting.GetListCount(node);
    //            //Physical
    //            if (node.IsEnableHoldPhyical)
    //            {
    //                columnSetting.AddPhysicalFlagForSPNode(node);
    //            }
    //            else
    //            {
    //                columnSetting.CancelPhysicalFlagForSPNode(node);
    //            }
    //            if (!hasErrorNode)
    //            {
    //                hasErrorNode = !this.IsJobFinishWithoutException(columnSetting.SPSettingJobDetails);
    //            }
    //            //判断当前node的Job detail中是否有成功的记录
    //            if (!currentHasSuccessNode && !IsJobFailed(columnSetting.SPSettingJobDetails))
    //            {
    //                currentHasSuccessNode = true;
    //            }
    //            if (currentHasSuccessNode && !hasErrorNode)
    //            {
    //                hasSuccessNode = true;
    //            }
    //            node.isFailedConfigMetaDataColumn = IsJobFailedByType(columnSetting.SPSettingJobDetails, FailedType.ConfigColumn);
    //            node.isFailedConfigClassification = IsJobFailedByType(columnSetting.SPSettingJobDetails, FailedType.ConfigClassification);
    //            if (node.NodeType == 0 && node.Level == 300)
    //            {//tree node type is GenericList
    //                node.IsEnableHoldPhyical = false;
    //            }
    //            else
    //            {
    //                if (node.IsEnableHoldPhyical)
    //                {
    //                    node.IsEnableHoldPhyical = !IsJobFailedByType(columnSetting.SPSettingJobDetails, FailedType.ConfigPhysical);
    //                }
    //            }
    //            SharePointSettingDao.AddOrUpdateCustomSetting(node, columnSetting.GetSiteId());
    //        }
    //        catch (Exception e)
    //        {
    //            logger.Error("Set SPObject {0} custom column setting error :{1} ", node.FullPath, e.ToString());
    //            errorMessage = "RM_SYNC_InitException";
    //            if (columnSetting != null)
    //            {
    //                columnSetting.SPSettingJobDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = node.Name, SourceURL = GetSiteCollectionNode(node).FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage, Classification = "RM_JS_Common_Pending" });
    //            }
    //            else
    //            {
    //                List<JMGlobalSettingJobDetails> finalDetails = new List<JMGlobalSettingJobDetails>();
    //                finalDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = node.Name, SourceURL = GetSiteCollectionNode(node).FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage, Classification = "RM_JS_Common_Pending" });
    //                this.RunUpdateJobDetails(finalDetails);
    //            }
    //            hasErrorNode = true;
    //            node.isFailedConfigMetaDataColumn = true;
    //            node.isFailedConfigClassification = true;
    //            node.IsEnableHoldPhyical = false;
    //            SharePointSettingDao.AddOrUpdateCustomSetting(node, columnSetting.GetSiteId());
    //        }
    //        finally
    //        {
    //            if (columnSetting != null)
    //            {
    //                this.RunUpdateJobDetails(columnSetting.SPSettingJobDetails);
    //                columnSetting.SPSettingJobDetails.Clear();
    //                columnSetting.Dispose();
    //            }
    //            RMJobService.UpdateJobProgress(jobSettings.JobId, CalculateProgress(progress, totalCount));
    //        }
    //    }
    //    JobDetailService.UploadJobDetailsAndReport(baseJobDto);
    //    if (hasSuccessNode && hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
    //    }
    //    else if (!hasSuccessNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Failed, "RM_TS_SS_Summary");
    //    }
    //    else if (!hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Finished, "");
    //    }
    //    else if (hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
    //    }
    //    else
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Skipped, "RM_SS_JobSkip");
    //    }

    //}
    //public void InheritGlobalSetting()
    //{
    //    baseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)JobType.SharePointGlobalSetting };
    //    var info = RMSettingsJob.GetRMSettingJob(currentJobId);
    //    string jobInfo = Encoding.UTF8.GetString(Convert.FromBase64String(info.JobInfos));
    //    JobSettings jobSettings = SerializerHelper.DeserializeByDataContractSerializer<JobSettings>(jobInfo);
    //    Guid tempGroupId = Guid.Empty;
    //    RMSharePointSetting globalSetting = null;
    //    RMSharePointColumn inheritGlobalSetting = null;
    //    int totalCount = jobSettings.Nodes.Count;
    //    RMJobService.UpdateJobProgress(jobSettings.JobId, 1);
    //    int progress = 0;
    //    bool hasErrorNode = false;
    //    bool hasSuccessNode = false;
    //    string errorMessage = string.Empty;
    //    foreach (var node in jobSettings.Nodes)
    //    {
    //        try
    //        {
    //            bool currentHasSuccessNode = false;
    //            progress++;
    //            Guid groupId = new Guid(GetGroupNode(node).SPObjectId);
    //            if (groupId != tempGroupId)
    //            {
    //                globalSetting = SharePointSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
    //                tempGroupId = groupId;
    //            }
    //            #region init node from gloabal settings
    //            node.ColumnName = globalSetting.ColumnName;
    //            node.DefaultTermId = globalSetting.DefaultTermId;
    //            node.TermSetId = globalSetting.TermSetId;
    //            node.TermId = globalSetting.TermId != null ? globalSetting.TermId : Guid.Empty;
    //            node.DefaultTermName = globalSetting.DefaultTermName != null ? globalSetting.DefaultTermName : string.Empty;
    //            node.TermName = globalSetting.TermName;
    //            node.TermSetName = globalSetting.TermSetName;
    //            node.Description = globalSetting.Description;
    //            node.DescriptionOfContainer = globalSetting.DescriptionOfContainer;
    //            node.TermIdOfContainer = globalSetting.TermIdOfContainer;
    //            node.TermNameOfContainer = globalSetting.TermNameOfContainer;
    //            node.isEnableClassification = globalSetting.isEnableClassification;
    //            #endregion
    //            //处理之前的defaultTermName半角全角的转换
    //            ProcessNodeBefore(node);
    //            //开始处理
    //            inheritGlobalSetting = new RMSharePointColumn(GetSiteCollectionNode(node));
    //            inheritGlobalSetting.currentjobProgress = CalculateProgress(progress, totalCount, false);
    //            inheritGlobalSetting.nextJobProgress = CalculateProgress(progress + 1, totalCount, false);
    //            inheritGlobalSetting.totalListCounts = inheritGlobalSetting.GetListCount(node);
    //            inheritGlobalSetting.jobId = jobSettings.JobId;
    //            inheritGlobalSetting.jobType = JobType.SharePointInheritSetting;
    //            inheritGlobalSetting.BreakCustomColumn(node);
    //            //判断Job detail中是否有失败的记录
    //            if (!hasErrorNode)
    //            {
    //                hasErrorNode = !this.IsJobFinishWithoutException(inheritGlobalSetting.SPSettingJobDetails);
    //            }
    //            //判断当前node的Job detail中是否有成功的记录
    //            if (!IsJobFailed(inheritGlobalSetting.SPSettingJobDetails))
    //            {
    //                currentHasSuccessNode = true;
    //            }
    //            //根据当前node是否有处理成功的Job detail记录来判断是否要把这个node的Custom Settings的设置保存从数据库中删除
    //            if (currentHasSuccessNode)
    //            {
    //                SharePointSettingDao.DeleteSharePointSetting(new Guid(node.SPObjectId), inheritGlobalSetting.GetSiteId());
    //                if (!hasSuccessNode)
    //                {
    //                    hasSuccessNode = true;
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            hasErrorNode = true;
    //            logger.Warn("Inherit global settings failed {0}:{1}", node.FullPath, e.ToString());
    //            errorMessage = "RM_SYNC_InitException";
    //            if (inheritGlobalSetting != null)
    //            {
    //                inheritGlobalSetting.SPSettingJobDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = node.Name, SourceURL = GetSiteCollectionNode(node).FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage, Classification = "RM_JS_Common_Pending" });
    //            }
    //            else
    //            {
    //                List<JMGlobalSettingJobDetails> finalDetails = new List<JMGlobalSettingJobDetails>();
    //                finalDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = node.Name, SourceURL = GetSiteCollectionNode(node).FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage, Classification = "RM_JS_Common_Pending" });
    //                this.RunUpdateJobDetails(finalDetails);
    //            }
    //        }
    //        finally
    //        {
    //            //更新Job detail
    //            if (inheritGlobalSetting != null)
    //            {
    //                this.RunUpdateJobDetails(inheritGlobalSetting.SPSettingJobDetails);
    //                inheritGlobalSetting.SPSettingJobDetails.Clear();
    //                inheritGlobalSetting.Dispose();
    //            }
    //            //更新Job进度
    //            RMJobService.UpdateJobProgress(jobSettings.JobId, CalculateProgress(progress, totalCount));
    //        }
    //    }
    //    JobDetailService.UploadJobDetailsAndReport(baseJobDto);
    //    //Job结束，更新Job状态
    //    if (hasSuccessNode && hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
    //    }
    //    else if (!hasSuccessNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Failed, "RM_TS_SS_Summary");
    //    }
    //    else if (!hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Finished, "");
    //    }
    //    else if (hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
    //    }
    //    else
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Skipped, "RM_SS_JobSkip");
    //    }
    //}
    //public void SetGlobalSetting()
    //{
    //    #region init job progress info && job setting
    //    baseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)JobType.SharePointGlobalSetting };
    //    var info = RMSettingsJob.GetRMSettingJob(currentJobId);
    //    string jobInfo = Encoding.UTF8.GetString(Convert.FromBase64String(info.JobInfos));
    //    JobSettings jobSettings = SerializerHelper.DeserializeByDataContractSerializer<JobSettings>(jobInfo);
    //    RMSharePointColumn globalSetting = null;
    //    RMJobService.UpdateJobProgress(jobSettings.JobId, 1);

    //    int progress = 0;
    //    bool hasErrorNode = false;
    //    bool hasSuccessNode = false;
    //    string errorMessage = string.Empty;
    //    int totalCount = 0;
    //    //browse出当前选择的group node下所有的site collection
    //    Dictionary<string, List<RMSPTreeNode>> processNodesMap = GetTotalRMSPTreeNode(jobSettings.Nodes, ref totalCount);
    //    if (totalCount == 0)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
    //        return;
    //    }
    //    RMJobService.UpdateJobProgress(jobSettings.JobId, 5);
    //    #endregion 
    //    #region 
    //    foreach (var groupNode in jobSettings.Nodes)
    //    {
    //        bool groupHasSuccessNode = false;
    //        bool groupHasConfigColumnSuccessNode = false;
    //        bool groupHasConfigClassificationSuccessNode = false;
    //        List<RMSPTreeNode> currentGroupNodes = processNodesMap[groupNode.SPObjectId];
    //        //若当前group下没有browse出site collection，则跳过处理这个group node
    //        if (currentGroupNodes == null || currentGroupNodes.Count == 0)
    //        {
    //            List<JMGlobalSettingJobDetails> finalDetails = new List<JMGlobalSettingJobDetails>();
    //            finalDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = groupNode.Name, SourceURL = groupNode.FullPath, ColumnName = groupNode.ColumnName, Action = "Skipped", Status = JobDetailsStatus.Skipped, Comment = "RM_SS_NoSCUnderGroup", Classification = groupNode.TermNameOfContainer });
    //            this.RunUpdateJobDetails(finalDetails);
    //            continue;
    //        }

    //        if (groupNode.IsUsingExistColumnName)
    //        {
    //            logger.Info("Begin set global column settings. Path:[{0}] ExistingColumnName:[{1}]", groupNode.FullPath, groupNode.ExistColumnName);
    //            AddExistingColumnDetail(groupNode);
    //        }
    //        else
    //        {
    //            logger.Info("Begin set global column settings {0}:{1}", groupNode.FullPath, groupNode.ColumnName);
    //        }
    //        //处理当前group下所有的site collection
    //        foreach (var siteCollectionNode in currentGroupNodes)
    //        {
    //            try
    //            {
    //                progress++;
    //                logger.Info("Begin set global column settings site collection url {0}:{1}", siteCollectionNode.FullPath, siteCollectionNode.ColumnName);
    //                var siteCollectionSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(siteCollectionNode.SPObjectId), new Guid(siteCollectionNode.SPObjectId));
    //                //
    //                if (siteCollectionSetting != null)
    //                {
    //                    //to do log & report
    //                    //若曾经设置过custom settings则只会修改这个site collection的column的name和description
    //                    if (siteCollectionSetting.ColumnName != groupNode.ColumnName || (groupNode.Description != null && siteCollectionSetting.Description != groupNode.Description))
    //                    {
    //                        #region init term property for custom settings
    //                        siteCollectionNode.ColumnName = groupNode.ColumnName;
    //                        siteCollectionNode.Description = groupNode.Description;
    //                        siteCollectionNode.IsUsingExistColumnName = groupNode.IsUsingExistColumnName;
    //                        siteCollectionNode.ExistColumnName = groupNode.ExistColumnName;
    //                        siteCollectionNode.DefaultTermId = siteCollectionSetting.DefaultTermId;
    //                        siteCollectionNode.DefaultTermName = siteCollectionSetting.DefaultTermName;
    //                        siteCollectionNode.TermId = siteCollectionSetting.TermId;
    //                        siteCollectionNode.TermName = siteCollectionSetting.TermName;
    //                        siteCollectionNode.TermSetId = siteCollectionSetting.TermSetId;
    //                        siteCollectionNode.TermSetName = siteCollectionSetting.TermSetName;
    //                        siteCollectionNode.TermStoreId = siteCollectionSetting.TermStoreId;
    //                        siteCollectionNode.FullPath = siteCollectionSetting.FullPath;
    //                        siteCollectionNode.DescriptionOfContainer = siteCollectionSetting.DescriptionOfContainer;
    //                        siteCollectionNode.TermIdOfContainer = siteCollectionSetting.TermIdOfContainer;
    //                        siteCollectionNode.TermNameOfContainer = siteCollectionSetting.TermNameOfContainer;
    //                        siteCollectionNode.isFailedConfigClassification = siteCollectionSetting.isFailedConfigClassification;
    //                        #endregion
    //                        //更新column的name和description
    //                        globalSetting = new RMSharePointColumn(siteCollectionNode);
    //                        globalSetting.currentjobProgress = CalculateProgress(progress, totalCount, true); ;
    //                        globalSetting.nextJobProgress = CalculateProgress(progress + 1, totalCount, true);
    //                        globalSetting.totalListCounts = globalSetting.GetListCount(siteCollectionNode);
    //                        globalSetting.jobId = jobSettings.JobId;
    //                        globalSetting.RenameSiteColumn(siteCollectionNode);
    //                        logger.Warn("SiteCollection has custom settings {0}", siteCollectionNode.FullPath);
    //                        //判断Job detail中是否有失败的记录
    //                        if (!hasErrorNode)
    //                        {
    //                            hasErrorNode = !this.IsJobFinishWithoutException(globalSetting.SPSettingJobDetails);
    //                        }
    //                        //判断当前group的Job detail中是否有成功的记录
    //                        if (!groupHasSuccessNode && !IsJobFailed(globalSetting.SPSettingJobDetails))
    //                        {
    //                            groupHasSuccessNode = true;
    //                        }
    //                        if (!groupHasConfigClassificationSuccessNode && !IsJobFailedByType(globalSetting.SPSettingJobDetails, FailedType.ConfigClassification))
    //                        {
    //                            groupHasConfigClassificationSuccessNode = true;
    //                        }
    //                        if (!groupHasConfigColumnSuccessNode && !IsJobFailedByType(globalSetting.SPSettingJobDetails, FailedType.ConfigColumn))
    //                        {
    //                            groupHasConfigColumnSuccessNode = true;
    //                        }
    //                        this.RunUpdateJobDetails(globalSetting.SPSettingJobDetails);
    //                        globalSetting.SPSettingJobDetails.Clear();
    //                        continue;
    //                    }
    //                    List<JMGlobalSettingJobDetails> finalDetails = new List<JMGlobalSettingJobDetails>();
    //                    finalDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = siteCollectionNode.Name, SourceURL = siteCollectionNode.FullPath, ColumnName = "RM_JS_Common_Pending", Action = I18NEntity.GetString("RM_JS_JMD_Status_SkipSiteCollectionColumn"), Status = JobDetailsStatus.Skipped, Comment = I18NEntity.GetString("RM_JS_JMD_Comment_ConfiguredCustomSettings"), Classification = "RM_JS_Common_Pending" });
    //                    this.RunUpdateJobDetails(finalDetails);
    //                    groupHasConfigClassificationSuccessNode = true;
    //                    groupHasConfigColumnSuccessNode = true;
    //                    groupHasSuccessNode = true;
    //                    continue;
    //                }
    //                AddNodeProperty(siteCollectionNode, groupNode);
    //                //处理之前的defaultTermName半角全角的转换
    //                ProcessNodeBefore(siteCollectionNode);
    //                globalSetting = new RMSharePointColumn(siteCollectionNode);
    //                globalSetting.currentjobProgress = CalculateProgress(progress, totalCount, true); ;
    //                globalSetting.nextJobProgress = CalculateProgress(progress + 1, totalCount, true);
    //                globalSetting.totalListCounts = globalSetting.GetListCount(siteCollectionNode);
    //                globalSetting.jobId = jobSettings.JobId;
    //                globalSetting.InitSiteObject(siteCollectionNode, jobSettings.NeedCheckDefaultVaule, false);
    //                globalSetting.ConfigSiteCollectionSetting(siteCollectionNode);
    //                //判断Job detail中是否有失败的记录
    //                if (!hasErrorNode)
    //                {
    //                    hasErrorNode = !this.IsJobFinishWithoutException(globalSetting.SPSettingJobDetails);
    //                }
    //                //判断当前group的Job detail中是否有成功的记录
    //                if (!groupHasSuccessNode && !IsJobFailed(globalSetting.SPSettingJobDetails))
    //                {
    //                    groupHasSuccessNode = true;
    //                }
    //                if (!groupHasConfigClassificationSuccessNode && !IsJobFailedByType(globalSetting.SPSettingJobDetails, FailedType.ConfigClassification))
    //                {
    //                    groupHasConfigClassificationSuccessNode = true;
    //                }
    //                if (!groupHasConfigColumnSuccessNode && !IsJobFailedByType(globalSetting.SPSettingJobDetails, FailedType.ConfigColumn))
    //                {
    //                    groupHasConfigColumnSuccessNode = true;
    //                }

    //            }
    //            catch (Exception exc)
    //            {
    //                hasErrorNode = true;
    //                errorMessage = "RM_SYNC_InitException";
    //                logger.Error("Add Global Settings Error Path {0} : {1}", groupNode.FullPath, exc.ToString());
    //                if (globalSetting != null)
    //                {
    //                    globalSetting.SPSettingJobDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = siteCollectionNode.Name, SourceURL = siteCollectionNode.FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage, Classification = "RM_JS_Common_Pending" });
    //                }
    //                else
    //                {
    //                    //在初始化site collection之前，就出现了异常
    //                    List<JMGlobalSettingJobDetails> finalDetails = new List<JMGlobalSettingJobDetails>();
    //                    finalDetails.Add(new JMGlobalSettingJobDetails() { ObjectName = siteCollectionNode.Name, SourceURL = siteCollectionNode.FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage, Classification = "RM_JS_Common_Pending" });
    //                    this.RunUpdateJobDetails(finalDetails);
    //                }
    //            }
    //            finally
    //            {
    //                if (globalSetting != null)
    //                {
    //                    //更新Job detail
    //                    this.RunUpdateJobDetails(globalSetting.SPSettingJobDetails);
    //                    globalSetting.SPSettingJobDetails.Clear();
    //                    globalSetting.Dispose();
    //                }
    //                //更新Job进度
    //                RMJobService.UpdateJobProgress(jobSettings.JobId, CalculateProgress(progress, totalCount, true));
    //            }
    //        }
    //        if (groupHasSuccessNode && !hasSuccessNode)
    //        {
    //            hasSuccessNode = true;
    //        }
    //        groupNode.isFailedConfigMetaDataColumn = !groupHasConfigColumnSuccessNode;
    //        groupNode.isFailedConfigClassification = !groupHasConfigClassificationSuccessNode;
    //        SharePointSettingDao.AddOrUpdateGlobalSetting(groupNode);
    //    }
    //    JobDetailService.UploadJobDetailsAndReport(baseJobDto);
    //    #endregion
    //    //Job结束，更新Job状态
    //    if (hasSuccessNode && hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
    //    }
    //    else if (!hasSuccessNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Failed, "RM_TS_SS_Summary");
    //    }
    //    else if (!hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Finished, "");
    //    }
    //    else if (hasErrorNode)
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
    //    }
    //    else
    //    {
    //        RMJobService.UpdateJobStatus(jobSettings.JobId, JobStatus.Skipped, "RM_SS_JobSkip");
    //    }
    //}
    #endregion
}
