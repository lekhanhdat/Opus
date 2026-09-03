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

    public class UpdatePlannerTaskObj
    {
        [JsonProperty("bucketId")]
        public string BucketId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }

        //[JsonProperty("assigneePriority")]
        //public string AssigneePriority { get; set; }

        [JsonProperty("percentComplete")]
        public int PercentComplete { get; set; }

        [JsonProperty("startDateTime")]
        public string StartDateTime { get; set; }

        [JsonProperty("dueDateTime")]
        public string DueDateTime { get; set; }

        [JsonProperty("conversationThreadId")]
        public string ConversationThreadId { get; set; }

        [JsonProperty("assignments")]
        public Dictionary<string, UTAssignmentValue> Assignments { get; set; }

        [JsonProperty("appliedCategories")]
        public Dictionary<string, bool> AppliedCategories { get; set; }

        [JsonProperty("priority", NullValueHandling = NullValueHandling.Ignore)]
        public int? Priority { get; set; }
    }

    public class UTAssignmentValue
    {
        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UTAppliedCategories
    {
        [JsonProperty("category1")]
        public bool Category1 { get; set; }

        [JsonProperty("category2")]
        public bool Category2 { get; set; }

        [JsonProperty("category3")]
        public bool Category3 { get; set; }

        [JsonProperty("category4")]
        public bool Category4 { get; set; }

        [JsonProperty("category5")]
        public bool Category5 { get; set; }

        [JsonProperty("category6")]
        public bool Category6 { get; set; }

        [JsonProperty("category7")]
        public bool Category7 { get; set; }

        [JsonProperty("category8")]
        public bool Category8 { get; set; }

        [JsonProperty("category9")]
        public bool Category9 { get; set; }

        [JsonProperty("category10")]
        public bool Category10 { get; set; }

        [JsonProperty("category11")]
        public bool Category11 { get; set; }

        [JsonProperty("category12")]
        public bool Category12 { get; set; }

        [JsonProperty("category13")]
        public bool Category13 { get; set; }

        [JsonProperty("category14")]
        public bool Category14 { get; set; }

        [JsonProperty("category15")]
        public bool Category15 { get; set; }

        [JsonProperty("category16")]
        public bool Category16 { get; set; }

        [JsonProperty("category17")]
        public bool Category17 { get; set; }

        [JsonProperty("category18")]
        public bool Category18 { get; set; }

        [JsonProperty("category19")]
        public bool Category19 { get; set; }

        [JsonProperty("category20")]
        public bool Category20 { get; set; }

        [JsonProperty("category21")]
        public bool Category21 { get; set; }

        [JsonProperty("category22")]
        public bool Category22 { get; set; }

        [JsonProperty("category23")]
        public bool Category23 { get; set; }

        [JsonProperty("category24")]
        public bool Category24 { get; set; }

        [JsonProperty("category25")]
        public bool Category25 { get; set; }
    }
}