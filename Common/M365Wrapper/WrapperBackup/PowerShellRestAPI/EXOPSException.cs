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
using Newtonsoft.Json;

namespace ExchangeUtility.Graph.PowerShellRestAPI
{
    public class EXOPSException
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
    public class Detail
    {

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class Internalexception
    {

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("stacktrace")]
        public string Stacktrace { get; set; }
    }

    public class Innererror
    {

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("stacktrace")]
        public string Stacktrace { get; set; }

        [JsonProperty("internalexception")]
        public Internalexception Internalexception { get; set; }
    }

    public class Error
    {

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public Detail[] Details { get; set; }

        [JsonProperty("innererror")]
        public Innererror Innererror { get; set; }
    }

}
