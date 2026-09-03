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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Common
{
    class AveSiteProperties : AveClientObject, IAveSiteProperties
    {
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveSiteProperties));

        private IAveRequest mRequest;
        private string mSiteUrl;
        public AveSiteProperties(IAveRequest request, string siteUrl, Dictionary<string, object> prop)
        {
            mRequest = request;
            mSiteUrl = siteUrl;
            base.DataCache.AddPropertyies(prop);
        }

        public bool AllowDownloadingNonWebViewableFiles
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowDownloadingNonWebViewableFiles");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowDownloadingNonWebViewableFiles", value);
            }
        }

        public bool AllowEditing
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowEditing");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowEditing", value);
            }
        }

        public bool AllowSelfServiceUpgrade
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowSelfServiceUpgrade");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowSelfServiceUpgrade", value);
            }
        }

        public double AverageResourceUsage
        {
            get { return base.DataCache.GetProperty<double>("AverageResourceUsage"); }
        }

        public bool CommentsOnSitePagesDisabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CommentsOnSitePagesDisabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("CommentsOnSitePagesDisabled", value);
            }
        }

        public int CompatibilityLevel
        {
            get { return base.DataCache.GetProperty<int>("CompatibilityLevel"); }
        }

        public AveSPOConditionalAccessPolicyType ConditionalAccessPolicy
        {
            get
            {
                return base.DataCache.GetProperty<AveSPOConditionalAccessPolicyType>("ConditionalAccessPolicy");
            }
            set
            {
                base.DataCache.AddChangedProperty("ConditionalAccessPolicy", value);
            }
        }

        public double CurrentResourceUsage
        {
            get { return base.DataCache.GetProperty<double>("CurrentResourceUsage"); }
        }

        public AveDenyAddAndCustomizePagesStatus DenyAddAndCustomizePages
        {
            get
            {
                return base.DataCache.GetProperty<AveDenyAddAndCustomizePagesStatus>("DenyAddAndCustomizePages");
            }
            set
            {
                base.DataCache.AddChangedProperty("DenyAddAndCustomizePages", value);
            }
        }

        public AveAppViewsPolicy DisableAppViews
        {
            get
            {
                return base.DataCache.GetProperty<AveAppViewsPolicy>("DisableAppViews");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisableAppViews", value);
            }
        }

        public AveCompanyWideSharingLinksPolicy DisableCompanyWideSharingLinks
        {
            get
            {
                return base.DataCache.GetProperty<AveCompanyWideSharingLinksPolicy>("DisableCompanyWideSharingLinks");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisableCompanyWideSharingLinks", value);
            }
        }

        public AveFlowsPolicy DisableFlows
        {
            get
            {
                return base.DataCache.GetProperty<AveFlowsPolicy>("DisableFlows");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisableFlows", value);
            }
        }

        public bool HasHolds
        {
            get { return base.DataCache.GetProperty<bool>("HasHolds"); }

        }

        public Guid HubSiteId
        {
            get { return base.DataCache.GetProperty<Guid>("HubSiteId"); }

        }

        public bool IsHubSite
        {
            get { return base.DataCache.GetProperty<bool>("IsHubSite"); }

        }

        public DateTime LastContentModifiedDate
        {
            get { return base.DataCache.GetProperty<DateTime>("LastContentModifiedDate"); }
        }

        public uint Lcid
        {
            get
            {
                return base.DataCache.GetProperty<uint>("Lcid");
            }
            set
            {
                base.DataCache.AddChangedProperty("Lcid", value);
            }
        }

        public string LockIssue
        {
            get { return base.DataCache.GetProperty<string>("LockIssue"); }
        }

        public string LockState
        {
            get
            {
                return base.DataCache.GetProperty<string>("LockState");
            }
            set
            {
                base.DataCache.AddChangedProperty("LockState", value);
            }
        }

        public string NewUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("NewUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("NewUrl", value);
            }
        }

        public string Owner
        {
            get
            {
                return base.DataCache.GetProperty<string>("Owner");
            }
            set
            {
                base.DataCache.AddChangedProperty("Owner", value);
            }
        }

        public string OwnerEmail
        {
            get
            {
                return base.DataCache.GetProperty<string>("OwnerEmail");
            }
        }

        public string OwnerName
        {
            get
            {
                return base.DataCache.GetProperty<string>("OwnerName");
            }
        }

        public AvePWAEnabledStatus PWAEnabled
        {
            get
            {
                return base.DataCache.GetProperty<AvePWAEnabledStatus>("PWAEnabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("PWAEnabled", value);
            }
        }

        public AveRestrictedToRegion RestrictedToRegion
        {
            get
            {
                return base.DataCache.GetProperty<AveRestrictedToRegion>("RestrictedToRegion");
            }
            set
            {
                base.DataCache.AddChangedProperty("RestrictedToRegion", value);
            }
        }

        public AveSandboxedCodeActivationCapabilities SandboxedCodeActivationCapability
        {
            get
            {
                return base.DataCache.GetProperty<AveSandboxedCodeActivationCapabilities>("SandboxedCodeActivationCapability");
            }
            set
            {
                base.DataCache.AddChangedProperty("SandboxedCodeActivationCapability", value);
            }
        }

        public bool SetOwnerWithoutUpdatingSecondaryAdmin
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SetOwnerWithoutUpdatingSecondaryAdmin");
            }
            set
            {
                base.DataCache.AddChangedProperty("SetOwnerWithoutUpdatingSecondaryAdmin", value);
            }
        }

        public string SharingAllowedDomainList
        {
            get
            {
                return base.DataCache.GetProperty<string>("SharingAllowedDomainList");
            }
            set
            {
                base.DataCache.AddChangedProperty("SharingAllowedDomainList", value);
            }
        }

        public string SharingBlockedDomainList
        {
            get
            {
                return base.DataCache.GetProperty<string>("SharingBlockedDomainList");
            }
            set
            {
                base.DataCache.AddChangedProperty("SharingBlockedDomainList", value);
            }
        }

        public AveSharingCapabilities SharingCapability
        {
            get
            {
                return base.DataCache.GetProperty<AveSharingCapabilities>("SharingCapability");
            }
            set
            {
                base.DataCache.AddChangedProperty("SharingCapability", value);
            }
        }

        public AveSharingDomainRestrictionModes SharingDomainRestrictionMode
        {
            get
            {
                return base.DataCache.GetProperty<AveSharingDomainRestrictionModes>("SharingDomainRestrictionMode");
            }
            set
            {
                base.DataCache.AddChangedProperty("SharingDomainRestrictionMode", value);
            }
        }

        public bool ShowPeoplePickerSuggestionsForGuestUsers
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowPeoplePickerSuggestionsForGuestUsers");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowPeoplePickerSuggestionsForGuestUsers", value);
            }
        }

        public AveSharingCapabilities SiteDefinedSharingCapability
        {
            get
            {
                return base.DataCache.GetProperty<AveSharingCapabilities>("SiteDefinedSharingCapability");
            }
        }

        public bool SocialBarOnSitePagesDisabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SocialBarOnSitePagesDisabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("SocialBarOnSitePagesDisabled", value);
            }
        }

        public string Status
        {
            get { return base.DataCache.GetProperty<string>("Status"); }
        }

        public long StorageMaximumLevel
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageMaximumLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("StorageMaximumLevel", value);
            }
        }

        public string StorageQuotaType
        {
            get
            {
                return base.DataCache.GetProperty<string>("StorageQuotaType");
            }
        }

        public long StorageUsage
        {
            get { return base.DataCache.GetProperty<long>("StorageUsage"); }
        }

        public long StorageWarningLevel
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageWarningLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("StorageWarningLevel", value);
            }
        }

        public string Template
        {
            get
            {
                return base.DataCache.GetProperty<string>("Template");
            }
            set
            {
                base.DataCache.AddChangedProperty("Template", value);
            }
        }

        public int TimeZoneId
        {
            get { return base.DataCache.GetProperty<int>("TimeZoneId"); }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                base.DataCache.AddChangedProperty("Title", value);
            }
        }

        public string Url
        {
            get { return base.DataCache.GetProperty<string>("Url"); }
        }

        public double UserCodeMaximumLevel
        {
            get
            {
                return base.DataCache.GetProperty<double>("UserCodeMaximumLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("UserCodeMaximumLevel", value);
            }
        }

        public double UserCodeWarningLevel
        {
            get
            {
                return base.DataCache.GetProperty<double>("UserCodeWarningLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("UserCodeWarningLevel", value);
            }
        }

        public int WebsCount
        {
            get { return base.DataCache.GetProperty<int>("WebsCount"); }
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                (mRequest ).UpdateSiteBasicPropertiesByUrl(mSiteUrl, base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(base.DataCache.ChangedProperties);
            }
        }
    }
}