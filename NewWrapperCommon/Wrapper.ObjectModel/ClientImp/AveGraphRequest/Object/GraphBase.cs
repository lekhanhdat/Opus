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
namespace AvePoint.ObjectModel.AveGraphRequest
{
    using System.Net;
    using Office365.Api;
    using System.Net.Http.Headers;
    using Newtonsoft.Json.Linq;

    public abstract class GraphBase
    {
        protected ITokenProvider tokenProvider;
        protected GraphRequest request;

        public GraphBase(ITokenProvider tokenProvider, IWebProxy proxy)
        {
            this.tokenProvider = tokenProvider;
            request = new GraphRequest(proxy);
        }

        protected virtual RequestParameters GenerateRequestsParameters(string requestUri)
        {
            return new RequestParameters
            {
                AccessToken = tokenProvider.GetToken(null),
                AcceptTypes = new MediaTypeWithQualityHeaderValue[] { MediaTypeWithQualityHeaderValue.Parse("application/json") },
                RequestUri = requestUri,
            };
        }

        protected virtual RequestParameters GenerateStringContentRequestParameters(string requestUri, string content, string contentType)
        {
            var parameter = GenerateRequestsParameters(requestUri);
            parameter.Content = new StringContentRequest(content, contentType);
            return parameter;
        }

        protected virtual RequestParameters GenerateByteArrayContentRequestParameters(string requestUri, byte[] content, string contentType)
        {
            var parameter = GenerateRequestsParameters(requestUri);
            parameter.Content = new ByteArrayContentRequest(content, contentType);
            return parameter;
        }


        protected JObject GetObjectInfo(string requestUri)
        {
            var parameter = GenerateRequestsParameters(requestUri);
            return request.GetAsync<JObject>(parameter).Result;
        }

        protected void DeleteObject(string requestUri)
        {
            var parameter = GenerateRequestsParameters(requestUri);

            request.DeleteRequest(parameter);
        }
    }
}
