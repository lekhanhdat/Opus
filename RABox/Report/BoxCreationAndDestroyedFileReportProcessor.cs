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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using RABox.Converters;
using RABox.Report.Base;

namespace RABox.Report
{
    public class BoxCreationAndDestroyedFileReportProcessor : ReportProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(BoxCreationAndDestroyedFileReportProcessor));

        private readonly bool SelectCreated;
        private readonly bool SelectDestroyed;
        private readonly DateTime startUtcTime;
        private readonly DateTime endUtcTime;
        private Dictionary<int, RMAccount> cacheAllUsers;
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public BoxCreationAndDestroyedFileReportProcessor(RMCreationJobMessage msg) : base(msg.ProfileId)
        {
            JobId = msg.JobID;
            JobType = msg.JobType;
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.EndTime.AddDays(1), globalTimeZone);
            SelectCreated = msg.SelectCreated;
            SelectDestroyed = msg.SelectDestroyed;
            cacheAllUsers = AccountDao.FindAll().ToDictionary(key => key.Id, value => value);
        }

        protected override void Initialize()
        {
            TermManager.LoadTerms();
            RuleManager.InitRulesInfoAsync().Wait();
        }

        protected override void ProcessFolder(Record record)
        {
            try
            {
                ReportCenter.RecordSuccessful(record.GenerateCreateAndDestroyedReportJobDetail(), record.NodeType);
                ProcessFiles(record.Id);
            }
            catch (Exception ex)
            {
                _logger.Error($"Process folder has error:{ex}");
                ReportCenter.RecordFailed(record.GenerateCreateAndDestroyedReportJobDetail(ex.Message), record.NodeType);
            }
        }

        protected override void ProcessFiles(Guid folderId)
        {
            bool hasNext = true;
            string pageIndex = string.Empty;
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = RecordManager.QueryFileRecordsByParent(folderId, pageIndex, RMRecordStatus.Moved);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                List<Record> datas = result.Item1.ToList();
                foreach (var file in datas)
                {
                    ProcessFile(file);
                }
            }
        }

        protected override void ProcessFile(Record record)
        {
            try
            {
                var createResult = false;
                var destroyResult = false;
                if (SelectCreated)
                {
                    createResult = IsMatchOnCreateTime(record);
                }
                if (SelectDestroyed)
                {
                    destroyResult = IsMatchOnDestroyedTime(record);
                }
                if (createResult)
                {
                    ReportCenter.SendReport(GenerateCreateAndDestroyedReport(record, OperationType.Created), record.GenerateCreateAndDestroyedReportJobDetail());
                }
                if (destroyResult)
                {
                    ReportCenter.SendReport(GenerateCreateAndDestroyedReport(record, OperationType.Destroyed), record.GenerateCreateAndDestroyedReportJobDetail());
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Process box has error:{ex}");
                ReportCenter.RecordFailed(record.GenerateCreateAndDestroyedReportJobDetail(ex.Message), record.NodeType);
            }
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

        private CreateAndDestroyedFileReport GenerateCreateAndDestroyedReport(Record boxNode, OperationType operationType)
        {
            var report = new CreateAndDestroyedFileReport();

            if (TermManager.TryGetTerm(boxNode.TermId, out var term))
            {
                report.TermName = term.Name;
            };

            report.Title = boxNode.LeafName;
            report.LevelStr = boxNode.NodeType;
            report.Url = boxNode.DirPath;
            report.CreatedTime = boxNode.TimeCreated;
            report.LastModifiedTime = boxNode.TimeModified;
            report.FileType = boxNode.ExtensionForFile;
            if (operationType == OperationType.Created)
            {
                report.OperationTime = boxNode.TimeCreated.Equals(DateTime.MinValue) ? string.Empty : boxNode.TimeCreated.ToString();
                report.OperationBy = boxNode.CreatedBy;
                report.Operation = (int)operationType;
            }
            else
            {
                if (cacheAllUsers.TryGetValue(boxNode.ManualApprovedBy, out RMAccount approveUser) && boxNode.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
                report.RecordsId = boxNode.RecordsId;
                report.DisposalClass = RuleManager.TryGetRuleInfo(boxNode.RuleId, out var ruleInfo) ? ruleInfo.DisposalClass : null;
                report.RuleName = ruleInfo.RuleName ?? string.Empty;
                report.OperationTime = boxNode.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : boxNode.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                report.Operation = (int)operationType;
                report.ApprovalStatus = boxNode.ManualApprovedStatus;
                report.InternalApprovedStatus = boxNode.ManualInternalApprovedStatus;
                if (boxNode.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                {
                    if (boxNode.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove
                        || boxNode.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                        report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                    }
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

    }
    internal enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }
}