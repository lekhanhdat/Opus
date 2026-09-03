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
    public interface IAveTenant : IDisposable
    {
        string CompatibilityRange { get; set; }
        bool ExternalServicesEnabled { get; set; }
        string NoAccessRedirectUrl { get; set; }
        double ResourceQuota { get; }
        double ResourceQuotaAllocated { get; }
        long StorageQuota { get; }
        long StorageQuotaAllocated { get; }
        string SPVersion { get; }
        IAvePrefixCollection Prefixes { get; }
        IAveLanguageCollection InstalledLanguages { get; }
        AveSharingCapabilities SharingCapability { get; set; }
        string CreateSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota);
        void DeleteSite(string siteUrl);
        void DeleteSiteToRecylebin(string siteUrl);
        bool SetAdmin(string url, string admin);
        List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl);
        List<Dictionary<string, object>> GetGroupSiteCollectionsList(string tenantAdminSiteUrl);
        List<Dictionary<string, object>> GetAllSiteCollectionsList(string tenantAdminSiteUrl, bool inlcudeOneDriveSite, List<string> excludeTempaltes);
        List<Dictionary<string, object>> GetOneDriveSiteCollectionsList(string tenantAdminSiteUrl);
        IAveSiteProperties GetSitePropertiesByUrl(string siteUrl);
        int GetSiteCollectionsCount(string tenantAdminSiteUrl);
        int GetOneDriveCount(List<string> usernames);
        void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota);
        SiteStatus GetSiteStatus(string siteUrl);

        void ApplySiteDesign(string webUrl, Guid siteDesignId);

        /// <summary>
        /// tenant下所有站点已使用的Storage
        /// </summary>
        long StorageUsage { get; }
    }

    public enum SiteStatus
    {
        Normal,
        InRecycleBin,
        Deleted
    }
}
