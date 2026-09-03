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
namespace RASalesforce.APIs;

public class SalesforceSoapAPIService : SalesforceAPIService
{
    private static readonly ILogger Logger = LoggerFactory.Get(typeof(SalesforceSoapAPIService));
    public static readonly string ConnectedApplicationQuery = "SELECT DeveloperName,CreatedById,CreatedDate,Id,LastModifiedById,LastModifiedDate,MobileSessionTimeout,MobileStartUrl,Name,NamedUserUvidTimeout,OptionsAllowAdminApprovedUsersOnly,OptionsAppIssueJwtTokenEnabled,OptionsCodeCredentialGuestEnabled,OptionsFullContentPushNotifications,OptionsHasSessionLevelPolicy,OptionsIsInternal,OptionsRefreshTokenValidityMetric,PinLength,RefreshTokenValidityPeriod,StartUrl,SystemModstamp,UvidTimeout FROM ConnectedApplication";
    private SoapClient soapClient;
    private IReadOnlyList<string> SupportedFileType = ["Attachment", "Document", "ContentVersion"];
    private IReadOnlyList<string> SupportedFileExtensionType = ["Document", "ContentVersion"];
    public SalesforceSoapAPIService(SalesforceAPIProvider provider) : base(provider)
    {
        soapClient = new();
    }

    internal override void InitClientSetting(SFToken token)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10 * 60 * 60);
        soapClient.Endpoint.Binding.SendTimeout = timeout;
        soapClient.Endpoint.Binding.ReceiveTimeout = timeout;
        soapClient.Endpoint.Address = new System.ServiceModel.EndpointAddress($"{token.InstanceUrl}/services/Soap/u/61.0/{token.OrgShortId}");
    }
    /// <summary>
    /// To retrieve global sobject description
    /// </summary>
    /// <returns>list of global sobject description, but no field details</returns>
    public async Task<DescribeGlobalResult> DescribeGlobalAsync()
    {
        describeGlobalRequest request = new describeGlobalRequest();
        request.SessionHeader = new SforceService.SessionHeader();
        describeGlobalResponse response = await InvokerAsync(async () =>
        {
            var token = await GetTokenAsync();
            request.SessionHeader.sessionId = token.AccessToken;
            return await soapClient.describeGlobalAsync(request);
        }, 5, 2000);
        if (response?.result != null)
        {
            Logger.Debug($"API: Global schema retrieved.");
            return response.result;
        }
        else
        {
            Logger.Warn($"API: Failed to retireve global schema.");
        }
        return null;
    }

    /// <summary>
    /// To retrieve sobject description by object names
    /// </summary>
    /// <returns>list of sObject description, include field detail, can be use with query</returns>
    public async Task<DescribeSObjectResult[]> DescribeSObjectsAsync(List<string> names)
    {
        Logger.Debug($"API: Executing query");
        List<DescribeSObjectResult> result = new();
        var batchCount = 100;
        var currentCount = 100;
        var index = 0;
        while(currentCount >= batchCount)
        {
            var batchNames = names.Skip(index * batchCount).Take(batchCount);
            describeSObjectsResponse apiResult = await InvokerAsync(async () => await soapClient.describeSObjectsAsync(await GetDescribeSObjectsRequest(batchNames)), 3, 1000);//To do test 
            currentCount = batchNames.Count();
            if (apiResult is not null)
            {
                result.AddRange(apiResult.result);
            }
            index++;
        }

        //describeSObjectsResponse apiResult = await InvokerAsync(async () => await soapClient.describeSObjectsAsync(await GetDescribeSObjectsRequest(names)), 3, 1000);//To do test 
        Logger.Debug($"API: Query complete.");     
        return result.ToArray();
    }

    public async Task<int> GetRecordCountAsync(DescribeSObjectResult recordType, Query query)
    {
        query.Filters = GenerateDeletedFilter(recordType, query.Filters, query.IncludeDeleted);
        var queryClause = SOQLQueryUtil.BuildQuerySOQLClause(recordType, query);
        var queryStr = $"SELECT count() FROM {recordType.name} {queryClause.Where}";
        var dataResult = await QueryAsync(queryStr);
        ArgumentNullException.ThrowIfNull(dataResult,"query result");
        return dataResult.size;
    }

    public async Task<IEnumerable<FileExtensionResult>> GetFileExtensionSizeAsync(DescribeSObjectResult recordType, Query query)
    {
        List<FileExtensionResult> result = new();
        if (!SupportedFileExtensionType.Contains(recordType.name, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{nameof(recordType)} is not a file extension query type object");
        }
        query.Filters = GenerateDeletedFilter(recordType, query.Filters, query.IncludeDeleted);
        var queryClause = SOQLQueryUtil.BuildQuerySOQLClause(recordType, query);
        var sizeColumn = recordType.name.EqualsIgnoreCase("ContentVersion") ? "ContentSize" : "BodyLength";
        var fileTypeColumn = recordType.name.EqualsIgnoreCase("ContentVersion") ? "FileExtension" : "Type";
        var queryStr = $"SELECT count(Id) count1,{fileTypeColumn}, sum({sizeColumn}) sum1 FROM {recordType.name} {queryClause.Where} GROUP BY {fileTypeColumn}";
        var dataResult = await QueryAllDataAsync(queryStr);
        dataResult.ForEach(da =>
        {
            result.Add(new FileExtensionResult()
            {
                Extension = da[fileTypeColumn],
                Count = Convert.ToInt32(da["count1"]),
                TotalSize = Convert.ToDouble(da["sum1"])
            });
        });
        return result;
    }
    
    public async Task<List<Dictionary<string, string>>> GetFileRecordsAsync(DescribeSObjectResult recordType, RecordQuery query)
    {
        query.Filters = GenerateDeletedFilter(recordType, query.Filters, query.IncludeDeleted);
        var queryClause = SOQLQueryUtil.BuildQuerySOQLClause(recordType, query);
        var queryStr = $"SELECT Name, BodyLength, CreatedDate, LastModifiedDate  FROM {recordType.name} {queryClause.Where} {queryClause.Orderby}";
        if (query.Limit > 0) 
        {
            ArgumentNullException.ThrowIfNull(query.OrderBy, "Limit should be used with order by together");
            queryStr = $"{queryStr} Limit {query.Limit}";
        }
        return await QueryAllDataAsync(queryStr, 1000);
    }

    public async Task<FileCountResult> GetFileCountAsync(DescribeSObjectResult recordType, Query query)
    {
        FileCountResult result = new();
        if (!SupportedFileType.Contains(recordType.name, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{nameof(recordType)} is not a file type object");
        }
        query.Filters = GenerateDeletedFilter(recordType, query.Filters, query.IncludeDeleted);
        var queryClause = SOQLQueryUtil.BuildQuerySOQLClause(recordType, query);
        var sizeColumn = recordType.name.EqualsIgnoreCase("ContentVersion") ? "ContentSize" : "BodyLength";
        var queryStr = $"SELECT count(Id) count1, sum({sizeColumn}) sum1 FROM {recordType.name} {queryClause.Where}";
        var dataResult = await QueryAllDataAsync(queryStr);
        if (dataResult.Any())
        {
            var data = dataResult.First();
            var resultCount = int.TryParse(data["count1"], out var count) ? count : 0;
            var resultTotalSize = double.TryParse(data["sum1"], out var totalSize) ? totalSize : 0;
            result.Count = resultCount;
            result.TotalSize = resultTotalSize;
        }
        return result;
    }

    public async Task<List<Dictionary<string, string>>> GetRecordsAsync(DescribeSObjectResult recordType, RecordQuery query)
    {
        query.Filters = GenerateDeletedFilter(recordType, query.Filters, query.IncludeDeleted);
        IEnumerable<string> fields = !query.Fields.Any() ? ["Id"] :
            query.Fields.Where(fc => recordType.fields.Any(f => f.name.EqualsIgnoreCase(fc))).ToList();
        var queryClause = SOQLQueryUtil.BuildQuerySOQLClause(recordType, query);
        var queryStr = $"SELECT {string.Join(',', fields)} FROM {recordType.name} {queryClause.Where} {queryClause.Orderby}";
        if (query.Limit > 0) 
        {
            ArgumentNullException.ThrowIfNull(query.OrderBy, "Limit should be used with order by together");
            queryStr = $"{queryStr} Limit {query.Limit}";
        }
        return await QueryAllDataAsync(queryStr, 1000);
    }

    public async Task<Organization> GetOrganizationAsync()
    {
        var query = "select Name from Organization";
        var data = await QueryAllDataAsync(query);
        ArgumentNullException.ThrowIfNull(data, "query organization result");
        var dataResult = data.First();
        var result = new Organization
        {
            Name = dataResult["Name"]
        };
        return result;
    }


    //public async Task<int> GetRecordCountByTypeAsync(DescribeSObjectResult recordType, string? customizeFilter = null, bool throwException = false)
    //{
    //    int recordCount = -1;
    //    string query = $"SELECT count() FROM {recordType.name}";
    //    if (recordType.fields.Any(x => x.name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase)))
    //    {
    //        query = $"SELECT count() FROM {recordType.name} WHERE IsDeleted=false";
    //    }
    //    if (customizeFilter is not null)
    //    {
    //        query = $"{query} AND {customizeFilter}";
    //    }
    //    Logger.Info($"start to GetRecordCount for {recordType.name}");
    //    QueryResult qr = await QueryAsync(query);
    //    if (qr == null)
    //    {
    //        if (throwException)
    //        {
    //            throw new Exception($"API: Error retrieving record count for object {recordType.name}.");
    //        }
    //    }
    //    else
    //    {
    //        recordCount = qr.size;
    //    }
    //    return recordCount;
    //}


    private async Task<QueryResult> QueryAsync(string query, int pageSize = 1000)
    {
        Logger.Debug($"API: Executing query");

        queryResponse apiResult = await InvokerAsync(async () => await soapClient.queryAsync(await GetQueryRequest(query, pageSize)), 5, 1000);//To do test 
        Logger.Debug($"API: Query complete.");
        if (apiResult?.result != null)
        {
            return apiResult.result;
        }
        else
        {
            return null;
        }
    }



    private async Task<List<Dictionary<string, string>>> QueryAllDataAsync(string query, int pageSize = 1000)
    {
        List<Dictionary<string, string>> result = [];
        try
        {
            var header = new SforceService.SessionHeader();
            var callOptions = new SforceService.CallOptions();
            var queryOptions = new QueryOptions() { batchSize = pageSize, batchSizeSpecified = true };
            queryAllResponse response = await InvokerAsync(async () =>
            {
                var token = await GetTokenAsync();
                header.sessionId = token.AccessToken;
                return await soapClient.queryAllAsync(
                    header, callOptions, queryOptions, query
                    );
            }, 5, 2000);

            if (response?.result is null)
            {
                return result;
            }
            // add to result
            response.result.records?.ForEach(record =>
            {
                Dictionary<string, string> recordDict = [];
                record.Any.ForEach(field => { recordDict.Add(field.LocalName, field.InnerText); });
                result.Add(recordDict);
            });

            bool more = !response.result.done;
            var queryLocator = response.result.queryLocator;
            while (more && queryLocator.IsNotNullOrEmpty())
            {
                var moreResponse = await InvokerAsync(async () =>
                {
                    var queryMoreRequest = await GetQueryMoreRequest(response.result.queryLocator, 10);
                    return await soapClient.queryMoreAsync(queryMoreRequest);
                }, 5, 2000);
                if (moreResponse?.result is null)
                {
                    break;
                }
                // add to result
                moreResponse.result.records.ForEach(record =>
                {
                    Dictionary<string, string> recordDict = [];
                    record.Any.ForEach(field => { recordDict.Add(field.LocalName, field.InnerText); });
                    result.Add(recordDict);
                });
                more = !moreResponse.result.done;
                queryLocator = moreResponse.result.queryLocator;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Query all from SF failed, Query:{query}, Message:{ex}.");
        }
        return result;
    }

    private async Task<queryRequest> GetQueryRequest(string query, int pageSize)
    {
        var token = await GetTokenAsync();
        queryRequest request = new queryRequest();
        request.SessionHeader = new SforceService.SessionHeader();
        request.SessionHeader.sessionId = token.AccessToken;
        request.QueryOptions = new QueryOptions();
        request.QueryOptions.batchSizeSpecified = true;
        request.QueryOptions.batchSize = pageSize;
        request.queryString = query;
        return request;
    }

    private async Task<queryMoreRequest> GetQueryMoreRequest(string queryLocator, int pageSize)
    {
        var token = await GetTokenAsync();
        queryMoreRequest request = new();
        request.SessionHeader = new SforceService.SessionHeader();
        request.SessionHeader.sessionId = token.AccessToken;
        request.QueryOptions = new QueryOptions();
        request.QueryOptions.batchSizeSpecified = true;
        request.QueryOptions.batchSize = pageSize;
        request.queryLocator = queryLocator;
        return request;
    }



    private async Task<describeSObjectsRequest> GetDescribeSObjectsRequest(IEnumerable<string> names)
    {
        var token = await GetTokenAsync();
        describeSObjectsRequest request = new describeSObjectsRequest();
        request.SessionHeader = new SforceService.SessionHeader();
        request.SessionHeader.sessionId = token.AccessToken;
        request.sObjectType = names.ToArray();
        return request;
    }

    private List<QueryFilter>? GenerateDeletedFilter(DescribeSObjectResult recordType, List<QueryFilter>? filters, bool includeDeleted)
    {
        if (!filters?.Any(f => f.PropertyName.EqualsIgnoreCase("IsDeleted")) ?? true
            && recordType.fields.Any(x => x.name.EqualsIgnoreCase("IsDeleted")))
        {
            var filter = new QueryFilter() { PropertyName = "IsDeleted", Value = [includeDeleted.ToString()] };
            if (filters is null)
            {
                filters = [filter];
            }
            else
            {
                filters.Add(filter);
            }
        }
        return filters;
    }
}