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
using System.Net.Http;

namespace CloudRecordDownloadManager.Utils.Http
{
    public class RequestOptions
    {
        public Uri url { get; set; }
        public string protocol { get; set; }
        public string host { get; set; }
        public string hostname { get; set; }
        public int family { get; set; }
        public int port { get; set; }
        public int defaultPort { get; set; }
        public string localAddress { get; set; }
        public string socketPath { get; set; }
        public HttpMethod method { get; set; } = HttpMethod.Get;
        public string path { get; set; }
        public string auth { get; set; }
        public string timeout { get; set; }
        public string contentType { get; set; }
        public string contentLength { get; set; }

        public Dictionary<string, string> headers { get; set; }
//        headers?: OutgoingHttpHeaders;
//        agent?: Agent | boolean;
//        _defaultAgent?: Agent;
//        timeout?: number;
    }
}