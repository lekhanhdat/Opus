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
namespace AvePoint.GCommon.Utility.AvePointService
{
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility.Exceptions;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.ConfigSettings;
    using AvePoint.GCommon.Utility.Cryptography;

    /// <summary>
    /// Some functional methods to interactive with AvePoint service
    /// </summary>
    public class AresUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AresUtility));

        public static HttpClient client = new HttpClient(new ValidationMessageHandler(new HttpClientHandler())) {  Timeout= TimeSpan.FromSeconds(60) };

        /// <summary>
        /// The service url
        /// </summary>
        public static string ServiceUrl { get; private set; }



        /// <summary>
        /// Set the service url firstly
        /// </summary>
        /// <param name="serviceUrl"></param>
        public static void SetServiceUrl(string serviceUrl)
        {
            ServiceUrl = serviceUrl;
        }

        /// <summary>
        /// Send registration info to AvePoint service to register event to the SharePoint site
        /// </summary>
        /// <param name="infos"></param>
        public static void SendRegistrationInfos(RegistrationDto[] infos)
        {
            //using (HttpClient client = CreateHttpClientWithValidation())
            //{
                //client.Timeout = TimeSpan.FromSeconds(30 * infos.Count());
                //client.BaseAddress = new Uri(ServiceUrl);
                string returnResult = string.Empty;
                int retryCount = 2;
                while (true)
                {
                    try
                    {
                        HttpContent httpContent = GetJsonContent(infos);
                        var result = client.PostAsync($"{ServiceUrl}/api/EventReceivers/Register", httpContent).Result;
                        returnResult = result.Content.ReadAsStringAsync().Result;
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            var registerResults = JsonConvert.DeserializeObject<RegisterResultDto[]>(returnResult);
                            var failedRegisterResults = registerResults.Where(r => r.HasError);
                            if (failedRegisterResults != null && failedRegisterResults.Count() > 0)
                            {
                                var errorMsgs = failedRegisterResults.Select(r => r.ErrorMsg);
                                var ids = failedRegisterResults.Select(r => r.CorrelationId);
                                throw new AresException(GetTotalString(errorMsgs), result.StatusCode.ToString(), GetTotalString(ids));
                            }
                            break;
                        }
                        else
                        {
                            var registerResults = default(RegisterResultDto[]);
                            if (TryDoJsonDeserializer<RegisterResultDto[]>(returnResult, out registerResults))
                            {
                                var registerResult = registerResults.First();
                                throw new AresException(registerResult?.ErrorMsg, result.StatusCode.ToString(), registerResult?.CorrelationId);
                            }
                            else
                            {
                                throw new AresException(returnResult, result.StatusCode.ToString());
                            }
                        }
                    }
                    catch (AresException aresEx)
                    {
                        logger.Error("An error occurred while sending registration information with error: {0}", aresEx.ToString());
                        throw aresEx;
                    }
                    catch (Exception ex)
                    {
                        if (retryCount <= 0)
                        {
                            logger.Error("The retry count has reached the maximum 3 times, so do not reconnect.");
                            throw ex;
                        }
                        logger.Error("An error occurred while sending registration information with error: {0}, try to reconnect.", ex.ToString());
                    }
                    retryCount--;
                    Thread.Sleep(1000);
                }
            //}
        }

        static bool TryDoJsonDeserializer<T>(string value, out T result)
        {
            var success = false;
            result = default(T);
            try
            {
                result = JsonConvert.DeserializeObject<T>(value);
                success = true;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred when deserializing data by json. Error: {0}", ex.ToString());
            }
            return success;
        }

        static string GetTotalString(IEnumerable<String> collection)
        {
            var sb = new StringBuilder();
            collection.ToList().ForEach(ele => { sb.AppendLine(ele); });
            return sb.ToString();
        }

        /// <summary>
        /// Send service bus information to AvePoint service
        /// </summary>
        /// <param name="connectionDto">Service bus related information</param>
        /// <returns></returns>
        public static Endpoint RegistServiceBusInfoToAvePointService(SbConnectionDto connectionDto)
        {
            Endpoint endpoint = null;
            //using (HttpClient client = CreateHttpClientWithValidation())
            //{
                //client.Timeout = TimeSpan.FromSeconds(60);
                //client.BaseAddress = new Uri(ServiceUrl);
                string returnResult = string.Empty;
                int retryCount = 2;
                while (true)
                {
                    try
                    {
                        HttpContent httpContent = GetJsonContent(connectionDto);
                        var result = client.PostAsync($"{ServiceUrl}/api/EndPoints/Register", httpContent).Result;
                        returnResult = result.Content.ReadAsStringAsync().Result;
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            endpoint = JsonConvert.DeserializeObject<Endpoint>(returnResult);
                            break;
                        }
                        else
                        {
                            if (TryDoJsonDeserializer<Endpoint>(returnResult, out endpoint))
                            {
                                throw new AresException(endpoint.ErrorMsg, result.StatusCode.ToString(), endpoint.CorrelationId);
                            }
                            else
                            {
                                throw new AresException(returnResult, result.StatusCode.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (retryCount <= 0)
                        {
                            logger.Error("The retry count has reached the maximum 3 times, so do not reconnect.");
                            throw ex;
                        }
                        logger.Error("An error occurred while sending information with error: {0}, try to reconnect.", ex.ToString());
                    }
                    retryCount--;
                    Thread.Sleep(1000);
                }
            //}
            return endpoint;
        }

        /// <summary>
        /// Get service bus related information use client group key
        /// </summary>
        /// <param name="clientGroupId">Client group ID</param>
        /// <returns></returns>
        public static Endpoint GetEndpointInfoWithClientGroupId(Guid clientGroupId)
        {
            Endpoint endpoint = null;
            //using (HttpClient httpClient = CreateHttpClientWithValidation())
            //{
                //client.Timeout = TimeSpan.FromSeconds(60);
                //client.BaseAddress = new Uri(ServiceUrl);
                string returnResult = string.Empty;
                int retryCount = 2;
                while (true)
                {
                    try
                    {
                        var result = client.GetAsync(string.Format($"{ServiceUrl}/api/EndPoints/Get?clientGroupId={0}", clientGroupId.ToString())).Result;
                        returnResult = result.Content.ReadAsStringAsync().Result;
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            endpoint = JsonConvert.DeserializeObject<Endpoint>(returnResult);
                            break;
                        }
                        else
                        {
                            if (TryDoJsonDeserializer<Endpoint>(returnResult, out endpoint))
                            {
                                throw new AresException(endpoint.ErrorMsg, result.StatusCode.ToString(), endpoint.CorrelationId);
                            }
                            else
                            {
                                throw new AresException(returnResult, result.StatusCode.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (retryCount <= 0)
                        {
                            logger.Error("The retry count has reached the maximum 3 times, so do not reconnect.");
                            throw ex;
                        }
                        logger.Error("An error occurred while getting information use client group key with error: {0}, try to reconnect.", ex.ToString());
                    }
                    retryCount--;
                    Thread.Sleep(1000);
                }
            //}
            return endpoint;
        }

        /// <summary>
        /// Get Endpoint information with instance ID and module
        /// </summary>
        /// <param name="instanceId">Data center</param>
        /// <param name="serviceUrl">AvePoint service url</param>
        /// <param name="module">Module</param>
        /// <returns>Endpoint information</returns>
        public static Endpoint GetEndpointInfoWithInstanceIdAndModule(string instanceId, Module module)
        {
            Endpoint endpoint = null;
            //using (HttpClient httpClient = CreateHttpClientWithValidation())
            //{
                //client.Timeout = TimeSpan.FromSeconds(60);
                //client.BaseAddress = new Uri(ServiceUrl);
                string returnResult = string.Empty;
                int retryCount = 2;
                while (true)
                {
                    try
                    {
                        var result = client.GetAsync(string.Format($"{ServiceUrl}/api/EndPoints/Get?instanceId={0}&module={1}", instanceId, module.ToString())).Result;
                        returnResult = result.Content.ReadAsStringAsync().Result;
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            endpoint = JsonConvert.DeserializeObject<Endpoint>(returnResult);
                            break;
                        }
                        else
                        {
                            if (TryDoJsonDeserializer<Endpoint>(returnResult, out endpoint))
                            {
                                throw new AresException(endpoint.ErrorMsg, result.StatusCode.ToString(), endpoint.CorrelationId);
                            }
                            else
                            {
                                throw new AresException(returnResult, result.StatusCode.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (retryCount <= 0)
                        {
                            logger.Error("The retry count has reached the maximum 3 times, so do not reconnect.");
                            throw ex;
                        }
                        logger.Error("An error occurred while getting information use instance ID and module with error: {0}, try to reconnect.", ex.ToString());
                    }
                    retryCount--;
                    Thread.Sleep(1000);
                }
            //}
            return endpoint;
        }

        /// <summary>
        /// Get Endpoint information with instance ID and module and environment information
        /// </summary>
        /// <param name="instanceId">Data center</param>
        /// <param name="module">Module</param>
        /// <param name="isStaging">Is staging enviroment</param>
        /// <returns></returns>
        public static Endpoint GetEndpointInfoWIthInstanceIdAndModuleWithEnvInfo(string instanceId, Module module, bool isStaging)
        {
            Endpoint endpoint = null;
            //using (HttpClient httpClient = CreateHttpClientWithValidation())
            //{
                //client.Timeout = TimeSpan.FromSeconds(60);
                //client.BaseAddress = new Uri(ServiceUrl);
                string returnResult = string.Empty;
                int retryCount = 2;
                while (true)
                {
                    try
                    {
                        var result = client.GetAsync(string.Format($"{ServiceUrl}/api/EndPoints/Get?instanceId={0}&module={1}&isStaging={2}", instanceId, module.ToString(), isStaging)).Result;
                        returnResult = result.Content.ReadAsStringAsync().Result;
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            endpoint = JsonConvert.DeserializeObject<Endpoint>(returnResult);
                            break;
                        }
                        else
                        {
                            if (TryDoJsonDeserializer<Endpoint>(returnResult, out endpoint))
                            {
                                throw new AresException(endpoint.ErrorMsg, result.StatusCode.ToString(), endpoint.CorrelationId);
                            }
                            else
                            {
                                throw new AresException(returnResult, result.StatusCode.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (retryCount <= 0)
                        {
                            logger.Error("The retry count has reached the maximum 3 times, so do not reconnect.");
                            throw ex;
                        }
                        logger.Error("An error occurred while getting information use instance ID and module with error: {0}, try to reconnect.", ex.ToString());
                    }
                    retryCount--;
                    Thread.Sleep(1000);
                }
            //}
            return endpoint;
        }

        /// <summary>
        /// Assemble RegistrationDto instance method
        /// </summary>
        /// <param name="firstCheckNode">First checked node</param>
        /// <param name="extension">Extension information based on inquirement of each module</param>
        /// <param name="clientGroupId">Client group key ID</param>
        /// <param name="eventTypes">Event receiver types that need to be registed</param>
        /// <param name="scopeId">Const GUID value based on each module</param>
        /// <returns></returns>
        public static RegistrationDto AssembleRegitrationInfo(SPTreeNodeDto firstCheckNode, string extension, string clientGroupId, EventType[] eventTypes, string scopeId)
        {
            SPTreeNodeDto siteNode = GetLastNeedTreeNodeDto(firstCheckNode, NodeLevel.SiteCollection);
            SPTreeNodeDto webNode = null;
            SPTreeNodeDto listNode = null;
            string siteUrl = siteNode.FullPath.TrimEnd('/');
            string webUrl = string.Empty;
            string listTitle = string.Empty;
            ObjectType objType = ObjectType.Site;
            if (firstCheckNode.Level >= NodeLevel.Site)
            {
                objType = ObjectType.Web;
                webNode = GetLastNeedTreeNodeDto(firstCheckNode, NodeLevel.Site);
                webUrl = webNode.FullPath.TrimEnd('/');
                if (siteUrl.Equals(webUrl, StringComparison.OrdinalIgnoreCase))
                {
                    webUrl = "/";
                }
                else
                {
                    webUrl = webUrl.Substring(siteUrl.Length);
                }
            }
            if (firstCheckNode.Level >= NodeLevel.List)
            {
                objType = ObjectType.List;
                listNode = GetLastNeedTreeNodeDto(firstCheckNode, NodeLevel.List);
                listTitle = listNode.Name;
            }

            RegistrationDto dto = new RegistrationDto();
            dto.ClientGroupId = clientGroupId;
            dto.Credential = new RegistrationCredentialDto();
            dto.Credential.UserName = siteNode.NodeExtension.BposInfo.UserAccountInfo.Username;
            dto.Credential.UserPass = siteNode.NodeExtension.BposInfo.UserAccountInfo.Password;
            dto.EventTypes = eventTypes;
            dto.Extension = extension;
            dto.ListTitle = listTitle;
            dto.Module = Module.RP;
            dto.ObjectType = objType;
            dto.RelativeWeb = webUrl;
            dto.ScopeId = scopeId;
            dto.SiteUrl = siteUrl;
            return dto;
        }

        /// <summary>
        ///  Make a default service bus path
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static String MakeServiceBusPath(string instanceId, Module module)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return string.Empty;
            }
            var newIns = new StringBuilder();

            foreach (var c in instanceId)
            {
                if (char.IsLetterOrDigit(c)
                    || c == '-'
                    || c == '_'
                    || c == '.')
                {
                    newIns.Append(c);
                }
            }

            if (string.IsNullOrEmpty(newIns.ToString()))
            {
                throw new Exception("Invalid instance. It can contain only letters, numbers, periods (.), hyphens (-), and underscores (_).");
            }

            return string.Format("ares_{0}_{1}", newIns.ToString(), module);
        }

        private static HttpContent GetJsonContent(Object obj)
        {
            string tempString = JsonConvert.SerializeObject(obj);
            StringContent content = new StringContent(tempString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        //private static HttpClient CreateHttpClientWithValidation()
        //{
        //    return new HttpClient(new ValidationMessageHandler(new HttpClientHandler()));
        //}

        private static SPTreeNodeDto GetLastNeedTreeNodeDto(SPTreeNodeDto treeNode, NodeLevel level)
        {
            if (treeNode.Level == level)
            {
                return treeNode;
            }
            else if (treeNode.Level > level)
            {
                SPTreeNodeDto tempTreeNode = treeNode;
                while (tempTreeNode.Parent != null)
                {
                    tempTreeNode = tempTreeNode.Parent;
                    if (tempTreeNode.Level == level)
                    {
                        return tempTreeNode;
                    }
                }
            }
            return null;
        }
    }

    internal class ValidationMessageHandler : DelegatingHandler
    {
        public ValidationMessageHandler(HttpClientHandler handler)
            : base(handler)
        {

        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("apikey", "A70975FA-7389-4BBD-97E4-9B6651EE1FD8");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
