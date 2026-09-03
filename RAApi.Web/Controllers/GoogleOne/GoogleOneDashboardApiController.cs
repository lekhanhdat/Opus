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
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Service.Services.Dashboard;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Dashboard;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/dashboard")]
    public class GoogleOneDashboardApiController : GoogleOneApiBaseController
    {
        [HttpGet("getmanagedrecordscount")]
        public async Task<List<DashboardKeyValue<long>>> GetManagedRecordsCount()
        {
            return await DashboardQuerier.GetManagedRecordsCountForGGOneAsync();
        }
        [HttpGet("getmanualapprovalstatus")]
        public async Task<List<DashboardKeyValue<int>>> GetManualApprovalStatus()
        {
            return await DashboardQuerier.GetManualApprovalStatusForGGOneAsync();
        }
        [HttpPost("getlinechartitems")]
        public List<DashboardLineChartItem> GetLineChartItems([FromBody] ChartDateRange dateRange)
        {
            return DashboardQuerier.GetLineChartItems(SourceFlag.GGControl, dateRange);
        }
        [HttpGet("gettop10termusages")]
        public async Task<List<RMDashboardTermUsage>> GetTop10TermUsages()
        {
            return await DashboardQuerier.GetTop10MostUsedTermsAsync(SourceFlag.GGControl, true);
        }
        [HttpGet("gettop10mostusedsites")]
        public async Task<List<RMDashboardDataUsage>> GetTop10MostUsedSites()
        {
            return await DashboardQuerier.GetTop10MostUsedSitesAsync(SourceFlag.GGControl, true);
        }

        [HttpGet("gettop10userrecordswaitingapproval")]
        public async Task<List<RMDashboardUserWaitingApprovalCount>> GetTop10UserRecordsWaitingApproval()
        {
            return DashboardQuerier.GetTop10UserRecordsWaitingApproval(SourceFlag.GGControl);
        }
        [HttpPost("getlastcollecttime")]
        public async Task<long> GetLastCollectTime()
        {
            return await Task.Run(() => DashboardQuerier.GetLastCollectTimeTick());
        }
    }
}
