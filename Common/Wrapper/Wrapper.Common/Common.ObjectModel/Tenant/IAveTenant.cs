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


using Microsoft.SharePoint.Client;
using System.Collections.Generic;

namespace AvePoint.Wrapper.Common
{
    public interface IAveTenant
    {
        string CompatibilityRange { get; set; }
        bool ExternalServicesEnabled { get; set; }
        string NoAccessRedirectUrl { get; set; }
        double ResourceQuota { get; }
        double ResourceQuotaAllocated { get; }
        long StorageQuota { get; }
        long StorageQuotaAllocated { get; }
        //string SPVersion { get; }
        IAvePrefixCollection Prefixes { get; }
        IAveLanguageCollection InstalledLanguages { get; }

        Dictionary<string,object> CreateSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota);
        bool SetAdmin(string url, string admin);
        List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl);
        int GetSiteCollectionsCount(string tenantAdminSiteUrl);
        int GetOneDriveCount(List<string> usernames);
        void DeleteSite(string siteUrl);
        void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota);

        IAveSiteProperties GetSitePropertiesByUrl(string siteUrl);
        IAveDeletedSiteProperties GetDeletedSitePropertiesByUrl(string siteUrl);
        SiteExistence SiteExistsAnywhere(string siteUrl);

        /// <summary>
        /// tenant下所有站点已使用的Storage
        /// </summary>
        long StorageUsage { get; }

        AveSharingCapabilities SharingCapability { get; set; }

        string SharingAllowedDomainList { get; set; }

        string SharingBlockedDomainList { get; set; }

        AveSharingDomainRestrictionModes SharingDomainRestrictionMode { get; set; }

        void RemoveSite(string siteUrl);

        void DeleteSiteImmediately(string siteUrl);

        void RemoveDeletedSite(string siteUrl);

        //string Id { get; }

        bool ShowEveryoneClaim { get; set; }

        bool ShowEveryoneExceptExternalUsersClaim { get; set; }

        bool ShowAllUsersClaim { get; set; }

        void UpdateProperties();

        List<IAveTenantMultiGeoLocationInfo> GetTenantGeoLocationinfo();

        void UnlockSensitivityLabelEncryptedFile(string fileUrl, string justificationText);

        bool TryGetAdminUrlForMultiGeoTenant(string siteUrl, out string adminUrl);
    }
}
