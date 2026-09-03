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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureTable.Demo
{
    public class RMAzureTableDemo
    {
        public async Task Test1()
        {
            await RMRecordStorageAzureTableContext.ManualApproveHistories.Add(new Model.RMManualApproveHistoryTableEntity { });
        }

        public async Task Test2()
        {
            await RMRecordStorageAzureTableContext.ManualApproveHistories.Delete("partitionKey", "rowKey");
        }

        public async Task Test3()
        {
            var res = await RMRecordStorageAzureTableContext.ManualApproveHistories.Query().ToListAsync();
        }

        public async Task Test4()
        {
            var asyncEnumerable = RMRecordStorageAzureTableContext.ManualApproveHistories.Query();
            await foreach(var entity in asyncEnumerable)
            {

            }
        }

        public async Task Test5()
        {
            var pageRes = await RMRecordStorageAzureTableContext.ManualApproveHistories.QueryWithPagination(10, null);
            var values = pageRes.Values;
            var token = pageRes.ContinuatioinToken;
            var pageRes2 = await RMRecordStorageAzureTableContext.ManualApproveHistories.QueryWithPagination(10, token);
        }

        public async Task Test6()
        {
            var effectItemCount = await RMRecordStorageAzureTableContext.ManualApproveHistories
                .Delete(item => item.LeafName == "zhangsan");
        }
    }
}
