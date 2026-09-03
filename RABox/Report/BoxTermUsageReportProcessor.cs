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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Explorer.Model;
using RABox.Converters;
using RABox.Report.Base;

namespace RABox.Report
{
    public class BoxTermUsageReportProcessor : ReportProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(BoxTermUsageReportProcessor));

        private readonly bool isOrphanedTermReport;
        private readonly bool isRetiredTermReport;

        public BoxTermUsageReportProcessor(string jobId, JobType jobType, bool isOrphanedTermReport, bool isRetiredTermReport, string profileId) : base(profileId)
        {
            JobId = jobId;
            JobType = jobType;
            this.isOrphanedTermReport = isOrphanedTermReport;
            this.isRetiredTermReport = isRetiredTermReport;
        }

        protected override void Initialize()
        {
            var termIdentities = ReportCenter.GetTermsOfRMAsync(ProfileDto, isOrphanedTermReport, isRetiredTermReport).Result;
            SendUsageTermDetails(termIdentities);
        }

        private void SendUsageTermDetails(Dictionary<Guid, RMTermIdentity> termIdentities)
        {
            var details = TermManager.GetTermSelections(termIdentities);

            ReportCenter.BatchSendJobDetail(details);
        }

        protected override void ProcessFiles(Guid folderId)
        {
            bool hasNext = true;
            string pageIndex = string.Empty;
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = RecordManager.QueryFileRecordsByParent(folderId, pageIndex, RMRecordStatus.Destroyed);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                List<Record> datas = result.Item1.ToList();
                foreach (Record file in datas)
                {
                    ProcessFile(file);
                }
            }
        }

        protected override void ProcessFile(Record record)
        {
            try
            {
                if (TermManager.TryGetUsageTermInfo(record.TermId, out var termIdentity))
                {
                    ReportCenter.SendReport(GenerateTermUsageReport(record, termIdentity), record.GenerateReportJobDetail());
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Process file has error:{ex}");
                ReportCenter.RecordFailed(record.GenerateReportJobDetail(ex.Message), record.NodeType);
            }
        }

        private BCSTermUsageReport GenerateTermUsageReport(Record record, RMTermIdentity termIdentity)
        {
            BCSTermUsageReport report = new BCSTermUsageReport();
            report.TitleOrName = record.LeafName;
            report.Url = record.DirPath;

            if (record.NodeType == (int)NodeLevel.BoxFolder)
            {
                report.ObjectLevel = (int)RMReportObjectLevel.BoxFolder;
            }
            else
            {
                report.ObjectLevel = (int)RMReportObjectLevel.BoxFile;

                report.BCSTermId = termIdentity.UniqueId.ToString();
                report.BCSTermName = termIdentity.Name;
                report.TermStatus = termIdentity.Status;
                report.BCSTermFullPath = termIdentity.FullPath;
            }

            report.CreatedBy = record.CreatedBy;
            report.CreatedTime = record.TimeCreated;
            report.LastModifiedBy = record.ModifiedBy;
            report.LastModifiedTime = record.TimeModified;

            return report;
        }
    }
}