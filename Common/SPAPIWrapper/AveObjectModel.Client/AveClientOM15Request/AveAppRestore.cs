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


using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Apps;
using Microsoft.SharePoint.Client;
using Microsoft365.Authentication;
using Newtonsoft.Json;
using PnP.Framework.Enums;
using PnP.Framework.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AvePoint.ObjectModel.ClientOM
{
    internal class AveAppRestore
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveAppRestore));
        private readonly IAveRequest request;
        private readonly string webServerRelativeUrl;
        private AveRestoreOption mRestoreOption;
        private List<AveAppMetadata> avaliableTenantApp;
        private List<AveAppMetadata> avaliableSiteApp;
        private readonly Guid RecordAppProductId = new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116");
        public AveAppRestore(IAveRequest request, string webServerRelativeUrl, List<AveAppMetadata> avaliableTenantApp, List<AveAppMetadata> avaliableSiteApp)
        {
            this.request = request;
            this.webServerRelativeUrl = webServerRelativeUrl;
            this.avaliableSiteApp = avaliableSiteApp;
            this.avaliableTenantApp = avaliableTenantApp;
        }

        private void PrepareRestore(Dictionary<string, object> restoreInfo)
        {
            mRestoreOption = (AveRestoreOption)restoreInfo["RestoreOption"];
        }

        public bool RestoreApp(AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo)
        {
            PrepareRestore(restoreInfo);

            ClientObjectList<AppInstance> apps = null;
            if (request.GetAppStatus(webServerRelativeUrl ,appInfo.ProductId, out apps) == AveAppStatus.Installed)
            {
                if (WrapperConfiguration.WrapperConfigurationForBPOS.OverWriteApp)
                {
                    AssertAppCanBeAdded(appInfo.ProductId, out AveAppMetadata app);
                    UninstallApp(appInfo.ProductId);
                    AddAnApp(app);
                }
                else if (mRestoreOption == AveRestoreOption.Default)
                {
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Add(appInfo.InstanceId);
                }
                return false;
            }
            else
            {
                AssertAppCanBeAdded(appInfo.ProductId, out AveAppMetadata app);
                AddAnApp(app);
                return true;
            }
        }

        

        private void UninstallApp(Guid productId)
        {
            request.UninstallApp(webServerRelativeUrl, productId);
            WaitUtillAppUninstalled(productId);
        }

        private void AssertAppCanBeAdded(Guid productId, out AveAppMetadata app)
        {
            AveAppMetadata siteApp = avaliableSiteApp.FirstOrDefault(app => app.ProductId == productId);
            if(siteApp?.Deployed == true)
            {
                app = siteApp;
                return;
            }
            AveAppMetadata tenantApp = avaliableTenantApp.FirstOrDefault(app => app.ProductId == productId);
            if(tenantApp != null && tenantApp.Deployed)
            {
                app = tenantApp;
            }
            else if(siteApp != null)
            {
                app = siteApp;
            }
            else
            {
                app = null;
                throw new Exception("RM_JM_RestoreFaild_AppUnAvaliable_ErrorMessage");
            }
        }

        private void AddAnApp(AveAppMetadata app)
        {
            if (!app.Deployed && app.Scope == AppCatalogScope.Site)
            {
                request.DeployAppAsync(webServerRelativeUrl, app.Id, false, app.Scope).GetAwaiter().GetResult();
                app.Deployed = true;
            }            
            request.InstallAppAsync(webServerRelativeUrl, app.Id, app.Scope);
            //WaitUtillAppInstalled(productId);
        }
         
        private void WaitUtillAppInstalled(Guid productId)
        {
            WaitUtilAppStateChanges(productId, true);
        }

        private void WaitUtillAppUninstalled(Guid productId)
        {
            WaitUtilAppStateChanges(productId, false);
        }

        private void WaitUtilAppStateChanges(Guid productId, bool isInstall)
        {
            ClientObjectList<AppInstance> apps = null;
            AveAppStatus status = isInstall ? AveAppStatus.Installed : AveAppStatus.NoExist;
            int retryCount = 0;
            var timeOutLimit = 12;
            mLogger.Info($"Install app timeout limit is {timeOutLimit}");
            while (true)
            {
                AveAppStatus result = request.GetAppStatus(webServerRelativeUrl ,productId, out apps);
                //wait initialized status changed
                if (AveAppStatus.Initialized == result && retryCount < 2)
                {
                    System.Threading.Thread.Sleep(5000);
                    retryCount++;
                    continue;
                }
                if (result == status || (result != AveAppStatus.Installing && result != AveAppStatus.Uninstalling))
                {
                    break;
                }
                if (retryCount++ > timeOutLimit) //SAAS-37197 Install app timeout问题 
                {
                    throw new TimeoutException("time out when installing or uninstalling app");
                }
                System.Threading.Thread.Sleep(5000);
            }
        }
    }

    
}
