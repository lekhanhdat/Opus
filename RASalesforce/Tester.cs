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
//test code
using System.Linq;

namespace RASalesforce.APIs;


public class Tester
{
    public static async Task GetAccountAsync()
    {
        var client = await new SalesforceAPIClientFactory().CreateAPIClient("customerid", "organizationid");
        //var countSql = "SELECT FileExtension, count(Id) count1, Sum(ContentSize) sum1 FROM ContentVersion Where lastModifiedDate > 2024-10-23T07:57:08.4887498+00:00 Group BY FileExtension";
        //var countSql = "SELECT count(id) count1,Sum(ContentSize) sum1 FROM ContentVersion";
        //var test = await client.SoapService.QueryAsync(countSql);
        //var sql = "select  FIELDS(ALL) from Attachment Limit 200";
        ////var test1 = await client.SoapService.QueryAsync(sql);
        ////var de
        ////var allitems = await client.SoapService.GetRecordsAsync<SforceService.Attachment>();
        var objs = await client.SoapService.DescribeGlobalAsync();
        var sobjs = await client.SoapService.DescribeSObjectsAsync(["EmailMessage"]);
        var limit = await client.RestService.GetRecordsCountAsync();
        limit.SObjects = limit.SObjects.Where(o=> objs.sobjects.Any(c=>c.name.EqualsIgnoreCase(o.Name) && c.createable && c.updateable)).ToList();
        var limit1 = await client.RestService.GetOrganizationLimitsAsync();
        //var result = await client.SoapService.GetFileCountAsync(sobjs.First(), new Query() 
        //{
        //    Filters = new List<QueryFilter> { new QueryFilter() 
        //    {
        //        PropertyName = "LastModifiedDate",
        //        Value = new List<string>()
        //        {
        //            DateTime.UtcNow.Ticks.ToString(),
        //            DateTime.UtcNow.AddDays(-8).Ticks.ToString(),
        //        }
        //    } }
        //});
        //var ex = await client.SoapService.GetFileExtensionSizeAsync(sobjs.First(), new Query()
        //{
        //    Filters = new List<QueryFilter> { new QueryFilter()
        //    {
        //        PropertyName = "LastModifiedDate",
        //        Value = new List<string>()
        //        {
        //            DateTime.UtcNow.Ticks.ToString(),
        //            DateTime.UtcNow.AddDays(-8).Ticks.ToString(),
        //        }
        //    } }
        ////});
        //var dataCount = await client.SoapService.GetRecordCountAsync(sobjs.Last(), new Query()
        //{
        //    Filters = new List<QueryFilter> { new QueryFilter()
        //    {
        //        PropertyName = "LastModifiedDate",
        //        Value = new List<string>()
        //        {
        //            DateTime.UtcNow.AddDays(-30).Ticks.ToString(),
        //        }
        //    } }
        //});
        var data = await client.SoapService.GetRecordsAsync(sobjs.Last(), new RecordQuery()
        {
            Fields = new List<string> {"Id", "TextBody" },
            //Filters = new List<QueryFilter> { new QueryFilter()
            //{
            //    PropertyName = "CreatedDate",
            //    Value = new List<string>()
            //    {
            //        DateTime.UtcNow.AddDays(-30).Ticks.ToString(),
            //    }
            //}
            //},
            //OrderBy = new QueryOrder() 
            //{
            //    OrderByKeyword = "CreatedDate",
            //},
            //Limit = 1
        });
        var org = await client.SoapService.GetOrganizationAsync();
        //foreach (var item in sobjs[0].fields)
        //{
        //    Console.WriteLine(item.name);
        //}
        //var count = await client.SoapService.GetRecordCountByTypeAsync(sobjs[0], customizeFilter: "lastModifiedDate > 2024-10-23T07:57:08.4887498+00:00");
    }
}

