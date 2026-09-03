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

using AvePoint.Common.ActiveDirectoryWrapper.MemeryCache;
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Common.ActiveDirectoryWrapper.ActiveDirectoryChecker.#get_ForestInternalTrustDomains()", MessageId = "distinguishedname")]
namespace AvePoint.Common.ActiveDirectoryWrapper
{
    /// <summary>
    /// Need to keep that, one domain one checker
    /// </summary>
    public class ActiveDirectoryDomain: IDisposable
    {
        #region ==私有全局变量==

        private static AveLogger logger = AveLogger.GetInstance(typeof(ActiveDirectoryDomain));
        private string mDomainName = string.Empty;
        private string mUserName = string.Empty;
        private string mPassword = string.Empty;
        private bool mDirectAccess = false;
        private string mDistinguishedName = string.Empty;      
        private bool gc_connected = false;        
        private bool ldap_connected = false;
        private static string[] searchObject_SupportedAttributes = null;

        #endregion

        #region ==公有全局变量==

        public bool GCConnected { get { return this.gc_connected; } }
        public bool LDAPConnected { get { return this.ldap_connected; } }
        public bool Connected { get { return this.gc_connected || this.ldap_connected; } }
        public ActiveDirectoryEntry Entry { get; set; }
        public ActiveDirectoryEntry EntryForExtend { get; set; }
        public static string[] SearchGroup_SupportedAttributes = new string[] { "cn", "distinguishedName", "name",  "sAMAccountName" };
        public static string[] SearchUser_SupportedAttributes = new string[] { "cn", "distinguishedName", "name", "sAMAccountName", "mail", "department" };

        #endregion

        #region ==构造函数==

        public ActiveDirectoryDomain()
        {
            try
            {
                this.mDirectAccess = true;
                this.mDomainName = Domain.GetCurrentDomain().Name;
                logger.Debug("Auto selected domain: {0}", this.mDomainName);
            }
            catch (Exception e)
            {
                logger.Warn("Cannot bind server domain automatically, please check if the Domain Control & DNS setting is set correctly. Exception: {0}", e.Message);
                throw;
            }
        }

        public ActiveDirectoryDomain(string orgName)
        {
            this.mDirectAccess = true;
            this.mDomainName = orgName;
        }

        public ActiveDirectoryDomain(string domainName, string userName, string password)
        {
            this.mDirectAccess = false;
            this.mDomainName = domainName;
            try
            {
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                {
                    this.mDomainName = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, domainName, this.mUserName, this.mPassword)).Name;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Retry domain failed.Error:{0}", e.ToString());
            }
            this.mUserName = userName;
            this.mPassword = password;
        }

        public ActiveDirectoryDomain(string domainName, string distinguishedName)
        {
            this.mDirectAccess = true;
            this.mDomainName = domainName;
            this.mDistinguishedName = string.Format("/{0}", distinguishedName.Replace("/", "\\/"));
        }

        public ActiveDirectoryDomain(string domainName, string distinguishedName, string userName, string password)
        {
            this.mDirectAccess = false;
            this.mDomainName = domainName;
            this.mUserName = userName;
            this.mPassword = password;
            this.mDistinguishedName = string.Format("/{0}", distinguishedName.Replace("/", "\\/"));
        }

        #endregion

        #region ==属性==

        public string RealDomainName
        {
            get
            {
                return this.mDomainName;
            }
        }

        public static string[] SearchObject_SupportedAttributes
        {
            get
            {
                if (searchObject_SupportedAttributes == null)
                {
                    HashSet<string> realSupportedAttributesSet = new HashSet<string>();
                    foreach (string key in SearchGroup_SupportedAttributes)
                    {
                        realSupportedAttributesSet.Add(key);
                    }

                    foreach (string key in SearchUser_SupportedAttributes)
                    {
                        realSupportedAttributesSet.Add(key);
                    }
                    searchObject_SupportedAttributes = realSupportedAttributesSet.ToArray();
                    logger.Debug("ActiveDirectory supports attributes: {0}", string.Join(",", searchObject_SupportedAttributes));
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
                    if (string.IsNullOrEmpty(this.netbiosName))
                    {
                        DirectoryEntry entry = new DirectoryEntry(string.Format("LDAP://{0}/{1}",this.mDomainName ,"RootDSE"), this.mUserName, this.mPassword);
                        string domainNCName = entry.Properties[ActiveDirectoryPropertyNames.NamingContext.DEFAULT_NAMING_CONTEXT][0].ToString();
                        string configName = entry.Properties[ActiveDirectoryPropertyNames.NamingContext.CONFIGURATION_NAMING_CONTEXT][0].ToString();
                        DirectoryEntry configEntry = new DirectoryEntry(string.Format("LDAP://{0}/{1}", this.mDomainName, configName), this.mUserName, this.mPassword);
                        configEntry.RefreshCache(new string[] { ActiveDirectoryPropertyNames.NamingContext.NETBIOSNAME, ActiveDirectoryPropertyNames.NamingContext.N_CNAME });
                        DirectorySearcher searcher = new DirectorySearcher(configEntry);
                        searcher.Filter = string.Format("(&(objectClass={0})({1}={2}))",
                                                            ObjectClasses.NamingContext.CROSS_REF,
                                                            ActiveDirectoryPropertyNames.NamingContext.N_CNAME,
                                                            domainNCName);
                        SearchResult sr = searcher.FindOne();
                        if (sr != null)
                        {
                            this.netbiosName = sr.Properties[ActiveDirectoryPropertyNames.NamingContext.NETBIOSNAME][0].ToString();
                        }
                        else 
                        {
                            this.netbiosName = string.Empty;
                            throw new Exception("Failed to get domain configuration information.");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Failed to get NetBiosName: {0}. Exception: {1}", this.mDomainName, e.Message);
                    this.netbiosName = null;
                }
                return this.netbiosName;
            }
        }

        #endregion

        #region ==方法==
     
        /// <summary>
        ///  判断Domain是否是外部信任域
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>bool</returns>
        public bool IsExternalTrusted(string domain)
        {
            string cachePair = string.Format("{0}->{1}", this.RealDomainName, domain);
            if (! (MCache.GetValue<bool?>(cachePair) == null)) 
            {
                return MCache.GetValue<bool?>(cachePair).Value;
            }

            logger.Debug("Check if {0} and {1} are external trusted domains.", this.RealDomainName, domain);
            try
            {
                Domain currentDomain = null;
                if (string.IsNullOrEmpty(this.mDomainName))
                {
                    currentDomain = Domain.GetCurrentDomain();
                }
                else
                {
                    if (string.IsNullOrEmpty(this.mUserName) || string.IsNullOrEmpty(this.mPassword))
                    {
                        currentDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, this.RealDomainName));
                    }
                    else
                    {
                        currentDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, this.RealDomainName, this.mUserName, this.mPassword));
                    }
                }
                TrustRelationshipInformation trustInfo = currentDomain.GetTrustRelationship(domain);
                if (trustInfo != null && trustInfo.TrustType == TrustType.External)
                {
                    logger.Debug("{0} and {1} are external trusted domains: {2}", this.RealDomainName, domain, true);
                    MCache.CreateItem<bool?>(cachePair, true);
                    return true;
                }
            }
            catch (Exception e) 
            {
                logger.Warn("Failed to check trust relationship between {0} and {1}. Exception: {2}", this.RealDomainName, domain, e.Message);
            }
            logger.Debug("{0} and {1} are external trusted domains: {2}", this.RealDomainName, domain, false);
            MCache.CreateItem<bool?>(cachePair, false);
            return false;
        }

        /// <summary>
        /// Forest trust
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public bool IsForestTrusted(string forest)
        {
            string cachePair = string.Format("{0}<->{1}", this.RealDomainName, forest);
            if (!(MCache.GetValue<bool?>(cachePair) == null))
            {
                return MCache.GetValue<bool?>(cachePair).Value;
            }

            logger.Debug("Check if {0} and {1} are forest trusted.", this.RealDomainName, forest);
            try
            {
                Forest currentForest = null;
                if (string.IsNullOrEmpty(this.mDomainName))
                {
                    currentForest = Forest.GetCurrentForest();
                }
                else
                {
                    if (string.IsNullOrEmpty(this.mUserName) || string.IsNullOrEmpty(this.mPassword))
                    {
                        currentForest = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, this.RealDomainName));
                    }
                    else
                    {
                        currentForest = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, this.RealDomainName, this.mUserName, this.mPassword));
                    }
                }
                TrustRelationshipInformation trustInfo = currentForest.GetTrustRelationship(forest);
                if (trustInfo != null && trustInfo.TrustType == TrustType.Forest)
                {
                    logger.Debug("{0} and {1} are forest trusted : {2}", this.RealDomainName, forest, true);
                    MCache.CreateItem<bool?>(cachePair, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to check trust relationship between {0} and {1}. Exception: {2}", this.RealDomainName, forest, e.Message);
            }
            logger.Debug("{0} and {1} are forest trusted: {2}", this.RealDomainName, forest, false);
            MCache.CreateItem<bool?>(cachePair, false);
            return false;
        }
        /// <summary>
        ///  判断Domain是否是内部信任域
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>bool</returns>
        public bool IsInternalTrusted(string domain)
        {
            string cachePair = string.Format("{0}=>{1}", this.RealDomainName, domain);
            if (!(MCache.GetValue<bool?>(cachePair) == null))
            {
                return MCache.GetValue<bool?>(cachePair).Value;
            }

            Domain currentDomain = null;
            try
            {
                if (string.IsNullOrEmpty(this.mDomainName))
                {
                    currentDomain = Domain.GetCurrentDomain();
                }
                else
                {
                    if (string.IsNullOrEmpty(this.mUserName) || string.IsNullOrEmpty(this.mPassword))
                    {
                        currentDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, this.RealDomainName));
                    }
                    else
                    {
                        currentDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, this.RealDomainName, this.mUserName, this.mPassword));
                    }
                }
                TrustRelationshipInformation trustInfo = currentDomain.GetTrustRelationship(domain);
                if (trustInfo != null && (trustInfo.TrustType == TrustType.ParentChild || trustInfo.TrustType == TrustType.TreeRoot))
                {
                    MCache.CreateItem<bool?>(cachePair, true);
                    return true;
                }
            }
            catch(Exception e)
            {
                logger.Warn("Failed to check trust relationship between {0} and {1}. Exception: {2}", this.RealDomainName, domain, e.Message);
            }
            MCache.CreateItem<bool?>(cachePair, false);
            return false;
        }

        /// <summary>
        ///  获取GC服务对象
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="directAccess">false:获取指定域GC ; true:获取本地所在域GC</param>
        /// <returns></returns>
        public ActiveDirectoryDomain TryOthers(string domainName, bool directAccess=false) 
        {
            ActiveDirectoryDomain other;
            try
            {              
                if (!directAccess)
                {
                    other = new ActiveDirectoryDomain(domainName, this.mUserName, this.mPassword).ConnectGlobalCatalog();
                    logger.Debug("Bound domain: {0}", domainName);
                }
                else
                {
                    other = new ActiveDirectoryDomain().ConnectGlobalCatalog();
                    logger.Debug("Bound current server domain.");
                }
            }
            catch (Exception e) 
            {
                logger.Warn("Cannot bind server domain: {0} automatically, please check if the Domain Control & DNS setting is set correctly. Exception: {1}", domainName, e.Message);
                return null;
            }
            return other;
        }

        /// <summary>
        ///  通过distinguishedName获取GC服务对象
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="directAccess">false:获取指定域GC ; true:获取本地所在域GC</param>
        /// <returns></returns>
        public ActiveDirectoryDomain TryOthers(string domainName, string distinguishedName, bool directAccess=false)
        {
            ActiveDirectoryDomain other;
            try
            {
                if (!directAccess)
                {
                    other = new ActiveDirectoryDomain(domainName, distinguishedName, this.mUserName, this.mPassword).ConnectGlobalCatalog();
                    logger.Debug("Bound domain: {0}/{1}", domainName, distinguishedName);
                }
                else
                {
                    other = new ActiveDirectoryDomain(domainName, distinguishedName).ConnectGlobalCatalog();
                    logger.Debug("Bound domain: {0}/{1}", domainName, distinguishedName);
                }
            }
            catch (Exception e) 
            {
                logger.Warn("Cannot bind server domain: {0}/{1} automatically, please check if the Domain Control & DNS setting is set correctly. Exception: {2}", domainName,distinguishedName, e.Message);
                return null;
            }
            return other;
        }

        /// <summary>
        /// Create Directory Entry via Global Category protocol, usually be used to search
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryDomain ConnectGlobalCatalog() 
        {
            if (this.Entry == null)
            {
                try
                {
                    if (!this.mDirectAccess)
                    {
                        this.Entry = new ActiveDirectoryEntry(string.Format("GC://{0}{1}", this.mDomainName, this.mDistinguishedName), this.mUserName, this.mPassword);
                    }
                    else
                    {
                        this.Entry = new ActiveDirectoryEntry(string.Format("GC://{0}{1}", this.mDomainName, this.mDistinguishedName));
                    }
                    string name = this.Entry.Name;
                    this.gc_connected = true;
                    logger.Debug("Successfully Bound Global Catalog. Domain: {0}", this.mDomainName);
                }
                catch(Exception e)
                {
                    this.gc_connected = false;
                    logger.Warn("Cannot bind server domain via Global Catalog: {0} , please check if the Domain Control & DNS setting is set correctly. Exception: {1}", this.mDomainName, e.Message);
                }
            }
            return this;
        }

        /// <summary>
        /// Create Directory Entry via Lightweight Data Access Protocol, usually be used to retreive and modify  data
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryDomain ConnectLDAP() 
        {
            if (this.EntryForExtend == null)
            {
                if (!this.mDirectAccess)
                {
                    this.EntryForExtend = new ActiveDirectoryEntry(string.Format("LDAP://{0}{1}", this.mDomainName, this.mDistinguishedName), this.mUserName, this.mPassword);
                }
                else 
                {
                    this.EntryForExtend = new ActiveDirectoryEntry(string.Format("LDAP://{0}{1}", this.mDomainName, this.mDistinguishedName));
                } 
                try
                {
                    string name = this.EntryForExtend.Name;
                    this.ldap_connected = true;
                    logger.Debug("Successfully Bound LDAP. Domain: {0}", this.mDomainName);
                }
                catch(Exception e)
                {
                    this.ldap_connected = false;
                    logger.Warn("Cannot bind server domain via LDAP: {0} , please check if the Domain Control & DNS setting is set correctly. Exception: {1}", this.mDomainName, e.Message);
                }
            }
            return this;
        }

        /// <summary>
        /// Create a default searcher to search data in Active Directory
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="needToLoad"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "distinguishedname")]
        public ActiveDirectorySearcher CreateDefaultSearcher(string filter, params string[] needToLoad)
        {
            ActiveDirectorySearcher searcher = new ActiveDirectorySearcher(this).SetPageSize(120)
                                                                                            .SetPageSizeLimit(100)
                                                                                            .SetScope(SearchScope.Subtree)
                                                                                            .ToLoad(needToLoad)
                                                                                            .LoadMore(ActiveDirectoryPropertyNames.DISTINGUISHED_NAME)
                                                                                            .SetFilter(filter);
            return searcher;
            
        }

        /// <summary>
        /// Create a default Global Category Searcher
        /// </summary>
        /// <returns></returns>
        public ActiveDirectorySearcher CreateDefaultSearcher() 
        {
            ActiveDirectorySearcher searcher = new ActiveDirectorySearcher(this).SetPageSize(120)
                                                                                               .SetPageSizeLimit(100)
                                                                                               .SetScope(SearchScope.Subtree);
            return searcher;
        }

        /// <summary>
        /// Create a default LDAP Searcher
        /// </summary>
        /// <returns></returns>
        public ActiveDirectorySearcher CreateLDAPSearcher()
        {
            ActiveDirectorySearcher searcher = new ActiveDirectorySearcher(this, true).SetPageSize(120)
                                                                                               .SetPageSizeLimit(100)
                                                                                               .SetScope(SearchScope.Subtree);
            return searcher;
        }

        /// <summary>
        /// Create an ActiveDirectoryEntry from SearchResult, and then the ActiveDirectoryEntry can be cast to ActiveDirectoryObject
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "distinguishedname")]
        public ActiveDirectoryEntry CreateEntry(SearchResult searchResult, string protocol = "GC://") 
        {
            string dn = searchResult.Properties[ActiveDirectoryPropertyNames.DISTINGUISHED_NAME][0].ToString();
            logger.Debug("Creating Entry for : {0}", dn);
            
            ActiveDirectoryEntry entry =null;
            try
            {
                if (!this.mDirectAccess)
                {
                    entry = new ActiveDirectoryEntry();
                    entry.Path = string.Format("{2}{0}/{1}", this.mDomainName, dn.Replace("/","\\/"), protocol);
                    entry.Password = this.mPassword;
                    entry.Username = this.mUserName;
                    entry.Checker = this;
                }
                else
                {
                    entry = new ActiveDirectoryEntry(string.Format("{2}{0}/{1}", this.mDomainName, dn.Replace("/", "\\/"), protocol));
                    entry.Checker = this;
                }
                logger.Debug("Successfully to create Entry for: {0}", dn);
            }
            catch (Exception e) 
            {
                logger.Warn("Failed to create Entry for :{0}. Exception: {1}", dn, e.Message);
            }
            return entry;
        }

        /// <summary>
        /// Direct create an ActiveDirectoryEntry from Distinguished Name.
        /// </summary>
        /// <param name="distinguishedName">For example: CreateEntry("CN=someone, OU=SomeOrg, DC=domain, DC=com");</param>
        /// <returns></returns>
        public ActiveDirectoryEntry CreateEntry(string distinguishedName, string protocol = "GC://")
        {
            logger.Debug("Creating Entry for : {0}", distinguishedName);
            ActiveDirectoryEntry entry =null;
            try
            {
                if (!this.mDirectAccess)
                {
                    entry = new ActiveDirectoryEntry();
                    entry.Path = string.Format("{2}{0}/{1}", this.mDomainName, distinguishedName.Replace("/","\\/"), protocol);
                    entry.Password = this.mPassword;
                    entry.Username = this.mUserName;
                    entry.Checker = this;
                }
                else
                {
                    entry = new ActiveDirectoryEntry(string.Format("{2}{0}/{1}", this.mDomainName, distinguishedName.Replace("/", "\\/"), protocol));
                    entry.Checker = this;
                }
                logger.Debug("Successfully to create Entry for: {0}", distinguishedName);
            }
            catch (Exception e) 
            {
                logger.Warn("Failed to create Entry for :{0}. Exception: {1}", distinguishedName, e.Message);
            }
            return entry;
        }

        public ActiveDirectoryEntry CreateEntryBySid(string sid, string protocol = "LDAP://")
        {
            logger.Debug("Creating Entry for : {0}", sid);
            ActiveDirectoryEntry entry = null;
            try
            {
                if (!this.mDirectAccess)
                {
                    entry = new ActiveDirectoryEntry();
                    entry.Path = string.Format("{1}{2}/<SID={0}>", sid, protocol, mDomainName);
                    entry.Password = this.mPassword;
                    entry.Username = this.mUserName;
                    entry.Checker = this;
                }
                else
                {
                    entry = new ActiveDirectoryEntry(string.Format("{1}<SID={0}>", sid, protocol));
                    entry.Checker = this;
                }
                logger.Debug("Successfully to create Entry for: {0}", sid);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to create Entry for :{0}. Exception: {1}", sid, e.Message);
            }
            return entry;
        }

        /// <summary>
        /// Create an ActiveDirectoryObject from a SearchResult
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        public ActiveDirectoryObject CreateObject(SearchResult searchResult, string protocol = "GC://")
        {
            return this.CreateEntry(searchResult,protocol)
                .ToActiveDirectoryObject();
        }

        /// <summary>
        /// Direct create an ActiveDirectoryObject from Distinguished Name.
        /// </summary>
        /// <param name="distinguishedName">For example: CreateObject("CN=someone, OU=SomeOrg, DC=domain, DC=com");</param>
        /// <returns></returns>
        public ActiveDirectoryObject CreateObject(string distinguishedName) 
        {
            return this.CreateEntry(distinguishedName)
                .ToActiveDirectoryObject();
        }

        public ActiveDirectoryObject CreateObjectBySid(string sid) 
        {
            return this.CreateEntryBySid(sid).ToActiveDirectoryObject();
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
            logger.Debug("Compute full domain name of {0}", distinguishedname);
            return distinguishedname.Substring(distinguishedname.IndexOf("DC=", StringComparison.OrdinalIgnoreCase)).Replace("DC=", "").Replace(",", ".");
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.EntryForExtend != null)
            {
                this.EntryForExtend.Dispose();
            }

            if (this.Entry != null)
            {
                this.Entry.Dispose();
            }
        } 
      
        #endregion
    }    
}
