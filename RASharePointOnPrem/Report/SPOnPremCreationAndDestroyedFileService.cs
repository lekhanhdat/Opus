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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RASharePointOnPrem.Report.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RASharePointOnPrem.Report
{
    public class SPOnPremCreationAndDestroyedFileService : SPOnPremReportService
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SPOnPremCreationAndDestroyedFileService));
        private List<RMTerm> Terms { get; set; }
        private bool SelectCreated;
        private bool SelectDestroyed;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private string profileId;

        private Dictionary<int, RMAccount> cacheAllUsers;

        private Dictionary<Guid, RMRuleInfos> idRuleInfoDic = new Dictionary<Guid, RMRuleInfos>();
        private IRuleManagerService mRuleManagerService;
        protected IRuleManagerService RuleManagerService
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

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        public SPOnPremCreationAndDestroyedFileService(string jobId, string profileId) : base(jobId, profileId)
        {
            try
            {
                ReportMangerFactory.Instance.Init(jobId, JobType.SPOnPremCreateAndDestroyedFileReport, true);
                RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
                cacheAllUsers = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            }
            catch (Exception e)
            {
                mLog.Error($"Report ctor error: {e}");
            }
        }


        public async Task RunJobAsync(RMCreationJobMessage msg)
        {
            await InitParametersAsync(msg);
            base.Process();
        }

        private async Task InitParametersAsync(RMCreationJobMessage msg)
        {
            ReportManager.StartUpdateJobProgress();
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.EndTime.AddDays(1), globalTimeZone);//包含当天
            SelectCreated = msg.SelectCreated;
            SelectDestroyed = msg.SelectDestroyed;
            profileId = msg.ProfileId;
            LoadTerms();
            await InitRulesInfoAsync();
        }

        private void LoadTerms()
        {
            mLog.Info("Begin to load terms.");
            Terms = new TermDao().GetAllTermsForce();
            mLog.Info("Loaded {0} terms.", Terms.Count);
        }

        private async Task InitRulesInfoAsync()
        {
            using (var performance = new PerformanceScope($"Report.GetRules"))
            {
                var dbRules = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                if (dbRules.Count > 0)
                {
                    idRuleInfoDic = dbRules.ToDictionary(key => new Guid(key.RuleId), value => value);
                }
            }
        }

        private RMRuleInfos GetRuleInfo(Guid id)
        {
            return idRuleInfoDic.ContainsKey(id) ? idRuleInfoDic[id] : null;
        }

        protected override int ProcessItem(Record record)
        {
            var result = 0;
            CreateAndDestroyedFileReport report = null;
            var createResult = false;
            var destoryResult = false;
            ReportManager.Increase(1);
            try
            {
                if (SelectCreated)
                {
                    createResult = IsMatchOnCreateTime(record);
                }
                if (SelectDestroyed)
                {
                    destoryResult = IsMatchOnDestroyedTime(record);
                }
                if (createResult)
                {
                    report = GenerateReportItem(record, true, false);
                }
                if (destoryResult)
                {
                    report = GenerateReportItem(record, false, true);
                }
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

        protected override int[] GetProcessRecordStatus()
        {
            return new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Destroyed,  
                (int)RMRecordStatus.Closed, (int)RMRecordStatus.Missing };
        }

        protected override void SendJobReportDetails(Record item, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail
            {
                ObjectLevel = JobReportUtility.ConvertItemTypeForDetails((NodeLevel)item.NodeType),
                Title = item.LeafName,
                URL = WebUtil.MakeFullUrl(mSiteUrl, item.DirPath),
                Status = status,
                Comment = comments
            };
            ReportManager.SendJobDetail(detail);
        }


        private CreateAndDestroyedFileReport GenerateReportItem(Record record, bool Created, bool Destroyed)
        {
            var report = new CreateAndDestroyedFileReport();
            if (Terms.Any(o => o.UniqueId == record.TermId))
            {
                report.TermName = Terms.Where(o => o.UniqueId == record.TermId).First().Name;
            }
            report.Title = record.LeafName;
            report.CreatedTime = record.TimeCreated;
            report.LastModifiedTime = record.TimeModified;
            report.FileType = record.ExtensionForFile;
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
            {
                report.Url = GetListItemRealPath(record.ListId, record.DirPath);
                report.ObjectLevel = (int)RMReportObjectLevel.Item;
                report.LevelStr = (int)RMReportObjectLevel.Item;
            }
            else
            {
                report.Url = WebUtil.MakeFullUrl(mSiteUrl, record.DirPath);
                report.ObjectLevel = (int)RMReportObjectLevel.Document;
                report.LevelStr = (int)RMReportObjectLevel.Document;
            }


            if (Created)
            {
                report.OperationTime = record.TimeCreated.Equals(DateTime.MinValue) ? string.Empty : record.TimeCreated.ToString();
                report.OperationBy = record.CreatedBy;
                report.Operation = (int)OperationType.Created;
            }
            if (Destroyed)
            {
                if(record?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                {
                    if (record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                        report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                    }
                    else
                    {
                        report.ApprovalStatus = record.ManualApprovedStatus;
                        report.InternalApprovedStatus = record.ManualInternalApprovedStatus;
                    }
                }
                report.DisposalClass = GetRuleInfo(record.RuleId)?.DisposalClass;
                report.OperationTime = record.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : record.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                report.Operation = (int)OperationType.Destroyed;
                report.RecordsId = record.RecordsId;
                report.RuleName = GetRuleInfo(record.RuleId)?.RuleName;
                if (cacheAllUsers.TryGetValue(record.ManualApprovedBy, out RMAccount approveUser) && record.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
            }
            return report;
        }

        private bool IsMatchOnCreateTime(Record record)
        {
            bool result = false;
            if (record != null && record.TimeCreated > startUtcTime.Ticks && record.TimeCreated < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }

        private bool IsMatchOnDestroyedTime(Record record)
        {
            bool result = false;
            if (record != null && record.DestroyedTime > startUtcTime.Ticks && record.DestroyedTime < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }
    }
    internal enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }
}


