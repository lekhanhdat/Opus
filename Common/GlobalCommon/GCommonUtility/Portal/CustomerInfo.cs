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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Login;

namespace AvePoint.Common.Portal
{
    public class CustomerInfo
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Country { get; set; }
        public String Organization { get; set; }
        public List<UserInfo> Members { get; set; }

        public static CustomerInfo Convert(Cloud.Sdk.Data.Aos.CustomerInfo info)
        {
            if (info == null)
            {
                return null;
            }
            var customer = new CustomerInfo()
            {
                Id = info.Id,
                Name = info.Name,
                Country = info.Country,
                Organization = info.Organization,
                Members = UserInfo.Convert(info.Members)
            };
            return customer;
        }

        public static List<CustomerInfo> Convert(List<Cloud.Sdk.Data.Aos.CustomerInfo> infos)
        {
            if (infos == null)
            {
                return null;
            }
            List<CustomerInfo> customers = new List<CustomerInfo>();
            foreach (var item in infos)
            {
                if (item != null)
                {
                    customers.Add(Convert(item));
                }
            }
            return customers;
        }
    }

    public class UserInfo
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Int32 Status { get; set; }
        public Int32 UserType { get; set; }
        public long LastModifiedTime { get; set; }
        public Boolean LegalPerson { get; set; }
        public Int32 IdentityType { get; set; }
        public string DomainName { get; set; }
        public InviteType InviteType { get; set; }
        public string Email { get; set; }

        public AccountMappingDto GetAccountMapping()
        {
            AccountMappingDto account = new AccountMappingDto()
            {
                Id = Id,
                Name = Name,
                Mode = Status == 2 ? AccountMode.Enabled : AccountMode.Disabled,
                Role = RoleType,
                Email = Email
            };
            return account;
        }

        public ObjectRoleType RoleType
        {
            get
            {
                ObjectRoleType role = ObjectRoleType.Member;
                switch (UserType)
                {
                    case 0:
                        role = ObjectRoleType.Member;
                        break;
                    case 1:
                        role = ObjectRoleType.PowerUser;
                        break;
                    case 2:
                        role = LegalPerson ? ObjectRoleType.Owner : ObjectRoleType.PowerUser;
                        break;
                    default:
                        break;
                }
                return role;
            }
        }

        public static UserInfo Convert(Cloud.Sdk.Data.Aos.UserInfo info)
        {
            if (info == null)
            {
                return null;
            }
            return new UserInfo()
            {
                Id = info.Id,
                Name = info.Name,
                FirstName = info.FirstName,
                LastName = info.LastName,
                Status = info.Status,
                UserType = info.UserType,
                LastModifiedTime = info.LastModifiedTime,
                LegalPerson = info.LegalPerson,
                IdentityType = info.IdentityType,
                DomainName = info.DomainName,
                InviteType = (InviteType)info.InviteType
            };
        }

        public static List<UserInfo> Convert(List<Cloud.Sdk.Data.Aos.UserInfo> infos)
        {
            if (infos == null)
            {
                return null;
            }
            List<UserInfo> users = new List<UserInfo>();
            foreach (var item in infos)
            {
                if (item != null)
                {
                    users.Add(Convert(item));
                }
            }
            return users;
        }
    }

    public class RecycleBinCustomer
    {
        public RecycleBinCustomer() { }
        public RecycleBinCustomer(Cloud.Sdk.Data.Aos.CustomerRecycleBin info)
        {
            CustomerId = info.CustomerId;
            Name = info.Name;
            ConnectionString = info.ConnectionString;
            SchemeName = info.SchemeName;
        }
        public String CustomerId { get; set; }
        public String Name { get; set; }
        public String ConnectionString { get; set; }
        public String SchemeName { get; set; }
    }
}
