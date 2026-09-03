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





//namespace AvePoint.GCommon
//{
//    #region using directives
//    using System;
//    using System.Collections.Generic;
//    using System.DirectoryServices;
//    using System.Net;
//    using System.Text;
//    using System.Text.RegularExpressions;
//    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
//    #endregion

//    /// <summary>
//    /// 重构时遵循的几个原则：
//    /// 1，此类得方法主要涉及DocAve中的people picker控件的check和browser，默认不初始化环境及所有相关的Domain信任关系
//    /// 2，AD相关的check/browser agent端优先走SPUtility逻辑，check不到才调用此类（server端由于没有API，所以直接调用此类）
//    /// 3，如果输入的user含有doamin name（目前以\\判断），那么直接根据name初始化DiectoryEntry进行search
//    /// 4，如果没有含有domain，通过Global Catalog实例获取当前forest的DirectorySearcher，还不行再继续实例他的trust forest，并加到全局变量
//    /// 5，CA模块单独用到了两个方法，由于这两个方法参数已经是loginName，所以可以直接实例化domain,不再采用单例模式，而在外围对结果进行cache ：
//    /// #1）GetMembersInGroup（）展开ADGroup获得members 
//    /// #2）GetParentGroupCollection（） 获取Domain User的所有parent Doamin Group
//    /// </summary>
//    public class LDAPLookup : IAccountManager
//    {
//        static AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//        private static string BUILDTIN_DOMAIN = "builtIn";
//        private readonly static object lockObj = new object();
//        private static Dictionary<string, string> mSpecialChars = new Dictionary<string, string>();
//        public static Dictionary<string, string> SpecialChars
//        {
//            get
//            {
//                if (mSpecialChars.Count == 0)
//                {
//                    mSpecialChars.Add("\\", "\\5c");
//                    mSpecialChars.Add("*", "\\2a");
//                    mSpecialChars.Add("(", "\\28");
//                    mSpecialChars.Add(")", "\\29");
//                    mSpecialChars.Add("/", "\\2f");
//                    //    mSpecialChars.Add(" ", "\\00");
//                }
//                return mSpecialChars;
//            }
//        }
//        static LDAPLookup ldapLookup;
//        #region Construct and Init

//        public static LDAPLookup GetInstance()
//        {
//            lock (lockObj)
//            {
//                if (ldapLookup == null)
//                {
//                    ldapLookup = new LDAPLookup();
//                }
//            }
//            return ldapLookup;
//        }
//        #endregion

//        #region IAccountManager

//        public bool IsEnable()
//        {
//            return true;
//        }

//        public bool CheckUser(string username)
//        {
//            return CheckUser(username, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
//        }

//        public bool CheckUser(string username, AccountSearchFlag flag)
//        {
//            string tmpLogin = null;
//            return ExtractUserName(username, ref tmpLogin, flag);
//        }

//        public bool ExtractUserName(string username, ref string loginName)
//        {
//            return ExtractUserName(username, ref loginName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
//        }

//        public bool ExtractUserName(string username, ref string loginName, AccountSearchFlag flag)
//        {
//            UserDetail detail = GetUser(username, flag);
//            if (detail != null)
//            {
//                loginName = detail.LoginName;
//                return true;
//            }

//            return false;
//        }

//        public UserDetail GetUser(string username)
//        {
//            return GetUser(username, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
//        }

//        public UserDetail GetUser(string username, AccountSearchFlag flag)
//        {
//            UserDetail detail = null;
//            SearchResult result = null;
//            string domainName = string.Empty;
//            string samAccountName = GetAccountName(username, ref domainName);
//            string filter = GetSearchFilter(samAccountName, flag, false);
//            if (!string.IsNullOrEmpty(filter))
//            {
//                result = CheckAccount(filter, ref domainName);
//            }
//            if (result != null)
//            {
//                //people picker not include disabled users
//                detail = SearchResultToDetail(result, domainName, flag);
//            }
//            if (detail == null)
//            {
//                detail = GetLocalUserInfo(username, flag);
//            }
//            return detail;
//        }

//        public List<UserDetail> GetUsers(string username)
//        {
//            return GetUsers(username, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser);
//        }

//        public List<UserDetail> GetUsers(string username, AccountSearchFlag flag)
//        {
//            List<UserDetail> users = new List<UserDetail>();
//            string domainName = string.Empty;
//            string samAccountName = GetAccountName(username, ref domainName);
//            string filter = GetSearchFilter(samAccountName, flag, true);
//            return SearchAccount(filter, flag, ref domainName);
//        }

//        public bool IsMemeberAlive(string loginName)
//        {
//            switch (loginName.ToLower())
//            {
//                case "nt authority\\local service":
//                case "nt authority\\authenticated users":
//                case "sharepoint\\system":
//                case "nt authority\\system":
//                    return true;
//                default:
//                    break;
//            }
//            if (CheckUser(loginName))
//            {
//                return true;
//            }
//            return GetLocalUserInfo(loginName) != null;
//        }

//        #endregion

//        #region Tool Method

//        /// <summary>
//        /// depend on init ldap complete, will loop all directorysearcher
//        /// </summary>
//        /// <param name="filter"></param>
//        /// <returns></returns>
//        private SearchResult CheckAccount(string filter, ref string domainName)
//        {
//            if (!string.IsNullOrEmpty(domainName))
//            {
//                return CheckAccount(filter, domainName);
//            }
//            Dictionary<string, DirectorySearcher> searchers = DirectorySearcherFactory.GlobalSearcherCollection;
//            foreach (KeyValuePair<string, DirectorySearcher> pair in searchers)
//            {
//                try
//                {
//                    SearchResult result = CheckAccount(pair.Value, filter);
//                    if (result != null)
//                    {
//                        domainName = pair.Key;
//                        return result;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    mLog.Error(ex.ToString());
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// get directoryentry directly by domain name, not depend on ldap init
//        /// </summary>
//        /// <param name="filter"></param>
//        /// <param name="domainName"></param>
//        /// <returns></returns>
//        private SearchResult CheckAccount(string filter, string domainName)
//        {
//            try
//            {
//                DirectorySearcher searcherInfo = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
//                {
//                    SearchResult result = CheckAccount(searcherInfo, filter);
//                    if (result != null)
//                    {
//                        return result;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                mLog.Warn(ex.ToString());
//            }
//            return null;
//        }

//        private SearchResult CheckAccount(DirectorySearcher searcher, string filter)
//        {
//            try
//            {
//                lock (searcher)  //DOC-37394 If searcher is mDomainSearcher, it will have conflict in multithreading, because mDomainSearcher is static.
//                {
//                    searcher.Filter = filter;
//                    searcher.PropertiesToLoad.Clear();
//                    searcher.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn" });
//                    return searcher.FindOne();
//                }
//            }
//            catch (Exception ex)
//            {
//                mLog.Warn("An error occured while check account. Reason:{0}", ex.ToString());
//                return null;
//            }
//        }

//        private List<UserDetail> SearchAccount(string filter, AccountSearchFlag flag, ref string domainName)
//        {
//            if (!string.IsNullOrEmpty(domainName))
//            {
//                return SearchAccount(filter, flag, domainName);
//            }
//            Dictionary<string, DirectorySearcher> results = DirectorySearcherFactory.GlobalSearcherCollection;
//            List<UserDetail> users = new List<UserDetail>();
//            foreach (KeyValuePair<string, DirectorySearcher> pair in results)
//            {
//                List<SearchResult> srs = SearchAccount(pair.Value, filter);
//                foreach (SearchResult result in srs)
//                //people picker not include disabled users
//                {
//                    UserDetail detail = SearchResultToDetail(result, pair.Key, flag);
//                    if (detail != null)
//                    {
//                        users.Add(detail);
//                    }
//                }
//            }
//            return users;
//        }

//        private List<UserDetail> SearchAccount(string filter, AccountSearchFlag flag, string domainName)
//        {
//            List<UserDetail> users = new List<UserDetail>();
//            List<SearchResult> results = new List<SearchResult>();
//            DirectorySearcher searcherInfo = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
//            {
//                List<SearchResult> srs = SearchAccount(searcherInfo, filter);
//                foreach (SearchResult result in srs)
//                {
//                    //people picker not include disabled users
//                    UserDetail detail = SearchResultToDetail(result, domainName, flag);
//                    if (detail != null)
//                    {
//                        users.Add(detail);
//                    }
//                }
//            }
//            return users;
//        }

//        private List<SearchResult> SearchAccount(DirectorySearcher searcher, string filter)
//        {
//            List<SearchResult> returnResults = new List<SearchResult>();
//            try
//            {
//                searcher.Filter = filter;
//                searcher.SizeLimit = 201;
//                searcher.PageSize = 1000;
//                searcher.PropertiesToLoad.Clear();
//                searcher.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn", "mail" });
//                using (SearchResultCollection results = searcher.FindAll())
//                {
//                    foreach (SearchResult result in results)
//                    {
//                        returnResults.Add(result);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                mLog.Warn("An error occured while search account. Reason:{0}", ex.ToString());
//            }
//            return returnResults;
//        }

//        /// <summary>
//        /// 获取登录用户账户，用于组装filter，DomainA\\user1将返回user1， user2将返回user2
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="domainName"></param>
//        /// <returns></returns>
//        private string GetAccountName(string name, ref string domainName)
//        {
//            name = UserLoginNamePrefix.RemoveLoginNamePrifix(name);
//            name = GetRealAccountName(name);
//            string samAccountName = name;
//            if (name.IndexOf('\\') != -1)
//            {
//                domainName = name.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
//                if (BUILDTIN_DOMAIN.Equals(domainName, StringComparison.OrdinalIgnoreCase))
//                {
//                    domainName = string.Empty;
//                }
//                samAccountName = name.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[1];
//            }
//            foreach (string specialChar in SpecialChars.Keys)
//            {
//                samAccountName = samAccountName.Replace(specialChar, SpecialChars[specialChar]);
//            }
//            return samAccountName;
//        }

//        private string GetSearchFilter(string name, AccountSearchFlag flag, bool isSearch)
//        {
//            StringBuilder filter = new StringBuilder();
//            if ((flag & AccountSearchFlag.IncludeADGroup) != AccountSearchFlag.None)
//            {
//                filter.Append("(&(objectCategory=group)(objectClass=group))");
//                //filter.Append("(&(&(objectCategory=group)(objectClass=group))(groupType:1.2.840.113556.1.4.803:=2147483648))");
//            }
//            string formatName = name;

//            if ((flag & AccountSearchFlag.IncludeADUser) != AccountSearchFlag.None)
//            {
//                if (filter.Length > 0)
//                {
//                    filter.Insert(0, '|');
//                }
//                string userFilter = "(&(objectCategory=person)(objectClass=user))";
//                filter.Append(userFilter);
//            }

//            if (isSearch)
//            {
//                formatName = string.Format("*{0}*", formatName).Replace("**", "*");
//            }
//            else if (formatName.Contains("*"))//DOC-39896, A*B should not check any accounts.
//            {
//                return null;
//            }
//            string mailAndLogOnname = formatName;
//            if (!mailAndLogOnname.Contains("@"))
//            {
//                mailAndLogOnname = string.Format("{0}@*", mailAndLogOnname);
//            }
//            //去掉了first name(givenname)，last name(sn)和initials(initials)
//            //support upn check/find
//            return string.Format("(&({0})(|(|(|(|(|(cn={1})(samAccount={1}))(samAccountName={1}))(mail={2}))(userPrincipalName={2}))(displayName={1})))", filter.ToString(), formatName, mailAndLogOnname);
//        }

//        /// <summary>
//        /// this result must contains these properties:objectSid;groupType;userAccountControl;
//        /// displayName;cn;
//        /// mail(not needed)
//        /// </summary>
//        /// <param name="result"></param>
//        /// <returns></returns>
//        private UserDetail SearchResultToDetail(SearchResult result, string domainName, AccountSearchFlag flag)
//        {
//            string commonName = result.Properties["cn"][0].ToString();
//            string realName = GetRealAccountName(commonName);
//            if (!realName.Equals(commonName, StringComparison.OrdinalIgnoreCase)) //单向信任域，accountname为sid
//            {
//                return GetUser(realName, flag);
//            }
//            else
//            {
//                return GetDetailFromSearchResult(result, domainName, flag);
//            }
//        }

//        private UserDetail SearchResultToDetail(SearchResult result, string domainName)
//        {
//            //search all users in AD by default
//            return SearchResultToDetail(result, domainName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeADDisabledUsers);
//        }

//        protected virtual UserDetail GetDetailFromSearchResult(SearchResult result, string domainName, AccountSearchFlag flag)
//        {
//            UserDetail detail = new UserDetail();
//            try
//            {
//                Win32Native.SID_NAME_USE sidUse = Win32Native.SID_NAME_USE.SidTypeUnknown;
//                StringBuilder name = new StringBuilder(0x100);
//                StringBuilder domain = new StringBuilder(0x100);
//                int cbDomainName = 0x100;
//                int cbName = 0x100;
//                byte[] userId = (byte[])result.Properties["objectSid"][0];
//                if (Win32Native.LookupAccountSid(null, userId, name, ref cbName, domain, ref cbDomainName, ref sidUse))
//                {
//                    detail.LoginName = string.Format("{0}\\{1}", domain.ToString(), name.ToString());
//                }
//                else if (result.Properties.Contains("samaccountname"))
//                {
//                    string accountName = result.Properties["samaccountname"][0].ToString();
//                    detail.LoginName = string.Format("{0}\\{1}", domainName, accountName);
//                }
//                detail.AccountType = AccountType.ADUser;
//                if (result.Properties.Contains("groupType"))
//                {
//                    detail.AccountType = AccountType.ADGroup;
//                }
//                if (result.Properties.Contains("userAccountControl") && result.Properties["userAccountControl"].Count != 0)
//                {
//                    int control = (int)result.Properties["userAccountControl"][0];
//                    if ((control & 2) != 0)
//                    {
//                        detail.AccountState = AccountStatus.Deactived;
//                        if ((flag & AccountSearchFlag.IncludeADDisabledUsers) == AccountSearchFlag.None)
//                        {
//                            return null;
//                        }
//                    }
//                    else
//                    {
//                        detail.AccountState = AccountStatus.Actived;
//                    }
//                }
//                if (result.Properties.Contains("cn")) //common name==display name
//                {
//                    detail.DisplayName = result.Properties["cn"][0].ToString();
//                }
//                if (result.Properties.Contains("mail") && result.Properties["mail"].Count != 0)//should display this property when find user
//                {
//                    detail.Email = result.Properties["mail"][0].ToString();
//                }
//            }
//            catch (Exception ee)
//            {
//                mLog.Warn(ee.ToString());
//            }
//            return detail;
//        }

//        private string GetRealAccountName(string user)
//        {
//            string pattern = @"^[sS]-1-\d-\d*-\d*-\d*-\d*-\d*$";
//            if (Regex.IsMatch(user, pattern))
//            {
//                mLog.Info(string.Format("before translate the user name is {0}", user));
//                try
//                {
//                    user = new System.Security.Principal.SecurityIdentifier(user).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
//                }
//                catch (Exception ee)
//                {
//                    mLog.Warn(ee.ToString());
//                }
//                mLog.Info(string.Format("after translate the user name is {0}", user));
//            }
//            return user;
//        }
//        #endregion

//        #region For Local User
//        public static UserDetail GetLocalUserInfo(string username)
//        {
//            return GetLocalUserInfo(username, AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
//        }

//        public static UserDetail GetLocalUserInfo(string username, AccountSearchFlag flag)
//        {
//            username = UserLoginNamePrefix.RemoveLoginNamePrifix(username);
//            if ((flag & (AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser)) == AccountSearchFlag.None)
//            {
//                return null;
//            }
//            if (username.IndexOf('\\') > 0)
//            {
//                if (!username.Split('\\')[0].Equals(Dns.GetHostName(), StringComparison.OrdinalIgnoreCase))
//                {
//                    return null;
//                }
//                username = username.Substring(username.LastIndexOf('\\') + 1);
//            }
//            DirectoryEntry group = new DirectoryEntry("WinNT://" + Dns.GetHostName() + ",computer");

//            if ((flag & AccountSearchFlag.IncludeLocalUser) != AccountSearchFlag.None)
//            {
//                try
//                {
//                    DirectoryEntry user = group.Children.Find(username, "User");
//                    if (user != null)
//                    {
//                        bool disable = (bool)user.InvokeGet("AccountDisabled");
//                        if (!disable)
//                        {
//                            UserDetail detail = new UserDetail();
//                            detail.LoginName = string.Format("{0}\\{1}", Dns.GetHostName(), username);
//                            detail.DisplayName = user.Invoke("Get", new object[] { "FullName" }).ToString();
//                            if (string.IsNullOrEmpty(detail.DisplayName))
//                            {
//                                detail.DisplayName = detail.LoginName;
//                            }
//                            detail.AccountType = AccountType.LocalUser;
//                            return detail;
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    mLog.Warn(string.Format("An error occured while getting local user, user name : {0} reason : {1} ", username, e.ToString()));
//                }
//            }

//            if ((flag & AccountSearchFlag.IncludeLocalGroup) != AccountSearchFlag.None)
//            {
//                try
//                {
//                    DirectoryEntry user = group.Children.Find(username, "Group");
//                    if (user != null)
//                    {
//                        UserDetail detail = new UserDetail();
//                        detail.LoginName = string.Format("{0}\\{1}", Dns.GetHostName(), username);
//                        detail.DisplayName = detail.LoginName;
//                        detail.AccountType = AccountType.LocalGroup;
//                        return detail;
//                    }
//                }
//                catch (Exception e)
//                {
//                    mLog.Warn(string.Format("An error occured while getting local group, group name : {0} reason : {1} ", username, e.ToString()));
//                }
//            }
//            return null;
//        }
//        #endregion

//        /// <summary>
//        /// not completed
//        /// </summary>
//        /// <param name="isSearchTrust"></param>
//        /// <param name="domainFilter"></param>
//        /// <returns></returns>

//        #region For CA moudle
//        /// <summary>
//        /// 获取user在AD中的distinguishedName
//        /// </summary>
//        /// <param name="userName"></param>
//        /// <returns></returns>
//        public string GetADDistinguishedName(string userName)
//        {
//            string domain = string.Empty;//userName.Split('\\')[0];
//            string samAccountName = GetAccountName(userName, ref domain);
//            if (!string.IsNullOrEmpty(domain))
//            {
//                string filter = GetSearchFilter(samAccountName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser, false);
//                DirectorySearcher ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domain);
//                if (ds != null)
//                {
//                    ds.PropertiesToLoad.Clear();
//                    ds.PropertiesToLoad.Add("distinguishedName");
//                    ds.Filter = filter;
//                    SearchResult sr = ds.FindOne();
//                    if (sr != null)
//                    {
//                        return sr.Properties["distinguishedName"][0].ToString();
//                    }
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// 根据userName获取所有parent Doamin Group List
//        /// </summary>
//        /// <param name="userName">Domain User LoginName</param>
//        /// <returns>List<distinguishedName>CN=Domain Users,CN=Users,DC=SP10,DC=com</distinguishedName></returns>
//        public List<string> GetParentGroupCollection(string userName)
//        {
//            List<string> groups = new List<string>();
//            string domain = string.Empty;
//            string samAccountName = GetAccountName(userName, ref domain);
//            if (!string.IsNullOrEmpty(domain))
//            {
//                string filter = GetSearchFilter(samAccountName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser, false);
//                StringBuilder groupStrings = new StringBuilder();
//                groupStrings.Append("(|");
//                using (DirectorySearcher ds = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domain))
//                {
//                    if (ds != null)
//                    {
//                        ds.Filter = filter;
//                        SearchResult sr = ds.FindOne();
//                        if (sr != null)
//                        {
//                            #region get entery groups sids
//                            using (DirectoryEntry user = sr.GetDirectoryEntry())
//                            {
//                                //we must ask for this one first
//                                user.RefreshCache(new string[] { "tokenGroups" });
//                                foreach (byte[] sid in user.Properties["tokenGroups"])
//                                {
//                                    //append each member into the filter
//                                    groupStrings.AppendFormat("(objectSid={0})", BuildOctetString(sid));
//                                }
//                            }
//                            //end our initial filter
//                            groupStrings.Append(")");
//                            #endregion
//                            #region search group distinguished name by new filter
//                            //begin another search
//                            if (groupStrings.Length > 3) //"(|)"
//                            {
//                                ds.Filter = groupStrings.ToString();
//                                ds.PropertiesToLoad.Clear();
//                                ds.PropertiesToLoad.Add("distinguishedName");
//                                StringBuilder groupForLog = new StringBuilder();
//                                using (SearchResultCollection src = ds.FindAll())
//                                {
//                                    for (int i = 0; i < src.Count; i++)
//                                    {
//                                        string distinguishedName = src[i].Properties["distinguishedName"][0].ToString();
//                                        groups.Add(distinguishedName);
//                                        groupForLog.AppendFormat("[{0}]", distinguishedName);
//                                    }
//                                }
//                                mLog.Debug(string.Format("This user({0}) 's group collection is:{1}", userName, groupForLog.ToString()));
//                            }
//                            #endregion
//                        }
//                    }
//                }
//            }
//            return groups;
//        }

//        private string BuildOctetString(byte[] bytes)
//        {
//            StringBuilder sb = new StringBuilder();

//            for (int i = 0; i < bytes.Length; i++)
//            {
//                sb.AppendFormat("\\{0}", bytes[i].ToString("X2"));
//            }
//            return sb.ToString();
//        }
//        #region Get ALL Members In Domain Group

//        public List<UserDetail> GetMembersInGroup(string groupName)
//        {
//            List<UserDetail> users = new List<UserDetail>();
//            string domainName = string.Empty;
//            groupName = GetAccountName(groupName, ref domainName);
//            DirectorySearcher searcher = null;
//            Dictionary<SearchResult, string> results = GetMembersInNomalGroup(groupName, domainName, out searcher);
//            if (results != null)
//            {
//                foreach (SearchResult key in results.Keys)
//                {
//                    string localDomainName = GetGroupMemberDomainName(key.Path);
//                    string range = results[key];
//                    StringBuilder groupMemberString = new StringBuilder();
//                    StringBuilder usersString = new StringBuilder("(|");
//                    string realLoginName = string.Empty;
//                    foreach (string member in key.Properties[range])
//                    {
//                        try
//                        {
//                            groupMemberString.AppendFormat("[{0}].", member);
//                            string memberDomainName = GetGroupMemberDomainName(member);
//                            if (!CheckMemberRealName(member, ref realLoginName))//信任域
//                            {
//                                UserDetail user = GetUser(realLoginName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeADDisabledUsers | AccountSearchFlag.IncludeLocalGroup | AccountSearchFlag.IncludeLocalUser);
//                                if (user != null)
//                                {
//                                    users.Add(user);
//                                }
//                                else
//                                {
//                                    mLog.Warn("Failed to get group member using check account. member:{0} , group:{1}", realLoginName, groupName);
//                                }
//                            }
//                            else if (!localDomainName.Equals(memberDomainName, StringComparison.OrdinalIgnoreCase))//父子域
//                            {
//                                DirectoryEntry de = new DirectoryEntry("LDAP://" + member);
//                                DirectorySearcher ds = new DirectorySearcher(de);
//                                SearchResult sr = ds.FindOne();
//                                if (sr != null)
//                                {
//                                    UserDetail user = GetDetailFromSearchResult(sr, memberDomainName, AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeADDisabledUsers);
//                                    if (user != null)
//                                    {
//                                        users.Add(user);
//                                    }
//                                    else
//                                    {
//                                        mLog.Debug("Failed to get group member using LDAP. member:{0} , group:{1}", member, groupName);
//                                    }
//                                }
//                                else
//                                {
//                                    mLog.Debug("Failed to get group member:{0} , group:{1}, because search result is null", member, groupName);
//                                }
//                            }
//                            else
//                            {
//                                usersString.AppendFormat("(distinguishedName={0})", member);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            mLog.Warn("Get member:{0} failed. Reason:{0}", e.ToString());
//                        }
//                    }
//                    //current domain
//                    usersString.Append(")");
//                    if (!usersString.ToString().Equals("(|)", StringComparison.OrdinalIgnoreCase))
//                    {
//                        users.AddRange(GetUserDetailFromDS(searcher, usersString.ToString(), domainName));
//                    }
//                    mLog.Debug("Expand group members. group:{0} members:{1}", groupName, groupMemberString.ToString());
//                }
//            }
//            return users;
//        }

//        private List<UserDetail> GetUserDetailFromDS(DirectorySearcher ds, string filter, string domainName)
//        {
//            List<UserDetail> users = new List<UserDetail>();
//            ds.Filter = filter;
//            ds.PropertiesToLoad.AddRange(new string[] { "objectSid", "grouptype", "userAccountControl", "samaccountname", "cn" }); //only load UserDetail need
//            using (SearchResultCollection src = ds.FindAll())
//            {
//                foreach (SearchResult sr in src)
//                {
//                    UserDetail userDetail = SearchResultToDetail(sr, domainName);
//                    if (userDetail != null)
//                    {
//                        users.Add(userDetail);
//                    }
//                }
//            }
//            return users;
//        }

//        /// <summary>
//        /// 展开Domain Group获取Members成员（只展开一层，多层需要外围循环调用）
//        /// </summary>
//        /// <param name="groupName">Domain group LoginName</param>
//        /// <returns></returns>
//        private Dictionary<SearchResult, string> GetMembersInNomalGroup(string groupName, string domainName, out DirectorySearcher searcher)
//        {
//            Dictionary<SearchResult, string> results = null;
//            searcher = null;
//            try
//            {
//                searcher = DirectorySearcherFactory.GetLDAPSearchersByDomainName(domainName);
//                string filter = GetSearchFilter(groupName, AccountSearchFlag.IncludeADGroup, false);
//                results = GetMembersInNomalGroup(searcher, filter);
//            }
//            catch (Exception e)
//            {
//                mLog.Warn(e.ToString());
//            }
//            return results;
//        }

//        /// <summary>
//        /// this search only load "member:range" this one attribute, this will be soon
//        /// </summary>
//        /// <param name="searcher"></param>
//        /// <param name="filter"></param>
//        /// <returns></returns>
//        private Dictionary<SearchResult, string> GetMembersInNomalGroup(DirectorySearcher searcher, string filter)
//        {
//            if (searcher == null)
//            {
//                return null;
//            }
//            Dictionary<SearchResult, string> results = new Dictionary<SearchResult, string>();
//            uint rangeStep = 1000;
//            uint rangeLow = 0;
//            uint rangeHigh = rangeLow + (rangeStep - 1);
//            bool lastQuery = false;
//            bool quitLoop = false;
//            int exitFlag = 0;
//            string attributeWithRange = string.Empty;
//            lock (searcher)
//            {
//                try
//                {
//                    searcher.Filter = filter;
//                    do
//                    {
//                        if (!lastQuery)
//                        {
//                            attributeWithRange = String.Format("member;range={0}-{1}", rangeLow, rangeHigh);
//                        }
//                        else
//                        {
//                            attributeWithRange = String.Format("member;range={0}-*", rangeLow);
//                        }
//                        searcher.PropertiesToLoad.Clear();
//                        searcher.PropertiesToLoad.Add(attributeWithRange);
//                        SearchResult result = searcher.FindOne();
//                        if (result.Properties.Contains(attributeWithRange))
//                        {
//                            if (!results.ContainsKey(result))
//                            {
//                                results.Add(result, attributeWithRange);
//                            }
//                            if (lastQuery)
//                            {
//                                quitLoop = true;
//                            }
//                        }
//                        else
//                        {
//                            exitFlag++;
//                            lastQuery = true;
//                        }
//                        if (exitFlag == 2)
//                        {
//                            break;
//                        }
//                        if (!lastQuery)
//                        {
//                            rangeLow = rangeHigh + 1;
//                            rangeHigh = rangeLow + (rangeStep - 1);
//                        }
//                    }
//                    while (!quitLoop);
//                    return results;
//                }
//                catch (Exception ex)
//                {
//                    mLog.Warn("An error occured while get members in normal group. Reason:{0}", ex.ToString());
//                    return null;
//                }
//                finally
//                {
//                    if (searcher != null)
//                    {
//                        searcher.PropertiesToLoad.Clear();
//                    }
//                }
//            }
//        }

//        private bool CheckMemberRealName(string path, ref string realLoginName)
//        {
//            bool flag = true;
//            int index = path.IndexOf(',');
//            while (path[index - 1] == '\\')
//            {
//                index = path.IndexOf(',', index + 1);
//            }
//            string userName = path.Substring(3, index - 3);
//            foreach (string specialChar in SpecialChars.Keys)
//            {
//                userName = userName.Replace(specialChar, SpecialChars[specialChar]);
//            }
//            realLoginName = GetRealAccountName(userName);
//            if (!realLoginName.Equals(userName))
//            {
//                flag = false;
//            }
//            return flag;
//        }

//        private string GetGroupMemberDomainName(string path)
//        {
//            string domainName = string.Empty;
//            string tempDomain = path.Substring(path.IndexOf("DC="));
//            string[] separator = new string[] { "DC=" };
//            string[] domains = tempDomain.Split(separator, StringSplitOptions.RemoveEmptyEntries);
//            if (domains.Length > 0)
//            {
//                domainName = domains[0].TrimEnd(']').TrimEnd(',');
//            }
//            return domainName;
//        }
//        #endregion
//        #endregion
//    }
//}
