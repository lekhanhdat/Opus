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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Service.Services.CustomizeConnector.RuleManagement;
using Microsoft.Graph;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager;

namespace AvePoint.RA.Service.Services.CustomizeConnector.Timer
{
    public class RMConnectorTimerBase
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMConnectorTimerBase));
        private Dictionary<Guid, long> changeTerms = new Dictionary<Guid, long>();
        
        private List<Guid> reportedRecordIds = new List<Guid>();
        private DateTime mRunJobTime;
        private int itemsPerTask = 400;
        private Dictionary<Guid, RMRule> ruleNameCache;
        private Dictionary<int, string> connectorNameCache;
        private Dictionary<Guid, string> termNameCache;
        #region interface

        private IRMCustomizeConnectorService mRMCustomizeConnectorService;
        public IRMCustomizeConnectorService RMCustomizeConnectorService
        {
            get
            {
                if (mRMCustomizeConnectorService == null)
                {
                    mRMCustomizeConnectorService = (IRMCustomizeConnectorService)PlatformWindsorManager.GetService(typeof(IRMCustomizeConnectorService));
                }
                return mRMCustomizeConnectorService;
            }
        }

        private IRMPhysicalNodeFlagDao mPhysicalNodeInfoDao;
        protected IRMPhysicalNodeFlagDao PhysicalNodeInfoDao
        {
            get
            {
                if (mPhysicalNodeInfoDao == null)
                {
                    mPhysicalNodeInfoDao = new RMPhysicalNodeFlagDao();
                }
                return mPhysicalNodeInfoDao;
            }
        }

        private IExplorerService mExplorerService;
        protected IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
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
        private IRMRuleDao mRMRuleDao;
        public IRMRuleDao RMRuleDao
        {
            get
            {
                if (mRMRuleDao == null)
                {
                    mRMRuleDao = (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao));
                }
                return mRMRuleDao;
            }
        }

        private ITermDao mITermDao { get; set; }
        public ITermDao TermDao
        {
            get
            {
                if (mITermDao == null)
                {
                    mITermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mITermDao;
            }
        }
        #endregion
        public bool HasError { get; set; } = false;
        public async System.Threading.Tasks.Task RunNowAsync()
        {
            bool initSuccess = false;
            try
            {
                mRunJobTime = DateTime.UtcNow;               
                var connectorSources = await RMCustomizeConnectorService.GetAllAsync();
                if (connectorSources != null && connectorSources.Count() > 0)
                {
                    await InitAsync(connectorSources);
                    var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
                    initSuccess = true;
                    foreach (var connector in connectorSources)
                    {
                        var nodeInfo = PhysicalNodeInfoDao.GetPhysicalNodeInfo(connector.Id, Guid.Empty, (int)NodeFlagType.ConnectorTimer);
                        long collectionTime = DateTime.MinValue.Ticks;
                        if (nodeInfo != null)
                        {
                            collectionTime = nodeInfo.CollectionTime;
                        }
                        changeTerms = ExplorerService.GetChangedTerms(collectionTime);
                        await ProcessConnectorAsync(connector);
                        PhysicalNodeInfoDao.AddPhysicalNodeInfo(GenerateNodeFlag(connector));
                    }
                }
                else
                {
                    logger.Info("No connector found.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occured when RunNowAsync,error:{e}");
            }
            finally
            {
                if (initSuccess)
                {
                    CosmosBulkOperator.Instance.Complete();
                }
            }
        }

        private async System.Threading.Tasks.Task InitAsync(IEnumerable<CustomizeConnectorInfo> connectorInfos)
        {
            ruleNameCache = (await RMRuleDao.GetRulesWithoutRemovedAsync()).ToDictionary(r => r.RuleId);
            connectorNameCache = connectorInfos.ToDictionary(c => c.Flag, c => c.Name);
            termNameCache = TermDao.GetTermIdAndNameMapping();
        }

        private async System.Threading.Tasks.Task ProcessConnectorAsync(CustomizeConnectorInfo connectorInfo)
        {
            using (var performance1 = new PerformanceScope("RMConnectorTimerBase.ProcessConnector", addToStatistics: true))
            {
                try
                {
                    connectorInfo = await RMCustomizeConnectorService.GetAsync(connectorInfo.Id);
                    var columnManager = new ConnectorColumnManager(connectorInfo.ColumnInfoes);
                    var termRuleInfoManager = new ConnectorTermRuleInfoManagement();
                    logger.Info($"Process connector uniqueId: {connectorInfo.Id}.  changedtermid count {changeTerms.Count}");
                    //Changed Term可能存在大数据， 一次处理会导致SQL过长， changed in CI Nov 2021
                    int pageSize = 500;
                    if (changeTerms.Count <= pageSize)
                    {
                        var changeTermIds = changeTerms.Keys.ToList();
                        var records = ExplorerDao.GetConnectorRecordsByTerms(connectorInfo.Flag, changeTermIds);
                        ProcessConnectorRecords(records, termRuleInfoManager, columnManager);
                    }
                    else
                    {
                        List<Guid> tempIds = null;
                        int index = 0;
                        var changeTermIds = changeTerms.Keys.ToList();
                        do
                        {
                            tempIds = changeTermIds.Skip(index * pageSize).Take(pageSize).ToList();
                            if (tempIds.Count > 0)
                            {
                                var records = ExplorerDao.GetConnectorRecordsByTerms(connectorInfo.Flag, tempIds);
                                logger.Info($"Index {index}");
                                ProcessConnectorRecords(records, termRuleInfoManager, columnManager);
                            }
                            index++;
                        } while (tempIds.Count > 0);
                    }

                    // Handle apply the connector does not match rule before
                    logger.Info($"Handle the connector does match rule before");
                    var connectorRecords = ExplorerDao.GetDoesNotMatchRuleConnectorItems(connectorInfo.Flag);
                    ProcessConnectorItemsUnmatchedRuleConnectorRecords(connectorRecords.ToList(), termRuleInfoManager, columnManager);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in Process Connector : {connectorInfo.Id}, reason : {ex.ToString()}.");
                    HasError = true;
                    ReportManager.SendJobDetail(new JMConnectorTimerJobDetails()
                    {
                        ObjectName = connectorInfo.Name,
                        ConnectorName = connectorInfo.Name,
                        TermName = "",
                        RuleName = "",
                        Status = JobDetailsStatus.Failed,
                        Comment = ex.Message,
                    });
                }
            }
        }

        private void ProcessConnectorItemsUnmatchedRuleConnectorRecords(List<Record> records, ConnectorTermRuleInfoManagement connectorTermRuleInfo, ConnectorColumnManager columnManager)
        {
            logger.Info($"Process connector item count:[{records.Count}]");
            ReportManager.IncreaseBase(records.Count);
            int existingItemsPerTask = records.Count / 4;
            if (records.Count > itemsPerTask)
            {
                var cts = new System.Threading.CancellationTokenSource();
                AveTenantTasks.RunParallelBatch(records, existingItemsPerTask, cts, item =>
                {
                    ProcessRecordsUnmatchedRuleBatch(item.AsEnumerable(), connectorTermRuleInfo, columnManager);
                });
            }
            else
            {
                ProcessRecordsUnmatchedRuleBatch(records, connectorTermRuleInfo, columnManager);
            }
        }

        private void ProcessRecordsUnmatchedRuleBatch(IEnumerable<Record> records, ConnectorTermRuleInfoManagement connectorTermRuleInfo, ConnectorColumnManager columnManager)
        {
            foreach (var record in records)
            {
                try
                {
                    using (var performance1 = new PerformanceScope("RMConnectorTimerBase.ApplyRule", addToStatistics: true))
                    {
                        var rulePolicyValues = columnManager.ConvertToRulePolicy(record.CustomColumnDic);
                        connectorTermRuleInfo.ApplyRule(record, rulePolicyValues);
                    }
                    CosmosBulkOperator.Instance.Add(record);
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while process item.Id:{record.Id} Error:{e.ToString()}");
                    HasError = true;
                    ReportManager.SendJobDetail(new JMConnectorTimerJobDetails()
                    {
                        ObjectName = record.LeafName,
                        ConnectorName = connectorNameCache[record.SourceFlag],
                        TermName = termNameCache.ContainsKey(record.TermId) ? termNameCache[record.TermId] : "",
                        Status = JobDetailsStatus.Failed,
                        RuleName = ruleNameCache != null && ruleNameCache.ContainsKey(record.RuleId) ? ruleNameCache[record.RuleId].RuleName : "",
                        Comment = e.Message
                    });

                }
                finally
                {
                    ReportManager.Increase();
                }
            }
        }


        private void ProcessConnectorRecords(List<Record> records, ConnectorTermRuleInfoManagement connectorTermRuleInfo, ConnectorColumnManager columnManager)
        {
            logger.Info($"Process item count:[{records.Count}]");
            ReportManager.IncreaseBase(records.Count);
            int existingItemsPerTask = records.Count / 4;
            if (records.Count > itemsPerTask)
            {
                var cts = new System.Threading.CancellationTokenSource();
                AveTenantTasks.RunParallelBatch(records, existingItemsPerTask, cts, item =>
                {
                    ProcessRecordsBatch(item.AsEnumerable(), connectorTermRuleInfo, columnManager);
                });
            }
            else
            {
                ProcessRecordsBatch(records, connectorTermRuleInfo, columnManager);
            }
        }


        private void ProcessRecordsBatch(IEnumerable<Record> records, ConnectorTermRuleInfoManagement connectorTermRuleInfo, ConnectorColumnManager columnManager)
        {
            foreach (var record in records)
            {
                try
                {
                    if(record.CollectTime < changeTerms[record.TermId])
                    {
                        using (var performance1 = new PerformanceScope("RMConnectorTimerBase.ApplyRule", addToStatistics: true))
                        {
                            var rulePolicyValues = columnManager.ConvertToRulePolicy(record.CustomColumnDic);
                            connectorTermRuleInfo.ApplyRule(record, rulePolicyValues);
                        }
                        CosmosBulkOperator.Instance.Add(record);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while process item.Id:{record.Id} Error:{e.ToString()}");
                    HasError = true;
                    ReportManager.SendJobDetail(new JMConnectorTimerJobDetails()
                    {
                        ObjectName = record.LeafName,
                        ConnectorName = connectorNameCache[record.SourceFlag],
                        TermName = termNameCache.ContainsKey(record.TermId) ? termNameCache[record.TermId] : "",
                        Status = JobDetailsStatus.Failed,
                        RuleName = ruleNameCache != null && ruleNameCache.ContainsKey(record.RuleId) ? ruleNameCache[record.RuleId].RuleName : "",
                        Comment = e.Message
                    });

                }
                finally
                {
                    ReportManager.Increase();
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessSucceedRecord(Record newItem)
        {
            logger.Info($"Update record to db success :{newItem.Id} {newItem.RowKey}");
            if(reportedRecordIds.Contains(newItem.Id))
            {
                return;
            }
            ReportManager.SendJobDetail(new JMConnectorTimerJobDetails()
            {
                ObjectName = newItem.LeafName,
                ConnectorName = connectorNameCache[newItem.SourceFlag],
                TermName = termNameCache.ContainsKey(newItem.TermId)? termNameCache[newItem.TermId]:"",
                Status = JobDetailsStatus.Successful,
                RuleName = ruleNameCache != null && ruleNameCache.ContainsKey(newItem.RuleId) ? ruleNameCache[newItem.RuleId].RuleName : "",
                Comment = newItem.Comment
            });
            reportedRecordIds.Add(newItem.Id);
        }

        private void ProcessFailedRecord(Record record, Exception ex)
        {
            logger.Warn($"Failed to update record to db, the item id:{record.Id} {record.RowKey}");
            HasError = true;
            ReportManager.SendJobDetail(new JMConnectorTimerJobDetails()
            {
                ObjectName = record.LeafName,
                ConnectorName = connectorNameCache[record.SourceFlag],
                TermName = termNameCache.ContainsKey(record.TermId) ? termNameCache[record.TermId] : "",
                RuleName = ruleNameCache != null && ruleNameCache.ContainsKey(record.RuleId) ? ruleNameCache[record.RuleId].RuleName : "",
                Status = JobDetailsStatus.Failed,
                Comment = ex?.Message
            });
        }

        private RMPhysicalNodeFlag GenerateNodeFlag(CustomizeConnectorInfo connector)
        {
            RMPhysicalNodeFlag nodeFlag = new RMPhysicalNodeFlag();
            nodeFlag.CollectionTime = mRunJobTime.Ticks;
            nodeFlag.FullPath = connector.Name;
            nodeFlag.IsRemoved = false;
            nodeFlag.NodeFlagType = (int)NodeFlagType.ConnectorTimer;
            nodeFlag.NodeId = connector.Id;
            nodeFlag.Title = connector.Name;
            return nodeFlag;
        }
    }
}
