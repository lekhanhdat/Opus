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
namespace AvePoint.GCommon.GraphAPI
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;

    public abstract class MicrosoftGraphApiBase<TValue>
    {
        internal const string Version_V1 = "v1.0";
        internal const string Version_Beta = "beta";
        internal const string Version_Edu = "edu";

        private string apiUrlBase = string.Empty;
        protected string apiUrlV1 = string.Empty;
        protected string apiUrlBeta = string.Empty;
        protected string apiUrlEdu = string.Empty;
        protected Func<string> getAccessToken;
        protected IRetryable retryController;
        protected HttpMethod httpMethod = HttpMethod.Get;
        protected abstract string RequestUrl { get; }
        internal IDictionary<string, string> RequestHeader;
        internal IDictionary<string, string> ResponseHeader;

        public QueryParameters QueryParameters { get; } = new QueryParameters();
        protected virtual IEnumerable<string> IncludePropertiesName { get; }


        protected string FullUrl
        {
            get
            {
                if (this.QueryParameters.IsEmpty) return this.RequestUrl;
                var url = this.RequestUrl;
                var query = BuildQuery();
                if (string.IsNullOrEmpty(query)) return url;
                return $"{url}?{query}";
            }
        }

        private string BuildQuery()
        {
            var query = new StringBuilder();
            if (!string.IsNullOrEmpty(QueryParameters.ModelString))
            {
                query.Append($"model={QueryParameters.ModelString}&");
            }
            if (!string.IsNullOrEmpty(QueryParameters.TopString))
            {
                query.Append($"$top={QueryParameters.TopString}&");
            }
            if (!string.IsNullOrEmpty(QueryParameters.FilterString))
            {
                query.Append($"$filter={QueryParameters.FilterString}&");
            }
            if (QueryParameters.Selector != null && QueryParameters.Selector.FirstOrDefault() != null)
            {
                query.Append($"$select={string.Join(",", QueryParameters.Selector)}&");
            }
            if (!string.IsNullOrEmpty(QueryParameters.OrderByString))
            {
                query.Append($"$orderBy={QueryParameters.OrderByString}&");
            }
            if (!string.IsNullOrEmpty(QueryParameters.SearchString))
            {
                query.Append($"$search={QueryParameters.SearchString}&");
            }
            if (!string.IsNullOrEmpty(QueryParameters.ExpandString))
            {
                query.Append($"$expand={QueryParameters.ExpandString}&");
            }
            return query.ToString().TrimEnd(new char[] { '&' });
        }

        public MicrosoftGraphApiBase(string baseUrl, Func<string> getToken)
        {
            this.apiUrlBase = baseUrl.TrimEnd('/');
            this.apiUrlV1 = $"{this.apiUrlBase}/{Version_V1}";
            this.apiUrlBeta = $"{this.apiUrlBase}/{Version_Beta}";
            this.apiUrlEdu = $"{this.apiUrlBase}/{Version_Edu}";
            this.getAccessToken = getToken;
        }

        public MicrosoftGraphApiBase(string baseUrl, Func<string> getToken, IRetryable retryable) : this(baseUrl, getToken)
        {
            this.retryController = retryable;
        }

        public MicrosoftGraphApiBase(Func<string> getToken, IRetryable retryable)
        {
            retryController = retryable;
            getAccessToken = getToken;
        }

        public abstract TValue GetApiResult();

        protected TValue Get()
        {
            this.httpMethod = HttpMethod.Get;
            var json = Execute(null, this.FullUrl);
            return JsonDeserializer<TValue>(json);
        }

        protected TValue Post(Object postContentDto)
        {
            this.httpMethod = HttpMethod.Post;
            var json = Execute(ConvertToJsonString(postContentDto), this.FullUrl);
            return JsonDeserializer<TValue>(json);
        }
        protected TValue Put(Object content)
        {
            this.httpMethod = HttpMethod.Put;
            var json = Execute(ConvertToJsonString(content), this.FullUrl);
            return JsonDeserializer<TValue>(json);
        }

        /// <summary>
        /// 大部分Delete Request\Response都没有body，暂时使用void()
        /// </summary>
        protected TValue Delete()
        {
            this.httpMethod = HttpMethod.Delete;
            var json = Execute(null, FullUrl);
            return JsonDeserializer<TValue>(json);
        }

        /// <summary>
        /// 大部分Patch Response都没有body，暂时使用void
        /// </summary>
        /// <param name="patchContentObj"></param>
        protected void Patch(Object patchContentObj)
        {
            this.httpMethod = new HttpMethod("PATCH");
            Execute(ConvertToJsonString(patchContentObj), this.FullUrl);
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

        protected byte[] ExecuteV1(string httpContent, string url)
        {
            if (this.retryController == null)
            {
                return SendRequestV1(httpContent, url);
            }
            else
            {
                return (byte[])retryController.Retry(SendRequestV1, httpContent, url);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        protected string SendRequest(string body, string url)
        {
            var result = string.Empty;

            using (var request = new HttpRequestMessage(httpMethod, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getAccessToken());
                request.Content = GetJsonContent(body);//Do no need to dispose httpcontent. It will be disposed when request is disposed.
                                                       //https://github.com/dotnet/corefx/blob/6d7fca5aecc135b97aeb3f78938a6afee55b1b5d/src/System.Net.Http/src/System/Net/Http/HttpClient.cs#L500
                SetRequestHeader(request);
                using (var response = HttpClientHelper.httpClient.SendAsync(request).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                        //Logger.Info($"Success request: {httpMethod} {url}");
                        //Logger.Info($"FIND_ERROR : {result}");
                    }
                    else
                    {
                        HandleError(response, url, body);
                    }
                }
            }
            return result;
        }

        public readonly long BlockSize = 5 * 1024 * 1024;


        protected byte[] SendRequestV1(string body, string url)
        {
            var result = new byte[BlockSize];

            using (var request = new HttpRequestMessage(httpMethod, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getAccessToken());
                request.Content = GetJsonContent(body);//Do no need to dispose httpcontent. It will be disposed when request is disposed.
                                                       //https://github.com/dotnet/corefx/blob/6d7fca5aecc135b97aeb3f78938a6afee55b1b5d/src/System.Net.Http/src/System/Net/Http/HttpClient.cs#L500
                SetRequestHeader(request);
                using (var response = HttpClientHelper.httpClient.SendAsync(request).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result = response.Content.ReadAsByteArrayAsync().Result;
                        //Logger.Info($"Success request: {httpMethod} {url}");
                        //Logger.Info($"FIND_ERROR : {result}");
                    }
                    else
                    {
                        HandleError(response, url, body);
                    }
                }
            }
            return result ?? null;
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

        private GraphApiError HandleError(HttpResponseMessage response, string url, string requestBody)
        {
            GraphApiErrorRoot result = null;
            var errorString = response.Content?.ReadAsStringAsync().Result;
            LogError(errorString, response, requestBody);
            if (!string.IsNullOrEmpty(errorString))
            {
                try
                {
                    result = new GraphApiErrorRoot(errorString);
                }
                catch(Exception e)
                {
                    Logger.Warn($"Graph api error {e.ToString()}");
                }
            }
            result = result ?? new GraphApiErrorRoot { Error = new GraphApiError() { Code = "Unknown", Message = errorString } };
            throw new GraphAPIException(response, result, this.GetType().Name);
        }

        private void LogError(string errorString, HttpResponseMessage response, string requestBody)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Error occurred while invoke graph API {this.GetType().Name}, Request url: {response.RequestMessage.Method} {response.RequestMessage.RequestUri} {requestBody}, StatusCode: ({(int)response.StatusCode}) {response.ReasonPhrase}");
            builder.AppendLine(errorString);
            builder.Append(response.ToString());
            Logger.Warn(builder.ToString());
        }

        protected static T JsonDeserializer<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DateParseHandling = DateParseHandling.None });
        }

        protected static string JsonSerializer<T>(T value)
        {
            return JsonConvert.SerializeObject(value);
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
        }
    }

    public class QueryParameters
    {
        internal bool IsEmpty { get; private set; } = true;

        public IEnumerable<string> Selector { get; private set; }
        public QueryParameters Select(params string[] param)
        {
            Selector = param;
            IsEmpty = false;
            return this;
        }

        public string FilterString { get; private set; }

        public QueryParameters Filter(string filter)
        {
            FilterString = filter;
            IsEmpty = false;
            return this;

        }

        public string SearchString { get; private set; }

        public QueryParameters Search(string search)
        {
            SearchString = search;
            IsEmpty = false;
            return this;
        }

        public string ModelString { get; private set; }

        public QueryParameters Model(string model)
        {
            ModelString = model;
            IsEmpty = false;
            return this;
        }

        public string TopString { get; private set; }

        public QueryParameters Top(int top)
        {
            TopString = top.ToString();
            IsEmpty = false;
            return this;
        }

        public string OrderByString { get; private set; }

        public QueryParameters OrderBy(string orderByString)
        {
            OrderByString = orderByString;
            IsEmpty = false;
            return this;
        }

        public string ExpandString { get; private set; }

        public QueryParameters Expand(string expandString)
        {
            ExpandString = expandString;
            IsEmpty = false;
            return this;
        }
    }
}