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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class DashboardDataUsageOfDateDao : BaseDao<RMDashboardDataUsageOfDate>, IDashboardDataUsageOfDateDao
    {

        private static ManualApprovalRecordRepository Repository => new ManualApprovalRecordRepository();

        public Task RemoveAllAsync(SourceFlag sourceFlag)
        {
            return RemoveAllAsync((int)sourceFlag);
        }

        public Task RemoveAllAsync(int sourceFlag)
        {
            return BatchDeleteAsync(item => item.SourceFlag == sourceFlag);
        }

        public IEnumerable<IEnumerable<long>> GetNearlyYearWaitingApprovalData(SourceFlag sourceFlag, int limit = 5000)
        {
            var yearAgo = DateTime.UtcNow.AddYears(-1);
            var startDateTicks = new DateTime(yearAgo.Year, yearAgo.Month, yearAgo.Day).Ticks;

            var repository = Repository;

            string continuation = null;
            do
            {
                var (Result, Continuation) = repository.QueryItemsWithPaginationAsync(
                    item => item.IsManualSynced &&
                    item.SourceFlag == (int)sourceFlag &&
                    item.ManualCollectionTime >= startDateTicks,
                    item => item.ManualCollectionTime,
                    continuation,
                    limit
                ).GetAwaiter().GetResult();

                continuation = Continuation;
                yield return Result.ToList();
            } while (!string.IsNullOrEmpty(continuation));
        }
    }
}
