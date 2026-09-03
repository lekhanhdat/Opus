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
using System.IO;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    public class RequestParameters
    {
        public string AccessToken { get; set; }

        public string RequestUri { get; set; }

        public MediaTypeWithQualityHeaderValue[] AcceptTypes { get; set; }

        public Dictionary<string, string> Header { get; set; }

        public IRequestContent Content { get; set; }
    }


    public interface IRequestContent
    {
        HttpContent CreateHttpContent();
    }

    class StreamContentRequest : IRequestContent
    {
        private Stream content;
        private string contentType;

        public StreamContentRequest(Stream content, string contentType)
        {
            this.content = content;
            this.contentType = contentType;
        }

        public HttpContent CreateHttpContent()
        {
            var streamContent = new StreamContent(content);

            streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);

            return streamContent;
        }
    }

    class StringContentRequest : IRequestContent
    {
        protected string content;
        protected string contentType;

        public StringContentRequest(string content, string contentType)
        {
            this.content = content;
            this.contentType = contentType;
        }

        public virtual HttpContent CreateHttpContent()
        {
            var streamContent = new StringContent(content, Encoding.UTF8, contentType);

            return streamContent;
        }
    }

    class ByteArrayContentRequest : IRequestContent
    {
        protected byte[] content;
        protected string contentType;
        protected long? contentLength;
        protected ContentRangeHeaderValue contentRange;

        public ByteArrayContentRequest(byte[] content, string contentType)
        {
            this.content = content;
            this.contentType = contentType;
        }

        public ByteArrayContentRequest(byte[] content, string contentType, long contentLength, ContentRangeHeaderValue contentRange)
            :this(content,contentType)
        {
            this.contentLength = contentLength;
            this.contentRange = contentRange;
        }

        public virtual HttpContent CreateHttpContent()
        {
            var streamContent = new ByteArrayContent(content);
            if(!string.IsNullOrEmpty(contentType))
            {
                streamContent.Headers.ContentType= MediaTypeHeaderValue.Parse(contentType);
            }

            if (contentLength != null)
            {
                streamContent.Headers.ContentLength = contentLength;
            }
            if (contentRange != null)
            {
                streamContent.Headers.ContentRange = contentRange;
            }
            return streamContent;
        }
    }
}
