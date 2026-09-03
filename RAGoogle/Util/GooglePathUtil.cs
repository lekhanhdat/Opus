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
using AvePoint.RA.Contract.JobMonitor;

namespace RAGoogle.Util
{
    public class GooglePathUtil
    {
        private static readonly string _defaultDBFilePrefix = "GoogleDisposalRecords_";

        public static string GetJobReportPath(string jobId)
        {
            var separator = Path.DirectorySeparatorChar;
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { separator }).LastIndexOf(separator.ToString()));
            var reportPath = Path.Combine(basePath, "JobReports", (JobType.GoogleRecordsDisposal).ToString(), $"{jobId}.rpt");
            return reportPath;
        }

        public static string GetDisposalRecordDBPath(string jobId)
        {
            var separator = Path.DirectorySeparatorChar;
            string dbName = _defaultDBFilePrefix + jobId + ".db";
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { separator }).LastIndexOf(separator.ToString()));
            var reportPath = Path.Combine(basePath, "GoogleDisposalRecords", jobId, dbName);
            return reportPath;
        }

        public static string GenerateDisposalTempPath(string jobId)
        {
            var separator = Path.DirectorySeparatorChar;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { separator }).LastIndexOf(separator.ToString()));
            return Path.Combine(basePath, "DiposalTemp", jobId);
        }
    }
}
