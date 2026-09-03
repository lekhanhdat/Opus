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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    using AvePoint.GCommon.Utility.I18N;
    using System.Text.RegularExpressions;
    using System.Diagnostics.CodeAnalysis;
    using System.DirectoryServices.AccountManagement;
    using System.DirectoryServices.ActiveDirectory;
    #endregion

    /// <summary>
    /// 重构时遵循的几个原则：
    /// 1，此类得方法主要涉及DocAve中的people picker控件的check和browser，默认不初始化环境及所有相关的Domain信任关系
    /// 2，AD相关的check/browser agent端优先走SPUtility逻辑，check不到才调用此类（server端由于没有API，所以直接调用此类）
    /// 3，如果输入的user含有doamin name（目前以\\判断），那么直接根据name初始化DiectoryEntry进行search
    /// 4，如果没有含有domain，通过Global Catalog实例获取当前forest的DirectorySearcher，还不行再继续实例他的trust forest，并加到全局变量
    /// 5，CA模块单独用到了两个方法，由于这两个方法参数已经是loginName，所以可以直接实例化domain,不再采用单例模式，而在外围对结果进行cache ：
    /// #1）GetMembersInGroup（）展开ADGroup获得members 
    /// #2）GetParentGroupCollection（） 获取Domain User的所有parent Doamin Group
    /// </summary>

    public class LDAPLookup : IAccountManager
    {
        static AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string BUILDTIN_DOMAIN = "builtIn";
        private static object lockObj = new object();
        private static Dictionary<string, string> mSpecialChars = new Dictionary<string, string>();
        public static Dictionary<string, string> SpecialChars
        {
            get
            {
                if (mSpecialChars.Count == 0)
                {
                    mSpecialChars.Add("\\", "\\5c");
                    mSpecialChars.Add("*", "\\2a");
                    mSpecialChars.Add("(", "\\28");
                    mSpecialChars.Add(")", "\\29");
                    mSpecialChars.Add("/", "\\2f");
                    //    mSpecialChars.Add(" ", "\\00");
                }
                return mSpecialChars;
            }
        }
        static LDAPLookup ldapLookup;
        #region Construct and Init

        public static LDAPLookup GetInstance()
        {
            lock (lockObj)
            {
                if (ldapLookup == null)
                {
                    ldapLookup = new LDAPLookup();
                }
            }
            return ldapLookup;
        }
        #endregion

        #region IAccountManager

        public bool IsEnable()
        {
            return true;
        }

        public bool CheckUser(string username)
        {
            return CheckUser(username, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
        }

        public bool CheckUser(string username, AccountSearchFlag flag)
        {
            string tmpLogin = null;
            return ExtractUserName(username, ref tmpLogin, flag);
        }

        public bool ExtractUserName(string username, ref string loginName)
        {
            return ExtractUserName(username, ref loginName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
        }

        public bool ExtractUserName(string username, ref string loginName, AccountSearchFlag flag)
        {
            UserDetail detail = GetUser(username, flag);
            if (detail != null)
            {
                loginName = detail.LoginName;
                return true;
            }

            return false;
        }

        public UserDetail GetUser(string username)
        {
            return GetUser(username, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
        }

        public UserDetail GetUser(string username, AccountSearchFlag flag)
        {
            UserDetail detail = null;
            SearchResult result = null;
            string domainName = string.Empty;
            string samAccountName = GetAccountName(username, ref domainName);
            string filter = GetSearchFilter(samAccountName, flag, false, false);
            if (!string.IsNullOrEmpty(filter))
            {
                result = CheckAccount(filter, ref domainName);
            }
            if (result != null)
            {
                //people picker not include disabled users
                detail = SearchResultToDetail(result, domainName, flag);
            }
            if (detail == null)
            {
                detail = GetLocalUserInfo(username, flag);
            }
            return detail;
        }

        public List<UserDetail> GetUsers(string username)
        {
            return GetUsers(username, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser);
        }

        public List<UserDetail> GetUsers(string username, AccountSearchFlag flag, int findUserQuota = 200)
        {
            List<UserDetail> users = new List<UserDetail>();
            string domainName = string.Empty;
            string samAccountName = GetAccountName(username, ref domainName);
            string filter = GetSearchFilter(samAccountName, flag, true, false);
            return SearchAccount(filter, flag, ref domainName, findUserQuota);
        }

        public bool IsMemeberAlive(string loginName)
        {
            switch (loginName.ToLowerInvariant())
            {
                case "nt authority\\local service":
                case "nt authority\\authenticated users":
                case "sharepoint\\system":
                case "nt authority\\system":
                    return true;
                default:
                    break;
            }
            if (CheckUser(loginName))
            {
                return true;
            }
            return GetLocalUserInfo(loginName) != null;
        }

        #endregion

        #region Tool Method

        /// <summary>
        /// depend on init ldap complete, will loop all directorysearcher
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private SearchResult CheckAccount(string filter, ref string domainName)
        {
            if (!string.IsNullOrEmpty(domainName))
            {
                return CheckAccount(filter, domainName);
            }
            Dictionary<string, DirectorySearcher> searchers = DirectorySearcherFactory.GlobalSearcherCollection;
            foreach (KeyValuePair<string, DirectorySearcher> pair in searchers)
            {
                try
                {
                    SearchResult result = CheckAccount(pair.Value, filter);
                    if (result != null)
                    {
                        domainName = pair.Key;
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(ex.ToString());
                }
            }
            return null;
        }

        /// <summary>
        /// get directoryentry directly by domain name, not depend on ldap init
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="domainName"></param>
        /// <returns></returns>
        private SearchResult CheckAccount(string filter, string domainName)
        {
            try
            {
                DirectorySearcher searcherInfo = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
                {
                    SearchResult result = CheckAccount(searcherInfo, filter);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                mLog.Warn("An error occurred while checking account. Reason:{0}", e.ToString());
                mLog.Info("Begin to clear domain searcher cache.");
                DirectorySearcherFactory.ClearDomainSearcherCache();
                DirectorySearcher searcherInfo = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
                {
                    SearchResult result = CheckAccount(searcherInfo, filter);
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                mLog.Warn(ex.ToString());
            }
            return null;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "cn")]
        private SearchResult CheckAccount(DirectorySearcher searcher, string filter)
        {
            try
            {
                lock (searcher)  //DOC-37394 If searcher is mDomainSearcher, it will have conflict in multithreading, because mDomainSearcher is static.
                {
                    searcher.Filter = filter;
                    searcher.PropertiesToLoad.Clear();
                    searcher.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn", "displayName", "mail" });
                    return searcher.FindOne();
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                throw;
                //searcher = DirectorySearcherFactory.GetLDAPSearchersByDomainName(searcher.d);
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while checking account. Reason:{0}", ex.ToString());
                return null;
            }
        }

        private List<UserDetail> SearchAccount(string filter, AccountSearchFlag flag, ref string domainName, int findUserQuota = 200)
        {
            if (!string.IsNullOrEmpty(domainName))
            {
                return SearchAccount(filter, flag, domainName, findUserQuota);
            }
            Dictionary<string, DirectorySearcher> results = DirectorySearcherFactory.GlobalSearcherCollection;
            List<UserDetail> users = new List<UserDetail>();
            foreach (KeyValuePair<string, DirectorySearcher> pair in results)
            {
                List<SearchResult> srs = SearchAccount(pair.Value, filter, findUserQuota);
                foreach (SearchResult result in srs)
                //people picker not include disabled users
                {
                    UserDetail detail = SearchResultToDetail(result, pair.Key, flag);
                    if (detail != null)
                    {
                        users.Add(detail);
                    }
                }
            }
            return users;
        }

        private List<UserDetail> SearchAccount(string filter, AccountSearchFlag flag, string domainName, int findUserQuota = 200)
        {
            List<UserDetail> users = new List<UserDetail>();
            List<SearchResult> results = new List<SearchResult>();
            DirectorySearcher searcherInfo = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
            {
                List<SearchResult> srs = SearchAccount(searcherInfo, filter, findUserQuota);
                foreach (SearchResult result in srs)
                {
                    //people picker not include disabled users
                    UserDetail detail = SearchResultToDetail(result, domainName, flag);
                    if (detail != null)
                    {
                        users.Add(detail);
                    }
                }
            }
            return users;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SearchAccount is unmodifiable as the cause of being referenced.")]
        private List<SearchResult> SearchAccount(DirectorySearcher searcher, string filter, int findUserQuota = 200)
        {
            List<SearchResult> returnResults = new List<SearchResult>();
            try
            {
                searcher.Filter = filter;
                searcher.SizeLimit = findUserQuota + 1;
                searcher.PageSize = 1000;
                searcher.PropertiesToLoad.Clear();
                searcher.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn", "mail", "displayName" });
                using (SearchResultCollection results = searcher.FindAll())
                {
                    foreach (SearchResult result in results)
                    {
                        returnResults.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while search account. Reason:{0}", ex.ToString());
            }
            return returnResults;
        }

        /// <summary>
        /// 获取登录用户账户，用于组装filter，DomainA\\user1将返回user1， user2将返回user2
        /// </summary>
        /// <param name="name"></param>
        /// <param name="domainName"></param>
        /// <returns></returns>
        private string GetAccountName(string name, ref string domainName)
        {
            name = UserLoginNamePrefix.RemoveLoginNamePrifix(name);
            name = GetRealAccountName(name);
            string samAccountName = name;
            if (name.IndexOf('\\') != -1)
            {
                domainName = name.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (BUILDTIN_DOMAIN.Equals(domainName, StringComparison.OrdinalIgnoreCase))
                {
                    domainName = string.Empty;
                }
                samAccountName = name.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            foreach (string specialChar in SpecialChars.Keys)
            {
                samAccountName = samAccountName.Replace(specialChar, SpecialChars[specialChar]);
            }
            return samAccountName;
        }

        private string GetSearchFilter(string name, AccountSearchFlag flag, bool isSearch)
        {
            return GetSearchFilter(name, flag, isSearch, true);
        }

        private string GetSearchFilter(string name, AccountSearchFlag flag, bool isSearch, bool includeDistributionGroup)
        {
            StringBuilder filter = new StringBuilder();
            if ((flag & AccountSearchFlag.IncludeADGroup) != AccountSearchFlag.None)
            {
                if (includeDistributionGroup)
                {
                    filter.Append("(&(objectCategory=group)(objectClass=group))");
                }
                else
                {
                    filter.Append("(&(&(objectCategory=group)(objectClass=group))(groupType:1.2.840.113556.1.4.803:=2147483648))");//搜索所有的security group
                }
            }
            string formatName = name;

            if ((flag & AccountSearchFlag.IncludeADUser) != AccountSearchFlag.None)
            {
                if (filter.Length > 0)
                {
                    filter.Insert(0, '|');
                }
                string userFilter = "(&(objectCategory=person)(objectClass=user))";
                filter.Append(userFilter);
            }

            if (isSearch)
            {
                formatName = string.Format("{0}*", formatName).Replace("**", "*");
            }
            else if (formatName.Contains("*"))//DOC-39896, A*B should not check any accounts.
            {
                return null;
            }
            string mailAndLogOnname = formatName;
            if (!mailAndLogOnname.Contains("@"))
            {
                mailAndLogOnname = string.Format("{0}@*", mailAndLogOnname);
            }
            //去掉了first name(givenname)，last name(sn)和initials(initials)
            //support upn check/find
            return string.Format("(&({0})(|(|(|(|(|(cn={1})(samAccount={1}))(samAccountName={1}))(mail={2}))(userPrincipalName={2}))(displayName={1})))", filter.ToString(), formatName, mailAndLogOnname);
        }

        /// <summary>
        /// this result must contains these properties:objectSid;groupType;userAccountControl;
        /// displayName;cn;
        /// mail(not needed)
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private UserDetail SearchResultToDetail(SearchResult result, string domainName, AccountSearchFlag flag)
        {
            string commonName = result.Properties["cn"][0].ToString();
            string realName = GetRealAccountName(commonName);
            if (!realName.Equals(commonName, StringComparison.OrdinalIgnoreCase)) //单向信任域，accountname为sid
            {
                return GetUser(realName, flag);
            }
            else
            {
                return GetDetailFromSearchResult(result, domainName, flag);
            }
        }

        private UserDetail SearchResultToDetail(SearchResult result, string domainName)
        {
            //search all users in AD by default
            return SearchResultToDetail(result, domainName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeADDisabledUsers);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ValidateSearchResult is unmodifiable as the cause of being referenced.")]
        private bool ValidateSearchResult(SearchResult result, UserDetail detail)
        {
            bool flag = true;
            if (string.IsNullOrEmpty(detail.LoginName) || string.IsNullOrEmpty(detail.DisplayName))
            {
                flag = false;
                string detailInfo = string.Empty;
                string objectSidInfo = string.Empty;
                string samAccountNameInfo = string.Empty;
                string cnInfo = string.Empty;
                if (result.Properties.Contains("objectSid"))
                {
                    objectSidInfo = string.Format("has {0} value", result.Properties["objectSid"].Count);
                }
                if (result.Properties.Contains("samaccountname"))
                {
                    foreach (object obj in result.Properties["samaccountname"])
                    {
                        samAccountNameInfo = samAccountNameInfo + obj.ToString() + ";";
                    }
                }
                if (result.Properties.Contains("cn"))
                {
                    foreach (object obj in result.Properties["cn"])
                    {
                        cnInfo = cnInfo + obj.ToString() + ";";
                    }
                }
                detailInfo = string.Format("loginName:{0} displayName:{1} objectSid:{2} samAccountName:{3} cn:{4} path:{5}",
                    detail.LoginName, detail.DisplayName, objectSidInfo, samAccountNameInfo, cnInfo, result.Path);
                mLog.Debug("User's name is empty, will not return as result. Detail info is, {0}", detailInfo);
            }
            return flag;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "GetDetailFromSearchResult is unmodifiable as the cause of being referenced.")]
        protected virtual UserDetail GetDetailFromSearchResult(SearchResult result, string domainName, AccountSearchFlag flag)
        {
            UserDetail detail = new UserDetail();
            try
            {
                bool lookUpOK = false;
                if (result.Properties.Contains("objectSid") && result.Properties["objectSid"].Count > 0)
                {
                    Win32Native.SID_NAME_USE sidUse = Win32Native.SID_NAME_USE.SidTypeUnknown;
                    StringBuilder name = new StringBuilder(0x100);
                    StringBuilder domain = new StringBuilder(0x100);
                    int cbDomainName = 0x100;
                    int cbName = 0x100;
                    byte[] userId = (byte[])result.Properties["objectSid"][0];
                    if (Win32Native.LookupAccountSid(null, userId, name, ref cbName, domain, ref cbDomainName, ref sidUse))
                    {
                        lookUpOK = true;
                        detail.LoginName = string.Format("{0}\\{1}", domain.ToString(), name.ToString());
                    }
                }
                if (!lookUpOK && result.Properties.Contains("samaccountname") && result.Properties["samaccountname"].Count > 0)
                {
                    string accountName = result.Properties["samaccountname"][0].ToString();
                    detail.LoginName = string.Format("{0}\\{1}", domainName, accountName);
                }
                detail.AccountType = AccountType.ADUser;
                if (result.Properties.Contains("userAccountControl") && result.Properties["userAccountControl"].Count != 0)
                {
                    int control = (int)result.Properties["userAccountControl"][0];
                    if ((control & 2) != 0)
                    {
                        detail.AccountState = AccountStatus.Deactived;
                        if ((flag & AccountSearchFlag.IncludeADDisabledUsers) == AccountSearchFlag.None)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        detail.AccountState = AccountStatus.Actived;
                    }
                }
                if (result.Properties.Contains("groupType"))
                {
                    detail.AccountType = AccountType.ADGroup;
                    detail.AccountState = AccountStatus.Actived;
                }
                if (result.Properties.Contains("cn")) //common name==display name
                {
                    detail.DisplayName = result.Properties["cn"][0].ToString();
                }
                if (result.Properties.Contains("mail") && result.Properties["mail"].Count != 0)//should display this property when find user
                {
                    detail.Email = result.Properties["mail"][0].ToString();
                }
                if (!ValidateSearchResult(result, detail))
                {
                    return null;
                }
            }
            catch (Exception ee)
            {
                mLog.Warn(ee.ToString());
            }
            return detail;
        }

        private string GetRealAccountName(string user)
        {
            string pattern = @"^[sS]-1-\d-\d*-\d*-\d*-\d*-\d*$";
            if (Regex.IsMatch(user, pattern))
            {
                mLog.Info(string.Format("before translate the user name is {0}", user));
                try
                {
                    user = new System.Security.Principal.SecurityIdentifier(user).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                }
                catch (Exception ee)
                {
                    mLog.Warn(ee.ToString());
                }
                mLog.Info(string.Format("after translate the user name is {0}", user));
            }
            return user;
        }
        #endregion

        #region For Local User
        public static UserDetail GetLocalUserInfo(string username)
        {
            return GetLocalUserInfo(username, AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "GetLocalUserInfo is unmodifiable as the cause of being referenced.")]
        public static UserDetail GetLocalUserInfo(string username, AccountSearchFlag flag)
        {
            username = UserLoginNamePrefix.RemoveLoginNamePrifix(username);
            if ((flag & (AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser)) == AccountSearchFlag.None)
            {
                return null;
            }
            if (username.IndexOf('\\') > 0)
            {
                if (!username.Split('\\')[0].Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                username = username.Substring(username.LastIndexOf('\\') + 1);
            }
            DirectoryEntry group = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

            if ((flag & AccountSearchFlag.IncludeLocalUser) != AccountSearchFlag.None)
            {
                try
                {
                    DirectoryEntry user = group.Children.Find(username, "User");
                    if (user != null)
                    {
                        bool disable = (bool)user.InvokeGet("AccountDisabled");
                        if (!disable)
                        {
                            UserDetail detail = new UserDetail();
                            detail.LoginName = string.Format("{0}\\{1}", Environment.MachineName, username);
                            detail.DisplayName = user.Invoke("Get", new object[] { "FullName" }).ToString();
                            if (string.IsNullOrEmpty(detail.DisplayName))
                            {
                                detail.DisplayName = detail.LoginName;
                            }
                            detail.AccountType = AccountType.LocalUser;
                            return detail;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn(string.Format("An error occurred while getting local user, user name : {0} reason : {1} ", username, e.ToString()));
                }
            }

            if ((flag & AccountSearchFlag.IncludeLocalGroup) != AccountSearchFlag.None)
            {
                try
                {
                    DirectoryEntry user = group.Children.Find(username, "Group");
                    if (user != null)
                    {
                        UserDetail detail = new UserDetail();
                        detail.LoginName = string.Format("{0}\\{1}", Environment.MachineName, username);
                        detail.DisplayName = detail.LoginName;
                        detail.AccountType = AccountType.LocalGroup;
                        return detail;
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn(string.Format("An error occurred while getting local group, group name : {0} reason : {1} ", username, e.ToString()));
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// not completed
        /// </summary>
        /// <param name="isSearchTrust"></param>
        /// <param name="domainFilter"></param>
        /// <returns></returns>

        #region For CA moudle
        /// <summary>
        /// 获取user在AD中的distinguishedName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetADDistinguishedName(string userName)
        {
            string domain = string.Empty;//userName.Split('\\')[0];
            string samAccountName = GetAccountName(userName, ref domain);
            if (!string.IsNullOrEmpty(domain))
            {
                string filter = GetSearchFilter(samAccountName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser, false);
                DirectorySearcher ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domain);
                if (ds != null)
                {
                    ds.PropertiesToLoad.Clear();
                    ds.PropertiesToLoad.Add("distinguishedName");
                    ds.Filter = filter;
                    SearchResult sr = ds.FindOne();
                    if (sr != null)
                    {
                        return sr.Properties["distinguishedName"][0].ToString();
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 根据userName获取所有parent Doamin Group List
        /// </summary>
        /// <param name="userName">Domain User LoginName</param>
        /// <returns>List<distinguishedName>CN=Domain Users,CN=Users,DC=SP10,DC=com</distinguishedName></returns>
        public List<string> GetParentGroupCollection(string userName)
        {
            List<string> groups = new List<string>();
            string domain = string.Empty;
            string samAccountName = GetAccountName(userName, ref domain);
            if (!string.IsNullOrEmpty(domain))
            {
                string filter = GetSearchFilter(samAccountName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser, false);
                StringBuilder groupStrings = new StringBuilder();
                groupStrings.Append("(|");
                using (DirectorySearcher ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domain))
                {
                    if (ds != null)
                    {
                        ds.Filter = filter;
                        SearchResult sr = ds.FindOne();
                        if (sr != null)
                        {
                            #region get entery groups sids
                            using (DirectoryEntry user = sr.GetDirectoryEntry())
                            {
                                //we must ask for this one first
                                user.RefreshCache(new string[] { "tokenGroups" });
                                foreach (byte[] sid in user.Properties["tokenGroups"])
                                {
                                    //append each member into the filter
                                    groupStrings.AppendFormat("(objectSid={0})", BuildOctetString(sid));
                                }
                            }
                            //end our initial filter
                            groupStrings.Append(")");
                            #endregion
                            #region search group distinguished name by new filter
                            //begin another search
                            if (groupStrings.Length > 3) //"(|)"
                            {
                                ds.Filter = groupStrings.ToString();
                                ds.PropertiesToLoad.Clear();
                                ds.PropertiesToLoad.Add("distinguishedName");
                                StringBuilder groupForLog = new StringBuilder();
                                using (SearchResultCollection src = ds.FindAll())
                                {
                                    for (int i = 0; i < src.Count; i++)
                                    {
                                        string distinguishedName = src[i].Properties["distinguishedName"][0].ToString();
                                        groups.Add(distinguishedName);
                                        groupForLog.AppendFormat("[{0}]", distinguishedName);
                                    }
                                }
                                mLog.Debug(string.Format("This user({0}) 's group collection is:{1}", userName, groupForLog.ToString()));
                            }
                            #endregion
                        }
                    }
                }
            }
            return groups;
        }

        /// <summary>
        /// 根据userName获取所有parent Doamin Group List
        /// </summary>
        /// <param name="userName">Domain User LoginName</param>
        /// <returns>List<groupsid>s-1-**</groupsid></returns>
        public List<string> GetParentGroupSidCollection(string userName)
        {
            bool IsTrustDomain = false;
            List<string> groups = new List<string>();
            string domain = string.Empty;
            string samAccountName = GetAccountName(userName, ref domain);
            mLog.Info("domain name:{0},user:{1}", domain, samAccountName);
            if (!string.IsNullOrEmpty(domain))
            {
                #region 通过ds 获取parent group，但是primary group 类型group 获取不到
                string filter = GetSearchFilter(samAccountName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser, false);
                using (DirectorySearcher ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domain))
                {
                    if (ds != null)
                    {
                        ds.Filter = filter;
                        SearchResult sr = ds.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry user = sr.GetDirectoryEntry())
                            {
                                user.RefreshCache(new string[] { "tokenGroups" });
                                foreach (byte[] sid in user.Properties["tokenGroups"])
                                {
                                    string sidString = ConvertBinaryIdToSid(sid);
                                    if (!groups.Contains(sidString))
                                    {
                                        groups.Add(sidString);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                List<string> tempGroups = new List<string>();
                #region 遍历信任域找当前的user 或group
                foreach (DomainInfo info in DirectorySearcherFactory.OneWayTrustDoamins)
                {
                    try
                    {
                        if (CompareDomain(domain, info))
                        {
                            IsTrustDomain = true;
                            UserPrincipal user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domain, info.LoginName, info.Password), System.DirectoryServices.AccountManagement.IdentityType.SamAccountName, samAccountName);
                            if (user != null)
                            {
                                foreach (GroupPrincipal group in user.GetGroups())
                                {
                                    GetParentGroup(group, tempGroups);
                                }
                                AddTempGroupsToGroup(groups, tempGroups);
                                try
                                {
                                    foreach (GroupPrincipal group in user.GetGroups(new PrincipalContext(ContextType.Domain, Domain.GetCurrentDomain().Name)))
                                    {
                                        GetParentGroup(group, tempGroups);
                                    }
                                    AddTempGroupsToGroup(groups, tempGroups);
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("Get user trust doamin parent group occurred error,detail:{0}", e.ToString());
                                    tempGroups = new List<string>();
                                }
                                break;
                            }
                            else
                            {
                                GroupPrincipal group = GroupPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domain, info.LoginName, info.Password), System.DirectoryServices.AccountManagement.IdentityType.SamAccountName, samAccountName);
                                if (group != null)
                                {
                                    IsTrustDomain = true;
                                    GetParentGroup(group, tempGroups);
                                    AddTempGroupsToGroup(groups, tempGroups);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(ex.ToString());
                    }
                }
                #endregion
                #region 如果信任域里没有这个group 或user，到本域里找
                if (!IsTrustDomain)
                {
                    try
                    {
                        UserPrincipal user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, Domain.GetCurrentDomain().Name), System.DirectoryServices.AccountManagement.IdentityType.SamAccountName, samAccountName);
                        if (user != null)
                        {
                            foreach (GroupPrincipal group in user.GetGroups())
                            {
                                if (!groups.Contains(group.Sid.ToString()))
                                {
                                    GetCurentDomainParentGroup(group, tempGroups);
                                }
                            }
                            AddTempGroupsToGroup(groups, tempGroups);
                        }
                        else
                        {
                            GroupPrincipal group = GroupPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, Domain.GetCurrentDomain().Name), System.DirectoryServices.AccountManagement.IdentityType.SamAccountName, samAccountName);
                            if (group != null)
                            {
                                IsTrustDomain = true;
                                GetParentGroup(group, tempGroups);
                                AddTempGroupsToGroup(groups, tempGroups);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(ex.ToString());
                    }
                }
                #endregion
            }
            return groups;
        }

        private void AddTempGroupsToGroup(List<string> groups, List<string> tempGroups)
        {
            if (tempGroups != null || tempGroups.Count > 0)
            {
                foreach (var temp in tempGroups)
                {
                    if (!groups.Contains(temp))
                    {
                        groups.Add(temp);
                    }
                }
            }
            tempGroups = new List<string>();
        }

        private void GetCurentDomainParentGroup(GroupPrincipal group, List<string> groups)
        {
            foreach (GroupPrincipal parentGroup in group.GetGroups())
            {
                if (!groups.Contains(parentGroup.Sid.ToString()))
                {
                    GetCurentDomainParentGroup(parentGroup, groups);
                }
            }
            groups.Add(group.Sid.ToString());
        }

        private void GetParentGroup(GroupPrincipal group, List<string> groups)
        {
            if (!groups.Contains(group.Sid.ToString()))
            {
                groups.Add(group.Sid.ToString());
            }
            else
            {
                return;//停止递归   ADO-188565
            }
            foreach (GroupPrincipal paentGroup in group.GetGroups())
            {
                GetParentGroup(paentGroup, groups);
            }
            try
            {
                foreach (GroupPrincipal paentGroup in group.GetGroups(new PrincipalContext(ContextType.Domain, Domain.GetCurrentDomain().Name)))
                {
                    GetParentGroup(paentGroup, groups);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Get user trust doamin parent group occurred error,detail:{0}", e.ToString());
            }

        }

        private bool CompareDomain(string domain, DomainInfo domainInfo)
        {
            if (domainInfo.DomainName.Equals(domain, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                string[] domainSuffixs = domainInfo.DomainName.ToLowerInvariant().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string suffix in domainSuffixs)
                {
                    if (suffix.Equals(domain, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            DirectorySearcher ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainInfo.DomainName, domainInfo.LoginName, domainInfo.Password);
            if (ds != null)
            {
                try
                {
                    ds.SearchScope = SearchScope.Subtree;
                    ds.PropertiesToLoad.Add("msDS-PrincipalName");
                    ds.Filter = "(&(|(objectCategory=Person)(objectCategory=Computer)))";
                    SearchResult result = ds.FindOne();
                    if (result != null)
                    {
                        string principalName = result.Properties["msDS-PrincipalName"][0].ToString();
                        string netbiosName = principalName.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (netbiosName.Equals(domain, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Init {0}'s netbiosName occurred an error: {1}", domainInfo.DomainName, e.ToString());
                }
            }
            return false;
        }

        private string ConvertBinaryIdToSid(byte[] bytes)
        {
            SecurityIdentifier securityIdentifier = new SecurityIdentifier(bytes, 0);
            string sid = securityIdentifier.Value;
            return sid;
        }
        private string BuildOctetString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat("\\{0}", bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #region Get ALL Members In Domain Group

        public List<UserDetail> GetMembersInGroup(string groupName)
        {
            List<UserDetail> users = new List<UserDetail>();
            string domainName = string.Empty;
            groupName = GetAccountName(groupName, ref domainName);
            DirectorySearcher searcher = null;
            Dictionary<SearchResult, string> results = GetMembersInNomalGroup(groupName, domainName, out searcher);
            if (results != null)
            {
                foreach (SearchResult key in results.Keys)
                {
                    string localDomainName = GetGroupMemberDomainName(key.Path);
                    string range = results[key];
                    StringBuilder groupMemberString = new StringBuilder();
                    StringBuilder usersString = new StringBuilder("(|");
                    string realLoginName = string.Empty;
                    foreach (string member in key.Properties[range])
                    {
                        try
                        {
                            groupMemberString.AppendFormat("[{0}].", member);
                            string memberDomainName = GetGroupMemberDomainName(member);
                            if (!CheckMemberRealName(member, ref realLoginName))//信任域
                            {
                                UserDetail user = GetUser(realLoginName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeADDisabledUsers | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
                                if (user != null)
                                {
                                    users.Add(user);
                                }
                                else
                                {
                                    mLog.Warn("Failed to get group member using check account. member:{0} , group:{1}", realLoginName, groupName);
                                }
                            }
                            else if (!localDomainName.Equals(memberDomainName, StringComparison.OrdinalIgnoreCase))//父子域
                            {
                                string nameWithDomain = realLoginName.Contains("\\") ? realLoginName : memberDomainName + "\\" + realLoginName;
                                UserDetail user = GetUser(nameWithDomain, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeADDisabledUsers | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
                                if (user != null)
                                {
                                    users.Add(user);
                                }
                                else
                                {
                                    mLog.Warn("Failed to get group member using check account. member:{0} , group:{1}", realLoginName, groupName);
                                }
                            }
                            else
                            {
                                usersString.AppendFormat("(distinguishedName={0})", ReplaceSpecialChars(member));
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("Get member:{0} failed. Reason:{0}", e.ToString());
                        }
                    }
                    //current domain
                    usersString.Append(")");
                    if (!usersString.ToString().Equals("(|)", StringComparison.OrdinalIgnoreCase))
                    {
                        users.AddRange(GetUserDetailFromDS(searcher, usersString.ToString(), domainName));
                    }
                    mLog.Debug("Expand group members. group:{0} members:{1}", groupName, groupMemberString.ToString());
                }
            }
            return users;
        }

        private string ReplaceSpecialChars(string name)
        {
            foreach (string specialChar in SpecialChars.Keys)
            {
                name = name.Replace(specialChar, SpecialChars[specialChar]);
            }
            return name;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "GetUserDetailFromDS is unmodifiable as the cause of being referenced.")]
        private List<UserDetail> GetUserDetailFromDS(DirectorySearcher ds, string filter, string domainName)
        {
            List<UserDetail> users = new List<UserDetail>();
            try
            {
                ds.Filter = filter;
                ds.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn", "mail" }); //only load UserDetail need
                using (SearchResultCollection src = ds.FindAll())
                {
                    foreach (SearchResult sr in src)
                    {
                        UserDetail userDetail = SearchResultToDetail(sr, domainName);
                        if (userDetail != null)
                        {
                            users.Add(userDetail);
                        }
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                mLog.Warn(e.ToString());
                mLog.Info("Begin to clear domain searcher cache.");
                DirectorySearcherFactory.ClearDomainSearcherCache();
                ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
                ds.Filter = filter;
                ds.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn", "mail" }); //only load UserDetail need
                using (SearchResultCollection src = ds.FindAll())
                {
                    foreach (SearchResult sr in src)
                    {
                        UserDetail userDetail = SearchResultToDetail(sr, domainName);
                        if (userDetail != null)
                        {
                            users.Add(userDetail);
                        }
                    }
                }
            }
            return users;
        }

        /// <summary>
        /// 展开Domain Group获取Members成员（只展开一层，多层需要外围循环调用）
        /// </summary>
        /// <param name="groupName">Domain group LoginName</param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "GetMembersInNomalGroup is unmodifiable as the cause of being referenced.")]
        private Dictionary<SearchResult, string> GetMembersInNomalGroup(string groupName, string domainName, out DirectorySearcher searcher)
        {
            Dictionary<SearchResult, string> results = null;
            searcher = null;
            try
            {
                searcher = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
                string filter = GetSearchFilter(groupName, AccountSearchFlag.IncludeADGroup, false);
                results = GetMembersInNomalGroup(searcher, filter, domainName);
            }
            catch (Exception e)
            {
                mLog.Warn(e.ToString());
            }
            return results;
        }

        /// <summary>
        /// this search only load "member:range" this one attribute, this will be soon
        /// </summary>
        /// <param name="searcher"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private Dictionary<SearchResult, string> GetMembersInNomalGroup(DirectorySearcher searcher, string filter, string domainName = "")
        {
            bool needClearCache = false;
            if (searcher == null)
            {
                return null;
            }
            Dictionary<SearchResult, string> results = new Dictionary<SearchResult, string>();
            uint rangeStep = 1000;
            uint rangeLow = 0;
            uint rangeHigh = rangeLow + (rangeStep - 1);
            bool lastQuery = false;
            bool quitLoop = false;
            int exitFlag = 0;
            string attributeWithRange = string.Empty;
            lock (searcher)
            {
                try
                {
                    searcher.Filter = filter;
                    do
                    {
                        if (!lastQuery)
                        {
                            attributeWithRange = String.Format("member;range={0}-{1}", rangeLow, rangeHigh);
                        }
                        else
                        {
                            attributeWithRange = String.Format("member;range={0}-*", rangeLow);
                        }
                        searcher.PropertiesToLoad.Clear();
                        searcher.PropertiesToLoad.Add(attributeWithRange);
                        SearchResult result = null;
                        result = searcher.FindOne();
                        if (result.Properties.Contains(attributeWithRange))
                        {
                            if (!results.ContainsKey(result))
                            {
                                results.Add(result, attributeWithRange);
                            }
                            if (lastQuery)
                            {
                                quitLoop = true;
                            }
                        }
                        else
                        {
                            exitFlag++;
                            lastQuery = true;
                        }
                        if (exitFlag == 2)
                        {
                            break;
                        }
                        if (!lastQuery)
                        {
                            rangeLow = rangeHigh + 1;
                            rangeHigh = rangeLow + (rangeStep - 1);
                        }
                    }
                    while (!quitLoop);
                    return results;
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    mLog.Warn("An error occurred while get members in normal group. Reason:{0}", e.ToString());
                    needClearCache = true;
                }
                catch (Exception ex)
                {
                    mLog.Warn("An error occurred while get members in normal group. Reason:{0}", ex.ToString());
                    return null;
                }
                finally
                {
                    if (searcher != null)
                    {
                        searcher.PropertiesToLoad.Clear();
                    }
                }
                if (needClearCache)
                {
                    mLog.Info("Begin to clear domain searcher cache.");
                    DirectorySearcherFactory.ClearDomainSearcherCache();
                    searcher = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
                    return GetMembersInNomalGroupRetry(searcher, filter);
                }
                return null;
            }
        }

        private Dictionary<SearchResult, string> GetMembersInNomalGroupRetry(DirectorySearcher searcher, string filter)
        {
            if (searcher == null)
            {
                return null;
            }
            Dictionary<SearchResult, string> results = new Dictionary<SearchResult, string>();
            uint rangeStep = 1000;
            uint rangeLow = 0;
            uint rangeHigh = rangeLow + (rangeStep - 1);
            bool lastQuery = false;
            bool quitLoop = false;
            int exitFlag = 0;
            string attributeWithRange = string.Empty;
            lock (searcher)
            {
                try
                {
                    searcher.Filter = filter;
                    do
                    {
                        if (!lastQuery)
                        {
                            attributeWithRange = String.Format("member;range={0}-{1}", rangeLow, rangeHigh);
                        }
                        else
                        {
                            attributeWithRange = String.Format("member;range={0}-*", rangeLow);
                        }
                        searcher.PropertiesToLoad.Clear();
                        searcher.PropertiesToLoad.Add(attributeWithRange);
                        SearchResult result = null;

                        result = searcher.FindOne();


                        if (result.Properties.Contains(attributeWithRange))
                        {
                            if (!results.ContainsKey(result))
                            {
                                results.Add(result, attributeWithRange);
                            }
                            if (lastQuery)
                            {
                                quitLoop = true;
                            }
                        }
                        else
                        {
                            exitFlag++;
                            lastQuery = true;
                        }
                        if (exitFlag == 2)
                        {
                            break;
                        }
                        if (!lastQuery)
                        {
                            rangeLow = rangeHigh + 1;
                            rangeHigh = rangeLow + (rangeStep - 1);
                        }
                    }
                    while (!quitLoop);
                    return results;
                }
                catch (Exception ex)
                {
                    mLog.Warn("An error occurred while retry get members in normal group. Reason:{0}", ex.ToString());
                    return null;
                }
                finally
                {
                    if (searcher != null)
                    {
                        searcher.PropertiesToLoad.Clear();
                    }
                }
            }
        }

        private bool CheckMemberRealName(string path, ref string realLoginName)
        {
            bool flag = true;
            int index = path.IndexOf(',');
            while (path[index - 1] == '\\')
            {
                index = path.IndexOf(',', index + 1);
            }
            string userName = path.Substring(3, index - 3);
            foreach (string specialChar in SpecialChars.Keys)
            {
                userName = userName.Replace(specialChar, SpecialChars[specialChar]);
            }
            realLoginName = GetRealAccountName(userName);
            if (!realLoginName.Equals(userName))
            {
                flag = false;
            }
            return flag;
        }

        private string GetGroupMemberDomainName(string path)
        {
            string domainName = string.Empty;
            string tempDomain = path.Substring(path.IndexOf("DC=", StringComparison.OrdinalIgnoreCase));
            string[] separator = new string[] { "DC=" };
            string[] domains = tempDomain.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (domains.Length > 0)
            {
                domainName = domains[0].TrimEnd(']').TrimEnd(',');
            }
            return domainName;
        }
        #endregion
        #endregion
    }
}
