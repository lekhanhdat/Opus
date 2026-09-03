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
namespace ExchangeUtility.Graph
{
    using AvePoint.RA.CommonUtil;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    public abstract class YammerAPIBase<TValue>
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(YammerAPIBase<TValue>));
        internal const string VersionV1 = "v1";

        private string apiUrlBase = string.Empty;
        protected string apiUrlV1 = string.Empty;
        protected Func<string> refreshAccessToken;
        protected IYammerRetryable retryController;
        protected HttpMethod httpMethod = HttpMethod.Get;
        protected abstract string RequestUrl { get; }
        internal IDictionary<string, string> RequestHeader;
        internal IDictionary<string, string> ResponseHeader;
        protected virtual IEnumerable<string> IncludePropertiesName { get; }

        public YammerAPIBase(string baseUrl, Func<string> refreshToken)
        {
            this.apiUrlBase = baseUrl.TrimEnd('/');
            this.apiUrlV1 = $"{this.apiUrlBase}/{VersionV1}";
            this.refreshAccessToken = refreshToken;
        }

        public YammerAPIBase(string baseUrl, Func<string> refreshToken, IYammerRetryable retryable) : this(baseUrl, refreshToken)
        {
            this.retryController = retryable;
        }

        public abstract TValue GetApiResult();

        protected TValue Get()
        {
            this.httpMethod = HttpMethod.Get;
            var json = Execute(null, this.RequestUrl);
            return JsonDeserializer<TValue>(json);
        }

        protected TValue Post(Object postContentDto)
        {
            this.httpMethod = HttpMethod.Post;
            var json = Execute(ConvertToJsonString(postContentDto), this.RequestUrl);
            return JsonDeserializer<TValue>(json);
        }
        protected TValue Put(Object content)
        {
            this.httpMethod = HttpMethod.Put;
            var json = Execute(ConvertToJsonString(content), this.RequestUrl);
            return JsonDeserializer<TValue>(json);
        }

        protected TValue Delete()
        {
            this.httpMethod = HttpMethod.Delete;
            var json = Execute(null, RequestUrl);
            return JsonDeserializer<TValue>(json);
        }

        protected void Patch(Object patchContentObj)
        {
            this.httpMethod = new HttpMethod("PATCH");
            Execute(ConvertToJsonString(patchContentObj), this.RequestUrl);
        }

        protected string Execute(string httpContent, string url)
        {
            if (this.retryController == null)
            {
                return SendRequest(httpContent, url);
            }
            else
            {
                return (string)retryController.Retry(SendRequest, httpContent, url);
            }
        }

        protected ExportResult ExecuteV1(string httpContent, string url, string directoryPath)
        {
            if (this.retryController == null)
            {
                return SendRequestV1(httpContent, url, directoryPath);
            }
            else
            {
                return (ExportResult)retryController.Retry(SendRequestV1, httpContent, url, directoryPath);
            }
        }

        protected ExportResult ExecuteV2(string httpContent, string url, string directoryPath)
        {
            if (this.retryController == null)
            {
                return SendRequestV1Async(httpContent, url, directoryPath).Result;
            }
            else
            {
                return ((Task<ExportResult>)retryController.Retry(SendRequestV1Async, httpContent, url, directoryPath)).Result;
            }
        }

        protected string SendRequest(string body, string url)
        {
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                using (var request = new HttpRequestMessage(httpMethod, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshAccessToken());
                    request.Content = GetJsonContent(body);
                    SetRequestHeader(request);
                    using (var response = client.SendAsync(request).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            HandleError(response, url);
                        }
                    }
                }
            }
            return result;
        }

        protected ExportResult SendRequestV1(string body, string url, string directoryPath)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(60);
                using (var request = new HttpRequestMessage(httpMethod, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshAccessToken());
                    request.Content = GetJsonContent(body);
                    SetRequestHeader(request);
                    using (var response = client.SendAsync(request).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var exportFilePath = ExportFile(response.Content, GetExportPath(directoryPath, response.Content));
                            return ExportResult.CreateSuccessfulResult(url, exportFilePath);
                        }
                        else
                        {
                            //var errorResult = HandleError(response, url);
                            return ExportResult.CreateFailedResult(url, response.ReasonPhrase, response.StatusCode);
                        }
                    }
                }
            }
        }

        protected async Task<ExportResult> SendRequestV1Async(string body, string url, string directoryPath)
        {
            ExportResult exportResult = null;
            var timeOutHandler = new TimeoutHandler
            {
                DefaultTimeout = TimeSpan.FromSeconds(10),
                InnerHandler = new HttpClientHandler()
            };
            using (var cts = new CancellationTokenSource())
            {
                using (var client = new HttpClient(timeOutHandler))
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;// TimeSpan.FromMinutes(60);
                    using (var request = new HttpRequestMessage(httpMethod, url))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshAccessToken());
                        request.Content = GetJsonContent(body);
                        SetRequestHeader(request);
                        request.SetTimeout(TimeSpan.FromMinutes(30));
                        using (var response = await client.SendAsync(request, cts.Token))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var exportFilePath = ExportFile(response.Content, GetExportPath(directoryPath, response.Content));
                                exportResult = ExportResult.CreateSuccessfulResult(url, exportFilePath);
                            }
                            else
                            {
                                HandleError(response, url);
                                //return ExportResult.CreateFailedResult(url, response.ReasonPhrase, response.StatusCode);
                            }
                        }
                    }
                }
            }
            return exportResult;
        }

        private static string ExportFile(HttpContent responseContent, string exportFilePath)
        {
            try
            {
                logger.Info($"Start to export file: [{exportFilePath}]. ");
                using (var responseStream = responseContent.ReadAsStreamAsync().Result)
                {
                    using (FileStream fs = new FileStream(exportFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        byte[] buffer = new byte[64 * 1024];
                        int readLength = 0;
                        while ((readLength = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, readLength);
                        }
                    }
                }
                logger.Info($"Finish to export file: [{exportFilePath}]. ");
                return exportFilePath;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to export file: [{exportFilePath}]. Reason: {ex}. ");
                return string.Empty;
            }
        }

        private static string GetExportPath(string directoryPath, HttpContent responseContent)
        {
            try
            {
                logger.Info($"Start to get export file path.");
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                var contentDisposition = responseContent.Headers.GetValues("content-disposition").First();
                var nameTag = "filename=";
                var exportFileName = contentDisposition.Substring(contentDisposition.IndexOf(nameTag) + nameTag.Length);
                if (string.IsNullOrEmpty(exportFileName) || !exportFileName.StartsWith("export") || !exportFileName.EndsWith(".zip"))
                    exportFileName = $"export-{(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks) / 10000}.zip";
                logger.Info($"Finish to get export file path. Directory: [{directoryPath}]. FileName: [{exportFileName}].");
                return Path.Combine(directoryPath, exportFileName);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get export file path, will use an generated path. Reason: {ex}. ");
                return Path.Combine(directoryPath, $"export-{(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks) / 10000}.zip");
            }
        }

        private void SetRequestHeader(HttpRequestMessage request)
        {
            if (this.RequestHeader == null) return;
            foreach (var header in this.RequestHeader)
            {
                request.Headers.Add(header.Key, header.Value);
                //request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private YammerApiErrorRoot HandleError(HttpResponseMessage response, string url)
        {
            YammerApiErrorRoot result = null;
            var errorString = response.Content?.ReadAsStringAsync().Result;
            LogError(errorString, response);
            if (!string.IsNullOrEmpty(errorString))
            {
                try
                {
                    result = new YammerApiErrorRoot(errorString);
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while invoke Yammer API. Error message : {e.Message}. API: {url}.");
                }
            }
            result = result ?? new YammerApiErrorRoot { Error = new YammerApiError() { Code = "Unknown", Message = errorString } };

            throw new YammerAPIException(response, result, this.GetType().Name);
        }

        private void LogError(string errorString, HttpResponseMessage response)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Error occurred while invoke yammer API {this.GetType().Name}, Request url: {response.RequestMessage.Method} {response.RequestMessage.RequestUri}, StatusCode: ({(int)response.StatusCode}) {response.ReasonPhrase}");
            builder.AppendLine(errorString);
            builder.Append(response.ToString());
            logger.Warn(builder.ToString());
        }

        protected static T JsonDeserializer<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DateParseHandling = DateParseHandling.None });
        }

        private HttpContent GetJsonContent(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) return null;
            var content = new StringContent(jsonString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        private string ConvertToJsonString(Object obj)
        {
            return JsonConvert.SerializeObject(obj, GetJsonSerializerSettings());
        }

        protected virtual JsonSerializerSettings GetJsonSerializerSettings()
        {
            IContractResolver contractResolver = new CamelCasePropertyNamesContractResolver();//to camel case
            if (this.IncludePropertiesName != null && this.IncludePropertiesName.FirstOrDefault() != null)
            {
                contractResolver = new DynamicContractResolver(this.IncludePropertiesName);
            }

            return new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
            };
        }

        class DynamicContractResolver : CamelCasePropertyNamesContractResolver
        {
            private readonly HashSet<string> includeProperties;

            public DynamicContractResolver(IEnumerable<string> includePropertiesName)
            {
                includeProperties = new HashSet<string>(includePropertiesName, StringComparer.OrdinalIgnoreCase);
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
                return properties.Where(p => includeProperties.Contains(p.PropertyName) || includeProperties.Contains(p.UnderlyingName)).ToList();
            }

            //public override JsonContract ResolveContract(Type type)
            //{
            //    var c = base.ResolveContract(type);
            //    return c;
            //}
        }
    }
}