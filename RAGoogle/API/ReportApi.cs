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
using Google.Apis.Admin.Reports.reports_v1;
using Google.Apis.Admin.Reports.reports_v1.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using RAGoogle.API.Query;
using RAGoogle.Extension;
using static Google.Apis.Admin.Reports.reports_v1.ActivitiesResource.ListRequest;

namespace RAGoogle.API
{
    internal class ReportApi : IDisposable
    {
        private ReportsService _service;
        private string ActivityDateTimeFormate = "yyyy-MM-ddTHH:mm:ssZ";
        private string UsageDateTimeFormate = "yyyy-MM-dd";
        internal ReportApi(BaseClientService.Initializer initializer)
        {
            _service = new ReportsService(initializer);
            _service.HttpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// only test code
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        internal async Task<Activities> GetDriveActivity(ActivityQuery query)
        {
            query.ThrowIfNull("Activity Query Param is null");
            ActivitiesResource.ListRequest request = _service.Activities.List(query.UserKey, ApplicationNameEnum.Drive);
            request.StartTime = query.StartTime.ToString(ActivityDateTimeFormate);
            request.EndTime = query.EndTime.ToString(ActivityDateTimeFormate);
            request.PageToken = query.PageToken;
            request.Fields = query.Fields;
            request.Filters = query.Filters;
            request.MaxResults = query.PageSize > 0 ? query.PageSize : 100;
            return await request.ExecuteExAsync();
        }

        internal async Task<UsageReports> GetCustomerDriveReportUsage(DateTime datetime)
        {
            var request = _service.CustomerUsageReports.Get(datetime.ToString(UsageDateTimeFormate));
            request.Parameters = "accounts:drive_used_quota_in_mb,accounts:shared_drive_used_quota_in_mb";
            return await request.ExecuteExAsync();
        }

        internal async Task<(Activities activities, string pageToken)> GetPagedDriveActivity(ActivityQuery query)
        {
            query.ThrowIfNull("Activity Query Param is null");
            ActivitiesResource.ListRequest request = _service.Activities.List(query.UserKey, ApplicationNameEnum.Drive);
            request.StartTime = query.StartTime.ToString(ActivityDateTimeFormate);
            request.EndTime = query.EndTime.ToString(ActivityDateTimeFormate);
            request.PageToken = query.PageToken;
            request.Fields = query.Fields ?? "*";
            request.Filters = query.Filters;
            request.MaxResults = query.PageSize > 0 ? query.PageSize : 1000;
            var activities = await request.ExecuteExAsync();
            query.PageToken = activities.NextPageToken;
            return (activities, query.PageToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _service?.Dispose();
            _service = null;
        }

        ~ReportApi()
        {
            Dispose(false);
        }
    }
}
