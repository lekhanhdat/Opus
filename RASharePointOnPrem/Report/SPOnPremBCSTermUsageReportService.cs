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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RASharePointOnPrem.Report.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RASharePointOnPrem.Report
{
    public class SPOnPremBCSTermUsageReportService: SPOnPremReportService
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SPOnPremBCSTermUsageReportService));
        private Dictionary<Guid, RMTermIdentity> mUsageTermInfo;
        private bool isOrphanedTermReport;
        private bool mIsRetiredTermReport;
        private ITermDao TermDao;

        public SPOnPremBCSTermUsageReportService(string jobId, string profileId, bool IsOrphanedTermReport, bool isRetiredTermReport) : base(jobId, profileId)
        {
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
            isOrphanedTermReport = IsOrphanedTermReport;
            mIsRetiredTermReport = isRetiredTermReport;
            if (IsOrphanedTermReport)
            {
                mUsageTermInfo = ReportService.GetOrphanedTermsOfRMAsync().Result;
            }
            else if (isRetiredTermReport)
            {
                mUsageTermInfo = ReportService.GetRetiredTermsOfRMAsync().Result;
            }
            else
            {
                mUsageTermInfo = ReportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1).Result;
            }
            TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            SendUsageTermDetails();
        }

        public void RunReportJob()
        {
            try
            {
                base.Process();
            }
            catch (Exception e)
            {
                ReportManager.SetJobFinished(JobStatus.Failed, e.Message);
                mLog.Error($"Run Report Job error:{e}");
            }
        }

        protected override int ProcessItem(Record record)
        {
            var result = 0;
            BCSTermUsageReport report = null;
            ReportManager.Increase(1);
            try
            {
                if (IsMatchTerm(record.TermId))
                {
                    report = GenerateReportItem(record);
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

        private BCSTermUsageReport GenerateReportItem(Record record)
        {
            BCSTermUsageReport report = new BCSTermUsageReport();
            report.TitleOrName = record.LeafName;
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
            {
                report.Url = GetListItemRealPath(record.ListId, record.DirPath);
                report.ObjectLevel = (int)RMReportObjectLevel.Item;
            }
            else
            {
                report.Url = WebUtil.MakeFullUrl(mSiteUrl, record.DirPath);
                report.ObjectLevel = (int)RMReportObjectLevel.Document;
            }
            var termUniqueId = record.TermId;
            report.BCSTermId = termUniqueId.ToString();
            report.BCSTermName = mUsageTermInfo[termUniqueId].Name;
            report.TermStatus = mUsageTermInfo[termUniqueId].Status;
            report.BCSTermFullPath = mUsageTermInfo[termUniqueId].FullPath;
            report.CreatedBy = record.CreatedBy;
            report.CreatedTime = record.TimeCreated;
            report.LastModifiedBy = record.ModifiedBy;
            report.LastModifiedTime = record.TimeModified;
            return report;
        }

        private bool IsMatchTerm(Guid termId)
        {
            return mUsageTermInfo.ContainsKey(termId);
        }

        private void SendUsageTermDetails()
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
            ReportManager.BatchSendJobDetail(details);
        }
    }
}
