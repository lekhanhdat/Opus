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
namespace ExchangeUtility.Graph
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class YammerApiErrorRoot
    {
        public YammerApiErrorRoot(string error)
        {
            this.Error = JsonConvert.DeserializeObject<YammerApiErrorRoot>(error)?.Error;
        }
        public YammerApiErrorRoot()
        {
        }

        [JsonProperty(PropertyName = "error")]
        public YammerApiError Error
        {
            get;
            set;
        }

    }

    public class YammerApiError 
    {
        [DefaultValue("")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "id", Required = Required.Default)]
        public string Id { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "code")]
        public string Code
        {
            get;
            set;
        }
        [DefaultValue("")]
        [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "innerError", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public YammerApiError InnerError
        {
            get;
            set;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class YammerApiInnerError
    {
        [JsonProperty(PropertyName = "request-id")]
        public string RequestId
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "date")]
        public string Date
        {
            get;
            set;
        }
    }
}