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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Http;
using RAManualApproval.Upgrade;
using System.Text.RegularExpressions;

namespace RecoUnitTest
{
    [TestClass]
    public class HistoryMigration
    {
        [TestMethod]
        public void TestMigrationJob()
        {
            //SetTenantInfo();
            //var dbSet = RMHistoryStorageAzureTableContext.ManualApproveHistories;
            //try
            //{
            //    dbSet.AddRange(new List<RMManualApproveHistoryTableEntity>
            //    {
            //        new AvePoint.RA.DB.AzureTable.Model.RMManualApproveHistoryTableEntity
            //        {
            //            PartitionKey = "202210",
            //            RowKey = "6379795213984810231",
            //        },
            //        new AvePoint.RA.DB.AzureTable.Model.RMManualApproveHistoryTableEntity
            //        {
            //            PartitionKey = "202210",
            //            RowKey = "637979521398481022100",
            //        },
            //        //new AvePoint.RA.DB.AzureTable.Model.RMManualApproveHistoryTableEntity
            //        //{
            //        //    PartitionKey = "202209",
            //        //    RowKey = "2517399454601498883",
            //        //},
            //    }).GetAwaiter().GetResult();
            //}
            //catch (Azure.RequestFailedException e)
            //{
            //    if (e.Status == StatusCodes.Status409Conflict)
            //    {

            //    }
            //}
        }

        [TestMethod]
        public void Test2()
        {
            //    SetTenantInfo();
            //    var dbSet = RMHistoryStorageAzureTableContext.ManualApproveHistories;
            //    var values = (dbSet.QueryWithPagination(item => item.PartitionKey == "202210", 8, null)).GetAwaiter().GetResult().Values.ToList();
            var formaTime = string.Format("{0} {1}", "abc", DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == "Fiji Standard Time").FirstOrDefault()?.DisplayName/*tiz.DisplayName*/);
            
        }

        private static void SetTenantInfo()
        {
            TenantLocalValue.LogonGroupId = "35226de4-9d1c-44df-8dd9-2b419109e93c";
            TenantLocalValue.LogonUserId = "acca1285-cdd0-45f3-9746-0550c8284c14";
            TenantLocalValue.LogonUserEmail = "lambert.shen@avepoint.com";
        }
    }
}