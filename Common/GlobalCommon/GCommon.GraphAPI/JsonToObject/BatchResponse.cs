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
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class BatchResponseObj
    {
        [JsonProperty("responses")]
        public ResponseItem[] Responses { get; set; }
    }
    public class ResponseItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("headers")]
        public Dictionary<String, String> Headers { get; set; }

        [JsonProperty("body")]
        public Object Body { get; set; }

        public bool IsSuccessStatusCode { get { return (Int32)Status >= 200 && (Int32)Status < 300; } }

        public T ToObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(Body.ToString());
        }
        public GraphAPIException ToGraphAPIExceptionObj()
        {
            return new GraphAPIException(this.Status, this.ToObject<GraphApiErrorRoot>());
        }
    }
}