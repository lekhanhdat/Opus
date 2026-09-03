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
using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    public class ManualApprovalAADUserInfo
    {
        [JsonProperty("users")]
        public List<ManualApprovalAOPUserInfo> Users { get; set; }

        [JsonProperty("statusMsg")]
        public string StatusMsg { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class ManualApprovalAOPUserInfo
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("displayName_Lower")]
        public string DisplayName_Lower
        {
            get
            {
                return DisplayName?.ToLower();
            }
        }

        [JsonProperty("inviteType")]
        public AccountType InviteType { get; set; }

        [JsonProperty("rmUserId")]
        public int RMUserId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("surName")]
        public string SurName { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }
    }
}
