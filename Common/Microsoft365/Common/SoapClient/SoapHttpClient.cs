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
namespace Microsoft365.Common.SoapClient
{
    using Microsoft365.Common.HttpUtil;
    using Microsoft365.Common.Utility;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    public class SoapHttpClient :IDisposable
    {
        protected ISoapEnvelopeSerializer SoapEnvelopeSerializer { get; set; }
        protected Func<string> GetCookie { get; set; }
        protected Uri ServiceUri { get; set; }

        public SoapHttpClient(Uri serviceUri,Func<string> getCookie)
        {
            GetCookie = getCookie;
            ServiceUri = serviceUri;
            SoapEnvelopeSerializer=new SoapEnvelopeSerializer();
        }

        public TResponse SendRequest<TRequest, TResponse>(TRequest request)
            where TRequest : class, ISoapHttpRequest 
            where TResponse : class
        {
            using (var httpClient = RestClientFactory.CreateSharePointRestClient("SharePoint"))
            {
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Post
                };                
                message.Headers.Add("Authorization", GetCookie());
                message.Headers.Add("SOAPAction", request.SoapAction);
                message.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                message.Headers.Add("Expect", "100-continue");
                message.RequestUri = ServiceUri;
                var content = new StringContent(BuildContent(request), Encoding.UTF8, "text/xml");
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml; charset=utf-8");
                message.Content = content;
                var response = httpClient.SendAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new HttpRequestException(response.ToString(), null, response.StatusCode);
                }
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var responseSoapEnvelope = SoapEnvelopeSerializer.ToSoapEnvelope(responseContent);
                responseSoapEnvelope.ThrowIfFaulted();
                var result = responseSoapEnvelope.GetBody<TResponse>();
                if (result == default)
                {
                    throw new ArgumentNullException("resultObj");
                }
                return result;
            }
        }

        private string BuildContent<TRequest>(TRequest request) where TRequest:class
        {
            return SoapEnvelopeSerializer.FromSoapEnvelope(SoapEnvelopeBuilder.Create().WithBody(request));
        }

        public void Dispose()
        {
            ServiceUri = null;
            GetCookie = null;
        }
    }
}
