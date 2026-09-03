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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.RMCollectionData;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Microsoft.Exchange.WebServices.Data;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal
{
    public class RMEXOEnforceRuleActionProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXOEnforceRuleActionProcessor));
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        #region interface
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMSubJobDao mSubJobDao { set; get; }
        public IRMSubJobDao SubJobDao
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

        private IEXOSettingDao mEXOSettingDao { set; get; }
        public IEXOSettingDao EXOSettingDao
        {
            get
            {
                if (mEXOSettingDao == null)
                {
                    mEXOSettingDao = (IEXOSettingDao)PlatformWindsorManager.GetService(typeof(IEXOSettingDao));
                }
                return mEXOSettingDao;
            }
        }        
        #endregion
        public RMEXOEnforceRuleActionProcessor()
        {
            
        }
        public void RunNow(string subJobId)
        {
            //allRecordsRules = RuleManagerService.GetRulesFromDA();
            using (var performance = new PerformanceScope("RMEXOEnforceRuleActionProcessor.RunNow", "", true))
            {
                var mainJobId = subJobId?.Split('_')?.FirstOrDefault() ?? subJobId;
                
                CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
                {
                    MainJobId = mainJobId,
                    SubJobId = subJobId,
                    JobType = JobType.EXORecordsDisposal
                });
                CompoundDisposalStatistics.Instance.StartStatistic();
                JobManagement jm = JobManagement.GetInstance(subJobId, JobType.EXORecordsDisposal);
                try
                {
                    RMSubJob subJobWithContext = SubJobDao.GetSubJob(subJobId, true);
                    EXOEnforceRuleActionStatisticStore.BeginSubJob(subJobId, mainJobId, subJobWithContext.String1);
                    ArchiverCommonStaticMethod.InitExchangeOnlineSetting();
                    List<ExchangeOnlineTreeNodeDto> nodes = new List<ExchangeOnlineTreeNodeDto>();

                    //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater

                    List<RMEXOTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMEXOTreeNode>>(subJobWithContext.JobContext.Settings);
                    var tempNode = tempList.FirstOrDefault();
                    if (tempNode != null)
                    {
                        WrapperConfiguration.IsProcessApprovalDatasOnly = tempNode.IsProcessApprovalDatasOnly;
                        WrapperConfiguration.IsRecheckRule = true;
                        logger.Info($"this job is process approval data job?{WrapperConfiguration.IsProcessApprovalDatasOnly}.");
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            var isRecheckRule = FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule).GetAwaiter().GetResult();
                            if (!string.IsNullOrEmpty(isRecheckRule))
                            {
                                bool result = Convert.ToBoolean(isRecheckRule);
                                WrapperConfiguration.IsRecheckRule = result;
                            }
                            else
                            {
                                WrapperConfiguration.IsRecheckRule = true;//the old setting need to check rule
                            }
                            logger.Info($"current is recheck rule status is :{WrapperConfiguration.IsRecheckRule}");
                        }
                    }
                    foreach (var tempnode in tempList)
                    {
                        if (tempnode.Parent != null && tempnode.Parent.SkipRemoveContentAndDestroyAction)
                        {
                            tempnode.SkipRemoveContentAndDestroyAction = true;
                        }
                    }
                    tempList.ForEach(node => nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node)));

                    if (nodes == null || nodes.Count == 0)
                    {
                        logger.Error("Tree node is null.");
                        return;
                    }


                    foreach (var node in nodes)
                    {
                        using (new PerformanceScope("EnforceRuleActionProcessExoNode", $"EnforceRuleActionProcessExoNode{node.ID}", true))
                        {
                            logger.Info($"Process node : {node?.ObjectId}, node id : {node.ID}.");
                            try
                            {
                                RMEXOEnforceRuleActionBase enforceRuleActionBase = GenerateEnforceRuleActionObject(node, jm);
                                enforceRuleActionBase.Scan();
                                enforceRuleActionBase.Archive();
                            }
                            catch (ServiceRequestException ex)
                            {
                                jm.HasErrorNode = true;
                                var comment = ex.ToString();
                                if (ex.Message.Contains("401"))
                                {
                                    comment = "RM_JS_Common_PasswordError";
                                }
                                EXOCommonUtil.AddDetail(node.Level, node.Name, node.EmailAddress + "\\" + node.FullPath, "", "",
                                    Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_EXODisposal_Action_Scan", comment);
                                logger.Error($"Error in process node {node.Name}, reason : {ex.ToString()}.");
                            }
                            catch (JobStopException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message == "243")
                                {
                                    logger.Error($"[DirtyData] Mailbox: {node.Name}, id: {node.ID} is deleted, ErrorCode:[{ex.Message}]");
                                    return;
                                }

                                jm.HasErrorNode = true;
                                EXOCommonUtil.AddDetail(node.Level, node.Name, node.EmailAddress + "\\" + node.FullPath, "", "",
                                    Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_EXODisposal_Action_Scan", ex.InnerException?.Message ?? ex.Message);
                                logger.Error($"Error in process node {node.Name}, reason : {ex.ToString()}.");
                            }
                            finally
                            {
                                EXOLiteDBWrapper.CreateInstance(EXOPathUtil.GetDisposalDueRecordDBPath(jm.SubJobId)).DeleteDBFile();
                            }
                        }
                    }
                }
                catch(JobStopException ex){
                    logger.Warn($"Job stop {ex}");
                    jm.JobHasStopped = true;
                }
                catch (Exception exception)
                {
                    logger.Error($"Error in job level, reason : {exception.ToString()}");
                    jm.HasErrorNode = true;
                    EXOCommonUtil.AddDetail(NodeLevel.Undefined, "", "", "", "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "", exception.ToString());
                }
                finally
                {
                    EXOCommonUtil.AddJobSummaryStatistic();
                    EXOEnforceRuleActionStatisticStore.CompleteSubJob(mainJobId, subJobId);
                    CompoundDisposalStatistics.Instance.PrepareEndStatistic();          
                    CosmosDBManualDataUpdater.WaitComplete();
                    CompoundDisposalStatistics.Instance.WaitEndStatistic();
                    jm.Finish();
                }
            }
        }
      
        private RMEXOEnforceRuleActionBase GenerateEnforceRuleActionObject(ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement)
        {
            var groupId = new Guid(TreeManagement.GetGroupNode(treeNode).ID);
            bool isNullClassification = CheckIsNullClassificationSetting(treeNode, groupId);
            var discoverType = isNullClassification || !WrapperConfiguration.IsRecheckRule ? EXODiscoverType.Full : EXODiscoverType.Search;
            RMEXOEnforceRuleActionBase enforceRuleActionBase = new RMEXOEnforceRuleActionBase(treeNode, jobManagement, isNullClassification);
            var factory = EXODiscoverFactoryV2.CreateFactory(discoverType, NodeFlagType.ExplorerSync, groupId, null);
            enforceRuleActionBase.SetDiscoverObject(factory);
            return enforceRuleActionBase;
        }

        private bool CheckIsNullClassificationSetting(ExchangeOnlineTreeNodeDto treeNode, Guid groupId)
        {
            bool isNullClassificationSetting = false;
            RMExchangeOnlineSetting currentNodeTermSetting = null;
            if (treeNode.Level == NodeLevel.ExchangeOnlineMailbox)
            {
                currentNodeTermSetting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, new Guid(treeNode.ID));
            }
            if (treeNode.IsNullClassificationSetting)
            {
                if (currentNodeTermSetting == null)
                {
                    isNullClassificationSetting = true;
                }
                else if (currentNodeTermSetting != null && currentNodeTermSetting.TermSetId == Guid.Empty)
                {
                    isNullClassificationSetting = true;
                }
            }
            return isNullClassificationSetting;
        }
    }
}
