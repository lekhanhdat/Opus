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
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Common.ActiveDirectoryWrapper;

namespace AvePoint.Wrapper.Common
{
    public static class AveBrowserHelper
    {

        private static AveLogger logger = AveLogger.GetInstance(typeof(AveBrowserHelper));
        public static bool IsForceNativeModel(string pageInfo)
        {
            return string.IsNullOrEmpty(pageInfo) ? false : pageInfo.IndexOf("RootFolder", StringComparison.OrdinalIgnoreCase) < 0;
        }

        public static int GetPagedCount(string pageInfo)
        {
            int pagedCount = 0;
            if (string.IsNullOrEmpty(pageInfo))
            {
                return pagedCount;
            }
            else
            {
                pagedCount = Convert.ToInt32(pageInfo.Substring(pageInfo.LastIndexOf("=", StringComparison.OrdinalIgnoreCase) + 1));
            }
            return pagedCount;
        }


        #region Security Trimming

        private static string RemoveGroupPrefix(string groupNameOrSid)
        {
            int index = groupNameOrSid.IndexOf('|');
            if (index >= 0)
            {
                groupNameOrSid = groupNameOrSid.Substring(index + 1);
            }
            return groupNameOrSid;
        }
        /// <summary>
        /// 获取与当前域所关联的所有Bidirectional域。
        /// </summary>
        /// <param name="webapp">web application</param>
        /// <returns>AD domain collection</returns>
        public static Dictionary<string, ActiveDirectoryDomain> GetBidirectionalDirectoryDomains()
        {
            var directoryDomains = new Dictionary<string, ActiveDirectoryDomain>(StringComparer.OrdinalIgnoreCase);
            using (var currentDomain = Domain.GetComputerDomain())
            {
                //Add current domain to dictionary first.
                directoryDomains.Add(currentDomain.Name, new ActiveDirectoryDomain(currentDomain.Name));
                try
                {
                    TrustRelationshipInformationCollection[] trustCollections = new TrustRelationshipInformationCollection[2];
                    //获取当前域的所有双向信任域。
                    trustCollections[0] = currentDomain.GetAllTrustRelationships();
                    //获取当前域的Forest节点的所有双向信任域。
                    trustCollections[1] = currentDomain.Forest.GetAllTrustRelationships();
                    foreach (TrustRelationshipInformationCollection collection in trustCollections)
                    {
                        if (collection != null)
                        {
                            foreach (TrustRelationshipInformation trustInformation in collection)
                            {
                                if (trustInformation.TrustDirection != TrustDirection.Bidirectional)
                                {
                                    continue;
                                }
                                if (trustInformation.TrustType != TrustType.Unknown)
                                {
                                    //directoryDomains.Add(trustInformation.TargetName, new ActiveDirectoryDomain(trustInformation.TargetName));
                                    directoryDomains[trustInformation.TargetName] = new ActiveDirectoryDomain(trustInformation.TargetName);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while getting bidirectional domains. CurrentDomanName: {0}. Error: {1}", currentDomain.Name, e.ToString());
                }
            }
            return directoryDomains;
        }

        /// <summary>
        /// 获取当前Web application所关联的所有Outbound域。
        /// </summary>
        /// <param name="webapp">Web application</param>
        /// <returns>AD domain collection</returns>
        public static Dictionary<string, ActiveDirectoryDomain> GetOutboundDirectoryDomains(IAveWebApplication webapp)
        {
            var directoryDomains = new Dictionary<string, ActiveDirectoryDomain>(StringComparer.OrdinalIgnoreCase);
            foreach (IAvePeoplePickerSearchActiveDirectoryDomain domain in webapp.PeoplePickerSettings.SearchActiveDirectoryDomains)
            {
                try
                {
                    string username = domain.LoginName == null ? string.Empty : domain.LoginName;
                    string password = domain.Password == null ? null : new string(CryptoUtil.ConvertSecureStringToChars(domain.Password));
                    string domainName = GetRealDomainName(domain.DomainName);
                    logger.Debug("Get one out bound directory domain. Username: {0}, DomainName: {1}, PWIsNullOrEmpty: {2}, RealDomainName: {3}", 
                        string.IsNullOrEmpty(username) ? string.Empty : username,
                        string.IsNullOrEmpty(domain.DomainName) ? string.Empty : domain.DomainName,
                        string.IsNullOrEmpty(password) ? true : false,
                        domainName);
                    if (directoryDomains.ContainsKey(domainName))
                    {
                        directoryDomains[domainName].Dispose();
                    }
                    directoryDomains[domainName] = new ActiveDirectoryDomain(domainName, username, password);
                }
                catch (Exception e)
                {
                    logger.Error("Failure to get one way trust domain information failed. Web application: {0}  Error: {1}", webapp.Name, e.ToString());
                }
            }
            return directoryDomains;
        }

        /// <summary>
        /// Claim认证，AD Group在SP中是以SID形式存在的。用此方法获取Group的ActiveDirectoryObject.
        /// </summary>
        /// <param name="sid">Group Sid</param>
        /// <param name="domains">outbound domains where used to search in.</param>
        /// <param name="domains">bidirectional domains where used to search in.</param>
        /// <returns></returns>
        public static ActiveDirectoryObject GetADGroupObjectBySID(string sid, Dictionary<string, ActiveDirectoryDomain> outboundDomains, Dictionary<string, ActiveDirectoryDomain> bidirectionalDomains,out bool isOutboundObject)
        {
            isOutboundObject = true;
            sid = RemoveGroupPrefix(sid);
            try
            {
                foreach (var domain in bidirectionalDomains)
                {
                    var activeDirectoryObject = domain.Value.CreateDefaultSearcher().LoadByObjectSid(sid);
                    if (activeDirectoryObject != null)
                    {
                        isOutboundObject = false;
                        return activeDirectoryObject;
                    }
                }
                foreach (var domain in outboundDomains)
                {
                    var activeDirectoryObject = domain.Value.CreateDefaultSearcher().LoadByObjectSid(sid);
                    if (activeDirectoryObject != null)
                    {
                        return activeDirectoryObject;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Failure to get user by sid. SID: {0}  Error: {1}", sid, e.ToString());
            }
            return null;
        }

        /// <summary>
        /// 通过login name，获取ActiveDirectoryObject
        /// </summary>
        /// <param name="loginName">longin name，格式必须是 [domain]\[username] </param>
        /// <param name="outboundDomains">outbound domains where used to search in</param>
        /// <param name="bidirectionalDomains">bidirectional domains where used to search in</param>
        /// <returns>ad object</returns>
        public static ActiveDirectoryObject GetADObjectByLoginName(string loginName, Dictionary<string, ActiveDirectoryDomain> outboundDomains, Dictionary<string, ActiveDirectoryDomain> bidirectionalDomains, out bool outboundObject)
        {
            outboundObject = true;
            ActiveDirectoryObject searchResult = null;
            int index = loginName.IndexOf('\\');
            if (index > 0)
            {
                string domain = loginName.Substring(0, index);
                string userName = loginName.Substring(index + 1);
                try
                {
                    if (bidirectionalDomains.ContainsKey(domain))
                    {
                        searchResult = bidirectionalDomains[domain].CreateDefaultSearcher().SingleSearch(userName);
                        outboundObject = false;
                    }
                    else if (outboundDomains.ContainsKey(domain))
                    {
                        searchResult = outboundDomains[domain].CreateDefaultSearcher().SingleSearch(userName);
                    }
                    else
                    {
                        logger.Error("Do not have bidirectional or Outbound relationship between {0} and {1}.", GetCurrentDomainName(), domain);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Failure to get user. User: {0}  Error: {1}", loginName, e.ToString());
                }
            }
            else
            {
                //不是domain\username 格式的
                logger.Warn("Wrong Format: {0}", loginName);
            }
            if (searchResult == null)
            {
                logger.Warn("Can not found this user: {0}", loginName);
            }
            return searchResult;
        }

        public static string GetCurrentDomainName()
        {
            using (Domain curretnDomain = Domain.GetComputerDomain())
            {
                return curretnDomain.Name;
            }
        }

        /// <summary>
        /// 获取Domain的Real name
        /// </summary>
        /// <param name="domainName">The domain name that you want to check.</param>
        /// <returns>Real name</returns>
        public static string GetRealDomainName(string domainName)
        {
            try
            {
                int index = domainName.IndexOf('.');
                if (index > 0)
                {
                    domainName = domainName.Substring(0, index);
                }
                DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, domainName);

                using (Domain domain = Domain.GetDomain(context))
                {
                    domainName = domain.Name;
                }
            }
            catch (Exception e)
            {
                logger.Debug("This domain do not exist in current domain. Domain: {0}, CurrentDomain: {1}. Error: {2}", domainName, GetCurrentDomainName(), e);
                try
                {
                    using (Domain domain = Domain.GetComputerDomain())
                    {
                        TrustRelationshipInformation trustInformation = domain.GetTrustRelationship(domainName);
                        domainName = trustInformation.TargetName;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Can not find any relationship between current domain and '{0}'. Error :{1}", domainName, ex.ToString());
                }
            }
            return domainName;
        }

        /// <summary>
        /// 获取ad use的ad name.
        /// </summary>
        /// <param name="loginName">SP中存储的login name. Format must be: [domain]\[username]。目前还不支持SID</param>
        /// <param name="domainMapping">获取Domain name会很耗时，建议使用此Cache</param>
        /// <returns>AD Name</returns>
        public static string GetUserRealLoginName(string loginName,Dictionary<string,string> domainMapping)
        {
            if (domainMapping == null)
            {
                throw new ArgumentNullException("domainMapping");
            }
            int index = loginName.IndexOf('\\');
            if (index > 0)
            {
                string domain = RemoveGroupPrefix(loginName.Substring(0, index));
                string name = loginName.Substring(index + 1);
                string newDomain = string.Empty;
                if (domainMapping.TryGetValue(domain, out newDomain))
                {
                    loginName = newDomain + "\\" + name;
                }
                else
                {
                    newDomain = GetRealDomainName(domain);
                    domainMapping[domain] = newDomain;
                    loginName = newDomain + "\\" + name; ;
                }
            }
            return loginName;
        }

        public static void DisposeADCache<T>(Dictionary<string, T> caches) where T : IDisposable
        {
            if (caches != null)
            {
                foreach (var cache in caches)
                {
                    if (cache.Value != null)
                    {
                        try
                        {
                            cache.Value.Dispose();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while dispose this AD Object: {0}. Error: {1}", cache.Key, e);
                        }
                    }
                }
                caches.Clear();
            }
        }

        #endregion
    }
}
