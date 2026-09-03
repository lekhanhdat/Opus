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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AOS;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AvePoint.RA.Common.AOS;

namespace AvePoint.RA.Service.Services.AzureADAPIWrapper
{
    public class UserWrapperService : IUserWrapperService
    {
        private static RALogger mLogger = RALogger.GetInstance(typeof(UserWrapperService));


        private const string GraphApiUserQueryString = @"https://graph.windows.net/{0}/users?api-version={1}&$top={2}&$filter=startswith(userPrincipalName,'{3}') or startswith(displayName,'{3}') or startswith(givenName,'{3}') or startswith(surname,'{3}')";
        private const string GraphApiGroupQueryString = @"https://graph.windows.net/{0}/groups?api-version={1}&$top={2}&$filter=startswith(displayName,'{3}') and securityEnabled eq true";
        private const string AuthString = "https://login.microsoftonline.com/";
        private const string AuthStringCommon = "https://login.windows.net/common/";
        private const string ResourceUrl = "https://graph.windows.net";
        private const string ApiVersion = "1.6";
        private const int PagingSize = 999;

        public List<Account> SearchAllAccounts(string tenantId, string searchString)
        {
            List<Account> results = new List<Account>();
            results.AddRange(SearchAccounts(tenantId, searchString, PagingSize, true, Contract.Object.AccountType.User));
            results.AddRange(SearchAccounts(tenantId, searchString, PagingSize, true, Contract.Object.AccountType.Group));
            return results;
        }

        public Account SearchSingleAccount(string tenantId, string searchString)
        {
            var account = SearchAccount(tenantId, searchString, Contract.Object.AccountType.User);
            if (account != null)
            {
                return account;
            }
            else
            {
                return SearchAccount(tenantId, searchString, Contract.Object.AccountType.Group);
            }
        }

        //top must Leq PagingSize: 999
        public List<Account> SearchAccounts(string tenantId, string searchString, int top)
        {
            int groupsCount = top / 2;
            int usersCount = top - groupsCount;
            var users = SearchAccounts(tenantId, searchString, top, false, Contract.Object.AccountType.User);
            if(users.Count < usersCount)
            {
                groupsCount = top - users.Count;
            }

            var groups = SearchAccounts(tenantId, searchString, groupsCount, false, Contract.Object.AccountType.Group);
            usersCount = Math.Min(top - groups.Count, users.Count);

            List<Account> results = new List<Account>();
            results.AddRange(users.GetRange(0, usersCount));
            results.AddRange(groups);
            results = results.OrderBy(a => a.DisplayName).ToList();
            return results;
        }

        public bool CheckAccountIsExist(string tenantId, string userPrincipalName)
        {
            return FindAccount(tenantId, userPrincipalName) != null;
        }

        private List<Account> SearchAccounts(string tenantId, string searchString, int pagingSize, bool getAll, Contract.Object.AccountType type)
        {
            List<Account> results = new List<Account>();
            try
            {
                string upnEncode = System.Web.HttpUtility.UrlEncode(searchString);
                string uri = string.Format(
                    (type == Contract.Object.AccountType.User ? GraphApiUserQueryString : GraphApiGroupQueryString)
                    , tenantId, ApiVersion, pagingSize, upnEncode);
                var accessToken = AosApiService.GetSPOnlineAccessTokenByTenantId(tenantId);
                string r = HttpHelper.Get(uri, accessToken);
                Accounts users = JsonConvert.DeserializeObject<Accounts>(r);
                results.AddRange(users.Value);

                while (getAll && users.OdataNextLink != null)
                {
                    users = GetNextPageSearchUserBySearchstr(accessToken, uri, users.Skiptoken);
                    if (users.Value != null)
                    {
                        results.AddRange(users.Value);
                    }
                }

                results.ForEach(a => {
                    a.InviteType = type.ToString();
                    //a.tenantId = tenantId;
                });
            }
            catch (Exception ex)
            {
                mLogger.Error("CAA UserWrapper SearchUser Exception: searchString {0}, Exception {1}", searchString, ex);
            }
            return results;
        }

        public Account GetGroupByObjectId(string tenantId, string objectID)
        {
            return FindAccount(tenantId, objectID, Contract.Object.AccountType.Group);
        }

        public Account GetUserByObjectId(string tenantId, string objectID)
        {
            return FindAccount(tenantId, objectID, Contract.Object.AccountType.User);
        }

        public HashSet<string> GetAllUserEMailsFromGroup(string tenantId, string objectID)
        {
            HashSet<string> emails = new HashSet<string>();
            var accounts = GetAllAccountsFromGroup(tenantId, objectID);
            accounts.ForEach(a => {
                //if (!string.IsNullOrEmpty(a.mail))
                //{
                //    emails.Add(a.mail);
                //}
            });
            return emails;
        }


        private Account SearchAccount(string tenantId, string searchString, Contract.Object.AccountType type)
        {
            var results = SearchAccounts(tenantId, searchString, 1, false, type);
            if(results != null && results.Count > 0)
            {
                //results[0].tenantId = tenantId;
                return results[0];
            }
            return null;
        }

        private Accounts GetNextPageSearchUserBySearchstr(string accessToken, string uri, string skipToken)
        {
            try
            {
                StringBuilder u = new StringBuilder(uri);
                u.Append("&$skiptoken=").Append(skipToken);
                string r = HttpHelper.Get(u.ToString(), accessToken);
                Accounts gl = JsonConvert.DeserializeObject<Accounts>(r);
                return gl;
            }
            catch (Exception e)
            {
                mLogger.Error("GetNextPageSearchUserBySearchstr Exception:{0}", e);
                return null;
            }
        }

        private Account FindAccount(string tenantId, string upnOrObjectId, Contract.Object.AccountType type = Contract.Object.AccountType.User)
        {
            try
            {
                string upnEncode = System.Web.HttpUtility.UrlEncode(upnOrObjectId);
                string uri = null;
                if(type == Contract.Object.AccountType.User)
                {
                    uri = string.Format("https://graph.windows.net/{0}/users/{1}?api-version={2}",
                        tenantId, upnEncode, ApiVersion);
                }
                else
                {
                    uri = string.Format("https://graph.windows.net/{0}/groups/{1}?api-version={2}&$filter=securityEnabled eq true",
                        tenantId, upnEncode, ApiVersion);
                }
                
                var accessToken = AosApiService.GetSPOnlineAccessTokenByTenantId(tenantId);
                string r = HttpHelper.Get(uri, accessToken);
                Account user = JsonConvert.DeserializeObject<Account>(r);
                if(user != null)
                {
                    //user.tenantId = tenantId;
                }
                return user;
            }
            catch (Exception ex)
            {
                mLogger.Error("UserWrapper FindAccount Exception: userPrincipalName or object id: {0}, Exception {1}.", upnOrObjectId, ex.ToString());
            }
            return null;
        }

        private List<Account> GetAllAccountsFromGroup(string tenantId, string groupObjectID)
        {
            List<Account> accounts = new List<Account>();
            try
            {
                string uri = string.Format("https://graph.windows.net/{0}/groups/{1}/members?api-version={2}",
                    tenantId, groupObjectID, ApiVersion);
                var accessToken = AosApiService.GetSPOnlineAccessTokenByTenantId(tenantId);
                string r = HttpHelper.Get(uri, accessToken);
                Accounts users = JsonConvert.DeserializeObject<Accounts>(r);
                if (users != null)
                {
                    users.Value.ForEach(u => {
                        if (u.InviteType.Equals("User", StringComparison.OrdinalIgnoreCase))
                        {
                            accounts.Add(u);
                        }
                        else if (u.InviteType.Equals("Group", StringComparison.OrdinalIgnoreCase))
                        {
                            accounts.AddRange(GetAllAccountsFromGroup(tenantId, u.UserId));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("UserWrapper GetAllAccountsFromGroup Exception. tenantId: {0}, userPrincipalName or object id: {1}, Exception {2}.", 
                    tenantId, groupObjectID, ex.ToString());
            }
            return accounts;
        }
    }
}
