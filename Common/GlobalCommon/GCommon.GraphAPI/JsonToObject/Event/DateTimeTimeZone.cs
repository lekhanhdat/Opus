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

    public class DateTimeTimeZone
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeTimeZone"/> class.
        /// </summary>
        public DateTimeTimeZone() => ODataType = "microsoft.graph.dateTimeTimeZone";
        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "@odata.type", Required = Required.Default)]
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets dateTime.
        /// A single point of time in a combined date and time representation ({date}T{time}). For example, '2019-04-16T09:00:00'.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "dateTime", Required = Required.Default)]
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets timeZone.
        /// Represents a time zone, for example, 'Pacific Standard Time'. See below for possible values.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "timeZone", Required = Required.Default)]
        public string TimeZone { get; set; }
    }
}