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
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.GraphApi;
using Cloud.Sdk.Data.AosModern;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.Tenant
{
    public class RMGraphTenantManager : RMGraphApiManager
    {

        public RMGraphTenantManager(string tenantId) : base(tenantId)
        {
        }

        public async Task<IEnumerable<RMGraphTenantSubscribedSku>> GetSubscribedSkusAsync()
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/subscribedSkus";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGraphApiResponse<List<RMGraphTenantSubscribedSku>>>(resultJson);
            return result.Value;
        }

        public async Task<IEnumerable<RMGraphTenantSubscribedSku>> GetSharePointSubscribedSkusAsync()
        {
            var subscribedSkus = await GetSubscribedSkusAsync();
            return subscribedSkus.Where(item => RMGraphTenantConstants.SHAREPOINT_AVAILABLE_SUBSCRIPTION.Contains(item.SkuPartNumber.ToUpper()));
        }

        public async Task<ItemInfo> GetItemArchiveStatusAsync(string siteId, string listId,int rowId)
        {
            var requestUri = $"{GraphEndPoint}/{BetaApi}/sites/{siteId}/lists/{listId}/items/{rowId}?$expand=fields($select=_FileArchiveStatus)";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<ItemInfo>(resultJson);
            return result;
        }
        public async Task<string> SetItemToArchiveStatusAsync(string siteId, string listId, int rowId)
        {
            var requestUri = $"{GraphEndPoint}/{BetaApi}/sites/{siteId}/lists/{listId}/items/{rowId}/driveItem/archive";
            var resultJson = await HttpHelper.PostAsync(requestUri,"", AccessToken);
            return resultJson;
        }
        public async Task<string> SetItemToUnarchiveStatusAsync(string siteId, string listId, int rowId)
        {
            var requestUri = $"{GraphEndPoint}/{BetaApi}/sites/{siteId}/lists/{listId}/items/{rowId}/driveItem/unarchive";
            var resultJson = await HttpHelper.PostAsync(requestUri, "", AccessToken);
            return resultJson;
        }
        public async Task<string> SetSiteToArchiveStatusAsync(string siteId)
        {
            var requestUri = $"{GraphEndPoint}/{BetaApi}/sites/{siteId}/archive";
            var resultJson = await HttpHelper.PostAsync(requestUri, "", AccessToken);
            return resultJson;
        }
    }
}
