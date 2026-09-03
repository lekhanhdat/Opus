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
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Graph;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Data.Aos;
using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Common.Aos
{
    public class AADAccountWrapperService : IAccountWrapperService
    {
        private static RALogger mLogger = RALogger.GetInstance(typeof(AADAccountWrapperService));

        private const string GraphApiQueryUserByEmailString = @"{0}/{1}/users?$filter=userPrincipalName eq '{2}' or mail eq '{2}'&$select={3}";
        private const string GraphApiQueryGroupByIdString = @"{0}/{1}/groups?$filter=id eq '{2}'&$select={3}";
        private const string GroupApiQueryGroupByMailString = @"{0}/{1}/groups?$filter=mail eq '{2}'&$select={3}";
        private const string GraphApiTeamGroupOwnersQueryString = @"{0}/{1}/groups/{2}/owners?$select={3}";
        private const string GraphApiUniqueUserQueryString = @"{0}/{1}/users('{2}')?$select={3}";
        private const string GraphApiUserMemberOfQueryString = @"{0}/{1}/users/{2}/memberOf?$select={3}";
        private const string GraphApiUserOwnedGroupsQueryString = @"{0}/{1}/users/{2}/ownedObjects/microsoft.graph.group?$select={3}";
        private const string GraphApiUserQueryString = @"{0}/{1}/users?$top={2}&$filter= userType eq 'member' and (startswith(userPrincipalName,'{3}') or startswith(displayName,'{3}') or startswith(givenName,'{3}') or startswith(surname,'{3}') or startswith(mail,'{3}') or startswith(mailNickname,'{3}'))&$select={4}";
        //private const string GraphApiUserQueryString = @"{0}/{1}/users?$top={2}&$filter= startswith(userPrincipalName,'{3}') or startswith(displayName,'{3}') or startswith(givenName,'{3}') or startswith(surname,'{3}') or startswith(mailNickname,'{3}')&$select={4}";
        private const string GraphApiGroupQueryString = @"{0}/{1}/groups?$top={2}&$filter=startswith(displayName,'{3}') or startswith(mail,'{3}') or startswith(mailNickname,'{3}')&$select={4}";
        private const string UserSelector = @"id,displayName,mail,userPrincipalName,surName,givenName";
        private const string GroupSelector = @"id,displayName,mail";
        private const string ApiVersion = "v1.0";

        public List<AADAccount> Regester2AOS(string tenantId, IList<AADAccount> accounts)
        {
            var result = new List<AADAccount>();
            var groups = accounts.GroupBy(o => o.TenantId); // group by O365 tenant id
            foreach (var group in groups)
            {
                result.AddRange(Regester2AOS(tenantId, group.Key, group.ToList()));
            }

            return result;

        }

        public IList<AADAccount> Regester2AOS(string tenantId, string o365TenantId, IList<AADAccount> accounts)
        {
            var maxCountPerRequest = 15;
            var total = accounts.Count();
            var index = 0;
            var result = new List<AADAccount>();
            while (index < total)
            {
                var aac = accounts.Skip(index).Take(maxCountPerRequest).ToList();
                var temp = RMAosApiClient.RegisterAADAccount(aac, tenantId, o365TenantId, Contract.Tenant.TenantLocalValue.LogonUserEmail);
                result.AddRange(temp);
                index += maxCountPerRequest;
            }

            return result;
        }

        public  List<AADAccount> GetAADAccounts(List<AADAccount> accounts, string customerId)
        {
            return RMAosApiClient.GetAADAccounts(accounts, customerId);
        }

        public AADAccount GetAccount(string tenantId, string userIdOrUPN)
        {
            using (new PerformanceScope($"Get account from O365 tenant '{tenantId}'"))
            {
                var profiles = GetProfiles(tenantId);
                if (profiles.Count == 0)
                {
                    return null;
                }
                var o365TenantIds = profiles.Select(s => s.TenantId).ToList().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var o365TenantId in o365TenantIds)
                {
                    try
                    {
                        var profile = profiles.First(o => o365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

                        mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

                        var user = GetAccount(profile, userIdOrUPN);
                        if (user != null) return user;
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn($"An error occurred while get user from Azure AD with O365 tenant id : {o365TenantId}. error : {e.ToString()}");
                    }
                }
                return null;
            }

        }

        public AADAccount GetAccountByIdOrUPN(string tenantId, string userId, string userPrincipalName)
        {
            using (new PerformanceScope($"Get account from O365 tenant '{tenantId}'"))
            {
                var profiles = GetProfiles(tenantId);
                if (profiles.Count == 0)
                {
                    return null;
                }
                var o365TenantIds = profiles.Select(s => s.TenantId).ToList().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                AADAccount user = null;
                foreach (var o365TenantId in o365TenantIds)
                {
                    try
                    {
                        var profile = profiles.First(o => o365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

                        mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

                         user = null;
                        if (!string.IsNullOrEmpty(userId))
                        {
                            user = GetAccount(profile, userId);
                        }
                        if(user == null && !string.IsNullOrEmpty(userPrincipalName))
                        {
                            user = GetAccount(profile, userPrincipalName);
                        }
                        if (user != null) return user;
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn($"An error occurred while get user from Azure AD with O365 tenant id : {o365TenantId}. error : {e.ToString()}");
                    }
                }
                return null;
            }

        }

        public AADAccount GetAccount(CloudAos.AppProfileInfo profile, string userIdOrUPN)
        {
            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                return null;
            }

            //mLogger.Info($"Try to get user by id/upn with customer Id : {profile.CustomerId}, o365 tenant Id : {profile.TenantId}, userIdOrUPN: {userIdOrUPN}");
            var user = GetUser(profile, accessToken, userIdOrUPN);

            return user;
        }

        private AADAccount GetUser(CloudAos.AppProfileInfo profile, string accessToken, string userIdOrUPN)
        {
            AADAccount result = null;
            try
            {
                string upnEncode = System.Web.HttpUtility.UrlEncode(userIdOrUPN);
                var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
                string uri =
                    string.Format(GraphApiUniqueUserQueryString, graphEndPoint, ApiVersion, upnEncode, UserSelector);

                string r = HttpHelper.Get(uri, accessToken);
                result = JsonConvert.DeserializeObject<AADAccount>(r);
            }
            catch (Exception ex)
            {
                mLogger.Error("CAA UserWrapper Get User Exception: {0}", ex);
            }

            return result;
        }

        public List<AADAccount> GetTeamSiteGroupOwners(string tenantId, string aadId, string office365TenantId)
        {
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
            var profile = profiles.FirstOrDefault(o => office365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                throw new Exception("RM_MA_SiteOwner_No_AppProfile");
            }

            mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                throw new Exception($"Can't find accss token.");
            }
            var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
            var uri = string.Format(GraphApiTeamGroupOwnersQueryString, graphEndPoint, ApiVersion, aadId, UserSelector);
            mLogger.Info($"Get team site group owners uri: [{uri}].");
            var result = HttpHelper.Get(uri, accessToken);
            return JsonConvert.DeserializeObject<AADAccounts>(result).Value;
        }

        public List<AADAccount> GetGroupsByAadIds(string tenantId, List<string> groupAadIds, string office365TenantId)
        {
            var accounts = new List<AADAccount>();
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
            var profile = profiles.FirstOrDefault(o => office365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                throw new Exception("RM_MA_SiteOwner_No_AppProfile");
            }

            mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                throw new Exception($"Can't find accss token.");
            }
            var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
            foreach (var groupAadId in groupAadIds)
            {
                var uri = string.Format(GraphApiQueryGroupByIdString, graphEndPoint, ApiVersion, groupAadId, GroupSelector);
                mLogger.Info($"Get sharepoint site groups uri: [{uri}].");
                var result = HttpHelper.Get(uri, accessToken);
                var account = JsonConvert.DeserializeObject<AADAccounts>(result).Value.First();
                accounts.Add(account);
            }
            return accounts;
        }

        public AADAccount GetGroupsByAadId(string tenantId, string groupAadId)
        {
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);

            if (profiles == null || profiles.Count == 0)
            {
                mLogger.Warn($"No profiles found from AOS with customer Id: {tenantId}");
                return null;
            }

            foreach (var profile in profiles)
            {
                mLogger.Debug($"Record search group used aos app: [{profile.Id} - {profile.Type}]");

                var accessToken = GetAccessToken(profile);
                if (accessToken == null)
                {
                    mLogger.Warn($"Can't get the access token with customer Id: {TenantLocalValue.LogonGroupId}, o365 tenant Id: {profile.TenantId}");
                    continue;
                }

                var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
                var uri = string.Format(GraphApiQueryGroupByIdString, graphEndPoint, ApiVersion, groupAadId, GroupSelector);
                mLogger.Info($"Get group by AAD id uri: [{uri}].");

                var result = HttpHelper.Get(uri, accessToken);
                if (result == null)
                {
                    mLogger.Warn($"Graph API returned null for group query with aadId: {groupAadId}, profile: {profile.Id}");
                    continue;
                }

                var account = JsonConvert.DeserializeObject<AADAccounts>(result).Value?.FirstOrDefault();
                if (account != null)
                {
                    mLogger.Info($"Successfully found group with AAD id: {groupAadId} from profile: {profile.Id}");
                    return account;
                }

                mLogger.Debug($"Group with AAD id: {groupAadId} not found in profile: {profile.Id}, trying next profile.");
            }

            mLogger.Warn($"Group with AAD id: {groupAadId} not found in any profile for tenant: {tenantId}");
            return null;
        }

        public AADAccount GetGroupsByIdOrGroupEmail(string tenantId, string groupAadId, string groupEmail)
        {
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);

            if (profiles == null || profiles.Count == 0)
            {
                mLogger.Warn($"No profiles found from AOS with customer Id: {tenantId}");
                return null;
            }

            foreach (var profile in profiles)
            {
                mLogger.Debug($"Record search group used aos app: [{profile.Id} - {profile.Type}]");
                var accessToken = GetAccessToken(profile);
                if (accessToken == null)
                {
                    mLogger.Warn($"Can't get the access token with customer Id: {TenantLocalValue.LogonGroupId}, o365 tenant Id: {profile.TenantId}");
                    continue;
                }
                var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
                string uri;
                if (!string.IsNullOrEmpty(groupAadId))
                {
                    try
                    {
                        uri = string.Format(GraphApiQueryGroupByIdString, graphEndPoint, ApiVersion, groupAadId, GroupSelector);
                        mLogger.Info($"Get group by AAD id uri: [{uri}].");
                        var result = HttpHelper.Get(uri, accessToken);
                        if (result != null)
                        {
                            var account = JsonConvert.DeserializeObject<AADAccounts>(result).Value?.FirstOrDefault();
                            if (account != null)
                            {
                                mLogger.Info($"Successfully found group with AAD id: {groupAadId} from profile: {profile.Id}");
                                return account;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error($"Error occurred while getting group by AAD id: {groupAadId} from profile: {profile.Id}", ex);
                    }
                }

                if (!string.IsNullOrEmpty(groupEmail))
                {
                    try
                    {
                        uri = string.Format(GroupApiQueryGroupByMailString, graphEndPoint, ApiVersion, groupEmail, GroupSelector);
                        mLogger.Info($"Get group by email uri: [{uri}].");
                        var result = HttpHelper.Get(uri, accessToken);
                        if (result == null)
                        {
                            mLogger.Warn($"Graph API returned null for group query with email: {groupEmail}, profile: {profile.Id}");
                            continue;
                        }
                        var account = JsonConvert.DeserializeObject<AADAccounts>(result).Value?.FirstOrDefault();
                        if (account != null)
                        {
                            mLogger.Info($"Successfully found group with email: {groupEmail} from profile: {profile.Id}");
                            return account;
                        }
                    }
                    catch(Exception ex)
                    {
                        mLogger.Error($"Error occurred while getting group by email: {groupEmail} from profile: {profile.Id}", ex);
                    }
                }

                mLogger.Debug($"Group with AAD id: {groupAadId} or email: {groupEmail} not found in profile: {profile.Id}, trying next profile.");
            }

            mLogger.Warn($"Group with AAD id: {groupAadId} not found in any profile for tenant: {tenantId}");
            return null;
        }

        public List<AADAccount> GetAccountsByUserOrGroupEmails(string tenantId, List<string> emails)
        {
            try
            {
                var needQueryFromAdUsersOrGroups = new HashSet<string>(emails);
                var accounts = new List<AADAccount>();
                var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);

                if (profiles == null || profiles.Count == 0)
                {
                    mLogger.Warn($"Current tenant no config app profile.");
                    return accounts;
                }

                foreach (var profile in profiles)
                {
                    var accessToken = GetAccessTokenWithNull(profile);
                    if (accessToken == null)
                    {
                        mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                        continue;
                    }

                    var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
                    foreach (var mail in new List<string>(needQueryFromAdUsersOrGroups))
                    {
                        var uri = string.Format(GraphApiQueryUserByEmailString, graphEndPoint, ApiVersion, mail, UserSelector);
                        mLogger.Info($"Get add user uri: [{uri}].");
                        var result = HttpHelper.Get(uri, accessToken);
                        var account = JsonConvert.DeserializeObject<AADAccounts>(result).Value.FirstOrDefault();
                        if (account != null)
                        {
                            account.InviteType = Contract.Object.AccountType.User;
                            account.TenantId = profile.TenantId;
                            accounts.Add(account);
                            needQueryFromAdUsersOrGroups.Remove(mail);
                        }
                        else
                        {
                            var groupUri = string.Format(GroupApiQueryGroupByMailString, graphEndPoint, ApiVersion, mail, GroupSelector);
                            mLogger.Info($"Get sharepoint site groups uri: [{groupUri}].");
                            var groupResult = HttpHelper.Get(groupUri, accessToken);
                            var groupAccount = JsonConvert.DeserializeObject<AADAccounts>(groupResult).Value.FirstOrDefault();
                            if (groupAccount != null)
                            {
                                groupAccount.InviteType = Contract.Object.AccountType.Group;
                                groupAccount.TenantId = profile.TenantId;
                                accounts.Add(groupAccount);
                                needQueryFromAdUsersOrGroups.Remove(mail);
                            }
                        }
                    }
                }

                foreach (var account in accounts)
                {
                    if (string.IsNullOrEmpty(account.Mail))
                    {
                        account.Mail = account.UserPrincipalName;
                    }
                }
                return accounts;
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occurred while get acounts by emails. Error: {e}");
                return new List<AADAccount>();
            }
        }

        public List<AADAccount> GetGroupsByUserId(string tenantId, string userId, string office365TenantId)
        {
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
            var profile = profiles.FirstOrDefault(o => office365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                throw new Exception("RM_MA_SiteOwner_No_AppProfile");
            }

            mLogger.Debug($"Get user groups used aos app: [{profile.Id} - {profile.Type}]");

            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                throw new Exception("Can't find access token.");
            }

            var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);

            var memberOfUri = string.Format(GraphApiUserMemberOfQueryString, graphEndPoint, ApiVersion, userId, GroupSelector);
            mLogger.Info($"Get member groups by user id uri: [{memberOfUri}].");
            var memberGroups = JsonConvert.DeserializeObject<AADAccounts>(HttpHelper.Get(memberOfUri, accessToken)).Value ?? new List<AADAccount>();

            var ownedGroupsUri = string.Format(GraphApiUserOwnedGroupsQueryString, graphEndPoint, ApiVersion, userId, GroupSelector);
            mLogger.Info($"Get owned groups by user id uri: [{ownedGroupsUri}].");
            var ownedGroups = JsonConvert.DeserializeObject<AADAccounts>(HttpHelper.Get(ownedGroupsUri, accessToken)).Value ?? new List<AADAccount>();

            var mergedGroups = memberGroups
                .UnionBy(ownedGroups, a => a.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            mLogger.Info($"GetGroupsByUserId result count: member={memberGroups.Count}, owned={ownedGroups.Count}, merged={mergedGroups.Count}");
            return mergedGroups;
        }

        public List<AADAccount> GetAccountsByUserEmials(string tenantId, List<string> userEmails, string office365TenantId)
        {
            var accounts = new List<AADAccount>();
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
            var profile = profiles.FirstOrDefault(o => office365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                throw new Exception("RM_MA_SiteOwner_No_AppProfile");
            }

            mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                throw new Exception($"Can't find accss token.");
            }
            var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
            foreach (var userEmail in userEmails)
            {
                var uri = string.Format(GraphApiQueryUserByEmailString, graphEndPoint, ApiVersion, userEmail, UserSelector);
                mLogger.Info($"Get sharepoint site owners uri: [{uri}].");
                var result = HttpHelper.Get(uri, accessToken);
                var values = JsonConvert.DeserializeObject<AADAccounts>(result).Value;
                if (values.Any())
                {
                    accounts.Add(values.First());
                }
                else
                {
                    mLogger.Warn($"Get account by user email failed, current tenant id : {TenantLocalValue.LogonGroupId}");
                }
            }

            if (accounts.Count == 0)
            {
                mLogger.Error($"No site owner found for all provided emails, current tenant id : {TenantLocalValue.LogonGroupId}");
                throw new Exception("RM_MA_SiteOwner_NoSiteOwner");
            }
            return accounts;
        }

        /// <summary>
        /// now it only search the accounts from the O365 tenants.
        /// </summary>
        /// <param name="tenantId">customer id in AOS</param>
        /// <param name="searchString"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public List<AADAccount> SearchAccounts(string tenantId, string searchString, int top = 20, bool onlyIncludeAAdUser = false)
        {
            List<AADAccount> results = new List<AADAccount>();
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
            if (profiles.Count == 0)
            {
                mLogger.Warn($"No profiles found from AOS with customer Id : {tenantId}");
            }
            var o365TenantIds = profiles.Select(s => s.TenantId).ToList().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var leftCount = top;
            foreach (var o365TenantId in o365TenantIds)
            {
                try
                {
                    var profile = profiles.First(o => o365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

                    mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

                    if (onlyIncludeAAdUser)
                    {
                        var temp = SearchAccountUsers(profile, searchString, leftCount);
                        results.AddRange(temp);
                    }
                    else
                    {
                        var temp = SearchAccounts(profile, searchString, leftCount);
                        results.AddRange(temp);
                    }
                    leftCount -= results.Count();
                    if (leftCount <= 0) break;
                }
                catch (Exception e)
                {
                    mLogger.Warn($"An error occurred while search user/group from Azure AD with O365 tenant id : {o365TenantId}. error : {e.ToString()}");
                }
            }

            return results;
        }

        public List<AADAccount> SearchAccounts4FSConnection(string tenantId, string searchString, int top = 20)
        {
            List<AADAccount> results = new List<AADAccount>();
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
            if (profiles.Count == 0)
            {
                mLogger.Warn($"No profiles found from AOS with customer Id : {tenantId}");
            }
            var o365TenantIds = profiles.Select(s => s.TenantId).ToList().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var leftCount = top;
            foreach (var o365TenantId in o365TenantIds)
            {
                try
                {
                    var profile = profiles.First(o => o365TenantId.Equals(o.TenantId, StringComparison.OrdinalIgnoreCase));

                    mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

                    var temp = SearchAccounts(profile, searchString, leftCount);
                    results.AddRange(temp);
                    leftCount -= results.Count();
                    if (leftCount <= 0) break;
                }
                catch (Exception e)
                {
                    mLogger.Warn($"An error occurred while search user/group from Azure AD with O365 tenant id : {o365TenantId}. error : {e.ToString()}");
                }
            }

            return results;
        }
        public List<AADAccount> SearchAccounts(string tenantId, string searchString, string appProfileId, int top = 20)
        {
            try
            {
                List<AADAccount> results = new List<AADAccount>();
                var profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
                var profile = profiles.First(item => item.Id.Trim().Equals(appProfileId.Trim(), StringComparison.OrdinalIgnoreCase));
            
                mLogger.Debug($"Record search user used aos app: [{profile.Id} - {profile.Type}]");

                var users = SearchAccountUsers(profile, searchString, top);
                results.AddRange(users);
                return results;
            }
            catch (Exception e)
            {
                mLogger.Warn($"An error occurred while search user/group from Azure AD with profile id : {appProfileId}. error : {e.ToString()}");
            }

            return new List<AADAccount>();
        }

        private List<AADAccount> SearchAccounts(CloudAos.AppProfileInfo profile, string searchString, int top)
        {
            List<AADAccount> results = new List<AADAccount>();
            int groupsCount = top / 2;
            int usersCount = top - groupsCount;



            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                return results;
            }

            mLogger.Info($"Try to search users with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
            var users = SearchAccounts(profile, accessToken, searchString, top, false, Contract.Object.AccountType.User);
            if (users.Count < usersCount)
            {
                groupsCount = top - users.Count;
            }
            List<AADAccount> groups = new List<AADAccount>();
            if (groupsCount > 0)
            {
                mLogger.Info($"Try to search groups with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                groups = SearchAccounts(profile, accessToken, searchString, groupsCount, false, Contract.Object.AccountType.Group);
                usersCount = Math.Min(top - groups.Count, users.Count);
            }

            results.AddRange(users.GetRange(0, usersCount));
            results.AddRange(groups);
            results = results.OrderBy(a => a.DisplayName).ToList();
            return results;
        }

        private List<AADAccount> SearchAccountUsers(CloudAos.AppProfileInfo profile, string searchString, int top)
        {
            List<AADAccount> results = new List<AADAccount>();
            var accessToken = GetAccessToken(profile);
            if (accessToken == null)
            {
                mLogger.Warn($"Can't get the access token with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
                return results;
            }

            mLogger.Info($"Try to search users with customer Id : {TenantLocalValue.LogonGroupId}, o365 tenant Id : {profile.TenantId}");
            var users = SearchAccounts(profile, accessToken, searchString, top, false, Contract.Object.AccountType.User);
            results.AddRange(users);
            results = results.OrderBy(a => a.DisplayName).ToList();
            return results;
        }

        /// <summary>
        /// Firstly try to get token from cache, then get from AOS token service if it doesn't exists in cache, 
        /// if still not succussful, will finally get token directly from Azure AD with Graph api.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private string GetAccessToken(CloudAos.AppProfileInfo profile)
        {
            var token = CacheService.Get(CacheNamespace.O365AccessToken, profile.TenantId);
            if (!string.IsNullOrEmpty(token)) return token;


            var tokenResult = RMAosApiClient.GetO365AccessToken(profile); //get token from AOS
            if (tokenResult != null)
            {
                token = tokenResult.AccessToken;

                CacheService.Set(CacheNamespace.O365AccessToken, profile.TenantId, token, tokenResult.ExpiresOn.UtcDateTime);
            }
            else
            {
                //get token with graph client api
                throw new Exception("Error occurred while getting graph token.");
                //var graphToken = GetAccessTokenWithGraphAPI(profile);
                //if (graphToken != null)
                //{
                //    token = graphToken.AccessToken;
                //    CacheService.Set(CacheNamespace.O365AccessToken, profile.TenantId, token, graphToken.ExpiresOn.UtcDateTime);
                //}
            }
            return token;
        }
        private string GetAccessTokenWithNull(CloudAos.AppProfileInfo profile)
        {
            var token = CacheService.Get(CacheNamespace.O365AccessToken, profile.TenantId);
            if (!string.IsNullOrEmpty(token)) return token;


            var tokenResult = RMAosApiClient.GetO365AccessToken(profile); //get token from AOS
            if (tokenResult != null)
            {
                token = tokenResult.AccessToken;

                CacheService.Set(CacheNamespace.O365AccessToken, profile.TenantId, token, tokenResult.ExpiresOn.UtcDateTime);
            }
            else
            {
                //get token with graph client api
                mLogger.Error("Error occurred while getting graph token.");
                return null;
                //var graphToken = GetAccessTokenWithGraphAPI(profile);
                //if (graphToken != null)
                //{
                //    token = graphToken.AccessToken;
                //    CacheService.Set(CacheNamespace.O365AccessToken, profile.TenantId, token, graphToken.ExpiresOn.UtcDateTime);
                //}
            }
            return token;
        }

        /// <summary>
        /// will get profiles from cache first, if not found in cache, then will get from AOS and then cached.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        private List<CloudAos.AppProfileInfo> GetProfiles(string tenantId)
        {
            List<CloudAos.AppProfileInfo> profiles = new List<CloudAos.AppProfileInfo>();
            try
            {
                //should get from cache first.....
                var profileJson = CacheService.Get(CacheNamespace.AuthenticationProfiles, tenantId);
                if (!string.IsNullOrEmpty(profileJson))
                {
                    return JsonConvert.DeserializeObject<List<CloudAos.AppProfileInfo>>(profileJson);
                }

                //get from AOS
                profiles = RMAosApiClient.GetHasADPermissionProfiles(tenantId);
                if (profiles.Count > 0)
                {
                    CacheService.Set(CacheNamespace.AuthenticationProfiles, tenantId, JsonConvert.SerializeObject(profiles), DateTime.UtcNow.AddMinutes(30));
                }
            }
            catch (Exception e)
            {
                mLogger.Warn($"No profiles found with customer Id : {tenantId}, error: {e.ToString()}");
            }

            return profiles;
        }

        private List<AADAccount> SearchAccounts(CloudAos.AppProfileInfo profile, string accessToken, string searchString, int pagingSize, bool getAll, Contract.Object.AccountType type)
        {
            List<AADAccount> results = new List<AADAccount>();
            try
            {
                string upnEncode = System.Web.HttpUtility.UrlEncode(searchString);
                var graphEndPoint = EndpointUtil.GetGraphEndpoint(profile.AADEnvironment);
                string uri =
                    type == Contract.Object.AccountType.User ?
                    string.Format(GraphApiUserQueryString, graphEndPoint, ApiVersion, pagingSize, upnEncode, UserSelector)
                    : string.Format(GraphApiGroupQueryString, graphEndPoint, ApiVersion, pagingSize, upnEncode, GroupSelector);
                //mLogger.Info($"Search uri: {uri}");

                string r = HttpHelper.Get(uri, accessToken);
                AADAccounts users = JsonConvert.DeserializeObject<AADAccounts>(r);
                results.AddRange(users.Value);

                while (getAll && users.OdataNextLink != null)
                {
                    users = GetNextPageSearchUserBySearchstr(accessToken, uri, users.Skiptoken);
                    if (users.Value != null)
                    {
                        results.AddRange(users.Value);
                    }
                }

                results.ForEach(a =>
                {
                    a.InviteType = type;
                    a.TenantId = profile.TenantId;
                    //由于user在登录时，会重设display name为First Name + Last Name的形式，为了保持一致，此处不使用Azure AD中的Display Name属性
                    if (string.IsNullOrEmpty(a.DisplayName) && type == Contract.Object.AccountType.User)
                    {
                        a.DisplayName = RMAOSConvertUtil.GetUserName(a.GivenName, a.SurName, a.UserPrincipalName) ?? RMAOSConvertUtil.GetUserName(a.GivenName, a.SurName, a.Mail);
                    }
                });
                mLogger.Info("SearchResultCount {0}", results.Count);
            }
            catch (Exception ex)
            {
                mLogger.Error("CAA UserWrapper SearchUser Exception: searchString {0}, Exception {1}", searchString, ex);
            }
            return results;
        }

        private AADAccounts GetNextPageSearchUserBySearchstr(string accessToken, string uri, string skipToken)
        {
            try
            {
                StringBuilder u = new StringBuilder(uri);
                u.Append("&$skiptoken=").Append(skipToken);
                string r = HttpHelper.Get(u.ToString(), accessToken);
                AADAccounts gl = JsonConvert.DeserializeObject<AADAccounts>(r);
                return gl;
            }
            catch (Exception e)
            {
                mLogger.Error("GetNextPageSearchUserBySearchstr Exception:{0}", e);
                return null;
            }
        }
    }

}
