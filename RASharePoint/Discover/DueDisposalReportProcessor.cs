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
using AvePoint.RA.Common.JobService;
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
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Discover
{
    public class DueDisposalReportProcessor : RMReportProcessor
    {
        private Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping;
        private ConcurrentDictionary<string, Rule> _ruleDic = new ConcurrentDictionary<string, Rule>();
        private Dictionary<Guid, Guid> mTermIdCache = new();
        private int _itemsPerTask = 500;
        private SOArchiverSettings mArchiverSettings;
        //private NodeItem mFarmNode;
        private DateTime mTimePoint;
        private RuleManagement mRuleManagement;
        private List<PolicyLevel> ruleLevels;
        private string jobId;
        private RMProfileDto profile;
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
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

        //protected override bool IsProcessListInParallel => true;

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

        //public IRecordAllianceDao RecordAllianceDao { get; set; }
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
        public DueDisposalReportProcessor(string jobId, string profileId)
            : base(jobId, (int)JobType.ItemsFilesDueDisposal, false)
        {
            this.jobId = jobId;
            profile = ReportService.GetProfileByIdForReportJob(profileId);
            SourceFlag flag = SourceFlag.SharePoint;
            mTimePoint = ReportService.GetUtcTimePoint(profile.Extension1);
            mTermAndRulesMapping = ReportService.GetTermAndRuleMappingsNew(mTimePoint, flag);
            ruleLevels = ReportService.GetRuleLevels(mTermAndRulesMapping);
            _ruleDic = new ConcurrentDictionary<string, Rule>(RuleManagerService.GetRulesFromRecords().ToDictionary(r => r.Id));
            //ProcessWebApplication += InitRuleManagement;
            mArchiverSettings = ReportService.GetSOArchiverSettings();

            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_DUE_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }
            mLog.Info($"SPOContentDueItemsPerTask : {_itemsPerTask}");
        }

        public override async System.Threading.Tasks.Task RunReportJobAsync()
        {
            try
            {
                foreach (var SiteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    if (await ProcessSiteCollectionNodeItemAsync(SiteCollectionNodeItem))
                        mHasNodeSuccessed = true;
                    else
                        mHasNodeFailed = true;
                }
            }
            catch (JobStopException ex)
            {
                mJobHasStopped = true;
            }
            catch (Exception ex)
            {
                if (ex is PropertyNotAssignedException)
                {
                    mLog.Error("A property was not assigned while running the SharePoint due-disposal report. Exception:{0}", ex.ToString());
                }
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasException || (mHasNodeSuccessed && mHasNodeFailed))
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                else if (mJobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                else if (mHasNodeFailed && !mHasNodeSuccessed)
                {
                    finalStatus = JobStatus.Failed;
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

        //private async Task<bool> WaitForReportJobFinishedAsync(string reportJobId)
        //{
        //    List<int> allStatus = SubJobDao.GetAllStatesByParent(reportJobId);
        //    return allStatus.All(s => s == (int)JobStatus.Finished);
        //}

        private async Task<bool> ProcessSiteCollectionNodeItemAsync(NodeItem item)
        {
            try
            {
                if (mBCSColumnNameDics.TryGetValue(item.Id, out mBCSColumnName))
                {
                    await ProcessAsync(item);
                    return true;
                }

                mLog.Warn("Get BCS Column Name error.");
                return false;
            }
            catch (JobStopException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error("Failed to process node item due to: {0}", ex.ToString());
                return false;
            }
        }

        public static List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertSOFilterPolicysToFilterPolicys(List<SOFilterPolicy> soFilterPolicys)
        {
            List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> result = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            if (soFilterPolicys != null && soFilterPolicys.Count > 0)
            {
                foreach (SOFilterPolicy soFilterPolicy in soFilterPolicys)
                {
                    result.Add(ConvertSOFilterPolicyToFilterPolicy(soFilterPolicy));
                }
            }
            return result;
        }

        public static AvePoint.GCommon.Contract.CommonFilter.FilterPolicy ConvertSOFilterPolicyToFilterPolicy(SOFilterPolicy soFilterPolicy)
        {
            if (soFilterPolicy == null)
            {
                return null;
            }
            AvePoint.GCommon.Contract.CommonFilter.FilterPolicy result = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
            result.Rule = soFilterPolicy.Rule;
            result.Condition = soFilterPolicy.Condition;
            result.Value = soFilterPolicy.Value;
            result.Level = soFilterPolicy.Level;
            result.SequenceNo = soFilterPolicy.SequenceNo;
            return result;
        }

        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds)
        {
            CAMLManager cm = new CAMLManager();
            mLog.Info("The Document created after {0} can't be reported", mTimePoint);
            foreach (var termId in termIds)
            {
                QueryGroup group = null;
                RMRuleItemCollection checkerColl = null;
                int wssid = 0;
                if (mTermAndRulesMapping.TryGetValue(termId, out checkerColl) && GetWssidOfTerm(taxonomyField, termId, out wssid))
                {
                    var groupFactory = new QueryGroupFactory(
                        checkerColl,
                        listFields,
                        SPWebTimeZone,
                        null,//SP Source，Rule中时间条件和BeforeReportTime都是UTC，不需要传RegionSetting
                        mTimePoint,
                        BCSColumnInternalName,
                        wssid);
                    group = groupFactory.GetQueryGroupByRuleCheckerCollection();

                }
                if (group != null && (group.Conditions.Count != 0 || group.Groups.Count != 0))
                {
                    cm.QueryGroup.AddGroup(group);
                }
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

        protected override CAMLManager InitUnclassificationCamlQuery(IAveFieldCollection listFields, IAveWeb web, IAveList list, RMReportExtension reportExt)
        {
            CAMLManager cm = new CAMLManager();
            mLog.Info("The Document created after {0} can't be reported", mTimePoint);

            var listSetting = GetListSetting(web, list);
            if (listSetting != null && listSetting.IsInheritParentTerm)
            {
                mLog.Info($"[GCI] List {list.RootFolder.Url} has enable inherit parent term setting, fullpath {listSetting.FullPath}");
                var parentTermId = GetParentTermId(web, list);
                if (parentTermId != Guid.Empty && mTermAndRulesMapping.TryGetValue(parentTermId, out var parentCheckerColl))
                {
                    mLog.Info($"[GCI] List {list.RootFolder.Url} has parentTermId {parentTermId}, termName: {parentCheckerColl.TermName}. Start build CAMLManager");

                    var newRuleColl = CloneRuleColl(parentCheckerColl);

                    var isCreatedIndexed = false;
                    if (reportExt != null && reportExt is RMDueDisposalReportListExtension listExtension)
                    {
                        HashSet<int> criterias = [];
                        foreach (var rule in newRuleColl.Rules)
                            foreach (var rf in rule.RuleFilters)
                                criterias.Add((int)rf.RuleType);

                        criterias.Add((int)ArchiverFilterRuleType.CreatedTime); // due action report use created time as common query condition

                        mLog.Info($"[GCI] List {list.RootFolder.Url} has isCreatedIndexed: {listExtension.IsCreatedIndexed}, criterias: {string.Join(',', criterias)} . indexedFields: {string.Join(',', listExtension.IndexedFieldStaticNames)}");

                        isCreatedIndexed = listExtension.IsCreatedIndexed;
                        newRuleColl.HasUnCamlQueryableCondition |= SPCommonUtility.FilterIndexedIncludeCriteria(listExtension.IndexedFieldStaticNames, criterias);
                    }

                    QueryGroup parentGroup = null;
                    var parentGroupFactory = new QueryGroupFactory(
                        newRuleColl,
                        listFields,
                        SPWebTimeZone,
                        null,//SP Source，Rule中时间条件和BeforeReportTime都是UTC，不需要传RegionSetting
                        mTimePoint,
                        BCSColumnInternalName,
                        0,
                        !isCreatedIndexed);
                    parentGroup = parentGroupFactory.GetQueryGroupByRuleCheckerCollection();
                    if (parentGroup != null && (parentGroup.Conditions.Count != 0 || parentGroup.Groups.Count != 0))
                    {
                        cm.QueryGroup.AddGroup(parentGroup);
                    }
                }
            }
            else
            {
                mLog.Info($"[GCI] List {list.RootFolder.Url} hasUniqueSetting: {listSetting != null}, fullpath {listSetting?.FullPath}, isInheritTerm: {listSetting?.IsInheritParentTerm}");
            }

            if (cm.QueryGroup.Groups.Count > 0)
            {
                cm.IsUnclassificationQuery = true;
                return cm;
            }
            else
            {
                return null;
            }
        }

        private RMRuleItemCollection CloneRuleColl(RMRuleItemCollection coll)
        {
            return new RMRuleItemCollection()
            {
                HasUnCamlQueryableCondition = coll.HasUnCamlQueryableCondition,
                TermId = coll.TermId,
                TermName = coll.TermName,
                CommonRules = coll.CommonRules,
                Rules = coll.Rules,
            };
        }

        protected override async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem site)
        {
            using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessSite", $"DueDisposalReportProcessor.ProcessSite.[{site.NameOrTitle}]", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                    var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                    var mfactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                    IAveSite discoverSite = null;
                    try
                    {
                        discoverSite = mfactory.CreateSite(site.FullPath);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Can not connect to the site collection, fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
                        SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                        throw;
                    }
                    site.NameOrTitle = discoverSite.RootWeb.Title;
                    //var discoverSite = site.DiscoverObj as IAveSite;
                    RMRuleItemCollection rules;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.SiteCollection);
                    if (discoverSite.RootWeb.Properties.ContainsKey("RevIM"))
                    {
                        var termId = new Guid(discoverSite.RootWeb.Properties["RevIM"].ToString());
                        mTermIdCache.TryAdd(siteId, termId);
                        if (site.IsChecked && mTermAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            mRuleManagement = new RuleManagement(rules.CommonRules);
                            Rule rs = mRuleManagement.CheckSiteCollectionCriteria(discoverSite);
                            if (rs != null)
                            {
                                base.SPWebTimeZone = discoverSite.RootWeb.RegionalSettings.TimeZone;
                                DueDisposalReport report = new DueDisposalReport();
                                report.AppliedRuleId = rs.Id;
                                report.AppliedRuleName = rs.Name;
                                report.DisposalAction = RuleHelper.GetOperationTypeForSP(rs);
                                report.ManualApproval = report.DisposalAction != (int)RMContentDisposalAction.Remove ?
                                    RMDisposalManualApproval.No : (rs.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No);
                                report.ExportType = (RMExportTypeValue)(rs.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.ExportInfo.exportType);
                                report.TitleOrName = discoverSite.RootWeb.Title;
                                report.SiteCollectionTitle = report.TitleOrName;
                                report.Url = discoverSite.Url;
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = rules.TermName;
                                report.ObjectLevel = (int)RMReportObjectLevel.SiteCollection;
                                Int32 index = discoverSite.Owner.NoPrefixLoginName.IndexOf("|");
                                if (index != -1)
                                {
                                    report.CreatedBy = discoverSite.Owner.NoPrefixLoginName.Substring(index + 1);
                                }
                                report.CreatedTime = GetDateTimeValue(discoverSite.RootWeb.Created).Ticks;
                                report.LastModifiedBy = "";
                                report.LastModifiedTime = GetDateTimeValue(discoverSite.RootWeb.LastItemModifiedDate).Ticks;
                                report.SPWebTimeZoneName = SPWebTimeZone.Description;
                                report.DisposalClass = rs.DisposalClass;
                                ReportManager.SendJobReport(report);
                                //List<BaseReport> reports = new List<BaseReport>();
                                //reports.Add(report);
                                //SendJobReport(reports);
                                mLog.Info("Web fit the disposal rule {0}:{1}", discoverSite.RootWeb.Url, rs.Name);
                                //send report 
                                SendJobReportDetails(site, JobDetailsStatus.Successful);
                                return;
                            }
                        }
                    }
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
        protected override async System.Threading.Tasks.Task ProcessWebAsync(NodeItem web, bool IsProcessLists)
        {
            using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessWeb", $"DueDisposalReportProcessor.ProcessWeb.[{web.NameOrTitle}]", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverWeb = web.DiscoverObj as IAveWeb;
                    RMRuleItemCollection rules;
                    bool isRootWeb = discoverWeb.ID.Equals(discoverWeb.Site.RootWeb.ID) ? true : false;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.Site);
                    IsProcessLists = isHasLowLevelRule;
                    if (discoverWeb.Properties.ContainsKey("RevIM"))
                    {
                        var termId = new Guid(discoverWeb.Properties["RevIM"].ToString());
                        mTermIdCache.TryAdd(discoverWeb.ID, termId);
                        if (web.IsChecked && !isRootWeb && mTermAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            mRuleManagement = new RuleManagement(rules.CommonRules);
                            Rule rs = mRuleManagement.CheckSiteCriteria(discoverWeb);
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
                                report.SiteCollectionTitle = discoverWeb.Site.RootWeb.Title;
                                report.Url = discoverWeb.Url;
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = rules.TermName;
                                report.ObjectLevel = (int)RMReportObjectLevel.Site;
                                if (discoverWeb.Author != null)
                                {
                                    report.CreatedBy = discoverWeb.Author.Name;
                                }
                                report.CreatedTime = GetDateTimeValue(discoverWeb.Created).Ticks;
                                report.LastModifiedBy = "";
                                report.LastModifiedTime = GetDateTimeValue(discoverWeb.LastItemModifiedDate).Ticks;
                                report.SPWebTimeZoneName = SPWebTimeZone.Description;
                                report.DisposalClass = rs.DisposalClass;
                                ReportManager.SendJobReport(report);
                                //List<BaseReport> reports = new List<BaseReport>();
                                //reports.Add(report);
                                //SendJobReport(reports);
                                mLog.Info("Web fit the disposal rule {0}:{1}", discoverWeb.Url, rs.Name);
                                //send report 
                                SendJobReportDetails(web, JobDetailsStatus.Successful);
                                return;
                            }
                        }
                    }
                    await base.ProcessWebAsync(web, IsProcessLists);
                }
            }
        }
        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem list)
        {
            using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessList", $"DueDisposalReportProcessor.ProcessList.[{list.NameOrTitle}]", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverList = list.DiscoverObj as IAveList;
                    RMRuleItemCollection rules;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.List);
                    //get list term id;
                    if (discoverList.RootFolder.Properties.ContainsKey("RevIM"))
                    {
                        var termId = new Guid(discoverList.RootFolder.Properties["RevIM"].ToString());
                        mTermIdCache.TryAdd(discoverList.ID, termId);
                        if (list.IsChecked && mTermAndRulesMapping.TryGetValue(termId, out rules))
                        {
                            mRuleManagement = new RuleManagement(rules.CommonRules);
                            Rule rs = mRuleManagement.CheckListCriteria(discoverList);
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
                                report.SiteCollectionTitle = discoverList.ParentWeb.Site.RootWeb.Title;
                                report.Url = MakeFullUrl(discoverList.ParentWeb.Url, discoverList.RootFolder.Url);
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = rules.TermName;
                                report.ObjectLevel = (int)RMReportObjectLevel.List;
                                if (discoverList.Author != null)
                                {
                                    report.CreatedBy = discoverList.Author.Name;
                                }
                                report.CreatedTime = GetDateTimeValue(discoverList.Created).Ticks;
                                report.LastModifiedBy = "";
                                report.LastModifiedTime = GetDateTimeValue(discoverList.LastItemModifiedDate).Ticks;
                                report.SPWebTimeZoneName = SPWebTimeZone.Description;
                                report.DisposalClass = rs.DisposalClass;
                                ReportManager.SendJobReport(report);
                                //List<BaseReport> reports = new List<BaseReport>();
                                //reports.Add(report);
                                //SendJobReport(reports);
                                //send report 
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
                        var indexedColumns = discoverList.Fields.Where(f => f.Indexed).Select(f => f.StaticName);
                        mLog.Info($"List {list.FullPath} has indexed columns: {string.Join(", ", indexedColumns)}");
                        RMDueDisposalReportListExtension listExt = new()
                        {
                            TimePoint = mTimePoint,
                            IsCreatedIndexed = indexedColumns.Contains(SPColumnConstants.SP_Created),
                            CanUnclassificationQuery = true,
                            IndexedFieldStaticNames = indexedColumns
                        };
                        list.ReportExtension = listExt;

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
        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items)
        {
            int results = 0;
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessItems", $"DueDisposalReportProcessor.ProcessItemsOfList[{list.Title}]", addToStatistics: true))
                {
                    //int tempCounter = 0;
                    int objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                    //List<BaseReport> reports = new List<BaseReport>();
                    var siteId = web.Site.ID;
                    var listSetting = GetListSetting(web, list);
                    var recordIds = items.Select(o => IDGenerator.GetRecordId(siteId, o.CurrentItem.UniqueId)).ToList();
                    if (items.Count > _itemsPerTask)
                    {
                        results = RunMultiThreadsProcessItems(mTimePoint.Ticks, items, web, list, siteId, objectLevel, listSetting);
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            results += ProcessOneItem(mTimePoint.Ticks, web, list, item, siteId, objectLevel, listSetting);
                        }
                    }
                }
            }
            return results;
        }

        private int RunMultiThreadsProcessItems(long ticks, List<RMDiscoverItem> items, IAveWeb web, IAveList list, Guid siteId, int objectLevel, RMSharePointSetting listSetting)
        {
            using (PerformanceScope scope = new PerformanceScope("RunMultiThreadsProcessItems", $"RunMultiThreadsProcessItemsOfList[{list.Title}]", addToStatistics: true))
            {
                mLog.Info($"Run multi threads to process items, items count : {items.Count}");
                var cts = new CancellationTokenSource();
                var t = AveTenantTasks.RunAndWaitResult(items, cts, item =>
                {
                    return ProcessOneItem(ticks, web, list, item, siteId, objectLevel, listSetting, cts);
                });
                return t;
            }
        }

        private bool ValidateItem(IAveListItem item, out Guid termId, out string termName)
        {
            using (PerformanceScope scope = new PerformanceScope("ValidateItem", addToStatistics: true))
            {
                if (!item.GetSingleTaxonomyFieldValue(BCSColumnInternalName, out termId, out termName))
                {
                    mLog.Warn("can't get sigle item value {0}", item?.UniqueId);
                    return false;   //error
                }

                // check is delete record
                //if (!mArchiverSettings.IsDeleteRecord && CheckIsRecord(item))
                //{
                //    mLog.Warn("File is record and option is not delete record {0}", item.Url);
                //    continue;
                //}

                if (item.CheckHasHold())
                {
                    mLog.Warn("File is Hold ,not delete hold {0}", item?.UniqueId);
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
                        mLog.Warn("File is record and option is not delete record {0}", item?.UniqueId);
                        return false;
                    }
                }
                if (ruleItem.RuleFilters[0].Level == GCommon.Contract.CommonFilter.PolicyLevel.Item && list.BaseType == AveBaseType.DocumentLibrary)
                {
                    mLog.Info("document can't fit item rule");
                    return false;
                }
                else if (ruleItem.RuleFilters[0].Level == GCommon.Contract.CommonFilter.PolicyLevel.Document && list.BaseType != AveBaseType.DocumentLibrary)
                {
                    mLog.Info("item can't fit document rule");
                    return false;
                }
                else if (ruleItem.RuleFilters[0].Level == GCommon.Contract.CommonFilter.PolicyLevel.Folder)
                {
                    if (item.Folder == null)
                    {
                        mLog.Info("item can't fit folder rule {0}", item?.UniqueId);
                        return false;
                    }
                }
                else if (ruleItem.RuleFilters[0].Level != GCommon.Contract.CommonFilter.PolicyLevel.Folder && item.Folder != null)
                {
                    mLog.Info("folder can't fit item rule {0}", item?.UniqueId);
                    return false;
                }

                return true;
            }
        }
        private int ProcessOneItem(long ticks, IAveWeb web, IAveList list, RMDiscoverItem discoverItem, Guid siteId, int objectLevel, RMSharePointSetting listSetting, CancellationTokenSource cts = null)
        {
            var result = 0;
            ReportManager.Increase();
            using (PerformanceScope scope0 = new PerformanceScope("DueDisposalReportProcessor.ProcessItem", $"DueDisposalReportProcessor.ProcessItemOfList[{list.Title}]", addToStatistics: true))
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
                        //mLog.Info("Process item {0}", item.Url);
                        if (item.IsStubItem())
                        {
                            mLog.Debug($"Current item [{item?.UniqueId}] is stub file, so skipped.");
                            return result;
                        }

                        if (!ValidateItem(item, out termId, out termName))
                        {
                            if (item.Folder != null)
                            {
                                mLog.Warn($"Folder {discoverItem.Url} will not use inherit parent term, so skipped.");
                                return result;
                            }

                            if (listSetting == null)
                            {
                                mLog.Warn($"List setting is null for list {list?.Title} in web {web?.Url}");
                                return result;
                            }
                            if (!listSetting.IsInheritParentTerm)
                            {
                                mLog.Warn($"List {list?.Title} is not inherit parent term, so skipped.");
                                return result;
                            }

                            var parentTermId = GetParentTermId(web, list);
                            if (parentTermId == Guid.Empty)
                            {
                                mLog.Warn($"Parent term id is empty for list {list.Title} in web {web.Url}");
                                return result;
                            }

                            mLog.Info($"Current item [{item?.UniqueId}] term id is {termId}, term name is {termName}, parent term id is {parentTermId}");
                            termId = parentTermId;
                            termName = mTermAndRulesMapping.TryGetValue(termId, out var ruleInfo) ? ruleInfo.TermName : string.Empty;
                        }

                        RMRuleItemCollection rules;
                        DueDisposalReport report = new DueDisposalReport();
                        report.SiteCollectionTitle = web.Site.RootWeb.Title;
                        AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption relatedRecords = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.None;
                        if (mTermAndRulesMapping.TryGetValue(termId, out rules))
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
                                    mLog.Warn("File is checked out and matched rule is moveto rule {0}", item?.UniqueId);
                                    return result;
                                }

                                DB.Explorer.Model.Record record = null;
                                try
                                {
                                    record = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn($"Error occurred while get GetHoldRecordsByIds. Error:{e.ToString()}");
                                }
                                int archiverAction = RuleHelper.GetOldLogicDisposalAction((int)ruleItem.ArchiverAction);
                                if (record != null && record.HoldReleaseTime > ticks
                                    && !(archiverAction == (int)RMContentDisposalAction.Move || archiverAction == (int)RMContentDisposalAction.MoveDeclare
                                    || archiverAction == (int)RMContentDisposalAction.KeepData || archiverAction == (int)RMContentDisposalAction.ExportOnly
                                    || archiverAction == (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication || archiverAction == (int)RMContentDisposalAction.MoveWithDeleteSource
                                    || archiverAction == (int)RMContentDisposalAction.MoveWithKeepClassfication))
                                {
                                    mLog.Warn("File is on explorer hold. The file should not be reported. Record id: {0}.RuleAction:{1}.", recordId.ToString(), ruleItem.ArchiverAction);
                                    return result;
                                }
                                report.AppliedRuleId = ruleItem.RuleId;
                                report.AppliedRuleName = ruleItem.RuleName;

                                #region Modify logic in ConvertRuleChecker
                                //if (ruleItem.ArchiverAction == RMContentDisposalAction.ArchiveAndRemove)
                                //{
                                //    //var client = new DAOAPIClientV1();
                                //    //Rule rule = client.LoadRule(ruleItem.RuleId);

                                //    var rule = GetRule(ruleItem.RuleId);

                                //    //兼容老数据
                                //    if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
                                //    {
                                //        //DelaredRecord为false时，是Declare，否则不是Declare，这样设计是为了升级兼容老数据
                                //        if (!rule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
                                //        {
                                //            report.DisposalAction = (int)RMContentDisposalAction.MoveDeclare;
                                //        }
                                //        else
                                //        {
                                //            report.DisposalAction = (int)RMContentDisposalAction.Move;
                                //        }
                                //    }
                                //    //新Move判断
                                //    else if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                                //    {
                                //        if (!rule.spMoveOption.MoveDestination.NotDeclareMovedData)
                                //        {
                                //            report.DisposalAction = (int)RMContentDisposalAction.MoveDeclare;
                                //        }
                                //        else
                                //        {
                                //            report.DisposalAction = (int)RMContentDisposalAction.Move;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        report.DisposalAction = (int)RMContentDisposalAction.ArchiveAndRemove;
                                //    }
                                //}
                                //else
                                //{
                                //    report.DisposalAction = (int)ruleItem.ArchiverAction;
                                //}
                                #endregion
                                report.DisposalAction = (int)ruleItem.ArchiverAction;
                                report.ManualApproval = ruleItem.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                                report.ExportType = (RMExportTypeValue)ruleItem.ExportType;
                                report.DisposalClass = ruleItem.DisposalClass;
                            }
                            else
                            {
                                //get object base info 
                                //check rule
                                #region rebuild sp rule
                                //RuleAssembler ruleAssembler = new RuleAssembler();
                                //RuleCollection newRuleCol = new RuleCollection();
                                //Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
                                //int reOrlder = 0;
                                //foreach (var order in rules.CommonRules.Rules.Keys)
                                //{
                                //    if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].SOFilters != null && rules.CommonRules.Rules[order].SOFilters.Count > 0)
                                //    {
                                //        var rule = rules.CommonRules.Rules[order];
                                //        //var newRule = ruleAssembler.ConvertToSPRule(rule);
                                //        if (rule.PolicyLevel != PolicyLevel.None)
                                //        {
                                //            reOrlder++;
                                //            newRules.Add(reOrlder, rule);
                                //        }
                                //    }
                                //}
                                //newRuleCol.Rules = newRules;
                                #endregion
                                RuleManagement ruleManagement = new RuleManagement(rules.CommonRules);
                                //commented out by byron, current query will get all item's field info, so need not to get the item again.
                                //this function will throw exception if the list's itemcount > threshold.
                                //var retryitem = web.GetListItem(item.Url, list.ID, item.UniqueId);
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
                                        if (!rs.DeleteRecords && !item.IsBlockDeleteOnlyRecord() && !RuleHelper.CheckMoveRule(rs) && !RuleHelper.CheckArchiveOnlyRule(rs))//Get from message & merge contract
                                        {
                                            mLog.Warn("File is record and option is not delete record {0}", item?.UniqueId);
                                            return result;
                                        }
                                    }

                                    if (RuleHelper.CheckMoveRule(rs) && IsCheckoutFile(item))
                                    {
                                        mLog.Warn("File is checked out and matched rule is moveto rule {0}", item?.UniqueId);
                                        return result;
                                    }

                                    var disposalAction = RuleHelper.GetOperationTypeForSP(rs);

                                    DB.Explorer.Model.Record record = null;
                                    try
                                    {
                                        record = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Warn($"Error occurred while get GetHoldRecordsByIds. Error:{e.ToString()}");
                                    }
                                    if (record != null && record.HoldReleaseTime > ticks
                                        && !RuleHelper.IsAllowedDisposalAction(disposalAction))
                                    {
                                        mLog.Warn("File is on explorer hold. The file should not be reported. Record id: {0}.RuleAction:{1}.", recordId.ToString(), disposalAction);
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
                                    mLog.Info("Item not fit rule {0}", item?.UniqueId);
                                    return result;
                                }
                            }
                        }

                        //mLog.Info("build item report{0}", item.Url);

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
                            //if (!RecordAllianceDao.CheckExplorerHold(recordId, mTimePoint.Ticks, report.DisposalAction))
                            //{
                            System.Threading.Tasks.Task.Run(() =>
                            {
                                mLog.Info("add item report{0}", item?.UniqueId);
                                ReportManager.SendJobReport(report);
                            });
                            //reports.Add(report);
                            //results++;
                            result = 1;
                            //}
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
                    JobHasExceptions = true;
                    mLog.Warn("Report item failed. item id: {0}, error message: {1}.", item?.UniqueId, ex.ToString());
                    string comment = ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message) && ex.InnerException.Message.StartsWith("The site do not meet the conditions.", StringComparison.OrdinalIgnoreCase) ?
                        "RM_SPS_LastAccessTimeQueryException" : ex.Message;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        mLog.Info("add item report{0}", item?.UniqueId);
                        base.SendJobReportItemDetails(item, NodeLevel.Item, JobDetailsStatus.Failed, comment);
                    });
                }

                return result;
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
                        mLog.Info(string.Format("The file is Check Out file,File id:{0}", item?.UniqueId.ToString()));
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
                //GetDateTimeFieldValue(item, "Created").Ticks;
                report.LastModifiedBy = GetSingleUserFieldValue(item, "Editor");
                report.LastModifiedTime = DateTime.Parse(item["Modified"].ToString()).Ticks;
                //GetDateTimeFieldValue(item, "Modified").Ticks;
                report.SPWebTimeZoneName = SPWebTimeZone.Description;
                // check document is skip file
                string itemUrl = item.Url;
                foreach (string fileExtension in mArchiverSettings.SkipFileExtensions)
                {
                    //.aspx, .js, and .css
                    if (itemUrl.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        report.Status = RMReportStatus.Skip;
                        report.DisposalAction = (int)RMContentDisposalAction.None;
                        report.Comment = "RM_JM_ReportComment_ContentSkip";//string.Format(I18N.Core.I18NEntity.GetString("RM_JM_ReportComment_ContentSkip"), ".aspx, .js, and .css")
                        break;
                    }
                }
            }
            catch
            {
                mLog.Info("build item report error{0}", item.Url);
                report.Status = RMReportStatus.Failed;
                report.Comment = "RM_JM_ReportComment_Failed";
                throw;
            }
        }
        //private RMContentDisposalAction GetOperationType(Rule rule)
        //{
        //    int keepDataOption = rule.KeepDataOption;
        //    if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
        //    {
        //        return RMContentDisposalAction.ArchiveLeaveStub;
        //    }
        //    else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
        //    {
        //        return RMContentDisposalAction.ArchiveAndKeepData;
        //    }
        //    else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
        //    {
        //        //DelaredRecord为false时，是Declare，否则不是Declare，这样设计是为了升级兼容老数据
        //        if (!rule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
        //        {
        //            return RMContentDisposalAction.MoveDeclare;
        //        }
        //        return RMContentDisposalAction.Move;
        //    }
        //    else
        //    {
        //        return RMContentDisposalAction.ArchiveAndRemove;
        //    }
        //}

        private RMSharePointSetting GetListSetting(IAveWeb web, IAveList list)
        {
            if (list != null && list.ID != Guid.Empty)
            {
                var setting = mSiteSettingsCache.TryGetValue(list.ID, out var listSetting) ? listSetting : null;
                if (setting != null) return setting;
            }
            //var listUrl = MakeFullUrl(list.ParentWeb.Url, list.RootFolder.ServerRelativeUrl);
            //foreach (var setting in mSiteSettingsCache.Values)
            //{
            //    if(listUrl.StartsWith(setting.FullPath, StringComparison.OrdinalIgnoreCase))
            //    {
            //        return setting;
            //    }
            //}

            var currentWeb = web;
            while (currentWeb != null)
            {
                var setting = mSiteSettingsCache.TryGetValue(currentWeb.ID, out var listSetting) ? listSetting : null;
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
                var termId = new Guid(web.Properties["RevIM"].ToString());
                if (termId != Guid.Empty)
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
