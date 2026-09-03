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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.RAExchange.Disposal.Common;
using Microsoft.Exchange.WebServices.Data;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.RMCollectionData
{
    public class RMEXOSyncDataProcesser
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXOSyncDataProcesser));

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        protected bool IsSupportGraphApi { get; set; }

        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleManagerService
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

        private IRMSubJobDao mSubJobDao { set; get; }
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if(mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }

        private IEXONodeFlagDao _rmEXONodeFlagDao;
        public IEXONodeFlagDao EXONodeFlagDao
        {
            get
            {
                if (_rmEXONodeFlagDao == null)
                {
                    _rmEXONodeFlagDao = new EXONodeFlagDao();
                }
                return _rmEXONodeFlagDao;
            }
        }

        private IEXONodeFlagDao mEXONodeInfoDao;
        protected IEXONodeFlagDao EXONodeInfoDao
        {
            get
            {
                if (mEXONodeInfoDao == null)
                {
                    mEXONodeInfoDao = new EXONodeFlagDao();
                }
                return mEXONodeInfoDao;
            }
        }

        public async System.Threading.Tasks.Task RunNowAsync(string subJobId)
        {
            //allRecordsRules = RuleManagerService.GetRulesFromDA();
            using (var performance = new PerformanceScope("EXO.RMEXOSyncData.RunNow", "", true))
            {
                CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
                {
                    MainJobId = subJobId.Split('_')[0],
                    SubJobId = subJobId,
                    JobType = JobType.EXODataSynchronisation
                });
                CompoundDisposalStatistics.Instance.StartStatistic();

                JobManagement jm = JobManagement.GetInstance(subJobId, JobType.EXODataSynchronisation);
                try
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        List<ExchangeOnlineTreeNodeDto> nodes = new List<ExchangeOnlineTreeNodeDto>();
                        if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(subJobId))
                        {
                            //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                            RMSubJob subJobWithContext = SubJobDao.GetSubJob(subJobId, true);
                            List<RMEXOTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMEXOTreeNode>>(subJobWithContext.JobContext.Settings);
                            tempList.ForEach(node => nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node)));
                        }
                        if (nodes != null && nodes.Count > 0)
                        {
                            foreach (var node in nodes)
                            {
                                using (new PerformanceScope("SyncDataProcessExoNode", $"ProcessExoNode{node.ID}", true))
                                {
                                    logger.Info($"Process node : {node?.ObjectId}, node id : {node.ID}.");
                                    try
                                    {
                                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                                        {
                                            //setting = GetEXOSetting(node);
                                            IEXOSyncDataJob dataSyncBase = null;
                                            using (new PerformanceScope("GenerateApplySettingObject", $"GenerateApplySettingObject{node.ID}", true))
                                            {
                                                dataSyncBase = ApplySetting(node, jm);
                                            }
                                            await dataSyncBase.RunNowAsync();
                                        }
                                    }
                                    catch (JobStopException)
                                    {
                                        logger.Info("Job Stopped");
                                        jm.JobHasStopped = true;
                                        throw new JobStopException("This Job is stopped.");
                                    }
                                    catch (ServiceRequestException ex)
                                    {
                                        jm.HasErrorNode = true;
                                        var comment = ex.Message;
                                        if (ex.Message.Contains("401"))
                                        {
                                            comment = "RM_JS_Common_PasswordError";
                                        }
                                        jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
                                        {
                                            ObjectName = node.Name,
                                            FullPath = node.EmailAddress + "\\" + node.FullPath,
                                            ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                            Comment = EXOCommonUtil.ProcessJobDetailMessage(comment, JobDetailsStatus.Failed),
                                            Status = EXOCommonUtil.ProcessJobDetailStatus(comment, JobDetailsStatus.Failed),
                                        });
                                        logger.Error($"Error in process node {node.Name}, reason : {ex.ToString()}.");
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.Message == "243")
                                        {
                                            logger.Error($"[DirtyData] Mailbox: {node.Name}, id: {node.ID} is deleted, ErrorCode:[{ex.Message}]");
                                            return;
                                        }

                                        jm.HasErrorNode = true;
                                        jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
                                        {
                                            ObjectName = node.Name,
                                            FullPath = node.EmailAddress + "\\" + node.FullPath,
                                            ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                            Comment = EXOCommonUtil.ProcessJobDetailMessage(ex.Message, JobDetailsStatus.Failed),
                                            Status = EXOCommonUtil.ProcessJobDetailStatus(ex.Message, JobDetailsStatus.Failed),
                                        });
                                        logger.Error($"Error in process node {node.Name}, reason : {ex.ToString()}.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            logger.Info("Tree node is null.");
                        }
                    }   
                }
                catch (JobStopException)
                {
                    logger.Info("Job Stopped");
                    jm.JobHasStopped = true;
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception exception)
                {
                    logger.Error($"Error in job level, reason : {exception.ToString()}");
                    jm.HasErrorNode = true;
                    jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
                    {
                        Comment = EXOCommonUtil.ProcessJobDetailMessage(exception.Message, JobDetailsStatus.Failed),
                        Status = EXOCommonUtil.ProcessJobDetailStatus(exception.Message, JobDetailsStatus.Failed),
                    });
                }
                finally
                {
                    CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                    CompoundDisposalStatistics.Instance.WaitEndStatistic();
                    jm.Finish();
                }
            }
        }
        //should change a common name method
        private IEXOSyncDataJob ApplySetting(ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement)
        {
            IEXOSyncDataJob syncDataJob = null;
            var groupId = new Guid(TreeManagement.GetGroupNode(treeNode).ID);
            var nodeId = new Guid(TreeManagement.GetMailboxNode(treeNode).ID);
            var exoNodeInfo = GetEXONodeInfo(nodeId, groupId, treeNode);
            var discoverType = EXODiscoverType.Search;
            ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
            SearchFilter searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
            if (exoNodeInfo != null)
            {
                logger.Info("Discover Type is incremental.");
            }
            else
            {
                logger.Info("Discover Type is Search full.");
              
                //created time  00020329-0000-0000-C000-000000000046 0x0040

                //modified PidTagLastModificationTime
                // Guid guid = new Guid("00062008-0000-0000-C000-000000000046");
                //ExtendedPropertyDefinition modifiedPropertyDefinition = new ExtendedPropertyDefinition(guid, 0x3008, MapiPropertyType.SystemTime);
                //DateTime t = DateTime.UtcNow.AddDays(-100);
                //var modifiedFileter = new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.LastModifiedTime, t);

                //SearchFilter.SearchFilterCollection collection = new SearchFilter.SearchFilterCollection();
                //collection.LogicalOperator = LogicalOperator.And;
                //collection.Add(columnFilter);
                //collection.Add(modifiedFileter);
                //searchFilter = collection;
            }

            TreeManagement tm = new TreeManagement();
            var mailboxAddress = TreeManagement.GetMailboxNode(treeNode)?.Name;
            var isSupportGraphApi = EXOGraphApiResolver.ShouldUseGraph(_keyValueDao, mailboxAddress, tm.GetRealMailboxStringId(treeNode), treeNode);
            IsSupportGraphApi = isSupportGraphApi;

            if (!IsSupportGraphApi)
            {
                syncDataJob = new RMCollectionData.RMEXOSyncDataJobBase(treeNode, jobManagement);
                RMEXODiscoverHelper help = new RMEXODiscoverHelper();
                var factory = EXODiscoverFactory.CreateFactory(help, discoverType, NodeFlagType.ExplorerSync, groupId, searchFilter);
                (syncDataJob as RMEXOSyncDataJobBase).SetDiscoverObject(help, factory);
            }
            else
            {
                syncDataJob = new RMEXOSyncDataJobBaseV2(treeNode, jobManagement);
                var factory = EXODiscoverFactoryV2.CreateFactory(discoverType, NodeFlagType.ExplorerSync, groupId, searchFilter);
                (syncDataJob as RMEXOSyncDataJobBaseV2).SetDiscoverObject(factory);
            }
            return syncDataJob;
        }

        /// <summary>
        /// 1.兼容旧数据升级逻辑，先用DAOTreeNodeID获取DB中旧记录，如果有则用旧记录并删除旧记录.
        /// 2.新数据，直接通过AOSMailboxID和AOSObjectId取对应Mailbox记录
        /// </summary>
        protected EXONodeFlag GetEXONodeInfo(Guid mailboxId, Guid groupId, ExchangeOnlineTreeNodeDto treeNode)
        {
            TreeManagement treeManagement = new TreeManagement();
            string AOSObjectId = treeManagement.GetAOSObjectId(treeNode);
            string AOSMailboxId = treeManagement.GetRealMailboxGuid(treeNode);
            var mAOSEXONodeFlag = EXONodeInfoDao.GetEXONodeInfoByAOSMailboxIdAndObjectId(new Guid(AOSMailboxId), groupId, (int)NodeFlagType.ExplorerSync, AOSObjectId);
            if (mAOSEXONodeFlag != null)
            {
                DateTime collectionTime = new DateTime(mAOSEXONodeFlag.CollectionTime);
                logger.Info($"Current get CollectionTime:{collectionTime} by AOSMailboxId when EXO sync data processer.AOSMailboxId:{AOSMailboxId}.DAOTreeNodeID:{mailboxId}.groupId:{groupId}.AOSObjectId:{AOSObjectId}.");
                return mAOSEXONodeFlag;
            }
            else
            {
                var mEXONodeFlag = EXONodeInfoDao.GetEXONodeInfo(mailboxId, groupId, (int)NodeFlagType.ExplorerSync);
                if (mEXONodeFlag != null)
                {
                    DateTime collectionTime = new DateTime(mEXONodeFlag.CollectionTime);
                    logger.Info($"Current get CollectionTime by DAOTreeNodeID when EXO sync data processer.CollectionTime:{collectionTime}.DAOTreeNodeID:{mailboxId}.groupId:{groupId}.");
                    //EXONodeFlagDao.DeleteEXONodeInfo(new Guid(mailboxId), groupId, (int)NodeFlagType.EnforceRetention);
                    return mEXONodeFlag;
                }
                else
                {
                    logger.Info($"Current CollectionTime can not be get by DAOTreeNodeID & AOSMailboxId when EXO sync data processer.AOSMailboxId:{AOSMailboxId}.groupId:{groupId}.AOSObjectId:{AOSObjectId}.");
                    return null;
                }
            }
        }
    }
}
