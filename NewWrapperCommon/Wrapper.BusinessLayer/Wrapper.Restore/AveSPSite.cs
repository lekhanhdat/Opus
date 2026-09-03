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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.SPService;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Restore.NintexForm;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/04/20", "Yuzhi.Jiang@AvePoint.com", "Yongqiang.Zhou@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]

    public class AveSPSite : RestoreableObject, IDisposable, AvePoint.Wrapper.Restore.IAveSPSite
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal WrapperBusinessBehaviorController BusinessBehavior { get; private set; }
        private IAveBackupRestoreQueryService mQueryService;
        private string mSiteUrl;
        private AveMetadataService mMetadataService;
        private IAveWebApplication mWebApplication = null;
        protected IAveSite mSPSite = null;
        private AveMappingManager mMappingManager = new AveMappingManager();
        private bool mIsNewCreated = false;
        private uint mLanguageForNewCreated = 0;
        private bool mUseHostHeader = false;
        private Guid mContentDBId = Guid.Empty;
        private bool mCreationAccountResetted = false;
        protected AveLanguageProcesser mAveLanguageProcesser = null;
        protected AveSPMembers mSPMembers;
        internal DateTime mSPRequestTimeout = DateTime.UtcNow;
        internal int mHoursReloadSite = 12;
        public AveSiteSettingInfo SourceSiteSettingInfo { get; set; }
        private AveServiceContext mServiceContext = null;
        private AveRestoreGhostPageOption m_SaveBinaryForGhostPage = AveRestoreGhostPageOption.NoAction;
        private bool mSetLookupFieldSourceValue = false;
        private AveRBSRestore mRestore;
        private IAveTemplateMapping mTemplateMapping;
        private NavigationRestoreSetting navigationRestoreSetting = new NavigationRestoreSetting();
        //<WebId,<ListId,<ItemTPGuid,ItemRowId>>>
        private Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, int>>> lookupListItemIdAndGuidCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, int>>>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<string, Dictionary<int, Guid>>>>> lookupListItemIdAndDisplayValueCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<string, Dictionary<int, Guid>>>>>();
        private List<Guid> PostReloadLookupListCache = new List<Guid>(); //保证post 只reload 一次lookup list
        public AveSiteInfo SourceSiteInfo { get; set; }
        public Dictionary<Guid, AveXmlField> xmlFieldCache = new Dictionary<Guid, AveXmlField>();

        private AveUserProfile mSiteOwnerUserProfile;
        private AveSocialFollowing mSiteOwnerSocialFollowing;
        //private AveDenyAddAndCustomizePagesStatus? siteDenyAddAndCustomizePagesStatus;
        private AveBasePermissions denyPermissionsMask = AveBasePermissions.EmptyMask;
        public WorkflowSiteCollectionCache WorkflowCache { get; set; }

        public IAveTemplateMapping TemplateMapping
        {
            get
            {
                if (mTemplateMapping == null)
                {
                    mTemplateMapping = new AveTemplateMapping();
                }
                return mTemplateMapping;
            }
        }

        public Dictionary<Guid, Dictionary<string, AveItemHoldRecord>> UnRestoreFileHoldRecordCache = new Dictionary<Guid, Dictionary<string, AveItemHoldRecord>>();
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, AveItemHoldRecord>>> UnRestoreItemHoldRecordCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, AveItemHoldRecord>>>();
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, List<string>>>> UnReplaceUrlIDCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, List<string>>>>();
        public Dictionary<Guid, DateTime> UnRestoreListLastModifiedTime = new Dictionary<Guid, DateTime>();
        private Dictionary<Guid, Dictionary<Guid, List<int>>> postUpdateSocialItems = new Dictionary<Guid, Dictionary<Guid, List<int>>> { };
        public Dictionary<Guid, Dictionary<Guid, AveComplianceTagInfo>> UnRestoreListComplianceTagProperties = new Dictionary<Guid, Dictionary<Guid, AveComplianceTagInfo>>();

        private List<string> tempMasterPages;
        public List<string> TempMasterPages
        {
            get
            {
                if (tempMasterPages == null)
                {
                    tempMasterPages = new List<string>();
                }
                return tempMasterPages;
            }
        }

        internal void AddPostUpdateSocialItem(Guid webId, Guid listId, int itemId)
        {
            log.Debug("Add social item to post action update cache. WebId:{0},ListId:{1},ItemId:{2}", webId, listId, itemId);
            if (webId == Guid.Empty || listId == Guid.Empty || itemId <= 0)
            {
                return;
            }
            //目前一个site中只有一个这种类型的list需要处理(而且只是mysite，包括local mysite和online oneDrive)，暂时不需要考虑多线程的问题
            if (!postUpdateSocialItems.ContainsKey(webId))
            {
                postUpdateSocialItems.Add(webId, new Dictionary<Guid, List<int>>());
            }
            Dictionary<Guid, List<int>> webCache = postUpdateSocialItems[webId];
            if (!webCache.ContainsKey(listId))
            {
                webCache.Add(listId, new List<int>());
            }
            if (!webCache[listId].Contains(itemId))
            {
                webCache[listId].Add(itemId);
            }
        }

        internal void RestoreAssignEmailSetting()
        {
            foreach (KeyValuePair<Guid, List<Guid>> kv in MappingManager.SiteMappingManager.GetAssignToEmailSettingmappingOnlyForPostAction())
            {
                try
                {
                    using (IAveWeb web = this.SPSite.OpenWeb(kv.Key))
                    {
                        foreach (var id in kv.Value)
                        {
                            IAveList list = web.GetList(id);
                            list.EnableAssignToEmail = true;
                            list.Update();
                            log.Debug("Process assign to email setting in post action, list url: {0}.", list.DefaultDisplayFormUrl);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occured while processing AssignToEmailSetting. ex:{0}.", e.ToString());
                }
            }
        }

        public void RestoreListComplianceTagProperties()
        {
            foreach (var webId in UnRestoreListComplianceTagProperties.Keys)
            {
                using (var web = SPSite.OpenWeb(webId))
                {
                    foreach (var propertyInfo in UnRestoreListComplianceTagProperties[webId])
                    {
                        var list = web.Lists.GetListById(propertyInfo.Key, false);
                        if (list == null)
                        {
                            log.Warn("Cannot find list {0} in web {1} while post update list compliance tag properties.", propertyInfo.Key, web.Url);
                            continue;
                        }
                        list.ComplianceTag = propertyInfo.Value;
                    }
                }
            }
        }

        public void PostUpdateSocialItems()
        {
            if (postUpdateSocialItems == null || postUpdateSocialItems.Count == 0)
            {
                return;
            }
            foreach (var webId in postUpdateSocialItems.Keys)
            {
                try
                {
                    using (IAveWeb parentWeb = SPSite.OpenWeb(webId))
                    {
                        foreach (var listId in postUpdateSocialItems[webId].Keys)
                        {
                            IAveList parentList = parentWeb.Lists.GetListById(listId, false);
                            if (parentList == null)
                            {
                                log.Warn("Cannot find list {0} in web {1} while post update social items.", listId, parentWeb.Url);
                                continue;
                            }
                            AveSPList.PostUpdateSocialItems(postUpdateSocialItems[webId][listId], parentList, parentWeb, MappingManager.SiteMappingManager);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while post update social items in web {0},Error:{1}.", webId, e);
                }
            }
        }
        /// <summary>
        /// Restore Nintex form in post action
        /// </summary>
        public void RestoreNintexFormInPostAction()
        {
            try
            {
                var siteMappingManager = this.MappingManager.SiteMappingManager;
                foreach (var webForms in siteMappingManager.GetNintexFormsDataFormSiteLevelCache)
                {
                    using (var web = new AveSPWeb(this, webForms.Key))
                    {
                        foreach (var listNintexForms in webForms.Value)
                        {
                            var listId = listNintexForms.Key;
                            var tmpList = web.AveWeb.Lists.GetById(listId);
                            foreach (var contentNintexFormData in listNintexForms.Value)
                            {
                                var contentTypeId = contentNintexFormData.ContentTypeId;
                                var nintexFormXmls = contentNintexFormData.NintexFormsInfo;
                                INintexFormService service = NintexFormServiceBase.CreateNintexForm(tmpList, web, true);
                                foreach (var nintexFormXml in nintexFormXmls)
                                {
                                    try
                                    {
                                        service.RestoreForm(nintexFormXml, contentTypeId);
                                        log.Info("In site post action, success to restore nintex form in content type:{0} of list:{1}.", contentTypeId, listId);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error(WrapperRestoreResource.RestoreNintexFormInPostActionFailed, contentTypeId, listId, e);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while restore nintex form in postaction, error: {0}", e);
            }
        }

        public void RestoreNintexFormValueInPostAction()
        {
            try
            {
                foreach (var webUrlCache in MappingManager.SiteMappingManager.GetNintexFormDataCache)
                {
                    using (var web = new AveSPWeb(this, webUrlCache.Key))
                    {
                        foreach (var listIdCache in webUrlCache.Value)
                        {
                            var listid = listIdCache.Key;
                            var list = web.AveWeb.Lists.GetById(listid);
                            foreach (var itemCache in listIdCache.Value)
                            {
                                var itemId = itemCache.Key;
                                //need item  id mapping
                                var item = list.GetItemById(itemId);
                                int itemUIVersion = new AveSPItem(this).GetCurrentUIVersion(this.SPSite.ID, item);
                                foreach (var formDataCache in itemCache.Value)
                                {
                                    if (formDataCache.Key == itemUIVersion)
                                    {
                                        if (item.Fields.ContainsField("NFFormData"))
                                        {
                                            item["NFFormData"] = formDataCache.Value;
                                            item["Modified"] = item["Modified"];
                                            item.SystemUpdate(false);
                                        }
                                        else
                                        {
                                            log.Warn("Can not find nintex form field by name: NFFormData");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while restore nintex form data in post action, error: {0}", e);
            }
        }
        public void ResetDenyPermissionsMask()
        {
            if (denyPermissionsMask != AveBasePermissions.EmptyMask)
            {
                try
                {
                    this.SPSite.DenyPermissionsMask = denyPermissionsMask;
                    denyPermissionsMask = AveBasePermissions.EmptyMask;
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while reset deny permissions mask in post action, error: {0}", e);
                }
            }
        }

        public Dictionary<string, string> ContentTypeIdMapping = new Dictionary<string, string>();

        public AveRestoreGhostPageOption SaveBinaryForGhostPage
        {
            get { return m_SaveBinaryForGhostPage; }
            set { m_SaveBinaryForGhostPage = value; }
        }
        public bool SetLookupFieldSourceValue
        {
            get { return mSetLookupFieldSourceValue; }
        }

        private bool autoDropOffContentOrganizer = false;
        public bool AutoDropOffContentOrganizer
        {
            get { return autoDropOffContentOrganizer; }
            set { autoDropOffContentOrganizer = value; }
        }

        private bool mNeedSkipMicroFeedList = false;
        public bool NeedSkipMicroFeedList
        {
            get { return mNeedSkipMicroFeedList; }
            set { mNeedSkipMicroFeedList = value; }
        }

        private bool mRestoreManagedMetadataNavigation = false;
        public bool RestoreManagedMetadataNavigation
        {
            get { return mRestoreManagedMetadataNavigation; }
        }

        private bool mNotRestoreWebCss = false;
        public bool NotRestoreWebCss
        {
            get { return mNotRestoreWebCss; }
            set { mNotRestoreWebCss = value; }
        }

        public AveServiceContext ServiceContext
        {
            get
            {
                if (mServiceContext == null)
                {
                    mServiceContext = new AveServiceContext(this.SPSite, mOMFactory);
                    mServiceContext.UserMap = this.SPMembers.GetMappingUserLogin;
                }

                return mServiceContext;
            }
        }

        public AveUserProfile SiteOwnerUserProfile
        {
            get
            {
                if (mSiteOwnerUserProfile == null)
                {
                    mSiteOwnerUserProfile = new AveUserProfile(ServiceContext, this.SPSite.Owner.LoginName, true, SourceSiteInfo, mSiteUrl);
                }
                return mSiteOwnerUserProfile;
            }
        }

        public AveSocialFollowing SiteOwnerSocialFollowing
        {
            get
            {
                if (mSiteOwnerSocialFollowing == null)
                {
                    mSiteOwnerSocialFollowing = new AveSocialFollowing(SiteOwnerUserProfile, ObjectModelFactory);
                }
                return mSiteOwnerSocialFollowing;
            }
        }

        #region  Services

        public AveMetadataService MetadataService
        {
            get
            {
                if (mMetadataService == null)
                {
                    mMetadataService = new AveMetadataService(this);
                }
                return mMetadataService;
            }
            set
            {
                mMetadataService = value;
            }
        }

        #endregion

        public string DefaultUser { get; set; }
        public Collection<string> webappBlockTypes = null;

        private uint mSrcLcd;
        public Guid mWebId;
        protected AveObjectModelFactory mOMFactory;

        private string mWebAppName;
        private string mRootWebRelativeUrl;
        private AveBPOSAccountInfo mAccount;

        protected string mPlaceHolderAccount = string.Empty;
        private IAveSite mCheckoutSite;
        private IAveWeb mCheckoutWeb;
        private IAvePublishing mPublishing;
        private IReport report = new AveWrapperReport();

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetPlaceHolderAccount(string login)
        {
            mPlaceHolderAccount = login;
        }
        public void SetLookupSourceValue(bool setSourceValue)
        {
            mSetLookupFieldSourceValue = setSourceValue;
        }
        public string GetPlaceHolderAccount()
        {
            return mPlaceHolderAccount;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public uint LanguageForNewCreate
        {
            get { return mLanguageForNewCreated; }
            set { mLanguageForNewCreated = value; }
        }

        public AveObjectModelFactory ObjectModelFactory
        {
            get
            {
                return mOMFactory;
            }
        }

        public AveMappingManager MappingManager
        {
            get
            {
                return mMappingManager;
            }
        }
        public AveContextKind SPContextKind
        {
            get { return this.ObjectModelFactory.ContextKind; }
        }

        public AveRBSRestore RBSRestore
        {
            get
            {
                if (mRestore == null)
                {
                    mRestore = new AveRBSRestore(this.SPSite.ID, this.QueryService);
                }
                return mRestore;
            }
        }

        public AveBPOSAccountInfo BPOSUserAccountInfo
        {
            get { return mAccount; }
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetUserMapping(Dictionary<string, string> userMapping, Dictionary<string, string> domainMapping, string defaultUser)
        {
            //MappingManager.SiteMappingManager.UserMapping = userMapping;
            //MappingManager.SiteMappingManager.DomainMapping = domainMapping;
            SPMembers.UserAndDomainMapping.SetUserAndDomainMappings(userMapping, domainMapping);
            DefaultUser = defaultUser;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetTemplateMapping(XmlElement xe)
        {
            AveTemplateMapping mTemplateMapping = new AveTemplateMapping(xe);
            this.mTemplateMapping = mTemplateMapping;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetLanguageMapping(AveLanguageProcesser languageMapping)
        {
            AveLanguageProcesser = languageMapping;
        }

        public void SetWebTemplate(Guid webId)
        {
            mWebId = webId;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public AveItemFieldFilterRule ItemFieldFilter;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeFields"></param>
        /// <param name="excludeFields"></param>
        /// <param name="mode">0:depend,includeFields || all-excludeFields; 1:including all; 2: excluding all</param>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetFieldFilter(HashSet<string> includeFields, HashSet<string> excludeFields, int mode)
        {
            if (ItemFieldFilter == null)
            {
                ItemFieldFilter = new AveItemFieldFilterRule();
            }
            ItemFieldFilter.Mode = mode;
            ItemFieldFilter.IncludeFields = includeFields;
            ItemFieldFilter.ExcludeFields = excludeFields;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetUseHostHeader(bool value)
        {
            mUseHostHeader = value;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetContentDBId(Guid id)
        {
            mContentDBId = id;
        }

        //在2014.9.18之后的包里，这个Method已经不再能够控制MMS中term属性的还原了。
        public void SetRestoreManagedMetadataNavigation(bool restoreMetadataNavigation)
        {
            mRestoreManagedMetadataNavigation = restoreMetadataNavigation;
            WrapperRuntime.CurrentContext.RestoreManagedMetadataNavigation = restoreMetadataNavigation;
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetLanguageForNew(uint LCD)
        {
            mLanguageForNewCreated = LCD;
        }

        public List<string> KpiListIdCol = new List<string>();

        public int CURRENT_USER_ID = 0;

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public AveLanguageProcesser AveLanguageProcesser
        {
            get { return mAveLanguageProcesser; }
            set
            {
                mAveLanguageProcesser = value;
                if (mAveLanguageProcesser != null)
                {
                    mAveLanguageProcesser.ContextKind = this.SPContextKind;
                }
            }
        }

        public string SiteUrl
        {
            get { return mSiteUrl; }
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public string DestinationURL
        {
            get;
            set;
        }

        public IAveSite SPSite
        {
            get { return mSPSite; }
            set { mSPSite = value; }
        }

        public IAveWebApplication SPWebApplication
        {
            get { return mWebApplication; }
            set { mWebApplication = value; }
        }

        private bool mCheckManagedPath = true;

        public bool CheckManagedPath
        {
            get { return mCheckManagedPath; }
            set { mCheckManagedPath = value; }
        }

        public uint SrcLanguageId
        {
            get { return this.mSrcLcd; }
        }
        public bool SiteReadOnly
        {
            get
            {
                if (mSPSite == null)
                {
                    return true;
                }
                return mSPSite.ReadOnly;
            }
        }
        public bool IsNewCreated
        {
            get { return mIsNewCreated; }
            ///这个函数只给Replicator使用，请不要随便使用，谢谢。
            set { mIsNewCreated = value; }
        }

        public AveSPMembers SPMembers
        {
            get { return mSPMembers; }
        }
        public bool OverWriteNavigation { get; set; }

        public NavigationRestoreSetting NavigationRestoreSetting
        {
            set { navigationRestoreSetting = value; }
            get { return navigationRestoreSetting; }
        }

        public IAvePublishing Publishing
        {
            get
            {
                if (mPublishing == null)
                {
                    try
                    {
                        mPublishing = this.mOMFactory.CreatePublishing(mSPSite);
                    }
                    catch (Exception e)
                    {
                        log.Warn("CreatePublishing error:{0}", e.ToString());
                    }
                }
                return mPublishing;
            }
        }

        private bool isSitePostRestoreWebPart = false;
        public bool IsSitePostRestore
        {
            get { return isSitePostRestoreWebPart; }
        }

        public bool KeepDestItemRowId { get; set; }
        /// <summary>
        /// the option that determines if we keep the default value, when creating/updating a list item
        /// </summary>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        public bool KeepDefaultValue { get; set; }
        /// <summary>
        /// the option that determines if we should verify metadata column value before restoring a list item
        /// </summary>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        public bool VerifyItemMMSColumnValue { get; set; }
        /// <summary>
        /// only for replicator to save some data to do post action.
        /// </summary>
        public AveSPSite(string _url, string parentFullPath, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            Init(_url, parentFullPath, contextKind, aveUserAccountInfo);
            mWebAppName = parentFullPath;
            mSPSite = mOMFactory.CreateSite(_url);
            log.Debug("Current user: {0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        public AveSPSite(string _url, string parentFullPath, AveSqlConnection _sqlConn, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            Init(_url, parentFullPath, contextKind, aveUserAccountInfo);
            log.Debug("Current user: {0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        private void Init(string _url, string parentFullPath, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            WrapperRuntime.CurrentContext.MappingManager.Clear();
            WrapperRuntime.CurrentContext.MappingManager = mMappingManager;
            mSiteUrl = _url;
            mSPMembers = new AveSPMembers(this);
            mAccount = aveUserAccountInfo;
            mOMFactory = AveObjectModelFactory.CreateObjectModelFactory(parentFullPath, mAccount, contextKind);
            this.BusinessBehavior = new WrapperBusinessBehaviorController(this.SPContextKind);
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void SetSiteCreationAccount(string ownerlogin, AveSiteInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.SetSiteCreationAccount"))
            {

                if (!string.IsNullOrEmpty(ownerlogin) && info != null)
                {
                    string oldLogin = info.OwnerLogin;
                    info.OwnerName = ownerlogin;
                    info.OwnerLogin = ownerlogin;
                    info.SecondaryContactLogin = null;
                    mCreationAccountResetted = true;
                    log.Info("Replace siteCollection owner. {0} to {1}", oldLogin, info.OwnerLogin);
                }

            }

        }

        private void InitializeMembers()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.InitializeMembers"))
            {

                if (CURRENT_USER_ID <= 0)
                {
                    try
                    {
                        if (mSPSite.RootWeb.CurrentUser == null)
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotFindCurrentUser);
                        }
                        else
                        {
                            CURRENT_USER_ID = mSPSite.RootWeb.CurrentUser.ID;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Find root web current user error, use site Owner instead error. \n error message:{0}", e));
                        //mLog.Error("Find root web current user error, use site Owner instead Error: " + e.ToString());
                        CURRENT_USER_ID = mSPSite.Owner.ID;
                    }
                }
                if (mQueryService == null && this.SPContextKind.IsServerMode())
                {
                    mQueryService = ObjectModelFactory.CreateQueryService<IAveBackupRestoreQueryService>(mSPSite);
                }
            }

        }

        /// <summary>
        /// 修改当owner和secondary其中有一个为不存在用户时会把两个都转换成agent account的问题
        /// </summary>
        /// <param name="siteInfo"></param>
        /// <param name="webApp"></param>
        private void ReplaceInvalidUser(AveSiteInfo siteInfo, IAveWebApplication webApp)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.ReplaceInvalidUser"))
            {

                string loginName = Environment.UserDomainName + "\\" + Environment.UserName;

                IAvePrincipalInfo ownerInfo = mOMFactory.Utility.ResolvePrincipal(webApp, null, siteInfo.OwnerLogin, AvePrincipalType.User, AvePrincipalSource.Windows, false);
                if (ownerInfo == null)
                {
                    ownerInfo = mOMFactory.Utility.ResolvePrincipal(webApp, null, siteInfo.OwnerLogin, AvePrincipalType.User, AvePrincipalSource.MembershipProvider, false);
                    if (ownerInfo == null)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Site owner: {0}", siteInfo.OwnerLogin));

                        siteInfo.OwnerLogin = loginName;
                        siteInfo.OwnerName = Environment.UserName;
                    }
                }

                if (!string.IsNullOrEmpty(siteInfo.SecondaryContactLogin))
                {
                    IAvePrincipalInfo secondaryInfo = mOMFactory.Utility.ResolvePrincipal(webApp, null, siteInfo.SecondaryContactLogin, AvePrincipalType.User, AvePrincipalSource.Windows, false);
                    if (secondaryInfo == null)
                    {
                        secondaryInfo = mOMFactory.Utility.ResolvePrincipal(webApp, null, siteInfo.SecondaryContactLogin, AvePrincipalType.User, AvePrincipalSource.MembershipProvider, false);
                        if (secondaryInfo == null)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Site Secondary owner: {0}", siteInfo.SecondaryContactLogin));

                            siteInfo.SecondaryContactLogin = loginName;
                            siteInfo.SecondaryContactName = Environment.UserName;
                        }
                    }
                }

            }

        }

        private void CreateNewSPSite(string siteUrl, AveSiteInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.CreateNewSPSite"))
            {

                //IAveWebApplication mWebApplication = null;
                SetCreateNewSPSiteTemplate(info);
                if (mLanguageForNewCreated != 0 && info.LCID != mLanguageForNewCreated)
                {
                    info.LCID = mLanguageForNewCreated;
                }
                if (string.IsNullOrEmpty(info.OnlineAdminSiteUrl))
                {
                    if (string.IsNullOrEmpty(this.DestinationURL))
                    {
                        this.DestinationURL = mSiteUrl;
                    }
                    mWebApplication = AveSPWebApplication.FindWebApplication(string.Empty, this.DestinationURL, true, mOMFactory);
                    AveAuthenticationUtility.InitAuthenticationProvider(mWebApplication);
                }
                if (info.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(info.OnlineAdminSiteUrl))
                    {
                        CreateNewLocalMySite(siteUrl, info);
                    }
                    else
                    {
                        CreateNewOffice365MySite(siteUrl, info);
                    }
                }
                else if (string.IsNullOrEmpty(info.OnlineAdminSiteUrl))
                {
                    CreateNewLocalSPSite(siteUrl, info);
                    RemoveRoleAssignments(info);
                }
                else
                {
                    CreateNewOffice365Site(siteUrl, info);
                }

            }

        }

        private void SetCreateNewSPSiteTemplate(AveSiteInfo info)
        {
            if (MappingManager.SiteMappingManager.TemplateMapping.ContainsKey(info.WebTemplate))
            {
                info.WebTemplate = MappingManager.SiteMappingManager.TemplateMapping[info.WebTemplate];
            }
            if (mWebId != Guid.Empty && info.AllWebTemplates != null)
            {
                if (info.AllWebTemplates.ContainsKey(mWebId))
                {
                    info.WebTemplate = info.AllWebTemplates[mWebId];
                }
            }
            //获取custom template mapping
            TemplateKeyInfo templateInfo = new TemplateKeyInfo(TemplateMappingLevel.Web, "", info.WebTemplate);
            string mappingTemplate = this.TemplateMapping.GetMappingTemplateBeforeAdd(templateInfo);
            if (!mappingTemplate.Equals(info.WebTemplate, StringComparison.OrdinalIgnoreCase))
            {
                info.WebTemplate = mappingTemplate;
            }
        }
        private void CreateNewLocalMySite(string siteUrl, AveSiteInfo info)
        {
            //if (webApp != null)
            //{
            //    this.mSPMembers.SetFBAStatus(webApp);
            //}
            if (!string.IsNullOrEmpty(info.OwnerLogin))
            {
                info.OwnerLogin = this.mSPMembers.GetMappingUserLogin(info.OwnerLogin, !mCreationAccountResetted);
            }
            if (!string.IsNullOrEmpty(info.SecondaryContactLogin))
            {
                info.SecondaryContactLogin = this.mSPMembers.GetMappingUserLogin(info.SecondaryContactLogin, !mCreationAccountResetted);
            }
            //ADO-137210 对于FBA User由于通过：格式无法创建mysite，因此需要将：格式转为|格式再创建mysite
            var userInfo = this.ObjectModelFactory.Utility.ResolvePrincipal(mWebApplication, null, info.OwnerLogin, AvePrincipalType.User, AvePrincipalSource.MembershipProvider, false);
            if (userInfo != null && !string.IsNullOrEmpty(userInfo.LoginName))
            {
                info.OwnerLogin = userInfo.LoginName;
            }
            else
            {
                log.Warn("Can not get owner login name. Owner login name is: {0}.", info.OwnerLogin);
            }
            mSPSite = AveSPMySite.FindOrCreatePersonalSite(mWebApplication, info.OwnerLogin, info.LCID, mOMFactory);
            mSiteUrl = mSPSite.Url;
        }
        private void CreateNewLocalSPSite(string siteUrl, AveSiteInfo info)
        {
            //if (string.IsNullOrEmpty(this.DestinationURL))
            //{
            //    this.DestinationURL = mSiteUrl;
            //}
            //mWebApplication = AveSPWebApplication.FindWebApplication(mSiteUrl, this.DestinationURL, info.IsHostheader, mOMFactory);
            //if (webApp != null)
            //{
            //    this.mSPMembers.SetFBAStatus(webApp);
            //}
            bool needGiveUp = false;

            IAveSiteCollection sites = null;
            if (!mContentDBId.Equals(Guid.Empty))
            {
                IAveContentDatabase contentDB = mWebApplication.ContentDatabases[mContentDBId];
                if (contentDB != null && contentDB.Exists)
                {
                    sites = contentDB.Sites;
                }
                else
                {
                    sites = mWebApplication.Sites;
                    //throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotGetContentDatabaseById, mContentDBId);
                    //throw new ErrorCodeExceptions.SharePoint.ContentDataBaseNotExistException(contentDB.Name, mWebApplication.Name, AveUrlUtility.GetSolutionUrl(mSiteUrl, mWebApplication.ID, AveConstants.DATABASEVIEW));
                }
            }
            else
            {
                sites = mWebApplication.Sites;
            }
            if (!string.IsNullOrEmpty(info.OwnerLogin))
            {
                string oldLogin = info.OwnerLogin;
                info.OwnerLogin = this.mSPMembers.GetMappingUserLogin(info.OwnerLogin, !mCreationAccountResetted);
                if (!oldLogin.Equals(info.OwnerLogin))
                {
                    if (!string.Equals(this.mSPMembers.GetMappingUserLogin(oldLogin, false), info.OwnerLogin))
                    {
                        info.OwnerName = GetUserDisplayName(info.OwnerLogin, ref info.OwnerEmail);
                    }
                }
            }
            if (!string.IsNullOrEmpty(info.SecondaryContactLogin))
            {
                string oldLogin = info.SecondaryContactLogin;
                info.SecondaryContactLogin = this.mSPMembers.GetMappingUserLogin(info.SecondaryContactLogin, !mCreationAccountResetted);
                if (!oldLogin.Equals(info.SecondaryContactLogin))
                {
                    if (!string.Equals(this.mSPMembers.GetMappingUserLogin(oldLogin, false), info.SecondaryContactLogin))
                    {
                        info.SecondaryContactName = GetUserDisplayName(info.SecondaryContactLogin, ref info.SecondaryContactEmail);
                    }
                }
            }
            if (info.WebTemplate.Contains('#'))
            {
                int result = 0;
                int.TryParse(info.WebTemplate.Substring(info.WebTemplate.LastIndexOf('#') + 1), out result);
                if (result < 0)
                {
                    //如果#后面的数字小于0则用空模板创建
                    info.WebTemplate = string.Empty;
                }
            }
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current = null;
            }
            if (!siteUrl.StartsWith(this.DestinationURL.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                mUseHostHeader = true;
            }

            try
            {
                ReplaceInvalidUser(info, mWebApplication);
            }
            catch (Exception ex)
            {
                log.Warn("ReplaceInvalidUser failed while creating new SPSite.error:{0}", ex.ToString());
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_ReplaceInvalidUserFailed);
            }

            while (true)
            {
                try
                {
                    if (info.IsHostheader & mUseHostHeader)
                    {
                        if (info.SecondaryContactLogin != null)
                        {
                            mSPSite = sites.Add(siteUrl,
                                                     info.Title,
                                                     info.Description,
                                                     info.LCID,
                                                     info.CompatibilityLevel,
                                                     info.WebTemplate,
                                                     info.OwnerLogin,
                                                     info.OwnerName,
                                                     info.OwnerEmail,
                                                     info.SecondaryContactLogin,
                                                     info.SecondaryContactName,
                                                     info.SecondaryContactEmail,
                                                     true);
                        }
                        else
                        {
                            mSPSite = sites.Add(siteUrl,
                                                     info.Title,
                                                     info.Description,
                                                     info.LCID,
                                                     info.CompatibilityLevel,
                                                     info.WebTemplate,
                                                     info.OwnerLogin,
                                                     info.OwnerName,
                                                     info.OwnerEmail,
                                                     null,
                                                     null,
                                                     null,
                                                     true);

                        }
                    }
                    else
                    {
                        if (info.SecondaryContactLogin != null)
                        {
                            mSPSite = sites.Add(siteUrl,
                                                     info.Title,
                                                     info.Description,
                                                     info.LCID,
                                                     info.CompatibilityLevel,
                                                     info.WebTemplate,
                                                     info.OwnerLogin,
                                                     info.OwnerName,
                                                     info.OwnerEmail,
                                                     info.SecondaryContactLogin,
                                                     info.SecondaryContactName,
                                                     info.SecondaryContactEmail);
                        }
                        else
                        {
                            mSPSite = sites.Add(siteUrl,
                                                     info.Title,
                                                     info.Description,
                                                     info.LCID,
                                                     info.CompatibilityLevel,
                                                     info.WebTemplate,
                                                     info.OwnerLogin,
                                                     info.OwnerName,
                                                     info.OwnerEmail,
                                                     null,
                                                     null,
                                                     null);
                        }
                    }

                    break;
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Warn("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}", info.OwnerLogin, info.OwnerName, info.SecondaryContactLogin, info.SecondaryContactName, info.OwnerEmail, info.SecondaryContactEmail, info.Title, info.Description, info.LCID, info.WebTemplate, siteUrl, e.ToString());
                    var sqlException = e as System.Data.SqlClient.SqlException;
                    //ADO-169048: sqlExcetion 229表示user对contentDB没有权限，不再尝试重新创建，直接抛出，防止report error信息提示错误
                    if (needGiveUp || (sqlException != null && sqlException.Number == 229))
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("An error occurred while creating site collection. url:{0}\n error message:{1}", siteUrl, e));
                        throw;
                    }

                    string loginName = Environment.UserDomainName + "\\" + Environment.UserName;

                    log.Log(AveLogLevel.WARN, string.Format("Site owner: {0}", info.OwnerLogin));
                    //mLog.Warn(string.Format("site owner: {0}", info.OwnerLogin));

                    info.OwnerLogin = loginName;
                    info.OwnerName = Environment.UserName;
                    info.SecondaryContactLogin = loginName;
                    info.SecondaryContactName = Environment.UserName;

                    needGiveUp = true;
                }
            }
        }
        private void RemoveRoleAssignments(AveSiteInfo info)
        {
            IAvePrincipal principal = null;
            try
            {
                principal = mSPSite.RootWeb.EnsureAvailableUser(info.OwnerLogin);
                mSPSite.RootWeb.RoleAssignments.RemoveById(principal.ID);
                if (!string.IsNullOrEmpty(info.SecondaryContactLogin))
                {
                    principal = mSPSite.RootWeb.EnsureAvailableUser(info.SecondaryContactLogin);
                    mSPSite.RootWeb.RoleAssignments.RemoveById(principal.ID);
                }
                string loginName = Environment.UserDomainName + "\\" + Environment.UserName;
                principal = mSPSite.RootWeb.EnsureAvailableUser(loginName);
                if (principal != null && principal.ID != mSPSite.Owner.ID && (mSPSite.SecondaryContact == null || mSPSite.SecondaryContact.ID != principal.ID))
                {
                    mSPSite.RootWeb.SiteUsers.RemoveByID(principal.ID);
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "Failed to clear user after create new site, user: {0}, exception: {1}", principal == null ? "null" : principal.LoginName, e);
                //log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(info.OwnerLogin, e));
            }
        }
        private void CreateNewOffice365Site(string siteUrl, AveSiteInfo info)
        {
            var tenant = mOMFactory.CreateTenant(info.OnlineAdminSiteUrl, info.IsOnline);
            string exceptionString = null;
            try
            {
                var onlineSiteList = tenant.GetManagedSiteCollectionsList(info.OnlineAdminSiteUrl);
                if (onlineSiteList == null || !onlineSiteList.Exists(v => (v.Keys.Contains("SiteCollectionUrl") && v["SiteCollectionUrl"].ToString() == siteUrl)))
                {
                    var ownerLogin = info.OwnerLogin.Substring(info.OwnerLogin.LastIndexOf('|') + 1);
                    exceptionString = tenant.CreateSite(0, info.LCID, ownerLogin, info.StorageMaximumLevel, info.WebTemplate, info.TimeZoneId, info.Title, siteUrl, info.UserCodeMaximumLevel);
                    //这里的ExceptionString是WebService或Client API返回的，不用做国际化。
                    if (!string.IsNullOrEmpty(exceptionString))
                    {
                        throw new Exception(exceptionString);
                    }
                    mSPSite = mOMFactory.CreateSite(siteUrl);
                }
            }
            catch (Exception e)
            {
                log.Warn("OwnerLogin: {0}\tOwnerName: {1}\tSecondaryContactLogin: {2}\tSecondaryContactName: {3}\tOwnerEmail: {4}\tSecondaryContactEmail: {5}\tTitle: {6}\tDescription: {7}\tLCID: {8}\tWebTemplate: {9}\tSiteUrl: {10}\tStorageMaximumLevel: {11}\tUserCodeMaximumLevel: {12}\tTimeZoneId: {13}\terror message: {14}",
                     info.OwnerLogin, info.OwnerName, info.SecondaryContactLogin, info.SecondaryContactName, info.OwnerEmail, info.SecondaryContactEmail, info.Title, info.Description, info.LCID, info.WebTemplate, siteUrl, info.StorageMaximumLevel, info.UserCodeMaximumLevel, info.TimeZoneId, exceptionString);
                log.Error("An error occurred while creating site collection. url: {0}\nadmin url: {1}\nerror message: {2}", siteUrl, info.OnlineAdminSiteUrl, e);
                throw;
            }
        }

        private void CreateNewOffice365MySite(string siteUrl, AveSiteInfo info)
        {
            try
            {
                IAveProfileLoader profileLoader = mOMFactory.CreateProfileLoader(info.OnlineAdminSiteUrl);
                string[] emailIds = new string[] { info.OwnerLogin };
                Dictionary<string, object> personalSiteMessage = profileLoader.CreatePersonalSiteEnqueueBulk(emailIds, info.OwnerLogin);
                if (personalSiteMessage.ContainsKey("ErrorMessage"))
                {
                    log.Error("Create Site Collection is Failed,Error Message:{0}", (personalSiteMessage["ErrorMessage"].ToString()));
                    throw new Exception(personalSiteMessage["ErrorMessage"].ToString());
                }

                var siteRealUrl = personalSiteMessage["PersonalUrl"].ToString();
                mSPSite = mOMFactory.CreateSite(siteRealUrl);
                mSiteUrl = mSPSite.Url;
            }
            catch (Exception e)
            {
                log.Warn("OwnerLogin: {0}\tOwnerName: {1}\tSecondaryContactLogin: {2}\tSecondaryContactName: {3}\tOwnerEmail: {4}\tSecondaryContactEmail: {5}\tTitle: {6}\tDescription: {7}\tLCID: {8}\tWebTemplate: {9}\tSiteUrl: {10}\tStorageMaximumLevel: {11}\tUserCodeMaximumLevel: {12}\tTimeZoneId: {13}\terror message: {14}",
                     info.OwnerLogin, info.OwnerName, info.SecondaryContactLogin, info.SecondaryContactName, info.OwnerEmail, info.SecondaryContactEmail, info.Title, info.Description, info.LCID, info.WebTemplate, siteUrl, info.StorageMaximumLevel, info.UserCodeMaximumLevel, info.TimeZoneId, e.Message);
                throw;
            }
        }

        private string GetUserDisplayName(string loginName, ref string userEmail)
        {
            try
            {
                IAveUtility utility = this.ObjectModelFactory.Utility;
                var userInfo = utility.ResolvePrincipal(this.SPWebApplication, null, loginName, AvePrincipalType.User, AvePrincipalSource.All, false);
                if (userInfo != null)
                {
                    userEmail = userInfo.Email;
                    return userInfo.DisplayName;
                }
                if (loginName.IndexOf('|') > 0)
                {
                    return loginName.Substring(loginName.LastIndexOf('|') + 1);
                }
                else if (loginName.IndexOf(':') > 0)
                {
                    return loginName.Substring(loginName.LastIndexOf(':') + 1);
                }
                else if (loginName.IndexOf('\\') > 0)
                {
                    return loginName.Substring(loginName.LastIndexOf('\\') + 1);
                }
            }
            catch (Exception ex)
            {
                log.Info("Cannot find user by login name:{0}. Reason:{1}.", loginName, ex.ToString());
            }
            return loginName;
        }

        /// <summary>
        /// /// 这个函数主要是为了load或者创建基本的site所需要的，如果需要还原setting，请到restore property函数中。
        /// </summary>
        /// <param name="siteInfo"></param>
        public virtual void RestoreSiteSelf(AveSiteInfo siteInfo)
        {
            RestoreSiteSelf(siteInfo, true);
        }

        private bool hasInitialAveSite = false;
        public IAveSite AveSite
        {
            get
            {
                try
                {
                    if (mSPSite == null && !string.IsNullOrEmpty(mSiteUrl) && !hasInitialAveSite)
                    {
                        mSPSite = mOMFactory.CreateSite(mSiteUrl);
                    }
                }
                catch (Exception e)
                {
                    log.Debug(string.Format("Get AveSite error.Exception:{0}", e.ToString()));
                }
                finally
                {
                    hasInitialAveSite = true;
                }

                return mSPSite;
            }
        }

        public bool TemplateAvalible(string templateName)
        {
            IAveSite site = mOMFactory.CreateAdministrationWebApplication().Local.Sites[0];
            IAveWeb rootWeb = site.RootWeb;
            IAveRegionalSettings regionalSettings = mOMFactory.CreateRegionalSettings(rootWeb, false);
            foreach (IAveLanguage lanuage in regionalSettings.InstalledLanguages)
            {
                var templates = site.GetWebTemplates((uint)lanuage.LCID);

                foreach (var template in templates)
                {
                    if (string.Equals(template.Name, templateName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TemplateAvalible(string templateName, out bool isHidden)
        {
            isHidden = false;
            IAveSite site = mOMFactory.CreateAdministrationWebApplication().Local.Sites[0];
            IAveWeb rootWeb = site.RootWeb;
            IAveRegionalSettings regionalSettings = mOMFactory.CreateRegionalSettings(rootWeb, false);
            foreach (IAveLanguage lanuage in regionalSettings.InstalledLanguages)
            {
                var templates = site.GetWebTemplates((uint)lanuage.LCID);

                foreach (var template in templates)
                {
                    if (string.Equals(template.Name, templateName, StringComparison.OrdinalIgnoreCase))
                    {
                        isHidden = template.IsHidden;
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual void RestoreSiteSelf(AveSiteInfo siteInfo, bool needCreateSite)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Site.RestoreSiteSelf"))
            {

                SourceSiteInfo = siteInfo;
                mSrcLcd = siteInfo.LCID;
                if (string.IsNullOrEmpty(SourceSiteInfo.OwnerLogin))
                {
                    string loginName = Environment.UserDomainName + "\\" + Environment.UserName;
                    SourceSiteInfo.OwnerLogin = loginName;
                    SourceSiteInfo.OwnerName = Environment.UserName;
                }

                try
                {
                    if (siteInfo.IsHostheader && mUseHostHeader && string.Equals(mSiteUrl.Trim('/'), DestinationURL.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        mSiteUrl = siteInfo.Url;
                    }
                    if (siteInfo.IsHostheader &&
                            !String.IsNullOrEmpty(this.DestinationURL) && (!mSiteUrl.StartsWith(this.DestinationURL.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)))
                    {
                        mUseHostHeader = true;
                    }
                    if (mSPSite == null)
                    {
                        mSPSite = mOMFactory.CreateSite(mSiteUrl);
                        mWebApplication = mSPSite.WebApplication;
                    }

                    //检测获取到的SPSite对象是否在目的端指定的WebApplication下，主要是Host Header类型的Site
                    try
                    {

                        //if (mOMFactory.ContextKind != AveContextKind.ClientObjectModel && !String.IsNullOrEmpty(this.DestinationURL))
                        //{
                        if (siteInfo.IsHostheader && this.mUseHostHeader)
                        {
                            IAveWebApplication webApp = mOMFactory.CreateWebApplication().Lookup(new Uri(DestinationURL));
                            if (mSPSite.WebApplication.ID != webApp.ID)
                            {
                                Dispose();
                                mSPSite = null;
                                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_HostHeaderSiteCollectionAlreadyExists);
                            }
                            if (webApp is IDisposable)
                            {
                                (webApp as IDisposable).Dispose();
                            }
                        }
                        //判断New的SPSite对象是否为目的端Sitecollection
                        if (!mSiteUrl.TrimEnd('/').Equals(mSPSite.Url.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                        {
                            if (this.SPContextKind != AveContextKind.ClientObjectModel)
                            {
                                Dispose();
                                mSPSite = null;
                                if (SourceSiteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new FileNotFoundException();
                                }
                                else
                                {
                                    throw new FileNotFoundException("The site collection URL [" + mSiteUrl + "] is invalid.");
                                }
                            }
                            else if (this.AveSite.IsOnlineSite)
                            {
                                //ADO-169316，ADO-181945:真实365站点不支持自定义ManagedPath
                                string sourceManagedPath = GetSiteManagedPath(mSiteUrl, siteInfo.Prefixes);
                                if (sourceManagedPath != null
                                    && !sourceManagedPath.Equals("sites", StringComparison.OrdinalIgnoreCase)
                                    && !sourceManagedPath.Equals("teams", StringComparison.OrdinalIgnoreCase)
                                    && !sourceManagedPath.Equals("personal", StringComparison.OrdinalIgnoreCase))
                                {
                                    Dispose();
                                    mSPSite = null;
                                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreSiteCollectionFailedEventMessage(mSiteUrl, new AvePoint.GCommon.Utility.Exceptions.SharePoint.ManagedPathNotFoundException()));
                                    throw new AvePoint.Wrapper.Common.ManagedPathNotFoundException(AveInternalResourceKey.Wrapper_Exception_Restore_ManagedPathNotFound);
                                }
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        throw;
                    }
                    catch (AveException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        log.Warn("Error while get Web Application: " + this.DestinationURL + ". Error: " + e.ToString());
                    }
                    if (this.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                    {
                        AveAuthenticationUtility.InitAuthenticationProvider(mSPSite.WebApplication);
                    }
                }
                catch (AveException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CreateSiteCollectionError, e.ToString());
                    if (needCreateSite)
                    {
                        if (!String.IsNullOrEmpty(this.DestinationURL) && string.IsNullOrEmpty(siteInfo.OnlineAdminSiteUrl))
                        {
                            log.Debug(string.Format("Destination WebApplication Url: {0}", this.DestinationURL));
                            IAveWebApplication webApp = mOMFactory.CreateWebApplication().Lookup(new Uri(this.DestinationURL));
                            if (!SourceSiteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase)//do not check managed path if source site collection is mysite
                                && (mCheckManagedPath && !AveUrlUtility.CheckManagedPath(webApp, mOMFactory, mSiteUrl, mUseHostHeader)))
                            {
                                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreSiteCollectionFailedEventMessage(mSiteUrl, new AvePoint.GCommon.Utility.Exceptions.SharePoint.ManagedPathNotFoundException()));
                                throw new AvePoint.Wrapper.Common.ManagedPathNotFoundException(AveInternalResourceKey.Wrapper_Exception_Restore_ManagedPathNotFound);
                            }
                            if (webApp is IDisposable)
                            {
                                (webApp as IDisposable).Dispose();
                            }
                        }
                        try
                        {
                            CreateNewSPSite(mSiteUrl, SourceSiteInfo);
                            mIsNewCreated = true;
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            log.Warn("Cannot create a site collection, the user is not in the farm administrator's group.", ex);
                            //report.AddDetail(new AveWrapperReportDto(mSiteUrl, mSiteUrl, AveReportObjectType.CreateNewSite, AveStatus.Failed, WrapperReportResource.Wrapper_Report_NoPermissionToCreateSite + ex.Message));
                            throw;
                        }
                        catch (UnauthorizedAccessException unex)
                        {
                            log.Warn("Cannot create a site collection, the user is not in the farm administrator's group.", unex);
                            //report.AddDetail(new AveWrapperReportDto(mSiteUrl, mSiteUrl, AveReportObjectType.CreateNewSite, AveStatus.Failed, WrapperReportResource.Wrapper_Report_NoPermissionToCreateSite + unex.Message));
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotHavePermissionToCreateSiteCollection);
                        }
                        catch (FileNotFoundException fnfex)
                        {
                            log.Warn(fnfex.ToString());//异常内容已经完成拼接，直接输出即可。
                            throw;
                        }
                    }
                    else
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_CannotCreateSiteCollectionForUserPermission);
                    }
                }
                if (mOMFactory.ContextKind != AveContextKind.ClientObjectModel)
                {
                    CheckSiteLock(mSiteUrl);
                }
                try
                {
                    if (AveUrlUtility.GetServerUrl(SourceSiteInfo.Url).EndsWith("/", StringComparison.OrdinalIgnoreCase) && AveUrlUtility.GetServerUrl(mSiteUrl).EndsWith("/", StringComparison.OrdinalIgnoreCase) && AveReplaceProcessor.ReplaceExternalUrl)
                    {
                        WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(AveUrlUtility.GetServerUrl(SourceSiteInfo.Url).TrimEnd('/'), AveUrlUtility.GetServerUrl(mSiteUrl).TrimEnd('/'));
                    }
                }
                catch (Exception e)
                {
                    log.Debug("Add WebApplication url error: {0} " + e.ToString());
                }
                WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(AveUrlUtility.GetServerUrl(SourceSiteInfo.Url), AveUrlUtility.GetServerUrl(mSiteUrl));
                WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(SourceSiteInfo.Url, mSiteUrl);
                WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddSiteUrlMapping(SourceSiteInfo.ServerRelativeUrl, mSPSite.ServerRelativeUrl);

                MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(AveUrlUtility.GetServerUrl(SourceSiteInfo.Url), AveUrlUtility.GetServerUrl(mSiteUrl));
                MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(SourceSiteInfo.Url, mSiteUrl);
                MappingManager.SiteMappingManager.AddSiteUrlMapping(SourceSiteInfo.ServerRelativeUrl, mSPSite.ServerRelativeUrl);
                MappingManager.SiteMappingManager.AddSiteIDMapping(SourceSiteInfo.Id, mSPSite.ID);
                MappingManager.SiteMappingManager.SourceSiteInfo = this.SourceSiteInfo;
                MappingManager.SiteMappingManager.DestSiteInfo = new AveSiteInfo() { ServerRelativeUrl = mSPSite.ServerRelativeUrl, Url = mSiteUrl, SPVersion = mSPSite.SPVersion };
                WFConflictResolution.ParentSite = this;
                WorkflowCache = new WorkflowSiteCollectionCache();
                WorkflowCache.SiteId = this.SPSite.ID;
                if (this.SPSite.IsOnlineSite)
                {
                    mSPMembers = new AveSPMembersMultiThread(mSPMembers, this);
                }
                else
                {
                    try
                    {
                        if (this.SPSite.DenyPermissionsMask != AveBasePermissions.EmptyMask)
                        {
                            denyPermissionsMask = this.SPSite.DenyPermissionsMask;
                            this.SPSite.DenyPermissionsMask = AveBasePermissions.EmptyMask;
                            this.SPSite.ReloadSite();//需要Reload site，否则不生效。
                        }

                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while set deny permissions mask to Empty. error: {0}", e);
                    }
                }
                InitializeMembers();

            }

        }

        public void RestoreWorkflowStartOption()
        {
            try
            {
                if (this.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    log.Info("Begin restore workflow start option");
                    if (WorkflowCache.Cache != null)
                    {
                        foreach (KeyValuePair<Guid, Dictionary<Guid, WorkflowStartOptionCache>> webWorkflows in WorkflowCache.Cache)
                        {
                            log.Info("Web ID:{0}", webWorkflows.Key);
                            var web = mSPSite.OpenWeb(webWorkflows.Key);
                            log.Info("Being to restore workflow start option for web {0}", web.ServerRelativeUrl);
                            foreach (KeyValuePair<Guid, WorkflowStartOptionCache> listWorkflows in webWorkflows.Value)
                            {
                                log.Info("List ID:{0}", listWorkflows.Key);
                                IAveList spList = web.Lists[listWorkflows.Key];
                                log.Info("Being to restore workflow start option for list {0}", spList.Title);
                                spList.RestoreWOrkflowStartOption(web.Url, web.ID, spList.ID, listWorkflows.Value);
                                log.Info("Finish restore workflow start option for list {0}", spList.Title);
                            }
                            log.Info("Finish restore workflow start option for web {0}", web.ServerRelativeUrl);
                        }
                    }
                    else
                    {
                        log.Debug("Workflow cache is null");
                    }
                }
                else
                {
                    log.Debug("Not client object model");
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while restore list workflow start option setting.Error:{0}", e);
            }
        }

        private string GetSiteManagedPath(string siteUrl, List<string> prefixes)
        {
            try
            {
                //按length从大到小比。ADO-200159
                prefixes.Sort((x, y) => { return (string.IsNullOrEmpty(y) ? 0 : y.Length) - (string.IsNullOrEmpty(x) ? 0 : x.Length); });
                string urlHeader = siteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? "https://" : "http://";
                int index = siteUrl.LastIndexOf('/');
                while (index > urlHeader.Length)
                {
                    siteUrl = siteUrl.Substring(0, index);
                    foreach (string prefix in prefixes)
                    {
                        //root prefix为Empty, 先判断root以外的prefix
                        if (!String.IsNullOrEmpty(prefix) && siteUrl.EndsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            return prefix;
                        }
                    }
                    index = siteUrl.LastIndexOf('/');
                }
                //返回empty表示为root prefix
                return String.Empty;
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting site managedPath, site url:{0}, error:{1}", siteUrl, e);
                return null;
            }

        }

        public virtual bool IsValidCompatibilityLevel(int compatibilityLevel)
        {
            if (15 != compatibilityLevel)
            {
                return (14 == compatibilityLevel);
            }
            return true;
        }


        private void CheckSiteLock(string siteUrl)
        {
            using (IAveSite siteAdmin = mOMFactory.CreateSite(siteUrl))
            {
                if (siteAdmin.ReadLocked || siteAdmin.WriteLocked || siteAdmin.ReadOnly)
                {
                    siteAdmin.RootWeb.AddProperty(Guid.NewGuid().ToString(), 1);
                    siteAdmin.RootWeb.Update();//throw exception if lock.
                }
            }
        }

        public void RestoreSiteProperty(AveSiteSettingInfo siteSettingInfo)
        {
            this.RestoreSiteProperty(siteSettingInfo, false);
        }
        public void RestoreSiteProperty(AveSiteSettingInfo siteSettingInfo, bool needRestoreSiteAdmin)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreSiteProperty"))
            {

                SourceSiteSettingInfo = siteSettingInfo;
                try
                {
                    if (SourceSiteSettingInfo.PortalURL != null && SourceSiteSettingInfo.PortalURL.IsAvailable && WrapperConfiguration.RestorePortalConnection)
                    {
                        if (SourceSiteSettingInfo.PortalURL.Value != null)
                        {
                            MappingManager.SiteMappingManager.AddUrlNeedPostActionMapping(Guid.Empty, "PortalUrl", SourceSiteSettingInfo.PortalURL.Value);
                        }
                    }
                    base.IsSettingRestored = true;
                    mSPSite.RestoreSettings(siteSettingInfo);
                    if (needRestoreSiteAdmin && mSPSite != null)
                    {

                        if (SourceSiteSettingInfo.OwnerID != null)
                        {
                            mSPSite.Owner = mSPMembers.FindMember(SourceSiteSettingInfo.OwnerID.Value.Value, true) as IAveUser;
                        }
                        if (SourceSiteSettingInfo.SecondaryContactID != null)
                        {
                            mSPSite.SecondaryContact = SPMembers.FindMember(SourceSiteSettingInfo.SecondaryContactID.Value.Value, true) as IAveUser;
                        }

                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while creating site collection. url:{0}, site id:{1}\n error message:{2}", mSPSite.Url, mSPSite.ID, ex));
                    report.AddDetail(new AveWrapperReportDto(mSPSite.Url, mSPSite.Url, AveReportObjectType.SiteSetting, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToUpdateSiteSetting, ex.Message));
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto(mSPSite.Url, mSPSite.Url, AveReportObjectType.SiteSetting, AveStatus.Skipped, AveReportResource.Wrapper_Report_CreateSiteCollectionError, mSPSite.Url, mSPSite.ID, e.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while creating site collection. url:{0}, site id:{1}\n error message:{2}", mSPSite.Url, mSPSite.ID, e));
                }

            }

        }

        public void RestoreLanguageFile(AveLanguageInfo languageInfo)
        {
            if (string.IsNullOrEmpty(mAveLanguageProcesser.JobDir))
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_SetLanguageResourcePath);
            }


            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Site.RestoreLanguageFile"))
            {

                string path = mAveLanguageProcesser.JobDir + "\\" + languageInfo.LanguageLCD + "src.resx";
                mAveLanguageProcesser.AddLanguageFilePath(path);
                using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(languageInfo.LanguageContent);
                }

            }

        }

        //废弃此方法
        //此方法名写错了
        public void RestroeLanguageFile(AveLanguageInfo languageInfo)
        {
            if (string.IsNullOrEmpty(mAveLanguageProcesser.JobDir))
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_SetLanguageResourcePath);
            }


            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreLanguageFile"))
            {

                string path = mAveLanguageProcesser.JobDir + "\\" + languageInfo.LanguageLCD + "src.resx";
                using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(languageInfo.LanguageContent);
                }

            }

        }
        private bool disableTaskSynchronous = false;

        public bool DisableTaskSynchronous
        {
            set { disableTaskSynchronous = value; }
            get { return disableTaskSynchronous; }
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void DisableSPEventReceiver()
        {
            if (RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER)
            {
                AveSPEventReceiverConfig.InitEventReceiver(mOMFactory);
                AveSPEventReceiverConfig.DisableEventReceiver();
            }
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void EnableSPEventReceiver()
        {
            AveSPEventReceiverConfig.EnableEventReceiver();
        }

        public void ReloadSite()
        {
            try
            {
                mSPSite.ReloadSite();
                InitializeMembers();
                mSPRequestTimeout = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("Reload site failed. Site name:{0}\n error message:{1}", mSPSite == null ? "" : mSPSite.Url, e));
            }
        }

        public void SetTimeoutForReloadSPRequest(int hours)
        {
            if (hours > 0 && hours < 24)
            {
                mHoursReloadSite = hours;
            }
        }

        /// <summary>
        /// check iis directory for restore WebPart, if iis directory doesn't exist, all pages that contain WebPart will be blank.
        /// </summary>
        /// <param name="zone">specify whick zone to check</param>
        /// <returns>true is the iis directory exists, otherwise, false</returns>
        public bool CheckIisDirectory(AveUrlZone zone = AveUrlZone.Default)
        {
            try
            {
                if (mOMFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    return true;
                }
                return mSPSite.WebApplication.IisSettings[zone].Path.Exists;
            }
            catch (Exception ex)
            {
                log.Warn("Failed to check IIS directory. Error:{0}", ex.ToString());
                return false;
            }
        }

        #region For Post Action
        public void RestoreNavNodes(IReport report)
        {
            AveSPNavigation navManager = null;
            try
            {
                navManager = new AveSPNavigation(this);
                navManager.OverWrite = OverWriteNavigation || IsNewCreated;
                navManager.Restore();
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore navigation. Url:{0}\n error message:{1}", mSiteUrl, ex));
                //qlluo: Post action do not support report, remove it.
                //report.AddDetail(new AveWrapperReportDto("RestoreNavNodes", "RestoreNavNodes", AveReportObjectType.NavNodes, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreNavNodes + ex.Message));
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore navigation. Url:{0}\n error message:{1}", mSiteUrl, e));
                //mLog.Warn(e, "An error occurred while restoring navigation. Url:{0}", mSiteUrl);
            }
            finally
            {
                if (navManager != null)
                {
                    report.AddDetails(navManager.GetReport().GetDetails());
                    navManager.Dispose();
                }
            }
        }

        public void RestorePerformancePointProperties()
        {
            try
            {
                AvePerformancePointServiceControl.UpdateItemProperties(this);
            }
            catch (AveSecurityTrimingException e)
            {
                log.Warn("An error occurred while restore performance point properties. ", e);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_routermanageremail: Property of web's MetaInfo")]
        /// <summary>
        /// 有些情況下，__GlobalNavigationExcludes和__CurrentNavigationExcludes在创建Sub Site之后被修改，例如Search类型sub site
        /// 所以需要在Post Action里面Keep下这些值，只对restore过的web做处理，而且目前只处理Root Web，如果日后有问题，可以处理所有web
        /// </summary>
        internal void RestoreWebMetaInfo()
        {
            try
            {
                foreach (KeyValuePair<Guid, Dictionary<string, string>> keyValue in MappingManager.SiteMappingManager.WebAllPropertiesMapping)
                {
                    try
                    {
                        using (IAveWeb web = mSPSite.OpenWeb(keyValue.Key))
                        {
                            Dictionary<string, string> sourcWebsAndPages = new Dictionary<string, string>();
                            if (MappingManager.SiteMappingManager.AllSubWebsAndPagesMapping.ContainsKey(web.ID))
                            {
                                sourcWebsAndPages = MappingManager.SiteMappingManager.AllSubWebsAndPagesMapping[web.ID];
                            }
                            bool changed = false;
                            string globalNavigationExcludes = string.Empty;
                            string currentNavigationExcludes = string.Empty;
                            StringBuilder destSelfGlobalNavigation = new StringBuilder();
                            StringBuilder destSelfCurrentNavigation = new StringBuilder();
                            if (web.AllProperties.ContainsKey("__GlobalNavigationExcludes") && !string.IsNullOrEmpty(web.AllProperties["__GlobalNavigationExcludes"].ToString()))
                            {
                                string oldGlobalNavigationExcludes = web.AllProperties["__GlobalNavigationExcludes"].ToString();
                                getDestSelfHidden(oldGlobalNavigationExcludes, destSelfGlobalNavigation, web, sourcWebsAndPages);
                                if (keyValue.Value.ContainsKey("__GlobalNavigationExcludes") && !string.IsNullOrEmpty(keyValue.Value["__GlobalNavigationExcludes"]))
                                {
                                    globalNavigationExcludes = string.Format("{0};{1}", keyValue.Value["__GlobalNavigationExcludes"].TrimEnd(';'), destSelfGlobalNavigation.ToString());
                                }
                                else
                                {
                                    globalNavigationExcludes = destSelfGlobalNavigation.ToString();
                                }
                                changed = true;
                            }
                            else
                            {
                                if (keyValue.Value.ContainsKey("__GlobalNavigationExcludes"))
                                {
                                    globalNavigationExcludes = keyValue.Value["__GlobalNavigationExcludes"];
                                    changed = true;
                                }
                            }

                            if (web.AllProperties.ContainsKey("__CurrentNavigationExcludes") && !string.IsNullOrEmpty(web.AllProperties["__CurrentNavigationExcludes"].ToString()))
                            {
                                string oldCurrentNavigationExcludes = web.AllProperties["__CurrentNavigationExcludes"].ToString();
                                getDestSelfHidden(oldCurrentNavigationExcludes, destSelfCurrentNavigation, web, sourcWebsAndPages);
                                if (keyValue.Value.ContainsKey("__CurrentNavigationExcludes") && !string.IsNullOrEmpty(keyValue.Value["__CurrentNavigationExcludes"]))
                                {
                                    currentNavigationExcludes = string.Format("{0};{1}", keyValue.Value["__CurrentNavigationExcludes"].TrimEnd(';'), destSelfCurrentNavigation.ToString());
                                }
                                else
                                {
                                    currentNavigationExcludes = destSelfCurrentNavigation.ToString();
                                }
                                changed = true;
                            }
                            else
                            {
                                if (keyValue.Value.ContainsKey("__CurrentNavigationExcludes"))
                                {
                                    currentNavigationExcludes = keyValue.Value["__CurrentNavigationExcludes"];
                                    changed = true;
                                }
                            }
                            if (changed)
                            {
                                if (globalNavigationExcludes != string.Empty)
                                {
                                    web.AllProperties["__GlobalNavigationExcludes"] = RemoveNavigationExcludesDuplicate(globalNavigationExcludes);
                                    log.Debug("Web GlobalNavigationExcludes:{0}.", web.AllProperties["__GlobalNavigationExcludes"].ToString());
                                }
                                if (currentNavigationExcludes != string.Empty)
                                {
                                    web.AllProperties["__CurrentNavigationExcludes"] = RemoveNavigationExcludesDuplicate(currentNavigationExcludes);
                                    log.Debug("Web CurrentNavigationExcludes:{0}.", web.AllProperties["__CurrentNavigationExcludes"].ToString());
                                }
                                web.Update();
                            }
                            if (keyValue.Value.ContainsKey("_routermanageremail"))
                            {
                                try
                                {
                                    string routerManager = string.Empty;
                                    string original = web.Properties["_routermanageremail"];
                                    IAveList list = web.GetList(web.ServerRelativeUrl + "/RoutingRules");
                                    if (list != null && list.HasUniqueRoleAssignments)
                                    {
                                        foreach (IAveRoleAssignment role in list.RoleAssignments)
                                        {
                                            if (role.Member.PrincipalType == AvePrincipalType.User)
                                            {
                                                routerManager += role.Member.LoginName + ",";
                                            }
                                        }
                                        if (IsExistSystemAccount(original))
                                        {
                                            if (!IsExistSystemAccount(routerManager))
                                            {
                                                routerManager += "SHAREPOINT\\system,";
                                            }
                                        }
                                        web.Properties["_routermanageremail"] = routerManager.TrimEnd(',');
                                        web.Properties.Update();
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Warn("An error occurred while restore content organizer setting in metaInfo. error:{0}", e.ToString());
                                }
                            }
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        log.Warn("An error occurred while restore web:{0} metaInfo. error:{1}", keyValue.Key, ex.ToString());
                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn("An error occurred while restore web metaInfo. error:{0}", ex.ToString());
                //qlluo: Post action do not support report, remove it.
                //report.AddDetail(new AveWrapperReportDto("RestoreWebMetaInfo", "RestoreWebMetaInfo", AveReportObjectType.WebMetaInfo, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreWebMetaInfo + ex.Message));
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while restore web metaInfo. error:{0}", ex.ToString());
            }
        }

        public string RemoveNavigationExcludesDuplicate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            StringBuilder finalNavigation = new StringBuilder();
            try
            {
                List<string> excludes = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                HashSet<string> set = new HashSet<string>(excludes);
                excludes = set.ToList();
                foreach (string str in excludes)
                {
                    finalNavigation.Append(str).Append(";");
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred when Remove NavigationExcludes Dupuls. value:{0}, finalNavigation:{1}, e:{2}.", value, finalNavigation, e);
            }
            return finalNavigation.ToString();
        }

        private bool IsExistSystemAccount(string routerManager)
        {
            bool flag = false;
            string[] routerArray = routerManager.Split(',');
            foreach (String router in routerArray)
            {
                if (router.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase))
                {
                    return flag = true;
                }
            }
            return flag;
        }
        internal void RestoreWebWelcomePage()
        {
            foreach (var webIdAndWelcomePage in this.MappingManager.SiteMappingManager.UnRestoredWelcomePages)
            {
                string webUrl = string.Empty;
                try
                {
                    using (IAveWeb web = mSPSite.OpenWeb(webIdAndWelcomePage.Key))
                    {
                        webUrl = web.Url;
                        IAveFile file = web.GetFile(webIdAndWelcomePage.Value);
                        if (file.Exists)
                        {
                            if (this.SPContextKind != AveContextKind.ClientObjectModel && AveEnv.IsMoss && web.IsPublish)
                            {
                                this.Publishing.SetWelcomePage(web, webIdAndWelcomePage.Value);
                            }
                            else
                            {
                                IAveFolder folder = web.RootFolder;
                                folder.WelcomePage = webIdAndWelcomePage.Value;
                                folder.Update();
                            }
                        }
                        else
                        {
                            throw new FileNotFoundException();
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn(WrapperRestoreResource.RestoreWebWelcomePageFailed, webUrl, webIdAndWelcomePage.Value, e);
                }
            }
        }

        public void getDestSelfHidden(string navigationExclude, StringBuilder selfNavigation, IAveWeb web, Dictionary<string, string> sourcWebsAndPages)
        {
            string[] excludes = navigationExclude.Split(';');
            foreach (string exclude in excludes)
            {
                if (exclude.Trim().Length == 36)
                {
                    bool destSelf = true;
                    foreach (Guid key in MappingManager.SiteMappingManager.WebIDMapping.Keys)
                    {
                        if (exclude.Equals(MappingManager.SiteMappingManager.WebIDMapping[key].ToString()))
                        {
                            destSelf = false;
                            break;
                        }
                    }
                    //如果value包含exclude，说明exclude是新创建的page的id，如果key包含exclude，说明exclude是先删除后还原page的原id。
                    if (destSelf)
                    {
                        try
                        {
                            Guid excludeGuid = new Guid(exclude);
                            if (MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(excludeGuid) || MappingManager.SiteMappingManager.HiddenWebsPages.ContainsValue(excludeGuid))
                            {
                                destSelf = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, "Failed to parse destination hidden page id. Page Id:{0}. Error:{1}", exclude, ex.ToString());
                            return;
                        }
                    }
                    if (destSelf)
                    {
                        Dictionary<string, object> param = new Dictionary<string, object>();
                        try
                        {
                            var id = new Guid(exclude);
                            string path = mQueryService.GetWebFullUrlById(web.Site.ID, id);
                            if (string.IsNullOrEmpty(path))
                            {
                                path = mQueryService.GetPageUrlById(web.Site.ID, id);
                            }
                            string end = path.Substring(web.ServerRelativeUrl.Length - 1);
                            bool flag = false;
                            foreach (string key in sourcWebsAndPages.Keys)
                            {
                                if (sourcWebsAndPages[key].EndsWith(end, StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = true;
                                }
                            }
                            if (!flag)
                            {
                                selfNavigation.Append(exclude).Append(";");
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetDestSelfHiddenFailed, ex);
                        }
                    }
                }
            }
        }

        public void mergeNavigation(string dest, string source, ref StringBuilder finalNavigation)
        {
            string[] selfExcludes = dest.ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] sourceExcludes = source.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            finalNavigation.Append(source);
            foreach (string str in selfExcludes)
            {
                if (!sourceExcludes.Contains(str))
                {
                    if (source.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                    {
                        finalNavigation.Append(str).Append(";");
                    }
                    else
                    {
                        finalNavigation.Append(";").Append(str).Append(";");
                    }
                }
            }
        }

        public void RestoreHiddenSiteProperty()
        {
            try
            {
                if ((this.SPContextKind == AveContextKind.ClientObjectModel) || mSPSite.IsPublish)
                {
                    //List<Guid> webIds = GetAllWebsGuid(mSPSite);
                    foreach (Guid webId in MappingManager.SiteMappingManager.WebIDMapping.Values)
                    {
                        try
                        {
                            using (IAveWeb web = mSPSite.OpenWeb(webId))
                            {
                                //还原Hidden状态不要求 spWeb.IsPublish=true,只要求SPSite.IsPublish=true                                
                                bool change = false;
                                string globalNavigationExcludes = string.Empty;
                                string currentNavigationExcludes = string.Empty;
                                if (web.AllProperties.ContainsKey("__GlobalNavigationExcludes"))
                                {
                                    globalNavigationExcludes = web.AllProperties["__GlobalNavigationExcludes"].ToString();
                                    foreach (Guid key in MappingManager.SiteMappingManager.WebIDMapping.Keys)
                                    {
                                        if (!key.Equals(MappingManager.SiteMappingManager.WebIDMapping[key]))
                                        {
                                            if (globalNavigationExcludes.Contains(key.ToString()) && globalNavigationExcludes.Contains(MappingManager.SiteMappingManager.WebIDMapping[key].ToString()))
                                            {
                                                globalNavigationExcludes = globalNavigationExcludes.Replace(MappingManager.SiteMappingManager.WebIDMapping[key].ToString() + ";", "");
                                            }
                                        }
                                    }

                                    string[] excludes = globalNavigationExcludes.Split(';');
                                    foreach (string exclude in excludes)
                                    {
                                        if (exclude.Trim().Length == 36)
                                        {
                                            Guid id = new Guid(exclude);
                                            if (MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(id))
                                            {
                                                if (!globalNavigationExcludes.Contains(MappingManager.SiteMappingManager.WebIDMapping[id].ToString()))
                                                {
                                                    globalNavigationExcludes = globalNavigationExcludes.Replace(id.ToString(), MappingManager.SiteMappingManager.WebIDMapping[id].ToString());
                                                    change = true;
                                                }
                                            }
                                            else
                                            {
                                                if (MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(id))
                                                {
                                                    globalNavigationExcludes = globalNavigationExcludes.Replace(id.ToString(), MappingManager.SiteMappingManager.HiddenWebsPages[id].ToString());
                                                    change = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (web.AllProperties.ContainsKey("__CurrentNavigationExcludes"))
                                {
                                    currentNavigationExcludes = web.AllProperties["__CurrentNavigationExcludes"].ToString();
                                    foreach (Guid key in MappingManager.SiteMappingManager.WebIDMapping.Keys)
                                    {
                                        if (!key.Equals(MappingManager.SiteMappingManager.WebIDMapping[key]))
                                        {
                                            if (currentNavigationExcludes.Contains(key.ToString()) && currentNavigationExcludes.Contains(MappingManager.SiteMappingManager.WebIDMapping[key].ToString()))
                                            {
                                                currentNavigationExcludes = currentNavigationExcludes.Replace(MappingManager.SiteMappingManager.WebIDMapping[key].ToString() + ";", "");
                                            }
                                        }
                                    }

                                    string[] excludes = currentNavigationExcludes.Split(';');
                                    foreach (string exclude in excludes)
                                    {
                                        if (exclude.Trim().Length == 36)
                                        {
                                            Guid id = new Guid(exclude);
                                            if (MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(id))
                                            {
                                                if (!currentNavigationExcludes.Contains(MappingManager.SiteMappingManager.WebIDMapping[id].ToString()))
                                                {
                                                    currentNavigationExcludes = currentNavigationExcludes.Replace(id.ToString(), MappingManager.SiteMappingManager.WebIDMapping[id].ToString());
                                                    change = true;
                                                }
                                            }
                                            else
                                            {
                                                if (MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(id))
                                                {
                                                    currentNavigationExcludes = currentNavigationExcludes.Replace(id.ToString(), MappingManager.SiteMappingManager.HiddenWebsPages[id].ToString());
                                                    change = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (change)
                                {
                                    if (globalNavigationExcludes != string.Empty)
                                    {
                                        web.AllProperties["__GlobalNavigationExcludes"] = globalNavigationExcludes;
                                    }
                                    if (currentNavigationExcludes != string.Empty)
                                    {
                                        web.AllProperties["__CurrentNavigationExcludes"] = currentNavigationExcludes;
                                    }
                                    web.Update();
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenSiteProperty,webId:{0} . error:{1}", webId, e.ToString());
                            //mLog.Warn("An error occurred while RestoreHiddenSiteProperty,webId:{0} . error:{1}", webId, e.ToString());
                        }
                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenSiteProperty. error:{{0}", ex.ToString());
                //qlluo: Post action do not support report, remove it.
                //report.AddDetail(new AveWrapperReportDto("RestoreHiddenSiteProperty", "RestoreHiddenSiteProperty", AveReportObjectType.RestoreHiddenSiteProperty, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreHiddenSiteProperty + ex.Message));
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenSiteProperty. error:{{0}", e.ToString());
                //mLog.Warn("An error occurred while RestoreHiddenSiteProperty. error:{{0}", e.ToString());
            }
        }

        /// <summary>
        /// 还原View上的CalendarSettings属性，该属性用于显示Calender的Overlay
        /// </summary>
        public void RestoreCalendarSettings()
        {
            var needResetCalendarSettingsViews = MappingManager.SiteMappingManager.GetNeedResetCalendarSettingsViewsForSitePostAction();
            try
            {
                foreach (Guid webId in needResetCalendarSettingsViews.Keys)
                {
                    using (IAveWeb web = mSPSite.OpenWeb(webId))
                    {
                        try
                        {
                            foreach (Guid listId in needResetCalendarSettingsViews[webId].Keys)
                            {
                                IAveList list = web.Lists[listId];
                                foreach (Guid viewId in needResetCalendarSettingsViews[webId][listId])
                                {
                                    try
                                    {
                                        //对于Home page类型的添加 calendar webaprt 并存在version时，由于还下个version时之前version的SPView会删除，导致此处GetView失败，因此添加该try-catch

                                        IAveView view = list.GetView(viewId);
                                        XmlDocument xDoc = new XmlDocument();
                                        xDoc.PreserveWhitespace = true;
                                        if (!string.IsNullOrEmpty(view.CalendarSettings))
                                        {
                                            try
                                            {
                                                xDoc.InnerXml = view.CalendarSettings;
                                                bool needUpdate = false;
                                                foreach (XmlNode node in xDoc.GetElementsByTagName("AggregationCalendar"))
                                                {
                                                    try
                                                    {
                                                        XmlElement tempXe = node as XmlElement;
                                                        XmlNode settingNode = null;
                                                        if (tempXe.GetElementsByTagName("Settings").Count > 0)
                                                        {
                                                            settingNode = tempXe.GetElementsByTagName("Settings")[0];
                                                        }
                                                        if (node.Attributes["CalendarUrl"] != null && settingNode.Attributes["WebUrl"] != null)
                                                        {
                                                            string oldWebUrl = settingNode.Attributes["WebUrl"].Value;
                                                            settingNode.Attributes["WebUrl"].Value = AveReplaceProcessor.UrlReplace(oldWebUrl, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                                            string oldCalendarUrl = node.Attributes["CalendarUrl"].Value;
                                                            //判断CalenderUrl是否指向本Site Collection，如果指向其他site collection则不进行替换
                                                            if (oldCalendarUrl.StartsWith(SourceSiteInfo.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                node.Attributes["CalendarUrl"].Value = AveReplaceProcessor.UrlReplace(oldCalendarUrl, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                                            }
                                                            string calendarUrl = node.Attributes["CalendarUrl"].Value;
                                                            string webUrl = settingNode.Attributes["WebUrl"].Value;
                                                            string sourceViewId = settingNode.Attributes["ViewId"].Value;

                                                            if (!calendarUrl.Equals(oldCalendarUrl, StringComparison.OrdinalIgnoreCase) || !webUrl.Equals(oldWebUrl, StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                needUpdate = true;
                                                            }
                                                            //由于可能指向其他site collection，因此需要每次都创建site，web对象
                                                            using (IAveSite tSite = mOMFactory.CreateSite(webUrl))
                                                            {
                                                                using (IAveWeb tWeb = tSite.OpenWeb(AveUrlUtility.GetServerRelativeUrl(webUrl)))
                                                                {
                                                                    IAveList tList = tWeb.GetListFromUrl(calendarUrl);
                                                                    if (tList == null)
                                                                    {
                                                                        log.Warn("Cannot restore this calendar setting, becaust can not find list by url: {0}, web url: {1}, view:{2}, calendar settings: {3}.",calendarUrl,webUrl, view.Title, node.InnerXml ?? string.Empty);
                                                                        continue;
                                                                    }
                                                                    IAveView tView = GetListViewFromUrl(tList, calendarUrl);
                                                                    string listID = tList.ID.ToString("B");
                                                                    Guid viewID = GetListViewId(new Guid(sourceViewId), tList, calendarUrl);
                                                                    string listFormUrl = settingNode.Attributes["ListFormUrl"].Value.StartsWith(SourceSiteInfo.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) ?
                                                                        AveReplaceProcessor.UrlReplace(settingNode.Attributes["ListFormUrl"].Value, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl)
                                                                       : settingNode.Attributes["ListFormUrl"].Value;
                                                                    if (settingNode.Attributes["ListId"] != null && settingNode.Attributes["ListId"].Value != listID)
                                                                    {
                                                                        settingNode.Attributes["ListId"].Value = tList.ID.ToString("B");
                                                                        needUpdate = true;
                                                                    }
                                                                    if (settingNode.Attributes["ViewId"] != null && settingNode.Attributes["ViewId"].Value != viewID.ToString("B"))
                                                                    {
                                                                        settingNode.Attributes["ViewId"].Value = viewID.ToString("B");
                                                                        needUpdate = true;
                                                                    }
                                                                    if (settingNode.Attributes["ListFormUrl"] != null && settingNode.Attributes["ListFormUrl"].Value != listFormUrl)
                                                                    {
                                                                        settingNode.Attributes["ListFormUrl"].Value = listFormUrl;
                                                                        needUpdate = true;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        log.Warn(string.Format("An error occurred while restore one setting of calendar overlay settings list:{0}, view:{1}, CalendarSettings:{2}, error:{3}", list.RootFolder.ServerRelativeUrl, view.Title, node.InnerXml ?? string.Empty, ex.ToString()), ex);
                                                    }
                                                }
                                                if (needUpdate)
                                                {
                                                    view.CalendarSettings = xDoc.InnerXml;
                                                    view.Update();
                                                }
                                            }
                                            catch (AveSecurityTrimingException)
                                            {
                                                throw;
                                            }
                                            catch (Exception e)
                                            {
                                                log.Warn(string.Format("An error occurred while reset view CalendarSettings. list:{0}, view:{1}, CalendarSettings:{2}, error:{3}", list.RootFolder.ServerRelativeUrl, view.Title, view.CalendarSettings, e.ToString()), e);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Warn("An error occurred while restore calendar settings.view id:{0}, error: {1}.", viewId, e);
                                    }
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn(string.Format("An error occurred while RestoreCalendarSettings. error:{0}.", e.ToString()), e);
                        }
                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn(string.Format("An error occurred while dispose web and site. error:{0}", ex.ToString()), ex);
                //qlluo: Post action do not support report, remove it.
                //report.AddDetail(new AveWrapperReportDto("CalendarSettings", "CalendarSettings", AveReportObjectType.RestoreCalendarSettings, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreCalendarSettings + ex.Message));
            }
        }

        public Guid GetListViewId(Guid sourceViewId, IAveList list, string viewUrl)
        {
            Guid destViewId;
            if (!MappingManager.SiteMappingManager.GetViewGuidMappingValue(sourceViewId, out destViewId))
            {
                var view = GetListViewFromUrl(list, viewUrl);
                if (view != null)
                {
                    destViewId = view.ID;
                }
            }
            return destViewId;
        }
        public IAveView GetListViewFromUrl(IAveList list, string viewUrl)
        {
            foreach (IAveView view in list.Views)
            {
                if (view.ServerRelativeUrl.Equals(viewUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return view;
                }
            }
            return null;
        }

        public List<Guid> GetAllWebsGuidByNative(Guid siteId)
        {
            List<Guid> webIds = null;
            try
            {
                webIds = mQueryService.GetAllWebsGuidByNative(siteId);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while get all webs id. error:{0}", e.ToString());
                //mLog.Warn("An error occurred while get all webs id. error:{0}", e.ToString());
            }
            return webIds;
        }

        public List<Guid> GetAllWebsGuid(IAveSite site)
        {
            List<Guid> webIds = new List<Guid>();
            foreach (IAveWeb web in site.AllWebs)
            {
                webIds.Add(web.ID);
                web.Dispose();
            }
            return webIds;
        }
        #endregion

        public virtual void Dispose()
        {
            WFConflictResolution.ParentSite = null;
            if (mCheckoutWeb != null)
            {
                mCheckoutWeb.Dispose();
                mCheckoutWeb = null;
            }
            if (mCheckoutSite != null)
            {
                mCheckoutSite.Dispose();
                mCheckoutSite = null;
            }
            if (mSPSite != null)
            {
                mSPSite.InternalCleanup();
                mSPSite.Dispose();
                mSPSite = null;
            }
            if (mQueryService != null)
            {
                mQueryService.Dispose();
                mQueryService = null;
            }
            if (mWebApplication is IDisposable)
            {
                (mWebApplication as IDisposable).Dispose();
                mWebApplication = null;
            }
            // NOTE:
            // Do NOT dispose mSender and mSqlConn in here,
            // They will be disposed outside AveSPSite
        }

        public string ServerRelativeUrl
        {
            get { return mSPSite.ServerRelativeUrl; }
        }

        public long Size
        {
            get { return 0; }
        }

        public void RestoreDataSourceFields()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreDataSourceFields"))
            {

                try
                {
                    for (int i = 0; i < KpiListIdCol.Count; ++i)
                    {
                        try
                        {
                            string[] idCol = KpiListIdCol[i].Split(new char[] { ':' });
                            using (IAveWeb web = mSPSite.OpenWeb(new Guid(idCol[1])))
                            {
                                IAveList list = web.Lists[new Guid(idCol[0])];
                                foreach (IAveListItem item in list.Items)
                                {
                                    if (item.Fields.TryGetFieldByStaticName("DataSource") != null)
                                    {
                                        string sUrl = item["DataSource"].ToString().Split(new char[] { ',' })[0];
                                        string sDescription = item["DataSource"].ToString().Split(new char[] { ',' })[1].TrimStart();
                                        string dUrl;
                                        if (!MappingManager.SiteMappingManager.GetValueFromListUrlMapping(sUrl, out dUrl))
                                        {
                                            if (!MappingManager.SiteMappingManager.GetValueFromAbsoluteUrlMapping(sUrl, out dUrl))
                                            {
                                                dUrl = sUrl;
                                            }
                                        }
                                        string dDescription;
                                        if (!MappingManager.SiteMappingManager.GetValueFromListUrlMapping(sDescription, out dDescription))
                                        {
                                            if (!MappingManager.SiteMappingManager.GetValueFromAbsoluteUrlMapping(sDescription, out dDescription))
                                            {
                                                dDescription = sDescription;
                                            }
                                        }
                                        if (sUrl == dUrl)
                                        {
                                            string tempUrl;
                                            string tempDes;
                                            if (this.MappingManager.SiteMappingManager.GetValueFromListDefaultViewMapping(sUrl, out tempUrl))
                                            {
                                                dUrl = tempUrl;
                                            }
                                            if (this.MappingManager.SiteMappingManager.GetValueFromListDefaultViewMapping(sDescription, out tempDes))
                                            {
                                                dDescription = tempDes;
                                            }
                                        }
                                        IAveFieldUrlValue urlValue = this.ObjectModelFactory.CreateFieldUrlValue();
                                        urlValue.Url = dUrl;
                                        urlValue.Description = dDescription;
                                        item["DataSource"] = urlValue;
                                        item.SystemUpdate(false);

                                    }
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataSource field.\n error message:{0}", e));
                            //mLog.Warn(e, "An error occur when restore datasource Field");
                        }
                    }
                    var tempListIdToWebIdMapping = MappingManager.SiteMappingManager.GetListIdToWebIdMappingJustForPostAction();
                    if (tempListIdToWebIdMapping.Count > 0)
                    {
                        foreach (var de in tempListIdToWebIdMapping)
                        {
                            using (IAveWeb web = SPSite.OpenWeb(de.Value))
                            {
                                IAveList l = web.Lists[de.Key];
                                foreach (IAveListItem item in l.Items)
                                {
                                    if (item.Fields.ContainsField("ViewGuid") && item["ViewGuid"] != null)
                                    {
                                        Guid viewCuid = new Guid(item["ViewGuid"].ToString());
                                        Guid viewGuidMappingValue;
                                        if (MappingManager.SiteMappingManager.GetViewGuidMappingValue(viewCuid, out viewGuidMappingValue))
                                        {
                                            item["ViewGuid"] = viewGuidMappingValue;
                                        }
                                        item.SystemUpdate(false);
                                    }
                                }
                                l.Update();
                            }
                        }

                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataSource field.\n error message:{0}", ex));
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RestoreDataSourceFields", "RestoreDataSourceFields", AveReportObjectType.RestoreDataSourceFields, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreDataSourceFields + ex.Message));
                }

            }

        }

        public void RestoreUrlIDNeedReplace()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUrlIDNeedReplace"))
            {

                try
                {
                    bool needReplaceLast = false;
                    foreach (Guid webId in UnReplaceUrlIDCache.Keys)
                    {
                        using (IAveWeb web = mSPSite.OpenWeb(webId))
                        {
                            foreach (Guid listId in UnReplaceUrlIDCache[webId].Keys)
                            {
                                IAveList list = web.Lists[listId];
                                if (MappingManager.SiteMappingManager.ContainsKeyForItemIdMapping(listId))
                                {
                                    foreach (int originalItemId in UnReplaceUrlIDCache[webId][listId].Keys)
                                    {

                                        int itemId = MappingManager.SiteMappingManager.GetMappingItemId(listId, originalItemId);
                                        if (itemId != -1)
                                        {
                                            IAveListItem item = list.GetItemById(itemId);
                                            foreach (string fieldName in UnReplaceUrlIDCache[webId][listId][originalItemId])
                                            {
                                                try
                                                {
                                                    object fieldValue = item[fieldName];
                                                    IAveField spField = item.Fields.GetFieldByInternalName(fieldName);
                                                    string newValue = string.Empty;
                                                    switch (spField.TypeAsString.ToUpperInvariant())
                                                    {
                                                        case "URL":
                                                            string urlValue = fieldValue.ToString();
                                                            string url = AveReplaceProcessor.IdReplace(urlValue.Split(new char[] { ',' })[0], MappingManager, ref needReplaceLast);
                                                            string description = AveReplaceProcessor.IdReplace(urlValue.Split(new char[] { ',' })[1], MappingManager, ref needReplaceLast);
                                                            newValue = url + "," + description;
                                                            break;
                                                        case "NOTE":
                                                        case "HTML":
                                                        case "LINK":
                                                        case "IMAGE":
                                                        case "SUMMARYLINKS":
                                                        case "MEDIAFIELDTYPE":
                                                            newValue = AveReplaceProcessor.ReplaceXmlLinks(fieldValue.ToString(), MappingManager, this.SourceSiteInfo, this.ServerRelativeUrl, list, ref needReplaceLast);
                                                            break;
                                                        default:
                                                            newValue = AveReplaceProcessor.IdReplace(fieldValue.ToString(), MappingManager, ref needReplaceLast);
                                                            break;
                                                    }
                                                    item[fieldName] = newValue;
                                                }
                                                catch (AveSecurityTrimingException)
                                                {
                                                    throw;
                                                }
                                                catch (Exception e)
                                                {
                                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReplaceIdInUrlFailed, e);
                                                }

                                                item.SystemUpdate(false);

                                            }
                                            //list.Update();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceIdInUrlFailed, ex);
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RestoreUrlIDNeedReplace", "RestoreUrlIDNeedReplace", AveReportObjectType.RestoreUrlIDNeedReplace, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreUrlIDNeedReplace + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceIdInUrlFailed, e);
                }


            }

        }

        public void RestoreLookupFields(Guid oldId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreLookupFields"))
            {
                IAveWeb web = null;
                try
                {
                    List<AveLookupObject> lookupObjs;
                    if (MappingManager.SiteMappingManager.TryGetValueFromNotUpdateLookupFieldMapping(oldId, out lookupObjs))
                    {
                        foreach (AveLookupObject lookupObj in lookupObjs)
                        {
                            try
                            {
                                if (web == null)
                                {
                                    web = mSPSite.OpenWeb(lookupObj.WebId);
                                }
                                else if (web != null && web.ID != lookupObj.WebId)
                                {
                                    web.Dispose();
                                    web = mSPSite.OpenWeb(lookupObj.WebId);
                                }
                                IAveFieldLookup field = null;
                                if (lookupObj.ListId == Guid.Empty)
                                {
                                    field = web.Fields.GetById(lookupObj.Id) as IAveFieldLookup;
                                }
                                else
                                {
                                    IAveList list = web.Lists.GetById(lookupObj.ListId);
                                    field = list.Fields.GetById(lookupObj.Id) as IAveFieldLookup;
                                }
                                if (field == null)
                                {
                                    continue;
                                }
                                if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                                {
                                    //if (AveTaxonomyField.UpdateTaxonomyFieldLookupProperties(mSPSite, field))
                                    //{
                                    //    try
                                    //    {
                                    //        web.Dispose();
                                    //    }
                                    //    catch { }
                                    //    web = mSPSite.AllWebs[lookupObj.WebId];
                                    //}
                                }
                                else
                                {
                                    Guid listId = Guid.Empty;
                                    MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(lookupObj.SourceListId), out listId);
                                    if (listId.Equals(Guid.Empty))
                                    {
                                        continue;
                                    }
                                    bool needUpdate = false;
                                    if (field.LookupList != listId.ToString("B"))
                                    {
                                        field.LookupList = listId.ToString("B");
                                        needUpdate = true;
                                    }
                                    IAveFieldMapping fieldMapping;
                                    if (MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(listId, out fieldMapping))
                                    {
                                        var mappedName = fieldMapping.GetMappingRestoredFieldInternalName(field.LookupField);
                                        if (!string.IsNullOrEmpty(mappedName) && !string.Equals(field.LookupField, mappedName, StringComparison.Ordinal))
                                        {
                                            field.LookupField = mappedName;
                                            needUpdate = true;
                                        }
                                    }
                                    if (needUpdate)
                                    {
                                        //qlluo: 去掉Remove Version的操作。
                                        //如果Version>0，Remove之后调用Field.Update()会跑异常，Microsoft.SharePoint.SPException: The object has been updated by another user since it was last fetched.
                                        //如果Version=0，Remove相当于什么都没做。
                                        //field.RemoveFieldAttributeValue("Version");
                                        field.RelationshipDeleteBehavior = lookupObj.DeleteBehavior;

                                        field.Update();
                                        if (lookupObj.Sealed)
                                        {
                                            //ADO-186449 Lookup field sealed need update after lookuplist proprty set.
                                            //由于Online无法通过Sealed更新Sealed 值，因此通过更新SchemaXml的方式更新Sealed
                                            field.SchemaXml = Regex.Replace(field.SchemaXml, "Sealed=\"FALSE\"", "Sealed=\"TRUE\"", RegexOptions.IgnoreCase);
                                            field.Update();
                                        }
                                    }
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore lookup field.list title:{0}, webUrl:{1}, field id:{2}\n error message:{3}", lookupObj.ListTitle, lookupObj.WebUrl, lookupObj.Id, ex));
                                //mLog.Warn("An error occurred when restore lookup field, list title: {0}, web url: {1}, field id:{2}. Reason:{3}", lookupObj.ListTitle, lookupObj.WebUrl, lookupObj.ID.ToString(), ex.ToString());
                            }

                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore lookup field. Error message: {0} ", ex);
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("LookupFields", "LookupFields", AveReportObjectType.LookupFields, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreLookupFields + ex.Message));
                }
                finally
                {
                    if (web != null)
                    {
                        web.Dispose();
                    }
                }
            }

        }

        /// <summary>
        /// 用于还原一些需要放到最后还原的url记录
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_auditlogreportstoragelocation:Property of site.")]
        public void RestoreUrlNeedPost()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUrlNeedPost"))
            {
                try
                {
                    foreach (var temp in MappingManager.SiteMappingManager.UrlNeedPostAction)
                    {
                        IAveWeb aveWeb = null;
                        try
                        {
                            if (temp.Key != Guid.Empty)
                            {
                                aveWeb = this.SPSite.OpenWeb(temp.Key);
                            }
                            Dictionary<string, string> urlPropertyMapping = temp.Value;
                            foreach (var property in urlPropertyMapping)
                            {
                                try
                                {
                                    if (property.Key.Equals("PortalUrl", StringComparison.OrdinalIgnoreCase)) //site property
                                    {
                                        SPSite.PortalUrl = AveReplaceProcessor.UrlReplace(property.Value, this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                    }
                                    else if (aveWeb != null) //web property
                                    {
                                        if (property.Key.Equals("SRCH_ENH_FTR_URL", StringComparison.OrdinalIgnoreCase)
                                            || property.Key.Equals("SRCH_ENH_FTR_URL_WEB", StringComparison.OrdinalIgnoreCase)
                                            || property.Key.Equals("SRCH_ENH_FTR_URL_SITE", StringComparison.OrdinalIgnoreCase)
                                            || property.Key.Equals("SRCH_TRAGET_RESULTS_PAGE", StringComparison.OrdinalIgnoreCase))
                                        {
                                            aveWeb.AllProperties[property.Key] = AveReplaceProcessor.UrlReplace(property.Value, this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                        }
                                        else if (property.Key.Equals("_auditlogreportstoragelocation", StringComparison.OrdinalIgnoreCase))
                                        {
                                            aveWeb.AllProperties[property.Key] = AveReplaceProcessor.UrlReplace(property.Value, this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                        }
                                        else if (property.Key.Equals("SRCH_SB_SET_SITE", StringComparison.OrdinalIgnoreCase) || property.Key.Equals("SRCH_SB_SET_WEB", StringComparison.OrdinalIgnoreCase))
                                        {
                                            aveWeb.AllProperties[property.Key] = ReplaceResultsPageUrl(property.Value);
                                        }
                                        else
                                        {
                                            //对于未来知道的还需要处理的url可以放在这里处理
                                        }
                                    }
                                }
                                catch (AveSecurityTrimingException)
                                {
                                    throw;
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while site post replaceUrl. Property:{0}\n Error message:{1}", property.Key, e));
                                }
                            }
                            if (aveWeb != null)
                            {
                                aveWeb.Update();
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while site post replaceUrl. Error message:{0}", e));
                        }
                        finally
                        {
                            if (aveWeb != null)
                            {
                                aveWeb.Dispose();
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore url need post. ", ex));
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RestoreUrlNeedPost", "RestoreUrlNeedPost", AveReportObjectType.RestoreUrlNeedPost, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreUrlNeedPost + ex.Message));
                }
            }
        }

        private string ReplaceResultsPageUrl(string value)
        {
            //Example:==>  {"Inherit":false,"ResultsPageAddress":"http://sid-vm50/sites/search/Pages/results.aspx","ShowNavigation":false}
            var realValue = value.Trim('{', '}').Split(',').Where(line => line.Contains(':')).ToDictionary(line => line.Substring(0, line.IndexOf(':')).Trim('"'), line => line.Substring(line.IndexOf(':') + 1).Trim('"'));
            if (realValue.ContainsKey("Inherit") && realValue["Inherit"].Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
            {
                if (realValue.ContainsKey("ResultsPageAddress") && !string.IsNullOrEmpty(realValue["ResultsPageAddress"]))
                {
                    var mappedUrl = AveReplaceProcessor.UrlReplace(realValue["ResultsPageAddress"], this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                    if (!realValue["ResultsPageAddress"].Equals(mappedUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        value = value.Replace(realValue["ResultsPageAddress"], mappedUrl);
                    }
                }
            }
            return value;
        }

        public void RestoreMasterPageProperty()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreMasterPageProperty"))
            {

                //if (!AveSPEnv.IsMoss)
                //{
                //    return;
                //}
                try
                {
                    foreach (Guid key in MappingManager.SiteMappingManager.WebMastPageMapping.Keys)
                    {
                        try
                        {
                            using (IAveWeb web = SPSite.OpenWeb(key))
                            {
                                AveWebMasterPageInfo pageInfo = MappingManager.SiteMappingManager.WebMastPageMapping[key];
                                string destPageUrl = string.Empty;
                                if (!string.IsNullOrEmpty(pageInfo.PageUrl))
                                {
                                    destPageUrl = AveReplaceProcessor.UrlReplace(pageInfo.PageUrl, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), MappingManager.SiteMappingManager.SourceSiteInfo, this.ServerRelativeUrl);
                                }

                                this.Publishing.SetWebMasterPageInfo(pageInfo, web, destPageUrl, !NotRestoreWebCss);
                                if (web.IsRootWeb)
                                {
                                    SPSite.RootWeb.ReloadWeb();
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while set web master page property. \n error message:{0}", e));
                            //mLog.Warn("An error occurred while setting web master page property,Exception:{0}", e.ToString());
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while set web master page property. ", ex));
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RestoreMasterPageProperty", "RestoreMasterPageProperty", AveReportObjectType.RestoreMasterPageProperty, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreMasterPageProperty + ex.Message));
                }


            }

        }

        public void RestoreUnRestoreWebPart(IReport report)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUnRestoreWebPart"))
            {

                try
                {
                    var unRestoreWebPartCache = MappingManager.SiteMappingManager.GetUnRestoreWebPartCacheForSitePostAction();
                    foreach (var valuePair in unRestoreWebPartCache)
                    {
                        isSitePostRestoreWebPart = true;
                        try
                        {
                            var listIdKey = valuePair.Key;
                            foreach (Guid webIdKey in valuePair.Value.Keys)
                            {
                                IAveWeb web = null;
                                try
                                {
                                    web = mSPSite.OpenWeb(webIdKey);
                                    foreach (KeyValuePair<string, List<object>> pair in valuePair.Value[webIdKey])
                                    {
                                        AveSPDoc spDoc = new AveSPDoc(this);
                                        try
                                        {
                                            IAveFile file = web.GetFile(pair.Key);
                                            int userId = -1;
                                            if (this.QueryService != null && this.QueryService.IsCheckOutFile(mSPSite.ID, file.UniqueId, ref userId) && userId != web.CurrentUser.ID)
                                            {
                                                IAveUser checkOutUser = null;
                                                try
                                                {
                                                    checkOutUser = web.SiteUsers.GetByID(userId);
                                                }
                                                catch (AveSecurityTrimingException)
                                                {
                                                    throw;
                                                }
                                                catch (Exception e)
                                                {
                                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetCheckOutUserFailed, e);
                                                }
                                                if (checkOutUser != null)
                                                {
                                                    file = (mSPSite.GetCheckoutWeb(mSPSite.ID, web, checkOutUser, file.UniqueId).GetFile(file.UniqueId));
                                                }
                                            }
                                            spDoc.SPFile = file;
                                            spDoc.SPWeb = file.Web;
                                            spDoc.SetRestoreOption(RestoreOption);

                                            #region get document views from cache
                                            Guid parentListId = spDoc.SPFile.ParentFolder != null ? spDoc.SPFile.ParentFolder.ParentListId : Guid.Empty;
                                            Dictionary<Guid, Guid> viewMapping;
                                            if (parentListId != Guid.Empty && MappingManager.SiteMappingManager.ListViewMapping.TryGetValue(parentListId, out viewMapping))
                                            {
                                                foreach (var map in viewMapping)
                                                {
                                                    spDoc.AveView.Views.Add(map.Key, map.Value);
                                                }
                                            }
                                            #endregion
                                            spDoc.RestoreWebPart(pair.Value, false);
                                        }
                                        catch (AveSecurityTrimingException)
                                        {
                                            throw;
                                        }
                                        catch (Exception e)
                                        {
                                            log.Warn("An error occurred while restore an webPart in site post action. error:{0}", e.ToString());
                                        }
                                        finally
                                        {
                                            report.AddDetails(spDoc.GetReport().GetDetails());
                                        }
                                    }
                                }
                                catch (AveSecurityTrimingException)
                                {
                                    throw;
                                }
                                catch (Exception e)
                                {
                                    log.Warn("An error occurred while restore webPart in site post action.error:{0}", e.ToString());
                                }
                                finally
                                {
                                    if (web != null)
                                        web.Dispose();
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore webPart in site post action.error:{0}", ex.ToString());
                    report.AddDetail(new AveWrapperWebpartReportDto("RestoreWebPar", "RestoreWebPar", null, string.Empty, string.Empty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebPart, ex.Message));
                }

            }

        }

        //public void RestoreMasterPageProperty()
        //{
        //    if (!AveSPEnv.IsMoss)
        //    {
        //        return;
        //    }
        //    foreach (Guid key in WebMastPageMapping.Keys)
        //    {
        //        try
        //        {
        //            using (IAveWeb web = SPSite.OpenWeb(key))
        //            {
        //                AveWebMasterPageInfo pageInfo = WebMastPageMapping[key];
        //                AvePublishing.SetWebMasterPageInfo(pageInfo, web, SiteManagedMappings);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while set web master page property. \n error message:{0}", e));
        //            //mLog.Warn("An error occurred while setting web master page property,Exception:{0}", e.ToString());
        //        }
        //    }
        //}

        public void RestoreLookupFieldValues()
        {
            IAveWeb originalWeb = null;
            IAveList originalList = null;
            try
            {
                Guid[] keys = MappingManager.SiteMappingManager.GetLookupFieldValuesMappingKeys();
                foreach (Guid key in keys)
                {
                    RestoreLookupFieldValues(key, ref originalWeb, ref originalList);
                }
            }
            finally
            {
                if (originalWeb != null)
                {
                    originalWeb.Dispose();
                }
            }
        }

        public void RestoreSocialRatingInfo()
        {
            var ratingInfoCache = MappingManager.SiteMappingManager.SocialRatingCache;
            if (!AveEnv.IsMoss
                || this.SPContextKind == AveContextKind.ClientObjectModel
                || this.SPContextKind == AveContextKind.Server07ObjectModel
                || ratingInfoCache == null)
            {
                return;
            }
            AveSPUserProfile userProfile;
            foreach (var cache in ratingInfoCache)
            {
                try
                {
                    userProfile = new AveSPUserProfile(this, cache.Key, true);
                    userProfile.RestoreRating(cache.Value);
                }
                catch (Exception e)
                {
                    log.Error("Restore rating failed. User: {0}. Error: {1}", cache.Key, e);
                }
            }
            MappingManager.SiteMappingManager.ClearRatingCache();
        }

        public void RestoreRelatedItemsValue()
        {
            Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> relatedItemsCache = MappingManager.SiteMappingManager.GetRelatedItemsCacheMappingOnlyForPostAction();
            foreach (var pair in relatedItemsCache)
            {
                RestoreRelatedItemsValue(pair.Key, pair.Value);
            }
            relatedItemsCache.Clear();
        }

        public void RestoreRelatedItemsValue(Guid webId, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>> listIdPair)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreRelatedItemsValue"))
            {

                IAveWeb web = null;
                try
                {
                    web = mSPSite.OpenWeb(webId);
                    IAveList list = null;
                    IAveField field = null;
                    foreach (Guid listId in listIdPair.Keys)
                    {
                        try
                        {
                            list = web.Lists.GetById(listId);
                            field = list.Fields.GetById(new Guid("d2a04afc-9a05-48c8-a7fa-fa98f9496141"));
                            IAveListItem item = null;
                            Dictionary<int, Dictionary<int, string>> rowIdPair = listIdPair[listId];
                            foreach (int rowId in rowIdPair.Keys)
                            {
                                try
                                {
                                    item = list.GetItemById(rowId);
                                    Dictionary<int, string> versionPair = rowIdPair[rowId];
                                    foreach (int version in versionPair.Keys)
                                    {
                                        try
                                        {
                                            string newSchema = ProceeRelatedItemsSchema(versionPair[version]);
                                            item[field.InternalName] = newSchema;
                                            item.SystemUpdate();
                                        }
                                        catch (Exception e)
                                        {
                                            log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the Related Items value . Web Id {0} List Id {1} Item Id {2} version :{3}\n error message:{4}", webId, listId, rowId, version, e));
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the Related Items value . Web Id {0} List Id {1} Item id:{2}\n error message:{3}", webId, listId, rowId, e));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the Related Items value in list. Web Id {0} List id:{1}\n error message:{2}", webId, listId, e));
                        }
                    }


                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the Related Items value in SPWeb. web id:{0}\n error message:{1}", webId, e));
                }
                finally
                {
                    if (web != null)
                    {
                        web.Dispose();
                    }
                }

            }

        }

        /// <summary>
        /// 替换RelatedItems Schema中的WebId，ListId，ItemId
        /// </summary>
        /// <param name="schemas"></param>
        /// <returns></returns>
        private string ProceeRelatedItemsSchema(string schemas)
        {
            if (string.IsNullOrEmpty(schemas))
                return string.Empty;

            StringBuilder sb = new StringBuilder("[");
            string[] schemaDic = schemas.Split('}');
            foreach (string value in schemaDic)
            {
                string webId = string.Empty;
                string listId = string.Empty;
                string itemId = string.Empty;

                int webIndex = value.IndexOf("\"WebId\"", StringComparison.OrdinalIgnoreCase);
                int listIndex = value.IndexOf("\"ListId\"", StringComparison.OrdinalIgnoreCase);
                int itemIndex = value.IndexOf("\"ItemId\"", StringComparison.OrdinalIgnoreCase);

                if (webIndex > 0 && listIndex > 0 && itemIndex > 0)
                {
                    webId = value.Substring(webIndex + 9, 36);
                    listId = value.Substring(listIndex + 10, 36);
                    itemId = value.Substring(itemIndex + 9, (value.Substring(Convert.ToInt32(itemIndex)).IndexOf(',') - value.Substring(Convert.ToInt32(itemIndex)).IndexOf(':') - 1));

                    var newListId = Guid.Empty;
                    if (MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(new Guid(webId)) &&
                        MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(listId), out newListId))
                    {
                        int tempItemId = MappingManager.SiteMappingManager.GetMappingItemId(newListId, Convert.ToInt32(itemId));
                        if (tempItemId != -1)
                        {
                            webId = MappingManager.SiteMappingManager.WebIDMapping[new Guid(webId)].ToString();
                            listId = newListId.ToString();
                            itemId = tempItemId.ToString();
                        }
                    }

                    sb.Append(string.Format("{0}\"ItemId\":{1},\"WebId\":\"{2}\",\"ListId\":\"{3}\"{4},", "{", itemId, webId, listId, "}"));
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
            return sb.ToString();
        }

        public void RestoreUnrestoredWebParts()
        {

        }

        /// <summary>
        /// 调用者需要dispose parentweb
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="parentWeb"></param>
        /// <param name="parentList"></param>
        public void RestoreLookupFieldValues(Guid ID, ref IAveWeb parentWeb, ref IAveList parentList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreLookupFieldValues"))
            {
                try
                {
                    Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>> tempLookupFieldValuesMapping = null;
                    if (MappingManager.SiteMappingManager.GetLookupFieldValuesMapping(ID, out tempLookupFieldValuesMapping))
                    {
                        foreach (Guid webId in tempLookupFieldValuesMapping.Keys)
                        {
                            try
                            {
                                var webValueDic = MappingManager.SiteMappingManager.GetLookupFieldValuesMappingWebValuesDictionary(tempLookupFieldValuesMapping, webId);
                                if (parentWeb == null || parentWeb.ID != webId)
                                {
                                    parentWeb = mSPSite.OpenWeb(webId);
                                }
                                foreach (Guid listId in webValueDic.Keys)
                                {
                                    var listValueDic = MappingManager.SiteMappingManager.GetLookupFieldValuesMappingListValuesDictionary(webValueDic, listId);
                                    Dictionary<Guid, Tuple<IAveWeb, IAveList>> objCache = new Dictionary<Guid, Tuple<IAveWeb, IAveList>>();
                                    try
                                    {
                                        if (parentList == null || listId != parentList.ID)
                                        {
                                            parentList = parentWeb.Lists.GetById(listId);
                                        }
                                        foreach (int itemId in listValueDic.Keys)
                                        {
                                            if (itemId <= 0)
                                            {
                                                continue;
                                            }
                                            try
                                            {
                                                Dictionary<int, Dictionary<Guid, object>> itemValueDic = MappingManager.SiteMappingManager.GetLookupFiledValuesMappingItemValuesDictionary(listValueDic, itemId);
                                                IAveListItem item = parentList.GetItemById(itemId);
                                                int itemUIVersion = new AveSPItem(this).GetCurrentUIVersion(mSPSite.ID, item);
                                                AveFileLevel itemLevel = AveFileLevel.Published;
                                                #region 使用API获取的Item，如果document当前versioncheckout的话，无法获取checkout状态，只能使用file的状态来判断
                                                if (item.File != null)
                                                {
                                                    itemLevel = (item.File.CheckOutType != AveCheckOutType.None) ? AveFileLevel.Checkout : AveFileLevel.Published;
                                                }
                                                else
                                                {
                                                    itemLevel = item.Level;
                                                }
                                                if (itemLevel == AveFileLevel.Checkout && this.SPContextKind != AveContextKind.ClientObjectModel)
                                                {
                                                    if (mSPSite.CompatibilityLevel == 15 && this.mSPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                                                    {
                                                        log.Log(AveLogLevel.WARN, "Skip to restore lookup column value of checkout item because of lack of permission. Item Url: {0}", item.Url);
                                                        continue;
                                                    }
                                                }
                                                #endregion
                                                bool needUpdate = false;
                                                foreach (int version in itemValueDic.Keys)
                                                {
                                                    if (version < itemUIVersion && this.SPContextKind != AveContextKind.ClientObjectModel)
                                                    {
                                                        if (mSPSite.CompatibilityLevel == 15 && this.mSPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                                                        {
                                                            log.Log(AveLogLevel.WARN, "Skip to restore lookup column value of item version because of lack of permission. Item Url: {0}", item.Url);
                                                            continue;
                                                        }
                                                    }
                                                    foreach (Guid fieldId in MappingManager.SiteMappingManager.GetLookupFiledValuesMappingVersionValuesDictionary(itemValueDic, version).Keys)
                                                    {
                                                        try
                                                        {
                                                            IAveFieldLookup field = parentList.Fields.GetById(fieldId) as IAveFieldLookup;
                                                            if (string.IsNullOrEmpty(field.LookupList))
                                                            {
                                                                log.Warn("Failed restoring look field value, because lookup list property is null. field title: {0}", field.Title);
                                                                continue;
                                                            }
                                                            Guid lookupListId = new Guid(field.LookupList);
                                                            IAveWeb lookupWeb = parentWeb;
                                                            IAveList lookupList;
                                                            Tuple<IAveWeb, IAveList> tmp;
                                                            if (!objCache.TryGetValue(fieldId, out tmp))
                                                            {
                                                                if (field.LookupWebId != parentWeb.ID)
                                                                {
                                                                    lookupWeb = this.SPSite.OpenWeb(field.LookupWebId);
                                                                }
                                                                lookupList = lookupWeb.GetList(lookupListId);
                                                                objCache.Add(fieldId, new Tuple<IAveWeb, IAveList>(lookupWeb, lookupList));
                                                            }
                                                            else
                                                            {
                                                                lookupWeb = tmp.Item1;
                                                                lookupList = tmp.Item2;
                                                            }
                                                            object sourceValue = itemValueDic[version][fieldId];
                                                            ArrayList valueList = sourceValue as ArrayList;
                                                            AveXmlField xmlField;
                                                            bool isMapped = false;
                                                            if (xmlFieldCache.TryGetValue(field.ID, out xmlField))
                                                            {
                                                                valueList = GetLookupIdByMapping(parentList.ID, item, xmlField, field, sourceValue, parentList);
                                                                isMapped = true;
                                                            }
                                                            if (valueList != null && valueList.Count != 0)
                                                            {

                                                                if (!field.AllowMultipleValues && valueList.Count == 1)
                                                                {
                                                                    int lookupItemId;
                                                                    if (!isMapped)
                                                                    {
                                                                        lookupItemId = GetLookupItemId(valueList[0].ToString(), field.LookupWebId, lookupList.ID, field.LookupField);
                                                                    }
                                                                    else
                                                                    {
                                                                        lookupItemId = Convert.ToInt32(valueList[0]);
                                                                    }
                                                                    //365只有current version会用api update，其他version不去update；
                                                                    //local只有是current version且非checkout状态时用api update，其他情况用SQL update。
                                                                    if (itemUIVersion == version && (itemLevel != AveFileLevel.Checkout || this.SPContextKind == AveContextKind.ClientObjectModel))
                                                                    {
                                                                        if (lookupItemId <= 0)
                                                                        {
                                                                            log.Debug("do not update lookup column value in post action, rowid: {0}", lookupItemId);
                                                                        }
                                                                        else
                                                                        {
                                                                            item[fieldId] = lookupItemId.ToString();
                                                                            needUpdate = true;
                                                                        }
                                                                    }
                                                                    else if (this.SPContextKind != AveContextKind.ClientObjectModel)
                                                                    {
                                                                        new AveSPItem(this).UpdateColumnByNative(SPSite.ID, item, version, field.RowOrdinal, field.ColName, lookupItemId);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    IAveFieldLookupValueCollection lookupCol = mOMFactory.CreateFieldLookupValueCollection();
                                                                    foreach (var value in valueList)
                                                                    {
                                                                        int lookupItemId;
                                                                        if (!isMapped)
                                                                        {
                                                                            lookupItemId = GetLookupItemId(value.ToString(), field.LookupWebId, lookupList.ID, field.LookupField);
                                                                        }
                                                                        else
                                                                        {
                                                                            lookupItemId = Convert.ToInt32(value);
                                                                        }
                                                                        if (lookupItemId <= 0)
                                                                        {
                                                                            log.Debug("do not add rowid to multi lookup column value, rowid: {0}", lookupItemId);
                                                                        }
                                                                        else
                                                                        {
                                                                            lookupCol.Add(mOMFactory.CreateFieldLookupValue(lookupItemId, "Title"));
                                                                        }
                                                                    }
                                                                    if (lookupCol.Count == 0)
                                                                    {
                                                                        continue;
                                                                    }
                                                                    if (itemUIVersion == version && (itemLevel != AveFileLevel.Checkout || this.SPContextKind == AveContextKind.ClientObjectModel))
                                                                    {
                                                                        item[fieldId] = lookupCol;
                                                                        needUpdate = true;
                                                                    }
                                                                    else if (this.SPContextKind != AveContextKind.ClientObjectModel)
                                                                    {
                                                                        List<int> values = new List<int>();
                                                                        foreach (IAveFieldLookupValue value in lookupCol)
                                                                        {
                                                                            values.Add(value.LookupId);
                                                                        }
                                                                        if (item.Level == AveFileLevel.Checkout)
                                                                        {
                                                                            new AveSPItem(this).RemoveDatajunctionByNative(item, fieldId, listId, version);
                                                                        }
                                                                        new AveSPItem(this).CreateDatajunctionByNative(item, fieldId, listId, version, values);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            log.Log(AveLogLevel.DEBUG, "An exception occurred while get a version lookup column value. exception:{0}", e.ToString());
                                                        }
                                                    }
                                                }

                                                if (needUpdate)
                                                {
                                                    log.Debug("Update item modified time for post action");
                                                    item["Modified"] = item["Modified"];
                                                    item.SystemUpdate(false);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Log(AveLogLevel.WARN, "An exception occurred while update item lookup column value. exception:{0}", ex.ToString());
                                            }
                                        }
                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception le)
                                    {
                                        log.Warn("An error occurred when restore the lookup field value in list. web id:{0}, list id:{1}\n error message:{2}", webId, listId, le);
                                    }
                                    finally
                                    {
                                        if (objCache.Count > 0)
                                        {
                                            foreach (var cache in objCache.Values)
                                            {
                                                //当前Web对象不能被释放，否则会对后面有影响
                                                if (cache.Item1 != null & cache.Item1.ID != parentWeb.ID)
                                                {
                                                    cache.Item1.Dispose();
                                                }
                                            }
                                            objCache.Clear();
                                        }
                                    }
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception we)
                            {
                                log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the lookup field value in SPWeb. web id:{0}\n error message:{1}", webId, we));
                                //mLog.Warn("Error happenned when restore the lookup field value in SPWeb, id: {0}. Reason: {1}", webId, we.ToString());
                            }
                        }
                        MappingManager.SiteMappingManager.RemoveNotUpdateLookupFieldValue(ID);
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the lookup field value in SPWeb. ", ex));
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("RestoreLookupFieldValues", "RestoreLookupFieldValues", AveReportObjectType.RestoreLookupFieldValues, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreLookupFieldValues + ex.Message));
                }


            }

        }

        private int GetLookupItemId(string singleValue, Guid lookupWebId, Guid lookupListId, string lookupFieldName)
        {
            Guid itemGuid;
            string itemLeafName;
            string itemColumnValueDisplayName;
            int sourceItemId = ResolveLookupFieldValue(singleValue, out itemGuid, out itemLeafName, out itemColumnValueDisplayName);
            int lookupItemId = GetLookupIdByItemMapping(lookupListId, sourceItemId);
            if (lookupItemId <= 0 && itemGuid != Guid.Empty)
            {
                lookupItemId = GetLookupIdByGUID(lookupWebId, lookupListId, itemGuid);
            }
            if (lookupItemId <= 0 && !String.IsNullOrEmpty(itemLeafName))
            {
                string leafNameFieldName = "FileLeafRef";
                lookupItemId = GetLookupIdByFieldNameAndDisplayValue(lookupWebId, lookupListId, leafNameFieldName, itemLeafName);
            }
            if (lookupItemId <= 0 && !String.IsNullOrEmpty(lookupFieldName) && !String.IsNullOrEmpty(itemColumnValueDisplayName))
            {
                lookupItemId = GetLookupIdByFieldNameAndDisplayValue(lookupWebId, lookupListId, lookupFieldName, itemColumnValueDisplayName);
            }
            if (lookupItemId <= 0 && SetLookupFieldSourceValue)
            {
                lookupItemId = sourceItemId;
            }
            return lookupItemId;
        }

        private int GetLookupIdByGUID(Guid lookupWebId, Guid lookupListId, Guid tpGuid)
        {
            if (mQueryService != null)
            {
                return mQueryService.GetLookupIdByGUID(this.SPSite.ID, lookupListId, tpGuid);
            }
            else if (lookupWebId != Guid.Empty)
            {
                int itemId = GetLookupItemIdAndGuid(lookupWebId, lookupListId, tpGuid, true);
                if (itemId <= 0)
                {
                    log.Debug("Can not find the Lookup Item RowId by Item TPGuid. LookupWebId: {0}, LookupListId: {1}, ItemTPGuid: {2}", lookupWebId, lookupListId, tpGuid);
                }
                return itemId;
            }
            else
            {
                return -1;
            }
        }

        private int GetLookupIdByFieldNameAndDisplayValue(Guid lookupWebId, Guid lookupListId, String lookupColumnName, String itemLookupColumnDisplayValue)
        {
            if (lookupWebId != Guid.Empty)
            {
                int itemId = GetLookupItemIdByDisplayValue(lookupWebId, lookupListId, lookupColumnName, itemLookupColumnDisplayValue);
                if (itemId == -1)
                {
                    log.Debug("Can not find the Lookup Item RowId by lookup column name and display value. LookupWebId: {0}, LookupListId: {1}, FieldDisplayName: {2}, FieldValue: {3}", lookupWebId, lookupListId, lookupColumnName, itemLookupColumnDisplayValue);
                }
                return itemId;
            }
            else
            {
                return -1;
            }
        }

        private int ResolveLookupFieldValue(string singleValue, out Guid itemGuid, out string itemLeafName, out string itemColumnValueDisplayName)
        {
            bool needRestoreLookupItemByLookupValue = false;
            itemGuid = Guid.Empty;
            itemLeafName = String.Empty;
            itemColumnValueDisplayName = String.Empty;
            if (singleValue.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                singleValue = singleValue.TrimEnd('*');
                needRestoreLookupItemByLookupValue = true;
            }
            //var leafNameIndex = singleValue.LastIndexOf('&');
            var leafNameIndex = singleValue.IndexOf("&leafName&");
            //value中包含&说明备份了itemLeafName,只有lookup List为Document Library才可能会备份
            if (leafNameIndex > 0)
            {
                //itemLeafName = singleValue.Substring(leafNameIndex + 1);
                itemLeafName = singleValue.Substring(leafNameIndex + 10);
                singleValue = singleValue.Substring(0, leafNameIndex);
            }
            //var guidIndex = singleValue.IndexOf('#');
            var guidIndex = singleValue.IndexOf("#GUID#");
            //value中包含#说明备份了TPGuid
            //if (guidIndex >= 0 && AveTypeHelper.IsGuid(singleValue.Substring(guidIndex + 1)))
            if (guidIndex >= 0 && AveTypeHelper.IsGuid(singleValue.Substring(guidIndex + 6)))
            {
                //itemGuid = new Guid(singleValue.Substring(guidIndex + 1));
                itemGuid = new Guid(singleValue.Substring(guidIndex + 6));
                singleValue = singleValue.Substring(0, guidIndex);
            }
            var idIndex = singleValue.IndexOf(';');
            var idStr = idIndex >= 0 ? singleValue.Substring(0, idIndex) : singleValue;
            int itemId = String.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
            if (needRestoreLookupItemByLookupValue)
            {
                itemColumnValueDisplayName = singleValue.Substring(singleValue.IndexOf(';') + 1);
            }
            return itemId;
        }
        private int GetLookupIdByItemMapping(Guid lookupListId, int sourceItemId)
        {
            Dictionary<int, int> itemMapping = null;
            int desItemId = -1;
            if (MappingManager.SiteMappingManager.GetValueFromItemIdMapping(lookupListId, out itemMapping))
            {
                itemMapping.TryGetValue(sourceItemId, out desItemId);
            }
            return desItemId;
        }

        private int GetSourceLookupIdByItemMapping(Guid lookupListId, int id)
        {
            Dictionary<int, int> itemMapping = null;
            if (MappingManager.SiteMappingManager.GetValueFromItemIdMapping(lookupListId, out itemMapping) && itemMapping != null)
            {
                var mappingValuePair = itemMapping.FirstOrDefault(valuePair => valuePair.Value == id);
                if (mappingValuePair.Key != 0)
                {
                    return mappingValuePair.Key;
                }
                else
                {
                    if (!SetLookupFieldSourceValue)
                    {
                        return -1;
                    }
                }
            }
            return id;
        }
        private int GetItemIdByCacheValue(object value)
        {
            string temp = value != null ? value.ToString() : string.Empty;
            int itemId;
            if (temp.Contains(";"))
            {
                var tempSubStrs = temp.Split(new string[] { ";" }, StringSplitOptions.None);
                itemId = Convert.ToInt32(tempSubStrs[0]);
            }
            else if (!Int32.TryParse(temp, out itemId))
            {
                itemId = -1;
            }
            return itemId;
        }

        private void ReloadLookupListItemIdAndDisplayValueCache(Guid lookupWebId, Guid lookupListId, string fieldName, ref Dictionary<Guid, Dictionary<string, Dictionary<string, Dictionary<int, Guid>>>> listLevelItemCache, ref Dictionary<string, Dictionary<string, Dictionary<int, Guid>>> fieldLevelItemCache, ref Dictionary<string, Dictionary<int, Guid>> itemLevelItemCache, string displayValue)
        {
            if (listLevelItemCache == null)
            {
                listLevelItemCache = new Dictionary<Guid, Dictionary<string, Dictionary<string, Dictionary<int, Guid>>>>();
            }
            if (fieldLevelItemCache == null)
            {
                fieldLevelItemCache = new Dictionary<string, Dictionary<string, Dictionary<int, Guid>>>();
            }
            if (itemLevelItemCache == null)
            {
                itemLevelItemCache = new Dictionary<string, Dictionary<int, Guid>>();
            }
            using (var web = this.SPSite.OpenWeb(lookupWebId))
            {
                var list = web.GetList(lookupListId);
                var field = list.Fields.GetFieldByInternalName(fieldName);
                if (!string.IsNullOrEmpty(displayValue))
                {
                    foreach (var item in list.Items.Where(item => CompareColumnValue(web, field, item, displayValue)))
                    {
                        itemLevelItemCache[displayValue] = new Dictionary<int, Guid> { { item.ID, item.UniqueId } };
                        break;//找到一个即break,不用遍历所有。
                    }
                    foreach (var folder in list.Folders.Where(folder => CompareColumnValue(web, field, folder, displayValue)))
                    {
                        itemLevelItemCache[displayValue] = new Dictionary<int, Guid> { { folder.ID, folder.UniqueId } };
                        break;
                    }
                    fieldLevelItemCache[fieldName] = itemLevelItemCache;
                    listLevelItemCache[lookupListId] = fieldLevelItemCache;
                    lookupListItemIdAndDisplayValueCache[lookupWebId] = listLevelItemCache;
                }
            }
        }
        private bool CompareColumnValue(IAveWeb web, IAveField field, IAveListItem item, string displayValue)
        {
            var itemFieldValue = item[field.InternalName];
            if (itemFieldValue == null)
            {
                return false;
            }

            if (field.Type == AveFieldType.DateTime)
            {
                try
                {
                    DateTime sourceTime = DateTime.Parse(displayValue);
                    DateTime itemTime = (DateTime)itemFieldValue;
                    if (itemTime.Kind == DateTimeKind.Utc)//目的端如果是O365,API获取到的时间是UTC时间。 需要将源端时间根据时区转换之后再比较。
                    {
                        sourceTime = web.RegionalSettings.TimeZone.LocalTimeToUTC(sourceTime);
                        return string.Equals(itemTime.ToString(), sourceTime.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        return string.Equals(itemTime.ToString(), sourceTime.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while comparing date time value. SourceValue: {0}, Error: {1}", displayValue, e);
                }
            }
            return itemFieldValue.ToString().Equals(displayValue, StringComparison.OrdinalIgnoreCase);
        }

        public Dictionary<int, Guid> GetLookupItemIdAndUniqueIdByDisplayValue(Guid lookupWebId, Guid lookupListId, string fieldName, string displayValue)
        {
            var result = new Dictionary<int, Guid>();
            try
            {
                lock (lookupListItemIdAndDisplayValueCache)
                {
                    Dictionary<Guid, Dictionary<string, Dictionary<string, Dictionary<int, Guid>>>> listLevelItemCache = null;
                    Dictionary<string, Dictionary<string, Dictionary<int, Guid>>> fieldLevelItemCache = null;
                    Dictionary<string, Dictionary<int, Guid>> itemLevelItemCache = null;
                    if (!lookupListItemIdAndDisplayValueCache.TryGetValue(lookupWebId, out listLevelItemCache) ||
                        !listLevelItemCache.TryGetValue(lookupListId, out fieldLevelItemCache) ||
                        !fieldLevelItemCache.TryGetValue(fieldName, out itemLevelItemCache) ||
                        !itemLevelItemCache.TryGetValue(displayValue, out result))
                    {
                        ReloadLookupListItemIdAndDisplayValueCache(lookupWebId, lookupListId, fieldName, ref listLevelItemCache, ref fieldLevelItemCache, ref itemLevelItemCache, displayValue);
                        itemLevelItemCache.TryGetValue(displayValue, out result);

                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Can not cache the Lookup ListItem Guid. WebId: {0}, ListId: {1}, field name: {2}, Error Message: {3}", lookupWebId, lookupListId, fieldName, e);
            }
            return result;
        }

        public int GetLookupItemIdByDisplayValue(Guid lookupWebId, Guid lookupListId, string fieldName, string displayValue)
        {
            return GetLookupItemIdAndUniqueIdByDisplayValue(lookupWebId, lookupListId, fieldName, displayValue).Keys.First();
        }

        public int GetLookupItemIdAndGuid(Guid lookupWebId, Guid lookupListId, Guid tpGuid, bool isPostAction = false)
        {
            try
            {
                lock (lookupListItemIdAndGuidCache)
                {
                    Dictionary<Guid, Dictionary<Guid, int>> listLevelItemCache = null;
                    Dictionary<Guid, int> itemLevelItemCache = null;
                    if (!lookupListItemIdAndGuidCache.TryGetValue(lookupWebId, out listLevelItemCache)
                        || !lookupListItemIdAndGuidCache[lookupWebId].TryGetValue(lookupListId, out itemLevelItemCache)
                        || (isPostAction && !PostReloadLookupListCache.Contains(lookupListId) && !itemLevelItemCache.ContainsKey(tpGuid)))
                    {
                        if (isPostAction)
                        {
                            PostReloadLookupListCache.Add(lookupListId);
                        }
                        itemLevelItemCache = new Dictionary<Guid, int>();
                        using (IAveWeb web = this.SPSite.OpenWeb(lookupWebId))
                        {
                            var list = web.GetList(lookupListId);
                            foreach (var item in list.Items)
                            {
                                itemLevelItemCache[item.GetTPGuid()] = item.ID;
                            }
                            foreach (var folder in list.Folders)
                            {
                                itemLevelItemCache[folder.GetTPGuid()] = folder.ID;
                            }
                            if (listLevelItemCache == null)
                            {
                                listLevelItemCache = new Dictionary<Guid, Dictionary<Guid, int>>();
                            }
                            listLevelItemCache[lookupListId] = itemLevelItemCache;
                            lookupListItemIdAndGuidCache[lookupWebId] = listLevelItemCache;
                        }
                    }
                    int itemId;
                    if (itemLevelItemCache.TryGetValue(tpGuid, out itemId))
                    {
                        return itemId;
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Can not cache the Lookup ListItem Guid. WebId: {0}, ListId: {1}, Error Message: {2}", lookupWebId, lookupListId, e);
            }
            return -1;
        }

        private ArrayList GetLookupIdByMapping(Guid listId, IAveListItem item, AveXmlField xmlField, IAveFieldLookup field, object value, IAveList lookupList)
        {
            ArrayList itemIds = new ArrayList();
            IAveFieldMapping fieldMapping;
            if (this.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(listId, out fieldMapping))
            {
                int mappingItemId;
                if (lookupList != null)
                {
                    var tempItemId = GetSourceLookupIdByItemMapping(lookupList.ID, item.ID);
                    if (tempItemId > 0)
                    {
                        mappingItemId = tempItemId;
                    }
                    else
                    {
                        mappingItemId = item.ID;
                    }
                }
                else
                {
                    mappingItemId = item.ID;
                }
                AveSourceFieldValueInfo valueInfo = new AveSourceFieldValueInfo { SourceItemName = item.Name, SourceItemRowId = mappingItemId };
                valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                {
                    SourceDisplayName = xmlField.Title,
                    SourceInternalName = xmlField.FieldInternalName,
                    SourceType = xmlField.Type,
                    SourceTypeAsString = xmlField.TypeAsString
                };
                if (xmlField.CustomFieldInfo is AveCustomLookupFieldInfo)
                {
                    valueInfo.SplitString = (xmlField.CustomFieldInfo as AveCustomLookupFieldInfo).SeparateChar;
                }
                else if (xmlField.CustomFieldInfo is AveCustomMetadataFieldInfo)
                {
                    valueInfo.SplitString = (xmlField.CustomFieldInfo as AveCustomMetadataFieldInfo).SeparateChar;
                }
                else if (xmlField.CustomFieldInfo.CustomFieldType == AveCustomFieldType.ChangeToDestination && field.AllowMultipleValues)
                {
                    valueInfo.SplitString = ";";
                }
                if (value != null)
                {
                    if (xmlField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase)
                        || xmlField.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        var lookupDataJunction = value as Dictionary<int, string>;
                        if (lookupDataJunction != null)
                        {
                            valueInfo.SourceDataJunction = lookupDataJunction;
                        }
                        else
                        {
                            valueInfo.SourceValue = value.ToString();
                        }
                    }
                    else
                    {
                        valueInfo.SourceValue = value.ToString();
                    }
                }
                if (field.AllowMultipleValues)
                {
                    List<string> mappingValues = fieldMapping.GetMultiMappingValue(valueInfo);
                    var lookupListId = new Guid(field.LookupList);
                    foreach (var mappingValue in mappingValues)
                    {
                        int itemId = GetLookupItemIdByDisplayValue(field.LookupWebId, lookupListId, field.LookupField, mappingValue);
                        if (itemId > 0)
                        {
                            itemIds.Add(itemId);
                        }
                    }
                }
                else
                {
                    string mappingValue = fieldMapping.GetMappingValue(valueInfo);
                    int itemId = GetLookupItemIdByDisplayValue(field.LookupWebId, new Guid(field.LookupList), field.LookupField, mappingValue);
                    if (itemId > 0)
                    {
                        itemIds.Add(itemId);
                    }
                }
            }
            return itemIds;
        }

        public void RestoreLinkingUrlFieldValues()
        {
            var durableLinkCache = MappingManager.SiteMappingManager.GetDurableLinkCacheForSitePostAction();
            foreach (var webCache in durableLinkCache)
            {
                var webId = webCache.Key;
                try
                {
                    using (var web = mSPSite.OpenWeb(webId))
                    {
                        foreach (var listCache in webCache.Value)
                        {
                            var listId = listCache.Key;
                            IAveList list = null;
                            var fieldCache = new Dictionary<Guid, IAveField>();
                            try
                            {
                                list = web.Lists.GetById(listId);
                            }
                            catch (Exception e)
                            {
                                log.Error("An error while getting list in restore link url field values. Web: {0}, List: {1}, Error: {2}", web.Url, listId, e);
                                continue;
                            }
                            foreach (var itemCache in listCache.Value)
                            {
                                bool needUpdate = false;
                                var itemId = itemCache.Key;
                                try
                                {
                                    var item = list.GetItemById(itemId);
                                    int itemUIVersion = new AveSPItem(this).GetCurrentUIVersion(mSPSite.ID, item);

                                    AveFileLevel itemLevel = AveFileLevel.Published;
                                    // 使用API获取的Item，如果document当前versioncheckout的话，无法获取checkout状态，只能使用file的状态来判断
                                    if (item.File != null)
                                    {
                                        itemLevel = (item.File.CheckOutType != AveCheckOutType.None) ? AveFileLevel.Checkout : AveFileLevel.Published;
                                    }
                                    else
                                    {
                                        itemLevel = item.Level;
                                    }
                                    if (itemLevel == AveFileLevel.Checkout && this.SPContextKind != AveContextKind.ClientObjectModel)
                                    {
                                        if (mSPSite.CompatibilityLevel == 15 && this.mSPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                                        {
                                            log.Log(AveLogLevel.WARN, "Skip to restore link url column value of checkout item because of lack of permission. Item Url: {0}", item.Url);
                                            continue;
                                        }
                                    }

                                    foreach (var versionCache in itemCache.Value)
                                    {
                                        int version = versionCache.Key;
                                        foreach (var valueCache in versionCache.Value)
                                        {
                                            string mappingUrl;
                                            if (this.MappingManager.SiteMappingManager.TryGetDurableLinkUrl(valueCache.Value, out mappingUrl))
                                            {
                                                IAveField field;
                                                if (!fieldCache.TryGetValue(valueCache.Key, out field))
                                                {
                                                    try
                                                    {
                                                        field = list.Fields.GetById(valueCache.Key);
                                                        fieldCache.Add(field.ID, field);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        log.Error("An exception occurred while getting column in restore link url field values. web: {0} list: {1}, ColumnId: {2}, Exception: {3}", web.Url, list.Title, valueCache.Key, e);
                                                        continue;
                                                    }
                                                }
                                                //365只有current version会用api update 其他version不去update
                                                //local只有是current version且非checkout状态时用api update,其他情况用SQL update。
                                                if (itemUIVersion == version && (itemLevel != AveFileLevel.Checkout || this.SPContextKind == AveContextKind.ClientObjectModel))
                                                {
                                                    IAveFieldUrlValue urlValue;
                                                    if (item.Fields.Contains(valueCache.Key) && item[valueCache.Key] != null)
                                                    {
                                                        urlValue = mOMFactory.CreateFieldUrlValue(item[valueCache.Key].ToString());
                                                    }
                                                    else
                                                    {
                                                        urlValue = mOMFactory.CreateFieldUrlValue();
                                                    }
                                                    urlValue.Url = mappingUrl;
                                                    if (string.Compare(urlValue.Description, urlValue.Url, StringComparison.OrdinalIgnoreCase) == 0)
                                                    {
                                                        urlValue.Description = mappingUrl;
                                                    }
                                                    item[valueCache.Key] = field.GetFieldValueAsText(urlValue);
                                                    needUpdate = true;
                                                }
                                                else if (this.SPContextKind != AveContextKind.ClientObjectModel)
                                                {
                                                    new AveSPItem(this).UpdateColumnByNative(SPSite.ID, item, version, field.RowOrdinal, field.ColName, mappingUrl);
                                                }
                                            }
                                        }
                                    }
                                    if (needUpdate)
                                    {
                                        item.SystemUpdate(false);
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Error("An exception occurred while updating item durable link column value. web: {0},  list: {1}, ItemId: {2}, Exception: {3}", web.Url, list.Title, itemId, e);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting web in restore linking url field values. Web: {0}, Error: {1}", webId, e);
                }
            }
        }

        public Guid GetList(Guid webId, string title)
        {
            return mSPSite.GetListId(webId, title);
            //Guid id = Guid.Empty;
            //using (IAveWeb web = mSPSite.OpenWeb(webId))
            //{
            //    IAveList list = web.Lists[title];
            //    id = list.ID;
            //}
            //return id;
        }

        //Query Review Not Used
        //public static Guid GetWebByNative(AveSqlConnection sqlCon, string url)
        //{
        //    Guid id = Guid.Empty;
        //    string text = "SELECT Id FROM Webs WHERE FullUrl=@Url";
        //    sqlCon.AddParameter("@Url", url.Trim('/'));
        //    using (SqlDataReader reader = sqlCon.ExecuteReader(text))
        //    {
        //        if (reader.Read())
        //        {
        //            id = reader.GetGuid(0);
        //        }
        //    }
        //    return id;
        //}        

        /// <summary>
        /// added for languageMapping, get dest Title or Name by LanguageMapping Type
        /// 只根据对应LanguageMappingType 进行mapping
        /// </summary>
        /// <param name="name">source name</param>
        /// <param name="languageType">including listMapping, fieldMapping, permissonMapping</param>
        /// <returns></returns>
        public string GetNameByLanguageMapping(string name, AveLanguageMappingType languageType)
        {
            string mappedValue = name;
            if (AveLanguageProcesser == null)
            {
                return mappedValue;
            }
            bool valueMapped = false;
            switch (languageType)
            {
                case AveLanguageMappingType.FieldMapping:
                    if (AveLanguageProcesser.FieldMapping.TryGetValue(name, out mappedValue))
                    {
                        valueMapped = true;
                    }
                    break;
                case AveLanguageMappingType.PermissionMapping:
                    if (AveLanguageProcesser.PermissionMapping.TryGetValue(name, out mappedValue))
                    {
                        valueMapped = true;
                    }
                    break;
                case AveLanguageMappingType.ContentTypeMapping:
                    if (AveLanguageProcesser.ContentTypeMapping.TryGetValue(name, out mappedValue))
                    {
                        valueMapped = true;
                    }
                    break;
                case AveLanguageMappingType.NavigationMapping:
                    if (AveLanguageProcesser.NavigationMapping.TryGetValue(name, out mappedValue))
                    {
                        valueMapped = true;
                    }
                    break;
                case AveLanguageMappingType.ListMapping:
                    if (AveLanguageProcesser.ListMapping.TryGetValue(name, out mappedValue))
                    {
                        valueMapped = true;
                    }
                    break;
                case AveLanguageMappingType.ViewTitleMapping:
                    if (AveLanguageProcesser.ViewTitleMapping.TryGetValue(name, out mappedValue))
                    {
                        valueMapped = true;
                    }
                    break;
                default:
                    //Error message: wrong type
                    break;
            }
            if (valueMapped)
            {
                log.Debug("Mapping name by language mapping from [{0}]  to [{1}],Type:[{2}]", name, mappedValue, languageType);
            }
            else
            {
                mappedValue = name;
            }
            return mappedValue;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "blogwebid: Key of web properties.")]
        public void RestoreMySiteRecentBlog()
        {
            IAvePropertyBag propertiesBag = SPSite.RootWeb.Properties;
            if (propertiesBag == null || !propertiesBag.ContainsKey("urn:schemas-microsoft-com:sharepoint:portal:profile:blogwebid"))
            {
                return;
            }
            using (IAveWeb web = SPSite.OpenWeb("Blog"))
            {
                if (!web.Exists ||
                    SPSite.RootWeb.Properties["urn:schemas-microsoft-com:sharepoint:portal:profile:blogwebid"].ToString().Equals(web.ID.ToString()))
                {
                    return;
                }
                SPSite.RootWeb.Properties["urn:schemas-microsoft-com:sharepoint:portal:profile:blogwebid"] = web.ID.ToString();
                SPSite.RootWeb.Properties.Update();
            }
        }

        public IAveWeb GetWebByName(string name)
        {
            IAveWeb retWeb = null;
            if (AveProtocolHeaderConstants.ROOT_WEB_NAME.Equals(name))
            {
                retWeb = mSPSite.RootWeb;
            }
            else
            {
                retWeb = mSPSite.RootWeb.Webs[name];
            }

            return retWeb;
        }

        public IAveWeb OpenWeb(string relativeUrl)
        {
            IAveWeb retWeb = null;
            if (AveProtocolHeaderConstants.ROOT_WEB_NAME.Equals(relativeUrl))
            {
                retWeb = mSPSite.RootWeb;
            }
            else
            {
                retWeb = mSPSite.OpenWeb(relativeUrl);
            }
            return retWeb;
        }

        public string ApplicationName
        {
            get
            {
                return mWebAppName;
            }
        }

        private string GetApplicationName(string siteUrl)
        {
            string appName = siteUrl;
            int maxPrefix = 8; //count of letter 'https://'
            int index = mSiteUrl.IndexOf("/", maxPrefix, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                appName = mSiteUrl.Substring(0, index);
            }
            return appName;
        }

        public Guid GetWeb(IAveBackupRestoreQueryService queryService, string p)
        {
            return mSPSite.GetWeb(queryService, p);
        }

        [Obsolete("use AveReplaceProcessor.IdReplace instead.")]
        public string IdReplace(string oldUrl, ref bool needReplaceLast)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.IdReplace"))
            {

                return AveReplaceProcessor.IdReplace(oldUrl, MappingManager, ref needReplaceLast);

            }

        }


        public void ScheduleDocument()
        {
            var tempScheduleItemCacheMapping = MappingManager.SiteMappingManager.GetScheduleItemCacheMappingJustForPostAction();
            if (tempScheduleItemCacheMapping.Count > 0)
            {
                foreach (Guid webId in tempScheduleItemCacheMapping.Keys)
                {
                    using (IAveWeb temp = mSPSite.OpenWeb(webId))
                    {
                        foreach (Guid fileId in tempScheduleItemCacheMapping[webId])
                        {
                            try
                            {
                                IAveFile file = temp.GetFile(fileId);
                                IAveScheduledItem scheduledItem = this.ObjectModelFactory.CreateScheduledItem();
                                if (scheduledItem.IsScheduledItem(file.Item))
                                {
                                    scheduledItem.SetScheduledItemStatus(file.Item);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("An exception occurred while set scheduledItem :{0} exception :{1}", fileId, e.ToString());
                            }
                        }
                    }

                }
            }
        }

        internal void UpdateUserInfoByNative(IAveUser _user, AveUserInfo old)
        {
            throw new NotImplementedException();
        }

        internal IAveWeb GetCheckoutWeb(IAveWeb web, IAveUser user, Guid fileId)
        {
            return GetCheckoutWeb(this.SPSite.ID, web, user, fileId);
        }

        internal IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId)
        {
            return mSPSite.GetCheckoutWeb(siteId, web, user, fileId);
        }

        [Obsolete("This method will be deprecated and removed later. key--001")]
        public void ClearSiteGroups()
        {
            try
            {
                var count = mSPSite.RootWeb.SiteGroups.Count;

                for (int i = count - 1; i >= 0; i--)
                {
                    mSPSite.RootWeb.SiteGroups.Remove(i);
                }

                if (count > 0)
                {
                    this.SPSite.RootWeb.Update();
                }
            }
            catch (Exception ex)
            {
                log.Warn("ClearSiteGroups Error. Message:" + ex.ToString());
            }
        }

        internal void ReplaceWebPartContent(Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.ReplaceWebPartContent"))
            {
                Dictionary<Guid, Dictionary<string, List<int>>> listUnupdateFileCache;
                if (this.MappingManager.SiteMappingManager.TryGetValueFromUnupdateFileCacheMappingOnlyForPostAction(listId, out listUnupdateFileCache))
                {
                    foreach (KeyValuePair<Guid, Dictionary<string, List<int>>> pair in listUnupdateFileCache)
                    {
                        try
                        {
                            using (IAveWeb web = this.SPSite.OpenWeb(pair.Key))
                            {
                                IAveFileCollection fileCollection = web.RootFolder.Files;
                                foreach (KeyValuePair<string, List<int>> filePair in pair.Value)
                                {
                                    IAveFile file = web.GetCheckoutFile(filePair.Key);
                                    foreach (int version in filePair.Value)
                                    {
                                        AveDocumentInfo info = new AveDocumentInfo();
                                        info.MappingManager = this.MappingManager;
                                        info.SiteId = SPSite.ID;
                                        info.GUID = file.UniqueId;
                                        info.ParentId = file.ParentFolder.UniqueId;
                                        info.Version = version;
                                        info.ParentSiteServerRelativeUrl = this.ServerRelativeUrl;
                                        info.OriginalVersion = version;
                                        info.Url = web.Url.TrimEnd('/') + "/" + file.Url;
                                        info.Name = file.Name;

                                        if (fileCollection.ChangeContent(file, info))
                                        {
                                            file = web.GetCheckoutFile(filePair.Key);
                                        }
                                    }
                                }
                            }
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            log.Warn("An error occurred while restoring file which is not updated. Reason: {0}", ex);
                        }
                        catch (Exception ex)
                        {
                            log.Error("An error occurred while restoring file which is not updated. Reason: {0}", ex);
                        }
                    }
                }
            }

        }

        internal void ReplaceWebPartContent()
        {
            List<Guid> listIds = new List<Guid>();
            foreach (Guid listId in this.MappingManager.SiteMappingManager.GetUnupdateFileCacheMappingOnlyForPostAction().Keys)
            {
                if (MappingManager.SiteMappingManager.ListIdMappingContainsKey(listId))
                {
                    listIds.Add(listId);
                }
                else
                {
                    log.Warn("List:{0} did not restore at this job, it may cause webpart issue.", listId);
                }
            }
            foreach (Guid listId in listIds)
            {
                ReplaceWebPartContent(listId);
            }
        }

        public Guid CheckOutFileId
        {
            get
            {
                return mSPSite.CheckOutFileId;
            }
            set
            {
                mSPSite.CheckOutFileId = value;

            }
        }

        public int CheckOutUser
        {
            get
            {
                return mSPSite.CheckOutUser;
            }
            set
            {
                mSPSite.CheckOutUser = value;
            }
        }
        //hold和Declared record重构
        //public void RestoreFileHoldStatus(Dictionary<string, AveItemHoldRecord> versionAndValue, IAveFile file)
        //{
        //    foreach (KeyValuePair<string, AveItemHoldRecord> pair in versionAndValue)
        //    {
        //        try
        //        {
        //            if (file.UIVersionLabel.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
        //            {
        //                List<IAveListItem> holdItems = GetHoldItemID(pair.Value.HoldsProperty);
        //                foreach (IAveListItem holdItem in holdItems)
        //                {
        //                    AvePublishing.LockItem(holdItem, file.Item, string.Empty);
        //                }
        //                //mSPFile.Properties["_vti_ItemHoldRecordStatus"] = pair.Value.ItemHoldRecordStatus;
        //                //mSPFile.Properties["ecm_ItemLockHolders"] = pair.Value.ItemLockHolders;
        //                //mSPFile.Properties["ecm_ItemDeleteBlockHolders"] = pair.Value.ItemDeleteBlockHolders;
        //                //mSPFile.Properties["_dlc_Holds_Property"] = ModifyHoldsProperty(pair.Value.HoldsProperty);
        //                //mSPFile.Properties["IconOverlay"] = pair.Value.IconOverlay;
        //                //mSPFile.Update();
        //            }
        //            else
        //            {
        //                //SPFileVersion version = mSPFile.Versions.GetVersionFromLabel(pair.Key);
        //                //version.Properties["_vti_ItemHoldRecordStatus"] = pair.Value.ItemHoldRecordStatus;
        //                //version.Properties["ecm_ItemLockHolders"] = pair.Value.ItemLockHolders;
        //                //version.Properties["ecm_ItemDeleteBlockHolders"] = pair.Value.ItemDeleteBlockHolders;
        //                //version.Properties["_dlc_Holds_Property"] = ModifyHoldsProperty(pair.Value.HoldsProperty);
        //                //version.Properties["IconOverlay"] = pair.Value.IconOverlay;
        //                //mSPFile.Update();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            mLog.Warn("update file [" + file.Url + "], uiversionLabel [" + pair.Key + "] error: " + ex.ToString());
        //        }
        //    }
        //}

        public List<IAveListItem> GetHoldItemID(string holdsProperty)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.GetHoldItemID"))
            {

                var holdIds = new List<IAveListItem>();
                if (string.IsNullOrEmpty(holdsProperty))
                {
                    return holdIds;
                }
                string[] holds = holdsProperty.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string hold in holds)
                {
                    try
                    {
                        var destWebUrl = GetDestWebUrl(hold);
                        if (destWebUrl != string.Empty)
                        {
                            using (IAveWeb web = mSPSite.OpenWeb(destWebUrl))
                            {
                                AvePublishing.Factory = this.mOMFactory;
                                var holdList = AvePublishing.GetHoldsList(web);
                                if (holdList != null)
                                {
                                    var holdId = hold.Substring(hold.LastIndexOf('/') + 1);
                                    var item = holdList.GetItemById(Convert.ToInt32(holdId));
                                    holdIds.Add(item);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("get the hold item error: " + e.ToString());
                    }
                }
                return holdIds;

            }

        }

        private string GetDestWebUrl(string sourceHold)
        {
            if (sourceHold.Contains("/Lists/Holds/"))
            {
                string sourceWebUrl = SourceSiteInfo.ServerRelativeUrl + sourceHold.Substring(sourceHold.IndexOf('/'), sourceHold.IndexOf("/Lists/Holds/", StringComparison.OrdinalIgnoreCase) - sourceHold.IndexOf('/'));

                if (this.MappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(sourceWebUrl))
                {
                    return "/" + AveReplaceProcessor.UrlReplace(sourceWebUrl, this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl).TrimStart('/');
                }
            }
            return string.Empty;
        }

        public void AddUnRestoreListLastModifiedTime(Guid listId, DateTime lastModified)
        {
            if (!UnRestoreListLastModifiedTime.ContainsKey(listId))
            {
                UnRestoreListLastModifiedTime.Add(listId, lastModified);
            }
        }

        public void AddUnRestoreFileHoldRecordInfo(Guid webId, string url, AveItemHoldRecord itemHoldRecord)
        {
            if (!UnRestoreFileHoldRecordCache.ContainsKey(webId))
            {
                UnRestoreFileHoldRecordCache.Add(webId, new Dictionary<string, AveItemHoldRecord>());
            }

            if (!UnRestoreFileHoldRecordCache[webId].ContainsKey(url))
            {
                UnRestoreFileHoldRecordCache[webId].Add(url, itemHoldRecord);
            }
        }

        public void AddUnRestoreItemHoldRecordInfo(Guid webId, Guid listId, int itemId, AveItemHoldRecord itemHoldRecord)
        {
            if (!UnRestoreItemHoldRecordCache.ContainsKey(webId))
            {
                UnRestoreItemHoldRecordCache.Add(webId, new Dictionary<Guid, Dictionary<int, AveItemHoldRecord>>());
            }

            if (!UnRestoreItemHoldRecordCache[webId].ContainsKey(listId))
            {
                UnRestoreItemHoldRecordCache[webId].Add(listId, new Dictionary<int, AveItemHoldRecord>());
            }

            if (!UnRestoreItemHoldRecordCache[webId][listId].ContainsKey(itemId))
            {
                UnRestoreItemHoldRecordCache[webId][listId].Add(itemId, itemHoldRecord);
            }
        }
        public void RestoreListLastModifiedTime()
        {
            if (mQueryService != null)
            {
                foreach (Guid listId in UnRestoreListLastModifiedTime.Keys)
                {
                    mQueryService.UpdateListModifiedTime(this.SPSite.ID, listId, UnRestoreListLastModifiedTime[listId]);
                }
            }
        }

        public void RemoveTempMasterPage()
        {
            try
            {
                foreach (var tempMasterPage in TempMasterPages)
                {
                    log.Debug("Try to delete temp master page in site post action. tempFilePathInfo: {0}.", tempMasterPage);
                    var tempFilePathInfo = tempMasterPage.Split(':');
                    if (tempFilePathInfo.Length != 2)
                    {
                        log.Warn("Failed to delete temp master page in site post action. tempFilePathInfo: {0}.", tempMasterPage);
                        continue;
                    }
                    var fileWebId = tempFilePathInfo[0];
                    string tempFileUrl = tempFilePathInfo[1];
                    using (var fileWeb = this.SPSite.OpenWeb(new Guid(fileWebId)))
                    {
                        IAveFile tempFile = fileWeb.GetFile(tempFileUrl);
                        if (!tempFile.Exists)
                        {
                            continue;
                        }
                        try
                        {
                            tempFile.Delete();
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, "Cannot delete temp master page in site post action :{0}, exception:{1}", tempMasterPage, ex); ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An exception occurred while do remove temp master page in site post action. exception:{0}", ex);
            }
        }

        public void ReplaceMetadataTermSetAndTermPropertyUrl()
        {
            AveMetadataService restoreHandler = new AveMetadataService(this, new MetaDataServiceOption());
            restoreHandler.PostActionReplaceMetadataTermSetAndTermPropertyUrl();
        }

        public void RestoreWebLastModifiedTime()
        {
            if (this.SPContextKind != AveContextKind.ClientObjectModel)
            {
                foreach (var webId in mMappingManager.SiteMappingManager.UnRestoreWebLastModifiedTime.Keys)
                {
                    using (var web = mSPSite.OpenWeb(webId))
                    {
                        web.LastItemModifiedDate = mMappingManager.SiteMappingManager.UnRestoreWebLastModifiedTime[webId];
                        web.Update();
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "file property")]
        public void RestoreInfoPathDoc()
        {
            using (new AvePerformanceScope("Restore.AveSPSite.RestoreInfoPathDoc"))
            {
                List<string> unRestoreGuidAndUrlInfopath = MappingManager.SiteMappingManager.UnReplaceGuidAndUrlInfoPathCache;
                string fileRelativeUrl = null;
                if (unRestoreGuidAndUrlInfopath != null)
                {
                    foreach (string docInfo in unRestoreGuidAndUrlInfopath)
                    {
                        try
                        {
                            string[] IDs = docInfo.Split(new char[] { ',' });
                            if (IDs.Length == 2)
                            {
                                using (IAveWeb web = this.SPSite.OpenWeb(new Guid(IDs[1])))
                                {
                                    IAveFile file = web.GetFile(IDs[0]);
                                    bool isListTypeInfoPath = file.ParentFolder.ParentList != null &&
                                        file.ParentFolder.ParentList.BaseType != AveBaseType.DocumentLibrary && IsListTypeInfoPath(file);

                                    #region 获取infopath目的端文件的完整路径
                                    StringBuilder stringBuilder = new StringBuilder();
                                    stringBuilder.Append(AveUrlUtility.CombineUrl(web.Url, file.ParentFolder.Url));
                                    stringBuilder.Append("/");   //publish to list 需要的url为parentFolder+'/'
                                    if (!isListTypeInfoPath)
                                    {
                                        stringBuilder.Append(file.Name);//publish to library和content type需要的url为parentFolder+'/'+当前文件名
                                    }
                                    fileRelativeUrl = stringBuilder.ToString();

                                    #endregion

                                    #region 创建DocumentInfo，并赋值
                                    AveDocumentInfo fileInfo = new AveDocumentInfo();
                                    fileInfo.MappingManager = this.MappingManager;
                                    fileInfo.Version = file.UIVersion;
                                    fileInfo.OriginalVersion = file.UIVersion;
                                    fileInfo.Level = (int)file.Level;
                                    if (file.Item != null)
                                    {
                                        fileInfo.DTimeCreated = (DateTime)file.Item["Created"];
                                        fileInfo.DTimeLastModified = (DateTime)file.Item["Modified"];
                                    }
                                    else
                                    {
                                        fileInfo.DTimeCreated = file.TimeCreated;
                                        fileInfo.DTimeLastModified = file.TimeLastModified;
                                    }
                                    fileInfo.ParentId = file.ParentFolder.UniqueId;
                                    fileInfo.GUID = file.UniqueId;
                                    fileInfo.SiteId = this.SPSite.ID;
                                    fileInfo.Url = fileRelativeUrl;//该url为infopath需要替换的url，并非fileInfo真正的url
                                    fileInfo.HasStream = file.HasStream();
                                    #endregion
                                    //替换infopath文件
                                    string publishContentTypeId = String.Empty;
                                    if (!file.ChangeXSNContent(fileInfo, file.ParentFolder.ParentListId, out publishContentTypeId))
                                    {
                                        log.Warn("Change InfoPath Content failed, fileRelativeUrl: {0}", fileRelativeUrl);
                                    }
                                    //ADO-128220,Client环境需要重新publish
                                    if (mOMFactory.ContextKind == AveContextKind.ClientObjectModel && !string.IsNullOrEmpty(publishContentTypeId) && isListTypeInfoPath)
                                    {
                                        PublishInfoPathList(file, publishContentTypeId);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Warn("Restore infopath: {0} failed. Error: {1}", fileRelativeUrl, e);
                        }
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "xml namespace")]
        private bool IsListTypeInfoPath(IAveFile XSNFile)
        {
            Stream stream = null;
            Stream fileStream = null;
            try
            {
                var originalCabBinary = XSNFile.OpenBinary();
                stream = new MemoryStream(originalCabBinary, false);
                using (CabinetExtractor extractor = new CabinetExtractor())
                {
                    fileStream = extractor.Extract(stream, "manifest.xsf");
                }
                XmlDocument xmlDocument = new XmlDocument();
                fileStream.Seek(0L, SeekOrigin.Begin);
                xmlDocument.PreserveWhitespace = true;
                XmlReader reader = XmlReader.Create(fileStream);
                xmlDocument.Load(reader);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                nsmgr.AddNamespace("xsf", "http://schemas.microsoft.com/office/infopath/2003/solutionDefinition");
                nsmgr.AddNamespace("xsf2", "http://schemas.microsoft.com/office/infopath/2006/solutionDefinition/extensions");
                nsmgr.AddNamespace("xsf3", "http://schemas.microsoft.com/office/infopath/2009/solutionDefinition/extensions");

                XmlNode typeNode = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf2:solutionDefinition/xsf2:solutionPropertiesExtension", nsmgr);
                if (typeNode.Attributes["branch"].InnerText.Equals("list"))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                log.Warn("Load InfoPath XSNFile failed, error message: {0}", e);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
            return false;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "property key")]
        public void PublishInfoPathList(IAveFile templateFile, string infoPathListContentTypeId)
        {
            if (templateFile.Exists &&
                templateFile.ParentFolder.Properties.ContainsKey("_ipfs_infopathenabled") &&
                ((string)templateFile.ParentFolder.Properties["_ipfs_infopathenabled"]).Equals("True", StringComparison.OrdinalIgnoreCase))
            {
                IAveList parentList = templateFile.ParentFolder.ParentList;
                if (parentList != null)
                {
                    parentList.PublicSharepointInfoPathList(templateFile, (int)parentList.ParentWeb.RegionalSettings.LocaleId, parentList.ID.ToString(), infoPathListContentTypeId);
                }
            }
        }
        /// <summary>
        /// 尽量减少UpdateWeb次数，按RecordRestrictions排序还原
        /// 只有三种情况：
        /// Null
        /// BlockDelete
        /// BlockDelete, BlockEdit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        private Dictionary<T, AveItemHoldRecord> SortHoldRecordCache<T>(Dictionary<T, AveItemHoldRecord> collection)
        {
            var list = collection.ToList();
            list.Sort(
                (p1, p2) =>
                {
                    var x = string.IsNullOrEmpty(p1.Value.RecordRestrictions) ? string.Empty : p1.Value.RecordRestrictions;
                    var y = string.IsNullOrEmpty(p2.Value.RecordRestrictions) ? string.Empty : p2.Value.RecordRestrictions;
                    return x.Length.CompareTo(y.Length);
                }
                );
            return list.ToDictionary(p => p.Key, p => p.Value);
        }
        public void RestoreUnRestoreHoldRecord()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUnRestoreHoldRecord"))
            {
                string webLevelRecordRestrictions = null;
                try
                {
                    webLevelRecordRestrictions = this.SPSite.RootWeb.Properties.ContainsKey("ecm_siterecordrestrictions") ?
                                                             this.SPSite.RootWeb.Properties["ecm_siterecordrestrictions"] : null;
                    if (UnRestoreFileHoldRecordCache != null && UnRestoreFileHoldRecordCache.Count > 0)
                    {
                        var isSiteProvisionHold = false;
                        foreach (var webid in UnRestoreFileHoldRecordCache.Keys)
                        {
                            using (var web = mSPSite.OpenWeb(webid))
                            {
                                var isWebProvisionHold = false;
                                //bool provisionList = false;
                                foreach (var pair in SortHoldRecordCache(UnRestoreFileHoldRecordCache[webid]))
                                {
                                    var isListProvisionHold = false;
                                    //file 被hold后会产生checkout version。
                                    var file = web.GetCheckoutFile(pair.Key);
                                    DeclareRecordOrHoldItem(pair.Value, file.Item, ref isSiteProvisionHold, ref isWebProvisionHold, ref isListProvisionHold);
                                }
                            }
                        }
                    }
                    if (UnRestoreItemHoldRecordCache != null && UnRestoreItemHoldRecordCache.Count > 0)
                    {
                        var isSiteProvisionHold = false;
                        foreach (var webid in UnRestoreItemHoldRecordCache.Keys)
                        {
                            using (var web = mSPSite.OpenWeb(webid))
                            {
                                var isWebProvisionHold = false;
                                foreach (var listId in UnRestoreItemHoldRecordCache[webid].Keys)
                                {
                                    var isListProvisionHold = false;
                                    var list = web.Lists[listId];
                                    foreach (var pair in SortHoldRecordCache(UnRestoreItemHoldRecordCache[webid][listId]))
                                    {
                                        var item = list.GetItemById(pair.Key);
                                        DeclareRecordOrHoldItem(pair.Value, item, ref isSiteProvisionHold, ref isWebProvisionHold, ref isListProvisionHold);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while RestoreUnRestoreHoldRecord. Error:{0}", ex);
                    //qlluo: Post action do not support report, remove it.
                    //report.AddDetail(new AveWrapperReportDto("HoldRecord", "HoldRecord", AveReportObjectType.HoldRecord, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreHoldRecord + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while RestoreUnRestoreHoldRecord. Error:{0}", ex);
                }
                finally
                {
                    var currentRecordRestrictions = this.SPSite.RootWeb.Properties.ContainsKey("ecm_siterecordrestrictions") ?
                                                             this.SPSite.RootWeb.Properties["ecm_siterecordrestrictions"] : null;
                    if (string.Equals(currentRecordRestrictions, webLevelRecordRestrictions, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SPSite.RootWeb.Properties["ecm_siterecordrestrictions"] = string.IsNullOrEmpty(webLevelRecordRestrictions) ?
                            string.Empty : webLevelRecordRestrictions;
                        this.SPSite.RootWeb.Properties.Update();
                        if (SPContextKind == AveContextKind.ClientObjectModel)//Need update web to take properties effect for client model.s
                        {
                            this.SPSite.RootWeb.Update();
                        }
                    }
                }
            }

        }

        protected void DeclareRecordOrHoldItem(AveItemHoldRecord itemHoldRecord, IAveListItem item, ref bool isSiteProvisionHold, ref bool isWebProvisionHold, ref bool isListProvisionHold)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.DeclareRecordOrHoldItem"))
            {

                try
                {
                    //If item["_vti_ItemHoldRecordStatus"] is not null ,an exception is thrown in AvePublishing.DeclareItemAsRecord(item). 
                    //item["_vti_ItemHoldRecordStatus"]不为null的话hold的属性还不回去。
                    try
                    {
                        if (item["_vti_ItemHoldRecordStatus"] != null)
                        {
                            item["_vti_ItemHoldRecordStatus"] = null;
                            item.SystemUpdate(false);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateItemFailed, e);
                    }
                    AvePublishing.Factory = this.mOMFactory;
                    if (itemHoldRecord.IsHold)
                    {
                        if (!isSiteProvisionHold)
                        {
                            AvePublishing.SetSiteLockProperty(mSPSite);
                            isSiteProvisionHold = true;
                        }
                        if (!isWebProvisionHold)
                        {
                            AvePublishing.ProvisionWeb(item.Web);
                            isWebProvisionHold = true;
                        }
                        if (!isListProvisionHold)
                        {
                            AvePublishing.ProvisionList(item.ParentList);
                            isListProvisionHold = true;
                        }
                        List<IAveListItem> holdItems = GetHoldItemID(itemHoldRecord.HoldsProperty);
                        foreach (IAveListItem holdItem in holdItems)
                        {
                            AvePublishing.LockItem(item, holdItem, string.Empty);
                        }
                    }
                    //Hold后不能再改变属性
                    if (itemHoldRecord.IsRecord)
                    {
                        UpdateWebRecordSetting(itemHoldRecord, item.Web.Site);
                        AvePublishing.DeclareItemAsRecord(item);
                        //item["_vti_ItemDeclaredRecord"] = DateTime.Parse(itemHoldRecord.ItemDeclaredRecord).ToLocalTime().ToString();
                    }
                    //item.Properties["ecm_ItemDeleteBlockHolders"] = itemHoldRecord.ItemDeleteBlockHolders;
                    //item.Properties["ecm_ItemLockHolders"] = itemHoldRecord.ItemLockHolders;
                    //item["_vti_ItemHoldRecordStatus"] = itemHoldRecord.ItemHoldRecordStatus;              
                    //item.SystemUpdate(false);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while declaring record or holding item. Name:{0}. error:{1}", item.Name, e);
                }

            }
        }
        public void UpdateWebRecordSetting(AveItemHoldRecord itemHoldRecord, IAveSite site)
        {
            if (!string.IsNullOrEmpty(itemHoldRecord.RecordRestrictions))
            {
                string webLevelRecordRestrictions = site.RootWeb.Properties.ContainsKey("ecm_siterecordrestrictions") ?
                    site.RootWeb.Properties["ecm_siterecordrestrictions"] : null;
                if (webLevelRecordRestrictions == null ||
                    !string.Equals(itemHoldRecord.RecordRestrictions, webLevelRecordRestrictions.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    site.RootWeb.Properties["ecm_siterecordrestrictions"] = itemHoldRecord.RecordRestrictions;
                    site.RootWeb.Properties.Update();
                    if (SPContextKind == AveContextKind.ClientObjectModel)//Need update web to take properties effect for client model. 
                    {
                        site.RootWeb.Update();
                    }
                }
            }
        }

        public void AddUnReplaceUrlIDCache(Guid webId, Guid listId, int itemId, string fieldName)
        {
            if (!UnReplaceUrlIDCache.ContainsKey(webId))
            {
                UnReplaceUrlIDCache.Add(webId, new Dictionary<Guid, Dictionary<int, List<string>>>());
            }

            if (!UnReplaceUrlIDCache[webId].ContainsKey(listId))
            {
                UnReplaceUrlIDCache[webId].Add(listId, new Dictionary<int, List<string>>());
            }

            if (!UnReplaceUrlIDCache[webId][listId].ContainsKey(itemId))
            {
                UnReplaceUrlIDCache[webId][listId].Add(itemId, new List<string>());
            }
            if (!UnReplaceUrlIDCache[webId][listId][itemId].Contains(fieldName))
            {
                UnReplaceUrlIDCache[webId][listId][itemId].Add(fieldName);
            }
        }

        private List<string> mFBAProviders = null;
        public bool IsFBAUser(string domain)
        {
            if (mFBAProviders == null)
            {
                mFBAProviders = AveAuthenticationUtility.GetFBAProviders(mWebApplication);
            }
            return mFBAProviders.Contains(domain, StringComparer.OrdinalIgnoreCase);
        }

        public bool HasFBAProvider()
        {
            if (mFBAProviders == null)
            {
                mFBAProviders = AveAuthenticationUtility.GetFBAProviders(mWebApplication);
            }
            return mFBAProviders.Count != 0;
        }

        public IReport GetReport()
        {
            return report;
        }

        #region IAveSPSite Members


        IAveMetadataService IAveSPSite.MetadataService
        {
            get
            {
                if (mMetadataService == null)
                {
                    mMetadataService = new AveMetadataService(this);
                }
                return mMetadataService;
            }
            set
            {
                mMetadataService = value as AveMetadataService;
            }
        }

        IAveSPMembers IAveSPSite.SPMembers
        {
            get { return mSPMembers; }
        }

        #endregion

        #region Add for SP2013
        public bool CheckCompatibilityLevel(int sourceCompatibilityLevel, bool isThrowException = false)
        {
            if (sourceCompatibilityLevel > 0 && sourceCompatibilityLevel != SPSite.CompatibilityLevel)
            {
                if (isThrowException)
                {
                    throw new CompatibilityLevelSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_CompatibilityLevelConflictError);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        internal void RestoreUser(AveUserList userCache)
        {
            if (userCache != null)
            {
                foreach (var userInfo in userCache.Users)
                {
                    mSPMembers.RestoreUser(userInfo, mSPMembers.DefaultOption);
                }
            }
        }

        internal void RestoreGroup(AveGroupList groupCache)
        {
            if (groupCache != null)
            {
                foreach (var groupInfo in groupCache.Groups)
                {
                    foreach (var userInfo in groupInfo.Members)
                    {
                        mSPMembers.RestoreUser(userInfo, mSPMembers.DefaultOption);
                    }
                    mSPMembers.RestoreGroup(groupInfo, mSPMembers.DefaultOption);
                }
            }
        }

        internal void RestoreMetadataInfo(List<AveTermStoreInfo> metadataInfo)
        {
            if (metadataInfo != null && metadataInfo.Count > 0)
            {
                try
                {
                    using (WrapperStopwatch.CreateInstance(true, (time) => log.Debug("restore mms time:{0}", time)))
                    {
                        var metadataService = MetadataService;
                        metadataService.EnableCache = false;
                        metadataService.Restore(metadataInfo, MappingManager, RestoreManagedMetadataNavigation);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("restore mms failed:{0}", ex.ToString());
                }
            }
        }

        public void RestoreDefaultContentTypeRequiredProperty()
        {
            try
            {
                Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<Guid, bool>>>> listFieldRequiredCache = mMappingManager.SiteMappingManager.GetListFieldRequiredCacheMappingOnlyForPostAction();
                if (null == listFieldRequiredCache || listFieldRequiredCache.Count == 0)
                {
                    log.Debug("the ListDefaultCTFieldLinks is null or empty");
                    return;
                }
                //1,获取SPList的Default ContentType
                mSPSite.ReloadSite();
                //1,遍历获取WebId
                foreach (Guid webId in listFieldRequiredCache.Keys)
                {
                    try
                    {
                        //获取AveWeb
                        IAveWeb web = mSPSite.OpenWeb(webId);
                        log.Debug("The web Url is : {1}", web.Url);
                        //遍历获取List
                        foreach (Guid listId in listFieldRequiredCache[webId].Keys)
                        {
                            try
                            {
                                //获取AveList
                                IAveList list = web.GetList(listId);
                                log.Debug("The List Title is: {0}", list.Title);
                                //获取List的默认ContentType
                                var contentType = list.ContentTypes[0];
                                log.Debug("step 1: Get the default contentType: {0} of the list: {1}", contentType.Name, list.Title);
                                //2，获取Default CT的FieldLinks
                                var fieldLinks = contentType.FieldLinks;
                                log.Debug("step 2: Get the fieldLinks of the default ContentType");
                                //3，便利FieldLinks
                                if (null != fieldLinks && fieldLinks.Count > 0)
                                {
                                    bool isChange = false;
                                    log.Debug("step 3: Get FieldLink from the Cache according to the default ContentType Name");
                                    if (listFieldRequiredCache[webId][listId].ContainsKey(contentType.Name))
                                    {
                                        var mListFieldLinks = listFieldRequiredCache[webId][listId][contentType.Name];
                                        if (mListFieldLinks != null && mListFieldLinks.Count > 0)
                                        {
                                            log.Debug("step 4: For each the fieldLinks from Default ContentType");
                                            foreach (var fieldLink in fieldLinks)
                                            {
                                                if (mListFieldLinks.ContainsKey(fieldLink.ID))
                                                {
                                                    log.Debug("step 5: Replace the required property, old id {0}, cache is {1}, fieldName is {2}", fieldLink.Required, mListFieldLinks[fieldLink.ID], fieldLink.Name);
                                                    if (fieldLink.Required.Equals(mListFieldLinks[fieldLink.ID]) == false)
                                                    {
                                                        fieldLink.Required = mListFieldLinks[fieldLink.ID];
                                                        isChange = true;
                                                        log.Debug("isChange = {0}", isChange);
                                                    }
                                                }
                                            }
                                        }
                                        else //当前ContentType的FieldLinks缓存为空
                                        {
                                            log.Debug("the fieldLinks in the Cache is empty,ContentTypeName: {0}, List: {1}, Web: {2}", contentType.Name, list.Title, web.Url);
                                        }
                                    }
                                    if (isChange)
                                    {
                                        contentType.Update();
                                        //list.ContentTypes.Update();
                                        log.Debug("step 6: Update the Default ContentType");
                                    }
                                }
                                else
                                {
                                    log.Debug("the fieldLinks in the contentType is empty,ContentTypeName: {0}, List: {1}, Web: {2}", contentType.Name, list.Title, web.Url);
                                }
                            }
                            catch (Exception ListEx)
                            {
                                log.Debug("An error occurred when restore the List level, error: {0}", ListEx);
                            }
                        }
                    }
                    catch (Exception webEx)
                    {
                        log.Debug("An error occurred when restore the web level, error: {0}", webEx);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred when call the method RestoreDefaultContentTypeRequiredProperty, error: {0}", ex);
            }
        }
        //internal void RestoreWorkflowStartOptions()
        //{
        //    try
        //    {
        //        WFConflictResolution wfResolution = WFConflictResolution.Instance;
        //        wfResolution.UpdateWorkflowStartOptions();
        //    }
        //    catch (Exception e)
        //    {
        //        log.Log(AveLogLevel.WARN, "An error occured when restore workflow startOptions error: {0}", e);
        //    }
        //}
        public void RestoreUserCustomActions(List<AveUserCustomActionInfo> customActions)
        {
            AveSPUserCustomActionCollection restoreUserCustomActions = new AveSPSiteUserCustomActionCollection(this);
            restoreUserCustomActions.Restore(customActions);
        }

        //public void ResetPropertyForMordenSite()
        //{
        //    if (siteDenyAddAndCustomizePagesStatus.HasValue)
        //    {
        //        var tenant = mOMFactory.CreateTenant(mOMFactory.GetAdminUrl(mAccount), true);
        //        var siteProperty = tenant.GetSitePropertiesByUrl(mSiteUrl);
        //        siteProperty.DenyAddAndCustomizePages = siteDenyAddAndCustomizePagesStatus.Value;
        //        siteProperty.Update();
        //        siteDenyAddAndCustomizePagesStatus = null;
        //    }
        //}
    }

    //internal class AveSPSiteV1 : AveSPSite, ISPSiteImport
    //{
    //    private static object siteLock = new object();

    //    private IUserMapping userMapping;
    //    private ITemplateMapping templateMapping;
    //    private ILanguageMappingController languageMappingController;
    //    private AveSPUserProfile userProfileManager;
    //    public AveSPSiteV1(string _url, string parentFullPath, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
    //        : base(_url, parentFullPath, contextKind, aveUserAccountInfo)
    //    {
    //        this.DestinationURL = parentFullPath;
    //    }

    //    public AveSPSiteV1(string _url, string parentFullPath, AveSqlConnection _sqlConn, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
    //        : base(_url, parentFullPath, _sqlConn, contextKind, aveUserAccountInfo)
    //    {
    //        this.DestinationURL = parentFullPath;
    //    }

    //    public Wrapper.Core.SPAPI.ISPAPIUtility SPAPIUtility
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    /// <summary>
    //    /// Restore SiteCollection
    //    /// 
    //    /// 这个是新加的接口,外围请暂时不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spSiteRestoreOption"></param>
    //    /// <returns></returns>
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPSiteRestoreOption spSiteRestoreOption)
    //    {
    //        var profiler = new AvePoint.Wrapper.Restore.Core.DefaultRestoreSiteProfiler();

    //        Restore(restoreStream, spSiteRestoreOption, profiler);

    //        return profiler.GenerateReport();
    //    }

    //    private Action<SPSiteRestoreOption, AveMetadata, ISPSiteImportProfiler> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<SPSiteRestoreOption, AveMetadata, ISPSiteImportProfiler> action = null;

    //        switch (metadataType)
    //        {
    //            case AveMetadataType.SiteBasicInfo:
    //                action = RestoreSiteBasicInfo;
    //                break;
    //            case AveMetadataType.SiteProperty:
    //                action = RestoreSiteProperty;
    //                break;
    //            case AveMetadataType.SiteFeature:
    //                action = RestoreSiteFeature;
    //                break;
    //            case AveMetadataType.Users:
    //                action = RestoreSiteUsers;
    //                break;
    //            case AveMetadataType.Groups:
    //                action = RestoreSiteGroups;
    //                break;
    //            case AveMetadataType.AudienceCache:
    //                action = RestoreAudienceCache;
    //                break;
    //            case AveMetadataType.SiteSearchInfo:
    //                action = RestoreSiteSearchInfo;
    //                break;
    //            #region ###########UserProfile###########
    //            case AveMetadataType.UserProfileProperties:
    //                action = RestoreUserProfileProperties;
    //                break;
    //            case AveMetadataType.UserProfile:
    //                action = RestoreUserProfile;
    //                break;
    //            #region 为兼容6.0 UserProfile老数据，6.1开始这部分数据都备份到UserProfile type中了
    //            case AveMetadataType.UserProfileDetail:
    //                action = RestoreUserProfileDetails;
    //                break;
    //            case AveMetadataType.UserProfileColleague:
    //                action = RestoreUserProfileColleague;
    //                break;
    //            case AveMetadataType.UserProfileMembership:
    //                action = RestoreUserProfileMembership;
    //                break;
    //            case AveMetadataType.UserProfileComment:
    //                action = RestoreUserProfileComment;
    //                break;
    //            case AveMetadataType.UserProfileTag:
    //                action = RestoreUserProfileTag;
    //                break;
    //            #endregion
    //            case AveMetadataType.UserProfileSubTypes:
    //                action = RestoreUserProfileSubTypes;
    //                break;
    //            #endregion
    //            case AveMetadataType.MetadataService:
    //                action = RestoreMetadataService;
    //                break;
    //            case AveMetadataType.LanguageFile:
    //                action = RestoreLanguageFile;
    //                break;
    //        }

    //        return action;
    //    }

    //    private void EnsureSettingOption(SPSiteRestoreOption option)
    //    {
    //        if (option.ConfigurationRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.SettingRestoreOption");
    //        }
    //    }

    //    private void EnsureSecurityOption(SPSiteRestoreOption option)
    //    {
    //        if (option.SecurityRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.SecurityRestoreOption");
    //        }
    //    }

    //    private void EnsureUserProfileOption(SPSiteRestoreOption option)
    //    {
    //        if (option.UserProfileOption == null)
    //        {
    //            throw new ArgumentNullException("option.UserProfileOption");
    //        }
    //    }

    //    private void EnsureMMSOption(SPSiteRestoreOption option)
    //    {
    //        if (option.ManagedMetadataOption == null)
    //        {
    //            throw new ArgumentNullException("option.ManagedMetadataOption");
    //        }
    //    }

    //    private void DeleteSite(SPSiteRestoreOption option)
    //    {
    //        try
    //        {
    //            var site = AveSite;

    //            if (site != null)
    //            {
    //                using (site)
    //                {
    //                    using (var web = site.RootWeb)
    //                    {
    //                        if (web.Properties.ContainsKey("BackedUp"))
    //                        {
    //                            web.Properties["BackedUp"] = "true";
    //                        }
    //                        else
    //                        {
    //                            web.Properties.Add("BackedUp", "true");
    //                        }
    //                        web.Properties.Update();
    //                    }
    //                    site.Delete();
    //                    if (option.SiteDeleted != null)
    //                    {
    //                        option.SiteDeleted();
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("delete site:{0} failed:{1}", SiteUrl, ex);
    //        }
    //        finally
    //        {
    //            mSPSite = null;
    //        }
    //    }

    //    private void RestoreSiteBasicInfo(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        try
    //        {
    //            //外围多线程创建不同的site collection会出现问题，所以需要添加lock
    //            lock (siteLock)
    //            {
    //                if (option.RestoreAction == SPContainerRestoreAction.Replace)
    //                {
    //                    DeleteSite(option);
    //                }


    //                if (option.RestoreAction != SPContainerRestoreAction.None)
    //                {
    //                    this.RestoreOption.SetRequestOption(false, false, (int)AveRestoreMode.Default);
    //                }

    //                var siteInfo = metadata.GetMetadata<AveSiteInfo>();


    //                if (languageMappingController != null)
    //                {
    //                    var lcid = languageMappingController.GetMappingLCID(siteInfo.LCID);
    //                    if (lcid != siteInfo.LCID)
    //                    {
    //                        SetLanguageForNew(lcid);
    //                    }
    //                }

    //                if (siteInfo.IsHostheader && (!SiteUrl.StartsWith(DestinationURL.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)))
    //                {
    //                    SetUseHostHeader(true);
    //                }

    //                if (!string.IsNullOrEmpty(option.SpecialSiteCreationAccount))
    //                {
    //                    SetSiteCreationAccount(option.SpecialSiteCreationAccount, siteInfo);
    //                }

    //                SetContentDBId(option.ContentDBId);


    //                RestoreSiteSelf(siteInfo, option.RestoreAction != SPContainerRestoreAction.None);


    //                if (languageMappingController != null)
    //                {
    //                    EnsureLanguageProcesser();
    //                }

    //                if (option.CleanDefaultSPObjects && IsNewCreated)
    //                {
    //                    ClearSiteGroups();
    //                }
    //            }

    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //        }
    //        catch (Exception ex)
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //            log.Error("Restore site basic info for {0} failed:{1}", SiteUrl, ex);
    //            throw;
    //        }
    //    }

    //    //不需要改函数，由外围来控制
    //    //private bool NeedRestore(SPContainerRestoreAction restoreAction)
    //    //{
    //    //    return IsNewCreated || restoreAction != SPContainerRestoreAction.Skip;
    //    //}

    //    private void RestoreAudienceCache(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var audienceManager = new AveAudienceManager(this);
    //        audienceManager.GenerateIDMapping(metadata.GetMetadata<Dictionary<string, string>>());
    //        //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //    }

    //    /// <summary>
    //    /// TODO-LONG 需要重新计算状态。
    //    /// </summary>
    //    /// <param name="option"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="report"></param>
    //    private void RestoreSiteProperty(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureSettingOption(option);
    //        if (option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            RestoreSiteProperty(metadata.GetMetadata<AveSiteSettingInfo>());
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            //TODO report.Details.AnalyzeReport(GetReport());
    //        }
    //    }

    //    /// <summary>
    //    /// TODO-LONG 需要重新计算状态。
    //    /// </summary>
    //    /// <param name="option"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="report"></param>
    //    private void RestoreSiteFeature(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureSettingOption(option);
    //        if (option.ConfigurationRestoreOption.RestoreConfiguration)
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
    //                log.Error("Restore Site Feature failed:{0}", ex);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //            }
    //        }
    //    }

    //    private void RestoreSiteUsers(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            RestoreSiteUsers(option.SecurityRestoreOption, metadata, profiler, true);
    //        }
    //        else
    //        {
    //            var users = metadata.GetMetadata<List<AveUserInfo>>();
    //            this.SPMembers.LoadUsers(users);
    //        }
    //    }

    //    internal void RestoreSiteUsers(SPSecurityRestoreOption option, AveMetadata metadata, ISPImportProfiler profiler, bool isSiteLevel = false)
    //    {
    //        var users = metadata.GetMetadata<List<AveUserInfo>>();
    //        if (option.RestoreSecurity)
    //        {
    //            if (users != null)
    //            {
    //                if (option.UserGroupRestoreOption.ProcessUserInfoBeforeRestore != null)
    //                {
    //                    users = option.UserGroupRestoreOption.ProcessUserInfoBeforeRestore(users);
    //                }
    //                this.SPMembers.RestoreUsers(users, option.UserGroupRestoreOption.ToMembersRestoreOption(isSiteLevel), profiler);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //TODO report.Details.AnalyzeReport(this.SPMembers.GetReport());
    //            }
    //        }
    //        else
    //        {
    //            this.SPMembers.LoadUsers(users);
    //        }
    //    }

    //    private void RestoreSiteGroups(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            RestoreSiteGroups(option.SecurityRestoreOption, metadata, profiler);
    //        }
    //        else
    //        {
    //            var groups = metadata.GetMetadata<List<AveGroupInfo>>();
    //            this.SPMembers.LoadGroups(groups);
    //        }
    //    }

    //    internal void RestoreSiteGroups(SPSecurityRestoreOption option, AveMetadata metadata, ISPImportProfiler profiler)
    //    {
    //        var groups = metadata.GetMetadata<List<AveGroupInfo>>();
    //        if (option.RestoreSecurity)
    //        {
    //            if (groups != null)
    //            {
    //                if (option.UserGroupRestoreOption.ProcessGroupInfoBrforeRestore != null)
    //                {
    //                    groups = option.UserGroupRestoreOption.ProcessGroupInfoBrforeRestore(groups);
    //                }
    //                foreach (AveGroupInfo group in groups)
    //                {
    //                    if (option.UserGroupRestoreOption.ProcessUserInfoBeforeRestore != null)
    //                    {
    //                        group.Members = option.UserGroupRestoreOption.ProcessUserInfoBeforeRestore(group.Members);
    //                        group.Memberships.Clear();
    //                        group.Members.ForEach(userInfo => group.Memberships.Add(userInfo.ID));
    //                    }
    //                    this.SPMembers.RestoreGroup(group, option.UserGroupRestoreOption.ToMembersRestoreOption(true), profiler);
    //                }

    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                ////TODO report.Details.AnalyzeReport(this.SPMembers.GetReport());
    //            }
    //        }
    //        else
    //        {
    //            this.SPMembers.LoadGroups(groups);
    //        }
    //    }

    //    private void RestoreSiteSearchInfo(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureSettingOption(option);
    //        if (option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            var searchInfo = metadata.GetMetadata<AveSearchInfo>();
    //            if (searchInfo != null)
    //            {
    //                if (AveEnv.IsMoss)
    //                {
    //                    using (var searchManager = new AveSPSearch(this))
    //                    {
    //                        searchManager.Restore(searchInfo);
    //                        //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                        ////TODO report.Details.AnalyzeReport(searchManager.GetReport());
    //                    }
    //                }
    //                else
    //                {
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                }
    //            }
    //        }
    //    }

    //    private void RestoreUserProfileSubTypes(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureUserProfileOption(option);

    //        var subtypesInfos = metadata.GetMetadata<List<AveUserProfileSubTypeInfo>>();

    //        if (subtypesInfos != null && subtypesInfos.Count > 0 && option.UserProfileOption.RestoreUserProfile)
    //        {
    //            if (AveEnv.IsMoss)
    //            {
    //                try
    //                {
    //                    if (userProfileManager != null)
    //                    {
    //                        userProfileManager.Dispose();
    //                    }

    //                    userProfileManager = new AveSPUserProfile(this, false);
    //                    userProfileManager.ExistSkip = !option.UserProfileOption.Overwrite;
    //                    userProfileManager.RestoreUserProfileSubTypes(subtypesInfos);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                }
    //                catch (Exception ex)
    //                {
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                    log.Error("Restore User profile subTypes failed:{0}", ex);
    //                    userProfileManager.Dispose();
    //                    userProfileManager = null;
    //                }
    //            }
    //            else
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //            }
    //        }

    //    }

    //    private void RestoreUserProfileProperties(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var propertyInfos = metadata.GetMetadata<List<AvePropertyInfo>>();

    //        if (propertyInfos != null && propertyInfos.Count > 0 && (SourceSiteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase)//对于mysite，不需要判断userprofile的option，一定还原property
    //            || option.UserProfileOption.RestoreUserProfile))
    //        {
    //            if (AveEnv.IsMoss)
    //            {
    //                try
    //                {
    //                    userProfileManager.RestoreUserProfileProperties(propertyInfos);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                }
    //                catch (Exception ex)
    //                {
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                    log.Error("Restore User profile properties failed:{0}", ex);
    //                    userProfileManager.Dispose();
    //                    userProfileManager = null;
    //                }
    //            }
    //            else
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //            }
    //        }
    //    }

    //    private void RestoreUserProfile(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var profileInfo = metadata.GetMetadata<AveUserProfileInfo>();
    //        if (userProfileManager != null)
    //        {
    //            try
    //            {
    //                userProfileManager.Restore(profileInfo);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            }
    //            catch (Exception ex)
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                log.Error("Restore user profile failed:{0}", ex);
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreUserProfileDetails(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var detailsInfo = metadata.GetMetadata<List<AveUserProfileValueInfo>>();
    //        if (userProfileManager != null)
    //        {
    //            try
    //            {
    //                userProfileManager.RestoreDetails(detailsInfo);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            }
    //            catch (Exception ex)
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                log.Error("Restore user profile details failed:{0}", ex);
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreUserProfileColleague(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var colleagueInfo = metadata.GetMetadata<AveColleagueInfo>();
    //        if (userProfileManager != null)
    //        {
    //            try
    //            {
    //                userProfileManager.RestoreColleague(colleagueInfo);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            }
    //            catch (Exception ex)
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                log.Error("Restore user profile colleague failed:{0}", ex);
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreUserProfileMembership(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var membership = metadata.GetMetadata<AveMembershipInfo>();
    //        if (userProfileManager != null)
    //        {
    //            try
    //            {
    //                userProfileManager.RestoreMembership(membership);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            }
    //            catch (Exception ex)
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                log.Error("Restore user profile membership failed:{0}", ex);
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreUserProfileComment(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var comment = metadata.GetMetadata<AveSocialCommentInfo>();
    //        if (userProfileManager != null)
    //        {
    //            try
    //            {
    //                userProfileManager.RestoreComment(comment);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            }
    //            catch (Exception ex)
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                log.Error("Restore user profile comment failed:{0}", ex);
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreUserProfileTag(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        var tag = metadata.GetMetadata<AveSocialTagInfo>();
    //        if (userProfileManager != null)
    //        {
    //            try
    //            {
    //                userProfileManager.RestoreTag(tag);
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            }
    //            catch (Exception ex)
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                log.Error("Restore user profile tag failed:{0}", ex);
    //            }
    //        }
    //        else
    //        {
    //            //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreMetadataService(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        EnsureMMSOption(option);
    //        RestoreMetadataService(option.ManagedMetadataOption, metadata, profiler);
    //    }

    //    internal void RestoreMetadataService(SPManagedMetadataRestoreOption option, AveMetadata metadata, ISPImportProfiler profiler)
    //    {
    //        var termStoreInfos = metadata.GetMetadata<List<AveTermStoreInfo>>();

    //        if (option.RestoreType != SPManagedMetadataRestoreType.None)
    //        {
    //            if ((AveEnv.IsMoss && ObjectModelFactory.ContextKind != AveContextKind.Server07ObjectModel) || (ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel))
    //            {
    //                try
    //                {   //只有客户选择整体还原SiteCollection的情况时，才会执行该处的逻辑，Term Only的逻辑会针对使用MMS的Column和Value在Web、List和Item级别单独restore处理
    //                    this.MetadataService = new AveMetadataService(this);
    //                    this.MetadataService.ImportProfiler = profiler;
    //                    if (option.FilterAction != null)
    //                    {
    //                        option.FilterAction(termStoreInfos);
    //                    }
    //                    if (option.RestoreType == SPManagedMetadataRestoreType.Cache)
    //                    {   //同步过一次的SiteCollection在客户需要整体Replicate SiteCollection MMS时，可以将传过来的termStoreInfos放入缓存中，为resotre mms column和value里的try restore保险提供支持
    //                        //wrapper以后可能会删除try restore的逻辑，目前暂时保留，如果删除try restore，那么目的端Site的MMS Cache就没什么用了.
    //                        this.MetadataService.CacheTermStoreInfo(termStoreInfos);
    //                    }
    //                    else
    //                    {
    //                        this.MetadataService.EnableCache = option.EnableCache;
    //                        this.MetadataService.SkipGlobalTermGroup = option.SkipGlobalTermGroup;
    //                        this.MetadataService.SkipLocalTermGroup = option.SkipLocalTermGroup;
    //                        this.MetadataService.Restore(termStoreInfos, MappingManager, RestoreManagedMetadataNavigation);
    //                    }

    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //TODO report.Details.AnalyzeReport(this.MetadataService.GetReport());
    //                }
    //                catch (Exception ex)
    //                {
    //                    log.Error("Restore metadata service failed:{0}", ex);
    //                    //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //                }
    //            }
    //            else
    //            {
    //                //TODO report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //            }
    //        }
    //    }

    //    private void RestoreLanguageFile(SPSiteRestoreOption option, AveMetadata metadata, ISPSiteImportProfiler profiler)
    //    {
    //        RestoreLanguageFile(SrcLanguageId, SPSite.RootWeb.Language, metadata, profiler);
    //    }

    //    internal void RestoreLanguageFile(uint sourceLanguageId, uint currentLanguageId, AveMetadata metadata, ISPImportProfiler profiler)
    //    {
    //        if (languageMappingController != null && (!string.IsNullOrEmpty(languageMappingController.TemporaryDirectoryForSPResourceFile)))
    //        {
    //            var languageInfo = metadata.GetMetadata<AveLanguageInfo>();

    //            if (languageInfo != null)
    //            {
    //                languageMappingController.RestoreLanguageFile(languageInfo);

    //                EnsureLanguageProcesser(sourceLanguageId, currentLanguageId);
    //            }
    //        }
    //    }

    //    private void EnsureLanguageProcesser(uint sourceLanguageId, uint currentLanguageId)
    //    {
    //        if (sourceLanguageId != currentLanguageId)
    //        {
    //            var language = languageMappingController.GetLanguageMapping(sourceLanguageId, currentLanguageId);

    //            if (language != null)
    //            {
    //                if (this.mAveLanguageProcesser == null)
    //                {
    //                    this.mAveLanguageProcesser = AveLanguageProcesser.GetLanguageInstance(AveEnv.AgentRootFolder, languageMappingController.TemporaryDirectoryForSPResourceFile);
    //                }

    //                this.mAveLanguageProcesser.LoadMapping(string.Empty, sourceLanguageId, currentLanguageId, language.ExportMapping());
    //            }
    //        }
    //    }

    //    private void EnsureLanguageProcesser()
    //    {
    //        EnsureLanguageProcesser(SrcLanguageId, SPSite.RootWeb.Language);
    //    }

    //    public void Restore(IAveRestoreStream restoreStream, SPSiteRestoreOption spSiteRestoreOption, ISPSiteImportProfiler profiler)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }

    //        if (spSiteRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spSiteRestoreOption");
    //        }

    //        try
    //        {
    //            if (profiler != null) { profiler.BeginRestore(); }

    //            while (true)
    //            {
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

    //                        action(spSiteRestoreOption, metadata, profiler);
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

    //    public IUserMapping UserMapping
    //    {
    //        get
    //        {
    //            return userMapping;
    //        }
    //        set
    //        {
    //            userMapping = value;
    //            if (userMapping != null)
    //            {
    //                mPlaceHolderAccount = userMapping.PlaceHolderAccount;
    //                DefaultUser = userMapping.DefaultUserAccount;
    //                SPMembers.UserAndDomainMapping.SetUserAndDomainMappings(userMapping.ExportUserMapping(), userMapping.ExportDomainMapping());
    //            }
    //        }
    //    }

    //    public ILanguageMappingController LanguageMappingController
    //    {
    //        get
    //        {
    //            if (languageMappingController == null)
    //            {
    //                languageMappingController = new BuiltInLanguageMappingController();
    //            }
    //            return languageMappingController;
    //        }
    //    }

    //    public bool EventReceiverFiringDisabled
    //    {
    //        get
    //        {
    //            AveSPEventReceiverConfig.InitEventReceiver(mOMFactory);
    //            var enabled = AveSPEventReceiverConfig.EventReceiverEnabled;
    //            if (enabled.HasValue)
    //            {
    //                return !enabled.Value;
    //            }

    //            return false;
    //        }
    //        set
    //        {
    //            AveSPEventReceiverConfig.InitEventReceiver(mOMFactory);
    //            if (value)
    //            {
    //                AveSPEventReceiverConfig.DisableEventReceiver();
    //            }
    //            else
    //            {
    //                AveSPEventReceiverConfig.EnableEventReceiver();
    //            }
    //        }
    //    }

    //    ITemplateMapping ISPSiteImport.TemplateMapping
    //    {
    //        get
    //        {
    //            return templateMapping;
    //        }
    //        set
    //        {
    //            templateMapping = value;
    //            this.SetTemplateMapping(templateMapping.ExportXml());
    //        }
    //    }

    //    public override void Dispose()
    //    {
    //        new AveSPSitePostAction(this).Excute();

    //        if (userProfileManager != null)
    //        {
    //            userProfileManager.Dispose();
    //            userProfileManager = null;
    //        }

    //        base.Dispose();
    //        if (this.languageMappingController != null)
    //        {
    //            this.languageMappingController.CleanLanguageFile();
    //        }
    //    }

    //}
}