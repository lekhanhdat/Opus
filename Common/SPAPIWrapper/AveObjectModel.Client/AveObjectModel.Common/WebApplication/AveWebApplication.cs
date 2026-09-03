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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/02/29", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    class AveWebApplication : AveClientObject, IAveWebApplication
    {
        private IAveRequest mRequest;
        private AveSite mSite;

        public AveWebApplication(IAveRequest request, AveSite site, Dictionary<string, object> prop)
        {
            mRequest = request;
            mSite = site;
            base.DataCache.AddChangedProperties(prop);
        }

        public AveWebApplication(string Url) 
        { 
        }
        #region IAveWebApplication Members

        public IAveAlternateUrlCollection AlternateUrls
        {
            get { throw new NotImplementedException(); }
        }

        public IAveApplicationPool ApplicationPool
        {
            get { throw new NotImplementedException(); }
        }

        public IAveWebApplication Lookup(Uri uri)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.Collection<string> BlockedFileExtensions
        {
            get
            {
                return base.DataCache.GetProperty<Collection<string>>("BlockedFileExtensions");
            }
        }

        public IAveContentDatabaseCollection ContentDatabases
        {
            get { throw new NotImplementedException(); }
        }

        public IAveDocumentConverterCollection DocumentConverters
        {
            get { throw new NotImplementedException(); }
        }

        public bool EventHandlersEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool EmailToNoPermissionWorkflowParticipantsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ExternalWorkflowParticipantsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<AveUrlZone, IAveIisSettings> IisSettings
        {
            get 
            {
                if (base.DataCache.IsPropertyNotLoaded("IisSettings"))
                {
                    Dictionary<AveUrlZone, IAveIisSettings> iisSettings = new Dictionary<AveUrlZone, IAveIisSettings>();
                    Dictionary<int, Dictionary<string, object>> iisSettingValue = base.DataCache.GetProperty<Dictionary<int, Dictionary<string, object>>>("IisSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    foreach (KeyValuePair<int, Dictionary<string, object>> kv in iisSettingValue)
                    {
                        AveIisSettings iisSetting = new AveIisSettings(kv.Value);
                        iisSettings.Add((AveUrlZone)kv.Key, iisSetting);
                    }
                    base.DataCache.AddProperty("IisSettings",iisSettings);
                    return iisSettings;
                }
                return base.DataCache.GetProperty<Dictionary<AveUrlZone, IAveIisSettings>>("IisSettings");
            }
        }

        public bool SelfServiceSiteCreationEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SelfServiceSiteCreationEnabled");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveSiteCollection Sites
        {
            get { throw new NotImplementedException(); }
        }

        public IAvePrefixCollection Prefixes
        {
            get { throw new NotImplementedException(); }
        }

        public Uri GetResponseUri(AveUrlZone urlZone)
        {
            if (base.DataCache.IsPropertyAvailable("GetResponseUri"))
            {                
                Dictionary<int, string> responseUri = base.DataCache.GetProperty<Dictionary<int, string>>("GetResponseUri");
                if (responseUri.ContainsKey((int)urlZone))
                {
                    return new Uri(responseUri[(int)urlZone]);
                }   
            }            
            return null;
        }

        public IAveMobileMessagingAccount OutboundSmsServiceAccount
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOutboundMailServiceInstance OutboundMailServiceInstance
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ScopeExternalConnectionsToSiteSubscriptions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public System.Collections.ObjectModel.Collection<IAveOfficialFileHost> OfficialFileHosts
        {
            get { throw new NotImplementedException(); }
        }

        public int OutboundMailCodePage
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AlertsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AlertsLimited
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int AlertsMaximum
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ChangeLogExpirationEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public TimeSpan ChangeLogRetentionPeriod
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DefaultQuotaTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int DefaultTimeZone
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int MaximumFileSize
        {
            get
            {
                return base.DataCache.GetProperty<int>("MaximumFileSize");
            }
            set
            {
                base.DataCache.AddChangedProperty("MaximumFileSize", value);
            }
        }

        public bool MetaWeblogAuthenticationEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool MetaWeblogEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string OutboundMailReplyToAddress
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string OutboundMailSenderAddress
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string StrOutboundSMTPServer
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PresenceEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool RecycleBinCleanupEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int SecondStageRecycleBinQuota
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SendLoginCredentialsByEmail
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveFormDigestSettings FormDigestSettings
        {
            get { throw new NotImplementedException(); }
        }

        public bool SyndicationEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AllowAccessToWebPartCatalog
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AllowPartToPartCommunication
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool RequireContactForSelfServiceSiteCreation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool RecycleBinEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool MasterPageReferenceEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool UserDefinedWorkflowsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool InheritDataRetrievalSettings
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveDataRetrievalProvider DataRetrievalProvider
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsAdministrationWebApplication
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void UpdateSmsAccount(IAveMobileMessagingAccount account)
        {
            throw new NotImplementedException();
        }

        public void UpdateWorkflowConfigurationSettings()
        {
            throw new NotImplementedException();
        }

        public void UpdateMailSettings(string strOutboundSMTPServer, string strFromAddress, string strReplyToAddress, int nCodePage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAvePersistedObject Members

        public string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAvePersistedObject Parent
        {
            get { throw new NotImplementedException(); }
        }

        public string TypeName
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public IAveConfigurationDatabase ConfigurationDatabase
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Hashtable Properties
        {
            get { throw new NotImplementedException(); }
        }

        public void UnprovisionIisWebSites(bool deleteWebSites, string[] serverComments, string applicationPoolId)
        {
            throw new NotImplementedException();
        }

        public void Update(bool ensure)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, Version> Versions
        {
            get { throw new NotImplementedException(); }
        }

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

        public IAveFarm Farm
        {
            get { throw new NotImplementedException(); }
        }

        public AveObjectStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public IAvePolicyCollection Policies
        {
            get { throw new NotImplementedException(); }
        }

        public IAvePolicyCollection ZonePolicies(AveUrlZone zone)
        {
            throw new NotImplementedException();
        }


        public void Provision()
        {
            throw new NotImplementedException();
        }

        public void Unprovision()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }


        public IAveServiceApplicationProxyGroup ServiceApplicationProxyGroup
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ServiceApplicationProxyGroup"))
                {
                    Dictionary<string, object> serviceAppPG = base.DataCache.GetProperty<Dictionary<string, object>>("ServiceApplicationProxyGroup" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveServiceApplicationProxyGroup serviceAppProxyGroup = new AveServiceApplicationProxyGroup(serviceAppPG);
                    base.DataCache.AddProperty("ServiceApplicationProxyGroup",serviceAppProxyGroup);
                    return serviceAppProxyGroup;
                }
                return base.DataCache.GetProperty<IAveServiceApplicationProxyGroup>("ServiceApplicationProxyGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("ServiceApplicationProxyGroup", value);
            }
        }

        public bool WasCreated
        {
            get { throw new NotImplementedException(); }
        }


        public AveUrlZone? ExternalUrlZone
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool UseClaimsAuthentication
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public Uri GetResponseUri(AveUrlZone zone, string path)
        {
            throw new NotImplementedException();
        }

        public bool BrowserCEIPEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveFeatureCollection Features
        {
            get { throw new NotImplementedException(); }
        }

        public AveBrowserFileHandling BrowserFileHandling
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int RecycleBinRetentionPeriod
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void UnprovisionGlobally(bool deleteIisWebSite)
        {
            throw new NotImplementedException();
        }

        public bool AllowContributorsToEditScriptableParts
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AllowDesigner
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AllowMasterPageEditing
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AllowRevertFromTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AllowOMCodeOverrideThrottleSettings
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AutomaticallyDeleteUnusedSiteCollections
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Exists
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public uint DailyStartUnthrottledPrivilegedOperationsHour
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint DailyStartUnthrottledPrivilegedOperationsMinute
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint DailyUnthrottledPrivilegedOperationsDuration
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveHttpThrottleSettings HttpThrottleSettings
        {
            get { throw new NotImplementedException(); }
        }

        public uint MaxItemsPerThrottledOperation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint MaxItemsPerThrottledOperationOverride
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint MaxItemsPerThrottledOperationWarningLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint MaxQueryLookupFields
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint MaxUniquePermScopesPerList
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SendUnusedSiteCollectionNotifications
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool UnthrottledPrivilegedOperationWindowEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public TimeSpan UnusedSiteNotificationPeriod
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int UnusedSiteNotificationsBeforeDeletion
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetDailyUnthrottledPrivilegedOperationWindow(uint hour, uint minute, uint duration)
        {
            throw new NotImplementedException();
        }


        public bool DocumentConversionsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Guid DocumentConversionsLoadBalancerServerId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DocumentConversionsLoadBalancerUrl
        {
            get { throw new NotImplementedException(); }
        }

        public string DocumentConversionsSchedule
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ShowURLStructure
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public long Version
        {
            get { throw new NotImplementedException(); }
        }       


        public IAvePolicyRoleCollection PolicyRoles
        {
            get { throw new NotImplementedException(); }
        }


        public Guid ID
        {
            get
            {
                //throw new NotImplementedException();
                //获取Web Application的ID
                return base.DataCache.GetProperty<Guid>("Id");
            }
            set
            {
                //throw new NotImplementedException();
                //set Web Application的ID
                base.DataCache.AddChangedProperty("Id", value);
            }
        }


        public AveBasePermissions RightsMask
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public string OfficialFileName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri OfficialFileUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveJobDefinitionCollection JobDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        public long GetWebAppStorageNoStub()
        {
            throw new NotImplementedException();
        }


        public List<AveUserDetail> GetWebApplicationPolicyUsers(string userSearchInfo, AveAccountSearchFlag flag, bool isExact)
        {
            throw new NotImplementedException();
        }

        public bool NeedsUpgrade
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DefaultServerComment
        {
            get { throw new NotImplementedException(); }
        }

        public bool NeedsUpgradeIncludeChildren
        {
            get { throw new NotImplementedException(); }
        }

        public AveTriState IsBackwardsCompatible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAvePeoplePickerSettings PeoplePickerSettings
        {
            get { throw new NotImplementedException(); }
        }

        public void Uncache()
        {
            throw new NotImplementedException();
        }

        public IAveLastUpdateInfo LastUpdateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void RemoveEventHandlerFromWebapplication(string assemblyFullName)
        {
            throw new NotImplementedException();
        }

        public IAveDatabase FindBestContentDatabaseForSiteCreation(string destUrl)
        {
            throw new NotImplementedException();
        }
    }
}
