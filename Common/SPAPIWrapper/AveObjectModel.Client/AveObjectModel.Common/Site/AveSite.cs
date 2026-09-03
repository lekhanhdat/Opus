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
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.WebService;
//using AvePoint.ObjectModel.ClientExtension;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common.Office;
using System.IO;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using AvePoint.Wrapper.Common.Common.ObjectModel.Apps;

namespace AvePoint.ObjectModel.Common
{
    class AveSite : AveClientObject, IAveSite
    {
        private IAveRequest mRequest;
        private AveBPOSAccountInfo mUserAccountInfo;
        private ReadOnlyCollection<IAveThmxTheme> mManagedThemes;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveSite));
        private string mSiteUrl;
        private string mOriginalUrl;
        private AveSiteSerializer m_SiteSerializer;
        private AveSiteSettingSerializer m_SiteSettingSerializer;
        //private AveMetaDataServiceSerializer MetaDataServiceSerializer;
        private AveUserSerializer m_UserSerializer;
        private AveGroupSerializer m_GroupSerializer;
        private AveSiteUsersSerializer m_SiteUsersSerializer;
        private AveFeatureSerializer m_FeatureSerializer;
        private AveMetaDataServiceSerializer m_MetaDataServiceSerializer;
        private AveAPIType m_APIType;
        private string m_SPVersion;
        private bool isAdminCenter;
        //private AveSPServerVersionType m_VersionType;
        private VariationsSettings mVariationsSettings;
        private AveProjectServer mProjectServer;
        public string SPVersion
        {
            get
            {
                return m_SPVersion;
            }
        }

        #region add for SP2013
        private bool m_Archived = false;
        public bool Archived
        {
            get { return m_Archived; }
            set { m_Archived = value; }
        }

        private bool m_ReadOnlyMode = false;
        public bool ReadOnlyMode
        {
            get { return m_ReadOnlyMode; }
            set { m_ReadOnlyMode = value; }
        }
        #endregion


        public AveRequestParameter RequestParameter { get; private set; }
        public AveBPOSAccountInfo UserAccountInfo
        {
            get
            {
                return this.mUserAccountInfo;
            }
        }

        public AveSite(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            mUserAccountInfo = userAccountInfo;
            this.isAdminCenter = siteUrl.TrimEnd('/').EndsWith("-admin.sharepoint.com", StringComparison.OrdinalIgnoreCase) || siteUrl.TrimEnd('/').EndsWith("-admin.sharepoint.cn", StringComparison.OrdinalIgnoreCase); ;
            mSiteUrl = siteUrl;
            InitRequest();
            mSiteUrl = mRequest.Url;
            mOriginalUrl = mRequest.OriginalUrl;
            RequestParameter = new AveRequestParameter(mRequest, userAccountInfo);
            if (!string.IsNullOrEmpty(userAccountInfo.AdminUrl) && string.Equals(mSiteUrl, userAccountInfo.AdminUrl))
            {
                this.isAdminCenter = true;
            }
            Dictionary<string, object> siteProperites = isAdminCenter ? mRequest.GetAdminCenterSite() : mRequest.GetSite();
            base.DataCache.AddPropertyies(siteProperites);
            mVariationsSettings = new VariationsSettings(this);
            m_APIType = mRequest.Kind == AveRequestKind.Extension ? AveAPIType.BPOS_D : AveAPIType.BPOS_S;
        }

        public AveSite(string siteUrl, AveBPOSAccountInfo userAccountInfo, bool isAdminCenter)
        {
            this.isAdminCenter = isAdminCenter;
            mUserAccountInfo = userAccountInfo;
            mSiteUrl = siteUrl;
            InitRequest();
            mSiteUrl = mRequest.Url;
            mOriginalUrl = mRequest.OriginalUrl;
            RequestParameter = new AveRequestParameter(mRequest, userAccountInfo);
            Dictionary<string, object> siteProperites = isAdminCenter ? mRequest.GetAdminCenterSite() : mRequest.GetSite();
            base.DataCache.AddPropertyies(siteProperites);
            mVariationsSettings = new VariationsSettings(this);
            m_APIType = mRequest.Kind == AveRequestKind.Extension ? AveAPIType.BPOS_D : AveAPIType.BPOS_S;
        }

        private void InitRequest()
        {
            //#if PerformaceLog
            AveRequestInterceptor request = new AveRequestInterceptor(mSiteUrl, mUserAccountInfo);
            mRequest = request.Proxy;
            //m_SPVersion = request.SPVersion;
            //m_VersionType = request.VersionType;
            //#else
            //            AveClientRequest request = new AveClientRequest(mSiteUrl, mUserAccountInfo);
            //            mRequest = request.InitRequest();
            //#endif
        }

        internal IAveRequest Request
        {
            get
            {
                return mRequest;
            }
        }

        #region IAveSite Members

        public IAveTaxonomySession AveSPTaxonomySession
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AveSPTaxonomySession"))
                {
                    AveTaxonomySession session = new AveTaxonomySession(this);
                    base.DataCache.AddProperty("AveSPTaxonomySession",session);
                }
                return base.DataCache.GetProperty<IAveTaxonomySession>("AveSPTaxonomySession");
            }
        }

        public void Close()
        {
        }

        public void Delete()
        {
            mRequest.DeleteSite();
        }

        public IAveWebTemplateCollection GetWebTemplates(uint lcid)
        {
            Dictionary<string, object> webTeplateProperties = mRequest.GetWebTemplates(null, lcid, false, "site.getWebTemplates");
            AveWebTemplateCollection templateCollection = new AveWebTemplateCollection(this, mRequest, webTeplateProperties);
            return templateCollection;
        }

        public IAveWeb OpenWeb(Guid webId)
        {
            if (webId == this.RootWeb.ID)
            {
                return this.RootWeb;
            }
            else
            {
                AveWeb web = base.DataCache.GetWeakReferenceObject(webId.ToString()) as AveWeb;
                if (web == null)
                {
                    Dictionary<string, object> webProperties = mRequest.GetWeb(webId);
                    web = new AveWeb(mRequest, this, null, webProperties);
                    base.DataCache.AddWeakReferenceHandler(webId.ToString(), web);
                }
                return web;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webUrl">server relative url of the web</param>
        /// <returns></returns>
        public IAveWeb OpenWeb(string webUrl)
        {
            AveWeb web = base.DataCache.GetWeakReferenceObject("OpenWeb" + webUrl) as AveWeb;
            if (web == null)
            {
                Dictionary<string, object> webProperties = mRequest.GetWeb(webUrl);
                //这里不需要判断exist，应该返回一个exist为false的对象，避免空引用；
                //if ((bool)webProperties["Exists"])
                //{
                web = new AveWeb(mRequest, this, null, webProperties);
                base.DataCache.AddWeakReferenceHandler("OpenWeb" + webUrl, web);
                //}
            }
            return web;
        }

        public IAveWeb OpenWeb()
        {
            string url = string.IsNullOrEmpty(mOriginalUrl) ? mSiteUrl : mOriginalUrl;
            return this.OpenWeb(AveUrlUtility.GetServerRelativeUrl(url));
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> siteProperties = mRequest.UpdateSite(base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(siteProperties);
                //updatasite后需要reload rootweb，因为在updateproperties的时候是不会update"rootweb"的
                base.DataCache.RemoveProperty("RootWeb");
            }
        }

        /// <summary>
        /// Update SharepointDesigner Setting
        /// </summary>
        public void UpdateSpecialProperty()
        {
            Dictionary<string, object> Cache = base.DataCache.ChangedProperties;
            if (Cache.Count > 0)
            {
                mRequest.UpdateSpecialProperty(Cache);
                base.DataCache.UpdateProperties(Cache);
            }
        }

        public bool AllowDesigner
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowDesigner");
            }
            set
            {
                if (!AllowDesigner.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowDesigner", value);
                }
            }
        }
        public bool AllowMasterPageEditing
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowMasterPageEditing");
            }
            set
            {
                if (!AllowMasterPageEditing.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowMasterPageEditing", value);
                }
            }
        }
        public bool AllowRevertFromTemplate
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowRevertFromTemplate");
            }
            set
            {
                if (!AllowRevertFromTemplate.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowRevertFromTemplate", value);
                }
            }
        }

        public IAveWebCollection AllWebs
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllWebs"))
                {
                    Dictionary<string, object> websProp = mRequest.GetAllWebs();
                    AveWebCollection webs = new AveWebCollection(mRequest, this, null, websProp);
                    base.DataCache.AddProperty("AllWebs",webs);
                }
                return base.DataCache.GetProperty<IAveWebCollection>("AllWebs");
            }
        }

        public IAveAudit Audit
        {
            get
            {
                return DataCache.EnsureLoadProperty("Audit",
                    () =>
                    {
                        //Create an empty audit object, init it while it is being used. Now only AuditFlags property is in used.
                        return new AveAudit(mRequest, this, null);
                    });
            }
        }

        public string AuditLogTrimmingCallout
        {
            get
            {
                return base.DataCache.GetProperty<string>("AuditLogTrimmingCallout");
            }
            set
            {
                if (!string.Equals(AuditLogTrimmingCallout, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("AuditLogTrimmingCallout", value);
                }
            }
        }

        public int AuditLogTrimmingRetention
        {
            get
            {
                return base.DataCache.GetProperty<int>("AuditLogTrimmingRetention");
            }
            set
            {
                if (!AuditLogTrimmingRetention.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AuditLogTrimmingRetention", value);
                }
            }
        }

        public IAveContentDatabase ContentDatabase
        {
            get
            {
                return null;
            }
        }

        public bool HostHeaderIsSiteName
        {
            get
            {
                return base.DataCache.GetProperty<bool>("HostHeaderIsSiteName");
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public IAveUser Owner
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Owner") && base.DataCache.IsPropertyAvailable("Owner" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int ownerId = base.DataCache.GetProperty<int>("Owner" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser owner = this.RootWeb.SiteUsers.GetByID(ownerId);
                    base.DataCache.AddProperty("Owner",owner);
                    return owner;
                }
                return base.DataCache.GetProperty<IAveUser>("Owner");
            }
            set
            {
                base.DataCache.AddChangedProperty("Owner", value.ID);
            }
        }

        public IAveUser SecondaryContact
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SecondaryContact") && base.DataCache.IsPropertyAvailable("SecondaryContact" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int secondaryContactID = base.DataCache.GetProperty<int>("SecondaryContact" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (secondaryContactID != 0)
                    {
                        IAveUser secondaryContact = this.RootWeb.SiteUsers.GetByID(secondaryContactID);
                        base.DataCache.AddProperty("SecondaryContact",secondaryContact);
                        return secondaryContact;
                    }
                    return null;
                }
                return base.DataCache.GetProperty<IAveUser>("SecondaryContact");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string PortalName
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("PortalName"))
                {
                    Dictionary<string, object> portalProperties = mRequest.GetSitePortal(mSiteUrl);
                    base.DataCache.AddPropertyies(portalProperties);
                }
                return base.DataCache.GetProperty<string>("PortalName");
            }
            set
            {
                if (!string.Equals(PortalName, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("PortalName", value);
                }
            }
        }

        public string PortalUrl
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("PortalUrl"))
                {
                    Dictionary<string, object> portalProperties = mRequest.GetSitePortal(mSiteUrl);
                    base.DataCache.AddPropertyies(portalProperties);
                }
                return base.DataCache.GetProperty<string>("PortalUrl");
            }
            set
            {
                if (!string.Equals(PortalUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    if (value == null)
                    {
                        base.DataCache.AddChangedProperty("PortalName", null);
                    }
                    base.DataCache.AddChangedProperty("PortalUrl", value);
                }
                //zj server api 不用update修改此属性直接就好使，所以wrapper restore里不用update。按照Sid建议，为了保证业务wrapper的逻辑一致，在此修改
                Update();
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public bool ShowURLStructure
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowUrlStructure");
            }
            set
            {
                if (!ShowURLStructure.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ShowUrlStructure", value);
                }
            }
        }

        public IAveUserSolutionCollection Solutions
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Solutions"))
                {
                    Dictionary<string, object> userSolutionColProperties = mRequest.GetUserSolutions();
                    AveUserSolutionCollection userSolutionCol = null;
                    if (userSolutionColProperties != null)
                    {
                        userSolutionCol = new AveUserSolutionCollection(mRequest, userSolutionColProperties);
                    }
                    base.DataCache.AddProperty("Solutions",userSolutionCol);
                    return userSolutionCol;
                }
                return base.DataCache.GetProperty<IAveUserSolutionCollection>("Solutions");
            }
        }

        public IAveRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RecycleBin"))
                {
                    Dictionary<string, object> recycleBinItemCollection = mRequest.GetRecycleBin();
                    AveRecycleBinItemCollection recycleBinItems = new AveRecycleBinItemCollection(mRequest, this, null, recycleBinItemCollection);
                    base.DataCache.AddProperty("RecycleBin", recycleBinItems);
                }
                return base.DataCache.GetProperty<IAveRecycleBinItemCollection>("RecycleBin");
            }
        }

        public IAveWeb RootWeb
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RootWeb"))
                {
                    Dictionary<string, object> rootWebProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RootWeb" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (!rootWebProperties.ContainsKey("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix) && !isAdminCenter)
                    {
                        throw new Exception(rootWebProperties["LoadRootWebErrorMsg"].ToString());
                    }
                    AveWeb rootWeb = new AveWeb(mRequest, this, null, rootWebProperties);
                    base.DataCache.AddProperty("RootWeb",rootWeb);
                    return rootWeb;
                }
                return base.DataCache.GetProperty<IAveWeb>("RootWeb");
            }
        }

        private readonly object mLoadFeaturesLock = new object();
        public IAveFeatureCollection Features
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Features"))
                {
                    lock (mLoadFeaturesLock)
                    {
                        if (base.DataCache.IsPropertyNotLoaded("Features"))
                        {
                            Dictionary<string, object> featureCollection = mRequest.GetFeatures(null, "site.features");
                            AveFeatureCollection features = new AveFeatureCollection(this.RootWeb as AveWeb, mRequest, featureCollection, "site.features");
                            base.DataCache.AddProperty("Features", features);
                            return features;
                        }
                    }
                }
                return base.DataCache.GetProperty<IAveFeatureCollection>("Features");
            }
        }

        public bool SyndicationEnabled
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SyndicationEnabled"))
                {
                    bool syndicationEnabled = mRequest.GetSiteRssSetting();
                    base.DataCache.AddProperty("SyndicationEnabled",syndicationEnabled);
                }
                return base.DataCache.GetProperty<bool>("SyndicationEnabled");
            }
            set
            {
                if (!SyndicationEnabled.Equals(value))
                {
                    mRequest.UpdateSiteRssSetting(value);
                    base.DataCache.AddChangedProperty("SyndicationEnabled", value);
                }
            }
        }

        public string LockIssue
        {
            get
            {
                return base.DataCache.GetProperty<string>("LockIssue");
            }
            set
            {
                if (!string.Equals(LockIssue, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("LockIssue", value);
                }
            }
        }

        public bool DenyAddAndCustomizePagesStatus
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DenyAddAndCustomizePagesStatus"))
                {
                    bool status = mRequest.GetDenyAddAndCustomizePagesStatus();
                    base.DataCache.AddProperty("DenyAddAndCustomizePagesStatus",status);
                    return status;
                }
                return base.DataCache.GetProperty<bool>("DenyAddAndCustomizePagesStatus");
            }
            set
            {
                if (!DenyAddAndCustomizePagesStatus.Equals(value))
                {
                    mRequest.SetDenyAddAndCustomizePagesStatus(value);
                    base.DataCache.AddChangedProperty("DenyAddAndCustomizePagesStatus", value);
                }
            }
        }

        public bool AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled"))
                {
                    bool status = mRequest.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled();
                    base.DataCache.AddProperty("AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled", status);
                    return status;
                }
                return base.DataCache.GetProperty<bool>("AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled");
            }
        }

        public IAveQuota Quota
        {
            get
            {
                AveQuota quota = base.DataCache.GetWeakReferenceObject("Quota") as AveQuota;
                if (quota == null)
                {
                    Dictionary<string, object> quotaProperties = mRequest.GetQuota();

                    quota = new AveQuota(mRequest, quotaProperties);
                    base.DataCache.AddWeakReferenceHandler("Quota", quota);
                }
                return quota;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ReadLocked
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ReadLocked");
            }
            set
            {
                if (!ReadLocked.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ReadLocked", value);
                }
            }
        }

        public bool ReadOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ReadOnly");
            }
            set
            {
                if (!ReadOnly.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ReadOnly", value);
                }
            }
        }

        public bool WriteLocked
        {
            get
            {
                return base.DataCache.GetProperty<bool>("WriteLocked");
            }
            set
            {
                if (!WriteLocked.Equals(value))
                {
                    base.DataCache.AddChangedProperty("WriteLocked", value);
                }
            }
        }

        public bool TrimAuditLog
        {
            get
            {
                return base.DataCache.GetProperty<bool>("TrimAuditLog");
            }
            set
            {
                if (!TrimAuditLog.Equals(value))
                {
                    base.DataCache.AddChangedProperty("TrimAuditLog", value);
                }
            }
        }

        public bool UIVersionConfigurationEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UIVersionConfigurationEnabled");
            }
            set
            {
                if (!UIVersionConfigurationEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("UIVersionConfigurationEnabled", value);
                }
            }
        }

        public IAveWebApplication WebApplication
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("WebApplication"))
                {
                    Dictionary<string, object> webAppProperties = mRequest.GetWebApplication();
                    AveWebApplication webApps = null;
                    if (webAppProperties != null)
                    {
                        webApps = new AveWebApplication(mRequest, this, webAppProperties);
                        webApps.DataCache.AddPropertyies(webAppProperties);
                    }
                    base.DataCache.AddProperty("WebApplication",webApps);
                    return webApps;
                }
                return base.DataCache.GetProperty<IAveWebApplication>("WebApplication");
            }
        }

        public bool HasHolds
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("HasHolds"))
                {
                    bool enableEDiscoveryHold = mRequest.GetSiteHasHolds();
                    base.DataCache.AddProperty("HasHolds", enableEDiscoveryHold);
                    return enableEDiscoveryHold;
                }
                return base.DataCache.GetProperty<bool>("HasHolds");
            }
        }

        public bool AllowRssFeeds
        {
            get
            {
                //return base.DataCache.GetProperty<bool>("AllowRssFeeds"); 
                return this.SyndicationEnabled;
            }
        }

        public bool IISAllowsAnonymous
        {
            get { return base.DataCache.GetProperty<bool>("IISAllowsAnonymous"); }
        }

        public IAveWorkflowManager WorkflowManager
        {
            get { throw new NotImplementedException(); }
        }

        public ReadOnlyCollection<IAveThmxTheme> ManagedThemes
        {
            get
            {
                if (mManagedThemes == null)
                {
                    IList<Dictionary<string, object>> managedThemes = mRequest.GetManagedThemes();
                    List<IAveThmxTheme> themes = new List<IAveThmxTheme>();
                    if (managedThemes != null)
                    {
                        foreach (Dictionary<string, object> managedThemeProperties in managedThemes)
                        {
                            AveThmxTheme thmxTheme = new AveThmxTheme(this, managedThemeProperties);
                            themes.Add(thmxTheme);
                        }
                    }
                    themes.Sort(new Comparison<IAveThmxTheme>(delegate(IAveThmxTheme x, IAveThmxTheme y) { return string.CompareOrdinal(x.Name, y.Name); }));
                    mManagedThemes = new ReadOnlyCollection<IAveThmxTheme>(themes);
                }
                return mManagedThemes;
            }
        }

        public AveUrlZone Zone
        {
            get
            {
                return AveUrlZone.Default;
            }
        }


        public IAveListTemplateCollection GetCustomListTemplates(IAveWeb web)
        {
            Dictionary<string, object> listTemplateCollection = this.mRequest.GetCustomListTemplates(web.ServerRelativeUrl);
            AveListTemplateCollection listTemplates = new AveListTemplateCollection(this.mRequest, web, listTemplateCollection);
            return listTemplates;
        }


        public bool InvalidateCacheEntry(Uri uri, Guid siteId)
        {
            throw new NotImplementedException();
        }

        public double AverageResourceUsage
        {
            get { return base.DataCache.GetProperty<double>("AverageResourceUsage"); }
        }

        public double CurrentResourceUsage
        {
            get { return base.DataCache.GetProperty<double>("CurrentResourceUsage"); }
        }

        public AveUsageInfo Usage
        {
            get
            {
                return base.DataCache.GetProperty<AveUsageInfo>("Usage");
            }
        }

        public DateTime LastContentModifiedDate
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("LastContentModifiedDate"))
                {
                    //由于site的这个属性现在无法得到，暂时用rootweb的属性代替
                    base.DataCache.AddProperty("LastContentModifiedDate",RootWeb.LastItemModifiedDate);
                }
                return base.DataCache.GetProperty<DateTime>("LastContentModifiedDate");
            }
        }

        public DateTime LastItemUserModifiedDate
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("LastItemUserModifiedDate"))
                {
                    //由于site的这个属性现在无法得到，暂时用rootweb的属性代替
                    base.DataCache.AddProperty("LastItemUserModifiedDate",RootWeb.LastItemUserModifiedDate);
                }
                return base.DataCache.GetProperty<DateTime>("LastItemUserModifiedDate");
            }
        }

        public AveObjectModelFactory ModelFactory
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        private AveDesignPackage mDesignPackageSerializer;
        public IAveDesignPackage DesignPackageSerializer
        {
           get
            {
                if (mDesignPackageSerializer == null)
                {
                    mDesignPackageSerializer = new AveDesignPackage(mRequest);
                }
                return mDesignPackageSerializer;
            }
        }

        public List<AveTermStoreInfo> GetMetadataServiceData()
        {
            return null;
        }

        public Guid ID
        {
            get { return this.Id; }
        }

        public void Restore(List<AveTermStoreInfo> termStoreInfos)
        {

        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            AveRequestInterceptor.DisposeAvailableRequest(this.RequestParameter, mSiteUrl);
            base.DataCache.RemoveProperty("RootWeb");
        }

        #endregion

        public bool BrowserDocumentsEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("BrowserDocumentsEnabled");
            }
        }

        public Guid GetWeb(IAveBackupRestoreQueryService queryService, string p)
        {
            return this.OpenWeb(p).ID;
        }

        public bool AllowUnsafeUpdates
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowUnsafeUpdates");
            }
            set
            {
                if (!AllowUnsafeUpdates.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowUnsafeUpdates", value);
                }
            }
        }

        public VariationsSettings VariationsSettings
        {
            get { return mVariationsSettings; }
        }

        public string GetWebCTNameById(string contentTypeId)
        {
            IAveContentType contentType = this.RootWeb.ContentTypes[new AveContentTypeId(contentTypeId)];
            if (contentType != null)
            {
                return contentType.Name;
            }
            else
            {
                return string.Empty;
            }
        }

        public void UpdateUserInfoByNative(string listName, int userId, AveUserInfo old)
        {

        }

        public void VisualUpgradeWebs()
        {
            throw new NotImplementedException();
        }

        public void UpdateUserInfo(string listName, int userId, AveUserInfo old)
        {
            throw new NotImplementedException();
        }

        public bool CheckUserIfAvailable(int userId)
        {
            return this.RootWeb.SiteUsers.GetByID(userId) != null;
        }


        public IAveWeb OpenWeb(string strUrl, bool requireExactUrl)
        {
            throw new NotImplementedException();
        }

        public void InitializeDBService(string connectionString)
        {

        }

        public IAveList GetCatalog(AveListTemplateType typeCatalog)
        {
            return this.RootWeb.GetCatalog(typeCatalog);
        }

        public Guid GetListId(Guid webId, string listTitle)
        {
            return mRequest.GetListId(webId, listTitle);
        }

        public List<AveComplianceTagInfo> GetAvailableTagsForSite()
        {
            return mRequest.GetAvailableTagsForSite(this.Url);
        }

        #region IAveSite Members


        public string Protocol
        {
            get
            {
                return base.DataCache.GetProperty<string>("Protocol");
            }
        }

        public int Port
        {
            get
            {
                return base.DataCache.GetProperty<int>("Port");
            }
        }

        public string HostName
        {
            get
            {
                return base.DataCache.GetProperty<string>("HostName");
            }
        }

        public IAveUserToken UserToken
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public IAveWeb GetCheckoutWeb(Guid webId, IAveUserToken token)
        {
            throw new NotImplementedException();
        }

        public long GetSiteCollectionStorageNoStub()
        {
            throw new NotImplementedException();
        }


        public DateTime CertificationDate
        {
            get { throw new NotImplementedException(); }
        }

        public IAveUser SystemAccount
        {
            get { throw new NotImplementedException(); }
        }

        public void Delete(bool deleteADAccounts, bool gradualDelete)
        {
            throw new NotImplementedException();
        }

        public void RestoreSettings(AveSiteSettingInfo settingInfo)
        {
            bool needUploadAudit = false;
            if (settingInfo.SyndicationEnabled != null && settingInfo.SyndicationEnabled.IsAvailable && settingInfo.SyndicationEnabled.Value != null)
            {
                this.SyndicationEnabled = settingInfo.SyndicationEnabled.Value.Value;
            }
            Dictionary<string, object> auditChangeProperties = new Dictionary<string, object>();
            if (settingInfo.AuditFlags != null && settingInfo.AuditFlags.IsAvailable && settingInfo.AuditFlags.Value.HasValue && this.Audit != null)
            {
                this.Audit.AuditFlags = (AveAuditMaskType)settingInfo.AuditFlags.Value;
                needUploadAudit = true;
            }
            if (settingInfo.TrimAuditLog != null && settingInfo.TrimAuditLog.IsAvailable && settingInfo.TrimAuditLog.Value != null)
            {
                this.TrimAuditLog = settingInfo.TrimAuditLog.Value.Value;
            }
            // SAAS-28929 CM：Modern site Site collection audit settings中the number of days of audit log data没转
            // 更新 AuditLogTrimmingRetention 属性需要开启 custom script
            if (!DenyAddAndCustomizePagesStatus && settingInfo.AuditLogTrimmingRetention != null && settingInfo.AuditLogTrimmingRetention.IsAvailable && settingInfo.AuditLogTrimmingRetention.Value != null)
            {
                this.AuditLogTrimmingRetention = settingInfo.AuditLogTrimmingRetention.Value.Value;
            }

            if (settingInfo.AuditLogTrimmingCallout != null && settingInfo.AuditLogTrimmingCallout.IsAvailable)
            {
                this.AuditLogTrimmingCallout = settingInfo.AuditLogTrimmingCallout.Value;
            }
            if (settingInfo.UseAuditFlagCache != null && settingInfo.UseAuditFlagCache.IsAvailable
                && settingInfo.UseAuditFlagCache.Value != null && this.Audit != null)
            {
                this.Audit.UseAuditFlagCache = (bool)settingInfo.UseAuditFlagCache.Value.Value;
                needUploadAudit = true;
            }
            if (settingInfo.AuditLogTrimmingCallout != null && settingInfo.AuditLogTrimmingCallout.IsAvailable)
            {
                this.AuditLogTrimmingCallout = settingInfo.AuditLogTrimmingCallout.Value;
            }
            if (settingInfo.UiversionConfigurationEnable != null && settingInfo.UiversionConfigurationEnable.IsAvailable && settingInfo.UiversionConfigurationEnable.Value != null)
            {
                this.UIVersionConfigurationEnabled = settingInfo.UiversionConfigurationEnable.Value.Value;
            }
            if (this.Audit != null && needUploadAudit)
            {
                this.Audit.Update();
            }

            if (settingInfo.PortalName != null && settingInfo.PortalName.IsAvailable)
            {
                this.PortalName = settingInfo.PortalName.Value;
            }
            if (settingInfo.PortalURL != null && settingInfo.PortalURL.IsAvailable)
            {
                this.PortalUrl = settingInfo.PortalURL.Value;
            }
            // add for version B
            if (settingInfo.AllowDesigner != null && settingInfo.AllowDesigner.IsAvailable && settingInfo.AllowDesigner.Value != null)
            {
                this.AllowDesigner = settingInfo.AllowDesigner.Value.Value;
            }
            if (settingInfo.AllowMasterPageEditing != null && settingInfo.AllowMasterPageEditing.IsAvailable && settingInfo.AllowMasterPageEditing.Value != null)
            {
                this.AllowMasterPageEditing = settingInfo.AllowMasterPageEditing.Value.Value;
            }
            if (settingInfo.AllowRevertFromTemplate != null && settingInfo.AllowRevertFromTemplate.IsAvailable && settingInfo.AllowRevertFromTemplate.Value != null)
            {
                this.AllowRevertFromTemplate = settingInfo.AllowRevertFromTemplate.Value.Value;
            }
            if (settingInfo.ShowURLStructure != null && settingInfo.ShowURLStructure.IsAvailable && settingInfo.ShowURLStructure.Value != null)
            {
                this.ShowURLStructure = settingInfo.ShowURLStructure.Value.Value;
            }
            #region variations settings
            bool changed = false;
            if (settingInfo.UserConfiguredEnableAutoSpawn != null && settingInfo.UserConfiguredEnableAutoSpawn.IsAvailable && settingInfo.UserConfiguredEnableAutoSpawn.Value != null)
            {
                this.VariationsSettings.UserConfiguredEnableAutoSpawn = settingInfo.UserConfiguredEnableAutoSpawn.Value.Value;
                changed = true;
            }
            if (settingInfo.StopAutoSpawnAfterDelete != null && settingInfo.StopAutoSpawnAfterDelete.IsAvailable && settingInfo.StopAutoSpawnAfterDelete.Value != null)
            {
                this.VariationsSettings.StopAutoSpawnAfterDelete = settingInfo.StopAutoSpawnAfterDelete.Value.Value;
                changed = true;
            }
            if (settingInfo.UpdateWebParts != null && settingInfo.UpdateWebParts.IsAvailable && settingInfo.UpdateWebParts.Value != null)
            {
                this.VariationsSettings.UpdateWebParts = settingInfo.UpdateWebParts.Value.Value;
                changed = true;
            }
            if (settingInfo.SendNotificationEmail != null && settingInfo.SendNotificationEmail.IsAvailable && settingInfo.SendNotificationEmail.Value != null)
            {
                this.VariationsSettings.SendNotificationEmail = settingInfo.SendNotificationEmail.Value.Value;
                changed = true;
            }
            if (changed)
            {
                if (!this.VariationsSettings.RelationshipsListId.Equals(Guid.Empty))
                {
                    this.VariationsSettings.Update();
                }
                else
                {
                    // relationships list不存在，在postaction还原variations settings
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddNeedRestroreVariationsSettings(this.VariationsSettings.ChangedProperties);
                }
            }
            #endregion
            Dictionary<string, object> newSiteProperties = mRequest.UpdateSite(base.DataCache.ChangedProperties);
            base.DataCache.UpdateProperties(newSiteProperties);
            //updatasite后需要reload rootweb，因为在updateproperties的时候是不会update"rootweb"的
            base.DataCache.RemoveProperty("RootWeb");
        }

        public bool IsMoss
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsMoss");
            }
        }

        public bool IsPublish
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("IsPublish"))
                {
                    Guid featureId = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
                    IAveFeature feature = this.Features[featureId];
                    DataCache.AddProperty("IsPublish", feature != null);
                }
                return base.DataCache.GetProperty<bool>("IsPublish");
            }
        }

        public List<AveAppMetadata> AvaliableTenantApp
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("AvaliableTenantApp"))
                {
                    try
                    {
                        DataCache.AddProperty("AvaliableTenantApp", Request.GetAvailableAppsAsync(this.ServerRelativeUrl, PnP.Framework.Enums.AppCatalogScope.Tenant).GetAwaiter().GetResult());
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error($"Get AvaliableTenantApp failed: {ex}");
                        DataCache.AddProperty("AvaliableTenantApp", new List<AveAppMetadata>());
                    }                    
                }
                return base.DataCache.GetProperty<List<AveAppMetadata>>("AvaliableTenantApp");
            }
        }

        public List<AveAppMetadata> AvaliableSiteApp
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("AvaliableSiteApp"))
                {
                    try
                    {
                        DataCache.AddProperty("AvaliableSiteApp", Request.GetAvailableAppsAsync(this.ServerRelativeUrl, PnP.Framework.Enums.AppCatalogScope.Site).GetAwaiter().GetResult());
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error($"Get AvaliableSiteApp failed: {ex}");
                        DataCache.AddProperty("AvaliableSiteApp", new List<AveAppMetadata>());
                    }
                }
                return base.DataCache.GetProperty<List<AveAppMetadata>>("AvaliableSiteApp");
            }
        }

        public bool EnableSiteAppCatalog
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("EnableSiteAppCatalog"))
                {
                    try
                    {
                        Request.GetAvailableAppsAsync(this.ServerRelativeUrl, PnP.Framework.Enums.AppCatalogScope.Site).GetAwaiter().GetResult();
                        DataCache.AddProperty("EnableSiteAppCatalog", true);
                    }
                    catch(WebException ex)
                    {
                        if (ex.Response is HttpWebResponse
                            && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                        {
                            mLogger.Info("un enable site app catalog list");
                            DataCache.AddProperty("EnableSiteAppCatalog", false);
                        }
                        else
                        {
                            mLogger.Error($"exception when check EnableSiteAppCatalog,error message : {ex}");
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error($"exception when check EnableSiteAppCatalog : {ex}");
                        throw;
                    }
                }
                return base.DataCache.GetProperty<bool>("EnableSiteAppCatalog");
            }
        }

        public string MakeFullUrl(string strUrl)
        {
            throw new NotImplementedException();
        }

        public string MakeFullUrl(string strUrl, string realWebAppUrl)
        {
            throw new NotImplementedException();
        }

        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        #region IAveSite Members


        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Guid CheckOutFileId
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

        public int CheckOutUser
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

        #endregion

        public IAveSiteSerializer SiteSerializer
        {
            get
            {
                if (m_SiteSerializer == null)
                {
                    m_SiteSerializer = new AveSiteSerializer(this);
                }
                return m_SiteSerializer;
            }
        }

        public IAveSiteSettingSerializer SiteSettingSerializer
        {
            get
            {
                if (m_SiteSettingSerializer == null)
                {
                    m_SiteSettingSerializer = new AveSiteSettingSerializer(this);
                }
                return m_SiteSettingSerializer;
            }
        }

        public IAveMetaDataServiceSerializer MetaDataServiceSerializer
        {
            get
            {
                if (m_MetaDataServiceSerializer == null)
                {
                    m_MetaDataServiceSerializer = new AveMetaDataServiceSerializer(this);
                }
                return m_MetaDataServiceSerializer;
            }
        }

        public IAveUserSerializer UserSerializer
        {
            get
            {
                if (m_UserSerializer == null)
                {
                    m_UserSerializer = new AveUserSerializer(this);
                }
                return m_UserSerializer;
            }
        }

        public IAveGroupSerializer GroupSerializer
        {
            get
            {
                if (m_GroupSerializer == null)
                {
                    m_GroupSerializer = new AveGroupSerializer(this);
                }
                return m_GroupSerializer;
            }
        }

        public IAveUsersSerializer SiteUsersSerializer
        {
            get
            {
                if (m_SiteUsersSerializer == null)
                {
                    m_SiteUsersSerializer = new AveSiteUsersSerializer(this);
                }
                return m_SiteUsersSerializer;
            }
        }

        public void ReloadSite()
        {
        }

        public IAveWeb ReloadWeb(Guid webId)
        {
            IAveWeb web = null;
            Dictionary<string, object> webProperties = mRequest.GetWeb(webId);
            web = new AveWeb(mRequest, this, null, webProperties);
            base.DataCache.AddWeakReferenceHandler(webId.ToString(), web);
            return web;
        }

        public void ReloadTaxonomySession()
        {
            try
            {
                AveTaxonomySession session = new AveTaxonomySession(this);
                base.DataCache.AddProperty("AveSPTaxonomySession",session);
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred whil reloading TaxonomySession: " + e.Message + e.StackTrace);
            }
        }

        public IAveFeatureSerializer FeatureSerializer
        {
            get
            {
                if (m_FeatureSerializer == null)
                {
                    m_FeatureSerializer = new AveFeatureSerializer(this, mRequest);
                }
                return m_FeatureSerializer;
            }
        }

        public AveAPIType GetAPIType()
        {
            return m_APIType;
        }

        public AveAPIType APIType
        {
            get
            {
                return m_APIType;
            }
        }

        public long Size
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Size"))
                {
                    DataCache.AddProperty("Size",Usage.Storage);
                }
                return base.DataCache.GetProperty<long>("Size");
            }
        }

        public DateTime LastSecurityModifiedDate
        {
            get { throw new NotImplementedException(); }
        }

        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return mRequest.GetPublishedContentTypes();
        }

        public IAveFeatureDefinitionCollection GetAllFeatureDefinitions()
        {
            Dictionary<string, object> definitionCollection = new Dictionary<string, object>();
            definitionCollection = mRequest.GetAllFeatureDefinitions(this.Url, (int)this.RootWeb.RegionalSettings.LocaleId, "site.features");
            AveFeatureDefinitionCollection definitions = new AveFeatureDefinitionCollection(this.RootWeb, mRequest, definitionCollection, "site.features");
            return definitions;
        }

        public object DataProvider
        {
            get { return this.RequestParameter; }
        }

        public void EnableAlerts(Dictionary<Guid, List<Guid>> alerts)
        {

        }

        public DateTime GetLastAccessedDayOfSite()
        {
            throw new NotImplementedException();
        }

        public void GetRecycleBinStatistics(out int itemCount, out long size)
        {
            throw new NotImplementedException();
        }

        public IAveOUserProfileManager GetUserProfileManager()
        {
            throw new NotImplementedException();
        }


        public DateTime LastReloadTimeUTC
        {
            get { return DateTime.MinValue; }
        }

        public string GetUserLoginBySystemId(byte[] systemId)
        {
            return null;
        }

        public bool IsClassicWindowsModeAuthentication { get { return false; } }


        public int CompatibilityLevel
        {
            get { return base.DataCache.GetProperty<int>("CompatibilityLevel"); }
        }

        public bool ExternalSharingTipsEnabled
        {
            get { return base.DataCache.GetProperty<bool>("ExternalSharingTipsEnabled"); }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return DataCache.EnsureLoadProperty("UserCustomActionCollection",
                    () =>
                    {
                        Dictionary<string, object> userCustomActions = mRequest.UserCustomActionCollection_Load(AveUserCustomActionScope.Site, "", Guid.Empty);
                        AveUserCustomActionCollection aveUserCustomActions = new AveUserCustomActionCollection(this, mRequest, userCustomActions);
                        return aveUserCustomActions;
                    });
            }
        }

        public string GeoLocation
        {
            get { return base.DataCache.GetProperty<string>("GeoLocation"); }
        }

        public IAveWeb AddWeb(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            string parentWebServerRelativeUrl = string.Empty;
            if (strWebUrl.Contains("/"))
            {
                parentWebServerRelativeUrl = strWebUrl.Substring(0, strWebUrl.TrimEnd('/').LastIndexOf('/'));
                if (!parentWebServerRelativeUrl.StartsWith(this.ServerRelativeUrl))
                {
                    parentWebServerRelativeUrl = this.ServerRelativeUrl.TrimEnd('/') + "/" + parentWebServerRelativeUrl.TrimStart('/');
                }
                //strWebUrl = strWebUrl.Substring(strWebUrl.TrimEnd('/').LastIndexOf('/') + 1);
            }
            strWebUrl = strWebUrl.Substring(strWebUrl.TrimEnd('/').LastIndexOf('/') + 1);
            if (this.ServerRelativeUrl.Equals(strWebUrl))
            {
                throw new Exception(string.Format("{0} is root web url", strWebUrl));
            }
            else
            {
                webProperties = this.mRequest.AddWeb(parentWebServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);
            }

            AveWeb web = new AveWeb(mRequest, this, null, webProperties);
            this.DataCache.AddWeakReferenceHandler("OpenWeb" + web.ServerRelativeUrl, web);
            return web;
        }

        /// <summary>
        /// Use customer upload solutionfile  create web
        /// </summary>
        public void ApplyCustomWebTemplateInSolution(string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            mRequest.ApplyCustomWebTemplateInSolution(this.ServerRelativeUrl, solutionPath, solutionName, webTemplateName, lcid, packageFeatures, packageSolutionId);
        }

        public void SetAuditLogTrimming(Dictionary<string, object> parameters)
        {
            mRequest.SetAuditLogTrimming(CompatibilityLevel, parameters);
            string isEnable = (string)parameters["TrimAuditLog"];
            if (isEnable.Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase))
            {
                base.DataCache.AddProperty("AuditLogTrimmingRetention",Convert.ToInt32(parameters["TrimRetention"]));
                base.DataCache.AddProperty("TrimAuditLog",true);
            }
            else
            {
                base.DataCache.AddProperty("TrimAuditLog",false);
            }
        }

        private AveProjectServer EnsureProjectServer()
        {
            if (this.mProjectServer == null)
            {
                this.mProjectServer = new AveProjectServer(mRequest, this);
            }
            return mProjectServer;
        }

        public void AddChangePropertiesToDataCache(Dictionary<string, object> changeProperties)
        {
            if (changeProperties != null && changeProperties.Count > 0)
            {
                base.DataCache.AddChangedProperties(changeProperties);
            }
        }

        public DateTime GetLastAccessTime(string sitecollectionURL, DateTime? modifiedTime = null, bool isCompatibleByModifiedTime = false)
        {
            return mRequest.QueryLastAccessTime(sitecollectionURL, modifiedTime, isCompatibleByModifiedTime);
        }

        public bool DeleteMigrationJob(Guid id)
        {
            return mRequest.DeleteMigrationJob(id);
        }

        public AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            return mRequest.GetMigrationJobStatus(id);
        }

        public MigrationJobProgress GetMigrationJobProgress(Guid id, string nextToken = "0")
        {
            return mRequest.GetMigrationJobProgress(id, nextToken);
        }

        // job is valid and not end
        public bool NeedDeleteMigrationJob(Guid id)
        {
            string currentToken = "0";
            int notExistProgressJobRetryTime = 1;
            try
            {
                var retryHelper = new AveTaskRetryHelper(3);
                MigrationJobProgress migrationJobProgress = null;

                while (true)
                {
                    retryHelper.ExecuteWithRetryMechanism(() => migrationJobProgress = mRequest.GetMigrationJobProgress(id, currentToken));
                    if (migrationJobProgress == null)
                    {
                        mLogger.Info($"MigrationJobProgress not found for this job. Job: {id}, retry: {notExistProgressJobRetryTime}");
                        notExistProgressJobRetryTime++;
                        if (notExistProgressJobRetryTime > 3) return false;
                        continue;
                    }

                    var logs = migrationJobProgress.Logs;
                    var nextToken = migrationJobProgress.NextToken;
                    if (!logs.IsNullOrEmpty())
                    {
                        mLogger.Info($"MigrationJobProgress. Job: {id}, nextToken: {nextToken}, logs count: {logs.Count}");
                        // read from the end to return results earlier if there is a JobEnd in the Logs
                        for (int i = logs.Count -1; i >= 0; i--)
                        {
                            var logStr = logs[i];
                            if (string.IsNullOrWhiteSpace(logStr)) continue;

                            try
                            {
                                var obj = JObject.Parse(logStr);
                                var evt = (obj["Event"] ?? obj["event"])?.ToString()?.Trim();
                                if (string.IsNullOrWhiteSpace(evt)) continue;

                                if (string.Equals(evt, "JobEnd", StringComparison.OrdinalIgnoreCase))
                                {
                                    mLogger.Info($"Job end. Job: {id}");
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                // log parse problem but continue
                                mLogger.Warn($"Failed parse migration log for job: {id}, logStr: {logStr}, Ex: {ex.Message}");

                                // try finding Log Event manually
                                if (logStr.Contains("\"Event\":\"JobEnd\"", StringComparison.OrdinalIgnoreCase))
                                {
                                    mLogger.Info($"Job end in Exception. Job: {id}");
                                    return false;
                                }
                            }
                        }
                    }

                    if (nextToken != "0" && logs.IsNullOrEmpty())
                    {
                        mLogger.Info($"Empty Logs collection in MigrationJobProgress because there is no update for job: {id}");
                        return true;
                    }

                    if (string.IsNullOrEmpty(nextToken) || string.Equals(currentToken, nextToken, StringComparison.OrdinalIgnoreCase))
                    {
                        mLogger.Info($"NextToken is null or not change for job {id}, token '{currentToken}', nextToken {nextToken}. Stop paging.");
                        return true;
                    }

                    currentToken = nextToken;
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occured while checking MigrationJobProgress for Job: {id}, last token: {currentToken}, exception: {e}");
                return false;
            }

            return true;
        }

        public Dictionary<Guid, AveMigrationJobState> GetMigrationStatus()
        {
            return mRequest.GetMigrationStatus();
        }

        public Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            return mRequest.CreateMigrationJob(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri);
        }

        public Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            return mRequest.CreateMigrationJobEncrypted(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri, options);
        }

        public AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            return mRequest.ProvisionMigraitonContainers();
        }

        public AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            return mRequest.ProvisionMigrationQueue();
        }

        #region pwa

        public IAveProjectServer ProjectServer
        {
            get
            {
                return EnsureProjectServer();
            }
        }

        public IAveProjectCollection Projects
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.Projects;
            }
        }

        public IAveProjectCalendarCollection ProjectCalendars
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.Calendars;
            }
        }

        public IAveProjectCustomFieldCollection ProjectCustomFields
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.CustomFields;
            }
        }

        public IAveProjectLookupTableCollection ProjectLookupTables
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.LookupTables;
            }
        }

        public IAveProjectEnterpriseProjectTypeCollection ProjectEnterpriseProjectTypes
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.EnterpriseProjectTypes;
            }
        }

        public IAveProjectEnterpriseResourceCollection ProjectEnterpriseResources
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.EnterpriseResources;
            }
        }

        public IAveProjectPhaseCollection ProjectPhases
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.ProjectPhases;
            }
        }

        public IAveProjectStageCollection ProjectStages
        {
            get
            {
                EnsureProjectServer();
                return this.mProjectServer.ProjectStages;
            }
        }

        IAveRequest IAveSite.Request
        {
            get
            {
                return mRequest;
            }
        }

        #endregion

        public bool CheckSiteIsLocked()
        {
            return mRequest.CheckSiteIsLocked();
        }

        public void RemoveSiteLockedState()
        {
            mRequest.RemoveSiteLockedState();
        }

        public bool DeleteSCTermGroup()
        {
            try
            {
                mRequest.DeleteSCTermGroup();
                mLogger.Info($"Success delete sc term group");
                return true;
            }
            catch(Exception e)
            {
                mLogger.Error($"Fail delete SC Term Group,ex:{e}");
                return false;
            }
        }

        public bool ExistSCTermGroup()
        {
            try
            {
                return mRequest.ExistSCTermGroup();
            }
            catch (Exception e)
            {
                mLogger.Error($"Fail check SC Term Group exist,ex:{e}");
                throw;
            }
        }

        public bool UpdateSCTermGroupName(string name)
        {
            try
            {
                mRequest.UpdateSCTermGroupName(name);
                return true;
            }
            catch (Exception e)
            {
                mLogger.Error($"Fail update SC Term Group name, name:{name} ,ex:{e}");
                return false;
            }
        }
    }

    class VariationsSettings
    {
        private bool isLoaded = false;
        private bool isAvailable = false;
        private AveSite aveSite;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(VariationsSettings));

        public static string[] PropertyNames = new string[] { UserConfiguredEnableAutoSpawnKey, StopAutoSpawnAfterDeleteKey, UpdateWebPartsKey, SendNotificationEmailKey };

        public const string RelationshipsListIdKey = "_VarRelationshipsListId";
        public const string UserConfiguredEnableAutoSpawnKey = "EnableAutoSpawnPropertyName";
        public const string StopAutoSpawnAfterDeleteKey = "AutoSpawnStopAfterDeletePropertyName";
        public const string UpdateWebPartsKey = "UpdateWebPartsPropertyName";
        public const string SendNotificationEmailKey = "SendNotificationEmailPropertyName";

        public Dictionary<string, object> ChangedProperties = new Dictionary<string, object>();

        public VariationsSettings(AveSite aveSite)
        {
            // TODO: Complete member initialization
            this.aveSite = aveSite;
        }

        private Guid relationshipsListId;
        public Guid RelationshipsListId
        {
            get
            {
                EnsureVariationsSetttings();
                return relationshipsListId;
            }
        }

        public bool? UserConfiguredEnableAutoSpawn
        {
            get
            {
                EnsureVariationsSetttings();
                return aveSite.DataCache.GetProperty<bool?>(UserConfiguredEnableAutoSpawnKey);
            }
            set
            {
                if (!UserConfiguredEnableAutoSpawn.Equals(value))
                {
                    ChangedProperties[UserConfiguredEnableAutoSpawnKey] = value;
                }
            }
        }
        public bool? StopAutoSpawnAfterDelete
        {
            get
            {
                EnsureVariationsSetttings();
                return aveSite.DataCache.GetProperty<bool?>(StopAutoSpawnAfterDeleteKey);
            }
            set
            {
                if (!StopAutoSpawnAfterDelete.Equals(value))
                {
                    ChangedProperties[StopAutoSpawnAfterDeleteKey] = value;
                }
            }
        }
        public bool? UpdateWebParts
        {
            get
            {
                EnsureVariationsSetttings();
                return aveSite.DataCache.GetProperty<bool?>(UpdateWebPartsKey);
            }
            set
            {
                if (!UpdateWebParts.Equals(value))
                {
                    ChangedProperties[UpdateWebPartsKey] = value;
                }
            }
        }
        public bool? SendNotificationEmail
        {
            get
            {
                EnsureVariationsSetttings();
                return aveSite.DataCache.GetProperty<bool?>(SendNotificationEmailKey);
            }
            set
            {
                if (!SendNotificationEmail.Equals(value))
                {
                    ChangedProperties[SendNotificationEmailKey] = value;
                }
            }
        }

        private void EnsureVariationsSetttings()
        {
            if (!isLoaded)
            {
                if (aveSite.RootWeb.AllProperties.ContainsKey(RelationshipsListIdKey))
                {
                    var listId = aveSite.RootWeb.AllProperties[RelationshipsListIdKey];
                    if (listId != null)
                    {
                        try
                        {
                            relationshipsListId = new Guid(listId.ToString());
                            var list = aveSite.RootWeb.Lists[relationshipsListId];

                            Dictionary<string, object> variationProperties = new Dictionary<string, object>();
                            foreach (var propertyName in PropertyNames)
                            {
                                if (list.RootFolder.Properties.ContainsKey(propertyName))
                                {
                                    variationProperties[propertyName] = Convert.ToBoolean((string)list.RootFolder.Properties[propertyName]);
                                }
                            }
                            aveSite.DataCache.AddPropertyies(variationProperties);

                            isAvailable = true;
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Error while init site:{0} variation settings, relationships listId:{1}, error:{2}", aveSite.Url, listId, ex);
                        }
                    }
                }
                isLoaded = true;
            }
        }

        public void Update()
        {
            if (isAvailable && ChangedProperties != null && ChangedProperties.Count > 0)
            {
                ChangedProperties[RelationshipsListIdKey] = RelationshipsListId;
                aveSite.DataCache.AddChangedProperties(ChangedProperties);
                ChangedProperties.Clear();
            }
        }
    }
}
