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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeUtility.MicrosoftGraph
{
    public abstract class MicrosoftGraphApiBase
    {
        protected string apiUrlBase = string.Empty;
        protected string accessToken = string.Empty;
        protected HttpMethod httpMethod = HttpMethod.Get;
        protected abstract string RequestUrl { get; }
        private HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(3) };
        public MicrosoftGraphApiBase(string baseUrl, string token)
        {
            this.apiUrlBase = baseUrl.TrimEnd('/');
            this.accessToken = token;
        }

        public abstract object GetApiResult();

        public string GetInfoHelper()
        {
            string result = string.Empty;
            //using (var client = new HttpClient())
            //{
                //client.Timeout = TimeSpan.FromMinutes(3);
                using (var request = new HttpRequestMessage(httpMethod, this.RequestUrl))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var respose = client.SendAsync(request))
                    {
                        if (respose.Result.StatusCode == HttpStatusCode.OK)
                        {
                            result = respose.Result.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            //}
            return result;
        }

        public void JsonDeserializer<T>(string value, out T result)
        {
            result = default(T);
            result = JsonConvert.DeserializeObject<T>(value);
        }
    }
}
