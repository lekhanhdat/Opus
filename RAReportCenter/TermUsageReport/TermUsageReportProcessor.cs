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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.ReportCenter.Model;
using RAReportCenter.TermUsageReport.Scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.TermUsageReport
{
    public class TermUsageReportProcessor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TermUsageReportProcessor));

        private static readonly ITermUsageReportService TermUsageReportService = PlatformWindsorManager.GetService<ITermUsageReportService>();

        private static readonly Dictionary<SourceFlag, Action<TermUsageReportModel>> ScanFuncs =
            new Dictionary<SourceFlag, Action<TermUsageReportModel>>
            {
                { SourceFlag.SharePoint, SharePointReportScan }
            };

        public static void Process(string jobId, int profileId)
        {
            try
            {
                TermUsageReportJobManager.Init(jobId);
                var reportModel = TermUsageReportService.Get(profileId).Result;
                if (reportModel == null || !ScanFuncs.TryGetValue(reportModel.Source, out var scanFunc))
                {
                    TermUsageReportJobManager.SetJobFailed("");
                    return;
                }

                scanFunc(reportModel);
                TermUsageReportJobManager.SetJobFinished();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred process disposal report: [{profileId}] job: [{jobId}]. Error: {e}");
                TermUsageReportJobManager.SetJobFailed(e.Message);
            }
        }

        public static void SharePointReportScan(TermUsageReportModel reportModel)
        {
            //var scanner = new SharePointOnlineTermUsageReportScanner(reportModel);
            //scanner.Scan();
        }
    }
}
