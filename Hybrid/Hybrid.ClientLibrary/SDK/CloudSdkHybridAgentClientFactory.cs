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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace AvePoint.Hybrid.ClientLibrary.SDK
{
    public class CloudSdkHybridAgentClientFactory : ICloudSdkHybridAgentClientFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly HybridAgentApiOption options;
        public CloudSdkHybridAgentClientFactory(IServiceProvider serviceProvider,
            IOptions<HybridAgentApiOption> options)
        {
            this.serviceProvider = serviceProvider;
            this.options = options.Value;
        }

        public HybridAgentApiClient CreateHybridAgentClient(string tenantId, string identityServerScope, string HybridAgentId = null, string HybridAgentAuth = null,string apiUrl = null)
        {
            apiUrl = apiUrl ?? options.DefaultApiUrl;
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new ArgumentNullException("apiUrl");
            }
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentNullException("tenantId");
            }
            var client = ActivatorUtilities.CreateInstance<HybridAgentApiClient>(serviceProvider);
            client.ApiUrl = apiUrl;
            client.TenantId = tenantId;
            client.IdentityServerScope = identityServerScope;
            client.HybridAgentId = HybridAgentId;
            client.HybridAgentAuth = HybridAgentAuth;

            return client;
        }
    }
}
