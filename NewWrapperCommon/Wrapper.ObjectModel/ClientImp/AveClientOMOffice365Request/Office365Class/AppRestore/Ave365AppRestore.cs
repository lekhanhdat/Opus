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
    #region reference.
    using AveClientRequest.Common;
    using AvePoint.Office365.Api;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;
    using System.Web.Script.Serialization;
    #endregion
    internal class Ave365AppRestore : AveAppRestore
    {
        private ITokenProvider mProvider;
        public Ave365AppRestore(ClientContext context, ITokenProvider provider, Site site, Web web, string webfullUrl)
            : base(context, null, site, web, webfullUrl)
        {
            mProvider = provider;
        }
        protected override Dictionary<string, object> GetAppMetadata(Guid productId)
        {
            string getMyAppsUrl = mWebFullUrl + "/_layouts/15/addanapp.aspx?task=GetMyApps&sort=1&query=&myappscatalog=1&ci=1&vd=1";
            string result = AveHttpWebRequestUtility.HttpGet(getMyAppsUrl,null,mProvider);
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            IList<Dictionary<string, object>> appsMetadata = jsSerializer.Deserialize<List<Dictionary<string, object>>>(result);
            Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, productId);
            return appMetadata;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint URL")]
        protected override string TrustApp(Dictionary<string, object> appMetadata, Guid productId)
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
            string appinvResult = AveHttpWebRequestUtility.HttpGet(appInv.ToString(), null, mProvider);

            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(appinvResult);
            EncodeNecessaryFormValue(formValues, new List<string>() { "__EVENTVALIDATION", "__VIEWSTATE" }, true);
            formValues["__EVENTTARGET"] = "ctl00$PlaceHolderMain$BtnAllow";
            AddTrustList(formValues, productId);
            byte[] body = AveHttpWebRequestUtility.GetByte(formValues, null);
            string closeDialogContent = AveHttpWebRequestUtility.HttpReturn(appInv.ToString(), null, "application/x-www-form-urlencoded", body, null, "", mProvider);

            //<script type="text/javascript">window.frameElement.commonModalDialogClose(1, 'i:0i.t|ms.sp.int|f32519e3-4e7f-41ff-8eff-e3e1efbb3e28@3325bbbf-2b71-4815-a615-68cfb39ad96e');</script>
            string closeDialog = "window.frameElement.commonModalDialogClose(";
            int leftBracePos = closeDialogContent.IndexOf(closeDialog, StringComparison.OrdinalIgnoreCase);
            if (leftBracePos != -1)
            {
                string principalId = closeDialogContent.Substring(leftBracePos + closeDialog.Length, closeDialogContent.IndexOf(")", leftBracePos, StringComparison.OrdinalIgnoreCase) - leftBracePos - closeDialog.Length);
                principalId = principalId.Split(',')[1].Trim().Trim('\'');
                return principalId;
            }
            return string.Empty;
        }
        protected override void InstallApp(Dictionary<string, object> appMetadata, string appPrincipalId, Guid productId)
        {
            string addAnAppUrl = mWebFullUrl + "/_layouts/15/addanapp.aspx";
            string yourApps = AveHttpWebRequestUtility.HttpGet(addAnAppUrl, null,mProvider);
            Dictionary<string, object> addappFormValues = AveHttpWebRequestUtility.GetPostFormValues(yourApps);
            addappFormValues["task"] = "AppDownload";
            addappFormValues["appid"] = appMetadata.ContainsKey("ID") ? appMetadata["ID"].ToString() : productId.ToString();
            addappFormValues["oID"] = appPrincipalId;
            addappFormValues["catalog"] = appMetadata["Catalog"];
            EncodeNecessaryFormValue(addappFormValues, new List<string>() { "__EVENTVALIDATION", "__VIEWSTATE" }, true);
            byte[] body1 = AveHttpWebRequestUtility.GetByte(addappFormValues, null);
            string r = AveHttpWebRequestUtility.HttpReturn(addAnAppUrl, null, "application/x-www-form-urlencoded", body1, null, "", mProvider);
        }
    }
}
