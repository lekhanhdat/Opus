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
using System.Threading;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.ObjectModel.Server19.Office;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.QueryService;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Utilities;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Linq;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server19
{
    public class AveSite : IAveSite
    {
        internal delegate SPUser CheckoutUserHandler(SPWeb web, SPUser user, Guid fileId);
        internal CheckoutUserHandler checkoutUserHandler = new CheckoutUserHandler(
                            (web, user, fileId) => { throw new AveUnauthorizedAccessException("Failed to get checkout file because of permission.", user.LoginName); }
                            //(web, user, fileId) => { return web.CurrentUser; }
                        );

        private SPSite mSite;
        private SPSite mCheckoutSite;
        private SPWeb mCheckoutWeb;
        private const SPBasePermissions mCheckoutUserPermission = SPBasePermissions.OpenItems | SPBasePermissions.EditListItems | SPBasePermissions.AddListItems | SPBasePermissions.DeleteListItems;
        private AveWebApplication mWebAppilcation;
        private AveWebCollection mAllWebs;
        private AveWeb mRootWeb;
        private AveContentDatabase mContentDatabase;
        private AveFeatureCollection mFeatureCollection;
        private AveUser mOwner;
        private AveAudit mAudit;
        private AveUserSolutionCollection mSolutions;
        private AveRecycleBinItemCollection mRecycleBin;
        private AveUser mSecondaryContact;
        private AveWorkflowManager mWorkflowManager;
        private AveUsageInfo mUsageInfo;
        private IAveBackupRestoreQueryService13 mQueryService;
        //private AveDBQueryService mDBService = new AveDBQueryService();
        private bool mConnInitialized = false;
        private static Object synRoot = new Object();
        private AveFeatureDefinitionCollection mFeatureDefinitionCollection;
        private Guid mCheckOutFileId = Guid.Empty;
        private int mCheckOutUser = -1;
        private AveSiteSerializer m_SiteSerializer;
        private AveSiteSettingSerializer m_SiteSettingSerializer;
        private AveMetaDataServiceSerializer m_MetaDataServiceSerializer;
        private AveUserSerializer m_UserSerializer;
        private AveGroupSerializer m_GroupSerializer;
        private AveSiteUsersSerializer m_SiteUsersSerializer;
        private AveFeatureSerializer m_FeatureSerializer;
        private AveUserToken mUserToken;
        private AveUser mSystemAccount;
        private AveQuota mQuota;
        private Dictionary<Guid, IAveTerm> mTermIdCache = new Dictionary<Guid, IAveTerm>();
        private AveEventReceiverDefinitionCollection mEventReceivers;

        //To fix ADO-60795
        private List<string> mSafeDomains;
        private WrapperNativeApiPermission nativeApiPermission = WrapperNativeApiPermission.None;

        public Guid CheckOutFileId
        {
            set { mCheckOutFileId = value; }
            get { return mCheckOutFileId; }
        }

        public Dictionary<Guid, IAveTerm> TermIdCache
        {
            get { return mTermIdCache; }
            set { mTermIdCache = value; }
        }

        public bool DenyAddAndCustomizePagesStatus
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public int CheckOutUser
        {
            set { mCheckOutUser = value; }
            get { return mCheckOutUser; }
        }

        //public AveDBQueryService DBService
        //{
        //    get
        //    {
        //        if (!mConnInitialized)
        //        {
        //            lock (synRoot)
        //            {
        //                if (!mConnInitialized) //Double-Checked Locking
        //                {
        //                    InitializeDBService(mSite.ContentDatabase.DatabaseConnectionString);
        //                    mConnInitialized = true;
        //                }
        //            }
        //        }
        //        return mDBService;
        //    }
        //}

        internal IAveBackupRestoreQueryService13 QueryService
        {
            get
            {
                if (!mConnInitialized)
                {
                    lock (synRoot)
                    {
                        if (!mConnInitialized) //Double-Checked Locking
                        {
                            mQueryService = AveQueryServiceProvider.Instance<IAveBackupRestoreQueryService13>(mSite.ContentDatabase.DatabaseConnectionString);
                            mConnInitialized = true;
                        }
                    }
                }
                return mQueryService;
            }
        }

        private static AveLogger log = AveLogger.GetInstance(typeof(AveSite));

        /// <summary>
        /// add for using static method
        /// </summary>
        public AveSite()
        { }

        public AveSite(string url, AveBPOSAccountInfo userAccountInfo)
        {
            mSite = new SPSite(url);
            this.LastReloadTimeUTC = DateTime.UtcNow;
            log.Debug("Site: {0}, Current user{1}\\{2}", url, Environment.UserDomainName, Environment.UserName);
        }

        public AveSite(string url, IAveUserToken token)
        {
            mSite = new SPSite(url, (token as AveUserToken).UserToken);
            this.LastReloadTimeUTC = DateTime.UtcNow;
            log.Debug("Site: {0}, Current user{1}\\{2}", url, Environment.UserDomainName, Environment.UserName);
        }

        public AveSite(Guid siteID, IAveUserToken token)
        {
            mSite = new SPSite(siteID, (token as AveUserToken).UserToken);
            this.LastReloadTimeUTC = DateTime.UtcNow;
            log.Debug("Site: {0}, Current user{1}\\{2}", mSite.Url, Environment.UserDomainName, Environment.UserName);
        }

        public AveSite(SPSite site)
        {
            mSite = site;
            this.LastReloadTimeUTC = DateTime.UtcNow;
            log.Debug("Site: {0}, Current user{1}\\{2}", site.Url, Environment.UserDomainName, Environment.UserName);
        }

        public AveSite(string url)
        {
            mSite = new SPSite(url);
            this.LastReloadTimeUTC = DateTime.UtcNow;
        }

        public AveSite(Guid id)
        {
            mSite = new SPSite(id);
            this.LastReloadTimeUTC = DateTime.UtcNow;
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        public AveSite(Guid id, AveUrlZone zone)
        {
            mSite = new SPSite(id, (SPUrlZone)zone);
            this.LastReloadTimeUTC = DateTime.UtcNow;
            log.Debug("Current user{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        public WrapperSPMode SPMode
        {
            get { return WrapperSPMode.Server; }
        }

        public AveBPOSAccountInfo UserAccountInfo
        {
            get { return null; }
        }
        
        #region IAveSite Members

        public IAveTaxonomySession AveSPTaxonomySession
        {
            get
            {
                return new AveTaxonomySession(this);
            }
        }

        public IAveWeb OpenWeb(Guid webId)
        {
            SPWeb web = mSite.OpenWeb(webId);
            return new AveWeb(this, web);
        }

        internal SPSite Site
        {
            get { return mSite; }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        internal string DatabaseConnectionString
        {
            get { return mSite.ContentDatabase.DatabaseConnectionString; }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (mEventReceivers == null)
                {
                    mEventReceivers = new AveEventReceiverDefinitionCollection(mSite.EventReceivers);
                }
                return mEventReceivers;
            }
        }

        public IAveWeb OpenWeb(string webUrl)
        {
            return new AveWeb(this, mSite.OpenWeb(webUrl));
        }

        public IAveWeb OpenWeb()
        {
            return new AveWeb(this, mSite.OpenWeb());
        }

        public bool IISAllowsAnonymous
        {
            get
            {
                return mSite.IISAllowsAnonymous;
            }
        }

        public string Url
        {
            get { return mSite.Url; }
        }

        public string ServerRelativeUrl
        {
            get { return mSite.ServerRelativeUrl; }
        }

        public IAveWeb RootWeb
        {
            get
            {
                if (mRootWeb == null)
                {
                    mRootWeb = new AveWeb(this, mSite.RootWeb);
                }
                return mRootWeb;
            }
        }

        public IAveFeatureCollection Features
        {
            get
            {
                if (mFeatureCollection == null)
                {
                    mFeatureCollection = new AveFeatureCollection(mSite.Features, this);
                }
                return mFeatureCollection;
            }
        }

        public IAveWebApplication WebApplication
        {
            get
            {
                if (mWebAppilcation == null)
                {
                    SPWebApplication webApplication = mSite.WebApplication;
                    if (webApplication != null)
                    {
                        mWebAppilcation = new AveWebApplication(webApplication);
                    }
                }
                return mWebAppilcation;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", Justification = "The reason about add this supress message")]
        public IAveWebCollection AllWebs
        {
            get
            {
                if (mAllWebs == null)
                {
                    mAllWebs = new AveWebCollection(this, mSite.AllWebs);
                }
                return mAllWebs;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public IAveContentDatabase ContentDatabase
        {
            get
            {
                if (mContentDatabase == null)
                {
                    SPContentDatabase contentDatabase = mSite.ContentDatabase;
                    if (contentDatabase != null)
                    {
                        mContentDatabase = new AveContentDatabase(contentDatabase);
                    }
                }
                return mContentDatabase;
            }
        }

        public void Close()
        {
            mSite.Close();
        }

        public void Delete()
        {
            mSite.Delete();
        }

        public IAveWebTemplateCollection GetWebTemplates(uint licd)
        {
            return new AveWebTemplateCollection(mSite.GetWebTemplates(licd));
        }

        public IAveWebTemplateCollection GetWebTemplates(uint licd, int overrideCompatLevel)
        {
            return new AveWebTemplateCollection(mSite.GetWebTemplates(licd, overrideCompatLevel));
        }

        public void Update()
        { }

        public void UpdateSpecialProperty()
        {
            throw new NotImplementedException();
        }

        public bool AllowDesigner
        {
            get
            {
                return mSite.AllowDesigner;
            }
            set
            {
                mSite.AllowDesigner = value;
            }
        }

        public bool AllowMasterPageEditing
        {
            get
            {
                return mSite.AllowMasterPageEditing;
            }
            set
            {
                mSite.AllowMasterPageEditing = value;
            }
        }

        public bool AllowRevertFromTemplate
        {
            get
            {
                return mSite.AllowRevertFromTemplate;
            }
            set
            {
                mSite.AllowRevertFromTemplate = value;
            }
        }

        public IAveAudit Audit
        {
            get
            {
                if (mAudit == null)
                {
                    mAudit = new AveAudit(mSite.Audit);
                }
                return mAudit;
            }
        }

        public string AuditLogTrimmingCallout
        {
            get
            {
                return mSite.AuditLogTrimmingCallout;
            }
            set
            {
                mSite.AuditLogTrimmingCallout = value;
            }
        }

        public int AuditLogTrimmingRetention
        {
            get
            {
                return mSite.AuditLogTrimmingRetention;
            }
            set
            {
                mSite.AuditLogTrimmingRetention = value;
            }
        }

        public bool HostHeaderIsSiteName
        {
            get { return mSite.HostHeaderIsSiteName; }
        }

        public IAveUser Owner
        {
            get
            {
                if (mOwner == null)
                {
                    mOwner = new AveUser(this.RootWeb as AveWeb, mSite.Owner);
                }
                return mOwner;
            }
            set
            {
                mOwner = value as AveUser;
                if (mOwner != null)
                {
                    mSite.Owner = mOwner.User;
                }
                else
                {
                    mSite.Owner = null;
                }
            }
        }

        public string PortalName
        {
            get
            {
                return mSite.PortalName;
            }
            set
            {
                mSite.PortalName = value;
            }
        }

        public string PortalUrl
        {
            get
            {
                return mSite.PortalUrl;
            }
            set
            {
                mSite.PortalUrl = value;
            }
        }

        public bool ShowURLStructure
        {
            get
            {
                return mSite.ShowURLStructure;
            }
            set
            {
                mSite.ShowURLStructure = value;
            }
        }

        public IAveUserSolutionCollection Solutions
        {
            get
            {
                if (mSolutions == null)
                {
                    mSolutions = new AveUserSolutionCollection(mSite.Solutions);
                }
                return mSolutions;
            }
        }

        public IAveRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (mRecycleBin == null)
                {
                    mRecycleBin = new AveRecycleBinItemCollection(this, mSite.RecycleBin);
                }
                return mRecycleBin;
            }
        }

        public bool SyndicationEnabled
        {
            get
            {
                return mSite.SyndicationEnabled;
            }
            set
            {
                mSite.SyndicationEnabled = value;
            }
        }

        public bool TrimAuditLog
        {
            get
            {
                return mSite.TrimAuditLog;
            }
            set
            {
                mSite.TrimAuditLog = value;
            }
        }

        public bool UIVersionConfigurationEnabled
        {
            get
            {
                return mSite.UIVersionConfigurationEnabled;
            }
            set
            {
                mSite.UIVersionConfigurationEnabled = value;
            }
        }

        public IAveUser SecondaryContact
        {
            get
            {
                if (mSecondaryContact == null)
                {
                    SPUser user = mSite.SecondaryContact;
                    if (user != null)
                    {
                        mSecondaryContact = new AveUser(this.RootWeb as AveWeb, user);
                    }
                }
                return mSecondaryContact;
            }
            set
            {
                mSecondaryContact = value as AveUser;
                if (mSecondaryContact != null)
                {
                    mSite.SecondaryContact = mSecondaryContact.User;
                }
                else
                {
                    mSite.SecondaryContact = null;
                }
            }
        }

        public bool AllowRssFeeds
        {
            get { return mSite.AllowRssFeeds; }
        }

        public IAveWorkflowManager WorkflowManager
        {
            get
            {
                if (mWorkflowManager == null)
                {
                    mWorkflowManager = new AveWorkflowManager(mSite.WorkflowManager);
                }
                return mWorkflowManager;
            }
        }

        public bool ReadLocked
        {
            get
            {
                return mSite.ReadLocked;
            }
            set
            {
                mSite.ReadLocked = value;
            }
        }

        public bool IsReadLocked
        {
            get
            {
                return mSite.IsReadLocked;
            }
        }

        public bool WriteLocked
        {
            get
            {
                return mSite.WriteLocked;
            }
            set
            {
                mSite.WriteLocked = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return mSite.ReadOnly;
            }
            set
            {
                mSite.ReadOnly = value;
            }
        }

        public string LockIssue
        {
            get
            {
                return mSite.LockIssue;
            }
            set
            {
                mSite.LockIssue = value;
            }
        }

        public IAveQuota Quota
        {
            get
            {
                if (mQuota == null)
                {
                    mQuota = new AveQuota(mSite.Quota);
                }
                return mQuota;
            }
            set
            {
                mQuota = value as AveQuota;
                if (mQuota != null)
                {
                    mSite.Quota = mQuota.Quota;
                }
                else
                {
                    mSite.Quota = null;
                }
            }
        }

        public AveUrlZone Zone
        {
            get { return (AveUrlZone)mSite.Zone; }
        }

        public IAveListTemplateCollection GetCustomListTemplates(IAveWeb web)
        {
            return new AveListTemplateCollection(web as AveWeb, mSite.GetCustomListTemplates((web as AveWeb).Web));
        }

        public bool InvalidateCacheEntry(Uri uri, Guid siteId)
        {
            return SPSite.InvalidateCacheEntry(uri, siteId);
        }

        public double AverageResourceUsage
        {
            get
            {
                return mSite.AverageResourceUsage;
            }
        }

        public double CurrentResourceUsage
        {
            get
            {
                return mSite.CurrentResourceUsage;
            }
        }

        public AveUsageInfo Usage
        {
            get
            {
                if (mUsageInfo.Storage == 0L)
                {
                    mUsageInfo = new AveUsageInfo();
                    mUsageInfo.Bandwidth = mSite.Usage.Bandwidth;
                    mUsageInfo.DiscussionStorage = mSite.Usage.DiscussionStorage;
                    mUsageInfo.Hits = mSite.Usage.Hits;
                    mUsageInfo.Storage = mSite.Usage.Storage;
                    mUsageInfo.Visits = mSite.Usage.Visits;
                }
                return mUsageInfo;
            }
        }

        public DateTime LastContentModifiedDate
        {
            get
            {
                return mSite.LastContentModifiedDate;
            }
        }

        public Guid GetWeb(IAveBackupRestoreQueryService queryService, string url)
        {
            Guid siteId = Guid.Empty;
            if (mSite != null)
            {
                siteId = mSite.ID;
            }
            return queryService.GetWebId(siteId, url);
        }

        public bool AllowUnsafeUpdates
        {
            get
            {
                return mSite.AllowUnsafeUpdates;
            }
            set
            {
                mSite.AllowUnsafeUpdates = value;
            }
        }

        public Guid ID
        {
            get
            {
                return mSite.ID;
            }
        }

        public bool BrowserDocumentsEnabled
        {
            get
            {
                return mSite.BrowserDocumentsEnabled;
            }
        }

        public bool IsSiteMaster
        {
            get { return mSite.IsSiteMaster; }
        }

        /// <summary>
        /// To backup/restore HTML Field Security. ADO-60795
        /// </summary>
        public List<string> ScriptSafeDomains
        {
            get
            {
                if (mSafeDomains == null)
                {
                    mSafeDomains = new List<string>();
                    foreach (string d in mSite.ScriptSafeDomains)
                    {
                        mSafeDomains.Add(d);
                    }
                }
                return mSafeDomains;
            }
            set
            {
                mSafeDomains = value;

                HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach(string d in mSite.ScriptSafeDomains)
                {
                    set.Add(d);
                }

                //Add new ones
                foreach (string d in mSafeDomains)
                {
                    if (set.Contains(d))
                    {
                        set.Remove(d);
                    }
                    else
                    {
                        mSite.ScriptSafeDomains.Add(d);
                    }
                }

                //Remove deleted ones
                foreach (string d in set)
                {
                    mSite.ScriptSafeDomains.Remove(d);
                }
            }
        }

        public string GetWebCTNameById(string contentTypeId)
        {
            return QueryService.GetWebCTNameById(this.ID, contentTypeId);
        }

        public void UpdateUserInfo(string listName, int userId, AveUserInfo old)
        {
            SPList userList = mSite.RootWeb.SiteUserInfoList;
            if (this.NativeApiPermission == WrapperNativeApiPermission.FullControl)
            {
                XmlDocument doc = new XmlDocument();
                string nameFieldSchema = userList.Fields.GetFieldByInternalName("Name").SchemaXml;
                string displayFieldSchema = userList.Fields.GetFieldByInternalName("Title").SchemaXml;
                string eMailFieldSchema = userList.Fields.GetFieldByInternalName("EMail").SchemaXml;
                doc.LoadXml("<Fields>" + nameFieldSchema + "</Fields>");
                XmlElement xe = (XmlElement)doc.FirstChild.ChildNodes[0];
                string nameField = xe.GetAttribute("ColName");
                doc.RemoveAll();
                doc.LoadXml("<Fields>" + displayFieldSchema + "</Fields>");
                xe = (XmlElement)doc.FirstChild.ChildNodes[0];
                string displayField = xe.GetAttribute("ColName");
                doc.RemoveAll();
                doc.LoadXml("<Fields>" + eMailFieldSchema + "</Fields>");
                xe = (XmlElement)doc.FirstChild.ChildNodes[0];
                string eMailField = xe.GetAttribute("ColName");
                doc.RemoveAll();

                QueryService.UpdateUserInfo(mSite.ID, userList.ID, userId, old, displayField, nameField, eMailField);
            }
            else
            {
                SPListItem userItem = userList.GetItemById(userId);
                //对于Place Holder产生的User Information List下的item，没有办法Keep这4个Column，所以不需要再次更新
                //DateTime modifiedTime = Convert.ToDateTime(userItem[SPBuiltInFieldId.Modified]);
                //DateTime createdTime = Convert.ToDateTime(userItem[SPBuiltInFieldId.Created]);
                //var author = userItem[SPBuiltInFieldId.Author];
                //var editor = userItem[SPBuiltInFieldId.Editor];

                userItem["Name"] = old.Login;
                userItem["Title"] = old.Title;
                userItem["EMail"] = old.Email;
                userItem.SystemUpdate(false);

                //userItem[SPBuiltInFieldId.Author] = author;
                //userItem[SPBuiltInFieldId.Editor] = editor;
                //userItem[SPBuiltInFieldId.Modified] = modifiedTime;
                //userItem[SPBuiltInFieldId.Created] = createdTime;
                //if (!AveItem.AveItemSystemUpdate(userItem, true))
                //{
                //    log.Log(AveLogLevel.WARN, "Failed to keep user item properties. User Id: {0}", userId);
                //    userItem.SystemUpdate(false);
                //}
            }
        }

        internal void DisposeConnection()
        {
            if (mConnInitialized)
            {
                mQueryService.Dispose();
                mQueryService = null;
                mConnInitialized = false;
            }
        }

        public void Dispose()
        {
            try
            {
                //RestoreCheckOutUser();
                CleanUp();
                //InternalCleanup();
                DoDispose(mQueryService);
                mQueryService = null;
                mConnInitialized = false;
                DoDispose(mRootWeb);
                mRootWeb = null;
                DoDispose(mSite);
                //mSite = null;

                AveMonitoredScope.RemoveCurrentScope();
            }
            catch (Exception ex)
            {
                log.Error("An error occurred while disposing site: {0}", ex.ToString());
            }
        }

        public void VisualUpgradeWebs()
        {
            mSite.VisualUpgradeWebs();
        }

        #region ForPerformance
        Dictionary<int, bool> mUserAvailableCache = new Dictionary<int, bool>();
        #endregion

        public bool CheckUserIfAvailable(int userId)
        {
            if (!mUserAvailableCache.ContainsKey(userId))
            {
                mUserAvailableCache.Add(userId, QueryService.CheckUserIfAvailable(mSite.ID, userId));
            }
            return mUserAvailableCache[userId];
        }

        public IAveWeb OpenWeb(string strUrl, bool requireExactUrl)
        {
            if (strUrl == null)
            {
                throw new ArgumentNullException();
            }
            return new AveWeb(this, mSite.OpenWeb(strUrl, requireExactUrl));
        }

        public IAveList GetCatalog(AveListTemplateType typeCatalog)
        {
            SPList list = mSite.GetCatalog((SPListTemplateType)typeCatalog);
            if (list != null)
            {
                AveListCollection lists = new AveListCollection(new AveWeb(this, list.ParentWeb), list.ParentWeb.Lists);
                return (lists).CreateListByType(list);
            }
            return null;
        }

        public Guid GetListId(Guid webId, string listTitle)
        {
            return this.QueryService.GetListId(this.Site.ID, webId, listTitle);
        }

        public string Protocol
        {
            get { return mSite.Protocol; }
        }

        public int Port
        {
            get
            {
                return mSite.Port;
            }
        }

        public string HostName
        {
            get { return mSite.HostName; }
        }

        public IAveUserToken UserToken
        {
            get
            {
                if (mUserToken == null)
                {
                    mUserToken = new AveUserToken(mSite.UserToken);
                }
                return mUserToken;
            }
        }

        [Obsolete("replace with GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob)")]
        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId)
        {
            return GetCheckoutWeb(siteId, web, user, fileId, false);
        }

        [Obsolete("replace with GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob)")]
        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId, bool isBackupJob)
        {
            SPWeb temp = null;
            temp = GetCheckoutWeb(siteId, (web as AveWeb).Web, null, (user as AveUser).User, fileId, isBackupJob);
            if (temp == null)
            {
                return null;
            }
            return new AveWeb(this, temp);
        }

        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob, bool throwIfNotHaveEnoughPermission = false)
        {
            var checkoutWeb = GetCheckoutWeb(siteId, (web as AveWeb).Web, list == null ? null : (list as AveList).List, (user as AveUser).User, fileId, isBackupJob, throwIfNotHaveEnoughPermission);
            if (checkoutWeb == null)
            {
                return null;
            }
            return new AveWeb(this, checkoutWeb);
        }

        public SPWeb GetCheckoutWeb(Guid siteId, SPWeb web, SPList list, SPUser user, Guid fileId, bool isBackupJob, bool throwIfNotHaveEnoughPermission = false)
        {
            SPWeb temp = null;
            if (isBackupJob)
            {
                temp = GetCheckoutWebByUser(fileId, web, user);
            }
            else
            {
                bool hasPermission = CheckPermissionForCheckoutUser(web, list, user);
                ThrowIfNotHaveEnoughPermission(throwIfNotHaveEnoughPermission, hasPermission);
                temp = GetCheckoutWebWithPermission(web, user, fileId, hasPermission);
            }
            return temp;
        }

        private static void ThrowIfNotHaveEnoughPermission(bool throwIfNotHaveEnoughPermission, bool hasPermission)
        {
            if (throwIfNotHaveEnoughPermission && !hasPermission)
            {
                //todo:wbhu,国际化
                throw new AveWrapperCheckoutFileException();
            }
        }

        //目前只给Agent Account没有Full Control的情况使用，因为接口的问题，没有替换
        //以后要替代所有的GetCheckoutWeb方法
        //需要将CheckoutUserHandler 委托公开出去
        internal SPWeb GetCheckoutWeb(SPWeb web, SPList list, ref SPUser user, Guid fileId)
        {
            bool hasPermission = CheckPermissionForCheckoutUser(web, list, user);

            if (!hasPermission)
            {
                if (this.checkoutUserHandler != null)
                {
                    user = checkoutUserHandler(web, user, fileId);
                }
                else
                {
                    throw new AveUnauthorizedAccessException("Failed to get checkout web because of permission.", user.LoginName);
                }
            }

            return GetCheckoutWebByUser(web, user);
        }

        private bool CheckPermissionForCheckoutUser(SPWeb web, SPList list, SPUser user)
        {
            if (user == null) return false;
            if (user.LoginName.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (!SPUtility.IsLoginValid(mSite, user.LoginName))
            {
                return false;
            }

            if (list != null)
            {
                return list.DoesUserHavePermissions(user, mCheckoutUserPermission);
            }
            return web.DoesUserHavePermissions(user.LoginName, mCheckoutUserPermission);
        }

        private SPUser SwitchCheckoutUser(SPWeb web, SPUser user, Guid fileId)
        {
            RestoreCheckOutUser();
            // 将文件的checkout user修改为当前web的user
            if (fileId != Guid.Empty)
            {
                mCheckOutUser = user.ID;
                mCheckOutFileId = fileId;
                QueryService.ChangeCheckoutUserID(mSite.ID, fileId, web.CurrentUser.ID);
                return web.CurrentUser;
            }
            else
            {
                log.Warn("The unique id of checkout file is empty!");
            }
            return user;
        }

        private SPWeb GetCheckoutWebWithPermission(SPWeb web, SPUser user, Guid fileId, bool hasPermission)
        {
            if (fileId != Guid.Empty && !hasPermission)
            {
                SwitchCheckoutUser(web, user, fileId);
                return web;
            }

            // 使用user重新获取web
            GetCheckoutWebByUser(fileId, web, user);

            try
            {
                ///由于两端System Account不一致，例如源端的checkedout不是system account，但是在目的端是system account
                ///所以需要用native方式改变checkedout，最后再替换。
                if (!mCheckoutWeb.CurrentUser.ID.Equals(user.ID))
                {
                    mCheckOutUser = user.ID;
                    mCheckOutFileId = fileId;
                    QueryService.ChangeCheckoutUserID(mSite.ID, fileId, web.CurrentUser.ID);
                    return web;
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while Getting Checkout Web. Error: " + e.ToString());
                throw;
            }
            return mCheckoutWeb;
        }

        //新的GetCheckoutWeb方法使用
        private SPWeb GetCheckoutWebByUser(SPWeb web, SPUser user)
        {
            if (web.CurrentUser != null)
            {
                if (web.CurrentUser.UserToken.CompareUser(user.UserToken)) return web;
            }

            Guid webId = web.ID;
            DisposeCheckoutResources(mSite.ID, webId, user);

            if (mCheckoutSite == null)
            {
                mCheckoutSite = new SPSite(mSite.ID, user.UserToken);
            }
            if (mCheckoutWeb == null)
            {
                mCheckoutWeb = mCheckoutSite.OpenWeb(webId);
            }
            if (!mCheckoutSite.AllowUnsafeUpdates)
            {
                mCheckoutSite.AllowUnsafeUpdates = true;
            }
            if (!mCheckoutWeb.AllowUnsafeUpdates)
            {
                mCheckoutWeb.AllowUnsafeUpdates = true;
            }

            return mCheckoutWeb;
        }

        private SPWeb GetCheckoutWebByUser(Guid fileId, SPWeb web, SPUser user)
        {
            Guid webId = web.ID;
            DisposeCheckoutResources(mSite.ID, webId, user);

            try
            {
                if (mCheckoutSite == null)
                {
                    mCheckoutSite = new SPSite(mSite.ID, user.UserToken);
                }
                if (mCheckoutWeb == null)
                {
                    mCheckoutWeb = mCheckoutSite.OpenWeb(webId);
                }
                if (!mCheckoutSite.AllowUnsafeUpdates)
                {
                    mCheckoutSite.AllowUnsafeUpdates = true;
                }
                if (!mCheckoutWeb.AllowUnsafeUpdates)
                {
                    mCheckoutWeb.AllowUnsafeUpdates = true;
                }
            }
            catch (Exception e)
            {
                throw new AveWrapperCheckoutFileException(e.Message, e)
                {
                    SiteId = mSite.ID,
                    WebId = webId,
                    FileId = fileId,
                    CheckoutUserId = user.ID,
                    CheckoutUserLoginName = user.LoginName,
                    CheckoutUserDisplayName = user.Name,
                    ErrorType = CheckoutFileErrorType.CheckoutUserNotPermission
                };
            }
            return mCheckoutWeb;
        }

        private void DisposeCheckoutResources(Guid siteId, Guid webId, SPUser user)
        {
            if (mCheckoutSite != null)
            {
                if (siteId != mCheckoutSite.ID || !mCheckoutSite.UserToken.CompareUser(user.UserToken))
                {
                    if (mCheckoutWeb != null)
                    {
                        mCheckoutWeb.Dispose();
                        mCheckoutWeb = null;
                    }
                    mCheckoutSite.Dispose();
                    mCheckoutSite = null;
                }
            }
            if (mCheckoutWeb != null && mCheckoutWeb.ID != webId)
            {
                mCheckoutWeb.Dispose();
                mCheckoutWeb = null;
            }
        }

        public DateTime CertificationDate
        {
            get { return mSite.CertificationDate; }
        }

        public IAveUser SystemAccount
        {
            get
            {
                if (mSystemAccount == null)
                {
                    SPUser user = mSite.SystemAccount;
                    if (user != null)
                    {
                        mSystemAccount = new AveUser(this.RootWeb as AveWeb, user);
                    }
                }
                return mSystemAccount;
            }
        }

        public void Delete(bool deleteADAccounts, bool gradualDelete)
        {
            mSite.Delete(deleteADAccounts, gradualDelete);
        }

        public string MakeFullUrl(string strUrl)
        {
            return mSite.MakeFullUrl(strUrl);
        }

        public string MakeFullUrl(string strUrl, string realWebAppUrl)
        {
            if (string.IsNullOrEmpty(strUrl))
            {
                return strUrl;
            }
            if (!realWebAppUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                realWebAppUrl += "/";
            }
            strUrl = strUrl.TrimStart('/');
            return realWebAppUrl + strUrl;
        }


        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get
            {
                if (mFeatureDefinitionCollection == null)
                {
                    mFeatureDefinitionCollection = new AveFeatureDefinitionCollection(mSite.FeatureDefinitions);
                }
                return mFeatureDefinitionCollection;
            }
        }

        public bool IsMoss
        {
            get
            {
                return WrapperRuntime.CurrentContext.IsMoss;
            }
        }

        public bool IsPublish
        {
            get
            {
                if (IsMoss)
                {
                    return PublishingSite.IsPublishingSite(mSite);
                }
                return false;
            }
        }

        public void RestoreSettings(AveSiteSettingInfo settingInfo)
        {
            if (settingInfo.SyndicationEnabled != null && settingInfo.SyndicationEnabled.IsAvailable && settingInfo.SyndicationEnabled.Value != null)
            {
                mSite.SyndicationEnabled = settingInfo.SyndicationEnabled.Value.Value;
            }
            if (settingInfo.AuditFlags != null && settingInfo.AuditFlags.IsAvailable && mSite.Audit != null)
            {
                if (settingInfo.AuditFlags.Value == null)
                {
                    mSite.Audit.AuditFlags = SPAuditMaskType.None;
                }
                else
                {
                    mSite.Audit.AuditFlags = (SPAuditMaskType)settingInfo.AuditFlags.Value;
                }
            }
            if (settingInfo.TrimAuditLog != null && settingInfo.TrimAuditLog.IsAvailable && settingInfo.TrimAuditLog.Value != null)
            {
                mSite.TrimAuditLog = settingInfo.TrimAuditLog.Value.Value;
            }
                if (settingInfo.AuditLogTrimmingRetention != null && settingInfo.AuditLogTrimmingRetention.IsAvailable && settingInfo.AuditLogTrimmingRetention.Value != null)
                {
                    mSite.AuditLogTrimmingRetention = settingInfo.AuditLogTrimmingRetention.Value.Value;
                }

                if (settingInfo.AuditLogTrimmingCallout != null && settingInfo.AuditLogTrimmingCallout.IsAvailable)
                {
                    mSite.AuditLogTrimmingCallout = settingInfo.AuditLogTrimmingCallout.Value;
                }
            if (settingInfo.UseAuditFlagCache != null && settingInfo.UseAuditFlagCache.IsAvailable
                && settingInfo.UseAuditFlagCache.Value != null && this.Audit != null)
            {
                mSite.Audit.UseAuditFlagCache = settingInfo.UseAuditFlagCache.Value.Value;
            }
            if (settingInfo.AuditLogTrimmingCallout != null && settingInfo.AuditLogTrimmingCallout.IsAvailable)
            {
                mSite.AuditLogTrimmingCallout = settingInfo.AuditLogTrimmingCallout.Value;
            }
            if (settingInfo.UiversionConfigurationEnable != null && settingInfo.UiversionConfigurationEnable.IsAvailable && settingInfo.UiversionConfigurationEnable.Value != null)
            {
                mSite.UIVersionConfigurationEnabled = settingInfo.UiversionConfigurationEnable.Value.Value;
            }
            if (mSite.Audit != null)
            {
                mSite.Audit.Update();
            }
            if (WrapperConfiguration.RestorePortalConnection)
            {
                if (settingInfo.PortalName != null && settingInfo.PortalName.IsAvailable)
                {
                    mSite.PortalName = settingInfo.PortalName.Value;
                }
                if (settingInfo.PortalURL != null && settingInfo.PortalURL.IsAvailable)
                {
                    mSite.PortalUrl = settingInfo.PortalURL.Value;
                }
            }
            // add for version B
            if (settingInfo.AllowDesigner != null && settingInfo.AllowDesigner.IsAvailable && settingInfo.AllowDesigner.Value != null)
            {
                mSite.AllowDesigner = settingInfo.AllowDesigner.Value.Value;
            }
            if (settingInfo.AllowMasterPageEditing != null && settingInfo.AllowMasterPageEditing.IsAvailable && settingInfo.AllowMasterPageEditing.Value != null)
            {
                mSite.AllowMasterPageEditing = settingInfo.AllowMasterPageEditing.Value.Value;
            }
            if (settingInfo.AllowRevertFromTemplate != null && settingInfo.AllowRevertFromTemplate.IsAvailable && settingInfo.AllowRevertFromTemplate.Value != null)
            {
                mSite.AllowRevertFromTemplate = settingInfo.AllowRevertFromTemplate.Value.Value;
            }
            if (settingInfo.ShowURLStructure != null && settingInfo.ShowURLStructure.IsAvailable && settingInfo.ShowURLStructure.Value != null)
            {
                mSite.ShowURLStructure = settingInfo.ShowURLStructure.Value.Value;
            }
            // add for sharepoint 2013 specific settings
            if (settingInfo.AllowExternalEmbedding != null && settingInfo.AllowExternalEmbedding.IsAvailable && settingInfo.AllowExternalEmbedding.Value != null)
            {
                mSite.AllowExternalEmbedding = (ScriptSafeExternalEmbedding)settingInfo.AllowExternalEmbedding.Value;
            }
            //add for Client Object Model Permission Requirement 
            if ((settingInfo.BitFlags.Value & 0x40000000) == 0)
            {
                mSite.UpdateClientObjectModelUseRemoteAPIsPermissionSetting(true);
            }
            else
            {
                mSite.UpdateClientObjectModelUseRemoteAPIsPermissionSetting(false);
            }

            if(settingInfo.ScriptSafeDomains != null && settingInfo.ScriptSafeDomains.IsAvailable)
            {
                ScriptSafeDomains = settingInfo.ScriptSafeDomains.Value;
            }
        }

        /// <summary>
        /// 用于将上次修改的check out user还原。
        /// </summary>
        private void RestoreCheckOutUser()
        {
            if (mCheckOutUser != -1 && mCheckOutFileId != Guid.Empty)
            {
                int tempId = 0;
                if (QueryService.IsCheckOutFile(mSite.ID, mCheckOutFileId, ref tempId))
                {
                    QueryService.ChangeCheckoutUserID(mSite.ID, mCheckOutFileId, mCheckOutUser);
                    mCheckOutFileId = Guid.Empty;
                    mCheckOutUser = -1;
                }
            }
        }

        public IAveSiteSerializer SiteSerializer
        {
            get
            {
                if (m_SiteSerializer == null)
                {
                    m_SiteSerializer = new AveSiteSerializer(this.QueryService, this);
                }
                return m_SiteSerializer;
            }
        }

        public IAveSiteSettingSerializer SiteSettingSerializer
        {
            get
            {
                if (m_SiteSettingSerializer == null)
                {
                    m_SiteSettingSerializer = new AveSiteSettingSerializer(this.QueryService, this);
                }
                return m_SiteSettingSerializer;
            }
        }

        public IAveMetaDataServiceSerializer MetaDataServiceSerializer
        {
            get
            {
                if (m_MetaDataServiceSerializer == null)
                {
                    m_MetaDataServiceSerializer = new AveMetaDataServiceSerializer(mSite);
                }
                return m_MetaDataServiceSerializer;
            }
        }

        public IAveUserSerializer UserSerializer
        {
            get
            {
                if (m_UserSerializer == null)
                {
                    m_UserSerializer = new AveUserSerializer(this.QueryService, mSite.ID);
                }
                return m_UserSerializer;
            }
        }

        public IAveGroupSerializer GroupSerializer
        {
            get
            {
                if (m_GroupSerializer == null)
                {
                    m_GroupSerializer = new AveGroupSerializer(this.QueryService, mSite.ID);
                }
                return m_GroupSerializer;
            }
        }

        public IAveUsersSerializer SiteUsersSerializer
        {
            get
            {
                if (m_SiteUsersSerializer == null)
                {
                    m_SiteUsersSerializer = new AveSiteUsersSerializer(this.QueryService, this);
                }
                return m_SiteUsersSerializer;
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

        public void ReloadSite()
        {
            if (mSite != null)
            {
                if (mSite.ContentDatabase.ExistsInFarm)
                {
                    //Guid id = mSite.ID;
                    String url = mSite.Url;
                    CleanUp();
                    mSite.Close();
                    //不能使用id reload SPSite，因为通过id取SPSite是在进程缓存中取到的，不准确
                    mSite = new SPSite(url);
                    if (mRootWeb != null)
                    {
                        mRootWeb.Dispose();
                        mRootWeb = null;
                        //mRootWeb.ReloadWeb();
                    }
                    this.LastReloadTimeUTC = DateTime.UtcNow;
                    if (!AllowUnsafeUpdates)
                    {
                        AllowUnsafeUpdates = true;
                    }
                }
                else
                {
                    //ADO-162501 对于PR unattachedDB中site的ExistsInFarm为false,不去进行reload
                    //if the ContentDatabase does not exist in farm, we cannot get the site by the way "new SPSite(url)"
                    log.Debug("The site {0} with ID {1} was not exists in Farm, can not reload it.", mSite.Url, mSite.ID);
                }
            }
        }

        public DateTime LastReloadTimeUTC
        {
            get;
            private set;
        }

        public AveAPIType GetAPIType()
        {
            return AveAPIType.Server;
        }

        public AveAPIType APIType
        {
            get
            {
                return AveAPIType.Server;
            }
        }

        public long Size
        {
            get
            {
                return this.QueryService.GetSiteSizeFromSites(this);
            }
        }

        public DateTime LastSecurityModifiedDate
        {
            get
            {
                return mSite.LastSecurityModifiedDate;
            }
        }

        public Dictionary<Guid, long> GetAllWebSize()
        {
            return this.QueryService.GetAllWebSize(this);
        }

        public void GetRecycleBinStatistics(out int itemCount, out long size)
        {
            mSite.GetRecycleBinStatistics(out itemCount, out size);
        }

        public IAveOUserProfileManager GetUserProfileManager()
        {
            return new AveOUserProfileManager(new UserProfileManager(SPServiceContext.GetContext(mSite)));
        }

        public bool Exists(Uri uri)
        {
            return SPSite.Exists(uri);
        }

        #endregion
        public object DataProvider
        {
            get { return null; }
        }

        #region Private Method
        private void CleanUp()
        {
            RestoreCheckOutUser();
            DoDispose(mCheckoutWeb);
            mCheckoutWeb = null;
            DoDispose(mCheckoutSite);
            mCheckoutSite = null;
            DoDispose(mWebAppilcation);
            mWebAppilcation = null;
            DoDispose(mContentDatabase);
            mContentDatabase = null;
            DoDispose(mWorkflowManager);
            mWorkflowManager = null;

            mAllWebs = null;
            mFeatureCollection = null;
            mOwner = null;
            mAudit = null;
            mSolutions = null;
            mRecycleBin = null;
            mSecondaryContact = null;
            mWorkflowManager = null;
            mUsageInfo = default(AveUsageInfo);
            synRoot = new Object();
            mFeatureDefinitionCollection = null;
            m_SiteSerializer = null;
            m_SiteSettingSerializer = null;
            m_MetaDataServiceSerializer = null;
            m_UserSerializer = null;
            m_GroupSerializer = null;
            m_SiteUsersSerializer = null;
            m_FeatureSerializer = null;
            mUserToken = null;
            mSystemAccount = null;
            mQuota = null;
        }

        public void InternalCleanup()
        {
            try
            {
                AveAssemblyUtility.SetFieldValue(null, typeof(Microsoft.SharePoint.Upgrade.SPManager), "s_dictatorPre", null);
                AveAssemblyUtility.SetFieldValue(null, typeof(SPWebService), "m_GalleryCustomTemplates", null);

                var configDb = AveAssemblyUtility.GetFieldValue(this.mSite, "m_ConfigurationDatabase");
                ClearConfigDB(configDb);
                configDb = AveAssemblyUtility.GetFieldValue(null, configDb.GetType(), "s_Local");
                if (configDb != null)
                {
                    ClearConfigDB(configDb);
                    //AveAssemblyUtility.SetFieldValue(null, configDb.GetType(), "s_Local", null);
                }
                //var workflowTemplates = AveAssemblyUtility.GetFieldValue(null, typeof(Microsoft.SharePoint.Workflow.SPWorkflowManager), "_noCodeTemplates") as IDictionary;
                //if (workflowTemplates != null)
                //{
                //    workflowTemplates.Clear();
                //}
                if (AvePoint.Common.AveEnv.IsPublishing)
                {
                    CleanUpForPublishing();
                }
            }
            catch (Exception ex)
            {
                log.Error("{0}", ex);
            }
        }

        private void CleanUpForPublishing()
        {
            AveAssemblyUtility.InvokeStaticMethod(typeof(PublishingSite).Assembly.GetType("Microsoft.SharePoint.Publishing.CacheManager"), "FlushAllCaches");
        }

        private void ClearConfigDB(object db)
        {
            var configDb = db as IDisposable;
            if (configDb != null)
            {
                var objectCache = AveAssemblyUtility.GetPropertyValue(configDb, "ObjectCache");
                if (objectCache != null)
                {
                    AveAssemblyUtility.InvokeMethod(objectCache, "Clear");
                }
                configDb.Dispose();
            }
        }

        private void DoDispose(IDisposable disposable)
        {
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
        #endregion
        // Summary:
        //     Enable all the alerts.the Dictionary key is WebId and value is the Id list of the alerts which you want to enable
        public void EnableAlerts(Dictionary<Guid, List<Guid>> alerts)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSite.EnableAlerts"))
            {

            try
            {
                if (alerts.Count > 0)
                {
                    foreach (IAveJobDefinition definition in this.WebApplication.JobDefinitions)
                    {
                        if (definition.Name.Equals("job-immediate-alerts", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var scope = new AvePerformanceScope("AvePoint.Wrapper.Restore.AveSPAlert.EnableAllAlerts.RunTimerJob"))
                            {
                                DateTime originalLastRunTime = definition.LastRunTime;
                                definition.RunNow();
                                //如果环境的timer job不起，会一直hang在这里，加入等待的最大时间 ADO-33776
                                int mostWaitingTimeCount = 0;
                                while (definition.LastRunTime == originalLastRunTime && mostWaitingTimeCount < 600)
                                {
                                    Thread.Sleep(1000);
                                    mostWaitingTimeCount++;
                                }
                                if (mostWaitingTimeCount == 600)
                                {
                                    log.Log(AveLogLevel.WARN, "Timer job job-immediate-alerts does not exist after 10 minutes. Disability Status: {0}", definition.IsDisabled.ToString());
                                }
                            }
                            break;
                        }
                    }
                    foreach (Guid webId in alerts.Keys)
                    {
                        using (IAveWeb web = this.OpenWeb(webId))
                        {
                            foreach (Guid alertId in alerts[webId])
                            {
                                try
                                {
                                    IAveAlert alert = web.Alerts[alertId];
                                    alert.Status = AveAlertStatus.On;
                                    alert.Update(false);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateAlertError, e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while enable alerts. Error Message: {0}", ex);
            }

            }

        }

        public DateTime GetLastAccessedDayOfSite()
        {
            DateTime lastAccessedDayOfSite = DateTime.MinValue;
            foreach (Guid webId in QueryService.GetAllWebsGuidByNative(mSite.ID))
            {
                DateTime lastAccessedDayOfWeb = QueryService.GetLastAccessedDayOfWeb(ID, webId);
                lastAccessedDayOfSite = lastAccessedDayOfWeb > lastAccessedDayOfSite ? lastAccessedDayOfWeb : lastAccessedDayOfSite;
            }

            if (lastAccessedDayOfSite < LastContentModifiedDate)
            {
                lastAccessedDayOfSite = LastContentModifiedDate;
            }
            if (lastAccessedDayOfSite < LastSecurityModifiedDate)
            {
                lastAccessedDayOfSite = LastSecurityModifiedDate;
            }
            return lastAccessedDayOfSite;
        }

        public string GetUserLoginBySystemId(byte[] systemId)
        {
            return QueryService.GetUserLoginBySystemId(mSite.ID, systemId);
        }

        public bool ActiveDeletedUserBySystemId(byte[] systemId)
        {
            QueryService.ActiveDeletedUserBySystemId(mSite.ID, systemId);
            return true;
        }

        private Nullable<bool> isClassicModeAuthentication;
        public bool IsClassicWindowsModeAuthentication
        {
            get
            {
                if (!this.isClassicModeAuthentication.HasValue)
                {
                    isClassicModeAuthentication = false;
                    foreach (var setting in Site.WebApplication.IisSettings.Values)
                    {
                        this.isClassicModeAuthentication = setting.AuthenticationMode == System.Web.Configuration.AuthenticationMode.Windows;
                        break;
                    }
                }
                return this.isClassicModeAuthentication.Value;
            }
        }

        public bool IsOnlineSite
        {
            get 
            {
                return false;
            }
        }

        public int CompatibilityLevel
        {
            get { return mSite.CompatibilityLevel; }
        }

        public string SPVersion
        {
            get
            {
                return SPFarm.Local.BuildVersion.ToString();
            }
        }

        #region add for SP2013
        public bool Archived
        {
            get { return mSite.Archived; }
            set { mSite.Archived = value; }
        }

        public bool ReadOnlyMode
        {
            get { return (bool)AveAssemblyUtility.GetPropertyValue(mSite, "ReadOnlyMode"); }
            set { AveAssemblyUtility.SetPropertyValue(mSite, "ReadOnlyMode", value); }
        }
        public AveBasePermissions DenyPermissionsMask
        {
            get { return (AveBasePermissions)mSite.DenyPermissionsMask; }
            set { mSite.DenyPermissionsMask = (SPBasePermissions)value; }
        }

        public AveScriptSafeExternalEmbedding AllowExternalEmbedding
        {
            get { return (AveScriptSafeExternalEmbedding)mSite.AllowExternalEmbedding; }
            set { mSite.AllowExternalEmbedding = (ScriptSafeExternalEmbedding)value; }
        }
        #endregion

        #region Add to operate Change Log

        public IAveChangeCollection GetChanges()
        {
            return new AveChangeCollection(mSite.GetChanges());
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            return new AveChangeCollection(mSite.GetChanges((query as AveChangeQuery).ChangeQuery));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            return new AveChangeCollection(mSite.GetChanges((changeToken as AveChangeToken).ChangeToken));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            SPChangeToken ct1 = (changeToken as AveChangeToken).ChangeToken;
            SPChangeToken ct2 = (changeTokenEnd as AveChangeToken).ChangeToken;
            return new AveChangeCollection(mSite.GetChanges(ct1, ct2));
        }

        #endregion

        public IAveRecycleBinItemCollection GetRecycleBinItems(IAveRecycleBinQuery query)
        {
            var collection = mSite.GetRecycleBinItems(((AveRecycleBinQuery)query)?.RecycleBinQuery);
            if (collection != null)
            {
                return new AveRecycleBinItemCollection(this, collection);
            }
            return null;
        }

        public Dictionary<string, string> GetLookupItemIdAndDisplayValue(AveLookupFieldInfo fieldInfo)
        {
            return QueryService.GetLookupItemIdAndDisplayValue(fieldInfo);
        }


        public bool AdministratorOperationMode
        {
            get
            {
                return Convert.ToBoolean(AveAssemblyUtility.GetPropertyValue(this.mSite, "AdministratorOperationMode"));
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(this.mSite, "AdministratorOperationMode", value);
            }
        }


        public AveBitField Flags
        {
            get
            {
                return (AveBitField)AveAssemblyUtility.GetPropertyValue(mSite, "Flags");
            }
        }


        public IAveQuerySession SqlSession
        {
            get 
            {
                object session = AveAssemblyUtility.GetPropertyValue(mSite, "SqlSession");
                if (session != null)
                {
                    return new AveQuerySession(session);
                }
                return null; 
            }
        }


        public string AppSiteDomainPrefix
        {
            get
            {
                return Convert.ToString(AveAssemblyUtility.GetPropertyValue(mSite, "AppSiteDomainPrefix"));
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mSite, "m_appSiteDomainPrefix", value);
            }
        }


        public void CustomizeReport(Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public bool MigrateUser(string oldLogin, byte[] oldSid, string newLogin, byte[] newSid)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(this.mSite, "MigrateUser", new Type[] { typeof(string), typeof(byte[]), typeof(string), typeof(byte[]) }, new object[] { oldLogin, oldSid, newLogin, newSid });
        }


        public WrapperNativeApiPermission NativeApiPermission
        {
            get 
            {
                if(nativeApiPermission == WrapperNativeApiPermission.None)
                {
                    try
                    {
                        if (WrapperConfiguration.VerifyNativePermissionAutomatically)
                        {
                            nativeApiPermission = QueryService.DoesUserHasEnoughPermission() ? WrapperNativeApiPermission.FullControl : WrapperNativeApiPermission.NativeRead;
                        }
                        else
                        {
                            nativeApiPermission = WrapperConfiguration.DefaultNativePermissionLevel;
                        }
                    }
                    catch(Exception ex)
                    {
                        log.Warn("cannot get analyze permission:{0}", ex);
                        nativeApiPermission = WrapperConfiguration.DefaultNativePermissionLevel;
                    }
                }

                return nativeApiPermission;
            }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return new AveUserCustomActionCollection(Site.UserCustomActions);
            }
        }

        public bool DeleteMigrationJob(Guid id)
        {
            throw new NotSupportedException();
        }

        public AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            throw new NotSupportedException();
        }

        public Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            throw new NotSupportedException();
        }

        public Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            throw new NotSupportedException();
        }

        public AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            throw new NotSupportedException();
        }

        public AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            throw new NotSupportedException();
        }

        public void ApplyCustomWebTemplateInSolution(string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            throw new NotImplementedException();
        }
        internal string GetVariationLabelName(Guid labelId)
        {
            return VariationLabelsMappingIdKey.ContainsKey(labelId) ? VariationLabelsMappingIdKey[labelId] : string.Empty;
        }

        public Guid GetVariationLabelId(string labelName)
        {
            return VariationLabelsMappingNameKey.ContainsKey(labelName) ? VariationLabelsMappingNameKey[labelName] : Guid.Empty;
        }

        
        private Dictionary<Guid, string> variationLabelsMappingIdKey;
        internal Dictionary<Guid, string> VariationLabelsMappingIdKey
        {
            get
            {
                return variationLabelsMappingIdKey = variationLabelsMappingIdKey != null ? variationLabelsMappingIdKey : GetVariationLabelMappingIdKey();
            }
        }

        private Dictionary<string, Guid> variationLabelsMappingNameKey;
        internal Dictionary<string, Guid> VariationLabelsMappingNameKey
        {
            get
            {
                return variationLabelsMappingNameKey = variationLabelsMappingNameKey ?? GetVariationLabelMappingNameKey();
            }
        }

        public IAveProjectServer ProjectServer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectCollection Projects
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectCalendarCollection ProjectCalendars
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectCustomFieldCollection ProjectCustomFields
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectLookupTableCollection ProjectLookupTables
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectEnterpriseProjectTypeCollection ProjectEnterpriseProjectTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectEnterpriseResourceCollection ProjectEnterpriseResources
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectPhaseCollection ProjectPhases
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveProjectStageCollection ProjectStages
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private Dictionary<Guid, string> GetVariationLabelMappingIdKey()
        {
            lock (synRoot)
            {
                var items = GetVariationLabels();
                return Enumerable.Cast<SPListItem>(items).ToDictionary(item => item.UniqueId, item => item["Title"] != null ? item["Title"].ToString() : string.Empty); //InternalName=Title，DisplayName=Label，使用InternalName取值
            }
        }

        private Dictionary<string, Guid> GetVariationLabelMappingNameKey()
        {
            lock (synRoot)
            {
                var items = GetVariationLabels();
                return Enumerable.Cast<SPListItem>(items).ToDictionary(item => item["Title"] != null ? item["Title"].ToString() : string.Empty, item => item.UniqueId); //InternalName=Title，DisplayName=Label，使用InternalName取值
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", MessageId = "Microsoft.SharePoint.SPList.get_Items", Justification = "Do not refuse SharePoint API")]
        private IEnumerable<SPListItem> GetVariationLabels()
        {
            try
            {
                var listiId = new Guid(this.Site.RootWeb.AllProperties["_VarLabelsListId"].ToString());
                var list = this.Site.RootWeb.Lists.GetList(listiId, true);
                return list.Items.Cast<SPListItem>();
            }
            catch (Exception ex)
            {
                log.Warn("Cannot find variation labels list in site: {0}, error: {1}", this.Url, ex);
                return new SPListItem[0];
            }
        }

        public IAveFeatureDefinitionCollection GetAllFeatureDefinitions()
        {
            throw new NotImplementedException();
        }

        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            throw new NotImplementedException();
        }

        public List<AveComplianceTagInfo> GetAvailableTagsForSite()
        {
            throw new NotImplementedException();
        }
    }

    class AveMonitoredScope
    {
        static System.Collections.Concurrent.ConcurrentDictionary<Guid, SPMonitoredScope> s_GlobalCurrentScopes;

        public static System.Collections.Concurrent.ConcurrentDictionary<Guid, SPMonitoredScope> GlobalCurrentScopes
        {
            get
            {
                if (s_GlobalCurrentScopes == null)
                {
                    s_GlobalCurrentScopes = AveAssemblyUtility.GetFieldValue(null, typeof(SPMonitoredScope), "s_GlobalCurrentScopes") as System.Collections.Concurrent.ConcurrentDictionary<Guid, SPMonitoredScope>;
                }

                return s_GlobalCurrentScopes;
            }
        }

        internal static void RemoveCurrentScope()
        {
            var scope = SPMonitoredScope.Current;
            if(scope != null)
            {
                SPMonitoredScope relatedScope;
                if(GlobalCurrentScopes.TryRemove(scope.Id, out relatedScope))
                {
                    relatedScope.Dispose();
                }
            }
        }
    }
}
