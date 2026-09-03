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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.Server16
{
    class AveAppSerializer : IAveAppSerializer, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveAppSerializer));
        private IAveWeb aveWeb;
        private Guid productId;
        private IAveAppInstance appInstance;
        private SPSite appCatalogSite = null;
        private SPWeb appCatalogWeb = null;
        private SPList appCatalogList = null;

        private AveRestoreOption restoreOption;
        private IAveRestoreStream receiver;
        AveAppPackageInfo appPackageInfo;

        public AveAppSerializer(IAveWeb web)
        {
            this.aveWeb = web;
        }

        public IList<IAveAppInstance> Apps
        {
            get 
            { 
                return new AveAppCatalog().GetAppInstances(this.aveWeb);
            }
        }

        #region App Backup Code
        public Guid ProductId
        {
            get { return this.productId; }
            set { this.productId = value; }
        }

        /// <summary>
        /// 应该是能够通过查询数据库得到这些信息的
        /// </summary>
        /// <returns></returns>
        public AveAppPackageInfo GetObjectData()
        {
            AveAppPackageInfo packageInfo = new AveAppPackageInfo();
            try
            {
                appInstance = this.aveWeb.GetAppInstancesByProductId(productId)[0];
                packageInfo.Title = appInstance.Title;
                packageInfo.ProductId = appInstance.App.ProductId;
                packageInfo.Version = appInstance.App.VersionString;
                packageInfo.AppSource = appInstance.App.Source;
                packageInfo.InstanceId = appInstance.Id;
                packageInfo.AssetId = appInstance.App.AssetId;
                packageInfo.ContentMarket = appInstance.App.ContentMarket;
            }
            catch (Exception ex)
            {
                logger.Warn(ex.ToString());
            }
            return packageInfo;
        }

        public Stream GetAppPackage()
        {
            Stream appStream = appInstance.App.GetPackage() as Stream;
            return appStream;
        }

        public Stream GetAppPackageForPRItem13()
        {
            Stream appStream = appInstance.App.GetPackageForPRItem13(aveWeb) as Stream;
            return appStream;
        }
        #endregion

        #region App Restore Code
        public IAveAppInstance SetObjectData(AveAppPackageInfo obj)
        {
            AveAppCatalog appCatalog = new AveAppCatalog();
            this.appPackageInfo = obj;

            AveSPFileStream fileStream = new AveSPFileStream(this.receiver);
            if (fileStream == null)
            {
                logger.Warn("App stream is null " + obj.Title);
                throw new AveWrapperAppException("App stream is null.");
            }
            //用ProductId来判断目的端是否已经安装此App
            IList<IAveAppInstance> appInstances = appCatalog.GetAppInstancesByProductId(this.aveWeb, obj.ProductId);
            if (appInstances.Count > 0)
            {

                if (restoreOption.mAveRestoreMode == AveRestoreMode.Default) //skip
                {
                    this.appInstance = appInstances[0];
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Add(this.appPackageInfo.InstanceId);
                    return this.appInstance;
                }
                else if (restoreOption.mAveRestoreMode == AveRestoreMode.UpgradeOnly) //upgrade
                {
                    this.appInstance = appInstances[0];
                    if (Version.Parse(this.appInstance.App.VersionString) < Version.Parse(this.appPackageInfo.Version))
                    {
                        this.appInstance.Upgrade(fileStream, this.aveWeb, (int)obj.AppSource);
                        CheckAppInstanceInstalled(this.appInstance.Id);
                    }
                    else
                    {
                        restoreOption.mAveRestoreMode = AveRestoreMode.Default;
                    }
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Add(this.appPackageInfo.InstanceId);
                    return this.appInstance;
                }
                else if (restoreOption.mAveRestoreMode == AveRestoreMode.Replace)
                {
                    appInstances[0].Uninstall();
                    if (!CheckAppInstanceUninstalled(appInstances[0]))
                    {
                        //如果在15s内仍然没有卸载成功，...
                    }
                }
            }
            else
            {
                restoreOption.mAveRestoreMode = AveRestoreMode.Restore;
            }

            IAveAppInstance appInstance = null;
            try
            {
                appInstance = this.aveWeb.LoadAndInstallApp(fileStream, (int)obj.AppSource, obj.AssetId, obj.ContentMarket);
            }
            catch (SPException ex)
            {
                logger.Warn("An error occurred while loading app. Message:{0}", ex);
                if (ex.ErrorCode == -2146232832 && ex.InnerException != null)
                {
                    throw new AveWrapperAppException(ex.InnerException.Message);
                }
                throw new AveWrapperAppException(ex.Message);
            }
            if (CheckAppInstanceInstalled(appInstance.Id))
            {
                this.appInstance = aveWeb.GetAppInstanceById(appInstance.Id);
                TrustApp(appInstance);
            }
            return this.appInstance;
        }

        public void SetStream(IAveRestoreStream stream)
        {
            this.receiver = stream;
        }

        public void SetRestoreOption(object option)
        {
            this.restoreOption = option as AveRestoreOption;
        }

        /// <summary>
        /// get the real restore option
        /// </summary>
        /// <returns>AveRestoreMode</returns>
        public int GetRestoreOption()
        {
            return (int)restoreOption.mAveRestoreMode;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "the wrong word is part of url.")]
        private void TrustApp(IAveAppInstance appInstance)
        {
            string appManifest = GetAppManifest(appInstance.App.GetFingerprint(), aveWeb.Site.ID);
            appManifest = appManifest.Substring(appManifest.IndexOf("<App",StringComparison.Ordinal));
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(appManifest);

            string appPrincipalIdentifier = GetAppPrincipalIdentifier(appInstance.AppPrincipalId);
            SPAppPrincipal appPrincipal;
            if (IsExternalApp(xmlDoc.GetElementsByTagName("AppPrincipal")))
            {
                appPrincipal = SPAppPrincipalManager.GetManager(((AveWeb)this.aveWeb).Web).LookupAppPrincipal(SPAppPrincipalIdentityProvider.External, SPAppPrincipalName.CreateFromAppPrincipalIdentifier(appPrincipalIdentifier));
            }
            else
            {
                appPrincipal = SPAppPrincipalManager.GetManager(((AveWeb)this.aveWeb).Web).LookupAppPrincipal(SPAppPrincipalIdentityProvider.Internal, SPAppPrincipalName.CreateFromAppPrincipalIdentifier(appPrincipalIdentifier));
            }

            SPAppPrincipalPermissionsManager permissionManager = new SPAppPrincipalPermissionsManager(((AveWeb)this.aveWeb).Web);
            bool isDefaultPermission = true;
            XmlNodeList appPermissionRequests = xmlDoc.GetElementsByTagName("AppPermissionRequests");
            if (appPermissionRequests != null && appPermissionRequests.Count > 0)
            {
                foreach (XmlNode node in appPermissionRequests[0].ChildNodes)
                {
                    XmlElement element = node as XmlElement;
                    if (element == null)
                        continue;
                    string scope = element.GetAttribute("Scope");
                    string right = element.GetAttribute("Right");
                    SPAppPrincipalPermissionKind kind = SPAppPrincipalPermissionKind.None;
                    if (Enum.TryParse<SPAppPrincipalPermissionKind>(right, true, out kind))
                    {
                        //参考https://msdn.microsoft.com/en-us/library/office/fp142383(v=office.15).aspx
                        if (scope.Equals("http://sharepoint/content/sitecollection", StringComparison.OrdinalIgnoreCase))
                        {
                            permissionManager.AddAppPrincipalToSite(appPrincipal, kind);
                        }
                        else if (scope.Equals("http://sharepoint/content/sitecollection/web", StringComparison.OrdinalIgnoreCase))
                        {
                            permissionManager.AddAppPrincipalToWeb(appPrincipal, kind);
                            isDefaultPermission = false;
                        }
                        else if (scope.Equals("http://sharepoint/content/sitecollection/web/list", StringComparison.OrdinalIgnoreCase))
                        {
                            //No particular list found, need to do in the future.
                            //permissionManager.AddAppPrincipalToList(appPrincipal, null, kind);
                        }
                        else
                        {
                            permissionManager.AddSiteSubscriptionContentPermission(appPrincipal, kind);
                        }
                    }
                }
            }
            if (isDefaultPermission)
            {
                permissionManager.AddAppPrincipalToWeb(appPrincipal, SPAppPrincipalPermissionKind.Guest);
            }

            AveAssemblyUtility.InvokeMethod(permissionManager, "AddAppInstanceAppPrincipalToAppWeb", new Type[] { typeof(Guid) }, new object[] { this.appInstance.Id });

            //Microsoft.SharePoint.Administration.SPContentServiceAppPermissionProvider+SPContentServiceAppPermissionUI
        }

        private string GetAppManifest(byte[] appFingerprint, Guid siteId)
        {
            return (aveWeb.Site as AveSite).QueryService.GetAppManifest(appFingerprint, siteId);

        }

        private bool IsExternalApp(XmlNodeList nodeList)
        {
            foreach (XmlElement element in nodeList.OfType<XmlElement>())
            {
                if (element.InnerXml.StartsWith("<Internal", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        private string GetAppPrincipalIdentifier(string appPrincipalId)
        {
            if (appPrincipalId.Contains('|'))
            {
                appPrincipalId = appPrincipalId.Substring(appPrincipalId.LastIndexOf('|') + 1);
            }
            if (appPrincipalId.Contains('@'))
            {
                appPrincipalId = appPrincipalId.Substring(0, appPrincipalId.IndexOf('@'));
            }
            return appPrincipalId;
        }

        #endregion

        #region App Upgrade code
        public void UpgradeAppByProductId(Guid productId)
        {
            IList<IAveAppInstance> instances = this.aveWeb.GetAppInstancesByProductId(productId);
            if (instances.Count == 1)
            {
                IAveAppInstance instance = instances[0];
                if (instance.App.IsUpdateAvailable)
                {
                    Stream appStream = null;
                    switch (instance.App.Source)
                    {
                        case AveAppSource.CorporateCatalog:
                            appStream = GetAppUpgradeStreamFromCatalog(productId);
                            if (appStream != null)
                            {
                                //instance.Upgrade(appStream);
                                instance.Upgrade(appStream, aveWeb, (int)AveAppSource.CorporateCatalog);
                            }
                            break;
                        case AveAppSource.Marketplace:
                            appStream = GetAppUpgradeStreamFromOnline(productId, instance);
                            
                            break;
                        default:
                            //to do
                            break;
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", Justification = "appCatalogList.Items in a foreach")]
        private Stream GetAppUpgradeStreamFromCatalog(Guid productId)
        {
            Stream appStream = null;
            IAveFeature appFeature = this.aveWeb.Site.WebApplication.Features[AveSP2013FeatureDefinitions.AppCatalogSettings];

            if (appFeature != null)
            {
                Guid appCatalogSiteId = Guid.Empty;
                Guid appCatalogListId = Guid.Empty;
                IAveFeatureProperty appCatalogSiteIdProperty = appFeature.Properties["__AppCatSiteId"];
                IAveFeatureProperty appCatalogListIdProperty = appFeature.Properties["__AppCatListId"];
                if (appCatalogSiteIdProperty != null && Guid.TryParse(appCatalogSiteIdProperty.Value, out appCatalogSiteId) &&
                    appCatalogListIdProperty != null && Guid.TryParse(appCatalogListIdProperty.Value, out appCatalogListId))
                {
                    if (this.appCatalogSite == null || this.appCatalogSite.ID != appCatalogSiteId)
                    {
                        this.appCatalogSite = new SPSite(appCatalogSiteId);
                        this.appCatalogWeb = appCatalogSite.RootWeb;
                        this.appCatalogList = this.appCatalogWeb.Lists[appCatalogListId];
                    }

                    if (WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping == null)
                    {
                        Dictionary<Guid, Version> cacheAppVersion = new Dictionary<Guid, Version>();
                        WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping = new Dictionary<Guid, Guid>();
                        foreach (SPListItem item in this.appCatalogList.Items)
                        {
                            try
                            {
                                Guid tempId = new Guid(item["AppProductID"].ToString().Trim('{', '}').ToLower(CultureInfo.InvariantCulture));
                                if (WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping.ContainsKey(tempId))
                                {
                                    SPListItem listitem = this.appCatalogList.GetItemByUniqueId(item.UniqueId);
                                    if (Version.Parse(listitem["AppVersion"].ToString()) > cacheAppVersion[tempId])
                                    {
                                        WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping[tempId] = item.UniqueId;
                                        cacheAppVersion[tempId] = Version.Parse(listitem["AppVersion"].ToString());
                                    }
                                }
                                else
                                {
                                    WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping[tempId] = item.UniqueId;
                                    SPListItem listitem = this.appCatalogList.GetItemByUniqueId(item.UniqueId);
                                    cacheAppVersion[tempId] = Version.Parse(listitem["AppVersion"].ToString());
                                }
                            }
                            catch(Exception ex)
                            {
                                logger.Warn("Get app stream from " + ex.Message);
                            }
                        }
                    }

                    if (WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping.ContainsKey(productId))
                    {
                        Guid itemUniqueId = WrapperRuntime.CurrentContext.MappingManager.CommonMappingManager.AppProductIdMapping[productId];
                        appStream = this.appCatalogList.GetItemByUniqueId(itemUniqueId).File.OpenBinaryStream();
                    }
                }
                else
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server13_NotCreateAppCatalog);
                }
            }
            return appStream;
        }

        private Stream GetAppUpgradeStreamFromOnline(Guid productId, IAveAppInstance instance)
        {
            SPAppLicense license;
            Stream appStream = null;
            long appStreamLength = 0;
            string responseUrl = string.Empty;
            #region for current context
            SPWeb web = ((AveWeb)this.aveWeb).Web;
            MemoryStream httpStream = new MemoryStream();
            HttpRequest resquest = new HttpRequest(string.Empty, web.Url, string.Empty);
            HttpResponse respose = new HttpResponse(new StreamWriter(httpStream));
            HttpContext.Current = new HttpContext(resquest, respose);
            HttpContext.Current.Items.Add("HttpHandlerSPWeb", web);
            typeof(HttpRequest).GetField("_httpMethod", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(HttpContext.Current.Request, "POST");
            #endregion
            TryGetAppLicense(productId, out license);
            if (license != null)
            {
                object assetId = AveAssemblyUtility.GetPropertyValue(license, "AssetId");
                object billingMarket = AveAssemblyUtility.GetPropertyValue(license, "BillingMarket");
                object contentMarket = AveAssemblyUtility.GetPropertyValue(license, "ContentMarket");
                string rawXMLLicenseToken = license.RawXMLLicenseToken;
                //app details
                Type t = typeof(SPWeb).Assembly.GetType("Microsoft.SharePoint.Marketplace.OfficeProxy.OfficeProxy", false);
                Type ts = typeof(SPWeb).Assembly.GetType("Microsoft.SharePoint.Marketplace.OfficeProxy.SPAppMetadataDetail", false);
                MethodInfo method = t.GetMethod("GetAppDetails", BindingFlags.Public | BindingFlags.Static, null, new Type[] {typeof(string), typeof(string), typeof(string), ts.MakeByRefType()},null);
                dynamic appDetails = null;
                object[] objs = new object[] { billingMarket, contentMarket, assetId, appDetails };
                method.Invoke(null, objs);
                Type tt = typeof(SPWeb).Assembly.GetType("Microsoft.SharePoint.Marketplace.OfficeProxy.SPAppMetadataDetail", false);
                byte[] fingerprint = (byte[])tt.GetField("Fingerprint").GetValue(objs[3]);
                PropertyInfo appBasicDetails = tt.GetProperty("BasicDetails", BindingFlags.Instance | BindingFlags.Public);
                Version version = null;
                if (appBasicDetails != null)
                {
                    object obj = appBasicDetails.GetValue(objs[3]);
                    Type appDataType = typeof(SPWeb).Assembly.GetType("Microsoft.SharePoint.Marketplace.OfficeProxy.SPAppMetadata", false);
                    PropertyInfo versionProperty = appDataType.GetProperty("VersionObject", BindingFlags.Instance | BindingFlags.Public);
                    if (versionProperty != null)
                    {
                        version = (Version)versionProperty.GetValue(obj);
                    }
                }
                string title = instance.Title;
                string tempIconUrl = null;
                object tempIconUrlObj = typeof(SPApp).GetProperty("IconFallbackUrlAbsolute", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(((AveApp)instance.App).App);
                if (tempIconUrlObj != null)
                {
                    tempIconUrl = tempIconUrlObj.ToString();
                }
                string oauthAppId = instance.AppPrincipalId;

                //app stream
                method = t.GetMethod("GetAppDownloadStream");
                objs = new object[] { billingMarket, contentMarket, rawXMLLicenseToken, appStream, appStreamLength, responseUrl };
                method.Invoke(null, objs);                
                appStream = (Stream)objs[3];
                appStreamLength = (long)objs[4];
                if (appStream != null)
                {

                    object[] para = new object[] { instance.Id, fingerprint, appStream, web, productId, version, title, appStreamLength, SPAppSource.Marketplace, contentMarket, assetId, tempIconUrl, oauthAppId };
                    //MethodInfo mmm = null;
                    //MethodInfo[] md = typeof(SPSecurity).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                    //List<MethodInfo> ms = md.Where(m => { return m.Name.Equals("RunAsUser"); }).ToList();
                    //foreach (MethodInfo item in ms)
                    //{
                    //    if (item.GetParameters().Length == 4)
                    //    {
                    //        mmm = item;
                    //        break;
                    //    }
                    //}
                    MethodInfo mmm = typeof(SPSecurity).GetMethod("RunAsUser", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] {typeof(SPUserToken),typeof(bool),typeof(WaitCallback),typeof(object) }, null);
                    object[] paramss = new object[] { web.CurrentUser.UserToken, true, new WaitCallback(CallBackMethod), para };
                    mmm.Invoke(null, paramss);
                    int counter = 0;
                    while (this.aveWeb.GetAppInstancesByProductId(productId)[0].App.VersionString != version.ToString())                    
                    {
                        Thread.Sleep(1000);
                        counter++;
                        if (counter > WrapperConfiguration.CheckAppInstanceInstalledTime)
                            break;
                    }

                    if (this.aveWeb.GetAppInstancesByProductId(productId)[0].App.VersionString != version.ToString())
                    {
                        logger.Warn("Update app {0} failed." + instance.Title);
                    }

                    //method = typeof(SPApp).GetMethod("AsyncLoadAndUpgrade", BindingFlags.NonPublic | BindingFlags.Static);
                    //object[] objcts = new object[] { instance.Id, fingerprint, appStream, web, productId, version, title, appStreamLength, SPAppSource.Marketplace, contentMarket, assetId, tempIconUrl, oauthAppId };
                    //method.Invoke(null, objcts);
                }
                //{
                //    method = typeof(SPApp).GetMethod("AsyncLoadAndInstall", BindingFlags.NonPublic | BindingFlags.Static);
                //    object[] objcts = new object[] { fingerprint, appStream, web, productId, version, title, appStreamLength, SPAppSource.Marketplace, contentMarket, assetId, tempIconUrl, oauthAppId };
                //    method.Invoke(null, objcts);
                //}
            }
            else
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server13_NotGetAppLicenseForUser);
            }
            return appStream;
        }

        private bool TryGetAppLicense(Guid productId, out SPAppLicense license)
        {
            try
            {
                object licenseManager = AveAssemblyUtility.CreateInstance("Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c",
                    "Microsoft.SharePoint.SPAppLicenseManager",
                    new Type[] { typeof(SPWeb) }, new object[] { (this.aveWeb as AveWeb).Web });
                SPAppLicenseCollection licenses = AveAssemblyUtility.InvokeMethod(licenseManager, "CheckLicense",
                    new Type[] { typeof(Guid) }, new object[] { productId }) as SPAppLicenseCollection;
                license = SPUtility.GetTopEntitlement(licenses);

            }
            catch (Exception exception)
            {
                logger.Warn("get app license exception :" + exception.ToString());
                license = null;
                return false;
            }
            return true;
        }

        private void CallBackMethod(object obj)
        {
            try
            {
                SPWeb web = ((AveWeb)this.aveWeb).Web;
                object[] param = obj as object[];
                MethodInfo method = typeof(SPApp).GetMethod("AsyncLoadAndUpgrade", BindingFlags.NonPublic | BindingFlags.Static);
                Type genericIdentityType = Thread.CurrentPrincipal.Identity.GetType();
                genericIdentityType.GetField("m_name", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Thread.CurrentPrincipal.Identity, web.CurrentUser.LoginName);
                genericIdentityType.GetField("m_type", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Thread.CurrentPrincipal.Identity, SPSecurity.AuthenticationMode.ToString());
                //MemoryStream httpStream = new MemoryStream();
                //HttpRequest resquest = new HttpRequest(string.Empty, web.Url, string.Empty);
                //HttpResponse respose = new HttpResponse(new StreamWriter(httpStream));
                //HttpContext.Current = new HttpContext(resquest, respose);
                //HttpContext.Current.Items.Add("HttpHandlerSPWeb", web);
                //typeof(HttpRequest).GetField("_httpMethod", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(HttpContext.Current.Request, "POST");
                //HttpContext.Current.User = Thread.CurrentPrincipal;
                ((AveWeb)this.aveWeb).FakeSPContext(true);
                method.Invoke(null, param);

            }
            catch (Exception ex)
            {
                string msg = ex.InnerException.ToString();
            }
            finally
            {
                this.aveWeb.SetSPContextNull();
            }
        }

        #endregion


        /// <summary>
        /// 检查App是否安装成功
        /// </summary>
        /// <param name="appInstanceId"></param>
        /// <returns></returns>
        private bool CheckAppInstanceInstalled(Guid appInstanceId)
        {
            int count = 0;
            do
            {
                Thread.Sleep(1000);
                appInstance = this.aveWeb.GetAppInstanceById(appInstanceId);
                count++;
                if (count > WrapperConfiguration.CheckAppInstanceInstalledTime)
                    break;
            }
            while (appInstance != null && (appInstance.Status == AveAppInstanceStatus.Installing || appInstance.Status == AveAppInstanceStatus.Upgrading));

            if (appInstance.Status != AveAppInstanceStatus.Installed)
            {
                logger.Warn("Install or upgrade app {0} failed, counter is {1} status is {2}.", appPackageInfo.Title, count,appInstance.Status);
                throw new AveWrapperAppException(AveInternalResourceKey.Wrapper_Exception_Server13_InstallAppFailed, appPackageInfo.Title, count);
            }
            else
            {
                logger.Info("Install or upgrade app {0} successfully.", appPackageInfo.Title);
                return true;
            }
        }

        /// <summary>
        /// 检查App是否卸载成功
        /// </summary>
        /// <param name="appInstance"></param>
        public bool CheckAppInstanceUninstalled(IAveAppInstance appInstance)
        {
            try
            {
                int count = 0;
                AveAppInstanceStatus status;
                do
                {
                    Thread.Sleep(1000);
                    status = (aveWeb.Site as AveSite).QueryService.CheckAppInstallationStatus(this.aveWeb.Site.ID, this.aveWeb.ID, appInstance.App.SourceInfoId);
                    count++;
                    if (count > WrapperConfiguration.CheckAppInstanceInstalledTime)
                        break;

                }
                while (status != AveAppInstanceStatus.InvalidStatus);

                if (status != AveAppInstanceStatus.InvalidStatus)
                {
                    logger.Warn("Install app {0} failed, counter is {1}.", appInstance.Title, count);
                    return false;
                }
                else
                {
                    logger.Info("Install app {0} successfully.", appInstance.Title);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Uninstall app error {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.appCatalogList != null)
                this.appCatalogList = null;
            if (this.appCatalogWeb != null)
            {
                this.appCatalogWeb.Dispose();
                this.appCatalogWeb = null;
            }
            if (this.appCatalogSite != null)
            {
                this.appCatalogSite.Dispose();
                this.appCatalogSite = null;
            }
        }
    }
}
