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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveTenant : AveClientObject, IAveTenant
    {
        private IAveRequest mRequest;
        private string mSPVersion;
        private string mCAUrl;
        private bool mIsOnlineSite;

        private AveRequestParameter requestParameter;
        private string mSiteUrl;
        private AveBPOSAccountInfo m_UserAccountInfo;
        public AveTenant(IAveRequest request)
        {
            mRequest = request;
            Dictionary<string, object> tenantProp = mRequest.GetManagedSitecollectionData();
            base.DataCache.AddPropertyies(tenantProp);
        }

        public AveTenant(string adminUrl, AveBPOSAccountInfo userAccountInfo, bool isOnline = true, bool needLoadProperties = false) //for create operation
        {
            AveRequestInterceptor request = new AveRequestInterceptor(adminUrl, userAccountInfo);
            mRequest = request.Proxy;
            mSPVersion = request.SPVersion;
            requestParameter = new AveRequestParameter(mRequest, mSPVersion);
            mSiteUrl = adminUrl;
            m_UserAccountInfo = userAccountInfo;
            mIsOnlineSite = isOnline;

            if (!isOnline)
            {
                mCAUrl = adminUrl;
            }
            if (needLoadProperties)
            {
                Dictionary<string, object> tenantProp = mRequest.GetManagedSitecollectionData();
                base.DataCache.AddPropertyies(tenantProp);
            }
        }

        public string CompatibilityRange
        {
            get
            {
                return base.DataCache.GetProperty<string>("CompatibilityRange");
            }
            set
            {
                base.DataCache.AddChangedProperty("CompatibilityRange", value);
            }
        }

        public bool ExternalServicesEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ExternalServicesEnabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("ExternalServicesEnabled", value);
            }
        }

        public string NoAccessRedirectUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("NoAccessRedirectUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("NoAccessRedirectUrl", value);
            }
        }

        public double ResourceQuota
        {
            get
            {
                return base.DataCache.GetProperty<double>("ResourceQuota");
            }
        }

        public double ResourceQuotaAllocated
        {
            get
            {
                return base.DataCache.GetProperty<double>("ResourceQuotaAllocated");
            }
        }

        public long StorageQuota
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageQuota");
            }
        }

        public long StorageQuotaAllocated
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageQuotaAllocated");
            }
        }

        public long StorageUsage
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageUsage");
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

        public string SPVersion
        {
            get
            {
                return mSPVersion;
            }
        }

        public IAvePrefixCollection Prefixes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Prefixes") && base.DataCache.IsPropertyAvailable("Prefixes" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> prefixsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Prefixes" + AveObjectModelConstant.ObjectPropertySuffix);
                    AvePrefixCollection prefixs = new AvePrefixCollection(prefixsProperties);
                    base.DataCache.PropertiesCache["Prefixes"] = prefixs;
                    return prefixs;
                }
                return base.DataCache.GetProperty<IAvePrefixCollection>("Prefixes");
            }
        }

        public IAveLanguageCollection InstalledLanguages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("InstalledLanguages") && base.DataCache.IsPropertyAvailable("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> installLanuagesProperties = base.DataCache.GetProperty<Dictionary<string, object>>("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveLanguageCollection languages = new AveLanguageCollection(installLanuagesProperties);
                    base.DataCache.PropertiesCache["InstalledLanguages"] = languages;
                    return languages;
                }
                return base.DataCache.GetProperty<IAveLanguageCollection>("InstalledLanguages");
            }
        }


        /// <summary>
        /// Create site collection for sharepoint online
        /// </summary>
        /// <returns>string.Empty if site collection is successfully created, otherwise, error message</returns>
        public string CreateSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            return mRequest.AddSite(mCAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
        }

        public void ApplySiteDesign(string webUrl, Guid siteDesignId)
        {
            mRequest.ApplySiteDesign(webUrl, siteDesignId);
        }

        public void DeleteSite(string siteUrl)
        {
            string adminUrl = mCAUrl;
            if (string.IsNullOrEmpty(mCAUrl) && mIsOnlineSite)
            {
                adminUrl = mSiteUrl;
            }
            mRequest.DeleteSite(adminUrl, siteUrl);
        }

        public void DeleteSiteToRecylebin(string siteUrl)
        {
            string adminUrl = mCAUrl;
            if (string.IsNullOrEmpty(mCAUrl) && mIsOnlineSite)
            {
                adminUrl = mSiteUrl;
            }
            mRequest.DeleteSiteToRecylebin(adminUrl, siteUrl);
        }

        public bool SetAdmin(string url, string admin)
        {
            return mRequest.AddSiteAdmin(admin, url);
        }

        public List<Dictionary<string, object>> GetAllSiteCollectionsList(string tenantAdminSiteUrl, bool inlcudeOneDriveSite, List<string> excludeTempaltes)
        {
            return mRequest.GetAllSiteCollectionsList(tenantAdminSiteUrl, inlcudeOneDriveSite, excludeTempaltes);
        }

        public List<Dictionary<string, object>> GetOneDriveSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return mRequest.GetOneDriveSiteCollectionsList(tenantAdminSiteUrl);
        }

        public List<Dictionary<string, object>> GetGroupSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return mRequest.GetGroupSiteCollectionsList(tenantAdminSiteUrl);
        }

        public List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return mRequest.GetManagedSiteCollectionsList(tenantAdminSiteUrl);
        }

        public SiteStatus GetSiteStatus(string siteUrl)
        {
            return mRequest.GetSiteStatus(siteUrl, AveSPCommonUtility.GetTenantAdminSiteUrl);
        }

        public IAveSiteProperties GetSitePropertiesByUrl(string siteUrl)
        {
            Dictionary<string, object> dic = (mRequest ).GetSitePropertiesByUrl(siteUrl);
            return new AveSiteProperties(mRequest, siteUrl, dic);
        }
        public int GetSiteCollectionsCount(string tenantAdminSiteUrl)
        {
            return (mRequest ).GetSiteCollectionsCount(tenantAdminSiteUrl);
        }
        public int GetOneDriveCount(List<string> usernames)
        {
            return (mRequest ).GetOneDriveCount(usernames);
        }
        public void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota)
        {
            (mRequest ).UpdateSiteUsage(siteUrl, storageQuota, serverResourceQuota);
        }

        public void Dispose()
        {
            if (requestParameter != null)
            {
                AveRequestInterceptor.DisposeAvailableRequest(requestParameter, mSiteUrl, m_UserAccountInfo.GetAccountName());
            }
        }
    }
}