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
using AvePoint.GCommon;
using Microsoft.SharePoint.Client;
using AvePoint.Wrapper.Common;
using System.Web.Script.Serialization;
using System.Web;
using AveClientRequest.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ClientOM
{
    internal class AveAppRestore
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveAppRestore));
        private Site mSite;
        private Web mWeb;
        private ClientContext mContext;
        private object mCredential;
        protected string mWebFullUrl;
        private AveRestoreOption mRestoreOption;
        private readonly Guid RecordAppProductId = new Guid("cac9f789-860b-4cd5-ac4f-346d3f78a2ed");

        public AveAppRestore(ClientContext context, object credential, Site site, Web web, string webfullUrl)
        {
            mContext = context;
            mSite = site;
            mWeb = web;
            mCredential = credential;
            mWebFullUrl = webfullUrl;
        }

        private void PrepareRestore(Dictionary<string, object> restoreInfo)
        {
            mRestoreOption = (AveRestoreOption)restoreInfo["RestoreOption"];
        }

        public void RestoreApp(AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo)
        {
            PrepareRestore(restoreInfo);

            ClientObjectList<AppInstance> apps = null;
            if (GetAppStatus(appInfo.ProductId, out apps) == AppStatus.Installed)
            {
                if (mRestoreOption == AveRestoreOption.OverWrite ||
                    mRestoreOption == AveRestoreOption.Replace)
                {
                    UninstallApp(appInfo.ProductId, apps);
                    AddAnApp(appInfo.ProductId);
                }
                else if (mRestoreOption == AveRestoreOption.Default || mRestoreOption == AveRestoreOption.UpgradeOnly)
                {
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Add(appInfo.InstanceId);
                    return;
                }
            }
            else
            {
                AddAnApp(appInfo.ProductId);
            }
        }

        private AppStatus GetAppStatus(Guid productId, out ClientObjectList<AppInstance> apps)
        {
            AppStatus status = AppStatus.NoExist;
            apps = mWeb.GetAppInstancesByProductId(productId);
            mContext.Load(apps);
            mContext.ExecuteQuery();
            if (apps.Count <= 0)
            {
                return status;
            }
            status = AppStatus.Uninstalling;
            if (apps.All((a) => a.Status == AppInstanceStatus.Installed))
            {
                status = AppStatus.Installed;
            }
            return status;
        }

        private void UninstallApp(Guid productId, ClientObjectList<AppInstance> apps)
        {
            foreach (AppInstance appInstance in apps)
            {
                appInstance.Uninstall();
            }
            mContext.ExecuteQuery();

            WaitUtillAppUninstalled(productId);
        }

        private void AddAnApp(Guid productId)
        {
            Dictionary<string, object> appMetadata = GetAppMetadata(productId);
            if (appMetadata != null)
            {
                string appPrincipalId = TrustApp(appMetadata, productId);
                InstallApp(appMetadata, appPrincipalId, productId);
                WaitUtillAppInstalled(productId);
            }
            else
            {
                throw new Exception("An occurred when getting app catalog");
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Url")]
        protected virtual Dictionary<string, object> GetAppMetadata(Guid productId)
        {
            string getMyAppsUrl = mWebFullUrl + "/_layouts/15/addanapp.aspx?task=GetMyApps&sort=1&query=&myappscatalog=1&ci=1&vd=1";
            string result = AveHttpWebRequestUtility.HttpGet(getMyAppsUrl, mCredential);
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            IList<Dictionary<string, object>> appsMetadata = jsSerializer.Deserialize<List<Dictionary<string, object>>>(result);
            Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, productId);
            return appMetadata;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint URL")]
        protected virtual string TrustApp(Dictionary<string, object> appMetadata, Guid productId)
        {
            #region init data
            string catalogId = appMetadata["Catalog"] as string;
            string appCatalogGuid = appMetadata.ContainsKey("AssetId") ? appMetadata["AssetId"] as string : appMetadata["ID"] as string;
            string bm = string.Empty;
            string cm = string.Empty;
            if (appMetadata.ContainsKey("License"))
            {
                Dictionary<string, object> licenseData = appMetadata["License"] as Dictionary<string, object>;
                if (licenseData != null)
                {
                    if (licenseData.ContainsKey("CountryRegion"))
                    {
                        bm = licenseData["CountryRegion"] as string;
                    }
                    if (licenseData.ContainsKey("Culture"))
                    {
                        cm = licenseData["Culture"] as string;
                    }
                }
            }
            #endregion
            StringBuilder appInv = new StringBuilder(mWebFullUrl);
            appInv.Append("/_layouts/15/appinv.aspx?")
                  .Append("catalog=").Append(HttpUtility.UrlEncode(catalogId))
                  .Append("&appcatalogid=").Append(HttpUtility.UrlEncode(appCatalogGuid))
                  .Append("&bm=").Append(HttpUtility.UrlEncode(bm))
                  .Append("&cm=").Append(HttpUtility.UrlEncode(cm))
                  .Append("&IsDlg=1");
            string appinvResult = AveHttpWebRequestUtility.HttpGet(appInv.ToString(), mCredential);

            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(appinvResult);
            EncodeNecessaryFormValue(formValues, new List<string>() { "__EVENTVALIDATION", "__VIEWSTATE" }, true);
            formValues["__EVENTTARGET"] = "ctl00$PlaceHolderMain$BtnAllow";
            AddTrustList(formValues, productId);
            byte[] body = AveHttpWebRequestUtility.GetByte(formValues, null);
            string closeDialogContent = AveHttpWebRequestUtility.HttpReturn(appInv.ToString(), mCredential, "application/x-www-form-urlencoded", body, null);

            //<script type="text/javascript">window.frameElement.commonModalDialogClose(1, 'i:0i.t|ms.sp.int|f32519e3-4e7f-41ff-8eff-e3e1efbb3e28@3325bbbf-2b71-4815-a615-68cfb39ad96e');</script>
            string closeDialog = "window.frameElement.commonModalDialogClose(";
            int leftBracePos = closeDialogContent.IndexOf(closeDialog,StringComparison.OrdinalIgnoreCase);
            if (leftBracePos != -1)
            {
                string principalId = closeDialogContent.Substring(leftBracePos + closeDialog.Length, closeDialogContent.IndexOf(")", leftBracePos,StringComparison.OrdinalIgnoreCase) - leftBracePos - closeDialog.Length);
                principalId = principalId.Split(',')[1].Trim().Trim('\'');
                return principalId;
            }
            return string.Empty;
        }

        protected void EncodeNecessaryFormValue(Dictionary<string, object> formValues, List<string> encodeValues = null, bool needEncode = false)
        {
            if (!needEncode)
            {
                return;
            }
            if (encodeValues == null)
            {
                EncodeAllValue(formValues);
            }
            else
            {
                EncodeCustomValue(formValues, encodeValues);
            }
        }
        protected void AddTrustList(Dictionary<string, object> formValues, Guid productId)
        {
            //Record app.
            if (productId == RecordAppProductId)
            {
                Guid listId = Guid.Empty;
                mContext.Load(mWeb.Lists, lists => lists.Include(l => l.Id, l => l.BaseTemplate));
                mContext.ExecuteQuery();
                var list = mWeb.Lists.FirstOrDefault(l => l.BaseTemplate == (int)AveListTemplateType.DocumentLibrary);
                if (list != null)
                {
                    listId = list.Id;
                }
                if (formValues.ContainsKey("ctl00$PlaceHolderMain$ctl05$DdlList") && (formValues["ctl00$PlaceHolderMain$ctl05$DdlList"] == null || string.IsNullOrEmpty(formValues["ctl00$PlaceHolderMain$ctl05$DdlList"].ToString())))
                {
                    formValues["ctl00$PlaceHolderMain$ctl05$DdlList"] = listId.ToString();
                }
                else
                {
                    if (formValues.ContainsKey("ctl00$PlaceHolderMain$ctl06$DdlList") && (formValues["ctl00$PlaceHolderMain$ctl06$DdlList"] == null || string.IsNullOrEmpty(formValues["ctl00$PlaceHolderMain$ctl06$DdlList"].ToString())))
                    {
                        mLogger.Info("ctl00$PlaceHolderMain$ctl06$DdlList :: in sp16");
                    }
                    formValues["ctl00$PlaceHolderMain$ctl06$DdlList"] = listId.ToString();
                }
            }
        }

        private void EncodeAllValue(Dictionary<string, object> formValues)
        {
            EncodeCustomValue(formValues, formValues.Keys);
        }

        private void EncodeCustomValue(Dictionary<string, object> formValues, ICollection<string> encodeValues)
        {
            foreach (string tempValue in encodeValues)
            {
                if (formValues.ContainsKey(tempValue))
                {
                    formValues[tempValue] = HttpUtility.UrlEncode(formValues[tempValue].ToString());
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Url and post data")]
        protected virtual void InstallApp(Dictionary<string, object> appMetadata, string appPrincipalId, Guid productId)
        {
            string addAnAppUrl = mWebFullUrl + "/_layouts/15/addanapp.aspx";
            string yourApps = AveHttpWebRequestUtility.HttpGet(addAnAppUrl, mCredential);
            Dictionary<string, object> addappFormValues = AveHttpWebRequestUtility.GetPostFormValues(yourApps);
            addappFormValues["task"] = "AppDownload";
            addappFormValues["appid"] = appMetadata.ContainsKey("ID") ? appMetadata["ID"].ToString() : productId.ToString();
            addappFormValues["oID"] = appPrincipalId;
            addappFormValues["catalog"] = appMetadata["Catalog"];
            EncodeNecessaryFormValue(addappFormValues, new List<string>() { "__EVENTVALIDATION", "__VIEWSTATE" }, true);
            byte[] body1 = AveHttpWebRequestUtility.GetByte(addappFormValues, null);
            string r = AveHttpWebRequestUtility.HttpReturn(addAnAppUrl, mCredential, "application/x-www-form-urlencoded", body1, null);
        }

        protected Dictionary<string, object> GetAppPropertiesById(IList<Dictionary<string, object>> appsMetadata, Guid appId)
        {
            return appsMetadata.FirstOrDefault<Dictionary<string, object>>(
                 (appMetadata) => appMetadata.ContainsKey("ProductId")
                    && new Guid(appMetadata["ProductId"] as string) == appId);
        }

        private void WaitUtillAppInstalled(Guid productId)
        {
            WaitUtilAppStateChanges(productId, true);
        }

        private void WaitUtillAppUninstalled(Guid productId)
        {
            WaitUtilAppStateChanges(productId, false);
        }

        private void WaitUtilAppStateChanges(Guid productId, bool installed)
        {
            ClientObjectList<AppInstance> apps = null;
            AppStatus status = installed ? AppStatus.Installed : AppStatus.NoExist;
            int retryCount = 0;
            while (true)
            {
                if (GetAppStatus(productId, out apps) == status)
                {
                    break;
                }
                if (retryCount++ > WrapperConfiguration.CheckAppInstanceInstalledTime)
                {
                    throw new TimeoutException("time out when installing or uninstalling app");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        internal enum AppStatus
        {
            NoExist = 0,
            Uninstalling = 1,
            Installed = 2
        }
    }
}
