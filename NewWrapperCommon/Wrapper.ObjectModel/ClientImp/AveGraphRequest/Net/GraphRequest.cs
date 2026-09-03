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
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    public class GraphRequest
    {
        private IWebProxy proxy;
        public GraphRequest(IWebProxy proxy)
        {
            this.proxy = proxy;
        }

        private HttpClient GenerateHttpClient(RequestParameters parameter)
        {
            var handler = new WebRequestHandler();
            if (proxy != null)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            var client = new HttpClient(handler, true);

            if (parameter.AcceptTypes != null)
            {
                foreach (var item in parameter.AcceptTypes)
                {
                    client.DefaultRequestHeaders.Accept.Add(item);
                }
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", parameter.AccessToken);

            if (parameter.Header != null && parameter.Header.Count > 0)
            {
                foreach (var item in parameter.Header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            return client;
        }

        private async Task<T> HandleJsonResponse<T>(HttpResponseMessage response, RequestParameters parameter)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            if (string.Equals("application/json", response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase))
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            throw new GraphHttpException(
                response.RequestMessage.RequestUri.ToString(),
                response.StatusCode,
                response.ReasonPhrase,
                responseString);
        }


        public async Task<T> PatchJsonAsync<T>(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                var method = new HttpMethod("PATCH");
                var requestMessgae = new HttpRequestMessage(method, parameter.RequestUri);
                if (parameter.Content != null)
                {
                    requestMessgae.Content = parameter.Content.CreateHttpContent();
                }

                using (var response = await client.SendAsync(requestMessgae))
                {
                    return await HandleJsonResponse<T>(response, parameter);
                }
            }
        }

        public async Task<T> PutAsync<T>(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                HttpContent content = null;

                if (parameter.Content != null)
                {
                    content = parameter.Content.CreateHttpContent();
                }

                using (var response = await client.PutAsync(parameter.RequestUri, content))
                {
                    return await HandleJsonResponse<T>(response, parameter);
                }
            }
        }
        

        public async Task<T> PostAsync<T>(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                HttpContent content = null;

                if (parameter.Content != null)
                {
                    content = parameter.Content.CreateHttpContent();
                }

                using (var response = await client.PostAsync(parameter.RequestUri, content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await HandleJsonResponse<T>(response, parameter);
                    }
                    throw new Exception();
                }
            }
        }

        public void PostRequest(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                HttpContent content = null;

                if (parameter.Content != null)
                {
                    content = parameter.Content.CreateHttpContent();
                }
                using (var response = client.PostAsync(parameter.RequestUri, content).Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var jsonResponse = response.Content.ReadAsStringAsync().Result;

                        throw new GraphHttpException(
                            parameter.RequestUri,
                            response.StatusCode,
                            response.ReasonPhrase,
                            jsonResponse);
                    }
                }
            }
        }

        public void DeleteRequest(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                using (var response = client.DeleteAsync(parameter.RequestUri).Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var jsonResponse = response.Content.ReadAsStringAsync().Result;

                        throw new GraphHttpException(
                             parameter.RequestUri,
                            response.StatusCode,
                            response.ReasonPhrase,
                            jsonResponse);
                    }
                }
            }
        }

        public async Task<T> GetAsync<T>(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                using (var response = await client.GetAsync(parameter.RequestUri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await HandleJsonResponse<T>(response, parameter);
                    }
                    throw new Exception();
                }
            }
        }

        public async Task<byte[]> GetByteArrayAsync(RequestParameters parameter)
        {
            using (var client = GenerateHttpClient(parameter))
            {
                using (var response = await client.GetAsync(parameter.RequestUri))
                {
                    if (response.IsSuccessStatusCode &&
                        "attachment".Equals(
                            response.Content.Headers.ContentDisposition.DispositionType,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    throw new GraphHttpException(
                        parameter.RequestUri,
                        response.StatusCode,
                        response.ReasonPhrase,
                        jsonResponse);
                }
            }
        }
    }
}
