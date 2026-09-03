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
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.AzureCosmosDB.Query.SQL;
using AvePoint.RA.DB.AzureCosmosDB.WriteMode;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB.Demo
{
    public class Demo
    {
        private RMAzureCosmosDBContainer Container => RMAzureCosmosDBContext.GetContainerAsync().GetAwaiter().GetResult();
        

        public async Task Test01()
        {
            var res = await Container.UseLinqQuery().Where(item => item.LeafName == "Demo Name")
                .AsResultSet().AllAsync().ToListAsync();

            var res2 = await Container.UseLinqQuery().Where(item => item.LeafName == "Demo Name")
                .AsResultSet().PaginateAsync(null, 15);

            var res3 = await Container.UseLinqQuery().Where(item => item.LeafName == "Demo Name")
                .OrderBy(item => item.TimeCreated).AsResultSet().FirstOrDefault();

            var res4 = await Container.UseLinqQuery().Where(item => item.LeafName == "Demo Name")
                .Select(item => item.LeafName).AsResultSet().TopAsync(10);

            var res5 = await Container.UseLinqQuery().Where(item => item.LeafName == "Demo Name")
                .OrderByDescending(item => item.TimeCreated)
                .Select(item => item.LeafName).AsResultSet().CountAsync();
        }

        public async Task Test02()
        {
            var res = await Container.UseSqlQuery().WithSql("SELECT * FROM c WHERE c.LeafName = 'Demo Name'").AllAsync<Record>().ToListAsync();

            var res2 = await Container.UseSqlQuery().WithSql("SELECT LeafName FROM c").FirstOrDefaultAsync<string>();

            var res3 = await Container.UseSqlQuery().WithSql("SELECT * FROM c WHERE c.LeafName = @leafName")
                .WithParameter(new Query.SQL.RMAzureCosmosDBQueryParameter("@leafName", "Demo Name"))
                .PaginateAsync<Record>(null, 15);

            var res4 = await Container.UseSqlQuery().WithSql("SELECT SourceFlag, Count(1) as Count FROM c GROUP BY SourceFlag")
                .ToDictionaryAsync<int, int>();
        }

        public async Task Test03()
        {
            RMAzureCosmosDBService CosmosService = new RMAzureCosmosDBService(Container);
           
            await CosmosService.UseLinqQuery().Where(item => item.LeafName == "Raven Le")
                .AsResultSet().AllAsync().ToListAsync();
            
            await CosmosService.UseSqlQuery()
                .WithSql("SELECT * FROM c WHERE c.leafName == @leafName")
                .WithParameter(new RMAzureCosmosDBQueryParameter("@leafName", "Raven Le"))
                .PaginateAsync<Record>(null, 15);

            #region Immediately
            CosmosService.ConfigureImmediatelyAction();
            await CosmosService.ImmediatelyAddAsync(new Record { });
            await CosmosService.ImmediatelyDeleteAsync(new Record { });
            #endregion

            #region Delay
            CosmosService.ConfigureDelayAction(CallbackFunction);
            CosmosService.DelayAdd(new List<Record> { });
            CosmosService.CompleteAddingDelayQueue();
            await CosmosService.WaitDelayQueueCompletedAsync();
            #endregion

            async Task CallbackFunction(RMAzureCosmosDBDelayConcurrentActionResult actionResult)
            {
                await Task.CompletedTask;
            }
        }
    }
}
