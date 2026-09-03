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
using AvePoint.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Cache;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.Report.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.RAPhysical.Report
{
    public class PRContentDueReportService : IPRContentDueReportJobService
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(PRContentDueReportService));
        //public IPRTermService PRTermService { get; set; }
        public IPRReportProcessor PRReportProcessor { get; set; }
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
        private RA.DB.Explorer.Dao.IExplorerDao mExplorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new ExplorerDao();
                }
                return mExplorerDao;
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
        protected IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        protected IRMReportService ReportService = PlatformWindsorManager.GetService<IRMReportService>();

        public IRMPhysicalPushColumnDao RMPhysicalPushColumnDao { get; set; }
        public IRecordLoanAllianceDao RecordLoanAllianceDao { set; get; }
        private DateTime mTimePoint;
        private DateTime mRunJobTime;
        private List<Guid> deletedFiles;
        private Dictionary<Guid, RMTerm> Terms { get; set; }
        /// <summary>
        /// key=rule id  value=rule
        /// </summary>
        private Dictionary<Guid, Rule> Rules { get; set; }
        /// <summary>
        /// key=termid  value=binded rules
        /// </summary>
        private Dictionary<Guid, List<Rule>> TermRuleMapping { get; set; }
        
        public async Task RunReportJobAsync(string jobId, string profileId)
        {
            mRunJobTime = DateTime.UtcNow;
            mTimePoint = GetTimePoint(profileId);
            deletedFiles = new List<Guid>();
            await InitializeAsync();

            var options = new ReportOptions()
            {
                JobId = jobId,
                ProfileId = profileId,
                JobType = JobType.PhysicalItemsFilesDueDisposalReport,
                BrowseOptions = new BrowseOptions() { NeedProcessBox = false, NeedProcessFile = false },
                IsUseBuiltInBottomLocationAction = false,
                IsUseBuiltInNormalLocationAction = false
            };
            await PRReportProcessor
            .ConfigTreeAction(treeService =>
            {
                treeService
                .ConfigNormalLocationAction(ProcessLocationAsync)
                .ConfigBottomLocationAction(ProcessLocationAsync);
                return Task.CompletedTask;
            })
            .ProcessAsync(options);
            var profile = ReportService.GetProfileByIdForReportJob(profileId);
            if (profile.ScheduleId != null)
            {
                var jobIdReal = jobId?.Split('_')[0];
                var job = JobMonitorDao.GetJobById(jobIdReal);
                if (job.Status == (int)JobStatus.Finished || job.Status == (int)JobStatus.FinishWithException)
                {
                    var exportModel = new ExportReportCommonModel
                    {
                        ReportJobType = ((int)profile.Type).ToString(),
                        ReportJobId = jobIdReal,
                        ProfileName = profile.ProfileName,
                        ProfileId = profile.Id.ToString(),
                    };
                    var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
                    ReportService.RunExportReportJob(reportParameters);

                }

            }
        }

        private DateTime GetTimePoint(string profileId)
        {
            var profile = PRReportProcessor.mRMReportService.GetProfileByIdForReportJob(profileId);
            var timePoint = PRReportProcessor.mRMReportService.GetUtcTimePoint(profile.Extension1);
            return timePoint;
        }

        private async Task ProcessLocationAsync(IPhysicalLocation location)
        {
            try
            {
                mLog.Info($"Process location {location.DirPath}");
                PRReportProcessor.ReportManager.IncreaseBase(1);

                var boxs = GetBoxes(location);
                var files = GetFiles(location);
                if(boxs!= null)
                {
                    await boxs.ForEachAsync(async b => await ProcessBoxAsync(b));
                }
                if(files!= null)
                {
                    await files.ForEachAsync(async f => await ProcessFileAsync(f));
                }
                
                SendJobDetail("RM_Common_ObjectLevel_PhysicalLocation", location.Name, location.DirPath, JobDetailsStatus.Successful);
            }
            catch (Exception e)
            {
                mLog.Error($"Process location error:{e.ToString()}");
                SendJobDetail("RM_Common_ObjectLevel_PhysicalLocation", location.Name, location.DirPath, JobDetailsStatus.Failed, e.Message);
                throw;
            }
            finally
            {
                PRReportProcessor.ReportManager.Increase(1);
            }
        }

        private async Task ProcessBoxAsync(IPhysicalBox box)
        {
            PRReportProcessor.ReportManager.IncreaseBase(1);
            List<Rule> rules;
            PhysicalDisposalActionType physicalAction = PhysicalDisposalActionType.None;
            mLog.Info($"Process box {box.DirPath}");
            try
            {
                var boxTermId = box.TermId;
                if (TermRuleMapping.TryGetValue(boxTermId, out rules))
                {
                    //此处逻辑需要优化，避免每个template 都获取一次，应该做global 级别的缓存
                    var template = await TemplateManagementService.LoadTemplateDtoAsync(box.TemplateId);
                    var columnCollection = new Dictionary<Guid, TemplateColumnDto>();
                    template.categories.ForEach(cat =>
                    {
                        cat.columns.ForEach(col => columnCollection[col.uniqueId] = col);
                    });

                    PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
                    Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn = new Dictionary<Guid, List<RMPhysicalPushColumn>>();
                    foreach (var fieldKey in box.Fields.Keys)
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
                                    physicObjectIds.Add(box.Id);
                                    List<RMPhysicalPushColumn> pushColumn = RMPhysicalPushColumnDao.GetPushColumns(column.uniqueId, physicObjectIds);
                                    columnIdAndPushColumn[column.uniqueId] = pushColumn;
                                }
                            }
                        }
                    }
                    var boxFilterObj = PhysicalObjectConvertor.ConvertPhysicalBoxFilterObject(engine.FilterPolicyCollection, box, columnCollection, columnIdAndPushColumn);
                    var physicalRule = engine.CheckRule(boxFilterObj);//Imps objct
                    physicalAction = GetDisposalAction(physicalRule, box.RuleId.ToString(), box.DisposalStatus);
                    if (physicalRule != null)
                    {
                        mLog.Info($"{box?.Id} fit rule {physicalRule.Name} Action {physicalAction.ToString()}");
                    }
                    bool onHold = ExplorerDao.IsRecordsHold(new List<Guid>() { box.Id }, mTimePoint.Ticks) || RecordLoanAllianceDao.IsRecordsLoan(new List<Guid>() { box.Id }, mTimePoint.Ticks);
                    switch (physicalAction)
                    {
                        case PhysicalDisposalActionType.Disposal:                            
                            if (onHold)
                            {
                                //TODO log
                                mLog.Info($"box is on hold. box id:[{box.Id}]");
                            }
                            else
                            {
                                SendReportForBox(box, physicalRule, physicalAction);
                            }
                            break;
                        //case PhysicalDisposalActionType.EmptyRuleInfo:
                        //    GetFiles(box).ForEach(f => ProcessFile(f));
                        //    break;
                        case PhysicalDisposalActionType.Move:
                            if (box.BoxUnderContainer())
                            {
                                mLog.Info($"Box is under container, will not move. box id:[{box.Id}]");
                            }
                            else if (RecordLoanAllianceDao.GetPhyRecordAllianceById(box.Id)?.Count > 0)
                            {
                                mLog.Info($"Box is loaned, will not move. record id :[{box.Id}]");
                            }
                            else
                            {
                                SendReportForBox(box, physicalRule, physicalAction);
                            }
                            break;
                        case PhysicalDisposalActionType.Pending:
                            if (onHold)
                            {
                                mLog.Info($"box is on hold. box id:[{box.Id}]");
                            }
                            else
                            {
                                SendReportForBox(box, physicalRule, physicalAction);
                            }
                            break;
                        case PhysicalDisposalActionType.None:
                            if (physicalRule == null)
                            {
                                await GetFiles(box).ForEachAsync(async f => await ProcessFileAsync(f));
                            }
                            else
                            {
                                //例如waiting approve数据没换Rule状态也是None，此时不需要处理
                            }
                            return;
                    }
                }
                else
                {
                    await GetFiles(box).ForEachAsync(async f => await ProcessFileAsync(f));
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Disposal box failed {box.DirPath} : {e.ToString()}");
                throw;
            }
            finally
            {
                PRReportProcessor.ReportManager.Increase(1);
            }
            
        }
        private async Task ProcessFileAsync(IPhysicalFile file)
        {
            PRReportProcessor.ReportManager.IncreaseBase(1);
            List<Rule> rules;
            mLog.Info($"Process File {file.Id}");
            PhysicalDisposalActionType physicalAction = PhysicalDisposalActionType.None;
            try
            {
                var fileTermId = file.TermId;
                if (TermRuleMapping.TryGetValue(fileTermId, out rules))
                {
                    PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
                    //此处逻辑需要优化，避免每个template 都获取一次，应该做global 级别的缓存
                    var template = await TemplateManagementService.LoadTemplateDtoAsync(file.TemplateId);
                    if (file.BoxId != Guid.Empty)
                    {
                        ExplorerService.AddPushColumnToFold(template, file.BoxId);
                    }
                    var columnIdAndNameMapping = new Dictionary<Guid, TemplateColumnDto>();
                    template.categories.ForEach(cat =>
                    {
                        cat.columns.ForEach(col => columnIdAndNameMapping[col.uniqueId] = col);
                    });
                    Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn = new Dictionary<Guid, List<RMPhysicalPushColumn>>();
                    foreach (var fieldKey in file.Fields.Keys)
                    {
                        Guid fieldId;
                        if (Guid.TryParse(fieldKey, out fieldId))
                        {
                            if (columnIdAndNameMapping != null && columnIdAndNameMapping.ContainsKey(fieldId))
                            {
                                var column = columnIdAndNameMapping[fieldId];
                                if (column.pushToChild)
                                {
                                    List<Guid> physicObjectIds = new List<Guid>();
                                    if (column.inheritFromParent)
                                    {
                                        physicObjectIds.Add(file.BoxId);
                                    }
                                    else
                                    {
                                        physicObjectIds.Add(file.Id);
                                    }
                                    List<RMPhysicalPushColumn> pushColumn = RMPhysicalPushColumnDao.GetPushColumns(column.uniqueId, physicObjectIds);
                                    columnIdAndPushColumn[column.uniqueId] = pushColumn;
                                }
                            }
                        }
                    }
                    var fileFilterObj = PhysicalObjectConvertor.ConvertPhysicalFileFilterObject(engine.FilterPolicyCollection, file, columnIdAndNameMapping, columnIdAndPushColumn);
                    Rule rule = engine.CheckRule(fileFilterObj);
                    physicalAction = GetDisposalAction(rule, file.RuleId.ToString(), file.DisposalStatus);
                    if (rule != null)
                    {
                        mLog.Info($"{file?.Id} fit rule :{rule.Name} Action: {physicalAction.ToString()}");
                    }
                    bool onHold = ExplorerDao.IsRecordsHold(new List<Guid>() { file.Id, file.BoxId }, mTimePoint.Ticks) || RecordLoanAllianceDao.IsRecordsLoan(new List<Guid>() { file.Id, file.BoxId }, mTimePoint.Ticks);
                    switch (physicalAction)
                    {
                        case PhysicalDisposalActionType.Disposal:
                            if (onHold)
                            {
                                mLog.Info($"Current file is on hold. id:[{file.Id}]");
                            }
                            else
                            {
                                deletedFiles.Add(file.Id);
                                SendReportForFile(file, rule, physicalAction);
                                ArgumentCheck.NotNull(rule, nameof(rule));
                                if (rule.PhysicalRule != null && rule.PhysicalRule.IsDeleteParentBox && file.ParentBox != null)
                                {
                                    if (NeedDeleteParentBox(file.ParentBox))
                                    {
                                        SendReportForBox(file.ParentBox, rule, PhysicalDisposalActionType.Disposal);
                                    }
                                }
                            }
                            break;
                        //case PhysicalDisposalActionType.EmptyRuleInfo:
                        //    break;
                        case PhysicalDisposalActionType.Move:
                            if (file.FolderUnderContainer())
                            {
                                mLog.Info($"Folder is under container, will not move. id:[{file.Id}]");
                            }
                            else if (RecordLoanAllianceDao.GetPhyRecordAllianceById(file.Id)?.Count > 0)
                            {
                                mLog.Info($"Folder is loaned, will not move. record id :[{file.Id}]");
                            }
                            else
                            {
                                SendReportForFile(file, rule, physicalAction);
                            }
                            break;
                        case PhysicalDisposalActionType.CalculateDisposalDate:
                            mLog.Info($"Current file [{file.Id}] is on hold: {onHold}");
                            SendReportForFile(file, rule, physicalAction);
                            break;
                        case PhysicalDisposalActionType.Pending:
                            if (onHold)
                            {
                                mLog.Info($"Current file is on hold. id:[{file.Id}]");
                            }
                            else
                            {
                                SendReportForFile(file, rule, physicalAction);
                            }
                            break;
                    }
                }
                else
                {
                    mLog.Info($"Process file skip {file.Id}");
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Disposal file failed {file.DirPath} : {e.ToString()}");
                throw;
            }
            finally
            {
                PRReportProcessor.ReportManager.Increase(1);
            }
            
        }

        private bool NeedDeleteParentBox(IPhysicalBox box)
        {
            var filesInBox = box.GetFiles(f => f.RecordStatus != (int)RMRecordStatus.Destroyed && f.RecordStatus != (int)RMRecordStatus.RMDeleted && f.RecordStatus != (int)RMRecordStatus.MoveOverwrite);
            foreach (var file in filesInBox)
            {
                if (!deletedFiles.Contains(file.Id))
                {
                    return false;
                }
            }
            return true;
        }

        private PhysicalDisposalActionType GetDisposalAction(Rule physicalRule, string originalRuleId, int originalStatus)
        {
            if (physicalRule != null && physicalRule.PhysicalRule.IsManualApproval)
            {
                if (physicalRule.Id != null && physicalRule.Id.Equals(originalRuleId.ToString()))
                {
                    if (originalStatus == (int)SOApproveDBStatus.Approved)
                    {
                        //disposalAction.DisposalBox(box);
                        return PhysicalDisposalActionType.Disposal;
                    }
                    else if (originalStatus == (int)SOApproveDBStatus.WaitingApprove)
                    {
                        //disposalAction.PendingBox(box);
                        //return PhysicalDisposalActionType.None;
                        return PhysicalDisposalActionType.Pending;
                    }
                    else
                    {
                        return PhysicalDisposalActionType.Pending;
                    }
                }
                else
                {
                    return PhysicalDisposalActionType.Pending;//if ruleid is not the same switch the rule.
                }
            }
            else if (physicalRule != null && physicalRule.PhysicalRule.spMoveOption != null && physicalRule.PhysicalRule.spMoveOption.MoveDestination != null)//TO do how to judge move option.
            {
                return PhysicalDisposalActionType.Move;
            }
            else if (physicalRule != null && physicalRule.PhysicalRule.IsCalculationDisposalDate)
            {
                return PhysicalDisposalActionType.CalculateDisposalDate;
            }
            else if (physicalRule != null)
            {
                return PhysicalDisposalActionType.Disposal;
            }
            return PhysicalDisposalActionType.None;
        }
        
        private Task InitializeAsync()
        {
            LoadRules();
            foreach (var rule in Rules.Values)
            {
                ModifyTimeCriteria(rule, mTimePoint);
            }
            LoadTerms();
            return AssembleTermRuleMappingAsync();
        }
        private void LoadTerms()
        {
            mLog.Info("Begin to load terms.");
            Terms = new TermDao().GetAllTermsForce().ToDictionary(t => t.UniqueId);
            mLog.Info("Loaded {0} terms.", Terms.Count);
        }

        private void LoadRules()
        {
            mLog.Info("Begin to Load rules.");
            Rules = RuleManagerService.GetRulesFromRecords().ToDictionary(rule => new Guid(rule.Id));
            mLog.Info("End to load Rules");
        }

        private void ModifyTimeCriteria(Rule rule, DateTime timePoint)
        {
            var soFilters = rule.PhysicalRule?.SOFilters;
            if (soFilters != null)
            {
                foreach (var filter in soFilters)
                {
                    ModifyOlderThanCriteria(filter, timePoint);
                    filter.SequenceNo += 1;
                }
                //add created time criteria
                soFilters.Add(new SOFilterPolicy()
                {
                    Condition = PolicyCondition.Before,
                    Level = rule.PhysicalRule.PolicyLevel,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });
                mLog.Info($"Before convert and or express:{rule.PhysicalRule.AndOrExpression[rule.PhysicalRule.PolicyLevel]}");
                var tempStrs = rule.PhysicalRule.AndOrExpression[rule.PhysicalRule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                string andOrExpression = "(1 And (";
                foreach (var str in tempStrs)
                {
                    int sequenceNo = 0;
                    if (int.TryParse(str, out sequenceNo))
                    {
                        sequenceNo++;
                        andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                    }
                    else
                    {
                        andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                    }
                }
                andOrExpression += "))";
                rule.PhysicalRule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                {
                    { rule.PhysicalRule.PolicyLevel, andOrExpression }
                };
                mLog.Info($"After convert and or express:{rule.PhysicalRule.AndOrExpression[rule.PhysicalRule.PolicyLevel]}");
            }
        }

        private async Task AssembleTermRuleMappingAsync()
        {
            mLog.Info("Begin to assemble term rules mappings.");
            TermRuleMapping = new Dictionary<Guid, List<Rule>>();
            Dictionary<int, Guid> termIdUniqueIdMapping = Terms.Values.ToDictionary(r => r.Id, r => r.UniqueId);
            ITermRuleAssociationDao termRuleAssociationDao = new TermRuleAssociationDao();
            Dictionary<int, List<RMTermRuleAssociation>> tempMapping = termRuleAssociationDao.GetTermWithRule()
                .GroupBy(t => t.TermId)
                .ToDictionary(t => t.Key, v => v.OrderBy(r => r.RuleOrder).ToList());
            Dictionary<int, List<Rule>> termRuleMapping = new Dictionary<int, List<Rule>>();

            foreach (var t in tempMapping)
            {
                List<Rule> rules = new List<Rule>();
                t.Value.ForEach(association => rules.Add(Rules[association.RuleId]));
                termRuleMapping[t.Key] = rules;
            }

            ITermSetMembershipDao membershipDao = new TermSetMembershipDao();
            Dictionary<int, List<int>> memberships = (await membershipDao.FindListWithColumnsAsync(c => new { c.TermId, c.ParentTermId }, e => !e.IsRemoved))
                .GroupBy(t => t.ParentTermId, v => v.TermId)
                .ToDictionary(t => t.Key, v => v.ToList());

            foreach (var pId in memberships.Keys.OrderBy(k => k))
            {
                if (termRuleMapping.ContainsKey(pId))
                {
                    memberships[pId].ForEach(cId =>
                    {
                        if (!termRuleMapping.ContainsKey(cId))
                        {
                            termRuleMapping[cId] = termRuleMapping[pId];
                        }
                    });
                }
            }
            foreach (var termId in termRuleMapping.Keys)
            {
                if (termIdUniqueIdMapping.ContainsKey(termId))
                {
                    Guid termGuid = termIdUniqueIdMapping[termId];
                    TermRuleMapping[termGuid] = termRuleMapping[termId];
                }
            }
        }

        private void ModifyOlderThanCriteria(SOFilterPolicy filter, DateTime timePoint)
        {
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule)
            {
                switch (filter.Condition)
                {
                    case PolicyCondition.OlderThan:
                        int num;
                        DateTime tempDt = DateTime.UtcNow;
                        if (int.TryParse(filter.Value.Value1, out num))
                        {
                            if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                            {
                                tempDt = timePoint.AddDays(-num);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                            {
                                tempDt = timePoint.AddDays(-num * 7);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                            {
                                tempDt = timePoint.AddMonths(-num);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                            {
                                tempDt = timePoint.AddYears(-num);
                            }
                            filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                            filter.Condition = PolicyCondition.Before;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /*private bool ShouldBeProcessed(IPhysicalRecord item)
        {
            ///1.get term id by item
            ///2.get rules by term
            ///3.validate if item fit the rule, check if disposed and due date as well.)

            //List<Rule> rules;
            //if (!RMPhysicalDisposalCache.Instance.TermRuleMapping.TryGetValue(item.TermId, out rules)) return false;
            //PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
            //var boxFilterObj = PhysicalObjectConvertor.ConvertPhysicalFileFilterObject(engine.FilterPolicyCollection, i, columnIdAndNameMapping);
            //var physicalRule = engine.CheckRule(boxFilterObj);//Imps objct

            throw new NotImplementedException();
        }*/

        private List<IPhysicalFile> GetFiles(IPhysicalLocation location)
        {
            return location.GetFiles(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed)
                                            && b.TimeCreated <= mTimePoint.Ticks);
        }

        private List<IPhysicalFile> GetFiles(IPhysicalBox box)
        {
            return box.GetFiles(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed)
                                            && b.TimeCreated <= mTimePoint.Ticks);
        }

        private List<IPhysicalBox> GetBoxes(IPhysicalLocation location)
        {
            return location.GetBoxes(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed)
                                            && b.TimeCreated <= mTimePoint.Ticks);
        }

        private void SendReportForBox(IPhysicalBox box, Rule rule, PhysicalDisposalActionType disposalAction)
        {
            DueDisposalReport report = new DueDisposalReport();
            report.TitleOrName = box.Name;
            report.Url = box.DirPath;
            report.BCSTermId = box.TermId.ToString();
            report.BCSTermName = Terms[box.TermId].Name;
            report.ObjectLevel = (int)RMReportObjectLevel.PhyBox;
            report.CreatedBy = box.CreateBy;
            report.CreatedTime = box.CreateTimeTicks;
            report.LastModifiedBy = box.ModifiedBy;
            report.LastModifiedTime = box.ModifiedTimeTicks;
            AddReportProperty(report, rule, disposalAction, box.Id);
            PRReportProcessor.ReportManager.SendJobReport(report);
        }


        private void SendReportForFile(IPhysicalFile file, Rule rule, PhysicalDisposalActionType disposalAction)
        {
            DueDisposalReport report = new DueDisposalReport();
            report.TitleOrName = file.Name;
            report.Url = file.DirPath;
            report.BCSTermId = file.TermId.ToString();
            report.BCSTermName = Terms[file.TermId].Name;
            report.ObjectLevel = (int)RMReportObjectLevel.PhyFolder;
            report.CreatedBy = file.CreateBy;
            report.CreatedTime = file.CreateTimeTicks;
            report.LastModifiedBy = file.ModifiedBy;
            report.LastModifiedTime = file.ModifiedTimeTicks;
            AddReportProperty(report, rule, disposalAction, file.Id);
            PRReportProcessor.ReportManager.SendJobReport(report);
        }

        private List<ReportRelatedRecords> GetRelatedRecords(Guid id)
        {
            var currRecord = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
            //if (string.IsNullOrEmpty(currRecord?.RelatedRecords))
            //{
            //    return result;
            //}
            List<ReportRelatedRecords> result = new List<ReportRelatedRecords>();
            if (!string.IsNullOrEmpty(currRecord?.RelatedRecords))
            {
                List<RMRelatedItemInfo> relatedRecordsInDB = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);

                foreach (var relatedRecord in relatedRecordsInDB)
                {
                    //result.Add(new ReportRelatedRecords() { Name = relatedRecord.name, Url = relatedRecord.url });
                    if (relatedRecord.SourceFlag == (int)SourceFlag.Physical)
                    {
                        result.Add(new ReportRelatedRecords() { Name = relatedRecord.recId, Url = "" });
                    }
                    else
                    {
                        string itemFullUrl = string.Empty;
                        if (!relatedRecord.url.StartsWith(relatedRecord.SiteUrl))
                        {
                            itemFullUrl = AvePoint.RA.Common.Util.WebUtil.MakeFullUrl(relatedRecord.SiteUrl, relatedRecord.url);
                        }
                        else
                        {
                            itemFullUrl = relatedRecord.url;
                        }
                        result.Add(new ReportRelatedRecords() { Name = relatedRecord.name, Url = itemFullUrl });
                    }
                }
            }
            return result;
        }

        private BaseReport AddReportProperty(DueDisposalReport report, Rule rule, PhysicalDisposalActionType disposalAction, Guid physicalObjectId)
        {
            report.AppliedRuleId = rule.Id;
            report.AppliedRuleName = rule.Name;
            switch (disposalAction)
            {
                case PhysicalDisposalActionType.Pending:
                    report.ManualApproval = RMDisposalManualApproval.Yes;
                    break;
                case PhysicalDisposalActionType.Disposal:
                    report.ManualApproval = RMDisposalManualApproval.No;
                    break;
                case PhysicalDisposalActionType.Move:
                    report.ManualApproval = RMDisposalManualApproval.Nonsupport;
                    break;
                case PhysicalDisposalActionType.CalculateDisposalDate:
                    report.ManualApproval = RMDisposalManualApproval.Nonsupport;
                    break;
                default:
                    break;
            }
            report.RelatedRecords = SerializerHelper.SerializeToXmlString(GetRelatedRecords(physicalObjectId));
            report.RelatedRecordsAction = (int)rule.PhysicalRule.RelatedRecordOption;
            report.DisposalAction = (int)GetOperationTypeForPhysical(rule.PhysicalRule);
            report.ExportType = (RMExportTypeValue)AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None;
            report.DisposalClass = rule.DisposalClass;
            return report;
        }

        private RMContentDisposalAction GetOperationTypeForPhysical(Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                return RMContentDisposalAction.Move;
            }
            else if (rule.IsCalculationDisposalDate)
            {
                return RMContentDisposalAction.CalculationDisposalDate;
            }
            else
            {
                var deleteOption = RMContentDisposalAction.Remove;
                if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    deleteOption |= RMContentDisposalAction.RelatedRecords;
                }
                if (rule.IsDeleteParentBox)
                {
                    deleteOption |= RMContentDisposalAction.DeleteParentBox;
                }
                return deleteOption;
            }
        }

        private void SendJobDetail(string type, string name, string path, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = type;
            detail.TitleOrName = name;
            detail.Url = path;
            detail.Status = status;
            detail.Comment = comments;
            PRReportProcessor.ReportManager.SendJobDetail(detail);
        }
    }
}
