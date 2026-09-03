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
namespace AvePoint.ObjectModel.ClientOM
{
    using AvePoint.Common.Portal;
    using AvePoint.Wrapper.Common;
    using Microsoft365.Authentication;
    using Microsoft.Online.SharePoint.TenantAdministration;
    using Microsoft.SharePoint.Client;
    using Microsoft.SharePoint.Client.UserProfiles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using ClientFolder = Microsoft.SharePoint.Client.Folder;
    using SPChangeType = Microsoft.SharePoint.Client.ChangeType;
    using AvePoint.GCommon.Utility.TransientFault;

    public partial class AveClientOM2013Request
    {
        public Dictionary<string, object> GetTenant(bool includeProperties)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                context.Load(tenant);
                context.ExecuteQuery();
                AveObjectCopy.GetObjectBasicProperties(data, tenant);
                mLogger.Info($"Begin to retrieve site properties for tenant {mWebUrl}");
                
                if (includeProperties)
                {
                    //SAAS-13525 SP Online 上available的值是storage减去所用站点已使用的值
                    //在这里获取所有站点已使用的值
                    long storageUsage = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    Action<SPOSitePropertiesEnumerable> loadAction = delegate (SPOSitePropertiesEnumerable spe)
                      {
                          spe.Context.Load(spe, p => p.Include(s => s.StorageUsage), p => p.NextStartIndexFromSharePoint,p=>p.NextStartIndex);
                      };
                    try
                    {
                        foreach (var siteProperties in tenant.GetSitePropertiesNew(loadAction, false, true, null, null, null, 0))
                        {
                            storageUsage += siteProperties.StorageUsage;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Retrieve site collections failed.Will try old method later.Error:{0}",ex);
                        storageUsage = 0;
                        foreach (var siteProperties in tenant.GetSitePropertiesOriginal(loadAction,false,0))
                        {
                            storageUsage += siteProperties.StorageUsage;
                        }
                    }
                    watch.Stop();
                    mLogger.Info($"retrieve site properties for tenant {mWebUrl} complete.TimeCost:{watch.Elapsed}");
                    data["StorageUsage"] = storageUsage;
                    
                }
              

            }
            return data;
        }

        public Dictionary<string, object> GetTenantInstalledLanguages(long availableStorageQuota, double availableResourceQuota)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (tokenProvider.TokenType != TokenType.Bearer)
            {
                if (mRequestCommon == null)
                {
                    mRequestCommon = new AveHttpWebRequestCommon2013(mWebUrl, tokenProvider, mInternalServerVersion);
                }
                mRequestCommon.GetManagedSiteCollectionData(data, mWebUrl, availableStorageQuota, availableStorageQuota);
            }
            else
            {
                GenerateManagedSiteCollectionData(data);
            }
            return data;
        }

        public Dictionary<string, object> GetManagedSitecollectionData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                context.Load(tenant);
                context.ExecuteQuery();
                AveObjectCopy.GetObjectBasicProperties(data, tenant);
                data["StorageUsage"] = tenant.StorageQuotaAllocated;
                if (tokenProvider.TokenType == TokenType.Bearer)
                {
                    GenerateManagedSiteCollectionData(data);
                }
                else
                {
                    mRequestCommon.GetManagedSiteCollectionData(data, mWebUrl, tenant.StorageQuota - tenant.StorageQuotaAllocated, tenant.ResourceQuota - tenant.ResourceQuotaAllocated);
                }
            }
            return data;
        }

        public void UnlockSensitivityLabelEncryptedFile(string fileUrl, string justificationText)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                context.Load(tenant);
                tenant.UnlockSensitivityLabelEncryptedFile(fileUrl, justificationText);
                context.ExecuteQuery();
            }
        }

        /// <summary>
        /// 由于APPToken不支持httprequest,当前这部分数据写死，如果以后有API支持了，再替换成API
        /// </summary>
        /// <param name="managedData"></param>
        private void GenerateManagedSiteCollectionData(Dictionary<string, object> managedData)
        {
            #region Add Lanauages

            var languageList = new List<IDictionary<string, object>> {
                new Dictionary<string, object> { { "DisplayName" , "Arabic" }, { "LCID", 1025 } },
                new Dictionary<string, object> { { "DisplayName", "Azerbaijani" }, { "LCID", 1068 }},
                new Dictionary<string, object> { { "DisplayName", "Basque" }, { "LCID", 1069 }},
                new Dictionary<string, object> { { "DisplayName", "Bosnian (Latin)" }, { "LCID", 5146 }},
                new Dictionary<string, object> { { "DisplayName", "Bulgarian" }, { "LCID", 1026 }},
                new Dictionary<string, object> { { "DisplayName", "Catalan" }, { "LCID", 1027 }},
                new Dictionary<string, object> { { "DisplayName", "Chinese (Simplified)" }, { "LCID", 2052 } },
                new Dictionary<string, object> { { "DisplayName", "Chinese (Traditional)" }, { "LCID", 1028 } },
                new Dictionary<string, object> { { "DisplayName", "Croatian" }, { "LCID", 1050 } },
                new Dictionary<string, object> { { "DisplayName", "Czech" }, { "LCID", 1029 } },
                new Dictionary<string, object> { { "DisplayName", "Danish" }, { "LCID", 1030 } },
                new Dictionary<string, object> { { "DisplayName", "Dari" }, { "LCID", 1164 } },
                new Dictionary<string, object> { { "DisplayName", "Dutch" }, { "LCID", 1043 } },
                new Dictionary<string, object> { { "DisplayName", "English" }, { "LCID", 1033 } },
                new Dictionary<string, object> { { "DisplayName", "Estonian" }, { "LCID", 1061 } },
                new Dictionary<string, object> { { "DisplayName", "Finnish" }, { "LCID", 1035 }  },
                new Dictionary<string, object> { { "DisplayName", "French" }, { "LCID", 1036 } },
                new Dictionary<string, object> { { "DisplayName", "Galician" }, { "LCID", 1110 }},
                new Dictionary<string, object> { { "DisplayName", "German" }, { "LCID", 1031 } },
                new Dictionary<string, object> { { "DisplayName", "Greek" }, { "LCID", 1032 } },
                new Dictionary<string, object> { { "DisplayName", "Hebrew" }, { "LCID", 1037 }},
                new Dictionary<string, object> { { "DisplayName", "Hindi" }, { "LCID", 1081 } },
                new Dictionary<string, object> { { "DisplayName", "Hungarian" }, { "LCID", 1038 } },
                new Dictionary<string, object> { { "DisplayName", "Indonesian" }, { "LCID", 1057 }  },
                new Dictionary<string, object> { { "DisplayName", "Irish" }, { "LCID", 2108 } },
                new Dictionary<string, object> { { "DisplayName", "Italian" }, { "LCID", 1040 } },
                new Dictionary<string, object> { { "DisplayName", "Japanese" }, { "LCID", 1041 }},
                new Dictionary<string, object> { { "DisplayName", "Kazakh" }, { "LCID", 1087 }},
                new Dictionary<string, object> { { "DisplayName", "Korean" }, { "LCID", 1042 }  },
                new Dictionary<string, object> { { "DisplayName", "Latvian" }, { "LCID", 1062 }  },
                new Dictionary<string, object> { { "DisplayName", "Lithuanian" }, { "LCID", 1063 }},
                new Dictionary<string, object> { { "DisplayName", "Macedonian" }, { "LCID", 1071 } },
                new Dictionary<string, object> { { "DisplayName", "Malay" }, { "LCID", 1086 } },
                new Dictionary<string, object> { { "DisplayName", "Norwegian (Bokmål)" }, { "LCID", 1044 } },
                new Dictionary<string, object> { { "DisplayName", "Polish" }, { "LCID", 1045 } },
                new Dictionary<string, object> { { "DisplayName", "Portuguese (Brazil)" }, { "LCID", 1046 }},
                new Dictionary<string, object> { { "DisplayName", "Portuguese (Portugal)" }, { "LCID", 2070 } },
                new Dictionary<string, object> { { "DisplayName", "Romanian" }, { "LCID", 1048 } },
                new Dictionary<string, object> { { "DisplayName", "Russian" }, { "LCID", 1049 } },
                new Dictionary<string, object> { { "DisplayName", "Serbian (Cyrillic, Serbia)" }, { "LCID", 10266 }},
                new Dictionary<string, object> { { "DisplayName", "Serbian (Latin, Serbia)" }, { "LCID", 9242 } },
                new Dictionary<string, object> { { "DisplayName", "Slovak" }, { "LCID", 1051 } },
                new Dictionary<string, object> { { "DisplayName", "Slovenian" }, { "LCID", 1060 }},
                new Dictionary<string, object> { { "DisplayName", "Spanish" }, { "LCID", 3082 }},
                new Dictionary<string, object> { { "DisplayName", "Swedish" }, { "LCID", 1053 }},
                new Dictionary<string, object> { { "DisplayName", "Thai" }, { "LCID", 1054 } },
                new Dictionary<string, object> { { "DisplayName", "Turkish" }, { "LCID", 1055 } },
                new Dictionary<string, object> { { "DisplayName", "Ukrainian" }, { "LCID", 1058 } },
                new Dictionary<string, object> { { "DisplayName", "Vietnamese" }, { "LCID", 1066 }},
                new Dictionary<string, object> { { "DisplayName", "Welsh" }, { "LCID", 1106 } },};

            Dictionary<string, object> languages = new Dictionary<string, object>();
            languages.AddChildren(languageList);
            managedData["InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix] = languages;
            #endregion

            #region  Add Prefixes
            var prefixList = new List<IDictionary<string, object>> { new Dictionary<string, object> { { "Name", "/sites/" } }, new Dictionary<string, object> { { "Name", "/teams/" } } };
            Dictionary<string, object> prefixs = new Dictionary<string, object>();
            prefixs.AddChildren(prefixList);
            managedData["Prefixes" + AveObjectModelConstant.ObjectPropertySuffix] = prefixs;
            #endregion
        }

        public bool GetDenyAddAndCustomizePagesStatus()
        {
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);
                using (AveClientContext context = InitClientObject(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                    context.Load(sp, p => p.DenyAddAndCustomizePages);
                    context.ExecuteQuery();
                    return sp.DenyAddAndCustomizePages == DenyAddAndCustomizePagesStatus.Enabled;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("get site custom script info error . Url : {0}  Error : {1}", tenantSiteUrl, ex.ToString());
            }
            return false;
        }

        public bool AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled()
        {
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);
                using (AveClientContext context = InitClientObject(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                    context.Load(sp, p => p.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled);
                    context.ExecuteQuery();
                    return sp.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("get site custom script info error . Url : {0}  Error : {1}", tenantSiteUrl, ex.ToString());
            }
            return false;
        }

        public void SetDenyAddAndCustomizePagesStatus(bool enable)
        {
            var tenantSiteUrl = string.Empty;
            tenantSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);
            using (AveClientContext context = InitClientObject(tenantSiteUrl))
            {
                Tenant tenant = new Tenant(context);
                SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                context.Load(sp, p => p.DenyAddAndCustomizePages);
                context.ExecuteQuery();
                sp.DenyAddAndCustomizePages = enable ? DenyAddAndCustomizePagesStatus.Enabled : DenyAddAndCustomizePagesStatus.Disabled;
                sp.Update();
                context.ExecuteQuery();
            }
        }

        public bool GetSiteHasHolds()
        {
            bool enableEDiscoveryHold = false;
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);
                using (AveClientContext context = InitClientObject(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                    context.Load(sp, p => p.HasHolds);
                    context.ExecuteQuery();
                    enableEDiscoveryHold = sp.HasHolds;
                    mLogger.Info($"GetSiteHasHolds.mWebUrl:{mWebUrl}.Enable Hold:{enableEDiscoveryHold}.");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("GetSiteHasHolds error . Url : {0}  Error : {1}", tenantSiteUrl, ex.ToString());
            }
            return enableEDiscoveryHold;
        }
        public bool CheckSiteIsLocked()
        {
            bool siteIsLocked = false;
            string siteLockState = string.Empty;
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);
                using (AveClientContext context = InitClientObject(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                    context.Load(sp, p => p.LockState);
                    context.ExecuteQuery();
                    if (sp != null && !string.IsNullOrEmpty(sp.LockState))
                    {
                        siteLockState = sp.LockState;
                        if (sp.LockState.EqualIgnoreCase("ReadOnly") || sp.LockState.EqualIgnoreCase("NoAccess"))
                        {
                            siteIsLocked = true;
                        }
                    }
                }
                mLogger.Info($"CheckSiteIsLocked finished.Url:{mWebUrl} siteLockState:{siteLockState}.siteIsLocked:{siteIsLocked}.");
            }
            catch (Exception ex)
            {
                mLogger.Error($"CheckSiteIsLocked error.Url:{mWebUrl} Error:{ex}.");
            }
            return siteIsLocked;
        }

        public void RemoveSiteLockedState()
        {
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);
                using (AveClientContext context = InitClientObject(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                    context.Load(sp, p => p.LockState);
                    context.ExecuteQuery();
                    if (sp != null && !string.IsNullOrEmpty(sp.LockState))
                    {
                        if (sp.LockState.EqualIgnoreCase("ReadOnly") || sp.LockState.EqualIgnoreCase("NoAccess"))
                        {
                            sp.LockState = "Unlock";
                            sp.Update();
                            context.ExecuteQuery();
                            mLogger.Info($"Success remove lock site finished.Url:{mWebUrl}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"RemoveSiteLockedState error.Url:{mWebUrl} Error:{ex}.");
                throw;
            }
        }

        public Dictionary<string, object> GetQuota()
        {
            string adminSiteUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mUserAccountInfo, mWebUrl);

            var geoLocationInfo = GetTenantGeoLocationinfo(adminSiteUrl);
            if (geoLocationInfo != null && geoLocationInfo.Count > 1)
            {
                foreach (var location in geoLocationInfo)
                {
                    if (mWebUrl.StartsWith(location.RootSiteUrl) || mWebUrl.StartsWith(location.MySiteHostUrl))
                    {
                        adminSiteUrl = location.TenantAdminUrl;
                        mLogger.Info($"GetQuota.O365 Admin New Url is : {adminSiteUrl}.SiteUrl:{mWebUrl}.");
                        break;
                    }
                }
            }
            using (AveClientContext context = InitClientObject(adminSiteUrl))
            {
                Dictionary<string, object> quotaProperties = new Dictionary<string, object>();
                Tenant tenant = new Tenant(context);
                SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                sp.Retrieve(SitePropertiesPropertyNames.StorageMaximumLevel,
                    SitePropertiesPropertyNames.StorageWarningLevel,
                    SitePropertiesPropertyNames.UserCodeMaximumLevel,
                    SitePropertiesPropertyNames.UserCodeWarningLevel);
                context.ExecuteQuery();
                quotaProperties.Add("StorageMaximumLevel", sp.StorageMaximumLevel);
                quotaProperties.Add("StorageWarningLevel", sp.StorageWarningLevel);
                quotaProperties.Add("UserCodeMaximumLevel", sp.UserCodeMaximumLevel);
                quotaProperties.Add("UserCodeWarningLevel", sp.UserCodeWarningLevel);
                return quotaProperties;
            }
        }

        public List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames)
        {
            List<Dictionary<string, object>> skyDriveProInfos = new List<Dictionary<string, object>>();
            using (AveClientContext context = CreateContext())
            {
                PeopleManager pm = new PeopleManager(context);
                Dictionary<string, PersonProperties> props = new Dictionary<string, PersonProperties>();
                int batchSize = 250;
                foreach (string username in usernames)
                {
                    PersonProperties prop = pm.GetPropertiesFor(string.Format("i:0#.f|membership|{0}", username));
                    context.Load(prop, p => p.PersonalUrl);
                    props.Add(username, prop);
                    if (props.Count >= batchSize && props.Count % batchSize == 0)
                    {
                        context.ExecuteQuery();
                    }
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }

                foreach (KeyValuePair<string, PersonProperties> prop in props)
                {
                    skyDriveProInfos.Add(AssembleSkyDriveProProperties(prop.Value, prop.Key));
                }

                return skyDriveProInfos;
            }
        }

        public int GetOneDriveCount(List<string> usernames)
        {
            using (AveClientContext context = CreateContext())
            {
                int oneDriveCount = 0;
                PeopleManager pm = new PeopleManager(context);
                Dictionary<string, PersonProperties> props = new Dictionary<string, PersonProperties>();
                int batchSize = 250;
                foreach (string username in usernames)
                {
                    PersonProperties prop = pm.GetPropertiesFor(string.Format("i:0#.f|membership|{0}", username));
                    context.Load(prop, p => p.PersonalUrl);
                    props.Add(username, prop);
                    if (props.Count >= batchSize && props.Count % batchSize == 0)
                    {
                        context.ExecuteQuery();
                    }
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }

                foreach (KeyValuePair<string, PersonProperties> prop in props)
                {
                    Dictionary<string, object> oneDriveInfo = AssembleSkyDriveProProperties(prop.Value, prop.Key);
                    if (!string.IsNullOrEmpty(oneDriveInfo["PersonalSpace"].ToString()))
                    {
                        oneDriveCount++;
                    }
                }
                return oneDriveCount;
            }
        }

        public Dictionary<string, object> LoadMySiteInfo()
        {
            using (AveClientContext context = CreateContext())
            {
                PeopleManager pm = new PeopleManager(context);
                PersonProperties prop = pm.GetMyProperties();
                context.Load(prop);
                context.ExecuteQuery();

                return AssembleSkyDriveProProperties(prop, prop.ServerObjectIsNull.Value ? string.Empty : prop.UserProfileProperties["UserName"]);
            }
        }

        private Dictionary<string, object> AssembleSkyDriveProProperties(PersonProperties prop, string username = null)
        {
            Dictionary<string, object> skyDriveProp = new Dictionary<string, object>();
            bool isUsernameExists = prop.ServerObjectIsNull.HasValue && prop.ServerObjectIsNull == false;
            skyDriveProp["Exists"] = isUsernameExists;
            skyDriveProp["PersonalUrl"] = isUsernameExists ? prop.PersonalUrl : string.Empty;
            if (isUsernameExists)
            {
                Uri personalUrl = new Uri(prop.PersonalUrl, UriKind.RelativeOrAbsolute);

                if ((personalUrl.IsAbsoluteUri
                    && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("Person.aspx", StringComparison.OrdinalIgnoreCase)
                    && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("PersonImmersive.aspx", StringComparison.OrdinalIgnoreCase)
                    ))
                {
                    skyDriveProp["PersonalSpace"] = prop.PersonalUrl;
                }
                else
                {
                    skyDriveProp["PersonalSpace"] = string.Empty;
                }
            }
            else
            {
                skyDriveProp["PersonalSpace"] = string.Empty;
            }
            skyDriveProp["UserName"] = username;
            skyDriveProp["Version"] = prop.Context.ServerLibraryVersion.ToString();
            return skyDriveProp;
        }

        public List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
        {
            List<Dictionary<string, object>> managedSiteCollections = new List<Dictionary<string, object>>();
            using (AveClientContext context = InitClientObject(tenantAdminSiteUrl))     //mTokenProvider should be the cookieContainer we get from tenant admin site
            {
                Tenant tenant = new Tenant(context);

                Stopwatch watch = Stopwatch.StartNew();
                Action<SPOSitePropertiesEnumerable> loadAction = delegate (SPOSitePropertiesEnumerable spe)
                {
                    spe.Context.Load(spe, spe1 => spe1.Include(s => s.Url, s => s.CompatibilityLevel, s => s.Lcid, s => s.Template), spe1 => spe.NextStartIndexFromSharePoint);
                };
                try
                {
                    foreach (var siteProperties in tenant.GetSitePropertiesNew(loadAction, false, true, null, null, null, 0))
                    {
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        properties.Add("SiteCollectionUrl", siteProperties.Url);
                        properties.Add("CompatibilityLevel", siteProperties.CompatibilityLevel);
                        properties.Add("WebTemplateName", siteProperties.Template);
                        properties.Add("Lcid", siteProperties.Lcid);
                        managedSiteCollections.Add(properties);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("GetManagedSiteCollectionsList failed.Will try old method later.Error:{0}", ex);
                    managedSiteCollections = new List<Dictionary<string, object>>();
                    foreach (var siteProperties in tenant.GetSitePropertiesOriginal(loadAction, true, 0))
                    {
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        properties.Add("SiteCollectionUrl", siteProperties.Url);
                        properties.Add("CompatibilityLevel", siteProperties.CompatibilityLevel);
                        properties.Add("WebTemplateName", siteProperties.Template);
                        properties.Add("Lcid", siteProperties.Lcid);
                        managedSiteCollections.Add(properties);
                    }
                }
                watch.Stop();
                mLogger.Info($"retrieve site properties for tenant {mWebUrl} complete.TimeCost:{watch.Elapsed}");
                return managedSiteCollections;
            }
        }

        public int GetSiteCollectionsCount(string tenantAdminSiteUrl)
        {
            int count = 0;
            using (AveClientContext context = InitClientObject(tenantAdminSiteUrl))     //mTokenProvider should be the cookieContainer we get from tenant admin site
            {
                Tenant tenant = new Tenant(context);
                Stopwatch watch = Stopwatch.StartNew();
                Action<SPOSitePropertiesEnumerable> loadAction = delegate (SPOSitePropertiesEnumerable spe)
                {
                    spe.Context.Load(spe, p => p.Include(s => s.Status), p => p.NextStartIndexFromSharePoint);
                };
                try
                {
                    foreach (var siteProperties in tenant.GetSitePropertiesNew(loadAction, false, false, null, null, null, 0))
                    {
                        count ++;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("GetSiteCollectionsCount failed.Will try old method later.Error:{0}", ex);
                    count = 0;
                    foreach (var siteProperties in tenant.GetSitePropertiesOriginal(loadAction, false, 0))
                    {
                        count++;
                    }
                }
                watch.Stop();
                mLogger.Info($"GetSiteCollectionsCount for tenant {mWebUrl} complete.TimeCost:{watch.Elapsed}");
                return count;
            }
        }

        public List<IAveTenantMultiGeoLocationInfo> GetTenantGeoLocationinfo(string adminUrl = null)
        {
            List<IAveTenantMultiGeoLocationInfo> tenantMultiGeoLocationInfos = new List<IAveTenantMultiGeoLocationInfo>();
            using (var adminContext = CreateContext(string.IsNullOrEmpty(adminUrl) ? mWebUrl : adminUrl))
            {
                try
                {
                    var tenant = new Tenant(adminContext);
                    var instances = tenant.GetTenantInstances();
                    adminContext.Load(tenant, t => t.IsMultiGeo);
                    adminContext.Load(instances);
                    adminContext.ExecuteQuery();
                    if (!tenant.IsMultiGeo || instances == null || instances.Count < 2)
                    {
                        return tenantMultiGeoLocationInfos;
                    }
                    else
                    {
                        mLogger.Info($"This tenant has enabled geo location. AdminUrl: {mWebUrl}");
                        foreach (var instance in instances)
                        {
                            mLogger.Info($"GetTenantGeoLocationinfo.DataLocation:{instance.DataLocation}.IsDefaultDataLocation:{instance.IsDefaultDataLocation}.MySiteHostUrl:{instance.MySiteHostUrl}.RootSiteUrl:{instance.RootSiteUrl}.TenantAdminUrl:{instance.TenantAdminUrl}.");
                            tenantMultiGeoLocationInfos.Add(new IAveTenantMultiGeoLocationInfo()
                            {
                                DataLocation = instance.DataLocation,
                                IsDefaultDataLocation = instance.IsDefaultDataLocation,
                                MySiteHostUrl = instance.MySiteHostUrl,
                                PortalUrl = instance.PortalUrl,
                                RootSiteUrl = instance.RootSiteUrl,
                                TenantAdminUrl = instance.TenantAdminUrl
                            });
                        }
                        return tenantMultiGeoLocationInfos;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error($"Get tenant MultiGeo infomation failed. Admin: {mWebUrl}, error: {e}");
                    return tenantMultiGeoLocationInfos;
                }
            }
        }
    }
}
