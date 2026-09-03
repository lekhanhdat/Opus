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
using System.Text;
using System.Collections.ObjectModel;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWebApplication : IAvePersistedUpgradableObject
    {
        bool AllowAccessToWebPartCatalog { get; set; }
        bool AllowContributorsToEditScriptableParts { get; set; }
        bool AllowPartToPartCommunication { get; set; }
        bool AllowDesigner { get; set; }
        bool AllowMasterPageEditing { get; set; }
        bool AllowRevertFromTemplate { get; set; }
        bool AllowOMCodeOverrideThrottleSettings { get; set; }
        bool AutomaticallyDeleteUnusedSiteCollections { get; set; }
        bool Exists { get; }

        IAveAlternateUrlCollection AlternateUrls { get; }
        IAveApplicationPool ApplicationPool { get; }
        Collection<string> BlockedFileExtensions { get; }
        IAveContentDatabaseCollection ContentDatabases { get; }
        IAveDocumentConverterCollection DocumentConverters { get; }
        uint DailyStartUnthrottledPrivilegedOperationsHour { get; set; }
        uint DailyStartUnthrottledPrivilegedOperationsMinute { get; set; }
        uint DailyUnthrottledPrivilegedOperationsDuration { get; set; }
        bool DocumentConversionsEnabled { get; set; }
        Guid DocumentConversionsLoadBalancerServerId { get; set; }
        string DocumentConversionsLoadBalancerUrl { get; }
        string DocumentConversionsSchedule { get; set; }

        bool EventHandlersEnabled { get; set; }
        bool EmailToNoPermissionWorkflowParticipantsEnabled { get; set; }
        bool ExternalWorkflowParticipantsEnabled { get; set; }
        IAveHttpThrottleSettings HttpThrottleSettings { get; }
        Dictionary<AveUrlZone, IAveIisSettings> IisSettings { get; }
        uint MaxItemsPerThrottledOperation { get; set; }
        uint MaxItemsPerThrottledOperationOverride { get; set; }
        uint MaxItemsPerThrottledOperationWarningLevel { get; set; }
        uint MaxQueryLookupFields { get; set; }
        uint MaxUniquePermScopesPerList { get; set; }

        bool ShowURLStructure { get; set; }
        bool SelfServiceSiteCreationEnabled { get; set; }
        IAveSiteCollection Sites { get; }
        IAvePrefixCollection Prefixes { get; }
        Uri GetResponseUri(AveUrlZone zone, string path);
        IAveServiceApplicationProxyGroup ServiceApplicationProxyGroup { get; set; }
        bool SendUnusedSiteCollectionNotifications { get; set; }
        IAveMobileMessagingAccount OutboundSmsServiceAccount { get; }
        IAveOutboundMailServiceInstance OutboundMailServiceInstance { get; set; }
        bool ScopeExternalConnectionsToSiteSubscriptions { get; set; }
        Collection<IAveOfficialFileHost> OfficialFileHosts { get; }
        int OutboundMailCodePage { get; set; }
        bool AlertsEnabled { get; set; }
        bool AlertsLimited { get; set; }
        int AlertsMaximum { get; set; }
        bool ChangeLogExpirationEnabled { get; set; }
        TimeSpan ChangeLogRetentionPeriod { get; set; }
        string DefaultQuotaTemplate { get; set; }
        int DefaultTimeZone { get; set; }
        int MaximumFileSize { get; set; }
        bool MetaWeblogAuthenticationEnabled { get; set; }
        bool MetaWeblogEnabled { get; set; }
        string OutboundMailReplyToAddress { get; set; }
        string OutboundMailSenderAddress { get; set; }
        string StrOutboundSMTPServer { get; set; }
        bool PresenceEnabled { get; set; }
        bool RecycleBinCleanupEnabled { get; set; }
        int SecondStageRecycleBinQuota { get; set; }
        bool SendLoginCredentialsByEmail { get; set; }
        IAveFormDigestSettings FormDigestSettings { get; }
        bool SyndicationEnabled { get; set; }

        bool RequireContactForSelfServiceSiteCreation { get; set; }
        bool RecycleBinEnabled { get; set; }
        bool MasterPageReferenceEnabled { get; set; }
        bool UserDefinedWorkflowsEnabled { get; set; }
        bool UnthrottledPrivilegedOperationWindowEnabled { get; set; }
        TimeSpan UnusedSiteNotificationPeriod { get; set; }
        int UnusedSiteNotificationsBeforeDeletion { get; set; }

        bool InheritDataRetrievalSettings { get; set; }
        IAveDataRetrievalProvider DataRetrievalProvider { get; }
        bool IsAdministrationWebApplication { get; set; }
        AveUrlZone? ExternalUrlZone { get; set; }
        bool UseClaimsAuthentication { get; set; }
        IAvePolicyCollection Policies { get; }
        bool BrowserCEIPEnabled { get; set; }
        IAveFeatureCollection Features { get; }
        AveBrowserFileHandling BrowserFileHandling { get; set; }
        int RecycleBinRetentionPeriod { get; set; }
        AveBasePermissions RightsMask { get; set; }
        IAvePolicyRoleCollection PolicyRoles { get; }
        string OfficialFileName { get; set; }
        Uri OfficialFileUrl { get; set; }
        IAveJobDefinitionCollection JobDefinitions { get; }
        string DefaultServerComment { get; }
        IAvePeoplePickerSettings PeoplePickerSettings { get; }

        void UnprovisionGlobally(bool deleteIisWebSite);
        Uri GetResponseUri(AveUrlZone urlZone);
        IAveWebApplication Lookup(Uri uri);
        void SetDailyUnthrottledPrivilegedOperationWindow(uint hour, uint minute, uint duration);
        void UpdateSmsAccount(IAveMobileMessagingAccount account);
        void UpdateWorkflowConfigurationSettings();
        void UpdateMailSettings(string strOutboundSMTPServer, string strFromAddress, string strReplyToAddress, int nCodePage);
        void UnprovisionIisWebSites(bool deleteWebSites, string[] serverComments, string applicationPoolId);
        IAvePolicyCollection ZonePolicies(AveUrlZone zone);
        List<AveUserDetail> GetWebApplicationPolicyUsers(string userSearchInfo, AveAccountSearchFlag flag, bool isExact);
        void RemoveEventHandlerFromWebapplication(string assemblyFullName);
        IAveDatabase FindBestContentDatabaseForSiteCreation(string destUrl);
    }
}