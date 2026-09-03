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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IMyhubReportJobDao : IBaseDao<RMMyhubReportJob>
    {
        List<RMMyhubReportJob> GetAllReportJobByUserName(string userName);
        List<RMMyhubReportJob> GetJobByScopeId(List<string> scopeIds, string userId);
        Task UpdateStatusByJobId(string jobId, MyhubReportJobStatus status);
        Task DeleteReportJobByJobId(string jobId);
        Task CreateJobReports(RMDownloadDataInfo downloadInfo);
        List<RMMyhubReportJob> GetAllMyhubReportJobByJobType(int jobType, int pageIndex, int pageSize, out int totalRecord);
        List<RMMyhubReportJob> GetMyhubReportByScopeIds(List<string> scopeIds, int jobType, int downloadType, int pageIndex, int pageSize, out int totalRecord);
    }
}