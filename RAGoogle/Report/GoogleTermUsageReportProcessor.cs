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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Model;
using RAGoogle.Extension;
using RAGoogle.Models;
using Util;

namespace RAGoogle.Report
{
    public class GoogleTermUsageReportProcessor : BaseReportProcessor
    {
        #region properties
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleTermUsageReportProcessor));

        private readonly bool isOrphanedTermReport;
        private readonly bool isRetiredTermReport;

        private Dictionary<Guid, RMTermIdentity>? _usageTermInfo;
        #endregion

        public GoogleTermUsageReportProcessor(string jobId, string profileId, bool isOrphanedTermReport, bool isRetiredTermReport) : base(jobId, profileId)
        {
            this.jobType = JobType.GoogleBCSTermUsageReport;
            this.isOrphanedTermReport = isOrphanedTermReport;
            this.isRetiredTermReport = isRetiredTermReport;
        }

        protected override void InitializeReport()
        {
            if (isOrphanedTermReport)
            {
                _usageTermInfo = ReportService.GetOrphanedTermsOfRMAsync().Result;
            }
            else if (isRetiredTermReport)
            {
                _usageTermInfo = ReportService.GetRetiredTermsOfRMAsync().Result;
            }
            else
            {
                _usageTermInfo = ReportService.GetTermIDsFromBCSTermTreeAsync(ProfileDto.Extension1).Result;
            }

            if (_usageTermInfo == null || _usageTermInfo.Count == 0)
            {
                throw new Exception("RM_RC_TUR_NoTermForReport");
            }
            SendJobReportSummary();
        }

        protected override async Task ProcessDriveAsync(GoogleDriveTreeNodeDto treeNode, DataQueue<GoogleItemData> itemQueue)
        {
            logger.Info($"Start processing node [{treeNode.ID}-{treeNode.Name}].");
            using (var performance = new PerformanceScope("GoogleTermUsageReportProcessor:ProcessDriveAsync"))
            using (CheckJobStopScope subJScope = new CheckJobStopScope())
            {
                try
                {
                    if (treeNode.Level == NodeLevel.GoogleMyDrive || treeNode.Level == NodeLevel.GoogleSharedDrive)
                    {
                        await ProcessScanTimeRangeDriveAsync(treeNode, itemQueue, default, default);
                    }
                }
                catch (JobStopException)
                {
                    _logger.Warn("The term usage report job has been stopped.");
                    throw new JobStopException("The job has stopped."); ;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to process term usage report job, Message: {ex}");
                    throw;
                }
            }
        }

        protected override void ProcessFileReport(GoogleItemData file)
        {
            using (CheckJobStopScope jScope = new())
            {
                foreach (var labelId in file.LableIds)
                {
                    var rmTerms = new List<RMTerm>();
                    if (isOrphanedTermReport)
                    {
                        rmTerms = TermDao.GetRMTermsByLabelId(labelId, true);
                    }
                    else
                    {
                        rmTerms = TermDao.GetRMTermsByLabelId(labelId);
                    }
                    foreach (var rmTerm in rmTerms)
                    {
                        if (rmTerm != null && _usageTermInfo.TryGetValue(rmTerm.UniqueId, out var termIdentity))
                        {
                            ReportCenter.SendReport(GenerateTermUsageReport(file, termIdentity), file.GenerateReportJobDetail());
                        }
                    }
                }
            }

        }

        private void SendJobReportSummary()
        {
            List<JMJobDetails> details = new();
            foreach (var term in _usageTermInfo.Values)
            {
                details.Add(new JMTermSelection()
                {
                    Term = term.Name,
                    TermFullPath = term.FullPath
                });
            }
            ReportCenter.RecordSuccessfulBulk(details, (int)RMNodeLevel.GoogleDrive);
        }

        private BCSTermUsageReport GenerateTermUsageReport(GoogleItemData item, RMTermIdentity termIdentity)
        {
            return new BCSTermUsageReport()
            {
                TitleOrName = item.Name,
                Url = item.RelativePath,
                ObjectLevel = item.Level == RMNodeLevel.GoogleFile ? (int)RMReportObjectLevel.GoogleFile : (int)RMReportObjectLevel.GoogleFolder,
                BCSTermId = termIdentity.UniqueId.ToString(),
                BCSTermName = termIdentity.Name,
                TermStatus = termIdentity.Status,
                BCSTermFullPath = termIdentity.FullPath,
                CreatedBy = item.CreatedBy,
                CreatedTime = item.CreatedTime.Ticks,
                LastModifiedBy = item.ModifiedBy,
                LastModifiedTime = item.ModifiedTime.Ticks,
            };
        }
    }
}
