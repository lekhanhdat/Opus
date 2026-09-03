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
using Azure.Core;
using Azure.Identity;
using AvePoint.RAI.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RAI.Core.Utils
{
    /// <summary>
    /// Utility class for Azure cloud environment detection and credential configuration
    /// </summary>
    public static class AzureCloudEnvironmentHelper
    {
        /// <summary>
        /// Configures DefaultAzureCredentialOptions based on the Azure endpoint URL
        /// </summary>
        /// <param name="endpoint">Azure endpoint URL</param>
        /// <param name="logger">Logger instance for debug information</param>
        /// <returns>Configured DefaultAzureCredentialOptions</returns>
        public static DefaultAzureCredentialOptions ConfigureCredentialOptions(string endpoint, IAiLogger logger)
        {
            var credentialOptions = new DefaultAzureCredentialOptions();
            var cloudEnvironment = DetectAzureCloudEnvironment(endpoint);

            // Set the appropriate Authority Host based on detected cloud environment
            switch (cloudEnvironment)
            {
                case AzureCloudEnvironment.AzureGovernment:
                    credentialOptions.AuthorityHost = AzureAuthorityHosts.AzureGovernment;
                    logger.LogDebug("Using Azure US Government cloud authentication for endpoint: {0}", endpoint);
                    break;
                case AzureCloudEnvironment.AzureChina:
                    credentialOptions.AuthorityHost = AzureAuthorityHosts.AzureChina;
                    logger.LogDebug("Using Azure China cloud authentication for endpoint: {0}", endpoint);
                    break;
                case AzureCloudEnvironment.AzureGermany:
                    // Azure Germany Cloud was closed on October 29th, 2021
                    credentialOptions.AuthorityHost = AzureAuthorityHosts.AzurePublicCloud;
                    logger.LogWarning("Azure Germany cloud endpoint detected but service was closed. Using Azure Commercial cloud authentication for endpoint: {0}", endpoint);
                    break;
                case AzureCloudEnvironment.AzurePublicCloud:
                default:
                    credentialOptions.AuthorityHost = AzureAuthorityHosts.AzurePublicCloud;
                    logger.LogDebug("Using Azure Commercial cloud authentication for endpoint: {0}", endpoint);
                    break;
            }

            // Configure timeout for token acquisition to avoid long waits
            credentialOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(10);
            credentialOptions.Retry.MaxRetries = 2;

            return credentialOptions;
        }

        /// <summary>
        /// Gets the appropriate token request context for Azure OpenAI based on the Azure cloud environment
        /// </summary>
        /// <param name="endpoint">Azure endpoint URL</param>
        /// <param name="logger">Logger instance for debug information</param>
        /// <returns>TokenRequestContext with appropriate scopes for the cloud environment</returns>
        public static TokenRequestContext GetAzureOpenAITokenRequestContext(string endpoint, IAiLogger logger)
        {
            var cloudEnvironment = DetectAzureCloudEnvironment(endpoint);
            string scope;

            switch (cloudEnvironment)
            {
                case AzureCloudEnvironment.AzureGovernment:
                    scope = "https://cognitiveservices.azure.us/.default";
                    logger.LogDebug("Using Azure US Government cognitive services scope for endpoint: {0}", endpoint);
                    break;
                case AzureCloudEnvironment.AzureChina:
                    scope = "https://cognitiveservices.azure.cn/.default";
                    logger.LogDebug("Using Azure China cognitive services scope for endpoint: {0}", endpoint);
                    break;
                case AzureCloudEnvironment.AzureGermany:
                    // Azure Germany is deprecated, use commercial cloud scope
                    scope = "https://cognitiveservices.azure.com/.default";
                    logger.LogWarning("Azure Germany cloud detected but deprecated. Using Azure Commercial cognitive services scope for endpoint: {0}", endpoint);
                    break;
                case AzureCloudEnvironment.AzurePublicCloud:
                default:
                    scope = "https://cognitiveservices.azure.com/.default";
                    logger.LogDebug("Using Azure Commercial cognitive services scope for endpoint: {0}", endpoint);
                    break;
            }

            return new TokenRequestContext(new[] { scope });
        }

        /// <summary>
        /// Detects the Azure cloud environment based on the endpoint URL
        /// </summary>
        /// <param name="endpoint">Azure endpoint URL</param>
        /// <returns>The detected Azure cloud environment</returns>
        private static AzureCloudEnvironment DetectAzureCloudEnvironment(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return AzureCloudEnvironment.AzurePublicCloud;

            var endpointLower = endpoint.ToLowerInvariant();

            // More precise detection patterns
            if (endpointLower.Contains(".azure.us") || endpointLower.Contains(".usgovcloudapi.net"))
                return AzureCloudEnvironment.AzureGovernment;
            
            if (endpointLower.Contains(".azure.cn") || endpointLower.Contains(".chinacloudapi.cn"))
                return AzureCloudEnvironment.AzureChina;
            
            if (endpointLower.Contains(".microsoftazure.de") || endpointLower.Contains(".cloudapi.de"))
                return AzureCloudEnvironment.AzureGermany;

            return AzureCloudEnvironment.AzurePublicCloud;
        }

        /// <summary>
        /// Azure cloud environment enumeration
        /// </summary>
        private enum AzureCloudEnvironment
        {
            AzurePublicCloud,
            AzureGovernment,
            AzureChina,
            AzureGermany
        }

        /// <summary>
        /// Creates a properly configured Azure credential for Azure OpenAI that ensures correct scope usage
        /// </summary>
        /// <param name="endpoint">Azure endpoint URL</param>
        /// <param name="logger">Logger instance for debug information</param>
        /// <returns>Configured TokenCredential for Azure OpenAI with correct scope enforcement</returns>
        public static TokenCredential CreateAzureOpenAICredential(string endpoint, IAiLogger logger)
        {
            var credentialOptions = ConfigureCredentialOptions(endpoint, logger);
            var baseCredential = new DefaultAzureCredential(credentialOptions);
            var tokenContext = GetAzureOpenAITokenRequestContext(endpoint, logger);
            
            // Return a wrapper that ensures the correct scope is always used
            var scopedCredential = new ScopedTokenCredential(baseCredential, tokenContext, logger);
            
            logger.LogDebug("Created scoped Azure credential for endpoint: {0} with scope: {1}", endpoint, string.Join(", ", tokenContext.Scopes));
            return scopedCredential;
        }
    }

    /// <summary>
    /// A TokenCredential wrapper that ensures a specific scope is always used for token requests
    /// </summary>
    internal class ScopedTokenCredential : TokenCredential
    {
        private readonly TokenCredential _baseCredential;
        private readonly TokenRequestContext _fixedContext;
        private readonly IAiLogger _logger;

        public ScopedTokenCredential(TokenCredential baseCredential, TokenRequestContext fixedContext, IAiLogger logger)
        {
            _baseCredential = baseCredential ?? throw new ArgumentNullException(nameof(baseCredential));
            _fixedContext = fixedContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            // Always use the fixed context (correct scope) regardless of what's requested
            _logger.LogDebug("Enforcing correct scope {0} for token request (requested scope was: {1})", 
                string.Join(", ", _fixedContext.Scopes), 
                string.Join(", ", requestContext.Scopes));
            
            return _baseCredential.GetToken(_fixedContext, cancellationToken);
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            // Always use the fixed context (correct scope) regardless of what's requested
            _logger.LogDebug("Enforcing correct scope {0} for async token request (requested scope was: {1})", 
                string.Join(", ", _fixedContext.Scopes), 
                string.Join(", ", requestContext.Scopes));
            
            return _baseCredential.GetTokenAsync(_fixedContext, cancellationToken);
        }
    }
}