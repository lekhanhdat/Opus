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
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    
    public interface IAuditDao : IBaseDao<RMAudit>
    {
        List<RMAudit> FindAuditInfoByTimeInterval(int pageIndex, int pageSize, ref int dataCount, Expression<Func<RMAudit, bool>> whereLamdba);
        List<long> FindAuditInfoByTimeIntervalAndGroupByTime(DateTime startTime, DateTime endTime);
        List<RMAudit> FindAllAuditInfos();
        Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByUser(DateTime startTime, DateTime endTime);
        Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByRole(DateTime startTime, DateTime endTime);
        Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByModule(DateTime startTime, DateTime endTime);
        Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByObject(DateTime startTime, DateTime endTime);
        Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByAction(DateTime startTime, DateTime endTime);
        Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByStatus(DateTime startTime, DateTime endTime);
        List<RMAudit> FindAuditInfoByFilterAndSort(int pageIndex, int pageSize, ref int dataCount, Expression<Func<RMAudit, bool>> whereLamdba, DisplayColumn orderColumn, bool? IsAscending);
        List<AuditAction> GetAuditActionFromDB();
        List<AuditCategory> GetAuditModuleFromDB();
        List<string> GetAuditUserFromDb();

        Task Add(RMAudit audit);
    }
}
