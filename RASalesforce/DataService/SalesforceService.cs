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
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Salesforce;
using RASalesforce.APIs;
using RASalesforce.DataObject;

namespace RASalesforce
{
    public class SalesforceService
    {
        SalesforceAPIClient client;

        public string CustomerId { get; set; }
        public string OrganizationId { get; set; }

        public SalesforceService(string customerId, string organizationId)
        {
            CustomerId = customerId;
            OrganizationId = organizationId;
        }

        public SalesforceService Build()
        {
            client = new SalesforceAPIClientFactory().CreateAPIClient(CustomerId, OrganizationId).GetAwaiter().GetResult();
            return this;
        }
        
        public Task RefreshAsync()
        {
           return client.RefreshLimit();
        }

        public async IAsyncEnumerable<string> GetObjectAsync()
        {
            var sobjs = await client.SoapService.DescribeSObjectsAsync(["ContentVersion", "Attachment", "Document"]);

            foreach (var obj in sobjs)
            {
                yield return $"{obj}";
            }
        }

        public async Task<OrganizationLimits?> GetOrganizationLimitsAsync()
        {
            return await client.RestService.GetOrganizationLimitsAsync();
        }
        
        public async Task<List<SFObjectProxy>> GetSalesforceObjectsAsync()
        {
            var globalObjects = await client.SoapService.DescribeGlobalAsync();
            return globalObjects.sobjects.ConvertAll(sObject => new SFObjectProxy(sObject)).ToList();
        }

        public async Task<Organization?> GetOrganizationAsync()
        {
            return await client.SoapService.GetOrganizationAsync();
        }

        public async Task<List<SFObjectProxy>> GetDetailObjectsAsync(List<string> objectNames)
        {
            var sObjects = await client.SoapService.DescribeSObjectsAsync(objectNames);
            return sObjects.ConvertAll(sObject => new SFObjectProxy(sObject)).ToList();
        }

        public async Task<RecordCount?> GetRecordCountAsync(List<string> objectNames)
        {
            return await client.RestService.GetRecordsCountByObjectsAsync(objectNames);
        }

        public async Task<DateTime> GetOldestRecordAsync(DescribeSObjectResult sObject)
        {
            var data = await client.SoapService.GetRecordsAsync(sObject, new RecordQuery()
            {
                Fields = new List<string> { "CreatedDate" },
                OrderBy = new QueryOrder()
                {
                    OrderByKeyword = "CreatedDate",
                },
                Limit = 1
            });
                
            return DateTime.Parse(data[0]["CreatedDate"]);
        }
        public async Task<DateTime> GetLastModifiedTimeAsync(DescribeSObjectResult sObject)
        {
            var data = await client.SoapService.GetRecordsAsync(sObject, new RecordQuery()
            {
                Fields = new List<string> { "LastModifiedDate" },
                OrderBy = new QueryOrder()
                {
                    OrderByKeyword = "LastModifiedDate",
                    OrderByDesc = true
                },
                Limit = 1
            });

            return DateTime.Parse(data[0]["LastModifiedDate"]);
        }

        public async Task<(long, long)> GetFileRecordDataAsync(DescribeSObjectResult sObject, RecordQuery query)
        {
            var result = await client.SoapService.GetFileCountAsync(sObject, query);
            return (result.Count, (long)result.TotalSize);
        }
        
        public async Task<long> GetDataRecordDataAsync(DescribeSObjectResult sObject, RecordQuery query)
        {
            return await client.SoapService.GetRecordCountAsync(sObject, query);
        }

        public async Task<List<SFFileProxy>> GetAttachmentsAsync(DescribeSObjectResult sObject, RecordQuery query)
        {
            var records = await client.SoapService.GetFileRecordsAsync(sObject, query);
                
            return records.Select(record => new SFFileProxy(record)).ToList();
        }

        public async Task<SFStorageLimitProxy> GetStorageLimitProxyAsync()
        {
            var organizationLimitApi = await client.RestService.GetOrganizationLimitsAsync();
            return new SFStorageLimitProxy(organizationLimitApi!);
        }

        public async Task<int?> GetRecordCountWithModifiedTimeAsync(DescribeSObjectResult sObject, RecordQuery query)
        {
            var recordCount = await client.SoapService.GetRecordCountAsync(sObject, query);
            return recordCount;
        }
        public async Task<IEnumerable<FileExtensionResult>> GetFileDataWithModifiedTimeAndSizeRangeAsync(DescribeSObjectResult sObject, RecordQuery query)
        {
            var fileExtensionResults = await client.SoapService.GetFileExtensionSizeAsync(sObject, query);
            return fileExtensionResults;
        }
    }
}
