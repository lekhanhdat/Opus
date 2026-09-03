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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Reflection;
using System.Net;
using System.Xml;
using System.Globalization;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.ObjectModel.Common.Workflow;
using System.IO;
using AveClientRequest.Common;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/01/31", "Navy.Li@avepoint.com", "yanjun.wang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_CS_1 }, null, true)]
    [AveCodeReview("2012/04/19", "yuzhi.jiang@avepoint.com", "yanjun.wang@AvePoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CO_5 }, null, true)]

    class AveWeb : AveSecurableObject, IAveWeb
    {

        private new IAveRequest mRequest;
        private AveSite mSite;
        private AveUserCollection mSiteAdministrators;
        private AveList mSiteUserInfoList;
        private AveWebCollection mWebCollection;
        private string applicationPath;
        private AveRolesSerializer m_RolesSerializer;
        private AveWebSerializer m_WebSerializer;
        private AveWebUsersSerializer m_WebUsersSerializer;
        private AveGroupsSerializer m_GroupsSerializer;
        private AveRoleAssignmentsSerializer m_RoleAssignmentsSerializer;
        private AveWebSettingSerializer m_WebSettingSerializer;
        private AveNavigationSerializer m_NavigationSerializer;
        private AveFeatureSerializer m_FeatureSerializer;
        private IAveSecurableObjectImpl m_SecurableObjectImpl;
        private AveAppSerializer m_AppSerializer;
        private readonly object privateLock = new object();
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWeb));
        //private AveWeb mParentWeb;    
        public AveWeb(IAveRequest request, AveSite site, AveWebCollection webCollection, IDictionary<string, object> prop)
            : base(request)
        {
            base.DataCache = new AveClientConcurrentObjectData();
            mRequest = request;
            mSite = site;
            //mParentWeb = parentWeb;
            mWebCollection = webCollection;
            base.DataCache.AddPropertyies(prop);
        }

        public int GetWorkingLanguage()
        {
            mLogger.Info($"Get working language for web,EnableUseWorkingLanguage:{WrapperConfiguration.EnableUseWorkingLanguage}");
            try
            {
                if (!WrapperConfiguration.EnableUseWorkingLanguage)
                {
                    return (int)Language;
                }
                if (mSite != null && mSite.UserAccountInfo != null)
                {
                    var account = mSite.UserAccountInfo;
                    if (account != null)
                    {
                        if (account.ConnectionType == BposConnectionType.ServiceAccount)
                        {
                            return mRequest.GetWebWorkingLanguage(Url);
                        }
                        else
                        {
                            //app token 
                            return (int)Language;
                        }
                    }
                    return (int)Language;
                }
                else
                {
                    //use default
                    return (int)Language;
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occurred while getting web working language.Web:{0},Error:{1}", ServerRelativeUrl, e);
            }
            return (int)Language;
        }

        internal override void InitRoleAssignmentProperties(Dictionary<string, object> roleAssignmentProperties)
        {
            roleAssignmentProperties[AveObjectModelConstant.WebServerRelativeUrl] = this.ServerRelativeUrl;
        }

        internal override Dictionary<string, object> AddRoleAssignment(Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.AddRoleAssignment(this.ServerRelativeUrl, null, null, Guid.Empty, -1, roleAssignmentProperties, "web.roleAssignments");
        }

        internal override Dictionary<string, object> UpdateRoleAssignment(int principalId, Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.UpdateRoleAssignment(this.ServerRelativeUrl, null, null, Guid.Empty, -1, principalId, roleAssignmentProperties, "web.roleAssignments");
        }

        #region IAveWeb Members

        internal IAveRequest SPRequest
        {
            get
            {
                return mRequest as IAveRequest;
            }
        }

        public IAveUserResource TitleResource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded(AveWebUserResourceConstants.Title_Resource))
                {
                    var titleResource = new AveWebUserResource(this, AveUserResourceConstants.TITLE_RESOUCE, DataCache);
                    base.DataCache.AddProperty(AveWebUserResourceConstants.Title_Resource, titleResource);
                }
                return base.DataCache.GetProperty<AveWebUserResource>(AveWebUserResourceConstants.Title_Resource);
            }
        }

        public IAveUserResource DescriptionResource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded(AveWebUserResourceConstants.Description_Resource))
                {
                    var titleResource = new AveWebUserResource(this, AveUserResourceConstants.DESCRIPTION_RESOUCE, DataCache);
                    base.DataCache.AddProperty(AveWebUserResourceConstants.Description_Resource, titleResource);
                    return titleResource;
                }
                return base.DataCache.GetProperty<AveWebUserResource>(AveWebUserResourceConstants.Description_Resource);
            }
        }

        public string AlternateCssUrl
        {
            get
            {
                // GetMasterPageProperties(); API已支持 load web 即可
                return base.DataCache.GetProperty<string>("AlternateCssUrl");
            }
            set
            {
                if (!string.Equals(AlternateCssUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("AlternateCssUrl", value);
                }
            }
        }

        private readonly Queue<IAveList> cachingFieldsList = new Queue<IAveList>();

        public Queue<IAveList> CachingFieldsList => cachingFieldsList;

        public IAveAlertCollection Alerts
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Alerts"))
                {
                    Dictionary<string, object> alertsProperties = mRequest.GetAlerts(this.ServerRelativeUrl);
                    AveAlertCollection alertCollection = null;
                    if (alertsProperties != null)
                    {
                        alertCollection = new AveAlertCollection(this, mRequest, alertsProperties);
                    }
                    base.DataCache.AddProperty("Alerts", alertCollection);
                    return alertCollection;
                }
                return base.DataCache.GetProperty<IAveAlertCollection>("Alerts");
            }
        }

        public IAveAlertCollection AlertsV2
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Alerts"))
                {
                    Dictionary<string, object> alertsProperties = mRequest.GetAlertsV2(this.ServerRelativeUrl);
                    AveAlertCollection alertCollection = null;
                    if (alertsProperties != null)
                    {
                        alertCollection = new AveAlertCollection(this, mRequest, alertsProperties);
                    }
                    base.DataCache.AddProperty("Alerts", alertCollection);
                    return alertCollection;
                }
                return base.DataCache.GetProperty<IAveAlertCollection>("Alerts");
            }
        }

        public bool AllowUnsafeUpdates
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowUnsafeUpdates");
            }
            set
            {
                //client api doesn't support this property
                //if (!AllowUnsafeUpdates.Equals(value))
                //{
                //    base.DataCache.AddChangedProperty("AllowUnsafeUpdates", value);
                //}
            }
        }

        public bool AllowAutomaticASPXPageIndexing
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowAutomaticASPXPageIndexing");
            }
            set
            {
                if (!AllowAutomaticASPXPageIndexing.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowAutomaticASPXPageIndexing", value);
                }
            }
        }

        public System.Collections.Hashtable AllProperties
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllProperties") && base.DataCache.IsPropertyAvailable("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> dic = base.DataCache.GetProperty<Dictionary<string, object>>("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix);
                    System.Collections.Hashtable table = new System.Collections.Hashtable(dic);
                    base.DataCache.AddProperty("AllProperties", new AveCustomHashtable(table, SetChangeProperty));
                    //return table; SAAS-1588
                }
                return base.DataCache.GetProperty<AveCustomHashtable>("AllProperties");
            }
        }

        public IAveUserCollection AllUsers
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllUsers"))
                {
                    Dictionary<string, object> userProperties = mRequest.GetUsers(this.ServerRelativeUrl, null, "web.allUsers");
                    AveUserCollection users = new AveUserCollection(mRequest, this, "web.allUsers", this.Groups.Count <= 0 ? "" : this.Groups[0].Name, userProperties);
                    base.DataCache.AddProperty("AllUsers", users);
                    return users;
                }
                return base.DataCache.GetProperty<IAveUserCollection>("AllUsers");
            }
        }

        public AveWebASPXPageIndexMode ASPXPageIndexMode
        {
            get
            {
                return base.DataCache.GetProperty<AveWebASPXPageIndexMode>("ASPXPageIndexMode");
            }
            set
            {
                base.DataCache.AddChangedProperty("ASPXPageIndexMode", (int)value);
                base.DataCache.AddChangedProperty("NoCrawl", this.NoCrawl);
                base.DataCache.AddChangedProperty("ExcludeFromOfflineClient", this.ExcludeFromOfflineClient);
            }
        }

        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author"))
                {
                    string authorLoginName = mRequest.GetAuthor(ServerRelativeUrl);
                    AveUser user = SiteUsers.GetByLoginName(authorLoginName) as AveUser;
                    base.DataCache.AddProperty("Author", user);
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
        }

        public IAveFieldCollection AvailableFields
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AvailableFields"))
                {
                    Dictionary<string, object> availableFieldCollection = this.mRequest.GetFields(this.ServerRelativeUrl, null, null, Guid.Empty, "web.availableFields", null, AveUserResourceExtension.SupportedResourceCultureNames);
                    AveFieldCollection availableFields = new AveFieldCollection(this, null, this.mRequest, "web.availableFields", null, availableFieldCollection);
                    base.DataCache.AddProperty("AvailableFields", availableFields);
                }
                return base.DataCache.GetProperty<IAveFieldCollection>("AvailableFields");
            }
        }

        public IAveContentTypeCollection AvailableContentTypes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AvailableContentTypes"))
                {
                    Dictionary<string, object> availableContentTypes = mRequest.GetContentTypes(this.ServerRelativeUrl, null, Guid.Empty, "web.availableContentTypes", AveUserResourceExtension.SupportedResourceCultureNames);
                    AveContentTypeCollection availableContentTypeCollection = new AveContentTypeCollection(mRequest, this, null, "web.availableContentTypes", availableContentTypes);
                    base.DataCache.AddProperty("AvailableContentTypes", availableContentTypeCollection);
                    return availableContentTypeCollection;
                }
                return base.DataCache.GetProperty<IAveContentTypeCollection>("AvailableContentTypes");
            }
        }

        private readonly object mLoadCTLock = new object();
        public IAveContentTypeCollection ContentTypes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ContentTypes"))
                {
                    lock (mLoadCTLock)
                    {
                        if (base.DataCache.IsPropertyNotLoaded("ContentTypes"))
                        {
                            Dictionary<string, object> contentTypes = mRequest.GetContentTypes(this.ServerRelativeUrl, null, Guid.Empty, "web.contentTypes", AveUserResourceExtension.SupportedResourceCultureNames);
                            AveContentTypeCollection aveContentTypeCollection = new AveContentTypeCollection(mRequest, this, null, "web.contentTypes", contentTypes);
                            base.DataCache.AddProperty("ContentTypes", aveContentTypeCollection);
                            return aveContentTypeCollection;
                        }
                    }
                }
                return base.DataCache.GetProperty<IAveContentTypeCollection>("ContentTypes");
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                if (!string.Equals(Description, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Description", value);
                }
            }
        }

        private readonly object mLoadFeaturesLock = new object();
        public IAveFeatureCollection Features
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Features"))
                {
                    lock (mLoadFeaturesLock)
                    {
                        if (base.DataCache.IsPropertyNotLoaded("Features"))
                        {
                            Dictionary<string, object> featureCollection = mRequest.GetFeatures(this.ServerRelativeUrl, "web.features");
                            AveFeatureCollection features = new AveFeatureCollection(this, mRequest, featureCollection, "web.features");
                            base.DataCache.AddProperty("Features", features);
                            return features;
                        }
                    }
                }
                return base.DataCache.GetProperty<IAveFeatureCollection>("Features");
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Fields"))
                {
                    Dictionary<string, object> fieldCollection = this.mRequest.GetFields(this.ServerRelativeUrl, null, null, Guid.Empty, "web.fields", null, AveUserResourceExtension.SupportedResourceCultureNames);
                    AveFieldCollection fields = new AveFieldCollection(this, null, this.mRequest, "web.fields", null, fieldCollection);
                    base.DataCache.AddProperty("Fields", fields);
                    return fields;
                }
                return base.DataCache.GetProperty<IAveFieldCollection>("Fields");
            }
        }

        public IAveWeb FirstUniqueRoleDefinitionWeb
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("FirstUniqueRoleDefinitionWeb") && base.DataCache.IsPropertyAvailable("FirstUniqueRoleDefinitionWeb" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Guid webId = base.DataCache.GetProperty<Guid>("FirstUniqueRoleDefinitionWeb" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveWeb web = this.mSite.OpenWeb(webId) as AveWeb;
                    return web;
                }
                return base.DataCache.GetProperty<IAveWeb>("FirstUniqueRoleDefinitionWeb");
            }
        }

        public IAveGroupCollection Groups
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Groups"))
                {
                    Dictionary<string, object> groupCollection = this.mRequest.GetGroups(this.ServerRelativeUrl, "web.groups", null);
                    AveGroupCollection groups = new AveGroupCollection(this, this.mRequest, "web.groups", groupCollection);
                    base.DataCache.AddProperty("Groups", groups);
                    return groups;
                }
                return base.DataCache.GetProperty<IAveGroupCollection>("Groups");
            }
        }

        public IAveGroupCollection SiteGroups
        {
            get
            {
                if (this.IsRootWeb)
                {
                    if (base.DataCache.IsPropertyNotLoaded("SiteGroups"))
                    {
                        Dictionary<string, object> siteGroupCollection = this.mRequest.GetGroups(this.ServerRelativeUrl, "web.siteGroups", null);
                        AveGroupCollection siteGroups = new AveGroupCollection(this, this.mRequest, "web.siteGroups", siteGroupCollection);
                        base.DataCache.AddProperty("SiteGroups", siteGroups);
                        return siteGroups;
                    }
                    return base.DataCache.GetProperty<IAveGroupCollection>("SiteGroups");
                }
                else
                {
                    return mSite.RootWeb.SiteGroups;
                }
            }
        }


        public IAveGroupCollection SiteGroupsWithUsers
        {
            get
            {
                if (this.IsRootWeb)
                {
                    if (base.DataCache.IsPropertyNotLoaded("SiteGroupsWithGroupMember"))
                    {
                        Dictionary<string, object> siteGroupCollection = this.mRequest.GetSiteGroupsWithUsers(this.ServerRelativeUrl);
                        AveGroupCollection siteGroups = new AveGroupCollection(this, this.mRequest, "web.siteGroups", siteGroupCollection);
                        base.DataCache.AddProperty("SiteGroups", siteGroups);
                        return siteGroups;
                    }
                    return base.DataCache.GetProperty<IAveGroupCollection>("SiteGroups");
                }
                else
                {
                    return mSite.RootWeb.SiteGroups;
                }
            }
        }

        public bool IsRootWeb
        {
            get
            {
                return base.DataCache.GetProperty<Boolean>("IsRootWeb");
            }
        }

        public uint Language
        {
            get
            {
                return base.DataCache.GetProperty<uint>("Language");
            }
        }

        public IAveListCollection Lists
        {
            get
            {
                lock (privateLock)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Lists"))
                    {
                        Dictionary<string, object> listCollectionProperties = mRequest.GetLists(this.ServerRelativeUrl, AveUserResourceExtension.SupportedResourceCultureNames);
                        AveListCollection Lists = new AveListCollection(mRequest, this, listCollectionProperties);
                        base.DataCache.AddProperty("Lists", Lists);
                    }
                    return base.DataCache.GetProperty<AveListCollection>("Lists");
                }
            }
        }

        /// <summary>
        /// 添加BroswerLists属性，GetLists更少的属性来提高效率；
        /// </summary>
        public IAveListCollection BrowserLists
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("BrowserLists"))
                {
                    Dictionary<string, object> listCollectionProperties = mRequest.GetListsLightly(this.ID);
                    AveListCollection Lists = new AveListCollection(mRequest, this, listCollectionProperties);
                    base.DataCache.AddProperty("BrowserLists", Lists);
                }
                return base.DataCache.GetProperty<AveListCollection>("BrowserLists");
            }
        }

        public System.Globalization.CultureInfo Locale
        {
            get
            {
                return base.DataCache.GetProperty<System.Globalization.CultureInfo>("Locale");
            }
        }

        public string MasterUrl
        {
            get
            {
                GetMasterPageProperties();
                return base.DataCache.GetProperty<string>("MasterUrl");
            }
            set
            {
                if (!string.Equals(MasterUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("MasterUrl", value);
                }
            }
        }

        public string CustomMasterUrl
        {
            get
            {
                GetMasterPageProperties();
                return base.DataCache.GetProperty<string>("CustomMasterUrl");
            }
            set
            {
                if (!string.Equals(CustomMasterUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("CustomMasterUrl", value);
                }
            }
        }


        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                if (!string.Equals(Name, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Name", value);
                    base.DataCache.AddChangedProperty("Title", value);
                }
            }
        }

        internal void RemoveNavigation()
        {
            if (!base.DataCache.IsPropertyNotLoaded("Navigation"))
            {
                base.DataCache.RemoveProperty("Navigation");
            }
        }

        public IAveNavigation Navigation
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Navigation"))
                {
                    bool isPublishFeatureEnable = this.Features[new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb")] != null;
                    Dictionary<string, object> navigation = mRequest.GetNavigation(this.ServerRelativeUrl, isPublishFeatureEnable);
                    AveNavigation navg = new AveNavigation(this, mRequest, navigation);
                    base.DataCache.AddProperty("Navigation", navg);
                }
                return base.DataCache.GetProperty<IAveNavigation>("Navigation");
            }
        }

        public bool NoCrawl
        {
            get
            {
                return base.DataCache.GetProperty<bool>("NoCrawl");
            }
            set
            {
                base.DataCache.AddChangedProperty("NoCrawl", value);
                base.DataCache.AddChangedProperty("ASPXPageIndexMode", this.ASPXPageIndexMode);
                base.DataCache.AddChangedProperty("ExcludeFromOfflineClient", this.ExcludeFromOfflineClient);
            }
        }

        public IAveUserCollection SiteUsers
        {
            get
            {
                lock (privateLock)
                {
                    if (this.IsRootWeb)
                    {
                        if (base.DataCache.IsPropertyNotLoaded("SiteUsers"))
                        {
                            Dictionary<string, object> userProperties = mRequest.GetUsers(this.ServerRelativeUrl, null, "web.siteUsers");
                            AveUserCollection users = new AveUserCollection(mRequest, this, "web.siteUsers", string.Empty, userProperties);
                            base.DataCache.AddProperty("SiteUsers", users);
                            return users;
                        }
                        return base.DataCache.GetProperty<IAveUserCollection>("SiteUsers");
                    }
                    else
                    {
                        return mSite.RootWeb.SiteUsers;
                    }
                }
            }
        }

        public bool QuickLaunchEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("QuickLaunchEnabled");
            }
            set
            {
                if (!QuickLaunchEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("QuickLaunchEnabled", value);
                }
            }
        }

        public IAveRegionalSettings RegionalSettings
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RegionalSettings") && !base.DataCache.IsPropertyAvailable("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> dic = base.DataCache.GetProperty<Dictionary<string, object>>("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    Dictionary<string, object> regionalSettingsProperties = mRequest.GetWebRegionalSetting(this.ServerRelativeUrl);
                    AveRegionalSettings regionalSettings = new AveRegionalSettings(this, mRequest, regionalSettingsProperties);
                    base.DataCache.AddProperty("RegionalSettings", regionalSettings);
                    return regionalSettings;
                }
                else if (base.DataCache.IsPropertyNotLoaded("RegionalSettings") && base.DataCache.IsPropertyAvailable("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> regionalSettingsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveRegionalSettings regionalSettings = new AveRegionalSettings(this, mRequest, regionalSettingsProperties);
                    base.DataCache.AddProperty("RegionalSettings", regionalSettings);
                    return regionalSettings;
                }
                return base.DataCache.GetProperty<IAveRegionalSettings>("RegionalSettings");
            }
        }

        public IAveRoleDefinitionCollection RoleDefinitions
        {
            get
            {
                //if (this.IsRootWeb) //SAAS-26366 需要获取web对应语言的definition
                //{
                if (base.DataCache.IsPropertyNotLoaded("RoleDefinitions"))
                {
                    Dictionary<string, object> roleDefinitionColProperties = mRequest.GetRoleDefinitions(this.ServerRelativeUrl);
                    AveRoleDefinitionCollection roleDefinitonCollection = new AveRoleDefinitionCollection(this, mRequest, roleDefinitionColProperties);
                    base.DataCache.AddProperty("RoleDefinitions", roleDefinitonCollection);
                }
                return base.DataCache.GetProperty<IAveRoleDefinitionCollection>("RoleDefinitions");
                //}
                //else
                //{
                //    return mSite.RootWeb.RoleDefinitions;
                //}
            }
        }

        public IAveFolder RootFolder
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RootFolder"))
                {
                    Dictionary<string, object> folderProp = mRequest.GetFolder(this.ServerRelativeUrl, null, this.ServerRelativeUrl);
                    AveFolder rootFolder = new AveFolder(mRequest, this, null, null, folderProp);
                    base.DataCache.AddProperty("RootFolder", rootFolder);
                }
                return base.DataCache.GetProperty<IAveFolder>("RootFolder");
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public IAveSite Site
        {
            get
            {
                return mSite;
            }
        }

        public bool SyndicationEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SyndicationEnabled");
            }
            set
            {
                if (!SyndicationEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("SyndicationEnabled", value);
                }
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");

            }
            set
            {
                if (!string.Equals(Title, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Title", value);
                }
            }
        }

        public bool TreeViewEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("TreeViewEnabled");
            }
            set
            {
                if (!TreeViewEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("TreeViewEnabled", value);
                }
            }
        }

        public IAveWebCollection Webs
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Webs"))
                {
                    Dictionary<string, object> webCollection = mRequest.GetSubWebs(this.ServerRelativeUrl);
                    AveWebCollection webs = new AveWebCollection(mRequest, mSite, this, webCollection);
                    base.DataCache.AddProperty("Webs", webs);
                }
                return base.DataCache.GetProperty<IAveWebCollection>("Webs");
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
        }

        public string WebTemplate
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("WebTemplate"))
                {
                    LoadWebTemplateConfiguration();
                }
                return base.DataCache.GetProperty<string>("WebTemplate");
            }
        }

        private void LoadWebTemplateConfiguration()
        {
            string configuration = mRequest.GetWebTemplateConfiguration(this.ServerRelativeUrl);
            string[] datas = configuration.Split('#');
            if (datas.Length == 2)
            {
                base.DataCache.AddProperty("WebTemplate", datas[0]);
                base.DataCache.AddProperty("Configuration", short.Parse(datas[1]));
            }
        }
        
        public string GetTenantAppCatalogSite()
        {
            return mRequest.GetTenantAppCatalogSite(this.ServerRelativeUrl);
        }

        public short Configuration
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Configuration"))
                {
                    LoadWebTemplateConfiguration();
                }
                return base.DataCache.GetProperty<short>("Configuration");
            }
        }

        public int WebTemplateId
        {
            get
            {
                return base.DataCache.GetProperty<int>("WebTemplateId");
            }
        }

        private readonly object locker = new object();
        public IAveWeb ParentWeb
        {
            get
            {
                lock (locker)
                {
                    if (base.DataCache.IsPropertyNotLoaded("ParentWeb"))
                    {
                        if (this.IsRootWeb)
                        {
                            base.DataCache.AddProperty("ParentWeb", null);
                        }
                        else
                        {
                            string parentWebServerRelativeUrl = base.DataCache.GetProperty<string>("ParentWeb" + AveObjectModelConstant.ObjectPropertySuffix);
                            if (mSite != null && mSite.RootWeb.ServerRelativeUrl.Equals(parentWebServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                return mSite.RootWeb;
                            }
                            else if (!string.IsNullOrEmpty(parentWebServerRelativeUrl))
                            {
                                Dictionary<string, object> parentWebProperties = this.mRequest.GetWeb(parentWebServerRelativeUrl);
                                AveWeb parentWeb = new AveWeb(mRequest, this.mSite, null, parentWebProperties);
                                if (!parentWeb.Exists)
                                {
                                    Guid parentWebId = base.DataCache.GetProperty<Guid>("ParentWebId" + AveObjectModelConstant.ObjectPropertySuffix);
                                    parentWebProperties = this.mRequest.GetWeb(parentWebId);
                                    parentWeb = new AveWeb(mRequest, this.mSite, null, parentWebProperties);
                                }
                                base.DataCache.AddProperty("ParentWeb", parentWeb);
                                return parentWeb;
                            }
                            return null;
                        }
                    }
                }
                return base.DataCache.GetProperty<IAveWeb>("ParentWeb");
            }
        }

        public bool ParserEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ParserEnabled");
            }
            set
            {
                if (!ParserEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ParserEnabled", value);
                }
            }
        }

        public bool PresenceEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("PresenceEnabled");
            }
            set
            {
                if (!PresenceEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("PresenceEnabled", value);
                }
            }
        }

        public bool HasUniqueRoleDefinitions
        {
            get
            {
                return base.DataCache.GetProperty<bool>("HasUniqueRoleDefinitions");
            }
        }

        public string Theme
        {
            get
            {
                return base.DataCache.GetProperty<string>("Theme");
            }
        }

        public bool UIVersionConfigurationEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UIVersionConfigurationEnabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("UIVersionConfigurationEnabled", value);
            }
        }

        public string SiteLogoUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("SiteLogoUrl");
            }
            set
            {
                if (!string.Equals(SiteLogoUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("SiteLogoUrl", value);
                }
            }
        }

        public string SiteLogoDescription
        {
            get
            {
                return base.DataCache.GetProperty<string>("SiteLogoDescription");
            }
            set
            {
                if (!string.Equals(SiteLogoDescription, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("SiteLogoDescription", value);
                }
            }
        }

        public int UIVersion
        {
            get
            {
                return base.DataCache.GetProperty<int>("UIVersion");
            }
            set
            {
                if (!UIVersion.Equals(value))
                {
                    base.DataCache.AddChangedProperty("UIVersion", value);
                }
            }
        }

        public IAveUserCollection Users
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Users"))
                {
                    Dictionary<string, object> userProperties = mRequest.GetUsers(this.ServerRelativeUrl, null, "web.users");
                    string groupName = string.Empty;
                    if (this.Groups.Count != 0)
                    {
                        groupName = this.Groups[0].Name;
                    }
                    AveUserCollection users = new AveUserCollection(mRequest, this, "web.users", groupName, userProperties);
                    base.DataCache.AddProperty("Users", users);
                    return users;
                }
                return base.DataCache.GetProperty<IAveUserCollection>("Users");
            }
        }

        public bool IsMultilingual
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsMultilingual");
            }
            set
            {
                if (!IsMultilingual.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IsMultilingual", value);
                }
            }
        }

        public bool OverwriteTranslationsOnChange
        {
            get
            {
                return base.DataCache.GetProperty<bool>("OverwriteTranslationsOnChange");
            }
            set
            {
                if (!OverwriteTranslationsOnChange.Equals(value))
                {
                    base.DataCache.AddChangedProperty("OverwriteTranslationsOnChange", value);
                }
            }
        }

        public IAveUserCollection SiteAdministrators
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SiteAdministrators"))
                {
                    var siteAdministratorList = new List<IDictionary<string, object>>();
                    IDictionary<string, object> siteAdministratorCollectionProperties = new Dictionary<string, object>();
                    foreach (AveUser user in this.SiteUsers)
                    {
                        if (user.IsSiteAdmin)
                        {
                            siteAdministratorList.Add(user.DataCache.GetPropertyCache());
                        }
                    }
                    siteAdministratorCollectionProperties.AddChildren(siteAdministratorList);
                    mSiteAdministrators = new AveUserCollection(this.mRequest, this, "web.siteAdministrators", this.Groups.Count > 0 ? this.Groups[0].Name : string.Empty, siteAdministratorCollectionProperties);
                    base.DataCache.AddProperty("SiteAdministrators", mSiteAdministrators);
                }
                return base.DataCache.GetProperty<IAveUserCollection>("SiteAdministrators");
            }
        }

        public string ThemedCssFolderUrl
        {
            get
            {
                GetThemedProperties();
                return base.DataCache.GetProperty<string>("ThemedCssFolderUrl");
            }
            set
            {
                if (!string.Equals(ThemedCssFolderUrl, value))
                {
                    base.DataCache.AddChangedProperty("ThemedCssFolderUrl", value);
                }
            }
        }

        public string ThemedTemplate
        {
            get
            {
                GetThemedProperties();
                return base.DataCache.GetProperty<string>("ThemedTemplate");
            }
        }
        public bool InheritsThemedCssFolderUrl
        {
            get
            {
                if (this.AllProperties.Contains("__InheritsThemedCssFolderUrl"))
                {
                    object obj = this.AllProperties["__InheritsThemedCssFolderUrl"];
                    return bool.Parse(obj.ToString());
                }
                return false;
            }
        }

        #region Modern Look and Feel
        public AveSPVariantThemeType HeaderEmphasis
        {
            get
            {
                return base.DataCache.GetProperty<AveSPVariantThemeType>("HeaderEmphasis");
            }
            set
            {
                if (!Enum.Equals(HeaderEmphasis, value))
                {
                    base.DataCache.AddChangedProperty("HeaderEmphasis", (int)value);
                }
            }
        }
        public AveHeaderLayoutType HeaderLayout
        {
            get
            {
                return base.DataCache.GetProperty<AveHeaderLayoutType>("HeaderLayout");
            }
            set
            {
                if (!Enum.Equals(HeaderLayout, value))
                {
                    base.DataCache.AddChangedProperty("HeaderLayout", (int)value);
                }
            }
        }
        public bool MegaMenuEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MegaMenuEnabled");
            }
            set
            {
                if (!MegaMenuEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MegaMenuEnabled", value);
                }
            }
        }
        public bool FooterEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("FooterEnabled");
            }
            set
            {
                if (!FooterEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("FooterEnabled", value);
                }
            }
        }

        #endregion
        public AveWebAnonymousState AnonymousState
        {
            get
            {
                return base.DataCache.GetProperty<AveWebAnonymousState>("AnonymousState");
            }
            set
            {
                base.DataCache.AddChangedProperty("AnonymousState", (int)value);
            }
        }

        public bool ExcludeFromOfflineClient
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ExcludeFromOfflineClient");
            }
            set
            {
                base.DataCache.AddChangedProperty("ExcludeFromOfflineClient", value);
                base.DataCache.AddChangedProperty("NoCrawl", this.NoCrawl);
                base.DataCache.AddChangedProperty("ASPXPageIndexMode", this.ASPXPageIndexMode);
            }
        }

        public IAveList SiteUserInfoList
        {
            get
            {
                if (mSiteUserInfoList == null)
                {
                    string siteUserInfoListRelativeUrl = this.mSite.RootWeb.ServerRelativeUrl.TrimEnd('/') + "/_catalogs/users";
                    mSiteUserInfoList = this.mSite.RootWeb.GetList(siteUserInfoListRelativeUrl) as AveList;
                }
                return mSiteUserInfoList;
            }
        }

        public IAveListTemplateCollection ListTemplates
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ListTemplates"))
                {
                    Dictionary<string, object> listTemplateCollection = this.mRequest.GetListTemplates(this.ServerRelativeUrl);
                    AveListTemplateCollection listTemplates = new AveListTemplateCollection(this.mRequest, this, listTemplateCollection);
                    base.DataCache.AddProperty("ListTemplates", listTemplates);
                }
                return base.DataCache.GetProperty<IAveListTemplateCollection>("ListTemplates");
            }
        }

        public IAveUser CurrentUser
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("CurrentUser"))
                {
                    string loginName = base.DataCache.GetProperty<string>("CurrentUser" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser currentUser = this.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.AddProperty("CurrentUser", currentUser);
                    return currentUser;
                }
                return base.DataCache.GetProperty<IAveUser>("CurrentUser");
            }
        }

        public IAvePropertyBag Properties
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Properties") && base.DataCache.IsPropertyAvailable("Properties" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    AvePropertyBag propertyBag = new AvePropertyBag(this, this.mRequest, base.DataCache.GetProperty<Dictionary<string, object>>("Properties" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.AddProperty("Properties", propertyBag);
                }
                IAvePropertyBag prop = base.DataCache.GetProperty<IAvePropertyBag>("Properties");
                if (prop == null)
                {
                    //由于client api中没有properties属性，而properties属性和allproperties一致，所以用allproperties赋值
                    prop = new AvePropertyBag(this, this.mRequest, base.DataCache.GetProperty<Dictionary<string, object>>("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.AddProperty("Properties", prop);

                }
                return prop;
            }
        }

        public bool Exists
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Exists");
            }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EventReceivers"))
                {
                    Dictionary<string, object> eventReceiversProperties = mRequest.GetEventReceiverDefinitions(this.ServerRelativeUrl, null, Guid.Empty, null, "web.eventReceivers");
                    AveEventReceiverDefinitionCollection eventReceiverDefinitionCol = null;
                    if (eventReceiversProperties != null)
                    {
                        eventReceiverDefinitionCol = new AveEventReceiverDefinitionCollection(this, null, mRequest, "web.eventReceivers", eventReceiversProperties);
                    }
                    base.DataCache.AddProperty("EventReceivers", eventReceiverDefinitionCol);
                    return eventReceiverDefinitionCol;
                }
                return base.DataCache.GetProperty<IAveEventReceiverDefinitionCollection>("EventReceivers");
            }
        }

        public bool AllowRssFeeds
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowRssFeeds");
            }
        }

        //Identifies the SchemaXml cache container for multi threads or jobs.
        private Guid mCacheHandlerId;
        public Guid CacheHandlerId
        {
            get
            {
                if (mCacheHandlerId == Guid.Empty)
                {
                    mCacheHandlerId = Guid.NewGuid();
                }
                return mCacheHandlerId;
            }
        }

        public IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid)
        {
            return GetAvailableWebTemplates(lcid, true);//SAAS-1048
        }

        public void ApplyTheme(string theme)
        {
            throw new NotImplementedException();
        }

        public void ApplyTheme(string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            mRequest.ApplyTheme(this.ServerRelativeUrl, colorPaletteUrl, fontSchemeUrl, backgroundImageUrl, shareGenerated);
        }

        public void ApplyWebTemplate(string webTemplate, uint lcid)
        {
            mRequest.ApplyWebTemplate(this.ServerRelativeUrl, webTemplate, lcid);
        }

        public void ApplyWebTemplate(IAveWebTemplate template)
        {
            mRequest.ApplyWebTemplate(this.ServerRelativeUrl, template.Name);
        }

        public void Close()
        {
        }

        public IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid, bool doIncludeCrossLanguage)
        {
            Dictionary<string, object> webTemplateColProperties = this.mRequest.GetAvailableWebTemplates(this.ServerRelativeUrl, lcid, doIncludeCrossLanguage);
            AveWebTemplateCollection webTemplateCollection = new AveWebTemplateCollection(this, mRequest, webTemplateColProperties);
            return webTemplateCollection;
        }

        public IAveFolder GetFolder(string serverRelativeUrl)
        {
            if (serverRelativeUrl.Equals(this.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return this.RootFolder;
            }
            Dictionary<string, object> folderProperties = null;

            //if (!serverRelativeUrl.StartsWith(this.ServerRelativeUrl.TrimEnd('/') + '/'))
            //{
            //    serverRelativeUrl = this.ServerRelativeUrl.TrimEnd('/') + "/" + serverRelativeUrl.TrimStart('/');
            //}

            AveList parentList = this.GetList(serverRelativeUrl) as AveList;
            if (parentList != null)
            {
                folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, parentList.Title, serverRelativeUrl);
            }
            else
            {
                folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, null, serverRelativeUrl);
            }
            return new AveFolder(mRequest, this, parentList, null, folderProperties);
        }

        public IAveFolder GetFolderFromCache(int rowId, string serverRelativeUrl)
        {
            if (serverRelativeUrl.Equals(this.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return this.RootFolder;
            }
            Dictionary<string, object> folderProperties = null;
            AveList parentList = this.GetList(serverRelativeUrl) as AveList;
            if (parentList != null)
            {
                folderProperties = mRequest.GetFolderFromCache(this.ServerRelativeUrl, parentList.Title, serverRelativeUrl, parentList.ID, rowId);
            }
            else
            {
                folderProperties = mRequest.GetFolderFromCache(this.ServerRelativeUrl, null, serverRelativeUrl, Guid.Empty, rowId);
            }
            return new AveFolder(mRequest, this, parentList, null, folderProperties);
        }

        //use GetFolder(string serverRelativeUrl) instead, in case BPOS-S
        public IAveFolder GetFolder(Guid uniqueId)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get root/sub folder in web
        /// </summary>
        /// <param name="uniqueId">folder guid, will not be used</param>
        /// <param name="rowId">folder's row id. -1 means root folder</param>
        /// <param name="serverRelativeUrl">folder's relative url</param>
        /// <returns></returns>
        public IAveFolder GetFolder(Guid uniqueId, int rowId, string serverRelativeUrl)
        {
            if (rowId == -1)
            {
                return GetFolder(serverRelativeUrl);
            }
            else
            {
                return GetFolder(rowId, serverRelativeUrl);
            }
        }
        /// <summary>
        /// To get ListItem's parent folder
        /// </summary>
        /// <param name="rowId">ListItem.RowId</param>
        /// <param name="serverRelativeUrl">List.ServerRelativeUrl</param>
        /// <returns></returns>
        private IAveFolder GetFolder(int rowId, string serverRelativeUrl)
        {
            AvePerformanceTimerPool.Start("AvePoint.ObjectModel.Common.AveWeb.GetFolder");
            if (string.IsNullOrEmpty(serverRelativeUrl) || !serverRelativeUrl.StartsWith(this.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) || rowId < default(int))
            {
                AvePerformanceTimerPool.Stop("AvePoint.ObjectModel.Common.AveWeb.GetFolder");
                return null;
            }
            Dictionary<string, object> folderProperties = null;

            //if (!serverRelativeUrl.StartsWith(this.ServerRelativeUrl.TrimEnd('/') + '/'))
            //{
            //    serverRelativeUrl = this.ServerRelativeUrl.TrimEnd('/') + "/" + serverRelativeUrl.TrimStart('/');
            //}

            AveList parentList = this.GetList(serverRelativeUrl) as AveList;
            if (parentList != null)
            {
                IAveListItem item = parentList.GetItemById(rowId);
                string urlTag = serverRelativeUrl.Substring(0, serverRelativeUrl.Length - parentList.RootFolder.Url.Length);
                string folderUrl = '/' + urlTag.Trim('/') + "/" + item.Url.Substring(0, item.Url.LastIndexOf('/')).Trim('/');
                folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, parentList.Title, folderUrl);
            }
            else
            {
                folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, null, serverRelativeUrl);
            }
            AvePerformanceTimerPool.Stop("AvePoint.ObjectModel.Common.AveWeb.GetFolder");
            return new AveFolder(mRequest, this, parentList, null, folderProperties);
        }

        public IAveFile GetFileByFullPath(string fullPath)
        {
            string serverRelativeUrl = AveUrlUtility.GetServerRelativeUrl(fullPath);
            return this.GetFile(serverRelativeUrl);
        }

        public IAveFile GetFile(string serverRelativeUrl)
        {
            Dictionary<string, object> fileProperties = null;

            if (!serverRelativeUrl.StartsWith(this.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
            {
                if (serverRelativeUrl.StartsWith(this.Url, StringComparison.OrdinalIgnoreCase))
                {
                    // if Root SiteCollection, this.ServerRelativeUrl = "/", serverRelativeUrl.IndexOf must start after "https://"
                    serverRelativeUrl = serverRelativeUrl.Substring(serverRelativeUrl.IndexOf(this.ServerRelativeUrl, 8, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    serverRelativeUrl = this.ServerRelativeUrl.TrimEnd('/') + "/" + serverRelativeUrl.TrimStart('/');
                }
            }

            AveList parentList = this.GetList(serverRelativeUrl) as AveList;
            if (parentList != null)
            {
                fileProperties = mRequest.GetFile(this.ServerRelativeUrl, serverRelativeUrl, parentList.Title);
            }
            else
            {
                fileProperties = mRequest.GetFile(this.ServerRelativeUrl, serverRelativeUrl, null);
            }
            return new AveFile(mRequest, this, parentList, null, fileProperties);
        }

        //use GetFile(string serverRelativeUrl) instead, in case BPOS-S
        public IAveFile GetFile(Guid fileId)
        {
            var fileProperties = mRequest.GetFile(this.ServerRelativeUrl, fileId);

            if (fileProperties.ContainsKey("ServerRelativeUrl"))
            {

                var url = fileProperties["ServerRelativeUrl"].ToString();

                if (!url.StartsWith(this.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
                {
                    if (url.StartsWith(this.Url, StringComparison.OrdinalIgnoreCase))
                    {
                        // if Root SiteCollection, this.ServerRelativeUrl = "/", serverRelativeUrl.IndexOf must start after "https://"
                        url = url.Substring(url.IndexOf(this.ServerRelativeUrl, 8, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        url = this.ServerRelativeUrl.TrimEnd('/') + "/" + url.TrimStart('/');
                    }
                }

                AveList parentList = this.GetList(url) as AveList;

                if (parentList != null)
                {
                    fileProperties["ListName"] = parentList.Title;
                }

                return new AveFile(mRequest, this, parentList, null, fileProperties);
            }
            return null;
        }

        public IAveFile GetFile(Guid fileId, string serverRelativeUrl)
        {
            return this.GetFile(serverRelativeUrl);
        }
        /// <summary>
        /// strUrl : server relative Url
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public IAveList GetList(string strUrl)
        {
            if (AveUrlUtility.IsUrlFull(strUrl))
            {
                strUrl = AveUrlUtility.GetServerRelativeUrl(strUrl);
            }
            IAveListCollection lists = this.Lists;
            strUrl = "/" + strUrl.Trim('/');
            if (!strUrl.StartsWith(this.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
            {
                if (strUrl.StartsWith(this.Url))
                {
                    strUrl = strUrl.Substring(strUrl.IndexOf(this.ServerRelativeUrl));
                }
                else
                {
                    strUrl = this.ServerRelativeUrl.TrimEnd('/') + "/" + strUrl.TrimStart('/');
                }
            }
            strUrl = strUrl.TrimEnd('/') + '/';
            foreach (IAveList list in lists)
            {
                if (list.RootFolder.Exists && strUrl.StartsWith(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
                {
                    return list;
                }
            }
            return null;
        }

        public IAveList GetList(Guid listId)
        {
            Dictionary<string, object> listProp = mRequest.GetList(this.ServerRelativeUrl, listId);
            if (listProp.ContainsKey("BaseType"))
            {
                switch ((AveBaseType)listProp["BaseType"])
                {
                    case AveBaseType.DocumentLibrary:
                        return new AveDocumentLibrary(mRequest, this, listProp);
                        break;
                    default:
                        return new AveList(mRequest, this, listProp);
                        break;
                }
            }
            return null;
        }

        public IAveList GetListByTitle(string title)
        {
            try
            {
                Dictionary<string, object> listProp = mRequest.GetListByTitle(this.ID, title);
                return new AveList(mRequest, this, listProp);
            }
            catch (Exception ex)
            {
                mLogger.Error("get list:{0} by title failed.due to {1}.", title, ex);
                return null;
            }
        }

        public string GetDomainGroupLoginName(string groupName)
        {
            string loginName = string.Empty;
            Dictionary<string, object> principalInfos = this.mRequest.SearchPrincipals(this.ServerRelativeUrl, groupName, (int)AvePrincipalType.SecurityGroup, (int)AvePrincipalSource.All, 30);
            object infoList;
            if (principalInfos.Count > 0)
            {
                if (principalInfos.TryGetValue("Principals", out infoList))
                {
                    List<Dictionary<string, object>> tempList = infoList as List<Dictionary<string, object>>;
                    //if (tempList.Count > 0 && tempList[0].TryGetValue("LoginName", out object tempLoginName))
                    //{
                    //    loginName = tempLoginName.ToString();
                    //}
                    if (tempList.Count > 0)
                    {
                        var coincidentList = new List<Dictionary<string, object>>();
                        tempList.ForEach(temp =>
                        {
                            if (temp.TryGetValue("DisplayName", out object tempDisplayName) && groupName.Equals(tempDisplayName.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (temp.ContainsKey("LoginName")) coincidentList.Add(temp);
                            }
                        });
                        if (coincidentList.Count > 0)
                        {
                            loginName = coincidentList[0]["LoginName"].ToString();
                            coincidentList.ForEach(temp =>
                            {
                                var principalType = temp.TryGetValue("PrincipalType", out object tempPrincipalType) ? tempPrincipalType.ToString() : "Non_PrincipalType";
                                var principalId = temp.TryGetValue("PrincipalId", out object tempPrincipalId) ? tempPrincipalId.ToString() : "Non_PrincipalId";
                                mLogger.Info($"Search User Result. DisplayName: [{temp["DisplayName"]}]. LoginName: [{temp["LoginName"]}]. PrincipalType: [{principalType}]. PrincipalId: [{principalId}]. ");
                            });
                        }
                    }
                }
            }
            return loginName;
        }

        public IAveUser EnsureUser(string loginName)
        {
            int index = -1;
            if (!loginName.Contains("|") && loginName.Contains(":"))
            {
                index = loginName.IndexOf(':');
                loginName = loginName.Substring(index + 1);
            }
            AveUser ensureUser = (AveUser)this.SiteUsers.GetByLoginName(loginName);
            if (ensureUser == null)
            {
                Dictionary<string, object> ensureUserProperties = this.mRequest.GetEnsureUser(this.ServerRelativeUrl, loginName);
                ensureUser = new AveUser(this.mRequest, this, "web.ensureUser", ensureUserProperties);
                //(this.SiteUsers as AveUserCollection).ListData.Add(ensureUser);
                this.SiteUsers.AddOrRemoveUserInCache(ensureUser, true);
            }
            return ensureUser;
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                //SAAS-25121，判断是否为group team site
                var isCustomScriptDisabled = this.Site.DenyAddAndCustomizePagesStatus;// string.Equals("GROUP", WebTemplate, StringComparison.OrdinalIgnoreCase);
                var webProperties = mRequest.UpdateWeb(this.ServerRelativeUrl, base.DataCache.ChangedProperties, isCustomScriptDisabled);
                base.DataCache.UpdateProperties(webProperties);
                //由于在UpdateProperties的时候并没有一起更新DataCache里面的AllProperties属性，所以在这里将AllProperties给Remove掉，下一次用的时候会再次Load一下，保持一致。
                DataCache.RemoveProperty("AllProperties");
                DataCache.RemoveProperty("AssociatedMemberGroup");
                DataCache.RemoveProperty("AssociatedOwnerGroup");
                DataCache.RemoveProperty("AssociatedVisitorGroup");
                if (base.DataCache.IsPropertyAvailable("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> regionalSettingsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveRegionalSettings regionalSettings = new AveRegionalSettings(this, mRequest, regionalSettingsProperties);
                    base.DataCache.AddProperty("RegionalSettings", regionalSettings);
                }
            }
        }

        public void Delete()
        {
            this.mRequest.DeleteWeb(this.ServerRelativeUrl);
            if (mWebCollection != null)
            {
                this.mWebCollection.ListData.Remove(this);
            }
            else
            {
                this.mSite.DataCache.RemoveWeakReferenceHandler("OpenWeb" + this.ServerRelativeUrl);
            }
        }

        public IAveList GetCatalog(AveListTemplateType typeCatalog)
        {
            if (base.DataCache.IsPropertyAvailable("Lists"))
            {
                foreach (IAveList list in this.Lists)
                {
                    if (list.BaseTemplate == typeCatalog)
                    {
                        return list;
                    }
                }
            }
            else
            {
                Dictionary<string, object> listProperties = this.mRequest.GetCatalog(this.ServerRelativeUrl, (int)typeCatalog);
                return new AveList(this.mRequest, this, listProperties);
            }
            return null;
        }

        public void UpdateWebSetting(AveWebSettingInfo awei)
        {
            if (awei.AnonymousState.IsAvailable && this.HasUniqueRoleAssignments)
            {
                if ((int)this.AnonymousState != awei.AnonymousState.Value)
                {
                    this.AnonymousState = (AveWebAnonymousState)awei.AnonymousState.Value;
                }
            }
            if (awei.Flags.IsAvailable && awei.Flags != null)
            {
                if (AveWebFlags.IsMustNotIndexAspPageContentWeb(awei.Flags.Value))
                {
                    this.ASPXPageIndexMode = AveWebASPXPageIndexMode.Never;
                }
                else if (AveWebFlags.IsAllowAlwaysAspxIndexWeb(awei.Flags.Value))
                {
                    this.ASPXPageIndexMode = AveWebASPXPageIndexMode.Always;
                }
                else
                {
                    this.ASPXPageIndexMode = AveWebASPXPageIndexMode.Automatic;
                }
            }
            if (awei.AdjustHijriDays.IsAvailable)
            {
                this.RegionalSettings.AdjustHijriDays = awei.AdjustHijriDays.Value.HasValue ? awei.AdjustHijriDays.Value.Value : AveWebsTableColumnValue.AdjustHijriDays;
            }
            if (awei.AltCalendarType.IsAvailable)
            {
                this.RegionalSettings.AlternateCalendarType = awei.AltCalendarType.Value.HasValue ? awei.AltCalendarType.Value.Value : AveWebsTableColumnValue.AlternateCalendarType;
            }
            if (awei.CalendarType.IsAvailable)
            {
                this.RegionalSettings.CalendarType = awei.CalendarType.Value.HasValue ? awei.CalendarType.Value.Value : AveWebsTableColumnValue.CalendarType;
            }
            if (awei.Collation.IsAvailable && awei.Collation != null)
            {
                this.RegionalSettings.Collation = awei.Collation.Value;
            }

            if (awei.WorkDayStartHour.IsAvailable)
            {
                this.RegionalSettings.WorkDayStartHour = awei.WorkDayStartHour.Value.HasValue ? awei.WorkDayStartHour.Value.Value : AveWebsTableColumnValue.WorkDayStartHour;
            }
            if (awei.WorkDayEndHour.IsAvailable)
            {
                this.RegionalSettings.WorkDayEndHour = awei.WorkDayEndHour.Value.HasValue ? awei.WorkDayEndHour.Value.Value : AveWebsTableColumnValue.WorkDayEndHour;
            }
            if (awei.WorkDays.IsAvailable)
            {
                this.RegionalSettings.WorkDays = awei.WorkDays.Value.HasValue ? awei.WorkDays.Value.Value : AveWebsTableColumnValue.WorkDays;
            }

            if (awei.Time24.IsAvailable && awei.Time24.Value.HasValue)
            {
                this.RegionalSettings.Time24 = awei.Time24.Value.Value;
            }
            if (awei.Locale.IsAvailable && awei.Locale != null)
            {
                this.RegionalSettings.LocaleId = (uint)awei.Locale.Value;
            }
            if (awei.TimeZone.IsAvailable && awei.TimeZone != null)
            {
                this.RegionalSettings.TimeZone.ID = (ushort)awei.TimeZone.Value;
            }
            string[] dProp = { "AllowUnsafeUpdate", "ExcludeFromOfflineClient", "OverwriteTranslationsOnChange", "IsMultilingual", "SiteLogoDescription" };
            string[] sProp = { "QuickLaunchEnabled", "Title", "Description", "UIVersion", "UIVersionConfigurationEnable", };
            CopyObjectAve(this, awei, sProp, dProp);
            Update();
        }

        public IAveNavigationSerializer NavigationSerializer
        {
            get
            {
                if (m_NavigationSerializer == null)
                {
                    m_NavigationSerializer = new AveNavigationSerializer(this, mRequest);
                }
                return m_NavigationSerializer;
            }
        }

        public string GetFileAsString(string url)
        {
            throw new NotImplementedException();
        }

        public IAveListItem GetListItem(string url)
        {
            throw new NotImplementedException();
        }

        public IAveListItem GetListItem(string itemFullUrl, Guid listId, Guid docId)
        {
            IAveList list = this.Lists[listId];
            IAveListItem item = list.GetItemByUniqueId(docId);
            return item;
        }

        public IAveListItem GetListItem(string itemFullUrl, Guid listId, int rowid)
        {
            IAveList list = this.Lists[listId];
            IAveListItem item = list.GetItemById(rowid);
            return item;
        }

        public IAveLimitedWebPartManager GetLimitedWebPartManager(string fullOrRelativeUrl, AvePersonalizationScope scope)
        {
            if (fullOrRelativeUrl.StartsWith(this.Url))
            {
                fullOrRelativeUrl = AveUrlUtility.GetServerRelativeUrl(fullOrRelativeUrl).TrimEnd('/');
            }
            if (AveUrlUtility.IsAspx(fullOrRelativeUrl, false))
            {
                Dictionary<string, object> webpartManagerProperties = this.mRequest.GetLimitedWebPartManager(this.ServerRelativeUrl, fullOrRelativeUrl, (int)scope);
                AveLimitedWebPartManager limitedWebPartManager = new AveLimitedWebPartManager(this, fullOrRelativeUrl, mRequest, webpartManagerProperties);
                return limitedWebPartManager;
            }
            else
            {
                return null;
            }
        }

        public IAveWebPartCollection GetWebPartCollection(string fullOrRelativeUrl, AveStorage storage)
        {
            throw new NotImplementedException();
        }

        public List<string> GetFields()
        {
            List<string> fields = new List<string>();
            foreach (IAveField field in this.Fields)
            {
                fields.Add(field.SchemaXml);
            }
            return fields;
        }

        public AveFieldCollectionInfo GetFieldInfoObj()
        {
            List<AveFieldInfo> fieldInfoList = null;
            IAveFieldCollection fields = this.Fields;
            if (fields.Count > 0)
            {
                fieldInfoList = new List<AveFieldInfo>(fields.Count);
            }
            foreach (IAveField field in fields)
            {
                AveFieldInfo fieldInfo = new AveFieldInfo();
                fieldInfo.Name = field.Title;
                fieldInfo.Type = field.Type.ToString();
                fieldInfo.SchemaXml = field.SchemaXml;
                fieldInfo.AddToDefaultView = true;
                fieldInfoList?.Add(fieldInfo);
            }
            AveFieldCollectionInfo fieldCollectionInfo = new AveFieldCollectionInfo();
            fieldCollectionInfo.Fields = fieldInfoList;
            fieldCollectionInfo.AveSchemaXml = fields.SchemaXml;
            return fieldCollectionInfo;
        }

        public IAveCommonRequest Request
        {
            get
            {
                return null;
            }
        }

        public void RevertAllDocumentContentStreams()
        {
            mRequest.RevertAllDocumentContentStreams(this.ServerRelativeUrl);
        }

        public IAveView GetViewFromUrl(string listUrl)
        {
            foreach (IAveList list in this.Lists)
            {
                foreach (IAveView view in list.Views)
                {
                    if (view.Url.Equals(listUrl))
                    {
                        return view;
                    }
                }
            }
            return null;
        }

        public IAveGroup AssociatedMemberGroup
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AssociatedMemberGroup") && base.DataCache.IsPropertyAvailable("AssociatedMemberGroup" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> groupProp = base.DataCache.GetProperty<Dictionary<string, object>>("AssociatedMemberGroup" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (groupProp.ContainsKey("Exists") && Convert.ToBoolean(groupProp["Exists"]))
                    {
                        AveGroup group = new AveGroup(mRequest, this, groupProp);
                        base.DataCache.AddProperty("AssociatedMemberGroup", group);
                    }
                }
                return base.DataCache.GetProperty<IAveGroup>("AssociatedMemberGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociatedMemberGroup", value.ID);
            }
        }

        public IAveGroup AssociatedOwnerGroup
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AssociatedOwnerGroup") && base.DataCache.IsPropertyAvailable("AssociatedOwnerGroup" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> groupProp = base.DataCache.GetProperty<Dictionary<string, object>>("AssociatedOwnerGroup" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (groupProp.ContainsKey("Exists") && Convert.ToBoolean(groupProp["Exists"]))
                    {
                        AveGroup group = new AveGroup(mRequest, this, groupProp);
                        base.DataCache.AddProperty("AssociatedOwnerGroup", group);
                    }
                }
                return base.DataCache.GetProperty<IAveGroup>("AssociatedOwnerGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociatedOwnerGroup", value.ID);
            }
        }

        public DateTime Created
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Created");
            }
            set
            {
                if (!Created.Equals(value))
                {
                    base.DataCache.AddChangedProperty("Created", value);
                }
            }
        }

        public Guid ParentWebId
        {
            get
            {
                if (this.IsRootWeb)
                {
                    return Guid.Empty;
                }
                return this.ParentWeb.ID;
            }
        }

        public string GetServerRelativeUrlFromUrl(string fullOrRelativeUrl, bool includeQueryString, bool canonicalizeUrl)
        {
            throw new NotImplementedException();
        }

        public DateTime LastItemModifiedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastItemModifiedDate");
            }
            set
            {
                if (!LastItemModifiedDate.Equals(value))
                {
                    base.DataCache.AddChangedProperty("LastItemModifiedDate", value);
                }
            }
        }

        public DateTime LastItemUserModifiedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastItemUserModifiedDate");
            }
        }
        #endregion

        #region IAveSecurableObject Members

        protected override IAveRoleAssignmentCollection InternalBreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.BreakRoleInheritance(this.ServerRelativeUrl, null, null, Guid.Empty, -1, copyRoleAssignments, clearSubscopes, "web.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, mSite, this, null, -1, "web.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        protected override IAveRoleAssignmentCollection InternalResetRoleInheritance()
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.ResetRoleInheritance(this.ServerRelativeUrl, null, null, Guid.Empty, -1, "web.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, mSite, this, null, -1, "web.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        public override void RemoveRoleAssignment(int principalId)
        {
            if (this.RoleAssignments.GetByPrincipalId(principalId) != null)
            {
                mRequest.DeleteRoleAssignment(this.ServerRelativeUrl, null, null, Guid.Empty, -1, principalId, "web.roleAssignments");
            }
        }

        public override IAveRoleAssignmentCollection RoleAssignments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RoleAssignments"))
                {
                    if (!this.IsRootWeb && !this.HasUniqueRoleAssignments)
                    {
                        return this.ParentWeb.RoleAssignments;
                    }
                    Dictionary<string, object> roleAssignmentsProperties = mRequest.GetRoleAssignments(this.ServerRelativeUrl, null, null, Guid.Empty, -1, "web.roleAssignments");
                    AveRoleAssignmentCollection roleAssignments = new AveRoleAssignmentCollection(this, mRequest, mSite, this, null, -1, "web.roleAssignments", roleAssignmentsProperties);
                    base.DataCache.AddProperty("RoleAssignments", roleAssignments);
                    return roleAssignments;
                }
                return base.DataCache.GetProperty<IAveRoleAssignmentCollection>("RoleAssignments");
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            AveClientCacheHandler.CleanSchemaXml(this.CacheHandlerId, this.ID.ToString());
            this.CachingFieldsList?.Clear();
            base.DataCache.RemoveProperty("Lists");
            base.DataCache.RemoveProperty("Webs");
        }

        #endregion

        //used for 07 to 10 migration, there is tag field exist in 07 but removed in 10, should remove this kind of field when restoring
        public IAveFieldTypeDefinitionCollection FieldTypeDefinitionCollection
        {
            get
            {
                return null;
            }
        }

        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public void FakeSPContext()
        {

        }

        public IAveViewStyleCollection ViewStyles
        {
            get { return null; }
        }


        public IAveList GetListFromUrl(string pageUrl)
        {
            return GetList(pageUrl);
        }

        public void InvalidateRequest()
        {

        }

        public void InitializeSPRequest()
        {

        }

        public CultureInfo UICulture
        {
            get
            {
                return this.LanguageCulture;
            }
        }

        public CultureInfo LanguageCulture
        {
            get
            {
                CultureInfo languageCulture = null;
                try
                {
                    languageCulture = new CultureInfo((int)this.Language, false);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get web:{0} LanguageCulture failed.Error Message:{1}", ServerRelativeUrl, ex.ToString());
                }
                return languageCulture;
            }
        }

        public Guid TaxonomyListId
        {
            get
            {
                if (Properties.ContainsKey("taxonomyhiddenlist"))
                {
                    var taxonomyHiddenListProperty = Properties["taxonomyhiddenlist"];
                    Guid taxonomyHiddenListId;
                    if (Guid.TryParse(taxonomyHiddenListProperty, out taxonomyHiddenListId)
                        && taxonomyHiddenListId != Guid.Empty)
                    {
                        return taxonomyHiddenListId;
                    }
                }
                return Guid.Empty;
            }
        }

        #region Access Request Setting
        public string RequestAccessEmail
        {
            get
            {
                return base.DataCache.GetProperty<string>("RequestAccessEmail");
            }
            set
            {
                if (!string.Equals(RequestAccessEmail, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("RequestAccessEmail", value);
                }
            }
        }

        public string AccessRequestSiteDescription
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccessRequestSiteDescription");
            }
            set
            {
                if (!string.Equals(value, AccessRequestSiteDescription, StringComparison.InvariantCulture))
                {
                    base.DataCache.AddChangedProperty("AccessRequestSiteDescription", value);
                }
            }
        }
        public bool UseAccessRequestDefault
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UseAccessRequestDefault");
            }
            set
            {
                if (!UseAccessRequestDefault.Equals(value))
                {
                    base.DataCache.AddChangedProperty("UseAccessRequestDefault", value);
                }
            }
        }


        public bool MembersCanShare
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MembersCanShare");
            }
            set
            {
                if (!MembersCanShare.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MembersCanShare", value);
                }
            }
        }

        #endregion Access Request Settings

        public bool RequestAccessEnable
        {
            get
            {
                bool enable = mRequest.GetRequestAccessEnable(this.Url);
                return enable;
            }
            set
            {
                mRequest.SetRequestAccessEnable(this.Url, value);
            }
        }

        public object GetObject(string strUrl)
        {
            throw new NotImplementedException();
        }

        public bool IsPublish
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("IsPublish"))
                {
                    Guid featureId = new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb");
                    IAveFeature feature = this.Features[featureId];
                    if (feature != null)
                    {
                        base.DataCache.AddProperty("IsPublish", true);
                    }
                    else
                    {
                        base.DataCache.AddProperty("IsPublish", false);
                    }
                }
                return base.DataCache.GetProperty<bool>("IsPublish");
            }
        }


        public IAveFile GetFile(string serverRelativeUrl, bool needProperties)
        {
            throw new NotImplementedException();
        }


        public IAveFile GetCheckoutFile(string url)
        {
            return this.GetFile(url);
        }

        public IAveGroup AssociatedVisitorGroup
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AssociatedVisitorGroup") && base.DataCache.IsPropertyAvailable("AssociatedVisitorGroup" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> groupProp = base.DataCache.GetProperty<Dictionary<string, object>>("AssociatedVisitorGroup" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (groupProp.ContainsKey("Exists") && Convert.ToBoolean(groupProp["Exists"]))
                    {
                        AveGroup group = new AveGroup(mRequest, this, groupProp);
                        base.DataCache.AddProperty("AssociatedVisitorGroup", group);
                    }
                }
                return base.DataCache.GetProperty<IAveGroup>("AssociatedVisitorGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociatedVisitorGroup", value.ID);
            }
        }

        public string ApplicationPath
        {
            get { return applicationPath; }
            set { applicationPath = value; }
        }

        public void RestoreTheme(AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            string siteServerRelativeUrl = string.Empty;
            if (!this.IsRootWeb)
            {
                siteServerRelativeUrl = mSite.ServerRelativeUrl;
            }
            //if (webSettingInfo.WebTheme != null)  //if source web is default, WebTheme will be null, so here it should not be skipped
            //{
            //if inherit parent theme ,don't restore own select theme.
            if (!this.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") || !Convert.ToBoolean(this.AllProperties["__InheritsThemedCssFolderUrl"].ToString()))
            {
                mLogger.Info("Request : RestoreTheme");
                mRequest.RestoreTheme(this.ServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, themedCssFolderUrl);
            }
            //}
        }

        public void RestoreMasterPage(AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            string siteServerRelativeUrl = string.Empty;
            if (!this.IsRootWeb)
            {
                siteServerRelativeUrl = mSite.ServerRelativeUrl;
            }
            else
            {
                /*
                 * Root web inherite property should be false.
                 */
                pageInfo.CInheriting = false;
                pageInfo.MInheriting = false;
                pageInfo.Inheriting = false;
            }
            mRequest.RestoreMasterPage(this.ServerRelativeUrl, siteServerRelativeUrl, pageInfo, alternateCssUrl);
        }
        public IAveAudit Audit
        {
            get { throw new NotImplementedException(); }
        }

        public IAveRolesSerializer RolesSerializer
        {
            get
            {
                if (m_RolesSerializer == null)
                {
                    m_RolesSerializer = new AveRolesSerializer(this);
                }
                return m_RolesSerializer;
            }
        }

        public IAveWebSerializer WebSerializer
        {
            get
            {
                if (m_WebSerializer == null)
                {
                    m_WebSerializer = new AveWebSerializer(this);
                }
                return m_WebSerializer;
            }
        }

        public IAveUsersSerializer WebUsersSerializer
        {
            get
            {
                if (m_WebUsersSerializer == null)
                {
                    m_WebUsersSerializer = new AveWebUsersSerializer(this);
                }
                return m_WebUsersSerializer;
            }
        }

        public IAveGroupsSerializer GroupsSerializer
        {
            get
            {
                if (m_GroupsSerializer == null)
                {
                    m_GroupsSerializer = new AveGroupsSerializer(this);
                }
                return m_GroupsSerializer;
            }
        }

        public IAveRoleAssignmentsSerializer RoleAssignmentsSerializer
        {
            get
            {
                if (m_RoleAssignmentsSerializer == null)
                {
                    m_RoleAssignmentsSerializer = new AveRoleAssignmentsSerializer(this);
                }
                return m_RoleAssignmentsSerializer;
            }
        }

        public IAveWebSettingSerializer WebSettingSerializer
        {
            get
            {
                if (m_WebSettingSerializer == null)
                {
                    m_WebSettingSerializer = new AveWebSettingSerializer(this);
                }
                return m_WebSettingSerializer;
            }
        }

        public IAveFeatureSerializer FeatureSerializer
        {
            get
            {
                if (m_FeatureSerializer == null)
                {
                    m_FeatureSerializer = new AveFeatureSerializer(this, mRequest);
                }
                return m_FeatureSerializer;
            }
        }
        /// <summary>
        /// add app serializer for deploy records app.
        /// </summary>
        public IAveAppSerializer AppSerializer
        {
            get
            {
                if (m_AppSerializer == null)
                {
                    m_AppSerializer = new AveAppSerializer(this, (int)AveRestoreMode.Default);
                }
                return m_AppSerializer;
            }
        }
        public IEnumerable<int> SupportedUICultures
        {
            get { return base.DataCache.GetProperty<IEnumerable<int>>("SupportedUILanguageIds"); }
        }

        public void AddSupportedUICulture(List<int> lcids)
        {
            mRequest.AddSupportedUILanguage(this.ServerRelativeUrl, lcids);
        }


        public IAveList GetListByName(string strListName, bool bThrowException)
        {
            IAveList list = null;
            try
            {
                list = this.Lists.GetByTitle(strListName);
            }
            catch (Exception)
            {
                if (bThrowException)
                {
                    throw;
                }
            }
            return list;
        }

        public IAveWorkflowTemplateCollection WorkflowTemplates
        {
            get
            {
                //throw new NotImplementedException();
                //if (base.DataCache.IsPropertyNotLoaded("WorkflowTemplates"))
                {
                    Dictionary<string, object> workflowTemplates = mRequest.GetWorkflowTemplates(this.ServerRelativeUrl, this.Name, this.ID, "web.workflowTemplates", null);
                    AveWorkflowTemplateCollection aveWorkflowTemplateCollection = new AveWorkflowTemplateCollection(mRequest, this, null, "web.workflowTemplates", workflowTemplates);
                    base.DataCache.AddProperty("WorkflowTemplates", aveWorkflowTemplateCollection);
                    return aveWorkflowTemplateCollection;
                }
                //return base.DataCache.GetProperty<IAveWorkflowTemplateCollection>("WorkflowTemplates");
            }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("UserCustomActionCollection"))
                {
                    Dictionary<string, object> userCustomActions = mRequest.UserCustomActionCollection_Load(AveUserCustomActionScope.Web, ServerRelativeUrl, Guid.Empty);
                    AveUserCustomActionCollection aveUserCustomActions = new AveUserCustomActionCollection(mSite, this, mRequest, userCustomActions);
                    base.DataCache.AddProperty("UserCustomActionCollection", aveUserCustomActions);
                    return aveUserCustomActions;
                }
                return base.DataCache.GetProperty<IAveUserCustomActionCollection>("UserCustomActionCollection");

            }
        }


        public long Size
        {
            get { throw new NotImplementedException(); }
        }

        public IAveDocTemplateCollection DocTemplates
        {
            get { throw new NotImplementedException(); }
        }

        public IAveListCollection GetListsOfType(AveBaseType baseType)
        {
            throw new NotImplementedException();
        }

        public void CreateDefaultAssociatedGroups(string userLogin, string userLogin2, string groupNameSeed)
        {
            if (!HasUniqueRoleAssignments)
            {
                throw new ArgumentException();
            }
            if (string.IsNullOrEmpty(groupNameSeed))
            {
                groupNameSeed = Title;
            }
            var visitorGroupName = AveSPResource.GetString((int)Language, "DefaultVisitorGroupName", groupNameSeed);
            var memberGroupName = AveSPResource.GetString((int)Language, "DefaultMemberGroupName", groupNameSeed);
            var ownerGroupName = AveSPResource.GetString((int)Language, "DefaultOwnerGroupName", groupNameSeed);
            var visitorGroupDescription = AveSPResource.GetString((int)Language, "DefaultVisitorGroupDescription", Title);
            var memberGroupDescription = AveSPResource.GetString((int)Language, "DefaultMemberGroupDescription", Title);
            var ownerGroupDescription = AveSPResource.GetString((int)Language, "DefaultOwnerGroupDescription", Title);
            var encodedTitle = AveHttpUtility.HtmlEncode(Title);
            var encodedUrl = AveHttpUtility.HtmlUrlAttributeEncode(AveHttpUtility.UrlPathEncode(ServerRelativeUrl, false));
            var visitorGroupDescriptionRichText = AveSPResource.GetString((int)Language, "DefaultVisitorGroupDescriptionRichText", encodedTitle, encodedUrl);
            var memberGroupDescriptionRichText = AveSPResource.GetString((int)Language, "DefaultMemberGroupDescriptionRichText", encodedTitle, encodedUrl);
            var ownerGroupDescriptionRichText = AveSPResource.GetString((int)Language, "DefaultOwnerGroupDescriptionRichText", encodedTitle, encodedUrl);
            visitorGroupName = AveSPUtility.NormalizeSharePointGroupName(visitorGroupName, groupNameSeed);
            memberGroupName = AveSPUtility.NormalizeSharePointGroupName(memberGroupName, groupNameSeed);
            ownerGroupName = AveSPUtility.NormalizeSharePointGroupName(ownerGroupName, groupNameSeed);

            var createdVisitorGroup = false;
            var createdMemberGroup = false;
            var createdOwnerGorup = false;
            var createdassociategroups = AllProperties["vti_createdassociategroups"] as string;
            var stringBuilder = new StringBuilder(createdassociategroups ?? "");
            var user = string.IsNullOrEmpty(userLogin) ? CurrentUser : SiteUsers[userLogin];
            var ownerGroup = AssociatedOwnerGroup;
            if (ownerGroup == null)
            {
                string uniqueGroupName = GetUniqueGroupName(ownerGroupName);
                SiteGroups.Add(uniqueGroupName, user, user, ownerGroupDescription);
                ownerGroup = SiteGroups[uniqueGroupName];
                if (Site.CompatibilityLevel >= 15)
                {
                    ownerGroup.OnlyAllowMembersViewMembership = false;
                }
                ownerGroup.Owner = ownerGroup;
                ownerGroup.RequestToJoinLeaveEmailSetting = user.Email;
                ownerGroup.Update();
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(';');
                }
                stringBuilder.Append(ownerGroup.ID.ToString(CultureInfo.InvariantCulture));
                createdOwnerGorup = true;
            }
            else
            {
                AssociatedOwnerGroup.AddUser(user);
            }

            var visitorGroup = AssociatedVisitorGroup;
            if (visitorGroup == null)
            {
                string uniqueGroupName2 = GetUniqueGroupName(visitorGroupName);
                SiteGroups.Add(uniqueGroupName2, ownerGroup, null, visitorGroupDescription);
                visitorGroup = SiteGroups[uniqueGroupName2];
                visitorGroup.RequestToJoinLeaveEmailSetting = user.Email;
                if (Site.CompatibilityLevel >= 15)
                {
                    visitorGroup.OnlyAllowMembersViewMembership = false;
                }
                visitorGroup.Update();
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(';');
                }
                stringBuilder.Append(visitorGroup.ID.ToString(CultureInfo.InvariantCulture));
                createdVisitorGroup = true;
            }

            var memberGroup = AssociatedMemberGroup;
            if (memberGroup == null)
            {
                string uniqueGroupName3 = GetUniqueGroupName(memberGroupName);
                SiteGroups.Add(uniqueGroupName3, ownerGroup, null, memberGroupDescription);
                memberGroup = SiteGroups[uniqueGroupName3];
                memberGroup.OnlyAllowMembersViewMembership = false;
                memberGroup.RequestToJoinLeaveEmailSetting = user.Email;
                memberGroup.Update();
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(';');
                }
                stringBuilder.Append(memberGroup.ID.ToString(CultureInfo.InvariantCulture));
                createdMemberGroup = true;
            }

            if (!string.IsNullOrEmpty(userLogin2))
            {
                var user2 = SiteUsers[userLogin2];
                ownerGroup.AddUser(user2);
            }
            var roleType = AveSPUtility.ShouldUseEditRole(this) ? AveRoleType.Editor : AveRoleType.Contributor;
            try
            {
                if (createdOwnerGorup)
                {
                    GrantPermissionToSharePointGroup(ownerGroup, AveRoleType.Administrator);
                }
                if (createdVisitorGroup)
                {
                    GrantPermissionToSharePointGroup(visitorGroup, AveRoleType.Reader);
                }
                if (createdMemberGroup)
                {
                    GrantPermissionToSharePointGroup(memberGroup, roleType);
                }
                if (createdOwnerGorup)
                {
                    RoleAssignments.RemoveById(user.ID);
                }
            }
            catch (ArgumentException ex)
            {
                mLogger.Warn("grant permission failed:{0}", ex);
            }
            IAveFieldMultiLineText sPFieldMultiLineText = null;
            try
            {
                var sPField = SiteUserInfoList.Fields[AveBuiltInFieldId.Notes];
                sPFieldMultiLineText = (sPField as IAveFieldMultiLineText);
            }
            catch (ArgumentException ex)
            {
                mLogger.Warn("cannot get notes field:{0}", ex);
            }
            if (sPFieldMultiLineText != null)
            {
                var array = new int[]
                {
                    ownerGroup.ID,
                    memberGroup.ID,
                    visitorGroup.ID
                };
                var array2 = new bool[]
                {
                    createdOwnerGorup,
                    createdMemberGroup,
                    createdVisitorGroup
                };
                string[] array3;
                if (sPFieldMultiLineText.RichText)
                {
                    array3 = new string[]
                    {
                        ownerGroupDescriptionRichText,
                        memberGroupDescriptionRichText,
                        visitorGroupDescriptionRichText
                    };
                }
                else
                {
                    array3 = new string[]
                    {
                        ownerGroupDescription,
                        memberGroupDescription,
                        visitorGroupDescription
                    };
                }
                for (var i = 0; i < array.Length; i++)
                {
                    if (array2[i])
                    {
                        var itemByIdSelectedFields = SiteUserInfoList.GetItemByIdSelectedFields(array[i], new string[0]);
                        itemByIdSelectedFields[sPFieldMultiLineText.InternalName] = array3[i];
                        itemByIdSelectedFields.Update();
                    }
                }
            }
            AssociatedMemberGroup = memberGroup;
            AssociatedVisitorGroup = visitorGroup;
            AssociatedOwnerGroup = ownerGroup;
            AllProperties["vti_createdassociategroups"] = stringBuilder.ToString();
            Update();
        }

        private void GrantPermissionToSharePointGroup(IAveGroup group, AveRoleType roleType)
        {
            var sPRoleDefinitionBindingCollection = new AveRoleDefinitionBindingCollection();
            sPRoleDefinitionBindingCollection.Add(RoleDefinitions.GetByType(roleType));
            var sPRoleAssignment = new AveRoleAssignment(group as AveGroup);
            sPRoleAssignment.ImportRoleDefinitionBindings(sPRoleDefinitionBindingCollection);
            RoleAssignments.Add(sPRoleAssignment);
        }

        private string GetUniqueGroupName(string groupName)
        {
            var num = -1;
            string text;
            do
            {
                if (++num == 0)
                {
                    text = groupName;
                }
                else
                {
                    text = groupName + num.ToString(CultureInfo.CurrentCulture);
                }
            }
            while (SiteGroups[text] != null);
            return text;
        }

        public IAveFileCollection Files
        {
            get { return this.RootFolder.Files; }
        }

        public void ReloadWeb()
        {
            base.DataCache.RemoveProperty("Lists");
        }

        private void SetChangeProperty(object key, object value)
        {
            if (key == null)
            {
                return;
            }
            if (!this.DataCache.ChangedProperties.ContainsKey("AllPropertiesDictionary"))
            {
                this.DataCache.ChangedProperties["AllPropertiesDictionary"] = new Dictionary<string, object>();
            }
            Dictionary<string, object> folderChangedProperties = this.DataCache.ChangedProperties["AllPropertiesDictionary"] as Dictionary<string, object>;
            folderChangedProperties[key.ToString()] = value;
        }

        private void GetThemedProperties()
        {
            if (!base.DataCache.IsPropertyAvailable("ThemedCssFolderUrl"))
            {
                Dictionary<string, object> webThemeProp = mRequest.GetThemeUrlForWeb(this.ServerRelativeUrl);
                base.DataCache.AddPropertyies(webThemeProp);
            }
        }
        public Dictionary<string, object> GetThmxThemeInfo()
        {
            Dictionary<string, object> thmxThemeInfo = mRequest.GetThmxThemeInfo(this.ServerRelativeUrl);
            return thmxThemeInfo;
        }
        private void GetMasterPageProperties()
        {
            if (!base.DataCache.IsPropertyAvailable("AlternateCssUrl"))
            {
                //当BPOS_S时，如果不开publishing feature的时候，用web service取master page的一些属性会取不到。
                //BPOS_D做过处理，走不到这里。
                if (this.Features[new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb")] != null)
                {
                    Dictionary<string, object> masterPageProp = mRequest.GetMasterPageProperties(this.ServerRelativeUrl);
                    if (masterPageProp != null) //SAAS-11697，在此之前已经通过13的API直接获取到MasterUrl（07和10没有直接获取的API），不需要再去获取
                    {
                        foreach (KeyValuePair<string, object> kv in masterPageProp)
                        {
                            if (!DataCache.IsPropertyAvailable(kv.Key))
                            {
                                DataCache.AddProperty(kv.Key, kv.Value);
                            }
                        }
                    }
                }
            }
        }

        public IAveFeatureDefinitionCollection GetAllFeatureDefinitions()
        {
            Dictionary<string, object> definitionCollection = new Dictionary<string, object>();
            definitionCollection = mRequest.GetAllFeatureDefinitions(this.Url, (int)this.RegionalSettings.LocaleId, "web.features");
            AveFeatureDefinitionCollection definitions = new AveFeatureDefinitionCollection(this, mRequest, definitionCollection, "web.features");
            return definitions;
        }

        public Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlerts()
        {
            Dictionary<Guid, Dictionary<Guid, Guid>> alertIDMapping = new Dictionary<Guid, Dictionary<Guid, Guid>>();
            foreach (IAveAlert alert in this.Alerts)
            {
                Guid alertOldId = alert.ID;
                Guid listId = alert.ListID;
                if (alert.Properties.ContainsKey("AlertOldId"))
                {
                    alertOldId = new Guid(alert.Properties["AlertOldId"].ToString());
                }
                if (alertIDMapping.ContainsKey(listId))
                {
                    if (!(alertIDMapping[listId] as Dictionary<Guid, Guid>).ContainsKey(alertOldId))
                    {
                        alertIDMapping[listId].Add(alertOldId, alert.ID);
                    }
                }
                else
                {
                    Dictionary<Guid, Guid> listAlert = new Dictionary<Guid, Guid>();
                    listAlert.Add(alertOldId, alert.ID);
                    alertIDMapping.Add(listId, listAlert);
                }
            }
            return alertIDMapping;
        }


        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                return DataCache.EnsureLoadProperty("WorkflowAssociations",
                    () =>
                    {
                        Dictionary<string, object> workflowsPro = mRequest.GetWorkflowAssociations(this.ServerRelativeUrl, this.Title, this.ID, "web.workflow", null);
                        AveWorkflowAssociationCollection workflows = new AveWorkflowAssociationCollection(this, null, "web.workflow", workflowsPro);
                        return workflows;
                    });
            }
        }

        public IAveWorkflowCollection Workflows
        {
            get { throw new NotImplementedException(); }
        }

        public string WebTemplateName
        {
            get
            {
                return this.WebTemplate + "#" + this.Configuration;
            }
        }


        public void SetSPContextNull()
        {
        }

        public int Count
        {
            get { return this.Webs.Count; }
        }


        public List<Guid> StopListAlerts(IAveList list)
        {
            return null;
        }

        public DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId)
        {
            throw new NotImplementedException();
            //return mSite.QueryService.GetLastAccessedDayOfWeb(siteId, webId);
        }

        public IAveWeb FirstUniqueTopLinkBarNavigationWeb
        {
            get
            {
                if (this.IsRootWeb)
                {
                    base.DataCache.AddProperty("FirstUniqueTopLinkBarNavigationWeb", this);
                }
                else
                {
                    Dictionary<string, object> webProperties = this.mRequest.GetFirstUniqueNavigationWeb(this.ServerRelativeUrl);
                    AveWeb firstUniqueNavigationWeb = new AveWeb(mRequest, this.mSite, null, webProperties);
                    base.DataCache.AddProperty("FirstUniqueTopLinkBarNavigationWeb", firstUniqueNavigationWeb);
                }
                return base.DataCache.GetProperty<IAveWeb>("FirstUniqueTopLinkBarNavigationWeb");
            }
        }

        public IAveWeb FirstUniqueQuickLaunchNavigationWeb
        {
            get
            {
                if (this.IsRootWeb)
                {
                    base.DataCache.AddProperty("FirstUniqueQuickLaunchNavigationWeb", this);
                }
                else
                {
                    var webProperties = this.mRequest.GetQuickLaunchFromInheritWeb(this.ServerRelativeUrl);
                    AveWeb firstUniqueNavigationWeb = new AveWeb(mRequest, this.mSite, null, webProperties);
                    base.DataCache.AddProperty("FirstUniqueQuickLaunchNavigationWeb", firstUniqueNavigationWeb);
                }
                return DataCache.GetProperty<IAveWeb>("FirstUniqueQuickLaunchNavigationWeb");
            }
        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get
            {
                if (this.HasUniqueRoleAssignments)
                {
                    if (m_SecurableObjectImpl == null)
                    {
                        m_SecurableObjectImpl = new AveSecurableObjectImpl(Guid.NewGuid(), this.RoleAssignments);
                    }
                    return m_SecurableObjectImpl;
                }
                else
                {
                    return this.ParentWeb.SecurableObjectImpl;
                }
            }
        }

        public void SetFormForList(int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId)
        {
            IAveRequest request = mRequest as IAveRequest;
            if (request != null)
            {
                request.SetFormForList(this.ServerRelativeUrl, lcid, base64FormTemplate, applicationId, listGuid, contentTypeId);
            }
        }

        public string ProcessBatchData(string strBatchData)
        {
            throw new NotImplementedException();
        }

        public void AddProperty(object key, object value)
        {
            throw new NotImplementedException();
        }

        public List<IAveContentTypeId> GetAllListContentTypeIds()
        {
            return new List<IAveContentTypeId>();
        }

        public IList<IAveAppInstance> GetAppInstancesByProductId(Guid productId)
        {
            IList<IAveAppInstance> apps = new List<IAveAppInstance>();
            Dictionary<string, object> appsProperties = mRequest.GetAppsByProductId(this.ServerRelativeUrl, productId);
            var appListProperties = appsProperties.GetChildren();
            foreach (var appProperty in appListProperties)
            {
                apps.Add(new AveAppInstance(mSite, appProperty));
            }
            return apps;
        }

        public IAveAppInstance GetAppInstanceById(Guid appInstanceId)
        {
            IAveAppInstance appInstance = null;
            Dictionary<string, object> appProperty = mRequest.GetAppInstanceById(this.ServerRelativeUrl, appInstanceId);
            if (appProperty != null && appProperty.Count > 0)
            {
                appInstance = new AveAppInstance(this.Site as AveSite, appProperty);
            }
            return appInstance;
        }

        public IAveAppInstance LoadAndInstallApp(string webServerRelativeUrl, Stream stream)
        {
            Dictionary<string, object> appinstanceProp = mRequest.LoadAndInstallApp(webServerRelativeUrl, stream);
            AveAppInstance instance = new AveAppInstance(this.Site as AveSite, appinstanceProp);
            return instance;
        }
        /// <summary>
        /// Only for Records module. Deploy app.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="restoreMode"></param>
        /// <returns></returns>
        public IAveAppInstance DeployApp(Guid productId, AveRestoreMode restoreMode)
        {
            //var restoreOption = new AveRestoreOption();
            //restoreOption.mAveRestoreMode = restoreMode;
            //this.AppSerializer.SetRestoreOption(restoreOption);
            var appInfo = new AveAppPackageInfo
            {
                ProductId = productId
            };
            return this.AppSerializer.SetObjectData(appInfo);
        }
        public Guid AppInstanceId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("AppInstanceId");
            }
        }

        public bool IsAppWeb
        {
            get { return base.DataCache.GetProperty<Boolean>("IsAppWeb"); }
        }

        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get
            {
                return null;
            }
        }

        public void Recycle()
        {
            throw new NotImplementedException();
        }


        public string Template
        {
            get
            {
                return this.WebTemplate + "#" + this.Configuration;
            }
        }

        #region Add to operate Change Log ** We will implement this in SP2013 first **
        public IAveChangeCollection GetChanges()
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            Dictionary<string, object> changeCollectionDic = mRequest.GetWebChangesByQuery(this.ServerRelativeUrl, (query as AveChangeQuery).DataCache.GetPropertyCache());
            return new AveChangeCollection(changeCollectionDic);
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            throw new NotImplementedException();
        }
        #endregion

        public IAveRecycleBinItemCollection RecycleBin
        {
            get
            {
                return DataCache.EnsureLoadProperty("RecycleBin",
                    () =>
                    {
                        Dictionary<string, object> recycleBinItemCollection = mRequest.GetRecycleBin(this.ServerRelativeUrl);
                        AveRecycleBinItemCollection recycleBinItems = new AveRecycleBinItemCollection(mRequest, this.Site, this, recycleBinItemCollection);
                        return recycleBinItems;
                    });
            }
        }

        #region add for SP2013   Reindex

        public int SearchVersion
        {
            get
            {
                object obj2 = this.AllProperties["vti_searchversion"];
                return ((obj2 is int) ? ((int)obj2) : 0);
            }
            set
            {
                this.AllProperties["vti_searchversion"] = value;
            }
        }
        #endregion


        public void UpgradeAppByProductId(Guid productId)
        {
            throw new NotImplementedException();
        }

        private readonly object replyLock = new object();
        private Dictionary<int, AveBaseItemInfo> discussionReplyCache = null;
        public Dictionary<int, AveBaseItemInfo> DiscussionReplyCache
        {
            get
            {
                if (discussionReplyCache == null)
                {
                    lock (replyLock)
                    {
                        if (discussionReplyCache == null)
                        {
                            discussionReplyCache = new Dictionary<int, AveBaseItemInfo>();
                        }
                    }
                }
                return discussionReplyCache;
            }
        }

        private readonly object topicLock = new object();
        private Dictionary<int, AveBaseItemInfo> discussionTopicCache = null;
        public Dictionary<int, AveBaseItemInfo> DiscussionTopicCache
        {
            get
            {
                if (discussionTopicCache == null)
                {
                    lock (topicLock)
                    {
                        if (discussionTopicCache == null)
                        {
                            discussionTopicCache = new Dictionary<int, AveBaseItemInfo>();
                        }
                    }
                }
                return discussionTopicCache;
            }
        }

        private bool isEnableAchievementPoints = false;

        private bool CheckAchievementPointsEnabled(IAveList list)
        {
            if (list.RootFolder.Properties != null && list.RootFolder.Properties["AchievementPointsEnabled"] != null)
            {
                bool Enabled = false;
                string strEnabled = list.RootFolder.Properties["AchievementPointsEnabled"].ToString();
                if (bool.TryParse(strEnabled, out Enabled))
                {
                    return Enabled;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private List<int> GetPeopleReputationScores(IAveList list)
        {
            //newPost, reply, votedUp, bested
            int newPost = 0;
            int reply = 0;
            int votedUp = 0;
            int bested = 0;
            if (list != null && list.RootFolder.Properties != null && list.RootFolder.Properties["AchievementPoints"] != null)
            {
                string strScores = list.RootFolder.Properties["AchievementPoints"].ToString();
                string[] tmpScores = strScores.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (tmpScores != null && tmpScores.Length == 4)
                {
                    int.TryParse(tmpScores[0], out newPost);
                    int.TryParse(tmpScores[1], out reply);
                    int.TryParse(tmpScores[2], out votedUp);
                    int.TryParse(tmpScores[3], out bested);
                }
            }
            return new List<int> { newPost, reply, votedUp, bested };
        }

        private IAveListItem GetMembersListItem(IAveList memberList, int userId)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.ViewXml = "<View><Query><Where><Eq><FieldRef Name='Member' LookupId='TRUE'/><Value Type='Lookup'>" + userId + "</Value></Eq></Where></Query></View>";
            IAveListItemCollection items = memberList.GetItems(query);
            if (items.Count == 1)
            {
                return items[0];
            }
            return null;
        }

        public void RecalculateForCommunitySite(IAveList discussionsList, Dictionary<int, int> itemIdmapping)
        {
            if (DiscussionTopicCache.Count <= 0 && DiscussionReplyCache.Count <= 0)
            {
                return;
            }
            if (discussionsList == null)
            {
                mLogger.Log(AveLogLevel.DEBUG, "The discussion list in community site is null.");
                return;
            }

            isEnableAchievementPoints = CheckAchievementPointsEnabled(discussionsList);

            //newPost, reply, votedUp, bested
            List<int> scores = GetPeopleReputationScores(discussionsList);
            IAveList categoryList = GetList("/Lists/Categories");
            IAveList membersList = GetList("/Lists/Members");
            foreach (int rowId in DiscussionTopicCache.Keys)
            {
                try
                {
                    IAveListItem discussion = discussionsList.GetItemById(rowId);
                    UpdateDiscussionProperties(discussion, itemIdmapping);
                    RecalculateForCategoryList(categoryList, discussion, false);
                    RecalculateForMembersList(membersList, discussion, scores, false);
                }
                catch (Exception ex)
                {
                    mLogger.Log(AveLogLevel.WARN, "An error occurred while recalculate for community site with discussion. Guid:{0}. Error:{1}", rowId.ToString(), ex.ToString());
                }
            }
            foreach (int rowId in DiscussionReplyCache.Keys)
            {
                try
                {
                    IAveListItem reply = discussionsList.GetItemById(rowId);
                    RecalculateForCategoryList(categoryList, reply, true);
                    RecalculateForMembersList(membersList, reply, scores, true);
                }
                catch (Exception ex)
                {
                    mLogger.Log(AveLogLevel.WARN, "An error occurred while recalculate for community site with reply.Guid:{0}. Error:{1}", rowId.ToString(), ex.ToString());
                }
            }
        }

        //update discussion popularity
        private void UpdateDiscussionProperties(IAveListItem discussion, Dictionary<int, int> itemIdMap)
        {
            try
            {
                discussion[AveCommunitiesConstants.ContentReputation_Popularity_FieldId] = CalculatePopularity(discussion);
                if (discussion[AveCommunitiesConstants.CommunityDiscussion_BestAnswerId_FieldId] != null)
                {
                    int sourId = 0;
                    int desId = 0;
                    int.TryParse(discussion[AveCommunitiesConstants.CommunityDiscussion_BestAnswerId_FieldId].ToString(), out sourId);
                    if (sourId > 0 && itemIdMap.TryGetValue(sourId, out desId))
                    {
                        discussion[AveCommunitiesConstants.CommunityDiscussion_BestAnswerId_FieldId] = desId;
                    }
                }
                discussion.SystemUpdate();
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "An error occurred while update discussion properties. Error:{0}", ex.ToString());
            }
        }
        ////update discussion popularity
        //private void UpdateDiscussionProperties(IAveListItem disItem, Dictionary<int, int> itemIdMap)
        //{
        //    Guid parentFolderFieldGuid = new Guid("a9ec25bf-5a22-4658-bd19-484e52efbe1a");
        //    int parentRowId = GetInt32ColumnValue(disItem, parentFolderFieldGuid);
        //    if (parentRowId > 0)
        //    {
        //        try
        //        {
        //            IAveListItem discussion = disItem.ParentList.GetItemById(parentRowId);
        //            discussion[AveCommunitiesConstants.ContentReputation_Popularity_FieldId] = CalculatePopularity(discussion);
        //            if (discussion[AveCommunitiesConstants.CommunityDiscussion_BestAnswerId_FieldId] != null)
        //            {
        //                int sourId = 0;
        //                int desId = 0;
        //                int.TryParse(discussion[AveCommunitiesConstants.CommunityDiscussion_BestAnswerId_FieldId].ToString(), out sourId);
        //                if (sourId > 0 && itemIdMap.TryGetValue(sourId, out desId))
        //                {
        //                    discussion[AveCommunitiesConstants.CommunityDiscussion_BestAnswerId_FieldId] = desId;
        //                }
        //            }
        //            discussion.SystemUpdate();
        //        }
        //        catch (Exception ex)
        //        {
        //            mLogger.Log(AveLogLevel.DEBUG, "An error occurred while update discussion properties. Error:{0}", ex.ToString());
        //        }
        //    }
        //}

        private double CalculatePopularity(IAveListItem discussion)
        {
            try
            {
                Guid CreatedFieldGuid = new Guid("8c06beca-0777-48f7-91c7-6da68bc07b69");
                int ratingsCount = GetInt32ColumnValue(discussion, AveCommunitiesConstants.ContentReputation_DescendantRatingsCount_FieldId);
                int likesCount = GetInt32ColumnValue(discussion, AveCommunitiesConstants.ContentReputation_DescendantLikesCount_FieldId);
                int itemCount = 0;
                if (discussion.Folder != null)
                {
                    itemCount = discussion.Folder.ItemCount;
                }
                double num = 1.0;
                DateTime dateTime;
                if (discussion.Fields.Contains(CreatedFieldGuid) && discussion.Fields[CreatedFieldGuid] != null)
                {
                    if (DateTime.TryParse(discussion[CreatedFieldGuid].ToString(), out dateTime))
                    {
                        IAveUser currentUser = discussion.Web.CurrentUser;
                        IAveTimeZone timeZone = (currentUser != null && currentUser.RegionalSettings != null) ? currentUser.RegionalSettings.TimeZone : discussion.Web.RegionalSettings.TimeZone;
                        DateTime d1 = (timeZone != null) ? timeZone.LocalTimeToUTC(dateTime) : dateTime;
                        DateTime d2 = (timeZone != null) ? timeZone.LocalTimeToUTC(discussion.Web.Created) : discussion.Web.Created;
                        double totalDays = (d1 - d2).TotalDays;
                        num = (totalDays < 1.0 ? 1.0 : totalDays);
                    }
                }
                return Math.Log10((double)checked(ratingsCount + likesCount + 5 * itemCount)) + 0.05 * num;
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "An error occurred while calculate the popularity of discussion:{0}. Error:{1}", discussion.ID, ex.ToString());
                return default(double);
            }
        }

        //update category list item reply/discussion count
        private void RecalculateForCategoryList(IAveList categoryList, IAveListItem disItem, bool isReply)
        {
            try
            {
                if (categoryList == null)
                {
                    mLogger.Log(AveLogLevel.DEBUG, "Category List is null when recalculate for it. ");
                    return;
                }

                int categoryRowId = GetCategoryItemRowId(disItem, isReply);
                if (categoryRowId > 0)
                {
                    IAveListItem categoryItem = categoryList.GetItemById(categoryRowId);
                    if (!isReply)
                    {
                        int topicNum = GetInt32ColumnValue(categoryItem, AveCommunitiesConstants.CategoriesList_TopicCount_FieldId);
                        categoryItem[AveCommunitiesConstants.CategoriesList_TopicCount_FieldId] = topicNum + 1;
                    }
                    else
                    {
                        int replyNum = GetInt32ColumnValue(categoryItem, AveCommunitiesConstants.CategoriesList_ReplyCount_FieldId);
                        categoryItem[AveCommunitiesConstants.CategoriesList_ReplyCount_FieldId] = replyNum + 1;
                    }
                    categoryItem.SystemUpdate();
                }
                else
                {
                    mLogger.Log(AveLogLevel.DEBUG, "Can not get category item that the discussion lookup to. Discussion Item RowId:{0}", disItem.ID);
                    return;
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.WARN, "An error occurred while recalculate for category list. Error:{0}", ex.ToString());
            }
        }

        private int GetCategoryItemRowId(IAveListItem disItem, bool isReply)
        {
            try
            {
                IAveListItem item = disItem;
                if (isReply)
                {
                    Guid parentFolderFieldGuid = new Guid("a9ec25bf-5a22-4658-bd19-484e52efbe1a");
                    int parentFolderId = GetInt32ColumnValue(disItem, parentFolderFieldGuid, ";#");
                    if (parentFolderId > 0)
                    {
                        item = disItem.ParentList.GetItemById(parentFolderId);
                    }
                }
                return GetInt32ColumnValue(item, new Guid("3f44dee7-b4ba-4e0f-9a4c-84f4420dfaf6"), ";#");  //CategoriesLookup
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "An error occurred while get category item id. Error:{0}", ex.ToString());
                return 0;
            }

        }

        private void RecalculateForMembersList(IAveList membersList, IAveListItem disItem, List<int> scores, bool isReply)
        {
            try
            {
                if (membersList == null)
                {
                    mLogger.Log(AveLogLevel.DEBUG, "Memebers List is null when recalculate for it.");
                    return;
                }
                Guid AuthorFieldGuid = new Guid("1df5e554-ec7e-46a6-901d-d85a3881cb18");
                int author = GetInt32ColumnValue(disItem, AuthorFieldGuid, ";#");
                if (author > 0)
                {
                    bool needUpdateReputation = true;
                    if (isReply)
                    {
                        Guid parentFolderFieldGuid = new Guid("a9ec25bf-5a22-4658-bd19-484e52efbe1a");
                        int parentRowId = GetInt32ColumnValue(disItem, parentFolderFieldGuid);
                        if (parentRowId > 0)
                        {
                            int parentAuthorId = GetInt32ColumnValue(disItem.ParentList, parentRowId, AuthorFieldGuid, ";#");
                            needUpdateReputation = author != parentAuthorId;
                        }
                    }
                    else
                    {
                        Guid bestAnswerIdFieldGuid = new Guid("a8b93fba-7396-443d-9884-ee332caa4560");
                        int bestAnswerId = GetInt32ColumnValue(disItem, bestAnswerIdFieldGuid);
                        if (bestAnswerId > 0)
                        {
                            int bestAnswerAuthor = GetInt32ColumnValue(disItem.ParentList, bestAnswerId, AuthorFieldGuid, ";#");
                            if (bestAnswerAuthor > 0)
                            {
                                IAveListItem bestAnswerMemberItem = GetMembersListItem(membersList, bestAnswerAuthor);
                                if (bestAnswerMemberItem != null)
                                {
                                    bool needUpdate = bestAnswerAuthor != author;
                                    UpdateMembersListItemBestAnswer(bestAnswerMemberItem, scores, needUpdate);
                                }
                            }
                        }
                    }
                    IAveListItem memberItem = GetMembersListItem(membersList, author);
                    if (memberItem != null)
                    {
                        UpdateMembersListItem(memberItem, disItem, scores, isReply, needUpdateReputation);
                    }
                }
                else
                {
                    mLogger.Log(AveLogLevel.DEBUG, "Can not get members item that the discussion lookup to. Discussion Item RowId:{0}", disItem.ID);
                    return;
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.WARN, "An error occurred while recalculate for members list. Error:{0}", ex.ToString());
            }
        }

        //update member list item best answer count and its reputation score if needed
        private void UpdateMembersListItemBestAnswer(IAveListItem memberItem, List<int> scores, bool needUpdateReputation)
        {
            try
            {
                int bestAnswerNum = GetInt32ColumnValue(memberItem, AveCommunitiesConstants.CommunityMembership_NumBestResponses_FieldId);
                memberItem[AveCommunitiesConstants.CommunityMembership_NumBestResponses_FieldId] = bestAnswerNum + 1;
                if (needUpdateReputation && isEnableAchievementPoints)
                {
                    int score = GetInt32ColumnValue(memberItem, AveCommunitiesConstants.CommunityMembership_ReputationScore_FieldId);
                    score = score + scores[3];
                    memberItem[AveCommunitiesConstants.CommunityMembership_ReputationScore_FieldId] = score > 0 ? score : 0;
                }
                memberItem.SystemUpdate();
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "An error occurred while update member best answer count. Error:{0}", ex.ToString());
            }
        }

        // update member list item reply/discussion count and its reputation score if needed.
        private void UpdateMembersListItem(IAveListItem memberItem, IAveListItem disItem, List<int> scores, bool isReply, bool needUpdateReputation)
        {
            if (isReply)
            {
                int replyNum = GetInt32ColumnValue(memberItem, AveCommunitiesConstants.CommunityMembership_NumReplies_FieldId);
                memberItem[AveCommunitiesConstants.CommunityMembership_NumReplies_FieldId] = replyNum + 1;
                if (needUpdateReputation && isEnableAchievementPoints)
                {
                    int reputationScore = GetInt32ColumnValue(memberItem, AveCommunitiesConstants.CommunityMembership_ReputationScore_FieldId);
                    reputationScore = reputationScore + scores[1];
                    memberItem[AveCommunitiesConstants.CommunityMembership_ReputationScore_FieldId] = reputationScore > 0 ? reputationScore : 0;
                }
            }
            else
            {
                int disNum = GetInt32ColumnValue(memberItem, AveCommunitiesConstants.CommunityMembership_NumDiscussions_FieldId);
                memberItem[AveCommunitiesConstants.CommunityMembership_NumDiscussions_FieldId] = disNum + 1;
                if (isEnableAchievementPoints)
                {
                    int score = GetInt32ColumnValue(memberItem, AveCommunitiesConstants.CommunityMembership_ReputationScore_FieldId);
                    score = score + scores[0];
                    memberItem[AveCommunitiesConstants.CommunityMembership_ReputationScore_FieldId] = score > 0 ? score : 0;
                }
            }
            memberItem.SystemUpdate();
        }

        private int GetInt32ColumnValue(IAveList list, int RowId, Guid fieldGuid, string key)
        {
            int result = 0;
            try
            {
                IAveListItem item = list.GetItemById(RowId);
                result = GetInt32ColumnValue(item, fieldGuid, key);
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "Can not get item by id: {0}. Error:{1}", RowId.ToString(), ex.ToString());
            }
            return result;
        }

        private int GetInt32ColumnValue(IAveListItem item, Guid fieldGuid)
        {
            return GetInt32ColumnValue(item, fieldGuid, string.Empty);
        }

        //for user and category column like "2;#General"
        private int GetInt32ColumnValue(IAveListItem item, Guid fieldGuid, string key)
        {
            try
            {
                int result = 0;
                string columnValue = item[fieldGuid] == null ? "0" : item[fieldGuid].ToString();
                if (!string.IsNullOrEmpty(key) && columnValue.IndexOf(key) > 0)
                {
                    columnValue = columnValue.Replace(key, "$");
                    string[] results = columnValue.Split('$');
                    if (results.Length > 0)
                    {
                        columnValue = results[0];
                    }
                }
                Int32.TryParse(columnValue, out result);
                return result;
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "An error occurred while get item column value. Column Guid:{0}. Error:{1}", fieldGuid.ToString(), ex.ToString());
                return 0;
            }
        }

        public void LoadListTitleResource(string cultureName)
        {
            try
            {
                var titleResource = mRequest.GetListTitleResource(this.ServerRelativeUrl, cultureName);

                ((AveListCollection)Lists).EnsureTitleResource(cultureName, titleResource);
            }
            catch (Exception ex)
            {
                mLogger.Warn("Load list title resource for culture:{0} under web:{1} failed:{2}", cultureName, ServerRelativeUrl, ex);
            }
        }

        public void PublishNintexWorkflow(string workflowId, string workflowRestrictToScope)
        {
            mRequest.PublishNintexWorkflow(Url, workflowId, workflowRestrictToScope);
        }

        /// <summary>
        /// 请注意如果一个list/lib本身是打破继承的，同时list下面包含打破继承的item，该API无法判断出该list下是否包含打破继承的item
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetListsIdContainItemsWithUniquePermissions()
        {
            return mRequest.GetListsIdContainItemsWithUniquePermissions(this.Url);
        }

        public bool GetAccessRequestApprover()
        {
            return mRequest.GetAccessRequestApprover(this.Url);
        }

        /// <summary>
        /// 如果defaultApprover为false，但是email是空，defaultApprover更新时将变为true
        /// </summary>
        /// <param name="defaultApprover"></param>
        /// <param name="email"></param>
        public void SetAccessRequestApprover(bool defaultApprover, string email)
        {
            mRequest.SetAccessRequestApprover(this.Url, defaultApprover, email);
        }

        public object GetClientContext()
        {
            return mRequest.GetClientContext();
        }

        public Dictionary<string,object> GetListItemSharingInformation(Guid listid,int itemID,bool excludeCurrentUser=true, bool excludeSiteAdmin=false, bool excludeSecurityGroups = true, bool retrieveAnonymousLinks = true, bool retrieveUserInfoDetails = false, bool checkForAccessRequests = false)
        {
            return mRequest.GetListItemSharingInformation(listid,itemID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests);
        }

        public void DisableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds)
        {
            mRequest.DisableAlert(webServerRelativeUrl, disableAlertIds);
        }

        public void EnableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds)
        {
            mRequest.EnableAlert(webServerRelativeUrl, disableAlertIds);
        }

        public Dictionary<string, (Guid UniqueId, Guid ListId)> GetStubNodesByBatchPath(List<string> serverRelativeUrls)
        {
            try
            {
                return mRequest.GetStubNodesByBatchPath(serverRelativeUrls);
            }
            catch (Exception e)
            {
                mLogger.Error("Batch check files exist failed. Exception: {0}", e);
                return [];
                //throw;
            }
        }
    }
}
