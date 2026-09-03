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
    using AvePoint.ObjectModel.WebService;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Common.Common.ObjectModel.Apps;
    using Microsoft.SharePoint.Client;
    using Microsoft365.Authentication;
    using Microsoft365.SharePoint.Extension;
    using PnP.Core.Model.SharePoint;
    using PnP.Framework.ALM;
    using PnP.Framework.Enums;
    using PnP.Framework.Utilities.Async;
    using PnP.Framework.Utilities.REST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Xml;
    using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using ClientFolder = Microsoft.SharePoint.Client.Folder;
    using SPChangeType = Microsoft.SharePoint.Client.ChangeType;


    public partial class AveClientOM2013Request
    {

        public Dictionary<string, object> GetApps(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> appsProperties = new Dictionary<string, object>();
                var appPropertyList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientObjectList<AppInstance> apps = AppCatalog.GetAppInstances(context, web);
                context.Load(apps);
                context.Load(web.AppTiles);
                context.ExecuteQuery();
                AssemblyAppsProperties(webServerRelativeUrl, web.AppTiles, apps, appPropertyList);
                appsProperties.AddChildren(appPropertyList);
                return appsProperties;
            }
        }

        private void AssemblyAppsProperties(string webServerRelativeUrl, AppTileCollection appTiles, ClientObjectList<AppInstance> apps, List<IDictionary<string, object>> appPropertyList)
        {
            if (apps.Count > 0)
            {
                //List<Dictionary<string, object>> appsMetadata = GetInstalledApps(webServerRelativeUrl);

                Dictionary<Guid, AppTile> appTileMapping = new Dictionary<Guid, AppTile>();
                foreach (var appTile in appTiles)
                {
                    appTileMapping[appTile.AppId] = appTile;
                }

                foreach (AppInstance app in apps)
                {
                    Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                    CopyProperty(appInstanceProperties, app);
                    if (!string.IsNullOrEmpty(app.AppWebFullUrl))
                    {
                        appInstanceProperties["AppWebFullUrl"] = new Uri(app.AppWebFullUrl);
                    }

                    Uri startPage = null;
                    if (Uri.TryCreate(app.StartPage, UriKind.RelativeOrAbsolute, out startPage))
                    {
                        appInstanceProperties["StartPage"] = startPage;
                    }

                    Dictionary<string, object> appProperties = new Dictionary<string, object>();
                    appProperties["ProductId"] = app.ProductId;

                    AppTile appTile;
                    if (appTileMapping.TryGetValue(app.Id, out appTile))
                    {
                        appProperties["Source"] = (AveAppSource)(int)appTile.AppSource;
                    }
                    else
                    {
                        appProperties["Source"] = AveAppSource.InvalidSource;
                    }

                    appInstanceProperties["App"] = appProperties;
                    appPropertyList.Add(appInstanceProperties);
                }
            }
        }
        private void WaitUntilUninstallFinish(AveClientContext context, Web web, Guid productId)
        {
            int retryCount = 0;
            while (true)
            {
                if (!GetAppStatus(context, web, productId))
                {
                    break;
                }
                if (retryCount++ > 60)
                {
                    throw new TimeoutException("time out when uninstalling app");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        private bool GetAppStatus(AveClientContext context, Web web, Guid productId)
        {
            bool exist = false;
            var apps = web.GetAppInstancesByProductId(productId);
            context.Load(apps);
            context.ExecuteQuery();
            if (apps.Count > 0)
            {
                exist = true;
            }
            return exist;
        }
        public Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> appsProperties = new Dictionary<string, object>();
                var appPropertyList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientObjectList<AppInstance> apps = web.GetAppInstancesByProductId(productId);
                context.Load(apps);
                context.Load(web.AppTiles);
                context.ExecuteQuery();
                AssemblyAppsProperties(webServerRelativeUrl, web.AppTiles, apps, appPropertyList);
                appsProperties.AddChildren(appPropertyList);
                return appsProperties;
            }
        }

        public Dictionary<string, object> GetAppInstanceById(string webServerRelativeUrl, Guid appInstanceId)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                AppInstance appInstance = web.GetAppInstanceById(appInstanceId);
                context.Load(appInstance);
                context.ExecuteQuery();
                CopyProperty(appInstanceProperties, appInstance);
                if (!string.IsNullOrEmpty(appInstance.AppWebFullUrl))
                {
                    appInstanceProperties["AppWebFullUrl"] = new Uri(appInstance.AppWebFullUrl);
                }
                Uri startPage = null;
                if (Uri.TryCreate(appInstance.StartPage, UriKind.RelativeOrAbsolute, out startPage))
                {
                    appInstanceProperties["StartPage"] = startPage;
                }
                return appInstanceProperties;
            }
        }

        public async Task<List<AveAppMetadata>> GetAvailableAppsAsync(string webServerRelativeUrl, AppCatalogScope scope)
        {
            string webUrl = this.WebAppName + webServerRelativeUrl;
            string catalogScope = (scope == AppCatalogScope.Tenant) ? "tenant" : "sitecollection";
            string requestUri = $"{webUrl}/_api/web/{catalogScope}appcatalog/AvailableApps";

            try
            {
                string response = await ExecuteAppCatalogRequestAsync(webUrl, requestUri, null, "Failed to get available apps.");
                if (string.IsNullOrWhiteSpace(response))
                {
                    return new List<AveAppMetadata>();
                }

                JsonSerializerOptions serializerOptions = new JsonSerializerOptions
                {
                    IgnoreNullValues = true
                };
                ResultCollection<AveAppMetadata> resultCollection = JsonSerializer.Deserialize<ResultCollection<AveAppMetadata>>(response, serializerOptions);
                if(resultCollection?.Items != null)
                {
                    foreach(var app in resultCollection.Items)
                    {
                        app.Scope = scope;
                    }
                }
                return resultCollection?.Items?.ToList() ?? new List<AveAppMetadata>();
            }
            catch (Exception ex)
            {
                mLogger.Error($"Failed to get available apps. Url:{requestUri}, ex:{ex.Message}", ex);
                throw;
            }
        }

        public async Task<List<AveAppMetadata>> GetAvailableAppsByTitleAsync(string webServerRelativeUrl, AppCatalogScope scope, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("title");
            }

            return (await GetAvailableAppsAsync(webServerRelativeUrl, scope)).Where(app => app.Title == title).ToList();
        }

        public async Task<AveAppMetadata> GetAvailableAppByIdAsync(string webServerRelativeUrl, AppCatalogScope scope, Guid id)
        {
            string webUrl = this.WebAppName + webServerRelativeUrl;
            string catalogScope = (scope == AppCatalogScope.Tenant) ? "tenant" : "sitecollection";
            string requestUri = string.Format("{0}/_api/web/{1}appcatalog/AvailableApps/GetById('{2}')", webUrl, catalogScope, id.ToString());

            try
            {
                string response = await ExecuteAppCatalogRequestAsync(webUrl, requestUri, null, "Failed to get available apps.");
                if (string.IsNullOrEmpty(response))
                {
                    return null;
                }

                JsonSerializerOptions serializerOptions = new JsonSerializerOptions
                {
                    IgnoreNullValues = true
                };
                return JsonSerializer.Deserialize<AveAppMetadata>(response, serializerOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get available apps. Url:{requestUri}, ex:{ex.Message}", ex);
            }
        }

        //
        // Summary:
        //     Installs an available app from the app catalog in a site.
        //
        // Parameters:
        //   id:
        //     The unique id of the app. Notice that this is not the product id as listed in
        //     the app catalog.
        //
        //   scope:
        //     Specifies the app catalog to work with. Defaults to Tenant
        public async Task<bool> InstallAppAsync(string webServerRelativeUrl, Guid id, AppCatalogScope scope = AppCatalogScope.Tenant)
        {
            await default(SynchronizationContextRemover);
            return await AppOperateBaseRequest(this.WebAppName + webServerRelativeUrl, id, AppManagerAction.Install, switchToAppCatalogContext: false, null, scope);
        }

        public async Task<bool> DeployAppAsync(string webServerRelativeUrl, Guid id, bool skipFeatureDeployment = true, AppCatalogScope scope = AppCatalogScope.Tenant)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id");
            }

            Dictionary<string, object> postObj = new Dictionary<string, object> { { "skipFeatureDeployment", skipFeatureDeployment } };
            await default(SynchronizationContextRemover);
            return await AppOperateBaseRequest(this.WebAppName + webServerRelativeUrl, id, AppManagerAction.Deploy, switchToAppCatalogContext: true, postObj, scope);
        }

        private async Task<bool> AppOperateBaseRequest(string webUrl, Guid id, AppManagerAction action, bool switchToAppCatalogContext, Dictionary<string, object> postObject, AppCatalogScope scope, int timeoutSeconds = 200)
        {
            bool returnValue = false;
            string text = action.ToString();
            string requestUri = string.Format("{0}/_api/web/{1}appcatalog/AvailableApps/GetByID('{2}')/{3}", webUrl, (scope == AppCatalogScope.Tenant) ? "tenant" : "sitecollection", id, text);

            try
            {
                string responseText = await ExecuteAppCatalogRequestAsync(webUrl, requestUri, postObject, "Failed to execute app request.");

                if (!string.IsNullOrEmpty(responseText))
                {
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute app request. Url:{requestUri}, ex:{ex.Message}", ex);
            }

            return await Task.Run(() => returnValue);
        }

        private async Task<string> ExecuteAppCatalogRequestAsync(string webUrl, string requestUri, Dictionary<string, object> postObject, string failureMessage)
        {
            ReliableHttpWebRequest webRequest = ReliableHttpWebRequest.CreateRequest(requestUri, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            webRequest.RefreshDigestInfo(requestUri, TokenProvider);
            webRequest.SetTokenProvider(webUrl, TokenProvider);
            webRequest.Method = "POST";
            webRequest.Accept = "application/json;odata=nometadata";

            if (postObject != null)
            {
                string payload = JsonSerializer.Serialize(postObject);
                byte[] buffer = Encoding.UTF8.GetBytes(payload);
                webRequest.ContentType = "application/json;odata=nometadata;charset=utf-8";
                webRequest.ContentLength = buffer.Length;
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            else
            {
                webRequest.ContentType = "application/json;odata=nometadata";
                webRequest.ContentLength = 0L;
            }

            using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
            {
                if (response == null)
                {
                    throw new WebException($"{failureMessage} Url:{requestUri}");
                }

                string responseText;
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream ?? Stream.Null))
                {
                    responseText = await reader.ReadToEndAsync();
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.IsNullOrEmpty(responseText) ? $"{failureMessage} Url:{requestUri}, StatusCode:{response.StatusCode}" : responseText);
                }

                return responseText;
            }
        }

        public void UninstallApp(string webServerRelativeUrl, Guid productId)
        {

            using ClientContext _context = CreateContext();
            Web web = _context.Site.OpenWeb(webServerRelativeUrl);
            ClientObjectList<AppInstance> appInstances = web.GetAppInstancesByProductId(productId);
            _context.Load(appInstances);
            _context.ExecuteQuery();
            foreach (AppInstance appInstance in appInstances)
            {
                appInstance.Uninstall();
            }
            _context.ExecuteQuery();
        }        

        public AveAppStatus GetAppStatus(string webServerRelativeUrl, Guid productId, out ClientObjectList<AppInstance> apps)
        {
            using ClientContext mContext = CreateContext();
            Web mWeb = mContext.Site.OpenWeb(webServerRelativeUrl);
            AveAppStatus status = AveAppStatus.NoExist;
            apps = mWeb.GetAppInstancesByProductId(productId);
            mContext.Load(apps);
            mContext.ExecuteQuery();
            if (apps.Count <= 0)
            {
                return status;
            }
            if (apps.Count == 1)
            {
                status = ConvertAppInstanceStatus(apps.First());
                return status;
            }
            status = AveAppStatus.Uninstalling;//Need to be accurate
            if (apps.All((a) => a.Status == AppInstanceStatus.Installed))
            {
                status = AveAppStatus.Installed;
            }
            return status;
        }

        private AveAppStatus ConvertAppInstanceStatus(AppInstance appInstance)
        {
            AveAppStatus status = AveAppStatus.NoExist;
            if (appInstance == null)
            {
                return status;
            }
            switch (appInstance.Status)
            {
                case AppInstanceStatus.Uninstalling:
                    status = AveAppStatus.Uninstalling;
                    break;
                case AppInstanceStatus.Installing:
                    status = AveAppStatus.Installing;
                    break;
                case AppInstanceStatus.Installed:
                    status = AveAppStatus.Installed;
                    break;
                case AppInstanceStatus.Initialized:
                    status = AveAppStatus.Initialized;
                    break;
                default:
                    status = AveAppStatus.InvalidStatus;
                    break;
            }
            return status;
        }

        private enum AppManagerAction
        {
            Install,
            Retract,
            Remove,
            Deploy,
            Upgrade,
            Uninstall
        }

    }
}
