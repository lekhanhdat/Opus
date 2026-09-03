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
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSite : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IAveBackupRestoreQueryService mQueryService;
        private IAveBackupStream mSender;
        private AveLanguageProcesser mLanguageProcessor;
        private IAveSite mSPSite = null;
        private bool mUserProfileApplicationAvailable = true;
        private AveBPOSAccountInfo mAccount = null;
        private List<AveUserInfo> mSiteUserInfoCache = null;
        private List<Guid> mScopeIdsProcessed = new List<Guid>();
        private AveContextKind mSPContextKind = AveContextKind.ServerObjectModel;
        private IAveServiceContext mServiceContext;

        private Dictionary<long, string> mUserProfiles = new Dictionary<long, string>();
        private AveMappingManager mAveMappingManager = new AveMappingManager();
        private AveRBSBackup mRBSBackup;
        private List<AveGroupInfo> mAllGroups;
        private bool? isMySite;

        public AveSiteDataCache DataCache
        {
            get;
            set;
        }

        //add for RevIM export
        internal AveUserInfo GetUserInfo(int userId)
        {
            if (!this.DataCache.UserCache.Contains(userId))
            {
                return null;
            }
            return this.DataCache.UserCache.GetUserInfo(userId);
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

        //internal IAveOSocialTagManager TagManager
        //{
        //    get
        //    {
        //        if (mTagManager == null)
        //        {
        //            mTagManager = this.ObjectModelFactory.CreateSocialTagManager(this.ServiceContext);
        //        }
        //        return mTagManager;
        //    }
        //}

        //internal IAveOSocialCommentManager CommentManager
        //{
        //    get
        //    {
        //        if (mCommentManager == null)
        //        {
        //            mCommentManager = this.ObjectModelFactory.CreateSocialCommentManager(this.ServiceContext); ;
        //        }
        //        return mCommentManager;
        //    }
        //}

        public List<AveGroupInfo> AllGroups
        {
            get
            {
                if (mAllGroups == null)
                {
                    mAllGroups = mSPSite.RootWeb.GroupsSerializer.GetObjectData(true) as List<AveGroupInfo>;
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
        }

        public IAveBackupStream Sender
        {
            get { return mSender; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public IAveSite SPSite
        {
            get { return mSPSite; }
        }

        public List<AveUserInfo> SiteUserInfoCache
        {
            get { return mSiteUserInfoCache; }
            set { mSiteUserInfoCache = value; }
        }

        public List<Guid> ScopeIdsProcessed
        {
            get { return mScopeIdsProcessed; }
            set { mScopeIdsProcessed = value; }
        }

        public AveContextKind SPContextKind
        {
            get { return mSPContextKind; }
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

        public AveObjectModelFactory ObjectModelFactory { get; set; }

        public AveSPSite(string _url, AveContextKind contextKind, AveBPOSAccountInfo userAccountInfo, IAveBackupStream _stream)
        {
            this.mAccount = userAccountInfo;
            this.mSender = _stream;
            _url = _url.TrimEnd('/');

            //AveEnvironment.SiteUrl = _url;
            AveObjectModelFactory siteFactory = AveObjectModelFactory.CreateObjectModelFactory(_url, mAccount, contextKind);//mSPContextKind);
            this.mSPSite = siteFactory.CreateSite(_url);
            CheckSiteAvailable(this.mSPSite, _url);
            this.mSPContextKind = siteFactory.ContextKind;
            ObjectModelFactory = siteFactory;
            DataCache = new AveSiteDataCache(this);

            if (siteFactory.ContextKind == AveContextKind.ServerObjectModel || siteFactory.ContextKind == AveContextKind.Server07ObjectModel)
            {
                this.mQueryService = siteFactory.CreateQueryService<IAveBackupRestoreQueryService>(this.mSPSite);
            }
            //log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
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
                throw new AveException("Site collection does not exist or can not be accessed.");
            }
        }

        public AveSPSite(IAveSite site, string _url, AveContextKind contextKind, AveBPOSAccountInfo userAccountInfo, IAveBackupStream _stream)
        {
            this.mAccount = userAccountInfo;
            this.mSender = _stream;
            _url = _url.TrimEnd('/');
            AveObjectModelFactory siteFactory = AveObjectModelFactory.CreateObjectModelFactory(_url, mAccount, contextKind);
            this.mSPContextKind = siteFactory.ContextKind;
            ObjectModelFactory = siteFactory;

            mSPSite = site;
            CheckSiteAvailable(site, _url);
            DataCache = new AveSiteDataCache(this);
            //AveEnvironment.SiteUrl = site.Url;
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        private void CheckSiteLock(IAveSite site)
        {
                try
                {
                    if (site.ReadLocked)
                    {
                        var tmp = site.RootWeb.WorkflowTemplates;//throw exception even use spsecurity.
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBCheckSiteLockError, site.ID, ex.ToString());
                    throw;
                }
        }

        public void Dispose()
        {
            if (mSPSite != null)
            {
                mSPSite.Dispose();
            }
            //TODO: implement this if necessary.
        }

        public void SetLanguageMappingProcesser(AveLanguageProcesser LanguageProcessor)
        {
            this.mLanguageProcessor = LanguageProcessor;
        }

        public string ServerRelativeUrl
        {
            get { return string.Empty; }
        }

        public AveMappingManager MappingManager
        {
            get
            {
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

        //此方法无引用
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
                    if (isMySite.HasValue)
                    {
                        return isMySite.Value;
                    }
                    else
                    {
                        throw new NullReferenceException("IsMySite");
                    }
                }
                else
                {
                    return isMySite.Value;
                }
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

        public string GetScopeUrlByScopeId(Guid scopeId)
        {
            return QueryService.GetScopeUrl(SPSite.ID,scopeId);
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

        public AveFeatureInfoBox GetFeatures()
        {
            var featureManager = AveSPFeature.CreateInstance(this);
            return featureManager.GetFeatures();
        }

        public void ExportUserCustomActions(IAveBackupStream output)
        {
            AveSPUserCustomActionCollection spUserCustomActionCollection = new AveSPSiteUserCustomActionCollection(this);
            output.WriteMetadata(AveMetadataType.SiteUserCustomAction, spUserCustomActionCollection.GetUserCustomActionInfos());
        }

        #region add for RevIM
        public void ExportUsers(IAveBackupStream output)
        {
            var users = AveUser.CreateInstance(this);
            users.Export(output, true);
        }

        public void ExportGroups(IAveBackupStream output)
        {
            var groups = AveGroup.CreateInstatnce(this);
            groups.Export(output, true);
        }

        public List<AveUserInfo> GetUsers()
        {
            var users = AveUser.CreateInstance(this);
            var result = users.GetUsers(true);
            if (result == null)
            {
                return new List<AveUserInfo>();
            }
            else
            {
                return result;
            }
        }

        public List<AveGroupInfo> GetGroups()
        {
            var groups = AveGroup.CreateInstatnce(this);
            var result = groups.GetGroups();
            if (result == null)
            {
                return new List<AveGroupInfo>();
            }
            else
            {
                return result;
            }
        }

        public List<AveGroupInfo> GetAllGroups()
        {
            var groups = AveGroup.CreateInstatnce(this);
            var result = groups.GetGroups(true);
            if (result == null)
            {
                return new List<AveGroupInfo>();
            }
            else
            {
                return result;
            }
        }
        #endregion
    }
}
