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
using System;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System.Linq;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Schedule;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Explorer.Model;
using System.IO;
using AvePoint.RA.Common.Report;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.Common.FilterEngine;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RASharePointOnPrem.Report.Base;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.RA.Common.SystemSetting;
using System.Threading.Tasks;
using RelatedRecordOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption;
using AvePoint.RA.RACommonUtility;

namespace AvePoint.RA.RASharePointOnPrem.Report
{
    public class SPOnPremContentDueReportService : SPOnPremReportService
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SPOnPremContentDueReportService));

        private DateTime mTimePoint;
        private DateTime mRunJobTime;
        private string mCurrentTimeZone;
        private string mCurrentTimeZoneId;
        private SOArchiverSettings mArchiverSettings;
        //private Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping;
        private Dictionary<Guid, Rule> mAllRules;
        private RMProfileDto profile;
        private string jobId;
        #region interface
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

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

        private IGeneralSettingService mGeneralSettingService;
        public IGeneralSettingService GeneralSettingService
        {
            get
            {
                if (mGeneralSettingService == null)
                {
                    mGeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
                }
                return mGeneralSettingService;
            }
        }
        #endregion;

        public SPOnPremContentDueReportService(string jobId, string profileId) : base(jobId, profileId)
        {
            try
            {
                this.jobId = jobId;
                ReportMangerFactory.Instance.Init(jobId, JobType.SPOnPremItemsFilesDueDisposal, true);
                profile = ReportService.GetProfileByIdForReportJob(profileId);
                mTimePoint = GetTimePoint(profile.Extension1);
            }
            catch (Exception e)
            {
                mLog.Error($"Report ctor error: {e}");
            }
        }

        public async Task RunReportJobAsync()
        {
            try
            {
                await InitializeAsync();
                base.Process();
            }
            catch (Exception e)
            {
                ReportManager.SetJobFinished(JobStatus.Failed, e.Message);
                mLog.Error($"Run Report Job error:{e}");
            }
            finally
            {
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


        private async Task InitializeAsync()
        {
            ReportManager.StartUpdateJobProgress();

            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            mCurrentTimeZoneId = gls.TimeZoneId;
            mCurrentTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId).ToString();
            mRunJobTime = DateTime.UtcNow;
            //mTermAndRulesMapping = ReportService.GetTermAndRuleMappingsNew(mTimePoint, SourceFlag.SharePointOnPrem);
            mAllRules = RuleManagerService.GetRulesFromRecords().Where(r => r.SPLocalRule != null && r.SPLocalRule.SOFilters != null && r.SPLocalRule.SOFilters.Count != 0).ToDictionary(r => new Guid(r.Id));
            mArchiverSettings = ReportService.GetSOArchiverSettings();
        }

        private DateTime GetTimePoint(string ext1)
        {
            var timePoint = ReportService.GetUtcTimePoint(ext1);
            return timePoint;
        }

        //protected override void ProcessSite(Record record) {

        //}

        protected override int ProcessItem(Record record)
        {
            var result = 0;
            ReportManager.Increase(1);
            DueDisposalReport report = new DueDisposalReport();
            report.SiteCollectionTitle = mSiteTitle;
            Rule rs = null;
            if (record.TermId != Guid.Empty && record.RuleId != Guid.Empty)
            {
                if (record.NodeType == (int)NodeLevel.Folder)
                {
                    mAllRules.TryGetValue(record.RuleId, out rs);
                }
                else if (record.NodeType == (int)NodeLevel.Item)
                {
                    mAllRules.TryGetValue(record.RuleId, out rs);
                }
            }

            if (record.TimeCreated > mTimePoint.Ticks)
            {
                return result;
            }

            if (rs != null)
            {
                var splRule = rs.SPLocalRule;

                if (record.DisposalDueDate != -1 && record.DisposalDueDate > mTimePoint.Ticks)
                {
                    mLog.Warn("record due date is after report time point. The file should not be reported. Record id: {0}.", record.NodeId);
                    return result;
                }

                if (record.DeclareAsRecord)
                {
                    if (!splRule.DeleteRecords && !RuleHelper.CheckMoveRule(splRule))//Get from message & merge contract
                    {
                        mLog.Warn("File is record and option is not delete record, record id {0}", record.NodeId);
                        return result;
                    }
                }

                var disposalAction = RuleHelper.GetOperationTypeForSPLocal(splRule);
                if (record.HoldReleaseTime > mTimePoint.Ticks
                    && !(disposalAction == (int)RMContentDisposalAction.Move || disposalAction == (int)RMContentDisposalAction.MoveDeclare || disposalAction == (int)RMContentDisposalAction.KeepData || disposalAction == (int)RMContentDisposalAction.ExportOnly))
                {
                    mLog.Warn("File is on explorer hold. The file should not be reported. Record id: {0}.RuleAction:{1}.", record.NodeId, disposalAction);
                    return result;
                }

                report.AppliedRuleId = rs.Id;
                report.AppliedRuleName = rs.Name;
                report.DisposalAction = disposalAction;
                report.ManualApproval = splRule.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
                report.ExportType = (RMExportTypeValue)(splRule.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : splRule.ExportInfo.exportType);
                report.DisposalClass = rs.DisposalClass;

                report.RelatedRecordsAction = (int)splRule.RelatedRecordOption;
                BuildRelatedRecords(ref report, record, mSiteUrl);
            }
            else
            {
                mLog.Info("Item not fit rule, record id {0}", record.NodeId);
                return result;
            }

            try
            {
                BuildReport(report, record, record.TermId, record.TermName);
            }
            catch
            {
                throw;
            }
            finally
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    ReportManager.SendJobReport(report);
                });
                result = 1;
            }

            return result;
        }

        private void BuildReport(DueDisposalReport report, Record record, Guid termId, string termName)
        {
            try
            {

                string itemUrl = WebUtil.MakeFullUrl(mSiteUrl, record.DirPath);
                report.TitleOrName = record.LeafName;
                report.BCSTermId = termId.ToString();
                report.BCSTermName = termName;
                if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                {
                    report.ObjectLevel = (int)RMReportObjectLevel.Item;
                    report.Url = GetListItemRealPath(record.ListId, record.DirPath);
                }
                else
                {
                    report.Url = itemUrl;
                    report.ObjectLevel = (int)RMReportObjectLevel.Document;
                }
                report.CreatedBy = record.CreatedBy;
                report.CreatedTime = ConvertTimeFromUtc(record.TimeCreated, mCurrentTimeZoneId);
                report.LastModifiedBy = record.ModifiedBy;
                report.LastModifiedTime = ConvertTimeFromUtc(record.TimeModified, mCurrentTimeZoneId);
                report.SPWebTimeZoneName = mCurrentTimeZone;
                // check document is skip file
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
                mLog.Info("build item report error, record id {0}", record.NodeId);
                report.Status = RMReportStatus.Failed;
                report.Comment = "RM_JM_ReportComment_Failed";
                throw;
            }
        }

        private void BuildRelatedRecords(ref DueDisposalReport report, Record item, string siteUrl)
        {
            try
            {
                using (PerformanceScope scope6 = new PerformanceScope("DueDisposalReportProcessor.RelatedRecord", addToStatistics: true))
                {
                    List<ReportRelatedRecords> allSourceReportRelatedRecords = new List<ReportRelatedRecords>();
                    List<ReportRelatedRecords> electronicReportRelatedRecords = new List<ReportRelatedRecords>();
                    List<ReportRelatedRecords> physicalReportRelatedRecords = new List<ReportRelatedRecords>();

                    if (!string.IsNullOrEmpty(item.RelatedRecords))
                    {
                        List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(item.RelatedRecords);
                        if (infos != null && infos.Count > 0)
                        {
                            foreach (var rProp in infos)
                            {
                                if (rProp.SourceFlag == (int)SourceFlag.Physical)
                                {
                                    physicalReportRelatedRecords.Add(new ReportRelatedRecords() { Name = rProp.recId, Url = "" });
                                }
                                else
                                {
                                    string itemFullUrl = string.Empty;
                                    if (!rProp.url.StartsWith(siteUrl))
                                    {
                                        itemFullUrl = AvePoint.RA.Common.Util.WebUtil.MakeFullUrl(siteUrl, rProp.url);
                                    }
                                    else
                                    {
                                        itemFullUrl = rProp.url;
                                    }
                                    electronicReportRelatedRecords.Add(new ReportRelatedRecords() { Name = rProp.name, Url = itemFullUrl });
                                }
                            }
                            allSourceReportRelatedRecords.AddRange(electronicReportRelatedRecords);
                            allSourceReportRelatedRecords.AddRange(physicalReportRelatedRecords);
                            report.RelatedRecords = SerializerHelper.SerializeToXmlString(allSourceReportRelatedRecords);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                mLog.Warn("get related record info error{0}", e.ToString());
            }
        }
    }
}
