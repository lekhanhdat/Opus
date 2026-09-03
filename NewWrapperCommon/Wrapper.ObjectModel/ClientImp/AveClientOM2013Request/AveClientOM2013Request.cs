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
using Microsoft.SharePoint.Client.UserProfiles;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.WebParts;
using Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint.Client.Publishing.Navigation;
using Microsoft.SharePoint.Client.DocumentSet;
using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
using ClientFile = Microsoft.SharePoint.Client.File;
using ClientFolder = Microsoft.SharePoint.Client.Folder;
using SPChangeType = Microsoft.SharePoint.Client.ChangeType;
using Microsoft.SharePoint.Client.Social;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Script.Serialization;
using Microsoft.SharePoint.Client.WorkflowServices;
using System.Diagnostics;
using Microsoft.SharePoint.ApplicationPages.ClientPickerQuery;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.ClientOM
{
    [AveCodeReview("2012/11/15", "cbi@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_4 }, "ADO-53377", true)]
    public class AveClientOM2013Request : AveClientOMRequest, IAveRequest, IDisposable
    {
        protected static readonly string mUnauthorizedMessage = "The remote server returned an error: (401) Unauthorized.";
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOM2013Request));
        //protected AveWebServiceRequest mWebServiceRequest;
        protected IAveHttpWebRequestCommon mRequestCommon;
        protected int mCompatibilityLevel = 0;
        //private AveHttpWebRequestCommon mRequestCommon;  
        protected FormDigestProvider provider;

        public AveClientOM2013Request(string url, AveBPOSAccountInfo userAccountInfo, object obj, string serverVersion)
            : base(url, userAccountInfo, obj, serverVersion)
        {
            Type = AveClientRequestType.AveClientOM2013Request;
            provider = new FormDigestProvider();
            mRequestCommon = new AveHttpWebRequestCommon2013(mWebUrl, mObj, mServerVersion);
            //mWebServiceRequest = new AveWebServiceRequest(url, userAccountInfo, obj, mServerVersion, mSiteTrimObj);
            //mRequestCommon = new AveHttpWebRequestCommon(this.mWebUrl, obj, serverVersion);            
        }

        #region Get
        public override AveRequestAudit GetAuditValues()
        {
            try
            {
                return mRequestCommon.GetRequestAudit();
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get site audit flags failed. Message:{0}", ex);
            }
            return new AveRequestAudit();
        }

        public override Dictionary<string, object> GetSite()
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    LoadSite(context);
                    LoadWeb(context.Site.RootWeb, context);
                    context.ExecuteQuery();

                    this.mCompatibilityLevel = context.Site.CompatibilityLevel;
                    if (mCompatibilityLevel == 15)
                    {
                        mRequestCommon = new AveHttpWebRequestCommon2013(mWebUrl, mObj, mServerVersion);
                    }
                    else
                    {
                        mRequestCommon = new AveHttpWebRequestCommon2010(mWebUrl, mObj, mServerVersion);
                    }

                    this.maxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                    CopyProperty(siteProperties, context.Site);
                    siteProperties["Usage"] = AssemblyUsageInfo(context.Site.Usage);
                    Dictionary<string, object> rootWebProperties = GetWebProperties(context, context.Site.RootWeb, mWebUrl, context.Site.ServerRelativeUrl, true);
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    if (context.Site.IsObjectPropertyInstantiated("Owner") && context.Site.Owner.IsPropertyAvailable("Id"))
                    {
                        siteProperties["Owner" + AveObjectModelConstant.ObjectPropertySuffix] = context.Site.Owner.Id;
                    }
                    //siteProperties.Add("SyndicationEnabled", context.Site.RootWeb.SyndicationEnabled);
                    siteProperties["IsMoss"] = false;
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

        protected void LoadSite(ClientContext context)
        {
            ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
            using (exceptionScope.StartScope())
            {
                using (exceptionScope.StartTry())
                {
                    context.Load(context.Site);
                    context.Load(context.Site, site => site.Usage);
                    context.Load(context.Site.Owner);
                }
                using (exceptionScope.StartCatch())
                {
                    context.Load(context.Site);
                    context.Load(context.Site, site => site.Usage);
                }
            }
        }

        protected override void LoadWeb(Web web, ClientContext context)
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
            context.Load(web, w => w.AllProperties);// w => w.ListTemplates,
            context.Load(web, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch);
            context.Load(web, w => w.AllowDesignerForCurrentUser, w => w.HasUniqueRoleAssignments);
            context.Load(web, w => w.SupportedUILanguageIds);
        }

        public override Site GetSiteById(Guid siteId)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = null;
                if (context.Site.Id.Equals(siteId))
                {
                    site = context.Site;
                }
                return site;
            }
        }

        protected virtual void LoadAppsInfo(AveClientContext context, ClientObjectList<AppInstance> apps, Web web)
        {
            context.Load(apps);
        }

        public override Dictionary<string, object> GetApps(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> appsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> appPropertyList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientObjectList<AppInstance> apps = AppCatalog.GetAppInstances(context, web);
                LoadAppsInfo(context, apps, web);
                context.ExecuteQuery();
                AssembleAppsProperties(webServerRelativeUrl, web, apps, appPropertyList);
                appsProperties[AveObjectModelConstant.ChildrenProperties] = appPropertyList;
                return appsProperties;
            }
        }

        protected virtual void AssembleAppsProperties(string webServerRelativeUrl, Web web, ClientObjectList<AppInstance> apps, List<Dictionary<string, object>> appPropertyList)
        {
            if (apps.Count > 0)
            {
                List<Dictionary<string, object>> appsMetadata = GetInstalledApps(webServerRelativeUrl);

                foreach (AppInstance app in apps)
                {
                    Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                    CopyProperty(appInstanceProperties, app);
                    if (!string.IsNullOrEmpty(app.AppWebFullUrl))
                    {
                        appInstanceProperties["AppWebFullUrl"] = new Uri(app.AppWebFullUrl);
                    }
                    Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, app.Id);
                    if (appMetadata == null)
                    {
                        mLogger.Debug(string.Format("Can not find app in the AppCatalog with Id:{0}.", app.Id));
                        continue;
                    }

                    appInstanceProperties["App"] = AssembleAppProperties(appMetadata);

                    appPropertyList.Add(appInstanceProperties);
                }
            }
        }

        public override Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> appsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> appPropertyList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientObjectList<AppInstance> apps = web.GetAppInstancesByProductId(productId);
                LoadAppsInfo(context, apps, web);
                context.ExecuteQuery();
                AssembleAppsProperties(webServerRelativeUrl, web, apps, appPropertyList);

                appsProperties[AveObjectModelConstant.ChildrenProperties] = appPropertyList;
                return appsProperties;
            }
        }
        public override Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinish)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                var appInstance = web.GetAppInstanceById(instanceId);
                var result = appInstance.Uninstall();
                context.ExecuteQuery();
                if (waitUninstallFinish)
                {
                    WaitUntilUninstallFinish(context, web, productId);
                }
                return result.Value;
            }
        }
        private void WaitUntilUninstallFinish(AveClientContext context, Web web, Guid productId)
        {
            int retryCount = 0;
            while (true)
            {
                if (!GetAppStatus(context, web, productId))
                {
                    break;
                }
                if (retryCount++ > WrapperConfiguration.CheckAppInstanceInstalledTime)
                {
                    throw new TimeoutException("time out when uninstalling app");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        private bool GetAppStatus(AveClientContext context, Web web, Guid productId)
        {
            bool exist = false;
            var apps = web.GetAppInstancesByProductId(productId);
            context.Load(apps);
            context.ExecuteQuery();
            if (apps.Count > 0)
            {
                exist = true;
            }
            return exist;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint URL")]
        protected virtual List<Dictionary<string, object>> GetInstalledApps(string webServerRelativeUrl)
        {
            string getAppsUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/15/addanapp.aspx?task=GetMyApps&sort=1&query=&myappscatalog=1&ci=1&vd=1";
            string jasonResponse = AveHttpWebRequestUtility.HttpGet(getAppsUrl, this.mObj);
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> appsMetadata = jsSerializer.Deserialize<List<Dictionary<string, object>>>(jasonResponse);
            return appsMetadata;
        }

        protected Dictionary<string, object> GetAppPropertiesById(IList<Dictionary<string, object>> appsMetadata, Guid appId)
        {
            return appsMetadata.FirstOrDefault(
                 (appMetadata) => appMetadata.ContainsKey("Instance")
                    && new Guid((appMetadata["Instance"] as Dictionary<string, object>)["ID"] as string) == appId);
        }

        protected Dictionary<string, object> AssembleAppProperties(Dictionary<string, object> appMetadata)
        {
            Dictionary<string, object> appProperties = new Dictionary<string, object>();
            appProperties["ProductId"] = new Guid(appMetadata["ProductId"] as string);
            switch (Convert.ToInt32(appMetadata["Catalog"]))
            {
                case 0:
                    appProperties["Source"] = AveAppSource.Marketplace;
                    break;
                case 1:
                    appProperties["Source"] = AveAppSource.CorporateCatalog;
                    break;
                case 3:
                    appProperties["Source"] = AveAppSource.ObjectModel;
                    break;
                default:
                    appProperties["Source"] = AveAppSource.InvalidSource;
                    break;
            }
            return appProperties;
        }

        public override Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null)
        {
            if (string.IsNullOrEmpty(webServerRelativeUrl))
            {
                return base.GetRecycleBin();
            }
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> recycleBin = new Dictionary<string, object>();
                try
                {
                    RecycleBinItemCollection binItems = context.Site.OpenWeb(webServerRelativeUrl).RecycleBin;
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
                    AssembleRecycleBinProperties(binItems, recycleBin);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetRecycleBinError, context.Url, e.ToString());
                    throw;
                }
                return recycleBin;
            }
        }

        protected Dictionary<string, object> GetNavigation(string webServerRelativeUrl, Dictionary<string, object> nodesProp)
        {
            using (ClientContext context = CreateContext())
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
                quickLaunchProperties[AveObjectModelConstant.ChildrenProperties] = quickLaunchList;
                Dictionary<string, object> topNavigationBarProperties = new Dictionary<string, object>();
                topNavigationBarProperties[AveObjectModelConstant.ChildrenProperties] = topNavigationBarList;

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

        private static String TrimFolderUrl(string webServerRelativeUrl, string folderServerRelativeUrl)
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

        public override Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> webTemplatesProperties = new Dictionary<string, object>();
                WebTemplateCollection templates = context.Site.GetWebTemplates(lcid, 0);
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

        public virtual Dictionary<string, object> GetItemVersionsForBrowser(string webServerRelativeUrl, string listId, int itemId, Dictionary<string, string> fields)
        {
            return mWebServiceRequest.GetItemVersions(webServerRelativeUrl, listId, itemId, fields);
        }
        public override Dictionary<string, object> GetUser(int id)
        {
            using (ClientContext context = CreateContext())
            {
                try
                {
                    var userInfo = new Dictionary<string, object>();
                    var user = context.Site.RootWeb.SiteUsers.GetById(id);
                    context.Load(user);
                    context.ExecuteQuery();
                    CopyProperty(userInfo, user);
                    return userInfo;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get user failed. Site: {0}, Id: {1}, Error: {2}", mWebUrl, id, e);
                    return null;
                }
            }
        }
        public override Dictionary<string, object> GetUser(string userEmail)
        {
            using (ClientContext context = CreateContext())
            {
                try
                {
                    var userInfo = new Dictionary<string, object>();
                    var user = context.Site.RootWeb.SiteUsers.GetByEmail(userEmail);
                    context.Load(user);
                    context.ExecuteQuery();
                    CopyProperty(userInfo, user);
                    return userInfo;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get user failed. Site: {0}, Email: {1}, Error: {2}", mWebUrl, userEmail, e);
                    return null;
                }
            }
        }

        public override Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, int itemId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> attachmentCollection = new Dictionary<string, object>();
                List<Dictionary<string, object>> attachmentPropertiesList = new List<Dictionary<string, object>>();
                try
                {
                    Web web = context.Site.OpenWeb(webRelativeUrl);
                    List list = web.Lists.GetByTitle(listTitle);
                    ListItem item = list.GetItemById(itemId);
                    AttachmentCollection attachments = item.AttachmentFiles;
                    context.Load(list.RootFolder, f => f.ServerRelativeUrl);
                    context.Load(attachments);
                    context.ExecuteQuery();
                    foreach (Attachment attachment in attachments)
                    {
                        Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
                        AssembleAttachmentProperties(attachment, attachmentProperties);
                        attachmentPropertiesList.Add(attachmentProperties);
                    }
                    attachmentCollection["UrlPrefix"] = this.WebAppName.TrimEnd('/') + list.RootFolder.ServerRelativeUrl + "/Attachments/" + itemId + "/";
                }
                catch (Exception e)
                {
                    mLogger.Warn("failed to get attachments due to: {0}", e.ToString());
                }
                attachmentCollection.Add(AveObjectModelConstant.ChildrenProperties, attachmentPropertiesList);
                return attachmentCollection;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "obj is a key")]
        public override void AssembleSystemFolderProperties(AveClientContext context, Dictionary<string, object> listItemProperty, Folder folder, string webServerRelativeUrl)
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
            listItemProperty["DirName"] = folder.ServerRelativeUrl.Contains('/') ? folder.ServerRelativeUrl.Substring(0, folder.ServerRelativeUrl.LastIndexOf('/')) : folder.Name;//listItem.FieldValues["FileDirRef"].ToString().TrimStart('/');
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
            listItemProperty["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = folder.Properties.FieldValues;
        }
        internal void AssembleAttachmentProperties(Attachment attachment, Dictionary<string, object> attachmentProperties)
        {
            CopyProperty(attachmentProperties, attachment);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "gs is a part of variable")]
        public override Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName)
        {
            if (groupColSource.Equals("web.siteGroups"))
            {
                using (ClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webRelativeUrl);
                    context.Load(web.SiteGroups, gs => gs.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                    context.ExecuteQuery();
                    Dictionary<string, object> groups = new Dictionary<string, object>();
                    List<Dictionary<string, object>> groupList = new List<Dictionary<string, object>>();
                    foreach (Group group in web.SiteGroups)
                    {
                        Dictionary<string, object> groupProp = GetGroupProperties(base.mSiteTrimObj, context, group, true);
                        groupList.Add(groupProp);
                    }
                    groups.Add(AveObjectModelConstant.ChildrenProperties, groupList);
                    return groups;
                }
            }
            else
            {
                return mWebServiceRequest.GetGroups(webRelativeUrl, groupColSource, loginName);
            }
        }

        public override Dictionary<string, object> GetEventReceiverDefinitions(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiversInfo = new Dictionary<string, object>();
                List<Dictionary<string, object>> lists = new List<Dictionary<string, object>>();
                EventReceiverDefinitionCollection eventReceivers;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                if (string.Equals(eventReceiverDefSource, "list.eventReceivers"))
                {
                    List list = null;
                    if (listId != Guid.Empty)
                    {
                        list = web.Lists.GetById(listId);
                    }
                    else
                    {
                        list = web.Lists.GetByTitle(listTitle);
                    }
                    eventReceivers = list.EventReceivers;
                }
                else
                {
                    eventReceivers = web.EventReceivers;
                }
                context.Load(eventReceivers);
                context.ExecuteQuery();
                foreach (EventReceiverDefinition eventReceiver in eventReceivers)
                {
                    Dictionary<string, object> eventReceiverInfo = new Dictionary<string, object>();
                    eventReceiverInfo["Assembly"] = eventReceiver.ReceiverAssembly;
                    eventReceiverInfo["Class"] = eventReceiver.ReceiverClass;
                    eventReceiverInfo["Name"] = eventReceiver.ReceiverName;
                    eventReceiverInfo["Id"] = eventReceiver.ReceiverId;
                    eventReceiverInfo["Type"] = eventReceiver.EventType;
                    lists.Add(eventReceiverInfo);
                }
                eventReceiversInfo.Add(AveObjectModelConstant.ChildrenProperties, lists);
                return eventReceiversInfo;
            }

        }
        public override Dictionary<string, object> GetSiteEventReceiverDefinitions(string siteServerRelativeUrl, string eventReceiverDefSource)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiversInfo = new Dictionary<string, object>();
                List<Dictionary<string, object>> lists = new List<Dictionary<string, object>>();
                Site site = context.Site;
                EventReceiverDefinitionCollection eventReceivers = site.EventReceivers;
                context.Load(eventReceivers);
                context.ExecuteQuery();
                foreach (EventReceiverDefinition eventReceiver in eventReceivers)
                {
                    Dictionary<string, object> eventReceiverInfo = new Dictionary<string, object>();
                    eventReceiverInfo["Assembly"] = eventReceiver.ReceiverAssembly;
                    eventReceiverInfo["Class"] = eventReceiver.ReceiverClass;
                    eventReceiverInfo["Name"] = eventReceiver.ReceiverName;
                    eventReceiverInfo["Id"] = eventReceiver.ReceiverId;
                    eventReceiverInfo["Type"] = eventReceiver.EventType;
                    lists.Add(eventReceiverInfo);
                }
                eventReceiversInfo.Add(AveObjectModelConstant.ChildrenProperties, lists);
                return eventReceiversInfo;
            }
        }
        public override Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties)
        {
            using (ClientContext context = CreateContext())
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
                            navigationProperty["IsExternal"] = !navigation.Url.StartsWith(this.WebAppName.TrimEnd('/') + "/" + this.mSiteRelativeUrl.TrimStart('/') + "/", StringComparison.OrdinalIgnoreCase);
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

        public override Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, string appWebFulUrl = null)
        {
            //using (ClientContext context = CreateContext())
            //{
            //    Dictionary<string, object> webpartManagerProperties = new Dictionary<string, object>();
            //    Web web = context.Site.OpenWeb(webServerRelativeUrl);
            //    ClientFile file = web.GetFileByServerRelativeUrl(fileServerRelativeUrl);
            //    LimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager((PersonalizationScope)personalizationScope);
            //    context.Load(limitedWebPartManager, lwp => lwp.WebParts.IncludeWithDefaultProperties(wpd => wpd.WebPart));
            //    context.ExecuteQuery();
            //    AssembleWebPartManagerProperties(webpartManagerProperties, limitedWebPartManager);
            //    return webpartManagerProperties;
            //}
            return mWebServiceRequest.GetLimitedWebPartManager(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope, appWebFulUrl);
        }

        public override Stream OpenBinaryDirect(ClientRuntimeContext context, string serverRelativeUrl, object obj)
        {
            try
            {
                ClientFile file = (context as AveClientContext).Web.GetFileByServerRelativeUrl(serverRelativeUrl);
                ClientResult<Stream> fileStream = file.OpenBinaryStream();
                context.ExecuteQuery();
                return fileStream.Value;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get file stream failed.Error:{0}", ex);
                return null;
            }
        }

        public override Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string input, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff)
        {
            using (AveClientContext context = CreateContext())
            {
                //O365 need support external user
                ClientResult<string> UserInfos = ResolveUser(context, input, scopes, sources);
                context.ExecuteQuery();
                return !string.IsNullOrEmpty(UserInfos.Value)
                    ? AssemblePrincipalInfo(input, String.Format("[{0}]", UserInfos.Value), ignoreDomainDiff)
                    : new Dictionary<string, object>();
            }
        }

        private ClientResult<string> ResolveUser(ClientContext context, string input, int scopes, int sources)
        {
            ClientPeoplePickerQueryParameters searchParams = new ClientPeoplePickerQueryParameters()
            {
                AllowEmailAddresses = true,
                AllowMultipleEntities = true,
                QueryString = input,
                Required = true,
                PrincipalType = (PrincipalType)scopes,
                PrincipalSource = (PrincipalSource)sources,
                MaximumEntitySuggestions = 30
            };
            return ClientPeoplePickerWebServiceInterface.ClientPeoplePickerResolveUser(context, searchParams);
        }
        private Dictionary<string, object> AssemblePrincipalInfo(string searchName, string userInfos, bool ignoreDomainDiff)
        {
            List<Dictionary<string, object>> infoList = ResolveUsersFromJson(userInfos);
            if (!ignoreDomainDiff)
            {
                infoList = RemoveNotMatchPrincipalInfo(infoList, searchName);
            }
            return infoList.Count > 0 ? infoList[0] : new Dictionary<string, object>();
        }

        private List<Dictionary<string, object>> RemoveNotMatchPrincipalInfo(List<Dictionary<string, object>> infoList, string searchName)
        {
            List<Dictionary<string, object>> principalInfos = new List<Dictionary<string, object>>();
            foreach (var principalInfo in infoList)
            {
                //Office365 ExternalUser also need use Email to check 
                if (principalInfo.ContainsKey("LoginName")
                 && principalInfo["LoginName"].ToString().IndexOf(searchName, StringComparison.OrdinalIgnoreCase) < 0
                 && principalInfo.ContainsKey("Email")
                 && principalInfo["Email"].ToString().IndexOf(searchName, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                principalInfos.Add(principalInfo);
            }
            return principalInfos;
        }

        private List<Dictionary<string, object>> ResolveUsersFromJson(string jsonData)
        {
            List<Dictionary<string, object>> infoList = new List<Dictionary<string, object>>();
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> jsonObj = jsSerializer.Deserialize<List<Dictionary<string, object>>>(jsonData);
            if (jsonObj != null)
            {
                foreach (Dictionary<string, object> obj in jsonObj)
                {
                    Dictionary<string, object> userProp = new Dictionary<string, object>();
                    if (obj.ContainsKey("MultipleMatches") && (obj["MultipleMatches"] as ArrayList) != null && (obj["MultipleMatches"] as ArrayList).Count > 0)
                    {
                        Array multipleObj = (obj["MultipleMatches"] as ArrayList).ToArray(typeof(Dictionary<string, object>));
                        foreach (Dictionary<string, object> dir in multipleObj)
                        {
                            userProp = AssembleUserProperties(dir);
                            if (userProp != null)
                            {
                                infoList.Add(userProp);
                            }
                        }
                        return infoList;
                    }
                    userProp = AssembleUserProperties(obj);
                    if (userProp != null)
                    {
                        infoList.Add(userProp);
                    }
                }
            }
            return infoList;
        }

        private Dictionary<string, object> AssembleUserProperties(Dictionary<string, object> originalData)
        {
            if (!originalData.ContainsKey("ProviderName"))//not exist user and group
            {
                return null;
            }
            if (originalData.ContainsKey("EntityData"))
            {
                if ((originalData["EntityData"] as Dictionary<string, object>).ContainsKey("PrincipalType"))
                {
                    if ((originalData["EntityData"] as Dictionary<string, object>)["PrincipalType"].ToString().Contains("UNVALIDATED"))
                    {
                        return null;
                    }
                }
                else if (originalData.ContainsKey("IsResolved") && !Convert.ToBoolean(originalData["IsResolved"]))
                {
                    return null;
                }
            }//not exist email address
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            if (originalData.ContainsKey("Key"))
            {
                infoDic["LoginName"] = originalData["Key"].ToString();
            }
            infoDic["DisplayName"] = originalData.ContainsKey("DisplayText") ? originalData["DisplayText"] : string.Empty;
            if (originalData.ContainsKey("EntityType"))
            {
                if (originalData["EntityType"].ToString().Equals("User"))
                {
                    infoDic.Add("PrincipalType", AvePrincipalType.User);
                }
                else if (originalData["EntityType"].ToString().Equals("FormsRole"))
                {
                    infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                }
                else if (originalData["EntityType"].ToString().Equals("SecGroup"))
                {
                    infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                }
                infoDic["PrincipalId"] = -1;
            }
            if (originalData.ContainsKey("EntityData"))
            {
                Dictionary<string, object> ed = originalData["EntityData"] as Dictionary<string, object>;
                if (ed.ContainsKey("PrincipalType") && !infoDic.ContainsKey("PrincipalType"))
                {
                    switch (ed["PrincipalType"].ToString())
                    {
                        case "SecurityGroup":
                            infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                            break;
                        case "SharePointGroup":
                            infoDic.Add("PrincipalType", AvePrincipalType.SharePointGroup);
                            break;
                        case "DistributionList":
                            infoDic.Add("PrincipalType", AvePrincipalType.DistributionList);
                            break;
                        case "User":
                            infoDic.Add("PrincipalType", AvePrincipalType.User);
                            break;
                        default:
                            infoDic.Add("PrincipalType", AvePrincipalType.None);
                            break;
                    }
                }
                if (ed.ContainsKey("Title"))
                {
                    infoDic.Add("JobTitle", ed["Title"]);
                }
                if (ed.ContainsKey("MobilePhone"))
                {
                    infoDic.Add("Mobile", ed["MobilePhone"]);
                }
                if (ed.ContainsKey("Email"))
                {
                    infoDic["Email"] = ed["Email"];
                }
                if (ed.ContainsKey("Department"))
                {
                    infoDic.Add("Department", ed["Department"]);
                }
            }
            return infoDic;
        }

        public override Dictionary<string, object> GetTaxonomySession()
        {
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                context.Load(session);
                TermStore keyTermStore = session.GetDefaultKeywordsTermStore();
                context.Load(keyTermStore);
                context.Load(keyTermStore, k => k.KeywordsTermSet);
                TermStore sitecollectionTermStore = session.GetDefaultSiteCollectionTermStore();
                context.Load(sitecollectionTermStore);
                context.ExecuteQuery();
                Dictionary<string, object> sessionProp = new Dictionary<string, object>();
                AveObjectCopy.GetObjectBasicProperties(sessionProp, session);

                if (keyTermStore.ServerObjectIsNull != null && keyTermStore.ServerObjectIsNull != true)
                {
                    Dictionary<string, object> keyTermStoreProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(keyTermStoreProp, keyTermStore);
                    Dictionary<string, object> keywordsTermSetProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(keywordsTermSetProp, keyTermStore.KeywordsTermSet);
                    keyTermStoreProp["KeywordsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] = keywordsTermSetProp;
                    sessionProp["DefaultKeywordsTermStore" + AveObjectModelConstant.ObjectPropertySuffix] = keyTermStoreProp;
                }
                if (sitecollectionTermStore.ServerObjectIsNull != null && sitecollectionTermStore.ServerObjectIsNull != true)
                {
                    Dictionary<string, object> sitecollectionTermStoreProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(sitecollectionTermStoreProp, sitecollectionTermStore);
                    sessionProp["DefaultSiteCollectionTermStore" + AveObjectModelConstant.ObjectPropertySuffix] = sitecollectionTermStoreProp;
                }
                return sessionProp;
            }
        }
        public override Dictionary<string, object> GetTermStores()
        {
            Dictionary<string, object> termStoresProp = new Dictionary<string, object>();
            List<Dictionary<string, object>> termStoresList = new List<Dictionary<string, object>>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                context.Load(session.TermStores);
                context.ExecuteQuery();
                foreach (TermStore store in session.TermStores)
                {
                    List<int> languages = new List<int>();
                    Dictionary<string, object> storeProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(storeProp, store);
                    foreach (int language in store.Languages)
                    {
                        languages.Add(language);
                    }
                    storeProp["Languages" + AveObjectModelConstant.ObjectPropertySuffix] = languages;
                    context.Load(store.OrphanedTermsTermSet);
                    context.Load(store.KeywordsTermSet);
                    context.Load(store.HashTagsTermSet);
                    context.ExecuteQuery();
                    Dictionary<string, object> OrphanedTermsTermSetProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(OrphanedTermsTermSetProp, store.OrphanedTermsTermSet);
                    storeProp["OrphanedTermsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] = OrphanedTermsTermSetProp;
                    Dictionary<string, object> keyWordsTermSetProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(keyWordsTermSetProp, store.KeywordsTermSet);
                    storeProp["KeywordsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] = keyWordsTermSetProp;
                    Dictionary<string, object> hashTagsTermSetProp = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(hashTagsTermSetProp, store.HashTagsTermSet);
                    storeProp["HashTagsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] = hashTagsTermSetProp;
                    termStoresList.Add(storeProp);
                }
            }
            termStoresProp[AveObjectModelConstant.ChildrenProperties] = termStoresList;
            return termStoresProp;
        }
        public override Dictionary<string, object> GetTaxonomyGroups(Guid guid)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> groupsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> groupsList = new List<Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(guid);
                    context.Load(store.Groups);
                    context.ExecuteQuery();
                    foreach (TermGroup group in store.Groups)
                    {
                        Dictionary<string, object> groupProp = new Dictionary<string, object>();
                        AveObjectCopy.GetObjectBasicProperties(groupProp, group);
                        groupsList.Add(groupProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermGroups Failed, error message:{0}", e.ToString());
                }
                groupsProp[AveObjectModelConstant.ChildrenProperties] = groupsList;
                return groupsProp;
            }
        }
        public override Dictionary<string, object> GetTermSets(Guid termStoreId, Guid groupId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termSetsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> termSetsList = new List<Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermGroup group = store.Groups.GetById(groupId);
                    context.Load(group.TermSets);
                    context.ExecuteQuery();
                    foreach (TermSet set in group.TermSets)
                    {
                        Dictionary<string, object> setProp = new Dictionary<string, object>();
                        CopyProperty(setProp, set);
                        #region 外层接口是<int,string>类型, 在这里转换下。
                        object nameProperty;
                        var names = new Dictionary<int, string>();
                        if (setProp.TryGetValue("Names", out nameProperty))
                        {
                            setProp.Remove("Names");
                            if (nameProperty is Dictionary<string, string>)
                            {
                                foreach (var namePair in nameProperty as Dictionary<string, string>)
                                {
                                    int culture;
                                    if (int.TryParse(namePair.Key, out culture))
                                    {
                                        names.Add(culture, namePair.Value);
                                    }
                                }
                            }
                        }
                        setProp["Names"] = names;
                        #endregion

                        termSetsList.Add(setProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermSets Failed, groupId: {0},error message:{1}", groupId, e.ToString());
                }
                termSetsProp[AveObjectModelConstant.ChildrenProperties] = termSetsList;
                return termSetsProp;
            }
        }

        public override Dictionary<string, object> GetTerms(Guid termStoreId, Guid groupId, Guid termSetId, Guid parentTermId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> termsList = new List<Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermGroup group = store.Groups.GetById(groupId);
                    TermSet set = group.TermSets.GetById(termSetId);
                    TermCollection terms;
                    if (parentTermId.Equals(Guid.Empty))
                    {
                        terms = set.Terms;
                    }
                    else
                    {
                        terms = set.GetTerm(parentTermId).Terms;
                    }
                    context.Load(terms, termCollection => termCollection.IncludeWithDefaultProperties(t => t.Parent.Id, t => t.PinSourceTermSet, t => t.Labels.IncludeWithDefaultProperties()));
                    context.ExecuteQuery();
                    foreach (Term term in terms)
                    {
                        Dictionary<string, object> termProp = new Dictionary<string, object>();
                        CopyProperty(termProp, term);
                        //AveObjectCopy.GetObjectBasicProperties(termProp, term);
                        termProp["ParentTermId"] = term.Parent.IsPropertyAvailable("Id") ? term.Parent.Id : Guid.Empty;
                        try
                        {
                            if (!(term.PinSourceTermSet.ServerObjectIsNull.HasValue && term.PinSourceTermSet.ServerObjectIsNull.Value))
                            {
                                termProp["PinSourceTermSetId"] = term.PinSourceTermSet.Id;
                            }
                        }
                        catch (ServerObjectNullReferenceException e)
                        {
                            mLogger.Debug("Term does not has pin source term set. Error message: {0}", e.ToString());
                        }
                        termProp.Add("tempLabels", GetLables(term));

                        termsList.Add(termProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get Terms Failed, groupId: {0},termSetId: {1},error message:{2}", groupId, termSetId, e);
                }
                termsProp[AveObjectModelConstant.ChildrenProperties] = termsList;
                return termsProp;
            }
        }

        private Dictionary<string, object> GetLables(Term term)
        {
            List<Dictionary<string, object>> lableList = new List<Dictionary<string, object>>();
            foreach (Label label in term.Labels)
            {
                Dictionary<string, object> labelProperties = new Dictionary<string, object>();
                CopyProperty(labelProperties, label);
                lableList.Add(labelProperties);
            }
            Dictionary<string, object> labelsProp = new Dictionary<string, object>();
            labelsProp.Add("Labels" + AveObjectModelConstant.ObjectPropertySuffix, lableList);
            return labelsProp;
        }

        public override Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId)
        {
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermSet set = store.GetTermSet(termSetId);
                Term term = set.GetTerm(parentTermId);
                context.Load(term.Labels);
                context.ExecuteQuery();
                return GetLables(term);
            }
        }

        public override string GetDescription(Guid termStoreId, Guid termSetId, Guid parentTermId, int lcid)
        {
            string description = string.Empty;
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermSet set = store.GetTermSet(termSetId);
                Term term = set.GetTerm(parentTermId);
                try
                {
                    var des = term.GetDescription(lcid);
                    context.ExecuteQuery();
                    description = des.Value;
                }
                catch (Exception e)
                {
                    mLogger.Debug("Failed to get description for term:{0}, in language:{1}. Exception:{2}", parentTermId, lcid, e);
                }
                return description;
            }
        }
        public override Dictionary<int, string> GetAllDescriptions(Guid termStoreId, Guid termSetId, Guid parentTermId, Collection<int> lcids)
        {
            var descriptions = new Dictionary<int, string>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermSet set = store.GetTermSet(termSetId);
                Term term = set.GetTerm(parentTermId);
                try
                {
                    var results = lcids.ToDictionary(lcid => lcid, lcid => term.GetDescription(lcid));
                    context.ExecuteQuery();
                    descriptions = results.ToDictionary(p => p.Key, p => p.Value.Value);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Failed to get description for term: {0}. Exception:{1}", parentTermId, e);
                }
                return descriptions;
            }
        }
        public override Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl, bool createIfMissing)
        {
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermGroup group = store.GetSiteCollectionGroup(context.Site, createIfMissing);
                context.Load(group);
                context.ExecuteQuery();
                if (group.ServerObjectIsNull.HasValue && group.ServerObjectIsNull.Value)
                {
                    return null;
                }
                Dictionary<string, object> groupProp = new Dictionary<string, object>();
                AveObjectCopy.GetObjectBasicProperties(groupProp, group);
                return groupProp;
            }
        }
        public override Dictionary<string, object> GetTermGroup(Guid termStoreId, Guid groupId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                try
                {
                    if (groupId == Guid.Empty)
                    {
                        return groupProperties;
                    }
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermGroup group = null;
                    if (termStoreId != Guid.Empty)
                    {
                        TermStore store = session.TermStores.GetById(termStoreId);
                        group = store.GetGroup(groupId);
                    }
                    else
                    {
                        throw new InvalidDataException("Term store id invalid.");
                    }
                    context.Load(group);
                    context.ExecuteQuery();
                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get the term group. termGroupId:{0}, error message:{1}", groupId, e.ToString());
                }
                return groupProperties;
            }
        }
        public override Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termSetProperties = new Dictionary<string, object>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermSet termSet = store.GetTermSet(termSetId);
                    context.Load(termSet);
                    TermGroup group = termSet.Group;
                    context.Load(group);
                    context.ExecuteQuery();
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                    Dictionary<string, object> setProperties = new Dictionary<string, object>();
                    CopyProperty(setProperties, termSet);
                    groupProperties.Add("TermSet", setProperties);
                    termSetProperties.Add("Group", groupProperties);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get TermSet Failed, error message:{0}", e.ToString());
                }
                return termSetProperties;
            }
        }
        public override Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termSetCollectionProperties = new Dictionary<string, object>();
                Dictionary<string, Dictionary<string, object>> termStoresProperties = new Dictionary<string, Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermSetCollection termSetCollection = session.GetTermSetsByName(termSetName, LCID);
                    context.Load(termSetCollection, tempSetCollection => tempSetCollection.IncludeWithDefaultProperties(temp => temp.Group, temp => temp.TermStore));
                    context.ExecuteQuery();
                    foreach (TermSet termSet in termSetCollection)
                    {
                        Dictionary<string, object> termStoreProperties = null;
                        Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                        Dictionary<string, object> termSetProperties = new Dictionary<string, object>();

                        string storeId = termSet.TermStore.Id.ToString();
                        string groupId = termSet.Group.Id.ToString();
                        CopyProperty(termSetProperties, termSet);
                        AveObjectCopy.GetObjectBasicProperties(groupProperties, termSet.Group);
                        groupProperties["TermSet"] = termSetProperties;

                        if (termStoresProperties.ContainsKey(storeId))
                        {
                            termStoreProperties = termStoresProperties[storeId];
                        }
                        else
                        {
                            termStoreProperties = new Dictionary<string, object>();
                            AveObjectCopy.GetObjectBasicProperties(termStoreProperties, termSet.TermStore);
                            termStoresProperties[storeId] = termStoreProperties;
                        }
                        if (!termStoreProperties.ContainsKey("Groups"))
                        {
                            termStoreProperties["Groups"] = new Dictionary<string, Dictionary<string, object>>();
                            Dictionary<string, Dictionary<string, object>> dic = termStoreProperties["Groups"] as Dictionary<string, Dictionary<string, object>>;
                            dic[groupId] = groupProperties;
                        }
                        else
                        {
                            Dictionary<string, Dictionary<string, object>> dic = termStoreProperties["Groups"] as Dictionary<string, Dictionary<string, object>>;
                            dic[groupId] = groupProperties;
                        }
                    }
                    termSetCollectionProperties.Add("TermStores", termStoresProperties);
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermSets in TermStore Failed, error message:{0}", e.ToString());
                }
                return termSetCollectionProperties;
            }
        }
        public override Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termProperties = new Dictionary<string, object>();
                try
                {
                    if (termId == Guid.Empty)
                    {
                        return termProperties;
                    }
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    Term term = null;
                    if (termStoreId != Guid.Empty)
                    {
                        TermStore store = session.TermStores.GetById(termStoreId);
                        term = store.GetTerm(termId);
                    }
                    else
                    {
                        term = session.GetTerm(termId);
                    }
                    context.Load(term);
                    context.Load(term, t => t.Parent.Id, t => t.PinSourceTermSet);
                    TermSet termSet = term.TermSet;
                    context.Load(termSet);
                    TermGroup group = termSet.Group;
                    context.Load(termSet.Group);
                    context.ExecuteQuery();
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                    Dictionary<string, object> setProperties = new Dictionary<string, object>();
                    CopyProperty(setProperties, termSet);
                    groupProperties.Add("TermSet", setProperties);
                    Dictionary<string, object> findedTermProperties = new Dictionary<string, object>();
                    AssembleTermProperties(term, findedTermProperties);
                    setProperties.Add("Term", findedTermProperties);
                    termProperties.Add("Group", groupProperties);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get the term. termId:{0}, error message:{1}", termId, e.ToString());
                }
                return termProperties;
            }
        }
        public override bool IsTermExist(Guid termStoreId, Guid termId)
        {
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                Term term = store.GetTerm(termId);
                context.ExecuteQuery();
                return term.ServerObjectIsNull.HasValue ? !term.ServerObjectIsNull.Value : false;
            }
        }

        public override bool IsTermSetExist(Guid termStoreId, Guid termSetId)
        {
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                var termSet = store.GetTermSet(termSetId);
                context.ExecuteQuery();
                return termSet.ServerObjectIsNull.HasValue ? !termSet.ServerObjectIsNull.Value : false;
            }
        }

        public override Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termProperties = new Dictionary<string, object>();
                try
                {
                    if (termStoreId == Guid.Empty || termSetId == Guid.Empty || termId == Guid.Empty)
                    {
                        return termProperties;
                    }
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    Term term = store.GetTermInTermSet(termSetId, termId);
                    context.Load(term);
                    context.Load(term, t => t.Parent.Id, t => t.PinSourceTermSet);
                    TermSet termSet = term.TermSet;
                    context.Load(termSet);
                    TermGroup group = termSet.Group;
                    context.Load(termSet.Group);
                    context.ExecuteQuery();
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                    Dictionary<string, object> setProperties = new Dictionary<string, object>();
                    CopyProperty(setProperties, termSet);
                    groupProperties.Add("TermSet", setProperties);
                    Dictionary<string, object> findedTermProperties = new Dictionary<string, object>();
                    AssembleTermProperties(term, findedTermProperties);
                    setProperties.Add("Term", findedTermProperties);
                    termProperties.Add("Group", groupProperties);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get the term. termId:{0}, error message:{1}", termId, e.ToString());
                }
                return termProperties;
            }
        }
        public override Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> termsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> termsList = new List<Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermCollection terms = null;
                    LabelMatchInformation info = new LabelMatchInformation(context);
                    info.TermLabel = termLabel;
                    info.TrimUnavailable = trimUnavailable;
                    if (termStoreId != Guid.Empty)
                    {
                        TermStore store = session.TermStores.GetById(termStoreId);
                        TermSet set = store.GetTermSet(termSetId);
                        terms = set.GetTerms(info);
                    }
                    else
                    {
                        terms = session.GetTerms(info);
                    }
                    context.Load(terms, termCollection => termCollection.IncludeWithDefaultProperties(t => t.Parent.Id, t => t.PinSourceTermSet));
                    context.ExecuteQuery();
                    foreach (Term term in terms)
                    {
                        Dictionary<string, object> termProp = new Dictionary<string, object>();
                        //AveObjectCopy.GetObjectBasicProperties(termProp, term);
                        CopyProperty(termProp, term);
                        termProp["ParentTermId"] = term.Parent.IsPropertyAvailable("Id") ? term.Parent.Id : Guid.Empty;
                        try
                        {
                            termProp["PinSourceTermSetId"] = term.PinSourceTermSet.Id;
                        }
                        catch (ServerObjectNullReferenceException e)
                        {
                            mLogger.Debug("Term does not has pin source term set. Error message: {0}", e.ToString());
                        }
                        termsList.Add(termProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug("Failed to get terms properties, error message : {0}", e.ToString());
                }
                termsProp[AveObjectModelConstant.ChildrenProperties] = termsList;
                return termsProp;
            }
        }

        public override Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime)
        {
            using (AveClientContext context = CreateContext())
            {
                var info = new ChangeInformation(context)
                {
                    StartTime = startTime
                };
                return GetChanges(termStoreId, context, info);
            }
        }

        public override Dictionary<string, object> GetChanges(Guid termStoreId, TimeSpan sinceTimeAgo)
        {
            using (AveClientContext context = CreateContext())
            {
                var info = new ChangeInformation(context)
                {
                    WithinTimeSpan = sinceTimeAgo
                };
                return GetChanges(termStoreId, context, info);
            }
        }

        public override Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType)
        {
            using (AveClientContext context = CreateContext())
            {
                var info = new ChangeInformation(context)
                {
                    StartTime = startTime,
                    ItemType = (ChangedItemType)itemType,
                };
                return GetChanges(termStoreId, context, info);
            }
        }

        public override Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType)
        {
            using (AveClientContext context = CreateContext())
            {
                var info = new ChangeInformation(context)
                {
                    StartTime = startTime,
                    ItemType = (ChangedItemType)itemType,
                    OperationType = (ChangedOperationType)operationType
                };
                return GetChanges(termStoreId, context, info);
            }
        }

        private Dictionary<string, object> GetChanges(Guid termStoreId, AveClientContext context, ChangeInformation info)
        {
            TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
            if (termStoreId != Guid.Empty)
            {
                var changeItemProp = new Dictionary<string, object>();
                try
                {
                    List<Dictionary<string, object>> changedList = new List<Dictionary<string, object>>();
                    TermStore store = session.TermStores.GetById(termStoreId);
                    var collection = store.GetChanges(info);
                    context.Load(collection);
                    context.ExecuteQuery();
                    foreach (var changeItem in collection)
                    {
                        Dictionary<string, object> itemProp = new Dictionary<string, object>();
                        AveObjectCopy.GetObjectBasicProperties(itemProp, changeItem);
                        changedList.Add(itemProp);
                    }
                    changeItemProp[AveObjectModelConstant.ChildrenProperties] = changedList;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get taxonomy changes failed. Error:{0}", e);
                }
                return changeItemProp;
            }
            else
            {
                throw new InvalidDataException("Term store id invalid.");
            }
        }


        public override string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID)
        {
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                Term term = store.GetTerm(termId);
                ClientResult<string> defaultLabel = term.GetDefaultLabel(defaultID);
                context.ExecuteQuery();
                return defaultLabel.Value;
            }
        }
        public override Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> relatedfields = new Dictionary<string, object>();
                List<Dictionary<string, object>> relatedFieldPropertiesList = new List<Dictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listTitle);
                List list = web.Lists.GetById(listId);
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
        //public override Dictionary<string, object> GetListAssociastedProperty(string webServerRelativeUrl, string listTitle)
        //{
        //    throw new NotImplementedException();
        //}
        public override Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            try
            {
                return mRequestCommon.GetSitePortal(siteUrl);
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get site audit flags failed. Message:{0}", ex.ToString());
                Dictionary<string, object> sitePortal = new Dictionary<string, object>();
                sitePortal.Add("PortalUrl", string.Empty);
                sitePortal.Add("PortalName", string.Empty);
                return sitePortal;
            }
        }
        public override List<string> GetSiteEnabledHelpCollections()
        {
            //string getUrl = mWebUrl.TrimEnd('/') + "/_layouts/15/HelpSettings.aspx";
            return mRequestCommon.GetSiteEnabledHelpCollections();
        }
        public override bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListRated(webServerRelativeUrl, listId);
        }
        public override string GetListExperience(string webServerRelativeUrl, Guid guid)
        {
            return mRequestCommon.GetListExperience(webServerRelativeUrl, guid);
        }
        public override Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            return mRequestCommon.GetMetadataNavigationSettings(webServerRelativeUrl, listId, listTitle);
        }
        public override List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListCheckedOutFiles(webServerRelativeUrl, listId);
        }
        public override Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            return mRequestCommon.GetMetadataListFieldSettings(webServerRelativeUrl, listTitle, listId);
        }
        public override void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            mRequestCommon.UpdateMetadataListFieldSettings(webServerRelativeUrl, listId, updateProperties);
        }
        public override Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListVersionLimited(webServerRelativeUrl, listId);
        }
        public override Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            return new Dictionary<string, object>();
        }
        public override Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListRssProperties(webServerRelativeUrl, listId);
        }
        public override void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            mRequestCommon.UpdateListRssSetting(webServerRelativeUrl, listId, updateProp);
        }
        public override List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return mRequestCommon.GetPublishedContentTypes();
        }
        public override Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            return mRequestCommon.GetListGeneralProperties(webServerRelativeUrl, listId);
        }
        public override Dictionary<string, object> GetListEditViewSettingProperties(string webServerRelativeUrl, String listTitle, Guid listId, Guid viewId)
        {
            return new Dictionary<string, object>();
        }
        public override Dictionary<string, object> GetListAccessRequestsSettingProperties(String webServerRelativeUrl, Guid listId)
        {
            return new Dictionary<string, object>();
        }
        public override Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            return this.mRequestCommon.GetListAdvancedSettingProperties(webServerRelativeUrl, listId, null);
        }
        public override Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> settings = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                context.Load(list.InformationRightsManagementSettings);
                context.ExecuteQuery();
                CopyProperty(settings, list.InformationRightsManagementSettings);
                return settings;
            }
        }
        public override Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> setting = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                list.InformationRightsManagementSettings.Reset();
                context.Load(list.InformationRightsManagementSettings);
                context.ExecuteQuery();
                CopyProperty(setting, list.InformationRightsManagementSettings);
                return setting;
            }
        }
        public override Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> setting = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                AveObjectCopy.UpdateObjectBasicProperties(updateProperties, list.InformationRightsManagementSettings);
                list.InformationRightsManagementSettings.Update();
                context.Load(list.InformationRightsManagementSettings);
                context.ExecuteQuery();
                CopyProperty(setting, list.InformationRightsManagementSettings);
                return setting;
            }
        }
        public override List<Dictionary<string, object>> GetDisplayGroupsForSite()
        {
            return new List<Dictionary<string, object>>();
        }
        public override List<Dictionary<string, object>> GetKeyWords()
        {
            return mRequestCommon.GetKeyWords();
        }
        public override Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            return mRequestCommon.GetWebLogoProperties(webServerRelativeUrl);
        }
        public override Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl)
        {
            using (ClientContext context = CreateContext())
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

        public override Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            return mRequestCommon.GetAllFeatureDefinitions(Url, featuresSource);
        }

        public override bool DoesUserHavePermissions(string webServerRelativeUrl, ulong permissionMask)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                BasePermissions permissions = new BasePermissions();
                //permissions.Set((PermissionKind)permissionMask);
                //此段set逻辑通过查看SPBasePermissions_Client的Init()实现的
                //目前只有AveSecurableObject.DoesUserHavePermissions(AveBasePermissions permissionMask)一处调用此方法
                //可以不用循环set PermissionKind来实现多枚举的BasePermissions
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
        public override Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> regionalSettingProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                RegionalSettings regionalSettings = web.RegionalSettings;
                context.Load(regionalSettings);
                context.Load(regionalSettings.TimeZone);
                context.ExecuteQuery();
                CopyProperty(regionalSettingProperties, regionalSettings);
                Dictionary<string, object> timeZoneProperties = new Dictionary<string, object>();
                CopyProperty(timeZoneProperties, regionalSettings.TimeZone);
                timeZoneProperties["ID"] = Convert.ToUInt16(regionalSettings.TimeZone.Id);
                if (timeZoneProperties.ContainsKey("Id"))
                {
                    timeZoneProperties.Remove("Id");
                }
                regionalSettingProperties["TimeZone" + AveObjectModelConstant.ObjectPropertySuffix] = timeZoneProperties;
                try
                {
                    List<Dictionary<string, object>> installedLanguages = mRequestCommon.GetInstalledLanguages(webServerRelativeUrl);
                    if (installedLanguages.Count > 0)
                    {
                        Dictionary<string, object> InstalledLanguagesLCID = new Dictionary<string, object>();
                        InstalledLanguagesLCID[AveObjectModelConstant.ChildrenProperties] = installedLanguages;
                        regionalSettingProperties["InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix] = InstalledLanguagesLCID;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Info("Get web installed language from page failed.Message: {0}", ex.ToString());
                }
                return regionalSettingProperties;
            }
        }
        public override DateTime GetUTCToLocalTime(string webServerRelativeUrl, DateTime time)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                RegionalSettings regionalSettings = web.RegionalSettings;
                var localTime = regionalSettings.TimeZone.UTCToLocalTime(time);
                context.ExecuteQuery();
                return localTime.Value;
            }
        }

        public override DateTime GetLocalToUTCTime(string webServerRelativeUrl, DateTime time)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                RegionalSettings regionalSettings = web.RegionalSettings;
                //ADO-147814 对于真实O365 ,由于Client API可以正确转换Unspecified -> Utc DateTime,  无法正确转换 Local -> Utc DateTime,
                //因此在转换之前先把Kind设为Unspecified
                var utcTime = regionalSettings.TimeZone.LocalTimeToUTC(DateTime.SpecifyKind(time, DateTimeKind.Unspecified));
                context.ExecuteQuery();
                return utcTime.Value;
            }

        }

        public override Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            return mRequestCommon.GetDefaultRegionalSetting(webServerRelativeUrl, lcid);
        }
        public override Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl, int compatibilityLevel)
        {
            if (compatibilityLevel == 14)
            {
                return mWebServiceRequest.GetThemeUrlForWeb(webServerRelativeUrl, compatibilityLevel);
            }
            else
            {
                return mRequestCommon.GetThemeUrlForWeb(webServerRelativeUrl);
            }
        }
        public override Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl)
        {
            return new Dictionary<string, object>();
        }
        public override Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            return mRequestCommon.GetMasterPageProperties(webServerRelativeUrl);
        }

        public override bool GetSiteRssSetting()
        {
            return mRequestCommon.GetSiteRssSetting();
        }
        public override Dictionary<string, object> GetNavigation(string webServerRelativeUrl)
        {
            Dictionary<string, object> nodesProp = new Dictionary<string, object>();
            try
            {
                string getUrl;
                if (this.mCompatibilityLevel == 15)
                {
                    getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/15/AreaNavigationSettings.aspx";
                }
                else
                {
                    getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/AreaNavigationSettings.aspx";
                }
                string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj);
                string searchContent = "newNode = new NavigationNode(";
                AveHttpWebRequestUtility.GetNodesProperties(html, searchContent, nodesProp);
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get Web:{0} Navigation failed.Error Message:{1}", webServerRelativeUrl, ex.ToString());
            }
            return this.GetNavigation(webServerRelativeUrl, nodesProp);
        }

        public override List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            List<AveItemVersionBrowserInfo> ItemVersionsInfoList = new List<AveItemVersionBrowserInfo>();
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                needLoadFields.Add("_UIVersionString", "Text");
                Guid listId = Guid.Empty;
                int itemId = GetItemIdByUniqueId(option.ParentWebServerRelativeUrl, option.ParentItemUniqueId, option.ParentListTitle, ref listId);
                Dictionary<string, object> versionsInfo = GetItemVersionsForBrowser(option.ParentWebServerRelativeUrl, listId.ToString(), itemId, needLoadFields);

                List<Dictionary<string, object>> versionLabels = (versionsInfo[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>);
                if (versionLabels.Count > 0)
                {
                    int pagingCount = 0;
                    option.ChildrenTotalCount = versionLabels.Count;
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
                            AveItemVersionBrowserInfo versionInfo = new AveItemVersionBrowserInfo();
                            var versionLabel = versionLabels[i + option.StartIndex];
                            if (versionLabel.ContainsKey("VersionLabel"))//Office365 can get version label property
                            {
                                versionInfo.VersionLabel = versionLabel["VersionLabel"].ToString();
                            }
                            else
                            {
                                versionInfo.VersionLabel = (versionLabel["FieldValues"] as Dictionary<string, object>)["_UIVersionString"].ToString();
                            }
                            ItemVersionsInfoList.Add(versionInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, option.ChildrenTotalCount, ex.ToString());
                    }
                }
            }
            return ItemVersionsInfoList;

        }


        protected List<Folder> QueryFoldersForLargeList(ClientContext context, List list, string folderUrl)
        {
            List<Folder> folders = new List<Folder>();
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, null);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                    item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "1"));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                context.Load(itemArg.Folder);
                context.Load(itemArg.Folder, f => f.ListItemAllFields.HasUniqueRoleAssignments, f => f.ListItemAllFields["UniqueId"], f => f.Properties["vti_etag"]);
                folders.Add(itemArg.Folder);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                folders.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin discover folders in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            context.ExecuteQuery();
            mLogger.Debug("Finish discover folders in large list, {0} folders in folder {1}", folders.Count, folderUrl);
            return folders;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is property name")]
        public override List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            List<AveFolderBrowserInfo> folders = new List<AveFolderBrowserInfo>();
            using (AveClientContext context = CreateContext())
            {
                List<Folder> tempFolders = null;
                Web web = context.Site.OpenWebById(option.ParentWebId);
                List list = web.Lists.GetById(option.ParentListId);
                context.Load(list);
                context.Load(list.RootFolder, folder => folder.ServerRelativeUrl);
                context.ExecuteQuery();
                if (IsThrottled(list.ItemCount))
                {
                    var queryFolders = QueryFoldersForLargeList(context, list, option.ParentFolderServerRelativeUrl);
                    tempFolders = queryFolders;
                }
                else
                {
                    Folder folder = GetFolderByAPI(web, option.ParentFolderServerRelativeUrl);
                    ExceptionHandlingScope han = new ExceptionHandlingScope(context);
                    using (han.StartScope())
                    {
                        using (han.StartTry())
                        {
                            context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ListItemAllFields.HasUniqueRoleAssignments,
                                                                                               f => f.ListItemAllFields["UniqueId"],
                                                                                               f => f.Properties["vti_etag"]));
                        }
                        using (han.StartCatch())
                        {
                            context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.Properties));
                        }
                    }
                    context.ExecuteQuery();
                    tempFolders = folder.Folders.ToList<Folder>();
                }
                option.ChildrenTotalCount = tempFolders.Count;
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
                    int filterFolderCount = 0;
                    for (int i = 0; i < pagingCount; i++)
                    {
                        if (i + option.StartIndex < option.ChildrenTotalCount)
                        {
                            Folder subFolder = tempFolders[i + option.StartIndex];
                            AveFolderBrowserInfo folderInfo = new AveFolderBrowserInfo();
                            folderInfo.Name = subFolder.Name;
                            folderInfo.ServerRelativeUrl = subFolder.ServerRelativeUrl;
                            folderInfo.Url = new Uri(new Uri(this.mWebUrl), subFolder.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
                            folderInfo.ParentId = option.ParentFolderId;
                            folderInfo.Hidden = subFolder.ListItemAllFields.FieldValues.Count <= 0;
                            if (subFolder.ListItemAllFields.FieldValues.Count > 0)
                            {
                                folderInfo.UniqueId = (Guid)subFolder.ListItemAllFields.FieldValues["UniqueId"];
                                folderInfo.HasUniqueRoleAssignments = subFolder.ListItemAllFields.HasUniqueRoleAssignments;
                            }
                            else if (subFolder.Properties.FieldValues.ContainsKey("vti_etag") &&
                                     subFolder.Properties["vti_etag"] != null)
                            {
                                string tagString = subFolder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                                folderInfo.UniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                                folderInfo.HasUniqueRoleAssignments = false;
                            }
                            if (option.ParentListId != Guid.Empty)
                            {
                                folderInfo.ParentListId = option.ParentListId;
                            }
                            else if (subFolder.Properties.FieldValues.ContainsKey("vti_listname") &&
                                     subFolder.Properties["vti_listname"] != null)
                            {
                                folderInfo.ParentListId = new Guid(subFolder.Properties["vti_listname"].ToString());
                            }
                            bool isWebFolder = option.ParentListId == Guid.Empty;
                            if (option.NeedFilter && option.FilterSystemFolder)
                            {
                                if (isWebFolder && option.ParentListId != Guid.Empty)
                                {
                                    filterFolderCount++;
                                    continue;
                                }
                                if (!isWebFolder && folderInfo.Hidden)
                                {
                                    filterFolderCount++;
                                    continue;
                                }
                            }
                            folders.Add(folderInfo);
                        }
                    }
                    option.ChildrenTotalCount -= filterFolderCount;
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, option.ChildrenTotalCount, ex.ToString());
                }
            }
            return folders;
        }

        public override AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            AveWebBrowserInfo webBrowserInfo = new AveWebBrowserInfo();
            using (AveClientContext context = CreateContext())
            {
                Web rootWeb = context.Site.RootWeb;
                context.Load(rootWeb, w => w.ServerRelativeUrl,
                                             w => w.Id,
                                             w => w.Title,
                                             //w => w.ListTemplates,
                                             w => w.Language,
                                             w => w.WebTemplate,
                                             w => w.Configuration,
                                             w => w.HasUniqueRoleAssignments);
                context.ExecuteQuery();
                base.SetWebBrowserInfo(webBrowserInfo, rootWeb);
                return webBrowserInfo;
            }

        }
        public override List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            using (AveClientContext context = CreateContext())
            {
                List<AveWebBrowserInfo> webInfos = new List<AveWebBrowserInfo>();
                List<Web> realWebs = new List<Web>();
                WebCollection subWebs = null;
                int pagingCount = 0;
                var childrenCount = 0;
                try
                {
                    Web parentWeb = context.Site.OpenWebById(option.ParentWebId);
                    subWebs = parentWeb.GetSubwebsForCurrentUser(null);
                    context.Load(subWebs, webs => webs.IncludeWithDefaultProperties(w => w.HasUniqueRoleAssignments));//w => w.ListTemplates, 
                    context.ExecuteQuery();
                    for (int i = 0; i < subWebs.Count; i++)
                    {
                        var web = subWebs[i];
                        if (web.AppInstanceId != Guid.Empty && option.FilterAppWeb)
                        {
                            continue;
                        }
                        realWebs.Add(web);
                    }
                    childrenCount = realWebs.Count;
                    if (option.StartIndex > childrenCount)
                    {
                        foreach (Web web in realWebs)
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
                if (childrenCount - option.StartIndex < option.PerPage)
                {
                    pagingCount = childrenCount - option.StartIndex;
                }
                else
                {
                    pagingCount = (int)option.PerPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        var web = realWebs[option.StartIndex + i];
                        AveWebBrowserInfo info = new AveWebBrowserInfo();
                        SetWebBrowserInfo(info, web);
                        webInfos.Add(info);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, childrenCount, ex.ToString());
                }
                option.ChildrenTotalCount = childrenCount;
                return webInfos;
            }
        }

        protected override void SetWebBrowserInfo(AveWebBrowserInfo info, Web web)
        {
            base.SetWebBrowserInfo(info, web);
            info.IsAppWeb = IsApplicationWeb(web);
        }

        public override Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            Dictionary<string, object> clientAPIProperties = base.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
            try
            {
                List<Dictionary<string, object>> workflows = clientAPIProperties[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                if (workflows.Count > 0)//当存在workflow时,再调用mRequestCommon.GetWorkflowAssociations
                {
                    Dictionary<string, object> webRequestProperties = mRequestCommon.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
                    if (webRequestProperties != null)
                    {
                        foreach (Dictionary<string, object> workflowProp in workflows)
                        {
                            workflowProp["RunningInstances"] = webRequestProperties[workflowProp["Name"].ToString()];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Info(e.ToString());
                return clientAPIProperties;
            }
            return clientAPIProperties;
        }
        public override Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                context.Load(workflowServicesManager);
                context.ExecuteQuery();

                CopyProperty(returnInfo, workflowServicesManager);

                return returnInfo;
            }
        }

        public override Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowSubscriptionCollection workflowSubscriptionColl = workflowServicesManager.GetWorkflowSubscriptionService().EnumerateSubscriptionsByList(listId);
                context.Load(workflowSubscriptionColl);
                context.ExecuteQuery();

                List<Dictionary<string, object>> subscrips = new List<Dictionary<string, object>>();
                foreach (WorkflowSubscription workflow in workflowSubscriptionColl)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    subscrips.Add(workflowPro);
                }
                returnInfo[AveObjectModelConstant.ChildrenProperties] = subscrips;

                return returnInfo;
            }
        }
        public override Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId)
        {
            var returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowServicesManager workflowServicesManager = new WorkflowServicesManager(context, web);
                WorkflowSubscriptionCollection workflowSubscriptionColl = workflowServicesManager.GetWorkflowSubscriptionService().EnumerateSubscriptionsByEventSource(webId);
                context.Load(workflowSubscriptionColl);
                context.ExecuteQuery();
                List<Dictionary<string, object>> subscrips = new List<Dictionary<string, object>>();
                foreach (WorkflowSubscription workflow in workflowSubscriptionColl)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, workflow);
                    subscrips.Add(workflowPro);
                }
                returnInfo[AveObjectModelConstant.ChildrenProperties] = subscrips;
                return returnInfo;
            }
        }
        public override Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                WorkflowDeploymentService workflowDeploymentService = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();
                WorkflowDefinition workflowDefinition = workflowDeploymentService.GetDefinition(definitionId);
                context.Load(workflowDefinition);
                context.ExecuteQuery();

                CopyProperty(returnInfo, workflowDefinition);

                return returnInfo;
            }
        }

        public override Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WorkflowDeploymentService workflowDeploymentService = new WorkflowServicesManager(context, web).GetWorkflowDeploymentService();
                WorkflowDefinitionCollection workflowDefinitions = workflowDeploymentService.EnumerateDefinitions(publishedOnly);
                context.Load(workflowDefinitions);
                context.ExecuteQuery();

                List<Dictionary<string, object>> definitions = new List<Dictionary<string, object>>();
                foreach (WorkflowDefinition definition in workflowDefinitions)
                {
                    Dictionary<string, object> workflowPro = new Dictionary<string, object>();
                    CopyProperty(workflowPro, definition);
                    definitions.Add(workflowPro);
                }
                returnInfo[AveObjectModelConstant.ChildrenProperties] = definitions;
                return returnInfo;
            }
        }
        public override Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition)
        {
            //Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                WorkflowDeploymentService workflowDeploymentService = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();

                WorkflowDefinition workflowDefinition = new WorkflowDefinition(context);
                workflowDefinition.AssociationUrl = definition.AssociationUrl;
                workflowDefinition.Description = definition.Description;
                workflowDefinition.DisplayName = definition.DisplayName;
                workflowDefinition.DraftVersion = definition.DraftVersion;
                workflowDefinition.FormField = definition.FormField;
                workflowDefinition.InitiationUrl = definition.InitiationUrl;
                if (definition.Properties != null)
                {
                    foreach (KeyValuePair<string, string> kv in definition.Properties)
                    {
                        workflowDefinition.SetProperty(kv.Key, kv.Value);
                    }
                }
                workflowDefinition.RequiresAssociationForm = definition.RequiresAssociationForm;
                workflowDefinition.RequiresInitiationForm = definition.RequiresInitiationForm;
                workflowDefinition.RestrictToScope = definition.RestrictToScope;
                workflowDefinition.RestrictToType = definition.RestrictToType;
                workflowDefinition.Xaml = definition.Xaml;
                if (!definition.Id.Equals(Guid.Empty))
                {
                    workflowDefinition.Id = definition.Id;
                }

                ClientResult<Guid> res = workflowDeploymentService.SaveDefinition(workflowDefinition);
                context.Load(workflowDefinition);
                context.ExecuteQuery();
                //definition.Id = workflowDefinition.Id;
                return res.Value;
            }
        }

        public override void PublishDefinition(string webServerRelativeUrl, Guid definitionId)
        {
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                WorkflowDeploymentService workflowDeploymentService = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();
                workflowDeploymentService.PublishDefinition(definitionId);
                context.ExecuteQuery();
            }
        }

        public override Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                WorkflowSubscriptionService workflowSubscriptionService = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();
                WorkflowSubscription workflowSubscription = new WorkflowSubscription(context);
                workflowSubscription.DefinitionId = subscription.DefinitionId;
                //workflowSubscription.Enabled = subscription.Enabled;
                workflowSubscription.EventSourceId = subscription.EventSourceId;
                workflowSubscription.Id = subscription.Id;
                workflowSubscription.Name = subscription.Name;
                //workflowSubscription.SetProperty("HistoryListId", subscription.GetProperty("HistoryListId"));
                //workflowSubscription.SetProperty("TaskListId", subscription.GetProperty("TaskListId"));
                if (subscription.PropertyDefinitions != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in subscription.PropertyDefinitions)
                    {
                        workflowSubscription.SetProperty(keyValuePair.Key, keyValuePair.Value);
                    }
                }
                workflowSubscription.StatusFieldName = subscription.StatusFieldName;
                string eventTypeStr = string.Empty;
                List<string> eventTypes = new List<string>();
                foreach (string eventType in subscription.EventTypes)
                {
                    if (!eventTypes.Contains(eventType))
                    {
                        eventTypeStr += eventType + "#;";
                        eventTypes.Add(eventType);
                    }
                }
                workflowSubscription.EventTypes = eventTypes.ToArray();
                ClientResult<Guid> res = listId != Guid.Empty ? workflowSubscriptionService.PublishSubscriptionForList(workflowSubscription, listId) :
                                                                workflowSubscriptionService.PublishSubscription(workflowSubscription);
                context.ExecuteQuery();
                return res.Value;
            }
        }

        public override Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                WorkflowServicesManager workflowServicesManager = null;
                WorkflowSubscriptionService workflowSubscriptionService = null;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                workflowServicesManager = new WorkflowServicesManager(context, web);
                workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();
                WorkflowSubscription workflowSubscription = workflowSubscriptionService.GetSubscription(subscriptionId);
                context.Load(workflowSubscription);
                context.ExecuteQuery();

                CopyProperty(returnInfo, workflowSubscription);
                return returnInfo;
            }
        }

        #region Get Workflow SubscriptionService
        //public Dictionary<string, object> GetWorkflowSubscriptionService(string webServerRelativeUrl) 
        //{
        //    Dictionary<string, object> returnInfo = new Dictionary<string, object>();
        //    using (AveClientContext context = CreateContext()) 
        //    {
        //        WorkflowServicesManager workflowServicesManager = null;
        //        WorkflowSubscriptionService workflowSubscriptionService = null;
        //        Web web = context.Site.OpenWeb(webServerRelativeUrl);

        //        context.Load(web);
        //        context.ExecuteQuery();

        //        workflowServicesManager = new WorkflowServicesManager(context, web);

        //        context.Load(workflowServicesManager);
        //        context.ExecuteQuery();

        //        workflowSubscriptionService = workflowServicesManager.GetWorkflowSubscriptionService();

        //        context.Load(workflowSubscriptionService);
        //        context.ExecuteQuery();

        //        CopyProperty(returnInfo, workflowSubscriptionService);

        //        return returnInfo;
        //    }
        //}

        //public Dictionary<string, object> GetWorkflowDeploymentService(string webServerRelativeUrl)
        //{
        //    Dictionary<string, object> returnInfo = new Dictionary<string, object>();
        //    using (AveClientContext context = CreateContext())
        //    {
        //        WorkflowServicesManager workflowServicesManager = null;
        //        WorkflowDeploymentService workflowDeploymentService = null;
        //        Web web = context.Site.OpenWeb(webServerRelativeUrl);

        //        context.Load(web);
        //        context.ExecuteQuery();

        //        workflowServicesManager = new WorkflowServicesManager(context, web);

        //        context.Load(workflowServicesManager);
        //        context.ExecuteQuery();

        //        workflowDeploymentService = workflowServicesManager.GetWorkflowDeploymentService();

        //        context.Load(workflowDeploymentService);
        //        context.ExecuteQuery();

        //        CopyProperty(returnInfo, workflowDeploymentService);

        //        return returnInfo;
        //    }
        //}
        #endregion

        public override Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, int receiverType, string assembly, string className, string name)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiverInfo = new Dictionary<string, object>();
                try
                {
                    EventReceiverDefinitionCollection eventReceivers;
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = null;
                    switch (eventReceiverDefSource)
                    {
                        case "web.eventReceivers":
                            eventReceivers = web.EventReceivers; break;
                        case "list.eventReceivers":
                            if (listId != Guid.Empty)
                            {
                                list = web.Lists.GetById(listId);
                            }
                            else
                            {
                                list = web.Lists.GetByTitle(listTitle);
                            }
                            eventReceivers = list.EventReceivers; break;
                        default:
                            eventReceivers = web.EventReceivers; break;
                    }
                    EventReceiverDefinitionCreationInformation eventReceiverCreateInfo = new EventReceiverDefinitionCreationInformation();
                    eventReceiverCreateInfo.EventType = (EventReceiverType)receiverType;
                    eventReceiverCreateInfo.ReceiverAssembly = assembly;
                    eventReceiverCreateInfo.ReceiverClass = className;
                    eventReceiverCreateInfo.ReceiverName = name;
                    EventReceiverDefinition eventReceiver = eventReceivers.Add(eventReceiverCreateInfo);
                    if (list != null)
                    {
                        list.Update();
                    }
                    else
                    {
                        web.Update();
                    }
                    context.Load(eventReceiver);
                    context.ExecuteQuery();
                    eventReceiverInfo["Assembly"] = eventReceiver.ReceiverAssembly;
                    eventReceiverInfo["Class"] = eventReceiver.ReceiverClass;
                    eventReceiverInfo["Name"] = eventReceiver.ReceiverName;
                    eventReceiverInfo["Id"] = eventReceiver.ReceiverId;
                    eventReceiverInfo["Type"] = eventReceiver.EventType;
                }
                catch (Exception e)
                {
                    mLogger.Warn(e.Message);
                }
                return eventReceiverInfo;
            }
        }

        public virtual Dictionary<string, object> GetManagedSitecollectionData()
        {
            throw new NotImplementedException();
        }



        #endregion

        #region  Add

        //public override Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource)
        //{
        //    return mWebServiceRequest.AddGroup(webRelativeUrl, ownerName, ownerType, defaultUserName, groupName, description, groupSource);
        //}

        public override Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, Guid listId, int itemId, string leafName, byte[] attachment)
        {
            Dictionary<string, object> item = base.GetItem(webRelativeUrl, listName, listId, itemId, default(Guid));
            Dictionary<string, object> attach = mWebServiceRequest.AddAttachmentNow(webRelativeUrl, listName, listId, itemId, leafName, attachment);
            Dictionary<string, object> keeps = new Dictionary<string, object>();
            Dictionary<string, object> itemPros = new Dictionary<string, object>();
            #region Reset Modified time to keep modified time property
            itemPros.Add("Modified", item["TimeLastModified"]);
            itemPros.Add("_ModerationStatus", item["_ModerationStatus"]);
            #endregion
            keeps[AveObjectModelConstant.UpdateMethodName] = "Update";
            keeps["ChangedFieldValues"] = itemPros;
            base.UpdateItem(webRelativeUrl, listName, listId, itemId, keeps);
            return attach;
        }

        protected override void AddViewItems(ClientContext context, List list, Folder folder, string folderServerRelativeUrl, List<Dictionary<string, object>> items, List<Dictionary<string, object>> folders, string webServerRelativeUrl)
        {
            if (!WrapperConfiguration.BPOS_S.IncludeListView)
            {
                return;
            }
            bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
            bool isSystemFolder = folder.ListItemAllFields.ServerObjectIsNull.HasValue && folder.ListItemAllFields.ServerObjectIsNull.Value;
            var sortFileCache = new SortedDictionary<string, Dictionary<string, object>>();
            if (((list.BaseType.Equals(BaseType.GenericList) || list.BaseType.Equals(BaseType.Issue)) && isRootFolder) || isForms || isSystemFolder)
            {
                AddViewFiles(context, folder, list, sortFileCache);
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

        protected virtual void AddViewFiles(ClientContext context, ClientFolder folder, List list, SortedDictionary<string, Dictionary<string, object>> sortFileCache)
        {
            if (!IsThrottled(list.ItemCount))
            {
                context.Load(folder, f => f.Files.Where(file => file.ListItemAllFields.ServerObjectIsNull.Value));
                context.ExecuteQuery();
                foreach (ClientFile viewFile in folder.Files)
                {
                    Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                    AssembleViewFileProperties(itemProperty, viewFile);
                    itemProperty["IsSystemFile"] = true;
                    sortFileCache.Add(viewFile.Name, itemProperty);
                }
            }
            else
            {
                foreach (View view in list.Views)
                {
                    if (!string.IsNullOrEmpty(view.ServerRelativeUrl) && view.ServerRelativeUrl.StartsWith(folder.ServerRelativeUrl.TrimEnd('/') + '/'))
                    {
                        ClientFile viewFile = GetFileByAPI(list.ParentWeb, view.ServerRelativeUrl);
                        context.Load(viewFile);
                        context.ExecuteQuery();
                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                        AssembleViewFileProperties(itemProperty, viewFile);
                        itemProperty["IsSystemFile"] = true;
                        sortFileCache.Add(viewFile.Name, itemProperty);
                    }
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is folder property name")]
        protected override void AddViewFolder(ClientContext context, Folder folder, List list, List<Dictionary<string, object>> folders, string webServerRelativeUrl)
        {
            if (!IsThrottled(list.ItemCount))
            {
                context.Load(folder.Folders);
                context.Load(folder.Folders, fs => fs.Include(f => f.ParentFolder.ServerRelativeUrl, f => f.Properties).Where(f => f.ListItemAllFields.ServerObjectIsNull.Value));
                context.ExecuteQuery();
                foreach (var tempFolder in folder.Folders)
                {
                    //skip attachments backup
                    if (tempFolder.Name.Equals("Attachments", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    Dictionary<string, object> itemPro = new Dictionary<string, object>();
                    itemPro["Items"] = new List<Dictionary<string, object>>();
                    itemPro["Folders"] = new List<Dictionary<string, object>>();
                    itemPro["Attachments"] = new List<Dictionary<string, object>>();
                    itemPro["Versions"] = new List<Dictionary<string, object>>();
                    AssembleSystemFolderProperties(context as AveClientContext, itemPro, tempFolder, webServerRelativeUrl);
                    if (tempFolder.Properties.FieldValues.ContainsKey("vti_etag") &&
                        tempFolder.Properties["vti_etag"] != null)
                    {
                        string tagString = tempFolder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                        itemPro["UniqueId"] = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                    }
                    folders.Add(itemPro);
                }
            }
        }

        public override Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp)
        {
            return this.mWebServiceRequest.AddUser(webServerRelativeUrl, source, groupName, userProp);
        }

        public override Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            return mRequestCommon.AddKeyWord(term, startDate, localId, calendarType);
        }
        public override string AddSynonm(string term, string synTerm, string terms)
        {
            return mRequestCommon.AddSynonm(term, synTerm, terms);
        }
        public override Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            return mRequestCommon.AddBestBet(term, bestBetUrlList, bestBetProp, action);
        }

        public override void AddSitePolicy(string policySchema, string siteUrl)
        {
            mRequestCommon.AddSitePolicy(policySchema, siteUrl);
        }

        public virtual Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            return mRequestCommon.AddAlert(webServerRelativeUrl, listUrl, listTitle, listId, itemId, data);
        }


        public override Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions)
        {
            if (templateType == 110)
            {
                //2013环境里添加datasource模板的list会失败，因为server端会强制把quicklauch属性设置成true，但datasource没有view，导致出com异常0x8107140D，改用webservice添加
                Dictionary<string, object> listProperties = null;
                if (string.IsNullOrEmpty(featureId))
                {
                    listProperties = this.mWebServiceRequest.AddList(webServerRelativeUrl, title, description, Guid.Empty, templateType);
                }
                else
                {
                    listProperties = this.mWebServiceRequest.AddList(webServerRelativeUrl, title, description, url, featureId, templateType, docTemplateType, quickLaunchOptions);
                }
                return this.GetList(webServerRelativeUrl, (Guid)listProperties["Id"]);
            }
            else
            {
                return base.AddList(webServerRelativeUrl, title, description, url, featureId, templateType, docTemplateType, quickLaunchOptions);
            }
        }

        public override Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = web.Lists.GetById(listId);
                Folder parentFolder = GetFolderByAPI(web, folderUrl);
                ContentType contentType = list.ContentTypes.GetById(contentTypeId.ToString());
                context.Load(contentType, c => c.Id);
                context.ExecuteQuery();
                ClientResult<string> result = DocumentSet.Create(context, parentFolder, name, contentType.Id);
                context.ExecuteQuery();
                string documentSetRelativeUrl = result.Value;
                Dictionary<string, object> folderInfo = this.GetFolder(webServerRelativeUrl, listName, listId, AveUrlUtility.GetServerRelativeUrl(documentSetRelativeUrl));
                return folderInfo;
            }
        }

        public virtual bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "")
        {
            throw new NotImplementedException();
        }

        public override string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
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

        public void AddFileByRestApi(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite, Guid parentFolderUniqueId)
        {
            bool fileAdded = false;
            try
            {
                if (parentFolderUniqueId != Guid.Empty)
                {
                    AddFileByRestApiWithParentFolderId(parentWebUrl, parentFolderUniqueId, fileServerRelativeUrl, body, isOverwrite);
                    fileAdded = true;
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Add file by folder unique id failed.File:{0}.Will Add With path.Error:{1}", fileServerRelativeUrl, e);

            }
            if (!fileAdded)
            {
                AddFileByRestApiWithParentFolderPath(parentWebUrl, fileServerRelativeUrl, body, isOverwrite);
            }
        }
        private void AddFileByRestApiWithParentFolderId(string parentWebUrl, Guid parentFolderId, string fileName, Stream body, bool isOverwrite)
        {
            string realName = fileName;
            if (fileName.IndexOf('/') >= 0)
            {
                realName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            }
            realName = realName.Replace("'", "''");
            string methodCmd = string.Format("getfolderbyid(guid'{0}')/files/addUsingPath(decodedUrl='{1}', overwrite={2})", parentFolderId, realName, isOverwrite.ToString().ToLowerInvariant());
            AddFileByRestApi(methodCmd, parentWebUrl, body);
        }
        private void AddFileByRestApiWithParentFolderPath(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite)
        {
            int index = fileServerRelativeUrl.LastIndexOf('/');
            string indexstring = fileServerRelativeUrl.Substring(0, index);
            if (indexstring.Contains("'"))
            {
                indexstring = indexstring.Replace("'", "''");
            }
            string fileUrl = fileServerRelativeUrl.Substring(index + 1);
            if (fileUrl.Contains("'"))
            {
                fileUrl = fileUrl.Replace("'", "''");
            }
            string methodCmd = string.Format("getfolderbyserverrelativepath(decodedUrl='{0}')/files/addUsingPath(decodedUrl='{1}', overwrite={2})", Uri.EscapeDataString(indexstring), Uri.EscapeDataString(fileUrl), isOverwrite.ToString().ToLowerInvariant());
            AddFileByRestApi(methodCmd, parentWebUrl, body);
        }

        private void AddFileByRestApi(string methodCmd, string parentWebUrl, Stream body)
        {
            string request = string.Format("{0}/_api/Web/{1}", this.WebAppName + parentWebUrl, methodCmd);
            mLogger.Info("Add Large OneNote file request: {0}", request);
            ReconnectableHttpWebRequest webRequest = ReconnectableHttpWebRequest.CreateRequest(request);
            webRequest.RefreshDigestInfo(mFormDigestContext, provider);
            webRequest.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "T";
            if (mObj is CookieContainer)
            {
                webRequest.CookieContainer = mObj as CookieContainer;
            }
            else
            {
                webRequest.Credentials = mFormDigestContext.Credentials;
            }
            webRequest.ContentLength = body.Length;
            webRequest.Method = "POST";
            webRequest.Timeout = 600000;
            webRequest.ReadWriteTimeout = 1800000;
            webRequest.AllowWriteStreamBuffering = false;
            Stream inputBody = webRequest.GetRequestStream();
            byte[] buffer = new byte[1024 * 64];
            int len = 0;
            while ((len = body.Read(buffer, 0, buffer.Length)) != 0)
            {
                inputBody.Write(buffer, 0, len);
            }
            //AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
            //                                                                   new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
            //                                                                   new KeyValuePair<string, string>("WebException", "The operation has timed out"),
            //                                                                   new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
            //retryHelper.ExecuteWithRetryMechanism(() =>
            //{
            AddContentStream(webRequest);
            //});

        }

        private void AddContentStream(ReconnectableHttpWebRequest webRequest)
        {

            using (HttpWebResponse result = webRequest.GetResponse() as HttpWebResponse)
            {
                if (result != null)
                {
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        mLogger.Error("Failed to Restore large OneNote file by Rest API.cause: {0}", result.StatusCode.ToString());
                        throw new WebException(string.Format("unable to save the one note file. {0}", result.StatusCode));
                    }
                    mLogger.Info("Finished upload a large file by Rest Api");
                }
                else
                {
                    mLogger.Error("Failed to get Response when Restore large OneNote file.");
                    throw new WebException("unable to save the one note file. ");
                }
            }
        }

        public void AddFileByRPC(string parentWebUrl, string fileUrl, Stream bodyStream, bool isOverwrite)
        {
            string url = WebAppName + parentWebUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            if (fileUrl.StartsWith(parentWebUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileUrl = fileUrl.Substring(parentWebUrl.TrimEnd('/').Length + 1);
            }
            string order = "method=put+document%3a"
                + "&service%5fname=" + System.Web.HttpUtility.UrlEncode(parentWebUrl)
                + "&document=%5bdocument%5fname%3d" + System.Web.HttpUtility.UrlEncode(fileUrl) + "%3bmeta%5finfo%3d%5bvti%5fmodifiedby%3bSW%7cSHAREPOINT%5c%5csystem%3bvti%5fauthor%3bSW%7cSHAREPOINT%5c%5csystem%5d%5d"
                + "&put%5foption=edit" + (isOverwrite ? ",overwrite" : "") + "&comment=" + "&keep%5fchecked%5fout=false\n";
            var streamHeader = Encoding.UTF8.GetBytes(order);
            using (WebResponse response = StartWebRequest(url, streamHeader, bodyStream))
            {
                string responseString = GetResponseString(response.GetResponseStream());
                CheckForInternalErrorMessage(responseString);
                CheckForSuccessMessage(responseString);
            }

        }

        private static string GetResponseString(Stream responseStream)
        {
            StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        private WebResponse StartWebRequest(string url, byte[] streamHeader, Stream content)
        {
            ReconnectableHttpWebRequest request = ReconnectableHttpWebRequest.CreateRequest(url);
            //GetFormDigest();
            if (mObj is CookieContainer)
            {
                request.CookieContainer = mObj as CookieContainer;
            }
            else
            {
                request.Credentials = mFormDigestContext.Credentials;
            }
            request.RefreshDigestInfo(mFormDigestContext, provider);
            request.Timeout = 600000;
            request.ReadWriteTimeout = 1800000;
            request.Method = "POST";
            request.Headers["MINME_Version"] = "1.0";
            request.UserAgent = "MSFrontPage/15.0";
            request.Accept = "auth/sicily";
            request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "T";
            request.PreAuthenticate = true;
            request.Headers["Accept-encoding"] = "gzip, deflate";
            request.ContentLength = streamHeader.Length + content.Length;
            request.AllowWriteStreamBuffering = false;
            request.ContentType = "application/x-vermeer-urlencoded";
            request.Headers.Add("X-Vermeer-Content-Type", "application/x-vermeer-urlencoded");
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(streamHeader, 0, streamHeader.Length);
                content.CopyTo(reqStream);
                reqStream.Flush();
            }
            return request.GetResponse();
        }
        private void CheckForSuccessMessage(string response)
        {
            string message = GetReturnValue(response, "message");
            if (null == message || !message.StartsWith("successfully"))
            {
                throw new WebException("Failed to perform operation. Message:" + message);
            }
            mLogger.Info("Finished upload a large file by RPC");
        }
        private void CheckForInternalErrorMessage(string response)
        {
            string message = DecodeString(GetReturnValue(response, "msg"));
            if (!string.IsNullOrEmpty(message))
            {
                throw new WebException(message);
            }
        }

        private string DecodeString(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex("&#([0-9]{1,3});&#([0-9]{1,3});");
                foreach (System.Text.RegularExpressions.Match match in rg.Matches(source))
                {
                    byte[] bytes = new[] { byte.Parse(match.Groups[1].Value), byte.Parse(match.Groups[2].Value) };
                    source = source.Replace(match.Value, Encoding.UTF8.GetString(bytes));
                }
                source = System.Web.HttpUtility.HtmlDecode(source);
            }
            return source;
        }
        private string GetReturnValue(string response, string key)
        {
            key = key.TrimEnd('=') + "=";
            int startPos = response.IndexOf(key);
            if (-1 == startPos)
            {
                return
                null;
            }
            startPos += key.Length;
            int endPos = response.IndexOf("\n", startPos);
            return response.Substring(startPos, endPos - startPos);
        }

        #endregion

        #region  Update

        protected virtual bool NeedUpdateWebLogo(Dictionary<string, object> webProperties)
        {
            return webProperties.ContainsKey("SiteLogoUrl") || webProperties.ContainsKey("SiteLogoDescription");
        }

        private void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webNewProp, Dictionary<string, object> webProperties)
        {
            if (NeedUpdateWebLogo(webProperties))
            {
                if (webProperties.ContainsKey("SiteLogoUrl"))
                {
                    webNewProp["SiteLogoUrl"] = webProperties["SiteLogoUrl"];
                }
                if (webProperties.ContainsKey("SiteLogoDescription"))
                {
                    webNewProp["SiteLogoDescription"] = webProperties["SiteLogoDescription"];
                }
                //if (webProperties.ContainsKey("Name"))
                //{
                //    webNewProp["Name"] = webProperties["Name"];
                //}
                this.mRequestCommon.UpdateWebLogo(webServerRelativeUrl, webProperties);
            }
        }
        protected virtual void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp)
        {
            mRequestCommon.UpdateWebRegionalSetting(webServerRelativeUrl, regionalProp);
        }

        public override Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            bool needLoadNoCrawlProperty = false;
            Dictionary<string, object> webProp = new Dictionary<string, object>();
            UpdateWebLogo(webServerRelativeUrl, webProp, webProperties);
            if (webProperties.ContainsKey("NoCrawl") && webProperties.ContainsKey("ASPXPageIndexMode") && webProperties.ContainsKey("ExcludeFromOfflineClient"))
            {
                needLoadNoCrawlProperty = true;
                mRequestCommon.UpdateWebSearchAndOfflineAvailability(webServerRelativeUrl, webProperties);
                webProp["NoCrawl"] = webProperties["NoCrawl"];
                webProp["ASPXPageIndexMode"] = webProperties["ASPXPageIndexMode"];
                webProp["ExcludeFromOfflineClient"] = webProperties["ExcludeFromOfflineClient"];
            }
            if (webProperties.ContainsKey("RegionalSettingsChangedProperties"))
            {
                Dictionary<string, object> regionalProp = webProperties["RegionalSettingsChangedProperties"] as Dictionary<string, object>;
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                mRequestCommon.UpdateWebRegionalSetting(webServerRelativeUrl, regionalProp);
                newProp = this.GetWebRegionalSetting(webServerRelativeUrl);
                webProp["RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix] = newProp;
            }

            foreach (var property in UpdateWebAndGetProperties(webServerRelativeUrl, webProperties))
            {
                webProp[property.Key] = property.Value;
            }

            if (needLoadNoCrawlProperty)
            {
                //由于NoCrawl这个属性是用Web Service还原的，而又因为在AllProperties里面也有一个这样的字段，并且没有更新到webPro里面，所以加到里面。
                //注：在web.NoCrawl里面这个属性是bool类型，而在AllProperties里面这个字段是String类型。
                Dictionary<string, object> tempProp = new Dictionary<string, object>();
                if (webProp.ContainsKey("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    tempProp = webProp["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    if (tempProp.ContainsKey("NoCrawl"))
                    {
                        tempProp["NoCrawl"] = webProperties["NoCrawl"].ToString();
                    }
                }
            }
            return webProp;
        }
        private Dictionary<string, object> UpdateWebAndGetProperties(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);

                object newWebName; //ADO-153129：Name不是client web的基本属性，拼接为ServerRelativeUrl后，再更新
                if (webProperties.TryGetValue("Name", out newWebName))
                {
                    webProperties["ServerRelativeUrl"] = webServerRelativeUrl.Substring(0, webServerRelativeUrl.LastIndexOf('/') + 1) + newWebName.ToString();
                }
                AveObjectCopy.UpdateObjectBasicProperties(webProperties, web);

                bool changed = UpdateWebAccessRequestSetting(context, web, webProperties);
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
                UpdateSupportedUICulture(webProperties, web, ref changed);

                changed |= UpdateWebUserResource(web, webProperties);

                Dictionary<string, object> webPro = new Dictionary<string, object>();
                if (Convert.ToInt32(webProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0 || changed)
                {
                    web.Update();
                    if (webProperties.ContainsKey("ServerRelativeUrl") &&
                        webProperties["ServerRelativeUrl"] != null &&
                        !string.IsNullOrEmpty(webProperties["ServerRelativeUrl"].ToString()) &&
                        IsWebMoved(webServerRelativeUrl, webProperties["ServerRelativeUrl"].ToString()))
                    {
                        context.ExecuteQuery();
                    }
                    webPro = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                    if (newWebName != null)
                    {
                        webPro["Name"] = newWebName;
                    }
                }
                return webPro;
            }
        }

        protected virtual bool UpdateWebAccessRequestSetting(ClientContext context, Web web, Dictionary<string, object> webProperties)
        {
            return false;
        }

        protected virtual bool IsWebMoved(string originalServerRelativeUrl, string targetServerRelativeUrl)
        {
            return !originalServerRelativeUrl.Equals(targetServerRelativeUrl, StringComparison.InvariantCultureIgnoreCase);
        }

        public override Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties)
        {
            Dictionary<string, object> needAddProperties = new Dictionary<string, object>();
            if (siteProperties.ContainsKey("PortalUrl"))
            {
                Dictionary<string, object> portalProperties = new Dictionary<string, object>();
                if (siteProperties.ContainsKey("PortalName"))
                {
                    portalProperties.Add("PortalName", siteProperties["PortalName"]);
                    needAddProperties.Add("PortalName", siteProperties["PortalName"]);
                    siteProperties.Remove("PortalName");
                }
                if (siteProperties.ContainsKey("PortalUrl"))
                {
                    portalProperties.Add("PortalUrl", siteProperties["PortalUrl"]);
                    needAddProperties.Add("PortalUrl", siteProperties["PortalUrl"]);
                    siteProperties.Remove("PortalUrl");
                }
                mRequestCommon.UpdateSitePortal(portalProperties);
            }
            //if (siteProperties.ContainsKey("SyndicationEnabled"))
            //{
            //    mRequestCommon.UpdateSiteRssSetting(Convert.ToBoolean(siteProperties["SyndicationEnabled"]));
            //    needAddProperties.Add("SyndicationEnabled", siteProperties["SyndicationEnabled"]);
            //    siteProperties.Remove("SyndicationEnabled");
            //}
            if (siteProperties.Count > 0)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties = UpdateSiteProeprties(siteProperties);//原来的updateUser改成UpdateUserProperties
                foreach (string key in needAddProperties.Keys)
                {
                    if (!properties.ContainsKey(key))
                    {
                        properties.Add(key, needAddProperties[key]);
                    }
                }
                return properties;
            }
            return needAddProperties;
        }
        public Dictionary<string, object> UpdateSiteProeprties(Dictionary<string, object> siteProperties)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperty = new Dictionary<string, object>();
                Site site = context.Site;
                AveObjectCopy.UpdateObjectBasicProperties(siteProperties, site);
                site.RefreshLoad();
                context.Load(site);
                context.ExecuteQuery();
                siteProperty = GetSite();
                return siteProperty;
            }
        }

        protected virtual List<string> UpdateListNormalProperties
        {
            get
            {
                return new List<string> { "NoCrawl" };
            }
        }
        public override Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            int type = listProperties.ContainsKey("ListType") ? (int)listProperties["ListType"] : -1;
            listProperties.Remove("ListType");
            if (type == (int)AveListTemplateType.Survey)
            {
                Dictionary<string, object> dicPro = new Dictionary<string, object>();
                foreach (var property in UpdateListNormalProperties)
                {
                    if (listProperties.ContainsKey(property))
                    {
                        dicPro[property] = listProperties[property];
                        listProperties.Remove(property);
                    }
                }
                if (dicPro.Count > 0)
                {
                    base.UpdateList(webServerRelativeUrl, listName, listId, dicPro);
                }
                return mWebServiceRequest.UpdateList(webServerRelativeUrl, listName, listId, listProperties);
            }
            else
            {
                Dictionary<string, object> versionLimitedProperties = new Dictionary<string, object>();
                SetVersionSetting(versionLimitedProperties, listProperties);
                Dictionary<string, object> advancedSettingProp = new Dictionary<string, object>();
                SetAdvancedSetting(advancedSettingProp, listProperties);
                Dictionary<string, object> generalSettings = new Dictionary<string, object>();
                SetGeneralSetting(generalSettings, listProperties);
                using (ClientContext context = CreateContext())
                {
                    //code: "list.DocumentTemplateUrl = string.Empty;" works fine in server mode, we should make it work in client mode
                    context.ValidateOnClient = false;
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = null;
                    if (listId != Guid.Empty)
                    {
                        list = web.Lists.GetById(listId);
                    }
                    else
                    {
                        list = web.Lists.GetByTitle(listName);
                    }
                    //API都支持的Setting，可以不需要走反射
                    //某些list setting是需要在开启version的情况下才能设置的，例如：MajorVersionLimit，提前设置list version setting [ADO-159059]
                    object obj = null;
                    if (listProperties.TryGetValue("EnableModeration", out obj))
                    {
                        list.EnableModeration = (bool)obj;
                        listProperties.Remove("EnableModeration");
                    }
                    if (listProperties.TryGetValue("EnableVersioning", out obj))
                    {
                        list.EnableVersioning = (bool)obj;
                        listProperties.Remove("EnableVersioning");
                    }
                    if (listProperties.TryGetValue("EnableMinorVersions", out obj))
                    {
                        list.EnableMinorVersions = (bool)obj;
                        listProperties.Remove("EnableMinorVersions");
                    }
                    AveObjectCopy.UpdateObjectBasicProperties(listProperties, list);
                    Dictionary<string, object> newProp = new Dictionary<string, object>();
                    UpdateListUserResource(list, listProperties);
                    list.Update();
                    this.LoadList(context, list);
                    AveObjectCopy.GetObjectBasicProperties(newProp, list);
                    if (versionLimitedProperties.Count > 0)
                    {
                        mRequestCommon.SetListVersionLimited(webServerRelativeUrl, listId, versionLimitedProperties);
                        Dictionary<string, object> listVersionLimitedDic = mRequestCommon.GetListVersionLimited(webServerRelativeUrl, listId);
                        if (listVersionLimitedDic.ContainsKey("MajorVersionLimit"))
                        {
                            newProp["MajorVersionLimit"] = listVersionLimitedDic["MajorVersionLimit"];
                        }
                        if (listVersionLimitedDic.ContainsKey("MajorWithMinorVersionsLimit"))
                        {
                            newProp["MajorWithMinorVersionsLimit"] = listVersionLimitedDic["MajorWithMinorVersionsLimit"];
                        }
                    }
                    if (advancedSettingProp.Count > 0)
                    {
                        mRequestCommon.UpdateListAdvancedSetting(webServerRelativeUrl, listId, advancedSettingProp);
                    }
                    if (generalSettings.Count > 0)
                    {
                        mRequestCommon.UpdateListGeneralSetting(webServerRelativeUrl, listId, generalSettings);
                    }
                    return newProp;
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is property name")]
        protected override void LoadFolderProperties(AveClientContext context, string webServerRelativeUrl, Guid listId, Folder newFolder, Dictionary<string, object> folderProps)
        {
            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
            using (excepScope.StartScope())
            {
                using (excepScope.StartTry())
                {
                    context.Load(newFolder);
                    context.Load(newFolder.Properties);
                    context.Load(newFolder.ListItemAllFields);
                    context.Load(newFolder.ListItemAllFields, item => item.HasUniqueRoleAssignments);
                }
                using (excepScope.StartCatch())
                {
                    context.Load(newFolder);
                    context.Load(newFolder.Properties);
                }
            }
            context.ExecuteQuery();
            CopyProperty(folderProps, newFolder);
            folderProps["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = newFolder.Properties.FieldValues;
            if (newFolder.ListItemAllFields.IsPropertyAvailable("Id"))
            {
                Dictionary<string, object> itemProps = new Dictionary<string, object>();
                GetItemDic(itemProps, newFolder.ListItemAllFields);
                folderProps["UniqueId"] = itemProps["UniqueId"];
                folderProps["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProps;
            }
            else if (newFolder.Properties.FieldValues.ContainsKey("vti_etag") &&
                     newFolder.Properties["vti_etag"] != null)
            {
                string tagString = newFolder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                folderProps["UniqueId"] = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
            }
        }

        protected override void UpdateTranslationColumnsSetting(AveClientContext context, ClientFolder folder, string propertyValue)
        {
            try
            {
                List<string> fieldIdList = new List<string>();
                XmlDocument document = new XmlDocument();
                document.LoadXml(propertyValue);
                XmlNodeList nodeList = document.SelectNodes("/Fields/Field");
                foreach (XmlNode fieldIdNode in nodeList)
                {
                    fieldIdList.Add("{" + fieldIdNode.Attributes[0].Value + "}");
                }
                context.Load(folder, f => f.Properties);
                context.ExecuteQuery();

                string fieldsXml = folder.Properties["{00888AA8-0AEA-49B9-8A7B-09ABE0D5A9BF}"].ToString();//这个属性控制Translable Columns的勾选
                XmlDocument fieldsXmlDoc = new XmlDocument();
                fieldsXmlDoc.LoadXml(fieldsXml);
                XmlNode node = fieldsXmlDoc.SelectSingleNode(".//*[name()='InheritanceSource']");
                XmlNodeList desNodeList = node.ChildNodes;

                foreach (XmlNode subNode in desNodeList)
                {
                    string id = subNode.Attributes["id"].Value;
                    if (fieldIdList.Contains(id))
                    {
                        fieldIdList.Remove(id);
                    }
                }
                foreach (string needInsertId in fieldIdList)
                {
                    XmlAttribute attribute = fieldsXmlDoc.CreateAttribute("id");
                    attribute.Value = needInsertId;

                    XmlNode newNode = fieldsXmlDoc.CreateElement("Field");
                    newNode.Attributes.Append(attribute);
                    node.AppendChild(newNode);
                }
                folder.Properties["{00888AA8-0AEA-49B9-8A7B-09ABE0D5A9BF}"] = fieldsXmlDoc.OuterXml.Replace("xmlns=\"\"", "");//添加节点会自动添加xmlns="" ,导致更新失败
                folder.Update();
            }
            catch (Exception e)
            {
                mLogger.Error("Update Translation Packages Find Error: {0}", e.Message);
            }
        }

        protected override void SetFolderPropertyValues(ClientFolder folder, Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, object> tempPair in properties)
            {
                folder.Properties[tempPair.Key] = tempPair.Value;
            }
        }

        protected override void SetEditorReadOnly(List list, bool readOnly) {/*do nothing.or 2013 will throw exception when add a file version.*/}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of Value")]
        protected virtual void SetAdvancedSetting(Dictionary<string, object> advancedSettingProp, Dictionary<string, object> listProperties)
        {
            if (mCompatibilityLevel == 15)
            {
                if (listProperties.ContainsKey("EnableAssignToEmail"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$TasksIssuesEmailSettingsSection$ctl01$EnableAssignToEmail"] = (bool)listProperties["EnableAssignToEmail"] ? "RadEnableAssigntoEmailYes" : "RadEnableAssigntoEmailNo";
                    listProperties.Remove("EnableAssigntoEmail");
                }
                if (listProperties.ContainsKey("ExcludeFromOfflineClient"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$AllowSyncSection$ctl02$AllowSync"] = (bool)listProperties["ExcludeFromOfflineClient"] ? "RadAllowSyncNo" : "RadAllowSyncYes";
                    listProperties.Remove("ExcludeFromOfflineClient");
                }
                if (listProperties.ContainsKey("DisableGridEditing"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl02$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                    listProperties.Remove("DisableGridEditing");
                }
                if (listProperties.ContainsKey("NavigateForFormsPages"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl03$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                    listProperties.Remove("NavigateForFormsPages");
                }
                if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl02$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                    listProperties.Remove("IsSiteAssetsLibrary");
                }
                if (listProperties.ContainsKey("DefaultItemOpenUseListSetting") && !(bool)listProperties["DefaultItemOpenUseListSetting"])
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = "RadDefaultItemOpenServerSetting";
                    listProperties.Remove("DefaultItemOpenUseListSetting");
                }
                else if (listProperties.ContainsKey("DefaultItemOpen"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                    listProperties.Remove("DefaultItemOpen");
                }
                if (listProperties.ContainsKey("SendToLocationName"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                    listProperties.Remove("SendToLocationName");
                }
                if (listProperties.ContainsKey("SendToLocationUrl"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl02$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                    listProperties.Remove("SendToLocationUrl");
                }
                if (listProperties.ContainsKey("EnableManagedIndexes"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$ManagedIndexesSection$ctl02$AllowManagedIndex"] = (bool)listProperties["EnableManagedIndexes"] ? "RadManagedIndexesYes" : "RadManagedIndexesNo";
                    listProperties.Remove("EnableManagedIndexes");
                }
            }
            else
            {
                if (listProperties.ContainsKey("ExcludeFromOfflineClient"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$AllowSyncSection$ctl01$AllowSync"] = (bool)listProperties["ExcludeFromOfflineClient"] ? "RadAllowSyncNo" : "RadAllowSyncYes";
                    listProperties.Remove("ExcludeFromOfflineClient");
                }
                if (listProperties.ContainsKey("DisableGridEditing"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl01$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                    listProperties.Remove("DisableGridEditing");
                }
                if (listProperties.ContainsKey("NavigateForFormsPages"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl02$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                    listProperties.Remove("NavigateForFormsPages");
                }
                if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl01$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                    listProperties.Remove("IsSiteAssetsLibrary");
                }
                if (listProperties.ContainsKey("DefaultItemOpenUseListSetting") && !(bool)listProperties["DefaultItemOpenUseListSetting"])
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = "RadDefaultItemOpenServerSetting";
                    listProperties.Remove("DefaultItemOpenUseListSetting");
                }
                else if (listProperties.ContainsKey("DefaultItemOpen"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                    listProperties.Remove("DefaultItemOpen");
                }
                if (listProperties.ContainsKey("SendToLocationName"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl00$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                    listProperties.Remove("SendToLocationName");
                }
                if (listProperties.ContainsKey("SendToLocationUrl"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                    listProperties.Remove("SendToLocationUrl");
                }
                if (listProperties.ContainsKey("EnableManagedIndexes"))
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$ManagedIndexesSection$ctl02$AllowManagedIndex"] = (bool)listProperties["EnableManagedIndexes"] ? "RadManagedIndexesYes" : "RadManagedIndexesNo";
                    listProperties.Remove("EnableManagedIndexes");
                }
            }
            if (listProperties.ContainsKey("EnableAttachments"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentsSection$ctl02$DisableAttachments"] = (bool)listProperties["EnableAttachments"] ? "RadAttachmentsEnabled" : "RadAttachmentsDisabled";
                listProperties.Remove("EnableAttachments");
            }
            if (listProperties.ContainsKey("ReadSecurity"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$ReadSecurity"] = listProperties["ReadSecurity"];
                listProperties.Remove("ReadSecurity");
            }
            if (listProperties.ContainsKey("WriteSecurity"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl10$WriteSecurity"] = listProperties["WriteSecurity"];
                listProperties.Remove("WriteSecurity");
            }
        }
        protected virtual void SetVersionSetting(Dictionary<string, object> versionLimitedProperties, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("MajorVersionLimit"))
            {
                versionLimitedProperties.Add("MajorVersionLimit", listProperties["MajorVersionLimit"]);
                listProperties.Remove("MajorVersionLimit");
            }
            if (listProperties.ContainsKey("MajorWithMinorVersionsLimit"))
            {
                versionLimitedProperties.Add("MajorWithMinorVersionsLimit", listProperties["MajorWithMinorVersionsLimit"]);
                listProperties.Remove("MajorWithMinorVersionsLimit");
            }
        }
        protected void SetGeneralSetting(Dictionary<string, object> generalSettings, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("EnablePeopleSelector"))
            {
                generalSettings["ctl00$PlaceHolderMain$EventSection$ctl01$enablePeopleSelector"] = (bool)listProperties["EnablePeopleSelector"] ? "RadEnablePeopleSelectorYes" : "RadEnablePeopleSelectorNo";
                listProperties.Remove("EnablePeopleSelector");
            }
        }


        public override Dictionary<string, object> UpdateEventReceiver(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId, Dictionary<string, object> needUpdateEventReceiverProperties)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiverProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                EventReceiverDefinition eventReceiverDefinition = null;
                switch (eventReceiverDefSource)
                {
                    case "web.eventReceivers":
                        eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                        break;
                    case "list.eventReceivers":
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        eventReceiverDefinition = list.EventReceivers.GetById(eventReceiverDefId);
                        break;
                    default:
                        eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                        break;
                }
                if (eventReceiverDefinition != null)
                {
                    AveObjectCopy.UpdateObjectBasicProperties(needUpdateEventReceiverProperties, eventReceiverDefinition);
                    eventReceiverDefinition.Update();
                    context.Load(eventReceiverDefinition);
                    context.ExecuteQuery();
                    AveObjectCopy.GetObjectBasicProperties(eventReceiverProperties, eventReceiverDefinition);
                }
                return eventReceiverProperties;
            }
        }

        public override Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            var result = base.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties);
            mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties);
            return result;
        }

        public override bool UpdateFieldLinkProperties(ClientContext context, ContentType contentType, Dictionary<string, object> needUpdateContentTypeProperties, bool updateChildren)
        {
            bool changed = base.UpdateFieldLinkProperties(context, contentType, needUpdateContentTypeProperties, updateChildren);

            if (needUpdateContentTypeProperties.ContainsKey("Reorder"))
            {
                contentType.FieldLinks.Reorder(((List<string>)needUpdateContentTypeProperties["Reorder"]).ToArray());
                changed = true;
            }

            return changed;
        }

        public override Dictionary<string, object> UpdateTermStore(Guid termStoreId, int termStoreDefaultLanguage, Dictionary<string, object> needUpdateProperties)
        {
            Dictionary<string, object> TermStoreProp = new Dictionary<string, object>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore termStore = session.TermStores.GetById(termStoreId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateProperties, termStore);
                if (Convert.ToInt32(needUpdateProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.Load(termStore);
                    context.ExecuteQuery();
                    AveObjectCopy.GetObjectBasicProperties(TermStoreProp, termStore);
                }
                Dictionary<string, object> GroupsProperties = new Dictionary<string, object>();
                Dictionary<string, object> GroupList = new Dictionary<string, object>();
                if (needUpdateProperties.ContainsKey("GroupActions"))
                {
                    var groupActions = needUpdateProperties["GroupActions"] as Dictionary<Guid, Dictionary<string, object>>;
                    foreach (var groupAction in groupActions)
                    {
                        Dictionary<string, object> groupProperties = null;
                        TermGroup group = null;
                        //Create Group
                        if (groupAction.Value.ContainsKey("CreateGroup"))
                        {
                            group = termStore.CreateGroup(groupAction.Value["CreateGroup"] as string, groupAction.Key);
                        }

                        if (groupAction.Value.ContainsKey("UpdateGroup"))
                        {
                            bool needLoadGroupProperties = false;
                            if (group == null)
                            {
                                group = termStore.GetGroup(groupAction.Key);
                            }
                            Dictionary<string, object> needUpdateGroupProperties = groupAction.Value["UpdateGroup"] as Dictionary<string, object>;
                            if (needUpdateGroupProperties.ContainsKey("DeleteGroup"))
                            {
                                context.Load(group.TermSets, TermSet => TermSet.Include());//Do not need load properties
                                context.ExecuteQuery();

                                if (group.TermSets.Count > 0)
                                {
                                    foreach (TermSet set in group.TermSets)
                                    {
                                        set.DeleteObject();
                                    }
                                }
                                group.DeleteObject();
                                context.ExecuteQuery();
                                continue;
                            }

                            AveObjectCopy.UpdateObjectBasicProperties(needUpdateGroupProperties, group);
                            if (UpdateTermGroupUserInfo(group, needUpdateGroupProperties) || Convert.ToInt32(needUpdateGroupProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                            {
                                group.TermStore.CommitAll();
                                context.Load(group);
                                needLoadGroupProperties = true;
                            }
                            if (needUpdateGroupProperties.ContainsKey("TermSetActions"))
                            {
                                groupProperties = UpdateGroupTermSet(context, group, termStoreDefaultLanguage, needUpdateGroupProperties);
                                if (needLoadGroupProperties)
                                {
                                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                                }
                            }
                        }
                        if (groupProperties == null)
                        {
                            groupProperties = new Dictionary<string, object>();
                            context.Load(group);
                            context.ExecuteQuery();
                            AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                        }

                        GroupList.Add(groupAction.Key.ToString(), groupProperties);
                    }
                }
                GroupsProperties["Group"] = GroupList;
                TermStoreProp.Add(termStoreId.ToString(), GroupsProperties);
            }
            return TermStoreProp;
        }

        public virtual bool UpdateTermGroupUserInfo(TermGroup group, Dictionary<string, object> needUpdateGroupProperties)
        {
            return false;
        }


        public override Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp)
        {
            bool updateAdminOnly = false;
            if (userProp.ContainsKey("OldAdministrators") && userProp.ContainsKey("NewAdministrators") && userProp.ContainsKey("IsSiteAdmin"))
            {
                if (userProp.Count == 3)
                {
                    updateAdminOnly = true;
                }
                if (userProp.ContainsKey("IsSiteAdmin"))
                {
                    UpdateUserSiteAdmin(webServerRelativeUrl, loginName, Convert.ToBoolean(userProp["IsSiteAdmin"]));
                }
            }
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            return mWebServiceRequest.GetUserProperties(webFullUrl, loginName, name, updateAdminOnly, userProp);
        }

        private void UpdateUserSiteAdmin(string webServerRelativeUrl, string loginName, bool isSiteAdmin)
        {
            using (ClientContext cc = CreateContext())
            {
                User user = cc.Site.RootWeb.SiteUsers.GetByLoginName(loginName);
                user.IsSiteAdmin = isSiteAdmin;
                user.Update();
                cc.ExecuteQuery();
            }
        }

        public override void RevertAllDocumentContentStreams(string webServerRelativeUrl)
        {

        }
        public override void RevertContentStream(string webServerRelativeUrl, string fileUrl)
        {

        }
        public override void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            mRequestCommon.UpdateSiteRssSetting(syndicationEnabled);
        }
        public override Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            return mRequestCommon.UpdateKeyWord(term, localId, calendarType, keyWordProp);
        }

        public override void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            if (moveMethodName.Equals("MoveToCollection"))
            {//SAAS-611
                this.mRequestCommon.MoveNavigationNodeToCollection(webServerRelativeUrl, navigationNodeProperties);
            }
            else
            {
                this.mRequestCommon.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
            }
        }

        public override void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            using (ClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.ApplyTheme(colorPaletteUrl, fontSchemeUrl, backgroundImageUrl, shareGenerated);
                context.ExecuteQuery();
            }
        }

        //该用CSOM api，其他两个属性在Site 上更新
        public override Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            if (needUpdateProperties.ContainsKey("AuditFlags"))
            {
                int auditFlags = (int)needUpdateProperties["AuditFlags"];
                using (var context = CreateContext(mWebUrl))
                {
                    context.Site.Audit.AuditFlags = (AuditMaskType)auditFlags;
                    context.Site.Audit.Update();
                    context.ExecuteQuery();
                }
            }
            return needUpdateProperties;
        }
        #endregion

        #region Delete
        public override void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            if (string.IsNullOrEmpty(webServerRelativeUrl))
            {
                base.DeleteRecycleItem(id, webServerRelativeUrl);
                return;
            }
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    context.Site.OpenWeb(webServerRelativeUrl).RecycleBin.GetById(id).DeleteObject();
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.DeleteRecycleBinItemFailed, context.Url, e.ToString());
                    throw;
                }
            }
        }

        public override void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            mRequestCommon.OperateOnVersion(webServerRelativeUrl, webAppName, obj, listUrl, itemId, versionId, listId, fileName, op);
            //string url = webAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/') + "/_layouts/15/Versions.aspx?";
            //AveHttpWebRequestCommon.OperateOnVersion(url, webAppName, obj, listUrl, itemId, versionId, listId, fileName, op);
        }

        public override void DeleteAttachment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid webId, Guid listId, int rowId, string attachmentName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ListItem item = list.GetItemById(rowId);
                if (item != null)
                {
                    var attachment = item.AttachmentFiles.GetByFileName(attachmentName);
                    attachment.DeleteObject();
                    context.ExecuteQuery();
                }
            }
        }


        public override void DeleteEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> eventReceiverProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                EventReceiverDefinition eventReceiverDefinition = null;
                switch (eventReceiverDefSource)
                {
                    case "web.eventReceivers":
                        eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                        break;
                    case "list.eventReceivers":
                        List list = null;
                        if (listId != Guid.Empty)
                        {
                            list = web.Lists.GetById(listId);
                        }
                        else
                        {
                            list = web.Lists.GetByTitle(listTitle);
                        }
                        eventReceiverDefinition = list.EventReceivers.GetById(eventReceiverDefId);
                        break;
                    default:
                        eventReceiverDefinition = web.EventReceivers.GetById(eventReceiverDefId);
                        break;
                }

                eventReceiverDefinition.DeleteObject();
                context.ExecuteQuery();
            }
        }

        public override void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName)
        {
            using (AveClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (source)
                {
                    case "group.users":
                        web.SiteGroups.GetByName(groupName).Users.RemoveByLoginName(loginName);
                        context.ExecuteQuery();
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteAdministrators":
                    case "web.siteUsers":
                        web.SiteUsers.RemoveByLoginName(loginName);
                        context.ExecuteQuery();
                        break;
                    default:
                        break;
                }
            }
        }

        public override void DeleteTag(string url, Guid termId)
        {
            mWebServiceRequest.DeleteTag(url, termId);
        }

        #endregion

        #region Restore
        public override void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            if (string.IsNullOrEmpty(webServerRelativeUrl))
            {
                base.RestoreRecycleItem(id, webServerRelativeUrl);
                return;
            }
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    context.Site.OpenWeb(webServerRelativeUrl).RecycleBin.GetById(id).Restore();
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.RestoreRecycleBinItemFailed, context.Url, e.ToString());
                    throw;
                }
            }
        }

        public override void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, IList webpartBaseInfoList, AveWebPartCache mapping, bool clearAll, IAveWeb web, IReport report)
        {
            using (ClientContext context = CreateContext(web.Url))//AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl)
            {
                using (Ave2013WebPartRestore webpartRestore = new Ave2013WebPartRestore(webServerRelativeUrl, listTitle, listId, fileServerRelativeUrl, scope, clearAll, context, mapping, web, report, mObj))
                {
                    //webpartRestore.RestoreWebParts(webpartBaseInfoList);
                    webpartRestore.RestoreWebParts(webpartRestore.GetNeedRestoreWebParts(webpartBaseInfoList, clearAll));
                }
            }
        }

        public override Dictionary<string, object> AddUserProfile(string accountName)
        {
            return this.mWebServiceRequest.AddUserProfile(accountName);
        }

        public override void AddDocumentsetVersion(string webRelativeUrl, string listTitle, int itemId, bool isMajor, string comment)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                List list = web.Lists.GetByTitle(listTitle);
                ListItem folder = list.GetItemById(itemId);
                context.Load(web.CurrentUser);
                context.Load(folder);
                context.Load(folder.ContentType.FieldLinks);
                context.Load(folder.Folder.Properties);
                context.Load(folder.Folder.Files, fs => fs.Include(f => f.ListItemAllFields));
                context.ExecuteQuery();

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root;
                XmlElement items;
                XmlElement snapshots;
                List<string> oldItemsGuid;
                CheckFirstVersion(xmlDoc, folder, out root, out items, out snapshots, out oldItemsGuid);
                DocumentSetVersionItems(folder, root, items, oldItemsGuid);
                DocumentSetVersionSnapshot(web, folder, root, snapshots, isMajor, comment);
                root.Attributes["NextSnapshotNumber"].Value = (int.Parse(root.Attributes["NextSnapshotNumber"].Value) + 1).ToString();

                folder.Folder.Properties["snapshots"] = xmlDoc.OuterXml;
                folder.Folder.Update();
                context.ExecuteQuery();
            }
        }

        protected void CheckFirstVersion(XmlDocument xmlDoc, ListItem folder, out XmlElement root, out XmlElement items, out XmlElement snapshots, out List<string> oldItemsGuid)
        {
            oldItemsGuid = new List<string>();
            if (folder.Folder.Properties.FieldValues.ContainsKey("snapshots"))
            {
                string originalXml = folder.Folder.Properties["snapshots"].ToString();
                xmlDoc.LoadXml(originalXml);
                root = xmlDoc.SelectSingleNode("//SnapshotCollection") as XmlElement;
                items = xmlDoc.SelectSingleNode("//Items") as XmlElement;
                snapshots = xmlDoc.SelectSingleNode("//Snapshots") as XmlElement;
                XmlNodeList oldItems = xmlDoc.SelectNodes("//Items/Item");
                foreach (XmlNode node in oldItems)
                {
                    oldItemsGuid.Add(node.Attributes["Guid"].Value);
                }
            }
            //First Version
            else
            {
                root = xmlDoc.CreateElement("SnapshotCollection");
                items = xmlDoc.CreateElement("Items");
                root.AppendChild(items);
                snapshots = xmlDoc.CreateElement("Snapshots");
                root.AppendChild(snapshots);
                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes["NextSnapshotNumber"] = "1";
                attributes["NextInternalId"] = "1";
                AppendAttributes(root, attributes);
                xmlDoc.AppendChild(root);
            }
        }

        protected void DocumentSetVersionItems(ListItem folder, XmlElement root, XmlElement items, List<string> oldItemsGuid)
        {
            int fileId = int.Parse(root.Attributes["NextInternalId"].Value);
            foreach (Microsoft.SharePoint.Client.File file in folder.Folder.Files)
            {
                if (oldItemsGuid.Count > 0 && oldItemsGuid.Contains(file.ListItemAllFields.FieldValues["UniqueId"].ToString()))
                {
                    continue;
                }
                XmlElement item = root.OwnerDocument.CreateElement("Item");
                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes["Id"] = fileId.ToString();
                attributes["Guid"] = file.ListItemAllFields.FieldValues["UniqueId"].ToString();
                attributes["Url"] = file.ListItemAllFields.FieldValues["FileLeafRef"].ToString();
                attributes["LinkToDoc"] = Boolean.FalseString;
                AppendAttributes(item, attributes);
                items.AppendChild(item);
                fileId++;
            }
            root.ReplaceChild(items, items);
            root.Attributes["NextInternalId"].Value = fileId.ToString();
        }

        private void AppendAttributes(XmlNode node, Dictionary<string, string> attributes)
        {
            if (attributes == null || attributes.Count <= 0)
            {
                return;
            }
            foreach (KeyValuePair<string, string> tempAttribute in attributes)
            {
                XmlAttribute attribute = node.OwnerDocument.CreateAttribute(tempAttribute.Key);
                attribute.Value = tempAttribute.Value;
                node.Attributes.Append(attribute);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DateTime format string. ")]
        protected void DocumentSetVersionSnapshot(Web web, ListItem folder, XmlElement root, XmlElement snapshots, bool isMajor, string comment)
        {
            XmlElement snapshot = root.OwnerDocument.CreateElement("Snapshot");
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes["Label"] = root.Attributes["NextSnapshotNumber"].Value.ToString();
            attributes["Major"] = isMajor.ToString();
            attributes["Created"] = DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");
            attributes["By"] = web.CurrentUser.LoginName;
            AppendAttributes(snapshot, attributes);
            XmlElement comments = root.OwnerDocument.CreateElement("Comments");
            comments.InnerText = comment;
            snapshot.AppendChild(comments);
            DocumentSetVersionFields(folder, snapshot);
            DocumentSetVersionSnapshotItems(folder, snapshot, isMajor);
            snapshots.InnerXml = snapshot.OuterXml + snapshots.InnerXml;
            root.AppendChild(snapshots);
        }

        protected void DocumentSetVersionFields(ListItem folder, XmlElement snapshot)
        {
            List<string> needSkipFields = new List<string>() { "ContentType", "ItemChildCount", "FolderChildCount" };
            Dictionary<string, Guid> fieldsInfo = new Dictionary<string, Guid>();
            XmlElement fields = snapshot.OwnerDocument.CreateElement("Fields");
            foreach (FieldLink fieldlink in folder.ContentType.FieldLinks)
            {
                if (!needSkipFields.Contains(fieldlink.Name))
                {
                    fieldsInfo[fieldlink.Name] = fieldlink.Id;
                }
            }
            //DocumentSet默认的contenttype中自带的field
            if (fieldsInfo.ContainsKey("Title"))
            {
                DocumentSetVersionField(fields, folder, "Title", fieldsInfo);
            }
            if (fieldsInfo.ContainsKey("DocumentSetDescription"))
            {
                DocumentSetVersionField(fields, folder, "DocumentSetDescription", fieldsInfo);
            }
            if (fieldsInfo.ContainsKey("FileLeafRef"))
            {
                DocumentSetVersionField(fields, folder, "FileLeafRef", fieldsInfo);
            }
            //如果有额外添加的Field存在
            if (fieldsInfo.Count > 0)
            {
                //foreach (KeyValuePair<string, Guid> pair in fieldsInfo)
                //{
                //    DocumentSetVersionField(fields, folder, pair.Key, fieldsInfo);
                //}
                List<string> list = fieldsInfo.Keys.ToList<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    DocumentSetVersionField(fields, folder, list[i], fieldsInfo);
                }
            }
            snapshot.AppendChild(fields);
        }

        protected void DocumentSetVersionField(XmlElement fields, ListItem folder, string fieldName, Dictionary<string, Guid> fieldsInfo)
        {
            XmlElement field = fields.OwnerDocument.CreateElement("Field");
            XmlAttribute id = fields.OwnerDocument.CreateAttribute("Id");
            id.Value = fieldsInfo[fieldName].ToString();
            field.Attributes.Append(id);
            if (folder.FieldValues.ContainsKey(fieldName) && folder[fieldName] != null)
            {
                field.InnerText = folder[fieldName].ToString();
            }
            else
            {
                field.InnerText = string.Empty;
            }
            fields.AppendChild(field);
            fieldsInfo.Remove(fieldName);
        }

        protected void DocumentSetVersionSnapshotItems(ListItem folder, XmlElement snapshot, bool isMajor)
        {
            XmlElement snapshotItems = snapshot.OwnerDocument.CreateElement("SnapshotItems");
            if (!isMajor)
            {
                foreach (Microsoft.SharePoint.Client.File f in folder.Folder.Files)
                {
                    XmlElement snapshotItem = snapshot.OwnerDocument.CreateElement("SnapshotItem");
                    Dictionary<string, string> attributes = new Dictionary<string, string>();
                    XmlNode itemNode = snapshot.OwnerDocument.SelectSingleNode("//Items/Item[@Guid=\'" + f.ListItemAllFields["UniqueId"].ToString() + "\']");
                    if (itemNode != null)
                    {
                        attributes["Id"] = itemNode.Attributes["Id"].Value;
                    }
                    attributes["Version"] = f.ListItemAllFields["_UIVersionString"].ToString();
                    AppendAttributes(snapshotItem, attributes);

                    snapshotItems.AppendChild(snapshotItem);
                }
            }
            snapshot.AppendChild(snapshotItems);
        }

        public override Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo, bool isOnlineSite, bool isExistSkip)
        {
            var userName = userProfileInfo["LoginName"].ToString();
            var profile = mWebServiceRequest.GetUserProfile(userName);
            if (profile == null)
            {
                CreateUserProfile(userName);
            }
            else if (isExistSkip)
            {
                return new Dictionary<string, object>();
            }
            return this.mWebServiceRequest.RestoreUserProfileInfo(userProfileInfo, isOnlineSite, isExistSkip);
        }

        protected virtual void CreateUserProfile(string userName)
        {
        }

        public override Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (Ave2013ListItemRestore listItemRestore = new Ave2013ListItemRestore(this, site, context, mObj))
                {
                    return listItemRestore.RestoreListItem(data, userData, AddItemMapping);
                }
            }
        }

        public override Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (Ave2013FolderRestore folderRestore = new Ave2013FolderRestore(this, site, context, mObj))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }

        public override Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        {
            string oldWebUrl = string.Empty;
            if (!string.IsNullOrEmpty(info.ParentWebRelativeUrl) && !string.IsNullOrEmpty(this.mWebUrl) && this.mWebUrl.Contains("/sites"))
            {
                oldWebUrl = this.mWebUrl;
                this.mWebUrl = string.Format("{0}{1}", this.mWebUrl.Substring(0, this.mWebUrl.IndexOf("/sites", StringComparison.OrdinalIgnoreCase)), info.ParentWebRelativeUrl);
            }
            try
            {
                using (AveClientContext context = base.CreateContext())
                {
                    Site site = context.Site;
                    using (var documentRestore = new Ave2013DocumentRestore(this, site, mObj, context, mServerVersion, report))
                    {
                        return documentRestore.RestoreDocument(info, fileStream); ;
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(oldWebUrl))
                {
                    this.mWebUrl = oldWebUrl;
                }
            }
        }

        public override ListItem InternUpdate(List list, ListItem item, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        {
            MethodInfo updateMethod = typeof(ListItem).GetMethod("ValidateUpdateListItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
            Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
            bool isCurrentCheckOut = itemProperties.ContainsKey("IsCurrentCheckOut") ? (bool)itemProperties["IsCurrentCheckOut"] : false;
            bool changed = AveListItemRestore.SetFieldValues(item, itemFieldValues);
            if (changed)
            {
                string itemTitle = itemFieldValues.ContainsKey("FileLeafRef") ? itemFieldValues["FileLeafRef"] as string : string.Empty;
                itemFieldValues.Remove("FileLeafRef");
                IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
                values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = itemTitle });
                // ADO-169105 office文件的EnterpriseKeyword使用更新column的方法无法更新成功，需要使用ValidateUpdateListItem来更新。
                if (itemFieldValues.ContainsKey("TaxKeyword"))
                {
                    string taxKeyword = itemFieldValues["TaxKeyword"] as string;
                    itemFieldValues.Remove("TaxKeyword");
                    values.Add(new ListItemFormUpdateValue() { FieldName = "TaxKeyword", FieldValue = taxKeyword });
                }
                if (updateMethod.GetParameters().Length == 3)
                {
                    updateMethod.Invoke(item, new object[] { values, !isCurrentCheckOut, string.Empty });
                }
                else if (updateMethod.GetParameters().Length == 4)
                {
                    updateMethod.Invoke(item, new object[] { values, !isCurrentCheckOut, string.Empty, true });
                }
                else
                {
                    updateMethod.Invoke(item, new object[] { values, !isCurrentCheckOut });
                }
                list.Context.Load(item);
                list.Context.Load(item, it => it.HasUniqueRoleAssignments);
            }

            return item;
        }

        public override Dictionary<string, object> RestoreAttachment(Dictionary<string, object> data, Dictionary<string, object> userData, Stream fileStream)
        {
            using (AveClientContext context = base.CreateContext())
            {
                using (Ave2013AttachmentRestore attachmentRestore = new Ave2013AttachmentRestore(this, context, mObj))
                {
                    return attachmentRestore.RestoreAttachment(data, fileStream);
                }
            }
        }

        public override List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                return mRequestCommon.RestoreFeatures(webServerRelativeUrl, force, scope, featuresSource, featureInfoList, context, web);
            }
        }

        public override bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            return mRequestCommon.RestoreNavigation(webServerRelativeUrl, nodes, webAllProperties);
        }

        public override void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                ValidMasterPageProperties(context, context.Site.RootWeb, pageInfo);
                mRequestCommon.RestoreMasterPage(webServerRelativeUrl, siteServerRelativeUrl, pageInfo, alternateCssUrl);
            }
        }

        private void ValidMasterPageProperties(AveClientContext context, Web web, AveWebMasterPageInfo pageInfo)
        {
            mLogger.Debug("Begin to validate master page information: MasterPageUrl: {0}, CustomMasterPageUrl: {1} ", pageInfo.MPageUrl, pageInfo.CPageUrl);
            try
            {
                if (!IsFileExist(context, web, pageInfo.MPageUrl))
                {
                    pageInfo.MPageUrl = string.Empty;
                }
                if (!IsFileExist(context, web, pageInfo.CPageUrl))
                {
                    pageInfo.CPageUrl = string.Empty;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while get master page.MasterPage:{0},CustomMasterPage:{1} Error: {2}", pageInfo.MPageUrl, pageInfo.CPageUrl, e);
                pageInfo.MPageUrl = string.Empty;
                pageInfo.CPageUrl = string.Empty;
            }
            finally
            {
                mLogger.Debug("Finish validate master page information: MasterPageUrl: {0}, CustomMasterPageUrl: {1}", pageInfo.MPageUrl, pageInfo.CPageUrl);
            }
        }

        private bool IsFileExist(AveClientContext context, Web web, string fileServerRelativeUrl)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(fileServerRelativeUrl))
            {
                try
                {
                    ClientFile file = web.GetFileByServerRelativeUrl(fileServerRelativeUrl);
                    context.Load(file, f => f.Exists);
                    context.ExecuteQuery();
                    if (file.IsPropertyAvailable("Exists"))
                    {
                        result = file.Exists;
                    }
                }
                catch (Exception e)
                {
                    result = false;
                    mLogger.Warn("An error occurred while checking file existence.Url:{0},Error:{1}.", fileServerRelativeUrl, e);
                }
            }
            mLogger.Debug("Finish check file existence.Url:{0},Exist:{1}", fileServerRelativeUrl, result);
            return result;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of ViewXml")]
        public override void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            try
            {
                if (webSettingInfo.ThemedTitle != null && webSettingInfo.ThemedTitle.IsAvailable)
                {
                    using (var context = CreateContext())
                    {
                        string themeColorURL = null;
                        string themeFontURL = null;
                        string themeImageURL = null;
                        if (webSettingInfo.ThemedColorUrl != null && webSettingInfo.ThemedColorUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedColorUrl.Value))
                        {
                            themeColorURL = webSettingInfo.ThemedColorUrl.Value;
                        }
                        if (webSettingInfo.ThemedFontUrl != null && webSettingInfo.ThemedFontUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedFontUrl.Value))
                        {
                            themeFontURL = webSettingInfo.ThemedFontUrl.Value;
                        }
                        if (webSettingInfo.ThemedImageUrl != null && webSettingInfo.ThemedImageUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedImageUrl.Value))
                        {
                            themeImageURL = webSettingInfo.ThemedImageUrl.Value;
                        }
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        if (webSettingInfo.MasterUrl != null && webSettingInfo.MasterUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.MasterUrl.Value))
                        {
                            if (IsFileExist(context, web, webSettingInfo.MasterUrl.Value))
                            {
                                web.MasterUrl = webSettingInfo.MasterUrl.Value;
                                web.Update();
                            }
                            else
                            {
                                mLogger.Info("The master page restoring for current theme does not exist in destination site.WebUrl:{0},MasterUrl:{1}", webServerRelativeUrl, webSettingInfo.MasterUrl.Value);
                            }
                        }
                        context.Load(web, w => w.ServerRelativeUrl);
                        context.ExecuteQuery();
                        if (!IsThemeFileExist(themeFontURL, context, web))
                        {
                            themeFontURL = null;
                        }
                        if (!IsThemeFileExist(themeImageURL, context, web))
                        {
                            themeImageURL = null;
                        }
                        try
                        {
                            web.ApplyTheme(themeColorURL, themeFontURL, themeImageURL, true);
                        }
                        catch (ArgumentException ex)
                        {
                            mLogger.Info("Invalidate argument.Message:{0}", ex.ToString());
                            if (context.HasPendingRequest)
                            {
                                context.ExecuteQuery();
                            }
                            return;
                        }
                        List list = web.GetCatalog((int)ListTemplateType.DesignCatalog);
                        CamlQuery camlQuery = new CamlQuery();
                        //ADO-51026
                        camlQuery.ViewXml = "<View>" +
                                           "<Query><Where>" +
                                           "<Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq>" +
                                           "</Where></Query>" +
                                       "</View>";
                        camlQuery.DatesInUtc = true;
                        ListItemCollection items = list.GetItems(camlQuery);
                        context.Load(items, its => its.Include(it => it.DisplayName));
                        context.ExecuteQuery();
                        if (items.Count == 1)
                        {
                            //item["MasterPageUrl"] = "";
                            items[0]["ThemeUrl"] = themeColorURL;
                            items[0]["FontSchemeUrl"] = themeFontURL;
                            items[0]["ImageUrl"] = themeImageURL;
                            items[0].Update();
                            context.ExecuteQuery();
                        }
                    }
                }
                else
                {
                    mWebServiceRequest.RestoreTheme(webServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, themedCssFolderUrl);
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Failed to restore theme. Exception: {0}", ex.ToString());
            }
        }

        private bool IsThemeFileExist(string fileRelativeUrl, AveClientContext context, Web web)
        {
            bool exist = false;
            if (string.IsNullOrEmpty(fileRelativeUrl))
            {
                exist = false;
            }
            else
            {
                if (IsSharedTheme(fileRelativeUrl, web))
                {
                    exist = IsFileExist(context, context.Site.RootWeb, fileRelativeUrl);
                }
                else
                {
                    exist = IsFileExist(context, web, fileRelativeUrl);
                }
            }
            return exist;
        }

        protected bool IsSharedTheme(string fileRelativeUrl, Web web)
        {
            string str = AveUrlUtility.CombineUrl(web.ServerRelativeUrl, "_themes");
            return !fileRelativeUrl.StartsWith(str, StringComparison.OrdinalIgnoreCase);
        }

        public override void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            mWebServiceRequest.BrowserEnableUserFormTemplate(formTemplateUrl);
        }

        public override string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            return mWebServiceRequest.AssociateWorkflowMarkup(webServerRelativeUrl, configUrl, configVersion);
        }

        public override Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                Web web = site.OpenWeb(webServerRelativeUrl);
                string webFullUrl = this.WebAppName + webServerRelativeUrl;
                AveAppRestore appRestore = new AveAppRestore(context, mObj, site, web, webFullUrl);
                appRestore.RestoreApp(appInfo, restoreInfo);
                return GetAppsByProductId(webServerRelativeUrl, appInfo.ProductId);
            }
        }
        #endregion

        #region Recycle
        #endregion

        #region private method



        private List<Dictionary<string, object>> NavigationNodeCollectionToList(NavigationNodeCollection nodes, Dictionary<string, object> nodesProp)
        {
            List<Dictionary<string, object>> returnPropeties = new List<Dictionary<string, object>>();
            if (nodes.ServerObjectIsNull.HasValue && nodes.ServerObjectIsNull.Value)
            {
                return returnPropeties;
            }
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
                //ADO-133135 直接使用API获取出来的 IsExternal属性，如果有不准的情况，再加逻辑和注释特殊处理
                //if (!string.IsNullOrEmpty(node.Url))
                //{
                //    if (node.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                //        node.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                //        node.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                //    {
                //        nodeDic["IsExternal"] = !node.Url.StartsWith(this.WebAppName.TrimEnd('/') + "/" + this.mSiteRelativeUrl.TrimStart('/') + "/", StringComparison.OrdinalIgnoreCase);
                //    }
                //    else
                //    {
                //        nodeDic["IsExternal"] = false;
                //    }
                //}
                //else
                //{
                //    nodeDic["IsExternal"] = true;
                //}
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
                        break;
                    }
                }
                returnPropeties.Add(nodeDic);
            }
            return returnPropeties;
        }
        private void GetNavigationNodeChild(NavigationNode node, List<Dictionary<string, object>> dic, Dictionary<string, object> nodesProp)
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
                //if (!string.IsNullOrEmpty(childNode.Url))
                //{
                //    if (childNode.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                //        childNode.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                //        childNode.Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                //    {
                //        nodeDic["IsExternal"] = !childNode.Url.StartsWith(this.WebAppName, StringComparison.OrdinalIgnoreCase);
                //    }
                //    else
                //    {
                //        nodeDic["IsExternal"] = false;
                //    }
                //}
                //else
                //{
                //    nodeDic["IsExternal"] = true;
                //}
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
                        break;
                    }
                }
                dic.Add(nodeDic);
            }
        }
        protected override void LoadListCollection(AveClientContext context, ExceptionHandlingScope scope, ListCollection listCollection)
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
                                                                                                      l => l.RootFolder,
                                                                                                      l => l.RootFolder.Properties
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

        protected override void LoadWebAndSubwebs(ClientContext context, Web web, WebCollection subWebs)
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
                                                 //tempWeb => tempWeb.ListTemplates,
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
                                                                                  //tempWeb => tempWeb.ListTemplates,
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
                                                 //temp => temp.ListTemplates,
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
                                                                                  //temp => temp.ListTemplates,
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

        protected override void LoadWebCollection(ClientContext context, WebCollection webCollection)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                                 w => w.RootFolder,
                                                                                                 //w => w.ListTemplates,
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
                                                                                                 //w => w.ListTemplates,
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

        /// <summary>
        /// load properties of list for discover,browser
        /// </summary>
        protected override void LoadListPropertiesForDiscoverBrowser(ClientContext context, Web web)
        {
            context.Load(web.Lists, listCollection => listCollection.Include(
                                                     l => l.Id,
                                                     l => l.Title,
                                                     l => l.BaseType,
                                                     l => l.BaseTemplate,
                                                     l => l.Hidden,
                                                     l => l.EnableVersioning,
                                                     l => l.EnableAttachments,
                                                     l => l.HasUniqueRoleAssignments,
                                                     l => l.EnableFolderCreation,
                                                     l => l.RootFolder,
                                                     l => l.RootFolder.ServerRelativeUrl,
                                                     l => l.RootFolder.Name,
                                                     l => l.RootFolder.Properties));
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is property name")]
        protected override void AssemblRootFolderProperties(string webServerRelativeUrl, Dictionary<string, object> folderProperties, Folder rootFolder)
        {
            base.AssemblRootFolderProperties(webServerRelativeUrl, folderProperties, rootFolder);
            //add root folder unique id properties
            Guid folderUniqueId = Guid.Empty;
            if (rootFolder.Properties.FieldValues.ContainsKey("vti_etag") && rootFolder.Properties["vti_etag"] != null)
            {
                string tagString = rootFolder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                folderUniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
            }
            if (folderUniqueId != Guid.Empty)
            {
                folderProperties["UniqueId"] = folderUniqueId;
            }
            folderProperties["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable(rootFolder.Properties.FieldValues);
        }

        public override Dictionary<string, object> GetAllWebs()
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
                        //if (IsApplicationWeb(web))
                        //{
                        //    continue;
                        //}
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

        protected override Dictionary<string, object> GetWebProperties(ClientContext context, Web web, string contextUrl, string siteServerRelativeUrl, bool webLoaded)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            if (!webLoaded)
            {
                context.Load(context.Site.RootWeb);
                LoadWeb(web, context);
                context.ExecuteQuery();
            }
            CopyProperty(webProperties, web);

            bool isAppWeb = web.AppInstanceId != Guid.Empty;
            webProperties["IsAppWeb"] = isAppWeb;
            webProperties["Exists"] = true;
            webProperties["CurrentUser" + AveObjectModelConstant.ObjectPropertySuffix] = web.CurrentUser.LoginName;
            //webProperties.Add("IsPublish", false);
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
            string configuration = AveWebServiceRequest.GetWebTemplateConfiguration(this.WebAppName, web.ServerRelativeUrl, this.mObj);
            string[] datas = configuration.Split('#');
            if (datas.Length == 2)
            {
                webProperties["WebTemplate"] = datas[0];
                webProperties["Configuration"] = short.Parse(datas[1]);
            }
            Dictionary<string, object> result = AveWebTemplateHelper.GetWebTemplateConfigurationProperty(mWebUrl, mObj, mServerVersion, mCompatibilityLevel);
            if (result.ContainsKey("WebTemplateId"))
            {
                webProperties["WebTemplateId"] = result["WebTemplateId"];
            }
            webProperties["WebTemplate"] = web.WebTemplate;
            webProperties["Configuration"] = web.Configuration;
            webProperties["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] = web.AllProperties.FieldValues;

            Dictionary<string, object> AssociatedMemberGroupProperties = GetGroupProperties(base.mSiteTrimObj, context, web.AssociatedMemberGroup, false);
            Dictionary<string, object> AssociatedOwnerGroupProperties = GetGroupProperties(base.mSiteTrimObj, context, web.AssociatedOwnerGroup, false);
            Dictionary<string, object> AssociatedVisitorGroupProperties = GetGroupProperties(base.mSiteTrimObj, context, web.AssociatedVisitorGroup, false);

            webProperties["AssociatedMemberGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedMemberGroupProperties;
            webProperties["AssociatedOwnerGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedOwnerGroupProperties;
            webProperties["AssociatedVisitorGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedVisitorGroupProperties;

            if (!isAppWeb)
            {
                mRequestCommon.GetWebSearchAndOfflineAvailability(web.ServerRelativeUrl, webProperties, mObj);
            }
            return webProperties;
        }

        protected override void GetWebTemplate(AveWebBrowserInfo info, Web web, AveClientContext context)
        {
            info.TemplateName = web.WebTemplate + "#" + web.Configuration;
            string siteUrl = this.WebAppName + AveUrlUtility.GetSiteServerRelativeUrl(web.Context.Url);
            using (AveWebServiceRequest aveWebServiceRequest = new AveWebServiceRequest(siteUrl, mUserAccountInfo, mObj, "15"))
            {
                Dictionary<string, object> WebTemplates = aveWebServiceRequest.GetWebTemplates(web.ServerRelativeUrl, web.Language, false, "");
                info.TemplateTitle = GetWebTemplateNameById(info.TemplateName, WebTemplates);
            }
        }


        public override string GetWebTemplateConfiguration(string webRelativeUrl)
        {
            try
            {
                return AveWebTemplateHelper.GetWebTemplateConfiguration(WebAppName.TrimEnd('/') + "/" + webRelativeUrl.Trim('/'), mObj, mServerVersion, mCompatibilityLevel);
            }
            catch (Exception e)
            {
                mLogger.Warn("Get Web Template Configuration Error. Web:{0} Exception Message:{1}", webRelativeUrl, e.ToString());
                return string.Empty;
            }
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

        protected bool IsApplicationWeb(Web web)
        {
            return web.AppInstanceId != Guid.Empty;
        }

        protected override void WebGetSubwebs(AveClientContext context, Web rootWeb, List<Dictionary<string, object>> webList, string siteUrl, string siteServerRelativeUrl)
        {
            WebCollection subWebs = rootWeb.GetSubwebsForCurrentUser(null);
            LoadWebCollection(context, subWebs);
            foreach (Web web in subWebs)
            {
                //if (IsApplicationWeb(web))
                //{
                //    continue;
                //}
                Dictionary<string, object> dicWeb = new Dictionary<string, object>();
                dicWeb = GetWebProperties(context, web, siteUrl, siteServerRelativeUrl, true);
                webList.Add(dicWeb);
                WebGetSubwebs(context, web, webList, siteUrl, siteServerRelativeUrl);
            }
        }

        public override Dictionary<string, object> GetSubWebs(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WebCollection webCollection = web.GetSubwebsForCurrentUser(null);
                context.Load(context.Site);
                context.Load(context.Site.RootWeb, w => w.Id);
                LoadSubWebs(context, webCollection);
                context.ExecuteQuery();
                Dictionary<string, object> subWebs = new Dictionary<string, object>();
                List<Dictionary<string, object>> subWebList = new List<Dictionary<string, object>>();
                foreach (Web subWeb in webCollection)
                {
                    //if (IsApplicationWeb(subWeb)) { continue; }
                    Dictionary<string, object> subWebProperties = new Dictionary<string, object>();
                    subWebProperties = GetWebProperties(context, subWeb, context.Site.Url, context.Site.ServerRelativeUrl, true);
                    subWebList.Add(subWebProperties);
                }
                subWebs.Add(AveObjectModelConstant.ChildrenProperties, subWebList);
                return subWebs;
            }
        }

        protected virtual void LoadSubWebs(AveClientContext context, WebCollection webCollection)
        {
            context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                w => w.RootFolder,
                                                                                //w => w.ListTemplates,
                                                                                w => w.AllProperties,
                                                                                w => w.Navigation.TopNavigationBar,
                                                                                w => w.Navigation.QuickLaunch,
                                                                                w => w.AllowDesignerForCurrentUser,
                                                                                w => w.HasUniqueRoleAssignments,
                                                                                w => w.AppInstanceId,
                                                                                w => w.AssociatedMemberGroup,
                                                                                w => w.AssociatedMemberGroup.Users,
                                                                                w => w.AssociatedMemberGroup.Owner.Id,
                                                                                w => w.AssociatedMemberGroup.Owner.PrincipalType
                                                                                ));
        }

        protected override void GetSubWebs(Web web, string siteServerRelativeUrl, Dictionary<string, object> webDic)
        {
            AssembleDiscoverWebProperties(webDic, web, siteServerRelativeUrl);
            WebCollection webs = web.GetSubwebsForCurrentUser(null);
            web.Context.Load(webs, collection => collection.Include(w => w.Id, w => w.Title, w => w.ServerRelativeUrl, w => w.AppInstanceId));
            web.Context.ExecuteQuery();
            foreach (Web subWeb in webs)
            {
                //if (IsApplicationWeb(subWeb)) { continue; }
                Dictionary<string, object> subWebDic = new Dictionary<string, object>();
                GetSubWebs(subWeb, siteServerRelativeUrl, subWebDic);
                ((Dictionary<Guid, object>)webDic["SubWebs"]).Add((Guid)subWebDic["WebID"], subWebDic);
            }
        }

        public override Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<Guid, object> webProperties = new Dictionary<Guid, object>();
                Web web = context.Site.OpenWebById(parentWebId);
                WebCollection webs = web.GetSubwebsForCurrentUser(null);
                context.Load(webs, collection => collection.Include(w => w.Id, w => w.Title, w => w.ServerRelativeUrl, w => w.AppInstanceId));
                context.Load(context.Site, site => site.ServerRelativeUrl);
                context.ExecuteQuery();
                foreach (Web subWeb in webs)
                {
                    Dictionary<string, object> subWebProperty = new Dictionary<string, object>();
                    if (IsApplicationWeb(subWeb))
                    {
                        subWebProperty["IsAppWeb"] = true;
                        subWebProperty["AppInstanceId"] = subWeb.AppInstanceId;
                    }
                    AssembleDiscoverWebProperties(subWebProperty, subWeb, context.Site.ServerRelativeUrl);
                    webProperties.Add(subWeb.Id, subWebProperty);
                }
                return webProperties;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is property name")]
        public override void AssembleFolderProperties(AveClientContext context, string webServerRelativeUrl, Folder folder, string folderServerRelativeUrl, Dictionary<string, object> folderProp)
        {
            string Url = string.Empty;
            if (!folder.ServerRelativeUrl.TrimEnd('/').Equals(webServerRelativeUrl.TrimEnd('/')))
            {
                Url = folder.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            LoadFolderProperties(context, webServerRelativeUrl, Guid.Empty, folder, folderProp);
            folderProp["Url"] = Url;
            folderProp["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = folder.ParentFolder.ServerRelativeUrl;
            if (folder.Properties.FieldValues.ContainsKey("vti_etag") &&
                folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                Guid uniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                folderProp["UniqueId"] = uniqueId;
            }
        }

        private void AssembleSingleContentTypeProperties(Dictionary<string, object> contentTypeProperties, ContentType contentType)
        {
            CopyProperty(contentTypeProperties, contentType);
            contentTypeProperties.Remove("Id");
            contentTypeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = contentType.Id.ToString();
            contentTypeProperties["ParentId"] = contentType.Parent.Id.ToString();
        }

        protected override void AssembleSingleFieldProperties(Dictionary<string, object> fieldProperties, Field field)
        {
            CopyProperty(fieldProperties, field);
            fieldProperties["BaseTypeString"] = AssembleFieldBaseTypeString(field);
            //these properties can't get from client api, so get it from schemal
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = field.SchemaXml;

            GetNormalFieldProperties(field, doc, fieldProperties);
            if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
            {
                GetCustomization(doc, fieldProperties);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Reorderable is a part of Attribute")]
        private void GetNormalFieldProperties(Field field, XmlDocument doc, Dictionary<string, object> fieldProperties)
        {
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

            fieldProperties["Type"] = GetFieldType(field);
            fieldProperties["ObjectPath"] = GetObjectPathString(field.Path);
            fieldProperties["FieldType"] = field.TypedObject.GetType();
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

        private int GetFieldType(Field field)
        {
            switch (field.FieldTypeKind)
            {
                case FieldType.Invalid:
                    return (int)FieldType.Invalid;
                default:
                    return (int)field.FieldTypeKind;
            }
        }

        public override Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            // 下面这行没有任何逻辑意义，只是为了加载Taxonomy的DLL. ADO-160121
            var loadDll = ChangedItemType.Term;
            mLogger.Info("Load  Microsoft.SharePoint.Client.Taxonomy dll. Load dll value:{0}", loadDll);
            return base.GetFields(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, fieldSource, contentTypeProp);
        }

        protected override string AssembleFieldBaseTypeString(Field field)
        {
            string mBaseFieldType = field.TypeAsString;
            if (field.FieldTypeKind == FieldType.Invalid)
            {
                if (field is TaxonomyField)
                {
                    mBaseFieldType = (field as TaxonomyField).AllowMultipleValues ? "TaxonomyFieldTypeMulti" : "TaxonomyFieldType";
                }
                else if (field is FieldUser)
                {
                    mBaseFieldType = (field as FieldUser).AllowMultipleValues ? "UserMulti" : "User";
                }
                else if (field is FieldLookup)
                {
                    mBaseFieldType = (field as FieldLookup).AllowMultipleValues ? "LookupMulti" : "Lookup";
                }
            }
            return mBaseFieldType;
        }

        protected override void AssembleItemProperties(Dictionary<string, object> props, object fieldValue, string fieldName)
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
                else if (fieldValue is TaxonomyFieldValue)
                {
                    TaxonomyFieldValue taxonomyValue = (fieldValue as TaxonomyFieldValue);
                    StringBuilder fieldTaxonomyValue = new StringBuilder();
                    fieldTaxonomyValue.Append(taxonomyValue.Label);
                    fieldTaxonomyValue.Append("|");
                    fieldTaxonomyValue.Append(taxonomyValue.TermGuid);
                    fieldValue = fieldTaxonomyValue.ToString();
                }
                else if (fieldValue is TaxonomyFieldValueCollection)
                {
                    TaxonomyFieldValueCollection taxonomyValueCol = (fieldValue as TaxonomyFieldValueCollection);
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    foreach (TaxonomyFieldValue value2 in taxonomyValueCol)
                    {
                        if (value2 == null)
                        {
                            continue;
                        }
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            builder.Append(';');
                        }
                        builder.Append(value2.Label);
                        builder.Append("|");
                        builder.Append(value2.TermGuid);
                    }
                    fieldValue = builder.ToString();

                }
                else if (fieldValue is FieldRatingScaleQuestionAnswer[])//For Rating Scale
                {
                    StringBuilder builder = new StringBuilder();
                    FieldRatingScaleQuestionAnswer[] answers = fieldValue as FieldRatingScaleQuestionAnswer[];
                    foreach (FieldRatingScaleQuestionAnswer answer in answers)
                    {
                        builder.Append(answer.Question);
                        builder.Append(";#");
                        builder.Append(answer.Answer);
                        builder.Append("#");
                    }
                    fieldValue = builder.ToString();
                }
                else
                {
                    fieldValue = fieldValue.ToString();
                }
            }
            else if (fieldValue is DateTime)
            {
                var dateTime = (DateTime)fieldValue;

                if (dateTime.Kind == DateTimeKind.Unspecified)
                {//从Client API中获取的时间是Local的，为其指定Kind，以便区分于Utc时间
                    fieldValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                }
            }
            else if (fieldName.Equals("QuickAddGroups"))
            {
                fieldValue = GetQuickAddGroupsProp(fieldValue as string[]);
            }
            props[fieldName] = fieldValue;
        }

        #region metadata

        private void AssembleTermProperties(Term term, Dictionary<string, object> termProperties)
        {
            CopyProperty(termProperties, term);
            if (termProperties.ContainsKey("MergedTermIds"))
            {
                termProperties.Remove("MergedTermIds");
            }
            try
            {
                if (term.PinSourceTermSet.IsPropertyAvailable("Id"))
                {
                    termProperties["PinSourceTermSetId"] = term.PinSourceTermSet.Id;
                }
            }
            catch (ServerObjectNullReferenceException e)
            {
                mLogger.Debug("Term does not has pin source term set. Error message: {0}", e.ToString());
            }
            termProperties["ParentTermId"] = term.Parent.IsPropertyAvailable("Id") ? term.Parent.Id : Guid.Empty;
        }

        private bool ExecuteTermMethod(Term term, Dictionary<string, object> needUpdateTermProperties)
        {
            bool needCommit = false;
            if (needUpdateTermProperties.ContainsKey("SetDescription"))
            {
                List<List<string>> multilingualParms = needUpdateTermProperties["SetDescription"] as List<List<string>>;
                foreach (var parms in multilingualParms)
                {
                    term.SetDescription(parms[0], Convert.ToInt32(parms[1]));
                }
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("CreateLabel"))
            {
                List<List<string>> parmLists = needUpdateTermProperties["CreateLabel"] as List<List<string>>;
                foreach (List<string> parms in parmLists)
                {
                    term.CreateLabel(parms[0], Convert.ToInt32(parms[1]), Convert.ToBoolean(parms[2]));
                }
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("Deprecate"))
            {
                bool doDeprecate = Convert.ToBoolean(needUpdateTermProperties["Deprecate"]);
                term.Deprecate(doDeprecate);
                needUpdateTermProperties.Remove("Deprecate");
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("CustomProperties"))
            {
                //term.DeleteAllCustomProperties();
                Dictionary<string, string> customProperties = needUpdateTermProperties["CustomProperties"] as Dictionary<string, string>;
                foreach (KeyValuePair<string, string> pair in customProperties)
                {
                    term.SetCustomProperty(pair.Key, pair.Value);
                }
                needCommit = true;
            }
            if (needUpdateTermProperties.ContainsKey("LocalCustomProperties"))
            {
                //term.DeleteAllLocalCustomProperties();
                Dictionary<string, string> localCustomProperties = needUpdateTermProperties["LocalCustomProperties"] as Dictionary<string, string>;
                foreach (KeyValuePair<string, string> pair in localCustomProperties)
                {
                    term.SetLocalCustomProperty(pair.Key, pair.Value);
                }
                needCommit = true;
            }
            return needCommit;
        }

        private Dictionary<string, object> UpdateGroupTermSet(ClientContext context, TermGroup updateGroup, int language, Dictionary<string, object> needUpdateGroupProperties)
        {
            TermSet termSet = null;
            Dictionary<string, object> GroupProp = new Dictionary<string, object>();
            Dictionary<string, object> TermSetsList = new Dictionary<string, object>();
            var termSetActions = needUpdateGroupProperties["TermSetActions"] as Dictionary<Guid, Dictionary<string, object>>;


            foreach (var termSetAction in termSetActions)
            {
                Dictionary<string, object> termSetProp = null;
                if (termSetAction.Value.ContainsKey("CreateTermSet"))
                {
                    termSet = updateGroup.CreateTermSet(termSetAction.Value["CreateTermSet"].ToString(), termSetAction.Key, language);
                }

                if (termSetAction.Value.ContainsKey("UpdateTermSet"))
                {
                    Dictionary<string, object> needUpdateTermSetProperties = termSetAction.Value["UpdateTermSet"] as Dictionary<string, object>;
                    bool needLoadTermSetProperties = false;
                    if (termSet == null)
                    {
                        termSet = updateGroup.TermSets.GetById(termSetAction.Key);
                    }

                    if (needUpdateTermSetProperties.ContainsKey("DeleteTermSet"))
                    {
                        termSet.DeleteObject();
                        context.ExecuteQuery();
                        continue;
                    }

                    AveObjectCopy.UpdateObjectBasicProperties(needUpdateTermSetProperties, termSet);
                    if (needUpdateTermSetProperties.ContainsKey("AddStakeholder"))
                    {
                        List<string> stakeHolders = needUpdateTermSetProperties["AddStakeholder"] as List<string>;
                        foreach (string userName in stakeHolders)
                        {
                            termSet.AddStakeholder(userName);
                        }
                    }
                    object customProperties;
                    if (needUpdateTermSetProperties.TryGetValue("CustomProperties", out customProperties))
                    {
                        foreach (var pair in (customProperties as Dictionary<string, string>))
                        {
                            termSet.SetCustomProperty(pair.Key, pair.Value);
                        }
                    }
                    if (needUpdateTermSetProperties.ContainsKey("AddStakeholder")
                         || needUpdateTermSetProperties.ContainsKey("CustomProperties")
                         || Convert.ToInt32(needUpdateTermSetProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                    {
                        termSet.TermStore.CommitAll();
                        context.Load(termSet);
                        needLoadTermSetProperties = true;
                    }

                    if (needUpdateTermSetProperties.ContainsKey("TermActions"))
                    {
                        termSetProp = UpdateTermSetSubTerm(context, termSet, needUpdateTermSetProperties);
                        if (needLoadTermSetProperties)
                        {
                            CopyProperty(termSetProp, termSet);
                        }
                    }
                }
                if (termSetProp == null)
                {
                    termSetProp = new Dictionary<string, object>();
                    context.Load(termSet);
                    context.ExecuteQuery();
                    CopyProperty(termSetProp, termSet);
                }
                TermSetsList[termSetAction.Key.ToString()] = termSetProp;
            }
            GroupProp.Add("TermSet", TermSetsList);
            return GroupProp;
        }


        private Dictionary<string, object> UpdateTermSetSubTerm(ClientContext context, TermSet termSet, Dictionary<string, object> needUpdateTermSetProperties)
        {
            Dictionary<string, object> TermSetProp = new Dictionary<string, object>();

            Dictionary<string, object> TermsList = new Dictionary<string, object>();

            var updateTermActions = needUpdateTermSetProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>;
            foreach (var action in updateTermActions)
            {
                Term term = null;
                bool isNewTerm = false;
                #region AddTerm,ReuseTerm和PinTerm 都属于新建Term, 所以优先执行。
                if (action.Value.ContainsKey("CreateTerm"))
                {
                    var creatInfo = action.Value["CreateTerm"] as List<string>;

                    term = termSet.CreateTerm(creatInfo[0], Convert.ToInt32(creatInfo[1]), action.Key);
                    isNewTerm = true;
                }
                if (action.Value.ContainsKey("ReuseTerm"))
                {
                    var sourceTerm = termSet.TermStore.GetTerm(action.Key);
                    term = termSet.ReuseTerm(sourceTerm, Convert.ToBoolean(action.Value["ReuseTerm"]));
                    isNewTerm = true;

                }
                if (action.Value.ContainsKey("PinTerm"))
                {
                    var sourceTerm = termSet.TermStore.GetTerm(action.Key);
                    term = termSet.ReuseTermWithPinning(sourceTerm);
                    isNewTerm = true;
                }
                #endregion
                bool deletedTerm = false;
                if (action.Value.ContainsKey("UpdateTerm"))
                {
                    if (term == null)
                    {
                        term = termSet.GetTerm(action.Key);
                        Dictionary<string, object> needUpdateTermProperties = action.Value["UpdateTerm"] as Dictionary<string, object>;
                        deletedTerm = UpdateTermSubTerm(context, term, needUpdateTermProperties, TermsList, action.Key);
                    }
                }
                if (!deletedTerm && !TermsList.ContainsKey(action.Key.ToString()))
                {
                    if (isNewTerm)
                    {
                        context.Load(term);
                        context.ExecuteQuery();
                        var termProperties = new Dictionary<string, object>();
                        AssembleTermProperties(term, termProperties);
                        TermsList.Add(action.Key.ToString(), termProperties);
                    }
                    else
                    {
                        TermsList.Add(action.Key.ToString(), new Dictionary<string, object>());
                    }
                }
            }
            TermSetProp.Add("Term", TermsList);

            return TermSetProp;
        }

        private bool UpdateTermSubTerm(ClientContext context, Term term, Dictionary<string, object> needUpdateTermProperties, Dictionary<string, object> TermsList, Guid termId)
        {
            if (needUpdateTermProperties.ContainsKey("DeleteTerm"))
            {
                term.DeleteObject();
                context.ExecuteQuery();
                return true;
            }

            Dictionary<string, object> termProp = new Dictionary<string, object>();
            AveObjectCopy.UpdateObjectBasicPropertiesWithEscape(needUpdateTermProperties, term, new string[] { });
            bool isLocalCustomPropertyEdit = false;
            if (needUpdateTermProperties.ContainsKey("DeleteLocalCustomPropertyByName"))
            {
                List<string> names = needUpdateTermProperties["DeleteLocalCustomPropertyByName"] as List<string>;
                if (names != null && names.Count > 0)
                {
                    foreach (var name in names)
                    {
                        term.DeleteLocalCustomProperty(name);
                    }
                    isLocalCustomPropertyEdit = true;
                }
            }
            if (needUpdateTermProperties.ContainsKey("DeleteAllLocalCustomProperty"))
            {
                term.DeleteAllLocalCustomProperties();
                isLocalCustomPropertyEdit = true;
            }
            if (ExecuteTermMethod(term, needUpdateTermProperties) || isLocalCustomPropertyEdit
                || Convert.ToInt32(needUpdateTermProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
            {
                term.TermStore.CommitAll();
                context.Load(term);
                context.ExecuteQuery();
                CopyProperty(termProp, term);
                TermsList[termId.ToString()] = termProp;
            }


            if (needUpdateTermProperties.ContainsKey("ReassignSourceTerm"))
            {
                bool reAssignSourceTerm = Convert.ToBoolean(needUpdateTermProperties["ReassignSourceTerm"]);
                if (reAssignSourceTerm)
                {
                    Dictionary<string, object> termProperties = new Dictionary<string, object>();
                    term.SourceTerm.ReassignSourceTerm(term);
                    context.ExecuteQuery();
                    AssembleTermProperties(term, termProperties);
                    TermsList[termId.ToString()] = termProperties;
                }
            }

            if (needUpdateTermProperties.ContainsKey("TermActions"))
            {
                var actions = needUpdateTermProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>;
                foreach (var action in actions)
                {
                    Term newTerm = null;
                    bool isNewTerm = false;
                    #region AddTerm,ReuseTerm和PinTerm 都属于新建Term, 所以优先执行。
                    if (action.Value.ContainsKey("CreateTerm"))
                    {
                        var creatInfo = action.Value["CreateTerm"] as List<string>;

                        newTerm = term.CreateTerm(creatInfo[0], Convert.ToInt32(creatInfo[1]), action.Key);

                        isNewTerm = true;
                    }
                    if (action.Value.ContainsKey("ReuseTerm"))
                    {
                        var sourceTerm = term.TermStore.GetTerm(action.Key);
                        newTerm = term.ReuseTerm(sourceTerm, Convert.ToBoolean(action.Value["ReuseTerm"]));
                        isNewTerm = true;
                    }
                    if (action.Value.ContainsKey("PinTerm"))
                    {
                        var sourceTerm = term.TermStore.GetTerm(action.Key);
                        newTerm = term.ReuseTermWithPinning(sourceTerm);
                        isNewTerm = true;
                    }
                    #endregion
                    bool deletedTerm = false;
                    if (action.Value.ContainsKey("UpdateTerm"))
                    {
                        if (newTerm == null)
                        {
                            newTerm = term.TermSet.GetTerm(action.Key);
                            var newNeedUpdateTermProperties = action.Value["UpdateTerm"] as Dictionary<string, object>;
                            deletedTerm = UpdateTermSubTerm(context, newTerm, newNeedUpdateTermProperties, TermsList, action.Key);
                        }
                    }
                    if (!deletedTerm && !TermsList.ContainsKey(action.Key.ToString()))
                    {
                        if (isNewTerm)
                        {
                            context.Load(newTerm);
                            context.ExecuteQuery();
                            var termProperties = new Dictionary<string, object>();
                            AssembleTermProperties(newTerm, termProperties);
                            TermsList.Add(action.Key.ToString(), termProperties);
                        }
                        else
                        {
                            TermsList.Add(action.Key.ToString(), new Dictionary<string, object>());
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Discovery Query
        private object lockObject = new object();
        /// <summary>
        /// 过滤冗余Version
        /// </summary>
        /// <param name="context"></param>
        /// <param name="list"></param>
        /// <param name="versions"></param>
        /// <param name="itemId"></param>
        /// <param name="itemUIVersion"></param>
        protected void FilterUnusedVersion(AveClientContext context, List list, List<Dictionary<string, object>> versions, int itemId, int itemUIVersion)
        {
            try
            {
                var file = (ClientFile)null;
                lock (lockObject)
                {
                    file = list.GetItemById(itemId).File;
                }

                context.Load(file, f => f.Exists);
                context.ExecuteQuery();
                if (file != null && file.Exists)
                {
                    context.Load(file.Versions);
                    context.ExecuteQuery();
                    if (file.Versions == null || file.Versions.Count == 0)
                    {
                        return;
                    }
                    var versionIds = new List<int>();
                    foreach (var version in file.Versions)
                    {
                        versionIds.Add(version.ID);
                    }
                    if (!versionIds.Contains(itemUIVersion))
                    {
                        versionIds.Add(itemUIVersion);
                    }
                    for (int i = 0; i < versions.Count; i++)
                    {
                        var version = versions[i];
                        if (!versionIds.Contains((int)version["VersionId"]))
                        {
                            versions.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while filtering unused version. Error: {0}", e);
            }
        }

        protected virtual bool IsSpecialLibrary(AveClientContext context, string webUrl, Guid webId, Guid listId, out List list)
        {
            list = null;
            try
            {
                var web = webId == Guid.Empty ? context.Site.OpenWeb(webUrl) : context.Site.OpenWebById(webId);
                list = web.Lists.GetById(listId);
                if (string.IsNullOrEmpty(webUrl))
                {
                    context.Load(web, w => w.ServerRelativeUrl);
                    context.ExecuteQuery();
                    webUrl = web.ServerRelativeUrl;
                }
                context.Load(list, l => l.BaseType);
                context.ExecuteQuery();
                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    var versionSetting = GetListVersionLimited(webUrl, listId);
                    if (versionSetting.ContainsKey("MajorWithMinorVersionsLimit") && (int)versionSetting["MajorWithMinorVersionsLimit"] != 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while confirm whether this list is special library. WebURl: {0}, WebId: {1} ListId: {2},  Error: {3}", webUrl, webId, listId, e);
            }
            return false;
        }

        ///<summary>
        /// 将list下需要备份的item/folder填充并缓存，使备份时无需再次进行GetItem操作。Note: Discover不生效并且缓存只对当前request有效
        /// </summary>
        public override Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool isDiscover, bool includeSystemFolder = false)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> parentFolder = new Dictionary<string, object>();
                Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                //needLoadFields.Add("ID", "Counter");
                //needLoadFields.Add("GUID", "Guid");
                needLoadFields.Add("_Level", "Integer");
                //needLoadFields.Add("_IsCurrentVersion", "Boolean");
                needLoadFields.Add("_UIVersion", "Integer");
                Web web = context.Site.OpenWebById(webId);
                parentFolder["Items"] = new List<Dictionary<string, object>>();
                parentFolder["Folders"] = new List<Dictionary<string, object>>();
                bool isSpecialLibrary = false;
                List list = null;
                //parentFolder["Attachments"] = new List<Dictionary<string, object>>();
                //parentFolder["Versions"] = new List<Dictionary<string, object>>();
                if (listId != Guid.Empty) // for system folder, we skip it now, to do it later
                {
                    string folderServerRelativeUrl = "/" + folderUrl.TrimStart('/');
                    Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    context.Load(folder, f => f.ItemCount, f => f.ListItemAllFields, f => f.ServerRelativeUrl);
                    isSpecialLibrary = IsSpecialLibrary(context, string.Empty, webId, listId, out list);
                    //需要优化，只需要获取要用的属性
                    context.Load(list,
                        l => l.BaseType, l => l.EnableVersioning, l => l.EnableMinorVersions, l => l.EnableAttachments,
                        l => l.EnableFolderCreation, l => l.EnableModeration, l => l.BaseTemplate,
                        l => l.Id, l => l.Title, l => l.Created, l => l.ItemCount, l => l.Views);
                    context.Load(list.RootFolder, r => r.ServerRelativeUrl, r => r.ItemCount);
                    context.Load(web, tempWeb => tempWeb.ServerRelativeUrl);
                    context.ExecuteQuery();
                    GetItemsFromFolder(context, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl, parentFolder, isDiscover);
                }
                else
                {
                    context.Load(web, tempWeb => tempWeb.ServerRelativeUrl);
                    context.ExecuteQuery();
                    List<Dictionary<string, object>> webItems = parentFolder["Items"] as List<Dictionary<string, object>>;
                    List<Dictionary<string, object>> webFolders = parentFolder["Folders"] as List<Dictionary<string, object>>;
                    Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/");
                    Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/", includeSystemFolder);
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
                List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)parentFolder["Items"];
                if (!listId.Equals(Guid.Empty))
                {
                    string webUrl = parentFolder.ContainsKey("WebServerRelativeUrl") ?
                    parentFolder["WebServerRelativeUrl"].ToString() : this.GetWeb(webId)["ServerRelativeUrl"].ToString();
                    List<Task> getItemVersionTasks = new List<Task>();
                    items.ForEach((item) =>
                    {
                        if (item.ContainsKey("Versions") && WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                        {
                            getItemVersionTasks.Add(() => { this.GetListItemVersion(context, list, isSpecialLibrary, item, webUrl, listId, needLoadFields, isDiscover); });
                        }
                        else
                        {// list enable version is false, we just add current version here
                            List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                            this.AssembleItemVersionProperty(item, versions);
                            item["HasVersion"] = false;
                        }
                    });
                    if (getItemVersionTasks.Count > 0)
                    {
                        using (AveTaskExecutor taskExecutor = new AveTaskExecutor(WrapperConfiguration.BPOS_S.MaximumThreadsGettingVersions))
                        {
                            taskExecutor.Execute(getItemVersionTasks);
                        }
                    }
                }
                else
                {
                    items.ForEach((item) =>
                    {
                        List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                        AssembleWebItemVersionProperty(item, versions);
                        item["HasVersion"] = false;
                    });
                }
                return parentFolder;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        public override Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, bool includeSystemFolder)
        {
            Dictionary<string, object> subFolders = new Dictionary<string, object>();
            List<Dictionary<string, object>> subFolderList = new List<Dictionary<string, object>>();
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    context.Load(folder);
                    //context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder));
                    //items properties   
                    ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                    using (excepScope.StartScope())
                    {
                        using (excepScope.StartTry())
                        {
                            if (listName != null)
                            {
                                context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder, f => f.ListItemAllFields, f => f.ListItemAllFields.HasUniqueRoleAssignments));
                            }
                            else if (includeSystemFolder)
                            {
                                context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder, f => f.Properties));
                            }
                            else
                            {
                                context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder));
                            }
                        }
                        using (excepScope.StartCatch())
                        {
                            if (includeSystemFolder)
                            {
                                context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder, f => f.Properties));
                            }
                        }
                    }
                    context.ExecuteQuery();
                    List<string> excludeFolders = null;
                    if (!includeSystemFolder && folderServerRelativeUrl.Trim('/').Equals(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
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
                        //For DPM discover
                        if (includeSystemFolder && listName == null)
                        {
                            if (subFolder.Properties.FieldValues.ContainsKey("vti_listname") && subFolder.Properties.FieldValues["vti_listname"] != null &&
                                AveTypeHelper.IsGuid(subFolder.Properties.FieldValues["vti_listname"].ToString()) && new Guid(subFolder.Properties.FieldValues["vti_listname"].ToString()) != Guid.Empty)
                            {
                                continue;
                            }
                        }
                        Dictionary<string, object> subFolderProperties = new Dictionary<string, object>();
                        subFolderProperties["Exists"] = true;
                        AssembleFolderProperties(context, webServerRelativeUrl, subFolder, subFolder.ServerRelativeUrl, subFolderProperties);
                        if (subFolder.ListItemAllFields.ServerObjectIsNull.HasValue &&
                            !subFolder.ListItemAllFields.ServerObjectIsNull.Value &&
                            subFolder.ListItemAllFields.FieldValues.Count > 0)
                        {
                            Dictionary<string, object> itmProp = new Dictionary<string, object>();
                            GetItemDic(itmProp, subFolder.ListItemAllFields);
                            subFolderProperties["UniqueId"] = itmProp["UniqueId"];
                            subFolderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itmProp;
                        }
                        subFolderList.Add(subFolderProperties);
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of keys")]
        protected void AssembleWebItemVersionProperty(Dictionary<string, object> item, List<Dictionary<string, object>> versions)
        {
            Dictionary<string, object> version = new Dictionary<string, object>();
            version["ID"] = item.ContainsKey("ID") ? (int)item["ID"] : default(int);
            if (item.ContainsKey("GUID"))  //Survey List item没有GUID
            {
                version["GUID"] = new Guid(item["GUID"].ToString());
                version["UserDataGuid"] = item["GUID"];
            }
            else if (item.ContainsKey("UniqueId"))
            {
                version["GUID"] = new Guid(item["UniqueId"].ToString());
                version["UserDataGuid"] = item["UniqueId"];
            }
            version["Size"] = 0;
            version["ObjType"] = 2;
            version["TimeLastModified"] = item["TimeLastModified"];
            version["UIVersion"] = item["UIVersion"];
            version["IsCurrentVersion"] = true;
            version["Level"] = item["Level"];
            versions.Add(version);
            item["Versions"] = versions;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of Keys")]
        private void GetListItemVersion(AveClientContext context, List list, bool needFilterUnusedVersion, Dictionary<string, object> item, string webUrl, Guid listId, Dictionary<string, string> needLoadFields, bool isDiscover)
        {
            List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
            Dictionary<string, object> allVersionProperties = isDiscover ? QueryItemVersionsForDiscover(webUrl, string.Empty, listId.ToString(), (int)item["Id"], "", null, needLoadFields)
                : GetItemVersions(webUrl, string.Empty, listId.ToString(), (int)item["Id"], "", null, needLoadFields);
            if (allVersionProperties.ContainsKey("HasVersion") && !Convert.ToBoolean(allVersionProperties["HasVersion"]))
            {
                AssembleItemVersionProperty(item, versions);
                item["HasVersion"] = false;
            }
            else
            {
                List<Dictionary<string, object>> versionProperties = (List<Dictionary<string, object>>)allVersionProperties["ChildrenProperties"];
                foreach (Dictionary<string, object> version in versionProperties)
                {
                    version["ID"] = (int)item["Id"];
                    version["GUID"] = new Guid(item["GUID"].ToString());
                    version["Size"] = 0;
                    version["ObjType"] = item["ObjType"];
                    version["TimeLastModified"] = version["Modified"];
                    int versionId = (int)version["VersionId"];
                    if (!version.ContainsKey("Level"))
                    {
                        version["Level"] = (byte)1;
                    }
                    version["UIVersion"] = version["VersionId"];
                    version["UserDataGuid"] = version["GUID"];
                    object fieldValues;
                    if (item.TryGetValue("FieldValues", out fieldValues) && fieldValues != null)
                    {
                        version["IsCurrentVersion"] = versionId == (int)((Dictionary<string, object>)fieldValues)["_UIVersion"];
                    }
                    else
                    {
                        version["IsCurrentVersion"] = versionId == (int)item["UIVersion"];
                    }
                    versions.Add(version);
                }
                if (needFilterUnusedVersion && versions.Count > 0)
                {
                    FilterUnusedVersion(context, list, versions, (int)item["Id"], (int)item["UIVersion"]);
                }
            }
        }
        protected virtual Dictionary<string, object> QueryItemVersionsForDiscover(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            return GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of ViewXml")]
        private void AssembleItemVersionProperty(Dictionary<string, object> item, List<Dictionary<string, object>> versions)
        {
            Dictionary<string, object> version = new Dictionary<string, object>();
            version["ID"] = (int)item["ID"];
            if (item.ContainsKey("GUID"))  //Survey List item没有GUID
            {
                version["GUID"] = new Guid(item["GUID"].ToString());
                version["UserDataGuid"] = item["GUID"];
            }
            version["Size"] = 0;
            version["ObjType"] = item["ObjType"];
            version["TimeLastModified"] = item["TimeLastModified"];
            version["UIVersion"] = item["UIVersion"];
            version["IsCurrentVersion"] = item.ContainsKey("_IsCurrentVersion") ? item["_IsCurrentVersion"] : true;//Avoid Key not exist exception, reproduce  list: siteUrl/_catalogs/design
            version["Level"] = item["Level"];
            versions.Add(version);
            item["Versions"] = versions;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of Keys")]
        public override Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changeItemsCache)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                //needLoadFields.Add("ID", "Counter");
                //needLoadFields.Add("GUID", "Guid");
                //needLoadFields.Add("_Level", "Integer");
                //needLoadFields.Add("_IsCurrentVersion", "Boolean");
                needLoadFields.Add("_UIVersion", "Integer");
                Dictionary<string, object> folder = new Dictionary<string, object>();
                folder["Items"] = new List<Dictionary<string, object>>();
                folder["Folders"] = new List<Dictionary<string, object>>();
                GetChangeItemsFromChangeCache(folder, webId, listId, folderUrl, changeItemsCache);
                List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)folder["Items"];
                string webUrl = folder.ContainsKey("WebServerRelativeUrl") ?
                    folder["WebServerRelativeUrl"].ToString() : this.GetWeb(webId)["ServerRelativeUrl"].ToString();
                List list = null;
                bool isSpecialLibrary = listId == Guid.Empty ? false : IsSpecialLibrary(context, webUrl, Guid.Empty, listId, out list);
                foreach (Dictionary<string, object> item in items)
                {
                    if ((item.ContainsKey("ChangeType") && (AvePoint.Wrapper.Common.ChangeType)item["ChangeType"] == AvePoint.Wrapper.Common.ChangeType.Delete) || !item.ContainsKey("Id"))
                    {
                        item["Versions"] = new List<Dictionary<string, object>>();
                        continue;
                    }
                    if (item.ContainsKey("Versions") && WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                    {
                        List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
                        Dictionary<string, object> allVersionProperties = QueryItemVersionsForDiscover(webUrl, string.Empty, listId.ToString(), (int)item["ID"], string.Empty, null, needLoadFields);
                        List<Dictionary<string, object>> versionProperties = (List<Dictionary<string, object>>)allVersionProperties["ChildrenProperties"];
                        foreach (Dictionary<string, object> version in versionProperties)
                        {
                            version["ID"] = (int)item["ID"];
                            version["GUID"] = new Guid(item["GUID"].ToString());
                            version["Size"] = 0;
                            version["ObjType"] = item["ObjType"];
                            version["TimeLastModified"] = version["Modified"];
                            int versionId = (int)version["VersionId"];
                            version["Level"] = versionId == (int)item["UIVersion"] ? item["Level"] : (byte)1;
                            version["UIVersion"] = versionId;
                            version["UserDataGuid"] = version["GUID"];
                            version["IsCurrentVersion"] = versionId == (int)item["UIVersion"] ? true : false;
                            versions.Add(version);
                        }
                        if (isSpecialLibrary && versions.Count > 0)
                        {
                            FilterUnusedVersion(context, list, versions, (int)item["ID"], (int)item["UIVersion"]);
                        }
                    }
                    else
                    {// list enable version is false, we just add current version here
                        List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                        Dictionary<string, object> version = new Dictionary<string, object>();
                        version["ID"] = (int)item["ID"];
                        version["GUID"] = new Guid(item["GUID"].ToString());
                        version["Size"] = 0;
                        version["ObjType"] = item["ObjType"];
                        version["TimeLastModified"] = item["TimeLastModified"];
                        version["UIVersion"] = item["UIVersion"];
                        version["UserDataGuid"] = item["GUID"];
                        version["IsCurrentVersion"] = item["_IsCurrentVersion"];
                        version["Level"] = item["Level"];
                        versions.Add(version);
                        item["Versions"] = versions;
                    }
                }
                return folder;
            }
        }
        public virtual Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listId, int itemId, string itemUrl, Dictionary<string, string> needLoadFields)
        {
            return mWebServiceRequest.GetItemVersionsWithMultiRequest(webRelativeUrl, listId.ToString(), itemId, "", needLoadFields);
        }

        protected override bool IsThrottled(int itemCount)
        {
            return itemCount >= this.MaxItemsPerThrottledOperation;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key, FSObjType")]
        protected override void QueryFoldersForLargeList(ClientContext context, List list, string folderUrl, List<Dictionary<string, object>> results, List<string> viewFields = null)
        {
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, null, viewFields);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                    item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "1"));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                //当viewFields不为空时，说明只Load了item的部分属性，不能添加到cache里。
                var itemProperty = viewFields == null ? AssemblyFolderInfo(contextArg, itemArg)
                : AssemblyFolderInfoWithoutAddToCache(contextArg, itemArg);
                results.Add(itemProperty);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                results.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin discover folders in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            mLogger.Debug("Finish discover folders in large list, {0} folders in folder {1}", results.Count, folderUrl);
        }
        internal override ListItemCollectionPosition QueryItemsByQueryStringForLargeList(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, ExceptionHandlingScope exceptionScope, Dictionary<string, ClientFile> filesMap, List<Dictionary<string, object>> results, CamlQuery query)
        {
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, query);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition);
                LoadItemsProperty(contextArg, listItemsArg);
            };
            worker.AfterQueryAction += (contextArg, listItemArg, isLibraryArg) =>
            {
                var itemProperty = AssmeblyItemInfo(contextArg, webServerRelativeUrl, exceptionScope.HasException, isLibraryArg, filesMap, listItemArg);
                results.Add(itemProperty);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                results.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin query items in large list, list.ItemCount:{0}, folder URL:{1}, query string: {2}", list.ItemCount, folderUrl, query.ViewXml);
            worker.Run();
            mLogger.Debug("Finish query items in large list, {0} items in folder {1}", results.Count, folderUrl);
            return worker.Position;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key, FSObjType")]
        protected override void QueryItemsForLargeList(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, ExceptionHandlingScope exceptionScope, Dictionary<string, ClientFile> filesMap, List<Dictionary<string, object>> results, List<string> viewFields = null)
        {
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, null, viewFields);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                              items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                          item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
            };
            worker.AfterQueryAction += (contextArg, listItemArg, isLibraryArg) =>
            {
                //当viewFields不为空时，说明只Load了item的部分属性，不能添加到cache里。
                var itemProperty = viewFields == null ? AssmeblyItemInfo(contextArg, webServerRelativeUrl, exceptionScope.HasException, isLibraryArg, filesMap, listItemArg)
                : AssmeblyItemInfoWithoutAddToCache(contextArg, webServerRelativeUrl, exceptionScope.HasException, isLibraryArg, filesMap, listItemArg);
                results.Add(itemProperty);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                results.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin discover items in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            mLogger.Debug("Finish discover items in large list, {0} items in folder {1}", results.Count, folderUrl);
        }

        protected override void QueryBrowserItemsForLargeList(AveClientContext context, List list, AveBrowserOption option, Guid parentFolderId, string webServerRelativeUrl, List<AveItemBrowserInfo> itemBrowserInfos, CamlQuery query)
        {
            var worker = new LargeListQueryWorker(context, list, option.ParentFolderServerRelativeUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, query);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                var scope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    // ADO-131294 office365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName的时候会出异常，这个做一个异常处理。(目前这个属性在ListItem Browser中没有使用，如果会出异常不获取即可。)
                    using (scope.StartTry())
                    {
                        contextArg.Load(listItemsArg);
                        contextArg.Load(listItemsArg, its => its.Include(tm => tm.DisplayName, tm => tm.ParentList.BaseType, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments, tm => tm["FSObjType"]));
                    }
                    using (scope.StartCatch())
                    {
                        contextArg.Load(listItemsArg);
                        contextArg.Load(listItemsArg, its => its.Include(tm => tm.ParentList.BaseType, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments, tm => tm["FSObjType"]));
                    }
                }
            };
            worker.AfterQueryAction += (contextArg, listItemArg, isLibraryArg) =>
            {
                var itemInfo = GetItemBrowserInfo(webServerRelativeUrl, listItemArg);
                itemInfo.ParentFolderUniqueID = parentFolderId;
                itemBrowserInfos.Add(itemInfo);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                itemBrowserInfos.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            worker.Run();
            option.PageInfo = worker.Position == null ? null : worker.Position.PagingInfo;
        }



        internal class LargeListQueryWorker
        {
            private static AveLogger logger = AveLogger.GetInstance(typeof(LargeListQueryWorker));
            /// <summary>
            /// 在执行Query之前的操作，主要是Load属性
            /// BeforeQueryActionImp(ClientContext context,ListItemCollection items)
            /// </summary>
            public event Action<ClientContext, ListItemCollection> BeforeQueryAction;
            /// <summary>
            /// 在成功执行Query后的操作，主要是获取Item信息，组装数据对象
            /// AfterQueryActionImp(ClientContext context,ListItem item,bool isLibrary)
            /// </summary>
            public event Action<ClientContext, ListItem, bool> AfterQueryAction;

            public Action<CamlQuery, string> SetCamlQueryUrl;

            public Action ExceptionWhenQueryAction;
            private bool isLibrary;
            private string folderServerRelatedUrl;
            private uint perPage;
            private List<string> viewFields;
            private QueryFindOption findOption;

            private ClientContext context;
            private List list;
            private CamlQuery query;
            private Func<Web, string, Folder> GetFolder;
            private int rowLimitCount;
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="list">list的ItemCount和BaseType属性必须初始化</param>
            /// <param name="folderServerRelatedUrl"></param>
            /// <param name="perPage"></param>
            /// <param name="getFolderMethod"></param>
            /// <param name="query">如果此参数不为null，则viewFields参数失效。</param>
            /// <param name="viewFields">如果查询所有column value，请保持此参数为null</param>
            public LargeListQueryWorker(ClientContext context, List list, string folderServerRelatedUrl, uint perPage, Func<Web, string, Folder> getFolderMethod, CamlQuery query, List<string> viewFields = null, QueryFindOption findOption = QueryFindOption.None)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (list == null) throw new ArgumentNullException("list");
                if (perPage == 0) throw new ArgumentException("perPage must be great than 0.");
                if (string.IsNullOrEmpty(folderServerRelatedUrl)) throw new ArgumentNullException("folderServerRelatedUrl");

                this.GetFolder = getFolderMethod;
                this.context = context;
                this.list = list;
                this.folderServerRelatedUrl = folderServerRelatedUrl;
                this.perPage = perPage;
                this.isLibrary = list.BaseType == BaseType.DocumentLibrary;
                this.query = query;
                this.viewFields = viewFields;
                this.findOption = findOption;
                rowLimitCount = GetRowLimited(query);
            }
            public ListItemCollectionPosition Position
            {
                get;
                private set;
            }
            private int GetRowLimited(CamlQuery query)
            {
                try
                {
                    int rowLimitedCount = 0;
                    if (query == null || string.IsNullOrEmpty(query.ViewXml))
                    {
                        return rowLimitedCount;
                    }
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(query.ViewXml);
                    var node = xd.SelectSingleNode(".//*[name() = 'RowLimit']");
                    if (node != null)
                    {
                        int.TryParse(node.FirstChild.Value, out rowLimitCount);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Get query row limited value failed. xml: {0}, Error: {1}", query.ViewXml, e);
                }
                return rowLimitCount;
            }
            private void SetRowLimit(CamlQuery query, int rowLimit)
            {
                if (query == null || string.IsNullOrEmpty(query.ViewXml) || rowLimit < 1)
                {
                    return;
                }
                try
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(query.ViewXml);
                    var node = xd.SelectSingleNode(".//*[name() = 'RowLimit']");
                    if (node != null)
                    {
                        node.FirstChild.Value = rowLimit.ToString();
                        query.ViewXml = xd.OuterXml;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Set query row limited value failed. xml: {0}, Error: {1}", query.ViewXml, e);
                }
            }
            public void Run()
            {
                if (this.BeforeQueryAction == null) throw new ArgumentNullException("BeforeQueryAction");
                if (this.AfterQueryAction == null) throw new ArgumentNullException("AfterQueryAction");
                try
                {
                    InitQueryStringForList();
                    if (this.isLibrary)
                    {
                        try
                        {
                            //Query library 的效率最好，但是无法支持带条件的query，如果发现是带条件的query，那么就使用query list
                            if (this.query == null && findOption != QueryFindOption.RecursiveAll)
                            {
                                QueryItemsInLibrary();
                            }
                            else
                            {
                                QueryItemsUseIdIndex();
                            }
                        }
                        catch (Exception e)//如果Library里的Document个数超过5000,并且Disable folder。需要用ID方式查询。
                        {
                            mLogger.Debug("An error occurred while query items in library. Error: {0}", e);
                            if (ExceptionWhenQueryAction != null)
                            {
                                ExceptionWhenQueryAction();
                            }
                            QueryItemsUseIdIndex();
                        }
                    }
                    else
                    {
                        QueryItemsUseIdIndex();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while query large list, query string: {0}, error: {1}", this.query == null ? string.Empty : this.query.ViewXml, e);
                    throw;
                }
            }
            #region Init query string
            private XmlNode InitWhereQuery(XmlNode whereNode)
            {
                string queryString = whereNode == null ? string.Format(@"<Where><And><Geq><FieldRef Name='ID'/><Value Type='Integer'>{0}</Value></Geq><Lt><FieldRef Name='ID'/><Value Type='Integer'>{1}</Value></Lt></And></Where>", "{0}", "{1}")
                                                      : string.Format(@"<And><And><Geq><FieldRef Name='ID'/><Value Type='Integer'>{0}</Value></Geq><Lt><FieldRef Name='ID'/><Value Type='Integer'>{1}</Value></Lt></And>{2}</And>", "{0}", "{1}", whereNode.InnerXml);
                XmlDocument document = new XmlDocument();
                document.LoadXml(queryString);
                return document.DocumentElement;
            }
            private void InitQueryStringForList()
            {
                if (this.query == null)
                {
                    return;
                }
                XmlDocument document = new XmlDocument();
                document.LoadXml(this.query.ViewXml);

                var whereNode = document.DocumentElement.SelectSingleNode("//Where");

                var importNode = document.ImportNode(InitWhereQuery(whereNode), true);
                if (whereNode == null)
                {
                    var queryNode = document.DocumentElement.SelectSingleNode("//Query");
                    if (queryNode != null)
                    {
                        queryNode.AppendChild(importNode);
                    }
                }
                else
                {
                    whereNode.RemoveAll();
                    whereNode.AppendChild(importNode);
                }

                this.query.ViewXml = document.OuterXml;
            }
            #endregion
            #region Query for List
            #region Mutil Thread for list
            //使用多线程可以显著提高执行效率, 必要时可以使用多线程来提升效率, 从测试结果看限制最大线程数=2~3即可。
            //测试数据结构如下:
            //Custom List(without additional columns)
            //  |-RootFolder(6000Items)
            //      |-SubFoler(7000Items)
            //查询RootFolder下Item记录效率测试结果如下:
            //Threads	1	    2	    3	    4	    5	    10	    Unlimited
            //TEST1	    00:15.2	00:10.7	00:11.3	00:13.2	00:12.2	00:12.2	00:11.6
            //TEST2 	00:11.4	00:08.1	00:08.4	00:09.4	00:07.9	00:08.8	00:08.1
            //TEST3 	00:12.9	00:07.8	00:08.2	00:09.3	00:08.6	00:08.9	00:08.9
            //TEST4	    00:11.7	00:07.9	00:08.8	00:10.8	00:08.0	00:08.5	00:08.2
            //TEST5	    00:11.4	00:08.1	00:08.3	00:08.0	00:08.1	00:08.0	00:09.0
            //TEST6	    00:17.2	00:08.0	00:08.2	00:08.0	00:07.7	00:08.6	00:09.2
            //TEST7	    00:11.6	00:08.0	00:08.5	00:09.8	00:08.4	00:10.2	00:08.9
            //TEST8	    00:13.1	00:08.8	00:09.8	00:08.2	00:08.2	00:08.8	00:09.8
            //TEST9	    00:11.6	00:08.2	00:08.8	00:08.1	00:09.9	00:08.7	00:08.6
            //TEST10	00:10.8	00:08.0	00:09.9	00:10.0	00:08.0	00:12.1	00:08.0
            //Average	00:12.7	00:08.3	00:09.0	00:09.5	00:08.7	00:09.5	00:09.0
            #endregion
            private void QueryItemsUseIdIndex()
            {
                int minId;
                int maxId;
                int queryCount = 0;
                GetListItemMinAndMaxId(out minId, out maxId);
                int startIndex = GetPageStartIndex(minId);//冗余增加可读性
                while (startIndex <= maxId)
                {
                    mLogger.Debug("Start index: {0}, max id: {1}", startIndex, maxId);
                    var query = BuildCamlQueryById(startIndex, maxId);
                    SetCamlQueryUrl(query, this.folderServerRelatedUrl);
                    var listItems = this.list.GetItems(query);
                    this.BeforeQueryAction(this.context, listItems);
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        this.AfterQueryAction(context, item, isLibrary);
                        queryCount++;
                        if (rowLimitCount > 0 && queryCount >= rowLimitCount)
                        {
                            Position = listItems.ListItemCollectionPosition;
                            return;
                        }
                    }
                    SetRowLimit(this.query, rowLimitCount - queryCount);
                    startIndex += (int)this.perPage;
                    Position = listItems.ListItemCollectionPosition;
                }
            }

            private int GetPageStartIndex(int defaultValue)
            {
                if (this.query == null || this.query.ListItemCollectionPosition == null)
                {
                    return defaultValue;
                }
                try
                {
                    var pageInfo = this.query.ListItemCollectionPosition.PagingInfo;
                    var arguements = pageInfo.Split('&');
                    foreach (var arguement in arguements)
                    {
                        if (string.IsNullOrEmpty(arguement))
                        {
                            continue;
                        }

                        if (arguement.StartsWith("p_ID", StringComparison.OrdinalIgnoreCase))
                        {
                            return int.Parse(arguement.Split('=')[1]);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Anerror occurred while get p_ID property from pageinfo, error: {0}", e);
                }
                return defaultValue;
            }

            [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Lamda表达式参数")]
            private void GetListItemMinAndMaxId(out int minId, out int maxId)
            {
                var maxQuery = new CamlQuery()
                {
                    ViewXml = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Ascending='FALSE' Name='ID' /></OrderBy></Query><RowLimit>1</RowLimit></View>",
                };
                SetCamlQueryUrl(maxQuery, list.RootFolder.ServerRelativeUrl);
                var maxItems = this.list.GetItems(maxQuery);
                this.context.Load(maxItems, itemsArg => itemsArg.Include(itemArg => itemArg.Id));
                var minQuery = new CamlQuery()
                {
                    ViewXml = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Ascending='TRUE' Name='ID' /></OrderBy></Query><RowLimit>1</RowLimit></View>",
                };
                SetCamlQueryUrl(minQuery, list.RootFolder.ServerRelativeUrl);
                var minItems = this.list.GetItems(minQuery);
                this.context.Load(minItems, itemsArg => itemsArg.Include(itemArg => itemArg.Id));
                this.context.ExecuteQuery();

                minId = minItems.Count <= 0 ? 1: minItems[0].Id;//Root folder无法获取ListItem对象
                maxId = maxItems.Count <= 0 ? -1 : maxItems[0].Id;
            }

            [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Caml语法")]
            private CamlQuery BuildCamlQueryById(int startIndex, int maxId)
            {
                var endIndex = (startIndex + this.perPage) > maxId ? maxId + 1 : startIndex + this.perPage;
                if (this.query == null)
                {
                    return new CamlQuery()
                    {
                        // Id in [startInde, startIndex + perPage)
                        ViewXml = string.Format(
    @"<View Scope='{0}'>
    {1}
    <Query>
        <OrderBy><FieldRef Name='ID'/></OrderBy>
            <Where>
                <And>
                    <Geq><FieldRef Name='ID'/><Value Type='Integer'>{2}</Value></Geq>
                    <Lt><FieldRef Name='ID'/><Value Type='Integer'>{3}</Value></Lt>
                </And>    
            </Where>
    </Query>
</View>", findOption == QueryFindOption.None ? string.Empty : findOption.ToString(), GenerateViewFieldsString(), startIndex, endIndex),
                    };
                }
                else
                {
                    return new CamlQuery
                    {
                        ViewXml = string.Format(this.query.ViewXml, startIndex, endIndex),
                        ListItemCollectionPosition = this.query.ListItemCollectionPosition,
                    };
                }
            }
            #endregion

            #region Query for Library
            private void QueryItemsInLibrary()
            {
                var query = BuildCamlQueryByLeafName();
                do
                {
                    SetCamlQueryUrl(query, this.folderServerRelatedUrl);

                    query.ListItemCollectionPosition = Position;

                    var listItems = this.list.GetItems(query);
                    this.BeforeQueryAction(this.context, listItems);
                    context.ExecuteQuery();
                    foreach (ListItem item in listItems)
                    {
                        this.AfterQueryAction(context, item, isLibrary);
                    }
                    Position = listItems.ListItemCollectionPosition;
                }
                while (Position != null);
            }

            private CamlQuery BuildCamlQueryByLeafName()
            {
                return new CamlQuery()
                {
                    ViewXml = string.Format(
@"<View Scope='{0}'>
        {1}
        <Query>
            <OrderBy>
              <FieldRef Name='FileLeafRef'/>
            </OrderBy>
        </Query>
         <RowLimit>{2}</RowLimit>
</View>", findOption == QueryFindOption.None ? string.Empty : findOption.ToString(), GenerateViewFieldsString(), this.perPage)
                };
            }
            #endregion
            private string GenerateViewFieldsString()
            {
                var viewFieldsString = new StringBuilder();
                if (viewFields != null && viewFields.Count > 0)
                {
                    viewFieldsString.AppendLine("<ViewFields>");
                    foreach (var field in viewFields)
                    {
                        viewFieldsString.AppendLine(string.Format("<FieldRef Name='{0}'/>", field));
                    }
                    viewFieldsString.AppendLine("</ViewFields>");
                }
                return viewFieldsString.ToString();
            }
        }
        #endregion

        #region set
        public override void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            mRequestCommon.SetSiteEnabledHelpCollections(enabledHelpCollections);
        }

        public override bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, string experience)
        {
            bool isLikesExp = !string.IsNullOrEmpty(experience) && experience.Equals("Likes", StringComparison.OrdinalIgnoreCase) ? true : false;
            return mRequestCommon.SetListRateSetting(webServerRelativeUrl, listUrl, listId, enableRating, isLikesExp);
        }
        public override void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {

        }
        public override void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp)
        {

        }
        public override Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            return new Dictionary<string, object>();
        }
        public override Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter)
        {
            return new Dictionary<string, object>();
        }
        public override Dictionary<string, object> CreatePost(string targetId, Dictionary<string, object> creationData)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> postResultProperties = new Dictionary<string, object>();
                SocialFeedManager socialFeedManager = new SocialFeedManager(context);
                SocialPostCreationData socialPostCreationData = new SocialPostCreationData();
                socialPostCreationData.ContentText = creationData["ContentText"].ToString();
                socialPostCreationData.UpdateStatusText = bool.Parse(creationData["UpdateStatusText"].ToString());
                ClientResult<SocialThread> PostResult = socialFeedManager.CreatePost(targetId, socialPostCreationData);
                context.ExecuteQuery();
                AssembleSocialThreadProperties(postResultProperties, PostResult.Value);
                return postResultProperties;
            }
        }
        private void AssembleSocialThreadProperties(Dictionary<string, object> SocialThreadProperties, SocialThread socialThread)
        {
            #region basicInfo
            SocialThreadProperties.Add("Id", socialThread.Id);
            SocialThreadProperties.Add("Attributes", socialThread.Attributes);
            SocialThreadProperties.Add("OwnerIndex", socialThread.OwnerIndex);
            SocialThreadProperties.Add("Permalink", socialThread.Permalink);
            SocialThreadProperties.Add("ThreadType", socialThread.ThreadType);
            SocialThreadProperties.Add("TotalReplyCount", socialThread.TotalReplyCount);
            if (socialThread.PostReference != null)
            {
                SocialThreadProperties.Add("PostReference.ThreadId", socialThread.PostReference.ThreadId);
                SocialThreadProperties.Add("PostReference.ThreadOwnerIndex", socialThread.PostReference.ThreadOwnerIndex);
            }
            #endregion

            #region rootpost
            SocialThreadProperties.Add("RootPostText", socialThread.RootPost.Text);
            SocialThreadProperties.Add("RootPostId", socialThread.RootPost.Id);
            SocialThreadProperties.Add("RootPostAttributes", socialThread.RootPost.Attributes);
            SocialThreadProperties.Add("RootPostModifiedTime", socialThread.RootPost.ModifiedTime);
            SocialThreadProperties.Add("RootPostCreatedTime", socialThread.RootPost.CreatedTime);
            SocialThreadProperties.Add("RootPostAuthorIndex", socialThread.RootPost.AuthorIndex);
            SocialThreadProperties.Add("RootPostPreferredImageUri", socialThread.RootPost.PreferredImageUri ?? string.Empty);
            SocialThreadProperties.Add("RootPostPostType", socialThread.RootPost.PostType);
            SocialThreadProperties.Add("RootPostLikerInfoIncludesCurrentUser", socialThread.RootPost.LikerInfo.IncludesCurrentUser);
            SocialThreadProperties.Add("RootPostLikerInfoIndexes", socialThread.RootPost.LikerInfo.Indexes);
            SocialThreadProperties.Add("RootPostLikerInfoTotalCount", socialThread.RootPost.LikerInfo.TotalCount);
            #endregion

            #region actors
            List<Dictionary<string, object>> Actors = new List<Dictionary<string, object>>();
            foreach (SocialActor actor in socialThread.Actors)
            {
                Dictionary<string, object> aveOSocialActors = new Dictionary<string, object>();
                aveOSocialActors["AccountName"] = actor.AccountName ?? string.Empty;
                aveOSocialActors["ActorType"] = actor.ActorType;
                aveOSocialActors["CanFollow"] = actor.CanFollow;
                aveOSocialActors["ContentUri"] = actor.ContentUri ?? string.Empty;
                aveOSocialActors["EmailAddress"] = actor.EmailAddress ?? string.Empty;
                aveOSocialActors["FollowedContentUri"] = actor.FollowedContentUri ?? string.Empty;
                aveOSocialActors["Id"] = actor.Id ?? string.Empty;
                aveOSocialActors["ImageUri"] = actor.ImageUri ?? string.Empty;
                aveOSocialActors["IsFollowed"] = actor.IsFollowed;
                aveOSocialActors["LibraryUri"] = actor.LibraryUri ?? string.Empty;
                aveOSocialActors["Name"] = actor.Name ?? string.Empty;
                aveOSocialActors["PersonalSiteUri"] = actor.PersonalSiteUri ?? string.Empty;
                aveOSocialActors["Status"] = actor.Status;
                aveOSocialActors["StatusText"] = actor.StatusText ?? string.Empty;
                aveOSocialActors["TagGuid"] = actor.TagGuid;
                aveOSocialActors["Title"] = actor.Title ?? string.Empty;
                aveOSocialActors["Uri"] = actor.Uri ?? string.Empty;
                Actors.Add(aveOSocialActors);
            }
            SocialThreadProperties.Add("Actors", Actors);
            #endregion

            #region replies
            List<Dictionary<string, object>> replies = new List<Dictionary<string, object>>();
            foreach (SocialPost reply in socialThread.Replies)
            {
                Dictionary<string, object> aveOSocialReply = new Dictionary<string, object>();
                aveOSocialReply["Id"] = reply.Id;
                aveOSocialReply["Text"] = reply.Text;
                aveOSocialReply["ModifiedTime"] = reply.ModifiedTime;
                aveOSocialReply["CreatedTime"] = reply.CreatedTime;
                aveOSocialReply["AuthorIndex"] = reply.AuthorIndex;
                aveOSocialReply["LikerInfoIncludesCurrentUser"] = reply.LikerInfo.IncludesCurrentUser;
                aveOSocialReply["LikerInfoIndexes"] = reply.LikerInfo.Indexes;
                aveOSocialReply["LikerInfoTotalCount"] = reply.LikerInfo.TotalCount;
                replies.Add(aveOSocialReply);
            }
            SocialThreadProperties["Replies"] = replies;
            #endregion
        }
        public override Dictionary<string, object> GetFullThread(string threadId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> postResultValue = new Dictionary<string, object>();
                SocialFeedManager socialFeedManager = new SocialFeedManager(context);
                ClientResult<SocialThread> FullThreadResult = socialFeedManager.GetFullThread(threadId);
                context.ExecuteQuery();
                AssembleSocialThreadProperties(postResultValue, FullThreadResult.Value);
                return postResultValue;
            }
        }
        public override Dictionary<string, object> LikePost(string postId)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> postResultValue = new Dictionary<string, object>();
                SocialFeedManager socialFeedManager = new SocialFeedManager(context);
                ClientResult<SocialThread> LikePostResult = socialFeedManager.LikePost(postId);
                context.ExecuteQuery();
                return postResultValue;
            }
        }
        public override Dictionary<string, object> GetFeedFor(string postId, Dictionary<string, object> options)
        {
            using (ClientContext context = CreateContext())
            {
                SocialFeedManager socialFeedManager = new SocialFeedManager(context);
                SocialFeedOptions socialFeedOptions = new SocialFeedOptions();
                socialFeedOptions.MaxThreadCount = int.Parse(options["MaxThreadCount"].ToString());
                socialFeedOptions.NewerThan = DateTime.Parse(options["NewerThan"].ToString());
                socialFeedOptions.OlderThan = DateTime.Parse(options["OlderThan"].ToString());
                socialFeedOptions.SortOrder = (SocialFeedSortOrder)Enum.Parse(typeof(SocialFeedSortOrder), options["SortOrder"].ToString(), true);
                ClientResult<SocialFeed> GetFeedForResult = socialFeedManager.GetFeedFor(postId, socialFeedOptions);
                context.ExecuteQuery();

                Dictionary<string, object> postResultProperties = new Dictionary<string, object>();
                SocialThread[] tmpThread = GetFeedForResult.Value.Threads;
                List<Dictionary<string, object>> threads = new List<Dictionary<string, object>>();
                foreach (SocialThread socialThread in tmpThread)
                {
                    Dictionary<string, object> SocialThreadProperties = new Dictionary<string, object>();
                    AssembleSocialThreadProperties(SocialThreadProperties, socialThread);
                    threads.Add(SocialThreadProperties);
                }
                postResultProperties["Threads"] = threads;
                return postResultProperties;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is a part of folder property name")]
        protected override void GetRootFolderProperties(Dictionary<string, object> rootFolderProp, AveClientContext context, List l)
        {
            Guid folderUniqueId = Guid.Empty;
            context.Load(l.RootFolder, r => r.ServerRelativeUrl, r => r.Name, r => r.Properties);
            context.ExecuteQuery();
            if (l.RootFolder.Properties.FieldValues.ContainsKey("vti_etag") && l.RootFolder.Properties["vti_etag"] != null)
            {
                string tagString = l.RootFolder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                folderUniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
            }
            if (folderUniqueId != Guid.Empty)
            {
                rootFolderProp["UniqueId"] = folderUniqueId;
            }
            AveObjectCopy.GetObjectBasicProperties(rootFolderProp, l.RootFolder);
            rootFolderProp["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable();
        }

        public override void MoveTo(string parentWebUrl, string parentWebServerRelativeUrl, string folderServerRelativeUrl, string newUrl)
        {
            using (ClientContext context = CreateContext())
            {
                mRequestCommon.MoveTo(parentWebServerRelativeUrl, folderServerRelativeUrl, newUrl);
                //RPC不抛异常, 在这里特殊判断下。
                if (GetFolderExists(context as AveClientContext, parentWebServerRelativeUrl, folderServerRelativeUrl))
                {
                    throw new Exception(string.Format("Move folder from {0} to {1} failed.", folderServerRelativeUrl, newUrl));
                }
            }
        }

        private bool GetFolderExists(AveClientContext context, string parentWebServerRelativeUrl, string folderServerRelativeUrl)
        {
            Web web = context.Site.OpenWeb(parentWebServerRelativeUrl);
            var folder = GetFolderByAPI(web, folderServerRelativeUrl);
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            LoadFolderProperties(folderProperties, context as AveClientContext, folder, parentWebServerRelativeUrl, folderServerRelativeUrl);
            return (bool)folderProperties["Exists"];
        }

        protected virtual void LoadFolderProperties(Dictionary<string, object> folderProperties, AveClientContext context, Folder folder, string webServerRelativeUrl, object folderUrlOrId)
        {
            bool folderServerRelativeUrlVaild = true;
            context.Load(folder, f => f.ParentFolder);
            try
            {
                LoadFolderProperties(context, webServerRelativeUrl, Guid.Empty, folder, folderProperties);
                folderProperties["Exists"] = true;
                folderServerRelativeUrlVaild = true;
            }
            catch (ServerUnauthorizedAccessException e)
            {
                context.Load(folder, f => f.ServerRelativeUrl);
                context.Load(folder, f => f.Name);
                context.Load(folder, f => f.ParentFolder);
                context.ExecuteQuery();
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject folderTrimObj = webTrimObj.GetFolder(folder.ServerRelativeUrl, folder.Name);
                folderTrimObj.TrimmedProperties["Files"] = e.Message;
                folderTrimObj.TrimmedProperties["Folders"] = e.Message;
                folderTrimObj.TrimmedProperties["Tag"] = e.Message;
                folderTrimObj.TrimmedProperties["ItemCount"] = e.Message;
                folderTrimObj.TrimmedProperties["UniqueContentTypeOrder"] = e.Message;
                folderTrimObj.TrimmedProperties["WelcomePage"] = e.Message;
                folderTrimObj.TrimmedProperties["ServerObjectIsNull"] = e.Message;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Folder:{0} not exists.Error Message:{1}", folderUrlOrId, ex);
                folderProperties["Exists"] = false;
                folderServerRelativeUrlVaild = false;
            }
            if (folderServerRelativeUrlVaild == true)
            {
                string Url = string.Empty;
                if (!folder.ServerRelativeUrl.TrimEnd('/').Equals(webServerRelativeUrl.TrimEnd('/')))
                {
                    Url = folder.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                }
                folderProperties["Url"] = Url;
                folderProperties["ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix] = folder.ParentFolder.ServerRelativeUrl;
            }
        }
        protected override Dictionary<string, object> GetFolder(AveClientContext context, string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            if (!string.IsNullOrEmpty(folderServerRelativeUrl) && !folderServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                folderServerRelativeUrl = "/" + folderServerRelativeUrl;
            }
            Folder folder = null;
            folder = GetFolderByAPI(web, folderServerRelativeUrl);
            LoadFolderProperties(folderProperties, context as AveClientContext, folder, webServerRelativeUrl, folderServerRelativeUrl);
            return folderProperties;
        }
        public override Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type)
        {
            Dictionary<string, string> customProperties = new Dictionary<string, string>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermSetItem item = null;
                if (type == AveTermSetItemType.TermSet)
                {
                    item = store.GetTermSet(termSetId) as TermSetItem;
                }
                else
                {
                    item = store.GetTermInTermSet(termSetId, termId) as TermSetItem;
                }
                item.SetCustomProperty(name, value);
                context.Load(item, it => it.CustomProperties);
                context.ExecuteQuery();
                foreach (KeyValuePair<string, string> pair in item.CustomProperties)
                {
                    customProperties[pair.Key] = pair.Value;
                }
            }
            return customProperties;
        }
        public override Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value)
        {
            Dictionary<string, string> localCustomProperties = new Dictionary<string, string>();
            using (ClientContext context = CreateContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                Term term = store.GetTermInTermSet(termSetId, termId);
                term.SetLocalCustomProperty(name, value);
                context.Load(term, t => t.LocalCustomProperties);
                context.ExecuteQuery();
                foreach (KeyValuePair<string, string> pair in term.LocalCustomProperties)
                {
                    localCustomProperties[pair.Key] = pair.Value;
                }
            }
            return localCustomProperties;
        }

        //public override Dictionary<string, object> SetWebNavigationSettings(string webServerRelativeUrl, int globalSource, int currentSource, Dictionary<string, Guid> globalTaxonomy, Dictionary<string, Guid> currentTaxonomy)
        //{
        //    Dictionary<string, object> webProp = new Dictionary<string, object>();
        //    using (ClientContext context = CreateContext())
        //    {
        //        TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
        //        Web web = context.Site.OpenWeb(webServerRelativeUrl);
        //        WebNavigationSettings navigationSettings = new WebNavigationSettings(context, web);
        //        if (globalSource == (int)StandardNavigationSource.TaxonomyProvider)
        //        {
        //            navigationSettings.GlobalNavigation.Source = StandardNavigationSource.TaxonomyProvider;
        //            navigationSettings.GlobalNavigation.TermStoreId = globalTaxonomy["TermStoreId"];
        //            navigationSettings.GlobalNavigation.TermSetId = globalTaxonomy["TermSetId"];
        //        }
        //        else if (globalSource == (int)StandardNavigationSource.PortalProvider)
        //        {
        //            navigationSettings.GlobalNavigation.Source = StandardNavigationSource.PortalProvider;
        //        }
        //        else
        //        {
        //            navigationSettings.GlobalNavigation.Source = StandardNavigationSource.InheritFromParentWeb;
        //        }

        //        if (currentSource == (int)StandardNavigationSource.TaxonomyProvider)
        //        {
        //            navigationSettings.CurrentNavigation.Source = StandardNavigationSource.TaxonomyProvider;
        //            navigationSettings.CurrentNavigation.TermStoreId = currentTaxonomy["TermStoreId"];
        //            navigationSettings.CurrentNavigation.TermSetId = currentTaxonomy["TermSetId"];
        //        }
        //        else if (currentSource == (int)StandardNavigationSource.PortalProvider)
        //        {
        //            navigationSettings.CurrentNavigation.Source = StandardNavigationSource.PortalProvider;
        //        }
        //        else
        //        {
        //            navigationSettings.CurrentNavigation.Source = StandardNavigationSource.InheritFromParentWeb;
        //        }
        //        navigationSettings.Update(session);
        //        context.ExecuteQuery();
        //        //webProp = GetWebProperties(context, context.Site.RootWeb, mWebUrl, context.Site.ServerRelativeUrl, true);
        //    }
        //    return webProp;
        //}

        #endregion


        public override Dictionary<string, object> GetWebAppById(string webServerRelativeUrl, Guid appId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                AppInstance apps = web.GetAppInstanceById(appId);
                context.Load(apps);
                context.ExecuteQuery();
                List<Dictionary<string, object>> appsMetadata = GetInstalledApps(webServerRelativeUrl);
                Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                CopyProperty(appInstanceProperties, apps);
                if (!string.IsNullOrEmpty(apps.AppWebFullUrl))
                {
                    appInstanceProperties["AppWebFullUrl"] = new Uri(apps.AppWebFullUrl);
                }
                Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, apps.Id);
                appInstanceProperties["App"] = AssembleAppProperties(appMetadata);
                return appInstanceProperties;
            }
        }

        public override Dictionary<string, object> GetSiteBasicProperties()
        {
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Site site = context.Site;
                    Web web = context.Web;
                    context.Load(site);
                    context.Load(web);
                    context.ExecuteQuery();
                    Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                    siteProperties.Add("CompatibilityLevel", site.CompatibilityLevel);
                    siteProperties.Add("RootWebWebTemplate", web.WebTemplate);
                    siteProperties.Add("RootWebServerRelativeUrl", web.ServerRelativeUrl);

                    return siteProperties;
                }
                catch (Exception e)
                {
                    mLogger.Error("Get site basic properties failed, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        public override List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames)
        {
            List<Dictionary<string, object>> skyDriveProInfos = new List<Dictionary<string, object>>();
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    PeopleManager pm = new PeopleManager(context);
                    Dictionary<string, PersonProperties> props = new Dictionary<string, PersonProperties>();
                    int batchSize = 250;
                    foreach (string username in usernames)
                    {
                        PersonProperties prop = pm.GetPropertiesFor(string.Format("i:0#.f|membership|{0}", username));
                        context.Load(prop, p => p.PersonalUrl);
                        context.Load(context.Site, s => s.ReadOnly);
                        props.Add(username, prop);
                        if (props.Count >= batchSize && props.Count % batchSize == 0)
                        {
                            context.ExecuteQuery();
                        }
                    }
                    if (context.HasPendingRequest)
                    {
                        context.ExecuteQuery();
                    }

                    foreach (KeyValuePair<string, PersonProperties> prop in props)
                    {
                        var oneDetail = AssembleSkyDriveProProperties(prop.Value, prop.Key);
                        oneDetail["ReadOnly"] = context.Site.ReadOnly;
                        skyDriveProInfos.Add(oneDetail);
                    }

                    return skyDriveProInfos;
                }
                catch (Exception e)
                {
                    mLogger.Error("Get SkyDrivePro information failed, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        private Dictionary<string, object> AssembleSkyDriveProProperties(PersonProperties prop, string username = null)
        {
            Dictionary<string, object> skyDriveProp = new Dictionary<string, object>();
            bool isUsernameExists = prop.ServerObjectIsNull.HasValue && prop.ServerObjectIsNull == false;
            skyDriveProp["Exists"] = isUsernameExists;
            skyDriveProp["PersonalUrl"] = isUsernameExists ? prop.PersonalUrl : string.Empty;
            if (isUsernameExists)
            {
                Uri personalUrl = new Uri(prop.PersonalUrl, UriKind.RelativeOrAbsolute);
                if ((personalUrl.IsAbsoluteUri && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("Persona.aspx", StringComparison.OrdinalIgnoreCase)))
                {
                    skyDriveProp["PersonalSpace"] = prop.PersonalUrl;
                }
            }
            else
            {
                skyDriveProp["PersonalSpace"] = string.Empty;
            }
            skyDriveProp["UserName"] = username;
            skyDriveProp["Version"] = prop.Context.ServerLibraryVersion.ToString();
            return skyDriveProp;
        }

        public override int GetSiteOwnerId() //check addmin permission when add single sitecollection or save scan results
        {
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    Site site = context.Site;
                    context.Load(site.Owner, o => o.Id);
                    context.ExecuteQuery();
                    return site.Owner.Id;
                }
                catch (Exception e)
                {
                    mLogger.Error("Get site owner id failed, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        public override void DeleteSite(string CAUrl, string url)
        {
            mWebServiceRequest.DeleteSite(CAUrl, url);
        }

        public override Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop)
        {
            if (prop.ContainsKey("ChangedMetaInfo"))
            {
                Dictionary<string, object> changedMetaInfo = prop["ChangedMetaInfo"] as Dictionary<string, object>;
                mRequestCommon.UpdateFileProperties(webServerRelativeUrl, fileServerRelativeUrl, changedMetaInfo);
            }
            return null;
        }

        public override void CustomizeReport(Dictionary<string, object> parameters, Guid reportId)
        {
            mRequestCommon.CustomizeReport(parameters);
        }


        public override Dictionary<string, object> GetFieldValueAsTaxonomyFieldValue(string webRelativeUrl, Guid listId, Guid fieldId, string text)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);

                Field field;
                if (listId != Guid.Empty)
                {//List Column
                    List list = web.Lists.GetById(listId);
                    field = list.Fields.GetById(fieldId);
                }
                else
                {//WebColumn
                    field = web.Fields.GetById(fieldId);
                }
                context.Load(field);
                context.ExecuteQuery();
                ClientResult<TaxonomyFieldValue> fieldValue = (field.TypedObject as TaxonomyField).GetFieldValueAsTaxonomyFieldValue(text);
                //try
                //{
                context.ExecuteQuery();
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties["WssId"] = fieldValue.Value.WssId;
                properties["TermGuid"] = fieldValue.Value.TermGuid;
                properties["Label"] = fieldValue.Value.Label;
                return properties;
                //}
                //catch (Exception ex) 
                //{
                //    mLogger.Warn("Get taxonomy field value by text failed.Error Message:{0}", ex.ToString());
                //    return new Dictionary<string, object>();
                //}
            }
        }

        public override Dictionary<string, object> OperateSolution(string operation, string siteUrl, string webServerRelativeUrl, int id)
        {
            mWebServiceRequest.OperateSolution(operation, siteUrl, webServerRelativeUrl, id);
            return base.OperateSolution(operation, siteUrl, webServerRelativeUrl, id);
        }
        public override Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl)
        {
            return mWebServiceRequest.GetMetaInfo(webServerRelativeUrl, docServerRelativeUrl);
        }

        public override void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            this.mRequestCommon.DeclareOrUndeclareItem(itemId, listId, webUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="contentTypeId"></param>
        public override void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            this.mRequestCommon.UpdateWorkflowAssociationsOnChildren(webUrl, contentTypeId);
        }

        public override Dictionary<string, Dictionary<string, int>> GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, Guid listId, List<string> fieldNameList)
        {

            var idMapping = new Dictionary<string, Dictionary<string, int>>();
            if (fieldNameList == null || fieldNameList.Count == 0)
            {
                return idMapping;
            }
            fieldNameList.ForEach(name => idMapping.Add(name, new Dictionary<string, int>()));
            try
            {
                using (var context = CreateContext())
                {
                    var web = context.Site.OpenWeb(webServerRelativeUrl);
                    var list = web.Lists.GetById(listId);
                    ListItemCollectionPosition position = null;
                    do
                    {
                        var camlQuery = new CamlQuery();
                        var viewFields = new List<string> { AveFieldNameCollection.FileDirRef_Field };
                        viewFields.AddRange(fieldNameList);
                        camlQuery.ViewXml = AveCamlQueryString.GetAllItemsString(viewFields, 5000, QueryFindOption.RecursiveAll);
                        //camlQuery.FolderServerRelativeUrl = rootFolderServerRelativeUrl;
                        SetCamlQueryFolderUrl(camlQuery, rootFolderServerRelativeUrl);
                        camlQuery.ListItemCollectionPosition = position;
                        var items = list.GetItems(camlQuery);
                        context.Load(items);
                        context.Load(items, it => it.ListItemCollectionPosition);
                        context.ExecuteQuery();
                        position = items.ListItemCollectionPosition;
                        foreach (var item in items)
                        {
                            object fileDirRef;
                            if (item.FieldValues.TryGetValue(AveFieldNameCollection.FileDirRef_Field, out fileDirRef) && fileDirRef != null)
                            {
                                fieldNameList.ForEach(name =>
                                {
                                    object fieldValue;
                                    if (item.FieldValues.TryGetValue(name, out fieldValue) && fieldValue != null)
                                    {
                                        var mappingKey = fieldValue + fileDirRef.ToString().Substring(rootFolderServerRelativeUrl.Length);
                                        idMapping[name][mappingKey] = item.Id;
                                    }
                                });
                            }

                        }
                    } while (position != null);
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to get list item Guid and RowId mappings due to: {0},WebUrl:{1},ListId:{2}", e, webServerRelativeUrl, listId);
            }
            return idMapping;
        }

        public override void PublishSharepointList(string webServerRelativeUrl, IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {
            mWebServiceRequest.PublishSharepointList(webServerRelativeUrl, templateFile, lcid, listId, contentTypeId);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of keys")]
        protected override void AddParentFolderToCache(ClientContext context, List list, Folder folder, Dictionary<string, object> existFolders, List<Dictionary<string, object>> changeFolderCache)
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
            folderProperties["ChangeType"] = AvePoint.Wrapper.Common.ChangeType.None;
            folderProperties["LeafName"] = folder.Name;
            folderProperties["Versions"] = new List<Dictionary<string, object>>();
            string dirName = folderUrl.Substring(0, folderUrl.LastIndexOf('/'));
            string leafName = folderUrl.Substring(folderUrl.LastIndexOf('/') + 1);
            ListItem item;
            try
            {
                item = GetListItemByDirName(context, list, dirName, leafName);
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred GetListItemByDirName. ERROR:{0}", e);
                var mItem = folder.ListItemAllFields;
                context.Load(mItem, it => it.Id);
                context.ExecuteQuery();
                item = GetListItemByID(context, list, mItem.Id);
            }
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


        public override AveWebMasterPageInfo GetRootWebMasterPageInfo()
        {
            AveWebMasterPageInfo pageInfo = new AveWebMasterPageInfo();
            using (AveClientContext context = CreateContext())
            {
                try
                {
                    context.Load(context.Site.RootWeb);
                    context.ExecuteQuery();
                    pageInfo.CPageUrl = context.Site.RootWeb.CustomMasterUrl;
                    pageInfo.MPageUrl = context.Site.RootWeb.MasterUrl;
                    mLogger.Info("CustomMasterUrl:{0}, MasterUrl:{1}", pageInfo.CPageUrl, pageInfo.MPageUrl);
                }
                catch (Exception e)
                {
                    mLogger.Warn("GetRootWebMasterPageInfo error, message:{0}", e.ToString());
                }
            }
            return pageInfo;
        }

        public override void SetRootWebAndMySiteWebMasterPageInfo(string mySiteWebServerRelativeUrl, AveWebMasterPageInfo pageInfo)
        {
            if (pageInfo != null)
            {
                mLogger.Info("SetRootWebAndMySiteWebMasterPageInfo, CustomMasterUrl:{0}, MasterUrl:{1}", pageInfo.CPageUrl, pageInfo.MPageUrl);
                using (AveClientContext context = CreateContext())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(pageInfo.CPageUrl))
                        {
                            context.Site.RootWeb.CustomMasterUrl = pageInfo.CPageUrl;
                        }
                        if (!string.IsNullOrEmpty(pageInfo.MPageUrl))
                        {
                            context.Site.RootWeb.MasterUrl = pageInfo.MPageUrl;
                        }
                        if (!string.IsNullOrEmpty(pageInfo.MPageUrl) || !string.IsNullOrEmpty(pageInfo.CPageUrl))
                        {
                            context.Site.RootWeb.Update();
                            try
                            {
                                context.ExecuteQuery();
                            }
                            catch (ServerUnauthorizedAccessException e)
                            {
                                mLogger.Debug("Catch a ServerUnauthorizedAccessException exception but can still set value correctly. {0}", e.Message);
                            }
                        }
                        //the newly created web type of mysite url is invalid too, set it to root site. 
                        var newCreateWeb = string.IsNullOrEmpty(mySiteWebServerRelativeUrl) ? context.Web : context.Site.OpenWeb(mySiteWebServerRelativeUrl);
                        if (!string.IsNullOrEmpty(pageInfo.CPageUrl))
                        {
                            newCreateWeb.CustomMasterUrl = pageInfo.CPageUrl;
                        }
                        if (!string.IsNullOrEmpty(pageInfo.MPageUrl))
                        {
                            newCreateWeb.MasterUrl = pageInfo.MPageUrl;
                        }
                        if (!string.IsNullOrEmpty(pageInfo.MPageUrl) || !string.IsNullOrEmpty(pageInfo.CPageUrl))
                        {
                            newCreateWeb.Update();
                            try
                            {
                                context.ExecuteQuery();
                            }
                            catch (ServerUnauthorizedAccessException e)
                            {
                                mLogger.Debug("Catch a ServerUnauthorizedAccessException exception but can still set value correctly. {0}", e.Message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("SetRootWebAndMySiteWebMasterPageInfo Failed, exception:{0}", e.ToString());
                    }
                }
            }
        }

        public virtual void UpdateSupportedUICulture(Dictionary<string, object> webProperties, Web web, ref bool changed)
        {
        }


        public override Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            using (AveClientContext context = CreateContext())
            {
                Guid id = Guid.Empty;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list);
                context.Load(web, w => w.ServerRelativeUrl);
                context.Load(list.RootFolder, f => f.ServerRelativeUrl);
                context.ExecuteQuery();
                int listTemplate = list.BaseTemplate;
                if (listTemplate != (int)ListTemplateType.Survey)
                {
                    var filesMap = new Dictionary<string, ClientFile>();
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    List<Dictionary<string, object>> itemList = new List<Dictionary<string, object>>();
                    var query = new CamlQuery
                    {
                        ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq></Where></Query></View>", tp_Guid)
                    };
                    QueryItemsByQueryStringForLargeList(context, list, web.ServerRelativeUrl, list.RootFolder.ServerRelativeUrl, scope, filesMap, itemList, query);
                    if (itemList.Count > 0)
                    {
                        id = (Guid)itemList[0]["UniqueId"];
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


        #region MetaDefaults
        //need debug for records.
        public override string GetFieldDefault(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string fieldName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.RootFolder);
                context.ExecuteQuery();
                Folder formsFolder = list.ParentWeb.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + "/forms");
                context.Load(formsFolder, f => f.Files);
                context.ExecuteQuery();
                folderUrl = Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderUrl, false);
                var clientLocationBasedDefaultsFile =
                    formsFolder.Files.FirstOrDefault(
                        f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

                if (clientLocationBasedDefaultsFile != null)
                {
                    string defaultValues = ReadFileContent(clientLocationBasedDefaultsFile);
                    var defaultsXmlDoc = new XmlDocument();
                    defaultsXmlDoc.LoadXml(defaultValues);
                    XmlNode xmlNode = SelectSingleFieldDefaultNode(defaultsXmlDoc, folderUrl, fieldName);
                    var existFolderDefaultValue = xmlNode.InnerText;
                    return existFolderDefaultValue;
                }

            }
            return string.Empty;
        }

        //need debug records....
        public override bool RemoveFieldDefault(string webServerRelativeUrl, string listName, Guid listId, string folderPath, string fieldName)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    context.Load(list, l => l.RootFolder);
                    context.ExecuteQuery();
                    Folder formsFolder = list.ParentWeb.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + "/forms");
                    var defaultValues = string.Empty;
                    context.Load(formsFolder, f => f.Files);
                    context.ExecuteQuery();
                    folderPath = Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false);

                    var clientLocationBasedDefaultsFile =
                        formsFolder.Files.FirstOrDefault(
                            f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

                    if (clientLocationBasedDefaultsFile != null)
                    {
                        defaultValues = ReadFileContent(clientLocationBasedDefaultsFile);
                    }
                    if (!string.IsNullOrEmpty(defaultValues))
                    {
                        mLogger.Warn("'/forms/client_LocationBasedDefaults.html' is not exist.");
                        var defaultsXmlDoc = new XmlDocument();
                        defaultsXmlDoc.LoadXml(defaultValues);
                        defaultsXmlDoc = RemoveFieldDefault(defaultsXmlDoc, folderPath, fieldName);
                        var fci = new FileCreationInformation();
                        fci.Content = Encoding.UTF8.GetBytes(defaultsXmlDoc.OuterXml);
                        fci.Url = "client_LocationBasedDefaults.html";
                        fci.Overwrite = true;
                        var metaDataFile = formsFolder.Files.Add(fci);

                        context.Load(metaDataFile);
                        context.ExecuteQuery();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                mLogger.Warn("Remove folder default value failed {0}", e.ToString());
                return false;
            }
        }

        public override bool SetFieldDefault(string webServerRelativeUrl, string listName, Guid listId, string folderPath, string fieldName, string value)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    context.Load(list, l => l.RootFolder);
                    context.ExecuteQuery();
                    Folder formsFolder = list.ParentWeb.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + "/forms");
                    var defaultValues = string.Empty;
                    var defaultsXmlDoc = new XmlDocument();
                    context.Load(formsFolder, f => f.Files);
                    context.ExecuteQuery();
                    folderPath = Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false);
                    var clientLocationBasedDefaultsFile =
                        formsFolder.Files.FirstOrDefault(
                            f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

                    if (clientLocationBasedDefaultsFile != null)
                    {
                        defaultValues = ReadFileContent(clientLocationBasedDefaultsFile);
                    }
                    else
                    {
                        defaultValues = @"<MetadataDefaults></MetadataDefaults>";
                    }
                    if (!string.IsNullOrEmpty(defaultValues))
                    {
                        mLogger.Warn("'/forms/client_LocationBasedDefaults.html' is not exist.");

                        defaultsXmlDoc.LoadXml(defaultValues);
                        defaultsXmlDoc = AddOrUpdateFieldDefault(defaultsXmlDoc, folderPath, fieldName, value);
                        var fci = new FileCreationInformation();
                        fci.Content = Encoding.UTF8.GetBytes(defaultsXmlDoc.OuterXml);
                        fci.Url = "client_LocationBasedDefaults.html";
                        fci.Overwrite = true;
                        var metaDataFile = formsFolder.Files.Add(fci);

                        context.Load(metaDataFile);
                        context.ExecuteQuery();
                        RegisterEventReceiver(list);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                mLogger.Warn("Set folder default value failed {0}", e.ToString());
                return false;
            }
        }
        #endregion

        #region private xml method for handle folder default value change or remove (For Cloud Records.)
        private XmlNode SelectSingleFieldDefaultNode(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']/DefaultValue[@FieldName='{1}']", new object[]
            {
                folderPath,
        //Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false),
        fieldName
            }));
        }
        private XmlDocument AddOrUpdateFieldDefault(XmlDocument defaultsXml, string folderPath, string fieldName, string value)
        {
            XmlNode xmlNode = this.SelectSingleFolderNode(defaultsXml, folderPath);
            if (xmlNode == null)
            {
                XmlElement linkEle = defaultsXml.CreateElement("a");
                linkEle.SetAttribute("href", folderPath);
                XmlElement valueEle = defaultsXml.CreateElement("DefaultValue");
                valueEle.SetAttribute("FieldName", fieldName);
                valueEle.InnerText = value;
                linkEle.AppendChild(valueEle);
                defaultsXml.DocumentElement.AppendChild(linkEle);
                return defaultsXml;
            }
            XmlNode xmlNode2 = xmlNode.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "./DefaultValue[@FieldName='{0}']", new object[]
            {
        fieldName
            }));
            if (xmlNode2 != null)
            {
                xmlNode.RemoveChild(xmlNode2);
                XmlElement valueEle1 = defaultsXml.CreateElement("DefaultValue");
                valueEle1.SetAttribute("FieldName", fieldName);
                valueEle1.InnerText = value;
                xmlNode.AppendChild(valueEle1);
            }

            return defaultsXml;
        }
        private XmlDocument RemoveFieldDefault(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            XmlNode xmlNode = this.SelectSingleFolderNode(defaultsXml, folderPath);
            if (xmlNode == null)
            {
                return defaultsXml;
            }
            XmlNode xmlNode2 = xmlNode.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "./DefaultValue[@FieldName='{0}']", new object[]
            {
        fieldName
            }));
            if (xmlNode2 == null)
            {
                return defaultsXml;
            }
            xmlNode.RemoveChild(xmlNode2);
            if (!xmlNode.HasChildNodes)
            {
                defaultsXml.DocumentElement.RemoveChild(xmlNode);
            }
            return defaultsXml;
        }
        private string ReadFileContent(ClientFile file)
        {
            ClientResult<System.IO.Stream> stream = file.OpenBinaryStream();
            file.Context.ExecuteQuery();

            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream.Value, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        private XmlNode SelectSingleFolderNode(XmlDocument defaultsXml, string folderPath)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']", new object[]
            {
                folderPath
        //Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false)
            }));
        }

        public void RegisterEventReceiver(List destinationList)
        {
            EventReceiverDefinitionCreationInformation erci = GetEventReceiverCreationInformation();
            EnsureEventReceiver(destinationList, erci);
        }

        private EventReceiverDefinitionCreationInformation GetEventReceiverCreationInformation()
        {
            var erci = new EventReceiverDefinitionCreationInformation();

            erci.ReceiverName = "LocationBasedMetadataDefaultsReceiver ItemAdded";
            erci.SequenceNumber = 1000;
            erci.ReceiverClass = "Microsoft.Office.DocumentManagement.LocationBasedMetadataDefaultsReceiver";
            if (Type == AveClientRequestType.AveClientOM2016Request || Type == AveClientRequestType.AveClientOM2019Request || Type == AveClientRequestType.AveClientOMOffice365Request)
            {
                erci.ReceiverAssembly =
                "Microsoft.Office.DocumentManagement, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            }
            else if (Type == AveClientRequestType.AveClientOM2013Request)
            {
                erci.ReceiverAssembly =
                    "Microsoft.Office.DocumentManagement, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            }
            erci.EventType = EventReceiverType.ItemAdded;
            erci.Synchronization = EventReceiverSynchronization.Synchronous;

            return erci;
        }

        private void EnsureEventReceiver(List destinationList, EventReceiverDefinitionCreationInformation erci)
        {
            var destinationContext = (ClientContext)destinationList.Context;

            destinationContext.Load(destinationList.EventReceivers);
            destinationContext.ExecuteQuery();
            var receiver = destinationList.EventReceivers.FirstOrDefault(e => e.ReceiverName == erci.ReceiverName);

            if (receiver == null)
            {
                receiver = destinationList.EventReceivers.Add(erci);
                destinationContext.Load(receiver);
                destinationContext.ExecuteQuery();
            }
        }

        #endregion

        public override string GetWebTemplateTitle(string siteUrl, uint language, string templateName)
        {
            return GetWebTemplateTitle(siteUrl, language, templateName, "15");
        }

    }

    static class ReconnectableHttpWebRequestExtension
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(ReconnectableHttpWebRequestExtension));
        public static void RefreshDigestInfo(this ReconnectableHttpWebRequest request, ClientContext context, FormDigestProvider provider)
        {
            request.RequestFailed += (r, e) =>
            {
                try
                {
                    mLogger.Info("Refresh form digest");
                    request.Headers["X-RequestDigest"] = provider.GetFormDigest(context).DigestValue;
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Refresh digest at request excutor.error:{0}", ex);
                }
            };
        }
    }
}


