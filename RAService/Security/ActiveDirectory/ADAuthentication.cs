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
using ActiveDirectoryWrapper.WorkGroup;
using AvePoint.Common.ActiveDirectoryWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RMContract;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Common.Cryptography;
using System.Text;
using AvePoint.RA.DB.Model;
using System.DirectoryServices;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb.Account.Security;

namespace AvePoint.RA.Service.Security.ActiveDirectory
{
    public class ADAuthentication
    {
        private static readonly IAveLogger logger = AveLogger.GetInstance(typeof(ADAuthentication));
        private IDatabaseEncryption mDBEncrypt = DatabaseEncryptionFactory.CreateDatabaseEncryption();

        #region Database Dao
        private IAccountDao accountDao;
        private IADDomainDao domainDao;

        protected IAccountDao AccountDao
        {
            get
            {
                if (accountDao == null)
                {
                    accountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return accountDao;
            }
        }
        
        protected IADDomainDao DomainDao
        {
            get
            {
                if (domainDao == null)
                {
                    domainDao = (IADDomainDao)PlatformWindsorManager.GetService(typeof(IADDomainDao));
                }
                return domainDao;
            }
        }
        #endregion

        [RACodeReview("Allen Yin")]
        public bool DomainValidationTest(ref RMDomainDto info)
        {
            try
            {
                using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(info.DomainName, info.UserName, info.Password))
                {
                    checker.CreateDefaultSearcher();
                    if (checker.Connected)
                    {
                        info.RealName = checker.RealDomainName;
                        info.NetBiosName = checker.NetBIOSName;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while Validation Domain. error message: {0}.", ex.ToString());
            }
            return false;
        }

        public bool DomainValidationTest(string domainName, string userName, string password)
        {
            try
            {
                using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(domainName, userName, password))
                {
                    checker.CreateDefaultSearcher();
                    if (checker.Connected)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while Validation Domain. error message: {0}.", ex.ToString());
            }
            return false;
        }

        public bool AccountsValidationTest(ref List<RMADAccountDto> accounts)
        {
            bool result = true;
            List<string> identities = new List<string>();
            int len = accounts.Count;
            for (int i = 0; i < len; i++)
            {
                var account = accounts[i];
                if (!CheckADAccountIsExistsInDomain(ref account))
                {
                    result = false;
                }
                else if (!string.IsNullOrEmpty(account.AccountSID) && account.DomainId > 0 &&
                    AccountDao.Exist(a => account.DomainId == a.DomainId && account.AccountSID.Equals(a.SID, StringComparison.OrdinalIgnoreCase)))
                {
                    account.Status = RMAccountStatus.Added;
                }
                else
                {
                    //identity用于过滤掉重复的User
                    var identity = account.DomainId + "_" + account.AccountSID;
                    if (identities.Contains(identity))
                    {
                        account.Status = RMAccountStatus.Repeated;
                    }
                    else
                    {
                        identities.Add(identity);
                    }
                }
            }
            return result;
        }

        public RMIdentity AuthenticateCredential(ADAccountCredential cren)
        {
            RMIdentity identity = new RMIdentity();
            identity.IsAuthenticated = false;
            identity.AuthenticationType = RMAuthenticationTypes.ADIntegration.ToString();

            if (cren == null || string.IsNullOrEmpty(cren.Password))
            {
                return identity;
            }

            //目前只支持Login Name登录,必须不带域名
            try
            {
                using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(cren.Domain, cren.UserName, cren.Password))
                {
                    ActiveDirectoryObject obj = null;
                    try
                    {
                        obj = checker.CreateDefaultSearcher().SingleSearchUser(cren.UserName);
                    }
                    catch(Exception ex)
                    {
                        logger.Warn("Use LDAP Search user {0}", ex.ToString());
                        obj = checker.CreateLDAPSearcher().SingleSearchUser(cren.UserName);
                    }
                    if (obj != null)
                    {
                        identity.Name = GetLoginName(obj);
                        identity.DisplayName = string.IsNullOrEmpty(obj.DisplayName) ? obj.CommonName : obj.DisplayName;
                        identity.ObjectSID = obj.ObjectSID;
                        var adAccount = AccountDao.GetADAccount(obj.ObjectSID);
                        if (adAccount != null)
                        {
                            identity.AccountType = RMAccountType.ADUser;
                            identity.AccountId = adAccount.Id;
                            identity.IsAuthenticated = true;
                        }
                        else
                        {
                            ActiveDirectorySearcher searcher = checker.CreateDefaultSearcher();
                            var groups = searcher.GetUserMemberOf(obj.DistinguishedName);
                            while (groups != null && groups.Count > 0)
                            {
                                var adGroupAccounts = AccountDao.GetADGroup(groups.Select(g => g.ObjectSID).ToList());
                                if (adGroupAccounts != null && adGroupAccounts.Count > 0)
                                {
                                    identity.AccountType = RMAccountType.ADGroup;
                                    identity.AccountId = adGroupAccounts[0].Id;
                                    identity.IsAuthenticated = true;
                                    break;
                                }
                                var tempGroups = new List<ActiveDirectoryObject>();
                                foreach (var group in groups)
                                {
                                    try
                                    {
                                        tempGroups.AddRange(searcher.GetGroupMemberOf(group.DistinguishedName));
                                    }
                                    catch
                                    {
                                    }
                                }
                                groups = tempGroups;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Ad Authentication mode login failed, userName: {0}, error message: {1}.", cren.UserName, ex.ToString());
            }
            
            return identity;
        }


        private ActiveDirectoryNameInfo AnalyzeName(string name)
        {
            string[] domainAndName = null;
            ActiveDirectoryNameInfo nameInfo = new ActiveDirectoryNameInfo();
            if (name.Contains("\\"))
            {
                domainAndName = name.Split('\\');
                nameInfo.Type = NameType.Classic;
                nameInfo.Domain = domainAndName[0];
                nameInfo.UserName = domainAndName[1];
            }
            else if (name.Contains("@"))
            {
                name = name.TrimEnd('*');
                string[] result = new string[2];
                int lastAt = name.LastIndexOf('@');
                nameInfo.UserName = name.Substring(0, lastAt);
                nameInfo.Domain = name.Substring(lastAt + 1);
                nameInfo.Type = NameType.UPN;
            }
            else
            {
                nameInfo.Type = NameType.SingleName;
                nameInfo.UserName = name;
            }
            if (nameInfo.Domain != null)
            {
                nameInfo.Domain = nameInfo.Domain.Trim();
            }
            if (nameInfo.UserName != null)
            {
                nameInfo.UserName = nameInfo.UserName.Trim();
            }
            return nameInfo;
        }

        private bool CheckADAccountIsExistsInDomain(ref RMADAccountDto accountInfo)
        {
            bool result = false;
            if (accountInfo.Status == RMAccountStatus.Available)
            {
                int domainId = accountInfo.DomainId;
                var domain = DomainDao.Find(d => d.Id == domainId);
                if (domain != null)
                {
                    var domainInfo = RMSecurityUtil.ConvertToDomainDto(domain, true);
                    using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(domainInfo.DomainName, domainInfo.UserName, domainInfo.Password))
                    {
                        ActiveDirectoryObject obj = checker.CreateObjectBySid(accountInfo.AccountSID);
                        if (obj != null)
                        {
                            result = true;
                            CopyADAccountInfo(ExchangeSearchResult(obj, domainInfo.Id), ref accountInfo);
                        }
                        else
                        {
                            accountInfo.Status = RMAccountStatus.Unavailable;
                        }
                    }
                }
                else
                {
                    accountInfo.Status = RMAccountStatus.Unavailable;
                }
            }
            else
            {
                var info = SearchSingleAccountByFullName(accountInfo.LoginName);
                if (info != null)
                {
                    result = true;
                    CopyADAccountInfo(info, ref accountInfo);
                }
                else
                {
                    accountInfo.Status = RMAccountStatus.Unavailable;
                }
            }
            
            return result;
        }

        public RMADAccountDto SearchSingleAccount(string name)
        {
            RMADAccountDto account = null;
            var activeDoamins = DomainDao.GetADDomains(true);
            if (activeDoamins == null && activeDoamins.Count == 0)
            {
                return null;
            }

            var adNameInfo = AnalyzeName(name);
            if (adNameInfo.Type == NameType.Classic)
            {
                if (!string.IsNullOrEmpty(adNameInfo.UserName))
                {
                    var matchDomains = activeDoamins.Where(d => d.NetBiosName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchDomains.Count > 0)
                    {
                        account = SearchAccountBySamAccountName(matchDomains, adNameInfo.UserName);
                    }
                }
            }
            else if(adNameInfo.Type == NameType.SingleName)
            {
                account = SearchAccountBySingleTypeName(activeDoamins, name);
            }
            else    //包含@符号的情况，先认为name是UPN或Mail进行搜索
            {
                if (!string.IsNullOrEmpty(adNameInfo.UserName))
                {
                    var matchDomains = activeDoamins.Where(d => d.NetBiosName.StartsWith(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.StartsWith(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchDomains.Count > 0)
                    {
                        account = SearchAccountByUPNTypeName(matchDomains, adNameInfo.UserName);
                    }
                    else  //从可用的Doamin里，用所有条件search
                    {
                        account = SearchAccountByName(activeDoamins, name);
                    }
                }
                else  //从可用的Doamin里，用所有条件search
                {
                    account = SearchAccountByName(activeDoamins, name);
                }
            }

            return account;
        }

        public RMADAccountDto SearchSingleAccountByFullName(string fullName)
        {
            RMADAccountDto account = null;
            var activeDoamins = DomainDao.GetADDomains(true);
            if (activeDoamins == null && activeDoamins.Count == 0)
            {
                return null;
            }

            var adNameInfo = AnalyzeName(fullName);
            if (string.IsNullOrEmpty(adNameInfo.UserName))
            {
                return null;
            }

            if (adNameInfo.Type == NameType.Classic)
            {
                var matchDomains = activeDoamins.Where(d => d.NetBiosName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matchDomains.Count > 0)
                {
                    account = SearchAccountBySamAccountName(matchDomains, adNameInfo.UserName, true);
                }
            }
            else if (adNameInfo.Type == NameType.SingleName)
            {
                account = SearchAccountBySingleTypeName(activeDoamins, fullName, true);
            }
            else   //包含@符号的情况，先认为name是UPN或Mail进行搜索
            {
                var matchDomains = activeDoamins.Where(d => d.NetBiosName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                                || d.RealName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matchDomains.Count > 0)
                {
                    account = SearchAccountByUPNTypeName(matchDomains, fullName, true);
                }
                else    //从可用的Doamin里，用所有条件search
                {
                    account = SearchAccountByName(activeDoamins, fullName, true);
                }
            }

            return account;
        }

        public List<RMADAccountDto> SearchAccounts(string name, int perDomainUserCount)
        {
            List<RMADAccountDto> accounts = null;
            var activeDoamins = DomainDao.GetADDomains(true);
            if (activeDoamins == null && activeDoamins.Count == 0)
            {
                return null;
            }

            var adNameInfo = AnalyzeName(name);
            if (string.IsNullOrEmpty(adNameInfo.UserName))
            {
                return null;
            }
            if (adNameInfo.Type == NameType.Classic)
            {
                if (!string.IsNullOrEmpty(adNameInfo.UserName))
                {
                    var matchDomains = activeDoamins.Where(d => d.NetBiosName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchDomains.Count > 0)
                    {
                        accounts = SearchAccountsBySamAccountName(matchDomains, adNameInfo.UserName, perDomainUserCount);
                    }
                }
            }
            else if (adNameInfo.Type == NameType.SingleName)
            {
                accounts = SearchAccountsBySingleTypeName(activeDoamins, name, perDomainUserCount);
            }
            else    //包含@符号的情况，先认为name是UPN或Mail进行搜索
            {
                if (string.IsNullOrEmpty(adNameInfo.UserName))
                {
                    accounts = SearchAccountsByName(activeDoamins, name, perDomainUserCount);
                }
                else
                {
                    var matchDomains = activeDoamins.Where(d => d.NetBiosName.StartsWith(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.StartsWith(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchDomains.Count > 0)
                    {
                        accounts = SearchAccountsByUPNTypeName(matchDomains, name, perDomainUserCount);
                    }
                    else    //从可用的Doamin里，用所有条件search
                    {
                        accounts = SearchAccountsByName(activeDoamins, name, perDomainUserCount);
                    }
                }
            }

            return accounts;
        }

        public List<RMADAccountDto> SearchAccounts(string name, int index, int count)
        {
            List<RMADAccountDto> accounts = null;
            var activeDoamins = DomainDao.GetADDomains(true);
            if (activeDoamins == null && activeDoamins.Count == 0)
            {
                return null;
            }

            var adNameInfo = AnalyzeName(name);
            if (string.IsNullOrEmpty(adNameInfo.UserName))
            {
                return null;
            }
            if (adNameInfo.Type == NameType.Classic)
            {
                if (!string.IsNullOrEmpty(adNameInfo.UserName))
                {
                    var matchDomains = activeDoamins.Where(d => d.NetBiosName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.Equals(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchDomains.Count > 0)
                    {
                        accounts = SearchAccountsBySamAccountName(matchDomains, adNameInfo.UserName, index, count);
                    }
                }
            }
            else if (adNameInfo.Type == NameType.SingleName)
            {
                accounts = SearchAccountsBySingleTypeName(activeDoamins, name, index, count);
            }
            else    //包含@符号的情况，先认为name是UPN或Mail进行搜索
            {
                if (!string.IsNullOrEmpty(adNameInfo.UserName))
                {
                    var matchDomains = activeDoamins.Where(d => d.NetBiosName.StartsWith(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)
                                                            || d.RealName.StartsWith(adNameInfo.Domain, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchDomains.Count > 0)
                    {
                        accounts = SearchAccountsByUPNTypeName(matchDomains, name, index, count);
                    }
                    else    //从可用的Doamin里，用所有条件search
                    {
                        accounts = SearchAccountsByName(activeDoamins, name, index, count);
                    }
                }
            }
            
            return accounts;
        }


        private RMADAccountDto SearchAccountBySamAccountName(List<RMADDomain> domains, string samName, bool isFullName = false)
        {
            string filter;
            if (isFullName)
            {
                filter = BuildSearchFilter(RMActiveDirectoryObjectType.All, samName, RMActiveDirectoryPropertyNames.SAMACCOUNTNAME);
            }
            else
            {
                filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, samName, RMActiveDirectoryPropertyNames.SAMACCOUNTNAME);
            }
            return SearchAccountFromDomains(domains, filter);
        }
        private List<RMADAccountDto> SearchAccountsBySamAccountName(List<RMADDomain> domains, string samName, int perDomainUserCount)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, samName, RMActiveDirectoryPropertyNames.SAMACCOUNTNAME);
            return SearchAccountsFromDomains(domains, filter, perDomainUserCount);
        }
        private List<RMADAccountDto> SearchAccountsBySamAccountName(List<RMADDomain> domains, string samName, int index, int count)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, samName, RMActiveDirectoryPropertyNames.SAMACCOUNTNAME);
            return SearchAccountsFromDomains(domains, filter, index, count);
        }

        private RMADAccountDto SearchAccountByUPNTypeName(List<RMADDomain> domains, string upnOrMail, bool isFullName = false)
        {
            string filter;
            if (isFullName)
            {
                filter = BuildSearchFilter(RMActiveDirectoryObjectType.All, upnOrMail, RMActiveDirectoryPropertyNames.MAIL, RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME);
            }
            else
            {
                filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, upnOrMail, RMActiveDirectoryPropertyNames.MAIL, RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME);
            }
            return SearchAccountFromDomains(domains, filter);
        }
        private List<RMADAccountDto> SearchAccountsByUPNTypeName(List<RMADDomain> domains, string upnOrMail, int perDomainUserCount)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, upnOrMail, RMActiveDirectoryPropertyNames.MAIL, RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME);
            return SearchAccountsFromDomains(domains, filter, perDomainUserCount);
        }
        private List<RMADAccountDto> SearchAccountsByUPNTypeName(List<RMADDomain> domains, string upnOrMail, int index, int count)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, upnOrMail, RMActiveDirectoryPropertyNames.MAIL, RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME);
            return SearchAccountsFromDomains(domains, filter, index, count);
        }

        private RMADAccountDto SearchAccountBySingleTypeName(List<RMADDomain> domains, string key, bool isFullName = false)
        {
            RMADAccountDto account = null;
            string filter;

            if (domains != null && domains.Count > 0)
            {
                if (isFullName)
                {
                    filter = BuildSearchFilter(RMActiveDirectoryObjectType.All, key,
                        RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                        //RMActiveDirectoryPropertyNames.FIRSTNAME,
                        //RMActiveDirectoryPropertyNames.LASTNAME,
                        RMActiveDirectoryPropertyNames.DISPLAY_NAME);
                }
                else
                {
                    filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, key,
                        RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                        RMActiveDirectoryPropertyNames.FIRSTNAME,
                        RMActiveDirectoryPropertyNames.LASTNAME,
                        RMActiveDirectoryPropertyNames.DISPLAY_NAME);
                }
                account = SearchAccountFromDomains(domains, filter);
            }
            return account;
        }
        private List<RMADAccountDto> SearchAccountsBySingleTypeName(List<RMADDomain> domains, string name, int index, int count)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, name,
                RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                RMActiveDirectoryPropertyNames.FIRSTNAME,
                RMActiveDirectoryPropertyNames.LASTNAME,
                RMActiveDirectoryPropertyNames.DISPLAY_NAME,
                RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME,
                RMActiveDirectoryPropertyNames.MAIL);
            return SearchAccountsFromDomains(domains, filter, index, count);
        }
        private List<RMADAccountDto> SearchAccountsBySingleTypeName(List<RMADDomain> domains, string name, int perDomainUserCount)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, name,
                RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                RMActiveDirectoryPropertyNames.FIRSTNAME,
                RMActiveDirectoryPropertyNames.LASTNAME,
                RMActiveDirectoryPropertyNames.DISPLAY_NAME,
                RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME,
                RMActiveDirectoryPropertyNames.MAIL);
            return SearchAccountsFromDomains(domains, filter, perDomainUserCount);
        }

        private RMADAccountDto SearchAccountByName(List<RMADDomain> domains, string key, bool isFullName = false)
        {
            RMADAccountDto account = null;
            string filter;

            if (domains != null && domains.Count > 0)
            {
                if (isFullName)
                {
                    filter = BuildSearchFilter(RMActiveDirectoryObjectType.All, key,
                        RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                        //RMActiveDirectoryPropertyNames.FIRSTNAME,
                        //RMActiveDirectoryPropertyNames.LASTNAME,
                        RMActiveDirectoryPropertyNames.DISPLAY_NAME,
                        RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME,
                        RMActiveDirectoryPropertyNames.MAIL);
                }
                else
                {
                    filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, key,
                        RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                        RMActiveDirectoryPropertyNames.FIRSTNAME,
                        RMActiveDirectoryPropertyNames.LASTNAME,
                        RMActiveDirectoryPropertyNames.DISPLAY_NAME,
                        RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME,
                        RMActiveDirectoryPropertyNames.MAIL);
                }
                account = SearchAccountFromDomains(domains, filter);
            }
            return account;
        }
        private List<RMADAccountDto> SearchAccountsByName(List<RMADDomain> domains, string name, int index, int count)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, name,
                RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                RMActiveDirectoryPropertyNames.FIRSTNAME,
                RMActiveDirectoryPropertyNames.LASTNAME,
                RMActiveDirectoryPropertyNames.DISPLAY_NAME,
                RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME,
                RMActiveDirectoryPropertyNames.MAIL);
            return SearchAccountsFromDomains(domains, filter, index, count);
        }
        private List<RMADAccountDto> SearchAccountsByName(List<RMADDomain> domains, string name, int perDomainUserCount)
        {
            var filter = BuildWildcardFilter(RMActiveDirectoryObjectType.All, name,
                RMActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                RMActiveDirectoryPropertyNames.FIRSTNAME,
                RMActiveDirectoryPropertyNames.LASTNAME,
                RMActiveDirectoryPropertyNames.DISPLAY_NAME,
                RMActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME,
                RMActiveDirectoryPropertyNames.MAIL);
            return SearchAccountsFromDomains(domains, filter, perDomainUserCount);
        }

        private RMADAccountDto SearchAccountFromDomain(RMDomainDto domain, string filter)
        {
            try
            {
                using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(domain.DomainName, domain.UserName, domain.Password))
                {
                    ActiveDirectorySearcher searcher = checker.CreateLDAPSearcher();
                    var result = searcher.SetFilter(filter).SingleSearch();
                    if (result != null)
                    {
                        return ExchangeSearchResult(result, domain.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occured when get single account info, filter: {0}, error message: {1}.", filter, ex.ToString());
            }
            return null;
        }
        private RMADAccountDto SearchAccountFromDomains(List<RMADDomain> domains, string filter)
        {
            RMADAccountDto account = null;

            if (domains != null && domains.Count > 0)
            {
                foreach (var domain in domains)
                {
                    account = SearchAccountFromDomain(RMSecurityUtil.ConvertToDomainDto(domain, true), filter);
                    if (account != null)
                    {
                        break;
                    }
                }
            }
            return account;
        }
        private List<RMADAccountDto> SearchAccountsFromDomains(List<RMADDomain> domains, string filter, int perDomainUserCount)
        {
            List<RMADAccountDto> accounts = new List<RMADAccountDto>();

            if (domains != null && domains.Count > 0)
            {
                foreach (var domain in domains)
                {
                    try
                    {
                        using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(domain.DomainName, domain.UserName, mDBEncrypt.DecryptPasswordXmlToString(domain.Password)))
                        {
                            ActiveDirectorySearcher searcher = checker.CreateLDAPSearcher();
                            searcher = searcher.SetFilter(filter);
                            searcher.Searcher.ClientTimeout = new TimeSpan(0, 2, 0);
                            searcher.SetPageSizeLimit(perDomainUserCount);
                            var results = searcher.Search();
                            if (results != null)
                            {
                                foreach (var obj in results)
                                {
                                    if (obj != null)
                                    {
                                        accounts.Add(ExchangeSearchResult(obj, domain.Id));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Search ad account from Domain: '{0}' failed. error message: {1}.", domain.DomainName, ex.ToString());
                    }
                }

                if (accounts.Count > 0)
                {
                    return (from a in accounts orderby a.DisplayName ascending select a).ToList();
                }
            }

            return accounts;
        }
        private List<RMADAccountDto> SearchAccountsFromDomains(List<RMADDomain> domains, string filter, int index, int count)
        {
            List<RMADAccountDto> accounts = new List<RMADAccountDto>();

            if (domains != null && domains.Count > 0)
            {
                foreach (var domain in domains)
                {
                    try
                    {
                        int lackCount = count - accounts.Count;
                        if (lackCount > 0)
                        {
                            using (ActiveDirectoryDomain checker = new ActiveDirectoryDomain(domain.DomainName, domain.UserName, mDBEncrypt.DecryptPasswordXmlToString(domain.Password)))
                            {
                                ActiveDirectorySearcher searcher = checker.CreateLDAPSearcher();
                                searcher = searcher.SetFilter(filter);
                                searcher.Searcher.ClientTimeout = new TimeSpan(0, 2, 0);
                                var results = searcher.YieldSearch(index);
                                if (results != null)
                                {
                                    SearchResult tempResult;
                                    do
                                    {
                                        tempResult = results.Current as SearchResult;
                                        if (tempResult != null)
                                        {
                                            accounts.Add(ExchangeSearchResult(checker.CreateObject(tempResult), domain.Id));
                                            lackCount--;
                                        }

                                    } while (results.MoveNext() && lackCount > 0);
                                }
                                else
                                {
                                    var totalResults = searcher.SetPageSizeLimit(index).YieldSearch();
                                    if (totalResults != null)
                                    {
                                        int tCount = totalResults.Count;
                                        index -= tCount;
                                        if (index > 0)
                                        {
                                            //处理 AD 服务器设置的Page size limit小于我们设置的limit的情况
                                            var temResults = searcher.YieldSearch(tCount);
                                            if (temResults != null)
                                            {
                                                while (--index >= 0)
                                                {
                                                    if (!temResults.MoveNext())
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Search ad account from Domain: '{0}' failed. error message: {1}.", domain.DomainName, ex.ToString());
                    }
                }

                if (accounts.Count > 0)
                {
                    return (from a in accounts orderby a.DisplayName ascending select a).ToList();
                }
            }

            return accounts;
        }

        /// <summary>
        /// 组建search的语句
        /// </summary>
        /// <param name="type">search: User or Group or All</param>
        /// <param name="finalKey">最终的search关键字</param>
        /// <param name="properties">匹配关键字的属性，不允许为空</param>
        /// <returns></returns>
        private string BuildSearchFilter(RMActiveDirectoryObjectType type, string finalKey, params string[] properties)
        {
            if (finalKey == null && finalKey.Trim().Length == 0)
            {
                logger.Error("The filter search key is null or empty when build search filter string.");
                throw new Exception("The filter search key isn't allow null or empty.");
            }

            bool hasProperty = false;
            bool oneProperty = false;
            if (properties != null && properties.Length > 0)
            {
                oneProperty = properties.Length == 1;
                hasProperty = true;
            }
            StringBuilder filter = new StringBuilder();
            switch (type)
            {
                case RMActiveDirectoryObjectType.All:
                    if (hasProperty)
                    {
                        filter.AppendFormat("(&(|(&(objectClass=user)(objectCategory=person))(&(objectClass=group)(objectCategory=group)))({0}", oneProperty ? "" : "|");
                    }
                    else
                    {
                        filter.Append("(|(&(objectClass=user)(objectCategory=person))(&(objectClass=group)(objectCategory=group)))");
                    }
                    break;
                case RMActiveDirectoryObjectType.User:
                    if (hasProperty)
                    {
                        filter.AppendFormat("(&(&(objectClass=user)(objectCategory=person))({0}", oneProperty ? "" : "|");
                    }
                    else
                    {
                        filter.Append("(&(objectClass=user)(objectCategory=person))");
                    }
                    break;
                case RMActiveDirectoryObjectType.Group:
                    if (hasProperty)
                    {
                        filter.AppendFormat("(&(&(objectClass=group)(objectCategory=group))({0}", oneProperty ? "" : "|");
                    }
                    else
                    {
                        filter.Append("(&(objectClass=group)(objectCategory=group))"); 
                    }
                    break;
                default:
                    break;
            }

            if (hasProperty)
            {
                foreach (string prop in properties)
                {
                    filter.AppendFormat("({0}={1})", prop, finalKey);
                }

                if (oneProperty)
                {
                    filter.Append(")");
                }
                else
                {
                    filter.Append("))");
                }
            }

            return filter.ToString();
        }

        /// <summary>
        /// 组建Contain方式Search的filter语句（通配"searchKey*"）
        /// </summary>
        /// <param name="type">search: User or Group or All</param>
        /// <param name="searchKey">search的关键字</param>
        /// <param name="properties">匹配关键字的属性，不允许为空</param>
        /// <returns></returns>
        private string BuildWildcardFilter(RMActiveDirectoryObjectType type, string searchKey, params string[] properties)
        {
            return BuildSearchFilter(type, string.Format("{0}**", searchKey), properties);
        }

        private RMADAccountDto ExchangeSearchResult(ActiveDirectoryObject obj, int domainId)
        {
            return new RMADAccountDto() {
                AccountSID = obj.ObjectSID,
                DomainId = domainId,
                Domain = obj.DomainName,
                LoginName = GetLoginName(obj),
                DisplayName = string.IsNullOrEmpty(obj.DisplayName) ? obj.CommonName : obj.DisplayName,
                Status = RMAccountStatus.Available,
                Type = obj.IsGroup ? RMAccountType.ADGroup : RMAccountType.ADUser
            };
        }

        private void CopyADAccountInfo(RMADAccountDto source, ref RMADAccountDto target)
        {
            if (source != null && target != null)
            {
                target.AccountSID = source.AccountSID;
                target.DomainId = source.DomainId;
                target.Domain = source.Domain;
                target.LoginName = source.LoginName;
                target.DisplayName = source.DisplayName;
                target.Status = source.Status;
                target.Type = source.Type;
            }
        }

        private string GetLoginName(ActiveDirectoryObject obj)
        {
            string loginname = obj.GetPropertySingleValue(ActiveDirectoryPropertyNames.MSDS_PRINCIPAL_NAME);
            if (string.IsNullOrEmpty(loginname))
            {
                loginname = string.Format("{0}\\{1}", obj.Domain.NetBIOSName, obj.SamAccountName);
            }
            return loginname;
        }
    }

    public static class ActiveDirectorySearcherExtension
    {
        public static List<ActiveDirectoryObject> GetGroupMemberOf(this ActiveDirectorySearcher searcher, string userDistingishedName)
        {
            string searchString = string.Format("(&(objectCategory=group)(objectClass=group)({0}={1}){2})", "distinguishedname", userDistingishedName, searcher.BaseFilter);
            searcher.SetFilter(searchString);
            List<ActiveDirectoryObject> memberof = new List<ActiveDirectoryObject>();
            try
            {
                searcher.Searcher.Filter = searcher.Filter;
                searcher.Searcher.SearchScope = searcher.Scope;
                if (searcher.PropertiesToLoad != null)
                {
                    searcher.Searcher.PropertiesToLoad.AddRange(searcher.PropertiesToLoad);
                }
                searcher.Searcher.PageSize = searcher.PageSize;
                searcher.Searcher.SizeLimit = searcher.SizeLimit;
                SearchResult results = searcher.Searcher.FindOne();
                foreach (object obj in results.Properties["memberof"])
                {
                    if (obj.GetType().Equals(typeof(System.String)))
                    {
                        ActiveDirectoryObject member = searcher.Checker.CreateObject(obj.ToString());
                        memberof.Add(member);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return memberof;
        }
    }
}