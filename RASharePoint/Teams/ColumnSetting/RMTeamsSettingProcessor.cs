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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AI;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.RMSharePointColumn;
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

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting
{
    public class RMTeamsSettingProcessor
    {
        private readonly IRALogger logger = RALogger.GetInstance(typeof(RMTeamsSettingProcessor));
        #region variable
        private string currentJobId;
        public RMSharePointColumn.RMSharePointColumn RevIMConfig = new RMSharePointColumn.RMSharePointColumn();
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
        private BaseJobDto baseJobDto;
        protected JobQueueMessage mJobQueueMsg;

        #endregion

        #region service
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
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ITeamsSettingTreeService RMTeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private IRMMLTrainingModelDao RMMLTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        #endregion
        public RMTeamsSettingProcessor(string jobId, JobQueueMessage jobQueueMsg)
        {
            currentJobId = jobId;
            ReportManager.StartUpdateJobProgress();
            mJobQueueMsg = jobQueueMsg;
            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                MainJobId = currentJobId.Split('_')[0],
                SubJobId = currentJobId,
                JobType = jobQueueMsg.JobType
            });
        }

        public async Task ApplyTeamsSettingAsync(bool isSchduleJob)
        {
            using (var scope = new PerformanceScope("RMTeamsSettingProcessor.ApplyTeamsSetting"))
            {
                CompoundDisposalStatistics.Instance.StartStatistic();
                Dictionary<Guid, RMTeamsSetting> gruopSetingMap = [];
                try
                {
                    Wrapper.Common.WrapperConfiguration.CheckFileContentDismatch = false;
                    logger.Info($"EnableCheckFileContentDismatch is {Wrapper.Common.WrapperConfiguration.CheckFileContentDismatch}");
                    List<SPTreeNodeDto> treeList = new List<SPTreeNodeDto>();
                    Dictionary<Guid, bool> lifecycleValueByScopeId = new();
                    if (RA.Common.JobService.JobServiceUtility.IsSubJob(currentJobId))
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
                            gruopSetingMap = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<Guid, RMTeamsSetting>>(subJobWithContext.JobContext.Content);
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
                            var teamsId = Guid.Empty;
                            var scopeId = Guid.Parse(treeNode.SPObjectId);
                            var groupId = Guid.Parse(GetGroupNode(treeNode).SPObjectId);

                            if (treeNode.Level != NodeLevel.WebApplication)
                            {
                                teamsId = new Guid(GetTeamsNode(treeNode).TeamsId);
                                siteId = treeNode.Level != NodeLevel.Office365GroupEntire ? new Guid(GetSiteCollectionNode(treeNode).SPObjectId) : Guid.Empty;
                            }

                            RMTeamsSetting setting = TeamsSettingDao.GetSettingInfoByScope(groupId, teamsId, siteId, new Guid(treeNode.SPObjectId));
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
                                    setting = TeamsSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, Guid.Empty, groupId);
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
                                    var parentSetting = GetParentWebSeting(treeNode, teamsId, siteId, out var nodeLevel);
                                    if (nodeLevel == NodeLevel.WebApplication 
                                        || nodeLevel == NodeLevel.Office365GroupEntire 
                                        || nodeLevel == NodeLevel.SiteCollection)
                                    {
                                        isSupportLockedSite = CheckSupportLockedSite(parentSetting.NodeInfo);
                                    }
                                    setting.EnableRelatedRecords = parentSetting.EnableRelatedRecords;
                                }
                            }

                            if (!gruopSetingMap.TryGetValue(groupId, out var gSetting))
                            {
                                gSetting = TeamsSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, Guid.Empty, groupId);
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
                                var tempSetting = TeamsSettingDao.GetSettingInfoByScope(groupId, teamsId, siteId, siteId);
                                tempSetting ??= TeamsSettingDao.GetSettingInfoByScope(groupId, teamsId, Guid.Empty, teamsId);
                                isSupportLockedSite = tempSetting != null
                                    ? CheckSupportLockedSite(tempSetting.NodeInfo)
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
                                List<SPTreeNodeDto> teamsNodes = RMTeamsTreeService.BrowseTeamsTreeNode(treeNode);
                                foreach (SPTreeNodeDto teams in teamsNodes)
                                {
                                    List<SPTreeNodeDto> virtualSiteCollectionNode = RMTeamsTreeService.BrowseTeamsTreeNode(treeNode);
                                    if (virtualSiteCollectionNode == null || virtualSiteCollectionNode.Count == 0) continue;
                                    List<SPTreeNodeDto> siteCollectionNodes = RMTeamsTreeService.BrowseTeamsTreeNode(virtualSiteCollectionNode[0]);
                                    foreach (SPTreeNodeDto site in siteCollectionNodes)
                                    {
                                        site.PredictionModeType = predictionModeType;
                                        var siteLifecycleValue = GetLifecycleValue(lifecycleValueByScopeId, site.SPObjectId, enableLifecycleManagementForSharePointLists);
                                        var processor = new TeamsSettingFullProcessor(setting, site, setting.SettingTime, baseJobDto, new TeamsLabelUtility(true), teamsId, isSupportLockedSite.Value, siteLifecycleValue);//TO DO setting.SettingTime From Job Message Later
                                        await processor.RunAsync();
                                        SetResultProcessSettingValue(processor);
                                    }
                                }
                            }
                            else if (treeNode.Level == NodeLevel.Office365GroupEntire)
                            {
                                List<SPTreeNodeDto> virtualSiteCollectionNode = RMTeamsTreeService.BrowseTeamsTreeNode(treeNode);
                                if (virtualSiteCollectionNode == null || virtualSiteCollectionNode.Count == 0) continue;
                                List<SPTreeNodeDto> siteCollectionNodes = RMTeamsTreeService.BrowseTeamsTreeNode(virtualSiteCollectionNode[0]);
                                foreach (SPTreeNodeDto site in siteCollectionNodes)
                                {
                                    site.PredictionModeType = predictionModeType;
                                    var siteCollectionSetting = TeamsSettingDao.GetSettingInfoByScope(groupId, teamsId, new Guid(site.SPObjectId), new Guid(site.SPObjectId));
                                    if (siteCollectionSetting != null)
                                    {
                                        logger.Info($"Current site collection is break node so skip");
                                        continue;
                                    }
                                    var siteLifecycleValue = GetLifecycleValue(lifecycleValueByScopeId, site.SPObjectId, enableLifecycleManagementForSharePointLists);
                                    var processor = new TeamsSettingFullProcessor(setting, site, setting.SettingTime, baseJobDto, new TeamsLabelUtility(true), teamsId, isSupportLockedSite.Value, siteLifecycleValue);//TO DO setting.SettingTime From Job Message Later
                                    await processor.RunAsync();
                                    SetResultProcessSettingValue(processor);
                                }
                            }
                            else
                            {
                                var processor = new TeamsSettingFullProcessor(setting, treeNode, setting.SettingTime, baseJobDto, new TeamsLabelUtility(true), teamsId, isSupportLockedSite.Value, enableLifecycleManagementForSharePointLists);//TO DO setting.SettingTime From Job Message Later
                                await processor.RunAsync();
                                SetResultProcessSettingValue(processor);
                            }
                        }
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

        private void SetResultProcessSettingValue(SPSettingFullProcessor processor)
        {
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
        }

        private async Task HandleAddOrUpdateVectorTerm(RMTeamsSetting setting)
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

        private SPTreeNodeDto GetNodeByNodeLevel(SPTreeNodeDto curNode, NodeLevel level)
        {
            var node = curNode;
            while (node.Level != level)
            {
                node = node.Parent;
            }
            return node;
        }

        private SPTreeNodeDto GetSiteCollectionNode(SPTreeNodeDto curNode)
        {
            return GetNodeByNodeLevel(curNode, NodeLevel.SiteCollection);
        }

        private SPTreeNodeDto GetTeamsNode(SPTreeNodeDto curNode)
        {
            return GetNodeByNodeLevel(curNode, NodeLevel.Office365GroupEntire);
        }

        private SPTreeNodeDto GetGroupNode(SPTreeNodeDto curNode)
        {
            return GetNodeByNodeLevel(curNode, NodeLevel.WebApplication);
        }

        public RMTeamsSetting GetParentWebSeting(SPTreeNodeDto node, Guid teamsId, Guid siteId, out NodeLevel nodeLevel)
        {
            RMTeamsSetting teamsSetting = null;

            nodeLevel = node.Level;
            if (node.Level == NodeLevel.WebApplication)
            {
                return TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), Guid.Empty, Guid.Empty, true);
            }

            if (node.Level == NodeLevel.Office365UserContainer)
            {
                teamsSetting = TeamsSettingDao.LoadTeamsSetting(teamsId, teamsId, Guid.Empty);
            }

            if (node.Level == NodeLevel.SiteCollection || node.Level == NodeLevel.Site)
            {
                teamsSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), teamsId, siteId, true);
            }

            if (teamsSetting == null)
            {
                teamsSetting = GetParentWebSeting(node.Parent, teamsId, siteId, out nodeLevel);
            }
            if (node.Level == NodeLevel.List)
            {
                logger.Info("this list node: [{0}] use the node: [{1}] setting, enable related app is: {2}", node.FullPath, teamsSetting.FullPath, teamsSetting.EnableRelatedRecords);
            }
            return teamsSetting;
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
}
