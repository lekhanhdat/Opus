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
using RAReportCenter.CreateAndDestryoedReport.Scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.CreateAndDestryoedReport
{
    public class CreateAndDestryoedReportProcessor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(CreateAndDestryoedReportProcessor));

        private static readonly ICreateAndDestryoedReportService CreateAndDestryoedReportService = PlatformWindsorManager.GetService<ICreateAndDestryoedReportService>();

        private static readonly Dictionary<SourceFlag, Action<CreateAndDestryoedReportModel>> ScanFuncs =
            new Dictionary<SourceFlag, Action<CreateAndDestryoedReportModel>>
            {
                { SourceFlag.SharePoint, SharePointReportScan }
            };

        public static void Process(string jobId, int profileId)
        {
            try
            {
                CreateAndDestryoedReportJobManager.Init(jobId);
                var reportModel = CreateAndDestryoedReportService.Get(profileId).Result;
                if (reportModel == null || !ScanFuncs.TryGetValue(reportModel.Source, out var scanFunc))
                {
                    CreateAndDestryoedReportJobManager.SetJobFailed("");
                    return;
                }

                scanFunc(reportModel);
                CreateAndDestryoedReportJobManager.SetJobFinished();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred process create and destryoed report: [{profileId}] job: [{jobId}]. Error: {e}");
                CreateAndDestryoedReportJobManager.SetJobFailed(e.Message);
            }
        }

        public static void SharePointReportScan(CreateAndDestryoedReportModel reportModel)
        {
            //var scanner = new SharePointOnlineCreateAndDestryoedReportScanner(reportModel);
            //scanner.Scan();
        }
    }
}
