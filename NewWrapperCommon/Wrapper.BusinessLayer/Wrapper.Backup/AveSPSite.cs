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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Common;
using AvePoint.Wrapper.Resource.Backup;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSite : IDisposable, AvePoint.Wrapper.Backup.IAveSPSite, ISPSiteExport
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IAveBackupRestoreQueryService mQueryService;
        private IAveBackupStream mSender;
        private AveLanguageProcesser mLanguageProcessor;
        private IAveSite mSPSite = null;
        private bool mUserProfileApplicationAvailable = true;
        private AveBPOSAccountInfo mAccount = null;//new AveUserAccountInfo() { Domain = "sp10", UserName = "administrator", Password = "1qaz2wsxE" };
        private List<AveUserInfo> mSiteUserInfoCache = null;
        private Dictionary<int, AveUserInfo> mSiteUserCache = null;
        private IAveServiceContext mServiceContext;
        private IAveOSocialTagManager mTagManager;
        private IAveOSocialCommentManager mCommentManager;
        private Dictionary<long, string> mUserProfiles = new Dictionary<long, string>();
        private AveMappingManager mAveMappingManager = new AveMappingManager();
        private AveRBSBackup mRBSBackup;
        private List<AveGroupInfo> mAllGroups;
        private bool? isMySite;
        public AveBackupOption BackupOption = new AveBackupOption();
        //<WebId,<ListId,<RowId,ItemGuid>>>
        private Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>> lookupListItemIdAndGuidCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>();

        public AveSiteDataCache DataCache
        {
            get;
            set;
        }

        internal Dictionary<long, string> UserProfiles
        {
            get { return mUserProfiles; }
        }

        internal IAveServiceContext ServiceContext
        {
            get
            {
                if (mServiceContext == null)
                {
                    mServiceContext = this.ObjectModelFactory.CreateServiceContext().GetContext(this.SPSite); ;
                }
                return mServiceContext;
            }
        }

        internal DateTime mSPRequestTimeout = DateTime.UtcNow;
        internal int mHoursReloadSite = 12;

        internal IAveOSocialTagManager TagManager
        {
            get
            {
                if (mTagManager == null)
                {
                    mTagManager = this.ObjectModelFactory.CreateSocialTagManager(this.ServiceContext);
                }
                return mTagManager;
            }
        }

        internal IAveOSocialCommentManager CommentManager
        {
            get
            {
                if (mCommentManager == null)
                {
                    mCommentManager = this.ObjectModelFactory.CreateSocialCommentManager(this.ServiceContext); ;
                }
                return mCommentManager;
            }
        }

        #region Added to backup social feed by Austin

        private IAveOSocialFeedManager mFeedManager;

        internal IAveOSocialFeedManager FeedManager
        {
            get
            {
                if (mFeedManager == null)
                {
                    mFeedManager = this.ObjectModelFactory.CreateSocialFeedManager();
                }
                return mFeedManager;
            }
        }

        /// <summary>
        /// 该对象不缓存，既取即用，最好使用using,保证使用后可以dispose
        /// </summary>
        internal IAveServiceContextScope GetServiceContextScope()
        {
            return this.ObjectModelFactory.CreateServiceContextScope(this.ServiceContext);
        }

        #endregion

        public List<AveGroupInfo> AllGroups
        {
            get
            {
                if (mAllGroups == null)
                {
                    mAllGroups = mSPSite.RootWeb.GroupsSerializer.GetObjectData(true) as List<AveGroupInfo>;
                    AveGroup.SetAboutMeToGroupInfos(mAllGroups, mSPSite.RootWeb);
                    if (mAllGroups == null)
                    {
                        mAllGroups = new List<AveGroupInfo>();
                    }
                }
                return mAllGroups;
            }
        }

        public AveLanguageProcesser LanguageProcessor
        {
            get { return mLanguageProcessor; }
            set { mLanguageProcessor = value; }
        }

        public IAveBackupStream Sender
        {
            get { return mSender; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public List<AveUserInfo> SiteUserInfoCache
        {
            get { return mSiteUserInfoCache; }
            set { SetSiteUserInfoCache(value); }
        }

        public Dictionary<int, AveUserInfo> SiteUserCache
        {
            get
            {
                return mSiteUserCache;
            }
        }

        public bool UserProfileApplicationAvailable
        {
            get { return mUserProfileApplicationAvailable; }
            set { mUserProfileApplicationAvailable = value; }
        }

        public AveRBSBackup RBSBackup
        {
            get
            {
                if (mRBSBackup == null)
                {
                    mRBSBackup = new AveRBSBackup(this.SPSite.ID, this.QueryService);
                }
                return mRBSBackup;
            }
        }

        internal AveContextKind SPContextKind
        {
            get { return this.ObjectModelFactory.ContextKind; }
        }

        private void SetSiteUserInfoCache(List<AveUserInfo> users)
        {
            this.mSiteUserInfoCache = users;
            mSiteUserCache = new Dictionary<int, AveUserInfo>();
            foreach (var userinfo in mSiteUserInfoCache)
            {
                mSiteUserCache.Add(userinfo.ID, userinfo);
            }
        }
        public AveSPSite(string _url, AveContextKind contextKind, AveBPOSAccountInfo userAccountInfo, IAveBackupStream _stream)
        {
            this.mAccount = userAccountInfo;
            this.mSender = _stream;

            //AveEnvironment.SiteUrl = _url;
            AveObjectModelFactory siteFactory = AveObjectModelFactory.CreateObjectModelFactory(_url, mAccount, contextKind);//mSPContextKind);
            this.mSPSite = siteFactory.CreateSite(_url);
            CheckSiteAvailable(this.mSPSite, _url);
            ObjectModelFactory = siteFactory;
            DataCache = new AveSiteDataCache(this);
            //All Server Object Model Should Get The QueryService
            if (siteFactory.ContextKind.IsServerMode())
            {
                this.mQueryService = siteFactory.CreateQueryService<IAveBackupRestoreQueryService>(this.mSPSite);
                //this.mQueryService.SetIsolationLevel(System.Data.IsolationLevel.ReadUncommitted);
                //this.mQueryService.Open(this.mSPSite.ContentDatabase.DatabaseConnectionString);
                //if (WrapperConfiguration.IsMonitorEnable && mQueryService.Command != null)
                //{
                //    AveQueryMonitor.RegisterConnection(this.mQueryService);
                //}
            }
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        private void CheckSiteAvailable(IAveSite aveSite, string url)
        {
            CheckSiteUrlAvailable(aveSite, url);
            CheckSiteLock(aveSite);
        }

        private void CheckSiteUrlAvailable(IAveSite aveSite, string url)
        {
            if (aveSite == null
                || !aveSite.Url.Equals(url, StringComparison.OrdinalIgnoreCase))
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Backup_NotFindSiteCollection, url);
            }
        }

        public AveSPSite(IAveSite site, string databaseConnectionString, IAveBackupStream _stream, AveObjectModelFactory factory)
        {
            mSPSite = site;
            CheckSiteLock(site);
            //AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto);
            mQueryService = factory.CreateQueryService<IAveBackupRestoreQueryService>(databaseConnectionString);
            //mQueryService.SetIsolationLevel(System.Data.IsolationLevel.ReadUncommitted);
            //if (WrapperConfiguration.IsMonitorEnable && mQueryService != null && mQueryService.Command != null)
            //{
            //    AveQueryMonitor.RegisterConnection(this.mQueryService);
            //}
            ObjectModelFactory = factory;
            mSender = _stream;
            DataCache = new AveSiteDataCache(this);
            //AveEnvironment.SiteUrl = site.Url;
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        internal AveSPSite(IAveSite sourceSite)
        {
            // TODO: Complete member initialization

            if (sourceSite == null)
            {
                throw new ArgumentNullException("sourceSite");
            }

            this.mSPSite = sourceSite;

            CheckSiteLock(mSPSite);

            ObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(sourceSite.Url, mSPSite.UserAccountInfo,
                                                                                mSPSite.SPMode == WrapperSPMode.Server
                                                                                    ? AveContextKind.ServerObjectModel
                                                                                    : AveContextKind.ClientObjectModel);
            ObjectModelFactory.CreateSite(sourceSite.Url);
            if (mSPSite.SPMode == WrapperSPMode.Server)
            {
                mQueryService = ObjectModelFactory.CreateQueryService<IAveBackupRestoreQueryService>(mSPSite);
            }
            DataCache = new AveSiteDataCache(this);
            log.Debug("initializing backup for site:{0}", mSPSite.Url);
        }

        private void CheckSiteLock(IAveSite site)
        {
            try
            {
                if (site.IsReadLocked)
                {
                    // var tmp = site.RootWeb.WorkflowTemplates;//throw exception even use spsecurity.
                    Exception warn = new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Backup_BlockSite);//can not throw exception when use spsecurity.
                    throw warn;
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBCheckSiteLockError, site.ID, ex.ToString());
                throw;
            }
        }


        public string ServerRelativeUrl
        {
            get { return string.Empty; }
        }

        public AveMappingManager MappingManager
        {
            get
            {
                if (mAveMappingManager.BackupMappingManager.WebPartTypeIDMapping == null)
                {
                    mAveMappingManager.BackupMappingManager.WebPartTypeIDMapping = AveSiteMappingManager.GetWebPartIDMapping(mSPSite);
                }
                return mAveMappingManager;
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
        /// PR不能在backup时进行sitecollection的reload
        /// </summary>
        public void ReloadSite()
        {
            try
            {
                mSPSite.ReloadSite();
                //InitializeMembers();
                mSPRequestTimeout = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("Reload site failed. Site name:{0}\n error message:{1}", mSPSite == null ? "" : mSPSite.Url, e));
            }
        }

        private void InitializeMembers()
        {
            if (mQueryService != null && (ObjectModelFactory.ContextKind.IsServerMode()))
            {
                mQueryService = ObjectModelFactory.CreateQueryService<IAveBackupRestoreQueryService>(mSPSite);
            }
        }

        public bool IsMySite
        {
            get
            {
                if (!isMySite.HasValue)
                {
                    try
                    {
                        using (IAveWeb web = mSPSite.OpenWeb())
                        {
                            if (web.Exists)
                            {
                                isMySite = web.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while get web template,url:{0},Error:{1}", mSPSite.Url, ex);
                    }
                }
                return isMySite.Value;
            }
        }

        //Nullable<bool> mEnableDocAveRBS;
        //public bool EnableDocAveRBS(byte[] rbsId)
        //{
        //    //get
        //    //{
        //    //    if (!mEnableDocAveRBS.HasValue && this.SPSite.ContentDatabase != null)
        //    //    {
        //    //        mEnableDocAveRBS = string.Equals(this.SPSite.ContentDatabase.RemoteBlobStorageSettings.ActiveProviderName, AveRBSCommon.RBS_PROVIDER_NAME, StringComparison.OrdinalIgnoreCase);
        //    //    }
        //    //    return mEnableDocAveRBS.HasValue ? mEnableDocAveRBS.Value : false;
        //    //}
        //    return this.RBSBackup.BackupRBSStub(rbsId) != null ? true : false;
        //}

        public void Dispose()
        {
            if (mSPSite != null)
            {
                mSPSite.Dispose();
            }
            if (mQueryService != null)
            {
                mQueryService.Dispose();
            }
            LS.SPWorkflowProcessor.SPWorkflowProcessorRuntime.CloseNinexDBConnection();
            //TODO: implement this if necessary.
        }

        #region IAveSPSite members

        public IAveSite SPSite
        {
            get { return mSPSite; }
        }

        public AveObjectModelFactory ObjectModelFactory { get; private set; }

        public void SetLanguageMappingProcesser(AveLanguageProcesser processer)
        {
            this.mLanguageProcessor = processer;
        }

        public AveUserInfo GetUserInfo(int userId)
        {
            if (!this.DataCache.UserCache.Contains(userId))
            {
                return null;
            }
            return this.DataCache.UserCache.GetUserInfo(userId);
        }

        public object GetPrincipalInfo(int principalId)
        {
            return this.DataCache.GetPrincipalInfo(principalId);
        }

        public string GetScopeUrlByScopeId(Guid scopeId)
        {
            return QueryService.GetScopeUrl(SPSite.ID, scopeId);
        }

        public int GetCheckOutUserId(AveBaseItemInfo itemInfo)
        {
            return this.QueryService.GetCheckOutUserId(itemInfo);
        }

        public Guid GetLookupItemIdAndGuid(Guid lookupWebId, Guid lookupListId, int rowId)
        {
            Guid itemTPGuid = Guid.Empty;
            try
            {
                lock (lookupListItemIdAndGuidCache)
                {
                    Dictionary<Guid, Dictionary<int, Guid>> listLevelItemCache;
                    Dictionary<int, Guid> itemLevelItemCache;
                    if (!lookupListItemIdAndGuidCache.TryGetValue(lookupWebId, out listLevelItemCache)
                        || !listLevelItemCache.TryGetValue(lookupListId, out itemLevelItemCache))
                    {
                        itemLevelItemCache = new Dictionary<int, Guid>();
                        using (IAveWeb web = this.SPSite.OpenWeb(lookupWebId))
                        {
                            var list = web.GetList(lookupListId);
                            foreach (var item in list.Items)
                            {
                                itemLevelItemCache[item.ID] = item.GetTPGuid();
                            }
                            foreach (var folder in list.Folders)
                            {
                                itemLevelItemCache[folder.ID] = folder.GetTPGuid();
                            }
                            if (listLevelItemCache == null)
                            {
                                listLevelItemCache = new Dictionary<Guid, Dictionary<int, Guid>>();
                            }
                            listLevelItemCache[lookupListId] = itemLevelItemCache;
                            lookupListItemIdAndGuidCache[lookupWebId] = listLevelItemCache;
                        }
                    }
                    itemLevelItemCache.TryGetValue(rowId, out itemTPGuid);
                }
            }
            catch (Exception e)
            {
                log.Warn("Can not cache the Lookup ListItem Guid. WebId: {0}, ListId: {1}, Error Message: {2}", lookupWebId, lookupListId, e);
            }
            return itemTPGuid;
        }

        #region Export Methods
        public void ExportBaseInfo(IAveBackupStream output)
        {
            ExportBaseInfo(output, null);
        }

        /// <summary>
        /// PR Item is virtual site
        /// </summary>
        public void ExportBaseInfo(IAveBackupStream output, string url, string webappUrl, bool isHostHeader, string webTemplate = null)
        {
            var siteInfo = new AveSPSiteInfo(this);
            var result = siteInfo.GetSiteInfo();
            result.Url = url;
            result.WebAppUrl = webappUrl;
            result.IsHostheader = isHostHeader;
            if (webTemplate != null)
            {
                result.WebTemplate = webTemplate;
            }
            output.WriteMetadata(AveMetadataType.SiteBasicInfo, result);
        }

        public void ExportFeatures(IAveBackupStream output)
        {
            var featureManager = AveSPFeature.CreateInstance(this);
            featureManager.Export(output);
        }

        public void ExportSettings(IAveBackupStream output)
        {
            var aveSPSiteSettingInfo = new AveSPSiteSettingInfo(this);
            aveSPSiteSettingInfo.Export(output);
        }

        public void ExportSearchInfo(IAveBackupStream output)
        {
            if (this.SPContextKind.IsServerMode())
            {
                if (AveEnv.IsMoss && AveEnv.IsPublishing)
                {
                    var aveSPSiteSearch = new AveSPSearch(this);
                    aveSPSiteSearch.Export(output);
                }
            }
        }

        public void ExportLanguageInfo(IAveBackupStream output)
        {
            if (this.SPContextKind.IsServerMode() && this.LanguageProcessor != null)
            {
                var languageResFile = AveLanguage.CreateInstance(this);
                languageResFile.Export(output);
            }
            else
            {
                //ADO-61291 
                output.WriteMetadata(AveMetadataType.LanguageFile, new AveLanguageInfo() { LanguageLCD = this.SPSite.RootWeb.Language });//client虽不用加载资源文件，但是在后面需用到AveMetadataType.LanguageFile，进行LoadXML
            }
        }

        public void ExportUsers(IAveBackupStream output, bool includeUsersWithoutSecurity)
        {
            var option = includeUsersWithoutSecurity ?
                new AveUserBackupOption() { UserQueryOption = AveSiteUsersQueryOption.AllUsers } :
                new AveUserBackupOption() { UserQueryOption = AveSiteUsersQueryOption.OnlyHaveSecurityUsers };
            ExportUsers(output,option);
        }

        public void ExportGroups(IAveBackupStream output, bool includeGroupsWithoutSecurity)
        {
            var groups = AveGroup.CreateInstatnce(this);
            groups.Export(output, includeGroupsWithoutSecurity);
        }

        public void ExportUserProfiles(IAveBackupStream output, bool allUsers)
        {
            if ((this.SPContextKind.IsServerMode() && AveEnv.IsMoss) || this.SPContextKind == AveContextKind.ClientObjectModel)
            {
                    var users = AveUser.CreateInstance(this);
                    if (allUsers)
                    {
                        var userProfile = new AveSPUserProfile(this, users.GetUsers(new AveUserBackupOption() { UserQueryOption = AveSiteUsersQueryOption.OnlyHaveSecurityUsers }));
                        userProfile.Export(output);
                    }
                    else if (this.IsMySite)
                    {
                        var userProfile = new AveSPUserProfile(this, this.SPSite.Owner.LoginName);
                        userProfile.Export(output);
                    }
            }
        }

        public void ExportAudience(IAveBackupStream output)
        {
            if (this.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var audienceManager = new AveAudienceManager(this);
                    audienceManager.Export(output);
                }
            }
        }

        public void ExportManagedMetadata(IAveBackupStream output, bool includeGlobalTermGroup = true, bool enableCache = false)
        {
            if ((this.SPContextKind.IsServerMode10Upper() && AveEnv.IsMoss) || ((this.SPContextKind == AveContextKind.ClientObjectModel) && string.Compare(this.SPSite.SPVersion,"15.",StringComparison.OrdinalIgnoreCase) > 0))
            {
                //if (AveEnv.IsMoss)
                //{
                var metadataService = new AveMetadataService(this.SPSite);
                metadataService.SkipGlobalTermGroup = !includeGlobalTermGroup;
                metadataService.Export(output, enableCache);
                //}
            }
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.ExportFullTextIndex"))
            {
                var index = new FullTextIndex()
                {
                    TimeZoneInfoID = AveTimeZoneUtility.ToTimeZoneInfoId(SPSite.RootWeb.RegionalSettings.TimeZone.ID),
                };
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }

        public void ExportVariationSetting(IAveBackupStream output)
        {
            if (this.SPContextKind.IsServerMode13Upper())
            {
                var variationSetting = new AveSPVariationSetting(this);
                variationSetting.Export(output);
            }
        }

        public void ExportSEOSetting(IAveBackupStream output)
        {
            if (this.SPContextKind.IsServerMode13Upper())
            {
                var seoInfo = new AveSPSEOSettings(this);
                seoInfo.Export(output);
            }
        }
        #endregion

        public AveFeatureInfoBox GetFeatures()
        {
            var featureManager = AveSPFeature.CreateInstance(this);
            return featureManager.GetFeatures();
        }

        public List<AveUserInfo> GetUsers(bool includeUsersWithoutSecurity = true)
        {
            var option = includeUsersWithoutSecurity ?
              new AveUserBackupOption() { UserQueryOption = AveSiteUsersQueryOption.AllUsers } :
              new AveUserBackupOption() { UserQueryOption = AveSiteUsersQueryOption.OnlyHaveSecurityUsers };
            return GetUsers(option);
        }

        public List<AveUserInfo> GetUsers(AveUserBackupOption option)
        {
            var users = AveUser.CreateInstance(this);
            return users.GetUsers(option);
        }

        public List<AveGroupInfo> GetGroupsWithAllMembers(bool includeUsersWithoutSecurity = true)
        {
            var groups = AveGroup.CreateInstatnce(this);
            return groups.GetGroupsWithAllMembers(includeUsersWithoutSecurity);
        }
        #endregion



        public void ExportBaseInfo(IAveBackupStream stream, SetSiteBaseInfoAction setSiteBaseInfo)
        {
            var siteInfo = new AveSPSiteInfo(this);
            var result = siteInfo.GetSiteInfo();
            if (setSiteBaseInfo != null)
            {
                setSiteBaseInfo(result);
            }
            stream.WriteMetadata(AveMetadataType.SiteBasicInfo, result);
        }

        public void ExportManagedMetadata(IAveBackupStream output, SPSiteManagedMetadataBackupOption backupOption)
        {
            var metadataService = new AveMetadataService(this.SPSite);
            metadataService.SkipGlobalTermGroup = !backupOption.IncludeGlobalTermGroup;
            metadataService.Export(output, backupOption.EnableCache);
        }


        public void ExportGroups(IAveBackupStream output)
        {
            ExportGroups(output, true);
        }

        public void ExportUsers(IAveBackupStream output)
        {
            ExportUsers(output, true);
        }

        public void ExportUsers(IAveBackupStream output, AveUserBackupOption option)
        {
            var users = AveUser.CreateInstance(this);
            users.Export(output, option);
        }

        public void ExportUserCustomActions(IAveBackupStream output)
        {
            AveSPUserCustomActionCollection spUserCustomActionCollection = new AveSPSiteUserCustomActionCollection(this);
            output.WriteMetadata(AveMetadataType.SiteUserCustomAction, spUserCustomActionCollection.GetUserCustomActionInfos());
        }
        public void SetBackupOption(AveBackupOption option)
        {
            BackupOption = option;
            WrapperRuntime.CurrentContext.BackupContentTypeDocumentTemplateFile = BackupOption.BackupContentTypeDocumentTemplateFile;
            WrapperRuntime.CurrentContext.BackupWebpartPropertiesForOffice365 = BackupOption.BackupWebpartPropertiesForOffice365;

        }

    }
}
