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

    public class GraphPlannerTaskDetails : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("previewType")]
        public string PreviewType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("references")]
        public Dictionary<string, GPTDReferencesValue> References { get; set; }

        [JsonProperty("checklist")]
        public Dictionary<string, GPTDCheckListValue> Checklist { get; set; }
    }

    #region Sub-Object
    public class GPTDReferencesValue : EntityBase
    {

        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("previewPriority")]
        public string PreviewPriority { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("lastModifiedBy", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public GPLastModifiedBy LastModifiedBy { get; set; }
    }

    public class GPTDCheckListValue : EntityBase
    {

        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("isChecked")]
        public bool IsChecked { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("lastModifiedBy", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public GPLastModifiedBy LastModifiedBy { get; set; }
    }

    #endregion
}