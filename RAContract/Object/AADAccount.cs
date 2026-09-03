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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    public class AADAccounts
    {
        [JsonProperty("odata.metadata")]
        public string OdataMetadata { get; set; }

        [JsonProperty("value")]
        public List<AADAccount> Value { get; set; }

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
    public class AADAccount
    {
        /// <summary>
        /// Id in O365
        /// </summary>
        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// Account id in AOS
        /// </summary>
        [JsonProperty("AccountId", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty("InviteType", NullValueHandling = NullValueHandling.Ignore)]
        public AccountType InviteType { get; set; }

        [JsonProperty("DisplayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("SurName", NullValueHandling = NullValueHandling.Ignore)]
        public string SurName { get; set; }

        [JsonProperty("GivenName", NullValueHandling = NullValueHandling.Ignore)]
        public string GivenName { get; set; }

        [JsonProperty("UserPrincipalName", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPrincipalName { get; set; }

        [JsonProperty("Mail", NullValueHandling = NullValueHandling.Ignore)]
        public string Mail { get; set; }

        /// <summary>
        /// ///O365 tenant id
        /// </summary>
        [JsonProperty("TenantId", NullValueHandling = NullValueHandling.Ignore)]        
        public string TenantId { get; set; }

        public static AvePoint.RA.Contract.RMWeb.Account.AccountDto Convert2AccountDto(AADAccount account)
        {
            return new AvePoint.RA.Contract.RMWeb.Account.AccountDto()
            {
                AADId = account.Id,
                UserId = account.InviteType == AccountType.Group ?account.Id:account.AccountId,//change this for loginuser under azure ad group can't get account id in aos.
                UserPrincipalName = account.UserPrincipalName?? account.Mail,
                DisplayName = account.DisplayName,
                ObjectType = account.InviteType== AccountType.Group? RMWeb.RMActiveDirectoryObjectType.Group: RMWeb.RMActiveDirectoryObjectType.User,
                FirstName = account.GivenName,   //名字
                LastName = account.SurName,     //姓
            };
        }

        public static AOSUserDto Convert2AOSUserDto(AADAccount account)
        {
            return new AOSUserDto
            {
                UserId = account.AccountId,
                UserName = account.DisplayName,
                SurName = account.SurName,
                GivenName = account.GivenName,
                Id = account.Id,
                TenantId = account.TenantId,
                UserPrincipalName = account.UserPrincipalName ?? account.Mail,
                DisplayName = account.DisplayName,
                Email = account.Mail,
                InviteType = account.InviteType
            };
        }        
        
        public static ManualApprovalAOPUserInfo Convert2ManualAOSUserDto(AADAccount account)
        {
            return new ManualApprovalAOPUserInfo
            {
                UserId = account.AccountId,
                UserName = account.DisplayName,
                SurName = account.SurName,
                GivenName = account.GivenName,
                Id = account.Id,
                TenantId = account.TenantId,
                UserPrincipalName = account.UserPrincipalName ?? account.Mail,
                DisplayName = account.DisplayName,
                Email = account.Mail,
                InviteType = account.InviteType
            };
        }

        public static AADAccount Convert2AADAccountDto(ReviewerUser user)
        {
            var dto = new AADAccount
            {
                AccountId = user.UserId,
                DisplayName = user.DisplayName,
                SurName = user.SurName,
                GivenName = user.GivenName,
                Id = user.Id,
                TenantId = user.TenantId,
                Mail = user.UserPrincipalName,
                UserPrincipalName = user.UserPrincipalName
            };
            switch (user.InviteType)
            {
                case RMActiveDirectoryObjectType.User:
                    dto.InviteType = AccountType.User;
                    break;
                case RMActiveDirectoryObjectType.Group:
                    dto.InviteType = AccountType.Group;
                    dto.Mail = dto.Mail ?? dto.UserPrincipalName;
                    break;
                case RMActiveDirectoryObjectType.UserInGroup:
                    break;
                default:
                    break;
            }
            return dto;
        }

        public static AADAccount Convert2AADAccountDto(ToUserInfo user)
        {
            var account = new AADAccount
            {
                AccountId = user.UserId,
                DisplayName = user.DisplayName,
                SurName = user.SurName,
                GivenName = user.GivenName,
                Id = user.Id,
                TenantId = user.TenantId,
                UserPrincipalName = user.UserPrincipalName,
                Mail = user.Email ?? user.UserPrincipalName,
                InviteType = user.InviteType
            };
            return account;
        }

        public static AADAccount Convert2AADAccountDto(AOSUserDto user)
        {
            var account = new AADAccount
            {
                AccountId = user.UserId,
                DisplayName = user.DisplayName,
                SurName = user.SurName,
                GivenName = user.GivenName,
                Id = user.Id,
                TenantId = user.TenantId,
                UserPrincipalName = user.UserPrincipalName,
                Mail = user.Email ?? user.UserPrincipalName,
                InviteType = user.InviteType
            };
            return account;
        }
    }
}
