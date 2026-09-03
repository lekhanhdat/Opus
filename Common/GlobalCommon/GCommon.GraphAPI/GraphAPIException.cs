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
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    [System.Serializable]
    public class GraphAPIException : System.Exception
    {
        public GraphAPIException(HttpStatusCode code, GraphApiErrorRoot errorRoot) : base(FormatMessage(code, errorRoot.Error))
        {
            this.Error = errorRoot.Error;
            this.HttpStatusCode = code;
        }

        public GraphAPIException(HttpResponseMessage response, GraphApiErrorRoot errorRoot) : this(response.StatusCode, errorRoot)
        {
            this.RetryAfter = response.Headers.RetryAfter;
        }
        public GraphAPIException(HttpResponseMessage response, GraphApiErrorRoot errorRoot, string tag) : this(response, errorRoot)
        {
            this.Tag = tag;
        }
        protected GraphAPIException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public GraphApiError Error { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public RetryConditionHeaderValue RetryAfter { get; set; }
        public string Tag { get; set; } = string.Empty;
        private static string FormatMessage(HttpStatusCode code, GraphApiError error)
        {
            return $"{error?.Message}, status code: {code}, internal error code:{error?.Code}";
        }
    }

}