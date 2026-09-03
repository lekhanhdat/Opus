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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Services;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;

namespace AvePoint.RA.Common.AzureService
{
    
    public class FailoverGroupService
    {
        private static readonly HttpClient httpClient;

        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(FailoverGroupService));

        static FailoverGroupService()
        {
            httpClient = new HttpClient { Timeout = TimeSpan.FromHours(1) };
            httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2;)");
        }

        public static bool AddDatabasesToFailoverGroup(string databaseName)
        {
            try
            {
                using (var performace = new PerformanceScope("addDBToFailoverGroup"))
                {
                    var parameters = GetPrimaryDBServerInfo();
                    if(parameters == null)
                    {
                        return false;
                    }

                    string token = GetBearerToken(parameters);
                    if (string.IsNullOrEmpty(token))
                    {
                        logger.Warn("Can not get access token.");
                        return false;
                    }

                    var failoverGroups = GetFailoverGroupsByServer(parameters, token);
                    var failoverGroup = failoverGroups.FirstOrDefault();
                    if (failoverGroup == null)
                    {
                        logger.Warn("Can not get any failover group by server.");
                        return false;
                    }
                    logger.Info($"Failover Group: {failoverGroup.Name}");
                    var databases = GetDatabasesByServer(parameters, token);
                    var database = databases.FirstOrDefault(i => i.Name == databaseName);
                    if (database == null)
                    {
                        logger.Warn("Can not get the database by the server.");
                        return false;
                    }
                    if (failoverGroup.Properties.Databases.Contains(database.Id))
                    {
                        logger.Info("The database has already been added to the failover group");
                        return true;
                    }
                    
                    AddDatabaseToFailoverGroup(parameters, failoverGroup, database.Id, token);
                    // wait a few minutes then check if the database was added to the failover group
                    int retries = 0;
                    while (retries < 10)
                    {
                        retries++;
                        Thread.Sleep(5 * 1000);
                        failoverGroups = GetFailoverGroupsByServer(parameters, token);
                        failoverGroup = failoverGroups.FirstOrDefault();
                        if ((bool?)(failoverGroup?.Properties.Databases.Contains(database.Id)) ?? false)
                        {
                            logger.Info("Add the database to the failover group successfully.");
                            return true;
                        }
                    }
                    logger.Warn("Can not get the database by the failover group.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while adding databases to the failover group - {ex}");
                throw;
            }
        }

        public static bool DeleteDatabaseFromServerAndFog(string databaseName)
        {
            try
            {
                using (var performace = new PerformanceScope("DeleteDBFromFailoverGroup"))
                {
                    var parameters = GetPrimaryDBServerInfo();
                    if (parameters == null)
                    {
                        return false;
                    }

                    string token = GetBearerToken(parameters);
                    if (string.IsNullOrEmpty(token))
                    {
                        logger.Warn("Can not get access token.");
                        return false;
                    }

                    var failoverGroups = GetFailoverGroupsByServer(parameters, token);
                    var failoverGroup = failoverGroups.FirstOrDefault();
                    if (failoverGroup == null)
                    {
                        logger.Warn("Can not get any failover group by server.");
                        return false;
                    }
                    logger.Info($"Failover Group: {failoverGroup.Name}");
                    var database = GetDatabasesByDatabaseName(parameters, token, databaseName);
                    if (database == null)
                    {
                        logger.Warn("Can not get the database by the server.");
                        return false;
                    }
                    if (!failoverGroup.Properties.Databases.Contains(database.Id))
                    {
                        logger.Info("The database has not been added to the failover group");
                    }

                    var secondaryServer = failoverGroup.Properties.PartnerServers.FirstOrDefault();
                    if (secondaryServer != null)
                    {
                        logger.Info("Start to delete secondary db");
                        var deleteSecondaryResult = DeleteSecondaryDatabaseByServer(parameters, token, database.Name, secondaryServer.Id);
                        logger.Warn($"Delete secondary database result: {deleteSecondaryResult}.");
                    }

                    var deletePrimaryResult = DeleteDatabaseByServer(parameters, token, database.Name);
                    logger.Warn($"Delete primary database result: {deletePrimaryResult}.");
                    int serverRetries = 0;
                    while (serverRetries < 10)
                    {
                        serverRetries++;
                        Thread.Sleep(5 * 1000);
                        try
                        {
                            var serverDatabase = GetDatabasesByDatabaseName(parameters, token, database.Name);
                            if (serverDatabase == null)
                            {
                                logger.Warn("Delete database by the server successfully.");
                                break;
                            }

                            if (serverRetries == 9)
                            {
                                logger.Warn("Delete database by the server failed. Delete again.");
                                DeleteDatabaseByServer(parameters, token, database.Name);
                            }
                        }
                        catch (HttpRequestException httpEx)
                        {
                            if (httpEx.StatusCode == HttpStatusCode.NotFound)
                            {
                                logger.Warn("Delete database by the server successfully.Can not find.");
                                break;
                            }
                            logger.Error($"An error occurred while delete database by the server - {httpEx}.");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"An error occurred while delete database by the server - {ex}.");
                        }
                    }


                    // wait a few minutes then check if the database was deleted from the failover group
                    int retries = 0;
                    while (retries < 10)
                    {
                        retries++;
                        Thread.Sleep(5 * 1000);
                        failoverGroups = GetFailoverGroupsByServer(parameters, token);
                        failoverGroup = failoverGroups.First();
                        if ((!failoverGroup.Properties.Databases.Contains(database.Id)))
                        {
                            logger.Info("Delete the database from the failover group successfully.");
                            return true;
                        }
                    }
                    logger.Warn("Delete the database from the failover group failed.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while delete databases from the failover group - {ex}");
                return false;
            }
        }

        private static FailoverParameters GetPrimaryDBServerInfo()
        {
            string primaryServer = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_PRIMARY_SERVER];
            if (string.IsNullOrEmpty(primaryServer) || primaryServer.Split('/').Length != 3)
            {
                logger.Warn("The primary server is not set.");
                return null;
            }

            return new FailoverParameters(primaryServer);
        }

        public static string GetPrimaryDBServerName() 
        {
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.AppConfig.DatabasePrimaryServerName))
            {
                return RMGlobalConfiguration.AppConfig.DatabasePrimaryServerName;
            }

            var parameters = GetPrimaryDBServerInfo(); 
            if (parameters == null)
            {
                return string.Empty;
            }

            string token = GetBearerToken(parameters);
            var failoverGroups = GetFailoverGroupsByServer(parameters, token);
            var failoverGroup = failoverGroups.FirstOrDefault();
            if (failoverGroup == null)
            {
                logger.Warn("Can not get any failover group by server.");
                return string.Empty;
            }

            var PrimaryDB = failoverGroup.Properties.PartnerServers
                .Where(p => p.ReplicationRole == FailoverGroupReplicationRole.Primary).FirstOrDefault();
            var result = PrimaryDB != null ? PrimaryDB.Id.Split('/').Last() : parameters.ServerName;
            RMGlobalConfiguration.AppConfig.DatabasePrimaryServerName = result;
            return result;
        }

        private static List<FailoverGroup> GetFailoverGroupsByServer(FailoverParameters parameters, string token)
        {
            string requestUri = $"{parameters.ToUri()}/failoverGroups?api-version=2021-11-01";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"GetFailoverGroupsByServer failed. {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            //response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ListResult<FailoverGroup>>(content).Value;
        }

        private static List<Database> GetDatabasesByServer(FailoverParameters parameters, string token)
        {
            string requestUri = $"{parameters.ToUri()}/databases?api-version=2021-11-01";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"GetFailoverGroupsByServer failed. {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            //response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ListResult<Database>>(content).Value;
        }

        private static Database GetDatabasesByDatabaseName(FailoverParameters parameters, string token, string databaseName)
        {
            string requestUri = $"{parameters.ToUri()}/databases/{databaseName}?api-version=2021-11-01";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"GetFailoverGroupsByServer failed. {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            //response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<Database>(content);
        }

        private static bool DeleteDatabaseByServer(FailoverParameters parameters, string token, string databaseName)
        {
            string requestUri = $"{parameters.ToUri()}/databases/{databaseName}?api-version=2021-11-01";
            logger.Info($"DeleteDatabaseByServer requestUri: {requestUri}");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = httpClient.DeleteAsync(requestUri).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"DeleteFailoverGroupsByServer failed. {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            logger.Info($"DeleteDatabaseByServer response: {response.IsSuccessStatusCode}");
            //response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }

        private static bool DeleteSecondaryDatabaseByServer(FailoverParameters parameters, string token, string databaseName, string secondaryServer)
        {
            var requestUri = $"{parameters.ResourceManager}{secondaryServer}/databases/{databaseName}?api-version=2021-11-01";
            logger.Info($"DeleteSecondaryDatabaseByServer requestUri: {requestUri}");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = httpClient.DeleteAsync(requestUri).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"Delete SecondaryDatabaseByServer failed. {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            }
            logger.Info($"DeleteSecondaryDatabaseByServer response: {response.IsSuccessStatusCode}");
            //response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }

        private static string GetBearerToken(FailoverParameters parameters)
        {
            if (!RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                try
                {
                    var managementUrl = AzureUtil.GetManagementResourceId();
                    return AzureUtil.GetTokenByPodIdentity(managementUrl);
                }
                catch (Exception ex)
                {
                    logger.Error($"GetTokenByPodIdentity error: {ex}");
                }
            }

            var clientId = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.KEY_VAULT_CLIENT_ID];
            string thumbprint = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.MASTER_CERTIFICATE_THUMBPRINT];
            HttpResponseMessage response;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{parameters.ToUri()}/failoverGroups?api-version=2021-11-01"))
            {
                response = httpClient.SendAsync(request).Result;
            }
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"GetFailoverGroupsByServer failed. {response.StatusCode}");
                //response.EnsureSuccessStatusCode();
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                string authority = GetAuthority(response.Headers.WwwAuthenticate.ElementAt(0).ToString());
                // var context = new AuthenticationContext(authority);
                var certificate = GetCertificateFromLocal(thumbprint);

                var app = ConfidentialClientApplicationBuilder.Create(clientId)
                   .WithCertificate(certificate)
                   .WithAuthority(authority)
                   .Build();
                var authResult = app.AcquireTokenForClient(
                    new[] {new Uri(parameters.ResourceManager).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" })
                    .ExecuteAsync().Result.AccessToken;
                return authResult;
                //context.AcquireTokenAsync(parameters.ResourceManager, new ClientAssertionCertificate(clientId, certificate)).Result.AccessToken;
            }
            return null;
        }

        private static string GetAuthority(string header)
        {
            if (!string.IsNullOrEmpty(header) && header.Trim().StartsWith("Bearer "))
            {
                var match = Regex.Match(header, "(authorization|authorization_uri)=\"(.+?)\"");
                if (match.Groups.Count == 3)
                {
                    return match.Groups[2].Value;
                }
            }
            return null;
        }

        private static void AddDatabaseToFailoverGroup(FailoverParameters parameters, FailoverGroup failoverGroup, string databaseId, string token)
        {
            string requestUri = $"{parameters.ToUri()}/failoverGroups/{failoverGroup.Name}?api-version=2021-11-01";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            failoverGroup.Properties.Databases.Add(databaseId);
            string content = JsonConvert.SerializeObject(new FailoverGroup
            {
                Properties = new FailoverGroupProperties
                {
                    Databases = failoverGroup.Properties.Databases,
                    ReadWriteEndpoint = failoverGroup.Properties.ReadWriteEndpoint
                }
            });
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn($"GetFailoverGroupsByServer failed. {response.StatusCode}");
                //response.EnsureSuccessStatusCode();
            }
            response.EnsureSuccessStatusCode();
        }



        //private static string GetBearerToken(string authority, string resource, string clientId, string thumbprint)
        //{
        //    var certificate = GetCertificateFromLocal(thumbprint);
        //    var context = new AuthenticationContext(authority);
        //    return context.AcquireTokenAsync(resource, new ClientAssertionCertificate(clientId, certificate)).Result.AccessToken;
        //}

        private static X509Certificate2 GetCertificateFromLocal(string thumbprint)
        {
            return GetCertificateFromLocal(thumbprint, StoreLocation.LocalMachine)
                ?? GetCertificateFromLocal(thumbprint, StoreLocation.CurrentUser);
        }

        private static X509Certificate2 GetCertificateFromLocal(string thumbprint, StoreLocation storeLocation)
        {
            using (var store = new X509Store(storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                return certificates.Count > 0 ? certificates[0] : null;
            }
        }

        /*private static T Execute<T>(Func<T> func)
        {
            Polly.Retry.RetryPolicy retry = Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3) });
            TimeoutPolicy timeout = Policy.Timeout(TimeSpan.FromMinutes(4), TimeoutStrategy.Pessimistic);
            var wrap = retry.Wrap(timeout);
            try
            {
                return wrap.Execute(func);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while get data from azure. {0}, {1} {2}", func.Method.Name, e.Message, e);
                throw;
            }
        }*/
    }
}