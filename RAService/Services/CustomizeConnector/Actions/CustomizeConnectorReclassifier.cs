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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager;
using AvePoint.RA.Service.Services.CustomizeConnector.RuleManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.Actions
{
    public class CustomizeConnectorReclassifier
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(CustomizeConnectorReclassifier));

        private static IRMRecordsUpdateTempDao RecordsUpdateTempDao =>
            PlatformWindsorManager.GetService<IRMRecordsUpdateTempDao>();

        private static IRMClassificationHistoryDao ClassificationHistoryDao =>
            PlatformWindsorManager.GetService<IRMClassificationHistoryDao>();

        private static IRecordsHistoryService RecordsHistoryService =>
            PlatformWindsorManager.GetService<IRecordsHistoryService>();



        private static IRMCustomizeConnectorService CustomizeConnectorService =>
            PlatformWindsorManager.GetService<IRMCustomizeConnectorService>();

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private ITenantService mTenantService;
        public ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }

        private readonly ConnectorTermRuleInfoManagement RuleInfoManager = new ();

        private readonly Dictionary<Guid, ConnectorColumnManager> ColumnManagers = new();

        private readonly ChangeTermOption ChangeTermInfo;

        private readonly string JobId;

        private readonly bool IsRunOnJob;

        public int FailedItemsCount { get; private set; }

        private bool isNewLogicAccount;

        public CustomizeConnectorReclassifier(ChangeTermOption changeTermInfo, string jobId, bool isRunOnJob)
        {
            ChangeTermInfo = changeTermInfo;
            JobId = jobId;
            IsRunOnJob = isRunOnJob;
            isNewLogicAccount = TenantService.IsNewOpusTenant();
        }

        public async System.Threading.Tasks.Task ReclassifyAsync()
        {
            try
            {
                using (new PerformanceScope("CustomizeConnector.Reclassify"))
                {
                    Logger.Info("Is new logic account is {0}", isNewLogicAccount);
                    Logger.Info($"Start process reclassify action.");
                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Running);
                    var needProcessItemIds = ChangeTermInfo.SourceCustomizeConnectorRecordIds;
                    if (needProcessItemIds?.Count == 0)
                    {
                        Logger.Warn($"Has't need process azure file share items.");
                        RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Finished);
                        return;
                    }

                    var items = ExplorerDao.QueryAll(item => needProcessItemIds.Contains(item.Id) && item.NodeType == (int)RMNodeLevel.CustomizeConnectorItem).ToList();

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(items.Select(item => item.LeafName)));

                    await ReclassifyAsync(items);

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Finished);
                    Logger.Info($"Successful process reclassify action.");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute reclassify action. Error: {e}");
                RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
            }
        }

        private async System.Threading.Tasks.Task ReclassifyAsync(List<Record> items)
        {
            Logger.Info($"Need process reclassify action items count: [{items.Count}].");
            var succeedItems = new List<Record>();
            var failedItems = new List<Record>();

            foreach (var item in items)
            {
                try
                {
                    if (item.NodeType != (int)RMNodeLevel.CustomizeConnectorItem)
                    {
                        Logger.Warn($"Customize connector item reclassify action not support except file node type [{item.Id}].");
                        continue;
                    }
                    var previousTermId = item.TermId;
                    var manualReviewer = item.ManualReviewer;
                    var disposalStatus = item.DisposalStatus;
                    var isManualSynced = item.IsManualSynced;
                    item.TermId = ChangeTermInfo.TargetTermUniqueId;
                    item.TermName = ChangeTermInfo.TargetTermName;
                    if(isNewLogicAccount && previousTermId != ChangeTermInfo.TargetTermUniqueId) item.RemoveManualFields();

                    var columnManager = await GetConnectorColumnManagerAsync(new Guid(item.ContainerId));
                    RuleInfoManager.ApplyRule(item, columnManager.ConvertToRulePolicy(item.CustomColumnDic));

                    if (isManualSynced)
                    {
                        item.ManualReviewer = manualReviewer;
                        item.DisposalStatus = disposalStatus;
                        item.IsManualSynced = isManualSynced;
                    }

                    ExplorerDao.Upsert(item);

                    AddSucceedDetail(item, previousTermId);

                    succeedItems.Add(item);
                    Logger.Info($"Succeed process record [{item.Id}] reclassify action.");
                }
                catch (Exception e)
                {
                    AddFailedDetail(item);

                    failedItems.Add(item);
                    Logger.Error($"An error occurred while process record [{item.Id}] reclassify action. Error: {e}");
                }
            }

            AddProcessReclassifyItemsToHistory(succeedItems, failedItems);
        }

        private async Task<ConnectorColumnManager> GetConnectorColumnManagerAsync(Guid connectorId)
        {
            if(!ColumnManagers.TryGetValue(connectorId, out var columnManager))
            {
                var connectorInfo = await CustomizeConnectorService.GetAsync(connectorId);
                columnManager = new (connectorInfo.ColumnInfoes);
                ColumnManagers[connectorId] = columnManager;
            }

            return columnManager;
        }

        private void AddSucceedDetail(Record item, Guid previousTermId)
        {
            try
            {
                ClassificationHistoryDao.Create(new DB.Model.RMClassificationHistory
                {
                    RecordId = item.Id,
                    PreviousTermId = previousTermId,
                    NewTermId = item.TermId,
                    OperationTime = DateTime.UtcNow.Ticks
                });
                Logger.Info($"Succeed add item [{item.Id}] reclassify action to history.");

                if (IsRunOnJob)
                {
                    ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = item.LeafName,
                        FullPath = "",
                        Action = "RM_JS_BCM_Explorer_ChangeTerm",
                        Status = JobDetailsStatus.Successful,
                        Type = "RM_Connector_ItemLevel_Item"
                    });
                }

                Logger.Info($"Add item [{item.Id}] succeed detail completed.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while add item [{item.Id}] succeed detail. Error: {e}");
            }
        }

        private void AddFailedDetail(Record item)
        {
            try
            {
                if (IsRunOnJob)
                {
                    ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = item.LeafName,
                        FullPath = "",
                        Action = "RM_JS_BCM_Explorer_ChangeTerm",
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JM_GlobalSearch_ChangeTermFailed",
                        Type = "RM_Connector_ItemLevel_Item"
                    });
                }

                Logger.Info($"Add item [{item.Id}] failed detail completed.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while add item [{item.Id}] failed detail. Error: {e}");
            }
        }

        private void AddProcessReclassifyItemsToHistory(List<Record> succeedItems, List<Record> failedItems)
        {
            try
            {
                if (succeedItems.Any())
                {
                    RecordsHistoryService.AddRecordsHistory(succeedItems.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_ChangeTerm", ChangeTermInfo.Comment);
                }

                if (failedItems.Any())
                {
                    FailedItemsCount += failedItems.Count;
                    RecordsHistoryService.AddRecordsHistory(failedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeTermErrorMessage");
                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, string.Join(";", failedItems.Select(item => item.LeafName)), RecordsConstants.Explorer_RealTime_Failed_Partial);
                }

                Logger.Info($"Succeed add process reclassify items to history.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while add process reclassify items to history. Error: {e}");
            }
        }
    }
}
