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
using AvePoint.ObjectModel.CompoundRequest;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common.Office;
using System.IO;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.ObjectModel.Common
{
    class AveSite : AveClientObject, IAveSite
    {
        private IAveRequest mRequest;
        private ReadOnlyCollection<IAveThmxTheme> mManagedThemes;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveSite));
        private string mSiteUrl;
        private AveSiteSerializer m_SiteSerializer;
        private AveSiteSettingSerializer m_SiteSettingSerializer;
        private Dictionary<Guid, IAveTerm> mTermIdCache = new Dictionary<Guid, IAveTerm>();
        private ThreadSafeDictionary<string, Guid> webUrlAndIdMapping = new ThreadSafeDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        //private AveMetaDataServiceSerializer MetaDataServiceSerializer;
        private AveUserSerializer m_UserSerializer;
        private AveGroupSerializer m_GroupSerializer;
        private AveSiteUsersSerializer m_SiteUsersSerializer;
        private AveFeatureSerializer m_FeatureSerializer;
        private AveMetaDataServiceSerializer m_MetaDataServiceSerializer;
        private string m_SPVersion;
        private AveAuthenticationMode m_AveAuthenticationMode;
        private IAveQuota mAveQuota;
        private AveWebCollection.ISPWebCollectionProvider webCollectionProvider;
        private IAveWebCollection allWebs;
        private static object locker = new object();
        private Dictionary<string, int> workingLanguage = new Dictionary<string, int>();
        private AveProjectServer mProjectServer;

        public AveRequestParameter RequestParameter { get; private set; }
        public Dictionary<Guid, IAveTerm> TermIdCache
        {
            get { return mTermIdCache; }
            set { mTermIdCache = value; }
        }

        public AveBPOSAccountInfo UserAccountInfo { get; private set; }
        public bool IsAdminCenter { get; private set; }

        internal AveWebCollection.ISPWebCollectionProvider WebCollectionProvider
        {
            get { return webCollectionProvider ?? (webCollectionProvider = new SPWebCollectionProvider(this)); }
        }
        public AveSite(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            UserAccountInfo = userAccountInfo;
            mSiteUrl = siteUrl;
            InitRequest();
            RequestParameter = new AveRequestParameter(mRequest, m_SPVersion, m_AveAuthenticationMode);
            Dictionary<string, object> siteProperites = mRequest.GetSite();
            base.DataCache.AddPropertyies(siteProperites);
            mSiteUrl = this.Url;
            string relativeUrl = siteUrl.TrimEnd('/').Substring(Url.TrimEnd('/').Length);
            if (relativeUrl.Length > 0    // source url(siteUrl) is different with the url(Url) that gotten site collection
                && System.Text.RegularExpressions.Regex.Matches(Url.TrimEnd('/'), "/").Count == 2   //Gotten site collection is root site collection.
                && (relativeUrl.StartsWith("/sites/", StringComparison.OrdinalIgnoreCase)
                || relativeUrl.StartsWith("/teams/", StringComparison.OrdinalIgnoreCase)
                || relativeUrl.StartsWith("/personal/", StringComparison.OrdinalIgnoreCase)))   //source site collection is not root site collection              
            {
                mLogger.Info("An error occurred while get AveSite, site does not exist.Site url: {0}", siteUrl);
                throw new FileNotFoundException(string.Format(AvePoint.Wrapper.Resource.Exception.WrapperExceptionResource.Wrapper_Exception_Backup_NotFindSiteCollection), siteUrl);
            }
        }

        public AveSite(string siteUrl, AveBPOSAccountInfo userAccountInfo, bool isAdminCenter)
        {
            this.IsAdminCenter = isAdminCenter;
            UserAccountInfo = userAccountInfo;
            mSiteUrl = siteUrl;
            InitRequest();
            RequestParameter = new AveRequestParameter(mRequest, m_SPVersion, m_AveAuthenticationMode);
            Dictionary<string, object> siteProperites = IsAdminCenter ? mRequest.GetAdminCenterSite() : mRequest.GetSite();
            base.DataCache.AddPropertyies(siteProperites);
        }

        public AveSite()
        { }

        private void InitRequest()
        {
            
            AveRequestInterceptor request = new AveRequestInterceptor(mSiteUrl, UserAccountInfo);
            mRequest = request.Proxy;
            m_SPVersion = request.SPVersion;
            m_AveAuthenticationMode = request.AuthMode;
            this.APIType = mRequest.Kind == AveRequestKind.Extension ? AveAPIType.BPOS_D : AveAPIType.BPOS_S;

            
            //            AveClientRequest request = new AveClientRequest(mSiteUrl, mUserAccountInfo);
            //            mRequest = request.InitRequest();
            
        }

        
        internal void GetWorkingLanguage(ref int language)
        {
            lock(locker)
            {
                bool getprofileValue = false;
                var languageValue = 0;
                if (!workingLanguage.TryGetValue(this.UserAccountInfo.UserName, out languageValue))
                {
                    try
                    {
                        var pm = new Office.AveOUserProfileManager(null, this);
                        var user = this.RootWeb.EnsureUser(this.UserAccountInfo.UserName);
                        var profile = pm.GetUserProfile(user.LoginName);
                        if (profile != null)
                        {
                            var values = (profile["SPS-MUILanguages"].Value as List<object>);
                            if (values != null && values.Count > 0)
                            {
                                var value = values[0].ToString();
                                language = new System.Globalization.CultureInfo(value.Substring(0, value.IndexOf(','))).LCID;
                                getprofileValue = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (!getprofileValue && this.IsOnlineSite) 
                    {
                        using (var azurePSRequest = new AveAzurePowerShellRequest(this.UserAccountInfo))
                        {
                            var user = azurePSRequest.GetUser(this.UserAccountInfo.UserName);
                            if (user != null && !string.IsNullOrEmpty(user.PreferredLanguage))
                            {
                                language = new System.Globalization.CultureInfo(user.PreferredLanguage).LCID;
                            }
                        }
                    }
                    workingLanguage[this.UserAccountInfo.UserName] = language;
                }
                else
                {
                    language = languageValue;
                }
            }
        }

        internal IAveRequest Request
        {
            get
            {
                return mRequest;
            }
        }

        #region IAveSite Members

        public AvePoint.Wrapper.Core.Common.WrapperSPMode SPMode
        {
            get { return AvePoint.Wrapper.Core.Common.WrapperSPMode.O365; }
        }

        public IAveTaxonomySession AveSPTaxonomySession
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AveSPTaxonomySession"))
                {
                    AveTaxonomySession session = new AveTaxonomySession(this);
                    base.DataCache.PropertiesCache["AveSPTaxonomySession"] = session;
                }
                return base.DataCache.GetProperty<IAveTaxonomySession>("AveSPTaxonomySession");
            }
        }

        public void Close()
        {
            Dispose();
        }



        public void Delete()
        {
            this.RootWeb.Delete();   //delete lists under RootWeb
            DeleteWebs(this.RootWeb);   //delete webs under RootWeb
            //mRequest.DeleteSite();
        }

        private void DeleteWebs(IAveWeb web)
        {
            try
            {
                foreach (IAveWeb subWeb in web.Webs)
                {
                    try
                    {
                        DeleteWebs(subWeb);
                    }
                    finally
                    {
                        if (subWeb != null)
                        {
                            subWeb.Dispose();
                        }
                    }
                }
                if (!web.IsRootWeb)
                {
                    web.Delete();
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, (web == null || web.Exists == false) ? string.Empty : web.Url, ex);
            }
        }
        public IAveWebTemplateCollection GetWebTemplates(uint lcid)
        {
            if (base.DataCache.IsPropertyNotLoaded("AveWebTemplateCollection"))
            {
                Dictionary<string, object> webTemplateProperties = mRequest.GetWebTemplates(null, lcid, false, "site.getWebTemplates");
                IAveWebTemplateCollection templateCollection = new AveWebTemplateCollection(this, mRequest, webTemplateProperties);
                base.DataCache.PropertiesCache["AveWebTemplateCollection"] = templateCollection;
            }
            return base.DataCache.GetProperty<IAveWebTemplateCollection>("AveWebTemplateCollection");
        }

        public IAveWebTemplateCollection GetWebTemplates(uint licd, int overrideCompatLevel)
        {
            return GetWebTemplates(licd);
        }

        public IAveWeb OpenWeb(Guid webId)
        {
            return new AveWeb(this, webId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webUrl">server relative url of the web</param>
        /// <returns></returns>
        public IAveWeb OpenWeb(string webUrl)
        {
            return new AveWeb(this, webUrl);
        }

        public IAveWeb OpenWeb()
        {
            return new AveWeb(this, mRequest.OpenCurrentWeb());
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> siteProperties = mRequest.UpdateSite(base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(siteProperties);
                //updatasite后需要reload rootweb，因为在updateproperties的时候是不会update"rootweb"的
                if (base.DataCache.PropertiesCache.ContainsKey("RootWeb"))
                {
                    base.DataCache.PropertiesCache.Remove("RootWeb");
                }
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
            get { return allWebs ?? (allWebs = new AveWebCollection(mRequest, this, this.WebCollectionProvider)); }
        }

        public IAveAudit Audit
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Audit")) //&& base.DataCache.PropertiesCache.ContainsKey("Audit" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    //Create an empty audit object, init it while it is being used. Now only AuditFlags property is in used.
                    AveAudit audit = new AveAudit(mRequest, this, null);
                    base.DataCache.PropertiesCache["Audit"] = audit;
                }
                return base.DataCache.GetProperty<IAveAudit>("Audit");
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
                //if (!string.Equals(AuditLogTrimmingCallout, value, StringComparison.OrdinalIgnoreCase))
                //{
                //    base.DataCache.AddChangedProperty("AuditLogTrimmingCallout", value);
                //}
            }
        }

        public int AuditLogTrimmingRetention
        {
            get
            {
                return (this.Audit as AveAudit).AuditLogTrimmingRetention;
            }
            set
            {
                if (!AuditLogTrimmingRetention.Equals(value))
                {
                    //(this.Audit as AveAudit).AuditLogTrimmingRetention = value;
                    //if (IsGreaterThanOrEqualToSPVersion(15))
                    //{
                    //    base.DataCache.AddChangedProperty("AuditLogTrimmingRetention", value);
                    //}
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
                    base.DataCache.PropertiesCache["Owner"] = owner;
                    return owner;
                }
                return base.DataCache.GetProperty<IAveUser>("Owner");
            }
            set
            {
                throw new NotImplementedException();
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
                        base.DataCache.PropertiesCache["SecondaryContact"] = secondaryContact;
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
                        userSolutionCol = new AveUserSolutionCollection(mRequest, this, userSolutionColProperties);
                    }
                    base.DataCache.PropertiesCache["Solutions"] = userSolutionCol;
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
                    base.DataCache.PropertiesCache.Add("RecycleBin", recycleBinItems);
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
                    AveWeb rootWeb = new AveWeb(this, rootWebProperties);
                    base.DataCache.PropertiesCache["RootWeb"] = rootWeb;
                    return rootWeb;
                }
                return base.DataCache.GetProperty<IAveWeb>("RootWeb");
            }
        }

        public IAveFeatureCollection Features
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Features"))
                {
                    Dictionary<string, object> featureCollection = mRequest.GetFeatures(null, "site.features");
                    AveFeatureCollection features = new AveFeatureCollection(this.RootWeb as AveWeb, mRequest, featureCollection, "site.features");
                    base.DataCache.PropertiesCache.Add("Features", features);
                    return features;
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
                    base.DataCache.PropertiesCache["SyndicationEnabled"] = syndicationEnabled;
                }
                return base.DataCache.GetProperty<bool>("SyndicationEnabled");
            }
            set
            {
                if (!SyndicationEnabled.Equals(value))
                {
                    mRequest.UpdateSiteRssSetting(value);
                    base.DataCache.PropertiesCache["SyndicationEnabled"] = value;
                    //base.DataCache.AddChangedProperty("SyndicationEnabled", value);
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

        public IAveQuota Quota
        {
            get
            {
                if (mAveQuota == null)
                {
                    mAveQuota = new AveQuota(this.mRequest);
                }
                return mAveQuota;
            }
            set
            {
                mAveQuota = value;
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
                return (this.Audit as AveAudit).RequestTrimAuditLog;
            }
            set
            {
                if (!TrimAuditLog.Equals(value))
                {
                    //(this.Audit as AveAudit).RequestTrimAuditLog = value;
                    //if (IsGreaterThanOrEqualToSPVersion(15))
                    //{
                    //    base.DataCache.AddChangedProperty("TrimAuditLog", value);
                    //}
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
                return null;
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
            get { return null; }
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
                    themes.Sort(new Comparison<IAveThmxTheme>(delegate (IAveThmxTheme x, IAveThmxTheme y) { return string.CompareOrdinal(x.Name, y.Name); }));
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
                if (base.DataCache.IsPropertyNotLoaded("Usage"))
                {

                    base.DataCache.PropertiesCache["Usage"] = new AveUsageInfo();
                }
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
                    base.DataCache.PropertiesCache["LastContentModifiedDate"] = this.RootWeb.LastItemModifiedDate;
                }
                return base.DataCache.GetProperty<DateTime>("LastContentModifiedDate");
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
            RootWeb.Dispose();//need dispose RootWeb to clear cache.
            AveRequestInterceptor.DisposeAvailableRequest(this.RequestParameter, mSiteUrl, UserAccountInfo.GetAccountName());
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
            //ADO-166862: Open Web 是费时的操作， 添加Cache 提高效率
            if (!webUrlAndIdMapping.ContainsKey(p))
            {
                using (var tempWeb = this.OpenWeb(p))
                {
                    webUrlAndIdMapping[p] = tempWeb.ID;
                }
            }
            return webUrlAndIdMapping[p];
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

        public bool IsSiteMaster
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSiteMaster");
            }
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

        public IAveList GetCatalog(AveListTemplateType typeCatalog)
        {
            return this.RootWeb.GetCatalog(typeCatalog);
        }

        public Guid GetListId(Guid webId, string listTitle)
        {
            return mRequest.GetListId(webId, listTitle);
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
            if (settingInfo.SyndicationEnabled != null && settingInfo.SyndicationEnabled.IsAvailable && settingInfo.SyndicationEnabled.Value != null)
            {
                this.SyndicationEnabled = settingInfo.SyndicationEnabled.Value.Value;
            }
            Dictionary<string, object> auditChangeProperties = new Dictionary<string, object>();
            bool needUpdateAudit = false;
            if (settingInfo.AuditFlags != null && settingInfo.AuditFlags.IsAvailable && this.Audit != null)
            {
                if (settingInfo.AuditFlags.Value == null)
                {
                    this.Audit.AuditFlags = AveAuditMaskType.None;
                }
                else
                {
                    this.Audit.AuditFlags = (AveAuditMaskType)settingInfo.AuditFlags.Value;
                }
                needUpdateAudit = true;
            }
            if (settingInfo.TrimAuditLog != null && settingInfo.TrimAuditLog.IsAvailable && settingInfo.TrimAuditLog.Value != null)
            {
                this.TrimAuditLog = settingInfo.TrimAuditLog.Value.Value;
                needUpdateAudit = true;
            }
            if (settingInfo.AuditLogTrimmingRetention != null && settingInfo.AuditLogTrimmingRetention.IsAvailable && settingInfo.AuditLogTrimmingRetention.Value != null)
            {
                this.AuditLogTrimmingRetention = settingInfo.AuditLogTrimmingRetention.Value.Value;
                needUpdateAudit = true;
            }

            if (settingInfo.AuditLogTrimmingCallout != null && settingInfo.AuditLogTrimmingCallout.IsAvailable)
            {
                this.AuditLogTrimmingCallout = settingInfo.AuditLogTrimmingCallout.Value;
            }
            if (settingInfo.UseAuditFlagCache != null && settingInfo.UseAuditFlagCache.IsAvailable
                && settingInfo.UseAuditFlagCache.Value != null && this.Audit != null)
            {
                auditChangeProperties.Add("UseAuditFlagCache", settingInfo.UseAuditFlagCache.Value.Value);
            }
            if (settingInfo.AuditLogTrimmingCallout != null && settingInfo.AuditLogTrimmingCallout.IsAvailable)
            {
                this.AuditLogTrimmingCallout = settingInfo.AuditLogTrimmingCallout.Value;
            }
            if (settingInfo.UiversionConfigurationEnable != null && settingInfo.UiversionConfigurationEnable.IsAvailable && settingInfo.UiversionConfigurationEnable.Value != null)
            {
                this.UIVersionConfigurationEnabled = settingInfo.UiversionConfigurationEnable.Value.Value;
            }
            if (this.Audit != null && needUpdateAudit)
            {
                this.Audit.Update();
            }
            if (WrapperConfiguration.RestorePortalConnection)
            {
                if (settingInfo.PortalName != null && settingInfo.PortalName.IsAvailable)
                {
                    this.PortalName = settingInfo.PortalName.Value;
                }
                if (settingInfo.PortalURL != null && settingInfo.PortalURL.IsAvailable)
                {
                    this.PortalUrl = settingInfo.PortalURL.Value;
                }
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
            //enable share external user setting
            if (settingInfo.ShareByEmailEnabled != null && settingInfo.ShareByEmailEnabled.IsAvailable && settingInfo.ShareByEmailEnabled.Value != null)
            {
                this.ShareByEmailEnabled = settingInfo.ShareByEmailEnabled.Value.Value;
            }
            Dictionary<string, object> newSiteProperties = mRequest.UpdateSite(base.DataCache.ChangedProperties);
            base.DataCache.UpdateProperties(newSiteProperties);
            //updatasite后需要reload rootweb，因为在updateproperties的时候是不会update"rootweb"的
            if (base.DataCache.PropertiesCache.ContainsKey("RootWeb"))
            {
                base.DataCache.PropertiesCache.Remove("RootWeb");
            }
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
                    IAveFeature feature = this.Features[AveSP2010FeatureDefinitions.PublishingSite];
                    if (feature != null)
                    {
                        base.DataCache.PropertiesCache["IsPublish"] = true;
                    }
                    else
                    {
                        base.DataCache.PropertiesCache["IsPublish"] = false;
                    }
                }
                return base.DataCache.GetProperty<bool>("IsPublish");
            }
        }

        public string MakeFullUrl(string strUrl)
        {
            if (strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            strUrl = strUrl.Trim();
            StringBuilder builder = new StringBuilder(0x200);
            if (strUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(Protocol);
                builder.Append("//");
                builder.Append(HostName);
                if ((AveSPUtility.StsCompareStrings(Protocol, "http:") && (Port != 80)) || (AveSPUtility.StsCompareStrings(Protocol, "https:") && (Port != 443)))
                {
                    builder.Append(":");
                    builder.Append(Port);
                }
                builder.Append(strUrl);
            }
            else
            {
                builder.Append(Url);
                if (strUrl != "")
                {
                    builder.Append("/");
                    builder.Append(strUrl);
                }
            }
            if (builder[builder.Length - 1] == '/')
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public string MakeFullUrl(string strUrl, string realWebAppUrl)
        {
            throw new NotImplementedException();
        }

        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EventReceivers"))
                {
                    Dictionary<string, object> eventReceiversProperties = mRequest.GetSiteEventReceiverDefinitions(this.ServerRelativeUrl, "site.eventReceivers");
                    AveEventReceiverDefinitionCollection eventReceiverDefinitionCol = null;
                    if (eventReceiversProperties != null)
                    {
                        eventReceiverDefinitionCol = new AveEventReceiverDefinitionCollection(this, mRequest, "site.eventReceivers", eventReceiversProperties);
                    }
                    base.DataCache.PropertiesCache["EventReceivers"] = eventReceiverDefinitionCol;
                    return eventReceiverDefinitionCol;
                }
                return base.DataCache.GetProperty<IAveEventReceiverDefinitionCollection>("EventReceivers");
            }
        }

        #region IAveSite Members


        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId)
        {
            throw new NotImplementedException();
        }

        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob, bool throwIfNotHaveEnoughPermission = false)
        {
            throw new NotImplementedException();
        }

        public IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId, bool isBackupJob)
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
            web = new AveWeb(this, webProperties);
            base.DataCache.AddWeakReferenceHandler(webId.ToString(), web);
            return web;
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
            return this.APIType;
        }

        public AveAPIType APIType
        {
            set;
            get;
        }

        public long Size
        {
            get { return this.Usage.Storage; }
        }

        public DateTime LastSecurityModifiedDate
        {
            get { throw new NotImplementedException(); }
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
            //DateTime lastAccessedDayOfSite = DateTime.MinValue;
            //foreach (Guid webId in QueryService.GetAllWebsGuidByNative(mSite.ID))
            //{
            //    DateTime lastAccessedDayOfWeb = QueryService.GetLastAccessedDayOfWeb(ID, webId);
            //    lastAccessedDayOfSite = lastAccessedDayOfWeb > lastAccessedDayOfSite ? lastAccessedDayOfWeb : lastAccessedDayOfSite;
            //}

            //if (lastAccessedDayOfSite < LastContentModifiedDate)
            //{
            //    lastAccessedDayOfSite = LastContentModifiedDate;
            //}
            //if (lastAccessedDayOfSite < LastSecurityModifiedDate)
            //{
            //    lastAccessedDayOfSite = LastSecurityModifiedDate;
            //}
            //return lastAccessedDayOfSite;
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

        public bool ActiveDeletedUserBySystemId(byte[] systemId)
        {
            return false;
        }

        public bool IsClassicWindowsModeAuthentication { get { return false; } }

        public bool IsOnlineSite
        {
            get
            {
                return (AveAuthenticationMode.Online & m_AveAuthenticationMode) != 0;
            }
        }
        /// <summary>
        /// 只有真实365会返回正确的值,其他返回false.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool Exists(Uri uri)
        {
            return mRequest.GetSiteExists(uri.ToString());
        }

        public string SPVersion
        {
            get
            {
                return m_SPVersion;
            }
        }


        public bool ShareByEmailEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShareByEmailEnabled");
            }
            set
            {
                if (!ShareByEmailEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ShareByEmailEnabled", value);
                }
            }
        }

        #region add for SP2013
        public int CompatibilityLevel
        {
            get { return base.DataCache.GetProperty<int>("CompatibilityLevel"); }
        }

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
        public AveBasePermissions DenyPermissionsMask
        {
            get { return AveBasePermissions.EmptyMask; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region Add to operate Change Log ** We will implement this for SP2013 server model first **
        public IAveChangeCollection GetChanges()
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            Dictionary<string, object> changeCollectionDic = mRequest.GetSiteChangesByQuery((query as AveChangeQuery).DataCache.PropertiesCache);
            return new AveChangeCollection(changeCollectionDic);
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            throw new NotImplementedException();
        }
        #endregion

        public IAveRecycleBinItemCollection GetRecycleBinItems(IAveRecycleBinQuery query)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// this property is not supported by client api
        /// </summary>
        public bool IsReadLocked
        {
            get { return false; }
        }

        public void InternalCleanup()
        {
            //only server object moudle use this method.
        }

        public Dictionary<string, string> GetLookupItemIdAndDisplayValue(AveLookupFieldInfo fieldInfo)
        {
            return null;
        }

        public bool AdministratorOperationMode
        {
            get
            {
                //Not Implemented.return false as default value
                return false;
            }
            set
            { }
        }

        public AveBitField Flags
        {
            //set default value to 0 for API/Cmdlet Browser
            get { return default(AveBitField); }
        }

        /// <summary>
        /// add for PRItem apps,Not Implemented.return null as default value
        /// </summary>
        public IAveQuerySession SqlSession
        {
            get { return null; }
        }

        /// <summary>
        /// this property only used for PRItem 2013,so not need to implemente in client API
        /// </summary>
        public string AppSiteDomainPrefix
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public void CustomizeReport(Dictionary<string, object> parameters)
        {
            Guid reportId = Guid.Empty;
            if (this.CompatibilityLevel < 15 && string.Compare(this.SPVersion, "15.0.0.0", StringComparison.OrdinalIgnoreCase) < 0)
            {
                IAveList list = null;
                if (this.RootWeb.AllProperties.ContainsKey("_reportinggallerymetadataid") &&
                    this.RootWeb.AllProperties["_reportinggallerymetadataid"] != null)
                {
                    list = this.RootWeb.Lists.GetById(new Guid(this.RootWeb.AllProperties["_reportinggallerymetadataid"].ToString()));
                }
                else
                {
                    list = this.RootWeb.GetList("Lists/Reporting Metadata");
                }
                IAveListItem item = list.GetItemById(1);
                reportId = item.UniqueId;
            }
            mRequest.CustomizeReport(parameters, reportId);
        }

        public Dictionary<Guid, long> GetAllWebSize()
        {
            throw new NotImplementedException();
        }

        public bool MigrateUser(string oldLogin, byte[] oldSid, string newLogin, byte[] newSid)
        {
            return false;
        }


        public WrapperNativeApiPermission NativeApiPermission
        {
            get { return WrapperNativeApiPermission.Api; }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return null;
            }
        }

        private class SPWebCollectionProvider : AveWebCollection.ISPWebCollectionProvider
        {
            private AveSite Site { get; set; }
            private IAveRequest Request { get; set; }

            public SPWebCollectionProvider(AveSite site)
            {
                Site = site;
                Request = site.Request;
            }


            public IEnumerable<Dictionary<string, object>> GetWebsData()
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Id", this.Site.RootWeb.ID }, { "Title", this.Site.RootWeb.Title }, { "ServerRelativeUrl", this.Site.RootWeb.ServerRelativeUrl } } }
                    .Concat(RecurseWebs(this.Site.RootWeb.ID));
            }

            public Dictionary<string, object> Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere)
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                string parentWebServerRelativeUrl = string.Empty;
                var lastSlashPos = strWebUrl.TrimEnd('/').LastIndexOf('/');

                if (lastSlashPos > 0)
                {
                    parentWebServerRelativeUrl = strWebUrl.Substring(0, lastSlashPos);
                }
                strWebUrl = strWebUrl.Substring(lastSlashPos + 1);
                if (string.Equals(this.Site.ServerRelativeUrl, strWebUrl, StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_RootWebError, strWebUrl);
                }
                if (!string.IsNullOrEmpty(strWebTemplate) && (strWebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase) || strWebTemplate.StartsWith("SPSMSITEHOST", StringComparison.OrdinalIgnoreCase)))
                {
                    var masterPageInfo = this.Request.GetRootWebMasterPageInfo();
                    webProperties = this.Request.AddWeb(parentWebServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);

                    string mySiteWebServerRelativeUrl = webProperties.ContainsKey("ServerRelativeUrl") ? (webProperties["ServerRelativeUrl"]).ToString() : string.Empty;
                    this.Request.SetRootWebAndMySiteWebMasterPageInfo(mySiteWebServerRelativeUrl, masterPageInfo);

                    if(webProperties.ContainsKey("MasterUrl") && !string.IsNullOrEmpty(masterPageInfo.MPageUrl))
                    {
                        webProperties["MasterUrl"] = masterPageInfo.MPageUrl;
                    }
                    if (webProperties.ContainsKey("CustomMasterUrl") && !string.IsNullOrEmpty(masterPageInfo.CPageUrl))
                    {
                        webProperties["CustomMasterUrl"] = masterPageInfo.CPageUrl;
                    }
                }
                else
                {
                    webProperties = this.Request.AddWeb(parentWebServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);
                }
                return webProperties;
            }

            public IAveWeb OpenWeb(string name)
            {
                return this.Site.OpenWeb(name);
            }

            private IEnumerable<Dictionary<string, object>> RecurseWebs(Guid webId)
            {
                var subWebs = Request.GetSubWebsBasicInfo(this.Site.Url, webId);
                return subWebs.Select(web => web.Value).Concat(subWebs.SelectMany(currentWeb => RecurseWebs(currentWeb.Key)));
            }
        }

        #region Only Online Site support these Method
        public bool DenyAddAndCustomizePagesStatus
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DenyAddAndCustomizePagesStatus"))
                {
                    bool status = (mRequest ).GetDenyAddAndCustomizePagesStatus();
                    base.DataCache.PropertiesCache["DenyAddAndCustomizePagesStatus"] = status;
                    return status;
                }
                return base.DataCache.GetProperty<bool>("DenyAddAndCustomizePagesStatus");
            }
            set
            {
                if (!DenyAddAndCustomizePagesStatus.Equals(value))
                {
                    (mRequest).SetDenyAddAndCustomizePagesStatus(value);
                    base.DataCache.AddChangedProperty("DenyAddAndCustomizePagesStatus", value);
                }
            }
        }

        public bool DeleteMigrationJob(Guid id)
        {
            return Request.DeleteMigrationJob(id);
        }

        public AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            return Request.GetMigrationJobStatus(id);
        }

        public Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            return Request.CreateMigrationJob(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri);
        }

        public virtual Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            return Request.CreateMigrationJobEncrypted(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri, options);
        }

        /// <summary>
        /// Use customer upload solutionfile  create web
        /// </summary>
        public void ApplyCustomWebTemplateInSolution(string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            (Request ).ApplyCustomWebTemplateInSolution(this.ServerRelativeUrl, solutionPath, solutionName, webTemplateName, lcid, packageFeatures, packageSolutionId);
        }

        public AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            return (Request ).ProvisionMigraitonContainers();
        }

        public AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            return (Request ).ProvisionMigrationQueue();
        }


        public List<AveComplianceTagInfo> GetAvailableTagsForSite()
        {
            return (Request).GetAvailableTagsForSite(this.Url);
        }

        #endregion


        private bool IsGreaterThanOrEqualToSPVersion(int spMajorVersion)
        {
            try
            {
                var spVersion = new Version(m_SPVersion);
                return spVersion.Major >= spMajorVersion;
            }
            catch (Exception e)
            {
                mLogger.Warn("spVersion value is not format version class require. spVersion: {0}, error: {1}", m_SPVersion, e);
            }
            return m_SPVersion != null && m_SPVersion.StartsWith(spMajorVersion.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        internal string GetVariationLabelName(Guid labelId)
        {
            return VariationLabelsMappingIdKey.ContainsKey(labelId) ? VariationLabelsMappingIdKey[labelId] : string.Empty;
        }

        public Guid GetVariationLabelId(string labelName)
        {
            return VariationLabelsMappingNameKey.ContainsKey(labelName) ? VariationLabelsMappingNameKey[labelName] : Guid.Empty;
        }

        private Dictionary<Guid, string> variationLabelsMappingIdKey;
        internal Dictionary<Guid, string> VariationLabelsMappingIdKey
        {
            get
            {
                return variationLabelsMappingIdKey = variationLabelsMappingIdKey != null ? variationLabelsMappingIdKey : GetVariationLabelMappingIdKey();
            }
        }

        private Dictionary<string, Guid> variationLabelsMappingNameKey;
        internal Dictionary<string, Guid> VariationLabelsMappingNameKey
        {
            get
            {
                return variationLabelsMappingNameKey = variationLabelsMappingNameKey ?? GetVariationLabelMappingNameKey();
            }
        }

        private Dictionary<Guid, string> GetVariationLabelMappingIdKey()
        {

                var items = GetVariationLabels();
                return Enumerable.Cast<AveListItem>(items).ToDictionary(item => item.UniqueId, item => item["Title"] != null ? item["Title"].ToString() : string.Empty); //InternalName=Title，DisplayName=Label，使用InternalName取值
            
        }

        private Dictionary<string, Guid> GetVariationLabelMappingNameKey()
        {

                var items = GetVariationLabels();
                return Enumerable.Cast<AveListItem>(items).ToDictionary(item => item["Title"] != null ? item["Title"].ToString() : string.Empty, item => item.UniqueId); //InternalName=Title，DisplayName=Label，使用InternalName取值
            
        }

        private IEnumerable<AveListItem> GetVariationLabels()
        {
            try
            {
                var listiId = new Guid(this.RootWeb.AllProperties["_VarLabelsListId"].ToString());
                var list = this.RootWeb.Lists.GetList(listiId, true);
                return list.Items.Cast<AveListItem>();
            }
            catch (Exception ex)
            {
                mLogger.Warn("Cannot find variation labels list in site: {0}, error: {1}", this.Url, ex);
                return new AveListItem[0];
            }
        }

        public IAveFeatureDefinitionCollection GetAllFeatureDefinitions()
        {
            Dictionary<string, object> definitionCollection = new Dictionary<string, object>();
            definitionCollection = mRequest.GetAllFeatureDefinitions(this.Url, "site.features");
            AveFeatureDefinitionCollection definitions = new AveFeatureDefinitionCollection(this.RootWeb, mRequest, definitionCollection, "site.features");
            return definitions;
        }
        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return mRequest.GetPublishedContentTypes();
        }

        #region pwa

        public IAveProjectServer ProjectServer
        {
            get
            {
                return EnsureProjectServer();
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

        #endregion
    }
}
