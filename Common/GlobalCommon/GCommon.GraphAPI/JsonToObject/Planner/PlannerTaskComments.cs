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

    public class GraphTaskComment : EntityBase
    {
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("changeKey")]
        public string ChangeKey { get; set; }

        [JsonProperty("categories")]
        public object[] Categories { get; set; }

        [JsonProperty("receivedDateTime")]
        public string ReceivedDateTime { get; set; }

        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }

        [JsonProperty("body")]
        public GPTCBody Body { get; set; }

        [JsonProperty("from")]
        public GPTCFrom From { get; set; }

        [JsonProperty("sender")]
        public GPTCSender Sender { get; set; }
    }


    #region Sub-Object
    public class GPTCBody : EntityBase
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
    public class GPTCFrom : EntityBase
    {

        [JsonProperty("emailAddress")]
        public GPEmailAddress EmailAddress { get; set; }
    }
    public class GPTCSender : EntityBase
    {

        [JsonProperty("emailAddress")]
        public GPEmailAddress EmailAddress { get; set; }
    }
    #endregion
}