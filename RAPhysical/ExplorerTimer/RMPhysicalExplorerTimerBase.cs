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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Disposal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.Common;

namespace AvePoint.RA.RAPhysical.ExplorerTimer
{
    public class RMPhysicalExplorerTimerBase
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMPhysicalExplorerTimerBase));
        private List<Guid> changeTermIds = new List<Guid>();
        private List<Guid> releaseHoldObjectIds = new List<Guid>();
        //private Expression<Func<Record, bool>> filterLambda;
        private int pendingStatus = (int)DueDateUtil.Pending;
        private DateTime mRunJobTime;
        private Dictionary<int, List<RMPhysicalColumnChangeLog>> changedTemplates = new Dictionary<int, List<RMPhysicalColumnChangeLog>>();

        private IPhysicalRecordSettingDao mPhysicalRecordSettingDao;
        public IPhysicalRecordSettingDao PhysicalRecordSettingDao
        {
            get
            {
                if (mPhysicalRecordSettingDao == null)
                {
                    mPhysicalRecordSettingDao = (IPhysicalRecordSettingDao)PlatformWindsorManager.GetService(typeof(IPhysicalRecordSettingDao));
                }
                return mPhysicalRecordSettingDao;
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
        private IRecordAllianceDao mRecordAllianceDao;
        public IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mRecordAllianceDao == null)
                {
                    mRecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return mRecordAllianceDao;
            }
        }

        private IRMPhysicalColumnChangeLogDao mPhysicalColumnChangeLogDao { get; set; }
        protected IRMPhysicalColumnChangeLogDao PhysicalColumnChangeLogDao
        {
            get
            {
                if (mPhysicalColumnChangeLogDao == null)
                {
                    mPhysicalColumnChangeLogDao = (IRMPhysicalColumnChangeLogDao)PlatformWindsorManager.GetService(typeof(IRMPhysicalColumnChangeLogDao));
                }
                return mPhysicalColumnChangeLogDao;
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

        private ITemplateManagementService mTemplateManagementService { get; set; }
        public ITemplateManagementService TemplateManagementService
        {
            get
            {
                if (mTemplateManagementService == null)
                {
                    mTemplateManagementService = (ITemplateManagementService)PlatformWindsorManager.GetService(typeof(ITemplateManagementService));
                }
                return mTemplateManagementService;
            }
        }

        private IRMTemplateDao mTemplateDao { get; set; }
        public IRMTemplateDao TemplateDao
        {
            get
            {
                if (mTemplateDao == null)
                {
                    mTemplateDao = (IRMTemplateDao)PlatformWindsorManager.GetService(typeof(IRMTemplateDao));
                }
                return mTemplateDao;
            }
        }

        private IRMPhysicalPushColumnDao mRMPhysicalPushColumnDao;
        public IRMPhysicalPushColumnDao RMPhysicalPushColumnDao
        {
            get
            {
                if (mRMPhysicalPushColumnDao == null)
                {
                    mRMPhysicalPushColumnDao = (IRMPhysicalPushColumnDao)PlatformWindsorManager.GetService(typeof(IRMPhysicalPushColumnDao));
                }
                return mRMPhysicalPushColumnDao;
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

        public bool HasError { get; set; } = false;

        public async Task RunNowAsync()
        {
            mRunJobTime = DateTime.UtcNow;
            //UpdateHeldItems();
            logger.Info($"Begin load all settings.");
            var allSettings = PhysicalRecordSettingDao.LoadAllSetting();
            logger.Info($"Load all settings finished, setting count : {allSettings?.Count}.");
            ArgumentCheck.NotNull(allSettings, nameof(allSettings));
            foreach (var setting in allSettings)
            {
                try
                {
                    var locationId = setting.LocationUniqueId;
                    IPhysicalLocation currentLocation = new PhysicalLocation(locationId);
                    if (currentLocation.Exist)
                    {
                        logger.Info($"Begin process setting : {setting.Id}, Location id: {currentLocation?.UniqueId}, .");
                        //跑Job之前，就获取一下上次sync 时间，这样才能正确处理Term Rule 变化的case
                        var nodeInfo = PhysicalNodeInfoDao.GetPhysicalNodeInfo(currentLocation.UniqueId, Guid.Empty, (int)NodeFlagType.ExplorerSync);
                        long collectionTime = DateTime.MinValue.Ticks;
                        if (nodeInfo != null)
                        {
                            collectionTime = nodeInfo.CollectionTime;
                        }
                        changeTermIds = ExplorerService.GetChangeTermIds(collectionTime).Distinct().ToList();
                        var changedColumns = this.GetChangedColumns(collectionTime);
                        logger.Info($"Changed columns count : {changedColumns.Count}, Ids are : {string.Join(";", changedColumns.Select(c => c.ColumnUniqueId))}.");
                        changedTemplates = changedColumns.GroupBy(c => c.TemplateId).ToDictionary(col => col.Key, col => col.OrderBy(column => column.ActionTime).ToList());
                        //filterLambda = GetFilterLambdaForPhysical(changedTemplates);
                        //logger.Info($"The Express is : {filterLambda.ToString()}.");
                        //if (changeTermId.Count > 0)
                        //{
                        await ProcessLocationAsync(currentLocation);
                        //}
                        //else
                        //{
                        //    logger.Info($"No changed term. no need to sync for location : {currentLocation.DirPath}.");
                        //}
                        //Need to distinct the return change columns laster
                        //var changeColumns = PhysicalColumnChangeLogDao.GetChangedColumns(collectionTime);
                        //var changeTemplateIds = changeColumns.Select(r => r.TemplateId).Distinct();
                        //foreach(var changeTemplateId in changeTemplateIds)
                        //{

                        //}

                        PhysicalNodeInfoDao.AddPhysicalNodeInfo(GenerateNodeFlag(currentLocation));
                        logger.Info($"Finish process setting : {setting.Id}.");
                    }
                    else
                    {
                        logger.Info($"The location : {locationId} does not exist, setting id is : {setting.Id}.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in run timer job for setting : {setting.Id}, reason : {ex.ToString()}.");
                }
            }
        }

        private Expression<Func<Record, bool>> GetFilterLambdaForPhysical(Dictionary<int, List<RMPhysicalColumnChangeLog>> changeTemplates, List<Guid> termIds)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            #region Node Status condition
            List<Expression> nodeStatusExpressionList = new List<Expression>();
            nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Active));
            nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Closed));
            allExpressionList.Add(nodeStatusExpressionList.Aggregate(Expression.OrElse));
            List<Expression> otherExpressionList = new List<Expression>();
            #endregion
            #region ChangeTermIds
            if (termIds.Count > 0)
            {
                List<Expression> changeTermExpressionList = new List<Expression>();
                foreach (var id in termIds)
                {
                    changeTermExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "TermId", id));
                }
                otherExpressionList.Add(changeTermExpressionList.Aggregate(Expression.OrElse));
            }
            #endregion
            #region Release hold items
            if (releaseHoldObjectIds.Count > 0)
            {
                List<Expression> releaseHoldItemExpressionList = new List<Expression>();
                foreach (var id in releaseHoldObjectIds)
                {
                    releaseHoldItemExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "Id", id));
                }
                otherExpressionList.Add(releaseHoldItemExpressionList.Aggregate(Expression.OrElse));
            }
            #endregion
            #region Pending Status
            otherExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "DisposalDueDate", pendingStatus));
            #endregion
            #region Changed Column condition
            if (changeTemplates != null && changeTemplates.Count > 0)
            {
                List<Expression> changedColumnExpressionList = new List<Expression>();
                foreach (var changedTemplateId in changeTemplates.Keys)
                {
                    changedColumnExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "TemplateId", changedTemplateId));
                }
                otherExpressionList.Add(changedColumnExpressionList.Aggregate(Expression.OrElse));
            }
            #endregion
            allExpressionList.Add(otherExpressionList.Aggregate(Expression.OrElse));
            queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<Record, bool>>(queryExpr, param);
        }



        private List<RMPhysicalColumnChangeLog> GetChangedColumns(long lastJobRunTime)
        {
            return PhysicalColumnChangeLogDao.GetChangedColumns(lastJobRunTime);
        }

        private async Task ProcessLocationAsync(IPhysicalLocation physicalLocation)
        {
            using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.ProcessLocation", addToStatistics: true))
            {
                try
                {
                    if (physicalLocation.IsBottomLocation)
                    {
                        logger.Info($"Process bottom location uniqueId: {physicalLocation.UniqueId}.  changedtermid count {changeTermIds.Count}");
                        //Changed Term可能存在大数据， 一次处理会导致SQL过长， changed in CI Nov 2021
                        int pageSize = 500;
                        if (changeTermIds.Count <= pageSize)
                        {
                            var filter = GetFilterLambdaForPhysical(changedTemplates, changeTermIds);
                            logger.Info($"The Express is : {filter.ToString()}.");
                            Dictionary<Contract.RMWeb.Tree.Base.RMNodeLevel, List<Object>> queryResult = null;
                            using (new PerformanceScope("RMPhysicalExplorerTimerBase.Query", addToStatistics: true))
                            {
                                queryResult = physicalLocation.Query(filter);
                            }
                            await InnerProcessLocationAsync(queryResult);
                        }
                        else
                        {
                            List<Guid> tempIds = null;
                            int index = 0;
                            do
                            {
                                tempIds = changeTermIds.Skip(index * pageSize).Take(pageSize).ToList();
                                if (tempIds.Count > 0)
                                {
                                    var filter = GetFilterLambdaForPhysical(changedTemplates, tempIds);
                                    logger.Info($"Index {index}, The Express is : {filter.ToString()}.");
                                    Dictionary<Contract.RMWeb.Tree.Base.RMNodeLevel, List<Object>> queryResult = null;
                                    using (new PerformanceScope("RMPhysicalExplorerTimerBase.Query", addToStatistics: true))
                                    {
                                        queryResult = physicalLocation.Query(filter);
                                    }
                                    await InnerProcessLocationAsync(queryResult);
                                }
                                index++;
                            } while (tempIds.Count > 0);
                        }
                    }
                    else
                    {
                        logger.Info($"Process location uniqueId: {physicalLocation?.UniqueId}.");
                        foreach (var location in physicalLocation.AllSubLocations)
                        {
                            var setting = PhysicalRecordSettingDao.GetPhysicalRecordSetting(location.UniqueId);
                            if (setting != null)
                            {
                                logger.Info($"Location uniqueId: {location?.UniqueId} has setting,  so skip it in current node.");
                            }
                            else
                            {
                                await ProcessLocationAsync(location);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in Process Location : {physicalLocation.DirPath}, reason : {ex.ToString()}.");
                    HasError = true;
                    ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                    {
                        ObjectName = physicalLocation.Name,
                        FullPath = physicalLocation.DirPath,
                        ItemType = "location",
                        RuleName = "",
                        Status = JobDetailsStatus.Failed,
                        Comment = ex.Message,
                    });
                }
            }
        }

        private async Task InnerProcessLocationAsync(Dictionary<Contract.RMWeb.Tree.Base.RMNodeLevel, List<Object>> queryResult)
        {
            List<IPhysicalBox> queryBoxes = queryResult[Contract.RMWeb.Tree.Base.RMNodeLevel.PhysicalBox].ConvertAll(b => b as IPhysicalBox);
            List<IPhysicalFile> queryFolders = queryResult[Contract.RMWeb.Tree.Base.RMNodeLevel.PhysicalFile].ConvertAll(b => b as IPhysicalFile);
            List<IPhysicalRecord> queryRecords = queryResult[Contract.RMWeb.Tree.Base.RMNodeLevel.PhysicalRecord].ConvertAll(b => b as IPhysicalRecord);
            ReportManager.IncreaseBase(queryBoxes.Count);
            foreach (var box in queryBoxes)
            {
                await ProcessBoxAsync(box);
            }
            ReportManager.IncreaseBase(queryFolders.Count);
            foreach (var folder in queryFolders)
            {
                await ProcessFolderAsync(folder);
            }
        }

        private async Task ProcessBoxAsync(IPhysicalBox physicalBox)
        {
            using (new PerformanceScope("RMPhysicalExplorerTimerBase.ProcessBox", addToStatistics: true))
            {
                logger.Info($"Process physical box id: {physicalBox?.Id}.");
                ArgumentCheck.NotNull(physicalBox, nameof(physicalBox));
                ReportManager.Increase(1);
                var boxTermId = physicalBox.TermId;
                List<Rule> rules;
                var dueDisposalTime = string.Empty;
                try
                {
                    Rule physicalRule = null;
                    var dueDate = DueDateUtil.None;
                    if (RMPhysicalDisposalCache.Instance.TermRuleMapping.TryGetValue(boxTermId, out rules))
                    {
                        //此处逻辑需要优化，避免每个template 都获取一次，应该做global 级别的缓存
                        var columnCollection = new Dictionary<Guid, TemplateColumnDto>();
                        PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
                        Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn = new Dictionary<Guid, List<RMPhysicalPushColumn>>();
                        using (new PerformanceScope("RMPhysicalExplorerTimerBase.LoadBoxTemplateDto", addToStatistics: true))
                        {
                            var template = await TemplateManagementService.LoadTemplateDtoAsync(physicalBox.TemplateId);

                            template.categories.ForEach(cat =>
                            {
                                cat.columns.ForEach(col => columnCollection[col.uniqueId] = col);
                            });

                            foreach (var fieldKey in physicalBox.Fields.Keys)
                            {
                                Guid fieldId;
                                if (Guid.TryParse(fieldKey, out fieldId))
                                {
                                    if (columnCollection != null && columnCollection.ContainsKey(fieldId))
                                    {
                                        var column = columnCollection[fieldId];
                                        if (column.pushToChild)
                                        {
                                            List<Guid> physicObjectIds = new List<Guid>();
                                            physicObjectIds.Add(physicalBox.Id);
                                            List<RMPhysicalPushColumn> pushColumn = RMPhysicalPushColumnDao.GetPushColumns(column.uniqueId, physicObjectIds);
                                            columnIdAndPushColumn[column.uniqueId] = pushColumn;
                                        }
                                    }
                                }
                            }
                        }
                        var boxFilterObj = PhysicalObjectConvertor.ConvertPhysicalBoxFilterObject(engine.FilterPolicyCollection, physicalBox, columnCollection, columnIdAndPushColumn);
                        using (new PerformanceScope("RMPhysicalExplorerTimerBase.CheckBoxRule", addToStatistics: true))
                        {
                            physicalRule = engine.CheckRule(boxFilterObj);
                        }

                        if (physicalRule != null)
                        {
                            dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                        }
                        else
                        {
                            using (new PerformanceScope("RMPhysicalExplorerTimerBase.CheckBoxDueDisposalRule", addToStatistics: true))
                            {
                                physicalRule = engine.CheckDueDisposalRule(physicalBox, boxFilterObj, ref dueDisposalTime);
                            }
                            if (dueDisposalTime != string.Empty && physicalRule.IsLastestSubFolderActionDueDateRule() && !physicalBox.AreAllFolderRulesCalculateRule())
                            {
                                dueDisposalTime = string.Empty;
                                physicalRule = null;
                            }
                        }

                        if (physicalRule.IsPhysicalMoveToRule() && physicalBox.BoxUnderContainer())
                        {
                            logger.Info($"No need to process physical box id'{physicalBox?.Id}' because it is under container.");
                            using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.BoxUpdateRuleId", addToStatistics: true))
                            {
                                logger.Info("Matched rule is moveto rule and current record is under container, will clear rule id.");
                                physicalBox.RuleId = Guid.Empty;
                                physicalBox.DisposalDueDate = DueDateUtil.None;
                                physicalBox.PreviousDisposalDueDate = DueDateUtil.None;
                                physicalBox.Update();
                            }
                            ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                            {
                                ObjectName = physicalBox.Name,
                                FullPath = physicalBox.DirPath,
                                ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                                RuleName = physicalRule?.Name,
                                Status = JobDetailsStatus.Skipped,
                                Comment = "RM_PRM_Disposal_SkipBoxUnderContainer",
                            });
                            return;
                        }
                        var disposalDueTime = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime);
                        dueDate = disposalDueTime;
                        physicalBox.RuleId = physicalRule != null ? new Guid(physicalRule.Id) : Guid.Empty;
                    }
                    else
                    {
                        logger.Info($"Cannot find rule on term : {boxTermId}");
                        dueDate = DueDateUtil.None;
                        physicalBox.RuleId = Guid.Empty;
                    }

                    var isHold = physicalBox.HoldStatus;
                    if (!isHold)
                    {
                        physicalBox.DisposalDueDate = dueDate;
                        physicalBox.PreviousDisposalDueDate = dueDate;
                        physicalBox.HoldStatus = false;
                        physicalBox.HoldReleaseTime = 0;
                        physicalBox.HoldBy = string.Empty;
                        physicalBox.HoldId = string.Empty;
                    }
                    else
                    {
                        var finalDuedate = CalculateDueDate(physicalBox.HoldReleaseTime, physicalRule, dueDate);
                        physicalBox.PreviousDisposalDueDate = dueDate;
                        physicalBox.DisposalDueDate = finalDuedate;
                    }

                    await UpdateChangeColumnsAsync(physicalBox.TemplateId, physicalBox.Id, physicalBox);

                    using (var performance = new PerformanceScope("Phy.RMPhysicalExplorerTimerBase.BoxUpdate", addToStatistics: true))
                    {
                        physicalBox.Update();
                    }
                    ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                    {
                        ObjectName = physicalBox.Name,
                        FullPath = physicalBox.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                        RuleName = physicalRule?.Name,
                        Status = JobDetailsStatus.Successful,
                        Comment = "",
                    });
                }
                catch (Exception exp)
                {
                    logger.Error($"Error in process box : {physicalBox.DirPath}. reason : {exp.ToString()}.");
                    HasError = true;
                    ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                    {
                        ObjectName = physicalBox.Name,
                        FullPath = physicalBox.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                        RuleName = "",
                        Status = JobDetailsStatus.Failed,
                        Comment = exp.Message,
                    });
                }
            }
        }

        private async Task ProcessFolderAsync(IPhysicalFile physicalFile)
        {
            using (var performance = new PerformanceScope("RMPhysicalExplorerTimerBase.ProcessFolder", addToStatistics: true))
            {
                logger.Info($"Process physical file id: {physicalFile?.Id}.");
                ReportManager.Increase(1);
                ArgumentCheck.NotNull(physicalFile, nameof(physicalFile));
                var fileTermId = physicalFile.TermId;
                List<Rule> rules;
                var dueDisposalTime = string.Empty;
                try
                {
                    Rule physicalRule = null;
                    var dueDate = DueDateUtil.None;
                    if (RMPhysicalDisposalCache.Instance.TermRuleMapping.TryGetValue(fileTermId, out rules))
                    {
                        PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
                        Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn = new Dictionary<Guid, List<RMPhysicalPushColumn>>();
                        var columnCollection = new Dictionary<Guid, TemplateColumnDto>();
                        //此处逻辑需要优化，避免每个template 都获取一次，应该做global 级别的缓存
                        using (var performance0 = new PerformanceScope("RMPhysicalExplorerTimerBase.LoadFolderTemplateDto", addToStatistics: true))
                        {
                            var template = await TemplateManagementService.LoadTemplateDtoAsync(physicalFile.TemplateId);
                            if (physicalFile.BoxId != Guid.Empty)
                            {
                                using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.AddPushColumnToFold", addToStatistics: true))
                                {
                                    ExplorerService.AddPushColumnToFold(template, physicalFile.BoxId);
                                }
                            }                            
                            template.categories.ForEach(cat =>
                            {
                                cat.columns.ForEach(col => columnCollection[col.uniqueId] = col);
                            });
                         
                            foreach (var fieldKey in physicalFile.Fields.Keys)
                            {
                                Guid fieldId;
                                if (Guid.TryParse(fieldKey, out fieldId))
                                {
                                    if (columnCollection != null && columnCollection.ContainsKey(fieldId))
                                    {
                                        var column = columnCollection[fieldId];
                                        if (column.pushToChild)
                                        {
                                            List<Guid> physicObjectIds = new List<Guid>();
                                            if (column.inheritFromParent)
                                            {
                                                physicObjectIds.Add(physicalFile.BoxId);
                                            }
                                            else
                                            {
                                                physicObjectIds.Add(physicalFile.Id);
                                            }
                                            List<RMPhysicalPushColumn> pushColumn = RMPhysicalPushColumnDao.GetPushColumns(column.uniqueId, physicObjectIds);
                                            columnIdAndPushColumn[column.uniqueId] = pushColumn;
                                        }
                                    }
                                }
                            }
                        }
                        var fileFilterObj = PhysicalObjectConvertor.ConvertPhysicalFileFilterObject(engine.FilterPolicyCollection, physicalFile, columnCollection, columnIdAndPushColumn);
                        using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.CheckFolderRule", addToStatistics: true))
                        {
                            physicalRule = engine.CheckRule(fileFilterObj);
                        }
                        if (physicalRule != null)
                        {
                            if (physicalRule.PhysicalRule.IsCalculationDisposalDate)
                            {
                                logger.Info($"Folder {physicalFile.Name} match rule {physicalRule.PhysicalRule} is IsCalculationDisposalDate rule, keep rule Id, and action due date");
                                ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                                {
                                    ObjectName = physicalFile.Name,
                                    FullPath = physicalFile.DirPath,
                                    ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                                    RuleName = physicalRule?.Name,
                                    Status = JobDetailsStatus.Skipped,
                                    Comment = "RM_PRM_Disposal_SkipCalculateRule",
                                });
                                return;
                            }
                            else {

                            }
                            dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                        }
                        else
                        {
                            using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.CheckFolderDueDisposalRule", addToStatistics: true))
                            {
                                physicalRule = engine.CheckDueDisposalRule(physicalFile, fileFilterObj, ref dueDisposalTime);
                            }
                        }

                        if (physicalRule.IsPhysicalMoveToRule() && physicalFile.FolderUnderContainer())
                        {
                            logger.Info($"No need to process physical file id'{physicalFile?.Id}' because it is under container.");
                            using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.FolderUpdateRuleId", addToStatistics: true))
                            {
                                logger.Info("Matched rule is moveto rule and current record is under container, will clear rule id.");
                                physicalFile.RuleId = Guid.Empty;
                                physicalFile.DisposalDueDate = DueDateUtil.None;
                                physicalFile.PreviousDisposalDueDate = DueDateUtil.None;
                                physicalFile.Update();
                            }
                            ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                            {
                                ObjectName = physicalFile.Name,
                                FullPath = physicalFile.DirPath,
                                ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                                RuleName = physicalRule?.Name,
                                Status = JobDetailsStatus.Skipped,
                                Comment = "RM_PRM_Disposal_SkipFolderUnderContainer",
                            });
                            return;
                        }
                        var disposalDueTime = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime);
                        dueDate = disposalDueTime;
                        physicalFile.RuleId = physicalRule != null ? new Guid(physicalRule.Id) : Guid.Empty;
                    }
                    else
                    {
                        logger.Info($"Cannot find rule on term : {fileTermId}");
                        dueDate = DueDateUtil.None;
                        physicalFile.RuleId = Guid.Empty;
                    }
                    var ids = new List<Guid>();
                    ids.Add(physicalFile.Id);
                    ids.Add(physicalFile.BoxId);

                    using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.GetRecordAllianceByIds", addToStatistics: true))
                    {
                        var isHold = physicalFile.HoldStatus || (physicalFile.ParentBox != null && physicalFile.ParentBox.HoldStatus);
                        if (!isHold)
                        {
                            logger.Info($"File id: {physicalFile.Id} is on hold.");
                            physicalFile.DisposalDueDate = dueDate;
                            physicalFile.PreviousDisposalDueDate = dueDate;
                            physicalFile.HoldStatus = false;
                            physicalFile.HoldReleaseTime = 0;
                            physicalFile.HoldBy = string.Empty;
                            physicalFile.HoldId = string.Empty;
                        }
                        else
                        {
                            var finalDuedate = CalculateDueDate(physicalFile.HoldReleaseTime, physicalRule, dueDate);
                            physicalFile.PreviousDisposalDueDate = dueDate;
                            physicalFile.DisposalDueDate = finalDuedate;
                        }
                    }

                    await UpdateChangeColumnsAsync(physicalFile.TemplateId, physicalFile.Id, physicalFile);

                    using (var performance1 = new PerformanceScope("RMPhysicalExplorerTimerBase.FolderUpdate", addToStatistics: true))
                    {
                        physicalFile.Update();
                    }

                    ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                    {
                        ObjectName = physicalFile.Name,
                        FullPath = physicalFile.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                        RuleName = physicalRule?.Name,
                        Status = JobDetailsStatus.Successful,
                        Comment = "",
                    });
                }
                catch (Exception exp)
                {
                    logger.Error($"Error in process file : {physicalFile.DirPath}. reason : {exp.ToString()}.");
                    HasError = true;
                    ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                    {
                        ObjectName = physicalFile.Name,
                        FullPath = physicalFile.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                        RuleName = "",
                        Status = JobDetailsStatus.Failed,
                        Comment = exp.Message,
                    });
                }
            }
        }

        private async Task UpdateChangeColumnsAsync(int templateId, Guid physicalObjectId, IPhysicalFields fields)
        {
            using (var performance = new PerformanceScope("Phy.RMPhysicalExplorerTimerBase.UpdateChangeColumns", addToStatistics: true))
            {
                if (changedTemplates.ContainsKey(templateId))
                {
                    var changedColumns = changedTemplates[templateId];
                    //var template = TemplateDao.GetTemplateByIdToDto(templateId);
                    //var columnSchema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                    foreach (var changedColumn in changedColumns)
                    {
                        switch (changedColumn.Action)
                        {
                            case (int)ColumnChangeType.Deleted:
                            case (int)ColumnChangeType.CheckToUncheck:
                                await RMPhysicalPushColumnDao.DeletePushColumnAsync(changedColumn.ColumnUniqueId, physicalObjectId);
                                break;
                            case (int)ColumnChangeType.UncheckToChecked:
                            case (int)ColumnChangeType.NewAdded:
                                RMPhysicalPushColumnDao.AddOrUpdate(new RMPhysicalPushColumn()
                                {
                                    ColumnUniqueId = changedColumn.ColumnUniqueId,
                                    PhysicalObjectId = physicalObjectId,
                                    TemplateId = templateId,
                                    ColumnValue = fields[changedColumn.ColumnUniqueId.ToString()]
                                });
                                break;
                            default:
                                break;

                        }
                    }
                }
            }
        }

        private long CalculateDueDate(long holdReleaseTime, Rule rule, long disposalDueDate)
        {
            long caculateDisposalDueDate = disposalDueDate;
            if (rule != null && IsRemoveRule(rule))
            {
                //Remove Rule需要计算Due Date
                if (disposalDueDate == DueDateUtil.NextJob)
                {
                    caculateDisposalDueDate = holdReleaseTime;
                }
                if (disposalDueDate > 0)
                {
                    if (disposalDueDate > holdReleaseTime)
                    {
                        caculateDisposalDueDate = disposalDueDate;
                    }
                    else
                    {
                        caculateDisposalDueDate = holdReleaseTime;
                    }
                }
            }
            return caculateDisposalDueDate;
        }

        private bool IsRemoveRule(Rule rule)
        {
            bool isRemoveRule = false;
            //Current we only have move and remove action, this function need to do some change later
            if (rule.PhysicalRule.spMoveOption != null && rule.PhysicalRule.spMoveOption.MoveDestination != null)
            {
                isRemoveRule = false;
            }
            else
            {
                isRemoveRule = true;
            }
            return isRemoveRule;
        }

        private RMPhysicalNodeFlag GenerateNodeFlag(IPhysicalLocation physicalLocation)
        {
            RMPhysicalNodeFlag nodeFlag = new RMPhysicalNodeFlag();
            nodeFlag.CollectionTime = mRunJobTime.Ticks;
            nodeFlag.FullPath = physicalLocation.DirPath;
            //nodeFlag.GroupId = physicalLocation.RootLocationId;
            nodeFlag.IsRemoved = false;
            nodeFlag.NodeFlagType = (int)NodeFlagType.ExplorerSync;
            nodeFlag.NodeId = physicalLocation.UniqueId;
            nodeFlag.Title = physicalLocation.Name;
            return nodeFlag;
        }
    }
}
