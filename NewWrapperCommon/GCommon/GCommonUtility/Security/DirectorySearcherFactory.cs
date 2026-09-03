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
    using System.Collections;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.DirectoryServices.ActiveDirectory;
    using System.Text;
    using AvePoint.GCommon.Utility.I18N;
    #endregion

    public static class DirectorySearcherFactory
    {
        static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static bool InitComplete;
        static Object initLock = new Object();
        public static List<DomainInfo> OneWayTrustDoamins = new List<DomainInfo>();

        /// <summary>
        /// key is domain name,value is domain info
        /// </summary>
        private static Dictionary<string, DomainInfo> globalDomainNameMapping = new Dictionary<string, DomainInfo>();
        public static Dictionary<string, DomainInfo> GlobalDomainNameMapping
        {
            get { return globalDomainNameMapping; }
        }

        public static List<string> ForestCache = new List<string>();
        static Object forestCacheLock = new object();

        private static Dictionary<string, DirectorySearcher> globalSearcherCollection;
        public static Dictionary<string, DirectorySearcher> GlobalSearcherCollection
        {
            get
            {
                lock (initLock)
                {
                    if (globalSearcherCollection == null)
                    {
                        globalSearcherCollection = new Dictionary<string, DirectorySearcher>();
                    }
                    if (OneWayTrustDoamins.Count > 0)
                    {
                        foreach (DomainInfo info in OneWayTrustDoamins)
                        {
                            if (info.IsForest)
                            {
                                if (!ForestCache.Contains(info.DomainName))
                                {
                                    AddAllDomainSearcherFromTrustForest(info);
                                    lock (forestCacheLock)
                                    {
                                        ForestCache.Add(info.DomainName);
                                    }
                                }
                            }
                            else
                            {
                                if (!globalDomainNameMapping.ContainsKey(info.DomainName))
                                {
                                    globalDomainNameMapping.Add(info.DomainName, info);
                                }
                                AddOneWayTrustSearch(info.DomainName, info.LoginName, info.Password);
                            }

                        }
                    }
                    if (!InitComplete)
                    {
                        Init();
                    }
                }
                return globalSearcherCollection;
            }
            set
            {
                globalSearcherCollection = value;
            }
        }

        public static List<string> BadDomainCahce = new List<string>();
        static Object badDomainLock = new object();

        public static void ClearDomainSearcherCache()
        {
            lock (initLock)
            {
                globalSearcherCollection = null;
                InitComplete = false;
                ForestCache = new List<string>();
                BadDomainCahce = new List<string>();
            }
        }

        /// <summary>
        /// 初始化DirectorySearcherFactory
        /// </summary>

        public static void Init()
        {
            try
            {
                Domain currentDomain = Domain.GetComputerDomain();
                AddGCSearchersCore(currentDomain.Forest);
                AddGCSearchersCore(currentDomain.GetAllTrustRelationships());//add cross-forest gc server
                AddGCSearchersCore(currentDomain.Forest.GetAllTrustRelationships());//add cross-forest gc server
                InitComplete = true;
            }
            catch (Exception ee)
            {
                logger.Warn(ee.ToString());
            }
        }

        /// <summary>
        /// one-way trust need username and pwd
        /// </summary>
        /// <param name="domainName">domain or netbios name, uniqe</param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        public static void AddOneWayTrustSearch(string domainName, string userName, string pwd)
        {
            if (!globalSearcherCollection.ContainsKey(domainName))
            {
                DirectorySearcher ds = GetLDAPSearchersByDomainName(domainName, userName, pwd);
                if (ds != null)
                {
                    globalSearcherCollection.Add(domainName, ds);
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
                            if (!globalSearcherCollection.ContainsKey(netbiosName))
                            {
                                globalSearcherCollection.Add(netbiosName, ds);
                            }
                        }
                        else
                        {
                            logger.Error("Domain name: {0}, Can not find any user.", domainName);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Init {0}'s netbiosName occurred an error: {1}", domainName, e.ToString());
                    }
                }
            }
        }

        private static void AddGCSearchersCore(TrustRelationshipInformationCollection trusts)
        {
            foreach (TrustRelationshipInformation trust in trusts)
            {
                if (trust.TrustDirection != TrustDirection.Bidirectional)
                {
                    continue;
                }
                if (trust.TrustType == TrustType.External || trust.TrustType == TrustType.Forest)
                {
                    try
                    {
                        Domain trustDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, trust.TargetName));
                        AddGCSearchersCore(trustDomain.Forest);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("AddTrustDomainSearcher : TargetName:{0}; error:{1} ", trust.TargetName, ex.ToString());
                    }
                }
            }
        }


        private static void AddAllDomainSearcherFromTrustForest(DomainInfo domainInfo)
        {
            try
            {
                Domain trrustForest = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, domainInfo.DomainName, domainInfo.LoginName, domainInfo.Password));
                foreach (Domain domain in trrustForest.Forest.Domains)
                {
                    var dcSearcher = GetLDAPSearchersByDomainName(domain.Name, domainInfo.LoginName, domainInfo.Password);
                    if (dcSearcher != null)
                    {
                        try
                        {
                            if (!globalDomainNameMapping.ContainsKey(domain.Name))
                            {
                                globalDomainNameMapping.Add(domain.Name, domainInfo);
                            }
                            if (!globalSearcherCollection.ContainsKey(domain.Name))
                            {
                                globalSearcherCollection.Add(domain.Name, dcSearcher);
                            }
                            dcSearcher.SearchScope = SearchScope.Subtree;
                            dcSearcher.PropertiesToLoad.Add("msDS-PrincipalName");
                            dcSearcher.Filter = "(&(|(objectCategory=Person)(objectCategory=Computer)))";
                            SearchResult result = dcSearcher.FindOne();
                            if (result != null)
                            {
                                string principalName = result.Properties["msDS-PrincipalName"][0].ToString();
                                string netbiosName = principalName.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
                                if (!globalSearcherCollection.ContainsKey(netbiosName))
                                {
                                    globalSearcherCollection.Add(netbiosName, dcSearcher);
                                }
                            }
                            else
                            {
                                logger.Error("Domain name: {0}, Can not find any user.", domain.Name);
                            }
                            if (!globalSearcherCollection.ContainsKey(domain.Name))
                            {
                                globalSearcherCollection.Add(domain.Name, dcSearcher);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("Init {0}'s netbiosName occurred an error: {1}", domain.Name, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Add all domains from trust forest {0} occurred an error.{1}", domainInfo.DomainName, e.ToString());
            }

        }
        private static void AddGCSearchersCore(Forest forest)
        {
            if (!globalSearcherCollection.ContainsKey(forest.Name))
            {
                try
                {
                    logger.Info("Add GC server to dictionary, forest name is :" + forest.Name);
                    DirectoryEntry de = new DirectoryEntry("GC://" + forest.Name);
                    DirectorySearcher ds = new DirectorySearcher();
                    ds.SearchRoot = de;
                    globalSearcherCollection.Add(forest.Name, ds);
                }
                catch (Exception ee)
                {
                    logger.Warn("Init GC failed, reason:{0}", ee.ToString());
                }
            }
        }

        /// <summary>
        /// note: 注意这个domain那么可能为单向信任域而连接不了域控，需要遍历
        /// globalSearcherCollection获取
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public static DirectorySearcher GetLDAPSearchersByDomainName(string domainName)
        {
            return GetAllLDAPSearchersByDomainName(domainName);
        }

        public static DirectorySearcher GetLDAPSearchersByDomainName(string domainName, string userName, string pwd)
        {
            DomainController dc = null;
            if (TestDomainController(domainName, userName, pwd, ref dc))
            {
                return dc.GetDirectorySearcher();
            }
            return null;
        }

        public static DirectorySearcher GetAllLDAPSearchersByDomainName(string domainName)
        {
            try
            {
                if (!BadDomainCahce.Contains(domainName))
                {
                    string ldap = "LDAP://" + domainName;
                    DirectoryEntry de = new DirectoryEntry(ldap);
                    DirectorySearcher ldapSearcher = new DirectorySearcher(de);
                    SearchResult result = ldapSearcher.FindOne();
                    if (result != null)
                    {
                        logger.Info("Get searcher by LDAP succeed");
                        return ldapSearcher;
                    }
                    else
                    {
                        lock (badDomainLock)
                        {
                            if (!BadDomainCahce.Contains(domainName))
                            {
                                BadDomainCahce.Add(domainName);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Info("Get searcher by LDAP failed. Reason:" + e.ToString());
                lock (badDomainLock)
                {
                    if (!BadDomainCahce.Contains(domainName))
                    {
                        BadDomainCahce.Add(domainName);
                    }
                }
            }
            return GetDirectorySearcherFromGloabl(domainName);
        }

        private static DirectorySearcher GetDirectorySearcherFromGloabl(string domainName)
        {
            foreach (KeyValuePair<string, DirectorySearcher> pair in DirectorySearcherFactory.GlobalSearcherCollection)
            {
                bool isThisSearcher = false;
                string dnsOrForest = pair.Key;
                logger.Debug(dnsOrForest);
                string[] domainSuffixs = dnsOrForest.ToLowerInvariant().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (domainSuffixs.Length > 0)
                {
                    if (domainSuffixs[0].Equals(domainName, StringComparison.OrdinalIgnoreCase))
                    {
                        isThisSearcher = true;
                    }
                }
                if (isThisSearcher)
                {
                    logger.Info("Get DC from GlobalCollection success, domain name is: " + dnsOrForest);
                    return pair.Value;
                }
                logger.Info("Get DC from GlobalCollection failed.");
            }
            return null;
        }

        /// <summary>
        /// 该方法主要用于单向信任域环境，通过domain name将不能连接域控，需要提供username和pwd，这两个参数
        /// 可以通过其他途径获得
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public static bool TestDomainController(string domainName, ref DomainController dc)
        {
            return TestDomainController(domainName, null, null, ref dc);
        }
        public static bool TestDomainController(string domainName, string userName, string pwd, ref DomainController dc)
        {
            try
            {
                DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, domainName, userName, pwd);
                dc = DomainController.FindOne(context);
                logger.Info("Domain:" + domainName + " test successfully, will get searcher directly by domain name");
            }
            catch (Exception ee)
            {
                logger.Warn("Test connect Domain failed, reason is: " + ee.ToString());
            }
            return dc != null;
        }

    }

    /// <summary>
    /// this class used for one way trust, now we only support domainName, user, pwd
    /// </summary>
    public class DomainInfo
    {
        public string DomainName { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public bool IsForest { get; set; }
        public DomainInfo(string domainName, string loginName, string pwd)
        {
            this.DomainName = domainName;
            this.LoginName = loginName;
            this.Password = pwd;
        }
        public DomainInfo(string domainName, string loginName, string pwd, bool isForest)
        {
            this.DomainName = domainName;
            this.LoginName = loginName;
            this.Password = pwd;
            this.IsForest = isForest;
        }

        public override string ToString()
        {
            return string.Format("domainName:{0}, loginName:{1}", this.DomainName, this.LoginName);
        }
    }
}
