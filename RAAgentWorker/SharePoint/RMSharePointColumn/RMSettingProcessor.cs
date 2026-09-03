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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.SharePoint.Common;
using System;
using System.Collections.Generic;
using AvePoint.RA.Contract.Services;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMSettingProcessor : IScheduleJobWorker
    {
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(RMSettingProcessor));
        public RMSettingProcessor()
        {
        }

        #region for SharePoint Settings Job
        private string currentJobId;
        private int totalCount;
        private int progress = 0;
        private bool hasErrorNode = false;
        private bool mJobHasStopped = false;
        private bool hasSuccessNode = false;
        private bool isAddBCSFailed = false;
        private bool isAddContainerFailed = false;
        private bool isEnablePhysicalFailed = false;
        private bool isEnableAppFailed = false;
        private bool applyTermHasError = false;
        private bool autoApplyTermHasError = false;
        private string errorMessage = string.Empty;
        private ApplySettingJobMessage mMessage;

        //private List<RMSharePointSetting> mAllSettings;

        private void InitCurrentJobInfo()
        {

            //var info = RMSettingsJob.GetRMSettingJob(currentJobId);
            //string jobInfo = Encoding.UTF8.GetString(Convert.FromBase64String(info.JobInfos));
            //jobSettings = SerializerHelper.DeserializeByDataContractSerializer<JobSettings>(jobInfo);
            //totalCount = jobSettings.Nodes.Count;
            //RMJobService.UpdateJobProgress(currentJobId, 1);//
            //JobInfoUpdater.UpdateJobState(currentJobId, (int)JobStatus.InProgress);
            // JobInfoUpdater.UpdateJobProgress(currentJobId, 1);  //使用这个更新子job的进度, 才会级联到主job
        }
        #endregion

        #region Interface
        // private ISPSettingTreeService mSPTreeService;
        //private IJobMonitorService mJobService;
        // private ISharePointSettingDao mSharePointSettingDao;
        //private BaseJobDto baseJobDto;
        //private IJobDetailService mJobDetailService;
        //private IRMSettingJobDao mSettingJobDao;
        // private IRMSubJobDao SubJobDao { set; get; }




        #endregion

        //public RMSharePointColumn RevIMConfig = new RMSharePointColumn();
        private bool IsOnlySetPhysicalLibrary(RMSPTreeNode node)
        {
            if (node.TermId == Guid.Empty && !node.isEnableClassification)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private RMSharePointOnPremiseSetting CloneSetting(RMSharePointOnPremiseSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMSharePointOnPremiseSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMSharePointOnPremiseSetting>(xml);
            return result;
        }

        #region SharePoint Setting Job method group

        /// <summary>
        /// browse选中group node下的所有site collection，并记录site collection的总数
        /// </summary>
        /// <param name="rootNodes"></param>
        /// <param name="nodeCount"></param>
        /// <returns></returns>
        private Dictionary<string, List<RMSPTreeNode>> GetTotalRMSPTreeNode(List<RMSPTreeNode> rootNodes, ref int nodeCount)
        {
            Dictionary<string, List<RMSPTreeNode>> returnMap = new Dictionary<string, List<RMSPTreeNode>>();
            foreach (RMSPTreeNode rootNode in rootNodes)
            {
                List<RMSPTreeNode> childNodes = HybridApiClient.Instance.BrowseSPTreeNode(rootNode);
                if (childNodes != null && childNodes.Count > 0)
                {
                    returnMap.Add(rootNode.SPObjectId, childNodes);
                    nodeCount = nodeCount + childNodes.Count;
                }
                else
                {
                    returnMap.Add(rootNode.SPObjectId, new List<RMSPTreeNode>());
                    nodeCount = nodeCount + 0;
                }
            }
            return returnMap;
        }
        private List<JMGlobalSettingJobDetails> CloneJobDetailsAddSCUrl(List<JMGlobalSettingJobDetails> details)
        {
            List<JMGlobalSettingJobDetails> cloneDetails = new List<JMGlobalSettingJobDetails>();
            foreach (JMGlobalSettingJobDetails detail in details)
            {
                cloneDetails.Add(detail);
            }
            return cloneDetails;
        }

        private int CalculateProgress(int numerator, int denominator, bool isGlobalSetting = false)
        {
            double progressCount = 0;
            if (numerator == denominator)
            {
                progressCount = 99;
            }
            else
            {
                if (isGlobalSetting)
                {
                    progressCount = (double)numerator / (double)denominator * 95 + 5;
                }
                else
                {
                    progressCount = (double)numerator / (double)denominator * 99 + 1;
                }

            }
            return (int)progressCount;
        }

        #endregion

        /// <summary>
        /// Merge 3.5 To Online ApplySPSetting
        /// </summary>
        private void ApplySPSetting(string jobId)
        {
            //using (var scope = new AgentPerformanceScope("RMSettingProcessor.ApplySPSetting"))
            using (var performance = new AgentPerformanceScope("RMSettingProcessor.ApplySPSetting", addToStatistics: true))
            {
                currentJobId = jobId;
                Dictionary<Guid, RMSharePointOnPremiseSetting> gruopSetingMap = null;
                try
                {
                    List<SPTreeNodeDto> treeList = new List<SPTreeNodeDto>();
                    //if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(currentJobId))
                    {
                        //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                        //RMSubJob subJobWithContext = SubJobDao.GetSubJob(currentJobId, true);

                        //var subJobWithContext = SerializerHelper.DeserializeByDataContractSerializer<ApplySettingJobMessage>(jobMessage);
                        //for debug xwwang start
                        //logger.Info("subJobWithContext.JobContext is null:{0}", subJobWithContext.JobContext == null);
                        //logger.Info("subJobWithContext.JobContext.Settings is null:{0}", string.IsNullOrEmpty(subJobWithContext.JobContext.Settings));
                        //logger.Info("subJobWithContext.JobContext.Content is null:{0}", string.IsNullOrEmpty(subJobWithContext.JobContext.Content));
                        //for debug xwwang end

                        List<RMSPTreeNode> tempList = mMessage.TreeNodes;
                        if (mMessage.GroupSettingMapping != null)
                        {
                            gruopSetingMap = mMessage.GroupSettingMapping;
                        }
                        tempList.ForEach(node => treeList.Add(DtoConverter.ConvertRMTree2SPTree(node)));
                        //mAllSettings = subJobWithContext.AllSettings;
                        RMSPSettingUtil.Init(mMessage.AllSettings);
                    }
                    if (treeList != null && treeList.Count > 0)
                    {
                        logger.Info("treenode count is {0}", treeList.Count);
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
                            var gruopId = Guid.Parse(GetGroupNode(treeNode).SPObjectId);

                            if (treeNode.Level != NodeLevel.WebApplication)
                            {
                                siteId = new Guid(GetSiteCollectionNode(treeNode).SPObjectId);
                            }

                            RMSharePointOnPremiseSetting setting = RMSPSettingUtil.GetSettingInfoByScope(gruopId, siteId, new Guid(treeNode.SPObjectId));
                            logger.Info("get sp setting for node, id:{0}, setting is null:{1}", treeNode.SPObjectId, setting == null);

                            if (setting == null)
                            {
                                if (gruopSetingMap != null && gruopSetingMap.ContainsKey(gruopId))
                                {
                                    setting = gruopSetingMap[gruopId];
                                }
                                else
                                {
                                    // run group level setting ,get group setting
                                    setting = RMSPSettingUtil.GetSettingInfoByScope(gruopId, Guid.Empty, gruopId);
                                    if (setting == null)
                                    {
                                        logger.Warn("Setting not available {0}", treeNode.FullPath.LogBase64());
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                if (treeNode.Level == NodeLevel.List)
                                {
                                    var parentSetting = GetParentWebSeting(treeNode, siteId);
                                    setting.EnableRelatedRecords = parentSetting.EnableRelatedRecords;
                                }
                            }

                            if (treeNode.Level == NodeLevel.WebApplication)
                            {
                                List<SPTreeNodeDto> spNodes = HybridApiClient.Instance.BrowseSPTreeNode(DtoConverter.ConvertSPTree2RMTree(treeNode)).ConvertAll(n => DtoConverter.ConvertRMTree2SPTree(n));
                                foreach (SPTreeNodeDto siteCollection in spNodes)
                                {
                                    var processor = new SPSettingFullProcessor(setting, siteCollection, setting.SettingTime);//TO DO setting.SettingTime From Job Message Later
                                    processor.Run();
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
                                    hasSuccessNode = processor.HasSuccessfulNode;
                                    hasErrorNode = processor.HasFailedNode;
                                    //processor.RunUpdateJobDetails();
                                    //RunUpdateJobDetails(jobDetails);
                                }
                            }
                            else
                            {
                                var processor = new SPSettingFullProcessor(setting, treeNode, setting.SettingTime);//TO DO setting.SettingTime From Job Message Later
                                processor.Run();
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
                                hasSuccessNode = processor.HasSuccessfulNode;
                                hasErrorNode = processor.HasFailedNode;
                                //processor.RunUpdateJobDetails();
                                //RunUpdateJobDetails(jobDetails);
                            }
                        }
                        //UpdateSettingJobStatus();
                    }
                }
                //catch (JobStopException ex)
                //{
                //    mJobHasStopped = true;
                //}
                catch (Exception ex)
                {
                    logger.Error("error occurred while apply SharePoint settings: {0}", ex.ToString());
                }
                finally
                {
                    //if (mJobHasStopped)
                    //{
                    //    ReportManager.SetJobFinished(JobStatus.Stopped);
                    //}
                    //else
                    try
                    {
                        JobContext.Current.Cleanup();
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
                    }

                    if (hasSuccessNode && hasErrorNode)
                    {
                        //ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_TS_SS_Summary");
                        HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.FinishWithException, "RM_TS_SS_Summary");
                    }
                    else if (!hasErrorNode)
                    {
                        if (isAddBCSFailed || isAddContainerFailed || isEnablePhysicalFailed || isEnableAppFailed || applyTermHasError || autoApplyTermHasError)
                        {
                            //ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_TS_SS_Summary");
                            HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.FinishWithException, "RM_TS_SS_Summary");
                        }
                        else
                        {
                            //ReportManager.SetJobFinished(JobStatus.Finished);
                            HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.Finished, "");
                        }
                    }
                    else if (!hasSuccessNode)
                    {
                        //ReportManager.SetJobFinished(JobStatus.Failed, "RM_TS_SS_Summary");
                        HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.Failed, "RM_TS_SS_Summary");
                    }
                    else
                    {
                        //ReportManager.SetJobFinished(JobStatus.Skipped, "RM_SS_JobSkip");
                        HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.Skipped, "");
                    }

                }
            }
        }




        //public RMSharePointSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        //{
        //    return mAllSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId).FirstOrDefault();
        //}
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

        public RMSharePointOnPremiseSetting GetParentWebSeting(SPTreeNodeDto node, Guid siteId)
        {
            RMSharePointOnPremiseSetting spSetting = null;

            if (node.Level == NodeLevel.WebApplication)
            {
                return RMSPSettingUtil.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty, true);
            }

            if (node.Level == NodeLevel.SiteCollection || node.Level == NodeLevel.Site)
            {
                spSetting = RMSPSettingUtil.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }

            if (spSetting == null)
            {
                spSetting = GetParentWebSeting(node.Parent, siteId);
            }
            if (node.Level == NodeLevel.List)
            {
                logger.Info("this list node: [{0}] use the node: [{1}] setting, enable related app is: {2}", node.Title.LogBase64(), spSetting.ScopeId, spSetting.EnableRelatedRecords);
            }
            return spSetting;
        }

        public void Bind(string msg)
        {
            mMessage = SerializerHelper.DeserializeByDataContractSerializer<ApplySettingJobMessage>(msg);
        }

        public void Run()
        {
            ApplySPSetting(JobContext.Current.JobId);

        }

        //public RMSharePointSetting LoadSharePointSetting(Guid id, Guid siteId, bool includeOnlySetPhysicalNode = false)
        //{
        //    //using (var context = GetNewContext())
        //    {
        //        RMSharePointSetting spSetting = null;
        //        if (siteId != Guid.Empty)
        //        {
        //            spSetting = mAllSettings.Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
        //            //当TermId为空时，代表该节点只设置了“Mark the Physical Library”，并没有设置Custom Setting所以返回null.
        //            if (!includeOnlySetPhysicalNode
        //                && spSetting != null
        //                && spSetting.TermId == Guid.Empty && spSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
        //            {
        //                spSetting = null;
        //            }
        //        }
        //        if (spSetting == null)
        //        {
        //            //add this for RA 3.1 old data.
        //            spSetting = mAllSettings.Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
        //        }
        //        return spSetting;
        //    }
        //}
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
    //    #region init treenode for WebAPI
    //    if (jobSettings.IsWebAPI)
    //    {
    //        bool isEnableHoldPhyical = jobSettings.Nodes[0].IsEnableHoldPhyical;
    //        RMSPTreeNode parent = jobSettings.Nodes[0].Parent;
    //        RMSPTreeNode settingNode = new RMSharePointColumn().GetCustomSettingsNode(jobSettings.Nodes[0].FullPath, jobSettings.Nodes[0].BposInfo.UserAccountInfo.Username, jobSettings.Nodes[0].BposInfo.UserAccountInfo.Password, ref parent);
    //        settingNode.Parent = parent;
    //        settingNode.BposInfo = jobSettings.Nodes[0].BposInfo;
    //        settingNode.IsEnableHoldPhyical = isEnableHoldPhyical;
    //        if (settingNode.Level == (int)NodeLevel.SiteCollection)
    //        {
    //            settingNode.SPObjectId = jobSettings.Nodes[0].SPObjectId;
    //        }
    //        parent.BposInfo = jobSettings.Nodes[0].BposInfo;
    //        jobSettings.Nodes[0] = settingNode;
    //    }
    //    #endregion
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
