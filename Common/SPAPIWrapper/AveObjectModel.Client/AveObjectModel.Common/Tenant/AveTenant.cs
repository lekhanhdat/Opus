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


using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveTenant : AveClientObject, IAveTenant
    {
        private IAveRequest mRequest;
        //private string mSPVersion;

        public AveTenant(IAveRequest request)
        {
            mRequest = request;
            Dictionary<string, object> tenantProp = mRequest.GetManagedSitecollectionData();
            base.DataCache.AddPropertyies(tenantProp);
        }
        public AveTenant(string adminUrl, AveBPOSAccountInfo userAccountInfo, bool includeProperties = false) //for create operation
        {
            AveRequestInterceptor request = new AveRequestInterceptor(adminUrl, userAccountInfo);
            mRequest = request.Proxy;
            //mSPVersion = request.SPVersion;
            //添加获取tenant本身属性
            Dictionary<string, object> tenantProp = mRequest.GetTenant(includeProperties);
            base.DataCache.AddPropertyies(tenantProp);
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

        //public string SPVersion
        //{
        //    get
        //    {
        //        return mSPVersion;
        //    }
        //}

        public IAvePrefixCollection Prefixes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Prefixes") && base.DataCache.IsPropertyAvailable("Prefixes" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> prefixsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Prefixes" + AveObjectModelConstant.ObjectPropertySuffix);
                    AvePrefixCollection prefixs = new AvePrefixCollection(prefixsProperties);
                    base.DataCache.AddProperty("Prefixes",prefixs);
                    return prefixs;
                }
                return base.DataCache.GetProperty<IAvePrefixCollection>("Prefixes");
            }
        }

        public IAveLanguageCollection InstalledLanguages
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> tenantProp = mRequest.GetTenantInstalledLanguages(StorageQuota - StorageQuotaAllocated, ResourceQuota - ResourceQuotaAllocated);
                    base.DataCache.AddPropertyies(tenantProp);
                }
                if (base.DataCache.IsPropertyNotLoaded("InstalledLanguages") && base.DataCache.IsPropertyAvailable("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> installLanuagesProperties = base.DataCache.GetProperty<Dictionary<string, object>>("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveLanguageCollection languages = new AveLanguageCollection(installLanuagesProperties);
                    base.DataCache.AddProperty("InstalledLanguages",languages);
                    return languages;
                }
                return base.DataCache.GetProperty<IAveLanguageCollection>("InstalledLanguages");
            }
        }

        /// <summary>
        /// Create site collection for sharepoint online
        /// </summary>
        /// <returns>string.Empty if site collection is successfully created, otherwise, error message</returns>
        public Dictionary<string, object> CreateSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            return mRequest.AddSite(compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
        }

        public bool SetAdmin(string url, string admin)
        {
            return mRequest.AddSiteAdmin(admin, url);
        }

        public List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
        {
            return mRequest.GetManagedSiteCollectionsList(tenantAdminSiteUrl);
        }

        public int GetSiteCollectionsCount(string tenantAdminSiteUrl)
        {
            return mRequest.GetSiteCollectionsCount(tenantAdminSiteUrl);
        }

        public int GetOneDriveCount(List<string> usernames)
        {
            return mRequest.GetOneDriveCount(usernames);
        }

        [Obsolete("should use RemoveSite instead this")]
        public void DeleteSite(string siteUrl)
        {
            mRequest.DeleteSite(siteUrl);        
        }

        public void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota)
        {
            mRequest.UpdateSiteUsage(siteUrl, storageQuota, serverResourceQuota);
        }

        public IAveSiteProperties GetSitePropertiesByUrl(string siteUrl)
        {
            Dictionary<string, object> dic = mRequest.GetSitePropertiesByUrl(siteUrl);
            return new AveSiteProperties(mRequest, siteUrl, dic);
        }

        public IAveDeletedSiteProperties GetDeletedSitePropertiesByUrl(string siteUrl)
        {
            Dictionary<string, object> dic = mRequest.GetDeletedSitePropertiesByUrl(siteUrl);
            return new AveDeletedSiteProperties(mRequest, siteUrl, dic);
        }

        public SiteExistence SiteExistsAnywhere(string siteUrl)
        {
            return mRequest.SiteExistsAnywhere(siteUrl);
        }

        /// <summary>
        /// remove site to recycle bin
        /// </summary>
        /// <param name="siteUrl"></param>
        public void RemoveSite(string siteUrl)
        {
            mRequest.RemoveSiteCollection(siteUrl);
        }

        /// <summary>
        /// delete site directly
        /// </summary>
        /// <param name="siteUrl"></param>
        public void DeleteSiteImmediately(string siteUrl)
        {
            mRequest.DeleteSiteCollectionImmediately(siteUrl);
        }

        /// <summary>
        /// delete site from recycle bin
        /// </summary>
        /// <param name="siteUrl"></param>
        public void RemoveDeletedSite(string siteUrl)
        {
            mRequest.RemoveDeletedSiteCollection(siteUrl);
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
                return (AveSharingCapabilities)base.DataCache.GetProperty<int>("SharingCapability");
            }
            set
            {
                base.DataCache.AddChangedProperty("SharingCapability", Convert.ToInt32(value));
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

        //public string Id
        //{
        //    get
        //    {
        //        return mRequest.GetTenantId();
        //    }
        //}

        public bool ShowEveryoneClaim
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowEveryoneClaim");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowEveryoneClaim", value);
            }
        }

        public bool ShowEveryoneExceptExternalUsersClaim
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowEveryoneExceptExternalUsersClaim");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowEveryoneExceptExternalUsersClaim", value);
            }
        }

        public bool ShowAllUsersClaim
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowAllUsersClaim");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowAllUsersClaim", value);
            }
        }

        public void UpdateProperties()
        {
            mRequest.UpdateTenantProperties(base.DataCache.ChangedProperties);
        }

        public List<IAveTenantMultiGeoLocationInfo> GetTenantGeoLocationinfo()
        {
            return (mRequest as IAveRequest).GetTenantGeoLocationinfo();
        }

        public void UnlockSensitivityLabelEncryptedFile(string fileUrl, string justificationText)
        {
            mRequest.UnlockSensitivityLabelEncryptedFile(fileUrl, justificationText);
        }

        public bool TryGetAdminUrlForMultiGeoTenant(string siteUrl, out string adminUrl)
        {
            var geoLocationInfo = GetTenantGeoLocationinfo();
            if (geoLocationInfo != null && geoLocationInfo.Count > 1)
            {
                foreach (var location in geoLocationInfo)
                {
                    if (siteUrl.StartsWith(location.RootSiteUrl) || siteUrl.StartsWith(location.MySiteHostUrl))
                    {
                        adminUrl = location.TenantAdminUrl;
                        return true;
                    }
                }
            }
            adminUrl = null;
            return false;
        }
    }
}
