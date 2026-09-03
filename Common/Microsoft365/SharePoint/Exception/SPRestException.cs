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

namespace Microsoft365.SharePoint.Rest
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;

    [Serializable]
    public class SPRestException : Exception
    {
        private string error;

        /// <summary>
        /// Construct exception for SharePoint rest API call.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="reasonPhrase"></param>
        /// <param name="error"></param>
        public SPRestException(string endpoint,HttpStatusCode statusCode, string reasonPhrase, string error, string headers = null)
        {
            this.EndPoint = endpoint ?? throw new ArgumentNullException();
            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhrase;
            this.ResponseHeaders = headers;
            this.error = error;
            this.jObject = ConvertToJ(error);
        }

        private static JObject ConvertToJ(string error)
        {
            if (string.IsNullOrEmpty(error)) return null;
            try
            {
                return JObject.Parse(error);
            }
            catch
            {
                return null;
            }
        }
        protected SPRestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private JObject jObject;

        public HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        public string ResponseHeaders { get;private set; }
        public string EndPoint { get; private set; }

        public string ErrorCode => this.jObject?["odata.error"]?["code"]?.ToString();
        public string ErrorMessage => this.jObject?["odata.error"]?["message"]?["value"]?.ToString();
        public string ErrorLanguage => this.jObject?["odata.error"]?["message"]?["lang"]?.ToString();

        public SPRestErrorCode ErrorCodeType => this.ToRestErrorCode();

        public override string Message => this.ErrorMessage;

        public string ErrorDetails => this.FormatError();
        
        private string FormatError()
        {
            var buffer = new StringBuilder();
            buffer.AppendLine(this.EndPoint);
            buffer.AppendLine($"{(int)this.StatusCode} {this.ReasonPhrase}");
            buffer.AppendLine(ResponseHeaders);
            buffer.Append(this.error);
            return buffer.ToString();
        }

        

    }
}
