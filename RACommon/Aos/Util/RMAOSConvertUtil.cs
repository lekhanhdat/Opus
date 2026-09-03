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
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb.Account;
using Cloud.Sdk.Data.Aos;
using Cloud.Sdk.Data.Aos.SecurityProfile;
using Cloud.Sdk.Data.Aos.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Common.Aos.Util
{
    public class RMAOSConvertUtil
    {
        public static RMAosSecurityProfile Convert2SecurityProfile(SecurityProfileModel profile)
        {
            return new RMAosSecurityProfile
            {
                Id = profile.Id,
                Name = profile.Name,
                SecurityProfileType = (int)profile.Type,
                KeyIdentity = profile.KeyIdentity,
                ClientId = profile.ClientId,
                ClientSecret = profile.ClientSecret
            };
        }
        public static RMAosAuthenticationProfile Convert2AuthenticationProfile(CloudAos.AppProfileInfo profile)
        {
            return new RMAosAuthenticationProfile()
            {
                Id = profile.Id,
                //AppCertContent = profile.AppCertContent,
                AADEnvironment = (RMAOSAADEnvironment)profile.AADEnvironment,
                //AppCertSecret = profile.AppCertSecret,
                //AppCertSecretContent = profile.AppCertSecretContent,
                AppClientId = profile.AppClientId,
                TenantId = profile.TenantId,
                AppType = (int)profile.Type,
            };
        }

        public static RMAosGoogleAppProfile Convert2GoogleAppProfile(CloudAos.GsuiteCustomAppProfile profile, string customerId)
        {
            if (profile is null)
            {
                return null;
            }
            return new RMAosGoogleAppProfile(customerId)
            {
                TenantId = profile.TenantId,
                AOSAppId = profile.Id,
                DomainName = profile.DomainName,
                UserName = profile.UserName,
                DefaultDomainName = profile.DomainName,
                ProfileName = profile.Name,
                TokenType = (int)profile.Type,
                ServiceAccount = profile.ServiceAccount,
                PrivateKey = profile.PrivateKey,
                AuthenticationType = profile.GoogleAuthenticationType
            };
        }

        public static RMAosServiceAccount Convert2RMServiceAccount(ServiceAccount dto)
        {
            if (dto == null) return null;
            return new RMAosServiceAccount()
            {
                TenantId = dto.TenantId,
                Password = dto.Password,
                UserName = dto.UserName,
                AdminUrl = dto.AdminUrl,
                Status = (int)dto.Status
            };

        }

        public static AccountDto Convert2RMAccount(CloudAos.UserInfo dto, bool isOwner = false)
        {
            if (dto == null) return null;
            return new AccountDto()
            {
                UserId = dto.InviteType == CloudAos.InviteType.Group ? dto.ObjectId : dto.Id, //use userId from AOS first.Replace to AADID in release version after July.
                UserPrincipalName = dto.InviteType == CloudAos.InviteType.Group ? dto.Email : dto.Name,
                DisplayName = GetUserName(dto.FirstName, dto.LastName, dto.Name),
                ObjectType = (Contract.RMWeb.RMActiveDirectoryObjectType)dto.InviteType,
                AccountType = isOwner ? Contract.RMWeb.RMAccountType.RegisteredUser : (Contract.RMWeb.RMAccountType)dto.Role.Type,
                Email = dto.Email,
                LastModifiedTime = dto.LastModifiedTime.Ticks,
                AADId = dto.ObjectId, // google user id can assign into AADId
                FirstName = dto.FirstName,
                LastName = dto.LastName 
            };
        }

        public static RMAosAccountModelResult Convert2RMAccountModelResult(CloudAos.AccountInfo dto)
        {
            if (dto == null) return null;
            return new RMAosAccountModelResult()
            {
                Account = Convert2RMAosAccountInfo(dto),
                AOSDataCenter = dto.DataCenter,
                IsInAOS = dto.IsExisted,
                IsOffice365TenantInAOS = dto.IsOffice365TenantExist
            };
        }

        public static RMAosPostRole Convert2RMAosPostRole(CloudAos.PostRole role)
        {
            if (role == null) return null;
            return new RMAosPostRole()
            {
                ApplicationName = role.Product,
                IsAcceptedLicenseAgreement = role.IsAcceptedLicenseAgreement,
                Url = role.Url,
                UserType = (int)role.UserType
            };
        }
        public static PostRole Convert2RMAccoutPostRole(Cloud.Sdk.Data.Aos.PostRole role)
        {
            if (role == null) return null;
            return new PostRole()
            {
                ApplicationName = role.ApplicationName,
                IsAcceptedLicenseAgreement = role.IsAcceptedLicenseAgreement,
                Url = role.Url,
                UserType = role.UserType
            };
        }

        public static RMAosCustomerInfo Convert2RMAosCustomerInfo(CloudAos.CustomerInfo info)
        {
            if (info == null) return null;
            return new RMAosCustomerInfo()
            {
                Id = info.Id,
                //AppStatus = info.AppStatus,
                Country = info.CountryCode,
                //IsInternal = info.IsInternal,
                Name = info.Name,
                Organization = info.Organization,
                //Region = info.CountryCode,
                RegistrationTime = info.RegistrationTime.Ticks
            };
        }

        public static RMAosAccountInfo Convert2RMAosAccountInfo(CloudAos.AccountInfo info)
        {
            if (info == null) return null;
            return new RMAosAccountInfo()
            {
                Id = info.User.Id,
                Customer = Convert2RMAosCustomerInfo(info.Customer),
                InviteType = (RMAosInviteType)info.User.InviteType,
                Name = info.User.Name,
                PostRole = info?.PostRoles.ConvertAll(r => Convert2RMAosPostRole(r)),
                TenantId = info.Customer.Id,
                UserGroups = info.UserGroups?.ConvertAll(r => Convert2AzureADGroupInfo(r))
            };
        }

        public static Contract.Tenant.AzureADGroupInfo Convert2AzureADGroupInfo(CloudAos.AzureADGroupInfo info)
        {
            if (info == null) return null;
            return new Contract.Tenant.AzureADGroupInfo
            {
                DisplayName = info.DisplayName,
                DomainName = info.DomainName,
                Email = info.Email,
                IdentityType = (int)info.IdentityType,
                IsActive = info.IsActive,
                ObjectId = info.ObjectId,
                ParentGroupId = info.ParentGroupId,
                ParentGroupIds = info.ParentGroupIds,
                //Roles = info.Roles.ConvertAll(r => Convert2RMAccoutPostRole(r))
            };
        }
        public static PoolUserDto ConvertToRMPoolUser(CloudAos.ServiceAccount dto)
        {
            if (dto == null) return null;

            var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(dto.TenantId).GetAwaiter().GetResult().AdminUrl;

            return new PoolUserDto()
            {
                TenantId = dto.TenantId,
                Password = dto.Password,
                UserName = dto.UserName,
                AdminUrl = adminUrl,
                Status = (int)dto.Status
            };

        }

        public static PoolUserDto ConvertToPoolUserDto(CloudAos.ServiceAccount dto)
        {
            if (dto == null) return null;

            var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(dto.TenantId).GetAwaiter().GetResult().AdminUrl;

            return new PoolUserDto()
            {
                TenantId = dto.TenantId,
                Password = dto.Password,
                UserName = dto.UserName,
                AdminUrl = adminUrl,
                Status = (int)dto.Status
            };

        }

        public static string GetUserName(string firstName, string lastName, string name)
        {
            var result = string.Empty;
            if ((string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(firstName.Trim())) &&
                (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(lastName.Trim())))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var tempNames = name.Split('@');
                    result = tempNames[0];
                }
            }
            else
            {
                result = firstName + " " + lastName;
            }
            return result;
        }


    }
}
