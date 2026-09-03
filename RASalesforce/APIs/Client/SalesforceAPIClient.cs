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

public class SalesforceAPIClient
{

    private SalesforceSoapAPIService soapService;
    private SalesforceRestAPIService restService;
    private SalesforceMetadataAPIService metadataService;
    private readonly string customerId;
    private readonly string organizationId;
    private readonly SalesforceTokenService tokenService = new();
    public SalesforceSoapAPIService SoapService => soapService;
    public SalesforceRestAPIService RestService => restService;
    public SalesforceMetadataAPIService MetaDataService => metadataService;

    public SalesforceAPIClient(string customerId, string organizationId)
    {
        this.customerId = customerId;
        this.organizationId = organizationId;
        var provider = new SalesforceAPIProvider(GetSFTokenAsync, RefreshSetting);
        soapService = new SalesforceSoapAPIService(provider);
        restService = new SalesforceRestAPIService(provider);
        metadataService = new SalesforceMetadataAPIService(provider);
    }

    public async Task RefreshLimit()
    {
        var orgLimits = await this.restService.GetOrganizationLimitsAsync();
        if (orgLimits is not null)
        {
            SalesforceAPIHelper.Instance.Refresh(orgLimits);
        }
    }

    internal async Task Init()
    {
        var token = await GetSFTokenAsync();
        soapService.InitClientSetting(token);
        restService.InitClientSetting(token);
        metadataService.InitClientSetting(token);
        await RefreshLimit();
    }

    private void RefreshSetting(SFToken token)
    {
        soapService.InitClientSetting(token);
        restService.InitClientSetting(token);
        metadataService.InitClientSetting(token);
    }

    private async Task<SFToken> GetSFTokenAsync(bool forceNew = false)
    {
        var token = await tokenService.GetSalesTokenAsync(customerId, organizationId, forceNew);
        ArgumentNullException.ThrowIfNull(token, nameof(token));
        return token;
    }
}