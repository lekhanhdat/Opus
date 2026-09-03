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

using System.Net;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    class GraphException : System.Exception
    {
        public HttpStatusCode StatusCode { get; protected set; }

        public GraphException(int statusCode, string message) : base(message)
        {
            StatusCode = (HttpStatusCode)statusCode;
        }

        public GraphException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }


    class GraphHttpException : GraphException
    {
        public string Url { get; private set; }
        public string StatusDescription { get; private set; }
        public string Response { get; private set; }

        public GraphHttpException(string url, HttpStatusCode statusCode, string statusDescription, string response)
            : base(statusCode, string.Empty)
        {
            Url = url;
            StatusDescription = StatusDescription;
            Response = response;
        }
    }
}
