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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Report
{
    public class RMDiscoveryFSJobReportManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSJobReportManager));

        private readonly IRMDiscoveryFSJobDao _jobDao = new RMDiscoveryFSJobDao();

        private readonly Guid _mainJobId;

        private const string REPORT_FILE_NAME = "DiscoveryFileSystemJobReport.csv";

        private readonly Dictionary<RMDiscoveryJobStatus, string> _jobStatusI18ns = new()
        {
            {RMDiscoveryJobStatus.Finished, "RM_JS_JMD_Status_Successful" },
            {RMDiscoveryJobStatus.Failed, "RM_JS_JMD_Status_Failed" },
            {RMDiscoveryJobStatus.Exception, "RM_JS_JMD_Status_Exception" },
        };

        public RMDiscoveryFSJobReportManager(Guid mainJobId)
        {
            _mainJobId = mainJobId;
        }

        public async Task<string> GenerateReportAsync()
        {
            try
            {
                var downloadPath = Path.Combine(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], JobReportUtility.GetTenantIdentity(), REPORT_FILE_NAME);
                if (!Directory.Exists(Path.GetDirectoryName(downloadPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                }

                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                }

                _logger.Info($"Discovery file system generate report path: [{downloadPath}].");
                using (var fs = new FileStream(downloadPath, FileMode.Create))
                {
                    using var writer = new StreamWriter(fs, Encoding.UTF8);
                    writer.WriteLine(string.Join(",", ["Connection Name", "Status"]));
                    var jobDetails = _jobDao.GetAnalysisJobReportWithPaginationAsync(_mainJobId, 1000);
                    await foreach (var jobDetail in jobDetails)
                    {
                        var safeConnectionName = $"=\"{jobDetail.ConnectionName.Replace("\"", "\"\"")}\"";
                        writer.WriteLine(string.Join(",", [safeConnectionName, I18NEntity.GetString(_jobStatusI18ns[jobDetail.Status])]));
                    }
                }
                return downloadPath;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while generate file system discovery job report. Error: {e}");
                return null;
            }
        }
    }
}
