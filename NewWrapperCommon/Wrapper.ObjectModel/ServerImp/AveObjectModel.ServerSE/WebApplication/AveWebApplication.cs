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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.MobileMessage;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWebApplication : AvePersistedUpgradableObject, IAveWebApplication
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveWebApplication));
        protected SPWebApplication mWebApplication;
        private const string mWebApplication_Type = "Microsoft.SharePoint.Administration.SPWebApplication";
        private AveContentDatabaseCollection mDBCol;
        private AveAlternateUrlCollection mAlterCol;
        private AveApplicationPool mApplicationPool;
        private AveDocumentConverterCollection mDocumentConverters;
        private Dictionary<AveUrlZone, IAveIisSettings> mIisSettings;
        private AvePrefixCollection mPrefixes;
        private bool mSecurityPolicyChanged;
        private AveServiceApplicationProxyGroup mServiceApplicationProxyGroup;
        private AveMobileMessagingAccount mOutboundSmsServiceAccount;
        private AveFormDigestSettings mFormDigestSettings;
        private AveOutboundMailServiceInstance mOutboundMailServiceInstance;
        private Collection<IAveOfficialFileHost> mOfficialFileHosts;
        private AveSiteCollection mSites;
        private AveHttpThrottleSettings mHttpThrottleSettings;
        private AvePolicyRoleCollection mPolicyRoleCollection;
        private AveJobDefinitionCollection mJobDefinitions;
        private AveDataRetrievalProvider mDataRetrievalProvider;
        private AvePolicyCollection mPolicies;
        private AveFeatureCollection mFeatures;
        private AvePeoplePickerSettings mPeoplePickerSettings;
        private AveWebConfigModificationCollection mWebConfigModifications;

        public AveWebApplication(SPWebApplication webApp)
            : base(webApp)
        {
            mWebApplication = webApp;
        }

        public AveWebApplication(string url)
            : this(SPWebApplication.Lookup(new Uri(url)))
        { }

        public AveWebApplication()
            : this(new SPWebApplication())
        { }

        internal SPWebApplication WebApplication
        {
            get
            {
                return mWebApplication;
            }
        }

        #region IAveWebApplication Members

        public IAveServiceApplicationProxyGroup ServiceApplicationProxyGroup
        {
            get
            {
                if (mServiceApplicationProxyGroup == null)
                {
                    SPServiceApplicationProxyGroup serviceApplicationProxyGroup = mWebApplication.ServiceApplicationProxyGroup;
                    if (serviceApplicationProxyGroup != null)
                    {
                        mServiceApplicationProxyGroup = new AveServiceApplicationProxyGroup(serviceApplicationProxyGroup);
                    }
                }
                return mServiceApplicationProxyGroup;
            }
            set
            {
                mServiceApplicationProxyGroup = value as AveServiceApplicationProxyGroup;
                if (mServiceApplicationProxyGroup != null)
                {
                    mWebApplication.ServiceApplicationProxyGroup = mServiceApplicationProxyGroup.ServiceApplicationProxyGroup;
                }
                else
                {
                    mWebApplication.ServiceApplicationProxyGroup = null;
                }
            }
        }

        public IAveWebApplication Lookup(Uri uri)
        {
            AveWebApplication aveWebApp = null;
            try
            {
                SPWebApplication webApplication = SPWebApplication.Lookup(uri);
                if (webApplication == null)
                {
                    return null;
                }
                aveWebApp = new AveWebApplication(webApplication);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.WebAppLookupError,
                    uri == null ? string.Empty : uri.AbsoluteUri, ex);
            }
            return aveWebApp;
        }

        public IAveSiteCollection Sites
        {
            get
            {
                if (mSites == null)
                {
                    mSites = new AveSiteCollection(mWebApplication.Sites);
                }
                return mSites;
            }
        }

        public Uri GetResponseUri()
        {
            return mWebApplication.GetResponseUri(SPUrlZone.Default);
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public IAveContentDatabaseCollection ContentDatabases
        {
            get
            {
                if (mDBCol == null)
                {
                    SPContentDatabaseCollection contentDatabaseCollection = mWebApplication.ContentDatabases;
                    if (contentDatabaseCollection != null)
                    {
                        mDBCol = new AveContentDatabaseCollection(contentDatabaseCollection);
                    }
                }
                return mDBCol;
            }
        }

        public bool EmailToNoPermissionWorkflowParticipantsEnabled
        {
            get
            {
                return mWebApplication.EmailToNoPermissionWorkflowParticipantsEnabled;
            }
            set
            {
                mWebApplication.EmailToNoPermissionWorkflowParticipantsEnabled = value;
            }
        }

        public bool ExternalWorkflowParticipantsEnabled
        {
            get
            {
                return mWebApplication.ExternalWorkflowParticipantsEnabled;
            }
            set
            {
                mWebApplication.ExternalWorkflowParticipantsEnabled = value;
            }
        }

        public IAveAlternateUrlCollection AlternateUrls
        {
            get
            {
                if (mAlterCol == null)
                {
                    SPAlternateUrlCollection alternateUrlCollection = mWebApplication.AlternateUrls;
                    if (alternateUrlCollection != null)
                    {
                        mAlterCol = new AveAlternateUrlCollection(alternateUrlCollection);
                    }
                }
                return mAlterCol;
            }
        }

        public Collection<string> BlockedFileExtensions
        {
            get
            {
                return mWebApplication.BlockedFileExtensions;
            }
        }

        public IAveApplicationPool ApplicationPool
        {
            get
            {
                if (mApplicationPool == null)
                {
                    SPApplicationPool applicationPool = mWebApplication.ApplicationPool;
                    if (applicationPool != null)
                    {
                        mApplicationPool = new AveApplicationPool(applicationPool);
                    }
                }
                return mApplicationPool;
            }
        }

        public Dictionary<AveUrlZone, IAveIisSettings> IisSettings
        {
            get
            {
                Dictionary<SPUrlZone, SPIisSettings> iisSettings = mWebApplication.IisSettings;
                if (iisSettings != null)
                {
                    mIisSettings = new Dictionary<AveUrlZone, IAveIisSettings>();
                    foreach (SPUrlZone uselZone in mWebApplication.IisSettings.Keys)
                    {
                        SPIisSettings spIisSettings = mWebApplication.IisSettings[uselZone];
                        if (spIisSettings != null)
                        {
                            mIisSettings.Add((AveUrlZone)uselZone, new AveIisSettings(spIisSettings));
                        }
                        else
                        {
                            mIisSettings.Add((AveUrlZone)uselZone, null);
                        }
                    }
                    return mIisSettings;
                }
                return null;
            }
        }

        public bool SelfServiceSiteCreationEnabled
        {
            get
            {
                return mWebApplication.SelfServiceSiteCreationEnabled;
            }
            set
            {
                mWebApplication.SelfServiceSiteCreationEnabled = value;
            }
        }

        public Uri GetResponseUri(AveUrlZone urlZone)
        {
            return mWebApplication.GetResponseUri((SPUrlZone)urlZone);
        }

        public override void Update()
        {
            if (mIisSettings != null)
            {
                Dictionary<SPUrlZone, SPIisSettings> iisSettings = new Dictionary<SPUrlZone, SPIisSettings>();
                foreach (AveUrlZone urlZone in mIisSettings.Keys)
                {
                    iisSettings.Add((SPUrlZone)urlZone, (mIisSettings[urlZone] as AveIisSettings).IisSettings);
                }
                AveAssemblyUtility.SetFieldValue(mWebApplication, "m_IisSettings", iisSettings);
            }
            if (mOfficialFileHosts != null)
            {
                Collection<SPOfficialFileHost> officialFileHost = new Collection<SPOfficialFileHost>();
                foreach (IAveOfficialFileHost host in mOfficialFileHosts)
                {
                    officialFileHost.Add((host as AveOfficialFileHost).OfficialFileHost);
                }
                AveAssemblyUtility.SetFieldValue(mWebApplication, "m_OfficialFileHosts", officialFileHost);
            } 
            mWebApplication.Update();
        }

        public bool AlertsEnabled
        {
            get
            {
                return mWebApplication.AlertsEnabled;
            }
            set
            {
                mWebApplication.AlertsEnabled = value;
            }
        }

        public bool AlertsLimited
        {
            get
            {
                return mWebApplication.AlertsLimited;
            }
            set
            {
                mWebApplication.AlertsLimited = value;
            }
        }

        public int AlertsMaximum
        {
            get
            {
                return mWebApplication.AlertsMaximum;
            }
            set
            {
                mWebApplication.AlertsMaximum = value;
            }
        }

        public bool ChangeLogExpirationEnabled
        {
            get
            {
                return mWebApplication.ChangeLogExpirationEnabled;
            }
            set
            {
                mWebApplication.ChangeLogExpirationEnabled = value;
            }
        }

        public TimeSpan ChangeLogRetentionPeriod
        {
            get
            {
                return mWebApplication.ChangeLogRetentionPeriod;
            }
            set
            {
                mWebApplication.ChangeLogRetentionPeriod = value;
            }
        }

        public string DefaultQuotaTemplate
        {
            get
            {
                return mWebApplication.DefaultQuotaTemplate;
            }
            set
            {
                mWebApplication.DefaultQuotaTemplate = value;
            }
        }

        public int DefaultTimeZone
        {
            get
            {
                return mWebApplication.DefaultTimeZone;
            }
            set
            {
                mWebApplication.DefaultTimeZone = value;
            }
        }

        public bool EventHandlersEnabled
        {
            get
            {
                return mWebApplication.EventHandlersEnabled;
            }
            set
            {
                mWebApplication.EventHandlersEnabled = value;
            }
        }

        public int MaximumFileSize
        {
            get
            {
                return mWebApplication.MaximumFileSize;
            }
            set
            {
                mWebApplication.MaximumFileSize = value;
            }
        }

        public bool MetaWeblogAuthenticationEnabled
        {
            get
            {
                return mWebApplication.MetaWeblogAuthenticationEnabled;
            }
            set
            {
                mWebApplication.MetaWeblogAuthenticationEnabled = value;
            }
        }

        public bool MetaWeblogEnabled
        {
            get
            {
                return mWebApplication.MetaWeblogEnabled;
            }
            set
            {
                mWebApplication.MetaWeblogEnabled = value;
            }
        }

        public string OutboundMailReplyToAddress
        {
            get
            {
                return mWebApplication.OutboundMailReplyToAddress;
            }
            set
            {
                mWebApplication.OutboundMailReplyToAddress = value;
            }
        }

        public string OutboundMailSenderAddress
        {
            get
            {
                return mWebApplication.OutboundMailSenderAddress;
            }
            set
            {
                mWebApplication.OutboundMailSenderAddress = value;
            }
        }

        public string StrOutboundSMTPServer
        {
            get
            {
                if (OutboundMailServiceInstance != null)
                {
                    return OutboundMailServiceInstance.Server.Address;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (OutboundMailServiceInstance != null)
                {
                    OutboundMailServiceInstance.Server.Address = value;
                }                
            }

        }

        public int OutboundMailCodePage
        {
            get
            {
                return mWebApplication.OutboundMailCodePage;
            }
            set
            {
                mWebApplication.OutboundMailCodePage = value;
            }
        }

        public bool PresenceEnabled
        {
            get
            {
                return mWebApplication.PresenceEnabled;
            }
            set
            {
                mWebApplication.PresenceEnabled = value;
            }
        }

        public bool RecycleBinCleanupEnabled
        {
            get
            {
                return mWebApplication.RecycleBinCleanupEnabled;
            }
            set
            {
                mWebApplication.RecycleBinCleanupEnabled = value;
            }
        }

        public int SecondStageRecycleBinQuota
        {
            get
            {
                return mWebApplication.SecondStageRecycleBinQuota;
            }
            set
            {
                mWebApplication.SecondStageRecycleBinQuota = value;
            }
        }

        public bool SendLoginCredentialsByEmail
        {
            get
            {
                return mWebApplication.SendLoginCredentialsByEmail;
            }
            set
            {
                mWebApplication.SendLoginCredentialsByEmail = value;
            }
        }

        public IAveFormDigestSettings FormDigestSettings
        {
            get
            {
                if (mFormDigestSettings == null)
                {
                    mFormDigestSettings = new AveFormDigestSettings(mWebApplication.FormDigestSettings);
                }
                return mFormDigestSettings;
            }
        }

        public bool SyndicationEnabled
        {
            get
            {
                return mWebApplication.SyndicationEnabled;
            }
            set
            {
                mWebApplication.SyndicationEnabled = value;
            }
        }

        public bool AllowAccessToWebPartCatalog
        {
            get
            {
                return mWebApplication.AllowAccessToWebPartCatalog;
            }
            set
            {
                mWebApplication.AllowAccessToWebPartCatalog = value;
            }
        }

        public bool AllowPartToPartCommunication
        {
            get
            {
                return mWebApplication.AllowPartToPartCommunication;
            }
            set
            {
                mWebApplication.AllowPartToPartCommunication = value;
            }
        }

        public bool RequireContactForSelfServiceSiteCreation
        {
            get
            {
                return mWebApplication.RequireContactForSelfServiceSiteCreation;
            }
            set
            {
                mWebApplication.RequireContactForSelfServiceSiteCreation = value;
            }
        }

        public bool RecycleBinEnabled
        {
            get
            {
                return mWebApplication.RecycleBinEnabled;
            }
            set
            {
                mWebApplication.RecycleBinEnabled = value;
            }
        }

        public bool MasterPageReferenceEnabled
        {
            get
            {
                return mWebApplication.MasterPageReferenceEnabled;
            }
            set
            {
                mWebApplication.MasterPageReferenceEnabled = value;
            }
        }

        public IAveOutboundMailServiceInstance OutboundMailServiceInstance
        {
            get
            {
                if (mOutboundMailServiceInstance == null)
                {
                    SPOutboundMailServiceInstance outboundMailServiceInstance = mWebApplication.OutboundMailServiceInstance;
                    if (outboundMailServiceInstance != null)
                    {
                        mOutboundMailServiceInstance = new AveOutboundMailServiceInstance(outboundMailServiceInstance);
                    }
                }
                return mOutboundMailServiceInstance;
            }
            set
            {
                mOutboundMailServiceInstance = value as AveOutboundMailServiceInstance;
                if (mOutboundMailServiceInstance != null)
                {
                    mWebApplication.OutboundMailServiceInstance = mOutboundMailServiceInstance.OutboundMailServiceInstance;
                }
                else
                {
                    mWebApplication.OutboundMailServiceInstance = null;
                }
            }
        }

        public IAvePrefixCollection Prefixes
        {
            get
            {
                if (mPrefixes == null)
                {
                    SPPrefixCollection prefixCollection = mWebApplication.Prefixes;
                    if (prefixCollection != null)
                    {
                        mPrefixes = new AvePrefixCollection(prefixCollection);
                    }
                }
                return mPrefixes;
            }
        }

        public IAveMobileMessagingAccount OutboundSmsServiceAccount
        {
            get
            {
                if (mOutboundSmsServiceAccount == null)
                {
                    SPMobileMessagingAccount mobileMessagingAccount = mWebApplication.OutboundSmsServiceAccount;
                    if (mobileMessagingAccount != null)
                    {
                        mOutboundSmsServiceAccount = new AveMobileMessagingAccount(mobileMessagingAccount);
                    }
                }
                return mOutboundSmsServiceAccount;
            }
        }

        public bool ScopeExternalConnectionsToSiteSubscriptions
        {
            get
            {
                return mWebApplication.ScopeExternalConnectionsToSiteSubscriptions;
            }
            set
            {
                mWebApplication.ScopeExternalConnectionsToSiteSubscriptions = value;
            }
        }

        public Collection<IAveOfficialFileHost> OfficialFileHosts
        {
            get
            {
                if (mOfficialFileHosts == null)
                {
                    mOfficialFileHosts = new Collection<IAveOfficialFileHost>();
                    foreach (SPOfficialFileHost spOfficialFileHost in mWebApplication.OfficialFileHosts)
                    {
                        if (spOfficialFileHost != null)
                        {
                            mOfficialFileHosts.Add(new AveOfficialFileHost(spOfficialFileHost));
                        }
                        else
                        {
                            mOfficialFileHosts.Add(null);
                        }
                    }
                }
                return mOfficialFileHosts;
            }
        }

        public void UpdateSmsAccount(IAveMobileMessagingAccount account)
        {
            mWebApplication.UpdateSmsAccount((account as AveMobileMessagingAccount).MobileMessagingAccount);
        }

        public void UpdateWorkflowConfigurationSettings()
        {
            mWebApplication.UpdateWorkflowConfigurationSettings();
        }

        public void UpdateMailSettings(string strOutboundSMTPServer, string strFromAddress, string strReplyToAddress, int nCodePage)
        {
            mWebApplication.UpdateMailSettings(strOutboundSMTPServer, strFromAddress, strReplyToAddress, nCodePage);
        }

        public bool UserDefinedWorkflowsEnabled
        {
            get
            {
                return mWebApplication.UserDefinedWorkflowsEnabled;
            }
            set
            {
                mWebApplication.UserDefinedWorkflowsEnabled = value;
            }
        }

        public IAveDocumentConverterCollection DocumentConverters
        {
            get
            {
                if (mDocumentConverters == null)
                {
                    mDocumentConverters = new AveDocumentConverterCollection(mWebApplication.DocumentConverters);
                }
                return mDocumentConverters;
            }
        }

        public bool InheritDataRetrievalSettings
        {
            get
            {
                return mWebApplication.InheritDataRetrievalSettings;
            }
            set
            {
                mWebApplication.InheritDataRetrievalSettings = value;
            }
        }

        public IAveDataRetrievalProvider DataRetrievalProvider
        {
            get
            {
                if (mDataRetrievalProvider == null)
                {
                    SPDataRetrievalProvider dataRetrievalProvider = mWebApplication.DataRetrievalProvider;
                    if (dataRetrievalProvider != null)
                    {
                        mDataRetrievalProvider = new AveDataRetrievalProvider(dataRetrievalProvider);
                    }
                }
                return mDataRetrievalProvider;
            }
        }

        public bool IsAdministrationWebApplication
        {
            get
            {
                return mWebApplication.IsAdministrationWebApplication;
            }
            set
            {
                mWebApplication.IsAdministrationWebApplication = value;
            }
        }

        public void UnprovisionIisWebSites(bool deleteWebSites, string[] serverComments, string applicationPoolId)
        {
            AveAssemblyUtility.InvokeStaticMethod(mWebApplication_Type, "UnprovisionIisWebSites", new Type[] { typeof(bool), typeof(string[]), typeof(string) }, new object[] { deleteWebSites, serverComments, applicationPoolId });
        }

        public AveUrlZone? ExternalUrlZone
        {
            get
            {
                return (AveUrlZone?)mWebApplication.ExternalUrlZone;
            }
            set
            {
                mWebApplication.ExternalUrlZone = (SPUrlZone?)value;
            }
        }

        public bool UseClaimsAuthentication
        {
            get
            {
                return mWebApplication.UseClaimsAuthentication;
            }
            set
            {
                mWebApplication.UseClaimsAuthentication = value;
            }
        }

        public IAvePolicyCollection Policies
        {
            get
            {
                if (mPolicies == null)
                {
                    SPPolicyCollection policies = mWebApplication.Policies;
                    if (policies != null)
                    {
                        mPolicies = new AvePolicyCollection(mWebApplication.Policies);
                    }
                }
                return mPolicies;
            }
        }

        public IAvePolicyCollection ZonePolicies(AveUrlZone zone)
        {
            SPPolicyCollection policyCollection = mWebApplication.ZonePolicies((SPUrlZone)zone);
            if (policyCollection != null)
            {
                return new AvePolicyCollection(policyCollection);
            }
            return null;
        }

        public bool AllowDesigner
        {
            get
            {
                return mWebApplication.AllowDesigner;
            }
            set
            {
                mWebApplication.AllowDesigner = value;
            }
        }

        public bool AllowMasterPageEditing
        {
            get
            {
                return mWebApplication.AllowMasterPageEditing;
            }
            set
            {
                mWebApplication.AllowMasterPageEditing = value;
            }
        }

        public bool AllowRevertFromTemplate
        {
            get
            {
                return mWebApplication.AllowRevertFromTemplate;
            }
            set
            {
                mWebApplication.AllowRevertFromTemplate = value;
            }
        }

        public bool AllowContributorsToEditScriptableParts
        {
            get
            {
                return mWebApplication.AllowContributorsToEditScriptableParts;
            }
            set
            {
                mWebApplication.AllowContributorsToEditScriptableParts = value;
            }
        }

        public bool AllowOMCodeOverrideThrottleSettings
        {
            get
            {
                return mWebApplication.AllowOMCodeOverrideThrottleSettings;
            }
            set
            {
                mWebApplication.AllowOMCodeOverrideThrottleSettings = value;
            }
        }

        public bool AutomaticallyDeleteUnusedSiteCollections
        {
            get
            {
                return mWebApplication.AutomaticallyDeleteUnusedSiteCollections;
            }
            set
            {
                mWebApplication.AutomaticallyDeleteUnusedSiteCollections = value;
            }
        }

        public bool Exists
        {
            get
            {
                return mWebApplication != null;
            }
        }

        public uint DailyStartUnthrottledPrivilegedOperationsHour
        {
            get
            {
                return mWebApplication.DailyStartUnthrottledPrivilegedOperationsHour;
            }
            set
            {
                mWebApplication.DailyStartUnthrottledPrivilegedOperationsHour = value;
            }
        }

        public uint DailyStartUnthrottledPrivilegedOperationsMinute
        {
            get
            {
                return mWebApplication.DailyStartUnthrottledPrivilegedOperationsMinute;
            }
            set
            {
                mWebApplication.DailyStartUnthrottledPrivilegedOperationsMinute = value;
            }
        }

        public uint DailyUnthrottledPrivilegedOperationsDuration
        {
            get
            {
                return mWebApplication.DailyUnthrottledPrivilegedOperationsDuration;
            }
            set
            {
                mWebApplication.DailyUnthrottledPrivilegedOperationsDuration = value;
            }
        }

        public IAveHttpThrottleSettings HttpThrottleSettings
        {
            get
            {
                if (mHttpThrottleSettings == null)
                {
                    SPHttpThrottleSettings httpThrottleSettings = mWebApplication.HttpThrottleSettings;
                    if (httpThrottleSettings != null)
                    {
                        mHttpThrottleSettings = new AveHttpThrottleSettings(httpThrottleSettings);
                    }
                }
                return mHttpThrottleSettings;
            }
        }

        public uint MaxItemsPerThrottledOperation
        {
            get
            {
                return mWebApplication.MaxItemsPerThrottledOperation;
            }
            set
            {
                mWebApplication.MaxItemsPerThrottledOperation = value;
            }
        }

        public uint MaxItemsPerThrottledOperationOverride
        {
            get
            {
                return mWebApplication.MaxItemsPerThrottledOperationOverride;
            }
            set
            {
                mWebApplication.MaxItemsPerThrottledOperationOverride = value;
            }
        }

        public uint MaxItemsPerThrottledOperationWarningLevel
        {
            get
            {
                return mWebApplication.MaxItemsPerThrottledOperationWarningLevel;
            }
            set
            {
                mWebApplication.MaxItemsPerThrottledOperationWarningLevel = value;
            }
        }

        public uint MaxQueryLookupFields
        {
            get
            {
                return mWebApplication.MaxQueryLookupFields;
            }
            set
            {
                mWebApplication.MaxQueryLookupFields = value;
            }
        }

        public uint MaxUniquePermScopesPerList
        {
            get
            {
                return mWebApplication.MaxUniquePermScopesPerList;
            }
            set
            {
                mWebApplication.MaxUniquePermScopesPerList = value;
            }
        }

        public bool SendUnusedSiteCollectionNotifications
        {
            get
            {
                return mWebApplication.SendUnusedSiteCollectionNotifications;
            }
            set
            {
                mWebApplication.SendUnusedSiteCollectionNotifications = value;
            }
        }

        public bool UnthrottledPrivilegedOperationWindowEnabled
        {
            get
            {
                return mWebApplication.UnthrottledPrivilegedOperationWindowEnabled;
            }
            set
            {
                mWebApplication.UnthrottledPrivilegedOperationWindowEnabled = value;
            }
        }

        public TimeSpan UnusedSiteNotificationPeriod
        {
            get
            {
                return mWebApplication.UnusedSiteNotificationPeriod;
            }
            set
            {
                mWebApplication.UnusedSiteNotificationPeriod = value;
            }
        }

        public int UnusedSiteNotificationsBeforeDeletion
        {
            get
            {
                return mWebApplication.UnusedSiteNotificationsBeforeDeletion;
            }
            set
            {
                mWebApplication.UnusedSiteNotificationsBeforeDeletion = value;
            }
        }

        public void SetDailyUnthrottledPrivilegedOperationWindow(uint hour, uint minute, uint duration)
        {
            mWebApplication.SetDailyUnthrottledPrivilegedOperationWindow(hour, minute, duration);
        }

        public bool BrowserCEIPEnabled
        {
            get
            {
                return mWebApplication.BrowserCEIPEnabled;
            }
            set
            {
                mWebApplication.BrowserCEIPEnabled = value;
            }
        }

        public void UnprovisionGlobally(bool deleteIisWebSite)
        {
            mWebApplication.UnprovisionGlobally(deleteIisWebSite);
        }

        public IAveFeatureCollection Features
        {
            get
            {
                if (mFeatures == null)
                {
                    mFeatures = new AveFeatureCollection(mWebApplication.Features);
                }
                return mFeatures;
            }
        }

        public AveBrowserFileHandling BrowserFileHandling
        {
            get
            {
                return (AveBrowserFileHandling)mWebApplication.BrowserFileHandling;
            }
            set
            {
                mWebApplication.BrowserFileHandling = (SPBrowserFileHandling)value;
            }
        }

        public int RecycleBinRetentionPeriod
        {
            get
            {
                return mWebApplication.RecycleBinRetentionPeriod;
            }
            set
            {
                mWebApplication.RecycleBinRetentionPeriod = value;
            }
        }

        public Uri GetResponseUri(AveUrlZone zone, string path)
        {
            return mWebApplication.GetResponseUri((SPUrlZone)zone, path);
        }

        public bool DocumentConversionsEnabled
        {
            get
            {
                return mWebApplication.DocumentConversionsEnabled;
            }
            set
            {
                mWebApplication.DocumentConversionsEnabled = value;
            }
        }

        public Guid DocumentConversionsLoadBalancerServerId
        {
            get
            {
                return mWebApplication.DocumentConversionsLoadBalancerServerId;
            }
            set
            {
                mWebApplication.DocumentConversionsLoadBalancerServerId = value;
            }
        }

        public string DocumentConversionsLoadBalancerUrl
        {
            get
            {
                return mWebApplication.DocumentConversionsLoadBalancerUrl;
            }
        }

        public string DocumentConversionsSchedule
        {
            get
            {
                return mWebApplication.DocumentConversionsSchedule;
            }
            set
            {
                mWebApplication.DocumentConversionsSchedule = value;
            }
        }

        public bool ShowURLStructure
        {
            get
            {
                return mWebApplication.ShowURLStructure;
            }
            set
            {
                mWebApplication.ShowURLStructure = value;
            }
        }

        public IAvePolicyRoleCollection PolicyRoles
        {
            get
            {
                if (mPolicyRoleCollection == null)
                {
                    SPPolicyRoleCollection policyRoles = mWebApplication.PolicyRoles;
                    if (policyRoles != null)
                    {
                        mPolicyRoleCollection = new AvePolicyRoleCollection(policyRoles);
                    }
                }
                return mPolicyRoleCollection;
            }
        }

        public AveBasePermissions RightsMask
        {
            get
            {
                return (AveBasePermissions)mWebApplication.RightsMask;
            }
            set
            {
                mWebApplication.RightsMask = (SPBasePermissions)value;
            }
        }

        public string OfficialFileName
        {
            get
            {
                return mWebApplication.OfficialFileName;
            }
            set
            {
                mWebApplication.OfficialFileName = value;
            }
        }

        public Uri OfficialFileUrl
        {
            get
            {
                return mWebApplication.OfficialFileUrl;
            }
            set
            {
                mWebApplication.OfficialFileUrl = value;
            }
        }

        public IAveJobDefinitionCollection JobDefinitions
        {
            get
            {
                if (mJobDefinitions == null)
                {
                    mJobDefinitions = new AveJobDefinitionCollection(mWebApplication.JobDefinitions);
                }
                return mJobDefinitions;
            }
        }

        public List<AveUserDetail> GetWebApplicationPolicyUsers(string userSearchInfo, AveAccountSearchFlag flag, bool isExact)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebApplication.GetWebApplicationPolicyUsers"))
            {

                List<AveUserDetail> userDetails = new List<AveUserDetail>();
                foreach (IAvePolicy spp in this.Policies)
                {
                    if (CheckMatch(spp.UserName, userSearchInfo, isExact) || CheckMatch(spp.DisplayName, userSearchInfo, isExact))
                    {
                        AveUserDetail detail = new AveUserDetail();
                        detail.LoginName = spp.UserName;
                        detail.DisplayName = spp.DisplayName;
                        detail.AccountType = AveAccountType.SharePointUser;
                        userDetails.Add(detail);
                    }
                }
                foreach (AveUrlZone urlZone in Enum.GetValues(typeof(AveUrlZone)))
                {
                    foreach (IAvePolicy spp in this.ZonePolicies(urlZone))
                    {
                        if (CheckMatch(spp.UserName, userSearchInfo, isExact) || CheckMatch(spp.DisplayName, userSearchInfo, isExact))
                        {
                            AveUserDetail detail = new AveUserDetail();
                            detail.LoginName = spp.UserName;
                            detail.DisplayName = spp.DisplayName;
                            detail.AccountType = AveAccountType.SharePointUser;
                            userDetails.Add(detail);
                        }
                    }
                }
                return userDetails;

            }

        }

        private bool CheckMatch(string input, string pattern, bool isExact)
        {
            if (isExact)
            {
                bool match = string.Compare(input, pattern, StringComparison.OrdinalIgnoreCase) == 0;
                if (!match)
                {
                    match = Regex.IsMatch(input, "([|]|\\\\)" + Regex.Escape(pattern.Trim()) + "$", RegexOptions.IgnoreCase);
                }
                return match;
            }
            return Regex.Match(input, Regex.Escape(pattern.Trim()), RegexOptions.IgnoreCase).Success;
        }

        public string DefaultServerComment
        {
            get
            {
                return mWebApplication.DefaultServerComment;
            }
        }

        public IAvePeoplePickerSettings PeoplePickerSettings
        {
            get
            {
                if (mPeoplePickerSettings == null)
                {
                    mPeoplePickerSettings = new AvePeoplePickerSettings(mWebApplication.PeoplePickerSettings);
                }
                return mPeoplePickerSettings;
            }
        }

        public void RemoveEventHandlerFromWebapplication(string assemblyFullName)
        {
            string deleteCmd = "delete from EventReceivers where Assembly='{0}'";
            deleteCmd = string.Format(deleteCmd, assemblyFullName);
            foreach (IAveContentDatabase dataBase in this.ContentDatabases)
            {
                try
                {
                    using (AvePoint.Common.AveSqlConnection conn = new AvePoint.Common.AveSqlConnection(dataBase.DatabaseConnectionString))
                    {
                        conn.ExecuteNonQuery(deleteCmd);
                    }
                }
                catch(Exception e)
                {
                    logger.Info("Delete from EventReceivers error, the web application is: {0}, DataBase is {1}. Exception: {2}.", this.Name, dataBase.DisplayName, e.ToString());
                }
            }
        }

        /// <summary>
        /// 此方法的作用是找到最合适创建SiteCollection的ContentDB
        /// 此方法中连续用了两个反射，最好能用其它方法替换一下
        /// </summary>
        /// <param name="destUrl"></param>
        /// <returns></returns>
        public IAveDatabase FindBestContentDatabaseForSiteCreation(string destUrl)
        {
            if (!string.IsNullOrEmpty(destUrl))
            {
                var destUri = new Uri(destUrl);
                var siteCreation = AveAssemblyUtility.InvokeStaticMethod(
                    typeof(SPSiteCreationParameters),
                    "Create",
                    new[] { typeof(SPWebApplication), typeof(Uri), typeof(bool), typeof(SPSiteSubscription), typeof(SPDatabaseServiceInstance) },
                    new object[] { mWebApplication, destUri, false, null, null }) as SPSiteCreationParameters;
                var spContentDataBase = AveAssemblyUtility.InvokeMethod(mWebApplication.ContentDatabases, "FindBestContentDatabaseForSiteCreation", siteCreation) as SPContentDatabase;
                var database = new AveDatabase(spContentDataBase);
                return database;
            }
            return null;
        }

        public IAveWebConfigModificationCollection WebConfigModifications
        {
            get
            {
                if (mWebConfigModifications == null)
                {
                    Collection<SPWebConfigModification> webConfigModifications = mWebApplication.WebConfigModifications;
                    if (webConfigModifications != null)
                    {
                        mWebConfigModifications = new AveWebConfigModificationCollection(mWebApplication.WebConfigModifications);
                    }
                }
                return mWebConfigModifications;
            }
        }

        #endregion

        #region add for SP2013
        public bool SelfServiceCreateIndividualSite
        {
            get { return mWebApplication.SelfServiceCreateIndividualSite; }
            set { mWebApplication.SelfServiceCreateIndividualSite = value; }
        }
        public string SelfServiceCreationParentSiteUrl
        {
            get { return mWebApplication.SelfServiceCreationParentSiteUrl; }
            set { mWebApplication.SelfServiceCreationParentSiteUrl = value; }
        }
        public string SelfServiceCreationQuotaTemplate
        {
            get { return mWebApplication.SelfServiceCreationQuotaTemplate; }
            set { mWebApplication.SelfServiceCreationQuotaTemplate = value; }
        }
        public string SelfServiceSiteCustomFormUrl
        {
            get { return mWebApplication.SelfServiceSiteCustomFormUrl; }
            set { mWebApplication.SelfServiceSiteCustomFormUrl = value; }
        }
        public bool ShowStartASiteMenuItem
        {
            get { return mWebApplication.ShowStartASiteMenuItem; }
            set { mWebApplication.ShowStartASiteMenuItem = value; }
        }
        public IAveUserSettingsProvider UserSettingsProvider
        {
            get
            {
                if (mWebApplication.UserSettingsProvider == null)
                    return null;
                else
                    return new AveUserSettingsProvider(mWebApplication.UserSettingsProvider);
            }
            set
            {
                if (value == null)
                    mWebApplication.UserSettingsProvider = null;
                else
                    mWebApplication.UserSettingsProvider = (value as AveUserSettingsProvider).mUserSettingsProvider;
            }
        }
        public bool AllowAnalyticsCookieForAnonymousUsers
        {
            get { return mWebApplication.AllowAnalyticsCookieForAnonymousUsers; }
            set { mWebApplication.AllowAnalyticsCookieForAnonymousUsers = value; }
        }
        #endregion
    }
}
