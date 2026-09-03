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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Authorization;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using ExchangeBackupUtility;
using ExchangeUtility;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.ApplySetting
{
    public class RMEXOApplySettingBase : RMEXODiscoverBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMEXOApplySettingProcesser));
        private Dictionary<string, Guid> ruleTermIdMapping = new Dictionary<string, Guid>();
        private Dictionary<string, string> termNameDic = new Dictionary<string, string>();
        protected RMExchangeOnlineSetting Setting = null;
        protected RuleManagement RuleManagement = null;
        private RMEXODiscoverHelper discoverHelper = null;
        private IBatchDiscover discover = null;
        private static Semaphore mWorkerThreads = new Semaphore(2, 2);
        protected Guid GroupId = Guid.Empty;
        /// <summary>
        /// 旧的ID，可能是DAOTreeNodeID，也可能是GUID的AOS MailboxID(经过特殊处理满足Records GUID格式需求的ID)
        /// </summary>
        protected Guid AOSMailboxId = Guid.Empty;
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

        private IEXOSettingDao mEXOSettingDao;
        protected IEXOSettingDao EXOSettingDao
        {
            get
            {
                if (mEXOSettingDao == null)
                {
                    mEXOSettingDao = new EXOSettingDao();
                }
                return mEXOSettingDao;
            }
        }

        private ITermDao mTermDao;
        protected ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = new TermDao();
                }
                return mTermDao;
            }
        }

        public RMEXOApplySettingBase(RMExchangeOnlineSetting setting, ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement)
            : base(treeNode)
        {
            Setting = setting;
            JobManagement = jobManagement;
            if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
            {
                List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
                var ruleCollection = RMEXOApplySettingRuleUtil.GetRuleCollection(autoRules, ref ruleTermIdMapping);
                RuleManagement = new RuleManagement(ruleCollection);
            }
        }

        protected JobManagement JobManagement = null;

        public virtual void RunNow()
        {
            using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.RunNow", "", true))
            {
                Init();
                ProcessFolder(CurrentFolder);
            }
        }

        public override void Init()
        {
            using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.Init", "", true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        base.Init();
                        //目前按照childFolder Count 算，更新也更新到Folder level。 后期考虑添加逻辑，Folder数量低于2的时候，按照Folder下的Item 更新
                        long totalCount = CurrentFolder.ChildFolderCount;
                        JobManagement.ReportManager.IncreaseBase(totalCount);
                        GroupId = new Guid(TreeManagement.GetGroupNode(TreeNodeDto).ID);
                        AOSMailboxId = new Guid(base.MailboxGuid); //new Guid(TreeManagement.GetMailboxNode(TreeNodeDto).ID);
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
            }
        }

        public void SetDiscoverObject(RMEXODiscoverHelper discoverHelper, IBatchDiscover discover)
        {
            this.discoverHelper = discoverHelper;
            this.discover = discover;
        }

        private void ProcessFolder(ExchangeFolder folder)
        {
            logger.Info($"Begin processing folder : {folder.FolderId}.");
            using (new PerformanceScope("ProcessingFolder", $"ProcessingFolder {folder.FolderId}", true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        //此处用GetItems 的值更合理，但是很多getitems是异步的，没有办法获取所有值
                        JobManagement.ReportManager.IncreaseBase(folder.ItemsCount);
                        var setting = EXOSettingDao.GetSettingInfoByScope(Guid.Empty, Guid.Empty, folder.FolderId.ToMd5());
                        if (setting != null)
                        {
                            logger.Info($"Folder : {folder.DisplayFolderPath} is a break-inherit node which has custom setting.");
                            return;
                        }
                        foreach (var mFolder in GetFolders(folder))
                        {
                            ProcessFolder(mFolder);
                        }

                        //foreach(var s in discover.GetItems(folder))
                        //{
                        //    ProcessGroupedItem(s, folder);
                        //}
                        ProcessGroupedItems(folder);

                        if (Setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            //Remove node info in db.
                            EXONodeInfoDao.DeleteEXONodeInfo(folder.FolderId.ToMd5(), GroupId, (int)NodeFlagType.AutoClassification);
                        }
                        else
                        {
                            using (new PerformanceScope("ProcessingFolder", $"ProcessingFolder GenerateCurrentItemSyncState {folder.FolderId}", true))
                            {
                                folder.GenerateCurrentItemSyncState();
                                EXONodeInfoDao.AddEXONodeInfo(GenerateNodeFlag(folder));
                            }
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    JobManagement.HasErrorNode = true;

                    logger.Error($"Error in process folder : {folder.DisplayFolderPath}, reason : {ex.ToString()}.");
                    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                    {
                        ObjectName = folder.FolderName,
                        FullPath = MailboxAddress + folder.DisplayFolderPath,
                        ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeFolder),
                        Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                        Action = "RM_JS_JMD_Action_SetAutoClassification",
                        Classification = string.Empty
                    });
                }
            }
        }

        private void ProcessGroupedItems(ExchangeFolder folder)
        {
            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
            {
                taskExecutor.StartExecute();
                IEnumerable<ExchangeItemGroup> items = null;
                using (new PerformanceScope("GetGroupedItems", $"GetGroupedItems{folder.FolderId}", true))
                {
                    var filter = GenerateSearchFilter(folder);
                    items = discover.GetGroupedItems(folder, filter);
                }
                using (new PerformanceScope("ProcessGroupItems", $"ProcessGroupItems{folder.FolderId}", true))
                {
                    foreach (var item in items)
                    {
                        taskExecutor.AddTask(() =>
                        {                          
                            ProcessGroupedItem(item, folder);
                        });
                    }

                    logger.Info($"Add items to task executor finished.");
                    if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                    {
                        //todo: handle timeout
                        logger.Error($"Time out exception.");
                    }
                }
            }
            logger.Info($"ProcessItems finish.");
        }

        private SearchFilter GenerateSearchFilter(ExchangeFolder folder)
        {
            SearchFilter filter = null;
            if (!Setting.RunAutoFullJob && Setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                DateTime collectionTime = DateTime.MinValue;
                var nodeInfo = EXONodeInfoDao.GetEXONodeInfo(folder.FolderId.ToMd5(), GroupId, (int)NodeFlagType.AutoClassification);
                if (nodeInfo != null)
                {
                    collectionTime = DateTime.SpecifyKind(new DateTime(nodeInfo.CollectionTime), DateTimeKind.Utc);
                }
                filter = new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.LastModifiedTime, collectionTime);
            }
            return filter;
        }

        private void ProcessGroupedItem(ExchangeItemGroup itemGroup, ExchangeFolder folder)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    logger.Info($"Begin process grouped item, item count: {itemGroup.ItemsCount}.");
                    //if (Setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification && Setting.AutoJobOption == (int)AutoJobOption.SkipAndKeep)
                    //{
                    //    foreach (var item in itemGroup.Items)
                    //    {
                    //        ProcessItem(item);
                    //    }
                    //}
                    //else
                    //{
                    ExchangeItemBulkHelper bulkHelper = new ExchangeItemBulkHelper(folder, "");
                    var def = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                    var detailAction = string.Empty;
                    if (Setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        using (new PerformanceScope("RemoveTermForGroupedItem", $"RemoveTermForGroupedItem{itemGroup.ItemsCount}", true))
                        {
                            RemoveTermForGroupedItem(itemGroup, bulkHelper, def);
                        }
                    }
                    else
                    {
                        using (new PerformanceScope("AddTermForGroupedItem", $"AddTermForGroupedItem{itemGroup.ItemsCount}", true))
                        {
                            AddTermForGroupedItem(itemGroup, bulkHelper, def, Setting.AutoJobOption == (int)AutoJobOption.Override);
                        }
                    }
                    // }
                    #region No use code
                    ////Key 是Item，Value是对应的TermId
                    //Dictionary<ExchangeItem, string> itemAndTermIdMapping = new Dictionary<ExchangeItem, string>();
                    //Dictionary<string, UpdateItemResult> checkRuleErrorResult = new Dictionary<string, UpdateItemResult>();
                    //foreach (var item in itemGroup.Items)
                    //{
                    //    try
                    //    {
                    //        var termId = GetTermId(item);
                    //        if (termId != Guid.Empty)
                    //        {
                    //            logger.Info($"Prepare add term column for item : {item.ItemPath}, term id : {termId.ToString()}.");
                    //            //Add to item and term id mapping
                    //            itemAndTermIdMapping[item] = termId.ToString();
                    //        }
                    //        else
                    //        {
                    //            logger.Warn($"Term id is empty. no need to add term for item : {item.ItemPath}.");
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        JobManagement.HasErrorNode = true;
                    //        logger.Error($"Error in add term to item : {item.ItemPath}, reason : {ex.ToString()}.");
                    //        var result = UpdateItemResult.CreateFailedResult(item.ItemId, ex.Message);
                    //        checkRuleErrorResult[item.ItemId] = result;
                    //    }
                    //}
                    //logger.Info($"Start batch update items.");
                    //var updateResult = bulkHelper.BatchUpdateItems(itemAndTermIdMapping, def);
                    //foreach (var kv in checkRuleErrorResult)
                    //{
                    //    updateResult.Add(kv.Key, kv.Value);
                    //}
                    //foreach (var item in itemGroup.Items)
                    //{
                    //    var status = AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful;
                    //    var comment = string.Empty;
                    //    UpdateItemResult result;
                    //    if (updateResult.TryGetValue(item.ItemId, out result))
                    //    {
                    //        if (result.IsFailed)
                    //        {
                    //            status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                    //            comment = result.ErrorMessage;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        logger.Warn($"Cannot find item by id : {item.ItemId} in update result collection.");
                    //        status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                    //        comment = "Cannot find item in update result collection";
                    //    }
                    //    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                    //    {
                    //        ObjectName = item.ItemName,
                    //        FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                    //        ItemType = NodeLevel.ExchangeOnlineItem.ToString(),
                    //        Status = status,
                    //        Comment = comment,
                    //    });
                    //}
                    #endregion
                    logger.Info($"Finish process grouped item, item count: {itemGroup.ItemsCount}.");
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            finally
            {
                JobManagement.ReportManager.Increase(itemGroup.ItemsCount);
            }
        }

        private void AddTermForGroupedItem(ExchangeItemGroup itemGroup, ExchangeItemBulkHelper bulkHelper, ExtendedPropertyDefinition def, bool isOverwrite)
        {
            //Key 是Item，Value是对应的TermId. 表示批处理的Item 集合
            Dictionary<ExchangeItem, string> itemAndTermIdMapping = new Dictionary<ExchangeItem, string>();
            ExtendedPropertyDefinition sensitivityLabelDef = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.InternetHeaders, "msip_labels", MapiPropertyType.String);
            if (!isOverwrite)
            {
                using (new PerformanceScope("AddTermForGroupedItem.LoadExtendProperties", $"AddTermForGroupedItem.LoadExtendProperties{itemGroup.ItemsCount}", true))
                {
                    try
                    {
                        bulkHelper.LoadExtendProperties(itemGroup.Items, def, sensitivityLabelDef);
                    }
                    catch (Exception e)
                    {
                        JobManagement.HasErrorNode = true;
                        logger.Error($"Error in getting item property, reason : {e.ToString()}.");
                        if (itemGroup != null && itemGroup.ItemsCount > 0)
                        {
                            foreach (var item in itemGroup.Items)
                            {
                                JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                                {
                                    Action = "RM_JS_JMD_Action_SetAutoClassification",
                                    ObjectName = item.ItemName,
                                    FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                                    ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                                    Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                                    Comment = e.Message,
                                    Classification = string.Empty
                                });
                            }
                        }
                        throw;
                    }
                }
            }
            else
            {
                try
                {
                    bulkHelper.LoadExtendProperties(itemGroup.Items, sensitivityLabelDef);
                }
                catch (Exception e)
                {
                    logger.Error($"Fail load sensitivity label , error message:{e.Message}, error :{e}");
                }
            }
            foreach (var item in itemGroup.Items)
            {
                try
                {
                    if (!isOverwrite)
                    {
                        string value;
                        if (item.TryGetProperty(def, out value))
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                logger.Info($"Item : {item.ItemId} already have the term property, value is : {value}, skip add the property for it.");
                                //Return here, no need to add job detail
                                continue;
                            }
                        }
                    }
                    var termId = GetTermId(item);
                    if (termId != Guid.Empty)
                    {
                        logger.Info($"Prepare add term column for item : {item.ItemId}, term id : {termId.ToString()}. ModifiedTime:{item.Modified.Ticks}");
                        //Add to item and term id mapping
                        itemAndTermIdMapping[item] = termId.ToString();
                        AddTermNameToDic(item.ItemId, termId);
                    }
                    else
                    {
                        logger.Warn($"Term id is empty. no need to add term for item : {item.ItemId}.");
                    }
                }
                catch (Exception ex)
                {
                    JobManagement.HasErrorNode = true;
                    logger.Error($"Error in add term to item : {item.ItemId}, reason : {ex.ToString()}.");
                    //Check rule 失败的部分，单独发送Job detail
                    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                    {
                        Action = "RM_JS_JMD_Action_SetAutoClassification",
                        ObjectName = item.ItemName,
                        FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                        ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                        Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                        Comment = ex.Message,
                        Classification = string.Empty
                    });
                }
            }

            logger.Info($"Start batch update items.");
            mWorkerThreads.WaitOne();
            var updateResult = new Dictionary<string, UpdateItemResult>();
            try
            {
                if (itemAndTermIdMapping != null && itemAndTermIdMapping.Count > 0)
                {
                    using (new PerformanceScope("AddTermForGroupedItem.BatchAddExtendPorperty", $"AddTermForGroupedItem.BatchAddExtendPorperty{itemAndTermIdMapping.Count}", true))
                    {
                        updateResult = bulkHelper.BatchAddExtendPorperty(itemAndTermIdMapping, def);
                    }
                }
                else
                {
                    logger.Info($"itemAndTermIdMapping is null or empty so skip BatchAddExtendPorperty when AddTermForGroupedItem.");
                }
            }
            finally
            {
                mWorkerThreads.Release();
            }
            logger.Info($"Finish batch update items.");

            //对批处理的Item 依次获取result
            using (var performance = new PerformanceScope("EXO.RuleManagement.AnalyzeResult", addToStatistics: true))
            {
                foreach (var item in itemAndTermIdMapping.Keys)
                {
                    var status = AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful;
                    var comment = string.Empty;
                    UpdateItemResult result;
                    if (updateResult.TryGetValue(item.ItemId, out result))
                    {
                        if (result.IsFailed)
                        {
                            status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                            comment = result.ErrorMessage;
                        }
                    }
                    else
                    {
                        logger.Warn($"Cannot find item by id :{item.ItemId} in update result collection.");
                        status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                        comment = "Cannot find item in update result collection";
                    }
                    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                    {
                        Action = "RM_JS_JMD_Action_SetAutoClassification",
                        ObjectName = item.ItemName,
                        FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                        ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                        Status = status,
                        Comment = comment,
                        Classification = GetTermNameFromDicByFileId(item.ItemId),
                    });
                    AnalyzeStatus(status);
                }
            }
        }


        private void AddTermNameToDic(string fileId, Guid termId)
        {
            try
            {
                string termName = string.Empty;
                if(!termNameDic.ContainsKey(fileId))
                {
                    var term = TermDao.GetRMTermByGuId(termId);
                    termName = term?.Name ?? string.Empty;
                    termNameDic.Add(fileId, termName);
                }
            }
            catch(Exception e)
            {
                logger.Error($"Add term name to dic occurs errors: {e}");
            }
        }

        private string GetTermNameFromDicByFileId(string fileId)
        {
            try
            {
                string termName = string.Empty;
                if(!termNameDic.TryGetValue(fileId, out termName))
                {
                    return string.Empty;
                }
                return termName;
            }
            catch (Exception e)
            {
                logger.Error($"Get term name from dic occurs errors: {e}");
                return string.Empty;
            }
        }

        private void RemoveTermForGroupedItem(ExchangeItemGroup itemGroup, ExchangeItemBulkHelper bulkHelper, ExtendedPropertyDefinition def)
        {
            logger.Info($"Start batch update items.");
            var updateResult = new Dictionary<string, UpdateItemResult>();
            try
            {
                mWorkerThreads.WaitOne();
                updateResult = bulkHelper.BatchRemoveExtendPorperty(itemGroup.Items.ToList(), def);
            }
            finally
            {
                mWorkerThreads.Release();
            }
            foreach (var item in itemGroup.Items)
            {
                var status = AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful;
                var comment = string.Empty;
                UpdateItemResult result;
                if (updateResult.TryGetValue(item.ItemId, out result))
                {
                    if (result.IsFailed)
                    {
                        status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                        comment = result.ErrorMessage;
                    }
                }
                else
                {
                    logger.Warn($"Cannot find item by id : {item.ItemId} in update result collection.");
                    status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                    comment = "Cannot find item in update result collection";
                }
                JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
                {
                    Action = "RM_EXO_RemoveTerm",
                    ObjectName = item.ItemName,
                    FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                    ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                    Status = status,
                    Comment = comment,
                    Classification = string.Empty
                });
                AnalyzeStatus(status);
            }
        }

        private void AnalyzeStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                case JobDetailsStatus.Skipped:
                    JobManagement.HasSuccessNode = true;
                    break;
                case JobDetailsStatus.Failed:
                    JobManagement.HasErrorNode = true;
                    break;
                default:
                    break;
            }
        }

        private Guid GetTermId(ExchangeItem item)
        {
            using (var performance = new PerformanceScope("EXO.RuleManagement.GetTermId", addToStatistics: true))
            {
                Guid termId;
                try
                {
                    var rule = RuleManagement.CheckItemCriteria(item);
                    termId = rule == null ? ruleTermIdMapping[Guid.Empty.ToString()] : ruleTermIdMapping[rule.Id];
                }
                catch
                {
                    throw;
                }
                return termId;
            }
        }

       /* private void ApplyTermValue(ExchangeItem item, string termUniqueId, string termName)
        {
            using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.ApplyTermValue", "", true))
            {
                if (Setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification && Setting.AutoJobOption == (int)AutoJobOption.Override)
                {
                    item.UpdateItemIdField(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, termUniqueId);
                    //item.UpdateItemIdField(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnName, termName);
                }
                else
                {
                    var def = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
                    var properties = item.LoadExtendProperties(def);
                    if (properties.ContainsKey(def) && !string.IsNullOrEmpty(properties[def]))
                    {
                        logger.Info($"Item : {item.ItemId} already have the term property, value is : {properties[def]}, skip add the property for it.");
                        //Return here, no need to add job detail
                        return;
                    }
                    else
                    {
                        item.UpdateItemIdField(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, termUniqueId);
                        //item.UpdateItemIdField(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnName, termName);
                    }
                }
            }
            logger.Info($"Finish add term for item : {item.ItemId}.");
            JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOApplySettingJobDetails()
            {
                ObjectName = item.ItemName,
                FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful,
                Comment = "",
                Action = "RM_EXO_ApplyTerm"
            });
        }*/

        private EXONodeFlag GenerateNodeFlag(ExchangeFolder folder)
        {
            EXONodeFlag nodeFlag = new EXONodeFlag();
            nodeFlag.CollectionTime = DateTime.UtcNow.Ticks;
            nodeFlag.EmailAdress = folder.Mailbox.MailboxAddress;
            nodeFlag.AOSEmailboxId = AOSMailboxId;
            nodeFlag.FolderSyncState = folder.FolderSyncState;
            nodeFlag.FullPath = folder.DisplayFolderPath;
            nodeFlag.GroupId = GroupId;
            nodeFlag.IsRemoved = false;
            nodeFlag.ItemSyncState = folder.ItemSyncState;
            nodeFlag.NodeFlagType = (int)NodeFlagType.AutoClassification;
            nodeFlag.NodeId = folder.IsRootFolder ? AOSMailboxId : folder.FolderId.ToMd5();
            nodeFlag.Title = folder.FolderName;
            nodeFlag.AOSObjectId = AOSObjectId;
            return nodeFlag;
        }
    }
}
