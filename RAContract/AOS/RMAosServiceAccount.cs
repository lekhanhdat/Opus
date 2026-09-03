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
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Aos
{
    public class RMAosServiceAccount
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Status { get; set; }
        public string TenantId { get; set; }
        public string Password { get; set; }
        public string AdminUrl { get; set; }
        public long Usage { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class RMAosAccountInfo 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public RMAosInviteType InviteType { get; set; }
        public RMAosCustomerInfo Customer { get; set; }
        public List<RMAosPostRole> PostRole { get; set; }
        public List<AzureADGroupInfo> UserGroups { get; set; }

    }

    public class RMAosPostRole 
    {
        public string ApplicationName { get; set; }
        public string Url { get; set; }
        public bool IsAcceptedLicenseAgreement { get; set; }
        public int UserType { get; set; }
    }

    public class RMAosCustomerInfo 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Organization { get; set; }
        public int AppStatus { get; set; }
        public string Region { get; set; }
        public long RegistrationTime { get; set; }
        public bool IsInternal { get; set; }
    }

    public class RMAosUserInfo 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserType { get; set; }
        public int Status { get; set; }
        public long LastModifiedTime { get; set; }
        public bool LegalPerson { get; set; }
        public int IdentityType { get; set; }
        public string DomainName { get; set; }
        public RMAosInviteType InviteType { get; set; }
        public string ObjectId { get; set; }
        public string Email { get; set; }
    }

    public enum RMAosInviteType
    {
        User = 0,
        Group = 1,
        UserInGroup = 2
    }
}
