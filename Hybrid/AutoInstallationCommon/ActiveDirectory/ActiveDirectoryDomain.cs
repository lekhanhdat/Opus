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
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;

namespace AutoInstallationCommon.ActiveDirectory
{
    /// <summary>
    ///     Need to keep that, one domain one checker
    /// </summary>
    public class ActiveDirectoryDomain : IDisposable
    {
        #region ==私有全局变量==

        private static readonly Logs log = Logs.CreateUniformLog();
        private readonly string mUserName = string.Empty;
        private readonly string mPassword = string.Empty;
        private readonly bool mDirectAccess;
        private readonly string mDistinguishedName = string.Empty;
        private static string[] searchObject_SupportedAttributes;

        #endregion

        #region ==公有全局变量==

        public bool GCConnected { get; private set; }
        public bool LDAPConnected { get; private set; }
        public bool Connected => GCConnected || LDAPConnected;
        public ActiveDirectoryEntry Entry { get; set; }

        public ActiveDirectoryEntry EntryForExtend { get; set; }
        //public static string[] SearchGroup_SupportedAttributes = new string[] { "cn", "distinguishedName", "name",  "sAMAccountName" };
        //public static string[] SearchUser_SupportedAttributes = new string[] { "cn", "distinguishedName", "name", "sAMAccountName", "mail", "department","displayName","userprincipalname" };

        public static string[] SearchGroup_SupportedAttributes = {"name", "sAMAccountName"};

        public static string[] SearchUser_SupportedAttributes =
            {"name", "sAMAccountName", "mail", "displayName", "userprincipalname"};

        #endregion

        #region ==构造函数==

        public ActiveDirectoryDomain()
        {
            try
            {
                mDirectAccess = true;
                RealDomainName = Domain.GetCurrentDomain().Name;
                log.Debug("Auto selected domain: {0}", RealDomainName);
            }
            catch (Exception e)
            {
                log.Warn(
                    "Cannot bind server domain automatically, please check if the Domain Control & DNS setting is set correctly. Exception: {0}",
                    e.Message);
                throw;
            }
        }

        public ActiveDirectoryDomain(string orgName)
        {
            mDirectAccess = true;
            RealDomainName = orgName;
        }

        public ActiveDirectoryDomain(string domainName, string userName, string password)
        {
            mDirectAccess = false;
            RealDomainName = domainName;
            mUserName = userName;
            mPassword = password;
        }

        public ActiveDirectoryDomain(string domainName, string distinguishedName)
        {
            mDirectAccess = true;
            RealDomainName = domainName;
            mDistinguishedName = string.Format("/{0}", distinguishedName.Replace("/", "\\/"));
        }

        public ActiveDirectoryDomain(string domainName, string distinguishedName, string userName, string password)
        {
            mDirectAccess = false;
            RealDomainName = domainName;
            mUserName = userName;
            mPassword = password;
            mDistinguishedName = string.Format("/{0}", distinguishedName.Replace("/", "\\/"));
        }

        #endregion

        #region ==属性==

        public string RealDomainName { get; } = string.Empty;

        public static string[] SearchObject_SupportedAttributes
        {
            get
            {
                if (searchObject_SupportedAttributes == null)
                {
                    var realSupportedAttributesSet = new HashSet<string>();
                    foreach (var key in SearchGroup_SupportedAttributes) realSupportedAttributesSet.Add(key);

                    foreach (var key in SearchUser_SupportedAttributes) realSupportedAttributesSet.Add(key);
                    searchObject_SupportedAttributes = realSupportedAttributesSet.ToArray();
                    log.Debug("ActiveDirectory supports attributes: {0}",
                        string.Join(",", searchObject_SupportedAttributes));
                }

                return searchObject_SupportedAttributes;
            }
        }

        private string netbiosName = string.Empty;

        public string NetBIOSName
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(netbiosName))
                    {
                        var entry = new DirectoryEntry(string.Format("LDAP://{0}/{1}", RealDomainName, "RootDSE"),
                            mUserName, mPassword);
                        var domainNCName =
                            entry.Properties[ActiveDirectoryPropertyNames.NamingContext.DEFAULT_NAMING_CONTEXT][0]
                                .ToString();
                        var configName =
                            entry.Properties[ActiveDirectoryPropertyNames.NamingContext.CONFIGURATION_NAMING_CONTEXT][0]
                                .ToString();
                        var configEntry =
                            new DirectoryEntry(string.Format("LDAP://{0}/{1}", RealDomainName, configName), mUserName,
                                mPassword);
                        configEntry.RefreshCache(new[]
                        {
                            ActiveDirectoryPropertyNames.NamingContext.NETBIOSNAME,
                            ActiveDirectoryPropertyNames.NamingContext.N_CNAME
                        });
                        var searcher = new DirectorySearcher(configEntry);
                        searcher.Filter = string.Format("(&(objectClass={0})({1}={2}))",
                            ObjectClasses.NamingContext.CROSS_REF,
                            ActiveDirectoryPropertyNames.NamingContext.N_CNAME,
                            domainNCName);
                        var sr = searcher.FindOne();
                        if (sr != null)
                        {
                            netbiosName = sr.Properties[ActiveDirectoryPropertyNames.NamingContext.NETBIOSNAME][0]
                                .ToString();
                        }
                        else
                        {
                            netbiosName = string.Empty;
                            throw new Exception("Failed to get domain configuration infomations.");
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Failed to get NetBiosName: {0}. Exception: {1}", RealDomainName, e.Message);
                    netbiosName = null;
                }

                return netbiosName;
            }
        }

        #endregion

        #region ==方法==

        private static readonly object _trustLock = new object();

        /// <summary>
        ///     判断Domain是否是外部信任域
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>bool</returns>
        public bool IsExternalTrusted(string domain)
        {
            if (string.Equals(RealDomainName, domain, StringComparison.OrdinalIgnoreCase)) return false;

            var cachePair = string.Format("{0}->{1}", RealDomainName, domain);
            if (!(MemeryCache.GetValue<bool?>(cachePair) == null)) return MemeryCache.GetValue<bool?>(cachePair).Value;
            lock (_trustLock)
            {
                log.Debug("Check if {0} and {1} are external trusted domains.", RealDomainName, domain);
                try
                {
                    Domain currentDomain = null;
                    if (string.IsNullOrEmpty(RealDomainName))
                    {
                        currentDomain = Domain.GetCurrentDomain();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mUserName) || string.IsNullOrEmpty(mPassword))
                            currentDomain =
                                Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, RealDomainName));
                        else
                            currentDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain,
                                RealDomainName, mUserName, mPassword));
                    }

                    var trustInfo = currentDomain.GetTrustRelationship(domain);
                    if (trustInfo != null && trustInfo.TrustType == TrustType.External)
                    {
                        MemeryCache.CreateItem<bool?>(cachePair, true);
                        log.Debug("{0} and {1} are external trusted domains: {2}", RealDomainName, domain, true);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    MemeryCache.CreateItem<bool?>(cachePair, false);
                    log.Warn("Failed to check trust relationship between {0} and {1}. Exception: {2}", RealDomainName,
                        domain, e.Message);
                }

                log.Debug("{0} and {1} are external trusted domains: {2}", RealDomainName, domain, false);
                return false;
            }
        }


        /// <summary>
        ///     Forest trust
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public bool IsForestTrusted(string forest)
        {
            if (string.Equals(RealDomainName, forest, StringComparison.OrdinalIgnoreCase)) return false;

            var cachePair = string.Format("{0}<->{1}", RealDomainName, forest);
            if (!(MemeryCache.GetValue<bool?>(cachePair) == null)) return MemeryCache.GetValue<bool?>(cachePair).Value;


            lock (_trustLock)
            {
                log.Debug("Check if {0} and {1} are forest trusted.", RealDomainName, forest);
                try
                {
                    Forest currentForest = null;
                    if (string.IsNullOrEmpty(RealDomainName))
                    {
                        currentForest = Forest.GetCurrentForest();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mUserName) || string.IsNullOrEmpty(mPassword))
                            currentForest =
                                Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, RealDomainName));
                        else
                            currentForest = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest,
                                RealDomainName, mUserName, mPassword));
                    }

                    TrustRelationshipInformation trustInfo = currentForest.GetTrustRelationship(forest);
                    if (trustInfo != null && trustInfo.TrustType == TrustType.Forest)
                    {
                        log.Debug("{0} and {1} are forest trusted : {2}", RealDomainName, forest, true);
                        MemeryCache.CreateItem<bool?>(cachePair, true);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    MemeryCache.CreateItem<bool?>(cachePair, false);
                    log.Warn("Failed to check trust relationship between {0} and {1}. Exception: {2}", RealDomainName,
                        forest, e.Message);
                }

                log.Debug("{0} and {1} are forest trusted: {2}", RealDomainName, forest, false);

                return false;
            }
        }

        /// <summary>
        ///     判断Domain是否是内部信任域
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>bool</returns>
        public bool IsInternalTrusted(string domain)
        {
            if (string.Equals(RealDomainName, domain, StringComparison.OrdinalIgnoreCase)) return false;

            var cachePair = string.Format("{0}=>{1}", RealDomainName, domain);
            if (!(MemeryCache.GetValue<bool?>(cachePair) == null)) return MemeryCache.GetValue<bool?>(cachePair).Value;
            lock (_trustLock)
            {
                Domain currentDomain = null;
                try
                {
                    if (string.IsNullOrEmpty(RealDomainName))
                    {
                        currentDomain = Domain.GetCurrentDomain();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mUserName) || string.IsNullOrEmpty(mPassword))
                            currentDomain =
                                Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, RealDomainName));
                        else
                            currentDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain,
                                RealDomainName, mUserName, mPassword));
                    }

                    var trustInfo = currentDomain.GetTrustRelationship(domain);
                    if (trustInfo != null && (trustInfo.TrustType == TrustType.ParentChild ||
                                              trustInfo.TrustType == TrustType.TreeRoot))
                    {
                        MemeryCache.CreateItem<bool?>(cachePair, true);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    MemeryCache.CreateItem<bool?>(cachePair, false);
                    log.Warn("Failed to check trust relationship between {0} and {1}. Exception: {2}", RealDomainName,
                        domain, e.Message);
                }

                return false;
            }
        }


        /// <summary>
        ///     获取GC服务对象
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="directAccess">false:获取指定域GC ; true:获取本地所在域GC</param>
        /// <returns></returns>
        public ActiveDirectoryDomain TryOthers(string domainName, bool directAccess = false)
        {
            ActiveDirectoryDomain other;
            try
            {
                if (!directAccess)
                {
                    other = new ActiveDirectoryDomain(domainName, mUserName, mPassword).ConnectGlobalCatalog();
                    log.Debug("Bound domain: {0}", domainName);
                }
                else
                {
                    other = new ActiveDirectoryDomain().ConnectGlobalCatalog();
                    log.Debug("Bound current server domain.");
                }
            }
            catch (Exception e)
            {
                log.Warn(
                    "Cannot bind server domain: {0} automatically, please check if the Domain Control & DNS setting is set correctly. Exception: {1}",
                    domainName, e.Message);
                return null;
            }

            return other;
        }

        /// <summary>
        ///     通过distinguishedName获取GC服务对象
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="directAccess">false:获取指定域GC ; true:获取本地所在域GC</param>
        /// <returns></returns>
        public ActiveDirectoryDomain TryOthers(string domainName, string distinguishedName, bool directAccess = false)
        {
            ActiveDirectoryDomain other;
            try
            {
                if (!directAccess)
                {
                    other = new ActiveDirectoryDomain(domainName, distinguishedName, mUserName, mPassword)
                        .ConnectGlobalCatalog();
                    log.Debug("Bound domain: {0}/{1}", domainName, distinguishedName);
                }
                else
                {
                    other = new ActiveDirectoryDomain(domainName, distinguishedName).ConnectGlobalCatalog();
                    log.Debug("Bound domain: {0}/{1}", domainName, distinguishedName);
                }
            }
            catch (Exception e)
            {
                log.Warn(
                    "Cannot bind server domain: {0}/{1} automatically, please check if the Domain Control & DNS setting is set correctly. Exception: {2}",
                    domainName, distinguishedName, e.Message);
                return null;
            }

            return other;
        }

        /// <summary>
        ///     Create Directory Entry via Global Category protocol, usually be used to search
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryDomain ConnectGlobalCatalog()
        {
            if (Entry == null)
            {
                if (!mDirectAccess)
                    Entry = new ActiveDirectoryEntry(string.Format("GC://{0}{1}", RealDomainName, mDistinguishedName),
                        mUserName, mPassword);
                else
                    Entry = new ActiveDirectoryEntry(string.Format("GC://{0}{1}", RealDomainName, mDistinguishedName));
                try
                {
                    var name = Entry.Name;
                    GCConnected = true;
                    log.Debug("Successfully Bound Global Catalog. Domain: {0}", RealDomainName);
                }
                catch (Exception e)
                {
                    GCConnected = false;
                    log.Warn(
                        "Cannot bind server domain via Global Catalog: {0} , please check if the Domain Control & DNS setting is set correctly. Exception: {1}",
                        RealDomainName, e.Message);
                }
            }

            return this;
        }

        /// <summary>
        ///     Create Directory Entry via Lightweight Data Access Protocol, usually be used to retreive and modify  data
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryDomain ConnectLDAP()
        {
            if (EntryForExtend == null)
            {
                if (!mDirectAccess)
                    EntryForExtend = new ActiveDirectoryEntry(
                        string.Format("LDAP://{0}{1}", RealDomainName, mDistinguishedName), mUserName, mPassword);
                else
                    EntryForExtend =
                        new ActiveDirectoryEntry(string.Format("LDAP://{0}{1}", RealDomainName, mDistinguishedName));
                try
                {
                    var name = EntryForExtend.Name;
                    LDAPConnected = true;
                    log.Debug("Successfully Bound LDAP. Domain: {0}", RealDomainName);
                }
                catch (Exception e)
                {
                    LDAPConnected = false;
                    log.Warn(
                        "Cannot bind server domain via LDAP: {0} , please check if the Domain Control & DNS setting is set correctly. Exception: {1}",
                        RealDomainName, e.Message);
                }
            }

            return this;
        }

        /// <summary>
        ///     Create a default searcher to search data in Active Directory
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="needToLoad"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "distinguishedname")]
        public ActiveDirectorySearcher CreateDefaultSearcher(string filter, params string[] needToLoad)
        {
            var searcher = new ActiveDirectorySearcher(this).SetPageSize(120)
                .SetPageSizeLimit(100)
                .SetScope(SearchScope.Subtree)
                .ToLoad(needToLoad)
                .LoadMore(ActiveDirectoryPropertyNames.DISTINGUISHED_NAME)
                .SetFilter(filter);
            return searcher;
        }

        /// <summary>
        ///     Create a default Global Category Searcher
        /// </summary>
        /// <returns></returns>
        public ActiveDirectorySearcher CreateDefaultSearcher()
        {
            var searcher = new ActiveDirectorySearcher(this).SetPageSize(120)
                .SetPageSizeLimit(100)
                .SetScope(SearchScope.Subtree);
            return searcher;
        }

        /// <summary>
        ///     Create a default LDAP Searcher
        /// </summary>
        /// <returns></returns>
        public ActiveDirectorySearcher CreateLDAPSearcher()
        {
            var searcher = new ActiveDirectorySearcher(this, true).SetPageSize(120)
                .SetPageSizeLimit(100)
                .SetScope(SearchScope.Subtree);
            return searcher;
        }

        /// <summary>
        ///     Create an ActiveDirectoryEntry from SearchResult, and then the ActiveDirectoryEntry can be cast to
        ///     ActiveDirectoryObject
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "distinguishedname")]
        public ActiveDirectoryEntry CreateEntry(SearchResult searchResult, string protocol = "GC://")
        {
            var dn = searchResult.Properties[ActiveDirectoryPropertyNames.DISTINGUISHED_NAME][0].ToString();
            log.Debug("Creating Entry for : {0}", dn);

            ActiveDirectoryEntry entry = null;
            try
            {
                if (!mDirectAccess)
                {
                    entry = new ActiveDirectoryEntry();
                    entry.Path = string.Format("{2}{0}/{1}", RealDomainName, dn.Replace("/", "\\/"), protocol);
                    entry.Password = mPassword;
                    entry.Username = mUserName;
                    entry.Checker = this;
                }
                else
                {
                    entry = new ActiveDirectoryEntry(string.Format("{2}{0}/{1}", RealDomainName, dn.Replace("/", "\\/"),
                        protocol));
                    entry.Checker = this;
                }

                log.Debug("Successfully to create Entry for: {0}", dn);
            }
            catch (Exception e)
            {
                log.Warn("Failed to create Entry for :{0}. Exception: {1}", dn, e.Message);
            }

            return entry;
        }

        /// <summary>
        ///     Direct create an ActiveDirectoryEntry from Distinguished Name.
        /// </summary>
        /// <param name="distinguishedName">For example: CreateEntry("CN=someone, OU=SomeOrg, DC=domain, DC=com");</param>
        /// <returns></returns>
        public ActiveDirectoryEntry CreateEntry(string distinguishedName, string protocol = "GC://")
        {
            log.Debug("Creating Entry for : {0}", distinguishedName);
            ActiveDirectoryEntry entry = null;
            try
            {
                if (!mDirectAccess)
                {
                    entry = new ActiveDirectoryEntry();
                    entry.Path = string.Format("{2}{0}/{1}", RealDomainName, distinguishedName.Replace("/", "\\/"),
                        protocol);
                    entry.Password = mPassword;
                    entry.Username = mUserName;
                    entry.Checker = this;
                }
                else
                {
                    entry = new ActiveDirectoryEntry(string.Format("{2}{0}/{1}", RealDomainName,
                        distinguishedName.Replace("/", "\\/"), protocol));
                    entry.Checker = this;
                }

                log.Debug("Successfully to create Entry for: {0}", distinguishedName);
            }
            catch (Exception e)
            {
                log.Warn("Failed to create Entry for :{0}. Exception: {1}", distinguishedName, e.Message);
            }

            return entry;
        }

        public ActiveDirectoryEntry CreateEntryBySid(string sid, string protocol = "LDAP://")
        {
            log.Debug("Creating Entry for : {0}", sid);
            ActiveDirectoryEntry entry = null;
            try
            {
                if (!mDirectAccess)
                {
                    entry = new ActiveDirectoryEntry();
                    entry.Path = string.Format("{1}{2}/<SID={0}>", sid, protocol, RealDomainName);
                    entry.Password = mPassword;
                    entry.Username = mUserName;
                    entry.Checker = this;
                }
                else
                {
                    entry = new ActiveDirectoryEntry(string.Format("{1}<SID={0}>", sid, protocol));
                    entry.Checker = this;
                }

                log.Debug("Successfully to create Entry for: {0}", sid);
            }
            catch (Exception e)
            {
                log.Warn("Failed to create Entry for :{0}. Exception: {1}", sid, e.Message);
            }

            return entry;
        }

        /// <summary>
        ///     Create an ActiveDirectoryObject from a SearchResult
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        public ActiveDirectoryObject CreateObject(SearchResult searchResult, string protocol = "GC://")
        {
            return CreateEntry(searchResult, protocol)
                .ToActiveDirectoryObject();
        }

        /// <summary>
        ///     Direct create an ActiveDirectoryObject from Distinguished Name.
        /// </summary>
        /// <param name="distinguishedName">For example: CreateObject("CN=someone, OU=SomeOrg, DC=domain, DC=com");</param>
        /// <returns></returns>
        public ActiveDirectoryObject CreateObject(string distinguishedName)
        {
            return CreateEntry(distinguishedName)
                .ToActiveDirectoryObject();
        }

        public ActiveDirectoryObject CreateObjectBySid(string sid)
        {
            return CreateEntryBySid(sid).ToActiveDirectoryObject();
        }

        #region ==静态方法==

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "distinguishedname")]
        public static string GetFullDomainName(PropertyCollection properties)
        {
            return GetFullDomainName(properties[ActiveDirectoryPropertyNames.DISTINGUISHED_NAME][0].ToString());
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "distinguishedname")]
        public static string GetFullDomainName(ResultPropertyCollection properties)
        {
            return GetFullDomainName(properties[ActiveDirectoryPropertyNames.DISTINGUISHED_NAME][0].ToString());
        }

        public static string GetFullDomainName(string distinguishedname)
        {
            log.Debug("Compute full domain name of {0}", distinguishedname);
            return distinguishedname.Substring(distinguishedname.IndexOf("DC=", StringComparison.OrdinalIgnoreCase))
                .Replace("DC=", "").Replace(",", ".");
        }

        #endregion

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (EntryForExtend != null) EntryForExtend.Dispose();

            if (Entry != null) Entry.Dispose();
        }

        #endregion
    }
}