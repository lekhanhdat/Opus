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
using AvePoint.RA.RAPhysical.Cache;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Report.Interface;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb;
using System.Text;
using AvePoint.RA.Contract.Explorer;
using System.Linq.Expressions;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Report
{
    public class PRTermUsageReportService : IPRTermUsageReportService
    {
        public IPRTermService PRTermService { get; set; }

        public IPRReportProcessor PRReportProcessor { get; set; }

        private Dictionary<Guid, RMTermIdentity> mUsageTermInfo;

        private static readonly RALogger mLog = RALogger.GetInstance(typeof(PRTermUsageReportService));

        public async Task RunReportJobAsync(string jobId, string profileId)
        {
        }

        public async Task RunReportJobAsync(string jobId, string profileId, bool reportOrphanedTerm, bool reportRetiredTerm)
        {
            await InitUsageTermInfosAsync(profileId, reportOrphanedTerm, reportRetiredTerm);
            if (mUsageTermInfo == null || mUsageTermInfo.Count == 0)
            {
                PRReportProcessor.ReportManager.SetJobFinished(JobStatus.Failed, "RM_RC_TUR_NoTermForReport");
                return;
            }
            var options = new ReportOptions()
            {
                JobId = jobId,
                JobType = JobType.PhysicalTermUsageReport,
                ProfileId = profileId,
                IsUseBuiltInNormalLocationAction = false,
                IsUseBuiltInBottomLocationAction = false,
                BrowseOptions = new BrowseOptions() { NeedProcessBox = false, NeedProcessFile = false },
                OtherDetails = GetUsageTermDetails(),
            };

            await PRReportProcessor
                .ConfigTreeAction(treeService =>
                {
                    treeService
                    .ConfigNormalLocationAction(ProcessLocation)
                    .ConfigBottomLocationAction(ProcessLocation);
                    return Task.CompletedTask;
                })
                .ProcessAsync(options);
        }

        public async System.Threading.Tasks.Task ProcessLocation(IPhysicalLocation location)
        {
            try
            {
                mLog.Info($"Process location {location.DirPath}");
                GetBoxes(location)?.ForEach(b => ProcessBox(b));
                GetFiles(location)?.ForEach(f => ProcessFile(f));
                SendDetail(location, JobDetailsStatus.Successful);
            }
            catch (Exception e)
            {
                mLog.Error($"Process location error:{e.ToString()}");
                SendDetail(location, JobDetailsStatus.Failed, e.Message);
                throw;
            }
        }

        public void ProcessBox(IPhysicalBox box)
        {
            var boxFullPath = box.DirPath;
            try
            {
                if (IsMatchTerm(box.TermId))
                {
                    SendReport(box, boxFullPath);
                }
                GetFiles(box)?.ForEach(ProcessFile);
            }
            catch (Exception ex)
            {
                mLog.Error($"An error occured while process physical box {boxFullPath}, message:{ex.Message}");
            }
        }

        public void ProcessFile(IPhysicalFile file)
        {
            var fileFullPath = file.DirPath;
            try
            {
                if (IsMatchTerm(file.TermId))
                {
                    SendReport(file, fileFullPath);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"An error occured while process physical file {fileFullPath}, message:{ex.Message}");
            }
        }

        private List<IPhysicalFile> GetFiles(IPhysicalLocation location)
        {
            return location.GetFiles(GetQueryCondition());
        }

        private List<IPhysicalFile> GetFiles(IPhysicalBox box)
        {
            return box.GetFiles(GetQueryCondition());
        }

        private List<IPhysicalBox> GetBoxes(IPhysicalLocation location)
        {
            return location.GetBoxes(GetQueryCondition());
        }

        private async Task InitUsageTermInfosAsync(string profileId, bool reportOrphanedTerm, bool reportRetiredTerm)
        {
            if (reportOrphanedTerm)
            {
                mUsageTermInfo = await PRReportProcessor.mRMReportService.GetOrphanedTermsOfRMAsync();
            }
            else if (reportRetiredTerm)
            {
                mUsageTermInfo = await PRReportProcessor.mRMReportService.GetRetiredTermsOfRMAsync();
            }
            else
            {
                var profile = PRReportProcessor.mRMReportService.GetProfileByIdForReportJob(profileId);
                mUsageTermInfo = await PRReportProcessor.mRMReportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1);
            }
        }

        private List<JMJobDetails> GetUsageTermDetails()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            foreach (var term in mUsageTermInfo.Values)
            {
                details.Add(new JMTermSelection()
                {
                    Term = term.Name,
                    TermFullPath = term.FullPath
                });
            }
            return details;
        }

        private Expression<Func<Record, bool>> GetQueryCondition()
        {
            Expression<Func<Record, bool>> condition = b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed
            || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed);
            return condition;
        }

        private bool IsMatchTerm(Guid termId)
        {
            return mUsageTermInfo.ContainsKey(termId);
        }

        private void SendReport(IPhysicalBox box, string path)
        {
            var report = new BCSTermUsageReport();
            report.TitleOrName = box.Name;
            report.Url = path;
            report.ObjectLevel = (int)RMReportObjectLevel.PhyBox;
            var termUniqueId = box.TermId;
            report.BCSTermId = termUniqueId.ToString();
            report.BCSTermName = mUsageTermInfo[termUniqueId].Name;
            report.TermStatus = mUsageTermInfo[termUniqueId].Status;
            report.BCSTermFullPath = mUsageTermInfo[termUniqueId].FullPath;
            report.CreatedBy = box.CreateBy;
            report.CreatedTime = box.CreateTimeTicks;
            report.LastModifiedBy = box.ModifiedBy;
            report.LastModifiedTime = box.ModifiedTimeTicks;
            PRReportProcessor.AddJobReport(report);
        }

        private void SendReport(IPhysicalFile file, string path)
        {
            var report = new BCSTermUsageReport();
            report.TitleOrName = file.Name;
            report.Url = path;
            report.ObjectLevel = (int)RMReportObjectLevel.PhyFolder;
            var termUniqueId = file.TermId;
            report.BCSTermId = termUniqueId.ToString();
            report.BCSTermName = mUsageTermInfo[termUniqueId].Name;
            report.TermStatus = mUsageTermInfo[termUniqueId].Status;
            report.BCSTermFullPath = mUsageTermInfo[termUniqueId].FullPath;
            report.CreatedBy = file.CreateBy;
            report.CreatedTime = file.CreateTimeTicks;
            report.LastModifiedBy = file.ModifiedBy;
            report.LastModifiedTime = file.ModifiedTimeTicks;
            PRReportProcessor.AddJobReport(report);
        }

        private void SendDetail(IPhysicalLocation location, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = "RM_Common_ObjectLevel_PhysicalLocation";
            detail.TitleOrName = location.Name;
            detail.Url = location.DirPath;
            detail.Status = status;
            detail.Comment = comments;
            PRReportProcessor.AddJobDetail(detail);
        }

    }
}
