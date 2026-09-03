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
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Report
{
    public class RMDiscoverySalesforceJobReportManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceJobReportManager));

        private readonly Dictionary<RMDiscoveryJobStatus, string> _jobStatusI18ns = new() 
        {
            {RMDiscoveryJobStatus.Finished, "RM_JS_JMD_Status_Successful" },
            {RMDiscoveryJobStatus.Failed, "RM_JS_JMD_Status_Failed" },
            {RMDiscoveryJobStatus.Exception, "RM_JS_JMD_Status_Exception" },
        };

        private readonly IRMDiscoverySalesforceDataQueryDao _dataQueryDao = new RMDiscoverySalesforceDataQueryDao();


        private readonly string _fileName = $"DiscoveryJobReport.csv";

        public async Task<string> GenerateReportAsync(RMDiscoverySalesforceMainJob jobInfo)
        {
            try
            {
                var downloadPath = Path.Combine(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], JobReportUtility.GetTenantIdentity(), _fileName);
                if (!Directory.Exists(Path.GetDirectoryName(downloadPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                }

                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                }

                _logger.Info($"Salesforce discovery generate report path: [{downloadPath}].");
                await using var fs = new FileStream(downloadPath, FileMode.Create);
                await using var writer = new StreamWriter(fs, Encoding.UTF8);
                if(jobInfo.Status == RMDiscoveryJobStatus.Failed || jobInfo.Status == RMDiscoveryJobStatus.Exception)
                {
                    writer.WriteLine(I18NEntity.GetString("RM_SF_LimitAPI"));
                }
                else
                {
                    writer.WriteLine(string.Join(",", ["Object", "Status"]));
                    var objectInfors = await _dataQueryDao.GetAllObjectInfor();
                    foreach (var objectInfor in objectInfors)
                    {
                        var escapedDisplayName = $"\"{objectInfor.DisplayName?.Replace("\"", "\"\"")}\"";
                        writer.WriteLine(string.Join(",", [escapedDisplayName, I18NEntity.GetString(_jobStatusI18ns[jobInfo.Status])]));
                    }

                }

                return downloadPath;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while generate salesforce discovery job report. Error: {e}");
                return null;
            }
        }
    }
}
