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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft365.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.SharePoint.RelatedRecords
{
    public class RelatedRecordsAppUtility
    {
        private static RALogger mLogger = RALogger.GetInstance(typeof(RelatedRecordsAppUtility));
        private Site mSite;
        private Web mWeb;
        private ClientContext mContext;
        private ITokenProvider tokenProvider;
        private string mWebFullUrl;
       
        public RelatedRecordsAppUtility(ClientContext context, ITokenProvider tokenProvider, Site site, Web web, string webfullUrl)
        {
            mContext = context;
            mSite = site;
            mWeb = web;
            this.tokenProvider = tokenProvider;
            mWebFullUrl = webfullUrl;
        }
        private AppStatus GetAppStatus(Guid productId, out ClientObjectList<AppInstance> apps)
        {
            apps = mWeb.GetAppInstancesByProductId(productId);
            AppStatus status = AppStatus.InvalidStatus;
            try
            {
                mContext.Load(apps);
                mContext.ExecuteQuery();
            }
            catch (Exception e)
            {
                mLogger.Info("get apps failed ,retry by appcatalog {0}", e.ToString());
                var instances = AppCatalog.GetAppInstances(mContext, mWeb);
                foreach (var instance in instances)
                {
                    if (instance.ProductId == productId)
                    {
                        if (instance.Status == AppInstanceStatus.Installed)
                        {
                            apps = null;
                            return AppStatus.Installed;
                        }
                    }
                }
                return status;
            }
            if (apps.Count <= 0)
            {
                return status;
            }
            if (apps.Count == 1)
            {
                status = ConvertAppInstanceStatus(apps.First());
                return status;
            }
            status = AppStatus.Uninstalling;//Need to be accurate
            if (apps.All((a) => a.Status == AppInstanceStatus.Installed))
            {
                status = AppStatus.Installed;
            }
            return status;
        }
        private AppStatus ConvertAppInstanceStatus(AppInstance appInstance)
        {
            AppStatus status = AppStatus.InvalidStatus;
            if (appInstance == null)
            {
                return status;
            }
            switch (appInstance.Status)
            {
                case AppInstanceStatus.Uninstalling:
                    status = AppStatus.Uninstalling;
                    break;
                case AppInstanceStatus.Installing:
                    status = AppStatus.Installing;
                    break;
                case AppInstanceStatus.Installed:
                    status = AppStatus.Installed;
                    break;
                case AppInstanceStatus.Initialized:
                    status = AppStatus.Initialized;
                    break;
                default:
                    status = AppStatus.InvalidStatus;
                    break;
            }
            return status;
        }
        public void AddAnApp(Guid productId)
        {
            AddTenantIdInWeb();
            Dictionary<string, object> appMetadata = GetAppMetadata(productId);
            if (appMetadata == null)
            {
                throw new Exception("It is failed to add the app, since its Metadata cannot be found.");
            }
            string appPrincipalId = TrustApp(appMetadata);
            InstallApp(appMetadata, appPrincipalId, productId);
            WaitUtillAppInstalled(productId);
        }
        public void AddTenantIdInWeb()
        {
            try
            {
                var allProperties = mWeb.AllProperties;
                allProperties["RelatedId"] = TenantLocalValue.LogonGroupId;
                mWeb.Update();
                mContext.ExecuteQuery();
                mLogger.Info("RelatedId set root web, web url:{0}, exist column: {1}", mWeb.Url, TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                mLogger.Error("set web property error, web url{0},error:{1}", mWebFullUrl, e.ToString());
            }
        }
        public void UninstallApp(Guid productId)
        {
            try
            {
                var appInstances = mWeb.GetAppInstancesByProductId(productId);
                mContext.Load(appInstances);
                mContext.ExecuteQuery();
                foreach (var app in appInstances)
                {
                    app.Uninstall();
                    mContext.ExecuteQuery();
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Uninstall App failed {0}", e.ToString());
            }
        }

        private Dictionary<string, object> GetAppMetadata(Guid productId)
        {
            string getMyAppsUrl = mWebFullUrl + "/_layouts/15/addanapp.aspx?task=GetMyApps&sort=1&query=&myappscatalog=1&ci=1&vd=1";
            string result = AveHttpWebRequestUtility.HttpGet(getMyAppsUrl, tokenProvider);
            IList<Dictionary<string, object>> appsMetadata = SerializerHelper.DeserializeByJsonSerializer<List<Dictionary<string, object>>>(result);
            Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, productId);
            return appMetadata;
        }
        private Dictionary<string, object> GetAppPropertiesById(IList<Dictionary<string, object>> appsMetadata, Guid appId)
        {
            return appsMetadata.FirstOrDefault<Dictionary<string, object>>(
                 (appMetadata) => appMetadata.ContainsKey("ProductId")
                    && new Guid(appMetadata["ProductId"] as string) == appId);
        }
        private string TrustApp(Dictionary<string, object> appMetadata)
        {
            StringBuilder appInv = new StringBuilder(mWebFullUrl);
            appInv.Append("/_layouts/15/appinv.aspx?");
            if (appMetadata.ContainsKey("Catalog"))
            {
                appInv.Append("catalog=").Append(HttpUtility.UrlEncode(appMetadata["Catalog"] as string));
            }
            if (appMetadata.ContainsKey("AssetId"))
            {
                appInv.Append("&appcatalogid=").Append(HttpUtility.UrlEncode(appMetadata["AssetId"] as string));
            }
            else if (appMetadata.ContainsKey("ID"))
            {
                appInv.Append("&appcatalogid=").Append(HttpUtility.UrlEncode(appMetadata["ID"] as string));
            }
            Dictionary<string, object> licenseMetadata = null;
            if (appMetadata.ContainsKey("License"))
            {
                licenseMetadata = appMetadata["License"] as Dictionary<string, object>;
            }
            if (licenseMetadata != null && licenseMetadata.ContainsKey("CountryRegion"))
            {
                appInv.Append("&bm=").Append(HttpUtility.UrlEncode(licenseMetadata["CountryRegion"] as string));
            }
            else
            {
                appInv.Append("&bm=");
            }
            if (licenseMetadata != null && licenseMetadata.ContainsKey("Culture"))
            {
                appInv.Append("&cm=").Append(HttpUtility.UrlEncode(licenseMetadata["Culture"] as string));
            }
            else
            {
                appInv.Append("&cm=");
            }
            appInv.Append("&IsDlg=1");
            string appinvResult = AveHttpWebRequestUtility.HttpGet(appInv.ToString(), tokenProvider);

            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(appinvResult);
            formValues["__EVENTTARGET"] = "ctl00$PlaceHolderMain$BtnAllow";
            byte[] body = AveHttpWebRequestUtility.GetByte(formValues, null);
            string closeDialogContent = AveHttpWebRequestUtility.HttpReturn(appInv.ToString(), tokenProvider, "application/x-www-form-urlencoded", body, null);

            //<script type="text/javascript">window.frameElement.commonModalDialogClose(1, 'i:0i.t|ms.sp.int|f32519e3-4e7f-41ff-8eff-e3e1efbb3e28@3325bbbf-2b71-4815-a615-68cfb39ad96e');</script>
            string closeDialog = "window.frameElement.commonModalDialogClose(";
            int leftBracePos = closeDialogContent.IndexOf(closeDialog);
            if (leftBracePos != -1)
            {
                string principalId = closeDialogContent.Substring(leftBracePos + closeDialog.Length, closeDialogContent.IndexOf(")", leftBracePos) - leftBracePos - closeDialog.Length);
                principalId = principalId.Split(',')[1].Trim().Trim('\'');
                return principalId;
            }
            return string.Empty;
        }
        private void InstallApp(Dictionary<string, object> appMetadata, string appPrincipalId, Guid productId)
        {
            string addAnAppUrl = mWebFullUrl + "/_layouts/15/addanapp.aspx";
            string yourApps = AveHttpWebRequestUtility.HttpGet(addAnAppUrl, tokenProvider);
            Dictionary<string, object> addappFormValues = AveHttpWebRequestUtility.GetPostFormValues(yourApps);
            addappFormValues["task"] = "AppDownload";
            if (appMetadata.ContainsKey("ID"))
            {
                addappFormValues["appid"] = appMetadata["ID"].ToString();
            }
            else
            {
                addappFormValues["appid"] = productId.ToString();
            }
            addappFormValues["oID"] = appPrincipalId;
            addappFormValues["catalog"] = appMetadata["Catalog"];
            byte[] body1 = AveHttpWebRequestUtility.GetByte(addappFormValues, null);
            string r = AveHttpWebRequestUtility.HttpReturn(addAnAppUrl, tokenProvider, "application/x-www-form-urlencoded", body1, null);
        }
        private void WaitUtillAppInstalled(Guid productId)
        {
            WaitUtilAppStateChanges(productId, true);
        }
        private void WaitUtilAppStateChanges(Guid productId, bool isInstall)
        {
            ClientObjectList<AppInstance> apps = null;
            AppStatus status = isInstall ? AppStatus.Installed : AppStatus.InvalidStatus;
            int retryCount = 0;
            while (true)
            {
                AppStatus result = GetAppStatus(productId, out apps);
                //wait initialized status changed
                if (AppStatus.Initialized == result && retryCount < 2)
                {
                    System.Threading.Thread.Sleep(10000);
                    retryCount++;
                    continue;
                }
                if (result == status || (result != AppStatus.Installing && result != AppStatus.Uninstalling))
                {
                    break;
                }
                if (retryCount++ > 90)
                {
                    throw new TimeoutException("time out when installing or uninstalling app");
                }
                System.Threading.Thread.Sleep(20000);
            }
        }
    }
}
