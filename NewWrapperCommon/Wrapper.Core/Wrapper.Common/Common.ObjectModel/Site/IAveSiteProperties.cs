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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSiteProperties
    {
        bool AllowDownloadingNonWebViewableFiles { get; set; }
        bool AllowEditing { get; set; }
        bool AllowSelfServiceUpgrade { get; set; }
        double AverageResourceUsage { get; }
        int CompatibilityLevel { get; }
        bool CommentsOnSitePagesDisabled { get; set; }
        double CurrentResourceUsage { get; }
        AveDenyAddAndCustomizePagesStatus DenyAddAndCustomizePages { get; set; }
        AveSPOConditionalAccessPolicyType ConditionalAccessPolicy { get; set; }
        AveAppViewsPolicy DisableAppViews { get; set; }
        AveCompanyWideSharingLinksPolicy DisableCompanyWideSharingLinks { get; set; }
        AveFlowsPolicy DisableFlows { get; set; }
        bool HasHolds { get; }
        Guid HubSiteId { get; }
        bool IsHubSite { get; }
        DateTime LastContentModifiedDate { get; }
        uint Lcid { get; set; }
        string LockIssue { get; }
        string LockState { get; set; }
        string NewUrl { get; set; }
        string Owner { get; set; }
        string OwnerEmail { get; }
        string OwnerName { get; }
        AvePWAEnabledStatus PWAEnabled { get; set; }
        AveRestrictedToRegion RestrictedToRegion { get; set; }
        AveSandboxedCodeActivationCapabilities SandboxedCodeActivationCapability { get; set; }
        bool SetOwnerWithoutUpdatingSecondaryAdmin { get; set; }
        string SharingAllowedDomainList { get; set; }
        string SharingBlockedDomainList { get; set; }
        AveSharingCapabilities SharingCapability { get; set; }
        AveSharingDomainRestrictionModes SharingDomainRestrictionMode { get; set; }
        bool ShowPeoplePickerSuggestionsForGuestUsers { get; set; }
        AveSharingCapabilities SiteDefinedSharingCapability { get; }
        bool SocialBarOnSitePagesDisabled { get; set; }
        string Status { get; }
        long StorageMaximumLevel { get; set; }
        string StorageQuotaType { get; }
        long StorageUsage { get; }
        long StorageWarningLevel { get; set; }
        string Template { get; set; }
        int TimeZoneId { get; }
        string Title { get; set; }
        string Url { get; }
        double UserCodeMaximumLevel { get; set; }
        double UserCodeWarningLevel { get; set; }
        int WebsCount { get; }

        void Update();
    }
}

