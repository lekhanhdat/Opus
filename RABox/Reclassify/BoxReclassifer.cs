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
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using RABox.Converters;
using RABox.RuleManagement;
using RABox.Util;

namespace RABox.Reclassify
{
    public class BoxReclassifier
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(BoxReclassifier));

        private static IRMRecordsUpdateTempDao RecordsUpdateTempDao => PlatformWindsorManager.GetService<IRMRecordsUpdateTempDao>();

        private static IRMClassificationHistoryDao ClassificationHistoryDao => PlatformWindsorManager.GetService<IRMClassificationHistoryDao>();

        private static IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        private static ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private static IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private static ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        private IExplorerDao? _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
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


        private readonly string JobId;

        private readonly bool IsRunOnReclassifyJob;
        public int FailedItemsCount { get; private set; }

        private readonly ChangeTermOption ChangeTermInfo;

        private readonly SettingManager SettingManager;

        private RecordsReturnMessage recordsReturnMessage = new RecordsReturnMessage { ResultType = ResultType.Failed };
        private bool isNewLogicAccount;

        public BoxReclassifier(ChangeTermOption changeTermInfo, string jobId, bool isRunOnReclassifyJob)
        {
            ChangeTermInfo = changeTermInfo;
            JobId = jobId;
            IsRunOnReclassifyJob = isRunOnReclassifyJob;
            SettingManager = new SettingManager();
            isNewLogicAccount = TenantService.IsNewOpusTenant();
        }
        public RecordsReturnMessage Reclassify()
        {
            try
            {
                using (new PerformanceScope("Box.Reclassify"))
                {
                    Logger.Info("Is new logic account is {0}", isNewLogicAccount);
                    Logger.Info($"Start process reclassify action.");

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Running);

                    var needProcessItemIds = ChangeTermInfo.SourceBoxRecordIds;

                    if (!needProcessItemIds.Any())
                    {
                        Logger.Warn($"Has't need process box items.");
                        RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Finished);
                        return recordsReturnMessage;
                    }

                    var items = ExplorerDao.QueryAll(item => needProcessItemIds.Contains(item.Id) && item.NodeType == (int)RMNodeLevel.BoxFile).ToList();

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(items.Select(item => item.LeafName)));

                    var rules = GetTermMatchedRules();

                    ProcessReclassify(items, rules);

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Finished);

                    Logger.Info($"Successful process reclassify action.");
                }
            }
            catch (Exception e)
            {
                RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
                Logger.Error($"An error occurred while execute reclassify action. Error: {e}");
                return recordsReturnMessage;
            }

            return recordsReturnMessage;
        }


        private void ProcessReclassify(List<Record> items, List<Rule> rules)
        {
            Logger.Info($"Need process reclassify action items count: [{items.Count}], target term: [{ChangeTermInfo.TargetTermUniqueId}], [{ChangeTermInfo.TargetTermName}].");
            var succeedItems = new List<Record>();
            var failedItems = new List<Record>();
            foreach (var item in items)
            {
                try
                {
                    Logger.Info($"Start reclassify item [{item.Id}] with term [{item.TermId}], [{item.TermName}]");
                    var previousTermId = item.TermId;

                    var targetTermId = ChangeTermInfo.TargetTermUniqueId;

                    //if (previousTermId == targetTermId)
                    //{
                    //    AddReclassifyDetailToClassificationHistory(item);
                    //    succeedItems.Add(item);
                    //    Logger.Warn($"{item.Id} is already classify in current Term. Previous term [{item.TermId}], Target term [{ChangeTermInfo.TargetTermUniqueId}]");
                    //    continue;
                    //}

                    List<Guid> subTermUniqueIds = new List<Guid>();


                    var (hasSetting, setting) = SettingManager.TryGetSettingInfoByAncestorIds(item.Ancestors);

                    if (!hasSetting || setting == null)
                    {
                        AddReclassifyDetailToClassificationHistory(item);
                        failedItems.Add(item);
                        Logger.Warn($"Could not found setting scope with current item: Id [{item.Id}], ScopeID [{item.ScopeId}], ContainerID [{item.ContainerId}].");
                        continue;
                    }

                    if (setting.TermId != Guid.Empty)
                    {
                        subTermUniqueIds = TermDao.GetAllSubTermUniqueIdsByTermId(setting.TermId);

                        bool isExist = subTermUniqueIds.Any(id => id == targetTermId);

                        bool isSelectedRootTerm = setting.TermId == targetTermId;

                        if (!isSelectedRootTerm)
                        {
                            if (!isExist)
                            {
                                AddReclassifyDetailToClassificationHistory(item);
                                failedItems.Add(item);
                                Logger.Warn($"Cannot reclassify a Term outside of a setting scope in content source.");
                                continue;
                            }
                        }
                    }
                    else
                    {
                        subTermUniqueIds = TermDao.GetAllSubTermUniqueIdsByTermSetId(setting.TermSetId);

                        bool isExist = subTermUniqueIds.Any(id => id == targetTermId);

                        if (!isExist)
                        {
                            AddReclassifyDetailToClassificationHistory(item);
                            failedItems.Add(item);
                            Logger.Warn($"Cannot reclassify a Term outside of a setting scope in content source.");
                            continue;
                        }
                    }

                    item.TermId = targetTermId;
                    item.TermName = ChangeTermInfo.TargetTermName;
                    if(isNewLogicAccount && previousTermId != targetTermId) item.RemoveManualFields();

                    var boxItemInfo = item.ConvertBoxItemInfo();

                    var boxRuleManagement = new BoxRuleManagement(rules);

                    boxRuleManagement.ApplyRuleInfo(boxItemInfo, item);

                    ExplorerDao.AddOrUpdateRecordWithKeepManual(item, true, isKeepManualColumn: false);

                    AddReclassifyDetailToClassificationHistory(item, previousTermId, true);

                    succeedItems.Add(item);

                    Logger.Info($"Succeed process reclassify action on record [{item.Id}].");
                }
                catch (Exception e)
                {
                    AddReclassifyDetailToClassificationHistory(item);
                    failedItems.Add(item);
                    recordsReturnMessage.ResultType = ResultType.Failed;
                    Logger.Error($"An error occurred while process record [{item.Id}] reclassify action. Error: {e}");
                }
            }

            AddReclassifyItemsToRecordHistory(succeedItems, failedItems);
        }

        private List<Rule> GetTermMatchedRules()
        {
            var termRelatedRuleInfoes = TermRuleAssociationDao.GetTermRuleInfoByTermUniqueId(ChangeTermInfo.TargetTermUniqueId);

            if (termRelatedRuleInfoes.Count == 0)
            {
                Logger.Warn($"Current term [{ChangeTermInfo.TargetTermUniqueId}] not found related rule infoes.");
                return new List<Rule>();
            }

            var ruleIds = termRelatedRuleInfoes.Select(item => item.RuleId).ToList();

            Logger.Info($"Term [{ChangeTermInfo.TargetTermUniqueId}] related rules [{string.Join(", ", ruleIds)}].");

            var rules = RuleManagerService.GetRulesByIds(ruleIds)
                        .Where(item => item.BoxRule != null)
                        .OrderBy(item => termRelatedRuleInfoes.First(i => i.RuleId.ToString() == item.Id).RuleOrder)
                        .ToList();

            return rules;
        }

        private void AddReclassifyDetailToClassificationHistory(Record item, Guid? previousTermId = null, bool isSucceedAction = false)
        {
            try
            {
                if (isSucceedAction && previousTermId != null)
                {
                    RMClassificationHistory classificationHistory = new RMClassificationHistory
                    {
                        RecordId = item.Id,
                        PreviousTermId = (Guid)previousTermId,
                        NewTermId = item.TermId,
                        OperationTime = DateTime.UtcNow.Ticks
                    };

                    ClassificationHistoryDao.Create(classificationHistory);

                    Logger.Info($"Succeed add item [{item.Id}] reclassify action to history.");
                }

                if (IsRunOnReclassifyJob)
                {
                    ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = item.LeafName,
                        Action = I18NResource.ExplorerChangeTerm,
                        Status = isSucceedAction ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                        Comment = isSucceedAction ? "" : I18NResource.ChangeTermFailed,
                        Type = I18NResource.ObjectLevelDocument,
                        FullPath = item.DirPath,
                    }); ;

                    Logger.Info($"Add item [{item.Id}] {0} detail to history completed.", isSucceedAction ? "succeed" : "failed");
                }

            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while add item [{item.Id}] succeed detail. Error: {e}");
            }
        }

        private void AddReclassifyItemsToRecordHistory(List<Record> succeedItems, List<Record> failedItems)
        {
            try
            {
                if (succeedItems.Any())
                {
                    recordsReturnMessage.ResultType = ResultType.Success;
                    RecordsHistoryService.AddRecordsHistory(succeedItems.Select(item => item.Id).ToList(), I18NResource.AuditChangeTerm, ChangeTermInfo.Comment);
                }

                if (failedItems.Any())
                {
                    FailedItemsCount += failedItems.Count;
                    RecordsHistoryService.AddRecordsHistory(failedItems.Select(item => item.Id).ToList(), I18NResource.AuditChangeTermErrorMessage);
                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, string.Join(";", failedItems.Select(item => item.LeafName)), RecordsConstants.Explorer_RealTime_Failed_Partial);
                }

                Logger.Info($"Succeed add process reclassify items to record history.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while add process reclassify items to record history. Error: {e}");
            }
        }

      
    }
}
