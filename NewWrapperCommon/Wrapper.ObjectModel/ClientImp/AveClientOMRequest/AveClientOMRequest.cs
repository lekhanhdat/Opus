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
using System.Collections.Specialized;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Web;
using System.Xml;
using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Application;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.WebParts;
using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
using ClientFile = Microsoft.SharePoint.Client.File;
using ClientFolder = Microsoft.SharePoint.Client.Folder;
using SPChangeType = Microsoft.SharePoint.Client.ChangeType;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Utility.I18N;
using System.Net.Sockets;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.SharePoint.Client.Workflow;
using System.Collections.ObjectModel;
using AvePoint.Wrapper.Restore;

namespace AvePoint.ObjectModel.ClientOM
{
    internal delegate ClientObjectData GetObjectData(ClientObject clientObject);

    public class AveClientOMRequest : IAveRequest, IDisposable
    {
        private static readonly string mUnauthorizedMessage = "The remote server returned an error: (401) Unauthorized.";
        private static readonly string mAccessDeniedMessage = "Access denied. You do not have permission to perform this action or access this resource.";
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOMRequest));
        protected string mWebUrl;
        protected string mWebAppName;
        protected static List<string> mListTemplateList = new List<string> { "Meetings", "Agenda", "MeetingUser", "Decision", "MeetingObjective", "TextBox", "ThingsToBring", "HomePageLibrary" };
        protected List<string> mSpecialFileList = new List<string>() { ".master", ".evtx", ".cs", ".xoml", ".rules" };
        protected object mObj;
        protected string mServerVersion;
        protected string mSiteRelativeUrl;
        protected uint maxItemsPerThrottledOperation;
        protected AveWebServiceRequest mWebServiceRequest;
        internal uint MaxItemsPerThrottledOperation
        {
            get
            {
                if (this.maxItemsPerThrottledOperation <= 0)
                {
                    using (AveClientContext context = CreateContext())
                    {
                        context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                        context.ExecuteQuery();
                        this.maxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation > 0 ? context.Site.MaxItemsPerThrottledOperation : 5000;
                    }
                }
                return this.maxItemsPerThrottledOperation;
            }
        }
        internal AveBPOSAccountInfo mUserAccountInfo;
        protected ClientContext mFormDigestContext;

        internal static GetObjectData GetProperty { get; set; }
        private static Action<ClientContext> FormDigestDelegate { get; set; }
        private static Func<ClientContext, object> GetFormDigestField { get; set; }
        private static Action<ClientContext, object> SetFormDigestField { get; set; }

        public AveClientRequestType Type { get; protected set; }

        //ensure formdigestvaue only be fetched once
        static AveClientOMRequest()
        {
            MethodInfo method = typeof(ClientObject).GetProperty("ObjectData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty).GetGetMethod(true);
            GetProperty = (GetObjectData)Delegate.CreateDelegate(typeof(GetObjectData), method);

            MethodInfo ensureFormDigestMethod = typeof(ClientContext).GetMethod("EnsureFormDigest", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            FormDigestDelegate = (Action<ClientContext>)Delegate.CreateDelegate(typeof(Action<ClientContext>), ensureFormDigestMethod);

            GetFormDigestField = typeof(ClientContext).GetField("m_formDigestInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue;
            SetFormDigestField = typeof(ClientContext).GetField("m_formDigestInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).SetValue;
        }

        protected SecurityTrimObject mSiteTrimObj;

        internal string WebAppName
        {
            get
            {
                if (mWebAppName == null)
                {
                    int indexOfSlash = mWebUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = mWebUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = mWebUrl.Substring(0, indexOfSlash);
                    }
                }
                return mWebAppName;
            }
        }

        public AveClientOMRequest(string url, AveBPOSAccountInfo userAccountInfo, object obj, string serverVersion)
        {
            mObj = obj;
            mWebUrl = url;
            mUserAccountInfo = userAccountInfo;
            mServerVersion = serverVersion;
            Type = AveClientRequestType.AveClientOMRequest;
            mFormDigestContext = InitClientObject(url);
            mSiteTrimObj = new SecurityTrimObject() { Level = SecurityTrimLevel.Site, Name = url };
            mWebServiceRequest = new AveWebServiceRequest(url, userAccountInfo, obj, mServerVersion, mSiteTrimObj);
        }

        protected virtual AveClientContext InitClientObject(string url)
        {
            AveClientContext context = new AveClientContext(url);
            context.RequestTimeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout;//ten miniutes
            context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(SetCookie);


            return context;
        }

        protected void EnsureFormDigest(ClientContext clientContext, string currentWebUrl)
        {
            if (mFormDigestContext == null || !mFormDigestContext.Url.Equals(currentWebUrl, StringComparison.OrdinalIgnoreCase))
            {
                mFormDigestContext = InitClientObject(currentWebUrl);
            }
            lock (mFormDigestContext)
            {
                FormDigestDelegate(mFormDigestContext);
            }
            object tempFormDigest = GetFormDigestField(mFormDigestContext);
            SetFormDigestField(clientContext, tempFormDigest);
        }

        /// <summary>
        /// 尽量使用 CreateContext(string weburl) 来创建Context，否则多线程可能会出异常
        /// </summary>
        /// <returns></returns>
        internal virtual AveClientContext CreateContext()
        {
            return CreateContext(mWebUrl);
        }

        protected virtual AveClientContext CreateContext(string weburl)
        {
            AveClientContext context = InitClientObject(weburl);
            EnsureFormDigest(context, weburl);
            return context;
        }

        protected void SetCookie(object sender, WebRequestEventArgs e)
        {
            AveWebRequestExecutor requestExecutor = e.WebRequestExecutor as AveWebRequestExecutor;
            if (requestExecutor != null)
            {
                if (mObj != null && mObj is CookieContainer)
                {
                    requestExecutor.Request.CookieContainer = mObj as CookieContainer;
                }
                else
                {
                    requestExecutor.RequestHeaders["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
                    requestExecutor.Request.Credentials = mObj as NetworkCredential;
                }
            }
        }
        public virtual object Credentials
        {
            get
            {
                return this.mObj;
            }
            set
            {
                this.RefreshCredentials(value);
            }
        }
        public virtual string Url
        {
            get
            {
                return this.mWebUrl;
            }
        }
        public virtual AveRequestKind Kind
        {
            get
            {
                return AveRequestKind.ClientObjectModel;
            }
        }
        public virtual void Dispose()
        {
        }

        #region Get
        public virtual Dictionary<string, object> GetSite()
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    context.Load(context.Site);
                    context.Load(context.Site, site => site.Usage);
                    LoadWeb(context.Site.RootWeb, context);
                    context.ExecuteQuery();
                    this.maxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                    CopyProperty(siteProperties, context.Site);
                    siteProperties["Usage"] = AssemblyUsageInfo(context.Site.Usage);
                    siteProperties["CompatibilityLevel"] = 14;
                    Dictionary<string, object> rootWebProperties = GetWebProperties(context, context.Site.RootWeb, mWebUrl, context.Site.ServerRelativeUrl, true);
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    //siteProperties.Add("SyndicationEnabled", context.Site.RootWeb.SyndicationEnabled);
                    siteProperties.Add("IsMoss", false);
                    mSiteRelativeUrl = context.Site.ServerRelativeUrl;
                    //siteProperties.Add("IsPublish", false);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetSiteError, context.Url, e.ToString());
                    throw;
                }
                return siteProperties;
            }
        }

        public virtual Dictionary<string, object> GetBrowserSiteInfo()
        {
            return GetSite();
        }

        public virtual Dictionary<string, object> GetAdminCenterSite()
        {
            return null;
        }

        public AveUsageInfo AssemblyUsageInfo(UsageInfo usageInfo)
        {
            AveUsageInfo usage = new AveUsageInfo();
            usage.Bandwidth = usageInfo.Bandwidth;
            usage.DiscussionStorage = usageInfo.DiscussionStorage;
            usage.Hits = usageInfo.Hits;
            usage.Storage = usageInfo.Storage;
            usage.Visits = usageInfo.Visits;
            return usage;
        }

        public virtual Site GetSiteById(Guid siteId)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = null;
                if (context.Site.Id.Equals(siteId))
                {
                    site = context.Site;
                }
                return site;
            }
        }

        public virtual Dictionary<string, object> GetWeb(Guid webId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                try
                {

                    Web web = context.Site.OpenWebById(webId);
                    webProperties = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetWebError, context.Url, e.ToString());
                    throw;
                }
                return webProperties;
            }
        }
        public virtual Dictionary<string, object> GetWeb(string webServerRelativeUrl)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl.TrimEnd('/'));
                    webProperties = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                }
            }
            catch (Exception e)
            {
                mLogger.Debug(AveClientOMRequestResource.CannotGetWeb, webServerRelativeUrl, e.ToString());
                webProperties["Exists"] = false;
            }
            return webProperties;
        }

        public virtual Dictionary<string, object> GetFirstUniqueNavigationWeb(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl.TrimEnd('/'));
                    string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                    context.Load(web, w => w.Navigation.UseShared, w => w.ServerRelativeUrl);
                    context.ExecuteQuery();
                    bool isUsedShared = web.Navigation.UseShared;
                    while (isUsedShared)
                    {
                        int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                        string parentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex);
                        web = context.Site.OpenWeb(parentWebServerRelativeUrl);
                        context.Load(web, w => w.Navigation.UseShared, w => w.ServerRelativeUrl);
                        context.ExecuteQuery();
                        isUsedShared = web.Navigation.UseShared;
                    }
                    webProperties = GetWebProperties(context, web, context.Url, siteServerRelativeUrl, false);
                }
                catch (Exception e)
                {
                    webProperties["Exists"] = false;
                    mLogger.Debug(e.ToString());
                }
                return webProperties;
            }
        }

        public virtual Dictionary<string, object> GetQuickLaunchFromInheritWeb(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl.TrimEnd('/'));
                    string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                    context.Load(web, w => w.ServerRelativeUrl);
                    context.ExecuteQuery();
                    //bool isUsedShared = web.Navigation.UseShared;
                    var properties = web.AllProperties;
                    context.Load(properties);
                    context.ExecuteQuery();
                    string isInheritCurrentNavigation = "False";
                    if (web.ServerRelativeUrl.Equals("/") || web.ServerRelativeUrl.Equals(siteServerRelativeUrl) || !properties.FieldValues.ContainsKey("__InheritCurrentNavigation"))
                    {
                        //isInheritCurrentNavigation = "False";
                    }
                    else
                    {
                        isInheritCurrentNavigation = (string)properties.FieldValues["__InheritCurrentNavigation"];
                    }
                    while (isInheritCurrentNavigation.Equals("True", StringComparison.OrdinalIgnoreCase))
                    {
                        int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                        string parentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex);
                        web = context.Site.OpenWeb(parentWebServerRelativeUrl);
                        context.Load(web, w => w.ServerRelativeUrl);
                        var propertiesOfSub = web.AllProperties;
                        context.Load(propertiesOfSub);
                        context.ExecuteQuery();
                        if (web.ServerRelativeUrl.Equals("/") || web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase) || !propertiesOfSub.FieldValues.ContainsKey("__InheritCurrentNavigation"))
                        {
                            isInheritCurrentNavigation = "False";
                        }
                        else
                        {
                            isInheritCurrentNavigation = (string)propertiesOfSub.FieldValues["__InheritCurrentNavigation"];
                        }
                    }
                    webProperties = GetWebProperties(context, web, context.Url, siteServerRelativeUrl, false);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.CannotGetFirstUniqueNavigationWeb, webServerRelativeUrl, e.ToString());
                    webProperties["Exists"] = false;
                }
                return webProperties;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ws is a variable")]
        public virtual Dictionary<string, object> GetAllWebs()
        {
            using (AveClientContext context = CreateContext())
            {
                List<Dictionary<string, object>> webList = new List<Dictionary<string, object>>();
                Dictionary<string, object> allWebs = new Dictionary<string, object>();
                try
                {
                    Web rootWeb = context.Site.RootWeb;
                    WebCollection subWebs = rootWeb.GetSubwebsForCurrentUser(null);
                    LoadWebAndSubwebs(context, rootWeb, subWebs);
                    webList.Add(GetWebProperties(context, rootWeb, context.Site.Url, context.Site.ServerRelativeUrl, true));
                    foreach (Web web in subWebs)
                    {
                        Dictionary<string, object> dicWeb = new Dictionary<string, object>();
                        dicWeb = GetWebProperties(context, web, context.Site.Url, context.Site.ServerRelativeUrl, true);
                        webList.Add(dicWeb);
                        WebGetSubwebs(context, web, webList, context.Site.Url, context.Site.ServerRelativeUrl);
                    }
                    allWebs.Add(AveObjectModelConstant.ChildrenProperties, webList);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetAllWebsError, context.Url, e.ToString());
                    throw;
                }
                return allWebs;
            }
        }

        protected virtual void LoadWebAndSubwebs(ClientContext context, Web web, WebCollection subWebs)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(context.Site);
                    context.Load(web);
                    context.Load(web, tempWeb => tempWeb.CurrentUser,
                                                 tempWeb => tempWeb.RootFolder,
                                                 tempWeb => tempWeb.ListTemplates,
                                                 tempWeb => tempWeb.AllProperties,
                                                 tempWeb => tempWeb.Navigation.TopNavigationBar,
                                                 tempWeb => tempWeb.Navigation.QuickLaunch,
                                                 tempWeb => tempWeb.AllowDesignerForCurrentUser,
                                                 tempWeb => tempWeb.HasUniqueRoleAssignments,
                                                 tempWeb => tempWeb.AssociatedMemberGroup, tempWeb => tempWeb.AssociatedMemberGroup.Users, tempWeb => tempWeb.AssociatedMemberGroup.Owner.Id, tempWeb => tempWeb.AssociatedMemberGroup.Owner.PrincipalType,
                                                 tempWeb => tempWeb.AssociatedOwnerGroup, tempWeb => tempWeb.AssociatedOwnerGroup.Users, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.Id, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.PrincipalType
                                                 );
                    context.Load(subWebs, tempWebs => tempWebs.IncludeWithDefaultProperties(tempWeb => tempWeb.CurrentUser,
                                                                                  tempWeb => tempWeb.RootFolder,
                                                                                  tempWeb => tempWeb.ListTemplates,
                                                                                  tempWeb => tempWeb.AllProperties,
                                                                                  tempWeb => tempWeb.Navigation.TopNavigationBar,
                                                                                  tempWeb => tempWeb.Navigation.QuickLaunch,
                                                                                  tempWeb => tempWeb.AllowDesignerForCurrentUser,
                                                                                  tempWeb => tempWeb.HasUniqueRoleAssignments,
                                                                                  tempWeb => tempWeb.AssociatedMemberGroup, tempWeb => tempWeb.AssociatedMemberGroup.Users, tempWeb => tempWeb.AssociatedMemberGroup.Owner.Id, tempWeb => tempWeb.AssociatedMemberGroup.Owner.PrincipalType,
                                                                                  tempWeb => tempWeb.AssociatedOwnerGroup, tempWeb => tempWeb.AssociatedOwnerGroup.Users, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.Id, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.PrincipalType
                                                                                  ));
                }
                using (scope.StartCatch())
                {
                    context.Load(context.Site);
                    context.Load(web);
                    context.Load(web, temp => temp.CurrentUser,
                                                 temp => temp.RootFolder,
                                                 temp => temp.ListTemplates,
                                                 temp => temp.AllProperties,
                                                 temp => temp.Navigation.TopNavigationBar,
                                                 temp => temp.Navigation.QuickLaunch,
                                                 temp => temp.AllowDesignerForCurrentUser,
                                                 temp => temp.HasUniqueRoleAssignments,
                                                 temp => temp.AssociatedMemberGroup, temp => temp.AssociatedMemberGroup.Users, temp => temp.AssociatedMemberGroup.Owner.Id, temp => temp.AssociatedMemberGroup.Owner.PrincipalType
                                                 //w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType
                                                 );
                    context.Load(subWebs, tempWebs => tempWebs.IncludeWithDefaultProperties(temp => temp.CurrentUser,
                                                                                  temp => temp.RootFolder,
                                                                                  temp => temp.ListTemplates,
                                                                                  temp => temp.AllProperties,
                                                                                  temp => temp.Navigation.TopNavigationBar,
                                                                                  temp => temp.Navigation.QuickLaunch,
                                                                                  temp => temp.AllowDesignerForCurrentUser,
                                                                                  temp => temp.HasUniqueRoleAssignments,
                                                                                  temp => temp.AssociatedMemberGroup, temp => temp.AssociatedMemberGroup.Users, temp => temp.AssociatedMemberGroup.Owner.Id, temp => temp.AssociatedMemberGroup.Owner.PrincipalType
                                                                                  //w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType
                                                                                  ));
                }
            }
            context.ExecuteQuery();
        }

        public virtual Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null)
        {
            if (!string.IsNullOrEmpty(webServerRelativeUrl))
            {
                throw new NotImplementedException("SharePoint 2010 do not support web's RecycleBin.");
            }
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> recycleBin = new Dictionary<string, object>();
                try
                {
                    ExceptionHandlingScope han = new ExceptionHandlingScope(context);
                    using (han.StartScope())
                    {
                        using (han.StartTry())
                        {
                            context.Load(context.Site.RecycleBin, bin => bin.IncludeWithDefaultProperties(temp => temp.Author, temp => temp.DeletedBy));
                        }
                        using (han.StartCatch())
                        {
                            context.Load(context.Site.RecycleBin, bin => bin.IncludeWithDefaultProperties());
                        }
                    }
                    context.ExecuteQuery();
                    AssembleRecycleBinProperties(context.Site.RecycleBin, recycleBin);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetRecycleBinError, context.Url, e.ToString());
                    throw;
                }
                return recycleBin;
            }
        }

        protected virtual void AssembleRecycleBinProperties(RecycleBinItemCollection recycleBinCollection, Dictionary<string, object> recycleBin)
        {
            List<Dictionary<string, object>> recycleBinList = new List<Dictionary<string, object>>();
            foreach (RecycleBinItem recycleBinItem in recycleBinCollection)
            {
                Dictionary<string, object> dicRecycleBin = new Dictionary<string, object>();
                CopyProperty(dicRecycleBin, recycleBinItem);
                try
                {
                    dicRecycleBin["Author" + AveObjectModelConstant.ObjectPropertySuffix] = recycleBinItem.Author.LoginName;
                    dicRecycleBin["DeletedBy" + AveObjectModelConstant.ObjectPropertySuffix] = recycleBinItem.DeletedBy.LoginName;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get recycle bin author and deleted by error. Error massage:{0}", e.Message);
                    continue;
                }
                recycleBinList.Add(dicRecycleBin);
            }
            recycleBin.Add(AveObjectModelConstant.ChildrenProperties, recycleBinList);
        }

        protected Dictionary<string, object> GetNavigation(string webServerRelativeUrl, Dictionary<string, object> nodesProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.Navigation);
                context.Load(web, w => w.Navigation.QuickLaunch, w => w.Navigation.TopNavigationBar);
                context.ExecuteQuery();
                Dictionary<string, object> navigationProperties = new Dictionary<string, object>();
                CopyProperty(navigationProperties, web.Navigation);
                navigationProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = web.Navigation.Path;

                List<Dictionary<string, object>> quickLaunchList = NavigationNodeCollectionToList(web.Navigation.QuickLaunch, nodesProp);
                List<Dictionary<string, object>> topNavigationBarList = NavigationNodeCollectionToList(web.Navigation.TopNavigationBar, nodesProp);
                Dictionary<string, object> quickLaunchProperties = new Dictionary<string, object>();
                quickLaunchProperties.Add(AveObjectModelConstant.ChildrenProperties, quickLaunchList);
                Dictionary<string, object> topNavigationBarProperties = new Dictionary<string, object>();
                topNavigationBarProperties.Add(AveObjectModelConstant.ChildrenProperties, topNavigationBarList);

                navigationProperties["QuickLaunch" + AveObjectModelConstant.ObjectPropertySuffix] = quickLaunchProperties;
                navigationProperties["TopNavigationBar" + AveObjectModelConstant.ObjectPropertySuffix] = topNavigationBarProperties;
                Dictionary<string, object> quickLaunchParentProperties = new Dictionary<string, object>();
                Dictionary<string, object> topNavigationBarParentProperties = new Dictionary<string, object>();
                quickLaunchParentProperties["Title"] = "Quick launch";
                quickLaunchParentProperties["Id"] = 1025;
                quickLaunchParentProperties["Children" + AveObjectModelConstant.ObjectPropertySuffix] = quickLaunchProperties;
                quickLaunchParentProperties["ClientContext"] = context;
                topNavigationBarParentProperties["Title"] = "SharePoint Top Navigation Bar";
                topNavigationBarParentProperties["Id"] = 1002;
                topNavigationBarParentProperties["Children" + AveObjectModelConstant.ObjectPropertySuffix] = topNavigationBarProperties;
                topNavigationBarParentProperties["ClientContext"] = context;
                navigationProperties["QuickLaunchParent" + AveObjectModelConstant.ObjectPropertySuffix] = quickLaunchParentProperties;
                navigationProperties["TopNavigationBarParent" + AveObjectModelConstant.ObjectPropertySuffix] = topNavigationBarParentProperties;
                return navigationProperties;
            }
        }

        public virtual Dictionary<string, object> GetItems(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType, l => l.ItemCount);
                context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                context.ExecuteQuery();
                CamlQuery camlquery = new CamlQuery();
                camlquery.ViewXml = camlQueryNode[3];
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    SetCamlQueryFolderUrl(camlquery, camlQueryNode[4]);
                    //camlquery.FolderServerRelativeUrl = camlQueryNode[4];
                }
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    ListItemCollectionPosition licp = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                    camlquery.ListItemCollectionPosition = licp;
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlquery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }
                if (!IsThrottled(list.ItemCount))
                {
                    ListItemCollection items = list.GetItems(camlquery);
                    LoadItemsProperty(context, items);
                    context.ExecuteQuery();
                    List<Dictionary<string, object>> itemList = new List<Dictionary<string, object>>();
                    foreach (ListItem item in items)
                    {
                        //if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor"))
                        //{
                        //    context.Load(item);
                        //    context.ExecuteQuery();
                        //}
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                        GetItemDic(itemProperties, item);
                        itemList.Add(itemProperties);
                    }
                    itemsProperties[AveObjectModelConstant.ChildrenProperties] = itemList;
                    if (items.ListItemCollectionPosition != null)
                    {
                        itemsProperties["PageInfo"] = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        itemsProperties["PageInfo"] = null;
                    }
                }
                else
                {
                    var filesMap = new Dictionary<string, ClientFile>();
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    List<Dictionary<string, object>> itemList = new List<Dictionary<string, object>>();
                    var parentFolderUrl = !string.IsNullOrEmpty(camlQueryNode[4]) ? camlQueryNode[4] : list.RootFolder.ServerRelativeUrl;
                    var listItemCollectionPosition = QueryItemsByQueryStringForLargeList(context, list, webServerRelativeUrl, parentFolderUrl, scope, filesMap, itemList, camlquery);
                    itemsProperties[AveObjectModelConstant.ChildrenProperties] = itemList;
                    if (listItemCollectionPosition != null)
                    {
                        itemsProperties["PageInfo"] = listItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        itemsProperties["PageInfo"] = null;
                    }
                }
                return itemsProperties;
            }
        }

        public virtual Dictionary<string, object> GetItemsForRecords(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType, l => l.ItemCount);
                context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                context.ExecuteQuery();
                CamlQuery camlquery = new CamlQuery();
                camlquery.ViewXml = camlQueryNode[3];
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    SetCamlQueryFolderUrl(camlquery, camlQueryNode[4]);
                    //camlquery.FolderServerRelativeUrl = camlQueryNode[4];
                }
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    ListItemCollectionPosition licp = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                    camlquery.ListItemCollectionPosition = licp;
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlquery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }

                ListItemCollection items = list.GetItems(camlquery);
                LoadItemsProperty(context, items);
                context.ExecuteQuery();
                List<Dictionary<string, object>> itemList = new List<Dictionary<string, object>>();
                foreach (ListItem item in items)
                {
                    //if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor"))
                    //{
                    //    context.Load(item);
                    //    context.ExecuteQuery();
                    //}
                    Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                    GetItemDic(itemProperties, item);
                    itemList.Add(itemProperties);
                }
                itemsProperties[AveObjectModelConstant.ChildrenProperties] = itemList;
                if (items.ListItemCollectionPosition != null)
                {
                    itemsProperties["PageInfo"] = items.ListItemCollectionPosition.PagingInfo;
                }
                else
                {
                    itemsProperties["PageInfo"] = null;
                }

                return itemsProperties;
            }
        }

        protected virtual void LoadItemsProperty(ClientContext context, ListItemCollection items)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                using (scope.StartTry())
                {
                    context.Load(items);
                    context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName));
                }
                using (scope.StartCatch())
                {
                    context.Load(items);
                    context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments));
                }
            }
        }

        public virtual Dictionary<string, object> GetItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Guid uniqueId)
        {
            ListItem item = null;
            List list = null;
            ClientFile file = null;
            if (itemId.Equals(default(int)))
            {
                throw new NullReferenceException("Item id is null.");
            }
            else
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    if (listId != Guid.Empty)
                    {
                        list = web.Lists.GetById(listId);
                    }
                    else
                    {
                        list = web.Lists.GetByTitle(listName);
                    }
                    context.Load(list,
                       tempList => tempList.BaseType, tempList => tempList.BaseTemplate);
                    item = list.GetItemById(itemId);
                    LoadItemProperty(context, item);
                    context.ExecuteQuery();
                }
            }
            Dictionary<string, object> itemPro = new Dictionary<string, object>();
            GetItemDic(itemPro, item);
            if (!ItemHasVersion(list, itemPro) || !WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
            {
                itemPro["HasVersion"] = false;
            }
            return itemPro;

        }

        protected virtual void LoadItemProperty(AveClientContext context, ListItem item)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                using (scope.StartTry())
                {
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments, tempItem => tempItem.DisplayName);
                }
                using (scope.StartCatch())
                {
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments);
                }
            }
        }

        public virtual Dictionary<string, object> GetForms(string webServerRelativeUrl, string listName, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.Forms);
                context.ExecuteQuery();
                List<Dictionary<string, object>> forms = new List<Dictionary<string, object>>();
                foreach (Form form in list.Forms)
                {
                    Dictionary<string, object> formPro = new Dictionary<string, object>();
                    formPro["ID"] = form.Id;
                    formPro["Url"] = form.ServerRelativeUrl;
                    formPro["TemplateName"] = form.FormType.ToString();
                    forms.Add(formPro);
                }
                returnInfo[AveObjectModelConstant.ChildrenProperties] = forms;
                return returnInfo;
            }
        }

        public virtual Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                List<Dictionary<string, object>> workflows = new List<Dictionary<string, object>>();
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCollection wfa = null;
                Web web = null;
                switch (workflowSource)
                {
                    case "list.workflow":
                        web = context.Site.OpenWeb(webServerRelativeUrl);
                        //List list = web.Lists.GetByTitle(listName);
                        List list = web.Lists.GetById(listId);
                        wfa = list.WorkflowAssociations;
                        break;
                    case "contentType.workflow":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        wfa = contentType.WorkflowAssociations;
                        break;
                    case "web.workflow":
                        web = context.Site.OpenWeb(webServerRelativeUrl);
                        wfa = web.WorkflowAssociations;
                        break;
                    default:
                        break;
                }
                context.Load(wfa);
                context.ExecuteQuery();
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflow in wfa)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    if (workflowSource == "contentType.workflow")
                    {
                        workflowPro["ContentTypeIdString"] = contentTypeProp["ContentTypeId"] as string;
                    }
                    workflows.Add(workflowPro);
                }
                returnInfo[AveObjectModelConstant.ChildrenProperties] = workflows;
                return returnInfo;
            }
        }
        public virtual Dictionary<string, object> GetWorkflowTemplates(string webServerRelativeUrl, string webName, Guid webId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                List<Dictionary<string, object>> workflowTemplates = new List<Dictionary<string, object>>();
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplateCollection wfTemplates = null;
                switch (workflowSource)
                {
                    case "web.workflowTemplates":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        wfTemplates = web.WorkflowTemplates;
                        break;
                    default:
                        break;
                }
                context.Load(wfTemplates);
                context.ExecuteQuery();
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template in wfTemplates)
                {
                    Dictionary<string, object> workflowtmplate = new Dictionary<string, object>();
                    CopyProperty(workflowtmplate, template);
                    workflowtmplate["PermissionsManual"] = ConvertBasePermToULong(template.PermissionsManual);
                    workflowTemplates.Add(workflowtmplate);
                }
                returnInfo[AveObjectModelConstant.ChildrenProperties] = workflowTemplates;
                return returnInfo;
            }
        }

        public virtual ClientFile GetFileByAPI(Web web, string url)
        {
            return web.GetFileByServerRelativeUrl(url);
        }

        public virtual Dictionary<string, object> GetViews(string webServerRelativeUrl, string listName, Guid listId)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl)))
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                List<Dictionary<string, object>> views = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields));

                context.ExecuteQuery();
                int max = 10;
                int count = 0;
                Dictionary<string, ClientFile> viewFiles = new Dictionary<string, ClientFile>();
                Dictionary<string, Dictionary<string, object>> viewPropderties = new Dictionary<string, Dictionary<string, object>>();
                foreach (View view in list.Views)
                {
                    Dictionary<string, object> viewPro = new Dictionary<string, object>();
                    AssembleViewProperties(viewPro, view, webServerRelativeUrl);
                    viewPropderties[view.ServerRelativeUrl] = viewPro;
                    ClientFile viewFile = GetFileByAPI(context.Web, view.ServerRelativeUrl);
                    context.Load(viewFile, v => v.ETag);
                    viewFiles[view.ServerRelativeUrl] = viewFile;
                    count++;
                    if (count >= max)
                    {
                        try
                        {
                            context.ExecuteQuery();
                        }
                        catch (ServerUnauthorizedAccessException e)
                        {
                            mLogger.Warn("You don't have permission to access this data. ", e);
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("Exception was thrown while get single view", e);
                        }
                        count = 0;
                    }
                }
                if (count > 0)
                {
                    try
                    {
                        context.ExecuteQuery();
                    }
                    catch (ServerUnauthorizedAccessException e)
                    {
                        mLogger.Warn("You don't have permission to access this data. ", e);
                    }
                }
                foreach (var viewFile in viewFiles)
                {
                    string guid = GetIdsFromEtag(viewFile.Value.ETag)[0];
                    Dictionary<string, object> tempViewProperty = viewPropderties[viewFile.Key];
                    tempViewProperty["PageUrlID"] = new Guid(guid);
                    views.Add(tempViewProperty);
                }
                viewFiles.Clear();
                viewPropderties.Clear();
                returnInfo[AveObjectModelConstant.ChildrenProperties] = views;
                return returnInfo;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ls is a variable")]
        public virtual Dictionary<string, object> GetLists(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                LoadListCollection(context, scope, web.Lists);
                context.ExecuteQuery();
                List<Dictionary<string, object>> lists = new List<Dictionary<string, object>>();
                foreach (List l in web.Lists)
                {
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    if (scope.HasException && scope.ServerErrorCode == ErrorCodes.AccessDenied)
                    {
                        GetRootFolderProperties(rootFolderProp, context, l);
                        //context.Load(l.RootFolder, r => r.ServerRelativeUrl, r => r.Name);
                        //context.ExecuteQuery();
                        //AveObjectCopy.GetObjectBasicProperties(rootFolderProp, l.RootFolder);
                        //rootFolderProp["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable();
                        SecurityTrimObject listTrimObj = webTrimObj.GetList(l.Id, l.Title);
                        //listTrimObj.TrimmedProperties["IsSiteAssetsLibrary"] = "Access Denied" + scope.ErrorMessage;
                        SecurityTrimObject rootFolderTrimObj = listTrimObj.GetFolder(l.RootFolder.ServerRelativeUrl, l.RootFolder.Name);
                        rootFolderTrimObj.TrimmedProperties["Files"] = "Access Denied";
                        rootFolderTrimObj.TrimmedProperties["Folders"] = "Access Denied";
                        rootFolderTrimObj.TrimmedProperties["Tag"] = "Access Denied";
                        rootFolderTrimObj.TrimmedProperties["ItemCount"] = "Access Denied";
                        rootFolderTrimObj.TrimmedProperties["UniqueContentTypeOrder"] = "Access Denied";
                        rootFolderTrimObj.TrimmedProperties["WelcomePage"] = "Access Denied";
                        rootFolderTrimObj.TrimmedProperties["ServerObjectIsNull"] = "Access Denied";
                    }
                    else
                    {
                        try
                        {
                            AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, l.RootFolder);
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("list name:{0}" + l.Title, ex.ToString());
                        }
                    }
                    Dictionary<string, object> listProperties = new Dictionary<string, object>();
                    CopyProperty(listProperties, l);
                    long flag = 0;
                    if (l.EnableVersioning)
                        flag |= 0x0000000000000080;
                    if (!l.EnableAttachments)
                        flag |= 0x0000000000000008;
                    listProperties["Flag"] = flag;    //Can not get this property.
                    //external list
                    if (l.DataSource != null && l.BaseTemplate == (int)AveListTemplateType.ExternalList)
                    {
                        Dictionary<string, object> listDataSource = new Dictionary<string, object>();
                        listDataSource.Add(AveBDCProperties.LobSystemInstance, l.DataSource.Properties[AveBDCProperties.LobSystemInstance]);
                        listDataSource.Add(AveBDCProperties.EntityNamespace, l.DataSource.Properties[AveBDCProperties.EntityNamespace]);
                        listDataSource.Add(AveBDCProperties.Entity, l.DataSource.Properties[AveBDCProperties.Entity]);
                        listDataSource.Add(AveBDCProperties.SpecificFinder, l.DataSource.Properties[AveBDCProperties.SpecificFinder]);
                        listProperties.Add("DataSource" + AveObjectModelConstant.ObjectPropertySuffix, listDataSource);
                        //Always, itemCount value is zero in external list,
                        //listProperties.Remove("ItemCount");
                    }

                    rootFolderProp["Exists"] = true;
                    listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    lists.Add(listProperties);
                }
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                returnInfo.Add(AveObjectModelConstant.ChildrenProperties, lists);
                return returnInfo;
            }
        }

        protected virtual void GetRootFolderProperties(Dictionary<string, object> rootFolderProp, AveClientContext context, List l)
        {
            context.Load(l.RootFolder, r => r.ServerRelativeUrl, r => r.Name);
            context.ExecuteQuery();
            AveObjectCopy.GetObjectBasicProperties(rootFolderProp, l.RootFolder);
            rootFolderProp["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable();
        }

        protected virtual void LoadListCollection(AveClientContext context, ExceptionHandlingScope scope, ListCollection listCollection)
        {
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(listCollection, tempListCollection => tempListCollection.IncludeWithDefaultProperties(l => l.ValidationFormula,
                                                                                                      l => l.ValidationMessage,
                                                                                                      l => l.OnQuickLaunch,
                                                                                                      l => l.IsSiteAssetsLibrary,
                                                                                                      l => l.HasUniqueRoleAssignments,
                                                                                                      l => l.DataSource,
                                                                                                      l => l.Id,
                                                                                                      l => l.ItemCount,
                                                                                                      l => l.EnableAttachments,
                                                                                                      l => l.EnableVersioning,
                                                                                                      l => l.DefaultDisplayFormUrl,
                                                                                                      l => l.RootFolder
                                                                                                      ));
                }
                using (scope.StartCatch())
                {
                    context.Load(listCollection, tempListCollection => tempListCollection.IncludeWithDefaultProperties(
                                                                                                      l => l.HasUniqueRoleAssignments,
                                                                                                      l => l.DataSource,
                                                                                                      l => l.Id,
                                                                                                      l => l.ItemCount,
                                                                                                      l => l.EnableAttachments,
                                                                                                      l => l.EnableVersioning
                                                                                                      ));
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ls is a variable")]
        /// <summary>
        /// load properties of list for discover,browser
        /// </summary>
        protected virtual void LoadListPropertiesForDiscoverBrowser(ClientContext context, Web web)
        {
            context.Load(web.Lists, listCollection => listCollection.Include(
                                                     l => l.Id,
                                                     l => l.Title,
                                                     l => l.BaseType,
                                                     l => l.BaseTemplate,
                                                     l => l.Hidden,
                                                     l => l.EnableVersioning,
                                                     l => l.EnableAttachments,
                                                     l => l.RootFolder.ServerRelativeUrl,
                                                     l => l.RootFolder.Name,
                                                     l => l.HasUniqueRoleAssignments,
                                                     l => l.EnableFolderCreation));
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ls is a variable")]
        public virtual Dictionary<string, object> GetLists(Guid webId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                context.Load(web, w => w.ServerRelativeUrl);
                LoadListPropertiesForDiscoverBrowser(context, web);
                context.ExecuteQuery();
                List<Dictionary<string, object>> lists = new List<Dictionary<string, object>>();
                foreach (List l in web.Lists)
                {
                    Dictionary<string, object> listProperties = new Dictionary<string, object>();
                    CopyProperty(listProperties, l);
                    long flag = 0;
                    if (l.EnableVersioning)
                        flag |= 0x0000000000000080;
                    if (!l.EnableAttachments)
                        flag |= 0x0000000000000008;
                    listProperties["Flag"] = flag;    //Can not get this property.
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    AssemblRootFolderProperties(web.ServerRelativeUrl, rootFolderProp, l.RootFolder);
                    listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    lists.Add(listProperties);
                }
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                returnInfo.Add(AveObjectModelConstant.ChildrenProperties, lists);
                return returnInfo;
            }
        }
        public virtual Dictionary<string, object> GetListByTitle(Guid webId, string listTitle)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                context.Load(web, w => w.ServerRelativeUrl);
                context.ExecuteQuery();
                List list = web.Lists.GetByTitle(listTitle);
                this.LoadList(context, list);
                context.ExecuteQuery();
                Dictionary<string, object> listProperties = new Dictionary<string, object>();
                CopyProperty(listProperties, list);
                long flag = 0;
                if (list.EnableVersioning)
                    flag |= 0x0000000000000080;
                if (!list.EnableAttachments)
                    flag |= 0x0000000000000008;
                listProperties["Flag"] = flag;    //Can not get this property.
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                try
                {
                    AssemblRootFolderProperties(web.ServerRelativeUrl, rootFolderProp, list.RootFolder);
                }
                catch (Exception e)
                {
                    mLogger.Warn("List's root folder is not available. {0}" + list.Title, e);
                }
                rootFolderProp["Exists"] = true;
                listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                listProperties["WebId"] = webId;
                listProperties["RootFolderUrl"] = rootFolderProp["ServerRelativeUrl"];
                if (rootFolderProp.ContainsKey("UniqueId"))
                {
                    listProperties["RootFolderId"] = rootFolderProp["UniqueId"];
                }
                return listProperties;
            }
        }
        public virtual string GetListSchemalXml(string ParentWebUrl, Guid Id, string listTitle)
        {
            using (AveClientContext context = CreateContext())
            {
                //if (string.IsNullOrEmpty(ParentWebUrl) || Guid.Equals(Id, Guid.Empty))
                //    RefreshContext();
                Web web = context.Site.OpenWeb(ParentWebUrl);
                List list = web.Lists.GetById(Id);
                context.Load(list, l => l.SchemaXml);
                try
                {
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    mLogger.Debug("Cannot get schema xml. Web: {0}, Id: {1}, Title: {2} \n {3}", ParentWebUrl, Id.ToString(), listTitle, ex.ToString());
                }
                return list.SchemaXml;
            }
        }
        public virtual Dictionary<string, object> GetList(string webServerRelativeUrl, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                this.LoadList(context, list);
                Dictionary<string, object> listProp = new Dictionary<string, object>();
                CopyProperty(listProp, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                listProp["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return listProp;
            }
        }

        public virtual Dictionary<string, object> GetList(string webServerRelativeUrl, string title)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetByTitle(title);
                this.LoadList(context, list);
                Dictionary<string, object> listProp = new Dictionary<string, object>();
                CopyProperty(listProp, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                listProp["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return listProp;
            }
        }

        protected virtual void AssemblRootFolderProperties(string webServerRelativeUrl, Dictionary<string, object> folderProperties, Folder rootFolder)
        {
            CopyProperty(folderProperties, rootFolder);
            string url = TrimFolderUrl(webServerRelativeUrl, rootFolder.ServerRelativeUrl);
            folderProperties["Url"] = url;
            int length = url.LastIndexOf('/');
            string parentFolderUrl = length == -1 ? webServerRelativeUrl : webServerRelativeUrl + "/" + url.Substring(0, length);
            folderProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = parentFolderUrl;
            folderProperties["Exists"] = true;
        }

        public virtual Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> webTemplatesProperties = new Dictionary<string, object>();
                WebTemplateCollection templates = context.Web.GetAvailableWebTemplates(lcid, true);
                context.Load(templates);
                context.ExecuteQuery();
                List<Dictionary<string, object>> templateList = new List<Dictionary<string, object>>();
                foreach (WebTemplate template in templates)
                {
                    Dictionary<string, object> templateProperties = new Dictionary<string, object>();
                    CopyProperty(templateProperties, template);
                    templateList.Add(templateProperties);
                }
                webTemplatesProperties.Add(AveObjectModelConstant.ChildrenProperties, templateList);
                return webTemplatesProperties;
            }
        }
        public virtual Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webpartsProperties = new Dictionary<string, object>();
                try
                {
                    PersonalizationScope scope = PersonalizationScope.Shared;
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web);
                    List list = web.Lists.GetById(listId);
                    context.Load(list);
                    context.Load(list, l => l.Views.IncludeWithDefaultProperties(v => v.HtmlSchemaXml));
                    context.ExecuteQuery();
                    foreach (View view in list.Views)
                    {
                        ClientFile file = GetFileByAPI(web, view.ServerRelativeUrl);
                        context.Load(file, f => f.ServerRelativeUrl, f => f.ETag);
                        context.ExecuteQuery();
                        string fileDocId = string.Empty;
                        if (!string.IsNullOrEmpty(file.ETag))
                        {
                            int index = file.ETag.IndexOf(',');
                            fileDocId = file.ETag.Substring(1, index - 1);
                            fileDocId = new Guid(fileDocId).ToString();
                        }
                        PersonalizationScope personalizationScope = view.PersonalView ? PersonalizationScope.User : PersonalizationScope.Shared;
                        Dictionary<string, object> webpartManagerProperties = GetLimitedWebPartManager(web.ServerRelativeUrl, view.ServerRelativeUrl, (int)scope);
                        webpartManagerProperties = webpartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                        List<Dictionary<string, object>> webpartProperties = webpartManagerProperties[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                        foreach (Dictionary<string, object> webpartProperty in webpartProperties)
                        {
                            webpartProperty["Id"] = webpartProperty.ContainsKey("ID") ? new Guid(webpartProperty["ID"].ToString()) : Guid.Empty;
                            webpartProperty["DisplayName"] = webpartProperty.ContainsKey("Title") ? webpartProperty["Title"].ToString() : string.Empty;
                            webpartProperty["ZoneId"] = webpartProperty.ContainsKey("ZoneId") ? webpartProperty["ZoneId"].ToString() : string.Empty;
                            webpartProperty["Flags"] = 0;
                            webpartProperty["AllUsersProperties"] = null;
                            webpartProperty["PerUserProperties"] = null;
                            webpartProperty["IsIncluded"] = false;
                            webpartProperty["PartOrder"] = webpartProperty.ContainsKey("ZoneIndex") ? (int)webpartProperty["ZoneIndex"] : 0;
                            webpartProperty["View"] = Encoding.UTF8.GetBytes(view.HtmlSchemaXml);
                        }
                        webpartsProperties.Add(fileDocId, webpartProperties);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get item web parts failed,document id:{0},error:{1}", itemDocId, e.ToString());
                }
                return webpartsProperties;
            }
        }

        public virtual bool HaveAddAndCustomizePagesPermission
        {
            get { return true; }
        }

        public virtual Dictionary<string, object> GetContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, string contentTypeId)
        {
            using (AveClientContext context = CreateContext())
            {
                ContentType contentType = this.GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, contentTypeId);
                context.ExecuteQuery();
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                this.AssembleSingleContentTypeProperties(newProp, contentType);
                return newProp;
            }
        }
        public virtual Dictionary<string, object> GetSubWebs(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WebCollection webCollection = web.GetSubwebsForCurrentUser(null);
                context.Load(context.Site);
                context.Load(context.Site.RootWeb, w => w.Id);
                context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                    w => w.RootFolder,
                                                                    w => w.ListTemplates,
                                                                    w => w.AllProperties,
                                                                    w => w.Navigation.TopNavigationBar,
                                                                    w => w.Navigation.QuickLaunch,
                                                                    w => w.AllowDesignerForCurrentUser,
                                                                    w => w.HasUniqueRoleAssignments,
                                                                    w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType
                                                                    ));
                context.ExecuteQuery();
                Dictionary<string, object> subWebs = new Dictionary<string, object>();
                List<Dictionary<string, object>> subWebList = new List<Dictionary<string, object>>();
                foreach (Web subWeb in webCollection)
                {
                    Dictionary<string, object> subWebProperties = new Dictionary<string, object>();
                    subWebProperties = GetWebProperties(context, subWeb, context.Site.Url, context.Site.ServerRelativeUrl, true);
                    subWebList.Add(subWebProperties);
                }
                subWebs.Add(AveObjectModelConstant.ChildrenProperties, subWebList);
                return subWebs;
            }
        }
        public virtual Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName)
        {
            ClientFile file = null;
            bool serverRelativeUrlVaild = true;
            Dictionary<string, object> fileProperties = new Dictionary<string, object>();

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                file = GetFileByAPI(web, serverRelativeUrl);
                ConditionalScope fileExistScope = new ConditionalScope(context, () => file.Exists);
                using (fileExistScope.StartScope())
                {
                    using (fileExistScope.StartIfTrue())
                    {
                        if (!string.IsNullOrEmpty(listName))
                        {
                            SafeLoadFile(context, file);
                        }
                        else
                        {
                            LoadFileSpecialProperty(context, file);
                            context.Load(file);
                        }
                    }
                }

                try
                {
                    context.ExecuteQuery();
                    fileProperties["Exists"] = fileExistScope.TestResult.HasValue && fileExistScope.TestResult.Value;
                    serverRelativeUrlVaild = Convert.ToBoolean(fileProperties["Exists"]);
                }
                catch (Exception ex)
                {
                    mLogger.Debug("An error occurred while getting file.Message:{0}.", ex.ToString());
                    fileProperties["Exists"] = false;
                    serverRelativeUrlVaild = false;
                }
            }
            if (!serverRelativeUrlVaild)
            {
                //Assemble file necessary properties for restore
                fileProperties["Name"] = serverRelativeUrl.Substring(serverRelativeUrl.LastIndexOf('/') + 1);
                string parentFolderServerRelativeUrl = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                if (string.IsNullOrEmpty(parentFolderServerRelativeUrl))
                {
                    parentFolderServerRelativeUrl = "/";
                }
                fileProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = parentFolderServerRelativeUrl;
            }
            else
            {
                fileProperties["ListName"] = listName;

                AssembleFileProperties(fileProperties, file, webServerRelativeUrl, file.ListItemAllFields);
            }
            return fileProperties;
        }

        protected virtual void LoadFileSpecialProperty(ClientContext context, ClientFile file)
        {
            context.Load(file, f => f.CheckedOutByUser, f => f.Author, f => f.ModifiedBy);
        }

        protected void SafeLoadFile(ClientContext context, ClientFile file)
        {
            ConditionalScope isListItem = new ConditionalScope(context, () => file.ListItemAllFields.ServerObjectIsNull.Value);
            using (isListItem.StartScope())
            {
                using (isListItem.StartIfTrue())
                {
                    ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                    using (excepScope.StartScope())
                    {
                        using (excepScope.StartTry())
                        {
                            LoadFileSpecialProperty(context, file);
                            context.Load(file);
                        }
                        using (excepScope.StartCatch())
                        {
                            context.Load(file);
                        }
                    }
                }
                using (isListItem.StartIfFalse())
                {
                    ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                    using (excepScope.StartScope())
                    {
                        using (excepScope.StartTry())
                        {
                            LoadFileSpecialProperty(context, file);
                            context.Load(file);
                            context.Load(file, f => f.ListItemAllFields);
                        }
                        using (excepScope.StartCatch())
                        {
                            context.Load(file);
                            context.Load(file, f => f.ListItemAllFields);
                        }

                    }
                }
            }
        }

        public virtual Dictionary<string, object> GetFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileVersionsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                context.Load(file, f => f.Versions);
                context.Load(file, f => f.Versions.IncludeWithDefaultProperties(v => v.CreatedBy));
                context.ExecuteQuery();
                List<Dictionary<string, object>> versionList = new List<Dictionary<string, object>>();
                foreach (FileVersion fileVersion in file.Versions)
                {
                    Dictionary<string, object> versionProperties = new Dictionary<string, object>();
                    CopyProperty(versionProperties, fileVersion);
                    versionProperties["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = fileVersion.CreatedBy.ServerObjectIsNull.HasValue && !fileVersion.CreatedBy.ServerObjectIsNull.Value ? fileVersion.CreatedBy.LoginName : string.Empty;
                    versionProperties["ServerRelativeUrl"] = webServerRelativeUrl.TrimEnd('/') + "/" + fileVersion.Url.TrimStart('/');
                    versionList.Add(versionProperties);
                }
                fileVersionsProperties[AveObjectModelConstant.ChildrenProperties] = versionList;
                return fileVersionsProperties;
            }
        }
        public virtual Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            Stream stream = null;
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    context.RequestTimeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;//30 mins

                    stream = OpenBinaryDirect(context, fileServerRelativeUrl, mObj);
                    int size = 64 * 1024;
                    AveCoordinatedStream cacheStream = new AveCoordinatedStream();
                    this.CopyStream(stream, cacheStream, size, true);
                    return cacheStream;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error while copy file stream to cache stream.We will try again. Error:{0}", ex);
                try
                {
                    stream = RetryGetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
                }
                catch (Exception e)
                {
                    mLogger.Error("Get file throught WebService failed. File:{0} Web:{1} Error:{2}", fileServerRelativeUrl, webServerRelativeUrl, e);
                    stream = GetFileStreamByRestApi(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl), fileServerRelativeUrl);
                }
            }
            return stream;
        }

        protected virtual Stream GetFileStreamByRestApi(string webUrl, string fileServerRelativeUrl)
        {
            return null;
        }

        protected virtual Stream GetFileVersionStreamByRestApi(string webUrl, string fileServerRelativeUrl)
        {
            return null;
        }

        public virtual Stream RetryGetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            return null;
        }

        public virtual byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options)
        {
            using (Stream stream = this.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, string.Empty))
            {
                byte[] buffer = new byte[stream.Length];
                int len = 0;
                int position = 0;
                int count = stream.Length > 32768 ? 32768 : (int)stream.Length;
                while ((len = stream.Read(buffer, position, count)) != 0)
                {
                    position += len;
                    if (stream.Length - position < count)
                    {
                        count = (int)stream.Length - position;
                    }
                }
                return buffer;
            }
        }
        public virtual Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            return mWebServiceRequest.GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
        }

        public virtual Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource)
        {
            return mWebServiceRequest.GetUsers(webRelativeUrl, groupName, userColSource);
        }
        public virtual Dictionary<string, object> GetUser(string loginName)
        {
            return null;
        }
        public virtual Dictionary<string, object> GetUser(int id)
        {
            return null;
        }
        public virtual Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, int itemId)
        {
            return null;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "gs is a variable")]
        public virtual Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web.SiteGroups, gs => gs.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                context.ExecuteQuery();
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webRelativeUrl, mSiteTrimObj.Name);
                Dictionary<string, object> groups = new Dictionary<string, object>();
                List<Dictionary<string, object>> groupList = new List<Dictionary<string, object>>();
                foreach (Group group in web.SiteGroups)
                {
                    Dictionary<string, object> groupProp = GetGroupProperties(webTrimObj, context, group, true);
                    groupList.Add(groupProp);
                }
                groups.Add(AveObjectModelConstant.ChildrenProperties, groupList);
                return groups;
            }
        }
        public virtual Dictionary<string, object> GetGroup(string webRelativeUrl, string groupName)
        {
            return null;
        }
        public virtual Dictionary<string, object> GetFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> folderProp = GetFolder(context, webServerRelativeUrl, listName, listId, folderServerRelativeUrl);
                return folderProp;
            }
        }

        public virtual Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            return GetFolders(webServerRelativeUrl, listName, listId, folderServerRelativeUrl, false);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "lms is a variable")]
        public virtual Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, bool includeSystemFolder)
        {
            Dictionary<string, object> subFolders = new Dictionary<string, object>();
            List<Dictionary<string, object>> subFolderList = new List<Dictionary<string, object>>();
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Folder folder = null;
                    List<ListItem> listItems = new List<ListItem>();
                    Hashtable _hashTable = null;

                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    if (folder == null)
                    {
                        folder = GetFolderByAPI(web, folderServerRelativeUrl);
                        context.Load(folder);
                        context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder));
                        //context.Load(web, w => w.ServerRelativeUrl);
                        //items properties   
                        ListItemCollection subItems = null;
                        ExceptionHandlingScope excepScope = null;
                        if (listName != null)
                        {
                            excepScope = new ExceptionHandlingScope(context);
                            using (excepScope.StartScope())
                            {
                                using (excepScope.StartTry())
                                {
                                    //List list = web.Lists.GetByTitle(listName);
                                    List list = null;
                                    if (listId != Guid.Empty)
                                    {
                                        list = web.Lists.GetById(listId);
                                    }
                                    else
                                    {
                                        list = web.Lists.GetByTitle(listName);
                                    }
                                    CamlQuery camlQuery = new CamlQuery();
                                    camlQuery.ViewXml = string.Format(
                                        "<View Scope=\"RecursiveAll\">" +
                                        "<Query><Where><And>" +
                                        "<Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                                        "<Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>1</Value></Eq>" +
                                        "</And></Where></Query></View>",
                                        folderServerRelativeUrl);
                                    subItems = list.GetItems(camlQuery);
                                    LoadItemsProperty(context, subItems);
                                }
                                using (excepScope.StartCatch())
                                { }
                            }
                        }
                        context.ExecuteQuery();
                        if (excepScope != null && excepScope.HasException)
                        {
                            mLogger.Error("Failed to load folder items, {0}, {1}, {2}, {3}.", excepScope.ErrorMessage, excepScope.ServerStackTrace, excepScope.ServerErrorValue, excepScope.ServerErrorCode);
                        }
                        if (subItems != null && subItems.ServerObjectIsNull.HasValue && !subItems.ServerObjectIsNull.Value && excepScope != null && !excepScope.HasException)
                        {
                            listItems = new List<ListItem>(subItems);
                        }
                    }
                    if (listItems != null && listItems.Count > 0)
                    {
                        _hashTable = new Hashtable(listItems.Count, StringComparer.OrdinalIgnoreCase);
                        foreach (ListItem itm in listItems)
                        {
                            _hashTable[itm.FieldValues["FileRef"] as string] = itm;
                        }
                    }

                    List<string> excludeFolders = null;
                    if (folderServerRelativeUrl.Trim('/').Equals(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        context.Load(web, w => w.Lists.Include(l => l.RootFolder.ServerRelativeUrl));
                        context.ExecuteQuery();
                        excludeFolders = new List<string>();
                        foreach (List l in web.Lists)
                        {
                            excludeFolders.Add(l.RootFolder.ServerRelativeUrl.ToLowerInvariant());
                        }
                        excludeFolders.AddRange(new string[] { "_catalogs",
                        "_vti_pvt", "_cts", "_private",
                        "_themes", "lists" , "m"});
                    }
                    foreach (Folder subFolder in folder.Folders)
                    {
                        if (excludeFolders != null && excludeFolders.Count > 0)
                        {
                            if (excludeFolders.Contains(subFolder.ServerRelativeUrl.ToLowerInvariant()) || excludeFolders.Contains(subFolder.Name.ToLowerInvariant()))
                            {
                                continue;
                            }
                        }
                        Dictionary<string, object> subFolderProperties = new Dictionary<string, object>();
                        subFolderProperties["Exists"] = true;
                        AssembleFolderProperties(context, webServerRelativeUrl, subFolder, subFolder.ServerRelativeUrl, subFolderProperties);
                        if (listItems != null && _hashTable != null)
                        {
                            if (_hashTable.ContainsKey(subFolder.ServerRelativeUrl))
                            {
                                Dictionary<string, object> itmProp = new Dictionary<string, object>();
                                ListItem itm = _hashTable[subFolder.ServerRelativeUrl] as ListItem;
                                GetItemDic(itmProp, itm);
                                subFolderProperties["UniqueId"] = itmProp["UniqueId"];
                                subFolderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itmProp;
                            }
                        }
                        subFolderList.Add(subFolderProperties);
                    }
                    if (_hashTable != null)
                    {
                        _hashTable.Clear();
                        _hashTable = null;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn(string.Format("get folders failed, parent folder url: {0}", folderServerRelativeUrl), e);
                }
                subFolders.Add(AveObjectModelConstant.ChildrenProperties, subFolderList);
                return subFolders;
            }
        }

        protected virtual void LoadFiles(AveClientContext context, Folder folder, string listName)
        {
            if (string.IsNullOrEmpty(listName))
            {
                context.Load(folder, f => f.Files);
            }
            else
            {
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    //if (folder.Files.Count > 0)
                    //{
                    using (excepScope.StartTry())
                    {
                        context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.ListItemAllFields,
                                                                                       file => file.CheckedOutByUser,
                                                                                       file => file.Author,
                                                                                       file => file.ModifiedBy));
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.CheckedOutByUser,
                                                                                       file => file.Author,
                                                                                       file => file.ModifiedBy));
                    }
                }
            }
        }
        public virtual Dictionary<string, object> GetFiles(string webServerRelativeUrl, string listName, string folderServerRelativeUrl)
        {
            Dictionary<string, object> files = new Dictionary<string, object>();
            List<Dictionary<string, object>> fileList = new List<Dictionary<string, object>>();
            Folder folder = null;
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    context.Load(folder);
                    context.Load(folder.ParentFolder, f => f.ServerRelativeUrl);

                    LoadFiles(context, folder, listName);
                    context.ExecuteQuery();
                    foreach (ClientFile file in folder.Files)
                    {
                        Dictionary<string, object> fileProp = new Dictionary<string, object>();
                        fileProp["Exists"] = true;
                        fileProp["ListName"] = listName;
                        AssembleFileProperties(fileProp, file, webServerRelativeUrl, file.ListItemAllFields);
                        fileProp["Versions"] = new List<Dictionary<string, object>>(); // need to fill it later
                        fileList.Add(fileProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn(string.Format("get files failed, parent folder url: {0}", folderServerRelativeUrl), e);
                }
                files.Add(AveObjectModelConstant.ChildrenProperties, fileList);
                return files;
            }
        }
        public virtual Dictionary<string, object> GetListTemplates(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> listTemplates = new Dictionary<string, object>();
                List<Dictionary<string, object>> listTemplateList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.ListTemplates);
                context.ExecuteQuery();
                foreach (ListTemplate listTemplate in web.ListTemplates)
                {
                    Dictionary<string, object> listTemplateProperties = new Dictionary<string, object>();
                    CopyProperty(listTemplateProperties, listTemplate);
                    listTemplateProperties["Type"] = listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateProperties["Type_Client"] = (int)listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateList.Add(listTemplateProperties);
                }
                listTemplates.Add(AveObjectModelConstant.ChildrenProperties, listTemplateList);
                return listTemplates;
            }
        }
        public virtual Dictionary<string, object> GetAvailableFields(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.AvailableFields);
                context.ExecuteQuery();
                Dictionary<string, object> availableFieldsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> availableFieldList = new List<Dictionary<string, object>>();
                foreach (Field field in web.AvailableFields)
                {
                    Dictionary<string, object> availableFieldProperties = new Dictionary<string, object>();
                    CopyProperty(availableFieldProperties, field);
                    availableFieldList.Add(availableFieldProperties);
                }
                availableFieldsProperties.Add(AveObjectModelConstant.ChildrenProperties, availableFieldList);
                return availableFieldsProperties;
            }
        }
        public virtual Dictionary<string, object> GetAvailableContentTypes(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                List<Dictionary<string, object>> availableContentTypes = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.AvailableContentTypes);
                context.ExecuteQuery();
                foreach (ContentType c in web.AvailableContentTypes)
                {
                    Dictionary<string, object> availableContentTypeProperties = new Dictionary<string, object>();
                    CopyProperty(availableContentTypeProperties, c);
                    availableContentTypeProperties["Id"] = c.Id.ToString();
                    availableContentTypes.Add(availableContentTypeProperties);
                }
                returnInfo.Add("ChildrenProperties", availableContentTypes);
                web = null;
                return returnInfo;
            }
        }
        public virtual Dictionary<string, object> GetSiteGroups(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                context.Load(web.SiteGroups, tempGroups => tempGroups.IncludeWithDefaultProperties(temp => temp.Owner.Id, temp => temp.Owner.PrincipalType));
                context.ExecuteQuery();
                Dictionary<string, object> siteGroups = new Dictionary<string, object>();
                List<Dictionary<string, object>> siteGroupList = new List<Dictionary<string, object>>();
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                foreach (Group siteGroup in web.SiteGroups)
                {
                    Dictionary<string, object> siteGroupProperties = GetGroupProperties(webTrimObj, context, siteGroup, true);
                    siteGroupList.Add(siteGroupProperties);
                }
                siteGroups.Add(AveObjectModelConstant.ChildrenProperties, siteGroupList);
                return siteGroups;
            }
        }

        public virtual Dictionary<string, object> GetEnsureUser(string webServerRelativeUrl, string loginName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                string newLoginName = loginName;
                if (WrapperConfiguration.BPOS_S.SearchPrincipal)//low performance.It's better not to search principal.
                {
                    IList<PrincipalInfo> searchResult = Utility.SearchPrincipals(context, web, loginName, PrincipalType.All, PrincipalSource.All, null, 5000);
                    context.ExecuteQuery();
                    PrincipalInfo principal = searchResult.First(prin => prin.DisplayName.Equals(loginName, StringComparison.Ordinal));
                    newLoginName = principal.LoginName;
                }
                User ensureUser = web.EnsureUser(newLoginName);
                context.Load(ensureUser);
                context.ExecuteQuery();
                Dictionary<string, object> ensureUserProperties = new Dictionary<string, object>();
                CopyProperty(ensureUserProperties, ensureUser);
                ConvertUserIdInfo(ensureUserProperties);
                ensureUserProperties["Name"] = ensureUser.Title;
                return ensureUserProperties;
            }
        }

        public virtual void ConvertUserIdInfo(Dictionary<string, object> userProperties)
        {

        }

        public virtual Dictionary<string, object> GetCatalog(string webServerRelativeUrl, int typeCatalog)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.GetCatalog(typeCatalog);
                this.LoadList(context, list);
                Dictionary<string, object> listProperties = new Dictionary<string, object>();
                CopyProperty(listProperties, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                CopyProperty(rootFolderProp, list.RootFolder);
                listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return listProperties;
            }
        }
        public virtual Dictionary<string, object> GetAvailableWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WebTemplateCollection webTemplateCollection = web.GetAvailableWebTemplates(lcid, doIncludeCrossLanguage);
                context.Load(webTemplateCollection);
                context.ExecuteQuery();
                Dictionary<string, object> webTemplates = new Dictionary<string, object>();
                List<Dictionary<string, object>> webTemplateList = new List<Dictionary<string, object>>();
                foreach (WebTemplate webTemplate in webTemplateCollection)
                {
                    Dictionary<string, object> webTemplateProperties = new Dictionary<string, object>();
                    CopyProperty(webTemplateProperties, webTemplate);
                    webTemplateList.Add(webTemplateProperties);
                }
                webTemplates["ChildrenProperties"] = webTemplateList;
                return webTemplates;
            }
        }

        public virtual Dictionary<string, object> GetRoleAssignments(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentColProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleAssignmentCollection roleAssignmentCol = null;
                string level = string.Empty;
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        level = "web";
                        roleAssignmentCol = web.RoleAssignments;
                        break;
                    case "list.roleAssignments":
                        level = "list";
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        roleAssignmentCol = list.RoleAssignments;
                        break;
                    case "item.roleAssignments":
                        level = "item";
                        //List list1 = web.Lists.GetByTitle(listTitle);
                        List list1 = null;
                        if (listId != Guid.Empty)
                        {
                            list1 = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list1 = web.Lists.GetByTitle(listTitle);
                        }
                        ListItem listItem = list1.GetItemById(itemId);
                        roleAssignmentCol = listItem.RoleAssignments;
                        break;
                }
                try
                {
                    context.Load(roleAssignmentCol, roles => roles.IncludeWithDefaultProperties(r => r.RoleDefinitionBindings, r => r.Member));
                    context.ExecuteQuery();
                }
                catch (ServerUnauthorizedAccessException ex)
                {
                    SecurityTrimObject trimObj = null;
                    switch (level)
                    {
                        case "web":
                            trimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                            break;
                        case "list":
                            //List list = web.Lists.GetByTitle(listTitle);
                            List list = null;
                            if (listId != Guid.Empty)
                            {
                                list = web.Lists.GetById(listId);
                            }
                            else
                            {
                                list = web.Lists.GetByTitle(listTitle);
                            }
                            context.Load(list, l => l.Id);
                            context.ExecuteQuery();
                            SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                            trimObj = webTrimObj.GetList(list.Id, listTitle);
                            break;
                        case "item":
                            //List list1 = web.Lists.GetByTitle(listTitle);
                            List list1 = null;
                            if (listId != Guid.Empty)
                            {
                                list1 = web.Lists.GetById(listId);
                            }
                            else
                            {
                                list1 = web.Lists.GetByTitle(listTitle);
                            }
                            ListItem listItem = list1.GetItemById(itemId);
                            context.Load(list1, l => l.Id);
                            context.Load(listItem, item => item.DisplayName);
                            context.ExecuteQuery();
                            SecurityTrimObject parentWebTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                            SecurityTrimObject listTrimObj = parentWebTrimObj.GetList(list1.Id, listTitle);
                            trimObj = listTrimObj.GetListItem(itemId, listItem.DisplayName);
                            break;
                    }
                    trimObj.TrimmedProperties["RoleAssignments"] = ex.Message;
                    return roleAssignmentColProperties;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Load RoleAssignments Error, Error Message: {0}", e.ToString());
                    return roleAssignmentColProperties;
                }
                AssembleRoleAssignmetsProperites(roleAssignmentColProperties, roleAssignmentCol);
                return roleAssignmentColProperties;
            }
        }

        public virtual Dictionary<string, object> GetRoleDefinitions(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleDefinitionColProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web.RoleDefinitions);
                context.ExecuteQuery();
                AssembleRoleDefinitionsProperties(roleDefinitionColProperties, webServerRelativeUrl, web.RoleDefinitions);
                return roleDefinitionColProperties;
            }
        }

        public virtual Dictionary<string, object> GetUserSolutions()
        {
            // SolutionCatalog = 0x79
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> solutionProperties = new Dictionary<string, object>();
                List solutionGallery = context.Site.GetCatalog(0x79);
                ListItemCollection solutionItems = solutionGallery.GetItems(CamlQuery.CreateAllItemsQuery());
                LoadItemsProperty(context, solutionItems);
                context.ExecuteQuery();
                List<Dictionary<string, object>> solutionList = new List<Dictionary<string, object>>();
                foreach (var tempItem in solutionItems)
                {
                    var itemProperties = new Dictionary<string, object>();
                    GetItemDic(itemProperties, tempItem);
                    if (itemProperties.ContainsKey("Status") && itemProperties["Status"] != null)
                    {
                        itemProperties["Status"] = (itemProperties["Status"] as FieldLookupValue).LookupValue;
                    }
                    if (itemProperties.ContainsKey("Hash") && itemProperties.ContainsKey("SolutionHash") && !itemProperties["Hash"].ToString().StartsWith(itemProperties["SolutionHash"].ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    solutionList.Add(itemProperties);
                }
                solutionProperties[AveObjectModelConstant.ChildrenProperties] = solutionList;
                return solutionProperties;
            }
        }

        public virtual Dictionary<string, object> GetAlerts(string webServerRelativeUrl)
        {
            return null;
        }

        public virtual Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        {
            //using (AveClientContext context = CreateContext())
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> Fields = new List<Dictionary<string, object>>();
                ContentTypeCollection contentTypes = this.GetContentTypesWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                context.ExecuteQuery();
                AssembleContentTypesProperties(contentTypeProperties, contentTypes);
                return contentTypeProperties;
            }
        }

        public virtual Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fieldsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> fieldList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                FieldCollection fieldCollection = null;

                switch (fieldSource)
                {
                    case "web.fields":
                        fieldCollection = web.Fields;
                        break;
                    case "web.availableFields":
                        fieldCollection = web.AvailableFields;
                        break;
                    case "list.fields":
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        fieldCollection = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listTitle, listId, contentTypeSource, id);
                        fieldCollection = contentType.Fields;
                        break;
                    default:
                        break;
                }
                context.Load(fieldCollection);
                context.ExecuteQuery();
                fieldsProperties["SchemaXml"] = fieldCollection.SchemaXml;

                foreach (Field field in fieldCollection)
                {
                    Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                    AssembleSingleFieldProperties(fieldProperties, field);
                    fieldList.Add(fieldProperties);
                }
                fieldsProperties.Add(AveObjectModelConstant.ChildrenProperties, fieldList);
                return fieldsProperties;
            }
        }

        public virtual Dictionary<string, object> GetFieldLinks(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string contentTypeId, string contentTypeSource)
        {
            using (AveClientContext context = CreateContext())
            {
                ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listTitle, listId, contentTypeSource, contentTypeId);
                //context.Load(contentType, c => c.FieldLinks);
                context.ExecuteQuery();
                FieldLinkCollection fieldLinks = contentType.FieldLinks;
                Dictionary<string, object> fieldLinksProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> fieldLinksList = new List<Dictionary<string, object>>();
                foreach (FieldLink fl in fieldLinks)
                {
                    Dictionary<string, object> fieldLinkProp = new Dictionary<string, object>();
                    CopyProperty(fieldLinkProp, fl);
                    fieldLinkProp["DisplayName"] = fl.Name;
                    fieldLinkProp["SchemaXml"] = GetFieldLinkSchemaXml(fl);
                    fieldLinksList.Add(fieldLinkProp);
                }
                fieldLinksProp[AveObjectModelConstant.ChildrenProperties] = fieldLinksList;
                return fieldLinksProp;
            }
        }

        private string GetFieldLinkSchemaXml(FieldLink fl)
        {
            XmlElement node = new XmlDocument().CreateElement("FieldRef");
            node.SetAttribute("ID", fl.Id.ToString());
            node.SetAttribute("Name", fl.Name);
            node.SetAttribute("Required", fl.Required.ToString());
            node.SetAttribute("Hidden", fl.Hidden.ToString());
            return node.OuterXml;
        }

        public virtual Dictionary<string, object> GetFeatures(string serverRelativeUrl, string featuresSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> features = new Dictionary<string, object>();
                List<Dictionary<string, object>> featuresList = new List<Dictionary<string, object>>();
                FeatureCollection featureCollection = null;
                switch (featuresSource)
                {
                    case "site.features":
                        context.Load(context.Site, f => f.Features);
                        context.ExecuteQuery();
                        featureCollection = context.Site.Features;
                        break;
                    case "web.features":
                        Web web = context.Site.OpenWeb(serverRelativeUrl);
                        context.Load(web, f => f.Features);
                        context.ExecuteQuery();
                        featureCollection = web.Features;
                        break;
                    default:
                        break;
                }
                foreach (Feature f in featureCollection)
                {
                    Dictionary<string, object> featurePropteries = new Dictionary<string, object>();
                    featurePropteries = ObjectToDicValue(f, typeof(Feature));
                    featuresList.Add(featurePropteries);
                }
                features.Add(AveObjectModelConstant.ChildrenProperties, featuresList);
                return features;
            }
        }

        public virtual Dictionary<string, object> GetEventReceiverDefinitions(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource)
        {
            return null;
        }
        public virtual Dictionary<string, object> GetSiteEventReceiverDefinitions(string siteServerRelativeUrl, string eventReceiverDefSource)
        {
            return null;
        }
        public virtual Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> navigationProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> navigationList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                NavigationNodeCollection navigationNodeCol = null;
                switch (navigationNodeSource)
                {
                    case "topNavigationBar":
                        navigationNodeCol = web.Navigation.TopNavigationBar;
                        break;
                    case "quickLaunch":
                        navigationNodeCol = web.Navigation.QuickLaunch;
                        break;
                    case "children":
                        NavigationNode navNode = new NavigationNode(context, navProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
                        navigationNodeCol = navNode.Children;
                        break;
                    default:
                        break;
                }
                context.Load(navigationNodeCol);
                context.ExecuteQuery();
                foreach (NavigationNode navigation in navigationNodeCol)
                {
                    Dictionary<string, object> navigationProperty = new Dictionary<string, object>();
                    CopyProperty(navigationProperty, navigation);
                    navigationProperty["Id" + AveObjectModelConstant.ObjectPropertySuffix] = navigation.Path;
                    if (!string.IsNullOrEmpty(navigation.Url))
                    {
                        if (navigation.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                            || navigation.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                            || navigation.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            navigationProperty["IsExternal"] = !navigation.Url.StartsWith(this.WebAppName, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            navigationProperty["IsExternal"] = false;
                        }
                    }
                    navigationList.Add(navigationProperty);
                }
                navigationProperties.Add(AveObjectModelConstant.ChildrenProperties, navigationList);
                return navigationProperties;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "lwp and wpd are variables")]
        public virtual Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, string appWebFulUrl = null)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webpartManagerProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                LimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager((PersonalizationScope)personalizationScope);
                context.Load(limitedWebPartManager, lwp => lwp.WebParts.IncludeWithDefaultProperties(wpd => wpd.WebPart));
                context.ExecuteQuery();
                AssembleWebPartManagerProperties(webpartManagerProperties, limitedWebPartManager);
                return webpartManagerProperties;
            }
        }

        public virtual Dictionary<string, object> GetUserProfileByName(string accountName, bool isOnlineSite)
        {
            return mWebServiceRequest.GetUserProfileByName(accountName, isOnlineSite);
        }

        public virtual Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            return mWebServiceRequest.GetFileVersionStream(webServerRelativeUrl, fileServerRelativeUrl, fileVerionServerRelativeUrl, versionId);
        }

        public virtual Dictionary<string, object> GetUserProfileManager()
        {
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> GetAudienceManager()
        {
            throw new NotImplementedException();
        }

        public virtual Guid GetListId(Guid webId, string listTitle)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    List list = web.Lists.GetByTitle(listTitle);
                    context.Load(list, l => l.Id);
                    context.ExecuteQuery();
                    return list.Id;
                }
            }
            catch (Exception ex)//when lookup list havn't been restored, there is going to be an exception
            {
                mLogger.Warn("Can't Get list:{0} Id.Error Message:{1}", listTitle, ex.ToString());
                return Guid.Empty;
            }
        }

        public virtual IList<Dictionary<string, object>> GetManagedThemes()
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> themes = new Dictionary<string, object>();
                List<Dictionary<string, object>> themeList = new List<Dictionary<string, object>>();
                Web web = context.Site.RootWeb;
                List list = web.GetCatalog((int)ListTemplateType.ThemeCatalog);
                FileCollection files = list.RootFolder.Files;
                context.Load(files);
                context.ExecuteQuery();
                foreach (ClientFile file in files)
                {
                    Dictionary<string, object> fileProp = new Dictionary<string, object>();
                    CopyProperty(fileProp, file);
                    themeList.Add(fileProp);
                }
                return themeList;
            }
        }

        public virtual Dictionary<string, object> GetPublishingWeb(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public virtual string GetApplicationPath(string serverRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string input, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> principalInfoDic = new Dictionary<string, object>();
                bool isInvalidUser = AveWebServiceRequest.CheckInvalidUser(WebAppName, webServerRelativeUrl, input, mObj);
                if (isInvalidUser)
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    Microsoft.SharePoint.Client.ClientResult<PrincipalInfo> info = Utility.ResolvePrincipal(context, web, input, (PrincipalType)scopes, (PrincipalSource)sources, null, inputIsEmailOnly);
                    context.ExecuteQuery();
                    PrincipalInfo principalInfo = info.Value;
                    if (principalInfo != null)
                    {
                        AssemblePrincipalProperty(principalInfoDic, principalInfo);
                    }
                }
                return principalInfoDic;
            }
        }

        protected void AssemblePrincipalProperty(Dictionary<string, object> properties, PrincipalInfo principalInfo)
        {
            properties.Add("Department", principalInfo.Department);
            properties.Add("DisplayName", principalInfo.DisplayName);
            properties.Add("Email", principalInfo.Email);
            properties.Add("JobTitle", principalInfo.JobTitle);
            properties.Add("LoginName", principalInfo.LoginName);
            properties.Add("Mobile", principalInfo.Mobile);
            properties.Add("PrincipalId", principalInfo.PrincipalId);
            properties.Add("PrincipalType", (int)principalInfo.PrincipalType);
            properties.Add("SIPAddress", principalInfo.SIPAddress);
        }

        public virtual Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> principalInfos = new Dictionary<string, object>();
                List<Dictionary<string, object>> infoList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                IList<PrincipalInfo> infos = Utility.SearchPrincipals(context, web, input, (PrincipalType)scopes, (PrincipalSource)sources, null, maxCount);
                context.ExecuteQuery();
                foreach (PrincipalInfo info in infos)
                {
                    Dictionary<string, object> infoDic = new Dictionary<string, object>();
                    AssemblePrincipalProperty(infoDic, info);
                    infoList.Add(infoDic);
                }
                principalInfos.Add("Principals", infoList);
                return principalInfos;
            }
        }
        public virtual Dictionary<string, object> GetContentTypesProperties(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> contentTypesProp, string contentTypeSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> Fields = new List<Dictionary<string, object>>();
                ContentTypeCollection contentTypes = this.GetContentTypesWithSimpleProperties(webServerRelativeUrl, listName, listId, contentTypeSource);
                context.ExecuteQuery();
                List<Dictionary<string, object>> properties = contentTypesProp[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                int i = 0;
                foreach (ContentType contentType in contentTypes)
                {
                    Dictionary<string, object> contentTypeProp = properties[i];
                    i++;
                    if (contentTypeProp.ContainsKey("Name") && contentType.Name.Equals(contentTypeProp["Name"].ToString()))
                    {
                        //contentTypeProp["SchemaXml"] = contentType.SchemaXml;
                        contentTypeProp["ReadOnly"] = contentType.ReadOnly;
                        contentTypeProp["ParentId"] = contentType.Parent.Id.ToString();
                        //XmlDocument doc = new XmlDocument();
                        //doc.InnerXml = contentType.SchemaXml;
                        //XmlElement ctElement = doc.FirstChild as XmlElement;
                        //string attributeValue = ctElement.GetAttribute("Sealed");
                        //if (!string.IsNullOrEmpty(attributeValue))
                        //{
                        //    contentTypeProp["Sealed"] = Convert.ToBoolean(attributeValue);
                        //}
                    }
                }
                return contentTypesProp;
            }
        }

        public virtual Dictionary<string, object> GetListsProperties(string webServerRelativeUrl, Dictionary<string, object> listsProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web.Lists, webLists => webLists.Include(l => l.Title, l => l.RootFolder, l => l.AllowContentTypes, l => l.ContentTypesEnabled));
                context.ExecuteQuery();
                List<Dictionary<string, object>> lists = listsProp[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                int i = 0;
                foreach (List list in web.Lists)
                {
                    Dictionary<string, object> listProperties = lists[i];
                    i++;
                    if (listProperties.ContainsKey("Title") && list.Title.Equals(listProperties["Title"].ToString()))
                    {
                        Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                        CopyProperty(rootFolderProp, list.RootFolder);
                        rootFolderProp["Url"] = list.RootFolder.ServerRelativeUrl.Substring(webServerRelativeUrl.Length + 1);
                        rootFolderProp["Exists"] = true;
                        listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                        listProperties["AllowContentTypes"] = list.AllowContentTypes;
                        listProperties["ContentTypesEnabled"] = list.ContentTypesEnabled;
                    }
                }
                return listsProp;
            }
        }
        public virtual Dictionary<string, object> GetTaxonomyCatchAllField(string webServerRelativeUrl, string listName, Guid listId)
        {
            if (string.IsNullOrEmpty(listName) && listId == Guid.Empty)
            {
                return null;
            }
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listName);
                }
                FieldCollection fields = list.Fields;
                try
                {
                    context.Load(fields, tempFields => tempFields.IncludeWithDefaultProperties().Where(temp => temp.InternalName == "TaxCatchAll"));
                    context.ExecuteQuery();
                    Dictionary<string, object> taxonomyCatchAllFieldProperties = new Dictionary<string, object>();
                    AssembleSingleFieldProperties(taxonomyCatchAllFieldProperties, fields[0]);
                    return taxonomyCatchAllFieldProperties;
                }
                catch (Exception e)
                {
                    mLogger.Warn("When creating this TaxonomyField, SharePoint didn't create taxonomy catch all field. Error Message:{0}", e.ToString());
                    return null;
                }
            }
        }

        public virtual Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime)
        {
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> GetChanges(Guid termStoreId, TimeSpan sinceTimeAgo)
        {
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType)
        {
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType)
        {
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> GetTaxonomySession()
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTermStores()
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTaxonomyGroups(Guid guid)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTermSets(Guid termStoreId, Guid groupId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTerms(Guid termStoreId, Guid groupId, Guid termSetId, Guid parentTermId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl, bool createIfMissing)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTermGroup(Guid termStoreId, Guid groupId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId)
        {
            return new Dictionary<string, object>();
        }
        public virtual Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable)
        {
            return new Dictionary<string, object>();
        }

        public virtual bool IsTermSetExist(Guid termStoreId, Guid termSetId)
        {
            return false;
        }

        public virtual bool IsTermExist(Guid termStoreId, Guid termSetId)
        {
            return false;
        }

        public virtual string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID)
        {
            throw new NotImplementedException();
        }

        public virtual string GetDescription(Guid termStoreId, Guid termSetId, Guid parentTermId, int lcid)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<int, string> GetAllDescriptions(Guid termStoreId, Guid termSetId, Guid parentTermId, Collection<int> lcids)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> relatedfields = new Dictionary<string, object>();
                List<Dictionary<string, object>> relatedFieldPropertiesList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                RelatedFieldCollection fieldCollection = list.GetRelatedFields();
                context.Load(fieldCollection);
                context.ExecuteQuery();
                foreach (RelatedField field in fieldCollection)
                {
                    Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                    CopyProperty(fieldProperties, field);
                    relatedFieldPropertiesList.Add(fieldProperties);
                }
                relatedfields.Add(AveObjectModelConstant.ChildrenProperties, relatedFieldPropertiesList);
                return relatedfields;
            }
        }
        //public virtual Dictionary<string, object> GetListAssociastedProperty(string webServerRelativeUrl, string listTitle)
        //{
        //    throw new NotImplementedException();
        //}
        public virtual Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            throw new NotImplementedException();
        }
        public virtual List<string> GetSiteEnabledHelpCollections()
        {
            throw new NotImplementedException();
        }
        public virtual bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            throw new NotImplementedException();
        }
        public virtual List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetListEditViewSettingProperties(string webServerRelativeUrl, String listTitle, Guid listId, Guid viewId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetListAccessRequestsSettingProperties(String webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }
        public virtual List<Dictionary<string, object>> GetDisplayGroupsForSite()
        {
            throw new NotImplementedException();
        }
        public virtual List<Dictionary<string, object>> GetKeyWords()
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> listTemplates = new Dictionary<string, object>();
                List<Dictionary<string, object>> listTemplateList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ListTemplateCollection templates = context.Site.GetCustomListTemplates(web);
                context.Load(templates);
                context.ExecuteQuery();
                foreach (ListTemplate listTemplate in templates)
                {
                    Dictionary<string, object> listTemplateProperties = new Dictionary<string, object>();
                    CopyProperty(listTemplateProperties, listTemplate);
                    listTemplateProperties["AveBaseType"] = listTemplateProperties["BaseType"];
                    listTemplateProperties["Type"] = listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateProperties["Type_Client"] = listTemplateProperties["ListTemplateTypeKind"];
                    listTemplateList.Add(listTemplateProperties);
                }
                listTemplates.Add(AveObjectModelConstant.ChildrenProperties, listTemplateList);
                return listTemplates;
            }
        }

        public virtual Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            throw new NotImplementedException();
        }

        public virtual bool DoesUserHavePermissions(string webServerRelativeUrl, ulong permissionMask)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                BasePermissions permissions = new BasePermissions();
                //permissions.Set((PermissionKind)permissionMask);
                ulong num = permissionMask;
                num = num >> 0x20;
                ulong num2 = permissionMask;
                num2 &= 0xffffffffL;
                var m_high = (uint)num;
                var m_low = (uint)num2;
                AveAssemblyUtility.SetFieldValue(permissions, "m_high", m_high);
                AveAssemblyUtility.SetFieldValue(permissions, "m_low", m_low);
                ClientResult<bool> doesUserHavePermissions = web.DoesUserHavePermissions(permissions);
                context.ExecuteQuery();
                return doesUserHavePermissions.Value;
            }
        }
        public virtual Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl, int compatibilityLevel)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "openxmlformats is a part of xml")]
        public virtual Dictionary<string, object> OpenThmxTheme(string fileServerRelativeUrl)
        {
            Dictionary<string, object> themeProp = new Dictionary<string, object>();
            Stream themeStream = null;
            try
            {
                themeStream = this.GetFileStream(string.Empty, fileServerRelativeUrl, string.Empty);
            }
            catch (Exception e)
            {
                mLogger.Debug(AveClientOMRequestResource.OpenThmxThemeError, fileServerRelativeUrl, e.ToString());
            }
            if (themeStream != null)
            {
                using (themeStream)
                {
                    Package package = Package.Open(themeStream, FileMode.Open, FileAccess.Read);
                    PackagePart part = package.GetPart(new Uri("/theme/theme/themeManager.xml", UriKind.Relative));
                    PackagePart themePart = null;
                    if (string.Compare(part.ContentType, "application/vnd.openxmlformats-officedocument.themeManager+xml", StringComparison.Ordinal) == 0)
                    {
                        foreach (PackageRelationship relationship in part.GetRelationshipsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme"))
                        {
                            if (relationship.TargetMode != TargetMode.Internal)
                            {
                                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Request_OpenThemeFailed);
                            }
                            themePart = package.GetPart(PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri));
                            break;
                        }
                    }
                    if (themePart != null)
                    {
                        try
                        {
                            XmlDocument themeDocument = new XmlDocument();
                            using (Stream stream = themePart.GetStream())
                            {
                                themeDocument.Load(stream);
                            }
                            if (themeDocument.ChildNodes.Count > 1)
                            {
                                XmlNode node = themeDocument.ChildNodes[1];
                                string name = node.Attributes["name"].Value;
                                themeProp["Name"] = name;
                                themeProp["ServerRelativeUrl"] = fileServerRelativeUrl;
                                this.GetThemeProperties(node.FirstChild, themeProp);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(string.Format("Open theme failed. Error message:{0}.", e.ToString()));
                        }
                    }
                }
            }
            return themeProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "dk is a part of keys")]
        protected void GetThemeProperties(XmlNode parent, Dictionary<string, object> themeProp)
        {
            if (parent != null)
            {
                foreach (XmlNode node in parent.ChildNodes)
                {
                    if (node.Name.Equals("a:clrScheme"))
                    {
                        foreach (XmlNode color in node.ChildNodes)
                        {
                            #region color
                            switch (color.Name)
                            {
                                case "a:dk1":
                                    themeProp["DarkColor1"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:lt1":
                                    themeProp["LightColor1"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:dk2":
                                    themeProp["DarkColor2"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:lt2":
                                    themeProp["LightColor2"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent1":
                                    themeProp["AccentColor1"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent2":
                                    themeProp["AccentColor2"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent3":
                                    themeProp["AccentColor3"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent4":
                                    themeProp["AccentColor4"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent5":
                                    themeProp["AccentColor5"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:accent6":
                                    themeProp["AccentColor6"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:hlink":
                                    themeProp["HyperlinkColor"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                case "a:folHlink":
                                    themeProp["FollowedHyperlinkColor"] = color.FirstChild.Attributes["val"].Value;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                    }
                    else if (node.Name.Equals("a:fontScheme"))
                    {
                        foreach (XmlNode font in node.ChildNodes)
                        {
                            #region font
                            switch (font.Name)
                            {
                                case "a:majorFont":
                                    themeProp["MajorFont"] = font.FirstChild.Attributes["typeface"].Value;
                                    break;
                                case "a:minorFont":
                                    themeProp["MinorFont"] = font.FirstChild.Attributes["typeface"].Value;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                    }
                }
            }
        }

        public virtual bool GetSiteRssSetting()
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetNavigation(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        //有些field在XML里会有RelatedField这项，还原这个field会一并把RelatedField也还回去。
        //这个方法就是单独取一个RelatedField的属性。
        public virtual Dictionary<string, object> GetRelatedFieldProperties(string webServerRelativeUrl, string fieldName, string fieldSource, string listTitle, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Dictionary<string, object> fieldProperties = null;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        fields = list.Fields;
                        break;
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    default:
                        break;
                }
                if (fields != null)
                {
                    context.Load(fields, tempFields => tempFields.IncludeWithDefaultProperties().Where(temp => temp.InternalName == fieldName));
                    context.ExecuteQuery();
                    if (fields.Count != 0)
                    {
                        fieldProperties = new Dictionary<string, object>();
                        AssembleSingleFieldProperties(fieldProperties, fields[0]);
                    }
                }
                return fieldProperties;
            }
        }

        public virtual Dictionary<string, object> GetFieldPropertiesById(string webServerRelativeUrl, Guid fieldId, string fieldSource, string listTitle, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                Field field = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        field = list.Fields.GetById(fieldId);
                        break;
                    case "web.fields":
                        field = web.Fields.GetById(fieldId);
                        break;
                    default:
                        break;
                }
                try
                {
                    context.Load(field);
                    context.ExecuteQuery();
                    AssembleSingleFieldProperties(fieldProperties, field);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("An error occurred while get field by ID.error message: {0}", ex.Message);
                }
                return fieldProperties;
            }
        }
        #endregion

        #region  Add
        protected virtual ListItem AddListItem(ClientContext context, List list, string folderUrl, int objectType, string leafName)
        {
            context.ValidateOnClient = false;
            ListItemCreationInformation itemCrtInfo = new ListItemCreationInformation();
            itemCrtInfo.FolderUrl = folderUrl;
            itemCrtInfo.UnderlyingObjectType = (FileSystemObjectType)objectType;
            itemCrtInfo.LeafName = leafName;
            return list.AddItem(itemCrtInfo);
        }

        protected ListItem AddDiscussionBoardItem(ClientRuntimeContext context, List list, string title, int objectType, ListItem parentItem)
        {
            if (objectType == 1)
            {
                return Utility.CreateNewDiscussion(context, list, title);
            }
            return Utility.CreateNewDiscussionReply(context, parentItem);
        }

        public virtual Dictionary<string, object> AddItem(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int parentId, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties, bool isDiscussion)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listName);
                }
                ListItem item = null;
                context.Load(list);
                context.ExecuteQuery();
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                if (list.TemplateFeatureId.ToString().Equals("0be49fe9-9bc9-409d-abf9-702753bd878d") && (int)itemProperties["FileSystemObjectType"] == 1)//说明所建Slide Library，并且Item类型是folder
                {
                    string actualName = folderUrl.Substring(webServerRelativeUrl.Length + 1);
                    AveWebServiceRequest.AddSlideFolder(WebAppName, webServerRelativeUrl, listName, folderUrl, leafName, mObj);
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = string.Format(
                      "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query></View>",
                      folderUrl + "/" + leafName);
                    ListItemCollection listItems = list.GetItems(camlQuery);
                    context.Load(listItems, its => its.IncludeWithDefaultProperties(t => t.HasUniqueRoleAssignments));
                    context.ExecuteQuery();
                    if (listItems != null && listItems.Count == 1)
                    {
                        item = listItems[0];
                    }
                }
                else
                {
                    switch (list.BaseTemplate)
                    {
                        case (int)ListTemplateType.DiscussionBoard:
                            if (!isDiscussion)
                            {
                                item = AddListItem(context, list, folderUrl, underlyingObjectType, leafName);
                                break;
                            }
                            ListItem parentItem = parentId > 0 ? list.GetItemById(parentId) : null;
                            item = AddDiscussionBoardItem(context, list, leafName, underlyingObjectType, parentItem);
                            break;
                        default:
                            item = AddListItem(context, list, folderUrl, underlyingObjectType, leafName);
                            break;
                    }
                }
                if (itemProperties.ContainsKey("ChangedFieldValues"))
                {
                    foreach (KeyValuePair<string, object> pair in itemProperties["ChangedFieldValues"] as Dictionary<string, object>)
                    {
                        item[pair.Key] = pair.Value;
                    }
                }
                string updateMethod = itemProperties[AveObjectModelConstant.UpdateMethodName] as string;
                switch (updateMethod)
                {
                    case "Update":
                        item.Update();
                        LoadItemProperty(context, item);
                        context.ExecuteQuery();
                        GetItemDic(returnInfo, item);
                        break;
                    default:
                        break;
                }
                return returnInfo;
            }
        }

        public virtual string AddAttachmentNow(string siteUrl, string webRelativeUrl, string listName, string itemId, string fileName, byte[] attachment)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource)
        {
            using (AveClientContext context = CreateContext())
            {
                GroupCreationInformation gci = new GroupCreationInformation();
                gci.Description = description;
                gci.Title = groupName;
                Web web = context.Site.OpenWeb(webRelativeUrl);
                Group group = web.SiteGroups.Add(gci);
                context.Load(group);
                context.ExecuteQuery();
                Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                CopyProperty(groupProperties, group);
                groupProperties["Name"] = groupProperties["Title"];
                return groupProperties;
            }
        }

        public virtual Dictionary<string, object> AddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                WebCreationInformation wci = new WebCreationInformation();
                wci.Url = webUrl.Trim(' ');
                wci.Title = title;
                wci.Description = description;
                wci.Language = (int)language;
                wci.UseSamePermissionsAsParentSite = useSamePermissionsAsParentSite;
                wci.WebTemplate = webTemplate;
                Web parentWeb = context.Site.OpenWeb(parentWebRelativeUrl);
                Web newWeb = parentWeb.Webs.Add(wci);
                try //SAAS-651
                {
                    webProperties = GetWebProperties(context, newWeb, context.Url, mSiteRelativeUrl, false);
                }
                catch (Exception te)
                {
                    //ADO-167220 Merge CI,经测试 retry逻辑即可保证Web成功创建，因此不需要像外围那样多次执行ExecuteQuery
                    if (te is ServerException && (te as ServerException).ServerErrorCode == AveStandardErrorCode.COR_E_APPLICATION)
                    {
                        parentWeb = context.Site.OpenWeb(parentWebRelativeUrl);
                        newWeb = parentWeb.Webs.Add(wci);
                        webProperties = GetWebProperties(context, newWeb, context.Url, mSiteRelativeUrl, false);
                    }
                    else if (IsTimeOutServerException(te))
                    {
                        Web destweb = null;
                        string destWebRelativeUrl = null;
                        destWebRelativeUrl = string.IsNullOrEmpty(parentWebRelativeUrl) ? mSiteRelativeUrl.TrimEnd('/') + "/" + webUrl : mSiteRelativeUrl.TrimEnd('/') + "/" + parentWebRelativeUrl + "/" + webUrl;
                        destweb = context.Site.OpenWeb(destWebRelativeUrl);
                        webProperties = GetWebProperties(context, destweb, context.Url, mSiteRelativeUrl, false);
                    }
                    else
                    {
                        throw;
                    }
                }
                return webProperties;
            }
        }
        public virtual Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, Guid featureId, int webTemplateType)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ListCreationInformation newList = new ListCreationInformation();
                newList.Description = description;
                newList.Title = title;
                newList.TemplateType = webTemplateType;
                newList.TemplateFeatureId = featureId;
                List list = web.Lists.Add(newList);
                TryLoadList(context, web, ref list, title);
                Dictionary<string, object> prop = new Dictionary<string, object>();
                CopyProperty(prop, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return prop;
            }
        }

        public virtual Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ListCreationInformation newList = new ListCreationInformation();
                newList.Description = description;
                newList.Title = title;
                newList.Url = url;
                newList.TemplateFeatureId = new Guid(featureId);
                newList.TemplateType = templateType;
                newList.DocumentTemplateType = string.IsNullOrEmpty(docTemplateType) ? 0 : Convert.ToInt32(docTemplateType);
                newList.QuickLaunchOption = (QuickLaunchOptions)quickLaunchOptions;
                List list = web.Lists.Add(newList);
                TryLoadList(context, web, ref list, title);
                Dictionary<string, object> prop = new Dictionary<string, object>();
                AveObjectCopy.GetObjectBasicProperties(prop, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return prop;
            }
        }

        protected void TryLoadList(AveClientContext context, Web web, ref List list, string title)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.LoadList(context, list)))
                {
                    throw new Exception(mAccessDeniedMessage);
                }
            }
            catch (Exception e)
            {
                if (e is ServerException && e.Message.ToUpperInvariant().Contains("HRESULT: 0X8107140D"))
                {
                    list = web.Lists.GetByTitle(title);
                    this.LoadList(context, list);
                }
                else
                {
                    throw;
                }
            }
        }

        public virtual Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, Dictionary<string, object> dataSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ListCreationInformation newList = new ListCreationInformation();
                newList.Title = title;
                newList.Description = description;
                newList.Url = url;
                newList.DataSourceProperties.Add(AveBDCProperties.LobSystemInstance, dataSource[AveBDCProperties.LobSystemInstance] as string);
                newList.DataSourceProperties.Add(AveBDCProperties.EntityNamespace, dataSource[AveBDCProperties.EntityNamespace] as string);
                newList.DataSourceProperties.Add(AveBDCProperties.Entity, dataSource[AveBDCProperties.Entity] as string);
                newList.DataSourceProperties.Add(AveBDCProperties.SpecificFinder, dataSource[AveBDCProperties.SpecificFinder] as string);
                List list = web.Lists.Add(newList);
                TryLoadList(context, web, ref list, title);
                Dictionary<string, object> prop = new Dictionary<string, object>();
                CopyProperty(prop, list);
                if (list.DataSource != null && list.BaseTemplate == (int)AveListTemplateType.ExternalList)
                {
                    Dictionary<string, object> listDataSource = new Dictionary<string, object>();
                    listDataSource.Add(AveBDCProperties.LobSystemInstance, list.DataSource.Properties[AveBDCProperties.LobSystemInstance]);
                    listDataSource.Add(AveBDCProperties.EntityNamespace, list.DataSource.Properties[AveBDCProperties.EntityNamespace]);
                    listDataSource.Add(AveBDCProperties.Entity, list.DataSource.Properties[AveBDCProperties.Entity]);
                    listDataSource.Add(AveBDCProperties.SpecificFinder, list.DataSource.Properties[AveBDCProperties.SpecificFinder]);
                    prop.Add("DataSource" + AveObjectModelConstant.ObjectPropertySuffix, listDataSource);
                    //ItemCount == 0
                    //prop.Remove("ItemCount");
                }
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return prop;
            }
        }

        public virtual Dictionary<string, object> AddRoleDefinition(string webServerRelativeUrl, Dictionary<string, object> roleDefinitionProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> newRoleDefinitionProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleDefinitionCreationInformation rdci = new RoleDefinitionCreationInformation();
                AveObjectCopy.UpdateObjectBasicPropertiesWithEscape(roleDefinitionProperties, rdci, new string[] { "BasePermissions" });
                rdci.BasePermissions = ConvertULongToBasePerm((ulong)roleDefinitionProperties["BasePermissions"]);
                RoleDefinition roleDefinition = web.RoleDefinitions.Add(rdci);
                context.Load(roleDefinition);
                context.ExecuteQuery();
                CopyProperty(newRoleDefinitionProperties, roleDefinition);
                newRoleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
                return newRoleDefinitionProperties;
            }
        }

        public virtual Dictionary<string, object> AddRoleAssignment(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> roleAssignmentProperties, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {

                Dictionary<string, object> newRoleAssignmentProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Principal principal = null;
                RoleDefinitionBindingCollection roleDefinitionBindingCol = null;
                RoleAssignment roleAssignment = null;
                if (GetRoleAssignment(context.Site, web, roleAssignmentProperties, out principal, out roleDefinitionBindingCol))
                {
                    switch (roleAssignmentsSource)
                    {
                        case "web.roleAssignments":
                            roleAssignment = web.RoleAssignments.Add(principal, roleDefinitionBindingCol);
                            break;
                        case "list.roleAssignments":
                            //List list = web.Lists.GetByTitle(listTitle);
                            List list = null;
                            if (listId != Guid.Empty)
                            {
                                list = web.Lists.GetById(listId);
                            }
                            else
                            {
                                list = web.Lists.GetByTitle(listTitle);
                            }
                            roleAssignment = list.RoleAssignments.Add(principal, roleDefinitionBindingCol);
                            break;
                        case "item.roleAssignments":
                            //List list1 = web.Lists.GetByTitle(listTitle);
                            List list1 = null;
                            if (listId != Guid.Empty)
                            {
                                list1 = web.Lists.GetById(listId);
                            }
                            else
                            {
                                list1 = web.Lists.GetByTitle(listTitle);
                            }
                            ListItem listItem = list1.GetItemById(itemId);
                            roleAssignment = listItem.RoleAssignments.Add(principal, roleDefinitionBindingCol);
                            break;
                    }
                    context.Load(roleAssignment);
                    context.Load(roleAssignment.RoleDefinitionBindings);
                    context.Load(roleAssignment, r => r.Member);
                    context.ExecuteQuery();
                    AssembleRoleAssignmetProperites(newRoleAssignmentProperties, roleAssignment);
                }
                return newRoleAssignmentProperties;
            }
        }

        public virtual Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, Guid listId, int itemId, string leafName, byte[] attachment)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile newFile = null;
                string fileType = Path.GetExtension(urlOfFile);
                if (mSpecialFileList.Contains(fileType))
                {
                    Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    FileCreationInformation fci = new FileCreationInformation();
                    fci.Url = urlOfFile;
                    fci.Content = file;
                    fci.Overwrite = overwrite;
                    newFile = AddFileByAPI(folder.Files, fci);
                }
                else
                {
                    context.ExecuteQuery();
                    MemoryStream stream = new MemoryStream(file);

                    if (urlOfFile.StartsWith("http", StringComparison.OrdinalIgnoreCase) || urlOfFile.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        //urlOfFile = urlOfFile.Substring(WebAppName.Length);
                        Uri fileUri = new Uri(urlOfFile);
                        urlOfFile = fileUri.AbsolutePath;
                    }
                    else if (!string.IsNullOrEmpty(webServerRelativeUrl) && (string.IsNullOrEmpty(folderServerRelativeUrl) || !urlOfFile.Trim('/').StartsWith(folderServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase)) && !urlOfFile.Trim('/').StartsWith(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        if (urlOfFile.Trim('/').IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            if (string.IsNullOrEmpty(folderServerRelativeUrl))
                            {
                                urlOfFile = webServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                            }
                            else
                            {
                                urlOfFile = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                            }
                        }
                        else
                        {
                            urlOfFile = string.Format("{0}/{1}", webServerRelativeUrl.TrimEnd('/'), urlOfFile.TrimStart('/'));
                        }
                    }

                    SaveBinary(urlOfFile, stream, null, true, SaveBinaryCheckMode.Overwrite, context, mObj);
                    newFile = GetFileByAPI(web, urlOfFile);
                }

                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(newFile);
                        context.Load(newFile.ListItemAllFields);
                        context.Load(newFile.CheckedOutByUser);
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(newFile);
                    }
                }
                context.ExecuteQuery();
                fileProperties["Exists"] = true;
                AssembleFileProperties(fileProperties, newFile, webServerRelativeUrl, newFile.ListItemAllFields);
                return fileProperties;
            }
        }
        public virtual Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, string listName, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields, bool? listEnableMinorVersion)
        {
            using (AveClientContext context = CreateContext())
            {
                string serverRelativeUrl = string.Empty;
                if (urlOfFile.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile.Substring(WebAppName.Length);
                }
                else if (urlOfFile.StartsWith(folderServerRelativeUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile;
                }
                else
                {
                    serverRelativeUrl = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                }
                SaveBinary(serverRelativeUrl, file, null, overwrite, SaveBinaryCheckMode.Overwrite, context, mObj);
                return this.GetFile(webServerRelativeUrl, serverRelativeUrl, listName);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of keys")]
        protected virtual void AddParentFolderToCache(ClientContext context, List list, Folder folder, Dictionary<string, object> existFolders, List<Dictionary<string, object>> changeFolderCache)
        {
            if (folder == null)
            {
                return;
            }
            context.Load(folder);
            context.ExecuteQuery();
            string folderUrl = folder.ServerRelativeUrl;
            if (folderUrl.Equals(list.RootFolder.ServerRelativeUrl))
            {
                return;
            }
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            CopyProperty(folderProperties, folder);
            folderProperties["FullUrl"] = folderProperties["ServerRelativeUrl"];
            folderProperties["ChangeType"] = AvePoint.Wrapper.Common.ChangeType.None;
            folderProperties["LeafName"] = folder.Name;
            folderProperties["Versions"] = new List<Dictionary<string, object>>();
            string dirName = folderUrl.Substring(0, folderUrl.LastIndexOf('/'));
            string leafName = folderUrl.Substring(folderUrl.LastIndexOf('/') + 1);
            ListItem item = GetListItemByDirName(context, list, dirName, leafName);
            if (item != null)
            {
                AssembleItemProperties(folderProperties, item);
                folderProperties["DoclibRowId"] = item.Id;
                folderProperties["DocID"] = item.FieldValues.ContainsKey("UniqueId") ? item.FieldValues["UniqueId"] : Guid.Empty;//获得folder的docid
            }
            string serverRelativeUrl = folderProperties["ServerRelativeUrl"].ToString();
            if (!existFolders.ContainsKey(serverRelativeUrl))// && serverRelativeUrl.Trim('/').Equals(rootFolderUrl.Trim('/') + "/" + folder.Name))
            {
                existFolders[serverRelativeUrl] = folderProperties;
                changeFolderCache.Add(folderProperties);
            }
            AddParentFolderToCache(context, list, folder.ParentFolder, existFolders, changeFolderCache);
        }

        public virtual void SaveBinary(string serverRelativeUrl, System.IO.Stream stream, string etag, bool overwriteIfExists, SaveBinaryCheckMode checkMode, ClientContext context, object obj)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.HasPendingRequest)
            {
                throw new ClientRequestException(Resources.GetString("NoDirectHttpRequest"));
            }
            string requestUrl = MakeFullUrl(serverRelativeUrl, context);
            AveWebRequestExecutor webRequestExecutor = context.WebRequestExecutorFactory.CreateWebRequestExecutor(context, requestUrl) as AveWebRequestExecutor;
            webRequestExecutor.RequestKeepAlive = false;
            webRequestExecutor.RequestMethod = "PUT";

            if (checkMode == SaveBinaryCheckMode.ETag)
            {
                if (!string.IsNullOrEmpty(etag))
                {
                    webRequestExecutor.RequestHeaders[HttpRequestHeader.IfMatch] = etag;
                }
            }
            else if (!overwriteIfExists)
            {
                webRequestExecutor.RequestHeaders[HttpRequestHeader.IfNoneMatch] = "*";
            }

            System.IO.Stream requestStream = null;
            if (obj is CookieContainer)
            {
                webRequestExecutor.Request.CookieContainer = obj as CookieContainer;
            }
            else
            {
                webRequestExecutor.Request.Credentials = obj as NetworkCredential;
            }
            webRequestExecutor.Request.Timeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;//reset timeout to upload big binary
            AveAssemblyUtility.SetFieldValue(webRequestExecutor.InnerWebRequestExecutor, webRequestExecutor.InnerWebRequestExecutorType, "m_setupCredential", true);
            requestStream = webRequestExecutor.GetRequestStream();
            byte[] buffer = new byte[0x400];
            int count = 0;
            while ((count = stream.Read(buffer, 0, 0x400)) > 0)
            {
                requestStream.Write(buffer, 0, count);
            }
            requestStream.Flush();
            requestStream.Close();
            try
            {
                webRequestExecutor.Execute();
                if ((webRequestExecutor.StatusCode != HttpStatusCode.Created) && (webRequestExecutor.StatusCode != HttpStatusCode.OK))
                {
                    throw new ClientRequestException(Resources.GetString("RequestUnexpectedResponse", new object[] { webRequestExecutor.ResponseContentType, webRequestExecutor.StatusCode }));
                }
            }
            catch (WebException exception)
            {
                if (exception.Message.Equals(mUnauthorizedMessage, StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveSecurityTrimingException(mUnauthorizedMessage, exception);
                }
                HttpWebResponse response = exception.Response as HttpWebResponse;
                if ((response == null) || (response.StatusCode != HttpStatusCode.PreconditionFailed))
                {
                    throw;
                }
                if (checkMode == SaveBinaryCheckMode.ETag)
                {
                    throw new ClientRequestException(Resources.GetString("ETagNotMatch"));
                }
                throw new ClientRequestException(Resources.GetString("FileAlreadyExists"));
            }
        }

        public virtual Stream OpenBinaryDirect(ClientRuntimeContext context, string serverRelativeUrl, object obj)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.HasPendingRequest)
            {
                context.ExecuteQuery();
                //throw new ClientRequestException(Resources.GetString("NoDirectHttpRequest"));
            }
            string requestUrl = MakeFullUrl(serverRelativeUrl, context);
            AveWebRequestExecutor webRequestExecutor = context.WebRequestExecutorFactory.CreateWebRequestExecutor(context, requestUrl) as AveWebRequestExecutor;
            webRequestExecutor.RequestHeaders[HttpRequestHeader.Translate] = "f";
            if (obj is CookieContainer)
            {
                webRequestExecutor.Request.CookieContainer = obj as CookieContainer;
            }
            else
            {
                webRequestExecutor.Request.Credentials = context.Credentials;
            }
            AveAssemblyUtility.SetFieldValue(webRequestExecutor.InnerWebRequestExecutor, webRequestExecutor.InnerWebRequestExecutorType, "m_setupCredential", true);
            webRequestExecutor.GetRequestStream().Write(new byte[0], 0, 0);
            webRequestExecutor.RequestKeepAlive = false;
            webRequestExecutor.RequestMethod = "GET";
            webRequestExecutor.Request.Timeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;//30 mins
            webRequestExecutor.Execute();
            if (webRequestExecutor.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientRequestException(Resources.GetString("RequestUnexpectedResponse", new object[] { webRequestExecutor.ResponseContentType, webRequestExecutor.StatusCode }));
            }
            return webRequestExecutor.GetResponseStream();
        }

        protected string MakeFullUrl(string serverRelativeUrl, ClientRuntimeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException("serverRelativeUrl");
            }
            if (!serverRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException("serverRelativeUrl");
            }
            Uri baseUri = new Uri(context.Url);
            baseUri = new Uri(baseUri, serverRelativeUrl);
            return baseUri.AbsoluteUri;
        }

        public enum SaveBinaryCheckMode
        {
            ETag,
            Overwrite
        }

        public virtual Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, int templateFileType)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                ClientFile newFile = folder.Files.AddTemplateFile(urlOfFile, (TemplateFileType)templateFileType);

                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(newFile);
                        context.Load(newFile.ListItemAllFields);
                        context.Load(newFile.CheckedOutByUser);
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(newFile);
                    }
                }
                context.ExecuteQuery();
                fileProperties["Exists"] = true;
                AssembleFileProperties(fileProperties, newFile, webServerRelativeUrl, newFile.ListItemAllFields);
                return fileProperties;
            }
        }
        public virtual Dictionary<string, object> AddFolder(string webServerRelativeUrl, Guid listId, string folderServerRelativeUrl, string strUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder parentFolder = GetFolderByAPI(web, folderServerRelativeUrl);
                Folder newFolder = AddFolderByAPI(parentFolder.Folders, strUrl);
                Dictionary<string, object> folderProps = new Dictionary<string, object>();
                LoadFolderProperties(context, webServerRelativeUrl, listId, newFolder, folderProps);
                folderProps["Exists"] = true;
                folderProps["Url"] = TrimFolderUrl(webServerRelativeUrl, newFolder.ServerRelativeUrl);
                folderProps["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = folderServerRelativeUrl;
                return folderProps;
            }
        }

        protected virtual void LoadFolderProperties(AveClientContext context, string webServerRelativeUrl, Guid listId, Folder newFolder, Dictionary<string, object> folderProps)
        {
            context.Load(newFolder);
            context.ExecuteQuery();
            CopyProperty(folderProps, newFolder);
            ListItemCollection listItems = null;
            if (listId != Guid.Empty)
            {
                //SharePoint 2010 API does not contain folder.Item, get folder.Item by CamlQuery.
                List list = context.Site.OpenWeb(webServerRelativeUrl).Lists.GetById(listId);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query></View>", newFolder.ServerRelativeUrl);
                listItems = list.GetItems(camlQuery);
                LoadItemsProperty(context, listItems);
                context.ExecuteQuery();
                if (listItems != null && listItems.Count == 1)
                {
                    ListItem folderNewItem = listItems[0];
                    Dictionary<string, object> itmProp = new Dictionary<string, object>();
                    GetItemDic(itmProp, folderNewItem);
                    folderProps["UniqueId"] = itmProp["UniqueId"];
                    folderProps["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itmProp;
                }
            }
        }

        public virtual Dictionary<string, object> AddView(string webServerRelativeUrl, string listTitle, Guid listId, string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, int type, bool bPersonalView)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                ViewCollection views = list.Views;
                string[] viewFields = new string[strCollViewFields.Count];
                strCollViewFields.CopyTo(viewFields, 0);
                ViewCreationInformation newViewCrtInfo = new ViewCreationInformation();
                newViewCrtInfo.Paged = bPaged;
                newViewCrtInfo.PersonalView = bPersonalView;
                newViewCrtInfo.Query = strQuery;
                newViewCrtInfo.RowLimit = iRowLimit;
                newViewCrtInfo.SetAsDefaultView = bMakeViewDefault;
                newViewCrtInfo.Title = strViewName;
                newViewCrtInfo.ViewFields = viewFields;
                newViewCrtInfo.ViewTypeKind = (ViewType)type;
                View newView = views.Add(newViewCrtInfo);
                context.Load(newView);
                context.Load(newView, v => v.ViewFields);
                context.ExecuteQuery();
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                AssembleViewProperties(returnInfo, newView, webServerRelativeUrl);
                return returnInfo;
            }
        }
        public virtual void AddViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.Add(field);
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public virtual Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> featureProperties = new Dictionary<string, object>();
                Feature newFeature = null;
                switch (featuresSource)
                {
                    case "site.features":
                        newFeature = context.Site.Features.Add(featureId, force, (FeatureDefinitionScope)scope);
                        break;
                    case "web.features":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        newFeature = web.Features.Add(featureId, force, (FeatureDefinitionScope)scope);
                        break;
                }
                context.Load(newFeature);
                context.ExecuteQuery();
                AssembleFeatureProperties(featureProperties, newFeature);
                return featureProperties;
            }
        }
        public virtual Dictionary<string, object> AddContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, Dictionary<string, object> newContentTypeProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                ContentType newCont = null;
                if (newContentTypeProperties.ContainsKey("IsNew"))
                {
                    object contentTypeId;
                    newContentTypeProperties.TryGetValue("ContentTypeId", out contentTypeId);
                    Dictionary<string, object> parentContentDic = newContentTypeProperties["ParentContentType" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    string parentContentTypeWebServerRelativeUrl = parentContentDic[AveObjectModelConstant.WebServerRelativeUrl] as string;
                    string parentContentTypeListName = parentContentDic[AveObjectModelConstant.ListTitle] as string;
                    string parentContentTypeId = parentContentDic["ContentTypeId"] as string;
                    string parentContentTypeSource = parentContentDic["ContentTypeSource"] as string;
                    ContentType parentContentType = this.GetContentTypeWithoutFields(context, parentContentTypeWebServerRelativeUrl, parentContentTypeListName, listId, parentContentTypeSource, parentContentTypeId);
                    //context.Load(parentContentType);
                    //context.ExecuteQuery();
                    ContentTypeCollection cts = this.GetContentTypesWithoutLoad(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                    ContentTypeCreationInformation createInfo = new ContentTypeCreationInformation();
                    if (contentTypeId != null && AveReflectionUtility.ContainsProperty(typeof(ContentTypeCreationInformation), "Id"))
                    {
                        typeof(ContentTypeCreationInformation).GetProperty("Id").SetValue(createInfo, contentTypeId.ToString(), null);
                    }
                    else
                    {
                        createInfo.ParentContentType = parentContentType;
                    }
                    createInfo.Name = newContentTypeProperties["Name"] as string;
                    //createInfo.Description = newContentTypeProperties["Description"] as string;
                    newCont = cts.Add(createInfo);
                    context.Load(newCont);
                    LoadContentType(context, newCont);
                    context.Load(newCont.Parent);
                    context.ExecuteQuery();
                }
                else
                {
                    string existContentTypeWebServerRelativeUrl = newContentTypeProperties[AveObjectModelConstant.WebServerRelativeUrl] as string;
                    string existContentTypeListName = newContentTypeProperties[AveObjectModelConstant.ListTitle] as string;
                    string existContentTypeId = newContentTypeProperties["ContentTypeId"] as string;
                    string existContentTypeSource = newContentTypeProperties["ContentTypeSource"] as string;
                    ContentType existContentType = this.GetContentTypeWithoutFields(context, existContentTypeWebServerRelativeUrl, existContentTypeListName, listId, existContentTypeSource, existContentTypeId);
                    //context.Load(existContentType);
                    //context.ExecuteQuery();
                    ContentTypeCollection cts = this.GetContentTypesWithoutLoad(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                    newCont = cts.AddExistingContentType(existContentType);
                    context.Load(newCont);
                    LoadContentType(context, newCont);
                    context.Load(newCont.Parent);
                    context.ExecuteQuery();
                }
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                this.AssembleSingleContentTypeProperties(newProp, newCont);
                return newProp;
            }
        }
        public virtual Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, int receiverType, string assembly, string className, string name)
        {
            return null;
        }
        public virtual Dictionary<string, object> AddNavigationNode(string webRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> newNodeProperties, string navigationSource)
        {
            AveClientContext context = CreateContext();

            if (parentNodeProperties != null && parentNodeProperties.ContainsKey("ClientContext"))
            {
                context = parentNodeProperties["ClientContext"] as AveClientContext;
            }

            Dictionary<string, object> newNavigationNodeProperties = new Dictionary<string, object>();
            Web web = context.Site.OpenWeb(webRelativeUrl);
            NavigationNode newNavigationNode = null;
            NavigationNodeCollection navigationNodeCollection = null;
            NavigationNodeCreationInformation createInfo = new NavigationNodeCreationInformation();
            createInfo.AsLastNode = newNodeProperties.ContainsKey("AsLastNode") ? (bool)newNodeProperties["AsLastNode"] : false;
            createInfo.Title = newNodeProperties.ContainsKey("Title") ? (string)newNodeProperties["Title"] : null;
            //ADO-198668 createInfo.Url置为null时创建出来的navigation node的url是empty，如果置为string.Empty，name创建出来的navigation node的url是webRelativeUrl
            createInfo.Url = newNodeProperties.ContainsKey("Url") && !string.IsNullOrEmpty((string)newNodeProperties["Url"]) ? (string)newNodeProperties["Url"] : null;
            createInfo.IsExternal = newNodeProperties.ContainsKey("IsExternal") ? (bool)newNodeProperties["IsExternal"] : false;

            if (newNodeProperties.ContainsKey("PreviousNode"))
            {
                Dictionary<string, object> location = newNodeProperties["PreviousNode"] as Dictionary<string, object>;
                if (location.ContainsKey("Id" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    createInfo.PreviousNode = new NavigationNode(context, location["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
                }
            }
            switch (navigationSource)
            {
                case "children":
                    NavigationNode parentNavigationNode = new NavigationNode(context, parentNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
                    navigationNodeCollection = parentNavigationNode.Children;
                    break;
                case "topNavigationBar":
                    navigationNodeCollection = web.Navigation.TopNavigationBar;
                    break;
                case "quickLaunch":
                    navigationNodeCollection = web.Navigation.QuickLaunch;
                    break;
            }
            //mClientContext.Load(navigationNodeCollection);
            newNavigationNode = navigationNodeCollection.Add(createInfo);
            newNavigationNode.Update();
            context.Load(newNavigationNode);
            context.ExecuteQuery();

            CopyProperty(newNavigationNodeProperties, newNavigationNode);
            newNavigationNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = newNavigationNode.Path;
            newNavigationNodeProperties["ClientContext"] = context;
            return newNavigationNodeProperties;
        }

        public virtual Dictionary<string, object> AddFieldAsXml(string webServerRelativeUrl, string listName, Guid listId, String fieldXml, bool addToDefaultView, int op, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                Field field = null;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        //List list = web.Lists.GetByTitle(listName);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listName);
                        }
                        fields = list.Fields;
                        break;
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    //case "web.availablefields":
                    //    field = web.AvailableFields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                    //    break;
                    //case "contenttype.fields":
                    //    string id = contentTypeProp["Id"] as string;
                    //    string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                    //    ContentType contentType = GetContentTypeWithoutFields(webServerRelativeUrl, listName, contentTypeSource, id);
                    //    field = contentType.Fields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                    //    break;
                    default:
                        break;
                }
                if (fields != null)
                {
                    field = fields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                    // the default load can't get the right type.
                    //context.Load(field);
                    context.Load(fields, tempFields => tempFields.IncludeWithDefaultProperties().Where(temp => temp.InternalName == field.InternalName));
                    context.ExecuteQuery();
                    AssembleSingleFieldProperties(fieldProperties, fields[0]);
                    //如果是TaxonomyFieldType或者TaxonomyFieldTypeMulti要把系统创建的与其关联的Note类型的field load出来
                    if ((fieldProperties["TypeAsString"] != null &&
                        (fieldProperties["TypeAsString"].Equals("TaxonomyFieldType") || fieldProperties["TypeAsString"].Equals("TaxonomyFieldTypeMulti"))) &&
                        fieldProperties.ContainsKey("TextField"))
                    {
                        Guid fieldId = (Guid)fieldProperties["TextField"];
                        Dictionary<string, object> RelatedFieldProperties = GetFieldPropertiesById(webServerRelativeUrl, fieldId, fieldSource, listName, listId);
                        fieldProperties.Add("RelatedNoteField", RelatedFieldProperties);
                    }
                }
                return fieldProperties;
            }
        }
        public virtual Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                User user = null;
                switch (source)
                {
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteAdministrators":
                    case "web.siteUsers":
                        user = web.EnsureUser(userProp["LoginName"] as string);
                        break;
                    case "group.users":
                        //GroupCollection groups = web.SiteGroups;
                        break;
                    default:
                        break;
                }
                user.Update();
                context.Load(user);
                context.ExecuteQuery();
                Dictionary<string, object> userPropDictionary = new Dictionary<string, object>();
                CopyProperty(userPropDictionary, user);
                return userPropDictionary;
            }
        }

        public virtual Dictionary<string, object> AddUserProfile(string accountName)
        {
            throw new NotImplementedException();
        }
        public virtual void AddPersonalSite(string accountName, int lcid)
        {
            throw new NotImplementedException();
        }
        public virtual void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            throw new NotImplementedException();
        }
        public virtual string AddSynonm(string term, string synTerm, string terms)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            throw new NotImplementedException();
        }

        public virtual void AddTag(string url, Guid termId, string title, bool? isPrivate)
        {
            throw new NotImplementedException();
        }
        public virtual void AddComment(string url, string comment, bool? isHighPriority, string title)
        {
            throw new NotImplementedException();
        }
        #endregion

        public virtual string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            throw new NotImplementedException();
        }
        public virtual void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> CreateListAssociation(string webServerRelativeUrl, Guid hostListId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                context.Load(template);

                List taskListCM = web.Lists.GetById(asso.TaskListId);
                context.Load(taskListCM);

                List historyListCM = web.Lists.GetById(asso.HistoryListId);
                context.Load(historyListCM);

                List hostListCM = web.Lists.GetById(hostListId);
                context.Load(hostListCM);
                context.ExecuteQuery();

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.TaskList = taskListCM;
                createInfo.HistoryList = historyListCM;
                createInfo.Template = template;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = hostListCM.WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                //assoNew.HistoryListTitle = asso.HistoryListTitle;
                //assoNew.TaskListTitle = asso.TaskListTitle;
                assoNew.Update();

                context.Load(assoNew);
                context.ExecuteQuery();

                CopyProperty(returnInfo, assoNew);

                return returnInfo;
            }
        }

        public virtual Dictionary<string, object> CreateWebAssociation(string webServerRelativeUrl, Guid webId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                List taskListCM = web.Lists.GetById(asso.TaskListId);
                List historyListCM = web.Lists.GetById(asso.HistoryListId);
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.Template = template;
                createInfo.HistoryList = historyListCM;
                createInfo.TaskList = taskListCM;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = web.WorkflowAssociations.Add(createInfo);
                try
                {
                    assoNew.AllowManual = asso.AllowManual;
                    assoNew.AssociationData = asso.AssociationData;
                    assoNew.AutoStartChange = asso.AutoStartChange;
                    assoNew.AutoStartCreate = asso.AutoStartCreate;
                    assoNew.Description = asso.Description;
                    assoNew.Enabled = true;//外面没有赋值 默认是false
                    assoNew.Update();

                    context.Load(assoNew);
                    context.ExecuteQuery();
                }
                catch (ServerException nullEx) //SharePoint Client API Bug: It will throw ArgumentNullException, when updating 10 mode web workflow, but update successfully.
                {
                    if (!nullEx.ServerErrorTypeName.Equals("System.ArgumentNullException"))
                    {
                        throw;
                    }
                    assoNew = web.WorkflowAssociations.GetByName(asso.Name);
                    context.Load(assoNew);
                    context.ExecuteQuery();
                    mLogger.Debug("SharePoint Client API bug when updating 10 mode workflow. Message:{0}.", nullEx.ToString());
                }
                CopyProperty(returnInfo, assoNew);

                return returnInfo;
            }
        }

        public virtual Dictionary<string, object> CreateListContentTypeAssociation(string webServerRelativeUrl, Guid hostListId, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                List hostListCM = web.Lists.GetById(hostListId);
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                //如果没有task list,history list对象，365 List ContentType Workflow Association无法创建出来，
                //属于API的问题，通过Microsoft.SharePoint.ServerStub.Workflow.SPWorkflowAssociationCollectionServerStub.Add_MethodProxy找到到local端最终调用的是
                //SPWorkflowAssociationCollection.Add_Client方法实现，该方法内ListContentTypeWorkflowAssociation如果没有task list，history list是创建不出来的
                //因此在此处强制用title再找一遍，如果还找不到，那么就直接抛异常出去
                if (!asso.TaskListId.Equals(Guid.Empty))
                {
                    createInfo.TaskList = web.Lists.GetById(asso.TaskListId);
                }
                else if (!string.IsNullOrEmpty(asso.TaskListTitle))
                {
                    createInfo.TaskList = web.Lists.GetByTitle(asso.TaskListTitle);
                }
                else
                {
                    throw new ArgumentException("List ContentType WorkflowAssociation Task List");
                }

                if (!asso.HistoryListId.Equals(Guid.Empty))
                {
                    createInfo.HistoryList = web.Lists.GetById(asso.HistoryListId);
                }
                else if (!string.IsNullOrEmpty(asso.HistoryListTitle))
                {
                    createInfo.HistoryList = web.Lists.GetByTitle(asso.HistoryListTitle);
                }
                else
                {
                    throw new ArgumentException("List ContentType WorkflowAssociation History List");
                }

                createInfo.Template = template;
                createInfo.ContentTypeAssociationHistoryListName = asso.HistoryListTitle;
                createInfo.ContentTypeAssociationTaskListName = asso.TaskListTitle;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = hostListCM.ContentTypes.GetById(ctId.ToString()).WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                //assoNew.HistoryListTitle = asso.HistoryListTitle;
                //assoNew.TaskListTitle = asso.TaskListTitle;
                assoNew.Update();

                context.Load(assoNew);
                context.ExecuteQuery();

                CopyProperty(returnInfo, assoNew);
                returnInfo["ContentTypeId"] = ctId;
                return returnInfo;
            }
        }

        public virtual Dictionary<string, object> CreatWebContentTypeAssociation(string webServerRelativeUrl, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Microsoft.SharePoint.Client.Workflow.WorkflowTemplate template = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (workflowTemplateSource)
                {
                    case "web.workflowTemplates":
                        template = web.WorkflowTemplates.GetById(asso.BaseTemplate.ID);
                        break;
                    default:
                        break;
                }
                context.Load(template);
                context.ExecuteQuery();

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation createInfo = new Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCreationInformation();
                createInfo.Name = asso.Name;
                createInfo.Template = template;
                createInfo.ContentTypeAssociationHistoryListName = asso.HistoryListTitle;
                createInfo.ContentTypeAssociationTaskListName = asso.TaskListTitle;

                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation assoNew = web.ContentTypes.GetById(ctId.ToString()).WorkflowAssociations.Add(createInfo);
                assoNew.AllowManual = asso.AllowManual;
                assoNew.AssociationData = asso.AssociationData;
                assoNew.AutoStartChange = asso.AutoStartChange;
                assoNew.AutoStartCreate = asso.AutoStartCreate;
                assoNew.Description = asso.Description;
                assoNew.Enabled = asso.Enabled;
                assoNew.Update();

                context.Load(assoNew);
                context.ExecuteQuery();

                CopyProperty(returnInfo, assoNew);
                returnInfo["ContentTypeId"] = ctId;
                return returnInfo;
            }
        }

        #region  Update
        public virtual Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                object newWebName; //ADO-153129：Name不是client web的基本属性，拼接为ServerRelativeUrl后，再更新
                if (webProperties.TryGetValue("Name", out newWebName))
                {
                    webProperties["ServerRelativeUrl"] = webServerRelativeUrl.Substring(0, webServerRelativeUrl.LastIndexOf('/') + 1) + newWebName.ToString();
                }
                AveObjectCopy.UpdateObjectBasicProperties(webProperties, web);

                bool changed = false;
                if (webProperties.ContainsKey("UseShared"))
                {
                    web.Navigation.UseShared = Convert.ToBoolean(webProperties["UseShared"]);
                    changed = true;
                }
                if (webProperties.ContainsKey("AllPropertiesDictionary"))
                {
                    Dictionary<string, object> allPropertiesDic = webProperties["AllPropertiesDictionary"] as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> pair in allPropertiesDic)
                    {
                        web.AllProperties[pair.Key] = pair.Value;
                    }
                    changed = true;
                }

                Dictionary<string, object> webPro = new Dictionary<string, object>();
                if (Convert.ToInt32(webProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0 || changed)
                {
                    web.Update();
                    webPro = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                    if (newWebName != null)
                    {
                        webPro["Name"] = newWebName;
                    }
                }
                return webPro;
            }
        }
        public virtual Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperty = new Dictionary<string, object>();
                Site site = context.Site;
                AveObjectCopy.UpdateObjectBasicProperties(siteProperties, site);
                site.RefreshLoad();
                //context.ExecuteQuery();
                //Site newSite = context.Site;
                //context.Load(newSite);
                context.Load(site);
                context.ExecuteQuery();
                siteProperty = GetSite();
                return siteProperty;
            }
        }
        public virtual Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                using (AveClientContext context = CreateContext())
                {
                    //code: "list.DocumentTemplateUrl = string.Empty;" works fine in server mode, we should make it work in client mode
                    context.ValidateOnClient = false;
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    //List list = web.Lists.GetByTitle(listName);
                    List list = null;
                    if (listId != Guid.Empty)
                    {
                        list = web.Lists.GetById(listId);
                    }
                    else
                    {
                        list = web.Lists.GetByTitle(listName);
                    }
                    AveObjectCopy.UpdateObjectBasicProperties(listProperties, list);
                    Dictionary<string, object> newProp = new Dictionary<string, object>();
                    if ((int)(listProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                    {
                        list.Update();
                        if (!string.IsNullOrEmpty(this.LoadList(context, list)))
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Request_AccessDenied);
                        }
                        AveObjectCopy.GetObjectBasicProperties(newProp, list);
                    }
                    return newProp;
                }
            }
        }
        public virtual Dictionary<string, object> UpdateFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, Dictionary<string, object> folderProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                AveObjectCopy.UpdateObjectBasicProperties(folderProperties, folder);
                if (folderProperties.ContainsKey("UniqueContentTypeOrder"))
                {
                    List<string> uniqueContentTypeOrder = folderProperties["UniqueContentTypeOrder"] as List<string>;
                    List<ContentTypeId> contentTypeOlders = new List<ContentTypeId>();
                    foreach (string ContentTypeId in uniqueContentTypeOrder)
                    {
                        ContentTypeId cti = new ContentTypeId();
                        AveAssemblyUtility.SetFieldValue(cti, "m_stringValue", ContentTypeId);
                        contentTypeOlders.Add(cti);
                    }
                    folder.UniqueContentTypeOrder = contentTypeOlders;
                }
                if (folderProperties.ContainsKey("FolderChangeProperties") && folderProperties["FolderChangeProperties"] is Dictionary<string, object>)
                {
                    Dictionary<string, object> properties = folderProperties["FolderChangeProperties"] as Dictionary<string, object>;
                    SetFolderPropertyValues(folder, properties);
                    if (!string.IsNullOrEmpty(listName) && listName.Equals("Relationships List", StringComparison.OrdinalIgnoreCase) && properties.ContainsKey("TranslateFields") && string.Compare(this.mServerVersion, "15", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        string folderUrl = webServerRelativeUrl.TrimEnd('/') + '/' + "Translation Packages";
                        UpdateTranslationColumnsSetting(context, GetFolderByAPI(web, folderUrl), properties["TranslateFields"] as string);
                    }
                }
                folder.Update();
                Dictionary<string, object> newProp = this.GetFolder(context, webServerRelativeUrl, listName, listId, folderServerRelativeUrl);
                return newProp;
            }
        }
        protected virtual void UpdateTranslationColumnsSetting(AveClientContext context, ClientFolder folder, string propertyValue)
        {

        }
        protected virtual void SetFolderPropertyValues(ClientFolder folder, Dictionary<string, object> properties)
        {
            //It is not supported to update Folder.Properties in SharePoint 2010.
        }
        public virtual Dictionary<string, object> UpdateView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, Dictionary<string, object> viewProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                View view = this.FindView(webServerRelativeUrl, listName, listId, viewId, context);
                if (view != null)
                {
                    if (viewProperties.ContainsKey("DeleteAllFields"))
                    {
                        view.ViewFields.RemoveAll();
                    }
                    if (viewProperties.ContainsKey("AddViewFields"))
                    {
                        List<string> addViewFieldList = viewProperties["AddViewFields"] as List<string>;
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        //List list = web.Lists.GetByTitle(listName);
                        List list = web.Lists.GetById(listId);
                        foreach (string fieldName in addViewFieldList)
                        {
                            if (list.Fields.GetByInternalNameOrTitle(fieldName) != null)
                            {
                                view.ViewFields.Add(fieldName);
                            }
                        }
                    }
                    if (viewProperties.ContainsKey("DeleteViewFields"))
                    {
                        List<string> deleteViewFieldList = viewProperties["DeleteViewFields"] as List<string>;
                        foreach (string fieldName in deleteViewFieldList)
                        {
                            for (int i = 0; i < view.ViewFields.Count; ++i)
                            {
                                if (fieldName.Equals(view.ViewFields[i]))
                                {
                                    view.ViewFields.Remove(fieldName);
                                    break;
                                }
                            }
                        }
                    }
                    AveObjectCopy.UpdateObjectBasicProperties(viewProperties, view);
                    view.Update();
                    context.Load(view);
                    context.Load(view, v => v.ViewFields);
                    context.ExecuteQuery();
                    Dictionary<string, object> viewProp = new Dictionary<string, object>();
                    AssembleViewProperties(viewProp, view, webServerRelativeUrl);
                    return viewProp;
                    //return null;
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual void HandleMetaInfoField(AveClientContext context, ListItem item, Dictionary<string, object> itemFieldValues)
        {
            if (!itemFieldValues.ContainsKey("ChangeMetaInfo") || !itemFieldValues.ContainsKey("MetaInfo"))
            {
                return;
            }
            Hashtable changedMetaInfos = itemFieldValues["ChangeMetaInfo"] as Hashtable;
            itemFieldValues.Remove("ChangeMetaInfo");
            MetaInfoHandler infoHandler = new MetaInfoHandler(itemFieldValues["MetaInfo"].ToString());
            foreach (DictionaryEntry meta in changedMetaInfos)
            {
                if (!infoHandler.Contains(meta.Key.ToString()))
                {
                    if (meta.Value == null)
                    {
                        continue;
                    }
                    if (meta.Value is string)
                    {
                        infoHandler.Add(new MetaInfoProperty(meta.Key.ToString(), (meta.Value as string).Replace("\\", "\\\\").Replace("\r\n", "\\r\\n")));
                    }
                    else
                    {
                        infoHandler.Add(new MetaInfoProperty(meta.Key.ToString(), meta.Value));
                    }
                }
            }
            itemFieldValues["MetaInfo"] = infoHandler.ToString();
        }
        public virtual Dictionary<string, object> UpdateItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listName);
                }
                ListItem item = list.GetItemById(itemId);
                //由于Moderation不能与其他field同时更新，而在存在ChangedFieldValues的情况下 会keep Modified，导致至少update 两个field（Modified & Moderation）
                //因此，Moderation独立于其他Field更新，当然 如果外围同时更新Moderation与其他Field的话，则与API保持一致 throw Exception
                if (itemProperties.ContainsKey("Ave_ModerationInformation"))
                {
                    Dictionary<string, object> moderationChangedProp = itemProperties["Ave_ModerationInformation"] as Dictionary<string, object>;
                    if (moderationChangedProp.ContainsKey("Comment"))
                    {
                        item["_ModerationComments"] = moderationChangedProp["Comment"] as string;
                    }
                    if (moderationChangedProp.ContainsKey("Status"))
                    {
                        item["_ModerationStatus"] = (int)moderationChangedProp["Status"];
                    }
                    if (!itemProperties.ContainsKey("ChangedFieldValues"))
                    {
                        item.Update();
                        context.Load(item);
                        context.ExecuteQuery();
                    }
                }
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                if (itemProperties.ContainsKey("ChangedFieldValues"))
                {
                    Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
                    HandleMetaInfoField(context, item, itemFieldValues);
                    string updateMethod = itemProperties[AveObjectModelConstant.UpdateMethodName] as string;
                    switch (updateMethod)
                    {
                        case "Update":
                            foreach (KeyValuePair<string, object> kv in itemFieldValues)
                            {
                                item[kv.Key] = kv.Value;
                            }
                            item.Update();
                            LoadItemProperty(context, item);
                            context.ExecuteQuery();
                            break;
                        case "SystemUpdate":
                            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                            {
                                item = InternUpdate(list, item, itemProperties, excepScope);
                                LoadItemProperty(context, item);
                                context.ExecuteQuery();
                            }
                            if (excepScope.HasException)
                            {
                                throw new Exception(excepScope.ErrorMessage);
                            }
                            break;
                        case "SystemUpdateAPI":
                            ExceptionHandlingScope excepScope2 = new ExceptionHandlingScope(context);
                            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                            {
                                item = InternUpdateAPI(list, item, itemProperties, excepScope2);
                                LoadItemProperty(context, item);
                                context.ExecuteQuery();
                            }
                            if (excepScope2.HasException)
                            {
                                throw new Exception(excepScope2.ErrorMessage);
                            }
                            break;
                        case "SystemUpdateForRecords":
                            ExceptionHandlingScope excepScope3 = new ExceptionHandlingScope(context);
                            item = InternUpdateAPI(list, item, itemProperties, excepScope3);
                            LoadItemProperty(context, item);
                            context.ExecuteQuery();
                            if (excepScope3.HasException)
                            {
                                throw new Exception(excepScope3.ErrorMessage);
                            }
                            break;
                        default:
                            break;
                    }
                }
                GetItemDic(returnInfo, item);
                return returnInfo;
            }
        }
        public virtual Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> UpdateGroup(string webServerRelativeUrl, int id, Dictionary<string, object> groupProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Group group = null;
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        group = web.SiteGroups.GetById(id);
                        AveObjectCopy.UpdateObjectBasicProperties(groupProperties, group);

                        if (groupProperties.ContainsKey("OwnerId") && groupProperties.ContainsKey("OwnerType"))
                        {
                            if (groupProperties["OwnerType"].ToString().Equals("group"))
                            {
                                Group ownerGroup = web.SiteGroups.GetById((int)groupProperties["OwnerId"]);
                                group.Owner = ownerGroup;
                            }
                            else if (groupProperties.ContainsKey("OwnerLoginName"))
                            {
                                User user = web.EnsureUser(groupProperties["OwnerLoginName"].ToString());
                                group.Owner = user;
                            }
                        }

                        group.Update();
                        context.Load(group);
                        //context.ExecuteQuery();
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(group);
                    }
                }
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                Dictionary<string, object> groupPro = new Dictionary<string, object>();
                groupPro = GetGroupProperties(webTrimObj, context, group, false);
                return groupPro;
            }
        }
        public virtual Dictionary<string, object> UpdateNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> needUpdateProperties)
        {
            AveClientContext context = CreateContext();
            if (navigationNodeProperties != null && navigationNodeProperties.ContainsKey("ClientContext"))
            {
                context = navigationNodeProperties["ClientContext"] as AveClientContext;
            }

            Dictionary<string, object> NavigationProperties = new Dictionary<string, object>();
            NavigationNode navigationNode = new NavigationNode(context, navigationNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
            AveObjectCopy.UpdateObjectBasicProperties(needUpdateProperties, navigationNode);
            navigationNode.Update();
            context.Load(navigationNode);
            context.ExecuteQuery();

            CopyProperty(NavigationProperties, navigationNode);
            NavigationProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = navigationNode.Path;
            NavigationProperties["ClientContext"] = context;
            return NavigationProperties;
        }
        public virtual Dictionary<string, object> UpdateRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, Dictionary<string, object> needUpdateRoleAssignmentProperties, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        UpdateRoleAssignment(web, needUpdateRoleAssignmentProperties, principalId, web, roleAssignmentProperties);
                        break;
                    case "list.roleAssignments":
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        UpdateRoleAssignment(web, needUpdateRoleAssignmentProperties, principalId, list, roleAssignmentProperties);
                        break;
                    case "item.roleAssignments":
                        //List list1 = web.Lists.GetByTitle(listTitle);
                        List list1 = null;
                        if (listId != Guid.Empty)
                        {
                            list1 = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list1 = web.Lists.GetByTitle(listTitle);
                        }
                        ListItem listItem = list1.GetItemById(itemId);
                        UpdateRoleAssignment(web, needUpdateRoleAssignmentProperties, principalId, listItem, roleAssignmentProperties);
                        break;
                }
                return roleAssignmentProperties;
            }
        }
        public virtual Dictionary<string, object> UpdateRoleDefinition(string webServerRelativeUrl, int id, Dictionary<string, object> needUpdateRoledefinitionProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleDefinitionProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleDefinition roleDefinition = web.RoleDefinitions.GetById(id);
                if (needUpdateRoledefinitionProperties.ContainsKey("BasePermissions"))
                {
                    roleDefinition.BasePermissions = ConvertULongToBasePerm((ulong)needUpdateRoledefinitionProperties["BasePermissions"]);
                }
                needUpdateRoledefinitionProperties.Remove("BasePermissions");
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateRoledefinitionProperties, roleDefinition);
                roleDefinition.Update();
                context.Load(roleDefinition);
                context.ExecuteQuery();
                AssembleRoleDefinitionProperties(roleDefinitionProperties, webServerRelativeUrl, roleDefinition);
                return roleDefinitionProperties;
            }
        }
        public virtual Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            //using (ClientContext context = CreateContext())
            using (ClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                //Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Web web = context.Web;
                FieldCollection fields = null;
                Field field = null;
                bool changed = false;
                ContentType contentType = this.GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, contentTypeId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateContentTypeProperties, contentType);
                if (needUpdateContentTypeProperties.ContainsKey("AddFieldLink"))
                {
                    foreach (Dictionary<string, object> fieldLinkProp in needUpdateContentTypeProperties["AddFieldLink"] as List<Dictionary<string, object>>)
                    {
                        bool isNew = fieldLinkProp.ContainsKey("IsNew") ? (bool)fieldLinkProp["IsNew"] : false;
                        if (isNew)
                        {
                            switch (fieldLinkProp["fieldSource"].ToString())
                            {
                                case "web.fields":
                                    fields = web.Fields;
                                    break;
                                case "web.availableFields":
                                    fields = web.AvailableFields;
                                    break;
                                case "list.fields":
                                    List list = web.Lists.GetByTitle(listName);
                                    fields = list.Fields;
                                    break;
                                default:
                                    break;
                            }
                            field = fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        else
                        {
                            ContentType newContentType = GetContentTypeWithoutFields(context, AveUrlUtility.GetServerRelativeUrl(fieldLinkProp["site"].ToString()), fieldLinkProp["ParentList"] == null ? null : fieldLinkProp["ParentList"].ToString(), Guid.Empty, fieldLinkProp["contentTypeSource"].ToString(), fieldLinkProp["Id"].ToString());
                            context.Load(newContentType, c => c.FieldLinks, c => c.Fields);
                            field = newContentType.Fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        AddContentTypeFieldLink(contentType, field, fieldLinkProp);
                        changed = true;
                        //contentType.Update(updateChildren);
                    }
                }

                changed = UpdateFieldLinkProperties(context, contentType, needUpdateContentTypeProperties, updateChildren) || changed;

                int propertiesCount = Convert.ToInt32(needUpdateContentTypeProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]);
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                if (changed || propertiesCount > 0)
                {
                    contentType.Update(updateChildren);
                    context.Load(contentType);
                    context.Load(contentType, c => c.Parent);
                    context.Load(contentType, c => c.SchemaXml);
                    context.ExecuteQuery();
                    this.AssembleSingleContentTypeProperties(newProp, contentType);
                }
                return newProp;
            }
        }

        public virtual bool UpdateFieldLinkProperties(ClientContext context, ContentType contentType, Dictionary<string, object> needUpdateContentTypeProperties, bool updateChildren)
        {
            bool changed = false;
            object updateLinks = null, deleteLinks = null;
            needUpdateContentTypeProperties.TryGetValue("UpdateFieldLinks", out updateLinks);
            needUpdateContentTypeProperties.TryGetValue("DeleteFieldLink", out deleteLinks);
            if (updateLinks != null || deleteLinks != null)
            {//[ADO-153455]先update，否则load会导致前面的属性update不上
                contentType.Update(updateChildren);
                context.Load(contentType, cts => cts.FieldLinks);
                context.ExecuteQuery();
            }
            changed |= UpdateLinks(contentType, updateLinks as Dictionary<Guid, Dictionary<string, object>>);
            changed |= DeleteFieldLinks(contentType, deleteLinks as List<Guid>);
            return changed;
        }

        protected virtual bool UpdateLinks(ContentType contentType, Dictionary<Guid, Dictionary<string, object>> fieldLinks)
        {
            bool changed = false;
            if (fieldLinks != null)
            {
                foreach (KeyValuePair<Guid, Dictionary<string, object>> fieldlinkInterator in fieldLinks)
                {
                    FieldLink fieldLink = null;

                    foreach (var currentFieldlink in contentType.FieldLinks)
                    {
                        if (currentFieldlink.Id == fieldlinkInterator.Key)
                        {
                            fieldLink = currentFieldlink;
                        }
                    }

                    if (fieldLink == null)
                    {
                        continue;
                    }

                    if (fieldlinkInterator.Value.ContainsKey("Hidden"))
                    {
                        fieldLink.Hidden = Convert.ToBoolean(fieldlinkInterator.Value["Hidden"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("Required"))
                    {
                        fieldLink.Required = Convert.ToBoolean(fieldlinkInterator.Value["Required"]);
                        changed = true;
                    }
                }
            }
            return changed;
        }
        private bool DeleteFieldLinks(ContentType contentType, List<Guid> links)
        {
            bool changed = false;
            if (links != null)
            {
                for (int i = contentType.FieldLinks.Count - 1; i >= 0; i--)
                {
                    if (!links.Contains(contentType.FieldLinks[i].Id))
                    {
                        continue;
                    }
                    contentType.FieldLinks[i].DeleteObject();
                    changed = true;
                }
            }
            return changed;
        }

        protected virtual void AddContentTypeFieldLink(ContentType contentType, Field field, Dictionary<string, object> fieldLinkProp)
        {
            FieldLinkCreationInformation Info = new FieldLinkCreationInformation();
            Info.Field = field;
            contentType.FieldLinks.Add(Info);
            int fieldLinksCount = contentType.FieldLinks.Count;
            FieldLink fieldLink = contentType.FieldLinks[fieldLinksCount - 1];
            if (fieldLinkProp.ContainsKey("Hidden"))
            {
                fieldLink.Hidden = bool.Parse(fieldLinkProp["Hidden"].ToString());
            }
            if (fieldLinkProp.ContainsKey("Required"))
            {
                fieldLink.Required = bool.Parse(fieldLinkProp["Required"].ToString());
            }
        }


        public virtual Dictionary<string, object> UpdateEventReceiver(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId, Dictionary<string, object> needUpdateEventReceiverProperties)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> BreakRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, bool copyRoleAssignments, bool clearSubscopes, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleAssignmentCollection roleAssignmentCol = null;
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        web.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                        roleAssignmentCol = web.RoleAssignments;
                        break;
                    case "list.roleAssignments":
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        list.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                        roleAssignmentCol = list.RoleAssignments;
                        break;
                    case "item.roleAssignments":
                        //List list1 = web.Lists.GetByTitle(listTitle);
                        List list1 = null;
                        if (listId != Guid.Empty)
                        {
                            list1 = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list1 = web.Lists.GetByTitle(listTitle);
                        }
                        ListItem listItem = list1.GetItemById(itemId);
                        listItem.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                        roleAssignmentCol = listItem.RoleAssignments;
                        break;
                }
                context.Load(roleAssignmentCol, roles => roles.IncludeWithDefaultProperties(role => role.RoleDefinitionBindings, role => role.Member));
                context.ExecuteQuery();
                AssembleRoleAssignmetsProperites(roleAssignmentsProperties, roleAssignmentCol);
                return roleAssignmentsProperties;
            }
        }

        public virtual Dictionary<string, object> ResetRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> roleAssignmentsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                RoleAssignmentCollection roleAssignmentCol = null;
                switch (roleAssignmentsSource)
                {
                    case "web.roleAssignments":
                        web.ResetRoleInheritance();
                        roleAssignmentCol = web.RoleAssignments;
                        break;
                    case "list.roleAssignments":
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        list.ResetRoleInheritance();
                        roleAssignmentCol = list.RoleAssignments;
                        break;
                    case "item.roleAssignments":
                        //List list1 = web.Lists.GetByTitle(listTitle);
                        List list1 = null;
                        if (listId != Guid.Empty)
                        {
                            list1 = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list1 = web.Lists.GetByTitle(listTitle);
                        }
                        ListItem listItem = list1.GetItemById(itemId);
                        listItem.ResetRoleInheritance();
                        roleAssignmentCol = listItem.RoleAssignments;
                        break;
                }
                context.Load(roleAssignmentCol, roles => roles.IncludeWithDefaultProperties(r => r.RoleDefinitionBindings, r => r.Member));
                context.ExecuteQuery();
                AssembleRoleAssignmetsProperites(roleAssignmentsProperties, roleAssignmentCol);
                return roleAssignmentsProperties;
            }
        }

        public virtual void MoveFieldTo(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field, int index)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.MoveFieldTo(field, index);
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public virtual void Approve(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                context.Load(file, f => f.ListItemAllFields.Id);
                context.ExecuteQuery();
                ListItem listItem = file.ListItemAllFields.ParentList.GetItemById(file.ListItemAllFields.Id);
                listItem["_ModerationStatus"] = (int)AveModerationStatusType.Approved;
                listItem["_ModerationComments"] = comment;
                listItem.Update();
                context.Load(listItem);
                context.ExecuteQuery();
            }
        }

        public virtual void Deny(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                context.Load(file, f => f.ListItemAllFields.Id);
                context.ExecuteQuery();
                ListItem listItem = file.ListItemAllFields.ParentList.GetItemById(file.ListItemAllFields.Id);
                listItem["_ModerationStatus"] = (int)AveModerationStatusType.Denied;
                listItem["_ModerationComments"] = comment;
                listItem.Update();
                context.Load(listItem);
                context.ExecuteQuery();
            }
        }

        public virtual Dictionary<string, object> CheckIn(string webServerRelativeUrl, string fileServerRelativeUrl, string comment, int checkinType)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.CheckIn(comment, (CheckinType)checkinType);
                ConditionalScope fileExistScope = new ConditionalScope(context, () => file.Exists);
                using (fileExistScope.StartScope())
                {
                    using (fileExistScope.StartIfTrue())
                    {
                        SafeLoadFile(context, file);
                    }
                }
                context.ExecuteQuery();
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                fileProperties["Exists"] = fileExistScope.TestResult.HasValue && fileExistScope.TestResult.Value;
                AssembleFileProperties(fileProperties, file, webServerRelativeUrl, file.ListItemAllFields);
                return fileProperties;
            }
        }
        public virtual Dictionary<string, object> CheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.CheckOut();
                ConditionalScope fileExistScope = new ConditionalScope(context, () => file.Exists);
                using (fileExistScope.StartScope())
                {
                    using (fileExistScope.StartIfTrue())
                    {
                        SafeLoadFile(context, file);
                    }
                }
                context.ExecuteQuery();
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                fileProperties["Exists"] = fileExistScope.TestResult.HasValue && fileExistScope.TestResult.Value;
                AssembleFileProperties(fileProperties, file, webServerRelativeUrl, file.ListItemAllFields);
                return fileProperties;
            }
        }
        public virtual void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.CopyTo(strNewUrl, bOverWrite);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public virtual void MoveTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, int flags)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.MoveTo(strNewUrl, (MoveOperations)flags);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public virtual void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, Stream file)
        {
            using (AveClientContext context = CreateContext())
            {
                context.RequestTimeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;//30 mins

                ClientFile.SaveBinaryDirect(context, fileServerRelativeUrl, file, true);
            }
        }
        public virtual void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, byte[] file)
        {
            using (AveClientContext context = CreateContext())
            {
                context.RequestTimeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;//30 mins

                FileSaveBinaryInformation fileInfo = new FileSaveBinaryInformation();
                fileInfo.Content = file;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile targetFile = GetFileByAPI(web, fileServerRelativeUrl);
                targetFile.SaveBinary(fileInfo);
                context.Load(targetFile);
                context.ExecuteQuery();
            }
        }
        public virtual void UndoCheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.UndoCheckOut();
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public virtual void UnPublish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.UnPublish(comment);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public virtual void Publish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.Publish(comment);
                context.Load(file);
                context.ExecuteQuery();
            }
        }
        public virtual Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop)
        {
            return GetFile(webServerRelativeUrl, fileServerRelativeUrl, listName);
        }
        public virtual void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
        }
        public virtual Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties)
        {
            //if (fieldProperties.ContainsKey("ClientContext"))
            //{
            //    context = fieldProperties["ClientContext"] as AveClientContext;
            //}
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        //List list = web.Lists.GetByTitle(listName);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listName);
                        }
                        context.Load(list, l => l.ItemCount);
                        context.ExecuteQuery();
                        if (list.ItemCount >= 20000)
                        {
                            mLogger.Warn("The item count in the list is larger than 20000, won't update indexed property");
                            fieldProperties.Remove("Indexed");
                        }
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                Guid fieldId = GetFieldIdFromIdentity(fieldProperties["ObjectPath"].ToString());
                ObjectPath path = new ObjectPathMethod(context, fields.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProperties["FieldType"] as Type, new object[] { context, path }) as Field;
                bool needReloadField = false;
                if (fieldProperties.ContainsKey("Type"))
                {
                    fieldProperties["FieldTypeKind"] = fieldProperties["Type"];
                    fieldProperties.Remove("Type");
                    needReloadField = true;
                }
                //部分field的property不能用client api直接更新，需要将这些property先更新的SchemaXml中，再更新
                List<string> needPostPropertyNames = new List<string> { "DisplayFormat", "ShowAsPercentage" };
                Dictionary<string, object> postUpdateProperties = new Dictionary<string, object>();
                bool needPostUpdate = AddPropertyToPostUpdate(needPostPropertyNames, fieldProperties, postUpdateProperties);

                AveObjectCopy.UpdateObjectBasicProperties(fieldProperties, field);
                var userResourceChanged = UpdateFieldUserResource(field, fieldProperties);
                var hasValidProperties = (int)(fieldProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0;
                if (needPostUpdate || hasValidProperties || userResourceChanged)
                {
                    try
                    {
                        if (hasValidProperties || userResourceChanged)
                        {
                            field.Update();
                            if (needReloadField)
                            {
                                field = fields.GetById(fieldId);
                            }
                        }
                        context.Load(field);
                        context.ExecuteQuery();
                        if (needPostUpdate)
                        {
                            UpdatePostFieldProperties(context, field, postUpdateProperties);
                        }
                    }
                    catch (ServerException e) //Ado-67156 在Template为 Language and translate 的List中,部分Field的ID会产生变化
                    {
                        if (e.ServerErrorCode == AveStandardErrorCode.COR_E_ARGUMENT)
                        {
                            mLogger.Warn("An error occurred while get field by ID.error message: {0}", e.Message);
                            path = new ObjectPathMethod(context, fields.Path, "GetByInternalNameOrTitle", new object[] { internalName });
                            field = Activator.CreateInstance(fieldProperties["FieldType"] as Type, new object[] { context, path }) as Field;
                            AveObjectCopy.UpdateObjectBasicProperties(fieldProperties, field);
                            field.Update();
                            context.Load(field);
                            context.ExecuteQuery();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    AssembleSingleFieldProperties(fieldProp, field);
                }
                return fieldProp;
            }
        }
        //cache需要在其他properties更新后，加入schema中再更新的properties，针对于client api无法直接更新的field properties
        private bool AddPropertyToPostUpdate(List<string> needPostPropertyNames, Dictionary<string, object> fieldProperties, Dictionary<string, object> postUpdateProperties)
        {
            bool needPostUpdate = false;
            foreach (var propertyName in needPostPropertyNames)
            {
                if (fieldProperties.ContainsKey(propertyName))
                {
                    needPostUpdate = true;
                    postUpdateProperties[propertyName] = fieldProperties[propertyName];
                    fieldProperties.Remove(propertyName);
                }
            }
            return needPostUpdate;
        }
        /// <summary>
        /// 用于更新Calculate类型Field（Web Level）特有的FieldRefs属性，这个属性会影响到Fomula的显示。
        /// </summary>
        /// <param name="context">AveClientContext 对象，用于Field的单独更新。</param>
        /// <param name="field">Field对象，SPClient对象，在传参前需要load field对象。</param>
        /// <param name="FieldRefsXml">需要加入的FieldRefs Xml string。</param>
        private void UpdateCalculateFieldRefs(AveClientContext context, Field field, string FieldRefsXml)
        {
            var tempXmlDoc = new XmlDocument();
            tempXmlDoc.LoadXml(field.SchemaXml);
            var tempCalculatedFieldRootXmlNode = tempXmlDoc.SelectSingleNode("Field");
            var fieldRefsXml = tempXmlDoc.CreateElement("FieldRefs");
            fieldRefsXml.InnerXml = FieldRefsXml;
            if (tempCalculatedFieldRootXmlNode.SelectSingleNode("FieldRefs") != null)
            {
                tempCalculatedFieldRootXmlNode.RemoveChild(tempCalculatedFieldRootXmlNode.SelectSingleNode("FieldRefs"));
            }
            tempCalculatedFieldRootXmlNode.AppendChild(fieldRefsXml as XmlNode);
            field.SchemaXml = tempXmlDoc.InnerXml;
            field.Update();
            context.Load(field);
            context.ExecuteQuery();
        }
        //将cache起来尚未更新的properties加入field.SchemaXml中，更新SchemaXml。针对于client api无法直接更新的field properties
        //此方法不仅普通update方法需要走，UpdateReadOnlyField方法也需要执行。一部分read only的Field也具有display属性。
        private void UpdatePostFieldProperties(AveClientContext context, Field field, Dictionary<string, object> postUpdateProperties)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(field.SchemaXml);
            foreach (var property in postUpdateProperties)
            {
                switch (property.Key)
                {
                    case "DisplayFormat":
                        if (field is FieldDateTime)// For DateTime Field And Url Field
                        {
                            doc.DocumentElement.SetAttribute("Format", ((AveDateTimeFieldFormatType)(Convert.ToInt32(postUpdateProperties["DisplayFormat"]))).ToString());
                        }
                        else if (field is FieldUrl)
                        {
                            doc.DocumentElement.SetAttribute("Format", ((AveUrlFieldFormatType)(Convert.ToInt32(postUpdateProperties["DisplayFormat"]))).ToString());
                        }
                        else // For Number Field 当前只发现 Number Field 对应Attribute 名字为 Decimals，不保证存在其他的情况，因此改回原来逻辑
                        {
                            doc.DocumentElement.SetAttribute("Decimals", (Convert.ToInt32(postUpdateProperties["DisplayFormat"])).ToString());
                        }
                        break;
                    case "ShowAsPercentage":
                        doc.DocumentElement.SetAttribute("Percentage", postUpdateProperties["ShowAsPercentage"].ToString().ToUpper(CultureInfo.InvariantCulture));
                        break;
                }
            }
            field.SchemaXml = doc.InnerXml;
            field.Update();
            context.Load(field);
            context.ExecuteQuery();
        }
        public virtual Dictionary<string, object> UpdateTermStore(Guid guid, int termStoreDefaultLanguage, Dictionary<string, object> needUpdateProperties)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> UpdateUserProfileProperties(string userProfilePropertyName, Dictionary<string, object> dictionary)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> UpdateReadOnlyField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                //if (fieldProperties.ContainsKey("ClientContext"))
                //{
                //    context = fieldProperties["ClientContext"] as AveClientContext;
                //}

                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        //List list = web.Lists.GetByTitle(listName);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listName);
                        }
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                Guid fieldId = GetFieldIdFromIdentity(fieldProperties["ObjectPath"].ToString());
                ObjectPath path = new ObjectPathMethod(context, fields.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProperties["FieldType"] as Type, new object[] { context, path }) as Field;
                List<string> needPostPropertyNames = new List<string> { "DisplayFormat", "ShowAsPercentage" };
                Dictionary<string, object> postUpdateProperties = new Dictionary<string, object>();
                bool needPostUpdate = AddPropertyToPostUpdate(needPostPropertyNames, fieldProperties, postUpdateProperties);
                AveObjectCopy.UpdateObjectBasicProperties(fieldProperties, field);
                if ((int)(fieldProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    //Readonly field can be update if the ReadOnlyField property is true.
                    //field.ReadOnlyField = false;
                    //field.Update();
                    //field.ReadOnlyField = true;
                    field.Update();
                    context.Load(field);
                    context.ExecuteQuery();
                    CopyProperty(fieldProp, field);
                    fieldProp["Type"] = field.FieldTypeKind;
                    fieldProp["ObjectPath"] = GetObjectPathString(field.Path);
                    fieldProp["FieldType"] = field.GetType();
                    if (fieldProperties.ContainsKey("FieldRefsXml"))
                    {
                        UpdateCalculateFieldRefs(context, field, fieldProperties["FieldRefsXml"].ToString());
                    }
                    if (needPostUpdate)
                    {
                        UpdatePostFieldProperties(context, field, postUpdateProperties);
                    }
                }



                return fieldProp;
            }
        }

        public virtual Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateUserProfileDetails(string accountName, string xml)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateUserProfileMemberships(string accountName, string xml)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateUserProfileColleages(string accountName, string xml)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateUserProfileTags(string accountName, string xml)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateSpecialProperty(Dictionary<string, object> specialProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                try
                {
                    AveObjectCopy.UpdateObjectBasicProperties(specialProp, site);
                    context.ExecuteQuery();
                }
                catch (ServerUnauthorizedAccessException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    mLogger.Debug("Error occurred while update special property.ErrorMessage:{0}.", e.ToString());
                    throw;
                }
            }
        }
        public virtual void RevertAllDocumentContentStreams(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }
        public virtual void RevertContentStream(string webServerRelativeUrl, string fileUrl)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            throw new NotImplementedException();
        }
        public virtual void UpdateNavigationUseShared(string webServerRelativeUrl, bool useShared)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.Navigation.UseShared = useShared;
                context.ExecuteQuery();
            }
        }
        public virtual void UpdateWorkflowAssociation(string webServerRelativeUrl, string listName, Guid listId, string ctId, Guid workflowAssociationId, string workflowSource, Dictionary<string, object> needUpdateWorkflowProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCollection workflowAssociations = null;
                switch (workflowSource)
                {
                    case "web.workflows":
                        workflowAssociations = web.WorkflowAssociations;
                        break;
                    case "list.workflows":
                        //List list = web.Lists.GetByTitle(listName);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listName);
                        }
                        workflowAssociations = list.WorkflowAssociations;
                        break;
                    case "contentType.workflows":
                        ContentType contentType = null;
                        if (!string.IsNullOrEmpty(listName))
                        {
                            //contentType = web.Lists.GetByTitle(listName).ContentTypes.GetById(ctId);
                            contentType = web.Lists.GetById(listId).ContentTypes.GetById(ctId);
                        }
                        else
                        {
                            contentType = web.ContentTypes.GetById(ctId);
                        }
                        workflowAssociations = contentType.WorkflowAssociations;
                        break;
                    default:
                        break;
                }
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflowAsso = workflowAssociations.GetById(workflowAssociationId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateWorkflowProperties, workflowAsso);
                workflowAsso.Update();
                context.ExecuteQuery();

            }
        }
        #endregion

        #region Delete
        public virtual void DeleteFeature(string webServerRelativeUrl, Guid featureId, bool force, string featureSource)
        {
            using (AveClientContext context = CreateContext())
            {
                FeatureCollection featureCollection = null;
                switch (featureSource)
                {
                    case "site.features":
                        context.Load(context.Site, f => f.Features);
                        featureCollection = context.Site.Features;
                        break;
                    case "web.features":
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        featureCollection = web.Features;
                        break;
                    default:
                        break;
                }
                featureCollection.Remove(featureId, force);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    context.Site.RecycleBin.GetById(id).DeleteObject();
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.DeleteRecycleBinItemFailed, context.Url, e.ToString());
                    throw;
                }
            }
        }
        public virtual void DeleteWeb(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.DeleteObject();
                context.ExecuteQuery();
            }
        }

        public virtual void DeleteView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId)
        {
            using (AveClientContext context = CreateContext())
            {
                View view = this.FindView(webServerRelativeUrl, listName, listId, viewId, context);
                if (view != null)
                {
                    view.DeleteObject();
                    context.ExecuteQuery();
                }
            }
        }
        public virtual void DeleteList(string webServerRelativeUrl, string listName, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listName);
                }
                try
                {
                    DeleteItemsUnderList(context, web, list, webServerRelativeUrl, listName, listId);
                    list.DeleteObject();
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Debug("Can not delete system list. List name: {0}, Exception Message: {1}", listName, e);
                }

            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Leq is a legal expression")]
        protected virtual void DeleteItemsUnderList(AveClientContext context, Web web, List list, string webServerRelativeUrl, string listName, Guid listId)
        {
            context.Load(list, l => l.ItemCount);
            context.Load(list, l => l.RootFolder);
            context.ExecuteQuery();
            string folderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            if (list.ItemCount > 5000)
            {
                ListItemCollection listItems = null;
                List<int> itemIds = new List<int>();
                List<int> folderIds = new List<int>();
                int index = 0; //遍历使用的item id
                int itemCount = 0; //每次遍历获取过的item count
                bool delete = false; //是否执行了删除操作
                do
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = string.Format(
                        "<View>" +
                        "<Query><Where><And>" +
                        "<Gt><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{0}</Value>" +
                        "</Gt>" +
                        "<Leq><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{1}</Value>" +
                        "</Leq>" +
                        "</And></Where></Query>" +
                        "</View>", index, index + 2000);
                    SetCamlQueryFolderUrl(camlQuery, folderServerRelativeUrl);
                    //camlQuery.FolderServerRelativeUrl = folderServerRelativeUrl;
                    listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                                     items => items.Include(item => item.Id, item => item["FSObjType"], item => item["FileRef"]
                                                         , item => item["ItemChildCount"], item => item["FolderChildCount"]));
                    context.ExecuteQuery();
                    if (listItems.Count > 0)
                    {
                        foreach (ListItem item in listItems)
                        {
                            index = item.Id;
                            if (item["FSObjType"].ToString().Equals("1"))
                            {
                                int count = Convert.ToInt32(item["ItemChildCount"]) + Convert.ToInt32(item["FolderChildCount"]);
                                if (count > 0)
                                {
                                    index = DeleteFolderItems(context, list, WebAppName.TrimEnd('/') + webServerRelativeUrl, item, count);
                                }
                                folderIds.Add(item.Id);
                            }
                            else
                            {
                                itemCount++;
                                if (itemCount > 4999)
                                {
                                    itemIds.Add(item.Id);
                                }
                            }
                        }
                        if (folderIds.Count > 0)
                        {
                            AveWebServiceRequest.DeleteItems(WebAppName.TrimEnd('/') + webServerRelativeUrl, listName, mObj, folderIds);
                            folderIds.Clear();
                            delete = true;
                        }
                        if (itemIds.Count > 0)
                        {
                            AveWebServiceRequest.DeleteItems(WebAppName.TrimEnd('/') + webServerRelativeUrl, listName, mObj, itemIds);
                            itemIds.Clear();
                            delete = true;
                        }
                        if (delete)
                        {
                            //list = web.Lists.GetByTitle(listName);
                            list = web.Lists.GetById(listId);
                            context.Load(list, l => l.ItemCount, l => l.Title);
                            context.ExecuteQuery();
                        }
                    }
                    else
                    {
                        index = index + 2000;
                    }
                }
                while (list.ItemCount > 5000);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Leq is a legal expression")]
        private int DeleteFolderItems(AveClientContext context, List list, string webUrl, ListItem folder, int childCount)
        {
            ListItemCollection listItems = null;
            List<int> itemIds = new List<int>();
            List<int> folderIds = new List<int>();
            int itemCount = 0;
            int index = folder.Id;
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                    "<View>" +
                    "<Query><Where><And>" +
                    "<Gt><FieldRef Name=\"ID\"/>" +
                    "<Value Type=\"Integer\">{0}</Value>" +
                    "</Gt>" +
                    "<Leq><FieldRef Name=\"ID\"/>" +
                    "<Value Type=\"Integer\">{1}</Value>" +
                    "</Leq>" +
                    "</And></Where></Query>" +
                    "</View>", index, index + 1000);
                //camlQuery.FolderServerRelativeUrl = folder["FileRef"].ToString();
                SetCamlQueryFolderUrl(camlQuery, folder["FileRef"].ToString());
                listItems = list.GetItems(camlQuery);
                context.Load(listItems, items => items.ListItemCollectionPosition,
                                                 items => items.Include(item => item.Id, item => item["FSObjType"], item => item["FileRef"]
                                                     , item => item["ItemChildCount"], item => item["FolderChildCount"]));
                context.ExecuteQuery();
                foreach (ListItem item in listItems)
                {
                    childCount--;
                    index = item.Id;
                    if (item["FSObjType"].ToString().Equals("1"))
                    {
                        int count = Convert.ToInt32(item["ItemChildCount"]) + Convert.ToInt32(item["FolderChildCount"]);
                        if (count > 0)
                        {
                            index = DeleteFolderItems(context, list, webUrl, item, count);
                        }
                        folderIds.Add(item.Id);
                    }
                    else
                    {
                        itemCount++;
                        if (itemCount > 4999)
                        {
                            itemIds.Add(item.Id);
                        }
                    }
                }
                if (folderIds.Count > 0)
                {
                    AveWebServiceRequest.DeleteItems(webUrl, list.Title, mObj, folderIds);
                    folderIds.Clear();
                }
                if (itemIds.Count > 0)
                {
                    AveWebServiceRequest.DeleteItems(webUrl, list.Title, mObj, itemIds);
                    itemIds.Clear();
                }
            }
            while (childCount > 0);
            return index;
        }

        public virtual void DeleteFolder(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                folder.DeleteObject();
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteItem(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTile);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                ListItem item = list.GetItemById(itemId);
                context.Load(item);
                item.DeleteObject();
                context.ExecuteQuery();
            }
        }

        public virtual void DeleteItemVersion(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, int versionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                context.Load(list);
                ListItem item = list.GetItemById(itemId);
                context.Load(item);
                context.ExecuteQuery();
                string listGuid = list.Id.ToString();
                string fileName = item.FieldValues["FileRef"].ToString();
                string op = "Delete";
                //if (item.FieldValues["_UIVersion"].Equals(versionId))
                //{
                //    throw new Exception(WrapperClientResource.Wrapper_Client_DeleteItemVersionFailed);
                //}
                //else
                //{
                this.OperateOnVersion(webServerRelativeUrl, WebAppName, mObj, listUrl, itemId, versionId, listGuid, fileName, op);
                //}
            }
        }
        public virtual void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            AveHttpWebRequestCommon.OperateOnVersion(webServerRelativeUrl, WebAppName, mObj, listUrl, itemId, versionId, listId, fileName, op, "/_layouts");
        }
        public virtual void DeleteFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.Versions.DeleteByID(id);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.Versions.DeleteAll();
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.Versions.DeleteByLabel(versionLabel);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteGroup(string webServerRelativeUrl, int id)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Group group = web.SiteGroups.GetById(id);
                web.SiteGroups.Remove(group);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, string source)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (source)
                {
                    case "web.roleAssignments":
                        web.RoleAssignments.GetByPrincipalId(principalId).DeleteObject();
                        break;
                    case "list.roleAssignments":
                        //List list = web.Lists.GetByTitle(listTitle);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        list.RoleAssignments.GetByPrincipalId(principalId).DeleteObject();
                        break;
                    case "item.roleAssignments":
                        //List _list = web.Lists.GetByTitle(listTitle);
                        List _list = null;
                        if (listId != Guid.Empty)
                        {
                            _list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            _list = web.Lists.GetByTitle(listTitle);
                        }
                        ListItem item = _list.GetItemById(itemId);
                        item.RoleAssignments.GetByPrincipalId(principalId).DeleteObject();
                        break;
                }
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteRoleDefinition(string webServerRelativeUrl, string roleDefintionName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.RoleDefinitions.GetByName(roleDefintionName).DeleteObject();
                context.ExecuteQuery();
            }
        }

        public virtual void DeleteAttachment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid webId, Guid listId, int rowId, string attachmentName)
        {
            // api in version 14 does not implement ListItem.AttachmentFiles
            //DeleteAttachmentNow(webServerRelativeUrl, listServerRelativeUrl, listTitle, rowId, attachmentName);
        }

        public virtual void DeleteViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string fieldName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.Remove(fieldName);
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteAllViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                View view = list.Views.GetById(viewId);
                ViewFieldCollection viewFs = view.ViewFields;
                viewFs.RemoveAll();
                context.Load(viewFs);
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteFile(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.DeleteObject();
                context.ExecuteQuery();
            }
        }
        public virtual void DeleteEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId)
        {
            throw new NotImplementedException();
        }
        public virtual void DeleteNavigationNode(string webServerRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> deleteNodeProperties)
        {
            AveClientContext context = CreateContext();
            if (deleteNodeProperties != null && deleteNodeProperties.ContainsKey("ClientContext"))
            {
                context = deleteNodeProperties["ClientContext"] as AveClientContext;
            }

            NavigationNode deleteNavigationNode = new NavigationNode(context, deleteNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] as ObjectPath);
            context.Load(deleteNavigationNode);
            deleteNavigationNode.DeleteObject();
            context.ExecuteQuery();
        }
        public virtual void DeleteField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Field field = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        //List list = web.Lists.GetByTitle(listName);
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listName);
                        }
                        field = list.Fields.GetByInternalNameOrTitle(internalName);
                        break;
                    case "web.fields":
                        field = web.Fields.GetByInternalNameOrTitle(internalName);
                        break;
                    case "web.availableFields":
                        field = web.AvailableFields.GetByInternalNameOrTitle(internalName);
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["Id"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, id);
                        field = contentType.Fields.GetByInternalNameOrTitle(internalName);
                        break;
                    default:
                        break;
                }
                field.DeleteObject();
                context.ExecuteQuery();
            }
        }

        public virtual void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName)
        {
            throw new NotImplementedException();
        }
        public virtual void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles)
        {
        }

        public virtual void DeleteTag(string url, Guid termId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Restore
        public virtual void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    context.Site.RecycleBin.GetById(id).Restore();
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.RestoreRecycleBinItemFailed, context.Url, e.ToString());
                    throw;
                }
            }
        }

        public virtual void RestoreFileVersion(string versionLabel, string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                file.Versions.RestoreByLabel(versionLabel);
                context.ExecuteQuery();
            }
        }
        public virtual void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, IList webpartBaseInfoList, AveWebPartCache mapping, bool clearAll, IAveWeb web, IReport report)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl))
            {
                using (AveWebPartRestore webpartRestore = new AveWebPartRestore(webServerRelativeUrl, listTitle, listId, fileServerRelativeUrl, scope, clearAll, context, mapping, web, report, mObj))
                {
                    webpartRestore.RestoreWebParts(webpartRestore.GetNeedRestoreWebParts(webpartBaseInfoList, clearAll));
                }
            }
        }

        public virtual Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveListItemRestore listItemRestore = new AveListItemRestore(this, site, context, mObj))
                {
                    return listItemRestore.RestoreListItem(data, userData, AddItemMapping);
                }
            }
        }

        public virtual Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveFolderRestore folderRestore = new AveFolderRestore(this, site, context, mObj))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }

        public virtual Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveDocumentRestore documentRestore = new AveDocumentRestore(this, site, mObj, context, mServerVersion, report))
                {
                    return documentRestore.RestoreDocument(info, fileStream);
                }
            }
        }

        public virtual Dictionary<string, object> RestoreAttachment(Dictionary<string, object> data, Dictionary<string, object> userData, Stream fileStream)
        {
            using (AveAttachmentRestore attachmentRestore = new AveAttachmentRestore(this))
            {
                return attachmentRestore.RestoreAttachment(data, fileStream);
            }
        }

        public virtual List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
                switch (featuresSource)
                {
                    case "site.features":
                        foreach (Dictionary<string, object> featureInfo in featureInfoList)
                        {
                            try
                            {
                                foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                                {
                                    site.Features.Add(id, force, FeatureDefinitionScope.Site);
                                }
                                Dictionary<string, object> featureProperties = new Dictionary<string, object>();
                                Feature newFeature = site.Features.Add(new Guid(featureInfo["ID"].ToString()), force, FeatureDefinitionScope.Site);
                                context.ExecuteQuery();
                                AssembleFeatureProperties(featureProperties, newFeature);
                                featuresProperties.Add(featureProperties);
                            }
                            catch (ServerUnauthorizedAccessException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Debug("An error occurred while adding site feature.Message:{0}.", ex.ToString());
                            }
                        }
                        break;
                    case "web.features":
                        Web web = site.OpenWeb(webServerRelativeUrl);
                        foreach (Dictionary<string, object> featureInfo in featureInfoList)
                        {
                            try
                            {
                                foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                                {
                                    try
                                    {
                                        web.Features.Add(id, force, FeatureDefinitionScope.Site);
                                        context.ExecuteQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        mLogger.Debug("An error occurred while adding web feature.Message:{0}.", ex.ToString());
                                        site.Features.Add(id, force, FeatureDefinitionScope.Site);
                                    }
                                }
                                Dictionary<string, object> featureProperties = new Dictionary<string, object>();
                                Feature newFeature = web.Features.Add(new Guid(featureInfo["ID"].ToString()), force, FeatureDefinitionScope.Site);
                                context.ExecuteQuery();
                                AssembleFeatureProperties(featureProperties, newFeature);
                                featuresProperties.Add(featureProperties);
                            }
                            catch (ServerUnauthorizedAccessException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Debug("An error occurred while restoring web feature.Message:{0}.", ex.ToString());
                            }
                        }
                        break;
                }
                return featuresProperties;
            }
        }

        public virtual bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> RestoreUserProfileProperties(Dictionary<string, object> userProfilePropertiesInfo, bool isOverWrite)
        {
            //throw new NotImplementedException();
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo, bool isOnlineSite, bool isExistSkip)
        {
            throw new NotImplementedException();
        }
        public virtual void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Recycle
        public virtual Guid RecycleItem(string webRelativeUrl, string listRelativeUrl, string listTitle, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                //List list = web.Lists.GetByTitle(listTile);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                ListItem item = list.GetItemById(itemId);
                context.Load(item);
                item.Recycle();
                context.ExecuteQuery();
                return (Guid)item.FieldValues["GUID"];
            }
        }

        public virtual Guid RecycleList(string webRelativeUrl, string listTitle, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                context.Load(list);
                list.Recycle();
                context.ExecuteQuery();
                return (Guid)list.Id;
            }
        }
        #endregion

        #region private method
        protected virtual void LoadWeb(Web web, ClientContext context)
        {
            context.Load(web);
            context.Load(web, w => w.CurrentUser);
            ExceptionHandlingScope memberGroupCondition = new ExceptionHandlingScope(context);
            using (memberGroupCondition.StartScope())
            {
                using (memberGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType);
                }
                using (memberGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedMemberGroup);
                }
            }
            ExceptionHandlingScope ownerGroupCondition = new ExceptionHandlingScope(context);
            using (ownerGroupCondition.StartScope())
            {
                using (ownerGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType);
                }
                using (ownerGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup);
                }
            }
            ExceptionHandlingScope visitorGroupCondition = new ExceptionHandlingScope(context);
            using (visitorGroupCondition.StartScope())
            {
                using (visitorGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup, w => w.AssociatedVisitorGroup.Users, w => w.AssociatedVisitorGroup.Owner.Id, w => w.AssociatedVisitorGroup.Owner.PrincipalType);
                }
                using (visitorGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup);
                }
            }
            context.Load(web, w => w.RootFolder);
            context.Load(web, w => w.ListTemplates, w => w.AllProperties);
            context.Load(web, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch);
            context.Load(web, w => w.AllowDesignerForCurrentUser, w => w.HasUniqueRoleAssignments);
        }

        protected Dictionary<string, object> ObjectToDicValue(object Object, Type type)
        {
            Dictionary<string, object> DicProperties = new Dictionary<string, object>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.Name == "DefinitionId")
                {
                    DicProperties[property.Name] = property.GetGetMethod().Invoke(Object, null);
                }
            }
            return DicProperties;
        }

        protected virtual void SetEditorReadOnly(List list, bool readOnly)
        {
            if (list != null)
            {
                Field editorField = list.Fields.GetById(AveBuiltInFieldId.Editor);
                editorField.ReadOnlyField = readOnly;
                editorField.Update();
            }
        }

        public virtual ListItem InternUpdateAPI(List list, ListItem item, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        {
            return this.InternUpdate(list, item, itemProperties, excepScope);
        }

        public virtual ListItem InternUpdate(List list, ListItem item, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        {
            Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
            bool enableVersioning = (bool)itemProperties["EnableVersioning"];
            bool enableMinVersion = (bool)itemProperties["EnableMinorVersions"];
            bool enableModeration = (bool)itemProperties["EnableModeration"];
            bool isApproved = itemProperties.ContainsKey("IsApproved") ? (bool)itemProperties["IsApproved"] : false;
            bool isCurrentMinorVersion = itemProperties.ContainsKey("IsCurrentMinorVersion") ? (bool)itemProperties["IsCurrentMinorVersion"] : false;
            bool isOriginalCheckOut = itemProperties.ContainsKey("IsOriginalCheckOut") ? (bool)itemProperties["IsOriginalCheckOut"] : false;
            int itemType = itemProperties.ContainsKey("FileSystemObjectType") ? (int)itemProperties["FileSystemObjectType"] : 0;
            string checkInComment = itemProperties.ContainsKey("CheckInComment") ? itemProperties["CheckInComment"].ToString() : string.Empty;
            //int moderationStatus = -1;
            //if (itemFieldValues.ContainsKey("_ModerationStatus"))
            //{
            //    moderationStatus = (int)itemFieldValues["_ModerationStatus"];
            //    itemFieldValues.Remove("_ModerationStatus");
            //}
            using (excepScope.StartScope())
            {
                using (excepScope.StartTry())
                {
                    list.Context.Load(item);
                    list.Context.Load(item, it => it.HasUniqueRoleAssignments);
                    if (enableVersioning)
                    {
                        if (!enableMinVersion || !isCurrentMinorVersion)
                        {
                            list.EnableVersioning = false;
                        }
                        if (isApproved && enableModeration)//If item status is approval,Need close the moderation for keep modified.Minor Version can not approval.
                        {
                            list.EnableModeration = false;
                        }
                        if (isCurrentMinorVersion && !enableModeration)//For Minor Version,If current is minor version,We need open moderation for making the version Level not become the published.
                        {
                            list.EnableModeration = true;
                        }
                        list.Update();
                    }
                    if (enableMinVersion && isCurrentMinorVersion && (FileSystemObjectType)itemType == FileSystemObjectType.File && !isOriginalCheckOut)//For Minor Version,Close Version and Check Out can Keep the Minor Version.
                    {
                        item.File.CheckOut();
                    }
                    SetEditorReadOnly(list, true);
                    bool changed = AveListItemRestore.SetFieldValues(item, itemFieldValues);
                    if (changed)
                    {
                        item.Update();
                        //if (moderationStatus != -1 && itemFieldValues.ContainsKey("Modified"))  //keep modified and moderationStatus when update lookup field or restore datajunction
                        //{
                        //    tempListItem["Modified"] = itemFieldValues["Modified"];
                        //    tempListItem["_ModerationStatus"] = moderationStatus;
                        //    tempListItem.Update();
                        //}
                    }
                    SetEditorReadOnly(list, false);
                    if (enableMinVersion && isCurrentMinorVersion && (FileSystemObjectType)itemType == FileSystemObjectType.File && !isOriginalCheckOut)
                    {
                        item.File.CheckIn(checkInComment, CheckinType.OverwriteCheckIn);
                    }
                }
                using (excepScope.StartFinally())
                {
                    if (enableVersioning)
                    {
                        list.EnableVersioning = true;
                        if (enableMinVersion)
                        {
                            list.EnableMinorVersions = true;
                            if (isCurrentMinorVersion && !enableModeration)
                            {
                                list.EnableModeration = false;
                            }
                        }
                        if (isApproved && enableModeration)
                        {
                            list.EnableModeration = true;
                        }
                        list.Update();
                    }
                }
            }
            return item;
        }
        protected virtual void WebGetSubwebs(AveClientContext context, Web rootWeb, List<Dictionary<string, object>> webList, string siteUrl, string siteServerRelativeUrl)
        {
            WebCollection subWebs = rootWeb.GetSubwebsForCurrentUser(null);
            LoadWebCollection(context, subWebs);
            foreach (Web web in subWebs)
            {
                Dictionary<string, object> dicWeb = new Dictionary<string, object>();
                dicWeb = GetWebProperties(context, web, siteUrl, siteServerRelativeUrl, true);
                webList.Add(dicWeb);
                WebGetSubwebs(context, web, webList, siteUrl, siteServerRelativeUrl);
            }
        }

        protected virtual void LoadWebCollection(ClientContext context, WebCollection webCollection)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                                 w => w.RootFolder,
                                                                                                 w => w.ListTemplates,
                                                                                                 w => w.AllProperties,
                                                                                                 w => w.Navigation.TopNavigationBar,
                                                                                                 w => w.Navigation.QuickLaunch,
                                                                                                 w => w.AllowDesignerForCurrentUser,
                                                                                                 w => w.HasUniqueRoleAssignments,
                                                                                                 w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType,
                                                                                                 w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType));
                }
                using (scope.StartCatch())
                {
                    context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                                 w => w.RootFolder,
                                                                                                 w => w.ListTemplates,
                                                                                                 w => w.AllProperties,
                                                                                                 w => w.Navigation.TopNavigationBar,
                                                                                                 w => w.Navigation.QuickLaunch,
                                                                                                 w => w.AllowDesignerForCurrentUser,
                                                                                                 w => w.HasUniqueRoleAssignments,
                                                                                                 w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType));
                    //w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType
                }
            }
            context.ExecuteQuery();
        }

        protected List<Dictionary<string, object>> NavigationNodeCollectionToList(NavigationNodeCollection nodes, Dictionary<string, object> nodesProp)
        {
            List<Dictionary<string, object>> returnPropeties = new List<Dictionary<string, object>>();
            foreach (NavigationNode node in nodes)
            {
                Dictionary<string, object> nodeDic = new Dictionary<string, object>();
                CopyProperty(nodeDic, node);

                List<Dictionary<string, object>> childNodeList = new List<Dictionary<string, object>>();
                GetNavigationNodeChild(node, childNodeList, nodesProp);
                Dictionary<string, object> childNodesProperties = new Dictionary<string, object>();
                childNodesProperties.Add(AveObjectModelConstant.ChildrenProperties, childNodeList);
                nodeDic["Children" + AveObjectModelConstant.ObjectPropertySuffix] = childNodesProperties;//childNodeList;
                nodeDic["Id" + AveObjectModelConstant.ObjectPropertySuffix] = node.Path;
                nodeDic["ClientContext"] = node.Context;

                bool nodeVisible = false;
                if (nodesProp.Count > 0) //使用httpwebrequest获取navigationUrls的时候可能因为权限问题获取不到
                {
                    foreach (KeyValuePair<string, object> pair in nodesProp)
                    {
                        if (pair.Key.Contains("," + node.Id.ToString()))
                        {
                            Dictionary<string, object> nodeProp = pair.Value as Dictionary<string, object>;
                            nodeDic["Target"] = nodeProp["Target"].ToString();
                            nodeDic["Url"] = nodeProp["NodeUrl"].ToString();
                            nodeDic["NodeType"] = nodeProp["NodeType"].ToString();
                            nodeDic["Description"] = nodeProp["Description"].ToString();
                            nodeDic["Audience"] = nodeProp["Audience"].ToString();
                            nodeVisible = true;
                            break;
                        }
                    }
                    if (!nodeVisible)//默认Home节点取不到，特殊处理一下
                    {
                        nodeDic["Url"] = node.Url;
                    }
                }
                else
                {
                    nodeDic["Url"] = node.Url;
                }
                if (!string.IsNullOrEmpty(node.Url))
                {
                    if (node.Url.IndexOf("_layouts", StringComparison.OrdinalIgnoreCase) != -1) //和sharepoint封装的CreateSPNavigationNode方法保持一致 ADO-57555
                    {
                        nodeDic["IsExternal"] = true;
                    }
                    else
                    {
                        if (node.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            node.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                            node.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            nodeDic["IsExternal"] = !node.Url.StartsWith(this.WebAppName, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            nodeDic["IsExternal"] = false;
                        }
                    }
                }
                else//ADO-57555,为空的时候此属性设置为true
                {
                    nodeDic["IsExternal"] = true;
                }
                returnPropeties.Add(nodeDic);
            }
            return returnPropeties;
        }
        protected void GetNavigationNodeChild(NavigationNode node, List<Dictionary<string, object>> dic, Dictionary<string, object> nodesProp)
        {
            node.Context.Load(node.Children);
            node.Context.ExecuteQuery();
            foreach (NavigationNode childNode in node.Children)
            {
                Dictionary<string, object> nodeDic = new Dictionary<string, object>();
                //AveObjectCopy.GetObjectBasicProperties(nodeDic, childNode);
                List<Dictionary<string, object>> childNodeList = new List<Dictionary<string, object>>();
                GetNavigationNodeChild(childNode, childNodeList, nodesProp);
                Dictionary<string, object> childNodesProperties = new Dictionary<string, object>();
                childNodesProperties.Add(AveObjectModelConstant.ChildrenProperties, childNodeList);
                CopyProperty(nodeDic, childNode);
                nodeDic["Children" + AveObjectModelConstant.ObjectPropertySuffix] = childNodesProperties;
                nodeDic["Id" + AveObjectModelConstant.ObjectPropertySuffix] = childNode.Path;
                nodeDic["ClientContext"] = childNode.Context;

                bool nodeVisible = false;
                if (nodesProp.Count > 0) //使用httpwebrequest获取navigationUrls的时候可能因为权限问题获取不到
                {
                    foreach (KeyValuePair<string, object> pair in nodesProp)
                    {
                        if (pair.Key.Contains("," + childNode.Id.ToString()))
                        {
                            Dictionary<string, object> nodeProp = pair.Value as Dictionary<string, object>;
                            nodeDic["Target"] = nodeProp["Target"].ToString();
                            nodeDic["Url"] = nodeProp["NodeUrl"].ToString();
                            nodeDic["NodeType"] = nodeProp["NodeType"].ToString();
                            nodeDic["Description"] = nodeProp["Description"].ToString();
                            nodeDic["Audience"] = nodeProp["Audience"].ToString();
                            nodeVisible = true;
                            break;
                        }
                    }
                    if (!nodeVisible)
                    {
                        nodeDic["Url"] = childNode.Url;
                    }
                }
                else
                {
                    nodeDic["Url"] = childNode.Url;
                }
                if (!string.IsNullOrEmpty(childNode.Url))
                {
                    if (childNode.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        childNode.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                        childNode.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                    {
                        string nodeFullUrl = childNode.Url.TrimEnd('/') + "/";
                        string siteFullUrl = this.WebAppName.TrimEnd('/') + "/" + this.mSiteRelativeUrl.Trim('/') + "/";
                        nodeDic["IsExternal"] = !nodeFullUrl.StartsWith(siteFullUrl, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        nodeDic["IsExternal"] = false;
                    }
                }
                else
                {
                    nodeDic["IsExternal"] = true;
                }
                dic.Add(nodeDic);
            }
        }
        protected Microsoft.SharePoint.Client.View FindView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, AveClientContext context)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            //List list = web.Lists.GetByTitle(listName);
            List list = null;
            if (listId != Guid.Empty)
            {
                list = web.Lists.GetById(listId);
            }
            else
            {
                list = web.Lists.GetByTitle(listName);
            }
            View view = null;
            try
            {
                view = list.Views.GetById(viewId);
                context.Load(view);
                context.Load(view, v => v.ViewFields);
                context.ExecuteQuery();
            }
            catch (Exception ex)
            {
                mLogger.Debug("An error occurred while finding view.Message:{0}.", ex.ToString());
                view = null;
            }
            return view;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        public virtual void GetItemDic(Dictionary<string, object> itemProperties, ListItem item)
        {
            //require object has been initialized
            CopyProperty(itemProperties, item);
            if (item.FieldValues.Count > 0)
            {
                Dictionary<string, object> fieldValues = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> fieldValue in item.FieldValues)
                {
                    AssembleItemProperties(fieldValues, fieldValue.Value, fieldValue.Key);
                }
                itemProperties["FieldValues"] = fieldValues;
                //item properties
                foreach (KeyValuePair<string, object> pair in item.FieldValues)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    if (pair.Value.GetType().FullName == "System.String")
                    {
                        if (string.IsNullOrEmpty(pair.Value.ToString()))
                        {
                            continue;
                        }
                    }
                    switch (pair.Key)
                    {
                        case "FileRef":
                            itemProperties["ServerRelativeUrl"] = pair.Value.ToString();
                            itemProperties["FullUrl"] = pair.Value.ToString();
                            break;
                        case "File_x0020_Size":
                            itemProperties["Length"] = long.Parse(pair.Value.ToString());
                            break;
                        case "Title":
                            itemProperties["Title"] = pair.Value.ToString();
                            break;
                        case "Created":
                            itemProperties["TimeCreated"] = pair.Value;
                            break;
                        case "Modified":
                            itemProperties["TimeLastModified"] = AssignTimeKind((DateTime)pair.Value);
                            break;
                        case "FSObjType":
                            itemProperties["FileSystemObjectType"] = int.Parse(pair.Value.ToString());
                            break;
                        case "ID":
                            itemProperties["Id"] = pair.Value;
                            itemProperties["ID"] = pair.Value;
                            break;
                        case "_UIVersionString":
                            itemProperties["UIVersionString"] = pair.Value.ToString();
                            break;
                        case "_UIVersion":
                            itemProperties["UIVersion"] = int.Parse(pair.Value.ToString());
                            break;
                        //多值column value,需要返回string[]而不是string
                        //case "UIVersion":
                        //    string[] uiVersion = pair.Value as string[];
                        //    if (uiVersion != null && uiVersion.Length > 0)
                        //    {
                        //        fieldValues["UIVersion"] = uiVersion[0];
                        //    }
                        //    break;
                        case "_Level":
                            itemProperties["Level"] = byte.Parse(pair.Value.ToString());
                            break;
                        case "ContentTypeId":
                            itemProperties["ContentTypeId"] = pair.Value.ToString();
                            break;
                        case "Attachments":
                            itemProperties["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = pair.Value.ToString();
                            break;
                        case "Url":
                            break;
                        case "FileLeafRef":
                            if (item.FieldValues.ContainsKey("FSObjType") && (item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
                            {
                                if ((pair.Value as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                                {
                                    itemProperties["Name"] = item.FieldValues["Title"];
                                    if (itemProperties["Name"] == null)
                                    {
                                        itemProperties["Name"] = "";
                                    }
                                }
                                else
                                {
                                    itemProperties["Name"] = pair.Value.ToString();
                                }
                            }
                            else
                            {
                                itemProperties["Name"] = pair.Value.ToString();
                            }
                            itemProperties["LeafName"] = pair.Value.ToString();
                            break;
                        case "UniqueId":
                            itemProperties["DocId"] = pair.Value;
                            itemProperties["DocID"] = pair.Value;
                            itemProperties[pair.Key] = pair.Value;
                            break;
                        default:
                            itemProperties[pair.Key] = pair.Value;
                            break;
                    }
                }
                if (!itemProperties.ContainsKey("GUID") && itemProperties.ContainsKey("UniqueId"))
                {
                    itemProperties["GUID"] = itemProperties["UniqueId"];
                }
            }
        }

        private DateTime AssignTimeKind(DateTime dataTime)
        {
            return dataTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dataTime, DateTimeKind.Local) : dataTime;
        }

        protected void CopyStream(Stream src, Stream dest, int size, bool resetPoistion)
        {
            byte[] buffer = new byte[size];
            int len = 0;
            while ((len = src.Read(buffer, 0, size)) != 0)
            {
                dest.Write(buffer, 0, len);
            }
            if (resetPoistion)
            {
                dest.Position = 0;
            }
        }
        protected ContentTypeCollection GetContentTypesWithoutLoad(ClientContext context, string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            ContentTypeCollection contentTypes = null;
            switch (contentTypeSource)
            {
                case "web.availableContentTypes":
                    contentTypes = web.AvailableContentTypes;
                    break;
                case "web.contentTypes":
                    contentTypes = web.ContentTypes;
                    break;
                case "list.contentTypes":
                    //List list = web.Lists.GetByTitle(listName);
                    List list = null;
                    if (listId != Guid.Empty)
                    {
                        list = web.Lists.GetById(listId);
                    }
                    else
                    {
                        list = web.Lists.GetByTitle(listName);
                    }
                    contentTypes = list.ContentTypes;
                    break;
                default:
                    break;
            }
            return contentTypes;
        }

        protected ContentTypeCollection GetContentTypesWithoutFields(ClientContext context, string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        {
            ContentTypeCollection contentTypes = GetContentTypesWithoutLoad(context, webServerRelativeUrl, listName, listId, contentTypeSource);
            LoadContentTypes(context, contentTypes);
            return contentTypes;
        }

        protected virtual void LoadContentTypes(ClientContext context, ContentTypeCollection contentTypes)
        {
            context.Load(contentTypes, tempContentTypes => tempContentTypes.IncludeWithDefaultProperties(temp => temp.Id, temp => temp.Parent.Id, temp => temp.SchemaXml, temp => temp.SchemaXml));//cts => cts.IncludeWithDefaultProperties(ct => ct.Fields, ct => ct.FieldLinks));
        }

        protected ContentTypeCollection GetContentTypesWithSimpleProperties(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        {
            using (AveClientContext context = CreateContext())
            {
                ContentTypeCollection contentTypes = GetContentTypesWithoutLoad(context, webServerRelativeUrl, listName, listId, contentTypeSource);
                context.Load(contentTypes, tempContentTypes => tempContentTypes.Include(temp => temp.Name, temp => temp.ReadOnly, temp => temp.Parent.Id));
                return contentTypes;
            }
        }
        protected ContentType GetContentTypeWithoutFields(ClientContext context, string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, string contentTypeId)
        {
            ContentTypeCollection cts = GetContentTypesWithoutLoad(context, webServerRelativeUrl, listName, listId, contentTypeSource);
            ContentType contentType = cts.GetById(contentTypeId);
            LoadContentType(context, contentType);

            return contentType;
        }

        protected virtual void LoadContentType(ClientContext context, ContentType contentType)
        {
            context.Load(contentType, c => c.Id, c => c.SchemaXml, c => c.FieldLinks);
        }

        protected virtual Dictionary<string, object> GetWebProperties(ClientContext context, Web web, string contextUrl, string siteServerRelativeUrl, bool webLoaded)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            if (!webLoaded)
            {
                context.Load(context.Site.RootWeb);
                LoadWeb(web, context);
                context.ExecuteQuery();
            }
            CopyProperty(webProperties, web);
            SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(web.ServerRelativeUrl, contextUrl);
            webProperties["Exists"] = true;
            webProperties["CurrentUser" + AveObjectModelConstant.ObjectPropertySuffix] = web.CurrentUser.LoginName;
            //webProperties.Add("IsPublish", false);

            string Url = string.Empty;
            if (web.ServerRelativeUrl.Equals("/"))
            {
                Url = this.WebAppName;
            }
            else if (contextUrl.EndsWith(web.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                Url = contextUrl;
            }
            else
            {
                Url = siteServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase) ?
                      AveUrlUtility.CombineUrl(contextUrl, web.ServerRelativeUrl) :
                      contextUrl.ToLowerInvariant().Replace(siteServerRelativeUrl.ToLowerInvariant(), web.ServerRelativeUrl);
            }
            webProperties["Url"] = Url;
            bool IsRootWeb = true;
            string Name = string.Empty;
            string ParentWebServerRelativeUrl = string.Empty;
            if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                IsRootWeb = false;//isRootWeb
                int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                Name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
                ParentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex);
            }
            webProperties["IsRootWeb"] = IsRootWeb;
            // The value of HasUniqueRoleDefinitions in RootWeb is true.
            webProperties["HasUniqueRoleDefinitions"] = IsRootWeb;
            // Add RootWeb Id
            webProperties["FirstUniqueRoleDefinitionWeb" + AveObjectModelConstant.ObjectPropertySuffix] = context.Site.RootWeb.Id;
            webProperties["Name"] = Name;
            webProperties["ParentWeb" + AveObjectModelConstant.ObjectPropertySuffix] = ParentWebServerRelativeUrl;
            string webTemplate = string.Empty;
            //对web的template和configuration进行赋值
            Dictionary<string, object> result = AveWebTemplateHelper.GetWebTemplateConfigurationProperty(Url, this.mObj, mServerVersion, 14);
            if (result.ContainsKey("WebTemplateId"))
            {
                webProperties["WebTemplateId"] = result["WebTemplateId"];
            }
            string configuration = string.Empty;
            if (result.ContainsKey("Configuration"))
            {
                configuration = result["Configuration"].ToString();
            }
            if (configuration.Equals("ACCSRV#0", StringComparison.OrdinalIgnoreCase))
            {
                configuration = GetDataBaseWebTemplate(web, context);
            }
            if (!string.IsNullOrEmpty(configuration))
            {
                string[] datas = configuration.Split('#');
                if (datas.Length == 2)
                {
                    webProperties["WebTemplate"] = datas[0];
                    webProperties["Configuration"] = short.Parse(datas[1]);
                }
            }
            webProperties["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] = web.AllProperties.FieldValues;

            Dictionary<string, object> AssociatedMemberGroupProperties = GetGroupProperties(webTrimObj, context, web.AssociatedMemberGroup, false);
            Dictionary<string, object> AssociatedOwnerGroupProperties = GetGroupProperties(webTrimObj, context, web.AssociatedOwnerGroup, false);
            Dictionary<string, object> AssociatedVisitorGroupProperties = GetGroupProperties(webTrimObj, context, web.AssociatedVisitorGroup, false);

            webProperties["AssociatedOwnerGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedOwnerGroupProperties;
            webProperties["AssociatedMemberGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedMemberGroupProperties;
            webProperties["AssociatedVisitorGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedVisitorGroupProperties;

            AveWebServiceRequest.GetWebSearchAndOfflineAvailability(this.WebAppName, web.ServerRelativeUrl, webProperties, mObj);
            return webProperties;
        }
        //DataBase类型web获取webtemplate时错误,模仿Local逻辑,通过Item Title判断web类型.
        protected string GetDataBaseWebTemplate(Web web, ClientContext context)
        {
            Dictionary<string, string> dataBaseWebTemplates = new Dictionary<string, string>();
            dataBaseWebTemplates["ACCSRV#1"] = "Users|#;Assets";
            dataBaseWebTemplates["ACCSRV#3"] = "Tasks|#;Users|#;Events|#;Settings|#;Campaigns|#;Donors|#;Donations|#;EventAttendees";
            dataBaseWebTemplates["ACCSRV#4"] = "Contacts|#;Comments";
            dataBaseWebTemplates["ACCSRV#6"] = "Users|#;Issues|#;Comments|#;RelatedIssues";
            dataBaseWebTemplates["ACCSRV#5"] = "Tasks|#;Projects|#;Users|#;Customers|#;ProjectHistory";

            List mSysList = null;
            if (web.AllProperties.FieldValues.ContainsKey("___MSysASOId") && !string.IsNullOrEmpty(web.AllProperties.FieldValues["___MSysASOId"].ToString()))
            {
                Guid listID = new Guid(web.AllProperties.FieldValues["___MSysASOId"].ToString());
                mSysList = web.Lists.GetById(listID);
            }
            else
            {
                mSysList = web.Lists.GetByTitle("MSysASO");
            }
            ListItemCollection items = mSysList.GetItems(CamlQuery.CreateAllItemsQuery());
            context.Load(items);
            context.ExecuteQuery();
            string webTemplate = string.Empty;
            foreach (KeyValuePair<string, string> pair in dataBaseWebTemplates)
            {
                bool getDataBaseWebTemplate = true;
                string[] titles = pair.Value.Split(new string[] { "|#;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string title in titles)
                {
                    if (!FindItemByTitle(title, items))
                    {
                        getDataBaseWebTemplate = false;
                        break;
                    }
                }
                if (getDataBaseWebTemplate)
                {
                    webTemplate = pair.Key;
                    break;
                }
            }
            return webTemplate;
        }
        protected bool FindItemByTitle(string title, ListItemCollection items)
        {
            bool exist = false;
            foreach (ListItem item in items)
            {
                if (item.FieldValues.ContainsKey("Title") && item.FieldValues["Title"].Equals(title))
                {
                    exist = true;
                    break;
                }
            }
            return exist;
        }
        protected void AssembleRoleAssignmetsProperites(Dictionary<string, object> roleAssignmentsProperties, RoleAssignmentCollection roleAssignmentCollection)
        {
            CopyProperty(roleAssignmentsProperties, roleAssignmentCollection);
            List<Dictionary<string, object>> roleAssignmentPropertiesList = new List<Dictionary<string, object>>(roleAssignmentCollection.Count);
            foreach (RoleAssignment roleAssignment in roleAssignmentCollection)
            {
                Dictionary<string, object> roleAssignemntProperties = new Dictionary<string, object>();
                AssembleRoleAssignmetProperites(roleAssignemntProperties, roleAssignment);
                roleAssignmentPropertiesList.Add(roleAssignemntProperties);
            }
            roleAssignmentsProperties.Add(AveObjectModelConstant.ChildrenProperties, roleAssignmentPropertiesList);
        }
        protected void AssembleRoleAssignmetProperites(Dictionary<string, object> roleAssignemntProperties, RoleAssignment roleAssignment)
        {
            CopyProperty(roleAssignemntProperties, roleAssignment);
            Principal member = roleAssignment.Member;
            roleAssignemntProperties.Add("MemberLoginName", member.LoginName);
            //roleAssignemntProperties.Add("MemberType", member.PrincipalType == PrincipalType.User ? "User" : "Group");
            if (member.PrincipalType == PrincipalType.User)
            {
                roleAssignemntProperties.Add("MemberType", "User");
            }
            else if (member.PrincipalType == PrincipalType.SecurityGroup)
            {
                roleAssignemntProperties.Add("MemberType", "SecurityGroup");
            }
            else
            {
                roleAssignemntProperties.Add("MemberType", "Group");
            }
            Dictionary<string, object> roleDefinitionBindingCollectionProperties = new Dictionary<string, object>();
            AssembleRoleDefinitionBindingsProperties(roleDefinitionBindingCollectionProperties, roleAssignment.RoleDefinitionBindings);
            roleAssignemntProperties.Add("RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix, roleDefinitionBindingCollectionProperties);
        }
        protected void AssembleRoleDefinitionBindingsProperties(Dictionary<string, object> roleDefinitionsProperties, RoleDefinitionBindingCollection roleDefinitionCollection)
        {
            List<Dictionary<string, object>> roleDefinitionPropertiesList = new List<Dictionary<string, object>>();
            foreach (RoleDefinition roleDefinition in roleDefinitionCollection)
            {
                Dictionary<string, object> roleDefinitionProperties = new Dictionary<string, object>();
                CopyProperty(roleDefinitionProperties, roleDefinition);
                roleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
                roleDefinitionPropertiesList.Add(roleDefinitionProperties);
            }
            roleDefinitionsProperties.Add("ChildrenProperties", roleDefinitionPropertiesList);
        }
        protected void AssembleRoleDefinitionsProperties(Dictionary<string, object> roleDefinitionsProperties, string webServerRelativeUrl, RoleDefinitionCollection roleDefinitionCollection)
        {
            List<Dictionary<string, object>> roleDefinitionPropertiesList = new List<Dictionary<string, object>>();
            foreach (RoleDefinition roleDefinition in roleDefinitionCollection)
            {
                Dictionary<string, object> roleDefinitionProperties = new Dictionary<string, object>();
                CopyProperty(roleDefinitionProperties, roleDefinition);
                roleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
                roleDefinitionProperties[AveObjectModelConstant.WebServerRelativeUrl] = webServerRelativeUrl;
                roleDefinitionProperties["Type"] = (int)roleDefinition.RoleTypeKind;
                roleDefinitionPropertiesList.Add(roleDefinitionProperties);
            }
            roleDefinitionsProperties.Add("ChildrenProperties", roleDefinitionPropertiesList);
        }
        protected void AssembleRoleDefinitionProperties(Dictionary<string, object> roleDefinitionProperties, string webServerRelativeUrl, RoleDefinition roleDefinition)
        {
            CopyProperty(roleDefinitionProperties, roleDefinition);
            roleDefinitionProperties["BasePermissions"] = ConvertBasePermToULong(roleDefinition.BasePermissions);
            roleDefinitionProperties[AveObjectModelConstant.WebServerRelativeUrl] = webServerRelativeUrl;
        }
        public virtual void AssembleFileProperties(Dictionary<string, object> fileProperties, ClientFile file, string webServerRelativeUrl, ListItem item)
        {
            if (!string.IsNullOrEmpty(webServerRelativeUrl))
            {
                fileProperties["Url"] = file.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            string parentFolderServerRelativeUrl = file.ServerRelativeUrl.Substring(0, file.ServerRelativeUrl.LastIndexOf('/'));
            if (string.IsNullOrEmpty(parentFolderServerRelativeUrl))
            {
                parentFolderServerRelativeUrl = "/";
            }
            fileProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = parentFolderServerRelativeUrl;

            if (fileProperties.ContainsKey("ListName") && !string.IsNullOrEmpty(fileProperties["ListName"] as string)
                && item != null && item.FieldValues.Count > 0)
            {
                Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                GetItemDic(itemProperties, item);
                fileProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperties;
                fileProperties["UniqueId"] = itemProperties["UniqueId"];
                if (itemProperties.ContainsKey("Length"))
                {
                    fileProperties["Length"] = itemProperties["Length"];
                }
            }
            else if (!fileProperties.ContainsKey("ListName") && item != null && item.FieldValues.Count > 0)
            {
                Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                GetItemDic(itemProperties, item);
                fileProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperties;
                fileProperties["UniqueId"] = itemProperties["UniqueId"];
                fileProperties["Length"] = itemProperties["Length"];
            }
            else
            {
                try
                {
                    string[] ids = GetIdsFromEtag(file.ETag);
                    string uniqueId = ids[0];
                    if (!string.IsNullOrEmpty(uniqueId))
                    {
                        fileProperties["UniqueId"] = new Guid(uniqueId);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Assemble file:{0} property Id failed.Error Message:{1}", file.ServerRelativeUrl, ex.ToString());
                }
                fileProperties["IsSystemFile"] = true;
            }
            if (file.IsObjectPropertyInstantiated("CheckedOutByUser") && file.CheckedOutByUser.IsPropertyAvailable("LoginName"))
            {
                fileProperties["CheckedOutByUser" + AveObjectModelConstant.ObjectPropertySuffix] = file.CheckedOutByUser.LoginName;
            }
            if (file.IsObjectPropertyInstantiated("Author") && file.Author.IsPropertyAvailable("LoginName"))
            {
                fileProperties["Author" + AveObjectModelConstant.ObjectPropertySuffix] = file.Author.LoginName;
            }
            if (file.IsObjectPropertyInstantiated("ModifiedBy") && file.ModifiedBy.IsPropertyAvailable("LoginName"))
            {
                fileProperties["ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix] = file.ModifiedBy.LoginName;
            }

            CopyProperty(fileProperties, file);
            fileProperties["CustomizedPageStatus"] = (int)file.CustomizedPageStatus;
        }

        public virtual void AssembleFolderProperties(AveClientContext context, string webServerRelativeUrl, Folder folder, string folderServerRelativeUrl, Dictionary<string, object> folderProp)
        {
            LoadFolderProperties(context, webServerRelativeUrl, Guid.Empty, folder, folderProp);
            folderProp.Add("Url", TrimFolderUrl(webServerRelativeUrl, folder.ServerRelativeUrl));
            folderProp.Add("ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix, folder.ParentFolder.ServerRelativeUrl);
        }

        protected String TrimFolderUrl(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            string url = string.Empty;
            if (folderServerRelativeUrl.TrimEnd('/').Equals(webServerRelativeUrl.TrimEnd('/')))
            {
                url = string.Empty;
            }
            else
            {
                url = folderServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            return url;
        }

        protected void AssembleItemProperties(Dictionary<string, object> listItemProperty, ListItem listItem)
        {
            CopyProperty(listItemProperty, listItem);
            Dictionary<string, object> fieldValues = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> fieldValue in listItem.FieldValues)
            {
                AssembleItemProperties(fieldValues, fieldValue.Value, fieldValue.Key);
            }
            listItemProperty["FieldValues"] = fieldValues;
        }

        protected virtual void AssembleItemProperties(Dictionary<string, object> props, object fieldValue, string fieldName)
        {
            if (fieldValue != null && !AveTypeHelper.IsBasicType(fieldValue))
            {
                if (fieldValue is FieldUserValue[])
                {
                    StringBuilder fieldUserValue = new StringBuilder();
                    foreach (FieldUserValue userValue in fieldValue as FieldUserValue[])
                    {
                        GetFieldLookupValue(fieldUserValue, userValue.LookupId, userValue.LookupValue);
                    }
                    if (fieldUserValue.Length > 0)
                    {
                        fieldValue = fieldUserValue.ToString(0, fieldUserValue.Length - 2);
                    }
                    else
                    {
                        fieldValue = fieldUserValue.ToString();
                    }
                }
                else if (fieldValue is FieldLookupValue[])
                {
                    StringBuilder fieldLookupValue = new StringBuilder();
                    foreach (FieldLookupValue lookupValue in fieldValue as FieldLookupValue[])
                    {
                        GetFieldLookupValue(fieldLookupValue, lookupValue.LookupId, lookupValue.LookupValue);
                    }
                    if (fieldLookupValue.Length > 0)
                    {
                        fieldValue = fieldLookupValue.ToString(0, fieldLookupValue.Length - 2);
                    }
                    else
                    {
                        fieldValue = fieldLookupValue.ToString();
                    }
                }
                else if (fieldValue is FieldUserValue)
                {
                    StringBuilder fieldUserValue = new StringBuilder();
                    FieldUserValue userValue = (fieldValue as FieldUserValue);
                    GetFieldLookupValue(fieldUserValue, userValue.LookupId, userValue.LookupValue);
                    fieldValue = fieldUserValue.ToString(0, fieldUserValue.Length - 2);
                }
                else if (fieldValue is FieldLookupValue)
                {
                    StringBuilder fieldLookupValue = new StringBuilder();
                    FieldLookupValue lookupValue = (fieldValue as FieldLookupValue);
                    if (string.IsNullOrEmpty(lookupValue.LookupValue))
                    {
                        fieldValue = string.Empty;
                    }
                    else
                    {
                        GetFieldLookupValue(fieldLookupValue, lookupValue.LookupId, lookupValue.LookupValue, string.Equals(fieldName, "_CheckinComment", StringComparison.OrdinalIgnoreCase) ? false : true);
                        fieldValue = fieldLookupValue.ToString(0, fieldLookupValue.Length - 2);
                    }
                }
                else if (fieldValue is FieldUrlValue)
                {
                    FieldUrlValue urlValue = (fieldValue as FieldUrlValue);
                    StringBuilder fieldUrlValue = new StringBuilder(urlValue.Url);
                    fieldUrlValue.Append(", ");
                    fieldUrlValue.Append(urlValue.Description);
                    fieldValue = fieldUrlValue.ToString();
                }
                else
                {
                    fieldValue = fieldValue.ToString();
                }
            }
            else if (fieldName.Equals("QuickAddGroups"))
            {
                fieldValue = GetQuickAddGroupsProp(fieldValue as string[]);
            }
            props[fieldName] = fieldValue;
        }

        protected string GetQuickAddGroupsProp(string[] QuickAddGroups)
        {
            if (QuickAddGroups != null)
            {
                StringBuilder Groups = new StringBuilder();
                foreach (string setting in QuickAddGroups)
                {
                    Groups.Append(";#" + setting);
                }
                Groups.Append(";#");
                return Groups.ToString();
            }
            return string.Empty;
        }

        protected void GetFieldLookupValue(StringBuilder builder, int lookupId, string lookupValue, bool needKeepLookupId = true)
        {
            if (needKeepLookupId)
            {
                builder.Append(lookupId);
                builder.Append(";#");
            }
            //ADO-178776
            double number;
            if (!string.IsNullOrEmpty(lookupValue)
                && lookupValue.Contains('.')
                && lookupValue.EndsWith("0", StringComparison.OrdinalIgnoreCase)
                && Double.TryParse(lookupValue, out number))
            {
                builder.Append(number.ToString());
            }
            else
            {
                builder.Append(lookupValue);
            }
            builder.Append(";#");
        }

        public virtual void AssembleDiscoverItemProperties(Dictionary<string, object> listItemProperty, ListItem listItem)
        {
            CopyProperty(listItemProperty, listItem);
            listItemProperty["DocID"] = listItem.FieldValues["UniqueId"];
            if (listItem.FieldValues.ContainsKey("FileLeafRef"))
            {
                listItemProperty["LeafName"] = listItem.FieldValues["FileLeafRef"];
            }
            else
            {
                listItemProperty["LeafName"] = string.Empty;
            }
            ;
            listItemProperty["ID"] = listItem.FieldValues["ID"];
            if (listItem.FieldValues.ContainsKey("GUID"))
            {
                listItemProperty["tp_GUID"] = listItem.FieldValues["GUID"];
                listItemProperty["GUID"] = listItem.FieldValues["GUID"];
            }
            else
            {
                listItemProperty["tp_GUID"] = Guid.Empty;
                listItemProperty["GUID"] = Guid.Empty;
            }
            if (listItem.FieldValues.ContainsKey("File_x0020_Size") && listItem.FieldValues["File_x0020_Size"].ToString() != string.Empty)
            {
                listItemProperty["HasStream"] = 1;
            }
            else
            {
                listItemProperty["HasStream"] = 0;
            }

            listItemProperty["Size"] = 0;   //Can not get this property.
            listItemProperty["FullUrl"] = listItem.FieldValues["FileRef"];
            listItemProperty["DirName"] = listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
            listItemProperty["Level"] = Convert.ToByte(listItem.FieldValues["_Level"]);
            listItemProperty["UIVersion"] = listItem.FieldValues["_UIVersion"];
            listItemProperty["TimeLastModified"] = listItem.FieldValues["Modified"];
            listItemProperty["Type"] = Convert.ToByte((int)listItem.FileSystemObjectType);
            listItemProperty["DocFlags"] = (int?)null;  //Can not get this property.
            listItemProperty["ParentID"] = Guid.Empty;
            listItemProperty["Hidden"] = (listItemProperty["ID"] == null) ? true : false;
            listItemProperty["QueryType"] = 2;
            listItemProperty["IsCurrentVersion"] = listItem.FieldValues["_IsCurrentVersion"];
            listItemProperty["_IsCurrentVersion"] = listItem.FieldValues["_IsCurrentVersion"];
        }

        public virtual void AssembleDiscoverWebProperties(Dictionary<string, object> webProperty, Web web, string siteServerRelativeUrl)
        {
            webProperty["WebID"] = web.Id;
            webProperty["Title"] = web.Title;
            webProperty["FullUrl"] = web.ServerRelativeUrl;
            webProperty["SubWebs"] = new Dictionary<Guid, object>();
            string name = string.Empty;
            if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                int index = siteServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase) ? 1 : siteServerRelativeUrl.Length + 1;
                name = web.ServerRelativeUrl.Substring(index);
            }
            else
            {
                name = ".";
            }
            webProperty["Name"] = name;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        public virtual void AssembleViewFileProperties(Dictionary<string, object> listItemProperty, ClientFile file)
        {
            CopyProperty(listItemProperty, file);
            if (!string.IsNullOrEmpty(file.ETag))
            {
                int index = file.ETag.IndexOf(',');
                string guid = file.ETag.Substring(1, index - 1);
                string id = file.ETag.Substring(index + 1, file.ETag.Length - index - 2);
                listItemProperty["GUID"] = new Guid(guid);
                listItemProperty["Id"] = default(int);//Convert.ToInt32(id);
                listItemProperty["ID"] = default(int);
            }
            listItemProperty["LeafName"] = file.Name;
            if (listItemProperty.ContainsKey("GUID"))
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = listItemProperty["GUID"];
            }
            else
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = Guid.Empty;
            }
            listItemProperty["HasStream"] = 1;
            listItemProperty["Size"] = 0;   //Can not get this property.
            listItemProperty["FullUrl"] = file.ServerRelativeUrl;//listItem.FieldValues["FileRef"];
            listItemProperty["FileDirRef"] = listItemProperty["DirName"] = file.ServerRelativeUrl.Contains('/') ? file.ServerRelativeUrl.Substring(0, file.ServerRelativeUrl.LastIndexOf('/')) : file.Name;//listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
            listItemProperty["Level"] = Convert.ToByte((int)file.Level);//listItem.FieldValues["_Level"]);
            listItemProperty["UIVersion"] = file.UIVersion;//listItem.FieldValues["_UIVersion"];
            listItemProperty["TimeLastModified"] = file.TimeLastModified;//listItem.FieldValues["Modified"];
            listItemProperty["Type"] = Convert.ToByte(1);//Convert.ToByte((int)listItem.FileSystemObjectType);
            listItemProperty["DocFlags"] = (int?)file.Tag; //(int?)null;  //Can not get this property.
            listItemProperty["ParentID"] = Guid.Empty;
            if (listItemProperty.ContainsKey("Id") && listItemProperty["Id"] != null)
            {
                listItemProperty["Hidden"] = true;
            }
            else
            {
                listItemProperty["Hidden"] = false;
            }
            listItemProperty["ObjType"] = 2;
            listItemProperty["QueryType"] = 2;
            listItemProperty["IsCurrentVersion"] = true;//listItem.FieldValues["_IsCurrentVersion"];
            listItemProperty["_IsCurrentVersion"] = true;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "obj is a key")]
        public virtual void AssembleSystemFolderProperties(AveClientContext context, Dictionary<string, object> listItemProperty, Folder folder, string webServerRelativeUrl)
        {
            CopyProperty(listItemProperty, folder);
            //if (!string.IsNullOrEmpty(file.ETag))
            //{
            //    int index = file.ETag.IndexOf(',');
            //    string guid = file.ETag.Substring(1, index - 1);
            //    string id = file.ETag.Substring(index + 1, file.ETag.Length - index - 2);
            //    listItemProperty["Guid"] = new Guid(guid);
            //    listItemProperty["ID"] = Convert.ToInt32(id);
            //}
            listItemProperty["GUID"] = Guid.Empty;
            listItemProperty["Id"] = 0;
            listItemProperty["ID"] = 0;
            listItemProperty["ItemId"] = 0;
            listItemProperty["LeafName"] = folder.Name;
            if (listItemProperty.ContainsKey("GUID"))
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = listItemProperty["GUID"];
            }
            else
            {
                listItemProperty["DocID"] = listItemProperty["tp_GUID"] = Guid.Empty;
            }
            listItemProperty["HasStream"] = 0;
            listItemProperty["Size"] = 0;   //Can not get this property.
            listItemProperty["FullUrl"] = folder.ServerRelativeUrl;//listItem.FieldValues["FileRef"];
            listItemProperty["FileDirRef"] = listItemProperty["DirName"] = folder.ServerRelativeUrl.Contains('/') ? folder.ServerRelativeUrl.Substring(0, folder.ServerRelativeUrl.LastIndexOf('/')) : folder.Name;//listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
            listItemProperty["Level"] = Convert.ToByte(1);//listItem.FieldValues["_Level"]);
            //代码中统一为“UIVersion”
            listItemProperty["UIVersion"] = 512;//listItem.FieldValues["_UIVersion"];
            listItemProperty["TimeLastModified"] = DateTime.MinValue;//listItem.FieldValues["Modified"];
            listItemProperty["Type"] = Convert.ToByte(1);//Convert.ToByte((int)listItem.FileSystemObjectType);
            listItemProperty["DocFlags"] = (int?)folder.Tag; //(int?)null;  //Can not get this property.
            listItemProperty["ParentID"] = Guid.Empty;
            listItemProperty["Hidden"] = true;
            listItemProperty["QueryType"] = 2;
            listItemProperty["IsCurrentVersion"] = true;//listItem.FieldValues["_IsCurrentVersion"];
            listItemProperty["_IsCurrentVersion"] = true;
            listItemProperty["Url"] = TrimFolderUrl(webServerRelativeUrl, folder.ServerRelativeUrl);
            listItemProperty["ObjType"] = 4;
        }

        protected void AssembleContentTypesProperties(Dictionary<string, object> contentTypesProperties, ContentTypeCollection contentTypeCol)
        {
            List<Dictionary<string, object>> contentTypePropertiesList = new List<Dictionary<string, object>>(contentTypeCol.Count);
            foreach (ContentType contentType in contentTypeCol)
            {
                Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                this.AssembleSingleContentTypeProperties(contentTypeProperties, contentType);
                contentTypePropertiesList.Add(contentTypeProperties);
            }
            contentTypesProperties[AveObjectModelConstant.ChildrenProperties] = contentTypePropertiesList;
        }
        protected void AssembleSingleContentTypeProperties(Dictionary<string, object> contentTypeProperties, ContentType contentType)
        {
            CopyProperty(contentTypeProperties, contentType);
            contentTypeProperties.Remove("Id");
            //these properties can't get from client api, so get it from schemal
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = contentType.SchemaXml;
            XmlElement ctElement = doc.FirstChild as XmlElement;
            string attributeValue = ctElement.GetAttribute("Sealed");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                contentTypeProperties["Sealed"] = Convert.ToBoolean(attributeValue);
            }
            string featureId = ctElement.GetAttribute("FeatureId");
            if (!string.IsNullOrEmpty(featureId))
            {
                contentTypeProperties["FeatureId"] = new Guid(featureId);
            }
            contentTypeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = contentType.Id.ToString();
            contentTypeProperties["ParentId"] = contentType.Parent.Id.ToString();
        }
        public virtual void CopyProperty(Dictionary<string, object> proDic, ClientObject Obj)
        {
            ClientObjectData objData = GetProperty(Obj);
            Dictionary<string, object> clientObjData = objData.Properties;
            foreach (KeyValuePair<string, object> propertyInfo in clientObjData)
            {
                object obj = propertyInfo.Value;
                if (obj == null)
                {
                    proDic[propertyInfo.Key] = null;
                }
                else
                {
                    Type proType = obj.GetType();
                    if (proType.IsEnum)
                    {
                        proDic[propertyInfo.Key] = AveTypeHelper.CastEnumValue((obj));
                    }
                    else
                    {
                        proDic[propertyInfo.Key] = obj;
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        protected virtual void AssembleSingleFieldProperties(Dictionary<string, object> fieldProperties, Field field)
        {
            CopyProperty(fieldProperties, field);
            fieldProperties["BaseTypeString"] = AssembleFieldBaseTypeString(field);
            //these properties can't get from client api, so get it from schemal
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = field.SchemaXml;
            XmlElement fElement = doc.FirstChild as XmlElement;
            string attributeValue = fElement.GetAttribute("AllowDeletion");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["AllowDeletion"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("Indexed");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["Indexed"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("LinkToItemAllowed");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                if (attributeValue == "Prohibited")
                {
                    fieldProperties["LinkToItemAllowed"] = false;
                }
                else if (attributeValue == "Required")
                {
                    fieldProperties["LinkToItemAllowed"] = true;
                }
            }
            if (fieldProperties.ContainsKey("LinkToItemAllowed"))
            {
                fieldProperties["LinkToItem"] = fieldProperties["LinkToItemAllowed"];
            }
            else
            {
                attributeValue = fElement.GetAttribute("LinkToItem");
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    fieldProperties["LinkToItem"] = Convert.ToBoolean(attributeValue);
                }
            }
            attributeValue = fElement.GetAttribute("NoCrawl");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["NoCrawl"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("Reorderable");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["Reorderable"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("ShowInDisplayForm");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["ShowInDisplayForm"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("ShowInEditForm");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["ShowInEditForm"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("ShowInListSettings");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["ShowInListSettings"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("ShowInNewForm");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["ShowInNewForm"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("ShowInVersionHistory");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["ShowInVersionHistory"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("ShowInViewForms");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["ShowInViewForms"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("RowOrdinal");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["RowOrdinal"] = Convert.ToInt32(attributeValue);
            }
            attributeValue = fElement.GetAttribute("AggregationFunction");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["AggregationFunction"] = fElement.GetAttribute("AggregationFunction");
            }

            fieldProperties["ColName"] = fElement.GetAttribute("ColName");

            attributeValue = GetSingleNodeValue(fElement, "DefaultFormula", false);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["DefaultFormula"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("DisplaySize");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["DisplaySize"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("IMEMode");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["IMEMode"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("JumpToField");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["JumpToField"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PIAttribute");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PIAttribute"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PITarget");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PITarget"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PrimaryPIAttribute");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PrimaryPIAttribute"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("PrimaryPITarget");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["PrimaryPITarget"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("RelatedField");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["RelatedField"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("SchemaXmlWithResourceTokens");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["SchemaXmlWithResourceTokens"] = attributeValue;
            }
            attributeValue = GetSingleNodeValue(fElement, "Translations", true);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["TranslationXml"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("SourceID");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["SourceId"] = attributeValue;
            }

            fieldProperties["Type"] = (int)field.FieldTypeKind;
            fieldProperties["ObjectPath"] = GetObjectPathString(field.Path);
            fieldProperties["FieldType"] = field.GetType();
            //fieldProperties["ClientContext"] = mClientContext;

            attributeValue = fElement.GetAttribute("DifferencingLimit");
            if (string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["DifferencingLimit"] = 0x5dc;
            }
            else
            {
                fieldProperties["DifferencingLimit"] = Convert.ToInt32(attributeValue);
            }
            attributeValue = fElement.GetAttribute("JumpToNo");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["JumpToNo"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("JumpToYes");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["JumpToYes"] = attributeValue;
            }
            attributeValue = fElement.GetAttribute("UnlimitedLengthInDocumentLibrary");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["UnlimitedLengthInDocumentLibrary"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("RichTextMode");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["RichTextMode"] = (AveRichTextMode)Enum.Parse(typeof(AveRichTextMode), attributeValue);
            }
            attributeValue = fElement.GetAttribute("IsolateStyles");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["IsolateStyles"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("CountRelated");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["CountRelated"] = Convert.ToBoolean(attributeValue);
            }
            attributeValue = fElement.GetAttribute("Node");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                fieldProperties["XPath"] = attributeValue;
            }
            FieldMultiChoice fieldMC = field as FieldMultiChoice;
            if (fieldMC != null && fieldMC.Choices != null)
            {
                StringCollection sc = new StringCollection();
                sc.AddRange(fieldMC.Choices);
                fieldProperties["Choices"] = sc;
            }
            if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
            {
                GetCustomization(doc, fieldProperties);
            }
        }

        protected virtual string AssembleFieldBaseTypeString(Field field)
        {
            string mBaseFieldType = field.TypeAsString;
            if (field.FieldTypeKind == FieldType.Invalid)
            {
                if (field is FieldUser)
                {
                    if ((field as FieldUser).AllowMultipleValues)
                    {
                        mBaseFieldType = "UserMulti";
                    }
                    mBaseFieldType = "User";
                }
                else if (field is FieldLookup)
                {
                    if (!mBaseFieldType.StartsWith("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((field as FieldLookup).AllowMultipleValues)
                        {
                            mBaseFieldType = "LookupMulti";
                        }
                        mBaseFieldType = "Lookup";
                    }
                }
            }
            return mBaseFieldType;
        }

        private void GetCustomization(XmlDocument doc, Dictionary<string, object> fieldProperties)
        {
            foreach (XmlElement customElement in doc.FirstChild.ChildElements())
            {
                if (customElement.Name.Equals("Customization"))
                {
                    foreach (XmlElement element in customElement.ChildElements())
                    {
                        if (element.Name.Equals("ArrayOfProperty"))
                        {
                            foreach (XmlElement propertyElement in element.ChildElements())
                            {
                                try
                                {
                                    if (propertyElement.Name.Equals("Property"))
                                    {
                                        string name = null;
                                        object value = null;
                                        XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement nameElement = (XmlElement)elements[0];
                                            name = nameElement.InnerText;
                                        }
                                        elements = propertyElement.GetElementsByTagName("Value");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement valueElement = (XmlElement)elements[0];
                                            string text = valueElement.InnerText;
                                            string type = valueElement.GetAttribute("p4:type");
                                            type = type.Substring(type.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);

                                            if (name.Equals("TextField") || name.Equals("SspId") || name.Equals("GroupId") || name.Equals("TermSetId") || name.Equals("AnchorId"))
                                            {
                                                type = "guid";
                                                string tValue = valueElement.InnerText;
                                                if (tValue.Contains('|'))
                                                {
                                                    string[] temp = tValue.ToString().Split('|');
                                                    if (temp.Length == 2)
                                                    {
                                                        fieldProperties.Add(name, valueElement.InnerText);
                                                        valueElement.InnerText = temp[0];
                                                        continue;
                                                    }
                                                }
                                            }
                                            switch (type)
                                            {
                                                case "datetime":
                                                    value = Convert.ToDateTime(valueElement.InnerText);
                                                    break;
                                                case "boolean":
                                                    value = Convert.ToBoolean(valueElement.InnerText);
                                                    break;
                                                case "guid":
                                                    value = new Guid(valueElement.InnerText);
                                                    break;
                                                case "int32":
                                                case "int":
                                                    value = Convert.ToInt32(valueElement.InnerText);
                                                    break;
                                                case "double":
                                                    value = Convert.ToDouble(valueElement.InnerText);
                                                    break;
                                                default:
                                                    value = valueElement.InnerText;
                                                    break;
                                            }
                                        }
                                        if (!String.IsNullOrEmpty(name) && !fieldProperties.ContainsKey(name))
                                        {
                                            fieldProperties.Add(name, value);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    string errorMsg = e.ToString();
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }

        protected string GetSingleNodeValue(XmlElement xmlElement, string nodeName, bool outXml)
        {
            XmlNode node = xmlElement.SelectSingleNode(nodeName);
            if (node != null)
            {
                if (outXml)
                {
                    return node.OuterXml;
                }
                else
                {
                    return node.InnerText;
                }
            }
            return string.Empty;
        }

        protected void AssembleFeatureProperties(Dictionary<string, object> featureProperties, Feature feature)
        {
            featureProperties["DefinitionId"] = feature.DefinitionId;
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProperties["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
        }

        protected void AssembleWebPartManagerProperties(Dictionary<string, object> limitedWebPartManagerProperties, LimitedWebPartManager limitedWebPartManager)
        {
            Dictionary<string, object> webpartsProperties = new Dictionary<string, object>();
            WebPartDefinitionCollection webpartDefCol = limitedWebPartManager.WebParts;
            AssembleWebPartsProperties(webpartsProperties, webpartDefCol);
            limitedWebPartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] = webpartsProperties;
        }

        protected void AssembleWebPartsProperties(Dictionary<string, object> webpartColProperties, WebPartDefinitionCollection webpartDefCol)
        {
            List<Dictionary<string, object>> webpartPropertiesList = new List<Dictionary<string, object>>();
            foreach (WebPartDefinition webpartDefinition in webpartDefCol)
            {
                Dictionary<string, object> webpartProperties = new Dictionary<string, object>();
                AssembleWebPartProperties(webpartProperties, webpartDefinition.WebPart);
                webpartPropertiesList.Add(webpartProperties);
            }
            webpartColProperties[AveObjectModelConstant.ChildrenProperties] = webpartPropertiesList;
        }

        protected void AssembleWebPartProperties(Dictionary<string, object> webpartProperties, WebPart webpart)
        {
            CopyProperty(webpartProperties, webpart);
        }

        public virtual void AssembleViewProperties(Dictionary<string, object> viewProperties, View view, string webServerRelativeUrl)
        {
            CopyProperty(viewProperties, view);
            if (viewProperties.ContainsKey("ContentTypeId"))
            {
                viewProperties.Remove("ContentTypeId");
            }
            viewProperties["ContentTypeId" + AveObjectModelConstant.ObjectPropertySuffix] = view.ContentTypeId.ToString();
            viewProperties["Query"] = view.ViewQuery;
            viewProperties["Type"] = view.ViewType;
            viewProperties["Url"] = view.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            //viewFields
            Dictionary<string, object> viewFields = new Dictionary<string, object>();
            List<string> viewFieldsList = new List<string>();
            for (int i = 0; i < view.ViewFields.Count; ++i)
            {
                viewFieldsList.Add(view.ViewFields[i]);
            }
            viewFields[AveObjectModelConstant.ChildrenProperties] = viewFieldsList;
            viewFields["SchemaXml"] = view.ViewFields.SchemaXml;
            viewProperties["ViewFields" + AveObjectModelConstant.ObjectPropertySuffix] = viewFields;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Uiversion used as key, change to UIVersion in DA6.2.")]
        public virtual void AssembleDiscoverViewProperties(Dictionary<string, object> viewProperties, View view, ClientFile file)
        {
            string etag = file.ETag;
            string id = etag.Substring(etag.IndexOf("{", StringComparison.OrdinalIgnoreCase) + 1, etag.IndexOf("}", StringComparison.OrdinalIgnoreCase) - etag.IndexOf("{", StringComparison.OrdinalIgnoreCase) - 1);
            Guid docId = new Guid(id);
            string dirName = view.ServerRelativeUrl.Substring(0, view.ServerRelativeUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).TrimStart('/');
            viewProperties.Add("ViewID", view.Id);
            viewProperties.Add("ViewType", 0);    //Can not get this property 
            viewProperties.Add("IsPersonalView", view.PersonalView);
            viewProperties.Add("BaseViewId", Convert.ToByte(view.BaseViewId));
            viewProperties.Add("ViewTitle", view.Title);
            viewProperties.Add("PageUrlID", docId);
            viewProperties.Add("ViewUserID", (int?)null);//Can not get this property 
            viewProperties.Add("DocID", docId);
            viewProperties.Add("DirName", dirName);
            viewProperties.Add("LeafName", file.Name);
            viewProperties.Add("ID", (file.ListItemAllFields.FieldValues.Count == 0) ? (int?)null : file.ListItemAllFields.Id);
            viewProperties.Add("Uiversion", file.UIVersion);
            viewProperties.Add("DocFlags", (int?)null);   //Can not get this property
            viewProperties.Add("TimeLastModified", file.TimeLastModified);
            viewProperties.Add("Level", Convert.ToByte(file.Level));
            viewProperties.Add("Type", Convert.ToByte(0));
            viewProperties.Add("Size", 0);     //Can not get this property
            viewProperties.Add("ParentID", Guid.Empty);    //Can not get this property
            viewProperties.Add("FullUrl", view.ServerRelativeUrl);
        }

        protected Principal GetPrincipalByLoginName(Web web, Dictionary<string, object> properties)
        {
            string loginName = properties["MemberLoginName"] as string;
            string memeberType = properties["MemberType"] as string;
            Principal member = null;
            switch (memeberType)
            {
                case "Group":
                    int groupId = (int)properties["MemberId"];
                    member = web.SiteGroups.GetById(groupId);
                    break;
                default:
                    member = web.EnsureUser(loginName);
                    break;
            }
            return member;
        }

        protected Dictionary<string, object> GetGroupProperties(SecurityTrimObject webTrimObj, ClientContext context, Group group, bool skipUsers)
        {
            Dictionary<string, object> siteGroupProperties = new Dictionary<string, object>();
            //bool neekSkipUsers = false;
            if (!skipUsers)
            {
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                ExceptionHandlingScope excepSubScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(group, g => g.Users);
                        context.Load(group, g => g.Owner.Id, g => g.Owner.PrincipalType, g => g.Title);
                    }
                    using (excepScope.StartCatch())
                    {
                        using (excepSubScope.StartScope())
                        {
                            using (excepSubScope.StartTry())
                            {
                                context.Load(group, g => g.Owner.Id, g => g.Owner.PrincipalType, g => g.Title);
                            }
                            using (excepSubScope.StartCatch())
                            {
                            }
                        }
                    }
                }

                context.ExecuteQuery();
                if (excepSubScope.HasException)
                {
                    return siteGroupProperties;
                }

                if (excepScope.ServerErrorCode == ErrorCodes.AccessDenied)
                {
                    skipUsers = true;
                    SecurityTrimObject groupTrimObj = webTrimObj.Children.Find(g => string.Equals(g.Name, group.Title, StringComparison.OrdinalIgnoreCase));
                    if (groupTrimObj == null)
                    {
                        groupTrimObj = new SecurityTrimObject() { Level = SecurityTrimLevel.WebProperties };
                        groupTrimObj.Name = group.Title;
                        groupTrimObj.Type = group.Owner.PrincipalType.ToString();
                        groupTrimObj.TrimmedProperties["Users"] = mAccessDeniedMessage;
                        webTrimObj.Children.Add(groupTrimObj);
                    }
                }
            }
            CopyProperty(siteGroupProperties, group);
            if (group.ServerObjectIsNull.HasValue && !group.ServerObjectIsNull.Value)
            {
                siteGroupProperties["Name"] = group.Title;
                siteGroupProperties["OwnerId"] = group.Owner.Id;
                siteGroupProperties["OwnerType"] = group.Owner.PrincipalType.ToString();
                siteGroupProperties["Exists"] = true;
                //Dictionary<string, object> users = new Dictionary<string, object>();
                //List<Dictionary<string, object>> userList = new List<Dictionary<string, object>>();

                if (!skipUsers)
                {
                    List<string> users = new List<string>();
                    foreach (User user in group.Users)
                    {
                        //Dictionary<string, object> userProperties = new Dictionary<string, object>();
                        //AveObjectCopy.GetObjectBasicProperties(userProperties, user);
                        //userProperties.Add("Name", user.Title);
                        //userList.Add(userProperties);
                        users.Add(user.LoginName);
                        //users.Add("ChildrenProperties", userList);
                        siteGroupProperties["Users" + AveObjectModelConstant.ObjectPropertySuffix] = users;
                    }
                }
            }
            else
            {
                siteGroupProperties["Exists"] = false;
            }
            return siteGroupProperties;
        }

        //roleassignment may come from web or list or listitem
        protected bool GetRoleAssignment(Site site, Web pricipalBelongedWeb, Dictionary<string, object> properties, out Principal principal, out RoleDefinitionBindingCollection roleDefinitionBindingCol)
        {
            bool rdbcUpdated = false;
            bool isNewCreated = (bool)properties[AveObjectModelConstant.IsNewCreated];
            Principal member = GetPrincipalByLoginName(pricipalBelongedWeb, properties);
            RoleAssignment roleAssignment = null;
            if (isNewCreated)
            {
                principal = member;
                roleDefinitionBindingCol = new RoleDefinitionBindingCollection(site.Context);
                foreach (string roleDefinitionName in properties["RoleDefinitionBindingCollection"] as List<string>)
                {
                    if (!roleDefinitionName.Equals("Limited Access", StringComparison.OrdinalIgnoreCase))
                    {
                        RoleDefinition roleDef = pricipalBelongedWeb.RoleDefinitions.GetByName(roleDefinitionName);
                        roleDefinitionBindingCol.Add(roleDef);
                        rdbcUpdated = true;
                    }
                }
            }
            else
            {
                string webServerRelativeUrl = properties[AveObjectModelConstant.WebServerRelativeUrl] as string;
                Web web = site.OpenWeb(webServerRelativeUrl);

                RoleAssignmentCollection roleAssignmentCol = null;
                if (properties.ContainsKey(AveObjectModelConstant.ListTitle))
                {
                    string listTitle = properties[AveObjectModelConstant.ListTitle] as string;
                    List list = web.Lists.GetByTitle(listTitle);
                    if (properties.ContainsKey(AveObjectModelConstant.ItemId))
                    {
                        int itemId = (int)properties[AveObjectModelConstant.ItemId];
                        ListItem listItem = list.GetItemById(itemId);
                        roleAssignmentCol = listItem.RoleAssignments;
                    }
                    else
                    {
                        roleAssignmentCol = list.RoleAssignments;
                    }
                }
                else
                {
                    roleAssignmentCol = web.RoleAssignments;
                }
                roleAssignment = roleAssignmentCol.GetByPrincipal(member);
                principal = roleAssignment.Member;
                roleDefinitionBindingCol = roleAssignment.RoleDefinitionBindings;
                rdbcUpdated = true;
            }
            return rdbcUpdated;
        }
        internal static BasePermissions ConvertULongToBasePerm(ulong aveBasePerm)
        {
            BasePermissions basePerm = new BasePermissions();
            ulong permValue = (ulong)aveBasePerm;
            AveReflectionUtility.SetFieldValue("m_high", basePerm, (uint)(permValue >> 32));
            AveReflectionUtility.SetFieldValue("m_low", basePerm, (uint)aveBasePerm);
            return basePerm;
        }
        internal static ulong ConvertBasePermToULong(BasePermissions basePerm)
        {
            if (basePerm == null)
            {
                return 0;
            }
            uint high = (uint)AveReflectionUtility.GetFieldValue("m_high", basePerm);
            uint low = (uint)AveReflectionUtility.GetFieldValue("m_low", basePerm);
            return ((ulong)high << 32) | low;
        }
        internal void ConvertToChangeObject(ChangeCollection changeCollection, Dictionary<string, object> changeCache)
        {
            Dictionary<Guid, object> changedSiteCache = changeCache["ChangedSiteCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedWebCache = changeCache["ChangedWebCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedListCache = changeCache["ChangedListCache"] as Dictionary<Guid, object>;
            Dictionary<string, object> changedItemsCache = changeCache["ChangedItemsCache"] as Dictionary<string, object>;
            Dictionary<Guid, object> changedFolderCache = changedItemsCache["ChangedFolderCache"] as Dictionary<Guid, object>;
            Dictionary<Guid, object> changedFileCache = changedItemsCache["ChangedFileCache"] as Dictionary<Guid, object>;
            Dictionary<string, object> changedItemCache = changedItemsCache["ChangedItemCache"] as Dictionary<string, object>;

            foreach (Change changeObject in changeCollection)
            {
                Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                Dictionary<string, object> tempProperties = new Dictionary<string, object>();
                CopyProperty(objectProperties, changeObject);
                AveChangeType preChangeType = AveChangeType.None;
                SPChangeType currentChangeType = (SPChangeType)objectProperties["ChangeType"];
                switch (changeObject.GetType().ToString())
                {
                    case "Microsoft.SharePoint.Client.ChangeItem":
                        Guid itemWebId = new Guid(objectProperties["WebId"].ToString());
                        Guid itemListId = new Guid(objectProperties["ListId"].ToString());
                        tempProperties = ConvertToChangeItem(changeObject, currentChangeType, itemListId, objectProperties, changedItemCache);
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        tempProperties["WebId"] = itemWebId;
                        if (!changedWebCache.ContainsKey(itemWebId))
                        {
                            changedWebCache[itemWebId] = tempProperties;
                        }
                        tempProperties["ListId"] = itemListId;
                        if (!changedListCache.ContainsKey(itemListId))
                        {
                            changedListCache[itemListId] = tempProperties;
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeFile":
                    case "Microsoft.SharePoint.Client.ChangeFolder":
                        bool isFile = changeObject.GetType().ToString().Equals("Microsoft.SharePoint.Client.ChangeFile");
                        Guid folderWebId = new Guid(objectProperties["WebId"].ToString());
                        Dictionary<Guid, object> changeFileOrFolderCache = isFile ? changedFileCache : changedFolderCache;
                        tempProperties = ConvertToChangeFileOrFolder(changeObject, currentChangeType, true, objectProperties, changeFileOrFolderCache);
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        tempProperties["WebId"] = folderWebId;
                        if (!changedWebCache.ContainsKey(folderWebId))
                        {
                            changedWebCache[folderWebId] = tempProperties;
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeList":
                        tempProperties = ConvertToChangeList(changeObject, currentChangeType, preChangeType, objectProperties, changedListCache);
                        Guid parentWebId = new Guid(objectProperties["WebId"].ToString());
                        #region Fill parent change cache
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        tempProperties["WebId"] = parentWebId;
                        if (!changedWebCache.ContainsKey(parentWebId))
                        {
                            changedWebCache[parentWebId] = tempProperties;
                        }
                        #endregion
                        break;
                    case "Microsoft.SharePoint.Client.ChangeView":
                        ConvertToChangeView(changeObject, currentChangeType, preChangeType, objectProperties, changedFileCache, changedListCache, changedWebCache, changedSiteCache);
                        break;
                    case "Microsoft.SharePoint.Client.ChangeWeb":
                        //case "Microsoft.SharePoint.Client.ChangeGroup":
                        //case "Microsoft.SharePoint.Client.ChangeUser":
                        //case "Microsoft.SharePoint.Client.ChangeField":
                        //case "Microsoft.SharePoint.Client.ChangeAlert":
                        //由于只知道Alert对象，但是无法知道是哪个alert对象变了，所以没变要处理Alert:
                        //case "Microsoft.SharePoint.Client.ChangeContentType":
                        tempProperties = ConvertToChangeWeb(changeObject, currentChangeType, preChangeType, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        break;
                    case "Microsoft.SharePoint.Client.ChangeAlert":
                        //由于只知道Alert对象，但是无法知道是哪个alert对象变了，等支持Alert的时候再处理，需要遍历Alert对象是哪个对象变化了。
                        break;
                    case "Microsoft.SharePoint.Client.ChangeField":
                        tempProperties = ConvertToChangeField(changeObject, currentChangeType, preChangeType, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        break;
                    case "Microsoft.SharePoint.Client.ChangeContentType":
                        tempProperties = ConvertToChangeContentType(changeObject, currentChangeType, preChangeType, objectProperties, changedWebCache);
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                        {
                            changedSiteCache[changeObject.SiteId] = tempProperties;
                        }
                        break;
                    case "Microsoft.SharePoint.Client.ChangeUser":
                        ConvertToChangeUser(changeObject, currentChangeType, preChangeType, objectProperties, changedSiteCache);
                        break;
                    case "Microsoft.SharePoint.Client.ChangeGroup":
                        ConvertToChangeGroup(changeObject, currentChangeType, preChangeType, objectProperties, changedSiteCache);
                        break;
                    case "Microsoft.SharePoint.Client.ChangeSite":
                        ConvertToChangeSite(changeObject, currentChangeType, preChangeType, objectProperties, changedSiteCache);
                        break;
                    default:
                        objectProperties["ChangeObjectType"] = ChangeObjectType.Site;
                        if (!changedSiteCache.ContainsKey(changeObject.SiteId) && objectProperties.Count > 0)
                        {
                            objectProperties["SPChangeType"] = objectProperties["ChangeType"];
                            objectProperties["ChangeType"] = ConvertChangeType(currentChangeType);
                            changedSiteCache[changeObject.SiteId] = objectProperties;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Convert SP Change Type to AveChagne Type
        /// </summary>
        /// <param name="spChangeType"></param>
        /// <returns></returns>
        protected AveChangeType ConvertChangeType(SPChangeType spChangeType)
        {
            switch (spChangeType)
            {
                case SPChangeType.Add:
                //case SPChangeType.AssignmentAdd:
                case SPChangeType.ListContentTypeAdd:
                    //case SPChangeType.RoleAdd:
                    return AveChangeType.Add;

                //case SPChangeType.MemberAdd:
                //case SPChangeType.MemberDelete:
                case SPChangeType.MoveAway:
                case SPChangeType.MoveInto:
                //case SPChangeType.Navigation:
                case SPChangeType.Rename:
                //case SPChangeType.RoleUpdate:
                case SPChangeType.SystemUpdate:
                case SPChangeType.Update:
                    //case SPChangeType.ScopeAdd:
                    //case SPChangeType.ScopeDelete:
                    return AveChangeType.Edit;

                //case SPChangeType.AssignmentDelete:
                case SPChangeType.DeleteObject:
                //case SPChangeType.RoleDelete:
                case SPChangeType.ListContentTypeDelete:
                    return AveChangeType.Delete;

                case SPChangeType.NoChange:
                    return AveChangeType.None;

                case SPChangeType.Restore:
                    return AveChangeType.Restore;

                default:
                    return AveChangeType.None;
            }
        }

        protected Dictionary<string, object> ConvertToChangeItem(Change changeObject, SPChangeType currentChangeType, Guid listId, Dictionary<string, object> objectProperties, Dictionary<string, object> changedItemCache)
        {
            bool isRenamed = true;
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            int itemId = (int)(objectProperties["ItemId"]);
            string key = listId.ToString() + ";" + itemId.ToString();
            Dictionary<string, object> itemObj = null;
            Object tempObj = null;

            if (changedItemCache.TryGetValue(key, out tempObj))
            {
                itemObj = (Dictionary<string, object>)tempObj;

                switch (currentChangeType)
                {
                    case SPChangeType.DeleteObject:
                    case SPChangeType.MoveAway:
                        itemObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Rename:
                        {
                            objectProperties["ChangeObjectType"] = ChangeObjectType.Item;
                            objectProperties["ChangeType"] = (int)AveChangeType.Add;
                            objectProperties["IsRenamed"] = isRenamed;
                            changedItemCache[key] = objectProperties;
                            itemObj = objectProperties;
                        }
                        break;
                    case SPChangeType.Restore:
                        itemObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    case SPChangeType.ScopeAdd:
                    case SPChangeType.ScopeDelete:
                    case SPChangeType.AssignmentAdd:
                    case SPChangeType.AssignmentDelete:
                        itemObj["RoleAssignmentsChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.Add:
                    case SPChangeType.MoveInto:
                        itemObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.SystemUpdate:
                    case SPChangeType.Update:
                        {
                            DateTime time = (DateTime)itemObj["Time"];
                            if ((DateTime)objectProperties["Time"] > time)
                            {
                                itemObj["Time"] = objectProperties["Time"];
                                itemObj["ChangeType"] = (int)AveChangeType.Edit;
                            }
                        }
                        break;
                    default:
                        break;
                }

            }
            else
            {
                itemObj = objectProperties;
                changedItemCache[key] = itemObj;
                objectProperties["ChangeObjectType"] = ChangeObjectType.Item;
                switch (currentChangeType)
                {
                    case SPChangeType.DeleteObject:
                    case SPChangeType.MoveAway:
                        itemObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Rename:
                        {
                            objectProperties["ChangeObjectType"] = ChangeObjectType.Item;
                            objectProperties["ChangeType"] = (int)AveChangeType.Add;
                            objectProperties["IsRenamed"] = isRenamed;
                            changedItemCache[key] = objectProperties;
                            itemObj = objectProperties;
                        }
                        break;
                    case SPChangeType.Restore:
                        itemObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    case SPChangeType.ScopeAdd:
                    case SPChangeType.ScopeDelete:
                    case SPChangeType.AssignmentAdd:
                    case SPChangeType.AssignmentDelete:
                        itemObj["RoleAssignmentsChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.Add:
                    case SPChangeType.MoveInto:
                        itemObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.SystemUpdate:
                    case SPChangeType.Update:
                        {
                            itemObj["ChangeType"] = (int)AveChangeType.Edit;
                        }
                        break;
                    default:
                        break;
                }
            }

            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.None;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        protected Dictionary<string, object> ConvertToChangeFileOrFolder(Change changeObject, SPChangeType currentChangeType, bool isFile, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedFileOrFolderCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = isFile ? ChangeObjectType.File : ChangeObjectType.Folder;
            if (currentChangeType == SPChangeType.DeleteObject)
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Delete;
            }
            else
            {
                objectProperties["ChangeType"] = (int)AveChangeType.Edit;
            }
            Guid uniqueId = new Guid(objectProperties["UniqueId"].ToString());

            changedFileOrFolderCache[uniqueId] = objectProperties;

            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.None;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        protected Dictionary<string, object> ConvertToChangeList(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedListCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.List;
            Guid listId = new Guid(objectProperties["ListId"].ToString());

            Dictionary<string, object> listObj = null;
            Object tempObj = null;

            if (changedListCache.TryGetValue(listId, out tempObj))
            {
                listObj = (Dictionary<string, object>)tempObj;
                preChangeType = (AveChangeType)listObj["ChangeType"];

                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        listObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.DeleteObject:
                        listObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.AssignmentAdd:
                    case SPChangeType.AssignmentDelete:
                    case SPChangeType.ScopeAdd:
                    case SPChangeType.ScopeDelete:
                        listObj["RoleAssignmentsChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.Update:
                    case SPChangeType.SystemUpdate:
                    case SPChangeType.ListContentTypeAdd:
                    case SPChangeType.ListContentTypeDelete:
                        {
                            if (preChangeType != AveChangeType.Add)
                            {
                                listObj["ChangeType"] = (int)AveChangeType.Edit;
                            }
                        }
                        break;
                    case SPChangeType.Restore:
                        listObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                listObj = objectProperties;
                changedListCache[listId] = listObj;
                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        listObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.DeleteObject:
                        listObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.AssignmentAdd:
                    case SPChangeType.AssignmentDelete:
                    case SPChangeType.ScopeAdd:
                    case SPChangeType.ScopeDelete:
                        listObj["RoleAssignmentsChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.Update:
                    case SPChangeType.SystemUpdate:
                    case SPChangeType.ListContentTypeAdd:
                    case SPChangeType.ListContentTypeDelete:
                        listObj["ChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.Restore:
                        listObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    default:
                        break;
                }
            }

            //if (!changedListCache.ContainsKey(listId))
            //{
            //    objectProperties["ChangeType"] = (int)AveChangeType.Edit;
            //    changedListCache[listId] = objectProperties;
            //}
            //Dictionary<string, object> listObj = changedListCache[listId] as Dictionary<string, object>;
            //preChangeType = (AveChangeType)listObj["ChangeType"];
            //currentChangeType = changeObject.ChangeType;
            //if (preChangeType == AveChangeType.Add ||
            //    preChangeType == AveChangeType.Restore)
            //{
            //    if (currentChangeType == SPChangeType.DeleteObject)
            //    {
            //        listObj["ChangeType"] = (int)AveChangeType.Delete;
            //    }
            //    //otherwise not change.
            //}
            //else //"None or Edit", change to "Edit or Delete".
            //{
            //    if (preChangeType == AveChangeType.Delete &&
            //        currentChangeType == SPChangeType.Restore)
            //    {
            //        listObj["ChangeType"] = listObj["ChangeTypeBeforeDelete"];
            //        if (preChangeType == AveChangeType.None)
            //        {
            //            changedListCache.Remove(listId);
            //        }
            //    }
            //    else if (currentChangeType == SPChangeType.DeleteObject)
            //    {
            //        listObj["ChangeTypeBeforeDelete"] = (int)preChangeType;
            //        listObj["ChangeType"] = (int)AveChangeType.Delete;
            //    }
            //    else if (currentChangeType == SPChangeType.Add)
            //    {
            //        listObj["ChangeType"] = (int)AveChangeType.Add;
            //    }
            //    else
            //    {
            //        listObj["ChangeType"] = (int)AveChangeType.Edit;
            //    }
            //}
            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.None;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        /// <summary>
        /// 改变Object的某些change type，比如RoleAssignmentsChangeType，PermissionLevelChangeType。当前方法只支持|操作，如需支持其他操作，可扩展。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="key"></param>
        /// <param name="changeType"></param>
        private void ChangeObjectChangeType(Dictionary<string, object> collection, string key, AveChangeType changeType)
        {
            object value;
            if (!collection.TryGetValue(key, out value))
            {
                value = AveChangeType.None;
            }
            collection[key] = changeType | (AveChangeType)value;
        }
        protected Dictionary<string, object> ConvertToChangeWeb(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedWebCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.Web;
            Guid tempWebId = new Guid(objectProperties["WebId"].ToString());
            Dictionary<string, object> webObj = null;
            Object tempObj = null;

            if (changedWebCache.TryGetValue(tempWebId, out tempObj))
            {
                webObj = (Dictionary<string, object>)tempObj;
                preChangeType = (AveChangeType)webObj["ChangeType"];

                switch (currentChangeType)
                {
                    case SPChangeType.Navigation:
                        webObj["NavigationChanged"] = true;
                        break;
                    case SPChangeType.AssignmentAdd:
                    case SPChangeType.ScopeAdd:
                        ChangeObjectChangeType(webObj, "RoleAssignmentsChangeType", AveChangeType.Add);
                        break;
                    case SPChangeType.AssignmentDelete:
                    case SPChangeType.ScopeDelete:
                        ChangeObjectChangeType(webObj, "RoleAssignmentsChangeType", AveChangeType.Delete);
                        break;
                    case SPChangeType.RoleAdd:
                        ChangeObjectChangeType(webObj, "PermissionLevelChangeType", AveChangeType.Add);
                        break;
                    case SPChangeType.RoleUpdate:
                        ChangeObjectChangeType(webObj, "PermissionLevelChangeType", AveChangeType.Edit);
                        break;
                    case SPChangeType.RoleDelete:
                        ChangeObjectChangeType(webObj, "PermissionLevelChangeType", AveChangeType.Delete);
                        break;
                    case SPChangeType.DeleteObject:
                        webObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Add:
                        webObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.Restore:
                        webObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    case SPChangeType.SystemUpdate:
                    case SPChangeType.Update:
                        if (preChangeType != AveChangeType.Add)
                        {
                            webObj["ChangeType"] = (int)AveChangeType.Edit;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                webObj = objectProperties;
                changedWebCache[tempWebId] = webObj;

                switch (currentChangeType)
                {
                    case SPChangeType.Navigation:
                        webObj["NavigationChanged"] = true;
                        break;
                    case SPChangeType.AssignmentAdd:
                    case SPChangeType.ScopeAdd:
                        webObj["RoleAssignmentsChangeType"] = AveChangeType.Add;
                        break;
                    case SPChangeType.AssignmentDelete:
                    case SPChangeType.ScopeDelete:
                        webObj["RoleAssignmentsChangeType"] = AveChangeType.Delete;
                        break;
                    case SPChangeType.RoleAdd:
                        webObj["PermissionLevelChangeType"] = AveChangeType.Add;
                        break;
                    case SPChangeType.RoleUpdate:
                        webObj["PermissionLevelChangeType"] = AveChangeType.Edit;
                        break;
                    case SPChangeType.RoleDelete:
                        webObj["PermissionLevelChangeType"] = AveChangeType.Delete;
                        break;
                    case SPChangeType.DeleteObject:
                        webObj["FullUrl"] = "";
                        webObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Add:
                        webObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.Restore:
                        webObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    case SPChangeType.SystemUpdate:
                    case SPChangeType.Update:
                        webObj["ChangeType"] = (int)AveChangeType.Edit;
                        break;
                    default:
                        break;
                }
            }

            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.None;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        protected Dictionary<string, object> ConvertToChangeField(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedWebCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.Web;
            Guid tempWebId = new Guid(objectProperties["WebId"].ToString());
            Dictionary<string, object> webObj = null;
            if (!changedWebCache.ContainsKey(tempWebId))
            {
                objectProperties["ChangeType"] = (int)AveChangeType.None;
                changedWebCache[tempWebId] = objectProperties;
                webObj = objectProperties;
            }
            else
            {
                webObj = changedWebCache[tempWebId] as Dictionary<string, object>;
            }

            webObj["ColumnChangeType"] = (int)ConvertChangeType(currentChangeType);

            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.None;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        protected Dictionary<string, object> ConvertToChangeContentType(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedWebCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.Web;
            Guid tempWebId = new Guid(objectProperties["WebId"].ToString());
            Dictionary<string, object> webObj = null;
            if (!changedWebCache.ContainsKey(tempWebId))
            {
                objectProperties["ChangeType"] = (int)AveChangeType.None;
                changedWebCache[tempWebId] = objectProperties;
                webObj = objectProperties;
            }
            else
            {
                webObj = changedWebCache[tempWebId] as Dictionary<string, object>;
            }

            webObj["ContentTypeChangeType"] = (int)ConvertChangeType(currentChangeType);

            tempProperties["SiteId"] = changeObject.SiteId;
            tempProperties["ChangeType"] = (int)AveChangeType.None;
            tempProperties["Time"] = changeObject.Time;
            return tempProperties;
        }
        protected Dictionary<string, object> ConvertToChangeUser(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedSiteCache)
        {
            object tempObj = null;
            Dictionary<string, object> siteObj = null;
            objectProperties["ChangeObjectType"] = ChangeObjectType.Site;

            if (changedSiteCache.TryGetValue(changeObject.SiteId, out tempObj))
            {
                siteObj = (Dictionary<string, object>)tempObj;
            }
            else
            {
                objectProperties["ChangeType"] = (int)AveChangeType.None;
                siteObj = new Dictionary<string, object>(objectProperties);
                changedSiteCache[changeObject.SiteId] = siteObj;
            }

            siteObj["UserChangeType"] = (int)AveChangeType.Edit;

            return null;
        }
        protected Dictionary<string, object> ConvertToChangeGroup(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedSiteCache)
        {
            object tempObj = null;
            Dictionary<string, object> siteObj = null;
            objectProperties["ChangeObjectType"] = ChangeObjectType.Site;

            if (changedSiteCache.TryGetValue(changeObject.SiteId, out tempObj))
            {
                siteObj = (Dictionary<string, object>)tempObj;
            }
            else
            {
                objectProperties["ChangeType"] = (int)AveChangeType.None;
                siteObj = new Dictionary<string, object>(objectProperties);
                changedSiteCache[changeObject.SiteId] = siteObj;
            }

            siteObj["GroupChangeType"] = (int)AveChangeType.Edit;

            return null;
        }
        protected Dictionary<string, object> ConvertToChangeSite(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedSiteCache)
        {
            object tempObj = null;
            Dictionary<string, object> siteObj = null;

            objectProperties["ChangeObjectType"] = ChangeObjectType.Site;

            if (changedSiteCache.TryGetValue(changeObject.SiteId, out tempObj))
            {
                siteObj = (Dictionary<string, object>)tempObj;
                preChangeType = (AveChangeType)siteObj["ChangeType"];

                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        siteObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.DeleteObject:
                        siteObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Update:
                        if (preChangeType != AveChangeType.Add)
                        {
                            siteObj["ChangeType"] = (int)AveChangeType.Edit;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                objectProperties["ChangeType"] = (int)ConvertChangeType(currentChangeType);
                changedSiteCache[changeObject.SiteId] = objectProperties;
            }

            return null;
        }
        protected Dictionary<string, object> ConvertToChangeView(Change changeObject, SPChangeType currentChangeType, AveChangeType preChangeType, Dictionary<string, object> objectProperties, Dictionary<Guid, object> changedFileCache, Dictionary<Guid, object> changedListCache, Dictionary<Guid, object> changedWebCache, Dictionary<Guid, object> changedSiteCache)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>();
            objectProperties["ChangeObjectType"] = ChangeObjectType.View;
            objectProperties["ChangeType"] = (int)AveChangeType.None;
            Guid listId = new Guid(objectProperties["ListId"].ToString());

            Dictionary<string, object> listObj = null;
            Object tempObj = null;

            if (changedListCache.TryGetValue(listId, out tempObj))
            {
                listObj = (Dictionary<string, object>)tempObj;
            }
            else
            {
                listObj = new Dictionary<string, object>(objectProperties);
                changedListCache[listId] = listObj;

                Guid webId = new Guid(objectProperties["WebId"].ToString());

                if (!changedWebCache.ContainsKey(webId))
                {
                    changedWebCache[webId] = listObj;

                    if (!changedSiteCache.ContainsKey(changeObject.SiteId))
                    {
                        changedSiteCache[changeObject.SiteId] = listObj;
                    }
                }
            }

            Guid viewId = new Guid(objectProperties["ViewId"].ToString());
            Dictionary<string, object> viewObj = null;
            if (changedFileCache.TryGetValue(viewId, out tempObj))
            {
                viewObj = (Dictionary<string, object>)tempObj;
                preChangeType = (AveChangeType)viewObj["ChangeType"];
                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        viewObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.Update:
                    case SPChangeType.SystemUpdate:
                        if (preChangeType != AveChangeType.Add)
                        {
                            viewObj["ChangeType"] = (int)AveChangeType.Edit;
                        }
                        break;
                    case SPChangeType.DeleteObject:
                        viewObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Restore:
                        viewObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                viewObj = objectProperties;
                changedFileCache[viewId] = viewObj;
                switch (currentChangeType)
                {
                    case SPChangeType.Add:
                        viewObj["ChangeType"] = (int)AveChangeType.Add;
                        break;
                    case SPChangeType.Update:
                    case SPChangeType.SystemUpdate:
                        viewObj["ChangeType"] = (int)AveChangeType.Edit;
                        break;
                    case SPChangeType.DeleteObject:
                        viewObj["ChangeType"] = (int)AveChangeType.Delete;
                        break;
                    case SPChangeType.Restore:
                        viewObj["ChangeType"] = (int)AveChangeType.Restore;
                        break;
                    default:
                        break;
                }
            }

            if (viewObj != null && objectProperties.ContainsKey("Time") && viewObj.ContainsKey("Time")
               && (DateTime)objectProperties["Time"] > (DateTime)viewObj["Time"])
            {
                viewObj["Time"] = objectProperties["Time"];
            }
            return null;
        }

        protected void UpdateRoleAssignment(Web web, Dictionary<string, object> roleAssignmentProperties, int principalId, SecurableObject securableObject, Dictionary<string, object> newRoleAssignmentProperties)
        {
            RoleAssignment roleAssignment = securableObject.RoleAssignments.GetByPrincipalId(principalId);
            web.Context.Load(roleAssignment);
            web.Context.Load(roleAssignment, role => role.Member);
            web.Context.Load(roleAssignment.RoleDefinitionBindings);
            web.Context.ExecuteQuery();
            UpdateRoleDefinitionBindingCollection(web, roleAssignment.RoleDefinitionBindings, roleAssignmentProperties);
            roleAssignment.Update();
            web.Context.Load(roleAssignment);
            web.Context.Load(roleAssignment, role => role.Member, role => role.RoleDefinitionBindings);
            web.Context.ExecuteQuery();
            AssembleRoleAssignmetProperites(newRoleAssignmentProperties, roleAssignment);
        }
        protected void UpdateRoleDefinitionBindingCollection(Web web, RoleDefinitionBindingCollection roleDefinitionBindingCol, Dictionary<string, object> roleAssignmentProperties)
        {
            HashSet<string> containedRoleDefintionNameSet = new HashSet<string>();
            List<RoleDefinition> shouldDeletedRoleDefinitionList = new List<RoleDefinition>();
            List<string> shouldAddedRoleDefinitionNameList = new List<string>();
            List<string> shouldContainedRoleDefinitonNameSet = roleAssignmentProperties["RoleDefinitionBindingCollection"] as List<string>;
            foreach (RoleDefinition roleDefinition in roleDefinitionBindingCol)
            {
                if (shouldContainedRoleDefinitonNameSet.Contains(roleDefinition.Name))
                {
                    containedRoleDefintionNameSet.Add(roleDefinition.Name);
                }
                else
                {
                    shouldDeletedRoleDefinitionList.Add(roleDefinition);
                }
            }
            foreach (string roleDefinitionName in shouldContainedRoleDefinitonNameSet)
            {
                if (!containedRoleDefintionNameSet.Contains(roleDefinitionName))
                {
                    shouldAddedRoleDefinitionNameList.Add(roleDefinitionName);
                }
            }
            foreach (RoleDefinition roleDefinition in shouldDeletedRoleDefinitionList)
            {
                roleDefinitionBindingCol.Remove(roleDefinition);
            }
            foreach (string roleDefinitionName in shouldAddedRoleDefinitionNameList)
            {
                roleDefinitionBindingCol.Add(web.RoleDefinitions.GetByName(roleDefinitionName));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="etagStr"></param>
        /// <returns>string[0]: UniqueId; string[1]: DocLibRowId</returns>
        protected string[] GetIdsFromEtag(string etagStr)
        {
            string UniqueId = string.Empty;
            string DocLibRowId = string.Empty;
            int startIndex1 = etagStr.IndexOf("{", StringComparison.OrdinalIgnoreCase);
            int startIndex2 = etagStr.IndexOf(",", StringComparison.OrdinalIgnoreCase) + 1;
            if (startIndex1 >= 0)
            {
                int endIndex1 = etagStr.IndexOf('}', startIndex1);
                if (endIndex1 > startIndex1)
                {
                    UniqueId = etagStr.Substring(startIndex1, endIndex1 - startIndex1 + 1);
                }
            }
            if (startIndex2 >= 0)
            {
                int endIndex2 = etagStr.IndexOf('"', startIndex2);
                if (endIndex2 > startIndex2)
                {
                    DocLibRowId = etagStr.Substring(startIndex2, endIndex2 - startIndex2);
                }
            }
            return new string[] { UniqueId, DocLibRowId };
        }

        protected string LoadList(ClientContext context, List list)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(list);
                    context.Load(list, l => l.ValidationFormula,
                                              l => l.ValidationMessage,
                                              l => l.OnQuickLaunch,
                                              //l => l.SchemaXml,
                                              l => l.RootFolder,
                                              l => l.IsSiteAssetsLibrary,
                                              l => l.HasUniqueRoleAssignments,
                                              l => l.DataSource,
                                              l => l.Id,
                                              l => l.ItemCount,
                                              l => l.DefaultDisplayFormUrl,
                                              l => l.DefaultViewUrl);
                }
                using (scope.StartCatch())
                {
                    context.Load(list);
                    context.Load(list, l => l.ValidationFormula,
                                              l => l.ValidationMessage,
                                              l => l.OnQuickLaunch,
                                              //l => l.SchemaXml,
                                              //l => l.RootFolder,
                                              //l => l.IsSiteAssetsLibrary,
                                              l => l.HasUniqueRoleAssignments,
                                              l => l.DataSource,
                                              l => l.Id,
                                              l => l.ItemCount);
                }
            }
            context.ExecuteQuery();
            if (scope.HasException)
            {
                return scope.ErrorMessage;
            }
            return null;
        }

        public virtual void DisableListVersion(string webRelativeUrl, string listTitle, Guid listId, bool enableVersioning)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetById(listId);

                if (enableVersioning)
                {
                    list.EnableVersioning = false;
                    list.Update();
                    context.ExecuteQuery();
                }

            }
        }

        public virtual void RevertListVersion(string webRelativeUrl, string listTitle, Guid listId, bool enableVersioning)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetById(listId);
                if (enableVersioning)
                {
                    list.EnableVersioning = true;
                    list.Update();
                    context.ExecuteQuery();
                }
            }
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        private static string GetUrl(string url, string fileName, string listId, string itemId, string col, string order, string source, string op, string verId, string isDlg, string type)
        {
            string strUrl = string.Empty;

            switch (type)
            {
                case "get":
                    strUrl = url
                + "list=" + listId
                + "&ID=" + itemId
                + "&FileName=" + fileName
                + "&Source=" + source
                + "&IsDlg=" + isDlg;
                    break;
                case "post":
                    strUrl = url
                 + "FileName=" + fileName
                 + "&list=" + listId
                 + "&ID=" + itemId
                 + "&col=" + col
                 + "&order=" + order
                 + "&Source=" + source
                 + "&op=" + op
                 + "&ver=" + verId
                 + "&IsDlg=" + isDlg;
                    break;
                default:
                    break;
            }

            return strUrl;
        }


        public virtual void MoveTo(string parentWebUrl, string parentWebServerRelativeUrl, string folderServerRelativeUrl, string newUrl)
        {
            throw new NotSupportedException();
        }


        protected virtual Dictionary<string, object> GetFolder(AveClientContext context, string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            bool folderServerRelativeUrlVaild = true;
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            Folder folder = null;
            ListItem item = null;
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            folder = GetFolderByAPI(web, folderServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) ? folderServerRelativeUrl : "/" + folderServerRelativeUrl);
            context.Load(folder);
            context.Load(folder, f => f.ParentFolder);
            try
            {
                ListItemCollection listItems = null;
                if (listName != null)
                {
                    //List list = web.Lists.GetByTitle(listName);
                    List list = web.Lists.GetById(listId);
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = string.Format(
                        "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query></View>",
                        folderServerRelativeUrl);
                    listItems = list.GetItems(camlQuery);
                    LoadItemsProperty(context, listItems);
                }
                context.ExecuteQuery();
                folderProperties["Exists"] = true;
                if (listItems != null && listItems.Count == 1)
                {
                    item = listItems[0];
                }
                folderServerRelativeUrlVaild = true;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Folder:{0} not exists.Error Message:{1}", folderServerRelativeUrl, ex.ToString());
                folderProperties["Exists"] = false;
                folderServerRelativeUrlVaild = false;
            }

            if (item != null && item.IsPropertyAvailable("Id"))
            {
                Dictionary<string, object> itmProp = new Dictionary<string, object>();
                GetItemDic(itmProp, item);
                folderProperties["UniqueId"] = itmProp["UniqueId"];
                folderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itmProp;
            }
            if (folderServerRelativeUrlVaild == true)
            {
                AssembleFolderProperties(context as AveClientContext, webServerRelativeUrl, folder, folderServerRelativeUrl, folderProperties);
            }
            return folderProperties;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private Dictionary<string, object> CloneProperties(Dictionary<string, object> originalProperties)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            if (originalProperties != null)
            {
                foreach (KeyValuePair<string, object> kv in originalProperties)
                {
                    properties[kv.Key] = kv.Value;
                }
            }
            return properties;
        }

        protected virtual void AddViewFiles(ClientContext context, Folder folder, SortedDictionary<string, Dictionary<string, object>> sortFileCache)
        {
            context.Load(folder, f => f.Files);
            context.ExecuteQuery();
            foreach (ClientFile viewFile in folder.Files)
            {
                Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                AssembleViewFileProperties(itemProperty, viewFile);
                itemProperty["IsSystemFile"] = true;
                sortFileCache.Add(viewFile.Name, itemProperty);
            }
        }

        protected virtual void AddViewFolder(ClientContext context, Folder folder, List list,  List<Dictionary<string, object>> folders, string webServerRelativeUrl)
        {
            context.Load(folder.Folders);
            context.Load(folder.Folders, fs => fs.Include(f => f.ParentFolder.ServerRelativeUrl));
            context.ExecuteQuery();
            foreach (var tempFolder in folder.Folders)
            {
                //过滤掉Attachments和在Folders集合中存在的Folder.  在集合中存在说明不是System Folder
                if (tempFolder.Name.Equals("Attachments", StringComparison.OrdinalIgnoreCase)
                    || folders.Any(f => f.ContainsKey("ServerRelativeUrl") && tempFolder.ServerRelativeUrl.Equals(f["ServerRelativeUrl"].ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                Dictionary<string, object> itemPro = new Dictionary<string, object>();
                itemPro["Items"] = new List<Dictionary<string, object>>();
                itemPro["Folders"] = new List<Dictionary<string, object>>();
                itemPro["Attachments"] = new List<Dictionary<string, object>>();
                itemPro["Versions"] = new List<Dictionary<string, object>>();
                AssembleSystemFolderProperties(context as AveClientContext, itemPro, tempFolder, webServerRelativeUrl);
                folders.Add(itemPro);
            }
        }

        protected virtual void AddViewItems(ClientContext context, List list, Folder folder, string folderServerRelativeUrl, List<Dictionary<string, object>> items, List<Dictionary<string, object>> folders, string webServerRelativeUrl)
        {
            if (!WrapperConfiguration.BPOS_S.IncludeListView)
            {
                return;
            }
            bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            //目前发现List下有Forms和Item两种System folder
            bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
            bool isItemFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Item", StringComparison.OrdinalIgnoreCase);
            if (!isRootFolder && !isForms && !isItemFolder)
            {
                return;
            }
            var sortFileCache = new SortedDictionary<string, Dictionary<string, object>>();
            if (((list.BaseType.Equals(BaseType.GenericList) || list.BaseType.Equals(BaseType.Issue)) && isRootFolder) || isForms || isItemFolder)
            {
                AddViewFiles(context, folder, sortFileCache);
                AddViewFolder(context, folder, list, folders, webServerRelativeUrl);
            }
            else if (isRootFolder)
            {
                AddViewFolder(context, folder, list, folders, webServerRelativeUrl);
            }
            if (sortFileCache.Count > 0)
            {
                items.InsertRange(0, sortFileCache.Values.ToList());
            }
        }

        /// <summary>
        /// 得到list下系统的view item;
        /// 可以得到指定的forms folder object；
        /// 可以得到指定的list下rootfolder的系统file；
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listRootFolderUrl"></param>
        /// <param name="isGenericList"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected Dictionary<string, object> GetViewItem(Web web, string listRootFolderUrl, bool isGenericList, string dirName, string leafName)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemProperty = null;
                bool isRootFolder = ("/" + dirName.TrimStart('/')).Equals(listRootFolderUrl, StringComparison.OrdinalIgnoreCase);
                bool isForms = ("/" + dirName.TrimStart('/')).Equals(listRootFolderUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
                if (!isRootFolder && !isForms)
                {
                    return null;
                }
                Folder folder = GetFolderByAPI(web, "/" + dirName.TrimStart('/'));
                context.Load(folder);
                if (isRootFolder && !isGenericList)
                {
                    context.Load(folder.Folders);
                    context.ExecuteQuery();
                    foreach (var tempFolder in folder.Folders)
                    {
                        if (tempFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase) &&
                            leafName.Equals("Forms", StringComparison.OrdinalIgnoreCase))
                        {
                            Dictionary<string, object> property = new Dictionary<string, object>();
                            AssembleSystemFolderProperties(context as AveClientContext, property, tempFolder, web.ServerRelativeUrl);
                            itemProperty = property;
                            break;
                        }
                    }
                }
                else
                {
                    context.Load(folder.Files);
                    context.ExecuteQuery();
                    foreach (ClientFile viewFile in folder.Files)
                    {
                        if (viewFile.Name.Equals(leafName, StringComparison.OrdinalIgnoreCase))
                        {
                            Dictionary<string, object> property = new Dictionary<string, object>();
                            AssembleViewFileProperties(property, viewFile);
                            itemProperty = property;
                            break;
                        }
                    }
                }
                return itemProperty;
            }
        }

        private bool IsConnectonForciblyClosedExceptioin(Exception te)
        {
            if (te.InnerException is SocketException || te.InnerException is IOException)
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }

        private bool IsTimeOutServerException(Exception se)
        {
            //ADO-60291:Add and load web time out, reload the new created web.
            if ((se is ServerException && (se as ServerException).ServerErrorCode == AveStandardErrorCode.COR_E_TIMEOUT))
            {
                return true;
            }
            else if (se.InnerException != null)
            {
                return IsTimeOutServerException(se);
            }
            return false;
        }
        #endregion

        #region Discovery Query
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ic is a variable")]
        public virtual bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                        "<View Scope=\"RecursiveAll\"><Query><Where><And><Eq><FieldRef Name=\"FileLeafRef\"/><Value Type=\"Lookup\">{0}</Value></Eq><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">/{1}</Value></Eq></And></Where></Query></View>",
                        leafName, dirName);
                ListItemCollection itemColl = list.GetItems(camlQuery);
                context.Load(itemColl, ic => ic.Include(i => i.Id));
                context.ExecuteQuery();
                if (itemColl.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public virtual Dictionary<string, object> QueryRootWeb(Guid siteId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.RootWeb;
                context.Load(web, w => w.Id, w => w.Title, w => w.ServerRelativeUrl);
                context.ExecuteQuery();
                string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                Dictionary<string, object> webDictionary = new Dictionary<string, object>();
                AssembleDiscoverWebProperties(webDictionary, web, siteServerRelativeUrl);
                return webDictionary;
            }
        }

        public virtual Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<Guid, object> webProperties = new Dictionary<Guid, object>();
                Web web = context.Site.OpenWebById(parentWebId);
                WebCollection webs = web.GetSubwebsForCurrentUser(null);
                context.Load(webs, collection => collection.Include(w => w.Id, w => w.Title, w => w.ServerRelativeUrl));
                context.Load(context.Site, site => site.ServerRelativeUrl);
                context.ExecuteQuery();
                foreach (Web subWeb in webs)
                {
                    Dictionary<string, object> subWebProperty = new Dictionary<string, object>();
                    AssembleDiscoverWebProperties(subWebProperty, subWeb, context.Site.ServerRelativeUrl);
                    webProperties.Add(subWeb.Id, subWebProperty);
                }
                return webProperties;
            }
        }

        public virtual Dictionary<Guid, Dictionary<string, object>> GetSubWebsBasicInfo(string siteUrl, Guid parentWebId)
        {
            using (AveClientContext context = CreateContext(siteUrl))
            {
                var webProperties = new Dictionary<Guid, Dictionary<string, object>>();
                Web web = context.Site.OpenWebById(parentWebId);
                WebCollection webs = web.GetSubwebsForCurrentUser(null);
                context.Load(webs, collection => collection.Include(w => w.Id, w => w.Title, w => w.ServerRelativeUrl));
                context.Load(context.Site, site => site.ServerRelativeUrl);
                context.ExecuteQuery();

                foreach (Web subWeb in webs)
                {
                    webProperties.Add(subWeb.Id,
                        new Dictionary<string, object> { { "Id", subWeb.Id }, { "Title", subWeb.Title }, { "ServerRelativeUrl", subWeb.ServerRelativeUrl }, });
                }

                return webProperties;
            }
        }

        protected virtual void GetSubWebs(Web web, string siteServerRelativeUrl, Dictionary<string, object> webDic)
        {
            AssembleDiscoverWebProperties(webDic, web, siteServerRelativeUrl);
            WebCollection webs = web.GetSubwebsForCurrentUser(null);
            web.Context.Load(webs, collection => collection.Include(w => w.Id, w => w.Title, w => w.ServerRelativeUrl));
            web.Context.ExecuteQuery();
            foreach (Web subWeb in webs)
            {
                Dictionary<string, object> subWebDic = new Dictionary<string, object>();
                GetSubWebs(subWeb, siteServerRelativeUrl, subWebDic);
                ((Dictionary<Guid, object>)webDic["SubWebs"]).Add((Guid)subWebDic["WebID"], subWebDic);
            }
        }
        public virtual Dictionary<string, object> DiscoverAllListContent(Guid siteId, Guid webId, Guid listId, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder)
        {
            throw new NotImplementedException();
        }
        ///<summary>
        /// 将list下需要备份的item/folder填充并缓存，使备份时无需再次进行GetItem操作。Note: 缓存只对当前request有效
        /// </summary>
        public virtual Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool isDiscover, bool includeSystemFolder = false)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> parentFolder = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                parentFolder["Items"] = new List<Dictionary<string, object>>();
                parentFolder["Folders"] = new List<Dictionary<string, object>>();
                //parentFolder["Attachments"] = new List<Dictionary<string, object>>();
                //parentFolder["Versions"] = new List<Dictionary<string, object>>();
                if (listId != Guid.Empty) // for system folder, we skip it now, to do it later
                {
                    List list = null;
                    string folderServerRelativeUrl = "/" + folderUrl.TrimStart('/');
                    Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    context.Load(folder, f => f.ItemCount);
                    list = web.Lists.GetById(listId);
                    //需要优化，只需要获取要用的属性
                    context.Load(list,
                        l => l.BaseType, l => l.EnableVersioning, l => l.EnableMinorVersions, l => l.EnableAttachments,
                        l => l.EnableFolderCreation, l => l.EnableModeration, l => l.BaseTemplate,
                        l => l.Id, l => l.Title, l => l.Created, l => l.ItemCount);
                    context.Load(list.RootFolder, r => r.ServerRelativeUrl, r => r.ItemCount);
                    context.Load(web, tempWeb => tempWeb.ServerRelativeUrl);
                    try
                    {
                        context.ExecuteQuery();
                    }
                    catch (ServerUnauthorizedAccessException ex)
                    {
                        mLogger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Office365, new AvePoint.GCommon.Utility.I18N.EventIds.Communication.ReceiveDataFailedEventMessage(ex));
                    }
                    GetItemsFromFolder(context, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl, parentFolder, isDiscover);
                }
                else
                {
                    context.Load(web, tempWeb => tempWeb.ServerRelativeUrl);
                    context.ExecuteQuery();
                    List<Dictionary<string, object>> webItems = parentFolder["Items"] as List<Dictionary<string, object>>;
                    List<Dictionary<string, object>> webFolders = parentFolder["Folders"] as List<Dictionary<string, object>>;
                    Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/");
                    Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/");
                    foreach (var item in files[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>)
                    {
                        webItems.Add(item);
                    }
                    foreach (var folder in folders[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>)
                    {
                        webFolders.Add(folder);
                    }
                }
                parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                return parentFolder;
            }
        }

        public virtual Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changeItemsCache)
        {
            Dictionary<string, object> changedItems = new Dictionary<string, object>();
            //changedItems["ChangeFile"] = new List<Dictionary<string, object>>();
            changedItems["Items"] = new List<Dictionary<string, object>>();
            changedItems["Folders"] = new List<Dictionary<string, object>>();
            GetChangeItemsFromChangeCache(changedItems, webId, listId, folderUrl, changeItemsCache);
            return changedItems;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected void GetChangeItemsFromChangeCache(Dictionary<string, object> changedItems, Guid webId, Guid listId, string folderUrl, Dictionary<string, object> changedItemsCache)
        {
            using (AveClientContext context = CreateContext())
            {
                //List<Dictionary<string, object>> changeFiles = changedItems["ChangeFile"] as List<Dictionary<string, object>>;
                List<Dictionary<string, object>> changeItems = changedItems["Items"] as List<Dictionary<string, object>>;
                List<Dictionary<string, object>> changeFolders = changedItems["Folders"] as List<Dictionary<string, object>>;
                Dictionary<string, object> tempFolders = new Dictionary<string, object>();
                List<string> failedParentFolders = new List<string>();
                Site site = context.Site;
                Web web = site.OpenWebById(webId);
                //存储List下的ListItem，解决一个一个地查找效率低下的问题
                Dictionary<int, ListItem> itemsCacheInfo = LoadItemsUnderList(context, web, listId, changedItemsCache);
                foreach (string key in changedItemsCache.Keys)
                {
                    switch (key)
                    {
                        case "ChangedFolderCache":
                        case "ChangedFileCache":
                            bool isFile = key.Equals("ChangedFileCache") ? true : false;
                            if (!isFile)
                            {
                                continue;//need to impletement later
                            }
                            Dictionary<Guid, object> changeObjects = changedItemsCache[key] as Dictionary<Guid, object>;
                            foreach (KeyValuePair<Guid, object> changeObject in changeObjects)
                            {
                                Dictionary<string, object> changedProperties = changeObject.Value as Dictionary<string, object>;
                                Guid parentWebId = new Guid(changedProperties["WebId"].ToString());
                                if (!parentWebId.Equals(webId))
                                {
                                    continue;
                                }
                                List list = web.Lists.GetById(listId);
                                if (!changedProperties.ContainsKey("ViewId"))
                                {
                                    continue;
                                }
                                Guid viewGuid = (Guid)changedProperties["ViewId"];
                                ClientObject item = null;
                                Dictionary<string, object> properties = new Dictionary<string, object>();
                                context.Load(list, l => l.BaseTemplate, l => l.BaseType);
                                item = GetFileByViewGuid(context, list, viewGuid);
                                if (item == null)
                                {
                                    //we can not get the change item,skip it now,need to handle this later
                                    mLogger.Debug("Can't find file with view Guid,it will be skipped now.View Id:{0},\tWebId:{1}.", viewGuid, parentWebId);
                                    continue;
                                }
                                AssembleFileProperties(properties, item as ClientFile, list.ParentWebUrl, null);
                                if (ItemHasVersion(list, properties))
                                {
                                    properties["Versions"] = new List<Dictionary<string, object>>();
                                }
                                properties["FullUrl"] = (item as ClientFile).ServerRelativeUrl;
                                properties["ObjType"] = ItemType.Document;
                                properties["ChangeType"] = changedProperties["ChangeType"];
                                properties["ChangeTime"] = changedProperties["Time"];

                                if (changedProperties.ContainsKey("RoleAssignmentsChangeType"))
                                {
                                    properties["RoleAssignmentsChangeType"] = changedProperties["RoleAssignmentsChangeType"];
                                }

                                if (isFile)
                                {
                                    changeItems.Add(properties);
                                }
                            }
                            break;
                        case "ChangedItemCache":
                            Dictionary<string, object> itemsInCache = changedItemsCache[key] as Dictionary<string, object>;
                            foreach (KeyValuePair<string, object> tempItem in itemsInCache)
                            {
                                Dictionary<string, object> itemChangeProperties = tempItem.Value as Dictionary<string, object>;
                                int itemId = (int)itemChangeProperties["ItemId"];
                                AveChangeType changeType = (AveChangeType)itemChangeProperties["ChangeType"];
                                if (!tempItem.Key.Equals(listId + ";" + itemId.ToString()))
                                {
                                    continue;
                                }
                                Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                                if (changeType == AveChangeType.Delete)
                                {// because bpos cannot check type on delete object, "folder, item or document" will all consider as item here
                                    itemProperties["LeafName"] = itemId + "_.000";
                                    itemProperties["DoclibRowId"] = itemId;
                                    itemProperties["ObjType"] = ItemType.Item;
                                    itemProperties["Id"] = itemId;
                                    itemProperties["ServerRelativeUrl"] = folderUrl.TrimEnd('/') + "/" + itemId + "_.000";
                                    itemProperties["FullUrl"] = itemProperties["ServerRelativeUrl"];
                                    itemProperties["ChangeType"] = itemChangeProperties["ChangeType"];
                                    changeItems.Add(itemProperties);
                                    continue;
                                }
                                if (itemChangeProperties.ContainsKey("IsRenamed") && itemChangeProperties["IsRenamed"].ToString().Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
                                {
                                    itemProperties["IsRenamed"] = true;
                                    #region  need consider whether to delete this codes as it is not used from now on
                                    Dictionary<string, object> renameProperties = new Dictionary<string, object>();
                                    renameProperties["LeafName"] = itemId + "_.000";
                                    renameProperties["DoclibRowId"] = itemId;
                                    renameProperties["ObjType"] = ItemType.Item;
                                    renameProperties["Id"] = itemId;
                                    renameProperties["ServerRelativeUrl"] = folderUrl.TrimEnd('/') + "/" + itemId + "_.000";
                                    renameProperties["FullUrl"] = renameProperties["ServerRelativeUrl"];
                                    renameProperties["ChangeType"] = (int)AveChangeType.Delete;
                                    changeItems.Add(renameProperties);
                                    #endregion
                                }
                                List list = web.Lists.GetById(listId);
                                //ListItem item = list.GetItemById(itemId);
                                //context.Load(item);
                                //context.Load(item, i => i.HasUniqueRoleAssignments, i => i.DisplayName);
                                context.Load(list, l => l.BaseTemplate, l => l.BaseType);
                                context.Load(list.RootFolder, folder => folder.ServerRelativeUrl);
                                context.ExecuteQuery();
                                ListItem item = itemsCacheInfo.ContainsKey(itemId) ? itemsCacheInfo[itemId] : null;
                                if (item == null)
                                {
                                    continue;
                                }
                                GetItemDic(itemProperties, item);
                                if (ItemHasVersion(list, itemProperties))
                                {
                                    itemProperties["Versions"] = new List<Dictionary<string, object>>();
                                }
                                itemProperties["FullUrl"] = itemProperties["ServerRelativeUrl"];
                                itemProperties["ChangeType"] = itemChangeProperties["ChangeType"];
                                itemProperties["ChangeTime"] = itemChangeProperties["Time"];
                                itemProperties["LeafName"] = itemProperties.ContainsKey("Name") ? itemProperties["Name"] : item.DisplayName;
                                itemProperties["HasStream"] = false;
                                if (itemChangeProperties.ContainsKey("RoleAssignmentsChangeType"))
                                {
                                    itemProperties["RoleAssignmentsChangeType"] = itemChangeProperties["RoleAssignmentsChangeType"];
                                }
                                string fullUrl = itemProperties["FullUrl"].ToString();
                                string parentFolderUrl = "/" + fullUrl.Substring(0, fullUrl.LastIndexOf('/')).Trim('/');
                                if (!parentFolderUrl.Trim('/').Equals(folderUrl.Trim('/')))
                                {
                                    mLogger.Debug("The item is not in the current parent folder.ItemUrl:{0}\t\rParentFolderUrl:{1}", fullUrl, folderUrl);
                                    if (!parentFolderUrl.Trim('/').StartsWith(folderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }
                                    //如果变化的Item位于同一层，那么他们共用父结点。当同一层变化Item数量足够大，那么，他们的父结点会被无限重复查找，导致效率低下
                                    if (failedParentFolders.Contains(parentFolderUrl))
                                    {
                                        continue;
                                    }
                                    if (!tempFolders.ContainsKey(parentFolderUrl))
                                    {
                                        Folder parentFolder = GetFolderByAPI(web, parentFolderUrl);
                                        try
                                        {
                                            AddParentFolderToCache(context, list, parentFolder, tempFolders, changeFolders);
                                        }
                                        catch (Exception ex)
                                        {
                                            mLogger.Warn("AddParentFolderToCache failed.folder:{0},error:{1}", parentFolderUrl, ex);
                                            failedParentFolders.Add(parentFolderUrl);
                                            continue;
                                        }
                                    }
                                }
                                itemProperties["Attachments"] = new List<Dictionary<string, object>>();
                                GetAttachmentsFromItem(context, list, itemProperties, list.RootFolder.ServerRelativeUrl, null);
                                Guid uniqueId = new Guid(itemProperties["UniqueId"].ToString());
                                if (item.FileSystemObjectType == FileSystemObjectType.Folder)
                                {
                                    itemProperties["ObjType"] = ItemType.Folder;
                                    tempFolders[fullUrl] = itemProperties;
                                    changeFolders.Add(itemProperties);
                                }
                                else
                                {

                                    if (itemProperties.ContainsKey("Length") && Convert.ToInt32(itemProperties["Length"]) > 0)
                                    {
                                        itemProperties["ObjType"] = ItemType.Document;
                                        itemProperties["HasStream"] = true;
                                        itemProperties["Size"] = itemProperties["Length"] = Convert.ToInt32(itemProperties["Length"]);
                                        changeItems.Add(itemProperties);
                                    }
                                    else
                                    {
                                        itemProperties["ObjType"] = ItemType.Item;
                                        changeItems.Add(itemProperties);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                tempFolders.Clear();
            }
        }

        private ClientFile GetFileByViewGuid(AveClientContext context, List list, Guid viewGuid)
        {
            try
            {
                //need to be optimised, to reduce the request count.
                context.Load(list, tempList => tempList.ParentWebUrl);
                View view = list.GetView(viewGuid);
                context.Load(view, tempView => tempView.ServerRelativeUrl);
                context.ExecuteQuery();
                ClientFile file = GetFileByAPI(list.ParentWeb, view.ServerRelativeUrl);
                context.Load(file);
                context.ExecuteQuery();
                return file;
            }
            catch (Exception ex)
            {
                mLogger.Debug("Cannot Get file by viewGuid:{0},error:{1}", viewGuid, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取List下的ListItem，可以不用一个一个地查找
        /// </summary>
        /// <param name="context"></param>
        /// <param name="web"></param>
        /// <param name="listId"></param>
        /// <param name="changeCache"></param>
        /// <returns></returns>
        protected Dictionary<int, ListItem> LoadItemsUnderList(AveClientContext context, Web web, Guid listId, Dictionary<string, object> changeCache)
        {
            Dictionary<int, ListItem> itemsCache = new Dictionary<int, ListItem>();
            //存储变化ListItem DocLibRowId的集合
            List<int> docLibRowIdCollection = new List<int>();
            //使用CamlQuery查询，通过In查询元素可以同时查询多个ListItem，如果每次查询数量多于500时，会抛出异常，使用<RowLimit></RowLimit>也无法解决
            int discoverCount = 500;
            if (changeCache != null && changeCache.ContainsKey("ChangedItemCache"))
            {
                Dictionary<string, object> itemsInCache = changeCache["ChangedItemCache"] as Dictionary<string, object>;
                foreach (KeyValuePair<string, object> tempItem in itemsInCache)
                {
                    Dictionary<string, object> itemChangeProperties = tempItem.Value as Dictionary<string, object>;
                    Guid itemListId = new Guid(itemChangeProperties["ListId"].ToString());
                    if (itemListId == listId)
                    {
                        int itemId = (int)itemChangeProperties["ItemId"];
                        docLibRowIdCollection.Add(itemId);
                    }
                }
            }
            if (docLibRowIdCollection.Count > 0)
            {
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.ItemCount);
                Site site = context.Site;
                context.Load(site, s => s.MaxItemsPerThrottledOperation);
                context.ExecuteQuery();
                // 当一个List下变化的Item数量过大时，将List下所有存在的Item都查询出来，效率会变得更高
                // ADO-159259 当item总数超过list设定阈值的时候（5000），一次query数量过多会导致api失败。
                if (docLibRowIdCollection.Count > 500 || (list.ItemCount >= site.MaxItemsPerThrottledOperation && docLibRowIdCollection.Count > 50))
                {
                    #region Load all items under list for better performance
                    CamlQuery query = CamlQuery.CreateAllItemsQuery((int)site.MaxItemsPerThrottledOperation);
                    ListItemCollection items = null;
                    ListItemCollectionPosition pos = null;
                    do
                    {
                        query.ListItemCollectionPosition = pos;
                        items = list.GetItems(query);
                        context.Load(items, tempItems => tempItems.IncludeWithDefaultProperties(temp => temp.HasUniqueRoleAssignments, temp => temp.DisplayName), tempItems => tempItems.ListItemCollectionPosition);
                        context.ExecuteQuery();
                        foreach (ListItem item in items)
                        {
                            if (!itemsCache.ContainsKey(item.Id))
                            {
                                itemsCache.Add(item.Id, item);
                            }
                        }
                        pos = items.ListItemCollectionPosition;
                    }
                    while (pos != null);
                    #endregion
                }
                else
                {
                    #region Get changed items under list according to item id
                    while (docLibRowIdCollection.Count > 0)
                    {
                        CamlQuery camlQuery = CamlQuery.CreateAllItemsQuery();
                        StringBuilder queryXml = new StringBuilder();
                        queryXml.Append("<View Scope='RecursiveAll'><Query><Where><In><FieldRef Name='ID'/><Values>");
                        int count = docLibRowIdCollection.Count >= discoverCount ? discoverCount : docLibRowIdCollection.Count;
                        for (int i = 0; i < count; i++)
                        {
                            queryXml.Append(string.Format("<Value Type=\"Integer\">{0}</Value>", docLibRowIdCollection[0]));
                            docLibRowIdCollection.RemoveAt(0);
                        }
                        queryXml.Append("</Values></In></Where></Query></View>");
                        camlQuery.ViewXml = queryXml.ToString();
                        ListItemCollection itemCollection = list.GetItems(camlQuery);
                        context.Load(itemCollection, tempItemCollection => tempItemCollection.IncludeWithDefaultProperties(temp => temp.HasUniqueRoleAssignments, temp => temp.DisplayName));
                        context.ExecuteQuery();
                        foreach (ListItem item in itemCollection)
                        {
                            if (!itemsCache.ContainsKey(item.Id))
                            {
                                itemsCache.Add(item.Id, item);
                            }
                        }
                    }
                    #endregion
                }
            }
            return itemsCache;
        }
        protected void GetItemsFromFolder(ClientContext context, List list, Folder folder, string webServerRelativeUrl, string folderServerRelativeUrl, Dictionary<string, object> parentFolder, bool isDiscover)
        {
            List<Dictionary<string, object>> items = parentFolder["Items"] as List<Dictionary<string, object>>;
            List<Dictionary<string, object>> folders = parentFolder["Folders"] as List<Dictionary<string, object>>;
            //Query Item
            string rootFolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            int totalItemCount = folder.ItemCount;//list.RootFolder.ItemCount;
            if (totalItemCount > 0)
            {
                List<Dictionary<string, object>> listItems = isDiscover ? GetItemsByCamlForDiscover(context, list, webServerRelativeUrl, folderServerRelativeUrl)
                    : GetItemsByCaml(context, list, webServerRelativeUrl, folderServerRelativeUrl);
                foreach (Dictionary<string, object> item in listItems)
                {
                    if (ItemHasVersion(list, item))
                    {
                        item["Versions"] = new List<Dictionary<string, object>>();
                    }
                    item["Attachments"] = new List<Dictionary<string, object>>();
                    item["RbsId"] = null;
                    if (list.BaseType != BaseType.DocumentLibrary)
                    {
                        GetAttachmentsFromItem(context, list, item, rootFolderServerRelativeUrl, webServerRelativeUrl);
                    }
                    items.Add(item);
                }

                if (totalItemCount - listItems.Count > 0)
                {
                    //Query Folder
                    listItems = isDiscover ? GetFoldersByCamlForDiscover(context, list, webServerRelativeUrl, folderServerRelativeUrl)
                        : GetFoldersByCaml(context, list, webServerRelativeUrl, folderServerRelativeUrl);
                    foreach (Dictionary<string, object> item in listItems)
                    {
                        item["Items"] = new List<Dictionary<string, object>>();
                        item["Folders"] = new List<Dictionary<string, object>>();
                        item["Attachments"] = new List<Dictionary<string, object>>();
                        if (ItemHasVersion(list, item))
                        {
                            item["Versions"] = new List<Dictionary<string, object>>();
                        }
                        item["ItemId"] = item["Id"];
                        item["Hidden"] = (item["Id"] == null) ? true : false;
                        GetAttachmentsFromItem(context, list, item, rootFolderServerRelativeUrl, webServerRelativeUrl);
                        folders.Add(item);
                    }
                }
            }
            //Add to Query View Item by Client API
            AddViewItems(context, list, folder, folderServerRelativeUrl, items, folders, webServerRelativeUrl);
        }

        protected bool ItemHasVersion(List list, Dictionary<string, object> item)
        {
            //0x70 means user info list
            return list.BaseTemplate != 0x70 && (list.BaseType == BaseType.DocumentLibrary || Convert.ToInt32(item["UIVersion"]) > 512);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected List<Dictionary<string, object>> GetItemsByCaml(ClientContext context, List list, string webServerRelativeUrl, string folderUrl)
        {
            ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            bool isFile = list.BaseType == BaseType.DocumentLibrary;
            var filesMap = new Dictionary<string, ClientFile>();
            if (isFile)
            {
                try
                {
                    FileCollection files = GetFolderByAPI(list.ParentWeb, folderUrl).Files;
                    using (exceptionScope.StartScope())
                    {
                        using (exceptionScope.StartTry())
                        {
                            context.Load(files, fs => fs.IncludeWithDefaultProperties(f => f.CheckedOutByUser));
                        }
                        using (exceptionScope.StartCatch())
                        {
                            context.Load(files);
                        }
                    }
                    context.ExecuteQuery();
                    foreach (ClientFile file in files)
                    {
                        filesMap[file.ServerRelativeUrl] = file;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("An error occurred while get checkout user for all of files, folderUrl: {0}, error: {1}", folderUrl, e);
                }
            }
            if (!IsThrottled(list.ItemCount))
            {
                ListItemCollectionPosition pos = null;
                do
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = AveCamlQueryString.GetAllItemsString(null, (int)this.MaxItemsPerThrottledOperation, QueryFindOption.None);
                    SetCamlQueryFolderUrl(camlQuery, folderUrl);
                    camlQuery.ListItemCollectionPosition = pos;
                    var listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                            items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                        item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        var itemProperty = AssmeblyItemInfo(context, webServerRelativeUrl, exceptionScope.HasException, isFile, filesMap, item);
                        results.Add(itemProperty);
                    }
                    pos = listItems.ListItemCollectionPosition;
                }
                while (pos != null);
            }
            else
            {
                QueryItemsForLargeList(context, list, webServerRelativeUrl, folderUrl, exceptionScope, filesMap, results, null);
            }
            EnsureParentThreadId(list, results);
            return results;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected List<Dictionary<string, object>> GetItemsByCamlForDiscover(ClientContext context, List list, string webServerRelativeUrl, string folderUrl)
        {
            ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            bool isFile = list.BaseType == BaseType.DocumentLibrary;
            var filesMap = new Dictionary<string, ClientFile>();
            if (isFile)
            {
                try
                {
                    FileCollection files = GetFolderByAPI(list.ParentWeb, folderUrl).Files;
                    using (exceptionScope.StartScope())
                    {
                        using (exceptionScope.StartTry())
                        {
                            context.Load(files, fs => fs.IncludeWithDefaultProperties(f => f.CheckedOutByUser));
                        }
                        using (exceptionScope.StartCatch())
                        {
                            context.Load(files);
                        }
                    }
                    context.ExecuteQuery();
                    foreach (ClientFile file in files)
                    {
                        filesMap[file.ServerRelativeUrl] = file;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("An error occurred while get checkout user for all of files, folderUrl: {0}, error: {1}", folderUrl, e);
                }
            }
            List<string> viewFields = new List<string> { "Modified_x0020_By", "Created_x0020_By", "FileDirRef", "FileLeafRef", "Title", "GUID", "_UIVersion" };
            if (list.BaseType != BaseType.DocumentLibrary)
            {
                viewFields.Add("Attachments");
            }
            if (list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
            {
                viewFields.Add("ThreadIndex");
                viewFields.Add("ParentFolderId");
            }
            if (!IsThrottled(list.ItemCount))
            {
                ListItemCollectionPosition pos = null;
                do
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = AveCamlQueryString.GetAllItemsString(viewFields, (int)this.MaxItemsPerThrottledOperation, QueryFindOption.None);
                    SetCamlQueryFolderUrl(camlQuery, folderUrl);
                    camlQuery.ListItemCollectionPosition = pos;
                    var listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                            items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                        item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        var itemProperty = AssmeblyItemInfoWithoutAddToCache(context, webServerRelativeUrl, exceptionScope.HasException, isFile, filesMap, item);
                        results.Add(itemProperty);
                    }
                    pos = listItems.ListItemCollectionPosition;
                }
                while (pos != null);
            }
            else
            {
                QueryItemsForLargeList(context, list, webServerRelativeUrl, folderUrl, exceptionScope, filesMap, results, viewFields);
            }
            EnsureParentThreadId(list, results);
            return results;
        }
        protected virtual void QueryFoldersForLargeList(ClientContext context, List list, string folderUrl, List<Dictionary<string, object>> results, List<string> viewFields = null)
        {
            throw new InvalidOperationException("Unreachable code.");
        }
        internal virtual ListItemCollectionPosition QueryItemsByQueryStringForLargeList(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, ExceptionHandlingScope exceptionScope, Dictionary<string, ClientFile> filesMap, List<Dictionary<string, object>> results, CamlQuery query)
        {
            throw new InvalidOperationException("Unreachable code.");
        }
        protected virtual void QueryItemsForLargeList(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, ExceptionHandlingScope exceptionScope, Dictionary<string, ClientFile> filesMap, List<Dictionary<string, object>> results, List<string> viewFields = null)
        {
            throw new InvalidOperationException("Unreachable code.");
        }

        protected virtual bool IsThrottled(int itemCount)
        {
            return false;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected Dictionary<string, object> AssmeblyItemInfo(ClientContext context, string webServerRelativeUrl, bool hasException, bool isFile, Dictionary<string, ClientFile> filesMap, ListItem item)
        {
            if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor")) //for community site discussion list
            {
                context.Load(item);
                context.ExecuteQuery();
            }
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);
            itemProperty["ObjType"] = ItemType.Item;
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
            if (isFile)
            {
                if (item.FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(item["FileRef"] as string) && filesMap.ContainsKey(item["FileRef"] as string))
                {
                    string fileRelativeUrl = item["FileRef"] as string;
                    ClientFile file = filesMap[fileRelativeUrl];
                    itemProperty["ServerRelativeUrl"] = fileRelativeUrl;
                    itemProperty["CheckoutUserId"] = null;
                    if (!hasException && file.CheckedOutByUser.IsPropertyAvailable("Id"))
                    {
                        itemProperty["CheckoutUserId"] = (int?)file.CheckedOutByUser.Id;
                    }
                    Dictionary<string, object> fileProperty = new Dictionary<string, object>();
                    AssembleFileProperties(fileProperty, file, webServerRelativeUrl, item);
                }
                itemProperty["ObjType"] = ItemType.Document;
            }
            return itemProperty;
        }
        protected Dictionary<string, object> AssmeblyItemInfoWithoutAddToCache(ClientContext context, string webServerRelativeUrl, bool hasException, bool isFile, Dictionary<string, ClientFile> filesMap, ListItem item)
        {
            //Do not need load Author and Editor during discover.
            //if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor")) //for community site discussion list
            //{
            //    context.Load(item);
            //    context.ExecuteQuery();
            //}
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);
            itemProperty["ObjType"] = ItemType.Item;
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
            if (isFile)
            {
                if (item.FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(item["FileRef"] as string) && filesMap.ContainsKey(item["FileRef"] as string))
                {
                    string fileRelativeUrl = item["FileRef"] as string;
                    ClientFile file = filesMap[fileRelativeUrl];
                    itemProperty["ServerRelativeUrl"] = fileRelativeUrl;
                    itemProperty["CheckoutUserId"] = null;
                    if (!hasException && file.CheckedOutByUser.IsPropertyAvailable("Id"))
                    {
                        itemProperty["CheckoutUserId"] = (int?)file.CheckedOutByUser.Id;
                    }
                }
                itemProperty["ObjType"] = ItemType.Document;
            }
            return itemProperty;
        }

        protected void EnsureParentThreadId(List list, List<Dictionary<string, object>> results)
        {
            if (list.BaseTemplate != (int)AveListTemplateType.DiscussionBoard)
            {
                return;
            }
            for (int i = results.Count - 1; i >= 0; i--)
            {
                Dictionary<string, object> tempItemProperties = results[i]["FieldValues"] as Dictionary<string, object>;
                try
                {
                    bool parentFound = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string currentThreadIndex = tempItemProperties["ThreadIndex"].ToString();
                        if (currentThreadIndex.StartsWith(results[j]["ThreadIndex"].ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            tempItemProperties["#ThreadIndexParentId"] = results[j]["Id"];
                            parentFound = true;
                            break;
                        }
                    }
                    if (!parentFound)
                    {
                        tempItemProperties["#ThreadIndexParentId"] = tempItemProperties["ParentFolderId"];
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Debug("Can not find item's parent thread index item.Using ParentFolderId instead of ThreadIndexParentId.Error:{0}", ex.ToString());
                    tempItemProperties["#ThreadIndexParentId"] = tempItemProperties["ParentFolderId"];
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected List<Dictionary<string, object>> GetFoldersByCaml(ClientContext context, List list, string webServerRelativeUrl, string folderUrl)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            if (!IsThrottled(list.ItemCount))
            {
                ListItemCollectionPosition pos = null;
                do
                {
                    var camlQuery = new CamlQuery()
                    {
                        ViewXml = AveCamlQueryString.GetAllItemsString(null, (int)this.MaxItemsPerThrottledOperation, QueryFindOption.None),
                        //FolderServerRelativeUrl = folderUrl,
                        ListItemCollectionPosition = pos,
                    };
                    SetCamlQueryFolderUrl(camlQuery, folderUrl);
                    var listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                            items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                        item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "1"));
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        var itemProperty = AssemblyFolderInfo(context, item);
                        results.Add(itemProperty);
                    }
                    pos = listItems.ListItemCollectionPosition;
                }
                while (pos != null);
            }
            else
            {
                QueryFoldersForLargeList(context, list, folderUrl, results);
            }

            //if (results.Count > 0)
            //{
            //    Folder folder = GetFolderByAPI(list.ParentWeb, folderUrl);
            //    context.Load(folder, f => f.Folders.IncludeWithDefaultProperties(tempFolder => tempFolder.ParentFolder.ServerRelativeUrl));
            //    context.ExecuteQuery();
            //    foreach (Folder tempFolder in folder.Folders)
            //    {
            //        Dictionary<string, object> folderProperty = new Dictionary<string, object>();
            //        AssembleFolderProperties(context as AveClientContext, webServerRelativeUrl, tempFolder, tempFolder.ServerRelativeUrl, folderProperty);
            //    }
            //}
            return results;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected List<Dictionary<string, object>> GetFoldersByCamlForDiscover(ClientContext context, List list, string webServerRelativeUrl, string folderUrl)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            List<string> viewFields = new List<string> { "FileDirRef", "FileLeafRef", "Title", "GUID", "_UIVersion", "ItemChildCount", "FolderChildCount" };
            if (list.BaseType != BaseType.DocumentLibrary)
            {
                viewFields.Add("Attachments");
            }

            if (!IsThrottled(list.ItemCount))
            {
                ListItemCollectionPosition pos = null;
                do
                {
                    var camlQuery = new CamlQuery()
                    {
                        ViewXml = AveCamlQueryString.GetAllItemsString(viewFields, (int)this.MaxItemsPerThrottledOperation, QueryFindOption.None),
                        //FolderServerRelativeUrl = folderUrl,
                        ListItemCollectionPosition = pos,
                    };
                    SetCamlQueryFolderUrl(camlQuery, folderUrl);
                    var listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                            items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                        item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "1"));
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        var itemProperty = AssemblyFolderInfoWithoutAddToCache(context, item);
                        results.Add(itemProperty);
                    }
                    pos = listItems.ListItemCollectionPosition;
                }
                while (pos != null);
            }
            else
            {
                QueryFoldersForLargeList(context, list, folderUrl, results, viewFields);
            }
            return results;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected Dictionary<string, object> AssemblyFolderInfo(ClientContext context, ListItem item)
        {
            if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor")) //for community site discussion list
            {
                context.Load(item);
                context.ExecuteQuery();
            }
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);
            itemProperty["ObjType"] = ItemType.Folder;
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
            return itemProperty;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected Dictionary<string, object> AssemblyFolderInfoWithoutAddToCache(ClientContext context, ListItem item)
        {
            //Do not need load Author and Editor during discover.
            //if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor")) //for community site discussion list
            //{
            //    context.Load(item);
            //    context.ExecuteQuery();
            //}
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);
            itemProperty["ObjType"] = ItemType.Folder;
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;

            return itemProperty;
        }

        protected void GetAttachmentsFromItem(ClientContext context, List list, Dictionary<string, object> item, string rootFolderServerRelativeUrl, string webServerRelativeUrl)
        {
            if (item.ContainsKey("Id") && item.ContainsKey("Attachments" + AveObjectModelConstant.ObjectPropertySuffix)
                && Convert.ToBoolean(item["Attachments" + AveObjectModelConstant.ObjectPropertySuffix]))
            {
                List<Dictionary<string, object>> attachments = item["Attachments"] as List<Dictionary<string, object>>;
                int id = (int)item["Id"];
                string attachmentFolderUrl = rootFolderServerRelativeUrl.TrimEnd('/') + "/Attachments/" + id;
                Folder attachmentFolder = GetFolderByAPI(list.ParentWeb, attachmentFolderUrl);
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        context.Load(attachmentFolder,
                                a => a.ServerRelativeUrl,
                                a => a.Files,
                                a => a.Files.IncludeWithDefaultProperties(file => file.Author, file => file.ModifiedBy, file => file.CheckedOutByUser));
                    }
                    using (exceptionScope.StartCatch())
                    {
                        context.Load(attachmentFolder,
                                a => a.ServerRelativeUrl,
                                a => a.Files);
                    }
                }
                context.ExecuteQuery();
                string attachmentFolderServerRelativeUrl = attachmentFolder.ServerRelativeUrl;
                foreach (ClientFile attachment in attachmentFolder.Files)
                {
                    Dictionary<string, object> attachmentPro = new Dictionary<string, object>();
                    string eTag = attachment.ETag.Trim('"');
                    string[] pros = eTag.Split(',');
                    attachmentPro["DocID"] = new Guid(pros[0]);
                    attachmentPro["DirName"] = attachmentFolderServerRelativeUrl;
                    attachmentPro["Name"] = attachmentPro["LeafName"] = attachment.Name;
                    //attachmentPro["UIVersion"] = attachment.UIVersion;//统一为UIVersion
                    attachmentPro["DocFlags"] = (int?)null;//cannot get this property
                    //attachmentPro["TimeLastModified"] = attachment.TimeLastModified;
                    attachmentPro["Level"] = (byte)attachment.Level;
                    attachmentPro["Type"] = (byte)FileSystemObjectType.File;
                    //attachmentPro["Size"] = 0; //cannot get this property
                    attachmentPro["ParentID"] = Guid.Empty;
                    attachmentPro["FullUrl"] = attachmentFolderServerRelativeUrl.TrimEnd('/') + "/" + attachmentPro["LeafName"];
                    attachmentPro["CheckoutUserId"] = (int?)null;
                    attachmentPro["HasStream"] = true;
                    attachmentPro["RbsId"] = null;
                    //attachmentPro["ServerRelativeUrl"] = attachment.ServerRelativeUrl;
                    attachmentPro["ID"] = (int?)id;
                    AssembleFileProperties(attachmentPro, attachment, webServerRelativeUrl, attachment.ListItemAllFields);
                    attachmentPro["Size"] = attachmentPro.ContainsKey("Length") ? int.Parse(attachmentPro["Length"].ToString()) : 0;
                    attachments.Add(attachmentPro);
                }
            }
        }

        public virtual Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl)
        {
            return QueryListItemForFB(siteId, webId, listId, folderId, folderUrl, false);
        }

        public virtual Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            using (AveClientContext context = CreateContext())
            {
                Guid id = Guid.Empty;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseTemplate);
                context.ExecuteQuery();
                int listTemplate = list.BaseTemplate;
                if (listTemplate != (int)ListTemplateType.Survey)
                {
                    CamlQuery query = new CamlQuery();
                    query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq></Where></Query></View>", tp_Guid);
                    ListItemCollection itemColl = list.GetItems(query);
                    context.Load(itemColl);
                    context.ExecuteQuery();
                    if (itemColl != null && itemColl.Count > 0)
                    {
                        id = (Guid)itemColl[0]["UniqueId"];
                    }
                }
                else
                {
                    ListItem item = list.GetItemById(rowId);
                    context.Load(item);
                    context.ExecuteQuery();
                    if (item != null)
                    {
                        id = (Guid)item["UniqueId"];
                    }
                }
                return id;
            }
        }

        public virtual Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return this.GetListItemGuid(webId, listId, tp_Guid, rowId);
        }

        public virtual Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId)
        {
            Dictionary<Guid, object> lists = new Dictionary<Guid, object>();
            try
            {
                Dictionary<string, object> allListProperties = GetLists(webId);
                List<Dictionary<string, object>> listPropertiesList = (List<Dictionary<string, object>>)allListProperties[AveObjectModelConstant.ChildrenProperties];
                foreach (Dictionary<string, object> listProperties in listPropertiesList)
                {
                    Dictionary<string, object> list = new Dictionary<string, object>();
                    list.Add("ListId", listProperties["Id"]);
                    list.Add("Name", listProperties["Title"]);
                    list.Add("Title", listProperties["Title"]);
                    list.Add("Type", listProperties["BaseType"]);
                    list.Add("Flag", listProperties["Flag"]);    //Can not get this property.
                    Dictionary<string, object> rootFolder = listProperties["RootFolderObject"] as Dictionary<string, object>;
                    list.Add("RootFolderUrl", rootFolder["ServerRelativeUrl"]);
                    list.Add("Hidden", listProperties["Hidden"]);
                    list.Add("ServerTemplate", listProperties["BaseTemplate"]);
                    if (rootFolder.ContainsKey("UniqueId"))
                    {
                        list.Add("RootFolderId", rootFolder["UniqueId"]);
                    }
                    else
                    {
                        list.Add("RootFolderId", Guid.Empty);
                    }
                    lists.Add((Guid)listProperties["Id"], list);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Query Lists For Web:{0} failed.Error Message:{1}", webId, ex.ToString());
            }
            finally
            {
            }
            //Add System Folder List
            //Dictionary<string, object> systemFolder = new Dictionary<string, object>();
            //systemFolder.Add("ListId", Guid.Empty);
            //systemFolder.Add("Name", "{System Folder}");
            //systemFolder.Add("Title", "{System Folder}");
            //systemFolder.Add("RootFolderId", Guid.Empty);

            //lists.Add(Guid.Empty, systemFolder);
            return lists;
        }

        public virtual int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache)
        {
            using (AveClientContext context = CreateContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;1;" + siteId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;1;" + siteId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                //changeCache的初始化操作，不能放入while(true)中，否则每一次循环开始，都会把已经获取的数据清空
                changeCache["ChangedSiteCache"] = new Dictionary<Guid, object>();
                changeCache["ChangedWebCache"] = new Dictionary<Guid, object>();
                changeCache["ChangedListCache"] = new Dictionary<Guid, object>();
                changeCache["ChangedItemsCache"] = new Dictionary<string, object>();
                Dictionary<string, object> changedItemsCache = changeCache["ChangedItemsCache"] as Dictionary<string, object>;
                changedItemsCache["ChangedFolderCache"] = new Dictionary<Guid, object>();
                changedItemsCache["ChangedFileCache"] = new Dictionary<Guid, object>();
                changedItemsCache["ChangedItemCache"] = new Dictionary<string, object>();

                while (true)
                {
                    ChangeCollection changedCollection = context.Site.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();

                    ConvertToChangeObject(changedCollection, changeCache);
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }
                if ((changeCache["ChangedSiteCache"] as Dictionary<Guid, object>).Count > 0)
                {
                    return 2;
                }
                return 0;
            }
        }

        public virtual Dictionary<string, object> GetWebChangesByQuery(string webServerRelativeUrl, Dictionary<string, object> queryProps)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> changesProps = new Dictionary<string, object>();
                Web targetWeb = context.Site.OpenWeb(webServerRelativeUrl);
                ChangeQuery query = GenerateChangeQuery(queryProps);
                ChangeCollection changeCollection = targetWeb.GetChanges(query);
                context.Load(changeCollection);
                context.ExecuteQuery();
                List<Dictionary<string, object>> changePropsList = new List<Dictionary<string, object>>();
                foreach (Change tempChange in changeCollection)
                {
                    Dictionary<string, object> changeProps = new Dictionary<string, object>();
                    CopyProperty(changeProps, tempChange);
                    changeProps["ChangeType"] = (int)tempChange.ChangeType;
                    changeProps["ChangeObjectType"] = tempChange.GetType().ToString();
                    changeProps["ChangeTokenString"] = tempChange.ChangeToken.StringValue;
                    changePropsList.Add(changeProps);
                }
                changesProps[AveObjectModelConstant.ChildrenProperties] = changePropsList;
                return changesProps;
            }
        }

        public virtual Dictionary<string, object> GetSiteChangesByQuery(Dictionary<string, object> queryProps)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> changesProps = new Dictionary<string, object>();
                var site = context.Site;
                ChangeQuery query = GenerateChangeQuery(queryProps);
                ChangeCollection changeCollection = site.GetChanges(query);
                context.Load(changeCollection);
                context.ExecuteQuery();
                List<Dictionary<string, object>> changePropsList = new List<Dictionary<string, object>>();
                foreach (Change tempChange in changeCollection)
                {
                    Dictionary<string, object> changeProps = new Dictionary<string, object>();
                    CopyProperty(changeProps, tempChange);
                    changeProps["ChangeType"] = (int)tempChange.ChangeType;
                    changeProps["ChangeObjectType"] = tempChange.GetType().ToString();
                    changeProps["ChangeTokenString"] = tempChange.ChangeToken.StringValue;
                    changePropsList.Add(changeProps);
                }
                changesProps[AveObjectModelConstant.ChildrenProperties] = changePropsList;
                return changesProps;
            }
        }

        public virtual Dictionary<string, object> GetListChangesByQuery(string webServerRelativeUrl, Guid listId, Dictionary<string, object> queryProps)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> changesProps = new Dictionary<string, object>();
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                ChangeQuery query = GenerateChangeQuery(queryProps);
                ChangeCollection changeCollection = list.GetChanges(query);
                context.Load(changeCollection);
                context.ExecuteQuery();
                List<Dictionary<string, object>> changePropsList = new List<Dictionary<string, object>>();
                foreach (Change tempChange in changeCollection)
                {
                    Dictionary<string, object> changeProps = new Dictionary<string, object>();
                    CopyProperty(changeProps, tempChange);
                    changeProps["ChangeType"] = (int)tempChange.ChangeType;
                    changeProps["ChangeObjectType"] = tempChange.GetType().ToString();
                    changeProps["ChangeTokenString"] = tempChange.ChangeToken.StringValue;
                    changePropsList.Add(changeProps);
                }
                changesProps[AveObjectModelConstant.ChildrenProperties] = changePropsList;
                return changesProps;
            }
        }

        private ChangeQuery GenerateChangeQuery(Dictionary<string, object> queryProps)
        {
            bool allChangeObjectTypes = queryProps.ContainsKey("allChangeObjectTypes") ? (bool)queryProps["allChangeObjectTypes"] : false;
            bool allChangeTypes = queryProps.ContainsKey("allChangeTypes") ? (bool)queryProps["allChangeTypes"] : false;
            ChangeQuery query = new ChangeQuery(allChangeObjectTypes, allChangeTypes);
            if (queryProps.ContainsKey("ChangeTokenStart"))
            {
                query.ChangeTokenStart = new ChangeToken()
                {
                    StringValue = queryProps["ChangeTokenStart"].ToString()
                };
            }
            if (queryProps.ContainsKey("ChangeTokenEnd"))
            {
                query.ChangeTokenEnd = new ChangeToken()
                {
                    StringValue = queryProps["ChangeTokenEnd"].ToString()
                };
            }
            AveObjectCopy.UpdateObjectBasicProperties(queryProps, query);
            return query;
        }

        public virtual Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<Guid, object> changedWebsProperties = new Dictionary<Guid, object>();
                foreach (KeyValuePair<Guid, object> pair in changedWebsInfo)
                {
                    Dictionary<string, object> change = pair.Value as Dictionary<string, object>;
                    if (change != null)
                    {
                        Dictionary<string, object> webProp = new Dictionary<string, object>();
                        if (!changedWebsProperties.ContainsKey(pair.Key))
                        {
                            AveChangeType changeType = (AveChangeType)change["ChangeType"];
                            webProp["ChangeType"] = (int)changeType;
                            webProp["WebID"] = pair.Key;
                            webProp["EventTime"] = change["Time"];
                            if (change.ContainsKey("NavigationChanged"))
                            {
                                webProp["NavigationChanged"] = change["NavigationChanged"];
                            }

                            if (change.ContainsKey("RoleAssignmentsChangeType"))
                            {
                                webProp["RoleAssignmentsChangeType"] = change["RoleAssignmentsChangeType"];
                            }
                            if (change.ContainsKey("PermissionLevelChangeType"))
                            {
                                webProp["PermissionLevelChangeType"] = change["PermissionLevelChangeType"];
                            }
                            if (change.ContainsKey("ColumnChangeType"))
                            {
                                webProp["ColumnChangeType"] = change["ColumnChangeType"];
                            }
                            if (change.ContainsKey("ContentTypeChangeType"))
                            {
                                webProp["ContentTypeChangeType"] = change["ContentTypeChangeType"];
                            }

                            if (changeType != AveChangeType.Delete)
                            {
                                Web web = context.Site.OpenWebById(pair.Key);
                                string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                                GetWebPropertiesForIB(web, context.Url, siteServerRelativeUrl, false, webProp);
                            }
                            changedWebsProperties.Add(pair.Key, webProp);
                        }
                    }
                }
                return changedWebsProperties;
            }
        }

        protected void GetWebPropertiesForIB(Web web, string siteUrl, string siteServerRelativeUrl, bool webLoaded, Dictionary<string, object> webProperties)
        {
            if (!webLoaded)
            {
                web.Context.Load(web);
                web.Context.ExecuteQuery();
            }
            webProperties["Title"] = web.Title;
            string Url = string.Empty;
            if (web.ServerRelativeUrl.Equals("/"))
            {
                Url = this.WebAppName;
            }
            else
            {
                Url = siteUrl.Replace(siteServerRelativeUrl, web.ServerRelativeUrl);
            }
            webProperties["FullUrl"] = Url;
            string Name = ".";
            if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl))
            {
                int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                Name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
            }
            webProperties["Name"] = Name;
        }

        public virtual Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            Dictionary<int, object> securityChanges = new Dictionary<int, object>();
            using (AveClientContext context = CreateContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;1;" + siteId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;1;" + siteId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;

                while (true)
                {
                    ChangeCollection changedCollection = context.Site.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    foreach (var changeObject in changedCollection)
                    {
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        int Id = -1;
                        if (changeObject is ChangeUser)
                        {
                            properties["IsUser"] = true;
                            Id = (changeObject as ChangeUser).UserId;
                            properties["PrincipleId"] = Id;
                        }
                        else if (changeObject is ChangeGroup)
                        {
                            properties["IsUser"] = false;
                            Id = (changeObject as ChangeGroup).GroupId;
                            properties["PrincipleId"] = Id;
                        }
                        if (Id == -1)
                        {
                            continue;
                        }
                        CopyProperty(properties, changeObject);
                        switch (changeObject.ChangeType)
                        {
                            case SPChangeType.DeleteObject:
                            case SPChangeType.MoveAway:
                            case SPChangeType.Rename:
                                properties["ChangeType"] = (int)AveChangeType.Delete;
                                break;
                            case SPChangeType.Restore:
                                properties["ChangeType"] = (int)AveChangeType.Restore;
                                break;
                            case SPChangeType.ScopeAdd:
                            case SPChangeType.ScopeDelete:
                            case SPChangeType.AssignmentAdd:
                            case SPChangeType.AssignmentDelete:
                            case SPChangeType.Add:
                            case SPChangeType.MoveInto:

                                properties["ChangeType"] = (int)AveChangeType.Add;
                                break;
                            case SPChangeType.MemberAdd:
                            case SPChangeType.RoleAdd:
                            case SPChangeType.MemberDelete:
                            case SPChangeType.SystemUpdate:
                            case SPChangeType.Update:
                                properties["ChangeType"] = (int)AveChangeType.Edit;
                                break;
                            default:
                                properties["ChangeType"] = (int)AveChangeType.Edit;
                                break;
                        }
                        securityChanges[Id] = properties;
                    }
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }
            }
            return securityChanges;
        }

        public virtual Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<Guid, object> lists = new Dictionary<Guid, object>();
                Web web = null;
                if (changedListCache.Count > 0)
                {
                    web = context.Site.OpenWebById(webId);
                }
                foreach (KeyValuePair<Guid, object> pair in changedListCache)
                {
                    Dictionary<string, object> change = pair.Value as Dictionary<string, object>;
                    if (change != null)
                    {
                        if (change.ContainsKey("WebId"))
                        {
                            Guid id = new Guid(change["WebId"].ToString());
                            if (id == webId)
                            {
                                Dictionary<string, object> listProp = new Dictionary<string, object>();
                                AveChangeType changeType = (AveChangeType)change["ChangeType"];
                                listProp["ChangeType"] = (int)changeType;
                                listProp["ListId"] = pair.Key;

                                if (change.ContainsKey("RoleAssignmentsChangeType"))
                                {
                                    listProp["RoleAssignmentsChangeType"] = change["RoleAssignmentsChangeType"];
                                }

                                if (changeType != AveChangeType.Delete)
                                {
                                    List list = web.Lists.GetById(pair.Key);
                                    context.Load(list);
                                    context.Load(list.RootFolder);
                                    context.ExecuteQuery();
                                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                                    CopyProperty(listProp, list);
                                    CopyProperty(rootFolderProp, list.RootFolder);
                                    long flag = 0;
                                    if (list.EnableVersioning)
                                        flag |= 0x0000000000000080;
                                    if (!list.EnableAttachments)
                                        flag |= 0x0000000000000008;
                                    listProp["Flag"] = flag;
                                    listProp["Name"] = listProp["Title"];
                                    listProp["Type"] = listProp["BaseType"];
                                    listProp["RootFolderUrl"] = rootFolderProp["ServerRelativeUrl"];
                                    listProp["ServerTemplate"] = listProp["BaseTemplate"];
                                    if (rootFolderProp.ContainsKey("UniqueId"))
                                    {
                                        listProp["RootFolderId"] = rootFolderProp["UniqueId"];
                                    }
                                    else
                                    {
                                        listProp["RootFolderId"] = Guid.Empty;
                                    }
                                }
                                lists.Add(pair.Key, listProp);
                            }
                        }
                    }
                }
                return lists;
            }
        }

        public virtual void DeleteSite(string CAUrl, string url)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteSiteToRecylebin(string CAUrl, string url)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mListID)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> folderPro = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(mListID);
                Folder folder = list.RootFolder;
                ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    using (scope.StartTry())
                    {
                        context.Load(folder);
                    }
                    using (scope.StartCatch())
                    {
                        context.Load(web, w => w.ServerRelativeUrl);
                        context.Load(list, l => l.Title);
                        context.Load(folder, f => f.ServerRelativeUrl, f => f.ParentFolder, f => f.Name);
                    }
                }
                context.ExecuteQuery();
                if (scope.HasException)
                {
                    string ex = "Access Denied, you don't have permission to access this data. ";
                    SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(web.ServerRelativeUrl, mSiteTrimObj.Name);
                    SecurityTrimObject listTrimObj = webTrimObj.GetList(mListID, list.Title);
                    SecurityTrimObject rootFolderTrimObj = listTrimObj.GetFolder(folder.ServerRelativeUrl, folder.Name);
                    rootFolderTrimObj.TrimmedProperties["Files"] = ex;
                    rootFolderTrimObj.TrimmedProperties["Folders"] = ex;
                    rootFolderTrimObj.TrimmedProperties["Tag"] = ex;
                    rootFolderTrimObj.TrimmedProperties["ItemCount"] = ex;
                    rootFolderTrimObj.TrimmedProperties["UniqueContentTypeOrder"] = ex;
                    rootFolderTrimObj.TrimmedProperties["WelcomePage"] = ex;
                    rootFolderTrimObj.TrimmedProperties["ServerObjectIsNull"] = ex;
                }
                //CamlQuery query = new CamlQuery();
                //string dirName = folder.ServerRelativeUrl.Substring(0, folder.ServerRelativeUrl.LastIndexOf('/'));
                //query.ViewXml = string.Format(
                //   "<View Scope=\"RecursiveAll\">" +
                //   "<Query><Where><And>" +
                //    "<Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                //   "<Eq><FieldRef Name=\"Title\"/><Value Type=\"Text\">{1}</Value></Eq>" +
                //   "<Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>1</Value></Eq>" +
                //   "</And></Where></Query></View>",
                //   dirName, folder.Name);
                //ListItemCollection itemColl = list.GetItems(query);
                //context.Load(itemColl);
                //context.ExecuteQuery();
                //ListItem item = itemColl[0];
                if (!string.IsNullOrEmpty(folder.ServerRelativeUrl))
                {
                    string serverRelativeUrl = folder.ServerRelativeUrl.Trim('/');
                    if (serverRelativeUrl.Contains("/"))
                    {
                        int index = serverRelativeUrl.LastIndexOf('/');
                        folderPro.Add("DirName", serverRelativeUrl.Substring(0, index));
                        folderPro.Add("LeafName", serverRelativeUrl.Substring(index + 1));
                    }
                    else
                    {
                        folderPro.Add("DirName", "");
                        folderPro.Add("LeafName", serverRelativeUrl);
                    }
                    folderPro.Add("FullUrl", folder.ServerRelativeUrl);
                }
                else
                {
                    throw new MissingMemberException("There is no serverRelativeUrl.");
                }
                folderPro.Add("Size", 0);    //Can not get this property.
                #region there is no following parameters in web root folder, so we set them as default value
                folderPro.Add("Type", Convert.ToByte(2));
                folderPro.Add("Level", Convert.ToByte(1));
                folderPro.Add("ID", null);
                folderPro.Add("DocID", Guid.Empty);
                folderPro.Add("CheckoutUserId", (int?)null);
                folderPro.Add("Hidden", (bool?)true);
                folderPro.Add("UIVersion", 512);
                folderPro.Add("DocFlags", 0);
                folderPro.Add("HasStream", 0);
                folderPro.Add("ParentID", Guid.Empty);
                folderPro.Add("TimeLastModified", DateTime.MinValue);
                folderPro.Add("IsCurrentVersion", (bool?)true);
                folderPro.Add("QueryType", 2);
                #endregion

                return folderPro;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        public virtual Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemPro = null;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType);
                ListItem item = null;
                if (Guid.Empty == id && !isListItem)
                {
                    item = GetListItemByDirName(context, list, dirName, leafName);
                }
                else
                {
                    item = GetListItemByUniqueId(context, list, id);
                }
                if (item != null)
                {
                    itemPro = new Dictionary<string, object>();
                    itemPro["Attachments"] = new List<Dictionary<string, object>>();
                    itemPro["Versions"] = new List<Dictionary<string, object>>();
                    AssembleDiscoverItemProperties(itemPro, item);
                    itemPro["RbsId"] = null;
                    itemPro["ObjType"] = 1; //Item
                    itemPro["CheckoutUserId"] = (int?)null;
                    if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        if (item.FileSystemObjectType == FileSystemObjectType.Folder)
                        {
                            itemPro["ObjType"] = 4;
                        }
                        else
                        {
                            itemPro["ObjType"] = 2;
                            ClientFile file = GetFileByAPI(list.ParentWeb, itemPro["FullUrl"].ToString());
                            context.Load(file, f => f.CheckedOutByUser);
                            context.ExecuteQuery();
                            if (file.IsObjectPropertyInstantiated("CheckedOutByUser") && file.IsPropertyAvailable("Id"))
                            //if (!file.CheckedOutByUser.ServerObjectIsNull.Value)
                            {
                                itemPro["CheckoutUserId"] = (int?)file.CheckedOutByUser.Id;
                            }
                        }
                    }
                }
                return itemPro;
            }
        }

        public virtual DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        {
            using (AveClientContext context = CreateContext())
            {
                DateTime time = DateTime.MinValue;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType);
                ListItem item = GetListItemByUniqueId(context, list, id);
                if (item != null)
                {
                    time = (DateTime)item.FieldValues["Modified"];
                }
                return time;
            }
        }

        public virtual DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            using (AveClientContext context = CreateContext())
            {
                DateTime time = DateTime.MinValue;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(web, w => w.ServerRelativeUrl);
                context.Load(list, l => l.BaseType);
                context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                ListItem item = GetListItemByDirName(context, list, dirName, leafName);
                if (item != null)
                {
                    time = (DateTime)item.FieldValues["Modified"];
                    docId = (Guid)item.FieldValues["UniqueId"];
                }
                else//得到listrootfolder下的系统文件
                {
                    Dictionary<string, object> viewProperty = GetViewItem(web, list.RootFolder.ServerRelativeUrl, (list.BaseType.Equals(BaseType.GenericList)), dirName, leafName);
                    if (viewProperty != null)
                    {
                        time = (DateTime)viewProperty["TimeLastModified"];
                        docId = (Guid)viewProperty["DocID"];
                    }
                }
                return time;
            }
        }

        public virtual DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
        {
            using (AveClientContext context = CreateContext())
            {
                DateTime time = DateTime.MinValue;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType);
                ListItem item = GetListItemBytpGuid(context, list, tp_Guid);
                if (item != null)
                {
                    time = (DateTime)item.FieldValues["Modified"];
                    docId = (Guid)item.FieldValues["UniqueId"];
                }
                return time;
            }
        }

        protected ListItem GetListItemByUniqueId(ClientContext context, List list, Guid id)
        {
            ListItem item = null;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\">" +
                "<Query><Where>" +
                "<Eq><FieldRef Name=\"UniqueId\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                "</Where></Query></View>",
                id.ToString());
            ListItemCollection listItems = list.GetItems(camlQuery);
            context.Load(listItems);
            context.ExecuteQuery();
            if (listItems.Count == 1)
            {
                item = listItems[0];
            }
            return item;
        }

        protected ListItem GetListItemByID(ClientContext context, List list, int id)
        {
            ListItem item = null;
            CamlQuery camlQuery = new CamlQuery();
            StringBuilder queryXml = new StringBuilder();
            queryXml.Append("<View Scope='RecursiveAll'><Query><Where><In><FieldRef Name='ID'/><Values>");
            queryXml.Append(string.Format("<Value Type=\"Integer\">{0}</Value>", id));
            queryXml.Append("</Values></In></Where></Query></View>");
            camlQuery.ViewXml = queryXml.ToString();

            ListItemCollection listItems = list.GetItems(camlQuery);
            context.Load(listItems);
            context.ExecuteQuery();
            if (listItems.Count == 1)
            {
                item = listItems[0];
            }
            return item;

        }

        protected ListItem GetListItemByDirName(ClientContext context, List list, string dirName, string leafName)
        {
            ListItem item = null;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\">" +
                "<Query><Where><And>" +
                "<Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                "<Eq><FieldRef Name=\"FileLeafRef\"/><Value Type=\"Lookup\">{1}</Value></Eq>" +
                "</And></Where></Query></View>",
                dirName, leafName);
            ListItemCollection listItems = list.GetItems(camlQuery);
            context.Load(listItems);
            context.ExecuteQuery();
            if (listItems.Count == 1)
            {
                item = listItems[0];
            }
            return item;
        }

        protected ListItem GetListItemBytpGuid(ClientContext context, List list, Guid tp_Guid, bool isLast = false)
        {
            ListItem item = null;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\">" +
                "<Query><Where>" +
                "<Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq>" +
                "</Where></Query></View>",
                tp_Guid.ToString());
            ListItemCollection listItems = list.GetItems(camlQuery);
            context.Load(listItems);
            context.ExecuteQuery();
            if (listItems.Count > 0)
            {
                item = isLast ? listItems.Last() : listItems.First();
            }
            return item;
        }

        public virtual Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mListID)
        {
            return null;
        }

        public virtual Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<Guid, object> views = new Dictionary<Guid, object>();
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ViewCollection viewColl = list.Views;
                //context.Load(viewColl, vc => vc.Include(v => v.Id, v => v.PersonalView, v => v.BaseViewId, v => v.Title, v => v.ServerRelativeUrl));
                context.Load(viewColl);
                context.ExecuteQuery();
                foreach (View view in viewColl)
                {
                    Dictionary<string, object> viewPro = new Dictionary<string, object>();
                    ClientFile file = GetFileByAPI(web, view.ServerRelativeUrl);
                    //context.Load(file, f => f.ETag, f => f.Name, f => f.ListItemAllFields, f => f.UIVersion, f => f.TimeLastModified, f => f.Level);
                    try
                    {
                        context.Load(file);
                        context.ExecuteQuery();
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Query ListView:{0} Failed.Error Message:{1}", view.ServerRelativeUrl, ex.ToString());
                        context.Load(web);
                        file = GetFileByAPI(web, view.ServerRelativeUrl);
                        context.Load(file);
                        context.ExecuteQuery();
                    }
                    AssembleDiscoverViewProperties(viewPro, view, file);
                    viewPro["CheckoutUserId"] = (int?)null;
                    views.Add(view.Id, viewPro);
                }
                return views;
            }
        }

        public virtual Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mListID)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseTemplate);
                context.ExecuteQuery();
                int listTemplate = list.BaseTemplate;
                if (listTemplate != (int)ListTemplateType.Survey)
                {
                    CamlQuery query = new CamlQuery();
                    query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq></Where></Query></View>", tpGuid);
                    ListItemCollection itemColl = list.GetItems(query);
                    context.Load(itemColl);
                    context.ExecuteQuery();
                    if (itemColl != null && itemColl.Count > 0)
                    {
                        return true;
                    }
                }
                else
                {
                    ListItem item = list.GetItemById(rowId);
                    context.Load(item);
                    context.ExecuteQuery();
                    if (item != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public virtual Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<byte[], object> contentTypes = new Dictionary<byte[], object>();
                Web web = context.Site.OpenWebById(webId);
                ContentTypeCollection ctColl = web.ContentTypes;
                context.Load(ctColl, collection => collection.Include(ct => ct.Id, ct => ct.Name, ct => ct.SchemaXml, ct => ct.Scope));
                context.ExecuteQuery();
                foreach (ContentType ct in ctColl)
                {
                    Dictionary<string, object> contentTye = new Dictionary<string, object>();
                    byte[] id = Encoding.UTF8.GetBytes(ct.Id.ToString());
                    contentTye["ContentTypeId"] = id;
                    contentTye["Name"] = ct.Name;
                    contentTye["SchemaXml"] = ct.SchemaXml;
                    contentTye["Scope"] = ct.Scope.TrimStart('/');
                    Dictionary<string, object> folder = GetContentTypeRelatedFolder(context, ct.SchemaXml, web, ct.Scope);
                    contentTye["RelatedFolder"] = folder;
                    contentTypes.Add(id, contentTye);
                }
                return contentTypes;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Uiversion used as key, change to UIVersion in DA6.2.")]
        protected Dictionary<string, object> GetContentTypeRelatedFolder(ClientContext context, string schema, Web web, string scope)
        {
            Dictionary<string, object> folderPro = null;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(schema);
            XmlNode node = xDoc.DocumentElement.SelectSingleNode("Folder");
            if (node != null)
            {
                string folderName = node.Attributes["TargetName"].Value;
                Folder folder = GetFolderByAPI(web, scope + "/" + folderName);
                context.Load(folder);
                context.ExecuteQuery();
                folderPro = new Dictionary<string, object>();
                folderPro.Add("DocID", Guid.Empty);   //Can not get Guid of root folder.
                folderPro.Add("DirName", folder.ServerRelativeUrl.Substring(0, folder.ServerRelativeUrl.Length - (folder.Name.Length + 1)).TrimStart('/'));
                folderPro.Add("LeafName", folder.Name);
                folderPro.Add("ID", null);  //Can not get ID of root folder.
                folderPro.Add("Uiversion", 512);    //Can not get this property.
                folderPro.Add("DocFlags", null);    //Can not get this property.
                folderPro.Add("TimeLastModified", DateTime.MinValue);    //Can not get this property.
                folderPro.Add("Level", Convert.ToByte(1));    //Can not get this property. default value: Published
                folderPro.Add("Type", Convert.ToByte(1));    //Can not get this property.  default value: Folder
                folderPro.Add("Size", 0);    //Can not get this property.
                folderPro.Add("ParentID", Guid.Empty);    //Can not get this property.
                folderPro.Add("FullUrl", folder.ServerRelativeUrl);
                folderPro.Add("CheckoutUserId", (int?)null);
                folderPro.Add("Hidden", (bool?)true);
            }
            return folderPro;
        }

        public virtual Dictionary<string, object> QueryWebRootFolder(Guid webId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> folder = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                Folder rootFolder = web.RootFolder;
                context.Load(rootFolder, f => f.ServerRelativeUrl);
                context.ExecuteQuery();

                var fullUrl = rootFolder.ServerRelativeUrl.Trim('/');
                int index = fullUrl.LastIndexOf('/');//Root site collection
                var dirName = index >= 0 ? fullUrl.Substring(0, index) : string.Empty;
                var leafName = index >= 0 ? fullUrl.Substring(index + 1) : fullUrl;

                folder.Add("DirName", dirName);
                folder.Add("LeafName", leafName);
                folder.Add("FullUrl", fullUrl);

                folder.Add("DocID", Guid.Empty);   //Can not get Guid of root folder.
                folder.Add("ID", null);  //Can not get ID of root folder.
                folder.Add("UIVersion", 512);    //Can not get this property.
                folder.Add("DocFlags", null);    //Can not get this property.
                folder.Add("TimeLastModified", DateTime.MinValue);    //Can not get this property.
                folder.Add("Level", Convert.ToByte(1));    //Can not get this property. default value: Published
                folder.Add("Type", Convert.ToByte(1));    //Can not get this property.  default value: Folder
                folder.Add("Size", 0);    //Can not get this property.
                folder.Add("ParentID", Guid.Empty);    //Can not get this property.

                folder.Add("CheckoutUserId", (int?)null);
                folder.Add("Hidden", (bool?)true);
                return folder;
            }
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.Item = true;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = true;
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                context.ExecuteQuery();
                Dictionary<string, object> changedItemCache = new Dictionary<string, object>();
                while (true)
                {
                    ChangeCollection changedCollection = list.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    changedItemCache.AddRange(GetChangeItemObject(changedCollection));
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }
                return changedItemCache;
            }
        }

        internal IDictionary<string, object> GetChangeItemObject(ChangeCollection changeCollection)
        {
            Dictionary<string, object> changedItemCache = new Dictionary<string, object>();
            foreach (Change changeObject in changeCollection)
            {
                Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                CopyProperty(objectProperties, changeObject);
                SPChangeType currentChangeType = (SPChangeType)objectProperties["ChangeType"];
                objectProperties["SPChangeType"] = currentChangeType.ToString();
                switch (changeObject.GetType().ToString())
                {
                    case "Microsoft.SharePoint.Client.ChangeItem":
                        Guid itemWebId = new Guid(objectProperties["WebId"].ToString());
                        Guid itemlistId = new Guid(objectProperties["ListId"].ToString());
                        ConvertToChangeItem(changeObject, currentChangeType, itemlistId, objectProperties, changedItemCache);
                        break;
                    default:
                        break;
                }
            }
            return changedItemCache;
        }

        public virtual Dictionary<string, object> GetItemByUniqueId(Guid webId, Guid listId, Guid itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemProp = null;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ListItem item = GetListItemByUniqueId(context, list, itemId);
                if (item != null)
                {
                    context.Load(list, tempList => tempList.BaseType, tempList => tempList.BaseTemplate);
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments);
                    context.Load(item, tempItem => tempItem.DisplayName);
                    context.ExecuteQuery();

                    itemProp = new Dictionary<string, object>();
                    GetItemDic(itemProp, item);
                    if (!ItemHasVersion(list, itemProp) || !WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                    {
                        itemProp["HasVersion"] = false;
                    }
                }
                return itemProp;
            }
        }

        protected virtual int GetItemIdByUniqueId(string webServerRelativeUrl, Guid itemId, string listTitle, ref Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetByTitle(listTitle);
                context.Load(list, l => l.Id);
                ListItem item = null;
                ListItemCollectionPosition pos = null;
                do
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ListItemCollectionPosition = pos;
                    camlQuery.ViewXml = string.Format(
                        "<View Scope=\"RecursiveAll\">" +
                        "<Query><Where>" +
                        "<Eq><FieldRef Name=\"UniqueId\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                        "</Where></Query></View>",
                        itemId.ToString());
                    ListItemCollection listItems = list.GetItems(camlQuery);
                    context.Load(listItems);
                    context.ExecuteQuery();
                    pos = listItems.ListItemCollectionPosition;
                    if (listItems.Count == 1)
                    {
                        item = listItems[0];
                        break;
                    }
                }
                while (pos != null);
                listId = list.Id;
                return item.Id;
            }
        }

        public virtual Dictionary<string, object> GetItemById(Guid webId, Guid listId, int itemId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> itemProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(itemId);
                context.Load(item);
                context.ExecuteQuery();
                CopyProperty(itemProp, item);
                return itemProp;
            }
        }

        /// <summary>
        /// Get ListItem by tp_Guid, just return the item's properties.
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="tp_Guid"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetItemByGuid(Guid webId, Guid listId, Guid tp_Guid)
        {
            Dictionary<string, object> itemProperyies = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ListItem item = GetListItemBytpGuid(context, list, tp_Guid, true);
                if (item == null) { return null; }
                CopyProperty(itemProperyies, item);
                return itemProperyies;
            }
        }

        public virtual Dictionary<string, object> GetWebFileItem(Guid webId, Guid fileId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProp = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                FileCollection files = web.RootFolder.Files;
                context.Load(files);
                context.ExecuteQuery();
                foreach (ClientFile file in files)
                {
                    string[] ids = GetIdsFromEtag(file.ETag);
                    string uniqueId = ids[0];
                    if (!string.IsNullOrEmpty(uniqueId))
                    {
                        if (fileId.ToString().Equals(uniqueId))
                        {
                            CopyProperty(fileProp, file);
                            break;
                        }
                    }
                }
                return fileProp;
            }
        }

        #endregion

        #region set
        public virtual void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            throw new NotImplementedException();
        }
        public virtual bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating)
        {
            throw new NotImplementedException();
        }
        public virtual void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            throw new NotImplementedException();
        }
        public virtual void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> LikePost(string postId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> CreatePost(string targetId, Dictionary<string, object> creationData)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetFullThread(string threadId)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetFeedFor(string postId, Dictionary<string, object> options)
        {
            throw new NotImplementedException();
        }
        //public virtual Dictionary<string, object> SetWebNavigationSettings(string webServerRelativeUrl, int globalSource, int currentSource, Dictionary<string, Guid> globalTaxonomy, Dictionary<string, Guid> currentTaxonomy)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
        public virtual List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {
            using (AveClientContext context = CreateContext())
            {
                List<AveListBrowserInfo> listInfoList = new List<AveListBrowserInfo>();
                Web web = context.Site.OpenWebById(option.ParentWebId);
                context.Load(context.Site, s => s.Url);
                context.Load(web.Lists, tempListCollection => tempListCollection.Include(l => l.Id,
                                                             l => l.ParentWebUrl,
                                                             l => l.Title,
                                                             l => l.BaseType,
                                                             l => l.BaseTemplate,
                                                             l => l.Hidden,
                                                             l => l.EnableVersioning,
                                                             l => l.EnableAttachments,
                                                             l => l.RootFolder.ServerRelativeUrl,
                                                             l => l.RootFolder.Name,
                                                             l => l.HasUniqueRoleAssignments,
                                                             l => l.EnableFolderCreation));
                context.ExecuteQuery();
                option.ChildrenTotalCount = web.Lists.Count;
                int pagingCount = 0;
                if (option.ChildrenTotalCount - option.StartIndex < option.PerPage)
                {
                    pagingCount = option.ChildrenTotalCount - option.StartIndex;
                }
                else
                {
                    pagingCount = (int)option.PerPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        List list = web.Lists[i + option.StartIndex];
                        AveListBrowserInfo listInfo = new AveListBrowserInfo();
                        listInfo.BaseTemplate = list.BaseTemplate;
                        listInfo.BaseType = (int)list.BaseType;
                        listInfo.EnableFolderCreation = list.EnableFolderCreation;
                        listInfo.HasUniqueRoleAssignments = list.HasUniqueRoleAssignments;
                        listInfo.Hidden = list.Hidden;
                        listInfo.ID = list.Id;
                        listInfo.Name = list.Title;
                        listInfo.rootFolderName = list.RootFolder.Name;
                        listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                        listInfo.Title = list.Title;
                        listInfo.Url = new Uri(new Uri(context.Site.Url), list.RootFolder.ServerRelativeUrl).ToString();
                        listInfoList.Add(listInfo);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, option.ChildrenTotalCount, ex.ToString());
                }
                return listInfoList;
            }
        }

        public virtual AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            AveWebBrowserInfo webBrowserInfo = new AveWebBrowserInfo();
            using (AveClientContext context = CreateContext())
            {
                Web rootWeb = context.Site.RootWeb;
                context.Load(rootWeb, w => w.ServerRelativeUrl,
                                             w => w.Id,
                                             w => w.Title,
                                             w => w.ListTemplates,
                                             w => w.Language,
                                             w => w.HasUniqueRoleAssignments);
                context.ExecuteQuery();
                SetWebBrowserInfo(webBrowserInfo, rootWeb);
                return webBrowserInfo;
            }
        }

        public virtual List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            using (AveClientContext context = CreateContext())
            {
                List<AveWebBrowserInfo> webInfos = new List<AveWebBrowserInfo>();
                WebCollection subWebs = null;
                int pagingCount = 0;
                try
                {
                    Web parentWeb = context.Site.OpenWebById(option.ParentWebId);
                    subWebs = parentWeb.GetSubwebsForCurrentUser(null);
                    context.Load(subWebs, webs => webs.IncludeWithDefaultProperties(w => w.ListTemplates, w => w.HasUniqueRoleAssignments));
                    context.ExecuteQuery();
                    option.ChildrenTotalCount = subWebs.Count;
                    if (option.StartIndex > option.ChildrenTotalCount)
                    {
                        foreach (Web web in subWebs)
                        {
                            AveWebBrowserInfo info = new AveWebBrowserInfo();
                            SetWebBrowserInfo(info, web);
                            webInfos.Add(info);
                        }
                        option.StartIndex = 0;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetBrowserWebFailed, context.Url, e.ToString());
                    throw;
                }
                if (option.ChildrenTotalCount - option.StartIndex < option.PerPage)
                {
                    pagingCount = option.ChildrenTotalCount - option.StartIndex;
                }
                else
                {
                    pagingCount = (int)option.PerPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        AveWebBrowserInfo info = new AveWebBrowserInfo();
                        SetWebBrowserInfo(info, subWebs[option.StartIndex + i]);
                        webInfos.Add(info);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, option.ChildrenTotalCount, ex.ToString());
                }
                return webInfos;
            }
        }

        protected virtual void SetWebBrowserInfo(AveWebBrowserInfo info, Web web)
        {
            using (AveClientContext context = CreateContext())
            {
                string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                string Url = string.Empty;
                if (web.ServerRelativeUrl.Equals("/"))
                {
                    Url = this.WebAppName;
                }
                else
                {
                    if (!siteServerRelativeUrl.Equals("/"))
                    {
                        Url = context.Url.Replace(siteServerRelativeUrl, web.ServerRelativeUrl);
                    }
                    else//host header类型的sitecollection走一下逻辑；
                    {
                        Url = string.Format("{0}/{1}", context.Url.TrimEnd('/'), web.ServerRelativeUrl.TrimStart('/'));
                    }
                }
                info.ID = web.Id;
                info.Title = web.Title;
                info.Url = Url;
                info.IsRootWeb = false;
                info.ServerRelativeUrl = web.ServerRelativeUrl;
                string name = string.Empty;
                if (web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    info.IsRootWeb = true;
                }
                else
                {
                    int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                    name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
                }
                info.Name = name;
                info.Language = web.Language;
                info.HasUniqueRoleAssignments = web.HasUniqueRoleAssignments;
                GetWebTemplate(info, web, context);
            }
        }

        protected virtual void GetWebTemplate(AveWebBrowserInfo info, Web web, AveClientContext context)
        {
            string webTemplate = string.Empty;
            foreach (ListTemplate temp in web.ListTemplates)
            {
                if (WrapperConfiguration.BPOS_S.ListTemplatesInMeetingSite.Contains(temp.ListTemplateTypeKind.ToString()))
                {
                    webTemplate = "MPS";
                    break;
                }
            }
            if (string.IsNullOrEmpty(webTemplate))
            {
                webTemplate = "STS";
            }
            string siteUrl = this.WebAppName + AveUrlUtility.GetSiteServerRelativeUrl(web.Context.Url);
            using (AveWebServiceRequest aveWebServiceRequest = new AveWebServiceRequest(siteUrl, mUserAccountInfo, mObj, "14"))
            {
                string tempString = AveWebServiceRequest.GetWebTemplateConfiguration(this.WebAppName, web.ServerRelativeUrl, mObj);
                Dictionary<string, object> WebTemplates = aveWebServiceRequest.GetWebTemplates(web.ServerRelativeUrl, web.Language, false, "");
                info.TemplateTitle = GetWebTemplateNameById(tempString, WebTemplates);
            }
            info.TemplateName = webTemplate;
        }

        private string GetWebTemplateNameById(string configuration, Dictionary<string, object> webTemplates)
        {
            string webTemplateStr = string.Empty;
            foreach (object sWebTemplate in webTemplates["ChildrenProperties"] as List<Dictionary<string, object>>)
            {
                Dictionary<string, object> WebTemplates = sWebTemplate as Dictionary<string, object>;
                if (WebTemplates["Name"].ToString().EndsWith(configuration, StringComparison.OrdinalIgnoreCase))
                {
                    webTemplateStr = WebTemplates["Title"].ToString();
                    break;
                }
            }
            return webTemplateStr;
        }
        public virtual AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            AveFolderBrowserInfo rootFolderInfo = new AveFolderBrowserInfo();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(option.ParentWebId);
                List list = web.Lists.GetById(option.ParentListId);
                Folder folder = list.RootFolder;
                context.Load(context.Site, s => s.Url);
                context.Load(web, w => w.ServerRelativeUrl);
                context.Load(list, l => l.Title, l => l.Id);
                context.Load(folder, f => f.ServerRelativeUrl, f => f.Name);
                context.ExecuteQuery();
                Dictionary<string, object> folderInfo = this.GetFolder(context, web.ServerRelativeUrl, list.Title, list.Id, folder.ServerRelativeUrl);
                if (folderInfo.ContainsKey("HasUniqueRoleAssignments") && folderInfo["HasUniqueRoleAssignments"] != null)
                {
                    rootFolderInfo.HasUniqueRoleAssignments = Boolean.Parse(folderInfo["HasUniqueRoleAssignments"].ToString());
                }
                if (folderInfo.ContainsKey("UniqueId") && folderInfo["UniqueId"] != null)
                {
                    rootFolderInfo.UniqueId = new Guid(folderInfo["UniqueId"].ToString());
                }
                rootFolderInfo.Name = folder.Name;
                rootFolderInfo.ParentListId = option.ParentListId;
                rootFolderInfo.Url = new Uri(new Uri(context.Site.Url), folderInfo["ServerRelativeUrl"].ToString()).ToString();
                rootFolderInfo.ServerRelativeUrl = folderInfo["ServerRelativeUrl"].ToString();
            }
            return rootFolderInfo;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ls is a variable")]
        public virtual List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            List<AveFolderBrowserInfo> folders = new List<AveFolderBrowserInfo>();
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWebById(option.ParentWebId);
                    List list = web.Lists.GetById(option.ParentListId);
                    //items properties
                    ListItemCollection listItems = null;
                    List<ListItem> items = new List<ListItem>();
                    if (option.ParentListId != null && option.ParentListId != Guid.Empty)
                    {
                        IEnumerable<ListItem> enumerableItems = new List<ListItem>();
                        ListItemCollectionPosition pos = null;
                        do
                        {
                            CamlQuery camlQuery = new CamlQuery();
                            camlQuery.ListItemCollectionPosition = pos;
                            camlQuery.FolderServerRelativeUrl = option.ParentFolderServerRelativeUrl;
                            camlQuery.ViewXml = string.Format(
                                "<View Scope=\"\">" +
                                "<Query><OrderBy><FieldRef Name=\"Title\" /></OrderBy>" + "<Where><And>" +
                                "<Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                                "<Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>1</Value></Eq>" +
                                "</And></Where></Query><RowLimit>{1}</RowLimit></View>",
                                option.ParentFolderServerRelativeUrl, option.StartIndex + option.PerPage);
                            listItems = list.GetItems(camlQuery);
                            context.Load(listItems, its => its.ListItemCollectionPosition,
                                its => its.IncludeWithDefaultProperties(it => it.HasUniqueRoleAssignments));
                            context.ExecuteQuery();
                            enumerableItems = enumerableItems.Concat(listItems);
                            pos = listItems.ListItemCollectionPosition;
                        }
                        while (pos != null);
                        items = enumerableItems.ToList();
                    }
                    int pagingCount = 0;
                    option.ChildrenTotalCount = items.Count;
                    if (option.ChildrenTotalCount - option.StartIndex < option.PerPage)
                    {
                        pagingCount = option.ChildrenTotalCount - option.StartIndex;
                    }
                    else
                    {
                        pagingCount = (int)option.PerPage;
                    }
                    try
                    {
                        for (int i = 0; i < pagingCount; i++)
                        {
                            ListItem item = items[i + option.StartIndex];
                            AveFolderBrowserInfo folderInfo = new AveFolderBrowserInfo();
                            folderInfo.UniqueId = (Guid)item.FieldValues["UniqueId"];
                            folderInfo.Name = item["Title"].ToString();
                            folderInfo.ServerRelativeUrl = item["FileRef"].ToString();
                            folderInfo.Url = new Uri(new Uri(this.mWebUrl), folderInfo.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
                            folderInfo.ParentListId = option.ParentListId;
                            folderInfo.ParentId = option.ParentFolderId;
                            folderInfo.HasUniqueRoleAssignments = item.HasUniqueRoleAssignments;
                            folders.Add(folderInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, option.ChildrenTotalCount, ex.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn(string.Format("get browser folders failed, parent folder url: {0}", option.ParentFolderServerRelativeUrl), e);
            }
            return folders;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ls is a variable")]
        public virtual List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {
            List<AveItemBrowserInfo> itemBrowserInfos = new List<AveItemBrowserInfo>();

            using (AveClientContext context = CreateContext())
            {
                Web parentWeb = context.Site.OpenWebById(option.ParentWebId);
                context.Load(parentWeb, web => web.ServerRelativeUrl);
                context.Load(parentWeb.Lists, ls => ls.Include(l => l.RootFolder.ServerRelativeUrl, l => l.BaseType));
                Folder parentFolder = GetFolderByAPI(parentWeb, option.ParentFolderServerRelativeUrl);
                context.ExecuteQuery();
                List list = GetParentList(parentWeb, option.ParentFolderServerRelativeUrl);

                if (list == null)
                {
                    context.Load(parentFolder.Files, fs => fs.Include(f => f.Name));
                    foreach (ClientFile file in parentFolder.Files)
                    {
                        AveItemBrowserInfo itemInfo = new AveItemBrowserInfo();
                        SetFileBrowserInfos(itemInfo, file, parentWeb.ServerRelativeUrl);
                        itemBrowserInfos.Add(itemInfo);
                    }
                }
                else
                {
                    var folderQuery = new CamlQuery()
                    {
                        ViewXml = string.Format(
                        "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query></View>",
                        option.ParentFolderServerRelativeUrl),
                    };
                    ListItemCollection listFolders = list.GetItems(folderQuery);
                    context.Load(listFolders);
                    context.Load(list, l => l.ItemCount);
                    context.ExecuteQuery();
                    Guid parentFolderId = Guid.Empty;
                    if (listFolders != null && listFolders.Count == 1)
                    {
                        ListItem item = listFolders[0];
                        var itmProp = new Dictionary<string, object>();
                        GetItemDic(itmProp, item);
                        parentFolderId = new Guid(itmProp["UniqueId"].ToString());
                    }
                    string viewXml = list.BaseType == BaseType.DocumentLibrary ?
                                                    IsThrottled(list.ItemCount) ?//对于DocumentLibrary，如果IsThrottled为true，那么不能使用FileLeafRef做orderby,否则会exception
                                                    "<View Scope=\"\">\r\n<Query>\r\n<Where>\r\n<Eq>\r\n<FieldRef Name=\"FSObjType\" />\r\n<Value Type=\"Integer\">0</Value>\r\n</Eq>\r\n</Where>\r\n</Query>\r\n<RowLimit>" + option.PerPage + "</RowLimit></View>"
                                                    : "<View Scope=\"\">\r\n<Query>\r\n<OrderBy>\r\n<FieldRef Name='FileLeafRef'/>\r\n</OrderBy>\r\n<Where>\r\n<Eq>\r\n<FieldRef Name=\"FSObjType\" />\r\n<Value Type=\"Integer\">0</Value>\r\n</Eq>\r\n</Where>\r\n</Query>\r\n<RowLimit>" + option.PerPage + "</RowLimit></View>"
                                                    : "<View Scope=\"\">\r\n<Query>\r\n<Where>\r\n<Eq>\r\n<FieldRef Name=\"FSObjType\" />\r\n<Value Type=\"Integer\">0</Value>\r\n</Eq>\r\n</Where>\r\n</Query>\r\n<RowLimit>" + option.PerPage + "</RowLimit></View>";
                    var camlquery = new CamlQuery()
                    {
                        ViewXml = viewXml,
                    };

                    SetCamlQueryFolderUrl(camlquery, option.ParentFolderServerRelativeUrl);

                    if (!string.IsNullOrEmpty(option.PageInfo))
                    {
                        camlquery.ListItemCollectionPosition = new ListItemCollectionPosition
                        {
                            PagingInfo = option.PageInfo
                        };
                    }

                    if (!IsThrottled(list.ItemCount))
                    {
                        var items = list.GetItems(camlquery);
                        var scope = new ExceptionHandlingScope(context);
                        using (scope.StartScope())
                        {
                            // ADO-131294 office365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName的时候会出异常，这个做一个异常处理。(目前这个属性在ListItem Browser中没有使用，如果会出异常不获取即可。)
                            using (scope.StartTry())
                            {
                                context.Load(items);
                                context.Load(items, its => its.Include(tm => tm.DisplayName, tm => tm.ParentList.BaseType, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments));
                            }
                            using (scope.StartCatch())
                            {
                                context.Load(items);
                                context.Load(items, its => its.Include(tm => tm.ParentList.BaseType, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments));
                            }
                        }

                        try
                        {
                            context.ExecuteQuery();
                        }
                        catch (ServerException sEx)
                        {
                            if (IsSPQueryThrottledException(sEx)) throw new AveQueryThrottledException(sEx.Message, sEx);
                            throw;
                        }
                        option.PageInfo = items.ListItemCollectionPosition != null ? items.ListItemCollectionPosition.PagingInfo : null;//items.ListItemCollectionPosition?.PagingInfo;

                        foreach (ListItem item in items)
                        {
                            var itemInfo = GetItemBrowserInfo(parentWeb.ServerRelativeUrl, item);
                            itemBrowserInfos.Add(itemInfo);
                            itemInfo.ParentFolderUniqueID = parentFolderId;
                        }
                    }
                    else
                    {
                        QueryBrowserItemsForLargeList(context, list, option, parentFolderId, parentWeb.ServerRelativeUrl, itemBrowserInfos, camlquery);
                    }
                }
            }
            return itemBrowserInfos;
        }

        internal virtual void SetCamlQueryFolderUrl(CamlQuery camlquery, string folderUrl)
        {
            camlquery.FolderServerRelativeUrl = folderUrl;
        }

        protected virtual void QueryBrowserItemsForLargeList(AveClientContext context, List list, AveBrowserOption option, Guid parentFolderId, string webServerRelativeUrl, List<AveItemBrowserInfo> itemBrowserInfos, CamlQuery query)
        {

        }

        protected bool IsSPQueryThrottledException(ServerException sEx)
        {
            return string.Equals(sEx.ServerErrorTypeName, "Microsoft.SharePoint.SPQueryThrottledException", StringComparison.OrdinalIgnoreCase);
        }


        protected void SetFileBrowserInfos(AveItemBrowserInfo itemInfo, ClientFile file, string webServerRelativeUrl)
        {
            itemInfo.Url = file.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            itemInfo.ParentListID = Guid.Empty;
            itemInfo.Name = file.Name;
            string uniqueId = GetIdsFromEtag(file.ETag)[0];
            if (!string.IsNullOrEmpty(uniqueId))
            {
                itemInfo.UniqueId = new Guid(uniqueId);
            }
            else
            {
                itemInfo.UniqueId = Guid.Empty;
            }
            itemInfo.DisplayName = file.Name;
            itemInfo.ParentFolderUniqueID = Guid.Empty;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        protected AveItemBrowserInfo GetItemBrowserInfo(string webServerRelativeUrl, ListItem item)
        {
            var itemInfo = new AveItemBrowserInfo();
            if (item.FieldValues.ContainsKey("FileRef"))
            {
                string str = item.FieldValues["FileRef"].ToString();
                itemInfo.Url = str.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            itemInfo.ID = item.Id;
            if ((item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
            {
                if ((item.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    itemInfo.Name = item.FieldValues["Title"] as string;
                    if (string.IsNullOrEmpty(itemInfo.Name))
                    {
                        itemInfo.Name = "";
                    }
                }
                else
                {
                    itemInfo.Name = item.FieldValues["FileLeafRef"].ToString();
                }
            }
            else
            {
                itemInfo.Name = item.FieldValues["FileLeafRef"].ToString();
            }
            if (item.FieldValues.ContainsKey("GUID"))
            {
                itemInfo.TpGuid = (Guid)item.FieldValues["GUID"];
            }
            if (item.FieldValues.ContainsKey("_UIVersionString"))
            {
                itemInfo.CurrentUIVersionString = item.FieldValues["_UIVersionString"] as string;
            }
            if (item.FieldValues.ContainsKey("Modified"))
            {
                itemInfo.LastModifyTime = (DateTime)item.FieldValues["Modified"];
            }
            if (item.FieldValues.ContainsKey("Editor"))
            {
                FieldUserValue fieldUserValue = item.FieldValues["Editor"] as FieldUserValue;
                itemInfo.LastModifier = fieldUserValue.LookupId;
            }
            if (item.FieldValues.ContainsKey("_Level") && item.FieldValues["_Level"] != null)
            {
                itemInfo.Level = byte.Parse(item.FieldValues["_Level"].ToString());
            }
            try
            {
                itemInfo.DisplayName = item.DisplayName;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to get DisplayName of item. Message: {0}", ex);
                itemInfo.DisplayName = string.Empty;
            }
            itemInfo.UniqueId = new Guid(item["UniqueId"].ToString());
            itemInfo.ListBaseType = (int)item.ParentList.BaseType;
            itemInfo.ParentListID = item.ParentList.Id;
            itemInfo.HasUniqueRoleAssignments = item.HasUniqueRoleAssignments;
            return itemInfo;
        }

        protected List GetParentList(Web parentWeb, string parentFolderServerRelativeUrl)
        {
            parentFolderServerRelativeUrl = parentFolderServerRelativeUrl.TrimEnd('/');
            if (!parentFolderServerRelativeUrl.StartsWith(parentWeb.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
            {
                parentFolderServerRelativeUrl = parentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + parentFolderServerRelativeUrl.TrimStart('/');
            }
            parentFolderServerRelativeUrl = parentFolderServerRelativeUrl.TrimEnd('/') + '/';
            foreach (List list in parentWeb.Lists)
            {
                if (parentFolderServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
                {
                    return list;
                }
            }
            return null;
        }

        protected void RefreshCredentials(object newCredentials)
        {
            mObj = newCredentials;
            //RefreshContext();
        }


        public virtual void Dispose(bool KeepRequest)
        {
        }

        public virtual string GetWebTemplateConfiguration(string webRelativeUrl)
        {
            return AveWebServiceRequest.GetWebTemplateConfiguration(this.WebAppName, webRelativeUrl, this.mObj);
        }

        internal delegate string GetObjectIdentity();
        internal delegate long GetPathId();
        internal delegate void SetObjectPathId(long id);

        internal object GetObjectPathString(ObjectPath path)
        {
            string identity = string.Empty;
            long id = 0;
            Assembly assembly = typeof(ObjectPath).Assembly;
            Type objectPathIdentity = assembly.GetType("Microsoft.SharePoint.Client.ObjectPathIdentity", false, true);
            if (path.GetType().Equals(objectPathIdentity))
            {
                BindingFlags flags = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.SetProperty
                    | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
                MethodInfo getIdentityMethod = objectPathIdentity.GetProperty("Identity", flags).GetGetMethod(true);
                GetObjectIdentity getIdentity = Delegate.CreateDelegate(typeof(GetObjectIdentity), path, getIdentityMethod) as GetObjectIdentity;
                identity = getIdentity();
                MethodInfo getIdMethod = objectPathIdentity.GetProperty("Id", flags).GetGetMethod(true);
                GetPathId getId = Delegate.CreateDelegate(typeof(GetPathId), path, getIdMethod) as GetPathId;
                id = getId();
                if (string.IsNullOrEmpty(identity) || id <= 0)
                {
                    return string.Empty;
                }
                return identity + "#" + id.ToString();
            }
            return path;
        }

        internal ObjectPath GetObjectPathByIdentity(object objectPath)
        {
            object path = null;
            if ((objectPath as ObjectPath) != null)
            {
                return objectPath as ObjectPath;
            }
            if (string.IsNullOrEmpty(objectPath.ToString()))
            {
                return null;
            }
            string[] splitStrings = objectPath.ToString().Split(new char[] { '#' });
            if (string.IsNullOrEmpty(splitStrings[0]) || string.IsNullOrEmpty(splitStrings[1]))
            {
                return null;
            }
            using (AveClientContext context = CreateContext())
            {
                Assembly assembly = typeof(ClientRuntimeContext).Assembly;
                Type objectPathIdentity = assembly.GetType("Microsoft.SharePoint.Client.ObjectPathIdentity", false);
                Type[] types = new Type[] { typeof(ClientRuntimeContext), typeof(string) };
                ConstructorInfo constructor = objectPathIdentity.GetConstructor(types);
                object[] paramaters = new object[] { context, splitStrings[0] };
                path = constructor.Invoke(paramaters);
                BindingFlags flags = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.SetProperty
                    | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
                MethodInfo setPathId = typeof(ObjectPath).GetProperty("Id", flags).GetSetMethod(true);
                SetObjectPathId setId = Delegate.CreateDelegate(typeof(SetObjectPathId), path, setPathId.Name) as SetObjectPathId;
                setId(Convert.ToInt64(splitStrings[1]));
                return path as ObjectPath;
            }
        }

        internal Guid GetFieldIdFromIdentity(string identity)
        {
            Guid fieldId = Guid.Empty;
            if (!string.IsNullOrEmpty(identity))
            {
                int startIndex = identity.IndexOf(":field:", StringComparison.OrdinalIgnoreCase) + 7;
                int endIndex = identity.IndexOf("#", StringComparison.OrdinalIgnoreCase);
                if (startIndex >= endIndex)
                {
                    return fieldId;
                }
                string id = identity.Substring(startIndex, endIndex - startIndex);
                fieldId = new Guid(id);
            }
            return fieldId;
        }

        [Obsolete("Use AveWebServiceNetwork.CheckWikiPage")]
        protected bool CheckWikiPage(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                bool isWikiPage = false;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                ExceptionHandlingScope itemIsNullCondition = new ExceptionHandlingScope(context);
                using (itemIsNullCondition.StartScope())
                {
                    using (itemIsNullCondition.StartTry())
                    {
                        context.Load(file);
                        context.Load(file.ListItemAllFields);
                    }
                    using (itemIsNullCondition.StartCatch())
                    {
                        context.Load(file);
                    }
                }
                context.ExecuteQuery();
                if (file.IsObjectPropertyInstantiated("ListItemAllFields") && file.ListItemAllFields.IsPropertyAvailable("Id"))
                {
                    if (file.ListItemAllFields.FieldValues.Count > 0 &&
                        file.ListItemAllFields.FieldValues.ContainsKey("WikiField") && file.ListItemAllFields.FieldValues["WikiField"] != null)
                    {
                        isWikiPage = true;
                    }
                }
                return isWikiPage;
            }
        }


        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            throw new NotImplementedException();
        }
        public virtual List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }



        public Dictionary<string, object> GetManagedSitecollectionData()
        {
            throw new NotImplementedException();
        }

        public bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "")
        {
            throw new NotImplementedException();
        }

        public virtual string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            try
            {
                if (string.IsNullOrEmpty(CAUrl))
                    return "Cannot create site collection because CA url is empty.";
                mWebServiceRequest.AddSite(CAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
                return string.Empty;
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create site collection, url : {0}, error message : {1}", url, e.ToString());
                return e is ServerException ? "ServerException: " + e.Message : e.Message;
            }
        }

        public virtual int GetListItemRatings(string listItemUrl)
        {
            throw new NotImplementedException();
        }

        public virtual AveRequestAudit GetAuditValues()
        {
            throw new NotImplementedException();
        }

        public virtual void CustomizeReport(Dictionary<string, object> parameters, Guid reportId)
        {
            throw new NotImplementedException();
        }


        public virtual Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteAllWorkflowAasociations(string webUrl, Guid listId, string contentTypeId, string source)
        {
            mLogger.Debug("Delete all workflow association in collection.");
            int retryTimes = 3;
            using (AveClientContext context = CreateContext())
            {
                DeleteWorkflowAasociations(context, webUrl, listId, contentTypeId, source, retryTimes);
            }
        }

        private void DeleteWorkflowAasociations(AveClientContext context, string webUrl, Guid listId, string contentTypeId, string source, int retryTimes)
        {
            WorkflowAssociationCollection associationCollection = GetAssociationCollection(context, webUrl, listId, contentTypeId, source);
            List<Guid> idCollection = new List<Guid>();
            foreach (var asso in associationCollection)
            {
                if (!idCollection.Contains(asso.Id))
                {
                    idCollection.Add(asso.Id);
                }
            }
            if (!DeleteAssociationByIds(associationCollection, context, idCollection) && retryTimes-- > 0)
            {
                DeleteWorkflowAasociations(context, webUrl, listId, contentTypeId, source, retryTimes);
            }
        }

        private bool DeleteAssociationByIds(WorkflowAssociationCollection associationCollection, AveClientContext context, List<Guid> idCollection)
        {
            if (associationCollection == null)
            {
                return true;
            }
            bool success = true;

            idCollection.ForEach(id =>
                {
                    try
                    {
                        var needDeleteWF = associationCollection.GetById(id);
                        needDeleteWF.DeleteObject();
                        context.ExecuteQuery();
                        mLogger.Debug("Delete association success.AssociatioId:{0}", id);
                    }
                    catch (Exception e)
                    {
                        success = false;
                        mLogger.Warn("Delete association failed.AssociatioId:{0},Error:{1}", id, e);
                    }
                });
            return success;
        }

        private WorkflowAssociationCollection GetAssociationCollection(AveClientContext context, string webUrl, Guid listId, string contentTypeId, string source)
        {
            List<Guid> idCollection = new List<Guid>();
            WorkflowAssociationCollection workflowAssociationCollection = null;
            Web web = context.Site.OpenWeb(webUrl);
            List list = null;
            if (listId != Guid.Empty)
            {
                list = web.Lists.GetById(listId);
            }
            switch (source)
            {
                case "web.workflow":
                    workflowAssociationCollection = web.WorkflowAssociations;
                    break;
                case "list.workflow":
                    if (list != null)
                    {
                        workflowAssociationCollection = list.WorkflowAssociations;
                    }
                    break;
                case "contentType.workflow":
                    ContentType contentType = null;
                    if (list != null)
                    {
                        contentType = list.ContentTypes.GetById(contentTypeId);
                    }
                    else
                    {
                        contentType = web.ContentTypes.GetById(contentTypeId);
                    }
                    workflowAssociationCollection = contentType.WorkflowAssociations;
                    break;
            }
            if (workflowAssociationCollection != null)
            {
                context.Load(workflowAssociationCollection, was => was.IncludeWithDefaultProperties(association => association.Id));
                context.ExecuteQuery();
            }
            return workflowAssociationCollection;
        }

        public virtual void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(workflow.ParentWeb.ServerRelativeUrl);
                WorkflowAssociation needDeleteWF = null;
                switch (source)
                {
                    case "web.workflow":
                        needDeleteWF = web.WorkflowAssociations.GetById(workflow.ID);
                        break;
                    case "list.workflow":
                        if (workflow.ParentList != null)
                        {
                            List list = web.Lists.GetById(workflow.ParentList.ID);
                            needDeleteWF = list.WorkflowAssociations.GetById(workflow.ID);
                        }
                        break;
                    case "contentType.workflow":
                        ContentType contentType = null;
                        if (workflow.ParentList != null)
                        {
                            contentType = web.Lists.GetById(workflow.ParentList.ID).ContentTypes.GetById(workflow.ContentTypeId.ToString());
                            needDeleteWF = contentType.WorkflowAssociations.GetById(workflow.ID);
                        }
                        else
                        {
                            contentType = web.ContentTypes.GetById(workflow.ContentTypeId.ToString());
                            needDeleteWF = contentType.WorkflowAssociations.GetById(workflow.ID);
                        }
                        break;
                    default:
                        break;
                }
                try
                {
                    if (needDeleteWF != null)
                    {
                        needDeleteWF.DeleteObject();
                        context.ExecuteQuery();
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Delete workflow failed.Workflow Name:{0},Message:{1}", workflow.Name, ex.ToString());
                }
            }
        }
        public virtual Dictionary<string, object> OperateSolution(string operation, string siteUrl, string webServerRelativeUrl, int id)
        {
            // SolutionCatalog = 0x79
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> solutionProperties = new Dictionary<string, object>();
                List solutionGallery = context.Site.GetCatalog(0x79);
                ListItem solutionItem = solutionGallery.GetItemById(id);
                context.Load(solutionItem);
                context.ExecuteQuery();
                GetItemDic(solutionProperties, solutionItem);
                if (solutionProperties.ContainsKey("Status") && solutionProperties["Status"] != null)
                {
                    solutionProperties["Status"] = (solutionProperties["Status"] as FieldLookupValue).LookupValue;
                }
                return solutionProperties;
            }
        }

        public virtual void ApplyWebTemplate(string webUrl, string webTemplate)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webUrl);
                web.ApplyWebTemplate(webTemplate);
                context.ExecuteQuery();
            }
        }

        public virtual Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public virtual void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            throw new NotImplementedException();
        }

        #region used for openWeb() method

        protected string mCurrentWebUrl { get; private set; }

        public void SetCurrentWebUrl(string currentWebUrl)
        {
            mCurrentWebUrl = currentWebUrl;
        }

        public virtual Dictionary<string, object> OpenCurrentWeb()
        {
            using (AveClientContext context = CreateContext(mCurrentWebUrl))
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                try
                {

                    Web web = context.Web;
                    webProperties = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetWebError, mCurrentWebUrl, e.ToString());
                    throw;
                }
                return webProperties;
            }
        }

        #endregion

        public virtual void PublishSharepointList(string webServerRelativeUrl, IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {

        }


        public virtual bool DeleteMigrationJob(Guid id)
        {
            throw new NotSupportedException();
        }


        public virtual AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            throw new NotSupportedException();
        }


        public virtual Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            throw new NotSupportedException();
        }

        public virtual Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            throw new NotSupportedException();
        }


        public virtual Dictionary<string, object> GetFileById(string webServerRelativeUrl, Guid fileId)
        {
            throw new NotSupportedException();
        }


        public virtual Dictionary<string, object> GetFolderById(string webServerRelativeUrl, Guid folderId)
        {
            throw new NotSupportedException();
        }
        public virtual bool GetSiteExists(string url)
        {
            return false;
        }

        public virtual Dictionary<string, object> GetItemByUrl(Guid webId, string itemUrl, out Guid listId)
        {
            throw new NotImplementedException();
        }

        public virtual AveWebMasterPageInfo GetRootWebMasterPageInfo()
        {
            throw new NotImplementedException();
        }

        public virtual void SetRootWebAndMySiteWebMasterPageInfo(string mySiteWebServerRelativeUrl, AveWebMasterPageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Folder GetFolderByAPI(Web web, string url)
        {
            return web.GetFolderByServerRelativeUrl(url);
        }

        public virtual Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            return folders.Add(url);
        }

        protected virtual ClientFile AddFileByAPI(FileCollection files, FileCreationInformation createInfo)
        {
            return files.Add(createInfo);
        }

        public virtual WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId)
        {
            return null;
        }

        public virtual void RestoreWorkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache)
        {

        }
        internal virtual bool UpdateWebUserResource(Web web, Dictionary<string, object> changeProperties)
        {
            return false;
        }
        internal virtual bool UpdateListUserResource(List list, Dictionary<string, object> changeProperties)
        {
            return false;
        }
        internal virtual bool UpdateContentTypeUserResource(ContentType contentType, Dictionary<string, object> changeProperties)
        {
            return false;
        }
        internal virtual bool UpdateFieldUserResource(Field field, Dictionary<string, object> changeProperties)
        {
            return false;
        }

        public virtual void ApplySiteDesign(string webUrl, Guid siteDesignId)
        {

        }

        public virtual void PostRestoreModernWebpart(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo sourceSitInfo, Func<string, string> GetUserFromMapping)
        {

        }
        public virtual string GetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName)
        {
            return string.Empty;
        }

        public virtual bool RemoveFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName)
        {
            return false;
        }

        public virtual bool SetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName, string value)
        {
            return false;
        }

        protected string GetWebTemplateTitle(string siteUrl, uint language, string templateName, string spVersoin)
        {
            using (AveWebServiceRequest aveWebServiceRequest = new AveWebServiceRequest(siteUrl, mUserAccountInfo, mObj, spVersoin))
            {
                Dictionary<string, object> WebTemplates = aveWebServiceRequest.GetWebTemplates("", language, false, "");
                return GetWebTemplateNameById(templateName, WebTemplates);
            }
        }
        public virtual string GetWebTemplateTitle(string siteUrl, uint language, string templateName)
        {
            return GetWebTemplateTitle(siteUrl, language, templateName, "14");
        }

        #region Remote 13 or above version  Method.
        public virtual string GetServerVersion()
        {
            throw new NotImplementedException();
        }

        public virtual string GetListExperience(string webServerRelativeUrl, Guid guid)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, string experience)
        {
            throw new NotImplementedException();
        }

        public virtual void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            throw new NotImplementedException();
        }

        public virtual void AddSitePolicy(string policySchema, string siteUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetApps(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId)
        {
            throw new NotImplementedException();
        }
        public virtual Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinsh)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId)
        {
            throw new NotImplementedException();
        }

        public virtual Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition)
        {
            throw new NotImplementedException();
        }

        public virtual void PublishDefinition(string webServerRelativeUrl, Guid definitionId)
        {
            throw new NotImplementedException();
        }

        public virtual Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetSiteStorageInfo()
        {
            return new Dictionary<string, object>();
        }

        public virtual DateTime GetUTCToLocalTime(string webServerRelativeUrl, DateTime time)
        {
            throw new NotImplementedException();
        }

        public virtual DateTime GetLocalToUTCTime(string webServerRelativeUrl, DateTime time)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId)
        {
            throw new NotImplementedException();
        }

        public virtual void AddDocumentsetVersion(string webRelativeUrl, string listTitle, int itemId, bool isMajor, string comment)
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> GetAllSiteCollectionsList(string tenantAdminSiteUrl, bool inlcudeOneDriveSite, List<string> excludeTempaltes)
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> GetGroupSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return null;
        }

        public virtual List<Dictionary<string, object>> GetOneDriveSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return null;
        }

        public virtual List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return null;
        }

        public virtual Dictionary<string, object> GetWebAppById(string webServerRelativeUrl, Guid appId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetFieldValueAsTaxonomyFieldValue(string webRelativeUrl, Guid listId, Guid fieldId, string text)
        {
            throw new NotImplementedException();
        }

        public virtual int GetSiteOwnerId()
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetSiteBasicProperties()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames)
        {
            throw new NotImplementedException();
        }

        public virtual SiteStatus GetSiteStatus(string siteUrl, Func<AveBPOSAccountInfo, string, string> GetAdminUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, Dictionary<string, int>> GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, Guid listId, List<string> fieldNameList)
        {
            throw new NotImplementedException();
        }

        public virtual void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            throw new NotImplementedException();
        }

        public virtual Guid PublishNintexWorkflow(Stream stream, string publishName, string webUrl, string listName, Guid parentListId)
        {
            throw new NotImplementedException();
        }

        public virtual Guid PublishNintexWorkflow(string webUrl, Guid workflowDefinitionId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> siteProp)
        {
            throw new NotImplementedException();
        }

        public virtual int GetSiteCollectionsCount(string tenantAdminSiteUrl)
        {
            throw new NotImplementedException();
        }

        public virtual int GetOneDriveCount(List<string> usernames)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota)
        {
            throw new NotImplementedException();
        }

        public virtual string ImportNintexWorkflow(Stream stream, string publishName, string webUrl, string listTitle, Guid parentListId, bool migrate)
        {
            throw new NotImplementedException();
        }

        public virtual AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            throw new NotImplementedException();
        }

        public virtual AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            throw new NotImplementedException();
        }

        public virtual void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId)
        {
            throw new NotImplementedException();
        }

        public virtual void PublishNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            throw new NotImplementedException();
        }

        public virtual Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            throw new NotImplementedException();
        }

        public virtual string ConvertNintexFormJsonObjectToXml(string webUrl, string formJsonData, string fileName)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIDs, string loginName)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, string> GetWebUserResource(string webServerRelativeUrl, string resourceName, List<string> cultureNames)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, string> GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, List<string> cultureNames)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, string> GetFieldUserResource(string webServerRelativeUrl, Guid listId, string resouceName, string fieldResourceName, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProp, List<string> cultureNames)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, string> GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string resouceName, string contentTypeResourceName, string contentTypeId, List<string> cultureNames)
        {
            throw new NotImplementedException();
        }

        public virtual bool GetDenyAddAndCustomizePagesStatus()
        {
            throw new NotImplementedException();
        }

        public virtual void SetDenyAddAndCustomizePagesStatus(bool status)
        {
            throw new NotImplementedException();
        }

        public virtual AveComplianceTagInfo GetListComplianceTagProperties(string webServerRelativeUrl, string listServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public virtual AveComplianceTagInfo UpdateListComplianceTagProperties(string webServerRelativeUrl, string listServerRelativeUrl, AveComplianceTagInfo properties)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetListItemComplianceTag(Guid webID, Guid listID, int rowID)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> SetComplianceTag(Guid webID, Guid listID, int rowID, AveItemComplianceTagInfo complianceSettingInfo)
        {
            throw new NotImplementedException();
        }

        public bool TestProjectLicense()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjects(bool includeDetails)
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectCalendars()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectCustomFields()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectLookupTables()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectEnterpriseProjectTypes()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectEnterpriseResources()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectPhases()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectStages()
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectTasks(Guid projectId, bool isPublished)
        {
            throw new NotImplementedException();
        }

        public virtual List<Dictionary<string, object>> QueryProjectDetailPages()
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> QueryDraftProject(Guid projectId)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetProjectById(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual string ReadServerTimeLine()
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateTimeLineByPSI(string tlViewData)
        {
            throw new NotImplementedException();
        }

        public virtual List<AveProjectDetailPageInfo> GetDetailPages(Guid eptId)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateEnterpriseTypeByPSI(Guid projId, AveProjectEnterpriseProjectTypeInfo eptInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreCalendar(List<AveProjectCalendarInfo> calendarInfos)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreCustomFields(List<AveProjectCustomFieldInfo> customFieldInfos)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreEnterpriseResource(List<AveProjectEnterpriseResourceInfo> resourceInfos)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreLookupTable(List<AveProjectLookupTableInfo> lookupTableInfos)
        {
            throw new NotImplementedException();
        }

        public virtual void RestorePhase(List<AveProjectPhaseInfo> phaseInfos)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreStage(List<AveProjectStageInfo> stageInfos)
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreEnterpriseProjectTypes(List<AveProjectEnterpriseProjectTypeInfo> eptInfos)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> RestoreProject(AveProjectInfo info, AveProjectReader projectDetails, AveProjectConfig projectConfig, AveRestoreMode option)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddLookupTable(AveProjectLookupTableInfo lookupTableInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddCustomField(AveProjectCustomFieldInfo customFieldInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddEnterpriseType(AveProjectEnterpriseProjectTypeInfo eptInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddEnterpriseResource(AveProjectEnterpriseResourceInfo resourceInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddStage(AveProjectStageInfo stageInfo)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> AddPhase(AveProjectPhaseInfo phaseInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteProject(Guid id, string siteUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdateLookupTable(Guid id, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdateCustomField(Guid id, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdateEnterpriseProjectType(Guid id, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdateEnterpriseResource(Guid id, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdateStage(Guid id, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> UpdatePhase(Guid id, Dictionary<string, object> updateProp)
        {
            throw new NotImplementedException();
        }

        public virtual List<AveComplianceTagInfo> GetAvailableTagsForSite(string siteUrl)
        {
            throw new NotImplementedException();
        }

        public virtual void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}


