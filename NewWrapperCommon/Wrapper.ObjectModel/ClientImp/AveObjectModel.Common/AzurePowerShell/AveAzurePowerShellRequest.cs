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
using System.Linq;
using AvePoint.GCommon;
using Microsoft.Online.Administration;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.ObjectModel.Common;
using AvePoint.Office365.Api.Provisioning;

namespace AvePoint.Wrapper.Common
{
    public class AveAzurePowerShellRequest : AvePoint.Office365.Api.Provisioning.AveAzurePowerShellRequest, IAveAzurePowerShellRequest
    {
        private AveBPOSAccountInfo mAccount;
        private static AveLogger mLog = new AveLogger(typeof(AveAzurePowerShellRequest));

        public AveAzurePowerShellRequest(AveBPOSAccountInfo accountInfo)
            : base(accountInfo.UserName, accountInfo.Password, null, accountInfo.TenantId, null, accountInfo.ClientId, accountInfo.AppCert, OnlineAuthenticationProvider.GetAveAzureEnvironment(accountInfo))
        {
            mAccount = accountInfo;
        }
        public Dictionary<string, object> GetSecurityGroups()
        {
            Dictionary<string, object> securityGroupsProp = new Dictionary<string, object>();
            List<Dictionary<string, object>> groupPropList = new List<Dictionary<string, object>>();
            try
            {
                foreach (var group in base.GetGroups())
                {
                    Dictionary<string, object> groupProps = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProps, group);
                    groupPropList.Add(groupProps);
                }
                securityGroupsProp.Add(AveObjectModelConstant.ChildrenProperties, groupPropList);
                return securityGroupsProp;
            }
            catch(Exception e)
            {
                mLog.Warn("Failed to get security groups. error message: {0}", e.ToString());
                return null;
            }
        }

        new public IAveO365User GetUser(string userPrincipalName)
        {
            try
            {
                var user = base.GetUser(userPrincipalName);
                if (user == null) return null;
                return new AveO365User
                {
                    BlockCredential = user.BlockCredential,
                    PreferredLanguage = user.PreferredLanguage,
                };
            }
            catch(Exception e)
            {
                mLog.Warn("The User:{0} not found. Error: {1}", userPrincipalName, e);
                return null;
            }
        }

        new public IAveO365Group GetGroup(string groupName, string email)
        {
            try
            {
                if (string.IsNullOrEmpty(groupName))
                {
                    return null;
                }
                var group = base.GetGroup(groupName, email);
                if (group == null) return null;
                return new AveO365Group
                {
                    Id = group.ObjectId.Value,
                };
            }
            catch(Exception e)
            {
                mLog.Warn("An error occured while getting group. Group: {0}, Error: {1}", groupName, e);
                return null;
            }
        }

        /// <summary>
        /// According to the groupname to find the security group 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>group objectid</returns>
        public Dictionary<string, object> GetUsersFromSecurityGroup(string groupName)
        {
            Dictionary<string, object> membersProp = new Dictionary<string, object>();
            List<Dictionary<string, object>> memberPropList = new List<Dictionary<string, object>>();
            try
            {
                var members = base.GetGroupMembers(groupName);
                foreach (GroupMember member in members)
                {
                    Dictionary<string, object> memberProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(memberProp, member);
                    memberPropList.Add(memberProp);
                }
                membersProp.Add(AveObjectModelConstant.ChildrenProperties, memberPropList);
                return membersProp;
            }
            catch(Exception e)
            {
                mLog.Warn("Failed to get users from security group. Group: {0}, error detail : {1}", groupName, e.ToString());
                return null;
            }
        }
        public List<Dictionary<string, object>> GetOffice365Domains()
        {
            try
            {
                List<Dictionary<string, object>> domainProperties = new List<Dictionary<string, object>>();
                var domains = base.GetDomains();
                if (domains != null)
                {
                    foreach (var domain in domains)
                    {
                        Dictionary<string, object> domainProp = new Dictionary<string, object>();
                        AveObjectCopy.GetObjectBasicProperties(domainProp, domain);
                        domainProperties.Add(domainProp);
                    }
                }
                return domainProperties;
            }
            catch(Exception e)
            {
                mLog.Warn("Failed to get O365 domains, error detail : {0}", e.ToString());
                return null;
            }
        }
        public List<Dictionary<string, object>> GetOffice365UserDetailsForUserSeat()
        {
            List<Dictionary<string, object>> userSeats = new List<Dictionary<string, object>>();
            var users = base.GetOffice365Users();

            foreach (var user in users)
            {
                try
                {
                    #region Need skip.
                    if (user.IsLicensed.HasValue && (!user.IsLicensed.Value))
                    {
                        mLog.Debug("This user is unlicensed. User: {0}", user.UserPrincipalName);
                        continue;
                    }
                    if (user.UserType.HasValue && user.UserType == UserType.Guest)
                    {
                        mLog.Debug("This user is a Guest. User: {0}", user.UserPrincipalName);
                        continue;
                    }
                    #endregion
                    Dictionary<string, object> userInfo = new Dictionary<string, object>();
                    List<Dictionary<string, string>> serviceStatusCollection = new List<Dictionary<string, string>>();
                    userInfo["Login"] = user.UserPrincipalName;
                    userInfo["Title"] = user.DisplayName;
                    userInfo["ServiceStatusCollection"] = serviceStatusCollection;

                    foreach (var l in user.Licenses)
                    {
                        if (l.ServiceStatus == null)
                        {
                            continue;
                        }
                        foreach (var serviceStatus in l.ServiceStatus)
                        {
                            if (serviceStatus.ProvisioningStatus == ProvisioningStatus.Disabled
                                || serviceStatus.ProvisioningStatus == ProvisioningStatus.Error
                                || serviceStatus.ProvisioningStatus == ProvisioningStatus.None)
                            {
                                continue;
                            }
                            serviceStatusCollection.Add(new Dictionary<string, string> {
                                { "ServiceName", serviceStatus.ServicePlan.ServiceName},
                                {"SkuPartNumber",l.AccountSku.SkuPartNumber }
                            });
                        }
                    }
                    userSeats.Add(userInfo);
                }
                catch (Exception e)
                {
                    mLog.Error("Handle a user failed during retrieve user seats. User: {0}, Error: {1}", user.UserPrincipalName, e);
                }
            }
            return userSeats;
        }
        [Obsolete]
        public bool CheckDeadUser(IAveUser user, ref AccountStatus userStatus)
        {
            bool isDeadUser = false;
            var searchDefinition = new UserSearchDefinition
            {
                PageSize = 500,
                SortDirection = SortDirection.Ascending,
                SortField = SortField.None,
                SearchString = user.Name
            };
            try
            {
                var users = base.GetUsers(searchDefinition);
                if (users != null)
                {
                    mLog.Info("Get {0} users, the account:{1}", users.Count, mAccount.UserName);
                }
                foreach (User tempUser in users)
                {
                    bool isBlocked = tempUser.BlockCredential ?? false;
                    string LoginName = user.NoPrefixLoginName.StartsWith("membership|", StringComparison.OrdinalIgnoreCase) ? user.NoPrefixLoginName.Substring("membership|".Length) : user.NoPrefixLoginName;
                    if (isBlocked && tempUser.UserPrincipalName.Equals(LoginName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        userStatus = AccountStatus.Deactived;
                        isDeadUser = true;
                        break;
                    }
                }
            }
            catch(Exception e)
            {
                mLog.Warn("Failed to get O365 users. error message: {0}", e.ToString());
            }
            return isDeadUser;
        }

        public string GetOffice365AdminSiteCollectionUrl()
        {
            try
            {
                return GetOffice365SPOAdminUrl();
            }
            catch (Exception e)
            {
                mLog.Error("Failed to get O365 Admin Site Collection Url by user account, user : {0}, error detail : {1}", mAccount.UserName, e.ToString());
                return null;
            }
        }
        new public int GetUserRole(string userPrincipalName)
        {
            return (int)base.GetUserRole(userPrincipalName);
        }
    }
}
