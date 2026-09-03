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
using Amazon.Runtime.Internal.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AzureService;
using AvePoint.RA.Contract.Configurations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Util.MSAzure;

namespace AvePoint.RA.Common.AzureService
{
    public class AzureSqlManagementService
    {
        // Use model types from AzureResourceModels
        private static readonly Type azureModelsType = typeof(AzureResourceModels);
        private static RALogger logger = RALogger.GetInstance(typeof(AzureSqlManagementService));

        private static HttpClient client = new HttpClient();
        private static AzureEnvironment CurrentAzureEnvironment
        {
            get
            {
                // Use RMGlobalConfiguration to get the current Azure environment
                var envSetting = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.AZURE_ENVIRONMENT];
                if (string.IsNullOrEmpty(envSetting))
                {
                    return AzureEnvironment.Worldwide;
                }

                if (Enum.TryParse<AzureEnvironment>(envSetting, out var environment))
                {
                    return environment;
                }

                return AzureEnvironment.Worldwide;
            }
        }

        public static String ResourceManagerEndpoint
        {
            get
            {
                switch (CurrentAzureEnvironment)
                {
                    case AzureEnvironment.USGovGCCHigh:
                        return Endpoints.USGovGCCHigh.ResourceManager.TrimEnd('/');
                    case AzureEnvironment.China:
                        return Endpoints.China.ResourceManager.TrimEnd('/');
                    case AzureEnvironment.Germany:
                        return Endpoints.Germany.ResourceManager.TrimEnd('/');
                    default:
                    case AzureEnvironment.Worldwide:
                        return Endpoints.Worldwide.ResourceManager.TrimEnd('/');
                }
            }
        }
        public static String ActiveDirectoryLoginEndpoint
        {
            get
            {
                switch (CurrentAzureEnvironment)
                {
                    case AzureEnvironment.USGovGCCHigh:
                        return Endpoints.USGovGCCHigh.ActiveDirectory.TrimEnd('/');
                    case AzureEnvironment.China:
                        return Endpoints.China.ActiveDirectory.TrimEnd('/');
                    case AzureEnvironment.Germany:
                        return Endpoints.Germany.ActiveDirectory.TrimEnd('/');
                    default:
                    case AzureEnvironment.Worldwide:
                        return Endpoints.Worldwide.ActiveDirectory.TrimEnd('/');
                }
            }
        }

        public static String DatabaseSuffix
        {
            get
            {
                switch (CurrentAzureEnvironment)
                {
                    case AzureEnvironment.USGovGCCHigh:
                        return "database.usgovcloudapi.net";
                    case AzureEnvironment.China:
                        return "database.chinacloudapi.cn";
                    case AzureEnvironment.Germany:
                        return "database.cloudapi.de";
                    default:
                    case AzureEnvironment.Worldwide:
                        return "database.windows.net";
                }
            }
        }
        /// <summary>
        /// Helper method for HTTP requests
        /// </summary>
        private static string HttpClientHelper_Send(string url, string method, string accessToken, string contentType, string content, Dictionary<string, string> headers)
        {
            //using (HttpClient client = new HttpClient())
            //{
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url);

                // Add authorization header
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Add other headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // Add content if provided
                if (!string.IsNullOrEmpty(content))
                {
                    HttpContent httpContent = new StringContent(content, Encoding.UTF8);

                    if (!string.IsNullOrEmpty(contentType))
                    {
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    }

                    request.Content = httpContent;
                }

                // Send the request and get the response
                HttpResponseMessage response = client.SendAsync(request).Result;

                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var responseErrorContent = response.Content.ReadAsStringAsync().Result;
                    logger.Error($"HTTP request failed with status code {response.StatusCode}: {responseErrorContent}");
                    throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}");
                }

                // Return the response content
                var responseContent = response.Content.ReadAsStringAsync().Result;
                return responseContent;
            //}
        }
        public static List<AzureResourceModels.Database> GetDatabasesByServer(String subscriptionId,
            String resourceGroup,
            String serverName,
            String accessToken)
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/databases?api-version=2017-10-01-preview";
            var content = HttpClientHelper_Send(url, "GET", accessToken, null, null, null);
            var tempResult = JObject.Parse(content);
            return JsonConvert.DeserializeObject<List<AzureResourceModels.Database>>(tempResult["value"].ToString());
        }

        public static List<AzureResourceModels.ElasticPool> GetElasticPoolsByServer(String subscriptionId,
            String resourceGroup,
            String serverName,
            String accessToken)
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/elasticPools?api-version=2017-10-01-preview";
            var content = HttpClientHelper_Send(url, "GET", accessToken, null, null, null);
            var tempResult = JObject.Parse(content);
            return JsonConvert.DeserializeObject<List<AzureResourceModels.ElasticPool>>(tempResult["value"].ToString());
        }

        public static List<AzureResourceModels.ElasticPool> GetElasticPoolsByServerId(
            String serverId,
            String accessToken)
        {
            var url = $"{ResourceManagerEndpoint}{serverId}/elasticPools?api-version=2017-10-01-preview";
            var content = HttpClientHelper_Send(url, "GET", accessToken, null, null, null);
            var tempResult = JObject.Parse(content);
            return JsonConvert.DeserializeObject<List<AzureResourceModels.ElasticPool>>(tempResult["value"].ToString());
        }

        public static List<AzureResourceModels.Database> GetDatabasesByPool(String subscriptionId,
            String resourceGroup,
            String serverName,
            String elasticPoolName,
            String accessToken
            )
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/elasticPools/{elasticPoolName}/databases?api-version=2017-10-01-preview";
            var content = HttpClientHelper_Send(url, "GET", accessToken, null, null, null);
            var tempResult = JObject.Parse(content);
            return JsonConvert.DeserializeObject<List<AzureResourceModels.Database>>(tempResult["value"].ToString());
        }
        public static void CreateElasticPool(String subscriptionId,
            String resourceGroup,
            String serverName,
            String elasticPoolName,
            String accessToken,
            AzureResourceModels.CreateElasticPoolRequest request
            )
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/elasticPools/{elasticPoolName}?api-version=2017-10-01-preview";
            HttpClientHelper_Send(url, "PUT", accessToken, "application/json", JsonConvert.SerializeObject(request), null);
        }

        public static void CreateElasticPool(String serverId,
            String elasticPoolName,
            String accessToken,
            AzureResourceModels.CreateElasticPoolRequest request
            )
        {
            var url = $"{ResourceManagerEndpoint}{serverId}/elasticPools/{elasticPoolName}?api-version=2017-10-01-preview";
            HttpClientHelper_Send(url, "PUT", accessToken, "application/json", JsonConvert.SerializeObject(request), null);
        }

        public static void MoveDatabaseToElasticPool(String subscriptionId,
            String resourceGroup,
            String serverName,
            String databaseName,
            String accessToken,
            AzureResourceModels.UpdateDatabaseRequest request
            )
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/databases/{databaseName}?api-version=2017-10-01-preview";
            HttpClientHelper_Send(url, "PATCH", accessToken, "application/json", JsonConvert.SerializeObject(request), null);
        }
        public static List<AzureResourceModels.FailoverGroup> GetFailoverGroupsByServer(String subscriptionId,
            String resourceGroup,
            String serverName,
            String accessToken)
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/failoverGroups?api-version=2015-05-01-preview";
            var content = HttpClientHelper_Send(url, "GET", accessToken, null, null, null);
            var tempResult = JObject.Parse(content);
            return JsonConvert.DeserializeObject<List<AzureResourceModels.FailoverGroup>>(tempResult["value"].ToString());
        }

        public static void AddDBOrPoolToFailoverGroup(String subscriptionId,
            String resourceGroup,
            String serverName,
            String fogName,
            String accessToken,
            AzureResourceModels.FailoverGroup group)
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/failoverGroups/{fogName}?api-version=2015-05-01-preview";
            HttpClientHelper_Send(url, "PATCH", accessToken, "application/json", JsonConvert.SerializeObject(group), null);
        }

        public static void CreateFailoverGroup(String subscriptionId,
            String resourceGroup,
            String serverName,
            String fogName,
            String accessToken,
            AzureResourceModels.FailoverGroup group)
        {
            var url = $"{ResourceManagerEndpoint}/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{serverName}/failoverGroups/{fogName}?api-version=2015-05-01-preview";
            HttpClientHelper_Send(url, "PUT", accessToken, "application/json", JsonConvert.SerializeObject(group), null);
        }
    }
    
   
    }
