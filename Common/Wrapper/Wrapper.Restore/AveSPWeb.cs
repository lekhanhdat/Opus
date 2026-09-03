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






using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Constants;
using AvePoint.Wrapper.Resource;
using LS.SPWorkflowProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    public class AveSPWeb : RestoreableObject, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static List<string> DesignManagerViewPropertyNames = new List<string>
        {
            "htmldesignviewnamehtmlmasterpages",
            "htmldesignviewnamehtmldesignfiles",
            "htmldesignviewnamehtmldesignsandrelatedfiles",
            "htmldesignviewnamedesigntemplates",
            "htmldesignviewnamehtmlpagelayouts"
        };

        private AveSPSite mAveSite = null;
        private string mName = string.Empty;
        private Guid mId = Guid.Empty;
        private IAveWeb mSPWeb = null;
        private IAveBackupRestoreQueryService mQueryService = null;

        private bool mIsNewCreated = false;
        private uint mLanguageForNewCreatedWeb = 0;
        private string mScope = string.Empty;
        private AveWebInfo mWebInfo = null;
        private AveWebSettingInfo mWebSettingInfo = null;
        private bool mIsRestoreWebSetting = false;
        public bool needListRestore = false;
        private bool mIsRestoreWebNavgation = true;
        private AveSPWebContentTypeCollection mContentTypes;
        private AveSPWebFieldCollection mFields;
        private Guid mOldId = Guid.Empty;
        private Guid mTaxonomyHiddenList = Guid.Empty;
        private RestoringDto mRestoringWeb = new RestoringDto();
        public List<AveRoleInfo> Roles { get; set; }
        private bool mNeedContinue = true;
        public bool NeedContinue
        {
            get { return this.mNeedContinue; }
            set { this.mNeedContinue = value; }
        }

        public RestoringDto RestoringWeb
        {
            get { return mRestoringWeb; }
        }

        public AveFeatureInfoBox SourceFeatures
        {
            get;
            set;
        }

        public Dictionary<string, string> MetaInfoDictionary = null;//mWebMetaInfoDictionary

        //add for master page setting restore
        private string mAlternateCSSUrl = null;

        private uint mSrcLanguageId;

        internal string CommunitySiteDiscussionsListTitle;
        private AveSPNavigation mNavigation;
        private AveWebFeature mFeature;
        private AveWebSecurity mWebSecurity;
        private AveSPMembers mMembers;
        private IAveWeb mParentAveWeb;
        private IAveThmxTheme mThmxTheme;
        private IAveRestoreStream mRestoreStream;
        private string mWebUrl;
        private string mUrl;
        private string mSrcUrl;
        private long mSize;
        private Dictionary<Guid, Dictionary<Guid, Guid>> listAlertIdMappings;
        private IReport report = new AveWrapperReport();
        /// <summary>
        /// Dictionary<ListId, Dictionary<ContentTypeId, Tuple<Dictionary<NintexFormControlUniqueId, AveNintexFormControlType, Dictionary<ControlName, AveNintexFormControlType>>>> 
        /// </summary>
        private Dictionary<Guid, Dictionary<string, Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>>> nintexFormControlTypeCache = new Dictionary<Guid, Dictionary<string, Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>>>();
        public Dictionary<Guid, Dictionary<string, Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>>> NintexFormControlTypeCache
        {
            get
            {
                return nintexFormControlTypeCache;
            }
        }
        public IReport GetReport()
        {
            return report;
        }

        private bool mRestorePermissionLevel = false;
        public bool RestorePermissionLevel
        {
            internal get { return mRestorePermissionLevel; }
            set { mRestorePermissionLevel = value; }
        }

        public string ReportMessage { get; private set; }

        public string ThemedCssFolderUrl { get; set; }

        public string AlternateCSSUrl
        {
            get
            {
                return mAlternateCSSUrl;
            }
            set
            {
                mAlternateCSSUrl = value;
            }
        }
        public AveRoleInfo GetRoleByName(string roleName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetRoleByName"))
            {
#endif
                foreach (AveRoleInfo roleinfo in Roles)
                {
                    if (roleinfo.Title.Equals(roleName))
                    {
                        return roleinfo;
                    }
                }
                return null;
#if PerformanceLog
            }
#endif
        }
        public bool WebNavigationRestore
        {
            get
            {
                return mIsRestoreWebNavgation;
            }
            set
            {
                mIsRestoreWebNavgation = value;
            }
        }

        public Guid TaxonomyHiddenList
        {
            get { return mTaxonomyHiddenList; }
        }

        public Guid OldId
        {
            get { return mOldId; }
        }

        public AveSPWebContentTypeCollection ContentTypes
        {
            get { return mContentTypes; }
        }

        public AveSPFieldCollection Fields
        {
            get { return mFields; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public IAveWeb SPWeb
        {
            get { return mSPWeb; }
        }
        public string ScopeString
        {
            get { return mScope; }
        }

        public bool IsNewCreated
        {
            get { return mIsNewCreated; }
        }
        public AveWebInfo WebInfo
        {
            get { return mWebInfo; }
        }
        public AveWebSettingInfo WebSettingInfo
        {
            set { mWebSettingInfo = value; }
            get { return mWebSettingInfo; }
        }

        public uint WebSrcLanguageId
        {
            get { return this.mSrcLanguageId; }
        }

        public AveSPSite ParentSite
        {
            get { return mAveSite; }
        }

        public AveObjectFeature Feature
        {
            get { return mFeature; }
        }

        public AveSPNavigation Navigation
        {
            get { return mNavigation; }
        }

        public AveSPMembers Members
        {
            get { return mMembers; }
        }

        public IAveThmxTheme ThmxTheme
        {
            get
            {
                if (mThmxTheme == null)
                {
                    mThmxTheme = mAveSite.ObjectModelFactory.CreateThmxTheme(mAveSite.SPSite);
                }
                return mThmxTheme;
            }
        }

        public string Url
        {
            get { return mUrl; }
        }

        public string SrcUrl
        {
            get { return mSrcUrl; }
        }

        public long Size
        {
            get { return mSize; }
        }

        public AveObjectSecurity Security
        {
            get
            {
                if (mWebSecurity == null)
                {
                    mWebSecurity = new AveWebSecurity(this);
                }
                return mWebSecurity;
            }
        }
        public Dictionary<Guid, Dictionary<Guid, Guid>> ListAlertIdMappings
        {
            get
            {
                if (listAlertIdMappings == null)
                {
                    listAlertIdMappings = mSPWeb.GetWebAlerts();
                }
                return listAlertIdMappings;
            }
        }
        public AveSPWeb(AveSPSite _AveSite, string _name)
        {
            mAveSite = _AveSite;
            mAveSite.ReloadSite();

            mName = _name;
            mQueryService = mAveSite.QueryService;
            mIsNewCreated = mAveSite.IsNewCreated;
            mContentTypes = new AveSPWebContentTypeCollection(this);
            mFields = new AveSPWebFieldCollection(this);
        }

        public AveSPWeb(AveSPSite aveSite)
        {
            mAveSite = aveSite;
            //mParentAveWeb = aveSite.SPSite.RootWeb;
            //mQueryService = mAveSite.QueryService;
            //mFields = new AveSPWebFieldCollection(this);
            //mContentTypes = new AveSPWebContentTypeCollection(this);
            //mMembers = new AveSPMembers(mAveSite);
            mWebUrl = mAveSite.SPSite.RootWeb.Url;
            mSPWeb = mAveSite.SPSite.RootWeb;
            //mNavigation = new AveSPNavigation(this);
            //mQueryService = mAveSite.QueryService;
        }

        /// <summary>
        /// only used for wrapper test purpose
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="restoreStream"></param>
        /// <param name="folderRelativeUrl"></param>
        /// <param name="isRestoreFolder"></param>
        public AveSPWeb(AveSPSite aveSite, IAveRestoreStream restoreStream, string webUrl)
        {
            mAveSite = aveSite;
            mRestoreStream = restoreStream;
            mParentAveWeb = aveSite.SPSite.RootWeb;
            mQueryService = mAveSite.QueryService;
            mFields = new AveSPWebFieldCollection(this);
            mContentTypes = new AveSPWebContentTypeCollection(this);
            mMembers = new AveSPMembers(mAveSite);
            if (string.Equals(webUrl, "."))
            {
                webUrl = "/";
            }
            mWebUrl = webUrl;
            mSPWeb = mAveSite.SPSite.OpenWeb(webUrl);
            //mNavigation = new AveSPNavigation(this);
            mQueryService = mAveSite.QueryService;
        }

        /// <summary>
        /// used for content type hub init
        /// </summary>
        /// <param name="aveSite"></param>
        /// <param name="restoreStream"></param>
        /// <param name="webUrl"></param>
        public AveSPWeb(AveSPSite aveSite, string webUrl, bool option)
        {
            mAveSite = aveSite;
            mParentAveWeb = aveSite.SPSite.RootWeb;
            mQueryService = mAveSite.QueryService;
            mFields = new AveSPWebFieldCollection(this);
            mContentTypes = new AveSPWebContentTypeCollection(this);
            mMembers = new AveSPMembers(mAveSite);
            if (string.Equals(webUrl, "."))
            {
                webUrl = "/";
            }
            mWebUrl = webUrl;
            mSPWeb = mAveSite.SPSite.OpenWeb();
            //mNavigation = new AveSPNavigation(this);
            mQueryService = mAveSite.QueryService;
        }

        /// <summary>
        /// only used for wrapper test purpose
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="restoreStream"></param>
        /// <param name="folderRelativeUrl"></param>
        /// <param name="isRestoreFolder"></param>
        public AveSPWeb(AveSPWeb aveWeb, IAveRestoreStream restoreStream)
        {
            mParentAveWeb = aveWeb.ParentSite.SPSite.RootWeb;
            mRestoreStream = restoreStream;
            mSPWeb = aveWeb.SPWeb;
        }

        /// <summary>
        /// only used for wrapper test purpose
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="restoreStream"></param>
        /// <param name="folderRelativeUrl"></param>
        /// <param name="isRestoreFolder"></param>
        public AveSPWeb(AveSPSite aveSite, IAveRestoreStream restoreStream, string webUrl, bool isRestoreWeb)
            : this(aveSite, restoreStream, webUrl)
        {
            if (isRestoreWeb)
            {
                mParentAveWeb = mAveSite.SPSite.OpenWeb(mWebUrl);
            }
            else
            {
                mSPWeb = mAveSite.SPSite.OpenWeb(mWebUrl);
            }
        }

        public IAveFolder GetFolderByRelativeUrl(string relativeUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetFolderByRelativeUrl"))
            {
#endif
                string fullRelativeUrl = null;

                if (mSPWeb.ServerRelativeUrl.EndsWith(AveProtocolHeaderConstants.URL_SEPERATOR, StringComparison.OrdinalIgnoreCase))
                {
                    fullRelativeUrl = mSPWeb.ServerRelativeUrl + relativeUrl;
                }
                else
                {
                    fullRelativeUrl = mSPWeb.ServerRelativeUrl + AveProtocolHeaderConstants.URL_SEPERATOR + relativeUrl;
                }

                return mSPWeb.GetFolder(fullRelativeUrl);
#if PerformanceLog
            }
#endif
        }


        public void GetWebSelf()
        {
            if (mName == ".")
            {
                IAveWeb rootWeb = mAveSite.SPSite.RootWeb;
                mSPWeb = mAveSite.SPSite.OpenWeb(rootWeb.ID);
            }
            else
            {
                mSPWeb = GetWebInSite(mAveSite.SPSite, mName);
            }
        }
        /*private void AddToTopNavigationiBar(string title, string url)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.AddToTopNavigationiBar"))
            {
#endif
                AveNavigationNodeCreationInformation navigationNode = new AveNavigationNodeCreationInformation();
                navigationNode.Url = mSPWeb.ServerRelativeUrl;
                navigationNode.Title = title;
                navigationNode.AsLastNode = true;
                navigationNode.IsExternal = false;
                mParentAveWeb.Navigation.TopNavigationBar.Add(navigationNode);
#if PerformanceLog
            }
#endif
        }*/

        public void GetWebSelf(AveWebInfo sourceWebInfo)
        {
            mWebInfo = sourceWebInfo;
            mOldId = sourceWebInfo.OldWebId;
            mSrcLanguageId = sourceWebInfo.LCID;
            if (mName == ".")
            {
                IAveWeb rootWeb = mAveSite.SPSite.RootWeb;
                mSPWeb = mAveSite.SPSite.OpenWeb(rootWeb.ID);
            }
            else
            {
                mSPWeb = GetWebInSite(mAveSite.SPSite, mName);
            }
            InitializeMembers();

            log.Info($"Get web self, web url:{mSPWeb?.Url}, source web url: {sourceWebInfo.Url}");
        }

        private void InitializeMembers()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.InitializeMembers"))
            {
#endif
                mId = mSPWeb.ID;
                mScope = mSPWeb.ServerRelativeUrl.Substring(1);
#if PerformanceLog
            }
#endif
        }

        public void Dispose()
        {
            if (mSPWeb != null)
            {
                mSPWeb.Dispose();
                //mSPWeb = null;
            }
        }

        public void SetLanguageForNew(uint LCD)
        {
            mLanguageForNewCreatedWeb = LCD;
        }

        public void Restore()
        {

        }

        public void UpdateDocumentSetCT()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.UpdateDocumentSetCT"))
            {
#endif
                if (mAveSite.MappingManager.WebMappingManager.DocumentSetCTCache.Count > 0)
                {
                    IAveContentTypeCollection CTs = mSPWeb.ContentTypes;
                    foreach (AveContentTypeInfo ctInfo in mAveSite.MappingManager.WebMappingManager.DocumentSetCTCache)
                    {
                        IAveContentType ct = CTs[ctInfo.Name];
                        //Need to change restore option
                        AveSPDocumentSet ctDocumentSet = new AveSPDocumentSet(ctInfo, ct, this.SPWeb, new AveRestoreOption().mAveContentTypeRestoreOption.WEB_CONTENTTYPE_UPDATECHILD);
                        ctDocumentSet.Update();
                    }
                }

                mAveSite.MappingManager.WebMappingManager.DocumentSetCTCache.Clear();
#if PerformanceLog
            }
#endif
        }

        private void SetTitleAndDescriptionResource(IAveWeb web, AveWebSettingInfo webSettingInfo)
        {
            //to do:是否需要还原web title resource
            if (webSettingInfo.TitleResourceInfo != null && webSettingInfo.TitleResourceInfo.IsAvailable)
            {
                web.TitleResource.SetUserResource(web,webSettingInfo.TitleResourceInfo.Value, !this.IsNewCreated);
                web.TitleResource.Update();
            }
            if (webSettingInfo.DescriptionResourceInfo != null && webSettingInfo.DescriptionResourceInfo.IsAvailable)
            {
                web.DescriptionResource.SetUserResource(web, 
                    webSettingInfo.DescriptionResourceInfo.Value, !this.IsNewCreated);
                web.DescriptionResource.Update();
            }
        }

        public void RestoreWebProperty(AveWebSettingInfo webSettingInfo, bool isIncludeCustomPropertyBag = false)
        {
#if PerformaceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WebProperty"))
            {
#endif
            AssembleD5DataToD6(webSettingInfo);
            mWebSettingInfo = webSettingInfo;
            mIsRestoreWebSetting = true;
            //SAAS-25121,SAAS-27655 用于判断是否开启custom script
            bool isCustomScriptEnabled = !mSPWeb.Site.DenyAddAndCustomizePagesStatus;
            //bool isCustomScriptEnabled = !mSPWeb.Site.DenyAddAndCustomizePagesStatus || mSPWeb.Site.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled;
            log.Info($"Restore web properties, web url:{mSPWeb.Url}, enable custom script:{isCustomScriptEnabled}.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled:{mSPWeb.Site.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled}.");
            try
            {
                if (!isCustomScriptEnabled)
                {
                    log.Info(@$"Update site deny Add AndCustomize page status for edit properteis");
                    SetDenyAddAndCustomizePagesStatus(mSPWeb.Site, false);
                }
                //this is code is no longer used, use RestoreThemeCssFolderUrl function in post action instead
                //try
                //{
                //    if (mWebSettingInfo.Theme != null && mWebSettingInfo.Theme.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.Theme.Value))
                //    {
                //        mSPWeb.ApplyTheme(mWebSettingInfo.Theme.Value);
                //    }
                //}
                //catch (Exception e)
                //{
                //    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while applying web theme. WebId:{0}, WebUrl:{1}, WebTheme:{2}\n error message:{3}", mSPWeb.ID, mSPWeb.Url, mWebSettingInfo.Theme, e));
                //    //mLog.Warn(e, "An error occurred while applying web theme. WebId:{0}, WebUrl:{1}, WebTheme:{2}",
                //    //    mSPWeb.ID, mSPWeb.Url, mWebSettingInfo.Theme);
                //}
                #region add for web master page setting

                AveWebMasterPageInfo webPageInfo = new AveWebMasterPageInfo();
                if (mWebSettingInfo.InheritAlertCss != null && mWebSettingInfo.InheritAlertCss.IsAvailable)
                {
                    webPageInfo.Inheriting = mWebSettingInfo.InheritAlertCss.Value;
                }
                if (mWebSettingInfo.InheritAlertCssUrl != null && mWebSettingInfo.InheritAlertCssUrl.IsAvailable)
                {
                    webPageInfo.PageUrl = mWebSettingInfo.InheritAlertCssUrl.Value;
                }
                if (mWebSettingInfo.CInheriting != null && mWebSettingInfo.CInheriting.IsAvailable)
                {
                    webPageInfo.CInheriting = mWebSettingInfo.CInheriting.Value;
                }
                if (mWebSettingInfo.CPageUrl != null && mWebSettingInfo.CPageUrl.IsAvailable)
                {
                    webPageInfo.CPageUrl = mWebSettingInfo.CPageUrl.Value;
                }
                if (mWebSettingInfo.MInheriting != null && mWebSettingInfo.MInheriting.IsAvailable)
                {
                    webPageInfo.MInheriting = mWebSettingInfo.MInheriting.Value;
                }
                if (mWebSettingInfo.MPageUrl != null && mWebSettingInfo.MPageUrl.IsAvailable)
                {
                    webPageInfo.MPageUrl = mWebSettingInfo.MPageUrl.Value;
                }

                if (mWebSettingInfo.MasterUrl != null && mWebSettingInfo.MasterUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.MasterUrl.Value))
                {
                    webPageInfo.MPageUrl = ParseMasterUrl(mSPWeb.MasterUrl, mWebSettingInfo.MasterUrl.Value);
                    mWebSettingInfo.MasterUrl = webPageInfo.MPageUrl;//ADO-75209 restoretheme时候会用到。
                }

                if (mSPWeb.CustomMasterUrl != null && mWebSettingInfo.CustomMasterUrl != null && mWebSettingInfo.CustomMasterUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.CustomMasterUrl.Value))
                {
                    webPageInfo.CPageUrl = ParseMasterUrl(mSPWeb.CustomMasterUrl, mWebSettingInfo.CustomMasterUrl.Value);
                    mWebSettingInfo.CustomMasterUrl = webPageInfo.CPageUrl;
                }
                this.ParentSite.MappingManager.SiteMappingManager.WebMastPageMapping.Add(mSPWeb.ID, webPageInfo);

                #endregion

                if (mWebSettingInfo.AllowUnsafeUpdate != null && mWebSettingInfo.AllowUnsafeUpdate.IsAvailable)
                {
                    mSPWeb.AllowUnsafeUpdates = mWebSettingInfo.AllowUnsafeUpdate.Value;
                }
                if (mWebSettingInfo.QuickLaunchEnabled != null && mWebSettingInfo.QuickLaunchEnabled.IsAvailable)
                {
                    mSPWeb.QuickLaunchEnabled = mWebSettingInfo.QuickLaunchEnabled.Value;
                }

                if (mWebSettingInfo.Description != null && mWebSettingInfo.Description.IsAvailable)
                {
                    mSPWeb.Description = mWebSettingInfo.Description.Value;
                }
                if (mWebSettingInfo.Uiversion != null && mWebSettingInfo.Uiversion.IsAvailable)
                {
                    mSPWeb.UIVersion = mWebSettingInfo.Uiversion.Value;
                }
                if (mWebSettingInfo.UiversionConfigurationEnable != null && mWebSettingInfo.UiversionConfigurationEnable.IsAvailable)
                {
                    mSPWeb.UIVersionConfigurationEnabled = mWebSettingInfo.UiversionConfigurationEnable.Value;
                }
                /* add for version B */
                //SAAS-25121，SAAS-27655 此属性中的set方法，会同时将NoCrawl和ASPXPageIndexMode进行设置，如果不开启custom script，则无法更新NoCrawl
                if (!mSPWeb.Site.DenyAddAndCustomizePagesStatus && mWebSettingInfo.ExcludeFromOfflineClient != null && mWebSettingInfo.ExcludeFromOfflineClient.IsAvailable)
                {
                    mSPWeb.ExcludeFromOfflineClient = mWebSettingInfo.ExcludeFromOfflineClient.Value;
                }

                /* update Flags */
                /*The user interface for this Site displays a hierarchical "tree view" navigational element.*/
                if (mWebSettingInfo.TreeViewEnabled != null && mWebSettingInfo.TreeViewEnabled.IsAvailable)
                {
                    mSPWeb.TreeViewEnabled = mWebSettingInfo.TreeViewEnabled.Value;
                }

                /*This Site has disabled syndication of List Items via RSS.*/
                if (mWebSettingInfo.SyndicationEnabled != null && mWebSettingInfo.SyndicationEnabled.IsAvailable)
                {
                    mSPWeb.SyndicationEnabled = mWebSettingInfo.SyndicationEnabled.Value;
                }

                /*Document parsing is disabled for this Site.*/
                if (mWebSettingInfo.ParserEnabled != null && mWebSettingInfo.ParserEnabled.IsAvailable)
                {
                    mSPWeb.ParserEnabled = mWebSettingInfo.ParserEnabled.Value;
                }

                /*This Site allows display of implementation-specific User presence information in the user interface.*/
                if (mWebSettingInfo.PresenceEnabled != null && mWebSettingInfo.PresenceEnabled.IsAvailable)
                {
                    mSPWeb.PresenceEnabled = mWebSettingInfo.PresenceEnabled.Value;
                }

                /*The user interface for this Site displays the quick launch navigational element.*/

                //mSPWeb.QuickLaunchEnabled = AveWebFlags.IsDisplayQuickLaunchWeb(mWebSettingInfo.Flags);

                /*Search indexing agents can pages within this Site.*/
                //mSPWeb.AllowAutomaticASPXPageIndexing = !AveWebFlags.IsAutoAspxIndexModeWeb(mWebSettingInfo.Flags);
                if (mSPWeb.HasUniqueRoleAssignments && mRestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity))
                {
                    if (mWebSettingInfo.AnonymousState != null && mWebSettingInfo.AnonymousState.IsAvailable && (int)mSPWeb.AnonymousState != mWebSettingInfo.AnonymousState.Value)
                    {
                        mSPWeb.AnonymousState = (AveWebAnonymousState)mWebSettingInfo.AnonymousState.Value;
                    }
                }

                //if (mWebSettingInfo.ThemedColorUrl != null && mWebSettingInfo.ThemedColorUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedColorUrl.Value))
                //{
                //    mWebSettingInfo.ThemedColorUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedColorUrl.Value, mAveSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                //}
                //if (mWebSettingInfo.ThemedFontUrl != null && mWebSettingInfo.ThemedFontUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedFontUrl.Value))
                //{
                //    mWebSettingInfo.ThemedFontUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedFontUrl.Value, mAveSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                //}
                //if (mWebSettingInfo.ThemedImageUrl != null && mWebSettingInfo.ThemedImageUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedImageUrl.Value))
                //{
                //    mWebSettingInfo.ThemedImageUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedImageUrl.Value, mAveSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                //}

                //Restore Theme in post action
                if (mWebSettingInfo.ThemedCssFolderUrl != null && mWebSettingInfo.ThemedCssFolderUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedCssFolderUrl.Value))
                {
                    this.ThemedCssFolderUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedCssFolderUrl.Value, mAveSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                }

                //if (mWebSettingInfo.InheritsThemedCssFolderUrl != null && mWebSettingInfo.InheritsThemedCssFolderUrl.IsAvailable)
                //{
                //    if (mWebSettingInfo.InheritsThemedCssFolderUrl.Value)
                //    {
                //        if (!mSPWeb.IsRootWeb)
                //        {
                //            mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = "True";
                //        }
                //    }
                //    else
                //    {
                //        mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = "False";
                //    }
                //}

                //Modern look and feel
                if (mWebSettingInfo.ThemedCssFolderUrl != null && mWebSettingInfo.ThemedCssFolderUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedCssFolderUrl.Value))
                {
                    this.ThemedCssFolderUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedCssFolderUrl.Value, mAveSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                }
                if (mWebSettingInfo.ModernThemeInfo != null && mWebSettingInfo.ModernThemeInfo.IsAvailable && mWebSettingInfo.ModernThemeInfo.Value != null)
                {
                    //Replace the URL of theme related files
                    AveModernThemeInfo modernThemeInfo = mWebSettingInfo.ModernThemeInfo.Value;
                    if (!string.IsNullOrEmpty(modernThemeInfo.ThemedColorUrl))
                    {
                        modernThemeInfo.ThemedColorUrl = AveReplaceProcessor.UrlReplace(modernThemeInfo.ThemedColorUrl, mAveSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                    }
                    if (!string.IsNullOrEmpty(modernThemeInfo.ThemedFontUrl))
                    {
                        modernThemeInfo.ThemedFontUrl = AveReplaceProcessor.UrlReplace(modernThemeInfo.ThemedFontUrl, mAveSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                    }
                    if (!string.IsNullOrEmpty(modernThemeInfo.ThemedImageUrl))
                    {
                        modernThemeInfo.ThemedImageUrl = AveReplaceProcessor.UrlReplace(modernThemeInfo.ThemedImageUrl, mAveSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                    }
                }
                if (mWebSettingInfo.HeaderEmphasis != null && mWebSettingInfo.HeaderEmphasis.IsAvailable)
                {
                    mSPWeb.HeaderEmphasis = (AveSPVariantThemeType)mWebSettingInfo.HeaderEmphasis.Value;
                }
                if (mWebSettingInfo.HeaderLayout != null && mWebSettingInfo.HeaderLayout.IsAvailable)
                {
                    mSPWeb.HeaderLayout = (AveHeaderLayoutType)mWebSettingInfo.HeaderLayout.Value;
                }
                if (mWebSettingInfo.MegaMenuEnabled != null && mWebSettingInfo.HeaderEmphasis.IsAvailable)
                {
                    mSPWeb.MegaMenuEnabled = mWebSettingInfo.MegaMenuEnabled.Value;
                }

                if (mWebSettingInfo.AlternateCSSUrl != null && mWebSettingInfo.AlternateCSSUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.AlternateCSSUrl.Value))
                {
                    this.AlternateCSSUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.AlternateCSSUrl.Value, mAveSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                }
                //SAAS-25121，SAAS-27655 如果不开启custom script，则不能更新此属性
                if (!mSPWeb.Site.DenyAddAndCustomizePagesStatus && mWebSettingInfo.ASPXPageIndexMode != null && mWebSettingInfo.ASPXPageIndexMode.IsAvailable)
                {
                    mSPWeb.ASPXPageIndexMode = (AveWebASPXPageIndexMode)mWebSettingInfo.ASPXPageIndexMode.Value;
                    if (mSPWeb.ASPXPageIndexMode.Equals(AveWebASPXPageIndexMode.Automatic))
                    {
                        mSPWeb.AllowAutomaticASPXPageIndexing = true;
                    }
                }
                if (mWebSettingInfo.OverwriteTranslationsOnChange != null && mWebSettingInfo.OverwriteTranslationsOnChange.IsAvailable)
                {
                    mSPWeb.OverwriteTranslationsOnChange = mWebSettingInfo.OverwriteTranslationsOnChange.Value;
                }
                /* update MetaInfo */
                //SAAS-25121，SAAS-27655 如果不开启custom script，则没有权限更新metaInfo

                if (!mSPWeb.Site.DenyAddAndCustomizePagesStatus && mWebSettingInfo.MetaInfo != null && mWebSettingInfo.MetaInfo.IsAvailable && mWebSettingInfo.MetaInfo.Value != null)
                {
                    RestoreMetaInfo(Encoding.UTF8.GetString(mWebSettingInfo.MetaInfo.Value), isIncludeCustomPropertyBag);
                }
                if (mWebSettingInfo.NavigationWebAndPage != null && mWebSettingInfo.NavigationWebAndPage.IsAvailable)
                {
                    getWebsAndPages(mWebSettingInfo.NavigationWebAndPage.Value);
                }
                /* update audit setting log report location*/
                //SAAS-25121，SAAS-27655 如果不开启custom script，则没有权限更新AllProperties
                if (!mSPWeb.Site.DenyAddAndCustomizePagesStatus && mWebSettingInfo.SettingTypes != null && ((AveWebSettingTypes)mWebSettingInfo.SettingTypes.Value & AveWebSettingTypes.SiteAuditSettings) == AveWebSettingTypes.SiteAuditSettings)
                {
                    if (mSPWeb.IsRootWeb)
                    {
                        if (mWebSettingInfo.AuditLogReportStorageLocation == null || string.IsNullOrEmpty(mWebSettingInfo.AuditLogReportStorageLocation.Value))
                        {
                            mSPWeb.AllProperties["_auditlogreportstoragelocation"] = "";
                        }
                        else
                        {
                            mSPWeb.AllProperties["_auditlogreportstoragelocation"] = mSPWeb.ServerRelativeUrl + mWebSettingInfo.AuditLogReportStorageLocation;
                        }
                    }
                }
                /* update region setting */
                if (mSPWeb.RegionalSettings != null && mWebSettingInfo.SettingTypes != null && ((AveWebSettingTypes)mWebSettingInfo.SettingTypes.Value & AveWebSettingTypes.SiteRegionalSettings) == AveWebSettingTypes.SiteRegionalSettings)
                {
                    if (mWebSettingInfo.AltCalendarType != null && mWebSettingInfo.AltCalendarType.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.AlternateCalendarType = mWebSettingInfo.AltCalendarType.Value.HasValue ? mWebSettingInfo.AltCalendarType.Value.Value : AveWebsTableColumnValue.AlternateCalendarType;
                    }
                    if (mWebSettingInfo.CalendarType != null && mWebSettingInfo.CalendarType.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.CalendarType = mWebSettingInfo.CalendarType.Value.HasValue ? mWebSettingInfo.CalendarType.Value.Value : AveWebsTableColumnValue.CalendarType;
                        if (mWebSettingInfo.CalendarType.Value.Value == 6 && mWebSettingInfo.AdjustHijriDays != null && mWebSettingInfo.AdjustHijriDays.IsAvailable)
                        {
                            mSPWeb.RegionalSettings.AdjustHijriDays = mWebSettingInfo.AdjustHijriDays.Value.HasValue ? mWebSettingInfo.AdjustHijriDays.Value.Value : AveWebsTableColumnValue.AdjustHijriDays;
                        }
                    }
                    if (mWebSettingInfo.Collation != null && mWebSettingInfo.Collation.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.Collation = mWebSettingInfo.Collation.Value;
                    }
                    if (mWebSettingInfo.WorkDayStartHour != null && mWebSettingInfo.WorkDayStartHour.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.WorkDayStartHour = mWebSettingInfo.WorkDayStartHour.Value.HasValue ? mWebSettingInfo.WorkDayStartHour.Value.Value : AveWebsTableColumnValue.WorkDayStartHour;
                    }
                    if (mWebSettingInfo.WorkDayEndHour != null && mWebSettingInfo.WorkDayEndHour.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.WorkDayEndHour = mWebSettingInfo.WorkDayEndHour.Value.HasValue ? mWebSettingInfo.WorkDayEndHour.Value.Value : AveWebsTableColumnValue.WorkDayEndHour;
                    }
                    if (mWebSettingInfo.WorkDays != null && mWebSettingInfo.WorkDays.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.WorkDays = mWebSettingInfo.WorkDays.Value.HasValue ? mWebSettingInfo.WorkDays.Value.Value : AveWebsTableColumnValue.WorkDays;
                    }

                    if (mWebSettingInfo.Time24 != null && mWebSettingInfo.Time24.IsAvailable && mWebSettingInfo.Time24.Value.HasValue)
                    {
                        mSPWeb.RegionalSettings.Time24 = mWebSettingInfo.Time24.Value.Value;
                    }
                    //不可以覆盖该属性，该属性控制目的端的语言，如果要覆盖，请和Wrapper Team联系下，谢谢。
                    if ((IsNewCreated || mAveSite.IsGAORunningJob) && mWebSettingInfo.LocaleId != null && mWebSettingInfo.LocaleId.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.LocaleId = (uint)mWebSettingInfo.LocaleId.Value;
                    }
                    if (mWebSettingInfo.TimeZone != null && mWebSettingInfo.TimeZone.IsAvailable)
                    {
                        mSPWeb.RegionalSettings.TimeZone.ID = (ushort)mWebSettingInfo.TimeZone.Value;
                    }

                    if (mWebSettingInfo.CalendarViewOptions != null && mWebSettingInfo.CalendarViewOptions.IsAvailable && mWebSettingInfo.CalendarViewOptions.Value.HasValue)
                    {
                        try
                        {
                            mSPWeb.RegionalSettings.FirstWeekOfYear = (short)((mWebSettingInfo.CalendarViewOptions.Value & 0x1F) >> 3);

                            uint firstDayOfWeek = (uint)(mWebSettingInfo.CalendarViewOptions.Value & 0x07);
                            if (firstDayOfWeek < 0 || firstDayOfWeek > 6)
                            {
                                firstDayOfWeek = mSPWeb.RegionalSettings.FirstDayOfWeek;
                            }
                            mSPWeb.RegionalSettings.FirstDayOfWeek = firstDayOfWeek;
                            mSPWeb.RegionalSettings.ShowWeeks = (int)(mWebSettingInfo.CalendarViewOptions.Value & 0x20) != 0 ? true : false;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while set web regionalSettings. error:{0}", e.ToString());
                            //mLog.Warn("An error occurred while set web regionalSettings. error:{0}", e.ToString());
                        }
                    }
                    else
                    {
                        mSPWeb.RegionalSettings.FirstWeekOfYear = 0;
                        mSPWeb.RegionalSettings.FirstDayOfWeek = 0;
                        mSPWeb.RegionalSettings.ShowWeeks = false;
                    }
                }
                //language setting restore
                if (mWebSettingInfo.SupportedUICultures != null && mWebSettingInfo.SupportedUICultures.IsAvailable)
                {
                        try
                        {
                            //IAveWebTemplateCollection webTemplates = mSPWeb.Site.GetWebTemplates(mSPWeb.Language);
                            //IAveWebTemplate template = webTemplates[mSPWeb.WebTemplate];
                            StringBuilder tracertLogBuilder = new StringBuilder();
                            tracertLogBuilder.AppendFormat("SourceWeb {0} Language Culture Info:DefaultLanguage:{1},Alternate language(s):", WebInfo.Url, WebInfo.LCID);
                            if (mWebSettingInfo.SupportedUICultures.Value.Count > 0)
                            {
                                mSPWeb.AddSupportedUICulture((List<int>)mWebSettingInfo.SupportedUICultures.Value);
                                tracertLogBuilder.AppendFormat("{0},", mWebSettingInfo.SupportedUICultures.Value);
                            }
                            log.Info(tracertLogBuilder.ToString());
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occured when restore web language setting.Error Message:{0}.", e.Message.ToString());
                        }
                }

                if (mWebSettingInfo.IsMultilingual != null && mWebSettingInfo.IsMultilingual.IsAvailable)
                {
                    mSPWeb.IsMultilingual = mWebSettingInfo.IsMultilingual.Value;
                }
                if (mWebSettingInfo.OverwriteTranslationsOnChange != null && mWebSettingInfo.OverwriteTranslationsOnChange.IsAvailable)
                {
                    mSPWeb.OverwriteTranslationsOnChange = mWebSettingInfo.OverwriteTranslationsOnChange.Value;
                }
                if (mWebSettingInfo.SiteLogoDescription != null && mWebSettingInfo.SiteLogoDescription.IsAvailable)
                {
                    mSPWeb.SiteLogoDescription = mWebSettingInfo.SiteLogoDescription.Value;
                }

                if (mWebSettingInfo.UserSharedNav != null && mWebSettingInfo.UserSharedNav.IsAvailable && !mSPWeb.IsRootWeb && mSPWeb.Navigation.UseShared != mWebSettingInfo.UserSharedNav.Value)
                {
                    mSPWeb.Update();
                    ReloadWeb();
                    mSPWeb.Navigation.UseShared = mWebSettingInfo.UserSharedNav.Value;
                }

                if (mWebSettingInfo.TimeCreated != null && mWebSettingInfo.TimeCreated.IsAvailable)
                {
                    mSPWeb.Created = mWebSettingInfo.TimeCreated.Value;
                }
                if (mWebSettingInfo.LastItemModifiedDate != null && mWebSettingInfo.LastItemModifiedDate.IsAvailable && mWebSettingInfo.LastItemModifiedDate.Value != DateTime.MinValue)
                {
                    if (!mAveSite.MappingManager.SiteMappingManager.UnRestoreWebLastModifiedTime.ContainsKey(mSPWeb.ID))
                    {
                        mAveSite.MappingManager.SiteMappingManager.UnRestoreWebLastModifiedTime.Add(mSPWeb.ID, mWebSettingInfo.LastItemModifiedDate.Value);
                    }
                }

                //更新以上一些属性值会影响到Title的赋值，放到此处更新
                if (mWebSettingInfo.Title != null && mWebSettingInfo.Title.IsAvailable)
                {
                    mSPWeb.Title = mWebSettingInfo.Title.Value;
                }
                mSPWeb.Update();

                SetTitleAndDescriptionResource(mSPWeb, mWebSettingInfo);

                //if (mSPWeb.Site.WebApplication != null && mSPWeb.Site.WebApplication.OutboundMailServiceInstance != null && mSPWeb.HasUniqueRoleAssignments)
                //{
                //    try
                //    {
                //        if (mWebSettingInfo.RequestAccessEmail != null && mWebSettingInfo.RequestAccessEmail.IsAvailable)
                //        {
                //            mSPWeb.RequestAccessEmail = mWebSettingInfo.RequestAccessEmail.Value;
                //            this.ReloadWeb();
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        report.AddDetail(new AveWrapperReportDto(webSettingInfo.Title.Value, webSettingInfo.Title.Value
                //    , AveReportObjectType.WebProperty, AveStatus.Skipped, string.Format("An error occurred while set web RequestAccessEmail. error:{0}", e.Message)));
                //        log.Warn("An error occurred while set web RequestAccessEmail. error:{0}", e.ToString());
                //        this.ReloadWeb();
                //    }
                //}

                if (webSettingInfo.ThemedColorUrl != null && webSettingInfo.ThemedColorUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedColorUrl.Value))
                {
                    if (webSettingInfo.ThemedColorUrl.Value.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) >= 0) //migrate a subsite under a top sitecollection to a subsite which is not under top sitecollection, theme will not be restored correctly
                    {
                        webSettingInfo.ThemedColorUrl = AveReplaceProcessor.UrlReplace(webSettingInfo.ThemedColorUrl.Value, ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                    }
                    else
                    {
                        webSettingInfo.ThemedColorUrl = AveReplaceProcessor.UrlReplace(webSettingInfo.ThemedColorUrl.Value, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                    }
                }
                if (webSettingInfo.ThemedFontUrl != null && webSettingInfo.ThemedFontUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedFontUrl.Value))
                {
                    if (webSettingInfo.ThemedColorUrl.Value.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        webSettingInfo.ThemedFontUrl = AveReplaceProcessor.UrlReplace(webSettingInfo.ThemedFontUrl.Value, ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                    }
                    else
                    {
                        webSettingInfo.ThemedFontUrl = AveReplaceProcessor.UrlReplace(webSettingInfo.ThemedFontUrl.Value, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                    }
                }
                if (webSettingInfo.ThemedImageUrl != null && webSettingInfo.ThemedImageUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedImageUrl.Value))
                {
                    if (webSettingInfo.ThemedColorUrl.Value.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        webSettingInfo.ThemedImageUrl = AveReplaceProcessor.UrlReplace(webSettingInfo.ThemedImageUrl.Value, ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                    }
                    else
                    {
                        webSettingInfo.ThemedImageUrl = AveReplaceProcessor.UrlReplace(webSettingInfo.ThemedImageUrl.Value, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while updating web property. WebId:{0}, WebUrl:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, ex));
                report.AddDetail(new AveWrapperReportDto(webSettingInfo.Title.Value, webSettingInfo.Title.Value
                    , AveReportObjectType.WebProperty, AveStatus.Skipped, "You don't have permission to restore web setting" + ex.Message));
            }
            catch (Exception e)
            {
                report.AddDetail(new AveWrapperReportDto(webSettingInfo.Title.Value, webSettingInfo.Title.Value
                    , AveReportObjectType.WebProperty, AveStatus.Skipped, string.Format("An error occurred while updating web property. WebId:{0}, WebUrl:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, e.Message)));
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while updating web property. WebId:{0}, WebUrl:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, e));
                //mLog.Warn(e, "An error occurred while updating web property. WebId:{0}, WebUrl:{1}", mSPWeb.ID, mSPWeb.Url);
            }
            finally
            {
                if (!isCustomScriptEnabled)
                {
                    log.Info(@$"Restore site deny Add AndCustomize page status after finish edit properteis");
                    SetDenyAddAndCustomizePagesStatus(mSPWeb.Site, true);
                }
            }
#if PerformaceLog
            }
#endif

            mSPWeb.ReloadWeb();
        }

        private void SetDenyAddAndCustomizePagesStatus(IAveSite site, bool enableStatus)
        {
            try
            {
                site.DenyAddAndCustomizePagesStatus = enableStatus;
            }
            catch (Exception e)
            {
                log.Error($"Fail Set Deny Add And Customize Pages Status,ex:{e}");
            }
        }

        private void AssembleD5DataToD6(AveWebSettingInfo webSettingInfo)
        {
            if (webSettingInfo.Flags != null && webSettingInfo.Flags.IsAvailable && webSettingInfo.Flags.Value > 0)
            {
                webSettingInfo.TreeViewEnabled = AveWebFlags.IsDiplayTreeViewWeb(webSettingInfo.Flags.Value);
                webSettingInfo.SyndicationEnabled = !AveWebFlags.IsDisableViaRssWeb(webSettingInfo.Flags.Value);
                webSettingInfo.ParserEnabled = !AveWebFlags.IsDocumentParseDiableWeb(webSettingInfo.Flags.Value);
                webSettingInfo.PresenceEnabled = AveWebFlags.IsDisplayUserPresenceInfoWeb(webSettingInfo.Flags.Value);
            }
        }

        /// <summary>
        /// Check 目的端语言包.
        /// </summary>
        /*private bool CheckLanguageIsInstalled(int lcid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.CheckLanguageIsInstalled"))
            {
#endif
                IAveRegionalSettings regionalSettings = mAveSite.ObjectModelFactory.CreateRegionalSettings();
                IAveLanguageCollection installedLanguages = regionalSettings.GlobalInstalledLanguages;
                foreach (IAveLanguage laguage in installedLanguages)
                {
                    if (lcid == laguage.LCID)
                    {
                        return true;
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }*/

        private void getWebsAndPages(Dictionary<string, Dictionary<string, Dictionary<Guid, string>>> navigationWebAndPage)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.getWebsAndPages"))
            {
#endif
                Dictionary<string, string> tempAllSubWebsAndPages = new Dictionary<string, string>();
                mAveSite.MappingManager.SiteMappingManager.AllSubWebsAndPagesMapping.Add(mSPWeb.ID, tempAllSubWebsAndPages);
                foreach (string hd in navigationWebAndPage.Keys)
                {
                    foreach (string type in navigationWebAndPage[hd].Keys)
                    {
                        foreach (Guid id in navigationWebAndPage[hd][type].Keys)
                        {
                            try
                            {
                                mAveSite.MappingManager.SiteMappingManager.AllSubWebsAndPagesMapping[mSPWeb.ID].Add(id.ToString(), navigationWebAndPage[hd][type][id]);
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetWebsAndPagesFailed, ex);
                            }
                        }
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private static string ConvertDictionary(IDictionary dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                var builder = new StringBuilder();
                foreach (DictionaryEntry item in dictionary)
                {
                    builder.Append(item.Key);
                    builder.Append(':');
                    builder.Append(item.Value);
                    builder.AppendLine();
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        public void UpdateDesignManagerViewSetting()
        {
            try
            {
                log.Info($"Begin to update Design Manager View Setting for web {mSPWeb.Url}");
                if (SPWeb.Site.DenyAddAndCustomizePagesStatus)
                {
                    log.Warn("Skip post update design manager view property as site script is disabled on current site collection.");
                    return;
                }
                var metaInfoBytes = mWebSettingInfo?.MetaInfo?.Value;
                if (metaInfoBytes == null)
                {
                    log.Warn("Skip post update design manager view property as metaInfoBytes is null.");
                    return;
                }
                var metaInfoString = Encoding.UTF8.GetString(mWebSettingInfo.MetaInfo.Value);
                if (string.IsNullOrEmpty(metaInfoString))
                {
                    log.Warn("Skip post update design manager view property as metaInfoString is empty.");
                    return;
                }
                Dictionary<string, string> needUpdateProperties = new Dictionary<string, string>();
                var metaInfoDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);
                var viewMapping = ParentSite.MappingManager.SiteMappingManager.ViewGuidMapping;
                foreach (var name in DesignManagerViewPropertyNames)
                {
                    if (!mSPWeb.AllProperties.ContainsKey(name))
                    {
                        log.Info($"Skip update DesinManager ViewSetting.Name:{name} as target site does not have this property.");
                    }
                    if (metaInfoDic.TryGetValue(name, out string viewIdString)
                        && Guid.TryParse(viewIdString, out Guid viewId)
                        && viewMapping.TryGetValue(viewId, out Guid newViewId))
                    {

                        needUpdateProperties.Add(name, newViewId.ToString());
                        log.Info($"Update Design Manager View Setting.Name:{name},SourceValue:{viewId},TargetOriginalValue:{mSPWeb.AllProperties[name]},TargetNewValue:{newViewId}");
                    }
                    else
                    {
                        log.Warn($"Web Property {name} does not exist in source setting or is not a valid view id,value:{viewIdString}.");
                    }
                }
                log.Warn("Commit Design Manager View Setting begins.");
                foreach (var key in needUpdateProperties.Keys)
                {
                    mSPWeb.AllProperties[key] = needUpdateProperties[key];
                }
                mSPWeb.Update();
                log.Info("Finish update Design Manager View Setting.");
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while update design manager view setting.Error:{0}",ex);
            }
        }

        // TODO:Restore other meta info that need restore
        //NOTE:现在全部都只是rootweb上的metainfo还原，后面还会有很多subsite的metainfo还原。
        //只需找到对应的属性，并在if中去掉 mSPWeb.IsRootWeb的条件即可
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPRSAccessibleTablix is a key")]
        private void RestoreMetaInfo(string metaInfoString, bool isIncludeCustomPropertyBag)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreMetaInfo"))
            {
#endif
                var restoredProperties = new List<string>();
                Dictionary<string, object> metaInfoDictionaryWithType = null;
                try
                {
                    if (String.IsNullOrEmpty(metaInfoString) || mSPWeb.AllProperties == null)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("MetaInfoString is Empty."));
                        //mLog.Error("metaInfoString is Empty");
                        return;
                    }
                    MetaInfoDictionary = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);
                    metaInfoDictionaryWithType = this.GetMetaInfoDicWithType(metaInfoString);
                    if (MetaInfoDictionary == null)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("After function GetWebMetaInfoDictionary,the metaInfoDictionary is null. metaInfoString:{0}", metaInfoString));
                        //mLog.Error("After function GetWebMetaInfoDictionary,the metainfodictionary is null " + metaInfoString);
                        throw new Exception(string.Format("After function GetWebMetaInfoDictionary,the metaInfoDictionary is null. metaInfoString:{0}", metaInfoString));
                    }

                    log.Info("start to restore metainfo, source:{0}, destination:{1}", ConvertDictionary(MetaInfoDictionary), ConvertDictionary(mSPWeb.AllProperties));

                    #region for 07 - 10 migration. 转换07和10的属性
                    var sp07Property = new string[] { "__IncludeSubSitesInNavigation", "__IncludePagesInNavigation" };
                    restoredProperties.AddRange(sp07Property);
                    int navigationIncludeTypes = 0;
                    if ((bool?)MetaInfoDictionary?.ContainsKey("__IncludeSubSitesInNavigation") ?? false && !MetaInfoDictionary.ContainsKey("__IncludePagesInNavigation"))
                    {
                        if (mSPWeb.Features[new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb")] != null && mSPWeb.IsPublish)
                        {
                            navigationIncludeTypes |= 2;
                        }
                    }
                    if (MetaInfoDictionary.ContainsKey("__IncludeSubSitesInNavigation")
                        && MetaInfoDictionary["__IncludeSubSitesInNavigation"].Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        navigationIncludeTypes |= 1;
                    }
                    if (MetaInfoDictionary.ContainsKey("__IncludePagesInNavigation")
                        && MetaInfoDictionary["__IncludePagesInNavigation"].Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        navigationIncludeTypes |= 2;
                    }
                    if (navigationIncludeTypes > 0)
                    {
                        MetaInfoDictionary["__GlobalNavigationIncludeTypes"] = navigationIncludeTypes.ToString();
                        MetaInfoDictionary["__CurrentNavigationIncludeTypes"] = navigationIncludeTypes.ToString();
                    }
                    else if (MetaInfoDictionary.ContainsKey("__IncludeSubSitesInNavigation")
                        && MetaInfoDictionary["__IncludeSubSitesInNavigation"].Equals("false", StringComparison.OrdinalIgnoreCase)
                        && MetaInfoDictionary.ContainsKey("__IncludePagesInNavigation")
                        && MetaInfoDictionary["__IncludePagesInNavigation"].Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        MetaInfoDictionary["__GlobalNavigationIncludeTypes"] = navigationIncludeTypes.ToString();
                        MetaInfoDictionary["__CurrentNavigationIncludeTypes"] = navigationIncludeTypes.ToString();
                    }
                    #endregion

                    if (MetaInfoDictionary.ContainsKey("__InheritsThemedCssFolderUrl") && !mSPWeb.IsRootWeb)
                    {
                        mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = MetaInfoDictionary["__InheritsThemedCssFolderUrl"];
                        if (mWebSettingInfo.WebTheme != null && mWebSettingInfo.WebTheme.Value != null)
                        {
                            mWebSettingInfo.WebTheme.Value.InheritsThemedCssFolderUrl = Convert.ToBoolean(MetaInfoDictionary["__InheritsThemedCssFolderUrl"]);
                        }
                        restoredProperties.Add("__InheritsThemedCssFolderUrl");
                    }
                    else if (!mSPWeb.IsRootWeb && mSPWeb.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && bool.TrueString.Equals(mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] as string, StringComparison.OrdinalIgnoreCase))
                    {
                        mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = bool.FalseString;
                        if (mWebSettingInfo.WebTheme != null && mWebSettingInfo.WebTheme.Value != null)
                        {
                            mWebSettingInfo.WebTheme.Value.InheritsThemedCssFolderUrl = false;
                        }
                        restoredProperties.Add("__InheritsThemedCssFolderUrl");
                    }
                    else
                    {
                        MetaInfoDictionary.Remove("__InheritsThemedCssFolderUrl");
                    }

                    if (MetaInfoDictionary.ContainsKey("_routermanageremail") && !string.IsNullOrEmpty(MetaInfoDictionary["_routermanageremail"]))  //SAAS-10575 更新属性“_routermanageremail”即为Content Organizer Settings->Rule Managers User的信息。
                    {
                        string[] userLoginNames = MetaInfoDictionary["_routermanageremail"].Split(',');
                        StringBuilder userLogonNames = new StringBuilder();
                        foreach (string userLoginName in userLoginNames)
                        {
                            string value = ParentSite.SPMembers.GetMappingUserLogin(userLoginName);
                            if (!string.IsNullOrEmpty(value))
                            {
                                userLogonNames.AppendFormat("{0},", value.Substring(value.LastIndexOf(':') + 1));
                            }
                        }
                        MetaInfoDictionary["_routermanageremail"] = userLogonNames.ToString().TrimEnd(',');
                    }

                    #region 对一些知道的websetting比如RssSetting，如果源端为空，目的端存在，也要将目的端的清空，保持与源端一致//SRCH_ENH_FTR_URL这个属性也要和源端保持一致
                    string[] mMetaNameNeedExactlySame = new string[] { "vti_rss_Copyright", "vti_rss_ManagingEditor", "vti_rss_WebMaster", "vti_rss_FeedFormat" //这5个属性为RSSsetting服务
                    , "SRCH_ENH_FTR_URL"};
                    foreach (string mStrKey in mMetaNameNeedExactlySame)
                    {
                        if ((!MetaInfoDictionary.ContainsKey(mStrKey)) || MetaInfoDictionary[mStrKey].ToString().Equals(string.Empty))
                        {
                            mSPWeb.AllProperties[mStrKey] = string.Empty;
                        }
                    }
                    //"vti_rss_TimeToLive"单独处理，如果目的端设置清空，value为-1，如果dictionary中不包含该值，表明系统采用了默认设置，需要将目的端包含的移除，其他的可以直接赋值
                    if (MetaInfoDictionary.ContainsKey("vti_rss_TimeToLive"))
                    {
                        mSPWeb.AllProperties["vti_rss_TimeToLive"] = MetaInfoDictionary["vti_rss_TimeToLive"];

                    }
                    else if (mSPWeb.AllProperties.ContainsKey("vti_rss_TimeToLive"))
                    {
                        mSPWeb.AllProperties.Remove("vti_rss_TimeToLive");
                    }

                    #endregion

                    #region 有一些属性在还原其他设置时就会自动为其赋值，在此不需要再进行还原
                    var metaNameNotNeedRestore = new string[] { "emailsubmittedrecordslistid" };
                    restoredProperties.AddRange(metaNameNotNeedRestore);

                    #endregion

                    #region Other Properties
                    var metaNameNeedRestore = new string[] { "vti_rss_Copyright","vti_rss_ManagingEditor","vti_rss_WebMaster","vti_rss_TimeToLive","vti_rss_FeedFormat",//The five properties restore  Web>Rss>Advanced Settings
                                                               "disabledhelpcollections", "enabledhelpcollections","_auditlogreportstoragelocation",
                                                               "SRCH_ENH_FTR_URL","taxonomyhiddenlist","SRCH_TRAGET_RESULTS_PAGE",//"EnforceNewListingForSites","SiteDirectoryEntryRequirements, "//this two properties will throw exception while do 07 to 10 migration
                                                               //"vti_associategroups",
                                                               "SRCH_ENH_FTR_URL_WEB", "SRCH_SB_SET_WEB", "SRCH_VERT_SET_WEB",   //web search settings
                                                               "SRCH_ENH_FTR_URL_SITE", "SRCH_SB_SET_SITE",   //site search settings
                                                               "vti_CommunityEnableAutoApproval", "vti_CommunityEnableReportAbuse", "vti_CommunityEstablishedDate",  //Community Settings
                                                               "ms-blogs-skinid",//blog skin property
                                                               "discoverycasestatistics"//eDiscovery Center cases
                                                             };

                    restoredProperties.AddRange(metaNameNeedRestore);
                    List<string> urlKeysNeededToBeMapped = new List<string>() { "SRCH_ENH_FTR_URL_WEB", "SRCH_SB_SET_WEB", "SRCH_VERT_SET_WEB",
                                                                                "SRCH_ENH_FTR_URL_SITE", "SRCH_SB_SET_SITE", 
                                                                                "SRCH_ENH_FTR_URL",
                                                                                "_AUDITLOGREPORTSTORAGELOCATION",
                                                                                "SRCH_TRAGET_RESULTS_PAGE"};

                    foreach (string mStrKey in metaNameNeedRestore)
                    {
                        try
                        {
                            if (mStrKey.Equals("__InheritCurrentNavigation", StringComparison.OrdinalIgnoreCase) && !MetaInfoDictionary.ContainsKey("__InheritCurrentNavigation"))
                            {
                                mSPWeb.AllProperties["__InheritCurrentNavigation"] = "False";
                            }

                            if (MetaInfoDictionary.ContainsKey(mStrKey))
                            {
                                if (mStrKey.Equals("taxonomyhiddenlist", StringComparison.OrdinalIgnoreCase) && !MetaInfoDictionary[mStrKey].StartsWith("$", StringComparison.OrdinalIgnoreCase))
                                {
                                    mTaxonomyHiddenList = new Guid(MetaInfoDictionary[mStrKey]);
                                    continue;
                                }
                                else if (urlKeysNeededToBeMapped.Contains(mStrKey.ToUpper()) && !string.IsNullOrEmpty(MetaInfoDictionary[mStrKey]))
                                {
                                    if (mStrKey.Equals("SRCH_SB_SET_SITE", StringComparison.OrdinalIgnoreCase) 
                                        || mStrKey.Equals("SRCH_ENH_FTR_URL_SITE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mAveSite.MappingManager.SiteMappingManager.UrlNeedPostAction.Add(mStrKey, MetaInfoDictionary[mStrKey].ToString());
                                        continue;
                                    }
                                    if (!mStrKey.Equals("SRCH_ENH_FTR_URL_WEB", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Uri uri = new Uri(MetaInfoDictionary[mStrKey], UriKind.RelativeOrAbsolute);
                                        if (uri.IsAbsoluteUri)
                                        {
                                            mAveSite.MappingManager.SiteMappingManager.UrlNeedPostAction.Add(mStrKey, MetaInfoDictionary[mStrKey].ToString());
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        MetaInfoDictionary[mStrKey] = AveReplaceProcessor.UrlReplace(MetaInfoDictionary[mStrKey], mAveSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.mAveSite.SourceSiteInfo, this.ServerRelativeUrl);
                                    }
                                }
                                if (mStrKey.Equals("vti_CommunityEstablishedDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    mSPWeb.AllProperties[mStrKey] = DateTime.Parse(MetaInfoDictionary[mStrKey]);
                                    continue;
                                }
                                mSPWeb.AllProperties[mStrKey] = MetaInfoDictionary[mStrKey].Replace("\\r\\n", "\r\n");
                            }
                            else if (mStrKey.Equals("SRCH_ENH_FTR_URL_WEB", StringComparison.OrdinalIgnoreCase))
                            {
                                mSPWeb.AllProperties[mStrKey] = null;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while update web metainfo. property name:{0}\n error message:{1}", mStrKey, e));
                        }
                    }

                    //Dictionary<string, List<string>> propertiesForSpecialTemplates = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    //if (propertiesForSpecialTemplates.ContainsKey(this.SPWeb.WebTemplate))
                    //{
                    //    foreach (string propertyName in propertiesForSpecialTemplates[this.SPWeb.WebTemplate])
                    //    {
                    //        mSPWeb.AllProperties[propertyName] = MetaInfoDictionary[propertyName];
                    //    }
                    //}

                    #endregion

                    #region Search Engine Optimization Settings
                    var searchEngineOptimizationSettings = new string[] { "seoincludecustommetatagpropertyname", "seoenablecanonicallinkparameterspropertyname", "seocustommetatagpropertyname", "seocanonicallinkparameterlistpropertyname" };
                    foreach (string searchEngineOptimizationProperty in searchEngineOptimizationSettings)
                    {
                        if (MetaInfoDictionary.ContainsKey(searchEngineOptimizationProperty))
                        {
                            mSPWeb.AllProperties[searchEngineOptimizationProperty] = MetaInfoDictionary[searchEngineOptimizationProperty];
                        }
                    }
                    restoredProperties.AddRange(searchEngineOptimizationSettings);
                    #endregion

                    #region Search Engine Sitemap Settings
                    var searchEngineSitemapPropName = "xmlsitemaprobotstxtpropertyname";
                    if (MetaInfoDictionary.ContainsKey(searchEngineSitemapPropName))
                    {
                        mSPWeb.AllProperties[searchEngineSitemapPropName] = MetaInfoDictionary[searchEngineSitemapPropName].Replace("\\r\\n", "\r\n").Replace(@"\\", @"\");
                    }
                    restoredProperties.Add(searchEngineSitemapPropName);
                    #endregion

                    #region Search
                    var specicalPropertyForSearch = new string[] { "NoCrawl", "docid_settings_ui", "SRCH_SITE_DROPDOWN_MODE" };
                    restoredProperties.AddRange(specicalPropertyForSearch);

                    //restore Web >Search Visibility > Indexing Site Content
                    if (MetaInfoDictionary.ContainsKey("NoCrawl"))
                    {
                        mSPWeb.NoCrawl = bool.Parse(MetaInfoDictionary["NoCrawl"]);
                    }
                    //[SAAS-9184]    showurlstructure属性与Site中的ShowUrlStructure相冲突。
                    //if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("showurlstructure"))   
                    //{
                    //    mSPWeb.AllProperties["showurlstructure"] = MetaInfoDictionary["showurlstructure"];
                    //}
                    //settings->Search Setting. 
                    if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("SRCH_SITE_DROPDOWN_MODE"))
                    {
                        mSPWeb.AllProperties["SRCH_SITE_DROPDOWN_MODE"] = MetaInfoDictionary["SRCH_SITE_DROPDOWN_MODE"];
                    }
                    #endregion

                    //settings->Site Collections Object Cache.
                    //settings->Site Collections Output Cache.
                    //NOTE: there is a property:blobcacheflushcount which comes from WebApplication.Properties,
                    //i haven't update it. maybe it should be done in the future.
                    #region SiteCollection Input Output Cache
                    string[] propertiesForObjectCache = new string[] { "EnableCache", "EnableDebuggingOutput", "AllowAreaPageOverrides",
                                                                   "AllowLayoutPageOverrides","AnonymousPageCacheProfileUrl",
                                                                   "AuthenticatedPageCacheProfileUrl","MaxObjectCacheSize",
                                                                   "CreateCachePerRequest","ObjectCacheFlushCount","IsImportInProgress",
                                                                   "LastImportStatusUpdateTicks","CBQFlushOnSiteChange","CBQFlushOnTimeChange",
                                                                   "CBQMultiplier","CBQTimeToLive"
                                                                 };
                    restoredProperties.AddRange(propertiesForObjectCache);
                    foreach (string eachProperty in propertiesForObjectCache)
                    {
                        if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey(eachProperty))
                        {
                            mSPWeb.AllProperties[eachProperty] = MetaInfoDictionary[eachProperty];
                        }
                    }

                    //site output cache
                    if (MetaInfoDictionary.ContainsKey("__AuthenticatedPageCacheProfileUrl"))
                    {
                        mSPWeb.AllProperties["__AuthenticatedPageCacheProfileUrl"] = MetaInfoDictionary["__AuthenticatedPageCacheProfileUrl"];
                        if (mSPWeb.Properties != null)
                        {
                            mSPWeb.Properties["__AuthenticatedPageCacheProfileUrl"] = MetaInfoDictionary["__AuthenticatedPageCacheProfileUrl"];
                        }
                    }
                    if (MetaInfoDictionary.ContainsKey("__AnonymousPageCacheProfileUrl"))
                    {
                        mSPWeb.AllProperties["__AnonymousPageCacheProfileUrl"] = MetaInfoDictionary["__AnonymousPageCacheProfileUrl"];
                        if (mSPWeb.Properties != null)
                        {
                            mSPWeb.Properties["__AnonymousPageCacheProfileUrl"] = MetaInfoDictionary["__AnonymousPageCacheProfileUrl"];
                        }
                    }
                    #endregion

                    #region audit log reports
                    var specicalPropertyForAuditlog = new string[] { "_reportinggallerymetadataid", "_reportinggallerytemplateid" };
                    restoredProperties.AddRange(specicalPropertyForAuditlog);
                    #endregion

                    #region Page Layout and Site Template Settings

                    var specicalPropertyForLayoutAndTemplate = new string[] { "__AllowSpacesInNewPageName", "__InheritWebTemplates", "__WebTemplates", "__PageLayouts", "__DefaultPageLayout" };
                    restoredProperties.AddRange(specicalPropertyForLayoutAndTemplate);
                    RestoreWebPageLayoutAndTemplate(MetaInfoDictionary);

                    #endregion

                    #region Reporting Services Site Settings
                    var specicalPropertyForReportingServices = new string[] { "SPRSPrintEnabled", "SPRSAccessibleTablix", "SPRSRemoteErrorsInLocalMode" };
                    restoredProperties.AddRange(specicalPropertyForReportingServices);
                    RestoreReportingServiceSiteSettings(MetaInfoDictionary);

                    #endregion

                    #region DocumentID Service
                    var specicalPropertyForDocumentIDService = new string[] { "docid_msft_hier_listcnt", "docid_msft_hier_listidx" };
                    var specicalPropertyForDocumentId = new string[] { "docid_enabled", "docid_msft_hier_siteprefix", "docid_customProvider_class", "docid_customProvider_assembly" };
                    restoredProperties.AddRange(specicalPropertyForDocumentId);
                    //该属性可以还原prefix值
                    if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("docid_settings_ui"))
                    {
                        mSPWeb.AllProperties["docid_settings_ui"] = MetaInfoDictionary["docid_settings_ui"].Replace("\\r\\n", "\r\n");
                        mSPWeb.Properties["docid_settings_ui"] = MetaInfoDictionary["docid_settings_ui"].Replace("\\r\\n", "\r\n");
                    }
                    foreach (var property in specicalPropertyForDocumentId)
                    {
                        if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey(property))
                        {
                            mSPWeb.AllProperties[property] = MetaInfoDictionary[property];
                            mSPWeb.Properties[property] = MetaInfoDictionary[property];
                        }
                    }
                    restoredProperties.AddRange(specicalPropertyForDocumentIDService);
                    #endregion

                    #region Setting in SiteNavigationSettings.aspx
                    var siteNavigationSettings = new string[] { "EnableNavigation", "EnableSecurityTrimming", "EnableAudienceTargeting" };
                    if (mSPWeb.IsRootWeb)
                    {
                        RestoreAllPropertiesOfWebMetaInfo(restoredProperties, siteNavigationSettings);
                    }

                    #endregion

                    #region --用于还原Navigation的Hidden属性，这里的Guid是源端的数据，在PostAction中需要多加处理
                    //__GlobalNavigationIncludeTypes存储了publishing web navigation的的Hidden page和Hidden site属性，
                    //该属性不在此处还原，将其放在SitePostAction的AveSite.ResotreWebMetaInfo()中还原，
                    //在CI-9492客户问题中，客户反映在做job的过程中刷新目的端页面时，发现原本hidden的site都显示了，
                    //为了解决这个问题，所以在还原过程中不修改该属性，在SitePostAction将替换正确的属性更新进去。

                    var specicalPropertyNotRestoreHere = new string[] { "__GlobalNavigationExcludes", "__CurrentNavigationExcludes" };
                    restoredProperties.AddRange(specicalPropertyNotRestoreHere);
                    mAveSite.MappingManager.SiteMappingManager.WebAllPropertiesMapping[mSPWeb.ID] = MetaInfoDictionary;
                    #endregion


                    if (mIsRestoreWebNavgation)
                    {
                        #region merge code for metadata navigation setting
                        if (MetaInfoDictionary.ContainsKey("_webnavigationsettings"))
                        {
                            bool isRestoreNavigationXml = false;
                            string navigationXml = MetaInfoDictionary["_webnavigationsettings"];
                            navigationXml = navigationXml.Replace("\\r\\n", " ");
                            if (CheckTaxonomyProperty(navigationXml))
                            {
                                navigationXml = ProcessWebNavigationSetting(navigationXml);
                                isRestoreNavigationXml = true;
                            }
                            else
                            {
                                if (!HasInheritNavigationNode(navigationXml))
                                    isRestoreNavigationXml = true;
                            }

                            if (isRestoreNavigationXml)
                            {
                                mSPWeb.AllProperties["_webnavigationsettings"] = navigationXml;
                                if (mSPWeb.Properties != null && mSPWeb.Properties.ContainsKey("_webnavigationsettings"))
                                {
                                    mSPWeb.Properties["_webnavigationsettings"] = navigationXml;
                                }
                            }

                            restoredProperties.Add("_webnavigationsettings");
                        }
                        #endregion

                        #region PublishSiteNavigation Setting
                        var navigaionSetting = new[] { "__CurrentDynamicChildLimit","__CurrentNavigationIncludeTypes", 
                        "__DisplayShowHideRibbonActionId","__GlobalDynamicChildLimit","__GlobalNavigationIncludeTypes",
                        "__InheritCurrentNavigation","__NavigationOrderingMethod","__NavigationShowSiblings",
                        "__NavigationSortAscending","__NavigationAutomaticSortingMethod" };

                        RestoreAllPropertiesOfWebMetaInfo(restoredProperties, navigaionSetting);
                        #endregion
                    }

                    #region Custom Properties
                    string[] customProperties = new string[] { "ClientId", "ClientTitle", "ClientEntityId",
                                                                   "ProjectId","ProjectTitle",
                                                                   "ProjectEntityId","OpportunityId",
                                                                   "OpportunityTitle","OpportunityEntityId","MethodologyId",
                                                                   "MethodologyTitle","MethodologyEntityId","PartnerId",
                                                                   "PartnerTitle","PartnerEntityId",
                                                                   "SolutionId","SolutionTitle",
                                                                   "SolutionEntityId"
                                                                 };
                    restoredProperties.AddRange(customProperties);
                    foreach (string eachProperty in customProperties)
                    {
                        if (MetaInfoDictionary.ContainsKey(eachProperty))
                        {
                            mSPWeb.AllProperties[eachProperty] = MetaInfoDictionary[eachProperty];
                        }
                    }
                    #endregion

                    mSPWeb.Update();

                    /*开启 In Place Records Management这个feature,会多出record Declaration Settings，这些Settings 存储在rootWeb的Properties中，
                     * 对应ecm_siterecordrestrictions，ecm_siterecorddeclarationdefault，ecm_siterecorddeclarationby，ecm_siterecordundeclarationby
                     */
                    if (mSPWeb.Properties != null)
                    {
                        #region ECM
                        var specicalPropertyForRootWeb = new string[] {"ecm_siterecordrestrictions", "ecm_siterecorddeclarationdefault", "ecm_siterecorddeclarationby"
                        ,"ecm_siterecordundeclarationby", "enabledhelpcollections","enabledhelpcollections", "disabledhelpcollections", "docidlookup_searchscope" };
                        restoredProperties.AddRange(specicalPropertyForRootWeb);
                        foreach (string property in specicalPropertyForRootWeb)
                        {
                            if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey(property))
                            {
                                mSPWeb.Properties[property] = MetaInfoDictionary[property];
                            }
                        }
                        #endregion

                        if (MetaInfoDictionary.ContainsKey("ContentTypes_Mapping"))
                        {
                            if (string.IsNullOrEmpty(mSPWeb.Properties["ContentTypes_Mapping"]))
                            {
                                mSPWeb.Properties["ContentTypes_Mapping"] = MetaInfoDictionary["ContentTypes_Mapping"];
                            }
                            restoredProperties.Add("ContentTypes_Mapping");
                        }

                        #region Meta Data Service
                        foreach (var pro in MetaInfoDictionary.Keys)
                        {
                            if (pro.StartsWith("SiteCollectionGroupId", StringComparison.OrdinalIgnoreCase))
                            {//Metadata Service Group.
                                restoredProperties.Add(pro);
                            }
                        }
                        #endregion

                        #region Hold
                        var specicalPropertyForHold = new string[] { "_dlc_repositoryusersgroup", "holdlistid", "holdreportslistid" };
                        restoredProperties.AddRange(specicalPropertyForHold);
                        #endregion

                        if (isIncludeCustomPropertyBag)
                        {
                            if (!string.IsNullOrEmpty(WrapperConfiguration.SpecialWebPropertyNames))
                            {
                                string[] needForceSkippedProperities = WrapperConfiguration.SpecialWebPropertyNames.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                restoredProperties.AddRange(needForceSkippedProperities);
                            }
                            restoredProperties.Sort(StringComparer.Ordinal);
                            foreach (string pro in metaInfoDictionaryWithType.Keys)
                            {
                                try
                                {
                                    if ((pro.Equals("_VarLabelsListId", StringComparison.OrdinalIgnoreCase)
                                        && pro.Equals("vti_sitemasterid", StringComparison.OrdinalIgnoreCase)//vti_sitemasterid属性标记site master站点的ID。不要覆盖目的端。
                                        && restoredProperties.BinarySearch(pro, StringComparer.Ordinal) >= 0) || pro.StartsWith("SiteCollectionGroupId"))
                                    {
                                        continue;
                                    }

                                    if (DesignManagerViewPropertyNames.Contains(pro,StringComparison.OrdinalIgnoreCase))
                                    {
                                        log.Info($"Skip update DesignManagerViewProperty {pro} during restore web property,will post update it later.");
                                        continue;
                                    }

                                    var sourceValueWithType = metaInfoDictionaryWithType[pro];

                                    if (sourceValueWithType == null)//备份数据值为null
                                    {
                                        mSPWeb.AllProperties[pro] = null;
                                    }
                                    else if (!mSPWeb.AllProperties.ContainsKey(pro) || mSPWeb.AllProperties[pro] == null)//目的端不存在对应property//目的端property value为null
                                    {
                                        mSPWeb.AllProperties[pro] = sourceValueWithType;
                                    }
                                    else
                                    {
                                        var targetType = mSPWeb.AllProperties[pro].GetType();
                                        if (targetType == sourceValueWithType.GetType())
                                        {
                                            mSPWeb.AllProperties[pro] = sourceValueWithType;
                                        }
                                        else
                                        {
                                            var targetValue = ConvertTypeString(targetType.ToString(), sourceValueWithType.ToString());
                                            mSPWeb.AllProperties[pro] = targetValue;
                                            if (targetValue == null)
                                            {
                                                log.Warn("The destination property:{0} value is null, because of the source value {1} can not convert to specific type {2}", pro, sourceValueWithType, targetType.GetType());
                                            }
                                        }
                                    }
                                       
                                }
                                catch (Exception e)
                                {
                                    log.Error("Set property {0} failed , because of {1}", pro, e);
                                }

                            }
                        }
                        else if (!string.IsNullOrEmpty(WrapperConfiguration.SpecialWebPropertyNames))
                        {
                            string[] forceRestoreProperities = WrapperConfiguration.SpecialWebPropertyNames.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var pro in forceRestoreProperities)
                            {
                                if (MetaInfoDictionary.ContainsKey(pro))
                                {
                                    mSPWeb.Properties[pro] = MetaInfoDictionary[pro];
                                }
                            }
                        }
                        mSPWeb.Properties.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("Restore metaInfo error. metaInfo:{0}, web id:{1}\n error message:{2}", metaInfoString, mSPWeb.ID, ex));
                    report.AddDetail(new AveWrapperReportDto("RestoreWebMetaInfo", "RestoreWebMetaInfo", AveReportObjectType.WebMetaInfo, AveStatus.Skipped, "You don't have permission to restore web metainfo . " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("Restore metaInfo error. metaInfo:{0}, web id:{1}\n error message:{2}", metaInfoString, mSPWeb.ID, e));
                    //mLog.Error(e, "Restore meta info Error{0},Dest webID:{1}", metaInfoString,mSPWeb.ID);
                }

                log.Info("after the restore, destination:{0}", ConvertDictionary(mSPWeb.AllProperties));
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// web.AllProperties中，仅有四种类型：string,datetime,int,boolean
        /// </summary>
        /// <param name="type">type的格式为：system.datetime</param>
        /// <param name="value"></param>
        /// <returns>返回null,说明备份数据的数据类型与目的端已存在的类型不匹配，如果需要源端的，可在SPD中将目的端对应的property删除，再进行还原</returns>
        private object ConvertTypeString(string type, string value)
        {
            if ((!string.IsNullOrEmpty(type)) && type.Length > 0)
            {
                switch (type[7])
                {
                    case 'B':
                        {
                            bool boolValue;
                            if (bool.TryParse(value, out boolValue))
                            {
                                return boolValue;
                            }
                        }
                        break;
                    case 'I':
                        {
                            int intValue;
                            if (int.TryParse(value, out intValue))
                            {
                                return intValue;
                            }
                        }
                        break;
                    case 'D':
                        {
                            DateTime dateValue;
                            if (DateTime.TryParse(value, out dateValue))
                            {
                                return dateValue;
                            }
                        }
                        break;
                    default:
                        {
                            return value;
                        }
                }
            }

            return value;
        }

        /// <summary>
        /// 由于WebProperties分为两种，一种是携带类型的AllProperties，一种是只是String类型的WebProperties，在还原中我们需要将保存的properties按照固定的
        /// 类型还原到Propertes当中。SPWeb.AllProperties仅支持4种类型，bool/String/Integer/DateTime，此方法输入备份出来的MetaDataString，返回以
        /// Propety Name为Key，Property真实类型的值为Value的Dictionary。
        /// </summary>
        /// <param name="metaString">此参数来源为AveWebMataInfo.MetaInfo.Value</param>
        /// <returns>返回以Propety Name为Key，Property真实类型的值为Value的Dictionary
        /// </returns>
        private Dictionary<String, object> GetMetaInfoDicWithType(String metaString)
        {
            var metaInfo = new Dictionary<String, object>();
            var tempHashTable = AveCompressedUtility.GetMetaInfoHashtable(metaString);
            foreach (DictionaryEntry pro in tempHashTable)
            {
                if ((pro.Value as MetaInfoProperty) != null)
                {
                    switch ((pro.Value as MetaInfoProperty).Type)
                    {
                        case MetaInfoValueType.Boolean:
                            {
                                metaInfo[pro.Key.ToString()] = Convert.ToBoolean((pro.Value as MetaInfoProperty).Value);
                                break;
                            }
                        case MetaInfoValueType.Integer:
                            {
                                metaInfo[pro.Key.ToString()] = Convert.ToInt32((pro.Value as MetaInfoProperty).Value);
                                break;
                            }
                        case MetaInfoValueType.Time:
                            {
                                metaInfo[pro.Key.ToString()] = Convert.ToDateTime((pro.Value as MetaInfoProperty).Value);
                                break;
                            }
                        default:
                            {
                                metaInfo[pro.Key.ToString()] = (pro.Value as MetaInfoProperty).Value;
                                break;
                            }
                    }

                }

            }
            return metaInfo;
        }

        /// <summary>
        /// 判断源端是否是Managed Metadata Navigation
        /// </summary>
        /// <param name="navigationXml"></param>
        /// <returns></returns>
        private bool CheckTaxonomyProperty(string navigationXml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            navigationXml = navigationXml.Replace("\\r\\n", " ");
            xmlDoc.LoadXml(navigationXml);
            XmlNodeList nodeList = xmlDoc.SelectNodes("WebNavigationSettings/SiteMapProviderSettings/TaxonomySiteMapProviderSettings");

            if (nodeList.Count == 0)
            {
                return false;
            }
            bool isHaveTaxonomyProperty = false;
            foreach (XmlElement node in nodeList)
            {
                if (node.HasAttribute("UseParentSiteMap") && bool.Parse(node.GetAttribute("UseParentSiteMap")))
                {
                    continue;
                }

                if (node.HasAttribute("Disabled"))
                {
                    if (!bool.Parse(node.GetAttribute("Disabled")))
                    {
                        isHaveTaxonomyProperty = true;
                        break;
                    }
                }
                else
                {
                    isHaveTaxonomyProperty = true;
                    break;
                }
            }

            return isHaveTaxonomyProperty;
        }

        private string ProcessWebNavigationSetting(string settingSchema)
        {
            List<Guid> restoredTermSetId = new List<Guid>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(settingSchema);
            XmlNodeList taxonomyNodes = xmlDoc.SelectNodes("WebNavigationSettings/SiteMapProviderSettings/TaxonomySiteMapProviderSettings");
            XmlElement xmlEle = null;
            foreach (XmlNode taxonomyNode in taxonomyNodes)
            {
                xmlEle = taxonomyNode as XmlElement;
                Guid destTermStoreId = Guid.Empty;
                Guid destTermSetId = Guid.Empty;
                if (xmlEle.HasAttribute("TermStoreId"))
                {
                    Guid termStoreId = new Guid(xmlEle.Attributes["TermStoreId"].Value);
                    if (this.ParentSite.MetadataService.TermStoreIdMapping.ContainsKey(termStoreId))
                    {
                        destTermStoreId = this.ParentSite.MetadataService.TermStoreIdMapping[termStoreId];
                        xmlEle.Attributes["TermStoreId"].Value = destTermStoreId.ToString();
                    }
                }
                if (xmlEle.HasAttribute("TermSetId"))
                {
                    Guid termSetId = new Guid(xmlEle.Attributes["TermSetId"].Value);
                    if (this.ParentSite.MetadataService.TermSetIdMapping.ContainsKey(termSetId))
                    {
                        destTermSetId = this.ParentSite.MetadataService.TermSetIdMapping[termSetId];
                        xmlEle.Attributes["TermSetId"].Value = destTermSetId.ToString();
                    }
                    else
                    {//If skip restore global term set, TermSetIdMapping will not contain the term set id.
                        destTermSetId = termSetId;
                    }
                }

                if (destTermSetId != Guid.Empty && !restoredTermSetId.Contains(destTermSetId))
                {
                    try
                    {
                        IAveTaxonomySession session = mAveSite.SPSite.AveSPTaxonomySession;//mAveSite.ObjectModelFactory.CreateTaxonomySession(mAveSite.SPSite);
                        IAveTermStore destTermStore = session.TermStores[destTermStoreId];
                        IAveTermSet destTermSet = destTermStore.GetTermSet(destTermSetId);
                        destTermSet.SetCustomProperty("_Sys_Nav_AttachedWeb_SiteId", mAveSite.SPSite.ID.ToString());
                        destTermSet.SetCustomProperty("_Sys_Nav_AttachedWeb_OriginalUrl", mSPWeb.Url);
                        destTermSet.SetCustomProperty("_Sys_Nav_AttachedWeb_WebId", mSPWeb.ID.ToString());
                        destTermSet.SetCustomProperty("_Sys_Nav_AttachedWebHistory", "");
                        destTermSet.SetCustomProperty("_Sys_Nav_AttachedWeb_Timestamp", DateTime.Now.ToString());

                        foreach (IAveTerm term in destTermSet.Terms)
                        {
                            ProcessTermLocalCustomProperties(term);
                        }
                        destTermSet.TermStore.CommitAll();
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Restore Web Navigation Error. {0}", ex.ToString());
                    }
                    restoredTermSetId.Add(destTermSetId);
                }
            }
            return xmlDoc.OuterXml;
        }

        private void ProcessTermLocalCustomProperties(IAveTerm term)
        {
            if (term.LocalCustomProperties.ContainsKey("_Sys_Nav_SimpleLinkUrl"))
            {
                string url = term.LocalCustomProperties["_Sys_Nav_SimpleLinkUrl"];
                term.SetLocalCustomProperty("_Sys_Nav_SimpleLinkUrl", AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), ParentSite.SourceSiteInfo, mAveSite.SiteUrl));
            }

            if (term.LocalCustomProperties.ContainsKey("_Sys_Nav_CatalogTargetUrl"))
            {
                string url = term.LocalCustomProperties["_Sys_Nav_CatalogTargetUrl"];
                term.SetLocalCustomProperty("_Sys_Nav_CatalogTargetUrl", AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, false), ParentSite.SourceSiteInfo, mAveSite.SiteUrl));
            }

            if (term.LocalCustomProperties.ContainsKey("_Sys_Nav_CatalogTargetUrlForChildTerms"))
            {
                string url = term.LocalCustomProperties["_Sys_Nav_CatalogTargetUrlForChildTerms"];
                term.SetLocalCustomProperty("_Sys_Nav_CatalogTargetUrlForChildTerms", AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, false), ParentSite.SourceSiteInfo, mAveSite.SiteUrl));
            }

            if (term.LocalCustomProperties.ContainsKey("_Sys_Nav_TargetUrl"))
            {
                string url = term.LocalCustomProperties["_Sys_Nav_TargetUrl"];
                term.SetLocalCustomProperty("_Sys_Nav_TargetUrl", AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, false), ParentSite.SourceSiteInfo, mAveSite.SiteUrl));
            }

            if (term.LocalCustomProperties.ContainsKey("_Sys_Nav_TargetUrlForChildTerms"))
            {
                string url = term.LocalCustomProperties["_Sys_Nav_TargetUrlForChildTerms"];
                term.SetLocalCustomProperty("_Sys_Nav_TargetUrlForChildTerms", AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, false), ParentSite.SourceSiteInfo, mAveSite.SiteUrl));
            }
            if (term.LocalCustomProperties.ContainsKey("_Sys_Nav_AssociatedFolderUrl"))
            {
                string url = term.LocalCustomProperties["_Sys_Nav_AssociatedFolderUrl"];
                term.SetLocalCustomProperty("_Sys_Nav_AssociatedFolderUrl", AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, false), ParentSite.SourceSiteInfo, mAveSite.SiteUrl));
            }

            foreach (IAveTerm item in term.Terms)
            {
                ProcessTermLocalCustomProperties(item);
            }
        }

        //cm中，如果是subsite升级到site collection，当源端的navigation有选择是继承类型的话，则不应该还原该属性，否则rootweb里的navigation也变成继承的了，当root web没有parent web所以不能是继承的
        private bool HasInheritNavigationNode(string sourceNavSetting)
        {
            if (mSPWeb.IsRootWeb)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sourceNavSetting);
                XmlNodeList switchableSiteMapNodes = xmlDoc.SelectNodes("WebNavigationSettings/SiteMapProviderSettings/SwitchableSiteMapProviderSettings");
                foreach (XmlElement switchableSettingNode in switchableSiteMapNodes)
                {
                    if (switchableSettingNode.HasAttribute("UseParentSiteMap") && Convert.ToBoolean(switchableSettingNode.GetAttribute("UseParentSiteMap")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RestoreAllPropertiesOfWebMetaInfo(List<string> restoredProperties, IEnumerable<string> settingToRestore, bool removeIfNotExsit = true)
        {
            foreach (string setting in settingToRestore)
            {
                if (mSPWeb.IsRootWeb && setting.Equals("__InheritCurrentNavigation", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!MetaInfoDictionary.ContainsKey(setting) && mSPWeb.AllProperties.ContainsKey(setting))
                {
                    if (removeIfNotExsit)
                    {
                        if (setting.Equals("__InheritCurrentNavigation", StringComparison.OrdinalIgnoreCase))
                        {
                            mSPWeb.AllProperties["__InheritCurrentNavigation"] = "False";
                        }
                        else if (setting.Equals("__CurrentNavigationIncludeTypes", StringComparison.OrdinalIgnoreCase) || setting.Equals("__GlobalNavigationIncludeTypes", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        else
                        {
                            mSPWeb.AllProperties.Remove(setting);
                        }
                    }
                }
                else if (MetaInfoDictionary.ContainsKey(setting))
                {
                    mSPWeb.AllProperties[setting] = MetaInfoDictionary[setting];
                    restoredProperties.Add(setting);
                }
            }
        }

        /*private static bool NoSuchWebInSite(IAveSite site, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.NoSuchWebInSite"))
            {
#endif
                if (!name.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    name = "/" + name;

                foreach (IAveWeb web in site.AllWebs)
                {
                    using (web)
                    {
                        if (web.ServerRelativeUrl.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return false;
                        }
                    }
                }

                return true;
#if PerformanceLog
            }
#endif
        }*/

        //public bool TemplateAvalible(string templateName, out bool isHidden)
        //{
        //    isHidden = false;
        //    try
        //    {
        //        if (ParentSite != null && this.ParentSite.SPSite != null && this.ParentSite.SPSite.RootWeb != null)
        //        {
        //            IAveWeb rootWeb = this.ParentSite.SPSite.RootWeb;
        //            IAveRegionalSettings regionalSettings = this.ParentSite.ObjectModelFactory.CreateRegionalSettings(rootWeb, false);
        //            foreach (IAveLanguage lanuage in regionalSettings.InstalledLanguages)
        //            {
        //                var templates = this.ParentSite.SPSite.GetWebTemplates((uint)lanuage.LCID);

        //                foreach (var template in templates)
        //                {
        //                    if (string.Equals(template.Name, templateName, StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        isHidden = template.IsHidden;
        //                        return true;
        //                    }
        //                }
        //            }
        //        }
        //        else if (ParentSite != null)
        //        {
        //            return ParentSite.TemplateAvalible(templateName, out isHidden);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Debug(string.Format("Check site template {0} error.Exception:{1}", templateName, e.ToString()));
        //        if (ParentSite != null)
        //            return ParentSite.TemplateAvalible(templateName, out isHidden);
        //    }
        //    return false;
        //}

        /// <summary>
        /// /// 这个函数主要是为了load或者创建基本的Web所需要的，如果需要还原setting，请到restore property函数中。
        /// </summary>
        /// <param name="webInfo"></param>
        public void RestoreWebSelf(AveWebInfo webInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreWebSelf"))
            {
#endif
                mWebInfo = webInfo;
                mOldId = webInfo.OldWebId;
                mSrcLanguageId = webInfo.LCID;

                if (mName == ".")
                {
                    // SPSite.RootWeb will not create new webs if you call it several
                    // times. It will return same SPWeb when you call it. So we create
                    // a new SPWeb in case we dispose it when we call AveSPWeb.Dispose().
                    IAveWeb rootWeb = mAveSite.SPSite.RootWeb;
                    mSPWeb = mAveSite.SPSite.OpenWeb(rootWeb.ID);
                    //To Do Something
                }
                else
                {
                    mSPWeb = GetWebInSite(mAveSite.SPSite, mName);
                    if (mSPWeb == null)
                    {
                        if (webInfo.WebTemplate.ToLower().Contains("app#"))  //if the app is skipped, and the web associated with it should not be created
                        {
                            NeedContinue = false;
                            return;
                        }
                        if (RestoringWeb.IsIncludingRecycleBinData && this.mRestoreOption.CheckRestoreOption(AveRestoreMode.Default) && !IsNewCreated)
                        {
                            if (IsConfictWithRecycle())
                            {
                                RestoringWeb.NeedSkipped = true;
                                ReportMessage = "Not overwrite and conflict with recycle bin";
                                NeedContinue = false;
                                return;
                            }
                        }

                        try
                        {
                            CreateNewWeb(mWebInfo);
                            mIsNewCreated = true;
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            throw ex;
                        }
                    }
                    else if (mSPWeb.IsAppWeb && WebInfo.IsAppWeb)
                    {
                        IAveAppInstance appInstance = mSPWeb.ParentWeb.GetAppInstanceById(mSPWeb.AppInstanceId);
                        if (appInstance != null && appInstance.Status != AveAppInstanceStatus.Installed)
                        {
                            throw new AveWrapperSkipException(WrapperReportResourceKey.Wrapper_RestoreAppDataFailedForInstallAppFailed.ToString(), WrapperRestoreReportResource.Wrapper_RestoreAppDataFailedForInstallAppFailed);
                        }
                        if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Contains(WebInfo.AppInstanceId))
                        {
                            throw new AveWrapperSkipException(WrapperReportResourceKey.Wrapper_SkippedApp.ToString(), WrapperRestoreReportResource.Wrapper_SkippedAppData);
                        }
                    }
                }
                mAveSite.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(mWebInfo.Url, mSPWeb.Url);
                mAveSite.MappingManager.SiteMappingManager.AddWebUrlMapping(mWebInfo.Name, mSPWeb.ServerRelativeUrl);
                mAveSite.MappingManager.SiteMappingManager.AddWebUrlDestToSourceMapping(mSPWeb.ServerRelativeUrl, mWebInfo.Name);
                mAveSite.MappingManager.SiteMappingManager.AddWebIDMapping(mWebInfo.OldWebId, mSPWeb.ID);

                if (mWebInfo.parentWebInfo != null)
                {
                    AveWebInfo tempWebInfo = mWebInfo.parentWebInfo;
                    string webRelativeUrl = mSPWeb.ServerRelativeUrl;
                    while (tempWebInfo != null)
                    {
                        if (!mSPWeb.IsRootWeb && webRelativeUrl.IndexOf('/') >= 0 && webRelativeUrl.StartsWith(mAveSite.SPSite.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!webRelativeUrl.TrimEnd('/').Equals(mAveSite.SPSite.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))//到SC level了就不能再截啦
                            {
                                webRelativeUrl = webRelativeUrl.Substring(0, webRelativeUrl.LastIndexOf('/'));
                            }
                            mAveSite.MappingManager.SiteMappingManager.AddWebUrlMapping(tempWebInfo.Name, webRelativeUrl);
                            tempWebInfo = tempWebInfo.parentWebInfo;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                TransformUICultureToWebCulture();
                InitializeMembers();

                if (mSPWeb.WebTemplate != null && mSPWeb.WebTemplate.StartsWith("MPS", StringComparison.OrdinalIgnoreCase))
                {
                    LinkToEventItem();
                }
            StringBuilder logInfo = new StringBuilder();
            logInfo.AppendLine("AveSPWeb RestoreSelf Complete.");
            logInfo.AppendLine(string.Format("SourceInfo:[{0}][{1}][{2}][{3}]",webInfo.Title,webInfo.Url,webInfo.LCID,webInfo.WebTemplate));
            logInfo.AppendLine(string.Format("CurrentWebInfo:[{0}][{1}][{2}][{3}#{4}]", mSPWeb.Title, mSPWeb.Url, mSPWeb.Language, mSPWeb.WebTemplate,mSPWeb.Configuration));
            log.Info(logInfo.ToString());

#if PerformanceLog
            }
#endif
        }

        private void TransformUICultureToWebCulture()
        {
            try
            {
                if (Thread.CurrentThread.CurrentUICulture != mSPWeb.UICulture)
                {
                    Thread.CurrentThread.CurrentUICulture = mSPWeb.UICulture;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while set CurrentUICulture of current thread. Error: {0}", e.ToString()));
            }
        }

        private void LinkToEventItem()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.LinkToEventItem"))
            {
#endif
                try
                {
                    if (mAveSite.MappingManager.SiteMappingManager.MeetingWorkSpaceMapping.ContainsKey(mSPWeb.Url))
                    {
                        var array = mAveSite.MappingManager.SiteMappingManager.MeetingWorkSpaceMapping[mSPWeb.Url];
                        var objectModel = AveObjectModelFactory.CreateObjectModelFactory("", new AveBPOSAccountInfo(), AveContextKind.ServerObjectModel);
                        var meeting = objectModel.CreateMeeting();
                        meeting.LinkWithEvent(mSPWeb, (string)array[0], (int)array[1], "WorkspaceLink", "Workspace");
                    }
                }
                catch (Exception exception)
                {
                    log.Warn("An error occurred while link web to event.Exception {0}", exception);
                }
#if PerformanceLog
            }
#endif
        }

        private bool IsConfictWithRecycle()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.IsConfictWithRecycle"))
            {
#endif
                if (!mAveSite.ObjectModelFactory.IsSPInstalled)
                {
                    return false;
                }
                string webUrl = null;
                IAveSite site = mAveSite.SPSite;
                if (mName.StartsWith(site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    webUrl = mName;
                }
                else if (site.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    webUrl = site.ServerRelativeUrl + mName;
                }
                else
                {
                    webUrl = site.ServerRelativeUrl + "/" + mName;
                }
                webUrl = webUrl.TrimStart(new char[] { '/' });
                if (mQueryService == null) //目的端为Office 365时防止抛空引用，job completed with exception，这里暂时控制一下。
                {
                    return false;
                }
                else
                {
                    return mQueryService.IsConflictWithRecycle(site.ID, webUrl);
                }
                //mSqlConn.ClearParameters();
                //mSqlConn.AddParameter("@SiteId", site.ID);
                //mSqlConn.AddParameter("@FullUrl", webUrl);

                //const string cmdText = @"SELECT Id FROM AllWebs WHERE SiteId =@SiteId AND FullUrl=@FullUrl AND DeleteTransactionId<>0x";
                //bool isConflict = false;
                //try
                //{
                //    using (SqlDataReader dr = mSqlConn.ExecuteReader(cmdText))
                //    {
                //        if (dr.HasRows)
                //        {
                //            isConflict = true;
                //        }
                //    }
                //}
                //catch
                //{
                //    isConflict = false;
                //}
                //return isConflict;
#if PerformanceLog
            }
#endif
        }

        public void ClearWebNavigation()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ClearWebNavigation"))
            {
#endif
                try
                {
                    ClearNavigation(mSPWeb.Navigation.QuickLaunch);
                    ClearNavigation(mSPWeb.Navigation.TopNavigationBar);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error while clearNavigation. ", ex);
                    report.AddDetail(new AveWrapperReportDto("WebNavigation", "WebNavigation", AveReportObjectType.WebNavigation, AveStatus.Skipped, "You don't have permission to clear web navigation" + ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        private void ClearNavigation(IAveNavigationNodeCollection co)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ClearNavigation"))
            {
#endif
                if (co != null)
                {
                    for (int index = co.Count - 1; index >= 0; index--)
                    {
                        try
                        {
                            co[index].Delete();
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Failed to clear navigation. Error message: {0}", ex.ToString()));
                            report.AddDetail(new AveWrapperReportDto("WebNavigation", "WebNavigation", AveReportObjectType.WebNavigation, AveStatus.Skipped, "You don't have permission to delete web navigations" + ex.Message));
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Failed to clear navigation. Error message: {0}", e.ToString()));
                        }
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private static IAveWeb GetWebInSite(IAveSite site, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetWebInSite"))
            {
#endif
                string webUrl = null;
                if (name.StartsWith(site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    webUrl = name;
                }
                else if (site.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    webUrl = site.ServerRelativeUrl + name;
                }
                else
                {
                    webUrl = site.ServerRelativeUrl + "/" + name;
                }
                IAveWeb web = site.OpenWeb(webUrl);
                if (web != null && web.Exists)
                {
                    return web;
                }
                return null;
#if PerformanceLog
            }
#endif
        }

        private void EnsureParentWeb(AveWebInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.EnsureParentWeb"))
            {
#endif
                List<KeyValuePair<string, AveWebInfo>> parentWebList = new List<KeyValuePair<string, AveWebInfo>>();
                string name = mName;
                while (true)
                {
                    if (name.Contains("/"))
                    {
                        name = name.Substring(0, name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        break;
                    }
                    using (IAveWeb web = GetWebInSite(mAveSite.SPSite, name))
                    {
                        if (web == null)
                        {
                            if (info.parentWebInfo != null)
                            {
                                parentWebList.Add(new KeyValuePair<string, AveWebInfo>(name, info.parentWebInfo));
                            }
                            else
                            {
                                parentWebList.Add(new KeyValuePair<string, AveWebInfo>(name, info));
                            }
                            info = info.parentWebInfo;
                        }
                        else
                        {
                            if (RestoringDto.ChangeToServerRelativeUrl)
                            {
                                if (!string.IsNullOrEmpty(mName))
                                {
                                    if (mName.TrimEnd('/').Contains("/"))
                                    {
                                        mName = mName.TrimEnd('/').Substring(mName.LastIndexOf('/') + 1);
                                    }
                                    mName = web.ServerRelativeUrl.TrimEnd('/') + "/" + mName.TrimStart('/').TrimEnd('/');
                                }
                            }
                            break;
                        }
                    }
                }
                for (int i = parentWebList.Count - 1; i >= 0; i--)
                {
                    KeyValuePair<string, AveWebInfo> value = parentWebList[i];
                    CreateNewWeb(value.Value, value.Key);
                }
#if PerformanceLog
            }
#endif
        }

        private void CreateNewWeb(AveWebInfo info, string webUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.CreateNewWeb_1"))
            {
#endif
                try
                {
                    if (info.WebTemplate == null)//该Web的ParentWeb还原失败.
                    {
                        return;
                    }
                    //publishing site, 需要先开启site collection Publishing Infrastructure feature。
                    if (info.WebTemplate.StartsWith("CMSPUBLISHING", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.Equals("SPS#0", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.StartsWith("BLANKINTERNET", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.Equals("SPSSITES#0", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.Equals("SRCHCEN#0", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.Equals("SPSREPORTCENTER#0", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.StartsWith("ENTERWIKI", StringComparison.OrdinalIgnoreCase)
                         || info.WebTemplate.Equals("SRCHCENTERFAST#0", StringComparison.OrdinalIgnoreCase))
                    {
                        if (AveSPWebTemplate.IsCommunicationSite($"{mAveSite.SPSite.RootWeb.WebTemplate}#{mAveSite.SPSite.RootWeb.Configuration}"))
                        {
                            log.Warn($"use communication site template({AveSPWebTemplate.COMMUNICATION_SITE}) instead of {info.WebTemplate} for web:{mName} because classic publishing site has retired.");
                            info.WebTemplate = AveSPWebTemplate.COMMUNICATION_SITE;
                        }
                        else
                        {
                            Guid publishingFeatureId = new Guid(AveFeatureConstants.PUBLISHING_FEATURE_ID);
                            if (mAveSite.SPSite.Features[publishingFeatureId] == null)
                            {
                                mAveSite.SPSite.Features.Add(publishingFeatureId, true);
                            }
                        }
                    }
                    //NewsSite site, 需要先开启site collection SharePoint Server Standard Site Collection features
                    if (info.WebTemplate.StartsWith("SPSNHOME", StringComparison.OrdinalIgnoreCase))
                    {
                        Guid standardFeatureId = new Guid("b21b090c-c796-4b0f-ac0f-7ef1659c20ae");
                        if (mAveSite.SPSite.Features[standardFeatureId] == null)
                        {
                            mAveSite.SPSite.Features.Add(standardFeatureId, true);
                        }
                        Guid publishingFeatureId = new Guid(AveFeatureConstants.PUBLISHING_FEATURE_ID);
                        if (mAveSite.SPSite.Features[publishingFeatureId] == null)
                        {
                            mAveSite.SPSite.Features.Add(publishingFeatureId, true);
                        }
                    }

                    if (info.WebTemplate.StartsWith("SPSPERS#", StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn("use team site template instead of {0} for web:{1} because O365 has issue.", info.WebTemplate, webUrl);
                        info.WebTemplate = "STS#0";
                    }

                    mSPWeb = mAveSite.SPSite.AddWeb(webUrl, info.Title, info.Description, info.LCID, info.WebTemplate, info.HasUniqueRoleDefinitions, false);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("An error occurred while creating new web. SiteUrl:{0}, WebName:{1}\n error message:{2}", mAveSite.SPSite.Url, webUrl, e));
                    throw;
                }
#if PerformanceLog
            }
#endif
        }

        private void CreateNewWeb(AveWebInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.CreateNewWeb"))
            {
#endif
                try
                {
                    if (info.WebTemplate.Contains("#"))
                    {
                        int result = 0;
                        int.TryParse(info.WebTemplate.Substring(info.WebTemplate.LastIndexOf('#') + 1), out result);
                        if (result < 0)
                        {
                            info.WebTemplate = info.WebTemplate.Substring(0, info.WebTemplate.LastIndexOf('#') + 1) + 0;
                        }
                    }
                    if (ParentSite.MappingManager.SiteMappingManager.TemplateMapping.ContainsKey(info.WebTemplate))
                    {
                        info.WebTemplate = ParentSite.MappingManager.SiteMappingManager.TemplateMapping[info.WebTemplate];
                    }
                    if (mLanguageForNewCreatedWeb != 0 && mLanguageForNewCreatedWeb != info.LCID)
                    {
                        info.LCID = mLanguageForNewCreatedWeb;
                    }
                    //获取custom template mapping
                    TemplateKeyInfo templateInfo = new TemplateKeyInfo(TemplateMappingLevel.Web, string.Empty, info.WebTemplate);
                    string mappingTemplate = ParentSite.TemplateMapping.GetMappingTemplateBeforeAdd(templateInfo);
                    if (!mappingTemplate.Equals(info.WebTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        info.WebTemplate = mappingTemplate;
                    }
                    EnsureParentWeb(info);
                    //publishing site, 需要先开启site collection Publishing Infrastructure feature。
                    if (info.WebTemplate.StartsWith("CMSPUBLISHING", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.Equals("SPS#0", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.StartsWith("BLANKINTERNET", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.Equals("SPSSITES#0", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.Equals("SRCHCEN#0", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.Equals("SPSREPORTCENTER#0", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.Equals("ENTERWIKI#0", StringComparison.OrdinalIgnoreCase)
                        || info.WebTemplate.Equals("SRCHCENTERFAST#0", StringComparison.OrdinalIgnoreCase))
                    {
                        if (AveSPWebTemplate.IsCommunicationSite($"{mAveSite.SPSite.RootWeb.WebTemplate}#{mAveSite.SPSite.RootWeb.Configuration}"))
                        {
                            log.Warn($"use communication site template({AveSPWebTemplate.COMMUNICATION_SITE}) instead of {info.WebTemplate} for web:{mName} because classic publishing site has retired.");
                            info.WebTemplate = AveSPWebTemplate.COMMUNICATION_SITE;
                        }
                        else
                        {
                            Guid publishingFeatureId = new Guid(AveFeatureConstants.PUBLISHING_FEATURE_ID);
                            if (mAveSite.SPSite.Features[publishingFeatureId] == null)
                            {
                                mAveSite.SPSite.Features.Add(publishingFeatureId, true);
                            }
                        }
                    }

                    //NewsSite site, 需要先开启site collection SharePoint Server Standard Site Collection features
                    if (info.WebTemplate.StartsWith("SPSNHOME", StringComparison.OrdinalIgnoreCase))
                    {
                        Guid standardFeatureId = new Guid("b21b090c-c796-4b0f-ac0f-7ef1659c20ae");
                        if (mAveSite.SPSite.Features[standardFeatureId] == null)
                        {
                            mAveSite.SPSite.Features.Add(standardFeatureId, true);
                        }
                        Guid publishingFeatureId = new Guid(AveFeatureConstants.PUBLISHING_FEATURE_ID);
                        if (mAveSite.SPSite.Features[publishingFeatureId] == null)
                        {
                            mAveSite.SPSite.Features.Add(publishingFeatureId, true);
                        }
                    }
                    AveSPEventReceiverConfig.EnableEventReceiver();

                    if (info.WebTemplate.StartsWith("SPSPERS#", StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn("use team site template instead of {0} for web:{1} because O365 has issue.", info.WebTemplate, mName);
                        info.WebTemplate = "STS#0";
                    }

                    mSPWeb = mAveSite.SPSite.AddWeb(mName, info.Title, info.Description, info.LCID, info.WebTemplate,
                                              false, false);
                }
                catch (AveSecurityTrimingException ex)
                {
                    throw ex;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("An error occurred while creating new web. SiteUrl:{0}, WebName:{1}, LCID:{2}, Template:{3}\n error message:{4}", mAveSite.SPSite.Url, mName, info.LCID, info.WebTemplate, e));
                    throw;
                }
                finally
                {
                    AveSPEventReceiverConfig.DisableEventReceiver();
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// handler to process web template and layout property
        /// 测试发现及时layout中有id，但是覆盖到目的端之后依然是正确的，可能sharepoint是以其中的url为依据进行查找的，暂时没有处理
        /// </summary>
        /// <param name="metaDataInfo"></param>
        private void RestoreWebPageLayoutAndTemplate(Dictionary<string, string> metaDataInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreWebPageLayoutAndTemplate"))
            {
#endif
                if (mSPWeb.IsPublish || string.Equals(mSPWeb.WebTemplate, "ENTERWIKI"))//only publishing web need to handle this value ;WebTemplate为"ENTERWIKI"的比较特殊，在不开启Publishing Feature的情况下也会存在 page layout 设置(SAAS-9299)
                {
                    if (metaDataInfo.ContainsKey("__InheritWebTemplates") && (!mSPWeb.IsRootWeb))//root web没有必要继承parent，目的端是什么值就是什么值
                    {
                        mSPWeb.AllProperties["__InheritWebTemplates"] = metaDataInfo["__InheritWebTemplates"];
                    }
                    if (metaDataInfo.ContainsKey("__WebTemplates"))
                    {
                        mSPWeb.AllProperties["__WebTemplates"] = MetaInfoDictionary["__WebTemplates"];
                    }
                    //rootweb can not inherit pagelayout
                    if (metaDataInfo.ContainsKey("__PageLayouts"))
                    {
                        if ((!mSPWeb.IsRootWeb) || (!MetaInfoDictionary["__PageLayouts"].Equals("__inherit", StringComparison.OrdinalIgnoreCase)))
                        {
                            mSPWeb.AllProperties["__PageLayouts"] = MetaInfoDictionary["__PageLayouts"];
                        }
                    }
                    if (metaDataInfo.ContainsKey("__DefaultPageLayout"))
                    {
                        //rootweb can not inherit __DefaultPageLayout
                        if ((!mSPWeb.IsRootWeb) || (!MetaInfoDictionary["__DefaultPageLayout"].Equals("__inherit", StringComparison.OrdinalIgnoreCase)))
                        {
                            mSPWeb.AllProperties["__DefaultPageLayout"] = MetaInfoDictionary["__DefaultPageLayout"];
                        }
                    }
                    if (metaDataInfo.ContainsKey("__AllowSpacesInNewPageName"))  //[SAAS-7998] "__AllowSpacesInNewPageName" 属于Page Layout Settings 的属性 
                    {
                        mSPWeb.AllProperties["__AllowSpacesInNewPageName"] = MetaInfoDictionary["__AllowSpacesInNewPageName"];
                    }
                }
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// 还原Reporting Services Site Settings，经过研究发现这个setting需要在AllProperties里更新
        /// 并且需要转成bool类型，如果直接赋值string类型，update之后，在SP界面上点击会报类型不匹配的错误
        /// </summary>
        /// <param name="metaDataInfo"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPRSAccessibleTablix is a key")]
        private void RestoreReportingServiceSiteSettings(Dictionary<string, string> metaDataInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreReportingServiceSiteSettings"))
            {
#endif
                if (metaDataInfo.ContainsKey("SPRSPrintEnabled"))
                {
                    mSPWeb.AllProperties["SPRSPrintEnabled"] = Convert.ToBoolean(MetaInfoDictionary["SPRSPrintEnabled"]);
                }
                if (metaDataInfo.ContainsKey("SPRSRemoteErrorsInLocalMode"))
                {
                    mSPWeb.AllProperties["SPRSRemoteErrorsInLocalMode"] = Convert.ToBoolean(MetaInfoDictionary["SPRSRemoteErrorsInLocalMode"]);
                }
                if (metaDataInfo.ContainsKey("SPRSAccessibleTablix"))
                {
                    mSPWeb.AllProperties["SPRSAccessibleTablix"] = Convert.ToBoolean(MetaInfoDictionary["SPRSAccessibleTablix"]);
                }
#if PerformanceLog
            }
#endif
        }
        private string ParseMasterUrl(string destMasterUrl, string masterUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ParseMasterUrl"))
            {
#endif
                string temp = masterUrl;
                Guid publishingFeatureId = new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb");//PublishWeb Feature Guid
                if (destMasterUrl != null)
                {
                    int index = temp.IndexOf("_catalogs/masterpage/", StringComparison.OrdinalIgnoreCase);
                    string sourceWebUrl = "/" + temp.Substring(0, index).Trim('/');
                    if (ParentSite.SourceSiteInfo.ServerRelativeUrl.Equals(sourceWebUrl, StringComparison.OrdinalIgnoreCase))//表示Site用的是Root Web的Master Page
                    {
                        masterUrl = mSPWeb.Site.RootWeb.ServerRelativeUrl + "/" + temp.Substring(index);
                    }
                    else if (mWebInfo.Name.Equals(sourceWebUrl, StringComparison.OrdinalIgnoreCase))//web使用的是自己的master page
                    {
                        int destIndex = destMasterUrl.IndexOf("/_catalogs/masterpage/", StringComparison.OrdinalIgnoreCase);
                        string destPartUrl = destMasterUrl.Substring(0, destIndex);
                        //根据目的端所使用masterpageurl的情况来为masterpageurl赋值
                        //SAAS-11697,masterurl获取错误，应该直接通过相对路径+masterurl的后缀获取
                        //if (destPartUrl.Equals(mSPWeb.ServerRelativeUrl))
                        //{
                        if (mSPWeb.ServerRelativeUrl.Length == 1 && mSPWeb.ServerRelativeUrl[0] == '/')
                        {
                            masterUrl = mSPWeb.ServerRelativeUrl + temp.Substring(index);
                        }
                        else
                        {
                            masterUrl = mSPWeb.ServerRelativeUrl + "/" + temp.Substring(index);
                        }

                        //}
                        //else
                        //{
                        //masterUrl = destPartUrl + "/" + temp.Substring(index);
                        //}
                    }
                    //else if (ParentSite.SourceSiteInfo.ServerRelativeUrl.Equals(sourceWebUrl, StringComparison.OrdinalIgnoreCase))//表示Site用的是Root Web的Master Page
                    //{
                    //    masterUrl = mSPWeb.Site.RootWeb.ServerRelativeUrl + "/" + temp.Substring(index);
                    //}
                    else
                    {
                        if (!temp.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            temp = "/" + temp;
                        }
                        ReplaceOption option = new ReplaceOption(true);
                        temp = AveReplaceProcessor.UrlReplace(temp, ParentSite.MappingManager.SiteMappingManager.WebUrlMapping, option, ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                        masterUrl = temp;
                    }
                }
                else
                {
                    int index = masterUrl.IndexOf("catalogs/masterpage/", StringComparison.OrdinalIgnoreCase);
                    if (index > 0)
                    {
                        temp = mSPWeb.ServerRelativeUrl + "/_" + masterUrl.Remove(0, index);
                    }
                    masterUrl = temp;
                }

                if (!string.IsNullOrEmpty(masterUrl))
                {
                    masterUrl = "/" + masterUrl.TrimStart('/');
                }

                return masterUrl;
#if PerformanceLog
            }
#endif
        }

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public string ServerRelativeUrl
        {
            get { return this.SPWeb.ServerRelativeUrl; }
        }

        /*[SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        private string GetCssFolderUniqueCode(string webCssFolder)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetCssFolderUniqueCode"))
            {
#endif
                int index = webCssFolder.IndexOf('-');
                if (index < 0) throw new Exception("the format of the CssFolderUrl is not correct" + webCssFolder);
                return "Custom.thmx-" + webCssFolder.Substring(index + 1);
#if PerformanceLog
            }
#endif
        }*/

        public void RestoreThemeCssFolderUrl()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAlternateCSSUrl"))
            {
#endif
                try
                {
                    if (!mIsRestoreWebSetting)
                    {
                        return;
                    }
                    if (WebSettingInfo != null && (!string.IsNullOrEmpty(this.ThemedCssFolderUrl) || WebSettingInfo.ThemedColorUrl != null))//存在theme,需要还原
                    {
                        mSPWeb.RestoreTheme(WebSettingInfo, this.ThemedCssFolderUrl);
                    }
                    //       ThemedCssUrl always null
                    else if (WebSettingInfo != null && WebSettingInfo.ThemedCssUrl != null && !string.IsNullOrEmpty(WebSettingInfo.ThemedCssUrl.Value))
                    {
                        mSPWeb.ApplyTheme(WebSettingInfo.Theme.Value);
                    }
                    else if (!Object.Equals(mSPWeb, null) && !string.IsNullOrEmpty(mSPWeb.ThemedCssFolderUrl) && mIsRestoreWebSetting)//备份了setting信息并且为空，说明源端是default
                    {
                        this.ThmxTheme.RemoveThemeFromWeb(mSPWeb, false);
                    }
                    // - Following logic should be legacy logic for 10 style theme, can be obsolete
                    //子web的inherit属性，及时跟parent一致但是使用api操作后都会发生变化，所以这里需要确保一下
                    //if (!mSPWeb.IsRootWeb)
                    //{
                    //    try
                    //    {
                    //        if ((WebSettingInfo.WebTheme != null && WebSettingInfo.WebTheme.IsAvailable && WebSettingInfo.WebTheme.Value.InheritsThemedCssFolderUrl)
                    //            || (WebSettingInfo.InheritsThemedCssFolderUrl != null && WebSettingInfo.InheritsThemedCssFolderUrl.IsAvailable && WebSettingInfo.InheritsThemedCssFolderUrl.Value))
                    //        {
                    //            mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = "True";
                    //            mSPWeb.ThemedCssFolderUrl = mSPWeb.ParentWeb.ThemedCssFolderUrl;
                    //            mSPWeb.Update();
                    //        }
                    //    }
                    //    catch (AveSecurityTrimingException)
                    //    {
                    //        throw;
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        log.Log(AveLogLevel.ERROR, WrapperRestoreResource.InheritWebPropertyFailed, mSPWeb.Url, e);
                    //    }
                    //}
                }
                catch (AveSecurityTrimingException ex)
                {
                    report.AddDetail(new AveWrapperReportDto("SiteTheme", "SiteTheme", AveReportObjectType.SiteTheme, AveStatus.Skipped, "You don't have permission to restore site theme. " + ex.Message));
                    if (!Object.Equals(mSPWeb, null))
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while getting web cssFolderUrl. WebId:{0}, WebUrl:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, ex));
                        //mLog.Warn(e, "An error occurred while getting web cssFolderUrl. WebId:{0}, WebUrl:{1}", mSPWeb.ID, mSPWeb.Url);
                    }
                }
                catch (Exception e)
                {
                    if (!Object.Equals(mSPWeb, null))
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while getting web cssFolderUrl. WebId:{0}, WebUrl:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, e));
                        //mLog.Warn(e, "An error occurred while getting web cssFolderUrl. WebId:{0}, WebUrl:{1}", mSPWeb.ID, mSPWeb.Url);
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreAlternateCSSUrl()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAlternateCSSUrl"))
            {
#endif
                try
                {
                    if (!mIsRestoreWebSetting)
                    {
                        return;
                    }
                    else
                    {
                        //if (this.AlternateCSSUrl != null)//!string.IsNullOrEmpty(this.AlternateCSSUrl))
                        //{
                        //string filePath = this.AlternateCSSUrl.Substring(mSPWeb.ServerRelativeUrl.Length + 1);
                        //if (mSPWeb.GetFile(filePath).Exists)
                        //{
                        mSPWeb.AlternateCssUrl = this.AlternateCSSUrl;
                        mSPWeb.Update();
                        //}
                        //}
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    report.AddDetail(new AveWrapperReportDto("AlternateCSSUrl", "AlternateCSSUrl", AveReportObjectType.AlternateCSSUrl, AveStatus.Skipped, "You don't have permission to restore AlternateCSSUrl. " + ex.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore web's alternateCssUrl. web id:{0}, web url:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, ex));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore web's alternateCssUrl. web id:{0}, web url:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, e));
                    //throw e;
                }
#if PerformanceLog
            }
#endif
        }

        //update web author
        public void RestoreAuthor()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAuthor"))
            {
#endif
                if (!mIsRestoreWebSetting)
                {
                    return;
                }
                if (mQueryService != null && mWebSettingInfo != null && mWebSettingInfo.Author != null && mWebSettingInfo.Author.IsAvailable)
                {
                    mQueryService.UpdateWebsAuthorByNative(mAveSite.SPMembers.FindMemberId(mWebSettingInfo.Author.Value), mId);
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreWelcomePage()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreWelcomePage"))
            {
#endif
                try
                {
                    if (!mIsRestoreWebSetting)
                    {
                        return;
                    }
                    if (this.mAveSite.SPContextKind != AveContextKind.ClientObjectModel && AveSPEnv.IsMoss && mSPWeb.IsPublish && mWebSettingInfo != null
                        && mWebSettingInfo.WelcomePage != null && mWebSettingInfo.WelcomePage.IsAvailable && !String.IsNullOrEmpty(mWebSettingInfo.WelcomePage.Value))
                    {
                        if (!mWebSettingInfo.WelcomePage.Value.Equals(mSPWeb.RootFolder.WelcomePage))
                        {
                            try
                            {
                                mAveSite.Publishing.SetWelcomePage(mSPWeb, mWebSettingInfo.WelcomePage.Value);
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while Setting welcome page, Welcome Page: " + mWebSettingInfo.WelcomePage + ". Error:" + e.ToString());
                            }
                            return;
                        }
                    }
                    if (mWebSettingInfo != null && mWebSettingInfo.WelcomePage != null && mWebSettingInfo.WelcomePage.IsAvailable
                        && !mWebSettingInfo.WelcomePage.Value.Equals(mSPWeb.RootFolder.WelcomePage))
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(mWebSettingInfo.WelcomePage.Value))
                            {
                                IAveFolder folder = mSPWeb.RootFolder;
                                folder.WelcomePage = mWebSettingInfo.WelcomePage.Value;
                                log.Info("Update web {0} welcome page to empty", mSPWeb.Url);
                                folder.Update();
                                return;
                            }
                            IAveFile file = mSPWeb.GetFile(mWebSettingInfo.WelcomePage.Value);
                            if (file.Exists)
                            {
                                log.Info("Begin to update web {0} welcome page from {1} to {2}",mSPWeb.Url,mSPWeb.RootFolder.WelcomePage, mWebSettingInfo.WelcomePage.Value);
                                IAveFolder folder = mSPWeb.RootFolder;
                                folder.WelcomePage = mWebSettingInfo.WelcomePage.Value;
                                folder.Update();
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            log.Warn("An error occurred while Setting welcome page, Welcome Page: " + mWebSettingInfo.WelcomePage + ". Error:" + ex.ToString());
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    Common.ArgumentCheck.CheckNotNull(mWebSettingInfo);
                    log.Warn("An error occurred while Setting welcome page, Welcome Page: " + mWebSettingInfo.WelcomePage + ". Error:" + ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("WelcomePage", "WelcomePage", AveReportObjectType.WelcomePage, AveStatus.Skipped, "You don;t have permission to restore WelcomePage. " + ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreSiteLogoUrl()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreSiteLogoUrl"))
            {
#endif
                if (!mIsRestoreWebSetting)
                {
                    return;
                }
                try
                {
                    InnerRestoreSiteLogoUrl();
                }
                catch (AveSecurityTrimingException ex)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreSiteLogoUrl", "RestoreSiteLogoUrl", AveReportObjectType.SiteLogoUrl, AveStatus.Skipped, "You don't have permission to restore site logo. " + ex.Message));
                    log.Log(AveLogLevel.WARN, "Restore site logo url error. siteId: {0} siteUrl: {1} Exception: {2}", mSPWeb.ID, mSPWeb.Url, ex.ToString());
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Restore site logo url error. siteId: {0} siteUrl: {1} Exception: {2}", mSPWeb.ID, mSPWeb.Url, ex.ToString());
                    try
                    {
                        this.ParentSite.ReloadSite();
                        this.ReloadWeb();

                        InnerRestoreSiteLogoUrl();
                    }
                    catch (Exception ex1)
                    {
                        log.Log(AveLogLevel.ERROR, "Restore site logo url error. Exception: {0}", ex1.ToString());
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private void InnerRestoreSiteLogoUrl()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.InnerRestoreSiteLogoUrl"))
            {
#endif
                if (mWebSettingInfo != null && mWebSettingInfo.SiteLogoUrl != null)
                {
                    if (mWebSettingInfo.SiteLogoUrl != null && mWebSettingInfo.SiteLogoUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.SiteLogoUrl.Value))
                    {
                        if (IsLogoUrlInSiteCollection(mWebSettingInfo.SiteLogoUrl.Value))
                        {
                            mWebSettingInfo.SiteLogoUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.SiteLogoUrl.Value, mAveSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                        }
                    }
                    if (mWebSettingInfo.SiteLogoUrl.IsAvailable && mSPWeb.SiteLogoUrl != mWebSettingInfo.SiteLogoUrl.Value)
                    {
                        mSPWeb.SiteLogoUrl = mWebSettingInfo.SiteLogoUrl.Value;
                        mSPWeb.Update();
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private bool IsLogoUrlInSiteCollection(string siteLogoUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.IsLogoUrlInSiteCollection"))
            {
#endif
                siteLogoUrl = HttpUtility.UrlDecode(siteLogoUrl);
                bool isAbsoluteUrl = AveReplaceProcessor.IsAbsoluteUrl(siteLogoUrl);
                if (AveReplaceProcessor.IsSpecialUrl(siteLogoUrl))
                {
                    return false;
                }

                if (siteLogoUrl.StartsWith(mAveSite.SourceSiteInfo.Url.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                    || siteLogoUrl.StartsWith(mAveSite.SourceSiteInfo.ServerRelativeUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Compare(mAveSite.SourceSiteInfo.ServerRelativeUrl, "/", StringComparison.Ordinal) == 0)//root Sitecollection
                    {
                        if (isAbsoluteUrl)
                        {
                            int root = siteLogoUrl.IndexOf("/", 8, StringComparison.OrdinalIgnoreCase);
                            siteLogoUrl = siteLogoUrl.Substring(root, siteLogoUrl.Length - root);//获取相对路径
                        }
                        foreach (string managePath in mAveSite.SourceSiteInfo.Prefixes)
                        {
                            if (!string.IsNullOrEmpty(managePath))
                            {
                                if (siteLogoUrl.StartsWith("/" + managePath + "/", StringComparison.OrdinalIgnoreCase))
                                {
                                    return false;
                                }
                            }
                        }
                        return true;//RootSiteCollection下的Url
                    }
                    return true;
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        public void RestoreHiddenPageProperty()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreHiddenPageProperty"))
            {
#endif
                try
                {
                    if (!mIsRestoreWebSetting)
                    {
                        return;
                    }
                    if (mAveSite.SPContextKind == AveContextKind.ClientObjectModel || AveSPEnv.IsMoss)
                    {
                        LoadHiddenPages();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenPageProperty. error:{0}", ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("HiddenPageProperty", "HiddenPageProperty", AveReportObjectType.HiddenPageProperty, AveStatus.Skipped, "You don't have permission to restore HiddenPageProperty. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenPageProperty. error:{0}", e.ToString());
                    //mLog.Warn("An error occurred while RestoreHiddenPageProperty. error:{{0}", e.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        private void LoadHiddenPages()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.LoadHiddenPages"))
            {
#endif
                try
                {
                    #region --reload hidden pages
                    if (mWebSettingInfo != null && mWebSettingInfo.NavigationWebAndPage != null && mWebSettingInfo.NavigationWebAndPage.Value != null
                        && mWebSettingInfo.NavigationWebAndPage.Value.ContainsKey("Hidden") && mWebSettingInfo.NavigationWebAndPage.Value["Hidden"] != null
                        && mWebSettingInfo.NavigationWebAndPage.Value["Hidden"].ContainsKey("page") && mWebSettingInfo.NavigationWebAndPage.Value["Hidden"]["page"] != null)
                    {
                        if (mQueryService != null)
                        {
                            Dictionary<Guid, string> hiddenPages = mWebSettingInfo.NavigationWebAndPage.Value["Hidden"]["page"];
                            mQueryService.LoadHiddenPages(hiddenPages, ParentSite.MappingManager.WebMappingManager.PageItemSDGuidMapping, mAveSite.MappingManager.SiteMappingManager.ListUrlMapping, mAveSite.SPSite.ID, mSPWeb);
                        }
                    }
                    foreach (Guid oldPageId in ParentSite.MappingManager.WebMappingManager.PageItemSDGuidMapping.Keys)
                    {
                        if (!ParentSite.MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(oldPageId))
                        {
                            ParentSite.MappingManager.SiteMappingManager.HiddenWebsPages.Add(oldPageId, ParentSite.MappingManager.WebMappingManager.PageItemSDGuidMapping[oldPageId]);
                        }
                    }
                    foreach (Guid oldPageId in ParentSite.MappingManager.WebMappingManager.PageItemONGuidMapping.Keys)
                    {
                        if (!ParentSite.MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(oldPageId))
                        {
                            ParentSite.MappingManager.SiteMappingManager.HiddenWebsPages.Add(oldPageId, ParentSite.MappingManager.WebMappingManager.PageItemONGuidMapping[oldPageId]);
                        }
                    }
                    #endregion
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReloadHiddenWebOrPagesFailed, e);
                }

                #region --reload hidden web property
                try
                {
                    if (mWebSettingInfo != null && mWebSettingInfo.NavigationWebAndPage != null && mWebSettingInfo.NavigationWebAndPage.Value != null
                         && mWebSettingInfo.NavigationWebAndPage.Value.ContainsKey("Hidden") && mWebSettingInfo.NavigationWebAndPage.Value["Hidden"] != null
                          && mWebSettingInfo.NavigationWebAndPage.Value["Hidden"].ContainsKey("web") && mWebSettingInfo.NavigationWebAndPage.Value["Hidden"]["web"] != null)
                    {
                        if (mQueryService != null)
                        {
                            Dictionary<Guid, Guid> HiddenWebMapping = mQueryService.ReloadHiddenWebProperty(mAveSite.SPSite.ID, mWebSettingInfo, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl, ParentSite.MappingManager.SiteMappingManager.WebIDMapping);//new Dictionary<Guid, Guid>();
                            ParentSite.MappingManager.SiteMappingManager.AddHiddenWebPage(HiddenWebMapping);
                        }
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReloadHiddenWebOrPagesFailed, e);
                }
                #endregion
#if PerformanceLog
            }
#endif
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Web property about associate groups.")]
        public void RestoreAssociateGroups()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAssociateGroups"))
            {
#endif
                try
                {
                    if (!mIsRestoreWebSetting)
                    {
                        return;
                    }
                    if (MetaInfoDictionary != null)
                    {
                        bool needUpdate = false;
                        if (MetaInfoDictionary.ContainsKey("vti_associategroups"))
                        {
                            string associateGroups = MetaInfoDictionary["vti_associategroups"];
                            if (!string.IsNullOrEmpty(associateGroups))
                            {
                                List<int> list = new List<int>();
                                foreach (string str2 in associateGroups.Split(new char[] { ';' }))
                                {
                                    int num2;
                                    if ((!string.IsNullOrEmpty(str2) && int.TryParse(str2, NumberStyles.Integer, CultureInfo.InvariantCulture, out num2))
                                        && ((num2 > 0) && !list.Contains(num2)))
                                    {
                                        list.Add(num2);
                                    }
                                }
                                StringBuilder builder = new StringBuilder();
                                bool restoreGroup = false;
                                if (list.Count > 0)
                                {
                                    foreach (int i in list)
                                    {
                                        if (builder.Length > 0)
                                        {
                                            builder.Append(';');
                                        }
                                        IAvePrincipal p = this.ParentSite.SPMembers.FindMember(i, false, true);
                                        if (p != null)
                                        {
                                            builder.Append(p.ID.ToString());
                                            restoreGroup = true;
                                        }
                                    }
                                }
                                if (restoreGroup)
                                {
                                    mSPWeb.AllProperties["vti_associategroups"] = builder.ToString();
                                    needUpdate = true;
                                }
                            }
                        }
                        if (MetaInfoDictionary.ContainsKey("vti_associateownergroup"))
                        {
                            string associateOwnerGroup = MetaInfoDictionary["vti_associateownergroup"];
                            int groupId = 0;
                            if ((!string.IsNullOrEmpty(associateOwnerGroup) && int.TryParse(associateOwnerGroup, out groupId)) && (groupId > 0))
                            {
                                IAvePrincipal p = this.ParentSite.SPMembers.FindMember(groupId, false, true);
                                if (p != null)
                                {
                                    groupId = p.ID;
                                    mSPWeb.AllProperties["vti_associateownergroup"] = groupId.ToString();
                                    needUpdate = true;
                                    log.Info("NeedUpdate assciateownergroup {0}",groupId);
                                }
                            }
                        }
                        if (MetaInfoDictionary.ContainsKey("vti_associatemembergroup"))
                        {
                            string associateMemberGroup = MetaInfoDictionary["vti_associatemembergroup"];
                            int groupId = 0;
                            if ((!string.IsNullOrEmpty(associateMemberGroup) && int.TryParse(associateMemberGroup, out groupId)) && (groupId > 0))
                            {
                                IAvePrincipal p = this.ParentSite.SPMembers.FindMember(groupId, false, true);
                                if (p != null)
                                {
                                    groupId = p.ID;
                                    mSPWeb.AllProperties["vti_associatemembergroup"] = groupId.ToString();
                                    needUpdate = true;
                                    log.Info("NeedUpdate assciatemembergroup {0}",groupId);
                                }
                            }
                        }
                        if (MetaInfoDictionary.ContainsKey("vti_associatevisitorgroup"))
                        {
                            string associateVisitorGroup = MetaInfoDictionary["vti_associatevisitorgroup"];
                            int groupId = 0;
                            if ((!string.IsNullOrEmpty(associateVisitorGroup) && int.TryParse(associateVisitorGroup, out groupId)) && (groupId > 0))
                            {
                                IAvePrincipal p = this.ParentSite.SPMembers.FindMember(groupId, false, true);
                                if (p != null)
                                {
                                    groupId = p.ID;
                                    mSPWeb.AllProperties["vti_associatevisitorgroup"] = groupId.ToString();
                                    needUpdate = true;
                                    log.Info("NeedUpdate assciatevisitorgroup {0}",groupId);
                                }
                            }
                        }
                        if (needUpdate)
                        {
                            mSPWeb.Update();
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreAssociateGroup", "RestoreAssociateGroup", AveReportObjectType.AssociteGroup, AveStatus.Skipped, "You don't have permission to restore associate groups. " + ex.Message));
                    log.Warn("update associategroups exception" + ex.ToString());
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTeSPWeb1060", mSPWeb.Url,  e);
                    log.Warn("update associategroups exception" + e.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        public int GetUserIdByName(string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetUserIdByName"))
            {
#endif
                IAveGroup spGroup = null;
                IAveUser spUser = null;
                int id = -1;
                try
                {
                    spGroup = mSPWeb.SiteGroups[name];
                    if (spGroup != null)
                    {
                        id = spGroup.ID;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserIdByNameFailed, name, e);
                }
                if (spGroup == null)
                {
                    try
                    {
                        name = ParentSite.SPMembers.GetMappingUserLogin(name, true);
                        spUser = mSPWeb.SiteUsers[name];
                        if (spUser != null)
                        {
                            id = spUser.ID;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetUserIdByNameFailed, name, e);
                    }
                }
                return id;
#if PerformanceLog
            }
#endif
        }
        public int GetUserIdByUserName(string userName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetUserIdByUserName"))
            {
#endif
                int id = -1;
                try
                {
                    foreach (IAveUser user in mSPWeb.SiteUsers)
                    {
                        if (user.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        {
                            return user.ID;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserIdByNameFailed, userName, e);
                }
                try
                {
                    foreach (IAveGroup group in mSPWeb.SiteGroups)
                    {
                        if (group.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        {
                            return group.ID;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetUserIdByNameFailed, userName, e);
                }
                return id;
#if PerformanceLog
            }
#endif
        }
        public void RestorePostUserInfo()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestorePostUserInfo"))
            {
#endif
                if (mSPWeb == null || !mSPWeb.IsRootWeb)
                {
                    return;
                }
                try
                {
                    foreach (string key in ParentSite.MappingManager.WebMappingManager.PostUserInfo.Keys)
                    {
                        //string name = key;[RECO-20916]Fortify Scan issue, Privacy Violation:Heap Inspection.
                        int id = GetUserIdByName(key);
                        if (id > 0)
                        {
                            try
                            {
                                Dictionary<string, object> fieldData = ParentSite.MappingManager.WebMappingManager.PostUserInfo[key];
                                IAveList userInfoList = mSPWeb.SiteUserInfoList;
                                IAveListItem listItem = userInfoList.GetItemById(id);
                                ParentSite.ObjectModelFactory.CreateAveItem(mSPWeb.Site).AddFields(listItem, fieldData, new AveBaseItemInfo());//TODOLMM
                                listItem.SystemUpdate(false);
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore web post userInfo. key:{0}\n error message:{1}", key, e));
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore web post userInfo. ", ex);
                    report.AddDetail(new AveWrapperReportDto("PostUserInfo", "PostUserInfo", AveReportObjectType.RestorePostUserInfo, AveStatus.Skipped, "You don't have permission to restore post user info. " + ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        public void ReloadWeb()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ReloadWeb"))
            {
#endif
                try
                {
                    if (mSPWeb != null)
                    {
                        mSPWeb.ReloadWeb();
                        InitializeMembers();
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Reload web failed. web name:{0}\n error message:{1}", mName, e));
                    //mLog.Warn("Reload web:{0} failed:{1}.", mName, e.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 如果程序运行一天以上，访问Web的一些属性，例如WebPartManager或者CreatList对象，都会出现如下错误：
        /// System.Runtime.InteropServices.COMException (0x80090317): The context has expired and can no longer be used. 
        /// </summary>
        /// <param name="ingoreTimeout"></param>
        internal void ReloadWebAndParentInternalForSPRequestTimeout(bool ingoreTimeout)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ReloadWebAndParentInternalForSPRequestTimeout"))
            {
#endif
                if (ingoreTimeout || ParentSite.mSPRequestTimeout.AddHours(ParentSite.mHoursReloadSite) < DateTime.UtcNow)
                {
                    this.ParentSite.ReloadSite();
                    this.ReloadWeb();
                }
#if PerformanceLog
            }
#endif
        }

        //public void AddUnRestoreWebPartInfo(string listTitle, Guid fileId, AveWebPartBaseInfo info)
        //{
        //    if (!this.ParentSite.MappingManager.WebMappingManager.UnRestoreWebPartCache.ContainsKey(listTitle))
        //    {
        //        this.ParentSite.MappingManager.WebMappingManager.UnRestoreWebPartCache.Add(listTitle, new Dictionary<string, List<AveWebPartBaseInfo>>());
        //    }
        //    if (!this.ParentSite.MappingManager.WebMappingManager.UnRestoreWebPartCache[listTitle].ContainsKey(fileId.ToString()))
        //    {
        //        this.ParentSite.MappingManager.WebMappingManager.UnRestoreWebPartCache[listTitle].Add(fileId.ToString(), new List<AveWebPartBaseInfo>());
        //    }
        //    this.ParentSite.MappingManager.WebMappingManager.UnRestoreWebPartCache[listTitle][fileId.ToString()].Add(info);
        //}

        public void ClearDefaultList()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ClearDefaultList"))
            {
#endif
                try
                {
                    IAveListCollection allList = mSPWeb.Lists;
                    List<Guid> allListsId = new List<Guid>();
                    string listTitle = string.Empty;
                    foreach (IAveList list in allList)
                    {
                        allListsId.Add(list.ID);
                    }
                    IAveList spList = null;
                    foreach (Guid id in allListsId)
                    {
                        try
                        {
                            spList = allList[id];
                            listTitle = spList.Title;
                            spList.Delete();
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while deleting default lists, System will recycle the items under this list [{0}]. error:{1}", listTitle, e.ToString());
                            List<int> allItemsId = new List<int>();
                            foreach (IAveListItem item in spList.Items)
                            {
                                allItemsId.Add(item.ID);
                            }
                            IAveListItem spItem = null;
                            foreach (int itemId in allItemsId)
                            {
                                try
                                {
                                    spItem = spList.GetItemById(itemId);
                                    spItem.Recycle();
                                }
                                catch (Exception e1)
                                {
                                    log.Log(AveLogLevel.WARN, "An error occurred while recycle item in list:{0}, item id:{1}, error:{2}.", listTitle, itemId, e1.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception e2)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while clearing default list of new created web:{0}. error:{1}", mSPWeb.Url, e2.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreCacheProfileListId()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreCacheProfileListId"))
            {
#endif
                try
                {
                    if (MetaInfoDictionary == null || !MetaInfoDictionary.ContainsKey("__CacheProfileListId"))
                    {
                        return;
                    }
                    string cacheProfileListId = MetaInfoDictionary["__CacheProfileListId"];
                    Guid oldcacheProfileListId = new Guid(cacheProfileListId);
                    if (string.IsNullOrEmpty(cacheProfileListId) || !ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(oldcacheProfileListId))
                    {
                        return;
                    }
                    string webCacheProfileListId = mSPWeb.AllProperties["__CacheProfileListId"].ToString();
                    string newCacheProfileListId = ParentSite.MappingManager.SiteMappingManager.ListIdMapping[oldcacheProfileListId].ToString();
                    if (!webCacheProfileListId.Equals(newCacheProfileListId, StringComparison.OrdinalIgnoreCase))
                    {
                        mSPWeb.AllProperties["__CacheProfileListId"] = newCacheProfileListId;
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("an error occurred while restore the web's CacheProfileListId in metainfo.\n error message:" + ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("CacheProfileListId", "CacheProfileListId", AveReportObjectType.CacheProfileListId, AveStatus.Skipped, "You don't have permission to restore CacheProfileListId. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Warn("an error occurred while restore the web's CacheProfileListId in metainfo.\n error message:" + e.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreRelationShipListSetting()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreRelationShipListSetting"))
            {
#endif
                try
                {
                    if (MetaInfoDictionary == null || !MetaInfoDictionary.ContainsKey("_VarRelationshipsListId"))
                    {
                        return;
                    }
                    string varRelationshipsListId = MetaInfoDictionary["_VarRelationshipsListId"];
                    Guid oldRelationShipListId = new Guid(varRelationshipsListId);
                    if (string.IsNullOrEmpty(varRelationshipsListId) || !ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(oldRelationShipListId))
                    {
                        return;
                    }
                    string webVarRelationshipsListId = mSPWeb.AllProperties["_VarRelationshipsListId"].ToString();
                    string siteVarRelationshipsListId = ParentSite.MappingManager.SiteMappingManager.ListIdMapping[oldRelationShipListId].ToString();
                    if (!webVarRelationshipsListId.Equals(siteVarRelationshipsListId, StringComparison.OrdinalIgnoreCase))
                    {
                        mSPWeb.AllProperties["_VarRelationshipsListId"] = siteVarRelationshipsListId;
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("RelationShipListSetting", "RelationShipListSetting", AveReportObjectType.RelationShipListSetting, AveStatus.Skipped, "You don't have permission to restore RelationShipListSetting. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + e.ToString());
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore relationshipList setting. \n error message:{0}", e));
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// ADO-26529 开启content orginazer feature之后还原到目的端，目的端site content orginazer setting无法打开
        /// 查看SharePoint log以及reflector了解到该页面在加载的时候会通过web property中的emailsubmittedrecordslistid属性来先获取
        /// 目的端的Submitted E-mail Records这个list，这个属性是该list的guid，如果不进行替换，会出现无法找到list而页面加载错误的现象
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "emailsubmittedrecordslistid is a key")]
        public void RestoreEmailSubmittedRecordsListIDProperty()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreEmailSubmittedRecordsListIDProperty"))
            {
#endif
                try
                {
                    if (MetaInfoDictionary == null || !MetaInfoDictionary.ContainsKey("emailsubmittedrecordslistid"))
                    {
                        return;
                    }
                    string EmailSubmittedRecordsListID = MetaInfoDictionary["emailsubmittedrecordslistid"];
                    Guid oldEmailSubmittedRecordsListID = new Guid(EmailSubmittedRecordsListID);
                    Guid newEmailSubmittedRecordsListID = Guid.Empty;
                    #region find Submitted E-mail Records list by mapping manager or using API
                    if (string.IsNullOrEmpty(EmailSubmittedRecordsListID) || !ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(oldEmailSubmittedRecordsListID))
                    {
                        try
                        {
                            IAveList emailSubmittedRecordsList = mSPWeb.Lists["Submitted E-mail Records"];
                            newEmailSubmittedRecordsListID = emailSubmittedRecordsList.ID;
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "Can not get List Submitted E-mail Records, Exception info:" + e.ToString());
                        }
                    }
                    else
                    {
                        newEmailSubmittedRecordsListID = ParentSite.MappingManager.SiteMappingManager.ListIdMapping[oldEmailSubmittedRecordsListID];
                    }
                    #endregion
                    string weboldEmailSubmittedRecordsListID = mSPWeb.AllProperties["emailsubmittedrecordslistid"].ToString();
                    if (!weboldEmailSubmittedRecordsListID.Equals(newEmailSubmittedRecordsListID.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        mSPWeb.AllProperties["emailsubmittedrecordslistid"] = newEmailSubmittedRecordsListID.ToString();
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("EmailSubmittedRecordsListIDProperty", "EmailSubmittedRecordsListIDProperty", AveReportObjectType.EmailSubmittedRecordsListIDProperty, AveStatus.Skipped, "You don't have permission to RestoreEmailSubmittedRecordsListIDProperty. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + e.ToString());
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore relationshipList setting. \n error message:{0}", e));
                }
#if PerformanceLog
            }
#endif
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Japanese.")]
        public void RestoreOriginTitle()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreOriginTitle"))
            {
#endif
                if (!mIsRestoreWebSetting)
                {
                    return;
                }
                log.Debug("Start site bin restore original title.");
                try
                {
                    //Restore the original title for Site Bin function
                    string siteBinTitle = "(This site is being backed up by AvePoint before being deleted, please wait...)";
                    string siteBinTitle_JP = "(このサイトは DocAve が削除前のバックアップを実行しています。しばらくお待ちください。)";
                    if (mWebInfo.Title.Contains(siteBinTitle) || mWebInfo.Title.Contains(siteBinTitle_JP))
                    {
                        mSPWeb.Title = mSPWeb.Title.Replace(siteBinTitle, string.Empty);
                        mSPWeb.Title = mSPWeb.Title.Replace(siteBinTitle_JP, string.Empty);
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("RestoreOriginalTitle failed: " + ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("OriginTitle", "OriginTitle", AveReportObjectType.OriginTitle, AveStatus.Skipped, "You don't have permission to restore OriginTitle. " + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Warn("RestoreOriginalTitle failed: " + ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// If ContentOrginazer (SP2010) feature activated in source, we need to update the web properties base on it
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        public void RestoreContentOrginazerSetting()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreContentOrginazerSetting"))
            {
#endif
                try
                {
                    if (MetaInfoDictionary == null)
                    {
                        return;
                    }
                    string[] mRouterMetaInfos = new string[]{
                        "_routerversioning",
                        "_routersaveauditlogs",
                        "_routerautofoldersettings",
                        "_dlc_repositoryusersgroup",
                        "_routermanageremail",
                        "_routerenablecrosssiterouting",
                        "_routerstalecontentthreshold",
                        "client_routerenforcerouting",
                        "_routeremailforstalecontent",
                        "_routeremailforproblems"};
                    bool needUpdate = false;
                    foreach (string key in mRouterMetaInfos)
                    {
                        if (MetaInfoDictionary.ContainsKey(key))
                        {
                            if (key.Equals("_dlc_repositoryusersgroup", StringComparison.OrdinalIgnoreCase))
                            {
                                int newAddGroupId = -1;
                                if (mSPWeb.AllProperties.ContainsKey(key) && mSPWeb.AllProperties[key] != null)
                                {
                                    newAddGroupId = Convert.ToInt32(mSPWeb.AllProperties[key]);
                                }
                                int oldId = Convert.ToInt32(MetaInfoDictionary[key].Replace("\\\\", "\\"));
                                int groupId = mAveSite.SPMembers.FindMemberId(oldId);
                                mSPWeb.AllProperties[key] = groupId;
                                if (this.IsNewCreated && newAddGroupId != -1 && newAddGroupId != groupId)
                                {
                                    this.ParentSite.SPSite.RootWeb.Groups.RemoveByID(newAddGroupId);
                                }
                            }
                            else
                            {
                                mSPWeb.AllProperties[key] = MetaInfoDictionary[key].Replace("\\\\", "\\");
                            }
                            needUpdate = true;
                        }
                    }
                    if (needUpdate)
                    {
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    report.AddDetail(new AveWrapperReportDto("ContentOrganizationSetting", "ContentOrganizationSetting", AveReportObjectType.ContentOrganizationSetting, AveStatus.Skipped, "You don't permission to restore ContentOrganizationSetting. " + ex.Message));
                    log.Warn("There is an error when update \"Content Organizer\" feature, please try again or configure manually. \t", ex);
                }
                catch (Exception ex)
                {
                    log.Warn("There is an error when update \"Content Organizer\" feature, please try again or configure manually. \t", ex);
                }
#if PerformanceLog
            }
#endif
        }

        //private void EnsureMeetingWebHasItem()
        //{
        //    try
        //    {
        //        IAveList list = mSPWeb.Lists["meeting series"];
        //        if (list.BaseTemplate == AveListTemplateType.Meetings)
        //        {
        //            if (list.Items.Count == 0)
        //            {
        //                IAveListItem listItem = list.Items.Add();

        //                listItem["EventType"] = 0;
        //                listItem["EventDate"] = DateTime.Now;
        //                listItem["EndDate"] = DateTime.Now;
        //                listItem["EventUID"] = "STSTeamCalendarEvent:List:{" + list.ID.ToString().ToUpper() + "}:Item:1";
        //                listItem.Update();
        //            }
        //        }
        //    }
        //    catch
        //    { }
        //}

        private void OutputAccessRequestSettings(IAveWeb web,AveWebSettingInfo webSetting)
        {
            try
            {
                StringBuilder auditLog = new StringBuilder();
                auditLog.AppendLine("OutputAccessRequestSettings before update");
                if (web.AssociatedMemberGroup != null)
                {
                    auditLog.AppendLine($"[AllowMembersEditMembership][{web.AssociatedMemberGroup.AllowMembersEditMembership}][{webSetting.AllowMembersEditMembership}]");
                }
                else
                {
                    auditLog.AppendLine($"[AllowMembersEditMembership][web.AssociatedMemberGroup is null][{webSetting.AllowMembersEditMembership}]");
                }
                auditLog.AppendLine($"[UseAccessRequestDefault][{web.UseAccessRequestDefault}][{webSetting.UseAccessRequestDefault}]");
                auditLog.AppendLine($"[RequestAccessEmail][{web.RequestAccessEmail}][{webSetting.RequestAccessEmail}]");
                auditLog.AppendLine($"[MembersCanShare][{web.MembersCanShare}][{webSetting.MembersCanShare}]");
                auditLog.AppendLine($"[AccessRequestSiteDescription][{web.AccessRequestSiteDescription}][{webSetting.AccessRequestSiteDescription}]");
                log.Info(auditLog.ToString());
            }
            catch (Exception e)
            {
                log.Error("OutputAccessRequestSettings failed due to {0}", e);
            }
        }

        /// <summary>
        /// 由于还原web property的时候，可能还没有打破继承，所以这个属性要放到post action里面还原
        /// </summary>
        public void RestoreRequestAccessEmail()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreRequestAccessEmail"))
            {
#endif
            
            try
            {
                OutputAccessRequestSettings(mSPWeb, mWebSettingInfo);
                bool changed = false;
                if (mSPWeb.HasUniqueRoleAssignments && mWebSettingInfo != null)
                {
                    if (mWebSettingInfo.AllowMembersEditMembership != null 
                        && mWebSettingInfo.AllowMembersEditMembership.IsAvailable
                        && mSPWeb.AssociatedMemberGroup != null
                        && mSPWeb.AssociatedMemberGroup.AllowMembersEditMembership != mWebSettingInfo.AllowMembersEditMembership.Value)
                    {
                        mSPWeb.AssociatedMemberGroup.AllowMembersEditMembership = mWebSettingInfo.AllowMembersEditMembership.Value;
                        mSPWeb.AssociatedMemberGroup.Update();
                    }
                    if (mWebSettingInfo.UseAccessRequestDefault != null 
                        && mWebSettingInfo.UseAccessRequestDefault.IsAvailable
                        && mSPWeb.UseAccessRequestDefault != mWebSettingInfo.UseAccessRequestDefault.Value)
                    {
                        mSPWeb.UseAccessRequestDefault = mWebSettingInfo.UseAccessRequestDefault.Value;
                        changed = true;
                    }
                    if (mWebSettingInfo.RequestAccessEmail != null 
                        && mWebSettingInfo.RequestAccessEmail.IsAvailable
                        && mSPWeb.RequestAccessEmail != mWebSettingInfo.RequestAccessEmail.Value)
                    {
                        mSPWeb.RequestAccessEmail = mWebSettingInfo.RequestAccessEmail.Value;
                        changed = true;
                    }
                    if (mWebSettingInfo.MembersCanShare != null 
                        && mWebSettingInfo.MembersCanShare.IsAvailable
                        && mSPWeb.MembersCanShare != mWebSettingInfo.MembersCanShare.Value)
                    {
                        mSPWeb.MembersCanShare = mWebSettingInfo.MembersCanShare.Value;
                        changed = true;
                    }
                }
                //inherit permisison subsite can also set this property
                if ( mWebSettingInfo != null && mWebSettingInfo.AccessRequestSiteDescription != null
                       && mWebSettingInfo.AccessRequestSiteDescription.IsAvailable
                       && mSPWeb.AccessRequestSiteDescription != mWebSettingInfo.AccessRequestSiteDescription.Value)
                {
                    mSPWeb.AccessRequestSiteDescription = mWebSettingInfo.AccessRequestSiteDescription.Value;
                    changed = true;
                }
                if (changed)
                {
                    mSPWeb.Update();
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                report.AddDetail(new AveWrapperReportDto("RequestAccessEmail", "RequestAccessEmail", AveReportObjectType.RequestAccessEmail, AveStatus.Skipped, "You don't have permission to restore request access email. " + ex.Message));
                log.Warn("An error occurred while set web RequestAccessEmail. error:{0}", ex.ToString());
                //this.ReloadWeb();
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while set web RequestAccessEmail. error:{0}", e.ToString());
                //this.ReloadWeb();
            }

#if PerformanceLog
            }
#endif
        }

        internal void RestoreWorkflowStartOptions()
        {
            try
            {
                WFConflictResolution wfResolution = WFConflictResolution.Instance;
                wfResolution.UpdateWorkflowStartOptions(this);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occured when restore workflow startOptions error: {0}", e);
            }
        }

        /// <summary>
        /// only for community site
        /// </summary>
        public void RestoreWebIndexedProperty()
        {
            if (this.SPWeb.WebTemplate.Equals(AveCommunitiesConstants.CommunityTemplateName, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    IAveList discussionList = this.SPWeb.GetListByName(CommunitySiteDiscussionsListTitle, true);
                    IAveList memberList = this.SPWeb.GetList("/Lists/Members");
                    int topicCount = discussionList.RootFolder.ItemCount;
                    int replyCount = discussionList.ItemCount - topicCount;
                    int memberCount = memberList.ItemCount;
                    this.SPWeb.AllProperties["Community_TopicsCount"] = topicCount.ToString();
                    this.SPWeb.AllProperties["Community_RepliesCount"] = replyCount.ToString();
                    this.SPWeb.AllProperties["Community_MembersCount"] = memberCount.ToString();
                    this.SPWeb.Update();
                }
                catch (Exception e)
                {
                    log.Warn("Update web indexed property failed. Error message:{0}", e.Message);
                }
            }
        }

        /// <summary>
        /// 重新计算community site下的统计信息
        /// </summary>
        public void ReCalculateForCommunitySite()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ReCalculateForCommunitySite"))
            {
#endif
                try
                {
                    if (SPWeb.Features[new Guid("961D6A9C-4388-4CF2-9733-38EE8C89AFD4")] == null)
                    {
                        return;
                    }
                    //if (!IsNewCreated && !string.IsNullOrEmpty(CommunitySiteDiscussionsListTitle))
                    if (!string.IsNullOrEmpty(CommunitySiteDiscussionsListTitle))
                    {
                        IAveList discussionList = SPWeb.GetListByName(CommunitySiteDiscussionsListTitle, false);
                        Dictionary<int, int> itemIdmapping = new Dictionary<int, int>();
                        this.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.TryGetValue(discussionList.ID, out itemIdmapping);
                        SPWeb.RecalculateForCommunitySite(discussionList, itemIdmapping);
                    }
                    bool changed = false;
                    if (SPWeb.AllProperties != null && SPWeb.AllProperties.ContainsKey("Community_MembersCount"))
                    {
                        SPWeb.AllProperties["Community_MembersCount"] = "-1";
                        changed = true;
                    }
                    if (SPWeb.AllProperties != null && SPWeb.AllProperties.ContainsKey("Community_RepliesCount"))
                    {
                        SPWeb.AllProperties["Community_RepliesCount"] = "-1";
                        changed = true;
                    }
                    if (SPWeb.AllProperties != null && SPWeb.AllProperties.ContainsKey("Community_TopicsCount"))
                    {
                        SPWeb.AllProperties["Community_TopicsCount"] = "-1";
                        changed = true;
                    }
                    if (changed)
                    {
                        SPWeb.Update();
                        SPWeb.Properties.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while post update community site property. Error:{0}", ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        public void AddNintexFormControlTypeMapping(Guid listId, string contentTypeId, Dictionary<Guid, AveNintexFormControlType> uniqueIdMapping, Dictionary<string, AveNintexFormControlType> displayNameMapping)
        {
            lock (nintexFormControlTypeCache)
            {
                if (!nintexFormControlTypeCache.ContainsKey(listId))
                {
                    nintexFormControlTypeCache[listId] = new Dictionary
                <string, Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>
                    >();
                }
                nintexFormControlTypeCache[listId][contentTypeId.ToLower()] =
             new Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>(
                   uniqueIdMapping, displayNameMapping);
            }
        }

        public void OutputAllListInformation(StringBuilder logBuilder)
        {
            try
            {
                logBuilder.AppendLine(string.Format("Output All Lists in web {0}", this.SPWeb?.Url));
                var lists = this.SPWeb?.Lists;
                if(lists == null)
                {
                    throw new NullReferenceException("lists is null");
                }
                foreach (var list in lists)
                {
                    logBuilder.AppendLine(string.Format("[{0}][{1}][{2}]", list.Title, list.RootFolder?.ServerRelativeUrl, list.BaseTemplate));
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occured when OutputAllListInformation due to {0}", e);
            }
        }
    }
}