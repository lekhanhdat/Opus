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
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Common.ActiveDirectoryWrapper;
using Microsoft.IdentityModel.Claims;
using System.DirectoryServices.AccountManagement;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    class AveBrowserQuery : IAveBrowserQuery
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveBrowserQuery));

        private string mConnectString;

        private string mSiteUrl;

        private static Dictionary<string, object> mWebTemplates = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public AveBrowserQuery(string siteUrl, string connectString)
        {
            mConnectString = connectString;
            mSiteUrl = siteUrl;
        }

        public void Dispose()
        {
            SqlConnection.ClearAllPools();
        }

        [SPDisposeCheck.SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._140, "Ignoring this error")]
        public List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserSites"))
            {

                List<AveSiteBrowserInfo> sites = new List<AveSiteBrowserInfo>();
                SPWebTemplateCollection webTemplates = null;
                //List<AveSiteDto> siteDtos = new List<AveSiteDto>();
                Dictionary<Guid, Guid> siteIdInConfig = null;
                Dictionary<Guid, string> sitePathInConfig = null;
                Dictionary<Guid, string> contentDBNameInConfig = null;
                AveSqlConnection sqlConn = null;

                #region Security Trimming Cache.
                //Dictionary<string, ActiveDirectoryObject> usersCache = new Dictionary<string, ActiveDirectoryObject>(StringComparer.OrdinalIgnoreCase);
                //Dictionary<string, string> domainMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                //Dictionary<string, ActiveDirectoryDomain> bidirectionalDirectoryDomains = (usernames != null && usernames.Count > 0) ?
                //    AveBrowserHelper.GetBidirectionalDirectoryDomains() : new Dictionary<string, ActiveDirectoryDomain>();
                //Dictionary<string, ActiveDirectoryDomain> outboundDirectoryDomains = (usernames != null && usernames.Count > 0) ?
                //    AveBrowserHelper.GetOutboundDirectoryDomains(webApp) : new Dictionary<string, ActiveDirectoryDomain>();
                //Dictionary<string, Dictionary<string, bool>> userGroupMatchCache = new Dictionary<string, Dictionary<string, bool>>();
                #endregion

                string webAppUrl = string.Empty;

                try
                {
                    webAppUrl = webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString();
                    if (!webAppUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        webAppUrl += "/";
                    }

                    #region Get siteCollectionId and ContentDBId

                    AveConfigurationDatabase configDb = (AveConfigurationDatabase)Invoker.GetProperty(webApp, "ConfigurationDatabase");
                    if (configDb != null)
                    {
                        using (AveSqlConnection sqlConn_ConfigDB = new AveSqlConnection())
                        {
                            sqlConn_ConfigDB.Open(configDb.DatabaseConnectionString);
                            siteIdInConfig = new Dictionary<Guid, Guid>();
                            sitePathInConfig = new Dictionary<Guid, string>();
                            contentDBNameInConfig = new Dictionary<Guid, string>();
                            sqlConn_ConfigDB.Command.Parameters.AddWithValue("@ApplicationId", webApp.ID);
                            string cmdText_ConfigDB = "SELECT s.Id,s.DatabaseId,s.Path,o.Name FROM SiteMap s With(NoLock) INNER JOIN Objects o With(NoLock) ON s.DatabaseId=o.Id  WHERE ApplicationId=@ApplicationId";
                            if (AveSPUtility.IsSP1DBSchema(sqlConn_ConfigDB, "DeleteTransactionId", "SiteMap"))
                            {
                                cmdText_ConfigDB = "SELECT s.Id,s.DatabaseId,s.Path,o.Name FROM SiteMap s With(NoLock) INNER JOIN Objects o With(NoLock) ON s.DatabaseId=o.Id  WHERE ApplicationId=@ApplicationId AND DeleteTransactionId=0x";
                            }
                            using (SqlDataReader sqlReader_ConfigDB = sqlConn_ConfigDB.ExecuteReader(cmdText_ConfigDB))
                            {
                                while (sqlReader_ConfigDB.Read())
                                {
                                    var siteId = sqlReader_ConfigDB.GetGuid(0);
                                    if (sitePathInConfig.ContainsKey(siteId) || siteIdInConfig.ContainsKey(siteId) || contentDBNameInConfig.ContainsKey(siteId))
                                    {
                                        logger.Warn(string.Format("There is same site id in SiteMap. The site id is {0}.", siteId.ToString()));
                                    }
                                    else
                                    {
                                        siteIdInConfig[siteId] = sqlReader_ConfigDB.GetGuid(1);
                                        sitePathInConfig[siteId] = sqlReader_ConfigDB.GetString(2);
                                        contentDBNameInConfig[siteId] = sqlReader_ConfigDB.GetString(3);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    sqlConn = new AveSqlConnection();
                    string head = webAppUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? "http://" : "https://";
                    foreach (var contentDatabase in webApp.ContentDatabases)
                    {
                        try
                        {
                            if (contentDatabase == null)
                            {
                                logger.Warn("Get null content database when browser sites, skip this one.");
                                continue;
                            }
                            sqlConn.Open(contentDatabase.DatabaseConnectionString);

                            List<AveSiteBrowserInfo> allSitesUnderContentDatabase = new List<AveSiteBrowserInfo>();

                            #region Get All Site Collections under this content database

                            string cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId
                                                   FROM Webs w With(NoLock) INNER JOIN Sites s With(NoLock) ON w.SiteId = s.Id WHERE w.ParentWebId is null ";
                            if (needFilterInfo)
                            {
                                cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId,s.LastContentChange,w.TimeCreated,w.MetaInfo,u.tp_Login,u.tp_Title,w.Id,s.DiskUsed 
                                                FROM Webs w With(NoLock) INNER JOIN Sites s With(NoLock) ON w.SiteId = s.Id LEFT JOIN UserInfo u With(NoLock) on u.tp_SiteID=s.Id and u.tp_ID=s.OwnerID WHERE w.ParentWebId is null ";
                            }
                            if (AveSPUtility.IsSP1DBSchema(sqlConn))
                            {
                                cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId
                                                FROM AllWebs w With(NoLock) INNER JOIN AllSites s With(NoLock) ON w.SiteId = s.Id WHERE s.Deleted = CONVERT(bit, 0) And w.DeleteTransactionId = 0x And w.ParentWebId is null ";
                                if (needFilterInfo)
                                {
                                    cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId,s.LastContentChange,w.TimeCreated,w.MetaInfo,u.tp_Login,u.tp_Title ,w.Id,s.DiskUsed 
                                                FROM AllWebs w With(NoLock) INNER JOIN AllSites s With(NoLock) ON w.SiteId = s.Id LEFT JOIN UserInfo u With(NoLock) on u.tp_SiteID=s.Id and u.tp_ID=s.OwnerID WHERE s.Deleted = CONVERT(bit, 0) And w.DeleteTransactionId = 0x And w.ParentWebId is null ";
                                }
                            }

                            using (SqlDataReader sqlReader = sqlConn.ExecuteReader(cmdText))
                            {
                                while (sqlReader.Read())
                                {
                                    try
                                    {
                                        Guid id = sqlReader.GetGuid(0);
                                        int templateId = sqlReader.GetInt32(2);
                                        int provisionConfig = sqlReader.GetInt16(3);
                                        uint language = (uint)sqlReader.GetInt32(4);
                                        int auditFlags = sqlReader.IsDBNull(7) ? 0 : sqlReader.GetInt32(7);
                                        uint bitFlags = (uint)sqlReader.GetInt32(8);
                                        var scopeId = sqlReader.GetGuid(9);

                                        //filter policy
                                        DateTime modified = DateTime.MinValue;
                                        DateTime created = DateTime.MinValue;
                                        string ownerLoginName = string.Empty;
                                        string ownerTitle = string.Empty;
                                        long size = 0;
                                        Hashtable properties = new Hashtable();
                                        if (needFilterInfo)
                                        {

                                            modified = sqlReader.GetDateTime(10);
                                            created = sqlReader.GetDateTime(11);
                                            if (modified.Kind != DateTimeKind.Utc)
                                            {
                                                modified = DateTime.SpecifyKind(modified, DateTimeKind.Utc);
                                            }
                                            if (created.Kind != DateTimeKind.Utc)
                                            {
                                                created = DateTime.SpecifyKind(created, DateTimeKind.Utc);
                                            }
                                            byte[] metaInfo = sqlReader.GetValue(12) as byte[];
                                            ownerLoginName = sqlReader.GetString(13);
                                            ownerTitle = sqlReader.GetString(14);
                                            var rootWebId = sqlReader.GetGuid(15);
                                            size = sqlReader.GetInt64(16);
                                            properties = ConvertMetaInfoToColumnInfos(Encoding.UTF8.GetString(metaInfo));
                                            foreach (var key in GAPolicyHelper.keysNeedToDecryption)
                                            {
                                                if (properties != null && properties.ContainsKey(key))
                                                {
                                                    try
                                                    {
                                                        properties[key] = GAPolicyHelper.GetPolicyValue(properties[key].ToString(), id, rootWebId);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        logger.Warn("An error occurred when get GA plus policy value, key: [{0}], value: {1}. Reason: {2}.", key, properties[key], ex.ToString());
                                                    }
                                                }
                                            }
                                        }

                                        // webTemplates = GetWebTemplatesByLanguageAndPlatformVersion(webTemplates, language);
                                        head = Ave2010SiteFlags.httpsHostHeaderSiteUrlScheme(bitFlags) ? "https://" : "http://";
                                        string title = sqlReader.IsDBNull(5) ? string.Empty : sqlReader.GetString(5);
                                        string hostHeader = sqlReader.IsDBNull(6) ? string.Empty : sqlReader.GetString(6);
                                        if (siteIdInConfig != null && (!siteIdInConfig.ContainsKey(id) || siteIdInConfig[id] != contentDatabase.ID))
                                        {
                                            continue;
                                        }
                                        string url = string.Empty;
                                        bool isHostHeader = !string.IsNullOrEmpty(hostHeader) || (sitePathInConfig.ContainsKey(id) && !sitePathInConfig[id].StartsWith("/", StringComparison.OrdinalIgnoreCase));
                                        if (!isHostHeader)
                                        {
                                            url = webAppUrl + sqlReader.GetString(1);
                                            if (url.EndsWith("/", StringComparison.Ordinal))
                                            {
                                                url = url.Substring(0, (url.Length - 1));
                                            }
                                        }
                                        else
                                        {
                                            url = head;
                                            if (sitePathInConfig.ContainsKey(id))
                                            {
                                                url += sitePathInConfig[id];
                                            }
                                            else
                                            {
                                                url += hostHeader;
                                            }
                                        }
                                        string displayName = string.Empty;
                                        if (sitePathInConfig.ContainsKey(id))
                                        {
                                            displayName = sitePathInConfig[id];
                                        }
                                        // string templateTitle = string.Empty;
                                        //   string templateName = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates, ref templateTitle);
                                        AveSiteBrowserInfo siteBrowserInfo = new AveSiteBrowserInfo()
                                        {
                                            Url = url,
                                            ID = id,
                                            DisplayName = displayName,
                                            WebTemplateId = templateId,
                                            ProvisionConfig = provisionConfig,
                                            // TemplateName = templateName,
                                            //  TemplateTitle = templateTitle,
                                            Language = language,
                                            Title = title,
                                            AuditActions = auditFlags,
                                            BitFlags = bitFlags,
                                            IsHostHeader = isHostHeader,
                                            rootWebScopeId = scopeId
                                        };
                                        if (needFilterInfo)
                                        {
                                            siteBrowserInfo.Properties = properties;
                                            siteBrowserInfo.Created = created;
                                            siteBrowserInfo.Modified = modified;
                                            siteBrowserInfo.OwnerLoginName = ownerLoginName;
                                            siteBrowserInfo.OwnerTitle = ownerTitle;
                                            siteBrowserInfo.Size = size;
                                        }



                                        if (configDb != null && (!siteIdInConfig.ContainsKey(siteBrowserInfo.ID) || siteIdInConfig[siteBrowserInfo.ID] != contentDatabase.ID))
                                        {
                                            continue;
                                        }
                                        if (siteIdInConfig != null && siteIdInConfig.ContainsKey(id) && siteIdInConfig[siteBrowserInfo.ID] == contentDatabase.ID)
                                        {
                                            siteBrowserInfo.ContentDBID = contentDatabase.ID.ToString();
                                            siteBrowserInfo.ContentDBName = contentDBNameInConfig[siteBrowserInfo.ID];
                                        }
                                        allSitesUnderContentDatabase.Add(siteBrowserInfo);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetBrowserSites. Error Message: {0}", e.ToString());
                                    }
                                }
                            }
                            #endregion

                            #region get platformVersion
                            sqlConn.ClearParameters();
                            foreach (AveSiteBrowserInfo browserInfo in allSitesUnderContentDatabase)
                            {
                                try
                                {
                                    browserInfo.PlatformVersion = GetSitePlatformVersion(browserInfo.ID, sqlConn);
                                    webTemplates = GetWebTemplatesFromCache(browserInfo.ID, browserInfo.Language, browserInfo.PlatformVersion);
                                    browserInfo.TemplateName = WebTemplateIdName(browserInfo.WebTemplateId, browserInfo.ProvisionConfig.ToString(), webTemplates, ref browserInfo.TemplateTitle);
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("Get template name error.Url:{0}.Language:{1}.PlatformVersion:{2}.Error:{3}", browserInfo.Url, browserInfo.Language, browserInfo.PlatformVersion, e);
                                    //throw;
                                }
                            }

                            #endregion

                            sites.AddRange(allSitesUnderContentDatabase);

                            #region Old Security Trimming with AD
                            //if (usernames != null && usernames.Count > 0)
                            //{
                            //    #region filter by user name

                            //    sqlConn.ClearParameters();
                            //    foreach (AveSiteBrowserInfo browserInfo in allSitesUnderContentDatabase)
                            //    {
                            //        try
                            //        {
                            //            cmdText = BuildCommandText(sqlConn, browserInfo);
                            //            using (SqlDataReader reader = sqlConn.ExecuteReader(cmdText))
                            //            {
                            //                while (reader.Read())
                            //                {
                            //                    var loginName = reader.GetString(0);
                            //                    var IsGroup = reader.GetBoolean(1);
                            //                    var systemId = reader.GetValue(2);
                            //                    var isSiteAdmin = reader.GetBoolean(3);
                            //                    var mask = (long)reader.GetDecimal(4);
                            //                    foreach (var user in usernames)
                            //                    {
                            //                        ActiveDirectoryObject searchUserObject = null;
                            //                        #region find the user from control
                            //                        if (!usersCache.TryGetValue(user, out searchUserObject))
                            //                        {
                            //                            var outboundObject = true;
                            //                            searchUserObject = AveBrowserHelper.GetADObjectByLoginName(user, outboundDirectoryDomains, bidirectionalDirectoryDomains, out outboundObject);
                            //                            usersCache[user] = searchUserObject;
                            //                        }
                            //                        if (searchUserObject == null)
                            //                        {
                            //                            continue;
                            //                        }
                            //                        #endregion
                            //                        if (!userGroupMatchCache.ContainsKey(user))
                            //                        {
                            //                            userGroupMatchCache[user] = new Dictionary<string, bool>();
                            //                        }

                            //                        SPTreePermission tempPermission;
                            //                        if (!browserInfo.Masks.TryGetValue(user, out tempPermission))
                            //                        {
                            //                            tempPermission = new SPTreePermission();
                            //                            browserInfo.Masks[user] = tempPermission;
                            //                        }
                            //                        if (!CheckSecurityTrimmingNeedContinue(tempPermission, loginName, isSiteAdmin, mask, usersCache))
                            //                        {
                            //                            continue;
                            //                        }
                            //                        if (IsGroup)//verify ad group
                            //                        {
                            //                            ActiveDirectoryObject groupObject = null;
                            //                            var outboundObject = true;
                            //                            if (!usersCache.TryGetValue(loginName, out groupObject))
                            //                            {
                            //                                int index = loginName.IndexOf('\\');
                            //                                if (index > 0)
                            //                                {
                            //                                    string realName = AveBrowserHelper.GetUserRealLoginName(loginName, domainMapping);
                            //                                    groupObject = AveBrowserHelper.GetADObjectByLoginName(realName, outboundDirectoryDomains, bidirectionalDirectoryDomains, out outboundObject);
                            //                                }
                            //                                else
                            //                                {
                            //                                    groupObject = AveBrowserHelper.GetADGroupObjectBySID(loginName, outboundDirectoryDomains, bidirectionalDirectoryDomains, out outboundObject);
                            //                                }
                            //                                usersCache[loginName] = groupObject;
                            //                            }
                            //                            if (userGroupMatchCache[user].ContainsKey(loginName))
                            //                            {
                            //                                if (userGroupMatchCache[user][loginName])
                            //                                {
                            //                                    tempPermission.GrantMask |= mask;
                            //                                    tempPermission.IsSiteCollectionAdmin |= isSiteAdmin;
                            //                                }
                            //                            }
                            //                            else
                            //                            {
                            //                                if (groupObject != null && searchUserObject.IsMemeberOf(groupObject))
                            //                                {
                            //                                    tempPermission.GrantMask |= mask;
                            //                                    tempPermission.IsSiteCollectionAdmin |= isSiteAdmin;
                            //                                    userGroupMatchCache[user][loginName] = true;
                            //                                }
                            //                                else
                            //                                {
                            //                                    userGroupMatchCache[user][loginName] = false;
                            //                                }
                            //                            }
                            //                        }
                            //                        else
                            //                        {
                            //                            var index = loginName.IndexOf('\\');
                            //                            if (index > 0)
                            //                            {
                            //                                loginName = AveBrowserHelper.GetUserRealLoginName(loginName, domainMapping);
                            //                            }
                            //                            if (user.Equals(loginName, StringComparison.OrdinalIgnoreCase))
                            //                            {
                            //                                tempPermission.GrantMask |= mask;
                            //                                tempPermission.IsSiteCollectionAdmin |= isSiteAdmin;
                            //                            }
                            //                        }

                            //                    }
                            //                }
                            //            }
                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            logger.Warn("Run security trim  under content:{0} of web application:{1} failed:{2}.", contentDatabase.Name, webAppUrl, ex.ToString());
                            //        }
                            //    }
                            //    #endregion
                            //}
                            #endregion
                        }
                        catch (Exception e)
                        {
                            hasError = true;
                            logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserSiteInfoFromContentDBError, e.ToString());
                        }
                    }
                    //由于通过native model取到的site无法过滤掉manage path被删除的site，需要过滤。
                    sites = sites.Where
                                 (siteInfo => AveUrlUtility.CheckManagedPath(webApp, siteInfo.Url, siteInfo.IsHostHeader)).ToList();
                    #region Security Trimming
                    if (usernames != null && usernames.Count > 0 && sites.Count > 0)
                    {
                        using (new PerformanceScope("Security Trimming"))
                        {
                            Dictionary<string, SPUserToken> tokens;
                            using (new PerformanceScope("Security Trimming---token"))
                            {
                                if (WrapperConfiguration.GenerateTokenDirectly)
                                {
                                    tokens = GenerateUserTokens(webApp, usernames);
                                }
                                else
                                {
                                    tokens = GetUserTokens(webApp, sites[0].ID, usernames);
                                }
                            }

                            foreach (var token in tokens)
                            {
                                foreach (AveSiteBrowserInfo browserInfo in sites)
                                {
                                    try
                                    {

                                        SPTreePermission tempPermission;
                                        if (!browserInfo.Masks.TryGetValue(token.Key, out tempPermission))
                                        {
                                            tempPermission = new SPTreePermission();
                                            browserInfo.Masks[token.Key] = tempPermission;
                                        }

                                        using (var site = new SPSite(browserInfo.ID, token.Value))
                                        {
                                            using (var web = site.RootWeb)
                                            {
                                                tempPermission.GrantMask |= (long)web.EffectiveBasePermissions;
                                                var currentUser = web.CurrentUser;
                                                if (currentUser != null)
                                                {
                                                    tempPermission.IsSiteCollectionAdmin |= currentUser.IsSiteAdmin;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Warn("get security trimming infor for site:{0}->{1} with user:{2}, details:{3}", browserInfo.Url, browserInfo.ID, token.Key, ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    hasError = true;
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserSiteInfoError, e.ToString());
                }
                finally
                {
                    if (sqlConn != null)
                    {
                        sqlConn.Dispose();
                    }
                    #region Dispose AD Object.
                    //AveBrowserHelper.DisposeADCache(bidirectionalDirectoryDomains);
                    //AveBrowserHelper.DisposeADCache(outboundDirectoryDomains);
                    //AveBrowserHelper.DisposeADCache(usersCache);
                    #endregion
                }
                childrenCount = sites.Count;
                var pageCount = perPage > childrenCount ? childrenCount : (int)perPage;
                sites.Sort(new AveSiteBrowserInfoComparer());
                return sites.Skip<AveSiteBrowserInfo>(startIndex).Take<AveSiteBrowserInfo>(pageCount).ToList<AveSiteBrowserInfo>();
            }
        }

        public List<AveSiteBrowserInfo> GetBrowserSitesWithToken(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false)
        {
            using (new PerformanceScope("GetBrowserSitesWithToken"))
            {
                var sites = new List<AveSiteBrowserInfo>();
                SPWebTemplateCollection webTemplates = null;
                //List<AveSiteDto> siteDtos = new List<AveSiteDto>();
                Dictionary<Guid, Guid> siteIdInConfig = null;
                Dictionary<Guid, string> sitePathInConfig = null;
                Dictionary<Guid, string> contentDBNameInConfig = null;
                AveSqlConnection sqlConn = null;

                string webAppUrl = string.Empty;

                try
                {
                    webAppUrl = webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString();
                    if (!webAppUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        webAppUrl += "/";
                    }

                    #region Get siteCollectionId and ContentDBId

                    AveConfigurationDatabase configDb = (AveConfigurationDatabase)Invoker.GetProperty(webApp, "ConfigurationDatabase");
                    if (configDb != null)
                    {
                        using (AveSqlConnection sqlConn_ConfigDB = new AveSqlConnection())
                        {
                            sqlConn_ConfigDB.Open(configDb.DatabaseConnectionString);
                            siteIdInConfig = new Dictionary<Guid, Guid>();
                            sitePathInConfig = new Dictionary<Guid, string>();
                            contentDBNameInConfig = new Dictionary<Guid, string>();
                            sqlConn_ConfigDB.Command.Parameters.AddWithValue("@ApplicationId", webApp.ID);
                            string cmdText_ConfigDB = "SELECT s.Id,s.DatabaseId,s.Path,o.Name FROM SiteMap s With(NoLock) INNER JOIN Objects o With(NoLock) ON s.DatabaseId=o.Id  WHERE ApplicationId=@ApplicationId";
                            if (AveSPUtility.IsSP1DBSchema(sqlConn_ConfigDB, "DeleteTransactionId", "SiteMap"))
                            {
                                cmdText_ConfigDB = "SELECT s.Id,s.DatabaseId,s.Path,o.Name FROM SiteMap s With(NoLock) INNER JOIN Objects o With(NoLock) ON s.DatabaseId=o.Id  WHERE ApplicationId=@ApplicationId AND DeleteTransactionId=0x";
                            }
                            using (SqlDataReader sqlReader_ConfigDB = sqlConn_ConfigDB.ExecuteReader(cmdText_ConfigDB))
                            {
                                while (sqlReader_ConfigDB.Read())
                                {
                                    var siteId = sqlReader_ConfigDB.GetGuid(0);
                                    if (sitePathInConfig.ContainsKey(siteId) || siteIdInConfig.ContainsKey(siteId) || contentDBNameInConfig.ContainsKey(siteId))
                                    {
                                        logger.Warn(string.Format("There are same site id in SiteMap.The site id is {0}.", siteId.ToString()));
                                    }
                                    else
                                    {
                                        siteIdInConfig[siteId] = sqlReader_ConfigDB.GetGuid(1);
                                        sitePathInConfig[siteId] = sqlReader_ConfigDB.GetString(2);
                                        contentDBNameInConfig[siteId] = sqlReader_ConfigDB.GetString(3);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Get All Site Collections
                    using (new PerformanceScope("Get All Site Collections"))
                    {
                        sqlConn = new AveSqlConnection();
                        string head = webAppUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? "http://" : "https://";
                        foreach (IAveContentDatabase contentDatabase in webApp.ContentDatabases)
                        {
                            try
                            {
                                sqlConn.Open(contentDatabase.DatabaseConnectionString);

                                List<AveSiteBrowserInfo> allSitesUnderContentDatabase = new List<AveSiteBrowserInfo>();

                                #region Get All Site Collections under this content database

                                string cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId
                                                   FROM Webs w With(NoLock) INNER JOIN Sites s With(NoLock) ON w.SiteId = s.Id WHERE w.ParentWebId is null ";
                                if (needFilterInfo)
                                {
                                    cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId,s.LastContentChange,w.TimeCreated,w.MetaInfo,u.tp_Login,u.tp_Title,w.Id,s.DiskUsed
                                                FROM Webs w With(NoLock) INNER JOIN Sites s With(NoLock) ON w.SiteId = s.Id LEFT JOIN UserInfo u With(NoLock) on u.tp_SiteID=s.Id and u.tp_ID=s.OwnerID WHERE w.ParentWebId is null ";
                                }
                                if (AveSPUtility.IsSP1DBSchema(sqlConn))
                                {
                                    cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId
                                                FROM AllWebs w With(NoLock) INNER JOIN AllSites s With(NoLock) ON w.SiteId = s.Id WHERE s.Deleted = CONVERT(bit, 0) And w.DeleteTransactionId = 0x And w.ParentWebId is null ";
                                    if (needFilterInfo)
                                    {
                                        cmdText = @"SELECT w.SiteId, w.FullUrl,w.WebTemplate,w.ProvisionConfig,w.Language,w.Title,s.HostHeader,s.AuditFlags,s.BitFlags,w.ScopeId,s.LastContentChange,w.TimeCreated,w.MetaInfo,u.tp_Login,u.tp_Title ,w.Id,s.DiskUsed 
                                                FROM AllWebs w With(NoLock) INNER JOIN AllSites s With(NoLock) ON w.SiteId = s.Id LEFT JOIN UserInfo u With(NoLock) on u.tp_SiteID=s.Id and u.tp_ID=s.OwnerID WHERE s.Deleted = CONVERT(bit, 0) And w.DeleteTransactionId = 0x And w.ParentWebId is null ";
                                    }
                                }

                                using (SqlDataReader sqlReader = sqlConn.ExecuteReader(cmdText))
                                {
                                    while (sqlReader.Read())
                                    {
                                        try
                                        {
                                            Guid id = sqlReader.GetGuid(0);
                                            int templateId = sqlReader.GetInt32(2);
                                            int provisionConfig = sqlReader.GetInt16(3);
                                            uint language = (uint)sqlReader.GetInt32(4);
                                            int auditFlags = sqlReader.IsDBNull(7) ? 0 : sqlReader.GetInt32(7);
                                            uint bitFlags = (uint)sqlReader.GetInt32(8);
                                            var scopeId = sqlReader.GetGuid(9);

                                            //filter policy
                                            DateTime modified = DateTime.MinValue;
                                            DateTime created = DateTime.MinValue;
                                            string ownerLoginName = string.Empty;
                                            string ownerTitle = string.Empty;
                                            long size = 0;
                                            Hashtable properties = new Hashtable();
                                            if (needFilterInfo)
                                            {

                                                modified = sqlReader.GetDateTime(10);
                                                created = sqlReader.GetDateTime(11);
                                                byte[] metaInfo = sqlReader.GetValue(12) as byte[];
                                                ownerLoginName = sqlReader.GetString(13);
                                                ownerTitle = sqlReader.GetString(14);
                                                var rootWebId = sqlReader.GetGuid(15);
                                                size = sqlReader.GetInt64(16);
                                                properties = ConvertMetaInfoToColumnInfos(Encoding.UTF8.GetString(metaInfo));
                                                foreach (var key in GAPolicyHelper.keysNeedToDecryption)
                                                {
                                                    if (properties != null && properties.ContainsKey(key))
                                                    {
                                                        try
                                                        {
                                                            properties[key] = GAPolicyHelper.GetPolicyValue(properties[key].ToString(), id, rootWebId);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            logger.Warn("An error occurred when get GA plus policy value, key:[{0}], value:{1}. Reason:{2].", key, properties[key], ex.ToString());
                                                        }
                                                    }
                                                }
                                            }

                                            head = Ave2010SiteFlags.httpsHostHeaderSiteUrlScheme(bitFlags) ? "https://" : "http://";
                                            string title = sqlReader.IsDBNull(5) ? string.Empty : sqlReader.GetString(5);
                                            string hostHeader = sqlReader.IsDBNull(6) ? string.Empty : sqlReader.GetString(6);
                                            if (siteIdInConfig != null && (!siteIdInConfig.ContainsKey(id) || siteIdInConfig[id] != contentDatabase.ID))
                                            {
                                                continue;
                                            }
                                            string url = string.Empty;
                                            bool isHostHeader = false;
                                            if (string.IsNullOrEmpty(hostHeader))
                                            {
                                                url = webAppUrl + sqlReader.GetString(1);
                                                if (url.EndsWith("/", StringComparison.Ordinal))
                                                {
                                                    url = url.Substring(0, (url.Length - 1));
                                                }
                                            }
                                            else
                                            {
                                                url = head;
                                                isHostHeader = true;
                                                if (sitePathInConfig.ContainsKey(id))
                                                {
                                                    url += sitePathInConfig[id];
                                                }
                                                else
                                                {
                                                    url += hostHeader;
                                                }
                                            }
                                            string displayName = string.Empty;
                                            if (sitePathInConfig.ContainsKey(id))
                                            {
                                                displayName = sitePathInConfig[id];
                                            }
                                            //string templateTitle = string.Empty;
                                            //string templateName = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates, ref templateTitle);
                                            AveSiteBrowserInfo siteBrowserInfo = new AveSiteBrowserInfo()
                                            {
                                                Url = url,
                                                ID = id,
                                                DisplayName = displayName,
                                                //TemplateName = templateName,
                                                //TemplateTitle = templateTitle,
                                                Language = language,
                                                Title = title,
                                                AuditActions = auditFlags,
                                                BitFlags = bitFlags,
                                                IsHostHeader = isHostHeader,
                                                rootWebScopeId = scopeId
                                            };
                                            if (needFilterInfo)
                                            {
                                                siteBrowserInfo.Properties = properties;
                                                siteBrowserInfo.Created = created;
                                                siteBrowserInfo.Modified = modified;
                                                siteBrowserInfo.OwnerLoginName = ownerLoginName;
                                                siteBrowserInfo.OwnerTitle = ownerTitle;
                                                siteBrowserInfo.Size = size;
                                            }



                                            if (configDb != null && (!siteIdInConfig.ContainsKey(siteBrowserInfo.ID) || siteIdInConfig[siteBrowserInfo.ID] != contentDatabase.ID))
                                            {
                                                continue;
                                            }
                                            if (siteIdInConfig != null && siteIdInConfig.ContainsKey(id) && siteIdInConfig[siteBrowserInfo.ID] == contentDatabase.ID)
                                            {
                                                siteBrowserInfo.ContentDBID = contentDatabase.ID.ToString();
                                                siteBrowserInfo.ContentDBName = contentDBNameInConfig[siteBrowserInfo.ID];
                                            }
                                            allSitesUnderContentDatabase.Add(siteBrowserInfo);
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Log(AveLogLevel.WARN, "Error occur while access data from GetBrowserSites.  ErrorMessage:{0}", e.ToString());
                                        }
                                    }
                                }
                                #endregion

                                sites.AddRange(allSitesUnderContentDatabase);
                            }
                            catch (Exception e)
                            {
                                hasError = true;
                                logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserSiteInfoFromContentDBError, e.ToString());
                            }
                        }
                    }
                    #endregion

                    //由于通过native model取到的site无法过滤掉manage path被删除的site，需要过滤。
                    sites = sites.Where
                                 (siteInfo => AveUrlUtility.CheckManagedPath(webApp, siteInfo.Url, siteInfo.IsHostHeader)).ToList();

                    #region Security Trimming
                    if (usernames != null && usernames.Count > 0 && sites.Count > 0)
                    {
                        using (new PerformanceScope("Security Trimming"))
                        {
                            Dictionary<string, SPUserToken> tokens;
                            using (new PerformanceScope("Security Trimming---token"))
                            {
                                if (WrapperConfiguration.GenerateTokenDirectly)
                                {
                                    tokens = GenerateUserTokens(webApp, usernames);
                                }
                                else
                                {
                                    tokens = GetUserTokens(webApp, sites[0].ID, usernames);
                                }
                            }

                            foreach (var token in tokens)
                            {
                                foreach (AveSiteBrowserInfo browserInfo in sites)
                                {
                                    try
                                    {

                                        SPTreePermission tempPermission;
                                        if (!browserInfo.Masks.TryGetValue(token.Key, out tempPermission))
                                        {
                                            tempPermission = new SPTreePermission();
                                            browserInfo.Masks[token.Key] = tempPermission;
                                        }

                                        using (var site = new SPSite(browserInfo.ID, token.Value))
                                        {
                                            using (var web = site.RootWeb)
                                            {
                                                tempPermission.GrantMask |= (long)web.EffectiveBasePermissions;
                                                var currentUser = web.CurrentUser;
                                                if (currentUser != null)
                                                {
                                                    tempPermission.IsSiteCollectionAdmin |= currentUser.IsSiteAdmin;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Warn("get security trimming infor for site:{0}->{1} with user:{2}, details:{3}", browserInfo.Url, browserInfo.ID, token.Key, ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    hasError = true;
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserSiteInfoError, e.ToString());
                }
                finally
                {
                    if (sqlConn != null)
                    {
                        sqlConn.Dispose();
                    }
                }
                childrenCount = sites.Count;
                var pageCount = perPage > childrenCount ? childrenCount : (int)perPage;
                sites.Sort(new AveSiteBrowserInfoComparer());
                return sites.Skip<AveSiteBrowserInfo>(startIndex).Take<AveSiteBrowserInfo>(pageCount).ToList<AveSiteBrowserInfo>();
            }

        }

        public Dictionary<string, SPUserToken> GenerateUserTokens(IAveWebApplication webApplication, List<string> userNames)
        {
            var tokens = new Dictionary<string, SPUserToken>();

            var domainMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var webApplicationUri = webApplication.GetResponseUri(AveUrlZone.Default);
            var farmId = webApplication.Farm.ID.ToString();

            foreach (var userName in userNames)
            {
                var userInfo = ResolveUser(null, userName, domainMapping);

                tokens[userName] = GenerateUserToken(userInfo, webApplicationUri, farmId); ;
            }

            return tokens;
        }

        public Dictionary<string, SPUserToken> GetUserTokens(IAveWebApplication webApplication, Guid siteId, List<string> userNames)
        {
            var tokens = new Dictionary<string, SPUserToken>();

            var domainMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var webApplicationUri = webApplication.GetResponseUri(AveUrlZone.Default);
            var farmId = webApplication.Farm.ID.ToString();

            string prefix = null;
            if (webApplication.UseClaimsAuthentication)
            {
                prefix = "i:0#.w|";
            }

            using (var site = new SPSite(siteId))
            {
                foreach (var userName in userNames)
                {
                    SPUserToken token = null;

                    var userInfo = ResolveUser(prefix, userName, domainMapping);

                    try
                    {
                        token = site.RootWeb.GetUserToken(userInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Get user token of {0}->{1} with API failed:{2}", userName, userInfo.FullName, ex);
                        token = GenerateUserToken(userInfo, webApplicationUri, farmId);
                    }

                    tokens[userName] = token;
                }
            }

            return tokens;
        }

        private UserInfo ResolveUser(string prefix, string userName, Dictionary<string, string> domainMapping)
        {
            UserInfo info = null;
            var index = userName.IndexOf('\\');

            if (index > 0)
            {
                info = new UserInfo();
                info.DomainFullName = userName.Substring(0, index);
                info.Name = userName.Substring(index + 1);
                info.DomainName = GetDomainNetbiosNameWithMapping(info.DomainFullName, domainMapping);
                info.LogonName = string.Concat(info.DomainName, "\\", info.Name);
                info.FullName = string.Concat(prefix, info.LogonName);
            }

            return info;
        }

        private string GetDomainNetbiosNameWithMapping(string fqdn, Dictionary<string, string> domainMapping)
        {
            string netbiosName;
            if (!domainMapping.TryGetValue(fqdn, out netbiosName))
            {
                netbiosName = GetDomainNetbiosName(fqdn);
                domainMapping[fqdn] = netbiosName;
                logger.Debug("domain mapping:{0}-->{1}", fqdn, netbiosName);
            }

            return netbiosName;
        }

        /// <summary>
        /// 不支持信任域
        /// </summary>
        /// <param name="fqdn"></param>
        /// <returns></returns>
        private string GetDomainNetbiosName(string fqdn)
        {
            try
            {
                try
                {
                    var netbios = GetNetbiosDomainName(fqdn);

                    if (!string.IsNullOrEmpty(netbios))
                    {
                        return netbios;
                    }
                }
                catch (Exception trustRelationShipEx)
                {
                    logger.Warn("Cannot get details for domain:{0}, details:{1}", fqdn, trustRelationShipEx);
                }

                using (var currentDomain = Domain.GetCurrentDomain())
                {
                    var trustInfo = currentDomain.GetTrustRelationship(fqdn);
                    if (trustInfo.TrustType != TrustType.Unknown)
                    {
                        return trustInfo.TargetName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred while getting real domain, Domain: {0}, Error Message : {0}", fqdn, ex.ToString());
            }

            var index = fqdn.IndexOf('.');

            if (index > 0)
            {
                var name = fqdn.Substring(0, index);

                logger.Warn("Cannot get domain net bios name, so return the first part {0}-->{1}", fqdn, name);

                return name;
            }

            return fqdn;
        }

        /// <summary>
        /// Get AD Domain NetBios Name
        /// </summary>
        /// <param name="dnsDomainName">DNS Suffix Name</param>
        /// <returns></returns>
        public string GetNetbiosDomainName(string dnsDomainName)
        {
            string netbiosDomainName = null;

            using (var rootDSE = new DirectoryEntry(string.Format("LDAP://{0}/RootDSE", dnsDomainName)))
            {
                string configurationNamingContext = rootDSE.Properties["configurationNamingContext"][0].ToString();

                using (var searchRoot = new DirectoryEntry("LDAP://cn=Partitions," + configurationNamingContext))
                {
                    using (var searcher = new DirectorySearcher(searchRoot))
                    {
                        searcher.SearchScope = SearchScope.OneLevel;
                        // searcher.PropertiesToLoad.Add("netbiosname");
                        searcher.Filter = string.Format("(&(objectcategory=Crossref)(dnsRoot={0})(netBIOSName=*))", dnsDomainName);

                        SearchResult result = searcher.FindOne();

                        if (result != null)
                        {
                            netbiosDomainName = result.Properties["netbiosname"][0].ToString();
                        }
                    }
                }

                return netbiosDomainName;
            }
        }

        private SPUserToken GenerateUserToken(UserInfo userInfo, Uri webAppUrl, string farmId)
        {
            var sids = GetSIDAndParentGroupSID(userInfo);

            var cI = new ClaimsIdentity();
            cI.Claims.Add(new Claim("http://schemas.microsoft.com/sharepoint/2009/08/claims/identityprovider", "windows", "", "Windows"));
            cI.Claims.Add(new Claim("http://sharepoint.microsoft.com/claims/2009/08/isauthenticated", "True", "", "Windows"));
            cI.Claims.Add(new Claim("http://schemas.microsoft.com/sharepoint/2009/08/claims/userlogonname", userInfo.LogonName, "", "Windows"));
            cI.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", string.Concat("0#.w|", userInfo.LogonName), "", "Windows"));
            if (sids.Item1 != null)
            {
                cI.Claims.Add(new Claim("http://schemas.microsoft.com/office/2012/01/nameid", sids.Item1, "", "Windows"));
            }
            cI.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userInfo.LogonName, "", "Windows"));
            if (sids.Item2 != null && sids.Item2.Count > 0)
            {
                foreach (var groupSid in sids.Item2)
                {
                    cI.Claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid", groupSid, "", "Windows"));
                }

                cI.Claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid", sids.Item2[0], "", "Windows"));
            }
            if (sids.Item1 != null)
            {
                cI.Claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid", sids.Item1, "", "Windows"));
            }
            cI.Claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2008/06/identity/authenticationmethod/windows", "", "Windows"));
            cI.Claims.Add(new Claim("http://schemas.microsoft.com/office/2012/01/nameidissuer", "urn:office:idp:activedirectory", "", "Windows"));
            cI.Claims.Add(new Claim("http://sharepoint.microsoft.com/claims/2012/02/claimprovidercontext", webAppUrl.ToString(), "", "Windows"));
            cI.Claims.Add(new Claim("http://schemas.microsoft.com/sharepoint/2009/08/claims/farmid", farmId, "", "Windows"));
            cI.NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            cI.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid";
            return new SPUserToken(cI, webAppUrl);
        }

        private Tuple<string, List<string>> GetSIDAndParentGroupSID(UserInfo userInfo)
        {
            string userSid = null;
            var parentGroups = new List<string>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, userInfo.DomainName))
                {
                    using (var user = UserPrincipal.FindByIdentity(context, System.DirectoryServices.AccountManagement.IdentityType.SamAccountName, userInfo.Name))
                    {
                        if (user != null)
                        {
                            userSid = user.Sid.Value;
                            PrincipalSearchResult<Principal> groups = null;
                            try
                            {
                                groups = user.GetAuthorizationGroups();
                            }
                            catch (Exception e)
                            {
                                logger.Debug("Get user's authorization group failed. Will use GetGroups method instead. Error:{0}  ", e);
                                groups = user.GetGroups();
                            }
                            var builder = new StringBuilder();
                            builder.AppendLine(user.Name);
                            using (groups)
                            {
                                using (var enumerator = groups.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        try
                                        {
                                            var currentValue = enumerator.Current;
                                            if (currentValue != null)
                                            {
                                                parentGroups.Add(currentValue.Sid.Value);
                                                builder.Append(currentValue.DistinguishedName);
                                                builder.Append(" | ");
                                                builder.Append(currentValue.Name);
                                                builder.AppendLine();
                                            }
                                        }
                                        catch (Exception groupException)
                                        {
                                            logger.Warn("get one group failed with user login name is {0}. Error message:{1}", userInfo.LogonName, groupException.ToString());
                                        }
                                    }
                                }
                            }
                            logger.Debug("User Information-->{0}", builder);
                        }
                        else
                        {
                            logger.Error("Cannot get user with name:{0} from domain:{1}", userInfo.Name, userInfo.DomainName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting user's parent groups, user login name is {0}. Error message:{1}", userInfo.LogonName, e.ToString());
            }
            return new Tuple<string, List<string>>(userSid, parentGroups);
        }

        private bool CheckSecurityTrimmingNeedContinue(SPTreePermission tempPermission, string loginName, bool isSiteAdmin, long mask, Dictionary<string, ActiveDirectoryObject> usersCache)
        {
            //四种情况不需要继续check:
            //1.已经确认到当前User是site admin.
            //2.已经确认到当前User含有FullControl权限,并且查询到的User/Group不是site admin.
            //3.已经确认到的当前user的权限,包含查询到的User/Group的权限。
            //4.查询到的usersCache在Cache里存在,但其对应的ActiveDirectoryObject是Null.
            if (tempPermission.IsSiteCollectionAdmin
                || (tempPermission.GrantMask == 9223372036854775807 && !isSiteAdmin)
                || usersCache.ContainsKey(loginName) && usersCache[loginName] == null
                || ((tempPermission.GrantMask | mask) == tempPermission.GrantMask)
                )
            {
                return false;
            }
            return true;
        }

        private class AveSiteBrowserInfoComparer : IComparer<AveSiteBrowserInfo>
        {
            public int Compare(AveSiteBrowserInfo x, AveSiteBrowserInfo y)
            {
                if (!x.IsHostHeader && y.IsHostHeader)
                {
                    return 1;
                }
                if (x.IsHostHeader && !y.IsHostHeader)
                {
                    return -1;
                }
                return string.Compare(x.Url, y.Url, StringComparison.CurrentCultureIgnoreCase);
            }
        }


        private string GetSitePlatformVersion(Guid siteId, AveSqlConnection sqlConn)
        {
            string platformVersion = string.Empty;
            try
            {
                string cmdText = @"SELECT PlatformVersion FROM AllSites With(nolock) WHERE Id=@SiteId";
                sqlConn.AddParameter("@SiteId", siteId);
                using (SqlDataReader reader = sqlConn.ExecuteReader(cmdText))
                {
                    if (reader.Read())
                    {
                        platformVersion = reader.GetString(0);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Cannot get site platform version, site Id: {0}, failed: {1}", siteId, ex.ToString());
            }
            return platformVersion;
        }

        private int GetPlatformVersion(string platformVersion)
        {
            if (string.IsNullOrEmpty(platformVersion))
            {
                return (int)CompatibilityLevelType.SP2013Mode;
            }
            else
            {
                try
                {
                    return (int)GetCompatibilityLevelType(new Version(platformVersion).Major);
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.INFO, "cannot parse platform version:{0}, exception:{1}", platformVersion, ex.ToString());
                }
            }
            return (int)CompatibilityLevelType.SP2013Mode;
        }

        private CompatibilityLevelType GetCompatibilityLevelType(int majorVersion)
        {
            switch (majorVersion)
            {
                case 0:
                case 2:
                case 3:
                case 4:
                case 11:
                case 12:
                case 14:
                    return CompatibilityLevelType.SP2010Mode;
                default:
                    return CompatibilityLevelType.SP2013Mode;
            }
        }

        private string GetWebTemplatesCacheKey(uint language, int platformVersion)
        {
            return string.Format("{0}{1}", language, platformVersion);
        }


        private SPWebTemplateCollection GetWebTemplatesFromCache(Guid siteId, uint language, string platformVersion)
        {
            SPWebTemplateCollection webTemplates = null;
            int version = GetPlatformVersion(platformVersion);
            string key = GetWebTemplatesCacheKey(language, version);
            if (!HasCachedWebTemplates(key))
            {
                webTemplates = GetWebTemplates(siteId, language, version);
                //mWebTemplates.Add(language, webTemplates);
                lock (mWebTemplates)
                {
                    if (!mWebTemplates.ContainsKey(key))
                    {
                        mWebTemplates[key] = webTemplates;
                    }
                }
            }
            else
            {
                webTemplates = (SPWebTemplateCollection)mWebTemplates[key];
            }
            return webTemplates;
        }

        private bool HasCachedWebTemplates(string key)
        {
            lock (mWebTemplates)
            {
                return mWebTemplates.ContainsKey(key);
            }
        }

        private List<AveBasePermissions> filterMask = new List<AveBasePermissions>
        {
            AveBasePermissions.ViewListItems ,
            AveBasePermissions.Open ,
            AveBasePermissions.ViewPages
        };

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wrong word is sql command text")]
        private string BuildCommandText(AveSqlConnection sqlConn, AveSiteBrowserInfo browserInfo)
        {
            sqlConn.AddParameter("@SiteId", browserInfo.ID);
            sqlConn.AddParameter("@ScopeId", browserInfo.rootWebScopeId);
            var cmdText = @"
SELECT u.tp_Login,u.tp_DomainGroup,u.tp_SystemID,u.tp_SiteAdmin,rl.PermMask
FROM UserInfo u With(NoLock)
INNER JOIN RoleAssignment r With(NoLock)
ON u.tp_SiteID = r.SiteId AND r.ScopeId = @ScopeId AND u.tp_ID = r.PrincipalId
INNER JOIN Roles rl With(NoLock)
ON u.tp_SiteID = rl.SiteId AND r.RoleId = rl.RoleId
@WHERE
UNION 
SELECT DISTINCT tp_Login,tp_DomainGroup,tp_SystemID,tp_SiteAdmin,(9223372036854775807)as PermMask FROM UserInfo With(nolock) WHERE tp_SiteID=@SiteId and tp_SiteAdmin = 1 and tp_Deleted=0 
UNION
SELECT u.tp_Login,u.tp_DomainGroup,u.tp_SystemID,u.tp_SiteAdmin,rl.PermMask
FROM Groups g With(NoLock)
INNER join GroupMembership gm With(NoLock)  on gm.SiteId = g.SiteId and gm.GroupId = g.ID
INNER join UserInfo u With(NoLock) on u.tp_SiteID = g.SiteId and u.tp_ID = gm.MemberId
INNER JOIN RoleAssignment r With(NoLock) ON g.SiteID = r.SiteId AND r.ScopeId = @ScopeId AND r.PrincipalId = g.ID
INNER JOIN Roles rl With(NoLock) ON u.tp_SiteID = rl.SiteId AND r.RoleId = rl.RoleId
@WHERE";
            var condition = new StringBuilder("WHERE tp_SiteID = @SiteId AND tp_Deleted = 0");
            return cmdText.Replace("@WHERE", condition.ToString());
        }

        //private class AveSiteBrowserInfoComparer : IComparer<AveSiteBrowserInfo>
        //{
        //    private string webAppUrl;

        //    public AveSiteBrowserInfoComparer(string webappUrl)
        //    {
        //        this.webAppUrl = webappUrl.TrimEnd('/');
        //    }

        //    public int Compare(AveSiteBrowserInfo x, AveSiteBrowserInfo y)
        //    {
        //        if (!x.Url.StartsWith(webAppUrl, StringComparison.OrdinalIgnoreCase) && y.Url.StartsWith(webAppUrl, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return 1;
        //        }
        //        else if (!y.Url.StartsWith(webAppUrl, StringComparison.OrdinalIgnoreCase) && x.Url.StartsWith(webAppUrl, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return -1;
        //        }
        //        return string.Compare(x.Url, y.Url, StringComparison.OrdinalIgnoreCase);
        //    }
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metaInfo is a parameter")]
        private Hashtable ConvertMetaInfoToColumnInfos(string metaInfo)
        {
            Hashtable siteCollectionColumn = new Hashtable();
            if (String.IsNullOrEmpty(metaInfo))
            {
                logger.Log(AveLogLevel.ERROR, string.Format("The site collection's metaInfo is Empty."));
                return siteCollectionColumn;
            }
            var hashCollectionColumn = AveCompressedUtility.GetMetaInfoHashtable(metaInfo);
            foreach (DictionaryEntry hashColumn in hashCollectionColumn)
            {
                switch ((hashColumn.Value as MetaInfoProperty).Type)
                {
                    case MetaInfoValueType.Boolean:
                        {
                            siteCollectionColumn[hashColumn.Key] = Convert.ToBoolean((hashColumn.Value as MetaInfoProperty).Value);
                            break;
                        }
                    case MetaInfoValueType.Integer:
                        {
                            siteCollectionColumn[hashColumn.Key] = Convert.ToInt32((hashColumn.Value as MetaInfoProperty).Value);
                            break;
                        }
                    case MetaInfoValueType.Time:
                        {
                            siteCollectionColumn[hashColumn.Key] = Convert.ToDateTime((hashColumn.Value as MetaInfoProperty).Value);
                            break;
                        }
                    default:
                        {
                            siteCollectionColumn[hashColumn.Key] = (hashColumn.Value as MetaInfoProperty).Value;
                            break;
                        }
                }

            }
            return siteCollectionColumn;
        }

        //        public List<AveWebBrowserInfo> GetBrowserWebs(Guid siteId, Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserWebs"))
        //            {
        //
        //                List<AveWebBrowserInfo> webBrowserInfos = new List<AveWebBrowserInfo>();
        //                SPWebTemplateCollection webTemplates = null;
        //                try
        //                {
        //                    using (var connect = new AveSqlConnection(mConnectString))
        //                    {
        //                        string platformVersion = GetSitePlatformVersion(siteId, connect);
        //                        connect.AddParameter("@siteId", siteId);
        //                        connect.AddParameter("@ParentWebId", parentWebId);
        //                        string cmdText = "SELECT FullUrl FROM Webs WHERE SiteId=@siteId AND Id=@ParentWebId";
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
        //                        }
        //                        string parentUrl = connect.ExecuteScalar(cmdText) as string;

        //                        cmdText = "SELECT count(Id) FROM Webs WHERE SiteId=@siteId AND ParentWebId=@ParentWebId";
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
        //                        }
        //                        childrenCount = (int)connect.ExecuteScalar(cmdText);
        //                        startIndex = startIndex > childrenCount ? 0 : startIndex;

        //                        cmdText = string.Format(@"SELECT * FROM (
        //SELECT top {0} Id,FullUrl,Title,Language,WebTemplate,ProvisionConfig,FirstUniqueAncestorWebId,AppInstanceId,ROW_NUMBER() OVER (ORDER BY FullUrl) AS RowNumber
        //FROM Webs 
        //WHERE SiteId=@siteId AND ParentWebId=@ParentWebId) As W
        //WHERE W.RowNumber > {1} ", perPage + startIndex, startIndex);

        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
        //                        }
        //                        using (SqlDataReader sr = connect.ExecuteReader(cmdText))
        //                        {
        //                            while (sr.Read())
        //                            {
        //                                try
        //                                {
        //                                    Guid id = sr.GetGuid(0);
        //                                    string name = sr.GetString(1);
        //                                    string title = null;
        //                                    int pos = -1;
        //                                    if (parentUrl != null && name.StartsWith(parentUrl, StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        if (parentUrl.Length != 0)
        //                                        {
        //                                            pos = parentUrl.Length;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        pos = name.LastIndexOf('/');
        //                                    }
        //                                    if (pos >= 0)
        //                                    {
        //                                        name = name.Substring(pos + 1);
        //                                    }

        //                                    if (!sr.IsDBNull(2))
        //                                    {
        //                                        title = sr.GetString(2);
        //                                    }
        //                                    string webUrl = sr.GetString(1);
        //                                    if (!webUrl.StartsWith("/", StringComparison.Ordinal))
        //                                    {
        //                                        webUrl = "/" + webUrl;
        //                                    }
        //                                    string fullUrl = new Uri(new Uri(mSiteUrl), webUrl).ToString();
        //                                    uint language = (uint)sr.GetInt32(3);
        //                                    //bool isRootWeb = false;
        //                                    bool hasUniqueRoleAssignments = (sr.GetGuid(0) == sr.GetGuid(6)) ? true : false;
        //                                    int templateId = sr.GetInt32(4);
        //                                    int provisionConfig = sr.GetInt16(5);
        //                                    webTemplates = GetWebTemplatesFromCache(siteId, language, platformVersion);
        //                                    string templateTitle = string.Empty;
        //                                    string templateName = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates, ref templateTitle);
        //                                    AveWebBrowserInfo webBrowserInfo = new AveWebBrowserInfo()
        //                                    {
        //                                        ID = id,
        //                                        Name = name,
        //                                        Url = fullUrl,
        //                                        Title = title,
        //                                        Language = language,
        //                                        IsRootWeb = false,
        //                                        HasUniqueRoleAssignments = hasUniqueRoleAssignments,
        //                                        TemplateName = templateName,
        //                                        TemplateTitle = templateTitle,
        //                                        IsAppWeb = !sr.GetGuid(7).Equals(Guid.Empty)
        //                                    };

        //                                    webBrowserInfos.Add(webBrowserInfo);
        //                                }
        //                                catch (Exception e)
        //                                {
        //                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserWebInfoFromContentDBError, e.ToString());
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserWebInfoError, e.ToString());
        //                }
        //                finally
        //                {
        //                }
        //                return webBrowserInfos;
        //
        //            }
        //
        //        }

        //public List<AveListBrowserInfo> GetBrowserLists(Guid siteId, Guid parentWebId)
        //{
        //    int childrenCount = 0;
        //    return GetBrowserLists(siteId, parentWebId, 0, uint.MaxValue, ref childrenCount);
        //}

        //        public List<AveListBrowserInfo> GetBrowserLists(Guid siteId, Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.Import"))
        //            {
        //
        //                List<AveListBrowserInfo> listBrowserInfos = new List<AveListBrowserInfo>();
        //                //bool isMyProfileList = false;

        //                try
        //                {
        //                    using (var connect = new AveSqlConnection(mConnectString))
        //                    {
        //                        connect.AddParameter("@SiteId", siteId);
        //                        connect.AddParameter("@WebId", parentWebId);
        //                        string cmdText = "SELECT count(tp_Id) FROM AllLists WHERE tp_WebId=@WebId and tp_DeleteTransactionId = 0x";
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = "SELECT count(tp_Id) FROM AllLists WHERE tp_WebId=@WebId and tp_DeleteTransactionId = 0x";
        //                        }
        //                        childrenCount = (int)connect.ExecuteScalar(cmdText);
        //                        childrenCount++; //add for {system folder} list
        //                        startIndex--;//for {system folder} list,startIndex需要减一
        //                        cmdText = string.Format(@"SELECT * FROM(
        //SELECT top {0} al.tp_ID, al.tp_Title, al.tp_BaseType, al.tp_ServerTemplate, ad.DirName, ad.LeafName, al.tp_Flags,al.tp_ScopeId,w.ScopeId,ROW_NUMBER() OVER (ORDER BY al.tp_Title) AS RowNumber 
        //FROM AllLists al with(nolock) 
        //INNER JOIN AllDocs ad with(nolock) ON al.tp_WebId=@WebId AND al.tp_DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND ad.Level=1 AND ad.DeleteTransactionId=0x 
        //INNER JOIN Webs w with(nolock) ON al.tp_WebId=w.Id ) AS temp
        //WHERE temp.RowNumber > {1}", perPage + startIndex, startIndex);

        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = string.Format(@"SELECT * FROM(
        //SELECT top {0} al.tp_ID, al.tp_Title, al.tp_BaseType, al.tp_ServerTemplate, ad.DirName, ad.LeafName, al.tp_Flags,al.tp_ScopeId,w.ScopeId,ROW_NUMBER() OVER (ORDER BY al.tp_Title) AS RowNumber
        //FROM (AllLists al with(nolock) 
        //INNER JOIN AllDocs ad with(nolock) ON al.tp_WebId=@WebId AND al.tp_DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND ad.Level=1 AND ad.DeleteTransactionId=0x) 
        //INNER JOIN AllWebs w with(nolock) ON al.tp_WebId=w.Id AND w.DeleteTransactionId=0x ) AS temp
        //WHERE temp.RowNumber > {1}", perPage + startIndex, startIndex);
        //                        }
        //                        using (SqlDataReader sr = connect.ExecuteReader(cmdText))
        //                        {
        //                            while (sr.Read())
        //                            {
        //                                try
        //                                {
        //                                    Guid id = sr.GetGuid(0);
        //                                    string title = sr.GetString(1);
        //                                    int baseType = sr.GetInt32(2);
        //                                    int serverTemplate = sr.GetInt32(3);
        //                                    var rootFolderName = sr.GetString(5);
        //                                    string dirName = sr.GetString(4);
        //                                    string serverRelativeUrl = string.IsNullOrEmpty(dirName) ? "/" + rootFolderName : "/" + dirName + "/" + rootFolderName;//root site 下的某些list的dirname为null
        //                                    bool hidden = (sr.GetInt64(6) & ((long)0x100L)) != 0L;
        //                                    string url = new Uri(new Uri(mSiteUrl), serverRelativeUrl).ToString();
        //                                    bool hasUniqueRoleAssignments = sr.GetGuid(7).Equals(sr.GetGuid(8)) ? false : true;
        //                                    bool enableFolderCreation = (sr.GetInt64(6) & 0x20000000) == 0;
        //                                    AveListBrowserInfo listDto = new AveListBrowserInfo()
        //                                    {
        //                                        ID = id,
        //                                        BaseType = baseType,
        //                                        BaseTemplate = serverTemplate,
        //                                        ServerRelativeUrl = serverRelativeUrl,
        //                                        Title = title,
        //                                        Hidden = hidden,
        //                                        Url = url,
        //                                        HasUniqueRoleAssignments = hasUniqueRoleAssignments,
        //                                        rootFolderName = rootFolderName,
        //                                        EnableFolderCreation = enableFolderCreation
        //                                    };

        //                                    listBrowserInfos.Add(listDto);
        //                                }
        //                                catch (Exception e)
        //                                {
        //                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserListInfoFromContentDBError, e.ToString());
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserListInfoError, e.ToString());
        //                }
        //                if (startIndex == -1)
        //                {
        //                    listBrowserInfos.Add(new AveListBrowserInfo()
        //                    {
        //                        ID = Guid.Empty,
        //                        Name = "{System Folder}",
        //                        Title = "{System Folder}",
        //                        rootFolderName = "Root Folder",
        //                        //这次返回的可能为perPage+1个
        //                    });
        //                }
        //                return listBrowserInfos;
        //
        //            }
        //
        //        }

        //        public List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid siteId, Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, int startIndex, uint perPage, ref int childrenCount)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserSubFolders"))
        //            {
        //
        //                List<AveFolderBrowserInfo> subFolders = new List<AveFolderBrowserInfo>();
        //                Guid parentFolderScopeId = Guid.Empty;

        //                string cmdText = "SELECT ScopeId from AllDocs where SiteId=@SiteId AND WebId=@ParentWebId AND Id=@Id AND DeleteTransactionId = 0x AND (Type=1 OR Type=2) AND IsCurrentVersion=1";
        //                using (var connect = new AveSqlConnection(mConnectString))
        //                {
        //                    connect.ClearParameters();
        //                    connect.AddParameter("@SiteId", siteId); //If it doesn't need site id, please uncomment this line. 
        //                    connect.AddParameter("@ParentWebId", parentWebId);
        //                    connect.AddParameter("@Id", parentFolderId);

        //                    try
        //                    {
        //                        parentFolderScopeId = (Guid)connect.ExecuteScalar(cmdText);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        logger.Log(AveLogLevel.WARN, "Error occurred while GetParentFolder ScopeId from GetBrowserSubFolders.  ErrorMessage: {0}", e.ToString());
        //                    }

        //                    cmdText = "SELECT count(Id) FROM AllDocs ad With(nolock) where ad.SiteId=@SiteId AND ad.WebId=@ParentWebId AND ad.ParentId=@ParentId AND ad.DeleteTransactionId = 0x AND ad.Type=1 AND ad.IsCurrentVersion=1 ";
        //                    connect.AddParameter("@ParentId", parentFolderId);
        //                    childrenCount = (int)connect.ExecuteScalar(cmdText);
        //                    startIndex = startIndex > childrenCount ? 0 : startIndex;

        //                    cmdText = string.Format("select * from(SELECT top {0} Id,LeafName,ListId,DoclibRowId,ScopeId ,ROW_NUMBER() over (ORDER BY LeafName) as RowNumber FROM AllDocs  With(nolock) where SiteId=@SiteId AND WebId=@ParentWebId AND ParentId=@ParentId AND DeleteTransactionId = 0x AND Type=1 AND IsCurrentVersion=1 ) as ad where ad.RowNumber > {1}", perPage + startIndex, startIndex); // WHERE SiteId=@SiteId AND 
        //                    //cmdText = "SELECT ad.Id,ad.LeafName,ad.ListId,ad.DoclibRowId,ad.ScopeId FROM AllDocs ad With(nolock) where ad.SiteId=@SiteId AND ad.WebId=@ParentWebId AND ad.ParentId=@ParentId AND ad.DeleteTransactionId = 0x AND ad.Type=1 AND ad.IsCurrentVersion=1 ORDER BY ad.LeafName"; // WHERE SiteId=@SiteId AND 

        //                    //mSqlConn.AddParameter("@ParentId", parentFolderId);

        //                    using (SqlDataReader dr = connect.ExecuteReader(cmdText))
        //                    {
        //                        try
        //                        {
        //                            while (dr.Read())
        //                            {
        //                                Guid listId = dr.IsDBNull(2) ? Guid.Empty : dr.GetGuid(2);
        //                                Guid uniqueId = dr.GetGuid(0);
        //                                string leafName = dr.GetString(1);
        //                                string serverRelativeUrl;
        //                                if (parentFolderServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    serverRelativeUrl = parentFolderServerRelativeUrl + leafName;
        //                                }
        //                                else
        //                                {
        //                                    serverRelativeUrl = parentFolderServerRelativeUrl + "/" + leafName; ;
        //                                }
        //                                bool Hidden = dr.IsDBNull(3) ? true : false;
        //                                Guid scopeId = dr.GetGuid(4);
        //                                string url = new Uri(new Uri(mSiteUrl), serverRelativeUrl).ToString();
        //                                bool hasUniqueRoleAssignments = !scopeId.Equals(parentFolderScopeId);
        //                                AveFolderBrowserInfo folder = new AveFolderBrowserInfo()
        //                                {
        //                                    UniqueId = uniqueId,
        //                                    Name = leafName,
        //                                    ServerRelativeUrl = serverRelativeUrl,
        //                                    Url = url,
        //                                    ParentListId = listId,
        //                                    ParentId = parentFolderId,
        //                                    Hidden = Hidden,
        //                                    //ListHasUniqueRoleAssignments = listHasUniqueRoleAssignment,
        //                                    HasUniqueRoleAssignments = hasUniqueRoleAssignments
        //                                };
        //                                subFolders.Add(folder);
        //                            }
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserFolderInfoError, e.ToString());
        //                        }
        //                    }
        //                    return subFolders;
        //                }
        //
        //            }
        //
        //        }

        //        public List<AveItemBrowserInfo> GetBrowserItems(Guid siteId, Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserItems"))
        //            {
        //
        //                List<AveItemBrowserInfo> items = new List<AveItemBrowserInfo>();
        //                int pagedCount = 0;
        //                int itemCount;
        //                Guid parentScopeId = Guid.Empty;
        //                Guid parentFolderListId = Guid.Empty;
        //                int listType = 0;
        //                GetParentFolderListId(siteId, webId, parentFolderUniqueId, ref parentFolderListId, ref parentScopeId, ref listType);
        //                itemCount = GetFolderItemsCount(siteId, webId, parentFolderListId, parentFolderUniqueId);
        //                var inputListType = listType == 1 ? "ad.leafName" : "ad.DoclibRowId";
        //                string cmdText = string.Empty;
        //                using (var connect = new AveSqlConnection(mConnectString))
        //                {
        //                    if (parentFolderListId == Guid.Empty)
        //                    {
        //                        connect.AddParameter("@ParentId", parentFolderUniqueId);
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = "SELECT ad.Id,ad.DirName,ad.LeafName,ad.ScopeId,w.FullUrl FROM AllDocs ad with(nolock) INNER JOIN AllWebs w with(nolock) ON ad.WebId=w.Id AND w.DeleteTransactionId=0x where ad.SiteId=@SiteId AND ad.ParentId=@ParentId AND ad.TYPE=0 AND ad.DeleteTransactionId=0x AND ad.IsCurrentVersion=1";
        //                        }
        //                        else
        //                        {
        //                            cmdText = "SELECT ad.Id,ad.DirName,ad.LeafName,ad.ScopeId,w.FullUrl FROM AllDocs ad with(nolock) INNER JOIN Webs w with(nolock) ON ad.WebId=w.Id where ad.SiteId=@SiteId AND ad.ParentId=@ParentId AND ad.TYPE=0 AND ad.DeleteTransactionId=0x AND ad.IsCurrentVersion=1";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        pagedCount = AveBrowserHelper.GetPagedCount(pageInfo);
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = string.Format(@"select * from(
        //SELECT top {0} ad.Id,ad.DirName,ad.LeafName,ad.DoclibRowId,ad.ScopeId,al.tp_BaseType,w.FullUrl,au.nvarchar1, au.tp_UIVersionString,au.tp_Editor,au.tp_Modified,au.tp_Level,
        //ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber
        //FROM AllDocs ad with(nolock) INNER JOIN AllLists al with(nolock) on al.tp_WebId=ad.WebId AND ad.ListId=al.tp_ID 
        //INNER JOIN AllWebs w with(nolock) on ad.WebId=w.Id AND w.DeleteTransactionId=0x 
        //INNER JOIN AllUserData au with(nolock) on ad.SiteId=au.tp_SiteId AND au.tp_DeleteTransactionId=0x AND au.tp_IsCurrentVersion=1  AND ad.ParentId=au.tp_ParentId AND ad.Id=au.tp_DocId AND ad.UIVersion=au.tp_UIVersion AND au.tp_RowOrdinal=0
        //WHERE ad.SiteId=@SiteId AND ad.DeleteTransactionId=0x AND ad.DirName=@DirName AND ad.DoclibRowId IS NOT NULL AND ad.TYPE=0 AND ad.IsCurrentVersion=1 
        //) tmp where tmp.RowNumber >{2}", perPage + pagedCount, inputListType, pagedCount);
        //                        }
        //                        else
        //                        {
        //                            cmdText = string.Format(@"select * from(
        //SELECT top {0} ad.Id,ad.DirName,ad.LeafName,ad.DoclibRowId,ad.ScopeId,al.tp_BaseType,w.FullUrl,au.nvarchar1, au.tp_UIVersionString,au.tp_Editor,au.tp_Modified,au.tp_Level,
        //ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber
        //FROM AllDocs ad with(nolock) INNER JOIN AllLists al with(nolock) on al.tp_WebId=ad.WebId AND ad.ListId=al.tp_ID 
        //INNER JOIN Webs w with(nolock) on ad.WebId=w.Id
        //INNER JOIN AllUserData au with(nolock) on ad.SiteId=au.tp_SiteId AND au.tp_DeleteTransactionId=0x AND au.tp_IsCurrentVersion=1  AND ad.ParentId=au.tp_ParentId AND ad.Id=au.tp_DocId AND ad.UIVersion=au.tp_UIVersion AND au.tp_RowOrdinal=0
        //WHERE ad.SiteId=@SiteId AND ad.DeleteTransactionId=0x AND ad.DirName=@DirName AND ad.DoclibRowId IS NOT NULL AND ad.TYPE=0 AND ad.IsCurrentVersion=1 
        //) tmp where tmp.RowNumber >{2}", perPage + pagedCount, inputListType, pagedCount);
        //                        }
        //                    }

        //                    connect.AddParameter("@SiteId", siteId); //If it doesn't need site id, please uncomment this line. 
        //                    connect.AddParameter("@DirName", parentFolderServerRelativeUrl.TrimStart('/'));
        //                    try
        //                    {
        //                        using (SqlDataReader dr = connect.ExecuteReader(cmdText))
        //                        {
        //                            try
        //                            {
        //                                while (dr.Read())
        //                                {
        //                                    AveItemBrowserInfo itemInfo = null;
        //                                    if (parentFolderListId == Guid.Empty)
        //                                    {
        //                                        Guid uniqueId = dr.GetGuid(0);
        //                                        string dirName = dr.GetString(1);
        //                                        string leafName = dr.GetString(2);
        //                                        Guid scopeId = dr.GetGuid(3);
        //                                        string webServerRelativeUrl = dr.GetString(4);
        //                                        string url = dirName.Substring(webServerRelativeUrl.Length + 1) + "/" + leafName;
        //                                        bool hasUniqueRoleAssignments = !scopeId.Equals(parentScopeId);
        //                                        itemInfo = new AveItemBrowserInfo
        //                                        {
        //                                            UniqueId = uniqueId,
        //                                            Name = leafName,
        //                                            Url = url,
        //                                            ParentFolderUniqueID = parentFolderUniqueId,
        //                                            ParentListID = Guid.Empty,
        //                                            HasUniqueRoleAssignments = hasUniqueRoleAssignments
        //                                        };
        //                                    }
        //                                    else
        //                                    {
        //                                        Guid uniqueId = dr.GetGuid(0);
        //                                        string dirName = dr.GetString(1);
        //                                        int listBaseType = dr.GetInt32(5);
        //                                        string name = string.Empty;
        //                                        string displayName = string.Empty;
        //                                        if (listBaseType == 1)//DocumentLibrary
        //                                        {
        //                                            displayName = name = dr.GetString(2);
        //                                            int index = displayName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
        //                                            if (index >= 0)
        //                                            {
        //                                                displayName = displayName.Substring(0, index);
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            name = displayName = dr.IsDBNull(7) ? string.Empty : dr.GetString(7);
        //                                        }
        //                                        Guid scopeId = dr.GetGuid(4);
        //                                        int docLibRowId = dr.GetInt32(3);
        //                                        string webServerRelativeUrl = dr.GetString(6);
        //                                        string url = dirName.Substring(webServerRelativeUrl.Length + 1) + "/" + name;
        //                                        bool hasUniqueRoleAssignments = !scopeId.Equals(parentScopeId);
        //                                        string currentUIVersionString = dr.GetString(8);
        //                                        int lastModifier = dr.GetInt32(9);
        //                                        DateTime lastModifyTime = dr.GetDateTime(10);
        //                                        byte level = dr.GetByte(11);
        //                                        itemInfo = new AveItemBrowserInfo
        //                                        {
        //                                            UniqueId = uniqueId,
        //                                            Name = name,
        //                                            DisplayName = displayName,
        //                                            ID = docLibRowId,
        //                                            ListBaseType = listBaseType,
        //                                            Url = url,
        //                                            ParentFolderUniqueID = parentFolderUniqueId,
        //                                            HasUniqueRoleAssignments = hasUniqueRoleAssignments,
        //                                            ParentListID = parentFolderListId,
        //                                            CurrentUIVersionString = currentUIVersionString,
        //                                            LastModifier = lastModifier,
        //                                            LastModifyTime = lastModifyTime,
        //                                            Level = level
        //                                        };
        //                                    }
        //                                    items.Add(itemInfo);
        //                                }
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetBrowserItems.  Error Message: {0}", e.ToString());
        //                            }
        //                        }
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetBrowserItems.  Error Message: {0}", e.ToString());

        //                    }
        //                    foreach (var item in items)
        //                    {
        //                        cmdText = @"select tp_UIVersionString,tp_Level from AllUserData where tp_SiteId=@tp_SiteId and tp_DeleteTransactionId=0x and (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) and tp_ParentId=@tp_ParentId
        //                               and tp_DocId=@tp_DocId  and tp_RowOrdinal=0 and tp_IsCurrent=0 order by AllUserData.tp_UIVersionString desc";
        //                        connect.ClearParameters();
        //                        connect.AddParameter("@tp_ParentId", parentFolderUniqueId);
        //                        connect.AddParameter("@tp_SiteId", siteId); //If it doesn't need site id, please uncomment this line. 
        //                        connect.AddParameter("@tp_DocId", item.UniqueId); //If it doesn't need doc id, please uncomment this line. 
        //                        try
        //                        {
        //                            using (SqlDataReader dr = connect.ExecuteReader(cmdText))
        //                            {
        //                                while (dr.Read())
        //                                {
        //                                    item.Versions[dr.GetString(0)] = dr.GetByte(1); ;
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            logger.Warn(string.Format("An error occurred while to get Item versions information. Exception: {0}", ex.ToString()));
        //                        }
        //                    }
        //                }
        //                if (itemCount - pagedCount <= perPage)
        //                {
        //                    pageInfo = string.Empty;
        //                }
        //                else
        //                {
        //                    pageInfo = listType == 1 ? string.Format("Paged=TRUE&p_SortBehavior=0&p_FileLeafRef={0}&RootFolder={1}&StartIndex={2}", items[items.Count - 1].Name, parentFolderServerRelativeUrl, pagedCount + perPage) :
        //                                               string.Format("Paged=TRUE&p_SortBehavior=0&p_ID={0}&RootFolder={1}&StartIndex={2}", items[items.Count - 1].ID, parentFolderServerRelativeUrl, pagedCount + perPage);
        //                }
        //                return items;

        //
        //            }
        //
        //        }

        //        public List<AveItemVersionBrowserInfo> GetBrowserItemVersions(Guid siteId, string webServerRelativeUrl, string listTitle, Guid parentFolderUniqueId, Guid itemUniqueId, int startIndex, uint perPage, ref int childrenCount)
        //        {
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserItemVersions"))
        //            {
        //                List<AveItemVersionBrowserInfo> itemVersions = new List<AveItemVersionBrowserInfo>();
        //                string cmdText = @"select COUNT(*) from( select UIVersion from AllDocs as ad where ad.SiteId = @SiteId and ad.DeleteTransactionId = 0x and ad.ParentId = @parentId and ad.Id = @docId union select UIVersion from AllDocVersions as adv where adv.SiteId = @SiteId and adv.Id = @docId and adv.DeleteTransactionId = 0x ) as itemversion";
        //                using (var connect = new AveSqlConnection(mConnectString))
        //                {
        //                    connect.AddParameter("@SiteId", siteId);
        //                    connect.AddParameter("@parentId", parentFolderUniqueId);
        //                    connect.AddParameter("@docId", itemUniqueId);
        //                    childrenCount = (int)connect.ExecuteScalar(cmdText);
        //                    startIndex = startIndex > childrenCount ? 0 : startIndex;
        //                    cmdText = string.Format(@"select top {0} * from(select nvarchar1,tp_UIVersionString,ROW_NUMBER() over (ORDER BY tp_UIVersionString) as RowNumber from ( select au.nvarchar1,au.tp_UIVersionString from AllDocs as ad inner join alluserdata au with(nolock) on  ad.SiteId=au.tp_SiteId AND ad.ParentId=au.tp_ParentId AND ad.Id=au.tp_DocId AND au.tp_UIVersion=ad.UIVersion where ad.SiteId = @SiteId and ad.ParentId = @parentId and ad.Id = @docId and (au.tp_IsCurrentVersion=1 or au.tp_IsCurrentVersion=0) and au.tp_DeleteTransactionId = 0x and ad.DeleteTransactionId = 0x
        //union
        //select au.nvarchar1,au.tp_UIVersionString from AllDocVersions as adv
        //inner join alluserdata au with(nolock) on  adv.SiteId=au.tp_SiteId AND adv.Id=au.tp_DocId AND au.tp_UIVersion=adv.UIVersion 
        //where adv.SiteId = @SiteId and  adv.Id = @docId and au.tp_IsCurrentVersion = 0 and au.tp_DeleteTransactionId = 0x and adv.DeleteTransactionId = 0x ) as itemVersion
        //) as itemVersion
        //where RowNumber > {1}", perPage + startIndex, startIndex);

        //                    using (SqlDataReader dr = connect.ExecuteReader(cmdText))
        //                    {
        //                        try
        //                        {
        //                            while (dr.Read())
        //                            {
        //                                AveItemVersionBrowserInfo itemInfo = null;
        //                                string displayName = dr.GetString(0);
        //                                string version = dr.GetString(1);
        //                                itemInfo = new AveItemVersionBrowserInfo
        //                                {
        //                                    ItemID = itemUniqueId,
        //                                    VersionLabel = version,
        //                                    ItemUniqueID = itemUniqueId
        //                                };
        //                                itemVersions.Add(itemInfo);
        //                            }
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserItemVersionInfoError, e.ToString());
        //                        }
        //                    }
        //                }
        //                return itemVersions;
        //            }
        //        }

        //        public AveWebBrowserInfo GetBrowserRootWeb(Guid siteId)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserRootWeb"))
        //            {
        //
        //                AveWebBrowserInfo webBrowserInfo = null;
        //                SPWebTemplateCollection webTemplates;

        //                try
        //                {
        //                    using (var connect = new AveSqlConnection(mConnectString))
        //                    {
        //                        string platformVersion = GetSitePlatformVersion(siteId, connect);
        //                        string cmdText = string.Format("SELECT Id,FullUrl,Title,Language,WebTemplate,ProvisionConfig,FirstUniqueAncestorWebId FROM Webs WHERE SiteId=@SiteId AND ParentWebId IS NULL ORDER BY FullUrl");
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
        //                        }
        //                        connect.AddParameter("@SiteId", siteId);
        //                        using (SqlDataReader sr = connect.ExecuteReader(cmdText))
        //                        {
        //                            while (sr.Read())
        //                            {
        //                                try
        //                                {
        //                                    Guid id = sr.GetGuid(0);
        //                                    string name = sr.GetString(1);
        //                                    string title = null;
        //                                    int pos = name.LastIndexOf('/');
        //                                    if (pos >= 0)
        //                                    {
        //                                        name = name.Substring(pos + 1);
        //                                    }

        //                                    if (!sr.IsDBNull(2))
        //                                    {
        //                                        title = sr.GetString(2);
        //                                    }
        //                                    string url = sr.GetString(1);
        //                                    if (!string.IsNullOrEmpty(url) && !url.StartsWith("/", StringComparison.Ordinal))
        //                                    {
        //                                        url = "/" + url;
        //                                    }
        //                                    string fullUrl = new Uri(new Uri(mSiteUrl), url).ToString().TrimEnd('/');
        //                                    uint language = (uint)sr.GetInt32(3);
        //                                    int templateId = sr.GetInt32(4);
        //                                    int provisionConfig = sr.GetInt16(5);
        //                                    bool hasUniqueRoleAssignments = (sr.GetGuid(0) == sr.GetGuid(6)) ? true : false;

        //                                    webTemplates = GetWebTemplatesFromCache(siteId, language, platformVersion);
        //                                    string templateTitle = string.Empty;
        //                                    string templateName = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates, ref templateTitle);
        //                                    //bool isRootWeb = true;

        //                                    webBrowserInfo = new AveWebBrowserInfo()
        //                                    {
        //                                        ID = id,
        //                                        Name = name,
        //                                        Url = fullUrl,
        //                                        Title = title,
        //                                        Language = language,
        //                                        IsRootWeb = true,
        //                                        TemplateName = templateName,
        //                                        TemplateTitle = templateTitle,
        //                                        HasUniqueRoleAssignments = hasUniqueRoleAssignments
        //                                    };
        //                                }
        //                                catch (Exception e)
        //                                {
        //                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserRootWebInfoFromContentDBError, e.ToString());
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserRootWebInfoError, e.ToString());
        //                }
        //                return webBrowserInfo;
        //
        //            }
        //
        //        }

        //        public AveFolderBrowserInfo GetBrowserRootFolder(Guid siteId, Guid parentWebId, Guid parentListId)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserRootFolder"))
        //            {
        //
        //                AveFolderBrowserInfo folderBrowserInfo = null;
        //                bool isWebFolder = parentListId == Guid.Empty;
        //                string cmdTextString = string.Empty;
        //                try
        //                {
        //                    using (var connect = new AveSqlConnection(mConnectString))
        //                    {
        //                        StringBuilder cmdText = new StringBuilder();
        //                        if (AveSPUtility.IsSP1DBSchema(connect))
        //                        {
        //                            cmdText.Append(string.Format("SELECT ad.Id,ad.DirName,ad.LeafName,ad.ListId,ad.DoclibRowId,ad.ParentId,ad.ScopeId,w.FullUrl,w.ScopeId {0} FROM (AllDocs ad With(nolock) inner join AllWebs w with(nolock) on w.SiteId=@siteId AND ad.WebId=w.Id AND w.DeleteTransactionId=0x) ", isWebFolder ? string.Empty : ",al.tp_ScopeId"));
        //                        }
        //                        else
        //                        {
        //                            cmdText.Append(string.Format("SELECT ad.Id,ad.DirName,ad.LeafName,ad.ListId,ad.DoclibRowId,ad.ParentId,ad.ScopeId,w.FullUrl,w.ScopeId {0} FROM (AllDocs ad With(nolock) inner join Webs w with(nolock) on w.SiteId=@siteId AND ad.WebId=w.Id) ", isWebFolder ? string.Empty : ",al.tp_ScopeId"));
        //                        }
        //                        if (isWebFolder)
        //                        {
        //                            cmdText.Append("where ad.ListId IS NULL AND (w.FullUrl =ad.DirName+'/'+ad.LeafName AND LEN(ad.DirName)<>0 or LEN(ad.DirName)=0 AND w.FullUrl=ad.LeafName) AND ad.SiteId=@siteId AND ad.WebId=@webId");
        //                        }
        //                        else
        //                        {
        //                            connect.AddParameter("@ListId", parentListId);
        //                            cmdText.Append("inner join AllLists al with(nolock) on ad.ListId=@listId AND ad.ListId= al.tp_ID AND ad.Id=tp_RootFolder where ad.SiteId=@siteId AND ad.WebId=@webId AND ad.DeleteTransactionId=0x AND Type=1 AND IsCurrentVersion=1");
        //                        }
        //                        connect.AddParameter("@siteId", siteId);
        //                        connect.AddParameter("@webId", parentWebId);
        //                        using (SqlDataReader dr = connect.ExecuteReader(cmdText.ToString()))
        //                        {
        //                            if (dr.Read())
        //                            {
        //                                Guid uniqueId = dr.GetGuid(0);
        //                                string dirName = dr.GetString(1);
        //                                string leafName = dr.GetString(2);
        //                                string serverRelativeUrl = dirName + "/" + leafName;
        //                                if (!serverRelativeUrl.StartsWith("/", StringComparison.Ordinal))
        //                                {
        //                                    serverRelativeUrl = "/" + serverRelativeUrl;
        //                                }
        //                                Guid listId = dr.IsDBNull(3) ? Guid.Empty : dr.GetGuid(3);
        //                                bool Hidden = dr.IsDBNull(4) ? true : false;
        //                                Guid parentId = dr.GetGuid(5);
        //                                string webServerRelativeUrl = dr.GetString(7);
        //                                if (!webServerRelativeUrl.StartsWith("/", StringComparison.Ordinal))
        //                                {
        //                                    webServerRelativeUrl = "/" + webServerRelativeUrl;
        //                                }
        //                                string parentWebUrl = new Uri(new Uri(mSiteUrl), webServerRelativeUrl).ToString();
        //                                bool listHasUniqueRoleAssignment = isWebFolder ? true : dr.GetGuid(8).Equals(dr.GetGuid(9)) ? false : true;
        //                                bool itemHasUniqueRoleAssignments = isWebFolder ? true : dr.GetGuid(6).Equals(dr.GetGuid(9)) ? false : true;
        //                                folderBrowserInfo = new AveFolderBrowserInfo()
        //                                {
        //                                    UniqueId = uniqueId,
        //                                    Name = leafName,
        //                                    ServerRelativeUrl = serverRelativeUrl,
        //                                    Url = parentWebUrl,
        //                                    ParentListId = listId,
        //                                    ParentId = listId,
        //                                    Hidden = Hidden,
        //                                    HasUniqueRoleAssignments = listHasUniqueRoleAssignment
        //                                };
        //                            }
        //                        }
        //                    }

        //                }
        //                catch (Exception e)
        //                {
        //                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserRootFolderInfoError, e.ToString());
        //                }
        //                return folderBrowserInfo;
        //
        //            }
        //
        //        }

        private SPWebTemplateCollection GetWebTemplates(Guid siteId, uint LCID, int platformVersion)
        {
            try
            {
                using (var site = new SPSite(siteId))
                {
                    return site.GetWebTemplates(LCID, platformVersion);
                }
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred when get web templates, error: {0}", e);
                using (var site = new SPSite(siteId, SPUserToken.SystemAccount))
                {
                    return site.GetWebTemplates(LCID, platformVersion);
                }
            }
        }

        private string WebTemplateIdName(int id, string configuration, SPWebTemplateCollection webTemplates, ref string templateTitle)
        {
            string webTemplateStr = null;
            string sConfig = "#" + configuration;
            foreach (SPWebTemplate sWebTemplate in webTemplates)
            {
                if (sWebTemplate.ID == id && sWebTemplate.Name.EndsWith(sConfig, StringComparison.OrdinalIgnoreCase))
                {
                    webTemplateStr = sWebTemplate.Name;
                    templateTitle = sWebTemplate.Title;
                    break;
                }
            }
            return webTemplateStr;
        }

        private void GetParentFolderListId(Guid siteId, Guid webId, Guid Id, ref Guid listId, ref Guid parentScopeId, ref int listType)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetParentFolderListId"))
            {

                try
                {
                    using (var connect = new AveSqlConnection(mConnectString))
                    {
                        string cmdText = @"
SELECT ad.ListId,ad.ScopeId,al.tp_BaseType FROM AllDocs as ad 
INNER JOIN AllLists al with(nolock) on al.tp_WebId=ad.WebId AND ad.ListId=al.tp_ID 
where SiteId=@SiteId AND WebId=@WebId AND Id=@Id";
                        //cmdText.Append("ad.SiteId=@siteId AND ad.WebId=@webId AND ad.DeleteTransaction=0x AND (Type=1 OR Type=2) AND IsCurrentVersion=1");
                        connect.AddParameter("@SiteId", siteId);
                        connect.AddParameter("@WebId", webId);
                        connect.AddParameter("@Id", Id);
                        using (SqlDataReader dr = connect.ExecuteReader(cmdText.ToString()))
                        {
                            if (dr.Read())
                            {
                                listId = dr.IsDBNull(0) ? Guid.Empty : dr.GetGuid(0);
                                parentScopeId = dr.GetGuid(1);
                                listType = dr.GetInt32(2);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetParentFolderListId. Error Message: {0}", e.ToString());
                }

            }

        }

        private int GetFolderItemsCount(Guid siteId, Guid webId, Guid parentFolderListId, Guid parentFolderUniqueId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetFolderItemsCount"))
            {

                string cmdText = string.Empty;
                int itemCount = 0;
                using (var connect = new AveSqlConnection(mConnectString))
                {
                    if (parentFolderListId == Guid.Empty)
                    {
                        cmdText = "SELECT count(Id) FROM AllDocs with(nolock) where SiteId=@SiteId AND WebId=@ParentWebId AND ParentId=@ParentId AND TYPE=0 AND DeleteTransactionId=0x AND IsCurrentVersion=1";
                    }
                    else
                    {
                        cmdText = "SELECT count(Id) FROM AllDocs ad with(nolock) where SiteId=@SiteId AND WebId=@ParentWebId AND ListId=@ListId AND ParentId=@ParentId AND DoclibRowId IS NOT NULL AND TYPE=0 AND DeleteTransactionId=0x AND IsCurrentVersion=1 ";
                        connect.AddParameter("@ListId", parentFolderListId);
                    }
                    try
                    {
                        connect.AddParameter("@SiteId", siteId);
                        connect.AddParameter("@ParentWebId", webId);
                        connect.AddParameter("@ParentId", parentFolderUniqueId);
                        using (SqlDataReader dr = connect.ExecuteReader(cmdText))
                        {
                            if (dr.Read())
                            {
                                itemCount = dr.GetInt32(0);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "Error occurred while access data from GetFolderItemsCount.  ErrorMessage: {0}", e.ToString());
                    }
                }

                return itemCount;

            }

        }

        public string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserQueryConnectionString"))
            {

                Guid ContentDBID = Guid.Empty;
                string sqlConnString = string.Empty;
                string path = string.Empty;
                SPWebApplication spWebApp = SPWebApplication.Lookup(new Uri(siteUrl));
                try
                {
                    if (spWebApp == null)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindWebApplication, siteUrl);
                    }

                    string webAppUrl = spWebApp.GetResponseUri(SPUrlZone.Default).ToString();
                    if (!webAppUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        webAppUrl += "/";
                    }
                    if (!siteUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        siteUrl += "/";
                    }
                    if (siteUrl.StartsWith(webAppUrl, StringComparison.OrdinalIgnoreCase))//数据库存储了site的相对路径
                    {
                        path = siteUrl.TrimEnd('/').Substring(webAppUrl.Length - 1);
                        if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            path += "/";
                        }
                    }
                    else//site is a hostheader sitecollection,数据库存储形式为Hostheader:port:/managedPath/sitename
                    {
                        int index = siteUrl.IndexOf("//", StringComparison.OrdinalIgnoreCase);
                        path = siteUrl.TrimEnd('/').Substring(index + 2);
                    }
                    SPDatabase configDb = (SPDatabase)Invoker.GetProperty(spWebApp, "ConfigurationDatabase");
                    if (configDb != null)
                    {
                        using (AveSqlConnection sqlConn_ConfigDB = new AveSqlConnection())
                        {
                            sqlConn_ConfigDB.Open(configDb.DatabaseConnectionString);
                            sqlConn_ConfigDB.Command.Parameters.AddWithValue("@ApplicationId", spWebApp.Id);
                            sqlConn_ConfigDB.Command.Parameters.AddWithValue("@Path", path);
                            string cmdText_ConfigDB = "SELECT DatabaseId,Id FROM SiteMap With(NoLock) WHERE ApplicationId=@ApplicationId AND Path=@Path";
                            if (AveSPUtility.IsSP1DBSchema(sqlConn_ConfigDB, "DeleteTransactionId", "SiteMap"))
                            {
                                cmdText_ConfigDB = "SELECT DatabaseId,Id FROM SiteMap With(NoLock) WHERE DeleteTransactionId=0x AND ApplicationId=@ApplicationId AND Path=@Path";
                            }
                            using (SqlDataReader sqlReader_ConfigDB = sqlConn_ConfigDB.ExecuteReader(cmdText_ConfigDB))
                            {
                                if (sqlReader_ConfigDB.Read())
                                {
                                    ContentDBID = sqlReader_ConfigDB.GetGuid(0);
                                    siteId = sqlReader_ConfigDB.GetGuid(1);
                                    logger.Log(AveLogLevel.INFO, "Find ContentDB for Site {0} successfully, ContentDB Id is: {1}", siteUrl, ContentDBID);
                                }
                            }
                        }
                    }

                    foreach (SPContentDatabase contentDatabase in spWebApp.ContentDatabases)
                    {
                        if (contentDatabase.Id == ContentDBID)
                        {
                            sqlConnString = contentDatabase.DatabaseConnectionString;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetBrowserDBConnectionString. Error Message: {0}", e.ToString());
                }
                return sqlConnString;

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "distinguishedame is a key")]
        public bool IsContainedInGroupMembers(string DomainName, string GroupName, string currentUsername)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.Import"))
            {

                try
                {
                    DirectoryEntry Entry = new DirectoryEntry("LDAP://" + DomainName);
                    DirectorySearcher Search = new DirectorySearcher(Entry);
                    Search.Filter = "(&(objectCategory=group)(cn=" + GroupName + "))";
                    Search.PropertiesToLoad.Add("distinguishedname");

                    SearchResult Result = Search.FindOne();

                    if (Result != null)
                    {
                        DirectoryEntry Group = new DirectoryEntry("LDAP://" + Result.Properties["distinguishedname"][0].ToString());
                        string loginName = string.Empty;
                        string active = string.Empty;
                        foreach (object Dn in Group.Properties["member"])
                        {
                            DirectoryEntry member = new DirectoryEntry("LDAP://" + Dn.ToString());
                            loginName = string.Format("{0}\\{1}", DomainName, member.Properties["samaccountname"].Value.ToString());
                            if (loginName.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("An Error occurred while getting group members, Error Message : {0}", ex.ToString());
                }
                return false;

            }

            //return members;
        }



        public List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserWebs"))
            {

                List<AveWebBrowserInfo> webBrowserInfos = new List<AveWebBrowserInfo>();
                SPWebTemplateCollection webTemplates = null;
                try
                {
                    using (var connect = new AveSqlConnection(mConnectString))
                    {
                        string platformVersion = GetSitePlatformVersion(option.ParentSiteId, connect);
                        connect.AddParameter("@siteId", option.ParentSiteId);
                        connect.AddParameter("@ParentWebId", option.ParentWebId);
                        string strFilterAppWeb = string.Empty;
                        if (option.FilterAppWeb)
                        {
                            strFilterAppWeb = "AND AppInstanceId=@AppInstanceId";
                            connect.AddParameter("@AppInstanceId", Guid.Empty);
                        }

                        string cmdText = "SELECT FullUrl FROM Webs WHERE SiteId=@siteId AND Id=@ParentWebId";
                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
                        }
                        string parentUrl = connect.ExecuteScalar(cmdText) as string;

                        cmdText = string.Format("SELECT count(Id) FROM Webs WHERE SiteId=@siteId AND ParentWebId=@ParentWebId {0} ", strFilterAppWeb);
                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
                        }
                        option.ChildrenTotalCount = (int)connect.ExecuteScalar(cmdText);

                        //if (option.NeedPaging) { } else { }
                        int index = option.StartIndex > option.ChildrenTotalCount ? 0 : option.StartIndex;

                        cmdText = string.Format(@"SELECT * FROM (
SELECT top {0} Id,FullUrl,Title,Language,WebTemplate,ProvisionConfig,FirstUniqueAncestorWebId,AppInstanceId,ROW_NUMBER() OVER (ORDER BY FullUrl) AS RowNumber
FROM Webs 
WHERE SiteId=@siteId AND ParentWebId=@ParentWebId {1}) As W
WHERE W.RowNumber > {2} ", option.PerPage + index, strFilterAppWeb, index);

                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
                        }
                        using (SqlDataReader sr = connect.ExecuteReader(cmdText))
                        {
                            while (sr.Read())
                            {
                                try
                                {
                                    Guid id = sr.GetGuid(0);
                                    string name = sr.GetString(1);
                                    string title = null;
                                    int pos = -1;
                                    if (parentUrl != null && name.StartsWith(parentUrl, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (parentUrl.Length != 0)
                                        {
                                            pos = parentUrl.Length;
                                        }
                                    }
                                    else
                                    {
                                        pos = name.LastIndexOf('/');
                                    }
                                    if (pos >= 0)
                                    {
                                        name = name.Substring(pos + 1);
                                    }

                                    if (!sr.IsDBNull(2))
                                    {
                                        title = sr.GetString(2);
                                    }
                                    string webUrl = sr.GetString(1);
                                    if (!webUrl.StartsWith("/", StringComparison.Ordinal))
                                    {
                                        webUrl = "/" + webUrl;
                                    }
                                    string fullUrl = new Uri(new Uri(mSiteUrl), webUrl).ToString();
                                    uint language = (uint)sr.GetInt32(3);
                                    //bool isRootWeb = false;
                                    bool hasUniqueRoleAssignments = (sr.GetGuid(0) == sr.GetGuid(6)) ? true : false;
                                    int templateId = sr.GetInt32(4);
                                    int provisionConfig = sr.GetInt16(5);
                                    webTemplates = GetWebTemplatesFromCache(option.ParentSiteId, language, platformVersion);
                                    string templateTitle = string.Empty;
                                    string templateName = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates, ref templateTitle);
                                    AveWebBrowserInfo webBrowserInfo = new AveWebBrowserInfo()
                                    {
                                        ID = id,
                                        Name = name,
                                        Url = fullUrl,
                                        Title = title,
                                        Language = language,
                                        IsRootWeb = false,
                                        HasUniqueRoleAssignments = hasUniqueRoleAssignments,
                                        TemplateName = templateName,
                                        TemplateTitle = templateTitle,
                                        IsAppWeb = !sr.GetGuid(7).Equals(Guid.Empty)
                                    };

                                    webBrowserInfos.Add(webBrowserInfo);
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserWebInfoFromContentDBError, e.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserWebInfoError, e.ToString());
                }
                finally
                {
                }
                return webBrowserInfos;

            }

        }

        public List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserLists"))
            {

                List<AveListBrowserInfo> listBrowserInfos = new List<AveListBrowserInfo>();
                //bool isMyProfileList = false;

                try
                {
                    using (var connect = new AveSqlConnection(mConnectString))
                    {
                        connect.AddParameter("@SiteId", option.ParentSiteId);
                        connect.AddParameter("@WebId", option.ParentWebId);
                        string cmdText = "SELECT count(tp_Id) FROM AllLists WHERE tp_SiteId = @SiteId AND tp_WebId=@WebId and tp_DeleteTransactionId = 0x";
                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = "SELECT count(tp_Id) FROM AllLists WHERE tp_SiteId = @SiteId AND  tp_WebId=@WebId and tp_DeleteTransactionId = 0x";
                        }
                        option.ChildrenTotalCount = (int)connect.ExecuteScalar(cmdText);
                        option.ChildrenTotalCount++; //add for {system folder} list
                        int index = option.StartIndex - 1; //for {system folder} list,startIndex需要减一

                        cmdText = string.Format(@"SELECT * FROM(
SELECT top {0} al.tp_ID, al.tp_Title, al.tp_BaseType, al.tp_ServerTemplate, ad.DirName, ad.LeafName, al.tp_Flags,al.tp_ScopeId,w.ScopeId,ROW_NUMBER() OVER (ORDER BY al.tp_Title) AS RowNumber 
FROM AllLists al with(nolock) 
INNER JOIN AllDocs ad with(nolock) ON al.tp_WebId=@WebId AND al.tp_DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND ad.Level=1 AND ad.DeleteTransactionId=0x AND ad.SiteId = al.tp_SiteId  
INNER JOIN Webs w with(nolock) ON al.tp_SiteId = w.SiteId AND al.tp_WebId=w.Id 
WHERE al.tp_SiteId = @SiteId AND al.tp_WebId=@WebId) AS temp
WHERE temp.RowNumber > {1}", option.PerPage + index, index);

                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = string.Format(@"SELECT * FROM(
SELECT top {0} al.tp_ID, al.tp_Title, al.tp_BaseType, al.tp_ServerTemplate, ad.DirName, ad.LeafName, al.tp_Flags,al.tp_ScopeId,w.ScopeId,ROW_NUMBER() OVER (ORDER BY al.tp_Title) AS RowNumber
FROM AllLists al with(nolock) 
INNER JOIN AllDocs ad with(nolock) ON al.tp_DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND ad.Level=1 AND ad.DeleteTransactionId=0x AND ad.SiteId = al.tp_SiteId  
INNER JOIN AllWebs w with(nolock) ON al.tp_SiteId = w.SiteId AND al.tp_WebId=w.Id AND w.DeleteTransactionId=0x 
WHERE al.tp_SiteId = @SiteId AND al.tp_WebId=@WebId) AS temp
WHERE temp.RowNumber > {1}", option.PerPage + index, index);
                        }
                        using (SqlDataReader sr = connect.ExecuteReader(cmdText))
                        {
                            while (sr.Read())
                            {
                                try
                                {
                                    Guid id = sr.GetGuid(0);
                                    string title = sr.GetString(1);
                                    int baseType = sr.GetInt32(2);
                                    int serverTemplate = sr.GetInt32(3);
                                    var rootFolderName = sr.GetString(5);
                                    string dirName = sr.GetString(4);
                                    string serverRelativeUrl = string.IsNullOrEmpty(dirName) ? "/" + rootFolderName : "/" + dirName + "/" + rootFolderName;//root site 下的某些list的dirname为null
                                    bool hidden = (sr.GetInt64(6) & ((long)0x100L)) != 0L;
                                    string url = new Uri(new Uri(mSiteUrl), serverRelativeUrl).ToString();
                                    bool hasUniqueRoleAssignments = sr.GetGuid(7).Equals(sr.GetGuid(8)) ? false : true;
                                    bool enableFolderCreation = (sr.GetInt64(6) & 0x20000000) == 0;
                                    AveListBrowserInfo listDto = new AveListBrowserInfo()
                                    {
                                        ID = id,
                                        BaseType = baseType,
                                        BaseTemplate = serverTemplate,
                                        ServerRelativeUrl = serverRelativeUrl,
                                        Title = title,
                                        Hidden = hidden,
                                        Url = url,
                                        HasUniqueRoleAssignments = hasUniqueRoleAssignments,
                                        rootFolderName = rootFolderName,
                                        EnableFolderCreation = enableFolderCreation
                                    };

                                    listBrowserInfos.Add(listDto);
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserListInfoFromContentDBError, e.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserListInfoError, e.ToString());
                }
                if (option.StartIndex == 0)
                {
                    listBrowserInfos.Add(new AveListBrowserInfo()
                    {
                        ID = Guid.Empty,
                        Name = "{System Folder}",
                        Title = "{System Folder}",
                        rootFolderName = "Root Folder",
                        BaseType = 1,
                        //这次返回的可能为perPage+1个
                    });
                }
                return listBrowserInfos;

            }

        }

        public List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserSubFolders"))
            {

                List<AveFolderBrowserInfo> subFolders = new List<AveFolderBrowserInfo>();
                Guid parentFolderScopeId = Guid.Empty;

                string cmdText = "SELECT ScopeId from AllDocs where SiteId=@SiteId AND WebId=@ParentWebId AND Id=@Id AND DeleteTransactionId = 0x AND (Type=1 OR Type=2) AND IsCurrentVersion=1";
                using (var connect = new AveSqlConnection(mConnectString))
                {
                    connect.ClearParameters();
                    connect.AddParameter("@SiteId", option.ParentSiteId); //If it doesn't need site id, please uncomment this line. 
                    connect.AddParameter("@ParentWebId", option.ParentWebId);
                    connect.AddParameter("@Id", option.ParentFolderId);
                    string strFilterSystemFolder = string.Empty;
                    if (option.FilterSystemFolder)
                    {
                        if (option.ParentListId == Guid.Empty)
                        {
                            strFilterSystemFolder = "AND ListId IS NULL";

                        }
                        else
                        {
                            strFilterSystemFolder = "AND DoclibRowId IS NOT NULL";
                        }
                    }

                    try
                    {
                        parentFolderScopeId = (Guid)connect.ExecuteScalar(cmdText);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "Error occurred while GetParentFolder ScopeId from GetBrowserSubFolders.  ErrorMessage: {0}", e.ToString());
                    }

                    cmdText = string.Format("SELECT count(Id) FROM AllDocs  With(nolock) where SiteId=@SiteId AND WebId=@ParentWebId AND ParentId=@ParentId AND DeleteTransactionId = 0x AND Type=1 AND IsCurrentVersion=1 {0} ", strFilterSystemFolder);
                    connect.AddParameter("@ParentId", option.ParentFolderId);
                    option.ChildrenTotalCount = (int)connect.ExecuteScalar(cmdText);


                    int index = option.StartIndex > option.ChildrenTotalCount ? 0 : option.StartIndex;

                    cmdText = string.Format("select * from(SELECT top {0} Id,LeafName,ListId,DoclibRowId,ScopeId ,ROW_NUMBER() over (ORDER BY LeafName) as RowNumber FROM AllDocs  With(nolock) where SiteId=@SiteId AND WebId=@ParentWebId AND ParentId=@ParentId AND DeleteTransactionId = 0x AND Type=1 AND IsCurrentVersion=1 {1}) as ad where ad.RowNumber > {2}", option.PerPage + index, strFilterSystemFolder, index); // WHERE SiteId=@SiteId AND 
                    //cmdText = "SELECT ad.Id,ad.LeafName,ad.ListId,ad.DoclibRowId,ad.ScopeId FROM AllDocs ad With(nolock) where ad.SiteId=@SiteId AND ad.WebId=@ParentWebId AND ad.ParentId=@ParentId AND ad.DeleteTransactionId = 0x AND ad.Type=1 AND ad.IsCurrentVersion=1 ORDER BY ad.LeafName"; // WHERE SiteId=@SiteId AND 

                    //mSqlConn.AddParameter("@ParentId", parentFolderId);

                    using (SqlDataReader dr = connect.ExecuteReader(cmdText))
                    {
                        try
                        {
                            while (dr.Read())
                            {
                                Guid listId = dr.IsDBNull(2) ? Guid.Empty : dr.GetGuid(2);
                                Guid uniqueId = dr.GetGuid(0);
                                string leafName = dr.GetString(1);
                                //string serverRelativeUrl;
                                //if (option.ParentFolderServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                                //{
                                //    serverRelativeUrl = option.ParentFolderServerRelativeUrl + leafName;
                                //}
                                //else
                                //{
                                //    serverRelativeUrl = option.ParentFolderServerRelativeUrl + "/" + leafName; ;
                                //}

                                string serverRelativeUrl = option.ParentFolderServerRelativeUrl.TrimEnd('/') + "/" + leafName;
                                bool hidden = dr.IsDBNull(3) ? true : false;
                                Guid scopeId = dr.GetGuid(4);
                                string url = new Uri(new Uri(mSiteUrl), serverRelativeUrl).ToString();
                                bool hasUniqueRoleAssignments = !scopeId.Equals(parentFolderScopeId);
                                AveFolderBrowserInfo folder = new AveFolderBrowserInfo()
                                {
                                    UniqueId = uniqueId,
                                    Name = leafName,
                                    ServerRelativeUrl = serverRelativeUrl,
                                    Url = url,
                                    ParentListId = listId,
                                    ParentId = option.ParentFolderId,
                                    Hidden = hidden,
                                    //ListHasUniqueRoleAssignments = listHasUniqueRoleAssignment,
                                    HasUniqueRoleAssignments = hasUniqueRoleAssignments
                                };
                                subFolders.Add(folder);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserFolderInfoError, e.ToString());
                        }
                    }
                    return subFolders;
                }

            }

        }

        public List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {


            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserItems"))
            {

                List<AveItemBrowserInfo> items = new List<AveItemBrowserInfo>();
                int pagedCount = 0;
                int itemCount;
                Guid parentScopeId = Guid.Empty;
                Guid parentFolderListId = Guid.Empty;
                int listType = 0;
                GetParentFolderListId(option.ParentSiteId, option.ParentWebId, option.ParentFolderId, ref parentFolderListId, ref parentScopeId, ref listType);
                itemCount = GetFolderItemsCount(option.ParentSiteId, option.ParentWebId, parentFolderListId, option.ParentFolderId);
                var inputListType = listType == 1 ? "ad.leafName" : "ad.DoclibRowId";
                string cmdText = string.Empty;
                using (var connect = new AveSqlConnection(mConnectString))
                {
                    if (parentFolderListId == Guid.Empty)
                    {
                        connect.AddParameter("@ParentId", option.ParentFolderId);
                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = "SELECT ad.Id,ad.DirName,ad.LeafName,ad.ScopeId,w.FullUrl FROM AllDocs ad with(nolock) INNER JOIN AllWebs w with(nolock) ON ad.WebId=w.Id AND w.DeleteTransactionId=0x where ad.SiteId=@SiteId AND ad.ParentId=@ParentId AND ad.TYPE=0 AND ad.DeleteTransactionId=0x AND ad.IsCurrentVersion=1";
                        }
                        else
                        {
                            cmdText = "SELECT ad.Id,ad.DirName,ad.LeafName,ad.ScopeId,w.FullUrl FROM AllDocs ad with(nolock) INNER JOIN Webs w with(nolock) ON ad.WebId=w.Id where ad.SiteId=@SiteId AND ad.ParentId=@ParentId AND ad.TYPE=0 AND ad.DeleteTransactionId=0x AND ad.IsCurrentVersion=1";
                        }
                    }
                    else
                    {
                        pagedCount = AveBrowserHelper.GetPagedCount(option.PageInfo);
                        cmdText = string.Format(@"select * from(
SELECT top {0} ad.Id,ad.DirName,ad.LeafName,ad.DoclibRowId,ad.ScopeId,al.tp_BaseType,w.FullUrl,au.nvarchar1, au.tp_UIVersionString,au.tp_Editor,au.tp_Modified,au.tp_Level,
ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber
FROM AllDocs ad with(nolock) INNER JOIN AllLists al with(nolock) on ad.SiteId=al.tp_SiteId AND al.tp_WebId=ad.WebId AND ad.ListId=al.tp_ID 
INNER JOIN AllWebs w with(nolock) on ad.SiteId=w.SiteId AND ad.WebId=w.Id AND w.DeleteTransactionId=0x 
INNER JOIN AllUserData au with(nolock) on ad.SiteId=au.tp_SiteId AND au.tp_DeleteTransactionId=0x AND au.tp_IsCurrentVersion=1  AND ad.ParentId=au.tp_ParentId AND ad.Id=au.tp_DocId AND ad.UIVersion=au.tp_UIVersion AND au.tp_RowOrdinal=0
WHERE ad.SiteId=@SiteId AND ad.DeleteTransactionId=0x AND ad.DirName=@DirName AND ad.DoclibRowId IS NOT NULL AND ad.TYPE=0 AND ad.IsCurrentVersion=1 
) tmp where tmp.RowNumber >{2}", option.PerPage + pagedCount, inputListType, pagedCount);
                    }

                    connect.AddParameter("@SiteId", option.ParentSiteId); //If it doesn't need site id, please uncomment this line. 
                    connect.AddParameter("@DirName", option.ParentFolderServerRelativeUrl.TrimStart('/'));
                    try
                    {
                        using (SqlDataReader dr = connect.ExecuteReader(cmdText))
                        {
                            try
                            {
                                while (dr.Read())
                                {
                                    AveItemBrowserInfo itemInfo = null;
                                    if (parentFolderListId == Guid.Empty)
                                    {
                                        Guid uniqueId = dr.GetGuid(0);
                                        string dirName = dr.GetString(1);
                                        string leafName = dr.GetString(2);
                                        Guid scopeId = dr.GetGuid(3);
                                        string webServerRelativeUrl = dr.GetString(4);
                                        string url = dirName.Substring(webServerRelativeUrl.Length + 1) + "/" + leafName;
                                        bool hasUniqueRoleAssignments = !scopeId.Equals(parentScopeId);
                                        itemInfo = new AveItemBrowserInfo
                                        {
                                            UniqueId = uniqueId,
                                            Name = leafName,
                                            Url = url,
                                            ParentFolderUniqueID = option.ParentFolderId,
                                            ParentListID = Guid.Empty,
                                            HasUniqueRoleAssignments = hasUniqueRoleAssignments
                                        };
                                    }
                                    else
                                    {
                                        Guid uniqueId = dr.GetGuid(0);
                                        string dirName = dr.GetString(1);
                                        int listBaseType = dr.GetInt32(5);
                                        string name = string.Empty;
                                        string displayName = string.Empty;
                                        if (listBaseType == 1)//DocumentLibrary
                                        {
                                            displayName = name = dr.GetString(2);
                                            int index = displayName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
                                            if (index >= 0)
                                            {
                                                displayName = displayName.Substring(0, index);
                                            }
                                        }
                                        else
                                        {
                                            name = displayName = dr.IsDBNull(7) ? string.Empty : dr.GetString(7);
                                        }
                                        Guid scopeId = dr.GetGuid(4);
                                        int docLibRowId = dr.GetInt32(3);
                                        string webServerRelativeUrl = dr.GetString(6);
                                        string url = dirName.Substring(webServerRelativeUrl.Length + 1) + "/" + name;
                                        bool hasUniqueRoleAssignments = !scopeId.Equals(parentScopeId);
                                        string currentUIVersionString = dr.GetString(8);
                                        int lastModifier = dr.GetInt32(9);
                                        DateTime lastModifyTime = dr.GetDateTime(10);
                                        byte level = dr.GetByte(11);
                                        itemInfo = new AveItemBrowserInfo
                                        {
                                            UniqueId = uniqueId,
                                            Name = name,
                                            DisplayName = displayName,
                                            ID = docLibRowId,
                                            ListBaseType = listBaseType,
                                            Url = url,
                                            ParentFolderUniqueID = option.ParentFolderId,
                                            HasUniqueRoleAssignments = hasUniqueRoleAssignments,
                                            ParentListID = parentFolderListId,
                                            CurrentUIVersionString = currentUIVersionString,
                                            LastModifier = lastModifier,
                                            LastModifyTime = lastModifyTime,
                                            Level = level
                                        };
                                    }
                                    items.Add(itemInfo);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetBrowserItems.  Error Message: {0}", e.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetBrowserItems.  Error Message: {0}", e.ToString());

                    }
                    foreach (var item in items)
                    {
                        cmdText = @"select tp_UIVersionString,tp_Level from AllUserData where tp_SiteId=@tp_SiteId and tp_DeleteTransactionId=0x and (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) and tp_ParentId=@tp_ParentId
                               and tp_DocId=@tp_DocId  and tp_RowOrdinal=0 and tp_IsCurrent=0 order by AllUserData.tp_UIVersionString desc";
                        connect.ClearParameters();
                        connect.AddParameter("@tp_ParentId", option.ParentFolderId);
                        connect.AddParameter("@tp_SiteId", option.ParentSiteId); //If it doesn't need site id, please uncomment this line. 
                        connect.AddParameter("@tp_DocId", item.UniqueId); //If it doesn't need doc id, please uncomment this line. 
                        try
                        {
                            using (SqlDataReader dr = connect.ExecuteReader(cmdText))
                            {
                                while (dr.Read())
                                {
                                    item.Versions[dr.GetString(0)] = dr.GetByte(1); ;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(string.Format("An error occurred while to get Item versions information. Exception: {0}", ex.ToString()));
                        }
                    }
                }
                if (itemCount - pagedCount <= option.PerPage)
                {
                    option.PageInfo = string.Empty;
                }
                else
                {
                    option.PageInfo = listType == 1 ? string.Format("Paged=TRUE&p_SortBehavior=0&p_FileLeafRef={0}&RootFolder={1}&StartIndex={2}", items[items.Count - 1].Name, option.ParentFolderServerRelativeUrl, pagedCount + option.PerPage) :
                                               string.Format("Paged=TRUE&p_SortBehavior=0&p_ID={0}&RootFolder={1}&StartIndex={2}", items[items.Count - 1].ID, option.ParentFolderServerRelativeUrl, pagedCount + option.PerPage);
                }
                return items;


            }


        }

        public List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserItemVersions"))
            {

                List<AveItemVersionBrowserInfo> itemVersions = new List<AveItemVersionBrowserInfo>();
                string cmdText = @"
SELECT Count(ad.tp_Id) FROM AllUserData as ad with(nolock) 
WHERE ad.tp_SiteId = @SiteId and ad.tp_DeleteTransactionId = 0x and ad.tp_ParentId = @parentId and ad.tp_DocId = @docId
";
                using (var connect = new AveSqlConnection(mConnectString))
                {
                    connect.AddParameter("@SiteId", option.ParentSiteId);
                    connect.AddParameter("@parentId", option.ParentFolderId);
                    connect.AddParameter("@docId", option.ParentItemUniqueId);
                    option.ChildrenTotalCount = (int)connect.ExecuteScalar(cmdText);
                    int index = option.StartIndex > option.ChildrenTotalCount ? 0 : option.StartIndex;

                    cmdText = string.Format(@"
SELECT TOP {0} * FROM
(
SELECT nvarchar1,tp_UIVersionString,ROW_NUMBER() over (ORDER BY tp_UIVersionString) as RowNumber 
FROM AllUserData ad with(nolock) 
WHERE ad.tp_SiteId = @SiteId and ad.tp_DeleteTransactionId = 0x and ad.tp_ParentId = @parentId and ad.tp_DocId = @docId
) as temp
WHERE RowNumber > {1}", option.PerPage + index, index);

                    using (SqlDataReader dr = connect.ExecuteReader(cmdText))
                    {
                        try
                        {
                            while (dr.Read())
                            {
                                AveItemVersionBrowserInfo itemInfo = null;
                                string displayName = dr.GetString(0);
                                string version = dr.GetString(1);
                                itemInfo = new AveItemVersionBrowserInfo
                                {
                                    ItemID = option.ParentItemUniqueId,
                                    VersionLabel = version,
                                    ItemUniqueID = option.ParentItemUniqueId
                                };
                                itemVersions.Add(itemInfo);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserItemVersionInfoError, e.ToString());
                        }
                    }
                }
                return itemVersions;

            }

        }

        public AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserRootWeb"))
            {

                AveWebBrowserInfo webBrowserInfo = null;
                SPWebTemplateCollection webTemplates;

                try
                {
                    using (var connect = new AveSqlConnection(mConnectString))
                    {
                        string platformVersion = GetSitePlatformVersion(option.ParentSiteId, connect);
                        string cmdText = string.Format("SELECT Id,FullUrl,Title,Language,WebTemplate,ProvisionConfig,FirstUniqueAncestorWebId FROM Webs WHERE SiteId=@SiteId AND ParentWebId IS NULL ORDER BY FullUrl");
                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText = AveReplaceProcessor.SqlQueryScriptReplace(cmdText, true);
                        }
                        connect.AddParameter("@SiteId", option.ParentSiteId);
                        using (SqlDataReader sr = connect.ExecuteReader(cmdText))
                        {
                            while (sr.Read())
                            {
                                try
                                {
                                    Guid id = sr.GetGuid(0);
                                    string name = sr.GetString(1);
                                    string title = null;
                                    int pos = name.LastIndexOf('/');
                                    if (pos >= 0)
                                    {
                                        name = name.Substring(pos + 1);
                                    }

                                    if (!sr.IsDBNull(2))
                                    {
                                        title = sr.GetString(2);
                                    }
                                    string url = sr.GetString(1);
                                    if (!string.IsNullOrEmpty(url) && !url.StartsWith("/", StringComparison.Ordinal))
                                    {
                                        url = "/" + url;
                                    }
                                    string fullUrl = new Uri(new Uri(mSiteUrl), url).ToString().TrimEnd('/');
                                    uint language = (uint)sr.GetInt32(3);
                                    int templateId = sr.GetInt32(4);
                                    int provisionConfig = sr.GetInt16(5);
                                    bool hasUniqueRoleAssignments = (sr.GetGuid(0) == sr.GetGuid(6)) ? true : false;

                                    webTemplates = GetWebTemplatesFromCache(option.ParentSiteId, language, platformVersion);
                                    string templateTitle = string.Empty;
                                    string templateName = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates, ref templateTitle);
                                    //bool isRootWeb = true;

                                    webBrowserInfo = new AveWebBrowserInfo()
                                    {
                                        ID = id,
                                        Name = name,
                                        Url = fullUrl,
                                        Title = title,
                                        Language = language,
                                        IsRootWeb = true,
                                        TemplateName = templateName,
                                        TemplateTitle = templateTitle,
                                        HasUniqueRoleAssignments = hasUniqueRoleAssignments
                                    };
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserRootWebInfoFromContentDBError, e.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserRootWebInfoError, e.ToString());
                }
                return webBrowserInfo;

            }

        }

        public AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveBrowserQuery.GetBrowserRootFolder"))
            {

                AveFolderBrowserInfo folderBrowserInfo = null;
                bool isWebFolder = option.ParentListId == Guid.Empty;
                string cmdTextString = string.Empty;
                try
                {
                    using (var connect = new AveSqlConnection(mConnectString))
                    {
                        StringBuilder cmdText = new StringBuilder();
                        if (AveSPUtility.IsSP1DBSchema(connect))
                        {
                            cmdText.Append(string.Format("SELECT ad.Id,ad.DirName,ad.LeafName,ad.ListId,ad.DoclibRowId,ad.ParentId,ad.ScopeId,w.FullUrl,w.ScopeId {0} FROM (AllDocs ad With(nolock) inner join AllWebs w with(nolock) on w.SiteId=@siteId AND ad.WebId=w.Id AND w.DeleteTransactionId=0x) ", isWebFolder ? string.Empty : ",al.tp_ScopeId"));
                        }
                        else
                        {
                            cmdText.Append(string.Format("SELECT ad.Id,ad.DirName,ad.LeafName,ad.ListId,ad.DoclibRowId,ad.ParentId,ad.ScopeId,w.FullUrl,w.ScopeId {0} FROM (AllDocs ad With(nolock) inner join Webs w with(nolock) on w.SiteId=@siteId AND ad.WebId=w.Id) ", isWebFolder ? string.Empty : ",al.tp_ScopeId"));
                        }
                        if (isWebFolder)
                        {
                            cmdText.Append("where ad.ListId IS NULL AND (w.FullUrl =ad.DirName+'/'+ad.LeafName AND LEN(ad.DirName)<>0 or LEN(ad.DirName)=0 AND w.FullUrl=ad.LeafName) AND ad.SiteId=@siteId AND ad.WebId=@webId");
                        }
                        else
                        {
                            connect.AddParameter("@ListId", option.ParentListId);
                            cmdText.Append("inner join AllLists al with(nolock) on ad.ListId=@listId AND ad.ListId= al.tp_ID AND ad.Id=tp_RootFolder where ad.SiteId=@siteId AND ad.WebId=@webId AND ad.DeleteTransactionId=0x AND Type=1 AND IsCurrentVersion=1");
                        }
                        connect.AddParameter("@siteId", option.ParentSiteId);
                        connect.AddParameter("@webId", option.ParentWebId);
                        using (SqlDataReader dr = connect.ExecuteReader(cmdText.ToString()))
                        {
                            if (dr.Read())
                            {
                                Guid uniqueId = dr.GetGuid(0);
                                string dirName = dr.GetString(1);
                                string leafName = dr.GetString(2);
                                string serverRelativeUrl = dirName + "/" + leafName;
                                if (!serverRelativeUrl.StartsWith("/", StringComparison.Ordinal))
                                {
                                    serverRelativeUrl = "/" + serverRelativeUrl;
                                }
                                Guid listId = dr.IsDBNull(3) ? Guid.Empty : dr.GetGuid(3);
                                bool Hidden = dr.IsDBNull(4) ? true : false;
                                Guid parentId = dr.GetGuid(5);
                                string webServerRelativeUrl = isWebFolder ? dr.GetString(7) : serverRelativeUrl;
                                if (!webServerRelativeUrl.StartsWith("/", StringComparison.Ordinal))
                                {
                                    webServerRelativeUrl = "/" + webServerRelativeUrl;
                                }
                                string parentUrl = new Uri(new Uri(mSiteUrl), webServerRelativeUrl).ToString();
                                bool listHasUniqueRoleAssignment = isWebFolder ? true : dr.GetGuid(8).Equals(dr.GetGuid(9)) ? false : true;
                                bool itemHasUniqueRoleAssignments = isWebFolder ? true : dr.GetGuid(6).Equals(dr.GetGuid(9)) ? false : true;
                                folderBrowserInfo = new AveFolderBrowserInfo()
                                {
                                    UniqueId = uniqueId,
                                    Name = leafName,
                                    ServerRelativeUrl = serverRelativeUrl,
                                    Url = parentUrl,
                                    ParentListId = listId,
                                    ParentId = listId,
                                    Hidden = Hidden,
                                    HasUniqueRoleAssignments = listHasUniqueRoleAssignment
                                };
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.BrowserRootFolderInfoError, e.ToString());
                }
                return folderBrowserInfo;

            }

        }
        private class UserInfo
        {
            /// <summary>
            /// full name with prefix
            /// </summary>
            public string FullName;
            /// <summary>
            /// full name without prefix
            /// </summary>
            public string LogonName;
            /// <summary>
            /// without domain
            /// </summary>
            public string Name;
            public string DomainName;
            public string DomainFullName;
        }
    }
}
