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
using AvePoint.Common.ActiveDirectoryWrapper;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public class WebApplicationLevel : IndividualBase
    {
        #region Security trimming caches
        //Cache本域和双向信任域的User和AD Group
        Dictionary<string, ActiveDirectoryObject> bidirectionaUsersCache = new Dictionary<string, ActiveDirectoryObject>(StringComparer.OrdinalIgnoreCase);
        //Cache单向信任域的User和AD Group
        Dictionary<string, ActiveDirectoryObject> outboundUsersCache = new Dictionary<string, ActiveDirectoryObject>(StringComparer.OrdinalIgnoreCase);
        //Key:SP 中存储的domain，Value：AD 中对应的Domain name
        Dictionary<string, string> domainsCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        #endregion

        public WebApplicationLevel(AveObjectModelFactory objectModel)
            : base(objectModel, string.Empty, string.Empty)
        {

        }

        public IAveWebApplication GetWebApplication(string url)
        {
            return ObjectModel.CreateWebApplication(url);
        }
        public List<SPTreeNodeDto> GetWebApplications(bool includeCAWebApp, int startIndex, uint perPage, ref int childrenCount)
        {
            bool hasError = false;
            return GetWebApplications(includeCAWebApp, startIndex, perPage, ref childrenCount, ref hasError, null);
        }
        public List<SPTreeNodeDto> GetWebApplications(bool includeCAWebApp, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, List<string> usernames)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> webApps = new List<SPTreeNodeDto>();
            List<SPTreeNodeDto> pagingWebApps = new List<SPTreeNodeDto>();
            IAveWebService webService = ObjectModel.CreateWebService();
            if (webService.ContentService == null)
            {
                hasError = true;
                Logger.Warn("Can not get the content service,maybe the agent account does not have sufficient permission.");
            }
            else
            {
                #region Deal with UserNames
                Dictionary<string, ActiveDirectoryDomain> bidirectionalDirectory = null;
                if (usernames != null && usernames.Count > 0)
                {
                    bidirectionalDirectory = AveBrowserHelper.GetBidirectionalDirectoryDomains();
                    StringBuilder usernamesForLog = new StringBuilder();
                    for (int index = 0; index < usernames.Count; index++)
                    {
                        string name = usernames[index];
                        if (string.IsNullOrEmpty(name))
                        {
                            usernames.RemoveAt(index);
                            usernamesForLog.Append(" " + ";");
                            index--;
                        }
                        else
                        {
                            usernamesForLog.Append(name + ";");
                        }
                    }
                    Logger.Debug("Get web applications with this users: {0}", usernamesForLog);
                }
                #endregion

                foreach (IAveWebApplication webApp in webService.ContentService.WebApplications)
                {
                    var webAppNode = ConvertToDto(webApp);

                    #region Security Trimming
                    try
                    {
                        if (usernames != null && usernames.Count > 0)
                        {
                            var masks = GetUserMaskInWebApp(webApp, usernames, bidirectionalDirectory);
                            var permissions = new List<SPTreePermissionMappingDto>();
                            foreach (var mask in masks)
                            {
                                var permissionDto = new SPTreePermissionMappingDto { UserName = mask.Key, Url = webAppNode.Url };
                                permissionDto.Permission = mask.Value;
                                permissions.Add(permissionDto);
                            }
                            webAppNode.NodeExtension.PermissionList = permissions;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("An error occurred while trimming web application: {0}.  Error: {1} ", webAppNode.Url, e);
                    }
                    #endregion

                    webApps.Add(webAppNode);
                }
                #region Dispose Bidirection Cache.
                AveBrowserHelper.DisposeADCache(bidirectionaUsersCache);
                AveBrowserHelper.DisposeADCache(bidirectionalDirectory);
                #endregion

                if (includeCAWebApp)
                {
                    IAveAdministrationWebApplication CAWebApp = ObjectModel.CreateAdministrationWebApplication();
                    webApps.Add(ConvertToDto(CAWebApp));
                }
                webApps.Sort(new SPTreeNodeDtoComparer(true));
                childrenCount = webApps.Count;
                if (perPage >= childrenCount)//all nodes can return in one page
                {
                    pagingWebApps.AddRange(webApps);
                }
                else
                {
                    int _index = 0;
                    int pagingCount = 0;
                    if (startIndex > childrenCount)
                    {
                        startIndex = 0;
                    }
                    if (childrenCount - startIndex < perPage)
                    {
                        pagingCount = childrenCount - startIndex;
                    }
                    else
                    {
                        pagingCount = (int)perPage;
                    }
                    try
                    {
                        while (_index < pagingCount)
                        {
                            pagingWebApps.Add(webApps[startIndex + _index]);
                            _index++;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Warn("StartIndex of current Page Out of Range.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", startIndex, childrenCount, e.ToString());
                    }
                }
            }
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower WebApplications Elapsed Time: {0}, WebApplicationCount: {1}", sw.Elapsed.ToString(), pagingWebApps.Count);
#endif
            return pagingWebApps;
        }


        /// <summary>
        /// Get User's mask from web application node.
        /// </summary>
        /// <param name="webApp">Web Application</param>
        /// <param name="users">List of users</param>
        /// <returns>Key: User name; Value: Deny and Grand mask value.</returns>

        public Dictionary<string, SPTreePermission> GetUserMaskInWebApp(IAveWebApplication webApp, List<string> users, Dictionary<string, ActiveDirectoryDomain> bidirectionalDomainCache)
        {
            var result = new Dictionary<string, SPTreePermission>();
            //当前web app的单向信任域。
            var outboundDomainCache = AveBrowserHelper.GetOutboundDirectoryDomains(webApp);
            var webAppPolicies = webApp.Policies;
            if (webAppPolicies != null)
            {
                foreach (var username in users)
                {
                    var webAppMask = new SPTreePermission();
                    try
                    {
                        long grantMask = 0;
                        long denyMask = 0;
                        foreach (IAvePolicy policy in webAppPolicies)
                        {
                            if (IsUserAccessible(username, webApp, policy, bidirectionalDomainCache, outboundDomainCache))
                            {
                                foreach (var bind in policy.PolicyRoleBindings)
                                {
                                    if (bind.DenyRightsMask != AveBasePermissions.EmptyMask)
                                    {
                                        denyMask |= (long)bind.DenyRightsMask;
                                    }
                                    if (bind.GrantRightsMask != AveBasePermissions.EmptyMask)
                                    {
                                        grantMask |= (long)bind.GrantRightsMask;
                                    }
                                }
                            }
                        }
                        webAppMask.GrantMask = grantMask;
                        webAppMask.DenyMask = denyMask;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("An error occurred while getting web application's mask.Web App:{0},Error:{1}", webApp.Name, e.ToString());
                    }
                    result[username] = webAppMask;
                }
            }
            #region Dispose Out Bound Cache.
            AveBrowserHelper.DisposeADCache(outboundUsersCache);
            AveBrowserHelper.DisposeADCache(outboundDomainCache);
            #endregion
            return result;
        }

        /// <summary>
        /// 判断指定User和policy的User的关系。
        /// </summary>
        /// <param name="userName">指定User</param>
        /// <param name="webApp"></param>
        /// <param name="policy"></param>
        /// <returns>当plicy中的User是指定User的parent group或者是同一User，返回true。否则false.</returns>
        private bool IsUserAccessible(string username, IAveWebApplication webApp, IAvePolicy policy, Dictionary<string, ActiveDirectoryDomain> bidirectionalDomainCache,
        Dictionary<string, ActiveDirectoryDomain> outboundDomainCache)
        {
            ActiveDirectoryObject searchUserADObject = null;
            #region Find user object from control.
            if (!bidirectionaUsersCache.TryGetValue(username, out searchUserADObject) && !outboundUsersCache.TryGetValue(username, out searchUserADObject))
            {
                bool isOutboundObject = true;
                searchUserADObject = AveBrowserHelper.GetADObjectByLoginName(username, outboundDomainCache, bidirectionalDomainCache, out isOutboundObject);
                if (isOutboundObject)
                {
                    outboundUsersCache[username] = searchUserADObject;
                }
                else
                {
                    bidirectionaUsersCache[username] = searchUserADObject;
                }
            }
            if (searchUserADObject == null)
            {
                Logger.Warn("Can not found this user.");
                return false;
            }
            #endregion

            ActiveDirectoryObject activeObject = null;

            #region Find user object from SP.
            string usernameFromPolicy = policy.UserName;
            if (IsAuthenticatedUsers(usernameFromPolicy))
            {
                return true;
            }
            else if (!IsBuiltInAccount(usernameFromPolicy) && !bidirectionaUsersCache.TryGetValue(usernameFromPolicy, out activeObject) && !outboundUsersCache.TryGetValue(usernameFromPolicy, out activeObject))
            {
                bool isOutboundObject;
                int index = usernameFromPolicy.IndexOf('\\');
                if (index > 0)
                {
                    string realName = AveBrowserHelper.GetUserRealLoginName(usernameFromPolicy, domainsCache);
                    activeObject = AveBrowserHelper.GetADObjectByLoginName(realName, outboundDomainCache, bidirectionalDomainCache, out isOutboundObject);
                    if (isOutboundObject)
                    {
                        outboundUsersCache[usernameFromPolicy] = activeObject;
                    }
                    else
                    {
                        bidirectionaUsersCache[usernameFromPolicy] = activeObject;
                    }
                }
                else
                {
                    activeObject = AveBrowserHelper.GetADGroupObjectBySID(usernameFromPolicy, outboundDomainCache, bidirectionalDomainCache, out isOutboundObject);
                    if (isOutboundObject)
                    {
                        outboundUsersCache[usernameFromPolicy] = activeObject;
                    }
                    else
                    {
                        bidirectionaUsersCache[usernameFromPolicy] = activeObject;
                    }
                }
            }
            #endregion
            if (activeObject != null)
            {
                if (activeObject.IsGroup)
                {
                    return searchUserADObject.IsMemeberOf(activeObject);
                }
                else
                {
                    return searchUserADObject.ObjectSID.Equals(activeObject.ObjectSID, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        public List<SPTreeNodeDto> GetContentDBs(string webAppUrl, ref bool hasError)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> contentDBs = new List<SPTreeNodeDto>();
            IAveWebApplication webApplication = ObjectModel.CreateWebApplication();
            webApplication = webApplication.Lookup(new Uri(webAppUrl));
            foreach (IAveContentDatabase contentDatabase in webApplication.ContentDatabases)
            {
                if (contentDatabase == null)
                {
                    Logger.Warn("Cannot get content database information, because it is null, webapplication url is {0}", webAppUrl);
                    hasError = true;
                    continue;
                }
                SPTreeNodeDto dto = new SPTreeNodeDto();
                dto.Name = contentDatabase.Name;
                dto.FullPath = contentDatabase.Name;
                dto.SPObjectId = contentDatabase.ID.ToString();
                dto.DisplayName = contentDatabase.Name;
                dto.FarmID = FarmId;
                dto.Level = NodeLevel.ContentDB;
                /*
                dto.NodeExtension.ContentDB = new ContentDB();
                dto.NodeExtension.ContentDB.ID = contentDatabase.ID.ToString();
                dto.NodeExtension.ContentDB.Name = contentDatabase.Name;
                dto.Level = NodeLevel.ContentDB;
                 */
                contentDBs.Add(dto);
            }
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Content Database Elapsed Time: {0}, ContentDBCount: {1}, WebAppUrl: {2}", sw.Elapsed.ToString(), contentDBs.Count, webAppUrl);
#endif
            return contentDBs;
        }

        public SPTreeNodeDto GetContentDB(IAveWebApplication webApp, string contentDBName)
        {
            foreach (IAveContentDatabase contentDatabase in webApp.ContentDatabases)
            {
                if (contentDatabase == null)
                {
                    continue;
                }
                SPTreeNodeDto dto = new SPTreeNodeDto();
                if (contentDatabase.Name.Equals(contentDBName, StringComparison.OrdinalIgnoreCase))
                {
                    dto.SPObjectId = contentDatabase.ID.ToString();
                    dto.FarmID = FarmId;
                    dto.Level = NodeLevel.ContentDB;
                    dto.Name = contentDatabase.Name;
                    return dto;
                }
            }
            throw new Exception(string.Format("Cannot get the content database."));
        }

        private SPTreeNodeDto ConvertToDto(IAveWebApplication webApp)
        {
            SPTreeNodeDto dto = new SPTreeNodeDto();
            string theUrl = webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString();
            dto.FullPath = dto.Url = theUrl;
            dto.Name = webApp.Name;
            dto.DisplayName = webApp.DisplayName;
            dto.SPObjectId = webApp.ID.ToString();
            dto.Level = NodeLevel.WebApplication;
            dto.FarmID = FarmId;
            //dto.IsFba = CheckIsPureFba(webApp);
            dto.NodeExtension = new NodeExtensionDto();
            dto.NodeExtension.IsFba = CheckIsPureFba(webApp);
            dto.NodeExtension.Languages = new Languages();
            dto.NodeExtension.Languages.Language = new List<Language>();
            IAveRegionalSettings settings = ObjectModel.CreateRegionalSettings();
            dto.NodeExtension.Languages.Default = settings.GlobalServerLanguage.LCID.ToString();
            foreach (IAveLanguage language in settings.GlobalInstalledLanguages)
            {
                Language temp = new Language();
                temp.DisplayName = language.DisplayName;
                temp.LCID = language.LCID;
                dto.NodeExtension.Languages.Language.Add(temp);
            }
            dto.NodeExtension.ContentDBList = new List<ContentDB>();
            Logger.Debug("content database count: " + webApp.ContentDatabases.Count.ToString());
            foreach (IAveContentDatabase aveContentDB in webApp.ContentDatabases)
            {
                if (aveContentDB == null)
                {
                    Logger.Warn("cannot retrieve info from a null content database");
                    continue;
                }
                Logger.Debug("content database id: " + aveContentDB?.ID);
                ContentDB tempDB = new ContentDB();
                tempDB.ID = aveContentDB.ID.ToString();
                tempDB.Name = aveContentDB.DisplayName;
                dto.NodeExtension.ContentDBList.Add(tempDB);
            }
            dto.NodeExtension.ManagedPathList = new List<ManagedPathDto>();
            foreach (IAvePrefix prefix in webApp.Prefixes)
            {
                ManagedPathDto managedPath = new ManagedPathDto();
                switch (prefix.PrefixType.ToString())
                {
                    case "Explicit":
                        managedPath.Type = ManagedPathType.Explicit;
                        break;
                    case "ExplicitInclusion":
                        managedPath.Type = ManagedPathType.ExplicitInclusion;
                        break;
                    case "Wildcard":
                        managedPath.Type = ManagedPathType.Wildcard;
                        break;
                    case "WildcardInclusion":
                        managedPath.Type = ManagedPathType.WildcardInclusion;
                        break;
                    case "Exclusion":
                        managedPath.Type = ManagedPathType.Exclusion;
                        break;
                    default:
                        break;
                }
                managedPath.Name = prefix.Name;
                dto.NodeExtension.ManagedPathList.Add(managedPath);
            }
            return dto;
        }

        public SPTreeNodeDto ConvertToWebApplicationDto(IAveWebApplication webApp)
        {
            return ConvertToDto(webApp);
        }

        private bool CheckIsPureFba(IAveWebApplication webApp)
        {
            //To Do: Change to Wrapper
            foreach (var iisSetting in webApp.IisSettings.Values)
            {
                if (iisSetting.AuthenticationMode == System.Web.Configuration.AuthenticationMode.Windows)
                {
                    return false;
                }
                else if (iisSetting.AuthenticationMode == System.Web.Configuration.AuthenticationMode.Forms)
                {
                    if (iisSetting.ClaimsAuthenticationProviders == null)
                    {
                        return false;
                    }
                    foreach (var oneAuthProvider in iisSetting.ClaimsAuthenticationProviders)
                    {
                        if (oneAuthProvider is IAveWindowsAuthenticationProvider)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private class SPTreeNodeDtoComparer : IComparer<SPTreeNodeDto>
        {
            private bool mAsc;
            private bool p;

            public SPTreeNodeDtoComparer(bool asc)
            {
                this.mAsc = asc;
            }

            public int Compare(SPTreeNodeDto a, SPTreeNodeDto b)
            {
                string x, y;
                if (mAsc)
                {
                    x = a.Name;
                    y = b.Name;
                }
                else
                {
                    x = b.Name;
                    y = a.Name;
                }
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
