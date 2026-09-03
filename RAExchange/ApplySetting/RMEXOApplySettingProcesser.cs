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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
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

namespace AvePoint.RA.RAExchange.ApplySetting
{
    public class RMEXOApplySettingProcesser
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXOApplySettingProcesser));
        private Dictionary<Guid, RMExchangeOnlineSetting> gruopSetingMap = null;
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMSubJobDao SubJobDao { set; get; }

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

        private IEXOSettingDao _exoSettingDao;
        protected IEXOSettingDao EXOSettingDao
        {
            get
            {
                if (_exoSettingDao == null)
                {
                    _exoSettingDao = new EXOSettingDao();
                }
                return _exoSettingDao;
            }
        }

        private IJobMonitorDao _jobMonitorDao;
        public IJobMonitorDao JobMonitorDao
        {
            get
            {
                if (_jobMonitorDao == null)
                {
                    _jobMonitorDao = new JobMonitorDao();
                }
                return _jobMonitorDao;
            }
        }

        public async System.Threading.Tasks.Task RunNowAsync(string subJobId)
        {
            using (var performance = new PerformanceScope("EXO.RMEXOApplySettingProcesser.RunNow", "", true))
            {
                CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
                {
                    MainJobId = subJobId.Split('_')[0],
                    SubJobId = subJobId,
                    JobType = JobType.EXOApplySetting
                });
                CompoundDisposalStatistics.Instance.StartStatistic();

                JobManagement jm = JobManagement.GetInstance(subJobId, JobType.EXOApplySetting);
                long mainJobStartTime = DateTime.MinValue.Ticks;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                        List<ExchangeOnlineTreeNodeDto> nodes = new List<ExchangeOnlineTreeNodeDto>();
                        Dictionary<Guid, RMExchangeOnlineSetting> settings = new Dictionary<Guid, RMExchangeOnlineSetting>();
                        if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(subJobId))
                        {
                            //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                            RMSubJob subJobWithContext = SubJobDao.GetSubJob(subJobId, true);
                            mainJobStartTime = JobMonitorDao.GetJob(subJobWithContext.ParentId).StartTime;
                            List<RMEXOTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMEXOTreeNode>>(subJobWithContext.JobContext.Settings);
                            settings = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<Guid, RMExchangeOnlineSetting>>(subJobWithContext.JobContext.Content);
                            tempList.ForEach(node => nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node)));
                        }

                        if (nodes != null && nodes.Count > 0)
                        {
                            foreach (var node in nodes)
                            {
                                using (new PerformanceScope("ProcessExoNode", $"ProcessExoNode{node.ID}", true))
                                {
                                    using (CheckJobStopScope checkJobStopScope = new CheckJobStopScope()) 
                                    {
                                        logger.Info($"Process node : {node?.ObjectId}, node id : {node.ID}.");
                                        RMExchangeOnlineSetting setting = null;
                                        try
                                        {
                                            setting = GetEXOSetting(node, settings);
                                            if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && (DeployTermMethod)setting.DeployTermMethod != DeployTermMethod.UseAutoClassification)
                                            {
                                                logger.Info($"Job setting is {setting.DeployTermMethod.ToString()}, No need to run job.");
                                                jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                                                {
                                                    ObjectName = node.Name,
                                                    FullPath = node.Name,
                                                    ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                                    Comment = "RM_JM_EXO_ApplySetting_SaveSettingFinish",
                                                    Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful,
                                                    Action = GetDetailAction(node),
                                                    Classification = string.Empty
                                                });
                                            }
                                            else
                                            {
                                                logger.Info($"Job setting is {setting.DeployTermMethod.ToString()}, run apply setting job.");
                                                TreeManagement tm = new TreeManagement();
                                                var mailboxAddress = TreeManagement.GetMailboxNode(node)?.Name;
                                                var isSupportGraphApi = await EXOGraphApiResolver.ShouldUseGraphAsync(_keyValueDao, mailboxAddress, tm.GetRealMailboxStringId(node));

                                                using (new PerformanceScope("GenerateApplySettingObject", $"GenerateApplySettingObject{node.ID}", true))
                                                {
                                                    if (isSupportGraphApi)
                                                    {
                                                        RMEXOApplySettingBaseV2 applySettingBaseV2 = GenerateApplySettingObjectV2(setting, node, jm);
                                                        applySettingBaseV2.RunNow();
                                                    }
                                                    else
                                                    {
                                                        RMEXOApplySettingBase applySettingBase = GenerateApplySettingObject(setting, node, jm);
                                                        applySettingBase.RunNow();
                                                    }
                                                }
                                            }
                                        }
                                        catch (ServiceRequestException ex)
                                        {
                                            jm.HasErrorNode = true;
                                            var comment = ex.Message;
                                            if (ex.Message.Contains("401"))
                                            {
                                                comment = "RM_JS_Common_PasswordError";
                                            }
                                            jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                                            {
                                                ObjectName = node.Name,
                                                FullPath = node.Name,
                                                ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                                Comment = EXOCommonUtil.ProcessJobDetailMessage(comment, JobDetailsStatus.Failed),
                                                Status = EXOCommonUtil.ProcessJobDetailStatus(ex.Message, JobDetailsStatus.Failed),
                                                Action = GetDetailAction(node),
                                                Classification = string.Empty
                                            });
                                            logger.Error($"Error in process node:{node.ID}, reason : {ex.ToString()}.");
                                        }
                                        catch (JobStopException)
                                        {
                                            throw;
                                        }
                                        catch (Exception ex)
                                        {
                                            jm.HasErrorNode = true;
                                            jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                                            {
                                                ObjectName = node.Name,
                                                FullPath = node.Name,
                                                ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                                Comment = EXOCommonUtil.ProcessJobDetailMessage(ex.Message, JobDetailsStatus.Failed),
                                                Status = EXOCommonUtil.ProcessJobDetailStatus(ex.Message, JobDetailsStatus.Failed),
                                                Action = GetDetailAction(node),
                                                Classification = string.Empty
                                            });
                                            logger.Error($"Error in process node:{node.ID}, reason : {ex.ToString()}.");
                                        }
                                        try
                                        {
                                            if (setting != null)
                                            {
                                                //如果当前setting记录的更新时间在主Job 起来之后，并且setting值是0 ，表示当前主Job 的子job有失败的。那么就不允许更新节点信息了
                                                if (setting.UpdateDate > mainJobStartTime && setting.SettingTime == 0)
                                                {
                                                    logger.Info($"Update data : {setting.UpdateDate} is large the job start time : {mainJobStartTime}, and setting time is 0, skip update the setting time");
                                                }
                                                else
                                                {
                                                    if (jm.HasErrorNode)
                                                    {
                                                        //此处强行更新成false， 有问题客户需要手动更改GUI 变成true 执行full 操作
                                                        await EXOSettingDao.SetSettingInfoAsync(setting.ScopeId, 0, false);
                                                    }
                                                    else
                                                    {
                                                        await EXOSettingDao.SetSettingInfoAsync(setting.ScopeId, DateTime.UtcNow.Ticks, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                logger.Warn("Exo setting is null, no need to update setting info.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Warn("Update status error {0}.", e.ToString());
                                        }
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
                catch (JobStopException ex)
                {
                    logger.Warn($"Job stop {ex}");
                    jm.JobHasStopped = true;
                }
                catch (Exception exception)
                {
                    logger.Error($"Error in job level, reason : {exception.ToString()}");
                    jm.HasErrorNode = true;
                    jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
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

        private RMEXOApplySettingBase GenerateApplySettingObject(RMExchangeOnlineSetting setting, ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement)
        {
            RMEXOApplySettingBase applySettingBase;
            ThrowUtil.ThrowIfNull(setting, "Exchange setting is null.");
            EXODiscoverType discoverType = EXODiscoverType.Search;
            //如果页面EnableRecordManagement 勾选了No， 需要跑Full job，然后remove掉所有extent properties， 并且remove掉DB 中的EXOFlag 记录，保证后续的第一个apply setting 能跑full
            SearchFilter searchFilter = null;
            ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
            if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                discoverType = EXODiscoverType.Search;
                searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
            }
            else
            {
                if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                {
                    if (setting.RunAutoFullJob)
                    {
                        if (setting.AutoJobOption == (int)AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override)
                        {
                            discoverType = EXODiscoverType.Full;
                        }
                        else
                        {
                            //没勾选overwrite，使用filter查出没有term的数据
                            searchFilter = new SearchFilter.Not(new SearchFilter.Exists(extendedPropertyDefinition));
                        }
                    }
                    else
                    {
                        //使用auto,没勾选scan all，可以看下是否勾选overwrite，如果勾选了overwrite。根据modified时间生成filter查询数据。 如果没勾选overwrite，根据modified时间和无termid filter生成filter
                        //discover folder时根据folder collection time生成此filter
                        if (setting.AutoJobOption != (int)AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override)
                        {
                            searchFilter = new SearchFilter.Not(new SearchFilter.Exists(extendedPropertyDefinition));
                        }
                    }
                }
                else
                {
                    throw new Exception($"Job setting is {setting.DeployTermMethod.ToString()}, No need to run job.");
                }
            }
            logger.Info($"Discover Type is : {discoverType.ToString()}.");
            applySettingBase = new RMEXOApplySettingBase(setting, treeNode, jobManagement);

            Guid groupId = Guid.Empty;

            switch (discoverType)
            {
                case EXODiscoverType.Full:
                    break;
                case EXODiscoverType.Incremental:
                    groupId = Guid.Parse(TreeManagement.GetGroupNode(treeNode).ID);
                    break;
                case EXODiscoverType.Search:
                    //ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                    //searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
                    break;
                default:
                    throw new Exception("Unknow discover type.");
            }
            RMEXODiscoverHelper help = new RMEXODiscoverHelper();
            var factory = EXODiscoverFactory.CreateFactory(help, discoverType, NodeFlagType.AutoClassification, groupId, searchFilter);
            applySettingBase.SetDiscoverObject(help, factory);
            return applySettingBase;
        }
        private RMEXOApplySettingBaseV2 GenerateApplySettingObjectV2(RMExchangeOnlineSetting setting, ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement)
        {
            RMEXOApplySettingBaseV2 applySettingBase;
            ThrowUtil.ThrowIfNull(setting, "Exchange setting is null.");
            EXODiscoverType discoverType = EXODiscoverType.Search;
            //如果页面EnableRecordManagement 勾选了No， 需要跑Full job，然后remove掉所有extent properties， 并且remove掉DB 中的EXOFlag 记录，保证后续的第一个apply setting 能跑full
            SearchFilter searchFilter = null;
            ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
            if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                discoverType = EXODiscoverType.Search;
                searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
            }
            else
            {
                if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                {
                    if (setting.RunAutoFullJob)
                    {
                        if (setting.AutoJobOption == (int)AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override)
                        {
                            discoverType = EXODiscoverType.Full;
                        }
                        else
                        {
                            //没勾选overwrite，使用filter查出没有term的数据
                            searchFilter = new SearchFilter.Not(new SearchFilter.Exists(extendedPropertyDefinition));
                        }
                    }
                    else
                    {
                        //使用auto,没勾选scan all，可以看下是否勾选overwrite，如果勾选了overwrite。根据modified时间生成filter查询数据。 如果没勾选overwrite，根据modified时间和无termid filter生成filter
                        //discover folder时根据folder collection time生成此filter
                        if (setting.AutoJobOption != (int)AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override)
                        {
                            searchFilter = new SearchFilter.Not(new SearchFilter.Exists(extendedPropertyDefinition));
                        }
                    }
                }
                else
                {
                    throw new Exception($"Job setting is {setting.DeployTermMethod.ToString()}, No need to run job.");
                }
            }
            logger.Info($"Discover Type is : {discoverType.ToString()}.");
            applySettingBase = new RMEXOApplySettingBaseV2(setting, treeNode, jobManagement);

            Guid groupId = Guid.Empty;

            switch (discoverType)
            {
                case EXODiscoverType.Full:
                    break;
                case EXODiscoverType.Incremental:
                    groupId = Guid.Parse(TreeManagement.GetGroupNode(treeNode).ID);
                    break;
                case EXODiscoverType.Search:
                    //ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                    //searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
                    break;
                default:
                    throw new Exception("Unknow discover type.");
            }
            var factory = EXODiscoverFactoryV2.CreateFactory(discoverType, NodeFlagType.AutoClassification, groupId, searchFilter);
            applySettingBase.SetDiscoverObject(factory);
            return applySettingBase;
        }


        private RMExchangeOnlineSetting GetEXOSetting(ExchangeOnlineTreeNodeDto treeNode, Dictionary<Guid, RMExchangeOnlineSetting> settings)
        {
            using (var performance = new PerformanceScope("EXO.RMEXOApplySettingProcesser.GetEXOSetting"))
            {
                var mailboxId = Guid.Empty;
                var scopeId = Guid.Parse(treeNode.ID);
                var groupId = Guid.Parse(TreeManagement.GetGroupNode(treeNode).ID);
                if (treeNode.Level != NodeLevel.ExchangeOnlineMailbox && treeNode.Level != NodeLevel.ExchangeOnlineO365Group)
                {
                    mailboxId = new Guid(TreeManagement.GetMailboxNode(treeNode).ID);
                }
                #region Get Setting from job info
                if (settings.ContainsKey(scopeId))
                {
                    logger.Info("Get setting from job info.");
                    return settings[scopeId];
                }
                else if (settings.ContainsKey(groupId))
                {
                    logger.Info("Get setting from job info.");
                    return settings[groupId];
                }
                #endregion
                logger.Info("Get setting from db.");
                var setting = EXOSettingDao.GetSettingInfoByScope(groupId, mailboxId, new Guid(treeNode.ID));

                if (setting == null)
                {

                    if (gruopSetingMap != null && gruopSetingMap.ContainsKey(groupId))
                    {
                        setting = gruopSetingMap[groupId];
                    }
                    else
                    {
                        // run group level setting ,get group setting
                        setting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
                        if (setting == null)
                        {
                            logger.Warn("Setting not available {0}", treeNode.ID);
                        }
                    }
                }
                return setting;
            }
        }
        private string GetDetailAction(ExchangeOnlineTreeNodeDto node)
        {
            string action = string.Empty;
            switch (node.Level)
            {
                //case NodeLevel.ExchangeOnlineMailboxGroup:
                //    action = "RM_EXO_ApplyTermToGroup";
                //    break;
                //case NodeLevel.ExchangeOnlineMailbox:
                //    action = "RM_EXO_ApplyTermToMailbox";
                //    break;
                default:
                    action = "RM_JS_JMD_Action_SetAutoClassification";
                    break;
            }
            return action;
        }
    }
}
