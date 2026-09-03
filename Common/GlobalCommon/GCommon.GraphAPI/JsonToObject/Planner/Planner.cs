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

    #region List-Object

    public class ListPlannerPlansObj : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public int OdataCount { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonProperty("value")]
        public GraphPlannerPlan[] Value { get; set; }
    }

    public class ListPlannerBucketsObj : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public int OdataCount { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonProperty("value")]
        public GraphPlannerBucket[] Value { get; set; }
    }

    public class ListPlannerTasksObj : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public int OdataCount { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonProperty("value")]
        public GraphPlannerTask[] Value { get; set; }
    }

    public class ListPlannerTaskCommentsObj : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("lastDeliveredDateTime")]
        public string LastDeliveredDateTime { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("posts@odata.context")]
        public string PostsOdataContext { get; set; }

        [JsonProperty("posts")]
        public List<GraphTaskComment> Posts { get; set; }
    }
    #endregion


    #region Sub-Object
    public class GPLastModifiedBy : EntityBase
    {
        [JsonProperty("user")]
        public GPUser User { get; set; }
    }
    public class GPCompletedBy : EntityBase
    {
        [JsonProperty("user")]
        public GPUser User { get; set; }
    }
    public class GPCreatedBy : EntityBase
    {

        [JsonProperty("user")]
        public GPUser User { get; set; }

        [JsonProperty("application")]
        public GPApplication Application { get; set; }
    }
    public class GPUser : EntityBase
    {

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }
    }
    public class GPApplication : EntityBase
    {

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }
    }
    public class GPEmailAddress : EntityBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
    #endregion
}
