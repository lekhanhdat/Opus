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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.RACommonUtility.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Security.Aos
{
    public class AosAuthentication
    {
        public RMIdentity AuthenticateCredential(AOSCredential model)
        {
            RMIdentity identity = new RMIdentity();
            identity.Name = model.UserName;
            identity.AuthenticationType = RMAuthenticationTypes.Aos;
            identity.DisplayName = model.DisplayName;
            identity.TenantGroupId = model.TenantGroupId;
            identity.AccountType = model.AccountType;
            identity.AccountId = model.UserId;
            identity.GPermission = model.GPermission;
            identity.IsAuthenticated = true;
            identity.AuthenticationType = RMAuthenticationTypes.Aos;
            identity.Company = model.Company;
            identity.AccountNumber = model.AccountNumber;
            identity.PartnerUser = model.PatnerUser;
            identity.PartnerOwner = model.PatnerOwner;
            return identity;
        }
        public RMIdentity Office365AuthenticateCredential(AOSCredential model)
        {
            RMIdentity identity = new RMIdentity();
            identity.Name = model.UserName;
            identity.AuthenticationType = RMAuthenticationTypes.Office365;
            identity.DisplayName = model.DisplayName;
            identity.TenantGroupId = model.TenantGroupId;
            identity.AccountType = model.AccountType;
            identity.AccountId = model.UserId;
            identity.IsAuthenticated = true;
            return identity;
        }

    }
}
