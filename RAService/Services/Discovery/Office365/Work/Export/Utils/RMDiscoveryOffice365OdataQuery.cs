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
using System.Dynamic;
using System.Linq;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.IE;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Utils;

public class RMDiscoveryOffice365OdataQuery
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365OdataQuery));
    
    private readonly IEApiClient _ieApiClient;

    private readonly RMDiscoveryOffice365SiteInfo _siteInfo;
    
    private readonly Guid _o365TenantId;
    
    private readonly SourceFlag _contentSource;
    
    private readonly Dictionary<string, string> _columNameMappingI18NDictionary;

    public RMDiscoveryOffice365OdataQuery(RMDiscoveryOffice365SiteInfo siteInfo, Guid o365TenantId, Dictionary<string, string> columNameMappingI18NDictionary)
    {
        _siteInfo = siteInfo;
        _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
        _o365TenantId = o365TenantId;
        _contentSource = siteInfo.ContentSource;
        _columNameMappingI18NDictionary = columNameMappingI18NDictionary;
    }

    public async IAsyncEnumerable<ExpandoObject> QueryAsync()
    {
        const int pageSize = 1000;

        var listManager = new RMDiscoveryOffice365ListManager(_o365TenantId, _siteInfo.SiteId);
        var listIds = await listManager.GetListsAsync();

        var exportColumns = string.Join(",", _columNameMappingI18NDictionary.Select(item => item.Key));

        foreach (var listId in listIds)
        {
            long latestItemId = 0;
            while (true)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?" +
                          $"$top={pageSize}" +
                          $"&filter=SiteId eq '{_siteInfo.SiteId}' " +
                          $"and ListId eq '{listId}' " +
                          $"and ItemId gt {latestItemId}" +
                          $"&$orderby=ItemId " +
                          $"&select={exportColumns},ItemId";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(
                    JsonConvert.SerializeObject(
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));

                foreach (var item in items)
                {
                    latestItemId = Math.Max(latestItemId, item.GetValue<long>("ItemId"));
                    yield return item;
                }

                if (items.Count < pageSize)
                {
                    break;
                }
            }
        }
    }
}