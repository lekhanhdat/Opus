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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    public enum AccountType
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Group
    }

    public class Accounts
    {
        [JsonProperty("odata.metadata")]
        public string OdataMetadata { get; set; }

        [JsonProperty("value")]
        public List<Account> Value { get; set; }

        [JsonProperty("odata.nextLink")]
        public string OdataNextLink { get; set; }

        public string Skiptoken
        {
            get
            {
                string t = string.Empty;
                if (!string.IsNullOrEmpty(OdataNextLink))
                {
                    t = OdataNextLink.Substring(OdataNextLink.LastIndexOf("$skiptoken=") + 11);
                }
                return t;
            }
        }
    }

    public class Account
    {
        [JsonProperty("UserId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("InviteType", NullValueHandling = NullValueHandling.Ignore)]
        public string InviteType { get; set; }

        [JsonProperty("DisplayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("UserPrincipalName", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPrincipalName { get; set; }

        //[JsonProperty("mail", NullValueHandling = NullValueHandling.Ignore)]
        //public string mail { get; set; }

        //[JsonProperty("tenantId", NullValueHandling = NullValueHandling.Ignore)]
        //public string tenantId { get; set; }

    }


    public class SignInName
    {
        [JsonProperty("type")]
        public string SignInNameType { get; set; }

        [JsonProperty("value")]
        public string SignInNameValue { get; set; }
    }

    public class PasswordProfile
    {
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty("forceChangePasswordNextLogin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ForceChangePasswordNextLogin { get; set; }

        [JsonProperty("enforceChangePasswordPolicy", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnforceChangePasswordPolicy { get; set; }
    }

    public class ProvisionedPlan
    {
        [JsonProperty("capabilityStatus")]
        public string CapabilityStatus { get; set; }

        [JsonProperty("provisioningStatus")]
        public string ProvisioningStatus { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }
    }

    public class ProvisioningError
    {
        [JsonProperty("errorDetail")]
        public string errorDetail { get; set; }

        [JsonProperty("resolved")]
        public bool? IsResolved { get; set; }

        [JsonProperty("serviceInstance")]
        public string ServiceInstance { get; set; }

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }
    }

    public class AssignedLicense
    {
        [JsonProperty("disabledPlans")]
        public List<Guid> DisabledPlans { get; set; }

        [JsonProperty("skuId")]
        public Guid? SkuId { get; set; }
    }

    public class AssignedPlan
    {
        [JsonProperty("assignedTimestamp")]
        public DateTime? AssignedTimestamp { get; set; }

        [JsonProperty("capabilityStatus")]
        public string CapabilityStatus { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("servicePlanId")]
        public string ServicePlanId { get; set; }
    }
}
