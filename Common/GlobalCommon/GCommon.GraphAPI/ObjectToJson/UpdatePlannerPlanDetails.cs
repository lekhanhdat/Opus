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
    using System.Collections.Generic;

    public class UpdatePlannerPlanDetailsObj
    {
        //[JsonProperty("sharedWith")]
        //public Dictionary<string, bool> SharedWith { get; set; }

        [JsonProperty("categoryDescriptions")]
        public Dictionary<string,string> CategoryDescriptions { get; set; }
    }

    public class UPDCategoryDescriptions
    {
        [JsonProperty("category1")]
        public string Category1 { get; set; }

        [JsonProperty("category2")]
        public string Category2 { get; set; }

        [JsonProperty("category3")]
        public string Category3 { get; set; }

        [JsonProperty("category4")]
        public string Category4 { get; set; }

        [JsonProperty("category5")]
        public string Category5 { get; set; }

        [JsonProperty("category6")]
        public string Category6 { get; set; }
    }
}