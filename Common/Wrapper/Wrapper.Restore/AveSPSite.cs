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
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using System.Reflection;
using System.IO;
using AvePoint.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using AvePoint.Wrapper.Common;
using System.Xml;
using System.Collections.ObjectModel;
//using AvePoint.Wrapper.SPService;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using AvePoint.GCommon.Utility.Cryptography;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Security;
using System.Management.Automation.Remoting;
//using Microsoft.Azure.ActiveDirectory.Client.Framework;
using AvePoint.Wrapper.Restore.NintexForm;
using AvePoint.Wrapper.Common.Graph;
using AvePoint.GCommon.GraphAPI;
using System.Web;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/04/20", "Yuzhi.Jiang@AvePoint.com", "Yongqiang.Zhou@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]

    public class AveSPSite : RestoreableObject, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        
        private IAveBackupRestoreQueryService mQueryService;
        private string mSiteUrl;
        private AveMetadataService mMetadataService;
        private IAveWebApplication mWebApplication = null;
        private IAveSite mSPSite = null;
        private AveMappingManager mMappingManager = new AveMappingManager();
        private bool mIsNewCreated = false;
        private uint mLanguageForNewCreated = 0;
        private bool mUseHostHeader = false;
        private Guid mContentDBId = Guid.Empty;
        private bool mCreationAccountResetted = false;
        private AveLanguageProcesser mAveLanguageProcesser = null;
        private AveSPMembers mSPMembers;
        internal DateTime mSPRequestTimeout = DateTime.UtcNow;
        internal int mHoursReloadSite = 12;
        public AveSiteSettingInfo SourceSiteSettingInfo = null;
        //private AveServiceContext mServiceContext = null;
        private AveRestoreGhostPageOption m_SaveBinaryForGhostPage = AveRestoreGhostPageOption.NoAction;
        private bool mSetLookupFieldSourceValue = false;
        private AveRBSRestore mRestore;
        private IAveTemplateMapping mTemplateMapping;
        private NavigationRestoreSetting navigationRestoreSetting = new NavigationRestoreSetting();

        //Source site id from backup header
        //Granular is using the SPSite Id, CM is using the site tree node Id
        public Guid SourceHeaderSiteId { get; set; }
        public AveSiteInfo SourceSiteInfo { get; set; }
        private bool needClosePublishingFeature = false;

        public bool NeedClosePublishingFeature
        {
            get { return needClosePublishingFeature; }
            set { needClosePublishingFeature = value; }
        }

        public bool FailedToEnablePublishingFeature { get; set; } = false;

        public PostFieldCacheWorker FieldPostCache { get; set; } 

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

        public AveFeatureInfoBox SourceFeatures
        {
            get;
            set;
        }

        public Dictionary<Guid, Dictionary<string, AveItemHoldRecord>> UnRestoreFileHoldRecordCache = new Dictionary<Guid, Dictionary<string, AveItemHoldRecord>>();
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, AveItemHoldRecord>>> UnRestoreItemHoldRecordCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, AveItemHoldRecord>>>();
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, List<string>>>> UnReplaceUrlIDCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, List<string>>>>();
        public Dictionary<Guid, DateTime> UnRestoreListLastModifiedTime = new Dictionary<Guid, DateTime>();
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
        private AveObjectModelFactory mOMFactory;

        private string mWebAppName;

        private AveBPOSAccountInfo mAccount;
        private AveContextKind mSPContextKind = AveContextKind.ServerObjectModel;

        private string mPlaceHolderAccount = string.Empty;
        private IAveSite mCheckoutSite;
        private IAveWeb mCheckoutWeb;
        private IAvePublishing mPublishing;
        private IReport report = new AveWrapperReport();

        private AvePWASettings mPWASettings;

        public AveGroupTeamInfo GroupTeamInfo { get; set; }

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
            get { return mSPContextKind; }
        }

        public AvePWASettings PWASettings
        {
            get
            {
                if (mPWASettings == null)
                {
                    mPWASettings = new AvePWASettings(this);
                }
                return mPWASettings;
            }
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
        public void SetUserMapping(Dictionary<string, string> userMapping, Dictionary<string, string> domainMapping, string defaultUser)
        {
            //MappingManager.SiteMappingManager.UserMapping = userMapping;
            //MappingManager.SiteMappingManager.DomainMapping = domainMapping;
            SPMembers.UserAndDomainMapping.SetUserAndDomainMappings(userMapping, domainMapping);
            DefaultUser = defaultUser;
        }

        public void SetTemplateMapping(XmlElement xe)
        {
            AveTemplateMapping mTemplateMapping = new AveTemplateMapping(xe);
            this.mTemplateMapping = mTemplateMapping;
        }

        public void SetLanguageMapping(AveLanguageProcesser languageMapping)
        {
            mAveLanguageProcesser = languageMapping;
        }

        public void SetWebTemplate(Guid webId)
        {
            mWebId = webId;
        }

        public AveItemFieldFilterRule ItemFieldFilter;

        private UserCustomActionCacheService mUserCustomActionSerializer;

        public UserCustomActionCacheService UserCustomActionSerializer
        {
            get
            {
                if (mUserCustomActionSerializer == null)
                {
                    mUserCustomActionSerializer = new UserCustomActionCacheService(this);
                }
                return mUserCustomActionSerializer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeFields"></param>
        /// <param name="excludeFields"></param>
        /// <param name="mode">0:depend,includeFields || all-excludeFields; 1:including all; 2: excluding all</param>
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

        public void SetUseHostHeader(bool value)
        {
            mUseHostHeader = value;
        }
        public void SetContentDBId(Guid id)
        {
            mContentDBId = id;
        }
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

        public AveLanguageProcesser AveLanguageProcesser
        {
            get { return mAveLanguageProcesser; }
            set { mAveLanguageProcesser = value; }
        }

        public string SiteUrl
        {
            get { return mSiteUrl; }
        }

        public string DestinationURL
        {
            get;
            set;
        }

        public string GroupSiteEmail
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
        public bool OverWriteNavigation = false;

        public NavigationRestoreSetting NavigationRestoreSetting
        {
            set { navigationRestoreSetting = value; }
            get { return navigationRestoreSetting; }
        }

        private bool isGAORunningJob = false;
        public bool IsGAORunningJob
        {
            get { return isGAORunningJob; }
            set { isGAORunningJob = value; }
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

        /// <summary>
        /// the option determines if the job is OutOfPlace restore, for creating the deleted site collection in InPlace restore job.
        /// </summary>
        public bool IsOutOfPlaceRestore { get; set; }
        /// <summary>
        /// the option that determines if we keep the default value, when creating/updating a list item
        /// </summary>
        public bool KeepDefaultValue { get; set; }
        /// <summary>
        /// the option that determines if we should verify metadata column value before restoring a list item
        /// </summary>
        public bool VerifyItemMMSColumnValue { get; set; }

        private void InitCache()
        {
            FieldPostCache = new PostFieldCacheWorker(this);
        }

        /// <summary>
        /// only for replicator to save some data to do post action.
        /// </summary>
        public AveSPSite(string _url, string parentFullPath, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            WrapperRuntime.CurrentContext.MappingManager.Clear();
            WrapperRuntime.CurrentContext.MappingManager = mMappingManager;

            mSiteUrl = _url.TrimEnd('/');
            //mSPContextKind = contextKind;
            mSPMembers = new AveSPMembers(this);
            mAccount = aveUserAccountInfo;
            mWebAppName = parentFullPath;
            //mWebAppName = GetApplicationName(_url);
            //mRootWebRelativeUrl = _url.Substring(mWebAppName.Length);
            mOMFactory = AveObjectModelFactory.CreateObjectModelFactory(parentFullPath, mAccount, contextKind);
            mSPContextKind = mOMFactory.ContextKind;
            mSPSite = mOMFactory.CreateSite(mSiteUrl);
            InitCache();
            //mSqlConn = new AveSqlConnection(mSPSite.ContentDatabase.DatabaseConnectionString);
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        public AveSPSite(string _url, string parentFullPath, AveSqlConnection _sqlConn, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            WrapperRuntime.CurrentContext.MappingManager.Clear();
            WrapperRuntime.CurrentContext.MappingManager = mMappingManager;
            mSiteUrl = _url.TrimEnd('/');

            mSPMembers = new AveSPMembers(this);
            mAccount = aveUserAccountInfo;
            mOMFactory = AveObjectModelFactory.CreateObjectModelFactory(parentFullPath, mAccount, contextKind);
            mSPContextKind = mOMFactory.ContextKind;
            InitCache();
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }
        public void GetSPSite(AveSiteInfo siteInfo)
        {
            SourceSiteInfo = siteInfo;
            mSPSite = mOMFactory.CreateSite(mSiteUrl);
        }

        public void GetSiteSelf()
        {
            mSPSite = mOMFactory.CreateSite(mSiteUrl);
        }

        public void GetSiteSelf(AveSiteInfo siteInfo)
        {
            SourceSiteInfo = siteInfo;
            mSrcLcd = siteInfo.LCID;
            if (string.IsNullOrEmpty(SourceSiteInfo.OwnerLogin))
            {
                string loginName = Environment.UserDomainName + "\\" + Environment.UserName;
                SourceSiteInfo.OwnerLogin = loginName;
                SourceSiteInfo.OwnerName = Environment.UserName;
            }

            mSPSite = mOMFactory.CreateSite(mSiteUrl);
            if (mOMFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                CheckSiteLock(mSiteUrl);
            }
            MappingManager.SiteMappingManager.SourceSiteInfo = this.SourceSiteInfo;
            MappingManager.SiteMappingManager.DestSiteInfo = new AveSiteInfo() { ServerRelativeUrl = mSPSite.ServerRelativeUrl, Url = mSiteUrl };
            WFConflictResolution.ParentSite = this;
            InitializeMembers();
            log.Info($"Get site collection successfully, url: {mSiteUrl}, source url: {siteInfo.Url}");
        }

        public void SetSiteCreationAccount(string ownerlogin, AveSiteInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.SetSiteCreationAccount"))
            {
#endif
                if (!string.IsNullOrEmpty(ownerlogin) && info != null)
                {
                    string oldLogin = info.OwnerLogin;
                    info.OwnerName = ownerlogin;
                    info.OwnerLogin = ownerlogin;
                    info.SecondaryContactLogin = null;
                    mCreationAccountResetted = true;
                    log.Info("Replace siteCollection owner. {0} to {1}", oldLogin, info.OwnerLogin);
                }
#if PerformanceLog
            }
#endif
        }

        private void InitializeMembers()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.InitializeMembers"))
            {
#endif
                if (CURRENT_USER_ID <= 0)
                {
                    try
                    {
                        if (mSPSite.RootWeb.CurrentUser == null)
                        {
                            throw new Exception(
                                "Can not find CurrentUser, please check your Agent Monitor setting.");
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
                if (mQueryService == null && (mSPContextKind == AveContextKind.ServerObjectModel || mSPContextKind == AveContextKind.Server07ObjectModel))
                {
                    mQueryService = ObjectModelFactory.CreateQueryService<IAveBackupRestoreQueryService>(mSPSite);
                }

#if PerformanceLog
            }
#endif
        }

        /*private string GetUserDisplayName(string loginName)
        {
            try
            {
                IAveUtility utility = this.ObjectModelFactory.Utility;
                var userInfo = utility.ResolvePrincipal(this.SPWebApplication, null, loginName, AvePrincipalType.User, AvePrincipalSource.All, false);
                if (userInfo != null)
                {
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
        }*/

        /// <summary>
        /// /// 这个函数主要是为了load或者创建基本的site所需要的，如果需要还原setting，请到restore property函数中。
        /// </summary>
        /// <param name="siteInfo"></param>
        public virtual void RestoreSiteSelf(AveSiteInfo siteInfo, AveCreateSiteInfo createSiteInfo = null, ToExportUserInfo SiteOwnerUPN = null)
        {
            RestoreSiteSelf(siteInfo, true, createSiteInfo, SiteOwnerUPN);
        }

        public virtual void RestoreSiteSelf(AveSiteInfo siteInfo, bool needCreateSite, AveCreateSiteInfo createSiteInfo = null, ToExportUserInfo SiteOwnerUPN = null)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Site.RestoreSiteSelf"))
            {
#endif
            SourceSiteInfo = siteInfo;
            mSrcLcd = siteInfo.LCID;
            if (string.IsNullOrEmpty(SourceSiteInfo.OwnerLogin))
            {
                string loginName = Environment.UserDomainName + "\\" + Environment.UserName;
                SourceSiteInfo.OwnerLogin = loginName;
                SourceSiteInfo.OwnerName = Environment.UserName;
            }

            bool isRetried = false;     //SAAS-4638 add retry logic for SharePoint login failure
            while (true)
            {
                try
                {
                    if (siteInfo.IsHostheader && mUseHostHeader && string.Equals(mSiteUrl.Trim('/'), DestinationURL.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        mSiteUrl = siteInfo.Url;
                    }
                    if (mSPSite == null)
                    {
                        try
                        {
                            mSPSite = mOMFactory.CreateSite(mSiteUrl);
                            long storageMaximumLevel = mSPSite.Quota.StorageMaximumLevel * 1024L * 1024L;
                            log.Info($"Current Site:{mSPSite.Url} StorageMaximumLevel is:{mSPSite.Quota.StorageMaximumLevel}.Storage is:{mSPSite.Usage.Storage}.ByteStorageMaximumLevel:{storageMaximumLevel}.");
                            if (storageMaximumLevel == 0)
                            {
                                //special env,special site does not permission to get this value, so skip this check when size is 0.
                                Logger.Info($"CheckAveExceedStorageLimit.Current Site:{mSPSite.Url} StorageMaximumLevel is 0, skip check current site storage limit.");
                            }
                            else if (mSPSite.Usage.Storage >= storageMaximumLevel)
                            {
                                throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
                            }
                        }
                        catch (AveExceedStorageLimitException e)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn($"RestoreSiteSelf.Get Site Storage StorageMaximumLevel Error{e}");
                        }

                        mWebApplication = mSPSite.WebApplication;
                    }

                    try
                    {
                        // 没有应用Template的Custom站点
                        if (mSPSite.RootWeb.Configuration == -1 && "STS".Equals(mSPSite.RootWeb.WebTemplate, StringComparison.OrdinalIgnoreCase))
                        {
                            log.Info("Apply WebTemplate for rootweb, the web url is: {0}, language: {1}, webTemplate: {2}", mSPSite.RootWeb.Url, siteInfo.LCID, siteInfo.WebTemplate);
                            mSPSite.RootWeb.ApplyWebTemplate(siteInfo.WebTemplate, siteInfo.LCID);
                            // 应用Template后，需要重新获取site
                            mSPSite.Dispose();
                            mSPSite = mOMFactory.CreateSite(mSiteUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Apply site {0} WebTemplate:{1} failed, due to: {2}", mSiteUrl, siteInfo.WebTemplate, ex);
                    }

                    //检测获取到的SPSite对象是否在目的端指定的WebApplication下，主要是Host Header类型的Site
                    try
                    {

                        //if (mOMFactory.ContextKind != AveContextKind.ClientObjectModel && !String.IsNullOrEmpty(this.DestinationURL))
                        //{
                        if (!String.IsNullOrEmpty(this.DestinationURL))
                        {
                            if (siteInfo.IsHostheader & (!mSiteUrl.StartsWith(this.DestinationURL.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)))
                            {
                                mUseHostHeader = true;
                            }
                        }
                        if (siteInfo.IsHostheader && this.mUseHostHeader)
                        {
                            IAveWebApplication webApp = mOMFactory.CreateWebApplication().Lookup(new Uri(DestinationURL));
                            if (mSPSite.WebApplication.ID != webApp.ID)
                            {
                                Dispose();
                                mSPSite = null;
                                throw new AveException("The host header site collection already exists, but it does not exist in the specified web application.");
                            }
                            if (webApp is IDisposable)
                            {
                                (webApp as IDisposable).Dispose();
                            }
                        }
                        //判断New的SPSite对象是否为目的端Sitecollection
                        if (this.SPContextKind != AveContextKind.ClientObjectModel && !mSiteUrl.TrimEnd('/').Equals(mSPSite.Url.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
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
                    //if (this.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                    //{
                    //    AveAuthenticationUtility.InitAuthenticationProvider(mSPSite.WebApplication);
                    //}
                    break;
                }
                catch (IncorrectUserNameOrPasswordException incorrectError)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateSiteCollectionError, incorrectError.ToString());
                    throw;
                }
                catch (PasswordExpiredException expiredError)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateSiteCollectionError, expiredError.ToString());
                    throw;
                }
                catch (AveSkipLockSiteException LockSiteError)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateSiteCollectionError, LockSiteError.ToString());
                    throw;
                }
                catch (AveExceedStorageLimitException e)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateSiteCollectionError, e.ToString());
                    if (needCreateSite)
                    {
                        #region  create site collection

                        if (AveSPWebTemplate.IsGroupTeamSite(siteInfo.WebTemplate, GroupSiteEmail) || AveSPWebTemplate.IsTeamPrivateChannelSite(siteInfo.WebTemplate))
                        {
                            var newUrl = VerifySiteUrl(siteInfo);
                            if (!string.Equals(newUrl, mSiteUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                mSiteUrl = newUrl;
                                mSPSite = mOMFactory.CreateSite(mSiteUrl);
                                mIsNewCreated = true;
                            }
                            else
                            {
                                log.Error("Can't find new site collection url for the office365 group.");
                                throw new Exception(WrapperRestoreResource.DestSiteCollectionConnectionFailed);
                            }
                        }
                        else if (createSiteInfo == null && !IsOutOfPlaceRestore)
                        {
                            //to create the deleted site collection for in place restore.
                            var tempCreateSiteInfo = new AveCreateSiteInfo()
                            {
                                UserName = mAccount.UserName,
                                Password = CspCommunicationWrapper.WrapKeyToBase64String(mAccount.Password),
                                AdminUrl = mAccount.AdminUrl,

                                StorageQuota = 25600 * 1024,//Unit MB
                                ResourceQuota = 300
                            };
                            if (SiteOwnerUPN != null)
                            {
                                tempCreateSiteInfo.SiteOwnerUPN = SiteOwnerUPN.UserPrincipalName;
                            }

                            log.Info("Begin Create Site Collection,Url:{0}", siteInfo.Url);
                            try
                            {
                                CreateSiteCollection(siteInfo, tempCreateSiteInfo);
                                mIsNewCreated = true;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("StorageOptimization_SOARArchiverFailCreateSiteCollection", ex);
                            }

                            mSPSite = mOMFactory.CreateSite(mSiteUrl);
                        }
                        #endregion
                        else if (this.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel && isRetried)
                        {
                            throw new Exception(WrapperRestoreResource.DestSiteCollectionConnectionFailed);
                        }
                        else
                        {
                            isRetried = true;
                            continue;
                        }
                    }
                    else
                    {
                        throw new AveException("Cannot create a site collection, the user is not in the farm administrator's group.");
                    }
                    break;
                }
            }
            if (mOMFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                CheckSiteLock(mSiteUrl);
            }
            WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(AveUrlUtility.GetServerUrl(SourceSiteInfo.Url), AveUrlUtility.GetServerUrl(mSiteUrl));
            WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(SourceSiteInfo.Url, mSiteUrl);
            WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddSiteUrlMapping(SourceSiteInfo.ServerRelativeUrl, mSPSite.ServerRelativeUrl);

            MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(AveUrlUtility.GetServerUrl(SourceSiteInfo.Url), AveUrlUtility.GetServerUrl(mSiteUrl));
            MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(SourceSiteInfo.Url, mSiteUrl);
            MappingManager.SiteMappingManager.AddSiteUrlMapping(SourceSiteInfo.ServerRelativeUrl, mSPSite.ServerRelativeUrl);
            MappingManager.SiteMappingManager.AddSiteFullUrlMapping(SourceSiteInfo.Url, mSPSite.Url);
            MappingManager.SiteMappingManager.SourceSiteInfo = this.SourceSiteInfo;
            MappingManager.SiteMappingManager.DestSiteInfo = new AveSiteInfo() { ServerRelativeUrl = mSPSite.ServerRelativeUrl, Url = mSiteUrl };
            WFConflictResolution.ParentSite = this;
            InitializeMembers();
#if PerformanceLog
            }
#endif
        }

        public string VerifySiteUrl(AveSiteInfo siteInfo)
        {
            if (IsOutOfPlaceRestore)
            {
                return mSiteUrl;
            }

            var url = mSiteUrl;
            log.Info($"Verify site collection url with information. Template: {siteInfo?.WebTemplate} EmailAddress: {GroupSiteEmail} Title: {SourceSiteInfo?.Title}");
            if ((AveSPWebTemplate.IsGroupTeamSite(siteInfo?.WebTemplate, GroupSiteEmail) && TryGetGroupSiteUrlByGraphWithRetry(ref url))
                || (AveSPWebTemplate.IsTeamPrivateChannelSite(siteInfo?.WebTemplate) && TryGetPrivateChannelSiteUrl(ref url)))
            {
                log.Info($"Map to the new site url:{url} ");
                GrantSiteAdminPermission(url);
            }
            return url;
        }

        private bool TryGetGroupSiteUrlByGraphWithRetry(ref string outputSiteUrl)
        {
            log.Info(string.Format("Begin check office365 group site url, Group:{0}", GroupSiteEmail));
            ////AOSBR-16287, try apply domain mapping
            //if (targetTenantGroupId.Equals(currentTenantGroupId, StringComparison.OrdinalIgnoreCase) || testMode)
            //{
            //    foreach (var mapping in SPMembers.UserAndDomainMapping.EnumCustomDomainMapping())
            //    {
            //        if (GroupSiteEmail.ToLower().IndexOf(mapping.Key.ToLower()) > 0)
            //        {
            //            GroupSiteEmail = GroupSiteEmail.ToLower().Replace(mapping.Key.ToLower(), mapping.Value);
            //            logger.Info(string.Format("Applied domain mapping before checking office365 group site url, Group mapped to:{0}", GroupSiteEmail));
            //            break;
            //        }
            //    }
            //}
            if (string.IsNullOrEmpty(GroupSiteEmail))
            {
                return false;
            }
            string siteUrl = string.Empty;
            var retryHelper = new AveTaskRetryHelper(new TimeSpan(0, 10, 0));
            retryHelper.AddRetryExceptionDetail("GraphAPIException", string.Empty);
            try
            {
                retryHelper.ExecuteWithRetryMechanismV2(() => siteUrl = GraphHelper.GetGroupSiteUrlByEmail(GroupSiteEmail, BPOSUserAccountInfo));
            }
            catch (GraphAPIException ge)
            {
                log.Warn($"An error occurred while try to get group site url by graph, exception tag: {ge.Tag}, ex: {ge}");
                //throw new AveGetAssociatedSiteFailedException(WrapperReportResourceKey.Wrapper_GetGroupAssociatedSiteFailed.ToString(), WrapperReportResource.Wrapper_GetGroupAssociatedSiteFailed, GroupSiteEmail);
                throw ge;
            }
            if (!string.IsNullOrEmpty(siteUrl))
            {
                outputSiteUrl = siteUrl;
                return true;
            }
            return false;
        }

        private bool TryGetPrivateChannelSiteUrl(ref string privateChannelSiteUrl)
        {
            if (string.IsNullOrEmpty(GroupSiteEmail))
            {
                return false;
            }

            if (string.IsNullOrEmpty(SourceSiteInfo.Title))
            {
                return false;
            }
            //AOSBR-16287, try apply domain mapping
            //if (targetTenantGroupId.Equals(currentTenantGroupId, StringComparison.OrdinalIgnoreCase) || testMode)
            //{
            //    foreach (var mapping in SPMembers.UserAndDomainMapping.EnumCustomDomainMapping())
            //    {
            //        if (GroupSiteEmail.ToLower().IndexOf(mapping.Key.ToLower()) > 0)
            //        {
            //            GroupSiteEmail = GroupSiteEmail.ToLower().Replace(mapping.Key.ToLower(), mapping.Value);
            //            logger.Info(string.Format("Applied domain mapping before checking office365 private channel site url, Group mapped to:{0}", GroupSiteEmail));
            //            break;
            //        }
            //    }
            //}
            string siteUrl = string.Empty;
            var retryHelper = new AveTaskRetryHelper(new TimeSpan(0, 10, 0));
            retryHelper.AddRetryExceptionDetail("GraphAPIException", string.Empty);
            retryHelper.AddRetryExceptionDetail("FileNotFoundException", "Private channel not found");
            retryHelper.AddRetryExceptionDetail("FileNotFoundException", "Private channel site collection not found");
            try
            {
                var privateChannel = GroupTeamInfo?.Channels?.FirstOrDefault(i =>
                    i.IsPrivateChannel && HttpUtility.UrlDecode(i.RelatedSiteUrl).EqualsIgnoreCase(mSiteUrl));
                retryHelper.ExecuteWithRetryMechanismV2(() =>
                    siteUrl = GraphHelper.GetPrivateChannelSiteUrl(GroupSiteEmail, privateChannel, SourceSiteInfo.Title, BPOSUserAccountInfo));
            }
            catch (GraphAPIException ge)
            {
                log.Warn($"An error occurred while try to get private channel site url by graph, exception tag: {ge.Tag}, ex: {ge}");
                //throw new AveGetAssociatedSiteFailedException(WrapperReportResourceKey.Wrapper_GetPrivateChannelAssociatedSiteFailed.ToString(), WrapperReportResource.Wrapper_GetPrivateChannelAssociatedSiteFailed, SourceSiteInfo.Title);
                throw ge;
            }
            if (!string.IsNullOrEmpty(siteUrl))
            {
                privateChannelSiteUrl = siteUrl;
                return true;
            }
            return false;
        }

        public void GrantSiteAdminPermission(string siteUrl)
        {
            IAveTenant tenant = mOMFactory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(BPOSUserAccountInfo, siteUrl));
            tenant.SetAdmin(siteUrl, BPOSUserAccountInfo.UserName);
        }


        public static SecureString ConvertToSecureString(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                throw new ArgumentNullException("args");
            }
            SecureString result = new SecureString();
            foreach (char tChar in args.ToCharArray())
            {
                result.AppendChar(tChar);
            }
            return result;
        }


        internal void SetResultValueByEachPrefix(IAvePrefix prefix, string webAppUrl, string siteUrl, ref bool result)
        {
            string avaliableManagedPathUrl = string.IsNullOrEmpty(prefix.Name) ? webAppUrl.TrimEnd('/') : webAppUrl.TrimEnd('/') + "/" + prefix.Name;
            if (prefix.PrefixType == AvePrefixType.ExplicitInclusion)
            {
                if (string.Compare(avaliableManagedPathUrl, siteUrl, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = true;
                    return;
                }
            }
            if (prefix.PrefixType == AvePrefixType.WildcardInclusion)
            {
                if (siteUrl.StartsWith(avaliableManagedPathUrl, StringComparison.OrdinalIgnoreCase) &&
                    !siteUrl.Equals(avaliableManagedPathUrl, StringComparison.OrdinalIgnoreCase))
                {
                    string siteRelatedUrl = siteUrl.Remove(0, avaliableManagedPathUrl.Length + 1);
                    if (siteRelatedUrl.IndexOf('/') == -1)
                    {
                        result = true;
                        return;
                    }
                }
            }
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreSiteProperty"))
            {
#endif
            try
            {
                log.Info($"[SAAS-38254]Target site setting info, AllowDesigner:{mSPSite.AllowDesigner}, AllowMasterPageEditing{mSPSite.AllowMasterPageEditing}, AllowRevertFromTemplate:{mSPSite.AllowRevertFromTemplate}, ShowUrlStructure:{mSPSite.ShowURLStructure}");
            }
            catch (System.Exception e)
            {
                log.Warn($"An error occurred when get site setting infos:{0}", e);
            }
            SourceSiteSettingInfo = siteSettingInfo;
            try
            {
                if (SourceSiteSettingInfo.PortalURL != null && SourceSiteSettingInfo.PortalURL.IsAvailable)
                {
                    if (SourceSiteSettingInfo.PortalURL.Value != null)
                    {
                        MappingManager.SiteMappingManager.UrlNeedPostAction.Add("PortalUrl", SourceSiteSettingInfo.PortalURL.Value);
                    }
                }
                mSPSite.RestoreSettings(siteSettingInfo);
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while creating site collection. url:{0}, site id:{1}\n error message:{2}", mSPSite.Url, mSPSite.ID, ex));
                report.AddDetail(new AveWrapperReportDto(mSPSite.Url, mSPSite.Url, AveReportObjectType.SiteSetting, AveStatus.Skipped, "You don't have permission to update site setting. " + ex.Message));
            }
            catch (Exception e)
            {
                report.AddDetail(new AveWrapperReportDto(mSPSite.Url, mSPSite.Url, AveReportObjectType.SiteSetting, AveStatus.Skipped, string.Format("An error occurred while creating site collection. url:{0}, site id:{1}\n error message:{2}", mSPSite.Url, mSPSite.ID, e.Message)));
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while creating site collection. url:{0}, site id:{1}\n error message:{2}", mSPSite.Url, mSPSite.ID, e));
            }
#if PerformanceLog
            }
#endif
        }

        public void RestoreLanguageFile(AveLanguageInfo languageInfo)
        {
            if (string.IsNullOrEmpty(mAveLanguageProcesser.JobDir))
            {
                throw new Exception("Please set JobDir for AveLanguageProcessor at first.");
            }

#if PerformaceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Site.RestoreLanguageFile"))
            {
#endif
            string path = SecurityUtils.SafeCombinePath(mAveLanguageProcesser.JobDir, languageInfo.LanguageLCD.ToString(), "src.resx");
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(languageInfo.LanguageContent);
            }
#if PerformaceLog
            }
#endif
        }

        //废弃此方法
        public void RestroeLanguageFile(AveLanguageInfo languageInfo)
        {
            if (string.IsNullOrEmpty(mAveLanguageProcesser.JobDir))
            {
                throw new Exception("Please set JobDir for AveLanguageProcessor at first.");
            }

#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestroeLanguageFile"))
            {
#endif
                string path = mAveLanguageProcesser.JobDir + Path.DirectorySeparatorChar + languageInfo.LanguageLCD + "src.resx";
                using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(languageInfo.LanguageContent);
                }
#if PerformanceLog
            }
#endif
        }

        public void DisableSPEventReceiver()
        {
            if (RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER)
            {
                AveSPEventReceiverConfig.InitEventReceiver(mOMFactory);
                AveSPEventReceiverConfig.DisableEventReceiver();
            }
        }

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

        #region For Post Action

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
                    using (var web = new AveSPWeb(this, null, webForms.Key))
                    {
                        foreach (var listNintexForms in webForms.Value)
                        {
                            var listId = listNintexForms.Key;
                            var tmpList = web.SPWeb.Lists.GetById(listId);
                            var formXml = listNintexForms.Value.FormXml;
                            var contentTypeId = listNintexForms.Value.ContentTypeId;
                            var nintexFormService = new NintexFormService(tmpList, web, true);
                            try
                            {
                                nintexFormService.RestoreForm(formXml, contentTypeId);
                                log.Info("In site post action, success to restore nintex form in content type:{0} of list:{1}.", contentTypeId, listId);
                            }
                            catch (Exception e)
                            {
                                log.Error("An error occurred while restoring nintex form of content type:{0} in the list:{1} in post action, Error:{2}.", contentTypeId, listId, e);
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
                    using (var web = this.SPSite.OpenWeb(webUrlCache.Key))
                    {
                        foreach (var listIdCache in webUrlCache.Value)
                        {
                            var listid = listIdCache.Key;
                            var list = web.Lists.GetById(listid);
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
                                            item.SystemUpdate();
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

        public void RestoreProjectTimeline()
        {
            this.PWASettings.RestoreTimeline();
        }

        public void RestoreNavNodes()
        {
            try
            {
                using (AveSPNavigation navManager = new AveSPNavigation(this))
                {
                    navManager.OverWrite = OverWriteNavigation || IsNewCreated;
                    navManager.Restore();
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore navigation. Url:{0}\n error message:{1}", mSiteUrl, ex));
                report.AddDetail(new AveWrapperReportDto("RestoreNavNodes", "RestoreNavNodes", AveReportObjectType.NavNodes, AveStatus.Skipped, "you don't have permission to RestoreNavNodes. " + ex.Message));
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore navigation. Url:{0}\n error message:{1}", mSiteUrl, e));
                //mLog.Warn(e, "An error occurred while restoring navigation. Url:{0}", mSiteUrl);
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
        internal void ResotreWebMetaInfo()
        {
            try
            {
                foreach (KeyValuePair<Guid, Dictionary<string, string>> keyValue in MappingManager.SiteMappingManager.WebAllPropertiesMapping)
                {
                    try
                    {
                        using (IAveWeb web = mSPSite.OpenWeb(keyValue.Key))
                        {
                            bool changed = false;
                            string sourceGlobalNavigationExcludes;
                            if (keyValue.Value.TryGetValue("__GlobalNavigationExcludes", out sourceGlobalNavigationExcludes))
                            {
                                string destSelfGlobalNavigation = GetDestSelfHidden(sourceGlobalNavigationExcludes);

                                string oldGlobalNavigationExcludes = string.Empty;
                                if (web.AllProperties.ContainsKey("__GlobalNavigationExcludes") && web.AllProperties["__GlobalNavigationExcludes"] != null)
                                {
                                    oldGlobalNavigationExcludes = web.AllProperties["__GlobalNavigationExcludes"].ToString();
                                }

                                string finalNavigation = MergeNavigation(oldGlobalNavigationExcludes, destSelfGlobalNavigation);

                                web.AllProperties["__GlobalNavigationExcludes"] = finalNavigation;
                                changed = true;
                            }

                            string sourceCurrentNavigationExcludes;
                            if (keyValue.Value.TryGetValue("__CurrentNavigationExcludes", out sourceCurrentNavigationExcludes))
                            {
                                string destSelfCurrentNavigation = GetDestSelfHidden(sourceCurrentNavigationExcludes);

                                string oldCurrentNavigationExcludes = string.Empty;
                                if (web.AllProperties.ContainsKey("__CurrentNavigationExcludes") && web.AllProperties["__CurrentNavigationExcludes"] != null)
                                {
                                    oldCurrentNavigationExcludes = web.AllProperties["__CurrentNavigationExcludes"].ToString();
                                }

                                string finalNavigation = MergeNavigation(oldCurrentNavigationExcludes, destSelfCurrentNavigation);

                                web.AllProperties["__CurrentNavigationExcludes"] = finalNavigation;
                                changed = true;
                            }
                            if (changed)
                            {
                                web.Update();
                            }
                            if (keyValue.Value.ContainsKey("_routermanageremail"))
                            {
                                try
                                {
                                    string routerManager = string.Empty;
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
                report.AddDetail(new AveWrapperReportDto("RestoreWebMetaInfo", "RestoreWebMetaInfo", AveReportObjectType.WebMetaInfo, AveStatus.Skipped, "you don't have permission to RestoreWebMetaInfo. " + ex.Message));
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while restore web metaInfo. error:{0}", ex.ToString());
            }
        }

        public string GetDestSelfHidden(string navigationExclude)
        {
            StringBuilder selfNavigation = new StringBuilder(navigationExclude);
            string[] excludes = navigationExclude.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string exclude in excludes)
            {
                Guid sourceObjectId;
                Guid mappingId;
                if (Guid.TryParse(exclude, out sourceObjectId))
                {
                    if (MappingManager.SiteMappingManager.WebIDMapping.TryGetValue(sourceObjectId, out mappingId) || MappingManager.SiteMappingManager.HiddenWebsPages.TryGetValue(sourceObjectId, out mappingId))
                    {
                        selfNavigation = selfNavigation.Replace(exclude, mappingId.ToString());
                    }
                }
            }
            return selfNavigation.ToString();
        }

        public string MergeNavigation(string source, string dest)
        {
            StringBuilder finalNavigation = new StringBuilder(source);
            string[] selfExcludes = dest.Split(';');
            string[] sourceExcludes = source.Split(';');
            foreach (string str in selfExcludes)
            {
                if (!sourceExcludes.Contains(str))
                {
                    finalNavigation.Append(str).Append(";");
                }
            }
            return finalNavigation.ToString();
        }

        public void RestoreHiddenSiteProperty()
        {
            try
            {
                if (this.SPContextKind == AveContextKind.ClientObjectModel ||
                    (mSPSite.IsPublish && (AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007)))
                {
                    //List<Guid> webIds = GetAllWebsGuid(mSPSite);
                    foreach (Guid webId in MappingManager.SiteMappingManager.WebIDMapping.Values)
                    {
                        try
                        {
                            using (IAveWeb web = mSPSite.OpenWeb(webId))
                            {
                                if (this.SPContextKind != AveContextKind.ServerObjectModel && !web.IsPublish)
                                {
                                    continue;
                                }
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
                                                if (MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(id) && !globalNavigationExcludes.Contains(MappingManager.SiteMappingManager.HiddenWebsPages[id].ToString()))
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
                                                if (MappingManager.SiteMappingManager.HiddenWebsPages.ContainsKey(id) && !currentNavigationExcludes.Contains(MappingManager.SiteMappingManager.HiddenWebsPages[id].ToString()))
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
                report.AddDetail(new AveWrapperReportDto("RestoreHiddenSiteProperty", "RestoreHiddenSiteProperty", AveReportObjectType.RestoreHiddenSiteProperty, AveStatus.Skipped, "You don't have permission to RestoreHiddenSiteProperty. " + ex.Message));
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while RestoreHiddenSiteProperty. error:{{0}", e.ToString());
                //mLog.Warn("An error occurred while RestoreHiddenSiteProperty. error:{{0}", e.ToString());
            }
        }

        public void RestoreCalendarSettings()
        {
            if (MappingManager.SiteMappingManager.NeedResetCalendarSettingsViews.Count == 0)
            {
                return;
            }
            IAveSite tSite = null;
            IAveWeb tWeb = null;
            IAveList tList = null;
            IAveView tView = null;
            try
            {
                foreach (Guid webId in MappingManager.SiteMappingManager.NeedResetCalendarSettingsViews.Keys)
                {
                    using (IAveWeb web = mSPSite.OpenWeb(webId))
                    {
                        try
                        {
                            foreach (Guid listId in MappingManager.SiteMappingManager.NeedResetCalendarSettingsViews[webId].Keys)
                            {
                                IAveList list = web.Lists[listId];
                                foreach (Guid viewId in MappingManager.SiteMappingManager.NeedResetCalendarSettingsViews[webId][listId])
                                {
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
                                                XmlElement tempXe = node as XmlElement;
                                                XmlNode settingNode = null;
                                                if (tempXe.GetElementsByTagName("Settings").Count > 0)
                                                {
                                                    settingNode = tempXe.GetElementsByTagName("Settings")[0];
                                                }
                                                Common.ArgumentCheck.CheckNotNull(settingNode);
                                                if (node.Attributes["CalendarUrl"] != null && settingNode.Attributes["WebUrl"] != null)
                                                {
                                                    string oldCalendarUrl = node.Attributes["CalendarUrl"].Value;
                                                    node.Attributes["CalendarUrl"].Value = AveReplaceProcessor.UrlReplace(oldCalendarUrl, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                                    string oldWebUrl = settingNode.Attributes["WebUrl"].Value;
                                                    settingNode.Attributes["WebUrl"].Value = AveReplaceProcessor.UrlReplace(oldWebUrl, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                                    string calendarUrl = node.Attributes["CalendarUrl"].Value;
                                                    string webUrl = settingNode.Attributes["WebUrl"].Value;
                                                    if (calendarUrl.Equals(oldCalendarUrl, StringComparison.OrdinalIgnoreCase) && webUrl.Equals(oldWebUrl, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        needUpdate = true;
                                                    }
                                                    if (tWeb != null)
                                                    {
                                                        if (!tWeb.Url.Equals(webUrl, StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            tWeb.Dispose();
                                                            tWeb = null;
                                                            if (tSite != null)
                                                            {
                                                                tSite.Dispose();
                                                                tSite = null;
                                                            }
                                                        }
                                                    }
                                                    if (tWeb == null)
                                                    {
                                                        if (tSite != null)
                                                        {
                                                            tSite.Dispose();
                                                            tSite = null;
                                                        }
                                                        tSite = mOMFactory.CreateSite(webUrl);
                                                        tWeb = tSite.OpenWeb();
                                                    }
                                                    if (tList != null)
                                                    {
                                                        if (!calendarUrl.StartsWith(tList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            tList = tWeb.GetListFromUrl(calendarUrl);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        tList = tWeb.GetListFromUrl(calendarUrl);
                                                    }
                                                    tView = GetListViewFromUrl(tList, calendarUrl);
                                                }
                                                var node1 = tempXe.GetElementsByTagName("Settings")[0];
                                                //foreach (XmlNode node1 in tempXe.GetElementsByTagName("Settings"))
                                                {
                                                    if (node1.Attributes["ListId"] != null)
                                                    {
                                                        Common.ArgumentCheck.CheckNotNull(tList);
                                                        node1.Attributes["ListId"].Value = tList.ID.ToString("B");
                                                    }
                                                    if (node1.Attributes["ViewId"] != null)
                                                    {
                                                        Common.ArgumentCheck.CheckNotNull(tView);
                                                        node1.Attributes["ViewId"].Value = tView.ID.ToString("B");
                                                    }
                                                    if (node1.Attributes["ListFormUrl"] != null)
                                                    {
                                                        string newValue = AveReplaceProcessor.UrlReplace(node1.Attributes["ListFormUrl"].Value, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                                        node1.Attributes["ListFormUrl"].Value = newValue;
                                                    }
                                                    //break;
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
                        finally
                        {
                            try
                            {
                                if (tWeb != null)
                                {
                                    tWeb.Dispose();
                                    tWeb = null;
                                }
                                if (tSite != null)
                                {
                                    tSite.Dispose();
                                    tSite = null;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn(string.Format("An error occurred while dispose web and site. error:{0}", e.ToString()), e);
                            }
                        }
                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn(string.Format("An error occurred while dispose web and site. error:{0}", ex.ToString()), ex);
                report.AddDetail(new AveWrapperReportDto("CalendarSettings", "CalendarSettings", AveReportObjectType.RestoreCalendarSettings, AveStatus.Skipped, "You don't have permission to restore calendar settings." + ex.Message));
            }
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

        public void Dispose()
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreDataSourceFields"))
            {
#endif
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
                                        string dUrl = MappingManager.SiteMappingManager.ListUrlMapping.ContainsKey(sUrl) ? MappingManager.SiteMappingManager.ListUrlMapping[sUrl].ToString() : (MappingManager.SiteMappingManager.AbsoluteUrlMapping.ContainsKey(sUrl) ? MappingManager.SiteMappingManager.AbsoluteUrlMapping[sUrl] : sUrl);
                                        string dDescription = MappingManager.SiteMappingManager.ListUrlMapping.ContainsKey(sDescription) ? MappingManager.SiteMappingManager.ListUrlMapping[sDescription].ToString() : (MappingManager.SiteMappingManager.AbsoluteUrlMapping.ContainsKey(sDescription) ? MappingManager.SiteMappingManager.AbsoluteUrlMapping[sDescription] : sDescription);

                                        if (sUrl == dUrl)
                                        {
                                            dUrl = this.MappingManager.SiteMappingManager.ListDefaultViewMapping[sUrl];
                                            dDescription = this.MappingManager.SiteMappingManager.ListDefaultViewMapping[sDescription];
                                        }

                                        item["DataSource"] = dUrl + "," + dDescription;
                                        item.Update();
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
                    if (MappingManager.SiteMappingManager.KpiListNeedUpdate.Count > 0)
                    {
                        foreach (DictionaryEntry de in MappingManager.SiteMappingManager.KpiListNeedUpdate)
                        {
                            using (IAveWeb web = SPSite.OpenWeb(new Guid(de.Value.ToString())))
                            {
                                IAveList l = web.Lists[new Guid(de.Key.ToString())];
                                foreach (IAveListItem item in l.Items)
                                {
                                    if (item.Fields.ContainsField("ViewGuid") && item["ViewGuid"] != null)
                                    {
                                        Guid viewCuid = new Guid(item["ViewGuid"].ToString());
                                        lock (MappingManager.SiteMappingManager.ViewGuidMapping)
                                        {
                                            if (MappingManager.SiteMappingManager.ViewGuidMapping.Keys.Contains(viewCuid))
                                            {
                                                item["ViewGuid"] = MappingManager.SiteMappingManager.ViewGuidMapping[viewCuid];
                                            }
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
                    report.AddDetail(new AveWrapperReportDto("RestoreDataSourceFields", "RestoreDataSourceFields", AveReportObjectType.RestoreDataSourceFields, AveStatus.Skipped, "You don't have permission to RestoreDataSourceFields. " + ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreUrlIDNeedReplace()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUrlIDNeedReplace"))
            {
#endif
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
                                if (MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(listId))
                                {
                                    foreach (int originalItemId in UnReplaceUrlIDCache[webId][listId].Keys)
                                    {

                                        if (MappingManager.SiteMappingManager.ItemIdMapping[listId].ContainsKey(originalItemId))
                                        {
                                            int itemId = MappingManager.SiteMappingManager.ItemIdMapping[listId][originalItemId];
                                            IAveListItem item = list.GetItemById(itemId);
                                            bool needUpdate = false;
                                            foreach (string fieldName in UnReplaceUrlIDCache[webId][listId][originalItemId])
                                            {
                                                try
                                                {
                                                    object fieldValue = item[fieldName];
                                                    var aveFieldValue = ObjectModelFactory.CreateFieldUrlValue(fieldValue.ToString());
                                                    string newValue = string.Empty;
                                                    string url = IdReplace(aveFieldValue.Url, ref needReplaceLast);
                                                    if (!string.Equals(aveFieldValue.Url, url, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        aveFieldValue.Url = url;
                                                        item[fieldName] = aveFieldValue; 
                                                        needUpdate = true;
                                                    }
                                                   
                                                    //switch (fieldName)
                                                    //{
                                                    //    case "URL":
                                                    //        string urlValue = fieldValue.ToString();
                                                    //        string url = IdReplace(urlValue.Split(new char[] { ',' })[0], ref needReplaceLast);
                                                    //        string description = IdReplace(urlValue.Split(new char[] { ',' })[1], ref needReplaceLast);
                                                    //        newValue = url + "," + description;
                                                    //        break;
                                                    //    default:
                                                    //        newValue = IdReplace(fieldValue.ToString(), ref needReplaceLast);
                                                    //        break;
                                                    //}
                                                    //item[fieldName] = newValue;
                                                }
                                                catch (AveSecurityTrimingException)
                                                {
                                                    throw;
                                                }
                                                catch (Exception e)
                                                {
                                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReplaceIdInUrlFailed, e);
                                                }
                                                if (needUpdate)
                                                {
                                                    item.SystemUpdate();
                                                }
                                            }
                                        }
                                        //list.Update();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceIdInUrlFailed, ex);
                    report.AddDetail(new AveWrapperReportDto("RestoreUrlIDNeedReplace", "RestoreUrlIDNeedReplace", AveReportObjectType.RestoreUrlIDNeedReplace, AveStatus.Skipped, "You don't have permission to RestoreUrlIDNeedReplace. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceIdInUrlFailed, e);
                }

#if PerformanceLog
            }
#endif
        }

        public void RestoreLookupFields(Guid oldId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreLookupFields"))
            {
#endif
                if (!MappingManager.SiteMappingManager.NotUpdateLookupFieldCache.ContainsKey(oldId))
                {
                    return;
                }
                IAveWeb web = null;

                try
                {
                    foreach (AveLookupObject lookupObj in MappingManager.SiteMappingManager.NotUpdateLookupFieldCache[oldId])
                    {
                        log.Log(AveLogLevel.INFO, "start to restore lookup field. list title:{0}, lookup webUrl:{1}, lookup list:{2}, field id:{3}, listId:{4}", lookupObj.ListTitle, lookupObj.WebUrl, lookupObj.List, lookupObj.Id, lookupObj.ListId);

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
                            bool needUpdate = false;
                            if (!string.IsNullOrEmpty(lookupObj.WebUrl))
                            {
                                string destWebUrl = AveReplaceProcessor.UrlReplace(lookupObj.WebUrl, MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                IAveWeb lookupWeb = this.SPSite.OpenWeb(destWebUrl);
                                if (lookupWeb.Exists)
                                {
                                    field.LookupWebId = lookupWeb.ID;
                                    needUpdate = true;
                                }
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
                                Guid listId = MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(lookupObj.List)) ? MappingManager.SiteMappingManager.ListIdMapping[new Guid(lookupObj.List)] : Guid.Empty;
                                if (listId.Equals(Guid.Empty))
                                {
                                    continue;
                                }
                                if (field.LookupList != listId.ToString("B"))
                                {
                                    field.LookupList = listId.ToString("B");
                                    needUpdate = true;
                                }
                                if (MappingManager.SiteMappingManager.ListFieldsInternalNameMapping.ContainsKey(listId))
                                {
                                    if (!string.IsNullOrEmpty(field.LookupField) && MappingManager.SiteMappingManager.ListFieldsInternalNameMapping[listId].ContainsKey(field.LookupField))
                                    {
                                        field.LookupField = MappingManager.SiteMappingManager.ListFieldsInternalNameMapping[listId][field.LookupField];
                                        needUpdate = true;
                                    }
                                }
                                if (needUpdate)
                                {
                                    //AveAssemblyUtility.InvokeMethod(field, typeof(SPField), "RemoveFieldAttributeValue", new object[] { "Version" });
                                    field.RemoveFieldAttributeValue("Version");
                                    switch (lookupObj.DeleteBehavior)
                                    {
                                        case AveRelationshipDeleteBehavior.Cascade:
                                            field.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.Cascade;
                                            break;
                                        case AveRelationshipDeleteBehavior.Restrict:
                                            field.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.Restrict;
                                            break;
                                        default:
                                            break;
                                    }
                                    field.Update();
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
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore lookup field.list title. ", ex));
                    report.AddDetail(new AveWrapperReportDto("LookupFields", "LookupFields", AveReportObjectType.LookupFields, AveStatus.Skipped, "You don't have permission to restore LookupFields. " + ex.Message));
                }


                if (web != null)
                {
                    web.Dispose();
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreInfoPathDoc()
        {
            using (new AvePerformanceScope("Restore.AveSPSite.RestoreInfoPathDoc"))
            {
                try
                {
                    List<string> unRestoreGuidAndUrlInfopath = MappingManager.SiteMappingManager.UnReplaceGuidAndUrlInfoPathCache;
                    InfoPathLinkReplace infoPathLinkReplace = new InfoPathLinkReplace();
                    string fileRelativeUrl = null;
                    IAveWeb web = null;
                    IAveFile file = null;
                    if (unRestoreGuidAndUrlInfopath != null)
                    {
                        foreach (string docInfo in unRestoreGuidAndUrlInfopath)
                        {
                            try
                            {
                                string[] IDs = docInfo.Split(new char[] { ',' });
                                StringBuilder stringBuilder = new StringBuilder();
                                if (IDs.Length == 2)
                                {
                                    using (web = this.SPSite.OpenWeb(new Guid(IDs[1])))
                                    {
                                        file = web.GetFile(IDs[0]);
                                        #region 获取infopath目的端文件的完整路径
                                        stringBuilder.Append(web.Url);
                                        if (!this.SPSite.Url.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                                        {
                                            stringBuilder.Append("/");
                                        }
                                        stringBuilder.Append(file.ParentFolder.Url);
                                        if (file.ParentFolder.Url.EndsWith("Item", StringComparison.OrdinalIgnoreCase)
                                            || file.ParentFolder.Url.EndsWith("Task", StringComparison.OrdinalIgnoreCase))
                                        {
                                            stringBuilder.Append("/");//publish to list 需要的url为parentFolder+“item/”，list为Tasks类型的url为parentFolder+“Task/”
                                        }
                                        else
                                        {
                                            stringBuilder.Append("/");
                                            stringBuilder.Append(file.Name);//publish to library和content type需要的url为parentFolder+当前文件名
                                            stringBuilder.Append("/");
                                        }
                                        fileRelativeUrl = stringBuilder.ToString();
                                        #endregion
                                        #region 获取fileInfo并赋值
                                        AveDocumentInfo fileInfo = new AveDocumentInfo();
                                        fileInfo.MappingManager = this.MappingManager;
                                        //fileInfo.Version = Convert.ToInt32(IDs[2]);
                                        fileInfo.SiteId = this.SPSite.ID;
                                        fileInfo.ParentSiteServerRelativeUrl = this.ServerRelativeUrl;
                                        IAveItem item = this.ObjectModelFactory.CreateAveItem(this.SPSite);
                                        fileInfo.GUID = file.UniqueId;
                                        fileInfo.ParentId = file.ParentFolder.UniqueId;
                                        fileInfo.InternalVersion = item.GetInternalVersion(fileInfo);
                                        fileInfo.Url = fileRelativeUrl;//该url为infopath需要替换的url，并非fileInfo真正的url
                                        #endregion

                                        bool changed = false;
                                        InfoPathLinkReplace replacer = new InfoPathLinkReplace();
                                        string contentTypeId = null;
                                        bool isListForm = false;
                                        byte[] buffer = replacer.FixXSNBinary(file.OpenBinary(), fileRelativeUrl, this.MappingManager, ref changed, ref contentTypeId, ref isListForm);

                                        if (changed && file.ParentFolder.ParentList != null)
                                        {
                                            if (file.UIVersion % 512 == 0)
                                            {
                                                //file是大版本，开关list的version
                                                MajorVersionFileUpdate(web, file, contentTypeId, isListForm, buffer);
                                            }
                                            else
                                            {
                                                //file是小版本，check in Overwrite
                                                MinorVersionFileUpdate(web, file, contentTypeId, isListForm, buffer);
                                            }
                                        }
                                        else if (changed)
                                        {
                                            try
                                            {
                                                file.SaveBinary(buffer);
                                            }
                                            catch (Exception e)
                                            {
                                                log.Warn("Save InfoPath content error when parent list was null {0}", e.Message);
                                            }
                                        }

                                        file.ParentFolder.ParentList.Update();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("Restore infopath:" + fileRelativeUrl + " failed", e.ToString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Restore infopath failed ", e.ToString());
                }
            }
        }

        private void MajorVersionFileUpdate(IAveWeb web, IAveFile file, string contentTypeId, bool isListForm, byte[] buffer)
        {
            IAveList list = file.ParentFolder.ParentList;
            bool enableVersioning = list.EnableVersioning; ;
            bool enableModeration = list.EnableModeration;
            bool enableMinorVersions = list.EnableMinorVersions;
            bool versionSettingChanged = false;
            if (file.Level == AveFileLevel.Published && file.Item != null)
            {
                if (enableMinorVersions || enableModeration)
                {
                    list.EnableVersioning = false;
                    list.EnableMinorVersions = false;
                    list.EnableModeration = false;
                    versionSettingChanged = true;
                    list.Update();
                }
            }
            SaveChangedFile(web, file, contentTypeId, isListForm, buffer);
            if (versionSettingChanged)
            {
                list.EnableVersioning = enableVersioning;
                list.EnableModeration = enableModeration;
                list.EnableMinorVersions = enableMinorVersions;
                list.Update();
            }
        }

        private void MinorVersionFileUpdate(IAveWeb web, IAveFile file, string contentTypeId, bool isListForm, byte[] buffer)
        {
            file.CheckOut();
            SaveChangedFile(web, file, contentTypeId, isListForm, buffer);
            file.CheckIn(string.Empty, AveCheckinType.OverwriteCheckIn);
        }

        private void SaveChangedFile(IAveWeb web, IAveFile file, string contentTypeId, bool isListForm, byte[] buffer)
        {
            try
            {
                IAveList list = file.ParentFolder.ParentList;
                if (isListForm)
                {
                    web.SetFormForList((int)web.Language, Convert.ToBase64String(buffer), string.Empty, list.ID.ToString(), contentTypeId);
                }
                else
                {
                    byte[] buffer2 = null;
                    using (SHA256 sha = new SHA256Managed())
                    {
                        buffer2 = sha.ComputeHash(buffer);
                    }
                    string value = Convert.ToBase64String(buffer2);
                    file.SaveBinary(buffer);
                    file.Properties["ipfs_streamhash"] = value;
                    file.Update();
                }
            }
            catch (Exception e)
            {
                log.Warn("Save InfoPath content error {0}", e.Message);
            }
        }

        /// <summary>
        /// 用于还原一些需要放到最后还原的url记录
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_auditlogreportstoragelocation:Property of site.")]
        public void RestoreUrlNeedPost()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUrlNeedPost"))
            {
#endif
                try
                {
                    foreach (string key in MappingManager.SiteMappingManager.UrlNeedPostAction.Keys)
                    {
                        try
                        {
                            if (key.Equals("PortalUrl", StringComparison.OrdinalIgnoreCase))
                            {
                                SPSite.PortalUrl = AveReplaceProcessor.UrlReplace(MappingManager.SiteMappingManager.UrlNeedPostAction[key], this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                            }
                            else if (key.Equals("SRCH_ENH_FTR_URL", StringComparison.OrdinalIgnoreCase))
                            {
                                this.SPSite.RootWeb.AllProperties[key] = AveReplaceProcessor.UrlReplace(MappingManager.SiteMappingManager.UrlNeedPostAction[key], this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                this.SPSite.RootWeb.Update();
                            }
                            else if (key.Equals("_auditlogreportstoragelocation", StringComparison.OrdinalIgnoreCase))
                            {
                                this.SPSite.RootWeb.AllProperties[key] = AveReplaceProcessor.UrlReplace(MappingManager.SiteMappingManager.UrlNeedPostAction[key], this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                this.SPSite.RootWeb.Update();
                            }
                            else if (key.Equals("SRCH_TRAGET_RESULTS_PAGE", StringComparison.OrdinalIgnoreCase))
                            {
                                this.SPSite.RootWeb.AllProperties[key] = AveReplaceProcessor.UrlReplace(MappingManager.SiteMappingManager.UrlNeedPostAction[key], this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                this.SPSite.RootWeb.Update();
                            }
                            else if (key.Equals("SRCH_ENH_FTR_URL_SITE", StringComparison.OrdinalIgnoreCase))
                            {
                                this.SPSite.RootWeb.AllProperties[key] = AveReplaceProcessor.UrlReplace(MappingManager.SiteMappingManager.UrlNeedPostAction[key], this.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), this.SourceSiteInfo, this.ServerRelativeUrl);
                                this.SPSite.RootWeb.Update();
                            }
                            else if (key.Equals("SRCH_SB_SET_SITE", StringComparison.OrdinalIgnoreCase))
                            {
                                this.SPSite.RootWeb.AllProperties[key] = ReplaceResultsPageUrl(MappingManager.SiteMappingManager.UrlNeedPostAction[key]);
                                this.SPSite.RootWeb.Update();
                            }

                            else
                            {
                                //对于未来知道的还需要处理的url可以放在这里处理
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore site. site:{0}\n error message:{1}", key, e));
                            //mLog.Warn("An error occurred while restore site {0},Exception{1}", key, e.ToString());
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore url need post. ", ex));
                    report.AddDetail(new AveWrapperReportDto("RestoreUrlNeedPost", "RestoreUrlNeedPost", AveReportObjectType.RestoreUrlNeedPost, AveStatus.Skipped, "You don't have permission to RestoreUrlNeedPost. " + ex.Message));
                }
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreMasterPageProperty"))
            {
#endif
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
                                this.Publishing.SetWebMasterPageInfo(pageInfo, web, destPageUrl);
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
                    report.AddDetail(new AveWrapperReportDto("RestoreMasterPageProperty", "RestoreMasterPageProperty", AveReportObjectType.RestoreMasterPageProperty, AveStatus.Skipped, "You don't have permission to RestoreMasterPageProperty. " + ex.Message));
                }

#if PerformanceLog
            }
#endif
        }

        public void RestoreUnRestoreWebPart()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUnRestoreWebPart"))
            {
#endif
                try
                {
                    //if (this.SPContextKind != AveContextKind.ClientObjectModel)
                    //{
                    if (MappingManager.SiteMappingManager.UnRestoreWebPartCache != null)
                    {
                        foreach (Guid listIdKey in MappingManager.SiteMappingManager.UnRestoreWebPartCache.Keys)
                        {
                            if (true)//MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(listIdKey))
                            {
                                //Guid listId = MappingManager.SiteMappingManager.ListIdMapping[listIdKey];
                                try
                                {
                                    foreach (Guid webIdKey in MappingManager.SiteMappingManager.UnRestoreWebPartCache[listIdKey].Keys)
                                    {
                                        IAveWeb web = null;
                                        try
                                        {
                                            web = mSPSite.OpenWeb(webIdKey);
                                            //web = mSPSite.AllWebs[webIdKey];
                                            foreach (KeyValuePair<string, List<object>> pair in MappingManager.SiteMappingManager.UnRestoreWebPartCache[listIdKey][webIdKey])
                                            {
                                                try
                                                {
                                                    IAveFile file = web.GetFile(pair.Key);
                                                    AveSPDoc spDoc = new AveSPDoc(this);
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
                                                    spDoc.AveSPItem.SPFile = file;
                                                    spDoc.Web = file.Web;
                                                    spDoc.SPFile = file;
                                                    spDoc.SetRestoreOption(RestoreOption);
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
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore webPart in site post action.error:{0}", ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("RestoreWebPar", "RestoreWebPar", AveReportObjectType.WebPart, AveStatus.Skipped, "You don't have permission to restore webpart. " + ex.Message));
                }
                //}
                //else
                //{
                //    AveSPWebPart postRestoreWebPart = new AveSPWebPart(this);
                //    postRestoreWebPart.PostRestoreWebParts();
                //}
#if PerformanceLog
            }
#endif
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

        public void RestoreProjectWebGuidValues()
        {
            try
            {
                foreach (Guid projectPolicyItemListId in MappingManager.SiteMappingManager.ProjectWebGuidMapping.Keys)
                {
                    IAveWeb rootWeb = mSPSite.RootWeb;
                    IAveList projectPolicyItemList = rootWeb.Lists.GetById(projectPolicyItemListId);
                    foreach (KeyValuePair<int, Guid> itemIdAndWebGuid in MappingManager.SiteMappingManager.ProjectWebGuidMapping[projectPolicyItemListId])
                    {
                        int itemId = MappingManager.SiteMappingManager.GetMappingItemId(projectPolicyItemListId, itemIdAndWebGuid.Key);
                        IAveListItem item = projectPolicyItemList.GetItemById(itemId);
                        item["ProjectWebGuid"] = MappingManager.SiteMappingManager.WebIDMapping[itemIdAndWebGuid.Value];
                        item.SystemUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(string.Format("Error occurred when restore ProjectWebGuid fields, error detail : {0}", ex.ToString()));
            }
            finally
            {
                MappingManager.SiteMappingManager.ProjectWebGuidMapping.Clear();
            }
        }

        public void RestoreLookupFieldValues()
        {
            Guid[] keys = new Guid[MappingManager.SiteMappingManager.LookupFieldValues.Keys.Count];
            MappingManager.SiteMappingManager.LookupFieldValues.Keys.CopyTo(keys, 0);
            foreach (Guid key in keys)
            {
                RestoreLookupFieldValues(key);
            }
        }

        public void RestoreDependentUrlFieldValues()
        {
            string[] keys = new string[MappingManager.SiteMappingManager.DependentUrlFieldValues.Keys.Count];
            MappingManager.SiteMappingManager.DependentUrlFieldValues.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
            {
                RestoreDependentUrlFieldValues(key, Guid.Empty);
            }
        }

        public void RestoreUnrestoredWebParts()
        {

        }

        /// <summary>
        /// Update list setting that needs to be restored at the end.
        /// </summary>
        public void RestoreEndListSettings()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreEndListSettting"))
            {
#endif
                try
                {
                    foreach (var info in MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping)
                    {
                        Guid webId = info.Key;
                        IAveWeb web = mSPSite.OpenWeb(webId);
                        web.ReloadWeb();
                        foreach (var listInfo in info.Value)
                        {
                            Guid listId = listInfo.Key;
                            AveNoImmediateListSettingInfo listSettingInfo = listInfo.Value;
                            try
                            {
                                IAveList list = web.Lists.GetById(listId);
                                if (list != null && listSettingInfo != null)
                                {
                                    log.Info($"[RestoreEndListSettings]EnableAssignEmail:{list.EnableAssignToEmail},SourceEnableAssignToEmail:{listSettingInfo.SourceEnableAssignToEmail},TargetEnableAssignToEmail:{listSettingInfo.TargetEnableAssignToEmail},LastItemRestoreFinishedTimePoint:{listSettingInfo.LastItemRestoreFinishedTimePoint}");
                                    bool needUpdate = false;
                                    if(NeedUpdateEnableAssignToEmailProperty(listSettingInfo, list))
                                    {
                                        needUpdate = true;
                                        log.Info($"RestoreEndListSettting:List:[{listId.ToString()}] property,EnableAssignToEmail:[{list.EnableAssignToEmail}]");
                                    }

                                    if (needUpdate)
                                    {
                                        WaitForListSettingsAvailable(listSettingInfo);
                                        list.Update();
                                        log.Info($"RestoreEndListSettting:List:[{listId.ToString()}] Success");
                                    }
                                }
                                else
                                {
                                    throw new InvalidDataException($"List:[{list != null}] or ListSettingInfo:[{listSettingInfo != null}] Not Found");
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("An error occurred while restore end list:[{0}] settting in web:[{1}] due to [{2}]", listId.ToString(), webId.ToString(), e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while RestoreEndListSettting due to [{0}]", e);
                }
#if PerformanceLog
            }
#endif
        }

        private bool NeedUpdateEnableAssignToEmailProperty(AveNoImmediateListSettingInfo listSettingInfo, IAveList list)
        {
            if (listSettingInfo == null || listSettingInfo.LastItemRestoreFinishedTimePoint == default(DateTime) || list == null)
            {
                log.Info($"WaitForListSettingsAvailable param_listSettingInfo:[{listSettingInfo == null}],list:[{list == null}] is invalid");
                return false;
            }
            if (listSettingInfo.SourceEnableAssignToEmail == null)
            {
                //源端空值,保留目的端setting
                list.EnableAssignToEmail = listSettingInfo.TargetEnableAssignToEmail;
                return true;
            }
            else
            {
                if (listSettingInfo.SourceEnableAssignToEmail == listSettingInfo.TargetEnableAssignToEmail && !listSettingInfo.TargetEnableAssignToEmail)
                {
                    //不做操作
                    return false;
                }
                else
                {
                    //false-true,true-true,true-false,
                    list.EnableAssignToEmail = (bool)listSettingInfo.SourceEnableAssignToEmail;
                    return true;
                }

                    //源端目的端不同
                    if (listSettingInfo.SourceEnableAssignToEmail != listSettingInfo.TargetEnableAssignToEmail)
                {
                    list.EnableAssignToEmail = (bool)listSettingInfo.SourceEnableAssignToEmail;
                    return true;
                }
                else
                {
                    //源端目的端都是true,将目的端改成true
                    if (listSettingInfo.TargetEnableAssignToEmail)
                    {
                        list.EnableAssignToEmail = (bool)listSettingInfo.SourceEnableAssignToEmail;
                        return true;
                    }
                    else
                    {
                        //不做操作
                        return false;
                    }
                }
            }
        }

        private void WaitForListSettingsAvailable(AveNoImmediateListSettingInfo listSettingInfo)
        {
            if (listSettingInfo == null || listSettingInfo.LastItemRestoreFinishedTimePoint==default(DateTime))
            {
                log.Warn($"WaitForListSettingsAvailable param_listSettingInfo:[{listSettingInfo==null}] is invalid");
                return;
            }
            try
            {
                int enableAssignToEmailWaitTime = 1000 * 60 * 3;//3min
                int timeDiff = (int)DateTime.UtcNow.Subtract(listSettingInfo.LastItemRestoreFinishedTimePoint).TotalMilliseconds;
                if (timeDiff < enableAssignToEmailWaitTime)
                {
                    log.Info($"Start to wait ListSettings_EnableAssignToEmail Available,sleep time:[{enableAssignToEmailWaitTime - timeDiff}]");
                    System.Threading.Thread.Sleep(enableAssignToEmailWaitTime - timeDiff);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while WaitForListSettingsAvailable due to [{0}]", e);
            }
        }

        public void RestoreLookupFieldValues(Guid ID)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreLookupFieldValues"))
            {
#endif
                try
                {
                    if (MappingManager.SiteMappingManager.LookupFieldValues.ContainsKey(ID))
                    {
                        foreach (Guid webId in MappingManager.SiteMappingManager.LookupFieldValues[ID].Keys)
                        {
                            IAveWeb web = null;
                            try
                            {
                                Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>> webValueDic = MappingManager.SiteMappingManager.LookupFieldValues[ID][webId];

                                web = mSPSite.OpenWeb(webId);
                                foreach (Guid listId in webValueDic.Keys)
                                {
                                    Dictionary<int, Dictionary<int, Dictionary<Guid, object>>> listValueDic = webValueDic[listId];

                                    try
                                    {
                                        Dictionary<Guid, AveLookupObject> fieldMap = null;
                                        if (MappingManager.SiteMappingManager.LookupFieldCache.ContainsKey(listId))
                                        {
                                            fieldMap = MappingManager.SiteMappingManager.LookupFieldCache[listId];
                                        }
                                        else
                                        {
                                            log.Log(AveLogLevel.WARN, string.Format("Can't find the lookup field mapping of List, list ID: {0}.", listId));
                                            //mLog.Warn("Can't find the lookup field mapping of List, list ID: {0}.", listId.ToString());
                                        }

                                        IAveList list = web.Lists.GetById(listId);
                                        log.Info("start to restore item under list: ID {0} Title {1}", listId, list.Title);
                                        foreach (int itemId in listValueDic.Keys)
                                        {
                                            Dictionary<int, Dictionary<Guid, object>> itemValueDic = listValueDic[itemId];

                                            if (itemId <= 0)
                                            {
                                                continue;
                                            }
                                            IAveListItem item = list.GetItemById(itemId);
                                            log.Info("Get item in this list: {0}", itemId);
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
                                            #endregion
                                            bool needUpdate = false;
                                            var stringBuilder = new StringBuilder();
                                            foreach (int version in itemValueDic.Keys)
                                            {
                                                log.Info("Version: {0}  itemUIVersion: {1}", version, itemUIVersion);
                                                foreach (Guid fieldId in itemValueDic[version].Keys)
                                                {
                                                    log.Info("Field: {0}", fieldId);
                                                    AveLookupObject obj = null;
                                                    Guid lookupListid = Guid.Empty;
                                                    if (fieldMap != null && fieldMap.ContainsKey(fieldId))
                                                    {
                                                        log.Info("Get this field from field mapping");
                                                        obj = fieldMap[fieldId];
                                                        if (AveTypeHelper.IsGuid(obj.List))
                                                        {
                                                            lookupListid = new Guid(obj.List);
                                                            lookupListid = MappingManager.SiteMappingManager.GetMappingList(mSPSite, webId, obj.ListTitle, lookupListid);
                                                            if (lookupListid == Guid.Empty)
                                                            {
                                                                lookupListid = new Guid(obj.List);
                                                            }
                                                        }
                                                    }
                                                    if (lookupListid == Guid.Empty && ID != Guid.Empty)
                                                    {
                                                        lookupListid = ID;
                                                        //TODO: Warn log
                                                    }

                                                    IAveFieldLookup field = list.Fields.GetById(fieldId) as IAveFieldLookup;
                                                    log.Info("field.ReadOnlyField: {0}", field.ReadOnlyField);
                                                    if (field.ReadOnlyField)
                                                    {
                                                        stringBuilder.AppendFormat("The field:{0} is readonly\r\n", field.Title);
                                                        continue;
                                                    }

                                                    ArrayList valueList = itemValueDic[version][fieldId] as ArrayList;
                                                    if (valueList != null && valueList.Count != 0)
                                                    {
                                                        if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                                                        {
                                                            //if (AveTaxonomyField.UpdateTaxonomyFieldValue(this, item, field, valueList))
                                                            //{
                                                            //    valueList.Clear();
                                                            //    needUpdate = true;
                                                            //}
                                                        }
                                                        else
                                                        {
                                                            Dictionary<int, int> itemMapping = null;
                                                            if (MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(lookupListid))
                                                            {
                                                                itemMapping = MappingManager.SiteMappingManager.ItemIdMapping[lookupListid];
                                                            }
                                                            if (!field.AllowMultipleValues && valueList.Count == 1)
                                                            {
                                                                int lookupItemId = (int)valueList[0];
                                                                if (itemMapping != null && itemMapping.ContainsKey(lookupItemId))
                                                                {
                                                                    lookupItemId = itemMapping[lookupItemId];
                                                                }
                                                                else
                                                                {
                                                                    if (!SetLookupFieldSourceValue)
                                                                    {
                                                                        continue;
                                                                    }
                                                                }
                                                                //if (itemUIVersion == version)
                                                                //{
                                                                    stringBuilder.AppendFormat("key:{0}, value:{1},", field.Title, lookupItemId);
                                                                    item[fieldId] = lookupItemId.ToString();
                                                                    needUpdate = true;
                                                            //}//不keep version号后该段逻辑受到影响导致lookup value无法还原
                                                            if (mSPContextKind != AveContextKind.ClientObjectModel)
                                                                {
                                                                    new AveSPItem(this).UpdateColumnByNative(SPSite.ID, item, version, field.RowOrdinal, field.ColName, lookupItemId);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                IAveFieldLookupValueCollection lookupCol = mOMFactory.CreateFieldLookupValueCollection();
                                                                foreach (int id in valueList)
                                                                {
                                                                    int lookupItemId = id;
                                                                    if (itemMapping != null && itemMapping.ContainsKey(id))
                                                                    {
                                                                        lookupItemId = itemMapping[lookupItemId];
                                                                    }
                                                                    else
                                                                    {
                                                                        if (!SetLookupFieldSourceValue)
                                                                        {
                                                                            continue;
                                                                        }
                                                                    }
                                                                    lookupCol.Add(mOMFactory.CreateFieldLookupValue(lookupItemId, "Title"));
                                                                }
                                                                if (lookupCol.Count == 0)
                                                                {
                                                                    continue;
                                                                }
                                                                //if (itemUIVersion == version)
                                                                //{
                                                                    stringBuilder.AppendFormat("key:{0}, value:{1},", field.Title, lookupCol);
                                                                    item[fieldId] = lookupCol;
                                                                    needUpdate = true;
                                                            //} //不keep version号后该段逻辑受到影响导致lookup value无法还原
                                                            if (mSPContextKind != AveContextKind.ClientObjectModel)
                                                            {
                                                                List<int> values = new List<int>();
                                                                foreach (IAveFieldLookupValue value in lookupCol)
                                                                {
                                                                    values.Add(value.LookupId);
                                                                }
                                                                if (item.Level == AveFileLevel.Checkout)
                                                                {
                                                                    AveSPItem.RemoveDatajunctionByNative(mQueryService, item, fieldId, listId, version);
                                                                }
                                                                new AveSPItem(this).CreateDatajunctionByNative(item, fieldId, listId, version, values);
                                                            }
                                                        }
                                                        }
                                                    }
                                                }
                                            }

                                            if (needUpdate)
                                            {
                                                //SAAS-10083 check out的file使用item.update()更新,非check out的file使用item.systemupdate()更新.
                                                if (itemLevel != AveFileLevel.Checkout)
                                                {
                                                    log.Log(AveLogLevel.WARN, string.Format("Update lookup field by system update.List title is {0}, Item id is {1}, details:{2}", list.Title, itemId.ToString(), stringBuilder.ToString()));
                                                    //SAAS-10063 防止以ValidateUpdateListItem更新Publish状态的文件时涨version的情况
                                                    bool isEnableMinorVersionsChanged = false;
                                                    if (itemUIVersion % 512 == 0 && list.EnableMinorVersions)
                                                    {
                                                        list.EnableMinorVersions = false;
                                                        list.Update();
                                                        isEnableMinorVersionsChanged = true;
                                                    }
                                                    item.SystemUpdate();
                                                    if (isEnableMinorVersionsChanged)
                                                    {
                                                        list.EnableMinorVersions = true;
                                                        list.Update();
                                                    }
                                                }
                                                else
                                                {
                                                    log.Log(AveLogLevel.WARN, string.Format("Update lookup field by normal update.List title is {0}, Item id is {1}, details:{2}", list.Title, itemId.ToString(), stringBuilder.ToString()));

                                                    bool isEnableMinorVersionsChanged = false;
                                                    bool enableMinorVersions = list.EnableMinorVersions;
                                                    if (itemUIVersion % 512 == 0 && list.EnableMinorVersions)
                                                    {
                                                        list.EnableMinorVersions = false;
                                                        list.Update();
                                                        isEnableMinorVersionsChanged = true;
                                                    }
                                                    else if (itemUIVersion % 512 > 0 && !list.EnableMinorVersions)
                                                    {
                                                        list.EnableMinorVersions = true;
                                                        list.Update();
                                                        isEnableMinorVersionsChanged = true;
                                                    }
                                                    item.Update();
                                                    if (isEnableMinorVersionsChanged)
                                                    {
                                                        list.EnableMinorVersions = enableMinorVersions;
                                                        list.Update();
                                                    }
                                                }
                                                if (IsListIncludeEnableAssignEmail(list))
                                                {
                                                    UpdateListRestoreFinishedTimePoint(list);
                                                }
                                            }
                                        }
                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception le)
                                    {
                                        log.Log(AveLogLevel.WARN, string.Format("An error occurred when restore the lookup field value in list. web id:{0}, list id:{1}\n error message:{2}", webId, listId, le));
                                        //mLog.Warn("Error happenned when restore the lookup field value in list, web id: {0}, list id: {1}. Reason: {2}", webId, listId, le.ToString());
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
                            finally
                            {
                                if (web != null)
                                {
                                    web.Dispose();
                                }
                            }
                        }
                        MappingManager.SiteMappingManager.LookupFieldValues.Remove(ID);
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the lookup field value in SPWeb. ", ex));
                    report.AddDetail(new AveWrapperReportDto("RestoreLookupFieldValues", "RestoreLookupFieldValues", AveReportObjectType.RestoreLookupFieldValues, AveStatus.Skipped, "You don't have permission to RestoreLookupFieldValues. " + ex.Message));
                }

#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// Try to update Url Filed values.
        /// </summary>
        /// <param name="dependentlistUrl">List server relative url or the original Url filed value</param>
        /// <param name="dependentlistId">List id or Guid.Empty</param>
        public void RestoreDependentUrlFieldValues(string dependentlistUrl, Guid dependentlistId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreDependentUrlFieldValues"))
            {
#endif
                try
                {
                    var dependentValues = from tempValue in MappingManager.SiteMappingManager.DependentUrlFieldValues where tempValue.Key.ToLower().StartsWith(dependentlistUrl.ToLower() + "/attachments/") || tempValue.Key.Equals(dependentlistUrl) select tempValue;

                    List<string> needRemoveKeys = new List<string>();
                    foreach (var temp in dependentValues)
                    {
                        log.Info("Begin to restore Url field value, value url:{0}, list url:{1}", temp.Key, dependentlistUrl);
                        string key = temp.Key;
                        foreach (var webId in temp.Value.Keys)
                        {
                            try
                            {
                                IAveWeb web = mSPSite.OpenWeb(webId);
                                foreach (var tempListId in temp.Value[webId].Keys)
                                {
                                    IAveList list = web.Lists[tempListId];

                                    foreach (var originalItemId in temp.Value[webId][tempListId].Keys)
                                    {
                                        if (!MappingManager.SiteMappingManager.ItemIdMapping[tempListId].ContainsKey(originalItemId))
                                        {
                                            continue;
                                        }

                                        int itemId = MappingManager.SiteMappingManager.ItemIdMapping[tempListId][originalItemId];
                                        IAveListItem item = list.GetItemById(itemId);
                                        if (item == null)
                                        {
                                            continue;
                                        }
                                        log.Info("Get item in this list: {0}", itemId);
                                        int itemUIVersion = new AveSPItem(this).GetCurrentUIVersion(mSPSite.ID, item);

                                        bool needUpdate = false;
                                        foreach (var versionId in temp.Value[webId][tempListId][originalItemId].Keys)
                                        {
                                            if (versionId != itemUIVersion)//need take versionId > itemUIversion into consideration
                                            {
                                                continue;
                                            }
                                            foreach (string fieldInternalName in temp.Value[webId][tempListId][originalItemId][versionId].Keys)
                                            {
                                                var tempValue = temp.Value[webId][tempListId][originalItemId][versionId][fieldInternalName];
                                                if (tempValue is IAveFieldUrlValue)
                                                {

                                                    IAveFieldUrlValue urlValue = tempValue as IAveFieldUrlValue;
                                                    //urlValue.Url == dependentlistUrl  means site last post action.
                                                    if (!urlValue.Url.Equals(dependentlistUrl) && !Guid.Empty.Equals(dependentlistId))
                                                    {
                                                        if (MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(dependentlistId))
                                                        {
                                                            string attachmentUrl = dependentlistUrl.ToLower() + "/attachments/";
                                                            int index = urlValue.Url.ToLower().IndexOf(attachmentUrl);
                                                            if (index >= 0)
                                                            {
                                                                int endIndex = urlValue.Url.ToLower().IndexOf("/", attachmentUrl.Length);
                                                                string idFromString = urlValue.Url.Substring(attachmentUrl.Length, endIndex - (index + attachmentUrl.Length));

                                                                int needMapId;
                                                                if (Int32.TryParse(idFromString, out needMapId))
                                                                {
                                                                    if (MappingManager.SiteMappingManager.ItemIdMapping[dependentlistId].ContainsKey(needMapId))
                                                                    {
                                                                        int mappedId = MappingManager.SiteMappingManager.ItemIdMapping[dependentlistId][needMapId];
                                                                        if (mappedId == needMapId)
                                                                        {
                                                                            continue;
                                                                        }
                                                                        urlValue.Url = attachmentUrl + mappedId + urlValue.Url.Substring(endIndex);
                                                                        if (urlValue.Description.ToLower().Contains(attachmentUrl + needMapId + "/"))
                                                                        {
                                                                            urlValue.Description = urlValue.Description.ToLower().Replace(attachmentUrl + needMapId + "/", attachmentUrl + mappedId + "/");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    item[fieldInternalName] = urlValue;
                                                    needUpdate = true;
                                                }
                                            }
                                        }
                                        if (needUpdate)
                                        {
                                            item.SystemUpdate();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.WARN, string.Format("Error occurred when restore the dependent url field value in SPWeb. Message:{0}", ex));
                            }
                        }
                        needRemoveKeys.Add(temp.Key);
                    }
                    lock (MappingManager.SiteMappingManager.DependentUrlFieldValues)
                    {
                        foreach (var key in needRemoveKeys)
                        {
                            MappingManager.SiteMappingManager.DependentUrlFieldValues.Remove(key);
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Permission error occurred when restore the dependent url field value in SPWeb. Message:{0}", ex));
                    report.AddDetail(new AveWrapperReportDto("RestoreDependentUrlFieldValues", "RestoreDependentUrlFieldValues", AveReportObjectType.RestoreUrlIDNeedReplace, AveStatus.Skipped, "You don't have permission to RestoreDependentUrlFieldValues. " + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Unknown error occurred when restore the dependent url field value in SPWeb. Message:{0}", ex));
                }

#if PerformanceLog
            }
#endif
        }

        public AveNoImmediateListSettingInfo GetOrCreateEndRestoreListSettingsInfo(IAveList list)
        {
            try
            {
                if (list == null || MappingManager == null || MappingManager.SiteMappingManager == null) throw new ArgumentNullException($"GetEndRestoreListSettingsInfo param is null");
                if (!MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping.ContainsKey(list.ParentWeb.ID))
                {
                    MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping[list.ParentWeb.ID] = new Dictionary<Guid, AveNoImmediateListSettingInfo>();
                }

                if (!MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping[list.ParentWeb.ID].ContainsKey(list.ID))
                {
                    log.Info($"Not found list:[{list.ID}] in NeedEndRestoreListSettingsMapping");
                    MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping[list.ParentWeb.ID][list.ID] = new AveNoImmediateListSettingInfo();
                }
                return MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping[list.ParentWeb.ID][list.ID];
            }
            catch (Exception e)
            {
                log.Error("An error occurred while AddNeedEndRestoreListSettingsMapping due to [{0}]", e);
                return null;
            }
        }

        public AveNoImmediateListSettingInfo GetEndRestoreListSettingsInfo(IAveList list)
        {
            try
            {
                return MappingManager.SiteMappingManager.NeedEndRestoreListSettingsMapping[list.ParentWeb.ID][list.ID];
            }
            catch(Exception e)
            {
                log.Error($"Not found list:[{list.ID}] in NeedEndRestoreListSettingsMapping,ex:[{e}]");
                return null;
            }
        }

        public bool IsListIncludeEnableAssignEmail(IAveList list)
        {
            return list != null && (list.BaseTemplate == AveListTemplateType.Tasks || list.BaseTemplate == AveListTemplateType.IssueTracking || list.BaseTemplate == AveListTemplateType.TasksWithTimelineAndHierarchy);
        }

        public void UpdateListRestoreFinishedTimePoint(IAveList list)
        {
            try
            {
                var settingInfo = GetOrCreateEndRestoreListSettingsInfo(list);
                if (settingInfo != null)
                {
                    settingInfo.LastItemRestoreFinishedTimePoint = DateTime.UtcNow;
                    log.Info($"Update time point success for list:[{list.FullUrl()}] sub all items restore finished.");
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while UpdateListRestoreFinishedTimePoint due to [{0}]", e);
            }
        }

        /*public static Guid GetListByNative(AveSqlConnection sqlCon, Guid webId, string title)
        {
            Guid id = Guid.Empty;
            if (String.IsNullOrEmpty(title))
            {
                return id;
            }
            string text = "SELECT tp_Id FROM AllLists WHERE tp_WebId=@WebId AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
            sqlCon.AddParameter("@WebId", webId);
            sqlCon.AddParameter("@Title", title);
            using (SqlDataReader reader = sqlCon.ExecuteReader(text))
            {
                if (reader.Read())
                {
                    id = reader.GetGuid(0);
                }
            }
            return id;
        }*/

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

        /*public static Guid GetWebByNative(AveSqlConnection sqlCon, string url)
        {
            Guid id = Guid.Empty;
            string text = "SELECT Id FROM Webs WHERE FullUrl=@Url";
            sqlCon.AddParameter("@Url", url.Trim('/'));
            using (SqlDataReader reader = sqlCon.ExecuteReader(text))
            {
                if (reader.Read())
                {
                    id = reader.GetGuid(0);
                }
            }
            return id;
        }*/

        //public void AddUnRestoreWebPartInfo(Guid webId, Guid listId, Guid fileId, string info)
        //{
        //    if (!this.MappingManager.SiteMappingManager.UnRestoreWebPartCache.ContainsKey(listId))
        //    {
        //        this.MappingManager.SiteMappingManager.UnRestoreWebPartCache.Add(listId, new Dictionary<Guid, Dictionary<string, List<object>>>());
        //    }
        //    if (!this.MappingManager.SiteMappingManager.UnRestoreWebPartCache[listId].ContainsKey(webId))
        //    {
        //        this.MappingManager.SiteMappingManager.UnRestoreWebPartCache[listId].Add(webId, new Dictionary<string, List<object>>());
        //    }
        //    if (!this.MappingManager.SiteMappingManager.UnRestoreWebPartCache[listId][webId].ContainsKey(fileId.ToString()))
        //    {
        //        this.MappingManager.SiteMappingManager.UnRestoreWebPartCache[listId][webId].Add(fileId.ToString(), new List<object>());
        //    }
        //    this.MappingManager.SiteMappingManager.UnRestoreWebPartCache[listId][webId][fileId.ToString()].Add(info);
        //}

        /// <summary>
        /// added for languageMapping, get dest Title or Name by LanguageMapping Type
        /// </summary>
        /// <param name="name">source name</param>
        /// <param name="languageType">including listMapping, fieldMapping, permissonMapping</param>
        /// <returns></returns>
        public string GetNameByLanguageMapping(string name, AveLanguageMappingType languageType)
        {
            string replaceName = name;
            if (AveLanguageProcesser == null)
            {
                return replaceName;
            }
            switch (languageType)
            {
                case AveLanguageMappingType.ListMapping:
                    if (AveLanguageProcesser.ListMapping.ContainsKey(name))
                    {
                        replaceName = AveLanguageProcesser.ListMapping[name].ToString();
                    }
                    break;
                case AveLanguageMappingType.ViewMapping:
                    if (AveLanguageProcesser.ViewMapping.ContainsKey(name))
                        replaceName = AveLanguageProcesser.ViewMapping[name].ToString();
                    break;
                case AveLanguageMappingType.ContentTypeMapping:
                    if (AveLanguageProcesser.ContentTypeMapping.ContainsKey(name))
                        replaceName = AveLanguageProcesser.ContentTypeMapping[name].ToString();
                    break;
                case AveLanguageMappingType.FieldMapping:
                    if (AveLanguageProcesser.FieldMapping.ContainsKey(name))
                    {
                        replaceName = AveLanguageProcesser.FieldMapping[name].ToString();
                    }
                    else if (AveLanguageProcesser.ListMappingFromRes.ContainsKey(name))
                    {
                        replaceName = AveLanguageProcesser.ListMappingFromRes[name].ToString();
                    }
                    break;
                case AveLanguageMappingType.PermissionMapping:
                    if (AveLanguageProcesser.PermissionMapping.ContainsKey(name))
                        replaceName = AveLanguageProcesser.PermissionMapping[name].ToString();
                    break;
                case AveLanguageMappingType.NavigationMapping:
                    if (AveLanguageProcesser.NavigationMapping.ContainsKey(name))
                    {
                        replaceName = AveLanguageProcesser.NavigationMapping[name].ToString();
                    }
                    else if (AveLanguageProcesser.ListMapping.ContainsKey(name))//SAAS-14689
                    {
                        replaceName = AveLanguageProcesser.ListMapping[name].ToString();
                    }
                    break;
                default:
                    //Error message: wrong type
                    break;
            }
            if (!string.Equals(name, replaceName, StringComparison.OrdinalIgnoreCase))
            {
                log.Debug("Language mapping processed.Mapping from {0} to {1},Object Type:{2}", name, replaceName, languageType);
            }
            return replaceName;

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

       /* public IAveWeb GetWebByName(string name)
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
        }*/

       /* public IAveWeb OpenWeb(string relativeUrl)
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
        }*/

        public string ApplicationName
        {
            get
            {
                return mWebAppName;
            }
        }

      /*  private string GetApplicationName(string siteUrl)
        {
            string appName = siteUrl;
            int maxPrefix = 8; //count of letter 'https://'
            int index = mSiteUrl.IndexOf("/", maxPrefix, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                appName = mSiteUrl.Substring(0, index);
            }
            return appName;
        }*/

        public Guid GetWeb(IAveBackupRestoreQueryService queryService, string p)
        {
            return mSPSite.GetWeb(queryService, p);
        }

        public string IdReplace(string oldUrl, ref bool needReplaceLast)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.IdReplace"))
            {
#endif
                try
                {
                    Dictionary<string, string> idDic = new Dictionary<string, string>();
                    string tempUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
                    if (string.IsNullOrEmpty(tempUrl))
                    {
                        return oldUrl;
                    }
                    string idUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
                    string[] ids = idUrl.Split('&');
                    foreach (string id in ids)
                    {
                        string[] kv = id.Split('=');
                        if (kv.Length == 2)
                        {
                            idDic.Add(kv[0], kv[1]);
                        }
                    }
                    foreach (KeyValuePair<string, string> kvp in idDic)
                    {
                        try
                        {
                            if (kvp.Key.Equals("list", StringComparison.OrdinalIgnoreCase))
                            {
                                Guid id = new Guid(kvp.Value);
                                Guid destId = MappingManager.SiteMappingManager.GetListIdMapping(id);
                                if (destId != Guid.Empty)
                                {
                                    //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                    int index = idUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                    string sourceId = idUrl.Substring(index, id.ToString().Length);
                                    idUrl = idUrl.Replace(sourceId, destId.ToString());
                                }
                                else
                                {
                                    needReplaceLast = true;
                                    return oldUrl;
                                }
                            }
                            else if (kvp.Key.Equals("sourcedoc", StringComparison.OrdinalIgnoreCase))
                            {
                                Guid id = new Guid(kvp.Value);
                                Guid destId = MappingManager.SiteMappingManager.GetDocumentUniqueIdMapping(id);
                                if (destId != Guid.Empty)
                                {
                                    //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                    int index = idUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                    string sourceId = idUrl.Substring(index, id.ToString().Length);
                                    idUrl = idUrl.Replace(sourceId, destId.ToString());
                                }
                                else
                                {
                                    needReplaceLast = true;
                                    return oldUrl;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetIdFailed, e);
                        }
                    }
                    return oldUrl.Replace(tempUrl, idUrl);
                }
                catch (Exception ex)
                {
                    log.Warn("Replace Id Error. Message:" + ex.ToString());

                }
                return oldUrl;

#if PerformanceLog
            }
#endif
        }

       /* internal void UpdateUserInfoByNative(IAveUser _user, AveUserInfo old)
        {
            throw new NotImplementedException();
        }*/

        internal IAveWeb GetCheckoutWeb(IAveWeb web, IAveUser user, Guid fileId)
        {
            return GetCheckoutWeb(this.SPSite.ID, web, user, fileId);
        }

        internal IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId)
        {
            return mSPSite.GetCheckoutWeb(siteId, web, user, fileId);
        }

       /* public void ClearSiteGroups()
        {
            try
            {
                for (int i = mSPSite.RootWeb.SiteGroups.Count - 1; i >= 0; i--)
                {
                    mSPSite.RootWeb.SiteGroups.Remove(i);
                }
            }
            catch (Exception ex)
            {
                log.Warn("ClearSiteGroups Error. Message:" + ex.ToString());
            }
        }*/

        internal void RestoreUnupdateFile(Guid listId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUnupdateFile"))
            {
#endif
                if (this.MappingManager.SiteMappingManager.UnupdateFileCache.ContainsKey(listId))
                {
                    foreach (KeyValuePair<Guid, Dictionary<string, List<int>>> pair in this.MappingManager.SiteMappingManager.UnupdateFileCache[listId])
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
                                        info.Version = version;
                                        info.SiteId = this.SPSite.ID;
                                        info.ParentSiteServerRelativeUrl = this.ServerRelativeUrl;
                                        //info.SourceWebUrl = this.ParentFolder.ParentList.ParentWeb.WebInfo.Url;

                                        IAveItem item = this.ObjectModelFactory.CreateAveItem(this.SPSite);
                                        info.GUID = file.UniqueId;
                                        info.ParentId = file.ParentFolder.UniqueId;
                                        info.InternalVersion = item.GetInternalVersion(info);

                                        if (fileCollection.ChangeContent(this.SPSite, file, info))
                                        {
                                            //versions.Add(version);
                                            file = web.GetCheckoutFile(filePair.Key);
                                        }
                                    }
                                }
                            }
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            log.Warn("An error occurred while restoring file which is not updated. Reason: " + ex.ToString());
                        }
                        catch (Exception ex)
                        {
                            log.Error("An error occurred while restoring file which is not updated. Reason: " + ex.ToString());
                        }
                    }
                    this.MappingManager.SiteMappingManager.UnupdateFileCache.Remove(listId);
                }
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.GetHoldItemID"))
            {
#endif
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
#if PerformanceLog
            }
#endif
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
            try
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
            catch(Exception e)
            {
                log.Warn($"An error occurred while AddUnRestoreFileHoldRecordInfo. Error:{e.ToString()}");
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
                    mQueryService.UpdateListModifiedTime(listId, UnRestoreListLastModifiedTime[listId]);
                }
            }
        }

        public void RestoreWebLastModifiedTime()
        {
            if (this.mSPContextKind != AveContextKind.ClientObjectModel)
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.RestoreUnRestoreHoldRecord"))
            {
#endif
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
                    report.AddDetail(new AveWrapperReportDto("HoldRecord", "HoldRecord", AveReportObjectType.HoldRecord, AveStatus.Skipped, "You don't have permission to restore HoldRecord. " + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while RestoreUnRestoreHoldRecord. Error:{0}", ex);
                }
                finally
                {
                    var currentRecordRestrictions = this.SPSite.RootWeb.Properties.ContainsKey("ecm_siterecordrestrictions") ?
                                                             this.SPSite.RootWeb.Properties["ecm_siterecordrestrictions"] : null;
                    if (!string.Equals(currentRecordRestrictions, webLevelRecordRestrictions, StringComparison.OrdinalIgnoreCase))
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
#if PerformanceLog
            }
#endif
        }

        protected void DeclareRecordOrHoldItem(AveItemHoldRecord itemHoldRecord, IAveListItem item, ref bool isSiteProvisionHold, ref bool isWebProvisionHold, ref bool isListProvisionHold)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.DeclareRecordOrHoldItem"))
            {
#endif
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
#if PerformanceLog
            }
#endif
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


        public IReport GetReport()
        {
            return report;
        }

        public void CreateSiteCollection(AveSiteInfo siteInfo, AveCreateSiteInfo createSiteInfo)
        {
            AveBPOSAccountInfo accountInfo = new AveBPOSAccountInfo()
            {
                UserName = createSiteInfo.UserName,
                Password = CspCommunicationWrapper.UnWrapKeyToSecureString(createSiteInfo.Password),
                AdminUrl = createSiteInfo.AdminUrl
            };
            //string adminUrl = AveUrlUtility.GetSPOAdminUrl(accountInfo, siteInfo.Url, createSiteInfo.CustomerId);
            string adminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(accountInfo, siteInfo.Url);
            log.Info("O365 Admin Url is : {0}", adminUrl);
            IAveTenant aveTenant = mOMFactory.CreateTenant(adminUrl);
            var isOD4B = siteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase);
            //string ownerEmail = siteInfo.OwnerLogin.Substring(starString.Length);  //site 的owner email可能会不存在，通过loginName进行截取。
            //SAAS-25548 个别user属于O365 Unlicensed user，用这种user备份SC的时候，primary user就会变成一串字符，导致在restore sc出现valid user异常
            //所以针对此类用户，我们在还原时从profile中获取Owner
            string ownerEmail = string.Empty;
            const string starString = "i:0#.f|membership|";
            if (createSiteInfo != null && !string.IsNullOrEmpty(createSiteInfo.SiteOwnerUPN))
            {
                if (createSiteInfo.SiteOwnerUPN.StartsWith(starString, StringComparison.OrdinalIgnoreCase))
                {
                    ownerEmail = createSiteInfo.SiteOwnerUPN.Substring(starString.Length);
                }
                else
                {
                    ownerEmail = createSiteInfo.SiteOwnerUPN;
                }
            }
            else if (siteInfo.OwnerLogin.StartsWith(starString, StringComparison.OrdinalIgnoreCase))
            {
                ownerEmail = siteInfo.OwnerLogin.Substring(starString.Length);  //site 的owner email可能会不存在，通过loginName进行截取。
            }
            else
            {
                if (isOD4B)
                {
                    //对于oneDriver的还原如果获取不到owner直接抛出异常，不进行截取
                    log.Error("The owner:{0} is not a valid user", siteInfo.OwnerLogin);
                    throw new Exception(string.Format("The owner:{0} is not a valid user", siteInfo.OwnerLogin));
                }
                else
                {
                    ownerEmail = createSiteInfo.UserName;
                }
            }
            if (accountInfo != null && string.IsNullOrEmpty(accountInfo.UserName))
            {
                accountInfo.UserName = ownerEmail;
            }
            log.Info($"CreateSiteCollection.OwnerLogin:{siteInfo.OwnerLogin}.OwnerName:{siteInfo.OwnerName}.OwnerEmail:{siteInfo.OwnerEmail}.FinalOwnerEmail:{ownerEmail}.SiteOwnerUPN:{createSiteInfo.SiteOwnerUPN}.accountInfo.UserName:{accountInfo.UserName}.");
            if (isOD4B)   //SAAS-13775 支持OneDrive的还原。
            {
                //OneDrive for Business的创建过程
                IAveProfileLoader profileLoader = mOMFactory.CreateOLProfileLoader(adminUrl);
                string[] emailIds = new string[] { ownerEmail };
                Dictionary<string, object> personalSiteMessage = profileLoader.CreatePersonalSiteEnqueueBulk(emailIds, siteInfo.OwnerLogin);
                if (personalSiteMessage.ContainsKey("ErrorMessage"))
                {
                    log.Error("Create Site Collection is Failed,Error Message:{0}", (personalSiteMessage["ErrorMessage"].ToString()));
                    throw new Exception(personalSiteMessage["ErrorMessage"].ToString());
                }
            }
            else
            {
                siteInfo.WebTemplate = GetRestoredWebTemplate(siteInfo.WebTemplate);
                Dictionary<string, object> siteCollectionProperties = aveTenant.CreateSite(siteInfo.CompatibilityLevel, siteInfo.LCID, ownerEmail, createSiteInfo.StorageQuota, siteInfo.WebTemplate, 13, siteInfo.Title, mSiteUrl, createSiteInfo.ResourceQuota);   //timeZoneId = 13 默认使用美国和加拿大的时区
                if (siteCollectionProperties.ContainsKey("ErrorMessage"))
                {
                    log.Error("Create Site Collection is Failed,Error Message:{0}", (siteCollectionProperties["ErrorMessage"].ToString()));
                    throw new Exception(siteCollectionProperties["ErrorMessage"].ToString());
                }
            }
            aveTenant.SetAdmin(siteInfo.Url, accountInfo.UserName);   //SAAS-13464 创建完成后将global admin设置成所创建site的SC admin,保证数据正常还原。
        }

        private string GetRestoredWebTemplate(string siteTemplate)
        {
            if (AveSPWebTemplate.IsRetiredClassicPublishingSite(siteTemplate))
            {
                log.Warn($"use communication site template({AveSPWebTemplate.COMMUNICATION_SITE}) instead of {siteTemplate} for site because classic publishing site has retired.");
                return AveSPWebTemplate.COMMUNICATION_SITE;
            }
            else
            {
                return siteTemplate;
            }
        }

        internal void RestoreVariationsSettings()
        {
            var variationsSettings = MappingManager.SiteMappingManager.NeedRestroreVariationsSettings;
            if (variationsSettings != null && variationsSettings.Count > 0)
            {
                var relationshipsListIdKey = "_VarRelationshipsListId";
                if (SPSite.RootWeb.AllProperties.ContainsKey(relationshipsListIdKey))
                {
                    var listId = SPSite.RootWeb.AllProperties[relationshipsListIdKey];
                    if (listId != null)
                    {
                        try
                        {
                            variationsSettings[relationshipsListIdKey] = new Guid(listId.ToString());
                            SPSite.AddChangePropertiesToDataCache(variationsSettings);
                            SPSite.Update();
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error while updating site collection:{0} variations settings, relationships list Id:{1}, error:{2}", SPSite.Url, listId, ex);
                        }
                    }
                }
                else
                {
                    log.Warn("Skip restore site:{0} variations settings because can not found relationships list.", SPSite.Url);
                }
            }
        }
    }

    public enum AveRestoreGhostPageOption
    {
        NoAction,
        KeepStreamOnly,
        KeepPathOnly,
        KeepStreamAndPath
    }
}