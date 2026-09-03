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
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/01/31", "Navy.Li@avepoint.com", "yanjun.wang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_CS_1 }, null, true)]
    [AveCodeReview("2012/04/19", "yuzhi.jiang@avepoint.com", "yanjun.wang@AvePoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CO_5 }, null, true)]
    [AveJsonIgnore]
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
        private AveAppSerializer m_AppSerializer;
        private AveFeatureSerializer m_FeatureSerializer;
        private AveWebCollection.ISPWebCollectionProvider webCollectionProvider;
        private List<IAveWeb> propertyOpenWebs = new List<IAveWeb>();
        private AveUserResource mTitleResource;
        private AveUserResource mDescriptionResource;
        #region lock objects,用于处理多线程同时获取同一个对象导致前一个被覆盖的问题

        private object privateLock = new object();
        private object privateLockAvailableContentTypes = new object();
        private object privateLockContentTypes = new object();
        private object privateLockFields = new object();
        private object privateLockWorkflowAssociations = new object();
        private object privateLockWorkflowTemplates = new object();
        private object privateLockTitleResource = new object();
        private object privateLockDescriptionResource = new object();
        private Guid? variationLabelListId = null;
        private Guid? relationshipsListId = null;
        #endregion

        private Dictionary<string, object> lockDictionary = new Dictionary<string, object> { };
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWeb));
        private IAveWebCollection allWebs;

        /// <summary>
        /// 通过已经取出来的属性初始化Web对象
        /// </summary>
        /// <param name="site"></param>
        /// <param name="prop"></param>
        public AveWeb(AveSite site, Dictionary<string, object> prop)
            : base(site.Request)
        {
            mRequest = site.Request;
            mSite = site;
            base.DataCache.AddPropertyies(prop);
        }

        /// <summary>
        /// 通过Id来初始化对象，
        /// </summary>
        /// <param name="site"></param>
        /// <param name="id"></param>
        public AveWeb(AveSite site, Guid id)
            : base(site.Request)
        {
            mRequest = site.Request;
            mSite = site;
            Dictionary<string, object> webProperties = this.mRequest.GetWeb(id);
            base.DataCache.AddPropertyies(webProperties);
        }

        /// <summary>
        /// 通过Url来初始化对象
        /// </summary>
        /// <param name="site"></param>
        /// <param name="url"></param>
        public AveWeb(AveSite site, string url)
            : base(site.Request)
        {
            mRequest = site.Request;
            mSite = site;
            Dictionary<string, object> webProperties = this.mRequest.GetWeb(url);
            base.DataCache.AddPropertyies(webProperties);
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

        internal AveWebCollection.ISPWebCollectionProvider WebCollectionProvider
        {
            get { return webCollectionProvider ?? (webCollectionProvider = new SPWebCollectionProvider(this, this.mRequest)); }
        }

        public bool HaveAddAndCustomizePagesPermission
        {
            get { return mRequest.HaveAddAndCustomizePagesPermission; }
        }

        #region IAveWeb Members

        public string AlternateCssUrl
        {
            get
            {
                GetMasterPageProperties();
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
                    else
                    {
                        alertCollection = new AveAlertCollection(this, mRequest);
                    }
                    base.DataCache.PropertiesCache["Alerts"] = alertCollection;
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
                if (!AllowUnsafeUpdates.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowUnsafeUpdates", value);
                }
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
                    base.DataCache.PropertiesCache["AllProperties"] = new AveCustomHashtable(table, SetChangeProperty);
                    //return table;
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
                    AveUserCollection users = new AveUserCollection(mRequest, this, "web.allUsers", this.Groups.Count > 0 ? this.Groups[0].Name : string.Empty, userProperties);
                    base.DataCache.PropertiesCache.Add("AllUsers", users);
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

        /// <summary>
        /// Client取不到Author属性。
        /// </summary>
        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author"))
                {
                    string authorLoginName = base.DataCache.GetProperty<string>("Author" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser user = SiteUsers.GetByLoginName(authorLoginName) as AveUser;
                    base.DataCache.PropertiesCache["Author"] = user;
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
            set
            {
                base.DataCache.AddChangedProperty("Author", value as AveUser);
            }
        }

        public IAveFieldCollection AvailableFields
        {
            get
            {
                AveFieldCollection availableFields = null;
                lock (privateLockFields)
                {
                    if (base.DataCache.IsPropertyNotLoaded("AvailableFields"))
                    {
                        availableFields = new AveFieldCollection(this, null, "web.availableFields", null);
                        base.DataCache.PropertiesCache["AvailableFields"] = availableFields;
                    }
                    else
                    {
                        availableFields = base.DataCache.GetProperty<AveFieldCollection>("AvailableFields");
                        if (availableFields.IsCollectionDirty)
                        {
                            availableFields.UpdateCollectionInternally();
                        }
                    }
                }
                return availableFields;
            }
        }

        public IAveContentTypeCollection AvailableContentTypes
        {
            get
            {
                AveContentTypeCollection availableContentTypeCollection = null;
                lock (privateLockAvailableContentTypes)
                {
                    if (base.DataCache.IsPropertyNotLoaded("AvailableContentTypes"))
                    {
                        availableContentTypeCollection = new AveContentTypeCollection(this, null, "web.availableContentTypes");
                        base.DataCache.PropertiesCache["AvailableContentTypes"] = availableContentTypeCollection;
                    }
                    else
                    {
                        availableContentTypeCollection = base.DataCache.GetProperty<AveContentTypeCollection>("AvailableContentTypes");
                        //IsCollectionDirty 控制reload时更新，IsDirty与local一致，在wrapper restore中使用
                        if (availableContentTypeCollection.IsCollectionDirty || availableContentTypeCollection.IsDirty)
                        {
                            availableContentTypeCollection.UpdateCollectionInternally();
                        }
                    }
                }
                return availableContentTypeCollection;
            }
        }

        public IAveContentTypeCollection ContentTypes
        {
            get
            {
                AveContentTypeCollection contentTypeCollection = null;
                lock (privateLockContentTypes)
                {
                    if (base.DataCache.IsPropertyNotLoaded("ContentTypes"))
                    {
                        contentTypeCollection = new AveContentTypeCollection(this, null, "web.contentTypes");
                        base.DataCache.PropertiesCache["ContentTypes"] = contentTypeCollection;
                    }
                    else
                    {
                        contentTypeCollection = base.DataCache.GetProperty<AveContentTypeCollection>("ContentTypes");
                        //IsCollectionDirty 控制reload时更新，IsDirty与local一致，在wrapper restore中使用
                        if (contentTypeCollection.IsCollectionDirty || contentTypeCollection.IsDirty)
                        {
                            contentTypeCollection.UpdateCollectionInternally();
                        }
                    }
                }
                return contentTypeCollection;
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

        public IAveFeatureCollection Features
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Features"))
                {
                    Dictionary<string, object> featureCollection = mRequest.GetFeatures(this.ServerRelativeUrl, "web.features");
                    AveFeatureCollection features = new AveFeatureCollection(this, mRequest, featureCollection, "web.features");
                    base.DataCache.PropertiesCache.Add("Features", features);
                    return features;
                }
                return base.DataCache.GetProperty<IAveFeatureCollection>("Features");
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                AveFieldCollection fields = null;
                lock (privateLockFields)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Fields"))
                    {
                        fields = new AveFieldCollection(this, null, "web.fields", null);
                        base.DataCache.PropertiesCache["Fields"] = fields;
                    }
                    else
                    {
                        fields = base.DataCache.GetProperty<AveFieldCollection>("Fields");
                        if (fields.IsCollectionDirty)
                        {
                            fields.UpdateCollectionInternally();
                        }
                    }
                }
                return fields;
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
                    propertyOpenWebs.Add(web);
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
                    base.DataCache.PropertiesCache["Groups"] = groups;
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
                        base.DataCache.PropertiesCache["SiteGroups"] = siteGroups;
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

        public int WorkingLanguage
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("WorkingLanguage"))
                {
                    var language = (int)Language;
                    
                    if (this.IsMultilingual && !string.IsNullOrEmpty(mSite.UserAccountInfo.UserName))
                    {
                        mSite.GetWorkingLanguage(ref language);
                    }
                    if (this.SupportedUICultures.Contains(new CultureInfo(language)))
                    {
                        base.DataCache.PropertiesCache["WorkingLanguage"] = language;
                    }
                    else
                    {
                        base.DataCache.PropertiesCache["WorkingLanguage"] = (int)Language;
                    }
                }
                return base.DataCache.GetProperty<int>("WorkingLanguage");
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
                        AveListCollection Lists = new AveListCollection(mRequest, this);
                        base.DataCache.PropertiesCache["Lists"] = Lists;
                    }
                    else if (base.DataCache.GetProperty<AveListCollection>("Lists").IsDirty)
                    {
                        base.DataCache.GetProperty<AveListCollection>("Lists").UpdateCollectionInternally(mRequest, this);
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
                    Dictionary<string, object> listCollectionProperties = mRequest.GetLists(this.ID);
                    AveListCollection Lists = new AveListCollection(mRequest, this, listCollectionProperties);
                    base.DataCache.PropertiesCache["BrowserLists"] = Lists;
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
                }
            }
        }

        public IAveNavigation Navigation
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Navigation"))
                {
                    Dictionary<string, object> navigation = mRequest.GetNavigation(this.ServerRelativeUrl);
                    AveNavigation navg = new AveNavigation(this, mRequest, navigation);
                    base.DataCache.PropertiesCache["Navigation"] = navg;
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
                if (this.IsRootWeb)
                {
                    lock (privateLock)
                    {
                        if (base.DataCache.IsPropertyNotLoaded("SiteUsers"))
                        {
                            Dictionary<string, object> userProperties = mRequest.GetUsers(this.ServerRelativeUrl, null, "web.siteUsers");
                            AveUserCollection users = new AveUserCollection(mRequest, this, "web.siteUsers", this.Groups.Count > 0 ? this.Groups[0].Name : string.Empty, userProperties);
                            base.DataCache.PropertiesCache.Add("SiteUsers", users);
                            return users;
                        }
                        return base.DataCache.GetProperty<IAveUserCollection>("SiteUsers");
                    }
                }
                else
                {
                    return mSite.RootWeb.SiteUsers;
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
                    base.DataCache.PropertiesCache["RegionalSettings"] = regionalSettings;
                    return regionalSettings;
                }
                else if (base.DataCache.IsPropertyNotLoaded("RegionalSettings") && base.DataCache.IsPropertyAvailable("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> regionalSettingsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveRegionalSettings regionalSettings = new AveRegionalSettings(this, mRequest, regionalSettingsProperties);
                    base.DataCache.PropertiesCache["RegionalSettings"] = regionalSettings;
                    return regionalSettings;
                }
                return base.DataCache.GetProperty<IAveRegionalSettings>("RegionalSettings");
            }
        }

        public IAveRoleDefinitionCollection RoleDefinitions
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RoleDefinitions"))
                {
                    Dictionary<string, object> roleDefinitionColProperties = mRequest.GetRoleDefinitions(this.ServerRelativeUrl);
                    AveRoleDefinitionCollection roleDefinitonCollection = new AveRoleDefinitionCollection(this, mRequest, roleDefinitionColProperties);
                    base.DataCache.PropertiesCache["RoleDefinitions"] = roleDefinitonCollection;
                }
                return base.DataCache.GetProperty<IAveRoleDefinitionCollection>("RoleDefinitions");
            }
        }

        public IAveFolder RootFolder
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RootFolder"))
                {
                    Dictionary<string, object> folderProp = mRequest.GetFolder(this.ServerRelativeUrl, null, Guid.Empty, this.ServerRelativeUrl);
                    AveFolder rootFolder = new AveFolder(mRequest, this, null, null, folderProp);
                    base.DataCache.PropertiesCache["RootFolder"] = rootFolder;
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
            set
            {
                if (!string.Equals(ServerRelativeUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("ServerRelativeUrl", value);
                }
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
            get { return allWebs ?? (allWebs = new AveWebCollection(mRequest, this.mSite, this.WebCollectionProvider)); }
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
                    GetWebTemplateProperty();
                }
                return base.DataCache.GetProperty<string>("WebTemplate");
            }
        }

        public short Configuration
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Configuration"))
                {
                    GetWebTemplateProperty();
                }
                return base.DataCache.GetProperty<short>("Configuration");
            }
        }

        public int WebTemplateId
        {
            get
            {
                //if (base.DataCache.IsPropertyNotLoaded("WebTemplateId"))
                //{
                //    GetWebTemplateProperty();
                //}
                return base.DataCache.GetProperty<int>("WebTemplateId");
            }
        }

        private void GetWebTemplateProperty()
        {
            string datas = mRequest.GetWebTemplateConfiguration(this.ServerRelativeUrl);
            string[] configuration = datas.Split('#');
            if (configuration.Length == 2)
            {
                base.DataCache.PropertiesCache["WebTemplate"] = configuration[0];
                base.DataCache.PropertiesCache["Configuration"] = short.Parse(configuration[1]);
            }

            //if (datas.ContainsKey("WebTemplateId"))
            //{
            //    base.DataCache.PropertiesCache["WebTemplateId"] = datas["WebTemplateId"];
            //}
        }

        public string Template
        {
            get
            {
                return this.WebTemplate + "#" + this.Configuration;
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ParentWeb"))
                {
                    if (this.IsRootWeb)
                    {
                        base.DataCache.PropertiesCache["ParentWeb"] = null;
                    }
                    else
                    {
                        string parentWebServerRelativeUrl = base.DataCache.GetProperty<string>("ParentWeb" + AveObjectModelConstant.ObjectPropertySuffix);
                        Dictionary<string, object> parentWebProperties = this.mRequest.GetWeb(parentWebServerRelativeUrl);
                        //如果获取web 失败或是其他原因导致parent web没有获取到，需要throw exception
                        if (!(bool)parentWebProperties["Exists"])
                        {
                            throw new AveWrapperBaseException(string.Format("An error occurred while get parent web, parent web url: {0}", parentWebServerRelativeUrl));
                        }
                        AveWeb parentWeb = new AveWeb(this.mSite, parentWebProperties);
                        propertyOpenWebs.Add(parentWeb);
                        base.DataCache.PropertiesCache["ParentWeb"] = parentWeb;
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
                //当前只有local 模拟才会not available,Online API已支持该属性的load
                if (!base.DataCache.IsPropertyAvailable("SiteLogoUrl"))
                {
                    GetWebLogoProperties();
                }
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
                //当前只有local 模拟才会not available,Online API已支持该属性的load
                if (!base.DataCache.IsPropertyAvailable("SiteLogoDescription"))
                {
                    GetWebLogoProperties();
                }
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
                    AveUserCollection users = new AveUserCollection(mRequest, this, "web.users", this.Groups.Count > 0 ? this.Groups[0].Name : string.Empty, userProperties);
                    base.DataCache.PropertiesCache.Add("Users", users);
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
                    List<Dictionary<string, object>> siteAdministratorList = new List<Dictionary<string, object>>(); Dictionary<string, object> siteAdministratorCollectionProperties = new Dictionary<string, object>();
                    foreach (AveUser user in this.SiteUsers)
                    {
                        if (user.IsSiteAdmin)
                        {
                            siteAdministratorList.Add(user.DataCache.PropertiesCache);
                        }
                    }
                    siteAdministratorCollectionProperties.Add(AveObjectModelConstant.ChildrenProperties, siteAdministratorList);
                    //ADO-56391
                    mSiteAdministrators = new AveUserCollection(this.mRequest, this, "web.siteAdministrators", this.Groups.Count > 0 ? this.Groups[0].Name : string.Empty, siteAdministratorCollectionProperties);
                    base.DataCache.PropertiesCache["SiteAdministrators"] = mSiteAdministrators;
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
                    base.DataCache.PropertiesCache["ListTemplates"] = listTemplates;
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
                    base.DataCache.PropertiesCache["CurrentUser"] = currentUser;
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
                    base.DataCache.PropertiesCache["Properties"] = propertyBag;
                }
                IAvePropertyBag prop = base.DataCache.GetProperty<IAvePropertyBag>("Properties");
                if (prop == null)
                {
                    //由于client api中没有properties属性，而properties属性和allproperties一致，所以用allproperties赋值
                    prop = new AvePropertyBag(this, this.mRequest, base.DataCache.GetProperty<Dictionary<string, object>>("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.PropertiesCache["Properties"] = prop;

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
                    Dictionary<string, object> eventReceiversProperties = mRequest.GetEventReceiverDefinitions(this.ServerRelativeUrl, null, null, Guid.Empty, "web.eventReceivers");
                    AveEventReceiverDefinitionCollection eventReceiverDefinitionCol = null;
                    if (eventReceiversProperties != null)
                    {
                        eventReceiverDefinitionCol = new AveEventReceiverDefinitionCollection(this, null, mRequest, "web.eventReceivers", eventReceiversProperties);
                    }
                    base.DataCache.PropertiesCache["EventReceivers"] = eventReceiverDefinitionCol;
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
            return GetAvailableWebTemplates(lcid, true);
        }

        public void ApplyTheme(string theme)
        {
            mLogger.Warn("Online doesn't support the ApplyTheme method.");
        }

        public void ApplyWebTemplate(IAveWebTemplate webTemplate)
        {
            if (webTemplate == null)
            {
                throw new ArgumentNullException("webTemplate");
            }
            mRequest.ApplyWebTemplate(this.ServerRelativeUrl, webTemplate.Name);
        }

        public bool Provisioned
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            Dispose();
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
                folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, parentList.Title, parentList.ID, serverRelativeUrl);
            }
            else
            {
                folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, null, Guid.Empty, serverRelativeUrl);
            }
            return new AveFolder(mRequest, this, parentList, null, folderProperties);
        }

        /// <summary>
        /// Only Online Support This Method
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public IAveFolder GetFolder(Guid uniqueId)
        {
            var folderProperties = mRequest.GetFolderById(this.ServerRelativeUrl, uniqueId);
            if (!(bool)folderProperties["Exists"])
            {
                return new AveFolder(mRequest, this, null, null, folderProperties);
            }
            AveList parentList = this.GetList((string)folderProperties["ServerRelativeUrl"]) as AveList;
            return new AveFolder(mRequest, this, parentList, null, folderProperties);
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
            using (new AvePerformanceScope("AvePoint.ObjectModel.Common.AveWeb.GetFolder"))
            {
                if (string.IsNullOrEmpty(serverRelativeUrl) || !serverRelativeUrl.StartsWith(this.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) || rowId < default(int))
                {
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
                    folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, parentList.Title, parentList.ID, folderUrl);
                }
                else
                {
                    folderProperties = mRequest.GetFolder(this.ServerRelativeUrl, null, Guid.Empty, serverRelativeUrl);
                }
                return new AveFolder(mRequest, this, parentList, null, folderProperties);
            }
        }

        public IAveFile GetFile(string serverRelativeUrl)
        {
            Dictionary<string, object> fileProperties = null;

            if (!serverRelativeUrl.StartsWith(this.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
            {
                if (serverRelativeUrl.StartsWith(this.Url, StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = serverRelativeUrl.Substring(serverRelativeUrl.IndexOf(this.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
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

        /// <summary>
        /// only online support
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public IAveFile GetFile(Guid fileId)
        {
            Dictionary<string, object> fileProperties = mRequest.GetFileById(this.ServerRelativeUrl, fileId);
            if (!(bool)fileProperties["Exists"])
            {
                return new AveFile(mRequest, this, null, null, fileProperties);
            }
            AveList parentList = this.GetList((string)fileProperties["ServerRelativeUrl"]) as AveList;
            return new AveFile(mRequest, this, parentList, null, fileProperties);
        }

        public IAveFile GetFile(Guid fileId, string serverRelativeUrl)
        {
            return this.GetFile(serverRelativeUrl);
        }

        /// <summary>
        /// strUrl : 
        /// 1.Server Relative Url, like:/sites/webName/Lists/ListUrl.
        /// 2.Web Relative Url, like: /Lists/ListUrl, Lists/ListUrl, /LibraryUrl, LibraryUrl.
        /// 3.List Full Url, like: http://example:443/sites/webName/Lists/Calendar.
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public IAveList GetList(string strUrl)
        {
            if (strUrl.StartsWith(this.Url, StringComparison.OrdinalIgnoreCase))
            {
                //3. Full Url.
                Uri tempUri = new Uri(strUrl);
                strUrl = tempUri.ToString().Remove(0, tempUri.GetLeftPart(UriPartial.Authority).Length);
            }
            else
            {
                strUrl = "/" + strUrl.Trim('/');
                if (!strUrl.StartsWith(this.ServerRelativeUrl + '/', StringComparison.OrdinalIgnoreCase))
                {
                    //2. Web Relative Url
                    strUrl = AveUrlUtility.CombineUrl(this.ServerRelativeUrl, strUrl);
                }
            }
            strUrl = '/' + strUrl.Trim('/') + '/';
            foreach (IAveList list in this.Lists)
            {
                if (list.RootFolder.Exists == true &&
                    strUrl.StartsWith(list.RootFolder.ServerRelativeUrl + '/', StringComparison.OrdinalIgnoreCase))
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

        public IAveUser EnsureUser(string loginName)
        {
            string searchLoginName = loginName;
            //ADO-154903 模拟365，ensure FBA user,站点中存在同名的AD user时，需用截取前的name来ensure
            if (mSite.IsOnlineSite && !loginName.Contains("|") && loginName.Contains(":"))
            {
                int index = loginName.IndexOf(':');
                searchLoginName = loginName.Substring(index + 1);
            }
            AveUser ensureUser = (AveUser)this.SiteUsers.GetByLoginName(searchLoginName);
            if (ensureUser == null)
            {
                Dictionary<string, object> ensureUserProperties = this.mRequest.GetEnsureUser(this.ServerRelativeUrl, searchLoginName);
                ensureUser = new AveUser(this.mRequest, this, "web.ensureUser", ensureUserProperties);
                (this.SiteUsers as AveUserCollection).ListData.Add(ensureUser);
            }
            return ensureUser;
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                //此处传入的serverRelativeUrl是用于open current web，所以这个url不应该是change后的url，需要从web原有的properties中获取这个属性而不是changeProperty
                object originalWebUrl = null;
                string serverRelativeUrl = this.ServerRelativeUrl;
                if (base.DataCache.PropertiesCache.TryGetValue("ServerRelativeUrl", out originalWebUrl) && originalWebUrl != null)
                {
                    serverRelativeUrl = originalWebUrl.ToString();
                }
                Dictionary<string, object> webProperties;
                try
                {
                    webProperties = mRequest.UpdateWeb(serverRelativeUrl, base.DataCache.ChangedProperties);
                }
                catch
                {
                    base.DataCache.ResetChangedProperties();
                    throw;
                }
                base.DataCache.UpdateProperties(webProperties);
                //由于在UpdateProperties的时候并没有一起更新DataCache里面的AllProperties属性，所以在这里将AllProperties给Remove掉，下一次用的时候会再次Load一下，保持一致。
                if (base.DataCache.PropertiesCache.ContainsKey("AllProperties"))
                {
                    base.DataCache.PropertiesCache.Remove("AllProperties");
                }
                if (base.DataCache.IsPropertyAvailable("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> regionalSettingsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveRegionalSettings regionalSettings = new AveRegionalSettings(this, mRequest, regionalSettingsProperties);
                    base.DataCache.PropertiesCache["RegionalSettings"] = regionalSettings;
                }
            }
        }

        public void Delete()
        {
            if (!this.IsRootWeb)
            {
                this.mRequest.DeleteWeb(this.ServerRelativeUrl);
            }
            else
            {
                DeleteLists(this, DeletedListType.Content); //delete lists under rootWeb
            }
        }

        private void DeleteLists(IAveWeb mWeb, DeletedListType type)
        {
            try
            {
                using (mWeb)
                {
                    Guid taxonomyHiddenListId = GetTaxonomyHiddenListId(mWeb);
                    int listCount = mWeb.Lists.Count;
                    for (int i = listCount - 1; i >= 0; i--)
                    {
                        IAveList list = null;
                        try
                        {
                            list = mWeb.Lists[i];

                            //过滤掉wfpub这个List,如果删除在还原其下的Folder时因调用System Update会抛出异常，导致Job Completed with exception，暂时作此处理。Doc-69855.
                            if (list.Title.Equals("wfpub", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            try
                            {
                                if (list.ID == taxonomyHiddenListId)
                                {
                                    mLogger.Info("the list is tax hidden list, do not delete or clear. ");
                                    continue;
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Warn("check if tax hidden list error. " + e);
                                continue;
                            }

                            switch (type)
                            {
                                case DeletedListType.Content:
                                    if (IsDesignList(list))
                                        continue;
                                    else
                                        break;
                                case DeletedListType.Design:
                                    if (!IsDesignList(list))
                                        continue;
                                    else
                                        break;
                                default:
                                    break;
                            }
                            try
                            {
                                list.Delete();
                            }
                            catch (Exception ex1)
                            {
                                mLogger.Warn("An error occurred while deleting list.", ex1.Message);
                                while (list.Items.Count > 0)
                                {
                                    try
                                    {
                                        list.Items[0].Recycle();
                                    }
                                    catch (Exception ex)
                                    {
                                        mLogger.Warn("Empty list [" + list.Title + "] in DeleteDesignLists in AveWeb error:", ex);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            mLogger.Warn("An error occurred while deleting ListItem.", ex2.Message);
                            if (list != null)
                            {
                                while (list.Items.Count > 0)
                                {
                                    try
                                    {
                                        list.Items.Delete(0);
                                    }
                                    catch (Exception ex)
                                    {
                                        mLogger.Warn("Empty this list with DeleteContentLists of AveWeb error:", ex);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Info("Exception in DeleteContentLists of AveWeb Error: " + e.Message + e.StackTrace);
            }
        }

        private static List<string> DesignLists = new List<string>(
                 new string[] { "FormServerTemplates,101",
                               "PublishingImages,851",
                               "Pages,850" ,
                               "SiteAssets,101",
                               "SiteCollectionDocuments,101",
                               "SiteCollectionImages,851",
                               "SitePages,119",
                               "Style Library,101",
                               "WorkflowTasks,107",
                               "AnalyticsReports,101"//delete exception
                             });

        private static bool IsDesignList(IAveList list)
        {
            if (list.Hidden == true || DesignLists.Contains(list.RootFolder.Name + "," + ((int)list.BaseTemplate).ToString()))
                return true;
            else
                return false;
        }

        internal enum DeletedListType
        {
            All,
            Design,
            Content
        }

        private Guid GetTaxonomyHiddenListId(IAveWeb mWeb)
        {
            Guid taxonomyHiddenListId = Guid.Empty;

            try
            {
                if (mWeb.IsRootWeb && mWeb.Properties.ContainsKey("TaxonomyHiddenList"))
                {
                    taxonomyHiddenListId = new Guid(mWeb.Properties["TaxonomyHiddenList"]);
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Getting tax hidden list id error. " + e);
            }
            return taxonomyHiddenListId;
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

        public IAveAppSerializer AppSerializer
        {
            get
            {
                if (m_AppSerializer == null)
                {
                    m_AppSerializer = new AveAppSerializer(this);
                }
                return m_AppSerializer;
            }
        }

        public IAveNavigationSerializer NavigationSerializer
        {
            get
            {
                if (m_NavigationSerializer == null)
                {
                    m_NavigationSerializer = new AveNavigationSerializer(this);
                }
                return m_NavigationSerializer;
            }
        }

        public string GetFileAsString(string url)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 只有真实365支持此方法, 其他抛出异常, 请使用GetListItem(string itemFullUrl, Guid listId, Guid docId)方法代替。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IAveListItem GetListItem(string url)
        {
            if (!this.Site.IsOnlineSite)
            {
                throw new NotImplementedException();
            }
            Guid listId;
            IAveListItem item = null;
            try
            {
                Dictionary<string, object> itemPro = mRequest.GetItemByUrl(this.ID, url, out listId);
                item = itemPro != null ? new AveListItem(mRequest, this, this.Lists[listId], itemPro, false) : null;
            }
            catch (Exception ex)
            {
                //List Item 获取不到的原因很多
                mLogger.Warn("Failed to get list item {0}, error message: {1}", url, ex);
            }
            return item;
        }

        public IAveListItem GetListItem(string itemFullUrl, Guid listId, Guid docId)
        {
            IAveList list = this.Lists[listId];
            IAveListItem item = null;
            try
            {
                item = list.GetItemByUniqueId(docId);
            }
            catch (Exception ex)
            {
                //List Item 获取不到的原因很多
                mLogger.Warn("Failed to get list item {0}, error message: {1}", itemFullUrl, ex);
            }
            return item;
        }

        public IAveLimitedWebPartManager GetLimitedWebPartManager(string fullOrRelativeUrl, AvePersonalizationScope scope)
        {
            throw new NotImplementedException();
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
                fieldInfoList.Add(fieldInfo);
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
                        base.DataCache.PropertiesCache.Add("AssociatedMemberGroup", group);
                    }
                }
                return base.DataCache.GetProperty<IAveGroup>("AssociatedMemberGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociatedMemberGroup", value);
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
                        base.DataCache.PropertiesCache.Add("AssociatedOwnerGroup", group);
                    }
                }
                return base.DataCache.GetProperty<IAveGroup>("AssociatedOwnerGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociatedOwnerGroup", value);
            }
        }

        public IList<IAveGroup> AssociatedGroups
        {
            get { throw new NotImplementedException(); }
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
                    base.DataCache.PropertiesCache["RoleAssignments"] = roleAssignments;
                    return roleAssignments;
                }
                return base.DataCache.GetProperty<IAveRoleAssignmentCollection>("RoleAssignments");
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            InvalidCollections();
            AveClientCacheHandler.CleanSchemaXml(this.CacheHandlerId, this.ID.ToString(), string.Empty);
            DisposePropertyOpenWebs();
        }

        private void DisposePropertyOpenWebs()
        {
            foreach (var web in propertyOpenWebs)
            {
                web.Dispose();
            }
            propertyOpenWebs.Clear();
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

        public void FakeSPContext(bool isPost)
        {

        }

        public void HandleSPContext(Action code)
        {
            if (code != null)
            {
                code();
            }
        }

        public void HandleSPContext(Action code, bool isPost)
        {
            if (code != null)
            {
                code();
            }
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
                    mLogger.Warn("Get web:{0} LanguageCulture failed.Error Message:{1}.", ServerRelativeUrl, ex.ToString());
                }
                return languageCulture;
            }
        }

        public string TaxonomyList
        {
            get { return null; }
        }

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
                    IAveFeature feature = this.Features[AveSP2010FeatureDefinitions.PublishingWeb];
                    if (feature != null)
                    {
                        base.DataCache.PropertiesCache["IsPublish"] = true;
                    }
                    else
                    {
                        base.DataCache.PropertiesCache["IsPublish"] = false;
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
                        base.DataCache.PropertiesCache.Add("AssociatedVisitorGroup", group);
                    }
                }
                return base.DataCache.GetProperty<IAveGroup>("AssociatedVisitorGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociatedVisitorGroup", value);
            }
        }

        public string ApplicationPath
        {
            get { return applicationPath; }
            set { applicationPath = value; }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "spcolor is a part of build-in url.")]
        private void SetThemeInfoFromParentWeb(AveWebSettingInfo webSettingInfo)
        {
            IAveList list = GetFirstUniqueThemeWeb(this.ParentWeb);
            AveCamlQuery query = new AveCamlQuery();
            query.ViewXml = "<View>" +
                                "<Query><Where>" +
                                "<Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq>" +
                                "</Where></Query>" +
                            "</View>";
            query.DatesInUtc = true;
            IAveListItemCollection items = list.GetItems(query);
            if (items.Count == 1)//ADO-51026
            {
                IAveListItem item = items[0];
                webSettingInfo.ThemedTitle = item["Name"] as string;
                string themeUrl = string.IsNullOrEmpty(item["ThemeUrl"] as string) ? string.Empty : item["ThemeUrl"] as string;
                string themeFontUrl = string.IsNullOrEmpty(item["FontSchemeUrl"] as string) ? string.Empty : item["FontSchemeUrl"] as string;
                string themeImageUrl = string.IsNullOrEmpty(item["ImageUrl"] as string) ? string.Empty : item["ImageUrl"] as string;
                if (string.IsNullOrEmpty(themeUrl))
                {
                    string defaultUrl = this.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/_catalogs/theme/15/palette001.spcolor";
                    //if (m_Web.GetFile(defaultUrl).Exists)
                    //{
                    webSettingInfo.ThemedColorUrl = defaultUrl;
                    //}
                }
                else
                {
                    webSettingInfo.ThemedColorUrl = GetThemeUrl(themeUrl);
                }
                if (!string.IsNullOrEmpty(themeFontUrl))
                {
                    webSettingInfo.ThemedFontUrl = GetThemeUrl(themeFontUrl);
                }
                if (!string.IsNullOrEmpty(themeImageUrl))
                {
                    webSettingInfo.ThemedImageUrl = GetThemeUrl(themeImageUrl);
                }
            }
        }

        private IAveList GetFirstUniqueThemeWeb(IAveWeb web)
        {
            if (web.IsRootWeb || !(web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && Convert.ToBoolean(web.AllProperties["__InheritsThemedCssFolderUrl"])))
            {
                return web.GetCatalog(AveListTemplateType.DesignCatalog);
            }
            else
            {
                return GetFirstUniqueThemeWeb(web.ParentWeb);
            }
        }

        private string GetThemeUrl(string combinedUrl)
        {
            return AveUrlUtility.GetServerRelativeUrl(new AveFieldUrlValue(combinedUrl).Url);
        }

        public void RestoreTheme(AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            string siteServerRelativeUrl = string.Empty;
            if (!this.IsRootWeb)
            {
                siteServerRelativeUrl = mSite.ServerRelativeUrl;
            }

            if (mSite.CompatibilityLevel == 15)
            {
                if (!this.IsRootWeb && webSettingInfo.InheritsThemedCssFolderUrl != null && webSettingInfo.InheritsThemedCssFolderUrl.IsAvailable && webSettingInfo.InheritsThemedCssFolderUrl.Value)
                {
                    SetThemeInfoFromParentWeb(webSettingInfo);
                }
                mRequest.RestoreTheme(this.ServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, null);
            }
            else if (webSettingInfo.WebTheme != null)
            {
                if (this.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && Convert.ToBoolean(this.AllProperties["__InheritsThemedCssFolderUrl"].ToString()))  //目的端为yes的情况
                {
                    this.AllProperties["__InheritsThemedCssFolderUrl"] = "False";        //由于api问题 需先将目的端inherit属性置成no
                    this.Update();
                }
                mRequest.RestoreTheme(this.ServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, themedCssFolderUrl);
            }
        }

        public void RestoreMasterPage(AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            string siteServerRelativeUrl = string.Empty;
            if (!this.IsRootWeb)
            {
                siteServerRelativeUrl = mSite.ServerRelativeUrl;
                pageInfo.InheritingTheme = (this.AllProperties["__InheritsThemedCssFolderUrl"] == null) ? false : Convert.ToBoolean(this.AllProperties["__InheritsThemedCssFolderUrl"].ToString());
            }
            else
            {
                /*
                 * Root web inherite property should be false.
                 */
                pageInfo.CInheriting = false;
                pageInfo.MInheriting = false;
                pageInfo.Inheriting = false;
                pageInfo.InheritingTheme = false;
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

        public IEnumerable<CultureInfo> SupportedUICultures
        {
            get
            {
                var supportUICultures = base.DataCache.GetProperty<IEnumerable<int>>("SupportedUILanguageIds");
                if (supportUICultures != null)
                {
                    return supportUICultures.Select(v => new CultureInfo(v));
                }
                return new List<CultureInfo>();
            }
        }

        public void AddSupportedUICulture(CultureInfo cultureInfo)
        {
            var languages = base.DataCache.GetProperty<List<int>>("SupportedUILanguageIds");
            languages.Add(cultureInfo.LCID);
            base.DataCache.AddChangedProperty("SupportedUILanguageIds", languages);
        }

        public IAveList GetListByName(string strListName, bool bThrowException)
        {
            Dictionary<string, object> listProperty = null;
            try
            {
                listProperty = mRequest.GetListByTitle(this.ID, strListName);
                if (listProperty.ContainsKey("BaseType"))
                {
                    switch ((AveBaseType)listProperty["BaseType"])
                    {
                        case AveBaseType.DocumentLibrary:
                            return new AveDocumentLibrary(mRequest, this, listProperty);
                        default:
                            return new AveList(mRequest, this, listProperty);
                    }
                }
            }
            catch (Exception e)
            {
                if (bThrowException)
                {
                    throw new ArgumentException("The list specified by ListName:" + strListName + " does not exist.  " + e.Message);
                }
            }
            return null;
        }

        public IAveWorkflowTemplateCollection WorkflowTemplates
        {
            get
            {
                AveWorkflowTemplateCollection workflowTemplateCollection = null;
                lock (privateLockWorkflowTemplates)
                {
                    if (base.DataCache.IsPropertyNotLoaded("WorkflowTemplates"))
                    {
                        workflowTemplateCollection = new AveWorkflowTemplateCollection(this, null, "web.workflowTemplates");
                        base.DataCache.PropertiesCache["WorkflowTemplates"] = workflowTemplateCollection;
                    }
                    else
                    {
                        workflowTemplateCollection = base.DataCache.GetProperty<AveWorkflowTemplateCollection>("WorkflowTemplates");
                        if (workflowTemplateCollection.IsDirty)
                        {
                            workflowTemplateCollection.UpdateCollectionInternally();
                        }
                    }
                }
                return workflowTemplateCollection;
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
            throw new NotImplementedException();
        }

        public IAveFileCollection Files
        {
            get { throw new NotImplementedException(); }
        }

        public void ReloadWeb()
        {
            this.Update();
            InvalidCollections();
        }

        private void InvalidCollections()
        {
            lock (privateLock)
            {
                if (this.DataCache.IsPropertyAvailable("Lists"))
                {
                    this.DataCache.GetProperty<AveListCollection>("Lists").IsDirty = true;
                }
            }
            lock (privateLockAvailableContentTypes)
            {
                if (this.DataCache.IsPropertyAvailable("AvailableContentTypes"))
                {
                    this.DataCache.GetProperty<AveContentTypeCollection>("AvailableContentTypes").IsCollectionDirty = true;
                }
            }
            lock (privateLockContentTypes)
            {
                if (this.DataCache.IsPropertyAvailable("ContentTypes"))
                {
                    this.DataCache.GetProperty<AveContentTypeCollection>("ContentTypes").IsCollectionDirty = true;
                }
            }
            lock (privateLockFields)
            {
                if (this.DataCache.IsPropertyAvailable("Fields"))
                {
                    this.DataCache.GetProperty<AveFieldCollection>("Fields").IsCollectionDirty = true;
                }
            }
            lock (privateLockWorkflowTemplates)
            {
                if (this.DataCache.IsPropertyAvailable("WorkflowTemplates"))
                {
                    this.DataCache.GetProperty<AveWorkflowTemplateCollection>("WorkflowTemplates").IsDirty = true;
                }
            }
            lock (privateLockWorkflowAssociations)
            {
                if (this.DataCache.IsPropertyAvailable("WorkflowAssociations"))
                {
                    this.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations").IsDirty = true;
                }
            }
        }

        public void ReloadFeatures()
        {
            Dictionary<string, object> featureCollection = mRequest.GetFeatures(this.ServerRelativeUrl, "web.features");
            AveFeatureCollection features = new AveFeatureCollection(this, mRequest, featureCollection, "web.features");
            base.DataCache.PropertiesCache["Features"] = features;
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
                Dictionary<string, object> webThemeProp = mRequest.GetThemeUrlForWeb(this.ServerRelativeUrl, this.Site.CompatibilityLevel);
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
            //2013 Client API contains MasterUrl and CustomMasterUrl property.
            if (!base.DataCache.IsPropertyAvailable("CustomMasterUrl") ||
                !base.DataCache.IsPropertyAvailable("AlternateCssUrl") ||
                !base.DataCache.IsPropertyAvailable("MasterUrl"))
            {
                //此处判断不能去掉，若web没有开过publishing feature，底层无法获取master page属性，且会抛异常。
                //BPOS_D做过处理，走不到这里。
                if (this.Features[AveSP2010FeatureDefinitions.PublishingWeb] != null)
                {
                    Dictionary<string, object> masterPageProp = mRequest.GetMasterPageProperties(this.ServerRelativeUrl);
                    base.DataCache.AddPropertyies(masterPageProp);
                }
            }
        }

        private void GetWebLogoProperties()
        {
            Dictionary<string, object> webLogoProp = mRequest.GetWebLogoProperties(this.ServerRelativeUrl);
            base.DataCache.AddPropertyies(webLogoProp);
        }

        public Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlerts()
        {
            Dictionary<Guid, Dictionary<Guid, Guid>> alertIDMapping = new Dictionary<Guid, Dictionary<Guid, Guid>>();
            foreach (IAveAlert alert in this.Alerts)
            {
                Guid alertOldId = alert.ID;
                Guid listId = alert.ListID;
                if (alert.Properties.ContainsKey("alertoldid"))
                {
                    alertOldId = new Guid(alert.Properties["alertoldid"].ToString());
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
                AveWorkflowAssociationCollection workflowAssociations = null;
                lock (privateLockWorkflowAssociations)
                {
                    if (base.DataCache.IsPropertyNotLoaded("WorkflowAssociations"))
                    {
                        workflowAssociations = new AveWorkflowAssociationCollection(this, null, null, "web.workflow");
                        base.DataCache.PropertiesCache["WorkflowAssociations"] = workflowAssociations;
                    }
                    else
                    {
                        workflowAssociations = base.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations");
                        if (workflowAssociations.IsDirty)
                        {
                            workflowAssociations.UpdateCollectionInternally();
                        }
                    }
                }
                return workflowAssociations;
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
                IAveWebTemplate template = mSite.GetWebTemplates(this.Language)[this.WebTemplate + "#" + this.Configuration];
                return template.Title;
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
                if (base.DataCache.IsPropertyNotLoaded("FirstUniqueTopLinkBarNavigationWeb"))
                {
                    if (this.IsRootWeb)
                    {
                        base.DataCache.PropertiesCache["FirstUniqueTopLinkBarNavigationWeb"] = this;
                    }
                    else
                    {
                        Dictionary<string, object> webProperties = this.mRequest.GetFirstUniqueNavigationWeb(this.ServerRelativeUrl);
                        AveWeb firstUniqueNavigationWeb = new AveWeb(this.mSite, webProperties);
                        propertyOpenWebs.Add(firstUniqueNavigationWeb);
                        base.DataCache.PropertiesCache["FirstUniqueTopLinkBarNavigationWeb"] = firstUniqueNavigationWeb;
                    }
                }
                return base.DataCache.GetProperty<IAveWeb>("FirstUniqueTopLinkBarNavigationWeb");
            }
        }

        public IAveWeb FirstUniqueQuickLaunchNavigationWeb
        {
            get
            {
                #region zyq add
                if (base.DataCache.IsPropertyNotLoaded("FirstUniqueQuickLaunchNavigationWeb"))
                {
                    if (this.IsRootWeb)
                    {
                        base.DataCache.PropertiesCache["FirstUniqueQuickLaunchNavigationWeb"] = this;
                    }
                    else
                    {
                        //var is
                        Dictionary<string, object> webProperties = this.mRequest.GetQuickLaunchFromInheritWeb(this.ServerRelativeUrl);
                        AveWeb firstUniqueNavigationWeb = new AveWeb(this.mSite, webProperties);
                        propertyOpenWebs.Add(firstUniqueNavigationWeb);
                        base.DataCache.PropertiesCache["FirstUniqueQuickLaunchNavigationWeb"] = firstUniqueNavigationWeb;
                    }
                }
                return base.DataCache.GetProperty<IAveWeb>("FirstUniqueQuickLaunchNavigationWeb");
                #region old code
                //if (base.DataCache.IsPropertyNotLoaded("FirstUniqueQuickLanuchNavigationWeb"))
                //{
                //    if (this.IsRootWeb)
                //    {
                //        base.DataCache.PropertiesCache["FirstUniqueQuickLanuchNavigationWeb"] = this;
                //    }
                //    else
                //    {
                //        Dictionary<string, object> webProperties = this.mRequest.GetFirstUniqueNavigationWeb(this.ServerRelativeUrl);
                //        AveWeb firstUniqueNavigationWeb = new AveWeb(mRequest, this.mSite, null, webProperties);
                //        base.DataCache.PropertiesCache["FirstUniqueQuickLanuchNavigationWeb"] = firstUniqueNavigationWeb;
                //    }
                //}
                //return base.DataCache.GetProperty<IAveWeb>("FirstUniqueQuickLanuchNavigationWeb");
                #endregion
                #endregion
                //return this;previous code
            }
        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get
            {
                if (this.HasUniqueRoleAssignments)
                {
                    return new AveSecurableObjectImpl(this.ID, this.RoleAssignments);
                }
                else
                {
                    return this.ParentWeb.SecurableObjectImpl;
                }
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

        public void Recycle()
        {
            throw new NotImplementedException();
        }

        public string GetFormula(string webUrl, string listId, string newFormula, string oldFormula)
        {
            return newFormula;
        }

        #region add for SP2013
        public int SearchVersion
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get
            {
                return null;
            }
        }

        public void ApplyTheme(string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            (mRequest ).ApplyTheme(this.ServerRelativeUrl, colorPaletteUrl, fontSchemeUrl, backgroundImageUrl, shareGenerated);
        }

        public void EnableDisableAbuseReports(bool bEnable)
        {

        }

        public bool HideSiteContentsLink
        {
            get { return base.DataCache.GetProperty<Boolean>("HideSiteContentsLink"); }
            set
            {
                if (HideSiteContentsLink != value)
                {
                    base.DataCache.AddChangedProperty("HideSiteContentsLink", value);
                }
            }
        }
        #endregion

        #region Add to operate Change Log ** We will implement this in SP2013 first **
        public IAveChangeCollection GetChanges()
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            Dictionary<string, object> changeCollectionDic = mRequest.GetWebChangesByQuery(this.ServerRelativeUrl, (query as AveChangeQuery).DataCache.PropertiesCache);
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


        public System.Data.DataTable GetSiteData(IAveSiteDataQuery siteDataQuery)
        {
            throw new NotImplementedException();
        }

        #region Add for SP App
        public bool IsAppWeb
        {
            get { return base.DataCache.GetProperty<Boolean>("IsAppWeb"); }
        }

        public IList<IAveAppInstance> GetAppInstancesByProductId(Guid productId)
        {
            List<IAveAppInstance> appInstances = new List<IAveAppInstance>();
            Dictionary<string, object> appInstanceProperties = (mRequest ).GetAppsByProductId(this.ServerRelativeUrl, productId);
            if (appInstanceProperties != null && appInstanceProperties.Count > 0)
            {
                List<Dictionary<string, object>> appInstanceList = appInstanceProperties[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                foreach (Dictionary<string, object> tempInstance in appInstanceList)
                {
                    AveAppInstance instance = new AveAppInstance(this.Site as AveSite, tempInstance);
                    appInstances.Add(instance);
                }
            }
            return appInstances;
        }

        public Guid AppInstanceId
        {
            //get { throw new NotImplementedException(); }
            get { return base.DataCache.GetProperty<Guid>("AppInstanceId"); }
        }

        public IAveAppInstance LoadAndInstallApp(Stream appPackageStream)
        {
            throw new NotImplementedException();
        }

        public IAveAppInstance GetAppInstanceById(Guid appInstanceId)
        {
            IAveAppInstance appInstance = null;
            Dictionary<string, object> appProperties = (mRequest ).GetWebAppById(this.ServerRelativeUrl, appInstanceId);
            if (appProperties != null && appProperties.Count > 0)
            {
                appInstance = new AveAppInstance(this.Site as AveSite, appProperties);
            }
            return appInstance;
        }

        public void UpgradeAppByProductId(Guid productId)
        {
            throw new NotImplementedException();
        }

        public IAveAppInstance LoadAndInstallApp(Stream appPackageStream, int appSource, string assetId, string contentMarket)
        {
            throw new NotImplementedException();
        }

        #endregion


        public string GetWebRelativeUrlFromUrl(string strUrl)
        {
            throw new NotImplementedException();
        }

        public IAveRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RecycleBin"))
                {
                    Dictionary<string, object> recycleBinItemCollection = mRequest.GetRecycleBin(this.ServerRelativeUrl);
                    AveRecycleBinItemCollection recycleBinItems = new AveRecycleBinItemCollection(mRequest, this.Site, this, recycleBinItemCollection);
                    base.DataCache.PropertiesCache.Add("RecycleBin", recycleBinItems);
                }
                return base.DataCache.GetProperty<IAveRecycleBinItemCollection>("RecycleBin");
            }
        }

        #region User Resource:need to confirm if support
        public IAveUserResource DescriptionResource
        {
            get
            {
                if(!Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockDescriptionResource)
                {
                    if (mDescriptionResource == null)
                    {
                        mDescriptionResource = new AveWebUserResource(this, AveUserResourceConstants.DESCRIPTION_RESOUCE, this.DataCache);
                    }
                    return mDescriptionResource;
                }
            }
        }

        public IAveUserResource TitleResource
        {
            get
            {
                if (!Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockTitleResource)
                {
                    if (mTitleResource == null)
                    {
                        mTitleResource = new AveWebUserResource(this, AveUserResourceConstants.TITLE_RESOUCE, this.DataCache);
                    }
                    return mTitleResource;
                }
            }
        }
        #endregion


        public IAvePublishingWeb GetPublishingWeb
        {
            get
            {
                if (this.IsPublish)
                {
                    return new AvePublishingWeb(mSite, this, mRequest.GetPublishingWeb(ServerRelativeUrl));
                }

                return null;
            }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return null;
            }
        }

        public Guid VariationLabelListId
        {
            get
            {
                if (variationLabelListId.HasValue) return variationLabelListId.Value;
                try
                {
                    variationLabelListId = IsRootWeb && AllProperties.ContainsKey("_VarLabelsListId")
                                                         ? new Guid(AllProperties["_VarLabelsListId"].ToString())
                                                         : Guid.Empty;
                }
                catch (Exception e)
                {
                    mLogger.Log(AveLogLevel.WARN, "An error occurred while checking whether the list is variation labels, exception:{0}.", e);
                    variationLabelListId = Guid.Empty;
                }
                return variationLabelListId.Value;
            }
        }

        public Guid RelationshipsListId
        {
            get
            {
                if (relationshipsListId.HasValue) return relationshipsListId.Value;
                try
                {
                    relationshipsListId = IsRootWeb && AllProperties.ContainsKey("_VarRelationshipsListId")
                                                      ? new Guid(AllProperties["_VarRelationshipsListId"].ToString())
                                                      : Guid.Empty;
                }
                catch (Exception e)
                {
                    mLogger.Log(AveLogLevel.WARN, "An error occurred while checking whether the list is relationships list, exception:{0}.", e);
                    relationshipsListId = Guid.Empty;
                }
                return relationshipsListId.Value;
            }
        }

        #region No use code
        //private Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string siteServerRelativeUrl, string listName, bool overWrite, bool isRetry)
        //{
        //    try
        //    {
        //        string tenant = GetTenant(this.mSite.Url);
        //        return (mRequest ).PublishNintexWorkflow(stream, publishName, tenant, siteServerRelativeUrl, listName, overWrite);
        //    }
        //    catch (Exception e)
        //    {
        //        var reponseStatusDescription = GetExceptionStatusDescription(e);
        //        if (!isRetry && !string.IsNullOrEmpty(listName) && !string.IsNullOrEmpty(reponseStatusDescription))
        //        {
        //            /*
        //                该问题重现步骤：目的端已经存在同名的workflow，先把目的端的list删除，再还原同名list，就会产生该问题，
        //                具体原因不太清楚，怀疑nintex有cache
        //            */
        //            var fieldName = GetFieldName(reponseStatusDescription);
        //            if (!string.IsNullOrEmpty(fieldName))
        //            {
        //                mLogger.Info("Try to add field which the workflow needed. Field name: {0}.", fieldName);
        //                EnsureWorkflowColumn(this.Lists[listName], fieldName);
        //                return PublishNintexWorkflow(stream, publishName, siteServerRelativeUrl, listName, overWrite, true);
        //            }
        //        }
        //        mLogger.Error("An error occurred while publish nintex workflow, workflow name: {0}, web url: {1}, list name: {2}, response status description:{3}, error: {4}.", publishName, siteServerRelativeUrl, listName, reponseStatusDescription, e);
        //        throw;
        //    }
        //}
        #endregion

        public string ImportNintexWorkflow(System.IO.Stream stream, string publishName, string listTitle, Guid listId, bool overWrite)
        {
            if (this.mSite.IsOnlineSite)
            {
                try
                {
                    return (mRequest ).ImportNintexWorkflow(stream, publishName, this.Url, listTitle, listId, overWrite);
                }
                catch (Exception e)
                {
                    var reponseStatusDescription = GetExceptionStatusDescription(e);
                    mLogger.Error("An error occurred while import nintex workflow, web url: {0}, list name: {1}, response status description:{2}, error: {3}.", this.Url, listTitle, reponseStatusDescription, e);
                    throw;
                }
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }

        public string ConvertNintexFormJsonObjectToXml(string formJsonData, string fileName)
        {
            if (this.mSite.IsOnlineSite)
            {
                return (mRequest ).ConvertNintexFormJsonObjectToXml(this.Url, formJsonData, fileName);
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }

        public Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string listName, Guid listId)
        {
            if (this.mSite.IsOnlineSite)
            {
                return (mRequest ).PublishNintexWorkflow(stream, publishName, this.Url, listName, listId);
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }
        public Guid PublishNintexWorkflow(Guid workflowDefinitionId)
        {
            if (this.mSite.IsOnlineSite)
            {
                return (mRequest ).PublishNintexWorkflow(this.Url, workflowDefinitionId);
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }
        /// <summary>
        /// Only for Records module. Deploy app.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="restoreMode"></param>
        /// <returns></returns>
        public IAveAppInstance DeployApp(Guid productId, Wrapper.Restore.AveRestoreMode restoreMode)
        {
            var restoreOption = new AvePoint.Wrapper.Restore.AveRestoreOption();
            restoreOption.mAveRestoreMode = restoreMode;
            this.AppSerializer.SetRestoreOption(restoreOption);
            var appInfo = new AveAppPackageInfo
            {
                ProductId = productId
            };
            return this.AppSerializer.SetObjectData(appInfo);
        }

        [SuppressMessage("FxCopCustomRules", "C100013:CheckExistingExceptionHandlingBlocks", Justification = "Do not need to handle exception")]
        private void EnsureWorkflowColumn(IAveList parentList, string mName)
        {
            try
            {
                parentList.Fields.GetField(mName);
            }
            catch
            {
                XElement fieldElement = new XElement("Field", null);
                fieldElement.SetAttributeValue("DisplayName", mName);
                //fieldElement.SetAttributeValue("Name", mName);
                fieldElement.SetAttributeValue("Type", "URL");
                fieldElement.SetAttributeValue("Required", "FALSE");
                fieldElement.SetAttributeValue("SourceID", parentList.ID.ToString());
                fieldElement.SetAttributeValue("ShowInEditForm", "FALSE");
                fieldElement.SetAttributeValue("ShowInNewForm", "FALSE");

                var field = parentList.Fields.AddFieldAsXml(fieldElement.ToString());
            }
        }

        /// <summary>
        /// reponseStatusDescription example： 
        /// Column 'BBBB' does not exist. It may have been deleted by another user.  /sites/XLuoNinTexTest/104
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private string GetFieldName(string reponseStatusDescription)
        {
            var startIndex = reponseStatusDescription.IndexOf("Column '", StringComparison.OrdinalIgnoreCase) + 8;
            var endIndex = reponseStatusDescription.IndexOf("' does not exist.", StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 8 && endIndex > 0)
            {
                return reponseStatusDescription.Substring(startIndex, endIndex - startIndex);
            }
            return string.Empty;
        }
        private string GetExceptionStatusDescription(Exception exception)
        {
            WebException webException = exception.InnerException != null ? exception.InnerException as WebException : null;
            HttpWebResponse webResponse = webException != null ? webException.Response as HttpWebResponse : null;
            if (webResponse == null)
            {
                return string.Empty;
            }
            return webResponse.StatusDescription;
        }

        private string GetTenant(string siteUrl)
        {
            return siteUrl.Substring("https://".Length, siteUrl.IndexOf('.') - "https://".Length);
        }

        public IAveFeatureDefinitionCollection GetAllFeatureDefinitions()
        {
            Dictionary<string, object> definitionCollection = new Dictionary<string, object>();
            definitionCollection = mRequest.GetAllFeatureDefinitions(this.Url, "web.features");
            AveFeatureDefinitionCollection definitions = new AveFeatureDefinitionCollection(this, mRequest, definitionCollection, "web.features");
            return definitions;
        }

        private class SPWebCollectionProvider : AveWebCollection.ISPWebCollectionProvider
        {
            public AveWeb Web { get; private set; }
            public IAveRequest Request { get; private set; }

            public SPWebCollectionProvider(AveWeb parentWeb, IAveRequest request)
            {
                Web = parentWeb;
                Request = request;
            }

            public IEnumerable<Dictionary<string, object>> GetWebsData()
            {
                var webId = this.Web.ID;
                return Request.GetSubWebsBasicInfo(this.Web.Site.Url, webId).Select(allWeb => allWeb.Value);
            }

            public Dictionary<string, object> Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere)
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                if (strWebUrl.Contains("/"))
                {
                    strWebUrl = strWebUrl.Substring(strWebUrl.TrimEnd('/').LastIndexOf('/') + 1);
                }
                if (!string.IsNullOrEmpty(strWebTemplate) && (strWebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase) || strWebTemplate.StartsWith("SPSMSITEHOST", StringComparison.OrdinalIgnoreCase)))
                {
                    var masterPageInfo = this.Request.GetRootWebMasterPageInfo();
                    webProperties = this.Request.AddWeb(this.Web.ServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);
                    string mySiteWebServerRelativeUrl = webProperties.ContainsKey("ServerRelativeUrl") ? (webProperties["ServerRelativeUrl"]).ToString() : string.Empty;
                    this.Request.SetRootWebAndMySiteWebMasterPageInfo(mySiteWebServerRelativeUrl, masterPageInfo);
                    if (webProperties.ContainsKey("MasterUrl") && !string.IsNullOrEmpty(masterPageInfo.MPageUrl))
                    {
                        webProperties["MasterUrl"] = masterPageInfo.MPageUrl;
                    }
                    if (webProperties.ContainsKey("CustomMasterUrl") && !string.IsNullOrEmpty(masterPageInfo.CPageUrl))
                    {
                        webProperties["CustomMasterUrl"] = masterPageInfo.CPageUrl;
                    }
                }
                else
                {
                    webProperties = this.Request.AddWeb(this.Web.ServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);
                }
                return webProperties;
            }

            public IAveWeb OpenWeb(string name)
            {
                return new AveWeb(this.Web.Site as AveSite, AveUrlUtility.CombineUrl(this.Web.ServerRelativeUrl, name));
            }
        }
    }
}
