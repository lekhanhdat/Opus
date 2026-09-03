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
using System.Net;
using System.Net.Http;
using System.Text;

namespace  AvePoint.Hybrid.Utility
{
    public static class ResponseHelper
    {
        public static string LogExceptionDetails(Exception exception)
        {
            var log = new StringBuilder(exception.ToString());
            if (exception.InnerException != null)
            {
                log.AppendLine(exception.InnerException.ToString());
            }

            return log.ToString();
        }

        //public static HttpResponseException CreateResponseException(HttpStatusCode status, string message)
        //{
        //    var response = new HttpResponseMessage(status);
        //    if (!string.IsNullOrEmpty(message))
        //    {
        //        response.Content = new StringContent(message);
        //    }

        //    return new HttpResponseException(response);
        //}

        //public static HttpResponseException CreateResponseException<T>(HttpStatusCode status, T message)
        //{
        //    var response = new HttpResponseMessage(status);
        //    if (message != null)
        //    {
        //        response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message));
        //    }

        //    return new HttpResponseException(response);
        //}

        //public static HttpResponseException CreateResponseException(HttpStatusCode status, Exception exception)
        //{
        //    var result = new StringBuilder();

        //    result.AppendLine(exception.ToString());
        //    result.AppendLine();

        //    Exception innerException = exception.InnerException;
        //    while (innerException != null)
        //    {
        //        result.AppendLine(innerException.ToString());
        //        result.AppendLine();
        //        innerException = innerException.InnerException;
        //    }

        //    return CreateResponseException(status, result.ToString());
        //}
    }
}
