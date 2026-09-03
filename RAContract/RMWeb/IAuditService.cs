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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    [RACodeReview("Allen yin")]
    public interface IAuditService
    {
        bool AddAudits(List<RMAuditInfo> auditInfos);
        List<RMAuditInfo> FindAuditInfoByTimeInterval(int pageIndex, int pageSize, ref int dataCount, DateTime startTime, DateTime endTime, DisplayColumn columnName, string columnValue);
        Task<Dictionary<DateTime, int>> FindAuditInfoByTimeIntervalAndGroupByTimeAsync(DateTime startTime, DateTime endTime);
        Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByUser(DateTime startTime, DateTime endTime);
        Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByRole(DateTime startTime, DateTime endTime);
        Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByModule(DateTime startTime, DateTime endTime);
        Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByObject(DateTime startTime, DateTime endTime);
        Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByAction(DateTime startTime, DateTime endTime);
        Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByStatus(DateTime startTime, DateTime endTime);
        Task<RAReturnMessage> GenerateReportForAuditReportAsync(string folderPath, string fileName, DateTime start, DateTime end);
        List<RMAuditInfo> FindAuditInfoBySortFilter(int pageIndex, int pageSize, ref int dataCount, DateTime startTime, DateTime endTime, bool? IsAscending, DisplayColumn SortBy, Dictionary<int, List<dynamic>> filterInfos, DisplayColumn viewBy, string ViewByValue);
        Dictionary<int, string> GetActionItemsSource();
        Dictionary<int, string> GetModuleItemsSource();
        Dictionary<int, string> GetStatusItemsSource();
        List<string> GetUserItemsSource();
        Task<int> DeleteAuditorBeforeTimeAsync(long ticks);
    }
}
