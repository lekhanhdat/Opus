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
    using System;
    using Newtonsoft.Json;

    public class ListGroupsObj : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public Group[] Value { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Group : EntityBase
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }
        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("classification")]
        public string Classification { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
        public string Description
        {
            get { return string.IsNullOrEmpty(this.mDescription) ? null : this.mDescription; }
            set { this.mDescription = value; }
        }

        [JsonIgnore]
        private string mDescription;

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("groupTypes")]
        public string[] GroupTypes { get; set; }

        [JsonProperty("membershipRule")]
        public string MembershipRule { get; set; }

        [JsonProperty("membershipRuleProcessingState")]
        public string MembershipRuleProcessingState { get; set; } // Possible values are On or Paused.

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("mailEnabled")]
        public object MailEnabled { get; set; }

        [JsonProperty("mailNickname")]
        public string MailNickname { get; set; }

        [JsonProperty("onPremisesLastSyncDateTime")]
        public object OnPremisesLastSyncDateTime { get; set; }

        [JsonProperty("onPremisesSecurityIdentifier")]
        public string OnPremisesSecurityIdentifier { get; set; }

        [JsonProperty("onPremisesSyncEnabled")]
        public object OnPremisesSyncEnabled { get; set; }

        [JsonProperty("proxyAddresses")]
        public string[] ProxyAddresses { get; set; }

        [JsonProperty("renewedDateTime")]
        public string RenewedDateTime { get; set; }

        [JsonProperty("resourceProvisioningOptions")]
        public string[] ResourceProvisioningOptions { get; set; }

        [JsonProperty("securityEnabled")]
        public object SecurityEnabled { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("creationOptions")]
        public string[] CreationOptions { get; set; }
        [JsonProperty(PropertyName = "extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType")]
        public string EducationObjectType { get; set; }
        [JsonProperty("preferredDataLocation")]
        public string PreferredDataLocation { get; set; }

        [JsonProperty("owners@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
        public string[] OwnersOdata { get; set; }
        [JsonProperty("members@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
        public string[] MembersOdata { get; set; }
    }
}