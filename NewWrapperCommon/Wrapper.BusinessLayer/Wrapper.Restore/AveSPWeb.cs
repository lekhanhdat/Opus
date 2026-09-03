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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Linq;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Extension;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using LS.SPWorkflowProcessor;
using AvePoint.Wrapper.Core.SPRestore.Mapping;
using System.Text.RegularExpressions;
using System.Collections;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Restore.NintexForm;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1,
                       CodeReviewConstants.CHECK_LIST_ID_CO_6,
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    public class AveSPWeb : RestoreableObject, IDisposable, AvePoint.Wrapper.Restore.IAveSPWeb
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPWeb));

        private AveSPSite mAveSite = null;
        protected string mName = string.Empty;
        private Guid mId = Guid.Empty;
        private IAveWeb mSPWeb = null;
        private IAveBackupRestoreQueryService mQueryService = null;
        private long mDataSize = 0;
        private bool mIsNewCreated = false;
        private uint mLanguageForNewCreatedWeb = 0;
        private string mScope = string.Empty;
        private AveWebInfo mWebInfo = null;
        private AveWebSettingInfo mWebSettingInfo = null;
        public bool needListRestore = false;
        private bool mIsRestoreWebNavgation = false;
        private AveSPWebContentTypeCollection mContentTypes;
        private AveSPWebFieldCollection mFields;
        private Guid mOldId = Guid.Empty;
        private Guid mTaxonomyHiddenList = Guid.Empty;
        private RestoringDto mRestoringWeb = new RestoringDto();
        public List<AveRoleInfo> Roles { get; set; }
        private bool mNeedContinue = true;
        public string ThemeTitle { get; set; }

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

        private Dictionary<Guid, Dictionary<Guid, WrokflowEnableModel>> workflowEnableCache = new Dictionary<Guid, Dictionary<Guid, WrokflowEnableModel>>();

        public bool NeedContinue
        {
            get { return this.mNeedContinue; }
            set { this.mNeedContinue = value; }
        }

        public RestoringDto RestoringWeb
        {
            get { return mRestoringWeb; }
        }

        private bool mNeedSkipMicroFeedList = false;
        public bool NeedSkipMicroFeedList
        {
            get { return mNeedSkipMicroFeedList; }
            set { mNeedSkipMicroFeedList = value; }
        }

        public int AppEditorId
        { get; set; }

        public int AppAuthorId
        { get; set; }

        public bool SecurityRestored = true;

        public Dictionary<string, string> MetaInfoDictionary = null;//mWebMetaInfoDictionary
        public Dictionary<String, Object> MetaInfoDictionaryWithType = null;
        internal List<Guid> ActivatedWebFeatureIDs = new List<Guid>();

        //add for master page setting restore
        private string mAlternateCSSUrl = null;

        private uint mSrcLanguageId;

        private AveSPNavigation mNavigation;
        private AveWebFeature mFeature;
        private AveWebSecurity mWebSecurity;
        private IAveSPMembers mMembers;
        private IAveWeb mParentAveWeb;
        private IAveThmxTheme mThmxTheme;
        private IAveRestoreStream mRestoreStream;
        private string mWebUrl;
        private string mUrl;
        private string mSrcUrl;
        private long mSize;
        private Dictionary<Guid, Dictionary<Guid, Guid>> listAlertIdMappings;
        private IReport report = new AveWrapperReport();

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
        private bool inheritAlertCss = false;
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

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetRoleByName"))
            {

                foreach (AveRoleInfo roleinfo in Roles)
                {
                    if (roleinfo.Title.Equals(roleName))
                    {
                        return roleinfo;
                    }
                }
                return null;

            }

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

        private IAveListMapping listMapping;

        internal IAveListMapping ListMapping
        {
            get
            {
                if (this.listMapping == null)
                {
                    this.listMapping = new AveListMapping(null);
                }
                return this.listMapping;
            }
        }

        public void SetListTitleMapping(Dictionary<string, string> mapping)
        {
            this.listMapping = new AveListMapping(mapping);
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

        public IAveSPMembers Members
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

        private bool hasInitialAveWeb = false;
        public IAveWeb AveWeb
        {
            get
            {
                try
                {
                    if (mAveSite != null && !hasInitialAveWeb && mAveSite.AveSite != null && mSPWeb == null && !string.IsNullOrEmpty(Name))
                    {
                        if (mName == ".")
                        {
                            // SPSite.RootWeb will not create new webs if you call it several
                            // times. It will return same SPWeb when you call it. So we create
                            // a new SPWeb in case we dispose it when we call AveSPWeb.Dispose().
                            IAveWeb rootWeb = mAveSite.AveSite.RootWeb;
                            mSPWeb = mAveSite.AveSite.OpenWeb(rootWeb.ID);
                        }
                        else
                        {
                            mSPWeb = GetWebInSite(mAveSite.AveSite, mName);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Debug(string.Format("Get AveWeb error.Exception:{0}", e.ToString()));
                }
                finally
                {
                    hasInitialAveWeb = true;
                }
                return mSPWeb;
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
            if (aveSite.SPSite.IsOnlineSite)
            {
                mMembers = new AveSPMembersMultiThread(mAveSite);
            }
            else
            {
                mMembers = new AveSPMembers(mAveSite);
            }
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
            if (aveSite.SPSite.IsOnlineSite)
            {
                mMembers = new AveSPMembersMultiThread(mAveSite);
            }
            else
            {
                mMembers = new AveSPMembers(mAveSite);
            }
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

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetFolderByRelativeUrl"))
            {

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

            }

        }

        private void UpdateWeb(AveWebInfo webInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.UpdateWeb"))
            {

                mSPWeb.Title = webInfo.Title;
                mSPWeb.Description = webInfo.Description;
                mSPWeb.Update();

            }

        }

        private void AddToTopNavigationBar(string title, string url)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.AddToTopNavigationBar"))
            {

                AveNavigationNodeCreationInformation navigationNode = new AveNavigationNodeCreationInformation();
                navigationNode.Url = mSPWeb.ServerRelativeUrl;
                navigationNode.Title = title;
                navigationNode.AsLastNode = true;
                navigationNode.IsExternal = false;
                mParentAveWeb.Navigation.TopNavigationBar.Add(navigationNode);

            }

        }

        private void InitializeMembers()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.InitializeMembers"))
            {

                mId = mSPWeb.ID;
                mScope = mSPWeb.ServerRelativeUrl.Substring(1);

            }

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

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.UpdateDocumentSetCT"))
            {

                if (mAveSite.MappingManager.WebMappingManager.DocumentSetCTCache.Count > 0)
                {
                    IAveContentTypeCollection CTs = mSPWeb.ContentTypes;
                    foreach (AveContentTypeInfo ctInfo in mAveSite.MappingManager.WebMappingManager.DocumentSetCTCache)
                    {
                        try
                        {
                            IAveContentType ct = CTs[ctInfo.Name];
                            //Need to change restore option
                            AveSPDocumentSet ctDocumentSet = new AveSPDocumentSet(ctInfo, ct, this.SPWeb, mAveSite.MappingManager, new AveRestoreOption().mAveContentTypeRestoreOption.WEB_CONTENTTYPE_UPDATECHILD);
                            ctDocumentSet.Update();
                        }
                        catch (Exception e)
                        {
                            log.Warn("UpdateDocumentSetCT Error.Content type name:" + ctInfo.Name, e);
                        }
                    }
                }

                mAveSite.MappingManager.WebMappingManager.DocumentSetCTCache.Clear();

            }

        }

        public void RestoreWebProperty(AveWebSettingInfo webSettingInfo)
        {
            RestoreWebProperty(webSettingInfo, true);
        }

        public void RestoreWebProperty(AveWebSettingInfo webSettingInfo, bool isRestoreWebRegionalSettings)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WebProperty"))
            {

                AssembleD5DataToD6(webSettingInfo);
                mWebSettingInfo = webSettingInfo;
                base.IsSettingRestored = true;
                try
                {
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
                        inheritAlertCss = webPageInfo.Inheriting;
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
                    else if (mSPWeb.CustomMasterUrl != null && mSPWeb.CustomMasterUrl.Contains("MasterUrl=\""))
                    {
                        var masterUrl = mSPWeb.CustomMasterUrl.Substring(mSPWeb.CustomMasterUrl.IndexOf("MasterUrl=\"", StringComparison.Ordinal) + "MasterUrl=\"".Length);
                        masterUrl = masterUrl.Substring(0, masterUrl.IndexOf('"'));
                        var masterFile = mSPWeb.GetFile(masterUrl);
                        if (masterFile.Exists)
                        {
                            mSPWeb.CustomMasterUrl = masterFile.ServerRelativeUrl;
                        }
                    }

                    if (webSettingInfo.HideSiteContentsLink != null && webSettingInfo.HideSiteContentsLink.IsAvailable)
                    {
                        mSPWeb.HideSiteContentsLink = webSettingInfo.HideSiteContentsLink.Value;
                    }

                    if (!this.ParentSite.MappingManager.SiteMappingManager.WebMastPageMapping.ContainsKey(mSPWeb.ID))
                    {
                        this.ParentSite.MappingManager.SiteMappingManager.WebMastPageMapping.Add(mSPWeb.ID, webPageInfo);
                    }
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
                    if (mWebSettingInfo.ExcludeFromOfflineClient != null && mWebSettingInfo.ExcludeFromOfflineClient.IsAvailable)
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
                    if (mWebSettingInfo.QuickLaunchEnabled != null && mWebSettingInfo.QuickLaunchEnabled.IsAvailable)
                    {
                        mSPWeb.QuickLaunchEnabled = mWebSettingInfo.QuickLaunchEnabled.Value;
                    }
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

                    if (mWebSettingInfo.ThemedCssFolderUrl != null && mWebSettingInfo.ThemedCssFolderUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedCssFolderUrl.Value))
                    {
                        this.ThemedCssFolderUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedCssFolderUrl.Value, mAveSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                        if (!mSPWeb.IsRootWeb)
                        {
                            var rootWeb = mSPWeb.Site.RootWeb;
                            if (!rootWeb.GetFile(ThemedCssFolderUrl).Exists && !rootWeb.GetFolder(ThemedCssFolderUrl).Exists)
                            {
                                this.ThemedCssFolderUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.ThemedCssFolderUrl.Value, mAveSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                            }
                        }
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
                    if (mWebSettingInfo.AlternateCSSUrl != null && mWebSettingInfo.AlternateCSSUrl.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.AlternateCSSUrl.Value))
                    {
                        this.AlternateCSSUrl = AveReplaceProcessor.UrlReplace(mWebSettingInfo.AlternateCSSUrl.Value, mAveSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                    }

                    if (mWebSettingInfo.ASPXPageIndexMode != null && mWebSettingInfo.ASPXPageIndexMode.IsAvailable)
                    {
                        mSPWeb.ASPXPageIndexMode = (AveWebASPXPageIndexMode)mWebSettingInfo.ASPXPageIndexMode.Value;
                        //if (mSPWeb.ASPXPageIndexMode.Equals(AveWebASPXPageIndexMode.Automatic))
                        //{
                        //    mSPWeb.AllowAutomaticASPXPageIndexing = true;
                        //}
                    }
                    //经过试验，发现AllowAutomaticASPXPageIndexing属性与ASPXPageIndexMode没有必然关系
                    if (mWebSettingInfo.AllowAutomaticASPXPageIndexing != null && mWebSettingInfo.AllowAutomaticASPXPageIndexing.IsAvailable)
                    {
                        mSPWeb.AllowAutomaticASPXPageIndexing = mWebSettingInfo.AllowAutomaticASPXPageIndexing.Value;
                    }
                    /* update MetaInfo */
                    if (mWebSettingInfo.MetaInfo != null && mWebSettingInfo.MetaInfo.IsAvailable && mWebSettingInfo.MetaInfo.Value != null)
                    {
                        RestoreMetaInfo(Encoding.UTF8.GetString(mWebSettingInfo.MetaInfo.Value));
                    }
                    if (mWebSettingInfo.NavigationWebAndPage != null && mWebSettingInfo.NavigationWebAndPage.IsAvailable)
                    {
                        getWebsAndPages(mWebSettingInfo.NavigationWebAndPage.Value);
                    }
                    /* update region setting */
                    if (mSPWeb.RegionalSettings != null && isRestoreWebRegionalSettings)
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
                        //目的端已存在的情况,不可以覆盖该属性，该属性控制目的端的语言，如果要覆盖，请和Wrapper Team联系下，谢谢。
                        if (IsNewCreated && mWebSettingInfo.Locale != null && mWebSettingInfo.Locale.IsAvailable)
                        {
                            mSPWeb.RegionalSettings.LocaleId = (uint)mWebSettingInfo.Locale.Value;
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

                    //还原Web.SupportedUICulture会还原该属性
                    if (mWebSettingInfo.IsMultilingual != null && mWebSettingInfo.IsMultilingual.IsAvailable)
                    {
                        mSPWeb.IsMultilingual = mWebSettingInfo.IsMultilingual.Value;
                    }
                    if (mWebSettingInfo.SupportedUICultures != null && mWebSettingInfo.SupportedUICultures.IsAvailable)
                    {
                        //Restore alternateUICulture
                        IAveWebTemplateCollection webTemplates = mSPWeb.Site.GetWebTemplates(mSPWeb.Language);
                        IAveWebTemplate template = webTemplates[mSPWeb.WebTemplate];
                        if (template.SupportsMultilingualUI)
                        {
                            foreach (int lcid in mWebSettingInfo.SupportedUICultures.Value)
                            {
                                CultureInfo culture = new CultureInfo(lcid);
                                if (this.ParentSite.AveSite.IsOnlineSite || CheckLanguageIsInstalled(culture.LCID))
                                {
                                    if (!mSPWeb.SupportedUICultures.Contains(culture))
                                    {
                                        mSPWeb.AddSupportedUICulture(culture);
                                    }
                                }
                                else
                                {
                                    log.Warn("The language package: " + culture.NativeName + "dose not installed on this farm.");
                                }
                            }
                        }
                        else
                        {
                            log.Warn("The web template not support multilingual.");
                        }
                    }
                    if (mWebSettingInfo.OverwriteTranslationsOnChange != null && mWebSettingInfo.OverwriteTranslationsOnChange.IsAvailable)
                    {
                        mSPWeb.OverwriteTranslationsOnChange = mWebSettingInfo.OverwriteTranslationsOnChange.Value;
                    }
                    //add by adrian
                    if (mWebSettingInfo.ThemedTitle != null && mWebSettingInfo.ThemedTitle.IsAvailable && !string.IsNullOrEmpty(mWebSettingInfo.ThemedTitle.Value))
                    {
                        ThemeTitle = mWebSettingInfo.ThemedTitle.Value;
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
                    RestoreAuthor();
                    //更新以上一些属性值会影响到Title的赋值，放到此处更新
                    if (mWebSettingInfo.Title != null && mWebSettingInfo.Title.IsAvailable)
                    {
                        mSPWeb.Title = mWebSettingInfo.Title.Value;
                    }
                    SetTitleAndDescriptionResource(mSPWeb, mWebSettingInfo);
                    mSPWeb.Update();

                    //add by adrian
                    if (webSettingInfo.ThemedColorUrl != null && webSettingInfo.ThemedColorUrl.IsAvailable && !string.IsNullOrEmpty(webSettingInfo.ThemedColorUrl.Value))
                    {
                        /*
                         * ADO-99846,因为Top Site Collection的Top Level Web的ThemedColorUrl.Value = “/_catalogs/theme/...”
                         * 现在更改为>=0
                         */
                        if (webSettingInfo.ThemedColorUrl.Value.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) >= 0)
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
                        if (webSettingInfo.ThemedFontUrl.Value.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > 0)
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
                        if (webSettingInfo.ThemedImageUrl.Value.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) >= 0)
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
                    if (webSettingInfo.Title != null)
                    {
                        report.AddDetail(new AveWrapperReportDto(webSettingInfo.Title.Value, webSettingInfo.Title.Value
                            , AveReportObjectType.WebProperty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebSetting, ex.Message));
                    }
                    else
                    {
                        report.AddDetail(new AveWrapperReportDto(mSPWeb.Title, mSPWeb.Title
                            , AveReportObjectType.WebProperty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebSetting, ex.Message));
                    }
                }
                catch (Exception e)
                {
                    if (webSettingInfo.Title != null)
                    {
                        report.AddDetail(new AveWrapperReportDto(webSettingInfo.Title.Value, webSettingInfo.Title.Value
                        , AveReportObjectType.WebProperty, AveStatus.Skipped, AveReportResource.Wrapper_Report_UpdateWebPropertyError, mSPWeb.ID, mSPWeb.Url, e.Message));
                    }
                    else
                    {
                        report.AddDetail(new AveWrapperReportDto(mSPWeb.Title, mSPWeb.Title
                        , AveReportObjectType.WebProperty, AveStatus.Skipped, AveReportResource.Wrapper_Report_UpdateWebPropertyError, mSPWeb.ID, mSPWeb.Url, e.Message));
                    }
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while updating web property. WebId:{0}, WebUrl:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, e));
                    //mLog.Warn(e, "An error occurred while updating web property. WebId:{0}, WebUrl:{1}", mSPWeb.ID, mSPWeb.Url);
                }

            }

        }

        private void SetTitleAndDescriptionResource(IAveWeb web, AveWebSettingInfo settingInfo)
        {
            if (settingInfo.TitleResource != null && settingInfo.TitleResource.IsAvailable)
            {
                web.TitleResource.SetUserResource(web, settingInfo.TitleResource.Value);
            }
            if (settingInfo.DescriptionResource != null && settingInfo.DescriptionResource.IsAvailable)
            {
                web.DescriptionResource.SetUserResource(web, settingInfo.DescriptionResource.Value);
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
        private bool CheckLanguageIsInstalled(int lcid)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.CheckLanguageIsInstalled"))
            {

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

            }

        }

        private void getWebsAndPages(Dictionary<string, Dictionary<string, Dictionary<Guid, string>>> navigationWebAndPage)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.getWebsAndPages"))
            {

                Dictionary<string, string> tempAllSubWebsAndPages = new Dictionary<string, string>();
                if (!mAveSite.MappingManager.SiteMappingManager.AllSubWebsAndPagesMapping.ContainsKey(mSPWeb.ID))
                {
                    mAveSite.MappingManager.SiteMappingManager.AllSubWebsAndPagesMapping.Add(mSPWeb.ID, tempAllSubWebsAndPages);
                }
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

            }

        }

        // TODO:Restore other meta info that need restore
        //NOTE:现在全部都只是rootweb上的metainfo还原，后面还会有很多subsite的metainfo还原。
        //只需找到对应的属性，并在if中去掉 mSPWeb.IsRootWeb的条件即可
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPRSAccessibleTablix is a key")]
        private void RestoreMetaInfo(string metaInfoString)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreMetaInfo"))
            {

                try
                {
                    if (mAveSite.SPSite.IsOnlineSite && mSPWeb.Site.RootWeb.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn("Can not update web properties for OneDrive site. ServerRelativeUrl: {0}", this.ServerRelativeUrl);
                        return;
                    }
                    var restoredProperties = new List<string>();
                    if (String.IsNullOrEmpty(metaInfoString) || mSPWeb.AllProperties == null)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("MetaInfoString is Empty."));
                        //mLog.Error("metaInfoString is Empty");
                        return;
                    }
                    MetaInfoDictionary = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);
                    MetaInfoDictionaryWithType = this.GetMetaInfoWithType(metaInfoString);
                    if (MetaInfoDictionary == null)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("After function GetWebMetaInfoDictionary,the metaInfoDictionary is null. metaInfoString:{0}", metaInfoString));
                        //mLog.Error("After function GetWebMetaInfoDictionary,the metainfodictionary is null " + metaInfoString);
                    }

                    #region for 07 - 10 migration. 转换07和10的属性
                    var sp07Property = new string[] { "__IncludeSubSitesInNavigation", "__IncludePagesInNavigation" };
                    restoredProperties.AddRange(sp07Property);
                    Convert07NavigationIncludeTypeTo10UpperFormat(this.MetaInfoDictionary);
                    #endregion

                    #region add for 07, 10 --> 13, 16 migration
                    if (mSPWeb.Site.CompatibilityLevel == 15)
                    {
                        if (!MetaInfoDictionary.ContainsKey("_webnavigationsettings"))
                        {
                            if (MetaInfoDictionary.ContainsKey("__InheritCurrentNavigation"))// migration job
                            {
                                bool inheritCurrentNavigation;
                                Boolean.TryParse(MetaInfoDictionary["__InheritCurrentNavigation"], out inheritCurrentNavigation);
                                bool inheritGlobalNavigation = mWebSettingInfo.UserSharedNav != null && mWebSettingInfo.UserSharedNav.IsAvailable && mWebSettingInfo.UserSharedNav.Value;

                                if (inheritCurrentNavigation && inheritGlobalNavigation)
                                {
                                    MetaInfoDictionary["_webnavigationsettings"] = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><WebNavigationSettings Version=\"1.1\"><SiteMapProviderSettings><SwitchableSiteMapProviderSettings Name=\"CurrentNavigationSwitchableProvider\" UseParentSiteMap=\"True\" /><TaxonomySiteMapProviderSettings Name=\"CurrentNavigationTaxonomyProvider\" UseParentSiteMap=\"True\" /><SwitchableSiteMapProviderSettings Name=\"GlobalNavigationSwitchableProvider\" UseParentSiteMap=\"True\" /><TaxonomySiteMapProviderSettings Name=\"GlobalNavigationTaxonomyProvider\" UseParentSiteMap=\"True\" /></SiteMapProviderSettings><NewPageSettings AddNewPagesToNavigation=\"True\" CreateFriendlyUrlsForNewPages=\"True\" /></WebNavigationSettings>";
                                }
                                else if (inheritCurrentNavigation && !inheritGlobalNavigation)
                                {
                                    MetaInfoDictionary["_webnavigationsettings"] = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><WebNavigationSettings Version=\"1.1\"><SiteMapProviderSettings><SwitchableSiteMapProviderSettings Name=\"CurrentNavigationSwitchableProvider\" UseParentSiteMap=\"True\" /><TaxonomySiteMapProviderSettings Name=\"CurrentNavigationTaxonomyProvider\" UseParentSiteMap=\"True\" /><SwitchableSiteMapProviderSettings Name=\"GlobalNavigationSwitchableProvider\" TargetProviderName=\"GlobalNavigation\" /><TaxonomySiteMapProviderSettings Name=\"GlobalNavigationTaxonomyProvider\" Disabled=\"True\" /></SiteMapProviderSettings><NewPageSettings AddNewPagesToNavigation=\"True\" CreateFriendlyUrlsForNewPages=\"True\" /></WebNavigationSettings>";
                                }
                                else if (!inheritCurrentNavigation && inheritGlobalNavigation)
                                {
                                    MetaInfoDictionary["_webnavigationsettings"] = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><WebNavigationSettings Version=\"1.1\"><SiteMapProviderSettings><SwitchableSiteMapProviderSettings Name=\"CurrentNavigationSwitchableProvider\" TargetProviderName=\"CurrentNavigation\" /><TaxonomySiteMapProviderSettings Name=\"CurrentNavigationTaxonomyProvider\" Disabled=\"True\" /><SwitchableSiteMapProviderSettings Name=\"GlobalNavigationSwitchableProvider\" UseParentSiteMap=\"True\" /><TaxonomySiteMapProviderSettings Name=\"GlobalNavigationTaxonomyProvider\" UseParentSiteMap=\"True\" /></SiteMapProviderSettings><NewPageSettings AddNewPagesToNavigation=\"True\" CreateFriendlyUrlsForNewPages=\"True\" /></WebNavigationSettings>";
                                }
                                else
                                {
                                    MetaInfoDictionary["_webnavigationsettings"] = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><WebNavigationSettings Version=\"1.1\"><SiteMapProviderSettings><SwitchableSiteMapProviderSettings Name=\"CurrentNavigationSwitchableProvider\" TargetProviderName=\"CurrentNavigation\" /><TaxonomySiteMapProviderSettings Name=\"CurrentNavigationTaxonomyProvider\" Disabled=\"True\" /><SwitchableSiteMapProviderSettings Name=\"GlobalNavigationSwitchableProvider\" TargetProviderName=\"GlobalNavigation\" /><TaxonomySiteMapProviderSettings Name=\"GlobalNavigationTaxonomyProvider\" Disabled=\"True\" /></SiteMapProviderSettings><NewPageSettings AddNewPagesToNavigation=\"True\" CreateFriendlyUrlsForNewPages=\"True\" /></WebNavigationSettings>";
                                }
                            }
                            else
                            {
                                if (mSPWeb.AllProperties.ContainsKey("_webnavigationsettings"))
                                {
                                    if (MetaInfoDictionary.ContainsKey("__GlobalNavigationIncludeTypes") || MetaInfoDictionary.ContainsKey("__CurrentNavigationIncludeTypes"))
                                    {
                                        MetaInfoDictionary["_webnavigationsettings"] = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><WebNavigationSettings Version=\"1.1\"><SiteMapProviderSettings><SwitchableSiteMapProviderSettings Name=\"CurrentNavigationSwitchableProvider\" TargetProviderName=\"CurrentNavigation\" /><TaxonomySiteMapProviderSettings Name=\"CurrentNavigationTaxonomyProvider\" Disabled=\"True\" /><SwitchableSiteMapProviderSettings Name=\"GlobalNavigationSwitchableProvider\" TargetProviderName=\"GlobalNavigation\" /><TaxonomySiteMapProviderSettings Name=\"GlobalNavigationTaxonomyProvider\" Disabled=\"True\" /></SiteMapProviderSettings><NewPageSettings AddNewPagesToNavigation=\"True\" CreateFriendlyUrlsForNewPages=\"True\" /></WebNavigationSettings>";
                                        MetaInfoDictionary["__InheritCurrentNavigation"] = "false";
                                    }
                                    else
                                    {
                                        //是否需要覆盖目的端
                                        //ADO-128101 原端没开启publish feature，目的端开启的情况下覆盖会导致目的端navigation变成继承parent，如果一定要覆盖需要考虑覆盖后的setting value
                                        //MetaInfoDictionary["_webnavigationsettings"] = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><WebNavigationSettings Version=\"1.1\"><SiteMapProviderSettings><SwitchableSiteMapProviderSettings Name=\"CurrentNavigationSwitchableProvider\" UseParentSiteMap=\"True\" /><TaxonomySiteMapProviderSettings Name=\"CurrentNavigationTaxonomyProvider\" UseParentSiteMap=\"True\" /><SwitchableSiteMapProviderSettings Name=\"GlobalNavigationSwitchableProvider\" UseParentSiteMap=\"True\" /><TaxonomySiteMapProviderSettings Name=\"GlobalNavigationTaxonomyProvider\" UseParentSiteMap=\"True\" /></SiteMapProviderSettings><NewPageSettings AddNewPagesToNavigation=\"True\" CreateFriendlyUrlsForNewPages=\"True\" /></WebNavigationSettings>";
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Move this to restore theme in web post action
                    //if (MetaInfoDictionary.ContainsKey("__InheritsThemedCssFolderUrl") && !mSPWeb.IsRootWeb)
                    //{
                    //    mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = MetaInfoDictionary["__InheritsThemedCssFolderUrl"];
                    //    if (mWebSettingInfo.WebTheme != null && mWebSettingInfo.WebTheme.Value != null)
                    //    {
                    //        mWebSettingInfo.WebTheme.Value.InheritsThemedCssFolderUrl = Convert.ToBoolean(MetaInfoDictionary["__InheritsThemedCssFolderUrl"]);
                    //    }
                    //    restoredProperties.Add("__InheritsThemedCssFolderUrl");
                    //}
                    //else if (!mSPWeb.IsRootWeb && mSPWeb.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && bool.TrueString.Equals(mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] as string, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = bool.FalseString;
                    //    if (mWebSettingInfo.WebTheme != null && mWebSettingInfo.WebTheme.Value != null)
                    //    {
                    //        mWebSettingInfo.WebTheme.Value.InheritsThemedCssFolderUrl = false;
                    //    }
                    //    restoredProperties.Add("__InheritsThemedCssFolderUrl");
                    //}
                    //else
                    //{
                    //    MetaInfoDictionary.Remove("__InheritsThemedCssFolderUrl");
                    //}
                    #endregion

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
                                                               "SRCH_ENH_FTR_URL","taxonomyhiddenlist","SRCH_TRAGET_RESULTS_PAGE", "SRCH_SB_SET_SITE", "SRCH_ENH_FTR_URL_SITE", "SRCH_ENH_FTR_URL_WEB", "SRCH_SB_SET_WEB", "discoverycasestatistics"//"EnforceNewListingForSites","SiteDirectoryEntryRequirements, "//this two properties will throw exception while do 07 to 10 migration
                                                                                                       //"vti_associategroups",
                                                             };

                    restoredProperties.AddRange(metaNameNeedRestore);

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
                                else if (mStrKey.Equals("SRCH_ENH_FTR_URL", StringComparison.OrdinalIgnoreCase) || mStrKey.Equals("_auditlogreportstoragelocation", StringComparison.OrdinalIgnoreCase)
                                    || mStrKey.Equals("SRCH_TRAGET_RESULTS_PAGE", StringComparison.OrdinalIgnoreCase) || mStrKey.Equals("SRCH_SB_SET_SITE", StringComparison.OrdinalIgnoreCase)
                                    || mStrKey.Equals("SRCH_ENH_FTR_URL_WEB", StringComparison.OrdinalIgnoreCase) || mStrKey.Equals("SRCH_SB_SET_WEB", StringComparison.OrdinalIgnoreCase)
                                    || mStrKey.Equals("SRCH_ENH_FTR_URL_SITE", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!string.IsNullOrEmpty(MetaInfoDictionary[mStrKey]))
                                    {
                                        mAveSite.MappingManager.SiteMappingManager.AddUrlNeedPostActionMapping(this.SPWeb.ID, mStrKey, MetaInfoDictionary[mStrKey].ToString());
                                        continue;
                                    }
                                }
                                mSPWeb.AllProperties[mStrKey] = MetaInfoDictionary[mStrKey].Replace(@"\\", @"\").Replace("\\r\\n", "\r\n");
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while update web metainfo. property name:{0}\n error message:{1}", mStrKey, e));
                        }
                    }


                    #endregion

                    #region Search
                    var specicalPropertyForSearch = new string[] { "NoCrawl", "showurlstructure", "docid_settings_ui", "SRCH_SITE_DROPDOWN_MODE" };
                    restoredProperties.AddRange(specicalPropertyForSearch);

                    //restore Web >Search Visibility > Indexing Site Content
                    if (MetaInfoDictionary.ContainsKey("NoCrawl"))
                    {
                        mSPWeb.NoCrawl = bool.Parse(MetaInfoDictionary["NoCrawl"]);
                    }
                    if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("showurlstructure"))
                    {
                        mSPWeb.AllProperties["showurlstructure"] = MetaInfoDictionary["showurlstructure"];
                    }
                    //settings->Search Setting. 
                    if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("SRCH_SITE_DROPDOWN_MODE"))
                    {
                        mSPWeb.AllProperties["SRCH_SITE_DROPDOWN_MODE"] = MetaInfoDictionary["SRCH_SITE_DROPDOWN_MODE"];
                    }

                    if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("SRCH_SB_SET_SITE"))
                    {
                        mSPWeb.AllProperties["SRCH_SB_SET_SITE"] = MetaInfoDictionary["SRCH_SB_SET_SITE"];
                    }

                    if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("SRCH_ENH_FTR_URL_SITE"))
                    {
                        mSPWeb.AllProperties["SRCH_ENH_FTR_URL_SITE"] = MetaInfoDictionary["SRCH_ENH_FTR_URL_SITE"];
                    }

                    if (MetaInfoDictionary.ContainsKey("SRCH_ENH_FTR_URL_WEB"))
                    {
                        mSPWeb.AllProperties["SRCH_ENH_FTR_URL_WEB"] = MetaInfoDictionary["SRCH_ENH_FTR_URL_WEB"];
                    }

                    if (MetaInfoDictionary.ContainsKey("SRCH_SB_SET_WEB"))
                    {
                        mSPWeb.AllProperties["SRCH_SB_SET_WEB"] = MetaInfoDictionary["SRCH_SB_SET_WEB"];
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
                    else
                    {
                        if (mSPWeb.AllProperties.ContainsKey("__AuthenticatedPageCacheProfileUrl"))
                        {
                            mSPWeb.AllProperties["__AuthenticatedPageCacheProfileUrl"] = string.Empty;
                            if (mSPWeb.Properties != null)
                            {
                                mSPWeb.Properties["__AuthenticatedPageCacheProfileUrl"] = string.Empty;
                            }
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
                    else
                    {
                        if (mSPWeb.AllProperties.ContainsKey("__AnonymousPageCacheProfileUrl"))
                        {
                            mSPWeb.AllProperties["__AnonymousPageCacheProfileUrl"] = string.Empty;
                            if (mSPWeb.Properties != null)
                            {
                                mSPWeb.Properties["__AnonymousPageCacheProfileUrl"] = string.Empty;
                            }
                        }
                    }
                    #endregion

                    #region audit log reports
                    var specicalPropertyForAuditlog = new string[] { "_reportinggallerymetadataid", "_reportinggallerytemplateid" };
                    restoredProperties.AddRange(specicalPropertyForAuditlog);
                    #endregion

                    #region Page Layout and Site Template Settings

                    var specicalPropertyForLayoutAndTemplate = new string[] { "__InheritWebTemplates", "__WebTemplates", "__PageLayouts", "__DefaultPageLayout" };
                    restoredProperties.AddRange(specicalPropertyForLayoutAndTemplate);
                    RestoreWebPageLayoutAndTemplate(MetaInfoDictionary);

                    var allowSpacesInNewPageNameSetting = new string[] { "__AllowSpacesInNewPageName" };
                    restoredProperties.AddRange(allowSpacesInNewPageNameSetting);
                    RestoreAllPropertiesOfWebMetaInfo(restoredProperties, allowSpacesInNewPageNameSetting);

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
                    if (WrapperConfiguration.OverwriteDocIdPrefix && mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey("docid_settings_ui"))
                    {
                        mSPWeb.AllProperties["docid_settings_ui"] = MetaInfoDictionary["docid_settings_ui"].Replace("\\r\\n", "\r\n");
                        mSPWeb.Properties["docid_settings_ui"] = MetaInfoDictionary["docid_settings_ui"].Replace("\\r\\n", "\r\n");
                    }
                    foreach (var property in specicalPropertyForDocumentId)
                    {
                        if (mSPWeb.IsRootWeb && MetaInfoDictionary.ContainsKey(property))
                        {
                            //For 365-local
                            if (property.Equals("docid_customProvider_assembly", StringComparison.OrdinalIgnoreCase) && !this.ParentSite.SPSite.IsOnlineSite &&
                                Regex.IsMatch(MetaInfoDictionary[property], @"Microsoft.Office.DocumentManagement, Version=([1-9][0-9].0.0.0), Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
                            {
                                MetaInfoDictionary[property] = String.Format("Microsoft.Office.DocumentManagement, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", this.ParentSite.SPSite.SPVersion.Substring(0, 2));
                            }
                            if (WrapperConfiguration.OverwriteDocIdPrefix || !property.Equals("docid_msft_hier_siteprefix", StringComparison.OrdinalIgnoreCase))
                            {
                                mSPWeb.AllProperties[property] = MetaInfoDictionary[property];
                                mSPWeb.Properties[property] = MetaInfoDictionary[property];
                            }
                        }
                    }
                    restoredProperties.AddRange(specicalPropertyForDocumentIDService);
                    #endregion

                    #region Setting in SiteNavigationSettings.aspx
                    var siteNavigationSettings = new string[] { "__AllowSpacesInNewPageName", "EnableNavigation", "EnableSecurityTrimming", "EnableAudienceTargeting" };
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

                    if (MetaInfoDictionary.ContainsKey("_webnavigationsettings"))
                    {
                        bool isRestoreNavigationXml = false;
                        string navigationXml = MetaInfoDictionary["_webnavigationsettings"];
                        navigationXml = navigationXml.Replace("\\r\\n", " ");
                        if (!CheckInheritedNavigation(navigationXml))
                        {
                            if (CheckTaxonomyProperty(navigationXml))
                            {
                                if (WrapperRuntime.CurrentContext.RestoreManagedMetadataNavigation)
                                {
                                    navigationXml = ProcessWebNavigationSetting(navigationXml);
                                    isRestoreNavigationXml = true;
                                }
                                else
                                {
                                    isRestoreNavigationXml = false;
                                }
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
                        }
                        restoredProperties.Add("_webnavigationsettings");
                    }

                    #region PublishSiteNavigation Setting
                    var navigaionSetting = new[] { "__CurrentDynamicChildLimit","__CurrentNavigationIncludeTypes",
                        "__DisplayShowHideRibbonActionId","__GlobalDynamicChildLimit","__GlobalNavigationIncludeTypes",
                        "__InheritCurrentNavigation","__NavigationOrderingMethod","__NavigationShowSiblings",
                        "__NavigationSortAscending","__NavigationAutomaticSortingMethod"};

                    RestoreAllPropertiesOfWebMetaInfo(restoredProperties, navigaionSetting);


                    #endregion

                    #region Community Site Properties
                    var communitySettings = new string[] { "Community_MembersCount", "Community_RepliesCount", "Community_TopicsCount", "vti_CommunityEstablishedDate", "vti_CommunityEnableAutoApproval", "vti_CommunityEnableReportAbuse" };
                    restoredProperties.AddRange(communitySettings);
                    RestoreCommunitySettings(communitySettings);
                    #endregion

                    #region Blog Site 的 Post Layout
                    var blogSettings = new string[] { "ms-blogs-skinid" };
                    restoredProperties.AddRange(blogSettings);
                    RestoreNormalWebProperties(blogSettings);
                    #endregion

                    #region site policy
                    var sitePolicy = new string[] { "dlc_sitehaspolicy" };
                    restoredProperties.AddRange(sitePolicy);
                    RestoreNormalWebProperties(sitePolicy);
                    #endregion

                    #region  13 Site Closure and Deletion
                    var siteClosureAndDeletion = new string[] { "PolicyName", "PolicyCTId", "SiteClosed" };
                    var dataTimeWebProperties = new string[] { "CloseDate", "DeleteDate" };
                    RestoreNormalWebProperties(siteClosureAndDeletion);
                    RestoreDateTimeWebProperties(dataTimeWebProperties);
                    #endregion

                    #region Manage catalog connections
                    restoredProperties.Add("_catalogsourcesconfig");
                    RestoreManageCatalogConnections();
                    #endregion

                    #region 16 community site home page "What's happening" webpart
                    string[] dashboardWebPartRequireProperty = new string[] { "Community_LastUpdated", "Category_URL", "Category_Name" };
                    RestoreAllPropertiesOfWebMetaInfo(restoredProperties, dashboardWebPartRequireProperty);
                    #endregion
                    RestoreVariationSettings();
                    mSPWeb.Update();
                    mSPWeb.Properties.Update();


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
                        var specicalPropertyForHold = new string[] { "OriginalNotebookUrl", "_dlc_repositoryusersgroup", "holdlistid", "holdreportslistid" };
                        restoredProperties.AddRange(specicalPropertyForHold);
                        #endregion

                        //ADO-188907 NoteBook QuickLaunch不需要替换，要按照目的端的URL来设置
                        if (MetaInfoDictionary.ContainsKey("OriginalNotebookUrl"))
                        {
                            Guid sourceItemId;
                            var notebookUrl = MetaInfoDictionary["OriginalNotebookUrl"];
                            if (AveUrlUtility.IsDurableLink(notebookUrl, out sourceItemId) && mSPWeb.Properties != null
                                && mSPWeb.Properties.ContainsKey("OriginalNotebookUrl"))
                            {
                                this.ParentSite.MappingManager.SiteMappingManager.AddDurableLinkMapping(sourceItemId, mSPWeb.Properties["OriginalNotebookUrl"]);
                            }
                        }

                        if (WrapperConfiguration.RestoredAllWebProperties)
                        {
                            if (!string.IsNullOrEmpty(WrapperConfiguration.SpecialWebPropertyNames))
                            {
                                string[] needForceSkippedProperities = WrapperConfiguration.SpecialWebPropertyNames.Split(new char[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                restoredProperties.AddRange(needForceSkippedProperities);
                            }
                            restoredProperties.Sort(StringComparer.Ordinal);
                            foreach (string pro in MetaInfoDictionaryWithType.Keys)
                            {
                                if (!pro.Equals("_VarLabelsListId", StringComparison.OrdinalIgnoreCase)
                                    && !pro.Equals("vti_sitemasterid", StringComparison.OrdinalIgnoreCase)//vti_sitemasterid属性标记site master站点的ID。不要覆盖目的端。
                                    && !pro.Equals("nintexformslibraryid", StringComparison.OrdinalIgnoreCase)//nintexformslibraryid标记当前web下的nintex form library, 不要覆盖目的端。
                                    && restoredProperties.BinarySearch(pro, StringComparer.Ordinal) < 0)
                                {
                                    if (pro.Equals(pro.ToLower(CultureInfo.InvariantCulture)) && MetaInfoDictionaryWithType[pro] is String)
                                    {
                                        mSPWeb.Properties[pro] = MetaInfoDictionary[pro];
                                    }
                                    else
                                    {
                                        mSPWeb.AllProperties[pro] = MetaInfoDictionaryWithType[pro];
                                    }
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(WrapperConfiguration.SpecialWebPropertyNames))
                        {
                            string[] forceRestoreProperities = WrapperConfiguration.SpecialWebPropertyNames.Split(new char[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var pro in forceRestoreProperities)
                            {
                                if (MetaInfoDictionary.ContainsKey(pro) && MetaInfoDictionaryWithType.ContainsKey(pro))
                                {
                                    if (pro.Equals(pro.ToLower(CultureInfo.InvariantCulture)) && MetaInfoDictionaryWithType[pro] is String)
                                    {
                                        mSPWeb.Properties[pro] = MetaInfoDictionary[pro];
                                    }
                                    else
                                    {
                                        mSPWeb.AllProperties[pro] = MetaInfoDictionaryWithType[pro];
                                    }
                                }
                            }
                        }
                        mSPWeb.Update();
                        mSPWeb.Properties.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("Restore metaInfo error. metaInfo:{0}, web id:{1}\n error message:{2}", metaInfoString, mSPWeb.ID, ex));
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RestoreWebMetaInfo", "RestoreWebMetaInfo", AveReportObjectType.WebMetaInfo, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreWebMetaInfo + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("Restore metaInfo error. metaInfo:{0}, web id:{1}\n error message:{2}", metaInfoString, mSPWeb.ID, e));
                    //mLog.Error(e, "Restore meta info Error{0},Dest webID:{1}", metaInfoString,mSPWeb.ID);
                }

            }

        }

        private void Convert07NavigationIncludeTypeTo10UpperFormat(Dictionary<string, string> metaInfo)
        {
            if (metaInfo.ContainsKey("__GlobalNavigationIncludeTypes") || metaInfo.ContainsKey("__CurrentNavigationIncludeTypes")) return;
            bool? includeSubSiteInNavigation = metaInfo.TryGetValue("__IncludeSubSitesInNavigation").ToNullableValueType<bool>();
            bool? includePagesInNavigation = metaInfo.TryGetValue("__IncludePagesInNavigation").ToNullableValueType<bool>();
            bool publishWeb = mSPWeb.Features[AveSP2010FeatureDefinitions.PublishingWeb] != null && mSPWeb.IsPublish;
            int? includeType = ConvertToNavigationIncludeType(includeSubSiteInNavigation, includePagesInNavigation, publishWeb);
            if (includeType.HasValue)
            {
                metaInfo["__GlobalNavigationIncludeTypes"] = includeType.Value.ToString();
                metaInfo["__CurrentNavigationIncludeTypes"] = includeType.Value.ToString();
            }
            //else
            //{
            //    metaInfo.Remove("__GlobalNavigationIncludeTypes");
            //    metaInfo.Remove("__CurrentNavigationIncludeTypes");
            //}
        }

        //includeSubSiteInNavigation includePagesInNavigation	return
        //NULL                       NULL                       NULL
        //NULL                       FALSE	                    0
        //FALSE                      NULL                       NULL
        //FALSE                      FALSE                  	0
        //TRUE                       FALSE                  	1
        //TRUE                       NULL                       publishWeb?3:1
        //FALSE                      TRUE                   	2
        //NULL                       TRUE	                    2
        //TRUE                       TRUE	                    3

        //
        internal static int? ConvertToNavigationIncludeType(bool? includeSubSiteInNavigation, bool? includePagesInNavigation, bool publishWeb)
        {
            if (!includePagesInNavigation.HasValue)
            {
                if (includeSubSiteInNavigation.HasValue && includeSubSiteInNavigation.Value)
                {
                    return (int)(NavigationIncludeType.IncludeSubSites | (publishWeb ? NavigationIncludeType.IncludePages : NavigationIncludeType.None));
                }
                return null;
            }
            var includeSubSite = (includeSubSiteInNavigation ?? false) ? NavigationIncludeType.IncludeSubSites : NavigationIncludeType.None;
            var includePage = includePagesInNavigation.Value ? NavigationIncludeType.IncludePages : NavigationIncludeType.None;
            return (int)(includePage | includeSubSite);
        }
        [Flags]
        enum NavigationIncludeType
        {
            None = 0,
            IncludeSubSites = 1,
            IncludePages = 2,
        }
        private void RestoreVariationSettings()
        {
            const string In_Source_Hierarchy = "__InSourceHierarchy";
            const string Variation_Group_Id = "Variation Group Id";
            if (this.MetaInfoDictionary.ContainsKey(In_Source_Hierarchy))
            {
                //__InSourceHierarchy对应Variation Label的IsSource，如果目的端存在这个属性，则不覆盖并打Log报错。
                if (!this.mSPWeb.AllProperties.ContainsKey(In_Source_Hierarchy))
                {
                    mSPWeb.AllProperties[In_Source_Hierarchy] = this.MetaInfoDictionary[In_Source_Hierarchy];
                }
                else
                {
                    log.Log(AveLogLevel.WARN, "Did not update __InSourceHierarchy since it is not empty in destination web. Source: {0}, destination: {1}", this.MetaInfoDictionary[In_Source_Hierarchy], mSPWeb.AllProperties[In_Source_Hierarchy]);
                }
            }
            if (this.MetaInfoDictionary.ContainsKey(Variation_Group_Id))
            {
                //Variation Group Id 对应Relationships List中Variation Site的Group Id，目前keep group id
                mSPWeb.AllProperties[Variation_Group_Id] = this.MetaInfoDictionary[Variation_Group_Id];
            }
        }

        private bool CheckInheritedNavigation(string navigationXml)
        {
            try
            {
                if (this.SPWeb.IsRootWeb && navigationXml.Contains("UseParentSiteMap=\"True\""))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                log.Warn("Check inherited navigation failed ", e.ToString());
            }
            return false;
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
            foreach (XmlElement node in nodeList.OfType<XmlElement>())
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

        //cm中，如果是subsite升级到site collection，当源端的navigation有选择是继承类型的话，则不应该还原该属性，否则rootweb里的navigation也变成继承的了，当root web没有parent web所以不能是继承的
        private bool HasInheritNavigationNode(string sourceNavSetting)
        {
            if (mSPWeb.IsRootWeb)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sourceNavSetting);
                XmlNodeList switchableSiteMapNodes = xmlDoc.SelectNodes("WebNavigationSettings/SiteMapProviderSettings/SwitchableSiteMapProviderSettings");
                foreach (XmlElement switchableSettingNode in switchableSiteMapNodes.OfType<XmlElement>())
                {
                    if (switchableSettingNode.HasAttribute("UseParentSiteMap") && Convert.ToBoolean(switchableSettingNode.GetAttribute("UseParentSiteMap")))
                    {
                        return true;
                    }
                }
            }
            return false;
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
                        //destTermSet.SetCustomProperty("_Sys_Nav_AttachedWebHistory", "");
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "~sitecollection is a part of url")]
        private void ProcessTermLocalCustomProperties(IAveTerm term)
        {
            List<string> systemNavgationPropertiesList = new List<string> { "_Sys_Nav_SimpleLinkUrl", "_Sys_Nav_CatalogTargetUrl", "_Sys_Nav_CatalogTargetUrlForChildTerms", "_Sys_Nav_TargetUrl", "_Sys_Nav_TargetUrlForChildTerms", "_Sys_Nav_AssociatedFolderUrl", "_Sys_Nav_CategoryImageUrl" };

            foreach (var prop in systemNavgationPropertiesList)
            {
                if (term.LocalCustomProperties.ContainsKey(prop))
                {
                    string url = term.LocalCustomProperties[prop];
                    if (term.ChangedLCPSourceValue.ContainsKey(prop))
                    {
                        url = term.ChangedLCPSourceValue[prop];
                    }
                    bool urlReplaced = false;
                    if (!string.IsNullOrEmpty(url) && url.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                    {
                        url = string.Concat(ParentSite.MappingManager.SiteMappingManager.SourceSiteInfo.ServerRelativeUrl, url.Substring("~sitecollection".Length));
                        urlReplaced = true;
                    }
                    var resultUrl = AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true, true), ParentSite.SourceSiteInfo, mAveSite.SiteUrl);
                    if (urlReplaced)
                    {
                        if (!string.IsNullOrEmpty(url) && resultUrl.StartsWith(ParentSite.SPSite.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            resultUrl = string.Concat("~sitecollection", resultUrl.Substring(ParentSite.SPSite.ServerRelativeUrl.Length));
                        }
                    }

                    term.SetLocalCustomProperty(prop, resultUrl);
                }
            }

            foreach (IAveTerm item in term.Terms)
            {
                ProcessTermLocalCustomProperties(item);
            }
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
                        mSPWeb.AllProperties.Remove(setting);
                    }
                }
                else if (MetaInfoDictionary.ContainsKey(setting))
                {
                    var propertyValue = MetaInfoDictionary[setting];
                    if (string.Equals("Category_URL", setting))
                    {
                        propertyValue = AveReplaceProcessor.UrlReplace(propertyValue, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true, true), ParentSite.SourceSiteInfo, mAveSite.SiteUrl);
                    }
                    mSPWeb.AllProperties[setting] = propertyValue;
                    restoredProperties.Add(setting);
                }
            }
        }

        public bool TemplateAvalible(string templateName)
        {
            try
            {
                if (ParentSite != null && this.ParentSite.AveSite != null && this.ParentSite.AveSite.RootWeb != null)
                {
                    IAveWeb rootWeb = this.ParentSite.AveSite.RootWeb;
                    IAveRegionalSettings regionalSettings = this.ParentSite.ObjectModelFactory.CreateRegionalSettings(rootWeb, false);
                    foreach (IAveLanguage lanuage in regionalSettings.InstalledLanguages)
                    {
                        var templates = this.ParentSite.AveSite.GetWebTemplates((uint)lanuage.LCID);

                        foreach (var template in templates)
                        {
                            if (string.Equals(template.Name, templateName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (ParentSite != null)
                {
                    return ParentSite.TemplateAvalible(templateName);
                }
            }
            catch (Exception e)
            {
                log.Debug(string.Format("Check site template {0} error.Exception:{1}", templateName, e.ToString()));
                if (ParentSite != null)
                    return ParentSite.TemplateAvalible(templateName);
            }
            return false;
        }

        public bool TemplateAvalible(string templateName, out bool isHidden)
        {
            isHidden = false;
            try
            {
                if (ParentSite != null && this.ParentSite.AveSite != null && this.ParentSite.AveSite.RootWeb != null)
                {
                    IAveWeb rootWeb = this.ParentSite.AveSite.RootWeb;
                    IAveRegionalSettings regionalSettings = this.ParentSite.ObjectModelFactory.CreateRegionalSettings(rootWeb, false);
                    foreach (IAveLanguage lanuage in regionalSettings.InstalledLanguages)
                    {
                        var templates = this.ParentSite.AveSite.GetWebTemplates((uint)lanuage.LCID);

                        foreach (var template in templates)
                        {
                            if (string.Equals(template.Name, templateName, StringComparison.OrdinalIgnoreCase))
                            {
                                isHidden = template.IsHidden;
                                return true;
                            }
                        }
                    }
                }
                else if (ParentSite != null)
                {
                    return ParentSite.TemplateAvalible(templateName, out isHidden);
                }
            }
            catch (Exception e)
            {
                log.Debug(string.Format("Check site template {0} error.Exception:{1}", templateName, e.ToString()));
                if (ParentSite != null)
                    return ParentSite.TemplateAvalible(templateName, out isHidden);
            }
            return false;
        }

        /// <summary>
        /// /// 这个函数主要是为了load或者创建基本的Web所需要的，如果需要还原setting，请到restore property函数中。
        /// </summary>
        /// <param name="webInfo"></param>
        public void RestoreWebSelf(AveWebInfo webInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreWebSelf"))
            {
                mWebInfo = webInfo;
                mOldId = webInfo.OldWebId;
                mSrcLanguageId = webInfo.LCID;

                if (mName == AveConstants.ROOT_WEB)
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
                        if (WebInfo.IsAppWeb)
                        {
                            //app web 不应该用普通Web的添加方式
                            throw new AveWrapperAppDataException(AveInternalResourceKey.Wrapper_Exception_Restore_RestoreAddDataFailedForNotRestoredCorrectly);
                        }

                        if (RestoringWeb.IsIncludingRecycleBinData && this.mRestoreOption.CheckRestoreOption(AveRestoreMode.Default) && !IsNewCreated)
                        {
                            if (IsConflictWithRecycle())
                            {
                                RestoringWeb.NeedSkipped = true;
                                ReportMessage = "Not overwrite and conflict with recycle bin";
                                NeedContinue = false;
                                return;
                            }
                        }
                        CreateNewWeb(mWebInfo);
                        mIsNewCreated = true;
                    }
                    else
                    {
                        if (mSPWeb.IsAppWeb && WebInfo.IsAppWeb)
                        {
                            IAveAppInstance appInstance = mSPWeb.ParentWeb.GetAppInstanceById(mSPWeb.AppInstanceId);
                            string AppPrincipalId = appInstance.AppPrincipalId;
                            if (!string.IsNullOrEmpty(AppPrincipalId) && mQueryService != null)
                            {
                                AppAuthorId = mQueryService.GetAppAuthorAndAppEditor(mSPWeb.Site.ID, AppPrincipalId);
                            }
                            else
                            {
                                AppAuthorId = -1;
                            }
                            AppEditorId = AppAuthorId;

                            if (appInstance != null && appInstance.Status != AveAppInstanceStatus.Installed)
                            {
                                throw new AveWrapperAppDataException(AveInternalResourceKey.Wrapper_Exception_Restore_RestoreAddDataFailedForInstallAppFailed, appInstance.Title);
                            }
                            if (mAveSite != null && mAveSite.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Contains(WebInfo.AppInstanceId))
                                throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_SkipRestoreAppData);

                            log.Log(AveLogLevel.DEBUG, "AppWeb AppAuthorId :{0}", AppAuthorId);
                        }
                        else
                        {
                            if (mSPWeb.IsAppWeb ^ WebInfo.IsAppWeb)
                            {
                                //源端或目的端只有一个不是AppWeb
                                throw new AveWrapperAppDataException(AveInternalResourceKey.Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl);
                            }
                        }

                    }
                }
                mAveSite.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(mWebInfo.Url, mSPWeb.Url);
                mAveSite.MappingManager.SiteMappingManager.AddWebUrlMapping(mWebInfo.Name, mSPWeb.ServerRelativeUrl);
                mAveSite.MappingManager.SiteMappingManager.AddWebUrlDestToSourceMapping(mSPWeb.ServerRelativeUrl, mWebInfo.Name);
                mAveSite.MappingManager.SiteMappingManager.AddWebIDMapping(mWebInfo.OldWebId, mSPWeb.ID);
                //ADO-164143  subsite attach 到 同sitecollection 的subsite下 存在URL替换问题
                //if (mWebInfo.parentWebInfo != null)
                //{
                //    AveWebInfo tempWebInfo = mWebInfo.parentWebInfo;
                //    string webRelativeUrl = mSPWeb.ServerRelativeUrl;
                //    while (tempWebInfo != null)
                //    {
                //        if (!mSPWeb.IsRootWeb && webRelativeUrl.IndexOf('/') >= 0 && webRelativeUrl.StartsWith(mAveSite.SPSite.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)
                //           && !webRelativeUrl.Equals(mAveSite.SPSite.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                //        {
                //            webRelativeUrl = webRelativeUrl.Substring(0, webRelativeUrl.LastIndexOf('/'));
                //            mAveSite.MappingManager.SiteMappingManager.AddWebUrlMapping(tempWebInfo.Name, webRelativeUrl);
                //            tempWebInfo = tempWebInfo.parentWebInfo;
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }
                //}
                TransformUICultureToWebCulture();
                InitializeMembers();

                //ADO-58663 当执行这个link操作的时候，在workspace的MeetingSeriesList下回自动多出两个Item，LeafName一个为1_.000一个为1_1.000
                //这时当我们在还原MeetingSeries下的Item的时候，就会因为根据源端的RowId取Item的时候出问题
                //这段代码是因为ADO-17238提上去的，经测试，注释掉这段代码，同样好用
                //if (mSPWeb.WebTemplate != null && mSPWeb.WebTemplate.StartsWith("MPS", StringComparison.OrdinalIgnoreCase))
                //{
                //    LinkToEventItem();
                //}
            }
        }

        private void TransformUICultureToWebCulture()
        {
            try
            {
                if (Thread.CurrentThread.CurrentUICulture != mSPWeb.UICulture)
                {
                    Thread.CurrentThread.CurrentUICulture = mSPWeb.UICulture;
                }
                if (Thread.CurrentThread.CurrentCulture != mSPWeb.UICulture)
                {
                    Thread.CurrentThread.CurrentCulture = mSPWeb.UICulture;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while set CurrentUICulture of current thread. Error: {0}", e.ToString()));
            }
        }

        private void LinkToEventItem()
        {
            try
            {
                if (mAveSite.MappingManager.SiteMappingManager.MeetingWorkSpaceMapping.ContainsKey(mSPWeb.Url))
                {
                    var array = mAveSite.MappingManager.SiteMappingManager.MeetingWorkSpaceMapping[mSPWeb.Url];
                    var objectModel = AveObjectModelFactory.CreateObjectModelFactory("", new AveBPOSAccountInfo(), AveContextKind.Auto);
                    var meeting = objectModel.CreateMeeting();
                    meeting.LinkWithEvent(mSPWeb, (string)array[0], (int)array[1], "WorkspaceLink", "Workspace");
                }
            }
            catch (Exception exception)
            {
                log.Warn("An error occurred while link web to event.Exception {0}", exception);
            }
        }

        private bool IsConflictWithRecycle()
        {
            string webUrl;
            var site = mAveSite.SPSite;
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
                webUrl = string.Format("{0}/{1}", site.ServerRelativeUrl, mName);
            }
            webUrl = webUrl.TrimStart('/');
            if (mAveSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel) //365模式下去load recycle bin下的web类型数据并进行比较来支持冲突处理。
            {
                return site.RecycleBin.Where(recycleWeb => recycleWeb.ItemType == AveRecycleBinItemType.Web)
                                                        .Any(recycleWeb => webUrl.Equals(string.Format("{0}/{1}", recycleWeb.DirName, recycleWeb.LeafName), StringComparison.OrdinalIgnoreCase));
            }
            return mQueryService.IsConflictWithRecycle(site.ID, webUrl);
        }

        public void ClearWebNavigation()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ClearWebNavigation"))
            {
                try
                {
                    ClearNavigation(mSPWeb.Navigation.QuickLaunch);
                    ClearNavigation(mSPWeb.Navigation.TopNavigationBar);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error while clearNavigation. ", ex);
                    //qlluo: Clear navigation, no need to add report
                    //report.AddDetail(new AveWrapperReportDto("WebNavigation", "WebNavigation", AveReportObjectType.WebNavigation, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToClearWebNavigation + ex.Message));
                }
            }
        }

        private void ClearNavigation(IAveNavigationNodeCollection co)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ClearNavigation"))
            {
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
                            //qlluo: Clear navigation, no need to add report
                            //report.AddDetail(new AveWrapperReportDto("WebNavigation", "WebNavigation", AveReportObjectType.WebNavigation, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToDeleteWebNavigations + ex.Message));
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Failed to clear navigation. Error message: {0}", e.ToString()));
                        }
                    }
                }
            }
        }

        private static IAveWeb GetWebInSite(IAveSite site, string name)
        {
            string webUrl = CombineServerRelativeUrl(site, name);
            IAveWeb web = site.OpenWeb(webUrl);
            if (web != null && web.Exists)
            {
                return web;
            }
            return null;
        }

        private static IAveFolder GetFolderInSite(IAveSite site, string name)
        {
            string folderUrl = CombineServerRelativeUrl(site, name);
            var foler = site.RootWeb.GetFolder(folderUrl);
            if (foler != null && foler.Exists)
            {
                return foler;
            }
            return null;
        }

        private static string CombineServerRelativeUrl(IAveSite site, string name)
        {
            string url = null;
            if (name.StartsWith(site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                url = name;
            }
            else if (site.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                url = site.ServerRelativeUrl + name;
            }
            else
            {
                url = site.ServerRelativeUrl + "/" + name;
            }
            return url;
        }

        private void EnsureParentWeb(AveWebInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.EnsureParentWeb"))
            {

                List<KeyValuePair<string, AveWebInfo>> parentWebList = new List<KeyValuePair<string, AveWebInfo>>();
                string name = mName;
                while (true)
                {
                    if (name.Contains("/"))
                    {
                        name = name.Substring(0, name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                        using (IAveWeb web = GetWebInSite(mAveSite.SPSite, name))
                        {
                            if (web == null)
                            {
                                var folder = GetFolderInSite(mAveSite.SPSite, name);
                                if (folder == null)
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
                                    continue;
                                }
                            }
                        }
                    }
                    break;
                }
                for (int i = parentWebList.Count - 1; i >= 0; i--)
                {
                    KeyValuePair<string, AveWebInfo> value = parentWebList[i];
                    CreateNewWeb(value.Value, value.Key);
                }

            }

        }

        private void CreateNewWeb(AveWebInfo info, string webUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.CreateNewWeb_1"))
            {

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
                        if (mAveSite.SPSite.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                        {
                            mAveSite.SPSite.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                        }
                    }
                    //NewsSite site, 需要先开启site collection SharePoint Server Standard Site Collection features
                    if (info.WebTemplate.StartsWith("SPSNHOME", StringComparison.OrdinalIgnoreCase))
                    {
                        if (mAveSite.SPSite.Features[AveSP2010FeatureDefinitions.StandardSiteFeature] == null)
                        {
                            mAveSite.SPSite.Features.Add(AveSP2010FeatureDefinitions.StandardSiteFeature, true);
                        }
                        if (mAveSite.SPSite.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                        {
                            mAveSite.SPSite.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                        }
                    }

                    mSPWeb = mAveSite.SPSite.AllWebs.Add(webUrl, info.Title, info.Description, info.LCID, info.WebTemplate, info.HasUniqueRoleDefinitions, false);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("An error occurred while creating new web. SiteUrl:{0}, WebName:{1}\n error message:{2}", mAveSite.SPSite.Url, webUrl, e));
                    throw;
                }

            }

        }

        private void CreateNewWeb(AveWebInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.CreateNewWeb"))
            {

                try
                {

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
                    if (info.WebTemplate.Contains("#"))
                    {
                        int result = 0;
                        int.TryParse(info.WebTemplate.Substring(info.WebTemplate.LastIndexOf('#') + 1), out result);
                        if (result < 0)
                        {
                            log.Debug("Reset web template as empty.");
                            info.WebTemplate = string.Empty;
                        }
                    }
                    EnsureParentWeb(info);

                    ActiveDependencyFeaturesByWebTemplate(info, mAveSite.SPSite);

                    if (RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER)
                    {
                        AveSPEventReceiverConfig.EnableEventReceiver();
                    }
                    mSPWeb = mAveSite.SPSite.AllWebs.Add(mName, info.Title, info.Description, info.LCID, info.WebTemplate,
                                              false, false);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("An error occurred while creating new web. SiteUrl:{0}, WebName:{1}, LCID:{2}, Template:{3}\n error message:{4}", mAveSite.SPSite.Url, mName, info.LCID, info.WebTemplate, e));
                    throw;
                }
                finally
                {
                    if (RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER)
                    {
                        AveSPEventReceiverConfig.DisableEventReceiver();
                    }
                }

            }

        }

        private void ActiveDependencyFeaturesByWebTemplate(AveWebInfo info, IAveSite site)
        {
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
                if (site.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                {
                    site.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                }
            }
            else if (info.WebTemplate.StartsWith("SPSNHOME", StringComparison.OrdinalIgnoreCase))
            {
                //NewsSite site, 需要先开启site collection SharePoint Server Standard Site Collection features
                if (site.Features[AveSP2010FeatureDefinitions.StandardSiteFeature] == null)
                {
                    site.Features.Add(AveSP2010FeatureDefinitions.StandardSiteFeature, true);
                }
                if (site.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                {
                    site.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                }
            }
            else if (info.WebTemplate.Equals("BICENTERSITE#0", StringComparison.OrdinalIgnoreCase))
            {
                //New Business Intelligence Site,需要开启Site Collection Publishing Infrastructure Feature，PerformancePoint Services Site Collection Features
                if (site.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                {
                    site.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                }
                if (!site.IsOnlineSite && site.Features[AveSP2010FeatureDefinitions.PerformancePointServicesSiteFeature] == null)
                {
                    site.Features.Add(AveSP2010FeatureDefinitions.PerformancePointServicesSiteFeature, true);
                }
            }
        }

        /// <summary>
        /// handler to process web template and layout property
        /// 测试发现及时layout中有id，但是覆盖到目的端之后依然是正确的，可能sharepoint是以其中的url为依据进行查找的，暂时没有处理
        /// </summary>
        /// <param name="metaDataInfo"></param>
        private void RestoreWebPageLayoutAndTemplate(Dictionary<string, string> metaDataInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreWebPageLayoutAndTemplate"))
            {

                if (mSPWeb.IsPublish)//only publishing web need to handle this value
                {
                    if (metaDataInfo.ContainsKey("__InheritWebTemplates") && (!mSPWeb.IsRootWeb))//root web没有必要继承parent，目的端是什么值就是什么值
                    {
                        mSPWeb.AllProperties["__InheritWebTemplates"] = metaDataInfo["__InheritWebTemplates"];
                    }
                    if (metaDataInfo.ContainsKey("__WebTemplates"))
                    {
                        mSPWeb.AllProperties["__WebTemplates"] = MetaInfoDictionary["__WebTemplates"];
                    }
                    else
                    {
                        mSPWeb.AllProperties["__WebTemplates"] = string.Empty;
                    }
                    //rootweb can not inherit pagelayout
                    if (metaDataInfo.ContainsKey("__PageLayouts"))
                    {
                        if ((!mSPWeb.IsRootWeb) || (!MetaInfoDictionary["__PageLayouts"].Equals("__inherit", StringComparison.OrdinalIgnoreCase)))
                        {
                            mSPWeb.AllProperties["__PageLayouts"] = MetaInfoDictionary["__PageLayouts"];
                        }
                    }
                    else
                    {
                        mSPWeb.AllProperties["__PageLayouts"] = string.Empty;
                    }
                    if (metaDataInfo.ContainsKey("__DefaultPageLayout"))
                    {
                        //rootweb can not inherit __DefaultPageLayout
                        if ((!mSPWeb.IsRootWeb) || (!MetaInfoDictionary["__DefaultPageLayout"].Equals("__inherit", StringComparison.OrdinalIgnoreCase)))
                        {
                            mSPWeb.AllProperties["__DefaultPageLayout"] = MetaInfoDictionary["__DefaultPageLayout"];
                        }
                    }
                }

            }

        }
        /// <summary>
        /// 还原Reporting Services Site Settings，经过研究发现这个setting需要在AllProperties里更新
        /// 并且需要转成bool类型，如果直接赋值string类型，update之后，在SP界面上点击会报类型不匹配的错误
        /// </summary>
        /// <param name="metaDataInfo"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPRSAccessibleTablix is a key")]
        private void RestoreReportingServiceSiteSettings(Dictionary<string, string> metaDataInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreReportingServiceSiteSettings"))
            {

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

            }

        }

        private void RestoreCommunitySettings(string[] communitySetting)
        {
            if (mSPWeb.Features[new Guid("961D6A9C-4388-4cf2-9733-38EE8C89AFD4")] != null)
            {
                foreach (string setting in communitySetting)
                {
                    if (MetaInfoDictionary.ContainsKey(setting))
                    {
                        if (string.Equals(setting, "vti_CommunityEnableAutoApproval") && !mSPWeb.IsRootWeb)
                        {
                            continue;
                        }
                        else if (string.Equals(setting, "vti_CommunityEnableReportAbuse"))
                        {
                            Guid abuseReportsList_FeatureId = new Guid("C6A92DBF-6441-4b8b-882F-8D97CB12C83A");
                            string originalSetting = mSPWeb.AllProperties[setting] != null ? mSPWeb.AllProperties[setting].ToString() : string.Empty;
                            if (mSPWeb.Features[abuseReportsList_FeatureId] != null && !string.Equals(originalSetting, MetaInfoDictionary[setting], StringComparison.OrdinalIgnoreCase))
                            {
                                mSPWeb.AllProperties[setting] = MetaInfoDictionary[setting];
                                try
                                {
                                    bool enable = Boolean.Parse(MetaInfoDictionary[setting]);
                                    mSPWeb.EnableDisableAbuseReports(enable);
                                }
                                catch (Exception ex)
                                {
                                    log.Log(AveLogLevel.INFO, "An exception occurred while set web abuse reports setting. status:{0}, exception:{1}", MetaInfoDictionary[setting], ex.ToString());
                                }
                            }
                        }
                        else if (string.Equals(setting, "vti_CommunityEstablishedDate"))
                        {
                            try
                            {
                                DateTime time = Convert.ToDateTime(MetaInfoDictionary[setting]);
                                mSPWeb.AllProperties["vti_CommunityEstablishedDate"] = mSPWeb.RegionalSettings.TimeZone.LocalTimeToUTC(time);
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.WARN, "An exception occurred while set web established date setting. DateTime:{0}, exception:{1}", MetaInfoDictionary[setting], ex.ToString());
                            }
                        }
                        else
                        {
                            mSPWeb.AllProperties[setting] = MetaInfoDictionary[setting];
                        }
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_catalogsourcesconfig is a property")]
        private void RestoreManageCatalogConnections()
        {
            string key = "_catalogsourcesconfig";
            if (MetaInfoDictionary.ContainsKey(key))
            {
                string realvalue = DealWithCatalogSourcesConfig(MetaInfoDictionary[key]);
                if (mSPWeb.AllProperties.ContainsKey(key))
                {
                    mSPWeb.AllProperties[key] = realvalue;
                }
                else
                {
                    mSPWeb.AllProperties.Add(key, realvalue);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_catalogsourcesconfig is a property")]
        private string DealWithCatalogSourcesConfig(string sourcesConfig)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(sourcesConfig);
                XmlElement root = doc.DocumentElement;
                foreach (XmlElement catalogConnectionSetting in root.ChildElements())
                {
                    if (catalogConnectionSetting.GetElementsByTagName("ConnectedWebServerRelativeUrl") != null)
                    {
                        catalogConnectionSetting.GetElementsByTagName("ConnectedWebServerRelativeUrl")[0].InnerText = mSPWeb.ServerRelativeUrl;
                    }
                    if (catalogConnectionSetting.GetElementsByTagName("ConnectedWebId") != null)
                    {
                        catalogConnectionSetting.GetElementsByTagName("ConnectedWebId")[0].InnerText = mSPWeb.ID.ToString();
                    }
                    if (catalogConnectionSetting.GetElementsByTagName("CatalogNavigationTerm") != null)
                    {
                        catalogConnectionSetting.GetElementsByTagName("CatalogNavigationTerm")[0].InnerText = ParentSite.MetadataService.TermSetIdMapping[new Guid(catalogConnectionSetting.GetElementsByTagName("CatalogNavigationTerm")[0].InnerText)].ToString();
                    }

                }
            }
            catch (Exception e)
            {
                log.Warn("An Error occurred when restoring _catalogsourcesconfig.Exception:{0}", e.ToString());
                return doc.OuterXml;
            }
            return doc.OuterXml;
        }

        private void RestoreNormalWebProperties(string[] settings)
        {
            foreach (string setting in settings)
            {
                if (MetaInfoDictionary.ContainsKey(setting))
                {
                    mSPWeb.AllProperties[setting] = MetaInfoDictionary[setting];
                }
            }
        }

        private void RestoreDateTimeWebProperties(string[] settings)
        {
            foreach (string setting in settings)
            {
                if (MetaInfoDictionary.ContainsKey(setting))
                {
                    DateTime time = Convert.ToDateTime(MetaInfoDictionary[setting].ToString());
                    mSPWeb.AllProperties[setting] = time;
                }
            }
        }

        private string ParseMasterUrl(string destMasterUrl, string masterUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ParseMasterUrl"))
            {

                string temp = masterUrl;
                if (destMasterUrl != null)
                {
                    int index = temp.IndexOf("_catalogs/masterpage/", StringComparison.OrdinalIgnoreCase);
                    string sourceWebUrl = "/" + temp.Substring(0, index).Trim('/');
                    if (mWebInfo.Name.Equals(sourceWebUrl, StringComparison.OrdinalIgnoreCase))//web使用的是自己的master page
                    {
                        int destIndex = destMasterUrl.IndexOf("/_catalogs/masterpage/", StringComparison.OrdinalIgnoreCase);
                        string destPartUrl = string.Empty;
                        if (destIndex > 0)
                        {
                            destPartUrl = destMasterUrl.Substring(0, destIndex);
                        }
                        //根据目的端所使用MasterPageUrl的情况来为masterPageUrl赋值
                        if (destIndex < 0 || destPartUrl.Equals(mSPWeb.ServerRelativeUrl))
                        {
                            // 如果开了publishing feature，master page的url会改变到rootweb上，所以需要用rootweb的server relative url，不然用post还master page settings的时候会因为这个url非法而失败。
                            if (this.SPWeb.Features[AveSP2010FeatureDefinitions.PublishingWeb] != null)
                            {
                                masterUrl = mSPWeb.Site.RootWeb.ServerRelativeUrl.TrimEnd('/') + "/" + temp.Substring(index);
                            }
                            else
                            {
                                masterUrl = mSPWeb.ServerRelativeUrl.TrimEnd('/') + "/" + temp.Substring(index);
                            }
                        }
                        else
                        {
                            masterUrl = destPartUrl + "/" + temp.Substring(index);
                        }
                    }
                    else if (ParentSite.SourceSiteInfo.ServerRelativeUrl.Equals(sourceWebUrl, StringComparison.OrdinalIgnoreCase))//表示Site用的是Root Web的Master Page
                    {
                        masterUrl = mSPWeb.Site.RootWeb.ServerRelativeUrl.TrimEnd('/') + "/" + temp.Substring(index);
                    }
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

            }

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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        private string GetCssFolderUniqueCode(string webCssFolder)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetCssFolderUniqueCode"))
            {

                int index = webCssFolder.IndexOf('-');
                if (index < 0) throw new Exception("the format of the CssFolderUrl is not correct" + webCssFolder);
                return "Custom.thmx-" + webCssFolder.Substring(index + 1);

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        private static string GetCssFolderUrlPath(IAveWeb web)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.GetCssFolderUrlPath"))
            {

                string prefixedUrl = @"~sitecollection/_catalogs/theme";
                StringBuilder builder = new StringBuilder(prefixedUrl.Length);
                int startIndex = -1;
                if (prefixedUrl.StartsWith(AveSPUtility.WebRelativeUrlPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append(web.ServerRelativeUrl);
                    startIndex = AveSPUtility.WebRelativeUrlPrefix.Length;
                }
                else
                {
                    if (!prefixedUrl.StartsWith(AveSPUtility.SiteRelativeUrlPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return prefixedUrl;
                    }
                    if (web.Site.ServerRelativeUrl == null)
                    {
                        builder.Append(web.Site.ServerRelativeUrl);
                    }
                    else
                    {
                        builder.Append(web.Site.ServerRelativeUrl);
                    }
                    startIndex = AveSPUtility.SiteRelativeUrlPrefix.Length;
                }
                if ((builder.Length <= 0) || (builder[builder.Length - 1] != '/'))
                {
                    builder.Append('/');
                }
                builder.Append(prefixedUrl.Substring(startIndex));
                return builder.ToString();

            }

        }

        public void RestoreThemeCssFolderUrl()
        {
            if (base.IsSettingRestored)
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAlternateCSSUrl"))
                {

                    try
                    {
                        //CM存在Web to Site的还原。如果目的端是root web，需要还原Theme.
                        if (WebSettingInfo != null && (!WebSettingInfo.InheritsThemedCssFolderUrl.Value || mSPWeb.IsRootWeb) && !string.IsNullOrEmpty(this.ThemedCssFolderUrl))//存在theme,需要还原
                        {
                            mSPWeb.RestoreTheme(WebSettingInfo, this.ThemedCssFolderUrl);
                        }
                        else if (WebSettingInfo != null && (!WebSettingInfo.InheritAlertCss.Value || mSPWeb.IsRootWeb) && WebSettingInfo.ThemedCssUrl != null && !string.IsNullOrEmpty(WebSettingInfo.ThemedCssUrl.Value))
                        {
                            mSPWeb.ApplyTheme(WebSettingInfo.Theme.Value);
                        }
                        else if (!Object.Equals(mSPWeb, null) && (!WebSettingInfo.InheritsThemedCssFolderUrl.Value || mSPWeb.IsRootWeb) && !string.IsNullOrEmpty(mSPWeb.ThemedCssFolderUrl) && base.IsSettingRestored)//备份了setting信息并且为空，说明源端是default
                        {
                            this.ThmxTheme.RemoveThemeFromWeb(mSPWeb, false);
                        }
                        //子web的inherit属性，及时跟parent一致但是使用api操作后都会发生变化，所以这里需要确保一下
                        if (!mSPWeb.IsRootWeb)
                        {
                            try
                            {
                                if ((WebSettingInfo.WebTheme != null && WebSettingInfo.WebTheme.IsAvailable && WebSettingInfo.WebTheme.Value.InheritsThemedCssFolderUrl)
                                    || (WebSettingInfo.InheritsThemedCssFolderUrl != null && WebSettingInfo.InheritsThemedCssFolderUrl.IsAvailable && WebSettingInfo.InheritsThemedCssFolderUrl.Value))
                                {
                                    mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = "True";
                                    mSPWeb.ThemedCssFolderUrl = mSPWeb.ParentWeb.ThemedCssFolderUrl;
                                    mSPWeb.Update();
                                    if (mWebSettingInfo.NavigationWebAndPage != null && mWebSettingInfo.NavigationWebAndPage.IsAvailable)
                                    {
                                        getWebsAndPages(mWebSettingInfo.NavigationWebAndPage.Value);
                                    }
                                }
                                //else 添加的是对于ADO-100486的更改
                                else
                                {
                                    mSPWeb.AllProperties["__InheritsThemedCssFolderUrl"] = "False";
                                    mSPWeb.Update();
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.InheritWebPropertyFailed, mSPWeb.Url, e);
                            }
                        }
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        //qlluo: Post action do not support report, remove it.
                        //report.AddDetail(new AveWrapperReportDto("SiteTheme", "SiteTheme", AveReportObjectType.SiteThemeCssFolderUrl, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreSiteTheme + ex.Message));
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

                }

            }
        }

        public void RestoreAlternateCSSUrl()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAlternateCSSUrl"))
            {

                try
                {
                    if (mAveSite.NotRestoreWebCss)
                    {
                        log.Debug("Don't restore alternate css url");
                    }
                    else
                    {
                        if (!base.IsSettingRestored || this.inheritAlertCss)
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
                }
                catch (AveSecurityTrimingException ex)
                {
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("AlternateCSSUrl", "AlternateCSSUrl", AveReportObjectType.AlternateCSSUrl, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreAlternateCSSUrl + ex.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore web's alternateCssUrl. web id:{0}, web url:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, ex));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore web's alternateCssUrl. web id:{0}, web url:{1}\n error message:{2}", mSPWeb.ID, mSPWeb.Url, e));
                    //throw e;
                }

            }

        }

        //update web author
        public void RestoreAuthor()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAuthor"))
            {

                if (!base.IsSettingRestored)
                {
                    return;
                }
                //mQueryService != null，限定必须是Local站点。
                if (mQueryService != null && mWebSettingInfo != null && mWebSettingInfo.Author != null && mWebSettingInfo.Author.IsAvailable)
                {
                    if (this.mAveSite.SPSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                    {
                        mSPWeb.Update();
                        mQueryService.UpdateWebsAuthorByNative(mAveSite.SPMembers.FindMemberId(mWebSettingInfo.Author.Value), this.mAveSite.SPSite.ID, mId);
                        //数据库更新，需要ReloadWeb，否则失效。
                        this.ReloadWeb();
                    }
                    else
                    {
                        var principal = mAveSite.SPMembers.FindMember(mWebSettingInfo.Author.Value, true);
                        if (principal is IAveUser)
                        {
                            mSPWeb.Author = principal as IAveUser;
                            //此方法已暴漏给外围，所以需要在这update，否则可能更新不进去。
                            mSPWeb.Update();
                        }
                        else
                        {
                            if (principal == null)
                            {
                                log.Warn("Restore web author failed, can not find the principal, source author id is {0}.", mWebSettingInfo.Author.Value);
                            }
                            else
                            {
                                log.Warn("Restore web author failed, find result is not an user. LoginName: {0}, ID: {1}, Type: {2}, source author id is {3}", principal.LoginName, principal.ID, principal.GetType().FullName, mWebSettingInfo.Author.Value);
                            }
                        }
                    }
                }

            }

        }

        public void RestoreWelcomePage()
        {
            if (base.IsSettingRestored)
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreWelcomePage"))
                {

                    try
                    {
                        if (mWebSettingInfo != null && mWebSettingInfo.WelcomePage != null && mWebSettingInfo.WelcomePage.IsAvailable
                            && !mWebSettingInfo.WelcomePage.Value.Equals(mSPWeb.RootFolder.WelcomePage))
                        {
                            if (string.IsNullOrEmpty(mWebSettingInfo.WelcomePage.Value))
                            {
                                IAveFolder folder = mSPWeb.RootFolder;
                                folder.WelcomePage = mWebSettingInfo.WelcomePage.Value;
                                folder.Update();
                            }
                            else
                            {
                                IAveFile file = mSPWeb.GetFile(mWebSettingInfo.WelcomePage.Value);
                                if (!file.Exists)
                                {
                                    mAveSite.MappingManager.SiteMappingManager.UnRestoredWelcomePages.Add(mSPWeb.ID, mWebSettingInfo.WelcomePage.Value);
                                }
                                else if (this.mAveSite.SPContextKind != AveContextKind.ClientObjectModel && AveEnv.IsMoss && mSPWeb.IsPublish)
                                {
                                    mAveSite.Publishing.SetWelcomePage(mSPWeb, mWebSettingInfo.WelcomePage.Value);
                                }
                                else
                                {
                                    IAveFolder folder = mSPWeb.RootFolder;
                                    folder.WelcomePage = mWebSettingInfo.WelcomePage.Value;
                                    folder.Update();
                                }
                            }
                        }
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        log.Warn(WrapperRestoreResource.RestoreWebWelcomePageFailed, mSPWeb.Url, mWebSettingInfo.WelcomePage, ex);
                        //qlluo: Post action do not support report, remove it.
                        //report.AddDetail(new AveWrapperReportDto("WelcomePage", "WelcomePage", AveReportObjectType.WelcomePage, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreWelcomePage + ex.Message));
                    }
                    catch (Exception e)
                    {
                        log.Warn(WrapperRestoreResource.RestoreWebWelcomePageFailed, mSPWeb.Url, mWebSettingInfo.WelcomePage, e);
                    }

                }

            }
        }

        public void RestoreSiteLogoUrl()
        {
            if (base.IsSettingRestored)
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreSiteLogoUrl"))
                {

                    try
                    {
                        InnerRestoreSiteLogoUrl();
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        //qlluo: Post action do not support report, remove it.
                        //report.AddDetail(new AveWrapperReportDto("RestoreSiteLogoUrl", "RestoreSiteLogoUrl", AveReportObjectType.SiteLogoUrl, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreSiteLogo + ex.Message));
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

                }

            }
        }

        private void InnerRestoreSiteLogoUrl()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.InnerRestoreSiteLogoUrl"))
            {

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

            }

        }

        private bool IsLogoUrlInSiteCollection(string siteLogoUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.IsLogoUrlInSiteCollection"))
            {

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

            }

        }

        public void RestoreHiddenPageProperty()
        {
            if (base.IsSettingRestored)
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreHiddenPageProperty"))
                {

                    try
                    {
                        if (mAveSite.SPContextKind == AveContextKind.ClientObjectModel || AveEnv.IsMoss)
                        {
                            ReloadHiddenPages();
                        }
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenPageProperty. error:{0}", ex.ToString());
                        //qlluo: Post action do not support report, remove it.
                        //report.AddDetail(new AveWrapperReportDto("HiddenPageProperty", "HiddenPageProperty", AveReportObjectType.HiddenPageProperty, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreHiddenPageProperty + ex.Message));
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenPageProperty. error:{0}", e.ToString());
                        //mLog.Warn("An error occurred while RestoreHiddenPageProperty. error:{{0}", e.ToString());
                    }

                }

            }
        }

        private void ReloadHiddenPages()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.LoadHiddenPages"))
            {

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
                            LoadHiddenPages(hiddenPages, ParentSite.MappingManager, mAveSite.SPSite.ID, mSPWeb);
                        }
                        else
                        {
                            Dictionary<Guid, string> hiddenPages = mWebSettingInfo.NavigationWebAndPage.Value["Hidden"]["page"];
                            LoadHiddenPagesLocal(hiddenPages, ParentSite.MappingManager, mAveSite.SPSite.ID, mSPWeb);
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
                        else
                        {
                            Dictionary<Guid, Guid> HiddenWebMapping = ReloadHiddenWebPropertyLocal(mWebSettingInfo, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl, ParentSite.MappingManager.SiteMappingManager.WebIDMapping, mSPWeb);
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

            }

        }

        /// <summary>
        /// 通过hiddenpage的value中原端page url, 解析出目的端page的leafname和dirname, QueryService查询目的端page guid.
        /// </summary>
        /// <param name="hiddenPages"></param>
        /// <param name="mappingManager"></param>
        /// <param name="siteId"></param>
        /// <param name="web"></param>
        /// <param name="getItemIdByName"></param>
        private void LoadHiddenPages(Dictionary<Guid, string> hiddenPages, AveMappingManager mappingManager, Guid siteId, IAveWeb web)
        {
            foreach (Guid id in hiddenPages.Keys)
            {
                if (!mappingManager.WebMappingManager.PageItemSDGuidMapping.ContainsKey(id))
                {
                    string leafName;
                    string dirName;
                    GetLeafNameAndDirName(mappingManager, web, hiddenPages[id], out leafName, out dirName);
                    var newId = mQueryService.GetItemIdByName(siteId, web.ID, leafName, dirName);
                    if (newId != Guid.Empty)
                    {
                        mappingManager.WebMappingManager.PageItemSDGuidMapping[id] = newId;
                    }
                }
            }
        }

        private void GetLeafNameAndDirName(AveMappingManager mappingManager, IAveWeb web, string path, out string leafName, out string dirName)
        {
            if (path.IndexOf('/') >= 0)
            {
                dirName = path.TrimEnd('/');
                if (!dirName.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    dirName = "/" + dirName;
                }
                string value;
                if (mappingManager.SiteMappingManager.GetValueFromListUrlMapping(dirName, out value))
                {
                    dirName = value;
                }
                else
                {
                    //dirName = web.ServerRelativeUrl + "/Pages";
                    dirName = GetPagesListDirname(web);
                }
                dirName = dirName.TrimStart('/');
                leafName = path.Substring(path.LastIndexOf('/') + 1);
            }
            else
            {
                dirName = null;
                leafName = path;
            }
        }

        private string GetPagesListDirname(IAveWeb web)
        {
            string dirName;
            var tempWeb = web.GetPublishingWeb;
            if (tempWeb != null)
            {
                dirName = string.Format("{0}/{1}", web.ServerRelativeUrl, tempWeb.PagesListName);
                tempWeb.Dispose();
            }
            else
            {
                dirName = string.Format("{0}/Pages", web.ServerRelativeUrl);
            }
            return dirName.Trim('/');
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "pendingreq", Justification = "page name in SharePoint")]
        internal void RestoreAccessRequestProperties(Guid listID, string url)
        {
            try
            {
                if (!Guid.Empty.Equals(listID))
                {
                    bool hasChanged = false;
                    if (!AveWeb.AllProperties.Contains("_VTI_ACCESSREQUESTSLISTID"))
                    {
                        string value = listID.ToString() + ",Access Requests";
                        AveWeb.AllProperties.Add("_VTI_ACCESSREQUESTSLISTID", value);
                        hasChanged = true;
                    }
                    if (!AveWeb.AllProperties.Contains("_VTI_PENDINGREQUESTSVIEWID"))
                    {
                        url = AveReplaceProcessor.UrlReplace(url, ParentSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                        url.Trim('/');
                        url = url + "/" + "pendingreq.aspx";
                        AveWeb.AllProperties.Add("_VTI_PENDINGREQUESTSVIEWID", url);
                        hasChanged = true;
                    }
                    if (hasChanged)
                    {
                        AveWeb.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Can't add web properties associated with Access Requests List:Exception:{0}", ex.ToString());
            }
        }

        private Dictionary<Guid, Guid> ReloadHiddenWebPropertyLocal(AveWebSettingInfo mWebSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping, IAveWeb mSPWeb)
        {
            Dictionary<Guid, Guid> hiddenWebMapping = new Dictionary<Guid, Guid>();
            try
            {
                Dictionary<Guid, string> hiddenWeb = new Dictionary<Guid, string>();
                hiddenWeb = mWebSettingInfo.NavigationWebAndPage.Value["Hidden"]["web"];
                IAveWebCollection webs = mSPWeb.Webs;
                foreach (Guid Id in hiddenWeb.Keys)
                {
                    if (!webIdMapping.ContainsKey(Id))
                    {
                        string webUrl = hiddenWeb[Id];
                        webUrl = AveReplaceProcessor.UrlReplace(hiddenWeb[Id], siteManagedMappings, new ReplaceOption(true), sourceSiteInfo, destSiteUrl);
                        if (webUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            webUrl = webUrl.Substring(1);
                        }
                        foreach (IAveWeb web in webs)
                        {
                            if (web.Url.Equals(webUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!hiddenWebMapping.ContainsKey(Id))
                                {
                                    hiddenWebMapping[Id] = web.ID;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Mapping hidden web navigation failed in local . error : {0}", ex.ToString());
            }
            return hiddenWebMapping;
        }

        private void LoadHiddenPagesLocal(Dictionary<Guid, string> hiddenPages, AveMappingManager mappingManager, Guid guid, IAveWeb mSPWeb)
        {
            try
            {
                foreach (Guid id in hiddenPages.Keys)
                {
                    if (!mappingManager.WebMappingManager.PageItemSDGuidMapping.ContainsKey(id))
                    {
                        string path = hiddenPages[id];
                        if (path.IndexOf('/') >= 0)
                        {
                            string dirName = path.Substring(0, path.LastIndexOf('/'));
                            if (!dirName.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                dirName = "/" + dirName;
                            }
                            string value;
                            if (mappingManager.SiteMappingManager.GetValueFromListUrlMapping(dirName, out value))
                            {
                                dirName = value;
                            }
                            else
                            {
                                dirName = mSPWeb.ServerRelativeUrl + "/Pages";
                            }
                            dirName = dirName.TrimStart('/');
                            string leafName = path.Substring(path.LastIndexOf('/') + 1);
                            string fullUrl = dirName + "/" + leafName;
                            var pageListIdStr = mSPWeb.AllProperties["__PagesListId"];

                            if (pageListIdStr != null)
                            {
                                Guid listId = new Guid(pageListIdStr.ToString());
                                IAveList list = mSPWeb.Lists[listId];
                                IAveListItemCollection items = list.Items;
                                foreach (IAveListItem item in items)
                                {
                                    if (item.File.ServerRelativeUrl.TrimStart('/').Equals(fullUrl, StringComparison.OrdinalIgnoreCase) && item.File.Name.Equals(leafName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        mappingManager.WebMappingManager.PageItemSDGuidMapping[id] = item.UniqueId;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Mapping pages id failed. error : {0}", ex.ToString());
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Web property about associate groups.")]
        public void RestoreAssociateGroups()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreAssociateGroups"))
            {

                try
                {
                    if (MetaInfoDictionary != null && this.SecurityRestored)
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
                                        IAvePrincipal p = this.ParentSite.SPMembers.FindMember(i, false);
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
                                IAvePrincipal p = this.ParentSite.SPMembers.FindMember(groupId, false);
                                if (p != null)
                                {
                                    groupId = p.ID;
                                    mSPWeb.AllProperties["vti_associateownergroup"] = groupId.ToString();
                                    needUpdate = true;
                                }
                            }
                        }
                        if (MetaInfoDictionary.ContainsKey("vti_associatemembergroup"))
                        {
                            string associateMemberGroup = MetaInfoDictionary["vti_associatemembergroup"];
                            int groupId = 0;
                            if ((!string.IsNullOrEmpty(associateMemberGroup) && int.TryParse(associateMemberGroup, out groupId)) && (groupId > 0))
                            {
                                IAvePrincipal p = this.ParentSite.SPMembers.FindMember(groupId, false);
                                if (p != null)
                                {
                                    groupId = p.ID;
                                    mSPWeb.AllProperties["vti_associatemembergroup"] = groupId.ToString();
                                    needUpdate = true;
                                }
                            }
                        }
                        if (MetaInfoDictionary.ContainsKey("vti_associatevisitorgroup"))
                        {
                            string associateVisitorGroup = MetaInfoDictionary["vti_associatevisitorgroup"];
                            int groupId = 0;
                            if ((!string.IsNullOrEmpty(associateVisitorGroup) && int.TryParse(associateVisitorGroup, out groupId)) && (groupId > 0))
                            {
                                IAvePrincipal p = this.ParentSite.SPMembers.FindMember(groupId, false);
                                if (p != null)
                                {
                                    groupId = p.ID;
                                    mSPWeb.AllProperties["vti_associatevisitorgroup"] = groupId.ToString();
                                    needUpdate = true;
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
                    //qlluo: Post action do not support report, remove it.                    
                    //report.AddDetail(new AveWrapperReportDto("RestoreAssociateGroup", "RestoreAssociateGroup", AveReportObjectType.AssociteGroup, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreAssociateGroups + ex.Message));
                    log.Warn("update associategroups exception" + ex.ToString());
                }
                catch (Exception e)
                {
                    log.Warn("update associategroups exception" + e.ToString());
                }

            }

        }
        /// <summary>
        ///根据loginname获取user id,如果user不存在，则根据isAddUser判断是否添加user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAddUser"></param>
        /// <returns></returns>
        public int GetUserIdByName(string name, bool isAddUser)
        {
            int id = GetUserIdByName(name);
            if (isAddUser && id < 0)
            {
                try
                {
                    name = ParentSite.SPMembers.GetMappingUserLogin(name, true);
                    id = this.SPWeb.EnsureAvailableUser(name).ID;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while add user. user login name: {0}, error message: {1}", name, e.ToString());
                }
            }
            return id;
        }
        public int GetUserIdByName(string name)
        {
            using (new AvePerformanceScope("Restore.AveSPWeb.GetUserIdByName"))
            {
                IAveGroup spGroup = null;
                IAveUser spUser = null;
                int id = -1;
                try
                {
                    spGroup = mSPWeb.SiteGroups[name];
                    id = spGroup.ID;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserIdByNameFailed, name, e.Message);
                }
                if (spGroup == null)
                {
                    try
                    {
                        name = ParentSite.SPMembers.GetMappingUserLogin(name, true);
                        spUser = mSPWeb.SiteUsers[name];
                        id = spUser.ID;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetUserIdByNameFailed, name, e.Message);
                    }
                }
                return id;
            }
        }
        public int GetUserIdByDisplayName(string userName)
        {
            using (new AvePerformanceScope("Restore.AveSPWeb.GetUserIdByUserName"))
            {
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
            }
        }

        public void ReloadWeb()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ReloadWeb"))
            {

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

            }

        }

        /// <summary>
        /// 如果程序运行一天以上，访问Web的一些属性，例如WebPartManager或者CreatList对象，都会出现如下错误：
        /// System.Runtime.InteropServices.COMException (0x80090317): The context has expired and can no longer be used. 
        /// </summary>
        /// <param name="ingoreTimeout"></param>
        internal bool ReloadWebAndParentInternalForSPRequestTimeout(bool ingoreTimeout)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ReloadWebAndParentInternalForSPRequestTimeout"))
            {

                if (ingoreTimeout || ParentSite.mSPRequestTimeout.AddHours(ParentSite.mHoursReloadSite) < DateTime.UtcNow)
                {
                    this.ParentSite.ReloadSite();
                    this.ReloadWeb();
                    return true;
                }
                return false;

            }

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

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.ClearDefaultList"))
            {

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

            }

        }

        public void RestoreCacheProfileListId()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreCacheProfileListId"))
            {

                try
                {
                    if (MetaInfoDictionary == null || !MetaInfoDictionary.ContainsKey("__CacheProfileListId") || !mSPWeb.AllProperties.ContainsKey("__CacheProfileListId"))
                    {
                        return;
                    }
                    string cacheProfileListId = MetaInfoDictionary["__CacheProfileListId"];
                    Guid oldcacheProfileListId = new Guid(cacheProfileListId);
                    var value = Guid.Empty;
                    if (string.IsNullOrEmpty(cacheProfileListId) || !ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(oldcacheProfileListId, out value))
                    {
                        return;
                    }
                    string webCacheProfileListId = mSPWeb.AllProperties["__CacheProfileListId"].ToString();
                    string newCacheProfileListId = value.ToString();
                    if (!webCacheProfileListId.Equals(newCacheProfileListId, StringComparison.OrdinalIgnoreCase))
                    {
                        mSPWeb.AllProperties["__CacheProfileListId"] = newCacheProfileListId;
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("an error occurred while restore the web's CacheProfileListId in metainfo.\n error message:" + ex.ToString());
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("CacheProfileListId", "CacheProfileListId", AveReportObjectType.CacheProfileListId, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreCacheProfileListId + ex.Message));
                }
                catch (Exception e)
                {
                    log.Warn("an error occurred while restore the web's CacheProfileListId in metainfo.\n error message:" + e.ToString());
                }

            }

        }

        public void RestoreRelationShipListSetting()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreRelationShipListSetting"))
            {

                try
                {
                    if (MetaInfoDictionary == null || !MetaInfoDictionary.ContainsKey("_VarRelationshipsListId"))
                    {
                        return;
                    }
                    string varRelationshipsListId = MetaInfoDictionary["_VarRelationshipsListId"];
                    Guid oldRelationShipListId = new Guid(varRelationshipsListId);
                    var value = Guid.Empty;
                    if (string.IsNullOrEmpty(varRelationshipsListId) || !ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(oldRelationShipListId, out value))
                    {
                        return;
                    }
                    string webVarRelationshipsListId = mSPWeb.AllProperties["_VarRelationshipsListId"].ToString();
                    string siteVarRelationshipsListId = value.ToString();
                    if (!webVarRelationshipsListId.Equals(siteVarRelationshipsListId, StringComparison.OrdinalIgnoreCase))
                    {
                        mSPWeb.AllProperties["_VarRelationshipsListId"] = siteVarRelationshipsListId;
                        mSPWeb.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + ex.ToString());
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RelationShipListSetting", "RelationShipListSetting", AveReportObjectType.RelationShipListSetting, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreRelationShipListSetting + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + e.ToString());
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore relationshipList setting. \n error message:{0}", e));
                }

            }

        }

        /// <summary>
        /// ADO-26529 开启content orginazer feature之后还原到目的端，目的端site content orginazer setting无法打开
        /// 查看SharePoint log以及reflector了解到该页面在加载的时候会通过web property中的emailsubmittedrecordslistid属性来先获取
        /// 目的端的Submitted E-mail Records这个list，这个属性是该list的guid，如果不进行替换，会出现无法找到list而页面加载错误的现象
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "emailsubmittedrecordslistid is a key")]
        public void RestoreEmailSubmittedRecordsListIDProperty()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreEmailSubmittedRecordsListIDProperty"))
            {

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
                    var value = Guid.Empty;
                    if (string.IsNullOrEmpty(EmailSubmittedRecordsListID) || !ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(oldEmailSubmittedRecordsListID, out value))
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
                        newEmailSubmittedRecordsListID = value;
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
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("EmailSubmittedRecordsListIDProperty", "EmailSubmittedRecordsListIDProperty", AveReportObjectType.EmailSubmittedRecordsListIDProperty, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreEmailSubmittedRecordsListIDProperty + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "error restore relationshipListId" + e.ToString());
                    //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore relationshipList setting. \n error message:{0}", e));
                }

            }

        }

        [Obsolete("Used for DocAve5 Site bin Restore, never used in DocAve6")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Japanese.")]
        public void RestoreOriginTitle()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreOriginTitle"))
            {

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
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("OriginTitle", "OriginTitle", AveReportObjectType.OriginTitle, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreOriginTitle + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Warn("RestoreOriginalTitle failed: " + ex.ToString());
                }

            }

        }
        /// <summary>
        /// If ContentOrginazer (SP2010) feature activated in source, we need to update the web properties base on it
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        public void RestoreContentOrginazerSetting()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreContentOrginazerSetting"))
            {

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
                        //不还原这个属性，在开启Feature的时候会自动赋值
                        //"_dlc_repositoryusersgroup",
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
                            //if (key.Equals("_dlc_repositoryusersgroup", StringComparison.OrdinalIgnoreCase))
                            //{
                            //    int newAddGroupId = -1;
                            //    if (mSPWeb.AllProperties.ContainsKey(key) && mSPWeb.AllProperties[key] != null)
                            //    {
                            //        newAddGroupId = Convert.ToInt32(mSPWeb.AllProperties[key]);
                            //    }
                            //    int oldId = Convert.ToInt32(MetaInfoDictionary[key].Replace("\\\\", "\\"));
                            //    int groupId = mAveSite.SPMembers.FindMemberId(oldId);
                            //    mSPWeb.AllProperties[key] = groupId;
                            //    if (this.IsNewCreated && newAddGroupId != -1 && newAddGroupId != groupId)
                            //    {
                            //        this.ParentSite.SPSite.RootWeb.Groups.RemoveByID(newAddGroupId);
                            //    }
                            //}
                            //else
                            //{
                            mSPWeb.AllProperties[key] = MetaInfoDictionary[key].Replace("\\\\", "\\");
                            //}
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
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("ContentOrganizationSetting", "ContentOrganizationSetting", AveReportObjectType.ContentOrganizationSetting, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreContentOrganizationSetting + ex.Message));
                    log.Warn("There is an error when update \"Content Organizer\" feature, please try again or configure manually. \t", ex);
                }
                catch (Exception ex)
                {
                    log.Warn("There is an error when update \"Content Organizer\" feature, please try again or configure manually. \t", ex);
                }

            }

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

        /// <summary>
        /// 由于还原web property的时候，可能还没有打破继承，所以这个属性要放到post action里面还原
        /// </summary>
        public void RestoreRequestAccessEmail()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWeb.RestoreRequestAccessEmail"))
            {

                bool changed = false;
                try
                {
                    if (mSPWeb.HasUniqueRoleAssignments && mWebSettingInfo != null)
                    {
                        if (mWebSettingInfo.AllowMembersEditMembership != null
                            && mWebSettingInfo.AllowMembersEditMembership.IsAvailable
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
                    if (mWebSettingInfo.AccessRequestSiteDescription != null
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
                catch (Exception e)
                {
                    log.Warn("An error occurred while set web RequestAccessEmail. error:{0}", e.ToString());
                }
            }
        }

        public void RestoreProjectPolicy()
        {
            if (ParentSite.ObjectModelFactory as AveObjectModelFactoryExtension != null && WebSettingInfo != null)
            {
                AveProjectPolicyInfo policyInfo = new AveProjectPolicyInfo();
                if (null != WebSettingInfo.IsSiteClosed && WebSettingInfo.IsSiteClosed.IsAvailable)
                {
                    policyInfo.IsSiteClosed = WebSettingInfo.IsSiteClosed.Value;
                }
                if (null != WebSettingInfo.ProjectPolicyContentTypeId && WebSettingInfo.ProjectPolicyContentTypeId.IsAvailable)
                {
                    policyInfo.ProjectPolicyContentType = WebSettingInfo.ProjectPolicyContentTypeId.Value;
                }
                if (null != WebSettingInfo.SiteClosedTime && WebSettingInfo.SiteClosedTime.IsAvailable)
                {
                    policyInfo.SiteClosedTime = WebSettingInfo.SiteClosedTime.Value;
                }
                if (null != WebSettingInfo.ProjectExpirationDate && WebSettingInfo.ProjectExpirationDate.IsAvailable)
                {
                    policyInfo.projectExpirationDate = WebSettingInfo.ProjectExpirationDate.Value;
                }
                if (null != WebSettingInfo.ProjectPolicyName && WebSettingInfo.ProjectPolicyName.IsAvailable)
                {
                    policyInfo.ProjectPolicyName = WebSettingInfo.ProjectPolicyName.Value;
                }

                if (ParentSite.SPContextKind.IsServerMode13Upper() && AvePoint.Common.AveEnv.IsMoss)
                {
                    IAveProjectPolicyItemListUtility utility = ((AvePoint.Wrapper.Common.Extension.AveObjectModelFactoryExtension)ParentSite.ObjectModelFactory).CreatePolicyItemListUtility();
                    utility.SetObjectData(ParentSite.SPSite.ID, SPWeb.ID, policyInfo);
                }
            }
        }

        public void RestoreWebIndexedProperty()
        {
            if (!AveContextKindExtension.IsServerMode(ParentSite.SPContextKind) && this.SPWeb.WebTemplate.Equals(AveCommunitiesConstants.CommunityTemplateName, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    IAveList discussionList = this.SPWeb.GetListByName("Discussions List", true);
                    IAveList memberList = this.SPWeb.GetListByName("Community Members", true);
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
        /// 由于WebProperties分为两种，一种是携带类型的AllProperties，一种是只是String类型的WebProperties，在还原中我们需要将保存的properties按照固定的
        /// 类型还原到Propertes当中。SPWeb.AllProperties仅支持4种类型，bool/String/Integer/DateTime，此方法输入备份出来的MetaDataString，返回以
        /// Propety Name为Key，Property真实类型的值为Value的Dictionary。
        /// </summary>
        /// <param name="metaString">此参数来源为AveWebMataInfo.MetaInfo.Value</param>
        /// <returns>返回以Propety Name为Key，Property真实类型的值为Value的Dictionary
        /// </returns>
        private Dictionary<String, object> GetMetaInfoWithType(String metaString)
        {
            var metaInfo = new Dictionary<String, object>();
            var tempHashTable = AveCompressedUtility.GetMetaInfoHashtable(metaString);
            foreach (DictionaryEntry pro in tempHashTable)
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
            return metaInfo;
        }
        public void RestoreUserCustomActions(List<AveUserCustomActionInfo> customActions)
        {
            AveSPUserCustomActionCollection restoreUserCustomActions = new AveSPWebUserCustomActionCollection(this);
            restoreUserCustomActions.Restore(customActions);
        }

        public void RestoreCacheListCustomActions()
        {
            try
            {
                log.Debug("proccess customaction at postaction.");
                foreach (var kv in mAveSite.MappingManager.SiteMappingManager.CustomActionCache)
                {
                    try
                    {
                        var list = this.AveWeb.Lists[kv.Key];
                        log.Debug("proccess list:{0}.", kv.Key);
                        foreach (var dsACD in kv.Value)
                        {
                            log.Debug("proccess dsACD.Key:{0}.", dsACD.Key);
                            var action = list.UserCustomActions[dsACD.Key];
                            if (action != null)
                            {
                                log.Debug("proccess action:{0}.", action.Name);
                                if (ReplaceCustomActionUrl(action))
                                {
                                    ReplaceCustomActionIDNative(action, dsACD.Value);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Restore single custom action failed in post action.list id:{0}, action id:{1}, Error:{2}", kv.Key, kv.Value, ex);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Restore list custom action failed in post action. Error:{0}", e);
            }
        }

        private void ReplaceCustomActionIDNative(IAveUserCustomAction action,Guid newId)
        {
            if (this.ParentSite.QueryService != null && newId != Guid.Empty)
            {
                this.ParentSite.QueryService.ReplaceCustomActionId(this.ParentSite.SPSite.ID, this.SPWeb.ID, action.RegistrationId, action.Id, newId);
            }
        }

        private bool ReplaceCustomActionUrl(IAveUserCustomAction action)
        {
            Regex reg = new Regex("[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.IgnoreCase);
            Match result = reg.Match(action.Url);
            if (!string.IsNullOrEmpty(result.Value))
            {
                Guid newId = Guid.Empty;
                var sourceId = new Guid(result.Value);
                if (this.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromWorkflowIdMapping(sourceId, out newId) && sourceId != newId)
                {
                    action.Url = reg.Replace(action.Url, newId.ToString());
                    action.Update();
                    log.Debug("Replace customaction url in postaction. Url is:{2}", action.Url);
                    return true;
                }
            }
            return false;
        }

        #region IAveSPWeb Members
        IAveSPContentTypeCollection IAveSPWeb.ContentTypes
        {
            get { return mContentTypes; }
        }

        IAveObjectFeature IAveSPWeb.Feature
        {
            get { return mFeature; }
        }

        IAveSPFieldCollection IAveSPWeb.Fields
        {
            get { return mFields; }
        }

        IAveSPMembers IAveSPWeb.Members
        {
            get { return mMembers; }
        }

        IAveSPNavigation IAveSPWeb.Navigation
        {
            get { return mNavigation; }
        }

        IAveSPSite IAveSPWeb.ParentSite
        {
            get { return mAveSite; }
        }

        IAveObjectSecurity IAveSPWeb.Security
        {
            get { return Security; }
        }

        public bool HasNintexWF { get; set; }

        #endregion

        public void UpdateNintexWorkflow()
        {
            var nintexWorkflowInventoryUpgrade = mAveSite.ObjectModelFactory.CreateWorkflowInventoryUpgrade();
            if (nintexWorkflowInventoryUpgrade != null)
            {
                nintexWorkflowInventoryUpgrade.UpgradeWeb(this.AveWeb);
            }
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


        public void AddtoWFEnableCache(Guid listId, Guid definationId, bool enable)
        {
            lock (workflowEnableCache)
            {
                Dictionary<Guid, WrokflowEnableModel> wf = null;
                if (workflowEnableCache.TryGetValue(listId, out wf))
                {
                    WrokflowEnableModel model = null;
                    if (!wf.TryGetValue(definationId, out model))
                    {
                        wf[definationId] = new WrokflowEnableModel() { definationId = definationId, enable = enable };
                    }
                }
                else
                {
                    workflowEnableCache[listId] = new Dictionary<Guid, WrokflowEnableModel> { { definationId, new WrokflowEnableModel() { definationId = definationId, enable = enable } } };
                }
            }
        }

        public void RestoreWFEnable()
        {
            lock (workflowEnableCache)
            {
                try
                {
                    foreach (var listwfCache in workflowEnableCache)
                    {
                        var list = this.SPWeb.Lists[listwfCache.Key];
                        foreach (var wfObject in listwfCache.Value)
                        {
                            var workflow = list.WorkflowAssociations[wfObject.Key];
                            if (workflow != null)
                            {
                                workflow.Enabled = wfObject.Value.enable;
                                list.WorkflowAssociations.Update(workflow);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restore workflow enable option. error: {0}", e);
                }
            }
        }
    }

    //internal class AveSPWebV1 : AveSPWeb, ISPWebImport
    //{
    //    private AveSPSiteV1 parentSite;

    //    private WebSourceInfo webCacheInfo = new WebSourceInfo();

    //    internal AveSPSiteV1 ParentSPSiteV1 { get { return parentSite; } }

    //    public AveSPWebV1(AveSPSiteV1 site, string url)
    //        : base(site, url)
    //    {
    //        parentSite = site;
    //        var web = AveWeb;
    //    }

    //    /// <summary>
    //    /// Restore Web
    //    /// 
    //    /// 这个是新加的接口,外围请暂时不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spWebRestoreOption"></param>
    //    /// <returns></returns>
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPWebRestoreOption spWebRestoreOption)
    //    {
    //        var profiler = new AvePoint.Wrapper.Restore.Core.DefaultRestoreWebProfiler();

    //        Restore(restoreStream, spWebRestoreOption, profiler);

    //        return profiler.GenerateReport();
    //    }

    //    private Action<IAveRestoreStream, SPWebRestoreOption, AveMetadata, ISPWebImportProfiler> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<IAveRestoreStream, SPWebRestoreOption, AveMetadata, ISPWebImportProfiler> action = null;

    //        switch (metadataType)
    //        {
    //            case AveMetadataType.WebBasicInfo:
    //                action = RestoreWebBasicInfo;
    //                break;
    //            case AveMetadataType.WebProperty:
    //                action = RestoreWebProperty;
    //                break;
    //            case AveMetadataType.WebField:
    //                action = RestoreWebField;
    //                break;
    //            case AveMetadataType.WebContentType:
    //                action = RestoreWebContentType;
    //                break;
    //            case AveMetadataType.Navigation:
    //                action = RestoreWebNavigation;
    //                break;
    //            case AveMetadataType.WebFeature:
    //                action = RestoreWebFeature;
    //                break;
    //            case AveMetadataType.Users:
    //                action = RestoreWebUsers;
    //                break;
    //            case AveMetadataType.Groups:
    //                action = RestoreWebGroups;
    //                break;
    //            case AveMetadataType.Roles:
    //                action = RestoreWebRoles;
    //                break;
    //            case AveMetadataType.RoleAssignment:
    //                action = RestoreWebRoleAssignment;
    //                break;
    //            case AveMetadataType.RoleAssignmentsDto:
    //                action = RestoreWebRoleAssignmentDto;
    //                break;
    //            case AveMetadataType.WebEventReceiver:
    //                action = RestoreWebEventReceiver;
    //                break;
    //            case AveMetadataType.LanguageFile:
    //                action = RestoreWebLanguageFile;
    //                break;
    //            case AveMetadataType.SiteSearchInfo:
    //                action = RestoreWebSearchInfo;
    //                break;
    //            case AveMetadataType.DocumentTagging:
    //                action = RestoreWebDocumentTagging;
    //                break;
    //            case AveMetadataType.SocialTag:
    //                action = RestoreWebSocialTag;
    //                break;
    //            case AveMetadataType.SocialComment:
    //                action = RestoreWebSocialComment;
    //                break;
    //            case AveMetadataType.WebCTWorkflowAssociation:
    //                action = RestoreWebCTWorkflowAssociation;
    //                break;
    //            case AveMetadataType.WebWorkflowAssociation:
    //                action = RestoreWebWorkflowAssociation;
    //                break;
    //            case AveMetadataType.WebWorkflowInstance:
    //                action = RestoreWebWorkflowInstance;
    //                break;
    //            case AveMetadataType.WebWorkflowSchedule:
    //                action = RestoreWebWorkflowSchedule;
    //                break;
    //            case AveMetadataType.WorkflowTemplate:
    //                action = RestoreWebWorkflowTemplate;
    //                break;
    //            case AveMetadataType.MetadataService:
    //                action = RestoreWebMetadataService;
    //                break;
    //            case AveMetadataType.WebProjectPolicy:
    //                action = RestoreWebProjectPolicy;
    //                break;
    //            case AveMetadataType.SocialDto:
    //                action = RestoreWebSocialDto;
    //                break;
    //        }

    //        return action;
    //    }

    //    public void Restore(IAveRestoreStream restoreStream, SPWebRestoreOption spWebRestoreOption, ISPWebImportProfiler profiler)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }

    //        if (spWebRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spWebRestoreOption");
    //        }


    //        try
    //        {
    //            if (profiler != null) { profiler.BeginRestore(); }

    //            while (true)
    //            {
    //                if (!NeedContinue)
    //                {
    //                    log.Error("This Web need to skip. Web url:{0}.", this.webCacheInfo.Url);
    //                    return;
    //                }

    //                var metadata = restoreStream.ReadMetadata();

    //                if (metadata == null)
    //                {
    //                    break;
    //                }

    //                var metadataType = metadata.MetadataType;

    //                var action = GetAction(metadata.MetadataType);

    //                if (action != null)
    //                {
    //                    try
    //                    {
    //                        if (profiler != null) { profiler.BeginRestoreMetadata(metadataType); }

    //                        action(restoreStream, spWebRestoreOption, metadata, profiler);
    //                    }
    //                    finally
    //                    {
    //                        if (profiler != null) { profiler.EndRestoreMetadata(metadataType); }
    //                    }
    //                }
    //                else
    //                {
    //                    log.Warn(
    //                        WrapperResource.GetString(WrapperResourceKey.Wrapper_NoAvailableActionAccordingToType,
    //                                                  metadataType));
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error(WrapperResource.GetString(WrapperResourceKey.Wrapper_RestoreFailed, ex));
    //            throw;
    //        }
    //        finally
    //        {
    //            if (profiler != null) { profiler.EndRestore(); }
    //        }
    //    }

    //    private void EnsureConfigurationOption(SPWebRestoreOption option)
    //    {
    //        if (option.ConfigurationRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.ConfigurationRestoreOption");
    //        }
    //    }

    //    private void EnsureSecurityOption(SPWebRestoreOption option)
    //    {
    //        if (option.SecurityRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.SecurityRestoreOption");
    //        }
    //    }

    //    private void EnsureWFAssociationOption(SPWebRestoreOption option)
    //    {
    //        if (option.WorkflowRestoreOption == null || option.WorkflowRestoreOption.AssociationRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.WorkflowRestoreOption.AssociationRestoreOption");
    //        }
    //    }

    //    private void EnsureWFInstanceOption(SPWebRestoreOption option)
    //    {
    //        if (option.WorkflowRestoreOption == null || option.WorkflowRestoreOption.InstanceRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.WorkflowRestoreOption.InstanceRestoreOption");
    //        }
    //    }

    //    private void EnsureMMSOption(SPWebRestoreOption option)
    //    {
    //        if (option.ManagedMetadataOption == null)
    //        {
    //            throw new ArgumentNullException("option.ManagedMetadataOption");
    //        }
    //    }

    //    private void EnsureNavigationOption(SPWebRestoreOption option)
    //    {
    //        if (option.NavigationRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.NavigationRestoreOption");
    //        }
    //    }

    //    private void RestoreWebBasicInfo(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        try
    //        {
    //            if (option.RestoreAction == SPContainerRestoreAction.Replace)
    //            {
    //                if (AveWeb.Exists)
    //                {
    //                    DeleteWeb(option);
    //                }
    //                this.RestoreOption.SetRequestOption(false, false, (int)AveRestoreMode.Replace);
    //            }
    //            else if (option.RestoreAction != SPContainerRestoreAction.None)
    //            {
    //                this.RestoreOption.SetRequestOption(false, false, (int)AveRestoreMode.Default);
    //            }

    //            if (option.ConflictCheckOption == SPWebConflictCheckOption.CheckRecycleBin)
    //            {
    //                RestoringWeb.IsIncludingRecycleBinData = true;
    //            }

    //            var webBaseInfo = metadata.GetMetadata<AveWebInfo>();

    //            if (option.BeforeBasicInfoAction != null)
    //            {
    //                option.BeforeBasicInfoAction(webBaseInfo);
    //            }

    //            webCacheInfo.SetAveWebInfoValue(webBaseInfo);

    //            if (parentSite.LanguageMappingController != null)
    //            {
    //                var lcid = parentSite.LanguageMappingController.GetMappingLCID(webBaseInfo.LCID);
    //                if (lcid != webBaseInfo.LCID)
    //                {
    //                    SetLanguageForNew(lcid);
    //                }
    //            }

    //            RestoreWebSelf(webBaseInfo);

    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            if (option.AfterBasicInfoAction != null)
    //            {
    //                var afterInfo = new AveWebRestoreBasicInfo();

    //                afterInfo.Status = RestoreObjectResult.Exist;
    //                if (IsNewCreated)
    //                {
    //                    afterInfo.Status = RestoreObjectResult.NewCreated;
    //                }
    //                else if (!NeedContinue)
    //                {
    //                    afterInfo.Status = RestoreObjectResult.Skipped;
    //                }
    //                option.AfterBasicInfoAction(afterInfo);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //            log.Error("Restore web basic info for {0} failed:{1}", mName, ex);
    //            throw;
    //        }
    //    }

    //    private void DeleteWeb(SPWebRestoreOption option)
    //    {
    //        if ((!parentSite.IsNewCreated) && (!mName.Equals(AveConstants.ROOT_WEB, StringComparison.OrdinalIgnoreCase)))
    //        {
    //            try
    //            {
    //                using (var web = parentSite.SPSite.OpenWeb(mName))
    //                {
    //                    if (web.Exists && (!web.IsRootWeb))
    //                    {
    //                        DeleteWeb(web);
    //                        if (option.WebDeleted != null)
    //                        {
    //                            option.WebDeleted();
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Warn("Delete web according to name:{0} failed:{1}", mName, ex);
    //            }
    //        }
    //        else
    //        {
    //            log.Info("ignore the deletion operation because of the web name:{0} and site created value:{1}", mName, parentSite.IsNewCreated);
    //        }
    //    }

    //    private void DeleteWeb(IAveWeb web)
    //    {
    //        try
    //        {
    //            foreach (var subWeb in web.Webs)
    //            {
    //                using (subWeb)
    //                {
    //                    DeleteWeb(subWeb);
    //                }
    //            }

    //            if (web.Properties.ContainsKey("BackedUp"))
    //            {
    //                web.Properties["BackedUp"] = "true";
    //            }
    //            else
    //            {
    //                web.Properties.Add("BackedUp", "true");
    //            }
    //            web.Properties.Update();
    //            web.Delete();
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Warn("Delete web:{0} failed:{1}", web.Url, ex);
    //        }
    //    }

    //    private void RestoreWebProperty(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            RestoreWebProperty(metadata.GetMetadata<AveWebSettingInfo>());
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            //TODO report.Details.AnalyzeReport(GetReport());
    //        }
    //        else
    //        {
    //            this.WebSettingInfo = metadata.GetMetadata<AveWebSettingInfo>();
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private bool NeedRestore(SPWebRestoreOption option)
    //    {
    //        return IsNewCreated || option.RestoreAction != SPContainerRestoreAction.Skip;
    //    }

    //    private void RestoreWebField(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if ((NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration) || option.ConfigurationRestoreOption.FieldRestoreAction == SPObjectRestoreAction.Cache)
    //        {
    //            var fieldXml = string.Empty;

    //            if (option.ConfigurationRestoreOption.FieldRestoreAction != SPObjectRestoreAction.Skip)
    //            {
    //                fieldXml = metadata.GetMetadata<string>();
    //                if (option.ConfigurationRestoreOption.ProcessFieldAction != null)
    //                {
    //                    fieldXml = option.ConfigurationRestoreOption.ProcessFieldAction(fieldXml);
    //                }
    //            }

    //            //Convert Mapping
    //            if (FieldMapping != null)
    //            {
    //                var ctMetaData = restoreStream.TryReadMetadata(AveMetadataType.WebContentType);
    //                if (ctMetaData != null)
    //                {
    //                    AveContentTypeCollectionInfo webCTCollectionInfo = ctMetaData.GetMetadata<AveContentTypeCollectionInfo>();
    //                    MappingWebInfo = new AveMappingSourceSPWebInfo(webCacheInfo.ToAveWebInfo(), webCTCollectionInfo);
    //                    var customMapping = FieldMapping.ToIAveFieldMapping(MappingWebInfo);
    //                    if (customMapping != null)
    //                    {
    //                        this.Fields.FieldMapping.SetCustomMapping(customMapping);
    //                    }
    //                }
    //            }

    //            switch (option.ConfigurationRestoreOption.FieldRestoreAction)
    //            {
    //                case SPObjectRestoreAction.Restore:
    //                    this.Fields.RestoreFields(fieldXml, option.ConfigurationRestoreOption.FieldRestoreOption);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //TODO report.Details.AnalyzeReport(this.Fields.GetReport());
    //                    break;
    //                case SPObjectRestoreAction.Cache:
    //                    this.Fields.LoadFields(fieldXml);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //                default:
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreWebContentType(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if ((NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration) || option.ConfigurationRestoreOption.ContentTypeRestoreAction == SPObjectRestoreAction.Cache)
    //        {
    //            AveContentTypeCollectionInfo info = null;

    //            if (option.ConfigurationRestoreOption.ContentTypeRestoreAction != SPObjectRestoreAction.Skip)
    //            {
    //                info = metadata.GetMetadata<AveContentTypeCollectionInfo>();

    //                if (option.ConfigurationRestoreOption.ProcessContentTypeAction != null)
    //                {
    //                    option.ConfigurationRestoreOption.ProcessContentTypeAction(info);
    //                }

    //                //Convert Mapping
    //                if (ContentTypeMapping != null)
    //                {
    //                    if (MappingWebInfo == null)
    //                    {
    //                        MappingWebInfo = new AveMappingSourceSPWebInfo(webCacheInfo.ToAveWebInfo(), info);
    //                    }
    //                    var customMapping = ContentTypeMapping.ToIAveContentTypeMapping(MappingWebInfo);
    //                    if (customMapping != null)
    //                    {
    //                        this.ContentTypes.ContentTypeMapping.SetCustomMapping(customMapping);
    //                    }
    //                }
    //            }

    //            switch (option.ConfigurationRestoreOption.ContentTypeRestoreAction)
    //            {
    //                case SPObjectRestoreAction.Restore:
    //                    this.ContentTypes.RestoreContentTypes(info, option.ConfigurationRestoreOption.ContentTypeNameMapping, option.ConfigurationRestoreOption.ContentTypeRestoreOption);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //TODO report.Details.AnalyzeReport(this.ContentTypes.GetReport());
    //                    this.UpdateDocumentSetCT();
    //                    break;
    //                case SPObjectRestoreAction.Cache:
    //                    this.ContentTypes.LoadContentTypes(info);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //                default:
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreWebNavigation(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureNavigationOption(option);

    //        if (NeedRestore(option) && option.NavigationRestoreOption.NeedRestoreNavigation)
    //        {
    //            var navigationInfoList = metadata.GetMetadata<AveNavigationInfoList>();

    //            parentSite.NavigationRestoreSetting.ForceKeepNode = option.NavigationRestoreOption.ForceKeepInvalidNode;
    //            parentSite.NavigationRestoreSetting.NavigationPromoteRestoreSettings = option.NavigationRestoreOption.IsMoveInheritNavigationNode ? NavigationPromoteRestoreSetting.MoveBoth : NavigationPromoteRestoreSetting.None;
    //            using (var navManager = new AveSPNavigation(this))
    //            {
    //                navManager.AddToNavNodesCache(navigationInfoList);
    //            }
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            //TODO report.Details.AnalyzeReport(GetReport());
    //        }
    //    }

    //    private void RestoreWebFeature(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            try
    //            {
    //                using (var featureManager = new AveSPFeature(this))
    //                {
    //                    featureManager.Restore(metadata.GetMetadata<AveFeatureInfoBox>());
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //TODO report.Details.AnalyzeReport(featureManager.GetReport());
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Error("Restore Web Feature failed:{0}", ex);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //            }
    //        }
    //    }

    //    private void RestoreWebUsers(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option))
    //        {
    //            parentSite.RestoreSiteUsers(option.SecurityRestoreOption, metadata, profiler);
    //        }
    //    }

    //    private void RestoreWebGroups(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option))
    //        {
    //            parentSite.RestoreSiteGroups(option.SecurityRestoreOption, metadata, profiler);
    //        }
    //    }

    //    private void RestoreWebRoles(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option) && option.SecurityRestoreOption.RestorePermissionLevel)
    //        {
    //            this.RestorePermissionLevel = option.SecurityRestoreOption.RestorePermissionLevel;

    //            var roles = metadata.GetMetadata<List<AveRoleInfo>>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                security.RestoreRoles(roles, option.SecurityRestoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //        else
    //        {
    //            this.Roles = metadata.GetMetadata<List<AveRoleInfo>>();
    //        }
    //    }

    //    private void RestoreWebRoleAssignment(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option) && option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments = option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments);
    //                }

    //                if (this.WebSettingInfo != null && this.WebSettingInfo.HasUniqueRoleAssignments != null && this.WebSettingInfo.HasUniqueRoleAssignments.IsAvailable)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = this.WebSettingInfo.HasUniqueRoleAssignments.Value;
    //                }
    //                security.RestoreRoleAssignments(roleAssignments, option.SecurityRestoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreWebRoleAssignmentDto(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            var roleAssignments = metadata.GetMetadata<AvePoint.Wrapper.Core.SPBackupDto.SPRoleAssignmentsDto>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments.RoleAssignmentInfos = option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments.RoleAssignmentInfos);
    //                }

    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = !roleAssignments.IsInherit;
    //                }

    //                security.ParentSite.RestoreUser(roleAssignments.UserCache);
    //                security.ParentSite.RestoreGroup(roleAssignments.GroupCache);

    //                security.RestoreRoleAssignments(roleAssignments.RoleAssignmentInfos, option.SecurityRestoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreWebEventReceiver(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            var eventReceivers = metadata.GetMetadata<List<AveEventReceiverInfo>>();
    //            using (var eventReceiverManager = AveSPEventReceiver.CreateInstance(this))
    //            {
    //                eventReceiverManager.RestoreEventReceivers(eventReceivers);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(eventReceiverManager.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreWebLanguageFile(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        if (NeedRestore(option))
    //        {
    //            this.parentSite.RestoreLanguageFile(this.WebSrcLanguageId, this.SPWeb.Language, metadata, profiler);
    //        }
    //    }

    //    private void RestoreWebSearchInfo(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            var searchInfo = metadata.GetMetadata<AveSearchInfo>();
    //            if (searchInfo != null)
    //            {
    //                using (var searchManager = new AveSPSearch(this))
    //                {
    //                    searchManager.Restore(searchInfo);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //TODO report.Details.AnalyzeReport(searchManager.GetReport());
    //                }
    //            }
    //        }
    //    }

    //    private void RestoreWebDocumentTagging(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            using (var documentTagging = new AveDocumentTagging(SPWeb.Url + "/", ParentSite))
    //            {
    //                var documentTags = metadata.GetMetadata<List<AveDocumentTaggingInfo>>();

    //                documentTagging.Restore(documentTags);

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(documentTagging.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreWebSocialTag(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            using (var socialTag = new AveSPSocialTag(SPWeb.Url + "/", ParentSite))
    //            {
    //                var socialTags = metadata.GetMetadata<List<AveSocialTagInfo>>();

    //                socialTag.Restore(socialTags);

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(socialTag.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreWebSocialComment(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            using (var socialComment = new AveSPSocialComment(SPWeb.Url + "/", ParentSite))
    //            {
    //                var socialComments = metadata.GetMetadata<List<AveSocialCommentInfo>>();

    //                socialComment.Restore(socialComments);

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(socialComment.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreWebCTWorkflowAssociation(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureWFAssociationOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction != SPObjectRestoreAction.Skip)
    //        {
    //            var ctWFInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var ctWFResolution = WFConflictResolution.Instance;
    //            ctWFResolution.AssociationOption = (WFAssociationConflictResolutionOption)option.WorkflowRestoreOption.AssociationRestoreOption.ConflictResolutionOption;
    //            SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //            SPWorkflowProcessorRuntime.TemplateFileConflictRules = (TemplateFileConflictRulesEnum)option.WorkflowRestoreOption.AssociationRestoreOption.TemplateFileConflictRules;
    //            foreach (AveWorkflowInfo unit in ctWFInfo)
    //            {
    //                if (option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction == SPObjectRestoreAction.Restore)
    //                {
    //                    string contentTypeId;
    //                    if ((contentTypeId = ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeId(unit.CTId)) != null)
    //                    {
    //                        var ct = SPWeb.ContentTypes[parentSite.ObjectModelFactory.CreateContentTypeId(contentTypeId)];
    //                        if (ct != null)
    //                        {
    //                            unit.CTName = ct.Name;
    //                        }
    //                        else
    //                        {
    //                            ct = SPWeb.ContentTypes[unit.CTName];
    //                        }
    //                        ctWFResolution.RestoreAssociationData(unit, this, ct);
    //                    }
    //                }
    //                else
    //                {
    //                    ctWFResolution.CacheAssociationData(unit);
    //                }
    //            }
    //            using (var workflowReport = ctWFResolution.GetReport())
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(workflowprofiler);
    //            }
    //            ctWFResolution.WebContentTypeAssociation = false;
    //        }
    //    }

    //    private void RestoreWebWorkflowAssociation(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureWFAssociationOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction != SPObjectRestoreAction.Skip)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;
    //            wfResolution.AssociationOption = (WFAssociationConflictResolutionOption)option.WorkflowRestoreOption.AssociationRestoreOption.ConflictResolutionOption;
    //            SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //            SPWorkflowProcessorRuntime.TemplateFileConflictRules = (TemplateFileConflictRulesEnum)option.WorkflowRestoreOption.AssociationRestoreOption.TemplateFileConflictRules;
    //            foreach (AveWorkflowInfo unit in wfInfo)
    //            {
    //                if (option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction == SPObjectRestoreAction.Restore)
    //                {
    //                    wfResolution.RestoreAssociationData(unit, this);
    //                }
    //                else
    //                {
    //                    wfResolution.CacheAssociationData(unit);
    //                }
    //            }
    //            using (var workflowReport = wfResolution.GetReport())
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(workflowprofiler);
    //            }
    //        }
    //    }

    //    private void RestoreWebWorkflowInstance(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureWFInstanceOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;
    //            wfResolution.InstanceOption = (WFInstanceConflictResolutionOption)option.WorkflowRestoreOption.InstanceRestoreOption.ConflictResolutionOption;
    //            option.WorkflowRestoreOption.InstanceRestoreOption.ToWFInstanceSetting();
    //            foreach (AveWorkflowInfo unit in wfInfo)
    //            {
    //                wfResolution.RestoreInstanceData(unit, this);
    //            }
    //            using (var workflowReport = wfResolution.GetReport())
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(workflowprofiler);
    //            }
    //        }
    //    }

    //    private void RestoreWebWorkflowSchedule(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureWFInstanceOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;
    //            wfResolution.AssociationParentObject = SPWeb;
    //            SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //            foreach (AveWorkflowInfo unit in wfInfo)
    //            {
    //                wfResolution.RestoreScheduleData(unit, SPWeb);
    //            }
    //            using (var workflowReport = wfResolution.GetReport())
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(workflowprofiler);
    //            }
    //        }
    //    }

    //    private void RestoreWebWorkflowTemplate(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureWFInstanceOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;
    //            wfResolution.AssociationParentObject = SPWeb;
    //            foreach (AveWorkflowInfo unit in wfInfo)
    //            {
    //                wfResolution.RestoreNintexWorkflowTemplates(unit, SPWeb);
    //            }
    //            using (var workflowReport = wfResolution.GetReport())
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(workflowprofiler);
    //            }
    //        }
    //    }

    //    private void RestoreWebMetadataService(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureMMSOption(option);
    //        if (NeedRestore(option))
    //        {
    //            this.parentSite.RestoreMetadataService(option.ManagedMetadataOption, metadata, profiler);
    //        }
    //    }

    //    private void RestoreWebProjectPolicy(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            if (parentSite.SPContextKind.IsServerMode13Upper() && AveEnv.IsMoss)
    //            {
    //                var policyInfo = metadata.GetMetadata<AveProjectPolicyInfo>();
    //                var utility = ((AveObjectModelFactoryExtension)parentSite.ObjectModelFactory).CreatePolicyItemListUtility();
    //                utility.SetObjectData(parentSite.AveSite.ID, AveWeb.ID, policyInfo);
    //            }
    //        }
    //    }

    //    private void RestoreWebSocialDto(IAveRestoreStream restoreStream, SPWebRestoreOption option, AveMetadata metadata, ISPWebImportProfiler profiler)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IFieldMapping FieldMapping { get; set; }

    //    public IContentTypeMapping ContentTypeMapping { get; set; }

    //    internal AveMappingSourceSPWebInfo MappingWebInfo { get; set; }
    //    private Dictionary<String, object> GetMetaInfoWithType(String metaString)
    //    {
    //        var metaInfo = new Dictionary<String, object>();
    //        var tempHashTable = AveCompressedUtility.GetMetaInfoHashtable(metaString);
    //        foreach (DictionaryEntry pro in tempHashTable)
    //        {
    //            switch ((pro.Value as MetaInfoProperty).Type)
    //            {
    //                case MetaInfoValueType.Boolean:
    //                    {
    //                        metaInfo[pro.Key.ToString()] = Convert.ToBoolean((pro.Value as MetaInfoProperty).Value);
    //                        break;
    //                    }
    //                case MetaInfoValueType.Integer:
    //                    {
    //                        metaInfo[pro.Key.ToString()] = Convert.ToInt32((pro.Value as MetaInfoProperty).Value);
    //                        break;
    //                    }
    //                case MetaInfoValueType.Time:
    //                    {
    //                        metaInfo[pro.Key.ToString()] = Convert.ToDateTime((pro.Value as MetaInfoProperty).Value);
    //                        break;
    //                    }
    //                default:
    //                    {
    //                        metaInfo[pro.Key.ToString()] = (pro.Value as MetaInfoProperty).Value.ToString();
    //                        break;
    //                    }
    //            }

    //        }
    //        return metaInfo;
    //    }
    //}

    internal class WebSourceInfo
    {
        //AveWebInfo
        public string Url { get; set; }
        public string WebTemplate { get; set; }

        internal void SetAveWebInfoValue(AveWebInfo webBaseInfo)
        {
            this.Url = webBaseInfo.Url;
            this.WebTemplate = webBaseInfo.WebTemplate;
        }

        internal AveWebInfo ToAveWebInfo()
        {
            return new AveWebInfo()
            {
                Url = this.Url,
                WebTemplate = this.WebTemplate
            };
        }

    }

    internal class WrokflowEnableModel
    {
        public Guid definationId;

        public bool enable;
    }

}