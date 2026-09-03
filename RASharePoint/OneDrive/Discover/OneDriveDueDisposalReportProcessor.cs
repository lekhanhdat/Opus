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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Common.Util;
using AvePoint.RA.SharePoint.RelatedRecords;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Common.Threads;
using System.Threading;
using System.Collections.Concurrent;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.RA.SharePoint.OneDrive.Discover.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.RACommonUtility.Browser;

namespace AvePoint.RA.SharePoint.Discover
{
    public class OneDriveDueDisposalReportProcessor : RMOneDriveReportProcessor
    {
        private Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping;
        private ConcurrentDictionary<string, Rule> _ruleDic = new ConcurrentDictionary<string, Rule>();
        private int _itemsPerTask = 500;
        private SOArchiverSettings mArchiverSettings;
        //private NodeItem mFarmNode;
        private DateTime mTimePoint;
        private List<PolicyLevel> ruleLevels;
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
        public Dictionary<Guid, Rule> Rules { get; private set; }
        public Dictionary<Guid, RMRuleItemCollection> TermRuleMapping { get; private set; }

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

        private ITermRuleAssociationDao termRuleAssociationDao;
        protected ITermRuleAssociationDao TermRuleInfos
        {
            get
            {
                if (termRuleAssociationDao == null)
                {
                    termRuleAssociationDao = new TermRuleAssociationDao();
                }
                return termRuleAssociationDao;
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
        private RMProfileDto profile;
        private string jobId;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public OneDriveDueDisposalReportProcessor(string jobId, string profileId)
            : base(jobId, (int)JobType.OneDriveItemsFilesDueDisposalReport, false)
        {
            this.jobId = jobId;
            profile = ReportService.GetProfileByIdForReportJob(profileId);
            mTimePoint = ReportService.GetUtcTimePoint(profile.Extension1);
            mTermAndRulesMapping = ReportService.GetTermAndRuleMappingsNew(mTimePoint, SourceFlag.OneDrive);
            ruleLevels = ReportService.GetRuleLevels(mTermAndRulesMapping);
            //ProcessWebApplication += InitRuleManagement;
            mArchiverSettings = ReportService.GetSOArchiverSettings();
            _ruleDic = new ConcurrentDictionary<string, Rule>(RuleManagerService.GetRulesFromRecords().ToDictionary(r => r.Id));
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_DUE_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }
            mLog.Info($"SPOContentDueItemsPerTask : {_itemsPerTask}");
            //LoadRules();
           // AssembleTermRuleMapping();
        }

        public override async System.Threading.Tasks.Task RunReportJobAsync()
        {
            try
            {
                foreach (var SiteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    await ProcessAsync(SiteCollectionNodeItem);
                }
            }
            catch (JobStopException ex)
            {
                mJobHasStopped = true;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (mJobHasStopped)
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
            }
        }

        protected override async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem site)
        {
            using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessSite"))
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
                    // RMRuleItemCollection rules;
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
        protected override async System.Threading.Tasks.Task ProcessWebAsync(NodeItem web, bool IsProcessLists)
        {
            if (!OneDriveSettingDao.GetSettingEnableInfoByScope(groupId, siteId, web.Id))
            {
                SendJobReportDetails(web, JobDetailsStatus.Skipped, "RM_JS_JMD_DisableRecordManagement");
                mLog.Info("Process web sharepoint setting is disable {0}", web.FullPath);
                return;
            }
            using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessWeb"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverWeb = web.DiscoverObj as IAveWeb;
                    RMRuleItemCollection rules;
                    bool isRootWeb = discoverWeb.ID.Equals(discoverWeb.Site.RootWeb.ID) ? true : false;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.Site);
                    IsProcessLists = isHasLowLevelRule;
                    await base.ProcessWebAsync(web, IsProcessLists);
                }
            }
        }
        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem list)
        {
            if (!OneDriveSettingDao.GetSettingEnableInfoByScope(groupId, siteId, list.Id))
            {
                SendJobReportDetails(list, JobDetailsStatus.Skipped, "RM_JS_JMD_DisableRecordManagement");
                mLog.Info("Process list sharepoint setting is disable {0}", list.FullPath);
                return;
            }
            using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessList"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverList = list.DiscoverObj as IAveList;
                    RMRuleItemCollection rules;
                    bool isHasLowLevelRule = ReportService.CheckHasLowLevelRule(ruleLevels, PolicyLevel.List);
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
        protected override int ProcessItems(IAveWeb web, IAveList list, List<BaseRecordDto> items)
        {
            int results = 0;
            var itemList = new List<BaseRecordDto>();
            //items = items.Where(i => i.TimeCreated < mTimePoint.Ticks).ToList();
            foreach(var item in items)
            { 
                if(item.TimeCreated < mTimePoint.Ticks)
                {
                    if(!item.DisposalDueDate.Equals("Next Job", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (item.DisposalDueDate.IndexOf("(") > 0)
                        {
                            try
                            {
                                var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
                                DateTime utcDt = GeneralSettingService.ConvertDateTimeToUtcAsync(item.DisposalDueDate, gls).GetAwaiter().GetResult();
                                if (utcDt.Ticks < mTimePoint.Ticks)
                                {
                                    itemList.Add(item);
                                }
                            }
                            catch(Exception e)
                            {
                                mLog.Error("Convert to utc date failed,item id:{0},error:{1},", item.Id, e);
                                continue;
                            }                            
                        }
                        else
                        {
                            continue;
                        }                       
                    }
                    else
                    {
                        itemList.Add(item);
                    }
                }
            }
            if (itemList != null && itemList.Count > 0)
            {
                ReportManager.IncreaseBase(itemList.Count);
                using (PerformanceScope scope = new PerformanceScope("DueDisposalReportProcessor.ProcessItems"))
                {
                    //int tempCounter = 0;
                    int objectLevel = (int)RMReportObjectLevel.Document;
                    //list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                    //List<BaseReport> reports = new List<BaseReport>();
                    var siteId = web.Site.ID;

                    var recordIds = itemList.Select(o => o.Id).ToList();

                    if (itemList.Count > _itemsPerTask)
                    {
                        results = RunMultiThreadsProcessItems(mTimePoint.Ticks, itemList, web, list, siteId, objectLevel);
                    }
                    else
                    {
                        foreach (var item in itemList)
                        {
                            results += ProcessOneItem(mTimePoint.Ticks, web, list, item, siteId, objectLevel);
                        }
                    }
                }
            }
            return results;
        }

        private int RunMultiThreadsProcessItems(long ticks, List<BaseRecordDto> items, IAveWeb web, IAveList list, Guid siteId, int objectLevel)
        {
            mLog.Info($"Run multi threads to process items, items count : {items.Count}");
            var cts = new CancellationTokenSource();
            var t = AveTenantTasks.RunAndWaitResult(items, cts, item =>
            {
                return ProcessOneItem(ticks, web, list, item, siteId, objectLevel, cts);
            });
            return t;
        }

       

       
        private int ProcessOneItem(long ticks, IAveWeb web, IAveList list, BaseRecordDto discoverItem, Guid siteId, int objectLevel, CancellationTokenSource cts = null)
        {
            var result = 0;
            ReportManager.Increase();
            using (PerformanceScope scope0 = new PerformanceScope("DueDisposalReportProcessor.ProcessItem"))
            {
                try
                {

                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        Guid termId = discoverItem.TermId;
                        var recordId = discoverItem.Id;
                        mLog.Info("Process item {0}", discoverItem.ItemRowId);


                        DueDisposalReport report = new DueDisposalReport();
                        report.SiteCollectionTitle = web.Site.RootWeb.Title;
                        AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption relatedRecords = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.None;

                        //commented out by byron, current query will get all item's field info, so need not to get the item again.
                        //this function will throw exception if the list's itemcount > threshold.
                        //var retryitem = web.GetListItem(item.Url, list.ID, item.UniqueId);

                        Rule rs = GetRule(discoverItem.RuleId.ToString());
                        if (rs != null && rs.OneDriveRule != null)
                        {
                            var disposalAction = RuleHelper.GetOperationTypeForOneDrive(rs.OneDriveRule);
                            var record = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
                            if (record != null && record.HoldReleaseTime > ticks
                                && !(disposalAction == (int)RMContentDisposalAction.Move || disposalAction == (int)RMContentDisposalAction.MoveDeclare 
                                || disposalAction == (int)RMContentDisposalAction.KeepData || disposalAction == (int)RMContentDisposalAction.ExportOnly
                                || disposalAction == (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication || disposalAction == (int)RMContentDisposalAction.MoveWithDeleteSource
                                || disposalAction == (int)RMContentDisposalAction.MoveWithKeepClassfication))
                            {
                                mLog.Warn("File is on explorer hold. The file should not be reported. Record id: {0}.RuleAction:{1}.", recordId.ToString(), disposalAction);
                                return result;
                            }

                            relatedRecords = rs.RelatedRecordOption;
                            report.AppliedRuleId = rs.Id;
                            report.AppliedRuleName = rs.Name;
                            report.DisposalAction = disposalAction;
                            report.ManualApproval = rs.OneDriveRule.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                            report.ExportType = (RMExportTypeValue)(rs.OneDriveRule.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rs.OneDriveRule.ExportInfo.exportType);
                            report.DisposalClass = rs.DisposalClass;
                        }
                        else
                        {
                            mLog.Info("Item not fit rule {0}", discoverItem.DirPath);
                            return result;
                        }

                        try
                        {
                            BuildReport(report, web, list, discoverItem, termId, discoverItem.TermName, objectLevel);
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
                                mLog.Info("add item report{0}", discoverItem.ItemRowId);
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
                    mLog.Warn("Report item failed. item url: {0}, error message: {1}.", discoverItem.DirPath, ex.ToString());
                }

                return result;
            }
        }





        private void BuildReport(DueDisposalReport report, IAveWeb web, IAveList list, BaseRecordDto discoverItem, Guid termId, string termName, int objectLevel)
        {
            try
            {
                report.TitleOrName = discoverItem.LeafName;
                report.Url = WebUtil.MakeFullUrl(web.Url, discoverItem.DirPath);
                //list.BaseType == AveBaseType.DocumentLibrary ? MakeFullUrl(web.Url, item.Url) : WebUtil.GetListItemRealPath(web.Url, list.RootFolder.ServerRelativeUrl, item.Url);
                report.BCSTermId = termId.ToString();
                report.BCSTermName = termName;
                report.ObjectLevel = objectLevel;

                report.CreatedBy = discoverItem.CreatedBy;
                report.CreatedTime = GetDateTimeFromUtc(discoverItem.TimeCreated, web).Ticks;
                //GetDateTimeFieldValue(item, "Created").Ticks;
                report.LastModifiedBy = discoverItem.ModifiedBy;
                report.LastModifiedTime = GetDateTimeFromUtc(discoverItem.TimeLastModified, web).Ticks;
                report.SPWebTimeZoneName = SPWebTimeZone.Description;
                // check document is skip file
                string itemUrl = discoverItem.DirPath;
                foreach (string fileExtension in mArchiverSettings.SkipFileExtensions)
                {
                    //.aspx, .js, and .css
                    if (itemUrl.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        report.Status = RMReportStatus.Skip;
                        report.DisposalAction = (int)RMContentDisposalAction.None;
                        report.Comment = "RM_JM_ReportComment_ContentSkip";//string.Format(I18N.Core.I18NEntity.GetString("RM_JM_ReportComment_ContentSkip"), ".aspx, .js, and .css");
                        break;
                    }
                }
            }
            catch
            {
                mLog.Info("build item report error{0}", discoverItem?.Id);
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


    }
}
