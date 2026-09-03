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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Online.Administration;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    using AvePoint.GCommon;

    public class AveAzurePowerShellRequest : IDisposable
    {
        private IAPSR request;
        private AveBPOSAccountInfo mAccount;
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveAzurePowerShellRequest));
        
        public AveAzurePowerShellRequest(AveBPOSAccountInfo accountInfo)
        {
            mAccount = accountInfo;
            var becWebServiceInstance = new BecWebServiceInstance(accountInfo);
            var tokenManager = new UserCredentialAPSTokenManager(accountInfo, becWebServiceInstance);
            var token = tokenManager.Token;//ensure token

            request = new BecWebServiceAPSR(becWebServiceInstance, tokenManager);
        }

        /// <summary>
        /// support MFA account
        /// </summary>
        /// <param name="accountInfo">Username with password, if the MFA is enabled, the password should be app password</param>
        /// <param name="customerId">TenantGroupId from AveMessage, for example: IdentityManager.IdentityContent = backupInfo.TenantGroupId;</param>
        /// <param name="aosApiUrl">{GCommonRoleConfiguration.PortalAPIURL}. production: https://api.avepointonlineservices.com/ test: https://testapi.avepointonlineservices.com </param>
        /// <param name="clientId">{GCommonRoleConfiguration.AppClientId}</param>
        public AveAzurePowerShellRequest(AveBPOSAccountInfo accountInfo, string customerId, string aosApiUrl, string clientId)
            : this(accountInfo, customerId, null, aosApiUrl, clientId)
        {

        }

        /// <summary>
        /// support MFA account with tenant id
        /// </summary>
        /// <param name="accountInfo">Username with password, if the MFA is enabled, the password should be app password</param>
        /// <param name="customerId">TenantGroupId from AveMessage, for example: IdentityManager.IdentityContent = backupInfo.TenantGroupId;</param>
        /// <param name="tenantId">Office 365 Tenant Id</param>
        /// <param name="aosApiUrl">{GCommonRoleConfiguration.PortalAPIURL}. production: https://api.avepointonlineservices.com/ test: https://testapi.avepointonlineservices.com </param>
        /// <param name="clientId">{GCommonRoleConfiguration.AppClientId}</param>
        public AveAzurePowerShellRequest(AveBPOSAccountInfo accountInfo, string customerId, string tenantId, string aosApiUrl, string clientId)
        {
            mAccount = accountInfo;
            var becWebServiceInstance = new BecWebServiceInstance(accountInfo);
            var tokenManager = new MFATokenManager(accountInfo, becWebServiceInstance, customerId, tenantId, aosApiUrl, clientId);
            var token = tokenManager.Token;//ensure token

            if (tokenManager.TokenType == APSTokenType.AppOnlyBearer)
            {
                request = new GraphAPSR(tokenManager);
            }
            else
            {
                request = new BecWebServiceAPSR(becWebServiceInstance, tokenManager);
            }
        }

        /// <summary>
        /// get office365 security groups
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetSecurityGroups()
        {
            List<Group> groups = request.GetGroups();

            if (groups.Count > 0)
            {
                Dictionary<string, object> securityGroupsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> groupPropList = new List<Dictionary<string, object>>();

                foreach (Group group in groups)
                {
                    Dictionary<string, object> groupProps = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProps, group);
                    groupPropList.Add(groupProps);
                }
                securityGroupsProp.Add(AveObjectModelConstant.ChildrenProperties, groupPropList);

                return securityGroupsProp;
            }

            return null;
        }

        /// <summary>
        /// According to the groupname to find the security group 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>group objectid</returns>
        public Guid GetGroupObjectIdByName(string groupName)
        {
            Guid groupId = Guid.Empty;
            if (!string.IsNullOrEmpty(groupName))
            {
                Group group = request.GetGroup(groupName);

                if (group != null)
                {
                    groupId = (Guid)group.ObjectId;
                }
            }
            return groupId;
        }

        /// <summary>
        /// get the security group members
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetUsersFromSecurityGroup(string groupName)
        {
            var members = request.GetGroupMembers(groupName);

            if (members != null && members.Count > 0)
            {
                Dictionary<string, object> membersProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> memberPropList = new List<Dictionary<string, object>>();

                foreach (GroupMember member in members)
                {
                    Dictionary<string, object> memberProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(memberProp, member);
                    memberPropList.Add(memberProp);
                }
                membersProp.Add(AveObjectModelConstant.ChildrenProperties, memberPropList);
                return membersProp;
            }
            return null;
        }

        public List<Dictionary<string, object>> GetOffice365Domains()
        {
            List<Domain> domains = request.GetDomains();
            if (domains != null && domains.Count > 0)
            {
                List<Dictionary<string, object>> domainProperties = new List<Dictionary<string, object>>();
                foreach (Domain domain in domains)
                {
                    Dictionary<string, object> domainProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(domainProp, domain);
                    domainProperties.Add(domainProp);
                }

                return domainProperties;
            }

            return null;
        }

        public string GetOffice365Domain()
        {
            var domains = request.GetDomains();

            if (domains != null && domains.Count > 0)
            {
                var domain = domains.Find(item => item.IsInitial.Value);

                if (domain != null)
                {
                    return domain.Name;
                }
            }

            return string.Empty;
        }

        public List<User> GetOffice365Users()
        {
            //缓存应该添加到外围，而不应该在这里维护。
            //if (mUsers != null && mUsers.Count > 0)
            //{
            //    return mUsers;
            //}
            return request.GetUsers();
        }

        //check dead user . if dead return true
        public bool CheckDeadUser(IAveUser user, ref AccountStatus userStatus)
        {
            bool isDeadUser = false;
            if (user.IsDomainGroup)
            {
                isDeadUser = request.GetGroup(user.Name, user.Email) == null ? true : false;
            }
            else
            {
                string loginName = user.NoPrefixLoginName.StartsWith("membership|") ? user.NoPrefixLoginName.Substring("membership|".Length) : user.NoPrefixLoginName;
                User adUser = request.GetUser(loginName);
                if (adUser != null)
                {
                    if (adUser.BlockCredential == true)
                    {
                        userStatus = AccountStatus.Deactived;
                        isDeadUser = true;
                    }
                    else
                    {
                        userStatus = AccountStatus.Actived;
                    }
                }
                else
                {
                    userStatus = AccountStatus.Deleted;
                    isDeadUser = true;
                }
            }
            return isDeadUser;
        }

        public User GetOffice365User(string userLoginName)
        {
            if (userLoginName.StartsWith("i:0#.f|membership|"))
            {
                userLoginName = userLoginName.Substring("i:0#.f|membership|".Length);
            }

            return request.GetUser(userLoginName);
        }

        public bool IsSmallBusinessSubscription()
        {
            var isSmallBusiness = false;
            var subscriptions = request.GetSubscriptions();
            if (subscriptions != null && subscriptions.Count > 0)
            {
                foreach (var item in subscriptions)
                {
                    if ("LITEPACK_P2".Equals(item.SkuPartNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        isSmallBusiness = true;
                        break;
                    }
                }
            }

            return isSmallBusiness;
        }

        public bool IsGlobalAdmin(string userPrincipalName)
        {
            UserRole userRole = GetUserRole(userPrincipalName);
            if ((userRole & UserRole.GlobalAdministrator) != UserRole.User)
            {
                return true;
            }
            return false;
        }

        public UserRole GetUserRole(string userPrincipalName)
        {
            UserRole userRole = UserRole.User;
            List<Role> roles = request.GetUserRoles(userPrincipalName);
            if (roles != null && roles.Count > 0)
            {
                foreach (var role in roles)
                {
                    if (role.ObjectId != null)
                    {
                        switch (role.ObjectId.ToString())
                        {
                            case AzureConst.GLOBAL_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.BILLING_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.EXCHANGE_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.PASSWORD_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.SKYPE_FOR_BUSINESS_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.SERVICE_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.SHAREPOINT_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            case AzureConst.USER_MANAGEMENT_ADMINISTRATOR:
                                userRole |= UserRole.GlobalAdministrator;
                                break;
                            default:
                                userRole |= UserRole.User;
                                break;
                        }
                    }
                }
            }

            return userRole;
        }

        public string GetTenantAdminUrl()
        {
            mLogger.Info("start to get admin url");
            if (mAccount!=null && !string.IsNullOrEmpty(mAccount.AdminUrl))
            {
                mLogger.Info("current user:{0},admin url:{1}", mAccount.UserName, mAccount.AdminUrl);
                return mAccount.AdminUrl;
            }
            string domain = GetOffice365Domain();
            if (string.IsNullOrEmpty(domain))   //SAAS-12056 如果账号不正确则domain的返回值为空
            {
                return null;
            }
            string adminUrl = "https://{0}-admin.sharepoint.com";
            if (domain.EndsWith(".partner.onmschina.cn"))
            {
                adminUrl = "https://{0}-admin.sharepoint.cn";
            }
            return string.Format(adminUrl, domain.Substring(0, domain.IndexOf('.')));
        }

        public void Dispose()
        {
            request.Dispose();
        }

        /// <summary>
        /// SAAS-23003
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public List<UserDetail> GetMembersInGroup(string groupName)
        {
            List<UserDetail> users = new List<UserDetail>();
            var members = request.GetGroupMembers(groupName);

            if (members != null && members.Count > 0)
            {
                List<Dictionary<string, object>> memberPropList = new List<Dictionary<string, object>>();
                //ObjectId,IsLicensed,OverallProvisioningStatus,ValidationStatus
                foreach (GroupMember member in members)
                {
                    if (!string.IsNullOrEmpty(member.DisplayName))
                    {
                        UserDetail user = new UserDetail();
                        user.DisplayName = member.DisplayName;
                        user.Email = member.EmailAddress;
                        if (member.GroupMemberType == GroupMemberType.User)
                        {
                            user.AccountType = AvePoint.GCommon.Contract.SharePointBrowser.Object.AccountType.ADUser;
                        }
                        else if (member.GroupMemberType == GroupMemberType.Group)
                        {
                            user.AccountType = AvePoint.GCommon.Contract.SharePointBrowser.Object.AccountType.ADGroup;
                        }
                        else
                        {
                            continue;
                        }
                        users.Add(user);
                    }
                }
            }
            return users;
        }
    }
}
