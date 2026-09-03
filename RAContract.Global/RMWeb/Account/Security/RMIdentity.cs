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
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.RMWeb.Account.Security
{
    public class RMIdentity
    {
        public RMAuthenticationTypes AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string RegisterEmail { get; set; }

        public string TenantGroupId { get; set; }

        public RMAccountType AccountType { get; set; }

        public string AccountId { get; set; }

        //public string ObjectSID { get; set; }

        public List<KeyValuePair<string, string>> Claims { get; set; }
        public string Url { get; set; }

        public Guid SessionId { get; set; }
        public long SessionFrom { get; set; }
        public DateTime ExpiredTime { get; set; }
        //Unit is minutes
        public int SessionOut { get; set; }
        public string GPermission { get; set; }
        public bool ForceLogined { get; set; }
        public bool ForcedLogout { get; set; }
        public bool DisableAVA { get; set; }
        public bool ExistAVAUser { get; set; }
        public string Company { get; set; }
        public string AccountNumber { get; set; }
        public string AccessToken { get; set; }

        public string PartnerUser { get; set; }
        public string PartnerOwner { get; set; }

        public bool IsEnableMultiGeo { get; set; }

        public string DataCenter { get; set; }
    }

    public class RMClaimTypes
    {
        public const string AuthByRM = "http://www.rm.com/claims/authbyrm";
        public const string Role = "http://www.rm.com/claims/role";
        public const string AccountType = "http://www.rm.com/claims/accounttype";
        public const string AccountId = "http://www.rm.com/claims/accountid";
        public const string DisplayName = "http://www.rm.com/claims/displayname";
        public const string GeneralSettingID = "http://www.rm.com/claims/generalsettingid";
        public const string TenantGroupId = "http://www.rm.com/claims/tenantgroupid";
        public const string RegisterEmail = "http://www.rm.com/claims/registeremail";
        public const string RecordsUrl = "http://www.rm.com/claims/url";
        public const string AuthType = "http://www.rm.com/claims/Auth";
        public const string SessionType = "http://www.rm.com/claims/sessionId";
        public const string Permission = "http://www.rm.com/claims/Permission";
        public const string ForceLogined = "http://www.rm.com/claims/forceLogined";
        public const string Company = "http://www.rm.com/claims/company";
        public const string AccountNumber = "http://www.rm.com/claims/accountnumber";
    }
}
