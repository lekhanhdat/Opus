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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeBackupUtility.Graph;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;
using ExchangeItem = ExchangeBackupUtility.ExchangeItem;
using Rule = AvePoint.GCommon.Contract.StorageOptimization.Object.Rule;
using RuleCollection = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection;

namespace AvePoint.RA.RAExchange.Report
{
    public class EXODueDisposalReportProcessor : EXOReportProcessor
    {
        private ITermDao TermDao;
        private Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping;
        private SOArchiverSettings mArchiverSettings;
        //private NodeItem mFarmNode;
        private DateTime mTimePoint;
        private List<PolicyLevel> ruleLevels;
        private Dictionary<Guid, string> cachedTermIdAndNameDic = new Dictionary<Guid, string>();

        private AvePoint.RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public AvePoint.RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private IRecordAllianceDao mIRecordAllianceDao;
        protected IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mIRecordAllianceDao == null)
                {
                    mIRecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return mIRecordAllianceDao;
            }
        }
        public EXODueDisposalReportProcessor(string jobId, string profileId)
            : base(jobId, (int)JobType.EXOItemsFilesDueDisposalReport, false)
        {
            this.ReportProfileId = profileId;
            this.ReportJobId = jobId;
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
            mTimePoint = ReportService.GetUtcTimePoint(profile.Extension1);
            mTermAndRulesMapping = ReportService.GetTermAndRuleMappingsForEXO(mTimePoint);
            ruleLevels = ReportService.GetRuleLevels(mTermAndRulesMapping);
            //ProcessWebApplication += InitRuleManagement;
            mArchiverSettings = ReportService.GetSOArchiverSettings();

            TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            cachedTermIdAndNameDic = TermDao.GetTermIdAndNameMapping();

            InitItemsPerTask();
        }

        /// <summary>
        /// 重设多线程处理方式的threshhold值， default value = 3000
        /// </summary>
        private void InitItemsPerTask()
        {
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.EXO_DUE_ITEMS_PER_TASK];
            var itemsPerTask = 3000;
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out itemsPerTask);
            }
            mLog.Info($"EXODueItemsPerTask : {itemsPerTask}");
            SetItemsPerTask(itemsPerTask); 
        }

        //private bool CheckDisposalHold(Guid scopeId, Guid nodeId, long ticks)
        //{
        //    bool result = false;
        //    try
        //    {
        //        var record = ExplorerDao.ReadById(scopeId, nodeId);
        //        var currentStatus = false;
        //        if (record != null)
        //        {
        //            currentStatus = record.HoldStatus;
        //        }
        //        if (currentStatus)
        //        {
        //            var releaseTime = RecordAllianceDao.GetRecordAllianceById(record.Id).HoldReleaseTime;
        //            if (ticks > releaseTime)
        //            {
        //                currentStatus = false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Error("error occurred while check hold status:{0}", ex.ToString());
        //    }
        //    return result;
        //}

            /// <summary>
            /// 得到hold status= true的itemid的集合
            /// </summary>
            /// <param name="scopeId"></param>
            /// <param name="items"></param>
            /// <param name="ticks"></param>
            /// <returns></returns>
        /*private IEnumerable<string> BatchCheckDisposalHold(Guid scopeId, IEnumerable<ExchangeItem> items, long ticks)
        {
            using (PerformanceScope scope = new PerformanceScope("EXODueDisposalReportProcessor.BatchCheckDisposalHold"))
            {
                var nodes = items.Select(o => new { ItemId = o.ItemId, NodeId = o.ItemId.ToMd5()});
                var nodeIds = nodes.Select(o => o.NodeId).ToList();
                try
                {
                    var records = ExplorerDao.QueryAll(rec => rec.ScopeId == scopeId && rec.HoldStatus == true && nodeIds.Contains(rec.NodeId))
                        .Select(r => new {Id = r.Id, NodeId = r.NodeId });
                    if (records.Count() > 0)
                    {
                        var holdRecordsIds = ExplorerDao.GetHoldRecordsByIds(records.Select(o => o.Id).ToList())
                            .Where(alliance => ticks <= alliance.HoldReleaseTime)
                            .Select(alliance => alliance.Id);
                        var holdNodeIds = records.Where(o => holdRecordsIds.Contains(o.Id))
                            .Select(o => o.NodeId);
                        var holdItemIds = nodes.Where(o => holdNodeIds.Contains(o.NodeId))
                            .Select(o => o.ItemId);
                        return holdItemIds;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("error occurred while batch check hold status:{0}", ex.ToString());
                }
                return new List<string>();
            }
        }*/

        /// <summary>
        /// 兼容老数据，分别使用DAOTreeNodeID和AOSMailboxID获取Hold数据
        /// </summary>
        private bool CheckDisposalHold(Guid nodeId, long ticks)           
        {
            using (PerformanceScope scope0 = new PerformanceScope("EXODueDisposalReportProcessor.CheckDisposalHold"))
            {
                bool result = false;
                try
                {
                    //1.先通过真实的AOSMailboxID获取Record
                    var recordByAOSMailboxId = ExplorerDao.QueryAll(rec => rec.ScopeId == aosMailboxId && rec.NodeId == nodeId).Select(r => new { HoldStatus = r.HoldStatus, Id = r.Id }).FirstOrDefault();
                    if (recordByAOSMailboxId == null)
                    {
                        var recordByDAOTeeNodeId = ExplorerDao.QueryAll(rec => rec.ScopeId == DAOTreeNodeID && rec.NodeId == nodeId).Select(r => new { HoldStatus = r.HoldStatus, Id = r.Id }).FirstOrDefault();
                        //2.再通过DAOTreeNodeID获取Record
                        if (recordByDAOTeeNodeId != null)
                        {
                            result = recordByDAOTeeNodeId.HoldStatus;
                            if (result)
                            {
                                Record alliance = ExplorerDao.GetRecordByIds(new List<Guid>() { recordByDAOTeeNodeId.Id }).FirstOrDefault();
                                if (alliance != null && ticks > alliance.HoldReleaseTime)
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (recordByAOSMailboxId != null)
                        {
                            result = recordByAOSMailboxId.HoldStatus;
                            if (result)
                            {
                                Record alliance = ExplorerDao.GetRecordByIds(new List<Guid>() { recordByAOSMailboxId.Id }).FirstOrDefault();
                                if (alliance != null && ticks > alliance.HoldReleaseTime)
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                    //result = CollectionDataDao.CheckDisposalHold(scopeId, nodeId, ticks);
                }
                catch (Exception ex)
                {
                    mLog.Error("error occurred while check hold status:{0}", ex.ToString());
                }
                return result;
            }
        }

        private string GetTermName(Guid termId)
        {
            if (cachedTermIdAndNameDic.ContainsKey(termId))
            {
                return cachedTermIdAndNameDic[termId];
            }
            return string.Empty;
        }

        protected override bool IsGroupItems => true;

        protected override void ProcessGroupItems(ExchangeFolder folder, IEnumerable<ExchangeItem> items)
        {
            using (PerformanceScope scope0 = new PerformanceScope("EXODueDisposalReportProcessor.ProcessGroupItems"))
            {
                //var unHoldItems = new List<ExchangeItem>();
                //foreach (var item in items)
                //{
                //    if (CheckDisposalHold(aosMailboxId, item.ItemId.ToMd5(), mTimePoint.Ticks))
                //    {
                //        mLog.Warn("File is Hold ,not delete hold {0}", item.ItemPath);
                //    }
                //    else
                //    {
                //        unHoldItems.Add(item);
                //    }
                //}
                //Hold需要判断Rule类型.
                //var holdItemIds = BatchCheckDisposalHold(aosMailboxId, items, mTimePoint.Ticks);
                //var holdItems = items.Where(o => holdItemIds.Contains(o.ItemId));
                //foreach (var item in holdItems)
                //{
                //    mLog.Warn("File is Hold ,not delete hold {0}", item.ItemPath);
                //}

                //var unHoldItems = items.Where(o => !holdItemIds.Contains(o.ItemId));
                var taxonomyTuple = GetItemsTaxonomyFieldValue(folder, items);
                foreach(var t in taxonomyTuple)
                {
                    var item = t.Item1;
                    if (!t.Item2)
                    {
                        mLog.Warn("can't get sigle item value {0}.", item.ItemId);
                        continue;
                    }

                    try
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            DealwithOneItem(item, t.Item3);
                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception ex)
                    {
                        //JobHasExceptions = true;
                        mLog.Warn("Report item failed. item url: {0}, error message: {1}.", item.ItemId, ex.ToString());
                    }
                }
            }
        }
        
        protected override void ProcessGroupItems(IExchangeFolder folder, IEnumerable<IExchangeItem> items)
        {
            using PerformanceScope scope0 = new PerformanceScope("EXODueDisposalReportProcessor.ProcessGroupItems");
            var taxonomyTuple = GetItemsTaxonomyFieldValue(folder, items);
            foreach(var t in taxonomyTuple)
            {
                var item = t.Item1;
                if (!t.Item2)
                {
                    mLog.Warn("can't get sigle item value {0}.", item.ItemId);
                    continue;
                }

                try
                {
                    using CheckJobStopScope jScope = new CheckJobStopScope();
                    DealwithOneItem(item, t.Item3);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    //JobHasExceptions = true;
                    mLog.Warn("Report item failed. item url: {0}, error message: {1}.", item.ItemId, ex.ToString());
                }
            }
        }

        protected override void ProcessItem(ExchangeItem item)
        {
            //int results = 0;
            //ReportManager.Increase();
            using (PerformanceScope scope0 = new PerformanceScope("EXODueDisposalReportProcessor.ProcessItem"))
            {
                mLog.Info("Process item: {0}.", item.ItemId);
                //if (CheckDisposalHold(aosMailboxId, new Guid(item.ExchangeId), mTimePoint.Ticks))              
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        Guid termId = Guid.Empty;
                        if (!GetSingleTaxonomyFieldValue(item, out termId))
                        {
                            mLog.Warn("can't get sigle item value: {0}.", item.ItemId);
                            return;  
                        }
                        DealwithOneItem(item, termId);
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    //JobHasExceptions = true;
                    mLog.Warn("Report item failed. item url: {0}, error message: {1}.", item.ItemId, ex.ToString());
                }
            }

        }
        
        protected override void ProcessItem(IExchangeItem item)
        {
            using PerformanceScope scope0 = new PerformanceScope("EXODueDisposalReportProcessor.ProcessItem");
            mLog.Info("Process item: {0}.", item.ItemId);
            try
            {
                using CheckJobStopScope jScope = new CheckJobStopScope();
                Guid termId = Guid.Empty;
                if (!GetSingleTaxonomyFieldValue(item, out termId))
                {
                    mLog.Warn("can't get sigle item value: {0}.", item.ItemId);
                    return;  
                }
                DealwithOneItem(item, termId);
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                //JobHasExceptions = true;
                mLog.Warn("Report item failed. item url: {0}, error message: {1}.", item.ItemId, ex.ToString());
            }
        }

        private void DealwithOneItem(ExchangeItem item, Guid termId)
        {
            RMRuleItemCollection rules;
            if (mTermAndRulesMapping.TryGetValue(termId, out rules))
            {
                if (rules.Rules.Count == 0)
                {
                    return;
                }
                DueDisposalReport report = new DueDisposalReport();
                //get object base info 
                //check rule
                #region rebuild sp rule
                RuleCollection newRuleCol = new RuleCollection();
                Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
                int reOrlder = 0;
                foreach (var order in rules.CommonRules.Rules.Keys)
                {
                    if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].SOFilters != null && rules.CommonRules.Rules[order].SOFilters.Count > 0)
                    {
                        var rule = rules.CommonRules.Rules[order];
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            reOrlder++;
                            newRules.Add(reOrlder, rule);
                        }
                    }
                }
                newRuleCol.Rules = newRules;
                #endregion
                RuleManagement ruleManagement = new RuleManagement(rules.CommonRules);
                //commented out by byron, current query will get all item's field info, so need not to get the item again.
                //this function will throw exception if the list's itemcount > threshold.
                Rule rs = ruleManagement.CheckItemCriteria(item);
                if (CheckDisposalHold(item.ItemId.ToMd5(), mTimePoint.Ticks) && IsRemoveRule(rs))
                {
                    mLog.Warn("File is Hold ,not delete hold {0}.", item.ItemId);
                    return;
                }
                if (rs != null)
                {
                    report.AppliedRuleId = rs.Id;
                    report.AppliedRuleName = rs.Name;
                    report.DisposalAction = RuleHelper.GetOperationTypeForEXO(rs);
                    report.ManualApproval = rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                    report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                    report.DisposalClass = rs.DisposalClass;
                    mLog.Info("Item fit rule {0}, {1}.", item.ItemId, rs.Name);
                }
                else
                {
                    mLog.Info("Item not fit rule {0}.", item.ItemId);
                    return;
                }

                mLog.Info("build item report{0}.", item.ItemId);
                try
                {
                    report.TitleOrName = item.ItemName;
                    report.Url = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                    report.BCSTermId = termId.ToString();
                    report.BCSTermName = GetTermName(termId);
                    report.ObjectLevel = (int)NodeLevel.ExchangeOnlineItem;
                    report.CreatedBy = item.Sender;
                    report.CreatedTime = item.Created.Ticks;
                    report.LastModifiedBy = item.ModifiedBy;
                    report.LastModifiedTime = item.Modified.Ticks;
                    report.SPWebTimeZoneName = "";

                    // check document is skip file
                    string itemUrl = item.ItemPath;
                }
                catch
                {
                    mLog.Info("build item report error{0}.", item.ItemId);
                    report.Status = RMReportStatus.Failed;
                    report.Comment = "RM_JM_ReportComment_Failed";
                    throw;
                }
                finally
                {
                    mLog.Info("add item report:{0}.", item.ItemId);
                    ReportManager.SendJobReport(report);
                    //reports.Add(report);
                    //results++;
                }
            }
        }

        private void DealwithOneItem(IExchangeItem item, Guid termId)
        {
            RMRuleItemCollection rules;
            if (mTermAndRulesMapping.TryGetValue(termId, out rules))
            {
                if (rules.Rules.Count == 0)
                {
                    return;
                }
                DueDisposalReport report = new DueDisposalReport();

                #region rebuild sp rule
                RuleCollection newRuleCol = new RuleCollection();
                Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
                int reOrlder = 0;
                foreach (var order in rules.CommonRules.Rules.Keys)
                {
                    if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].SOFilters != null && rules.CommonRules.Rules[order].SOFilters.Count > 0)
                    {
                        var rule = rules.CommonRules.Rules[order];
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            reOrlder++;
                            newRules.Add(reOrlder, rule);
                        }
                    }
                }
                newRuleCol.Rules = newRules;
                #endregion
                RuleManagement ruleManagement = new RuleManagement(rules.CommonRules);

                Rule rs = ruleManagement.CheckItemCriteria(item);
                if (CheckDisposalHold(item.ItemId.ToMd5(), mTimePoint.Ticks) && IsRemoveRule(rs))
                {
                    mLog.Warn("File is Hold ,not delete hold {0}.", item.ItemId);
                    return;
                }
                if (rs != null)
                {
                    report.AppliedRuleId = rs.Id;
                    report.AppliedRuleName = rs.Name;
                    report.DisposalAction = RuleHelper.GetOperationTypeForEXO(rs);
                    report.ManualApproval = rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                    report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                    report.DisposalClass = rs.DisposalClass;
                    mLog.Info("Item fit rule {0}, {1}.", item.ItemId, rs.Name);
                }
                else
                {
                    mLog.Info("Item not fit rule {0}.", item.ItemId);
                    return;
                }

                mLog.Info("build item report{0}.", item.ItemId);
                try
                {
                    report.TitleOrName = item.ItemName;
                    report.Url = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                    report.BCSTermId = termId.ToString();
                    report.BCSTermName = GetTermName(termId);
                    report.ObjectLevel = (int)NodeLevel.ExchangeOnlineItem;
                    report.CreatedBy = item.Sender;
                    report.CreatedTime = item.Created.Ticks;
                    report.LastModifiedBy = item.ModifiedBy;
                    report.LastModifiedTime = item.Modified.Ticks;
                    report.SPWebTimeZoneName = "";

                    string itemUrl = item.ItemPath;
                }
                catch
                {
                    mLog.Info("build item report error{0}.", item.ItemId);
                    report.Status = RMReportStatus.Failed;
                    report.Comment = "RM_JM_ReportComment_Failed";
                    throw;
                }
                finally
                {
                    mLog.Info("add item report:{0}.", item.ItemId);
                    ReportManager.SendJobReport(report);
                }
            }
        }

        private bool IsRemoveRule(Rule tempRule)
        {
            //var result = false;
            ////if (tempRule != null && tempRule.EXORule != null && tempRule.EXORule.KeepDataOption == 0)
            ////RECO-3972
            ////current rule is exo rule.
            //if (tempRule != null && tempRule.KeepDataOption == 0)
            //{
            //    result = true;
            //}
            if (tempRule != null)
            {
                int action = RuleHelper.GetOperationTypeForEXO(tempRule);
                if (action == 0)
                {
                    return true;
                }
            }
            return false;

        }
    }
}
