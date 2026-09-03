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
using RAReportCenter.DisposalReport.Scanner;
using System;
using System.Collections.Generic;


namespace RAReportCenter.DisposalReport
{
    public class DisposalReportProcessor
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DisposalReportProcessor));

        private static readonly IDisposalReportService DisposalReportService = PlatformWindsorManager.GetService<IDisposalReportService>();

        private static readonly Dictionary<SourceFlag, Action<DisposalReportModel>> ScanFuncs =
            new Dictionary<SourceFlag, Action<DisposalReportModel>>
            {
                { SourceFlag.SharePoint, SharePointReportScan }
            };

        public static void Process(string jobId, int profileId)
        {
            try
            {
                DisposalReportJobManager.Init(jobId);
                var reportModel = DisposalReportService.Get(profileId).Result;
                if (reportModel == null || !ScanFuncs.TryGetValue(reportModel.Source, out var scanFunc))
                {
                    DisposalReportJobManager.SetJobFailed("");
                    return;
                }

                scanFunc(reportModel);
                DisposalReportJobManager.SetJobFinished();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred process disposal report: [{profileId}] job: [{jobId}]. Error: {e}");
                DisposalReportJobManager.SetJobFailed(e.Message);
            }
        }

        public static void SharePointReportScan(DisposalReportModel reportModel)
        {
            //var scanner = new SharePointOnlineDisposalReportScanner(reportModel);
            //scanner.Scan();
        }
    }
}
