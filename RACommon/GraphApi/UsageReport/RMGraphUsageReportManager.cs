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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using Cloud.Sdk.Data.AosModern;
using Duende.IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.UsageReport
{

    public enum RMGraphUsageReportPeriod
    {
        None = 0,
        Day7 = 1,
        Day30 = 2,
        Day90 = 3,
        Day180 = 4
    }

    public record RMGraphUsageReportInfo(DateTime ReportDate, long Size);

    public class RMGraphUsageReportManager : RMGraphApiManager
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMGraphUsageReportManager));

        private static readonly Dictionary<SourceFlag, string> REQUEST_URI =
            ImmutableDictionary.CreateRange([
                KeyValuePair.Create(SourceFlag.SharePoint, "reports/getSharePointSiteUsageStorage(period='{0}')"),
                KeyValuePair.Create(SourceFlag.OneDrive, "reports/getOneDriveUsageStorage(period='{0}')"),
            ]).ToDictionary();

        private static readonly Dictionary<RMGraphUsageReportPeriod, string> PERIOD =
            ImmutableDictionary.CreateRange([
                KeyValuePair.Create(RMGraphUsageReportPeriod.Day7, "D7"),
                KeyValuePair.Create(RMGraphUsageReportPeriod.Day30, "D30"),
                KeyValuePair.Create(RMGraphUsageReportPeriod.Day90, "D90"),
                KeyValuePair.Create(RMGraphUsageReportPeriod.Day180, "D180"),
            ]).ToDictionary();

        public RMGraphUsageReportManager(string o365TenantId) : base(o365TenantId) { }

        public RMGraphUsageReportManager(AppProfileInfo profile) : base(profile) { }

        public async Task<List<RMGraphUsageReportInfo>> GetUsageReportsAsync(SourceFlag contentSource, RMGraphUsageReportPeriod period)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/{string.Format(REQUEST_URI[contentSource], PERIOD[period])}";
            var reportsStr = await HttpHelper.GetAsync(requestUri, AccessToken);

            if(!PreCheck(reportsStr))
            {
                return [];
            }

            var res = new List<RMGraphUsageReportInfo>();

            var reports = reportsStr.Split("\r\n").Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
            var addedDate = new HashSet<string>();
            for(var i = 1; i < reports.Count; i++)
            {
                var report = reports[i];
                var reportInfo = report.Split(",");
                var size = 0L;
                if (long.TryParse(reportInfo[2], out var parsedSize))
                {
                    size = parsedSize;
                }
                var dateStr = reportInfo[3];
                if(addedDate.Contains(dateStr))
                {
                    continue;
                }

                addedDate.Add(dateStr);
                res.Add(new(DateTime.Parse(dateStr), size));
            }

            return res;
        }

        private bool PreCheck(string reportsStr)
        {
            if (string.IsNullOrWhiteSpace(reportsStr))
            {
                _logger.Warn($"The o365 [{Profile.TenantId}] no [OneDrive] usage report found.");
                return false;
            }

            var reports = reportsStr.Split("\r\n");
            if (reports.Length < 2)
            {
                _logger.Warn($"The o365 [{Profile.TenantId}] no latest [OneDrive] storage usage report available.");
                return false;
            }

            return true;
        }
    }
}
