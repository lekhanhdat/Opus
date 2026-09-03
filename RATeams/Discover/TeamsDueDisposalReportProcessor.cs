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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.RACommonUtility.CAMLHelper.General;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.RA.RACommonUtility.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.Wrapper.Common;
using RATeams.Discover.Base;
using RATeams.Discover.Extension;
using System.Collections.Concurrent;

namespace RATeams.Discover
{
    public class TeamsDueDisposalReportProcessor : RMTeamsReportProcessor
    {
        private List<PolicyLevel> ruleLevels;
        private RuleManagement _ruleManagement;
        private DateTime _timePoint;
        private int _itemsPerTask = 500;
        private SOArchiverSettings _archiverSettings;
        private Dictionary<Guid, RMRuleItemCollection> _termAndRulesMapping;
        private ConcurrentDictionary<string, Rule> _ruleDic = new();
        private IExplorerDao _explorerDao = new ExplorerDao();
        private Dictionary<Guid, Guid> mTermIdCache = new();
        public IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private RMProfileDto profile;
        private string jobId;
        protected readonly IRuleManagerService _ruleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();

        public TeamsDueDisposalReportProcessor(string jobId, string profileId) : base(jobId, JobType.TeamsItemsFilesDueDisposalReport, false)
        {
            this.jobId = jobId;
            profile = ReportService.GetProfileByIdForReportJob(profileId);
            _timePoint = ReportService.GetUtcTimePoint(profile.Extension1);
            _termAndRulesMapping = ReportService.GetTermAndRuleMappingsNew(_timePoint, SourceFlag.SharePoint);
            ruleLevels = ReportService.GetRuleLevels(_termAndRulesMapping);
            _ruleDic = new ConcurrentDictionary<string, Rule>(_ruleManagerService.GetRulesFromRecords().ToDictionary(r => r.Id));
            _archiverSettings = ReportService.GetSOArchiverSettings();
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_DUE_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }
            Logger.Info($"TeamsContentDueItemsPerTask : {_itemsPerTask}");
        }
        public override async Task RunAsync()
        {
            try
            {
                foreach (var siteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    if (BCSColumnNameDics.TryGetValue(siteCollectionNodeItem.Id, out BCSColumnName))
                    {
                        await ProcessAsync(siteCollectionNodeItem);
                    }
                    else
                    {
                        Logger.Warn("Get BCS Column Name error.");
                    }
                }
            }
            catch (JobStopException e)
            {
                JobHasStopped = true;
            }
            catch (Exception e)
            {
                if (e is PropertyNotAssignedException)
                {
                    Logger.Error("A property was not assigned while running the Teams due-disposal report. Exception:{0}", e.ToString());
                }
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (JobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (JobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                ReportManager.SetJobFinished(finalStatus);
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
                PerformanceMonitor.WritePerformanceResult();
            }
        }
        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds, IAveWeb web, IAveList list)
        {
            CAMLManager cm = new CAMLManager();
            Logger.Info("The Document created after {0} can't be reported", _timePoint);
            foreach (var termId in termIds)
            {
                QueryGroup group = null;
                RMRuleItemCollection checkerColl = null;
                int wssid = 0;
                if (_termAndRulesMapping.TryGetValue(termId, out checkerColl) && GetWssidOfTerm(taxonomyField, termId, out wssid))
                {
                    var groupFactory = new QueryGroupFactory(
                        checkerColl,
                        listFields,
                        ClientContextHelper.SPWebTimeZone,
                        null,
                        _timePoint,
                        BCSColumnInternalName,
                        wssid);
                    group = groupFactory.GetQueryGroupByRuleCheckerCollection();
                }
                if (group != null && (group.Conditions.Count != 0 || group.Groups.Count != 0))
                {
                    cm.QueryGroup.AddGroup(group);
                }
            }

            var listSetting = GetListSetting(web, list);
            if (listSetting != null && listSetting.IsInheritParentTerm)
            {
                Logger.Info($"[GCI] List {list.RootFolder.Url} has enable inherit parent term setting, fullpath {listSetting.FullPath}");
                var parentTermId = GetParentTermId(web, list);
                if (parentTermId != Guid.Empty && _termAndRulesMapping.TryGetValue(parentTermId, out var parentCheckerColl))
                {
                    Logger.Info($"[GCI] List {list.RootFolder.Url} has parentTermId {parentTermId}, termName: {parentCheckerColl.TermName}. Start build CAMLManager");
                    QueryGroup parentGroup = null;
                    var parentGroupFactory = new QueryGroupFactory(
                        parentCheckerColl,
                        listFields,
                        ClientContextHelper.SPWebTimeZone,
                        null,//SP Source，Rule中时间条件和BeforeReportTime都是UTC，不需要传RegionSetting
                        _timePoint,
                        BCSColumnInternalName,
                        0);
                    parentGroup = parentGroupFactory.GetQueryGroupByRuleCheckerCollection();
                    if (parentGroup != null && (parentGroup.Conditions.Count != 0 || parentGroup.Groups.Count != 0))
                    {
                        cm.QueryGroup.AddGroup(parentGroup);
                    }
                }
            }
            else
            {
                Logger.Info($"[GCI] List {list.RootFolder.Url} hasUniqueSetting: {listSetting != null}, fullpath {listSetting?.FullPath}, isInheritTerm: {listSetting?.IsInheritParentTerm}");
            }

            if (cm.QueryGroup.Groups.Count > 0)
            {
                return cm;
            }
            else
            {
                return null;
            }
        }
        protected override async Task ProcessSiteAsync(NodeItem site)
        {
            using (PerformanceScope scope = new PerformanceScope("TeamsDueDisposalReportProcessor.ProcessSite", $"TeamsDueDisposalReportProcessor.ProcessSite.[{site.NameOrTitle}]", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                    var bposInfo = await CommonPoolUserUtil.GetBPOSInfoAsync(remoteSite);
                    var mfactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                    IAveSite discoverSite = null;
                    try
                    {
                        discoverSite = mfactory.CreateSite(site.FullPath);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Can not connect to the site collection, fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
                        SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                        throw;
                    }
                    site.NameOrTitle = discoverSite.RootWeb.Title;
                    RMRuleItemCollection rules;
                    if (discoverSite.RootWeb.Properties.ContainsKey("RevIM"))
                    {
                        var termId = new Guid(discoverSite.RootWeb.Properties["RevIM"].ToString());
                        mTermIdCache[siteId] = termId;
                        if (site.IsChecked && _termAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            _ruleManagement = new RuleManagement(rules.CommonRules);
                            Rule rs = _ruleManagement.CheckSiteCollectionCriteria(discoverSite);
                            if (rs != null)
                            {
                                ClientContextHelper.SPWebTimeZone = discoverSite.RootWeb.RegionalSettings.TimeZone;
                                DueDisposalReport report = new DueDisposalReport();
                                report.AppliedRuleId = rs.Id;
                                report.AppliedRuleName = rs.Name;
                                report.DisposalAction = RuleHelper.GetOperationTypeForSP(rs);
                                report.ManualApproval = report.DisposalAction != (int)RMContentDisposalAction.Remove ?
                                    RMDisposalManualApproval.No : (rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No);
                                report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                                report.TitleOrName = discoverSite.RootWeb.Title;
                                report.SiteCollectionTitle = site.TeamsName;
                                report.Url = discoverSite.Url;
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = rules.TermName;
                                report.ObjectLevel = (int)RMReportObjectLevel.SiteCollection;
                                Int32 index = discoverSite.Owner.NoPrefixLoginName.IndexOf("|");
                                if (index != -1)
                                {
                                    report.CreatedBy = discoverSite.Owner.NoPrefixLoginName.Substring(index + 1);
                                }
                                report.CreatedTime = ClientContextHelper.GetDateTimeValue(discoverSite.RootWeb.Created).Ticks;
                                report.LastModifiedBy = "";
                                report.LastModifiedTime = ClientContextHelper.GetDateTimeValue(discoverSite.RootWeb.LastItemModifiedDate).Ticks;
                                report.SPWebTimeZoneName = ClientContextHelper.SPWebTimeZone.Description;
                                report.DisposalClass = rs.DisposalClass;
                                ReportManager.SendJobReport(report);
                                Logger.Info("Web fit the disposal rule {0}:{1}", discoverSite.RootWeb.Url, rs.Name);
                                SendJobReportDetails(site, JobDetailsStatus.Successful);
                                return;
                            }
                        }
                    }
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.SiteCollection);
                    if (isHasLowLevelRule)
                    {
                        await base.ProcessSiteAsync(site);
                    }
                    else
                    {
                        SendJobReportDetails(site, JobDetailsStatus.Successful);
                    }
                }
            }
        }
        protected override async Task ProcessWebAsync(NodeItem web, bool IsProcessLists)
        {
            using (PerformanceScope scope = new PerformanceScope("TeamsDueDisposalReportProcessor.ProcessWeb", $"TeamsDueDisposalReportProcessor.ProcessWeb.[{web.NameOrTitle}]", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    RMRuleItemCollection rules;
                    var discoverWeb = web.DiscoverObj as IAveWeb;
                    bool isRootWeb = discoverWeb.ID.Equals(discoverWeb.Site.RootWeb.ID) ? true : false;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.Site);
                    IsProcessLists = isHasLowLevelRule;
                    if (discoverWeb.Properties.ContainsKey("RevIM"))
                    {
                        var termId = new Guid(discoverWeb.Properties["RevIM"].ToString());
                        mTermIdCache.TryAdd(discoverWeb.ID, termId);
                        if (web.IsChecked && !isRootWeb && _termAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            _ruleManagement = new RuleManagement(rules.CommonRules);
                            Rule rs = _ruleManagement.CheckSiteCriteria(discoverWeb);
                            if (rs != null)
                            {
                                DueDisposalReport report = new DueDisposalReport();
                                report.AppliedRuleId = rs.Id;
                                report.AppliedRuleName = rs.Name;
                                report.DisposalAction = RuleHelper.GetOperationTypeForSP(rs);
                                report.ManualApproval = report.DisposalAction != (int)RMContentDisposalAction.Remove ?
                                    RMDisposalManualApproval.No : (rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No);
                                report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                                report.TitleOrName = discoverWeb.Title;
                                report.SiteCollectionTitle = web.TeamsName;
                                report.Url = discoverWeb.Url;
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = rules.TermName;
                                report.ObjectLevel = (int)RMReportObjectLevel.Site;
                                if (discoverWeb.Author != null)
                                {
                                    report.CreatedBy = discoverWeb.Author.Name;
                                }
                                report.CreatedTime = ClientContextHelper.GetDateTimeValue(discoverWeb.Created).Ticks;
                                report.LastModifiedBy = "";
                                report.LastModifiedTime = ClientContextHelper.GetDateTimeValue(discoverWeb.LastItemModifiedDate).Ticks;
                                report.SPWebTimeZoneName = ClientContextHelper.SPWebTimeZone.Description;
                                report.DisposalClass = rs.DisposalClass;
                                ReportManager.SendJobReport(report);
                                Logger.Info("Web fit the disposal rule {0}:{1}", discoverWeb.Url, rs.Name);
                                SendJobReportDetails(web, JobDetailsStatus.Successful);
                                return;
                            }
                        }
                    }
                    await base.ProcessWebAsync(web, IsProcessLists);
                }
            }
        }
        protected override async Task ProcessListAsync(NodeItem list)
        {
            using (PerformanceScope scope = new PerformanceScope("TeamsDueDisposalReportProcessor.ProcessList", $"TeamsDueDisposalReportProcessor.ProcessList.[{list.NameOrTitle}]", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverList = list.DiscoverObj as IAveList;
                    RMRuleItemCollection rules;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.List);

                    if (discoverList.RootFolder.Properties.ContainsKey("RevIM"))
                    {
                        var termId = new Guid(discoverList.RootFolder.Properties["RevIM"].ToString());
                        mTermIdCache.TryAdd(discoverList.ID, termId);
                        if (list.IsChecked && _termAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            _ruleManagement = new RuleManagement(rules.CommonRules);
                            Rule rs = _ruleManagement.CheckListCriteria(discoverList);
                            if (rs != null)
                            {
                                DueDisposalReport report = new DueDisposalReport();
                                report.AppliedRuleId = rs.Id;
                                report.AppliedRuleName = rs.Name;
                                report.DisposalAction = RuleHelper.GetOperationTypeForSP(rs);
                                report.ManualApproval = report.DisposalAction != (int)RMContentDisposalAction.Remove ?
                                    RMDisposalManualApproval.No : (rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No);
                                report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                                report.TitleOrName = discoverList.Title;
                                report.SiteCollectionTitle = list.TeamsName;
                                report.Url = MakeFullUrl(discoverList.ParentWeb.Url, discoverList.RootFolder.Url);
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = rules.TermName;
                                report.ObjectLevel = (int)RMReportObjectLevel.List;
                                if (discoverList.Author != null)
                                {
                                    report.CreatedBy = discoverList.Author.Name;
                                }
                                report.CreatedTime = ClientContextHelper.GetDateTimeValue(discoverList.Created).Ticks;
                                report.LastModifiedBy = "";
                                report.LastModifiedTime = ClientContextHelper.GetDateTimeValue(discoverList.LastItemModifiedDate).Ticks;
                                report.SPWebTimeZoneName = ClientContextHelper.SPWebTimeZone.Description;
                                report.DisposalClass = rs.DisposalClass;
                                ReportManager.SendJobReport(report);
                                var rootFolder = discoverList.RootFolder;
                                var discoverWeb = discoverList.ParentWeb;
                                list.FullPath = MakeFullUrl(discoverWeb.Url, rootFolder.Url);
                                SendJobReportDetails(list, JobDetailsStatus.Successful);
                                return;
                            }
                        }
                    }
                    if (isHasLowLevelRule)
                    {
                        await base.ProcessListAsync(list);
                    }
                    else
                    {
                        var rootFolder = discoverList.RootFolder;
                        var discoverWeb = discoverList.ParentWeb;
                        list.FullPath = MakeFullUrl(discoverWeb.Url, rootFolder.Url);
                        SendJobReportDetails(list, JobDetailsStatus.Successful);
                    }
                }
            }
        }
        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items, string teamsName)
        {
            int results = 0;
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("TeamsDueDisposalReportProcessor.ProcessItems", $"TeamsDueDisposalReportProcessor.ProcessItemsOfList[{list.Title}]", addToStatistics: true))
                {
                    int objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                    var siteId = web.Site.ID;
                    var listSetting = GetListSetting(web, list);
                    var recordIds = items.Select(o => IDGenerator.GetRecordId(siteId, o.CurrentItem.UniqueId)).ToList();
                    if (items.Count > _itemsPerTask)
                    {
                        results = RunMultiThreadsProcessItems(_timePoint.Ticks, items, web, list, siteId, objectLevel, teamsName, listSetting);
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            results += ProcessOneItem(_timePoint.Ticks, web, list, item, siteId, objectLevel, teamsName, listSetting);
                        }
                    }
                }
            }
            return results;
        }
        private int RunMultiThreadsProcessItems(long ticks, List<RMDiscoverItem> items, IAveWeb web, IAveList list, Guid siteId, int objectLevel, string teamsName, RMTeamsSetting listSetting)
        {
            using (PerformanceScope scope = new PerformanceScope("RunMultiThreadsProcessItems", $"RunMultiThreadsProcessItemsOfList[{list.Title}]", addToStatistics: true))
            {
                Logger.Info($"Run multi threads to process items, items count : {items.Count}");
                var cts = new CancellationTokenSource();
                var t = AveTenantTasks.RunAndWaitResult(items, cts, item =>
                {
                    return ProcessOneItem(ticks, web, list, item, siteId, objectLevel, teamsName, listSetting, cts);
                });
                return t;
            }
        }
        private int ProcessOneItem(long ticks, IAveWeb web, IAveList list, RMDiscoverItem discoverItem, Guid siteId, int objectLevel, string teamsName, RMTeamsSetting listSetting, CancellationTokenSource cts = null)
        {
            var result = 0;
            ReportManager.Increase();
            using (PerformanceScope scope0 = new PerformanceScope("TeamsDueDisposalReportProcessor.ProcessItem", $"TeamsDueDisposalReportProcessor.ProcessItemOfList[{list.Title}]", addToStatistics: true))
            {
                var item = discoverItem.CurrentItem;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        Guid termId;
                        string termName;

                        Guid itemId = item.UniqueId;
                        var recordId = IDGenerator.GetRecordId(siteId, itemId);

                        if (item.IsStubItem())
                        {
                            Logger.Debug($"Current item [{item?.UniqueId}] is stub file, so skipped.");
                            return result;
                        }

                        if (!ValidateItem(item, out termId, out termName))
                        {
                            if (item.Folder != null)
                            {
                                Logger.Warn($"Folder {discoverItem.Url} will not use inherit parent term, so skipped.");
                                return result;
                            }

                            if (listSetting == null)
                            {
                                Logger.Warn($"List setting is null for list {list?.Title} in web {web?.Url}");
                                return result;
                            }
                            if (!listSetting.IsInheritParentTerm)
                            {
                                Logger.Warn($"List {list?.Title} is not inherit parent term, so skipped.");
                                return result;
                            }

                            var parentTermId = GetParentTermId(web, list);
                            if (parentTermId == Guid.Empty)
                            {
                                Logger.Warn($"Parent term id is empty for list {list.Title} in web {web.Url}");
                                return result;
                            }

                            Logger.Info($"Current item [{item?.UniqueId}] term id is {termId}, term name is {termName}, parent term id is {parentTermId}");
                            termId = parentTermId;
                            termName = _termAndRulesMapping.TryGetValue(termId, out var ruleInfo) ? ruleInfo.TermName : string.Empty;
                        }

                        DueDisposalReport report = new DueDisposalReport();
                        report.SiteCollectionTitle = teamsName;
                        RMRuleItemCollection rules;
                        AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption relatedRecords = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.None;
                        if (_termAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            if (rules.Rules.Count == 0)
                            {
                                return result;
                            }
                            if (!rules.HasUnCamlQueryableCondition && rules.Rules.Count == 1 && (rules.Rules[0].RuleFilters.Any(r => r.RuleType != ArchiverFilterRuleType.LastAccessedTime) || rules.Rules[0].RuleFilters.Any(r => r.RuleType != ArchiverFilterRuleType.LastActiveTime)))
                            {
                                var ruleItem = rules.Rules[0];
                                relatedRecords = ruleItem.RelatedRecordOption;
                                if (!ValidateRule(item, ruleItem, list)) return result;

                                Rule rs = GetRule(ruleItem.RuleId);
                                if (rs != null && RuleHelper.CheckMoveRule(rs) && IsCheckoutFile(item))
                                {
                                    Logger.Warn("File is checked out and matched rule is moveto rule {0}", item?.UniqueId);
                                    return result;
                                }

                                Record record = null;
                                try
                                {
                                    record = _explorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
                                }
                                catch (Exception e)
                                {
                                    Logger.Warn($"Error occurred while get GetHoldRecordsByIds. Error:{e.ToString()}");
                                }
                                int archiverAction = RuleHelper.GetOldLogicDisposalAction((int)ruleItem.ArchiverAction);
                                if (record != null && record.HoldReleaseTime > ticks
                                    && !(archiverAction == (int)RMContentDisposalAction.Move || archiverAction == (int)RMContentDisposalAction.MoveDeclare
                                    || archiverAction == (int)RMContentDisposalAction.KeepData || archiverAction == (int)RMContentDisposalAction.ExportOnly
                                    || archiverAction == (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication || archiverAction == (int)RMContentDisposalAction.MoveWithDeleteSource
                                    || archiverAction == (int)RMContentDisposalAction.MoveWithKeepClassfication))
                                {
                                    Logger.Warn("File is on explorer hold. The file should not be reported. Record id: {0}.RuleAction:{1}.", recordId.ToString(), ruleItem.ArchiverAction);
                                    return result;
                                }
                                report.AppliedRuleId = ruleItem.RuleId;
                                report.AppliedRuleName = ruleItem.RuleName;

                                report.DisposalAction = (int)ruleItem.ArchiverAction;
                                report.ManualApproval = ruleItem.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                                report.ExportType = (RMExportTypeValue)ruleItem.ExportType;
                                report.DisposalClass = ruleItem.DisposalClass;
                            }
                            else
                            {
                                RuleManagement ruleManagement = new RuleManagement(rules.CommonRules);
                                Rule rs = null;
                                if (item.Folder != null)
                                {
                                    rs = ruleManagement.CheckFolderCriteria(item.Folder);
                                    if (rs != null)
                                    {
                                        report.ObjectLevel = (int)RMReportObjectLevel.Folder;
                                        base.FitRuleFoldersInDisposalJob.Add(item.Folder.UniqueId);
                                    }
                                }
                                else
                                {
                                    using (PerformanceScope s = new PerformanceScope("DueDisposalReportProcessor.ProcessItems_CheckItemCriteria", $"DueDisposalReportProcessor.ProcessItems_CheckItemCriteriaOfList[{list.Title}]", addToStatistics: true))
                                    {
                                        rs = ruleManagement.CheckItemCriteria(item.UniqueId, item);
                                    }
                                }
                                if (rs != null)
                                {
                                    if (item.CheckIsRecord())
                                    {
                                        if (!rs.DeleteRecords && !item.IsBlockDeleteOnlyRecord() && !RuleHelper.CheckMoveRule(rs)
                                            && !RuleHelper.CheckArchiveOnlyRule(rs.KeepDataOption))//Get from message & merge contract
                                        {
                                            Logger.Warn("File is record and option is not delete record {0}", item?.UniqueId);
                                            return result;
                                        }
                                    }

                                    if (RuleHelper.CheckMoveRule(rs) && IsCheckoutFile(item))
                                    {
                                        Logger.Warn("File is checked out and matched rule is moveto rule {0}", item?.UniqueId);
                                        return result;
                                    }

                                    var disposalAction = RuleHelper.GetOperationTypeForSP(rs);

                                    Record record = null;
                                    try
                                    {
                                        record = _explorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Warn($"Error occurred while get GetHoldRecordsByIds. Error:{e.ToString()}");
                                    }
                                    if (record != null && record.HoldReleaseTime > ticks
                                        && !RuleHelper.IsAllowedDisposalAction(disposalAction))
                                    {
                                        Logger.Warn("File is on explorer hold. The file should not be reported. Record id: {0}.RuleAction:{1}.", recordId.ToString(), disposalAction);
                                        return result;
                                    }

                                    relatedRecords = rs.RelatedRecordOption;
                                    report.AppliedRuleId = rs.Id;
                                    report.AppliedRuleName = rs.Name;
                                    report.DisposalAction = disposalAction;
                                    report.ManualApproval = rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                                    report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                                    report.DisposalClass = rs.DisposalClass;
                                }
                                else
                                {
                                    Logger.Info("Item not fit rule {0}", item?.UniqueId);
                                    return result;
                                }
                            }
                        }

                        report.BuildRelatedRecords(item, Site.Url, relatedRecords);
                        objectLevel = ProcessObjectLevel(list, item, objectLevel);

                        try
                        {
                            BuildReport(report, web, list, item, termId, termName, objectLevel);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            Task.Run(() =>
                            {
                                Logger.Info("add item report{0}", item?.UniqueId);
                                ReportManager.SendJobReport(report);
                            });
                            result = 1;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    cts?.Cancel();
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    JobHasException = true;
                    Logger.Warn("Report item failed. item id: {0}, error message: {1}.", item?.UniqueId, ex.ToString());
                    string comment = ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message) && ex.InnerException.Message.StartsWith("The site do not meet the conditions.", StringComparison.OrdinalIgnoreCase) ?
                        "RM_SPS_LastAccessTimeQueryException" : ex.Message;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        Logger.Info("add item report{0}", item?.UniqueId);
                        base.SendJobReportItemDetails(item, NodeLevel.Item, JobDetailsStatus.Failed, comment);
                    });
                }

                return result;
            }
        }
        private bool ValidateItem(IAveListItem item, out Guid termId, out string termName)
        {
            using (PerformanceScope scope = new PerformanceScope("ValidateItem", addToStatistics: true))
            {
                if (!item.GetSingleTaxonomyFieldValue(BCSColumnInternalName, out termId, out termName))
                {
                    Logger.Warn("can't get sigle item value {0}", item?.UniqueId);
                    return false;
                }
                if (item.CheckHasHold())
                {
                    Logger.Warn("File is Hold ,not delete hold {0}", item?.UniqueId);
                    return false;
                }

                return true;
            }
        }
        private bool ValidateRule(IAveListItem item, RMRuleItem ruleItem, IAveList list)
        {
            using (PerformanceScope scope = new PerformanceScope("ValidateRule", $"ValidateItemRuleOfList[{list.Title}]", addToStatistics: true))
            {
                if (item.CheckIsRecord())
                {
                    if (!ruleItem.DeleteRecords && !item.IsBlockDeleteOnlyRecord() && !ruleItem.IsMoveRule)
                    {
                        Logger.Warn("File is record and option is not delete record {0}", item?.UniqueId);
                        return false;
                    }
                }
                if (ruleItem.RuleFilters[0].Level == PolicyLevel.Item && list.BaseType == AveBaseType.DocumentLibrary)
                {
                    Logger.Info("document can't fit item rule");
                    return false;
                }
                else if (ruleItem.RuleFilters[0].Level == PolicyLevel.Document && list.BaseType != AveBaseType.DocumentLibrary)
                {
                    Logger.Info("item can't fit document rule");
                    return false;
                }
                else if (ruleItem.RuleFilters[0].Level == PolicyLevel.Folder)
                {
                    if (item.Folder == null)
                    {
                        Logger.Info("item can't fit folder rule {0}", item?.UniqueId);
                        return false;
                    }
                }
                else if (ruleItem.RuleFilters[0].Level != PolicyLevel.Folder && item.Folder != null)
                {
                    Logger.Info("folder can't fit item rule {0}", item?.UniqueId);
                    return false;
                }

                return true;
            }
        }
        private bool IsCheckoutFile(IAveListItem item)
        {
            var values = item.FieldValues;
            string checkoutUser = values.ContainsKey("CheckoutUser") ? values["CheckoutUser"]?.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(checkoutUser))
            {
                string separator = ";#";
                int index = checkoutUser.IndexOf(separator);
                if (index > 0)
                {
                    var checkoutUserName = checkoutUser.Substring(index);
                    if (!string.IsNullOrEmpty(checkoutUser))
                    {
                        Logger.Info(string.Format("The file is Check Out file,File id:{0}", item?.UniqueId.ToString()));
                        return true;
                    }
                }
            }
            return false;
        }
        private int ProcessObjectLevel(IAveList list, IAveListItem item, int objectLevel)
        {
            if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                objectLevel = (int)RMReportObjectLevel.Folder;
            }
            else
            {
                objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
            }
            return objectLevel;
        }
        private void BuildReport(DueDisposalReport report, IAveWeb web, IAveList list, IAveListItem item, Guid termId, string termName, int objectLevel)
        {
            try
            {
                report.TitleOrName = this.GetListItemName(item);
                report.Url = list.BaseType == AveBaseType.DocumentLibrary ? MakeFullUrl(web.Url, item.Url) : WebUtil.GetListItemRealPath(web.Url, list.RootFolder.ServerRelativeUrl, item.Url);
                report.BCSTermId = termId.ToString();
                report.BCSTermName = termName;
                report.ObjectLevel = objectLevel;

                report.CreatedBy = GetSingleUserFieldValue(item, "Author");
                report.CreatedTime = DateTime.Parse(item["Created"].ToString()).Ticks;
                report.LastModifiedBy = GetSingleUserFieldValue(item, "Editor");
                report.LastModifiedTime = DateTime.Parse(item["Modified"].ToString()).Ticks;
                report.SPWebTimeZoneName = ClientContextHelper.SPWebTimeZone.Description;
                string itemUrl = item.Url;
                foreach (string fileExtension in _archiverSettings.SkipFileExtensions)
                {
                    if (itemUrl.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        report.Status = RMReportStatus.Skip;
                        report.DisposalAction = (int)RMContentDisposalAction.None;
                        report.Comment = "RM_JM_ReportComment_ContentSkip";
                        break;
                    }
                }
            }
            catch
            {
                Logger.Info("build item report error{0}", item.Url);
                report.Status = RMReportStatus.Failed;
                report.Comment = "RM_JM_ReportComment_Failed";
                throw;
            }
        }
        private Rule GetRule(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId)) return null;
            if (!_ruleDic.ContainsKey(ruleId))
            {
                var client = new DAOAPIClientV1();
                Rule rule = client.LoadRule(ruleId);
                _ruleDic.TryAdd(ruleId, rule);
            }
            return _ruleDic[ruleId];
        }

        private RMTeamsSetting GetListSetting(IAveWeb web, IAveList list)
        {
            if (list != null && list.ID != Guid.Empty)
            {
                var setting = _teamsSettingHelper.GetTeamsSettingByScope(list.ID);
                if (setting != null) return setting;
            }

            //var listUrl = MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
            //var siteSetting = _teamsSettingHelper.GetTeamsSettingByUrl(listUrl);
            //if (siteSetting != null) return siteSetting;
            var currentWeb = web;
            while (currentWeb != null)
            {
                var setting = _teamsSettingHelper.GetTeamsSettingByScope(currentWeb.ID);
                if (setting != null)
                {
                    return setting;
                }
                currentWeb = currentWeb.ParentWeb;
            }
            return mSiteSettingsCache.TryGetValue(siteId, out var cachedSetting) ? cachedSetting : null;
        }

        private Guid GetParentTermId(IAveWeb web, IAveList list)
        {
            Guid listId = list?.ID ?? Guid.Empty;
            Guid webId = web?.ID ?? Guid.Empty;
            // 1. List
            var parentTermId = list.RootFolder.Properties.ContainsKey("RevIM") ? new Guid(list.RootFolder.Properties["RevIM"].ToString()) : Guid.Empty;
            if (parentTermId != Guid.Empty)
            {
                return parentTermId;
            }
            Guid foundWebTermId = FindWebTermIdRecursive(web);
            if (foundWebTermId != Guid.Empty)
            {
                return foundWebTermId;
            }
            // 3. Site
            if (mTermIdCache.TryGetValue(siteId, out var siteTermId) && siteTermId != Guid.Empty)
            {
                return siteTermId;
            }
            return Guid.Empty;
        }


        private Guid FindWebTermIdRecursive(IAveWeb web)
        {
            while (web != null)
            {
                if (web.ID != Guid.Empty && mTermIdCache.TryGetValue(web.ID, out var termId) && termId != Guid.Empty)
                {
                    return termId;
                }
                try
                {
                    web = web.ParentWeb;
                }
                catch
                {
                    break;
                }
            }
            return Guid.Empty;
        }
    }
}
