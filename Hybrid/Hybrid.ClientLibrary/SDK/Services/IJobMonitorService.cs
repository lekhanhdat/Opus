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
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK.Services
{
    public interface IJobMonitorService
    {
        [Api(Url = "api/jobmonitor/UpdateJobProgress", HttpMethod = "POST")]
        Task<bool> UpdateJobProcess(HBJobStatusInfo hBJobStatusInfo);

        //[Api(Url = "api/jobmonitor/UpdateJobReport", HttpMethod = "GET")]
        //Task<bool> UpdateJobReport(string nodeRequestInfo);

        [Api(Url = "api/jobmonitor/UpdateJobState", HttpMethod = "POST")]
        Task<bool> UpdateJobState(HBJobStatusInfo hBJobStatusInfo);

        [Api(Url = "api/jobmonitor/SendReport", HttpMethod = "POST")]
        Task<bool> SendReport(HBReportInfo hBReportInfo);
        [Api(Url = "api/jobmonitor/DeleteJobForAgentById", HttpMethod = "POST")]
        Task DeleteJobForAgentById(string jobId);
        [Api(Url = "api/jobmonitor/GetJobState", HttpMethod = "GET")]
        Task<int> GetJobState(string jobId);
    }
}
