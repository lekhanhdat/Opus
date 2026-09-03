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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Linq;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;
using SPDisposeCheck;
using Microsoft.SharePoint.Workflow;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using Microsoft.SharePoint.Administration;
using System.Threading;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server19
{
    public class AveWeb : AveSecurableObject, IAveWeb
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveWeb));
        private const string mWeb_Request_Member = "Request";
        private const string mWeb_GetServerRelativeUrlFromUrl_Member = "GetServerRelativeUrlFromUrl";
        private const string mWeb_GetItem_Member = "GetItem";
        private SPWeb mWeb;
        private AveSite mSite;
        private AveContentTypeCollection mContentTypes;
        private AveFeatureCollection mFeatures;
        private AveFieldCollection mFields;
        private AveGroupCollection mGroups;
        private AveGroupCollection siteGroups;
        private AveRoleDefinitionCollection mRoleDefinitions;
        private AveFieldCollection mAvailableFields;
        private AveContentTypeCollection mAvailableContentTypes;
        private AveListCollection mLists;
        private AveNavigation mNavigation;
        private AveUserCollection mUsers;
        private AveUserCollection mSiteAdministrators;
        private AveRegionalSettings mRegionalSettigns;
        private AveFolder mRootFolder;
        private AveWebCollection mWebs;
        private AveUserCollection mAllUsers;
        private AveGroup mAssociatedMemberGroup;
        private AveGroup mAssociatedOwnerGroup;
        private AveGroup mAssociatedVisitorGroup;
        private AveUser mAuthor;
        private AveWeb mFirstUniqueRoleDefinitionWeb;
        private AveWeb mParentWeb;
        private AveUser mCurrentUser;
        private AveList mSiteUserInfoList;
        private AveAlertCollection mAlerts;
        private AveListTemplateCollection mListTemplates;
        private AveEventReceiverDefinitionCollection mEventReceivers;
        private AvePropertyBag mProperties;
        private AveCommonRequest mRequest;
        private AveViewStyleCollection mViewStyleCollection;
        private string mTaxonomyList;
        private AveUserCollection mSiteUsers;
        private AveAudit mAudit;
        private AveNavigationSerializer mNavigationSerializer;
        private AveAppSerializer mAppSerializer;
        private AveRolesSerializer m_RolesSerializer;
        private AveWebSerializer m_WebSerializer;
        private AveWebSettingSerializer m_WebSettingSerializer;
        private AveWebUsersSerializer m_WebUsersSerializer;
        private AveGroupsSerializer m_GroupsSerializer;
        private AveRoleAssignmentsSerializer m_RoleAssignmentsSerializer;
        private AveFeatureSerializer m_FeatureSerializer;
        private Guid m_Id;
        private AveDocTemplateCollection mDocTemplates;
        private AveFileCollection mFiles;
        private AveWorkflowAssociationCollection mWorkflowAssociations;
        private AveWorkflowCollection mWorkflows;
        private ObjectModelWrapperList<IAveGroup, SPGroup> associatedGroups;
        private Guid? variationLabelListId = null;
        private Guid? relationshipsListId = null;

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                if (mWorkflowAssociations == null)
                {
                    mWorkflowAssociations = new AveWorkflowAssociationCollection(this, mWeb.WorkflowAssociations);
                }
                return mWorkflowAssociations;
            }
        }


        public AveWeb(AveSite site, SPWeb web)
            : base(web)
        {
            mSite = site;
            mWeb = web;
            SetAllowUnsafeUpdate();
        }

        #region IAveWeb Members

        internal SPWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        public IAveContentTypeCollection ContentTypes
        {
            get
            {
                if (mContentTypes == null)
                {
                    mContentTypes = new AveContentTypeCollection(this, mWeb.ContentTypes);
                }
                return mContentTypes;
            }
        }

        public string Description
        {
            get
            {
                return mWeb.Description;
            }
            set
            {
                mWeb.Description = value;
            }
        }

        public IAveFeatureCollection Features
        {
            get
            {
                if (mFeatures == null)
                {
                    mFeatures = new AveFeatureCollection(mWeb.Features, this);
                }
                return mFeatures;
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                if (mFields == null || mFields.IsDirty)
                {
                    mFields = new AveFieldCollection(this, mWeb.Fields);
                }
                return mFields;
            }
        }

        public IAveGroupCollection SiteGroups
        {
            get
            {
                if (siteGroups == null)
                {
                    siteGroups = new AveGroupCollection(this, mWeb.SiteGroups);
                }
                return siteGroups;
            }
        }

        public bool IsRootWeb
        {
            get { return mWeb.IsRootWeb; }
        }

        public uint Language
        {
            get { return mWeb.Language; }
        }

        public int WorkingLanguage
        {
            get { return (int)mWeb.Language; }
        }

        public IAveListCollection Lists
        {
            get
            {
                if (mLists == null)
                {
                    mLists = new AveListCollection(this, mWeb.Lists);
                }
                return mLists;
            }
        }

        public IAveListCollection BrowserLists
        {
            get
            {
                if (mLists == null)
                {
                    mLists = new AveListCollection(this, mWeb.Lists);
                }
                return mLists;
            }
        }

        public string Name
        {
            get
            {
                return mWeb.Name;
            }
            set
            {
                mWeb.Name = value;
            }
        }

        public IAveNavigation Navigation
        {
            get
            {
                if (mNavigation == null)
                {
                    mNavigation = new AveNavigation(mWeb.Navigation);
                }
                return mNavigation;
            }
        }

        public IAveUserCollection SiteUsers
        {
            get
            {
                //if (mSiteUsers == null)
                //{
                mSiteUsers = new AveUserCollection(this, mWeb.SiteUsers);
                //}
                return mSiteUsers;
            }
        }

        public bool QuickLaunchEnabled
        {
            get
            {
                return mWeb.QuickLaunchEnabled;
            }
            set
            {
                mWeb.QuickLaunchEnabled = value;
            }
        }

        public IAveRegionalSettings RegionalSettings
        {
            get
            {
                if (mRegionalSettigns == null)
                {
                    mRegionalSettigns = new AveRegionalSettings(mWeb.RegionalSettings);
                }
                return mRegionalSettigns;
            }
        }

        public IAveRoleDefinitionCollection RoleDefinitions
        {
            get
            {
                if (mRoleDefinitions == null)
                {
                    mRoleDefinitions = new AveRoleDefinitionCollection(this, mWeb.RoleDefinitions);
                }
                return mRoleDefinitions;
            }
        }

        public IAveFolder RootFolder
        {
            get
            {
                if (mRootFolder == null)
                {
                    mRootFolder = new AveFolder(this, mWeb.RootFolder);
                }
                return mRootFolder;
            }
        }

        public string ServerRelativeUrl
        {
            get { return mWeb.ServerRelativeUrl; }
            set { mWeb.ServerRelativeUrl = value; }
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
                return mWeb.SyndicationEnabled;
            }
            set
            {
                mWeb.SyndicationEnabled = value;
            }
        }

        public string Title
        {
            get
            {
                return mWeb.Title;
            }
            set
            {
                mWeb.TitleResource.SetValueForUICulture(mWeb.Locale, value);
                mWeb.Title = value;
            }
        }

        public bool TreeViewEnabled
        {
            get
            {
                return mWeb.TreeViewEnabled;
            }
            set
            {
                mWeb.TreeViewEnabled = value;
            }
        }

        public IAveWebCollection Webs
        {
            get
            {
                if (mWebs == null)
                {
                    mWebs = new AveWebCollection(mSite, mWeb.Webs);
                }
                return mWebs;
            }
        }

        public IAveCommonRequest Request
        {
            get
            {
                if (mRequest == null)
                {
                    object request = AveAssemblyUtility.GetPropertyValue(mWeb, mWeb_Request_Member);
                    if (request == null)
                    {
                        return null;
                    }
                    mRequest = new AveCommonRequest(request);
                }
                return mRequest;
            }
        }

        public IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid, bool doIncludeCrossLanguage)
        {
            return new AveWebTemplateCollection(mWeb.GetAvailableWebTemplates(lcid, doIncludeCrossLanguage));
        }

        public IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid)
        {
            return this.GetAvailableWebTemplates(lcid, true);
        }

        public IAveFolder GetFolder(string serverRelativeUrl)
        {
            return new AveFolder(this, mWeb.GetFolder(SPResourcePath.FromDecodedUrl(serverRelativeUrl)));
        }

        public IAveFile GetFile(string serverRelativeUrl)
        {
            return new AveFile(this, mWeb.GetFile(SPResourcePath.FromDecodedUrl(serverRelativeUrl)));
        }

        public IAveFile GetFile(string serverRelativeUrl, bool needProperties)
        {
            return new AveFile(this, mWeb.GetFile(SPResourcePath.FromDecodedUrl(serverRelativeUrl)));
        }

        public IAveFile GetFile(Guid fileId, string serverRelativeUrl)
        {
            return this.GetFile(fileId);
        }

        public IAveUser EnsureUser(string logonName)
        {
            return new AveUser(this, mWeb.EnsureUser(logonName));
        }

        public void Update()
        {
            mWeb.Update();
        }

        public bool HasUniqueRoleDefinitions
        {
            get { return mWeb.HasUniqueRoleDefinitions; }
        }

        public string WebTemplate
        {
            get { return mWeb.WebTemplate; }
        }

        public IAveUserCollection AllUsers
        {
            get
            {
                if (mAllUsers == null)
                {
                    mAllUsers = new AveUserCollection(this, mWeb.AllUsers);
                }
                return mAllUsers;
            }
        }

        public string Url
        {
            get { return mWeb.Url; }
        }

        public IAveGroup AssociatedMemberGroup
        {
            get
            {
                if (mAssociatedMemberGroup == null)
                {
                    SPGroup group = mWeb.AssociatedMemberGroup;
                    if (group != null)
                    {
                        mAssociatedMemberGroup = new AveGroup(this, group);
                    }
                }
                return mAssociatedMemberGroup;
            }
            set
            {
                mAssociatedMemberGroup = value as AveGroup;
                if (mAssociatedMemberGroup != null)
                {
                    mWeb.AssociatedMemberGroup = mAssociatedMemberGroup.Group;
                }
                else
                {
                    mWeb.AssociatedMemberGroup = null;
                }
            }
        }

        public IAveGroup AssociatedOwnerGroup
        {
            get
            {
                if (mAssociatedOwnerGroup == null)
                {
                    SPGroup group = mWeb.AssociatedOwnerGroup;
                    if (group != null)
                    {
                        mAssociatedOwnerGroup = new AveGroup(this, group);
                    }
                }
                return mAssociatedOwnerGroup;
            }
            set
            {
                mAssociatedOwnerGroup = value as AveGroup;
                if (mAssociatedOwnerGroup != null)
                {
                    mWeb.AssociatedOwnerGroup = mAssociatedOwnerGroup.Group;
                }
                else
                {
                    mWeb.AssociatedOwnerGroup = null;
                }
            }
        }

        public IAveGroup AssociatedVisitorGroup
        {
            get
            {
                if (mAssociatedVisitorGroup == null)
                {
                    SPGroup group = mWeb.AssociatedVisitorGroup;
                    if (group != null)
                    {
                        mAssociatedVisitorGroup = new AveGroup(this, group);
                    }
                }
                return mAssociatedVisitorGroup;
            }
            set
            {
                mAssociatedVisitorGroup = value as AveGroup;
                if (mAssociatedVisitorGroup != null)
                {
                    mWeb.AssociatedVisitorGroup = mAssociatedVisitorGroup.Group;
                }
                else
                {
                    mWeb.AssociatedVisitorGroup = null;
                }
            }
        }

        public IList<IAveGroup> AssociatedGroups
        {
            get
            {
                if (this.associatedGroups == null)
                {
                    this.associatedGroups = mWeb.AssociatedGroups == null ? null :
                         new ObjectModelWrapperList<IAveGroup, SPGroup>(mWeb.AssociatedGroups,
                         spg => spg == null ? null : new AveGroup(this, spg),
                         aveg => aveg as AveGroup == null ? null : (aveg as AveGroup).Group);
                }
                return this.associatedGroups;
            }
        }

        public IAveFile GetFile(Guid uniqueId)
        {
            return new AveFile(this, mWeb.GetFile(uniqueId));
        }

        public IAveFile GetCheckoutFile(string url)
        {
            int userId = -1;

            if (mSite.QueryService.IsCheckOutFile(mSite.ID, url, ref userId) && userId != mWeb.CurrentUser.ID)
            {
                return mSite.GetCheckoutWeb(mSite.ID, this, this.SiteUsers.GetByID(userId), Guid.Empty).GetFile(url);//TODOLMM
            }
            else
            {
                return this.GetFile(url);
            }
        }

        public string AlternateCssUrl
        {
            get
            {
                return mWeb.AlternateCssUrl;
            }
            set
            {
                mWeb.AlternateCssUrl = value;
            }
        }

        public IAveAlertCollection Alerts
        {
            get
            {
                if (mAlerts == null)
                {
                    mAlerts = new AveAlertCollection(this, mWeb.Alerts);
                }
                return mAlerts;
            }
        }

        public bool AllowUnsafeUpdates
        {
            get
            {
                return mWeb.AllowUnsafeUpdates;
            }
            set
            {
                mWeb.AllowUnsafeUpdates = value;
            }
        }

        public bool AllowAutomaticASPXPageIndexing
        {
            get
            {
                return mWeb.AllowAutomaticASPXPageIndexing;
            }
            set
            {
                mWeb.AllowAutomaticASPXPageIndexing = value;
            }
        }

        public Hashtable AllProperties
        {
            get
            {
                return mWeb.AllProperties;
            }
        }

        public AveWebASPXPageIndexMode ASPXPageIndexMode
        {
            get
            {
                return (AveWebASPXPageIndexMode)mWeb.ASPXPageIndexMode;
            }
            set
            {
                mWeb.ASPXPageIndexMode = (WebASPXPageIndexMode)value;
            }
        }

        public IAveUser Author
        {
            get
            {
                if (mAuthor == null)
                {
                    mAuthor = new AveUser(this, mWeb.Author);
                }
                return mAuthor;
            }
            set
            {
                //经过测试，如果User是null，Author属性更新不进去。 所以当User是Null，mAuthor属性保持不变。
                var tempAuthor = value as AveUser;
                if (tempAuthor != null && tempAuthor.User != null)
                {
                    mWeb.Author = tempAuthor.User;
                    mAuthor = tempAuthor;
                }
            }
        }

        public IAveFieldCollection AvailableFields
        {
            get
            {
                if (mAvailableFields == null || mAvailableFields.IsDirty)
                {
                    mAvailableFields = new AveFieldCollection(this, mWeb.AvailableFields);
                }
                return mAvailableFields;
            }
        }

        public IAveContentTypeCollection AvailableContentTypes
        {
            get
            {
                if (mAvailableContentTypes == null || mAvailableContentTypes.IsDirty)
                {
                    mAvailableContentTypes = new AveContentTypeCollection(this, mWeb.AvailableContentTypes);
                }
                return mAvailableContentTypes;
            }
        }

        public IAveWeb FirstUniqueRoleDefinitionWeb
        {
            get
            {
                if (mFirstUniqueRoleDefinitionWeb == null)
                {
                    //mFirstUniqueRoleDefinitionWeb = new AveWeb(mSite, mWeb.FirstUniqueAncestorWeb);
                    mFirstUniqueRoleDefinitionWeb = new AveWeb(mSite, mWeb.FirstUniqueRoleDefinitionWeb);
                }
                return mFirstUniqueRoleDefinitionWeb;
            }
        }

        public IAveGroupCollection Groups
        {
            get
            {
                if (mGroups == null)
                {
                    mGroups = new AveGroupCollection(this, mWeb.Groups);
                }
                return mGroups;
            }
        }

        public CultureInfo Locale
        {
            get { return mWeb.Locale; }
        }

        public string MasterUrl
        {
            get
            {
                return mWeb.MasterUrl;
            }
            set
            {
                mWeb.MasterUrl = value;
            }
        }

        public string CustomMasterUrl
        {
            get
            {
                return mWeb.CustomMasterUrl;
            }
            set
            {
                mWeb.CustomMasterUrl = value;
            }
        }

        public bool NoCrawl
        {
            get
            {
                return mWeb.NoCrawl;
            }
            set
            {
                mWeb.NoCrawl = value;
            }
        }

        public int WebTemplateId
        {
            get { return mWeb.WebTemplateId; }
        }

        string template;
        public string Template
        {
            get
            {
                if (template == null)
                {
                    if (AveWebDatabaseSite.IsWebDatabaseWeb(mWeb))
                    {
                        template = AveWebDatabaseSite.TryGetACCSRVWebTemplate(mWeb);
                    }
                    else
                    {
                        template = mWeb.WebTemplate + "#" + mWeb.Configuration;
                    }
                }
                return template;
            }
        }

        public IAveWeb ParentWeb
        {
            [SPDisposeCheckIgnore(SPDisposeCheckID._170, "This Web will be Disposed by AveWeb")]
            get
            {
                if (mParentWeb == null)
                {
                    SPWeb web = mWeb.ParentWeb;
                    if (web != null)
                    {
                        mParentWeb = new AveWeb(mSite, web);
                    }
                }
                return mParentWeb;
            }
        }

        public bool ParserEnabled
        {
            get
            {
                return mWeb.ParserEnabled;
            }
            set
            {
                mWeb.ParserEnabled = value;
            }
        }

        public bool PresenceEnabled
        {
            get
            {
                return mWeb.PresenceEnabled;
            }
            set
            {
                mWeb.PresenceEnabled = value;
            }
        }

        public Guid ID
        {
            get
            {
                if (m_Id == Guid.Empty)
                {
                    m_Id = mWeb.ID;
                }
                return m_Id;
            }
        }

        public short Configuration
        {
            get { return mWeb.Configuration; }
        }

        public string Theme
        {
            get { return mWeb.Theme; }
        }

        public bool UIVersionConfigurationEnabled
        {
            get
            {
                return mWeb.UIVersionConfigurationEnabled;
            }
            set
            {
                mWeb.UIVersionConfigurationEnabled = value;
            }
        }

        public string SiteLogoUrl
        {
            get
            {
                return mWeb.SiteLogoUrl;
            }
            set
            {
                mWeb.SiteLogoUrl = value;
            }
        }

        public string SiteLogoDescription
        {
            get
            {
                return mWeb.SiteLogoDescription;
            }
            set
            {
                mWeb.SiteLogoDescription = value;
            }
        }

        public int UIVersion
        {
            get
            {
                return mWeb.UIVersion;
            }
            set
            {
                mWeb.UIVersion = value;
            }
        }

        public IAveUserCollection Users
        {
            get
            {
                if (mUsers == null)
                {
                    mUsers = new AveUserCollection(this, mWeb.Users);
                }
                return mUsers;
            }
        }

        public bool IsMultilingual
        {
            get
            {
                return mWeb.IsMultilingual;
            }
            set
            {
                mWeb.IsMultilingual = value;
            }
        }

        public bool OverwriteTranslationsOnChange
        {
            get
            {
                return mWeb.OverwriteTranslationsOnChange;
            }
            set
            {
                mWeb.OverwriteTranslationsOnChange = value;
            }
        }

        public IAveUserCollection SiteAdministrators
        {
            get
            {
                if (mSiteAdministrators == null)
                {
                    mSiteAdministrators = new AveUserCollection(this, mWeb.SiteAdministrators);
                }
                return mSiteAdministrators;
            }
        }

        public string ThemedCssFolderUrl
        {
            get
            {
                return mWeb.ThemedCssFolderUrl;
            }
            set
            {
                mWeb.ThemedCssFolderUrl = value;
            }
        }

        public string ThemeCssUrl
        {
            get { return mWeb.ThemeCssUrl; }
        }

        public AveWebAnonymousState AnonymousState
        {
            get
            {
                return (AveWebAnonymousState)mWeb.AnonymousState;
            }
            set
            {
                mWeb.AnonymousState = (SPWeb.WebAnonymousState)value;
            }
        }

        public bool ExcludeFromOfflineClient
        {
            get
            {
                return mWeb.ExcludeFromOfflineClient;
            }
            set
            {
                mWeb.ExcludeFromOfflineClient = value;
            }
        }

        public IAveList SiteUserInfoList
        {
            get
            {
                if (mSiteUserInfoList == null)
                {
                    SPList list = mWeb.SiteUserInfoList;
                    if (list != null)
                    {
                        mSiteUserInfoList = (this.Lists as AveListCollection).CreateListByType(list);
                    }
                }
                return mSiteUserInfoList;
            }
        }

        public IAveListTemplateCollection ListTemplates
        {
            get
            {
                if (mListTemplates == null)
                {
                    mListTemplates = new AveListTemplateCollection(this, mWeb.ListTemplates);
                }
                return mListTemplates;
            }
        }

        public IAveUser CurrentUser
        {
            get
            {
                if (mCurrentUser == null)
                {
                    SPUser user = mWeb.CurrentUser;
                    if (user != null)
                    {
                        mCurrentUser = new AveUser(this, user);
                    }
                }
                return mCurrentUser;
            }
        }

        public IAvePropertyBag Properties
        {
            get
            {
                if (mProperties == null)
                {
                    mProperties = new AvePropertyBag(mWeb.Properties);
                }
                return mProperties;
            }
        }

        public bool Exists
        {
            get
            {
                return mWeb.Exists;
            }
        }

        public void ApplyTheme(string theme)
        {
            mWeb.ApplyTheme(theme);
        }

        public void ApplyWebTemplate(IAveWebTemplate webTemplate)
        {
            if (webTemplate == null)
            {
                throw new ArgumentNullException("webTemplate");
            }
            mWeb.ApplyWebTemplate(webTemplate.Name);
        }

        public bool Provisioned
        {
            get { return mWeb.Provisioned; }
        }

        public void Close()
        {
            mWeb.Close();
        }

        public IAveFolder GetFolder(Guid uniqueId)
        {
            return new AveFolder(this, mWeb.GetFolder(uniqueId));
        }

        public IAveList GetList(string strUrl)
        {
            SPList list = mWeb.GetList(SPResourcePath.FromDecodedUrl(strUrl));
            if (list == null)
            {
                return null;
            }
            return this.Lists[list.ID];
        }

        public IAveList GetList(Guid listId)
        {
            return this.Lists.GetList(listId, true);
        }

        public void Delete()
        {
            mWeb.Delete();
        }

        public IAveList GetCatalog(AveListTemplateType typeCatalog)
        {
            SPListTemplateType type = (SPListTemplateType)Enum.Parse(typeof(SPListTemplateType), typeCatalog.ToString());
            SPList list = mWeb.GetCatalog(type);
            if (list == null)
            {
                return null;
            }
            return (this.Lists as AveListCollection).CreateListByType(list);
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (mEventReceivers == null)
                {
                    mEventReceivers = new AveEventReceiverDefinitionCollection(mWeb.EventReceivers);
                }
                return mEventReceivers;
            }
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "This Web will be Disposed by AveWeb")]
        public IAveLimitedWebPartManager GetLimitedWebPartManager(string fullOrRelativeUrl, AvePersonalizationScope scope)
        {
            SPLimitedWebPartManager limitedWebPartManager = mWeb.GetLimitedWebPartManager(fullOrRelativeUrl, (PersonalizationScope)scope);
            if (limitedWebPartManager == null)
            {
                return null;
            }
            return new AveLimitedWebPartManager(this, limitedWebPartManager);
        }

        public IAveWebPartCollection GetWebPartCollection(string fullOrRelativeUrl, AveStorage storage)
        {
            return null;
            //SPWebPartCollection webParts = mWeb.GetWebPartCollection(fullOrRelativeUrl, (Storage)storage);
            //if (webParts == null)
            //{
            //    return null;
            //}
            //return new AveWebPartCollection(webParts);
        }

        public bool AllowRssFeeds
        {
            get { return mWeb.AllowRssFeeds; }
        }

        public string TaxonomyList
        {
            get
            {
                if (string.IsNullOrEmpty(mTaxonomyList))
                {
                    GetTaxonomyList();
                }
                return mTaxonomyList;
            }
        }

        private void GetTaxonomyList()
        {
            if (mWeb.IsRootWeb && mWeb.Properties.ContainsKey("TaxonomyHiddenList"))
            {
                mTaxonomyList = mWeb.Properties["TaxonomyHiddenList"];
            }
            else
            {
                mTaxonomyList = string.Empty;
            }
            mTaxonomyList = string.Empty;
        }

        public string GetFileAsString(string url)
        {
            return mWeb.GetFileAsString(url);
        }

        public IAveListItem GetListItem(string url)
        {
            SPListItem item;
            try
            {
                item = mWeb.GetListItem(SPResourcePath.FromDecodedUrl(url));
            }
            catch (Exception ex)
            {
                //List Item 获取不到的原因很多
                logger.Warn("Failed to get list item {0}, error message: {1}", url, ex);
                item = null;
            }
            if (item == null)
            {
                return null;
            }
            return new AveListItem(((AveListCollection)Lists).CreateListByType(item.ParentList), item);
        }

        public IAveListItem GetListItem(string itemFullUrl, Guid listId, Guid docId)
        {
            return GetListItem(itemFullUrl);
        }

        public void RevertAllDocumentContentStreams()
        {
            mWeb.RevertAllDocumentContentStreams();
        }

        public IAveView GetViewFromUrl(string listUrl)
        {
            Guid guid;
            Guid guid2;
            string serverRelativeUrlFromUrl = this.GetServerRelativeUrlFromUrl(listUrl, true, true);
            this.Request.MapUrlToListAndView(this.Url, serverRelativeUrlFromUrl, out guid, out guid2);
            AveList list = this.Lists[guid] as AveList;
            SPView view = mWeb.GetViewFromUrl(listUrl);
            if (view == null)
            {
                return null;
            }
            return new AveView(list, view);
        }

        public Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlerts()
        {
            return GetWebAlertsByNative();
        }
        private Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlertsByAPI()
        {
            Dictionary<Guid, Dictionary<Guid, Guid>> alerts = new Dictionary<Guid, Dictionary<Guid, Guid>>();
            foreach (SPAlert alert in mWeb.Alerts)
            {
                Guid alertOldId = alert.ID;
                Guid listId = Guid.Empty;
                if (alert.Properties.ContainsKey("AlertOldId"))
                {
                    alertOldId = new Guid(alert.Properties["AlertOldId"].ToString());
                }
                if (alert.List != null)
                {
                    listId = alert.ListID;
                }
                if (alerts.ContainsKey(listId))
                {
                    alerts[listId].Add(alertOldId, alert.ID);
                }
                else
                {
                    Dictionary<Guid, Guid> listAlert = new Dictionary<Guid, Guid>();
                    listAlert.Add(alertOldId, alert.ID);
                    alerts.Add(listId, listAlert);
                }
            }
            return alerts;
        }

        private Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlertsByNative()
        {
            Dictionary<Guid, Dictionary<Guid, Guid>> alerts = new Dictionary<Guid, Dictionary<Guid, Guid>>();
            using (IAveQueryDataReader dr = mSite.QueryService.GetWebAlerts(mSite.ID, this.ID))
            {
                while (dr.Read())
                {
                    if (dr.IsDBNull(1))
                    {
                        continue;
                    }
                    Guid id = dr.GetGuid(0);
                    Guid oldId = AveAlertUtility.GetAlertOldByProperty(dr.GetString(1));
                    if (oldId == Guid.Empty)
                    {
                        oldId = id;
                    }
                    Guid listId = Guid.Empty;
                    if (!dr.IsDBNull(2))
                    {
                        listId = dr.GetGuid(2);
                    }
                    if (alerts.ContainsKey(listId))
                    {
                        if (!alerts[listId].ContainsKey(oldId))
                        {
                            alerts[listId].Add(oldId, id);
                        }
                    }
                    else
                    {
                        Dictionary<Guid, Guid> listAlert = new Dictionary<Guid, Guid>();
                        listAlert.Add(oldId, id);
                        alerts.Add(listId, listAlert);
                    }
                }
            }
            return alerts;
        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get { return new AveSecurableObjectImpl(this, AveAssemblyUtility.GetPropertyValue(mWeb, "SecurableObjectImpl")); }
        }

        public DateTime Created
        {
            get
            {
                return mWeb.Created;
            }
            set
            {
                mWeb.Created = value;
            }
        }

        public Guid ParentWebId
        {
            get
            {
                return mWeb.ParentWebId;
            }
        }

        public string GetServerRelativeUrlFromUrl(string fullOrRelativeUrl, bool includeQueryString, bool canonicalizeUrl)
        {
            return (string)AveAssemblyUtility.InvokeMethod(mWeb, mWeb_GetServerRelativeUrlFromUrl_Member, new Type[] { typeof(string), typeof(bool), typeof(bool) }, new object[] { fullOrRelativeUrl, includeQueryString, canonicalizeUrl });
        }

        public DateTime LastItemModifiedDate
        {
            get
            {
                return mWeb.LastItemModifiedDate;
            }
            set
            {
                mWeb.LastItemModifiedDate = value;
            }
        }

        public IAveFieldTypeDefinitionCollection FieldTypeDefinitionCollection
        {
            get
            {
                return new AveFieldTypeDefinitionCollection(mWeb.FieldTypeDefinitionCollection);
            }
        }
        public void FakeSPContext()
        {
            FakeSPContext(false);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public void FakeSPContext(bool isPost)
        {
            try
            {
                if (HttpContext.Current == null)
                {
                    HttpRequest request = new HttpRequest("", mWeb.Url, "");
                    HttpContext.Current = new HttpContext(request,
                      new HttpResponse(new StringWriter()));
                }

                // SPContext is based on SPControl.GetContextWeb(), which looks here
                if (HttpContext.Current.Items["HttpHandlerSPWeb"] == null)
                {
                    HttpContext.Current.Items["HttpHandlerSPWeb"] = mWeb;
                }
                if (HttpContext.Current.Request.Browser == null)
                {
                    HttpBrowserCapabilities browser = new HttpBrowserCapabilities();
                    var field = browser.GetType().BaseType.GetField("_browser", BindingFlags.Instance | BindingFlags.NonPublic);
                    field.SetValue(browser, System.Web.HttpContext.Current.Request.UserAgent);
                    field = browser.GetType().BaseType.GetField("_havebrowser", BindingFlags.Instance | BindingFlags.NonPublic);
                    field.SetValue(browser, true);
                    //ADO-170586 为_havecrawler赋值，不然在add ClientWebPart时sharepoint api会获取Crawler属性，抛空引用
                    field = browser.GetType().BaseType.GetField("_havecrawler", BindingFlags.Instance | BindingFlags.NonPublic);
                    field.SetValue(browser, true);
                    field = typeof(HttpRequest).GetField("_httpMethod", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        if (isPost)
                        {
                            field.SetValue(HttpContext.Current.Request, "POST");
                        }

                    }
                    HttpContext.Current.Request.Browser = browser;
                }
                HttpContext.Current.User = Thread.CurrentPrincipal;
                SPUtility.ValidateFormDigest();
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.SPContextFakingError, ex.ToString());
            }
        }

        public void HandleSPContext(Action code)
        {
            try
            {
                FakeSPContext(false);
                if (code != null)
                {
                    code();
                }
            }
            finally
            {
                SetSPContextNull();
            }
        }

        public void HandleSPContext(Action code, bool isPost)
        {
            try
            {
                FakeSPContext(isPost);
                if (code != null)
                {
                    code();
                }
            }
            finally
            {
                SetSPContextNull();
            }
        }

        public void SetSPContextNull()
        {
            System.Web.HttpContext.Current = null;
        }


        public IAveViewStyleCollection ViewStyles
        {
            get
            {
                if (mViewStyleCollection == null)
                {
                    mViewStyleCollection = new AveViewStyleCollection(this, mWeb.ViewStyles);
                }
                return mViewStyleCollection;
            }
        }

        public IAveList GetListFromUrl(string pageUrl)
        {
            SPList list = mWeb.GetListFromUrl(pageUrl);
            if (list == null)
            {
                return null;
            }
            return (this.Lists as AveListCollection).CreateListByType(list);
        }

        public void InvalidateRequest()
        {
            AveAssemblyUtility.InvokeMethod(mWeb, mWeb.GetType(), "InvalidateRequest", null);
        }

        public void InitializeSPRequest()
        {
            AveAssemblyUtility.InvokeMethod(mWeb, mWeb.GetType(), "InitializeSPRequest", null);
        }

        public CultureInfo UICulture
        {
            get
            {
                return mWeb.UICulture;
            }
        }

        public string RequestAccessEmail
        {
            get
            {
                return mWeb.RequestAccessEmail;
            }
            set
            {
                mWeb.RequestAccessEmail = value;
            }
        }

        public object GetObject(string strUrl)
        {
            object retObj = null;
            //SharePoint API Return value will be SPListItem, SPFile, SPFolder;
            retObj = mWeb.GetObject(strUrl);
            if (retObj != null)
            {
                object[] paramsObj;
                if (retObj.GetType().Equals(typeof(SPListItem)))
                {
                    SPList list = (retObj as SPListItem).ParentList;
                    paramsObj = new object[] { (Lists as AveListCollection).CreateListByType(list), retObj };
                }
                else
                {
                    paramsObj = new object[] { this, retObj };
                }
                retObj = AveServerAssemblyInit.CreateElement(typeof(object), paramsObj);
            }
            return retObj;
        }

        public IAveAudit Audit
        {
            get
            {
                if (mAudit == null)
                {
                    mAudit = new AveAudit(mWeb.Audit);
                }
                return mAudit;
            }
        }

        public bool IsPublish
        {
            get
            {
                if (this.Site.IsMoss)
                {
                    return AvePublishing.IsPublishingWeb(this);
                }
                return false;
            }
        }

        public IAveNavigationSerializer NavigationSerializer
        {
            get
            {
                if (mNavigationSerializer == null)
                {
                    mNavigationSerializer = new AveNavigationSerializer(this);
                }
                return mNavigationSerializer;
            }
        }

        private void RestoreThemeOfInheritance()
        {
            if (IsPublish)
            {
                PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(mWeb);
                publishingWeb.ThemedCssFolderUrl.SetValue(publishingWeb.Web.ThemedCssFolderUrl, false);
                publishingWeb.Close();
            }
        }

        /// <summary>
        /// ADO-86393,对于源端web theme是继承与parent web，而目的端是不继承的web，走publishing web的方法直接使之继承。
        /// </summary>
        /// <param name="webSettingInfo"></param>
        /// <returns></returns>
        private bool CheckIfNeedToRestoreTheme(AveWebSettingInfo webSettingInfo)
        {
            try
            {
                bool inheritFromParent = Web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") ? bool.Parse(Web.AllProperties["__InheritsThemedCssFolderUrl"].ToString()) : true;
                if (webSettingInfo.InheritsThemedCssFolderUrl != null)
                {
                    if (webSettingInfo.InheritsThemedCssFolderUrl.Value)
                    {
                        if (!inheritFromParent)
                        {
                            PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(mWeb);
                            publishingWeb.ThemedCssFolderUrl.SetInherit(true, false);
                            publishingWeb.Close();
                        }
                        return false;
                    }
                    else
                    {
                        if (inheritFromParent)
                        {
                            PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(mWeb);
                            publishingWeb.ThemedCssFolderUrl.SetInherit(false, false);
                            publishingWeb.Close();
                        }
                        return true;
                    }
                }
                else
                {
                    if (!inheritFromParent)
                    {
                        PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(mWeb);
                        publishingWeb.ThemedCssFolderUrl.SetInherit(true, false);
                        publishingWeb.Close();
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //if an error occurred return true.
                logger.Log(AveLogLevel.WARN, "An error occurred while checking web theme inheritance. Web Url: {0}. Error: {1}", mWeb.ServerRelativeUrl, ex.ToString());
                return true;
            }
        }

        //add by adrian
        [SPDisposeCheckIgnore(SPDisposeCheckID._140, "This Web will be Disposed by AveWeb")]
        public void RestoreTheme(AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            if (AveEnv.IsMoss && !Web.IsRootWeb && !CheckIfNeedToRestoreTheme(webSettingInfo))
            {
                return;
            }
            try
            {
                bool useRestoreCssFolder = true;
                string themeColorURL = string.Empty;
                string themeFontURL = string.Empty;
                string themeImageURL = string.Empty;
                string masterPageUrl = string.Empty;

                if (webSettingInfo.ThemedTitle != null && webSettingInfo.ThemedTitle.IsAvailable)
                {
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
                    if (webSettingInfo.CustomMasterUrl != null && webSettingInfo.CustomMasterUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.CustomMasterUrl.Value))
                    {
                        masterPageUrl = webSettingInfo.CustomMasterUrl.Value;
                    }
                    else
                    {
                        masterPageUrl = mWeb.CustomMasterUrl;
                    }
                    if (!string.IsNullOrEmpty(themedCssFolderUrl) && themedCssFolderUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                    {
                        themedCssFolderUrl = "/" + themedCssFolderUrl.TrimStart('/');
                    }


                    Uri imageFile = null;
                    SPFile colorFile = GetThemeFile(themeColorURL);
                    SPFile fontFile = GetThemeFile(themeFontURL);
                    SPFile aveImageFile = GetThemeFile(themeImageURL);

                    if (aveImageFile != null)
                    {
                        imageFile = new Uri(themeImageURL, UriKind.RelativeOrAbsolute);
                    }

                    //SPTheme.ApplyTo will throw nullreferenceexception when themcolorurl is empty
                    string themeCssFolderUrl = null;
                    if (webSettingInfo.ThemedCssFolderUrl != null)
                    {
                        themeCssFolderUrl = webSettingInfo.ThemedCssFolderUrl.Value;
                    }
                    if (colorFile != null && IfRestoreThemeAgain(themeCssFolderUrl, mWeb.ThemedCssFolderUrl))
                    {
                        SPTheme currentTheme = SPTheme.Open(webSettingInfo.ThemedTitle.Value, colorFile, fontFile, imageFile);
                        bool isSharedTheme = IsSharedTheme(themedCssFolderUrl);
                        ThmxTheme.RemoveThemeFromWeb(mWeb, !isSharedTheme);
                        //currentTheme.ApplyTo(mWeb, isSharedTheme);
                        string newThemedCssFolderUrl = AveAssemblyUtility.InvokeMethod(currentTheme, "ApplyToInternal", new Type[] { typeof(SPWeb), typeof(bool), typeof(bool), typeof(string) }, new object[] { mWeb, isSharedTheme, true, currentTheme.Name }) as string;
                        bool imageProcessed = (bool)AveAssemblyUtility.GetFieldValue(currentTheme, "m_backgroundImageProcessed");
                        string backgroundImageOutputFileName = AveAssemblyUtility.GetFieldValue(currentTheme, "m_backgroundImageOutputFileName") as string;
                        themeImageURL = imageProcessed ? (!string.IsNullOrEmpty(backgroundImageOutputFileName) ? SPUrlUtility.CombineUrl(newThemedCssFolderUrl, backgroundImageOutputFileName) : string.Empty) : string.Empty;
                        UpdateCurrentComposedLookItem(masterPageUrl, themeColorURL, themeImageURL, themeFontURL);

                        //add theme folder name mapping to web property
                        mWeb.AllProperties["AveThemedCssFolderUrlMapping"] = themeCssFolderUrl + ";" + newThemedCssFolderUrl;

                        //change the theme of the sub site whose theme is inherited
                        //need reload the web, otherwise there is a warning another process is processing the web.
                        using (SPSite mySite = new SPSite(mWeb.Site.ID))
                        {
                            PublishingWeb publishWeb = PublishingWeb.GetPublishingWeb(mySite.OpenWeb(mWeb.ID));
                            publishWeb.ThemedCssFolderUrl.SetValue(newThemedCssFolderUrl, false, string.Empty, null);
                            publishWeb.Close();
                        }
                    }

                }
                else
                {
                    //sp2013 10theme
                    if (webSettingInfo != null && webSettingInfo.ThemedTemplate != null && webSettingInfo.ThemedTemplate.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedTemplate.Value))
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<ThmxTheme> managedThemes = null;
                        managedThemes = ThmxTheme.GetManagedThemes(mWeb.Site);
                        ThmxTheme applyTheme = null;
                        foreach (ThmxTheme theme in managedThemes)
                        {
                            if (theme.Name.Equals(webSettingInfo.ThemedTemplate.Value))
                            {
                                applyTheme = theme;
                                break;
                            }
                        }
                        this.FakeSPContext(true);
                        if (applyTheme != null)
                        {
                            useRestoreCssFolder = false;
                            applyTheme.ApplyTo(mWeb, true);
                            mWeb.Update();
                            ReloadWeb();

                            if (AveEnv.IsMoss)
                            {
                                RestoreThemeOfInheritance();
                            }
                        }
                    }

                    if (useRestoreCssFolder)
                    {
                        SPWeb rootWeb = mWeb;
                        if (!mWeb.IsRootWeb)
                        {
                            rootWeb = mWeb.Site.RootWeb;
                        }
                        string folderPath = string.Empty;
                        if (!rootWeb.ServerRelativeUrl.Equals("/"))
                        {
                            folderPath = themedCssFolderUrl.Substring(rootWeb.ServerRelativeUrl.Length + 1);
                        }
                        else
                        {
                            folderPath = themedCssFolderUrl;
                        }
                        if (rootWeb.GetFolder(SPResourcePath.FromDecodedUrl(folderPath)).Exists)
                        {
                            if (themedCssFolderUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                            {
                                themedCssFolderUrl = "/" + themedCssFolderUrl.TrimStart('/');
                            }
                            ThmxTheme.SetThemeUrlForWeb(mWeb, AveUrlUtility.CombineUrl(folderPath, "theme.thmx"));
                            ReloadWeb();
                            //13环境中创建10的site，使用customer theme时，ThemedCssFolderUrl使用api返回的结果可能不准确，所以需要重新赋值，避免界面layout出错；
                            if (string.Compare(mWeb.ThemedCssFolderUrl, themedCssFolderUrl, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                mWeb.ThemedCssFolderUrl = themedCssFolderUrl;
                                mWeb.Update();
                            }

                            if (AveEnv.IsMoss)
                            {
                                RestoreThemeOfInheritance();
                            }
                        }
                        else
                        {
                            //mLog.Log(AveLogLevel.WARN, string.Format("ThemeCss Folder can not be found in destionary,Css Folder{0}", ThemedCssFolderUrl));
                        }
                    }
                }
            }
            finally
            {
                this.SetSPContextNull();
            }

        }

        private SPFile GetThemeFile(string fileRelativeUrl)
        {
            SPFile file = null;
            if (string.IsNullOrEmpty(fileRelativeUrl))
            {
                file = null;
            }
            else if (IsSharedTheme(fileRelativeUrl))//the theme file will exist in current web when it is not shared generated, refer SPTheme.Apply(web, shared) for more details            
            {
                file = mWeb.Site.RootWeb.GetFile(SPResourcePath.FromDecodedUrl(fileRelativeUrl));
            }
            else
            {
                file = mWeb.GetFile(SPResourcePath.FromDecodedUrl(fileRelativeUrl));
            }
            return (file != null && file.Exists) ? file : null;
        }

        private SPFolder GetThemeFolder(string folderRelativeUrl)
        {
            if (string.IsNullOrEmpty(folderRelativeUrl))
            {
                return null;
            }
            //the theme file will exist in current web when it is not shared generated, refer SPTheme.Apply(web, shared) for more details
            if (IsSharedTheme(folderRelativeUrl))
            {
                return mWeb.Site.RootWeb.GetFolder(SPResourcePath.FromDecodedUrl((folderRelativeUrl)));
            }
            else
            {
                return mWeb.GetFolder(SPResourcePath.FromDecodedUrl(folderRelativeUrl));
            }
        }

        private bool IsSharedTheme(string fileRelativeUrl)
        {
            if (string.IsNullOrEmpty(fileRelativeUrl))
            {
                return false;
            }
            else
            {
                string str = SPUrlUtility.CombineUrl(mWeb.ServerRelativeUrl, "_themes");
                return !fileRelativeUrl.StartsWith(str, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Web的Theme是否需要再次还原
        /// </summary>
        /// <param name="sourceThemedCssFolderUrl"></param>
        /// <param name="desThemedCssFolderUrl"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100013:DoNotUseSpecificSPMethod")]
        private bool IfRestoreThemeAgain(string sourceThemedCssFolderUrl, string desThemedCssFolderUrl)
        {
            bool result = true;
            try
            {
                if (!mWeb.AllProperties.ContainsKey("AveThemedCssFolderUrlMapping") || string.IsNullOrEmpty(desThemedCssFolderUrl))
                {
                    result = true;
                }
                else
                {
                    string content = mWeb.AllProperties["AveThemedCssFolderUrlMapping"] as string;
                    string[] lines = content.Split(';');
                    if (lines.Length == 2)
                    {
                        if (sourceThemedCssFolderUrl.Equals(lines[0]) && desThemedCssFolderUrl.Equals(lines[1]))
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Restore Theme Error. " + ex.ToString());
            }
            return result;
        }

        private void UpdateCurrentComposedLookItem(string webLayoutUrl, string themeUrl, string themeOutputImageUrl, string fontSchemeUrl)
        {
            SPList catalog = mWeb.GetCatalog(SPListTemplateType.DesignCatalog);
            if (catalog != null)
            {
                SPQuery query = new SPQuery();
                query.RowLimit = 1;
                query.Query = "<Where><Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq></Where>";
                query.ViewFields = "<FieldRef Name='DisplayOrder'/>";
                query.ViewFieldsOnly = true;
                SPListItemCollection items = catalog.GetItems(query);
                if (items.Count == 1)
                {
                    items[0].Delete();
                }
                SPListItem item = catalog.AddItem();
                item["Name"] = SPResource.GetString(CultureInfo.CurrentUICulture, "DesignGalleryCurrentItemName", new object[0]);
                item["Title"] = SPResource.GetString(CultureInfo.CurrentUICulture, "DesignGalleryCurrentItemName", new object[0]);
                item["MasterPageUrl"] = webLayoutUrl;
                item["ThemeUrl"] = themeUrl;
                item["ImageUrl"] = themeOutputImageUrl;
                item["FontSchemeUrl"] = (string.IsNullOrEmpty(fontSchemeUrl) || fontSchemeUrl.Equals("default", StringComparison.OrdinalIgnoreCase)) ? string.Empty : fontSchemeUrl;
                item["DisplayOrder"] = 0;
                item.Update();
            }
        }

        public void RestoreMasterPage(AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            throw new NotImplementedException();
        }
        public IAveRolesSerializer RolesSerializer
        {
            get
            {
                if (m_RolesSerializer == null)
                {
                    m_RolesSerializer = new AveRolesSerializer(mSite.QueryService, this);
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
                    m_WebSerializer = new AveWebSerializer(mWeb);
                }
                return m_WebSerializer;
            }
        }

        public IAveWebSettingSerializer WebSettingSerializer
        {
            get
            {
                if (m_WebSettingSerializer == null)
                {
                    m_WebSettingSerializer = new AveWebSettingSerializer(mSite.QueryService, this);
                }
                return m_WebSettingSerializer;
            }
        }

        public IAveUsersSerializer WebUsersSerializer
        {
            get
            {
                if (m_WebUsersSerializer == null)
                {
                    m_WebUsersSerializer = new AveWebUsersSerializer(mSite.QueryService, this);
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
                    m_GroupsSerializer = new AveGroupsSerializer(mSite.QueryService, this);
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
                    m_RoleAssignmentsSerializer = new AveRoleAssignmentsSerializer(mSite.QueryService);
                }
                return m_RoleAssignmentsSerializer;
            }
        }

        public IAveFeatureSerializer FeatureSerializer
        {
            get
            {
                if (m_FeatureSerializer == null)
                {
                    m_FeatureSerializer = new AveFeatureSerializer(this);
                }
                return m_FeatureSerializer;
            }
        }

        public IEnumerable<CultureInfo> SupportedUICultures
        {
            get
            {
                return mWeb.SupportedUICultures;
            }
        }

        public void AddSupportedUICulture(CultureInfo cultureInfo)
        {
            mWeb.AddSupportedUICulture(cultureInfo);
        }

        public CultureInfo LanguageCulture
        {
            get
            {
                return AveAssemblyUtility.GetPropertyValue(mWeb, "LanguageCulture") as CultureInfo;
            }
        }

        public IAveWorkflowTemplateCollection WorkflowTemplates
        {
            get
            {
                return new AveWorkflowTemplateCollection(this, mWeb.WorkflowTemplates);
            }
        }

        public IAveList GetListByName(string strListName, bool bThrowException)
        {
            try
            {
                return (Lists as AveListCollection).CreateListByType((SPList)AveAssemblyUtility.InvokeMethod(mWeb.Lists, "GetListByName", new Type[] { typeof(string), typeof(bool) }, new object[] { strListName, bThrowException }));
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public long Size
        {
            get
            {
                long size = 0;
                Dictionary<Guid, long> allWebSize = mSite.GetAllWebSize();
                if (allWebSize.ContainsKey(ID))
                {
                    size = allWebSize[ID];
                }
                return size;
            }
        }

        public IAveDocTemplateCollection DocTemplates
        {
            get
            {
                if (mDocTemplates == null)
                {
                    mDocTemplates = new AveDocTemplateCollection(this, mWeb.DocTemplates);
                }
                return mDocTemplates;
            }
        }

        public IAveListCollection GetListsOfType(AveBaseType baseType)
        {
            return new AveListCollection(this, mWeb.GetListsOfType((SPBaseType)baseType));
        }

        public void CreateDefaultAssociatedGroups(string userLogin, string userLogin2, string groupNameSeed)
        {
            mWeb.CreateDefaultAssociatedGroups(userLogin, userLogin2, groupNameSeed);
        }

        public IAveFileCollection Files
        {
            get
            {
                if (mFiles == null)
                {
                    mFiles = new AveFileCollection((this.RootFolder as AveFolder), mWeb.Files);
                }
                return mFiles;
            }
        }


        public void ReloadWeb()
        {
            if (mWeb != null)
            {
                Guid id = mWeb.ID;
                CleanUp();
                mWeb.Dispose();
                mWeb = (Site.OpenWeb(id) as AveWeb).Web;
                if (mLists != null)
                {
                    mLists.Reload();
                }
                base.Reload(mWeb);
                SetAllowUnsafeUpdate();
            }
        }

        public void SetAllowUnsafeUpdate()
        {
            if (!AllowUnsafeUpdates)
            {
                AllowUnsafeUpdates = true;
            }
        }

        public void ReloadFeatures()
        {
            mFeatures = new AveFeatureCollection(mWeb.Features, this);
        }

        public string WebTemplateName
        {
            get
            {
                SPFeature feat = mWeb.Features.FirstOrDefault(x => null != x.Properties["GeneratedBySaveAsTemplate"] &&
                    Convert.ToString(x.Properties["GeneratedBySaveAsTemplate"].Value) == "1");
                if (feat != null)
                {
                    Guid solution = feat.Definition.SolutionId;
                    String name = mSite.Solutions[solution].Name;
                    return name.Substring(0, name.IndexOf(".wsp", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    IAveWebTemplateCollection webTemplates = mSite.GetWebTemplates((uint)this.Locale.LCID);
                    IAveWebTemplate webTemplate = webTemplates.GetWebTemplateByIdConfiguration(mWeb.WebTemplateId, mWeb.Configuration);
                    if (webTemplate != null)
                    {
                        return (webTemplate as AveWebTemplate).Title;
                    }
                    return null;
                }
            }
        }

        public int Count
        {
            get
            {
                try
                {
                    return mSite.QueryService.GetSubWebCounts(this.Site.ID, this.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while access data from GetSubWebCounts.  Error Message: {0}", ex.ToString());
                    return 0;
                }
            }
        }

        #endregion

        #region IDisposable Members



        public void Dispose()
        {
            CleanUp();
            //if (s_GlobalCurrentScopes == null)
            //{
            //    s_GlobalCurrentScopes = AveAssemblyUtility.GetFieldValue(null, typeof(SPMonitoredScope), "s_GlobalCurrentScopes") as System.Collections.Concurrent.ConcurrentDictionary<Guid, SPMonitoredScope>;
            //}
            //if (s_GlobalCurrentScopes != null && s_GlobalCurrentScopes.Count > 0)
            //{
            //    //logger.Info("Disposing global monitor. Count:{0}. Url:{1}.", s_GlobalCurrentScopes.Count, mWeb.Url);
            //    foreach (var monitor in s_GlobalCurrentScopes.Values)
            //    {
            //        monitor.Dispose();
            //    }
            //    s_GlobalCurrentScopes.Clear();
            //}
            mLists = null;
            DoDispose(mWeb);
            //RotateContextHashTables();
        }

        private static void RotateContextHashTables()
        {
            try
            {
                Type pushContext = Type.GetType("Cobalt.PushContext, Microsoft.CobaltCore, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
                var hashtable = AveAssemblyUtility.GetPropertyValue(AveAssemblyUtility.GetStaticPropertyValue(pushContext, "TheGlobalContext"), "TheHashTable");
                AveAssemblyUtility.InvokeMethod(hashtable, "RotateHashTables", new object[] { false, true });
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
            }
        }

        #endregion

        #region IAveWeb Members

        public IAveFolder GetFolder(Guid uniqueId, int rowId, string serverRelativeUrl)
        {
            return GetFolder(uniqueId);
        }

        #endregion

        #region Private Method
        private void CleanUp()
        {
            DoDispose(mFields);
            mFields = null;
            DoDispose(mAvailableFields);
            mAvailableFields = null;
            DoDispose(mRootFolder);
            mRootFolder = null;
            DoDispose(mFirstUniqueRoleDefinitionWeb);
            mFirstUniqueRoleDefinitionWeb = null;
            DoDispose(mRequest);
            mRequest = null;

            mWorkflowAssociations = null;
            mContentTypes = null;
            mFeatures = null;
            mGroups = null;
            siteGroups = null;
            mRoleDefinitions = null;
            mAvailableContentTypes = null;
            mNavigation = null;
            mUsers = null;
            mSiteAdministrators = null;
            mRegionalSettigns = null;
            mWebs = null;
            mAllUsers = null;
            mAssociatedMemberGroup = null;
            mAssociatedOwnerGroup = null;
            mAssociatedVisitorGroup = null;
            mAuthor = null;
            mParentWeb = null;
            mCurrentUser = null;
            mSiteUserInfoList = null;
            mAlerts = null;
            mListTemplates = null;
            mEventReceivers = null;
            mProperties = null;
            mViewStyleCollection = null;
            mTaxonomyList = null;
            mSiteUsers = null;
            mAudit = null;
            mNavigationSerializer = null;
            m_RolesSerializer = null;
            m_WebSerializer = null;
            m_WebSettingSerializer = null;
            m_WebUsersSerializer = null;
            m_GroupsSerializer = null;
            m_RoleAssignmentsSerializer = null;
            m_FeatureSerializer = null;
            m_Id = Guid.Empty;
            mDocTemplates = null;
            mFiles = null;
            mLists = null;
            this.associatedGroups = null;
        }
        private void DoDispose(IDisposable disposable)
        {
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
        #endregion

        #region Rewrite IAveSecurableObject for unsafe update
        public new void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWeb.BreakRoleInheritance"))
            {

                SetAllowUnsafeUpdate();
                try
                {
                    base.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while BreakRoleInheritance. ex:{0}", e);
                    //Merge CI ADO-135171,当出现打破继承的权限转移的web时，还原feature会让web对象不同步，此时需要Reload一下web对象来继续进行还原。
                    ReloadWeb();
                    base.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
                }

            }

        }

        public new void BreakRoleInheritance(bool copyRoleAssignments)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWeb.BreakRoleInheritance_1"))
            {

                SetAllowUnsafeUpdate();
                try
                {
                    base.BreakRoleInheritance(copyRoleAssignments);
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while BreakRoleInheritance. ex:{0}", e);
                    //Merge CI ADO-135171.
                    ReloadWeb();
                    base.BreakRoleInheritance(copyRoleAssignments);
                }

            }

        }

        public new void ResetRoleInheritance()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWeb.ResetRoleInheritance"))
            {

                base.ResetRoleInheritance();
                SetAllowUnsafeUpdate();

            }

        }
        #endregion


        public List<Guid> StopListAlerts(IAveList list)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWeb.StopListAlerts"))
            {

                List<Guid> alerts = new List<Guid>();
                try
                {
                    Guid listId = list.ID;
                    List<Guid> tmpAlertIds = new List<Guid>();
                    if (this.Alerts != null)
                    {
                        foreach (IAveAlert alert in this.Alerts)
                        {
                            if (alert.ListID != null && alert.Status == AveAlertStatus.On && alert.ListID == listId)
                            {
                                tmpAlertIds.Add(alert.ID);
                            }
                        }
                        if (tmpAlertIds.Count > 0)
                        {
                            foreach (Guid alertId in tmpAlertIds)
                            {
                                IAveAlert alert = this.Alerts[alertId];
                                alert.Status = AveAlertStatus.Off;
                                alert.Update(false);
                                alerts.Add(alertId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.ListAlertStopError,
                        list == null ? string.Empty : list.Title, ex);
                }
                return alerts;

            }

        }

        public DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId)
        {
            return mSite.QueryService.GetLastAccessedDayOfWeb(siteId, webId);
        }

        public void Recycle()
        {
            mWeb.Recycle();
        }


        public string GetFormula(string webUrl, string listId, string newFormula, string oldFormula)
        {
            object request = AveAssemblyUtility.GetPropertyValue(mWeb, mWeb_Request_Member);
            object[] args = new object[] { 1, webUrl, listId, newFormula, null };
            return AveAssemblyUtility.InvokeMethod(request, request.GetType(), "CallCalcEngine", args) as string;
        }

        public string GetWebRelativeUrlFromUrl(string strUrl)
        {
            object obj = AveAssemblyUtility.InvokeMethod(mWeb, "GetWebRelativeUrlFromUrl", new Type[] { typeof(string) }, strUrl);
            return obj != null ? obj.ToString() : string.Empty;
        }

        #region IAveWeb Members

        public IAveFolder GetFolder(int rowId, string serverRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public IAveWorkflowCollection Workflows
        {
            get
            {
                if (mWorkflows == null)
                {
                    SPWorkflowCollection workflows = mWeb.Workflows;
                    if (workflows != null)
                    {
                        mWorkflows = new AveWorkflowCollection(this, workflows);
                    }
                }
                return mWorkflows;
            }
        }

        public string ProcessBatchData(string strBatchData)
        {
            return mWeb.ProcessBatchData(strBatchData);
        }

        public void AddProperty(object key, object value)
        {
            mWeb.AddProperty(key, value);
        }
        #endregion

        #region add for SP2013
        public int SearchVersion
        {
            get { return (int)AveAssemblyUtility.GetPropertyValue(mWeb, "SearchVersion"); }
            set { AveAssemblyUtility.SetPropertyValue(mWeb, "SearchVersion", value); }
        }

        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get
            {
                return new AveFeatureDefinitionCollection(mWeb.FeatureDefinitions);
            }
        }

        public void ApplyTheme(string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            mWeb.ApplyTheme(colorPaletteUrl, fontSchemeUrl, backgroundImageUrl, shareGenerated);
        }

        public void EnableDisableAbuseReports(bool bEnable)
        {
            string assemblyName = "Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string typeName = "Microsoft.SharePoint.Portal.FunctionalityEnablers";
            Type type = AveAssemblyUtility.GetType(assemblyName, typeName);
            AveAssemblyUtility.InvokeStaticMethod(type, "EnableDisableAbuseReports", new Type[] { typeof(SPWeb), typeof(bool) }, new object[] { mWeb, bEnable });
        }

        public bool HideSiteContentsLink
        {
            get { return mWeb.HideSiteContentsLink; }
            set { mWeb.HideSiteContentsLink = value; }
        }

        public DataTable GetSiteData(IAveSiteDataQuery siteDataQuery)
        {
            SPSiteDataQuery spSiteDataQuery = new SPSiteDataQuery()
            {
                Lists = siteDataQuery.Lists,
                Webs = siteDataQuery.Webs,
                Query = siteDataQuery.Query,
                ViewFields = siteDataQuery.ViewFields,
                RowLimit = siteDataQuery.RowLimit
            };

            return mWeb.GetSiteData(spSiteDataQuery);
        }

        #endregion

        #region Add to operate Change Log

        public IAveChangeCollection GetChanges()
        {
            return new AveChangeCollection(mWeb.GetChanges());
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            return new AveChangeCollection(mWeb.GetChanges((query as AveChangeQuery).ChangeQuery));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            return new AveChangeCollection(mWeb.GetChanges((changeToken as AveChangeToken).ChangeToken));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            SPChangeToken ct1 = (changeToken as AveChangeToken).ChangeToken;
            SPChangeToken ct2 = (changeTokenEnd as AveChangeToken).ChangeToken;
            return new AveChangeCollection(mWeb.GetChanges(ct1, ct2));
        }

        #endregion

        #region Add for SP App
        public bool IsAppWeb
        {
            get { return mWeb.IsAppWeb; }
        }

        public IList<IAveAppInstance> GetAppInstancesByProductId(Guid productId)
        {
            List<IAveAppInstance> list = new List<IAveAppInstance>();
            IList<SPAppInstance> instances = Web.GetAppInstancesByProductId(productId);
            foreach (SPAppInstance instance in instances)
            {
                list.Add(AveServerAssemblyInit.CreateElement(typeof(IAveAppInstance), new object[] { instance }) as AveAppInstance);
            }
            return list;
        }

        public Guid AppInstanceId
        {
            get { return mWeb.AppInstanceId; }
        }

        public IAveAppInstance LoadAndInstallApp(Stream appPackageStream)
        {
            return new AveAppInstance(mWeb.LoadAndInstallApp(appPackageStream));
        }

        public IAveAppInstance GetAppInstanceById(Guid appInstanceId)
        {
            return new AveAppInstance(mWeb.GetAppInstanceById(appInstanceId));
        }

        public void UpgradeAppByProductId(Guid productId)
        {

            try
            {
                this.AppSerializer.UpgradeAppByProductId(productId);
            }
            catch (Exception ex)
            {
                logger.Warn("Upgrade App Exception: " + ex.Message);
                throw;
            }

        }

        public IAveAppSerializer AppSerializer
        {
            get
            {
                if (mAppSerializer == null)
                {
                    mAppSerializer = new AveAppSerializer(this);
                }
                return mAppSerializer;
            }
        }

        public IAveAppInstance LoadAndInstallApp(Stream appPackageStream, int appSource, string assetId, string contentMarket)
        {
            object[] args = new object[] { appPackageStream, mWeb, (SPAppSource)appSource, false, assetId == null ? string.Empty : assetId, contentMarket == null ? string.Empty : contentMarket };
            object objectNew = AveAssemblyUtility.InvokeStaticMethod(typeof(SPApp), "CreateAppUsingPackageMetadata", args);

            if (objectNew is SPApp)
            {
                SPApp appNew = objectNew as SPApp;
                Guid appId = appNew.CreateAppInstance(mWeb, CultureInfo.InvariantCulture);
                SPAppInstance appInstanceNew = SPAppCatalog.GetAppInstance(mWeb, appId);
                appInstanceNew.Install();
                return new AveAppInstance(appInstanceNew);
            }
            {
                throw new AveWrapperAppException("App is null.");
            }
        }


        #endregion

        Dictionary<int, string> cacheUsers = new Dictionary<int, string>();
        internal string GetSiteUserById(int id)
        {
            if (cacheUsers.ContainsKey(id))
            {
                return cacheUsers[id];
            }
            var user = this.SiteUsers.GetByID(id);
            if (user != null)
            {
                cacheUsers.Add(id, user.LoginName);
            }
            return user.LoginName;
        }

        IAveRecycleBinItemCollection recycleBin;
        public IAveRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (recycleBin == null)
                {
                    recycleBin = new AveRecycleBinItemCollection(this, mWeb.RecycleBin);
                }
                return recycleBin;
            }
        }

        #region User Resource
        public IAveUserResource DescriptionResource
        {
            get { return new AveUserResource(Web.DescriptionResource); }
        }

        public IAveUserResource TitleResource
        {
            get { return new AveUserResource(Web.TitleResource); }
        }
        #endregion

        public IAvePublishingWeb GetPublishingWeb
        {
            get
            {
                if (IsPublish)
                {
                    return new AvePublishingWeb(this);
                }

                return null;
            }
        }

        public bool HaveAddAndCustomizePagesPermission
        {
            get { return true; }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return new AveUserCustomActionCollection(Web.UserCustomActions);
            }
        }

        public Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string listName, Guid listId)
        {
            throw new NotSupportedException();
        }

        public string ImportNintexWorkflow(System.IO.Stream stream, string publishName, string listTitle, Guid listId, bool overWrite)
        {
            throw new NotSupportedException();
        }

        public string ConvertNintexFormJsonObjectToXml(string formJsonData, string fileName)
        {
            throw new NotSupportedException();
        }

        public Guid PublishNintexWorkflow(Guid workflowDefinitionId)
        {
            throw new NotSupportedException();
        }
        public IAveAppInstance DeployApp(Guid productId, Wrapper.Restore.AveRestoreMode restoreMode)
        {
            throw new NotImplementedException();
        }
        public IAveFeatureDefinitionCollection GetAllFeatureDefinitions()
        {
            throw new NotImplementedException();
        }

        public Guid VariationLabelListId
        {
            get
            {
                if (variationLabelListId.HasValue) return variationLabelListId.Value;
                try
                {
                    variationLabelListId = Web.IsRootWeb && Web.AllProperties.ContainsKey("_VarLabelsListId")
                                                         ? new Guid(Web.AllProperties["_VarLabelsListId"].ToString())
                                                         : Guid.Empty;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while checking whether the list is variation labels, exception:{0}.", e);
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
                    relationshipsListId = Web.IsRootWeb && Web.AllProperties.ContainsKey("_VarRelationshipsListId")
                                                      ? new Guid(Web.AllProperties["_VarRelationshipsListId"].ToString())
                                                      : Guid.Empty;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while checking whether the list is relationships list, exception:{0}.", e);
                    relationshipsListId = Guid.Empty;
                }
                return relationshipsListId.Value;
            }
        }
        public bool MembersCanShare
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public string AccessRequestSiteDescription
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        public bool UseAccessRequestDefault
        {
            get
            {
                return false;
            }

            set
            {
            }
        }
    }
}
