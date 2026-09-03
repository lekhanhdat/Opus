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

internal class SalesforceTokenService
{
    private static readonly ILogger logger = LoggerFactory.Get(typeof(SalesforceTokenService));
    public async Task<SFToken?> GetSalesTokenAsync(string customerId, string organizationId, bool forceNew = false)
    {
        bool getFromCache = true;
        var tokenKey = $"{customerId}_{organizationId}";
        var tokenJson = CacheService.Get(CacheNamespace.SalesforceToken, tokenKey);
        if (tokenJson.IsNullOrEmpty() || forceNew)
        {
            //if (true) // temporally without profile
            //{
            //    var token = await GetAccessToken();
            //    if (forceNew)
            //    {
            //        CacheService.Remove(CacheNamespace.SalesforceToken, tokenKey);
            //    }
            //    CacheService.Set(CacheNamespace.SalesforceToken, tokenKey, tokenJson = JsonConvert.SerializeObject(token), DateTime.UtcNow.AddMinutes(10));
            //    token.SetNeedRefresh(true);
            //    return token;
            //}
            //else
            //{
            if (forceNew)
            {
                CacheService.Remove(CacheNamespace.SalesforceToken, tokenKey);
            }
            var appProfile = RMAosApiClient.GetSalesforceAppProfile(customerId, organizationId);
            logger.Info($"Get app name {appProfile.Name}");
            ArgumentNullException.ThrowIfNull(appProfile, nameof(appProfile));
            getFromCache = false;
            var newToken = await GetAppToken(customerId, appProfile);
            CacheService.Set(CacheNamespace.SalesforceToken, tokenKey, tokenJson = newToken.AccessToken, newToken.ExpiresOn.UtcDateTime.AddMinutes(-10));
        }
        var sfToken = JsonConvert.DeserializeObject<SFToken>(tokenJson);
        if (sfToken is not null)
        {
            sfToken.SetNeedRefresh(!getFromCache);
            return sfToken;
        }
        throw new ArgumentNullException("Miss profile token");
    }

    private async Task<TokenResult> GetAppToken(string customerId, AppProfileInfo appProfile)
    {
        return await AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(customerId).
             ModernTokenService.GetTokenByAppProfileAsync(appProfile.Type,
                      TokenResourceType.Graph, appProfile.TenantId, appProfile.Id);
    }



}
