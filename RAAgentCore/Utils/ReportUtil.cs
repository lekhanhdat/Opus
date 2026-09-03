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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.FileSystem.Core
{
    public class ReportUtil
    {
        private static readonly string _defaultDBFilePrefix = "FileSystemDueRecords_";
        public static string GetJobReportPath(BaseJobDto baseJobDto) 
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { '\\' }).LastIndexOf("\\"));

            var module = ((JobType)baseJobDto.JobType).ToString();
            if (baseJobDto.JobType == (int)JobType.FSRetainSimulate)
            {
                module = "ArchiverRetentionSimulate";
            }

            var reportPath = $"{basePath}\\JobReports\\{module}\\{baseJobDto.Id}.rpt";
            return reportPath;
        }
        /// <summary>
        /// 获取缓存Cosmos数据的LiteDB Path信息
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public static string GetDisposalDueRecordDBPath(string jobId)
        {
            string dbName = _defaultDBFilePrefix + jobId + ".db";
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { '\\' }).LastIndexOf("\\"));
            var reportPath = $"{basePath}\\FileSystemDueRecords\\{jobId}\\{dbName}";
            return reportPath;
        }
    }
}
