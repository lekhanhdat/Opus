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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AvePoint.RA.Api.Web;

namespace AvePoint.RA.Web
{
    public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
    { 
        private string _callbackQueryParamter;
        private const string POST_REQUEST_TYPE = "POST";

        public JsonpMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(DefaultMediaType);
            SupportedMediaTypes.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

            MediaTypeMappings.Add(new UriPathExtensionMapping("jsonp", DefaultMediaType));
        }

        public string CallbackQueryParameter
        {
            get { return _callbackQueryParamter ?? "callback"; }
            set { _callbackQueryParamter = value; }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            string callback;
            var objs = value as HttpError;
            if (IsJsonpRequest(out callback))
            {
                return Task.Factory.StartNew(() =>
                {
                    var writer = new StreamWriter(writeStream);
                    writer.Write(callback + "(");
                    writer.Flush();
                    base.WriteToStreamAsync(type, value, writeStream, content, transportContext).Wait();
                    writer.Write(")");
                    writer.Flush();
                });
            }
            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }

        private bool IsJsonpRequest(out string callback)
        {
            callback = null;
            if (HttpContextHelper.Current == null)
            {
                return false;
            }
            callback = HttpContextHelper.Current.Request.HttpMethod.Equals(POST_REQUEST_TYPE) ? HttpContextHelper.Current.Request.Form[CallbackQueryParameter] : HttpContextHelper.Current.Request.QueryString[CallbackQueryParameter];
            //switch (HttpContext.Current.Request.HttpMethod)
            //{
            //    case "POST":
            //        callback = HttpContext.Current.Request.Form[CallbackQueryParameter];
            //        break;
            //    default:
            //        callback = HttpContext.Current.Request.QueryString[CallbackQueryParameter];
            //        break;
            //}
            return !string.IsNullOrEmpty(callback);
        }
    }
}